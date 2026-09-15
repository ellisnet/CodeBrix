<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Add-ins](../08-add-ins.md) › CommandBar</sub>

# CommandBar

**The CommandBar add-in adds a desktop tool bar family of XAML elements to your application: a tray of bars, the bars themselves, groups, separators and spacers, and three kinds of button bound to view-model commands.** Small icon buttons with composed tooltips, an overflow chevron for what does not fit, and any control at all sitting inline as an item. Bar-level presentation - icon size, whether items show icon, text or both, where the text sits, whether tooltips are shown - comes from four inherited attached properties, so a bar states them once and any single item can override them. Icons are `ToolIconSource` objects in SVG or raster form, and because that base derives from the framework's `IconSource` the same artwork works anywhere the framework takes an icon source.

| | |
| --- | --- |
| **Package** | [`CodeBrix.Platform.CommandBar.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.CommandBar.ApacheLicenseForever) |
| **Adds** | `ToolBarTray`, `ToolBar`, `ToolBarGroup`, `ToolBarSeparator`, `ToolBarSpacer`, `ToolButton`, `ToolToggleButton` and `ToolDropDownButton`; the `ToolBarProperties` attached properties; the `ToolIconSource` family - `SvgIconSource`, `RasterIconSource`, `GlyphIconSource`, `SvgIcon`, `RasterIcon` - with markup extensions, `IconResourceScheme` and `IconRasterCache`; `ToolTipComposer`; the enums `LabelMode`, `LabelPosition`, `OverflowMode`, `PopupMode` and `IconTintMode`; and an automation peer per element |
| **Heads** | All six - Windows Win32-Skia, Windows WPF-Skia, Linux X11, Linux Wayland, Linux frame buffer and macOS |
| **Requires** | [`CodeBrix.Platform.Svg.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Svg.ApacheLicenseForever), the [Svg add-in](Svg.md) - a hard dependency, because SVG icons are not optional. It arrives with the package, as the core framework does. Nothing else: no OS package and no native prerequisite |

## Add it to your application

Reference the package from the project that carries your framework package references - the application's `.Core` project in the standard layout. The XAML in the shared `.UI` project then resolves the `cb:` namespace.

```bash
dotnet add package CodeBrix.Platform.CommandBar.ApacheLicenseForever
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
    <!-- Brings CodeBrix.Platform.Svg.ApacheLicenseForever with it: a hard
         dependency, because SVG icons are not optional. -->
    <PackageReference Include="CodeBrix.Platform.CommandBar.ApacheLicenseForever" />
  </ItemGroup>
</Project>
```

Declare the XAML namespace, or the C# using when you build bars in code:

```xml
xmlns:cb="using:CodeBrix.Platform.UI.CommandBar"
```

```csharp
using CodeBrix.Platform.UI.CommandBar;
```

There is no registration call, no feature flag and no head-project change. What the application supplies is a view model that owns the commands and markup that says what is on each bar and in what order.

## Using it

### The tray and the bar

`ToolBarTray` is a `Panel` that holds several `ToolBar`s in one row and wraps to the next when the row runs out. `Orientation` (default `Horizontal`) is the axis the bars run along: a bar that did not state an orientation of its own is turned to match, and stays matched if the tray changes later. `ToolBarSpacing` (default 8) is the gap between two bars.

`ToolBar` is an `ItemsControl`. Its items are plain UI elements - this package's buttons, groups, separators and spacers, or any control at all, a `ComboBox`, a `TextBox`, a `TextBlock`, hosted inline and centered across the bar. A non-element item is wrapped in a `ContentPresenter`, so `ItemTemplate` and `ItemTemplateSelector` work for data items.

| Property | Default | What it does |
| --- | --- | --- |
| `Orientation` | `Horizontal` | Sets the matching orientation on every `ToolBarGroup` and `ToolBarSeparator` it hosts, so those never state it themselves |
| `Title` | `""` | The bar's accessibility name, the accessibility name of the overflow flyout, and the tail of each button's accessible name. It is not shown in the visible tooltip |
| `ItemSpacing` | `4` | The gap between two items. Halved while `IsCompact` is true |
| `IsCompact` | `false` | Density: tightens the spacing and the bar's own padding |
| `OverflowMode` | `Chevron` | What happens to items that do not fit |
| `SeparatorBetweenGroups` | `true` | Puts a separator between two adjacent groups |
| `HasOverflowItems` | read-only | True while the chevron is shown |
| `OverflowItems` | read-only | The items behind the chevron, in order - the same instances the bar holds, not copies |

