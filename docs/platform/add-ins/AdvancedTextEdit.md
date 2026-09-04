<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Add-ins](../08-add-ins.md) › AdvancedTextEdit</sub>

# AdvancedTextEdit

**The AdvancedTextEdit add-in gives your application a full code and text editor control, `AdvancedTextEdit : Control`, on every CodeBrix.Platform head.** It has the editing model of a professional code editor: a rope-backed document with anchors and grouped undo, syntax highlighting, folding, code completion, snippets, a search panel and smart indentation. Rendering is virtualized and driven by the family's single text engine, so it stays responsive on very large documents and matches `TextBlock` shaping exactly.

| | |
| --- | --- |
| **Package** | [`CodeBrix.Platform.AdvancedTextEdit.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.AdvancedTextEdit.ApacheLicenseForever) |
| **Adds** | The `AdvancedTextEdit` control, `AdvancedTextEditOptions`, `AdvancedTextEditCommands`, and the `Document`, `Editing`, `Rendering`, `Highlighting`, `CodeCompletion`, `Snippets`, `Folding`, `Search`, `Indentation` and `Utils` namespaces beneath it |
| **Heads** | All six - Windows Win32-Skia and WPF-Skia, Linux X11, Wayland and frame buffer, and macOS |
| **Requires** | Nothing beyond the core framework: pure managed code, no native libraries, no theme resources, no initialization call |

## Add it to your application

Add the package to the shared `.Core` project of your application - the same project that references `CodeBrix.Platform.ApacheLicenseForever` - so every head picks it up:

```bash
dotnet add package CodeBrix.Platform.AdvancedTextEdit.ApacheLicenseForever
```

```xml
<!-- MyApp.Core/MyApp.Core.csproj (excerpt) -->
<ItemGroup>
  <PackageReference Include="CodeBrix.Platform.ApacheLicenseForever" />
  <PackageReference Include="CodeBrix.Platform.AdvancedTextEdit.ApacheLicenseForever" />
</ItemGroup>
```

`CodeBrix.Platform.ApacheLicenseForever` and the shared text engine, [`CodeBrix.Platform.TextLayout.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.TextLayout.ApacheLicenseForever), flow in automatically, and the SkiaSharp drawing types come with them - they are what a background renderer paints on.

Declare the XAML namespace in the page that hosts the editor. Both forms work:

```xml
xmlns:advtxt="clr-namespace:CodeBrix.Platform.UI.AdvancedTextEdit;assembly=CodeBrix.Platform.UI.AdvancedTextEdit"
xmlns:advtxt="using:CodeBrix.Platform.UI.AdvancedTextEdit"
```

The C# usings are organized by feature, and you take only the ones you use:

```csharp
using CodeBrix.Platform.UI.AdvancedTextEdit;
    // AdvancedTextEdit, AdvancedTextEditOptions, AdvancedTextEditCommands,
    // TextViewPosition
using CodeBrix.Platform.UI.AdvancedTextEdit.Document;
    // TextDocument, DocumentLine, TextAnchor, ISegment, TextLocation,
    // UndoStack, TextSegment, TextSegmentCollection<T>
using CodeBrix.Platform.UI.AdvancedTextEdit.Editing;
    // TextArea, Caret, Selection, RectangleSelection, EditorCommands,
    // KeyBinding, EditorCommandBinding
using CodeBrix.Platform.UI.AdvancedTextEdit.Rendering;
    // TextView, IBackgroundRenderer, IVisualLineTransformer,
    // DocumentColorizingTransformer, KnownLayer
using CodeBrix.Platform.UI.AdvancedTextEdit.Highlighting;
    // HighlightingManager, IHighlightingDefinition, HighlightingColor,
    // DocumentHighlighter
using CodeBrix.Platform.UI.AdvancedTextEdit.Highlighting.Xshd;
    // HighlightingLoader
using CodeBrix.Platform.UI.AdvancedTextEdit.CodeCompletion;
    // CompletionWindow, ICompletionData, InsightWindow,
    // OverloadInsightWindow, IOverloadProvider
using CodeBrix.Platform.UI.AdvancedTextEdit.Snippets;
    // Snippet, SnippetTextElement, SnippetReplaceableTextElement, ...
using CodeBrix.Platform.UI.AdvancedTextEdit.Folding;
    // FoldingManager, FoldingSection, NewFolding, XmlFoldingStrategy
using CodeBrix.Platform.UI.AdvancedTextEdit.Search;
    // SearchPanel, SearchCommands, ISearchStrategy, SearchStrategyFactory
using CodeBrix.Platform.UI.AdvancedTextEdit.Indentation;
    // IIndentationStrategy, DefaultIndentationStrategy
using CodeBrix.Platform.UI.AdvancedTextEdit.Indentation.CSharp;
    // CSharpIndentationStrategy
using CodeBrix.Platform.UI.AdvancedTextEdit.Utils;
    // Rope<T>, FileReader, ImmutableStack<T>, StringSegment
```

