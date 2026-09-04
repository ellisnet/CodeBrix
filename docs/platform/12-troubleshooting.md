<sub>[CodeBrix](../../README.md) › [Build a CodeBrix.Platform application](README.md) › Troubleshooting</sub>

# Troubleshooting

**By the end of this chapter you will be able to take a symptom - nothing starts, the window is black, the build reports a duplicate type, a member throws - and name its cause without guessing.** It is organized by what you see, not by what the framework does: startup failures per head, rendering fallbacks, build errors, "not implemented" exceptions, the capabilities a head does not have, and the performance switches worth reaching for.

Most first-run surprises are here. If yours is not, the two places to look next are the head's own section in [02 - Runs on every laptop](02-runs-on-every-laptop.md) and the `AGENT-README.txt` of the add-in involved.

## Start here: turn the logs on

The framework logs through `Microsoft.Extensions.Logging`. The bridge is enabled by setting `LogExtensionPoint.AmbientLoggerFactory` and then calling `LoggingAdapter.Initialize()` - both of which the reference `App.InitializeLogging()` does, from every head's `Main`, before the host is built:

```csharp
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
```

Notice the filter. Framework categories start with `CodeBrix.Platform` and sit at Warning in normal use, which is where the interesting messages are - a head that could not create a GPU context and fell back to software, or an API that is a protocol-level no-op on this head. Drop that filter to Information while you are diagnosing, and put it back afterwards: console logging at Information level in Release costs frames.

There is also an in-application panel. One line from a page shows it:

```csharp
DiagnosticsOverlay.Get(this.XamlRoot).Show();
```

## The application does not start

### The Wayland head exits immediately

The native Wayland head requires a Wayland compositor and fails fast when none is present, printing a clean `This application requires a Wayland compositor.` message and exiting with code 1. That is by design, not a bug. In an X11-only session, run the X11 head instead - it is the head to ship for maximum desktop-Linux reach, because it runs on X11 desktops and on Wayland desktops through XWayland.

A second, deliberate fast exit is `WaylandRenderingBackend.VulkanForced`: if the Vulkan renderer cannot be created the application prints a two-line "requires Vulkan rendering" message to standard error and exits with code 1. That mode exists to prove the GPU path is really in use; it is not a mode to ship.

### The X11 head starts and no window appears

The X11 head only activates when `DISPLAY` is set and looks like `"[host]:display[.screen]"`. Over a bare SSH session, in a container without an X socket, or under a service manager that strips the environment, `DISPLAY` is usually the missing piece.

### A second frame-buffer instance refuses to start

By default a second instance of the same frame-buffer application refuses to start, with an informative error: both instances would share the one frame buffer and each would receive every touch. Call `AllowMultipleApplicationInstances()` on the builder only when that is genuinely wanted.

### The frame-buffer head starts but renders in software

Over SSH, or on a board with no GPU, expect software rendering onto `/dev/fb0` - and prefer it. DRM master is never available to a process that is not the active console, so the DRM and GBM path cannot be used from a remote session. A launcher pins the choice with `CODEBRIX_FRAMEBUFFER_USE_DRM`, which overrides the builder. If the head cannot open the frame-buffer device, the DRM card or the input devices at all, the process lacks the group membership those devices need - typically `video` and `input` on a Debian-family system - or a getty is holding the console.

### An "undefined symbol" error at startup on ARM64 Linux

On some Linux ARM64 systems the native SkiaSharp library may fail to auto-load FreeType and throws an "undefined symbol" error at startup. Preload FreeType when launching:

```bash
LD_PRELOAD=/usr/lib/aarch64-linux-gnu/libfreetype.so.6 dotnet run ...
```

This is a SkiaSharp native-asset packaging issue, not a CodeBrix.Platform issue.

### The host builder throws before the window exists

`App.InitializeLogging()` must be called from `Main` before `CodeBrixPlatformHostBuilder.Create()`, in every head. Calling the builder first is the one ordering mistake in `Program.Main` that the framework cannot recover from.

## The window is blank, black or undecorated

