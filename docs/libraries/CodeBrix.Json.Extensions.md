<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Json.Extensions</sub>

# CodeBrix.Json.Extensions

**CodeBrix.Json.Extensions adds two things to `System.Text.Json` that it does not do on its own: attribute-driven polymorphic deserialization with a fallback type, and object-identity and cycle preservation.** Both are built on public, documented `System.Text.Json` surface, with no reflection into serializer internals and no NuGet dependencies. It is fully managed, and it is used from any .NET 10 application or from a CodeBrix.Platform application.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Json.Extensions](https://github.com/ellisnet/CodeBrix.Json.Extensions) |
| **Packages** | [`CodeBrix.Json.Extensions.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Json.Extensions.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later; no other dependencies |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Any platform .NET 10 supports - fully managed, no native libraries |

## What it does

- Dispatches incoming JSON to a concrete type from the value of a discriminator property you declare with attributes on the base class or interface.
- Sends an unrecognized, missing or null discriminator value to a fallback type you nominate, instead of throwing.
- Dispatches through several levels: a declared known type may itself be an abstract class or interface, as long as it declares its own discriminator.
- Needs no special entry point for polymorphism - once the attributes are in place, ordinary `JsonSerializer.Deserialize<TBase>(...)` calls dispatch correctly.
- Preserves object identity and cycles with an opt-in `"$id"` / `"$ref"` envelope: the first occurrence of an instance carries an id, later occurrences are references, and the whole graph is restored to shared instances on read.
- Serializes an entity that already exposes a stable id as only that id, and resolves it back to the live instance through a registry you own.
- Composes the two capabilities: a type can be both referenceable and discriminated, and a single node can carry both an id envelope and a discriminator.
- Costs nothing for types that carry none of its attributes - plain `JsonSerializer` paths are untouched.
- Ships XML documentation (IntelliSense) alongside the assembly.

## When to use it

Reach for this library when the JSON on the wire decides which concrete type you get, when an unknown discriminator value has to survive rather than fail the read, or when a graph shares instances or contains cycles that ordinary serialization would duplicate or reject.

It is an alternative to `System.Text.Json`'s own `[JsonPolymorphic]` / `[JsonDerivedType]` - one with a fallback type - and an opt-in, per-type alternative to the global `ReferenceHandler.Preserve`. Do not mix the two approaches on one type. When the wire format is YAML rather than JSON, [CodeBrix.YamlParse](CodeBrix.YamlParse.md) is the library that does the equivalent work.

These are the things it deliberately does not do:

- No polymorphism and no `$id`/`$ref` on value types. Both converter factories reject value types, so structs and record structs are out; use classes, or record classes with settable members.
- No reference preservation for types without a usable parameterless constructor or without settable members - constructor-only and init-only shapes throw or silently skip members on read.
- No replacement for `System.Text.Json`'s own polymorphism attributes; do not put both on one type.
- No writing of the discriminator value on serialize - the discriminator is your own model property.
- No source-generated `JsonSerializerContext`. The reference paths fall back to the reflection-based `DefaultJsonTypeInfoResolver` when you supply no resolver, so treat it as a reflection-based library; it is not designed for trimmed or AOT-only applications.
- No automatic deferral of unresolved by-id references; deferral is a call you make on `JsonReferenceRegistry`.
- No interchange between the two reference features inside one member: a member is either inlined with an id envelope or written as a bare id.
- No general-purpose converters for dates, enums, dictionaries or numbers - use `System.Text.Json`'s own converters for those.
- No use of serializer internals, and no change of behavior for types that carry none of its attributes.

## Getting started

```bash
dotnet add package CodeBrix.Json.Extensions.MitLicenseForever
```

The public API lives in two feature namespaces, so the namespace you import names the capability you are using. The root `CodeBrix.Json.Extensions` namespace intentionally holds no public types.

```csharp
using CodeBrix.Json.Extensions.Polymorphism;   // discriminator / fallback
using CodeBrix.Json.Extensions.References;     // $id/$ref and by-id refs
```

You will almost always also need:

```csharp
using System.Text.Json;                        // JsonSerializer, options
using System.Text.Json.Serialization;          // [JsonConverter], [JsonIgnore]
```

This is the headline capability end to end: an interface declares its discriminator, its known types and a catch-all, and a plain `JsonSerializer` call does the rest.

```csharp
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using CodeBrix.Json.Extensions.Polymorphism;

[JsonConverter(typeof(FallbackTypeConverterFactory))]
[JsonDiscriminator("type")]
[JsonKnownType(typeof(Circle), "circle")]
[JsonKnownType(typeof(Square), "square")]
[JsonFallbackType(typeof(UnknownShape))]
public interface IShape
{
    [JsonPropertyName("type")]
    string Type { get; set; }
}

public class Circle : IShape
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("radius")]
    public double Radius { get; set; }
}

