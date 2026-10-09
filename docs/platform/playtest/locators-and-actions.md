<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Testing your application](../10-testing-your-application.md) › [PlayTest](README.md) › Locators and actions</sub>

# Locators and actions

**A locator describes how to find an element; an action finds it, waits until it can be used, and drives it with real input.** Locators are lazy - nothing is looked up until an action, an assertion or a read uses one, and it is looked up again every time - so a locator can be created once, kept in a property, and used across page resets.

## Finding elements

Every factory is on `Page`, and the same set is on `Locator` for a search scoped to that element's subtree.

| Factory | Matches |
| --- | --- |
| `GetByRole(AriaRole role, PageGetByRoleOptions options = null)` | Elements with that ARIA role whose accessible name matches `Name` or `NameRegex` |
| `GetByTestId(string testId, PageGetByTestIdOptions options = null)` | The element whose `AutomationProperties.AutomationId` is `testId` |
| `GetByText(string text, PageGetByTextOptions options = null)`, `GetByText(Regex text)` | The innermost `TextBlock`, or control with string `Content`, whose text matches |
| `GetByLabel(string text, PageGetByTextOptions options = null)` | Controls whose accessible name matches |
| `GetByType<T>(bool includeHidden = false) where T : UIElement` | Elements of an application- or add-in-owned type, without PlayTest referencing that library |

```csharp
var send = Page.GetByRole(AriaRole.Button, new() { Name = "Send", Exact = true });
var owner = Page.GetByTestId("Owner");
var result = Page.GetByText("Improve keyboard navigation", new() { Exact = true });
var status = Page.GetByText(new Regex(@"^Done: \d+ items"));
var editor = Page.GetByLabel("Source editor");
var canvas = Page.GetByType<DrawingCanvas>();
```

The matching rules are worth knowing exactly.

- **Accessible name.** `GetByRole` and `GetByLabel` match the accessible name, taking the first of these that is not empty: `AutomationProperties.Name`; `AutomationProperties.LabeledBy`; the automation peer's own name (a button's string content, for example); the visible text inside the element, leaving out icon glyphs (so a button whose content is a panel is named by the text in it); the element's `ToolTipService.ToolTip` text (so an icon-only button is named by its tooltip); and last, any text the element draws, such as an icon glyph. A tooltip never replaces text the user can see. By default the match is a case-insensitive, whitespace-normalized *substring*. With `Exact = true` it is a whole, case-sensitive match. `NameRegex` matches a pattern instead.
- **Visible only.** Every factory matches only visible elements unless its `IncludeHidden` option (or `includeHidden` argument) is set. Hidden means not `Visible` itself or under a non-`Visible` ancestor, zero-size, or a cell an `ItemsRepeater` keeps off-screen for reuse.
- **Popups included.** Every search includes open popups and flyouts, so a menu's items and a dialog's buttons are found from `Page` like anything else.
- **Menu names** exclude the template's arrow and check glyphs.

```csharp
// Hidden elements are matched only on request.
await Expect(Page.GetByLabel("Hidden action")).ToHaveCountAsync(0);
await Expect(Page.GetByLabel("Hidden action", new() { IncludeHidden = true })).ToHaveCountAsync(1);
await Expect(Page.GetByTestId("HiddenTwin", new() { IncludeHidden = true })).ToHaveCountAsync(1);
```

> [!TIP]
> `GetByTestId` is the cheapest and least ambiguous locator. Give every control a test reaches by identity an `AutomationProperties.AutomationId`, and keep `GetByRole` for the places where the role and the visible name are themselves what the test means to prove.

### The roles PlayTest assigns

`Button` (also split buttons), `Checkbox`, `Combobox`, `Dialog` (`ContentDialog`), `Group`, `Img`, `Link`, `Listbox`, `Menu`, `Menubar`, `Menuitem` (also submenus and `MenuBarItem`), `Menuitemcheckbox` (`ToggleMenuFlyoutItem`), `Option` (list and combo items), `Progressbar`, `Radio`, `Separator`, `Slider`, `Switch` (`ToggleSwitch`), `Tab`, `Tablist`, `Textbox`, `Toolbar`, `Tree` and `Treeitem`. Every other element has the `Generic` role. The role comes from the control type or its automation peer. The full `AriaRole` enum is adapted from Microsoft Playwright for .NET; values outside this list never match.

### Narrowing a match