> [!WARNING]
> The WPF-hosted head needs the software render surface set after `Build()`. Without it you may get a blank or black window from the OpenGL airspace conflict between the Skia surface and WPF's own rendering. It is one `if` block, and it is the only post-`Build()` line any head requires.

```csharp
var host = CodeBrixPlatformHostBuilder.Create()
    .App(() => new App())
    .UseWindowsWpf()
    .Build();

if (host is WpfHost wpfHost)
{
    wpfHost.RenderSurfaceType = RenderSurfaceType.Software;
}

host.Run();
```

Other appearance failures have shorter answers:

- **The window has no border on a Wayland desktop.** On GNOME and Cinnamon the decorations are drawn client-side through the system's libdecor, so install `libdecor-0-0` and `libdecor-0-plugin-1-gtk`. On KDE and wlroots-family compositors the server draws them and nothing is needed.
- **The window comes up borderless on Raspberry Pi OS (labwc).** A labwc `windowRule` with `serverDecoration="yes"` in `~/.config/labwc/rc.xml` fixes it.
- **The window renders, but slowly, and you expected the GPU.** Every head falls back to software Skia when its GPU path is unavailable, and logs why. Software Skia is perfectly adequate for form-style UIs and is the only option over SSH, in virtual machines without 3D, and on GPU-less boards. To confirm which path is really in use on Wayland, run once with `WaylandRenderingBackend.VulkanForced`.
- **The window has no icon on Wayland.** The window and taskbar icon come from a `.desktop` file whose name matches the application id, placed in `~/.local/share/applications` or `/usr/share/applications` with an `Icon=` entry.
- **`Window.Activate()` does not bring the window forward on Wayland.** Self-activation rides xdg-activation-v1 and is subject to the compositor's focus-stealing policy: without a recent user interaction the compositor may only flag the window as demanding attention.

## The build fails

| What the build says | What it means |
| --- | --- |
| NETSDK1136, about a WPF framework reference | The WPF head is targeting the plain framework moniker. It must target the Windows-specific one, because its runtime package flows a WPF framework reference |
| CS0433, a duplicate `GlobalStaticResources` type, reported in a head | A library that also references CodeBrix.Platform is using the application's root namespace. Give that library its own `<RootNamespace>` |
| CS0234, on one head only, on an inline `Windows.` reference | That head is named `.Windows`, so its own namespace shadows the global `Windows` namespace. Name the Win32 head `.Win32Skia` |
| Your XAML pages are not compiled, or `InitializeComponent` is missing | The head is missing the `.xaml` page glob and the matching `None` removal, or the `.UI` `.projitems` import |
| The WPF build targets claim your pages | `<UseWPF>true</UseWPF>` is set on the WPF-hosted Skia head. It must not be. A genuinely native WPF head does set it |
| Conditional compilation behaves as if the framework were absent | `HAS_CODEBRIX` and `HAS_CODEBRIX_WINUI` are missing from `.Core` or from a head. Some public API - `Window.AppWindow` among it - is public only when `HAS_CODEBRIX_WINUI` is defined |
| The solution will not build with Any CPU selected | A native WinUI head declares only x86, x64 and ARM64. Restrict the solution platforms to match and map each one onto that project |
| A target framework below .NET 10 | CodeBrix.Platform requires .NET 10 or later |

Two build outcomes are worse than an error, because they succeed: a head with a second runtime package, and `.Core` with a head package in it. Nothing warns you, and the run does not work. One head project, one head package; every add-in referenced once, in `.Core`. [04 - Project architecture](04-project-architecture.md) has the rule in full.

The CS0433 case is the one most often misdiagnosed, because it is reported in the head rather than in the library that caused it. The fix is one property on the library:

```xml
<!-- From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/CodeBrixVideoTool.Playback.csproj -->
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>

  <!-- This library hosts a SimpleViewModel-derived view model, so it references CodeBrix.Platform.
       Keep this library's OWN RootNamespace (not the app's "CodeBrixVideoTool") so the per-head
       generated GlobalStaticResources class does not collide across assemblies (CS0433). -->
  <RootNamespace>CodeBrixVideoTool.Playback</RootNamespace>
</PropertyGroup>
```

