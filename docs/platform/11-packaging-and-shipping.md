<sub>[CodeBrix](../../README.md) › [Build a CodeBrix.Platform application](README.md) › Packaging and shipping</sub>

# Packaging and shipping

**By the end of this chapter you will be able to lay an application out so every head builds from one source tree, put each native payload on the head that needs it, keep solutions that restore on any build machine, and tell a target machine exactly what it must have installed to run what you shipped.** It covers the folder shape, the package split between `.Core` and the heads, embedded assets, the Windows-only heads and their solution platforms, the per-operating-system prerequisites, the frame-buffer device requirements, and the notices file every application carries.

[04 - Project architecture](04-project-architecture.md) establishes the three kinds of projects and the rule that decides which one owns each package reference. This chapter is about what leaves the build: the files on disk, the natives beside the executable, and the machine that has to run it.

## The application folder on disk

An application is one self-contained folder. Everything it needs to build is inside it, and the libraries it uses arrive as packages rather than as project references, so the folder can be copied, opened and built on its own.

```text
src/PalmVisualizer.UI/            .shproj + .projitems: App.xaml(.cs), Views/MainPage.xaml(.cs)
src/PalmVisualizer.Core/          view models + helpers; owns the platform and font packages
src/libs/PalmVisualizer.Camera/   capture + preview canvas       -> tests/libs/PalmVisualizer.Camera.Tests
src/libs/PalmVisualizer.Vision/   palm tracking + models         -> tests/libs/PalmVisualizer.Vision.Tests
src/libs/PalmVisualizer.Rendering/ engine session + shader scene -> tests/libs/PalmVisualizer.Rendering.Tests
src/PalmVisualizer.<Head>/        one per head; imports the .projitems, references Core
```

Notice that `src/`, `src/libs/` and `tests/libs/` are the folder names on disk. The solution folders that group them - `/Libraries/`, `/Tests/` - are declarations inside the solution file and correspond to nothing on disk:

```xml
<!-- From CodeBrix.Samples/PalmVisualizer/PalmVisualizer.slnx -->
<Folder Name="/Libraries/">
  <Project Path="src/libs/PalmVisualizer.Camera/PalmVisualizer.Camera.csproj" />
  <Project Path="src/libs/PalmVisualizer.Rendering/PalmVisualizer.Rendering.csproj" />
  <Project Path="src/libs/PalmVisualizer.Vision/PalmVisualizer.Vision.csproj" />
</Folder>
<Folder Name="/Tests/">
  <Project Path="tests/libs/PalmVisualizer.Camera.Tests/PalmVisualizer.Camera.Tests.csproj" />
  <Project Path="tests/libs/PalmVisualizer.Rendering.Tests/PalmVisualizer.Rendering.Tests.csproj" />
  <Project Path="tests/libs/PalmVisualizer.Vision.Tests/PalmVisualizer.Vision.Tests.csproj" />
</Folder>
```

Four things sit at the application root beside `src/` and `tests/`: the solution file or files, a `README.md` that walks the application's own layout, a `THIRD-PARTY-NOTICES.txt`, and - where the tests need one - a `global.json` that selects the test runner. Nothing else has to be there.

## Apportioning the packages

One project carries the package list. `.Core` names the framework package, every add-in, the font package, the generic host and every third-party library; each head project-references `.Core` and adds exactly one runtime package. That is what lets you add a head, or a dependency, without editing six project files.

```xml
<!-- Adapted from CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.Core/CodeBrixVideoTool.Core.csproj -->
<!-- Package IDs elided; see the project's csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>

    <!-- Match the namespace used by the app code -->
    <RootNamespace>CodeBrixVideoTool</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <!-- ... CodeBrix.Platform, the Roboto font package, the generic host and console logging ... -->

    <!-- The VideoPlayer add-in - the VideoPlayer element the main page hosts. Referenced ONCE here:
         every head inherits it transitively, and it is live on all four heads because the
         containers, the demultiplexer and the clock are all managed code. The two codec packages it
         plays through are the application's own and live in CodeBrixVideoTool.Playback. -->
    <!-- ... the VideoPlayer add-in package ... -->
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\libs\CodeBrixVideoTool.Processing\CodeBrixVideoTool.Processing.csproj" />
    <ProjectReference Include="..\libs\CodeBrixVideoTool.Playback\CodeBrixVideoTool.Playback.csproj" />
  </ItemGroup>
</Project>
```

