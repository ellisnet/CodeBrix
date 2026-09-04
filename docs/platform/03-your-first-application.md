<sub>[CodeBrix](../../README.md) › [Build a CodeBrix.Platform application](README.md) › Your first application</sub>

# Your first application

**By the end of this chapter you will have a CodeBrix.Platform application on disk that builds and opens a window on every head you add, and you will have added an add-in package to it.** Every file below is given in full. Follow the steps in order with a terminal open; nothing here depends on a step you have not done yet.

## What you need

CodeBrix.Platform requires .NET 10 or later. That is the only universal prerequisite: the packages come from nuget.org and the framework installs nothing on the machine.

Each head has its own requirements at run time - the X11 head needs a `DISPLAY`, the Wayland head needs a running compositor, the frame-buffer head needs a real frame-buffer device, the Win32 and WPF heads need Windows, and the macOS head needs macOS. [02 - Runs on every laptop](02-runs-on-every-laptop.md) has the full list. Build every head anywhere; run the ones your machine can host.

## The three projects you are about to create

Replace `MyApp` with your application name throughout. The recommended layout, for an application that eventually ships all six heads:

```text
  MyApp.Core             shared class library: view models, services, and the
                         framework + add-in package references (NOT a head)
  MyApp.UI               shared PROJECT (.shproj/.projitems, NOT an assembly):
                         App.xaml + the Views/XAML  (see note below)
  MyApp.LinuxFrameBuffer Linux framebuffer head
  MyApp.LinuxWayland     Linux native-Wayland head
  MyApp.LinuxX11         Linux X11 head
  MyApp.MacOS            macOS head
  MyApp.Win32Skia        Skia-on-Win32 head   (NEVER "MyApp.Windows")
  MyApp.WinWpfSkia       Skia-on-WPF head
```

The "Skia" suffix appears ONLY on the two Windows heads, because Windows is the only operating system that also ships a native head to disambiguate from. The Linux and macOS heads have no native counterpart, so they take no suffix.

> [!IMPORTANT]
> Never give a head project a name whose segments match a top-level SDK namespace your code uses unqualified - above all `Windows` (the root of the WinRT `Windows.*` namespaces), and also `System`. A head named `MyApp.Windows` gives that project its own `MyApp.Windows` namespace, which SHADOWS the global `Windows` namespace: an inline reference such as `Windows.System.VirtualKey` in shared code then binds to `MyApp.Windows` and fails to compile with CS0234 - on that ONE head only, which is baffling to diagnose. A `using Windows.System;` directive still resolves globally, so the breakage is inconsistent and easy to miss. This is why the Skia-on-Win32 head is named `.Win32Skia`, never `.Windows`. Keep every project name distinct from the solution file's base name too, and use the exact casing shown.

You will build these three kinds of project in order: the `.Core` library, the `.UI` shared project, and then one head at a time.

## Step 1 - Create the solution and the .Core library

```bash
dotnet new sln -n MyApp
dotnet new classlib -n MyApp.Core --framework net10.0
cd MyApp.Core
dotnet add package CodeBrix.Platform.ApacheLicenseForever
# add optional add-in packages here as needed (see their AGENT-READMEs)
cd ..
```

The `dotnet add package` line adds [CodeBrix.Platform.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.ApacheLicenseForever), the one package every CodeBrix.Platform application needs. The head builds and runs from its own project file, so the solution file is for your editor's benefit; add each project to it as you create it, including the shared project, which produces no assembly but should appear in the project tree.

Now open `MyApp.Core/MyApp.Core.csproj`. The smallest form that works is three properties and two package references:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>MyApp</RootNamespace>
    <DefineConstants>$(DefineConstants);HAS_CODEBRIX;HAS_CODEBRIX_WINUI</DefineConstants>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Logging.Console" />
    <PackageReference Include="CodeBrix.Platform.ApacheLicenseForever" />
  </ItemGroup>
