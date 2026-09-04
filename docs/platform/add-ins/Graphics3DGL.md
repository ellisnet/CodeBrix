<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Add-ins](../08-add-ins.md) › Graphics3DGL</sub>

# Graphics3DGL

**The OpenGL add-in: two GPU-rendered XAML elements and two helpers for off-screen GPU work.** Everything renders off-screen and is composited into the Skia scene - the two elements read the finished frame back into a `WriteableBitmap` shown as the element's background. There is no native child window, so a GL element clips, scrolls, overlaps and animates like any other XAML element, and XAML children can be layered on top of it because both elements derive from `Grid`.

| | |
| --- | --- |
| **Package** | [`CodeBrix.Platform.Graphics3DGL.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Graphics3DGL.ApacheLicenseForever) |
| **Adds** | `GLCanvasElement` (subclass and draw with raw OpenGL), `SkiaGLCanvasElement` (a GPU-backed `SKSurface` per frame), `OffscreenGLContext` and `SkiaGpuContext` (headless GPU work with no element) |
| **Heads** | All six. Each head supplies the context: Win32 and WPF through WGL, X11 through GLX, Wayland through EGL, the frame buffer through DRM/GBM or a software GL implementation, macOS through the GL libraries bundled in the package. |
| **Requires** | .NET 10 or later, and an OpenGL context of version 3.0 or later, supplied by the head. On Windows, a desktop OpenGL driver. On a GPU-less frame-buffer machine, `libegl1` and `libgl1-mesa-dri`. |

Dependencies arrive automatically: the core framework package, [`CodeBrix.Platform.OpenGL.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.OpenGL.MitLicenseForever) for the `GL` binding and its enums, and SkiaSharp for the `GRContext` and `SKSurface` members.

## Add it to your application

Reference the package once, in the `.Core` (shared UI) project - never in a head project.

```bash
dotnet add package CodeBrix.Platform.Graphics3DGL.ApacheLicenseForever
```

The `.Core` project file needs one property beyond the two package references, because the GL entry points take pointer arguments.

```xml
<PropertyGroup>
  <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="CodeBrix.Platform.ApacheLicenseForever" />
  <PackageReference Include="CodeBrix.Platform.Graphics3DGL.ApacheLicenseForever" />
</ItemGroup>
```

The usings a renderer needs:

```csharp
using CodeBrix.Platform.WinUI.Graphics3DGL;  // GLCanvasElement, SkiaGLCanvasElement,
                                             // GLInitializationState/Status,
                                             // SkiaGLPaintSurfaceEventArgs,
                                             // OffscreenGLContext, SkiaGpuContext
using CodeBrix.Platform.OpenGL;              // GL, GLEnum, ClearBufferMask, EnableCap,
                                             // BufferTargetARB, ShaderType, ...
                                             // (from the OpenGL package)
using CodeBrix.Platform.Graphics;            // SkiaGpuBackend enum (core package)
using SkiaSharp;                             // SKSurface, GRContext, SKImageInfo
using Microsoft.UI.Xaml;                     // Window, XamlRoot
```

`GLCanvasElement` is abstract, so the XAML namespace is your subclass's own:

```xml
xmlns:render="using:MyApp.Rendering"
<render:SpinningTriangle x:Name="Triangle" />
```

No head project changes are needed: each head registers its native OpenGL wrapper when its host starts.

## Using it

### GLCanvasElement: raw OpenGL

You subclass `GLCanvasElement` and implement three methods. The element owns the framebuffer, the read-back and the presentation; you own the GL resources and the draw calls.

```csharp
namespace CodeBrix.Platform.WinUI.Graphics3DGL;

public abstract partial class GLCanvasElement : Grid, INativeContext
{
    protected GLCanvasElement(Func<Window>? getWindowFunc);
    protected abstract void Init(GL gl);
    protected abstract void OnDestroy(GL gl);
    protected abstract void RenderOverride(GL gl);
    public void Invalidate();
    public static DependencyProperty IsGLInitializedProperty { get; }
    public bool? IsGLInitialized { get; }          // read-only
    public GLInitializationState GetGLInitializationState();
}
```

Pass `null` for `getWindowFunc`. `Init(GL gl)` creates your shaders, vertex arrays, buffers and textures with the context current, after the element's offscreen framebuffer exists; it can run more than once, and every call after the first is preceded by an `OnDestroy` call. `RenderOverride(GL gl)` draws one frame with the framebuffer bound and the viewport set to the element's `RenderSize`. `Invalidate()` queues exactly one call to `RenderOverride`, and the result is kept until the next `Invalidate`; call it at the end of `RenderOverride` for a continuous animation.