`ShowOverflow()` opens the overflow flyout and answers false when there is nothing to show or the bar is not in a window yet. Two constants belong to a re-template: `DefaultItemSpacing` is 4, and `ItemsHostPartName` is `"PART_ItemsHost"`, the name the items panel must carry in a replacement template.

A bar's `Title` reaches a screen reader as the tail of every button on it, so "Save" on a bar titled "Main" is announced as `Save (Ctrl+S), Main`.

This is the shape a desktop application puts across the top of its window: a tray of two bars, one of file and engrave commands and one of view controls.

```xml
<Page x:Class="MyApp.MainPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:cb="using:CodeBrix.Platform.UI.CommandBar">
  <cb:ToolBarTray cb:ToolBarProperties.IconSize="24"
                  cb:ToolBarProperties.LabelMode="IconOnly">

    <cb:ToolBar x:Name="MainBar" Title="Main" OverflowMode="Chevron">
      <cb:ToolBarGroup>
        <cb:ToolButton Command="{Binding NewCommand}" />
        <cb:ToolDropDownButton Command="{Binding OpenCommand}"
                               PopupMode="MenuButton"
                               Flyout="{StaticResource RecentFilesFlyout}" />
        <cb:ToolButton Command="{Binding SaveCommand}" />
      </cb:ToolBarGroup>

      <!-- Two adjacent groups get a separator between them automatically. -->
      <cb:ToolBarGroup>
        <cb:ToolDropDownButton Text="Engrave" PopupMode="MenuButton"
                               Command="{Binding EngraveCommand}"
                               Icon="{cb:SvgIconSource
                                      Source=Icons/engrave.svg,
                                      Dark=Icons/engrave.dark.svg}">
          <cb:ToolDropDownButton.Flyout>
            <MenuFlyout>
              <MenuFlyoutItem Text="Preview"
                              Command="{Binding EngraveModeCommand}"
                              CommandParameter="Preview" />
              <MenuFlyoutItem Text="Publish"
                              Command="{Binding EngraveModeCommand}"
                              CommandParameter="Publish" />
            </MenuFlyout>
          </cb:ToolDropDownButton.Flyout>
        </cb:ToolDropDownButton>
        <cb:ToolButton Text="Print" Shortcut="Ctrl+P"
                       Command="{Binding PrintCommand}"
                       Icon="{cb:SvgIconSource
                              Source=Icons/print.svg,
                              Dark=Icons/print.dark.svg}" />
      </cb:ToolBarGroup>

      <cb:ToolBarSeparator />

      <!-- Any control at all can be an item. -->
      <ComboBox ItemsSource="{Binding Scores}"
                SelectedItem="{Binding SelectedScore, Mode=TwoWay}"
                MinWidth="150" VerticalAlignment="Center" />

      <!-- Everything after a filling spacer is pushed to the far end. -->
      <cb:ToolBarSpacer Fill="True" />

      <cb:ToolToggleButton Text="Magnifier"
                           IsChecked="{Binding Magnifier, Mode=TwoWay}"
                           Icon="{cb:RasterIconSource
                                  Source=Icons/magnifier.png,
                                  Tint={ThemeResource TextFillColorPrimaryBrush}}" />
    </cb:ToolBar>

    <cb:ToolBar Title="Music">
      <ComboBox IsEditable="True" MinWidth="90" VerticalAlignment="Center"
                ItemsSource="{Binding ZoomLevels}" />
      <cb:ToolBarSeparator />
      <cb:ToolButton Text="Previous page" Command="{Binding PreviousPageCommand}" />
      <TextBlock Text="{Binding PageLabel}" VerticalAlignment="Center" Margin="6,0" />
      <cb:ToolButton Text="Next page" Command="{Binding NextPageCommand}" />
    </cb:ToolBar>
  </cb:ToolBarTray>
</Page>
```

Notice how little each button says. The first three name a command and nothing else, because the command carries the label, the icon, the description and the shortcut; the two that do state `Text` and `Icon` are overriding what the command would have supplied.