Notice the comment beside the add-in. It says which heads the add-in is live on for this application and why - a note like that saves the next reader a test run, and it is the kind of thing worth writing down where the reference sits.

The head half is the mirror image: the page glob, the shared-project import, the project reference to `.Core`, and one package.

```xml
<!-- Adapted from CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.LinuxX11/PalmVisualizer.LinuxX11.csproj
     (package ids elided - see the project's csproj) -->
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <OutputType>Exe</OutputType>
</PropertyGroup>

<!-- Tell MSBuild to treat .xaml files as CodeBrix.Platform XAML pages -->
<ItemGroup>
  <Page Include="**\*.xaml" Exclude="bin\**\*.xaml;obj\**\*.xaml" />
  <None Remove="**\*.xaml" />
</ItemGroup>

<!-- Shared UI files (App.xaml + Views) -->
<Import Project="..\PalmVisualizer.UI\PalmVisualizer.UI.projitems" Label="Shared" />
<ItemGroup>
  <ProjectReference Include="..\PalmVisualizer.Core\PalmVisualizer.Core.csproj" />
</ItemGroup>

<!-- EXACTLY ONE platform head package; all other packages come from PalmVisualizer.Core -->
<ItemGroup>
  <PackageReference Include="(the X11 platform runtime package)" />
</ItemGroup>
```

Notice that the page glob and the matching `None` removal are required in every head, or the shared XAML arrives as content and is never compiled. Notice too what is not there: no add-in package, no second runtime package. A second runtime package on one head is a build nothing will warn you about and a run that will not work.

Two smaller rules travel with the split. Where an application defines compilation symbols for the framework's own conditional compilation, define them in `.Core` and in every head that compiles shared source. And where a package identifier carries a license suffix, that suffix is how the family encodes the license - read it before taking the dependency.

## Native payloads travel on the heads

The documented exception to "exactly one platform package per head" is a native payload. A library that calls a native API references only the managed binding, so it stays runtime-independent and its test project stays free of runtime identifiers:

```xml
<!-- From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/PalmVisualizer.Vision.csproj -->
<ItemGroup>
  <!-- OpenCV 5 (managed binding): TFLite model inference via the DNN module.
       The native OpenCV library comes from the per-platform
       CodeBrix.VideoProcessing.OpenCV5.{Platform} packages referenced by each head. -->
  <PackageReference Include="..." />
</ItemGroup>
```

The heads carry the binaries, and each head names both architectures of its own platform unconditionally, so that head publishes for either without an edit:

```xml
<!-- Adapted from CodeBrix.Samples/WebcamPainter/src/WebcamPainter.MacOS/WebcamPainter.MacOS.csproj
     (package IDs removed - see the project's csproj for those) -->

<!-- EXACTLY ONE platform head package; all other packages come from WebcamPainter.Core -->
<ItemGroup>
  <PackageReference Include="(CodeBrix.Platform runtime for this head)" />
</ItemGroup>

<!-- Native OpenCV library for the hand-tracking (Paint Mode) pipeline -->
<ItemGroup>
  <PackageReference Include="(OpenCV native for macOS arm64)" />
  <PackageReference Include="(OpenCV native for macOS x64)" />
</ItemGroup>
```

| Head | Native packages referenced |
| --- | --- |
| LinuxX11, LinuxWayland, LinuxFrameBuffer | Linux x64 and Linux arm64 |
| MacOS | macOS arm64 and macOS x64 |
| Win32Skia, WinWpfSkia | Windows x64 and Windows arm64 |

Only a test project conditions on the build machine, because a test run needs one machine's binary rather than every machine's.

The second kind of native payload is one an add-in needs on a particular operating system. That belongs in the heads for that operating system, with the reason in a comment beside it, because leaving it off builds cleanly and fails at run time:

```xml
<!-- Adapted from CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.Win32Skia/MediaPlayerDemo.Win32Skia.csproj -->
<ItemGroup>
  <!-- EXACTLY ONE platform head package; all other packages come from MediaPlayerDemo.Core -->
  <PackageReference Include="(the CodeBrix.Platform Skia Win32 runtime package)" />
  <!--The following package is required on Window heads for the CodeBrix.Platform.MediaPlayer add-in-->
  <PackageReference Include="(the VideoLAN libVLC for Windows package)" />
</ItemGroup>
```

