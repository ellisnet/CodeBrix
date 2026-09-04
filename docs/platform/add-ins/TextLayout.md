<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Add-ins](../08-add-ins.md) › TextLayout</sub>

# TextLayout

**TextLayout is shaped, bidirectional text layout as a plain code API: no XAML, no controls, and no application host required.** It shapes text with HarfBuzz, resolves bidirectional runs per UAX #9, itemizes across fallback fonts, then reports the geometry an editor or a renderer needs - measured size, per-line metrics, caret and cluster rectangles, cluster-correct hit-testing, selection rectangles and glyph outlines - and draws into any `SKCanvas`. It is a facade over the very same engine that lays out every `TextBlock` in a CodeBrix.Platform application, so a fix here is a fix there.

| | |
| --- | --- |
| **Package** | [`CodeBrix.Platform.TextLayout.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.TextLayout.ApacheLicenseForever) |
| **Adds** | A code API in one namespace: `TextLayoutEngine`, `TextLayoutResult`, `TextLayoutOptions`, `TextRunDescriptor`, `TextLineInfo`, `TextLineMetrics`, `GlyphOutline`, and the enums `TextAlign`, `TextDirection`, `TextFontWeight`, `TextFontStyle`, `TextFontStretch`. No XAML types |
| **Heads** | Every head the framework has - and no head at all: a document model, a game, an image pipeline, a unit test or a console tool can use it directly |
| **Requires** | Native ICU. Windows and macOS get it from [`CodeBrix.Platform.Unicode.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Unicode.ApacheLicenseForever) and [`CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever), which flow in automatically; on Linux, install the distribution's libicu package. A project with no application head also supplies its own native Skia and HarfBuzz assets |

## Add it to your application

Add the package:

```bash
dotnet add package CodeBrix.Platform.TextLayout.ApacheLicenseForever
```

Which project? Any. The package is a plain code API and does not need to sit in the UI project. Inside an existing CodeBrix.Platform application there is nothing further to do: add the one package reference and use the API from any project - the head's runtime package already supplies the native Skia and HarfBuzz assets, and the two Unicode packages flow in with this one.

There is no XAML namespace, because nothing in the public surface is a XAML type, and no registration call. The C# usings are:

```csharp
using CodeBrix.Platform.UI.TextLayout;   // everything in this package
using SkiaSharp;                         // SKCanvas, SKPaint, SKPoint,
                                         // SKRect, SKSize, SKPath, SKFont,
                                         // SKColor
```

> [!IMPORTANT]
> On Linux the engine loads the system ICU from the dynamic-linker search path - nothing ships for Linux - so the distribution's libicu package must be installed. Without it the first `Layout` call fails with `Failed to load libicuuc.`

## Using it

### The one entry point

`TextLayoutEngine.Layout` has two static overloads: a convenience form for a single style, and the runs form for mixed styling.

```csharp
public static TextLayoutResult Layout(
    IReadOnlyList<TextRunDescriptor> runs,
    TextLayoutOptions? options = null)

public static TextLayoutResult Layout(
    string text,
    string? fontFamily = null,
    float fontSize = 12f,
    TextLayoutOptions? options = null)
```

The runs overload throws `ArgumentNullException` when `runs` is null or contains a null run, and `ArgumentException` when `runs` is empty - to lay out empty text, pass one run whose `Text` is `""`. The string overload throws `ArgumentNullException` when `text` is null. Empty text still has a caret at index 0 and a line height.

The first call in a process initializes the engine; inside an application that has already happened and costs nothing.

### An editor's four questions

Caret, hit-test, selection and draw, on one result. `Layout()` is the expensive call; every query on the result is cheap, so keep the result for as long as the text and style are unchanged.

```csharp
using CodeBrix.Platform.UI.TextLayout;
using SkiaSharp;

using var layout = TextLayoutEngine.Layout("Hello, world", "sans-serif", 24f);

SKRect caret = layout.GetCaretRect(3);                      // before 'l'
int index    = layout.GetIndexAt(new SKPoint(x, y));        // -1 outside
int nearest  = layout.GetNearestIndexAt(new SKPoint(x, y)); // never -1
IReadOnlyList<SKRect> selection = layout.GetSelectionRects(0, 5);

using var textPaint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
using var selPaint  = new SKPaint { Color = new SKColor(0x33, 0x66, 0xcc, 0x59) };

canvas.Save();
canvas.Translate(10, 10);                       // layout origin
foreach (var r in selection) canvas.DrawRect(r, selPaint);
layout.Draw(canvas, SKPoint.Empty, textPaint);
canvas.DrawRect(caret, textPaint);
canvas.Restore();
```

Every index parameter and return value is a **text** index into `TextLayoutResult.Text`, never a glyph index. That distinction matters wherever shaping is not one-to-one: ligatures, combining marks, anything that forms a cluster. `GetSelectionRects` comes back as a list because one logical range can be visually discontiguous, across lines and across bidi boundaries.

### Mixed runs, wrapping and alignment

A run is a span of text with one style. Runs are concatenated to form the layout text, and indices address that concatenation, never an individual run. A run is not a line - line breaks come from the text.

```csharp
public TextRunDescriptor(
    string text,                                       // may be "", not null
    string? fontFamily = null,                         // null = platform default
    float fontSize = 12f,                              // em size; > 0
    TextFontWeight weight = TextFontWeight.Normal,
    TextFontStyle style = TextFontStyle.Normal,
    TextFontStretch stretch = TextFontStretch.Normal,
    TextDirection direction = TextDirection.Auto)
```

`TextRunDescriptor.Create(text, fontFamily, fontSize, bold, italic)` is the shorthand. The `Color` property is set with an object initializer and affects drawing only - never measurement, shaping or hit-testing - so mixing colored and uncolored runs is free, and cheaper than splitting a layout in two.

```csharp
var runs = new[]
{
    new TextRunDescriptor("if ", "monospace", 13f) { Color = new SKColor(0x56, 0x9C, 0xD6) },
    TextRunDescriptor.Create("(ready)", "monospace", 13f, bold: true),
    new TextRunDescriptor(" // launch", "monospace", 13f,
                          TextFontWeight.Normal, TextFontStyle.Italic)
        { Color = new SKColor(0x6A, 0x99, 0x55) },
};
var options = new TextLayoutOptions
{
    MaxWidth = 240f,                 // wrapping ON, and a box to align in
    Alignment = TextAlign.Center,
    MaxLines = 3,
};
using var layout = TextLayoutEngine.Layout(runs, options);

// layout.Text == "if (ready) // launch"; indices address that string
using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true };
layout.Draw(canvas, new SKPoint(20, 20), paint);   // uncolored runs use White
```

The options object is small and every field has a meaningful default:

```csharp
public sealed class TextLayoutOptions
{
    public float? MaxWidth { get; set; }            // null = no wrapping
    public int MaxLines { get; set; }               // 0 = unlimited
    public TextAlign Alignment { get; set; }        // Left
    public float LineHeight { get; set; }           // 0 = from font metrics
    public TextDirection BaseDirection { get; set; }// Auto
}
```

`MaxWidth` is the switch for two things at once: wrapping is on only when it has a value, and with no box to align within, `Alignment` is ignored and every line starts at x = 0. A consumer that models its own line breaks - an editor holding a list of lines - wants null. `MaxLines` of 0, or a negative value clamped to 0, means unlimited. `LineHeight` of 0 takes the height from the font metrics of the tallest run on each line.

### Walking the lines

`TextLineInfo` answers "which line is this index on"; `TextLineMetrics` gives the geometry of a line by its zero-based line number.

```csharp
public readonly record struct TextLineInfo(
    int Start,          // index of the line's first character in Text
    int Length,         // characters on the line, INCLUDING a trailing line break
    int LineIndex,      // zero-based line number
    bool IsFirstLine,
    bool IsLastLine);