### The button family

`ToolButton` derives from `ButtonBase` and is the base of the other two.

| Member | Default | What it does |
| --- | --- | --- |
| `Icon` | `null` | A `ToolIconSource`. With none set and a `XamlUICommand` bound, the command's `IconSource` is shown instead - including the framework's own symbol, font and path icon sources |
| `Text` | `null` | The label. Used even when it is not drawn: an icon-only button puts it in the tooltip and reads it to a screen reader |
| `Shortcut` | `null` | Shortcut text for the tooltip. Set it only to override what is worked out from a keyboard accelerator registered on the button or carried by a bound `XamlUICommand` |
| `ShowToolTip` | `null` | Null means whatever the bar says. False silences this button in a bar that shows tooltips, true brings it back in a bar that does not. Silencing the tooltip does not change what a screen reader reads |
| `ComposedToolTipText` | read-only | What the tooltip would say, whether or not tooltips are on - useful for a status line |
| `AccessibleName` | read-only | What a screen reader announces, including the bar's `Title` |

`ClickWithModifiers` is raised right after the ordinary `Click`, for every route that clicks the button - pointer, keyboard and automation - carrying the modifier keys held at the click, read then rather than remembered from the last key event. Its `ClickWithModifiersEventArgs` exposes `Modifiers`, `IsShiftPressed`, `IsControlPressed` and `IsAltPressed`.

A re-template reads six more properties the button sets for it: `ResolvedText`, `IconVisual`, `EffectiveIconSize`, `IconVisibility`, `TextVisibility` and `LabelOrientation`.

`ToolToggleButton` adds `IsChecked` (default false) and `IsCheckedChanged`. Any click flips it. Bind it two-way to the view model; this XAML dialect has no `BindsTwoWayByDefault`, so write `Mode=TwoWay` yourself.

`ToolDropDownButton` adds a flyout. `Flyout` takes any `FlyoutBase`, and a `MenuFlyout` is the usual choice - the same instance may be shared with a menu bar or another button, because it is a reference and not a copy. `PopupMode` decides how the button splits its work:

- `MenuButton` (the default) - the main part runs `Command`, a separate arrow part opens the flyout. A Save button with Save-as behind its arrow.
- `Instant` - the whole button opens the flyout; there is no command.
- `Delayed` - a press runs `Command` on release; a press held for `PressAndHoldDelay` opens the flyout instead, and the release that follows then does nothing.

`PressAndHoldDelay` defaults to 600 ms. `ArrowVisibility` is read-only and collapsed in `Delayed` mode, `IsFlyoutOpen` is read-only, and `OpenFlyout()` and `CloseFlyout()` drive it from code. `FlyoutClosed` is raised after the flyout closed and its items' command bindings were re-hooked, so the items are usable again by the time you see it. A menu opens on the press, as every desktop menu does; the release that follows does nothing.

Sharing one menu between a bar button and something else is a matter of putting it in a resource dictionary.

```xml
<Page.Resources>
  <MenuFlyout x:Key="RecentFilesFlyout">
    <MenuFlyoutItem Text="bach-invention.ly"
                    Command="{Binding OpenRecentCommand}"
                    CommandParameter="bach-invention.ly" />
  </MenuFlyout>
</Page.Resources>

<cb:ToolDropDownButton Text="Open" PopupMode="MenuButton"
                       Command="{Binding OpenCommand}"
                       Flyout="{StaticResource RecentFilesFlyout}" />
```

The flyout is a reference: the same instance can be the drop-down's menu and a menu bar's sub-menu at once.

### Groups, separators and spacers

`ToolBarGroup` is a run of items that belong together, with its own tighter spacing: `Spacing` defaults to 4 and `Orientation` is set by the bar. The bar treats a group as one item, so a group overflows whole.

`ToolBarSeparator` is the divider between two runs, vertical in a horizontal bar and the other way round. `Orientation` is set by the bar. `Thickness` defaults to 1 and is measured in device pixels; the read-only `LogicalThickness` is that thickness divided by the display scale. The line is one device pixel at any scale and its offset is snapped to the device grid, so it never becomes a blurry pixel-and-a-quarter. With `SeparatorBetweenGroups` left at true the bar draws one between two adjacent groups by itself, and a separator you wrote yourself between them is respected, so this never doubles up.