> [!IMPORTANT]
> The GL context is shared with the head's own Skia renderer. Restore every GL state you change before returning from `RenderOverride` - unbind your vertex array and program, disable the depth test if you enabled it - or the whole window can render incorrectly.

This spinning triangle is a complete subclass: shaders as raw strings, resource creation in `Init`, an animated frame in `RenderOverride` that re-invalidates itself, and cleanup in `OnDestroy`.

```csharp
using System;
using System.Diagnostics;
using System.Numerics;
using CodeBrix.Platform.OpenGL;
using CodeBrix.Platform.WinUI.Graphics3DGL;

namespace MyApp.Rendering;

public sealed class SpinningTriangle : GLCanvasElement
{
    // "#version 300 es" is the GLES 3.0 dialect the repository's sample
    // uses. If a desktop-GL driver rejects it, use "#version 330 core"
    // instead; a rejected shader is reported through FailedReason.
    const string VertexShaderSource = """
        #version 300 es
        precision highp float;
        layout (location = 0) in vec2 aPosition;
        layout (location = 1) in vec3 aColor;
        uniform mat4 uTransform;
        out vec3 vColor;
        void main()
        {
            gl_Position = uTransform * vec4(aPosition, 0.0, 1.0);
            vColor = aColor;
        }
        """;

    const string FragmentShaderSource = """
        #version 300 es
        precision highp float;
        in vec3 vColor;
        out vec4 fragColor;
        void main() { fragColor = vec4(vColor, 1.0); }
        """;

    uint program, vertexArray, vertexBuffer, indexBuffer;
    int transformLocation;
    readonly Stopwatch clock = Stopwatch.StartNew();

    public SpinningTriangle() : base(null) { }

    protected override unsafe void Init(GL gl)
    {
        program = BuildProgram(gl);
        transformLocation = gl.GetUniformLocation(program, "uTransform");

        float[] vertices =
        {   //   x      y      r  g  b
            -0.6f, -0.5f,  1, 0, 0,
             0.6f, -0.5f,  0, 1, 0,
             0.0f,  0.7f,  0, 0, 1,
        };
        uint[] indices = { 0, 1, 2 };

        vertexArray = gl.GenVertexArray();
        gl.BindVertexArray(vertexArray);

        vertexBuffer = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertexBuffer);
        gl.BufferData<float>(BufferTargetARB.ArrayBuffer, vertices, BufferUsageARB.StaticDraw);

        const uint stride = 5 * sizeof(float);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, (void*) 0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride,
            (void*) (2 * sizeof(float)));

        indexBuffer = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, indexBuffer);
        gl.BufferData<uint>(BufferTargetARB.ElementArrayBuffer, indices, BufferUsageARB.StaticDraw);

        gl.BindVertexArray(0);
    }

    protected override unsafe void RenderOverride(GL gl)
    {
        gl.ClearColor(0.09f, 0.10f, 0.13f, 1f);
        gl.Clear((uint) ClearBufferMask.ColorBufferBit);

        var angle = (float) (clock.Elapsed.TotalSeconds * 1.5);
        var width = (float) Math.Max(1, RenderSize.Width);
        var height = (float) Math.Max(1, RenderSize.Height);
        // Keep the triangle's aspect ratio regardless of the element's shape.
        var transform = Matrix4x4.CreateRotationZ(angle)
                      * Matrix4x4.CreateScale(height / width, 1f, 1f);

        gl.UseProgram(program);
        gl.UniformMatrix4(transformLocation, 1, false, (float*) &transform);
        gl.BindVertexArray(vertexArray);
        gl.DrawElements(PrimitiveType.Triangles, 3, DrawElementsType.UnsignedInt, (void*) 0);

        // Restore shared state before returning.
        gl.BindVertexArray(0);
        gl.UseProgram(0);

        Invalidate();   // request the next frame -> continuous animation
    }

    protected override void OnDestroy(GL gl)
    {
        gl.DeleteVertexArray(vertexArray);
        gl.DeleteBuffer(vertexBuffer);
        gl.DeleteBuffer(indexBuffer);
        if (program != 0)
            gl.DeleteProgram(program);
        program = 0;
    }

    static uint BuildProgram(GL gl)
    {
        var vertex = Compile(gl, ShaderType.VertexShader, VertexShaderSource);
        var fragment = Compile(gl, ShaderType.FragmentShader, FragmentShaderSource);

        var handle = gl.CreateProgram();
        gl.AttachShader(handle, vertex);
        gl.AttachShader(handle, fragment);
        gl.LinkProgram(handle);
        gl.GetProgram(handle, ProgramPropertyARB.LinkStatus, out var linked);
        if (linked == 0)
            throw new InvalidOperationException($"Shader link failed: {gl.GetProgramInfoLog(handle)}");

        gl.DetachShader(handle, vertex);
        gl.DetachShader(handle, fragment);
        gl.DeleteShader(vertex);
        gl.DeleteShader(fragment);
        return handle;
    }

    static uint Compile(GL gl, ShaderType type, string source)
    {
        var shader = gl.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);
        gl.GetShader(shader, ShaderParameterName.CompileStatus, out var compiled);
        if (compiled == 0)
            throw new InvalidOperationException($"{type} compile failed: {gl.GetShaderInfoLog(shader)}");
        return shader;
    }
}
```

