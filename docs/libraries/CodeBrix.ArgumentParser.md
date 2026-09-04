<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.ArgumentParser</sub>

# CodeBrix.ArgumentParser

**CodeBrix.ArgumentParser turns a `string[] args` into invocations of callbacks you register, and
generates the matching `--help` text for you.** It handles short (`-v`), long (`--verbose`) and
Windows-style (`/v`) prefixes, required and optional values, typed values, flag bundling, response files
and whole suites of sub-commands. It is fully managed with no dependencies beyond the .NET base class
libraries, so it runs anywhere .NET 10 runs - in a console tool, a background service, or the
command-line surface of a CodeBrix.Platform application.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.ArgumentParser](https://github.com/ellisnet/CodeBrix.ArgumentParser) |
| **Packages** | [`CodeBrix.ArgumentParser.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.ArgumentParser.MitLicenseForever) |
| **License** | MIT License; see [License](#license) |
| **Requires** | .NET 10 or later; no NuGet dependencies of its own |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Every platform .NET 10 supports - pure managed IL with no platform-specific code |

## What it does

- Parses options with three prefixes - `-v`, `--verbose` and `/v` - from one registration
- Distinguishes required values (`name=`), optional values (`name:`) and value-less flags, and
  understands `+`/`-` boolean negation (`-a+`, `-a-`)
- Bundles short flags (`-abc` is `-a -b -c`) and attached values (`-Dk=v`)
- Converts typed values through `System.ComponentModel.TypeConverter`, so `Add<int>`, `Add<FileInfo>`
  and `Add<int?>` work with no extra code
- Supports two-value (key/value) options with configurable separators
- Routes every unmatched argument to a default handler registered as `"<>"`, or returns them from
  `Parse`
- Stops option processing at a `--` terminator
- Expands response files: `@options.rsp` is replaced in place by the arguments it holds, with cycle
  detection and a nesting cap
- Models suites of sub-commands with `Command`, `CommandSet` and `HelpCommand`, including nested suites
  and command-name completion
- Renders 80-column help from the descriptions you already wrote, with category headings and hidden
  options
- Runs every user-facing string through a localization converter you supply

## When to use it

Use it when a program's interface is a command line and you want the parsing, the help text and the
error messages to come from one declaration. Registration is a collection initializer, parsing is one
call, and `OptionException` is the single exception type to catch around it.

The model is callbacks, not binding: you write `v => name = v` and own the variable. That keeps the whole
parser dependency-free, and it is the reason for most of what the library deliberately leaves out:

- No attribute- or POCO-based binding. There is no `[Option]` attribute and no mapping to a settings
  class; you register callbacks.
- No dependency-injection, configuration-provider, or `IHost` integration.
- No automatic `--version`, no automatic `--help` for a bare `OptionSet` (only `CommandSet` wires up
  help), and no man-page generation.
- No required-option, mutually-exclusive-option or argument-count validation.
- No shell-completion script generation. `CommandSet.GetCompletions` returns candidate names; wiring
  them into bash/zsh/pwsh is the caller's job.
- No environment-variable or config-file input. Response files are the only built-in external source;
  anything else means writing an `ArgumentSource`.
- No sandboxing or allow-listing of `@file` paths.
- No console-width detection, color or ANSI styling in help output; the layout is a fixed 80 columns.
- No async or cancellable parsing, and no thread-safety guarantee: an `OptionSet` is meant to be built
  and parsed on one thread.
- No localization resources. `MessageLocalizer` is the hook; the strings are yours to translate.
- It is not a shell: quoting, globbing and variable expansion are done by the operating system before
  your process starts (only response files do their own quote handling).

## Getting started

```bash
dotnet add package CodeBrix.ArgumentParser.MitLicenseForever
```

```csharp
using CodeBrix.ArgumentParser;
```

The package id is `CodeBrix.ArgumentParser.MitLicenseForever`; the assembly and the primary namespace are
`CodeBrix.ArgumentParser`, and there is no package named plain `CodeBrix.ArgumentParser`.

Build an `OptionSet`, call `Parse`, catch `OptionException` - that is the whole of the common case:

```csharp
using CodeBrix.ArgumentParser;

int verbose = 0;
bool showHelp = false;
string name = null;

var p = new OptionSet
{
    { "v|verbose",  "Increase verbosity.",      v => ++verbose },
    { "n|name=",    "The {NAME} to greet.",     v => name = v },
    { "h|?|help",   "Show this message and exit.", v => showHelp = v != null },
};

List<string> extra = p.Parse(args);

if (showHelp)
{
    p.WriteOptionDescriptions(Console.Out);
    return;
}

Console.WriteLine($"Hello, {name}! (verbosity={verbose})");
```

`Parse` invokes each matched option's callback as it walks the arguments and returns the arguments it did
not consume. `WriteOptionDescriptions` renders the help text from the same declarations, so the help can
never drift from the parser.

Four namespaces hold everything, and one of them covers the common case:

```csharp
using CodeBrix.ArgumentParser;                  // OptionSet, OptionException,
                                                // OptionAction<TKey,TValue>
using CodeBrix.ArgumentParser.Options;          // Option, OptionContext,
                                                // OptionValueCollection,
                                                // OptionValueType
using CodeBrix.ArgumentParser.ArgumentSources;  // ArgumentSource,
                                                // ResponseFileSource
using CodeBrix.ArgumentParser.Commands;         // Command, CommandSet,
                                                // HelpCommand
```

You only need the `Options` namespace when you touch `OptionContext`, `OptionValueCollection`,
`OptionValueType`, or subclass `Option`.

## Key concepts

### OptionSet and its Add overloads

`OptionSet` is a `KeyedCollection<string, Option>` keyed by every alias of every option, so
`Contains("v")` and `Contains("verbose")` are both true for the prototype `"v|verbose"`, and the default
handler is reachable as `this["<>"]`. It has two constructors - the plain one and one taking a message
localizer - and this registration surface:

```csharp
OptionSet Add(string header)
OptionSet Add(Option option)
OptionSet Add(string prototype, Action<string> action)
OptionSet Add(string prototype, string description, Action<string> action)
OptionSet Add(string prototype, string description, Action<string> action,
              bool hidden)
OptionSet Add(string prototype, OptionAction<string, string> action)
OptionSet Add(string prototype, string description,
              OptionAction<string, string> action)
OptionSet Add(string prototype, string description,
              OptionAction<string, string> action, bool hidden)
OptionSet Add<T>(string prototype, Action<T> action)
OptionSet Add<T>(string prototype, string description, Action<T> action)
OptionSet Add<TKey, TValue>(string prototype,
                            OptionAction<TKey, TValue> action)
OptionSet Add<TKey, TValue>(string prototype, string description,
                            OptionAction<TKey, TValue> action)
OptionSet Add(ArgumentSource source)
```

The `Action<string>` overloads create an option with `MaxValueCount == 1`; the `OptionAction<...>`
overloads create one with `MaxValueCount == 2`; `Add<T>` and `Add<TKey, TValue>` convert each value with
the target type's `TypeConverter` and wrap a conversion failure in an `OptionException`; `hidden: true`
keeps the option working but omits it from the help.

> [!IMPORTANT]
> `Add(string header)` is not an option. A single-string `Add` inserts a category or heading line into
> the help output at that position, so `options.Add("verbose")` silently adds the text "verbose" to the
> help. An option always needs at least a prototype and a callback.

### The option prototype grammar

A prototype names every alias of an option and says what it takes:

```text
name:    .+
type:    [=:]        // '=' required value, ':' optional value,
                     // absent = value-less flag
sep:     ( [^{}]+ | '{' .+ '}' )?
aliases: ( name type sep ) ( '|' name type sep )*
```

| Prototype | What it accepts |
| --- | --- |
| `"v\|verbose"` | a flag: `-v`, `--v`, `/v`, `--verbose`, `/verbose` |
| `"n\|name="` | a required value: `--name=X`, `--name:X`, `--name X`, `-nX` |
| `"n\|name:"` | an optional value: `--name=X`, `--name:X`, or `--name` yielding null |
| `"<>"` | the default handler for unmatched arguments |
| `"D="` | a two-value option; `:` and `=` separate by default |
| `"D={=}"` | a two-value option where `=` is the only separator |
| `"D={-->}{=>}"` | two multi-character separators |
| `"D=+-*/"` | each of `+ - * /` separates |
| `"D={}"` | two values that must arrive as two separate arguments |

The constructor validates the prototype before parsing ever starts: the value type must agree across all
aliases (`"a=|b:"` throws), separators are only meaningful when an option takes more than one value,
`"<>"` may not require a value, and braces must balance.

Descriptions carry the value's display name for the help output: `"the {NAME} to greet"` renders
`--name=NAME`, `"define a {0:NAME}={1:VALUE} macro"` renders `--define=NAME=VALUE`, and `{{` and `}}` are
literal braces. Without a `{...}` marker the placeholder is `VALUE` for a one-value option and `VALUE1`,
`VALUE2` and so on for a multi-value option.

### Option, OptionContext and the values

`Option` is the abstract base; the `Add` overloads construct one for you, and you subclass it when a
callback is not enough. Its members are `Prototype`, `Description`, `OptionValueType`, `MaxValueCount`,
`Hidden`, `GetNames()`, `GetValueSeparators()`, `Invoke(OptionContext c)`, `ToString()`, the abstract
`OnParseComplete(OptionContext c)` you override, and the protected `Parse<T>(string value,
OptionContext c)` - the same `TypeConverter` conversion `Add<T>` uses, `OptionException` wrapping
included.

`OptionValueType` is `None` (a flag), `Optional` (the prototype ends with `:`) or `Required` (it ends
with `=`). `OptionContext` is the per-argument state handed to an option while it parses: `Option`,
`OptionName` (the flag prefix the user actually typed, which is what you want in error messages),
`OptionIndex`, `OptionSet` and `OptionValues`.

`OptionValueCollection` holds the values gathered for the option currently being parsed, and its indexer
validates before returning: `InvalidOperationException` when no option is being parsed,
`ArgumentOutOfRangeException` past `MaxValueCount`, and `OptionException` ("Missing required value for
option ...") when the option is `Required` and no value arrived. That last one is how a missing value at
the end of a command line surfaces.

### OptionException

`OptionException` is the one exception type to catch around `Parse` for user-input errors. `OptionName`
names the offending option, and `InnerException` carries the original `TypeConverter` failure for typed
options. Response-file failures report an `OptionName` of `"@file"`.

| What went wrong | What you get |
| --- | --- |
| Bad prototype at registration | an `ArgumentException`-family exception |
| Duplicate alias | `ArgumentException` |
| Unconvertible typed value | `OptionException`, with the converter failure as `InnerException` |
| Missing value for a required option | `OptionException` |
| Unregistered letter inside a bundle | `OptionException` |
| Cyclic or too deeply nested `@file` | `OptionException` |
| Missing `@file` | `FileNotFoundException` from inside `Parse` |
| Unmatched argument | returned by `Parse`, or routed to the `"<>"` handler |

### Response files and ArgumentSource

An `ArgumentSource` expands one command-line token into a sequence of arguments. Register instances with
`OptionSet.Add(ArgumentSource)`; the abstract members are `GetNames()` (the trigger tokens),
`Description`, and `GetArguments(string value, out IEnumerable<string> replacement)`, plus two static
readers - `GetArgumentsFromFile(string file)` and `GetArguments(TextReader reader)` - that both tokenize
on the space character and treat a `"..."` or `'...'` run as one token with the quotes stripped.
`GetArgumentsFromFile` disposes the reader it opened; `GetArguments(TextReader)` never disposes the
reader you hand it. An unterminated quote throws `OptionException`.

`ResponseFileSource` is the built-in one. Add it and any argument of the form `@path` is replaced, in
place, by the tokens read from `path`; response files may reference further response files. Files are
opened lazily and closed deterministically even when parsing throws part-way through, a self-referencing
or transitively cyclic file throws `OptionException` (cycle detection is by canonical full path,
case-insensitive), and nesting deeper than `ResponseFileSource.MaxNestingDepth` throws too.

> [!WARNING]
> `@` paths are not sandboxed. Absolute paths, UNC paths and `..`-relative paths are all honored, and a
> missing file surfaces as `FileNotFoundException` from inside `Parse`. If the command line can come from
> an untrusted source, screen `@`-prefixed tokens yourself before calling `Parse`.

### Commands and CommandSet

A `Command` is one named sub-command: `Name` (whitespace-normalized, so multi-word names are supported
and matched greedily), `Help`, an optional `Options` of its own, an optional `Run` action, and a virtual
`Invoke` that parses `arguments` through `Options`, passes what is left to `Run`, and returns `0`.
Override `Invoke` when you need a different exit code.

A `CommandSet` is a suite of commands plus its own suite-level `OptionSet`. Its four-argument constructor
takes the output and error writers, which is the one to use in tests; the single-argument form writes to
`Console.Out` and `Console.Error`. `Run(args)` is the whole entry point of a suite, and it returns the
process exit code:

1. It registers a `HelpCommand` and hidden `help` / `?` options, if the suite does not already have them.
2. It parses `args` with the suite-level option set.
3. With nothing left over it prints `Use '<suite> help' for usage.` and returns `1` - or shows the help
   and returns `0` when `--help` was given.
4. Otherwise it resolves the leading argument or arguments to a command, longest match first and nested
   suites included, and returns that command's `Invoke` result.
5. An unrecognized command writes "Unknown command" plus a usage hint to the error writer and returns `1`.

`Add(CommandSet)` nests a suite: its commands become reachable as `<nested-suite> <command>` and appear
in help under that compound name. A `Command` instance may belong to only one `CommandSet`; adding it to
a second throws `ArgumentException`. `GetCompletions(prefix)` returns the command names that start with
`prefix`, case-insensitively, including nested `"<suite> <command>"` names - a building block for shell
completion, not a completion-script generator.

`HelpCommand` is what `<suite> help` runs. `<suite> help` writes the suite's full option descriptions,
which include one line per command; `<suite> help <command>` writes that command's own option
descriptions, or invokes the command with `--help` when it has no `Options`; `<suite> help help` writes a
usage banner and an alphabetically sorted list of available commands, nested suites included; and
`<suite> help <unknown>` writes "Unknown command" to the error writer and returns `1`.

### Help output and localization

`WriteOptionDescriptions` renders a fixed 80-column layout: the option column is 29 characters wide,
descriptions wrap at column 80, and continuation lines indent to column 31. An option whose prototype
column exceeds 29 characters puts its description on the next line. Options and category headers render
in the order you added them, every registered `ArgumentSource` renders last regardless of when it was
added, and hidden options are skipped entirely. Short aliases render as `-x`, long aliases as `--xyz`,
and optional values in brackets: `-l, --log[=FILE]`.

`MessageLocalizer` runs over help text and error messages alike. Supply it through the constructor:

```csharp
// MessageLocalizer runs over help text AND error messages.
var options = new OptionSet(s => Resources.ResourceManager
    .GetString(s) ?? s)
{
    { "v|verbose", "be verbose", v => verbose = v != null },
};
```

## Examples

Every option shape in one program - flags, required and optional values, a typed value, a key/value
option, a repeatable counter, and a response-file source:

```csharp
using System;
using System.Collections.Generic;
using CodeBrix.ArgumentParser;
using CodeBrix.ArgumentParser.ArgumentSources;

internal static class Program
{
    private static int Main(string[] args)
    {
        var names = new List<string>();
        var macros = new Dictionary<string, string>();
        int repeat = 1;
        int verbosity = 0;
        bool upper = false;
        bool showHelp = false;
        string log = null;

        var options = new OptionSet
        {
            "Usage: greet [OPTIONS]+ MESSAGE",
            "Greet a person a number of times.",
            "",
            "Options:",
            { "n|name=", "the {NAME} of someone to greet",
                v => names.Add(v) },
            { "r|repeat=", "the number of {TIMES} to repeat",
                (int v) => repeat = v },
            { "u|upper", "greet in upper case",
                v => upper = v != null },
            { "l|log:", "write a log to {FILE}",
                v => log = v ?? "greet.log" },
            { "D|define={=}", "define a {0:NAME}={1:VALUE} macro",
                (k, v) => macros[k] = v },
            { "v", "increase debug message verbosity",
                v => { if (v != null) { ++verbosity; } } },
            { "h|help", "show this message and exit",
                v => showHelp = v != null },
            new ResponseFileSource(),
        };

        List<string> extra;
        try
        {
            extra = options.Parse(args);
        }
        catch (OptionException e)
        {
            Console.Error.WriteLine("greet: " + e.Message);
            Console.Error.WriteLine("Try `greet --help' for more information.");
            return 1;
        }

        if (showHelp)
        {
            options.WriteOptionDescriptions(Console.Out);
            return 0;
        }

        string message = extra.Count > 0
            ? string.Join(" ", extra)
            : "Hello";
        if (upper)
        {
            message = message.ToUpperInvariant();
        }

        foreach (string name in names)
        {
            for (int i = 0; i < repeat; ++i)
            {
                Console.WriteLine("{0}, {1}!", message, name);
            }
        }

        if (verbosity > 0)
        {
            Console.Error.WriteLine("verbosity: " + verbosity);
        }
        if (log != null)
        {
            Console.Error.WriteLine("log file: " + log);
        }
        foreach (KeyValuePair<string, string> macro in macros)
        {
            Console.Error.WriteLine($"macro {macro.Key} = {macro.Value}");
        }
        return 0;
    }
}
```

That declaration renders exactly this help text:

```text
Usage: greet [OPTIONS]+ MESSAGE
Greet a person a number of times.

Options:
  -n, --name=NAME            the NAME of someone to greet
  -r, --repeat=TIMES         the number of TIMES to repeat
  -u, --upper                greet in upper case
  -l, --log[=FILE]           write a log to FILE
  -D, --define=NAME=VALUE    define a NAME=VALUE macro
  -v                         increase debug message verbosity
  -h, --help                 show this message and exit
  @file                      Read response file for more options.
```

The `"<>"` default handler collects everything that is not an option - and changes what `Parse` returns:

```csharp
var files = new List<string>();
bool recurse = false;

var options = new OptionSet
{
    { "r|recurse", "recurse into sub-directories", v => recurse = v != null },
    { "<>", v => files.Add(v) },
};

List<string> extra = options.Parse(new[] { "-r", "a.txt", "b.txt" });
// files: a.txt, b.txt
// extra: EMPTY -- the "<>" handler consumed both, so nothing is returned
```

A suite of sub-commands, each with its own options, returning the process exit code:

```csharp
using System;
using System.Collections.Generic;
using CodeBrix.ArgumentParser;
using CodeBrix.ArgumentParser.Commands;

internal static class Program
{
    private static int Main(string[] args)
    {
        bool verbose = false;

        var build = new Command("build", "Build the widget");
        string output = "bin";
        bool release = false;
        build.Options = new OptionSet
        {
            { "o|output=", "output {DIR}", v => output = v },
            { "release", "build in release mode",
                v => release = v != null },
        };
        build.Run = extra =>
        {
            Console.WriteLine($"building into {output} " +
                $"({(release ? "release" : "debug")}); " +
                $"targets: {string.Join(",", extra)}");
        };

        var clean = new Command("clean", "Remove build output")
        {
            Run = extra => Console.WriteLine("cleaning"),
        };

        var suite = new CommandSet("widget")
        {
            "Usage: widget COMMAND [OPTIONS]",
            { "v|verbose", "be verbose", v => verbose = v != null },
        };
        suite.Add(build);
        suite.Add(clean);

        return suite.Run(args);       // returns the process exit code
    }
}
```

The same suite is testable without touching the console, by handing it writers:

```csharp
var stdout = new StringWriter();
var stderr = new StringWriter();
var suite = new CommandSet("widget", stdout, stderr);
suite.Add(new Command("do") { Run = _ => { } });
int exit = suite.Run(new[] { "do" });      // exit == 0
```

Response files put a long command line in a file:

```csharp
// options.rsp:
//     --name=Alice --verbose
//     "--message=hello there"

var options = new OptionSet
{
    new ResponseFileSource(),
    { "n|name=", v => name = v },
    { "m|message=", v => message = v },
    { "v|verbose", v => verbose = v != null },
};

options.Parse(new[] { "@options.rsp" });
// name == "Alice", verbose == true, message == "hello there"
```

When a callback is not enough, subclass `Option` - and when the arguments come from somewhere the library
does not know about, subclass `ArgumentSource`:

```csharp
using CodeBrix.ArgumentParser.Options;

internal sealed class DateOption : Option
{
    private readonly Action<DateTime> _action;

    public DateOption(string prototype, string description,
                      Action<DateTime> action)
        : base(prototype, description, 1)
    {
        _action = action;
    }

    protected override void OnParseComplete(OptionContext c)
    {
        string raw = c.OptionValues[0];
        if (!DateTime.TryParse(raw, out DateTime value))
        {
            throw new OptionException(
                $"'{raw}' is not a date for option {c.OptionName}.",
                c.OptionName);
        }
        _action(value);
    }
}

var options = new OptionSet();
options.Add(new DateOption("since=", "only entries after {DATE}",
    d => since = d));

// A source that pulls defaults out of the environment:
internal sealed class EnvironmentSource : ArgumentSource
{
    public override string[] GetNames() => new[] { "%VAR%" };

    public override string Description =>
        "Read more options from an environment variable.";

    public override bool GetArguments(string value,
                                      out IEnumerable<string> replacement)
    {
        if (value == null || value.Length < 3 ||
            !value.StartsWith("%") || !value.EndsWith("%"))
        {
            replacement = null;
            return false;
        }
        string text = Environment.GetEnvironmentVariable(
            value.Trim('%')) ?? string.Empty;
        replacement = ArgumentSource.GetArguments(new StringReader(text));
        return true;
    }
}
```

The whole registration surface, in one initializer, as a reference card:

```csharp
// Registration
var o = new OptionSet {
    "Heading",                                  // category line
    { "v|verbose", "desc", v => f = v != null },// flag
    { "n|name=",   "desc", v => s = v },        // required value
    { "l|log:",    "desc", v => s = v },        // optional value
    { "p|port=",   "desc", (int v) => p = v },  // typed value
    { "D=",        "desc", (k, v) => d[k] = v },// key/value pair
    { "secret=",   "desc", v => {}, true },     // hidden from help
    { "<>",                v => rest.Add(v) },  // default handler
    new ResponseFileSource(),                   // @file expansion
};

// Parse / help
List<string> extra = o.Parse(args);   // throws OptionException on bad input
o.WriteOptionDescriptions(Console.Out);

// Command suite
int exit = new CommandSet("suite") {
    "Usage: suite COMMAND [OPTIONS]",
    { "v|verbose", "desc", v => f = v != null },
}
.Add(new Command("build", "Build it") {
    Options = new OptionSet { { "o|out=", v => outDir = v } },
    Run = rest => DoBuild(outDir, rest),
})
.Run(args);
```

## Using it in a CodeBrix.Platform application

There is nothing to register and nothing platform-specific: the whole library is managed code with no
dependencies of its own, and a project needs only the package reference.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>Greet</RootNamespace>
    <AssemblyName>greet</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.ArgumentParser.MitLicenseForever" />
  </ItemGroup>
</Project>
```

One deployment note: the typed `Add<T>` overloads convert values through
`System.ComponentModel.TypeDescriptor.GetConverter`, which is reflection-based. If you trim aggressively,
either keep the converter types you rely on, or use the `Action<string>` overloads and convert the value
yourself inside the callback. Nothing else in the library reflects.

## Pitfalls

- A flag's callback receives a string, not a bool. For `-v` the callback gets the option name (`"v"`);
  for `-v+` it gets the raw argument; for `-v-` it gets `null`. The idiom is `v => flag = v != null`.
  Writing `v => flag = bool.Parse(v)` will throw.
- `"<>"` swallows the return value of `Parse`. When a default handler is registered, unmatched arguments
  are routed to it and `Parse` returns an empty list. Pick one mechanism. `"<>"` also cannot require a
  value - `"<>="` throws `ArgumentException` at registration time.
- Optional-value options (`:`) never take the next argument. `--log run.txt` with the prototype `"l|log:"`
  invokes the callback with `null` and leaves `run.txt` in the unmatched list; the value must be attached
  (`--log=run.txt`, `--log:run.txt`, `-lrun.txt`). Required-value options (`=`) do consume the next
  argument.
- Bundling only works with the single `-` prefix. `-abc` expands to `-a -b -c`, while `--abc` and `/abc`
  are looked up as whole names. Inside a bundle, an unregistered letter in the first position means
  "not a bundle" and the argument falls through unmatched, but an unregistered letter in any later
  position throws `OptionException`.
- `--` stops option processing and is itself discarded, but it does not stop a `"<>"` handler from firing
  - the remaining arguments go to the handler if one is registered, otherwise to the returned list.
  Response-file expansion is also disabled after `--`, so a trailing `@file` stays a literal string.
- `TypeConverter` failures surface as `OptionException`, not `FormatException`. Catch `OptionException`
  around `Parse` and print `e.Message`; that message already names the option and the target type.
- Add order is help order. Options, categories and commands render in registration order, and
  `ArgumentSource` entries always render last regardless of where they were added. Put a heading
  immediately before the group it introduces.
- Aliases are dictionary keys, so two options may not share a name. Adding `"v|verbose"` and then
  `"v|version"` throws `ArgumentException` on the second `Add`.
- Nothing enforces that an option was supplied. A required option is your job: check your variables after
  `Parse` and report the error yourself. Only a missing value for an option the user did type raises
  `OptionException`, and that error surfaces at the end of parsing, when the pending option is flushed.
- `Add<T>` is for options that take a value. Applying it to a value-less prototype
  (`Add<bool>("v", ...)`) makes the parser hand the option name to the converter, which throws. Use the
  `Action<string>` overload for flags.
- Two-value options default to `:` and `=` as separators, so `-Dkey:value` and `-Dkey=value` both work,
  and a value containing `=` will be split. Pin the separator with a brace group (`"D={=}"`) or suppress
  splitting entirely with `"D={}"` and pass the two values as separate arguments.
- Response-file tokens are split on the space character only, so a tab inside a response file becomes part
  of the token, and there is no comment syntax - every non-blank token is an argument.
- `CommandSet.Run` mutates the suite the first time it is called, adding a `HelpCommand` and the hidden
  `help` / `?` options. Do not assert on `Count` before and after the first run, and do not share a
  `Command` instance between two `CommandSet`s.
- `Command.Invoke` returns `0` unless you override it, so a failing command still produces exit code 0
  unless you subclass `Command`, or set a field from `Run` and return it yourself from `Main`.
- If you subclass `Option` and read `c.OptionValues[0]`, you are relying on the indexer's validation to
  raise the "missing required value" error. Do not guard it away with a `Count` check unless you intend
  to accept missing values.
- Build the `OptionSet` once rather than inside a loop - each instance allocates its own `Regex` - and do
  the real work after `Parse` returns, from the variables the callbacks set, rather than inside the
  callbacks. Response files are read lazily and streamed token by token, so a large `@file` never has to
  fit in memory as a token array.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.ArgumentParser/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.ArgumentParser/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.ArgumentParser/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.ArgumentParser.Tests](https://github.com/ellisnet/CodeBrix.ArgumentParser/tree/main/tests/CodeBrix.ArgumentParser.Tests) |

The repository ships no samples, demo applications or tools: it holds exactly one packable project and
one test project. The test project locks down parser behavior and doubles as the worked-example set the
AGENT-README points at, with one file per area - `OptionSetTests.cs` for prefixes, values, bundling,
termination and help rendering; `OptionTests.cs` for the prototype grammar and its error cases;
`OptionContextTests.cs` for the value-collection validation; `CommandTests.cs` and `CommandSetTests.cs`
for suite dispatch, exit codes and completions; and `ArgumentSourceTests.cs` and
`ResponseFileSourceTests.cs` for tokenization, quoting, nesting and cycles. It needs no test data files,
no environment variables and no network access.

## License

CodeBrix.ArgumentParser is licensed under the MIT License; the license is also named in the package ID
(`CodeBrix.ArgumentParser.MitLicenseForever`). For the provenance and licensing of open source code
included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.ArgumentParser/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.AssemblyTools](CodeBrix.AssemblyTools.md) - the other library for build-time and tooling work, and a natural companion in a command-line tool
- [SilverAssertions](SilverAssertions.md) - assert on a `CommandSet`'s exit codes and captured output in tests
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.ArgumentParser on GitHub](https://github.com/ellisnet/CodeBrix.ArgumentParser) - source and tests