`ToolBarSpacer` is empty space. Give it a `Width`, or a `Height` in a vertical bar, for a fixed gap, or set `Fill` to true and it takes everything left over, which pushes what follows to the far end. Two filling spacers share what is left equally, which gives a left, center and right bar.

### Overflow

`OverflowMode` decides what a bar does with the items that do not fit.

- `None` - items that do not fit are clipped.
- `Wrap` - the bar's panel continues on a further line.
- `Chevron` (the default) - the trailing items that do not fit move, in order, into a flyout behind a chevron button, and move back when the space returns.

The items behind the chevron are the same element instances throughout, so bindings, event handlers, toggle state and focus survive the move. Nothing has to be written to get this; to read or drive it:

```csharp
if (MainBar.HasOverflowItems)
{
    foreach (var item in MainBar.OverflowItems) { /* the same instances */ }
    MainBar.ShowOverflow();
}
```

The overflow flyout's panel is the flyout's content, not a child of the bar, so nothing about the bar reaches it by inheritance. The bar copies its four presentation settings across and re-hooks the command bindings of the buttons that move there. Walking up from a button while it is in the overflow will not find the bar; use `OverflowItems` to go the other way.

The partition is strictly the trailing items that do not fit, in order: there is no per-item overflow priority. A `Collapsed` item takes no space, and is skipped both by the partition and by keyboard navigation.

`ToolBarPanel` and `ToolBarOverflowButton` are public because the bar's template is public - an application that re-templates `ToolBar` has to be able to name `PART_ItemsHost`. Neither is meant to be used directly otherwise, and `ToolBar.ItemsPanel` is not used at all: the bar owns its items panel, because overflow means moving the same elements between two panels and an items presenter's own child management would undo that.

### Icons

`ToolIconSource` is the abstract base of every icon this package understands, and it derives from the framework's `IconSource`. Three sources and two elements come from it.

| Type | Properties |
| --- | --- |
| `SvgIconSource` | `Source`, `Dark`, `Markup`, `Tint`, `TintMode`, `Size` |
| `RasterIconSource` | `Source`, `Dark`, `Tint`, `Size` |
| `GlyphIconSource` | `Glyph`, `FontFamily`, `Size` - a symbol-font glyph, for an application that already ships an icon font |
| `SvgIcon`, `RasterIcon` | `UriSource`, `DarkUriSource`, `Markup` on the SVG one, `Tint`, `TintMode` on the SVG one, `Size`, plus the read-only `ResolvedUriSource` and `EffectiveIconSize`, and `UpdateIcon()` |

`Source` is the artwork, and the light artwork when `Dark` is set; `Dark` is the alternate the theme switches to. `Markup` takes an SVG document written inline instead of a `Source`. `Tint` recolors the artwork and must be a `SolidColorBrush`. `Size` overrides the inherited `IconSize` for that one icon. PNG is the raster format to reach for; JPEG, BMP, GIF, WebP and ICO come free through the platform's image decoder, because there is no format-specific code in this package.

The two element types carry the same artwork under different names - `UriSource` and `DarkUriSource` - because `ImageIcon`, which `SvgIcon` derives from, already owns the name `Source`. `SvgIcon` is an `ImageIcon` and `RasterIcon` is an `IconSourceElement`.

Markup extensions keep the common case terse. They are named after what they return, a source, which is what leaves `<cb:SvgIcon />` and `<cb:RasterIcon />` free to be the elements. A relative path is read as `ms-appx:///`, and an absolute URI is taken as written; use the full object syntax when a value has to be bound.

