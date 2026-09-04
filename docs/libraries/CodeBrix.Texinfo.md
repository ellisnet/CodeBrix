<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Texinfo</sub>

# CodeBrix.Texinfo

**CodeBrix.Texinfo turns GNU Texinfo documentation into a nicely formatted PDF, in managed code, with
nothing to install on any operating system.** The repository produces two packages: one reads a Texinfo
source file and renders it into HTML and CSS written specifically for PDF generation, and the other
takes that markup the rest of the way and produces the finished PDF. Both read standard `.texi` files
and the `.tely` Texinfo dialect used for documents that interleave music snippets with the markup, and
you use them from any .NET 10 application or from a CodeBrix.Platform application.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Texinfo](https://github.com/ellisnet/CodeBrix.Texinfo) |
| **Packages** | [`CodeBrix.Texinfo2Html.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Texinfo2Html.MitLicenseForever)<br>[`CodeBrix.Texinfo2Pdf.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Texinfo2Pdf.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later, and nothing else on any operating system: no native-assets package, no runtime identifier, no system font, no `apt`/`brew`/`msi` step |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Windows, macOS and Linux. Every part of the conversion is managed code - no native library, no GPU, no window system and no system font anywhere in the chain |

## What it does

| Package | Its role | Brings in |
| --- | --- | --- |
| `CodeBrix.Texinfo2Html.MitLicenseForever` | Texinfo source in, HTML and CSS out - nothing else | nothing; the .NET base class library only |
| `CodeBrix.Texinfo2Pdf.MitLicenseForever` | Texinfo source in, a finished PDF out, in one call | Texinfo2Html and `CodeBrix.PdfDocCreate.Html2Pdf.MitLicenseForever` |

```mermaid
flowchart LR
  TEXI[".texi / .tely source"] --> HTML[Texinfo2Html]
  HTML --> MARKUP["HTML + CSS + pictures"]
  MARKUP --> PDF[Texinfo2Pdf]
  PDF --> OUT["finished PDF"]
```

**CodeBrix.Texinfo2Html** reads a GNU Texinfo source file and renders it into HTML and CSS. The markup
stays inside the documented HTML and CSS subset that
[CodeBrix.PdfDocuments](CodeBrix.PdfDocuments.md)'s HTML renderer understands, so the output is ready
to be handed straight to it. Rendering never throws over the contents of a document: anything
unsupported, malformed or missing becomes a warning in the result plus the nearest readable
degradation. What it handles includes:

- Cross references. `@ref`, `@xref` and `@pxref` resolve through the document's anchor table to a
  `#identifier` link, with the wording Texinfo prescribes for each. A reference into another manual
  renders as text and is not a fault; names are matched with their whitespace collapsed; anything left
  unresolved produces one warning for the whole document, naming the count and the first one.
- Indices. `@printindex` builds the named index from entries collected during parsing - from the index
  commands, from the terms of an `@ftable` or `@vtable`, and from every definition command - after
  applying `@syncodeindex` and `@synindex` merges, sorting by `@sortas` key where given, and honoring
  the `txiindexbackslashignore`, `txiindexhyphenignore`, `txiindexlessthanignore` and
  `txiindexatsignignore` flags. A document defines its own index with `@defindex` or `@defcodeindex`.
- Definitions. `@deffn` and its relatives are read as a run of words - a braced group counting as one,
  so `{Special Form}` is a single category - laid out as category, class, data type, name, then the
  arguments, with each x form adding another heading line to the definition already open. The name is
  filed in the index the Texinfo manual names for that command.
- Footnotes, filed under the outermost sectioning unit containing them, skipping `@top` and `@part`,
  numbered document-wide.
- Floats. A `@float` counts within its chapter and within its own type (Figure 1.1, Figure 1.2, Table
  1.1, then Figure 2.1); a float in an unnumbered chapter counts straight through the document, and
  `@listoffloats` prefers `@shortcaption` where there is one.
- The text conventions: `---` is an em dash, `--` an en dash, and the directed quotes are applied - in
  running prose only. Code-like contexts (`@code`, `@samp`, `@kbd`, `@file`, `@option`, `@env`,
  `@command`, `@key`, `@t`, `@verb`, `@example`, `@lisp`, `@verbatim` and music snippets) keep every
  character as written, while `@display` and `@format` are preformatted but not code, so their prose
  *is* converted.
- Accents, composed onto the character they apply to and normalized, so a precomposed character reaches
  the output wherever Unicode has one.
- A document's own macros: `@macro`, `@rmacro` and `@linemacro`, including the ones a manual uses to
  define new definition commands.
- Nested preformatted blocks, written as one more step of indentation, keeping their own text
  conventions, with indentation emitted only on lines that have content.
- Pictures. `@image` references are found, named and copied - never decoded or rasterized.

**CodeBrix.Texinfo2Pdf** owns the hand-off and nothing else: it parses nothing and emits nothing. It
renders the source to HTML and CSS with the first package, feeds that markup to the HTML renderer, and
merges what both stages had to say into one warning list. It stages the document's pictures in a
temporary directory for the length of the render, so what lands beside the PDF is nothing at all.
Installing this one package brings the whole conversion chain with it, and the Texinfo2Html types flow
through transitively as part of its API.

SVG lands in the PDF as vector content by default: the picture's drawing commands are written into the
page as PDF operators - paths, fills, strokes, dashes, clips, transforms, gradients as PDF shading
patterns, group opacity as a PDF transparency group, and SVG `<text>` as real PDF text in the embedded
face. The picture stays sharp at any zoom, its text stays selectable and searchable, and no image
XObject is added to the file.

## When to use it

Install [`CodeBrix.Texinfo2Pdf.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Texinfo2Pdf.MitLicenseForever)
when what you want is a PDF; it brings the whole chain with it. Install
[`CodeBrix.Texinfo2Html.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Texinfo2Html.MitLicenseForever)
on its own when you want the intermediate HTML and CSS, or when you want to post-process the markup
before anything renders it.

Rendering is intended for a build step or an offline job. The AGENT-README is explicit that rendering a
very large manual inside a web request is not what any of this is for.

What the two packages deliberately do not do:

- The HTML package stops at HTML and CSS. There is no PDF in it; the PDF stage is a separate package.
- The target is one printed document. There is no Info output and no split-into-a-website HTML output;
  `@menu` is parsed and dropped, and node pointers are read and ignored.
- `@math` and `@displaymath` are styled text. There is no mathematical typesetter.
- `@documentencoding` is read and reported, not obeyed. Source is read as UTF-8 unless a byte order
  mark says otherwise, and invalid bytes become replacement characters rather than an exception.
- Raw output blocks (`@tex`, `@html`, `@docbook` and the rest, and `@inlineraw`) are skipped with a
  `RawBlockSkipped` warning under either conditional profile.
- A block environment that is not preformatted, written inside one that is - an `@itemize` inside an
  `@display` - is not rendered as itself; it degrades with a warning. A preformatted environment nested
  in another one does work.
- Nothing here engraves music. The seam is defined and wired, but the library will not take on that
  dependency, so with no renderer registered a snippet is its source text.
- Nothing here decodes or rasterizes pictures; decoding is the PDF stage's job.
- There is no structured warning object on the HTML package's surface. Warnings are strings with a
  category prefix.
- The PDF package does not parse, emit or restyle anything itself; every limitation of the HTML stage
  applies unchanged. It does not fall back to operating-system fonts, does not fetch remote images,
  does not expose the PDF renderer directly, and writes no HTML, CSS or pictures anywhere visible
  during a PDF render.

There is no list of supported commands to check a document against. What a document uses that the
library does not implement becomes a warning and the nearest readable degradation; the intended way to
find out is to render and read `result.Warnings`.

## Getting started

```bash
dotnet add package CodeBrix.Texinfo2Html.MitLicenseForever
dotnet add package CodeBrix.Texinfo2Pdf.MitLicenseForever
```

The package IDs carry the `.MitLicenseForever` suffix; the assemblies and namespaces never do. There is
no `CodeBrix.Texinfo` assembly or namespace - that name belongs to the repository only.

```csharp
using CodeBrix.Texinfo2Html;    //every public type of this package
```

```csharp
using CodeBrix.Texinfo2Pdf;             //TexinfoPdfRenderer and its four companions
using CodeBrix.Texinfo2Html;            //TexinfoHtmlResult, TexinfoHtmlOptions,
                                        //ILilypondSnippetRenderer ... (flows through)
using CodeBrix.PdfDocCreate.Html2Pdf;   //HtmlRenderOptions, RenderWarning,
                                        //RenderWarningCategory (only when you
                                        //declare variables of those types)
```

No registration call is required for either package. A source file becomes a PDF in two statements.

```csharp
using System;
using CodeBrix.Texinfo2Pdf;

internal static class Program
{
    private static void Main(string[] args)
    {
        var renderer = new TexinfoPdfRenderer();
        TexinfoPdfResult result = renderer.RenderFile("manual.texi", "out/manual.pdf");

        Console.WriteLine($"{result.PageCount} pages, {result.Warnings.Count} warnings");
        foreach (string warning in result.Warnings.Messages)
        {
            Console.WriteLine(warning);   //"[texinfo] ..." and "[pdf] ..."
        }
        //out/ holds one PDF and nothing else. Leave the output path off and
        //the PDF is written beside the source as manual.pdf.
    }
}
```

Every method that takes an output path creates the directory it names, so `"out/manual.pdf"` works
without the caller making `out` first.

## Key concepts

### The public surface of Texinfo2Html

Six types for rendering a document - `TexinfoHtmlRenderer`, `TexinfoHtmlOptions`, `TexinfoHtmlResult`,
`TexinfoImageReference`, `TexinfoRenderWarnings` and the `TexinfoConditionalProfile` enum - and six
making up the music-snippet seam: `ILilypondSnippetRenderer`, `LilypondSnippet`, `LilypondSnippetKind`,
`LilypondSnippetOptions`, `LilypondSnippetResult` and `LilypondSnippetImage`. Everything else in the
assembly is internal.

### TexinfoHtmlRenderer

One renderer serves many documents; set `Options` before calling. It is not safe across threads, so
give each thread its own.

```csharp
var renderer = new TexinfoHtmlRenderer();

TexinfoHtmlOptions Options { get; }
TexinfoHtmlResult  GenerateFromFile(string texinfoFilePath)
TexinfoHtmlResult  Generate(string texinfoSource, string baseDirectory = null)
```

Exceptions are reserved for the caller's own mistakes: `ArgumentException` for a null or blank path,
`FileNotFoundException` for a file that is not there, `ArgumentNullException` for a null source string.
`GenerateFromFile` seeds the search paths with the source file's directory and that directory's parent,
in that order, and derives the stylesheet name, the image folder name and the default output base name
from the source file's name. `Generate` reads source held in memory, and its `baseDirectory` is what
`@include`, `@image` and the music-file commands resolve against.

### TexinfoHtmlOptions

```csharp
bool                       EmitSingleFile      { get; set; }   (false)
TexinfoConditionalProfile  ConditionalProfile  { get; set; }   (Print)
List<string>               IncludeSearchPaths  { get; }        (empty)
List<string>               ImageSearchPaths    { get; }        (empty)
Dictionary<string,string>  PredefinedValues    { get; }        (empty; ordinal keys)
bool                       NumberSections      { get; set; }   (true)
string                     ExtraCss            { get; set; }   ("")
string                     CssFileName         { get; set; }   ("" - derived)
string                     ImageFolderName     { get; set; }   ("" - derived)
ILilypondSnippetRenderer   SnippetRenderer     { get; set; }   (null)
```

`EmitSingleFile` embeds the stylesheet in the HTML; either way `result.Css` is populated.
`IncludeSearchPaths` adds directories searched by `@include`, `@lilypondfile` and `@musicxmlfile`,
after the source file's directory and its parent; `ImageSearchPaths` adds directories searched for
`@image` files only, after everything `@include` searches. `PredefinedValues` acts as though the source
opened with `@set name value` for each pair, which is how you supply the strings a manual's build
normally generates; its keys are compared ordinally, as Texinfo flags are. `ExtraCss` is appended after
the built-in stylesheet, so a repeated rule of equal specificity wins.

### TexinfoHtmlResult

```csharp
string Html            //the complete document
string BodyHtml        //the generated markup on its own
string Css             //always separate, even when it was embedded
string Title           //from @settitle; "" when there was none
string Author          //from the title page's FIRST @author; "" when none
string BaseDirectory   //the directory the source was read from
string CssFileName     //the name the markup links to
IReadOnlyList<TexinfoImageReference> Images
TexinfoRenderWarnings Warnings

string ToHtmlDocument(string replacementCss)
string WriteToDirectory(string directory, string baseName = null)
int    CopyImagesTo(string directory)
```

`ToHtmlDocument` rebuilds the complete document around a stylesheet of the caller's own, embedded in it
- the hand-off point for restyling. `WriteToDirectory` creates the directory if needed, writes
`<baseName>.html` as UTF-8 with no byte order mark, writes the stylesheet beside it unless
`EmitSingleFile` was set, copies the document's pictures in, and returns the full path of the HTML file.
`CopyImagesTo` puts every picture under the relative paths the markup uses and returns how many files it
wrote.

### Warnings and the ten categories

`TexinfoRenderWarnings` exposes `IReadOnlyList<string> Messages` in the order the run produced them, and
`Count`. Every message has the shape `<Category>: <message> (at <source>:<line>:<column>)`, and the
leading category word is what to filter on: `Include`, `Conditional`, `Macro`, `Value`,
`RawBlockSkipped`, `Encoding`, `Syntax`, `UnknownCommand`, `Reference` and `Emit`. `Syntax` is the one
that means the document itself needs fixing; `RawBlockSkipped` is expected on most real manuals and is
usually harmless.

### TexinfoConditionalProfile

`Print` reads `@iftex` and every `@ifnot...` branch - the right one for PDF output. `Html` turns
`@ifhtml` on and `@ifnothtml` off, with every other format off and its `@ifnot...` branch on. `Print`
deliberately reads both `@iftex` and `@ifnottex`, because real manuals put document structure - most
often the `@node Top` and `@top` pair - in the `@ifnottex` branch. The cost is that a document writing
the same visible content into both branches contributes it twice.

### The music-snippet seam

`@lilypond` (block form and brace form), `@lilypondfile` and `@musicxmlfile` are parsed, their bracketed
options read into named properties, and the whole snippet offered to `ILilypondSnippetRenderer` when the
caller registered one on `Options.SnippetRenderer`.

```csharp
public interface ILilypondSnippetRenderer
{
    LilypondSnippetResult Render(LilypondSnippet snippet);
}
```

It is called once for each distinct snippet, on the thread rendering the document. Return the pictures
it engraved to; `LilypondSnippetResult.NotRendered` to decline quietly, which raises no warning because
declining is a decision rather than a fault; or `LilypondSnippetResult.Failed(message)` to report why it
could not be done. Never throw - a renderer that throws is caught and turned into the failure it should
have returned, but the message is worse. With no renderer registered, the snippet is shown as its source
text in a `<pre class="texinfo-lilypond">` block and one warning records how many there were.

Two option names decide what the document does and every other option is passed along untouched:
`verbatim` shows the source as well as the engraving, written above the picture, and `quote` indents the
snippet. The recognized vocabulary is `quote`, `verbatim`, `inline`, `notime`, `texidoc`, `doctitle`,
`noindent`, `ragged-right`, `noragged-right`, `fragment`, `nofragment`, `relative[=N]`, `staffsize=`,
`line-width=`, `indent=`, `papersize=`, `paper-width=` and `paper-height=`; anything else is kept as
written in `Options.All`, listed in `Options.Unrecognized`, and reported once for the document. A file
named without an extension is tried with `.ly`, `.xml`, `.musicxml` and `.mxl` in turn. An identical
snippet is engraved once, because the cache key is everything the renderer is given.

### How pictures are found, named and carried

An `@image` reference names a file with no directory and usually no extension, so the file is searched
for and its extension probed - `.png .jpg .jpeg .gif .bmp .svg .webp .tif .tiff .tga .ppm .pgm .pbm`,
plus any extension the command declares. `.pdf` is deliberately not probed. What the markup writes is
not where the file was found: it is a path relative to the document itself, `<ImageFolderName>/<file>`.
Two pictures with the same file name from different directories are numbered apart, and a picture that
cannot be found becomes its alternate text in gray italic with one `Include` warning. The search order
is the source directory, its parent, each `IncludeSearchPaths` entry, then each `ImageSearchPaths`
entry, trying every candidate extension in each directory before moving on.

### TexinfoPdfRenderer and its two workflows

```csharp
var renderer = new TexinfoPdfRenderer();

TexinfoPdfOptions Options { get; }

// workflow one - source in, PDF out
TexinfoPdfResult RenderFile(string texinfoFilePath, string outputPdfPath = null)
TexinfoPdfResult RenderTexinfo(string texinfoSource, string outputPdfPath,
                               string baseDirectory = null)
TexinfoPdfResult RenderFileToBytes(string texinfoFilePath)
TexinfoPdfResult RenderTexinfoToBytes(string texinfoSource, string baseDirectory = null)

// workflow two, step A - source in, markup out
TexinfoHtmlResult GenerateHtmlFromFile(string texinfoFilePath)
TexinfoHtmlResult GenerateHtml(string texinfoSource, string baseDirectory = null)

// workflow two, step B - markup back in, PDF out
TexinfoPdfResult RenderHtml(TexinfoHtmlResult htmlResult, string outputPdfPath,
                            string replacementCss = null)
TexinfoPdfResult RenderHtmlToBytes(TexinfoHtmlResult htmlResult, string replacementCss = null)
TexinfoPdfResult RenderHtmlFile(string htmlFilePath, string outputPdfPath)
TexinfoPdfResult RenderHtmlDocument(string html, string outputPdfPath,
                                    string baseDirectory = null)
```

`GenerateHtml` and `GenerateHtmlFromFile` hand back the very same `TexinfoHtmlResult` the HTML package
produces, so a consumer who installed only the PDF package still has the whole intermediate.
`RenderHtmlFile` is the way back in for a caller who wrote the document out with `WriteToDirectory` and
edited the files by hand. `GenerateHtml` plus `RenderHtml` costs the same as `RenderFile`: the Texinfo
stage runs once either way.

### Two live option objects

```csharp
TexinfoHtmlOptions  Texinfo   { get; }   //how the source is read
HtmlRenderOptions   Html      { get; }   //what the PDF looks like
```

Neither is a copy: they are the live options objects of the two renderers underneath, so every setting
either library has is reachable without a second package reference, and whatever you set stays set for
every later render through the same `TexinfoPdfRenderer`. `Options.Html` is the HTML renderer's own
`HtmlRenderOptions`, whose defaults as a `TexinfoPdfRenderer` constructs them are US Letter at 612 by
792 points, 72-point margins on all four sides, `GenerateOutline` on, `SvgPlacement` at `Vector`,
`SvgRasterScale` at 2.0, `AllowRemoteImages` and `KeepUncoveredCharacters` off, and `CffSubsetMode` at
`None`. This package sets `HeaderText` to `"{title}"` and `FooterText` to `"{page} / {pages}"` because a
printed manual wants them; set either to an empty string to be rid of it. `DocumentTitle` and
`DocumentAuthor` are filled from `@settitle` and the first `@author` when the caller left them empty,
and put back afterwards, which stops one manual's title following a reused renderer to the next.

### TexinfoPdfResult and merged warnings

```csharp
string             OutputFilePath  //full path; "" for a render that produced bytes
byte[]             PdfBytes        //null for a render that wrote a file
int                PageCount
string             Title           //the title the PDF carries in its metadata
TexinfoHtmlResult  Intermediate    //null when the caller supplied the markup
TexinfoPdfWarnings Warnings        //never null; empty for a clean document
```

`Intermediate` is the whole HTML and CSS result the PDF was made from, so a one-shot conversion still
gives access to the markup, the stylesheet and the picture list without running the source twice - it is
the object the PDF was made from, not a second render.

```csharp
IReadOnlyList<string>        Messages         //both stages, each tagged
IReadOnlyList<string>        TexinfoMessages  //untagged, as Texinfo2Html wrote them
IReadOnlyList<string>        PdfMessages      //untagged, as Html2Pdf wrote them
IReadOnlyList<RenderWarning> PdfItems         //the PDF stage's structured warnings
int                          Count            //total across both stages
string                       ToString()       //all of Messages, one per line
const string                 TexinfoStageTag = "[texinfo]"
const string                 PdfStageTag     = "[pdf]"
```

`Messages` is the list to print, Texinfo stage first because it ran first, source order kept within each
stage. `PdfItems` carries the PDF stage's structured warnings, each with a `Category`, a stable `Code`,
the `Message`, an `Occurrences` count and a nullable `CodePoint`. Display prose is not a compatibility
surface; codes are. The codes this package's documentation and tests rely on are
`image.svg.rasterized`, `image.svg.filter-unsupported`, `image.svg.text-unsupported`,
`image.svg.fonts-missing` (a tripwire that should not occur - report it), `image.svg.degraded`,
`image.svg.empty`, `image.svg.failed`, `font.uncovered.removed`, `font.uncovered.kept`,
`font.svg-text.notdef` and `image.format.unsupported`.

### Registering fonts for the PDF stage

```csharp
static void AddFontDirectory(string directory)
static void AddFontFile(string filePath, bool includeInFallback = false)
static void AddFontFiles(IEnumerable<string> filePaths, bool includeInFallback = false)
static void AddFontFilesFromDirectory(string directory, bool includeInFallback = false)
static void AddFallbackFamily(string familyName)
```

`TexinfoPdfFonts` registration is process-global and idempotent; do it once at startup, not per render.
`AddFontDirectory` probes a directory for `CodeBrix.Platform.Fonts.*` package folders, while the
`AddFontFile` family takes loose `.ttf` and `.otf` files with no manifest, reading the family name,
weight and style from the font's own tables. `AddFallbackFamily` appends an already-registered family to
the per-glyph fallback chain, which is consulted character by character and never substitutes a whole
run. A registered font is usable from the generated markup's font families - name it in
`Options.Texinfo.ExtraCss` or a replacement stylesheet - and from SVG text inside placed pictures. Noto
Music sits in the fallback chain automatically, which is what renders a manual's flat, natural and sharp
signs and the supplementary-plane music symbols.

## Examples

Texinfo to HTML, CSS and pictures on disk, with predefined values and a stylesheet tweak.

```csharp
using System;
using CodeBrix.Texinfo2Html;

internal static class Program
{
    private static void Main(string[] args)
    {
        var renderer = new TexinfoHtmlRenderer();
        renderer.Options.PredefinedValues["VERSION"] = "2.1";  //as @set VERSION 2.1
        renderer.Options.ExtraCss = "h1 { color: #003366; }";

        TexinfoHtmlResult result = renderer.GenerateFromFile("docs/manual.texi");

        Console.WriteLine($"Title: {result.Title}  Author: {result.Author}");
        Console.WriteLine($"{result.Images.Count} picture(s), {result.Warnings.Count} warning(s)");
        foreach (string warning in result.Warnings.Messages)
        {
            Console.WriteLine("  " + warning);
        }

        //out/manual.html + out/manual.css + out/manual-images/... - a
        //complete document, ready to open or to hand to a PDF renderer.
        string htmlPath = result.WriteToDirectory("out");
        Console.WriteLine("Wrote " + htmlPath);
    }
}
```

Page setup, a running footer, predefined values and an extra include path, with the PDF returned as
bytes instead of written to a file.

```csharp
using System.IO;
using CodeBrix.Texinfo2Pdf;

var renderer = new TexinfoPdfRenderer();
renderer.Options.Html.SetPageSize("a4");
renderer.Options.Html.MarginLeftPoints = 54;    //0.75 inch
renderer.Options.Html.MarginRightPoints = 54;
renderer.Options.Html.HeaderText = "";          //no running title
renderer.Options.Html.FooterText = "Page {page} of {pages}";
renderer.Options.Html.DocumentAuthor = "Documentation Team";   //overrides @author
renderer.Options.Texinfo.PredefinedValues["VERSION"] = "2.1";  //as @set VERSION 2.1
renderer.Options.Texinfo.IncludeSearchPaths.Add("/abs/path/to/shared-includes");

TexinfoPdfResult pdf = renderer.RenderFileToBytes("manual.texi");
File.WriteAllBytes("manual.pdf", pdf.PdfBytes);   //or send it somewhere
```

Restyling the intermediate before it becomes a PDF. The pictures need no handling at all on this path.

```csharp
using CodeBrix.Texinfo2Html;
using CodeBrix.Texinfo2Pdf;

var renderer = new TexinfoPdfRenderer();
TexinfoHtmlResult html = renderer.GenerateHtmlFromFile("manual.texi");

//html.BodyHtml, html.Css, html.Title, html.Images and html.Warnings are all here
string myCss = html.Css.Replace("#111111", "#000033");   //or replace it wholesale

TexinfoPdfResult pdf = renderer.RenderHtml(html, "out/manual.pdf", myCss);
```

Registering fonts, then handling warnings by stage and by code.

```csharp
using System;
using System.Linq;
using CodeBrix.PdfDocCreate.Html2Pdf;
using CodeBrix.Texinfo2Pdf;

TexinfoPdfFonts.AddFontFile("/fonts/MyCorporateSerif-Regular.ttf");
TexinfoPdfFonts.AddFontFile("/fonts/MyCorporateSerif-Bold.ttf");
TexinfoPdfFonts.AddFontFilesFromDirectory("/fonts/noto-extras", includeInFallback: true);

var renderer = new TexinfoPdfRenderer();
renderer.Options.Texinfo.ExtraCss =
    "html, h1, h2, h3, h4, h5, h6 { font-family: 'MyCorporateSerif', serif; }";

TexinfoPdfResult result = renderer.RenderFile("manual.texi", "out/manual.pdf");

//source problems, by Texinfo category
var syntax = result.Warnings.TexinfoMessages
    .Where(m => m.StartsWith("Syntax:", StringComparison.Ordinal)).ToList();

//typesetting problems, structured
RenderWarning svgRasterized = result.Warnings.PdfItems
    .FirstOrDefault(i => i.Code == "image.svg.rasterized");
if (svgRasterized != null)
{
    Console.Error.WriteLine(svgRasterized.Message);   //verbatim; names the reason
    Console.Error.WriteLine($"{svgRasterized.Occurrences} raster fallback(s)");
}
int droppedCodePoints = result.Warnings.PdfItems
    .Count(i => i.Category == RenderWarningCategory.Font && i.CodePoint.HasValue);
```

An `ILilypondSnippetRenderer` implementation and its registration. This one is adapted from
`AGENT-README.txt`: the `MyEngraverDriver` type and its failure message stand in for whatever engraving
program you drive.

```csharp
using System;
using System.IO;
using CodeBrix.Texinfo2Html;

public sealed class MyEngraver : ILilypondSnippetRenderer
{
    public LilypondSnippetResult Render(LilypondSnippet snippet)
    {
        if (snippet.Kind != LilypondSnippetKind.Music && snippet.FilePath.Length == 0)
        {
            return LilypondSnippetResult.NotRendered;   //file not found: decline
        }
        if (snippet.Kind == LilypondSnippetKind.MusicXmlFile)
        {
            return LilypondSnippetResult.Failed("MusicXML is not supported by this engraver.");
        }

        byte[] png = MyEngraverDriver.EngraveToPng(snippet);
        if (png == null)
        {
            return LilypondSnippetResult.Failed("The engraver produced no output.");
        }
        return LilypondSnippetResult.FromContent(png, "png");

        //Alternatives:
        //  return LilypondSnippetResult.FromFile("/tmp/engraved/score-1.png");
        //  return LilypondSnippetResult.FromImages(new[] {
        //      LilypondSnippetImage.FromFile("/tmp/engraved/page-1.svg"),
        //      LilypondSnippetImage.FromFile("/tmp/engraved/page-2.svg") });
    }
}

var renderer = new TexinfoHtmlRenderer();
renderer.Options.SnippetRenderer = new MyEngraver();
TexinfoHtmlResult result = renderer.GenerateFromFile("notation.tely");
result.WriteToDirectory("out");   //engraved pictures land in out/notation-images/
```

Warnings as a build gate, ignoring the category that every real manual raises.

```csharp
using System;
using System.Linq;
using CodeBrix.Texinfo2Html;

TexinfoHtmlResult result = new TexinfoHtmlRenderer().GenerateFromFile("manual.texi");
var real = result.Warnings.Messages
    .Where(m => !m.StartsWith("RawBlockSkipped:", StringComparison.Ordinal))
    .ToList();
if (real.Count > 0)
{
    Console.Error.WriteLine(string.Join(Environment.NewLine, real));
    Environment.Exit(1);
}
```

## Using it in a CodeBrix.Platform application

Neither package is a CodeBrix.Platform add-in, and neither has a head-specific behavior, a XAML type or
a registration call. A CodeBrix.Platform application references them exactly as any other .NET project
would.

The only contact with the CodeBrix.Platform family is through the PDF stage's font packages, which
arrive transitively:
[`CodeBrix.Platform.Fonts.Roboto.OflLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.Roboto.OflLicenseForever),
[`CodeBrix.Platform.Fonts.Merriweather.OflLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.Merriweather.OflLicenseForever),
[`CodeBrix.Platform.Fonts.RobotoMono.OflLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.RobotoMono.OflLicenseForever)
and
[`CodeBrix.Platform.Fonts.NotoMusic.OflLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.NotoMusic.OflLicenseForever).
`TexinfoPdfFonts.AddFontDirectory(directory)` probes a directory for exactly those package folders - the
`<Name>/Fonts/*.ttf` plus manifest layout they ship.

No native assets, no runtime identifier and no per-head package are ever required, so the same code runs
unchanged on every operating system a .NET 10 application targets.

## Pitfalls

- Do not treat the generated markup as web markup. The HTML and CSS are a contract with the HTML
  renderer's documented subset, not general-purpose web markup. It opens in a browser, but markup a
  browser tolerates is not automatically markup the PDF stage renders, so keep any post-processing
  inside that subset.
- Do not lose the pictures. The markup points at `<ImageFolderName>/<file>` relative to the document,
  not at where the files were found. Whatever directory the HTML lives in - or is handed to a PDF
  renderer as the base directory - must be the directory the pictures were copied into.
  `WriteToDirectory` does all of it; `CopyImagesTo(dir)` plus the same directory as base directory is
  the manual equivalent.
- Do not expect `result.Html` to link to nothing. With `EmitSingleFile` false, `Html` links to
  `CssFileName`; write the stylesheet beside it or use `ToHtmlDocument(result.Css)` to embed it.
- Options are live, not copies. `renderer.Options` - and, on the PDF renderer, both `Options.Texinfo`
  and `Options.Html` - belong to the renderers underneath and keep whatever you set for every later
  render. `DocumentTitle` and `DocumentAuthor` are the one exception.
- One renderer per thread. Neither `TexinfoHtmlRenderer` nor `TexinfoPdfRenderer` is thread-safe, and
  neither are the two renderers the PDF one owns.
- Do not expect exceptions for bad documents. Nothing about a document's contents throws, in either
  stage; a build gate has to read `result.Warnings`.
- Do not pattern-match whole warning messages. On the Texinfo stage the leading category word is the
  stable surface; on the PDF stage it is `Code`. Filter the split lists rather than the merged one,
  because a message means a different thing depending on which stage said it.
- Do not re-wrap a PDF-stage message. Pass the text of a `Warnings.PdfItems` entry through verbatim; the
  picture, reason or code point it names is its whole value.
- Do not pass the source directory as the base directory to `RenderHtmlDocument`. It must be the
  directory the pictures were copied into. `RenderFile`, `RenderHtml` and `RenderHtmlFile` never have
  this problem.
- Do not look for a pictures folder beside the PDF. There is none: staging is temporary and swept up. To
  get the HTML, CSS and pictures on disk, use `GenerateHtmlFromFile` plus `WriteToDirectory`.
- Do not expect text in an uncovered script to appear. The PDF stage never falls back to system fonts;
  register a font that covers the script with `includeInFallback: true`, or set
  `Options.Html.KeepUncoveredCharacters = true` to at least leave a visible trace.
- Do not expect `@page` in your CSS to be ignored. An `@page` rule in `ExtraCss` or a replacement
  stylesheet overrides `Options.Html`'s page size and margins. The generated stylesheet carries none, so
  by default `Options.Html` governs.
- Remember that `Intermediate` is null for `RenderHtmlFile` and `RenderHtmlDocument` - there was no
  Texinfo stage, and the warnings hold PDF-stage messages only.
- Do not expect music to be engraved. With no `SnippetRenderer` a snippet is its source text, by design.
  And do not implement `ILilypondSnippetRenderer` to throw: return `Failed(...)` instead.
- Do not confuse `snippet.IsInline` with `snippet.Options.Inline`. The first is where the snippet sits;
  the second is a request for a small engraving. A renderer usually wants both.
- Do not assume `LilypondSnippet.FilePath` holds a path. It is `""` when the named file was not found;
  decline rather than guess.
- Do not use relative entries in `IncludeSearchPaths` or `ImageSearchPaths` - they resolve against the
  process's current directory, not the document. Use absolute paths.
- Do not expect `.pdf` variants of pictures to be found; `.pdf` is not probed.
- Do not assume `@documentencoding` is obeyed. Convert a Latin-1 manual to UTF-8 first.
- Do not be surprised by double content under the `Print` profile when a manual writes the same visible
  text into both `@iftex` and `@ifnottex`. Both branches are read, deliberately.
- Do not add a second `PackageReference` to the HTML package when you already reference the PDF one; it
  flows through, and a mismatch turns into a restore conflict.
- Keep an `ILilypondSnippetRenderer` cheap to call - it is invoked synchronously on the rendering
  thread - and do font registration once at startup rather than per render.

## Documentation and source

The repository ships no samples, tools, demo applications or documentation folders. The two test
projects are the worked examples, and both packages carry a full API reference inside them.

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Texinfo/blob/main/README.md) |
| Texinfo2Html API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Texinfo/blob/main/AGENT-README.txt) |
| Texinfo2Pdf API guide (ships inside the package too) | [src/CodeBrix.Texinfo2Pdf/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Texinfo/blob/main/src/CodeBrix.Texinfo2Pdf/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Texinfo/blob/main/README-INDEX.txt) |
| Non-package content in the repository | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Texinfo/blob/main/EXTRAS-README.txt) |
| Tests for the HTML package | [tests/CodeBrix.Texinfo2Html.Tests](https://github.com/ellisnet/CodeBrix.Texinfo/tree/main/tests/CodeBrix.Texinfo2Html.Tests) |
| Tests for the PDF package | [tests/CodeBrix.Texinfo2Pdf.Tests](https://github.com/ellisnet/CodeBrix.Texinfo/tree/main/tests/CodeBrix.Texinfo2Pdf.Tests) |

XML documentation ships alongside both assemblies. Several test suites run against outside manual sets
that are not in the repository and skip cleanly when they are absent; every committed fixture is an
original document written for its test.

## License

CodeBrix.Texinfo is licensed under the MIT License, and the license is also named in both package IDs:
`CodeBrix.Texinfo2Html.MitLicenseForever` and `CodeBrix.Texinfo2Pdf.MitLicenseForever` carry the same
terms. The font packages the PDF stage brings with it are OFL-licensed. For the provenance and licensing
of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Texinfo/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.PdfDocuments](CodeBrix.PdfDocuments.md) - the PDF stage underneath, and every option `Options.Html` exposes
- [CodeBrix.LilyPort](CodeBrix.LilyPort.md) - a producer of the `.tely` dialect these libraries read
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.Texinfo on GitHub](https://github.com/ellisnet/CodeBrix.Texinfo) - source and tests