Notice that this is the one case where a head may name a non-CodeBrix package, and only because the add-in's own `AGENT-README.txt` says so. Where an application says nothing about a platform's native requirement, that is not the same as saying none is needed: check the add-in's documentation before shipping there. [08 - Add-ins](08-add-ins.md) lists every add-in with its companion packages and the heads it is live on.

Some packages need no fan-out at all. "Bundles its own natives" is worth stating literally when it is true: the package carries the native library for each supported runtime identifier, each with its own license beside it, so there is no per-head fan-out to arrange and no system library to install. Write that in a comment beside the reference so nobody adds one.

One such reference is also a lesson in what arrives transitively. In the page-rendering library of PdfSideBySide the project file names one package; the code uses types from three:

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Rendering/PageRenderer.cs
using CodeBrix.Imaging;
using CodeBrix.Imaging.Formats.Png;
using CodeBrix.PdfRasterizer;
```

The rasterizer brings the imaging library and the PDF authoring library with it, and the authoring library brings compression. Convenient - but an upgrade of the top package moves the others too. If you depend on one of them directly, name it directly.

## Assets that ship inside the assembly

A model, an image or a font that must travel inside an assembly rather than as loose content is an embedded resource with an explicit logical name. Without the explicit name, the name is derived from the root namespace and the link path, so it changes when the file moves or the project is renamed - and shared source compiled into several assemblies would get a different name in each of them.

```xml
<!-- Adapted from CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/PalmVisualizer.Vision.csproj
     (the model folder replaced with a placeholder) -->
<ItemGroup>
  <EmbeddedResource Include="..\..\..\models\<model-folder>\hand_landmarker\hand_detector.tflite"
                    Link="Models\hand_detector.tflite">
    <LogicalName>PalmVisualizer.Vision.Models.hand_detector.tflite</LogicalName>
  </EmbeddedResource>
</ItemGroup>
```

The loading half reads the resource from its own assembly by that exact name, and fails with a message that names the resource:

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/PalmTracker.cs
internal static byte[] LoadEmbeddedModel(string resourceName)
{
    using Stream stream = typeof(PalmTracker).Assembly.GetManifestResourceStream(resourceName);
    if (stream == null)
    {
        throw new InvalidOperationException($"Embedded model not found: {resourceName}");
    }
    using var buffer = new MemoryStream();
    stream.CopyTo(buffer);
    return buffer.ToArray();
}
```

Notice that this throws. Decide per asset whether a missing file is fatal: a missing background image logs and returns, a missing model throws with the resource name in the message. Where a name is derived rather than stated - embedded fonts resolved by root namespace plus folder - the removal item must precede the embed item, or the files are included twice. And embedding only part of a downloaded bundle deserves a comment saying why the rest was left out; that comment is what stops someone re-adding it.

## Windows heads and solution platforms

A native WinUI head declares the architectures it supports rather than building as Any CPU, and the solution has to declare the same list and map each platform onto the head:

```xml
<!-- From CodeBrix.Samples/PainDiagram/PainDiagram.Windows.slnx -->
<!-- PainDiagram.WinUI only declares Platforms x86/x64/ARM64 (no Any CPU),
     so the solution platforms are restricted to match - otherwise VS offers
     "Any CPU" and fails to map it to the WinUI project. -->
<Configurations>
  <Platform Name="x86" />
  <Platform Name="x64" />
  <Platform Name="ARM64" />
</Configurations>
<!-- ... -->
<Project Path="PainDiagram.WinUI/PainDiagram.WinUI.csproj">
  <Platform Solution="*|x86" Project="x86" />
  <Platform Solution="*|x64" Project="x64" />
  <Platform Solution="*|ARM64" Project="ARM64" />
  <Deploy Solution="Debug|x64" />
</Project>
```

The head half declares the architectures, the runtime identifiers, the publish-profile pattern and the packaging tooling:

