<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Platform.Extensions</sub>

# CodeBrix.Platform.Extensions

**CodeBrix.Platform.Extensions is a single-assembly bundle of general-purpose .NET helper types:
functional helpers, a disposables toolkit, LINQ-style collection extensions, equality and comparison
comparers, `ILogger` convenience extensions, and lock-free threading primitives.** It has no UI
dependency and no platform dependency - it is a plain class library. Use it from any .NET 10
application, or from a CodeBrix.Platform application, where it also carries the logging extension
point the framework bridges through.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Platform.Extensions](https://github.com/ellisnet/CodeBrix.Platform.Extensions) |
| **Packages** | [`CodeBrix.Platform.Extensions.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Extensions.ApacheLicenseForever) |
| **License** | Apache 2.0; see [License](#license) |
| **Requires** | .NET 10 or later; depends on [`CodeBrix.ServiceLocator.MsplLicenseForever`](https://www.nuget.org/packages/CodeBrix.ServiceLocator.MsplLicenseForever) and [`Microsoft.Extensions.Logging`](https://www.nuget.org/packages/Microsoft.Extensions.Logging) |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Any - no native libraries, no OS restrictions, no MSBuild targets or props |

## What it does

- Supplies threading primitives: `FastAsyncLock`, an async-aware mutex that is re-entrant within one
  `ExecutionContext`; `AsyncEvent`, a counting async gate; and `FastTaskCompletionSource<T>`, an
  allocation-light `TaskCompletionSource` replacement that is its own awaiter.
- Supplies `Transactional`, a set of lock-free compare-and-swap update helpers built on
  `Interlocked`, with overloads for `IImmutableDictionary<TKey,TValue>`, `IImmutableQueue<T>` and
  `IImmutableList<T>`.
- Supplies a disposables toolkit: `Disposable`, `AnonymousDisposable`, `CompositeDisposable`,
  `SerialDisposable`, `RefCountDisposable`, `CancellationDisposable`, `ConditionalDisposable`,
  `DefaultDisposable`, `ICancelable`, `IExtensibleDisposable`, `DisposableExtensions`,
  `CompositeDisposableExtensions`, plus `NullDisposable` and `DisposableAction`.
- Supplies logging helpers: the `LogExtensionPoint` ambient-factory singleton with `Log()` extension
  methods on `Type` and on any instance, and `LogExtensions`, five families of `ILogger` extension
  methods including conditional variants that build the message only when the level is enabled.
- Supplies functional helpers: thread-safe and non-thread-safe memoizers, `Retry`, weak per-instance
  memoization, currying, delegate factories, the `ActionAsync` and `FuncAsync` delegate families, and
  the `Null` placeholder type.
- Extends collections over `IEnumerable<T>`, `ICollection<T>`, `IList<T>`,
  `IDictionary<TKey,TValue>`, `Queue<T>`, `Stack<T>`, the non-generic `IEnumerable`, and
  `Span<T>`/`Memory<T>`.
- Patches a bound collection in place - `Update`, `UpdateWithResults` and `UpdateAsync` diff an
  `IList<T>` against fresh data instead of `Clear()` plus `AddRange()`, so bindings and selection
  survive.
- Produces bindable groups: fixed-size chunking, alphabetic bucketing, and descriptor-driven
  grouping that keeps the declared order.
- Supplies equality and comparison types: `FuncEqualityComparer`, `KeyEqualityComparer`,
  `CollectionEqualityComparer<T>`, `WeakReferenceEqualityComparer<T>`, `IKeyEquatable`,
  `FuncComparer<T>`, `FastTypeComparer` and `EqualityComparerExtensions`.
- Supplies string, double, stream, `TextWriter`, `Uri`, `Match`, weak-reference and enum helpers, the
  `DateTimeUnit` vocabulary enum, `CachedTuple` and `LegacyAttribute`.
- Supplies weak attached side tables - `WeakAttachedDictionary<TOwner,TKey>` (thread-safe) and
  `UnsafeWeakAttachedDictionary<TOwner,TKey>` (single-thread) - for attaching values to instances you
  do not own without keeping them alive.

## When to use it

Reach for this library when application code needs the small, sharp helpers that a codebase
otherwise re-implements: an async lock, a composite disposable, a memoized lookup, a retry, a
comparer built from a lambda, an in-place refresh of a bound collection. Every helper is a plain
managed type; referencing the package is the whole installation.

It is not a logging framework: there are no `ILogger` providers, sinks or configuration, and the
application brings its own logging providers. It is not a dependency-injection container - the
service-location dependency is used only as an optional lookup for `ILoggerFactory`, and the
abstraction behind that lookup is [CodeBrix.ServiceLocator](CodeBrix.ServiceLocator.md). It ships no
UI types, no XAML, no controls and no platform abstractions. It has no `DateTime` extension methods;
`DateTimeUnit` is a vocabulary enum other APIs can accept.

The packaged assembly ships no XML documentation file, so IntelliSense shows signatures without
summaries. The
[AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.Extensions/blob/main/AGENT-README.txt)
is the reference, and it ships inside the package.

## Getting started

```bash
dotnet add package CodeBrix.Platform.Extensions.ApacheLicenseForever
```

Every public type lives under `CodeBrix.Platform.Extensions.*`. The namespaces you import most are
the root one and a small set of area namespaces:

```csharp
using CodeBrix.Platform.Extensions;          // LogExtensionPoint
using CodeBrix.Platform.Extensions.Logging;  // LogExtensions
```

```csharp
using CodeBrix.Platform.Extensions;             // DisposableAdd
using CodeBrix.Platform.Extensions.Disposables; // the rest
```

Logging is the only bootstrap the library asks for. Set the ambient factory once at startup, before
anything calls `.Log()`, and every type in the process can log from `this`:

```csharp
using CodeBrix.Platform.Extensions;   //LogExtensionPoint and the .Log() extensions
using Microsoft.Extensions.Logging;   //ILoggerFactory, ILogger

LogExtensionPoint.AmbientLoggerFactory = myLoggerFactory;   //any ILoggerFactory

//...afterwards, from any instance - or from a Type
this.Log().LogInformation("Ready.");
```

If that assignment is skipped, nothing throws: `.Log()` falls back to a factory with no providers,
which drops every message. No output is the failure mode, not an exception.

## Key concepts

### Namespaces do not follow the folder layout

The folder layout inside the assembly does not match the namespaces, and three cases surprise
people. `NullDisposable` is in the root namespace, not `.Disposables`. The collection extension
classes - `EnumerableExtensions`, `CollectionExtensions`, `ListExtensions`, `DictionaryExtensions`,
`QueueExtensions`, `StackExtensions`, `ObservableCollectionExtensions` - are in the root namespace,
not `.Collections`. `Transactional` and `LogExtensionPoint` are in the root namespace too. Only
`MemoryExtensions` and the two `WeakAttachedDictionary` types live in
`CodeBrix.Platform.Extensions.Collections`.

### FastAsyncLock, AsyncEvent and FastTaskCompletionSource

`FastAsyncLock` is an async-aware mutex with a single member,
`Task<IDisposable> LockAsync(CancellationToken ct)`. Await it and dispose the returned handle to
release. It is re-entrant within one `ExecutionContext` - it tracks the holder in an `AsyncLocal`, so
a nested `LockAsync` on the same async flow completes synchronously instead of deadlocking. There is
no constructor argument, no timeout overload and no synchronous `Lock()`.

`AsyncEvent(int initialCount)` is a counting async gate with `CurrentCount`, `Wait`, `Release()` and
`Release(int)`. `Wait` returns `true` when a count was consumed and `false` - rather than throwing -
when the token is already canceled.

`FastTaskCompletionSource<T>` implements `INotifyCompletion` and is its own awaiter: `await source;`
works directly, without touching `.Task`. Its nested `TerminationType` enum reports `Running`,
`Result`, `Exception` or `Canceled`.

### Transactional: lock-free compare-and-swap updates

`Transactional` lives in the root namespace and updates a field you own without a lock. The `ref`
argument must be a field you own, and the selector may run more than once when another thread wins
the race, so it must be pure - never put I/O, a counter or an event raise inside it.

Beyond the scalar `Update<T>(ref T original, Func<T,T> selector)` shapes there are immutable
dictionary helpers (`GetOrAdd`, `TryAdd`, `TryRemove`, `Remove`, `SetItem`, `TryUpdateItem`),
immutable queue helpers (`Enqueue`, `TryDequeue`, `Dequeue`) and immutable list helpers (`Add`,
`AddDistinct`, `TryAddDistinct`, `Remove`, `RemoveRange`). These require the
`System.Collections.Immutable` interfaces; they do not work on `List<T>` or
`Dictionary<TKey,TValue>`.

### The disposables toolkit

`Disposable.Create(Action dispose)` wraps an action that runs at most once, however often `Dispose()`
is called, and throws `ArgumentNullException` on a null action. `Disposable.Empty` returns the shared
no-op instance.

`CompositeDisposable` implements `ICollection<IDisposable>` and `ICancelable`. `Add` disposes the
item immediately if the composite is already disposed, `Remove` also disposes the removed item, and
`Clear` disposes the children while keeping the composite usable. `SerialDisposable.Disposable` is a
slot: assigning disposes the previous value. `RefCountDisposable` disposes the underlying only when
`Dispose()` has been called and every handed-out reference is disposed. `ConditionalDisposable` keeps
itself alive only as long as its target is alive, so the action runs when the target is collected or
when `Dispose()` is called.

`DisposableExtensions` adds `DisposeWith` (into an `ICollection<IDisposable>` or a
`SerialDisposable`), `SafeDispose`, `TryDispose` and `DisposeAll`.

### LogExtensionPoint and the ILogger extension families

`LogExtensionPoint` exposes `AmbientLoggerFactory`, `Log(this Type forType)` and
`Log<T>(this T instance)`. The factory resolves lazily on first use: an instance you assigned wins;
otherwise, when a `CodeBrix.ServiceLocation` provider is set, the result of resolving `ILoggerFactory`
through it - a resolved object of the wrong type raises `InvalidOperationException`; otherwise an
empty `LoggerFactory`.

`LogExtensions`, in `CodeBrix.Platform.Extensions.Logging`, adds five families of extension methods
on `ILogger`: level-generic entry points (`LogFormat`, `Log`), per-level trios for Trace, Debug,
Info, Warn, Error and Critical, and the conditional `DebugIfEnabled`, `InfoIfEnabled`,
`WarnIfEnabled`, `ErrorIfEnabled` and `CriticalIfEnabled` variants that check `IsEnabled` before
building the message. Note the names `Info` and `Warn`, not `Information` and `Warning`.

### Memoization and retry

`AsMemoized` (over `Func<...>` arities up to five parameters, over `Func<CancellationToken,Task<T>>`
and over the `FuncAsync` delegates) is backed by a plain `Dictionary` with no lock: fastest, and only
safe when calls are serialized or externally synchronized. `AsLockedMemoized` adds a double-checked
lock for the parameterless form and a concurrent dictionary for the keyed forms. `ApplyMemoized` and
`AsWeakMemoized` memoize per instance without keeping the instance alive. Memoizers never evict, so
use them for bounded key spaces - types, enum values, configuration keys - never for user input or
request ids.

`FuncExtensions.Retry` has four overloads over `Func<Task<T>>`, `Func<CancellationToken,Task<T>>`,
`Func<Task>` and `Func<CancellationToken,Task>`, each taking `int tries = 3` and
`TimeSpan? retryDelay = null`. `tries` is the total number of attempts, not the number of retries;
the default delay between attempts is 100 ms; the returned Task fails with the last exception once
the budget is exhausted.

### Differential update of a bound collection

`ObservableCollectionExtensions` patches a bound collection in place, so bindings and selection
survive: `Update`, `UpdateWithResults` and `UpdateAsync`, each
`(this IList<T> collection, IEnumerable<T> updated, bool tryDispose = false, IEqualityComparer<T> comparer = null)`.
`ObservableCollectionUpdateResults<T>` reports `Added`, `Moved` and `Removed`. `UpdateAsync`
additionally calls `IUpdatable<T>.UpdateAsync` on every kept item, letting an existing item absorb
the newer instance's values.

Pass `tryDispose: true` only when the incoming items are different instances matched by `Equals` -
matching items are not compared by reference, so with shared instances you would dispose live
objects.

### Equality that models identity

`IKeyEquatable` and `IKeyEquatable<T>` express "same identity, possibly different version": two
instances of the same logical entity compare `KeyEquals`-true even when `Equals` is false.
`KeyEqualityComparer` uses `IKeyEquatable` when the items implement it and a fallback comparer
otherwise, which makes it the comparer to pass to the collection `Update` methods when items are
re-fetched versions of the same entities. `FastTypeComparer`, in `.Core.Comparison`, compares `Type`
by reference and `RuntimeHelpers.GetHashCode`, skipping `Type.Equals` entirely - use it as the
comparer of a `Dictionary<Type, ...>` on a hot path.

## Examples

An async lock around a shared resource, re-entrant on the same async flow:

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.Platform.Extensions.Threading;

public sealed class TokenCache
{
    private readonly FastAsyncLock _gate = new FastAsyncLock();
    private string _token;

    public async Task<string> GetTokenAsync(CancellationToken ct)
    {
        using (await _gate.LockAsync(ct))
        {
            if (_token == null)
            {
                _token = await FetchAsync(ct);
                // Re-entering the same lock on this async flow is safe:
                await TouchAsync(ct);
            }

            return _token;
        }
    }

    private async Task TouchAsync(CancellationToken ct)
    {
        using (await _gate.LockAsync(ct))   // re-entrant, no deadlock
        {
            await Task.Yield();
        }
    }

    private Task<string> FetchAsync(CancellationToken ct)
        => Task.FromResult(Guid.NewGuid().ToString("N"));
}
```

Composing disposables for a subscription lifetime - an arbitrary cleanup action, a fluent
registration, a one-live-value slot and a temporary membership handle:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using CodeBrix.Platform.Extensions;             // DisposableAdd
using CodeBrix.Platform.Extensions.Disposables; // the rest

public sealed class Watcher : IDisposable
{
    private readonly CompositeDisposable _subscriptions
        = new CompositeDisposable();
    private readonly SerialDisposable _current = new SerialDisposable();

    public Watcher(FileSystemWatcher watcher)
    {
        // 1. an arbitrary cleanup action
        _subscriptions.Add(() => Console.WriteLine("watcher torn down"));

        // 2. an explicit disposable, registered fluently
        watcher.DisposeWith(_subscriptions);

        // 3. a slot that always holds at most one live subscription:
        //    assigning a new value disposes the previous one
        _current.DisposeWith(_subscriptions);
        _current.Disposable = Disposable.Create(
            () => Console.WriteLine("first pass done"));
        _current.Disposable = Disposable.Create(
            () => Console.WriteLine("second pass done")); // 1st disposed

        // 4. temporary membership: dispose the handle to remove the item
        var registry = new List<string>();
        IDisposable membership = registry.DisposableAdd("watcher");
        _subscriptions.Add(membership);
    }

    public void Dispose() => _subscriptions.Dispose();
}

// After Dispose(), the composite is inert but still safe to use:
// anything you Add() is disposed immediately instead of being kept.
```

Lock-free state, with two fields updated by compare-and-swap and no lock anywhere:

```csharp
using System;
using System.Collections.Immutable;
using CodeBrix.Platform.Extensions;

public sealed class Registry
{
    // Both fields are updated without any lock.
    private ImmutableDictionary<string, int> _counts
        = ImmutableDictionary<string, int>.Empty;
    private State _state = new State(0, "idle");

    public int Touch(string key)
        // UpdateItem's factory receives (key, currentValue); a missing
        // key yields default(TValue), i.e. 0 here.
        => Transactional.UpdateItem(ref _counts, key,
               (k, current) => current + 1);

    public int GetOrAdd(string key)
        => Transactional.GetOrAdd(ref _counts, key, k => k.Length);

    public void Advance(string name)
        // The selector MUST be pure: on a lost race it runs again.
        => Transactional.Update(ref _state,
               s => new State(s.Version + 1, name));

    public sealed class State
    {
        public State(int version, string name)
        { Version = version; Name = name; }
        public int Version { get; }
        public string Name { get; }
    }
}
```

Refreshing a bound collection without losing selection, then grouping it into declared buckets:

```csharp
using System;
using System.Collections.ObjectModel;
using System.Linq;
using CodeBrix.Platform.Extensions;
using CodeBrix.Platform.Extensions.Equality;

public sealed class Person : IKeyEquatable<Person>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int GetKeyHashCode() => Id;
    public bool KeyEquals(Person other) => other?.Id == Id;
}

var bound = new ObservableCollection<Person>(existingPeople);

// Patch in place: existing instances that match are KEPT, so selection
// and bindings survive. KeyEqualityComparer matches by entity key.
var results = bound.UpdateWithResults(
    freshlyFetchedPeople,
    tryDispose: false,
    comparer: KeyEqualityComparer<Person>.Default);

Console.WriteLine($"+{results.Added.Count()} " +
                  $"~{results.Moved.Count()} " +
                  $"-{results.Removed.Count()}");

// Group into declared buckets, keeping the declared order.
var groups = bound.GroupBy(
    new GroupDescriptor<string, Person>("A-M",
        p => p.Name[0] <= 'M', required: true),
    new GroupDescriptor<string, Person>("N-Z",
        p => p.Name[0] > 'M', required: true));

foreach (IBindableGrouping<string, Person> g in groups)
{
    Console.WriteLine($"{g.Key}: {g.Count()}");
}

// Or bucket alphabetically by first letter:
var alpha = bound.GroupAlphabetically(p => p.Name,
                                      includeEmptyGroups: false);
```

`BindableGroup.Items` is a real `IList`, so an items control can bind to it and it can grow.

A complete minimum project - a reference and a `Program.cs` that uses the string, collection and
threading helpers together:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>disable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference
      Include="CodeBrix.Platform.Extensions.ApacheLicenseForever" />
  </ItemGroup>
</Project>
```

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.Platform.Extensions;
using CodeBrix.Platform.Extensions.Disposables;
using CodeBrix.Platform.Extensions.Threading;

internal static class Program
{
    private static readonly FastAsyncLock Gate = new FastAsyncLock();

    private static async Task Main()
    {
        using var cleanup = new CompositeDisposable();
        cleanup.Add(() => Console.WriteLine("done"));

        var names = new List<string> { "  ", "ada", null, "grace" };

        names.Safe()
             .Trim()                        // drop the nulls
             .Where(n => n.HasValueTrimmed())
             .ForEach(n => Console.WriteLine(n.UppercaseFirst()));

        using (await Gate.LockAsync(CancellationToken.None))
        {
            Console.WriteLine("inside the async lock");
        }
    }
}
```

## Using it in a CodeBrix.Platform application

The library is not an add-in and has no head-specific behavior; it is a plain class library that any
project in the solution can reference. Put the reference in the `.Core` library along with the rest
of the application's package references.

Two areas earn their place in a UI application. The bound-collection helpers (`Update`,
`UpdateWithResults`, `UpdateAsync`) and the bindable grouping types (`IBindableGrouping`,
`BindableGroup`) exist so that a refresh keeps bindings and selection intact - see
[MVVM the right way](../platform/05-mvvm-the-right-way.md). And
`UnsafeWeakAttachedDictionary<TOwner,TKey>` is the one to use when the owner is a UI object confined
to the UI thread and the concurrent-dictionary overhead is not wanted.

The framework's own logging bridge is built on `LogExtensionPoint`: a CodeBrix.Platform head sets
`LogExtensionPoint.AmbientLoggerFactory` in `App.InitializeLogging()` before building the host, which
is the same bootstrap this library documents.

## Pitfalls

- Namespaces do not follow folders. `NullDisposable`, the collection extension classes,
  `Transactional` and `LogExtensionPoint` are all in the root namespace.
- `ForEach` is eager and returns the source; `Do` is lazy and does nothing until enumerated.
  Reaching for `Do` and forgetting to enumerate is a silent no-op.
- `MinBy` and `MaxBy` here return a tuple `(TSource Item, TComparable Value)`, not the item, and they
  throw `InvalidOperationException` on an empty sequence. `System.Linq` also has `MinBy`/`MaxBy` with
  the same parameter shape, so with both namespaces imported the call can be ambiguous - write
  `EnumerableExtensions.MinBy(source, selector)` explicitly, and remember to read `.Item`.
- `MaxOrDefault<TSource,TResult>(source, selector, defaultValue)` is not an extension method, and
  neither is `UriExtensions.EscapeDataString`. Call them as plain statics.
- Adding to a `CompositeDisposable` that is already disposed disposes the new item immediately and
  silently - no exception, no membership. Check `IsDisposed` if that matters. `Remove(item)` also
  disposes the item.
- Assigning `SerialDisposable.Disposable` disposes whatever was there before, and assigning after the
  `SerialDisposable` itself is disposed disposes the incoming value at once.
- Logging goes nowhere until `AmbientLoggerFactory` is set, or a `CodeBrix.ServiceLocation` provider
  can resolve `ILoggerFactory`. No exception is thrown; no output means the bootstrap was skipped.
- The logger returned by `instance.Log()` is cached per static type `T` on first use, so set the
  factory before the first `.Log()` call, not after. `Log<T>` also keys off the static type of the
  expression: calling it through an `object`-typed variable yields a `System.Object` logger. Use
  `this.Log()` inside the class, or `typeof(X).Log()`.
- The ambient service-location provider is a private static field per assembly. A provider set
  through a different service-location library is not observed, and the result is an empty
  `LoggerFactory` - no exception, no output.
- `Transactional`'s selector can run several times when threads race. Never put a side effect - I/O,
  counters, event raising - inside it.
- `ObservableCollectionExtensions.Update` with `tryDispose: true` disposes removed items and incoming
  items that were not added. Pass `true` only when the incoming items are different instances matched
  by `Equals`.
- `UnsafeWeakAttachedDictionary` is not thread-safe by design. Use `WeakAttachedDictionary` unless
  single-thread access can be guaranteed.
- `AsyncEvent` has no parameterless constructor - write `new AsyncEvent(0)` - and its `Wait` returns
  `false` on an already-canceled token instead of throwing `OperationCanceledException`.
- `FastAsyncLock` re-entrancy is per `ExecutionContext`. Work pushed onto another thread with
  `Task.Run` does not inherit the lock and will block.
- `Retry`'s `tries` is the total attempt count: `tries: 1` means a single attempt with no retry, and
  `tries: 0` loops forever because the counter never reaches zero.
- The keyed memoizers cache by key forever, treat a null key as a separate single-value cache, and
  cache successful results only - an exception is not cached and the call is retried.
- Never share an `AsMemoized` delegate across threads; a concurrent write can corrupt the underlying
  `Dictionary`. Use `AsLockedMemoized` instead.
- `HasValue` and `HasValueTrimmed` are implemented identically - both are
  `!string.IsNullOrWhiteSpace(instance)` - so `"   ".HasValue()` is false.
- A `BindableGroup`'s `GetHashCode` mixes the key and the item hashes, so it changes as `Items`
  changes. Do not use one as a dictionary key while it is still being filled.
- The `UpdateItem` and `TryUpdateItem` factory overloads are not generic over the dictionary type, so
  the field must be declared as exactly `IImmutableDictionary<TKey,TValue>`,
  `ImmutableDictionary<TKey,TValue>` or `ImmutableSortedDictionary<TKey,TValue>`.
- `Null` has a private constructor. You never construct one; pass `null` wherever a `Null` is
  expected.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Platform.Extensions/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.Extensions/blob/main/AGENT-README.txt) |
| Samples, tools and other non-package content | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Platform.Extensions/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform.Extensions/blob/main/README-INDEX.txt) |
| Tests (worked examples of every area) | [tests/CodeBrix.Platform.Extensions.Tests](https://github.com/ellisnet/CodeBrix.Platform.Extensions/tree/main/tests/CodeBrix.Platform.Extensions.Tests) |
| Library source | [src/CodeBrix.Platform.Extensions](https://github.com/ellisnet/CodeBrix.Platform.Extensions/tree/main/src/CodeBrix.Platform.Extensions) |

The repository has no samples and no tools; the test project is the only non-package content, and it
doubles as the behavior specification. Run it with:

```bash
dotnet test CodeBrix.Platform.Extensions.slnx
```

## License

CodeBrix.Platform.Extensions is licensed under the Apache License 2.0; the license is also named in
the package ID (`CodeBrix.Platform.Extensions.ApacheLicenseForever`). For the provenance and
licensing of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.Extensions/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.ServiceLocator](CodeBrix.ServiceLocator.md) - the service-location abstraction behind the
  optional logging bootstrap route
- [CodeBrix.Platform](CodeBrix.Platform.md) - the framework these helpers were bundled for
- [MVVM the right way](../platform/05-mvvm-the-right-way.md) - where the bound-collection and
  grouping helpers belong in an application
- [ellisnet/CodeBrix.Platform.Extensions on GitHub](https://github.com/ellisnet/CodeBrix.Platform.Extensions) - source and tests