</Project>
```

Two things to notice. `RootNamespace` is set to the application namespace, not to the project name, so view models land in `MyApp.ViewModels` and shared XAML reaches them without extra qualification. And `HAS_CODEBRIX` and `HAS_CODEBRIX_WINUI` must be defined in EVERY project that participates in the UI - the `.Core` library and every head - because the framework uses them for internal conditional compilation and some public API, `Window.AppWindow` among it, is only public when `HAS_CODEBRIX_WINUI` is defined. The core package's build targets also add these constants to projects that reference it; declaring them yourself is the reference-application convention and is harmless. The head packages additionally define the head-specific constants for you - do not define those by hand.

A real application's `.Core` grows from there: the generic host, console logging, a bundled font package, and each add-in it uses.

<details>
<summary>The reference <code>.Core</code> project file, with the optional pieces in place</summary>

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>MyApp</RootNamespace>
    <!-- CodeBrix.Platform uses these for internal conditional compilation -->
    <DefineConstants>$(DefineConstants);HAS_CODEBRIX;HAS_CODEBRIX_WINUI</DefineConstants>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" />

    <!-- The core UI framework (REQUIRED) -->
    <PackageReference Include="CodeBrix.Platform.ApacheLicenseForever" />

    <!-- Optional add-ins - include only what you use, e.g.: -->
    <PackageReference Include="CodeBrix.Platform.Graphics2DSK.ApacheLicenseForever" />
    <PackageReference Include="CodeBrix.Platform.WebView.ApacheLicenseForever" />
    <!-- Optional bundled font: -->
    <PackageReference Include="CodeBrix.Platform.Fonts.OpenSans.ApacheLicenseForever" />
  </ItemGroup>
</Project>
```

</details>

The `App.xaml.cs` in Step 3 sets a bundled font as the application's default text font. If you want that line to work as written, add [CodeBrix.Platform.Fonts.OpenSans.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.OpenSans.ApacheLicenseForever) to `.Core` now; otherwise drop the font line when you get there.

## Step 2 - Create the .UI shared project

A shared project is a `.shproj` with a sibling `.projitems`. Create the folder `MyApp.UI` and create those two files with the exact contents below. Their contents are compiled into whichever head imports the `.projitems`.

`MyApp.UI/MyApp.UI.projitems` lists the shared files. Each XAML file is a `<Page>` with the `MSBuild:Compile` generator, and each code-behind is a `<Compile>` with `<DependentUpon>`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <HasSharedItems>true</HasSharedItems>
    <SharedGUID>PUT-A-NEW-GUID-HERE</SharedGUID>
  </PropertyGroup>
  <PropertyGroup Label="Configuration">
    <Import_RootNamespace>MyApp.UI</Import_RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <Page Include="$(MSBuildThisFileDirectory)App.xaml">
      <SubType>Designer</SubType>
      <Generator>MSBuild:Compile</Generator>
    </Page>
    <Page Include="$(MSBuildThisFileDirectory)Views\MainPage.xaml">
      <SubType>Designer</SubType>
      <Generator>MSBuild:Compile</Generator>
    </Page>
  </ItemGroup>
  <ItemGroup>
    <Compile Include="$(MSBuildThisFileDirectory)App.xaml.cs">
      <DependentUpon>App.xaml</DependentUpon>
    </Compile>
    <Compile Include="$(MSBuildThisFileDirectory)Views\MainPage.xaml.cs">
      <DependentUpon>MainPage.xaml</DependentUpon>
    </Compile>
  </ItemGroup>
