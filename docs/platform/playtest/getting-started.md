<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Testing your application](../10-testing-your-application.md) › [PlayTest](README.md) › Getting started</sub>

# Getting started with PlayTest

**By the end of this page you will have a `.PlayTests` project beside your application that builds, launches the real application offscreen once, gives every test a fresh page, and runs headless or with a live preview.** The [hub page](README.md) shows the complete minimal example in one place; this page explains each piece and the choices behind it.

## Requirements

- .NET 10 or later, like the rest of the family.
- An **executable** test project (`OutputType` `Exe`) - the norm for xUnit v3 and Microsoft Testing Platform projects.
- The **Microsoft Testing Platform** runner, selected by `global.json`, for the command-line switches.
- The **reflection-based xUnit v3 runner** for `--screenshotfolder` recording (assembly fixtures and test-lifecycle notifications).
- Nothing on the machine for headless runs: no display server, no GPU. A headed run needs a desktop session - Windows, macOS, or Linux on X11 or Wayland.

## The project file

The PlayTests project is a head in disguise. It imports the application's `.UI` shared project, so `App.xaml`, `App.xaml.cs` and every view compile into it, and it references `.Core` for the view models, services and the framework and add-in packages. Then it references the PlayTest package where a head would reference its one head package.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <RootNamespace>MyApp.PlayTests</RootNamespace>
    <IsPackable>false</IsPackable>
    <UseMicrosoftTestingPlatformRunner>true</UseMicrosoftTestingPlatformRunner>
    <TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>
    <Nullable>disable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
    <CodeBrixPlayTestPreferredOrientation>Landscape</CodeBrixPlayTestPreferredOrientation>
    <CodeBrixPlayTestPreferredTheme>Light</CodeBrixPlayTestPreferredTheme>
  </PropertyGroup>
  <Import Project="../../src/MyApp.UI/MyApp.UI.projitems" Label="Shared" />
  <ItemGroup>
    <ProjectReference Include="../../src/MyApp.Core/MyApp.Core.csproj" />
    <PackageReference Include="CodeBrix.Platform.PlayTest.ApacheLicenseForever" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit.v3" />
    <PackageReference Include="xunit.runner.visualstudio" PrivateAssets="all" />
    <PackageReference Include="SilverAssertions.ApacheLicenseForever" />
  </ItemGroup>
</Project>
```

Notice what each part is for.

- **`Nullable` and `ImplicitUsings`** must match the application's own projects, because the application's shared sources compile here. The CodeBrix.Platform applications have both off.
- **`CodeBrixPlayTestPreferredOrientation`** (`Landscape` or `Portrait`) and **`CodeBrixPlayTestPreferredTheme`** (`Light` or `Dark`) are optional project preferences. The build validates them and records them as assembly metadata; the command line and the environment override them. Leave them out to get Landscape and Light.
- **The PlayTest package's build logic** makes the project a PlayTest head, defines the `HAS_CODEBRIX`, `HAS_CODEBRIX_WINUI`, `HAS_CODEBRIX_SKIA` and `HAS_CODEBRIX_PLAYTEST` compilation constants, and compiles two small runner adapters into the project: one registers the command-line switches with the Microsoft Testing Platform, the other records screenshots under xUnit v3. There is nothing to wire up.
- **Packages carry no `Version` attribute.** Use the same family version as the application's other CodeBrix.Platform packages.

The package brings its own dependencies: the framework package, the shared Skia runtime, SkiaSharp and HarfBuzzSharp with their Linux native assets, the ICU data packages for Windows and macOS, and an SDL3 binding that only the headed preview window uses.

> [!IMPORTANT]
> Never add a desktop head package (`CodeBrix.Platform.Runtime.Skia.*`) to the PlayTests project, and never reference the PlayTest package from an application or head project. The PlayTests project is its own head.

When the application uses an add-in, it already arrives through `.Core`. When the application uses the Graphics3DGL add-in, add `CodeBrix.Platform.PlayTest.OpenGL.ApacheLicenseForever` beside the PlayTest package - see [OpenGL](opengl.md).

## global.json

```json
{
    "test": {
        "runner": "Microsoft.Testing.Platform"
    }
}
```

It sits at the application root and selects the runner for every test project below it - the same file [10 - Testing your application](../10-testing-your-application.md) uses for the unit tests.

## The fixture

One application runs per process. A second `LaunchAsync` in the same process throws `InvalidOperationException`, so the fixture launches once, shares the application with every test class, and disposes it when the run ends.

```csharp
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using MyApp.Views;
using Xunit;

