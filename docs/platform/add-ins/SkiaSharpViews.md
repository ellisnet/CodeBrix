<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Add-ins](../08-add-ins.md) › SkiaSharp views</sub>

# SkiaSharp views

**The SkiaSharp XAML view types for CodeBrix.Platform applications: `SKXamlCanvas`, its event args, and the Point, Rect, Size and Color conversion helpers.** `SKXamlCanvas` is a `Canvas` that raises a `PaintSurface` event with a processor-side `SKSurface` and presents the drawing through a `WriteableBitmap`. You declare it in XAML and handle one event - no subclass required.

| | |
| --- | --- |
| **Package** | [`CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever) |
| **Adds** | `SKXamlCanvas` (fully functional), `SKPaintSurfaceEventArgs`, `UWPExtensions` conversion helpers, and `SKSwapChainPanel` with `SKPaintGLSurfaceEventArgs` as a non-functional placeholder |
| **Heads** | All six. A single assembly, no extra native libraries. |
| **Requires** | .NET 10 or later, and a running CodeBrix.Platform application. SkiaSharp and the core framework package arrive automatically. |

The [Lottie](Lottie.md) and [Svg](Svg.md) add-ins depend on this package themselves. Reference it directly only when your own code uses `SKXamlCanvas` or the conversion helpers.

## Add it to your application

Reference the package in the `.Core` (shared UI) project.

```bash
dotnet add package CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever
```

```xml
<ItemGroup>
  <PackageReference Include="CodeBrix.Platform.ApacheLicenseForever" />
  <PackageReference Include="CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever" />
</ItemGroup>
```

The XAML namespace and a canvas declaration:

```xml
xmlns:skia="using:SkiaSharp.Views.Windows"
<skia:SKXamlCanvas x:Name="Canvas" PaintSurface="OnPaintSurface" />
```

The usings for code-behind:

```csharp
using SkiaSharp.Views.Windows;        // SKXamlCanvas, SKPaintSurfaceEventArgs,
                                      // SKSwapChainPanel, SKPaintGLSurfaceEventArgs,
                                      // UWPExtensions
using SkiaSharp;                      // SKCanvas, SKPaint, SKSurface, SKImageInfo, ...
using CodeBrix.Platform.UI.Hosting;   // UseDirectSkiaCanvasMode() (core package,
                                      // head Program.cs only)
```

No registration call and no head project changes are needed. The one optional head-project touch point is described under [Per-head notes](#per-head-notes).

## Using it

### SKXamlCanvas

```csharp
namespace SkiaSharp.Views.Windows;

public partial class SKXamlCanvas : Canvas
{
    public SKXamlCanvas();
    public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
    protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e);
    public new void Invalidate();
    public SKSize CanvasSize { get; }        // size of the last painted surface
    public bool IgnorePixelScaling { get; set; }
    public double Dpi { get; }               // display scale factor (logical DPI / 96)
}
```

`Invalidate()` *is* the paint. On the UI thread it synchronously recreates a BGRA8888 premultiplied pixel buffer when the size has changed, wraps it in an `SKSurface`, resets the canvas state, raises `PaintSurface`, flushes, copies the pixels into a `WriteableBitmap` and sets that bitmap as the element's background. Called from another thread, it marshals itself to the UI thread.

The element invalidates itself on `Loaded`, on `SizeChanged`, on a display-density change and on a `Visibility` change. Nothing else repaints it: after any state change of your own, call `Invalidate()`. While `Visibility` is `Collapsed`, or the element has no positive size, nothing is painted and `CanvasSize` becomes empty.

> [!IMPORTANT]
> The surface is not cleared between paints - the same buffer is reused - so start each handler with `canvas.Clear(...)` unless you intend to accumulate.

Because `SKXamlCanvas` derives from `Canvas`, ordinary XAML children may be placed inside it and are drawn above the painted background.

### Painting on demand

This bar chart repaints only when the data changes.

```xml
<Page x:Class="MyApp.Views.MainPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:skia="using:SkiaSharp.Views.Windows">
    <Grid RowSpacing="8" Padding="12">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>
        <Button Content="Shuffle" Click="OnShuffle" />
        <skia:SKXamlCanvas x:Name="Chart" Grid.Row="1" MinHeight="160"
                           PaintSurface="OnChartPaintSurface" />
    </Grid>