```csharp
var rows = Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option);

var first = rows.First;
var last = rows.Last;
var third = rows.Nth(2);

// A dialog chosen by its text, then a button inside it.
var dialog = Page.GetByRole(AriaRole.Dialog).Filter(new() { HasText = "Replace existing file?" });
await dialog.GetByRole(AriaRole.Button, new() { Name = "No", Exact = true }).ClickAsync();
```

`Filter` takes `HasText` (a substring) or `HasTextRegex`. A `ContentDialog`'s text is all of its text - title, content and button labels - so filtering dialogs by their title is reliable. That `Dialog(title)` helper appears in several of the sample suites:

```csharp
// From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/tests/PainDiagram.PlayTests/ApplicationTests.cs
private Locator Dialog(string title) => Page.GetByRole(AriaRole.Dialog).Filter(new() { HasText = title });

private async Task AnswerAsync(string title, string button)
{
    await Dialog(title).GetByRole(AriaRole.Button, new() { Name = button, Exact = true }).ClickAsync();
    await Expect(Dialog(title)).ToHaveCountAsync(0);
}
```

### Strict mode

An operation on *one* element throws `PlayTestException` when the locator matches more than one: "Strict mode violation ... resolved to N elements. Use a unique name, test ID, or Nth()". Inside retrying actions and assertions an ambiguous match is retried rather than failed at once - one dialog briefly overlapping the next is not an error - and the violation, with its match count, is reported only when the timeout elapses. One-shot reads (`IsVisibleAsync`, `IsEnabledAsync`, `IsDisabledAsync`, `BoundingBoxAsync`) report it immediately.

The fixes, in order of preference: `Exact = true`, a test ID, a scoped locator, `Filter(...)`, and last `Nth(...)`.

## Pointer actions

| Action | What it does |
| --- | --- |
| `ClickAsync(LocatorClickOptions options = null)` | Clicks the element. `Button` (`MouseButton.Left`, `Right`, `Middle`), `ClickCount` 1 to 3, `Position` relative to the element in logical pixels, `Timeout` |
| `HoverAsync(LocatorHoverOptions options = null)` | Moves the pointer over the element, optionally at `Position` |
| `DragByAsync(float deltaX, float deltaY, LocatorDragOptions options = null)` | Presses at `Position` (default the center), moves in `Steps` (default 10, 1 to 1000) and releases - always, even on failure. The end point must be inside the virtual screen |

Before every pointer action the locator waits until the element is visible, enabled, at stable bounds and actually the target of the hit test at that point. Positions must lie inside the element.

```csharp
await Page.GetByRole(AriaRole.Listbox).GetByText("Item 3").ClickAsync(
    new() { Button = MouseButton.Right, Position = new() { X = 10, Y = 5 } });

await editor.ClickAsync(new() { ClickCount = 2, Position = new() { X = 100, Y = 30 } });

await Page.GetByType<MySplitter>().DragByAsync(80, 0);
```

A drag that starts at a computed point is how the samples draw on a canvas with real pointer input:

```csharp
// From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/tests/PainDiagram.PlayTests/ApplicationTests.cs
var rect = await DrawingRectAsync();
await Canvas.DragByAsync(0, rect.Height * length,
    new() { Position = new() { X = rect.MidX, Y = rect.Top + rect.Height * top } });
await Fixture.Application.WaitForAsync(() => Fixture.Session.StrokeCount, value => value == count + 1,
    description: "the committed stroke");
```

### The mouse

`Page.Mouse` works in screen coordinates for anything a locator action does not cover - a stroke with several turns, a held button, the wheel.

```csharp
await Page.Mouse.MoveAsync(x, y);
await Page.Mouse.DownAsync();
await Page.Mouse.MoveAsync(x + 40, y + 50);
await Page.Mouse.UpAsync();

await Page.Mouse.ClickAsync(x, y);
await Page.Mouse.WheelAsync(0, 120);   // vertical only: deltaX must be 0
```

