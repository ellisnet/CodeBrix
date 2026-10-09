<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Testing your application](../10-testing-your-application.md) › [PlayTest](README.md) › Patterns from the samples</sub>

# Patterns from the samples

**Every application in [CodeBrix.Samples](https://github.com/ellisnet/CodeBrix.Samples) has a `tests/<App>.PlayTests` project, and together they settle how a real application is made testable: services are replaced where the application already builds them, one fixture base class serves most suites, settings live in a throwaway folder, and an application built around a process-wide engine is launched once and reset in place.** Each recipe below names the file it came from.

## Replace services at the DI boundary

A PlayTest drives the application's real pages and view models, so what must not run for real - a network API, a camera, a browser, a music model - is replaced where the application already registers its services, before any view model is constructed. The application gives its `App` one extra constructor that hands the service collection to its host:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/App.xaml.cs
public App() : this(null) { }

// Alternate hosts may replace external services before any view models are constructed.
public App(Action<IServiceCollection> configureServices, string settingsDirectory = null)
{
    // ... fonts ...

    SimpleServiceResolver.CreateInstance(HostHelper.GetHost(), services =>
    {
        //Register the app's services here
        services.AddGitHubIssueSearch(new GitHubSearchOptions());
        configureServices?.Invoke(services);
    });
    SimpleViewModel.SetIsDesignMode(false);

    if (settingsDirectory == null) SettingsService.Initialize();
    else SettingsService.Initialize(settingsDirectory);

    // ... theme, launch size, InitializeComponent() ...
}
```

Notice the order. The application's own registrations go first and the caller's after, so a later `AddSingleton` of the same service type is the one view models receive. The desktop heads call `new App()` and get the real services; nothing about the shipping application changes.

The fixture then launches with fakes in place:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/tests/GitHubIssueFinder.PlayTests/AppFixture.cs
public sealed class AppFixture : SampleFixture<MainPage>
{
    public SearchFixture Search { get; } = new();
    public UrlOpenerFixture Links { get; } = new();
    public MainViewModel Model => (MainViewModel)View.DataContext;
    protected override Application CreateApplication() => new App(
        services => services.AddSingleton<IGitHubIssueSearchService>(Search).AddSingleton<IUrlOpener>(Links),
        Path.Combine(DataDirectory, "settings"));
    // ...
}

public sealed class UrlOpenerFixture : IUrlOpener
{
    public List<Uri> Opened { get; } = new();

    public Task<bool> OpenAsync(Uri uri)
    {
        Opened.Add(uri);
        return Task.FromResult(true);
    }
}
```

The fake search service is scripted per test - pages to return, an error to throw, progress to report, a point at which to hold until the test releases it - and resets in the fixture's `BeforeResetAsync`. That makes every state of the page reachable: empty, failed, rate-limited, streaming, cancelled.

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/tests/GitHubIssueFinder.PlayTests/ApplicationTests.cs
[Fact]
public async Task Service_error_is_reported_and_search_can_be_retried()
{
    Fixture.Search.Fail = true;
    await Page.GetByTestId("Owner").FillAsync("sample");
    await Button("Search").ClickAsync();
    await Expect(Page.GetByText("Fixture service unavailable")).ToBeVisibleAsync();
    await Expect(Button("Search")).ToBeEnabledAsync();
    Fixture.Search.Fail = false;
    await Button("Search").ClickAsync();
    await Expect(Page.GetByText("Improve keyboard navigation", new() { Exact = true })).ToBeVisibleAsync();
}
```

> [!TIP]
> Replace services at the boundary the application already has rather than adding test hooks to view models. The suites that do this - GitHubIssueFinder, PolyHavenBrowser, BrixInvaders, PalmVisualizer, WebcamPainter and others - test the real view models against fake *services*, which is exactly the seam [05 - MVVM the right way](../05-mvvm-the-right-way.md) already puts in place.

## A shared fixture base class

Most of the sample suites share one generic fixture and test base, linked into each project from `PlayTestSupport/SampleFixture.cs`. It is a good starting point to copy into your own repository.

```csharp
// From CodeBrix.Samples/PlayTestSupport/SampleFixture.cs
public abstract class SampleFixture<TPage> : IAsyncLifetime where TPage : FrameworkElement, new()
{
    public PlayTestApplication Application { get; private set; }
    public TPage View { get; private set; }
    public string DataDirectory { get; } = Path.Combine(AppContext.BaseDirectory, "TestResults", "PlayTestData", Guid.NewGuid().ToString("N"));
    protected abstract Application CreateApplication();
    protected virtual void Prepare() { }
    // Adjusts the launch options (for example PlayTestOptions.OpenGL) before the application starts.
    protected virtual void Configure(PlayTestOptions options) { }
    protected virtual Task BeforeResetAsync() => Task.CompletedTask;
    protected virtual Task AfterResetAsync() => Task.CompletedTask;
    protected virtual void Cleanup() { }

    public async ValueTask InitializeAsync()
    {
        Directory.CreateDirectory(DataDirectory);
        Prepare();
        var options = new PlayTestOptions { ConfigurationAssembly = GetType().Assembly };
        Configure(options);
        Application = await PlayTestApplication.LaunchAsync(CreateApplication, options);
    }

    public async Task ResetAsync(ScreenOrientation? orientation = null)
    {
        await BeforeResetAsync();
        Application.FilePickers.Clear();
        await Application.Page.SetContentAsync(() => View = new TPage(), orientation);
        await AfterResetAsync();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (Application != null)
            {
                try { await Application.EvaluateAsync(() => (View?.DataContext as IDisposable)?.Dispose()); }
                finally { await Application.DisposeAsync(); }
            }
        }
        finally { Cleanup(); }
    }
}

