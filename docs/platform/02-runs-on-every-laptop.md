<sub>[CodeBrix](../../README.md) › [Build a CodeBrix.Platform application](README.md) › Runs on every laptop</sub>

# Runs on every laptop

**By the end of this chapter you will know which of the six platform heads to ship, what each one needs from the operating system, and how each one puts pixels on screen.** Every head runs the same `.Core` library and the same `.UI` XAML; what differs is one package reference, one bootstrap call, and a short list of platform facts. Those facts are this chapter.

## The six heads at a glance

| Head | Package | Bootstrap call | Renders with | Runs on |
| --- | --- | --- | --- | --- |
| Windows (Win32) | [CodeBrix.Platform.Runtime.Skia.Win32.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.Win32.ApacheLicenseForever) | `.UseWindowsWin32()` | OpenGL, software Skia otherwise | Windows |
| Windows (WPF) | [CodeBrix.Platform.Runtime.Skia.Wpf.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.Wpf.ApacheLicenseForever) | `.UseWindowsWpf()` | OpenGL by default; set Software after `Build()` | Windows |
| Linux (X11) | [CodeBrix.Platform.Runtime.Skia.X11.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.X11.ApacheLicenseForever) | `.UseLinuxX11()` | OpenGL via GLX, software otherwise | Linux with `DISPLAY` set, X11 desktops and Wayland desktops through XWayland |
| Linux (native Wayland) | [CodeBrix.Platform.Runtime.Skia.Wayland.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.Wayland.ApacheLicenseForever) | `.UseLinuxWayland()` | Vulkan, software otherwise | Linux with a running Wayland compositor |
| Linux (frame buffer) | [CodeBrix.Platform.Runtime.Skia.FrameBuffer.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.FrameBuffer.ApacheLicenseForever) | `.UseLinuxFrameBuffer()` | OpenGL ES over DRM and GBM, software otherwise | Linux with no desktop; embedded and kiosk devices |
| macOS | [CodeBrix.Platform.Runtime.Skia.MacOS.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.MacOS.ApacheLicenseForever) | `.UseMacOS()` | Metal, software otherwise | macOS on Apple Silicon and Intel |

Every `.Use...()` method is an extension method in the `CodeBrix.Platform.UI.Hosting` namespace, surfaced by the corresponding head package. A head sees only the one that matches its package, so you cannot select the wrong platform by accident. Every one except `UseMacOS()` also has an overload taking a configuration lambda.

`Build()` returns the concrete host for the selected head - `Win32Host`, `WpfHost`, `X11ApplicationHost`, `WaylandApplicationHost`, `FrameBufferHost` or `MacSkiaHost`, all deriving from `SkiaHost`, which derives from `CodeBrixPlatformHost`. Pattern-match on it to set host properties between `Build()` and `Run()`:

```csharp
var host = CodeBrixPlatformHostBuilder.Create()
    .App(() => new App())
    .AfterInit(() => Console.Error.WriteLine("host initialized"))
    .UseLinuxX11()
    .Build();
```

`AfterInit` runs after the host is initialized and before the run loop starts, which is where work belongs that needs the platform up but must precede the first frame.

## Windows in a Win32 window

**Package:** `CodeBrix.Platform.Runtime.Skia.Win32.ApacheLicenseForever`. **Bootstrap:** `.UseWindowsWin32()`.

Use the Win32 head for the simplest desktop experience on Windows. It renders with OpenGL when a driver is present and with software Skia otherwise. An OpenGL driver (ICD) is optional; nothing else has to be installed.

```csharp
ICodeBrixPlatformHostBuilder UseWindowsWin32()
ICodeBrixPlatformHostBuilder UseWindowsWin32(Action<Win32HostBuilder> action)

public class Win32HostBuilder
{
    public Win32HostBuilder PreloadMediaPlayer(bool preload);
        // pre-initializes the LibVLC media player at startup (only useful
        // with the MediaPlayer add-in; harmless otherwise)
}

// host (namespace CodeBrix.Platform.UI.Runtime.Skia.Win32)
public class Win32Host : SkiaHost
{
    public RenderSurfaceType? RenderSurfaceType { get; set; }  // null = auto-detect
}
public enum RenderSurfaceType { Software, OpenGL }
```

