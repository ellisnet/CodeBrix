<sub>[CodeBrix](../../README.md) › [Build a CodeBrix.Platform application](README.md) › Views and styling</sub>

# Views and styling

**By the end of this chapter you will be able to write a page that binds to a view model, restyle every control in the application from one dictionary, reflow a layout as the window changes shape, build the controls the framework does not ship, and put the fonts and icons your application draws with inside the application itself.** Everything here is markup and page code-behind: the view-model side of the same recipes is [05 - MVVM the right way](05-mvvm-the-right-way.md), and the two chapters meet at the binding.

Professional styling on CodeBrix.Platform comes from four habits: name the theme's own resource keys instead of restyling controls, keep presentation decisions in converters or computed properties instead of code-behind, let a panel reflow rather than a breakpoint switch layouts, and ship your fonts so the application looks the same on a laptop and on a device with no installed fonts at all.

## A page and its view model

You write standard WinUI XAML. Controls, panels, styles, visual states, animations, `ListView` and `GridView` with data templates, `NavigationView`, `TabView`, flyouts, `MenuBar`, `CommandBar`, `ScrollViewer`, `SplitView`, `Slider`, `ToggleSwitch`, `ComboBox`, `DatePicker` and `TimePicker`, `ProgressRing`, `Image`, `TextBox`, `PasswordBox`, `RichEditBox` and the rest of the `Microsoft.UI.Xaml.Controls` surface are written exactly as the WinUI documentation describes them. A member that exists but is not backed by an implementation throws a "not implemented" exception that names the member, so a wrong turn is loud rather than silent.

### Declaring the namespaces

The house form declares the two standard WinUI XAML namespaces and reaches the framework's own helper types with `using:` prefixes:

```xml
<Page
    x:Class="MyApp.Views.MainPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:conv="using:CodeBrix.Platform.UI.Converters"
    xmlns:toolkit="using:CodeBrix.Platform.UI.Toolkit">
    <Page.Resources>
        <conv:BoolToVisibilityConverter x:Key="BoolToVis" />
    </Page.Resources>
    <Grid Padding="16" RowSpacing="8">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <StackPanel Orientation="Horizontal" Spacing="8">
            <Button Content="Open file..." Click="OnOpenFile" />
            <Button Content="Copy status" Click="OnCopyStatus" />
            <Button Content="Confirm" Click="OnConfirm" />
            <Button Content="Slow work" Click="OnSlowWork" />
            <ToggleSwitch Header="Dark" Toggled="OnThemeToggled" />
            <ProgressRing IsActive="True"
                          Visibility="{x:Bind ViewModel.IsBusy, Mode=OneWay, Converter={StaticResource BoolToVis}}" />
        </StackPanel>

        <toolkit:ElevatedView Grid.Row="1" Elevation="8" Background="{ThemeResource LayerFillColorDefaultBrush}">
            <ListView ItemsSource="{x:Bind ViewModel.Files}" />
        </toolkit:ElevatedView>

        <TextBlock Grid.Row="2" Text="{x:Bind ViewModel.Status, Mode=OneWay}" />
    </Grid>
</Page>
```

Notice `{x:Bind}`: it is compiled by the XAML source generator, is type-checked, and defaults to `Mode=OneTime`, so a value that changes needs `Mode=OneWay` or `Mode=TwoWay` spelled out. `{Binding}` also works, resolves at run time through generated metadata rather than reflection, and needs a `DataContext`.

The reference applications use a second form. They point the default XML namespace straight at the framework's controls assembly, declare the data namespace as `d`, and bind with `{d:Binding ...}`:

```xml
<!-- From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml -->
<Page
    x:Class="PdfSideBySide.Views.MainPage"
    xmlns="clr-namespace:Microsoft.UI.Xaml.Controls;assembly=CodeBrix.Platform.UI"
    xmlns:d="clr-namespace:Microsoft.UI.Xaml.Data;assembly=CodeBrix.Platform.UI"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:vm="clr-namespace:PdfSideBySide.ViewModels;assembly=PdfSideBySide.Core"
    xmlns:local="using:PdfSideBySide.Views"
    FontFamily="{StaticResource RobotoFont}"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

    <Page.DataContext>
        <vm:MainViewModel />
    </Page.DataContext>
    <!-- ... -->
        <!-- Bottom row: page labels and the comparison note -->
        <TextBlock Grid.Row="1" Grid.Column="0" Text="{d:Binding LeftPane.PageLabel}" HorizontalAlignment="Center" />
        <TextBlock Grid.Row="1" Grid.Column="1" Text="{d:Binding StatusText}" HorizontalAlignment="Center"
                   TextTrimming="CharacterEllipsis" TextWrapping="NoWrap" />
        <TextBlock Grid.Row="1" Grid.Column="2" Text="{d:Binding RightPane.PageLabel}" HorizontalAlignment="Center" />
```

Notice what each prefix is for. The default namespace decides where plain element names resolve, so `<TextBlock>` and `<Grid>` come from the framework's controls assembly. Types from your own libraries need an explicit `clr-namespace:...;assembly=...` prefix, and the assembly name is usually not the same as the namespace - see the root-namespace rule in [04 - Project architecture](04-project-architecture.md). In a page written this way, bindings are `{d:Binding ...}`; plain `{Binding ...}` silently does nothing. That is the one place the markup for the Skia heads and the markup for a native WinUI, WPF or .NET MAUI head genuinely differs, which is why pages are per-stack files while the view model is one file - the subject of [14 - Sharing code with native frameworks](14-sharing-code-with-native-frameworks.md).

> [!IMPORTANT]
> A class used as a binding source must carry `[Microsoft.UI.Xaml.Data.Bindable]`. That applies to the page's view model and to every node view model bound inside a template, not only the top one.

### Where the DataContext comes from

`<Page.DataContext>` with a `<vm:MainViewModel />` element inside it is the shortest form and the one the samples show, but it constructs the view model with its parameterless constructor, so no constructor injection is possible. Resolving the view model from `SimpleServiceResolver` in the page's constructor is the more flexible shape, and it is the one to reach for as soon as a view model needs a service.

Either way, the page's own wiring belongs in a `DataContextChanged` handler subscribed **before** `InitializeComponent()`, because on these heads it is `InitializeComponent()` that sets the data context. Reference pages carry the comment `//Leave this line last` on that call for exactly that reason. [07 - Platform services](07-platform-services.md) is that handler's whole job.

### Scoping a region to a child view model

Re-point `DataContext` on a container and every binding inside it becomes relative to the child:

```xml
<!-- From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml -->
        <Grid Grid.Column="0" DataContext="{d:Binding LeftPane}" RowSpacing="6">
            <!-- ... -->
                <Button Content="{d:Binding BrowseLabel}" Command="{d:Binding BrowseCommand}" FontWeight="SemiBold"
                        Height="24" MinHeight="0" Padding="8,0" />
```

Notice that the button inside binds `BrowseLabel` and `BrowseCommand` with no `LeftPane.` prefix. Two panes of the same shape become one template and two child view models.

### Command parameters from markup

A command that acts on one of several things takes a plain string parameter, and parses it defensively so a typo disables the button instead of throwing:

```xml
<!-- From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml -->
                <Button Grid.Row="0" Grid.Column="1" Width="24" Height="24" MinWidth="0" MinHeight="0" Padding="0"
                        Command="{d:Binding PanCommand}" CommandParameter="Left:Up"
                        ToolTipService.ToolTip="Document 1 - pan up">
                    <FontIcon Glyph="&#xE70E;" FontSize="12" />
                </Button>
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
    public SimpleCommand PanCommand => field ??=
        new SimpleCommand(parameter => CanPan(parameter), parameter => DoPan(parameter));

    private static bool TryParsePan(object parameter, out DocumentSide side, out PanDirection direction)
    {
        side = default;
        direction = default;
        if (parameter is not string text) { return false; }
        var parts = text.Split(':');
        return parts.Length == 2
            && Enum.TryParse(parts[0], ignoreCase: true, out side)
            && Enum.TryParse(parts[1], ignoreCase: true, out direction);
    }
```

Notice that the same parse serves both halves of the command: `CanPan` returns false for an unparseable parameter, so eight pan buttons are declared in markup and gated by one method.

One more markup rule worth knowing early: `Mode=TwoWay, UpdateSourceTrigger=PropertyChanged` on a text box is what makes `[AffectsCommands]` refresh buttons while the user types.

## What the framework already gives you

Beyond the WinUI control set, the core package folds in a small toolkit assembly - no extra package to install.

| Namespace | What is in it |
| --- | --- |
| `CodeBrix.Platform.UI.Toolkit` | `ElevatedView`, a drop-shadow container with `Elevation`, `ShadowColor`, `ElevatedContent` and `Background`; `StorageFileHelper.ExistsInPackage(string)` |
| `CodeBrix.Platform.UI.Converters` | `BoolToVisibilityConverter`, `NullToVisibilityConverter`, `StringToVisibilityConverter`, `CollectionToVisibilityConverter`, `BoolNegationConverter`, `BoolToObjectConverter`, `StringFormatConverter` |
| `CodeBrix.Platform.Diagnostics.UI` | `DiagnosticsOverlay.Get(XamlRoot).Show()`, an in-application diagnostics panel |
| `CodeBrix.Platform.UI.Markup` | `FromJsonExtension`, a markup extension that turns inline JSON into an object |

A card is one element:

```xml
<toolkit:ElevatedView Elevation="12" ShadowColor="#66000000" Background="White">
    <TextBlock Text="Card" Margin="16" />
</toolkit:ElevatedView>
```

## Application and page resources

`App.xaml` is the application-wide resource dictionary, and it is where anything that must reach the popup layer belongs. Page-level keys in `<Page.Resources>` cover that page only; dialogs, pickers and the on-screen keyboard open in the popup layer, follow the application's `RequestedTheme`, and read their keys from application resources.

### Merge the control styles first

```xml
<!-- From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.UI/App.xaml -->
<Application x:Class="NotionDocumentCreator.App"
     xmlns="clr-namespace:Microsoft.UI.Xaml;assembly=CodeBrix.Platform.UI"
     xmlns:m="clr-namespace:Microsoft.UI.Xaml.Media;assembly=CodeBrix.Platform.UI"
     xmlns:c="clr-namespace:Microsoft.UI.Xaml.Controls;assembly=CodeBrix.Platform.UI.FluentTheme"
     xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
     RequestedTheme="Dark">

  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <!-- Load WinUI resources -->
        <c:XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
      </ResourceDictionary.MergedDictionaries>
      <!-- Roboto font - reference the .ttf file directly (the Fonts.xaml
           merge does not work on Skia targets) -->
      <m:FontFamily x:Key="RobotoFont">ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/Roboto.ttf</m:FontFamily>

      <m:SolidColorBrush x:Key="ContentDialogBackground" Color="#1F232B" />
      <m:SolidColorBrush x:Key="ContentDialogForeground" Color="#F2F4F8" />
      <m:SolidColorBrush x:Key="ContentDialogBorderBrush" Color="#2A2F39" />
      <m:SolidColorBrush x:Key="ContentDialogLightDismissOverlayBackground" Color="#99000000" />
      <m:SolidColorBrush x:Key="ContentDialogTopOverlay" Color="#1F232B" />
      <m:SolidColorBrush x:Key="ContentDialogSeparatorBorderBrush" Color="#2A2F39" />
      <m:SolidColorBrush x:Key="ContentDialogSmokeFill" Color="#4D000000" />
    </ResourceDictionary>
  </Application.Resources>

</Application>
```

