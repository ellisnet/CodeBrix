<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Testing your application](../10-testing-your-application.md) › [PlayTest](README.md) › OpenGL</sub>

# OpenGL in PlayTests

**An application that uses the [Graphics3DGL](../add-ins/Graphics3DGL.md) add-in gets real OpenGL contexts in its PlayTests when the test project references `CodeBrix.Platform.PlayTest.OpenGL.ApacheLicenseForever` and the fixture registers it before launch.** From then on its GL elements initialize and render on the application's UI thread as on a desktop head, and their pixels appear in every screenshot and in the preview. Without the companion package a PlayTest run has no OpenGL at all.

| Element | With the provider registered |
| --- | --- |
| `GLCanvasElement` | Draws, and its pixels appear in screenshots |
| `SkiaGLCanvasElement` | `IsGpuInitialized` is `true`; GPU Skia pixels appear too |
| `OffscreenGLContext` | `TryCreate` succeeds |
| `SkiaGpuContext` | `TryCreate` succeeds - also the VideoPlayer add-in's GPU path |

## Add it

Reference the package from the PlayTests project, beside the PlayTest package. It plugs into that package and does not bring it.

```xml
<ItemGroup>
  <PackageReference Include="CodeBrix.Platform.PlayTest.ApacheLicenseForever" />
  <PackageReference Include="CodeBrix.Platform.PlayTest.OpenGL.ApacheLicenseForever" />
</ItemGroup>
```

Its one package dependency is `CodeBrix.Platform.Graphics3DGL.ApacheLicenseForever`, which brings the GL binding and, for macOS, the ANGLE libraries. Never reference it from an application or head project.

Then register the provider in the fixture, **before** `LaunchAsync`:

```csharp
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Platform.PlayTest.OpenGL;
using Xunit;

public sealed class AppFixture : IAsyncLifetime
{
    public PlayTestApplication Application { get; private set; }

    public async ValueTask InitializeAsync()
    {
        CodeBrixPlayTestOpenGL.Register();
        Application = await PlayTestApplication.LaunchAsync(() => new App(), new()
        {
            ConfigurationAssembly = typeof(AppFixture).Assembly,
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (Application != null) await Application.DisposeAsync();
    }
}
```

`CodeBrixPlayTestOpenGL.Register()` registers this operating system's provider; further calls do nothing, and `CodeBrixPlayTestOpenGL.IsRegistered` reports whether it has run. Nothing native is loaded until the launch. It throws `PlatformNotSupportedException` on an operating system other than Linux, Windows and macOS. With the [shared fixture base class](patterns-from-the-samples.md#a-shared-fixture-base-class), call it in `Prepare()`.

## What each operating system needs

| Operating system | How the context is made | What the machine needs |
| --- | --- | --- |
| Linux | The system Mesa EGL on its surfaceless platform - no X11, no Wayland, no window. Mesa uses the GPU's render node when there is one and the llvmpipe software rasterizer otherwise. OpenGL ES | Mesa's EGL and DRI packages: `apt install libegl1 libgl1-mesa-dri` on Debian. No GPU, display server or desktop session |
| Windows | WGL on `opengl32.dll`, on a hidden window - the Win32 head's own path. Desktop OpenGL | An interactive desktop session with a GPU OpenGL driver. On Windows on ARM, Microsoft's "OpenCL and OpenGL Compatibility Pack". Microsoft's built-in "GDI Generic" OpenGL 1.1 is too old |
| macOS | ANGLE on Metal, through the libraries the Graphics3DGL package ships - the macOS head's own path. OpenGL ES | A Mac with a Metal device. Nothing to install; no software fallback |

Vulkan is not supported. Every new context is checked before it is handed out: it must report OpenGL (ES) 3.0 or later - the Graphics3DGL elements' floor, with ANGLE exempt as in the add-in - and clearing a small framebuffer object must read back the clear color.

## Failures are loud

Nothing is ever skipped.

- **Registered, but this machine cannot create a context:** `LaunchAsync` throws a `PlayTestException` naming the provider and the concrete reason - a missing library, no driver, OpenGL older than 3.0, no Metal device. On Linux without Mesa, for example, the message says `libEGL.so.1` could not be loaded and gives the `apt install` line.
- **Not registered, and the application initializes an OpenGL element:** the test fails with a message naming the companion package and `CodeBrixPlayTestOpenGL.Register()`. The GL elements themselves catch every error and only report "initialization failed", so PlayTest records the failure against the PlayTest call during which the element loaded - typically the click or `SetContentAsync` that showed it - and every later call throws it too. An application that never initializes a GL element is unaffected.

## What the launch found out

`PlayTestApplication.OpenGL` is a `PlayTestOpenGLInfo`, fixed at launch:

