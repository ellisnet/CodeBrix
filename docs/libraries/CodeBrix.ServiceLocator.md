<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.ServiceLocator</sub>

# CodeBrix.ServiceLocator

**CodeBrix.ServiceLocator is a shared abstraction over IoC containers and service locators: it lets
an application resolve services by type, or by type plus a string key, without taking a hard
reference on any specific container.** The package contains the abstraction only - it has no
container of its own, performs no registration, and creates no objects. You supply a container
adapter, and this package defines the shape that adapter presents to the rest of the application.
Use it from any .NET 10 application, or from a CodeBrix.Platform application.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.ServiceLocator](https://github.com/ellisnet/CodeBrix.ServiceLocator) |
| **Packages** | [`CodeBrix.ServiceLocator.MsplLicenseForever`](https://www.nuget.org/packages/CodeBrix.ServiceLocator.MsplLicenseForever) |
| **License** | Microsoft Public License (MS-PL); see [License](#license) |
| **Requires** | .NET 10 or later; no NuGet dependencies at all - only the base class library |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Any - fully managed, no native libraries, no platform-specific code; works in trimmed and AOT-published applications |

## What it does

- Defines `IServiceLocator`, the abstraction, which extends `System.IServiceProvider` and resolves by
  type or by type plus a string key.
- Supplies `ServiceLocatorImplBase`, an abstract base that implements the entire `IServiceLocator`
  surface in terms of two abstract template methods, so a container adapter implements exactly two
  methods.
- Supplies `ServiceLocator`, a static ambient-container accessor with `Current`,
  `SetLocatorProvider` and `IsLocationProviderSet`.
- Supplies `ServiceLocatorProvider`, the delegate that hands the ambient container to that accessor.
- Supplies `ActivationException`, the standard resolution-failure exception, into which the base
  class wraps whatever the container threw.
- Offers two overridable message formatters, `FormatActivationExceptionMessage` and
  `FormatActivateAllExceptionMessage`, for richer failure text.
- Ships XML documentation (IntelliSense) alongside the assembly, and the assembly is marked
  `[CLSCompliant(true)]`.

The whole public surface is those five types, in one flat namespace.

## When to use it

Reach for this package when a library or an application layer must resolve services but must not
depend on a particular container - a shared assembly consumed by applications that each pick their
own IoC container, or a component that wants a single ambient resolution point. Deriving from
`ServiceLocatorImplBase` for the container you use is the intended work, and it is two method bodies.

It is not a container. There is no registration, binding, factory, decorator or module API - nothing
named `Register`, `Bind` or `Configure` exists in it. It does not manage lifetimes or scopes: no
singleton, transient or scoped concepts, no child containers, no disposal. Whatever the adapter
returns is what callers get, and this package never disposes it. It ships no adapter for any specific
container.

There is no asynchronous resolution, no `GetInstanceAsync`, and nothing returns `Task`. There is no
try-style API - no `TryGetInstance`, no `TryResolve`, no null-on-missing overload; failure is an
exception. There is no constructor injection, no attribute model, no source generator, no assembly
scanning and no reflection-based auto-registration. The static accessor makes no thread-safety or
ambient-scope guarantees: no lock, no `AsyncLocal`, no per-request scope. `ActivationException` has
no `[Serializable]` `SerializationInfo`/`StreamingContext` constructor, and the assembly is not
strong-name signed.

> [!IMPORTANT]
> The package ID and the assembly end in **ServiceLocat*or***, while the namespace you write in a
> `using` directive ends in **ServiceLocat*ion***. `using CodeBrix.ServiceLocator;` does not compile.

## Getting started

```bash
dotnet add package CodeBrix.ServiceLocator.MsplLicenseForever
```

One namespace holds every public type:

```csharp
using CodeBrix.ServiceLocation;
```

An adapter file typically needs these three:

```csharp
using System;
using System.Collections.Generic;
using CodeBrix.ServiceLocation;
```

Derive an adapter for your container, publish it as the ambient container once at startup, and
resolve from anywhere. Create the adapter once and have the provider hand back that same instance -
the delegate is invoked on every read of `ServiceLocator.Current`.

```csharp
using System;
using System.Collections.Generic;
using CodeBrix.ServiceLocation;

// 1. Adapt your IoC container by deriving from ServiceLocatorImplBase.
public sealed class MyContainerAdapter : ServiceLocatorImplBase
{
    private readonly IMyContainer _container;

    public MyContainerAdapter(IMyContainer container)
    {
        _container = container;
    }

    protected override object DoGetInstance(Type serviceType, string key)
    {
        // Resolve a single service from your container.
        return _container.Resolve(serviceType, key);
    }

    protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
    {
        // Resolve all registered services of serviceType.
        return _container.ResolveAll(serviceType);
    }
}

// 2. Register the adapter as the ambient container, once, at startup.
//    Create the adapter ONCE and have the provider hand back that same
//    instance: the delegate is invoked on every read of ServiceLocator.Current,
//    so `() => new MyContainerAdapter(...)` would build a new adapter on every
//    single resolution.
var locator = new MyContainerAdapter(myContainer);
ServiceLocator.SetLocatorProvider(() => locator);

// 3. Resolve services anywhere via the ambient accessor.
var service = ServiceLocator.Current.GetInstance<IMyService>();
var named = ServiceLocator.Current.GetInstance<IMyService>("secondary");
var all = ServiceLocator.Current.GetAllInstances<IMyService>();
```

Those three steps are the whole integration: two overrides, one assignment at startup, and typed
resolution everywhere else.

## Key concepts

### The namespace is CodeBrix.ServiceLocation

The flat, single namespace is deliberate, and so is its spelling. A namespace named
`CodeBrix.ServiceLocator` would be a member of the enclosing `CodeBrix` namespace, so in any consumer
whose own namespace begins with `CodeBrix.` the bare name `ServiceLocator` would bind to the
*namespace* and hide the *class* of the same name. With `CodeBrix.ServiceLocation`,
`ServiceLocator.Current` resolves correctly from any namespace. A using-alias at the top of the file
does not help, because an enclosing-namespace member outranks a compilation-unit using-alias.

Writing it wrongly produces CS0246 (namespace not found) from `using CodeBrix.ServiceLocator;`, or
CS0234 / CS0118 naming a namespace where a type was expected when the calling code sits inside a
`CodeBrix.*` namespace. A compile-time regression guard in the test project keeps the collision from
being reintroduced.

### IServiceLocator

```csharp
public interface IServiceLocator : IServiceProvider
{
    object                GetInstance(Type serviceType);
    object                GetInstance(Type serviceType, string key);
    IEnumerable<object>   GetAllInstances(Type serviceType);
    TService              GetInstance<TService>();
    TService              GetInstance<TService>(string key);
    IEnumerable<TService> GetAllInstances<TService>();
}
```

Resolve by type, or by type plus a string key - the name the object was registered with in the
underlying container. Because the interface extends `System.IServiceProvider`, an `IServiceLocator`
can be handed to any API that accepts an `IServiceProvider`; the inherited member is
`object GetService(Type serviceType)`. Every member is documented as throwing `ActivationException`
when resolution fails.

### ServiceLocatorImplBase and the two template methods

```csharp
public abstract class ServiceLocatorImplBase : IServiceLocator
{
    public virtual object                GetService(Type serviceType);
    public virtual object                GetInstance(Type serviceType);
    public virtual object                GetInstance(Type serviceType, string key);
    public virtual IEnumerable<object>   GetAllInstances(Type serviceType);
    public virtual TService              GetInstance<TService>();
    public virtual TService              GetInstance<TService>(string key);
    public virtual IEnumerable<TService> GetAllInstances<TService>();

    protected abstract object              DoGetInstance(Type serviceType, string key);
    protected abstract IEnumerable<object> DoGetAllInstances(Type serviceType);

    protected virtual string FormatActivationExceptionMessage(
        Exception actualException, Type serviceType, string key);
    protected virtual string FormatActivateAllExceptionMessage(
        Exception actualException, Type serviceType);
}
```

This is the contract the two overrides must satisfy. `GetService(serviceType)` and
`GetInstance(serviceType)` both route to `GetInstance(serviceType, null)`. `GetInstance(serviceType,
key)` calls `DoGetInstance` inside a try/catch where any exception is caught and rethrown as
`ActivationException` whose `InnerException` is the original. `GetAllInstances(serviceType)` does the
same for `DoGetAllInstances`, using the other formatter. The generic single-instance overloads cast
the non-generic result, and `GetAllInstances<TService>()` is a C# iterator that walks
`GetAllInstances(typeof(TService))` and casts each element.

`key` is null whenever the caller did not ask for a named registration - treat null as "the default
registration for this type".

### The static ambient accessor

```csharp
public static class ServiceLocator
{
    public static IServiceLocator Current { get; }
    public static void SetLocatorProvider(ServiceLocatorProvider newProvider);
    public static bool IsLocationProviderSet { get; }
}
```

`Current` invokes the registered `ServiceLocatorProvider` delegate and returns what it returns. It is
a property, and the delegate is invoked on every get. If no provider has been set, `Current` throws
`InvalidOperationException` with the message " ServiceLocationProvider must be set." (leading space
included). It never returns null on its own - but a provider that returns null makes `Current` return
null.

`SetLocatorProvider` stores the delegate; passing null clears it, and there is no separate
`Reset`/`Clear` method. `IsLocationProviderSet` means only "a provider delegate is currently stored";
it does not call the delegate and says nothing about what the delegate will return. The stored
provider is a plain private static field - process-wide, not thread-local, not `AsyncLocal`, and not
synchronized.

The delegate itself is one line:

```csharp
public delegate IServiceLocator ServiceLocatorProvider();
```

### ActivationException and the error model

```csharp
public class ActivationException : Exception
{
    public ActivationException();
    public ActivationException(string message);
    public ActivationException(string message, Exception innerException);
}
```

All failures raised through a `ServiceLocatorImplBase`-derived adapter arrive as
`ActivationException`, so `catch (ActivationException ex)` is the one catch a consumer needs, and
`ex.InnerException` holds the container's own exception. "Not registered" is not distinguished from
"registered but failed to construct" - both are whatever the container throws, wrapped.

Two failures do not arrive that way. An unset ambient provider throws `InvalidOperationException`
from `ServiceLocator.Current`. And the casts performed by the generic overloads happen outside the
wrapping try/catch, so a container that returns an object of the wrong type produces a plain
`InvalidCastException`.

The default messages name only the requested type and key - "Activation error occurred while trying
to get instance of type {serviceType.Name}, key "{key}"" and "Activation error occurred while trying
to get all instances of type {serviceType.Name}". Neither includes the underlying exception's own
message, even though the actual exception is passed in; override the formatters to fold the
container's diagnostics into the message text.

### Performance characteristics

`ServiceLocator.Current` calls the provider delegate on every single access. Return a cached instance
from the delegate, never construct the adapter or the container inside it, and hoist
`IServiceLocator locator = ServiceLocator.Current;` out of a hot loop.

Resolution cost is entirely the container's. This package adds one delegate invocation, one virtual
call, one try/catch (free when nothing throws) and, for the generic overloads, one cast per instance.
`GetAllInstances<TService>()` casts element by element as it enumerates and re-walks the container
each time it is enumerated, so materialize once with `ToList` or `ToArray` when the same set is used
more than once. Exceptions are the expensive path: do not use failed resolution as normal control
flow in hot code.

## Examples

A complete dictionary-backed adapter, showing both overrides. Substitute the two method bodies with
calls into the real container; the shape does not change.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using CodeBrix.ServiceLocation;

namespace MyApp.Composition;

public sealed class MyContainerLocator : ServiceLocatorImplBase
{
    private readonly Dictionary<Type, List<KeyValuePair<string, object>>> _registry
        = new Dictionary<Type, List<KeyValuePair<string, object>>>();

    public MyContainerLocator Register(Type serviceType, string key, object instance)
    {
        if (!_registry.TryGetValue(serviceType, out List<KeyValuePair<string, object>> list))
        {
            list = new List<KeyValuePair<string, object>>();
            _registry[serviceType] = list;
        }
        list.Add(new KeyValuePair<string, object>(key, instance));
        return this;
    }

    //Called for GetInstance/GetService; key is null for unnamed requests.
    protected override object DoGetInstance(Type serviceType, string key)
    {
        if (!_registry.TryGetValue(serviceType, out List<KeyValuePair<string, object>> list))
        {
            throw new InvalidOperationException(
                "No registration for " + serviceType.FullName);
        }

        return list.First(kv => kv.Key == key).Value;
    }

    //Called for GetAllInstances; return an already-materialized sequence so
    //that failures happen inside the base class's try/catch.
    protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
    {
        return _registry.TryGetValue(serviceType, out List<KeyValuePair<string, object>> list)
            ? list.Select(kv => kv.Value).ToList()
            : (IEnumerable<object>)Array.Empty<object>();
    }
}
```

Notice that `DoGetAllInstances` materializes its result: only the production of the sequence is
inside the base class's try/catch.

Publishing the adapter as the ambient container, and clearing it again on shutdown:

```csharp
using CodeBrix.ServiceLocation;

namespace MyApp.Composition;

public static class AppComposition
{
    private static MyContainerLocator _locator;

    public static void Initialize()
    {
        _locator = new MyContainerLocator()
            .Register(typeof(IClock), null, new SystemClock())
            .Register(typeof(IGreeter), "friendly", new FriendlyGreeter())
            .Register(typeof(IGreeter), "terse", new TerseGreeter());

        //The lambda closes over the single instance - do not "new" here.
        ServiceLocator.SetLocatorProvider(() => _locator);
    }

    public static void Shutdown()
    {
        //Clearing the ambient provider: pass null.
        ServiceLocator.SetLocatorProvider(null);
        _locator = null;
    }
}
```

All six resolution shapes from a consumer, including the inherited `IServiceProvider` form:

```csharp
using System;
using System.Collections.Generic;
using CodeBrix.ServiceLocation;

public sealed class ReportService
{
    public void Run()
    {
        //Typed resolution - the common case.
        IClock clock = ServiceLocator.Current.GetInstance<IClock>();

        //Named resolution.
        IGreeter greeter = ServiceLocator.Current.GetInstance<IGreeter>("friendly");

        //All registrations of a type. Materialize before use (see PITFALLS).
        List<IGreeter> all =
            new List<IGreeter>(ServiceLocator.Current.GetAllInstances<IGreeter>());

        //Non-generic form, when the type is only known at run time.
        object byType = ServiceLocator.Current.GetInstance(typeof(IClock));

        //IServiceProvider form - inherited member; throws, does not return null.
        object viaProvider = ((IServiceProvider)ServiceLocator.Current)
            .GetService(typeof(IClock));

        Console.WriteLine(clock.GetType().Name + " / " + greeter.GetType().Name
                          + " / " + all.Count + " / " + byType
                          + " / " + viaProvider);
    }
}
```

Library code that may run before the application's startup path guards on
`IsLocationProviderSet` first, then catches the one exception type:

```csharp
using System;
using CodeBrix.ServiceLocation;

public static class Resolver
{
    public static TService TryResolve<TService>() where TService : class
    {
        //Never let Current throw just because startup has not run yet.
        if (!ServiceLocator.IsLocationProviderSet)
        {
            return null;
        }

        try
        {
            return ServiceLocator.Current.GetInstance<TService>();
        }
        catch (ActivationException ex)
        {
            //The container's own exception is the inner one; the
            //ActivationException message only names the type and key.
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex.InnerException);
            return null;
        }
    }
}
```

Overriding the two formatters folds the container's own diagnostics into the message text instead of
leaving them only in `InnerException`:

```csharp
using System;
using System.Collections.Generic;
using CodeBrix.ServiceLocation;

public sealed class DiagnosticLocator : ServiceLocatorImplBase
{
    protected override object DoGetInstance(Type serviceType, string key) =>
        throw new NotImplementedException("call the real container here");

    protected override IEnumerable<object> DoGetAllInstances(Type serviceType) =>
        throw new NotImplementedException("call the real container here");

    protected override string FormatActivationExceptionMessage(
        Exception actualException, Type serviceType, string key)
    {
        return "Could not resolve " + serviceType.FullName
               + " (key: " + (key ?? "<none>") + "): "
               + actualException.Message;
    }

    protected override string FormatActivateAllExceptionMessage(
        Exception actualException, Type serviceType)
    {
        return "Could not resolve all " + serviceType.FullName + ": "
               + actualException.Message;
    }
}
```

Tests, and applications that tear down and rebuild their container, clear the provider explicitly:

```csharp
using CodeBrix.ServiceLocation;

// The provider is process-wide static state. Tests, and applications that
// tear down and rebuild their container, should clear it explicitly.
ServiceLocator.SetLocatorProvider(null);

if (!ServiceLocator.IsLocationProviderSet)
{
    // ServiceLocator.Current would now throw InvalidOperationException.
}
```

## Using it in a CodeBrix.Platform application

Nothing in this package is UI-aware or head-specific: it has no platform-specific code and no OS
restrictions, so a CodeBrix.Platform application references it from the `.Core` library like any
other package and needs no per-head registration.

The one integration inside the family is on the consuming side.
[CodeBrix.Platform.Extensions](CodeBrix.Platform.Extensions.md) takes this package as a dependency
and uses it in exactly one place: `LogExtensionPoint` resolves an `ILoggerFactory` from
`ServiceLocator.Current` when `ServiceLocator.IsLocationProviderSet` is true. That is the optional
second bootstrap route for logging; assigning `LogExtensionPoint.AmbientLoggerFactory` directly is
the first, and needs no service locator at all.

## Pitfalls

- The namespace is `CodeBrix.ServiceLocation`; the package and the assembly are
  `CodeBrix.ServiceLocator`. This is the single most likely consumption error.
- `ServiceLocator.Current` throws `InvalidOperationException` - not `ActivationException` - when no
  provider is set. Guard with `if (ServiceLocator.IsLocationProviderSet)` in library code that may
  run before an application's startup path.
- `SetLocatorProvider(null)` is how the ambient container is cleared; there is no `Reset()` or
  `ClearLocatorProvider()`. Tests that set the provider must clear it afterwards, or later tests
  inherit it.
- The provider delegate runs on every `Current` access, so `SetLocatorProvider(() => new MyLocator())`
  builds a new adapter - and possibly a new container - per resolution. Capture one instance in a
  field and return it.
- `ActivationException` hides the real cause in `InnerException`, and the default message names only
  the requested type and key. Always log or inspect `ex.InnerException`.
- `GetService(Type)` does not follow the usual `IServiceProvider` convention of returning null for an
  unknown service: `ServiceLocatorImplBase` routes it to `GetInstance(serviceType, null)`, so an
  unresolvable service throws. Do not hand an `IServiceLocator` to code that tests the result of
  `GetService` for null and expects no exception.
- Null-key semantics matter. `GetInstance(Type)`, `GetInstance<TService>()` and `GetService(Type)`
  all reach `DoGetInstance` with `key == null`. An adapter that indexes registrations by a non-null
  name must map null onto its default registration, or every unnamed resolution fails.
- The generic casts are unwrapped: `GetInstance<TService>()` casts after the try/catch, so a
  container returning an object of the wrong type raises `InvalidCastException`, not
  `ActivationException`.
- `GetAllInstances<TService>()` is a C# iterator, so calling it executes nothing. The resolution -
  and therefore any `ActivationException` - happens on the first `MoveNext`. Put the enumeration,
  not the call, inside the try/catch, and materialize before crossing an error boundary.
- Wrapping only covers producing the sequence. If an override of `DoGetAllInstances` returns a lazily
  evaluated query, exceptions thrown while the caller enumerates escape unwrapped. Materialize inside
  `DoGetAllInstances`.
- The ambient provider is process-wide static state - not thread-local, not `AsyncLocal`, not
  synchronized. Set it once during startup, before other threads run. Parallel test classes that each
  set the provider interfere with one another; prefer injecting an `IServiceLocator` directly in
  tests, or serialize those tests.
- Every `ServiceLocatorImplBase` member is virtual. Overriding `GetInstance(Type, string)` or
  `GetAllInstances(Type)` without calling base silently removes the `ActivationException` wrapping
  that consumers rely on. Override the `Do*` methods instead.
- `ServiceLocator.Current` can legitimately return null if the registered provider delegate returns
  null; the accessor does not check. A provider that reads a field cleared at shutdown hands out null
  rather than throwing.
- Do not have two assemblies exposing these five type names in scope at once - identical type names
  in two namespaces produce ambiguous references.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.ServiceLocator/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.ServiceLocator/blob/main/AGENT-README.txt) |
| Samples, tools and other non-package content | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.ServiceLocator/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.ServiceLocator/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.ServiceLocator.Tests](https://github.com/ellisnet/CodeBrix.ServiceLocator/tree/main/tests/CodeBrix.ServiceLocator.Tests) |
| Library source | [src/CodeBrix.ServiceLocator](https://github.com/ellisnet/CodeBrix.ServiceLocator/tree/main/src/CodeBrix.ServiceLocator) |

The repository holds exactly two projects - the library and its test project - and no samples. The
tests are the worked examples:
[`MockServiceLocator.cs`](https://github.com/ellisnet/CodeBrix.ServiceLocator/blob/main/tests/CodeBrix.ServiceLocator.Tests/MockServiceLocator.cs)
is a minimal, complete container adapter with a `FailOnNextResolve()` switch that exercises the
`ActivationException` wrapping;
[`ServiceLocatorTests.cs`](https://github.com/ellisnet/CodeBrix.ServiceLocator/blob/main/tests/CodeBrix.ServiceLocator.Tests/ServiceLocatorTests.cs)
covers the ambient accessor;
[`ServiceLocatorImplBaseTests.cs`](https://github.com/ellisnet/CodeBrix.ServiceLocator/blob/main/tests/CodeBrix.ServiceLocator.Tests/ServiceLocatorImplBaseTests.cs)
covers typed, named and by-`Type` resolution and the exception wrapping;
[`ActivationExceptionTests.cs`](https://github.com/ellisnet/CodeBrix.ServiceLocator/blob/main/tests/CodeBrix.ServiceLocator.Tests/ActivationExceptionTests.cs)
covers the three constructors; and
[`NamespaceCollisionGuardTests.cs`](https://github.com/ellisnet/CodeBrix.ServiceLocator/blob/main/tests/CodeBrix.ServiceLocator.Tests/NamespaceCollisionGuardTests.cs)
is a compile-time guard that lives in an unrelated `CodeBrix.*` namespace and stops compiling if the
namespace collision is ever reintroduced. It needs no preparation, no environment variables and no
external services:

```bash
dotnet test CodeBrix.ServiceLocator.slnx
```

## License

CodeBrix.ServiceLocator is licensed under the Microsoft Public License (MS-PL); the license is also
named in the package ID (`CodeBrix.ServiceLocator.MsplLicenseForever`). For the provenance and
licensing of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.ServiceLocator/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Platform.Extensions](CodeBrix.Platform.Extensions.md) - the family library that resolves
  an `ILoggerFactory` through this abstraction
- [CodeBrix.Platform](CodeBrix.Platform.md) - where the ambient factory is set, in the head's
  `App.InitializeLogging()`
- [Platform services](../platform/07-platform-services.md) - dependency injection and services in a
  CodeBrix.Platform application
- [ellisnet/CodeBrix.ServiceLocator on GitHub](https://github.com/ellisnet/CodeBrix.ServiceLocator) - source and tests
