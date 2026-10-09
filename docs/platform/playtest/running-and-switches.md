<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Testing your application](../10-testing-your-application.md) › [PlayTest](README.md) › Running, switches and the preview</sub>

# Running, switches and the preview

**A PlayTests project runs headless by default; four preferences - preview mode, theme, orientation and action delay - can each be chosen from code, the command line, the environment or the project file, and one switch records the run.** Each preference is resolved once, at launch, and a test can read what was chosen from `PlayTestApplication`.

## Running

```bash
dotnet test --project tests/MyApp.PlayTests/MyApp.PlayTests.csproj
dotnet test --project tests/MyApp.PlayTests/MyApp.PlayTests.csproj -c Release
```

The project can also sit in the application's solution and run with its other tests. Run headed projects one at a time so that one preview is visible at a time.

## The command-line switches

With the Microsoft Testing Platform runner, the package registers these switches automatically - no project property or fixture code is involved:

| Switch | Effect |
| --- | --- |
| `--headed` | Open the live preview window |
| `--nonheadless` | Alias for `--headed` |
| `--headless` | No preview (the default) |
| `--theme=light` or `--theme=dark` | The simulated operating-system theme |
| `--orientation=landscape` or `--orientation=portrait` | The launch orientation |
| `--screenshotfolder=<empty folder>` | Record every step of every test - see [Screenshots and recording](screenshots-and-recording.md#recording-a-whole-run) |

```bash
dotnet test --project tests/MyApp.PlayTests/MyApp.PlayTests.csproj --headed
dotnet test --project tests/MyApp.PlayTests/MyApp.PlayTests.csproj --nonheadless --theme=dark --orientation=portrait
dotnet test --project tests/MyApp.PlayTests/MyApp.PlayTests.csproj --screenshotfolder="$HOME/Temp/run-1"
```

- Values are case-insensitive, and `--theme=dark` and `--theme dark` both work.
- Combining `--headless` with `--headed` or `--nonheadless` is rejected before any test runs.
- The switches appear in the test program's `--help`.
- The generated xUnit, MSTest and NUnit entry points register them. A custom Microsoft Testing Platform entry point must call its generated `AddSelfRegisteredExtensions(builder, args)`.
- When running a whole solution, other test projects may reject an unknown switch - pass the switches to PlayTest projects only.
- Runners in VSTest mode do not get the switches; use the environment variables and fixture options there.

## The environment variables

| Variable | Values |
| --- | --- |
| `CODEBRIX_PLAYTEST_HEADED` | `1` opens the preview; any other value means headless |
| `CODEBRIX_PLAYTEST_THEME` | `light` or `dark` |
| `CODEBRIX_PLAYTEST_ORIENTATION` | `landscape` or `portrait` |
| `CODEBRIX_PLAYTEST_SLOWMO` | Milliseconds to pause after each action, `0` for full speed |

```bash
CODEBRIX_PLAYTEST_HEADED=1 CODEBRIX_PLAYTEST_ORIENTATION=portrait CODEBRIX_PLAYTEST_THEME=dark \
  dotnet test --project tests/MyApp.PlayTests/MyApp.PlayTests.csproj -c Release
```

An unset or empty variable falls through to the next level. `CODEBRIX_PLAYTEST_SLOWMO` uses `.` as the decimal separator and must be finite and not negative.

## The project preferences

```xml
<PropertyGroup>
  <CodeBrixPlayTestPreferredOrientation>Portrait</CodeBrixPlayTestPreferredOrientation>
  <CodeBrixPlayTestPreferredTheme>Dark</CodeBrixPlayTestPreferredTheme>
</PropertyGroup>
```

The build accepts only `Landscape` or `Portrait` and `Light` or `Dark`, and records them as assembly metadata. That is why the fixture sets `ConfigurationAssembly = typeof(AppFixture).Assembly`: a runner may use a different entry assembly, and the preferences are read from the assembly named there. The properties need `GenerateAssemblyInfo`, which is on by default.

## Preferences and precedence

Each preference is resolved once at launch, highest priority first:

| Preference | Code | Command line | Environment | Project | Default |
| --- | --- | --- | --- | --- | --- |
| Orientation | `PlayTestOptions.Orientation` | `--orientation` | `CODEBRIX_PLAYTEST_ORIENTATION` | `<CodeBrixPlayTestPreferredOrientation>` | Landscape |
| Theme | `PlayTestOptions.Theme` | `--theme` | `CODEBRIX_PLAYTEST_THEME` | `<CodeBrixPlayTestPreferredTheme>` | Light |
| Preview | `PlayTestOptions.Headless` | `--headed`, `--nonheadless`, `--headless` | `CODEBRIX_PLAYTEST_HEADED` | - | Headless |
| Action delay | `PlayTestOptions.SlowMo` | - | `CODEBRIX_PLAYTEST_SLOWMO` | - | 250 ms headed, 0 headless |

- **An assigned option wins** - even one assigned its default value, and `SlowMo = 0` explicitly. Leave the options unset in the fixture so the command line and environment stay in control.
- **The action delay follows the final preview choice**: with no explicit value, a headed run pauses 250 ms after each action so a person can follow it, and a headless run does not pause at all.
- **An invalid selected value** fails the launch with an `ArgumentException` that names where the value came from.
- **Per-test orientation requirements** (`[PlayTestOrientation]`, `SetContentAsync(..., orientation)`) and an application's own `RequestedTheme` still win over the resolved launch values, because they apply after launch.

A test reads the resolved values from `PlayTestApplication`: `Headless`, `SystemTheme`, `PreferredOrientation`, `Orientation`, `Width` and `Height`. `new PlayTestOptions { ConfigurationAssembly = ... }.ResolveTheme()` and `ResolveOrientation()` compute what a launch would choose without launching.

## The headed preview

`--headed`, `--nonheadless` or `CODEBRIX_PLAYTEST_HEADED=1` opens a live, resizable, **view-only** preview window in a separate process - on Windows, macOS, and Linux on X11 or Wayland.

- All physical keyboard, pointer, wheel and touch input to the preview is discarded. Tests drive the application; a person only watches.
- Closing the preview does not stop the tests.
- The preview's proportions follow the launch orientation and never change when a test switches orientation: opposite-orientation frames are letterboxed, with bars left and right or top and bottom. The application still sees exactly 1920 x 1080 or 1080 x 1920, whatever size the preview is dragged to.
- Screenshots contain the virtual screen itself, never the preview or its bars.
- The default 250 ms action delay makes a run watchable. Set `CODEBRIX_PLAYTEST_SLOWMO=0` to watch at full speed, or a larger value to slow it further.

## Running on a build machine

Headless runs need no display server, GPU or desktop session on any operating system, and render identical pixels everywhere, so a PlayTests project runs on a build agent like any other test project. Run it headless, give `--screenshotfolder` a fresh empty folder when a person will review the run, and collect `TestResults/PlayTest` for the failure screenshots. Two exceptions need more than a headless machine: the [OpenGL provider](opengl.md#what-each-operating-system-needs) on Windows and macOS, and the WebView add-in's browser engines.

To cover both themes and both orientations, run the project once per combination rather than switching inside a run - application startup sees only the launch values. The CodeBrix.Samples repository's runners do exactly this, running every PlayTests project in headless light landscape and portrait, then headed dark landscape and portrait, and requiring at least one passing test and zero failures or skips from each run.

## Performance

- Launch one application per test process and reset the page per test; never relaunch.
- Prefer assertions and `WaitForAsync` to fixed delays - they return as soon as the condition holds. Never `Task.Delay` to let the UI settle.
- Leave `SlowMo` unset: headless runs then have no action delay.
- Prefer `GetByTestId`, the cheapest and least ambiguous locator.
- Record with `--screenshotfolder` only for runs you will inspect.

---

**Where to go next**

- [Patterns from the samples](patterns-from-the-samples.md) - fixtures for real applications
- [Screenshots and recording](screenshots-and-recording.md) - what `--screenshotfolder` produces
- [PlayTest](README.md) - the hub page
