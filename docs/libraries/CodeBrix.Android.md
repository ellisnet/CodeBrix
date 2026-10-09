<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Android</sub>

# CodeBrix.Android

**CodeBrix.Android builds CodeBrix.Platform applications as native Android applications: the pages,
view models and code-behind you wrote for CodeBrix.Platform compile unchanged for Android, and every
element is shown with a real Android or Material Components view.** It is desktop-first - it targets
Android desktops such as Googlebooks, the Android laptops from Google and its partners, and phones
docked in desktop mode - and treats tablets and phones as first-class targets too, adapting each page
to the window size it runs in. This page is the reference card; the chapter that walks through adding
an Android head is [Android and Googlebooks](../platform/15-android.md).

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Android](https://github.com/ellisnet/CodeBrix.Android) |
| **Packages** | <details><summary>The framework package and one package per add-in</summary><strong>Framework</strong><br>[`CodeBrix.Android.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Android.ApacheLicenseForever)<br><br><strong>Add-ins</strong><br>[`CodeBrix.Android.AdvancedTextEdit.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Android.AdvancedTextEdit.ApacheLicenseForever)<br>[`CodeBrix.Android.AppSettings.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Android.AppSettings.ApacheLicenseForever)<br>[`CodeBrix.Android.AudioPlayer.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Android.AudioPlayer.ApacheLicenseForever)<br>[`CodeBrix.Android.CommandBar.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Android.CommandBar.ApacheLicenseForever)<br>[`CodeBrix.Android.FlexPanel.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Android.FlexPanel.ApacheLicenseForever)<br>[`CodeBrix.Android.Graphics2DSK.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Android.Graphics2DSK.ApacheLicenseForever)<br>[`CodeBrix.Android.Graphics3DGL.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Android.Graphics3DGL.ApacheLicenseForever)<br>[`CodeBrix.Android.Lottie.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Android.Lottie.ApacheLicenseForever)<br>[`CodeBrix.Android.MediaPlayer.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Android.MediaPlayer.ApacheLicenseForever)<br>[`CodeBrix.Android.PlotterView.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Android.PlotterView.ApacheLicenseForever)<br>[`CodeBrix.Android.SkiaSharp.Views.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Android.SkiaSharp.Views.ApacheLicenseForever)<br>[`CodeBrix.Android.Svg.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Android.Svg.ApacheLicenseForever)<br>[`CodeBrix.Android.TerminalView.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Android.TerminalView.ApacheLicenseForever)<br>[`CodeBrix.Android.TextLayout.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Android.TextLayout.ApacheLicenseForever)<br>[`CodeBrix.Android.VideoPlayer.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Android.VideoPlayer.ApacheLicenseForever)<br>[`CodeBrix.Android.WebView.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Android.WebView.ApacheLicenseForever)</details> |
| **License** | Apache 2.0, every package; see [License](#license) |
| **Requires** | .NET 10 or later with the `android` workload, and the Android SDK |
| **Use it from** | The Android head of a CodeBrix.Platform application, and the `net10.0-android36.1` target of its `.Core` library |
| **Platforms** | Android 13 (API level 33) and later: Googlebooks and other Android desktops, phones docked in desktop mode, tablets and phones |

## What it does

- Compiles CodeBrix.Platform XAML and code-behind unchanged for Android ("paste always"), with the
  same XAML dialect as a CodeBrix.Platform desktop build.
- Hosts a CodeBrix.Platform application in an Android activity: `Application`, `OnLaunched`, `Window`
  and `Frame` navigation run unchanged, edge to edge, with resizing, docking and theme changes handled
  without restarting the activity.
- Carries the CodeBrix.Platform object model - dependency properties, bindings, resources, styles,
  templates, `Frame` and `Page` - laid out by the same layout engine as on the desktop.
- Shows every element as a native view: Material 3 buttons, check boxes, switches, sliders, progress
  indicators, text boxes, lists and grids on RecyclerView, tabs, navigation bars, rails and drawers,
  app bars, dialogs, menus, flyouts, date, time and color pickers, info bars and badges, with shapes,
  borders and gradients drawn natively.
- Adapts by window size class, live: a `NavigationView` becomes a bottom bar, a rail or a drawer, a
  `TriPaneView` shows one, two or three panes, and dialogs and menus change form when a phone docks or
  a window is resized.
- Themes with Material 3 from the application's own Fluent resources, light and dark, with dynamic
  color and the user's font scale.
- Routes touch, mouse, stylus and hardware-keyboard input through the CodeBrix.Platform routed events,
  keyboard accelerators and focus, and handles the Android back button and predictive back gesture.
- Maps `AutomationProperties.Name` to the content description TalkBack speaks and UI Automator matches.
- Implements the file pickers (Storage Access Framework), the clipboard, sharing, the launcher,
  connectivity, haptics, `ApplicationData`, preferred languages, `SoftwareBitmap` with PNG/JPEG
  encoding, and device and display information.
- Packs application and library assets addressed by `ms-appx:///` URIs, including the fonts of the
  CodeBrix font packages, and resolves every font family to the application's own font file - never to
  a system font.
- Logs to logcat through `Microsoft.Extensions.Logging`.
- Reports, as build-time warnings that never fail a build, the CodeBrix.Platform constructs Android
  accepts and ignores (the CBAND diagnostics), at the C# or XAML line that uses them.
- Provides an Android package for every CodeBrix.Platform add-in.

## When to use it

Reach for CodeBrix.Android when a CodeBrix.Platform application should also run on Android - above all
on Googlebooks and docked phones, where the desktop layout the application already has is the right
one, and on tablets and phones, where the adaptive controls fold it down. The application keeps one
`.Core`, one `.UI` and its view models; Android is one more head.

It is not the tool for an iOS application, for Android versions below API 33, or for a
CodeBrix.Platform application whose main surface is the GameEngine canvas, camera capture or TkCanvas -
none of those is supported on Android. A .NET MAUI application that wants to share view models with a
CodeBrix.Platform application uses the native-framework toolkit instead; see
[Sharing code with native frameworks](../platform/14-sharing-code-with-native-frameworks.md).

## Getting started

In the Android head project:

```bash
dotnet add package CodeBrix.Android.ApacheLicenseForever
```

A minimal Android head for an existing application is a project file and two classes:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-android36.1</TargetFramework>
    <SupportedOSPlatformVersion>33</SupportedOSPlatformVersion>
    <OutputType>Exe</OutputType>
    <ApplicationId>com.example.myapp</ApplicationId>
  </PropertyGroup>
  <!-- The app's shared XAML (App.xaml, Views/) is imported, not copied. -->
  <Import Project="..\MyApp.UI\MyApp.UI.projitems" Label="Shared" />
  <ItemGroup>
    <ProjectReference Include="..\MyApp.Core\MyApp.Core.csproj" />
    <PackageReference Include="CodeBrix.Android.ApacheLicenseForever" />
  </ItemGroup>
</Project>
```

```csharp
using Android.App;
using Android.Runtime;
using CodeBrix.Android.UI.Hosting;
using System;

namespace MyApp;

[Application]
public class MainApplication : CodeBrixApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer) { }

    protected override Microsoft.UI.Xaml.Application CreateApp() => new App();
}