public readonly record struct TextLineMetrics(
    int Start,            // index of the line's first character in Text
    int Length,           // characters on the line, incl. trailing line break
    float Top,            // vertical offset of the line's top edge
    float Height,         // line height
    float BaselineOffset);// distance from Top down to the baseline
```

Underlines and strikethroughs sit relative to `Top + BaselineOffset`; selection or highlight backgrounds fill `Top` to `Top + Height`. The package draws no decorations itself - they are the caller's, using exactly these numbers.

```csharp
using var layout = TextLayoutEngine.Layout(longText, "sans-serif", 14f,
    new TextLayoutOptions { MaxWidth = 300f });

using var rule = new SKPaint { Color = SKColors.Gray, StrokeWidth = 1f };
for (var i = 0; i < layout.LineCount; i++)
{
    TextLineMetrics m = layout.GetLineMetrics(i);
    var baseline = m.Top + m.BaselineOffset;
    canvas.DrawLine(0, baseline + 2, layout.Size.Width, baseline + 2, rule);
    string lineText = layout.Text.Substring(m.Start, m.Length);
}

TextLineInfo where = layout.GetLineAt(caretIndex);   // which line is the caret on?
```

Remember that both `Length` values include a trailing line break; trim it before showing the slice.

### Right-to-left and bidi

Base direction comes from `options.BaseDirection`, or - for `Auto` - is detected from the concatenated text per UAX #9. A run whose `Direction` is `Auto` inherits the layout's base direction rather than being resolved on its own.

```csharp
using var rtl = TextLayoutEngine.Layout("שלום", "sans-serif", 20f);
bool isRtl = rtl.IsBaseDirectionRightToLeft;            // true (detected)