Three things to notice. `XamlControlsResources` has to be in the merged dictionaries at all, or the built-in control styles are missing. The overriding brushes are declared **after** the merged dictionary, in the same dictionary - that ordering is what makes them win. And the `ContentDialog*` keys are at application level because a dialog is in the popup layer; on the frame-buffer head the built-in picker and on-screen keyboard chrome resolve those same keys, so they restyle with the dialogs and need no separate treatment.

Ordinary application-wide dictionaries and values go in the same place:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="ms-appx:///Styles/Colors.xaml" />
        </ResourceDictionary.MergedDictionaries>
        <x:Double x:Key="BodyFontSize">14</x:Double>
    </ResourceDictionary>
</Application.Resources>
```

The Fluent control styles - theme dictionaries for Light, Dark and HighContrast - are built into the core package. Nothing else is referenced to get them.

### Re-key the theme brushes

The cheapest way to make stock controls follow your palette is to override the theme's own brush keys rather than restyle the controls:

```xml
<!-- From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml -->
<!-- Re-key the theme's accent-button brushes to the app's coral accent, so
     {ThemeResource AccentButtonStyle} buttons follow the app palette -->
<m:SolidColorBrush x:Key="AccentButtonBackground" Color="#F96854" />
<m:SolidColorBrush x:Key="AccentButtonBackgroundPointerOver" Color="#FF7F6C" />
<m:SolidColorBrush x:Key="AccentButtonBackgroundPressed" Color="#D65344" />
<m:SolidColorBrush x:Key="AccentButtonBackgroundDisabled" Color="#3A3F49" />
<m:SolidColorBrush x:Key="AccentButtonForeground" Color="#FFFFFF" />
<!-- ... -->

<!-- The primary (accent) button: the theme's accent style plus app shaping -->
<ui:Style x:Key="PrimaryButtonStyle" TargetType="c:Button" BasedOn="{StaticResource AccentButtonStyle}">
  <ui:Setter Property="CornerRadius" Value="8" />
  <ui:Setter Property="Padding" Value="16,7" />
  <ui:Setter Property="FontWeight" Value="SemiBold" />
</ui:Style>
```

Notice that every state has a key of its own - normal, pointer-over, pressed and disabled. Overriding only the base one leaves a gated command's button wearing the theme's disabled color, and a gated button spends real time disabled. A list's own selection brushes deserve the same treatment, and for the same reason: the theme's selection accent can wash out the text in your rows.

```xml
<!-- From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/App.xaml -->
<!-- The theme's own selection brushes are a light accent, which the light text in the file
     rows disappears into. These are the same accent taken down to something the rows read on. -->
<m:SolidColorBrush x:Key="ListViewItemBackgroundSelected" Color="#FF25344D" />
<m:SolidColorBrush x:Key="ListViewItemBackgroundSelectedPointerOver" Color="#FF2C3E5C" />
<m:SolidColorBrush x:Key="ListViewItemBackgroundSelectedPressed" Color="#FF1F2C42" />
<m:SolidColorBrush x:Key="ListViewItemBackgroundPointerOver" Color="#FF262B34" />
<m:SolidColorBrush x:Key="ListViewItemBackgroundPressed" Color="#FF20242B" />
```

> [!TIP]
> Base your own style on the theme style with `BasedOn="{StaticResource AccentButtonStyle}"` and set only shaping - corner radius, padding, weight. The colors come from the keys you re-keyed, so a later palette change is one edit in one dictionary.

Two keys are worth naming, because they are the ones a sweep misses. A list that is clicked rather than selected wants every one of its selection faces transparent instead of re-colored, so the row's own chrome is all that draws. And a text control's placeholder needs `PlaceholderForeground` set on the element as well as re-keyed: the template otherwise reaches that color through a binding whose theme-resource fallback does not survive an element-theme change at run time, and the placeholder text then disappears for good. The family-by-family sweep, and the rest of what it turns up, is [Re-key every control brush family the platform ships](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-ThemingAndStyling.md#re-key-every-control-brush-family-the-platform-ships).

Some colors cannot be shared resources at all, because they follow the data rather than the scheme: a state glyph per row, a pill wearing a color a server sent. The item view model owns a `SolidColorBrush` created once in its constructor and exposed get-only, the template binds to it, and a scheme change walks the items and re-tints each brush where it sits - nothing raises a change notification, because the object never changes, only its color does. That is [Give each item its own brushes and re-tint them on a scheme change](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-ThemingAndStyling.md#give-each-item-its-own-brushes-and-re-tint-them-on-a-scheme-change). Where the colors belong to the application's subject matter rather than to its design, the palette entry settles both values when it is constructed - the ink, and the caption color that reads on it - and a converter turns each into a brush at the last possible moment: [Let a domain object carry the caption color for its own ink](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-ThemingAndStyling.md#let-a-domain-object-carry-the-caption-color-for-its-own-ink).

### Choosing the theme

`Application.RequestedTheme` may be set only before initialization completes - in the `App` constructor, before `InitializeComponent()`. Afterwards the setter throws `NotSupportedException`. To switch at run time, set `FrameworkElement.RequestedTheme` on an element; setting it on the window's root element switches the whole application:

```csharp
    void OnThemeToggled(object sender, RoutedEventArgs e)
    {
        // per-element theme, applied to the page's whole subtree at run time
        RequestedTheme = ((ToggleSwitch)sender).IsOn ? ElementTheme.Dark : ElementTheme.Light;
    }
```

Notice that `ElementTheme` has a `Default` member as well as `Light` and `Dark`, so a subtree can be pinned to one theme and the rest left following the application.

Two things follow from that rule, and an application with palettes of its own leans on both. Leaving `Application.RequestedTheme` unassigned is what keeps the platform following the desktop's own light or dark preference; assigning it is what stops it, which is why a "System default" entry in a picker is the one choice that never assigns it. And a picker can offer more palettes than the platform has themes, because a keyed `SolidColorBrush` can be handed a new `Color` in place: every consumer repaints at once - the page, the stock control chrome, and the rows a list has already realized - with no tree rebuilt, no scroll position lost and no search abandoned. The reasoning is in [Choose a repaint mechanism that can carry more than two schemes](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-ThemingAndStyling.md#choose-a-repaint-mechanism-that-can-carry-more-than-two-schemes), and the working code in [Switch between several color schemes by mutating keyed brushes in place](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-ViewsAndControls.md#switch-between-several-color-schemes-by-mutating-keyed-brushes-in-place) and [Follow the operating system light and dark preference with a System default entry](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-ViewsAndControls.md#follow-the-operating-system-light-and-dark-preference-with-a-system-default-entry).

## Converters

A converter is for presentation-only mapping - a number to a timecode, a bool to a style. When the decision is application state rather than presentation, prefer a computed property on the view model, which is testable without a window; [05 - MVVM the right way](05-mvvm-the-right-way.md) makes that case for `Visibility` in particular.

### Register a converter twice, once inverted

Every converter the framework ships that answers a yes-or-no question carries an `Invert` property, and the convention is to declare the same type twice with two keys:

```xml
<!-- From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml -->
xmlns:c="clr-namespace:CodeBrix.Platform.UI.Converters;assembly=CodeBrix.Platform.UI.Toolkit"
...
<Page.Resources>
    <c:BoolToVisibilityConverter x:Key="VisibleWhenTrue" />
    <c:BoolToVisibilityConverter x:Key="VisibleWhenFalse" Invert="True" />
</Page.Resources>

<!-- The main viewer: the mirrored live preview in Camera Mode; the palm-reactive
     shader visual in Visualize Mode -->
<Border Grid.Row="1" BorderBrush="Gray" BorderThickness="1" Background="Black">
    <Grid>
        <camera:CameraCanvas x:Name="PreviewCanvas"
                             Visibility="{d:Binding IsCameraMode, Converter={StaticResource VisibleWhenTrue}}" />
        <game:GameSurfaceCanvas x:Name="VisualizerCanvas"
                                Visibility="{d:Binding IsCameraMode, Converter={StaticResource VisibleWhenFalse}}" />
    </Grid>
</Border>

<Grid Grid.Row="2" Margin="0,8,0,0">
    <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Spacing="8"
                Visibility="{d:Binding IsCameraMode, Converter={StaticResource VisibleWhenTrue}}">
        <Button Content="Visualize!" Command="{d:Binding VisualizeCommand}"
                MinWidth="120" Style="{ThemeResource AccentButtonStyle}" />
    </StackPanel>

    <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Spacing="8"
                Visibility="{d:Binding IsCameraMode, Converter={StaticResource VisibleWhenFalse}}">
        <Button Content="Back" Command="{d:Binding BackCommand}" MinWidth="100" />
    </StackPanel>
</Grid>

<TextBlock Grid.Row="3" Text="{d:Binding StatusText}" Margin="0,8,0,0" TextWrapping="Wrap" />
```

Notice that both visuals live in the same grid cell and differ only in visibility. Stacking them is what keeps a long-lived canvas alive - and its engine merely paused - across a mode switch. A control that should stay put is disabled rather than hidden (`IsEnabled="{d:Binding IsCameraMode}"`), so the layout does not shift. Where there are more than two panes, computed `Visibility` properties on the view model are tidier than a converter per pane.

### Write your own for formatting

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.Core/Converters/TimecodeConverter.cs
public sealed class TimecodeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not TimeSpan time || time < TimeSpan.Zero)
        {
            return "0:00";
        }

        return time.TotalHours >= 1
            ? string.Create(CultureInfo.InvariantCulture, $"{(int)time.TotalHours}:{time.Minutes:00}:{time.Seconds:00}")
            : string.Create(CultureInfo.InvariantCulture, $"{(int)time.TotalMinutes}:{time.Seconds:00}");
    }

    /// <summary>Not supported: a timecode is never typed back into the player.</summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException("A timecode is shown, never entered.");
}
```

```xml
<!-- From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml -->
<Page.Resources>
    <conv:TimecodeConverter x:Key="Timecode" />
</Page.Resources>
```

Notice four things. `IValueConverter` comes from the framework's data namespace and its last parameter is a `string`, not a `CultureInfo`. A wrong-typed or out-of-range value returns a safe default rather than throwing, so a binding that is briefly wrong does not take the page down. Anything with fixed separators formats through the invariant culture. And a one-way formatter that throws from `ConvertBack` is correct for a label but would break the moment the same converter were attached to a two-way binding.