Leaving `RenderSurfaceType` null lets the head detect what is available. The framework-wide equivalent is `FeatureConfiguration.Rendering.UseOpenGLOnWin32`, a `bool?` whose default of null means "OpenGL when available, otherwise software".

**Known limits:** IME (composed CJK or dead-key) text input is not implemented on any Skia head, and initiating a drag is not implemented; accepting drops from other applications works.

## Windows hosted in WPF

**Package:** `CodeBrix.Platform.Runtime.Skia.Wpf.ApacheLicenseForever`. **Bootstrap:** `.UseWindowsWpf()`.

This head hosts the same application inside a WPF application context, which is what you want when a CodeBrix.Platform window has to live in a solution that is already WPF. For a plain Windows desktop application, prefer the Win32 head instead.

Two things about the project file are non-negotiable. The head must target the Windows-specific target framework moniker rather than the plain one, because the runtime package flows a `Microsoft.WindowsDesktop.App.WPF` FrameworkReference and the SDK requires a Windows target platform for that; without it you get NETSDK1136. The WPF head's project file in [03 - Your first application](03-your-first-application.md) shows the exact moniker to use. And the head must NOT set `<UseWPF>true</UseWPF>`, because that would make WPF's build targets treat the CodeBrix.Platform `.xaml` `<Page>` items as WPF XAML. WPF is loaded by the host at run time; the XAML stays CodeBrix.Platform XAML.

```csharp
ICodeBrixPlatformHostBuilder UseWindowsWpf(Action<IWindowsSkiaHostBuilder> windowsBuilder = null)

// extension methods on IWindowsSkiaHostBuilder
IWindowsSkiaHostBuilder WpfApplication(Func<System.Windows.Application> action)
    // supply your own WPF Application instance to host inside
IWindowsSkiaHostBuilder DispatcherScheduling(WpfDispatcherScheduling scheduling)

// host (namespace CodeBrix.Platform.UI.Runtime.Skia.Wpf)
public class WpfHost : SkiaHost
{
    public RenderSurfaceType? RenderSurfaceType { get; set; }        // null = auto
    public WpfDispatcherScheduling DispatcherScheduling { get; set; } // default RenderFirst
    public bool IgnorePixelScaling { get; set; }
}
public enum RenderSurfaceType { Software, OpenGL }
public enum WpfDispatcherScheduling
{
    RenderFirst = 0,  // pump runs at WPF DispatcherPriority.Render, above Input
    InputFair   = 1,  // for continuously-repainting apps: UI work cannot
                      // starve keyboard and pointer input
}
```

`DispatcherScheduling` is read once when the host initializes, from `Run()`, so set it either through `UseWindowsWpf(wpf => wpf.DispatcherScheduling(...))` or on the `WpfHost` after `Build()`. For continuously-repainting content such as games or live plots, use `WpfDispatcherScheduling.InputFair` so rendering cannot starve keyboard and pointer input.

> [!WARNING]
> The WPF host's default OpenGL renderer draws via raw OpenGL onto WPF's own DirectX-composited window, which causes "airspace" conflicts on many systems: the window appears but content never composites, leaving a blank window. Force software rendering right after `Build()`.

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

That block, and the extra `using CodeBrix.Platform.UI.Runtime.Skia.Wpf;` it needs, is in most applications the only per-head behavioral difference in the whole solution. Guarding the cast with `is` keeps the file valid whatever `Build()` returns.

## Linux on X11

**Package:** `CodeBrix.Platform.Runtime.Skia.X11.ApacheLicenseForever`. **Bootstrap:** `.UseLinuxX11()`.

This is the broad-compatibility desktop Linux head: it runs on X11 desktops and also on Wayland desktops through XWayland. It activates only when a `DISPLAY` environment variable is set and looks like `"[host]:display[.screen]"`. Pointer, keyboard and touch input are supported through XInput2.