[Activity(MainLauncher = true, Theme = "@style/Theme.Material3.DayNight.NoActionBar",
          ConfigurationChanges = CodeBrixActivity.HandledConfigurationChanges)]
public class MainActivity : CodeBrixActivity
{
}
```

`.Core` targets both frameworks and picks the UI package per target, so the desktop package never
reaches the APK:

```xml
<TargetFrameworks>net10.0;net10.0-android36.1</TargetFrameworks>
<ItemGroup Condition="'$(TargetFramework)' == 'net10.0'">
  <PackageReference Include="CodeBrix.Platform.ApacheLicenseForever" />
</ItemGroup>
<ItemGroup Condition="'$(TargetFramework)' == 'net10.0-android36.1'">
  <PackageReference Include="CodeBrix.Android.ApacheLicenseForever" />
</ItemGroup>
```

Deploy a Debug build to an emulator or a device with `dotnet build src/MyApp.Android -t:Install`, and
start it from the launcher. [Android and Googlebooks](../platform/15-android.md) has every file of the
template head, verbatim.

## Key concepts

### One head, two classes

The Android head has no `Program.cs`. `MainApplication : CodeBrixApplication` configures logging and
registers the Android implementation of every CodeBrix.Platform platform contract in `OnCreate`, before
any XAML type is used; its one required member is `CreateApp()`. `MainActivity : CodeBrixActivity`, an
`AppCompatActivity` with a `Theme.Material3.*` theme, starts the XAML application. The activity must
state `ConfigurationChanges = CodeBrixActivity.HandledConfigurationChanges` so that resizing, docking,
orientation, density and night-mode changes do not re-create it.

### Package IDs and namespaces

The framework package is `CodeBrix.Android.ApacheLicenseForever`. Each add-in package is named after
the add-in's CodeBrix.Platform package: `CodeBrix.Platform.Svg.ApacheLicenseForever` on the desktop,
`CodeBrix.Android.Svg.ApacheLicenseForever` on Android. Application code and XAML keep the
CodeBrix.Platform namespaces (`Microsoft.UI.Xaml.*`, `CodeBrix.Platform.UI`); only the head uses
`CodeBrix.Android.UI.Hosting`, `CodeBrix.Android.UI.Logging` and `CodeBrix.Android.UI.Diagnostics`.
Every package of one release depends on the others at exactly that version.

### Native views over the shared layout

CodeBrix.Android does not draw controls. The CodeBrix.Platform layout engine measures and arranges the
elements, and an element handler puts a native Android view exactly where each element was laid out.
Controls in their default style are Material widgets; a control with its own `ControlTemplate` is
expanded and shown with native views as written, without the widget's look, ripple and accessibility.
Every Fluent resource key the application defines wins over the Material defaults.

### Adaptive by window size class

The width size class - compact, medium, expanded - drives `NavigationView` (bottom bar, rail, drawer),
`TriPaneView` (one, two or three panes), the framework's `CommandBar` (bottom or top app bar) and the
form of dialogs and menus, and it switches live. `AnalyticsInfo.VersionInfo.DeviceFamily` reports
`"Android.Mobile"`, `"Android.Tablet"` or `"Android.Desktop"` from the current window every time it is
read.

### Compile constants

The package defines `HAS_CODEBRIX`, `HAS_CODEBRIX_WINUI`, `HAS_CODEBRIX_ANDROID`,
`__CODEBRIX_ANDROID__` and the other CodeBrix.Platform constants, and NOT `HAS_CODEBRIX_SKIA`,
`__CODEBRIX_SKIA__` or `__DESKTOP__`. Guard Android-only code with `#if HAS_CODEBRIX_ANDROID`.