</Project>
```

Generate a fresh GUID and put it in `SharedGUID`. There is no globbing here: every new page and its code-behind is added by hand, as a `<Page>` item with the compile generator and a `<Compile>` item that depends upon its XAML. `Import_RootNamespace` is deliberately not the namespace the files declare - the C# namespace and the XAML `x:Class` attribute win, and it is the head's own `RootNamespace` that has to agree with them.

`MyApp.UI/MyApp.UI.shproj` is the wrapper that lets an editor open the shared project. It imports the `.projitems` and the code-sharing targets, and uses the SAME GUID:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup Label="Globals">
    <ProjectGuid>PUT-THE-SAME-GUID-HERE</ProjectGuid>
    <MinimumVisualStudioVersion>14.0</MinimumVisualStudioVersion>
  </PropertyGroup>
  <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />
  <Import Project="$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)\CodeSharing\Microsoft.CodeSharing.Common.Default.props" />
  <Import Project="$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)\CodeSharing\Microsoft.CodeSharing.Common.props" />
  <PropertyGroup />
  <Import Project="MyApp.UI.projitems" Label="Shared" />
  <Import Project="$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)\CodeSharing\Microsoft.CodeSharing.CSharp.targets" />
</Project>
```

The shared project's GUID and the `SharedGUID` in the item list are the same value; that pairing is what makes the shared project work.

Finally, `MyApp.UI/App.xaml` - the application's resource dictionary root, in WinUI style:

```xml
<Application
    x:Class="MyApp.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
</Application>
```

That is deliberately empty. Application-wide dictionaries and resource keys go inside it as the application grows; [06 - Views and styling](06-views-and-styling.md) covers what belongs there.

## Step 3 - Write App.xaml.cs

`App.xaml.cs` lives in the `.UI` shared project and is compiled into every head. It derives from `Microsoft.UI.Xaml.Application` and does three jobs: configure the framework before the XAML is initialized, create the window and navigate to the first page, and expose the logging initializer that each head's `Main` calls. This is the reference pattern, complete:

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace MyApp;

public partial class App : Application
{
    public App()
    {
        // (Optional) set a default font, e.g. the bundled Open Sans package.
        // The "ms-appx:///<PackageId-without-suffix>/Fonts/<file>.ttf" form
        // loads a font shipped inside a referenced package:
        global::CodeBrix.Platform.UI.FeatureConfiguration.Font.DefaultTextFontFamily =
            "ms-appx:///CodeBrix.Platform.Fonts.OpenSans/Fonts/OpenSans.ttf";

        // (Optional) RequestedTheme = ApplicationTheme.Dark;  // ONLY here, before InitializeComponent
        // (Optional) register your DI services here, then:
        InitializeComponent();
    }

    protected Window MainWindow { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new Window { Title = "My App" };

        if (MainWindow.Content is not Frame rootFrame)
        {
            rootFrame = new Frame();
            MainWindow.Content = rootFrame;
            rootFrame.NavigationFailed += OnNavigationFailed;
        }

        if (rootFrame.Content == null)
        {
            rootFrame.Navigate(typeof(Views.MainPage), args.Arguments);
        }

        MainWindow.Activate();
    }

    void OnNavigationFailed(object sender, NavigationFailedEventArgs e) =>
        throw new InvalidOperationException(
            $"Failed to load {e.SourcePageType.FullName}: {e.Exception}");

    // Called from each head's Program.Main BEFORE building the host.
    public static void InitializeLogging()
    {
#if DEBUG
        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddFilter("CodeBrix.Platform", LogLevel.Warning);
            builder.AddFilter("Microsoft", LogLevel.Warning);
        });

        global::CodeBrix.Platform.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_CODEBRIX
        global::CodeBrix.Platform.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
#endif
    }
}
```

Four details in that file are load-bearing.

The constructor runs BEFORE any XAML is parsed, which is why the font line and the theme line are there and nowhere else. `FeatureConfiguration.Font.DefaultTextFontFamily` must be set before the first text is measured, and `Application.RequestedTheme` may be set ONLY before initialization completes - after `InitializeComponent()` its setter throws `NotSupportedException`. To switch themes at run time, set `FrameworkElement.RequestedTheme` on the window's root element instead. If you did not add a font package in Step 1, delete the font line; if you did, note that a bundled font must be referenced from `.Core` so every head ships it.

`OnLaunched` creates the window, gives it a `Frame`, and navigates. `NavigationFailed` throws rather than logging, so a typo in the page type surfaces immediately instead of showing an empty window.

The logging bridge takes two statements, in this order: assigning `LogExtensionPoint.AmbientLoggerFactory` is not enough on its own; `LoggingAdapter.Initialize()` is what connects the framework's own logging to your factory. The adapter is folded into the core package, so there is no separate package to install. The whole body sits inside `#if DEBUG`, so the method compiles to nothing in Release and every call site stays valid - console logging at Information level in Release costs frames. Keep `CodeBrix.Platform` filtered to Warning.

