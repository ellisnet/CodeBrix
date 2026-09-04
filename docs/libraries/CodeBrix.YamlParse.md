<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.YamlParse</sub>

# CodeBrix.YamlParse

**CodeBrix.YamlParse reads and writes YAML at three levels: object serialization, a document tree, and a constant-memory streaming scanner, parser and emitter.** Most applications want the serialization layer - `DeserializerBuilder` reads a configuration file into your settings class, `SerializerBuilder` writes your objects back out - and drop to the other two layers when they need to edit a document in place or walk a stream too large to hold in memory. It is fully managed with no dependencies beyond .NET, so it works the same from any .NET 10 application and from a CodeBrix.Platform application.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.YamlParse](https://github.com/ellisnet/CodeBrix.YamlParse) |
| **Packages** | [`CodeBrix.YamlParse.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.YamlParse.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later; no other dependencies |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Windows, macOS and Linux - pure IL, including single-file publish |

## What it does

- Deserializes YAML into your CLR types and serializes them back, through `DeserializerBuilder` / `IDeserializer` and `SerializerBuilder` / `ISerializer`.
- Applies naming conventions on the way in and out - camelCase, PascalCase, hyphenated, underscored and lower-case - with a separate convention for enum members.
- Gives you a document tree in `CodeBrix.YamlParse.RepresentationModel`: a `YamlStream` of `YamlDocument`s whose `RootNode` is a `YamlScalarNode`, `YamlSequenceNode` or `YamlMappingNode`, for loading, editing and saving a file.
- Streams in constant memory in `CodeBrix.YamlParse.Core`: `Scanner` turns characters into tokens, `Parser` turns tokens into parsing events, and `Emitter` turns events back into text, with no object graph built at any point.
- Lets you replace every stage of both pipelines: type converters, type inspectors, node deserializers, node type resolvers, object factories, event emitters, graph visitors and traversal strategies are all interfaces with shipped implementations you can add to, reorder or replace.
- Expands merge keys (`<<: *defaults`) through the `MergingParser` wrapper.
- Deserializes polymorphic documents by buffering a node and inspecting it: `WithTypeDiscriminatingNodeDeserializer`, `KeyValueTypeDiscriminator` and `UniqueKeyTypeDiscriminator`.
- Handles anchors and aliases at all three layers - emitted automatically for a repeated object, resolved to shared instances on load, and available as raw `NodeEvent.Anchor` / `AnchorAlias` at the core layer.
- Emits JSON-compatible output with `JsonCompatible()`.
- Reports positions everywhere: a `Mark` (index, line, column) on tokens, events and nodes, and on every `YamlException`.
- Offers reflection-free counterparts for AOT scenarios, `StaticSerializerBuilder` and `StaticDeserializerBuilder`, driven by a `StaticContext` you write.

## When to use it

Use CodeBrix.YamlParse whenever YAML is the wire format: application configuration, pipeline and job definitions, data fixtures, front matter. The decision that matters is which of the three layers to start from.

```text
  * "Read a config file into my settings class"            -> Deserializer
  * "Write my object out as YAML"                           -> Serializer
  * "Read YAML whose shape I do not know at compile time"   -> Deserializer with
        `Deserialize<Dictionary<string, object>>(...)`, or the representation model
  * "Edit a YAML file and write it back"                    -> YamlStream
  * "Walk a 500 MB YAML stream without loading it"          -> Parser
  * "Emit YAML by hand, event by event"                     -> Emitter
  * "Pretty-print / reformat"                               -> Parser + Emitter
  * "Support `<<` merge keys"                               -> MergingParser wrapper
```

The layers compose: `Deserialize<T>(IParser)` and `YamlStream.Load(IParser)` both accept any `IParser`, so a `MergingParser` or a hand-built `Parser` feeds either of the higher layers.

These are the things it deliberately does not do:

- No compile-time source generator for AOT. `StaticSerializerBuilder`, `StaticDeserializerBuilder`, `StaticContext` and `StaticObjectFactory` are present, but you write the static context by hand.
- Not trim-safe or AOT-safe out of the box. The default pipelines are reflection-based, so under `PublishTrimmed` or `PublishAot` you must preserve your model types or take the static-context route.
- No comment round-tripping. Comments can be read as events and written from `[YamlMember(Description = ...)]`, but no layer preserves the comments of a document you loaded when you save it again.
- No formatting preservation. Load-and-save through `YamlStream` re-emits from the node tree, so original quoting, key order within flow collections, blank lines and line breaks are regenerated.
- No schema validation. The `Schemas` namespace supplies the standard tag names, nothing more.
- No async API. Every entry point is synchronous; read the text with async I/O yourself and hand the string or `TextReader` to the library.
- No JSON parser. `JsonCompatible()` makes the output valid JSON; it does not add a JSON reader.
- No XML, TOML, INI or properties support.
- No encoding detection. The library works on `TextReader` / `TextWriter`; choosing an encoding and handling a byte-order mark is the caller's job.
- No YAML 1.1 semantics by default: `Yes`, `No`, `On` and `Off` are strings, not booleans.

## Getting started

```bash
dotnet add package CodeBrix.YamlParse.MitLicenseForever
```

Three namespaces cover the typical consumer:

```csharp
    using CodeBrix.YamlParse.Serialization;
        // the builders, Serializer, Deserializer, the attributes
    using CodeBrix.YamlParse.Serialization.NamingConventions;
        // CamelCase, PascalCase, Hyphenated, Underscored, LowerCase, Null
    using CodeBrix.YamlParse.RepresentationModel;
        // YamlStream, YamlDocument, YamlNode and its subclasses
```

The low-level layer adds three more:

```csharp
    using CodeBrix.YamlParse.Core;
        // Scanner, Parser, MergingParser, Emitter, EmitterSettings,
        // AnchorName, TagName, Mark, YamlException and its subclasses
    using CodeBrix.YamlParse.Core.Events;
        // ParsingEvent and its subclasses (Scalar, MappingStart, ...)
    using CodeBrix.YamlParse.Core.Tokens;
        // Token and its subclasses -- needed only with Scanner directly
```

Extension points live one namespace per pipeline stage, all under `CodeBrix.YamlParse.Serialization`: `Callbacks`, `Converters`, `EventEmitters`, `NodeDeserializers`, `NodeTypeResolvers`, `ObjectFactories`, `ObjectGraphTraversalStrategies`, `ObjectGraphVisitors`, `Schemas`, `TypeInspectors`, `TypeResolvers`, `Utilities`, `ValueDeserializers`, `BufferedDeserialization` and `BufferedDeserialization.TypeDiscriminators`, plus `CodeBrix.YamlParse.Helpers`. The bare `CodeBrix.YamlParse` root namespace contains no public types, so never write a using for it expecting to find API there.

This is a complete program that reads a configuration document into a class:

```csharp
    using System;
    using System.Collections.Generic;
    using System.IO;
    using CodeBrix.YamlParse.Serialization;
    using CodeBrix.YamlParse.Serialization.NamingConventions;

    namespace YamlDemo;

    public class ServerConfig
    {
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public bool UseTls { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    public static class Program
    {
        private const string Yaml = """
            host: db.example.com
            port: 5432
            useTls: true
            tags:
              - primary
              - eu-west
            """;

        public static void Main()
        {
            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            ServerConfig config = deserializer.Deserialize<ServerConfig>(Yaml);

            Console.WriteLine($"{config.Host}:{config.Port} tls={config.UseTls}");
            Console.WriteLine(string.Join(", ", config.Tags));

            // From a file instead of a string:
            // using var reader = File.OpenText("config.yaml");
            // config = deserializer.Deserialize<ServerConfig>(reader);
        }
    }
```

Nothing is registered at start-up: configuration happens entirely through the two builders and their `With*` methods.

## Key concepts

### Deserializing into your types

`IDeserializer` offers a generic, an untyped and a `Type`-taking overload of each entry point, over a string, a `TextReader` or an `IParser`.

```csharp
    T       Deserialize<T>(string input);
    T       Deserialize<T>(TextReader input);
    T       Deserialize<T>(IParser parser);
    object? Deserialize(string input);
    object? Deserialize(TextReader input);
    object? Deserialize(IParser parser);
    object? Deserialize(string input, Type type);
    object? Deserialize(TextReader input, Type type);
    object? Deserialize(IParser parser, Type type);
```

`DeserializerBuilder` shapes the matching rules with `IgnoreUnmatchedProperties()`, `WithCaseInsensitivePropertyMatching()`, `WithEnforceRequiredMembers()`, `WithEnforceNullability()`, `WithDuplicateKeyChecking()`, `WithAttemptingUnquotedStringTypeDeserialization()` and `WithMaximumRecursion(int)`; it selects types with `WithTagMapping`, `WithoutTagMapping`, `WithTypeMapping<TInterface, TConcrete>()` and `WithTypeDiscriminatingNodeDeserializer(...)`; and it creates instances with `WithObjectFactory`.

The defaults are worth memorizing, because each is off unless you turn it on: a YAML key with no matching property throws; property matching is case-sensitive after the naming convention has been applied; duplicate keys are accepted, last one wins; `required` members are not enforced; and a null assigned to a non-nullable member is not rejected. Deserializing into `object` gives `Dictionary<object, object>` for a mapping, `List<object>` for a sequence and `string` for a scalar - and mapping keys always arrive as `string`. For any other target type, the scalar deserializer looks for a static `Parse(string, IFormatProvider)` method, so `IParsable<T>` types such as `TimeSpan`, `DateTimeOffset`, `Guid`, `IPAddress` and your own types read from a plain scalar with no registration at all.

### Serializing your objects

```csharp
    string Serialize(object? graph);
    string Serialize(object? graph, Type type);
    void   Serialize(TextWriter writer, object? graph);
    void   Serialize(TextWriter writer, object? graph, Type type);
    void   Serialize(IEmitter emitter, object? graph);
    void   Serialize(IEmitter emitter, object? graph, Type type);
```

`SerializerBuilder` shapes the output with `WithDefaultScalarStyle(ScalarStyle)`, `WithQuotingNecessaryStrings(bool quoteYaml1_1Strings = false)`, `WithNewLine(string)`, `WithIndentedSequences()` and `JsonCompatible()`; it controls values with `ConfigureDefaultValuesHandling(DefaultValuesHandling)`, `EmitDefaults()`, `EnsureRoundtrip()`, `DisableAliases()` and `WithMaximumRecursion(int)`; and it maps tags with `WithTagMapping` and `WithoutTagMapping`. `EnsureRoundtrip()` forces tags to be emitted and restricts emission to properties that have setters. `DisableAliases()` writes a repeated object out in full each time instead of as an anchor and an alias.

`DefaultValuesHandling` decides what is omitted:

```csharp
    [Flags] public enum DefaultValuesHandling
    {
        Preserve             = 0,   // emit everything (the default)
        OmitNull             = 1,
        OmitDefaults         = 2,   // default(T) or the value in [DefaultValue]
        OmitEmptyCollections = 4
    }
```

Both builders share `BuilderSkeleton<TBuilder>`, which supplies `IgnoreFields()`, `IncludeNonPublicProperties()`, `EnablePrivateConstructors()`, `WithNamingConvention`, `WithEnumNamingConvention`, `WithTypeResolver`, `WithTagMapping`, `WithAttributeOverride`, `WithTypeConverter` / `WithoutTypeConverter`, `WithTypeInspector` / `WithoutTypeInspector` and `WithYamlFormatter`. Public fields are included by default; call `IgnoreFields()` to exclude them.

### Naming conventions

`INamingConvention` declares `string Apply(string value)` and `string Reverse(string value)`. Six implementations live in `CodeBrix.YamlParse.Serialization.NamingConventions`, each sealed with a public parameterless constructor and a `public static readonly INamingConvention Instance` field:

```text
    CamelCaseNamingConvention        MyProperty -> myProperty
    PascalCaseNamingConvention       myProperty -> MyProperty
    HyphenatedNamingConvention       MyProperty -> my-property
    UnderscoredNamingConvention      MyProperty -> my_property
    LowerCaseNamingConvention        MyProperty -> myproperty
    NullNamingConvention             MyProperty -> MyProperty  (identity; the default)
```

`WithNamingConvention` affects property names; `WithEnumNamingConvention` affects enum member names and is set separately.

### Attributes

`[YamlMember]` is the one you reach for most.

```csharp
    [AttributeUsage(...)] public sealed class YamlMemberAttribute : Attribute
    {
        public YamlMemberAttribute();
        public YamlMemberAttribute(Type serializeAs);
        public string? Description { get; set; }        // emitted as a YAML comment above the key
        public Type?   SerializeAs { get; set; }
        public int     Order { get; set; }
        public string? Alias { get; set; }              // the YAML key name to use
        public bool    ApplyNamingConventions { get; set; }
        public ScalarStyle ScalarStyle { get; set; }
        public DefaultValuesHandling DefaultValuesHandling { get; set; }
        public bool    IsDefaultValuesHandlingSpecified { get; }
    }
```

`[YamlIgnore]` skips a member entirely, `[YamlConverter(typeof(...))]` attaches a per-member converter, and `[YamlStaticContext]` marks a static context. In `CodeBrix.YamlParse.Serialization.Callbacks`, four parameterless marker attributes - `[OnSerializing]`, `[OnSerialized]`, `[OnDeserializing]`, `[OnDeserialized]` - are applied to methods on the object being processed. A separate hook, `IPostDeserializationCallback`, runs after the whole graph, including forward aliases, has been resolved.

### Custom type converters

```csharp
    public interface IYamlTypeConverter
    {
        bool    Accepts(Type type);
        object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer);
        void    WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer);
    }

    public delegate object? ObjectDeserializer(Type type);
    public delegate void    ObjectSerializer(object? value, Type? type = null);
```

Register one with `.WithTypeConverter(new MyConverter())` on either builder, or per member with `[YamlConverter(typeof(MyConverter))]`. For a converter over a scalar, derive from `ScalarConverterBase<T>`, which implements `Accepts` for you. Converters for `DateOnly`, `TimeOnly`, `DateTime`, ISO-8601 date-times, `DateTimeOffset`, `Guid`, `TimeSpan`, `Uri` and `System.Type` ship in `CodeBrix.YamlParse.Serialization.Converters`. To replace one rather than add to the chain, use the location selector: `.WithTypeConverter(new DateTimeConverter(DateTimeKind.Local), w => w.InsteadOf<DateTimeConverter>())`.

That selector is the general mechanism for ordering any pipeline component:

```csharp
    public interface IRegistrationLocationSelectionSyntax<TBaseRegistrationType>
    {
        void InsteadOf<TRegistrationType>() where TRegistrationType : TBaseRegistrationType;
        void Before<TRegistrationType>()    where TRegistrationType : TBaseRegistrationType;
        void After<TRegistrationType>()     where TRegistrationType : TBaseRegistrationType;
        void OnTop();
        void OnBottom();
    }
```

Without a selector, converters and node deserializers are registered on top, so they run first.

A type can also describe its own YAML through `IYamlConvertible`, which needs no registration because `YamlConvertibleNodeDeserializer` and `YamlConvertibleTypeResolver` are in both default pipelines:

```csharp
    public interface IYamlConvertible
    {
        void Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer);
        void Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer);
    }
