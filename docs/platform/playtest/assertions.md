<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Testing your application](../10-testing-your-application.md) › [PlayTest](README.md) › Assertions and reading state</sub>

# Assertions and reading state

**Every PlayTest assertion retries until it holds or its timeout elapses, so a test states the outcome it expects and never sleeps to "let the UI settle".** Asserting on what the user can see goes through `Expect(locator)`; asserting on what the application did behind the screen goes through `EvaluateAsync` and `WaitForAsync`, on the application's UI thread.

## Expect

`Assertions.Expect(Locator locator)` starts an assertion; a test class derived from `PageTest` has the same thing as a protected `Expect(locator)`.

| Assertion | Holds when |
| --- | --- |
| `ToBeVisibleAsync()` | The single match is visible |
| `ToBeHiddenAsync()` | Nothing visible matches |
| `ToBeEnabledAsync()`, `ToBeDisabledAsync()` | The element and every ancestor control are enabled, or not |
| `ToBeCheckedAsync()` | The element's checked state is on |
| `ToHaveCountAsync(int count)` | The locator matches exactly `count` elements |
| `ToHaveValueAsync(string)`, `ToHaveValueAsync(Regex)` | The input value matches |
| `ToHaveTextAsync(string)`, `ToHaveTextAsync(Regex)` | The whole text matches |
| `ToContainTextAsync(string)` | The text contains the substring |

`Not` inverts any of them - `Expect(locator).Not.ToBeVisibleAsync()`. Each takes an optional options object whose only member is `Timeout`, in milliseconds.

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/tests/GitHubIssueFinder.PlayTests/ApplicationTests.cs
await Expect(Button("Search")).ToBeDisabledAsync();
await Page.GetByTestId("Owner").FillAsync("sample");
await Page.GetByTestId("IncludeClosed").CheckAsync();
await Button("Search").ClickAsync();
await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync(new Regex(@"^Done: 5 items in 2 repositories in .+\.$"));
await Expect(Page.GetByTestId("HeaderOpenText")).ToHaveTextAsync("3 open");
await Expect(Page.GetByText("Arrives second", new() { Exact = true })).ToHaveCountAsync(0);
```

The rules behind them:

- **Text comparisons normalize whitespace.** `ToContainTextAsync` is case-sensitive.
- **A `ContentDialog`'s text is all of its text**: title, content and button labels. `Expect(dialog).ToContainTextAsync("Do you want to replace it?")` reads the body of a dialog found by its title.
- **Enabled is inherited.** An element counts as enabled when it and every ancestor *control* are enabled. In this framework only controls carry `IsEnabled`; a panel cannot be disabled.
- **`ToBeHiddenAsync` keeps Playwright's meaning.** It passes when nothing visible matches, including when nothing matches at all.
- **`ToHaveCountAsync(0)` is the way to wait for something to go away** - a dialog after it is answered, a row after it is removed.

```csharp
// From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/tests/PainDiagram.PlayTests/ApplicationTests.cs
Fixture.Application.FilePickers.EnqueueSaveFile(path);
await Button("Save").ClickAsync();
await Expect(Dialog("Image saved")).ToContainTextAsync("Do you want to clear the drawing?");
await AnswerAsync("Image saved", "No");
await Expect(Button("Save")).ToBeEnabledAsync();
```

## Reading application state: EvaluateAsync

UI objects belong to the application's UI thread. `EvaluateAsync` runs a function there and returns its result to the test, which is how a test reads a view model, a control property or the application's own `Window`. It is on both `PlayTestApplication` and `Page`.

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/tests/GitHubIssueFinder.PlayTests/ApplicationTests.cs
(await Page.EvaluateAsync(() => Fixture.Model.Groups[0].Rows.Count)).Should().Be(2);
(await Page.EvaluateAsync(() => string.Join(",", Fixture.Model.Groups.Select(g => g.FullName))))
    .Should().Be("alpha/lib,mid/tool,zeta/app");
```

```csharp
// From CodeBrix.Platform/samples/CodeBrixPlatform/PlayTestDemo/tests/PlayTestDemo.PlayTests/ApiLabTests.cs
var window = fixture.Application.Window;
(await Page.EvaluateAsync(() => window.Content)).Should().BeSameAs(fixture.View);
(await Page.EvaluateAsync(() => window.Title)).Should().Be("PlayTestDemo");
```