```xml
<!-- From CodeBrix.Samples/JustBetweenUs/JustBetweenUs.WinUI/JustBetweenUs.WinUI.csproj -->
<OutputType>WinExe</OutputType>
<TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
<TargetPlatformMinVersion>10.0.17763.0</TargetPlatformMinVersion>
<RootNamespace>JustBetweenUs.WinUI</RootNamespace>
<ApplicationManifest>app.manifest</ApplicationManifest>
<Platforms>x86;x64;ARM64</Platforms>
<RuntimeIdentifiers Condition="$([MSBuild]::GetTargetFrameworkVersion('$(TargetFramework)')) &gt;= 8">win-x86;win-x64;win-arm64</RuntimeIdentifiers>
<PublishProfile>win-$(Platform).pubxml</PublishProfile>
<UseWinUI>true</UseWinUI>
<EnableMsixTooling>true</EnableMsixTooling>
<DefineConstants>$(DefineConstants);HAS_WINUI</DefineConstants>
```

Notice what follows from that. Without the platform mapping the solution will not build with Any CPU selected, because the head declares no such platform. The WinUI head is usually the only project in the solution with deploy entries, and where a solution declares several platform names every other project maps all of them to Any CPU. The packaging capability blocks in the head are guarded, so the tooling menus appear even before the packaging package has been restored - and two launch profiles are worth keeping, packaged and unpackaged, because you do not have to package the application to run it. The cross-platform solution does not include this head at all, which is why that solution keeps the default configuration.

A genuinely native WPF head is the other Windows-only project, and it does set the WPF build support that the WPF-hosted Skia head must leave off:

```xml
<!-- From CodeBrix.Samples/PainDiagram/PainDiagram.Wpf/PainDiagram.Wpf.csproj -->
<PropertyGroup>
  <OutputType>WinExe</OutputType>
  <!-- SkiaSharp.Views.WPF ships net10.0-windows10.0.19041 assets, so the TFM must
       carry (at least) that Windows platform version -->
  <TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
  <UseWPF>true</UseWPF>
  <RootNamespace>$(MSBuildProjectName.Replace(" ", "_").Replace(".Wpf", ""))</RootNamespace>
  <!-- Lets the project compile (not run) on Linux/macOS build hosts -->
  <EnableWindowsTargeting>true</EnableWindowsTargeting>
</PropertyGroup>
```

> [!TIP]
> Keep two solution files at the application root: one cross-platform and one Windows-only superset, with a comment at the top of each saying which is which. Exclude a head only when it genuinely cannot restore - the Win32 Skia head targets the plain framework moniker and so restores and builds anywhere even though it only runs on Windows. A mobile head belongs only in the solutions whose workloads can build it.

## What the target machine needs

Every head requires .NET 10 or later. Beyond that, each one asks something different of the machine it runs on.

| Head | What the operating system must provide |
| --- | --- |
| Win32Skia | Windows. An OpenGL driver is optional: without one the head renders with software Skia |
| WinWpfSkia | Windows |
| LinuxX11 | Linux with `DISPLAY` set - the head activates only when `DISPLAY` looks like `"[host]:display[.screen]"`. Runs on X11 desktops, and on Wayland desktops through XWayland |
| LinuxWayland | Linux with a running Wayland compositor; the head fails fast without one. Client-side decorations on GNOME and Cinnamon use the system's libdecor |
| LinuxFrameBuffer | Linux with no desktop, and access to the frame-buffer device, the DRM card and the input devices |
| MacOS | macOS on Apple Silicon or Intel; the head package carries its own native universal library |

On Debian-family desktops the Wayland head wants the libdecor GTK plugin present for a native-looking title bar - the packages `libdecor-0-0` and `libdecor-0-plugin-1-gtk`, preinstalled on most GNOME desktops.

Two add-ins need a system-installed engine on Linux, and each add-in's own `AGENT-README.txt` gives the exact command. The [WebView](add-ins/WebView.md) add-in needs the WPE WebKit runtime; the [MediaPlayer](add-ins/MediaPlayer.md) add-in needs libvlc.

```bash
sudo apt install libwpewebkit-2.0-1 libwpebackend-fdo-1.0-1 libwpe-1.0-1
sudo apt install libvlc5 vlc-plugin-base
```

On Windows the MediaPlayer add-in's engine arrives as a package reference on the Windows heads - [`VideoLAN.LibVLC.Windows`](https://www.nuget.org/packages/VideoLAN.LibVLC.Windows) lays the native runtime into the application output - and on macOS the add-in uses the platform's built-in AVFoundation support.

## Shipping to a frame-buffer device