| Property | Meaning |
| --- | --- |
| `IsAvailable` | Real contexts in this run |
| `Provider` | The registered provider's name |
| `Renderer` | `GL_RENDERER` of the launch's probe context |
| `Version` | `GL_VERSION` of the probe context |
| `IsGles` | OpenGL ES (Linux, macOS) or desktop OpenGL (Windows) |
| `IsSoftware` | A CPU rasterizer such as llvmpipe or SwiftShader |
| `UnavailableReason` | Why there is no OpenGL, else `null` |

```csharp
// From CodeBrix.Platform/samples/CodeBrixPlatform/PlayTestDemo/tests/PlayTestDemo.PlayTests/OpenGLTests.cs
[Fact]
public void OpenGL_reports_its_provider_renderer_and_version()
{
    var info = fixture.Application.OpenGL;
    info.IsAvailable.Should().BeTrue(info.ToString());
    info.Provider.Should().Contain("CodeBrix.Platform.PlayTest.OpenGL");
    info.Renderer.Should().NotBeNullOrEmpty();
    info.UnavailableReason.Should().BeNull();
    // Mesa EGL and ANGLE give OpenGL ES contexts; WGL gives desktop OpenGL.
    info.IsGles.Should().Be(!OperatingSystem.IsWindows());
}
```

Both branches of an application can be asserted from it: if `IsAvailable`, expect the 3D view; otherwise expect the fallback.

## Asserting GL content with PixelStats

GL pixels have no visual tree to assert on. Measure the pixels the running test captured - `Locator.ScreenshotAsync` or `Page.ScreenshotAsync` bytes - with `PixelStats`, which is in the PlayTest package:

```csharp
var canvas = PixelStats.FromPng(await Page.GetByTestId("Viewport").ScreenshotAsync(new() { Stable = true }));
```

| Member | Measures |
| --- | --- |
| `PixelStats.FromPng(byte[] png)` | The only way in: bytes captured in this run |
| `Width`, `Height`, `GetPixel(x, y)`, `Region(x, y, width, height)`, `Half(PixelHalf)` | Size, one pixel, and sub-areas to measure on their own |
| `Coverage(Color color, int tolerance = 8)` | The fraction of pixels near a color |
| `DistinctColorCount`, `IsUniform` | How many colors; one color only (all black, all magenta) |
| `IsBlank` | All transparent or black - nothing was drawn or read back |
| `Bounds(Color background, int tolerance = 8)` | A `PixelBounds` box around everything that is not background, or `null` |
| `Centroid(Color background, int tolerance = 8)` | The center of everything that is not background, or `null` |
| `MeanLuminance(Color? background = null, int tolerance = 8)` | Average brightness, optionally ignoring background - compare `Half(PixelHalf.Left)` with `Half(PixelHalf.Right)` for the lighting direction |
| `LuminanceVariance()` | Whether shading or texture is present |
| `MirrorSymmetry(PixelAxis axis, int tolerance = 8)` | 1 is a perfect mirror image |
| `Difference(PixelStats other, int tolerance = 0)` | A `PixelDifference` with `ChangedFraction` and `MaxChannelDelta`, between two captures of the same test |

Colors are unpremultiplied RGBA compared per channel, and luminance is Rec. 709 on a 0-255 scale. The `Color` type is `Windows.UI.Color`.

```csharp
// From CodeBrix.Platform/samples/CodeBrixPlatform/PlayTestDemo/tests/PlayTestDemo.PlayTests/OpenGLTests.cs
private static readonly Color GLBackground = Color.FromArgb(255, 26, 26, 26);

[Fact]
public async Task GL_canvas_draws_the_lit_square()
{
    var view = await OpenOpenGLAsync();
    (await Page.EvaluateAsync(() => view.Square.GetGLInitializationState().Status)).Should().Be(GLInitializationStatus.Initialized);
    await Expect(Page.GetByTestId("GLFallback")).ToBeHiddenAsync();

    var canvas = PixelStats.FromPng(await Page.GetByTestId("GLCanvas").ScreenshotAsync(new() { Stable = true }));
    canvas.IsBlank.Should().BeFalse(canvas.ToString());
    canvas.IsUniform.Should().BeFalse(canvas.ToString());
    // A 180-pixel square in the middle of the 360-pixel canvas: a quarter of it.
    (1 - canvas.Coverage(GLBackground)).Should().BeInRange(0.23, 0.27);
    var bounds = canvas.Bounds(GLBackground);
    bounds.Value.Width.Should().BeCloseTo(180, 3u);
    // Lit from the left: the left half is clearly brighter than the right half.
    var left = canvas.Half(PixelHalf.Left).MeanLuminance(GLBackground);
    var right = canvas.Half(PixelHalf.Right).MeanLuminance(GLBackground);
    left.Should().BeGreaterThan(right + 30, $"left {left:0.0}, right {right:0.0}");
}

[Fact]
public async Task Rotating_the_square_changes_the_GL_picture()
{
    var view = await OpenOpenGLAsync();
    var before = PixelStats.FromPng(await Page.GetByTestId("GLCanvas").ScreenshotAsync(new() { Stable = true }));
    await Button("Rotate the square").ClickAsync();
    await fixture.Application.WaitForAsync(() => view.Square.Angle, angle => angle == 30, description: "the square's new angle");
    var after = PixelStats.FromPng(await Page.GetByTestId("GLCanvas").ScreenshotAsync(new() { Stable = true }));

    var difference = before.Difference(after, tolerance: 8);
    difference.ChangedFraction.Should().BeGreaterThan(0.05, difference.ToString());
}
```