Place the pointer over the scrollable area before `WheelAsync`, as the GitHubIssueFinder suite does to reach the last group of a long portrait list:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/tests/GitHubIssueFinder.PlayTests/OrientationTests.cs
var first = await Page.GetByText("sample/repository-01", new() { Exact = true }).BoundingBoxAsync();
await Page.Mouse.MoveAsync(first.X + first.Width / 2, first.Y + first.Height / 2);
await Page.Mouse.WheelAsync(0, 20000);
await last.ClickAsync();
```

## Text input

| Action | What it does |
| --- | --- |
| `FillAsync(string value)` | Replaces the whole value of a `TextBox`, `PasswordBox` or any element with an `IValueProvider` peer. A read-only provider is waited on, not written |
| `PressSequentiallyAsync(string text, LocatorPressOptions options = null)` | Focuses the element, then types real key events, Enter and Tab for newline and tab; `Delay` is the wait between characters |
| `PressAsync(string key, LocatorPressOptions options = null)` | Focuses the element and presses one key or chord; `Delay` is how long it is held |
| `Page.Keyboard.TypeAsync(string text, KeyboardTypeOptions options = null)` | Real key events to the focused element |
| `Page.Keyboard.InsertTextAsync(string text)` | Replaces the focused `TextBox` selection without key events, like a paste |

`FillAsync` is the fast way to put a value in; `PressSequentiallyAsync` and `TypeAsync` are the way to prove what happens while the user types, because completion, indentation and shortcuts run on real key events. `PressAsync` and `PressSequentiallyAsync` can focus any element made focusable with `IsTabStop` - a game surface or a canvas - not only controls. Focus already inside a composite editor's subtree is kept.

```csharp
// From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/tests/JustBetweenUs.PlayTests/ApplicationTests.cs
await Input.FillAsync("replace me");
await Input.PressAsync("ControlOrMeta+A");
await Page.Keyboard.InsertTextAsync("typed message");
await Expect(Input).ToHaveValueAsync("typed message");
await Input.PressAsync("ControlOrMeta+Backspace");
await Expect(Input).ToHaveValueAsync("typed ");
```

Values read back from a `TextBox` report LF line endings, although the control stores CR internally.

## Keys

Key names are a single letter or digit, any `Windows.System.VirtualKey` name (case-insensitive), or `Control`, `Alt`, `Meta`, `Backspace`, `ArrowLeft`, `ArrowRight`, `ArrowUp`, `ArrowDown` and `ControlOrMeta`. Chords join keys with `+`.

```csharp
await Page.Keyboard.PressAsync("Enter");
await Page.Keyboard.PressAsync("Control+z");
await Page.Keyboard.PressAsync("Shift+Tab");
await Page.Keyboard.PressAsync("F5");
await Page.GetByTestId("Owner").PressAsync("Escape");
```

> [!IMPORTANT]
> The virtual head uses one Control-based shortcut model on every operating system. `ControlOrMeta` is Control, and `TextBox` editing shortcuts follow Control on macOS too. Do not expect Meta (Command) shortcuts in a PlayTest.

### Held keys

`PressAsync` without a `Delay` dispatches key down and key up in one UI-thread step. Code that samples key state once per frame or per game cycle - a game loop - usually never sees such a press. Hold the key instead:

```csharp
await Page.Keyboard.DownAsync("ArrowRight");   // returns after a rendered frame, key still down
await app.WaitForAsync(() => game.ShipX, x => x > 100);
await Page.Keyboard.UpAsync("ArrowRight");

await Page.Keyboard.PressAsync("Space", new() { Delay = 150 });   // held for 150 ms
```

`DownAsync` takes one key, not a chord - `DownAsync("Shift+ArrowRight")` throws `ArgumentException`. Hold modifiers with separate calls; a held Shift, Control, Alt or Meta applies to later presses and typing. There is no automatic key repeat. `SetContentAsync` releases any key still held before it replaces the page (the key-up events go to the old page), and so does `DisposeAsync`, so held keys never leak into the next test. A launch-once suite that never resets the page must release its own keys, in a `finally` block.

```csharp
// From CodeBrix.Platform/samples/CodeBrixPlatform/PlayTestDemo/tests/PlayTestDemo.PlayTests/ApiLabTests.cs
await Page.GetByTestId("KeySampler").ClickAsync();
await Page.Keyboard.DownAsync("ArrowRight");
await fixture.Application.WaitForAsync(() => Lab.KeySampler.SamplesDown, samples => samples >= 3,
    description: "samples with the key down");
