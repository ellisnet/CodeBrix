<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.LilyScheme</sub>

# CodeBrix.LilyScheme

**A managed, cross-platform Scheme language implementation for .NET, for applications that need to run Scheme source - or to embed a scriptable Scheme layer of their own - without leaving managed code.** It implements full `syntax-case` macro expansion, the module system, a class and generic-function object system, the numeric tower, a dialect reader, ports, POSIX and regular-expression surfaces, and a modern exception API. Any .NET 10 application can host it, including a CodeBrix.Platform application.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.LilyScheme](https://github.com/ellisnet/CodeBrix.LilyScheme) |
| **Packages** | [`CodeBrix.LilyScheme.LgplLicenseForever`](https://www.nuget.org/packages/CodeBrix.LilyScheme.LgplLicenseForever) |
| **License** | LGPL-3.0-or-later; see [License](#license) |
| **Requires** | .NET 10 or later; no native libraries and no NuGet dependencies |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Windows, Linux and macOS |

## What it does

- Expands macros with full `syntax-case`: hygienic macros, `syntax-rules`, `quasisyntax` and the rest.
- Evaluates Tree-IL, the expander's intermediate representation - all eighteen node types, evaluated directly.
- Implements the module system: `define-module`, `use-modules` (including `#:select` renaming), public interfaces and exports, submodules, autoloading, and the anonymous-module naming that macro hygiene depends on.
- Gives you both import shapes: `use-modules` without `#:select` imports a module's public interface, and `Interpreter.NarrowModuleImports = false` asks instead for the whole module, private names included.
- Provides classes, generic functions, `define-method` dispatch and extension of generic-capable primitives.
- Implements the full numeric tower - fixnum, bignum, rational, real and complex.
- Reads the dialect: keywords, extended symbols, `#nil`, block and datum comments, array literals, fixed-width string escapes and reader hash extensions - recording source locations, which is what lets the expander attach them to procedures and error messages.
- Accepts all three R7RS line endings in source text, so a file checked out or authored with CRLF endings loads.
- Supports ports - soft ports, string ports, file ports and the `#:encoding` keyword - each tracking its own line and column, readable and settable from Scheme.
- Keeps an expansion cache that records each file's expanded Tree-IL and replays it, so a cold expansion is paid for once rather than on every start.
- Offers a POSIX-flavored surface: `(ice-9 rdelim)`, `(ice-9 popen)` with `system` and `system*`, `stat`, `strftime`/`localtime`/`gmtime`, `(ice-9 getopt-long)`, POSIX regular expressions with `(ice-9 regex)`, and `(srfi srfi-43)`.
- Implements the modern exception API - exception objects, `raise-exception`, `with-exception-handler` with `#:unwind?` and `#:unwind-for-type`, `(ice-9 exceptions)` and its standard types, `define-exception-type`, `raise-continuable` and R7RS `guard` - fully interoperable with classic `catch`/`throw` in both directions.
- Embeds from C#: register host primitives and values, redirect the output, error and input streams, install your own module loader, and read every Scheme value back as a .NET object.

## When to use it

Use it when an application needs a real Scheme inside the process: a scriptable extension layer, a rules language, a configuration dialect users can write, or a body of existing Scheme source that has to run on .NET. It ships as one managed assembly with no NuGet dependencies and no native libraries, so adding scripting costs one package reference.

Look elsewhere when you need speed of a compiled language or the parts it deliberately leaves out. There is no VM, no compiler and no bytecode: everything is interpreted, there are no stack frames, and so there is no backtrace, no debugger and no `make-stack`. There is no `call-with-current-continuation` - prompts are escape-only, so `call-with-prompt`, `abort-to-prompt` and `make-prompt-tag` work for aborting out of a thunk and re-entering a captured continuation fails loudly, while `dynamic-wind` is present. There is no FFI, no `dynamic-link` and no shared-library loading. There are no threads, futures or fibers at the Scheme level: `RunWithLargeStack` starts one thread for stack size, and that is the whole of the concurrency story. There are no exact complex numbers, no basic regular expressions, no input soft ports, no bidirectional ports, and no font, graphics, networking or notation layer - this package is the Scheme language only.

## Getting started

```bash
dotnet add package CodeBrix.LilyScheme.LgplLicenseForever
```

The assembly and the root namespace are `CodeBrix.LilyScheme`, without the `.LgplLicenseForever` suffix; the suffix is part of the package id only, so the license identification travels with the package name.

```csharp
using CodeBrix.LilyScheme;              // Interpreter
using CodeBrix.LilyScheme.Caching;      // ExpansionCache, ExpansionCacheFile
using CodeBrix.LilyScheme.Numeric;      // SchemeNumber, Ratio, ComplexNumber
using CodeBrix.LilyScheme.Primitives;   // TypeChecks, SchemeHashTable, ports,
                                        //   ColumnTrackingWriter,
                                        //   GOOPS classes, BuiltinClasses
using CodeBrix.LilyScheme.Reader;       // SchemeReader, SourceProperties,
                                        //   PortPosition
using CodeBrix.LilyScheme.Runtime;      // SchemeBootstrap, Printer, Evaluator,
                                        //   SchemeModule, SchemeThrow
using CodeBrix.LilyScheme.TreeIl;       // TreeIlEvaluator, TreeIlClosure
using CodeBrix.LilyScheme.Unicode;      // UnicodeCharacterNames
using CodeBrix.LilyScheme.Values;       // Pair, Symbol, MutableString, ...
```

`Interpreter` is the only type in the root namespace, and it is the entry point. Two calls are mandatory: `SchemeBootstrap.LoadCore(interpreter)` loads the macro expander and the prelude, and every evaluation runs inside `Interpreter.RunWithLargeStack`.

```csharp
using System;
using CodeBrix.LilyScheme;
using CodeBrix.LilyScheme.Reader;
using CodeBrix.LilyScheme.Runtime;

Interpreter interpreter = new Interpreter();
Interpreter.RunWithLargeStack(() =>
{
    SchemeBootstrap.LoadCore(interpreter);   // the macro expander and the prelude
    object form = SchemeReader.ReadAll("(+ 1 2)", "<input>")[0];
    object value = interpreter.TreeIlEvaluator.ExpandAndEval(form, interpreter.CurrentModule);
    Console.WriteLine(Printer.Write(value)); // 3
});
```

Until `LoadCore` has run there are no macros, no `and`/`or`/`cond`/`case`, no module forms and no vendored modules: a fresh `Interpreter` has only the C# primitives.

## Key concepts

### Two evaluators, and choosing between them

Which evaluator you call is the single most important decision an embedder makes.

```text
source text
   -> SchemeReader                  (text -> Scheme data)
   -> Evaluator                     (core s-expression evaluator; bootstrap)
   -> psyntax (vendored)            (macroexpand -> Tree-IL structs)
   -> TreeIlEvaluator               (evaluates the eighteen Tree-IL nodes)
```

`Interpreter.Evaluator` is the core evaluator. It knows `quote`, `if`, `lambda`, `lambda*`, `let`, `letrec`, `begin`, `set!`, `define`, `define-syntax` (recorded, not expanded), `quasiquote`, `case-lambda`, `delay` and `eval-when`, and nothing else; it has proper tail calls, it exists so the expander can be loaded, and it does not expand macros. `Interpreter.TreeIlEvaluator` is what everything else runs on.

```csharp
Interpreter.LoadFile(path)              -> core evaluator, NO macros
Interpreter.LoadFileWithProgress(...)   -> core evaluator, NO macros

Interpreter.Eval(form)                  -> full expansion once psyntax is loaded
Interpreter.EvalString(text, fileName)  -> full expansion once psyntax is loaded
interpreter.TreeIlEvaluator.ExpandAndEval(form, module)   -> full expansion
SchemeBootstrap.LoadExpanded(interpreter, source, name)   -> full expansion
```

A macro use handed to the core evaluator does not fail at definition time; it fails later, where the macro is used, as a wrong-type-arg saying the transformer cannot be applied. For loading a file of consumer code, use `SchemeBootstrap.LoadExpanded`.

### The host API

```csharp
Interpreter()
const int LargeStackBytes
ModuleRegistry Modules { get; }
SchemeModule GuileModule { get; }          // the root (guile) module
SchemeModule CurrentModule { get; set; }   // where top-level evaluation lands
Evaluator Evaluator { get; }
TreeIl.TreeIlEvaluator TreeIlEvaluator { get; }
bool IsPsyntaxLoaded { get; set; }         // set by LoadPsyntax/LoadCore
```

`DefinePrimitive(name, minimumArgumentCount, maximumArgumentCount, implementation)` registers a primitive in the root module, so every module sees it, and `DefineValue(name, value)` binds a non-procedure value there. `OutputWriter`, `ErrorWriter` and `InputReader` redirect the three streams; `TrackedOutputWriter()` and `TrackedErrorWriter()` hand back the position-tracking writers that `port-line` and `port-column` come from. `LoadPath` is the mutable list of directories `%search-load-path`, `primitive-load-path` and therefore `(load-from-path "name")` search. `ExpansionCache` is null by default, which loads live. `NarrowModuleImports` is true by default; set it false before loading the code it governs to get the wide import.

### Running on a big stack

`RunWithLargeStack` runs the work on a dedicated thread with a large stack, because the macro expander recurses hard while expanding and overflows the CLR's default one-megabyte stack; the limit is per thread, so a dedicated thread is the fix. A failure is re-thrown to the caller as itself, with its original stack trace, rather than wrapped - do not add a wrapper of your own. Wrap essentially everything: loading the bootstrap, loading files, and any evaluation that can recurse. Use one big-stack thread for a whole run rather than one per evaluation.

### Bootstrap and loading

`SchemeBootstrap.LoadCore(interpreter)` is the one call a normal embedder makes: it loads the expander, then the prelude, then enables module autoloading and installs the shim modules, and returns the number of top-level forms evaluated. Any registered reader hash extension is suspended for the duration and restored afterwards. Below it sit `LoadPsyntax`, `PrepareForPsyntax`, `LoadExpanded` (the loader for consumer code, which consults `Interpreter.ExpansionCache` when one is assigned) and `LoadSource` (core evaluator, bootstrap use only).

`SelfProvidedModules` lists the module names that must never be autoloaded from the vendored source even though that source is present: `(oop goops)`, `(ice-9 optargs)`, `(ice-9 and-let-star)`, `(ice-9 boot-9)`, `(guile)`, `(guile-user)`, `(system vm program)`, `(ice-9 iconv)`, `(ice-9 soft-ports)`, `(ice-9 unicode)` and `(ice-9 popen)`. If you install your own module loader, honor the same list.

### The value model

Every Scheme value is an `object`. There is no wrapper type and no unboxing step: you type-test what the evaluator hands back.

```text
Scheme                 C# representation
--------------------   ---------------------------------------------------
#t / #f                bool
the empty list ()      Values.Nil.Instance          (Values.Nil)
a pair                 Values.Pair
a symbol               Values.Symbol
a keyword #:k          Values.Keyword
a string               Values.MutableString
a character            Values.SchemeChar
a vector               object[]
an exact integer       long, widening to System.Numerics.BigInteger
an exact rational      Numeric.Ratio
an inexact real        double
a complex              Numeric.ComplexNumber
unspecified            Values.Unspecified.Instance
the EOF object         Values.EofObject.Instance
#nil (Elisp nil)       Values.ElispNil.Instance
multiple values        Values.MultipleValues
a procedure            Values.Procedure (abstract) or an IApplicable
a variable cell        Values.Variable
a fluid                Values.Fluid
a promise              Runtime.Promise or Values.LazyPromise
a hash table           Primitives.SchemeHashTable
an input port          Primitives.SchemeInputPort
an output port         Primitives.SchemeOutputPort
a character set        Values.CharSet
an array (rank != 1)   Values.SchemeArray  (a rank-1 array IS object[])
a record instance      object[] whose slot 0 is a Values.RecordType
a record type          Values.RecordType
a struct               Values.SchemeStruct   (Tree-IL nodes are these)
a GOOPS class          Primitives.SchemeClass
a GOOPS instance       Primitives.SchemeObject
a module               Runtime.SchemeModule
a compiled regexp      System.Text.RegularExpressions.Regex
a directory stream     Primitives.DirectoryStream
a hook                 Values.SchemeHook
```

A Scheme string is a `MutableString`, not a `System.String`, because `string-set!` has to work; most read-only text primitives also accept a symbol, character or keyword through `StringPrimitives.Text`. A `SchemeChar` holds a code point rather than a `char`, so astral characters are one Scheme character. An exact integer arrives as a `long` when it fits and a `BigInteger` when it does not, so never test for `long` alone. `Symbol.Intern` is the only way to get an `eq?` symbol.

### Calling Scheme from C#

`interpreter.Evaluator.Apply(procedure, arguments)` is the universal apply: it handles a core closure, a Tree-IL closure, a primitive (including generic dispatch), a `case-lambda` procedure, a generic function and, last, any `IApplicable` of your own.

```csharp
Variable variable = interpreter.CurrentModule.Lookup(Symbol.Intern("string-upcase"));
object procedure = variable.GetValue();
object result = interpreter.Evaluator.Apply(
    procedure, new object[] { new MutableString("hello") });
string text = result.ToString();          // "HELLO"
```

`SchemeModule.Lookup` searches the module's own bindings first and then walks its use list breadth-first, answering null when the name is unbound everywhere; `LookupLocal` restricts the search to the module's own bindings. A module lookup answers the *binding cell*, not the value - call `GetValue()`.

### Defining a host primitive

```csharp
interpreter.DefinePrimitive("host-add", 2, 2,
    a => SchemeNumber.Add(a[0], a[1]));
```

Validate arguments the Scheme way, not with a cast: a bare C# cast raises an `InvalidCastException` that escapes to the host where no Scheme `catch` can see it. `Primitives.TypeChecks` supplies `AsSymbol`, `AsChar`, `AsKeyword` and `AsMutableString`, each taking `(value, procedureName, position)` with one-based positions, and `Primitives.StringPrimitives.Text(value, procedureName)` accepts read-only text. `Primitive.Invoke` translates a stray `InvalidCastException` into a wrong-type-arg named for the primitive - treat that as the backstop, not the convention.

Three interfaces let a host object behave like a native one:

```csharp
public interface Values.IApplicable
    object Apply(object[] arguments)

public interface Values.ISchemeEqual
    bool SchemeEquals(object other)

public interface Values.ISchemePrintable
    string PrintRepresentation()
```

`IApplicable` lets your own type sit in operator position while staying its own type for every predicate, and it is consulted last in the apply path, so nothing built in is shadowed. `ISchemeEqual` is consulted only when both operands implement it, because `equal?` is symmetric. `BuiltinClasses.ClassOfExtensionHook` is how a host type becomes dispatchable by `define-method` without the library knowing about it.

### Modules

`Define` (`module-define!`) takes a value and gives the module a variable of its own. `AddVariable` (`module-add!`) takes a variable and installs *that cell* as the binding, so two modules share one location and a `set!` through either is seen by both. `Remove` (`module-remove!`) drops a module's own binding and nothing more, so a name it was shadowing goes back to resolving through imports. The three are not interchangeable.

A module you provide from C# must `DefinePublic` every name it means to export: `Define` alone makes a private binding, and the default narrow import then delivers nothing.

Saving and restoring `CurrentModule` around a load is not optional, and it is the most expensive mistake an embedder can make here. A loaded file's `define-module` makes its own module current and never puts the old one back; the symptom is not an error, because lookups still succeed through the use list - what breaks is shadowing, which surfaces much later as the wrong method winning. And chain rather than replace: `SchemeBootstrap.EnableModuleAutoload` keeps whatever loader is already installed and falls through to it, so ordering with your own loader is safe in either direction.

### The expansion cache

Macro expansion is the overwhelming majority of the cost of loading a Scheme layer, because the expander itself runs interpreted. Assign an `ExpansionCache` to `Interpreter.ExpansionCache` and `SchemeBootstrap.LoadExpanded` records each file's expanded Tree-IL on first load and substitutes it on later loads, keyed per file by name plus source hash. Everything is still evaluated live, in order: nested loads, module switches and load-time side effects behave exactly as an uncached boot.

Four rules govern it. Never share one instance between interpreters - recorded quoted constants become live, mutable runtime data when evaluated, so the file's bytes may be memoized but the graphs may not. Recording runs the expander in compile-and-evaluate mode and must not re-evaluate the form. Identity is part of the format: the serializer keeps an object table, and every repeated heap object round-trips to one object. And a cache can never falsify a boot: any mismatch, truncation or corruption is a miss and the boot records live again, while an unknown value type throws at record time so the boot keeps its live result and saves nothing.

The key is the caller's job. It must change whenever anything that shaped the expansion changes - your own assembly identity, this library's assembly identity, and the content of every Scheme source that participates.

### Errors and exceptions

```csharp
public class SchemeThrow : Exception       // NOT sealed -- see below
    SchemeThrow(object key, object arguments)
    object Key { get; }                  // the throw key, a Symbol
    object Arguments { get; }            // the remaining throw arguments, a LIST
    object ExceptionObject { get; set; } // the modern-API object, when there is one

public sealed class SchemeEvaluationException : Exception
    SchemeEvaluationException(string message)
    SchemeEvaluationException(string message, Exception innerException)

public sealed class SchemeReaderException : SchemeThrow   // namespace ...Reader
    SchemeReaderException(string message, object arguments)
    string ReaderMessage { get; }        // the text, position prefix included

public sealed class PromptAbort : Exception
    PromptAbort(object tag, object[] arguments)
    object Tag { get; }
    object[] Arguments { get; }
```

The keys you will actually meet are `wrong-type-arg`, `out-of-range`, `unbound-variable`, `wrong-number-of-args`, `misc-error`, `system-error`, `regular-expression-syntax`, `goops-error`, and `%exception` for a `raise-exception` of a plain object. Order your catch clauses derived-first: `SchemeReaderException` derives from `SchemeThrow`, and inside one `try` the compiler enforces the order for you, while across layers nothing does.

```csharp
try
{
    SchemeBootstrap.LoadExpanded(interpreter, source, fileName);
}
catch (SchemeReaderException syntaxError)   // FIRST: the derived type
{
    Report("Syntax error: " + syntaxError.ReaderMessage);
}
catch (SchemeThrow error)                   // then everything else Scheme threw
{
    Report("Scheme error: " + error.Message);
}
catch (SchemeEvaluationException loadError)  // which form of which file
{
    Report(loadError.Message);
}
```

Do not swallow `PromptAbort` in a C# primitive that calls back into Scheme: it carries an `abort-to-prompt` out to the matching `call-with-prompt`, and prompts here are escape-only.

### Ports, flushing and splicing paths

The file ports Scheme opens are buffered, and Scheme code is entitled not to close them, so call `flush-all-ports` at the end of every run that may have written files (`force-output` flushes one port). The tell for a forgotten flush is a set of output files whose sizes are all multiples of 1024. `close-port` on a file port disposes the writer rather than merely flushing it, while the current output and error ports are deliberately not disposed by it, because those writers belong to you and must survive being closed from Scheme.

Splicing a filesystem path into source text is not string concatenation. The reader implements fixed-width hex escapes, so a Windows path spliced in raw is not the path it names: `C:\Users\me` reaches `\U`, takes the next six characters as hex digits, and fails on the `s` of `Users`. The loud failure is the lucky case - a path component beginning with `a`, `b`, `f`, `n`, `r`, `t` or `v` spells a valid escape, so the source reads with no diagnostic and names a different file. Go through the printer instead:

```csharp
string source = "(open-input-file " + Printer.WriteString(path) + ")";
```

`Printer.ResetProgramPrintLatch` matters for a host that runs many input files in one process: the latch behind procedure printing never recovers if a non-local exit leaves it set, so reset it at the per-file boundary or every procedure printed after the first abort comes out in the low-level form.

## Examples

Defining a host primitive and calling it from Scheme, validating arguments the Scheme way.

```csharp
using System;
using CodeBrix.LilyScheme;
using CodeBrix.LilyScheme.Primitives;
using CodeBrix.LilyScheme.Runtime;
using CodeBrix.LilyScheme.Values;

Interpreter interpreter = new Interpreter();
Interpreter.RunWithLargeStack(() =>
{
    SchemeBootstrap.LoadCore(interpreter);

    // Two required arguments: (host-join symbol string) -> string.
    // TypeChecks and StringPrimitives raise a catchable Scheme wrong-type-arg
    // rather than letting an InvalidCastException escape to the host.
    interpreter.DefinePrimitive("host-join", 2, 2, a =>
    {
        Symbol prefix = TypeChecks.AsSymbol(a[0], "host-join", 1);
        string suffix = StringPrimitives.Text(a[1], "host-join");
        return new MutableString(prefix.Name + ":" + suffix);
    });

    object result = interpreter.EvalString("(host-join 'greeting \"hello\")", "<host>");
    Console.WriteLine(Printer.Display(result));   // greeting:hello
});
```

Installing your own module loader so `use-modules` finds your Scheme. Note the mandatory save and restore of `CurrentModule`, and the chaining to whatever loader was already installed.

```csharp
private static void InstallLoader(
    Interpreter interpreter, IReadOnlyDictionary<string, string> sources)
{
    Func<object, SchemeModule, bool> previous = interpreter.Modules.ModuleLoader;
    interpreter.Modules.ModuleLoader = (name, module) =>
    {
        string printed = Printer.Write(name);       // "(my app)"
        if (!sources.TryGetValue(printed, out string source))
        {
            return previous != null && previous(name, module);
        }

        SchemeModule saved = interpreter.CurrentModule;
        interpreter.CurrentModule = module;
        try
        {
            SchemeBootstrap.LoadExpanded(interpreter, source, printed + ".scm");
        }
        finally
        {
            interpreter.CurrentModule = saved;
        }

        return true;
    };
}

// ...
Interpreter interpreter = new Interpreter();
Interpreter.RunWithLargeStack(() =>
{
    SchemeBootstrap.LoadCore(interpreter);          // enables vendored autoload
    InstallLoader(interpreter, new Dictionary<string, string>
    {
        ["(my app)"] =
            "(define-module (my app) #:export (greet))\n"
            + "(define (greet who) (string-append \"hello, \" who))\n",
    });

    Console.WriteLine(Printer.Write(EvalOne(interpreter,
        "(begin (use-modules (my app)) (greet \"world\"))")));   // "hello, world"
});
```

Wiring the expansion cache: read it (a miss answers null), assign it, load, and write it back when it is dirty.

```csharp
private static Interpreter BootCached(string cachePath, string worldKey,
                                      IReadOnlyList<string> sources)
{
    Interpreter interpreter = new Interpreter();
    Interpreter.RunWithLargeStack(() =>
    {
        // A MISS -- absent file, wrong key, truncation, corruption -- answers
        // null, and the boot simply records live instead.
        ExpansionCache cache = ExpansionCacheFile.TryReadFile(cachePath, worldKey)
                               ?? new ExpansionCache();
        interpreter.ExpansionCache = cache;

        SchemeBootstrap.LoadCore(interpreter);
        for (int i = 0; i < sources.Count; i++)
        {
            SchemeBootstrap.LoadExpanded(
                interpreter, sources[i], "layer-" + i + ".scm");
        }

        if (cache.IsDirty)
        {
            ExpansionCacheFile.WriteFile(cache, cachePath, worldKey);
        }
    });

    return interpreter;
}
```

The world key that governs the cache must move whenever anything that shaped the expansion moves.

```csharp
string worldKey = string.Join(
    "|",
    typeof(MyHost).Assembly.ManifestModule.ModuleVersionId,
    typeof(Interpreter).Assembly.ManifestModule.ModuleVersionId,
    ExpansionCache.HashSource(string.Concat(sources)));
```

Redirecting the three streams, and flushing when the run is over.

```csharp
Interpreter interpreter = new Interpreter();
StringWriter output = new StringWriter();
StringWriter errors = new StringWriter();

Interpreter.RunWithLargeStack(() =>
{
    interpreter.OutputWriter = output;
    interpreter.ErrorWriter = errors;
    interpreter.InputReader = new StringReader("(1 2 3)\n");

    SchemeBootstrap.LoadCore(interpreter);
    EvalOne(interpreter, "(display (read)) (newline)");

    // Scheme is entitled to leave file ports open; nothing flushes them for
    // you, because an embedded interpreter has no process exit to hang that
    // on. Do this at the end of every run that may have written files.
    EvalOne(interpreter, "(flush-all-ports)");
    Printer.ResetProgramPrintLatch();     // per input file, if you run many
});

Console.WriteLine(output.ToString());     // (1 2 3)
```

The `EvalOne` helper those samples use, which is "read every form and expand-and-evaluate it".

```csharp
private static object EvalOne(Interpreter interpreter, string source)
{
    object result = Values.Unspecified.Instance;
    foreach (object form in SchemeReader.ReadAll(source, "<host>"))
    {
        result = interpreter.TreeIlEvaluator.ExpandAndEval(
            form, interpreter.CurrentModule);
    }

    return result;
}
```

## Pitfalls

- Evaluating with the wrong evaluator. `LoadFile` and `LoadFileWithProgress` run the core evaluator and do not expand macros - they are the boot paths that load the expander itself. A macro that fails where it is *used* rather than where it is defined means a core-evaluator path was taken.
- Not running on a big stack. Wrap the work in `Interpreter.RunWithLargeStack`; a failure on it reaches you as itself, with its original stack trace, so do not add a wrapper of your own.
- A bare cast in a host primitive. Use the positioned `TypeChecks` accessors or `StringPrimitives.Text`.
- A module loader that does not save `CurrentModule`. Nothing errors, because lookups still succeed through the use list; what breaks is shadowing, which surfaces much later as the wrong method winning.
- Splicing a filesystem path into source text. Go through `Printer.WriteString`.
- Sharing an `ExpansionCache` between interpreters. Recorded quoted constants become live, mutable data when evaluated: one deserialized instance per interpreter.
- Forgetting to flush. Call `flush-all-ports` at the end of a run.
- Forgetting `Printer.ResetProgramPrintLatch` between input files.
- Registering a reader hash extension and then bootstrapping by hand. Registration is process-wide, and a hand-rolled bootstrap that skips `SuspendHashExtensions` corrupts the expander with no error.
- Assuming an exact integer is a `long`. It is a `long` when it fits and a `BigInteger` when it does not, and primitives may hand you an `int`.
- Treating a Scheme string as a `System.String`, or constructing a `Symbol` instead of interning one, or mistaking a `Variable` for a value.
- Confusing the three module operations. `module-define!`, `module-add!` and `module-remove!` do different things.
- Extending a generic-capable primitive is global; defining a fresh generic is not. Getting this wrong is invisible from the defining module - everything there passes, and every other module resolves the raw primitive and raises a wrong-type-arg.
- Expecting the wide import by default. `Interpreter.NarrowModuleImports` is true; set it false *before* the code it governs is loaded.
- Relying on a name that exists only in the reference source that is vendored but never loaded. Check with `(defined? 'name)` rather than by reading vendored source; a module with no file behind it resolves empty, and every name it would have supplied comes out unbound.
- Constructing an input port from a `TextReader` and then asking Scheme to `read` from it. A stream-backed port streams, and refuses `read` loudly; construct the port from a string instead.
- Reaching for `SchemeOutputPort.Writer` when you want the concrete sink. What you assign is wrapped in a `ColumnTrackingWriter`; read `InnerWriter`.
- Assuming POSIX regular-expression behavior throughout. Flags are integers passed as separate rest arguments, `regexp/basic` and `regexp/noteol` are refused, bracket classes are ASCII, and alternation is leftmost-first rather than leftmost-longest.

## Samples and tools in the repository

The repository ships one NuGet package and has no sample applications or demo projects. One tool and the test suite are the non-package content.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| unicode-names | Generates the Unicode formal-name table the library embeds, from a Unicode Character Database file; `--check` verifies the shipped table instead of rewriting it | [`tools/unicode-names`](https://github.com/ellisnet/CodeBrix.LilyScheme/tree/main/tools/unicode-names) |

```bash
python3 tools/unicode-names/generate-unicode-names.py \
    [--check] [--version X.Y.Z] [UnicodeData.txt] [out.deflate]
```

Regenerating the table from a different Unicode Character Database is a deliberate act rather than a refresh, and nothing in the build regenerates it. The script needs Python 3 with the standard library only.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.LilyScheme/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.LilyScheme/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.LilyScheme/blob/main/README-INDEX.txt) |
| Tools and other non-package content | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.LilyScheme/blob/main/EXTRAS-README.txt) |
| Tests (the executable specification, including the host-embedding surface) | [tests/CodeBrix.LilyScheme.Tests](https://github.com/ellisnet/CodeBrix.LilyScheme/tree/main/tests/CodeBrix.LilyScheme.Tests) |

The test suite is long-running by design: read it rather than run it. `PsyntaxBootstrapTests.cs` is the minimal working embedding, `WrongTypeArgumentTests.cs` exercises host-registered primitives through `DefinePrimitive`, `ExpansionCacheTests.cs` records, serializes and replays the cache end to end, and `SchemeReaderTests.cs` covers the reader and the `Printer.WriteString` round trip.

## License

CodeBrix.LilyScheme is licensed under the GNU Lesser General Public License version 3 or later, and the license is also named in the package ID (`CodeBrix.LilyScheme.LgplLicenseForever`). This is a copyleft license and it constrains how you may link and redistribute: read the licensing section of the repository's AGENT-README.txt before choosing this package.

The package carries `LICENSE` (the full LGPL text), `LICENSE.GPL` (the full GPL text, which the LGPL incorporates by reference, so a license file carrying only the LGPL text would be incomplete) and `THIRD-PARTY-NOTICES.txt`. Ship all of them onward. One packaging consequence for a consuming application: the assembly must not be merged into another assembly or shipped only as a trimmed or single-file artifact from which it cannot be replaced.

For the provenance and licensing of open source code included in this library, see [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.LilyScheme/blob/main/THIRD-PARTY-NOTICES.txt) in the repository.

---

**Where to go next**

- [CodeBrix.LilyPort](CodeBrix.LilyPort.md) - the music engraving engine that is this interpreter's principal consumer, and a worked example of embedding a large Scheme layer
- [CodeBrix.Platform.TclTk](CodeBrix.Platform.TclTk.md) - another embeddable language for .NET, with a widget toolkit on top
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.LilyScheme on GitHub](https://github.com/ellisnet/CodeBrix.LilyScheme) - source, tests and tools