The frame-buffer head owns the whole panel, so the requirements are about permissions rather than packages. The process must be able to open the frame-buffer device, the DRM card (`/dev/dri/card*`) and the input devices (`/dev/input/*`). On a Debian-family system that typically means membership of the `video` and `input` groups, or running as the console user - and a getty must not be fighting for the console.

Rendering follows from what the device can offer. By default the host tries an OpenGL ES context through DRM and GBM and falls back to software rendering onto the device named by the `FRAMEBUFFER` environment variable, `/dev/fb0` by default. On a GPU-less board software rendering is the normal mode, not a degraded one. A launcher can pin the choice with `CODEBRIX_FRAMEBUFFER_USE_DRM`, which overrides the builder - that is how a remote run over SSH forces software rendering, because DRM master is never available to a process that is not the active console.

Three things are switched off unless the application asks for them, and shipping without them is a run-time surprise rather than a build error: the file and folder pickers, the on-screen keyboard, and the in-process text clipboard. If the device follows its panel orientation through the accelerometer, the sensor daemon is a separate install:

```bash
apt install iio-sensor-proxy
```

A second instance of the same application refuses to start by default, because both instances would share the one frame buffer and each would receive every touch. Call `AllowMultipleApplicationInstances()` only when that is genuinely wanted. [02 - Runs on every laptop](02-runs-on-every-laptop.md) has the full builder surface for this head.

## The notices file

Anything you bundle, download at run time, or ship inside an assembly has a license, and the place to say so is one `THIRD-PARTY-NOTICES.txt` at the application root. It lists bundled content by path, with its origin, copyright and license - and it says what it deliberately does not cover:

```text
// From CodeBrix.Samples/PalmVisualizer/THIRD-PARTY-NOTICES.txt
Third-party CODE dependencies are consumed as NuGet packages. Each package
carries its own license and third-party notices in its own repository/package
(the CodeBrix.* packages ship their own THIRD-PARTY-NOTICES.txt), so those are
not reproduced here.

------------------------------------------------------------------------
MediaPipe models (bundled: models/**/*.tflite)
------------------------------------------------------------------------
```

Content fetched at run time is a different statement from content that ships in the repository, and it is worth making explicitly:

```text
// From CodeBrix.Samples/PolyHavenBrowser/THIRD-PARTY-NOTICES.txt
------------------------------------------------------------------------
Poly Haven assets (downloaded at run time)
------------------------------------------------------------------------
...
None of these assets are redistributed as part of this repository; they are
fetched on demand and cached locally.
```

Notice the four habits in those two excerpts: name the path each entry covers so a reader can match a file on disk to its license; say what the file does not cover; count bundled fonts as bundled content, with their license text beside them; and edit the file in the same change that adds an asset, not later.

## Checklist

- [ ] The application folder holds `src/`, `tests/`, its solutions, its `README.md` and its `THIRD-PARTY-NOTICES.txt`, and builds without anything outside it
- [ ] `.Core` carries every managed package; each head carries exactly one runtime package and its project and shared-project references
- [ ] Each head declares the `.xaml` page glob and the matching `None` removal
- [ ] Native per-platform packages sit on the heads, both architectures unconditionally, and no library names a runtime identifier
- [ ] Any add-in's native prerequisite is on the heads that need it, with the reason in a comment
- [ ] Every embedded asset has an explicit `<LogicalName>`, and the loader names that resource in its failure message
- [ ] There is one cross-platform solution, and a Windows superset for the heads that cannot restore elsewhere
- [ ] A native WinUI head's architectures are declared in the head and mapped in the solution
- [ ] The target machine's prerequisites are written down: the Linux engines for the add-ins used, and the device access the frame-buffer head needs
- [ ] Pickers, the on-screen keyboard and the clipboard are enabled explicitly if a frame-buffer build uses them
- [ ] `THIRD-PARTY-NOTICES.txt` lists every bundled and downloaded asset by path, and was updated in the change that added it

---

**Where to go next**

- [12 - Troubleshooting](12-troubleshooting.md) - the next chapter: what a blank window, a missing picker or a duplicate type means
- [04 - Project architecture](04-project-architecture.md) - the layout this chapter ships, project by project
- [02 - Runs on every laptop](02-runs-on-every-laptop.md) - each head's render paths and limits in full
- [CodeBrix.Samples on GitHub](https://github.com/ellisnet/CodeBrix.Samples) - every project file quoted above, in its application
