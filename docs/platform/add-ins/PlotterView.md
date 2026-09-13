<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Add-ins](../08-add-ins.md) › PlotterView</sub>

# PlotterView

**PlotterView adds one XAML element, `PlotterControl`, that hosts a CodeBrix.Plotter `PlotModel` and draws it on a Skia surface.** Series, linear / logarithmic / date-time / category / polar axes, annotations and legends arrive with a complete interaction model already wired: pan, zoom, a data-point tracker, reset, and single-finger pan with two-finger pinch on touch. Every one of those gestures can be rebound or removed, and the control behaves the same way on all six heads.

| | |
| --- | --- |
| **Package** | [`CodeBrix.Platform.PlotterView.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.PlotterView.ApacheLicenseForever) |
| **Adds** | `PlotterControl` - a XAML `Control` that implements the CodeBrix.Plotter `IPlotView` contract |
| **Heads** | Every head the framework has: Windows (Win32 and Skia-on-WPF), Linux (X11, Wayland, frame buffer) and macOS |
| **Requires** | Nothing beyond the core framework. [`CodeBrix.Plotter.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Plotter.MitLicenseForever) flows in as a dependency |

## Add it to your application

Add the package:

```bash
dotnet add package CodeBrix.Platform.PlotterView.ApacheLicenseForever
```

