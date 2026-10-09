<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Testing your application](../10-testing-your-application.md) › [PlayTest](README.md) › Pickers, clipboard, theme and orientation</sub>

# Pickers, clipboard, theme and orientation

**Everything an application normally gets from the operating system - a file chosen in a dialog, a browser opened for a link, the clipboard, the light or dark preference, the screen's orientation, the window's close button - is scripted, isolated or simulated in PlayTest, so a test controls it and the machine running the test is never touched.** The application keeps calling the same APIs it calls on a desktop head; [07 - Platform services](../07-platform-services.md) describes those APIs.

## Scripted file and folder pickers

The application opens its normal `Windows.Storage.Pickers` picker - `FileOpenPicker`, `FileSavePicker`, `FolderPicker` - and PlayTest answers it from a queue the test filled beforehand. No dialog is ever displayed, headed or not. Queue the answer **before** the action that opens the picker.

| Member of `app.FilePickers` | Effect |
| --- | --- |
| `EnqueueFolder(string path)` | The next folder picker returns this folder; `null` means the user cancelled |
| `EnqueueOpenFile(string path)` | The next single open-file picker returns this file; `null` cancels |
| `EnqueueOpenFiles(params string[] paths)` | The next multiple open-file picker returns these files; `null` or an empty list cancels |
| `EnqueueSaveFile(string path)` | The next save picker returns this path; `null` cancels |
| `EnqueueFolderFailure(Exception error)`, `EnqueueOpenFileFailure(Exception error)`, `EnqueueSaveFileFailure(Exception error)` | The next picker of that kind throws `error` |
| `Clear()` | Empties the queues and the request history |
| `FolderRequestCount`, `OpenFileRequestCount`, `SaveFileRequestCount` | How many pickers of each kind the application opened |
| `LastSuggestedFileName` | The `SuggestedFileName` the application gave its last save picker |

```csharp
var pickers = Fixture.Application.FilePickers;
pickers.EnqueueFolder(existingFolder);
pickers.EnqueueOpenFile(existingFile);
pickers.EnqueueOpenFiles(firstFile, secondFile);
pickers.EnqueueSaveFile(savePath);
pickers.EnqueueFolder(null);   // the next folder picker is cancelled
```

The rules:

- **Three FIFO queues**: folder, open-file (single and multiple pickers share one queue) and save-file. A single-file picker rejects a multiple-file answer.
- **Paths become absolute when queued.** Folders and files to open must exist when the picker opens; a save path's parent folder must exist.
- **A new save path creates an empty file**, as a native picker does. An existing file is returned intact - never truncated - so the application's own "replace it?" logic runs.
- **A queued failure** makes that picker throw the given exception, in queue order with the other answers, so the application's "picker failed" or "pickers not supported" branch runs. It counts as a request.
- **An unqueued picker request throws `PlayTestException`.** It never silently cancels, so a test that forgot to queue an answer fails loudly.
- **`SetContentAsync` does not clear the queues.** Call `FilePickers.Clear()` in the per-test reset.

A save, end to end - the answer queued, the click, the dialog the application shows, the request counted and the file checked on disk:

```csharp
// From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/tests/PainDiagram.PlayTests/ApplicationTests.cs
[Fact]
public async Task Save_writes_png_and_offers_to_clear()
{
    await DrawStrokeAsync();
    var path = NewPngPath();
    Fixture.Application.FilePickers.EnqueueSaveFile(path);
    await Button("Save").ClickAsync();
    await Expect(Dialog("Image saved")).ToContainTextAsync("Do you want to clear the drawing?");
    Fixture.Application.FilePickers.LastSuggestedFileName.Should().Be("pain_diagram.png");
    Fixture.Application.FilePickers.SaveFileRequestCount.Should().Be(1);
    using (var image = SKBitmap.Decode(path))
    {
        image.Width.Should().Be(1000);
        image.Height.Should().Be(1000);
    }
    await AnswerAsync("Image saved", "No");
}
```