public class Square : IShape
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("side")]
    public double Side { get; set; }
}

public class UnknownShape : IShape
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
}

public static class ShapeDemo
{
    public static void Run()
    {
        // "circle" resolves to Circle.
        IShape circle = JsonSerializer.Deserialize<IShape>(
            "{\"type\":\"circle\",\"radius\":2.5}");
        Console.WriteLine(((Circle)circle).Radius);          // 2.5

        // An unrecognized discriminator resolves to the fallback type
        // instead of throwing.
        IShape unknown = JsonSerializer.Deserialize<IShape>(
            "{\"type\":\"hexagon\"}");
        Console.WriteLine(unknown.GetType().Name);           // UnknownShape

        // Writing uses the runtime type's normal contract.
        string json = JsonSerializer.Serialize<IShape>(
            new Square { Type = "square", Side = 4 });
        Console.WriteLine(json);      // {"type":"square","side":4}
    }
}
```

Notice that nothing is registered at start-up and no custom entry point is called: the attributes on `IShape` are the whole configuration, and `"hexagon"` lands on `UnknownShape` rather than throwing.

## Key concepts

### Polymorphism: the three attributes

All three go on the base class or interface, and none of them is inherited.

| Attribute | What it declares |
| --- | --- |
| `[JsonDiscriminator("propertyName")]` | The JSON property whose value selects the concrete type; exposes `string PropertyName` |
| `[JsonKnownType(typeof(Derived), "value")]` | One discriminator value and the type it selects; `AllowMultiple = true`, exposes `Type KnownType` and `string DiscriminatorValue` |
| `[JsonFallbackType(typeof(UnknownDerived))]` | The catch-all used when the discriminator is missing, null or unmatched; exposes `Type FallbackType` |

Discriminator *values* are matched ordinally and case-sensitively against the JSON string value. A numeric or other non-string discriminator is matched against its raw JSON text, so `[JsonKnownType(typeof(NumberedShape), "7")]` matches the JSON number `7`. The discriminator *property name* is matched exactly, and additionally case-insensitively when `JsonSerializerOptions.PropertyNameCaseInsensitive` is true.

When no fallback type is declared, an unmatched discriminator throws `JsonException`. `[JsonDiscriminator]`'s constructor throws `ArgumentException` when the name is null, empty or whitespace.

### Wiring the polymorphism converter

`FallbackTypeConverterFactory` is the entry point that wires the attributes into `System.Text.Json`. Register it per type with `[JsonConverter(typeof(FallbackTypeConverterFactory))]` on the base type, or globally by adding one instance to `JsonSerializerOptions.Converters`.

```csharp
var options = new JsonSerializerOptions();
options.Converters.Add(new FallbackTypeConverterFactory());