Reference it from the shared UI project of your application - the project that already references [`CodeBrix.Platform.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.ApacheLicenseForever) - not from the per-platform head projects. Three things flow in with it: the framework itself, `CodeBrix.Plotter.MitLicenseForever` (the plotting engine - `PlotModel`, series, axes, controller, exporters), and the Skia packages the framework already carries. `CodeBrix.Platform.TextLayout.ApacheLicenseForever` is deliberately *not* a dependency: the plot engine shapes its own text.

Declare the XAML namespace. Either form works:

```xml
xmlns:plot="using:CodeBrix.Platform.UI.PlotterView"
xmlns:plot="clr-namespace:CodeBrix.Platform.UI.PlotterView;assembly=CodeBrix.Platform.UI.PlotterView"
```

In code-behind or a view model, the control comes from one namespace and everything model-side from the plotting engine:

```csharp
using CodeBrix.Platform.UI.PlotterView;   // PlotterControl
using CodeBrix.Plotter;                   // PlotModel, PlotController,
                                          // IPlotController, PlotCommands,
                                          // PlotterColor, PlotterColors,
                                          // PlotterRect, TrackerHitResult,
                                          // CursorType, PlotterMouseButton,
                                          // PlotterKey, DataPoint
using CodeBrix.Plotter.Series;            // LineSeries, ScatterSeries, ...
using CodeBrix.Plotter.Axes;              // LinearAxis, CategoryAxis, ...
using CodeBrix.Plotter.Legends;           // Legend
using CodeBrix.Plotter.Skia;              // PngExporter, SvgExporter, ...
```

There is no registration call and no feature flag. No head project changes are needed; the package works on all six heads.

> [!NOTE]
> `PlotterColor` and `PlotterColors` live in the root `CodeBrix.Plotter` namespace, as do `PlotterRect`, `PlotterSize`, `ScreenPoint` and the input enumerations. The helper namespaces `CodeBrix.Platform.UI.PlotterView.Input` and `CodeBrix.Platform.UI.PlotterView.Rendering` exist, but an application never needs to import them.

## Using it

### Declare the control

Give the control a bounded cell. The client area follows the control size, so a `Grid` star row is the right host and a `StackPanel` is not.

```xml
<Page ...
      xmlns:plot="using:CodeBrix.Platform.UI.PlotterView">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>
        <Button Grid.Row="0" Content="Reset view" Click="ResetView_Click" />
        <plot:PlotterControl x:Name="Plotter" Grid.Row="1" Margin="8" />
    </Grid>
</Page>
```

The control is `IsTabStop`, so the keyboard bindings can take focus; a pointer press on the plot focuses it.

### Build a model and stream data into it

`Model` is a dependency property. Setting it detaches the previous model, attaches the new one to this view, clears any tracker or zoom rectangle, and schedules a full update. This page builds axes and a line series, then pushes samples in from a timer.

```csharp
using CodeBrix.Plotter;
using CodeBrix.Plotter.Axes;
using CodeBrix.Plotter.Series;
using Microsoft.UI.Xaml;

private readonly PlotModel _model = new() { Title = "Live Signal" };
private readonly LineSeries _channel = new() { Title = "Channel A" };
private readonly DispatcherTimer _timer = new()
    { Interval = TimeSpan.FromMilliseconds(35) };
private double _t;

public MainPage()
{
    InitializeComponent();

    _model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Seconds" });
    _model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Volts" });
    _model.Series.Add(_channel);

    Plotter.Model = _model;                  // attaches the model to this view

    _timer.Tick += (_, _) =>
    {
        _t += 0.035;
        _channel.Points.Add(new DataPoint(_t, Math.Sin(2 * Math.PI * _t)));
        _channel.Points.RemoveAll(p => p.X < _t - 10);   // keep a 10 s window
        _model.InvalidatePlot(true);         // data changed: re-read and repaint
    };

    Loaded += (_, _) => { _timer.Start(); Plotter.Focus(FocusState.Programmatic); };
    Unloaded += (_, _) => _timer.Stop();
}

private void ResetView_Click(object sender, RoutedEventArgs e)
{
    // The same effect as the controller's reset bindings (double-middle-click, A, Home)
    Plotter.ActualModel?.ResetAllAxes();
    Plotter.InvalidatePlot(false);           // view changed, data did not
    Plotter.Focus(FocusState.Programmatic);
}
```

Notice the two forms of invalidation: `InvalidatePlot(true)` re-reads the series data, `InvalidatePlot(false)` repaints a view change only. Note also that the streaming series is trimmed rather than accumulated - the paint pass renders the whole model every time.

### Update from a background thread

`InvalidatePlot` is safe from any thread, and it is the method `PlotModel.InvalidatePlot` reaches, so calling either is equivalent. Requests coalesce: any number of calls between two paints produce one update and one render. Mutate the model under its `SyncRoot`, because the control renders under that same lock.

```csharp
Task.Run(() =>
{
    while (running)
    {
        var sample = ReadSensor();
        lock (_model.SyncRoot)
        {
            _channel.Points.Add(sample);
        }
        _model.InvalidatePlot(true);         // thread-safe; coalesces
    }
});
```

Batch the data mutation - add a block of points under one lock, then invalidate once - because each lock hand-off competes with the paint.

### Bind the model from a view model

Because `Model` is a dependency property, a view model can own the chart and the page can stay empty.

```xml
<plot:PlotterControl Model="{x:Bind ViewModel.Chart, Mode=OneWay}" />
```

A `PlotModel` attaches to one view at a time, so a set of prepared models can be swapped freely through the binding.

### Rebind the interaction model

The stock bindings are right-drag to pan, wheel or `+` / `-` to zoom, middle-drag for a zoom rectangle, left-click for the data-point tracker (Ctrl+left for free tracking), double-middle-click or `A` / `Home` to reset, arrow keys to pan, and single-finger pan with two-finger pinch on touch. `Controller` is read at every input event, so it can be swapped at any time; leaving it null means a standard `PlotController` with the stock bindings. Commands are static properties of `PlotCommands` - `PanAt`, `PanLeft`/`Right`/`Up`/`Down` (with `...Fine` variants), `ZoomRectangle`, `ZoomWheel`, `ZoomWheelFine`, `ZoomIn`/`ZoomOut` (with `...At` and `...Fine` variants), `Track`, `SnapTrack`, `PointsOnlyTrack`, `HoverTrack`, `HoverSnapTrack`, `HoverPointsOnlyTrack`, `PanZoomByTouch`, `SnapTrackTouch`, `PointsOnlyTrackTouch`, `Reset`, `ResetAt` and `CopyCode`.

```csharp
// pan with the LEFT button and leave everything else stock
var controller = new PlotController();
controller.UnbindMouseDown(PlotterMouseButton.Left, PlotterModifierKeys.None, 1);
controller.BindMouseDown(PlotterMouseButton.Left, PlotCommands.PanAt);
Plotter.Controller = controller;

// A read-only chart (no interaction at all):
var frozen = new PlotController();
frozen.UnbindAll();
Plotter.Controller = frozen;

// Keyboard-only reset on Home, keeping wheel zoom:
var c = new PlotController();
c.UnbindAll();
c.BindKeyDown(PlotterKey.Home, PlotCommands.Reset);
c.BindMouseWheel(PlotCommands.ZoomWheel);
Plotter.Controller = c;
```

All input goes through the controller and `PlotCommands`; the control raises no input events of its own. `e.Handled` follows the controller's `args.Handled`, so an unbound gesture bubbles to the parent element.

### Style the tracker and the zoom rectangle

Five appearance properties repaint immediately when set: `TrackerBackground` (default `PlotterColor.FromArgb(0xE6, 0x2D, 0x2D, 0x30)`), `TrackerForeground` (default `PlotterColors.White`), `TrackerFontSize` (default 12 DIPs, values below 4 clamp to 4), `ZoomRectangleFill` (default `PlotterColor.FromArgb(0x40, 0xFF, 0xFF, 0x00)`) and `ZoomRectangleStroke` (default `PlotterColors.Black`). Set them once at construction, not per frame.

The tracker text comes from `TrackerHitResult.Text`. The box is centered above the tracked point with a small gap, flips below when there is no room above, and is clamped into the client area.

### Choose the plot font

Every piece of chart text renders through the application's fonts, never the host system's. Text is shaped by the plot engine itself, which is why this package has no TextLayout dependency. A model font family that is an application font URI loads that font; any bare family name - including the model default `"Segoe UI"` - becomes the control's plot font: `PlotFontFamily` if set, otherwise the application's default font.

```csharp
Plotter.PlotFontFamily = "ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/Roboto.ttf";
```

Weight is honored, so bold titles resolve the way bold XAML text does. A font still loading paints with an interim face and swaps on arrival, exactly as `TextBlock` does; the swap re-resolves typefaces and repaints layout only. Changing `PlotFontFamily` resets the typeface cache, so set it at construction time rather than in response to input.

### Export the same model

The control is for the screen and does not export. Hand the same `PlotModel` to one of the exporters in `CodeBrix.Plotter.Skia` - `PngExporter`, `JpegExporter`, `PdfExporter` or `SvgExporter`.

```csharp
using CodeBrix.Plotter.Skia;
var exporter = new PngExporter { Width = 800, Height = 480, Dpi = 96 };
using var stream = File.Create("plot.png");
exporter.Export(model, stream);
```

The application-font routing above belongs to the control's render context; an exporter builds its own.

### The smallest project that draws a chart

Two package references in the shared UI project, one element, three lines of code-behind.

```xml
<ItemGroup>
  <PackageReference Include="CodeBrix.Platform.ApacheLicenseForever" />
  <PackageReference Include="CodeBrix.Platform.PlotterView.ApacheLicenseForever" />
</ItemGroup>
```

```xml
<Page x:Class="MyApp.Views.MainPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:plot="using:CodeBrix.Platform.UI.PlotterView">
    <Grid>
        <plot:PlotterControl x:Name="Plotter" />
    </Grid>
</Page>
```

```csharp
// MainPage.xaml.cs
using CodeBrix.Plotter;
using CodeBrix.Plotter.Series;

public MainPage()
{
    InitializeComponent();
    var model = new PlotModel { Title = "Hello" };
    model.Series.Add(new FunctionSeries(Math.Sin, -10, 10, 0.05, "sin(x)"));
    Plotter.Model = model;
}
```

<details>
<summary>The whole application-facing surface of PlotterControl</summary>

```csharp
PlotterControl : Control, IPlotView
  Model (DP)               PlotModel?         one view per model
  Controller               IPlotController?   null = stock PlotController
  PlotFontFamily           string?            ms-appx:/// URI or null
  TrackerBackground        PlotterColor       0xE6 2D 2D 30
  TrackerForeground        PlotterColor       White
  TrackerFontSize          double             12 (min 4)
  ZoomRectangleFill        PlotterColor       0x40 FF FF 00
  ZoomRectangleStroke      PlotterColor       Black
  ActualModel              PlotModel?         == Model
  ActualController         IController        Controller ?? default
  ClientArea               PlotterRect        (0,0,w,h) DIPs
  InvalidatePlot(bool updateData = true)      any thread; coalesces
  ShowTracker(TrackerHitResult) / HideTracker()
  ShowZoomRectangle(PlotterRect) / HideZoomRectangle()
  SetCursorType(CursorType)
  SetClipboardText(string)
```

`SetCursorType` maps the plot cursor onto the framework cursor: Pan to SizeAll, ZoomRectangle to Cross, ZoomHorizontal to SizeWestEast, ZoomVertical to SizeNorthSouth, and Default to no override. `SetClipboardText` places text on the framework clipboard and flushes it, so the copy outlives the application; a transiently unavailable clipboard is swallowed and the copy does not take.

</details>

## Per-head notes

There is no head-specific setup and no head project changes: the control works on all six heads the same way. On heads with touch, the first finger down starts a gesture, one finger pans, the first two fingers pinch, and a canceled touch or a lost capture completes the gesture cleanly rather than leaving a phantom contact.

## Pitfalls

- A `PlotModel` attaches to one view at a time. Assigning a model that is still attached to another `PlotterControl` throws; set that control's `Model` to null first.
- Mutate a model only under its `SyncRoot`, or from one thread followed by `InvalidatePlot`. Mutating while the control renders - it renders under `SyncRoot` - corrupts the render pass.
- Give the control a bounded size: a `Grid` star cell, not a `StackPanel` or an auto-sized cell. The client area follows the control size.
- The keyboard bindings need focus. Clicking the plot focuses it; from code call `Focus(FocusState.Programmatic)` - there is no `GrabFocus` helper on this control.
- `"Segoe UI"`, the model default, is not missing. Every bare font name resolves to the application font by design; only an `ms-appx:///` URI selects a different application font, and there is no system-font path at all.
- `PlotterColor` is a struct with sentinel values. Test with `IsUndefined()` / `IsAutomatic()` rather than comparing against null.
- `HorizontalAlignment` and `VerticalAlignment` clash between `CodeBrix.Plotter` and the XAML namespaces. Alias one side: `using PlotterHorizontalAlignment = CodeBrix.Plotter.HorizontalAlignment;`
- `Controller` is typed `IPlotController`, not `PlotController`. To rebind, create a `PlotController`, adjust it, then assign; reading `Controller` back when it was never set returns null, not the default - read `ActualController` instead.
- Exporting is not the control's job. Use the `CodeBrix.Plotter.Skia` exporters on the model.
- `InvalidatePlot` coalesces, so do not throttle it yourself - but do batch the data mutation, because each lock hand-off competes with the paint.
- The paint pass renders the whole model every time; there is no partial redraw. Keep streaming series trimmed rather than accumulating history you no longer show.
- The types under `CodeBrix.Platform.UI.PlotterView.Input` and `...Rendering` are public so they can be unit-tested in isolation. They are not extension points: the control calls its own instances directly and never consults a replacement.

## Related

- [CodeBrix.Plotter](../../libraries/CodeBrix.Plotter.md) - the plotting engine this control hosts, and the reference for everything model-side: series, axes, annotations, legends, palettes, the controller and command model, and the exporters
- [PlotterViewDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/PlotterViewDemo) - a chart gallery driven by a drop-down: live streaming signal, function series, bar chart, scatter, heat map through a color axis, and pie, plus a Reset view button and a hint line documenting the interaction model. No hardware required
- [PicoScope.Brix](https://github.com/ellisnet/CodeBrix.Samples/tree/main/PicoScope.Brix) in [CodeBrix.Samples](../../samples/README.md) - the reference application: a live oscilloscope trace streamed into this control from a driver callback on its own thread, with a simulated device when no instrument is attached
- [TextLayout](TextLayout.md) - the text engine this add-in deliberately does not use, and the one every `TextBlock` does

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.PlotterView/AGENT-README.txt) |
| Add-in source (`PlotterControl.cs`, plus `Input/` and `Rendering/`) | [src/AddIns/Platform.UI.PlotterView](https://github.com/ellisnet/CodeBrix.Platform/tree/main/src/AddIns/Platform.UI.PlotterView) |
| Sample application | [samples/CodeBrixPlatform/PlotterViewDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/PlotterViewDemo) |
| Plotting engine API guide | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Plotter/blob/main/AGENT-README.txt) |
| Package | [`CodeBrix.Platform.PlotterView.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.PlotterView.ApacheLicenseForever) |

---

**Where to go next**

- [TerminalView](TerminalView.md) - the other data-heavy view element, and the one that does depend on TextLayout
- [CodeBrix.Plotter](../../libraries/CodeBrix.Plotter.md) - build and export the same models from any .NET application
- [All add-ins](../08-add-ins.md) - the whole set at a glance