### Build-time diagnostics

The CBAND analyzer ships inside the framework package and reports CBAND0001 to CBAND0008 - custom
templates on native controls, template members, composition and backdrops, 3-D projections,
`ScrollViewer` zoom, multi-character `PasswordChar`, frame-buffer options, acrylic and Mica - as
warnings that are kept out of `TreatWarningsAsErrors`.

## Examples

### A pasted page, unchanged

```xml
<Page x:Class="MyApp.Views.MainPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <StackPanel Spacing="16">
    <TextBlock Text="{Binding Greeting}" />
    <Button Content="Click me" Command="{Binding ClickCommand}" />
  </StackPanel>
</Page>
```

```csharp
public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
```

On Android the `TextBlock` is a native text view and the `Button` a Material button.

### Laying the page out above the soft keyboard

```csharp
public MainApplication(IntPtr handle, JniHandleOwnership transfer)
    : base(handle, transfer)
{
    SoftInputAdjust = SoftInputAdjust.Resize;   // the default, Pan, pans the window instead
}
```

### Logging levels and a visual-tree dump

```csharp
protected override bool LogVisualTreeAfterLayout => true;

protected override void ConfigureLogging(ILoggingBuilder builder)
{
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddFilter("CodeBrix.Android", LogLevel.Debug);
}
```

Read it with `adb logcat -s <package name>`.

### Choosing the native ComboBox

```xml
<ItemGroup>
  <RuntimeHostConfigurationOption Include="CodeBrix.Android.UI.NativeComboBox" Value="true" />
</ItemGroup>
```

## Pitfalls

- Never reference `CodeBrix.Platform.ApacheLicenseForever`, a CodeBrix.Platform head or a
  CodeBrix.Platform add-in package from the Android target framework: it brings the desktop Skia
  assemblies into the APK.
- Inside a namespace with a segment named `Android`, `Android` binds to that namespace. Write
  Mono.Android types as `global::Android.Views.View`, or alias them at the end of the using block.
- Do not set `<RootNamespace>` on the Android head; it collides with `.Core` in the XAML source
  generator.