`PlayTestApplication.Window` is the `Window` the application created in `OnLaunched`, with the content it set; read its members inside `EvaluateAsync`. The `EvaluateAsync(Action)` overload runs a statement, such as attaching an event handler a test wants to count.

> [!NOTE]
> `EvaluateAsync` reads state; it is not a way to act. Changing control state from inside it bypasses the input pipeline the test exists to prove. Act through locators and the keyboard and mouse, then read.

## Waiting for a non-visual outcome: WaitForAsync

```csharp
Task<T> WaitForAsync<T>(Func<T> probe, Func<T, bool> predicate,
                        float? timeout = null, string description = "application outcome")
```

`WaitForAsync` polls `probe` on the UI thread until `predicate` accepts its value, then returns the value. Use it for outcomes that have no element to assert on: a service call completed, a background job finished, a file was written, a link was opened. The `description` appears in the timeout message, so name the outcome.

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/tests/GitHubIssueFinder.PlayTests/ApplicationTests.cs
await Fixture.Application.WaitForAsync(() => Fixture.Model.IsSearching, busy => !busy);
```

```csharp
// A file the application saved through the save picker.
await app.WaitForAsync(() => File.Exists(outputPath) && new FileInfo(outputPath).Length > 0,
    written => written, description: "the saved file");
```

```csharp
// From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/tests/PainDiagram.PlayTests/ApplicationTests.cs
await Fixture.Application.WaitForAsync(() => Fixture.Application.FilePickers.SaveFileRequestCount, count => count == 1,
    description: "the save picker request");
```

## Timeouts

The default timeout for every action and assertion is 10 seconds. Change it for the whole run with `PlayTestOptions.Timeout`, for the page with `Page.SetDefaultTimeout(milliseconds)`, or for one call with its options object.

```csharp
await Expect(Page.GetByTestId("Progress")).ToHaveTextAsync("Done", new() { Timeout = 30_000 });
var missing = () => combo.SelectOptionAsync("Row 99", new() { Timeout = 200 });
await missing.Should().ThrowAsync<PlayTestException>().WithMessage("*Row 99*");
```

A longer timeout costs nothing when the condition holds early: assertions and `WaitForAsync` return the moment it does.

## When something fails

Every timeout, strict-mode violation, unsupported element and application or renderer failure throws `PlayTestException`. A failed locator action's message carries a description of the visible UI and the path of a screenshot taken at the moment of failure, written under `PlayTestOptions.ArtifactsDirectory` (`TestResults/PlayTest` by default, beside the test binary). Open the screenshot first: it usually shows the dialog that was in the way or the element that never became enabled.

## One-shot reads versus assertions

`CountAsync`, `IsVisibleAsync`, `IsEnabledAsync` and `IsDisabledAsync` read once and do not retry. Straight after an action they see the moment rather than the outcome, so a test that does this

```csharp
await Button("Search").ClickAsync();
(await Page.GetByText("Improve keyboard navigation").IsVisibleAsync()).Should().BeTrue();   // racy
```

passes or fails depending on timing. Write the assertion instead:

```csharp
await Button("Search").ClickAsync();
await Expect(Page.GetByText("Improve keyboard navigation", new() { Exact = true })).ToBeVisibleAsync();
```

One-shot reads are right where the moment is the point - checking that something is *not yet* realized before a scroll, for example.

## Combining with SilverAssertions

The sample suites use PlayTest's `Expect(...)` for everything on screen and [SilverAssertions](../../libraries/SilverAssertions.md) for values read back from the application or from fakes:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/tests/GitHubIssueFinder.PlayTests/ApplicationTests.cs
await Expect(Page.GetByText("Improve keyboard navigation", new() { Exact = true })).ToBeVisibleAsync();
Fixture.Search.LastRequest.Owner.Should().Be("sample");
Fixture.Search.LastRequest.IncludeClosed.Should().Be(includeClosed);
```

---

**Where to go next**

- [Pickers, clipboard, theme and orientation](pickers-clipboard-theme-orientation.md) - scripting what the operating system would have supplied
- [Screenshots and recording](screenshots-and-recording.md) - capturing what the screen shows
- [PlayTest](README.md) - the hub page