The converter lives in the library that carries the application's view types, so the page reaches it with a `clr-namespace:...;assembly=...` declaration of its own - a different prefix from the framework's converters.

### Map a bool to something that is not a bool

`BoolToObjectConverter` carries two content properties, and they can hold real typed values rather than strings:

```xml
<!-- From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml -->
<!-- A file this application cannot play is still listed, still selectable and still a
     conversion source - it is only shown dimmed, so the rows that can be played stand out.
     The two stops are real doubles rather than strings so the row's Opacity takes them
     without a conversion of its own. -->
<cv:BoolToObjectConverter x:Key="PlayableOpacity">
    <cv:BoolToObjectConverter.TrueValue>
        <x:Double>1.0</x:Double>
    </cv:BoolToObjectConverter.TrueValue>
    <cv:BoolToObjectConverter.FalseValue>
        <x:Double>0.45</x:Double>
    </cv:BoolToObjectConverter.FalseValue>
</cv:BoolToObjectConverter>

<ui:DataTemplate x:Key="LibraryItemTemplate">
    <!-- One Opacity on the row dims the badge, the name and the summary together. The name is
         how the scripted run finds a row and reads the opacity it is really shown at. -->
    <Grid x:Name="LibraryRow"
          Padding="2,6"
          Opacity="{d:Binding IsPlayable, Converter={StaticResource PlayableOpacity}}">
        <!-- ... a format badge in a Border, then the file name and summary ... -->
    </Grid>
</ui:DataTemplate>
```

Notice the single `Opacity` on the row's outermost element: the badge, the name and the summary dim together, and one bool on the item model drives it.