A cancelled picker, and a picker failure reaching the application's error branch:

```csharp
// From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/tests/PainDiagram.PlayTests/ApplicationTests.cs
Fixture.Application.FilePickers.EnqueueSaveFile(null);
await Button("Save").ClickAsync();
await Fixture.Application.WaitForAsync(() => Fixture.Application.FilePickers.SaveFileRequestCount, count => count == 1,
    description: "the save picker request");
await Expect(Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
```

```csharp
// From CodeBrix.Platform/samples/CodeBrixPlatform/PlayTestDemo/tests/PlayTestDemo.PlayTests/FilePickerContractTests.cs
var error = new NotSupportedException("No file dialogs on this head.");
pickers.EnqueueFolderFailure(error);
await PickThroughButtonAsync("Choose folder");
await Expect(Page.GetByTestId("PickerOutcome")).ToHaveTextAsync("Error");
await Expect(Page.GetByTestId("PickerStatus")).ToHaveTextAsync("NotSupportedException: No file dialogs on this head.");
```

> [!TIP]
> Write every file a test opens, and every folder a test saves into, at test time under a unique folder beside the test binary - `Path.Combine(AppContext.BaseDirectory, "TestResults", "PlayTestData", Guid.NewGuid().ToString("N"))` is the samples' choice. Nothing then depends on a committed asset or on another test's leftovers.

## The launcher

PlayTest registers its own launcher, as every desktop head does, so `Windows.System.Launcher` - a `HyperlinkButton`'s `NavigateUri`, or `LaunchUriAsync` with an `https:`, `mailto:` or `file:` URI - never starts a browser, viewer or any other process. Every request is recorded.

| Member of `app.Launcher` | Meaning |
| --- | --- |
| `LaunchedUris` | Every `LaunchUriAsync` request, in order |
| `QueriedUris` | Every `QueryUriSupportAsync` request |
| `LaunchResult` | What `LaunchUriAsync` returns; `true` by default |
| `QueryUriSupportResult` | What `QueryUriSupportAsync` returns; `Available` by default |
| `Clear()` | Clears the history and resets both results |

```csharp
// From CodeBrix.Platform/samples/CodeBrixPlatform/PlayTestDemo/tests/PlayTestDemo.PlayTests/ApiLabTests.cs
var launcher = fixture.Application.Launcher;
await Page.GetByRole(AriaRole.Link, new() { Name = "Project website", Exact = true }).ClickAsync();
await fixture.Application.WaitForAsync(() => launcher.LaunchedUris.Count, count => count == 1, description: "the link launch");
launcher.LaunchedUris[0].Should().Be(new Uri("https://example.com/playtest-demo"));

launcher.LaunchResult = false;   // reach the application's "could not open" branch
await Button("Open report").ClickAsync();
await Expect(Page.GetByTestId("LaunchStatus")).ToHaveTextAsync("Launch result: False");
```

