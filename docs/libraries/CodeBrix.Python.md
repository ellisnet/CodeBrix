<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Python</sub>

# CodeBrix.Python

**A cross-platform Python and .NET language-interoperability library: it embeds a CPython interpreter inside a .NET process, marshals values and objects across the Python and CLR boundary, and - through the embedded Python `clr` module - lets Python code load .NET assemblies and call .NET APIs.** There is no Python compiler here: the package drives a real CPython interpreter through its C API, so C extension modules work exactly as they do outside .NET. Any .NET 10 application can host it, including a CodeBrix.Platform application.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Python](https://github.com/ellisnet/CodeBrix.Python) |
| **Packages** | [`CodeBrix.Python.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Python.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later, and a CPython shared library already installed on the machine (a static-only build cannot be embedded, and the process bitness must match) |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Windows, macOS and Linux |

## What it does

- Embeds CPython in a .NET application and executes Python code from C#.
- Imports Python modules and calls Python functions, classes and objects from .NET.
- Marshals values and objects between the CPython and CLR type systems, with a codec mechanism for overriding or extending the mapping.
- Hosts the CLR from Python through the embedded `clr` module: load .NET assemblies and call .NET APIs from Python code.
- Lets Python classes derive from .NET classes and implement .NET interfaces, and lets Python members be exposed to .NET with explicit CLR signatures through `clr.clrproperty` and `clr.clrmethod`.
- Exposes the CPython buffer protocol through `PyObject.GetBuffer` and `PyBuffer`, so bulk data moves in blocks rather than element by element.
- Locates the CPython shared library through three routes, and reports the CPython version range it supports at run time - `PythonEngine.MinSupportedVersion`, `MaxSupportedVersion` and `IsSupportedVersion(version)` - rather than hard-coding it.
- Gives you the whole interop surface under the `CodeBrix.Python` namespace: engine lifecycle and the GIL, typed Python object wrappers, scopes, custom conversion codecs, .NET types subclassed from Python, and the finalizer queue.

## When to use it

Use it when a .NET application has to run real Python - a scientific stack, an existing analysis script, a plug-in written by someone who writes Python - or when a Python program needs to reach into .NET code you already own. It has no NuGet dependencies of its own; it calls into the CPython shared library directly.

Its boundaries are firm, and worth reading before you commit:

- It does not ship, install or download a CPython runtime. The shared library must already exist on the machine.
- It does not ship Python packages. Anything your Python code imports must be installed into the CPython installation or virtual environment that the embedded interpreter uses.
- It does not give you more than one interpreter per process. `PythonEngine`, `Runtime`, `PyObjectConversions` and `Finalizer` are process-global statics, `Shutdown()` is final for the process, and sub-interpreters are not exposed.
- It does not make Python run in parallel. The GIL serializes execution, and `BeginAllowThreads` yields the lock without removing it.
- It does not bridge `async`/`await` to Python coroutines or to `asyncio`. Run the event loop from Python code.
- It does not support application-domain unload or reload scenarios.
- It does not provide a Python-package distribution of the CLR loader; this package is the .NET-side embedding library only.
- It does not provide working binary state serialization on modern .NET. The formatter-based `RuntimeData` surface is retained for API parity and falls back to a no-op formatter unless you supply your own `IFormatter`.
- The assembly is not strong-named.

## Getting started

```bash
dotnet add package CodeBrix.Python.MitLicenseForever
```

The package ID carries the `.MitLicenseForever` suffix and the .NET namespace is plain `CodeBrix.Python`; the suffix never appears in code, and there is no package named plain `CodeBrix.Python`. You do not need `AllowUnsafeBlocks` in your own project - only this library compiles unsafe code.

```csharp
using CodeBrix.Python;          // Py, PythonEngine, Runtime, PyObject,
                                // PyModule, PyDict, PyList, PyTuple,
                                // PyInt, PyFloat, PyString, PyIter,
                                // PyIterable, PySequence, PyNumber,
                                // PyType, PyBuffer, PyBUF,
                                // BufferOrderStyle, PythonException,
                                // Finalizer, PyObjectConversions,
                                // IPyObjectEncoder, IPyObjectDecoder,
                                // InteropConfiguration, RuntimeData, ...

using CodeBrix.Python.Codecs;   // EncoderGroup, DecoderGroup,
                                // EncoderGroupExtensions,
                                // DecoderGroupExtensions, TupleCodec<T>,
                                // ListDecoder, SequenceDecoder,
                                // IterableDecoder, EnumPyIntCodec,
                                // RawProxyEncoder

using CodeBrix.Python.Serialization;  // JsonFormatter (rarely needed)
```

Almost all consumer work needs only the first using. The codec interfaces (`IPyObjectEncoder`, `IPyObjectDecoder`) and the registration entry point (`PyObjectConversions`) live in `CodeBrix.Python`, while the concrete built-in codecs live in `CodeBrix.Python.Codecs`, so writing a custom codec usually needs both.

Point the library at the CPython shared library before `PythonEngine.Initialize()`, then do everything Python under `Py.GIL()`.

```csharp
using System;
using CodeBrix.Python;

// Either assign the libpython path here, before Initialize()...
Runtime.PythonDLL = "/path/to/libpython3.XX.so";  // or .dll / .dylib
// ...or set the PYTHONNET_PYDLL environment variable for the process instead.

PythonEngine.Initialize();
try
{
    using (Py.GIL())
    {
        using PyObject sys = Py.Import("sys");
        Console.WriteLine(sys.GetAttr("version").As<string>());
    }
}
finally
{
    PythonEngine.Shutdown();
}
```

That is the whole contract: reference the package, make the shared library findable, initialize once, do everything else under `Py.GIL()`, and shut down once.

## Key concepts

### Finding the CPython shared library

The file is named `python3XX.dll` on Windows, `libpython3.XX.dylib` on macOS and `libpython3.XX.so` (sometimes with a `.1.0` suffix) on Linux, where `XX` is the CPython minor version installed on the machine. It must be a shared build, and the process bitness must match it.

Discovery tries three routes, and the first that yields a path wins:

1. `Runtime.PythonDLL`, assigned from code - the explicit, deterministic route.
2. The `PYTHONNET_PYDLL` environment variable.
3. Virtual-environment discovery: when `PYTHONNET_VENV` (preferred) or `VIRTUAL_ENV` names a directory containing a `pyvenv.cfg`, the `home` and `version` keys in that file locate the library next to the base interpreter, and the environment's own `bin/python` (`Scripts\python.exe` on Windows) becomes the program name. `PYTHONNET_PYEXE` overrides the program name independently.

Outside those cases you must set `Runtime.PythonDLL` yourself.

```csharp
public static string? Runtime.PythonDLL { get; set; }
public static int     Runtime.MainManagedThreadId { get; }
public static PyObject Runtime.None { get; }
public static bool    Runtime.TryCollectingGarbage(int runs)
```

`Runtime` is a public class, and `Runtime.PythonDLL` is the property consumers are expected to set; its other members are low-level interop and should be left alone. The property must be assigned before the engine is initialized - setting it afterwards throws `InvalidOperationException`.

### Engine lifecycle

Configuration that has to happen before initialization is all on `PythonEngine`:

```csharp
public static string ProgramName { get; set; }   // Py_SetProgramName
public static string PythonHome  { get; set; }   // Py_SetPythonHome
public static string PythonPath  { get; set; }   // Py_SetPath
public static void   SetNoSiteFlag()             // disables the site module
public static bool   DebugGIL { get; set; }      // default false
public static InteropConfiguration InteropConfiguration { get; set; }
```

`Initialize()` is idempotent - later calls return immediately - and it does not require the GIL. `Initialize(bool setSysArgv, bool initSigs)` and the overload taking an argument list publish `sys.argv` and, when `initSigs` is true, install CPython's default signal handlers, which takes signal handling away from your .NET application; leave it false unless you need Python's interrupt behavior. After initialization, `Version`, `BuildInfo`, `Platform`, `Copyright`, `Compiler` and `IsInitialized` are readable.

`PythonEngine` also implements `IDisposable` as a convenience wrapper: the constructor initializes and `Dispose()` shuts down. `AddShutdownHandler` and `RemoveShutdownHandler` register work to run at shutdown, in reverse registration order, so a handler sees the same resources that existed when it was added; do not add or remove handlers from inside a handler. `Shutdown()` throws `InvalidOperationException` if the Python error indicator is still set when it is called, and it resets `InteropConfiguration` back to the default.

`GetPythonThreadID()` and `Interrupt(pythonThreadID)` reach a running interpreter thread; `Interrupt` raises `KeyboardInterrupt` and returns the number of thread states it modified.

### The GIL

Every call that touches a Python object requires the CPython global interpreter lock.

```csharp
public static Py.GILState Py.GIL()

using (Py.GIL())
{
    // all Python work goes here
}
```

`Py.GILState` releases the lock in `Dispose()`. Its finalizer deliberately throws `InvalidOperationException` if the state is garbage collected without being disposed, so a dropped `using` becomes a hard failure rather than a silent leak. Setting `PythonEngine.DebugGIL = true` before acquiring makes `Py.GIL()` return a debug state that also throws when `Dispose()` runs on a different thread than the one that acquired the lock: turn it on while diagnosing threading problems.

Release the interpreter around long managed work so other threads can run:

```csharp
public static IntPtr BeginAllowThreads()          // Py_BEGIN_ALLOW_THREADS
public static void   EndAllowThreads(IntPtr ts)   // Py_END_ALLOW_THREADS

IntPtr ts = PythonEngine.BeginAllowThreads();
try { DoExpensiveManagedWork(); }
finally { PythonEngine.EndAllowThreads(ts); }
```

Four rules are not negotiable: initialize once, on one thread, before any other thread uses Python; acquire and release the GIL on the same thread, never passing a `GILState` between threads or storing it in a field; never `await` inside a `using (Py.GIL())` block; and a `PyObject` may be created on one thread and used on another only while that other thread holds the GIL.

### Executing Python code

There are three levels. `RunSimpleString(code)` runs in the `__main__` namespace, returns 0 or -1 and prints the traceback to standard error like the interactive interpreter instead of throwing - use it for fire-and-forget bootstrapping only.

```csharp
public static void     Exec(string code, PyDict? globals = null,
                                         PyObject? locals = null)
public static PyObject Eval(string code, PyDict? globals = null,
                                         PyObject? locals = null)
public static PyObject Compile(string code, string filename = "",
                               RunFlagType mode = RunFlagType.File)
```

`Exec` runs statements and `Eval` evaluates one expression and returns its value; both raise `PythonException` on a Python-level error. When `globals` is null the current Python frame's globals are used if there is one, and otherwise a throwaway dictionary seeded with `__builtins__` is created for that call alone - which is why you pass your own dictionary when you want to read results back.

```csharp
var locals = new PyDict();
PythonEngine.Exec("c = a + b", null, locals);
int c = locals.GetItem("c").As<int>();
```

`Compile` produces a reusable code object; `RunFlagType` is `{ Single = 256, File = 257, Eval = 258 }`. `Py.Import(name)` and `PyModule.Import(name)` import and return a module, `Py.SetArgv(...)` rewrites `sys.argv` after initialization, and `Py.With(obj, body)` drives a Python `with` block from C#, calling `__enter__` and `__exit__` correctly including exception suppression when `__exit__` returns a true value.

### Scopes

`PyModule : PyObject` is the scope abstraction: a real Python module object you can populate, execute code in and read variables out of, and it is the cleanest way to isolate one piece of Python work from another. Create one with `Py.CreateScope()`, `Py.CreateScope(name)` or `new PyModule(name)`, or build one from source with `PyModule.FromString(name, code)`.

Inside a scope, `Exec` is fluent and returns the module, `Eval` and `Eval<T>` evaluate an expression, and `Execute` and `Execute<T>` run a compiled code object. Variables come and go through `Set`, `Get`, `Get<T>`, `TryGet`, `TryGet<T>`, `Remove`, `Contains` and `Variables()`; `Set` converts the CLR value to Python automatically. `PyModule` overrides the dynamic member hooks, so a scope assigned to `dynamic` reads and writes its variables as properties. `Import(name, asname)` and `ImportAll(...)` are the `import` and `from X import *` equivalents scoped to that module.

### Python objects

`PyObject : DynamicObject, IDisposable, ISerializable` is the universal wrapper around one Python object reference, and everything else in the type system derives from it. Conversion runs through `FromManagedObject`, `AsManagedObject(Type)`, `As<T>()`, `GetPythonType()` and `TypeCheck(PyType)`; in the other direction the extension methods `ToPython()` and `ToPythonAs<T>()` convert CLR values, the first using the runtime type and the second forcing the static type when an interface or base type must drive overload resolution or codec selection. Both map null to Python `None`.

Attributes (`HasAttr`, `GetAttr`, `SetAttr`, `DelAttr`), items and indexers, `Length()`, `Invoke`, `InvokeMethod`, the predicates (`IsInstance`, `IsSubclass`, `IsCallable`, `IsIterable`, `IsTrue`, `IsNone`), `GetIterator()`, `Dir()`, `Repr()` and `Refcount` are all there. Keyword arguments come from `Py.kw`, which builds a keyword dictionary from alternating name and value pairs:

```csharp
public static Py.KeywordArguments Py.kw(params object?[] kv)

result = func.Invoke(new[] { 2.ToPython() }, Py.kw("a4", 8));
```

`==` and `Equals` compare by Python *value*, because they call the Python comparison protocol. To compare by object identity - Python's `is` - use `PythonReferenceComparer.Instance`, which is also the right comparer for a dictionary keyed on Python identity.

`Dispose()` decrements the reference count immediately. An undisposed wrapper is eventually collected by the finalizer queue, but disposal is deterministic and is what you want; using a disposed wrapper throws `ObjectDisposedException`. `PyObject.Handle` still exists but is obsolete - do not use raw handles.

The typed wrappers follow the Python type hierarchy:

```text
PyObject -> PyIterable -> PySequence -> PyList / PyTuple / PyString
PyObject -> PyIterable -> PyDict
PyObject -> PyNumber -> PyInt / PyFloat
PyObject -> PyIter / PyType / PyModule
```

Each has a `PyObject`-taking constructor that validates the type and a static `Is...Type(PyObject)` test.

### Conversion and codecs

Built-in conversion already handles the obvious cases in both directions: primitives, `string`, `bool`, arrays, delegates, and any CLR object, which becomes a Python proxy. Codecs override or extend that mapping.

```csharp
public interface IPyObjectEncoder                  // CLR -> Python
    bool      CanEncode(Type type);
    PyObject? TryEncode(object value);

public interface IPyObjectDecoder                  // Python -> CLR
    bool CanDecode(PyType objectType, Type targetType);
    bool TryDecode<T>(PyObject pyObj, out T? value);
```

Register with `PyObjectConversions.RegisterEncoder` and `RegisterDecoder`. Registration is process-wide and additive, the first registered codec that reports it can handle a conversion wins, so register the most specific codecs first, and there is no public unregister - scope codec registration to application start-up. `EncoderGroup` and `DecoderGroup` implement the single-codec interfaces themselves, so a group registers exactly like one codec, groups nest, and collection-initializer syntax works.

The built-in codecs in `CodeBrix.Python.Codecs` are each opt-in through a static `Register()` and expose a singleton `Instance`: `ListDecoder` (a Python `list` to `IList<T>`, as a live view rather than a copy), `SequenceDecoder` (any sequence to `ICollection<T>`), `IterableDecoder` (any iterable to `IEnumerable<T>`), `TupleCodec<TTuple>` (round-trips CLR and Python tuples), `EnumPyIntCodec` (CLR enums to and from Python `int`) and `RawProxyEncoder` (the base class for exposing a CLR object as a raw proxy, skipping all conversions). These decoders are lossless views, which is why they are deliberately narrow: `ListDecoder` will not decode to `List<int>`, because that would require copying. Lossy conversions belong in a codec of your own.

### Exposing .NET to Python

From the Python side, the embedded `clr` module is the door into the CLR, and it is always importable inside the embedded interpreter with no path setup. `clr.AddReference(name)` accepts a simple name, a full assembly name or a file path and raises `FileNotFoundException` when the assembly cannot be found; `clr.FindAssembly(name)`, `clr.ListAssemblies(verbose)`, `clr.GetClrType(t)`, `clr.getPreload()` and `clr.setPreload(flag)` complete the set - with preloading on, all assemblies on the path are scanned so their namespaces can be imported without an explicit `AddReference`.

Two decorators expose a Python class member to .NET with an explicit CLR signature: `clrproperty(type_)` takes the property type, and `clrmethod(return_type, arg_types, clrname=None)` takes the return type, a list of argument types and an optional .NET-side name. A `clrproperty` without a setter raises `AttributeError` on assignment.

```python
import clr
from System import String, Int32

class X(object):
    @clr.clrproperty(String)
    def test(self):
        return "x"

    @clr.clrmethod(Int32, [String])
    def compute(self, s):
        return len(s)
```

Python classes can also derive from .NET classes and implement .NET interfaces. Set `__namespace__` on the Python class so the generated .NET type gets a stable namespace. From the C# side, `[PyExport(false)]` hides a public type (or an entire assembly) from Python while leaving it public to CLR callers, and `[DocString("...")]` supplies the `__doc__` string Python sees for a type or member.

`IPythonBaseTypeProvider` and `InteropConfiguration.PythonBaseTypeProviders` control what Python sees a CLR type inherit from. `InteropConfiguration.MakeDefault()` returns the standard chain - the default provider plus the collection and dynamic-object mixin providers that make CLR collections behave like Python collections. The default provider requires the incoming base list to be empty, so it must stay first, and providers must be registered before the affected CLR type is first materialized into Python.

### Buffers

`PyObject.GetBuffer(PyBUF flags = PyBUF.SIMPLE)` exposes the CPython buffer protocol, which is how bulk bytes move without per-element marshalling. `PyBuffer : IDisposable` carries `Object`, `Length`, `ItemSize`, `Dimensions`, `ReadOnly`, `Buffer`, `Format`, `Shape`, `Strides` and `SubOffsets`, plus `IsContiguous`, `GetPointer`, `FromContiguous`, `ToContiguous`, `Write` and `Read`.

```csharp
using var buf = pythonArray.GetBuffer(PyBUF.WRITABLE);
byte[] managed = { (byte)' ' };
buf.Write(managed, 0, managed.Length, destinationOffset: 1);
```

Request `PyBUF.WRITABLE` before calling `Write`: a read-only exporter fails the request rather than silently giving you a read-only view. Always dispose the buffer, because the exporting object stays pinned until you do.

### Errors and object lifetime

Any Python-level error raised while executing Python code surfaces in C# as a `PythonException`, carrying `Type`, `Value`, `Traceback`, a `StackTrace` that spans Python and CLR frames, `Normalize()`, `Format()` and `Clone()`. Branch on `ex.Type.Name` or, better, register a decoder so a given Python exception type decodes into your own CLR exception type. `Format()` produces exactly what the CPython console would print, and it needs a running interpreter.

`InternalPythonnetException` means an invariant inside the interop layer was violated - that is a bug report. `FinalizationException` is raised on the finalizer path when a Python object cannot be released, and `RuntimeShutdownException` derives from it and means a `PyObject` outlived the interpreter run that created it. `InvalidOperationException` is what lifecycle misuse produces: using the engine before `Initialize()`, setting `Runtime.PythonDLL` after it, replacing `InteropConfiguration` while running, or releasing the GIL on the wrong thread with `DebugGIL` on.

Undisposed wrappers are queued and released in batches, because the release must happen under the GIL and the CLR finalizer thread does not hold it. `Finalizer.Instance` exposes `Threshold`, `Enable`, `Collect()`, a `BeforeCollect` event and an `ErrorHandler` event; set `Handled = true` in the error handler to swallow a release failure, which is the standard way to tolerate `RuntimeShutdownException` across an engine restart. `Collect()` must be called from a thread that is allowed to run Python code.

## Examples

Running a script file inside a scope and reading variables back out - the shape most host applications want, because nothing set in a scope leaks into `__main__` or into another scope.

```python
total = 0.0
for s in samples:
    total += s
mean = total / len(samples)
if mean > 100:
    warning = "mean is unusually high for " + label
```

```csharp
using System;
using System.IO;
using System.Linq;
using CodeBrix.Python;

public static class ScriptHost
{
    public static double RunAnalysis(string scriptPath, double[] samples)
    {
        using (Py.GIL())
        {
            // A scope is an isolated module namespace. Nothing set here
            // leaks into __main__ or into another scope.
            using PyModule scope = Py.CreateScope("analysis");

            using var pySamples = new PyList(
                samples.Select(v => (PyObject)new PyFloat(v)).ToArray());

            scope.Set("samples", pySamples);
            scope.Set("label", "run-1");        // CLR value, auto-converted

            // Exec is fluent, so several statements can be chained.
            scope.Exec(File.ReadAllText(scriptPath));

            // Read results back out, strongly typed.
            double mean = scope.Get<double>("mean");

            // Optional variables: TryGet does not throw when absent.
            if (scope.TryGet<string>("warning", out string warning))
            {
                Console.WriteLine("script warning: " + warning);
            }

            return mean;
        }
    }
}

// Re-running the same script many times? Compile it once instead:
//     PyObject code = PythonEngine.Compile(source, scriptPath,
//                                          RunFlagType.File);
//     scope.Execute(code);
```

A custom codec mapping a CLR `DateTime` to and from a Python `datetime.datetime`, and back.

```csharp
using System;
using CodeBrix.Python;
using CodeBrix.Python.Codecs;

public sealed class DateTimeCodec
    : IPyObjectEncoder, IPyObjectDecoder, IDisposable
{
    private readonly PyObject _pyDateTime;

    // Construct while holding the GIL - the constructor imports.
    public DateTimeCodec()
    {
        using PyObject module = Py.Import("datetime");
        _pyDateTime = module.GetAttr("datetime");
    }

    // ---- CLR -> Python -------------------------------------------
    public bool CanEncode(Type type) => type == typeof(DateTime);

    public PyObject TryEncode(object value)
    {
        var dt = (DateTime)value;
        return _pyDateTime.Invoke(
            dt.Year.ToPython(), dt.Month.ToPython(), dt.Day.ToPython(),
            dt.Hour.ToPython(), dt.Minute.ToPython(), dt.Second.ToPython());
    }

    // ---- Python -> CLR -------------------------------------------
    // Compare the SOURCE type by identity, never by name: two different
    // classes can share a __name__.
    public bool CanDecode(PyType objectType, Type targetType)
        => targetType == typeof(DateTime)
           && PythonReferenceComparer.Instance.Equals(
                  objectType, _pyDateTime);

    public bool TryDecode<T>(PyObject pyObj, out T value)
    {
        var dt = new DateTime(
            pyObj.GetAttr("year").As<int>(),
            pyObj.GetAttr("month").As<int>(),
            pyObj.GetAttr("day").As<int>(),
            pyObj.GetAttr("hour").As<int>(),
            pyObj.GetAttr("minute").As<int>(),
            pyObj.GetAttr("second").As<int>());
        value = (T)(object)dt;
        return true;
    }

    public void Dispose() => _pyDateTime.Dispose();
}
```

Register it once, at start-up, before any `DateTime` crosses the boundary - and remember that the built-in codecs are opt-in too.

```csharp
PythonEngine.Initialize();
using (Py.GIL())
{
    var codec = new DateTimeCodec();
    PyObjectConversions.RegisterEncoder(codec);
    PyObjectConversions.RegisterDecoder(codec);

    // Built-in codecs are opt-in too:
    TupleCodec<ValueTuple>.Register();
    ListDecoder.Register();
}
```

Grouping is equivalent and keeps the ordering explicit.

```csharp
var decoders = new DecoderGroup
{
    new DateTimeCodec(),      // most specific first
    ListDecoder.Instance,
    SequenceDecoder.Instance,
    IterableDecoder.Instance,
};
PyObjectConversions.RegisterDecoder(decoders);
```

A .NET class written to be consumed from Python, with a docstring and one type deliberately hidden.

```csharp
using CodeBrix.Python;

namespace Contoso.Reporting;

[DocString("A single line on a report.")]
public class LineItem
{
    public LineItem(string name, double amount)
    {
        Name = name;
        Amount = amount;
    }

    public string Name   { get; }
    public double Amount { get; }

    [DocString("Formats this item as one report row.")]
    public virtual string Render() => Name + ": " + Amount.ToString("0.00");
}

// Public in C#, but deliberately invisible from Python:
[PyExport(false)]
public class ReportInternals { }
```

Python then subclasses it, and the subclass is a `LineItem` on the .NET side: calling the virtual member from C# dispatches back into Python.

```csharp
PythonEngine.Initialize();
try
{
    using (Py.GIL())
    using (PyModule scope = Py.CreateScope("report"))
    {
        // A C# raw string literal ("""), so the Python keeps its own
        // indentation - a verbatim @"..." string would prefix every line
        // with the C# indentation and raise IndentationError.
        scope.Exec("""
            import clr
            clr.AddReference('Contoso.Reporting')   # assembly simple name
            from Contoso.Reporting import LineItem

            plain = LineItem('Widget', 9.99)
            plain_text = plain.Render()

            class DiscountedItem(LineItem):
                # Gives the generated .NET type a stable namespace.
                __namespace__ = 'Contoso.Reporting.Python'

                def Render(self):
                    return 'DISCOUNTED ' + LineItem.Render(self)

            item = DiscountedItem('Widget', 9.99)
            text = item.Render()
            doc  = LineItem.Render.__doc__
            """);

        Console.WriteLine(scope.Get<string>("plain_text"));  // Widget: 9.99
        Console.WriteLine(scope.Get<string>("text"));
        Console.WriteLine(scope.Get<string>("doc"));

        // The Python subclass IS a LineItem on the .NET side, and calling
        // the virtual member from C# dispatches back into Python.
        using PyObject pyItem = scope.Get("item");
        LineItem managed = pyItem.As<LineItem>();
        Console.WriteLine(managed.Render());   // DISCOUNTED Widget: 9.99
    }
}
finally
{
    PythonEngine.Shutdown();
}
```

## Pitfalls

- Forgetting the GIL. Every Python touch - including `PyObject.Dispose()`, `ToString()` and `Repr()` - needs `using (Py.GIL())`, and the symptoms range from wrong results to a hard process crash.
- Dropping the `using` on `Py.GIL()`. The state's finalizer throws when it is collected undisposed, so the failure surfaces far away from its cause.
- Releasing the GIL on a different thread than the one that acquired it. Set `PythonEngine.DebugGIL = true` during development to turn that into an immediate, precise exception.
- `await` inside a `using (Py.GIL())` block. The continuation may resume on another thread and release a lock it does not own; split the method so each GIL block is fully synchronous.
- Setting `Runtime.PythonDLL` too late. It must be assigned before `PythonEngine.Initialize()`, and the same applies to `PythonHome`, `PythonPath`, `ProgramName` and `SetNoSiteFlag()`.
- Expecting the shared library to be found automatically. Discovery covers only `PYTHONNET_PYDLL` and the virtual environments named by `PYTHONNET_VENV` or `VIRTUAL_ENV`; outside those cases you must set `Runtime.PythonDLL`.
- Assuming `Shutdown()` can be undone. The Python runtime can no longer be used in the process after it, so do not build a restart loop around it - keep one engine for the process lifetime. It also resets `InteropConfiguration`, discarding custom base-type providers.
- Letting `PyObject`s outlive the interpreter. Wrappers still alive at shutdown raise `RuntimeShutdownException` on the finalizer path; dispose them, or handle that exception in `Finalizer.Instance.ErrorHandler`.
- Using a disposed `PyObject`. `Dispose()` releases a *reference*, not the Python object - use `NewReference()` when you need an independently owned wrapper.
- Registering codecs too late. Encoder and decoder resolution is cached from the first conversion of a given type pair, so register everything at start-up, before any Python code runs.
- Comparing Python types by `Name`. `PyType.Name` is the raw type name and is not unique; compare by identity with `PythonReferenceComparer.Instance`.
- Confusing `==` with identity. `PyObject.operator ==` and `Equals` invoke the Python comparison protocol, which is value semantics; `PythonReferenceComparer` is the `is` operator.
- Calling `RunSimpleString` and expecting exceptions. It returns -1 and prints the traceback to standard error; use `Exec` or `Eval` when you need a `PythonException`.
- Reading `Exec` results from the wrong dictionary. With a null `globals` you have no handle on where the results landed; pass your own `PyDict`, or use a scope.
- Forgetting `__namespace__` on a Python class that derives from a .NET type. Without it the generated .NET type lands in an unpredictable namespace, and re-defining the class can collide.
- Registering an `IPythonBaseTypeProvider` after the affected CLR type has already been materialized into Python, or ahead of the default provider, which requires an empty base list.
- Expecting `RuntimeData` state serialization to work out of the box. The default factory falls back to a formatter that serializes nothing; supply `JsonFormatter` or your own `IFormatter`.
- Assuming `[PyExport(false)]` hides a type from .NET. It hides it only from Python.
- Adding a second Python interop assembly to the project. Only one may own the single embedded interpreter, and the namespaces here are `CodeBrix.Python` and its sub-namespaces.

For speed: hold the GIL once around a *batch* of Python work rather than per call, which is the single most common cause of slow embedding code; release it around long managed work; cache imported modules and callables in fields and dispose them in a shutdown handler; compile once and execute many; prefer the typed wrappers and `As<T>()` over `dynamic` in hot paths; move bulk data through `PyBuffer` rather than element by element; and keep CPU-bound parallelism on the .NET side with the GIL released.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Python/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Python/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Python/blob/main/README-INDEX.txt) |
| Non-package content in the repository | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Python/blob/main/EXTRAS-README.txt) |
| C# embedding tests (engine, GIL, scopes, wrappers, buffers, codecs, exceptions) | [tests/CodeBrix.Python.Tests](https://github.com/ellisnet/CodeBrix.Python/tree/main/tests/CodeBrix.Python.Tests) |
| .NET types written to be consumed from Python | [tests/CodeBrix.Python.TestSupport](https://github.com/ellisnet/CodeBrix.Python/tree/main/tests/CodeBrix.Python.TestSupport) |
| Python-side examples | [tests/CodeBrix.Python.PythonTests/pytests](https://github.com/ellisnet/CodeBrix.Python/tree/main/tests/CodeBrix.Python.PythonTests/pytests) |

The repository ships no sample or demo applications: the test projects are the runnable, verified usage. Two files are worth knowing about by name. `tests/CodeBrix.Python.Tests/PlatformPythonDll.cs` is a self-contained, copyable helper for locating the CPython shared library on Windows, macOS and Linux before `PythonEngine.Initialize()` runs, and it is useful outside the tests. `tests/CodeBrix.Python.TestSupport` is the best model for your own Python-facing .NET API, including `[DocString]` and subclassable types. XML documentation ships alongside the assembly.

## License

CodeBrix.Python is licensed under the MIT License, and the license is also named in the package ID (`CodeBrix.Python.MitLicenseForever`).

For the provenance and licensing of open source code included in this library, see [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Python/blob/main/THIRD-PARTY-NOTICES.txt) in the repository.

---

**Where to go next**

- [CodeBrix.LilyScheme](CodeBrix.LilyScheme.md) - a language runtime that is fully managed, when a machine-installed interpreter is not an option
- [CodeBrix.Platform.TclTk](CodeBrix.Platform.TclTk.md) - another embeddable scripting language, with a widget toolkit on top
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.Python on GitHub](https://github.com/ellisnet/CodeBrix.Python) - source and tests