Rendering is OpenGL through GLX by default, OpenGL ES through EGL on request, and software as the fallback.

```csharp
ICodeBrixPlatformHostBuilder UseLinuxX11()
ICodeBrixPlatformHostBuilder UseLinuxX11(Action<X11HostBuilder> action)

public partial class X11HostBuilder
{
    public X11HostBuilder RenderingBackend(X11RenderingBackend backend);
        // takes precedence over FeatureConfiguration.Rendering.UseOpenGLOnX11
    public X11HostBuilder RenderFrameRate(int renderFrameRate);   // default 60
    public X11HostBuilder PreloadMediaPlayer(bool preload);
}
public enum X11RenderingBackend
{
    Default,   // try OpenGL, fall back to software
    OpenGL,    // OpenGL via GLX, fall back to software
    OpenGLES,  // OpenGL ES via EGL, fall back to software
    Software,  // software rendering only
}
```

Choosing the backend and throttling the frame rate is two lines on the head builder:

```csharp
.UseLinuxX11(x11 => x11
    .RenderingBackend(X11RenderingBackend.OpenGLES)
    .RenderFrameRate(30))
```

A kiosk dashboard that updates once a second, or a battery-powered device, does not need 60 frames per second. The equivalent framework-wide flags, set before `Build()`, are `FeatureConfiguration.Rendering.UseOpenGLOnX11` (a `bool?`, null meaning OpenGL if available) and `PreferGLESOverGLOnX11` (a `bool`). The builder call takes precedence. The host type is `X11ApplicationHost` in namespace `CodeBrix.Platform.WinUI.Runtime.Skia.X11`, and it implements `IDisposable`.

**Known limits and sharp edges:**

- Accepting drops from other applications works; initiating a drag is not implemented. IME text input is not implemented.
- On some Linux ARM64 systems the native SkiaSharp library may fail to auto-load FreeType, throwing an "undefined symbol" error at startup. Preload FreeType when launching, for example `LD_PRELOAD=/usr/lib/aarch64-linux-gnu/libfreetype.so.6 dotnet run ...`. This is a SkiaSharp native-asset packaging issue, not a CodeBrix.Platform one.
- On Raspberry Pi OS running labwc the window may come up borderless; a labwc `windowRule` with `serverDecoration="yes"` in `~/.config/labwc/rc.xml` fixes it.

## Linux on native Wayland

**Package:** `CodeBrix.Platform.Runtime.Skia.Wayland.ApacheLicenseForever`. **Bootstrap:** `.UseLinuxWayland()`.

This head is a pure Wayland client: it speaks the Wayland protocol directly and never uses X11 or XWayland. It REQUIRES a Wayland compositor; without one it fails fast at startup with a clean "This application requires a Wayland compositor." message and exit code 1. It is permissively licensed top to bottom.

Rendering defaults to Vulkan through `VK_KHR_wayland_surface`, falling back to `wl_shm` software rendering when Vulkan is unavailable.

```csharp
ICodeBrixPlatformHostBuilder UseLinuxWayland()
ICodeBrixPlatformHostBuilder UseLinuxWayland(Action<WaylandHostBuilder> action)

public partial class WaylandHostBuilder
{
    public WaylandHostBuilder RenderingBackend(WaylandRenderingBackend backend);
        // takes precedence over the feature flags AND the environment variables
    public WaylandHostBuilder RenderFrameRate(int renderFrameRate);   // default 60
}
public enum WaylandRenderingBackend
{
    Default      = 0,   // Vulkan, falling back to software (same as omitting)
    Vulkan       = 1,   // same Vulkan-else-software selection, stated explicitly
    OpenGLES     = 2,   // OpenGL ES via EGL, falling back to software
    Software     = 3,   // wl_shm software rendering only
    VulkanForced = 11,  // Vulkan with NO fallback: if the Vulkan renderer cannot
                        // be created the app prints a clean two-line "requires
                        // Vulkan rendering" message to stderr and exits with
                        // code 1 (hardware qualification, perf tests)
}
```