</Page>
```

```csharp
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace MyApp.Views;

public sealed partial class MainPage : Page
{
    private readonly float[] _values = { 0.3f, 0.8f, 0.5f, 0.9f, 0.2f, 0.6f };
    private readonly SKPaint _bar = new() { Color = SKColors.SteelBlue, IsAntialias = true };
    private readonly Random _random = new();

    public MainPage() => InitializeComponent();

    private void OnChartPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var w = e.Info.Width;             // physical pixels (IgnorePixelScaling is false)
        var h = e.Info.Height;

        canvas.Clear(SKColors.White);     // the buffer is reused: always clear
        var slot = w / (float)_values.Length;
        for (var i = 0; i < _values.Length; i++)
        {
            var barHeight = _values[i] * (h - 10);
            canvas.DrawRect(i * slot + slot * 0.15f, h - barHeight,
                            slot * 0.7f, barHeight, _bar);
        }
    }

    private void OnShuffle(object sender, RoutedEventArgs e)
    {
        for (var i = 0; i < _values.Length; i++)
            _values[i] = (float)_random.NextDouble();
        Chart.Invalidate();               // nothing repaints until you ask
    }
}
```

Notice the paint is a field, not a local: allocating `SKPaint`, `SKPath`, `SKFont`, `SKTextBlob` or `SKImage` inside the handler causes garbage-collection churn at animation rates.

### The two pixel-unit modes

`IgnorePixelScaling` decides whether your handler works in physical pixels or in device-independent pixels. This is the single most common source of confusion in a drawn control.

```text
IgnorePixelScaling == false (default)
    e.Info.Width/Height  = PHYSICAL pixels (ActualWidth x Dpi, ...)
    canvas matrix        = identity: you draw in physical pixels
    CanvasSize           = physical pixels
    -> a 100-DIP-wide element on a 200 % display yields a 200-px-wide
       canvas; multiply pointer positions (DIPs) by Dpi before use.

IgnorePixelScaling == true
    e.Info.Width/Height  = DIPs (ActualWidth, ActualHeight, truncated)
    canvas matrix        = pre-scaled by Dpi: draw in DIPs, output stays
                           crisp at the physical resolution
    CanvasSize           = DIPs
    -> pointer positions can be used directly.

e.RawInfo is ALWAYS the physical pixel buffer (its size, and
Bgra8888 / Premul). Setting IgnorePixelScaling calls Invalidate() itself.
```

```csharp
public class SKPaintSurfaceEventArgs : EventArgs
{
    public SKPaintSurfaceEventArgs(SKSurface surface, SKImageInfo info);
    public SKPaintSurfaceEventArgs(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo);
    public SKSurface Surface { get; }     // draw on Surface.Canvas
    public SKImageInfo Info { get; }      // user-visible size (see IgnorePixelScaling)
    public SKImageInfo RawInfo { get; }   // the actual pixel buffer
}
```

Do not dispose `e.Surface` or keep a reference to it after the handler returns: the surface and the event args are cached between paints.

### Drawing in device-independent pixels with pointer input

Setting `IgnorePixelScaling="True"` pre-scales the canvas matrix, so pointer positions go straight into the drawing.

```xml
<skia:SKXamlCanvas x:Name="Sketch" IgnorePixelScaling="True"
                   PaintSurface="OnSketchPaintSurface"
                   PointerPressed="OnSketchPointerPressed"
                   PointerMoved="OnSketchPointerMoved"
                   PointerReleased="OnSketchPointerReleased" />
```

```csharp
private readonly SKPath _stroke = new();
private readonly SKPaint _ink = new()
{
    Color = SKColors.Black, StrokeWidth = 3, Style = SKPaintStyle.Stroke,
    IsAntialias = true, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round
};
private bool _drawing;

