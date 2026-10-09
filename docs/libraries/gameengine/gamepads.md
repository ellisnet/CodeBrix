<sub>[CodeBrix](../../../README.md) › [Libraries](../README.md) › [CodeBrix.Platform.GameEngine](../CodeBrix.Platform.GameEngine.md) › Gamepads</sub>

# Gamepads

**The gamepad add-on gives a [CodeBrix.Platform.GameEngine](../CodeBrix.Platform.GameEngine.md) game
buttons, sticks, triggers, device identity and hotplug through SDL2, on all six CodeBrix.Platform heads,
with one call.** It fills the gamepad seam the engine core defines, the engine refreshes it every cycle
or every tic, and nothing in it throws when SDL2 or a controller is missing, because gamepad support is
an enhancement to a game that is already playable with keyboard and mouse.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Platform.GameEngine](https://github.com/ellisnet/CodeBrix.Platform.GameEngine) |
| **Package** | [`CodeBrix.Platform.GameEngine.Sdl2.ZlibLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.GameEngine.Sdl2.ZlibLicenseForever) |
| **License** | `MIT AND Zlib`; see [License](#license) |
| **Requires** | .NET 10 or later and the engine package, which arrives automatically. On Linux, the system SDL2 runtime |
| **Use it from** | A CodeBrix.Platform game, in either hosting mode |
| **Platforms** | All six CodeBrix.Platform heads, frame buffer included |

## What it does

- Buttons, sticks, triggers, device identity and hotplug, from one implementation that covers all six
  heads.
- One call attaches it - `Engine.Instance.InitializeSdlGamepadManager()` - and from then on the engine
  refreshes controller state every cycle or every tic and raises its own gamepad events. A game never
  calls `Update()` itself.
- SDL2 is initialized with the game controller subsystem only, which starts no video subsystem: it
  never creates a window, never opens an X11 or Wayland display, and never touches Win32 or AppKit. It
  is a headless joystick backend, which is why the frame-buffer head gets controllers on the same terms
  as the desktop heads.
- Nothing in it throws when SDL2 or a controller is missing. Absence is reported through inspectable
  properties and one log line, because gamepad support is an enhancement to a game that is already
  playable with keyboard and mouse.
- The package carries SDL2 native binaries for Windows and macOS. On Linux it uses the system SDL2.

## When to use it

Add it when the game should also answer a game controller. A game that wants named actions over the
keyboard and every gamepad at once puts the engine's `InputActionMap` on top of it - see
[Input actions](../CodeBrix.Platform.GameEngine.md#input-actions) on the engine page; the map reads the
same seam and needs no gamepad backend of its own.

It deliberately leaves things out. There is no rumble, haptics, LED, gyro, touchpad, battery
level or audio routing - the bound surface is buttons, sticks, triggers, identity and hotplug. No raw
joystick mode, so devices SDL2 does not recognize as game controllers are not surfaced and there is no
API for custom mapping strings. No rebinding or profile UI; button identity is reported, and mapping a
game action to a button is the game's job. No windowing, rendering, audio, keyboard or mouse, because
CodeBrix.Platform owns all of that. It does not pump the SDL2 event queue, so no second event loop runs
alongside the CodeBrix.Platform one. And it neither drives itself nor disposes itself with the engine.

## Getting started

Add the package to the shared game library, next to the engine package:

```bash
dotnet add package CodeBrix.Platform.GameEngine.Sdl2.ZlibLicenseForever
```

On Linux, gamepads have exactly one prerequisite, and it is the runtime package rather than the
development one:

```bash
sudo apt install libsdl2-2.0-0
```

The package adds three namespaces, of which application code uses one:

```csharp
using CodeBrix.Platform.GameEngine.Sdl2;           // the ONE entry point:
                                                   // InitializeSdlGamepadManager
using CodeBrix.Platform.GameEngine.Sdl2.Gamepad;   // SdlGamepadManager,
                                                   // SdlGamepadAdapter,
                                                   // SdlGamepadButtons,
                                                   // SdlGamepadUnavailableCause
using CodeBrix.Platform.GameEngine.Sdl2.Native;    // raw SDL2 P/Invoke;
                                                   // NOT for app code
```

## Key concepts

### The seam and its SDL2 implementation

The engine core defines the seam - `IGamepadManager<T>`, `IGamepadAdapter`, `GamepadStickState` and
`GamepadEventPoller` - and the gamepad package fills it. One extension method is the whole entry point:

```csharp
public static class EngineGamepadExtensions
{
    public static SdlGamepadManager InitializeSdlGamepadManager(
        this Engine engine, bool logStatus = true);
}
```

It starts SDL2 gamepad support and assigns the resulting manager to `engine.Input.GamepadManager`;
nothing else has to be plumbed. Call it once, after the engine has been started. It always returns a
usable manager and never returns null, including when SDL2 or a controller is missing - in that case
`IsAvailable` is false and `UnavailableReason` explains why. With `logStatus` left at true it writes the
outcome to the engine log once: a warning carrying `UnavailableCause` and `UnavailableReason` when
support is unavailable, an information line carrying `GetNoControllersHint()` when support is available
with nothing connected, or one line per connected pad carrying its `Name`, `GamepadId` and mapping
string.

```csharp
public sealed class SdlGamepadManager
    : IGamepadManager<SdlGamepadAdapter>, IDisposable
{
    public static bool TryStart(out SdlGamepadManager manager);

    public bool IsAvailable { get; }
    public string? UnavailableReason { get; }
    public SdlGamepadUnavailableCause UnavailableCause { get; }
    public IReadOnlyCollection<SdlGamepadAdapter> ConnectedAdapters { get; }

    public void Update();
    public string? GetNoControllersHint();
    public void Dispose();
}
```

There is no public constructor. `IsAvailable` says gamepad support is functional, not that a controller
is plugged in - check `ConnectedAdapters.Count` for that. `ConnectedAdapters` is a live view: the same
collection instance every time, with contents changing as controllers come and go, which is what lets
the engine's `GamepadEventPoller` see controllers that appear later. Do not copy it and expect the copy
to track hotplug. `Update()` refreshes the device list and every connected controller's state, and the
engine calls it; calling it yourself defeats the engine's `TimeBetweenGamepadStateUpdates` throttle.
`SdlGamepadUnavailableCause` is the machine-readable counterpart to `UnavailableReason`, with
`NativeLibraryMissing` and `SubsystemInitializationFailed` alongside `None`; on Linux the first normally
means the SDL2 runtime package is not installed, something the player can fix, while on Windows and
macOS - where SDL2 ships inside the application - it indicates a packaging problem worth reporting.

```csharp
public sealed class SdlGamepadAdapter : IGamepadAdapter, IDisposable
{
    public string GamepadId { get; }                       // "sdl:<instanceId>"
    public int InstanceId { get; }
    public string Name { get; }            // e.g. "Xbox One S Controller"
    public IReadOnlyCollection<string> PressedButtons { get; }
    public GamepadStickState? LeftStick { get; }
    public GamepadStickState? RightStick { get; }
    public float LeftTrigger { get; }                      // 0.0 .. 1.0
    public float RightTrigger { get; }                     // 0.0 .. 1.0
    public bool IsConnected { get; }
    public string GetMappingString();
    public void Dispose();
}
```

Every property is a snapshot of the most recent refresh rather than a live native query, so reading
several within one frame gives a consistent view of the controller. `PressedButtons` holds values from
`SdlGamepadButtons` and is a live view refreshed in place each frame, so read it within the frame and
never cache the collection. `GetMappingString()` returns the mapping string for the device, or an empty
string when unavailable; the mapping reconciles the device's raw button and axis numbering with the
standard layout and differs by transport, so it is the first thing to log when a controller behaves
oddly.

`SdlGamepadButtons` is a set of `public const string` names - `A`, `B`, `X`, `Y`, `Back`, `Guide`,
`Start`, `LeftStick`, `RightStick`, `LeftShoulder`, `RightShoulder`, `DPadUp`, `DPadDown`, `DPadLeft`
and `DPadRight` - plus `All`. Use the constants rather than string literals: the seam identifies buttons
by string, so a misspelled name registers happily with the event poller and then never fires. The names
follow the standard layout, and physical positions map through the controller database, so `A` is always
the bottom face button whatever is printed on it.

`GamepadStickState` is a readonly struct with `X` (-1 full left, +1 full right), `Y` (-1 full down, +1
full up), `RawX`, `RawY`, `Magnitude`, `Angle` in radians, `IsEngaged(float threshold = 0.15f)`,
`Direction(float threshold = 0.15f)` and `WithDeadzone(float threshold = 0.15f)`. `WithDeadzone` returns
the state unchanged when engaged and a zeroed state - raw values preserved - otherwise. `Direction`
returns `StickDirection.None` below the threshold and otherwise ORs together every axis past it, so a
diagonal reads as `Up | Right`.

For edge-triggered button events, `Engine.Instance.Input.GamepadEventPoller` is nullable - use `?.` -
and assigning a manager to `Engine.Instance.Input.GamepadManager`, which
`InitializeSdlGamepadManager` does, calls `GamepadEventPoller.Initialize` for you. Its surface is
`StartMonitoringButton(gamepadId, button, timeBetweenEvents, isPaused)`, `StopMonitoringButton`,
`StopMonitoringAllButtons`, `AllButtonConfigsByGamepadId`, `PauseAllInput` and the `ButtonDown` event,
whose args carry the `Config` and the `Adapter`.

The `CodeBrix.Platform.GameEngine.Sdl2.Native` namespace - `Sdl2Library`, `Sdl2Native`,
`SDL_GameController`, `SDL_Joystick` and the enums - is public for interop, not for application code. It
is raw P/Invoke, mostly unsafe, it returns sentinel values instead of throwing when SDL2 is absent, and
calling it behind the manager's back can desynchronize the device list or shut the subsystem down under
it. Use `SdlGamepadManager`.

## Examples

Gamepads are one override in either hosting mode - `OnConfigureGamepads` on a `CodeBrixGameHost` (the
scene-pipeline mode, Mode A) and `ConfigureGamepads` on a `SoftwareRenderedGameHostBase` (the
software-rendered mode, Mode B); [The two hosting modes](../CodeBrix.Platform.GameEngine.md#the-two-hosting-modes)
explains both. Reading them is a loop over the live adapter collection.

```csharp
// Mode A
public sealed class MyGameHost : CodeBrixGameHost
{
    private SdlGamepadManager? _gamepads;

    public MyGameHost(GameSurfaceCanvas renderSurface) : base(renderSurface) { }

    protected override void OnConfigureGamepads()
        => _gamepads = Engine.Instance.InitializeSdlGamepadManager();
}

// Mode B
protected override void ConfigureGamepads()
    => _gamepads = Engine.Instance.InitializeSdlGamepadManager();
```

```csharp
private void ReadGamepads(SdlGamepadManager gamepads)
{
    foreach (SdlGamepadAdapter pad in gamepads.ConnectedAdapters)
    {
        GamepadStickState move = pad.LeftStick?.WithDeadzone() ?? default;

        // Clamp before using Magnitude as a speed scalar - see Pitfalls.
        float speed = MathF.Min(move.Magnitude, 1f) * MaxSpeed;
        _playerX += move.X * speed * deltaSeconds;
        _playerY += move.Y * speed * deltaSeconds;   // +Y is UP

        bool firing = pad.PressedButtons.Contains(SdlGamepadButtons.A)
                      || pad.RightTrigger > 0.5f;

        if (move.Direction().HasFlag(StickDirection.Up)) { AimUp(); }
    }
}
```

The clamp on the first line of arithmetic is not decoration: `Magnitude` can exceed 1 on a diagonal, and
using it unclamped as a speed scalar is the classic diagonal-speed-boost bug.

## Pitfalls

- A controller that reconnects gets a new `GamepadId`, because it derives from the joystick instance ID.
  Anything registered against the old id - button monitoring in particular - must be registered again.
  Bluetooth controllers sleep aggressively, so this happens in normal use.
- `GamepadStickState.Magnitude` can exceed 1, up to the square root of 2 on a diagonal, because `X` and
  `Y` are each clamped to the range -1 to 1 independently. Clamp the magnitude, or normalize the vector,
  before using it as a speed scalar.
- A resting stick does not read zero. Use `WithDeadzone()` or `IsEngaged()` rather than testing against
  `!= 0`.
- Stick `Y` is inverted relative to the native API: the native side reports up as negative, while
  `GamepadStickState` defines +1 as up. The adapter handles it; raw values read through the native layer
  do not.
- The first poll after a controller connects is discarded on purpose, because a freshly connected
  Bluetooth pad can report full deflection on every axis until its first report arrives. Sticks and
  triggers read neutral for one frame after connect; buttons are trustworthy immediately.
- Do not call `Math.Abs()` on a raw axis value as a `short`. -32768 is a real reading and negating it
  overflows a signed 16-bit type; widen to `int` first.
- Button and axis numbering is device- and transport-specific - the same pad reports a different raw
  layout over Bluetooth than over USB - so always go through `SdlGamepadButtons` and the adapter, never
  raw joystick indices.
- Use the `SdlGamepadButtons` constants rather than string literals, because the seam is string-keyed and
  a misspelled name registers happily and then never fires.
- Do not cache `ConnectedAdapters` or `PressedButtons`; both are live views reused across refreshes, and
  a copy stops tracking hotplug. Enumerate them rather than materializing a list each frame.
- Never call `IGamepadManager.Update()` yourself, and do not add throttling of your own: the engine
  refreshes gamepad state in both modes and the throttle is part of the seam's contract.
- A kiosk or frame-buffer deployment has a second failure mode: the evdev backend needs read access to
  `/dev/input/event*`, which a desktop login session grants through a per-device ACL but a bare service
  account may not have. That looks identical to "no controller plugged in", and
  `GetNoControllersHint()` tests for it specifically and says to add the account to the `input` group.
- `Engine.Instance.Input.GamepadEventPoller` is nullable; use `?.` rather than assuming it exists.
- Verify any gamepad change in both hosting modes. A check that supplies its own driver cannot detect a
  missing driver.
- `Engine.Dispose()` does not dispose the gamepad manager. A game whose process ends with the engine can
  let the process exit; a game that tears the engine down and keeps running must call
  `gamepads.Dispose()` itself, after `Engine.Dispose()`, and must call `InitializeSdlGamepadManager`
  again on the next engine.

```csharp
public void ShutDown()
{
    Engine.Instance.Dispose();
    _gamepads?.Dispose();
    _gamepads = null;
}
```

## Samples and tools in the repository

No sample in the repository wires up gamepads. The complete games that do are
[BrixInvaders](https://github.com/ellisnet/CodeBrix.Samples/tree/main/BrixInvaders) in
[CodeBrix.Samples](https://github.com/ellisnet/CodeBrix.Samples), which reads the keyboard and every gamepad
together through an `InputActionMap`, and [Doom.Brix](https://github.com/ellisnet/CodeBrix.Samples.Gpl2/tree/main/Doom.Brix)
and [Wolfenstein.Brix](https://github.com/ellisnet/CodeBrix.Samples.Gpl2/tree/main/Wolfenstein.Brix) in
[CodeBrix.Samples.Gpl2](https://github.com/ellisnet/CodeBrix.Samples.Gpl2), which add optional gamepad support
that degrades to keyboard only, in the software-rendered mode. Their recipes are in the
[game-engine blueprints](../../samples/blueprints.md#hosting-a-game-engine).

| Name | What it demonstrates | Where |
| --- | --- | --- |
| padcheck | An interactive hardware check that drives the real `SdlGamepadManager`, so what it prints is what a game would see | [`tools/padcheck`](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/tree/main/tools/padcheck) |
| sdl2_library_building | The hand-run script that builds the Windows-on-ARM64 SDL2 binary the package ships, verifying the source against a pinned hash and writing a provenance file beside the output | [`tools/sdl2_library_building`](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/tree/main/tools/sdl2_library_building) |

No sample wires up gamepads, because the thing that has to be checked is physical hardware rather than
an on-screen result. `padcheck` covers what unit tests cannot: whether each physical button reports
under the correct name, whether pushing a stick up reports `Up`, whether the left trigger reports as
left, and whether a controller that sleeps and wakes is picked back up. Run both of its drive modes
every time - the direct one and the `InputPump` path that the software-rendered mode uses.

```bash
cd tools/padcheck
dotnet run -- 45           # direct mode; seconds to poll, default 30
dotnet run -- 45 --pump    # the InputPump path
```

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide for the gamepad package (ships inside the package too) | [src/CodeBrix.Platform.GameEngine.Sdl2/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/blob/main/src/CodeBrix.Platform.GameEngine.Sdl2/AGENT-README.txt) |
| Gamepad tests | [tests/CodeBrix.Platform.GameEngine.Sdl2.Tests](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/tree/main/tests/CodeBrix.Platform.GameEngine.Sdl2.Tests) |
| The hardware check and its button and axis tables | [tools/padcheck/README.md](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/blob/main/tools/padcheck/README.md) |

## License

The gamepad package (`CodeBrix.Platform.GameEngine.Sdl2.ZlibLicenseForever`) is licensed `MIT AND Zlib`,
because it redistributes the SDL2 native libraries: the managed binding code is MIT and the SDL2 native
binaries are zlib, and the suffix names the more notice-demanding of the two. For the provenance of
both, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/blob/main/THIRD-PARTY-NOTICES.txt),
which also ships inside the package.

---

**Where to go next**

- [CodeBrix.Platform.GameEngine](../CodeBrix.Platform.GameEngine.md) - the engine, its input pollers and `InputActionMap`
- [Kenney assets](kenney-assets.md) - the add-on that loads art, sound and fonts from downloaded Kenney zips
- [Generated music](generated-music.md) - the add-on that plays endless generated music on the music bus
- [Cards and dice](cards-and-dice.md) - the add-on for card, dice and board-style games
