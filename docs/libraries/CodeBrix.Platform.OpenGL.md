<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Platform.OpenGL</sub>

# CodeBrix.Platform.OpenGL

**CodeBrix.Platform.OpenGL is a fully managed, cross-platform OpenGL Core-profile binding for .NET: it
exposes the OpenGL API to managed code through function-pointer dispatch, and brings the native-library
resolution and the vector, matrix and scalar math types the OpenGL signatures need.** Every call
dispatches through an unmanaged function pointer resolved lazily on first use, from the GL context you
provide. You bring a window and a current context from a windowing layer; this package never creates
either, and it works the same way from any .NET 10 application.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Platform.OpenGL](https://github.com/ellisnet/CodeBrix.Platform.OpenGL) |
| **Packages** | [`CodeBrix.Platform.OpenGL.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.OpenGL.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later, an OpenGL implementation on the machine (`libGL.so.1` on Linux, the OpenGL framework on macOS, `opengl32.dll` plus the vendor driver on Windows), and a current GL context |
| **Use it from** | Any .NET 10 application |
| **Platforms** | Windows, macOS and Linux; the calling convention is chosen at run time - Stdcall on Windows, Cdecl elsewhere |

## What it does

- Exposes the complete OpenGL core-profile surface as methods on a single class, `GL`, plus Span, array
  and `out` convenience overloads on the `GLOverloads` static extension class, which is in scope from
  the same `using`.
- Dispatches through function-pointer P/Invoke (`delegate* unmanaged[Stdcall|Cdecl]<...>`), so a call
  with blittable arguments allocates nothing and marshals nothing.
- Selects the calling convention at run time, and resolves the native library across platforms, so the
  same code loads `opengl32`, `libGL` or the platform equivalent.
- Runs no source generator in your build: the P/Invoke method bodies ship as ordinary compiled code
  inside the package, from ordinary committed C# source.
- Carries a native loading and marshalling layer - native-library loading, string and pointer
  marshalling, GL-context abstractions and native window handles.
- Carries a generic math layer: vector, matrix, quaternion and shape structs over any element type
  (`Vector3D<float>`, `Matrix4X4<float>` and the rest), plus converters to and from `System.Numerics`.
- Supports `System.Half` natively.
- Looks extensions up with `IsExtensionPresent(string extension)`, which enumerates once and caches (the
  `GL_` prefix is optional), and `TryGetExtension<T>(out T ext)`.

## When to use it

Reach for CodeBrix.Platform.OpenGL when your application already owns a window and a current OpenGL
context - from SDL, GLFW, a platform windowing layer, EGL or anything else - and you want to issue GL
calls from C# with no per-call overhead. It is a raw binding: the product is the entry points, the
enums, the handle structs and the math types, and nothing above them.

What you must bring is a window and a current OpenGL context created by some other library. This package
never creates windows or contexts, swaps buffers, handles input or runs a message loop.
`IGLContext`, `IGLContextSource` and `INativeWindow` are interfaces for a windowing layer to *implement*;
no implementation ships here.

What the package deliberately does not do beyond that:

- It ships no extension bindings. `TryGetExtension<T>` only finds classes you write yourself.
- No OpenGL ES, no legacy or compatibility-profile binding, and no WGL, GLX or EGL bindings - the core
  profile only.
- No image decoding, font rendering, scene graph or shader tooling.
- No SIMD-accelerated math: the generic math types dispatch through the base class library, and
  `Scalar.IsHardwareAccelerated` reports that state.
- It is not usable on a headless host for real GL calls: there is no software rasterizer and no
  off-screen context provider inside the package.

A CodeBrix.Platform application that wants GPU-accelerated drawing does not reach for this binding
directly - the [Graphics3DGL add-in](../platform/add-ins/Graphics3DGL.md) carries that path.

## Getting started

```bash
dotnet add package CodeBrix.Platform.OpenGL.MitLicenseForever
```

Your project will want unsafe blocks enabled:

```xml
<AllowUnsafeBlocks>true</AllowUnsafeBlocks>
```

Pointer-taking overloads (`void*`, `byte*`, `float*`) and the `DrawElements` "offset into a bound
element buffer" idiom require an unsafe context. The `out`, `ref readonly`, `Span<T>` and `string`
overloads do not, so a purely safe consumer is possible, but awkward for index-buffer drawing.

There are exactly seven namespaces. "Enums" and "Structs" are source folders only: every OpenGL enum and
every handle struct lives directly in `CodeBrix.Platform.OpenGL`.

```csharp
using CodeBrix.Platform.OpenGL;                 // GL, GLOverloads, all
                                                // enums, the GL
                                                // handle structs,
                                                // DebugProc, PortStatus,
                                                // ContextSourceExtensions
using CodeBrix.Platform.OpenGL.Core;            // Bool32, Bool8, Version32,
                                                // Version64, RawImage,
                                                // PfnVoidFunction,
                                                // PlatformException,
                                                // BreakneckLock
using CodeBrix.Platform.OpenGL.Core.Contexts;   // INativeContext,
                                                // IGLContext,
                                                // IGLContextSource,
                                                // LamdaNativeContext,
                                                // DefaultNativeContext,
                                                // MultiNativeContext,
                                                // INativeWindow, ...
using CodeBrix.Platform.OpenGL.Core.Loader;     // UnmanagedLibrary,
                                                // LibraryLoader,
                                                // PathResolver,
                                                // DefaultPathResolver,
                                                // SearchPathContainer,
                                                // SymbolLoadingException
using CodeBrix.Platform.OpenGL.Core.Native;     // SilkMarshal,
                                                // GlobalMemory, ComPtr<T>,
                                                // NativeAPI,
                                                // NativeExtension<T>,
                                                // NativeStringEncoding
using CodeBrix.Platform.OpenGL.Core.Attributes; // NativeName, Count, Flow,
                                                // Extension, Inject,
                                                // UnmanagedType attributes
using CodeBrix.Platform.OpenGL.Maths;           // Vector2D/3D/4D<T>,
                                                // Matrix4X4<T>, ...
```

There is no registration call and no feature flag. The only setup is obtaining a `GL` instance from a
context:

```csharp
using CodeBrix.Platform.OpenGL;
using CodeBrix.Platform.OpenGL.Core.Contexts;

IGLContext context = /* obtained from GLFW / SDL / your windowing layer */;
GL gl = GL.GetApi(context);
gl.ClearColor(0.2f, 0.3f, 0.4f, 1.0f);
gl.Clear(ClearBufferMask.ColorBufferBit);
```

`GL.GetApi` also accepts an `IGLContextSource`, an `INativeContext`, or a plain `Func<string, nint>`
address loader, so it fits whatever windowing layer supplies the context. Keep one `GL` instance per
context for the life of that context.

## Key concepts

### Creating a `GL` instance

`GL` is `public unsafe partial class GL : NativeAPI`, and the factories are:

- `public GL(INativeContext ctx)`
- `GetApi(IGLContextSource contextSource)` - throws `InvalidOperationException` if `.GLContext` is null
- `GetApi(IGLContext ctx)`
- `GetApi(Func<string, nint> getProcAddress)` - wraps the delegate in a `LamdaNativeContext`
- `GetApi(INativeContext ctx)`
- `CreateDefaultContext(string[] names)` - tries each library name, and throws
  `System.IO.FileNotFoundException` if none load
- the `ContextSourceExtensions` extension method `CreateOpenGL(this IGLContextSource src)`

On the instance you get `Context`, `Dispose()` (which disposes the `INativeContext` and the
function-pointer table), `PurgeEntryPoints()` (forget every resolved pointer; they are re-resolved on
the next call, which is what you use after switching to a different context), `CurrentVTable`,
`TryGetExtension<T>(out T ext)` and `IsExtensionPresent(string extension)`.

### The context abstraction

```csharp
public interface INativeContext : IDisposable
{
    nint GetProcAddress(string proc, int? slot = default);
    bool TryGetProcAddress(string proc, out nint addr, int? slot = default);
}
```

`IGLContext : INativeContext` adds `Handle`, `Source`, `IsCurrent`, `SwapInterval(int)`,
`SwapBuffers()`, `MakeCurrent()` and `Clear()`, and `IGLContextSource` is
`{ IGLContext? GLContext { get; } }`. Three implementations ship:

- `LamdaNativeContext` wraps a `Func<string, nint>` or a `TryLoader` delegate. Its `GetProcAddress`
  throws `SymbolLoadingException` when the delegate returns 0; `TryGetProcAddress` returns false
  instead.
- `DefaultNativeContext` opens a native library itself, through `TryCreate(string name, out
  DefaultNativeContext context)` or one of its constructors, and exposes `UnmanagedLibrary Library`.
- `MultiNativeContext` holds `INativeContext?[] Contexts` and asks each in order - first non-zero wins.

A proc-address function from a windowing library (`SDL_GL_GetProcAddress`, `glfwGetProcAddress`,
`glXGetProcAddress`, `eglGetProcAddress`, `wglGetProcAddress`) is the recommended route, because the
windowing layer already knows the driver and the current context.

> [!IMPORTANT]
> `DefaultNativeContext` and `CreateDefaultContext` resolve entry points **only** by exported-symbol
> lookup on the library handle; they never call the platform's own proc-address function. On Windows,
> `opengl32.dll` exports only OpenGL 1.1, so `CreateShader`, `GenBuffers` and the rest throw
> `SymbolLoadingException` through that route. Combine the two with a `MultiNativeContext`.

The library names are the consumer's to pass; the library does not choose for you: `"libGL.so.1"` on
Linux, `"/System/Library/Frameworks/OpenGL.framework/OpenGL"` on macOS, `"opengl32.dll"` on Windows.
`DefaultPathResolver` then tries the name itself, versioned-soname variants on Linux and macOS, the
application base directory, the main module directory, `runtimes/<rid>/native`, entries from the
application's `.deps.json`, and the directory beside the binding assembly. The first candidate that
loads wins.

### Entry-point resolution, and why errors surface late

Every GL method reads a cached `nint` for its `glXxx` symbol. On first use it calls
`INativeContext.GetProcAddress("glXxx")` and caches the result for the life of the `GL` instance. A
missing symbol therefore surfaces as `SymbolLoadingException` at the **first call of that method**, not
at construction - a `GL.GetApi` that "worked" says nothing about which functions exist. Check
`GetStringS(StringName.Version)` early.

### Reading the overload families

Every OpenGL function appears as a family of overloads on `GL` (instance methods) plus, for Span-taking
forms, static extension methods in `GLOverloads`, in scope from the same `using`.

| Shape | What you get |
| --- | --- |
| Typed enum versus `GLEnum` | Each enum parameter has a twin overload taking the catch-all `GLEnum`. Prefer the typed enum; use `GLEnum` for values the typed enum lacks. Mask parameters additionally accept a raw `uint` |
| A native `const T*` | `T*` (unsafe), `ref readonly T` (call with `in x`), and - where the count is inferable - `ReadOnlySpan<T>`, which drops the count parameter |
| A native `T*` output | `T*`, `out T` and `Span<T>` |
| A native `void*` | Generic `T0 : unmanaged` versions: `BufferData<T0>`, `TexImage2D<T0>`, `DrawElements<T0>`, `ReadPixels<T0>` |
| Gen and Delete | `GenBuffer()` returns one; `GenBuffers(uint n)` returns the **first** of n; `GenBuffers(Span<uint>)` fills a span; `DeleteBuffer(uint)`; `DeleteBuffers(ReadOnlySpan<uint>)`. The same shape exists for textures, vertex arrays, framebuffers, renderbuffers, samplers, queries, program pipelines and transform feedbacks |
| Handle structs | Wherever a `uint` handle array is accepted, an overload with the matching struct exists: `GenBuffers(Span<Buffer>)`, `DeleteTextures(ReadOnlySpan<Texture>)` |
| C strings | Parameters accept `string`, and several "get a string back" calls have string-returning conveniences: `GetStringS`, `GetShaderInfoLog(uint) : string` |

The compile and link status idiom is the same for both: `GetShader(s, ShaderParameterName.CompileStatus)`
returns 0 on failure, and `GetProgram(p, ProgramPropertyARB.LinkStatus)` likewise.

### The enums

Every enum lives in `CodeBrix.Platform.OpenGL`, is `: int` with the GL hex values, tags each member with
its native name, and carries `[Flags]` on the mask enums. The `GL_` prefix is dropped and the remainder
is PascalCased: `GL_ARRAY_BUFFER` becomes `ArrayBuffer`, `GL_COLOR_BUFFER_BIT` becomes `ColorBufferBit`.
Enums from the OpenGL registry keep their registry names, including `ARB` and `EXT` suffixes, even
though they are core: `BufferTargetARB`, `BufferUsageARB`, `ProgramPropertyARB`,
`BlendEquationModeEXT`. There is no `BufferTarget` and no `BufferUsage`.

`GLEnum` is the union enum covering every constant. Every typed-enum parameter has a `GLEnum` overload,
and `GetError` and `CheckFramebufferStatus` return `GLEnum` - compare with `GLEnum.NoError` and
`GLEnum.FramebufferComplete`, or cast.

### The handle structs

Each is shaped like `public unsafe partial struct Buffer { public Buffer(uint? handle = null); public
uint Handle; }`, and the set is `Buffer`, `DisplayList`, `Framebuffer`, `PerfQueryHandle`, `PerfQueryId`,
`Program`, `ProgramPipeline`, `Query`, `Renderbuffer`, `Sampler`, `Shader`, `Sync`, `Texture`,
`TransformFeedback` and `VertexArray`. They exist for the `Span<Buffer>` and `ReadOnlySpan<Texture>`
overload variants and for type-safe storage in your own code. There are **no** implicit conversions to
`uint`, so pass `.Handle` to the plain-`uint` entry points; most calls take `uint` directly and the
structs are optional.

### Drawing with an element buffer

`DrawElements` is the one place where the span overloads and the pointer overload mean genuinely
different things.

> [!WARNING]
> With an element buffer bound, the `indices` argument is a byte **offset**, so pass `(void*)0` (or
> `(void*)byteOffset`) in an unsafe block. Do not use the `ref readonly` or `ReadOnlySpan<T0>` forms for
> that case - they pass a managed address, which is only correct when no element buffer is bound.

`VertexAttribPointer` has the same offset semantics, but its `nint` overload needs no unsafe context,
because the pointer is only ever an offset there.

### Debug output

```csharp
[UnmanagedFunctionPointer(CallingConvention.Winapi)]
public delegate void DebugProc(GLEnum source, GLEnum type, int id, GLEnum severity,
                               int length, nint message, nint userParam);
```

with `DebugMessageCallback(DebugProc, void*)`, `DebugMessageCallback<T0>(DebugProc, ref readonly T0)`
and `DebugMessageControl(...)`. Read the message with
`SilkMarshal.PtrToString(message, NativeStringEncoding.UTF8)`, and enable it with
`Enable(EnableCap.DebugOutput)` plus, for callbacks on the calling thread,
`Enable(EnableCap.DebugOutputSynchronous)`. The enum member names carry their prefix -
`DebugSeverity.DebugSeverityHigh`, `DebugType.DebugTypeError`, `DebugSource.DebugSourceApi` - and each
has a `DontCare` member.

> [!WARNING]
> The binding keeps the delegate alive only until the **next** GL call, but the driver keeps calling it
> for the life of the context. Hold a strong reference to the `DebugProc` yourself - a static field is
> the usual choice - or the garbage collector collects it and the next debug message crashes the
> process.

### The math layer

`CodeBrix.Platform.OpenGL.Maths` holds generic value types over
`T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>` - use `float` for GL work, though `Half`,
`double`, `int` and `long` all work. Each generic struct has a same-named non-generic static companion
class holding the operations, except `Quaternion<T>`, whose statics live on the struct itself.

- `Vector2D<T>`, `Vector3D<T>` and `Vector4D<T>` carry `X`, `Y`, `Z` and `W` fields, the constants
  `Zero`, `One`, `UnitX`, `UnitY`, `UnitZ` and `UnitW`, an indexer, `Length`, `LengthSquared`, the
  operators, explicit casts to other element types and to the `System.Numerics` vectors, `As<TOther>()`
  and `CopyTo`. The static companions cover `Abs`, `Add`, `Clamp`, `Cross` (3D only), `Distance`,
  `DistanceSquared`, `Divide`, `Dot`, `Lerp`, `Max`, `Min`, `Multiply`, `Negate`, `Normalize`,
  `Reflect`, `SquareRoot`, `Subtract`, `Transform` and `TransformNormal`.
- `Matrix4X4<T>` carries `Row1` through `Row4`, the `Column1` through `Column4` properties, `M11`
  through `M44`, both indexers, `Identity`, `IsIdentity`, `GetDeterminant()` and `As<TOther>()`; the
  static companion covers the `CreatePerspective*`, `CreateOrthographic*`, `CreateLookAt`,
  `CreateTranslation`, `CreateScale`, `CreateRotationX/Y/Z`, `CreateFromAxisAngle`,
  `CreateFromQuaternion`, `CreateFromYawPitchRoll`, `CreateWorld`, `CreateBillboard`,
  `CreateConstrainedBillboard`, `CreateReflection` and `CreateShadow` builders, plus `Invert`,
  `Transpose`, `Multiply`, `Add`, `Subtract`, `Negate`, `Lerp`, `Transform` and `Decompose`. The other
  shapes - 2x2 through 5x4 - are all present with their own companions.
- `Quaternion<T>` carries `X`, `Y`, `Z`, `W`, `Identity`, `IsIdentity`, `Length()`, `LengthSquared()`,
  the `CreateFrom*` builders, `Normalize`, `Conjugate`, `Inverse`, `Dot`, `Lerp`, `Slerp`, `Concatenate`
  and the arithmetic, with an explicit cast to `System.Numerics.Quaternion`.
- The shapes are `Box2D<T>`, `Box3D<T>`, `Rectangle<T>` (with `Rectangle.FromLTRB<T>`), `Cube<T>`,
  `Circle<T>`, `Sphere<T>`, `Plane<T>` and `Ray2D<T>` / `Ray3D<T>`.
- `Scalar` is generic scalar math - `As<TFrom, TTo>`, `IsHardwareAccelerated`, arithmetic and
  comparison helpers, the full transcendental set, `DegreesToRadians<T>` and `RadiansToDegrees<T>` -
  and `Scalar<T>` holds the per-type constants (`Epsilon`, `MaxValue`, `MinValue`, `NaN`, the
  infinities, `Zero`, `One`, `Two`, `MinusOne`, `MinusTwo`, `E`, `Pi`, `PiOver2`, `Tau`,
  `DegreesPerRadian`, `RadiansPerDegree`).
- `SystemNumericsExtensions` converts with `ToSystem(...)` and `ToGeneric(...)`. Only the `<float>`
  instantiations convert; for others go through `As<float>()`.

Storage is row-major with the row-vector convention (`v * M`), the same as `System.Numerics.Matrix4x4`.
Upload with `transpose = false` and multiply in GLSL as `gl_Position = vec4(pos, 1) * model * view *
proj`, or keep column-vector GLSL and pass `transpose = true`.

There is no `Uniform` overload taking `Maths.Vector3D<T>` or `Matrix4X4<T>`: convert with `.ToSystem()`,
or pass the first component by `in`.

### The native helpers

`SilkMarshal` is the static marshalling helper - `Allocate`, `Free`, `StringToPtr`, `PtrToString`,
`FreeString`, `StringLength`, `StringToMemory`, `MemoryToString`, the string-array pairs,
`GetMaxSizeOf`, `StringIntoSpan`, `DelegateToPtr`, `PtrToDelegate<T>`, the `DelegateTo*` calling
convention helpers, `NullRef<T>`, `GuidOf<T>`, `GuidPtrOf<T>`, `ThrowHResult` and the readonly
`IsWinapiStdcall`. `NativeStringEncoding` names the encodings; GL strings are UTF-8 or ASCII, so use
`NativeStringEncoding.UTF8`. `GlobalMemory` is an owned unmanaged block with `Allocate(int)`, `Length`,
`Handle`, `AsSpan()`, `AsSpan<T>()`, `AsRef<T>()`, `AsPtr<T>()`, implicit conversions and an indexer.

The loader layer is `UnmanagedLibrary` over `LibraryLoader` and `PathResolver` / `DefaultPathResolver`,
with `SearchPathContainer` holding the per-operating-system name tables and `SymbolLoadingException`
reporting a missing symbol. The rest of `Core` carries `Bool32` and `Bool8` (4-byte and 1-byte native
bools with implicit conversions), `Version32` and `Version64` (packed versions, implicit to `uint` and
`System.Version`), `RawImage` (an RGBA8 pixel carrier), `PfnVoidFunction`, `PlatformException` and
`BreakneckLock`, a spin-lock struct. `Core.Contexts` also carries window-handle plumbing for windowing
layers to implement - `INativeWindow` with its X11, Wayland, Win32, Cocoa, EGL and other members,
`INativeWindowSource`, the Vulkan surface interfaces, and `NativeWindowFlags`. The Windows-only COM and
Direct3D interop types in `Core` are irrelevant to OpenGL work; ignore them.

## Examples

A complete triangle: a context from a proc-address delegate, a vertex array with a vertex and an element
buffer, shader compilation with real error checking, a draw and a clean teardown. It compiles with
`AllowUnsafeBlocks=true`.

```csharp
using System;
using CodeBrix.Platform.OpenGL;

public sealed class Triangle : IDisposable
{
    private readonly GL _gl;
    private readonly uint _vao, _vbo, _ebo, _program;

    private const string VertexSrc = @"#version 330 core
        layout(location = 0) in vec3 aPos;
        layout(location = 1) in vec3 aColor;
        out vec3 vColor;
        void main() { vColor = aColor; gl_Position = vec4(aPos, 1.0); }";

    private const string FragmentSrc = @"#version 330 core
        in vec3 vColor;
        out vec4 FragColor;
        void main() { FragColor = vec4(vColor, 1.0); }";

    public Triangle(Func<string, nint> getProcAddress)
    {
        _gl = GL.GetApi(getProcAddress);

        float[] vertices =
        {
            //   x      y     z      r     g     b
            -0.5f, -0.5f, 0f,    1f, 0f, 0f,
             0.5f, -0.5f, 0f,    0f, 1f, 0f,
             0.0f,  0.5f, 0f,    0f, 0f, 1f,
        };
        uint[] indices = { 0, 1, 2 };

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        _gl.BufferData<float>(BufferTargetARB.ArrayBuffer,
                              new ReadOnlySpan<float>(vertices),
                              BufferUsageARB.StaticDraw);

        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        _gl.BufferData<uint>(BufferTargetARB.ElementArrayBuffer,
                             new ReadOnlySpan<uint>(indices),
                             BufferUsageARB.StaticDraw);

        uint stride = 6 * sizeof(float);
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (nint)0);
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, (nint)(3 * sizeof(float)));
        _gl.EnableVertexAttribArray(1);

        uint vs = CompileShader(ShaderType.VertexShader, VertexSrc);
        uint fs = CompileShader(ShaderType.FragmentShader, FragmentSrc);
        _program = _gl.CreateProgram();
        _gl.AttachShader(_program, vs);
        _gl.AttachShader(_program, fs);
        _gl.LinkProgram(_program);
        if (_gl.GetProgram(_program, ProgramPropertyARB.LinkStatus) == 0)
        {
            throw new InvalidOperationException("Link failed: " + _gl.GetProgramInfoLog(_program));
        }
        _gl.DeleteShader(vs);
        _gl.DeleteShader(fs);

        _gl.BindVertexArray(0);
    }

    private uint CompileShader(ShaderType type, string source)
    {
        uint shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);
        if (_gl.GetShader(shader, ShaderParameterName.CompileStatus) == 0)
        {
            throw new InvalidOperationException(type + " compile failed: " + _gl.GetShaderInfoLog(shader));
        }
        return shader;
    }

    public unsafe void Render(int width, int height)
    {
        _gl.Viewport(0, 0, (uint)width, (uint)height);
        _gl.ClearColor(0.1f, 0.1f, 0.15f, 1f);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _gl.UseProgram(_program);
        _gl.BindVertexArray(_vao);
        _gl.DrawElements(PrimitiveType.Triangles, 3, DrawElementsType.UnsignedInt, (void*)0);
        _gl.BindVertexArray(0);
        // then: your windowing layer's SwapBuffers()
    }

    public void Dispose()
    {
        _gl.DeleteProgram(_program);
        _gl.DeleteBuffer(_ebo);
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteVertexArray(_vao);
        _gl.Dispose();
    }
}
```

Notice `(void*)0` in `DrawElements` - the element buffer is bound, so the argument is an offset - and
that the whole object owns exactly one `GL` instance, created once.

On Windows you generally want both routes at once, because the platform's proc-address function returns
0 for the oldest entry points while `opengl32.dll` exports only those:

```csharp
using CodeBrix.Platform.OpenGL;
using CodeBrix.Platform.OpenGL.Core.Contexts;