A converter can also return a `Style`, which turns a row of buttons into a radio group with no template:

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Converters/BoolToAccentStyleConverter.cs
public sealed class BoolToAccentStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool selected && selected
            && Application.Current is { } app
            && app.Resources.TryGetValue("AccentButtonStyle", out var resource)
            && resource is Style style)
        {
            return style;
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
```

```xml
<!-- From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml -->
<Page.Resources>
    <conv:BoolToAccentStyleConverter x:Key="SelectedButtonStyle" />
</Page.Resources>
<!-- ... -->
<Button Content="Sample Texture" Command="{d:Binding SelectTextureCommand}" MinWidth="140"
        Style="{d:Binding IsTextureSelected, Converter={StaticResource SelectedButtonStyle}}" />
```

Notice the defensive lookup: a missing resource returns `null`, which is the default style, rather than an exception. The view model exposes one bool per option, and raises all of them together from one helper whenever the selection changes.

## Layouts that reflow

A `Grid` with fixed columns is the right answer until the window changes shape. When a toolbar should fold onto a second line, or a two-pane split should become a stack on a tall window, reach for the FlexPanel add-in rather than a breakpoint and a converter.

Add [CodeBrix.Platform.FlexPanel.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.FlexPanel.ApacheLicenseForever) to `.Core` once, and the XAML in the shared `.UI` project resolves the prefix:

```xml
<!-- From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml -->
xmlns:flex="clr-namespace:CodeBrix.Platform.UI.FlexPanel;assembly=CodeBrix.Platform.UI.FlexPanel"
```

```xml
<!-- From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml -->
<!-- Header: identity on the left; search, category filter and the assets folder
     on the right. A wrapping FlexPanel keeps everything on one row while the
     window is wide enough. -->
<flex:FlexPanel Direction="Row" Wrap="Wrap" AlignItems="Center">
    <!-- Grow=1: the identity block soaks up the free main-axis space, keeping
         the other groups pinned right while they still share its row -->
    <StackPanel Spacing="2" Margin="0,6,16,6" flex:FlexPanel.Grow="1">
        <!-- ... title and strapline ... -->
    </StackPanel>

    <!-- Search and the category filter travel as one unit when the panel wraps -->
    <StackPanel Orientation="Horizontal" Spacing="12" Margin="0,6,16,6">
        <TextBox Width="240" VerticalAlignment="Center"
                 PlaceholderText="Search assets…"
                 Text="{d:Binding SearchText, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                 CornerRadius="8" />
        <ComboBox Width="190" VerticalAlignment="Center"
                  CornerRadius="8"
                  ItemsSource="{d:Binding Categories}"
                  SelectedItem="{d:Binding SelectedCategory, Mode=TwoWay}" />
    </StackPanel>

    <Button CornerRadius="8" Padding="14,8" Margin="0,6,0,6" BorderThickness="1"
            MaxWidth="300"
            Command="{d:Binding PickFolderCommand}">
        <!-- ... folder glyph and AssetsFolderLabel ... -->
    </Button>
</flex:FlexPanel>
```

Notice that each thing that must wrap as a unit is one child of the panel: the panel wraps children, not their contents. `Grow` and `Basis` are attached properties on the child, never on the panel. And the `MaxWidth` on the folder button matters, because its caption is a chosen path and could otherwise consume the whole row before the panel gets a chance to wrap.

Where the wrap point must be predictable rather than content-dependent, give the child a basis:

```xml
<!-- From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.UI/Views/MainPage.xaml -->
<flex:FlexPanel Direction="Row" Wrap="Wrap" AlignItems="Center">

    <!-- Save-target group; Grow=1 so the path box stretches into whatever
         width its row has, Basis so the wrap point is deterministic -->
    <Grid Margin="0,4,16,4" ColumnSpacing="10"
          flex:FlexPanel.Grow="1" flex:FlexPanel.Basis="420">
        <!-- ... label, path TextBox, Select button ... -->
    </Grid>

    <!-- Page-size + Create! group -->
    <StackPanel Orientation="Horizontal" Spacing="10" Margin="0,4,0,4">
        <!-- ... label, ComboBox, primary button ... -->
    </StackPanel>
</flex:FlexPanel>
```

### Flip the main axis when the window changes shape

Turning a side-by-side split into a stack is one property on the panel, and it is layout plumbing rather than application logic:

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs
//The Model View's content panes: side-by-side while the window is landscape. In
//portrait the FlexPanel's main axis flips so the 3D viewer drops below the info
//panes, and the info panes trade their fixed-width column (an explicit Width, so
//their content measures - and wraps - against it) for half the height as a flex
//basis, still scrolling internally.
SizeChanged += (_, args) =>
{
    var portrait = args.NewSize.Width < args.NewSize.Height;
    ModelContentFlex.Direction = portrait ? FlexDirection.Column : FlexDirection.Row;
    ModelInfoPane.Width = portrait ? double.NaN : 420;
    FlexPanel.SetBasis(ModelInfoPane,
        portrait ? new FlexBasis(0.5f, isRelative: true) : FlexBasis.Auto);
    ModelInfoPane.Margin = portrait ? new Thickness(0, 0, 0, 20) : new Thickness(0, 0, 20, 0);
};
```

Notice that an explicit `Width` and a `FlexPanel.Basis` are not interchangeable. Content is measured against a `Width`, so text wraps to the pane; a basis sizes the box without giving the content that constraint. The margin has to move with the axis too - a right margin in landscape, a bottom margin in portrait. If the orientation matters to anything beyond layout, put an `IsPortrait` property on the view model and set it from the same handler.

The full property set - `JustifyContent`, `AlignItems`, `AlignContent`, `AlignSelf`, `Order`, `Shrink`, percentage bases - is on the [FlexPanel add-in page](add-ins/FlexPanel.md).

## Controls you write yourself

Some things have no stock control, and some are more faithful drawn than composed. All of them are ordinary `FrameworkElement`, `Control` or `Panel` subclasses with dependency properties, declared in a library and named from XAML with a `clr-namespace:...;assembly=...` prefix.

### An image from an embedded resource

Vector icons that ship inside the assembly, referenced from XAML by name, with no file paths and no per-head asset pipeline:

```csharp
// From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.Core/Controls/EmbeddedImage.cs
public sealed class EmbeddedImage : Image
{
    public static readonly DependencyProperty UriSourceProperty =
        DependencyProperty.Register(
            nameof(UriSource), typeof(string), typeof(EmbeddedImage),
            new PropertyMetadata(null, OnUriSourceChanged));

    public string UriSource
    {
        get => (string)GetValue(UriSourceProperty);
        set => SetValue(UriSourceProperty, value);
    }

    private static void OnUriSourceChanged(
        DependencyObject d, DependencyPropertyChangedEventArgs e)
        => _ = LoadImageAsync((EmbeddedImage)d, e.NewValue as string);

    private static async Task LoadImageAsync(EmbeddedImage image, string uri)
    {
        // ...
        if (uri.StartsWith("embedded://", StringComparison.OrdinalIgnoreCase))
        {
            // Parse: embedded://AssemblyName/Fully.Qualified.Resource.Name
            var path = uri["embedded://".Length..];
            var separatorIndex = path.IndexOf('/');
            // ...
            var assemblyName = path[..separatorIndex];
            var resourceName = path[(separatorIndex + 1)..];

            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName)
                ?? throw new InvalidOperationException(
                    $"Assembly '{assemblyName}' is not loaded.");

            await using var resourceStream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException(
                    $"Resource '{resourceName}' not found in '{assemblyName}'.");

            // Copy embedded resource into an IRandomAccessStream.
            // Note: ras and writeStream are intentionally not disposed here.
            var ras = new InMemoryRandomAccessStream();
            var writeStream = ras.AsStreamForWrite();
            await resourceStream.CopyToAsync(writeStream);
            await writeStream.FlushAsync();
            ras.Seek(0);

            // Use SvgImageSource for .svg files, BitmapImage for everything else
            if (resourceName.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            {
                var svgSource = new SvgImageSource();
                await svgSource.SetSourceAsync(ras);
                image.Source = svgSource;
            }
            else
            {
                var bitmapSource = new BitmapImage();
                await bitmapSource.SetSourceAsync(ras);
                image.Source = bitmapSource;
            }
        }
        // ... otherwise fall back to SvgImageSource or BitmapImage with a plain UriSource ...
    }
}
```

```xml
<!-- From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml -->
<controls:EmbeddedImage Margin="20,0,0,0" Width="60" Height="60"
    VerticalAlignment="Center"
    UriSource="embedded://JustBetweenUs.Core/JustBetweenUs.Assets.padlock-icon.svg" />
```

Notice the stream-ownership comment, which records a real ordering rule: disposing the write stream closes the underlying random-access stream, and disposing the random-access stream is unsafe because the image source may keep a reference to it rather than copying. Both are left to the garbage collector, which is safe here because an in-memory stream holds no file or unmanaged handles. Two more things to know: the assembly is found by scanning already-loaded assemblies, so an assembly nothing has touched will not be found - reference a type from it to keep it loaded; and load failures are written to the debug output, so a wrong resource name shows an empty image with no visible error. `SvgImageSource` support comes from [CodeBrix.Platform.Svg.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Svg.ApacheLicenseForever) - see the [Svg add-in page](add-ins/Svg.md).

### An image-and-text button

A `Button` subclass that rebuilds its own content from dependency properties gives you toolbar buttons with the icon above, below, left or right of the caption:

```csharp
// From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.Core/Controls/EmbeddedImageButton.cs
public sealed class EmbeddedImageButton : Button
{
    public EmbeddedImageButton()
    {
        DefaultStyleKey = typeof(Button);
        CornerRadius = new CornerRadius(4);
    }

    // ... ImageUriSource, Text, ImagePosition, Spacing, ImageWidth, ImageHeight,
    //     TextVerticalAlignment and TextHorizontalAlignment dependency properties,
    //     all registered with OnLayoutPropertyChanged ...

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);

        if (!_isUpdatingContent && newContent is string text)
        {
            text = text.Trim();
            if (text.Length > 0)
            {
                Text = text;
            }
        }
    }

    private void UpdateContent()
    {
        _isUpdatingContent = true;
        try
        {
            var hasImage = !string.IsNullOrWhiteSpace(ImageUriSource);
            var hasText = !string.IsNullOrWhiteSpace(Text);

            if (!hasImage && !hasText) { Content = null; return; }

            if (hasImage && hasText)
            {
                var isHorizontal = ImagePosition is ImagePosition.Left or ImagePosition.Right;
                var imageFirst = ImagePosition is ImagePosition.Left or ImagePosition.Top;

                var panel = new StackPanel
                {
                    Orientation = isHorizontal ? Orientation.Horizontal : Orientation.Vertical,
                    Spacing = Spacing
                };

                panel.Children.Add(imageFirst ? CreateImage() : CreateTextBlock());
                panel.Children.Add(imageFirst ? CreateTextBlock() : CreateImage());

                Content = panel;
            }
            else if (hasImage) { Content = CreateImage(); }
            else { Content = CreateTextBlock(); }
        }
        finally
        {
            _isUpdatingContent = false;
        }
    }
}
```

```xml
<!-- From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml -->
<controls:EmbeddedImageButton Margin="0,0,20,0" Width="140" Height="90"
    VerticalAlignment="Center" HorizontalAlignment="Right"
    Background="#FFB85555"
    Command="{d:Binding EncryptCommand}"
    ImageUriSource="embedded://JustBetweenUs.Core/JustBetweenUs.Assets.padlock-icon.svg"
    Text="Encrypt" ImageWidth="40" ImageHeight="40" Spacing="6" ImagePosition="Top" />

<controls:EmbeddedImageButton Grid.Row="4" Grid.Column="0" Grid.ColumnSpan="2" Width="220" Height="50"
    VerticalAlignment="Center" HorizontalAlignment="Center"
    Background="#FFB85555"
    Command="{d:Binding CopyToClipboardCommand}"
    ImageUriSource="embedded://JustBetweenUs.Core/JustBetweenUs.Assets.clipboard.svg">
    Copy to Clipboard
</controls:EmbeddedImageButton>
```

Notice two details that make the control feel native. `DefaultStyleKey = typeof(Button)` makes the subclass pick up the standard button template instead of needing one of its own. And `OnContentChanged` is overridden so text written between the opening and closing tags becomes the `Text` property instead of replacing the composed panel - the guard flag is what stops the override fighting the rebuild. An equivalent control with the same property names and the same URI scheme ships in [CodeBrix.Platform.WinUI.Skia.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.WinUI.Skia.ApacheLicenseForever), so the same markup works on a native WinUI head with a different XML namespace.

### A drawn widget with hit testing

A small control whose geometry is fixed and pixel-exact - a color swatch strip, a gauge, a mini timeline - is more faithful drawn than composed. Subclass the Skia canvas element from [CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever) and raise a semantic event when the user asks for something the view cannot decide:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/Palette/PaletteWidget.cs
public sealed class PaletteWidget : SKXamlCanvas
{
    private const int WidgetHeight = 42;
    private static readonly SKRect PrimaryRect = SKRect.Create (4, 3, SwatchSize, SwatchSize);
    private static readonly SKRect SecondaryRect = SKRect.Create (17, 16, SwatchSize, SwatchSize);
    private static readonly SKRect SwapRect = SKRect.Create (27, 2, 15, 15);
    private static readonly SKRect ResetRect = SKRect.Create (2, 27, 15, 15);

    /// <summary>
    /// Raised when the user asks to edit a colour - a click on either swatch,
    /// or a modifier-click on a palette entry.
    /// </summary>
    public event EventHandler<PaletteColorEditEventArgs>? ColorEditRequested;

    public PaletteWidget ()
    {
        Height = WidgetHeight;
        MinWidth = 300;

        PaintSurface += OnPaintSurface;
        PointerPressed += OnPointerPressedHandler;

        PintaCore.Palette.PrimaryColorChanged += OnPaletteChanged;
        PintaCore.Palette.SecondaryColorChanged += OnPaletteChanged;
        PintaCore.Palette.RecentColorsChanged += OnPaletteChanged;
        PintaCore.Palette.CurrentPalette.PaletteChanged += OnPaletteChanged;
    }

    private void OnPaletteChanged (object? sender, EventArgs e) => Invalidate ();

    private void OnPointerPressedHandler (object sender, PointerRoutedEventArgs e)
    {
        PointerPoint point = e.GetCurrentPoint (this);
        SKPoint position = new ((float) point.Position.X, (float) point.Position.Y);
        // ...
        // The primary swatch is drawn on top, so it is tested first.
        if (PrimaryRect.Contains (position)) {
            ColorEditRequested?.Invoke (this, new PaletteColorEditEventArgs (PaletteColorTarget.Primary, -1));
            return;
        }
        // ...
    }
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Palette.cs
private void BuildPaletteWidget()
{
    paletteWidget = new PaletteWidget();
    paletteWidget.ColorEditRequested += async (_, args) => await EditColorAsync(args);
    PaletteWidgetHost.Content = paletteWidget;
}
```

Notice that hit testing runs in the same order as drawing, so overlapping regions resolve to the region the user can actually see. Every rectangle is a constant in device-independent pixels declared once, which is what stops the drawing and the hit test drifting apart. The control subscribes to four model events in its constructor and never unsubscribes - acceptable precisely because it lives for the life of the window, and the source says so rather than leaving it implied.

### A splitter bar

The framework ships no splitter control. A tiny `Border` subclass that captures the pointer and reports drag deltas is the whole of it; the owner decides what a delta means:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/ThumbSplitter.cs
public sealed class ThumbSplitter : Border
{
    /// <summary>
    /// Raised while dragging with the movement since the last report, in the
    /// axis the splitter resizes: X for a vertical bar, Y for a horizontal one.
    /// </summary>
    public event EventHandler<double>? DragDelta;

    public ThumbSplitter (Orientation orientation)
    {
        Orientation = orientation;
        Background = new SolidColorBrush (Windows.UI.Color.FromArgb (0x30, 0x80, 0x80, 0x80));

        if (orientation == Orientation.Vertical)
            Width = 6;
        else
            Height = 6;

        ProtectedCursor = InputSystemCursor.Create (
            orientation == Orientation.Vertical
            ? InputSystemCursorShape.SizeWestEast
            : InputSystemCursorShape.SizeNorthSouth);

        PointerPressed += OnPointerPressedHandler;
        PointerMoved += OnPointerMovedHandler;
        PointerReleased += OnPointerReleasedHandler;
    }
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs
ThumbSplitter columnSplitter = new(Orientation.Vertical);
Grid.SetColumn(columnSplitter, 2);
ContentGrid.Children.Add(columnSplitter);
columnSplitter.DragDelta += (_, delta) =>
{
    double width = Math.Clamp(PadsColumn.ActualWidth - delta, 200, 800);
    PadsColumn.Width = width;
    PintaCore.Settings.PutSetting("pads-width", (int)width);
};
```

Notice the sign: the pane grows as the splitter moves the other way, and the delta is relative to the previous report, so the owner clamps against its own minimum. `ProtectedCursor` is a protected member of `UIElement`, which is why setting a cursor needs a subclass. The XAML reserves an empty column for the splitter and the control is added in code on load. Writing the new size to settings on every delta is cheap only because the settings store skips unchanged values - see [07 - Platform services](07-platform-services.md).

Where the regions you want are panes rather than an arbitrary pair of elements, `TriPaneView` is already there: a three-pane control in the toolkit inside the core CodeBrix.Platform package, so a project that references the platform adds nothing to use it. It owns the dividers and their grips, takes a percentage and a minimum length per pane, and can minimize a pane and restore it. The control is sealed, so a window that wants a fourth region owns a pair of them rather than deriving from one - [Nest two TriPaneView controls to put four regions around an editor](https://github.com/ellisnet/CodeBrix.Samples.Gpl3/blob/main/BLUEPRINTS.md#nest-two-tripaneview-controls-to-put-four-regions-around-an-editor) and [Show and hide a TriPaneView pane by minimizing and restoring it](https://github.com/ellisnet/CodeBrix.Samples.Gpl3/blob/main/BLUEPRINTS.md#show-and-hide-a-tripaneview-pane-by-minimizing-and-restoring-it).

### A modeless options panel

A modal dialog that dims the window defeats a live preview. A popup-based host with its own title bar returns a `Task<bool>`, so the calling code awaits it exactly like a dialog:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/FloatingDialogHost.cs
public static async Task<bool> ShowAsync (string title, UIElement content, XamlRoot xamlRoot, double maxWidth = 460)
{
    TaskCompletionSource<bool> completion = new (TaskCreationOptions.RunContinuationsAsynchronously);

    //The panel is deliberately OPAQUE: translucent surfaces over a white
    //canvas wash out to unreadable (the menu flyouts demonstrated it).
    Border root = new () {
        Background = new SolidColorBrush (Windows.UI.Color.FromArgb (0xFF, 0x2B, 0x2B, 0x2B)),
        // ...
        RequestedTheme = ElementTheme.Dark,
    };

    Popup popup = new () {
        XamlRoot = xamlRoot,
        IsLightDismissEnabled = false,
        Child = root,
    };
    // ... title, content, OK/Cancel, Escape -> Complete(false)

    //Centre horizontally below the toolbars; the canvas stays visible
    //beneath and beside the panel.
    root.Measure (new Windows.Foundation.Size (double.PositiveInfinity, double.PositiveInfinity));
    popup.HorizontalOffset = Math.Max (0, (xamlRoot.Size.Width - root.DesiredSize.Width) / 2);
    popup.VerticalOffset = 110;
    popup.IsOpen = true;

    return await completion.Task;
}
```

Notice the `Measure` call with infinite constraints before the desired size is read: a popup child is not in the normal layout pass, so it has no desired size until it is measured. The `TaskCompletionSource` with `RunContinuationsAsynchronously` is what turns a modeless popup into an awaitable, dialog-shaped call. Dragging the title block moves the panel by adjusting the popup's offsets from pointer deltas, with pointer capture on the title block.

### A panel generated from a descriptor or by reflection

When a library must not reference the UI framework - a plugin's options, a tool's settings row - let the library append framework-free descriptors to a list and let a renderer in the UI layer materialize each one:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/ToolBarRenderer.cs
public sealed class ToolBarRenderer : IDisposable
{
    private readonly EngineToolBar model;
    private readonly StackPanel panel;

    //Container descriptors outlive any single Rebuild (they belong to the
    //tool), so their event subscriptions must be detached when the toolbar
    //is rebuilt or the old handlers keep rebuilding orphaned panels.
    private readonly List<Action> detachers = [];

    public ToolBarRenderer (EngineToolBar model, StackPanel panel)
    {
        this.model = model;
        this.panel = panel;
        model.ItemsChanged += OnItemsChanged;
        Rebuild ();
    }

    private UIElement? CreateElement (ToolBarItem item)
    {
        UIElement? element = item switch {
            ToolBarLabel label => new TextBlock { Text = label.Text, /* ... */ },
            ToolBarSeparator => new Border { Width = 1, /* ... */ },
            ToolBarImage image => CreateImage (image),
            ToolBarToggleButton toggle => CreateToggle (toggle),
            ToolBarDropDownButton dropDown => CreateDropDown (dropDown),
            ToolBarComboBox combo => CreateCombo (combo),
            ToolBarSpinButton spin => CreateSpin (spin),
            ToolBarScale scale => CreateScale (scale),
            ToolBarContainer container => CreateContainer (container),
            _ => null,
        };
        // ... tooltip, then a Visible->Visibility binding with a detacher
        return element;
    }
}
```

Notice the detacher list. The descriptors belong to the tool and outlive any single rebuild, so every subscription made during a rebuild needs an explicit undo, or old handlers keep rebuilding panels that are no longer in the tree. Descriptor visibility maps to `Visibility`, so a tool can hide an option without forcing a rebuild.

For many small parameter objects, reflection over the object's own members is less code than a panel each:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/EffectOptionsDialog.cs
private static IEnumerable<MemberInfo> GetDialogMembers (EffectData data)
{
    Type type = data.GetType ();
    foreach (MemberInfo member in type.GetMembers (BindingFlags.Public | BindingFlags.Instance)) {
        if (member is not PropertyInfo and not FieldInfo)
            continue;
        if (member is PropertyInfo { CanWrite: false })
            continue;
        if (member.DeclaringType == typeof (EffectData) || member.DeclaringType == typeof (ObservableObject))
            continue;
        if (member.GetCustomAttribute<SkipAttribute> () is not null)
            continue;
        yield return member;
    }
}

private static string GetCaption (MemberInfo member)
    => member.GetCustomAttribute<CaptionAttribute> ()?.Caption
        ?? AddSpaces (member.Name);

private static string AddSpaces (string name)
    => string.Concat (name.Select ((c, i) => i > 0 && char.IsUpper (c) ? " " + c : c.ToString ()));
```

Notice that base-class members are excluded explicitly, or every object gets its base type's plumbing rendered as editable rows, and that an unsupported member type degrades to a read-only note rather than being silently dropped - so a missing editor is visible while you are building it.

## Menus, toolbars and the shell

### Build the menu from a command model

Past a handful of commands, declaring a label, an icon, an enabled state and a shortcut in markup means editing two files for every change. Declare the command once in a headless library and build the menu from it:

```xml
<!-- From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml -->
<!-- Menu. Built from the Pinta.Brix.Engine action model at runtime; see
     MainPage.Menus.cs. Nothing is declared here, so a command declared
     once in Actions/*.cs gets its label, icon, enabled state and
     shortcut without a second edit. -->
<MenuBar x:Name="MainMenuBar" Grid.Row="0" />
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/Menus/CommandMenuBuilder.cs
public static MenuFlyoutItemBase Create (Command command, bool showIcon = true)
{
    ArgumentNullException.ThrowIfNull (command);

    if (command is ToggleCommand toggle)
        return CreateToggle (toggle);

    MenuFlyoutItem item = new () {
        Text = command.Label,
        IsEnabled = command.Sensitive,
    };

    ApplyIcon (item, command, showIcon);
    ApplyAcceleratorText (item, command);

    item.Click += (_, _) => command.Activate ();
    command.SensitiveChanged += (_, _) => item.IsEnabled = command.Sensitive;

    return item;
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Menus.cs
private static MenuBarItem BuildMenu(string title, params Command[] commands)
{
    MenuBarItem menu = new() { Title = title };

    foreach (Command command in commands)
    {
        menu.Items.Add(command is null
            ? CommandMenuBuilder.CreateSeparator()
            : CommandMenuBuilder.Create(command));
    }

    return menu;
}
```

Notice that a `null` entry in the command array is a separator, which keeps the call sites readable. With `SimpleCommand` the same builder would bind `CanExecute` instead of subscribing to an enabled-changed event, and the XAML would still declare no commands. A missing icon must not take the menu down: the icon factory can return null and the builder omits it.

Toolbars come from the same command objects. The [CommandBar add-in](add-ins/CommandBar.md) supplies the bar itself, a tray that lays several bars out side by side, plain, toggle and drop-down button types, separators and spacers, an overflow chevron, the keyboard walk along a bar and the automation peers, so what the application writes is a list saying what is on each bar and in what order - [Add the CommandBar add-in and build a tray of two toolbars from command objects](https://github.com/ellisnet/CodeBrix.Samples.Gpl3/blob/main/BLUEPRINTS.md#add-the-commandbar-add-in-and-build-a-tray-of-two-toolbars-from-command-objects) and [Give a panel its own toolbar with the CommandBar add-in](https://github.com/ellisnet/CodeBrix.Samples.Gpl3/blob/main/BLUEPRINTS.md#give-a-panel-its-own-toolbar-with-the-commandbar-add-in). Keeping that list as data is also what lets a test assert what is on a bar in a process with no window - [Assert a toolbar's contents and a shell's pane arithmetic in host-free tests](https://github.com/ellisnet/CodeBrix.Samples.Gpl3/blob/main/BLUEPRINTS.md#assert-a-toolbars-contents-and-a-shells-pane-arithmetic-in-host-free-tests).

### Keyboard shortcuts

XAML keyboard-accelerator objects do not invoke on the Skia heads. Dispatch from one page-level `KeyDown` handler against a table the model owns:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/Input/CommandAcceleratorTable.cs
public bool TryInvoke (VirtualKey key)
{
    if (!map.TryGetValue ((key, CurrentModifiers), out Engine.Command? command))
        return false;

    // A disabled command must swallow nothing: the key should behave as if
    // the shortcut were not bound at all.
    if (!command.Sensitive)
        return false;

    command.Activate ();
    return true;
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Menus.cs
acceleratorTable = new CommandAcceleratorTable();

foreach (Command command in actions.AllCommands())
{
    acceleratorTable.Register(command);
}

//Handled keys have to be seen too: the canvas marks most key events
//handled, and a shortcut must still work while it has focus.
AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(OnGlobalKeyDown), handledEventsToo: true);
AddHandler(UIElement.KeyUpEvent, new KeyEventHandler(OnGlobalKeyUp), handledEventsToo: true);
```

Notice `handledEventsToo: true`. A canvas marks most key events handled, so a plain `KeyDown` subscription never sees them. The menu items still show their shortcut text through the text-override property, which does work, and the builder deliberately attaches no real accelerator so there is never a second dispatch path. Modifier state is tracked from the modifier keys' own down and up transitions, because the key-state API returns nothing on the Skia heads - and a reset on focus loss keeps a modifier released elsewhere from staying stuck down. Duplicate accelerators resolve first-registration-wins, deliberately. Because the table holds model commands and no UI type, it is testable with no window at all.

For a single key on a single control, prefer the declarative form where the stack has one, and keep a handler to one forwarding line where it does not:

```csharp
// From CodeBrix.Samples/WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml.cs
//Pressing Enter in the search box runs Search, just like clicking the button.
private void SearchBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
{
    if (e.Key == Windows.System.VirtualKey.Enter
        && DataContext is MainViewModel { SearchCommand: var search }
        && search.CanExecute(null))
    {
        search.Execute(null);
        e.Handled = true;
    }
}
```

Notice the `CanExecute` check: a key handler bypasses the disabled state a button would have honored.

### The window shape of an editor

The XAML declares the grid and the named hosts; everything inside the hosts is built at load time from model state, or bound to observable collections in a view-model shape:

```xml
<!-- From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml -->
<Grid.RowDefinitions>
    <RowDefinition Height="Auto" />   <!-- menu bar -->
    <RowDefinition Height="Auto" />   <!-- icon toolbar -->
    <RowDefinition Height="Auto" />   <!-- tool options -->
    <RowDefinition Height="*" />      <!-- toolbox | tabs | splitter | pads -->
    <RowDefinition Height="Auto" />   <!-- status bar -->
</Grid.RowDefinitions>

<!-- In-app icon toolbar row. Deliberately NOT an OS header bar: the
     Frame Buffer head has no window chrome at all, so anything parked
     there would be unreachable. -->
<Border x:Name="MainToolbarBorder" Grid.Row="1" BorderThickness="0,0,0,1"
        BorderBrush="{ThemeResource SystemControlForegroundBaseLowBrush}">
    <ScrollViewer HorizontalScrollBarVisibility="Auto" VerticalScrollBarVisibility="Disabled">
        <StackPanel x:Name="MainToolbarPanel" Orientation="Horizontal" Padding="6,3" Spacing="2" />
    </ScrollViewer>
</Border>

<TabView x:Name="DocumentTabs"
         Grid.Column="1"
         IsAddTabButtonVisible="False"
         TabCloseRequested="DocumentTabs_TabCloseRequested"
         SelectionChanged="DocumentTabs_SelectionChanged" />
```

> [!WARNING]
> Do not put commands in an operating-system header bar if you ship the Linux frame-buffer head. That head has no window chrome at all, so anything parked there is unreachable. An in-application toolbar row works on all six heads.

One more shell detail worth copying: status text sourced from a model can be many lines long, and a status bar that grows is a layout bug the first time a shape tool sets it.

```xml
<!-- From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml -->
<!-- MaxLines keeps the bar one line tall: the shape tools carry
     many-line StatusBarText and would otherwise grow the bar. -->
<TextBlock x:Name="StatusText" Grid.Column="1" VerticalAlignment="Center" TextTrimming="CharacterEllipsis" MaxLines="1" />
```

## Pointer and keyboard input

### Forward pointer events into a model

Four handlers, a few lines each, and the model holds the state:

```csharp
// From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml.cs
DrawCanvas.PointerPressed += (_, e) =>
{
    var session = ViewModel?.Session;
    if (session == null) { return; }

    var pointerPoint = e.GetCurrentPoint(DrawCanvas);
    if (!pointerPoint.Properties.IsLeftButtonPressed) { return; }

    if (session.PointerPressed(DrawCanvasHelper.GetPointFromPosition(pointerPoint.Position), DrawCanvas.GetViewSize()))
    {
        DrawCanvas.CapturePointer(e.Pointer);
        e.Handled = true;
    }
};

DrawCanvas.PointerMoved += (_, e) =>
{
    var session = ViewModel?.Session;
    if (session is not { IsPointerActive: true }) { return; }

    session.PointerMoved(DrawCanvasHelper.GetPointFromPosition(e.GetCurrentPoint(DrawCanvas).Position), DrawCanvas.GetViewSize());
    e.Handled = true;
};

DrawCanvas.PointerReleased += (_, e) =>
{
    var session = ViewModel?.Session;
    if (session is not { IsPointerActive: true }) { return; }

    session.PointerReleased();
    DrawCanvas.ReleasePointerCapture(e.Pointer);
    e.Handled = true;
};

//If capture is lost mid-stroke (e.g. the window deactivates), discard the stroke
DrawCanvas.PointerCaptureLost += (_, _) => ViewModel?.Session?.PointerCanceled();
```

Notice four rules that all matter. Set `e.Handled = true` on moves - an unhandled move bubbles to the window manager, which then drags the window instead of driving your scene. Capture on press and release on release, or a drag that leaves the element stops delivering moves. Handle capture-lost as well as release, or a stroke stays half open when the window deactivates. And pass the current view size with every point where the model works in its own logical space, so a resize does not shift the geometry.

Wheel and orbit follow the same shape:

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/ModelSceneGlCanvas.cs
private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
{
    if (!_dragging) { return; }

    var position = e.GetCurrentPoint(this).Position;
    var deltaYaw = (float)(position.X - _lastX) * OrbitDegreesPerPixel;
    var deltaPitch = (float)(position.Y - _lastY) * OrbitDegreesPerPixel;
    _lastX = position.X;
    _lastY = position.Y;

    // Grab-and-drag feel: dragging right rolls the model's near face to the right, and
    // dragging up rolls its top toward you. Invalidate coalesces to one paint per frame.
    _renderer.Camera.Orbit(-deltaYaw, deltaPitch);
    Invalidate();
    e.Handled = true;
}

private void OnPointerCaptureLost(object sender, PointerRoutedEventArgs e) => _dragging = false;

private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
{
    var delta = e.GetCurrentPoint(this).Properties.MouseWheelDelta;
    _renderer.Camera.Zoom(delta > 0 ? 0.9f : 1.1f);
    Invalidate();
    e.Handled = true;
}
```

Handle a modified wheel and leave an unmodified one alone, so the surrounding scroll viewer still pans.

### Convert to canvas pixels

Pointer positions arrive in device-independent units while a canvas may render in pixels. Scale, or the input drifts from the image at any display scaling other than 100%:

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs
// Maps a pointer position (in view/DIP units) to the canvas's pixel space, so pointer
// input stays aligned with the rendered pixels at any DPI and after any window resize
private (double X, double Y) ToCanvasPixels(Point position)
{
    var canvasSize = DisplayCanvas.CanvasSize;
    var scaleX = DisplayCanvas.ActualWidth > 0 && canvasSize.Width > 0
        ? canvasSize.Width / DisplayCanvas.ActualWidth : 1.0;
    var scaleY = DisplayCanvas.ActualHeight > 0 && canvasSize.Height > 0
        ? canvasSize.Height / DisplayCanvas.ActualHeight : 1.0;
    return (position.X * scaleX, position.Y * scaleY);
}
```

`SizeChanged` also has to request a render, or the last frame keeps its old letterbox.

### Keep the model free of UI types

A model that must be unit-testable headless never sees a platform event type. Translate at the canvas:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs
private ToolMouseEventArgs BuildMouseArgs (PointerRoutedEventArgs e)
{
    var point = e.GetCurrentPoint (this);
    PointD viewPoint = new (point.Position.X, point.Position.Y);
    PointD canvasPoint = document?.Workspace.ViewPointToCanvas (viewPoint) ?? viewPoint;

    MouseButton button = MouseButton.None;
    var props = point.Properties;
    if (props.IsLeftButtonPressed)
        button = MouseButton.Left;
    else if (props.IsRightButtonPressed)
        button = MouseButton.Right;
    else if (props.IsMiddleButtonPressed)
        button = MouseButton.Middle;

    return new ToolMouseEventArgs {
        State = InputMapper.ToModifierType (e.KeyModifiers, props),
        MouseButton = button,
        PointDouble = canvasPoint,
        WindowPoint = viewPoint,
        RootPoint = viewPoint,
    };
}

private void OnCanvasPointerReleased (object sender, PointerRoutedEventArgs e)
{
    if (document is null)
        return;
    // The pressed-button flags are cleared by release time; recover the
    // released button from the update kind.
    ToolMouseEventArgs args = BuildMouseArgs (e);
    var kind = e.GetCurrentPoint (this).Properties.PointerUpdateKind;
    MouseButton released = kind switch {
        PointerUpdateKind.LeftButtonReleased => MouseButton.Left,
        PointerUpdateKind.RightButtonReleased => MouseButton.Right,
        PointerUpdateKind.MiddleButtonReleased => MouseButton.Middle,
        _ => args.MouseButton,
    };
    // ...
    ReleasePointerCapture (e.Pointer);
    PintaCore.Tools.DoMouseUp (document, args);
    e.Handled = true;
}
```

Notice the release path. The pressed-button flags are already cleared by the time the release arrives, so the released button has to be recovered from the update kind or every release reports `None`.

## Fonts and text

Set the application's font from a package rather than trusting the host machine. `Segoe UI` is the framework default and is not present on Linux or macOS, and an embedded device may have no installed fonts at all.

### Choose an application font

One assignment in the `App` constructor, before `InitializeComponent()`:

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/App.xaml.cs
        //Set Roboto as the default font for all text in the application
        global::CodeBrix.Platform.UI.FeatureConfiguration.Font.DefaultTextFontFamily =
            "ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/Roboto.ttf";

        //Fonts consulted for characters the default font has no glyph for
        global::CodeBrix.Platform.UI.FeatureConfiguration.Font.FallbackFontFamilies =
        [
            "ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/NotoSansArmenian.ttf",
            "ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/NotoSansGeorgian.ttf",
        ];
```

Notice the first path segment of each URI: it is the font assembly's name, without the package ID's license suffix. Fallback entries name the plain, weight-less face files. Both properties must be set before the first text is measured, which is why they belong in the `App` constructor; [04 - Project architecture](04-project-architecture.md) collects that timing rule with the rest of the startup order.

Publish the same family under a resource key so pages can name it, and bind with `{StaticResource}` rather than repeating the URI:

```xml
<Application
    x:Class="MyApp.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <ResourceDictionary>

            <FontFamily x:Key="OpenSansFont">ms-appx:///CodeBrix.Platform.Fonts.OpenSans/Fonts/OpenSans.ttf</FontFamily>

        </ResourceDictionary>
    </Application.Resources>
</Application>
```

```xml
<TextBlock Text="Hello, world."
           FontFamily="{StaticResource OpenSansFont}"
           FontWeight="SemiBold" />
```

Preload the face so the first screen does not re-layout when the font arrives:

```csharp
await FontFamilyHelper.PreloadAsync(
    "ms-appx:///CodeBrix.Platform.Fonts.OpenSans/Fonts/OpenSans.ttf",
    Windows.UI.Text.FontWeights.Normal, Windows.UI.Text.FontStretch.Normal,
    Windows.UI.Text.FontStyle.Normal);
```

`FontFamilyHelper` lives in `CodeBrix.Platform.UI.Xaml.Media` and also offers `PreloadAllFontsInManifest(Uri)`, taking the URI of the font itself, for a package that ships a manifest.

> [!WARNING]
> Never append a `#FamilyName` fragment to a text font's URI. The framework strips the fragment during resolution, so it never helps - and on the value assigned to `DefaultTextFontFamily` it silently disables the startup manifest preload, because the `.manifest` suffix the preload appends lands inside the fragment and is dropped. Text still renders; weight, style and stretch requests quietly stop resolving to the right face.

### The font packages

Each package is content only: it exposes no types and nothing to `using`, and referencing it is the whole integration. The font files are contributed to the application's assets, so do not add `<Content>` items or copy `.ttf` files into your project yourself - a hand-rolled copy duplicates assets and can shadow the manifest lookup.

| Package | What it supplies |
| --- | --- |
| [CodeBrix.Platform.Fonts.OpenSans.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.OpenSans.ApacheLicenseForever) | Open Sans, a variable font plus static instances. Latin including Vietnamese, modern monotonic Greek, essentially the whole base Cyrillic block, and Hebrew. No companion families |
| [CodeBrix.Platform.Fonts.Roboto.OflLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.Roboto.OflLicenseForever) | Roboto, plus three Noto Sans companions inside the same package for polytonic Greek, Armenian and Georgian |
| [CodeBrix.Platform.Fonts.RobotoMono.OflLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.RobotoMono.OflLicenseForever) | Roboto Mono for monospaced text, with Noto Sans Mono, Iosevka and Noto Sans Georgian as companions |
| [CodeBrix.Platform.Fonts.Merriweather.OflLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.Merriweather.OflLicenseForever) | Merriweather, a serif family, with three Noto Serif companions for Greek, Armenian and Georgian |
| [CodeBrix.Platform.Fonts.NotoMusic.OflLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.NotoMusic.OflLicenseForever) | Noto Music: clefs, noteheads, rests, accidentals, dynamics, articulations and other notation symbols. Reference it alongside a text font, never as the application's default |
| [CodeBrix.Platform.Fonts.Fluent.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.Fluent.ApacheLicenseForever) | The symbols font behind `SymbolIcon`, `FontIcon` and the `SymbolThemeFontFamily` theme resource. Not a text face |

A companion family is a file inside the same package, not a second package to reference. Name a companion directly on the elements that carry that script, or list it in `FallbackFontFamilies` and let the framework consult it:

```xml
<StackPanel>

    <!-- Polytonic (ancient) Greek. Roboto has no glyph for any of these
         codepoints, so naming NotoSans is not an optimisation — it is
         the difference between text and a row of tofu boxes. -->
    <TextBlock Text="μῆνιν ἄειδε θεὰ Πηληϊάδεω Ἀχιλῆος"
               FontFamily="ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/NotoSans.ttf" />

    <!-- Noto Sans is the one companion with real italics:
         {Italic, 400, Normal} -> NotoSans-Italic.ttf -->
    <TextBlock Text="ἔπεα πτερόεντα"
               FontFamily="ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/NotoSans.ttf"
               FontStyle="Italic" />

    <!-- Armenian, SemiBold: the companion manifest entry
         {Normal, 600, Normal} -> NotoSansArmenian-SemiBold.ttf -->
    <TextBlock Text="Armenian heading"
               FontFamily="ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/NotoSansArmenian.ttf"
               FontWeight="SemiBold" />

    <!-- Georgian, Regular -->
    <TextBlock Text="Georgian body text"
               FontFamily="ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/NotoSansGeorgian.ttf" />

</StackPanel>
```

Notice the rule the comments imply: the Armenian and Georgian companions are upright, normal-stretch faces only, so do not ask them for italics or a stretch. Each font package's library page carries its own coverage detail - [Open Sans](../libraries/CodeBrix.Platform.Fonts.OpenSans.md), [Roboto](../libraries/CodeBrix.Platform.Fonts.Roboto.md), [Roboto Mono](../libraries/CodeBrix.Platform.Fonts.RobotoMono.md), [Merriweather](../libraries/CodeBrix.Platform.Fonts.Merriweather.md), [Noto Music](../libraries/CodeBrix.Platform.Fonts.NotoMusic.md) and [Fluent](../libraries/CodeBrix.Platform.Fonts.Fluent.md).

### Select a face by property, not by file name

Each text font package ships a manifest beside its main `.ttf`, mapping a `{FontStyle, FontWeight, FontStretch}` triple to a static instance. Name the family once and set the XAML text properties; the framework does the lookup:

```xml
<StackPanel>

    <!-- Regular 400, upright, Normal stretch: the manifest entry for
         {Normal, 400, Normal} resolves to OpenSans-Regular.ttf -->
    <TextBlock Text="Hello, world."
               FontFamily="ms-appx:///CodeBrix.Platform.Fonts.OpenSans/Fonts/OpenSans.ttf" />

    <!-- Bold italic: {Italic, 700, Normal} -> OpenSans-BoldItalic.ttf -->
    <TextBlock Text="Bold italic sample"
               FontFamily="ms-appx:///CodeBrix.Platform.Fonts.OpenSans/Fonts/OpenSans.ttf"
               FontWeight="Bold"
               FontStyle="Italic" />

    <!-- SemiCondensed SemiBold: {Normal, 600, SemiCondensed}
         -> OpenSans_SemiCondensed-SemiBold.ttf -->
    <TextBlock Text="Narrow heading"
               FontFamily="ms-appx:///CodeBrix.Platform.Fonts.OpenSans/Fonts/OpenSans.ttf"
               FontWeight="SemiBold"
               FontStretch="SemiCondensed" />

    <!-- Mixed runs inside one paragraph -->
    <TextBlock FontFamily="ms-appx:///CodeBrix.Platform.Fonts.OpenSans/Fonts/OpenSans.ttf">
        <Run Text="Light " FontWeight="Light" />
        <Run Text="Medium " FontWeight="Medium" />
        <Run Text="ExtraBold " FontWeight="ExtraBold" />
        <Run Text="Condensed italic"
             FontStyle="Italic"
             FontStretch="Condensed" />
    </TextBlock>

</StackPanel>
```

Pinning one exact static file works and is occasionally what you want, but it is fragile: a head that does not support the manifest prunes the static instances from the build, and the pinned file is then not deployed. The plain family URI is never pruned.

Prefer one family URI plus the three properties over a different file URI per face for a second reason too: every distinct font URI is a separate font resource to load and cache, and declaring the family once as an application resource makes a later font swap a single edit.

### What a missing glyph looks like

A character no font in the chain can supply renders as that font's own `.notdef` glyph, and what that looks like is a property of the font. The framework never substitutes the host machine's fonts unless `FallbackFontFamilies` is left empty and the application's own fonts have no glyph:

- Open Sans has an **empty** `.notdef`, so an uncovered codepoint is an invisible blank gap. Plan for silent gaps rather than boxes when auditing coverage.
- Roboto and its three companions draw a `.notdef` **box**, so coverage gaps are obvious on screen.
- Merriweather and its companions draw a `.notdef` **box**, so missing text shows as boxes rather than disappearing.
- Noto Music draws a `.notdef` **box**.
- The Fluent symbols font does not fall back to a system icon font: a codepoint that is not in it renders as that font's missing-glyph mark, by design.

`FeatureConfiguration.Font.RestrictToEmbeddedFonts` confines resolution to the fonts the application ships, which is the setting to reach for when you want the rendering to be identical everywhere.

### Icons

Never put a literal symbol character in a text element for an icon. `FontIcon` and `SymbolIcon` resolve through the symbols font the application ships, so they render on a device with no system fonts at all:

```xml
<!-- From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml -->
<!-- FontIcon resolves through the Fluent symbols font that every
     CodeBrix.Platform application ships, so it renders on a device
     that has no system fonts at all. A literal symbol character
     here would depend on the host's fonts and come out as a
     missing-glyph box on an embedded frame-buffer device. -->
<FontIcon Glyph="&#xE82C;" FontSize="30"
          Foreground="#262B34"
          HorizontalAlignment="Center" VerticalAlignment="Center"
          Visibility="{d:Binding Thumbnail, Converter={StaticResource VisibleWhenNull}}" />
<Image Source="{d:Binding Thumbnail}" Stretch="UniformToFill" />
```

With the Fluent package referenced, both of these draw the right icon with nothing else written:

```xml
<SymbolIcon Symbol="Setting" />
<FontIcon Glyph="&#xE713;" />
```

`SymbolIcon` does not draw the `Symbol` enum's own numeric value: it runs the value through a translation table that moves the legacy codepoints to their modern equivalents, so the icon matches current iconography. `Symbol.Setting` is U+E115, and `SymbolIcon` draws U+E713. When you want a specific codepoint, use `FontIcon` and check that it renders - coverage is dense but not contiguous, so never compute a glyph by adding an offset to a known one.

For code that must be independent of whatever the application's default symbols font happens to be, name the font on the element:

```xml
<FontIcon
    FontFamily="ms-appx:///CodeBrix.Platform.Fonts.Fluent/Fonts/uno-fluentui-assets.ttf"
    Glyph="&#xE713;"
    FontSize="20" />
```

An icon inside a button, and a glyph inside ordinary text, are the same idea:

```xml
<Button>
    <StackPanel Orientation="Horizontal" Spacing="6">
        <SymbolIcon Symbol="Save" />
        <TextBlock Text="Save" />
    </StackPanel>
</Button>

<TextBlock>
    <Run Text="Press " />
    <Run FontFamily="ms-appx:///CodeBrix.Platform.Fonts.Fluent/Fonts/uno-fluentui-assets.ttf"
         Text="&#xE713;" />
    <Run Text=" to open settings." />
</TextBlock>
```

To point the whole application at your own icon font, either assign `FeatureConfiguration.Font.SymbolsFont` **after** `InitializeComponent()` - the one font property that is set late rather than early - or override the theme resource in all three theme dictionaries:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.ThemeDictionaries>
            <ResourceDictionary x:Key="Default">
                <FontFamily x:Key="SymbolThemeFontFamily">ms-appx:///MyApp/Assets/Fonts/my-icons.ttf</FontFamily>
            </ResourceDictionary>
            <ResourceDictionary x:Key="Light">
                <FontFamily x:Key="SymbolThemeFontFamily">ms-appx:///MyApp/Assets/Fonts/my-icons.ttf</FontFamily>
            </ResourceDictionary>
            <ResourceDictionary x:Key="HighContrast">
                <FontFamily x:Key="SymbolThemeFontFamily">ms-appx:///MyApp/Assets/Fonts/my-icons.ttf</FontFamily>
            </ResourceDictionary>
        </ResourceDictionary.ThemeDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

Mirror all three dictionaries or a theme switch loses your value.

Keep the codepoints a view model picks at run time in one named constants class in the shared library, rather than as escapes scattered through the markup and the code. And when a design is about to depend on the symbols font behaving on a head you have not shipped on before, prove it first: the smallest page that can answer the question, run on that head, looked at with your own eyes, and the answer written down where it outlives the session. [Draw every icon from the shipped symbols font](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-ThemingAndStyling.md#draw-every-icon-from-the-shipped-symbols-font) and [Prove a platform capability with a throwaway page before designing around it](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-ThemingAndStyling.md#prove-a-platform-capability-with-a-throwaway-page-before-designing-around-it).

### Text shaping on Windows and macOS

Line breaking, bidirectional resolution and the rest of the Unicode property tables come from ICU. Linux distributions ship their own, and a Linux head uses it. Windows and macOS do not, so a head for those operating systems references the package that carries it: [CodeBrix.Platform.Unicode.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Unicode.ApacheLicenseForever) for a Windows head, [CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever) for a macOS head. Neither has a managed API: the reference is the whole integration, and the data archive is delivered into the build automatically. Referencing both from one project is supported and delivers the archive exactly once. See [CodeBrix.Platform.Unicode](../libraries/CodeBrix.Platform.Unicode.md) for the delivery routes and the test-project case.

### When you need text geometry rather than a text control

An editor or a drawing tool needs caret rectangles, selection rectangles, cluster hit-testing and outline paths, none of which a `TextBlock` exposes. [CodeBrix.Platform.TextLayout.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.TextLayout.ApacheLicenseForever) gives you the same engine that lays out every `TextBlock`, with no XAML involved, so the wrapper can live in a headless library:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/Re-editable/Text/TextLayout.cs
private TextLayoutResult BuildResult ()
{
    string text = engine.ToString ();
    is_empty = text.Length == 0;

    FontDescription font = engine.Font;

    TextFontWeight weight = (TextFontWeight) Math.Clamp (font.Weight / 100 * 100, 100, 900);

    TextRunDescriptor run = new (
        is_empty ? " " : text,
        font.Family,
        (float) Math.Max (1.0, font.Size),
        weight,
        font.Italic ? TextFontStyle.Italic : TextFontStyle.Normal);

    TextAlign alignment = engine.Alignment switch {
        TextAlignment.Center => TextAlign.Center,
        TextAlignment.Right => TextAlign.Right,
        _ => TextAlign.Left,
    };

    TextLayoutResult first = TextLayoutEngine.Layout ([run], null);

    if (alignment == TextAlign.Left || is_empty)
        return first;

    float width = first.Size.Width;
    first.Dispose ();

    return TextLayoutEngine.Layout ([run], new TextLayoutOptions {
        MaxWidth = width,
        Alignment = alignment,
    });
}
```

Notice the two-pass alignment: alignment does nothing without a width, so a non-left alignment measures the natural width first and lays out again at that width - and disposes the first result. The incoming font weight is clamped onto the engine's own 100 to 900 scale. Empty text is laid out as a single space so caret and line metrics stay meaningful, with a private flag remembering the truth. Indices are .NET character indices, so a surrogate pair is two of them. Layout results are disposable and cached; drop the cache whenever the text model reports a change. The [TextLayout add-in page](add-ins/TextLayout.md) has the full API, and [CodeBrix.Platform.AdvancedTextEdit.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.AdvancedTextEdit.ApacheLicenseForever) is a complete code editor built on it - see the [AdvancedTextEdit add-in page](add-ins/AdvancedTextEdit.md).

## Splitting a page code-behind

The right answer to a large code-behind is a view model. Where that is genuinely not possible - shell wiring that touches named elements - partial files grouped by concern keep it navigable. The shared items project does not glob, so every partial must be listed by hand:

```xml
<!-- From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Pinta.Brix.UI.projitems -->
<Compile Include="$(MSBuildThisFileDirectory)Views\MainPage.xaml.cs">
  <DependentUpon>MainPage.xaml</DependentUpon>
</Compile>
<Compile Include="$(MSBuildThisFileDirectory)Views\MainPage.Menus.cs">
  <DependentUpon>MainPage.xaml</DependentUpon>
</Compile>
<Compile Include="$(MSBuildThisFileDirectory)Views\MainPage.Actions.cs">
  <DependentUpon>MainPage.xaml</DependentUpon>
</Compile>
<Compile Include="$(MSBuildThisFileDirectory)Views\MainPage.Dialogs.cs">
  <DependentUpon>MainPage.xaml</DependentUpon>
</Compile>
<Compile Include="$(MSBuildThisFileDirectory)Views\MainPage.Palette.cs">
  <DependentUpon>MainPage.xaml</DependentUpon>
</Compile>
```

Notice `DependentUpon`, which nests the partials under the page in a solution view. A file left out of the list is silently not compiled. Give each partial a header comment saying what it holds and, where it matters, why it is not somewhere else.

## Every recipe and the file that shows it

| Recipe | Shown by |
| --- | --- |
| Declare a page and bind it to a view model | [PdfSideBySide MainPage](https://github.com/ellisnet/CodeBrix.Samples/blob/main/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml) |
| Scope a region to a child view model | [PdfSideBySide MainPage](https://github.com/ellisnet/CodeBrix.Samples/blob/main/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml) |
| Re-key dialog and picker brushes at application level | [NotionDocumentCreator App.xaml](https://github.com/ellisnet/CodeBrix.Samples/blob/main/NotionDocumentCreator/src/NotionDocumentCreator.UI/App.xaml) |
| Re-key accent and list-selection brushes | [PolyHavenBrowser MainPage](https://github.com/ellisnet/CodeBrix.Samples/blob/main/PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml) |
| A palette as plain data, with no drawing type | [GitHubIssueFinder ColorSchemes](https://github.com/ellisnet/CodeBrix.Samples/blob/main/GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/ColorSchemes.cs) |
| Re-key every control family from one role table | [GitHubIssueFinder SchemeBrushMap](https://github.com/ellisnet/CodeBrix.Samples/blob/main/GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/SchemeBrushMap.cs) |
| Switch color schemes by re-pointing keyed brushes | [GitHubIssueFinder MainPage code-behind](https://github.com/ellisnet/CodeBrix.Samples/blob/main/GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml.cs) |
| Format a value with an `IValueConverter` | [CodeBrixVideoTool TimecodeConverter](https://github.com/ellisnet/CodeBrix.Samples/blob/main/CodeBrixVideoTool/src/CodeBrixVideoTool.Core/Converters/TimecodeConverter.cs) |
| Map a bool to a style | [PolyHavenBrowser_viewer_only BoolToAccentStyleConverter](https://github.com/ellisnet/CodeBrix.Samples/blob/main/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Converters/BoolToAccentStyleConverter.cs) |
| Switch a page between two modes with one bool | [PalmVisualizer MainPage](https://github.com/ellisnet/CodeBrix.Samples/blob/main/PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml) |
| Wrap a header with FlexPanel | [KenneyAssetBrowser MainPage](https://github.com/ellisnet/CodeBrix.Samples/blob/main/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml) |
| Flip the main axis on orientation | [PolyHavenBrowser MainPage code-behind](https://github.com/ellisnet/CodeBrix.Samples/blob/main/PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs) |
| Load an icon from an embedded resource | [JustBetweenUs EmbeddedImage](https://github.com/ellisnet/CodeBrix.Samples/blob/main/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.Core/Controls/EmbeddedImage.cs) |
| An image-and-text button | [JustBetweenUs EmbeddedImageButton](https://github.com/ellisnet/CodeBrix.Samples/blob/main/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.Core/Controls/EmbeddedImageButton.cs) |
| A drawn widget with hit testing | [Pinta.Brix PaletteWidget](https://github.com/ellisnet/CodeBrix.Samples/blob/main/Pinta.Brix/src/libs/Pinta.Brix.Controls/Palette/PaletteWidget.cs) |
| A splitter bar | [Pinta.Brix ThumbSplitter](https://github.com/ellisnet/CodeBrix.Samples/blob/main/Pinta.Brix/src/libs/Pinta.Brix.Controls/ThumbSplitter.cs) |
| A drag gesture on a drawn scene, settled by the view model | [InannaRosette MainPage code-behind](https://github.com/ellisnet/CodeBrix.Samples/blob/main/InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs) |
| A control face drawn in code at a fixed design size | [InannaRosette CardView](https://github.com/ellisnet/CodeBrix.Samples/blob/main/InannaRosette/src/InannaRosette.UI/Controls/CardView.xaml.cs) |
| A whole design system in one application resource dictionary | [InannaRosette App.xaml](https://github.com/ellisnet/CodeBrix.Samples/blob/main/InannaRosette/src/InannaRosette.UI/App.xaml) |
| A modeless options panel | [Pinta.Brix FloatingDialogHost](https://github.com/ellisnet/CodeBrix.Samples/blob/main/Pinta.Brix/src/libs/Pinta.Brix.Controls/FloatingDialogHost.cs) |
| A toolbar rendered from descriptors | [Pinta.Brix ToolBarRenderer](https://github.com/ellisnet/CodeBrix.Samples/blob/main/Pinta.Brix/src/libs/Pinta.Brix.Controls/ToolBarRenderer.cs) |
| An options panel generated by reflection | [Pinta.Brix EffectOptionsDialog](https://github.com/ellisnet/CodeBrix.Samples/blob/main/Pinta.Brix/src/libs/Pinta.Brix.Controls/EffectOptionsDialog.cs) |
| Menus built from a command model | [Pinta.Brix CommandMenuBuilder](https://github.com/ellisnet/CodeBrix.Samples/blob/main/Pinta.Brix/src/libs/Pinta.Brix.Controls/Menus/CommandMenuBuilder.cs) |
| Keyboard shortcuts from one handler | [Pinta.Brix CommandAcceleratorTable](https://github.com/ellisnet/CodeBrix.Samples/blob/main/Pinta.Brix/src/libs/Pinta.Brix.Controls/Input/CommandAcceleratorTable.cs) |
| Run a command on Enter in a text box | [WikipediaPublisher MainPage code-behind](https://github.com/ellisnet/CodeBrix.Samples/blob/main/WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml.cs) |
| The editor shell: tabs, toolbox, pads, status bar | [Pinta.Brix MainPage](https://github.com/ellisnet/CodeBrix.Samples/blob/main/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml) |
| Forward pointer input into a model | [PainDiagram MainPage code-behind](https://github.com/ellisnet/CodeBrix.Samples/blob/main/PainDiagram/CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml.cs) |
| Orbit and zoom a 3D scene | [PolyHavenBrowser ModelSceneGlCanvas](https://github.com/ellisnet/CodeBrix.Samples/blob/main/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/ModelSceneGlCanvas.cs) |
| Translate pointer events into a headless input model | [Pinta.Brix PintaCanvas](https://github.com/ellisnet/CodeBrix.Samples/blob/main/Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs) |
| A default font and script fallbacks | [PdfSideBySide App.xaml.cs](https://github.com/ellisnet/CodeBrix.Samples/blob/main/PdfSideBySide/src/PdfSideBySide.UI/App.xaml.cs) |
| Icons that survive on a device with no fonts | [PolyHavenBrowser MainPage](https://github.com/ellisnet/CodeBrix.Samples/blob/main/PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml) |
| Text geometry with no text control | [Pinta.Brix TextLayout](https://github.com/ellisnet/CodeBrix.Samples/blob/main/Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/Re-editable/Text/TextLayout.cs) |
| Split a page code-behind into partials | [Pinta.Brix MainPage.Menus](https://github.com/ellisnet/CodeBrix.Samples/blob/main/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Menus.cs) |

## Checklist

- [ ] Every bound class carries `[Microsoft.UI.Xaml.Data.Bindable]`, and a page written with `clr-namespace:` namespaces binds with `{d:Binding}`
- [ ] Page wiring is subscribed in `DataContextChanged` before `InitializeComponent()`, which is the last line of the constructor
- [ ] `XamlControlsResources` is merged in `App.xaml`, and every brush key you override is declared after it
- [ ] Dialog, picker and on-screen-keyboard keys are at application level; page-only keys are in `Page.Resources`
- [ ] Every re-keyed control family has its full set of state keys - normal, pointer-over, pressed and disabled - and a text control also sets `PlaceholderForeground` on the element
- [ ] `Application.RequestedTheme` is set only in the `App` constructor; run-time switching uses `FrameworkElement.RequestedTheme`
- [ ] A converter that answers a yes-or-no question is registered twice, the second with `Invert="True"`
- [ ] Converters return a safe default instead of throwing, and format with the invariant culture where separators are fixed
- [ ] Groups that must wrap together are one child of the `FlexPanel`; `Grow` and `Basis` are set on children
- [ ] Pointer handlers capture on press, release on release, handle capture-lost, and set `e.Handled = true` on moves
- [ ] Keyboard shortcuts are dispatched from one page handler added with `handledEventsToo: true`
- [ ] No commands live in an operating-system header bar if the frame-buffer head is shipped
- [ ] `DefaultTextFontFamily` and `FallbackFontFamilies` are set in the `App` constructor; `SymbolsFont` after `InitializeComponent()`
- [ ] No font URI carries a `#FamilyName` fragment
- [ ] Icons are `FontIcon` or `SymbolIcon` glyphs, never literal symbol characters in a text element
- [ ] A Windows head references the Unicode package and a macOS head the macOS Unicode package
- [ ] Every partial page file is listed in the `.projitems`

---

**Where to go next**

- [07 - Platform services](07-platform-services.md) - the next chapter: what the page hands the view model so dialogs, pickers and the clipboard work
- [05 - MVVM the right way](05-mvvm-the-right-way.md) - the properties and commands this markup binds against
- [FlexPanel](add-ins/FlexPanel.md) and [CommandBar](add-ins/CommandBar.md) add-ins - the full reflowing-layout property set, and the tool bar family the command model above drives
- [Reference applications](../samples/README.md) - the complete pages every sample here comes from
