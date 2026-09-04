<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.StyleSheetParse</sub>

# CodeBrix.StyleSheetParse

**CodeBrix.StyleSheetParse turns CSS text into a strongly typed object model that you can query, manipulate and serialize back to CSS.** It models style rules, declarations, at-rules and media queries, parses selectors on their own and computes their specificity, and gives you a vocabulary of typed CSS value types to convert the strings you read. It is fully managed with no dependencies beyond .NET, and it is used from any .NET 10 application or from a CodeBrix.Platform application.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.StyleSheetParse](https://github.com/ellisnet/CodeBrix.StyleSheetParse) |
| **Packages** | [`CodeBrix.StyleSheetParse.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.StyleSheetParse.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later; no other dependencies |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Any platform .NET 10 supports - fully managed, no native libraries |

## What it does

- Parses CSS from a string or a `Stream`, synchronously or asynchronously, with cancellation support.
- Walks the rule tree: `@media`, `@supports`, `@container`, `@keyframes`, `@font-face`, `@import`, `@namespace`, `@charset`, `@page`, `@document` and `@viewport`.
- Reads and writes style declarations by property name, including the `!important` priority.
- Parses selectors into a selector object model and computes CSS specificity, with comparison operators on the specificity value itself.
- Models media queries down to individual media features, and container queries with their names and conditions.
- Serializes the model back to CSS text through a pluggable formatter.
- Offers configurable parser tolerance for unknown rules, unknown declarations, invalid selectors, invalid values, invalid constraints, comments and duplicate properties.
- Ships a vocabulary of typed CSS value types - `Length`, `Angle`, `Time`, `Frequency`, `Resolution`, `Number`, `Percent`, `Color` and more - with parse helpers you point at the strings the declaration API returns.

## When to use it

Use CodeBrix.StyleSheetParse when you need to read, analyze or rewrite CSS: extracting the declarations a rule sets, ranking selectors by specificity, editing a media query in place, or generating CSS from a model you built. It is a parser and an object model - it is not a rendering engine, a style resolver or a CSS validator.

The sibling [CodeBrix.MarkupParse](CodeBrix.MarkupParse.md) parses HTML and CSS selectors for querying a DOM; this library is the one that understands stylesheets. [CodeBrix.SvgParse](CodeBrix.SvgParse.md) takes this package as its single dependency and uses it for CSS styling support.

These are the things it deliberately does not do:

- No rendering, and no applying of styles to elements.
- No matching of selectors against a document. `ISelector` has no `Match` method and there is no DOM here; selectors carry text, type and specificity.
- No computed, cascaded or inherited styles.
- No validation against a specific CSS specification version. `@supports` `Condition.Check()` only reports whether this library can parse the declaration.
- No minifying or prettifying. The built-in `CompressedStyleFormatter` emits readable spacing; implement `IStyleFormatter` for anything else.
- No resolving of `@import` rules - the URL is parsed but never fetched.
- No evaluating of media or container queries against a device context.
- No reading of CSS comments back out, and no reporting of parse errors to your code.
- No preprocessing of Sass, SCSS or Less, no CSS module scoping or transformation, and no plugin pipeline.
- No resolution of CSS custom properties: `var()` is preserved as text, never substituted.

## Getting started

```bash
dotnet add package CodeBrix.StyleSheetParse.MitLicenseForever
```

There is exactly one public namespace, so one using directive reaches everything:

```csharp
using CodeBrix.StyleSheetParse;         // everything public lives here
```

Consumer code usually wants these as well:

```csharp
using System.Linq;                      // OfType<T>(), First(), Where()
using System.IO;                        // Stream, TextWriter, StreamWriter
using System.Threading;                 // CancellationToken
using System.Threading.Tasks;           // Task, await
```

This is a complete program: it parses CSS, prints every declaration with its `!important` flag, builds a rule from scratch and round-trips the sheet.

```csharp
using System;
using System.IO;
using System.Linq;
using CodeBrix.StyleSheetParse;

var css = args.Length > 0
    ? File.ReadAllText(args[0])
    : "h1 { color: red; margin: 5px } @media print { h1 { color: black } }";

var parser = new StylesheetParser();
var sheet = parser.Parse(css);

foreach (var rule in sheet.StyleRules)
{
    Console.WriteLine(rule.SelectorText + "  " + rule.Selector.Specificity);
    foreach (var declaration in rule.Style.Declarations)
    {
        Console.Write($"    {declaration.Name}: {declaration.Value}");
        Console.WriteLine(declaration.IsImportant ? " !important" : "");
    }
}

// Build a rule from scratch and serialize it
var newRule = new StyleRule(parser);
newRule.SelectorText = "h2";
newRule.Style.BackgroundColor = "green";
Console.WriteLine(newRule.ToCss());   // h2 { background-color: rgb(0, 128, 0) }

Console.WriteLine("--- round trip ---");
Console.WriteLine(sheet.ToCss());
```

Notice two things in the output: the `h1` rule lists `margin-top`, `margin-right`, `margin-bottom` and `margin-left`, because the shorthand is expanded into longhands, and `color` comes back as the normalized `rgb(255, 0, 0)`.

There is nothing to register at start-up; the entry point is `new StylesheetParser()`.

## Key concepts

### The parser and its tolerance options

`StylesheetParser` is where all parsing begins. Every constructor parameter is optional and defaults to `false`.

```csharp
var parser = new StylesheetParser(
    bool includeUnknownRules        = false,
    bool includeUnknownDeclarations = false,
    bool tolerateInvalidSelectors   = false,
    bool tolerateInvalidValues      = false,
    bool tolerateInvalidConstraints = false,
    bool preserveComments           = false,
    bool preserveDuplicateProperties = false);
```

| Option | What it changes |
| --- | --- |
| `includeUnknownRules` | Keeps at-rules the library has no model for, as a rule with `Type == RuleType.Unknown` |
| `includeUnknownDeclarations` | Keeps unrecognized property names, and turns off strict mode on every `StyleDeclaration` the parser creates |
| `tolerateInvalidSelectors` | Accepts non-standard pseudo-elements, and makes `ParseSelector` return an `UnknownSelector` rather than `null` |
| `tolerateInvalidValues` | Keeps a declaration whose value cannot be converted, with its raw text, and keeps a style rule whose selector failed to parse |
| `tolerateInvalidConstraints` | Accepts media-feature constraints the library cannot validate |
| `preserveComments` | Retains comment nodes in the tree (the comment node type is internal, so the text is not readable from consumer code) |
| `preserveDuplicateProperties` | Keeps every occurrence of a repeated property instead of only the winning one |

The parsing methods:

```csharp
Stylesheet        Parse(string content)
Stylesheet        Parse(Stream content)
Task<Stylesheet>  ParseAsync(string content)
Task<Stylesheet>  ParseAsync(string content, CancellationToken cancelToken)
Task<Stylesheet>  ParseAsync(Stream content)
Task<Stylesheet>  ParseAsync(Stream content, CancellationToken cancelToken)
```

A parser holds no per-parse state, so one instance can serve the whole process and can be shared across threads for read-only parsing work.

### The stylesheet tree

Every node implements `IStylesheetNode`, and most concrete nodes derive from the abstract `StylesheetNode`.

```csharp
public interface IStyleFormattable
{
    void ToCss(TextWriter writer, IStyleFormatter formatter);
}

public interface IStylesheetNode : IStyleFormattable
{
    IEnumerable<IStylesheetNode> Children { get; }
    StylesheetText StylesheetText { get; }   // source text + range
}

public abstract class StylesheetNode : IStylesheetNode
{
    public StylesheetText StylesheetText { get; }   // internal setter
    public IEnumerable<IStylesheetNode> Children { get; }
    public abstract void ToCss(TextWriter writer, IStyleFormatter formatter);
    public void AppendChild(IStylesheetNode child);
    public void ReplaceChild(IStylesheetNode oldChild, IStylesheetNode newChild);
    public void InsertBefore(IStylesheetNode referenceChild, IStylesheetNode child);
    public void InsertChild(int index, IStylesheetNode child);
    public void RemoveChild(IStylesheetNode child);
    public void Clear();
}
```

`Children` is the general-purpose way to walk the tree, and it is how you reach every rule kind that `Stylesheet` does not expose as a typed collection. `Stylesheet` itself offers `CharacterSetRules`, `FontfaceSetRules`, `MediaRules`, `ContainerRules`, `ImportRules`, `NamespaceRules`, `PageRules` and `StyleRules`, plus `Add(RuleType)`, `RemoveAt(int)`, `Insert(string ruleText, int index)` and `ToCss`. There is no typed collection for `@keyframes`, `@supports`, `@document` or `@viewport`, and no public all-rules-in-order collection - use `Children`. The `Stylesheet` constructor is internal, so to start from an empty sheet, parse an empty string: `var sheet = parser.Parse(string.Empty);`.

### Rules

`IRule` carries `RuleType Type`, `string Text { get; set; }` (the setter re-parses and can throw `ParseException`), `IRule Parent` and `Stylesheet Owner`. `RuleType` is a public byte enum: `Unknown, Style, Charset, Import, Media, FontFace, Page, Keyframes, Keyframe, MarginBox, Namespace, CounterStyle, Supports, Document, FontFeatureValues, Viewport, RegionStyle, Container`.

Each rule kind has a public contract and a way to reach it:

```text
style rule      IStyleRule       stylesheet.StyleRules
@charset        ICharsetRule     stylesheet.CharacterSetRules
@import         IImportRule      stylesheet.ImportRules
@namespace      INamespaceRule   stylesheet.NamespaceRules
@media          IMediaRule       stylesheet.MediaRules
@container      IContainerRule   stylesheet.ContainerRules
@font-face      IFontFaceRule    stylesheet.FontfaceSetRules
@page           IPageRule        stylesheet.PageRules
@keyframes      IKeyframesRule   Children.OfType<IKeyframesRule>()
keyframe        IKeyframeRule    keyframesRule.Rules
@supports       ISupportsRule    Children.OfType<ISupportsRule>()
@document       IRule + IDocumentFunction children (no dedicated interface)
@viewport       IProperties      cast an IRule with Type == Viewport
margin box      MarginStyleRule  pageRule.Children.OfType<MarginStyleRule>()
```

Only four rule classes are public - `Rule` (the abstract base), `StyleRule`, `MarginStyleRule` and `CharsetRule` - so cast to the interface rather than to a concrete rule class. Grouping and condition contracts stack up as `IRuleCreator`, `IRuleList`, `IGroupingRule` (adds `Rules`, `Insert`, `RemoveAt`) and `IConditionRule` (adds `ConditionText`); `IMediaRule`, `IContainerRule` and `ISupportsRule` all derive from `IConditionRule`.

### Declarations and properties

`StyleDeclaration` is the content between `{` and `}`.

```csharp
public sealed class StyleDeclaration : StylesheetNode, IProperties
{
    public event Action<string> Changed;   // fires with the new CssText

    public string CssText { get; set; }    // set => re-parse the block
    public IEnumerable<Property> Declarations { get; }
    public int    Length { get; }
    public IRule  Parent { get; }
    public bool   IsStrictMode { get; }    // == !includeUnknownDeclarations
    public string this[int index]  { get; }   // property NAME at index
    public string this[string name] { get; }  // property VALUE by name

    public void   Update(string value);       // replace all declarations
    public void   SetProperty(string propertyName, string propertyValue,
                              string priority = null);
    public void   SetPropertyValue(string propertyName, string propertyValue);
    public void   SetPropertyPriority(string propertyName, string priority);
    public string GetPropertyValue(string propertyName);
    public string GetPropertyPriority(string propertyName);
    public string RemoveProperty(string propertyName);
    public IEnumerator<IProperty> GetEnumerator();
}
```

`SetProperty` fails silently, `SetProperty(name, null)` and `SetProperty(name, "")` remove the property, and the priority argument is the bare word `"important"` (case-insensitive), never `"!important"`. Setting a shorthand explodes it into longhands: `p { margin: 10px }` produces four declarations named `margin-top`, `margin-right`, `margin-bottom` and `margin-left`, and `Length` is 4. Reading the shorthand back through `style["margin"]` or `style.Margin` re-assembles it from the longhands in strict mode; with `includeUnknownDeclarations: true` that re-assembly does not happen and the shorthand getter returns an empty string.

`StyleDeclaration` exposes named CSS property accessors grouped by area - alignment and box, animation, background, border, break and page, columns, container, content and counters, flexbox, font and text, lists and tables, margin and padding, mask and SVG paint, transform, transition, and legacy or vendor properties. They are all of type `string`, get and set: thin wrappers over `GetPropertyValue`/`SetPropertyValue` with the matching `PropertyNames` constant. `style.Color` is a string, not the `Color` struct; `style.Width` is a string, not a `Length`.

Individual declarations arrive as `Property` / `IProperty`:

```csharp
public abstract class Property : StylesheetNode, IProperty
{
    public string Name        { get; }
    public string Value       { get; }   // normalized value, or "initial"
    public string Original    { get; }   // raw source text of the value
    public bool   IsImportant { get; set; }
    public string CssText     { get; }   // "name: value" (+ " !important")
    public bool   IsInherited { get; }
    public bool   IsInitial   { get; }
    public bool   IsAnimatable { get; }
    public bool   CanBeInherited { get; }
}
```

`Value` is normalized and `Original` is not: parsing `background-color: #5a5eed` gives `Value == "rgb(90, 94, 237)"` and `Original == "#5a5eed"`. `IProperties` is also implemented by `@font-face` and `@viewport` rules, so those can be enumerated or queried by name even though their rule classes are internal.

### Selectors and specificity

`ISelector` exposes exactly two members - `Priority Specificity` and `string Text`. Specificity is a comparable struct:

```csharp
public struct Priority : IEquatable<Priority>, IComparable<Priority>
{
    public Priority(uint priority);
    public Priority(byte inlines, byte ids, byte classes, byte tags);
    public byte Inlines { get; }   // (a) inline style
    public byte Ids     { get; }   // (b) id selectors
    public byte Classes { get; }   // (c) class, attribute, pseudo-class
    public byte Tags    { get; }   // (d) element, pseudo-element
    // static: Zero, OneTag, OneClass, OneId, Inline
    // operators: + == != < > <= >=   (ToString => "(a, b, c, d)")
}
```

What the parser hands back depends on the selector text: `"div"` gives a single simple selector, `"div.foo[bar]"` a `CompoundSelector`, `"div > p"` a `ComplexSelector`, `"h1, h2"` a `ListSelector`, and unparsable input an `UnknownSelector` when tolerated. Simple selectors - `AllSelector`, `TypeSelector`, `ClassSelector`, `IdSelector`, `PseudoClassSelector`, `PseudoElementSelector`, `NamespaceSelector` - are created through a static `Create` factory method because their constructors are private. Attribute selectors have one class per CSS operator (`AttrAvailableSelector`, `AttrMatchSelector`, `AttrListSelector`, `AttrHyphenSelector`, `AttrBeginsSelector`, `AttrEndsSelector`, `AttrContainsSelector`, and the non-standard `AttrNotMatchSelector`), all with a public `(string attribute, string value)` constructor. The `nth-child` family derives from the abstract `ChildSelector`, which carries `Step` (the A in An+B) and `Offset` (the B); note that `FirstChildSelector` models `:nth-child(...)`, while a bare `:first-child` is a `PseudoClassSelector`.

`Combinator` exposes the static instances `Child` (`">"`), `Deep` (`">>>"`), `Descendent` (`" "`), `AdjacentSibling` (`"+"`), `Sibling` (`"~"`), `Namespace` (`"|"`) and `Column` (`"||"`), and the static `Combinators` class holds the raw delimiter strings.

### Media, container and supports queries

`IMediaRule` adds a `MediaList Media` to the condition rule contract.

```csharp
public sealed class MediaList : StylesheetNode
{
    public string MediaText { get; set; }       // whole query list
    public IEnumerable<Medium> Media { get; }
    public int    Length { get; }
    public string this[int index] { get; }      // medium as CSS text
    public void Add(string newMedium);          // throws on bad input
    public void Remove(string oldMedium);       // throws if not found
    public IEnumerator<Medium> GetEnumerator();
}
```

Each `Medium` exposes `Type` (`"screen"`, `"print"`, `"all"`), `IsExclusive` (`only screen`), `IsInverse` (`not screen`), `Constraints` and `Features`; each `IMediaFeature` exposes `Name`, `Value` and `HasValue`, and `MediaFeature` adds `IsMinimum` and `IsMaximum`. Use the `FeatureNames` constants for the recognized names.

`ISupportsRule` adds an `IConditionFunction Condition` whose single member is `bool Check()`. `Check()` reports whether *this library* can parse the declarations in the condition; it is not a browser-support query. `IContainerRule` adds an optional `Name` and a `MediaList Media` holding the size query. `@document` has no dedicated interface - find it by rule type and read its conditions from the children:

```csharp
var docRules = stylesheet.Children.OfType<IRule>()
                         .Where(r => r.Type == RuleType.Document);
foreach (var rule in docRules)
    foreach (var fn in rule.Children.OfType<IDocumentFunction>())
        Console.WriteLine($"{fn.Name}({fn.Data})");
```

### The typed value vocabulary

The parsed model never hands you a typed value: declaration values are strings, and every class that would carry a typed value is internal. The value types are a vocabulary you construct yourself, plus static parse helpers you point at the strings the declaration API gives you.

```csharp
// Typical flow
var raw = rule.Style.Width;                    // "50%"  (a string)
if (Length.TryParse(raw, out var width))       // typed
    Console.WriteLine($"{width.Value} {width.UnitString}");
```

`Length` carries `Value`, `Type`, `UnitString`, `IsAbsolute`, `IsRelative`, `ToPixel()`, `To(Unit)` and `TryParse`, with the units `None, Px, Em, Ex, Cm, Mm, In, Pt, Pc, Ch, Rem, Vw, Vh, Vmin, Vmax, Percent`. `Angle`, `Time`, `Frequency`, `Resolution`, `Number` and `Percent` follow the same shape; all implement `IEquatable<T>`, `IComparable<T>` and `IFormattable`, and all define the comparison operators. `Color` offers `FromRgb`, `FromRgba`, `FromGray`, `FromHex`, `TryFromHex`, `FromFlexHex`, `FromHsl`, `FromHsla`, `FromHwb`, `FromHwba`, `FromName` and `Mix`, and the `Colors` static class maps names to colors and back. Composite types you can build by hand include `Point`, `Shadow`, `Shape`, `Counter`, `GradientStop`, `LinearGradient`, `RadialGradient`, `CubicBezierTimingFunction`, `StepsTimingFunction`, `TransformMatrix` and `Url`.

The library also ships a large set of public byte-backed CSS enums - layout and box, flexbox and alignment, fonts and text, paint and borders, backgrounds, lists and animation, interaction and media-feature vocabulary. Like the value types, no public member returns one; they are there for your own modeling and for mapping the strings you read out of declarations.

### Serialization

Every node implements `IStyleFormattable`, so anything in the tree can be written out. `FormatExtensions` adds `ToCss()`, `ToCss(IStyleFormatter)` and `ToCss(TextWriter)`, with the parameterless overloads using `CompressedStyleFormatter.Instance`. That formatter implements every `IStyleFormatter` member explicitly, so hold it as `IStyleFormatter`; despite the name its output is readable rather than minified, with rules joined by `Environment.NewLine` and a block written as `selector { a: b; c: d }`. Implement `IStyleFormatter` - its members are `Sheet`, `Block`, `Declaration`, `Declarations`, `Medium`, `Constraint`, `Rule`, `Style` and `Comment` - for minified or pretty output.

Use the string constants rather than literals: `PropertyNames`, `RuleNames`, `FunctionNames`, `PseudoClassNames`, `PseudoElementNames`, `FeatureNames`, `UnitNames`, `Combinators`, `Colors` and `ProtocolNames`. There is no public `Keywords` class - the CSS keyword constants are internal, so write those literals yourself.

### Errors and source positions

Parsing is forgiving by design: malformed rules, selectors and declarations are dropped rather than reported, and there is no public error-callback or error-collection API. Detect problems by comparing what you parsed with what you expected - rule counts, null selectors, empty property values.

`ParseException` is thrown from the mutation and re-parse paths instead: the `IRule.Text` setter, `Stylesheet.Insert`/`RemoveAt`, `IGroupingRule.Insert`/`RemoveAt`, the `MediaList.MediaText` setter with `Add` and `Remove`, the `IKeyframeRule.KeyText` setter, and a handful of parse-time conditions such as an unterminated media prelude or an unparsable `@supports` condition. Guard every mutation call with `try`/`catch (ParseException)`.

Positions live on nodes rather than on errors. `StylesheetText` carries a `TextRange Range` and the raw source slice; `TextRange` pairs two `TextPosition` values, each with a 1-based `Line` and `Column` and an absolute `Position`. Nodes you built by hand, and selectors from a standalone `ParseSelector` call, have a null `StylesheetText`, so null-check it. `ParseError` and `TokenizerError` are the tokenizer's error vocabulary, but the class that raises them is internal, so consumer code cannot subscribe.

## Examples

Parse a stylesheet and walk its style rules and media rules:

```csharp
using System;
using System.Linq;
using CodeBrix.StyleSheetParse;

var parser = new StylesheetParser();
var stylesheet = parser.Parse(@"
    h1 { color: red; font-size: 24px; }
    .highlight { background-color: yellow; }
    @media screen and (max-width: 768px) {
        h1 { font-size: 18px; }
    }
");

// Access style rules
foreach (var rule in stylesheet.StyleRules)
{
    Console.WriteLine($"Selector: {rule.SelectorText}");
    foreach (var property in rule.Style.Declarations)
    {
        Console.WriteLine($"  {property.Name}: {property.Value}");
    }
}

// Access media rules
foreach (var media in stylesheet.MediaRules)
{
    Console.WriteLine($"Media: {media.ConditionText}");
}
```

Modify properties - including a priority - and serialize the result back to CSS:

```csharp
using System;
using System.Linq;
using CodeBrix.StyleSheetParse;

var parser = new StylesheetParser();
var stylesheet = parser.Parse("h1 { color: red; } p { margin: 10px; }");

// Modify a property
var firstRule = stylesheet.StyleRules.First();
firstRule.Style.SetProperty("color", "blue");
firstRule.Style.SetProperty("font-weight", "bold");

// Add a new property with !important ("important", never "!important")
firstRule.Style.SetProperty("text-align", "center", "important");

// SetProperty is silent on failure - verify when the input is untrusted
firstRule.Style.SetProperty("color", "not-a-color");
Console.WriteLine(firstRule.Style.Color);   // still "rgb(0, 0, 255)"

// Serialize back to CSS
string css = stylesheet.ToCss();
Console.WriteLine(css);
```

Parse selectors on their own and compare their specificity with the overloaded operators:

```csharp
using System;
using CodeBrix.StyleSheetParse;

var parser = new StylesheetParser();

var selectors = new[]
{
    "h1",
    ".class",
    "#id",
    "div.class > span#id",
    "ul li a:hover"
};

foreach (var selectorText in selectors)
{
    var selector = parser.ParseSelector(selectorText);
    if (selector == null)
    {
        Console.WriteLine($"{selectorText} => invalid");
        continue;
    }

    var s = selector.Specificity;
    Console.WriteLine($"{selectorText} => ({s.Inlines},{s.Ids},{s.Classes},{s.Tags})");
}

// Compare two selectors with the overloaded operators
var a = parser.ParseSelector("#main .row");
var b = parser.ParseSelector("div.row span");
Console.WriteLine(a.Specificity > b.Specificity);   // True
```

Walk every rule in document order, descending into `@keyframes`:

```csharp
using System;
using System.Linq;
using CodeBrix.StyleSheetParse;

var parser = new StylesheetParser();
var stylesheet = parser.Parse(@"
    @keyframes fadeIn {
        from { opacity: 0; }
        to { opacity: 1; }
    }
    .animated { animation: fadeIn 1s ease-in; }
");

// Stylesheet.Rules is internal - Children is the public way to walk
// every rule in document order.
foreach (IRule rule in stylesheet.Children.OfType<IRule>())
{
    if (rule is IKeyframesRule keyframes)
    {
        Console.WriteLine($"Animation: {keyframes.Name}");
        foreach (IRule kfRule in keyframes.Rules)
        {
            if (kfRule is IKeyframeRule keyframe)
            {
                Console.WriteLine($"  {keyframe.KeyText}: {keyframe.Style.CssText}");
            }
        }
    }
    else
    {
        Console.WriteLine($"{rule.Type}: {rule.Text}");
    }
}
```

## Using it in a CodeBrix.Platform application

Add the package to your `.Core` library and use it from your services and view models. Nothing is registered at start-up, there is no UI surface and no native library, so the behavior is identical on every head.

One `StylesheetParser` instance can serve the whole process, including across threads for read-only parsing. Cache the results of the `Stylesheet` collections and of `Style.Declarations` before you enumerate them twice: they are LINQ queries over `Children` and are re-evaluated on every enumeration. For large stylesheets, parse from a `Stream` with `ParseAsync` and a `CancellationToken`, and write output through a `TextWriter` rather than building strings in memory.

## Pitfalls

- The package ID is `CodeBrix.StyleSheetParse.MitLicenseForever`; the namespace is `CodeBrix.StyleSheetParse`.
- `Parse()` returns a `Stylesheet`, not a list of rules. `Stylesheet.Rules` and the `Stylesheet` constructor are internal - to create an empty sheet, parse an empty string.
- Invalid CSS does not throw. By default invalid rules, selectors and values are silently dropped, and there is no public error event.
- `Property.Value` is normalized and `Property.Original` is the raw source text. Read whichever one your task needs.
- The named style accessors are strings, not typed values. Use `Length.TryParse`, `Color.TryFromHex` and `Colors.GetColor` yourself.
- A shorthand such as `margin: 10px` is stored as four longhand declarations. Only the read-back accessors re-assemble it, and only in strict mode.
- Pass `"important"` (case-insensitive) to `SetProperty` or `SetPropertyPriority`, never `"!important"`, and `null` for normal priority.
- `SetProperty` returns `void` and does nothing when the value fails to parse, when the name is unknown in strict mode, or when the priority is neither `null` nor `"important"`. Read the value back if it matters.
- `style.Clear` is the CSS `clear` property, and it hides `StylesheetNode.Clear()`. To empty a block use `style.CssText = string.Empty`.
- `ParseSelector` returns `null` for invalid input unless the parser was created with `tolerateInvalidSelectors: true`, in which case you get an `UnknownSelector`.
- The two selector tolerance options are different: inside a stylesheet, a style rule whose selector fails to parse is dropped unless `tolerateInvalidValues` is true, while `tolerateInvalidSelectors` governs `ParseSelector` and non-standard pseudo-elements.
- `AttributeSelectorFactory`, `PseudoClassSelectorFactory` and `PseudoElementSelectorFactory` are public classes whose constructors and instance accessors are internal. Use the static `Create` methods on the selector classes instead.
- `Color.FromHex` and `Color.TryFromHex` expect 3, 4, 6 or 8 bare hex digits with no leading `#`, and `FromHex` does not validate - use `TryFromHex` on untrusted input.
- `Length.ToPixel()` and `Length.To()` throw `InvalidOperationException` for relative units. Check `IsAbsolute` first.
- `Time` has `GetUnit` but no `TryParse`; parse the number yourself.
- `preserveComments` keeps comment nodes in the tree, but the comment node class is internal, so the text is not reachable from consumer code.
- Compare `Priority` values directly with `<`, `>` and `==`; do not compare the `ToString()` output.
- `IsImportant` lives on `Property`/`IProperty`, separate from the value.
- `MarginStyleRule` implements `IStyleRule`, its `Type` is `RuleType.Style`, and its `SelectorText` getter prefixes the text with `"@"`. `IMarginRule` is declared but nothing in the library implements it.
- `RadialGradient` implements `IImageSource` but not `IGradient`.
- Do not copy test code from the repository verbatim: the test project has `InternalsVisibleTo` access and calls internal members. Translate those to the public equivalents.

## Samples and tools in the repository

This repository contains no sample applications, demo projects, benchmarks or build tools. It has exactly two projects: the library that becomes the package, and its test project, which doubles as the largest body of working usage examples for the library.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Test project | The tokenizer, the rule and declaration model, the selector model, media queries, at-rules, CSS value parsing and serialization | [`tests/CodeBrix.StyleSheetParse.Tests`](https://github.com/ellisnet/CodeBrix.StyleSheetParse/tree/main/tests/CodeBrix.StyleSheetParse.Tests) |
| `bootstrap.css` | A full real-world stylesheet, embedded in the test assembly and asserted against by the selector and real-world suites | [`tests/CodeBrix.StyleSheetParse.Tests`](https://github.com/ellisnet/CodeBrix.StyleSheetParse/tree/main/tests/CodeBrix.StyleSheetParse.Tests) |

Run the suite with `dotnet test CodeBrix.StyleSheetParse.slnx`.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.StyleSheetParse/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.StyleSheetParse/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.StyleSheetParse/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.StyleSheetParse.Tests](https://github.com/ellisnet/CodeBrix.StyleSheetParse/tree/main/tests/CodeBrix.StyleSheetParse.Tests) |

## License

CodeBrix.StyleSheetParse is licensed under the MIT License; the license is also named in the package ID (`CodeBrix.StyleSheetParse.MitLicenseForever`). For the provenance and licensing of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.StyleSheetParse/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.MarkupParse](CodeBrix.MarkupParse.md) - parse the HTML that these stylesheets style
- [CodeBrix.SvgParse](CodeBrix.SvgParse.md) - the SVG library that builds its CSS styling on this package
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.StyleSheetParse on GitHub](https://github.com/ellisnet/CodeBrix.StyleSheetParse) - source, tests and samples