Notice that the rule is conditional. A library that hosts no XAML-facing type keeps its default root namespace, which is already its assembly name and therefore already distinct. One library goes the other way on purpose: a library whose embedded fonts are looked up by a name derived from the root namespace must not override it. Decide which rule a library is under before touching the property.

## A member throws "not implemented"

The framework keeps the full WinUI and UWP shape - types, properties, methods, events - so that your code, your XAML and third-party libraries compile unchanged. A subset of those members has no meaningful behavior on the supported targets, or is not implemented yet. Rather than silently doing the wrong thing, such a member throws an exception naming itself:

```text
The member `string SomeType.SomeMember` is not implemented.
```

What to do, in order: read the member name out of the message; look for an implemented alternative API; guard the call at run time if the code has to run on more than one path; and open an issue on the [CodeBrix.Platform repository](https://github.com/ellisnet/CodeBrix.Platform) if the member matters to your application. What not to do is expect it to work by trying harder - the message names the member precisely because there is nothing behind it.

Some absences are architectural rather than incomplete, and no amount of configuration reaches them:

- There is no mobile target and no WebAssembly or browser target, ever.
- Vulkan on X11 is not available to consumers: the renderer sits behind an internal flag with no public API, and `X11RenderingBackend` has no Vulkan member.
- 2D and 3D canvases, Lottie, SVG, media, audio, video, an embedded browser, a code editor, a terminal, charts, flex layout and a settings store are not in the core packages at all. They are separate add-in packages, listed in [08 - Add-ins](08-add-ins.md).

## Drag-and-drop and IME

Two input capabilities are not implemented on any Skia head:

- **Initiating a drag** from the application. Accepting drops from other applications works on X11, Windows and macOS, and on Wayland subject to the compositor.
- **IME text input** - composed CJK entry and dead keys.

On Wayland specifically, accepting drops is implemented and behaves correctly per the protocol, but some compositors running experimental Wayland sessions deliver unusable drag events: coordinates arrive wrong, hit-testing never finds a drop target, and the drop silently does nothing. That is a compositor-side defect. Drag-and-drop works normally on the X11 head, which is the head to prefer where an application depends on it.

## A head cannot do what the code assumes

> [!WARNING]
> On the frame-buffer head the file and folder pickers, the on-screen keyboard and the clipboard are off unless the builder switches them on. Without `EnableFileOpenPicker`, `EnableFileSavePicker` or `EnableFolderPicker`, the standard `Windows.Storage.Pickers` APIs throw `NotSupportedException`; without `EnableSimpleTextClipboard` the head has no clipboard at all and clipboard use logs "not implemented". The build is clean either way.

The frame-buffer head also has no window management and no window chrome. The application owns the whole panel, and the clipboard it can be given is text-only, in-process and last-in-only-out - nothing reaches a system clipboard, because there is none.

On the Wayland head a second group of APIs are protocol-level no-ops rather than gaps. Each logs a one-time Warning naming the API on first use, and none of them will ever be implemented, because core Wayland does not offer them:

| API | What happens on Wayland |
| --- | --- |
| `AppWindow.Move` and any window positioning | The compositor owns placement. `AppWindow.Position` always reports (0,0) |
| `AppWindow.Resize`, `ApplicationView.TryResizeView` | A client cannot force its outer window size. The initial size through `ApplicationView.PreferredLaunchViewSize` does work |
| `OverlappedPresenter.IsAlwaysOnTop` | Core Wayland and xdg-shell have no always-on-top for regular application windows |
| `OverlappedPresenter.IsMinimizable`, `IsMaximizable` | xdg-shell cannot remove those capabilities; compositor and decoration policy decide |
| Minimized-state readback | A client can request minimize, but Wayland never reports whether the window was unminimized, so `OverlappedPresenter.State` may report Minimized while the window is visible. Maximize and restore state do reflect correctly |

Design the UI so it does not need any of them, or accept that they work only on X11, Windows and macOS. Two more Wayland gaps are deferred rather than permanent: touch input, and native-view hosting inside a `ContentPresenter`, where the content is ignored with a one-time warning. The WebView and MediaPlayer add-ins are windowing-agnostic and unaffected.

## Dialogs, themes and threads

- **A `ContentDialog` does not appear, or throws.** Set `XamlRoot` on it before showing it, and never keep two on screen at once - re-showing a dialog that is already showing throws `InvalidOperationException` with the message "A ContentDialog is already opened."
- **Setting `Application.RequestedTheme` throws `NotSupportedException`.** It can only be set before `InitializeComponent()`, in the `App` constructor. To switch theme at run time, set `FrameworkElement.RequestedTheme` on the root element instead.
- **A UI object touched from a background thread misbehaves or throws.** Capture the `DispatcherQueue` on the UI thread and `TryEnqueue` back to it; check `HasThreadAccess` when you are unsure which thread you are on. [05 - MVVM the right way](05-mvvm-the-right-way.md) has the patterns.

## Text is missing glyphs or is in the wrong font

The default text font is `"Segoe UI"`, which is not present on Linux or macOS. Set a bundled font in the `App` constructor - before any text has been measured - and reference the font package from `.Core` so every head ships it:

```csharp
global::CodeBrix.Platform.UI.FeatureConfiguration.Font.DefaultTextFontFamily =
    "ms-appx:///CodeBrix.Platform.Fonts.OpenSans/Fonts/OpenSans.ttf";
```

The related settings have different timing rules, and getting them the wrong way round is the usual cause of a first screen in the wrong face:

- `Font.DefaultTextFontFamily` must be set before the first text is measured - the `App` constructor is the place.
- `Font.SymbolsFont`, the font behind `SymbolIcon` glyphs, must be set AFTER `App.InitializeComponent()`.
- `Font.FallbackFontFamilies` lists the fonts tried, in order, for a character the requested font lacks - before the host machine's fonts.
- `Font.RestrictToEmbeddedFonts` confines resolution to the fonts the application ships.

A character no font can supply renders as the font's `.notdef` glyph - blank or a box, depending on the font. The framework never substitutes the host system's fonts unless `FallbackFontFamilies` is left empty and the application's own fonts have no glyph. If the first screen re-lays-out as the font arrives, preload it:

```csharp
await FontFamilyHelper.PreloadAsync(
    "ms-appx:///CodeBrix.Platform.Fonts.OpenSans/Fonts/OpenSans.ttf",
    Windows.UI.Text.FontWeights.Normal, Windows.UI.Text.FontStretch.Normal,
    Windows.UI.Text.FontStyle.Normal);
```

## The UI is slower than it should be

Work down this list; the first two items account for most of it.

- **Pick the render backend deliberately.** The defaults are OpenGL on Win32, OpenGL through GLX on X11 with OpenGL ES available on request, Vulkan on Wayland, DRM and GBM with OpenGL ES on the frame buffer, Metal on macOS - each falling back to software. The WPF head is the exception that wants software. GPU paths win for large windows and animation.
- **Throttle the frame rate where it helps.** `X11HostBuilder.RenderFrameRate` and `WaylandHostBuilder.RenderFrameRate` default to 60. A kiosk dashboard that updates once a second, or a battery-powered device, does not need 60 frames a second.
- **On the WPF head, set `DispatcherScheduling = WpfDispatcherScheduling.InputFair`** for continuously repainting content such as games and live plots, so rendering cannot starve keyboard and pointer input.
- **Let the framework skip clean subtrees** with `FeatureConfiguration.Rendering.EnableVisualSubtreeSkippingOptimization` and its two thresholds, which avoids re-rendering subtrees that have not changed.
- **Keep `FeatureConfiguration.TextBlock.IsMeasureCacheEnabled` at its default of true**, and preload fonts so the first screen does not re-layout when the font arrives.
- **Lists virtualize already.** `FeatureConfiguration.ListViewBase.DefaultCacheLength` trades memory for scroll smoothness. Use an `ObservableCollection<T>` rather than resetting `ItemsSource`.
- **In navigation-heavy applications** turn on `FeatureConfiguration.Page.IsPoolingEnabled`, which reuses `Page` instances across `Frame` navigations.
- **Prefer `{x:Bind}` over `{Binding}`** in hot templates - it is compiled.
- **Keep the framework's own logging filtered to Warning.** The DEBUG-only logging block is DEBUG-only on purpose.
- **In canvas-heavy applications** try `UseDirectSkiaCanvasMode()` on the host builder: one fewer full-frame copy per paint. It is experimental and app-wide - measure before keeping it.
- **On the frame buffer**, `ScaleUserInterface` keeps every real pixel, so it costs no fill rate; software rendering at very high resolutions does, so keep the panel's native resolution in mind.

## The pitfall list in full

The framework's own list, condensed. Every item here is something that has caught someone.

- Package IDs carry a license suffix; namespaces do not. There is no package named plain `CodeBrix.Platform`.
- Never reference a head package from `.Core`, never put two head packages in one head, and never put an add-in package in a head.
- Define `HAS_CODEBRIX` and `HAS_CODEBRIX_WINUI` in `.Core` and in every head.
- Never set `<UseWPF>true</UseWPF>` on the WPF-hosted Skia head, and never target the plain framework moniker there.
- Never omit the software-rendering line on that head.
- Declare `.xaml` as `Page` items in each head and import the `.UI` `.projitems`; do not move the Views into `.Core`.
- Do not target a framework below .NET 10.
- Do not call `CodeBrixPlatformHostBuilder` before `App.InitializeLogging()`.
- Do not expect the Wayland head to run in an X11-only session.
- Do not name the Win32 head `.Windows`.
- Do not show a `ContentDialog` without `XamlRoot`, and do not keep two on screen.
- Do not set `Application.RequestedTheme` after `InitializeComponent()`.
- Do not touch UI objects from a background thread.
- Do not expect pickers, an on-screen keyboard or a clipboard on the frame-buffer head unless you enabled them.
- Do not reference [`CodeBrix.Platform.Runtime.Skia.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.ApacheLicenseForever) directly - it arrives transitively - and never reference [`CodeBrix.Platform.Runtime.Skia.FrameBuffer.Emulated.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.FrameBuffer.Emulated.ApacheLicenseForever), an off-screen variant of the frame-buffer head used by tooling.
- Do not rely on window positioning, forced resize or always-on-top on the Wayland head.
- Do not set `FeatureConfiguration.Font.DefaultTextFontFamily` after the first text has been measured.
- Do not expect a "not implemented" member to work by trying harder.

Two more, from the sample repositories rather than the framework: never launch a real frame-buffer build on a desktop machine, because it draws into `/dev/fb0` and takes over the console invisibly; and when you copy from a sample inside the framework repository, copy its project structure and its XAML and C#, not its reference lines - those samples reference the framework by project path, which is the opposite of what an application does.

## Checklist

- [ ] Logging is wired in every head's `Main`, and the framework filter goes to Information only while diagnosing
- [ ] The WPF head sets the software render surface after `Build()`
- [ ] Each head references exactly one runtime package, and every add-in is referenced once in `.Core`
- [ ] Every library that references CodeBrix.Platform and hosts a XAML-facing type keeps its own root namespace
- [ ] No code path depends on initiating a drag, on IME input, or on Wayland window positioning
- [ ] A frame-buffer build enables the pickers, keyboard and clipboard it uses
- [ ] A bundled font is set as the default in the `App` constructor, and the symbols font after `InitializeComponent()`
- [ ] Background work marshals back through the `DispatcherQueue` before touching UI objects
- [ ] Every "not implemented" message encountered has been answered with an alternative API or a run-time guard

---

**Where to go next**

- [13 - Reference applications](13-reference-applications.md) - the next chapter: complete applications to compare yours against
- [02 - Runs on every laptop](02-runs-on-every-laptop.md) - each head's prerequisites, render paths and limits
- [11 - Packaging and shipping](11-packaging-and-shipping.md) - what the target machine needs before any of this runs
- [CodeBrix.Platform on GitHub](https://github.com/ellisnet/CodeBrix.Platform) - source, tests and samples