static GL CreateGl(Func<string, nint> wglOrGlxGetProcAddress)
{
    string[] names = OperatingSystem.IsWindows() ? new[] { "opengl32.dll" }
                   : OperatingSystem.IsMacOS()   ? new[] { "/System/Library/Frameworks/OpenGL.framework/OpenGL" }
                   :                               new[] { "libGL.so.1", "libGL.so" };

    INativeContext exported = GL.CreateDefaultContext(names);   // FileNotFoundException if none load
    var ctx = new MultiNativeContext(new LamdaNativeContext(wglOrGlxGetProcAddress), exported);
    return GL.GetApi(ctx);
}
```

Debug output is the fastest way to find a driver-level mistake, and the static field is what keeps it
alive:

```csharp
using System;
using CodeBrix.Platform.OpenGL;
using CodeBrix.Platform.OpenGL.Core.Native;

static class GlDebug
{
    // Static field: the driver holds the native pointer for the life of
    // the context; the binding only pins it until the next GL call.
    private static readonly DebugProc Callback = OnMessage;

    public static unsafe void Enable(GL gl)
    {
        gl.Enable(EnableCap.DebugOutput);
        gl.Enable(EnableCap.DebugOutputSynchronous);
        gl.DebugMessageCallback(Callback, null);
        uint ids = 0;   // count == 0 -> applies to all ids
        gl.DebugMessageControl(DebugSource.DontCare, DebugType.DontCare,
                               DebugSeverity.DebugSeverityNotification, 0, in ids, false);
    }