IShape shape = JsonSerializer.Deserialize<IShape>(json, options);
```

Its `CanConvert` accepts a type only when the type is not a value type and declares `[JsonDiscriminator]` directly rather than by inheritance. `FallbackTypeConverter<T> : JsonConverter<T> where T : class` does the work: `Read` parses the object, resolves the concrete type from the discriminator and deserializes as that type; `Write` serializes the runtime type's normal contract, and throws `JsonException` when the runtime type is exactly the base type `T`. A known or fallback type that is non-instantiable and does not declare its own discriminator is a configuration error and throws `InvalidOperationException`.

### Identity and cycles with `$id` and `$ref`

`[JsonReferenceable]` marks a type as reference-tracked. The first occurrence of an instance is written with a `"$id"`; later occurrences of the *same* instance are written as a `"$ref"` and restored to one shared instance on read. Identity is reference identity, so two equal-but-distinct instances get two ids.

```json
{ "$id": "5", "type": "sprite" }
```

```json
{ "$ref": "5" }
```

This feature works only through the `ReferenceJson` static entry point. A plain `JsonSerializer` call ignores `[JsonReferenceable]`, inlines every occurrence, and will throw or loop on a cycle.

```csharp
string  Serialize<TValue>(TValue value, JsonSerializerOptions options = null)
string  Serialize(object value, Type inputType, JsonSerializerOptions options = null)
byte[]  SerializeToUtf8Bytes<TValue>(TValue value, JsonSerializerOptions options = null)
TValue  Deserialize<TValue>(string json, JsonSerializerOptions options = null)
TValue  Deserialize<TValue>(ReadOnlySpan<byte> utf8Json, JsonSerializerOptions options = null)
object  Deserialize(string json, Type returnType, JsonSerializerOptions options = null)
```

Each call is one self-contained operation with a fresh reference scope, and `"$id"` numbering restarts at `"1"` in every operation - ids are not stable across documents. Any `JsonSerializerOptions` you supply is treated as a settings template and copied, so it is never mutated; do not pass back options that this type returned. When the supplied options carry no `TypeInfoResolver`, the reflection-based `DefaultJsonTypeInfoResolver` is used to read object contracts.

A referenceable type must be constructible by `System.Text.Json` - a usable parameterless constructor - and must expose settable members, because cycles are restored by creating the instance first and populating it afterwards. A type that cannot be constructed that way throws a clear `JsonException` on read, and get-only members are skipped when reading. Members that are not themselves referenceable serialize inline as usual, and `[JsonIgnore]`d members stay ignored inside the id envelope.

### Serializing by identifier

For graphs whose shared entities already have stable ids, a member is serialized as only its identifier and resolved back to the live instance on read.

`IJsonReferenceable<out TId>` declares `TId JsonReferenceId { get; }`. Implement it on the entity type. `JsonReferenceId` is an ordinary member as far as `System.Text.Json` is concerned, so mark it `[JsonIgnore]` unless you want the id written twice - once as the entity's own property, once as this member.

`[JsonReferenceById]` marks a member whose value is an `IJsonReferenceable<TId>` entity to be written as only its id and resolved on read. It takes effect only through `ReferenceByIdJson`; a plain `JsonSerializer` call ignores it and inlines the member as usual. The wiring walks the type's `System.Text.Json` property list, so an annotated *field* is picked up only when the serializer surfaces it - with `JsonSerializerOptions.IncludeFields = true` or `[JsonInclude]`.

```csharp
string  Serialize<TValue>(TValue value, JsonSerializerOptions options = null)
byte[]  SerializeToUtf8Bytes<TValue>(TValue value, JsonSerializerOptions options = null)
TValue  Deserialize<TValue>(string json, JsonReferenceRegistry registry, JsonSerializerOptions options = null)
TValue  Deserialize<TValue>(ReadOnlySpan<byte> utf8Json, JsonReferenceRegistry registry, JsonSerializerOptions options = null)
```

`Serialize` needs no registry, because ids are read off the entities. `Deserialize` requires one; passing null throws `ArgumentNullException`. Supplied options are copied, and the copy's `TypeInfoResolver` gets the by-id modifier layered on top, falling back to `DefaultJsonTypeInfoResolver` when you supply none. Identifier types are whatever your entity exposes - string, int and `Guid` ids are all exercised by the test suite - and the id is written and read with the ordinary contract for that type.

### The registry and the two-phase apply

`JsonReferenceRegistry` is a sealed, caller-owned map from identifier to instance.

```csharp
void  Register(object entity)                          // by its IJsonReferenceable id
bool  TryResolve(Type type, object id, out object e)   // exact, then assignable
void  ResolveOrDefer(Type type, object id, Action<object> apply)  // forward refs
```

`Register` also runs any deferred fixups that the newly registered entity now satisfies. `TryResolve` first looks for an exact `(type, id)` match, then falls back to any registered entity with that id whose type is assignable to the requested type - which is how a reference declared as a base type resolves to a registered derived instance. A registry is a plain object you own, and it is not thread-safe for concurrent mutation.

> [!IMPORTANT]
> Read follows a two-phase apply: populate the registry with the authoritative entities (the owning collections) first, then deserialize the referencing graph. An identifier that is not registered by the time its member is read throws `JsonException` - the read path does not defer on your behalf.

`ResolveOrDefer` is the caller-facing path for genuine forward references. Call it yourself to record a fixup that runs when the target is registered later.

```csharp
var registry = new JsonReferenceRegistry();

