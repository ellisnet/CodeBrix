<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.AssemblyTools</sub>

# CodeBrix.AssemblyTools

**CodeBrix.AssemblyTools opens a compiled .NET assembly, lets you inspect or change anything inside it,
and writes the result back out.** It gives you an object model over every part of a managed PE file -
the assembly and module manifests, types, methods, fields, properties, events, generic parameters,
custom attributes, security declarations, P/Invoke and marshalling metadata, embedded and linked
resources, forwarded types, the IL of every method body, and the debug symbols that describe that IL -
and it builds a brand-new assembly from nothing through the same model. Everything is managed code with no
native dependency, so it runs wherever .NET 10 runs: in a build task, a code generator, a
command-line rewriter, or any other .NET 10 application, including a CodeBrix.Platform application.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.AssemblyTools](https://github.com/ellisnet/CodeBrix.AssemblyTools) |
| **Packages** | [`CodeBrix.AssemblyTools.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.AssemblyTools.MitLicenseForever) |
| **License** | MIT License; see [License](#license) |
| **Requires** | .NET 10 or later; no NuGet dependencies of its own |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Linux, macOS and Windows; writing a native Windows PDB and signing through a key container are Windows-only at run time |

## What it does

- Reads and writes managed assemblies and modules through `AssemblyDefinition` and `ModuleDefinition`,
  from a file path or from a stream
- Inspects and modifies types, methods, fields, properties, events, generic parameters and custom
  attributes as a live, mutable object graph
- Reads and emits IL through `MethodBody` and `ILProcessor`, with exception handlers, locals and
  branch targets kept as object references rather than offsets
- Reads and writes portable PDB and embedded portable PDB debug symbols
- Reads native Windows PDB symbols on any operating system, and writes them on Windows
- Reads and writes Mono MDB debug symbols
- Builds a complete assembly from nothing with `AssemblyDefinition.CreateAssembly` and
  `ModuleDefinition.CreateModule`
- Resolves a reference to its definition across assemblies through a pluggable `IAssemblyResolver` and
  `IMetadataResolver` pair
- Imports references from `System.Reflection` types or from another module, preserving required and
  optional custom modifiers (`in`, `ref readonly`, `init`, `required`, unmanaged constraints)
- Rewrites a file in place, or writes to a new path or a stream
- Signs on write with a strong name (`StrongNameKeyBlob`, `StrongNameKeyContainer`,
  `StrongNameKeyPair`) and can produce a deterministic MVID
- Ships extension-method "rocks" for the everyday tasks: `GetAllTypes`, `SimplifyMacros`,
  `OptimizeMacros`, `GetConstructors`, `MakeGenericInstanceType`, `GetDocCommentId` and more
- Exposes `GenericParameter.AllowByRefLikeConstraint` for the `where T : allows ref struct` flag
- Round-trips type-level custom debug information, such as NullableContext entries on
  compiler-generated types
- Ships XML documentation (IntelliSense) alongside the assembly

## When to use it

Reach for CodeBrix.AssemblyTools when a program has to treat a compiled assembly as data: instrument
method bodies with logging or timing calls, strip or rewrite attributes, retarget references, generate
a shim assembly, read IL to build a dependency graph or an API report, or convert debug symbols from
one format to another. The whole model is one merged assembly with no NuGet dependencies, so a tool
that uses it stays a single package reference.

The package is a file-format library, and it is deliberate about what it leaves alone:

- It does not load or execute code. Nothing here touches `AssemblyLoadContext`, `Reflection.Emit` or
  the JIT. To run what you wrote, use the normal .NET host.
- It does not verify IL. You can write an unbalanced stack, a branch to a removed instruction or a
  wrong-typed operand and get a file that fails at run time. `MaxStackSize` is computed for you;
  correctness is not.
- It does not decompile to C#, and it does not assemble from IL text. `Instruction.ToString()` gives a
  readable listing only.
- It does not generate host files: no apphost `.exe`, no `runtimeconfig.json`, no `deps.json`, no
  NuGet packaging.
- It does not merge assemblies or copy members between modules for you; you build new definitions and
  import every referenced type.
- It does not write mixed-mode (native plus managed) images, and it does not write native Windows PDBs
  on non-Windows hosts.
- It does not model Windows Runtime (`.winmd`) projections unless `ApplyWindowsRuntimeProjections` is
  true, and it does not author `.winmd` files.
- It does not sign with certificates; only strong-name signing is supported.
- It is not thread-safe: one module per thread, or external locking.

## Getting started

```bash
dotnet add package CodeBrix.AssemblyTools.MitLicenseForever
```

The NuGet package ID and the namespace are different - there is no package named plain
`CodeBrix.AssemblyTools`. The package ID is `CodeBrix.AssemblyTools.MitLicenseForever`; the assembly
and the primary namespace are `CodeBrix.AssemblyTools`.

Open an assembly, change something, write it back - the shape of every rewriting tool:

```csharp
using CodeBrix.AssemblyTools;

using var assembly = AssemblyDefinition.ReadAssembly("Input.dll");
assembly.Name.Name = "Renamed";
assembly.Write("Output.dll");
```

`ReadAssembly` holds the input file open until the `AssemblyDefinition` is disposed, which is why the
`using` matters, and why the output path here is a different file.

Reading IL is one more `using` and two more loops:

```csharp
using CodeBrix.AssemblyTools;
using CodeBrix.AssemblyTools.Cil;

using var module = ModuleDefinition.ReadModule("Input.dll");
foreach (var type in module.Types)
    foreach (var method in type.Methods)
        if (method.HasBody)
            foreach (Instruction instruction in method.Body.Instructions)
                Console.WriteLine($"{method.FullName}: {instruction}");
```

These are the namespaces, with the source's own notes on what each one holds:

```csharp
using CodeBrix.AssemblyTools;                     // assemblies, modules, the
                                                  // metadata model, resolvers,
                                                  // Reader/WriterParameters
using CodeBrix.AssemblyTools.Cil;                 // MethodBody, ILProcessor,
                                                  // Instruction, OpCodes,
                                                  // debug-info model, symbol
                                                  // reader/writer providers
using CodeBrix.AssemblyTools.Metadata;            // MetadataToken, TokenType
using CodeBrix.AssemblyTools.Collections.Generic; // Collection<T> (the list
                                                  // type used by every
                                                  // "Xxxs" property)
using CodeBrix.AssemblyTools.Rocks;               // extension methods:
                                                  // GetAllTypes, SimplifyMacros,
                                                  // MakeGenericInstanceType...
using CodeBrix.AssemblyTools.Pdb;                 // native-PDB providers
using CodeBrix.AssemblyTools.Mdb;                 // Mono MDB providers
using CodeBrix.AssemblyTools.Mdb.SymbolWriter;    // low-level MDB file model
                                                  // (rarely needed)
```

`CodeBrix.AssemblyTools.Collections.Generic.Collection<T>` implements `IList<T>`, so LINQ works on
every collection property in the model.

> [!NOTE]
> The namespaces `CodeBrix.AssemblyTools.PE`, `.Security.Cryptography`, `.Internal` and `.Pdb.Cci`
> exist in the assembly but contain no public types; do not add usings for them. The native-PDB
> providers consumers use live in `CodeBrix.AssemblyTools.Pdb`.

## Key concepts

### Reading an assembly or a module

An `AssemblyDefinition` owns one or more `ModuleDefinition`s; in practice nearly everything lives on
`MainModule`. Both types are entry points, and both are `IDisposable`:

```csharp
// CodeBrix.AssemblyTools.AssemblyDefinition
public static AssemblyDefinition ReadAssembly (string fileName)
public static AssemblyDefinition ReadAssembly (string fileName, ReaderParameters parameters)
public static AssemblyDefinition ReadAssembly (Stream stream)
public static AssemblyDefinition ReadAssembly (Stream stream, ReaderParameters parameters)

// CodeBrix.AssemblyTools.ModuleDefinition
public static ModuleDefinition ReadModule (string fileName)
public static ModuleDefinition ReadModule (string fileName, ReaderParameters parameters)
public static ModuleDefinition ReadModule (Stream stream)
public static ModuleDefinition ReadModule (Stream stream, ReaderParameters parameters)
```

The `fileName` overloads open the file with read access and keep it open until you dispose the module
or assembly; with `InMemory = true` the file is copied into a `MemoryStream` and closed immediately.
The `Stream` overloads do not take ownership of the stream, which must be readable and seekable, and
which you dispose. `ReadAssembly` throws `ArgumentException` when the file is a netmodule - use
`ReadModule` for those, and check `module.Assembly`.

`GetType(string)` returns null when the type is not defined in this module, and does not follow type
forwarders or references. Nested types use the slash form, `"Ns.Outer/Nested"`. `module.Types` is
top-level only; `GetTypes()` returns every type including nested ones, depth-first.

### ReaderParameters and the two reading modes

```csharp
public enum ReadingMode { Immediate = 1, Deferred = 2 }

public sealed class ReaderParameters {
    public ReaderParameters ()                         // ReadingMode.Deferred
    public ReaderParameters (ReadingMode readingMode)
    public ReadingMode ReadingMode {get;set;}
    public bool InMemory {get;set;}                    // copy file to memory, release
                                                       //   the file handle at once
    public bool ReadWrite {get;set;}                   // open the file for writing so
                                                       //   Write() can save in place
    public bool ReadSymbols {get;set;}                 // use DefaultSymbolReaderProvider
                                                       //   when SymbolReaderProvider is null
    public ISymbolReaderProvider SymbolReaderProvider {get;set;}
    public Stream SymbolStream {get;set;}              // explicit symbol stream instead
                                                       //   of a sidecar file
    public bool ThrowIfSymbolsAreNotMatching {get;set;}  // default TRUE
    public IAssemblyResolver AssemblyResolver {get;set;}
    public IMetadataResolver MetadataResolver {get;set;}
    public IMetadataImporterProvider MetadataImporterProvider {get;set;}
    public IReflectionImporterProvider ReflectionImporterProvider {get;set;}
    public bool ApplyWindowsRuntimeProjections {get;set;}  // for .winmd files
}
```

Deferred is the default: the manifest is read, and types, members, bodies, attributes and symbols are
materialized lazily on first access, each holding a reference to the still-open image. It is the
fastest mode when you touch a small part of a large assembly. Immediate decodes every table up front
and releases the internal metadata caches; use it when you will visit everything anyway, or when you
must dispose the input stream early. `Write()` on a deferred module first performs the equivalent of
an immediate read, so a read-tweak-write pipeline still reads the whole image once.

Symbol reading is triggered when either `ReadSymbols` is true or `SymbolReaderProvider` is not null.

### Assembly resolution

Resolution turns an `AssemblyNameReference` into a loaded `AssemblyDefinition`, and every `Resolve()`
call on a reference whose scope is another assembly goes through it. `BaseAssemblyResolver.Resolve`
looks in each search directory in order - the initial list is `.` and `bin`, relative to the current
working directory, not to the assembly being rewritten - trying `<Name>.dll` before `<Name>.exe`, so a
modern app layout with an unmanaged `Foo.exe` apphost beside the managed `Foo.dll` resolves correctly.
Failing that it uses the trusted-platform-assembly list of the running process, then raises the
`ResolveFailure` event, then throws `AssemblyResolutionException`.

```csharp
var resolver = new DefaultAssemblyResolver ();
resolver.AddSearchDirectory (Path.GetDirectoryName (inputPath));
resolver.AddSearchDirectory ("/path/to/target/framework/ref/pack");
var rp = new ReaderParameters { AssemblyResolver = resolver };
using var asm = AssemblyDefinition.ReadAssembly (inputPath, rp);
```

`DefaultAssemblyResolver` caches every result by full name and disposes all cached assemblies when it
is disposed. A module disposes the resolver only if it created the resolver itself; a resolver you
passed in through `ReaderParameters` is yours to dispose.

### From reference to definition

"Reference" classes describe a member as seen from some module, possibly another assembly's member;
"Definition" classes are the actual declaration and derive from the corresponding reference class.
`Resolve()` walks from one to the other: `TypeReference.Resolve()` yields a `TypeDefinition`,
`MethodReference.Resolve()` a `MethodDefinition`, `FieldReference.Resolve()` a `FieldDefinition`,
`MemberReference.Resolve()` an `IMemberDefinition`, and `ExportedType.Resolve()` a `TypeDefinition`
by following the forwarder.

Resolution uses `Module.MetadataResolver`, which uses `Module.AssemblyResolver` to open the assembly
named by the reference's scope and then looks the member up by name and signature. It returns null
when the assembly was opened but the member is not in it - version skew, a trimmed assembly, or a
wrong search directory that picked up an unrelated file of the same name - and it throws
`AssemblyResolutionException` when the assembly itself cannot be located. A `GenericParameter`
resolves to null; a `TypeSpecification` such as an array, pointer or generic instance resolves to the
definition of its element type. The returned definitions belong to assemblies cached inside the
resolver, so the same resolver instance has to stay alive for as long as you use them.

### Importing into a module

Every `TypeReference`, `MethodReference` and `FieldReference` stored in a module's metadata must
belong to that module. A reference obtained from another module, or from `System.Reflection`, must be
imported first, and the writer enforces it: writing a module that contains a foreign member throws
`ArgumentException` with the message "Member '...' is declared in another module and needs to be
imported".

```csharp
TypeReference   tr = mod.ImportReference (typeof (Console));
MethodReference mr = mod.ImportReference (
    typeof (Console).GetMethod ("WriteLine", new [] { typeof (string) }));
TypeReference   td = mod.ImportReference (otherModuleTypeDef);
```

There are overloads for the reflection side (`Type`, `FieldInfo`, `MethodBase`) and for the model side
(`TypeReference`, `FieldReference`, `MethodReference`), each with an optional
`IGenericParameterProvider context`. Importing a reference that already belongs to the target module
returns it unchanged, so importing is idempotent and cheap to do defensively. Importing a
`TypeDefinition` yields a `TypeReference`; the definition itself is not copied.

The reflection overloads name the assembly the type lives in at run time in your tool's process. That
is correct when the target runs on the same runtime; for a different target, import from a
`TypeReference` resolved through your assembly resolver, or use `module.TypeSystem.*` for primitives -
`module.TypeSystem.Int32` and friends are already imported and always correct for the module's own core
library.

### The IL model and ILProcessor

`MethodBody` exposes `MaxStackSize` (recomputed by the writer), `CodeSize`, `InitLocals`,
`LocalVarToken`, `Instructions`, `ExceptionHandlers`, `Variables`, `ThisParameter` and
`GetILProcessor()`. `MethodDefinition.HasBody` is false for abstract, P/Invoke, internalcall, native
and runtime methods; `Body` and `DebugInformation` are created on first access.

Always mutate instructions through an `ILProcessor`: it keeps `Instruction.Previous` and `Next`
linked and, when a symbol reader is attached, fixes up the debug scopes that refer to the instructions
you move. The operand type must match the opcode's `OperandType` - `OpCodes.Ldstr` takes a string,
`OpCodes.Call` a `MethodReference`, `OpCodes.Ldc_I4` an int, `OpCodes.Ldc_I4_S` an sbyte,
`OpCodes.Br_S` an `Instruction`, `OpCodes.Switch` an `Instruction[]` - and a mismatch is an
`ArgumentException` at `Create` time.

<details>
<summary>The complete ILProcessor surface: Create, Emit and the structural edits</summary>

```csharp
public sealed class ILProcessor {
    public MethodBody Body
    // Create: build an Instruction without adding it
    public Instruction Create (OpCode opcode)
    public Instruction Create (OpCode opcode, TypeReference type)
    public Instruction Create (OpCode opcode, MethodReference method)
    public Instruction Create (OpCode opcode, FieldReference field)
    public Instruction Create (OpCode opcode, CallSite site)
    public Instruction Create (OpCode opcode, string value)
    public Instruction Create (OpCode opcode, sbyte value)
    public Instruction Create (OpCode opcode, byte value)
    public Instruction Create (OpCode opcode, int value)
    public Instruction Create (OpCode opcode, long value)
    public Instruction Create (OpCode opcode, float value)
    public Instruction Create (OpCode opcode, double value)
    public Instruction Create (OpCode opcode, Instruction target)      // branches
    public Instruction Create (OpCode opcode, Instruction [] targets)  // switch
    public Instruction Create (OpCode opcode, VariableDefinition variable)
    public Instruction Create (OpCode opcode, ParameterDefinition parameter)
    // Emit: Create + Append, same overloads
    public void Emit (OpCode opcode)
    public void Emit (OpCode opcode, TypeReference type)
    public void Emit (OpCode opcode, MethodReference method)
    public void Emit (OpCode opcode, FieldReference field)
    public void Emit (OpCode opcode, CallSite site)
    public void Emit (OpCode opcode, string value)
    public void Emit (OpCode opcode, sbyte value)
    public void Emit (OpCode opcode, byte value)
    public void Emit (OpCode opcode, int value)
    public void Emit (OpCode opcode, long value)
    public void Emit (OpCode opcode, float value)
    public void Emit (OpCode opcode, double value)
    public void Emit (OpCode opcode, Instruction target)
    public void Emit (OpCode opcode, Instruction [] targets)
    public void Emit (OpCode opcode, VariableDefinition variable)
    public void Emit (OpCode opcode, ParameterDefinition parameter)
    // structural edits
    public void InsertBefore (Instruction target, Instruction instruction)
    public void InsertAfter (Instruction target, Instruction instruction)
    public void InsertAfter (int index, Instruction instruction)
    public void Append (Instruction instruction)
    public void Replace (Instruction target, Instruction instruction)
    public void Replace (int index, Instruction instruction)
    public void Remove (Instruction instruction)
    public void RemoveAt (int index)
    public void Clear ()
}
```

</details>

The safe rewriting pattern is `body.SimplifyMacros()`, then insert and remove, then
`body.OptimizeMacros()`. `SimplifyMacros` widens the short forms (`Br_S` becomes `Br`, `Ldloc_0`
becomes `Ldloc`, `Ldc_I4_S` becomes `Ldc_I4`); `OptimizeMacros` reverses that, and `Optimize()` adds
shrinking of int-range `Ldc_I8`. Short-form branches carry a one-byte signed offset, so inserting code
that pushes a target out of range without simplifying first produces an unverifiable method.

`ExceptionHandler` covers `Catch`, `Filter`, `Finally` and `Fault`, and both `TryEnd` and `HandlerEnd`
are exclusive - they name the first instruction after the block.

### Debug symbols

Symbol reading is provider-driven. `DefaultSymbolReaderProvider` probes, in order, an embedded
portable PDB inside the image, a `<assembly-name>.pdb` beside the file (portable or native, detected
by magic), and a `<assembly-file>.mdb` beside the file; when nothing is found it throws
`SymbolsNotFoundException`, or returns null when constructed with `throwIfNoSymbol: false`. The
explicit providers are `PortablePdbReaderProvider` and `EmbeddedPortablePdbReaderProvider` in
`CodeBrix.AssemblyTools.Cil`, `PdbReaderProvider` and `NativePdbReaderProvider` in
`CodeBrix.AssemblyTools.Pdb`, and `MdbReaderProvider` in `CodeBrix.AssemblyTools.Mdb`. Native PDB
reading is managed code and works on any operating system.

A PDB whose GUID and age do not match the image raises `SymbolsNotMatchingException` while
`ThrowIfSymbolsAreNotMatching` is true, which is the default; set it false to continue silently
without symbols.

The debug model hangs off `MethodDebugInformation`: `SequencePoints` (a `SequencePoint` is hidden when
`StartLine` is `0xfeefee`), `Scope`, `StateMachineKickOffMethod`, `GetSequencePoint`,
`GetSequencePointMapping`, `GetScopes` and `TryGetName`, with `Document` carrying `Url`, `Type`,
`HashAlgorithm`, `Language`, `Hash` and `EmbeddedSource`. Local variable names live only in the PDB,
in `Scope.Variables`, never in the IL: `VariableDefinition` has no `Name`, so to name a new local for
debuggers you add `new VariableDebugInformation(variable, "name")` to the enclosing scope.

### Writing, signing and determinism

```csharp
// AssemblyDefinition (forwards to MainModule) and ModuleDefinition
public void Write (string fileName)
public void Write (string fileName, WriterParameters parameters)
public void Write ()                                 // in place: requires the module
                                                     //   to have been read with
                                                     //   ReadWrite = true
public void Write (WriterParameters parameters)
public void Write (Stream stream)                    // stream must be writable AND
                                                     //   seekable; not disposed for you
public void Write (Stream stream, WriterParameters parameters)

public sealed class WriterParameters {
    public uint? Timestamp {get;set;}                // PE header timestamp; null
                                                     //   keeps the input's value
    public bool WriteSymbols {get;set;}              // use DefaultSymbolWriterProvider
                                                     //   when SymbolWriterProvider is null
    public ISymbolWriterProvider SymbolWriterProvider {get;set;}
    public Stream SymbolStream {get;set;}            // write the PDB here instead of
                                                     //   the sidecar path
    public bool DeterministicMvid {get;set;}         // MVID = hash of the output
    public byte [] StrongNameKeyBlob {get;set;}      // contents of a .snk (managed,
                                                     //   cross-platform)
    public string StrongNameKeyContainer {get;set;}  // CSP container (Windows)
    public System.Reflection.StrongNameKeyPair StrongNameKeyPair {get;set;}
    public bool HasStrongNameKey                     // any of the three is set
}
```

Write, in order: fully reads a deferred module; disposes the module's symbol reader; picks a symbol
writer; rebuilds all metadata tables and heaps from the object model, reassigning tokens; computes
`MaxStackSize` and instruction offsets; emits the PE; then optionally computes a deterministic MVID
and applies a strong-name signature. Because tokens are reassigned, `MetadataToken` values captured
before a write are not stable afterwards.

`DefaultSymbolWriterProvider` asks the reader which writer matches - portable to portable, embedded to
embedded, native to native, MDB to MDB - and therefore requires that symbols were read. To write
symbols for an assembly you did not read symbols for, name the provider explicitly:
`SymbolWriterProvider = new PortablePdbWriterProvider()`. `NativePdbWriterProvider` is Windows-only,
and its `Stream` overload is not implemented. Strong-name signing hashes with `SHA1.Create()`, which
is FIPS-compliant on FIPS-enforced hosts; prefer `StrongNameKeyBlob` (the bytes of a `.snk` file) over
`StrongNameKeyPair`, and note that `StrongNameKeyContainer` needs a Windows CSP key container.

Rewriting a file in place needs `ReadWrite` at read time and the parameterless `Write`:

```csharp
using (var module = ModuleDefinition.ReadModule (path, new ReaderParameters { ReadWrite = true })) {
    module.Assembly.Name.Version = new Version (2, 0, 0, 0);
    module.Write ();                                  // writes back to `path`
}
```

### Creating an assembly from nothing

`AssemblyDefinition.CreateAssembly(AssemblyNameDefinition, string moduleName, ModuleKind)` and
`ModuleDefinition.CreateModule(string, ModuleKind)` each have a `ModuleParameters` overload;
`ModuleParameters` carries `Kind`, `Runtime`, `Timestamp`, `Architecture`, `AssemblyResolver`,
`MetadataResolver`, `MetadataImporterProvider` and `ReflectionImporterProvider`.

A new module already contains the `<Module>` global type at `Types[0]`, so your first added type is
`Types[1]`. `CreateModule("Foo.dll", ...)` also creates the owning `AssemblyDefinition` named "Foo"
unless the kind is `NetModule`, and you set `assembly.EntryPoint` for the console and windows kinds. A
brand-new module's `TypeSystem.CoreLibrary` refers to the core library of the running runtime, and
reflection imports follow suit.

### Rocks: the extension methods

Add `using CodeBrix.AssemblyTools.Rocks;` for the everyday helpers:

```csharp
// ModuleDefinitionRocks
public static IEnumerable<TypeDefinition> GetAllTypes (this ModuleDefinition self)
    // every type incl. nested -- same result as GetTypes()
// TypeDefinitionRocks
public static IEnumerable<MethodDefinition> GetConstructors (this TypeDefinition self)
public static MethodDefinition GetStaticConstructor (this TypeDefinition self)
public static IEnumerable<MethodDefinition> GetMethods (this TypeDefinition self)
    // non-constructor methods
public static TypeReference GetEnumUnderlyingType (this TypeDefinition self)
// MethodDefinitionRocks
public static MethodDefinition GetBaseMethod (this MethodDefinition self)
    // the overridden method one level up (self if none)
public static MethodDefinition GetOriginalBaseMethod (this MethodDefinition self)
    // walks to the root of the override chain
// TypeReferenceRocks
public static ArrayType MakeArrayType (this TypeReference self)
public static ArrayType MakeArrayType (this TypeReference self, int rank)
public static PointerType MakePointerType (this TypeReference self)
public static ByReferenceType MakeByReferenceType (this TypeReference self)
public static OptionalModifierType MakeOptionalModifierType (this TypeReference self,
                                                             TypeReference modifierType)
public static RequiredModifierType MakeRequiredModifierType (this TypeReference self,
                                                             TypeReference modifierType)
public static GenericInstanceType MakeGenericInstanceType (this TypeReference self,
                                                           params TypeReference [] arguments)
public static PinnedType MakePinnedType (this TypeReference self)
public static SentinelType MakeSentinelType (this TypeReference self)
// ParameterReferenceRocks
public static int GetSequence (this ParameterReference self)
// DocCommentId
public static string GetDocCommentId (IMemberDefinition member)
    // "T:Ns.Type", "M:Ns.Type.Method(System.Int32)" -- the XML-doc id
```

`MethodBodyRocks` is where `SimplifyMacros`, `OptimizeMacros`, `Optimize` and
`Parse(MethodDefinition, IILVisitor)` live.

### Collections

Every `Xxxs` property in the model is a `Collection<T>` from
`CodeBrix.AssemblyTools.Collections.Generic`: an `IList<T>` that is live and mutable, so adding to
`type.Methods` edits the model and wires up `DeclaringType` and `Module`, and removing clears them.
Each collection has a matching `HasXxx` boolean that answers from the metadata row counts without
decoding anything, which makes `HasBody`, `HasCustomAttributes` and `HasNestedTypes` the cheap way to
test before touching a collection.

The collections are not thread-safe and are not snapshotted: never add to or remove from one while
`foreach`-ing over it - copy with `.ToList()` first.

## Examples

The canonical instrumentation pipeline: read with symbols, insert a call at the top of a method, and
write the assembly and its symbols back out.

```csharp
using System;
using System.IO;
using System.Linq;
using CodeBrix.AssemblyTools;
using CodeBrix.AssemblyTools.Cil;
using CodeBrix.AssemblyTools.Rocks;

static void Instrument (string inputPath, string outputPath)
{
    var resolver = new DefaultAssemblyResolver ();
    resolver.AddSearchDirectory (Path.GetDirectoryName (Path.GetFullPath (inputPath)));

    var readerParameters = new ReaderParameters {
        AssemblyResolver = resolver,
        ReadSymbols = true,                           // portable/embedded/native/mdb
        SymbolReaderProvider = new DefaultSymbolReaderProvider (throwIfNoSymbol: false),
    };

    using (resolver)
    using (var assembly = AssemblyDefinition.ReadAssembly (inputPath, readerParameters)) {
        var module = assembly.MainModule;

        var type = module.GetType ("MyApp.OrderService");   // null if absent
        var method = type.Methods.First (m => m.Name == "PlaceOrder" && m.HasBody);

        var writeLine = module.ImportReference (
            typeof (Console).GetMethod ("WriteLine", new [] { typeof (string) }));

        var body = method.Body;
        body.SimplifyMacros ();                       // widen short branches

        var il = body.GetILProcessor ();
        var first = body.Instructions [0];
        il.InsertBefore (first, il.Create (OpCodes.Ldstr, "Entering " + method.FullName));
        il.InsertBefore (first, il.Create (OpCodes.Call, writeLine));

        body.OptimizeMacros ();                       // re-shorten where legal

        var writerParameters = new WriterParameters {
            WriteSymbols = module.HasSymbols,         // only if symbols were read
        };
        assembly.Write (outputPath, writerParameters);
    }
}
```

Notice what `InsertBefore` preserves: existing exception handlers and sequence points that referenced
`first` now start at the injected `Ldstr`, because the target instruction stays in place and the new
one is linked before it, while branches to `first` still land on `first` - branch operands are
instruction references, not offsets.

Walking an entire assembly without holding the file open, by reading immediately and in memory:

```csharp
using var module = ModuleDefinition.ReadModule (path, new ReaderParameters {
    ReadingMode = ReadingMode.Immediate,
    InMemory = true,
});

foreach (var type in module.GetTypes ()) {            // includes nested types
    Console.WriteLine ($"{type.FullName}  base={type.BaseType?.FullName}");
    foreach (var field in type.Fields)
        Console.WriteLine ($"  {field.FieldType.FullName} {field.Name}" +
                           (field.HasConstant ? $" = {field.Constant}" : ""));
    foreach (var method in type.Methods) {
        Console.WriteLine ($"  {method.FullName}");
        if (!method.HasBody)
            continue;
        foreach (var instruction in method.Body.Instructions)
            Console.WriteLine ($"    {instruction}");
        foreach (var handler in method.Body.ExceptionHandlers)
            Console.WriteLine ($"    {handler.HandlerType} {handler.CatchType?.FullName}" +
                               $" try {handler.TryStart.Offset}-{handler.TryEnd.Offset}");
    }
}
```

Building a console assembly from nothing, with a portable PDB written beside it:

```csharp
var name = new AssemblyNameDefinition ("Hello", new Version (1, 0, 0, 0));
using var assembly = AssemblyDefinition.CreateAssembly (name, "Hello.dll", ModuleKind.Console);
var module = assembly.MainModule;

var program = new TypeDefinition ("Hello", "Program",
    TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract |
    TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
    module.TypeSystem.Object);
module.Types.Add (program);

var main = new MethodDefinition ("Main",
    MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
    module.TypeSystem.Void);
main.Parameters.Add (new ParameterDefinition ("args", ParameterAttributes.None,
    module.TypeSystem.String.MakeArrayType ()));      // Rocks
program.Methods.Add (main);

var il = main.Body.GetILProcessor ();                 // Body is created on demand
var counter = new VariableDefinition (module.TypeSystem.Int32);
main.Body.Variables.Add (counter);
main.Body.InitLocals = true;

var writeLine = module.ImportReference (
    typeof (Console).GetMethod ("WriteLine", new [] { typeof (string) }));

il.Emit (OpCodes.Ldstr, "Hello from CodeBrix.AssemblyTools");
il.Emit (OpCodes.Call, writeLine);
il.Emit (OpCodes.Ldc_I4, 42);
il.Emit (OpCodes.Stloc, counter);
il.Emit (OpCodes.Ret);

assembly.EntryPoint = main;
assembly.Write ("Hello.dll", new WriterParameters {
    SymbolWriterProvider = new PortablePdbWriterProvider (),   // explicit: nothing
});                                                            //   was read
```

To run it, copy a matching `Hello.runtimeconfig.json` next to it and use `dotnet Hello.dll`; this
package does not generate host files.

Wrapping an existing method body in a try/finally, which is the edit that most often goes wrong -
every `ret` has to become a `leave`, and the handler boundaries are exclusive:

```csharp
var body = method.Body;
body.SimplifyMacros ();
var il = body.GetILProcessor ();

// every `ret` must become `leave <after>`; collect them first
var rets = body.Instructions.Where (i => i.OpCode == OpCodes.Ret).ToList ();
var ret = il.Create (OpCodes.Ret);
var endFinally = il.Create (OpCodes.Endfinally);
il.Append (endFinally);
il.Append (ret);
foreach (var r in rets)
    il.Replace (r, il.Create (OpCodes.Leave, ret));   // Replace keeps branch
                                                      //   targets pointing at
                                                      //   the new instruction
var cleanup = module.ImportReference (typeof (Console).GetMethod ("Out").GetGetMethod ());
var flush = module.ImportReference (typeof (TextWriter).GetMethod ("Flush"));
il.InsertBefore (endFinally, il.Create (OpCodes.Call, cleanup));
il.InsertBefore (endFinally, il.Create (OpCodes.Callvirt, flush));

body.ExceptionHandlers.Add (new ExceptionHandler (ExceptionHandlerType.Finally) {
    TryStart = body.Instructions [0],
    TryEnd = body.Instructions.First (i => i.OpCode == OpCodes.Call && i.Operand == cleanup),
    HandlerStart = body.Instructions.First (i => i.OpCode == OpCodes.Call && i.Operand == cleanup),
    HandlerEnd = ret,
});
body.OptimizeMacros ();
```

Reading an existing custom attribute and adding new ones, on a type and on a method:

```csharp
var obsolete = type.CustomAttributes
    .FirstOrDefault (a => a.AttributeType.FullName == "System.ObsoleteAttribute");
if (obsolete != null && obsolete.HasConstructorArguments)
    Console.WriteLine ((string) obsolete.ConstructorArguments [0].Value);

var ctor = module.ImportReference (
    typeof (System.Runtime.CompilerServices.CompilerGeneratedAttribute).GetConstructor (Type.EmptyTypes));
type.CustomAttributes.Add (new CustomAttribute (ctor));

var descCtor = module.ImportReference (
    typeof (System.ComponentModel.DescriptionAttribute).GetConstructor (new [] { typeof (string) }));
var desc = new CustomAttribute (descCtor);
desc.ConstructorArguments.Add (new CustomAttributeArgument (module.TypeSystem.String, "rewritten"));
method.CustomAttributes.Add (desc);
```

Attributes are decoded lazily, one at a time, and the `AttributeType.FullName` check needs no
decoding - filter on it before you read `ConstructorArguments`. Enum-typed arguments arrive as the
underlying integer with `.Type` set to the enum type, and decoding them requires the enum's definition
to be resolvable.

<details>
<summary>Quick reference card: open, find, resolve, import, edit IL, build and write</summary>

```csharp
// open
using var asm = AssemblyDefinition.ReadAssembly (path);                    // deferred
using var asm = AssemblyDefinition.ReadAssembly (path, new ReaderParameters {
    ReadingMode = ReadingMode.Immediate, InMemory = true, ReadWrite = false,
    ReadSymbols = true, ThrowIfSymbolsAreNotMatching = false,
    AssemblyResolver = resolver });
using var mod = ModuleDefinition.ReadModule (path);                        // netmodules too
var mod = asm.MainModule;

// resolver
var resolver = new DefaultAssemblyResolver ();
resolver.AddSearchDirectory (dir);          resolver.ResolveFailure += (s, n) => null;

// find
TypeDefinition   t = mod.GetType ("Ns.Outer/Nested");   // null if absent
IEnumerable<TypeDefinition> all = mod.GetTypes ();       // incl. nested
MethodDefinition m = t.Methods.First (x => x.Name == "Foo");
FieldDefinition  f = t.Fields.First (x => x.Name == "_bar");
PropertyDefinition p = t.Properties.First (x => x.Name == "Baz");   // p.GetMethod / p.SetMethod
CustomAttribute  a = t.CustomAttributes.First (x => x.AttributeType.FullName == "...");

// reference -> definition
TypeDefinition   td = typeRef.Resolve ();     // null if missing; throws
MethodDefinition md = methodRef.Resolve ();   //   AssemblyResolutionException
                                              //   if the assembly is not found

// bring foreign things into this module
TypeReference   tr = mod.ImportReference (typeof (Console));
MethodReference mr = mod.ImportReference (typeof (Console).GetMethod ("WriteLine", new [] { typeof (string) }));
TypeReference   tr = mod.ImportReference (otherModuleTypeDef);
TypeReference   gi = mod.ImportReference (openType).MakeGenericInstanceType (mod.TypeSystem.Int32);  // Rocks

// primitives of THIS module
mod.TypeSystem.Void / Object / String / Int32 / Boolean / ... / CoreLibrary

// IL
MethodBody body = m.Body;                    // null when !m.HasBody
body.SimplifyMacros ();                      // Rocks
ILProcessor il = body.GetILProcessor ();
Instruction i = il.Create (OpCodes.Call, mr);
il.Emit (OpCodes.Ldstr, "x");   il.Append (i);   il.InsertBefore (target, i);
il.InsertAfter (target, i);     il.Replace (old, i);   il.Remove (i);   il.Clear ();
body.Variables.Add (new VariableDefinition (mod.TypeSystem.Int32));   body.InitLocals = true;
body.ExceptionHandlers.Add (new ExceptionHandler (ExceptionHandlerType.Catch) {
    TryStart = ..., TryEnd = ..., HandlerStart = ..., HandlerEnd = ..., CatchType = tr });
body.OptimizeMacros ();                      // Rocks
instr.OpCode.Code == Code.Call;  instr.Operand as MethodReference;  instr.Next / .Previous

// symbols
m.DebugInformation.SequencePoints            // SequencePoint: StartLine, Document.Url
m.DebugInformation.Scope.Variables           // VariableDebugInformation names
new ReaderParameters { SymbolReaderProvider = new PortablePdbReaderProvider () }
// Cil: DefaultSymbolReaderProvider, PortablePdbReaderProvider, EmbeddedPortablePdbReaderProvider
// Pdb: PdbReaderProvider, NativePdbReaderProvider   Mdb: MdbReaderProvider

// build new
var asm  = AssemblyDefinition.CreateAssembly (new AssemblyNameDefinition ("N", new Version (1,0,0,0)), "N.dll", ModuleKind.Dll);
var type = new TypeDefinition ("Ns", "T", TypeAttributes.Public | TypeAttributes.Class, mod.TypeSystem.Object);
var meth = new MethodDefinition ("M", MethodAttributes.Public | MethodAttributes.Static, mod.TypeSystem.Void);
var fld  = new FieldDefinition ("_f", FieldAttributes.Private, mod.TypeSystem.Int32);
var prop = new PropertyDefinition ("P", PropertyAttributes.None, mod.TypeSystem.Int32) { GetMethod = getter };
var evt  = new EventDefinition ("E", EventAttributes.None, handlerType) { AddMethod = add, RemoveMethod = remove };
var attr = new CustomAttribute (mod.ImportReference (ctorInfo));
attr.ConstructorArguments.Add (new CustomAttributeArgument (mod.TypeSystem.String, "v"));
mod.Types.Add (type);  type.Methods.Add (meth);  type.Fields.Add (fld);  asm.EntryPoint = meth;

// write
asm.Write (outPath);
asm.Write (outPath, new WriterParameters { WriteSymbols = true });                     // mirror what was read
asm.Write (outPath, new WriterParameters { SymbolWriterProvider = new PortablePdbWriterProvider () });
asm.Write (outPath, new WriterParameters { StrongNameKeyBlob = File.ReadAllBytes ("k.snk"), DeterministicMvid = true });
mod.Write ();                                // in place; needs ReadWrite = true at read time

// exceptions to expect
AssemblyResolutionException (FileNotFoundException)   ResolutionException
SymbolsNotFoundException (FileNotFoundException)      SymbolsNotMatchingException (InvalidOperationException)
ArgumentException "Member '...' is declared in another module and needs to be imported"
NotSupportedException "Writing mixed-mode assemblies is not supported"
```

</details>

## Using it in a CodeBrix.Platform application

There is nothing to register and nothing platform-specific: the library is managed code with no
dependencies of its own, and a project needs only the package reference.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>disable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.AssemblyTools.MitLicenseForever" />
  </ItemGroup>
</Project>
```

`<Nullable>disable</Nullable>` is only a suggestion - the package's API returns null in the documented
cases and carries no nullability annotations, so a project with nullable reference types enabled gets
no warnings and should null-check by hand.

Two operations are Windows-only at run time, and they are the only platform difference in the whole
library: writing a native Windows PDB, which goes through the COM symbol writer, and strong-name
signing with `WriterParameters.StrongNameKeyContainer`, which needs a Windows CSP key container.
Reading assemblies, IL and every symbol format is pure managed code and works on Linux, macOS and
Windows; `StrongNameKeyBlob` signing is cross-platform.

## Pitfalls

- The `ReadModule(string)` and `ReadAssembly(string)` overloads keep the input file open until you
  dispose the module. Wrap every module and assembly in a `using`, or read with `InMemory = true` if
  you must delete or overwrite the input while the model is alive.
- `Write(inputPath)` on a module that was opened read-only tries to recreate a file the module still
  holds open. Either write to a different path, or open with `new ReaderParameters { ReadWrite = true }`
  and call the parameterless `Write()` to save in place. `Write()` on a from-scratch module has no
  target - use `Write(path)`.
- Resolver search directories default to `.` and `bin` relative to the current working directory.
  Always add the folder of the assembly you are rewriting and the folders holding its references.
- The resolver's last fallback is the framework of the process running your tool, not the target's. A
  tool rewriting an assembly built against a different framework will silently resolve `System.Object`
  to the running runtime's core library unless the target's reference assemblies come first in the
  search list.
- `ImportReference(typeof(X))` references the running runtime's assembly for X. For other targets,
  import from a reference resolved from the target's own references, or use `module.TypeSystem.*`.
- Forgetting to import compiles fine and fails only at `Write` with "Member '...' is declared in
  another module and needs to be imported". A `Resolve()`d definition belongs to the other assembly:
  import everything foreign, always.
- `Resolve()` returns null when the member is missing from the located assembly and throws
  `AssemblyResolutionException` when the assembly is not found. Handle both.
- A stale `.pdb` beside the input throws `SymbolsNotMatchingException` by default. Setting
  `ThrowIfSymbolsAreNotMatching = false` proceeds without symbols - and then you must not set
  `WriteSymbols = true`, because there is no reader for the default writer provider to mirror.
- Native PDB writing is Windows-only. Reading native PDBs works everywhere; convert by reading native
  and writing with `PortablePdbWriterProvider`.
- Inserting instructions into a body that uses short forms can push a branch target out of the
  one-byte range. Call `body.SimplifyMacros()` before editing and `body.OptimizeMacros()` after.
- `Instruction.Offset` is meaningful only right after a read or a write; after edits it is stale until
  the next write. Compare instructions by reference, not by offset.
- `ExceptionHandler.TryEnd` and `HandlerEnd` are exclusive. Inserting before `TryEnd` puts the new
  instruction inside the protected region; inserting before `HandlerStart` puts it inside the try
  block, not the handler.
- Never mutate a `Collection<T>` while iterating it; copy with `.ToList()` first. Removing an
  instruction that is still a branch target or a handler boundary leaves a dangling reference that
  fails at write time or produces invalid IL.
- `MetadataToken` values are reassigned on write. Do not persist them across a write or use them as
  stable keys; use `FullName`.
- `module.Types` does not contain nested types. Use `GetType("Outer/Nested")` with the slash
  separator, or `GetTypes()`.
- `ReadAssembly` throws `ArgumentException` on a module without an assembly manifest; use `ReadModule`
  and check `module.Assembly`.
- Mixed-mode images read fine but cannot be written - `NotSupportedException`.
- Adding a property or an event does not add its accessor methods. Add the `MethodDefinition`s to
  `type.Methods` yourself and then assign `GetMethod`, `SetMethod`, `AddMethod` or `RemoveMethod`.
- The older `Import(...)` overloads still exist but are marked `[Obsolete]`; use `ImportReference`.
- The model is not thread-safe: one module per thread, or external locking.

For speed: keep `ReadingMode.Deferred` when you touch a fraction of the assembly and switch to
`Immediate` when you visit everything; test the `HasXxx` properties before touching a collection;
prefer `GetType("Ns.Name")`, a dictionary lookup, over a LINQ scan for a known name; share one
`DefaultAssemblyResolver` across every module you process, since it caches by full name; batch
`il.Create` with `InsertBefore`/`InsertAfter` rather than replacing one instruction at a time; and
write once at the end, because every write rebuilds every table.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.AssemblyTools/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.AssemblyTools/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.AssemblyTools/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.AssemblyTools.Tests](https://github.com/ellisnet/CodeBrix.AssemblyTools/tree/main/tests/CodeBrix.AssemblyTools.Tests) |
| Test fixtures (pre-built images, C# and IL sources) | [tests/CodeBrix.AssemblyTools.Tests/Resources](https://github.com/ellisnet/CodeBrix.AssemblyTools/tree/main/tests/CodeBrix.AssemblyTools.Tests/Resources) |

The repository holds exactly two projects - the packable library and its test project - and no
samples, demos, tools or build scripts. The test project is the worked-example corpus instead: if you
are looking for how to do something with this library, the test file named for it is the answer. The
fixtures split into `Resources/assemblies/` (pre-built images with their `.pdb` and `.mdb` sidecars),
`Resources/cs/` (C# sources compiled at test time) and `Resources/il/` (IL sources used as reader
inputs). The whole suite runs unconditionally on a headless host with no opt-in environment variables;
the only coverage that is naturally skipped is native-PDB writing, which the format supports on
Windows only.

<details>
<summary>Which test file demonstrates which feature</summary>

| Test file | What it demonstrates |
| --- | --- |
| `SmokeTests.cs` | CreateModule/CreateAssembly, TypeSystem, IL emission, MetadataToken, OpCodes, `Collection<T>` basics |
| `Core/ILProcessorTests.cs` | Create/Append/InsertBefore/InsertAfter/Replace/Remove/Clear; debug-scope fix-ups on edits |
| `Core/MethodBodyTests.cs` | Instructions, Variables, ExceptionHandlers, locals, SimplifyMacros/OptimizeMacros round trips |
| `Core/ModuleTests.cs` | ReadModule overloads, ReadingMode, in-place write, GetType/GetTypes, resources, exported types, MVID |
| `Core/AssemblyTests.cs` | ReadAssembly, Name/Version/PublicKey, symbol reading without a symbol file |
| `Core/ResolveTests.cs` | Resolve for types/methods/fields, a custom resolver subclass, search directories, resolution failures |
| `Core/ImportDefinitionTests.cs` | ImportReference of TypeReference/MethodReference/FieldReference, including generic contexts |
| `Core/ImportReflectionTests.cs` | ImportReference of Type/MethodBase/FieldInfo, including custom modifiers |
| `Core/CustomAttributesTests.cs` | ConstructorArguments, Fields, Properties, enums, arrays, boxed arguments, GetBlob |
| `Core/TypeTests.cs` | TypeDefinition and TypeReference, generics, arrays, pointers, modifiers, function pointers |
| `Core/NestedTypesTests.cs` | NestedTypes and the "Outer/Nested" name form |
| `Core/MethodTests.cs` | Attributes, Overrides, PInvokeInfo, generic methods, MethodReturnType |
| `Core/FieldTests.cs` | Constants, InitialValue and RVA, layout, MarshalInfo |
| `Core/PropertyTests.cs`, `Core/EventTests.cs`, `Core/ParameterTests.cs`, `Core/VariableTests.cs` | The remaining member kinds |
| `Core/SymbolTests.cs` | Portable, native and MDB providers, and the symbol-matching switches |
| `Core/PortablePdbTests.cs` | Sequence points, scopes, variable names, embedded PDB, deterministic MVID, source link, embedded source |
| `Core/SecurityDeclarationTests.cs` | SecurityDeclaration and SecurityAttribute |
| `Core/TypeParserTests.cs` | `GetType(fullName, runtimeName: true)` name parsing |
| `Core/TypeReferenceComparisonTests.cs`, `Core/MethodReferenceComparerTests.cs` | Reference comparison and comparers |
| `Core/ImageReadTests.cs` | PE header facts: Kind, Architecture, Runtime, Characteristics, debug header |
| `Pdb/PdbTests.cs` | Native PDB read and write round trips |
| `Mdb/MdbTests.cs` | Mono MDB read and write round trips |
| `Rocks/*.cs` | Every extension method in the Rocks namespace, including `GetDocCommentId` |

</details>

## License

CodeBrix.AssemblyTools is licensed under the MIT License; the license is also named in the package ID
(`CodeBrix.AssemblyTools.MitLicenseForever`). For the provenance and licensing of open source code
included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.AssemblyTools/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.ArgumentParser](CodeBrix.ArgumentParser.md) - the command-line surface most assembly-rewriting tools need
- [SilverAssertions](SilverAssertions.md) - assert on the assembly a rewriting step produced
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.AssemblyTools on GitHub](https://github.com/ellisnet/CodeBrix.AssemblyTools) - source and tests