```xml
<!-- A light/dark pair. The dark artwork replaces the light one whenever the
     element's ActualTheme is dark, and swaps back live on a theme change. -->
<cb:ToolButton Text="New"
               Icon="{cb:SvgIconSource Source=Icons/new.svg,
                                       Dark=Icons/new.dark.svg}" />

<!-- One file, any color: artwork drawn in currentColor takes the tint. -->
<cb:ToolButton Text="Next page"
               Icon="{cb:SvgIconSource Source=Icons/chevron.svg,
                                       Tint={ThemeResource AccentFillColorDefaultBrush}}" />

<!-- A PNG tinted through its ALPHA: the opaque pixels are painted with the
     tint, exactly as BitmapIcon.ShowAsMonochrome does. -->
<cb:ToolButton Text="Magnifier"
               Icon="{cb:RasterIconSource Source=Icons/magnifier.png,
                                          Tint=#FF3A6EA5}" />

<!-- A JPEG, drawn as it was written. A JPEG has no alpha, so a tint would
     paint the whole rectangle - leave it unset. -->
<cb:ToolButton Text="Score"
               Icon="{cb:RasterIconSource Source=Icons/score.jpg}" />

<!-- Artwork embedded in a class library rather than shipped beside the app. -->
<cb:ToolButton Text="Open"
               Icon="{cb:SvgIconSource Source=cb-res://MyCompany.Icons/open.svg}" />

<!-- The same icons as ELEMENTS, where the framework wants an IconElement. -->
<MenuFlyoutItem Text="Publish">
  <MenuFlyoutItem.Icon>
    <cb:SvgIcon UriSource="ms-appx:///Icons/publish.svg" Size="16" />
  </MenuFlyoutItem.Icon>
</MenuFlyoutItem>
```

In code, when a value has to be computed or bound, write the source object out:

```csharp
button.Icon = new SvgIconSource
{
    Markup = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'>"
           + "<circle cx='12' cy='12' r='9' fill='currentColor'/></svg>",
    Tint = new SolidColorBrush(Colors.SteelBlue),
};
```

`IconTintMode` decides how far a tint reaches. `CurrentColorOnly`, the default, recolors only artwork that asked for `currentColor`, and artwork that states a color keeps it. `ReplaceBlackAndWhite` additionally replaces hard-coded black and white fills and strokes; it does not reach a color written in an inline `style=` attribute, because an inline style outranks the stylesheet the tint is delivered as, nor an element that states no color at all and inherits the SVG default of black. `None` does no recoloring even with a `Tint` set.

`IconResourceScheme` reads artwork straight out of a library's embedded resources, so an icon set can ship inside a class library instead of beside the application. Its `Scheme` constant is `"cb-res"`, `Create(assemblyName, resourceName)` builds a URI such as `cb-res://MyLib/open.svg`, and `IsResourceUri`, `TryOpen` and `RegisterAssembly` are the rest of it. A resource is found by its exact manifest name, or by an unambiguous suffix, so `cb-res://MyLib/open.svg` finds `MyLib.Assets.Icons.open.svg`. A missing assembly or resource is a missing icon, never an exception.

`IconRasterCache` holds one rasterization per artwork, theme, size, display scale and tint, weakly, shared by every icon that wants the same thing; it exposes `Count` and `Clear()`. Scale variants are honored too: where `open.scale-125.png` sits beside `open.png`, the variant matching the display is used - the smallest one that is big enough, else the biggest there is - and a file named without a qualifier is the unscaled artwork.

Because `ToolIconSource` derives from the framework's `IconSource`, and `SvgIcon` and `RasterIcon` are `IconElement`s, the same artwork works on a `ToolButton` and on the framework's own `AppBarButton` - the element straight into the `Icon`, or the source through the framework's `IconSourceElement` wrapper.

```xml
<AppBarButton Label="Open">
    <AppBarButton.Icon>
        <cb:SvgIcon UriSource="ms-appx:///Icons/open.svg" Size="20" />
    </AppBarButton.Icon>
</AppBarButton>

<AppBarButton Label="Save">
    <AppBarButton.Icon>
        <IconSourceElement>
            <cb:SvgIconSource Source="ms-appx:///Icons/save.svg" Size="20" />
        </IconSourceElement>
    </AppBarButton.Icon>
</AppBarButton>
```

The core framework package never depends on the SVG renderer, so an SVG icon on an `AppBarButton` works only in an application that references the [Svg add-in](Svg.md) or this package, which brings it. Font, symbol, path and PNG icons on an `AppBarButton` need neither.

### Command binding

`Command` may be any `ICommand`. `IsEnabled` follows `CanExecute(CommandParameter)` and is re-evaluated on `CanExecuteChanged` and when `CommandParameter` changes. An explicit `IsEnabled="False"` always wins.