private void OnSketchPaintSurface(object sender, SKPaintSurfaceEventArgs e)
{
    var canvas = e.Surface.Canvas;        // already scaled by Dpi: DIP coordinates
    canvas.Clear(SKColors.White);
    canvas.DrawPath(_stroke, _ink);
}

private void OnSketchPointerPressed(object sender, PointerRoutedEventArgs e)
{
    var p = e.GetCurrentPoint(Sketch).Position;   // DIPs - usable as-is
    _stroke.MoveTo((float)p.X, (float)p.Y);
    _drawing = true;
    Sketch.CapturePointer(e.Pointer);
    Sketch.Invalidate();
    e.Handled = true;
}

private void OnSketchPointerMoved(object sender, PointerRoutedEventArgs e)
{
    if (!_drawing) return;
    var p = e.GetCurrentPoint(Sketch).Position;
    _stroke.LineTo((float)p.X, (float)p.Y);
    Sketch.Invalidate();
}

private void OnSketchPointerReleased(object sender, PointerRoutedEventArgs e)
{
    _drawing = false;
    Sketch.ReleasePointerCapture(e.Pointer);
}
```

The whole stroke is redrawn from the accumulated `SKPath` on every paint, which is what keeps the handler a pure function of the current state.

The alternative is to stay in physical pixels and convert the pointer position yourself.

```csharp
private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
{
    var p = e.GetCurrentPoint(Canvas).Position;          // DIPs
    var scale = (float)Canvas.Dpi;                        // e.g. 2.0 at 200 %
    _hit = new SKPoint((float)p.X * scale, (float)p.Y * scale);   // now pixels
    Canvas.Invalidate();
}
// In PaintSurface, e.Info is in pixels, e.RawInfo == e.Info, canvas matrix identity.
```

Do one or the other. Doing both double-scales everything.

### Conversion helpers

`UWPExtensions` converts between the framework's geometry and color types and SkiaSharp's. The class is named `UWPExtensions`.

```csharp
namespace SkiaSharp.Views.Windows;

public static class UWPExtensions
{
    public static SKPoint ToSKPoint(this Windows.Foundation.Point point);
    public static Windows.Foundation.Point ToPoint(this SKPoint point);
    public static SKRect ToSKRect(this Windows.Foundation.Rect rect);
    public static Windows.Foundation.Rect ToRect(this SKRect rect);
    public static SKSize ToSKSize(this Windows.Foundation.Size size);
    public static Windows.Foundation.Size ToSize(this SKSize size);
    public static SKColor ToSKColor(this Windows.UI.Color color);
    public static Windows.UI.Color ToColor(this SKColor color);
}
```

```csharp
using SkiaSharp.Views.Windows;    // brings the extension methods into scope