// Record what to do when scene "s9" eventually shows up...
registry.ResolveOrDefer(typeof(Scene), "s9", entity => holder.Target = (Scene)entity);

// ...and the fixup runs at registration time.
registry.Register(new Scene { Id = "s9", Name = "Late" });
```

### Choosing a reference strategy

| Question | `$id` / `$ref` through `ReferenceJson` | By identifier through `ReferenceByIdJson` |
| --- | --- | --- |
| Id values | Generated sentinel values | Plain, human-readable id values |
| Document shape | Whole graph in one document | Owning sets can be split or registered separately |
| Model requirement | No model requirement | Entity must expose a stable id |
| Cycles | Cycles round-trip | Targets must exist in the registry |

Pick one per relationship for clarity. Both are opt-in and add no cost to types that use neither, and a member is never half one and half the other.

### The error model

| Exception | When |
| --- | --- |
| `JsonException` | The JSON value is not an object where one is required; a discriminator is missing or unmatched and no fallback is declared; a base-type instance is written; a `"$ref"` names an unknown id; a referenceable type cannot be constructed; a `[JsonReferenceById]` member's type does not implement `IJsonReferenceable<TId>`; a by-id identifier cannot be resolved |
| `InvalidOperationException` | A polymorphic base declares no `[JsonDiscriminator]`; a known or fallback type maps to the base type itself, is not assignable, is declared twice, or is abstract or an interface without its own discriminator |
| `ArgumentNullException` | A required argument (`inputType`, `returnType`, `registry`, `entity`, `type`, `apply`) is null |
| `ArgumentException` | A `[JsonDiscriminator]` property name is null, empty or whitespace; an entity registered with `JsonReferenceRegistry` does not implement `IJsonReferenceable<TId>` |

Every message names the offending type and, where relevant, tells you what to do instead - read the exception text before guessing.

### Performance characteristics

The per-base-type discriminator map - property name, known types, fallback - is built once and cached, so repeated deserialization of the same base type does not re-read attributes. Validation errors therefore surface on the *first* use of a base type, not on every call.

Polymorphic `Read` buffers the JSON object into a `JsonDocument` so it can look at the discriminator before choosing a type. That is one extra parse of the object's own text per polymorphic node - fine for normal payloads, but do not put a discriminated base type on a hot path over very large objects when a non-polymorphic model would do.

`ReferenceJson` builds a fresh copy of the options, plus a sibling metadata options object, on every call; for a tight loop, serialize one bigger document rather than many small ones. Prefer the `ReadOnlySpan<byte>` and `SerializeToUtf8Bytes` overloads when you already have UTF-8 bytes. Reference identity uses reference equality with an ordinary dictionary and a per-operation scope, so memory is proportional to the number of distinct referenceable instances in one document. `JsonReferenceRegistry.TryResolve` is O(1) for an exact `(type, id)` hit and falls back to a linear scan when the requested type is a base type of the registered one - register entities under the type you will reference them as when a registry gets large.

## Examples

Preserve identity and cycles across a graph, including a shared instance that appears twice:

```csharp
using System;
using System.Collections.Generic;
using CodeBrix.Json.Extensions.References;

[JsonReferenceable]
public class Node
{
    public string Name { get; set; }

    public Node Link { get; set; }

    public List<Node> Friends { get; set; }
}

public static class NodeDemo
{
    public static void Run()
    {
        var a = new Node { Name = "a" };
        var b = new Node { Name = "b" };
        a.Link = b;
        b.Link = a;                          // a cycle

        string json = ReferenceJson.Serialize(a);
        // {"$id":"1","Name":"a","Link":{"$id":"2","Name":"b",
        //  "Link":{"$ref":"1"},"Friends":null},"Friends":null}

        Node back = ReferenceJson.Deserialize<Node>(json);

        Console.WriteLine(ReferenceEquals(back, back.Link.Link));   // True

        // A shared instance appearing twice round-trips to ONE instance.
        var shared = new Node { Name = "hero" };
        var list = new List<Node> { shared, shared };

        List<Node> backList = ReferenceJson.Deserialize<List<Node>>(
            ReferenceJson.Serialize(list));

        Console.WriteLine(ReferenceEquals(backList[0], backList[1])); // True
    }
}
```

Options you pass to `ReferenceJson` are copied rather than mutated, so a naming policy and case-insensitive matching apply to both directions of the round trip:

```csharp
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
};