    private static void OnMessage(GLEnum source, GLEnum type, int id, GLEnum severity,
                                  int length, nint message, nint userParam)
    {
        string text = SilkMarshal.PtrToString(message, NativeStringEncoding.UTF8) ?? string.Empty;
        Console.WriteLine($"GL {type} [{severity}] {id}: {text}");
    }
}
```

Camera matrices come from the math layer, and the row-vector convention decides what the shader must
look like:

```csharp
using CodeBrix.Platform.OpenGL;
using CodeBrix.Platform.OpenGL.Maths;

static void UploadMvp(GL gl, uint program, float aspect, float seconds)
{
    Matrix4X4<float> model = Matrix4X4.CreateRotationY(seconds);
    Matrix4X4<float> view = Matrix4X4.CreateLookAt(
        new Vector3D<float>(0f, 1f, 3f), Vector3D<float>.Zero, Vector3D<float>.UnitY);
    Matrix4X4<float> proj = Matrix4X4.CreatePerspectiveFieldOfView(
        Scalar.DegreesToRadians(60f), aspect, 0.1f, 100f);

    Matrix4X4<float> mvp = model * view * proj;   // row-vector convention

    int loc = gl.GetUniformLocation(program, "uMvp");
    gl.UniformMatrix4(loc, 1, false, in mvp.Row1.X);   // 16 contiguous floats

    Vector3D<float> lightDir = Vector3D.Normalize(new Vector3D<float>(1f, 2f, 0.5f));
    gl.Uniform3(gl.GetUniformLocation(program, "uLightDir"), lightDir.ToSystem());
}