SKRect r = new Windows.Foundation.Rect(10, 20, 100, 50).ToSKRect();
SKColor accent = ((SolidColorBrush)Resources["AccentBrush"]).Color.ToSKColor();
Windows.Foundation.Point p = new SKPoint(3, 4).ToPoint();
```

There are no bitmap conversion helpers in this package. To show Skia output in an `Image`, paint it in an `SKXamlCanvas`, or read the `SKImage`'s pixels into a `WriteableBitmap`'s `PixelBuffer` yourself.

### SKSwapChainPanel is a placeholder

`SKSwapChainPanel` exists here only so that shared code compiles. It is not GPU-backed and it does not paint.

```csharp
public partial class SKSwapChainPanel : FrameworkElement
{
    public static bool RaiseOnUnsupported { get; set; }   // default: true
    public SKSwapChainPanel();          // throws NotSupportedException when RaiseOnUnsupported
    public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;   // never raised
    protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e);
    public new void Invalidate();       // no-op
    public SKSize CanvasSize { get; }   // throws when RaiseOnUnsupported; else empty
    public GRContext GRContext { get; } // throws when RaiseOnUnsupported; else null
    public double ContentsScale { get; }
    public bool EnableRenderLoop { get; set; }   // setter accepted; has no effect
    public bool DrawInBackground { get; set; }   // [NotImplemented]: getter and setter
                                                 // throw NotImplementedException
}
```

With the default `RaiseOnUnsupported = true` its constructor throws `NotSupportedException` with the message "SKSwapChainPanel is not supported for Skia based platforms". Setting `SKSwapChainPanel.RaiseOnUnsupported = false` *before* constructing one turns it into a silent empty element - it still never paints. For a real GPU-backed `SKSurface`, use `SkiaGLCanvasElement` from the [Graphics3DGL](Graphics3DGL.md) add-in.

`SKPaintGLSurfaceEventArgs` is present as the event-args type of that placeholder, and nothing in this package raises it.

### Choosing among the drawing surfaces

| Element | Package | What it is |
| --- | --- | --- |
| `SKXamlCanvas` | this package | Declared in XAML, event-based, processor-side Skia into a bitmap - one copy per paint by default. Pixel units unless you opt into DIPs. |
| `SKCanvasElement` | [Graphics2DSK](Graphics2DSK.md) | Subclass plus `RenderOverride(SKCanvas, Size)`; processor-side Skia drawn straight into the compositor's frame - no bitmap, no copy, always in DIPs. The pick for new custom-drawn controls. |
| `SkiaGLCanvasElement` | [Graphics3DGL](Graphics3DGL.md) | A GPU-backed `SKSurface` plus `GRContext` on an offscreen OpenGL context, read back per frame. The pick when the drawing itself needs the GPU. |
| `GLCanvasElement` | [Graphics3DGL](Graphics3DGL.md) | Raw OpenGL 3.0 or later. |

This package does not contain SkiaSharp itself - `SKCanvas`, `SKPaint` and the rest come from the SkiaSharp package it depends on - and it does not contain `SKCanvasElement` or the GL elements. It offers no render loop, no vsync and no background-thread painting, and it does not bundle Lottie or SVG rendering.

## Per-head notes

`SKXamlCanvas` behaves the same on every head, and so does the `SKSwapChainPanel` placeholder. A single assembly serves all six, with no extra native libraries.

One optional flag lives in the core package and is chained onto the host builder in each head's `Program.cs`.

```csharp
namespace CodeBrix.Platform.UI.Hosting;   // core package

public static ICodeBrixPlatformHostBuilder UseDirectSkiaCanvasMode(
    this ICodeBrixPlatformHostBuilder builder);
public static class DirectSkiaCanvasMode { public static bool IsEnabled { get; } }
```

```csharp
using CodeBrix.Platform.UI.Hosting;

var host = CodeBrixPlatformHostBuilder.Create()
    .App(() => new App())
    .UseLinuxX11()                 // the head's own .Use...() call
    .UseDirectSkiaCanvasMode()     // EXPERIMENTAL; app-wide; SKXamlCanvas only
    .Build();