```

`StreamFragment` is a ready-made `IYamlConvertible` that captures a subtree as raw events, exposing `IList<ParsingEvent> Events`.

### Polymorphic documents

`WithTypeDiscriminatingNodeDeserializer` buffers a node so the reader can look at its content before choosing a type.

```csharp
    public interface ITypeDiscriminatingNodeDeserializerOptions
    {
        void AddTypeDiscriminator(ITypeDiscriminator discriminator);
        void AddKeyValueTypeDiscriminator<T>(string discriminatorKey,
                                             IDictionary<string, Type> valueTypeMapping);
        void AddKeyValueTypeDiscriminator<T>(string discriminatorKey,
                                             params (string, Type)[] valueTypeMapping);
        void AddUniqueKeyTypeDiscriminator<T>(IDictionary<string, Type> uniqueKeyTypeMapping);
        void AddUniqueKeyTypeDiscriminator<T>(params (string, Type)[] uniqueKeyTypeMapping);
    }

    public interface ITypeDiscriminator
    {
        Type BaseType { get; }
        bool TryDiscriminate(IParser buffer, out Type? suggestedType);
    }
```

`KeyValueTypeDiscriminator` picks the type from the value of a named key; `UniqueKeyTypeDiscriminator` picks it from the presence of a key. Buffering is done by `ParserBuffer`, whose `maxDepth` and `maxLength` arguments cap how much of the stream is held - both default to unlimited, so set them on untrusted or very large input.

### Tags and schemas

```csharp
    public readonly struct TagName : IEquatable<TagName>
    {
        public static readonly TagName Empty;
        public TagName(string value);
        public string Value { get; }          // throws InvalidOperationException when non-specific
        public bool IsEmpty { get; }
        public bool IsNonSpecific { get; }    // "!" or "?"
        public bool IsLocal { get; }          // starts with '!'
        public bool IsGlobal { get; }
        public static implicit operator TagName(string? value);
    }