Notice that the compile and link helpers throw with the driver's own info log. A throwing `Init` or `RenderOverride` is caught by the element and turned into a failure state you can read - a silent blank surface is much harder to diagnose than a reported message.

Hosting it is one line, and because the element is a `Grid`, XAML sits on top of the GL content.

```xml
<Page x:Class="MyApp.Views.MainPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:render="using:MyApp.Rendering">
    <Grid>
        <render:SpinningTriangle x:Name="Triangle" />
        <!-- XAML on top of the GL content: the element is a Grid -->
        <TextBlock Text="OpenGL" Margin="8" VerticalAlignment="Top"
                   IsHitTestVisible="False" />
        <TextBlock x:Name="Status" Foreground="OrangeRed" Margin="8"
                   VerticalAlignment="Bottom" TextWrapping="Wrap" />
    </Grid>
</Page>
```

### The element lifecycle

On `Loaded`, the head's native OpenGL context is created - once per `XamlRoot` (window), shared by every `GLCanvasElement` in that window, and destroyed when the window closes. The GL version is checked, an offscreen framebuffer is built for the element's `RenderSize` (a color texture plus a `Depth24Stencil8` renderbuffer), and `Init(gl)` runs.

Each `Invalidate()` binds the framebuffer, sets the viewport, calls `RenderOverride(gl)`, then reads the color attachment back into a `WriteableBitmap` and presents it as the `Grid`'s background through an `ImageBrush` with `ScaleY = -1`, because OpenGL's origin is bottom-left. **Do not flip again in your projection.** A `SizeChanged` rebuilds the framebuffer and the back buffer and invalidates; a zero-sized element is skipped until it has a real size. On `Unloaded`, `OnDestroy(gl)` runs with the context current.

The framebuffer, the viewport and the back buffer are all `RenderSize` - the element's layout size in device-independent pixels, the same units as `ActualWidth` and `ActualHeight`, not physical pixels. On a scaled display the compositor upscales the result.

### Reporting failure to the user

If the context cannot be created, the GL version is too low, or your own `Init` or `RenderOverride` throws, the element records `InitializationFailed` with a reason, sets `IsGLInitialized` to false, logs the error and stops calling `RenderOverride` until the element is unloaded and reloaded. The surface stays blank and no exception escapes to the application - so surface the reason yourself.

```csharp
public enum GLInitializationStatus
{
    NotYetInitialized = 0,   // not loaded yet, or unloaded again
    Initializing = 1,        // rarely observed: init is synchronous in Loaded
    Initialized = 2,         // == IsGLInitialized true
    InitializationFailed = 3 // == IsGLInitialized false; see FailedReason
}

public sealed class GLInitializationState
{
    public GLInitializationStatus Status { get; }
    public string? FailedReason { get; }   // non-null if and only if
                                           // Status == InitializationFailed
}
```

Query the state in `Loaded` or later, never in the constructor, where it is still `NotYetInitialized`. This handler is adapted from the add-in guide's example, with its store link replaced by the name of the item to install.

