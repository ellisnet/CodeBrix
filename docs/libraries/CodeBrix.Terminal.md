<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Terminal</sub>

# CodeBrix.Terminal

**An in-memory terminal emulation engine with Unicode text support: a virtual terminal (VT100, VT220, VT400 and xterm compatible) with a complete ANSI and DEC escape-sequence parser, terminal buffer and scrollback management, mouse-tracking protocols, search and selection services, keyboard-to-VT input encoding, and a full Unicode text toolkit.** It is renderer-agnostic and headless: you feed it bytes or text, and you read the resulting character buffer to paint it however you like. Use it to build terminal emulator user interfaces, to process terminal output programmatically, or to add terminal behavior to any .NET 10 application, including a CodeBrix.Platform application.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Terminal](https://github.com/ellisnet/CodeBrix.Terminal) |
| **Packages** | [`CodeBrix.Terminal.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Terminal.MitLicenseForever) |
| **License** | MIT, and the package requires license acceptance; see [License](#license) |
| **Requires** | .NET 10 or later; no NuGet dependencies |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Anywhere .NET 10 runs; the pseudo-terminal helpers are Unix and macOS only |

## What it does

- Parses ANSI and DEC escape sequences: VT100, VT220, VT400 and xterm - cursor movement (CUP, CUU, CUD, CUF, CUB), scroll regions and margins (DECSTBM, DECSLRM), scrolling (SU, SD), character, line and column insertion and deletion (ICH, DCH, DECIC, DECDC, IL, DL), erasing (ED, EL, ECH), rectangular-area operations (DECCRA, DECFRA, DECERA, DECSERA, DECRQCRA), attributes (SGR), device and mode reporting (DSR, DA1, DA2, DECRQSS), mode setting (DECSET, DECRESET) including origin mode, wraparound and bracketed paste, and G0 to G3 character-set designation.
- Manages the terminal buffer with scrollback history, and the alternate screen buffer.
- Tracks text attributes - bold, dim, italic, underline, blink, inverse, invisible, crossed-out - and color across the 8-color, 16-color and 256-color palettes.
- Implements the mouse-tracking protocols (X10, VT200, button-event tracking, any-event) and their encodings (X10, UTF-8, SGR, URXVT).
- Manages tab stops with a configurable default width, and character-set translation tables including DEC line drawing.
- Routes window-manipulation commands to a host delegate.
- Encodes keyboard input into VT byte sequences, aware of application-cursor mode.
- Resizes with reflow, both wider and narrower.
- Provides search and selection services over the scrollback.
- Forks and executes a pseudo-terminal, sets its window size and reports available bytes, on Unix and macOS.
- Provides a Unicode text toolkit in `CodeBrix.Terminal.Text`: character classification, case conversion with special-case and locale support, simple case folding, range-table lookups, code-point handling with terminal column widths, and the `ustring` UTF-8 string type.

## When to use it

Use it whenever your application has to understand terminal output rather than merely display bytes: a terminal control in a desktop application, a remote-shell client, a log viewer that has to honor escape sequences, or a test harness that asserts on what a program painted. It has no dependencies beyond the .NET runtime and no rendering code, so it drops into any host and any drawing stack.

Do not expect it to do the parts that belong to the host. It does not provide a graphical or console terminal *control*: there is no widget and no drawing code, and you write the renderer. It does not run a shell or command interpreter - the engine emulates the display side only, and spawning the process is your job, with the pseudo-terminal helpers assisting on Unix and macOS but not on Windows. It speaks no network protocol, so there is no transport here. It does not read the keyboard: the key encoder encodes key identifiers you supply, and capturing key events is the host's job. It does not render 24-bit color, because the color model tops out at the 256-color palette. It does not integrate with the clipboard, implement the DEC Locator mouse protocol or VT200 highlight mouse mode, render fonts, shape text or look up glyphs, or serialize terminal state.

## Getting started

```bash
dotnet add package CodeBrix.Terminal.MitLicenseForever
```

The package ID is `CodeBrix.Terminal.MitLicenseForever`, not `CodeBrix.Terminal`. One package gives you both namespaces in one assembly: `CodeBrix.Terminal.Engine` (the emulation engine) and `CodeBrix.Terminal.Text` (the Unicode text utilities).

```csharp
using CodeBrix.Terminal.Engine;   // Terminal, TerminalOptions,
                                  //   ITerminalDelegate, Buffer,
                                  //   BufferLine, BufferSet, CharData,
                                  //   CharacterAttribute, FLAGS, Color,
                                  //   EscapeSequenceParser, SearchService,
                                  //   SelectionService, Pty,
                                  //   TerminalKeyEncoder, CircularList<T>,
                                  //   RuneExt, RuneHelper, Renderer.
using CodeBrix.Terminal.Text;     // ustring, Utf8, Unicode.
using System.Drawing;             // Point -- SelectionService.Start/End and
                                  //   SearchSnapshot.SearchResult use it.
```

`Rune` and `RuneExtensions` are declared in the `System` namespace, so they need no using of their own. For terminal emulation only, take the first using; for Unicode text processing only, the second; add `System.Drawing` when you touch selection or search coordinates. One alias is worth writing down at the top of any file that names the buffer type, because `Buffer` collides with `System.Buffer`:

```csharp
using TerminalBuffer = CodeBrix.Terminal.Engine.Buffer;
```

There is no registration call and no feature flag. Create a terminal, feed it, read the buffer.

```csharp
using System;
using CodeBrix.Terminal.Engine;

var terminal = new Terminal(null,
    new TerminalOptions { Cols = 80, Rows = 25 });

terminal.Feed("Hello, Terminal!\r\n");
terminal.Feed("\x1b[1;31m");            // bold red foreground
terminal.Feed("Red bold text\r\n");
terminal.Feed("\x1b[0m");               // reset attributes

var line = terminal.Buffer.Lines[terminal.Buffer.YBase + 0];
for (int col = 0; col < terminal.Cols; col++)
{
    var ch = line[col];
    if (ch.Code != 0)
        Console.Write((char)ch.Code);
}
Console.WriteLine();
```

A null delegate is fine for headless use. Dimensions are clamped to the internal minimums of two columns by one row.

## Key concepts

### The Terminal class

`Terminal(ITerminalDelegate terminalDelegate = null, TerminalOptions options = null)` is the entry point, and both arguments are optional. Data goes in through `Feed(string text)`, `Feed(byte[] data, int len = -1)` (raw bytes; -1 means all of them) and `Feed(IntPtr data, int len = -1)` for unmanaged memory.

Its core properties are `Delegate`, `Buffer` (the *active* buffer), `Buffers` (normal and alternate), `Options`, `ControlCodes`, `Title`, `IconTitle`, `Cols`, `Rows` and `Charset`. The terminal *mode* properties - `MarginMode`, `OriginMode`, `Wraparound`, `ReverseWraparound`, `ApplicationCursor`, `ApplicationKeypad`, `Allow80To132`, `SendFocus`, `CursorHidden`, `BracketedPasteMode`, `SavedCols`, `MouseMode` and `MouseProtocol` - are read-only to consumers: their setters are internal, and the terminal updates them itself in response to the escape sequences you feed it. `InsertMode` and `CurAttr` are writable public fields.

Cursor and editing operations mirror the escape sequences: `SetCursor(int col, int row)` (note the column-then-row order), `ShowCursor`, `SaveCursor`, `RestoreCursor`, the `CursorUp`/`Down`/`Forward`/`Backward` family, `LineFeed`, `CarriageReturn`, `Backspace`, `DeleteChars`, `InsertColumn` and `DeleteColumn`. `Reset()` is a full reset, equivalent to feeding `"\x1bc"`, and `SoftReset()` is DECSTR. `Resize(cols, rows)`, `SetScrollRegion(top, bottom)` and `SetCursorStyle(style)` complete the control surface, and `SendResponse` writes a reply back through the delegate.

Three events tell a host what happened: `Scrolled` (the new viewport position, on every viewport move), `DataEmitted` and `LineFeedEvent`.

### TerminalOptions

```csharp
public class TerminalOptions
{
    public int Cols, Rows;                  // fields; default 80 x 25
    public bool ConvertEol = true, CursorBlink;
    public string TermName;                 // default "xterm"
    public CursorStyle CursorStyle;
    public bool ScreenReaderMode;
    public int? Scrollback { get; set; }    // default 1000 lines
    public int? TabStopWidth { get; set; }  // default 8
}
```

`Scrollback` and `TabStopWidth` are settable at any time, but set them *before* constructing the `Terminal`: the buffer sizes itself and lays out tab stops from those values at construction.

`ConvertEol` matters more than it looks. The default, true, treats a bare line feed as a carriage-return-plus-line-feed on input. A host whose data source already emits `\r\n` - a remote shell, most pseudo-terminals - must set `ConvertEol = false`, or every line break doubles into a blank line.

### The delegate

```csharp
public interface ITerminalDelegate
{
    void ShowCursor(Terminal source);
    void SetTerminalTitle(Terminal source, string title);
    void SetTerminalIconTitle(Terminal source, string title);
    void SizeChanged(Terminal source);
    void Send(byte[] data);
    string WindowCommand(Terminal source, WindowManipulationCommand command,
                         params int[] args);
    bool IsProcessTrusted();
}
```

`SimpleTerminalDelegate` provides virtual no-op implementations of all seven members; derive from it and override only what you need. `Send(byte[])` is how the terminal hands you data to write back to the process or the remote host - device reports, mouse events, and the responses you trigger with `SendResponse`. Return a string from `WindowCommand` when the command is one of the `Report*` members, and null for the rest.

### Buffer, lines and cells

`Buffer` is the terminal screen plus its scrollback. `Lines` is a `CircularList<BufferLine>`; `X`, `YBase` and `YDisp` are fields; `Y`, `ScrollTop`, `ScrollBottom`, `MarginLeft`, `MarginRight`, `HasScrollback` and `IsCursorInViewport` are properties; and `GetChar`, `GetBlankLine`, `Clear`, `Resize`, `SetMargins`, `SaveCursor`, `RestoreCursor`, `FillViewportRows`, `TranslateBufferLineToString` and the tab-stop family complete it. `TranslateBufferLineToString` answers a `ustring`, so call `.ToString()` when you need a `System.String`.

`YBase` is where the live screen starts and `YDisp` is where the viewport is: they differ whenever the user has scrolled back. The cell at visible position (row, col) is `terminal.Buffer.Lines[terminal.Buffer.YDisp + row][col]`.

`CharData` is one cell:

```csharp
int Attribute;                          // packed styling (fg, bg, flags)
Rune Rune;                              // the code point
int Width;                              // display width: 0, 1 or 2
                                        //   (0 = the continuation cell of
                                        //   a wide character)
int Code;                               // the code point as an int
```

`IsBlank` marks a never-written or erased cell: paint the background only, because its `Rune` is U+0200 and not a space. `BufferSet` holds the normal and alternate screens together, with `Active`, `IsAlternateBuffer`, an `Activated` event carrying the before and after buffers, and the two activation methods.

### Attribute packing

A cell's styling is a single packed `int`. Decode it with `CharacterAttribute` rather than hand-rolling the shifts.

```csharp
public static class CharacterAttribute
{
    public const int DefaultColorIndex = 256;
    public const int InvertedDefaultColorIndex = 257;
    public static UnpackedAttribute Unpack(int attribute);
    public static string ToSGR(int attribute);   // back to an SGR string
}

// Because UnpackedAttribute deconstructs, this is the idiomatic call:
var (fg, bg, flags) = CharacterAttribute.Unpack(cell.Attribute);
```

`FLAGS` is `BOLD = 1, UNDERLINE = 2, BLINK = 4, INVERSE = 8, INVISIBLE = 16, DIM = 32, ITALIC = 64, CrossedOut = 128`. The foreground and background values are indices into the 256-entry palette, *except* for the two sentinels `DefaultColorIndex` and `InvertedDefaultColorIndex`, which are not valid indices and mean "use your default" and "use the inverted default". Prefer those two constants over the same values on the `Renderer` class - they say what they mean.

### Colors

`Color` carries `Red`, `Green` and `Blue` as byte fields, with `DefaultAnsiColors` (the 256-entry palette), `DefaultForeground` and `DefaultBackground` as statics. The palette is laid out as the eight standard colors, then their eight bright variants, then the 6-by-6-by-6 color cube, then the grayscale ramp. `Terminal.MatchColor(r, g, b)` returns the nearest palette index for an arbitrary triple. This is `CodeBrix.Terminal.Engine.Color`, not `System.Drawing.Color`.

### Damage tracking

The engine accumulates a dirty span of visible rows, and the renderer reads and clears it: `GetUpdateRange(out int startY, out int endY)` then `ClearUpdateRange()`. A `startY` of `int.MaxValue` (with `endY` of -1) means nothing is dirty. The contract is renderer-clears: nothing resets the span except `ClearUpdateRange`. Repainting the whole surface each frame also works at modest terminal sizes - damage tracking is the optimization, not a requirement.

### Keyboard input

`TerminalKeyEncoder` translates key presses into the byte sequences a terminal application expects, so a host does not hand-roll the mapping. `TerminalKey` and `TerminalModifiers` are platform-neutral: map your native key events onto them.

```csharp
public static class TerminalKeyEncoder
{
    public static string Encode(TerminalKey key, TerminalModifiers modifiers,
                                bool applicationCursor = false);
    public static string EncodeSpecial(TerminalKey key,
                                bool applicationCursor = false);
    public static string EncodeComposed(int unicodeCodePoint,
                                TerminalModifiers modifiers);
}
```

A raw-key host calls `Encode` for everything. A composed-text host uses `EncodeSpecial` for named non-printables and `EncodeComposed` for printable input, which is correct on any keyboard layout. All three return null for keys that produce no terminal input. The rules applied are the conventional ones: control chords become C0 control codes, Alt prefixes an escape, Shift+Tab is back-tab, and the arrows, Home and End honor application-cursor mode - so always pass `terminal.ApplicationCursor`.

### Mouse tracking

`MouseMode` is `Off, X10, VT200, ButtonEventTracking, AnyEvent` and `MouseProtocolEncoding` is `X10, UTF8, SGR, URXVT`. Both are set by the application running inside the terminal, through the sequences you feed in, and read back from `terminal.MouseMode` and `terminal.MouseProtocol`.

```csharp
int flags = terminal.EncodeMouseButton(button, release, shift, meta,
                                       control);
terminal.SendEvent(flags, x, y);          // press/release
terminal.SendMouseMotion(flags, x, y);    // motion
```

Gate the calls on the mode, using the extension methods `SendButtonPress`, `SendButtonRelease`, `SendButtonTracking`, `SendMotionEvent` and `SendsModifiers`: only send motion when `terminal.MouseMode.SendMotionEvent()` is true, and so on.

### Selection and search

`SelectionService` holds a selection over the buffer with `Active`, `Start`, `End`, a `SelectionChanged` event, and `StartSelection`, `SetSoftStart`, `ShiftExtend`, `DragExtend`, `SelectAll`, `SelectNone`, `SelectRow`, `SelectWordOrExpression`, `GetSelectedText()` and `GetSelectedLines()`. `Start` and `End` are buffer-absolute, so a selection survives scrolling, while the method inputs take viewport-relative rows. `End` is column-*exclusive*: `GetSelectedText()` copies up to but not including `End.X` on the end row, and `Start == End` yields empty text, so a plain click carries no text.

`SearchService` exposes an `Invalidated` event, `GetSnapshot()` and `Invalidate()`. A `SearchSnapshot` carries `Text`, `LastSearch`, `LastSearchResults`, `CurrentSearchResult`, `FindText(string txt)` (which returns the match count), `FindNext()`, `FindPrevious()` and the nested `SearchSnapshot.SearchResult`. The snapshot is a point-in-time copy: take a new one after the buffer changes, or subscribe to `Invalidated`.

### Pseudo-terminals

```csharp
public struct UnixWindowSize { public short row, col, xpixel, ypixel; }

ForkAndExec(string programName, string[] args, string[] env,
            out int master, UnixWindowSize winSize)
SetWinSize(int fd, ref UnixWindowSize winSize)
AvailableBytes(int fd, ref long size)
```

`Terminal.GetEnvironmentVariables(termName)` builds a suitable environment array. These three are calls into the C library and work on Unix and macOS only. On Windows, drive the terminal from redirected `System.Diagnostics.Process` streams (or a console-pseudo-terminal wrapper of your own) and `Feed()` what you read.

### Unicode text

`ustring` is an immutable UTF-8 string, created with `ustring.Make(...)` or implicitly from a `System.String`. Its three size measures are different and all useful: `Length` counts *bytes*, `RuneCount` counts code points, and `ConsoleWidth` counts terminal columns. Slicing takes byte offsets unless the member name says otherwise (`RuneSubstring`, `RuneAt`), and every "mutating" method returns a new `ustring`.

`Rune` is one code point, with `Value`, `IsValid`, the surrogate predicates, `IsNonSpacing`, the statics `Error`, `MaxRune` and `ReplacementChar`, and `Rune.ColumnWidth(r)`, which answers 0 for a non-spacing mark, 1, or 2 for a wide character. Classification and case conversion are *static* methods - `Rune.IsLetter(r)`, `Rune.ToUpper(r)`, `Rune.SimpleFold(r)` - not instance methods. `Utf8` holds the UTF-8 primitives, and `Unicode` holds classification over raw code points plus the nested `Category`, `Script`, `Property`, `RangeTable`, `SpecialCase` and `Case` types. `Unicode.Version` reports the Unicode release the tables came from.

### Writing a renderer

The contract a host renderer follows has eight points:

1. **Indexing.** The cell at visible position (row, col) is `Lines[Buffer.YDisp + row][col]`. Paint from `YDisp`, or scrolled-back views render the wrong rows.
2. **Blank cells.** Never draw `CharData.Rune` verbatim: a never-written cell carries U+0200 and paints a stray glyph. When `ch.IsBlank` is true, paint only the background, and skip wide-character continuation cells, where `ch.Width == 0`.
3. **Attributes.** Unpack the packed int, resolve the foreground and background indices against `Color.DefaultAnsiColors`, and handle the two sentinels.
4. **Damage tracking.** Read the dirty span, paint it, then call `ClearUpdateRange()` - nothing else resets it.
5. **Viewport.** Wheel events call `ScrollLines(±lines)`, typing calls `ScrollToBottom()`, `IsAtBottom` decides whether to follow new output, and the `Scrolled` event keeps a scrollbar in sync.
6. **Selection.** The endpoints are buffer-absolute and `End` is column-exclusive; draw highlights with the same convention, or the highlight and the copied text disagree by one cell.
7. **Cursor and size.** Paint the cursor at `(Buffer.X, Buffer.Y)` relative to `YBase`, honoring `CursorHidden` and `Options.CursorStyle`. When your surface changes size, call `terminal.Resize(cols, rows)` and push the new size to the process too.
8. **Keyboard.** Do not invent escape sequences: run key events through `TerminalKeyEncoder` and hand the result to your transport, always passing `terminal.ApplicationCursor`.

## Examples

Painting a frame: the damage span, decoded attributes, and the two cells you must skip.

```csharp
using CodeBrix.Terminal.Engine;

var terminal = new Terminal(null,
    new TerminalOptions { Cols = 120, Rows = 50 });
terminal.Feed("\x1b[1;3mHello\x1b[0m\r\n");

terminal.GetUpdateRange(out int startY, out int endY);
if (startY != int.MaxValue)
{
    for (int row = startY; row <= endY; row++)
    {
        var line = terminal.Buffer.Lines[terminal.Buffer.YDisp + row];
        for (int col = 0; col < terminal.Cols; col++)
        {
            var ch = line[col];

            var (fg, bg, flags) = CharacterAttribute.Unpack(ch.Attribute);

            var background = bg == CharacterAttribute.DefaultColorIndex
                ? Color.DefaultBackground
                : Color.DefaultAnsiColors[bg];

            // Paint the cell background first...
            if (ch.IsBlank || ch.Width == 0)
                continue;               // nothing more to draw here

            var foreground = fg == CharacterAttribute.DefaultColorIndex
                ? Color.DefaultForeground
                : Color.DefaultAnsiColors[fg];

            bool bold = flags.HasFlag(FLAGS.BOLD);
            bool italic = flags.HasFlag(FLAGS.ITALIC);
            bool inverse = flags.HasFlag(FLAGS.INVERSE);

            // Draw ch.Rune with foreground/background/bold/italic/inverse,
            // advancing ch.Width columns.
        }
    }
    terminal.ClearUpdateRange();
}
```

A custom delegate: where the terminal's outbound bytes, its title changes and its window commands go.

```csharp
using System;
using CodeBrix.Terminal.Engine;

public class MyTerminalDelegate : SimpleTerminalDelegate
{
    public override void Send(byte[] data)
    {
        // Write these bytes to the process or remote host
        Console.WriteLine($"Terminal sent {data.Length} bytes");
    }

    public override void SizeChanged(Terminal source)
    {
        Console.WriteLine($"Resized to {source.Cols}x{source.Rows}");
    }

    public override void SetTerminalTitle(Terminal source, string title)
    {
        Console.Title = title;
    }

    public override string WindowCommand(Terminal source,
        WindowManipulationCommand command, params int[] args)
    {
        // Return a report string for the Report* commands; null otherwise
        return command == WindowManipulationCommand.ReportWindowTitle
            ? source.Title
            : null;
    }
}

var terminal = new Terminal(new MyTerminalDelegate(),
    new TerminalOptions { Cols = 120, Rows = 40 });
```

Scrollback, selection and copy - including the one method whose argument order is reversed.

```csharp
using System;
using System.Drawing;
using CodeBrix.Terminal.Engine;

var terminal = new Terminal(null,
    new TerminalOptions { Cols = 80, Rows = 10, Scrollback = 500 });

for (int i = 0; i < 100; i++)
    terminal.Feed($"line {i}\r\n");

// Scroll back five lines and check where we are
terminal.ScrollLines(-5);
Console.WriteLine($"at bottom: {terminal.IsAtBottom}, " +
                  $"YDisp={terminal.Buffer.YDisp}, " +
                  $"YBase={terminal.Buffer.YBase}");

var selection = new SelectionService(terminal);
selection.StartSelection(row: 0, col: 0);   // row, col
selection.DragExtend(row: 2, col: 6);       // row, col
string copied = selection.GetSelectedText();

selection.SelectWordOrExpression(col: 2, row: 1);   // col, row!

Point start = selection.Start;              // buffer-absolute
Point end = selection.End;                  // End.X is EXCLUSIVE

terminal.ScrollToBottom();
```

Turning key presses into terminal input, for both host styles.

```csharp
using CodeBrix.Terminal.Engine;

// A raw-key host: one call handles printables and non-printables alike.
void OnKeyDown(TerminalKey key, TerminalModifiers modifiers)
{
    string seq = TerminalKeyEncoder.Encode(
        key, modifiers, applicationCursor: terminal.ApplicationCursor);

    if (seq is not null)
    {
        // Send seq to the process / remote host. For a loopback demo:
        terminal.Feed(seq);
        terminal.ScrollToBottom();
    }
}

// A composed-text host: named keys through EncodeSpecial, typed
// characters through EncodeComposed (correct on any keyboard layout).
void OnSpecialKey(TerminalKey key)
    => Send(TerminalKeyEncoder.EncodeSpecial(
                key, terminal.ApplicationCursor));

void OnTextInput(int codePoint, TerminalModifiers modifiers)
    => Send(TerminalKeyEncoder.EncodeComposed(codePoint, modifiers));
```

The Unicode toolkit on its own, with the three different size measures.

```csharp
using CodeBrix.Terminal.Text;

ustring text = "Hello, World! éèê";
int runeCount = text.RuneCount;         // code points, not bytes
int displayWidth = text.ConsoleWidth;   // terminal columns

ustring upper = text.ToUpper();
ustring lower = text.ToLower();

foreach (var (index, rune) in text.Range())
{
    bool isLetter = Rune.IsLetter(new Rune(rune));
    int width = Rune.ColumnWidth(new Rune(rune));
}
```

## Using it in a CodeBrix.Platform application

Nothing in the engine is head-specific: it is renderer-agnostic and headless, so a CodeBrix.Platform application uses it exactly as any other host does. Paint the buffer onto whatever surface your view draws with, following the eight-point renderer contract above; map the head's key events onto the platform-neutral `TerminalKey` and `TerminalModifiers` and run them through `TerminalKeyEncoder`; and call `terminal.Resize(cols, rows)` when the surface changes size, pushing the new size to the process as well.

The repository's own reference renderer is the sample client's control, `Controls/TerminalControl.xaml` and its code-behind, which paints the buffer and drives the connection from a view model.

## Pitfalls

- Confusing the package ID with the namespaces. The package is `CodeBrix.Terminal.MitLicenseForever`; the namespaces are `CodeBrix.Terminal.Engine` and `CodeBrix.Terminal.Text`, plus `Rune` and `RuneExtensions` in `System`.
- Writing `using CodeBrix.Terminal.Engine.Utils;`. There is no such namespace: `CircularList<T>` and `RuneExt` are in `CodeBrix.Terminal.Engine`.
- Naming the type `Buffer` while both `System` and `CodeBrix.Terminal.Engine` are in scope, which is a CS0104 ambiguity against `System.Buffer`. Alias it. The same trap applies to `Color` once `System.Drawing` is in scope.
- Trying to *set* the terminal mode properties. They have internal setters and change only in response to the escape sequences you feed.
- Reading `Lines[row]` directly. Use `Lines[Buffer.YBase + row]` for the live screen or `Lines[Buffer.YDisp + row]` for what the viewport shows; they differ while the user is scrolled back, and getting this wrong is a correctness defect rather than a cosmetic one.
- Painting `CharData.Rune` for a blank cell. Check `ch.IsBlank` and paint the background only, and skip `ch.Width == 0` cells.
- Hand-rolling the attribute bit shifts. Use `CharacterAttribute.Unpack` and the two sentinels, which are not valid palette indices.
- Leaving `ConvertEol` at its default when the data source already sends `\r\n`. Every line break doubles into a blank line.
- Setting `Scrollback` or `TabStopWidth` after constructing the terminal and expecting it to take effect.
- Mixing up the coordinate orders. `Terminal.SetCursor` takes (col, row); `SelectionService.SelectWordOrExpression` takes (col, row) while `StartSelection`, `SetSoftStart`, `ShiftExtend` and `DragExtend` take (row, col).
- Forgetting that escape-sequence coordinates are 1-based while every buffer coordinate in this API is 0-based.
- Treating `SelectionService.End` as inclusive. It is column-exclusive, and `Start == End` is an empty selection.
- Confusing `ustring.Length` (bytes) with `RuneCount` (code points) or `ConsoleWidth` (terminal columns). The three differ for any non-ASCII text.
- Calling the `Rune` classification members as instance methods. They are static.
- Referencing `SearchResult` unqualified. It is nested, as `SearchSnapshot.SearchResult` - and so are the parser's handler delegates, `ustring.RunePredicate`, `Rune.Case` and the nested `Unicode` types.
- Holding a `SearchSnapshot` across buffer changes. Take a new one; `SearchService.Invalidated` tells you when.
- Calling the pseudo-terminal helpers on Windows.
- Expecting 24-bit color. The model is the 256-color palette, and `Terminal.MatchColor` maps an arbitrary triple onto the nearest index.

For speed: batch your `Feed()` calls, because the parser is a state machine and one larger feed is much cheaper than many tiny ones; feed raw bytes rather than decoded strings when the source is a byte stream; paint only dirty rows; use `TranslateToString` with `trimRight: true`; use `Rune.ColumnWidth()` or `CharData.Width` instead of assuming every cell is one column wide; size `Scrollback` deliberately, because every scrollback line is a live `BufferLine`; and take one search snapshot per search session rather than per keystroke.

## Samples and tools in the repository

The `samples/` folder holds one end-to-end demonstration in three projects: a client that renders a terminal, talking over a message hub to a server that runs the real process. The transport, the process management and the shared-key authentication all belong to the sample and not to the package - the engine itself has no networking and spawns no processes outside the pseudo-terminal helpers.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| RemoteTerminal.Shared | The message contracts the client and server exchange | [`samples/RemoteTerminal.Shared`](https://github.com/ellisnet/CodeBrix.Terminal/tree/main/samples/RemoteTerminal.Shared) |
| RemoteTerminal.Server | An ASP.NET Core host that starts and tracks child processes and guards its hub with a shared key; it does not use the terminal engine at all | [`samples/RemoteTerminal.Server`](https://github.com/ellisnet/CodeBrix.Terminal/tree/main/samples/RemoteTerminal.Server) |
| RemoteTerminal.Client | The reference renderer: a Windows-only desktop client whose control paints the buffer and whose view model drives the connection | [`samples/RemoteTerminal.Client`](https://github.com/ellisnet/CodeBrix.Terminal/tree/main/samples/RemoteTerminal.Client) |

Run the server with `dotnet run --project samples/RemoteTerminal.Server`; the client reads its server address and token from its own `appsettings.json`, and the checked-in token pairs with the sample server's default configuration - it is demonstration material, not a secret.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Terminal/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Terminal/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Terminal/blob/main/README-INDEX.txt) |
| Samples and other non-package content | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Terminal/blob/main/EXTRAS-README.txt) |
| Engine tests (escape sequences, attributes, selection, key encoding, viewport) | [tests/CodeBrix.Terminal.Engine.Tests](https://github.com/ellisnet/CodeBrix.Terminal/tree/main/tests/CodeBrix.Terminal.Engine.Tests) |
| Unicode text tests | [tests/CodeBrix.Terminal.Text.Tests](https://github.com/ellisnet/CodeBrix.Terminal/tree/main/tests/CodeBrix.Terminal.Text.Tests) |
| Samples | [samples](https://github.com/ellisnet/CodeBrix.Terminal/tree/main/samples) |

The two test suites are the best available body of compiling example code for the package, and they need no external fixture data and no opt-in environment variables. XML documentation ships alongside the assembly.

## License

CodeBrix.Terminal is licensed under the MIT License, the license is also named in the package ID (`CodeBrix.Terminal.MitLicenseForever`), and the package requires license acceptance when it is installed.

For the provenance and licensing of open source code included in this library, see [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Terminal/blob/main/THIRD-PARTY-NOTICES.txt) in the repository.

---

**Where to go next**

- [TerminalView add-in](../platform/add-ins/TerminalView.md) - the ready-made terminal element for a CodeBrix.Platform application window
- [CodeBrix.Platform.TclTk](CodeBrix.Platform.TclTk.md) - a sibling in this group, when the text a window shows is a script's user interface rather than a shell
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.Terminal on GitHub](https://github.com/ellisnet/CodeBrix.Terminal) - source, tests and samples