var forcedLtr = new TextLayoutOptions { BaseDirection = TextDirection.LeftToRight };
using var mixed = TextLayoutEngine.Layout("abc שלום def", "sans-serif", 20f, forcedLtr);
var rects = mixed.GetSelectionRects(2, 6);   // can be several rectangles on ONE
                                             // line: the RTL stretch is visually
                                             // discontiguous from its LTR neighbours
```

Set `BaseDirection` explicitly when a mixed-direction interface must not flip with its content. Caret rectangles advance leftwards in right-to-left text.

### Outlined text and per-glyph work

`Draw` paints filled glyphs through text blobs - the fast path. For stroked or outlined text, which a filled blob cannot give you, take a path instead. `GetOutlinePath()` returns one path combining every glyph, already positioned in layout coordinates; `GetGlyphOutlines()` returns every positioned glyph separately. You own both, and must dispose them.

```csharp
using var layout = TextLayoutEngine.Layout("OUTLINE", "sans-serif", 72f);

using var fill   = new SKPaint { Color = SKColors.White, IsAntialias = true };
using var stroke = new SKPaint
{
    Color = SKColors.Black, IsAntialias = true,
    Style = SKPaintStyle.Stroke, StrokeWidth = 3f, StrokeJoin = SKStrokeJoin.Round,
};

// One combined, already-positioned path: the cheap way to outline text
using (var path = layout.GetOutlinePath())       // caller owns it
{
    canvas.Save();
    canvas.Translate(40, 40);
    canvas.DrawPath(path, stroke);
    canvas.DrawPath(path, fill);
    canvas.Restore();
}

// Per glyph: each Path sits at the origin; translate by Origin to place it
var glyphs = layout.GetGlyphOutlines();          // caller owns each one
try
{
    foreach (var g in glyphs)
    {
        if (g.Path is null || g.Path.IsEmpty) continue;   // space, color emoji
        canvas.Save();
        canvas.Translate(40 + g.Origin.X, 140 + g.Origin.Y);
        canvas.DrawPath(g.Path, fill);
        canvas.Restore();
        // g.Advance, g.GlyphId and g.Font (engine-owned) are available too
    }
}
finally
{
    foreach (var g in glyphs) g.Dispose();
}
```

The ownership rules are in the type itself:

```csharp
public sealed class GlyphOutline : IDisposable
{
    public ushort GlyphId { get; }   // glyph id within Font
    public SKPath? Path { get; }     // outline at the ORIGIN, not at Origin;
                                     // translate by Origin to place it.
                                     // Empty for glyphs with nothing to draw
                                     // (a space, a bitmap or color emoji)
    public SKPoint Origin { get; }   // baseline-left, layout coordinates
    public float Advance { get; }    // horizontal advance
    public SKFont Font { get; }      // engine-owned - do NOT dispose. Can
                                     // differ glyph to glyph after fallback
    public void Dispose();           // disposes Path
}
```

Cache the `SKPath` from `GetOutlinePath` for text that is drawn repeatedly, and prefer it over `GetGlyphOutlines` unless glyphs really are handled individually.

### Rendering with no application at all

The package draws into any `SKCanvas`: an off-screen surface, a document layer, a bitmap, a window. Nothing here needs a XAML tree.

```csharp
using var layout = TextLayoutEngine.Layout("Headless", "sans-serif", 48f);
var info = new SKImageInfo((int)Math.Ceiling(layout.Size.Width) + 20,
                           (int)Math.Ceiling(layout.Size.Height) + 20);