```

Standard tag names are `static readonly TagName` fields on nested `Tags` classes in `CodeBrix.YamlParse.Serialization.Schemas`: `FailsafeSchema.Tags.Map`, `.Seq` and `.Str`; `JsonSchema.Tags.Null`, `.Bool`, `.Int` and `.Float`; `CoreSchema.Tags`; and `DefaultSchema.Tags.Timestamp`. `DeserializerBuilder` pre-registers the mappings from those tags to `Dictionary<object, object>`, `string`, `bool`, `double`, `int` and `DateTime`; add your own with `.WithTagMapping("!myType", typeof(MyType))`, and the same call on the serializer side emits `MyType` with that tag.

### The representation model

`YamlStream` loads and saves a whole document tree.

```csharp
    public class YamlStream : IEnumerable<YamlDocument>
    {
        public YamlStream();
        public YamlStream(params YamlDocument[] documents);
        public YamlStream(IEnumerable<YamlDocument> documents);
        public IList<YamlDocument> Documents { get; }
        public void Add(YamlDocument document);
        public void Load(TextReader input);
        public void Load(IParser parser);
        public void Save(TextWriter output);                              // assignAnchors: true
        public void Save(TextWriter output, bool assignAnchors);
        public void Save(IEmitter emitter, bool assignAnchors);
        public void Accept(IYamlVisitor visitor);
        public IEnumerator<YamlDocument> GetEnumerator();
    }