And `InitializeLogging()` is `static` because each head's `Program.Main` calls it before the host builder exists.

## Step 4 - Write the first page

`MyApp.UI/Views/MainPage.xaml` is a normal WinUI page:

```xml
<Page
    x:Class="MyApp.Views.MainPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
        <TextBlock Text="Hello from CodeBrix.Platform" />
        <Button Content="Click me" Click="OnClick" />
    </StackPanel>
</Page>
```

And `MyApp.UI/Views/MainPage.xaml.cs` alongside it:

```csharp
using Microsoft.UI.Xaml.Controls;

namespace MyApp.Views;

public sealed partial class MainPage : Page
{
    public MainPage() => InitializeComponent();
    void OnClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) { /* ... */ }
}
```

Nothing in either file is CodeBrix-specific. Controls, panels, styles, visual states, animations, `ListView` and `GridView` with data templates, `NavigationView`, `TabView`, flyouts, `MenuBar`, `CommandBar`, `ScrollViewer`, `SplitView`, `Slider`, `ToggleSwitch`, `ComboBox`, the date and time pickers, `ProgressRing`, `Image`, `TextBox`, `PasswordBox`, `RichEditBox` and the rest of the `Microsoft.UI.Xaml.Controls` surface are written exactly as in WinUI documentation.

Both files were already declared in the `.projitems` in Step 2, so nothing else needs updating.

## Step 5 - Add the first head

Every head project is nearly identical. The ONLY differences between heads are the single head package referenced, the `.Use...()` call in `Program.cs`, and - for the WPF head alone - the target framework. Create the one for your own machine first. This is the Skia-on-Win32 head:

```bash
dotnet new console -n MyApp.Win32Skia --framework net10.0
cd MyApp.Win32Skia
dotnet add package CodeBrix.Platform.Runtime.Skia.Win32.ApacheLicenseForever
dotnet add reference ../MyApp.Core/MyApp.Core.csproj
cd ..
```

For another platform, change only the package in the third line, using the table in Step 7.

Now replace the generated `MyApp.Win32Skia/MyApp.Win32Skia.csproj` with this. It sets `OutputType=Exe`, adds the compilation constants, declares `.xaml` as `<Page>` items, imports the `.UI` `.projitems`, and references exactly one head package:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <RootNamespace>MyApp</RootNamespace>
    <DefineConstants>$(DefineConstants);HAS_CODEBRIX;HAS_CODEBRIX_WINUI</DefineConstants>
  </PropertyGroup>

  <!-- Treat .xaml files as CodeBrix.Platform XAML pages -->
  <ItemGroup>
    <Page Include="**\*.xaml" Exclude="bin\**\*.xaml;obj\**\*.xaml" />
    <None Remove="**\*.xaml" />
  </ItemGroup>

  <!-- Pull in the shared App.xaml + Views -->
  <Import Project="..\MyApp.UI\MyApp.UI.projitems" Label="Shared" />

  <ItemGroup>
    <ProjectReference Include="..\MyApp.Core\MyApp.Core.csproj" />
  </ItemGroup>

  <!-- EXACTLY ONE platform head package (this one = Windows/Win32): -->
  <ItemGroup>
    <PackageReference Include="CodeBrix.Platform.Runtime.Skia.Win32.ApacheLicenseForever" />
  </ItemGroup>
</Project>
```

The page glob and the matching `None Remove` are required in every head, or the shared XAML arrives as content and is never compiled. The `Import ... Label="Shared"` line is what pulls `App.xaml`, `App.xaml.cs` and the Views into this assembly - the XAML compiles into the head, not into `.Core`.

Then replace the generated `Program.cs` with the bootstrap:

```csharp
using CodeBrix.Platform.UI.Hosting;
using System;