Nothing else is required: no theme resources, no native libraries, no initialization call. Two features are opt-in and are installed by a single call each - `SearchPanel.Install(Editor.TextArea)` and `FoldingManager.Install(Editor.TextArea)`.

> [!TIP]
> Give the control a bounded size - a star-sized `Grid` row or column, or an explicit `Width`/`Height`. It sizes its viewport from the space it is given, manages its own scrolling and virtualization, and must never be wrapped in a `ScrollViewer`.

## Using it

### The control in a page

A star-sized row for the editor and an auto row for a status line is the whole layout.

```xml
<!-- Views/MainPage.xaml (inside the shared .UI project) -->
<Page x:Class="MyApp.Views.MainPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:advtxt="clr-namespace:CodeBrix.Platform.UI.AdvancedTextEdit;assembly=CodeBrix.Platform.UI.AdvancedTextEdit">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>
        <advtxt:AdvancedTextEdit x:Name="Editor"
            FontFamily="monospace" FontSize="13"
            ShowLineNumbers="True" WordWrap="False" />
        <TextBlock x:Name="Status" Grid.Row="1" Margin="8,4" />
    </Grid>
</Page>
```

A new instance creates its own `TextDocument` and `TextArea`. The defaults are the `monospace` font family at size 13, a white background and black foreground.

The code-behind opens a file, picks a highlighting definition from its extension, installs the search panel and keeps the status line current.

```csharp
// Views/MainPage.xaml.cs
using CodeBrix.Platform.UI.AdvancedTextEdit;
using CodeBrix.Platform.UI.AdvancedTextEdit.Highlighting;
using CodeBrix.Platform.UI.AdvancedTextEdit.Indentation.CSharp;
using CodeBrix.Platform.UI.AdvancedTextEdit.Search;
using Microsoft.UI.Xaml.Controls;
using System.IO;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
        SearchPanel.Install(Editor.TextArea);                       // Ctrl+F
        Editor.Options.HighlightCurrentLine = true;
        Editor.Options.ConvertTabsToSpaces = true;
        Editor.TextArea.Caret.PositionChanged += (_, _) =>
            Status.Text = $"Ln {Editor.TextArea.Caret.Line}, Col {Editor.TextArea.Caret.Column}"
                        + (Editor.IsModified ? "  *" : "");
    }

    public void OpenFile(string path)
    {
        Editor.Load(path);                                          // encoding auto-detected
        Editor.SyntaxHighlighting =
            HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(path));
        Editor.TextArea.IndentationStrategy = Editor.SyntaxHighlighting?.Name == "C#"
            ? new CSharpIndentationStrategy(Editor.Options)
            : new CodeBrix.Platform.UI.AdvancedTextEdit.Indentation.DefaultIndentationStrategy();
        Editor.ScrollToHome();
    }

    public void SaveFile(string path) => Editor.Save(path);        // uses Editor.Encoding
}
```

`Load(string)` and `Load(Stream)` read the text, auto-detect the encoding, set `Encoding` and clear `IsModified`. `Save(string)` and `Save(Stream)` write with `Encoding`, flush, clear `IsModified`, and do not close the stream.

### The four layers

Every feature hangs off one of four objects, each reachable from the one above.