```csharp
// MainPage.xaml.cs
Triangle.Loaded += (_, _) =>
{
    var state = Triangle.GetGLInitializationState();
    if (state.Status == GLInitializationStatus.InitializationFailed)
    {
        var msg = "3D rendering is unavailable.\n\n" + state.FailedReason;
        if (OperatingSystem.IsWindows())
        {
            msg += "\n\nOn Windows you may need Microsoft's free "
                + "\"OpenCL and OpenGL Compatibility Pack\".";
        }
        Status.Text = msg;   // or a dialog
    }
};
```

### SkiaGLCanvasElement: GPU Skia in a page

When the drawing is 2D but heavy - shaders, large filters, very large scenes - you get the GPU with SkiaSharp's own API and a single read-back copy, with no shader code of your own.

```csharp
public partial class SkiaGLCanvasElement : Grid
{
    public SkiaGLCanvasElement(Func<Window>? getWindowFunc = null);
    public event EventHandler<SkiaGLPaintSurfaceEventArgs>? PaintSurface;
    public bool? IsGpuInitialized { get; }        // null until loaded
    protected virtual void OnPaintSurface(SkiaGLPaintSurfaceEventArgs args);
    public void Invalidate();
}
```

On `Loaded` it creates an `OffscreenGLContext` and a `GRContext` on it. Each `Invalidate()` makes the context current, raises `PaintSurface` with a GPU `SKSurface` sized to `RenderSize`, flushes, reads the pixels back in one copy and presents them. There is no vertical flip here, because a GPU `SKSurface` has a top-left origin. Failure is reported only as `IsGpuInitialized == false`; this element carries no failure reason.

```csharp
using CodeBrix.Platform.WinUI.Graphics3DGL;
using SkiaSharp;

// code-behind: create it and add it to a Grid named Host
var gpuCanvas = new SkiaGLCanvasElement();
var paint = new SKPaint { Color = SKColors.DeepSkyBlue, IsAntialias = true };
var sw = System.Diagnostics.Stopwatch.StartNew();

gpuCanvas.PaintSurface += (s, e) =>
{
    var canvas = e.Surface.Canvas;          // GPU-backed; GL context is current
    canvas.Clear(SKColors.Black);
    var t = (float) sw.Elapsed.TotalSeconds;
    var cx = e.Info.Width / 2f + MathF.Cos(t) * e.Info.Width / 4f;
    var cy = e.Info.Height / 2f + MathF.Sin(t) * e.Info.Height / 4f;
    canvas.DrawCircle(cx, cy, 40, paint);
    gpuCanvas.Invalidate();                  // animate
};
gpuCanvas.Loaded += (_, _) =>
{
    if (gpuCanvas.IsGpuInitialized == false)
        Status.Text = "GPU Skia is unavailable on this device.";
};
Host.Children.Add(gpuCanvas);
```

The event args carry everything the handler needs:

```csharp
public sealed class SkiaGLPaintSurfaceEventArgs : EventArgs
{
    public SkiaGLPaintSurfaceEventArgs(SKSurface surface, GRContext context, SKImageInfo info);
    public SKSurface Surface { get; }     // draw on Surface.Canvas; context is current
    public GRContext Context { get; }
    public SKImageInfo Info { get; }      // width, height and colour type of Surface
}
```

### Off-screen GPU work with no element

For thumbnails, batch rasterization or anything that never needs to be on screen, skip the element entirely. `SkiaGpuContext` is the backend-neutral way to get GPU Skia: on macOS it resolves the head's Skia-on-Metal provider, and on every other head it wraps an `OffscreenGLContext`.

```csharp
public sealed class SkiaGpuContext : IDisposable
{
    public static bool TryCreate(XamlRoot xamlRoot,
                                 [NotNullWhen(true)] out SkiaGpuContext? context);
    public GRContext GrContext { get; }
    public SkiaGpuBackend Backend { get; }   // CodeBrix.Platform.Graphics enum:
                                             // OpenGL, Metal (diagnostic only)
    public IDisposable BeginFrame();
    public void Dispose();
}
```

`TryCreate` returning false means "keep your processor-side fallback" - which is exactly how this thumbnail renderer is written.

```csharp
using CodeBrix.Platform.WinUI.Graphics3DGL;
using SkiaSharp;

// Call on the UI thread once the page is loaded (needs a XamlRoot).
SKImage RenderThumbnail(int width, int height)
{
    var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

    if (SkiaGpuContext.TryCreate(XamlRoot, out var gpu))
    {
        using (gpu)
        using (gpu.BeginFrame())                       // GL current / Metal no-op
        using (var surface = SKSurface.Create(gpu.GrContext, true, info))
        {
            Draw(surface.Canvas);
            surface.Flush();
            return surface.Snapshot().ToRasterImage(); // copy off the GPU before Dispose
        }
    }

    using (var surface = SKSurface.Create(info))        // CPU fallback
    {
        Draw(surface.Canvas);
        return surface.Snapshot();
    }
}
```

