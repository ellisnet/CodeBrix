<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Templating</sub>

# CodeBrix.Templating

**CodeBrix.Templating is a fully managed text-templating and scripting-language library: it parses
templates, binds them to a .NET object model, and renders them to text.** Templates are written in the
Scriban template language or in the Liquid template language, and the output is whatever you need
produced from a model - code generation, HTML pages, e-mail bodies, reports, configuration files, SQL.
It has no NuGet dependencies and no native code at all, so you use it from any .NET 10 application or
from a CodeBrix.Platform application.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Templating](https://github.com/ellisnet/CodeBrix.Templating) |
| **Packages** | [`CodeBrix.Templating.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.Templating.BsdLicenseForever) |
| **License** | BSD 2-Clause; see [License](#license) |
| **Requires** | .NET 10 or later. No NuGet dependencies, no native libraries, no operating-system setup |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Every platform .NET 10 supports, including trimmed and NativeAOT builds |

## What it does

- Renders a template against a model with one call - `Template.Parse(text).Render(model)` - or against a
  `TemplateContext` you control, synchronously or asynchronously.
- Gives you a template language with expressions, statements (`if` / `case` / `for` / `while`),
  user-defined functions, pipes, string interpolation and whitespace control.
- Ships a library of built-in functions covering arrays, strings, math, dates, timespans, objects,
  regular expressions and HTML.
- Accepts the Liquid template language as a compatible subset, with its own tag syntax, its filter
  spelling and its `forloop` object.
- Evaluates safely: loop, recursion, object-recursion, string-length and regex-timeout limits, a
  cancellation token, and an opt-in strict-variable mode.
- Binds models three ways - reflection over a POCO, a dictionary, or an `IScriptObject` you implement -
  with a member renamer and a member filter deciding what a template can see.
- Bridges to JSON over `System.Text.Json`: `object.from_json`, `object.to_json`, and importing a
  `JsonElement` straight into a `ScriptObject`.
- Loads partial templates for the `include` function through an `ITemplateLoader` you supply, and caches
  them for the lifetime of a context.
- Exposes the full public AST with visitors, a rewriter, a formatter and a printer, so templates can be
  analyzed, transformed and written back to text.
- Runs under trimming and NativeAOT: evaluation is a tree-walking interpreter, so no runtime code
  generation is required for the core engine.

## When to use it

Reach for CodeBrix.Templating whenever a program has to produce text from a model and you want the shape
of that text to live outside the code - in a file an author edits, a database row, or a string a user
supplies. It suits code generators, document and mail merges, report and configuration emitters, and any
place a Liquid template already exists and needs a .NET host.

What it deliberately does not do:

- It does not compile templates to IL or emit code at runtime; evaluation is a tree-walking interpreter.
  That is what makes it trim- and AOT-safe.
- It does not sandbox .NET. Anything you import into the globals is callable from the template; the
  engine limits loops, recursion and time, not access.
- It does not sanitize HTML, escape output automatically, or provide contextual auto-escaping. Call
  `html.escape` or `html.url_encode` yourself.
- It does not read templates from disk on its own. `include` needs an `ITemplateLoader` you provide;
  there is no built-in file-system loader.
- It does not implement every Liquid tag or filter from every Liquid dialect, and it does not implement
  vendor-specific dialect extensions. The Liquid support is the compatible subset.
- It does not localize or format for a culture unless you push one; the default culture is invariant.
- It does not provide a designer, a language server, or MSBuild and source-generator integration. It is
  a runtime library.
- It does not render Markdown, PDF, images, or any binary format. It produces text.

For turning Markdown or HTML into a PDF, that is
[CodeBrix.PdfDocuments](CodeBrix.PdfDocuments.md). For writing spreadsheets, see
[FreePPlus](FreePPlus.md).

## Getting started

```bash
dotnet add package CodeBrix.Templating.BsdLicenseForever
```

The package ID is `CodeBrix.Templating.BsdLicenseForever`; the assembly and the primary namespace are
both `CodeBrix.Templating`. There is no package named plain `CodeBrix.Templating`.

```csharp
using CodeBrix.Templating;
    // Template, TemplateContext, LiquidTemplateContext, LogMessageBag,
    // ScriptPrinter, ScriptPrinterOptions

using CodeBrix.Templating.Runtime;
    // ScriptObject, IScriptObject, ScriptObjectExtensions (the Import
    // extension methods), ScriptArray, ScriptRange, ScriptLazy,
    // EmptyScriptObject, ITemplateLoader, IScriptOutput,
    // StringBuilderOutput, TextWriterOutput, IScriptCustomFunction,
    // DelegateCustomFunction, DelegateCustomAction, DynamicCustomFunction,
    // MemberRenamerDelegate, MemberFilterDelegate, StandardMemberRenamer,
    // ScriptMemberIgnoreAttribute, ScriptMemberImportFlags,
    // IObjectAccessor, IListAccessor and the built-in accessors

using CodeBrix.Templating.Parsing;
    // ParserOptions, LexerOptions, ScriptLang, ScriptMode, Lexer, Parser,
    // Token, TokenType, SourceSpan, TextPosition, LogMessage,
    // ParserMessageType

using CodeBrix.Templating.Syntax;
    // ScriptNode and every AST node type, ScriptVisitor, ScriptRewriter,
    // ScriptFormatter, ScriptFormatterOptions, ScriptFormatterFlags,
    // ScriptRuntimeException, ScriptParserRuntimeException,
    // ScriptAbortException, ScriptArgumentException

using CodeBrix.Templating.Functions;
    // BuiltinFunctions, ArrayFunctions, StringFunctions, MathFunctions,
    // DateTimeFunctions, TimeSpanFunctions, ObjectFunctions,
    // HtmlFunctions, RegexFunctions, IncludeFunction,
    // IncludeJoinFunction, LiquidBuiltinsFunctions
```

Most consuming code needs only the first two. No registration call is required to render; the two things
an application sets up itself are a `TemplateContext.TemplateLoader` when templates use `include`, and
whatever globals the template reads.

Here is a complete program: parse, check for errors, push globals, render.

```csharp
using System;
using CodeBrix.Templating;
using CodeBrix.Templating.Runtime;

var template = Template.Parse("Hello {{ name }}, {{ items | array.size }} items.");
if (template.HasErrors)
{
    Console.Error.WriteLine(template.Messages.ToString());
    return 1;
}

var globals = new ScriptObject();
globals["name"] = "World";
globals["items"] = new[] { 1, 2, 3 };

var context = new TemplateContext();
context.PushGlobal(globals);

Console.WriteLine(template.Render(context));
return 0;
```

Notice that parsing never throws for a syntax error: `Template.Parse` returns a template with
`HasErrors` set, and it is rendering such a template that throws.

## Key concepts

### Template - the entry point

A parsed `Template` is immutable and safe to cache and reuse across renders. Parsing is by far the
expensive step, so parse once and render many times.

```csharp
static Template Parse(string text,
                      string sourceFilePath = null,
                      ParserOptions parserOptions = null,
                      LexerOptions lexerOptions = null)

static Template ParseLiquid(string text,
                            string sourceFilePath = null,
                            ParserOptions parserOptions = null,
                            LexerOptions lexerOptions = null)
```

Instance members are `SourceFilePath`, `Page` (the parsed AST root), `HasErrors`, `Messages`,
`ParserOptions`, `LexerOptions`, the `Render` and `RenderAsync` pairs, the four `Evaluate` and
`EvaluateAsync` forms, and `ToText(ScriptPrinterOptions options = default)`, which prints the AST back
to source. `Evaluate` returns the value of the last expression instead of the rendered text; `Render`
writes to the context output and returns the accumulated string.

> [!IMPORTANT]
> On `Render`, `RenderAsync`, `Evaluate` and `EvaluateAsync` the member **renamer** comes before the
> member **filter**. On `ScriptObjectExtensions.Import` it is the other way round. Use named arguments
> on both.

The overloads that take an `object model` import it by reflection and are annotated
`[RequiresUnreferencedCode]`; in trimmed or NativeAOT builds use the `TemplateContext` overloads with an
explicitly populated `ScriptObject`.

### TemplateContext - evaluation state

`TemplateContext` owns the output sink, the global and local variable stacks, the culture stack, the
loaded-template cache and every execution limit. Reuse one context across renders when the globals do
not change. Variables move through `PushGlobal` / `PopGlobal`, `PushLocal` / `PopLocal`, `SetValue`,
`GetValue`, `DeleteValue` and `SetReadOnly`; `LiquidTemplateContext` is the derived context that
installs the Liquid builtins.

`Output` and `CurrentCulture` are read-only properties. Push and pop them instead: `PushOutput(new
TextWriterOutput(writer))` to render into a `TextWriter`, `PushCulture(CultureInfo)` to leave the
invariant default. A fresh `TemplateContext` starts with a `StringBuilderOutput`.

### Safety limits

```csharp
int      LoopLimit            { get; set; }   // 1000 iterations per loop
int?     LoopLimitQueryable   { get; set; }   // null = use LoopLimit
int      RecursiveLimit       { get; set; }   // 100 nested evaluations
int      ObjectRecursionLimit { get; set; }   // 20
int      LimitToString        { get; set; }   // 1048576 characters
TimeSpan RegexTimeOut         { get; set; }   // 10 seconds
CancellationToken CancellationToken { get; set; }
void     CheckAbort()                         // throws ScriptAbortException
```

Exceeding any of the four limits raises `ScriptRuntimeException`; setting the cancellation token makes
the engine raise `ScriptAbortException` at the next `CheckAbort` point. Alongside them sit the behavior
switches - `StrictVariables` (false), `EnableRelaxedMemberAccess` (true), `EnableRelaxedTargetAccess`
(false), `EnableRelaxedFunctionAccess` (false), `EnableRelaxedIndexerAccess` (true), `EnableNullIndexer`
(false), `EnableBreakAndContinueAsReturnOutsideLoop` (false), `AutoIndent` (true) and
`IndentOnEmptyLines` (true).

### ScriptObject - the model container

`ScriptObject` is an ordered, case-sensitive string/object dictionary implementing `IScriptObject`, and
it is the canonical way to hand values to a template without reflection. A class deriving from
`ScriptObject` automatically imports its own public static members as template functions - that is
exactly how the built-in function tables are built - and `autoImportStaticsFromThisType: false`
suppresses it. Implement `IScriptObject` directly for a fully custom model object.

### Importing .NET objects

The `Import` methods are extension methods on `IScriptObject` declared in `ScriptObjectExtensions`, so
`using CodeBrix.Templating.Runtime;` is required for them to resolve.

```csharp
static void Import(this IScriptObject script, object obj,
                   MemberFilterDelegate filter = null,
                   MemberRenamerDelegate renamer = null)

static void Import(this IScriptObject script, object obj,
                   ScriptMemberImportFlags flags,
                   MemberFilterDelegate filter = null,
                   MemberRenamerDelegate renamer = null)

static void Import(this IScriptObject script, string member,
                   Delegate function)

static void Import(this IScriptObject @this, IScriptObject other)

static void Import(this IScriptObject script, JsonElement json)

static void ImportMember(this IScriptObject script, object obj,
                         string memberName, string exportName = null)
```

Pass a `Type` as `obj` to import that type's static members; pass an instance to import its instance
fields and properties. `ScriptMemberImportFlags` is `Field | Property | Method | All`.
`StandardMemberRenamer` turns PascalCase into snake_case - `ThisIsAnExample` becomes
`this_is_an_example` - and it is the default renamer for both `TemplateContext.MemberRenamer` and
`Template.Render`; pass `member => member.Name` to keep .NET names as-is. `[ScriptMemberIgnore]` on a
field, property or method keeps `Import` from exposing it at all.

### Template syntax

Everything outside `{{ ... }}` is raw text copied to the output verbatim. Inside, the content is a
sequence of script statements separated by `;` or a newline, and the value of a statement that is a bare
expression is written to the output. Escape blocks emit literal braces with one `%` per nesting level -
`{%{ this {{ is }} not interpreted }%}`. Comments are `{{ # single line }}` and `{{ ## multi line ## }}`.
Whitespace control comes in two strengths: `{{- ... -}}` is greedy and removes all whitespace including
newlines, while `{{~ ... ~}}` is non-greedy. With `AutoIndent` on, the indentation in front of an
interpolation is re-applied to every line of a multi-line value, including text produced by `include`.

```text
arithmetic   +  -  *  /  //  %      (`//` divides and floors)
string       +  (concatenation)   *  (repeat: "a" * 2)
comparison   ==  !=  <  >  <=  >=   (also compares strings)
logical      &&  ||  !
bitwise      &  |  ^  <<  >>        (on arrays: set/append operations)
coalescing   a ?? b                 b when a is null
             a ?! b                 b when a is NOT null, else null
ternary      cond ? then : else
ranges       1..5                   inclusive  -> [1, 2, 3, 4, 5]
             1..<5                  exclusive  -> [1, 2, 3, 4]
function     @name                  reference a function without calling it
             ^array                 expand an array as call arguments
```

Variables are `name` for a global and `$name` for a local scoped to the enclosing `func` or `include`;
`obj?.member` is null-conditional, and `x.empty?` is true for an empty list, string or script object,
false when it has content, and null when `x` is null. Each block statement closes with `end`, and the
set runs `if` / `else if` / `else`, `case` / `when` / `else`, `for ... [offset: n] [limit: n]
[reversed]` with an optional `else` for an empty sequence, `tablerow`, `while`, `break`, `continue`,
`capture`, `with`, `import`, `readonly`, `func`, `ret` and `wrap`. Inside a `for`, the `for` object
exposes `index`, `rindex`, `first`, `last`, `even`, `odd`, `length` and `changed`.

Functions take arguments separated by spaces rather than parentheses, and the pipe operator feeds the
left value in as the first argument of the right call, so pipelines read left to right. Inside a
function, `$0`, `$1`, `$2` and so on are the positional arguments, `$` is the whole arguments object,
and `$$` is the block passed by a `wrap` statement.

### Built-in functions

A new `TemplateContext` installs `BuiltinFunctions` as its bottom-most global scope, holding the tables
`array`, `string`, `math`, `date`, `timespan`, `object`, `regex` and `html`, the functions `include` and
`include_join`, and the two aliases `empty` and `blank`.

| Table | What it covers |
| --- | --- |
| `array` | add, add_range, any, compact, concat, contains, cycle, each, filter, first, insert_at, join, last, limit, map, offset, remove_at, reverse, size, sort, uniq |
| `string` | append, base64_decode/encode, capitalize, capitalizewords, contains, downcase, empty, ends_with, equals_ignore_case, escape, handleize, hmac_sha1/256/512, index_of, literal, lstrip, md5, pad_left, pad_right, pluralize, prepend, remove, remove_first, remove_last, replace, replace_first, rstrip, sha1/256/512, size, slice, slice1, split, starts_with, strip, strip_newlines, to_int, to_long, to_float, to_double, truncate, truncatewords, upcase, whitespace |
| `math` | abs, ceil, divided_by, floor, format, is_number, minus, modulo, plus, power-free arithmetic helpers, product, random, round, times, uuid |
| `date` | now, utc_now, the add_* family, parse, parse_to_string, to_string, `date.format` and the read-only `date.default_format` |
| `timespan` | zero, from_days, from_hours, from_minutes, from_seconds, from_milliseconds, parse |
| `object` | default, eval, eval_template, format, from_json, to_json, has_key, has_value, keys, values, size, typeof, kind |
| `regex` | escape, unescape, match, matches, replace, split |
| `html` | escape, strip, newline_to_br, url_encode, url_escape |

Date patterns are strftime-style; formatting uses the current culture, and prefixing the pattern with
`%g` forces the invariant culture. A date value is a .NET `DateTime`, so its properties are reachable
through the member renamer: `{{ date.now.year }}`, `{{ date.now.day_of_week }}`. Regex `options` is a
string of flag characters - `i` case-insensitive, `m` multiline, `s` single-line, `x` ignore pattern
whitespace - and every regex call is bounded by `TemplateContext.RegexTimeOut`.

### include and include_join

`{{ include 'header' }}` renders a partial; `{{ include 'arguments' 1 2 }}` passes positional values and
`{{ include 'partial' name: 'value' }}` passes named ones. Positional arguments inside an included
template start at `$1`, because `$0` is the template name - inside a `func` the first argument *is*
`$0`. `include` resolves the name through `TemplateContext.TemplateLoader` and throws at render time
without one; loaded templates are cached in `TemplateContext.CachedTemplates` for the lifetime of the
context. `include_join` takes a list of template names, a separator, and optional start and end
components, where a component prefixed with `tpl:` is itself rendered as a template name.

### Liquid mode

Parse with `Template.ParseLiquid` and render with a `LiquidTemplateContext`. Statements live in
`{% ... %}` tags and `{{ ... }}` only outputs a value; closing tags are explicit (`endif`, `endunless`,
`endfor`, `endcase`, `endcapture`, `endtablerow`, `endifchanged`); the tag set adds `assign`, `capture`,
`case`/`when`, `cycle`, `decrement`, `increment`, `if`/`elsif`/`else`, `ifchanged`, `include`, `raw`,
`comment`, `tablerow`, `unless`, `for`, `break` and `continue`; filter arguments use a colon and commas,
as in `{{ x | truncate: 5 }}`; the loop-state object is `forloop` (and `tablerowloop`), whose `index` is
1-based, with `index0` and `rindex0` available; and only the `-` whitespace modifier exists.
`LiquidBuiltinsFunctions` installs the Liquid filter names as aliases of the Scriban builtins and
exposes the mapping through `static bool TryLiquidToScriban(string liquidBuiltin, out string target, out
string member)`, while `ParserOptions.LiquidFunctionsToScriban = true` rewrites Liquid filter calls into
their Scriban equivalents at parse time.

### Three ways to add a function

Import a delegate under a name, which is the simplest:
`globals.Import("shout", new Func<string, string>(s => s.ToUpperInvariant()));`. Async delegates work
too - import a `Func<Task<T>>` or `Func<ValueTask<T>>` and await `RenderAsync`.

Import a whole type's public statics as a function table with `globals.Import(typeof(MyFunctions))`, or
derive from `ScriptObject` and let the constructor auto-import the derived type's statics, then
`context.PushGlobal(new MyFunctions())`.

Implement `IScriptCustomFunction` for full control over arity, argument conversion and the block
statement. `DelegateCustomFunction`, `DelegateCustomAction` and `DynamicCustomFunction` are the
ready-made wrappers; `DynamicCustomFunction.Create` picks a pre-generated, allocation-free adapter for
the common built-in signatures and falls back to a reflection-based one otherwise. A custom function may
take a leading `TemplateContext` and/or `SourceSpan` parameter, which the engine supplies and which are
not template arguments, and a trailing `params object[]` makes it variadic.

### Template loading

```csharp
public string GetPath(TemplateContext context, SourceSpan callerSpan,
                      string templateName)
public string Load(TemplateContext context, SourceSpan callerSpan,
                   string templatePath)
public ValueTask<string> LoadAsync(TemplateContext context,
                                   SourceSpan callerSpan,
                                   string templatePath)
```

`GetPath` turns the name written in the template into a canonical path - return null to make `include`
yield nothing - and `Load` / `LoadAsync` return the template text for that path. `RenderAsync` uses
`LoadAsync`, `Render` uses `Load`.

> [!WARNING]
> Guard against directory traversal inside `GetPath` when template names come from untrusted input. The
> engine does not sanitize them.

### Errors and diagnostics

`Template.Parse` and `ParseLiquid` never throw for a syntax error; they return a template with
`HasErrors` set and the details in `Messages`, a `LogMessageBag` of `LogMessage` values carrying a
`Type` (`ParserMessageType.Error` or `Warning`), a `Span` and a `Message`. Rendering a template that
`HasErrors` throws `InvalidOperationException`. At run time the hierarchy is `ScriptRuntimeException`
(with `Span`, `OriginalMessage` and a `Message` of the form `file(line,col) : error : ...`),
`ScriptParserRuntimeException` (adding `ParserMessages` from an included template), `ScriptAbortException`
(adding the `CancellationToken`) and `ScriptArgumentException` (with `ArgumentIndex`, which a custom
function throws to blame one argument). Pass a `sourceFilePath` to `Parse` so error messages and spans
name the file.

### The AST, visitors and the formatter

`template.Page` is a `ScriptPage` holding `FrontMatter` and `Body`, and every node derives from
`ScriptNode` with `Span`, `Parent`, `Children`, `Clone`, `Evaluate`, `EvaluateAsync`, `PrintTo`,
`Accept` and a sealed `ToString()` that prints the node back. Two roots split the tree: `ScriptExpression`
and `ScriptStatement`. Derive from `ScriptVisitor` to inspect a template - find every variable, every
include, every function call - and from `ScriptRewriter` to transform it; a rewriter returns a new tree,
and the base implementation copies structure without changing it. `ScriptFormatter` reprints a tree
under `ScriptFormatterFlags`, whose `Clean` combination is `AddSpaceBetweenOperators | CompressSpaces |
MinimizeParenthesisNesting`, and `template.ToText(options)` round-trips the whole template back to
source. Formatting and faithful re-printing need `LexerOptions.KeepTrivia = true`; without it comments
and spacing are discarded.

### Parse options and front matter

`ParserOptions` carries `ExpressionDepthLimit` (250), `LiquidFunctionsToScriban` and
`ParseFloatAsDecimal`. `LexerOptions` carries `Mode`, `Lang`, `FrontMatterMarker`
(`DefaultFrontMatterMarker` is `"+++"`), `EnableIncludeImplicitString`, `StartPosition`, `KeepTrivia`
and `TryMatchCustomToken`. `ScriptLang` is `Default`, `Liquid` or `Scientific`; `ScriptMode` is
`Default`, `FrontMatterOnly`, `FrontMatterAndContent` or `ScriptOnly`. `ScriptOnly` parses the whole
text as script with no `{{ }}` markers, which is what you want for evaluating a single expression. With
`FrontMatterAndContent`, a template may open with a `+++` delimited script block, available afterwards
as `template.Page.FrontMatter` and evaluable on its own with `context.Evaluate(frontMatter)`.

## Examples

Rendering against a plain .NET model, showing the default snake_case renaming.

```csharp
using System;
using CodeBrix.Templating;

public class Invoice
{
    public string CustomerName { get; set; }
    public decimal TotalDue { get; set; }
}

public static class BasicRender
{
    public static string Run()
    {
        var template = Template.Parse(
            "Dear {{ customer_name }}, you owe {{ total_due }}.");

        if (template.HasErrors)
        {
            throw new InvalidOperationException(template.Messages.ToString());
        }

        var model = new Invoice { CustomerName = "Ada", TotalDue = 42.50m };
        return template.Render(model);
        // "Dear Ada, you owe 42.50."
    }
}
```

A template loader plus a loop, a pipe and `include` - and the reminder that the first value you pass
lands in `$1`.

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBrix.Templating;
using CodeBrix.Templating.Parsing;
using CodeBrix.Templating.Runtime;

public class DictionaryTemplateLoader : ITemplateLoader
{
    private readonly Dictionary<string, string> _templates;
    public DictionaryTemplateLoader(Dictionary<string, string> templates)
        => _templates = templates;

    public string GetPath(TemplateContext context, SourceSpan callerSpan,
                          string templateName) => templateName;

    public string Load(TemplateContext context, SourceSpan callerSpan,
                       string templatePath) => _templates[templatePath];

    public ValueTask<string> LoadAsync(TemplateContext context,
                                       SourceSpan callerSpan,
                                       string templatePath)
        => ValueTask.FromResult(Load(context, callerSpan, templatePath));
}

public static class LoopPipeInclude
{
    public static async Task<string> RunAsync()
    {
        var loader = new DictionaryTemplateLoader(new Dictionary<string, string>
        {
            // $1 is the FIRST argument passed to include ($0 is the
            // template name itself)
            ["row"] = "  * {{ $1 | string.upcase }}\n",
        });

        var context = new TemplateContext { TemplateLoader = loader };
        var globals = new ScriptObject();
        globals["fruits"] = new[] { "apple", "pear", "fig" };
        context.PushGlobal(globals);

        var template = Template.Parse(
            "Fruit ({{ fruits | array.size }}):\n" +
            "{{ for fruit in (fruits | array.sort); include 'row' fruit; end }}");

        return await template.RenderAsync(context);
        // "Fruit (3):\n  * APPLE\n  * FIG\n  * PEAR\n"
    }
}
```

A custom function table and an imported delegate, with one member hidden from templates.

```csharp
using System;
using System.Globalization;
using CodeBrix.Templating;
using CodeBrix.Templating.Runtime;

public static class MoneyFunctions
{
    public static string Money(decimal value, string currency = "USD")
        => string.Format(CultureInfo.InvariantCulture, "{0} {1:0.00}",
                         currency, value);

    [ScriptMemberIgnore]
    public static void InternalHelper() { }
}

public static class CustomFunctionSample
{
    public static string Run()
    {
        var globals = new ScriptObject();
        globals.Import(typeof(MoneyFunctions));     // -> {{ money 3 }}
        globals.Import("now_utc",
                       new Func<DateTime>(() => DateTime.UtcNow));
        globals["amount"] = 12.5m;

        var context = new TemplateContext();
        context.PushGlobal(globals);

        var template = Template.Parse("{{ amount | money 'EUR' }}");
        return template.Render(context);      // "EUR 12.50"
    }
}
```

A Liquid template rendered with the context that carries the Liquid filter aliases.

```csharp
using CodeBrix.Templating;
using CodeBrix.Templating.Runtime;

public static class LiquidSample
{
    public static string Run()
    {
        var template = Template.ParseLiquid(
            "{% assign greeting = 'hello world' %}" +
            "{{ greeting | capitalize | truncate: 8 }}" +
            "{% for i in (1..3) %}[{{ forloop.index }}:{{ i }}]{% endfor %}");
        // "Hello...[1:1][2:2][3:3]"

        var context = new LiquidTemplateContext();
        return template.Render(context);
    }
}
```

Running a template that came from a user: strict variables, tightened limits, a timeout, and every
failure mode handled.

```csharp
using System;
using System.Threading;
using CodeBrix.Templating;
using CodeBrix.Templating.Parsing;
using CodeBrix.Templating.Runtime;
using CodeBrix.Templating.Syntax;

public static class SafeRenderer
{
    public static bool TryRender(string text, object model,
                                 TimeSpan timeout, out string result,
                                 out string error)
    {
        result = null;
        error = null;

        var template = Template.Parse(text, "user-template");
        if (template.HasErrors)
        {
            foreach (var message in template.Messages)
            {
                if (message.Type == ParserMessageType.Error)
                {
                    error = message.ToString();   // file(line,col) : error : ...
                    return false;
                }
            }
        }

        using var cts = new CancellationTokenSource(timeout);
        var context = new TemplateContext
        {
            StrictVariables = true,
            LoopLimit = 500,
            RecursiveLimit = 25,
            CancellationToken = cts.Token,
        };
        var globals = new ScriptObject();
        globals.Import(model);
        context.PushGlobal(globals);

        try
        {
            result = template.Render(context);
            return true;
        }
        catch (ScriptAbortException)
        {
            error = "The template took too long to render.";
        }
        catch (ScriptParserRuntimeException ex)
        {
            error = ex.Message + " " + ex.ParserMessages.ToString();
        }
        catch (ScriptRuntimeException ex)
        {
            error = ex.Message;                   // includes the location
        }
        return false;
    }
}
```

Walking the AST to collect every global variable a template reads - the shape of any static analysis you
want to run over templates before shipping them.

```csharp
public class VariableCollector : ScriptVisitor
{
    public readonly List<string> Names = new List<string>();
    public override void Visit(ScriptNode node)
    {
        if (node is ScriptVariableGlobal v) { Names.Add(v.Name); }
        base.Visit(node);
    }
}

var template = Template.Parse("{{ a }} {{ b.c }}");
var collector = new VariableCollector();
collector.Visit(template.Page);
```

And formatting a parsed template back to tidy source, which needs trivia kept at parse time.

```csharp
var template = Template.Parse("  x   +2  ; ", null, null,
                              new LexerOptions { KeepTrivia = true,
                                                 Mode = ScriptMode.ScriptOnly });
var formatted = template.Page.Format(
    new ScriptFormatterOptions(ScriptFormatterFlags.Clean));
string text = formatted.ToString();         // "x + 2"
```

## Using it in a CodeBrix.Platform application

CodeBrix.Templating is not a CodeBrix.Platform add-in and has no UI surface: it is a runtime library
that produces text. A CodeBrix.Platform application references it from its `.Core` library exactly as
any other .NET project would, and the deployment story is as short as it gets - no NuGet dependencies,
no native libraries, no operating-system restrictions, and it runs on every platform .NET 10 supports.

The engine needs no runtime code generation, so it works under trimming and NativeAOT. Avoid the
reflection-based `Render(object model)`, `Import(object)` and `ScriptObject.From` overloads in those
builds; they are marked `[RequiresUnreferencedCode]`.

## Pitfalls

- Do not pass `Render`'s arguments positionally in the wrong order. The signature is
  `Render(object model, MemberRenamerDelegate memberRenamer, MemberFilterDelegate memberFilter)` -
  renamer first - while `Import` is `(object obj, MemberFilterDelegate filter, MemberRenamerDelegate
  renamer)`. Always use named arguments.
- Do not expect .NET member names in templates. The default renamer converts PascalCase to snake_case,
  so `CustomerName` is `customer_name` and `ToString` is `to_string`.
- Do not call `ScriptObject.Import` without `using CodeBrix.Templating.Runtime;`. The `Import` methods
  are extension methods, not instance methods, and without the `using` they do not resolve.
- Do not assign `TemplateContext.Output` or `TemplateContext.CurrentCulture`; both are read-only. Use
  `PushOutput` / `PopOutput` and `PushCulture` / `PopCulture`. Rendering is culture-invariant unless you
  push a culture, which is usually what you want for machine-readable output and a surprise for
  human-readable output.
- Do not reuse a `TemplateContext` whose output is a `TextWriterOutput` and expect `Render(context)` to
  return only the latest render. `Render` returns `context.Output.ToString()` and only resets the buffer
  when the output is a `StringBuilderOutput`; with a `TextWriterOutput` the returned string keeps
  growing. Read the writer yourself and ignore the return value.
- Do not call `include` without setting `TemplateContext.TemplateLoader` - it throws at render time -
  and sanitize template names inside `GetPath` when they can come from user input.
- Do not read the first `include` argument as `$0`. Inside an included template the arguments array
  starts with the template name, so the first value you passed is `$1`. Inside a `func` the first
  argument is `$0`.
- Do not render a Liquid template with a plain `TemplateContext`.
  `Template.ParseLiquid(...).Render(model)` builds a `LiquidTemplateContext` for you, but
  `Render(context)` uses exactly the context you pass, and a plain one has none of the Liquid filter
  aliases.
- Do not expect every Liquid filter to exist. `escape_once` is present in the Liquid-to-Scriban name map
  but is not registered as a callable builtin, so `{{ x | escape_once }}` fails at run time. Verify
  unusual filters before relying on them.
- Do not iterate a range wider than `LoopLimit`. `{{ for i in 1..100000 }}` raises "Range expression
  exceeds LoopLimit" with the default limit of 1000, before a single item is produced. Raise `LoopLimit`
  explicitly for large loops.
- Do not assume a missing variable is an error. By default a missing global renders as an empty string;
  set `StrictVariables = true` to make it throw. `EnableRelaxedMemberAccess` is true by default too, so
  `a.b.c` on a null `a` yields null rather than failing - turn it off for strict templates.
- Do not ignore `Template.HasErrors`. `Render` and `Evaluate` on a template with parse errors throw
  `InvalidOperationException`, not a `ScriptRuntimeException`.
- Do not format dates with .NET format strings. `date.to_string` uses strftime-style patterns; it is
  `math.format` and `object.format` that take .NET format strings.
- Do not treat `html.strip` as a sanitizer. It is a regex-based tag stripper and can mis-handle
  malformed HTML; use a real HTML parser for security work.
- Do not run untrusted templates without limits. `object.eval` and `object.eval_template` execute
  arbitrary script; `LoopLimit`, `RecursiveLimit`, `ObjectRecursionLimit`, `LimitToString`,
  `RegexTimeOut` and `CancellationToken` are the controls that keep a hostile template from hanging the
  process.
- Do not rely on parsing succeeding for AST work without trivia. Formatting and faithful re-printing
  need `LexerOptions.KeepTrivia = true`.
- Do not assign to `x.empty?`; it is read-only and assigning to it raises a runtime error.

For performance: parse once and render many times, reuse a `TemplateContext` when the globals do not
change, prefer a pre-built `ScriptObject` over reflection, push output straight into your own sink with
`context.PushOutput(new TextWriterOutput(writer))`, and use `RenderAsync` when the model exposes async
delegates or the template loader does I/O. `array.each`, `array.filter` and range expressions are lazy
while `array.sort`, `array.uniq` and `array.reverse` materialize, so order pipelines to filter before
sorting.

## Samples and tools in the repository

The repository ships no sample applications, demo apps, benchmarks or build tools. The test project is
the worked example set, and its fixture folders are the authoritative specification of the template
language: each `<name>.txt` is a template and `<name>.out.txt` is exactly what it must render to, byte
for byte.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Test project | Every API, offline, with no environment variables or external services | [`tests/CodeBrix.Templating.Tests`](https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests/CodeBrix.Templating.Tests) |
| Template fixtures | Template / expected-output pairs by feature area, from literals to the AST | [`tests/CodeBrix.Templating.Tests/TestFiles`](https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests/CodeBrix.Templating.Tests/TestFiles) |
| Liquid compatibility cases | Liquid tags, filters, raw and comment blocks | [`tests/CodeBrix.Templating.Tests/LiquidTests`](https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests/CodeBrix.Templating.Tests/LiquidTests) |
| JSON bridge cases | `JsonElement` import, `object.from_json` / `object.to_json`, round-tripping | [`tests/CodeBrix.Templating.Tests/TestJsonSupport`](https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests/CodeBrix.Templating.Tests/TestJsonSupport) |
| `CustomTemplateLoader.cs` | Two `ITemplateLoader` implementations, one per language mode | [`tests/CodeBrix.Templating.Tests/CustomTemplateLoader.cs`](https://github.com/ellisnet/CodeBrix.Templating/blob/main/tests/CodeBrix.Templating.Tests/CustomTemplateLoader.cs) |
| `MathObject.cs` | A static class imported into a template as a function table | [`tests/CodeBrix.Templating.Tests/MathObject.cs`](https://github.com/ellisnet/CodeBrix.Templating/blob/main/tests/CodeBrix.Templating.Tests/MathObject.cs) |
| `SpecialFunctionProvider.cs` | Static methods with optional and `params` arguments, exercising argument binding | [`tests/CodeBrix.Templating.Tests/SpecialFunctionProvider.cs`](https://github.com/ellisnet/CodeBrix.Templating/blob/main/tests/CodeBrix.Templating.Tests/SpecialFunctionProvider.cs) |

The fixture folders run in order of concern: `000-basic`, `010-literals`, `020-interpolation`,
`100-expressions`, `200-statements`, `300-functions`, `400-builtins` (one file per builtin table, plus
one that prints the complete list of registered builtin names), `500-liquid` and `600-ast`. Dropping a
new pair into one of them adds a test case with no code change.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Templating/blob/main/README.md) |
| Complete API and language reference (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Templating/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Templating/blob/main/README-INDEX.txt) |
| Non-package content in the repository | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Templating/blob/main/EXTRAS-README.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.Templating.Tests](https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests/CodeBrix.Templating.Tests) |

XML documentation ships alongside the assembly, and the package carries `AGENT-README.txt` - a complete
API reference and usage guide that also documents the template-language syntax the engine accepts - so
an AI coding agent can be pointed straight at it.

## License

CodeBrix.Templating is licensed under the BSD 2-Clause License, and the license is also named in the
package ID (`CodeBrix.Templating.BsdLicenseForever`). For the provenance and licensing of open source
code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Templating/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.PdfDocuments](CodeBrix.PdfDocuments.md) - render the HTML or Markdown a template produced into a PDF
- [CodeBrix.Json.Extensions](CodeBrix.Json.Extensions.md) - more `System.Text.Json` machinery for the models you bind
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.Templating on GitHub](https://github.com/ellisnet/CodeBrix.Templating) - source and tests
