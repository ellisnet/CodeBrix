<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Add-ins](../08-add-ins.md) › Graphics2DSK</sub>

# Graphics2DSK

**The immediate-mode 2D drawing add-in: one abstract XAML element, `SKCanvasElement`, that you subclass and draw into with the ordinary SkiaSharp API.** Each time the element is painted, the framework hands your `RenderOverride` method the `SKCanvas` of the frame being composed, already translated and clipped to the element's own rectangle. It is the lightest way to put custom drawing into a page: there is no intermediate bitmap, no per-frame pixel copy and no extra texture - the element draws straight into the same Skia picture the rest of the UI is rendered with.

| | |
| --- | --- |
| **Package** | [`CodeBrix.Platform.Graphics2DSK.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Graphics2DSK.ApacheLicenseForever) |
| **Adds** | `SKCanvasElement` - an abstract `FrameworkElement` you subclass. That is the entire public surface. |
| **Heads** | All six. Every framework head is a Skia head, and the element works on each of them with no native library beyond what the head itself needs. |
| **Requires** | .NET 10 or later, and a running CodeBrix.Platform application. No OS package, no native prerequisite. |

Because the element is an ordinary `FrameworkElement`, it takes part in layout, clipping, opacity and transforms like any other element, and it receives pointer events over its whole area.

## Add it to your application

Reference the package once, in the `.Core` (shared UI) project - never in a head project.

```bash
dotnet add package CodeBrix.Platform.Graphics2DSK.ApacheLicenseForever
```

The smallest `.Core` project file that can host a drawn element:

```xml
<ItemGroup>
  <PackageReference Include="CodeBrix.Platform.ApacheLicenseForever" />
  <PackageReference Include="CodeBrix.Platform.Graphics2DSK.ApacheLicenseForever" />
</ItemGroup>
```

The usings your subclass needs:

```csharp
using CodeBrix.Platform.WinUI.Graphics2DSK;   // SKCanvasElement
using SkiaSharp;                              // SKCanvas, SKPaint, SKColors, ...
using Windows.Foundation;                     // Size (the RenderOverride argument)
```

There is nothing to declare in XAML for the package itself. Because you subclass `SKCanvasElement`, the XAML namespace is your own project's:

```xml
xmlns:local="using:MyApp.Views"
<local:SignalScope x:Name="Scope" Height="160" />
```

No head project changes and no bootstrap changes are needed: the element works as soon as the application's host builder has started a head.

## Using it

### The whole public surface

`SKCanvasElement` has four members, and that is all there is to learn.

```csharp
namespace CodeBrix.Platform.WinUI.Graphics2DSK;

public abstract partial class SKCanvasElement : FrameworkElement
{
    protected SKCanvasElement();
    public static bool IsSupportedOnCurrentPlatform();
    public void Invalidate();
    protected abstract void RenderOverride(SKCanvas canvas, Size area);
}
```

`Invalidate()` marks the element's visual dirty so the compositor repaints it on its next frame; call it after every state change that should become visible. `IsSupportedOnCurrentPlatform()` is true once a CodeBrix.Platform application has started, and the constructor throws `PlatformNotSupportedException` only where it is false.

### Drawing a frame

Inside `RenderOverride`, the canvas origin `(0,0)` is the top-left corner of this element: the framework has already applied the element's position, its transforms and the window's display scale. `area` is the element's arranged size, and anything drawn outside the `(0, 0, area.Width, area.Height)` rectangle is clipped away with an anti-aliased edge. The canvas state is saved before the call and restored after it, so you may `Translate`, `Scale` and `ClipRect` freely without balancing `Save`/`Restore` yourself.

Two properties of the method matter more than any others. It runs on the UI thread as part of frame composition, so it must be fast and allocation-free and must never block or await. And it may run whenever the compositor recomposes the frame for other reasons, so it must always draw the *current* state from your fields and never accumulate onto the canvas.

This live signal scope keeps every `SKPaint` and the `SKPath` in fields, and redraws the whole trace from the current sample buffer each time.

```csharp
using System;
using CodeBrix.Platform.WinUI.Graphics2DSK;
using SkiaSharp;
using Windows.Foundation;

namespace MyApp.Views;

public sealed partial class SignalScope : SKCanvasElement
{
    // Paints are reused across frames; never allocate them in RenderOverride.
    private readonly SKPaint _background = new() { Color = new SKColor(0x10, 0x14, 0x1C) };
    private readonly SKPaint _grid = new()
    {
        Color = new SKColor(0x30, 0x38, 0x48), StrokeWidth = 1,
        Style = SKPaintStyle.Stroke, IsAntialias = false
    };
    private readonly SKPaint _trace = new()
    {
        Color = SKColors.LimeGreen, StrokeWidth = 2,
        Style = SKPaintStyle.Stroke, IsAntialias = true
    };
    private readonly SKPath _path = new();
    private float[] _samples = Array.Empty<float>();   // values in -1..1

    public void SetSamples(float[] samples)
    {
        _samples = samples;
        Invalidate();                 // repaint with the new data
    }

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        var w = (float)area.Width;
        var h = (float)area.Height;

        canvas.DrawRect(0, 0, w, h, _background);
        for (var x = 0f; x < w; x += 20f) canvas.DrawLine(x, 0, x, h, _grid);
        for (var y = 0f; y < h; y += 20f) canvas.DrawLine(0, y, w, y, _grid);

        if (_samples.Length < 2) return;

        _path.Reset();
        var step = w / (_samples.Length - 1);
        for (var i = 0; i < _samples.Length; i++)
        {
            var x = i * step;
            var y = h / 2 - _samples[i] * (h / 2 - 4);
            if (i == 0) _path.MoveTo(x, y); else _path.LineTo(x, y);
        }
        canvas.DrawPath(_path, _trace);
    }
}
```

Notice that the canvas is not cleared for you: the first thing the method does is fill its own background. Without that, whatever is behind the element shows through.

Hosting the subclass is ordinary XAML, in a row that gives it a real size.

```xml
<Page x:Class="MyApp.Views.MainPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:local="using:MyApp.Views">
    <Grid RowSpacing="8" Padding="12">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>
        <TextBlock Text="Signal" FontSize="20" />
        <local:SignalScope x:Name="Scope" Grid.Row="1" MinHeight="120" />
    </Grid>