using var surface = SKSurface.Create(info);
surface.Canvas.Clear(SKColors.White);
using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
layout.Draw(surface.Canvas, new SKPoint(10, 10), paint);
using var image = surface.Snapshot();
using var data = image.Encode(SKEncodedImageFormat.Png, 100);
using var file = File.Create("headless.png");
data.SaveTo(file);
```

A console application that does this needs two extra things in its project file: the property that selects the framework's Skia runtime assemblies for a project with no head to select them, and the native-asset packages for the operating system it runs on.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <CodeBrixRuntimeIdentifier>Skia</CodeBrixRuntimeIdentifier>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.Platform.TextLayout.ApacheLicenseForever" />
    <!-- native assets for the OS you run on; use the .Win32 / .macOS pairs elsewhere -->
    <PackageReference Include="SkiaSharp.NativeAssets.Linux" />
    <PackageReference Include="HarfBuzzSharp.NativeAssets.Linux" />
  </ItemGroup>
</Project>
```

```csharp
// Program.cs
using CodeBrix.Platform.UI.TextLayout;
using SkiaSharp;

using var layout = TextLayoutEngine.Layout("Hello, layout", "sans-serif", 24f);
Console.WriteLine($"{layout.Size.Width:F1} x {layout.Size.Height:F1}, " +
                  $"{layout.LineCount} line(s), caret 5 at {layout.GetCaretRect(5).Left:F1}");
```

Pin the versions your package management requires; the native-asset packages must match the Skia and HarfBuzz versions the package brings in. On Linux, install the distribution's libicu package first.

<details>
<summary>The whole public surface of TextLayoutResult</summary>

```csharp
TextLayoutResult (using var ...)
  Text, Size, LineCount, LineHeight, IsBaseDirectionRightToLeft
  GetCaretRect(int, float = 1f) -> SKRect        0..Length inclusive
  GetRectForIndex(int) -> SKRect                 cluster rect
  GetIndexAt(SKPoint) -> int                     -1 outside
  GetNearestIndexAt(SKPoint) -> int              clamped
  GetLineAt(int) -> TextLineInfo                 (Start, Length, LineIndex,
                                                  IsFirstLine, IsLastLine)
  GetLineMetrics(int line) -> TextLineMetrics    (Start, Length, Top,
                                                  Height, BaselineOffset)
  GetSelectionRects(int start, int length) -> IReadOnlyList<SKRect>
  GetOutlinePath() -> SKPath                     caller disposes
  GetGlyphOutlines() -> IReadOnlyList<GlyphOutline>   caller disposes each
  Draw(SKCanvas, SKPoint origin, SKPaint) / Draw(SKCanvas, SKPaint)

Enums: TextAlign Left/Center/Right | TextDirection Auto/LeftToRight/
       RightToLeft | TextFontWeight Thin..Black (100..900) |
       TextFontStyle Normal/Oblique/Italic | TextFontStretch
       Undefined, UltraCondensed..UltraExpanded (Normal = 5)
```

`Dispose()` is a no-op: the layout holds no unmanaged resources of its own, and the fonts it references belong to the engine's shared cache. `IDisposable` is implemented so callers can adopt `using` and stay correct if that ever changes. Paths and outlines handed out by `GetOutlinePath` and `GetGlyphOutlines` are not covered by it.

</details>

## Per-head notes

- **Linux.** ICU is the system's. Install the distribution's libicu package; nothing ships for Linux, and the failure without it is `Failed to load libicuuc.` on the very first `Layout` call.
- **Windows and macOS.** ICU arrives with the two Unicode packages, which flow in as dependencies of this one.
- **Windows font names.** A host that does not know a bare family name substitutes its default face, and does so without honoring the requested weight. Name an application font by its `ms-appx:///` URI when the exact face matters.
- **No head at all.** A project without an application head - a console tool, a test project - supplies the native Skia and HarfBuzz assets for its own operating system, and sets `CodeBrixRuntimeIdentifier` to `Skia`. Every head sets that property through its own build props.

## Pitfalls