```

Every node derives from `YamlNode`, which carries `Anchor`, `Tag`, `Start` and `End` marks, a `NodeType`, an `AllNodes` walk and indexers for the sequence and mapping cases:

```csharp
    public abstract class YamlNode
    {
        public AnchorName Anchor { get; set; }
        public TagName Tag { get; set; }
        public Mark Start { get; }                          // position in the source, or Mark.Empty
        public Mark End { get; }
        public abstract YamlNodeType NodeType { get; }
        public IEnumerable<YamlNode> AllNodes { get; }
        public abstract void Accept(IYamlVisitor visitor);
        public YamlNode this[int index] { get; }      // sequence element only
        public YamlNode this[YamlNode key] { get; }   // mapping value only
        public static implicit operator YamlNode(string value);     // -> YamlScalarNode
        public static implicit operator YamlNode(string[] sequence);// -> YamlSequenceNode
        public static explicit operator string?(YamlNode node);     // scalar only; throws otherwise
    }

    public enum YamlNodeType { Alias, Mapping, Scalar, Sequence }
```

`YamlScalarNode` adds `Value` and `Style`; `YamlSequenceNode` is an `IEnumerable<YamlNode>` with `Children`, `Style` and `Add`; `YamlMappingNode` exposes `Children` as an `IOrderedDictionary<YamlNode, YamlNode>` - a dictionary plus a positional indexer, `Insert(int, TKey, TValue)` and `RemoveAt(int)` - along with `Style`, four `Add` overloads and `FromObject(object mapping)` for an anonymous object or POCO. Derive from `YamlVisitorBase` to visit a tree.

Node equality is by value, so two scalar nodes holding the same text are `Equals`; use `YamlNodeIdentityEqualityComparer` when you need reference identity. On load, `*aliases` resolve to the same node instance the `&anchor` produced, so the loaded tree is a graph. `Save(output)` re-assigns anchors to any node reachable more than once; `Save(output, assignAnchors: false)` skips that step and throws if the graph actually needs anchors.

### Streaming: scanner, parser and emitter

`IParser` has two members - `Current` and `MoveNext()` - and `Parser` wraps a `Scanner`.

```csharp
    public class Parser : IParser
    {
        public Parser(TextReader input);   // wraps a Scanner with skipComments: true
        public Parser(IScanner scanner);   // pass your own Scanner to keep comments
        public ParsingEvent? Current { get; }
        public bool MoveNext();
    }
