<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Sdl3</sub>

# CodeBrix.Sdl3

**CodeBrix.Sdl3 is a set of C# bindings for SDL3 (Simple DirectMedia Layer 3), with the SDL3 native
library for nine runtime identifiers inside the package.** SDL3 is the cross-platform C library for
windows, input, gamepads, audio, cameras, 2D rendering, the SDL GPU API and the rest; the bindings are a
near one-to-one projection of its C API - the same function, struct, enum and constant names as the C
headers - with a thin layer of C# conveniences on top: string overloads, `SDLBool`, typed flag enums,
array wrappers and the C macros as methods. Nothing has to be installed on the target machine. Use it
from any .NET 10 application, including an Android one, that wants SDL3 directly.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Sdl3](https://github.com/ellisnet/CodeBrix.Sdl3) |
| **Packages** | [`CodeBrix.Sdl3.ZlibLicenseForever`](https://www.nuget.org/packages/CodeBrix.Sdl3.ZlibLicenseForever) |
| **License** | MIT and zlib - the managed bindings are MIT, SDL3 and its native binaries are zlib; see [License](#license) |
| **Requires** | .NET 10 or later (`net10.0`, plus `net10.0-android`), and `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` in the consuming project. No NuGet dependencies |
| **Use it from** | Any .NET 10 application, or a .NET 10 Android application |
| **Platforms** | Windows x64 and ARM64, macOS on Intel and Apple Silicon, Linux x64, ARM64 and RISC-V 64, Android x64 and ARM64 |

## What it does

- Exposes the SDL3 C API from C#, generated from the SDL3 headers: initialization, windows and displays,
  events, keyboard, mouse, pen, touch, gamepads and joysticks, haptics, sensors, audio, cameras, the 2D
  renderer, the GPU API, surfaces and pixel formats, clipboard, file dialogs, filesystem and storage, I/O
  streams, properties, threads, timers, logging, tray icons and more.
- Brings every SDL3 `#define` constant, enum, struct, callback and macro across as C# constants, enums,
  structs, function pointers and methods.
- Adds friendly overloads, generated at compile time, that take C# strings for `const char *`
  parameters and return C# strings for `char *` results.
- Supplies `SDLBool`, `Utf8String` and SDL-owned array wrappers that free SDL memory when disposed.
- Ships the SDL3 native library for `win-x64`, `win-arm64`, `osx-x64`, `osx-arm64`, `linux-x64`,
  `linux-arm64`, `linux-riscv64`, `android-x64` and `android-arm64`, selected automatically by NuGet.
- Carries the SDL Java bridge (`SDLActivity`) in its `net10.0-android` build, so an Android application
  needs only one activity class of its own.

## When to use it

Reach for CodeBrix.Sdl3 when a .NET application wants SDL3 itself - an SDL window and event loop, the
SDL renderer or GPU API, gamepads, audio devices or cameras - with the native library delivered by the
package rather than installed on each machine.

What it does not do:

- It binds SDL3 core only. There are no SDL_image, SDL_ttf, SDL_mixer or SDL_net bindings or natives.
- It has no natives for iOS, `win-x86`, `linux-x86`, 32-bit ARM (`linux-arm`, `android-arm`) or
  `android-x86`; on those the first call throws `DllNotFoundException`.
- It adds no higher-level object model - no window class and no game loop. It is the SDL3 API as SDL
  defines it, and the [SDL3 wiki](https://wiki.libsdl.org/SDL3/) is the reference for behaviour,
  ownership and threading rules.
- It offers no managed wrappers around SDL's C-variadic functions; see [Pitfalls](#pitfalls).

## Getting started

```bash
dotnet add package CodeBrix.Sdl3.ZlibLicenseForever
```

The package ID carries the license suffix; the namespace does not. You install
`CodeBrix.Sdl3.ZlibLicenseForever` and you write `using CodeBrix.Sdl3;`. The API is pointer-based, so
the consuming project needs `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>`.

```csharp
using System;
using CodeBrix.Sdl3;
using static CodeBrix.Sdl3.SDL3;

unsafe
{
    if (!SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO))
        throw new InvalidOperationException($"SDL_Init failed: {SDL_GetError()}");

    SDL_Window* window = SDL_CreateWindow("Hello SDL3", 800, 600, SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
    SDL_Renderer* renderer = SDL_CreateRenderer(window, (byte*)null);

    bool running = true;
    SDL_Event e;
    while (running)
    {
        while (SDL_PollEvent(&e))
        {
            if (e.Type == SDL_EventType.SDL_EVENT_QUIT)
                running = false;
        }

        SDL_SetRenderDrawColor(renderer, 30, 60, 120, 255);
        SDL_RenderClear(renderer);
        SDL_RenderPresent(renderer);
    }

    SDL_DestroyRenderer(renderer);
    SDL_DestroyWindow(window);
    SDL_Quit();
}
```

Passing `(byte*)null` as the renderer name lets SDL pick the best renderer; the cast makes it plain which
of the two overloads (`byte*` or `Utf8String`) is meant.

| Using | What it gives you |
| --- | --- |
| `using CodeBrix.Sdl3;` | The structs and enums, `SDLBool`, `Utf8String`, `SDLArray<T>` and the other array wrappers |
| `using static CodeBrix.Sdl3.SDL3;` | Every `SDL_*` function and constant without the `SDL3.` prefix, so code reads like C |
| `using Org.Libsdl.App;` | Android only: `SDLActivity` |

There are no other public namespaces.

## Key concepts

### The SDL3 class

`public static unsafe partial class SDL3` holds every SDL function, constant and macro under exactly the
C names - `SDL3.SDL_Init(...)`, `SDL3.SDL_CreateWindow(...)`, `SDL3.SDL_MAJOR_VERSION`. Native functions
are `[DllImport("SDL3")]` declarations, and the name `SDL3` resolves to the packaged library for the
running platform. Any SDL documentation for a C function applies unchanged.

### Type mapping

| C | C# |
| --- | --- |
| `bool` | `SDLBool`, which converts implicitly to and from `bool` |
| `char` / `Uint8` | `byte` |
| `const char *` parameter | `byte*`, and a friendly overload taking `Utf8String` |
| `const char *` or `char *` result | `string?` from the friendly overload; the raw `byte*` form is named `Unsafe_<function>` |
| `void *` | `IntPtr` |
| `Sint64` / `Uint64` | `long` / `ulong` |
| `size_t` | `UIntPtr` (`nuint`) |
| opaque handles | `SDL_Window*`, `SDL_Renderer*`, `SDL_Gamepad*` and so on |
| ID typedefs | small structs - `SDL_JoystickID`, `SDL_DisplayID`, `SDL_AudioDeviceID`, `SDL_PropertiesID` |
| `#define` flag sets | typed `[Flags]` enums (`SDL_InitFlags`, `SDL_WindowFlags`) and the raw constants |
| callbacks | `delegate* unmanaged[Cdecl]<...>` function pointers |

### Strings

A `Utf8String` parameter accepts a C# string or a UTF-8 literal by implicit conversion, and a terminating
NUL is appended when missing:

```csharp
SDL_SetHint(SDL_HINT_VIDEO_DRIVER, "x11");     // string
SDL_SetHint(SDL_HINT_VIDEO_DRIVER, "x11"u8);   // ReadOnlySpan<byte>
SDL_SetAppMetadata("My App", "1.0", "com.example.myapp");

string? err  = SDL_GetError();
byte*   raw  = Unsafe_SDL_GetError();          // rarely needed
```

`Utf8String` is a `ref struct`: it cannot be stored in a field, captured by a lambda or used across an
`await`, so convert at the call site. `SDL_HINT_*` and the other string `#define`s are `ReadOnlySpan<byte>`
properties, usable anywhere a `Utf8String` is expected. `SDL3.PtrToStringUTF8(byte* ptr, bool free = false)`
copies a NUL-terminated string from a pointer, and event structs carry helpers for their string members,
such as `SDL_TextInputEvent.GetText()` and `SDL_DropEvent.GetData()`.

### Arrays SDL allocates

A function that returns an SDL-allocated array has an overload that wraps it, with `Count`, an indexer
and a `foreach` enumerator; the overload returns null when SDL returned NULL.

| Wrapper | Wraps | Disposal |
| --- | --- | --- |
| `SDLArray<T>` | `T*` owned by the caller | `Dispose` frees it |
| `SDLPointerArray<T>` | `T**` owned by the caller | the indexer dereferences |
| `SDLOpaquePointerArray<T>` | `T**` of opaque handles | `Dispose` frees it |
| `SDLConstOpaquePointerArray<T>` | `const T**` owned by SDL | not disposable |

`SDL_GetGamepads`, `SDL_GetKeyboards`, `SDL_GetDisplays`, `SDL_GetFullscreenDisplayModes`,
`SDL_GetWindows`, `SDL_GetAudioPlaybackDevices`, `SDL_GetAudioRecordingDevices` and
`SDL_GetGamepadMappings` are among them.

### Events, macros and errors

`SDL_Event` is a union struct: read `e.Type` (an `SDL_EventType`), then the matching member - `e.key`,
`e.button`, `e.motion`, `e.gbutton`, `e.gaxis`, `e.window`, `e.text`, `e.drop` and the rest. C macros are
methods marked `[Macro]`: `SDL_VERSIONNUM`, `SDL_VERSIONNUM_MAJOR`, `SDL_WINDOWPOS_CENTERED_DISPLAY`,
`SDL_AUDIO_BITSIZE`, `SDL_DEFINE_PIXELFORMAT` and so on.

Nothing throws for an SDL failure. Functions report failure exactly as in C - `false`, a null pointer, a
zero ID or -1 - and `SDL_GetError()` returns the message. Check every return value that can fail.

### The native libraries

The package carries the SDL3 native library under `runtimes/<rid>/native/`, each beside
`LICENSE-SDL3.txt`:

| Runtime identifier | File |
| --- | --- |
| `win-x64`, `win-arm64` | `SDL3.dll` |
| `osx-x64`, `osx-arm64` | `libSDL3.dylib` |
| `linux-x64`, `linux-arm64`, `linux-riscv64` | `libSDL3.so` |
| `android-x64`, `android-arm64` | `libSDL3.so` |

The .NET host picks the right one for the running platform: a framework-dependent `dotnet run` uses the
`deps.json` entry, and a runtime-specific build or publish copies just that one file beside the
application. The Linux libraries are built for an old glibc baseline and load SDL's X11, Wayland, ALSA,
PulseAudio and PipeWire backends at run time from whatever the system provides, so a missing backend is
simply not offered rather than failing to load.

### Android

Reference the package from a `net10.0-android` application. That build of the assembly also carries the
SDL Java bridge, bound to C# in the `Org.Libsdl.App` namespace, and the Android natives arrive in the APK
automatically. SDL on Android is driven by its Java activity, so the application's main activity must
derive from `SDLActivity`, load only `SDL3`, and run the C# entry point from `Main()`:

```csharp
using Android.App;
using Org.Libsdl.App;

[Activity(Label = "My SDL App", MainLauncher = true)]
public class MainActivity : SDLActivity
{
    protected override string[] GetLibraries() => ["SDL3"];

    protected override void Main() => MyGame.Run();   // your SDL loop
}
```

`SDL_Init` and the rest of the API are then used exactly as on the desktop, from inside `Main()`.

## Examples

Headless use - continuous integration, servers and tests - on SDL's dummy video driver.

```csharp
SDL_SetHint(SDL_HINT_VIDEO_DRIVER, "dummy");
if (!SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO))
    throw new InvalidOperationException(SDL_GetError());
SDL_Window* w = SDL_CreateWindow("offscreen", 320, 240, SDL_WindowFlags.SDL_WINDOW_HIDDEN);
SDL_Renderer* r = SDL_CreateRenderer(w, (byte*)null);   // "software"
// ...
SDL_DestroyRenderer(r);
SDL_DestroyWindow(w);
SDL_Quit();
```

Enumerating gamepads; button and axis events then arrive in the event loop as `e.gbutton.Button` and
`e.gaxis.Axis`.

```csharp
SDL_Init(SDL_InitFlags.SDL_INIT_GAMEPAD);
using (var ids = SDL_GetGamepads())
{
    if (ids != null)
    {
        foreach (SDL_JoystickID id in ids)
        {
            SDL_Gamepad* pad = SDL_OpenGamepad(id);
            Console.WriteLine(SDL_GetGamepadName(pad));
        }
    }
}
```

The version of the loaded library, against the header constants the bindings were generated from.

```csharp
using static CodeBrix.Sdl3.SDL3;

int version = SDL_GetVersion();
Console.WriteLine($"SDL {SDL_VERSIONNUM_MAJOR(version)}.{SDL_VERSIONNUM_MINOR(version)}.{SDL_VERSIONNUM_MICRO(version)} ({SDL_GetRevision()})");
// SDL_VERSION and SDL_MAJOR_VERSION are the header constants; SDL_GetVersion() is the loaded library
```

A minimal project file.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.Sdl3.ZlibLicenseForever" Version="*" />
  </ItemGroup>
</Project>
```

Pin the current version rather than `*` in a real project.

## Pitfalls

- `using SDL;` does not compile. The namespace is `CodeBrix.Sdl3`.
- Forgetting `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` gives `CS0227` on the first pointer.
- SDL's C-variadic functions - `SDL_SetError`, `SDL_Log` and its family, `SDL_snprintf`, `SDL_asprintf`,
  `SDL_sscanf`, `SDL_IOprintf`, `SDL_RenderDebugTextFormat`, and the `SDL_Unsupported` and
  `SDL_InvalidParamError` macros - are declared with `__arglist`, which .NET supports only on Windows. On
  Linux, macOS and Android the call throws `InvalidProgramException`. Format the text in C# and call a
  non-variadic function instead - `SDL_RenderDebugText(renderer, x, y, $"...")` rather than
  `SDL_RenderDebugTextFormat` - and log through your own logger rather than `SDL_Log`.
- Calling `SDL_free` on a string a friendly overload returned frees memory you do not own: it is already
  a managed copy, freed where SDL required it. Free only what an `Unsafe_` or `byte*` variant documents
  as caller-owned.
- Storing a `Utf8String`: it points at a temporary buffer. Convert at the call site.
- Treating `SDLBool` as an integer. Use it as a `bool`.
- Not checking returns. A null `SDL_Window*` or a false `SDL_Init` is a failure with a message in
  `SDL_GetError()`, never an exception.
- On Android, an activity that is not an `SDLActivity`, or one whose `GetLibraries` does not include
  `SDL3`, leaves SDL uninitialized.
- Running on a runtime identifier with no packaged native throws `DllNotFoundException` on the first
  call.
- In per-frame code, the `string` overloads allocate and encode on every call; prefer the `"..."u8`
  literal forms there, poll events into a stack `SDL_Event`, and dispose `SDLArray` results promptly.
- SDL functions are thread-affine where SDL says so - video and events on the main thread. Follow the
  SDL wiki rather than adding locks.

## Samples and tools in the repository

The repository ships no sample applications. Its non-package content is the test suite, the source
generator that writes the friendly overloads, the binding-generation tool, the committed native
libraries and the tooling that builds them.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Tests | The dummy video driver, versions, `Utf8String`, the array wrappers, `SDLBool`, the variadic-call platform behaviour and the native layout. Tests whose native library for the current platform is not present skip with the reason | [`tests/CodeBrix.Sdl3.Tests`](https://github.com/ellisnet/CodeBrix.Sdl3/tree/main/tests/CodeBrix.Sdl3.Tests) |
| Source generator | Writes the friendly overloads while the library compiles; not packed, its output is part of the assembly | [`src/CodeBrix.Sdl3.SourceGeneration`](https://github.com/ellisnet/CodeBrix.Sdl3/tree/main/src/CodeBrix.Sdl3.SourceGeneration) |
| Binding-generation tool | Regenerates the generated bindings from the SDL3 headers; run by hand only | [`tools/sdl3_binding_generation`](https://github.com/ellisnet/CodeBrix.Sdl3/tree/main/tools/sdl3_binding_generation) |
| Native libraries | The committed binaries the package redistributes, each with a provenance record | [`native_libraries`](https://github.com/ellisnet/CodeBrix.Sdl3/tree/main/native_libraries) |
| Native build tooling | Everything needed to rebuild every native library and the Java bridge from the vendored SDL source | [`sdl3-native-tools`](https://github.com/ellisnet/CodeBrix.Sdl3/tree/main/sdl3-native-tools) |

Test files worth opening first:

| Feature | Test file |
| --- | --- |
| Hint, init, window, renderer and event loop on the dummy driver | [`DummyVideoDriverSmoke.cs`](https://github.com/ellisnet/CodeBrix.Sdl3/blob/main/tests/CodeBrix.Sdl3.Tests/DummyVideoDriverSmoke.cs) |
| Versions and version macros | [`VersionTests.cs`](https://github.com/ellisnet/CodeBrix.Sdl3/blob/main/tests/CodeBrix.Sdl3.Tests/VersionTests.cs) |
| `Utf8String` | [`Utf8StringTests.cs`](https://github.com/ellisnet/CodeBrix.Sdl3/blob/main/tests/CodeBrix.Sdl3.Tests/Utf8StringTests.cs) |
| The array wrappers | [`SDLArrayTests.cs`](https://github.com/ellisnet/CodeBrix.Sdl3/blob/main/tests/CodeBrix.Sdl3.Tests/SDLArrayTests.cs) |
| `SDLBool` | [`SDLBoolTests.cs`](https://github.com/ellisnet/CodeBrix.Sdl3/blob/main/tests/CodeBrix.Sdl3.Tests/SDLBoolTests.cs) |
| Error macros and variadic calls | [`ErrorMacroTests.cs`](https://github.com/ellisnet/CodeBrix.Sdl3/blob/main/tests/CodeBrix.Sdl3.Tests/ErrorMacroTests.cs) |
| The `runtimes/<rid>/native` layout | [`NativeLibraryLoadTests.cs`](https://github.com/ellisnet/CodeBrix.Sdl3/blob/main/tests/CodeBrix.Sdl3.Tests/NativeLibraryLoadTests.cs) |

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Sdl3/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Sdl3/blob/main/AGENT-README.txt) |
| Non-package content in the repository | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Sdl3/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Sdl3/blob/main/README-INDEX.txt) |
| Tests (worked examples) | [tests/CodeBrix.Sdl3.Tests](https://github.com/ellisnet/CodeBrix.Sdl3/tree/main/tests/CodeBrix.Sdl3.Tests) |

XML documentation ships alongside the assembly.

## License

CodeBrix.Sdl3 is licensed under two licenses, and the package's license expression is `MIT AND Zlib`.
The managed binding code is MIT, under the repository's
[LICENSE](https://github.com/ellisnet/CodeBrix.Sdl3/blob/main/LICENSE) file. SDL3 itself - the native
binaries in `runtimes/`, and the binding code machine-generated from the SDL3 headers - is under the zlib
License, and every runtime-identifier folder carries SDL's license as `LICENSE-SDL3.txt` beside the
library. The package ID names the zlib License (`CodeBrix.Sdl3.ZlibLicenseForever`), the more
notice-demanding of the two, and the package requires license acceptance. For the provenance and
licensing of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Sdl3/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Platform.GameEngine](CodeBrix.Platform.GameEngine.md) - a managed 2D game engine, with its own optional gamepad package
- [CodeBrix.Platform.OpenGL](CodeBrix.Platform.OpenGL.md) - a managed OpenGL Core-profile binding
- [CodeBrix.Audio](CodeBrix.Audio.md) - reading, writing and playing audio and MIDI
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.Sdl3 on GitHub](https://github.com/ellisnet/CodeBrix.Sdl3) - source and tests
