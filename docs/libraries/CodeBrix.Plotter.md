<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Plotter</sub>

# CodeBrix.Plotter

**CodeBrix.Plotter draws charts, graphs and plots onto a SkiaSharp canvas.** You describe a plot as a
`PlotModel` - axes, series, annotations, legends - and the library renders that object graph, either
straight onto an `SKCanvas` your application already owns or into a PNG, JPEG, PDF or SVG file. It is
fully managed and cross-platform, and the same model renders whether the destination is a live window
in a CodeBrix.Platform application or a file written by a headless batch job.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Plotter](https://github.com/ellisnet/CodeBrix.Plotter) |
| **Packages** | [`CodeBrix.Plotter.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Plotter.MitLicenseForever) |
| **License** | MIT License; see [License](#license) |
| **Requires** | .NET 10 or later, and a `SkiaSharp.NativeAssets.*` package referenced by the application itself |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Anywhere .NET 10 and the SkiaSharp native assets run: Linux, macOS and Windows |

## What it does

- Renders a broad set of series types: line, area, bar, linear bar, interval bar, scatter, stem,
  stair-step, pie, heat map, contour, candlestick, box plot, error bar, histogram, rectangle, vector,
  tornado, volume, high/low, and the two-color, three-color and extrapolation variants
- Offers linear, logarithmic, date/time, time span, category, angle, magnitude, color and range-color
  axes, with full control over ticks, gridlines, formatting and zoom/pan limits
- Draws text, arrow, line, function, rectangle, ellipse, point, polygon, polyline and image
  annotations, on a layer of your choosing
- Places legends inside or outside the plot area, in multi-column layouts, with item ordering and
  custom symbol rendering
- Draws through `IRenderContext`, and `SkiaRenderContext` implements it against any `SKCanvas`, so a
  plot goes into whatever surface your application already owns
- Exports to PNG, JPEG, PDF and SVG through SkiaSharp, plus a built-in SVG writer that needs no
  native dependency at all
- Shapes complex scripts through SkiaSharp.HarfBuzz, and resolves every typeface through a delegate
  you supply when you want to render with no system fonts
- Ships a UI-framework-independent input model: mouse, touch and keyboard gestures bound to pan, zoom,
  tracker and reset commands, ready to be wired to a host
- Hit-tests elements and tracks selection state, with selected elements drawn in the model's
  selection color
- Decodes PNG, BMP and JPEG images and encodes PNG and BMP with its own managed codecs, so
  `ImageAnnotation` and `IRenderContext.DrawImage` need no third-party imaging dependency
- Emits C# that rebuilds a whole model: every item type implements `ICodeGenerating`, and
  `PlotModel.ToCode()` turns an interactively built plot into source
- Ships XML documentation (IntelliSense) alongside the assembly

## When to use it

Use CodeBrix.Plotter whenever an application has to turn numbers into a picture: a dashboard, a live
instrument trace, a report page, a heat map of a grid of measurements, or a batch job that writes one
chart per data file. The model is plain objects, so the chart can be built in a view model, unit
tested without a screen, and then rendered by whichever host happens to own the canvas.

It renders; it does not host. Knowing where the line falls saves time:

- There is no UI control for any framework in this package. `IPlotView` and `IView` are interfaces for
  a host to implement, and nothing here implements them.
- It does not import plots. There is no reader for SVG, PDF or any chart file format - only writers.
- It cannot encode JPEG through its own codecs. `PngEncoder` and `BmpEncoder` exist; `JpegDecoder` is
  decode-only, and JPEG output comes from `CodeBrix.Plotter.Skia.JpegExporter`, which encodes through
  SkiaSharp.
- It has no data-binding infrastructure beyond `ItemsSource` with `DataField*` or `Mapping`. There is
  no `INotifyPropertyChanged` or `INotifyCollectionChanged` plumbing; after mutating data you call
  `InvalidatePlot(true)` yourself.
- It exposes no public events at all. Input is handled through a controller and gesture-to-command
  bindings.
- It does not do 3D, geographic projection or map tiles.
- It does not ship native binaries. A consuming application adds the `SkiaSharp.NativeAssets.*`
  package for its own platform.

## Getting started

```bash
dotnet add package CodeBrix.Plotter.MitLicenseForever
```

The NuGet package ID and the namespace are different - there is no package named plain
`CodeBrix.Plotter`. The package ID is `CodeBrix.Plotter.MitLicenseForever`; the assembly and primary
namespace are `CodeBrix.Plotter`.

`SkiaSharp` and `SkiaSharp.HarfBuzz` arrive with the package, and no version pinning is needed in the
consuming project. The native assets are the application's own choice: the Windows and macOS ones
arrive transitively, and a Linux application adds them itself.

```bash
dotnet add package SkiaSharp.NativeAssets.Linux
dotnet add package HarfBuzzSharp.NativeAssets.Linux
```

Use [`SkiaSharp.NativeAssets.macOS`](https://www.nuget.org/packages/SkiaSharp.NativeAssets.macOS) or
[`SkiaSharp.NativeAssets.Win32`](https://www.nuget.org/packages/SkiaSharp.NativeAssets.Win32) for
those platforms; [`HarfBuzzSharp.NativeAssets.Linux`](https://www.nuget.org/packages/HarfBuzzSharp.NativeAssets.Linux)
is what shaped text needs on Linux.

> [!WARNING]
> Without the matching native-asset package a Linux build still compiles, and then fails at run time on
> the first render with a native-library load error.

A complete console project that writes a PNG - the project file states no versions, and the program is
the whole of the common case:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>disable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CodeBrix.Plotter.MitLicenseForever" />
    <!-- The native SkiaSharp binaries for the platform you run on.
         Pick the one(s) you need; SkiaSharp brings Win32 and macOS in
         transitively for net10.0, Linux must be named explicitly. -->
    <PackageReference Include="SkiaSharp.NativeAssets.Linux" />
    <!-- Only needed if any text is shaped through HarfBuzz on Linux. -->
    <PackageReference Include="HarfBuzzSharp.NativeAssets.Linux" />
  </ItemGroup>

</Project>
```

```csharp
using System;
using System.IO;
using CodeBrix.Plotter;
using CodeBrix.Plotter.Axes;
using CodeBrix.Plotter.Series;
using CodeBrix.Plotter.Skia;

var model = new PlotModel { Title = "Hello, plot" };
model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });
model.Axes.Add(new LinearAxis { Position = AxisPosition.Left });

var line = new LineSeries { Title = "Data", MarkerType = MarkerType.Circle };
line.Points.Add(new DataPoint(0, 0));
line.Points.Add(new DataPoint(1, 3));
line.Points.Add(new DataPoint(2, 1));
model.Series.Add(line);

PngExporter.Export(model, "hello.png", 640, 400);
Console.WriteLine(Path.GetFullPath("hello.png"));
```

The exporter updates and renders the model for you. Rendering by hand takes one extra step, described
under [Update and render](#update-and-render-through-iplotmodel).

These are the public namespaces. Folder is not namespace in this library, and guessing one is the most
common cause of a "type or namespace not found" error:

```csharp
using CodeBrix.Plotter;                 // PlotModel, IPlotModel, IRenderContext,
                                        //   PlotterColor(s), PlotterRect,
                                        //   DataPoint,
                                        //   ScreenPoint, MarkerType, LineStyle,
                                        //   the input model, the controller, the
                                        //   image codecs, the core SVG writer
using CodeBrix.Plotter.Axes;            // Axis, LinearAxis, DateTimeAxis,
                                        //   CategoryAxis, the colour axes,
                                        //   AxisPosition, TickStyle, AxisLayer
using CodeBrix.Plotter.Series;          // Series, LineSeries, BarSeries,
                                        //   PieSeries, the item types,
                                        //   LabelPlacement, VolumeStyle
using CodeBrix.Plotter.Annotations;     // Annotation, TextAnnotation,
                                        //   ArrowAnnotation, AnnotationLayer, ...
using CodeBrix.Plotter.Legends;         // Legend, LegendBase, LegendPosition,
                                        //   LegendPlacement, LegendOrientation, ...
using CodeBrix.Plotter.Axes.Rendering;  // axis renderers (only needed to write a
                                        //   custom axis renderer)
using CodeBrix.Plotter.Utilities;       // Helpers (Swap, ArgMin,
                                        //   LinearInterpolation) -- ONLY that
using CodeBrix.Plotter.Skia;            // SkiaRenderContext, RenderTarget,
                                        //   TypefaceResolver, SkiaExtensions,
                                        //   PngExporter, JpegExporter,
                                        //   PdfExporter, SvgExporter
```

There is no `CodeBrix.Plotter.Input`, `.Rendering`, `.Graphics`, `.Imaging`, `.PlotController`,
`.PlotModel`, `.PlotView`, `.Pdf`, `.Svg` or `.Foundation` namespace.

## Key concepts

### The PlotModel

Everything starts with a `PlotModel`. It owns four element collections - `Axes`, `Series`,
`Annotations` and `Legends` - plus the plot-wide appearance settings: `Title`, `Subtitle`,
`TitleFont`, `DefaultFont`, `DefaultFontSize`, `TitleColor`, `TextColor`, `Background`,
`PlotAreaBackground`, `Padding`, `PlotMargins`, `PlotType` (`XY`, `Cartesian` or `Polar`),
`EdgeRenderingMode`, `DefaultColors`, `IsLegendVisible`, `Culture` and `RenderingDecorator`.

Its geometry - `PlotBounds`, `PlotArea`, `PlotAndAxisArea`, `TitleArea`, `ActualPlotMargins`, `Width`,
`Height`, `DefaultXAxis`, `DefaultYAxis`, `DefaultColorAxis` - is read-only and valid after an update.
The methods you reach for are `InvalidatePlot(bool updateData)`, `GetAxis(string key)` (which throws
when the key is unknown) and `GetAxisOrDefault`, `GetSeriesFromPoint`, `ResetAllAxes`, `PanAllAxes`,
`ZoomAllAxes`, `GetLastPlotException()` and `ToCode()`.

A `PlotModel` is not thread-safe. Mutate it from one thread at a time, never while it is rendering;
`Model.SyncRoot` is available as a lock object.

### Update and render through IPlotModel

`PlotModel` implements `IPlotModel` explicitly, so `Update` and `Render` are not visible on a
`PlotModel` variable - you cast:

```csharp
public interface IPlotModel
{
    PlotterColor Background { get; }
    void Update(bool updateData);
    void Render(IRenderContext rc, PlotterRect rect);
    void AttachPlotView(IPlotView plotView);
}

((IPlotModel)model).Update(updateData: true);
((IPlotModel)model).Render(renderContext, new PlotterRect(0, 0, w, h));
```

Pass `updateData: true` when the underlying data changed, and false when only the layout changed, such
as a resize. A freshly built `PlotModel` has no resolved axis ranges, so rendering it without an
update first produces an empty or wrong plot. The exporters take `IPlotModel` and call `Update` and
`Render` themselves, which is why no cast is needed when you go through `PngExporter` and friends.

Errors raised while a plot renders are captured, not thrown out of `Render`; call
`model.GetLastPlotException()` to retrieve one.

### Elements and collections

Every axis, series, annotation and legend derives from `PlotElement`, which derives from `Element`.
`PlotElement` carries `Font`, `FontSize`, `FontWeight` (`FontWeights.Normal` or `FontWeights.Bold`),
`TextColor`, `EdgeRenderingMode`, `Tag` and `ToolTip`, plus a read-only `PlotModel` back-pointer.
`ElementCollection<T>` is an `IList<T>` that re-parents what you add and clears the parent of what you
remove.

### Axes

All axis types live in `CodeBrix.Plotter.Axes` and derive from `Axis`: `LinearAxis` and its
descendants `CategoryAxis`, `DateTimeAxis`, `TimeSpanAxis`, `AngleAxis`, `MagnitudeAxis`,
`LinearColorAxis` and `RangeColorAxis`, alongside `LogarithmicAxis` and `LogarithmicColorAxis`. These
are the members you set most:

```csharp
AxisPosition Position          // None | Left | Right | Top | Bottom | All
string Title, Unit, Key, StringFormat, TitleFormatString
double Minimum, Maximum                 // NaN = auto from the data
double AbsoluteMinimum, AbsoluteMaximum // hard pan/zoom limits
double MajorStep, MinorStep, MinimumMajorStep, MinimumMinorStep
double MinimumPadding, MaximumPadding   // fraction of the range, both ends
LineStyle MajorGridlineStyle, MinorGridlineStyle, AxislineStyle,
          ExtraGridlineStyle
double[] ExtraGridlines
TickStyle TickStyle            // Crossing | Inside | Outside | None
AxisLayer Layer                // BelowSeries | AboveSeries
bool IsAxisVisible, IsPanEnabled, IsZoomEnabled, IsReversed,
     PositionAtZeroCrossing, CropGridlines
double StartPosition, EndPosition   // 0..1 fractions of the plot area
int    PositionTier                 // stacking several axes on one side
Func<double, string> LabelFormatter
Func<double, bool>   FilterFunction
```

`ActualMinimum`, `ActualMaximum`, `ActualMajorStep`, `ActualStringFormat`, `ClipMinimum`,
`DataMinimum`, `ScreenMin`, `Scale` and `Offset` are read-only after an update. Two axes on the same
side are laid out with `StartPosition`/`EndPosition` or `PositionTier`; that is how stacked sub-plots
are built.

`DateTimeAxis` plots dates by conversion - `new DataPoint(DateTimeAxis.ToDouble(t), v)` or
`DateTimeAxis.CreateDataPoint(t, v)` - and carries `IntervalType`, `MinorIntervalType`,
`CalendarWeekRule`, `FirstDayOfWeek`, `DateTimePrecision` and `TimeZone`. `CategoryAxis` is the axis a
`BarSeries` needs: fill its `Labels` list one label per category, or bind `ItemsSource` with
`LabelField`, and control spacing with `GapWidth` and `IsTickCentered`. Polar plots pair `AngleAxis`
with `MagnitudeAxis` and `model.PlotType = PlotType.Polar`.

### Color axes and palettes

A color axis maps a value to a color. `HeatMapSeries`, `ContourSeries`, `RectangleSeries`,
`VectorSeries` and `ScatterSeries` can all take one, found by `ColorAxisKey` or, failing that, by
being the model's only color axis.

```csharp
interface IColorAxis : IPlotElement
{
    PlotterColor GetColor(int paletteIndex);
    int          GetPaletteIndex(double value);
}
interface INumericColorAxis : IColorAxis { }
```

`LinearColorAxis` and `LogarithmicColorAxis` default their `Palette` to `PlotterPalettes.Viridis()`
and add `HighColor`, `LowColor`, `InvalidNumberColor` and `RenderAsImage`; `RangeColorAxis` takes
explicit bands through `AddRange(double lowerBound, double upperBound, PlotterColor color)`;
`CategoryColorAxis` colors categories.

`PlotterPalettes` is a static class, and most of its members are methods that take a color count -
`BlackWhiteRed`, `BlueWhiteRed`, `Cool`, `Gray`, `Hot`, `Hue`, `HueDistinct`, `Jet` and `Rainbow`.
`Cividis`, `Inferno`, `Magma`, `Plasma` and `Viridis` are also methods, with a default count. Only
`BlueWhiteRed31`, `Hot64` and `Hue64` are properties, pre-sized as their names say. A palette of your
own is `new PlotterPalette(params PlotterColor[])`, or
`PlotterPalette.Interpolate(paletteSize, colors)`.

### Series and their item types

All series live in `CodeBrix.Plotter.Series`. The most common mistake is guessing the item type, so
this table pairs every concrete series with the exact collection you fill and the exact item type it
holds:

| Series | Fill this | With this item type |
| --- | --- | --- |
| `LineSeries` | `.Points` | `DataPoint` |
| `AreaSeries` | `.Points` and `.Points2` | `DataPoint` |
| `TwoColorLineSeries` | `.Points` | `DataPoint` |
| `TwoColorAreaSeries` | `.Points` and `.Points2` | `DataPoint` |
| `ThreeColorLineSeries` | `.Points` | `DataPoint` |
| `ExtrapolationLineSeries` | `.Points` (+ `.Intervals`) | `DataPoint` (+ `DataRange`) |
| `StairStepSeries` | `.Points` | `DataPoint` |
| `StemSeries` | `.Points` | `DataPoint` |
| `LinearBarSeries` | `.Points` | `DataPoint` |
| `FunctionSeries` | (constructor) | built from a `Func` |
| `ScatterSeries` | `.Points` | `ScatterPoint` |
| `ScatterErrorSeries` | `.Points` | `ScatterErrorPoint` |
| `BarSeries` | `.Items` | `BarItem` |
| `ErrorBarSeries` | `.Items` | `ErrorBarItem` |
| `IntervalBarSeries` | `.Items` | `IntervalBarItem` |
| `TornadoBarSeries` | `.Items` | `TornadoBarItem` |
| `RectangleBarSeries` | `.Items` | `RectangleBarItem` |
| `BoxPlotSeries` | `.Items` | `BoxPlotItem` |
| `HistogramSeries` | `.Items` | `HistogramItem` |
| `RectangleSeries` | `.Items` | `RectangleItem` |
| `VectorSeries` | `.Items` | `VectorItem` |
| `HighLowSeries` | `.Items` | `HighLowItem` |
| `CandleStickSeries` | `.Items` | `HighLowItem` |
| `VolumeSeries` | `.Items` | `OhlcvItem` |
| `PieSeries` | `.Slices` | `PieSlice` |
| `HeatMapSeries` | `.Data` | `double[,]` |
| `ContourSeries` | `.Data` + `.ColumnCoordinates` + `.RowCoordinates` | `double[,]` plus `double[]` coordinates |

Every series carries `Title` (shown in the legend), `IsVisible`, `RenderInLegend`, `LegendKey`,
`SeriesGroupName`, `TrackerKey`, `TrackerFormatString`, `Background` and
`GetNearestPoint(ScreenPoint point, bool interpolate)`. `XYAxisSeries` adds `XAxisKey` and `YAxisKey`,
the read-only `XAxis`/`YAxis` and `MinX`/`MaxX`/`MinY`/`MaxY`, and `Transform`/`InverseTransform`. A
series given no axis keys binds to the model's default axis in the matching position.

`PieSeries` is not an XY series, so it needs no axes at all.

### There is no ColumnSeries: transposition comes from the axes

One series type draws both horizontal bars and vertical columns, and the axis positions decide which.
`element.IsTransposed()` is defined as `element.XAxis.IsVertical()`: put the series' X axis on the
left or right and the plot is transposed; put it on the bottom or top and it is not. The helpers -
`IsTransposed()`, `Orientate`, `Transform`, `InverseTransform` - are extension methods on
`PlotElementExtensions` in the root namespace.

Bars stack through `BarSeries.IsStacked` and `StackGroup`; series sharing a group stack together.
`BarSeriesManager` coordinates stacking and slot widths across all the bar series sharing one
`CategoryAxis`, and you never create one - the update does. `BarItemBase.CategoryIndex` defaults to
`-1`, meaning the next free slot.

### Data binding, smoothing and decimation

`DataPointSeries` accepts `Points` directly, or an `ItemsSource` with `DataFieldX` and `DataFieldY`,
or a `Mapping` delegate. Items that implement `IDataPointProvider` are consumed directly, with no data
field or mapping needed. `DataPoint.Undefined`, whose coordinates are both NaN, breaks a line into
segments.

For smooth curves, assign an interpolation algorithm from the root namespace -
`InterpolationAlgorithms.CanonicalSpline`, `CatmullRomSpline`, `UniformCatmullRomSpline` or
`ChordalCatmullRomSpline` - or construct `CanonicalSpline(tension)` or `CatmullRomSpline(alpha)`.
`LineSeries` also carries the styling surface you would expect: `Color`, `MarkerFill`, `MarkerStroke`,
`StrokeThickness`, `Dashes`, `LineStyle`, `LineJoin`, `MarkerType`
(`None`, `Circle`, `Square`, `Diamond`, `Triangle`, `Cross`, `Plus`, `Star`, `Custom`), `MarkerSize`,
`MinimumSegmentLength`, `LabelFormatString`, `LineLegendPosition` and a `Decimator` delegate.

### Statistical, grid and financial series

`BoxPlotSeries` takes
`BoxPlotItem(double x, double lowerWhisker, double boxBottom, double median, double boxTop, double upperWhisker)`
plus optional `Mean` and `Outliers`. `HistogramSeries` bins raw samples through `HistogramHelpers` in
the root namespace:

```csharp
var binBreaks = HistogramHelpers.CreateUniformBins(0, 100, 20);
var items = HistogramHelpers.Collect(
    samples,
    binBreaks,
    new BinningOptions(BinningOutlierMode.CountOutliers,
                       BinningIntervalType.InclusiveLowerBound,
                       BinningExtremeValueMode.IncludeExtremeValues));
```

`HeatMapSeries` holds a `double[,]` in `Data` with `X0`/`X1`/`Y0`/`Y1` bounds, a `CoordinateDefinition`
of `Center` or `Edge`, a `RenderMethod` of `Bitmap` or `Rectangles`, and `Interpolate`; mutating
`Data` in place needs a call to `Invalidate()`. `ContourSeries` exposes `ContourLevels`,
`ContourLevelStep` and `CalculateContours()`, and the tracer itself is available directly as
`Conrec.Contour(...)`. `VectorSeries` draws an arrow field from
`VectorItem(DataPoint origin, DataVector direction, double value)`, each arrow colored by value.
`HighLowSeries` and `CandleStickSeries` share `HighLowItem`, and `VolumeSeries` takes `OhlcvItem` with
a `VolumeStyle` of `None`, `Combined`, `Stacked` or `PositiveNegative`.

### Annotations

Annotations live in `CodeBrix.Plotter.Annotations` and go into `model.Annotations`. Every one of them
carries a `Layer` of `BelowAxes`, `BelowSeries` or `AboveSeries`, plus `XAxisKey`/`YAxisKey`,
`ClipByXAxis`/`ClipByYAxis`, `Transform`/`InverseTransform` and `EnsureAxes()`.

`TextAnnotation` and `ArrowAnnotation` place text and arrows in data coordinates; `LineAnnotation`
draws a horizontal, vertical or `LinearEquation` line; `FunctionAnnotation` draws `y = f(x)` or
`x = f(y)`; `RectangleAnnotation`, `EllipseAnnotation`, `PointAnnotation`, `PolygonAnnotation` and
`PolylineAnnotation` cover the shapes; and `ImageAnnotation` places a `PlotterImage`. Positions and
sizes on an image annotation are `PlotLength` values -
`PlotLength(double value, PlotLengthUnit unit)` with units of `Data`, `ScreenUnits`,
`RelativeToViewport` or `RelativeToPlotArea`.

### Legends

Add a `Legend` from `CodeBrix.Plotter.Legends` to `model.Legends`. A series appears in a legend when
its `Title` is set and `RenderInLegend` is true; `LegendKey` on the series selects which legend when
there is more than one, matched against `LegendBase.Key`.

```csharp
model.IsLegendVisible = true;
model.Legends.Add(new Legend
{
    LegendTitle = "Series",
    LegendPosition = LegendPosition.RightTop,
    LegendPlacement = LegendPlacement.Outside,
    LegendOrientation = LegendOrientation.Vertical,
});
```

`LegendPosition` runs from `TopLeft` through `RightBottom`, `LegendPlacement` is `Inside` or
`Outside`, and `LegendOrientation`, `LegendItemOrder`, `LegendSymbolPlacement` and the spacing,
padding and maximum-size doubles cover the rest of the layout.

### Geometry and PlotterColor

The geometry types in the root namespace are immutable structs, so you "modify" them by constructing a
new value: `DataPoint`, `DataVector`, `ScreenPoint`, `ScreenVector`, `PlotterRect`, `PlotterSize`,
`PlotterThickness` and `PlotLength`.

`PlotterColor` is an immutable struct with sentinel values, not a class. Build one with `FromRgb`,
`FromArgb`, `FromAColor`, `FromUInt32`, `FromHsv`, `Parse` or `Interpolate`, adjust it with
`ChangeIntensity`, `ChangeSaturation`, `ChangeOpacity` or `Complementary`, and test it with
`IsUndefined()`, `IsAutomatic()`, `IsInvisible()` and `IsVisible()` - never against null. A struct is
never null, and `PlotterColors.Undefined` and `PlotterColors.Automatic` are distinct non-null
sentinels. `PlotterColors` also carries the named colors.

### SkiaRenderContext

Everything in the library draws through `IRenderContext` - `DrawEllipse`, `DrawLine`,
`DrawLineSegments`, `DrawPolygon`, `DrawRectangle`, `DrawText`, `DrawImage`, `MeasureText`,
`SetToolTip`, `PushClip`/`PopClip` and `CleanUp` - so anything that can draw can host a plot by
implementing it. `CodeBrix.Plotter.Skia.SkiaRenderContext` is that interface plus `IDisposable`,
implemented against SkiaSharp:

```csharp
using var context = new SkiaRenderContext
{
    RenderTarget = RenderTarget.Screen,   // Screen: hinted, subpixel text.
                                          // PixelGraphic / VectorGraphic:
                                          // unhinted, better for export.
    SkCanvas = canvas,                    // the SKCanvas to draw on
    DpiScale = 1.0f,                      // logical-to-device scale
    UseTextShaping = true,                // shape through HarfBuzz
    MiterLimit = 10,
};
```

`RendersToScreen` is derived from `RenderTarget` and cannot be assigned. The context caches
`SKTypeface`, `SKFont` and `SKShaper` instances per font descriptor, so keep one alive across frames
when you render repeatedly, and dispose it when you are finished. It does not own the `SKCanvas` and
will not dispose it; you may reassign `SkCanvas` between frames. `SkiaExtensions` offers
`PlotterColor.ToSKColor()` and `SKColor.ToPlotterColor()`.

`RenderContextBase` implements most of `IRenderContext` in terms of `DrawLine`, `DrawPolygon` and
`DrawText` if you write your own; `ClippingRenderContext` adds clip-stack bookkeeping; and
`XkcdRenderingDecorator` wraps another context to draw in a hand-drawn style, assigned through
`model.RenderingDecorator`.

### The TypefaceResolver contract

By default `SkiaRenderContext` resolves a font family name through the system font lookup. The
`TypefaceResolver` property replaces that lookup entirely; its delegate type has the same name, in the
same namespace:

```csharp
public delegate SKTypeface TypefaceResolver(string fontFamily, double fontWeight);

context.TypefaceResolver = (fontFamily, fontWeight) =>
    myFonts.TryGetValue(fontFamily, out SKTypeface found)
        ? found
        : myDefaultTypeface;
```

A resolver owns resolution completely: the system font lookup is never consulted, not even when the
resolver cannot resolve the family, and returning null is treated as `SKTypeface.Default` rather than
a fallback to the system. `fontWeight` arrives as the numeric weight, 400 for normal and 700 for bold.
Results are cached per family-and-weight pair, and assigning a different resolver, or null, clears the
typeface and shaper caches. Typefaces returned by a resolver stay owned by the resolver - the render
context never disposes them.

### Exporters

Every exporter implements one interface, and each also offers static path and stream overloads:

```csharp
public interface IExporter { void Export(IPlotModel model, Stream stream); }
```

The exporters call `Update(true)` and `Render` on the model themselves, so a `PlotModel` can be handed
straight to them with no cast. `CodeBrix.Plotter.Skia` carries `PngExporter` (`Width`, `Height`,
`Dpi`, `UseTextShaping`), `JpegExporter` (which adds `Quality`), `PdfExporter` (vector PDF through
`SKDocument`) and `SvgExporter` (vector SVG through `SKSvgCanvas`).

The root namespace also holds an SVG writer that needs no SkiaSharp at all, with `IsDocument`,
`UseVerticalTextAlignmentWorkaround`, `TextMeasurer`, `Export` and `ExportToString`. It writes through
the public `SvgRenderContext` over `SvgWriter`, so you can emit SVG fragments directly rather than a
whole plot. That writer has the same type name as the Skia one in a different namespace, so with both
`using` directives in scope you must qualify it. The Skia exporters generally produce better text
output, because they shape and measure with the real font; the core writer is there when you want zero
native dependencies. There is no PDF exporter in the root namespace - only
`CodeBrix.Plotter.Skia.PdfExporter`.

### Input, controllers and commands

The library ships a UI-framework-independent input model so a host view can map its own events onto
plot commands, and all of it is in the root namespace. Events arrive as `PlotterMouseEventArgs`,
`PlotterMouseDownEventArgs`, `PlotterMouseWheelEventArgs`, `PlotterKeyEventArgs` and
`PlotterTouchEventArgs`, all carrying `Handled` and the modifier-key tests. Gestures
(`PlotterMouseDownGesture`, `PlotterMouseWheelGesture`, `PlotterKeyGesture`, `PlotterTouchGesture`,
`PlotterShakeGesture`) bind to commands, and `PlotCommands` is a static class of ready-made ones
covering reset, pan, zoom, tracker, hover and touch.

```csharp
var controller = new PlotController();

// Left-drag pans, right-drag draws a zoom rectangle, wheel zooms,
// Ctrl+wheel zooms in fine steps, left-click shows the tracker.
controller.UnbindAll();
controller.BindMouseDown(PlotterMouseButton.Left, PlotCommands.PanAt);
controller.BindMouseDown(PlotterMouseButton.Right, PlotCommands.ZoomRectangle);
controller.BindMouseWheel(PlotCommands.ZoomWheel);
controller.BindMouseWheel(PlotterModifierKeys.Control, PlotCommands.ZoomWheelFine);
controller.BindMouseDown(PlotterMouseButton.Left, PlotterModifierKeys.Shift,
                         PlotCommands.SnapTrack);
controller.BindKeyDown(PlotterKey.A, PlotCommands.Reset);

// A command of your own:
var myCommand = new DelegateViewCommand<PlotterMouseDownEventArgs>(
    (view, ctrl, args) =>
    {
        var plotView = (IPlotView)view;
        plotView.ActualModel.ZoomAllAxes(2);
        plotView.InvalidatePlot(false);
        args.Handled = true;
    });
controller.BindMouseDown(PlotterMouseButton.Middle, myCommand);
```

Commands drive manipulators - `PanManipulator`, `ZoomRectangleManipulator`, `ZoomStepManipulator`,
`TrackerManipulator`, `TouchManipulator`, `TouchTrackerManipulator` - and a tracker command produces a
`TrackerHitResult` for the host view to display. If you want tracking without a controller,
`Series.GetNearestPoint(ScreenPoint point, bool interpolate)` returns one directly.

### Hit testing and selection

`Element.HitTest(new HitTestArguments(point, tolerance))` returns a `HitTestResult`, and because
`PlotModel` derives from `Model`, hit-testing the whole plot is one call:

```csharp
var hits = model.HitTest(new HitTestArguments(new ScreenPoint(x, y), 10));
```

Selection state lives on `Element`: `Selectable`, a `SelectionMode` of `All`, `Single` or `Multiple`,
and `Select()`, `Unselect()`, `SelectItem(int)`, `UnselectItem(int)`, `ClearSelection()`,
`IsSelected()`, `IsItemSelected(int)` and `GetSelectedItems()`. Selected elements are drawn in
`model.SelectionColor`.

### The built-in image codecs

`PlotterImage` accepts bytes or a stream and exposes `Format`, `Width`, `Height`, `BitsPerPixel`,
`DpiX`/`DpiY`, `GetData()` and `GetPixels()`, which returns a `PlotterColor[,]` indexed `[x, y]` with
`[0,0]` at the top left; `PlotterImage.Create` builds one from a pixel array or a palette-indexed
array. The codecs are `PngEncoder`, `BmpEncoder`, `PngDecoder`, `BmpDecoder` and `JpegDecoder`, which
also exposes `ExifTags`. PNG and BMP can be both encoded and decoded; JPEG can only be decoded.

## Examples

A complete program that builds a model with two function series, a legend and gridlines, and writes it
to a PNG file:

```csharp
using System;
using System.IO;
using CodeBrix.Plotter;
using CodeBrix.Plotter.Axes;
using CodeBrix.Plotter.Legends;
using CodeBrix.Plotter.Series;
using CodeBrix.Plotter.Skia;

public static class Program
{
    public static void Main()
    {
        var model = new PlotModel
        {
            Title = "Trigonometric functions",
            Subtitle = "sin and cos",
            Background = PlotterColors.White,
        };

        model.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Bottom,
            Title = "x",
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.Dot,
        });
        model.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "y",
            Minimum = -1.5,
            Maximum = 1.5,
        });

        model.Series.Add(new FunctionSeries(Math.Sin, -10, 10, 0.05, "sin(x)"));
        model.Series.Add(new FunctionSeries(Math.Cos, -10, 10, 0.05, "cos(x)")
        {
            Color = PlotterColors.IndianRed,
            LineStyle = LineStyle.Dash,
        });

        model.IsLegendVisible = true;
        model.Legends.Add(new Legend
        {
            LegendPosition = LegendPosition.RightTop,
            LegendPlacement = LegendPlacement.Outside,
        });

        // The exporter calls Update() and Render() itself.
        var exporter = new PngExporter { Width = 800, Height = 480, Dpi = 96 };
        using var stream = File.Create("trig.png");
        exporter.Export(model, stream);

        // Or, in one call:
        PngExporter.Export(model, "trig-2.png", 800, 480);
    }
}
```

Hosting the same model on a canvas your application already owns, with one render context kept alive
across frames because it caches typefaces, fonts and shapers:

```csharp
using CodeBrix.Plotter;
using CodeBrix.Plotter.Skia;
using SkiaSharp;

public sealed class PlotRenderer : IDisposable
{
    // One context, kept alive across frames: it caches typefaces,
    // fonts and shapers.
    private readonly SkiaRenderContext context =
        new SkiaRenderContext { RenderTarget = RenderTarget.Screen };

    public void Draw(SKCanvas canvas, PlotModel model, int width, int height,
                     float dpiScale, bool dataChanged)
    {
        context.SkCanvas = canvas;      // the context does NOT own the canvas
        context.DpiScale = dpiScale;

        var plot = (IPlotModel)model;   // Update/Render are explicit
        plot.Update(dataChanged);
        canvas.Clear(model.Background.ToSKColor());
        plot.Render(context, new PlotterRect(0, 0, width / dpiScale,
                                                   height / dpiScale));
    }

    public void Dispose() => this.context.Dispose();
}
```

Rendering into an `SKBitmap` - the way to get a bitmap, since no exporter has an `ExportToBitmap`:

```csharp
using CodeBrix.Plotter;
using CodeBrix.Plotter.Skia;
using SkiaSharp;

public static SKBitmap RenderToBitmap(PlotModel model, int width, int height)
{
    var bitmap = new SKBitmap(width, height);

    using (var canvas = new SKCanvas(bitmap))
    using (var context = new SkiaRenderContext
           {
               RenderTarget = RenderTarget.PixelGraphic,   // unhinted text
               SkCanvas = canvas,
           })
    {
        var plot = (IPlotModel)model;
        plot.Update(true);
        canvas.Clear(model.Background.ToSKColor());
        plot.Render(context, new PlotterRect(0, 0, width, height));
    }

    return bitmap;      // caller disposes
}
```

Horizontal bars and vertical columns, from the one transposable `BarSeries` - the second model is the
same series type with the axis roles swapped:

```csharp
using CodeBrix.Plotter;
using CodeBrix.Plotter.Axes;
using CodeBrix.Plotter.Series;

// ---- horizontal bars -------------------------------------------------
var bars = new PlotModel { Title = "Revenue by region" };

var categories = new CategoryAxis { Position = AxisPosition.Left };
categories.Labels.Add("North");
categories.Labels.Add("South");
categories.Labels.Add("East");
categories.Labels.Add("West");
bars.Axes.Add(categories);
bars.Axes.Add(new LinearAxis
{
    Position = AxisPosition.Bottom,
    MinimumPadding = 0,
    MaximumPadding = 0.06,
    ExtraGridlines = new[] { 0d },
});

var first = new BarSeries { Title = "First half", LabelFormatString = "{0}",
                            LabelPlacement = LabelPlacement.Inside };
first.Items.Add(new BarItem(120));
first.Items.Add(new BarItem(95));
first.Items.Add(new BarItem(140));
first.Items.Add(new BarItem(70));

var second = new BarSeries { Title = "Second half", LabelFormatString = "{0}" };
second.Items.Add(new BarItem(150));
second.Items.Add(new BarItem(88));
second.Items.Add(new BarItem(165));
second.Items.Add(new BarItem(102));

bars.Series.Add(first);
bars.Series.Add(second);
// Stack them instead of clustering:
//   first.IsStacked = second.IsStacked = true;
//   first.StackGroup = second.StackGroup = "revenue";

// ---- vertical columns: same series type, transposed axes -------------
var cols = new PlotModel { Title = "Revenue by region" };

var colCategories = new CategoryAxis { Position = AxisPosition.Bottom,
                                       Key = "categories" };
colCategories.Labels.Add("North");
colCategories.Labels.Add("South");
cols.Axes.Add(colCategories);
cols.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Key = "values" });

var columns = new BarSeries { Title = "Second half",
                              XAxisKey = "values",       // X is the VALUE axis
                              YAxisKey = "categories" }; // Y is the CATEGORY axis
columns.Items.Add(new BarItem(150));
columns.Items.Add(new BarItem(88));
cols.Series.Add(columns);
```

A heat map with its own color axis, found by key. `PlotterPalettes.Jet` is a method that takes a color
count, and the axis keeps the high, low and invalid colors distinct from the palette:

```csharp
using CodeBrix.Plotter;
using CodeBrix.Plotter.Axes;
using CodeBrix.Plotter.Series;

var data = new double[100, 80];
for (int x = 0; x < 100; x++)
for (int y = 0; y < 80; y++)
{
    data[x, y] = System.Math.Sin(x / 10.0) * System.Math.Cos(y / 8.0);
}

var model = new PlotModel { Title = "Heat map" };
model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });
model.Axes.Add(new LinearAxis { Position = AxisPosition.Left });
model.Axes.Add(new LinearColorAxis
{
    Position = AxisPosition.Right,
    Palette = PlotterPalettes.Jet(200),
    HighColor = PlotterColors.Gray,
    LowColor  = PlotterColors.Black,
    Key = "colors",
});

model.Series.Add(new HeatMapSeries
{
    X0 = 0, X1 = 99, Y0 = 0, Y1 = 79,
    Data = data,
    Interpolate = true,
    CoordinateDefinition = HeatMapCoordinateDefinition.Center,
    RenderMethod = HeatMapRenderMethod.Bitmap,
    ColorAxisKey = "colors",
});
```

Rendering with no system fonts at all, which is what a font-isolated host needs. The resolver never
returns null, and the typefaces stay owned by the code that opened them:

```csharp
using CodeBrix.Plotter;
using CodeBrix.Plotter.Skia;
using SkiaSharp;

// Typefaces YOU own; the resolver never disposes them and neither does
// the render context.
using var regular = SKTypeface.FromFile("Fonts/Roboto-Regular.ttf");
using var bold    = SKTypeface.FromFile("Fonts/Roboto-Bold.ttf");

using var context = new SkiaRenderContext
    { RenderTarget = RenderTarget.PixelGraphic };
context.TypefaceResolver = (fontFamily, fontWeight) =>
    fontWeight >= FontWeights.Bold ? bold : regular;   // never returns null

var model = new PlotModel { Title = "Isolated fonts", DefaultFont = "Roboto" };
// ... axes and series ...

using var bitmap = new SKBitmap(400, 300);
using var canvas = new SKCanvas(bitmap);
context.SkCanvas = canvas;
canvas.Clear(SKColors.White);

var plot = (IPlotModel)model;
plot.Update(true);
plot.Render(context, new PlotterRect(0, 0, 400, 300));
```

## Using it in a CodeBrix.Platform application

The `SkiaSharp` and `SkiaSharp.HarfBuzz` dependencies this package takes are pinned in lock-step with
rest of the CodeBrix family, so a CodeBrix.Platform application consumes the library without a version
conflict. If you pin SkiaSharp yourself, move `SkiaSharp` and `SkiaSharp.HarfBuzz` together - they
ship as a matched set.

This library renders; it does not host. There is no plot view control in the package, and `IPlotView`
is an interface for you to implement. Hosting it yourself is the pattern shown above: keep one
`SkiaRenderContext` alive, hand it the canvas the head gives you on each frame, and connect the model
to your view with `((IPlotModel)model).AttachPlotView(myView)` so a `PlotController` can drive it.

If you would rather not write that host, CodeBrix.Platform publishes a plot view add-in,
[`CodeBrix.Platform.PlotterView.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.PlotterView.ApacheLicenseForever);
its page is [PlotterView add-in](../platform/add-ins/PlotterView.md).

On Linux, the application must add `SkiaSharp.NativeAssets.Linux` and, for shaped text,
`HarfBuzzSharp.NativeAssets.Linux`. Without those a Linux build still compiles and then fails at run
time on the first render with a native-library load error.

## Pitfalls

- Forgetting to update the model. A freshly built `PlotModel` has no resolved axis ranges, so
  rendering it without `((IPlotModel)model).Update(true)` produces an empty or wrong plot. The
  exporters do this for you; hand-rolled rendering does not.
- Calling `Update` or `Render` on a `PlotModel` variable. They are explicit `IPlotModel`
  implementations, so you must cast. Passing a `PlotModel` to something that takes `IPlotModel` -
  every exporter - needs no cast.
- Looking for a `ColumnSeries`. It does not exist. One transposable `BarSeries` draws both bars and
  columns, and the transposed form is the one where the series' X axis is the value axis on the left
  and its Y axis is the `CategoryAxis` on the bottom.
- Mixing up which axis a bar series' `XAxisKey` names. `XAxisKey` is always the series' X axis, which
  for a vertical column chart is the value axis.
- Adding a `BarSeries` without a `CategoryAxis`, or adding more category labels than there are items,
  or fewer. Every `BarItem` lands in the next free category slot unless you set
  `BarItemBase.CategoryIndex` explicitly.
- Looking for `ExportToBitmap` on an exporter. No exporter has one; render to an `SKBitmap` through a
  `SkiaRenderContext` instead.
- Treating `PlotterPalettes` members as properties. Almost all of them are methods taking a color
  count - `PlotterPalettes.Jet(200)`, not `PlotterPalettes.Jet`. Only `BlueWhiteRed31`, `Hot64` and
  `Hue64` are properties.
- Comparing a `PlotterColor` against null. It is a struct with sentinel values; use `IsUndefined()`,
  `IsAutomatic()`, `IsInvisible()` and `IsVisible()`.
- Assuming folder equals namespace. `CodeBrix.Plotter.Utilities` holds only the `Helpers` class, and
  the input model, the controller, the codecs and the core SVG writer are all in the root namespace.
- Ambiguity between `CodeBrix.Plotter.SvgExporter` and `CodeBrix.Plotter.Skia.SvgExporter`. With both
  namespaces imported you must qualify the type name. There is no core `PdfExporter` at all.
- Looking for input events. There are no public events anywhere in this library; handle input through
  `PlotController` and gesture-to-command bindings.
- Mutating a `PlotModel` from several threads, or while it is rendering. It is not thread-safe;
  `Model.SyncRoot` is there to lock on.
- Reusing a `SkiaRenderContext` across canvases without reassigning `SkCanvas`, or disposing the
  `SKCanvas` expecting the context to have taken ownership. It never owns the canvas.
- Expecting a `TypefaceResolver` miss to fall back to the system font lookup. It never does. Return a
  default typeface instead of null, and do not dispose a resolver-supplied typeface while the render
  context might still draw with it.
- Forgetting the native-asset package. Without `SkiaSharp.NativeAssets.<os>`, and
  `HarfBuzzSharp.NativeAssets.Linux` for shaped text on Linux, the first render throws a
  `DllNotFoundException`, not a plotting error.
- Swallowing render errors. Exceptions raised while a plot renders are captured, so check
  `model.GetLastPlotException()` when a plot comes out blank.
- Mutating `HeatMapSeries.Data` in place without calling `Invalidate()` afterwards.
- The library is compiled with nullable reference types off, so its public surface carries no
  annotations. Consuming it from a project with them enabled is fine, but do not expect null-state
  analysis on its API.

For speed: keep one `SkiaRenderContext` alive across frames; pass `Update(false)` when only the layout
changed; set `UseTextShaping = false` when the text is plain Latin; assign
`LineSeries.Decimator = Decimator.Decimate` and raise `MinimumSegmentLength` for dense lines; use
`ScatterSeries.BinSize`; prefer `HeatMapRenderMethod.Bitmap`; fill `.Points` and `.Items` directly
rather than `ItemsSource` with `DataFieldX`/`DataFieldY`, because the data-field path resolves
properties by reflection on every update; set explicit `Minimum` and `Maximum` for streaming data; use
`RenderTarget.Screen` for a live canvas and `PixelGraphic` or `VectorGraphic` for deterministic
export; and note that `EdgeRenderingMode.PreferSpeed` skips pixel snapping.

## Samples and tools in the repository

| Name | What it demonstrates | Where |
| --- | --- | --- |
| PicoScope sample | Live oscilloscope traces streamed into a chart and rendered through two different SkiaSharp canvas hosts | [`samples/PicoScope`](https://github.com/ellisnet/CodeBrix.Plotter/tree/main/samples/PicoScope) |
| PicoScope guide | Capability map, cookbook, device API reference and troubleshooting for that sample | [`samples/PicoScope/README.md`](https://github.com/ellisnet/CodeBrix.Plotter/blob/main/samples/PicoScope/README.md) |
| Example library | A complete `PlotModel` per chart variation, organized by feature - the fastest way to find a working recipe | [`tests/CodeBrix.Plotter.Tests/ExampleLibrary`](https://github.com/ellisnet/CodeBrix.Plotter/tree/main/tests/CodeBrix.Plotter.Tests/ExampleLibrary) |
| Skia examples | Worked `SkiaRenderContext` usage, the `TypefaceResolver` contract, and every exporter | [`tests/CodeBrix.Plotter.Tests/Skia`](https://github.com/ellisnet/CodeBrix.Plotter/tree/main/tests/CodeBrix.Plotter.Tests/Skia) |

The PicoScope sample is the one to read for architecture. Its `ScopePlot` class builds and owns a
`PlotModel` and exposes it as a property - a clean separation between chart state and the UI head -
appends live data under `MaxPointsPerChannel` and `StreamWindowSeconds` limits, and is rendered
unchanged by a WPF head hosting an `SKElement` and a WinUI head hosting an `SKXamlCanvas`. The
device-agnostic project holds `IPicoScope`, the model types, `PicoScopeFinder` and
`SimulatedPicoScope`; a Windows-only project holds the driver interop; and a `Shared/` folder is
linked as source into both heads. Running it against real hardware is Windows-only, needs a 64-bit
process and the vendor's `ps2000.dll` resolvable at run time from a separate install, and needs the
vendor's own desktop application closed first, because only one process may hold the device open.
Without hardware, `PicoScopeFinder` falls back to `SimulatedPicoScope`, so the sample still runs.

The reference application in [CodeBrix.Samples](https://github.com/ellisnet/CodeBrix.Samples) is
[PicoScope.Brix](https://github.com/ellisnet/CodeBrix.Samples/tree/main/PicoScope.Brix), a six-head CodeBrix.Platform oscilloscope
front end that hosts a live `PlotModel` through the
[PlotterView add-in](../platform/add-ins/PlotterView.md), streams into it from the driver's own
polling thread, and drives the `ps2000` driver on Linux as well as Windows.

The example library is test-support code: it is not part of the library or the NuGet package, and it
is organized as `Series/`, `Axes/`, `Annotations/`, `Showcases/`, `CustomSeries/`, `Misc/`, `Issues/`
and `Discussions/`. The repository holds no build tools, scripts or generators.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Plotter/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Plotter/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Plotter/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.Plotter.Tests](https://github.com/ellisnet/CodeBrix.Plotter/tree/main/tests/CodeBrix.Plotter.Tests) |
| Samples | [samples/PicoScope](https://github.com/ellisnet/CodeBrix.Plotter/tree/main/samples/PicoScope) |

The test project is organized by area - `ExampleLibrary/`, `Axes/`, `Skia/`, `PlotController/`,
`PlotModel/`, `Graphics/`, `Foundation/`, `Imaging/`, `Svg/`, `Pdf/`, `Utilities/` and `Rendering/` -
and the AGENT-README points at those folders as the canonical usage examples for each feature area.

## License

CodeBrix.Plotter is licensed under the MIT License; the license is also named in the package ID
(`CodeBrix.Plotter.MitLicenseForever`). For the provenance and licensing of open source code included
in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Plotter/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [PlotterView add-in](../platform/add-ins/PlotterView.md) - the CodeBrix.Platform add-in that puts a plot in an application window
- [Graphics, media and vision](../platform/09-graphics-media-and-vision.md) - where charts sit among the drawing and media surfaces of a CodeBrix.Platform application
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.Plotter on GitHub](https://github.com/ellisnet/CodeBrix.Plotter) - source, tests and samples