await Expect(Page.GetByTestId("KeyStatus")).ToContainTextAsync("Right arrow down");
await Page.Keyboard.UpAsync("ArrowRight");
await Expect(Page.GetByTestId("KeyStatus")).ToContainTextAsync("Right arrow up");
```

## Checked controls

`CheckAsync`, `UncheckAsync` and `SetCheckedAsync(bool)` support `CheckBox`, `RadioButton`, `ToggleButton`, `ToggleSwitch` (clicked by its thumb), `ToggleMenuFlyoutItem` and any element with an `IToggleProvider` peer. They click with real pointer input only when the state has to change, then wait for the requested state. A toggle menu item's result is verified even after its flyout closes.

```csharp
await Page.GetByTestId("IncludeClosed").SetCheckedAsync(includeClosed);
await Page.GetByRole(AriaRole.Menuitem, new() { Name = "View", Exact = true }).ClickAsync();
await Page.GetByRole(AriaRole.Menuitemcheckbox, new() { Name = "Word wrap" }).CheckAsync();
await Expect(Page.GetByTestId("IncludeClosed")).ToBeCheckedAsync();
```

A checked `RadioButton` cannot be unchecked by clicking, so `UncheckAsync` on one times out - check a different radio button instead.

## Selecting options

`SelectOptionAsync` works on a `ComboBox`, `ListBox`, `ListView` or `GridView`. It selects by value or label (exact), or by a `SelectOptionValue { Value, Label, Index }`, without opening a drop-down or realizing any rows, and raises the control's normal `SelectionChanged`. It waits until every requested option exists and returns the selected values.

```csharp
// From CodeBrix.Platform/samples/CodeBrixPlatform/PlayTestDemo/tests/PlayTestDemo.PlayTests/ApiLabTests.cs
var combo = Page.GetByRole(AriaRole.Combobox, new() { Name = "Row", Exact = true });
(await combo.SelectOptionAsync("Row 24")).Should().Equal("Row 24");
(await combo.SelectOptionAsync(new SelectOptionValue { Index = 2 })).Should().Equal("Row 3");

var colors = Page.GetByTestId("ColorList");                       // a multiple-selection list
(await colors.SelectOptionAsync(new[] { "Yellow", "Green" })).Should().Equal("Green", "Yellow");
(await colors.SelectOptionAsync(Array.Empty<string>())).Should().BeEmpty();   // clears
```

A single-selection control takes the first match; a multiple-selection list gets exactly the matches. The label is the `DisplayMemberPath` member, a string item, string `Content`, the realized row's text, or `ToString()`; the value is the `SelectedValuePath` member, else the label. To prove the drop-down itself, click it and click the option like a user:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/tests/GitHubIssueFinder.PlayTests/ApplicationTests.cs
await Page.GetByTestId("SelectedScheme").ClickAsync();
await Page.GetByRole(AriaRole.Option, new() { Name = name, Exact = true }).ClickAsync();
```

## Scrolling

`ScrollIntoViewIfNeededAsync()` brings an *attached* element into its `ScrollViewer`'s viewport before a click. It does not materialize virtualized items that are not yet realized: scroll their container first (the mouse wheel, above), or, for a selector, use `SelectOptionAsync`, which needs no realized rows. A partly scrolled-out element is brought into view automatically before an element screenshot.

## Reads

| Read | Behavior |
| --- | --- |
| `InputValueAsync()`, `InnerTextAsync()`, `TextContentAsync()` | The value or text, once the element is attached |
| `IsCheckedAsync()` | Retries until the element is attached |
| `CountAsync()`, `IsVisibleAsync()`, `IsEnabledAsync()`, `IsDisabledAsync()` | Single reads, no retry |
| `BoundingBoxAsync()` | `X`, `Y`, `Width`, `Height` in screen coordinates, or `null` when not visible |

Prefer the retrying assertions in [Assertions and reading state](assertions.md) to these reads straight after an action: a one-shot read sees the moment, not the outcome.

## Pitfalls

- **Setting control state directly** - `IsChecked`, `Text`, `command.Execute(...)` - instead of acting skips the input pipeline the test exists to prove. Use `CheckAsync`, `FillAsync` and `ClickAsync`.
- **Ambiguous locators.** A substring name match finds "Save" inside "Save as…". Use `Exact = true` for buttons and menu items.
- **Reading a hidden element** with `GetByText`, `GetByTestId` or `GetByLabel` finds nothing. Pass `IncludeHidden = true`, or read the state with `EvaluateAsync`.
- **Keys a game loop never sees.** Use `Keyboard.DownAsync` and `UpAsync`, or `PressAsync` with a `Delay`.

---

**Where to go next**

- [Assertions and reading state](assertions.md) - retrying assertions, `EvaluateAsync` and `WaitForAsync`
- [Pickers, clipboard, theme and orientation](pickers-clipboard-theme-orientation.md) - the operating-system services a test scripts
- [PlayTest](README.md) - the hub page