namespace MyApp;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseWindowsWin32()   // <-- platform selector; see table below
            .Build();

        host.Run();
    }
}
```

`Program.Main` owns nothing but hosting. It initializes logging, builds a host, hands it a factory for the shared `App` class, selects exactly one backend, and runs. No application logic lives in a head; services, fonts, settings and the first page all belong to `App`, and everything the user interacts with belongs to a view model.

Four things about that file are conventions worth keeping. `App.InitializeLogging()` is called before the host is built, never after - logging wired after `Build()` misses the framework's own startup messages. `[STAThread]` is on `Main` in every head, including the Linux and macOS ones. `.App(() => new App())` takes a factory, not an instance, because the host decides when the application object is constructed. And the head declares the same namespace as the shared UI project, which is what lets `new App()` resolve with no using directive.

An asynchronous form is equivalent, and some heads use it:

```csharp
[STAThread]
public static async Task Main(string[] args)
{
    App.InitializeLogging();
    var host = CodeBrixPlatformHostBuilder.Create()
        .App(() => new App())
        .UseWindowsWin32()
        .Build();
    await host.RunAsync();
}
```

One exception: if the head hosts a `WebView2` on Windows, keep `Main` synchronous. `[STAThread]` is silently ignored on an `async Task Main`, the thread runs as MTA, and WebView creation later fails with `RPC_E_CHANGED_MODE`.

## Step 6 - Build and run

```bash
dotnet build MyApp.Win32Skia/MyApp.Win32Skia.csproj
dotnet run --project MyApp.Win32Skia/MyApp.Win32Skia.csproj
```

A window titled "My App" opens, with the text and the button from Step 4. On Linux, the equivalent for the X11 head is:

```bash
dotnet run --project MyApp.LinuxX11/MyApp.LinuxX11.csproj
```

If nothing appears, work through the usual causes first: a missing `HAS_CODEBRIX` or `HAS_CODEBRIX_WINUI` define, XAML that arrived as content because the `<Page>` glob is missing from the head, a Wayland head launched in an X11-only session, or - on the WPF head - the missing software-rendering line covered in Step 7. [12 - Troubleshooting](12-troubleshooting.md) has the rest.

## Step 7 - Add the other heads

Repeat Step 5 for each additional platform, changing only the head package and the `.Use...()` call in `Program.cs`. Nothing else changes.

| Platform target | Head package | Bootstrap call |
| --- | --- | --- |
| Windows (Win32) | [CodeBrix.Platform.Runtime.Skia.Win32.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.Win32.ApacheLicenseForever) | `.UseWindowsWin32()` |
| Windows (WPF) | [CodeBrix.Platform.Runtime.Skia.Wpf.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.Wpf.ApacheLicenseForever) | `.UseWindowsWpf()` |
| Linux (X11) | [CodeBrix.Platform.Runtime.Skia.X11.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.X11.ApacheLicenseForever) | `.UseLinuxX11()` |
| Linux (native Wayland) | [CodeBrix.Platform.Runtime.Skia.Wayland.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.Wayland.ApacheLicenseForever) | `.UseLinuxWayland()` |
| Linux (framebuffer) | [CodeBrix.Platform.Runtime.Skia.FrameBuffer.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.FrameBuffer.ApacheLicenseForever) | `.UseLinuxFrameBuffer()` |
| macOS | [CodeBrix.Platform.Runtime.Skia.MacOS.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.MacOS.ApacheLicenseForever) | `.UseMacOS()` |

Run each one the same way, substituting the head's project path:

```bash
dotnet run --project MyApp.LinuxWayland/MyApp.LinuxWayland.csproj
dotnet run --project MyApp.MacOS/MyApp.MacOS.csproj
dotnet run --project MyApp.WinWpfSkia/MyApp.WinWpfSkia.csproj
```

Three heads need a little more than the table.

### The WPF head

Its `.csproj` differs from Step 5 in the target framework and the package line, and in nothing else:

```xml
<PropertyGroup>
  <TargetFramework>net10.0-windows</TargetFramework>
  <OutputType>Exe</OutputType>
  <RootNamespace>MyApp</RootNamespace>
  <DefineConstants>$(DefineConstants);HAS_CODEBRIX;HAS_CODEBRIX_WINUI</DefineConstants>
