<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.SkiaSvg</sub>

# CodeBrix.SkiaSvg

**CodeBrix.SkiaSvg loads SVG documents, and Android VectorDrawable XML, and renders them to
SkiaSharp canvases, bitmaps and documents.** On top of plain rendering it provides hit testing, a
retained scene graph with incremental mutation, SMIL animation playback, layer-based native
composition, pointer-event dispatch, an editing and inspection API over the intermediate drawing
model, and export to raster and vector formats. Use it from any .NET 10 application, or from a
CodeBrix.Platform application, wherever an SVG has to become pixels - or has to answer questions
about itself.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.SkiaSvg](https://github.com/ellisnet/CodeBrix.SkiaSvg) |
| **Packages** | [`CodeBrix.SkiaSvg.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.SkiaSvg.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later, and a `SkiaSharp.NativeAssets.*` package referenced by the application itself |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Any platform SkiaSharp supports; native assets are published for Linux, macOS and Windows, covering ARM64 as well as x64 |

## What it does

- Loads SVG from files, streams, strings and `XmlReader`s, and renders to a SkiaSharp `SKPicture` or
  straight onto an `SKCanvas`.
- Loads and renders Android VectorDrawable XML, which is the XML form, not compiled Android binary
  resources.
- Exports to raster - whatever `SkiaSharp.SKEncodedImageFormat` supports (PNG, JPEG, WEBP, BMP,
  GIF and the rest) - and to the vector and document formats SVG, PDF and XPS.
- Hit-tests by point and by rectangle, on SVG elements and on scene nodes, optionally through a
  canvas transform. Results come back in rendering order and honor `pointer-events`, clip paths and
  masks.
- Keeps a retained scene graph: a compiled, queryable representation of the rendered SVG that
  enables efficient partial updates without re-rendering the entire document.
- Plays SVG SMIL animation with an explicit, caller-driven clock.
- Decomposes a document into layers for native composition, so a host composites the layers and
  re-renders only the animated ones instead of re-rendering the whole SVG each frame.
- Dispatches pointer and mouse interaction with tunneling, targeting and bubbling phases,
  hover/press/capture tracking and CSS cursor resolution.
- Resolves typefaces through a provider chain you control, and shapes text with HarfBuzz.
- Lets you inspect and mutate the intermediate drawing model programmatically, then re-render with
  `RebuildFromModel()`.
- Draws a wireframe debug view (`Wireframe`, `WireframePicture`, `ClearWireframePicture()`).
- Skips SVG features at render time through `IgnoreAttributes` / `DrawAttributes`, which is how you
  make cheap thumbnails.
- Injects XML entity values and an extra CSS stylesheet per load through `SvgParameters`.

## When to use it

Reach for CodeBrix.SkiaSvg when an application has to draw SVG: icons and logos at any size, a
floor plan or map the user clicks on, a dashboard whose bars change, an animated illustration, or a
batch job that turns SVG into PNG or PDF. It caches the parsed document, the intermediate model and
the rendered picture on the `SKSvg` instance, so rendering the same document repeatedly is cheap,
and the retained scene graph makes an interactive document cheaper still by recompiling only the
subtrees a change touched.

It is a renderer and an inspector, not an authoring tool. It does not create SVG markup from scratch
or serialize a document object model back to an `.svg` file: the document object model itself comes
from [CodeBrix.SvgParse](CodeBrix.SvgParse.md), which this package depends on and re-exposes, and
that is where DOM-level manipulation belongs. It adds only the render-oriented editing helpers
described below.

Also out of scope: SVG optimization or minification; converting HTML and CSS documents to SVG; 3D
rendering or WebGL-style effects; video or animated-GIF export (it renders individual frames;
encoding them into a movie is your job); browser-identical rendering (it rasterizes with SkiaSharp,
not with a browser engine, and it implements SVG 1.1 plus SMIL, not the whole of SVG 2); PDF/XPS
text extraction or reflow, since `ToPdf` and `ToXps` write the rendering rather than a structured
document; validating an SVG against the specification or reporting parse diagnostics; and font
shaping configuration beyond typeface resolution.

If you need SVG without a native dependency, the managed SVG renderer in
[CodeBrix.Imaging.Drawing](CodeBrix.Imaging.Drawing.md) rasterizes off-screen with no native
library at all.

## Getting started

```bash
dotnet add package CodeBrix.SkiaSvg.MitLicenseForever
dotnet add package SkiaSharp.NativeAssets.Linux
```

The second line is the application's own prerequisite, and it changes per platform: use
[`SkiaSharp.NativeAssets.macOS`](https://www.nuget.org/packages/SkiaSharp.NativeAssets.macOS) or
[`SkiaSharp.NativeAssets.Win32`](https://www.nuget.org/packages/SkiaSharp.NativeAssets.Win32)
instead. Everything else - the SVG document object model, SkiaSharp itself, and HarfBuzz with its
native assets - arrives transitively, with no version pinning needed in the consuming project.

> [!WARNING]
> Without the matching `SkiaSharp.NativeAssets.*` package the project compiles and then fails at run
> time on the first SkiaSharp call with a native-library load error.

A complete console project that turns an SVG file into a PNG:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>disable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.SkiaSvg.MitLicenseForever" />
    <!-- required on Linux; use the macOS/Win32 variant per platform -->
    <PackageReference Include="SkiaSharp.NativeAssets.Linux" />
  </ItemGroup>
</Project>
```

```csharp
using System;
using CodeBrix.SkiaSvg;
using SkiaSharp;

if (args.Length < 2)
{
    Console.Error.WriteLine("usage: MySvgTool <in.svg> <out.png>");
    return 1;
}

try
{
    using var svg = SKSvg.CreateFromFile(args[0]);
    bool ok = svg.Save(args[1], SKColors.Transparent,
                       SKEncodedImageFormat.Png, 100, 1f, 1f);
    Console.WriteLine(ok ? "written" : "nothing to write");
    return ok ? 0 : 2;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"failed: {ex.Message}");
    return 3;
}
```

Notice the `using` on the `SKSvg`, which holds unmanaged SkiaSharp resources, the explicit
background color on `Save`, and the `try`/`catch`: loading bytes that are not SVG throws rather than
returning null.

The smallest complete rendering usage is two lines against a canvas you already have:

```csharp
using CodeBrix.SkiaSvg;
using SkiaSharp;

using var svg = SKSvg.CreateFromFile("logo.svg");

canvas.Clear(SKColors.White);
canvas.DrawPicture(svg.Picture);
```

These are the namespaces the rest of this page uses:

```csharp
using CodeBrix.SkiaSvg;                       // SKSvg + everything core
using CodeBrix.SkiaSvg.ShimSkiaSharp;         // SKPoint/SKRect/SKMatrix/...
using CodeBrix.SkiaSvg.ShimSkiaSharp.Editing; // editing extensions
using CodeBrix.SkiaSvg.Model;                 // ISvgAssetLoader, SvgParameters
using CodeBrix.SkiaSvg.Model.Services;        // SvgService
using CodeBrix.SkiaSvg.Model.Editing;         // SvgDocumentEditingExtensions
using CodeBrix.SkiaSvg.TypefaceProviders;     // font resolution
using CodeBrix.SvgParse;                      // SvgDocument, SvgElement
using SkiaSharp;                              // real Skia types
```

There is no `CodeBrix.SkiaSvg.Interaction` namespace. `Interaction/` is only a source folder:
`SvgInteractionDispatcher`, `SvgInteractionDispatchResult`, `SvgPointerInput`, `SvgPointerEventArgs`,
`SvgPointerDeviceType`, `SvgMouseButton` and `SvgPointerEventRoutePhase` are all declared in the
plain `CodeBrix.SkiaSvg` namespace. The same is true of the scene-graph and animation types: despite
living in `SceneGraph/` and `Animation/` folders, they are in `CodeBrix.SkiaSvg`.

## Key concepts

### SKSvg, the entry point

`SKSvg` is the primary public API for loading and rendering, and it implements `IDisposable` - always
use a `using`. The static factories are the preferred way in: `CreateFromFile`, `CreateFromStream`,
`CreateFromXmlReader`, `CreateFromSvg`, `CreateFromSvgDocument`, and the `CreateFromVectorDrawable`
family over a path, a stream or an `XmlReader`. The instance loaders - `Load`, `LoadVectorDrawable`,
`FromSvg`, `FromVectorDrawable`, `FromSvgDocument` and `ReLoad` - each return the rendered
`SkiaSharp.SKPicture`.

The properties worth knowing are `Settings`, `SourceDocument` (the parsed document object model),
`Picture` (the rendered SkiaSharp picture), `Model` (the intermediate drawing model),
`AssetLoader`, `SkiaModel`, `Parameters`, `IgnoreAttributes` and `Sync`, the instance's own lock
object. `Draw(SKCanvas)` paints, `RebuildFromModel()` re-renders `Picture` from the current `Model`,
and `Clone()` copies the instance. Two events are raised: `OnDraw` after each `Draw(canvas)`, and
`AnimationInvalidated` when an animation frame needs presenting. There is also one static switch,
`SKSvg.CacheOriginalStream`, which retains the stream a document was loaded from so that `ReLoad()`
can re-parse it.

### Two families of SK types

Two kinds of `SKPoint`, `SKRect`, `SKMatrix`, `SKPicture`, `SKPaint` and `SKPath` exist, and mixing
them up is the single most common compile error against this library:

- `CodeBrix.SkiaSvg.ShimSkiaSharp.*` is the intermediate drawing model this library builds and
  inspects.
- `SkiaSharp.*` are the real rendering types.

There is no implicit conversion between them. Hit testing, the scene graph and the editing API speak
the `ShimSkiaSharp` types; `SKSvg.Picture`, `SKSvg.Draw` and the export extension methods speak the
SkiaSharp types. When both are in scope, alias the one you mean:

```csharp
using ShimPoint = CodeBrix.SkiaSvg.ShimSkiaSharp.SKPoint;
```

`SkiaModel` converts one way only, from the model to SkiaSharp; going the other way means building
the model value yourself.

### The intermediate drawing model

The model is a self-contained, inspectable description of what to draw: a picture is a list of
canvas commands and a path is a list of path commands, which `SkiaModel` later replays onto a real
canvas. Canvas commands cover clipping (`ClipPathCanvasCommand`, `ClipRectCanvasCommand`), drawing
(`DrawImageCanvasCommand`, `DrawPathCanvasCommand`, `DrawPictureCanvasCommand`,
`DrawTextCanvasCommand`, `DrawTextBlobCanvasCommand`, `DrawTextOnPathCanvasCommand`) and state
(`SaveCanvasCommand`, `RestoreCanvasCommand`, `SaveLayerCanvasCommand`, `SetMatrixCanvasCommand`).
Path commands are `MoveTo`, `LineTo`, `QuadTo`, `CubicTo`, `ArcTo`, `Close`, and the `AddRect`,
`AddRoundRect`, `AddOval`, `AddCircle` and `AddPoly` shorthands. Alongside them sit the shader,
color-filter, image-filter and path-effect records - the SVG filter-primitive model - plus the
value types, the enumerations, the `IDeepCloneable<out T>` deep-copy contract and the
`ICanvasCommandVisitor` visitor.

### Loading options: SvgParameters and DrawAttributes

`SvgParameters` is a readonly record struct,
`public readonly record struct SvgParameters(Dictionary<string, string> Entities, string Css);`.
Pass it to any load method to inject XML entity values and an extra CSS stylesheet applied to the
document. `DrawAttributes` is a `[Flags]` enum used by `IgnoreAttributes` to skip SVG features
during rendering: `None`, `Display`, `Visibility`, `Opacity`, `Filter`, `ClipPath`, `Mask`,
`RequiredFeatures`, `RequiredExtensions` and `SystemLanguage`.

### Saving and export

`SKSvg.Save` writes to a path or a stream and takes a background color, an encoded image format
(PNG by default), a quality and an x and y scale:

```csharp
bool Save(Stream stream, SkiaSharp.SKColor background,
          SkiaSharp.SKEncodedImageFormat format
              = SkiaSharp.SKEncodedImageFormat.Png,
          int quality = 100, float scaleX = 1f, float scaleY = 1f)
bool Save(string path, SkiaSharp.SKColor background,
          SkiaSharp.SKEncodedImageFormat format
              = SkiaSharp.SKEncodedImageFormat.Png,
          int quality = 100, float scaleX = 1f, float scaleY = 1f)
```

Beside them, `SKPictureExtensions` extends the real `SkiaSharp.SKPicture` - so they apply directly
to `svg.Picture` - with `Draw`, `ToBitmap`, `ToImage`, `ToSvg`, `ToPdf` and `ToXps`, each of the
last three in a path form and a stream form.

### Hit testing

`HitTestElements` takes a point or a rectangle and returns the SVG elements under it in rendering
order; `HitTestTopmostElement` returns only the front-most one. `HitTestSceneNodes` and
`HitTestTopmostSceneNode` do the same against the retained scene graph. Each has a `canvasMatrix`
overload that maps a point in canvas space back into picture space before testing, and
`TryGetPicturePoint` / `TryGetPictureRect` do that conversion on their own. Build the matrix from
the same transform you applied to the canvas:

```csharp
using CodeBrix.SkiaSvg.ShimSkiaSharp;

// the app drew the SVG at 2x, offset by (30, 15)
var canvasMatrix = SKMatrix.CreateScale(2f, 2f)
                           .PostConcat(SKMatrix.CreateTranslation(30f, 15f));
var hit = svg.HitTestTopmostElement(new SKPoint(mouseX, mouseY),
                                    canvasMatrix);
```

Identify what you hit by its CLR type: `SvgElement.ElementName` is `protected internal` in the
document object model and is not reachable from a consumer assembly.

### The retained scene graph

`RetainedSceneGraph` compiles on first read; `HasRetainedSceneGraph` and
`TryEnsureRetainedSceneGraph` let you check or force it without throwing. Nodes are looked up by
address key, by element or by id (`TryGetRetainedSceneNode`, `TryGetRetainedSceneNodes`,
`TryGetRetainedSceneNodeById`), and resources the same way (`TryGetRetainedSceneResource`,
`TryGetRetainedSceneResourceById`). You can render from it at several granularities:
`CreateRetainedSceneGraphModel` and `CreateRetainedSceneGraphPicture` for the whole document,
`CreateRetainedSceneNodeModel` and `CreateRetainedSceneNodePicture` for one node, and
`CreateRetainedSceneModel` and `CreateRetainedScenePicture` for one element, each with an optional
clip.

The point of the scene graph is incremental change: after editing the document object model, call
`ApplyRetainedSceneMutation` - by element, by address key or by id - and only the affected
compilation roots and resources are recompiled. The returned `SvgSceneMutationResult` reports
`Succeeded`, `CompilationRootCount` and `ResourceCount`.

An `SvgSceneNode` is read-only to consumers and carries its `Kind`, the source `Element` and
`HitTestTargetElement`, address and id, `PointerEvents`, visibility, `Cursor`, the clip, mask and
filter resource keys, the compilation-root key and strategy, `Parent`, `Children`, `LocalModel`,
`HitTestPath`, `GeometryBounds` and `TransformedBounds`, and `Transform` / `TotalTransform`.
`SvgSceneDocument` wraps the tree with `Traverse()`, the `TryGet*` family, `MarkDirty`,
`ApplyMutation`, `ClearDirty()`, its own `HitTest` methods and `CreateModel`. The compiler, runtime
and renderer behind all this - `SvgSceneCompiler`, `SvgSceneRuntime` and `SvgSceneRenderer` - are
public static classes, so a host can compile and render a scene on its own schedule.

### Animation

There is no internal timer. `SKSvg` exposes `AnimationController`, `HasAnimations`, `AnimationTime`,
`AnimationMinimumRenderInterval` (negative values clamp to zero), `HasPendingAnimationFrame`,
`LastAnimationDirtyTargetCount` and `UsesAnimationLayerCaching`, and you drive the clock with
`SetAnimationTime`, `AdvanceAnimation`, `ResetAnimation` and `FlushPendingAnimationFrame`:

```csharp
if (svg.HasAnimations)
{
    // render frames at 30fps for 5 seconds
    var frameDuration = TimeSpan.FromMilliseconds(1000.0 / 30);

    for (int frame = 0; frame < 150; frame++)
    {
        svg.SetAnimationTime(frameDuration * frame);
        canvas.Clear(SKColors.White);
        canvas.DrawPicture(svg.Picture);
        // ... present the frame
    }
}
```

Underneath, `SvgAnimationController` owns an `SvgAnimationClock` - the timeline itself, with
`CurrentTime`, `TimeChanged`, `Reset()`, `Seek(TimeSpan)` and `AdvanceBy(TimeSpan)` - and can build
an animated document for a given time. SMIL begin and end conditions react to pointer events, which
you feed with `SKSvg.NotifyPointerEvent(element, eventType)` or
`SvgAnimationController.RecordPointerEvent`, where `SvgPointerEventType` is `Move`, `Press`,
`Release`, `Enter`, `Leave`, `Wheel` or `Click`. `SvgAnimationInvalidation.AffectsDescendantSubtree`
answers whether an attribute change invalidates a whole subtree, which it does for the inheritable
presentation attributes.

A small set of types lets a UI host declare what it can do and be told which playback strategy to
use, while still driving the clock itself: `SvgAnimationHostBackend` (`Default`, `Manual`,
`DispatcherTimer`, `RenderLoop`, `NativeComposition`), `SvgAnimationHostBackendCapabilities`,
`SvgAnimationHostBackendResolution` and `SvgAnimationHostBackendResolver.Resolve`.

### Native composition

When `SupportsNativeComposition` is true, `TryCreateNativeCompositionScene` and
`TryCreateNativeCompositionFrame` hand back a decomposition of the document into layers. A scene and
a frame each expose `SourceBounds` and a list of `SvgNativeCompositionLayer`, and each layer carries
its `DocumentChildIndex`, whether it `IsAnimated`, its `Picture`, `Offset`, `Size`, `Opacity` and
`IsVisible` - enough for a host to composite the static layers once and re-render only the animated
ones.

### Pointer dispatch

`SvgInteractionDispatcher` is the entry point for pointer input. Create one per interactive view and
keep it alive across events, because it holds hover, press and capture state. It exposes
`RaiseSvgElementEvents` (true by default), `HoveredElement`, `PressedElement`, `CapturedElement`,
`CurrentCursor`, a `Dispatched` event, the fire-and-forget `HandlePointer*` methods, the
result-returning `DispatchPointer*` methods and `Reset()`. A dispatch result carries
`TargetElement`, `Cursor` and `Handled`.

You construct an `SvgPointerInput` per event. Its point is in picture coordinates - convert first
with `TryGetPicturePoint` if your canvas is transformed - and it is a `ShimSkiaSharp.SKPoint`. The
event args add the routing view: `EventType`, `Element` for the current phase, `TargetElement`,
`RelatedElement`, `RoutePhase` (`Tunnel`, `Target`, `Bubble`), `Input`, `Cursor`, `PicturePoint`
and a settable `Handled` that stops routing.

### Settings, typefaces and the asset seam

`SKSvgSettings` carries `AlphaType` (default `Unpremul`), `ColorType` (default
`SKImageInfo.PlatformColorType`), the `SrgbLinear` and `Srgb` color spaces, the
`TypefaceProviders` list, `StandaloneViewport`, `EnableSvgFonts` and `EnableTextReferences` (for
`<tref>`). The default provider chain is a system-font provider followed by the SkiaSharp default
typeface.

A provider is one method:

```csharp
public interface ITypefaceProvider
{
    SkiaSharp.SKTypeface FromFamilyName(
        string fontFamily,
        SkiaSharp.SKFontStyleWeight fontWeight,
        SkiaSharp.SKFontStyleWidth fontWidth,
        SkiaSharp.SKFontStyleSlant fontStyle);
}
```

`FontManagerTypefaceProvider` resolves through an `SKFontManager` and can create typefaces from a
stream, a path or `SKData`. `DefaultTypefaceProvider` resolves through the SkiaSharp default
typeface. `CustomTypefaceProvider` serves exactly one font face, loaded up front, and has no
parameterless constructor; it answers `FromFamilyName` only when one of the comma-separated
requested family names equals its `FamilyName` exactly, after trimming quotes, and the requested
weight, width and slant all match the loaded face. Register one provider per face, or set
`FamilyName` to make a face answer to the name the SVG asks for. Insert custom providers at index 0
for highest priority: the chain is consulted in order and the first non-null answer wins.

Below that sits the asset seam - `ISvgAssetLoader`, `ISvgTextReferenceRenderingOptions`,
`ISvgTextRunTypefaceResolver`, `ISvgTextGlyphRunResolver` and `ISvgTextDirectedGlyphRunResolver` -
which a host implements to take over image loading, font metrics and glyph shaping. The built-in
`SkiaSvgAssetLoader` implements all of them on top of SkiaSharp and HarfBuzz, and it caches resolved
typefaces, paints and shaped runs, so reuse one instance rather than creating one per render.

### Opening a document without an SKSvg

`SvgService`, a static class, opens SVG sources into a document object model without constructing an
`SKSvg`, and measures a fragment: `Open` over a path, stream or `XmlReader`, `OpenSvg`, `OpenSvgz`,
`FromSvg`, the `OpenVectorDrawable` family, `FromVectorDrawable`, and `GetDimensions`. `Open(path)`
dispatches on the file extension, so it reads both `.svg` and gzip-compressed `.svgz`.

### Editing the drawing model

This library renders SVGs; it is not an SVG editor. What it does offer is programmatic inspection
and mutation of two things: the parsed document object model, through the
[CodeBrix.SvgParse](CodeBrix.SvgParse.md) types, and the intermediate drawing model. After editing
the drawing model, call `svg.RebuildFromModel()` to re-render.

```csharp
using CodeBrix.SkiaSvg;
using CodeBrix.SkiaSvg.ShimSkiaSharp;
using CodeBrix.SkiaSvg.ShimSkiaSharp.Editing;

using var svg = SKSvg.CreateFromFile("chart.svg");

int changed = svg.Model.UpdatePaints(
    p => p.Color.HasValue && p.Color.Value.Red == 255,
    p => p.Color = new SKColor(0, 128, 0, 255));

if (changed > 0)
{
    svg.RebuildFromModel();
}
```

`SKPictureEditingExtensions` recurses into nested pictures with `FindCommands<TCommand>`,
`ReplaceCommands`, `UpdatePaints` and `UpdatePaths`, the last two taking an `EditMode` of `InPlace`
(the default) or `CloneOnWrite`. `SKPathEditingExtensions` adds `UpdateCommands` and `Transform`,
`SKPaintEditingExtensions` adds `ApplyColorTransform` and `ApplyShaderTransform`, and
`CanvasCommandVisitorExtensions.Accept` pairs a command with an `ICanvasCommandVisitor`. All of them
throw `ArgumentNullException` on a null receiver, predicate or callback. On the document side,
`SvgDocumentEditingExtensions` adds `TraverseElements` and `UpdateStyleAttributes`, which returns
the number of elements updated.

### Rasterizing an SVG whose content does not start at (0,0)

`ToBitmap()` and `Draw()` scale by the cull rectangle's width and height but do not translate by its
left and top, so an SVG whose content bounds do not start at the origin comes out clipped or offset.
Rasterize such a file by hand:

```csharp
using CodeBrix.SkiaSvg;
using SkiaSharp;

using var svg = SKSvg.CreateFromFile("offset-art.svg");
var cull = svg.Picture.CullRect;
const float scale = 2f;

var info = new SKImageInfo(
    (int)Math.Ceiling(cull.Width * scale),
    (int)Math.Ceiling(cull.Height * scale));

using var bitmap = new SKBitmap(info);
using (var canvas = new SKCanvas(bitmap))
{
    canvas.Clear(SKColors.Transparent);
    canvas.Scale(scale);
    canvas.Translate(-cull.Left, -cull.Top);
    canvas.DrawPicture(svg.Picture);
}

using var image = SKImage.FromBitmap(bitmap);
using var data = image.Encode(SKEncodedImageFormat.Png, 100);
using var file = File.OpenWrite("offset-art.png");
data.SaveTo(file);
```

## Examples

One loaded document, exported to raster and to vector and document formats:

```csharp
using CodeBrix.SkiaSvg;
using SkiaSharp;

using var svg = SKSvg.CreateFromFile("diagram.svg");
var bg = SKColors.White;

// raster
svg.Save("output.png", bg, SKEncodedImageFormat.Png, 100, 1f, 1f);
svg.Save("output.jpg", bg, SKEncodedImageFormat.Jpeg, 90, 1f, 1f);

// vector / document
svg.Picture.ToPdf("output.pdf", bg, 1f, 1f);
svg.Picture.ToSvg("output.svg", bg, 1f, 1f);
svg.Picture.ToXps("output.xps", bg, 1f, 1f);
```

Hit testing by point, showing both the `ShimSkiaSharp` import and identification by CLR type:

```csharp
using CodeBrix.SkiaSvg;
using CodeBrix.SkiaSvg.ShimSkiaSharp;   // for SKPoint
using CodeBrix.SvgParse;                // for SvgVisualElement

using var svg = SKSvg.CreateFromFile("map.svg");

foreach (var el in svg.HitTestElements(new SKPoint(150, 75)))
{
    Console.WriteLine($"Element: {el.GetType().Name}");
    if (el is SvgVisualElement visual)
    {
        Console.WriteLine($"  Fill: {visual.Fill}");
    }
}

var top = svg.HitTestTopmostElement(new SKPoint(150, 75));
Console.WriteLine($"Topmost: {top?.GetType().Name}");
```

A dashboard bar that changes: edit the document, then recompile only the affected subtree:

```csharp
using CodeBrix.SkiaSvg;
using CodeBrix.SvgParse;
using SkiaSharp;

using var svg = SKSvg.CreateFromFile("dashboard.svg");

if (svg.TryEnsureRetainedSceneGraph(out var scene))
{
    // modify an element in the DOM
    var bar = svg.SourceDocument.GetElementById<SvgRectangle>("bar1");
    bar.Height = new SvgUnit(SvgUnitType.Pixel, 150);
    bar.Fill = new SvgColorServer(System.Drawing.Color.Green);

    // recompile only the affected subtree
    var mutation = svg.ApplyRetainedSceneMutationById("bar1");
    Console.WriteLine($"recompiled {mutation.CompilationRootCount} roots");

    canvas.DrawPicture(svg.Picture);
}
```

An interactive view: hover updates the cursor, and a press reports which element was clicked:

```csharp
using CodeBrix.SkiaSvg;
using CodeBrix.SkiaSvg.ShimSkiaSharp;

using var svg = SKSvg.CreateFromFile("floorplan.svg");
var dispatcher = new SvgInteractionDispatcher();

static SvgPointerInput Input(float x, float y, SvgMouseButton button,
                             int clicks) =>
    new SvgPointerInput(new SKPoint(x, y), SvgPointerDeviceType.Mouse,
                        button, clicks, 0, false, false, false, "mouse");

// pointer moved -> update the cursor
var moved = dispatcher.DispatchPointerMoved(
    svg, Input(120, 80, SvgMouseButton.None, 0));
string cursor = moved.Cursor;              // e.g. "pointer"

// pointer pressed -> which room was clicked
var pressed = dispatcher.DispatchPointerPressed(
    svg, Input(120, 80, SvgMouseButton.Left, 1));
Console.WriteLine($"clicked: {pressed.TargetElement?.ID}");
```

Registering application fonts explicitly, which is what makes text rendering deterministic in a
container or on a CI agent:

```csharp
using CodeBrix.SkiaSvg;
using CodeBrix.SkiaSvg.TypefaceProviders;
using SkiaSharp;

var svg = new SKSvg();

// one provider per face; highest priority goes first
var regular = new CustomTypefaceProvider("/app/fonts/Brand-Regular.ttf");
var bold = new CustomTypefaceProvider("/app/fonts/Brand-Bold.ttf");
svg.Settings.TypefaceProviders.Insert(0, bold);
svg.Settings.TypefaceProviders.Insert(0, regular);

svg.Load("text-heavy.svg");
canvas.DrawPicture(svg.Picture);

// CustomTypefaceProvider is IDisposable - dispose with the SKSvg
```

## Pitfalls

- Do not mix up the two `SKPoint` / `SKRect` / `SKMatrix` / `SKPicture` / `SKPaint` / `SKPath`
  families. There is no implicit conversion: `new SKPoint(x, y)` under `using SkiaSharp;` will not
  bind to `HitTestElements`. Alias with
  `using ShimPoint = CodeBrix.SkiaSvg.ShimSkiaSharp.SKPoint;`.
- Do not confuse `svg.Model` (the intermediate drawing model) with `svg.Picture` (the rendered
  result). Edit `Model`, then call `RebuildFromModel()` to refresh `Picture`.
- Do not write `using CodeBrix.SkiaSvg.Interaction;` - that namespace does not exist. Folder names
  are not namespaces here.
- Do not confuse the package ID with the namespace. Package: `CodeBrix.SkiaSvg.MitLicenseForever`.
  Namespace: `CodeBrix.SkiaSvg`.
- Do not forget to dispose `SKSvg` instances - they hold unmanaged SkiaSharp resources.
- Do not assume thread safety. SkiaSharp objects are not thread-safe, and neither is an `SKSvg`
  instance. Every method that touches the picture takes the instance's own lock, exposed as the
  public `object Sync { get; }` property, so take `lock (svg.Sync)` around any multi-step sequence
  you perform from another thread.
- Do not call `ApplyRetainedSceneMutation` without a retained scene graph. Reading
  `RetainedSceneGraph` or calling `TryEnsureRetainedSceneGraph` compiles it; if compilation fails,
  both report null or false rather than throwing.
- Do not expect animation to run by itself. Drive the clock with `SetAnimationTime` or
  `AdvanceAnimation`, and call `FlushPendingAnimationFrame` when `HasPendingAnimationFrame` is true.
- Do not expect the `SvgPointerEventType` members to be named `PointerDown` / `PointerUp`. They are
  `Move`, `Press`, `Release`, `Enter`, `Leave`, `Wheel` and `Click`.
- Do not call `new CustomTypefaceProvider()` - there is no parameterless constructor, and one
  instance serves exactly one face, matching only on an exact family name plus exact weight, width
  and slant.
- Do not assume system fonts exist in container or CI images. Register fonts with
  `CustomTypefaceProvider`.
- Do not forget that `ReLoad(parameters)` has no default argument - pass null explicitly.
- Do not forget that VectorDrawable support is for Android vector drawable XML, not compiled Android
  binary resources.
- Do not forget the background-color argument on `Save()` and the export extension methods. Use
  `SKColors.Transparent` for transparency, on PNG and other alpha-capable formats only; JPEG with a
  transparent background produces a black background.
- `ToBitmap()` / `Draw()` do not translate by the cull rectangle's origin, and `ToBitmap` truncates
  rather than rounds when scaling. Rasterize offset artwork by hand, as shown above.
- `CreateFromStream()` / `Load()` on bytes that are not SVG throws - `System.Xml.XmlException`, for
  instance - rather than returning null. Wrap the load in a `try`/`catch` when the input is
  untrusted, such as user files or zip entries.
- `SvgElement.ElementName` is not reachable from a consumer assembly; identify elements by CLR type.

> [!TIP]
> Reuse `SKSvg` instances when rendering the same SVG repeatedly - the parsed document, the
> intermediate model and the rendered picture are all cached on the instance. Use the retained scene
> graph for interactive or frequently updated documents, turn features off with `IgnoreAttributes`
> (`DrawAttributes.Filter | DrawAttributes.Mask`, say) when rendering thumbnails at speed, and reuse
> one `SkiaSvgAssetLoader` and `SkiaModel` pair when calling the static `SKSvg.ToPicture` and
> `SKSvg.Draw` helpers in a loop.

## Documentation and source

The repository ships no sample applications, demos or command-line tools: exactly one project is
packable, and everything else exists to test or document it. For runnable, compilable usage, read
the test project - the AGENT-README maps each feature area to the test file that exercises it, from
`SKSvgTests.cs` and `HitTestTests.cs` to `SvgRetainedSceneGraphTests.cs`,
`SvgAnimationControllerTests.cs`, `SvgInteractionDispatcherTests.cs` and the rendering-conformance
suites.

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.SkiaSvg.Tests](https://github.com/ellisnet/CodeBrix.SkiaSvg/tree/main/tests/CodeBrix.SkiaSvg.Tests) |
| Library source | [src/CodeBrix.SkiaSvg](https://github.com/ellisnet/CodeBrix.SkiaSvg/tree/main/src/CodeBrix.SkiaSvg) |

Run the suite from the repository root:

```bash
dotnet test CodeBrix.SkiaSvg.slnx
```

## License

CodeBrix.SkiaSvg is licensed under the MIT License; the license is also named in the package ID
(`CodeBrix.SkiaSvg.MitLicenseForever`). The SVG document object model it depends on and re-exposes
arrives from `CodeBrix.SvgParse.MsplLicenseForever`, which is licensed under the Microsoft Public
License. For the provenance and licensing of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.SvgParse](CodeBrix.SvgParse.md) - the SVG document object model this package parses
  with, and where DOM-level editing belongs
- [CodeBrix.Imaging.Drawing](CodeBrix.Imaging.Drawing.md) - a fully managed SVG rasterizer when a
  native dependency is unwelcome
- [Svg add-in](../platform/add-ins/Svg.md) - SVG in a CodeBrix.Platform application
- [ellisnet/CodeBrix.SkiaSvg on GitHub](https://github.com/ellisnet/CodeBrix.SkiaSvg) - source and tests