Typical checks, in order: the element initialized; the region is neither blank nor uniform; the object covers the expected share at the expected place (coverage, bounds, centroid); the lit side is brighter; an input changed the picture. A single capture after an action already shows the GL frame that action caused - the capture renders twice with a UI-thread barrier between, and the elements render inside it - and `Stable = true` additionally waits out animation.

> [!IMPORTANT]
> Use tolerances. Linux may render on llvmpipe or on a GPU, Windows on its vendor's driver, macOS on Metal. Assert the structure of the picture, never exact pixel values, and when analysis cannot decide whether something "looks right", leave the looks to the screenshots (`Path` or `--screenshotfolder`) for a person to review. `PixelStats` never compares with saved images.

## Testing the fallback

A launch with `PlayTestOptions.OpenGL = PlayTestOpenGL.Unavailable` has no OpenGL, exactly as if no provider were registered: GL elements report that initialization failed (`GLCanvasElement.GetGLInitializationState()`, `SkiaGLCanvasElement.IsGpuInitialized == false`) and the application's own fallback runs. A registered provider is then left alone. OpenGL is a launch option for the whole run - `GLCanvasElement` keeps its context for the life of the window - so test the no-OpenGL path in a process of its own: a second PlayTests project whose fixture opts out.

```csharp
// From CodeBrix.Platform/samples/CodeBrixPlatform/PlayTestDemo/tests/PlayTestDemo.NoOpenGL.PlayTests/NoOpenGLTests.cs
public async ValueTask InitializeAsync()
{
    CodeBrixPlayTestOpenGL.Register();
    Application = await PlayTestApplication.LaunchAsync(() => new App(), new()
    {
        ConfigurationAssembly = typeof(NoOpenGLFixture).Assembly,
        OpenGL = PlayTestOpenGL.Unavailable,
    });
}

[Fact]
public async Task Without_OpenGL_the_page_shows_its_fallback()
{
    var info = fixture.Application.OpenGL;
    info.IsAvailable.Should().BeFalse();
    info.Renderer.Should().BeNull();

    await Page.GetByRole(AriaRole.Button, new() { Name = "Try OpenGL", Exact = true }).ClickAsync();
    await Expect(Page.GetByTestId("GLFallback")).ToHaveTextAsync("3D preview unavailable on this machine");
    await Expect(Page.GetByTestId("SkiaGLStatus")).ToHaveTextAsync("GPU Skia unavailable");
}
```

Several of the CodeBrix.Samples suites - PolyHavenBrowser, SimpleCbxVideoPlayer, KenneyAssetBrowser - reference the companion package but launch with `PlayTestOpenGL.Unavailable`, covering the fallback path and keeping real contexts one line away: call `CodeBrixPlayTestOpenGL.Register()` and drop the opt-out.

## Pitfalls

- **Registering after `LaunchAsync`.** The launch has already decided there is no provider. Register first.
- **Referencing the companion package without the PlayTest package.** It does not bring the head.
- **Expecting the same renderer everywhere.** `Application.OpenGL.Renderer` and `IsSoftware` say what rendered; assert with tolerances.
- **Running on a Windows service account, a headless Windows runner, or a Mac without Metal.** There is no OpenGL there, and the launch says so.

## The provider seam

`IPlayTestOpenGLProvider`, `IPlayTestOpenGLContext` and `PlayTestOpenGLProviders.Register` / `IsRegistered` are public in the PlayTest package so that the companion package can plug in; tests never call them. There is one provider per process, and registering the same type again does nothing. Contexts are created and made current on the application's UI thread, and the head disposes the provider when the application is disposed.

---

**Where to go next**

- [Graphics3DGL](../add-ins/Graphics3DGL.md) - the add-in these tests exercise
- [Screenshots and recording](screenshots-and-recording.md) - capturing what PixelStats measures
- [PlayTest](README.md) - the hub page