```text
AdvancedTextEdit  (Control)   the XAML control: scroll bars, Load/Save,
    |                         Text/Document/Options, SyntaxHighlighting
    +-- TextArea  (Control)   editing: Caret, Selection, input handlers,
          |                   command/key bindings, LeftMargins, TextEntered
          +-- TextView (Panel) rendering: visual lines, LineTransformers,
                |             BackgroundRenderers, ElementGenerators, scroll
                +-- TextDocument   the model: rope text, lines, anchors,
                                   UndoStack, change events
```

All three UI classes implement `ITextEditorComponent` (`Document`, `DocumentChanged`, `Options`, `OptionChanged`, plus `IServiceProvider`). Feature installers such as folding and search take the `TextArea`.

### The document model

`TextDocument` is a sealed `IDocument` with a rope-backed text buffer, `Insert`, `Remove` and `Replace` with `AnchorMovementType` and `OffsetChangeMappingType` overloads, `GetLineByNumber`/`GetLineByOffset`, `CreateAnchor`, an `UndoStack` you can share between documents, snapshots and readers, and `Changing`/`Changed`/`UpdateStarted`/`UpdateFinished` events.

Every API takes 0-based character offsets - a position between two characters, `0 <= offset <= TextLength`. `TextLocation(line, column)` is 1-based on both axes, and `ISegment` is the `(Offset, Length, EndOffset)` triple.

```csharp
using CodeBrix.Platform.UI.AdvancedTextEdit.Document;

TextDocument doc = Editor.Document;
TextAnchor mark = doc.CreateAnchor(doc.GetOffset(3, 1));          // start of line 3
mark.SurviveDeletion = true;
using (doc.RunUpdate())                                            // one undo group
{
    doc.Insert(0, "// generated\n");
    DocumentLine last = doc.GetLineByNumber(doc.LineCount);
    doc.Replace(last.Offset, last.Length, last.Length == 0 ? "// end" : doc.GetText(last) + " // end");
}
int lineThreeNow = mark.Line;                                      // anchor followed the insert
Editor.TextArea.Caret.Offset = mark.Offset;
Editor.TextArea.Caret.BringCaretToView();
doc.UndoStack.Undo();                                              // reverts both edits at once
```

`BeginUpdate`/`EndUpdate` nest, `RunUpdate()` returns a disposable, and everything inside becomes one undo group and one batch of change events. `DocumentLine` objects stay valid across edits; after deletion `IsDeleted` is true and `LineNumber`/`Offset` throw. Anchors are held by weak reference, so keep your own reference to one you care about.

A `TextDocument` is owned by the thread that created it and throws `InvalidOperationException` from any other thread. To load a large file on a worker, create the document there, call `SetOwnerThread(null)` on the worker and `SetOwnerThread(Thread.CurrentThread)` on the UI thread before assigning it to `Editor.Document`.

### The caret and the selection

`TextArea.Caret` and `TextArea.Selection` are the editing cursor.

```csharp
// caret and selection from code
Editor.TextArea.Caret.Offset = 42;
Editor.TextArea.Selection = Selection.Create(Editor.TextArea, 10, 20);
Editor.TextArea.SelectionChanged += (_, _) =>
    status.Text = Editor.TextArea.Selection.IsEmpty ? "" : Editor.SelectedText;
Editor.TextArea.Caret.PositionChanged += (_, _) =>
    pos.Text = $"Ln {Editor.TextArea.Caret.Line}, Col {Editor.TextArea.Caret.Column}";
```

Selections are immutable values: `Selection.Create(...)` returns a new one that you assign to `TextArea.Selection`. `Caret.Offset` and `Caret.Location` are cheap; `Caret.Position` validates the visual column and costs more.

Rectangular selection is on by default (`Options.EnableRectangularSelection`): Alt and drag with the mouse, or Alt+Shift with the arrow, Home and End keys. Double-click and drag selects whole words, triple-click selects whole lines.

### Syntax highlighting

`HighlightingManager.Instance` is process-wide and pre-loaded. Look a definition up by name or by extension, and assign it to `Editor.SyntaxHighlighting`; `null` means no coloring.