string json = ReferenceJson.Serialize(a, options);
Node back = ReferenceJson.Deserialize<Node>(json, options);
```

Write shared entities as bare identifiers, then read the referencing document back with the two-phase apply:

```csharp
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CodeBrix.Json.Extensions.References;

public class Scene : IJsonReferenceable<string>
{
    public string Id { get; set; }

    public string Name { get; set; }

    [JsonIgnore]
    public string JsonReferenceId => Id;
}

public class SceneRef
{
    [JsonReferenceById]
    public Scene Target { get; set; }        // written as just its id

    public string Label { get; set; }
}

public class SceneRefList
{
    public List<SceneRef> Refs { get; set; }
}

public static class SceneDemo
{
    public static void Run()
    {
        // The authoritative entities live in their own collection.
        var scenes = new List<Scene>
        {
            new Scene { Id = "s1", Name = "One" },
            new Scene { Id = "s2", Name = "Two" },
        };

        // The referencing document names them by id only.
        string refsJson = ReferenceByIdJson.Serialize(new SceneRefList
        {
            Refs = new List<SceneRef>
            {
                new SceneRef { Target = scenes[1], Label = "points-to-two" },
                new SceneRef { Target = scenes[0], Label = "points-to-one" },
            },
        });
        // {"Refs":[{"Target":"s2","Label":"points-to-two"},
        //          {"Target":"s1","Label":"points-to-one"}]}

        // Phase 1: register the owning entities.
        var registry = new JsonReferenceRegistry();

        foreach (var scene in scenes)
        {
            registry.Register(scene);
        }

        // Phase 2: deserialize the referencing graph.
        SceneRefList back = ReferenceByIdJson.Deserialize<SceneRefList>(
            refsJson, registry);

        Console.WriteLine(ReferenceEquals(back.Refs[0].Target, scenes[1]));  // True
        Console.WriteLine(back.Refs[1].Target.Name);                         // One
    }
}
```

Compose the two capabilities - a discriminated hierarchy whose instances also keep their identity through a cycle:

```csharp
using System;
using System.Text.Json.Serialization;
using CodeBrix.Json.Extensions.Polymorphism;
using CodeBrix.Json.Extensions.References;

// Note: the discriminator ATTRIBUTES are present, but NOT
// [JsonConverter(typeof(FallbackTypeConverterFactory))] - ReferenceJson's own
// converter does the dispatch, and attaching the polymorphism converter here
// would bypass reference handling.
[JsonReferenceable]
[JsonDiscriminator("kind")]
[JsonKnownType(typeof(Car), "car")]
[JsonKnownType(typeof(Truck), "truck")]
[JsonFallbackType(typeof(UnknownVehicle))]
public class Vehicle
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; }

    public string Name { get; set; }

    public Vehicle Tows { get; set; }
}

[JsonReferenceable]
public class Car : Vehicle
{
    public int Doors { get; set; }
}

[JsonReferenceable]
public class Truck : Vehicle
{
    public double Payload { get; set; }
}

[JsonReferenceable]
public class UnknownVehicle : Vehicle
{
}

public static class VehicleDemo
{
    public static void Run()
    {
        // Set the discriminator VALUE yourself - it is an ordinary member.
        var truck = new Truck { Kind = "truck", Name = "big", Payload = 2.5 };
        var car = new Car { Kind = "car", Name = "little", Doors = 4 };
        car.Tows = truck;
        truck.Tows = car;                    // a cycle across two types

        string json = ReferenceJson.Serialize<Vehicle>(car);
        Vehicle back = ReferenceJson.Deserialize<Vehicle>(json);

        Console.WriteLine(back.GetType().Name);                     // Car
        Console.WriteLine(back.Tows.GetType().Name);                // Truck
        Console.WriteLine(ReferenceEquals(back.Tows.Tows, back));   // True
    }
}
```

A minimum viable project, from an empty folder to a running program:

```bash
dotnet new console -n MyJsonApp --framework net10.0
cd MyJsonApp
dotnet add package CodeBrix.Json.Extensions.MitLicenseForever
```

```csharp
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using CodeBrix.Json.Extensions.Polymorphism;