- Indices are text indices into `TextLayoutResult.Text`, the concatenation of all runs - never glyph indices and never per-run offsets. Add the preceding runs' lengths when mapping from a run-local position.
- Wrapping is off unless `MaxWidth` is set: a long single-line string comes back as one line, however wide. Alignment is also ignored without a `MaxWidth`; `Center` and `Right` silently behave as `Left`.
- `Layout(runs)` with an empty list throws. Empty text is one run with `""`.
- `GetCaretRect`, `GetRectForIndex` and `GetLineAt` accept 0 to `Text.Length` inclusive and throw outside that range; `GetLineMetrics` takes a **line** index from 0 to `LineCount - 1`, not a text index.
- `GetIndexAt` returns -1 outside the text. For drag-selection use `GetNearestIndexAt`, which clamps instead.
- Selection always comes back as a list of rectangles. Never assume one.
- `TextLineInfo.Length` and `TextLineMetrics.Length` include a trailing line break. Trim it before showing the slice.
- `GetOutlinePath` and `GetGlyphOutlines` return objects you own - dispose them promptly, because they hold native Skia memory. `GlyphOutline.Font` is engine-owned: do not dispose it.
- `GlyphOutline.Path` is positioned at the origin, not at `Origin` - translate by `Origin` to place it. `GetOutlinePath` is already positioned. Spaces and bitmap or color emoji yield an empty path but still carry an `Advance`.
- A run's `Color` overrides the `Draw` paint's color for that run only, and has no effect on `GetOutlinePath`: a path has no color, so paint it.
- `TextDirection.Auto` on a *run* means "inherit the layout's base direction", not "detect this run".
- `MaxLines` below 0 is treated as 0 - unlimited - not as an error.
- Font names resolve per machine. The tests in this repository use `"sans-serif"` and deliberately never assert what it resolves to; do the same, or ship an application font and name it by URI.
- Font resolution goes through the engine's font cache, and a family that resolves to a font still loading asynchronously is laid out immediately with a fallback face. Layout never blocks on a font load; lay out again if exact metrics matter.
- Headless on Linux without libicu installed fails on the first `Layout` call; headless anywhere without the native Skia and HarfBuzz assets fails on load.
- Fonts are cached per family, size, weight, stretch and style. Many distinct sizes or families mean many cache entries; a handful of styles reused across runs is the cheap shape.
- There is no justification, no letter-spacing or word-spacing switches, no vertical text, no ruby, no text-on-a-path, no IME or preedit handling, no rich-text document model, and no font enumeration or loading API. Families are named and the engine resolves them.

## Related

- [TerminalView](TerminalView.md) - the reference render-pass consumer: it measures its cell from one `"x"` layout and draws every attribute run through `TextLayoutEngine.Layout` plus `Draw`
- [AdvancedTextEdit](AdvancedTextEdit.md) - the editor control, which uses this engine from a XAML control's render pass the same way
- [CodeBrix.Platform.Unicode](../../libraries/CodeBrix.Platform.Unicode.md) - the ICU natives for Windows and macOS that arrive with this package
- **Pinta.Brix** in [CodeBrix.Samples](https://github.com/ellisnet/CodeBrix.Samples/tree/main/Pinta.Brix) - an image editor whose engine library lays out and draws its text tool entirely through this API, from a headless library with no XAML text control involved

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.TextLayout/AGENT-README.txt) |
| Add-in source (`TextLayoutEngine.cs` plus `Models/`) | [src/AddIns/Platform.UI.TextLayout](https://github.com/ellisnet/CodeBrix.Platform/tree/main/src/AddIns/Platform.UI.TextLayout) |
| Tests - the verified hostless consumer, a plain xUnit project with no application head | [src/AddIns/Platform.UI.TextLayout.Tests](https://github.com/ellisnet/CodeBrix.Platform/tree/main/src/AddIns/Platform.UI.TextLayout.Tests) |
| Package | [`CodeBrix.Platform.TextLayout.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.TextLayout.ApacheLicenseForever) |

---

**Where to go next**

- [AdvancedTextEdit](AdvancedTextEdit.md) - a full editor control built on this engine
- [Views and styling](../06-views-and-styling.md) - how the same engine lays out every `TextBlock`
- [All add-ins](../08-add-ins.md) - the whole set at a glance