</PropertyGroup>
...
<PackageReference Include="CodeBrix.Platform.Runtime.Skia.Wpf.ApacheLicenseForever" />
```

Do NOT set `<UseWPF>true</UseWPF>`: that would make WPF's build targets try to treat the CodeBrix.Platform `.xaml` `<Page>` items as WPF XAML. WPF is loaded by the host at run time; the XAML stays CodeBrix.Platform XAML. Setting `<EnableWindowsTargeting>true</EnableWindowsTargeting>` alongside the Windows moniker lets this head restore and compile - though not run - inside a solution opened on a Linux or macOS build machine.

Its `Program.cs` adds one `using` and one block between `Build()` and `Run()`:

```csharp
using CodeBrix.Platform.UI.Hosting;
using CodeBrix.Platform.UI.Runtime.Skia.Wpf;   // for WpfHost + RenderSurfaceType
using System;

namespace MyApp;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseWindowsWpf()
            .Build();

        if (host is WpfHost wpfHost)
        {
            wpfHost.RenderSurfaceType = RenderSurfaceType.Software;
        }

        host.Run();
    }
}
```

Without that block the window appears and the content never composites - a blank window. In most applications this is the only per-head behavioral difference in the whole solution.

### The Wayland head

Nothing in the project changes beyond the package and the call, but the head fails fast with a clean "This application requires a Wayland compositor." message and exit code 1 when it is launched in an X11-only session. That is by design and not a fault to debug.

### The frame-buffer head

Pickers, the on-screen keyboard and the clipboard do not exist on this head unless the builder turns them on, and the picker APIs throw `NotSupportedException` otherwise. If your application asks the user for a file or takes typed input, configure them here:

```csharp
using CodeBrix.Platform.UI.Hosting;
using CodeBrix.Platform.UI.Runtime.Skia;
using Windows.Graphics.Display;

var host = CodeBrixPlatformHostBuilder.Create()
    .App(() => new App())
    .UseLinuxFrameBuffer(fb => fb
        .Orientation(DisplayOrientations.Landscape, isPreferredOrientation: true)
        .AutoRotationEnabled(DisplayOrientations.Landscape, DisplayOrientations.LandscapeFlipped)
        .DisableMouseCursor()
        .ScaleUserInterface(UserInterfaceScale.Percent150)
        .EnableFileOpenPicker(new FilePickerOptions { RestrictToFolder = "/data", AllowMultipleFileSelect = false })
        .EnableFolderPicker()
        .EnableSoftwareKeyboard(new SoftwareKeyboardOptions { KeyHeight = SoftwareKeyHeight.PortraitFullLandscapeHalf })
        .EnableSimpleTextClipboard())
    .Build();
host.Run();
```

The view model does not change: it still calls a picker and still binds a `TextBox`. The head is what decides whether a picker and an on-screen keyboard exist to serve those calls. [02 - Runs on every laptop](02-runs-on-every-laptop.md) explains each option.

> [!WARNING]
> Do not launch a frame-buffer build on a desktop machine. It draws into `/dev/fb0` and takes over the console invisibly.

## Step 8 - Add an add-in

Every add-in package is referenced once, in `.Core`, without a version attribute. It flows to every head transitively, and an add-in that only works on some heads is inert on the others and never breaks a build. Add audio playback:

```bash
dotnet add package CodeBrix.Platform.AudioPlayer.ApacheLicenseForever
```

Run that inside `MyApp.Core`. No head project changes. Now declare the element in a page, with a `Slider` bound to it as a scrubber:

```xml
<Page
    x:Class="MyApp.Views.PlayerPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:audio="using:CodeBrix.Platform.UI.AudioPlayer.Skia">
    <Grid>
        <audio:AudioPlayer x:Name="Player" Source="ms-appx:///Assets/song.mp3" AutoPlay="True" />
        <Slider VerticalAlignment="Bottom" Margin="16"
            Maximum="{Binding DurationSeconds, ElementName=Player}"
            Value="{Binding PositionSeconds, ElementName=Player, Mode=TwoWay}" />
    </Grid>