Selecting a backend is one call:

```csharp
.UseLinuxWayland(wayland =>
    wayland.RenderingBackend(WaylandRenderingBackend.Vulkan))
```

The two GPU paths are peers: Vulkan and OpenGL ES each fall back directly to software, never to each other. The framework-wide flags are `FeatureConfiguration.Rendering.UseVulkanOnWayland` (`bool?`), `.UseOpenGLOnWayland` (`bool?`) and `.ForceVulkanOnWayland` (`bool`). Environment variables are consulted ONLY when neither the builder backend nor the feature flags decided: `CODEBRIX_WAYLAND_NO_GPU=1` forces software rendering and `CODEBRIX_WAYLAND_USE_EGL=1` selects the OpenGL ES path, with `NO_GPU` winning if both are set. Code always beats the environment. The host type is `WaylandApplicationHost` in namespace `CodeBrix.Platform.WinUI.Runtime.Skia.Wayland`.

**What the operating system must have:** a running compositor, and - on GNOME and Cinnamon, which expect the client to draw its own decorations - the system's libdecor library, packages `libdecor-0-0` and `libdecor-0-plugin-1-gtk`, preinstalled on most GNOME desktops. On KDE and wlroots-family compositors the server draws the decorations and nothing extra is needed. The window and taskbar icon comes from a `.desktop` file whose name matches the app id (the appxmanifest package name, falling back to the entry assembly name), placed in `~/.local/share/applications` or `/usr/share/applications` with an `Icon=` entry.

**At parity with the X11 head:** flyout-based controls (ComboBox drop-downs, MenuFlyout, ToolTip, dialogs), rich clipboard with text, HTML, PNG images, file lists and custom formats for both copy and paste, fractional display scaling, custom title bars through `ExtendsContentIntoTitleBar`, and window activation through xdg-activation.

**Known limits:**

- Touch input, native-view hosting in a `ContentPresenter` (which needs subsurfaces) and IME text input are not implemented in this head. Where a `ContentPresenter` would host a native view the content is ignored with a one-time warning; the WebView and MediaPlayer add-ins are windowing-agnostic and unaffected.
- Several window-management APIs are permanent protocol-level differences, not bugs and not planned work, and each logs a one-time warning naming the API on first use. `AppWindow.Move` and any window positioning: the compositor owns placement, and `AppWindow.Position` always reports (0,0). `AppWindow.Resize` and `ApplicationView.TryResizeView`: a client cannot force its outer window size, though the initial size through `ApplicationView.PreferredLaunchViewSize` does work. `OverlappedPresenter.IsAlwaysOnTop`: core Wayland and xdg-shell have no always-on-top for regular application windows. `OverlappedPresenter.IsMinimizable` and `IsMaximizable`: xdg-shell cannot remove those capabilities, and compositor or decoration policy decides. Minimized-state readback: a client can request minimize, but Wayland never reports whether the window was unminimized, so `OverlappedPresenter.State` may report Minimized while the window is visible again - maximize and restore state do reflect correctly, including an external maximize from the title bar.
- Accepting drag-and-drop is implemented and behaves correctly per protocol, but compositors with experimental Wayland sessions can deliver unusable drag events, so a drop may silently do nothing. Drag-and-drop works normally on the X11 head.
- Window self-activation through `Window.Activate()` rides xdg-activation-v1 and is subject to the compositor's focus-stealing policy: without a recent user interaction the compositor may only flag the window as demanding attention rather than focusing it.

## Linux on the frame buffer

**Package:** `CodeBrix.Platform.Runtime.Skia.FrameBuffer.ApacheLicenseForever`. **Bootstrap:** `.UseLinuxFrameBuffer()`.

Use the frame-buffer head for embedded and kiosk devices with no X11 or desktop environment. Same application code; different head package and `.UseLinuxFrameBuffer()`. The application owns the whole panel: there is no window manager, no window chrome, and one surface.