A `XamlUICommand` or `StandardUICommand` supplies more, but only where the button did not state its own: `Label` becomes `Text`, `IconSource` becomes `Icon`, `Description` becomes the tooltip's second line, `KeyboardAccelerators` become the shortcut text and accelerators registered on the button itself, so the shortcut works while the bar is in the tree, and `AccessKey` becomes `AccessKey`. The button wins, then the command.

```csharp
var open = new XamlUICommand
{
    Label = "Open",
    Description = "Open a score from disk",
    IconSource = new SvgIconSource
    {
        Source = new Uri("ms-appx:///Icons/open.svg"),
        Dark = new Uri("ms-appx:///Icons/open.dark.svg"),
    },
};
open.KeyboardAccelerators.Add(new KeyboardAccelerator
{
    Key = VirtualKey.O,
    Modifiers = VirtualKeyModifiers.Control,
});
open.ExecuteRequested += (_, _) => OpenScore();
```

A button bound to that command states nothing at all:

```xml
<cb:ToolButton Command="{Binding OpenCommand}" />
```

and shows the icon, reads "Open" when its bar is showing text, offers the tooltip "Open (Ctrl+O)" with the description under it, and answers Ctrl+O while the bar is in the tree.

Checked state is the exception: it belongs to the view, so bind `ToolToggleButton.IsChecked` two-way to the view model. There is no checked state that comes from the command. `Visibility` is the ordinary property, and a collapsed item takes no space.

`ToolTipComposer` exposes the wording rules so an application can say the same thing elsewhere. `Compose(text, shortcutText, description)` produces "Save (Ctrl+S)", with the description on a second line when it says something the label does not; `ComposeAccessibleName(text, shortcutText, description, barTitle)` is the same wording with the bar's title appended, on one line; and `FormatShortcut` formats a keyboard accelerator the way the framework's own menus do - Ctrl, Alt, Windows, Shift, then the key.

### The four presentation settings

`ToolBarProperties` is a static class holding four inherited attached properties. They decide how every item in a bar presents itself, and because they are inherited, setting one on a tray, a bar, a group or the page reaches every item below it, while any single item can set its own and win.

| Property | Default | What it does |
| --- | --- | --- |
| `IconSize` | `24` | The icon's edge length in logical pixels. Artwork is rasterized at this size multiplied by the display's rasterization scale, so it is pixel-exact rather than drawn once and stretched |
| `LabelMode` | `IconOnly` | `IconOnly`, `TextOnly` or `IconAndText`. Switchable while the window is open: this is the "show button text" preference a desktop application offers |
| `LabelPosition` | `Right` | `Right`, beside the icon, or `Bottom`, under it |
| `ShowToolTips` | `true` | Whether items show their composed tooltip |

An icon-only button with no icon shows its text anyway, and a text-only button with no text shows its icon, so a mixed bar never contains blank squares.

In C# each has a `Get`/`Set` pair - `ToolBarProperties.GetIconSize(element)`, `ToolBarProperties.SetIconSize(element, 32)` - and in XAML they are written attached:

```xml
<cb:ToolBar cb:ToolBarProperties.IconSize="32"
            cb:ToolBarProperties.LabelMode="IconAndText">
```

`ToolBar` re-exposes all four as ordinary-looking properties, and those re-exposed properties **are** the attached properties rather than copies of them. Setting one is what the items inherit, and leaving one alone lets a value set on a tray or a page through untouched.

> [!IMPORTANT]
> Binding one of the four must be written in the attached form. The framework propagates an inherited attached property to a child by looking for a property with the same name on the child's type, so `LabelMode="IconAndText"` on a bar is fine, but a binding has to be written `cb:ToolBarProperties.LabelMode="{Binding ...}"`.

A "show button text" preference is the usual reason to bind one.

```xml
<cb:ToolBar Title="Main"
    cb:ToolBarProperties.LabelMode="{Binding Verbose,
                                     Converter={StaticResource LabelModeConverter}}">
```

Setting it in code is the same thing, and the target decides how far it reaches.

```csharp
ToolBarProperties.SetLabelMode(MainBar, LabelMode.IconAndText);   // one bar
ToolBarProperties.SetLabelMode(Tray, LabelMode.IconAndText);      // every bar
```

Silencing tooltips works the same way, with one button opting back in through its own singular `ShowToolTip`.

