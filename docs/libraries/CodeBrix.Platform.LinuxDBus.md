<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Platform.LinuxDBus</sub>

# CodeBrix.Platform.LinuxDBus

**CodeBrix.Platform.LinuxDBus is a fully managed, low-level D-Bus protocol library for Linux: it
connects to the session bus, the system bus or any transport the D-Bus specification allows, sends
and receives raw D-Bus messages, and exposes the wire-level reader and writer primitives that
higher-level consumers build on top of.** It is a protocol library, not a client framework - there is
no source generator, no interface-attribute model and no generated proxy classes, so you build each
outgoing message by writing its header and body fields in order, and decode each reply by reading
those fields back in the same order. Use it from any .NET 10 application running on Linux, or from a
CodeBrix.Platform Linux head.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Platform.LinuxDBus](https://github.com/ellisnet/CodeBrix.Platform.LinuxDBus) |
| **Packages** | [`CodeBrix.Platform.LinuxDBus.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.LinuxDBus.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later; no NuGet dependencies at all - only the .NET shared framework; a running D-Bus daemon, which nothing in this package starts for you |
| **Use it from** | Any .NET 10 application running on Linux, or a CodeBrix.Platform Linux head |
| **Platforms** | Linux. The package compiles and loads on Windows and macOS - the assembly is not OS-gated - but connecting fails without a reachable bus, and the file-descriptor paths call into `libc`. Treat it as Linux-only. |

## What it does

- Connects to the session bus, the system bus, or any address string the D-Bus specification allows:
  `unix:path=`, `unix:abstract=` and `tcp:host=`.
- Sends method calls and awaits their replies, decoding each reply through a reader delegate you
  supply.
- Subscribes to signals through match rules, and verifies the sender when the rule names a
  well-known bus name.
- Registers method handlers so your process can expose D-Bus objects, answering
  `org.freedesktop.DBus.Peer.Ping`, `.GetMachineId` and introspection requests for you.
- Reads and writes every D-Bus wire type: the basic types, arrays, dictionaries, structs, variants,
  object paths, signatures and Unix file descriptors.
- Generates D-Bus introspection XML for the object paths you registered.
- Monitors bus traffic with `BecomeMonitorAsync` and the static `MonitorBusAsync` async-enumerable,
  and lists what is on the bus with `ListServicesAsync()` and `ListActivatableServicesAsync()`.
- Pools messages and buffers, and offers span-based access to header fields, so a busy connection
  can run without allocating per message.
- Ships XML documentation (IntelliSense) alongside the assembly, and brings in no NuGet dependencies
  of its own.

Every public type lives in the `CodeBrix.Platform.LinuxDBus` namespace, no matter which source
sub-folder it came from - there are no sub-namespaces, and one `using` covers the whole surface.

## When to use it

Reach for this package when you need to speak D-Bus from .NET on Linux and you want the wire
protocol itself: calling a method on `systemd`, watching a signal from a desktop service, exposing an
object of your own on the session bus, or passing a Unix file descriptor to another process. It is
also the layer to build on when you want to put your own typed API over a specific D-Bus interface.

What it deliberately does not do:

- **No proxy generation and no high-level client.** There is no source generator, no
  interface-attribute model, no `Task<T> GetPropertyAsync()` style generated members. Every message
  is written and read by hand.
- **No property or object-manager helpers.** `org.freedesktop.DBus.Properties` and
  `org.freedesktop.DBus.ObjectManager` are ordinary interfaces you call and implement yourself; only
  their introspection XML is provided.
- **No name-ownership API.** `RequestName` and `ReleaseName` are not wrapped; call them on
  `org.freedesktop.DBus` yourself.
- **No bus daemon.** It does not start, spawn or embed `dbus-daemon`, and it does not implement the
  bus side of the protocol - only the peer side, plus the `Peer` and `Introspectable` replies.
- **No server-side listening socket.** It connects out to an address; it does not accept inbound
  D-Bus connections.
- **Not a general IPC library.** It is D-Bus, and Linux D-Bus at that: no Windows named-pipe
  transport story, no strong-name signing, no netstandard target.
- **No automatic reconnect semantics beyond `AutoConnect`**, and no message queueing while
  disconnected - `TrySendMessage` returns false.

Only one assembly defining these type names may be referenced by a project; two assemblies that
declare the same types produce ambiguous-reference build errors.

## Getting started

```bash
dotnet add package CodeBrix.Platform.LinuxDBus.MitLicenseForever
```

One `using` covers the entire public surface:

```csharp
using CodeBrix.Platform.LinuxDBus;
```

Typical code also needs a few base class library namespaces:

```csharp
using System;                            // IDisposable, Exception
using System.Collections.Generic;        // Dictionary<,>, KeyValuePair<,>
using System.Threading.Tasks;            // Task, ValueTask
using System.Runtime.InteropServices;    // SafeHandle (for Unix fds)
```

There is no registration call, no codec registration and no feature flags. The whole setup is
constructing a `Connection` and calling `ConnectAsync()`. This example connects to the session bus
and calls `org.freedesktop.DBus.ListNames`, decoding the reply with the reader delegate passed to the
generic `CallMethodAsync` overload:

```csharp
using System;
using System.Threading.Tasks;
using CodeBrix.Platform.LinuxDBus;

public static class ListNamesExample
{
    public static async Task RunAsync()
    {
        string address = Address.Session;
        if (address == null)
        {
            Console.WriteLine("No session bus available.");
            return;
        }

        using var connection = new Connection(address);
        await connection.ConnectAsync();

        string[] names = await connection.CallMethodAsync(
            CreateMessage(connection),
            (Message message, object state) =>
                message.GetBodyReader().ReadArrayOfString());

        foreach (string name in names)
        {
            Console.WriteLine(name);
        }
    }

    // A local/static helper keeps the ref struct writer out of the
    // async method body.
    private static MessageBuffer CreateMessage(Connection connection)
    {
        using var writer = connection.GetMessageWriter();
        writer.WriteMethodCallHeader(
            destination: Connection.DBusServiceName,
            path:        Connection.DBusObjectPath,
            @interface:  Connection.DBusInterface,
            member:      "ListNames");
        return writer.CreateMessage();
    }
}
```

Notice the split: the message is built in a plain, non-async helper because `MessageWriter` is a
`ref struct` and cannot appear in an `async` method body, while the reply is decoded inside the
delegate because `Reader` is a `ref struct` too. That split shapes almost every program written
against this library.

The smallest complete project is a console application whose csproj carries one package reference
and nothing else:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>disable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference
        Include="CodeBrix.Platform.LinuxDBus.MitLicenseForever" />
  </ItemGroup>
</Project>
```

With top-level statements, the whole program is this:

```csharp
using System;
using System.Threading.Tasks;
using CodeBrix.Platform.LinuxDBus;

string address = Address.Session;
if (address == null)
{
    Console.Error.WriteLine("No session bus (DBUS_SESSION_BUS_ADDRESS).");
    return 1;
}

using var connection = new Connection(address);
await connection.ConnectAsync();

Console.WriteLine($"connected as {connection.UniqueName}");

foreach (string name in await connection.ListServicesAsync())
{
    Console.WriteLine(name);
}

return 0;
```

Run it on a Linux desktop session, or under `dbus-run-session -- dotnet run`, and it prints every
name currently on the session bus.

## Key concepts

### Connection

`Connection` is the main entry point: `public partial class Connection : IDisposable`. It owns the
transport, the read and write pipeline, and the method-dispatch table. It implements `IDisposable`,
**not** `IAsyncDisposable` - use `using var` or call `connection.Dispose()`; there is no
`DisposeAsync`.

Two constructors are public: `Connection(string address)` and
`Connection(ConnectionOptions connectionOptions)`. The options overload requires a
`ClientConnectionOptions` instance, because `ConnectionOptions` is an abstract base with an internal
constructor.

Lifetime is `ConnectAsync()`, then work, then `Dispose()`:

```csharp
public async ValueTask ConnectAsync()
public void Dispose()
public Task<Exception> DisconnectedAsync()
```

`ConnectAsync()` takes no `CancellationToken` and, unless `AutoConnect` is set, may be called only
once; a second call throws `InvalidOperationException`. Failure throws `ConnectException`,
`DisconnectedException` or `ObjectDisposedException`. `DisconnectedAsync()` completes with the reason
the connection ended, which is how a hosting process waits for the bus to drop it.

Two shared, process-wide, auto-connecting connections are available as
`public static Connection System { get; }` and `public static Connection Session { get; }`. Use them
when several components in one process each want a bus connection - one socket instead of many.
`public string UniqueName { get; }` returns this connection's unique bus name, for example `":1.42"`.

The three bus constants save you from typing the well-known strings:

```csharp
public const string DBusObjectPath  = "/org/freedesktop/DBus";
public const string DBusServiceName = "org.freedesktop.DBus";
public const string DBusInterface   = "org.freedesktop.DBus";
```

### Ref structs and pooled messages

Two types drive every message: `MessageWriter` and `Reader`. Both are `ref struct`s, so they live on
the stack only.

> [!IMPORTANT]
> A `ref struct` cannot be a field, cannot be captured by a lambda, cannot be boxed, cannot cross an
> `await`, and cannot be declared in an `async` method body. Put message construction and message
> decoding in ordinary, non-async local functions or methods.

Messages and their buffers are pooled, which imposes two more rules. A `MessageBuffer` is opaque and
single-use: sending it transfers ownership and returns it to the pool, so never send the same buffer
twice, and never send a buffer built by one connection's writer over a different connection - the
buffer carries the serial number of the connection that issued the writer. A received `Message` is
not `IDisposable` and must never be stored: it is valid only for the duration of the callback that
received it, after which it goes back to the pool and its contents are recycled. Copy out the values
you need.

### Building a message

Every outgoing message starts at `connection.GetMessageWriter()`. Wrap the writer in `using`, write
exactly one header call, then write the body values in order, then call `CreateMessage()`:

```csharp
using var writer = connection.GetMessageWriter();
writer.WriteMethodCallHeader(
    destination: "com.example.Service",
    path:        "/com/example/Object",
    @interface:  "com.example.StringOperations",
    member:      "Concat",
    signature:   "ss");
writer.WriteString(lhs);
writer.WriteString(rhs);
return writer.CreateMessage();
```

The header calls are `WriteMethodCallHeader`, `WriteMethodReturnHeader`, `WriteError` and
`WriteSignalHeader`; exactly one comes first, before any body value. The `signature` argument is the
D-Bus signature of the body you are about to write - `"s"` for one string, `"ss"` for two, `"a{sv}"`
for a string-to-variant map, omitted for an empty body. `CreateMessage()` finalizes lengths and
transfers ownership of the buffer to you, leaving the writer empty; if you abandon a writer without
calling it, `Dispose()` returns the pooled buffer. After a successful `CreateMessage()` the extra
`Dispose()` is a no-op, so `using` is always right.

The value writes cover the whole wire vocabulary: `WriteByte`, `WriteBool`, `WriteInt16`,
`WriteUInt16`, `WriteInt32`, `WriteUInt32`, `WriteInt64`, `WriteUInt64`, `WriteDouble`,
`WriteString`, `WriteObjectPath`, `WriteSignature` and `WriteHandle(SafeHandle)`, each with a
variant-wrapping twin (`WriteVariantByte` through `WriteVariantSignature`, plus
`WriteVariantHandle` and `WriteVariant(VariantValue)`) that emits the signature byte and the value in
one call. A whole-collection array overload exists for every element type, in three shapes: `T[]`,
`ReadOnlySpan<T>` and `IEnumerable<T>`.

When the shape is not one of the ready-made ones, build it by hand. `WriteArrayStart(DBusType)`
returns an `ArrayStart` token that `WriteArrayEnd(start)` closes, and `WriteStructureStart()` opens a
struct - it is also how you write a bare, non-array struct. Here is a body with signature `"a(sou)"`:

```csharp
writer.WriteMethodCallHeader(..., signature: "a(sou)");
ArrayStart start = writer.WriteArrayStart(DBusType.Struct);
foreach (var entry in entries)
{
    writer.WriteStructureStart();
    writer.WriteString(entry.Name);
    writer.WriteObjectPath(entry.Path);
    writer.WriteUInt32(entry.Id);
}
writer.WriteArrayEnd(start);
```

Dictionaries have the same pairing - `WriteDictionaryStart`, `WriteDictionaryEnd` and
`WriteDictionaryEntryStart` - plus ready-made `WriteDictionary` overloads that cover the very common
`a{sv}` type for `Dictionary<string, VariantValue>`, `KeyValuePair<string, VariantValue>[]` and
`IEnumerable<KeyValuePair<string, VariantValue>>`. Introspection XML has its own writer,
`WriteIntrospectionXml(scoped ReadOnlySpan<ReadOnlyMemory<byte>> interfaceXmls, IEnumerable<string> childNames)`.

### Reading a reply

`Message` is the received message. Its header is exposed as `IsBigEndian`, `Serial`, `MessageFlags`,
`MessageType`, `ReplySerial` and `UnixFdCount`, and its body through `GetBodyReader()`. Header string
fields come in three flavors - as a decoded string, as raw UTF-8 bytes for allocation-free
comparisons, and as a presence flag: `PathAsString`, `Path` and `PathIsSet`, and the same triple for
`Interface`, `Member`, `ErrorName`, `Destination`, `Sender` and `Signature`.

`Reader` walks the body in signature order. There is no seeking and no "read field by name". It has
the matching set of basic reads, whole-array reads (`ReadArrayOfByte` through
`ReadArrayOfVariantValue`, and `ReadArrayOfHandle<T>() where T : SafeHandle, new()`), dictionary
reads (`ReadDictionaryStart()`, `ReadDictionaryOfStringToVariantValue()`) and variant reads
(`ReadVariantValue()`). Element-by-element reading uses `ReadArrayStart(DBusType)`,
`HasNext(ArrayEnd)`, `SkipTo(ArrayEnd)` and `AlignStruct()`:

```csharp
(string Name, string Path, uint Id)[] ReadEntries(Message message)
{
    var entries = new List<(string, string, uint)>();
    Reader reader = message.GetBodyReader();

    ArrayEnd end = reader.ReadArrayStart(DBusType.Struct);
    while (reader.HasNext(end))
    {
        string name = reader.ReadString();
        string path = reader.ReadObjectPathAsString();
        uint   id   = reader.ReadUInt32();
        entries.Add((name, path, id));
    }

    return entries.ToArray();
}
```

The two `void ReadSignature(expected)` overloads read a signature and throw `ProtocolException` when
it is not the one you expected - an inexpensive way to validate a variant's inner type before
decoding it. When the shape is genuinely unknown until run time, `SignatureReader` walks a signature
one complete type at a time through
`bool TryRead(out DBusType type, out ReadOnlySpan<byte> innerSignature)`.

Decoding always happens inside `public delegate T MessageValueReader<T>(Message message, object state);`,
the only public delegate type in the package. It is used by both `CallMethodAsync<T>` and
`AddMatchAsync<T>`, and because `Reader` is a `ref struct`, all decoding must happen inside it - you
cannot return a `Reader` from it or capture one outside it.

### Calling a method

There are two call overloads and one fire-and-forget send:

```csharp
public async Task CallMethodAsync(MessageBuffer message)
public async Task<T> CallMethodAsync<T>(
    MessageBuffer message, MessageValueReader<T> reader, object readerState = null)
public bool TrySendMessage(MessageBuffer message)
```

The non-generic overload returns `Task` and gives you no reply object; reading a reply requires the
generic overload plus a `MessageValueReader<T>`. When the peer replies with an error, the returned
task faults with a `DBusException` carrying `ErrorName` and `ErrorMessage`. `TrySendMessage` returns
false, and returns the buffer to the pool, when the connection is not currently connected.

### Subscribing to signals

A `MatchRule` describes the traffic you want. Every property - `Type`, `Sender`, `Interface`,
`Member`, `Path`, `PathNamespace`, `Destination`, `Arg0`, `Arg0Path`, `Arg0Namespace` - is optional,
and the ones you set are ANDed together. Per the D-Bus specification `Path` and `PathNamespace` may
not both be used on one rule, and neither may `Arg0`, `Arg0Path` and `Arg0Namespace`; the class does
not enforce that, so do not set both. `ToString()` returns the rule string exactly as it is sent to
`org.freedesktop.DBus.AddMatch`, with every key spelled as the specification defines it, including
`path_namespace`, `arg0path` and `arg0namespace`.

`AddMatchAsync` has four overloads; prefer `AddMatchAsync(rule, reader, handler, flags)`:

```csharp
public ValueTask<IDisposable> AddMatchAsync<T>(
    MatchRule rule, MessageValueReader<T> reader,
    Action<Exception, T, object, object> handler,
    ObserverFlags flags,
    object readerState = null, object handlerState = null,
    bool emitOnCapturedContext = true)
```

The first overload in the list, the one without an `ObserverFlags` parameter, is marked
`[Obsolete("Use an overload that accepts ObserverFlags.")]`. Dispose the returned `IDisposable` to
unsubscribe. The handler's four arguments are `(exception, value, readerState, handlerState)`;
`exception` is null for a normal delivery and non-null for a terminal notification.
`emitOnCapturedContext: true` captures `SynchronizationContext.Current` at subscribe time and
delivers on it; `false` delivers on the connection's own read loop.

`ObserverFlags` controls the terminal notifications and the subscription itself:

```csharp
[Flags]
public enum ObserverFlags
{
    None = 0,
    EmitOnConnectionDispose = 1,
    EmitOnObserverDispose = 2,
    NoSubscribe = 4,
    EmitOnDispose = EmitOnConnectionDispose | EmitOnObserverDispose,
}
```

`EmitOnConnectionDispose` calls your handler with an exception when the connection is disposed,
`EmitOnObserverDispose` does the same when you dispose the subscription, and `NoSubscribe` registers
the match locally without sending `AddMatch` to the bus - use it when another subscription already
covers the traffic. When the peer is not a bus, `NoSubscribe` is applied automatically.

### Exposing an object on the bus

Implement `IMethodHandler` and register it with `connection.AddMethodHandler(...)`:

```csharp
public interface IMethodHandler
{
    string Path { get; }
    ValueTask HandleMethodAsync(MethodContext context);
    bool RunMethodHandlerSynchronously(Message message);
}
```

`Path` must start with `/` and contain no empty segment; a path that does not, or that contains
`//`, makes `AddMethodHandler` throw `FormatException`. There is one handler per path: registering a
second handler for a path that already has one throws `InvalidOperationException`. Call
`RemoveMethodHandler(path)` first to replace one. The companion members are `AddMethodHandlers`,
`RemoveMethodHandler` and `RemoveMethodHandlers`.

`RunMethodHandlerSynchronously(message)` is asked before every dispatch. `true`, the usual answer,
makes the connection await your handler before reading the next message, which blocks the read loop;
`false` runs the handler detached, which is what you want for a handler that awaits I/O.

`MethodContext` carries the request and the reply machinery: `Request`, `Connection`,
`RequestAborted`, `ReplySent`, `NoReplyExpected`, `IsDBusIntrospectRequest`,
`CreateReplyWriter(string signature)`, `Reply(MessageBuffer)`,
`ReplyError(string errorName = null, string errorMsg = null)` and
`ReplyIntrospectXml(ReadOnlySpan<ReadOnlyMemory<byte>>)`. `context.Request` is valid only for the
duration of the call - never store it. Calling `Reply` a second time throws
`InvalidOperationException`, and you should check `NoReplyExpected` first and not build or send a
reply when it is true. `RequestAborted` is canceled when the connection goes away, which is how a
long-running handler learns to stop. `ReplyIntrospectXml` may be called only while
`IsDBusIntrospectRequest` is true.

An error reply is one call:

```csharp
context.ReplyError("com.example.Error.InvalidArgument",
                   "count must be greater than zero");
```

The caller's `CallMethodAsync` then throws a `DBusException` with exactly those values.

If your handler returns without sending a reply, the library answers for you:
`org.freedesktop.DBus.Peer.Ping` and `.GetMachineId` are answered automatically, an introspection
request gets the generated XML, and anything else gets `org.freedesktop.DBus.Error.UnknownMethod`. An
unhandled member therefore does not hang the caller. Ready-made interface XML blocks are available
for the standard interfaces:

```csharp
public static class IntrospectionXml
{
    public static ReadOnlyMemory<byte> DBusProperties { get; }
    public static ReadOnlyMemory<byte> DBusIntrospectable { get; }
    public static ReadOnlyMemory<byte> DBusPeer { get; }
}
```

### Variants and the typed container helpers

`VariantValue` is the owned, readable representation of a D-Bus variant, and it is the type to use
for both reading and writing. Its shape is described by `Type`, `ItemType` for arrays, `KeyType` and
`ValueType` for dictionaries, and `Count` for arrays, structs and dictionaries. The typed getters -
`GetByte`, `GetBool`, `GetInt16`, `GetUInt16`, `GetInt32`, `GetUInt32`, `GetInt64`, `GetUInt64`,
`GetDouble`, `GetString`, `GetObjectPathAsString`, `GetSignature`, `GetVariantValue()` and
`ReadHandle<T>()` - each throw when `Type` does not match, so check the shape first. Containers are
reached with `GetItem(int i)`, `GetDictionaryEntry(int i)`, `GetArray<T>()` and
`GetDictionary<TKey, TValue>()`.

Construction is by implicit conversion from the CLR types, or by the explicit factories `Byte`,
`Bool`, `Int16`, `UInt16`, `Int32`, `UInt32`, `Int64`, `UInt64`, `Double`, `String`, `ObjectPath`,
`Signature`, `UnixFd(SafeHandle)`, `Variant(VariantValue)` (which nests one level), `Struct(...)` and
`Array(...)` per element type plus `ArrayOfVariant`. There is no public `VariantValue.Dictionary(...)`
factory: `VariantValue` reads dictionaries but does not construct them. Writing an `a{sv}` body
therefore builds an ordinary `Dictionary<string, VariantValue>` and hands it to `WriteDictionary`:

```csharp
private static MessageBuffer CreateSetPropertiesMessage(
    Connection connection)
{
    var properties = new Dictionary<string, VariantValue>
    {
        ["Name"]    = VariantValue.String("example"),
        ["Count"]   = VariantValue.Int32(3),
        ["Enabled"] = VariantValue.Bool(true),
        ["Tags"]    = VariantValue.Array(
                          new[] { "alpha", "beta" }),
    };

    using var writer = connection.GetMessageWriter();
    writer.WriteMethodCallHeader(
        destination: "com.example.Service",
        path:        "/com/example/Object",
        @interface:  "com.example.Config",
        member:      "Apply",
        signature:   "a{sv}");
    writer.WriteDictionary(properties);
    return writer.CreateMessage();
}
```

Reading one back switches on `VariantValueType`, whose members share `DBusType`'s character values
except `Dictionary`, which is `DictEntry`:

```csharp
foreach (KeyValuePair<string, VariantValue> entry in result)
{
    VariantValue value = entry.Value;
    switch (value.Type)
    {
        case VariantValueType.String:
            Console.WriteLine($"{entry.Key} = {value.GetString()}");
            break;
        case VariantValueType.UInt32:
            Console.WriteLine($"{entry.Key} = {value.GetUInt32()}");
            break;
        case VariantValueType.Bool:
            Console.WriteLine($"{entry.Key} = {value.GetBool()}");
            break;
        case VariantValueType.Array
                when value.ItemType == VariantValueType.String:
            Console.WriteLine(
                $"{entry.Key} = " +
                $"[{string.Join(", ", value.GetArray<string>())}]");
            break;
        default:
            // ToString() renders the value with a type suffix
            Console.WriteLine($"{entry.Key} = {value}");
            break;
    }
}
```

`Array<T>`, `Dict<TKey, TValue>` and `Struct<...>` wrap ordinary collections so they can be handed to
the writer or converted to a `VariantValue` without hand-rolling the wire layout. `Array<T>`
implements `IDBusWritable`, `IList<T>` and `IVariantValueConvertable`; `Dict<TKey, TValue>` does the
same over `IDictionary<TKey, TValue>`; the static `Struct` class offers `Create<T1>` through
`Create<T1, ..., T10>`, and the `Struct<...>` types carry public fields `Item1` through `ItemN`. The
element and field types are checked at construction time, so an unsupported type throws immediately
rather than at write time. Calling `AsVariantValue()` on an `Array<T>` freezes it: every later
mutation throws `InvalidOperationException`, so build it fully and then convert.

`IDBusWritable` is implemented explicitly by `Array<T>`, `Dict<,>` and `Struct<...>` - cast to the
interface to call `WriteTo` directly, and implement it on your own type to make that type writable
the same way:

```csharp
public interface IVariantValueConvertable { VariantValue AsVariantValue(); }
public interface IDBusWritable { void WriteTo(ref MessageWriter writer); }
```

`Variant` is the older, lightweight, write-only variant view. It is marked obsolete in Release builds
with the message "Variant will be removed. Use the VariantValue type instead.", so consuming it
produces CS0618 in a Release build, and every `AsVariant()` member produces that type. New code uses
`VariantValue` and `AsVariantValue()`.

### Addresses and connection options

`Address` resolves the two well-known bus addresses:

```csharp
public static class Address
{
    public static string System  { get; }
    public static string Session { get; }
}
```

`System` returns `$DBUS_SYSTEM_BUS_ADDRESS`, falling back to
`unix:path=/var/run/dbus/system_bus_socket` on non-Windows hosts. `Session` returns
`$DBUS_SESSION_BUS_ADDRESS`, falling back to the X11 selection lookup, which needs `$DISPLAY` and
`libX11`. Either may return null when the bus cannot be located, and `new Connection(null)` throws
`ArgumentNullException`, so check first. Both values are resolved once and cached for the life of the
process.

`ClientConnectionOptions` is the configurable form of a connection:

```csharp
public class ClientConnectionOptions : ConnectionOptions
{
    public ClientConnectionOptions(string address)
    protected internal ClientConnectionOptions()
    public bool AutoConnect { get; set; }
    protected internal virtual ValueTask<ClientSetupResult> SetupAsync(
        CancellationToken cancellationToken)
    protected internal virtual void Teardown(object token)
}
```

With `AutoConnect = true` the first operation connects on demand and later operations reconnect after
a drop. The cost is that `UniqueName`, `TrySendMessage`, `DisconnectedAsync`, `AddMethodHandler`,
`AddMethodHandlers`, `RemoveMethodHandler` and `RemoveMethodHandlers` all throw
`InvalidOperationException("Method cannot be used on autoconnect connections.")`. Use `AutoConnect`
for pure client call-and-subscribe code, and a plain non-`AutoConnect` connection whenever you host
objects or send raw messages. The shared `Connection.Session` and `Connection.System` are
auto-connecting, so the same restriction applies to them; `BecomeMonitorAsync` additionally refuses
shared connections.

Overriding `SetupAsync` lets you supply a `ClientSetupResult`, whose members are
`ConnectionAddress`, `TeardownToken`, `UserId`, `MachineId`, `SupportsFdPassing` and
`ConnectionStream`. When `ConnectionStream` is set, `ConnectionAddress` and `SupportsFdPassing` are
ignored and the library uses your stream - which is how you drive the protocol over a transport you
opened yourself.

### Errors and what a throw inside a callback does

Four exception types cover the failure modes. `DBusException(string errorName, string errorMessage)`
carries a peer's error reply and exposes `ErrorName` and `ErrorMessage`. `ConnectException` reports a
failure to connect. `DisconnectedException` is thrown when the connection is closed, including for
calls that were still in flight, and its `InnerException` is the reason. `ProtocolException` reports
malformed or unexpected wire data - a bad signature, a truncated body.

> [!WARNING]
> Exceptions from your callbacks do not crash the process - they end the connection. A throw inside
> your `MessageValueReader<T>`, inside a signal handler, or out of `IMethodHandler.HandleMethodAsync`
> is caught, the connection is disconnected with that exception as the reason, and your handler is
> then invoked once more with a `DisconnectedException` whose `InnerException` is what you threw.
> Guard your callbacks.

Inside a signal handler, `ActionException` tells an orderly shutdown from a real failure without
matching on exception types or messages:

```csharp
public static class ActionException
{
    public static bool IsObserverDisposed(Exception exception);
    public static bool IsConnectionDisposed(Exception exception);
    public static bool IsDisposed(Exception exception);
}
```

### Security behavior you can rely on

When `MatchRule.Sender` names a well-known bus name, the connection resolves it to its current
unique-name owner, tracks `NameOwnerChanged`, and delivers a signal only when the actual unique-name
sender matches. A hostile peer therefore cannot impersonate a well-known name by forging the `Sender`
header. The consequence is a rule of thumb: always set `MatchRule.Sender` when you care who sent a
signal, because leaving it unset accepts signals from anyone.

Unix file descriptors are capped as the D-Bus specification requires, at most sixteen per message.
Received descriptors are made close-on-exec at receipt, truncated control messages are rejected, and
leftover descriptors are never carried from one message into the next. If you need to pass more
handles than the cap allows, split the work across several messages. `ReadHandle<T>` takes ownership
of a descriptor and gives you a `SafeHandle` you must dispose; `ReadHandleRaw()` gives you the raw
descriptor without transferring ownership.

### Monitoring the bus

Two entry points read traffic that is not addressed to you:

```csharp
public async Task BecomeMonitorAsync(
    Action<Exception, DisposableMessage> handler,
    IEnumerable<MatchRule> rules = null)

public static async IAsyncEnumerable<DisposableMessage> MonitorBusAsync(
    string address, IEnumerable<MatchRule> rules = null,
    CancellationToken ct = default)
```

`BecomeMonitorAsync` turns an existing connection into a monitor and throws
`InvalidOperationException` on a shared connection. `MonitorBusAsync` opens its own connection to an
address and yields messages. Both hand each message to you inside a
`struct DisposableMessage : IDisposable`, whose `Message` property is the message and whose
`Dispose()` releases it back to the pool - dispose it when you are done with it.

### Keeping a busy connection allocation-free

- Read header fields as spans when you are only comparing them.
  `message.Interface.SequenceEqual("com.example.Foo"u8)` allocates nothing, while
  `message.InterfaceAsString == "com.example.Foo"` allocates a string on every message.
- Prefer the bulk array reads and writes - `ReadArrayOfInt32`, `WriteArray(ReadOnlySpan<int>)` - over
  element-by-element loops.
- Pass state through `readerState` and `handlerState` instead of capturing it in a lambda. The
  delegates are then cacheable static lambdas, and no closure is allocated per call or per signal.
- Set `emitOnCapturedContext: false` on subscriptions that do not need a particular thread.
  Delivering on a captured `SynchronizationContext` uses a blocking `Send`, which stalls the
  connection's read loop until your context runs the callback.
- Return `false` from `RunMethodHandlerSynchronously` for handlers that await I/O. Returning `true`
  keeps the read loop parked on your handler, so one slow method call delays every other message on
  the connection.
- Reuse one `Connection` for the lifetime of your component. Connecting performs a SASL handshake and
  a `Hello` round-trip; it is not something to do per call.

## Examples

Subscribing to `org.freedesktop.DBus.NameOwnerChanged` shows the full subscription shape: a
`MatchRule`, a reader that decodes the `"sss"` body into a tuple, and the four-argument handler that
distinguishes a delivery from a terminal notification.

```csharp
using System;
using System.Threading.Tasks;
using CodeBrix.Platform.LinuxDBus;

public static class SignalExample
{
    public static async Task<IDisposable> WatchNameOwnerChangedAsync(
        Connection connection)
    {
        var rule = new MatchRule
        {
            Type      = MessageType.Signal,
            Sender    = Connection.DBusServiceName,
            Path      = Connection.DBusObjectPath,
            Interface = Connection.DBusInterface,
            Member    = "NameOwnerChanged",
        };

        return await connection.AddMatchAsync(
            rule,
            // reader: decode the "sss" body into a tuple
            (Message message, object state) =>
            {
                Reader reader = message.GetBodyReader();
                string name     = reader.ReadString();
                string oldOwner = reader.ReadString();
                string newOwner = reader.ReadString();
                return (name, oldOwner, newOwner);
            },
            // handler: (exception, value, readerState, handlerState)
            (Exception exception,
             (string Name, string Old, string New) value,
             object readerState,
             object handlerState) =>
            {
                if (exception != null)
                {
                    if (!ActionException.IsDisposed(exception))
                    {
                        Console.WriteLine(
                            $"subscription ended: {exception.Message}");
                    }
                    return;
                }

                Console.WriteLine(
                    $"{value.Name}: '{value.Old}' -> '{value.New}'");
            },
            ObserverFlags.EmitOnDispose);
    }
}
```

Exposing an object is one `IMethodHandler`: it decides which member the request names, decodes the
body, and replies through a writer the context creates.

```csharp
using System;
using System.Threading.Tasks;
using CodeBrix.Platform.LinuxDBus;

public sealed class StringOperations : IMethodHandler
{
    public const string InterfaceName = "com.example.StringOperations";

    public string Path => "/com/example/StringOperations";

    // true: the connection awaits this handler before reading the next
    // message. Return false to let the read loop continue while a slow
    // handler runs.
    public bool RunMethodHandlerSynchronously(Message message) => true;

    public ValueTask HandleMethodAsync(MethodContext context)
    {
        Message request = context.Request;

        if (request.InterfaceAsString == InterfaceName &&
            request.MemberAsString == "Concat" &&
            request.SignatureAsString == "ss")
        {
            return ConcatAsync(context);
        }

        // Returning without replying makes the library answer with
        // org.freedesktop.DBus.Error.UnknownMethod.
        return default;
    }

    private ValueTask ConcatAsync(MethodContext context)
    {
        Reader reader = context.Request.GetBodyReader();
        string lhs = reader.ReadString();
        string rhs = reader.ReadString();

        using var writer = context.CreateReplyWriter("s");
        writer.WriteString(lhs + rhs);
        context.Reply(writer.CreateMessage());

        return default;
    }
}
```

Hosting that handler takes a plain, explicitly connected connection - not a shared one - and a
well-known bus name, which you take by calling `RequestName` on the bus yourself.

```csharp
using var connection = new Connection(Address.Session);
await connection.ConnectAsync();

connection.AddMethodHandler(new StringOperations());

await connection.CallMethodAsync(CreateRequestName(
    connection, "com.example.Service"));

// keep the process alive until the bus drops us
Exception reason = await connection.DisconnectedAsync();

static MessageBuffer CreateRequestName(Connection connection,
                                       string name)
{
    using var writer = connection.GetMessageWriter();
    writer.WriteMethodCallHeader(
        destination: Connection.DBusServiceName,
        path:        Connection.DBusObjectPath,
        @interface:  Connection.DBusInterface,
        member:      "RequestName",
        signature:   "su");
    writer.WriteString(name);
    writer.WriteUInt32(0);      // flags: none
    return writer.CreateMessage();
}
```

Reading a property from a system-bus service goes through `org.freedesktop.DBus.Properties.Get`,
whose reply body is a single variant.

```csharp
using System;
using System.Threading.Tasks;
using CodeBrix.Platform.LinuxDBus;

public static class PropertyExample
{
    // Returns e.g. the hostname from systemd-hostnamed.
    public static async Task<string> GetHostnameAsync()
    {
        using var connection = new Connection(Address.System);
        await connection.ConnectAsync();

        VariantValue value = await connection.CallMethodAsync(
            CreateGetMessage(connection,
                destination: "org.freedesktop.hostname1",
                path:        "/org/freedesktop/hostname1",
                @interface:  "org.freedesktop.hostname1",
                property:    "Hostname"),
            (Message m, object s) =>
                m.GetBodyReader().ReadVariantValue());

        return value.GetString();
    }

    private static MessageBuffer CreateGetMessage(
        Connection connection, string destination, string path,
        string @interface, string property)
    {
        using var writer = connection.GetMessageWriter();
        writer.WriteMethodCallHeader(
            destination: destination,
            path:        path,
            @interface:  "org.freedesktop.DBus.Properties",
            member:      "Get",
            signature:   "ss");
        writer.WriteString(@interface);   // interface owning the property
        writer.WriteString(property);     // property name
        return writer.CreateMessage();
    }
}
```

## Using it in a CodeBrix.Platform application

There is no add-in and no service registration: this is a standalone library, referenced and used the
same way from a CodeBrix.Platform Linux head as from a console application. Add the package to the
project that needs it, construct a `Connection`, and keep it for the lifetime of the component that
owns it.

The one detail that matters in a UI application is where signal handlers run. Subscribing with
`emitOnCapturedContext: true` captures `SynchronizationContext.Current` at subscribe time and
delivers each signal on it, so subscribing from the UI thread delivers to the UI thread. That
convenience uses a blocking `Send`, which stalls the connection's read loop until your context runs
the callback, so use it only for the subscriptions that genuinely need it and marshal the rest
yourself.

## Pitfalls

- `CallMethodAsync(MessageBuffer)` returns `Task`, not a reply. To read a reply you must call
  `CallMethodAsync<T>(MessageBuffer, MessageValueReader<T>, object readerState = null)`.
- `Connection` is `IDisposable`, not `IAsyncDisposable`.
  `await using var connection = new Connection(...)` does not compile, and there is no
  `DisposeAsync()`.
- There is no `Connection.CreateMessage(...)`. Messages are built with `connection.GetMessageWriter()`
  plus a header-writing call.
- `Reader` and `MessageWriter` are `ref struct`s. They cannot be stored in a field, captured by a
  lambda, boxed, used across an `await`, or declared in an `async` method. Put message construction
  and message decoding in ordinary, non-async local functions or methods.
- `Message` is not `IDisposable` and must not be stored. It is valid only inside the callback that
  received it; afterwards it returns to the pool and its contents are recycled. Copy out the values
  you need.
- A `MessageBuffer` is single-use. Sending it transfers ownership, so do not send the same buffer
  twice, and do not send a buffer created from one connection's writer over a different connection -
  the serial number belongs to the connection that issued the writer.
- Always wrap the writer in `using`. If you build a message and then throw, or decide not to send,
  the pooled buffer is only reclaimed by `Dispose()`. After a successful `CreateMessage()` the extra
  `Dispose()` is a no-op.
- The signature must match the body. The header's `signature` argument is not validated against what
  you subsequently write; a mismatch produces a message that the peer rejects, or worse, silently
  misreads.
- A throw inside your callback tears down the connection - in the `MessageValueReader<T>`, in the
  signal handler, and out of `IMethodHandler.HandleMethodAsync`. Guard your callbacks.
- Auto-connecting connections refuse several members. On a connection created with
  `AutoConnect = true` - which includes the shared `Connection.Session` and `Connection.System` -
  `UniqueName`, `TrySendMessage`, `DisconnectedAsync`, `AddMethodHandler(s)` and
  `RemoveMethodHandler(s)` throw
  `InvalidOperationException("Method cannot be used on autoconnect connections.")`. Host objects on a
  plain connection you connected explicitly. `BecomeMonitorAsync` additionally refuses shared
  connections.
- `ConnectAsync()` may be called only once unless `AutoConnect` is set; a second call throws
  `InvalidOperationException`. It takes no `CancellationToken`.
- `Address.Session` and `Address.System` can return null, and `new Connection(null)` throws
  `ArgumentNullException`. Check before constructing. Both are cached on first access, so setting
  `DBUS_SESSION_BUS_ADDRESS` after the first read has no effect.
- A `MatchRule` with no `Sender` accepts signals from every peer. Sender verification protects you
  only for the name you actually specify.
- `Array<T>.AsVariantValue()` freezes the array; later `Add`, `Clear` and indexer-set calls throw
  `InvalidOperationException`.
- `Variant` is obsolete in Release builds. Likewise `Signature(string)` is obsolete - use the
  `ReadOnlySpan<byte>` constructor with a `"..."u8` literal, as in `new Signature("a{sv}"u8)` - and
  `VariantValue.GetObjectPath()` is obsolete in favor of `GetObjectPathAsString()`. Note that the
  header writers take signatures as plain `string`, so the `Signature` constructor matters only when
  you materialize a `Signature` value yourself.
- `VariantValue` getters are type-checked and throw. Check `Type`, and `ItemType`, `KeyType` or
  `ValueType` for containers, before calling `GetString()`, `GetInt32()`, `GetArray<T>()` and friends.
- At most sixteen Unix file descriptors per message, and only when the transport supports descriptor
  passing. The `SafeHandle`s you receive are yours to dispose.
- Linux only, in practice. The package compiles and loads anywhere, but it needs a D-Bus daemon and
  it calls into `libc`, and into `libX11` when resolving the session address without an environment
  variable.
- `AddMethodHandler` does not replace an existing handler - it throws `InvalidOperationException`
  when the path is already registered. Call `RemoveMethodHandler(path)` first. A path that does not
  start with `/`, or that contains `//`, throws `FormatException`.
- Match-rule keys are emitted per specification. `MatchRule.ToString()` writes `path_namespace`,
  `arg0path` and `arg0namespace` - the D-Bus key spellings - and a bus rejects `pathNamespace`-style
  keys with a `DBusException`.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Platform.LinuxDBus/blob/main/README.md) |
| Complete API reference and usage guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.LinuxDBus/blob/main/AGENT-README.txt) |
| Samples, tools and other non-package content | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Platform.LinuxDBus/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform.LinuxDBus/blob/main/README-INDEX.txt) |
| Tests (worked examples of every consumer scenario) | [tests/CodeBrix.Platform.LinuxDBus.Tests](https://github.com/ellisnet/CodeBrix.Platform.LinuxDBus/tree/main/tests/CodeBrix.Platform.LinuxDBus.Tests) |

The repository contains no samples, no tools and no demo applications; the test project is the only
non-package content, and it is where the worked examples live. Beyond verifying correctness, it is a
complete worked example of every consumer scenario the package supports - method calls and replies,
error replies, method handlers, signal subscription and delivery, variant round-trips and each
transport:

```bash
dotnet test CodeBrix.Platform.LinuxDBus.slnx
```

The files worth opening first are `ConnectionTests.cs` (method calls, replies, errors, method
handlers, signal delivery, `SynchronizationContext` behavior, observer and handler exception
semantics), `SignalOwnerTests.cs` (signal sender verification against a real bus, plus a worked
`RequestName` / `ReleaseName` pair written as extension methods), `ReaderTests.cs` and
`WriterTests.cs` (every wire type in both directions), `VariantValueTests.cs`,
`IntrospectionXmlTests.cs`, `TransportTests.cs` (connecting over unix, unix-abstract and tcp, and
supplying your own already-connected `Stream` through `ClientConnectionOptions`) and
`ExceptionTests.cs`.

Some tests need resources that only exist on a Linux host, and they skip rather than fail elsewhere.
There are no environment variables to set and no opt-in switches - the gates are automatic.
`dbus-daemon` must be installed for the transport tests and for most of the signal-owner tests; on a
Debian-based system:

```bash
apt install dbus
```

`libc` is needed for the Unix file-descriptor tests, and the TCP transport case skips itself at run
time under SELinux enforcement, because the daemon cannot obtain the peer's security context over TCP
and closes the connection.

Three test helpers are the pieces to copy when writing tests of your own. `PlatformGate.cs` is a
single `static bool IsLinux`, the skip target for every Linux-only test. `DBusDaemon.cs` starts a real
`dbus-daemon` subprocess with a temporary session-bus configuration over unix, unix-abstract or tcp,
exposes its `Address` and `IsSELinux`, and shuts it down on `Dispose()`. `PairedConnection.CreatePair()`
returns two `Connection` objects wired to each other through an in-memory message-stream pair, with no
daemon, no sockets and no bus - the two ends are peers rather than bus clients, so bus-only behavior
such as name ownership and `AddMatch` subscription is exercised against `DBusDaemon` instead.

## License

CodeBrix.Platform.LinuxDBus is licensed under the MIT License; the license is also named in the
package ID (`CodeBrix.Platform.LinuxDBus.MitLicenseForever`). For the provenance and licensing of
open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.LinuxDBus/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Platform](CodeBrix.Platform.md) - the framework and its X11, Wayland and frame-buffer
  Linux heads
- [Runs on every laptop](../platform/02-runs-on-every-laptop.md) - the six heads and the three
  operating systems they cover
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.Platform.LinuxDBus on GitHub](https://github.com/ellisnet/CodeBrix.Platform.LinuxDBus) - source and tests