```

`ParserExtensions` supplies the reading vocabulary: `Consume<T>` advances and throws if the next event is not a `T`, `TryConsume<T>` and `Accept<T>` test without throwing, `Require<T>` and `Peek<T>` look ahead, `SkipThisAndNestedEvents` jumps over a whole subtree, and `TryFindMappingEntry` pulls a single key out of a large mapping without materializing the rest. Events live in `CodeBrix.YamlParse.Core.Events`: `StreamStart`, `StreamEnd`, `DocumentStart`, `DocumentEnd`, `MappingStart`, `MappingEnd`, `SequenceStart`, `SequenceEnd`, `Scalar`, `AnchorAlias` and `Comment`, with the styles

```csharp
    public enum ScalarStyle
    { Any, Plain, SingleQuoted, DoubleQuoted, Literal, Folded, ForcePlain }
    public enum SequenceStyle { Any, Block, Flow }
    public enum MappingStyle  { Any, Block, Flow }
```

`EmitterSettings` controls the writing side, with `BestIndent` defaulting to 2, `BestWidth` to `int.MaxValue` (no wrapping), `NewLine` to `Environment.NewLine` and `MaxSimpleKeyLength` to 1024. Its `With*` methods return a new settings object rather than mutating. Emitting by hand is a matter of pushing events:

```csharp
            using var writer = new StringWriter();
            var emitter = new Emitter(writer);

            emitter.Emit(new StreamStart());
            emitter.Emit(new DocumentStart());
            emitter.Emit(new MappingStart());

            emitter.Emit(new Scalar("name"));
            emitter.Emit(new Scalar("Inanna"));

            emitter.Emit(new Scalar("titles"));
            emitter.Emit(new SequenceStart(AnchorName.Empty, TagName.Empty, true, SequenceStyle.Block));
            emitter.Emit(new Scalar("Queen of Heaven"));
            emitter.Emit(new Scalar("Lady of Uruk"));
            emitter.Emit(new SequenceEnd());

            emitter.Emit(new MappingEnd());
            emitter.Emit(new DocumentEnd(isImplicit: true));
            emitter.Emit(new StreamEnd());
```

A multi-document stream is read one CLR object at a time by consuming the `StreamStart` yourself and then looping. Taking the `StreamStart` first is what lets `Deserialize<T>(IParser)` be called once per document:

```csharp
            // (b) Deserializer: one CLR object per document, streaming.
            IDeserializer deserializer = new DeserializerBuilder().Build();
            IParser parser = new Parser(new StringReader(Multi));

            parser.Consume<StreamStart>();
            var records = new List<Record>();
            while (parser.Accept<DocumentStart>())
            {
                records.Add(deserializer.Deserialize<Record>(parser));
            }
            parser.Consume<StreamEnd>();