```xml
<cb:ToolBar Title="View" cb:ToolBarProperties.ShowToolTips="False">
  <cb:ToolButton Text="Zoom in"  Command="{Binding ZoomInCommand}" />
  <cb:ToolButton Text="Zoom out" Command="{Binding ZoomOutCommand}" />
  <!-- ... except this one. -->
  <cb:ToolButton Text="Fit page" Command="{Binding FitCommand}" ShowToolTip="True" />
</cb:ToolBar>
```

### Keyboard and automation peers

Tab moves into the bar and lands on the first item, and the next Tab leaves it, because the bar is not itself a tab stop. Inside it, Left and Right walk along a horizontal bar and Up and Down along a vertical one, Home and End go to the ends, and there is no wrap-around. Enter and Space invoke the focused button.

The drop-down key - Down in a horizontal bar, Right in a vertical one - opens the focused item's menu, whether that is a `ToolDropDownButton`'s own `Flyout` or an attached flyout on any other item. A group is walked through rather than being a stop of its own, separators and spacers are not focusable, an item that has moved into the overflow is off the bar's path, and the chevron is the last stop. Access keys, Alt plus a letter, work on these controls as on any other, through the framework's own access-key manager.

Every element has an automation peer: `ToolBarAutomationPeer` (ToolBar control type, named from `Title`), `ToolBarTrayAutomationPeer` and `ToolBarGroupAutomationPeer` (Group), `ToolBarSeparatorAutomationPeer` (Separator), `ToolButtonAutomationPeer` (Button, `IInvokeProvider`, composed name), `ToolToggleButtonAutomationPeer` (`IToggleProvider`) and `ToolDropDownButtonAutomationPeer` (SplitButton, `IExpandCollapseProvider`). The last one deliberately offers no Invoke pattern in `Instant` mode, because an `Instant` button has no command to invoke.

### What this add-in does not do

- It does not provide `CommandBar`, `AppBarButton`, `AppBarToggleButton` or `AppBarSeparator`. Those are framework types in `Microsoft.UI.Xaml.Controls`, and a page may hold both kinds - an app bar across the top and a `ToolBarTray` of these bars below it.
- It does not make SVG icons work on an `AppBarButton` by itself. That works because the application references the [Svg add-in](Svg.md), which this package brings.
- It is not a menu bar. Use the framework's `MenuBar` for the application menu; a `MenuFlyout` can be shared between the two.
- It is not a ribbon, and there is no tab-and-group ribbon model.
- There is no user customization UI: no drag-to-reorder, no docking of bars to window edges, no "customize tool bar" dialog, and no persistence of any of that. Bars are declared in XAML or built in code, and an application that wants them customizable stores its own state and rebuilds them.
- There is no per-item overflow priority, and no open or close animation on the overflow flyout.
- It does not ship an icon set. The icon types are here; the artwork is yours. There is no named-icon-set resource dictionary either - build a `ResourceDictionary` of `SvgIconSource` values yourself if you want icons by name.
- It does not rasterize SVG itself. Rendering goes through the platform's `SvgImageSource`, which the Svg add-in supplies; this package composes the stylesheet that carries a tint and asks for the right pixel size.

## Per-head notes

The controls are managed XAML with no per-head code, so a bar behaves the same on all six heads.

One host detail varies. `ClickWithModifiers` reports what the platform says is held down at the click, and some desktops reserve a modifier for themselves - Cinnamon uses Alt as its window-drag modifier by default - and grab that key, so no application sees it. Shift and Control are the safe choices for a modifier-aware click.

## Pitfalls