Built-in definitions, with the extensions they claim: `XmlDoc`, `C#` (`.cs`), `JavaScript` (`.js`), `HTML` (`.htm`, `.html`), `ASP/XHTML`, `Boo`, `Coco`, `CSS`, `C++` (`.c`, `.h`, `.cc`, `.cpp`, `.hpp`), `Java`, `Patch`, `PowerShell`, `PHP`, `Python` (`.py`, `.pyw`), `TeX`, `TSQL` (`.sql`), `VB`, `XML` (`.xml`, `.xsl`, `.xslt`, `.xsd`, `.manifest`, `.config`, `.addin`, `.xshd`, `.wxs`, `.wxi`, `.wxl`, `.proj`, `.csproj`, `.vbproj`, `.ilproj` and more MSBuild and XAML-style extensions), `MarkDown` (`.md`), `MarkDownWithFontSize` (`.md`) and `Json` (`.json`). Definitions are parsed lazily on first use, and the built-in color schemes assume a light surface.

Retheming a built-in definition is three lines:

```csharp
var def = HighlightingManager.Instance.GetDefinition("C#");
def.GetNamedColor("Comment").Foreground = new SimpleHighlightingBrush(Colors.Gray);
Editor.TextArea.TextView.Redraw();
```

XSHD is the highlighting definition format. Load your own from an embedded resource and register it under a name and a set of extensions:

```csharp
IHighlightingDefinition custom;
using (Stream s = GetType().Assembly.GetManifestResourceStream("MyApp.MyLang.xshd"))
using (XmlReader reader = XmlReader.Create(s))
    custom = HighlightingLoader.Load(reader, HighlightingManager.Instance);
HighlightingManager.Instance.RegisterHighlighting("MyLang", new[] { ".mylang" }, custom);
Editor.SyntaxHighlighting = custom;
```

Passing `HighlightingManager.Instance` as the resolver lets `<Import>` and `<Reference>` elements in your XSHD find the built-in definitions. Highlighting is also available without the editor: `DocumentHighlighter(TextDocument, IHighlightingDefinition)` returns a `HighlightedLine` per line, with `ToHtml`, `ToRichText` and `ToRichTextModel` conversions.

### Custom rendering

`Editor.TextArea.TextView` exposes three extension points: `LineTransformers` restyle text runs, `BackgroundRenderers` draw on a layer, and `ElementGenerators` replace text with custom elements.

A colorizer derives from `DocumentColorizingTransformer` and restyles ranges of the current line.

```csharp
sealed class TodoColorizer : DocumentColorizingTransformer
{
    protected override void ColorizeLine(DocumentLine line)
    {
        string text = CurrentContext.Document.GetText(line);
        int start = 0, index;
        while ((index = text.IndexOf("TODO", start, StringComparison.Ordinal)) >= 0)
        {
            ChangeLinePart(line.Offset + index, line.Offset + index + 4, element =>
            {
                element.TextRunProperties.SetFontWeight(FontWeights.Bold);
                element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(Colors.OrangeRed));
            });
            start = index + 4;
        }
    }
}
Editor.TextArea.TextView.LineTransformers.Add(new TodoColorizer());
```

A background renderer names the layer it draws on - `KnownLayer` is `Background`, `Selection`, `Text` or `Caret`, drawn in that order - and paints on an `SKCanvas` in device-independent pixels.

```csharp
sealed class LineMarker : IBackgroundRenderer
{
    public int LineNumber { get; set; } = 3;
    public KnownLayer Layer => KnownLayer.Background;
    public void Draw(TextView textView, SKCanvas canvas)
    {
        if (textView.Document == null || LineNumber > textView.Document.LineCount) return;
        DocumentLine line = textView.Document.GetLineByNumber(LineNumber);
        using var paint = new SKPaint { Color = new SKColor(255, 235, 59, 90) };
        foreach (Rect r in BackgroundGeometryBuilder.GetRectsForSegment(textView, line, true))
            canvas.DrawRect((float)r.X, (float)r.Y, (float)r.Width, (float)r.Height, paint);
    }
}
Editor.TextArea.TextView.BackgroundRenderers.Add(new LineMarker());
```