```

### Merge keys, anchors and comments

Nothing else in the library expands merge keys, so wrap the parser when your documents use `<<`:

```csharp
    public sealed class MergingParser : IParser
    {
        public MergingParser(IParser innerParser);
        public MergingParser(IParser innerParser, int maxParsingEvents = 100_000);
        public ParsingEvent? Current { get; }
        public bool MoveNext();
    }
```

`MergingParser` buffers the entire stream up to `maxParsingEvents`, so wrap it only around input that actually uses merge keys. An `AnchorName` may not be empty and may not contain `[`, `]`, `{`, `}` or `,`; the constructor validates it. Comments are discarded by default at every layer - to see `Comment` events, build the scanner yourself:

```csharp
    var parser = new Parser(new Scanner(reader, skipComments: false));
```

To *write* a comment above a mapping key, set `[YamlMember(Description = "...")]` and `CommentsObjectGraphVisitor` turns it into a `Comment` event.

### Positions and errors

```csharp
    public readonly struct Mark : IEquatable<Mark>, IComparable<Mark>, IComparable
    {
        public static readonly Mark Empty;      // index 0, line 1, column 1
        public Mark(long index, long line, long column);
        public long Index { get; }              // 0-based character offset
        public long Line { get; }               // 1-based
        public long Column { get; }             // 1-based
        // == != < <= > >= operators, CompareTo, ToString
    }
```

Every failure is a `YamlException` or one of its subclasses in `CodeBrix.YamlParse.Core`: `SyntaxErrorException` for malformed text, `SemanticErrorException` for well-formed but meaningless input, `AnchorNotFoundException` for an alias with no matching anchor, `ForwardAnchorNotSupportedException` for an alias resolved before its anchor, and `MaximumRecursionLevelReachedException` for the recursion cap. Catch `YamlException` to catch them all, and report `Start.Line` and `Start.Column`. A failure inside a property setter or a converter is wrapped in a `YamlException` positioned at the offending key, with the original exception as `InnerException`.

## Examples

Serialize a class with attributes and read it straight back:

```csharp
    using System;
    using System.Collections.Generic;
    using CodeBrix.YamlParse.Serialization;
    using CodeBrix.YamlParse.Serialization.NamingConventions;

    namespace YamlDemo;

    public class Catalog
    {
        [YamlMember(Alias = "catalog-title", Description = "Shown at the top of the page")]
        public string Title { get; set; } = "";

        [YamlMember(Order = 1)]
        public List<string> Items { get; set; } = new();

        [YamlIgnore]
        public DateTime LoadedAt { get; set; } = DateTime.UtcNow;
    }

    public static class Program
    {
        public static void Main()
        {
            var catalog = new Catalog { Title = "Tablets", Items = { "clay", "wax" } };

            ISerializer serializer = new SerializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .WithQuotingNecessaryStrings()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build();

            string yaml = serializer.Serialize(catalog);
            Console.WriteLine(yaml);
            // # Shown at the top of the page
            // catalog-title: Tablets
            // items:
            // - clay
            // - wax

            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();

            Catalog again = deserializer.Deserialize<Catalog>(yaml);
            Console.WriteLine(again.Items.Count);   // 2
        }
    }
```

Load a document, edit it in place and save it again, all through the representation model:

```csharp
    using System;
    using System.IO;
    using CodeBrix.YamlParse.Core.Events;      // SequenceStyle lives here
    using CodeBrix.YamlParse.RepresentationModel;

    namespace YamlDemo;

    public static class Program
    {
        public static void Main()
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(
                "city: Uruk\nriver: Euphrates\nwalls:\n  - inner\n  - outer\n"));

            var root = (YamlMappingNode)stream.Documents[0].RootNode;

            // Read a value. Children is keyed by YamlNode, and string converts implicitly.
            var city = (YamlScalarNode)root.Children["city"];
            Console.WriteLine(city.Value);                         // Uruk

            // The YamlNode indexer is the shorter form of the same lookup.
            Console.WriteLine((string?)root["river"]);             // Euphrates

            // Edit
            city.Value = "Uruk (Warka)";
            root.Add("founded", "-4000");
            ((YamlSequenceNode)root.Children["walls"]).Add("moat");

            // Style: force the sequence to flow style
            ((YamlSequenceNode)root.Children["walls"]).Style = SequenceStyle.Flow;

            // Save
            using var writer = new StringWriter();
            stream.Save(writer, assignAnchors: false);
            Console.WriteLine(writer.ToString());

            // Walk every node
            foreach (YamlNode node in stream.Documents[0].AllNodes)
            {
                Console.WriteLine($"{node.NodeType} at line {node.Start.Line}");
            }
        }
    }