Call `Launcher.Clear()` in the per-test reset. An application that opens links through its own service interface rather than `Launcher` is better served by replacing that service with a recording fake - see [Patterns from the samples](patterns-from-the-samples.md#replace-services-at-the-di-boundary).

## The isolated clipboard

The application's clipboard is isolated from the desktop's: whatever the application copies stays inside the test process, and the test never reads or disturbs the clipboard of the person running it. `Page.ClipboardTextAsync()` returns the text on the application's clipboard, and `SetContentAsync` clears it before every test.

```csharp
// From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/tests/JustBetweenUs.PlayTests/ApplicationTests.cs
var encrypted = await EncryptAsync("Copied content.");
await Copy.ClickAsync();
await Expect(Dialog).ToContainTextAsync("The processed text has been copied to the system clipboard.");
(await Page.ClipboardTextAsync()).Should().Be(encrypted);
```

For formats other than text, read the application's clipboard through the normal `Windows.ApplicationModel.DataTransfer.Clipboard` API inside `EvaluateAsync`:

```csharp
// From CodeBrix.Samples/Pinta.Brix/tests/Pinta.Brix.PlayTests/ClipboardTests.cs
await MenuAsync("Edit", "Copy");
await WaitAsync(() => Clipboard.GetContent().Contains(StandardDataFormats.Bitmap), copied => copied, "the copied bitmap");
(await Page.ClipboardTextAsync()).Should().BeEmpty();
```

Paste works the same way: the application reads its own isolated clipboard, so a copy followed by a paste in one test round-trips without touching the desktop.

## The simulated operating-system theme

The theme is a simulated *operating-system* preference, chosen once at launch and fixed for the process. It reaches application startup, theme resources and the `UISettings` system colors exactly as the desktop's light or dark setting would. It never follows the real desktop. An application, or any element, can still choose its own `RequestedTheme`, as it can on a desktop head.

`PlayTestApplication.SystemTheme` is the theme in force. Choose it with `--theme=light|dark`, `CODEBRIX_PLAYTEST_THEME`, the `<CodeBrixPlayTestPreferredTheme>` project property or `PlayTestOptions.Theme` - see [Running, switches and the preview](running-and-switches.md#preferences-and-precedence). Light is the default.

A test that is meant to pass under either theme asserts against `SystemTheme` rather than hard-coding one:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/tests/GitHubIssueFinder.PlayTests/ThemeTests.cs
[Fact]
public async Task System_default_follows_launch_theme()
{
    // The simulated OS theme is fixed per process; the runners cover both.
    var dark = Fixture.Application.SystemTheme == ApplicationTheme.Dark;
    var palette = dark ? ColorSchemes.Dark : ColorSchemes.Light;
    await Expect(Page.GetByTestId("SelectedScheme"))
        .ToHaveValueAsync(dark ? "System default (Dark)" : "System default (Light)");
    (await Page.EvaluateAsync(() => ((FrameworkElement)Fixture.View.Content).ActualTheme))
        .Should().Be(dark ? ElementTheme.Dark : ElementTheme.Light);
}
```

Because the theme is fixed for the process, test the application's startup under both themes by running the suite twice - `--theme=light`, then `--theme=dark` - rather than trying to switch inside one run.

## Orientation

The virtual screen is 1920 x 1080 in landscape and 1080 x 1920 in portrait. `PlayTestApplication.Width` and `Height` are the current size, `PreferredOrientation` is the launch preference, and `Orientation` is the current orientation. A change is reported to the application through `DisplayInformation.OrientationChanged`, with the new screen size, just as a rotated device would report it.

There are three ways to choose the orientation for a test.

**Per page reset.** `Page.SetContentAsync(() => new MainPage(), ScreenOrientation.Portrait)` applies the orientation *before* the page is constructed, so the page is built in portrait from the start. Omitting the argument restores the launch preference on every reset.

**Per test, declared.** `[PlayTestOrientation(ScreenOrientation.Portrait)]` declares a method's requirement; a theory row can override it with the `PlayTestOrientation` trait (`PlayTestOrientationAttribute.CaseTraitName`). The fixture resolves it with `PlayTestOrientationAttribute.Resolve(method, rowTraits)` - row, then method, then `null` for "inherit the launch preference" - and passes the result to `SetContentAsync`, as shown in [Getting started](getting-started.md#the-test-class). Conflicting row values throw `ArgumentException`.

```csharp
[Fact]
[PlayTestOrientation(ScreenOrientation.Portrait)]
public async Task Portrait_header_wraps_and_search_remains_usable() { /* ... */ }

[Theory]
[PlayTestOrientation(ScreenOrientation.Landscape)]
[InlineData("portrait row", Traits = new[] { "PlayTestOrientation", "Portrait" })]
[InlineData("landscape row")]                  // inherits the method requirement
public async Task Layout_fits(string scenario)
{
    var box = await Page.GetByTestId("status").BoundingBoxAsync();
    (box.X + box.Width).Should().BeLessThanOrEqualTo(fixture.Application.Width, scenario);
}
```

**Mid-test.** `PlayTestApplication.SetOrientationAsync(orientation)` re-lays-out the *current* page, keeping its state; `SetOrientationAsync()` with no argument restores the launch preference. This is how a test proves the page survives rotation:

```csharp
// From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/tests/JustBetweenUs.PlayTests/OrientationTests.cs
await Input.FillAsync("This page survives orientation changes.");
await app.SetOrientationAsync(opposite);
app.Orientation.Should().Be(opposite);
await Expect(Input).ToHaveValueAsync("This page survives orientation changes.");
await app.SetOrientationAsync();
app.Orientation.Should().Be(preferred);
```

Application singletons stay alive across orientation changes. To test application *startup* in each orientation, run the suite in separate processes with `--orientation=landscape` and `--orientation=portrait`.

## Closing and minimizing the window

| Call | Behaves like |
| --- | --- |
| `await app.RequestCloseAsync()` | The window's close button. `AppWindow.Closing` handlers run and may cancel (a "save changes?" prompt); then `Window.Closed` handlers run and may cancel by marking the event handled. Returns `false` when the application cancelled. When the close goes ahead the window is hidden, but the process and the application keep running; the next `SetContentAsync` shows it again |
| `await app.MinimizeAsync()` | Minimizing, as the Linux desktop heads report it: the window is deactivated (`Window.Activated` with `Deactivated`) and hidden (`Window.VisibilityChanged`), so pause-on-hidden logic runs. Rendering and screenshots continue |
| `await app.RestoreAsync()` | Makes the window visible and active again. `SetContentAsync` also restores a minimized window |

```csharp
// From CodeBrix.Platform/samples/CodeBrixPlatform/PlayTestDemo/tests/PlayTestDemo.PlayTests/ApiLabTests.cs
await Page.GetByRole(AriaRole.Checkbox, new() { Name = "Unsaved changes (refuse to close)", Exact = true }).CheckAsync();
(await app.RequestCloseAsync()).Should().BeFalse();
await Page.GetByRole(AriaRole.Checkbox, new() { Name = "Unsaved changes (refuse to close)", Exact = true }).UncheckAsync();
(await app.RequestCloseAsync()).Should().BeTrue();
```

The veto logic these exercise is the "veto a close until unsaved work is handled" pattern in [07 - Platform services](../07-platform-services.md#veto-a-close-until-unsaved-work-is-handled).

## Embedded browser content

An application that uses the [WebView add-in](../add-ins/WebView.md) can be tested with its browser rendered offscreen and composited into the virtual screen - including screenshots and the preview - and driven with XAML pointer and keyboard input: WPE WebKit on Linux (system libraries required), Edge WebView2 on Windows (installed runtime required), and WKWebView on macOS through the add-in's offscreen helper. PlayTest adds no browser dependency of its own and offers no DOM locators, and no browser download, upload or script-dialog API. On Linux, a PlayTest click on a download link raised the WebView's `DownloadStarting` event and the application's download policy ran and completed the download; downloads have not been tried on Windows or macOS.

## Pitfalls

- **Leaving picker answers queued between tests.** Call `FilePickers.Clear()` and `Launcher.Clear()` in the per-test reset.
- **Queuing after the click.** The picker opens during the click; queue first.
- **Reading the desktop clipboard.** The application's clipboard is isolated; read it with `Page.ClipboardTextAsync()`.
- **Expecting a theme or orientation change to re-run startup.** Both are fixed at launch for the application's singletons; run separate processes.
- **Expecting a test's orientation change to resize the preview window.** The preview keeps the launch proportions and letterboxes.

---

**Where to go next**

- [Screenshots and recording](screenshots-and-recording.md) - capturing what the screen shows
- [Running, switches and the preview](running-and-switches.md) - choosing theme and orientation from the command line
- [PlayTest](README.md) - the hub page