[JsonConverter(typeof(FallbackTypeConverterFactory))]
[JsonDiscriminator("type")]
[JsonKnownType(typeof(TextNote), "text")]
[JsonFallbackType(typeof(UnknownNote))]
public class Note
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
}

public class TextNote : Note
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}

public class UnknownNote : Note
{
}

public static class Program
{
    public static void Main()
    {
        Note known = JsonSerializer.Deserialize<Note>(
            "{\"type\":\"text\",\"text\":\"hello\"}");
        Console.WriteLine(((TextNote)known).Text);        // hello

        Note unknown = JsonSerializer.Deserialize<Note>("{\"type\":\"video\"}");
        Console.WriteLine(unknown.GetType().Name);        // UnknownNote
    }
}
```

```bash
dotnet build
dotnet run
```

## Using it in a CodeBrix.Platform application

Add the package to your `.Core` library and use it from your services and view models. There is no UI surface, no native library and nothing to register at start-up: polymorphism works through plain `JsonSerializer` once the attributes are in place or the factory has been added to your options, and the reference features are reached through their own static entry points. Behavior is identical on every head.

The one deployment-shaped caveat: this is a reflection-based library with no source-generated serializer context, so it is not designed for trimmed or AOT-only applications.

## Pitfalls

- The package ID is `CodeBrix.Json.Extensions.MitLicenseForever`; the namespaces are `CodeBrix.Json.Extensions.Polymorphism` and `CodeBrix.Json.Extensions.References`.
- Do not attach `[JsonConverter(typeof(FallbackTypeConverterFactory))]` to a type that is also `[JsonReferenceable]`. `ReferenceJson`'s own converter performs the discriminator dispatch, and the polymorphism converter would bypass reference handling. Attributes yes, converter no.
- Do not expect a plain `JsonSerializer` call to honor `[JsonReferenceable]` or `[JsonReferenceById]`. The first needs `ReferenceJson`, the second needs `ReferenceByIdJson`. Only the polymorphism attributes work through plain `JsonSerializer`.
- Do not reuse options that `ReferenceJson` or `ReferenceByIdJson` returned or configured. Options you pass in are a template that gets copied; pass your own plain `JsonSerializerOptions` each time.
- Do not rely on case-insensitive discriminator *values*. Values are matched with `StringComparer.Ordinal`. Only the discriminator property *name* gets a case-insensitive fallback, and only when `PropertyNameCaseInsensitive` is true.
- Do not make a referenceable type immutable. It needs a usable parameterless constructor and settable members, because the instance is created first and populated afterwards so cycles can point back at it. Get-only members are skipped on read, and a constructor-only type throws `JsonException`.
- `[JsonReferenceable]` is not inherited. Put it on every derived type whose identity should be preserved, not only on the base.
- The library does not write the discriminator value for you. On a referenceable-plus-discriminated type the discriminator is an ordinary member: set it before serializing, or the value is null on the wire and the fallback type is chosen on read.
- Do not forget `[JsonIgnore]` on `IJsonReferenceable<TId>.JsonReferenceId`. It is a normal public member and will otherwise appear in the entity's own JSON in addition to its real id property.
- Do not deserialize a by-id document before registering its targets. The read path throws `JsonException` on an unresolved id and does not defer for you - register the owning collection first, or call `JsonReferenceRegistry.ResolveOrDefer` yourself for real forward references.
- Do not pass null as the registry to `ReferenceByIdJson.Deserialize` - it throws `ArgumentNullException`. `Serialize`, by contrast, takes no registry at all.
- A `[JsonReferenceById]` *field* does not work by default. The wiring walks the serializer's property list, so a field needs `JsonSerializerOptions.IncludeFields = true` or `[JsonInclude]`.
- Do not point `[JsonReferenceById]` at a type that does not implement `IJsonReferenceable<TId>` - configuring that member throws `JsonException`.
- Do not declare a known or fallback type that is abstract or an interface unless it declares its own `[JsonDiscriminator]` to dispatch further, and never declare the base type as its own known or fallback type. Both throw `InvalidOperationException`.
- Do not serialize an instance whose runtime type is exactly the discriminated base type - it throws `JsonException`. That is what the fallback type exists for: serialize the fallback subclass instead.
- Do not share one `JsonReferenceRegistry` across threads that mutate it concurrently.
- `"$id"` numbering restarts at `"1"` in every `ReferenceJson` operation and each call has a fresh reference scope, so ids are not stable across documents.
- Configuration errors surface on the *first* use of a base type, not on every call, because the discriminator map is cached.
- The `Polymorphism.Internal` and `References.Internal` sub-namespaces are implementation detail. Do not reference them from consumer code.

## Samples and tools in the repository

This repository contains no sample applications, demo projects, tools or optional test-data downloads. It builds one library and one test project, and the test project doubles as the body of worked examples for every API. It needs no environment variables, downloads, fixtures or external tools.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| `TestShapes.cs` | Polymorphic bases and derived types, including a numeric discriminator and a two-level dispatch chain | [`tests/CodeBrix.Json.Extensions.Tests`](https://github.com/ellisnet/CodeBrix.Json.Extensions/tree/main/tests/CodeBrix.Json.Extensions.Tests) |
| `ReferenceShapes.cs` | Referenceable nodes, cycles, a non-constructible type, ignored members, and the polymorphic-plus-referenceable composition | [`tests/CodeBrix.Json.Extensions.Tests`](https://github.com/ellisnet/CodeBrix.Json.Extensions/tree/main/tests/CodeBrix.Json.Extensions.Tests) |
| `ByIdShapes.cs` | Entities keyed by string, `Guid` and int, plus a deliberately invalid by-id member | [`tests/CodeBrix.Json.Extensions.Tests`](https://github.com/ellisnet/CodeBrix.Json.Extensions/tree/main/tests/CodeBrix.Json.Extensions.Tests) |
| `FallbackTypeConverterTests.cs` | The polymorphic read and write dispatch matrix: known values, fallback paths, missing, null and numeric discriminators, multi-level dispatch, error paths | [`tests/CodeBrix.Json.Extensions.Tests`](https://github.com/ellisnet/CodeBrix.Json.Extensions/tree/main/tests/CodeBrix.Json.Extensions.Tests) |
| `DiscriminatorMapTests.cs` | Discriminator resolution rules: ordinal value matching, case-insensitive property-name fallback, invalid configurations | [`tests/CodeBrix.Json.Extensions.Tests`](https://github.com/ellisnet/CodeBrix.Json.Extensions/tree/main/tests/CodeBrix.Json.Extensions.Tests) |
| `ReferenceJsonTests.cs` | Round trips through `ReferenceJson`: shared references, cycles, self-loops, diamonds, camelCase and case-insensitive options, error paths | [`tests/CodeBrix.Json.Extensions.Tests`](https://github.com/ellisnet/CodeBrix.Json.Extensions/tree/main/tests/CodeBrix.Json.Extensions.Tests) |
| `ReferenceByIdJsonTests.cs` | By-id write and read, the two-phase apply, deferred fixups, registry validation | [`tests/CodeBrix.Json.Extensions.Tests`](https://github.com/ellisnet/CodeBrix.Json.Extensions/tree/main/tests/CodeBrix.Json.Extensions.Tests) |

Run the suite with `dotnet test CodeBrix.Json.Extensions.slnx`.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Json.Extensions/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Json.Extensions/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Json.Extensions/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.Json.Extensions.Tests](https://github.com/ellisnet/CodeBrix.Json.Extensions/tree/main/tests/CodeBrix.Json.Extensions.Tests) |

## License

CodeBrix.Json.Extensions is licensed under the MIT License; the license is also named in the package ID (`CodeBrix.Json.Extensions.MitLicenseForever`). For the provenance and licensing of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Json.Extensions/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.YamlParse](CodeBrix.YamlParse.md) - the same kind of work when the wire format is YAML
- [Project architecture](../platform/04-project-architecture.md) - where a library package belongs in a CodeBrix.Platform solution
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.Json.Extensions on GitHub](https://github.com/ellisnet/CodeBrix.Json.Extensions) - source, tests and samples