```

Transform a stream in constant memory by rewriting events between a parser and an emitter:

```csharp
    using System;
    using System.IO;
    using CodeBrix.YamlParse.Core;
    using CodeBrix.YamlParse.Core.Events;

    namespace YamlDemo;

    public static class Program
    {
        public static void Main()
        {
            using var reader = new StringReader("name: Inanna\ncity: Uruk\n");
            using var writer = new StringWriter();

            IParser parser = new Parser(reader);
            IEmitter emitter = new Emitter(writer, new EmitterSettings()
                .WithBestIndent(4)
                .WithIndentedSequences());

            while (parser.MoveNext())
            {
                ParsingEvent current = parser.Current!;

                // Upper-case every scalar VALUE, leaving keys alone.
                if (current is Scalar scalar && !scalar.IsKey)
                {
                    current = new Scalar(scalar.Anchor, scalar.Tag, scalar.Value.ToUpperInvariant(),
                                         scalar.Style, scalar.IsPlainImplicit, scalar.IsQuotedImplicit);
                }

                emitter.Emit(current);
            }

            Console.WriteLine(writer.ToString());
        }
    }
```

Read merge keys on the way in, and let anchors and aliases happen on the way out:

```csharp
    using System;
    using System.Collections.Generic;
    using System.IO;
    using CodeBrix.YamlParse.Core;
    using CodeBrix.YamlParse.Serialization;

    namespace YamlDemo;

    public class Job
    {
        public string Image { get; set; } = "";
        public int Retries { get; set; }
        public string Name { get; set; } = "";
    }

    public static class Program
    {
        private const string Yaml = """
            defaults: &defaults
              image: alpine
              retries: 3
            build:
              <<: *defaults
              name: build
            """;

        public static void Main()
        {
            IDeserializer deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();

            // MergingParser is REQUIRED for "<<" -- a plain Parser leaves it as a literal key.
            IParser parser = new MergingParser(new Parser(new StringReader(Yaml)));
            var all = deserializer.Deserialize<Dictionary<string, Job>>(parser);

            Job build = all["build"];
            Console.WriteLine($"{build.Name} {build.Image} {build.Retries}");   // build alpine 3

            // On the way out, a repeated object becomes an anchor + alias automatically.
            var shared = new Job { Image = "alpine", Retries = 3, Name = "shared" };
            string yaml = new SerializerBuilder().Build()
                .Serialize(new Dictionary<string, Job> { ["a"] = shared, ["b"] = shared });
            Console.WriteLine(yaml);      // b: *o0   (or similar alias)

            // ...unless you switch it off:
            string expanded = new SerializerBuilder().DisableAliases().Build()
                .Serialize(new Dictionary<string, Job> { ["a"] = shared, ["b"] = shared });
            Console.WriteLine(expanded);  // both entries written out in full
        }
    }