**What the operating system must have:** the process must be able to open the frame-buffer device, the DRM card (`/dev/dri/card*`) and the input devices (`/dev/input/*`) - on a Debian-family system that typically means membership of the `video` and `input` groups, or running as the console user - and a getty must not be fighting for the console. The head P/Invokes the distro's libinput and libxkbcommon for input, and DRM, GBM and EGL for GPU rendering.

**How it renders:** by default the host tries to create an OpenGL ES context through DRM and GBM, scanning `/dev/dri/card[0-9]+` unless `UseKMSDRM` gives a card path, and if that fails it logs an error and falls back to software rendering onto the frame-buffer device named by the `FRAMEBUFFER` environment variable, `/dev/fb0` by default. `UseKMSDRM()` requires the DRM path with no fallback; `DisableKMSDRM()` forces software. A launcher can pin the choice with `CODEBRIX_FRAMEBUFFER_USE_DRM`, which overrides the builder - that is how a remote run over SSH forces software `/dev/fb0` rendering, because DRM master is never available to a process that is not the active console. On GPU-less systems software rendering is the normal mode, not a degraded one.

Everything this head can be told to do lives on one builder.

<details>
<summary>The full FrameBuffer host builder and its option types</summary>

```csharp
ICodeBrixPlatformHostBuilder UseLinuxFrameBuffer()
ICodeBrixPlatformHostBuilder UseLinuxFrameBuffer(Action<FramebufferHostBuilder> action)

public partial class FramebufferHostBuilder
{
    // rendering
    public FramebufferHostBuilder UseKMSDRM(string? cardPath = null,
        DRMFourCCColorFormat? gbmSurfaceColorFormat = null,
        DRMConnectorChooserDelegate? connectorChooser = null);
    public FramebufferHostBuilder DisableKMSDRM();
    public FramebufferHostBuilder ScaleUserInterface(UserInterfaceScale scale);

    // mouse cursor
    public FramebufferHostBuilder EnableMouseCursor(float radius, System.Drawing.Color color);
    public FramebufferHostBuilder DisableMouseCursor();

    // orientation
    public FramebufferHostBuilder Orientation(DisplayOrientations orientation,
        bool isPreferredOrientation = false);
    public FramebufferHostBuilder AutoRotationEnabled(params DisplayOrientations[] orientations);
    public FramebufferHostBuilder AutoRotationEnabled(bool enabled);
    public FramebufferHostBuilder UseOrientationSensor();

    // keyboard
    public FramebufferHostBuilder XkbKeymap(XKBKeymapParams keymapParams);
    public FramebufferHostBuilder EnableSoftwareKeyboard(SoftwareKeyboardOptions? options = null);

    // in-application dialogs and clipboard (all OFF unless enabled)
    public FramebufferHostBuilder EnableFileOpenPicker(FilePickerOptions? options = null);
    public FramebufferHostBuilder EnableFileSavePicker(FilePickerOptions? options = null);
    public FramebufferHostBuilder EnableFolderPicker(FolderPickerOptions? options = null);
    public FramebufferHostBuilder EnableSimpleTextClipboard();

    // process policy
    public FramebufferHostBuilder AllowMultipleApplicationInstances();

    public readonly record struct DRMFourCCColorFormat(char C1, char C2, char C3, char C4);
    public readonly record struct DRMConnector(uint connectorType, uint connectorTypeId,
        uint connectorId, string connectorStringRepresentation);
    public delegate int DRMConnectorChooserDelegate(IReadOnlyList<DRMConnector> connector);
    public readonly record struct XKBKeymapParams(string? model = null, string? rules = null,
        string? layout = null, string? variant = null, string? options = null);
}

public class FrameBufferHost : SkiaHost, IDisposable
{
    public float? DisplayScale { get; set; }
        // overrides the framebuffer's default scale; the
        // CODEBRIX_DISPLAY_SCALE_OVERRIDE environment variable overrides it
}

public enum UserInterfaceScale { Percent100 = 100, Percent150 = 150, Percent200 = 200 }

public class FilePickerOptions
{
    public bool AllowNewFolderCreate { get; set; }
    public string? RestrictToFolder { get; set; }
    public string? RequiredExtension { get; set; }
    public string? StartFolder { get; set; }
    public bool AllowMultipleFileSelect { get; set; } = true;
    public bool ShowHiddenFiles { get; set; }
    public bool ShowHiddenFolders { get; set; }
}
public class FolderPickerOptions
{
    public bool AllowNewFolderCreate { get; set; }
    public string? RestrictToFolder { get; set; }
    public string? StartFolder { get; set; }
    public bool ShowHiddenFolders { get; set; }
}
public class SoftwareKeyboardOptions
{
    public string? Layout { get; set; }               // null = resolved from the system
    public IList<string>? EnabledLayouts { get; set; }
    public bool ShowDismissKey { get; set; } = true;
    public bool AllowLockOn { get; set; }
    public SoftwareKeyHeight KeyHeight { get; set; } = SoftwareKeyHeight.PortraitFullLandscapeFull;
}
public enum SoftwareKeyHeight
{
    PortraitFullLandscapeFull, PortraitHalfLandscapeHalf,
    PortraitFullLandscapeHalf, PortraitHalfLandscapeFull,
}
```