[assembly: Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.None)]

namespace MyApp.PlayTests;

public sealed class AppFixture : IAsyncLifetime
{
    public PlayTestApplication Application { get; private set; }

    public async ValueTask InitializeAsync() =>
        Application = await PlayTestApplication.LaunchAsync(() => new App(), new()
        {
            ConfigurationAssembly = typeof(AppFixture).Assembly,
        });

    public Task ResetAsync(ScreenOrientation? orientation = null)
    {
        Application.FilePickers.Clear();
        Application.Launcher.Clear();
        return Application.Page.SetContentAsync(() => new MainPage(), orientation);
    }

    public async ValueTask DisposeAsync()
    {
        if (Application != null) await Application.DisposeAsync();
    }
}

[CollectionDefinition("App")]
public sealed class AppCollection : ICollectionFixture<AppFixture> { }
```

Four lines in it carry the weight.

**The parallelization attribute.** The application, its singletons and the resolved preferences are per process, so the whole assembly runs serially.

**`PlayTestApplication.LaunchAsync(() => new App(), options)`** resolves and validates every preference once, starts the application on its own UI thread, and waits for the first rendered frame. The factory constructs your real `App`, so its constructor runs exactly as it does on a desktop head - fonts, theme, dependency injection - and its `OnLaunched` creates the window. A headed run also starts the preview process here.

**`ConfigurationAssembly = typeof(AppFixture).Assembly`** tells PlayTest where the project preferences were recorded. Under some runners the entry assembly is not the test assembly, and without this line the project's preferred orientation and theme are silently ignored.

**`Page.SetContentAsync(() => new MainPage(), orientation)`** installs a fresh page for each test. It closes open popups, disposes the previous page's disposable `DataContext` (also when the page is hosted in a `Frame`), applies the orientation *before* the factory runs, clears the isolated clipboard, releases any keys still held, restores a minimized or closed window, and waits for layout and rendering. It does not clear the picker queues or the launcher history - the fixture clears those itself.

> [!WARNING]
> Do not assign `PlayTestOptions.Headless`, `Theme`, `Orientation` or `SlowMo` in the fixture "just in case". An assigned value - even the default - beats the command line and the environment, so `--headed` or `--theme=dark` then stops working. Leave them unset unless the test project must always run one way.

### PlayTestOptions

| Property | Default | Meaning |
| --- | --- | --- |
| `ConfigurationAssembly` | the entry assembly | Where the project preferences are read from; set it to your fixture's assembly |
| `Orientation` | resolved | Explicit launch orientation, `ScreenOrientation.Landscape` or `Portrait` |
| `Theme` | resolved | Explicit simulated OS theme, `ApplicationTheme.Light` or `Dark` |
| `Headless` | resolved | Explicit preview choice |
| `SlowMo` | resolved | Delay after each action in milliseconds |
| `Timeout` | 10000 | Default action and assertion timeout in milliseconds |
| `ArtifactsDirectory` | `TestResults/PlayTest` | Where failure screenshots go |
| `OpenGL` | `PlayTestOpenGL.Available` | `PlayTestOpenGL.Unavailable` launches with no OpenGL - see [OpenGL](opengl.md) |

`ResolveOrientation()` and `ResolveTheme()` return what the launch will use. "Resolved" means the value comes from the command line, the environment or the project, in that order - see [Running, switches and the preview](running-and-switches.md).

## The test class

`PageTest` is an optional, runner-neutral base class that supplies `Page` and a protected `Expect(locator)`. A test class derives from it, joins the fixture's collection, and resets the page in its own `InitializeAsync`:

```csharp
[Collection("App")]
public sealed class SendTests(AppFixture fixture) : PageTest(fixture.Application), IAsyncLifetime
{
    public async ValueTask InitializeAsync()
    {
        var test = (Xunit.v3.IXunitTest)TestContext.Current.Test;
        test.Traits.TryGetValue(PlayTestOrientationAttribute.CaseTraitName, out var rows);
        await fixture.ResetAsync(PlayTestOrientationAttribute.Resolve(test.TestMethod.Method, rows));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Sending_shows_the_status()
    {
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Message" }).FillAsync("Hello");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Send", Exact = true }).ClickAsync();
        await Expect(Page.GetByTestId("status")).ToHaveTextAsync("Sent");
    }
}
```

The three lines in `InitializeAsync` resolve an optional orientation requirement for the test that is about to run: a `[PlayTestOrientation(...)]` attribute on the method, or a theory row's `PlayTestOrientation` trait. `PlayTestOrientationAttribute.Resolve` returns the row's value, else the method's, else `null`, which means "the launch preference". Read the traits of the *executing* test from `TestContext.Current.Test` as shown - not the discovery test case's, which may group deferred data rows. The attribute installs no hooks of its own; this call is what applies it. [Pickers, clipboard, theme and orientation](pickers-clipboard-theme-orientation.md#orientation) has the attribute in full.

A class fixture (`IClassFixture<AppFixture>`) works as well as a collection when the suite has one test class or when each class should be grouped on its own - the [samples' shared base class](patterns-from-the-samples.md#a-shared-fixture-base-class) uses `IClassFixture`. Either way there is still only one application per process.

### Usings

```csharp
using CodeBrix.Platform.PlayTest;      // everything a test uses
using Microsoft.UI.Xaml;               // ApplicationTheme, UIElement
using System.Text.RegularExpressions;  // the Regex overloads
```

Add `using CodeBrix.Platform.PlayTest.OpenGL;` in the fixture when you register the OpenGL provider.

## Running

```bash
dotnet test --project tests/MyApp.PlayTests/MyApp.PlayTests.csproj
dotnet test --project tests/MyApp.PlayTests/MyApp.PlayTests.csproj -c Release --headed
```

Headless is the default and needs no display. `--headed` opens a view-only preview with a 250 ms pause after each action so that a person can follow along. The project can also be added to the application's solution, and runs with the rest of its tests - but pass PlayTest switches to PlayTest projects only, because other test projects in the solution may reject an unknown switch.

A failed action or assertion throws `PlayTestException`; its message carries a description of the visible UI and the path of a failure screenshot under `TestResults/PlayTest`.

> [!TIP]
> When `dotnet test` reports "Zero tests ran", the cause can be an option error it hides. Run the built test program directly - `dotnet exec bin/Release/net10.0/MyApp.PlayTests.dll` with the same switches - to see the diagnostic.

## Checklist

- [ ] The PlayTests project is `Exe`, imports the `.UI` `.projitems`, references `.Core`, and references the PlayTest package with no `Version` and no head package
- [ ] `Nullable` and `ImplicitUsings` match the application's projects
- [ ] `global.json` selects the Microsoft Testing Platform runner
- [ ] The assembly is serialized with `Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.None)`
- [ ] One fixture launches the application once, with `ConfigurationAssembly = typeof(AppFixture).Assembly`, and disposes it
- [ ] Each test resets the page through `SetContentAsync` and clears the picker queues and launcher history
- [ ] The fixture leaves `Headless`, `Theme`, `Orientation` and `SlowMo` unset

---

**Where to go next**

- [Locators and actions](locators-and-actions.md) - finding elements and driving them
- [Patterns from the samples](patterns-from-the-samples.md) - replacing services, launch-once fixtures and a reusable fixture base
- [PlayTest](README.md) - the hub page and the complete minimal example