```

## Using it in a CodeBrix.Platform application

Add the package to your `.Core` library. Everything is pure IL and works on every platform .NET 10 runs on, including single-file publish, so the library behaves identically on every head, and nothing is registered at start-up.

Build once and reuse: `SerializerBuilder.Build()` and `DeserializerBuilder.Build()` construct the whole component chain and a `CachedTypeInspector`, so building a serializer per call throws that cache away every time. A built `ISerializer` or `IDeserializer` is safe to share across calls and threads - hold it in a `static readonly` field and treat it as immutable. A builder being configured is not thread-safe, and neither is a single `Parser`, `Scanner`, `Emitter` or `YamlStream` instance.

> [!IMPORTANT]
> The default pipelines are reflection-based. Under `PublishTrimmed` or `PublishAot` you must either preserve your model types or write a `StaticContext` and use `StaticSerializerBuilder` / `StaticDeserializerBuilder`.

## Pitfalls

- Unmatched properties throw by default. A key with no matching member raises a `YamlException`; call `IgnoreUnmatchedProperties()` if forward-compatible configuration files matter to you.
- Property matching is case-sensitive. `Name` does not bind to `name` unless you set a naming convention or `WithCaseInsensitivePropertyMatching()`, and the naming convention must match on both the serializer and the deserializer or a round trip fails.
- Public fields are included. Both builders read fields as well as properties unless you call `IgnoreFields()`.
- Strings that look like other types are emitted unquoted by default, so a `string` holding `"true"`, `"123"` or `"null"` reads back as a bool, an int or null. Call `WithQuotingNecessaryStrings()`.
- `YamlMappingNode.Children` is keyed by `YamlNode`, not by `string`; `root.Children["city"]` works only because `string` converts implicitly. A missing key throws `KeyNotFoundException`, not a `YamlException`.
- `Children` has two indexers, and an `int` picks by position: `mapping.Children[0]` returns the first entry, not the value stored under the key `0`. Look up a numeric key with `mapping.Children[new YamlScalarNode("0")]`.
- Node equality is by value. Use `YamlNodeIdentityEqualityComparer` when you are keeping a set of nodes by identity.
- Merge keys are not expanded unless you wrap the parser in `MergingParser`; without it, `<<` is an ordinary key whose value is an alias.
- Comments are discarded. `new Parser(reader)` builds a `Scanner` with `skipComments: true`, and deserialize-then-serialize never preserves comments.
- Duplicate keys are accepted silently, last one wins, unless `WithDuplicateKeyChecking()` is enabled.
- Deserializing into `object` gives `Dictionary<object, object>`, not `Dictionary<string, object>`. Ask for `Dictionary<string, object>` explicitly at the top level if that is what you want; nested values are still the untyped shapes.
- Scalars come back as `string` when the target is `object`. Enable `WithAttemptingUnquotedStringTypeDeserialization()` to infer numbers and booleans - mapping keys stay `string` even then.
- A round trip through an interface or base-class property loses the type unless you use `EnsureRoundtrip()`, a tag mapping or a type discriminator. `EnsureRoundtrip()` also stops emitting read-only properties, which changes your output.
- `DisableAliases()` plus a circular reference is a `StackOverflowException` that no `catch` will save you from. Leave aliases on for graphs that might contain cycles.
- `TagName.Value` and `AnchorName.Value` throw on an empty or non-specific instance. Check `IsEmpty`, and `IsNonSpecific` for tags, first.
- `(string?)node` throws `ArgumentException` for a non-scalar node, and `node[0]` / `node["key"]` throw when the node is not a sequence or a mapping. Check `NodeType` or pattern-match first.
- The `.MitLicenseForever` suffix is part of the package ID only; it is never part of a namespace or an assembly name.
- Bound recursion on untrusted input. The defaults already stop runaway nesting with `MaximumRecursionLevelReachedException`; lower them with `WithMaximumRecursion(n)`, and set `maxDepth` and `maxLength` on the type-discriminating deserializer.

## Samples and tools in the repository

This repository ships no sample applications and no tools: it contains one packable project and one test project. The test project is the worked-example set, and it needs no test-data files, no environment variables and no network access. Run it with `dotnet test CodeBrix.YamlParse.slnx`.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| `DeserializerTests.cs` | `Deserialize<T>` into a typed object, camelCase key mapping, dictionary and list targets, the unmatched-property throw and its escape | [`tests/CodeBrix.YamlParse.Tests`](https://github.com/ellisnet/CodeBrix.YamlParse/tree/main/tests/CodeBrix.YamlParse.Tests) |
| `SerializerTests.cs` | Scalar properties, camelCase output, dictionaries, block sequences, nested objects, serializing null | [`tests/CodeBrix.YamlParse.Tests`](https://github.com/ellisnet/CodeBrix.YamlParse/tree/main/tests/CodeBrix.YamlParse.Tests) |
| `SerializationRoundtrip.cs` | Serialize-then-deserialize for a flat object, a nested object with a naming convention, and a dictionary | [`tests/CodeBrix.YamlParse.Tests`](https://github.com/ellisnet/CodeBrix.YamlParse/tree/main/tests/CodeBrix.YamlParse.Tests) |
| `NamingConventionTests.cs` | `Apply` behavior for all six naming conventions | [`tests/CodeBrix.YamlParse.Tests`](https://github.com/ellisnet/CodeBrix.YamlParse/tree/main/tests/CodeBrix.YamlParse.Tests) |
| `YamlStreamTests.cs` | `YamlStream.Load`, mapping lookup, sequence ordering, and `Save` producing re-loadable YAML | [`tests/CodeBrix.YamlParse.Tests`](https://github.com/ellisnet/CodeBrix.YamlParse/tree/main/tests/CodeBrix.YamlParse.Tests) |
| `ParserTests.cs` | Event order from `Parser.MoveNext`, and the exception on malformed input | [`tests/CodeBrix.YamlParse.Tests`](https://github.com/ellisnet/CodeBrix.YamlParse/tree/main/tests/CodeBrix.YamlParse.Tests) |

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.YamlParse/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.YamlParse/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.YamlParse/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.YamlParse.Tests](https://github.com/ellisnet/CodeBrix.YamlParse/tree/main/tests/CodeBrix.YamlParse.Tests) |

## License

CodeBrix.YamlParse is licensed under the MIT License; the license is also named in the package ID (`CodeBrix.YamlParse.MitLicenseForever`). For the provenance and licensing of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.YamlParse/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Json.Extensions](CodeBrix.Json.Extensions.md) - the same kind of work when the wire format is JSON
- [Project architecture](../platform/04-project-architecture.md) - where a library package belongs in a CodeBrix.Platform solution
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.YamlParse on GitHub](https://github.com/ellisnet/CodeBrix.YamlParse) - source, tests and samples