</details>

A touch kiosk configured with orientation, scale, pickers, an on-screen keyboard and the in-process clipboard reads like this:

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

Everything in that lambda is opt-in on purpose, and each piece behaves in a specific way:

- **Pickers and keyboard.** Without `EnableFileOpenPicker`, `EnableFileSavePicker` or `EnableFolderPicker`, the standard `Windows.Storage.Pickers` APIs THROW `NotSupportedException` on this head. Enabled, they show a modal in-application dialog drawn on top of all app content, inside the application frame. `EnableSoftwareKeyboard` shows an on-screen keyboard automatically when a `TextBox` or `PasswordBox` gains focus, and honors `InputPane.TryShow()` and `TryHide()`; while it is visible the application's layout height is reduced so the focused field is never covered.
- **Clipboard.** Without `EnableSimpleTextClipboard` the head has no clipboard at all and clipboard use logs "not implemented". With it, a text-only, in-process, last-in-only-out clipboard exists. Nothing reaches a system clipboard, because there is none.
- **Input.** Input comes from libinput for mouse, touch and keyboard. The mouse cursor is drawn by the head: by default it appears after the first mouse event and never appears for touch-only use. `EnableMouseCursor` forces a small circle of the given radius and color; `DisableMouseCursor` hides it, which is what a touch-only device wants. Keyboard layouts come from libxkbcommon: `XkbKeymap` sets the RMLVO parameters, and if unset the system default is used, consulting `XKB_DEFAULT_LAYOUT`.
- **Orientation.** `Orientation(...)` with `isPreferredOrientation: false`, the default, is a ROTATION applied relative to the panel's scanout, so Landscape means no rotation and leaves a portrait-native panel portrait. With `isPreferredOrientation: true` it states the orientation the application WANTS TO BE and the rotation is worked out from the panel's native geometry. `AutoRotationEnabled(...)` lists the device orientations honored at run time - sugar over `DisplayInformation.AutoRotationPreferences`, which remains the source of truth - and `AutoRotationEnabled(false)` locks the application. `UseOrientationSensor()` follows the accelerometer through iio-sensor-proxy (`apt install iio-sensor-proxy`); a launcher can override the source with `CODEBRIX_FRAMEBUFFER_ORIENTATION_SOURCE`, whose values include `sensor` and `none`, with the builder honored when it is unset.
- **Scale.** `ScaleUserInterface` draws the UI larger for a dense panel: layout happens in logical units, pixels divided by scale, while drawing keeps every real pixel, so nothing is upscaled and it costs no fill rate.
- **Second instance.** By default a second instance of the same application refuses to start with an informative error, because both would share the one frame buffer and each would receive every touch. Call `AllowMultipleApplicationInstances()` only when that is wanted.