Call `TextView.InvalidateLayer(KnownLayer.Background)` after changing renderer state, and `Redraw()` or `Redraw(offset, length)` when the text styling changed.

### Code completion and insight

The completion and insight popups are anchored to the text area's `XamlRoot` and are never focused, so typing keeps flowing into the editor. While one is open, the arrow, page and Home/End keys move in the list, Tab and Enter insert, and Escape closes.

```csharp
sealed class WordData : ICompletionData
{
    public WordData(string text) { Text = text; }
    public ImageSource? Image => null;
    public string Text { get; }
    public object? Content => Text;
    public object? Description => "Inserts " + Text;
    public double Priority => 0;
    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs e)
        => textArea.Document.Replace(completionSegment, Text);
}

CompletionWindow? completionWindow;
Editor.TextArea.TextEntered += (_, e) =>
{
    if (e.Text != ".") return;
    completionWindow = new CompletionWindow(Editor.TextArea);
    IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
    data.Add(new WordData("Length"));
    data.Add(new WordData("ToString"));
    completionWindow.Closed += (_, _) => completionWindow = null;
    completionWindow.Show();
};
Editor.TextArea.TextEntering += (_, e) =>
{
    if (e.Text.Length > 0 && completionWindow != null && !char.IsLetterOrDigit(e.Text[0]))
        completionWindow.CompletionList.RequestInsertion(EventArgs.Empty);
    // leave e.Handled false so the typed character is still inserted
};
```

`CompletionList.IsFiltering` defaults to true, which filters the list by substring; false selects by prefix instead. `InsightWindow(TextArea)` is the tool-tip style popup at the caret - set `Content` to a string or any `UIElement`, then `Show()`. `OverloadInsightWindow` binds to an `IOverloadProvider` and cycles overloads with the arrow keys.

### Snippets

A `Snippet` is a tree of `SnippetElement` objects. `Snippet.Insert(TextArea)` inserts it at the caret, replacing the selection, inside one undo group; if it contains replaceable fields it then enters interactive mode, where Tab and Shift+Tab jump between fields, typing into a field updates every bound copy live, and Enter or Escape ends the mode.

```csharp
// for (int i = 0; i < |count|; i++) { <caret> }  with "i" bound in 3 places
var loopVar = new SnippetReplaceableTextElement { Text = "i" };
var count   = new SnippetReplaceableTextElement { Text = "count" };
var snippet = new Snippet
{
    Elements =
    {
        new SnippetTextElement { Text = "for (int " }, loopVar,
        new SnippetTextElement { Text = " = 0; " },
        new SnippetBoundElement { TargetElement = loopVar },
        new SnippetTextElement { Text = " < " }, count,
        new SnippetTextElement { Text = "; " },
        new SnippetBoundElement { TargetElement = loopVar },
        new SnippetTextElement { Text = "++)\n{\n\t" },
        new SnippetCaretElement(),
        new SnippetTextElement { Text = "\n}" },
    }
};
snippet.Insert(Editor.TextArea);
```

The element types are `SnippetTextElement`, `SnippetReplaceableTextElement`, `SnippetBoundElement` (with a `TargetElement` and an overridable `ConvertText`), `SnippetCaretElement`, `SnippetSelectionElement` and `SnippetAnchorElement`. Derive from `SnippetElement` for your own.

### Folding

Folding is opt-in. `FoldingManager.Install` adds a folding margin to `LeftMargins` and a folding element generator to the `TextView`.

```csharp
FoldingManager foldingManager = FoldingManager.Install(Editor.TextArea);
var xmlFolding = new XmlFoldingStrategy();
xmlFolding.UpdateFoldings(foldingManager, Editor.Document);   // re-run after edits
// hand-made foldings:
foldingManager.UpdateFoldings(new[] { new NewFolding(0, 120) { Name = "header" } }, -1);
// BEFORE assigning a different Editor.Document:
FoldingManager.Uninstall(foldingManager);
```