</Page>
```

That is the whole pattern: one package line in `.Core`, one `xmlns` in the page, and the element. [CodeBrix.Platform.AudioPlayer.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.AudioPlayer.ApacheLicenseForever) needs no native setup and is live on all six heads; other add-ins have their own per-head notes, and some need a system-installed engine on Linux. Remember to add the new page to the `.projitems`, as a `<Page>` item and a `<Compile>` item, exactly as in Step 2.

Add-ins bring their own dependencies automatically, and some name a companion package the application must add alongside them. [08 - Add-ins](08-add-ins.md) lists every add-in, its heads and its companions.

## The files you now have

The minimum viable project is eight files - one `.Core`, one `.UI`, one head:

```text
MyApp.Core/MyApp.Core.csproj          THE .Core PROJECT, keeping only the
                                      CodeBrix.Platform.ApacheLicenseForever
                                      reference
MyApp.UI/MyApp.UI.projitems           THE .UI SHARED PROJECT (A)
MyApp.UI/MyApp.UI.shproj              THE .UI SHARED PROJECT (B)
MyApp.UI/App.xaml                     THE .UI SHARED PROJECT (C)
MyApp.UI/App.xaml.cs                  APP.XAML.CS PATTERNS (drop the font
                                      line if you ship no font package)
MyApp.UI/Views/MainPage.xaml(.cs)     WRITING XAML AND VIEWS
MyApp.LinuxX11/MyApp.LinuxX11.csproj  THE PLATFORM HEAD PROJECTS with
                                      CodeBrix.Platform.Runtime.Skia.X11.ApacheLicenseForever
MyApp.LinuxX11/Program.cs             THE BOOTSTRAP with .UseLinuxX11()
```

Adding a second platform is one more head folder with the other head package and the other `.Use...()` call. Nothing else changes.

From here, the next thing to write is a view model - the shared XAML binds to plain classes in `.Core`, and that is where the application actually lives. [05 - MVVM the right way](05-mvvm-the-right-way.md) picks up exactly there.

## Checklist

- [ ] `.Core` sets `RootNamespace` to the application namespace and defines `HAS_CODEBRIX` and `HAS_CODEBRIX_WINUI`
- [ ] `.Core` holds the framework package and every add-in, and no head package
- [ ] The `.UI` shared project's `SharedGUID` and the `.shproj` `ProjectGuid` are the same fresh GUID
- [ ] Every XAML file and its code-behind is listed in the `.projitems`
- [ ] `App.xaml.cs` sets fonts and the requested theme before `InitializeComponent()`
- [ ] Every head declares the `<Page>` glob with the matching `None Remove`, imports the `.UI` `.projitems`, and references `.Core`
- [ ] Every head references exactly one platform head package
- [ ] Every head's `Main` is `[STAThread]` and calls `App.InitializeLogging()` before `CodeBrixPlatformHostBuilder.Create()`
- [ ] The Win32 head is named `.Win32Skia`, never `.Windows`
- [ ] The WPF head uses the Windows-specific framework moniker shown above, does not set `<UseWPF>`, and forces the software render surface after `Build()`
- [ ] The frame-buffer head enables the pickers, keyboard and clipboard the application needs
- [ ] Add-in packages were added to `.Core` only

---

**Where to go next**

- [04 - Project architecture](04-project-architecture.md) - the next chapter: which package goes where as the solution grows
- [05 - MVVM the right way](05-mvvm-the-right-way.md) - the view models the shared XAML binds to
- [08 - Add-ins](08-add-ins.md) - every add-in, its heads and its companion packages
- [Reference applications](../samples/README.md) - complete applications built exactly this way