Wrap each frame's GPU work in a single `using` of `BeginFrame()`, which makes the GL context current and is a no-op on Metal. For raw OpenGL with no XAML element at all, use `OffscreenGLContext` directly.

```csharp
public sealed class OffscreenGLContext : IDisposable
{
    public static bool TryCreate(XamlRoot xamlRoot,
                                 [NotNullWhen(true)] out OffscreenGLContext? context);
    public GL Gl { get; }
    public IDisposable MakeCurrent();
    public IntPtr GetProcAddress(string name);   // IntPtr.Zero when absent; never throws
    public GRContext CreateGrContext();          // throws InvalidOperationException when
                                                 // neither GL flavour works
    public void Dispose();
}
```

```csharp
if (OffscreenGLContext.TryCreate(XamlRoot, out var ctx))
{
    using (ctx)
    {
        using (ctx.MakeCurrent())
        {
            var gl = ctx.Gl;
            // create your own FBO, render, gl.ReadPixels(...) into your buffer
        }
    }
}
// Or build a GRContext on it yourself: var gr = ctx.CreateGrContext();
// (dispose gr inside a MakeCurrent scope, before ctx.Dispose()).
```

Only touch `Gl`, or a `GRContext` built on it, inside a `using` of `MakeCurrent()`. That call saves and restores whatever context was current, so it never disturbs the head's renderer even on the same thread. Keep the context and its `GRContext` on the thread that created them, and dispose the `GRContext` inside a `MakeCurrent` scope *before* disposing the `OffscreenGLContext`.

### What it does not do

There is no native child window and no on-screen swap chain: everything is off-screen plus read-back, with no zero-copy present path. The package hands your code no Vulkan, Direct3D or Metal API - `SkiaGpuContext` uses Metal internally on macOS but gives you only a `GRContext`. It loads no models, textures or images and has no scene graph, camera or math helpers; bring your own, such as `System.Numerics`. It manages no render loop and no vsync - you drive frames with `Invalidate()`. It does not render at physical-pixel resolution on scaled displays. And it is not the processor-side drawing element: for plain 2D Skia use [Graphics2DSK](Graphics2DSK.md) or [SkiaSharp views](SkiaSharpViews.md).

## Per-head notes

Each head supplies the OpenGL context in its own way.

| Head | How the context is provided |
| --- | --- |
| Windows Win32, Windows WPF | WGL - needs a desktop OpenGL driver installed on the device |
| Linux X11 | GLX |
| Linux Wayland | EGL, working under the head's default Vulkan presenter |
| Linux frame buffer | DRM/GBM, or a software GL implementation on GPU-less systems |
| macOS | The GL libraries bundled in the package; nothing to install |

On Windows the WGL context needs a real OpenGL driver. Most x64 machines have one from their GPU vendor, but many Windows-on-ARM devices do not ship a desktop OpenGL driver; there, OpenGL comes from Microsoft's free "OpenCL and OpenGL Compatibility Pack", installed once per device from the Microsoft Store. Without it the context cannot be created and the surface stays blank, while the rest of the application is unaffected because the head renders 2D Skia on the processor. This is a device-level prerequisite the end user installs; an application cannot supply it. Detect it with `GetGLInitializationState()` and tell the user.

On a GPU-less frame-buffer machine, install the software GL stack:

```bash
apt install libegl1 libgl1-mesa-dri
```

`CreateGrContext()` tries the GL flavor the running head implies first - desktop GL on X11, Win32 and WPF; GLES on Wayland, macOS and the frame buffer - and falls back to the other, so it also works on an unrecognized host. On macOS, `SkiaGpuContext.TryCreate` resolves the head's Skia-on-Metal provider and deliberately does *not* fall through to OpenGL when that head is in software-rendering mode; it returns false instead.

## Pitfalls