// GLSL for transpose = false with this convention:
//     gl_Position = vec4(aPos, 1.0) * uMvp;
// If your shader multiplies uMvp * vec4(aPos, 1.0), pass transpose = true
// (or upload Matrix4X4.Transpose(mvp)).
```

Uploading a texture and reading the framebuffer back show the `ReadOnlySpan` and `Span` convenience
overloads, and where the enum casts are required:

```csharp
static uint CreateTexture(GL gl, uint width, uint height, byte[] rgba)
{
    uint tex = gl.GenTexture();
    gl.ActiveTexture(TextureUnit.Texture0);
    gl.BindTexture(TextureTarget.Texture2D, tex);
    gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
    gl.TexImage2D<byte>(TextureTarget.Texture2D, 0, InternalFormat.Rgba8,
                        width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte,
                        new ReadOnlySpan<byte>(rgba));
    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
    gl.GenerateMipmap(TextureTarget.Texture2D);
    return tex;
}

static byte[] ReadBackRgba(GL gl, int width, int height)
{
    var pixels = new byte[width * height * 4];
    gl.PixelStore(PixelStoreParameter.PackAlignment, 1);
    gl.ReadPixels<byte>(0, 0, (uint)width, (uint)height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.AsSpan());
    GLEnum err = gl.GetError();
    if (err != GLEnum.NoError) throw new InvalidOperationException("GL error " + err);
    return pixels;   // bottom row first (OpenGL origin is bottom-left)
}
```

A skeleton application shows the whole wiring, with the windowing layer left as a placeholder delegate -
a console application that owns no window cannot render:

```xml
<!-- GlApp.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.Platform.OpenGL.MitLicenseForever" />
    <!-- plus your windowing package (SDL / GLFW / platform layer) -->
  </ItemGroup>