`UpdateFoldings` takes foldings sorted by `StartOffset` and keeps the `IsFolded` state of matching sections; pass `-1` as the error offset when there was no parse error. `XmlFoldingStrategy` is the only strategy in the box; a brace-based strategy is a small class you write against the same interface, and the reference application ships one.

### Search

The search panel is also opt-in. One call binds Ctrl+F to open it (seeded with a single-line selection), F3 and Shift+F3 to find next and previous, and Escape to close.

```csharp
SearchPanel panel = SearchPanel.Install(Editor.TextArea);
```

The panel appears at the top right over the text and highlights every match through a background renderer. It exposes `UseRegex`, `MatchCase`, `WholeWords`, `SearchPattern`, the marker brushes, `Open()`, `Close()`, `Reactivate()`, `FindNext()`, `FindPrevious()` and `Uninstall()`, plus a `Localization` object whose virtual text properties you override to translate the buttons and messages.

Searching without the panel goes through the strategy factory, which supports `SearchMode.Normal`, `RegEx` and `Wildcard`:

```csharp
ISearchStrategy strategy = SearchStrategyFactory.Create("foo", true, false, SearchMode.Normal);
foreach (ISearchResult hit in strategy.FindAll(Editor.Document, 0, Editor.Document.TextLength))
    Editor.Document.Replace(hit.Offset, hit.Length, "bar");   // wrap in RunUpdate() for one undo step
```

An invalid pattern throws `SearchPatternException`.

### Indentation

`IIndentationStrategy` has `IndentLine(TextDocument, DocumentLine)`, which runs when Enter inserts a new line, and `IndentLines(TextDocument, int, int)`, which runs for `AdvancedTextEditCommands.IndentSelection` (Ctrl+I). Assign one to `Editor.TextArea.IndentationStrategy`.

`DefaultIndentationStrategy` copies the previous line's leading whitespace, and its `IndentLines` does nothing. `CSharpIndentationStrategy` is brace-aware and takes `IndentationString` from the options when you construct it with them. Tab and Shift+Tab indent and unindent the selected lines using `Options.IndentationString`.

### Options

`Editor.Options` is an `AdvancedTextEditOptions`, shared with the `TextArea` and `TextView`, raising change notifications that re-render.

```text
bool ShowSpaces                       false     draw a dot for spaces
bool ShowTabs                         false     draw an arrow for tabs
bool ShowEndOfLine                    false     draw a paragraph mark
bool ShowBoxForControlCharacters      true      boxed names for controls
bool EnableHyperlinks                 true      clickable URLs
bool EnableEmailHyperlinks            true      clickable mailto
bool RequireControlModifierForHyperlinkClick  true
int IndentationSize                   4         width of one indent (1..1000)
bool ConvertTabsToSpaces              false     indent with spaces
string IndentationString              (derived) "\t" or IndentationSize spaces
string GetIndentationString(int column)
bool CutCopyWholeLine                 true      empty selection = whole line
bool AllowScrollBelowDocument         false
double WordWrapIndentation            0         extra indent of wrapped lines
bool InheritWordWrapIndentation       true
bool EnableRectangularSelection       true      Alt box selection
bool EnableVirtualSpace               false     caret past line end
bool ShowColumnRuler                  false     vertical guide line
int ColumnRulerPosition               80
bool HighlightCurrentLine             false     tinted band on the caret line
                                                (built-in default colors)
bool HideCursorWhileTyping            true
bool AllowToggleOverstrikeMode        false     lets Insert toggle overtype
```

`Options.EnableTextDragDrop` and `Options.EnableImeSupport` are accepted, but the features behind them are not available.

### Commands and key bindings

Commands are identity tokens - an `EditorCommand` carries a `Name` and a list of default gestures - and the default input handlers bind the implementations. `EditorCommands` covers Copy (Ctrl+C, Ctrl+Insert), Cut (Ctrl+X, Shift+Delete), Paste (Ctrl+V, Shift+Insert), SelectAll (Ctrl+A), Undo (Ctrl+Z), Redo (Ctrl+Y), DeleteNextCharacter (Delete), DeleteNextWord (Ctrl+Delete), Backspace, DeletePreviousWord (Ctrl+Back), EnterParagraphBreak (Enter), EnterLineBreak (Shift+Enter), TabForward (Tab), TabBackward (Shift+Tab) and the caret-movement family. `EditorCommands.Delete` has no gesture and only runs with a selection, and `EditorCommands.ToggleInsert` is defined but not bound.

