<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.TomlParse</sub>

# CodeBrix.TomlParse

**CodeBrix.TomlParse reads, parses, validates, updates and creates TOML 1.1 files at four levels: an
object serializer, an untyped document model, a lossless syntax tree, and an incremental lexer and
parser.** Most applications want the serializer - `TomlSerializer` maps TOML onto your own classes and
back, in the style of `System.Text.Json` - and drop to the document model for TOML of unknown shape, or to
the syntax tree to edit a file without disturbing a single comment or space. A source generator ships
inside the package for trimming- and NativeAOT-safe serialization. It is fully managed with no
dependencies beyond .NET. Use it from any .NET 10 application, or from a CodeBrix.Platform application.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.TomlParse](https://github.com/ellisnet/CodeBrix.TomlParse) |
| **Packages** | [`CodeBrix.TomlParse.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.TomlParse.BsdLicenseForever) |
| **License** | BSD 2-Clause; see [License](#license) |
| **Requires** | .NET 10 or later; no other dependencies. The source generator needs an up-to-date .NET 10 SDK |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Any operating system .NET 10 runs on - pure IL, trim- and NativeAOT-compatible |

## What it does

- Implements the TOML 1.1 specification, checked against a standard TOML conformance test corpus.
- Deserializes TOML into your classes and records and serializes them back with `TomlSerializer`, from
  and to a string, a UTF-8 `Stream`, a `TextReader` or a `TextWriter`, with a `TryDeserialize` form that
  returns false instead of throwing.
- Gives you an untyped, insertion-ordered document model - `TomlTable`, `TomlArray` and `TomlTableArray`
  in `CodeBrix.TomlParse.Model` - for reading and editing TOML of any shape.
- Parses a document into a lossless, round-trippable syntax tree that keeps every comment, blank line and
  whitespace character, for byte-exact surgical edits.
- Keeps comments across a read, modify and write cycle through a metadata store, for both the document
  model and your own classes.
- Applies naming policies (snake_case, camelCase, kebab-case), case-insensitive matching, and
  TOML-specific attributes alongside the `System.Text.Json` ones it also honours.
- Supports custom converters and converter factories, polymorphism with type discriminators, extension
  data and serialization callbacks.
- Generates serialization metadata at compile time for `[TomlSerializable]` contexts, with the generator
  inside the package.
- Reports errors with file name, line and column, and controls output formatting: inline tables, arrays
  of tables, string styles, key ordering, dotted keys, indentation and line endings.
- Offers an incremental, allocation-light lexer and event parser for tools that only scan documents.

## When to use it

Use CodeBrix.TomlParse whenever TOML is the format: application and tool configuration, project
manifests, data files. The decision that matters is which layer to start from.

```text
  * "Read a config file into my settings class"         -> TomlSerializer.Deserialize<T>
  * "Write my object out as TOML"                       -> TomlSerializer.Serialize
  * "Read TOML whose shape I do not know"               -> TomlSerializer.Deserialize<TomlTable>
  * "Change a value and keep the comments"              -> TomlTable + TomlMetadataStore
  * "Change a value and keep the file byte-for-byte"    -> SyntaxParser + the syntax tree
  * "Check a file for errors without mapping it"        -> SyntaxParser.Parse + Diagnostics
  * "Trimmed or NativeAOT application"                  -> a [TomlSerializable] context
  * "Scan tokens or parse events in a tool"             -> TomlLexer / TomlParser
```

What it does not do:

- It does not read or write JSON, YAML or INI. It uses `System.Text.Json` types - naming policies and
  attributes - only for familiarity. For YAML, see [CodeBrix.YamlParse](CodeBrix.YamlParse.md).
- It does not validate a document against a schema; it reports TOML syntax errors and type-mapping
  errors only.
- It does not watch files or reload configuration, and it is not a `Microsoft.Extensions.Configuration`
  provider.
- It does not support TOML versions before 1.1.
- The metadata store does not reproduce exact whitespace and blank lines - only the syntax tree is
  byte-exact.

## Getting started

```bash
dotnet add package CodeBrix.TomlParse.BsdLicenseForever
```

The package ID carries the license suffix; the namespaces do not. You install
`CodeBrix.TomlParse.BsdLicenseForever` and you write `using CodeBrix.TomlParse;`.

```csharp
using System;
using System.IO;
using CodeBrix.TomlParse;
using CodeBrix.TomlParse.Model;

var path = "settings.toml";
var options = new TomlSerializerOptions { SourceName = path };
var text = File.ReadAllText(path);
TomlTable doc = TomlSerializer.Deserialize<TomlTable>(text, options);

var title = (string)doc["title"];
var db = (TomlTable)doc["database"];
var port = (long)db["port"];                 // integers are long
var ports = (TomlArray)db["ports"];
Console.WriteLine($"{title}: {db["host"]}:{port} ({ports.Count} ports)");

db["port"] = 6543L;
db["enabled"] = true;
doc["owner"] = new TomlTable { ["name"] = "Ada" };
File.WriteAllText(path, TomlSerializer.Serialize(doc));   // comments dropped - see below
```

| Namespace | What it holds |
| --- | --- |
| `CodeBrix.TomlParse` | `TomlSerializer`, `TomlSerializerOptions`, `TomlException`, `TomlDateTime`, `TomlTypeInfo<T>` and the option enums |
| `CodeBrix.TomlParse.Model` | `TomlTable`, `TomlArray`, `TomlTableArray`, `TomlObject` and the metadata types |
| `CodeBrix.TomlParse.Serialization` | The attributes, `TomlConverter<T>`, `TomlSerializerContext`, `TomlMetadataStore`, `TomlReader`, `TomlWriter` and the callbacks |
| `CodeBrix.TomlParse.Parsing` | `SyntaxParser`, `TomlLexer`, `TomlParser` |
| `CodeBrix.TomlParse.Syntax` | `DocumentSyntax` and every syntax node |
| `CodeBrix.TomlParse.Text` | `TomlSourceSpan`, `TomlTextPosition` |
| `CodeBrix.TomlParse.Helpers` | `TomlFormatHelper`, for formatting TOML literals |

Typed code usually also needs `using System.Text.Json;` (for `JsonNamingPolicy`) and
`using System.Text.Json.Serialization;` (for `JsonKnownNamingPolicy` and the shared attributes).

## Key concepts

### The serializer

Every `TomlSerializer` method exists for string, `Stream` and `TextReader` input, and for string,
`Stream` and `TextWriter` output, in three flavours: with `TomlSerializerOptions` (reflection), with a
`TomlSerializerContext`, or with a `TomlTypeInfo<T>` (source-generated).

```csharp
T      Deserialize<T>(string toml, TomlSerializerOptions options = null)
T      Deserialize<T>(string toml, TomlSerializerContext context)
T      Deserialize<T>(string toml, TomlTypeInfo<T> typeInfo)
bool   TryDeserialize<T>(string toml, out T value, TomlSerializerOptions options = null)
string Serialize<T>(T value, TomlSerializerOptions options = null)
string Serialize<T>(T value, TomlTypeInfo<T> typeInfo)
void   Serialize<T>(Stream stream, T value, ...)
void   Serialize<T>(TextWriter writer, T value, ...)
```

`Deserialize<TomlTable>` gives the untyped document model, and serializing a `TomlTable` writes it back.

### Options

`TomlSerializerOptions` is an immutable record with init-only properties, so derive variants with
`options with { ... }` and share one instance rather than building options per call. The ones that
matter most:

| Property | Default | What it does |
| --- | --- | --- |
| `PropertyNamingPolicy` | null | CLR names as written, matched case-sensitively; set `JsonNamingPolicy.SnakeCaseLower`, `CamelCase` or `KebabCaseLower` |
| `PropertyNameCaseInsensitive` | false | case-insensitive key matching on read |
| `DefaultIgnoreCondition` | `WhenWritingNull` | which members are left out when writing |
| `DuplicateKeyHandling` | `Error` | duplicate keys are an error |
| `DottedKeyHandling` | `Literal` | or `Expand` |
| `RootValueHandling` | `Error` | a non-table root cannot be serialized unless set to `WrapInRootKey` |
| `WriteIndented`, `IndentSize`, `NewLine` | true, 2, `Lf` | output layout |
| `InlineTablePolicy`, `TableArrayStyle`, `StringStylePreferences` | `Never`, `Headers` | output style |
| `MetadataStore` | null | captures comments on read and replays them on write |
| `SourceName` | null | the file name used in error messages |
| `Converters`, `PolymorphismOptions`, `MaxDepth` | | custom converters, discriminators, nesting limit (0 means 64) |

### The document model

`TomlTable` is an insertion-ordered `IDictionary<string, object>`; `new TomlTable(inline: true)` is an
inline `{ ... }` table. `TomlArray` is an `IList<object>` and `TomlTableArray` an `IList<TomlTable>` for
`[[servers]]`. Scalars are plain .NET values: a TOML string is `string`, an integer is always `long`, a
float is `double`, a boolean is `bool`, and a date or time is a `TomlDateTime`, whose `Kind` is
`OffsetDateTimeByZ`, `OffsetDateTimeByNumber`, `LocalDateTime`, `LocalDate` or `LocalTime`.

### Attributes

Member-level: `[TomlPropertyName]`, `[TomlIgnore]`, `[TomlInclude]`, `[TomlPropertyOrder]`,
`[TomlRequired]`, `[TomlExtensionData]`, `[TomlSingleOrArray]` (accepts `x = "a"` or `x = ["a", "b"]`),
`[TomlTableArrayStyle]`, `[TomlInlineTable]`, `[TomlStringStyle]` and `[TomlConverter]`. Type-level:
`[TomlConstructor]`, `[TomlMappingOrder]`, `[TomlDottedKeyHandling]`, and `[TomlPolymorphic]` with
`[TomlDerivedType]`. The matching `System.Text.Json` attributes - `[JsonPropertyName]`, `[JsonIgnore]`,
`[JsonPolymorphic]` and the rest - work too, and the TOML-specific one wins when both are present.

### Converters and callbacks

Derive from `TomlConverter<T>` and override `Read(TomlReader reader)` and
`Write(TomlWriter writer, T value)`; `TomlConverterFactory` handles open generic and runtime-inspected
types. Register a converter through `TomlSerializerOptions.Converters`, through `[TomlConverter]`
(reflection only), or through `[TomlSourceGenerationOptions(Converters = [...])]` for a generated
context. Implement `ITomlOnDeserialized` (and its three siblings) on your own type to validate after
reading.

### The source generator and the build integration

The package carries two things beside the library itself, and neither needs any setup:

- **A Roslyn source generator**, packed under `analyzers/dotnet/cs`, which the compiler loads
  automatically. It emits serialization metadata for a `partial` class that derives from
  `TomlSerializerContext` and carries `[TomlSerializable(typeof(Root))]`: a `Default` instance and one
  `TomlTypeInfo<T>` property per root type and per type reachable from it, named after the type. That is
  the trimming- and NativeAOT-safe path for typed serialization. Its diagnostics are `TOMLPARSE001` to
  `TOMLPARSE011`. The generator adds no runtime dependency.
- **A build `.targets` file**, `buildTransitive/CodeBrix.TomlParse.BsdLicenseForever.targets`, which
  NuGet imports into every project that references the package, directly or transitively. It sets the
  `CodeBrix.TomlParse.TomlSerializer.IsReflectionEnabledByDefault` feature switch to false when the
  project publishes with `PublishAot=true`, so reflection-based serialization is off under NativeAOT, and
  passes the switch to the runtime configuration and the trimmer. Set the
  `TomlParseIsReflectionEnabledByDefault` MSBuild property yourself to override it either way.

The generator needs a C# compiler at least as new as the one it was built against, so use an up-to-date
.NET 10 SDK; on an older SDK the reflection-based and document-model APIs still work, but contexts are
not generated.

### The syntax tree

`SyntaxParser.Parse` is tolerant - errors go to `DocumentSyntax.Diagnostics` and `HasErrors` - while
`SyntaxParser.ParseStrict` throws `TomlException` on the first error. `ToString()` reproduces the source
text exactly, after any edits. A document exposes `KeyValues` (top-level `key = value` lines) and
`Tables` (`[table]` and `[[array]]` sections); each `KeyValueSyntax` has a `Key` and a `Value`, and each
scalar value node has a `Value` and a `Token` whose text is what `ToString()` writes. Comments and
whitespace hang off tokens as `LeadingTrivia` and `TrailingTrivia`.

### Errors

`TomlException` is thrown for syntax and mapping errors, and carries `SourceName`, `Line` and `Column`
(1-based), `Offset`, `Span` and a `Diagnostics` bag with every collected message. The message reads
`file.toml(line,col) : error : text`.

## Examples

Mapping TOML onto your own classes.

```csharp
using System.Collections.Generic;
using System.Text.Json;
using CodeBrix.TomlParse;

public sealed class ServerConfig
{
    public string ServerName { get; set; } = "";
    public int MaxConnections { get; set; } = 100;
    public List<Route> Routes { get; set; } = new();
}

public sealed class Route
{
    public string Path { get; set; } = "";
}

var options = new TomlSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
};
var cfg = TomlSerializer.Deserialize<ServerConfig>("""
    server_name = "alpha"
    max_connections = 10
    [[routes]]
    path = "/"
    [[routes]]
    path = "/api"
    """, options);
string toml = TomlSerializer.Serialize(cfg, options);
```

Updating a file while keeping its comments.

```csharp
using CodeBrix.TomlParse;
using CodeBrix.TomlParse.Model;
using CodeBrix.TomlParse.Serialization;

var store = new TomlMetadataStore();
var keepComments = new TomlSerializerOptions { MetadataStore = store };
var text = File.ReadAllText(path);
var table = TomlSerializer.Deserialize<TomlTable>(text, keepComments);
((TomlTable)table["database"])["port"] = 6543L;
File.WriteAllText(path, TomlSerializer.Serialize(table, keepComments));
// "# comment" lines and trailing "# comments" survive; blank lines may not.
```

A byte-exact edit with the syntax tree: replace the token, and carry its trivia across.

```csharp
using System.Linq;
using CodeBrix.TomlParse.Parsing;
using CodeBrix.TomlParse.Syntax;

var syntax = SyntaxParser.Parse(File.ReadAllText(path), sourceName: path);
if (syntax.HasErrors)
{
    foreach (var message in syntax.Diagnostics) Console.WriteLine(message);
    return;
}
var dbTable = syntax.Tables.OfType<TableSyntax>()
    .First(t => t.Name.ToString().Trim() == "database");
var portKv = dbTable.Items.First(kv => kv.Key.ToString().Trim() == "port");
var integer = (IntegerValueSyntax)portKv.Value;
var oldToken = integer.Token;
integer.Token = new SyntaxToken(TokenKind.Integer, "6543")
{
    LeadingTrivia = oldToken.LeadingTrivia,   // keeps the space after '='
    TrailingTrivia = oldToken.TrailingTrivia, // keeps " # comment"
};
integer.Value = 6543;
File.WriteAllText(path, syntax.ToString()); // everything else is untouched
```

Source-generated serialization, for trimmed and NativeAOT applications.

```csharp
using System.Text.Json.Serialization;
using CodeBrix.TomlParse;
using CodeBrix.TomlParse.Serialization;

[TomlSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[TomlSerializable(typeof(ServerConfig))]
internal partial class AppTomlContext : TomlSerializerContext
{
}

ServerConfig cfg = TomlSerializer.Deserialize(toml, AppTomlContext.Default.ServerConfig);
string text = TomlSerializer.Serialize(cfg, AppTomlContext.Default.ServerConfig);
```

Creating a document from scratch.

```csharp
using System;
using CodeBrix.TomlParse;
using CodeBrix.TomlParse.Model;

var created = new TomlTable
{
    ["title"] = "Generated",
    ["point"] = new TomlTable(inline: true) { ["x"] = 1L, ["y"] = 2L },
    ["released"] = new TomlDateTime(2026, 10, 7),
    ["servers"] = new TomlTableArray
    {
        new TomlTable { ["name"] = "alpha" },
        new TomlTable { ["name"] = "beta" },
    },
};
Console.WriteLine(TomlSerializer.Serialize(created));
```

Handling errors, and validating without mapping.

```csharp
using CodeBrix.TomlParse;
using CodeBrix.TomlParse.Model;
using CodeBrix.TomlParse.Parsing;

try
{
    var badOptions = new TomlSerializerOptions { SourceName = "bad.toml" };
    TomlSerializer.Deserialize<TomlTable>("a = \n", badOptions);
}
catch (TomlException ex)
{
    Console.WriteLine($"{ex.SourceName} line {ex.Line} col {ex.Column}");
    // ex.Message: "bad.toml(1,5) : error : Missing value after `=`."
}

var check = SyntaxParser.Parse(File.ReadAllText(path), sourceName: path);
bool valid = !check.HasErrors;
```

## Pitfalls

- Keys match CLR names case-sensitively by default, because `PropertyNamingPolicy` is null as in
  `System.Text.Json`. `port = 8080` does not bind to `public int Port` unless you set a naming policy,
  `PropertyNameCaseInsensitive = true` or `[TomlPropertyName("port")]` - and unbound keys are silently
  ignored, so the symptom is default values.
- Integers in the document model are `long` and floats are `double`. `(int)table["port"]` throws
  `InvalidCastException`; cast to `long`. When building a table, write `5432L`, not `5432`.
- Dates and times are `TomlDateTime`, not `DateTime`; read `.DateTime` and `.Kind`.
- A plain `Serialize` drops comments. Use a `TomlMetadataStore` (comments kept) or the syntax tree
  (byte-exact).
- Syntax-tree scalars render their `Token` text, not `.Value`. Setting only `IntegerValueSyntax.Value`
  changes nothing in `ToString()`; replace the token and carry its trivia across.
- A `[TomlSerializable]` context must be `partial` and derive from `TomlSerializerContext`.
  `[JsonSerializable]` on such a context is not supported (`TOMLPARSE008`).
- `[TomlConverter]` is honoured by the reflection path only; for a generated context register converters
  with `[TomlSourceGenerationOptions(Converters = [...])]`.
- Disabling reflection in code must happen before the first `TomlSerializer` call, with
  `AppContext.SetSwitch("CodeBrix.TomlParse.TomlSerializer.IsReflectionEnabledByDefault", false)`.
  Under `PublishAot=true` the package's build file has already done it.
- TOML 1.1 rules apply, so a document that relies on a 1.0-only restriction being enforced - rejecting a
  trailing comma in an inline table, for example - parses successfully.
- Duplicate keys are an error by default, and a root value that is not a table cannot be serialized
  unless `RootValueHandling = TomlRootValueHandling.WrapInRootKey`.

## Samples and tools in the repository

The repository ships no sample applications. Its non-package content is the source generator project
(packed into the library package rather than published on its own) and the test suite.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Tests | Every feature area, including the conformance corpus | [`tests/CodeBrix.TomlParse.Tests`](https://github.com/ellisnet/CodeBrix.TomlParse/tree/main/tests/CodeBrix.TomlParse.Tests) |
| Source generator | The `[TomlSerializable]` generator that ships inside the package | [`src/CodeBrix.TomlParse.SourceGeneration`](https://github.com/ellisnet/CodeBrix.TomlParse/tree/main/src/CodeBrix.TomlParse.SourceGeneration) |
| Conformance corpus | The standard TOML conformance test files, test data only and never packed | [`tests/CodeBrix.TomlParse.Tests/TestFiles/toml-test`](https://github.com/ellisnet/CodeBrix.TomlParse/tree/main/tests/CodeBrix.TomlParse.Tests/TestFiles/toml-test) |

Test files worth opening first, one per feature area:

| Feature | Test file |
| --- | --- |
| The document model | [`NewApiUntypedModelTests.cs`](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/tests/CodeBrix.TomlParse.Tests/NewApiUntypedModelTests.cs) · [`TomlTests.cs`](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/tests/CodeBrix.TomlParse.Tests/TomlTests.cs) |
| Typed (reflection) serialization | [`NewApiReflectionPocoTests.cs`](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/tests/CodeBrix.TomlParse.Tests/NewApiReflectionPocoTests.cs) · [`SerializationTests.cs`](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/tests/CodeBrix.TomlParse.Tests/SerializationTests.cs) |
| Source generation | [`NewApiSourceGenerationTests.cs`](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/tests/CodeBrix.TomlParse.Tests/NewApiSourceGenerationTests.cs) · [`SourceGenerationDiagnosticsTests.cs`](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/tests/CodeBrix.TomlParse.Tests/SourceGenerationDiagnosticsTests.cs) |
| Attributes | [`NewApiAttributeContractTests.cs`](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/tests/CodeBrix.TomlParse.Tests/NewApiAttributeContractTests.cs) |
| Converters | [`NewApiBuiltInConvertersTests.cs`](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/tests/CodeBrix.TomlParse.Tests/NewApiBuiltInConvertersTests.cs) · [`NewApiConverterAttributeTests.cs`](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/tests/CodeBrix.TomlParse.Tests/NewApiConverterAttributeTests.cs) |
| Polymorphism | [`NewApiPolymorphismTests.cs`](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/tests/CodeBrix.TomlParse.Tests/NewApiPolymorphismTests.cs) |
| Comments through the metadata store | [`NewApiMetadataStoreTests.cs`](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/tests/CodeBrix.TomlParse.Tests/NewApiMetadataStoreTests.cs) · [`NewApiMetadataStoreRoundtripTests.cs`](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/tests/CodeBrix.TomlParse.Tests/NewApiMetadataStoreRoundtripTests.cs) |
| The syntax tree and lossless round trip | [`SyntaxTests.cs`](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/tests/CodeBrix.TomlParse.Tests/SyntaxTests.cs) |
| Lexer and parser events | [`NewApiParsingPipelineTests.cs`](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/tests/CodeBrix.TomlParse.Tests/NewApiParsingPipelineTests.cs) |
| Errors and locations | [`NewApiExceptionLocationTests.cs`](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/tests/CodeBrix.TomlParse.Tests/NewApiExceptionLocationTests.cs) |
| Dates and times | [`TomlDateTimeTests.cs`](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/tests/CodeBrix.TomlParse.Tests/TomlDateTimeTests.cs) |
| TOML 1.1 features | [`Toml11ParsingTests.cs`](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/tests/CodeBrix.TomlParse.Tests/Toml11ParsingTests.cs) |
| Conformance | [`StandardTests.cs`](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/tests/CodeBrix.TomlParse.Tests/StandardTests.cs) |

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/AGENT-README.txt) |
| Non-package content in the repository | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.TomlParse.Tests](https://github.com/ellisnet/CodeBrix.TomlParse/tree/main/tests/CodeBrix.TomlParse.Tests) |

XML documentation ships alongside the assembly.

## License

CodeBrix.TomlParse is licensed under the BSD 2-Clause License, and the license is also named in the
package ID (`CodeBrix.TomlParse.BsdLicenseForever`). For the provenance and licensing of open source code
included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.YamlParse](CodeBrix.YamlParse.md) - the YAML counterpart: serialization, a document tree and a streaming parser
- [CodeBrix.Json.Extensions](CodeBrix.Json.Extensions.md) - polymorphism and reference preservation for `System.Text.Json`
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.TomlParse on GitHub](https://github.com/ellisnet/CodeBrix.TomlParse) - source and tests