public abstract class SampleTest<TFixture, TPage> : PageTest, IClassFixture<TFixture>, IAsyncLifetime
    where TFixture : SampleFixture<TPage>
    where TPage : FrameworkElement, new()
{
    protected TFixture Fixture { get; }
    protected SampleTest(TFixture fixture) : base(fixture.Application) => Fixture = fixture;

    public async ValueTask InitializeAsync()
    {
        var test = (Xunit.v3.IXunitTest)TestContext.Current.Test;
        test.Traits.TryGetValue(PlayTestOrientationAttribute.CaseTraitName, out var orientations);
        await Fixture.ResetAsync(PlayTestOrientationAttribute.Resolve(test.TestMethod.Method, orientations));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    protected Locator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });
    protected Task SnapshotAsync(string name) => Page.ScreenshotAsync(new()
    {
        Path = Path.Combine("TestResults", "PlayTest", name + ".png"),
    });
}
```

Notice the hooks. `Prepare` runs before launch (start a loopback server, write fixture data), `Configure` adjusts the launch options, `BeforeResetAsync` resets fakes before each fresh page, `AfterResetAsync` waits for the page's own startup work, and `Cleanup` runs after the application is gone. `View` is the page the fixture built, so tests can read its view model through `EvaluateAsync`. With all that in the base, an application with no external services needs a four-line fixture:

```csharp
// From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/tests/PainDiagram.PlayTests/AppFixture.cs
public sealed class AppFixture : SampleFixture<MainPage>
{
    public MainViewModel Model => (MainViewModel)View.DataContext;
    public DrawingSession Session => Model.Session;
    public DrawingCanvas Canvas => (DrawingCanvas)View.FindName("DrawCanvas");
    protected override Application CreateApplication() => new App();
}

public sealed partial class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    // ...
}
```

The samples share the project settings the same way, through one imported props file that sets `OutputType`, the runner properties, `Nullable` and `ImplicitUsings` off, the preference defaults, the four test packages and the linked fixture file - see [PlayTestSupport](https://github.com/ellisnet/CodeBrix.Samples/tree/main/PlayTestSupport). Each project then adds only its `RootNamespace`, the `.UI` import, the `.Core` reference and its own SilverAssertions reference.

## Keep settings and data in a throwaway folder

A test must never read or write the settings, recent files or high scores of the person running it. The applications take a settings directory in the same `App` constructor (above), and the fixture passes a folder under `DataDirectory`, a fresh GUID-named folder beside the test binary. Where the application reads its store before the `App` constructor runs, the fixture opens the store itself, before launch, and seeds it:

```csharp
// From CodeBrix.Samples/BrixInvaders/tests/BrixInvaders.PlayTests/AppFixture.cs
// A throwaway store, opened before the app opens the player's own (which then does nothing).
Directory.CreateDirectory(SettingsDirectory);
SettingsService.Initialize(SettingsDirectory);
SettingsService.Difficulty = SeededDifficulty;
SettingsService.MasterVolume = SeededMasterVolume;
var table = new HighScoreTable();
table.Insert(SeededDifficulty, SeededName, SeededScore, SeededSector);
SettingsService.SaveHighScores(table, SeededDifficulty);
```

Seeding values that are *not* the defaults lets a test prove the application really read them. Close the store in `Cleanup` or `DisposeAsync`, after the application is disposed.

## Launch once for a process-wide engine

`SetContentAsync` builds a fresh page per test, which is right for most applications. It is wrong for one whose page starts something process-wide exactly once - a game engine started from the canvas's first layout, an editor core the page subscribes to from its first `Loaded`. A second page would find the engine already running and never attach. Those suites launch once, keep the page the application navigated to for the whole run, and reset the *live* page before each test instead.

```csharp
// Adapted from CodeBrix.Samples/BrixInvaders/tests/BrixInvaders.PlayTests/AppFixture.cs
public async ValueTask InitializeAsync()
{
    // ... throwaway settings store, seeded (above) ...
    Application = await PlayTestApplication.LaunchAsync(() => new App(services => services
        .AddSingleton<IMusicDirector>(Music)
        .AddSingleton<IExternalLinkOpener>(Links)), new()
    {
        ConfigurationAssembly = typeof(AppFixture).Assembly,
    });
    // The page the application navigated to in OnLaunched - kept for the whole run.
    View = await Application.EvaluateAsync(() => (MainPage)((Frame)Application.Window.Content).Content);
    await Application.WaitForAsync(() => Session, session => session != null, description: "the started game session");
}

