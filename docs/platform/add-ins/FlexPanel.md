<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Add-ins](../08-add-ins.md) › FlexPanel</sub>

# FlexPanel

**The FlexPanel add-in adds a CSS flexbox-style XAML layout panel, `FlexPanel : Panel`, to your application.** Children are arranged in optionally wrapping rows or columns with the familiar flexbox model - `Direction`, `JustifyContent`, `AlignItems`, `Wrap` and `AlignContent` - plus the per-child attached properties `Grow`, `Shrink`, `Basis`, `Order` and `AlignSelf`. It is pure managed layout with no native dependency, so it is live on all six heads.

| | |
| --- | --- |
| **Package** | [`CodeBrix.Platform.FlexPanel.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.FlexPanel.ApacheLicenseForever) |
| **Adds** | The `FlexPanel` element, the `FlexBasis` struct, and the enums `FlexDirection`, `FlexJustify`, `FlexAlignItems`, `FlexAlignSelf`, `FlexAlignContent`, `FlexWrap` and `FlexPosition` |
| **Heads** | All six - Windows Win32-Skia, Windows WPF-Skia, Linux X11, Linux Wayland, Linux frame buffer and macOS |
| **Requires** | Nothing. The panel uses only public framework API and no native code |

## Add it to your application

Reference the package from the project that carries your framework package references - the application's `.Core` project in the standard layout. The XAML in the shared `.UI` project then resolves the `flex:` namespace.

```bash
dotnet add package CodeBrix.Platform.FlexPanel.ApacheLicenseForever
```

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>MyApp</RootNamespace>
    <DefineConstants>$(DefineConstants);HAS_CODEBRIX;HAS_CODEBRIX_WINUI</DefineConstants>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.Platform.ApacheLicenseForever" />
    <PackageReference Include="CodeBrix.Platform.FlexPanel.ApacheLicenseForever" />
  </ItemGroup>
</Project>
```

`CodeBrix.Platform.ApacheLicenseForever` is the only dependency, and it flows in automatically, supplying `Panel`, `DependencyProperty` and the measure and arrange passes.

Declare the XAML namespace, or the C# using when you build panels in code:

```xml
xmlns:flex="using:CodeBrix.Platform.UI.FlexPanel"
```

```csharp
using CodeBrix.Platform.UI.FlexPanel;
```

There is no registration call, no feature flag, no head-project change, and no code-behind. A complete page is this:

```xml
<Page x:Class="MyApp.MainPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:flex="using:CodeBrix.Platform.UI.FlexPanel">
  <flex:FlexPanel Direction="Row" Wrap="Wrap" JustifyContent="SpaceEvenly" Padding="16">
    <Border Width="120" Height="80" Margin="8" Background="CornflowerBlue" />
    <Border Width="120" Height="80" Margin="8" Background="Coral" flex:FlexPanel.Order="-1" />
    <Border Height="80" Margin="8" Background="SeaGreen" flex:FlexPanel.Grow="1" />
  </flex:FlexPanel>
</Page>
```

## Using it

### The two axes

`Direction` picks the main axis: `Row` makes it x, `Column` makes it y. `JustifyContent`, `Grow`, `Shrink`, `Basis` and `Order` act on the main axis; `AlignItems`, `AlignSelf` and `AlignContent` act on the cross axis. Everything else follows from that one distinction.

The panel-level properties, with their defaults, are `Direction` (`Row`), `JustifyContent` (`Start`), `AlignItems` (`Stretch`), `AlignContent` (`Stretch`), `Wrap` (`NoWrap`), `Position` (`Relative`) and `Padding` (0). Each has a matching static `<Name>Property` field, and changing any of them invalidates the panel's measure.

| Enum | Values |
| --- | --- |
| `FlexDirection` | `Row`, `RowReverse`, `Column`, `ColumnReverse` - the reverse values run the same axis backwards |
| `FlexJustify` | `Start`, `Center`, `End`, `SpaceBetween`, `SpaceAround`, `SpaceEvenly` |
| `FlexAlignItems` | `Stretch`, `Center`, `Start`, `End` |
| `FlexAlignSelf` | `Auto` (use the panel's value), `Stretch`, `Center`, `Start`, `End` |
| `FlexWrap` | `NoWrap`, `Wrap`, `Reverse` |
| `FlexAlignContent` | `Stretch`, `Center`, `Start`, `End`, `SpaceBetween`, `SpaceAround`, `SpaceEvenly` |
| `FlexPosition` | `Relative`, `Absolute` |

`Start`, `Center` and `End` pack the children on the main axis; `SpaceBetween` puts the first child at the start and the last at the end; `SpaceAround` gives the first and last a half-size space; `SpaceEvenly` gives equal space everywhere. On the cross axis, `Stretch` makes children without an explicit cross-axis size fill the line.

### Wrapping

`NoWrap` keeps every child on one line, where they shrink to fit according to their `Shrink`. `Wrap` starts new lines as needed, and `Reverse` wraps with the lines stacked in reverse order. `AlignContent` then decides how the lines are distributed on the cross axis, and is ignored while `Wrap` is `NoWrap`.

A wrapping tag cloud is the shortest useful example.

```xml
<flex:FlexPanel Direction="Row" Wrap="Wrap" JustifyContent="Center"
                AlignContent="Start" Padding="8">
  <Border Margin="4" Padding="8,4" CornerRadius="12" Background="LightGray">
    <TextBlock Text="alpha" />
  </Border>
  <Border Margin="4" Padding="8,4" CornerRadius="12" Background="LightGray">
    <TextBlock Text="beta" />
  </Border>
  <!-- ...as many as you like; they wrap onto new lines -->
</flex:FlexPanel>
```

Spacing between children is each child's `Margin`, which the engine honors exactly as CSS margins: two adjacent children with `Margin="4"` are 8 pixels apart. There is no gap property.

### Order, Grow and Shrink

The per-child attached properties are `Order` (int, default 0), `Grow` (float, default 0), `Shrink` (float, default 1), `Basis` (`FlexBasis`, default `Auto`) and `AlignSelf` (`FlexAlignSelf`, default `Auto`). In XAML they take the form `flex:FlexPanel.Xxx="..."`; in C# each has a static `Get`/`Set` pair and an attached `DependencyProperty` field. Changing one re-runs the owning panel's layout.

- `Order` - children are arranged by ascending order, and insertion order breaks ties. Negative values are fine: `Order="-1"` moves a child first.
- `Grow` - the child's share of free main-axis space. Two children with `Grow="1"` split it equally, `Grow="2"` takes twice as much as `Grow="1"`, and 0 never grows.
- `Shrink` - the child's share of overflow reclaimed when children exceed the main axis on one line. 0 means never shrink below the basis.

A navigation bar is the classic use: logo at the left, actions at the right, a growing spacer between them.

```xml
<flex:FlexPanel Direction="Row" AlignItems="Center" Height="48" Padding="8,0">
  <Image Source="ms-appx:///Assets/logo.png" Height="32" />
  <Border flex:FlexPanel.Grow="1" />                 <!-- takes all free space -->
  <Button Content="Open"   flex:FlexPanel.Shrink="0" />
  <Button Content="Save"   flex:FlexPanel.Shrink="0" />
  <Button Content="Help"   flex:FlexPanel.Order="1"
          flex:FlexPanel.AlignSelf="End" />         <!-- last, bottom-aligned -->
</flex:FlexPanel>
```

### FlexBasis

`FlexBasis` is a struct implementing `IEquatable<FlexBasis>`, with an implicit conversion from `float` and a string converter that both the XAML source generator and the runtime XAML reader honor.

```csharp
static readonly FlexBasis Auto        // the default (default(FlexBasis) == Auto)
FlexBasis(float length, bool isRelative = false)
float Length { get; }    // pixels, or a fraction in [0, 1] when IsRelative; 0 when Auto
bool  IsAuto { get; }
bool  IsRelative { get; }
static implicit operator FlexBasis(float length)   // absolute
static FlexBasis CreateFromString(string value)    // the XAML converter
bool Equals(FlexBasis other); == and !=; ToString()
```

There are three kinds of basis: `FlexBasis.Auto`, the child's measured main-axis size; an absolute length in device-independent pixels, written `new FlexBasis(150)` or `(FlexBasis)150f`; and a fraction of the panel's main axis, written `new FlexBasis(0.25f, isRelative: true)`.

In markup and in a `Style` setter the string forms are `"Auto"` (case-insensitive), an invariant-culture number such as `"150"` or `"150.5"`, and a percentage such as `"25%"`. `ToString()` round-trips as `"Auto"`, `"150"` or `"30%"`. A length must not be negative, and a relative length must be within `[0, 1]`; both violations throw `ArgumentException`, and a string that is none of the three forms throws `FormatException`.

Equal columns that fill the width, with one of them fixed, is the recipe worth memorizing.

```xml
<flex:FlexPanel Direction="Row" AlignItems="Stretch">
  <Border flex:FlexPanel.Basis="0" flex:FlexPanel.Grow="1" Background="#20FF0000" />
  <Border flex:FlexPanel.Basis="0" flex:FlexPanel.Grow="1" Background="#2000FF00" />
  <Border flex:FlexPanel.Basis="200" flex:FlexPanel.Shrink="0" Background="#200000FF" />
  <Border flex:FlexPanel.Basis="25%" Background="#20FFFF00" />
</flex:FlexPanel>
```

`new FlexBasis(0)` is an absolute zero basis, distinct from `Auto`: a `Grow` child with basis 0 shares free space from nothing, which is what makes the columns equal.

### From code

Everything declarable in markup is settable in code, through the panel's own properties and the static accessors `GetOrder`/`SetOrder`, `GetGrow`/`SetGrow`, `GetShrink`/`SetShrink`, `GetBasis`/`SetBasis` and `GetAlignSelf`/`SetAlignSelf`, all taking a `DependencyObject`.

```csharp
using CodeBrix.Platform.UI.FlexPanel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

var panel = new FlexPanel
{
    Direction = FlexDirection.Row,
    Wrap = FlexWrap.Wrap,
    JustifyContent = FlexJustify.SpaceBetween,
    AlignItems = FlexAlignItems.Center,
    AlignContent = FlexAlignContent.Start,
    Padding = new Thickness(8),
};

var logo   = new Border { Height = 32 };
var spacer = new Border();
var menu   = new Border { Height = 32 };

FlexPanel.SetOrder(logo, -1);                                     // first
FlexPanel.SetBasis(logo, 120f);                                   // implicit float -> absolute
FlexPanel.SetGrow(spacer, 1f);                                    // absorbs free space
FlexPanel.SetShrink(menu, 0f);                                    // never squeezed
FlexPanel.SetBasis(menu, new FlexBasis(0.25f, isRelative: true)); // "25%"
FlexPanel.SetAlignSelf(menu, FlexAlignSelf.End);

panel.Children.Add(logo);
panel.Children.Add(spacer);
panel.Children.Add(menu);

// Later - any change re-lays-out the owning panel:
panel.Direction = FlexDirection.Column;
FlexPanel.SetBasis(menu, FlexBasis.CreateFromString("40%"));
FlexPanel.SetBasis(logo, FlexBasis.Auto);
```

Reading a child's settings back is the mirror image.

```csharp
var basis = FlexPanel.GetBasis(menu);   // using System.Diagnostics; for Debug
if (basis.IsRelative)  Debug.WriteLine($"{basis.Length * 100}% of the main axis");
else if (basis.IsAuto) Debug.WriteLine("measured size");
else                   Debug.WriteLine($"{basis.Length}px");   // basis.ToString() gives "150"
```

Flipping `Direction` from a `SizeChanged` handler is how a page reflows between landscape and portrait, and it is one line of layout plumbing rather than application logic.

### What the panel reads from each child

Only four things: an explicitly set `Width` or `Height`, the `Margin`, the `Visibility`, and the `DesiredSize` from the measure pass. A collapsed child takes no space and no line position at all. Child `MinWidth`, `MaxWidth`, `MinHeight` and `MaxHeight` are not inputs to the flex computation, although they still apply when the framework arranges the child inside the slot the engine gave it, which can leave the slot partly empty.

The flex item tree is rebuilt on every layout pass from the current property values, so there is no state to reset when children are added, removed or reordered.

### Sizing and unconstrained dimensions

A child with an explicit `Width` or `Height` cannot be stretched or grown past it: the framework clamps the child inside its grown layout slot, exactly as a `Grid` does. Leave the main-axis dimension unset on children that should grow, and the cross-axis dimension unset on children that should stretch.

When the panel itself is measured with infinite width or height - inside a `StackPanel` along its orientation, or a `ScrollViewer` - it measures to its natural size for that pass, treating every child as `Shrink=0` and `AlignSelf=Start`. `Shrink` and cross-axis stretching therefore only do anything when the panel has a bounded size on that axis, so give it one: a `Grid` cell, or an explicit `Width` or `Height`.

### What the panel does not do

- No gap, row-gap or column-gap property: spacing between children is each child's `Margin`.
- No min-content, max-content or fit-content sizing keywords. A basis is `Auto`, an absolute length, or a percentage.
- No per-child absolute positioning: there are no `Left`/`Top`/`Right`/`Bottom` attached properties, and the panel-level `Position` property has no effect. Position an overlay with a `Canvas` or a `Grid` instead.
- No right-to-left flow: the panel does not consult `FlowDirection`. `Row` always runs left to right; use `RowReverse` for the mirrored order.
- No baseline alignment: `AlignItems` and `AlignSelf` offer `Stretch`, `Center`, `Start` and `End` only.
- No nested-layout awareness beyond ordinary measure and arrange. Nesting works - a `FlexPanel` inside a `FlexPanel` is only a child with a `DesiredSize` - but the inner panel does not share lines with the outer one.
- No animation or transition of layout changes.

## Per-head notes

None. The panel is pure managed layout with no native dependency and behaves identically on all six heads.

## Pitfalls

- A child with an explicit `Width` or `Height` cannot be stretched or grown past it. Leave the dimension unset on the axis where the child should flex.
- Basis percentages are of the panel's main axis, not of the remaining space, and the fraction must be within `[0, 1]`: `"150%"` throws `ArgumentException` when the value is created.
- `AlignContent` only matters when `Wrap` is `Wrap` or `Reverse`; on a single line it is silently ignored. Use `AlignItems` for that.
- `Position="Absolute"` is accepted and has no effect. There are no `Left`/`Top`/`Right`/`Bottom` attached properties.
- Negative `Grow` or `Shrink` throws `ArgumentException` from the property setter, including when a binding delivers the value.
- Inside a vertical `StackPanel` or a `ScrollViewer`, a `Row` panel's cross axis - its height - is unconstrained, so `AlignItems="Stretch"` does nothing visible: the line is exactly as tall as its tallest child.
- `Order` sorts within the whole panel, not within a line; a wrapped line is filled in order sequence.
- Margins count on both axes: a margin on the cross axis moves the child inside its line.
- The enum names carry a `Flex` prefix - `FlexDirection.Row`, `FlexWrap.Wrap`, `FlexJustify.SpaceBetween` - and CSS keyword strings such as `"space-between"` are not accepted anywhere. In XAML the plain member names are used: `"Row"`, `"Wrap"`, `"SpaceBetween"`.
- `new FlexBasis(0)` is an absolute zero basis, distinct from `Auto`.
- Every attached-property change on a child invalidates the owning panel's measure. When configuring many children in code, set their `Order`, `Grow` and `Basis` before adding them to `Children`, so the panel lays out once.
- The item tree is rebuilt on every measure and arrange pass, and each child is measured once per measure pass. That is cheap for the dozens of children a toolbar, tag cloud or card row holds; for thousands of items use a virtualizing `ItemsControl` and put a `FlexPanel` inside each item, not around them all.
- Prefer `Basis="0"` plus `Grow` for equal columns over percentages that must be kept in sum. The engine distributes free space in one pass either way, but the `Grow` form needs no arithmetic when a column is added.

## Related

- [Views and styling](../06-views-and-styling.md) - where `FlexPanel` sits among the framework's own panels
- [FlexPanelDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/FlexPanelDemo) in CodeBrix.Platform - a live playground on six heads: the panel properties as drop-downs, attached properties on individual children, a nested panel, a collapsible child, and two worked examples
- [PolyHavenBrowser](https://github.com/ellisnet/CodeBrix.Samples/tree/main/PolyHavenBrowser) in [the sample applications](../../samples/README.md) - a header row that wraps, and a main axis flipped from a `SizeChanged` handler so a 3D viewer drops below the information pane in portrait
- [GitHubIssueFinder](https://github.com/ellisnet/CodeBrix.Samples/tree/main/GitHubIssueFinder) in [the sample applications](../../samples/README.md) - a header bar and a search row that wrap on a narrow window, and label pills that flow after a row title
- KenneyAssetBrowser, NotionDocumentCreator and WikipediaPublisher in [the sample applications](../../samples/README.md) - wrapping headers and bottom bars that reflow on the Skia heads

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.FlexPanel/AGENT-README.txt) |
| Add-in source | [src/AddIns/Platform.UI.FlexPanel](https://github.com/ellisnet/CodeBrix.Platform/tree/main/src/AddIns/Platform.UI.FlexPanel) |
| Tests | [src/AddIns/Platform.UI.FlexPanel.Tests](https://github.com/ellisnet/CodeBrix.Platform/tree/main/src/AddIns/Platform.UI.FlexPanel.Tests) |
| Reference application | [samples/CodeBrixPlatform/FlexPanelDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/FlexPanelDemo) |
| Package | [`CodeBrix.Platform.FlexPanel.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.FlexPanel.ApacheLicenseForever) |

---

**Where to go next**

- [Views and styling](../06-views-and-styling.md) - layout, resources and themes across the whole application
- [TextLayout](TextLayout.md) - measuring text precisely, which is what a reflowing layout usually needs next
- [All add-ins](../08-add-ins.md) - the whole set at a glance