`AdvancedTextEditCommands` adds ToggleOverstrike (Insert, a no-op unless `Options.AllowToggleOverstrikeMode` is true), DeleteLine (Ctrl+D), IndentSelection (Ctrl+I), and the whitespace and case commands - RemoveLeadingWhitespace, RemoveTrailingWhitespace, ConvertToUppercase, ConvertToLowercase, ConvertToTitleCase, InvertCase, ConvertTabsToSpaces, ConvertSpacesToTabs, ConvertLeadingTabsToSpaces and ConvertLeadingSpacesToTabs - which are bound but carry no default gesture, so give them one or execute them from code.

```csharp
var upper = new EditorCommand("MyUpper", new KeyGesture(VirtualKey.U, VirtualKeyModifiers.Control));
Editor.TextArea.CommandBindings.Add(new EditorCommandBinding(upper,
    (s, e) => { Editor.SelectedText = Editor.SelectedText.ToUpperInvariant(); e.Handled = true; },
    (s, e) => { e.CanExecute = !Editor.TextArea.Selection.IsEmpty; e.Handled = true; }));
Editor.TextArea.InputBindings.Add(new KeyBinding(upper, VirtualKey.U, VirtualKeyModifiers.Control));
```

For modal input - a picker that swallows keys - derive from `TextAreaStackedInputHandler`, override `OnPreviewKeyDown`, and push and pop it on the text area.

### How it renders

One paint pass runs on a Skia canvas: Background-layer renderers, then the selection, then each visual line's element backgrounds, text and decorations, then Caret-layer renderers. Only the visual lines intersecting the viewport are constructed, and a height tree tracks line heights, so a very large document opens and scrolls like a small one.

Text is shaped and measured by the family's shared text engine, so glyphs, widths and line heights are identical to a `TextBlock` in the same font; the editor never falls back to a system font. The editor manages its own scrolling with two `ScrollBar` controls synced to the `TextView`'s scroll offsets; there is no `ScrollViewer` inside.

### What the control does not include

- No built-in language services: no parsers, no semantic completion, no error squiggles. `ICompletionData`, `IOverloadProvider` and the rendering hooks are the seams for your own.
- No brace-folding strategy in the box - only `XmlFoldingStrategy` - and no indentation strategy beyond `DefaultIndentationStrategy` and `CSharpIndentationStrategy`.
- No find-and-replace user interface: the search panel finds only, and replace goes through `ISearchStrategy` plus `Document.Replace`.
- No printing, no minimap, no multiple carets, no split views, no UI automation peer, and no themes or resource dictionaries to restyle the chrome. The editor, search panel and completion popups build their visuals in code and expose brush properties instead.
- No XAML-declared key bindings or commands; bindings are added from code.
- XSHD is the only highlighting definition format.
- No IME composition for CJK input methods, and no drag-and-drop of selected text, although the two option flags exist.

## Per-head notes

None. The control is pure managed code with no native dependency, and behaves identically on all six heads, including the frame-buffer head.

## Pitfalls