public async Task ResetAsync(ScreenOrientation? orientation)
{
    // No new page: change the orientation of the live one, and bring the game back to a known screen.
    await Application.SetOrientationAsync(orientation);
    if (await ReadAsync(() => Session.CurrentScreen) == GameScreen.Attract)
        await HoldClickAsync(() => Session.CurrentScreen, screen => screen == GameScreen.Title, "the title after a click");
    await WaitForScreenAsync(GameScreen.Title, (float)(ScreenStateMachine.SplashMaxSeconds + 5) * 1000);
    // A click on the canvas leaves it without keyboard focus; activating the window gives it back.
    await Application.EvaluateAsync(() => GameCanvas.Focus(FocusState.Programmatic));
}

public async ValueTask DisposeAsync()
{
    if (Application != null)
    {
        // The engine thread reads the settings every step: stop it before the store closes.
        await Task.Run(() => Engine.Instance.StopAndWait());
        await Application.DisposeAsync();
    }
    SettingsService.Shutdown();
}
```

Notice four things.

- **`Application.Window`** is the window the application created in `OnLaunched`; its `Content` is the `Frame` from [03 - Your first application](../03-your-first-application.md), and the `Frame`'s `Content` is the page. Read it inside `EvaluateAsync`. (The BrixInvaders source reaches the window through a small `App` subclass instead, because its `App` keeps `MainWindow` protected; `PlayTestApplication.Window` makes the subclass unnecessary.)
- **The reset puts the application back in a known state** rather than building a new page: orientation through `SetOrientationAsync`, the game back on its title screen, focus restored.
- **Anything the reset does not undo, the test must.** `SetContentAsync` is what releases held keys and clears the clipboard, so a launch-once suite releases its own keys and mouse buttons in `finally` blocks, and the Pinta.Brix suite clears the clipboard, closes every document and answers any dialog a failed test left open in its own reset.
- **Disposal stops the engine first**, then disposes the application, then closes the store the engine was reading.

Pinta.Brix, GameEngineMusicDemo and BrixInvaders use this pattern. Each one's `README.md` explains what its reset restores.

## Games and canvases

A game samples its input once per engine cycle, so a key or button that goes down and up within one UI-thread step is usually never seen. Hold it until the game has seen it:

```csharp
// From CodeBrix.Samples/BrixInvaders/tests/BrixInvaders.PlayTests/AppFixture.cs
// The engine samples the mouse button once per engine cycle, so the button stays down until the game has seen it.
public async Task HoldClickAsync<T>(Func<T> probe, Func<T, bool> ready, string description)
{
    var box = await Canvas.BoundingBoxAsync();
    await Application.Page.Mouse.MoveAsync(box.X + (box.Width / 2), box.Y + (box.Height / 2));
    await Application.Page.Mouse.DownAsync();
    try { await WaitAsync(probe, ready, description); }
    finally { await Application.Page.Mouse.UpAsync(); }
}
```

The keyboard equivalent is `Keyboard.DownAsync` and `UpAsync`, or `PressAsync` with a `Delay` - see [Held keys](locators-and-actions.md#held-keys). To prove key events reach the element the game listens on, count them with a handler attached through `EvaluateAsync`:

```csharp
// From CodeBrix.Samples/BrixInvaders/tests/BrixInvaders.PlayTests/ApplicationTests.cs
var keys = 0;
var canvas = await Page.EvaluateAsync(() => _fixture.GameCanvas);
var handler = new KeyEventHandler((_, _) => keys++);
await Page.EvaluateAsync(() => canvas.AddHandler(UIElement.KeyDownEvent, handler, true));
try
{
    await Page.Keyboard.PressAsync("W");
    (await Page.EvaluateAsync(() => keys)).Should().Be(1);
}
finally
{
    await Page.EvaluateAsync(() => canvas.RemoveHandler(UIElement.KeyDownEvent, handler));
}
```

A custom surface with no automation peer is found with `GetByType<T>()` - `Page.GetByType<GameSurfaceCanvas>()`, `Page.GetByType<DrawingCanvas>()` - and an engine that presents frames on its own clock is checked by taking screenshots until one shows what is expected, with a bounded number of attempts.

## Test the no-OpenGL path in its own project

OpenGL is a launch option, fixed for the whole run. An application whose 3D view has a fallback for machines without OpenGL tests the fallback in a process that launches with `PlayTestOptions.OpenGL = PlayTestOpenGL.Unavailable` - a second PlayTests project, or a suite that only covers the fallback. In the shared base class that is one override:

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/tests/PolyHavenBrowser.PlayTests/AppFixture.cs
// The application initializes OpenGL elements. These tests cover the no-OpenGL fallback, so the launch opts out;
// to give the elements real contexts instead, call CodeBrixPlayTestOpenGL.Register() in Prepare() and drop this override.
protected override void Configure(PlayTestOptions options) => options.OpenGL = PlayTestOpenGL.Unavailable;
```

