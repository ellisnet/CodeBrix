<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.LilyPort</sub>

# CodeBrix.LilyPort

**A managed, cross-platform music engraving engine for .NET: `.ly` notation source in - a file or a string - and SVG pages plus Standard MIDI Files out, entirely in process.** There are no native libraries, no fontconfig and no external notation program to install; the music fonts, the text fonts and the Scheme layer are embedded resources inside the assemblies. Any .NET 10 application can drive it, including a CodeBrix.Platform application.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.LilyPort](https://github.com/ellisnet/CodeBrix.LilyPort) |
| **Packages** | [`CodeBrix.LilyPort.GplLicenseForever`](https://www.nuget.org/packages/CodeBrix.LilyPort.GplLicenseForever) |
| **License** | GPL-3.0-only, and the package requires license acceptance; see [License](#license) |
| **Requires** | .NET 10 or later; a writable per-user cache directory (optional, for the boot cache) |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Windows, Linux and macOS |

## What it does

- Engraves `.ly` notation source - a file on disk or an in-memory string - to SVG pages.
- Writes Standard MIDI Files, one per performance.
- Runs the whole engraving pipeline in process: parser, contexts, engravers, grobs, spacing, line breaking and page breaking.
- Handles multi-book and multi-page documents, written under the notation program's own output file naming rules.
- Emits point-and-click anchors in the SVG, for an editor's click-to-source, switchable per run.
- Applies per-run `-d` program options, including accumulating `include-settings` house-style files.
- Brings an older document up to current syntax in process, with the rule list and the version comparisons exposed.
- Imports ABC, MIDI and MusicXML to notation source, each with its own options class.
- Embeds the music fonts and the text fonts, so there is no fontconfig, no system font fallback and nothing to install.
- Offers a direct surface for one music expression at a time: `LilyPortEngraver`, `LilyPortPerformer` and `SvgBackend` over a parsed `MusicObject`.
- Parses without engraving, for an editor's syntax check, with diagnostics carrying file, line and column.
- Takes a per-run message writer and a cancellation token.
- Keeps an on-disk boot cache, so every process after the first one starts quickly.

## When to use it

Reach for this when an application has to typeset music itself - a score editor, a documentation build, a batch that turns a corpus of notation source into pages - and cannot depend on an external program being installed. Everything happens inside your process, on every desktop operating system, with one package reference.

Do not reach for it when you want a different output format. The engine has exactly one backend, SVG, plus MIDI: there is no PDF, PostScript, EPS or PNG output. The family's route from those SVG pages to a PDF is [`CodeBrix.PdfDocCreate.Html2Pdf.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.PdfDocCreate.Html2Pdf.MitLicenseForever) (see [CodeBrix.PdfDocuments](CodeBrix.PdfDocuments.md)), which places SVG as vector content into a PDF; that dependency is deliberately not part of this package.

Other things it does not do, from its own list: no command-line program and no command-line parsing (options are per-run); no GUI, editor or preview control - it writes SVG, and displaying it is yours; no MIDI playback and no audio; no MusicXML, ABC or MIDI export, since the converters go one way, into notation source; no system fonts, no fontconfig; no Scheme REPL of its own (the interpreter is [CodeBrix.LilyScheme](CodeBrix.LilyScheme.md)); no parallel engraving within a process and no second engine per process; and no editing of files in place.

> [!WARNING]
> Referencing this package makes your application a GPL-3.0 work when you distribute it. Decide that before you write code against it - see [License](#license).

## Getting started

```bash
dotnet add package CodeBrix.LilyPort.GplLicenseForever
```

One package reference gives you five compile-time assemblies: `CodeBrix.LilyPort.dll` (the facade), `CodeBrix.LilyPort.Engine.dll` (the engine, carrying the embedded Scheme layer and the fonts), `CodeBrix.LilyPort.Backends.dll`, `CodeBrix.LilyPort.Parsing.dll` and `CodeBrix.LilyPort.Flower.dll`. The four sub-assemblies are bundled inside the package, not separate packages and not package dependencies. The root namespace is `CodeBrix.LilyPort`, without the `.GplLicenseForever` suffix; the suffix is part of the package id only, so the license identification travels with the package name.

Most applications need only the first using line, because `BatchRunner` does the whole job.

```csharp
using CodeBrix.LilyPort;                  // BatchRunner, BatchRunOptions,
                                          //   BatchRunResult, LilyPondInit,
                                          //   LilyPortEngraver, EngraveResult,
                                          //   LilyPortPerformer, LilyPortInfo
using CodeBrix.LilyPort.ConvertLy;        // DocumentConverter, ConversionVersion,
                                          //   ConversionRule, ConversionResult
using CodeBrix.LilyPort.Importers;        // AbcImporter, MidiImporter,
                                          //   MusicXmlImporter, *ImportOptions,
                                          //   ImportResult
using CodeBrix.LilyPort.Backends;         // SvgBackend
using CodeBrix.LilyPort.Flower;           // Warn, LogLevel,
                                          //   LilyPondErrorException,
                                          //   LineTrackingWriter, Rational,
                                          //   Interval, Offset, Axis, Direction
using CodeBrix.LilyPort.Engine.Bootstrap; // LilyPondScheme, ProgramOptions,
                                          //   CommandLineOptions,
                                          //   BootExpansionCache, LilyVersion
using CodeBrix.LilyPort.Engine.Layout;    // OutputDef, Stencil, Performance,
                                          //   PaperBook, PaperScore, IStencilSink
using CodeBrix.LilyPort.Engine.Music;     // MusicObject, Moment, Duration, Pitch
using CodeBrix.LilyPort.Parsing.Session;  // LilyParserSession, ParseOutcome
using CodeBrix.LilyScheme;                // Interpreter (RunWithLargeStack)
```

There is nothing to register at startup: no codec registration and no feature flag. This engraves a source string to an SVG page.

```csharp
using System;
using System.IO;
using CodeBrix.LilyPort;

string source = "\\relative { c'4 d e f | g1 }\n";
string outputDirectory = Path.Combine(Path.GetTempPath(), "lilyport-out");

BatchRunResult result = BatchRunner.RunText(
    source,            // the notation source text
    "first",           // output base name (no extension)
    null,              // directory that \include resolves against
    outputDirectory);  // where the .svg (and .midi) files land

foreach (string line in result.Diagnostics)
{
    Console.Error.WriteLine(line);
}

if (result.ErrorCount == 0 && result.SvgPath != null)
{
    Console.WriteLine("wrote " + result.SvgPath);        // .../first.svg
}
```

The first call boots the engine and takes far longer than any later call in the same process. A document with no `\version` statement is engraved anyway and reports a warning in `result.Diagnostics`, with `ErrorCount` staying at zero.

## Key concepts

### One engine per process, long-lived and serialized

Everything in the engine - the Scheme interpreter, the option table, the parser's toplevel scope, the font registry - is process-global state that lives in statics and dies with the process. The port boots that state once and then engraves any number of files through it, restoring the per-file state itself between runs. So the API is a long-lived, serialized engine driven through `BatchRunner`, not a stateless function you call in parallel: boot it once and engrave many files through it rather than starting a process per file.

Calls through `BatchRunner` take the process-wide engine lock for you and may be made from any thread. The direct surface (`LilyPortEngraver`, `LilyPortPerformer`, `LilyParserSession`) does not take that lock - serialize those calls yourself.

### Running on a large stack

Every engine call must run on a large stack, because the Scheme layer recurses hard and overflows the CLR's default thread stack. `BatchRunner` wraps itself; everything else you call yourself must be wrapped.

```csharp
Interpreter.RunWithLargeStack(() => { ... });        // Action
T value = Interpreter.RunWithLargeStack(() => ...);  // Func<T>
```

An exception thrown inside reaches you as itself, with its stack trace. A stack overflow, by contrast, ends the process without an exception - which is why this wrapper is not optional.

### The pipeline

```text
.ly text ---> LilyParserSession (Parsing)
                |  toplevel handlers run DURING the parse (Scheme, vendored)
                v
              Book / Score (Engine.Objects)
                |  Book.Process -> PaperBook: iterate music through a context
                |  tree built from ly/engraver-init.ly, engravers make grobs,
                |  spacing, line breaking, page breaking (Engine.Translation,
                |  Engine.Objects, Engine.Layout)
                v
              one Stencil per page  ---> SvgBackend (Backends) ---> <base>.svg
                |
                +--> (score has \midi) performers make audio elements
                     ---> Performance.WriteOutput ---> <base>.midi
```

The parse *is* the execution: a toplevel `\score` reaches its handler from the rule that reduces it. Much of the API hands you Scheme values - object-typed `Pair` lists, `Symbol` keys, `MutableString` text - because the layer above the parser is driven by Scheme running on [CodeBrix.LilyScheme](CodeBrix.LilyScheme.md), and the C# engine is what that Scheme calls.

### BatchRunner - the whole job in one call

```csharp
static BatchRunResult RunFile(string filePath, string outputDirectory)
static BatchRunResult RunFile(string filePath, string outputDirectory,
                              string outputBaseName)
static BatchRunResult RunFile(string filePath, string outputDirectory,
                              string outputBaseName, BatchRunOptions runOptions)
static BatchRunResult RunText(string text, string baseName,
                              string includeDirectory, string outputDirectory)
static BatchRunResult RunText(string text, string baseName,
                              string includeDirectory, string outputDirectory,
                              BatchRunOptions runOptions)
static void SplitOutputName(string outputName, out string directory,
                            out string baseName)
static void UseFontsFrom(string directory)
static void ReportWorkingDirectoryChange(string directory)
static void InstallSessionBindings(Interpreter interpreter)
```

`RunFile` reads the file and uses its own directory as the `\include` root. `RunText` takes the text plus a `baseName` (the output name without extension, and also what diagnostics call the input) and an `includeDirectory` that the text's own `\include` statements resolve against, or null for none; the output directory is created if it is missing. `SplitOutputName` turns one `-o` value into its two halves: an existing directory is a directory, and anything else splits into a directory part and a file part.

A named output renames what is written, not what was read. The progress line, every diagnostic's location and every music object's origin all keep the input's name; only the files on disk answer to the new one.

### One run's lifetime: BatchRunOptions

```csharp
public sealed class BatchRunOptions
    object          PointAndClick   { get; set; }   // null = leave default (true)
    IList<string>   Options         { get; set; }   // -d options, in order
    string          InputName       { get; set; }   // the INPUT's base name
    TextWriter      MessageWriter   { get; set; }   // this run's output
    CancellationToken CancellationToken { get; set; }
```

Everything here lives for one run: it is applied after the per-file restore, so the next run's restore takes it off again. `PointAndClick` is true by default, which wraps every grob that has an input origin in an `<a xlink:href="textedit://...">` anchor; pass false for output you publish or compare. Each entry in `Options` is the text that would follow `-d` on a command line - `"no-point-and-click"` sets an option false, `"debug-skylines"` sets one true, `"include-settings=/path/to/house.ily"` gives one a value and accumulates. Cancellation is cooperative and coarse: it is checked before the parse, between books and before output is written, and one book's engraving is one uninterruptible engine call.

### What comes back: BatchRunResult

```csharp
public sealed class BatchRunResult
    string                 SvgPath          // FIRST page written, or null
    IReadOnlyList<string>  SvgPaths         // every page, in page order
    IReadOnlyList<string>  MidiPaths        // every performance, in order
    int                    BookCount        // books the toplevel handlers made
    int                    SystemCount      // LINES the breaker chose (not scores)
    int                    SkippedEntries
    int                    ErrorCount       // parse + epilogue errors
    IReadOnlyList<string>  Diagnostics      // parse-side messages, in order
    string                 DeclaredVersion  // the main input's \version, or null
```

All paths are absolute. A file that parsed with errors can still produce a page, so test `ErrorCount` rather than `SvgPath` alone; and `Diagnostics` can carry lines while `ErrorCount` is zero, because a missing `\include` is a "cannot find file" message rather than a parse error. `DeclaredVersion` is what the lexer recorded, so an editor deciding whether to offer an update reads it instead of re-scanning the text.

### Output file names

```text
one-page book                <base>.svg
multi-page book              <base>-<pageNumber>.svg, numbered from the
                             book's first-page-number (a book that starts on
                             page 3 writes -3 and -4 and has no -1)
output-suffix set            <base>-<suffix>.svg (a toplevel
                             `#(define output-suffix "x")' or a paper
                             variable)
several books, same key      <base>.svg, <base>-1.svg, <base>-2.svg ...
                             (keyed by base name AND suffix together)
first performance of a book  <base>.midi
later performances of it     <base>-1.midi, <base>-2.midi ...
a score with no \midi block  no MIDI file at all
```

MIDI names are the book's, and the performance counter restarts for every book, so a host that pairs MIDI files with movements by name alone gets multi-book documents wrong. Read `MidiPaths` and `SvgPaths`, which are in the order written, rather than composing names.

### Diagnostics and errors

Every layer reports through one process-wide static class, `Warn`, with its own prefixes: `warning: `, `error: `, `fatal error: `, `programming error: `.

```csharp
static LogLevel   Level { get; set; }          // default LogLevel.LevelInfo
static TextWriter Output { get; set; }         // default: Console.Error,
                                               //   wrapped in LineTrackingWriter
static bool       RecordMessages { get; set; } // also keep them in memory
static IReadOnlyList<string> Messages { get; } // what was recorded
static void       ClearMessages()
static bool       WarningAsError { get; set; } // -dwarning-as-error
static Func<bool> WarningAsErrorSource { get; set; }
static bool       IsEnabled(LogLevel severity)
```

There are three places to read a run's diagnostics, chosen by scope: `BatchRunResult.Diagnostics` (the parse-side messages of that run, always collected whatever `Level` says), `BatchRunOptions.MessageWriter` (everything printed for that run, at the current level, without touching the process-wide writer) and `Warn.RecordMessages` with `Warn.Messages` (everything from every layer, process-wide, recorded regardless of level, debug lines included).

The errors a run can raise are `LilyPondErrorException` (a fatal error - where the program would have exited, a library throws instead), `ParseAbortedException` (the parser hit end of input while recovering, which is what a truncated document or an unclosed brace looks like; ordinary syntax errors do not throw and are counted in `ErrorCount`), `OperationCanceledException`, `NotPortedException` and `CodeBrix.LilyScheme.Runtime.SchemeThrow`. A book that fails is a `Diagnostics` line reading `"book processing failed: ..."`, not an exception, and the run goes on to the next book.

### The boot cache

Macro-expanding the embedded Scheme layer is nearly all of a cold boot, and replaying a recorded expansion is fast. The first boot on a machine records the expansion into a per-user cache file, and every later process replays it.

```csharp
const string EnabledVariable   = "LILYPORT_EXPANSION_CACHE"     // "0" disables
const string DirectoryVariable = "LILYPORT_EXPANSION_CACHE_DIR" // override
static bool Enabled { get; }
static string CacheDirectory { get; }
static string CacheFilePath { get; }
static ExpansionCache Acquire()          // attached by CreateInterpreter
static void SaveIfDirty(Interpreter interpreter)   // done by LoadViaLilyScm
static void ResetProcessMemo()
```

The cache directory is `$XDG_CACHE_HOME/CodeBrix.LilyScheme` (or `~/.cache/CodeBrix.LilyScheme`) on Linux, `~/Library/Caches/CodeBrix.LilyScheme` on macOS and `%LOCALAPPDATA%\CodeBrix.LilyScheme` on Windows. The key is a hash over the assembly identities and every embedded Scheme file, so a package upgrade means one cold boot and then warm boots again; a corrupt or foreign file is a miss, and a boot that cannot write its cache still boots.

### The direct surface: one music expression

Below `BatchRunner` sit two static classes that take one music tree and give back its layout or its performance, without a book, a paper or any file.

```csharp
public static class LilyPortEngraver
    static EngraveResult Engrave(MusicObject music, OutputDef layout = null)
    static string EngraveToSvg(MusicObject music, OutputDef layout = null)

public sealed class EngraveResult
    GlobalContext Global            // the root context the run used
    ScoreEngraver ScoreEngraver     // owns the PaperScore
    SystemGrob    System            // the first broken piece (or the root)
    Stencil       Stencil           // every line, stacked
    int           LineCount         // lines the breaker chose
    PaperScore    PaperScore        // or null
    static IReadOnlyList<string> MissingTranslators()

public static class LilyPortPerformer
    static Performance Perform(MusicObject music, OutputDef midi)
```

This surface engraves one score against an unscaled layout with no page, so headers, titles, page numbers, `\paper` variables and multi-score books are `BatchRunner`'s job, not this one's. It is the fastest way to a small preview of a single expression, and it is neither wrapped in `RunWithLargeStack` nor serialized: do both yourself.

### SvgBackend and Stencil

```csharp
namespace CodeBrix.LilyPort.Backends;
public sealed class SvgBackend : IStencilSink
    int    Precision  { get; set; }   // decimals per coordinate (4)
    double UnitLength { get; set; }   // mm per staff space; the layout's
                                      //   output-scale (1.7573 = 20 pt staff)
    List<(object Grob, Offset At)> Causes          // grobs that drew, in order
    List<string> UnhandledCommands                 // commands not understood
    string Body                                    // fragment so far
    void   Clear()
    string RenderFragment(Stencil stencil)         // no document wrapper
    string RenderDocument(Stencil stencil)         // complete <svg> document
    object Output(object expression)               // one drawing command
```

The document's width and height are millimeters - the stencil's extent times `UnitLength` - while the `viewBox` is in staff spaces, so `UnitLength` must be set from the paper the score was laid out under (`LilyPondInit.DefaultPaper().GetDimension("output-scale")`) or the page comes out at the wrong size. `BatchRunner` does this per book. Music glyphs are written as outline paths, so no music font is needed to view the output; text is written as `<text font-family="serif" ...>` (or `"sans"`, `"monospace"`) and the viewer resolves the generic family through its own fonts.

### Parsing without engraving

`LilyPondInit.Session()` answers the one parser session the engine uses; a fresh session of your own has no init layer and does not know what a `Staff` or a note name is.

```csharp
Interpreter.RunWithLargeStack(() =>
{
    LilyParserSession session = LilyPondInit.Session();
    session.IncludePath.Add(projectDirectory);
    try
    {
        ParseOutcome outcome = session.ParseText(text, "check.ly");
        // outcome.Success, outcome.ErrorCount, outcome.AllDiagnostics()
    }
    catch (ParseAbortedException aborted) { /* truncated input */ }
});
LilyPondInit.RestoreDefaults();   // undo whatever the text defined
```

`ParseText` runs toplevel handlers and defines variables in the shared scope, which is why `LilyPondInit.RestoreDefaults()` follows it. `IncludePath` is the only list the Scheme layer can see: `ly:find-file`, `ly:parse-file` and `ly:parse-init` search it and nothing else, so a directory that both the engine and Scheme must find belongs there.

### Converting an older document

`DocumentConverter` applies the conversion rules in order and needs no engine and no boot.

```csharp
static IReadOnlyList<ConversionRule> Rules { get; }     // all, in order
static ConversionVersion LatestVersion { get; }         // the default target
static bool TryReadDeclaredVersion(string text, out ConversionVersion version)
static bool TryReadDeclaredVersion(string text, out ConversionVersion version,
                                   out bool malformed)
static ConversionResult Convert(string text,
                                ConversionVersion? from = null,
                                ConversionVersion? to = null)
static IReadOnlyList<ConversionRule> RulesBetween(ConversionVersion from,
                                                  ConversionVersion to)
```

`Convert` applies every rule after `from` and up to `to`, rewrites the `\version` line to the version reached, and stops at the last successful rule if one has to give up. A null `from` takes the document's own declaration and a null `to` takes `LatestVersion`. The result carries `Text`, `FromVersion`, `ToVersion`, `LastRuleApplied`, `AppliedRules`, `Messages`, `Errors`, `Changed`, `StampedVersion` and `VersionUnknown`. `Messages` is the part of a conversion that still needs a human: show it. The converter answers text; writing it back is your decision.

### The importers

`AbcImporter`, `MidiImporter` and `MusicXmlImporter` take text or bytes and give back notation source plus the converter's own messages, again with no engine and no boot.

```csharp
public static class AbcImporter
    static ImportResult Import(string abcText, AbcImportOptions options = null)
public static class MidiImporter
    static ImportResult Import(byte[] midiData, MidiImportOptions options = null)
public static class MusicXmlImporter
    static ImportResult Import(string xmlText, MusicXmlImportOptions options = null)
    static ImportResult ImportCompressed(byte[] mxlData,
                                         MusicXmlImportOptions options = null)
```

Every result has the same shape: `Text` (notation source, or null when nothing could be converted), `Messages`, `Errors` and `Succeeded`. Failure is an `ImportResult`, never an exception. What comes back is ordinary notation source - hand it to `BatchRunner.RunText`. A MIDI transcription is a transcription rather than a score, because MIDI carries no beams, slurs or accidental spelling.

### Fonts

Nothing needs to be on disk. The music fonts, their SVG outline companions and the text faces are embedded resources in the engine assembly, and the engine reads them by name; there is no fontconfig and no system-font lookup at all. `FontAssets.SearchPaths` (or `BatchRunner.UseFontsFrom(directory)`) consults a directory for font *files* by file name before the embedded copies, for the rest of the process.

Three consequences matter. There is no fallback to the operating system's fonts, so a code point none of the embedded faces covers draws the `.notdef` box with a "no glyph for character" warning, by design. CJK, Hebrew and Arabic are unsupported unless the document supplies a font, which it can do with `#(ly:font-config-add-font "/abs/path/MyFont.otf")` or `#(ly:font-config-add-directory "/abs/path/fonts")` - with an absolute path, since a relative one resolves against the process's current directory, and a failure is a fatal error. Those registrations last for one file. And a document face is consulted alone for its family: no embedded face is chained behind it.

## Examples

Warming the engine at start-up and then serializing every operation is the canonical host shape.

```csharp
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.LilyPort;
using CodeBrix.LilyPort.Engine.Bootstrap;
using CodeBrix.LilyScheme;

public sealed class EngineHost
{
    private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
    private Task _load;

    public Task BeginLoadingAsync()
    {
        return _load ??= Task.Run(() => Interpreter.RunWithLargeStack(() =>
        {
            Interpreter interpreter = LilyPondScheme.CreateInterpreter();
            LilyPondScheme.LoadViaLilyScm(interpreter);   // the scm/ layer
            LilyPondInit.DefaultLayout();                 // the ly/ layer
        }));
    }

    public async Task<BatchRunResult> EngraveAsync(
        string source, string baseName, string includeDirectory,
        string outputDirectory, CancellationToken cancellationToken)
    {
        await BeginLoadingAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            BatchRunOptions options = new BatchRunOptions
            {
                CancellationToken = cancellationToken,
            };
            return await Task.Run(
                () => BatchRunner.RunText(
                    source, baseName, includeDirectory, outputDirectory, options),
                cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }
}
```

Per-run options, a log panel and cancellation, all scoped to the one run.

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using StringWriter log = new StringWriter();
using CancellationTokenSource cancellation =
    new CancellationTokenSource(TimeSpan.FromMinutes(2));

BatchRunOptions options = new BatchRunOptions
{
    PointAndClick = false,                          // publish build: no anchors
    Options = new List<string> { "no-point-and-click" },   // any -d option
    MessageWriter = log,                            // this run's output
    CancellationToken = cancellation.Token,
};

try
{
    BatchRunResult result = BatchRunner.RunText(
        source, "preview", includeDirectory, outputDirectory, options);
}
catch (OperationCanceledException)
{
    // nothing was written after the cancellation point
}

string transcript = log.ToString();   // "Parsing...", "Drawing systems...", warnings
```

Engraving one music expression straight to SVG and MIDI, with no files in the middle.

```csharp
using System.IO;
using CodeBrix.LilyPort;
using CodeBrix.LilyPort.Backends;
using CodeBrix.LilyPort.Engine.Layout;
using CodeBrix.LilyPort.Engine.Music;
using CodeBrix.LilyPort.Parsing.Session;
using CodeBrix.LilyScheme;

string directory = Path.Combine(Path.GetTempPath(), "lilyport-direct");
Directory.CreateDirectory(directory);

Interpreter.RunWithLargeStack(() =>
{
    LilyParserSession session = LilyPondInit.Session();   // boots if needed

    MusicObject music = (MusicObject)session.ParseStringExpression(
        "\\relative { c'4 d e f }", "<host>", 1);

    // One call:
    string svg = LilyPortEngraver.EngraveToSvg(music);

    // Or the pieces, with the paper's scale so the mm size is right:
    EngraveResult engraved = LilyPortEngraver.Engrave(music);
    SvgBackend backend = new SvgBackend
    {
        UnitLength = LilyPondInit.DefaultPaper().GetDimension("output-scale"),
        Precision = 4,
    };
    string document = backend.RenderDocument(engraved.Stencil);
    int lines = engraved.LineCount;

    // The MIDI twin:
    OutputDef midi = session.LookupIdentifier("$defaultmidi") as OutputDef;
    Performance performance = LilyPortPerformer.Perform(music, midi);
    performance?.WriteOutput(Path.Combine(directory, "direct.midi"), "direct");
});
LilyPondInit.RestoreDefaults();
```

Importing ABC, MIDI and MusicXML, then engraving the result.

```csharp
using System;
using System.IO;
using CodeBrix.LilyPort;
using CodeBrix.LilyPort.Importers;

// ABC
ImportResult fromAbc = AbcImporter.Import(abcText, new AbcImportOptions
{
    Beams = true,
    SourceName = "tune.abc",
});

// MIDI
MidiImportOptions midiOptions = new MidiImportOptions
{
    Key = "-2:1",             // two flats, minor
    DurationQuant = 32,
    Skip = true,
    SourceName = "song.midi",
};
midiOptions.AllowTuplet.Add("4*2/3");
ImportResult fromMidi = MidiImporter.Import(File.ReadAllBytes(midiPath), midiOptions);

// MusicXML, plain or compressed
MusicXmlImportOptions xmlOptions = new MusicXmlImportOptions
{
    PitchMode = MusicXmlPitchMode.Absolute,
    Language = "deutsch",
    NoPageLayout = true,
    Midi = true,
    SourceName = Path.GetFileName(xmlPath),
};
ImportResult fromXml = xmlPath.EndsWith(".mxl", StringComparison.OrdinalIgnoreCase)
    ? MusicXmlImporter.ImportCompressed(File.ReadAllBytes(xmlPath), xmlOptions)
    : MusicXmlImporter.Import(File.ReadAllText(xmlPath), xmlOptions);

// Every result has the same shape.
foreach (ImportResult result in new[] { fromAbc, fromMidi, fromXml })
{
    foreach (string message in result.Messages) { Console.Error.WriteLine(message); }
    if (result.Succeeded)
    {
        BatchRunResult engraved = BatchRunner.RunText(
            result.Text, "imported", null, outputDirectory);
    }
}
```

## Using it in a CodeBrix.Platform application

The package is not a CodeBrix.Platform add-in and takes no dependency on CodeBrix.Platform: you call it from a service or a view model exactly as any other .NET library. What a UI host has to arrange is the engine's shape - one engine, booted in the background, every call serialized - which is what the `EngineHost` example above does.

The repository's own [`tools/Lily.Shell`](https://github.com/ellisnet/CodeBrix.LilyPort/tree/main/tools/Lily.Shell) is a CodeBrix.Platform application whose window is a terminal, hosting the engine in process so that a session can parse a file, engrave it, talk to the Scheme layer, convert or import, and render a manual without an engine start-up between each command. It is a repository tool and ships nothing, but it is the reference for the shape:

- `src/Lily.Shell.UI/` is a shared project (`.shproj`) holding `App` and `MainPage`; `src/Lily.Shell.Core/` holds the host, the commands, the view model and the window chrome.
- `src/Lily.Shell.<head>/` is one head project per platform head - `Lily.Shell.LinuxX11`, `Lily.Shell.LinuxWayland`, `Lily.Shell.LinuxFrameBuffer`, `Lily.Shell.MacOS`, `Lily.Shell.Win32Skia` and `Lily.Shell.WinWpfSkia` - each with exactly one platform runtime package and one `Program.cs`.
- The engine load is background work and the first load is the slow one, so the shell reports progress in its window title. Commands that need the engine wait for it and say so; the converter and the importers do not need it at all.
- Its terminal control ships separately as the [`CodeBrix.Platform.TerminalView.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.TerminalView.ApacheLicenseForever) add-in, so the shell only flows that package through to the application. See the [TerminalView add-in](../platform/add-ins/TerminalView.md).
- The SkiaSharp a desktop head carries comes from CodeBrix.Platform's own runtime, as in every CodeBrix.Platform application.

## Pitfalls

- Starting a process per file. That is unusable at scale, and it is why this package has a batch runner rather than a command-line program. Keep the process alive and reuse it.
- Calling the engine on a default thread stack. `LilyPortEngraver`, `LilyPortPerformer`, `LilyParserSession` and any Scheme evaluation must run inside `Interpreter.RunWithLargeStack`; a stack overflow ends the process without an exception.
- Overlapping engine calls. `BatchRunner` takes a lock; the direct surface does not, and two direct calls at once corrupt each other's state silently.
- Expecting a string back. `RunText` and `RunFile` write files and return their paths. Read the SVG back if you need it, or use the direct surface for an in-memory fragment of one expression.
- Calling `RunText` with a null `includeDirectory` for a document that includes its neighbors. Only the built-in files resolve then; a missing include is a "cannot find file" line in `Diagnostics`, `ErrorCount` stays zero, and a page is still produced - without the included music.
- Letting `ParseAbortedException` escape in an editor host. An unclosed brace at end of input throws out of `RunText` rather than being counted, and half-typed documents are the normal case in an editor.
- Deciding on `SvgPath` instead of `ErrorCount`. A page and errors happen together.
- Leaving point-and-click anchors on for output you publish or compare. Every note becomes an anchor naming an absolute path composed at draw time; pass `PointAndClick = false` for a publish build and keep it on for an editor preview.
- Naming an output `name.pdf`. A named output keeps whatever extension it carries, so that engraves to `name.pdf.svg`. Give base names.
- Composing output file names yourself. Page files carry the page number, starting at the book's first-page number rather than an index from one, and the MIDI performance counter restarts for every book. Read `SvgPaths` and `MidiPaths`.
- Setting options directly on the option table. The next run's restore puts them back; per-run settings go through `BatchRunOptions.Options`, and a house style goes through `include-settings=...`.
- Forgetting that `Warn` is process-wide and prints to `Console.Error` by default. A UI host that forgets it floods its standard error, and a test host that redirects it and forgets to restore it changes every later test. Prefer `BatchRunOptions.MessageWriter`, which is scoped to the run and restored for you.
- Reading `Warn.Messages` without filtering. It records everything regardless of `Level`, debug lines included; filter by prefix, and call `ClearMessages` between documents.
- Driving `LilyPondInit.Session()` and not calling `LilyPondInit.RestoreDefaults()` afterwards.
- Creating a second interpreter in the same process. `CreateInterpreter` replaces the ambient interpreter and the cached init layer reloads. One per process.
- Confusing `LilyPortInfo.Version` (the package's own version) with `LilyPortInfo.CompatibleWithVersion` (the release of the input language this engine reads and reports as). They are two different things.
- "Correcting" the `\version` line an importer writes. That is the frozen output syntax of the converters, not a defect and not the engine's version.

## Samples and tools in the repository

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Lily.Shell | An interactive engine shell: a CodeBrix.Platform application with six heads, hosting the engine in process behind fifteen commands | [`tools/Lily.Shell`](https://github.com/ellisnet/CodeBrix.LilyPort/tree/main/tools/Lily.Shell) |
| Lily.Docs | Generates the port's documentation files and renders manuals as print-shaped HTML and PDF, with every music snippet engraved by the engine | [`tools/Lily.Docs`](https://github.com/ellisnet/CodeBrix.LilyPort/tree/main/tools/Lily.Docs) |
| regression-harness | Grades the engine's output page by page against a reference, with a committed per-file ratchet | [`tools/regression-harness`](https://github.com/ellisnet/CodeBrix.LilyPort/tree/main/tools/regression-harness) |
| abcprobe | The ABC importer's oracle harness: fixture generator, probe console and a divergence list | [`tools/abcprobe`](https://github.com/ellisnet/CodeBrix.LilyPort/tree/main/tools/abcprobe) |
| midi2lyprobe | The MIDI importer's oracle harness | [`tools/midi2lyprobe`](https://github.com/ellisnet/CodeBrix.LilyPort/tree/main/tools/midi2lyprobe) |
| musicxml2lyprobe | The MusicXML importer's oracle harness | [`tools/musicxml2lyprobe`](https://github.com/ellisnet/CodeBrix.LilyPort/tree/main/tools/musicxml2lyprobe) |
| mutopia-probe | Opens every entry point of a locally downloaded corpus, produces a PDF and a MIDI file from each and grades them | [`tools/mutopia-probe`](https://github.com/ellisnet/CodeBrix.LilyPort/tree/main/tools/mutopia-probe) |
| parity-probes | The measurement instruments behind the port's rulings - read a probe before running it | [`tools/parity-probes`](https://github.com/ellisnet/CodeBrix.LilyPort/tree/main/tools/parity-probes) |

Start `tools/Lily.Shell` with `dotnet run --project src/Lily.Shell.LinuxX11 -c Release`, substituting the head you want.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.LilyPort/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.LilyPort/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.LilyPort/blob/main/README-INDEX.txt) |
| Tools and other non-package content | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.LilyPort/blob/main/EXTRAS-README.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.LilyPort.Tests](https://github.com/ellisnet/CodeBrix.LilyPort/tree/main/tests/CodeBrix.LilyPort.Tests) |
| Samples and tools | [tools](https://github.com/ellisnet/CodeBrix.LilyPort/tree/main/tools) |

## License

CodeBrix.LilyPort is licensed under the GNU General Public License version 3 only - deliberately not "or later" - and the license is also named in the package ID (`CodeBrix.LilyPort.GplLicenseForever`). The package requires license acceptance when it is installed. The repository's own source files are GPL-3.0-or-later while the package conveys as GPL-3.0-only; both statements are true at once.

Your application becomes a work based on the package, and when you convey it to anyone it must be licensed under GPL version 3 with its complete corresponding source available; internal use with no distribution carries no obligation. `LICENSE`, `LICENSE.OFL` (the SIL Open Font License, for the music fonts) and `THIRD-PARTY-NOTICES.txt` travel in the package root - keep them with any redistribution. The Scheme interpreter dependency is LGPL-3.0-or-later, which a GPL work may consume freely. The SVG the engine writes contains no font data - glyphs are outline paths and text is text with a family name - so a rendered document does not carry a font license with it.

For the provenance and licensing of open source code included in this library, see [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.LilyPort/blob/main/THIRD-PARTY-NOTICES.txt) in the repository.

---

**Where to go next**

- [CodeBrix.LilyScheme](CodeBrix.LilyScheme.md) - the Scheme interpreter this package depends on and runs its embedded Scheme layer through
- [CodeBrix.PdfDocuments](CodeBrix.PdfDocuments.md) - the family's route from the SVG pages this engine writes to a PDF
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.LilyPort on GitHub](https://github.com/ellisnet/CodeBrix.LilyPort) - source, tests and tools