**Known limits:** no system clipboard, no window management, and no pickers or keyboard unless enabled. Software rendering at very high resolutions costs fill rate, so keep the panel's native resolution in mind.

## macOS

**Package:** `CodeBrix.Platform.Runtime.Skia.MacOS.ApacheLicenseForever`. **Bootstrap:** `.UseMacOS()`.

The macOS head package contains a small native library shipped as a universal binary, so it runs on Apple Silicon and Intel Macs. Rendering is Metal by default with a software fallback.

```csharp
ICodeBrixPlatformHostBuilder UseMacOS()          // no configuration overload

// host (namespace CodeBrix.Platform.UI.Runtime.Skia.MacOS)
public class MacSkiaHost : SkiaHost
{
    public RenderSurfaceType RenderSurfaceType { get; set; }
}
public enum RenderSurfaceType { Auto, Metal, Software }
```

`UseMacOS()` is the one bootstrap call with no configuration overload; configure through `FeatureConfiguration` and through the host after `Build()`. The framework-wide flag is `FeatureConfiguration.Rendering.UseMetalOnMacOS`, a `bool?` whose default of null uses Metal if available and software otherwise.

**Known limits:** IME text input and initiating drag-and-drop are not implemented, as on every Skia head. Accepting drops works.

## Choosing which heads to ship

Ship the Win32 head for Windows and the macOS head for macOS; those choices are not really choices. Add the WPF head only when the application has to live inside an existing WPF process.

Desktop Linux is the interesting decision, and the answer is usually both Linux desktop heads. Ship the X11 head for maximum desktop-Linux reach: it runs on X11 desktops and, through XWayland, on Wayland desktops too, so one executable covers essentially every desktop Linux machine. Ship the Wayland head alongside it when you want a genuine native Wayland client - one that speaks the protocol directly, never touches XWayland, and gets Vulkan rendering and fractional scaling from the compositor.

> [!TIP]
> Do not expect the Wayland head to run in an X11-only session. It requires a Wayland compositor and fails fast by design when none is present. For an application that must run everywhere on desktop Linux, ship the X11 head - alone, or alongside a Wayland head. Prefer the X11 head where the application depends on touch input or native-view hosting.

The frame-buffer head is a separate decision entirely: add it when you are shipping to an embedded or kiosk device that has no desktop at all.

Whichever heads you ship, pick the render backend deliberately. GPU paths win for large windows and animation; software Skia is perfectly adequate for form-style UIs and is the ONLY option over SSH, in virtual machines without 3D, and on GPU-less boards. Use `WaylandRenderingBackend.VulkanForced` when you need proof that the GPU path is really in use.

## Checklist

- [ ] Each head project references exactly one `CodeBrix.Platform.Runtime.Skia.*` package
- [ ] Each head's `Program.cs` calls the one `.Use...()` method its package surfaces
- [ ] The WPF head targets the Windows-specific framework moniker, does not set `<UseWPF>`, and forces the software render surface after `Build()`
- [ ] Desktop Linux ships the X11 head, with the Wayland head alongside it when a native Wayland client is wanted
- [ ] The Wayland head's target machines have a compositor, and libdecor where the desktop expects client-side decorations
- [ ] The frame-buffer head's target device grants access to the frame buffer, the DRM card and the input devices, with no getty on the console
- [ ] Pickers, the on-screen keyboard and the clipboard are explicitly enabled on the frame-buffer head if the application uses them
- [ ] No code depends on window positioning, forced resize or always-on-top if the application ships the Wayland head

---

**Where to go next**

- [03 - Your first application](03-your-first-application.md) - build all of this, file by file
- [12 - Troubleshooting](12-troubleshooting.md) - what a blank window, a missing picker or a fast-exit at startup means
- [07 - Platform services](07-platform-services.md) - windowing, dispatching, pickers and the clipboard across the heads
- [CodeBrix.Platform on GitHub](https://github.com/ellisnet/CodeBrix.Platform) - source, tests and samples