</Page>
```

### Animating from a timer

For live data, batch the updates and invalidate once per tick rather than once per sample. This page feeds the scope thirty times a second from a dispatcher timer that is started on `Loaded` and stopped on `Unloaded`.

```csharp
using System;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;

namespace MyApp.Views;

public sealed partial class MainPage : Page
{
    private readonly float[] _buffer = new float[256];
    private double _phase;
    private DispatcherQueueTimer _timer;

    public MainPage()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            _timer = DispatcherQueue.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(33);
            _timer.IsRepeating = true;
            _timer.Tick += (_, _) => Advance();
            _timer.Start();
        };
        Unloaded += (_, _) => _timer?.Stop();
    }

    private void Advance()
    {
        _phase += 0.15;
        for (var i = 0; i < _buffer.Length; i++)
            _buffer[i] = (float)Math.Sin(_phase + i * 0.08);
        Scope.SetSamples(_buffer);        // -> Invalidate() -> RenderOverride
    }
}
```

Each tick calls `SetSamples`, which calls `Invalidate()` once. Calling `Invalidate()` from inside `RenderOverride` would produce a continuous animation instead; each call schedules exactly one more frame.

### Pointer input in the same coordinate space

The element works entirely in device-independent pixels - the same units as `ActualWidth`, `ActualHeight` and pointer positions. Draw in DIPs and the output is crisp at any display scale; do not multiply coordinates by a DPI factor. A one-DIP stroke is one DIP wide on screen, which is two physical pixels at 200 percent. If you need the scale - to draw hairlines exactly one physical pixel wide, say - read `canvas.TotalMatrix.ScaleX`.

```csharp
// inside an SKCanvasElement subclass
private readonly SKPaint _dot = new() { Color = SKColors.OrangeRed, IsAntialias = true };
private SKPoint _marker = new(-100, -100);

public MarkerCanvas()
{
    PointerPressed += (s, e) =>
    {
        var p = e.GetCurrentPoint(this).Position;   // DIPs: same space as the drawing
        _marker = new SKPoint((float)p.X, (float)p.Y);
        Invalidate();
    };
}

protected override void RenderOverride(SKCanvas canvas, Size area)
{
    canvas.DrawCircle(_marker, 6, _dot);
}
```

The pointer position goes straight into the drawing with no conversion at all. The add-in provides no gestures, scrolling, zooming or hit-testing of drawn shapes - handle the pointer events yourself, as here.

### Guarding shared XAML

`IsSupportedOnCurrentPlatform()` exists for XAML that is also built for a target where the element is a stub. Check it before constructing the element in that case.

```csharp
if (SKCanvasElement.IsSupportedOnCurrentPlatform())
    Host.Children.Add(new SignalScope());
else
    Host.Children.Add(new TextBlock { Text = "Custom drawing is not available here." });