- Leave `HandledConfigurationChanges` on the activity, or docking and resizing re-create it.
- Only `Content` items reach `ms-appx:///`; files under an Android `Assets/` folder are not packaged by
  convention, and their paths keep any `Assets/` prefix.
- A `ControlTemplate` on `Button`, `CheckBox`, `TextBox` and friends takes the control off its native
  widget.
- Graphics3DGL shaders must be GLSL ES (`#version 300 es`).
- WebView and MediaPlayer need the `INTERNET` permission in `AndroidManifest.xml`.
- Do not reference the desktop `CodeBrix.Audio.MitLicenseForever` package from the Android head; the
  AudioPlayer and VideoPlayer add-ins bring CodeBrix.Audio's Android backend,
  [CodeBrix.Audio.Android](CodeBrix.Audio.Android.md).
- Do not side-load a Debug APK with `adb install`; deploy with `-t:Install`.
- The PlayTest head does not cover Android and CodeBrix.Android ships no test package: test the shared
  code through `.Core`'s `net10.0` build and the desktop PlayTests, and the Android head on an emulator
  or device - see
  [Testing an Android head](../platform/15-android.md#testing-an-android-head).
- Not supported on Android: the GameEngine canvas, camera capture, TkCanvas, `SKSwapChainPanel` and
  CodeBrix.VideoPlayback authoring.

## Samples and tools in the repository

| What | Where |
| --- | --- |
| HelloPaste - the JustBetweenUs and PdfSideBySide main pages, pasted unchanged, with multi-targeted `.Core` libraries and an on-device self-check | [samples/HelloPaste](https://github.com/ellisnet/CodeBrix.Android/tree/main/samples/HelloPaste) |
| SimpleDebugApp - plain .NET Android applications for checking that a machine can build, deploy and debug (API 36 on .NET 10, API 37 on .NET 11) | [samples/SimpleDebugApp_API_36](https://github.com/ellisnet/CodeBrix.Android/tree/main/samples/SimpleDebugApp_API_36) · [samples/SimpleDebugApp_API_37](https://github.com/ellisnet/CodeBrix.Android/tree/main/samples/SimpleDebugApp_API_37) |
| The Android head of the CodeBrix.Platform application template | [templates/AndroidHead](https://github.com/ellisnet/CodeBrix.Android/tree/main/templates/AndroidHead) |

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Android/blob/main/README.md) |
| Complete API guide, every add-in included (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Android/blob/main/AGENT-README.txt) |
| Samples, tools and other non-package content | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Android/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Android/blob/main/README-INDEX.txt) |
| The application template's Android head, and what it needs | [templates/TEMPLATE_INTEGRATION.md](https://github.com/ellisnet/CodeBrix.Android/blob/main/templates/TEMPLATE_INTEGRATION.md) |
| Tests - the framework, the toolkit, every add-in, the CBAND analyzer, paste-always compile heads and the UIReqs scenarios on an emulator | [tests](https://github.com/ellisnet/CodeBrix.Android/tree/main/tests) |

The one `AGENT-README.txt` at the repository root documents the framework package and every add-in
package (its ADD-INS ON ANDROID section).

## License

CodeBrix.Android and every one of its add-in packages are licensed under the Apache License 2.0, and
the license is named in each package ID. Two Android add-in packages differ in license from their
CodeBrix.Platform counterparts: `CodeBrix.Android.SkiaSharp.Views.ApacheLicenseForever` is Apache 2.0
where `CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever` is MIT, and
`CodeBrix.Android.MediaPlayer.ApacheLicenseForever` is Apache 2.0 where
`CodeBrix.Platform.MediaPlayer.LgplLicenseForever` is LGPL. See the
[LICENSE](https://github.com/ellisnet/CodeBrix.Android/blob/main/LICENSE) file, and for the provenance
and licensing of open source code included in the packages,
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Android/blob/main/THIRD-PARTY-NOTICES.txt).

---

**Where to go next**

- [Android and Googlebooks](../platform/15-android.md) - adding an Android head to an application,
  file by file
- [CodeBrix.Platform](CodeBrix.Platform.md) - the framework whose applications CodeBrix.Android runs
- [Add-ins](../platform/08-add-ins.md) - every add-in, and what it adds
- [ellisnet/CodeBrix.Android on GitHub](https://github.com/ellisnet/CodeBrix.Android) - source, tests
  and samples