</Project>
```

```csharp
// Program.cs
using System;
using CodeBrix.Platform.OpenGL;

// 1. create a window + GL context with your windowing library and make
//    it current on this thread
// 2. obtain its proc-address function:
Func<string, nint> getProcAddress = name => throw new NotImplementedException("wire to SDL_GL_GetProcAddress / glfwGetProcAddress");

using var triangle = new Triangle(getProcAddress);   // from Example 1
// 3. per frame:
//    triangle.Render(width, height);  then swap buffers via the windowing library
```

## Pitfalls

- **No current context, no calls.** Every GL method executes on the calling thread against whatever
  context is current there. Create the `GL` instance and issue all calls on the thread that owns the
  context - typically the UI or render thread. Calling from a `Task` or another thread yields
  `GL_INVALID_OPERATION` at best and a crash at worst.
- **Errors surface late.** A wrong or missing entry point throws `SymbolLoadingException` at the first
  *call* of that method, not when `GL` is created. Check `GetStringS(StringName.Version)` early.
- **`DefaultNativeContext` and `CreateDefaultContext` resolve exported symbols only.** On Windows that
  is OpenGL 1.1; anything newer needs the windowing layer's proc-address function, through
  `GL.GetApi(Func<string, nint>)` or a `MultiNativeContext`.
- **Unsafe is required** for the pointer overloads and for the `DrawElements` offset idiom. Set
  `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>`.
- **Span and pointer overloads mean different things for `DrawElements`.** The `ReadOnlySpan<T0>` and
  `in` forms pass a managed address (a client-side index array); `void*` passes an offset into the bound
  element buffer. Using the span form with an element buffer bound reads garbage or faults.
- **`BufferData<T0>(target, ReadOnlySpan<T0>, usage)` computes the size for you**; the
  `(target, nuint size, ref readonly T0, usage)` form does not - size is in bytes, and forgetting
  `sizeof(T)` uploads a truncated buffer.
- **Keep the `DebugProc` delegate in a static or long-lived field.** The binding pins it only until the
  next GL call; the driver calls it forever.
- **`ClearColor<T>(Vector4D<T>)` and `BlendColor<T>(Vector4D<T>)` divide each component by 255,** so a
  `Vector4D<float>(0.2f, ...)` clears to almost black. Use `ClearColor(float, float, float, float)` for
  0..1 values, which is also the direct path in a hot loop.
- **`GenBuffers(uint n)` returns only the first handle of n.** Use the Span form to get all of them; the
  single-handle helpers are `GenBuffer()`, `GenTexture()`, `GenVertexArray()`, `GenFramebuffer()` and
  `GenRenderbuffer()`.
- **Enum names keep their registry prefixes and suffixes,** and `GetError` and `CheckFramebufferStatus`
  return `GLEnum` rather than `ErrorCode` or `FramebufferStatus`.
- **`TexParameter` takes `int` or `float`, not the filter and wrap enums.** Cast:
  `(int)TextureMinFilter.Linear`.
- **`using CodeBrix.Platform.OpenGL;` brings in a struct named `Buffer`.** Qualify `System.Buffer` if you
  use both. `Maths.Rectangle<T>` and `Maths.Quaternion<T>` collide with `System.Drawing` and
  `System.Numerics` only by simple name, and the `GL.Uniform` overloads take the `System.Numerics`
  types.
- **Matrix layout.** The math matrices are row-major with the row-vector convention, so
  `transpose = false` requires GLSL that multiplies `vec * mat`; otherwise pass `transpose = true` or
  transpose the matrix yourself.
- **There is no GL context in unit tests.** On a headless host there is no way to make a context, so
  tests of code that calls GL must inject a fake around your own abstraction. The types and enums
  themselves can be tested freely - they never touch native code until a method is called.
- **Performance.** Entry points are resolved once per `GL` instance and then called through a cached
  unmanaged function pointer, so keep one instance per context and never call `GL.GetApi` per frame.
  Prefer the `Span<T>`, `ReadOnlySpan<T>` and `in` overloads over arrays of handles or manual pinning -
  they pin only for the duration of the call. The `string`-taking overloads allocate and encode on every
  call, so cache uniform locations at link time; the string-returning conveniences query GL twice, so
  use them at load time only; `IsExtensionPresent` and `TryGetExtension<T>` belong at start-up; and
  `GetError` is a full driver round-trip, so use it in debug builds only and rely on the debug callback
  otherwise. For hot inner loops on `float` data, consider converting to `System.Numerics` with
  `ToSystem()` and back with `ToGeneric()`.

## Samples and tools in the repository

The repository contains no sample applications, demo projects, benchmark projects or tool projects. It
holds exactly two projects: the packable library and its test project. There is no `samples/` folder, no
`tools/` folder and no build or code-generation scripts - the pre-captured generated method bodies are
committed as ordinary source inside the library.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Test project | The worked-example corpus for everything that can be exercised without a GPU: string and pointer marshalling in every supported native string encoding, the vector, matrix, quaternion, plane and scalar suites, and the GL type hierarchy and enum spec values | [`tests/CodeBrix.Platform.OpenGL.Tests`](https://github.com/ellisnet/CodeBrix.Platform.OpenGL/tree/main/tests/CodeBrix.Platform.OpenGL.Tests) |

```bash
dotnet test CodeBrix.Platform.OpenGL.slnx
```

What the test project deliberately does not contain is a running OpenGL demo. No test issues a real GL
call, because every entry point needs a live context current on the calling thread and the test host is
headless. To see the bindings draw something, build a small consuming application with a windowing
library, starting from the triangle above. There are no opt-in environment variables and no optional
test-data sets in the repository.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Platform.OpenGL/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.OpenGL/blob/main/AGENT-README.txt) |
| Non-package content in the repository | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Platform.OpenGL/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform.OpenGL/blob/main/README-INDEX.txt) |
| Tests (worked examples of the marshalling and math APIs) | [tests/CodeBrix.Platform.OpenGL.Tests](https://github.com/ellisnet/CodeBrix.Platform.OpenGL/tree/main/tests/CodeBrix.Platform.OpenGL.Tests) |

XML documentation ships alongside the assembly. The package's automatic dependencies are
`Microsoft.DotNet.PlatformAbstractions` and `Microsoft.Extensions.DependencyModel`, both used by the
native-library resolver; the package has no other dependencies, and it does not bring a source generator
into your build.

## License

CodeBrix.Platform.OpenGL is licensed under the MIT License, and the license is also named in the package
ID (`CodeBrix.Platform.OpenGL.MitLicenseForever`). For the provenance and licensing of open source code
included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.OpenGL/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Platform.GameEngine](CodeBrix.Platform.GameEngine.md) - a 2D and 2.5D engine with a GPU render path, in the same topic group
- [Graphics3DGL add-in](../platform/add-ins/Graphics3DGL.md) - the CodeBrix.Platform package that carries the GPU render path
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.Platform.OpenGL on GitHub](https://github.com/ellisnet/CodeBrix.Platform.OpenGL) - source and tests