- **Querying `GetGLInitializationState()` or `IsGLInitialized` in the constructor**, or before `Loaded`. The status is `NotYetInitialized` or null there.
- **Not restoring GL state** - bound vertex array, program, depth test, blend, viewport - at the end of `RenderOverride`. The head's own Skia rendering shares the context and will glitch.
- **Expecting a top-left origin from `GLCanvasElement`.** OpenGL renders bottom-up and the element flips the presented image for you. Do not flip again in your projection.
- **Letting exceptions "escape".** A throwing `Init` or `RenderOverride` becomes `InitializationFailed`, and the element then stays blank and silent until reloaded. Surface `FailedReason` to the user.
- **Zero-size element.** With no `Height`, `MinHeight` or star row, nothing is rendered.
- **Windows-on-ARM without an OpenGL driver.** Blank surface; detect it and point the user at the Compatibility Pack.
- **Frame-buffer head on a GPU-less machine** without `libegl1` and `libgl1-mesa-dri`: context creation fails.
- **Mixing up pixel units.** The GL viewport and the `SkiaGLCanvasElement` surface are `RenderSize` - the same units as `ActualWidth` and `ActualHeight` - not physical pixels.
- **Using `OffscreenGLContext.Gl` or a `GRContext` outside a `MakeCurrent` or `BeginFrame` scope**, or from another thread: undefined results. Dispose the `GRContext` before the context.
- **Giving `GLCanvasElement` a non-null `getWindowFunc`.** It is ignored here; `null` is the documented value.
- **Adding a second GL binding.** The `GL` type in these signatures is `CodeBrix.Platform.OpenGL.GL`, from the OpenGL package that arrives automatically.
- **Animating a large element, or several at once.** Every frame ends with a GPU-to-processor read-back of `RenderSize` pixels plus a bitmap present, and the cost scales with element area. Keep GL elements as small as the design allows.
- **Creating GL resources in `RenderOverride`.** Do it in `Init`, which runs again only after an unload and reload.
- **Continuously animating the element's size.** Each resize rebuilds the framebuffer and the back buffer.

## Related

- [CodeBrix.Platform.OpenGL](../../libraries/CodeBrix.Platform.OpenGL.md) - the managed OpenGL binding this add-in draws through, usable on its own in any .NET 10 application
- [Graphics2DSK](Graphics2DSK.md) - the zero-copy processor-side element, for drawing that does not need the GPU
- [SkiaSharp views](SkiaSharpViews.md) - `SKXamlCanvas`, whose `SKSwapChainPanel` placeholder `SkiaGLCanvasElement` stands in for
- [PolyHavenBrowser](https://github.com/ellisnet/CodeBrix.Samples/tree/main/PolyHavenBrowser) in [CodeBrix.Samples](../../samples/README.md) - a glTF preview in a GL canvas, automatic camera framing, a second translucent pass, and off-screen product shots
- [PolyHavenBrowser_viewer_only](https://github.com/ellisnet/CodeBrix.Samples/tree/main/PolyHavenBrowser_viewer_only) in [CodeBrix.Samples](../../samples/README.md) - three interchangeable GPU backends behind one interface, composited onto a Skia canvas
- [KenneyAssetBrowser](https://github.com/ellisnet/CodeBrix.Samples/tree/main/KenneyAssetBrowser) in [CodeBrix.Samples](../../samples/README.md) - a 3D model preview alongside image, sprite-sheet and animation viewers
- [EmulateFrameBufferDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/EmulateFrameBufferDemo) in CodeBrix.Platform - `src/EmulateFrameBufferDemo.Core/Rendering/ModelViewerCanvas.cs` is a complete `GLCanvasElement` subclass: glTF loading, shader compile and link with error reporting, buffer and texture upload in `Init`, a turntable loop, and cleanup in `OnDestroy`

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.WinUI.Graphics3DGL/AGENT-README.txt) |
| The core framework and the six heads | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/AGENT-README.txt) |
| Map of every README in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/README-INDEX.txt) |
| Samples and tools | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/EXTRAS-README.txt) |
| Package | [`CodeBrix.Platform.Graphics3DGL.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Graphics3DGL.ApacheLicenseForever) |

The package is Apache-2.0. The GL libraries it bundles for macOS are BSD-3-Clause, and the package carries its own THIRD-PARTY-NOTICES.txt for them.

---

**Where to go next**

- [Lottie](Lottie.md) - the next add-in: vector animation playback
- [Graphics, media and vision](../09-graphics-media-and-vision.md) - the chapter that puts the drawing add-ins to work
- [All add-ins](../08-add-ins.md) - the whole set at a glance
