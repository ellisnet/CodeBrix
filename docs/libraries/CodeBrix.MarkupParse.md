<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.MarkupParse</sub>

# CodeBrix.MarkupParse

**CodeBrix.MarkupParse parses HTML into a fully navigable DOM tree that you query with CSS selectors, traverse, modify and serialize back to HTML.** It reads strings, streams, character buffers, a `TextSource` or a URL, and it is fully managed with no dependencies beyond .NET itself. Reach for it from any .NET 10 application, or from a CodeBrix.Platform application, whenever you need to read or rewrite markup rather than render it.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.MarkupParse](https://github.com/ellisnet/CodeBrix.MarkupParse) |
| **Packages** | [`CodeBrix.MarkupParse.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.MarkupParse.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later; no other dependencies |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Windows, macOS and Linux - fully managed, no native libraries |

## What it does

- Parses HTML strings, streams, `char[]`, `ReadOnlyMemory<char>`, a `TextSource` or a URL into a complete DOM tree (`IHtmlDocument`).
- Queries elements with CSS selectors through `QuerySelector` and `QuerySelectorAll`, including typed generic overloads.
- Queries with LINQ over the document's `All` collection, and with the classic `GetElementById`, `GetElementsByClassName`, `GetElementsByTagName` and `GetElementsByName` methods.
- Traverses the tree: parent, children, siblings, descendants and ancestors.
- Manipulates the tree: create, append, insert, remove, replace and clone elements and nodes.
- Reads and modifies attributes, classes, IDs and text content, and reads and writes `InnerHtml`, `OuterHtml` and `TextContent` on any element.
- Serializes the DOM back to HTML with four formatters: standard, pretty-printed, minified and XHTML.
- Parses HTML fragments (partial HTML without a full document) and parses only the `<head>` section when all you need is metadata.
- Tracks source positions of parsed elements - line, column and offset - and raises callbacks for element creation and token events.
- Loads documents from URLs asynchronously with `WithDefaultLoader()`, and handles cookies with `WithDefaultCookies()`.
- Exposes a large set of typed `IHtml*Element` interfaces, so `<a>`, `<input>`, `<select>`, `<table>`, `<img>`, `<meta>` and friends expose their real properties instead of raw attribute strings.
- Handles forms: reading and setting field values, validation, and submission.
- Parses SVG and MathML elements inside HTML documents.

## When to use it

Use CodeBrix.MarkupParse when the input is HTML and you need a structured, queryable model of it: scraping data out of a page, rewriting markup before you store it, pulling metadata out of a `<head>`, or driving a form. The library implements the HTML parsing algorithm, which is deliberately lenient in ways XML is not, so it copes with the real-world markup that `System.Xml` rejects.

It parses CSS *selectors* for querying, but it does not parse or evaluate CSS *stylesheets* - the sibling [CodeBrix.StyleSheetParse](CodeBrix.StyleSheetParse.md) covers that. These are the other things it deliberately does not do:

- No JavaScript or other script execution. `<script>` content is text.
- No rendering. There is no layout, no box model, no measurement, no image or PDF output, and therefore no geometry properties.
- No general-purpose HTTP. It can load documents from URLs, but it is not an HTTP client - use `HttpClient` for API calls.
- No XML parsing. Use `System.Xml` for that.
- No Markdown or JSON parsing.
- No browser behavior: no navigation history, no rendering-dependent pseudo-classes, no `getComputedStyle`, no viewport.

> [!WARNING]
> This library is not an HTML sanitizer. There is no allow-list scrubber, and parsing untrusted markup and re-serializing it is not a security boundary.

## Getting started

```bash
dotnet add package CodeBrix.MarkupParse.MitLicenseForever
```

Or, in the project file, where NuGet resolves the package for you:

```xml
<PackageReference Include="CodeBrix.MarkupParse.MitLicenseForever" />
```

For most HTML parsing tasks, copy this block of usings:

```csharp
using CodeBrix.MarkupParse;
using CodeBrix.MarkupParse.Html.Parser;
using CodeBrix.MarkupParse.Dom;
```

This is a complete program that parses a list and prints its items:

```csharp
using System;
using System.Linq;
using CodeBrix.MarkupParse.Dom;
using CodeBrix.MarkupParse.Html.Parser;

var parser = new HtmlParser();
var html = "<ul><li>Item 1</li><li>Item 2</li><li>Item 3</li></ul>";
var document = parser.ParseDocument(html);

var items = document.QuerySelectorAll("li")
    .Select(li => li.TextContent)
    .ToList();

foreach (var item in items)
{
    Console.WriteLine(item);
}
```

Nothing else is required: there is no initialization call, no configuration file and no native dependency. Add `.WithDefaultLoader()` and a browsing context only when you need to fetch documents over HTTP.

The public surface is split across a small set of namespaces.

| Namespace | What it holds |
| --- | --- |
| `CodeBrix.MarkupParse` | `Configuration`, `BrowsingContext`, `ConfigurationExtensions`, `BrowsingContextExtensions`, `FormatExtensions` (`ToHtml`) |
| `CodeBrix.MarkupParse.Html.Parser` | `HtmlParser`, `HtmlParserOptions`, `HtmlParserExtensions` |
| `CodeBrix.MarkupParse.Html.Dom` | `IHtmlDocument`, the typed `IHtml*Element` interfaces, `FormExtensions` |
| `CodeBrix.MarkupParse.Dom` | `IDocument`, `IElement`, `INode`, `INodeList`, `QueryExtensions`, `SelectorExtensions`, `NodeExtensions`, `ParentNodeExtensions`, `AdjacentPosition` |
| `CodeBrix.MarkupParse.Html` | `HtmlMarkupFormatter`, `PrettyMarkupFormatter`, `MinifyMarkupFormatter` |
| `CodeBrix.MarkupParse.Xhtml` | `XhtmlMarkupFormatter` |
| `CodeBrix.MarkupParse.Css.Dom` and `.Css.Parser` | `ISelector` and the CSS selector types, `CssSelectorParser` |
| `CodeBrix.MarkupParse.Text` | `TextPosition`, `TextRange`, `TextSource` |
| `CodeBrix.MarkupParse.Io` | `LoaderOptions`, `IRequester`, `DefaultHttpRequester` |

## Key concepts

### The parser

`HtmlParser`, in `CodeBrix.MarkupParse.Html.Parser`, is the primary entry point and can be instantiated directly without any configuration.

```csharp
public HtmlParser()
public HtmlParser(HtmlParserOptions options)
public HtmlParser(HtmlParserOptions options, IBrowsingContext context)

public HtmlParserOptions Options { get; }

public IHtmlDocument ParseDocument(string source)
public IHtmlDocument ParseDocument(Stream source)
public IHtmlDocument ParseDocument(char[] source, int length = 0)
public IHtmlDocument ParseDocument(ReadOnlyMemory<char> chars)
public IHtmlDocument ParseDocument(TextSource source)
public IHtmlHeadElement ParseHead(string source)
public IHtmlHeadElement ParseHead(Stream source)
public INodeList ParseFragment(string source, IElement contextElement)
public INodeList ParseFragment(Stream source, IElement contextElement)

public Task<IHtmlDocument> ParseDocumentAsync(string source,
                                             CancellationToken cancel)
public Task<IHtmlDocument> ParseDocumentAsync(Stream source,
                                             CancellationToken cancel)
public Task<IHtmlHeadElement> ParseHeadAsync(string source,
                                             CancellationToken cancel)
public Task<IHtmlHeadElement> ParseHeadAsync(Stream source,
                                             CancellationToken cancel)
```

The parser also raises `Parsing`, `Parsed` and `Error` events (of type `DomEventHandler`) if you need to observe the parse. `HtmlParser` does not implement `IDisposable`, so no `using` statement is needed and instances can be reused; `IDocument` - and therefore `IHtmlDocument` - does implement `IDisposable`, and you should dispose documents when finished, especially when a browsing context loaded them.

### Fragments and heads

A fragment needs a context element, because HTML parsing rules depend on where the markup would sit: the same markup parses differently inside `<table>` than inside `<div>`.

```csharp
var parser = new HtmlParser();
var document = parser.ParseDocument("");
var body = document.Body;
INodeList nodes = parser.ParseFragment("<li>Item 1</li><li>Item 2</li>",
                                       body);
```

When you only need metadata, `ParseHead` stops once the head is complete instead of building the whole tree.

```csharp
var parser = new HtmlParser();
IHtmlHeadElement head = parser.ParseHead(
    "<html><head><title>My Page</title></head><body>...</body></html>");
//head.QuerySelector("title").TextContent == "My Page"
```

### Parser options

`HtmlParserOptions` is a struct; every member is a settable property and every default is `false` or `null`.

```csharp
var parser = new HtmlParser(new HtmlParserOptions
{
    //Keep references to original source positions on elements
    IsKeepingSourceReferences = true,

    //Treat parse errors as exceptions
    IsStrictMode = false,

    //Preserve original attribute name casing (normally lowercased)
    IsPreservingAttributeNames = false,

    //Allow custom elements everywhere (not just where the spec allows)
    IsAcceptingCustomElementsEverywhere = false,

    //Disable frame support (ignore <frame>, respect <noframes>)
    IsNotSupportingFrames = false,

    //Avoid consuming character references (e.g. &amp;)
    IsNotConsumingCharacterReferences = false,

    //Parse XML processing instructions into DOM nodes
    IsSupportingProcessingInstructions = false,

    //Callback when each element is created during parsing
    OnCreated = (IElement element, TextPosition position) =>
    {
        //Called for every element during parsing
    },

    //Callback when each token is read during tokenization
    OnToken = (HtmlToken token, TextRange range) =>
    {
        //Called for every token during parsing
    },
});
```

The struct also carries `IsEmbedded`, `IsScripting`, `DisableElementPositionTracking`, `ShouldEmitAttribute`, and the tokenizer skip switches `SkipComments`, `SkipPlaintext`, `SkipRCDataText`, `SkipCDATA`, `SkipProcessingInstructions`, `SkipDataText`, `SkipScriptText` and `SkipRawText`. The skip switches drop the corresponding content instead of materializing it, which is how you strip comments or script bodies at parse time rather than afterwards.

### Querying with CSS selectors

`QuerySelector` returns the first matching element or `null`; `QuerySelectorAll` returns all of them. Both can be called on any element, not only on the document, and both have typed generic forms.

```csharp
IElement element = document.QuerySelector("div.content");
IHtmlCollection<IElement> elements = document.QuerySelectorAll("li.blue");
```

`IDocument` and `IElement` both implement `IParentNode`, which declares `QuerySelector(string)` and `QuerySelectorAll(string)` directly. The typed generic forms, the `INodeList` forms and the pre-compiled `ISelector` forms come from `QueryExtensions` in `CodeBrix.MarkupParse.Dom`, which also supplies `GetElementsByClassName`, `GetElementsByTagName` and a scope-node parameter.

The selector engine supports type, class and ID selectors; attribute selectors (`[href]`, `[type="text"]`, `[class~="foo"]`, `[lang|="en"]`, `[href^="https"]`, `[src$=".png"]`, `[title*="hello"]`); pseudo-classes (`:first-child`, `:last-child`, `:nth-child(2n+1)`, `:nth-of-type(odd)`, `:not(.excluded)`, `:empty`, `:checked`, `:enabled`, `:disabled`, `:scope` and more); pseudo-elements (`::before`, `::after`, `::first-line`, `::first-letter`); the combinators (descendant, child `>`, adjacent sibling `+`, general sibling `~`); compound selectors; selector lists; the universal selector `*`; and namespace selectors. Interaction-state pseudo-classes such as `:hover` need a rendering engine, so they never match here.

`SelectorExtensions` adds helper methods over any `IEnumerable<T>` of nodes: `Eq`, `Gt`, `Lt`, `Even`, `Odd`, `Filter`, `Not`, `Children`, `Siblings`, `Parent`, `Next` and `Previous`. Each of `Filter`, `Not`, `Children`, `Siblings`, `Parent`, `Next` and `Previous` also has an overload taking a pre-parsed `ISelector`, plus `Is<T>(ISelector)`.

### Traversing and editing the tree

Every `INode` exposes `Parent`, `ParentElement`, `ChildNodes`, `FirstChild`, `LastChild`, `NextSibling`, `PreviousSibling` and `Contains(child)`; every `IElement` adds `NextElementSibling` and `PreviousElementSibling`. `IDocument` is the factory for every node type - `CreateElement`, `CreateTextNode`, `CreateComment`, `CreateDocumentFragment` and `CreateAttribute`. Nodes are added with `AppendChild`, `InsertBefore`, `Append`, `Prepend`, `Before` and `After`, removed with `RemoveChild` or `element.Remove()`, and replaced with `ReplaceChild` or `element.Replace(...)`. `IElement.Insert(AdjacentPosition, string)` inserts markup at `BeforeBegin`, `AfterBegin`, `BeforeEnd` or `AfterEnd`, and `INode.Clone(bool deep = true)` copies a subtree - note that the default is a deep clone.

<details>
<summary>The full NodeExtensions surface (namespace CodeBrix.MarkupParse.Dom)</summary>

```csharp
public static INode GetRoot(this INode node)
public static IEnumerable<INode> GetDescendants(this INode parent)
public static IEnumerable<INode> GetDescendantsAndSelf(this INode parent)
public static IEnumerable<INode> GetAncestors(this INode node)
public static IEnumerable<INode> GetInclusiveAncestors(this INode node)
public static T GetAncestor<T>(this INode node)
public static bool IsDescendantOf(this INode node, INode parent)
public static bool IsAncestorOf(this INode parent, INode node)
public static bool IsInclusiveDescendantOf(this INode node, INode parent)
public static bool IsInclusiveAncestorOf(this INode parent, INode node)
public static bool IsSiblingOf(this INode node, INode element)
public static bool IsPreceding(this INode before, INode after)
public static bool IsFollowing(this INode after, INode before)
public static int Index(this INode node)
public static int IndexOf(this INode parent, INode node)
public static TNode FindChild<TNode>(this INode parent)
public static TNode FindDescendant<TNode>(this INode parent,
                                          int maxDepth = 1024)
public static bool HasTextNodes(this INode node)
public static int GetElementCount(this INode parent)
public static Url HyperReference(this INode node, string url)
public static string Text(this INode node)
public static T Text<T>(this T nodes, string text)
```

`GetDescendants` walks in tree order and excludes the node itself; `GetDescendantsAndSelf` includes it. `HyperReference` resolves a relative URL against the node's base URL, which is how you turn a raw `href` attribute into an absolute address.

</details>

### Typed HTML element interfaces

Every HTML element in a parsed document is really one of the typed interfaces in `CodeBrix.MarkupParse.Html.Dom`, each of which extends `IHtmlElement` (and therefore `IElement` and `INode`). Casting to the typed interface - normally with the generic query overloads - gives you real, typed properties instead of raw attribute strings.

```csharp
var form = document.QuerySelector<IHtmlFormElement>("form");
var anchors = document.QuerySelectorAll<IHtmlAnchorElement>("a");
```

Watch the non-obvious names: `<br>` is `IHtmlBreakRowElement`, `<iframe>` is `IHtmlInlineFrameElement`, `<optgroup>` is `IHtmlOptionsGroupElement`, `<ol>` and `<ul>` are `IHtmlOrderedListElement` and `IHtmlUnorderedListElement`, `<li>` is `IHtmlListItemElement`, `<blockquote>` and `<q>` are `IHtmlQuoteElement`, `<ins>` and `<del>` are `IHtmlModElement`, `<hr>` is `IHtmlHrElement`, and an unrecognized tag is `IHtmlUnknownElement`. Alongside the `IHtml*` set there are typed SVG interfaces in `CodeBrix.MarkupParse.Svg.Dom` and MathML interfaces in `CodeBrix.MarkupParse.Mathml.Dom`, reached the same way.

<details>
<summary>The complete set of typed element interfaces</summary>

```csharp
IHtmlElement                    (the base for all of the below)
IHtmlAnchorElement              IHtmlAreaElement
IHtmlAudioElement               IHtmlBaseElement
IHtmlBodyElement                IHtmlBreakRowElement
IHtmlButtonElement              IHtmlCanvasElement
IHtmlCommandElement             IHtmlDataElement
IHtmlDataListElement            IHtmlDetailsElement
IHtmlDialogElement              IHtmlDivElement
IHtmlEmbedElement               IHtmlFieldSetElement
IHtmlFormElement                IHtmlHeadElement
IHtmlHeadingElement             IHtmlHrElement
IHtmlHtmlElement                IHtmlImageElement
IHtmlInlineFrameElement         IHtmlInputElement
IHtmlKeygenElement              IHtmlLabelElement
IHtmlLegendElement              IHtmlLinkElement
IHtmlListItemElement            IHtmlMapElement
IHtmlMarqueeElement             IHtmlMediaElement
IHtmlMenuElement                IHtmlMenuItemElement
IHtmlMetaElement                IHtmlMeterElement
IHtmlModElement                 IHtmlObjectElement
IHtmlOptionElement              IHtmlOptionsGroupElement
IHtmlOrderedListElement         IHtmlOutputElement
IHtmlParagraphElement           IHtmlParamElement
IHtmlPictureElement             IHtmlPreElement
IHtmlProgressElement            IHtmlQuoteElement
IHtmlScriptElement              IHtmlSelectElement
IHtmlSlotElement                IHtmlSourceElement
IHtmlSpanElement                IHtmlStyleElement
IHtmlTableCaptionElement        IHtmlTableCellElement
IHtmlTableColumnElement         IHtmlTableDataCellElement
IHtmlTableElement               IHtmlTableHeaderCellElement
IHtmlTableRowElement            IHtmlTableSectionElement
IHtmlTemplateElement            IHtmlTextAreaElement
IHtmlTimeElement                IHtmlTitleElement
IHtmlTrackElement               IHtmlUnknownElement
IHtmlUnorderedListElement       IHtmlVideoElement
```

</details>

### Forms

`IHtmlFormElement` is the entry point, with `AcceptCharset`, `Action`, `Autocomplete`, `Enctype`, `Encoding`, `Method`, `Name`, `NoValidate`, `Target`, `Length`, an `IHtmlFormControlsCollection Elements`, indexers by position and by name, and the methods `SubmitAsync()`, `GetSubmission()`, `Reset()`, `CheckValidity()`, `ReportValidity()` and `RequestAutocomplete()`. `FormExtensions`, in the same namespace, adds the shortcuts:

```csharp
public static IHtmlFormElement SetValues(this IHtmlFormElement form,
    IDictionary<string, string> fields, bool createMissing = false)
public static Task<IDocument> SubmitAsync(this IHtmlFormElement form,
    object fields)
public static Task<IDocument> SubmitAsync(this IHtmlFormElement form,
    IDictionary<string, string> fields, bool createMissing = false)
public static Task<IDocument> SubmitAsync(this IHtmlElement element,
    object fields = null)
public static Task<IDocument> SubmitAsync(this IHtmlElement element,
    IDictionary<string, string> fields, bool createMissing = false)
```

`SetValues` matches each dictionary key against the `name` of a form control and assigns the value; `createMissing: true` adds a hidden input for any key that has no matching control instead of ignoring it. The `object fields` overloads take an anonymous object and turn its properties into that dictionary. The `IHtmlElement` overloads submit the form that the given element belongs to, using that element as the submitter, which is how you model "the user clicked this submit button". Submission performs a real HTTP request, so it needs a document loaded from a browsing context configured with `.WithDefaultLoader()`; `GetSubmission()` returns the `DocumentRequest` that would be sent without sending it. Reading values back is done through the controls themselves - there is no `GetValues` counterpart.

### Serializing back to HTML

Four formatters serialize the DOM back to markup: `HtmlMarkupFormatter` (standard HTML5, the default), `PrettyMarkupFormatter` (indented), `MinifyMarkupFormatter` (whitespace-minimized) and `XhtmlMarkupFormatter`. The first three live in `CodeBrix.MarkupParse.Html`; `XhtmlMarkupFormatter` lives in `CodeBrix.MarkupParse.Xhtml`. `HtmlMarkupFormatter` and `XhtmlMarkupFormatter` each expose a shared `public static readonly IMarkupFormatter Instance`; the other two carry per-instance options, so construct those.

```csharp
//Default HTML output (what ToHtml(), OuterHtml and InnerHtml use)
string html = document.ToHtml();

//Pretty-printed output
string pretty = document.ToHtml(new PrettyMarkupFormatter());

//Minified output
string minified = document.ToHtml(new MinifyMarkupFormatter());

//XHTML output
string xhtml = document.ToHtml(XhtmlMarkupFormatter.Instance);

//Shorthands for the two option-less cases
string quickMin = document.Minify();      //== ToHtml(new MinifyMarkupFormatter())
string quickPretty = document.Prettify(); //== ToHtml(new PrettyMarkupFormatter())

//Write to a TextWriter or Stream
document.ToHtml(textWriter);
await document.ToHtmlAsync(stream);
```

`PrettyMarkupFormatter` exposes `Indentation` (a tab by default) and `NewLine`, plus a constructor taking `IEnumerable<INode> preserveTextFormatting` for nodes whose inner whitespace must be left exactly as parsed. `MinifyMarkupFormatter` exposes `PreservedTags`, `ShouldKeepStandardElements`, `ShouldKeepComments`, `ShouldKeepAttributeQuotes`, `ShouldKeepEmptyAttributes` and `ShouldKeepImpliedEndTag`. `XhtmlMarkupFormatter` takes an `emptyTagsToSelfClosing` flag and exposes `IsSelfClosingEmptyTags`.

### Source positions

Turn on `IsKeepingSourceReferences` and every element carries an `ISourceReference`; the `OnCreated` and `OnToken` callbacks answer the same question during the parse, and `OnToken` gives start *and* end positions.

```csharp
public int Line { get; }       //1-based line number
public int Column { get; }     //1-based column number
public int Position { get; }   //1-based character offset from the start
public int Index { get; }      //Position - 1, i.e. the 0-based offset
public static readonly TextPosition Empty
```

`TextPosition` also supports comparison and `Shift(int columns)` / `After(char)` / `After(string)` for advancing a position, and `TextRange` pairs two of them as `Start` and `End`. `Index` is the property you want when slicing the original source string; `Position` is 1-based and will be off by one.

### Loading documents from a URL

Loading over HTTP is the one feature that needs configuration: a `Configuration` with a loader, and a `BrowsingContext` built from it.

```csharp
using CodeBrix.MarkupParse;
using CodeBrix.MarkupParse.Dom;
using System.Linq;

//Configure with the default HTTP loader
var config = Configuration.Default.WithDefaultLoader();

//Create a browsing context and load a URL
var context = BrowsingContext.New(config);
var document = await context.OpenAsync("https://example.com");

//Query the loaded document
var title = document.Title;
var links = document.QuerySelectorAll("a")
    .Select(a => a.GetAttribute("href"));
```

> [!IMPORTANT]
> You must call `.WithDefaultLoader()` on the configuration to enable document loading from URLs. Without it, `OpenAsync()` fails or returns an empty document.

Add `.WithDefaultCookies()` for cookie handling. `context.OpenAsync(req => req.Content("<h1>Hello</h1>"))` builds a document from a virtual response with no network at all, which is what tests want, and `context.OpenNewAsync()` opens a blank document. `Configuration` is immutable and fluent: each `With`/`Without` call returns a *new* configuration instance, so assign the result and pass that to `BrowsingContext.New()`. `ConfigurationExtensions` also supplies `WithCulture`, `WithMetaRefresh`, `WithLocaleBasedEncoding`, `With<TService>`, `WithOnly<TService>`, `Without<TService>` and `Has<TService>`. `LoaderOptions` (namespace `CodeBrix.MarkupParse.Io`) carries `IsNavigationDisabled`, `IsResourceLoadingEnabled` and a `Predicate<Request> Filter` - return `false` from the filter to refuse an individual request.

## Examples

Parse a page and project the elements you care about into your own shape:

```csharp
using CodeBrix.MarkupParse.Html.Parser;
using System.Linq;

var parser = new HtmlParser();
var document = parser.ParseDocument(@"
    <html>
    <body>
        <h1>Products</h1>
        <ul>
            <li class='product' data-price='10.99'>Widget</li>
            <li class='product' data-price='24.99'>Gadget</li>
            <li class='product sale' data-price='7.50'>Doohickey</li>
        </ul>
    </body>
    </html>");

//Get all product names
var products = document.QuerySelectorAll("li.product")
    .Select(el => new
    {
        Name = el.TextContent,
        Price = el.GetAttribute("data-price"),
        OnSale = el.ClassList.Contains("sale")
    });
```

The typed generic overloads unlock the properties that a raw `GetAttribute` call cannot give you:

```csharp
using CodeBrix.MarkupParse.Html.Parser;
using CodeBrix.MarkupParse.Html.Dom;
using System.Linq;

var parser = new HtmlParser();
var document = parser.ParseDocument(htmlString);

var links = document.QuerySelectorAll<IHtmlAnchorElement>("a")
    .Select(a => new { Text = a.TextContent, Href = a.Href })
    .ToList();

var images = document.QuerySelectorAll<IHtmlImageElement>("img")
    .Select(i => new { i.Source, i.AlternativeText })
    .ToList();

var description = document
    .QuerySelector<IHtmlMetaElement>("meta[name=description]")
    ?.Content;
```

Editing works the same way whether the tree came from a string or from the network:

```csharp
using CodeBrix.MarkupParse.Html.Parser;

var parser = new HtmlParser();
var document = parser.ParseDocument("<h1>Hello</h1><p>World</p>");

//Add a new paragraph
var p = document.CreateElement("p");
p.TextContent = "Added dynamically";
p.SetAttribute("class", "dynamic");
document.Body.AppendChild(p);

//Modify existing elements
var h1 = document.QuerySelector("h1");
h1.TextContent = "Modified Title";
h1.SetAttribute("id", "main-title");

//Remove an element
var oldP = document.QuerySelector("p");
oldP.Remove();

Console.WriteLine(document.Body.InnerHtml);
```

Filling in and submitting a form, first inspecting what would be sent and then sending it:

```csharp
using System.Collections.Generic;
using CodeBrix.MarkupParse;
using CodeBrix.MarkupParse.Dom;
using CodeBrix.MarkupParse.Html.Dom;

var config = Configuration.Default.WithDefaultLoader().WithDefaultCookies();
var context = BrowsingContext.New(config);
using var document = await context.OpenAsync("https://example.com/login");

var form = document.QuerySelector<IHtmlFormElement>("form#login");

//Inspect what WOULD be sent, without sending it
form.SetValues(new Dictionary<string, string>
{
    ["username"] = "someone",
    ["password"] = "s3cret",
});
var request = form.GetSubmission();
//DocumentRequest exposes Method (HttpMethod), Target (Url), Body (Stream),
//MimeType, Headers, Referer and Source
Console.WriteLine($"{request.Method} {request.Target}");

//Or set the fields and submit in one call
using var result = await form.SubmitAsync(
    new { username = "someone", password = "s3cret" });

Console.WriteLine(result.QuerySelector("h1")?.TextContent);
```

## Using it in a CodeBrix.Platform application

Add the package to your `.Core` library and call it from your services and view models. There is nothing to register at start-up, and there is no UI surface, no native library and no platform-specific code path, so the library behaves identically on every head.

Give each thread its own `HtmlParser`: a shared instance is not thread-safe, and one document should not be mutated from several threads. Parser instances are otherwise cheap to keep around and reuse. Dispose documents that came from a browsing context when you are done with them.

## Pitfalls

- The package ID is `CodeBrix.MarkupParse.MitLicenseForever`; the namespaces are `CodeBrix.MarkupParse.*`. The license suffix belongs to the package ID only, never to a namespace or the assembly.
- `QuerySelector` returns `null` when nothing matches. Always null-check, or use `?.`.
- Forgetting `using CodeBrix.MarkupParse.Dom;` hides the typed generic overloads, `Text()` and the other extension methods that live in that namespace; forgetting `using CodeBrix.MarkupParse.Html.Parser;` hides `HtmlParser` and the async parse extensions.
- `TextContent`, `InnerHtml` and `OuterHtml` are three different things: plain text gathered recursively from descendant text nodes, the markup of the element's children, and the element itself plus its `InnerHtml`.
- `TagName` is uppercase (`"DIV"`), `LocalName` is lowercase (`"div"`). Compare with `LocalName`, or compare case-insensitively.
- `:hover`, `:active` and `:focus` are interaction-state pseudo-classes that need a rendering engine, so they never match.
- `Configuration` is immutable. Every `With...`/`Without...` call returns a new instance; assign the result and pass *that* to `BrowsingContext.New()`.
- `element.Clone()` defaults to `deep: true`. Pass `false` when you want a shallow copy.
- Template children are not ordinary descendants: `IHtmlTemplateElement.Content` is a separate `IDocumentFragment`, so a selector run on the surrounding tree will not find them.
- `TextPosition.Position` is 1-based; use `TextPosition.Index` to slice the original source string.
- `ParseFragment` requires a context element, because HTML parsing rules depend on the surrounding element.
- `PrettyMarkupFormatter.Indentation` defaults to a tab. Set it to spaces if that is what you want.
- `ToHtml()` and the `OuterHtml` property produce the same output with the default formatter, but only `ToHtml()` accepts an `IMarkupFormatter` for pretty, minified or XHTML output.
- Serializing the DOM to a string is relatively expensive. To read data, use `TextContent` or `InnerHtml` directly rather than serializing the document.

## Samples and tools in the repository

This repository ships no sample applications and no tools: it contains one packable project and one test project. The test project is the worked-example set, and it carries a substantial body of test data. Run it with `dotnet test CodeBrix.MarkupParse.slnx`; it needs no environment variables, no opt-in switches and no special preparation.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| `Examples/WikiTests.cs` | Parsing, querying and manipulation | [`tests/CodeBrix.MarkupParse.Tests/Examples`](https://github.com/ellisnet/CodeBrix.MarkupParse/tree/main/tests/CodeBrix.MarkupParse.Tests/Examples) |
| `Examples/ReadmeTests.cs` | Loading a URL through a browsing context | [`tests/CodeBrix.MarkupParse.Tests/Examples`](https://github.com/ellisnet/CodeBrix.MarkupParse/tree/main/tests/CodeBrix.MarkupParse.Tests/Examples) |
| `Examples/Questions.cs` | Source-position tracking | [`tests/CodeBrix.MarkupParse.Tests/Examples`](https://github.com/ellisnet/CodeBrix.MarkupParse/tree/main/tests/CodeBrix.MarkupParse.Tests/Examples) |
| `Examples/FormsTests.cs` | Form submission | [`tests/CodeBrix.MarkupParse.Tests/Examples`](https://github.com/ellisnet/CodeBrix.MarkupParse/tree/main/tests/CodeBrix.MarkupParse.Tests/Examples) |
| `Html/` | Tokenization, tree construction, DOM behavior, tables, SVG and MathML in HTML | [`tests/CodeBrix.MarkupParse.Tests/Html`](https://github.com/ellisnet/CodeBrix.MarkupParse/tree/main/tests/CodeBrix.MarkupParse.Tests/Html) |
| `Library/` | Formatters, async parsing, configuration, browsing context, parser options, DOM extensions and the form suites | [`tests/CodeBrix.MarkupParse.Tests/Library`](https://github.com/ellisnet/CodeBrix.MarkupParse/tree/main/tests/CodeBrix.MarkupParse.Tests/Library) |
| `Css/` | CSS selector coverage, including the W3C CSS3 Selectors test suite | [`tests/CodeBrix.MarkupParse.Tests/Css`](https://github.com/ellisnet/CodeBrix.MarkupParse/tree/main/tests/CodeBrix.MarkupParse.Tests/Css) |
| `Urls/` and `Io/` | URL parsing, validation, the URL API and MIME types | [`tests/CodeBrix.MarkupParse.Tests/Urls`](https://github.com/ellisnet/CodeBrix.MarkupParse/tree/main/tests/CodeBrix.MarkupParse.Tests/Urls) |

> [!NOTE]
> A small number of tests fetch live pages. Those known to be unreliable against the public internet carry an explicit skip reason; treat skipped network tests as documentation of the URL-loading API rather than as runnable examples.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.MarkupParse/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.MarkupParse/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.MarkupParse/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.MarkupParse.Tests](https://github.com/ellisnet/CodeBrix.MarkupParse/tree/main/tests/CodeBrix.MarkupParse.Tests) |

## License

CodeBrix.MarkupParse is licensed under the MIT License; the license is also named in the package ID (`CodeBrix.MarkupParse.MitLicenseForever`). The package requires license acceptance, so a restore asks the user to accept it. For the provenance and licensing of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.MarkupParse/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.StyleSheetParse](CodeBrix.StyleSheetParse.md) - parse the CSS stylesheets this library deliberately leaves alone
- [Project architecture](../platform/04-project-architecture.md) - where a library package belongs in a CodeBrix.Platform solution
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.MarkupParse on GitHub](https://github.com/ellisnet/CodeBrix.MarkupParse) - source, tests and samples