host.Run();
```

It makes every `SKXamlCanvas` draw straight into its on-screen `WriteableBitmap` buffer instead of into a staging buffer that is then copied - one fewer full-frame copy per paint. Its position relative to the head's own `.Use...()` call does not matter.

> [!WARNING]
> `UseDirectSkiaCanvasMode()` is experimental. It is application-wide and one-way: it cannot be turned off, there is no per-canvas override, and it may change or be removed. It affects `SKXamlCanvas` only - `SKCanvasElement` never had the copy it removes. Enable it to test performance and stability, and measure the result.

## Pitfalls

- **Constructing `SKSwapChainPanel`.** It throws `NotSupportedException` on every head. Move such a page to `SkiaGLCanvasElement` or `SKXamlCanvas`; setting `RaiseOnUnsupported = false` only silences the exception, it does not make the panel paint.
- **Forgetting `canvas.Clear()`.** The pixel buffer is reused, so old frames remain under the new drawing.
- **Mixing device-independent pixels and physical pixels.** By default `e.Info` is in physical pixels while pointer positions are in DIPs. Either set `IgnorePixelScaling = true` or multiply by `Dpi` - do both and everything is double-scaled.
- **Assuming `e.Info == e.RawInfo`.** With `IgnorePixelScaling` they differ. Use `RawInfo` when you need the buffer size.
- **Expecting automatic repaints.** Apart from load, resize, density and visibility changes, only `Invalidate()` paints.
- **Zero-size element.** With no bounded size the canvas never paints and `CanvasSize` stays empty. Give it a star row or a `Height` or `MinHeight`.
- **Calling `Invalidate()` in a tight loop from a background thread.** Each call is marshaled to the UI thread and paints a full frame; coalesce them.
- **Referencing a second SkiaSharp XAML views package.** The `SkiaSharp.Views.Windows` type names would collide and the build fails with CS0433.
- **Looking for a `WindowsExtensions` class.** The helper class here is `UWPExtensions`.
- **Using `SKPaintGLSurfaceEventArgs` in the hope of GPU access.** Nothing in this package raises it; the GPU path is `SkiaGLPaintSurfaceEventArgs` from [Graphics3DGL](Graphics3DGL.md).
- **Expecting GPU performance.** `SKXamlCanvas` is processor-side Skia; each paint costs a full-buffer copy that scales with element area times density squared. Keep canvases as small as the design allows, and move GPU-bound drawing to `SkiaGLCanvasElement`.
- **Allocating drawing objects inside `PaintSurface`.** Cache them in fields.

## Related

- [Graphics2DSK](Graphics2DSK.md) - `SKCanvasElement`, the zero-copy processor-side element for new custom-drawn controls; that package does not depend on this one
- [Graphics3DGL](Graphics3DGL.md) - the GPU path, and the working stand-in for the `SKSwapChainPanel` placeholder
- [Lottie](Lottie.md) and [Svg](Svg.md) - both depend on this package
- [Pinta.Brix](https://github.com/ellisnet/CodeBrix.Samples/tree/main/Pinta.Brix) in [CodeBrix.Samples](../../samples/README.md) - a zoomable document canvas over a cached composite with dirty rectangles, an overlay timer and a scroll-viewer host driven by the document model
- [KenneyAssetBrowser](https://github.com/ellisnet/CodeBrix.Samples/tree/main/KenneyAssetBrowser) in [CodeBrix.Samples](../../samples/README.md) - a zoomable image and sprite-sheet viewer with region spotlighting and baked animation playback
- [PolyHavenBrowser_viewer_only](https://github.com/ellisnet/CodeBrix.Samples/tree/main/PolyHavenBrowser_viewer_only) in [CodeBrix.Samples](../../samples/README.md) - composites off-screen engine output onto one Skia canvas
- [EmulateFrameBufferDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/EmulateFrameBufferDemo) in CodeBrix.Platform - `MainPage.xaml` declares an `SKXamlCanvas` free-hand sketch pane next to a `GLCanvasElement` pane, and `MainPage.xaml.cs` shows the paint handler, pointer capture and invalidation after each stroke

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/CodeBrix.Platform.SkiaSharp.Views/AGENT-README.txt) |
| The core framework, the six heads and the head bootstrap | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/AGENT-README.txt) |
| What a "not implemented" exception means | [NOT-IMPLEMENTED.md](https://github.com/ellisnet/CodeBrix.Platform/blob/main/NOT-IMPLEMENTED.md) |
| Map of every README in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/README-INDEX.txt) |
| Package | [`CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever) |

The package is MIT. For the provenance and licensing of open source code included in this library, see THIRD-PARTY-NOTICES.txt in the repository.

---

**Where to go next**

- [MediaPlayer](MediaPlayer.md) - the next add-in: audio and video through `MediaPlayerElement`
- [Graphics2DSK](Graphics2DSK.md) - the zero-copy alternative for new drawn controls
- [All add-ins](../08-add-ins.md) - the whole set at a glance