[OpenGL](opengl.md) covers both sides.

## The sample suites

Each suite's `README.md` lists what it covers and how its fakes work. A few to read first:

| Suite | Read it for |
| --- | --- |
| [GitHubIssueFinder.PlayTests](https://github.com/ellisnet/CodeBrix.Samples/tree/main/GitHubIssueFinder/tests/GitHubIssueFinder.PlayTests) | A scripted service with pages, errors and a hold; a recording link opener; settings restore; theme and portrait scrolling |
| [PainDiagram.PlayTests](https://github.com/ellisnet/CodeBrix.Samples/tree/main/PainDiagram/CodeBrixPlatform/tests/PainDiagram.PlayTests) | Real-pointer drawing, confirmation dialogs, the save picker with replace, cancel and failure, pixels on screen and in the saved file |
| [JustBetweenUs.PlayTests](https://github.com/ellisnet/CodeBrix.Samples/tree/main/JustBetweenUs/CodeBrixPlatform/tests/JustBetweenUs.PlayTests) | Its own fixture and project file without the shared props; keyboard editing, the isolated clipboard, orientation changes on a live page, the simulated theme reaching system colors |
| [Pinta.Brix.PlayTests](https://github.com/ellisnet/CodeBrix.Samples/tree/main/Pinta.Brix/tests/Pinta.Brix.PlayTests) | Launch-once over process-wide editor state; menus, shortcuts, layers, effect dialogs, clipboard images, selections and a splitter |
| [BrixInvaders.PlayTests](https://github.com/ellisnet/CodeBrix.Samples/tree/main/BrixInvaders/tests/BrixInvaders.PlayTests) | Launch-once over a running game engine; a seeded throwaway store; held mouse input; rendered frames in both orientations |
| [GoddessTempleDiscovery.PlayTests](https://github.com/ellisnet/CodeBrix.Samples/tree/main/GoddessTempleDiscovery/tests/GoddessTempleDiscovery.PlayTests) | Launch-once over a game engine with the CPU render tier forced; a store seeded with a returning player's settings and last seats; a recording journal-file bridge registered in place of the save picker; game state read on the engine thread; table coordinates mapped onto the scaled canvas and clicks held down for whole engine steps; the setup pane, and seeded cards-and-dice games played through the real table |
| [WikipediaPublisher.PlayTests](https://github.com/ellisnet/CodeBrix.Samples/tree/main/WikipediaPublisher/tests/WikipediaPublisher.PlayTests) | An embedded browser composited into the virtual screen, against a loopback HTTP server |
| [PlayTestDemo.PlayTests](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/PlayTestDemo/tests/PlayTestDemo.PlayTests) | Every part of the API in one place: checked controls, scrolling, every picker kind, typed locators, editors, menus, held keys, hidden elements, `SelectOptionAsync`, the launcher, window close and minimize, and OpenGL |
| [Fresco.Brix.PlayTests](https://github.com/ellisnet/CodeBrix.Samples.Gpl3/tree/main/Fresco.Brix/tests/Fresco.Brix.PlayTests) | A larger application: the AdvancedTextEdit document pipeline and a music-engraving engine driven through real input |

The [PlayTestSupport README](https://github.com/ellisnet/CodeBrix.Samples/blob/main/PlayTestSupport/README.md) has the full list of suites with one line of coverage each. PlayTestDemo lives in the framework repository and references the framework through `ProjectReference`; copy its tests, not its project reference lines.

---

**Where to go next**

- [OpenGL](opengl.md) - the companion package for Graphics3DGL applications
- [Getting started](getting-started.md) - the minimal fixture these patterns grow from
- [PlayTest](README.md) - the hub page