- Do not declare a property named `IconSize`, `LabelMode`, `LabelPosition` or `ShowToolTips` on a control you put in a bar. The framework propagates an inherited attached property by looking for a property of the same name on the child's type, so a control that registers its own `IconSize` silently stops seeing the bar's value and reads the default instead, with no error anywhere. That is also why the icon types' size property is called `Size`.
- Bind those four in the attached form. `cb:ToolBarProperties.LabelMode="{Binding ...}"` works; a binding on the bar's own `LabelMode` does not.
- `ShowToolTips`, plural and inherited, is not `ShowToolTip`, singular and per button. The plural one is the bar-level setting every item inherits; the singular one is the nullable override on a single `ToolButton`. The names differ deliberately, because a per-button property named `ShowToolTips` would hit the first pitfall.
- An icon source can be shared; an icon element cannot. Give the same `SvgIconSource` to five buttons and each builds its own element from it, and all five follow a later change to the source. An `SvgIcon` or `RasterIcon` is an element, and an element has one parent.
- Only a `SolidColorBrush` tints, and only its color is used. Make an icon translucent with the element's `Opacity`, not with the brush's alpha.
- On a raster icon the tint paints the alpha channel: the image's alpha becomes a mask the tint paints. That is what makes a monochrome PNG follow a theme, and it is what turns an opaque format such as JPEG into a filled rectangle. Leave `Tint` unset for artwork that carries its own colors.
- A button behind the chevron is not in the bar's visual tree. Walking up from it will not find the bar; use `ToolBar.OverflowItems` to go the other way.
- Write `Mode=TwoWay` on `IsChecked` yourself. This XAML dialect has no `BindsTwoWayByDefault`.
- A filling spacer in a bar inside a tray has nothing to fill, because a tray gives each bar the width the bar asks for. Put the bar somewhere that stretches - a `Grid` cell, a dock panel - for `Fill="True"` to push something to the far end.
- Remember which one you wrote: `<cb:SvgIcon UriSource="..." />` is an element, and `Icon="{cb:SvgIconSource Source=...}"` is a source. The markup extensions are named after the sources, because a prefixed XAML name is looked up with an `Extension` suffix first, in either syntax, so an extension named after an element would answer for the element form too and hand an `IconElement` property a source.
- Set `Size` on an icon element, not `Width` and `Height`. `Size`, or the inherited `IconSize`, is what the icon is rasterized at; `Width` and `Height` stretch a bitmap rendered for another size, which is the blur this package exists to avoid.
- `ToolBar.ItemsPanel` is not used. Re-template the bar, keeping a `ToolBarPanel` named `PART_ItemsHost`, to change how items are laid out.
- An icon with no artwork is an empty icon, not an exception. A missing file, a missing embedded resource or an unknown assembly leaves the icon blank and the application running - check `ResolvedUriSource` when an icon does not appear.
- A window manager may take a modifier before the application sees it, so prefer Shift and Control for a modifier-aware click.

## Related

- [Svg](Svg.md) - the renderer this add-in depends on and draws every vector icon through
- [CodeBrix.SkiaSvg](../../libraries/CodeBrix.SkiaSvg.md) - the standalone library under the Svg add-in, for rasterizing SVG with no window in sight
- [Views and styling](../06-views-and-styling.md) - building menus and toolbars from one command model the view model owns
- [CommandBarDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/CommandBarDemo) in CodeBrix.Platform - a score editor's two bars in one tray on six heads: groups with automatic separators, a filling spacer, all three button types, inline combo boxes, chevron overflow, both ways of binding a command, SVG and raster icons, and check boxes that change the bar-level label and tooltip settings while the window is open
- [Fresco.Brix](https://github.com/ellisnet/CodeBrix.Samples.Gpl3/tree/main/Fresco.Brix) in [the sample applications](../../samples/README.md) - the reference application: two window toolbars and two panel toolbars built from command objects, with one small shared builder behind the window's tray and a dock panel's own narrow bar

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.CommandBar/AGENT-README.txt) |
| Add-in source | [src/AddIns/Platform.UI.CommandBar](https://github.com/ellisnet/CodeBrix.Platform/tree/main/src/AddIns/Platform.UI.CommandBar) |
| Tests | [src/AddIns/Platform.UI.CommandBar.Tests](https://github.com/ellisnet/CodeBrix.Platform/tree/main/src/AddIns/Platform.UI.CommandBar.Tests) |
| Reference application | [samples/CodeBrixPlatform/CommandBarDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/CommandBarDemo) |
| Package | [`CodeBrix.Platform.CommandBar.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.CommandBar.ApacheLicenseForever) |

---

**Where to go next**

- [Views and styling](../06-views-and-styling.md) - the chapter where menus and toolbars are built from one command model
- [Svg](Svg.md) - the icon renderer this add-in brings with it
- [FlexPanel](FlexPanel.md) - the layout panel a bar's surroundings usually want next
- [All add-ins](../08-add-ins.md) - the whole set at a glance