- Do not wrap the editor in a `ScrollViewer`. It manages its own scrolling and virtualization; give it a bounded height and width - a star row, not `Auto`.
- `Editor.Text = "..."` resets the caret to offset 0 and clears the undo stack. Use `Document.Replace(...)` when the user should be able to undo the change.
- Highlighting definitions are looked up by name or by extension, and the result is `null` for unknown ones; `null` on `SyntaxHighlighting` silently disables coloring. `GetDefinition("CSharp")` is null - the name is `C#` - and extensions include the dot, as in `.cs`.
- `MarkDownWithFontSize` is registered after `MarkDown` for `.md`, so `GetDefinitionByExtension(".md")` returns the font-size variant.
- `HighlightingManager` is process-wide: registering an existing name replaces it for every editor in the application. Guard against re-registering when pages are recreated.
- The search panel is not installed by default; Ctrl+F and F3 do nothing until you call `SearchPanel.Install(Editor.TextArea)`.
- `FoldingManager` is bound to the text area's current document. Call `FoldingManager.Uninstall(manager)` before assigning a new `Editor.Document`, then install again.
- `CompletionWindow.Show()` throws if the text area is not yet in a visual tree, and a closed window cannot be shown again. Construct a new window per completion session, and drop your reference in the `Closed` event.
- In `TextEntering`, do not set `e.Handled = true` unless you really want to swallow the typed character; the completion pattern relies on leaving it alone.
- `TextDocument` is single-thread-owned: access from another thread throws `InvalidOperationException`. `SetOwnerThread` hands it over.
- `TextAnchor` is weakly referenced, so store it in a field rather than a local you drop. An anchor with `SurviveDeletion` false throws on `Offset` after its text is deleted; check `IsDeleted`.
- `Selection` objects are immutable: assign the result of `Selection.Create(...)` to `TextArea.Selection`.
- Every `ScrollTo*` call requires that layout has run, so calling one from a constructor or before the page is loaded does nothing useful. Defer to `Loaded` or the dispatcher.
- `TextView.VisualLines` throws `VisualLinesInvalidException` while lines are invalid; call `EnsureVisualLines()` first.
- `IsReadOnly = true` replaces `TextArea.ReadOnlySectionProvider`, so toggling it discards a `TextSegmentReadOnlySectionProvider` you installed.
- The defaults are a white surface with black text, and the built-in XSHD colors assume a light background. In a dark application, set `Background` and `Foreground` on the editor and retheme the definition's named colors, or ship your own XSHD.
- Batch edits in `doc.RunUpdate()` or `Editor.DeclareChangeBlock()`: one re-layout, one undo step, one `TextChanged`.
- Folding strategies re-parse the whole document, so run them on a timer or after idle rather than on every key. Colorizers run per visible line on every visual-line rebuild, so keep `ColorizeLine` cheap - no regular expression compilation inside it, and cache brushes.
- `HighlightingManager.HighlightingDefinitions` returns a copy on every call; fetch it once for a `ComboBox`.
- Setting `Text` replaces the whole rope. For appending output use `AppendText` or `doc.Insert(doc.TextLength, ...)`.

## Related

- [TextLayout](TextLayout.md) - the shared text engine the editor renders through; it arrives automatically with this package and is also usable on its own
- [AdvancedTextEditDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/AdvancedTextEditDemo) in CodeBrix.Platform - a working editor on six heads: Open and Save, undo and redo, a highlighting selector over `HighlightingManager.Instance.HighlightingDefinitions`, a custom XSHD definition, dot-triggered completion, the search panel, XML and brace folding refreshed by a timer, C# indentation, and a reflection-driven property pane
- [Views and styling](../06-views-and-styling.md) - where a large control like this sits in a page's layout

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.AdvancedTextEdit/AGENT-README.txt) |
| Add-in source | [src/AddIns/Platform.UI.AdvancedTextEdit](https://github.com/ellisnet/CodeBrix.Platform/tree/main/src/AddIns/Platform.UI.AdvancedTextEdit) |
| Tests, which double as API examples | [src/AddIns/Platform.UI.AdvancedTextEdit.Tests](https://github.com/ellisnet/CodeBrix.Platform/tree/main/src/AddIns/Platform.UI.AdvancedTextEdit.Tests) |
| Reference application | [samples/CodeBrixPlatform/AdvancedTextEditDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/AdvancedTextEditDemo) |
| Package | [`CodeBrix.Platform.AdvancedTextEdit.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.AdvancedTextEdit.ApacheLicenseForever) |

---

**Where to go next**

- [TextLayout](TextLayout.md) - the text engine underneath, for shaping and measuring text without a control
- [AppSettings](AppSettings.md) - remembering the editor's font, tab size and last file between runs
- [All add-ins](../08-add-ins.md) - the whole set at a glance