```

### Choosing among the drawing surfaces

Three add-ins can put Skia or OpenGL output in a page, and they differ in cost and in units.

| Element | Package | What it is |
| --- | --- | --- |
| `SKCanvasElement` | Graphics2DSK | Processor-side Skia drawn directly into the frame; subclass plus `RenderOverride`; DIPs; zero copies. The best default for custom-drawn controls, charts, gauges and editors. |
| `SKXamlCanvas` | [SkiaSharp views](SkiaSharpViews.md) | Processor-side Skia into an offscreen `WriteableBitmap` presented as the element's background; used from XAML through a `PaintSurface` event without subclassing; pixel-unit semantics by default. |
| `SkiaGLCanvasElement`, `GLCanvasElement` | [Graphics3DGL](Graphics3DGL.md) | A GPU-backed `SKSurface`, or raw OpenGL, rendered offscreen and read back. Choose them only when the drawing itself needs the GPU. |

This package does not contain `SKXamlCanvas` or `SKSwapChainPanel`, and it does not depend on the package that does. It provides no GPU rendering, no OpenGL and no `GRContext`. It does not render to an image or a file either: use SkiaSharp directly (`SKSurface`, `SKBitmap`) for offscreen output, or `RenderTargetBitmap` for a XAML snapshot. It is unaffected by `UseDirectSkiaCanvasMode()`, which concerns `SKXamlCanvas` only.

## Per-head notes

There are none. All six heads are Skia heads and the element behaves identically on each, with no native library beyond what the head itself needs.

## Pitfalls

- **Forgetting `Invalidate()`.** Changing a field does nothing visible until you call it. Conversely, `RenderOverride` may run whenever the frame is recomposed, so it must be a pure function of the current state.
- **Multiplying coordinates by a DPI factor.** The canvas already carries the display scale; scaling again makes drawings twice as big on a high-density display.
- **Drawing outside `area` and expecting it to show.** It is clipped.
- **Not drawing a background** and being surprised by transparency: the frame canvas is not cleared for you.
- **Blocking in `RenderOverride`** with file I/O, network calls or an `await` freezes the whole UI. Prepare data elsewhere and only draw in `RenderOverride`.
- **Zero-size element.** With no `Height` or `MinHeight` and no star row, an element may arrange at 0 by 0 and draw nothing. Give it a bounded size.
- **Expecting a GPU.** `SKCanvasElement` is processor-side Skia. For GPU rendering use [Graphics3DGL](Graphics3DGL.md).
- **Allocating per frame.** Keep `SKPaint`, `SKPath`, `SKFont` and `SKImage` objects in fields; creating them per frame causes garbage-collection churn and stalls at animation rates.
- **Re-shaping text every frame** is the usual hidden cost in drawn controls: shape once and keep the `SKTextBlob`.
- **Re-issuing a large static backdrop** every frame. Render it once into an `SKImage` - an `SKSurface` plus `Snapshot()` - and `DrawImage` it instead.
- **Assuming the clip saves work.** It protects correctness, not cost: Skia still processes commands that end up fully clipped. Draw only what lies inside `area`.

## Related

- [SkiaSharp views](SkiaSharpViews.md) - `SKXamlCanvas`, the event-based alternative that does not need a subclass
- [Graphics3DGL](Graphics3DGL.md) - the GPU path, when the drawing itself needs shaders or 3D
- [Svg](Svg.md) and [Lottie](Lottie.md) - both draw onto the canvas element this package supplies, and both take it as a dependency
- [EmulateFrameBufferDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/EmulateFrameBufferDemo) in CodeBrix.Platform - a Skia sketch pane and an OpenGL pane on one page; its pointer-handling and invalidation patterns are the ones an `SKCanvasElement` subclass uses
- [JustBetweenUs](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/JustBetweenUs) in CodeBrix.Platform - uses this add-in alongside the Lottie, Svg and SkiaSharp views add-ins on one page

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.WinUI.Graphics2DSK/AGENT-README.txt) |
| The core framework and the six heads | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/AGENT-README.txt) |
| Map of every README in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/README-INDEX.txt) |
| Samples and tools | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/EXTRAS-README.txt) |
| Package | [`CodeBrix.Platform.Graphics2DSK.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Graphics2DSK.ApacheLicenseForever) |

---

**Where to go next**

- [Graphics3DGL](Graphics3DGL.md) - the next add-in: OpenGL and GPU Skia in a page
- [Graphics, media and vision](../09-graphics-media-and-vision.md) - the chapter that puts the drawing add-ins to work
- [All add-ins](../08-add-ins.md) - the whole set at a glance
