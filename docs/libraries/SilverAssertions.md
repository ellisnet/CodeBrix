<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › SilverAssertions</sub>

# SilverAssertions

**SilverAssertions states the expected outcome of a unit test as one readable, chainable expression:
`actual.Should().Be(expected);`.** It brings no test runner of its own - it detects the test framework
you already use at the first failure and throws that framework's own assertion exception, so a failing
assertion is reported as an ordinary test failure. Add it to the test project of any .NET 10
application, including a CodeBrix.Platform application.

| | |
| --- | --- |
| **Repository** | [ellisnet/SilverAssertions](https://github.com/ellisnet/SilverAssertions) |
| **Packages** | [`SilverAssertions.ApacheLicenseForever`](https://www.nuget.org/packages/SilverAssertions.ApacheLicenseForever) |
| **License** | Apache License 2.0; see [License](#license) |
| **Requires** | .NET 10 or later; a test framework and its runner, which you choose |
| **Use it from** | Any .NET 10 test project, whatever the application under test is |
| **Platforms** | No native libraries, no OS restrictions - it is a pure managed library |

## What it does

- Wraps a subject in an assertion class chosen by the subject's compile-time type and exposes a
  chainable vocabulary on it: `actual.Should().StartWith("Silver").And.HaveLength(16);`
- Throws through an adapter for the detected framework, so failures read as ordinary test failures -
  xUnit, NUnit, MSTest and MSpec are all recognized
- Compares whole object graphs with `BeEquivalentTo`: a recursive, member-by-member comparison driven
  by the expectation, with an options object for every part of the walk
- Asserts on exceptions and delegates, synchronous and async, and unwraps `AggregateException`
- Covers collections, dictionaries and string collections: count, membership, order, content rules and
  equivalency
- Asserts on types, members and assemblies, which turns architecture rules into tests
  (`typeof(Widget).Assembly.Should().NotReference(...)`)
- Monitors events, with sender and argument refinement
- Batches failures in an `AssertionScope`, so every assertion inside the scope runs and all failures
  are reported together
- Covers streams, XML (LINQ-to-XML and `System.Xml`), `HttpResponseMessage`, `System.Data` and
  execution time
- Formats values in failure messages through an extensible `Formatter`, and lets you add your own
  assertions, formatters and equivalency steps
- Ends almost every assertion with an optional "because" phrase that is appended to the failure message

## When to use it

Reach for SilverAssertions when a test's intent is easier to read as a sentence than as a pile of
framework asserts, and when one failure message should say what was expected, what was found and why it
mattered. It is a leaf library: bring your own test framework and runner, and it throws that framework's
exception.

It does not mock or stub anything. When a test needs a fake dependency, generated test data or
data-driven theories, pair it with [CodeBrix.TestMocks](CodeBrix.TestMocks.md), which deliberately
ships no assertions of its own.

What it does not do, in the source's own terms:

- It does not discover, run or parallelize tests. That is the test framework's job; SilverAssertions
  only throws the failure exception.
- It is not a benchmarking tool. `ExecutionTime` measures one run with a stopwatch.
- It does not do snapshot/approval testing, property-based testing, code-coverage measurement, UI or
  browser automation, or HTTP/service hosting for integration tests.
- It does not assert on `HttpResponseMessage` content - only on status classes and codes. Read the
  content yourself and assert on the string or object.
- It does not do binary serialization round-trips: `BeBinarySerializable` is present for source
  compatibility but always fails, because `BinaryFormatter` is not available on modern .NET.
- It targets .NET 10 and later only; there is no asset for .NET Framework, .NET Standard or earlier
  .NET versions.

## Getting started

```bash
dotnet add package SilverAssertions.ApacheLicenseForever
```

```csharp
using SilverAssertions;
```

That one using covers the overwhelming majority of assertions. The package id is
`SilverAssertions.ApacheLicenseForever`; the namespace is `SilverAssertions`.

This is the whole of the API for the common case - a subject, `.Should()`, and a chain of expectations:

```csharp
using SilverAssertions;

string name = "SilverAssertions";
name.Should().StartWith("Silver").And.EndWith("Assertions").And.HaveLength(16);

int value = 42;
value.Should().BeGreaterThan(0).And.BeLessThan(100);

bool isActive = true;
isActive.Should().BeTrue();
```

No registration call is required. `.And` continues the chain on the same subject, which is what keeps a
multi-part expectation on one thought.

A test project that uses SilverAssertions needs the test framework, its runner and this package:

```bash
dotnet new xunit3 -n MyProject.Tests
cd MyProject.Tests
dotnet add package SilverAssertions.ApacheLicenseForever
```

> [!IMPORTANT]
> On the .NET 10 SDK a test project that runs on Microsoft Testing Platform also needs a `global.json`
> beside the solution, because the SDK will not run those tests through VSTest. Without it `dotnet test`
> fails immediately with "Testing with VSTest target is no longer supported by
> Microsoft.Testing.Platform on .NET 10 SDK and later."

```json
{
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

## Key concepts

### The `.Should()` pattern and the constraint types

`Should()` is an extension method that wraps the subject in an assertion class chosen by the subject's
compile-time type: `string` becomes `StringAssertions`, `int`/`long` and friends become
`NumericAssertions<T>`, `IEnumerable<T>` becomes `GenericCollectionAssertions<T>`,
`IDictionary<TKey,TValue>` becomes `GenericDictionaryAssertions<...>`, `Type` becomes `TypeAssertions`,
`Action` becomes `ActionAssertions`, `Func<Task>` becomes `NonGenericAsyncFunctionAssertions`, and so on
across the subject shapes. What comes back decides how you keep chaining:

```csharp
numbers.Should().HaveCount(3).And.OnlyHaveUniqueItems();

// ContainSingle returns AndWhichConstraint<..., T>
orders.Should().ContainSingle(o => o.Id == 42)
    .Which.Total.Should().Be(19.95m);

// ContainKey returns WhoseValueConstraint<...>
ages.Should().ContainKey("Alice").WhoseValue.Should().Be(30);

// Throw<T> returns ExceptionAssertions<T>; .And is the exception
act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("id");
```

`AndConstraint<T>` exposes `.And`; `AndWhichConstraint<TParent,TMatched>` adds `.Which` (the same value
as `.Subject`); `WhoseValueConstraint<...>` adds `.WhoseValue`.

### Namespaces beyond the root

The root namespace holds `AssertionExtensions` (all the `.Should()` overloads), `FluentActions`,
`EventRaisingExtensions`, the occurrence-constraint factories, `AndConstraint<T>`,
`CustomAssertionAttribute` and `AssertionOptions`. Everything else lives one level down:

```csharp
using SilverAssertions.Execution;    // AssertionScope, Execute,
                                     // IAssertionScope, FailReason,
                                     // AssertionFailedException
using SilverAssertions.Common;       // CSharpAccessModifier, Configuration,
                                     // Services, ValueFormatterDetectionMode
using SilverAssertions.Types;        // AllTypes, TypeSelector,
                                     // MethodInfoSelector,
                                     // PropertyInfoSelector
using SilverAssertions.Events;       // IMonitor<T>, IEventRecording,
                                     // OccurredEvent, EventMetadata
using SilverAssertions.Formatting;   // Formatter, IValueFormatter,
                                     // ValueFormatterAttribute,
                                     // FormattedObjectGraph,
                                     // FormattingOptions, FormattingContext
using SilverAssertions.Equivalency;  // EquivalencyAssertionOptions<T>,
                                     // IMemberInfo, IObjectInfo, INode,
                                     // IEquivalencyStep, MemberVisibility
using SilverAssertions.Extensions;   // 5.Seconds(), 1.January(2026), ...
using SilverAssertions.Specialized;  // ExecutionTime, ExceptionAssertions<T>
using SilverAssertions.Primitives;   // StringAssertions, ObjectAssertions...
using SilverAssertions.Collections;  // GenericCollectionAssertions<T>...
using SilverAssertions.Numeric;      // NumericAssertions<T>...
using SilverAssertions.Streams;      // StreamAssertions
using SilverAssertions.Xml;          // XDocumentAssertions...
using SilverAssertions.Data;         // DataSetAssertions<T>, RowMatchMode...
using SilverAssertions.Reflection;   // AssemblyAssertions
```

### Test framework detection

The failure exception is thrown through an adapter detected at the first failure: an explicit app
setting first, then dynamic scanning of the loaded assemblies, then a fallback.

| Adapter key | Assembly it looks for | Exception it throws |
| --- | --- | --- |
| `mspec` | `Machine.Specifications` | `SpecificationException` |
| `nunit` | `nunit.framework` | `AssertionException` |
| `mstestv2` | `Microsoft.VisualStudio.TestPlatform.TestFramework` | `AssertFailedException` |
| `mstestv3`, `mstestv4` | `MSTest.TestFramework` | `AssertFailedException` |
| `xunit2` | `xunit.assert` | `XunitException` |
| `xunit3` | `xunit.v3.assert` | `XunitException` |

When none is found, `SilverAssertions.Execution.AssertionFailedException` is thrown, which most runners
still report as a failed test. To force an adapter, set the app setting `SilverAssertions.TestFramework`
or assign it in code before the first assertion; naming an unsupported framework throws
`InvalidOperationException` listing the valid keys.

```csharp
using SilverAssertions.Common;

Configuration.Current.TestFrameworkName = "xunit3";
```

### The "because" phrase

Almost every assertion ends with `(..., string because = "", params object[] becauseArgs)`. The phrase is
appended to the failure message, and the word "because" is prepended if it is not there already;
`becauseArgs` are `string.Format` placeholders.

```csharp
value.Should().BeTrue("the user {0} is authenticated", userName);
count.Should().BeGreaterThan(0, "because counts must be positive");
```

### Object-graph equivalency

`BeEquivalentTo` is driven by the expectation: every member of the expectation must have a matching
member on the subject with an equivalent value, and members that exist only on the subject are ignored.
That is why an anonymous type makes a good partial expectation. Collections are compared without regard
to order by default, nested objects recursively, records and tuples natively, and cyclic references are
detected.

The options lambda receives an `EquivalencyAssertionOptions<TExpectation>` and must return it. Its
members cover member selection (`Excluding`, `Including`, `For<TNext>(...)`), typing
(`ComparingByMembers<T>()`, `ComparingByValue<T>()`, `RespectingRuntimeTypes()`,
`ComparingEnumsByName()`), ordering (`WithStrictOrdering()`, `WithStrictOrderingFor`, `AsCollection()`),
recursion (`IgnoringCyclicReferences()`, `AllowingInfiniteRecursion()`), custom comparison
(`Using<TProperty>(...)` with `.WhenTypeIs<TMemberType>()`, `Using<T>(IEqualityComparer<T>)`,
`Using(IEquivalencyStep)`), member mapping (`WithMapping`) and diagnostics (`WithTracing`).

Set the defaults for the whole test run once instead of repeating the same lambda:

```csharp
AssertionOptions.AssertEquivalencyUsing(options => options
    .ComparingByValue<Money>()
    .ExcludingMissingMembers());
```

### Exceptions, delegates and async

`Action` and `Func<T>` subjects get `Throw<TException>`, `ThrowExactly<TException>`, `NotThrow`,
`NotThrow<TException>` and `NotThrowAfter(...)`. `ExceptionAssertions<TException>` then refines the
caught exception with `.And` / `.Which` (the exception itself), `WithMessage(pattern)`,
`WithInnerException<TInner>`, `WithInnerExceptionExactly<TInner>`, `Where(...)` and, for
`ArgumentException`, `WithParameterName(...)`.

`Func<Task>` and `Func<Task<T>>` subjects get the async family instead: `ThrowAsync<TException>`,
`ThrowExactlyAsync<TException>`, `ThrowWithinAsync<TException>(TimeSpan)`, `NotThrowAsync`,
`NotThrowAfterAsync`, `CompleteWithinAsync` and `NotCompleteWithinAsync`. Every one returns a `Task` that
must be awaited. `FluentActions.Invoking`, `Awaiting` and `Enumerating` turn an expression into the
delegate these assertions need:

```csharp
repository.Enumerating(r => r.StreamAll()).Should()
    .Throw<InvalidOperationException>();   // deferred iterator throws
```

`Throw<T>` and `NotThrow` unwrap an `AggregateException` and match against the inner exceptions;
`ThrowExactly<T>` does not.

### Types, members and assemblies

`TypeAssertions` covers `BeDerivedFrom<TBaseClass>`, `Implement<TInterface>`, `BeSealed`, `BeAbstract`,
`BeStatic`, `BeDecoratedWith<TAttribute>`, `HaveProperty<TProperty>(name)`, `HaveMethod`, `HaveIndexer`,
`HaveConstructor`, `HaveDefaultConstructor`, `HaveAccessModifier(CSharpAccessModifier)` and the
conversion-operator assertions. `MethodInfoAssertions`, `PropertyInfoAssertions` and
`ConstructorInfoAssertions` do the same for members, and `AssemblyAssertions` adds `Reference`,
`NotReference`, `DefineType`, `BeUnsigned` and `BeSignedWithPublicKey`.

```csharp
// architecture rule: the domain assembly must not depend on the UI one
typeof(Widget).Assembly.Should().NotReference(typeof(MainWindow).Assembly);
```

To assert over a whole set of types, start from a `TypeSelector`, filter it, and assert on the set:

```csharp
using SilverAssertions.Types;

// CORRECT - start from a TypeSelector to get the type-level assertions:
AllTypes.From(typeof(Widget).Assembly)
    .ThatAreClasses()
    .ThatImplement<IDisposable>()
    .Should().BeSealed();

// ALSO CORRECT - wrap an existing sequence:
new TypeSelector(myTypes).ThatAreClasses().Should().BeSealed();

// WRONG - assembly.GetTypes().ThatAreClasses() is IEnumerable<Type>,
// so .Should() gives a collection assertion with no BeSealed member.
```

### Occurrence constraints

`Exactly`, `AtLeast`, `AtMost`, `MoreThan` and `LessThan` each produce an `OccurrenceConstraint` through
`Once()`, `Twice()`, `Thrice()` and `Times(n)` - except `LessThan`, which has no `Once()`. The int-first
spellings `3.TimesExactly()`, `3.TimesOrLess()` and `3.TimesOrMore()` do the same. They are accepted by
`StringAssertions.Contain`, `StringAssertions.ContainEquivalentOf`, `StringAssertions.MatchRegex`,
`XDocumentAssertions.HaveElement` and `XElementAssertions.HaveElement`, and nowhere else.

```csharp
"a-a-a".Should().Contain("a", Exactly.Times(3));
"banana".Should().ContainEquivalentOf("AN", AtLeast.Twice());
log.Should().MatchRegex(@"\d{4}", 2.TimesOrMore());
doc.Should().HaveElement("item", AtLeast.Twice());
```

### AssertionScope

An `AssertionScope` batches failures: every assertion inside the scope runs, and all of them are
reported together when the scope is disposed. Scopes nest - an inner scope reports its failures to the
outer one instead of throwing - and a scope constructed with a context string renames the subject in
every message inside it. The scope is also the assertion engine that custom assertions write against:
`Execute.Assertion` is a scope, and `BecauseOf`, `ForCondition`, `Given<T>`, `WithExpectation`,
`FailWith`, `AddReportable` and `Discard()` are its members.

### Fluent dates and times

`SilverAssertions.Extensions` builds the values a date or time assertion needs, readably:

```csharp
using SilverAssertions.Extensions;

5.Seconds()        // TimeSpan; also Ticks/Nanoseconds/Microseconds/
250.Milliseconds() // Milliseconds/Seconds/Minutes/Hours/Days,
2.Hours()          // each for int, several also for long/double
1.Hours().And(30.Minutes())

4.July(2026)                   // DateTime (one method per month name)
4.July(2026).At(12, 30)        // At(hours, minutes, seconds = 0, ...)
4.July(2026).At(12, 30).AsUtc()
5.Minutes().Before(deadline)   // DateTime
5.Minutes().After(start)       // DateTime
someDateTime.WithOffset(TimeSpan.Zero)   // DateTimeOffset
someDateTime.Microsecond()  someDateTime.AddNanoseconds(500)
```

The "distance from another moment" assertions read the same way: `BeMoreThan`, `BeAtLeast`, `BeExactly`,
`BeWithin` and `BeLessThan` return a range assertion that you finish with `.Before(target)` or
`.After(target)`.

### Extending it

There are four extension points, cheapest first.

An extension method on an existing assertion class returns the constraint so the chain continues:

```csharp
using SilverAssertions;
using SilverAssertions.Execution;
using SilverAssertions.Primitives;

public static class StringAssertionsExtensions
{
    [CustomAssertion]
    public static AndConstraint<StringAssertions> BeAValidEmailAddress(
        this StringAssertions assertions,
        string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .ForCondition(assertions.Subject is { Length: > 0 } s
                          && s.Contains('@'))
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected {context:string} to be a valid e-mail "
                      + "address{reason}, but found {0}.",
                      assertions.Subject);

        return new AndConstraint<StringAssertions>(assertions);
    }
}

// usage
user.Email.Should().BeAValidEmailAddress().And.EndWith(".com");
```

A custom assertion class derives from `ReferenceTypeAssertions<TSubject,TAssertions>` to inherit
`BeNull`, `BeOfType`, `BeSameAs`, `Match` and `Subject`, overrides `Identifier` to name the subject in
messages, and adds its own `Should()` extension - the full shape is in
[Examples](#examples). In failure messages, `{reason}` is replaced by the because phrase,
`{context:xxx}` by the caller-supplied identifier, and `{0}`, `{1}` by the formatted arguments.

A custom value formatter implements `IValueFormatter` and is registered with `Formatter.AddFormatter`,
or is a static method marked `[ValueFormatter]`:

```csharp
using SilverAssertions.Formatting;

public class MoneyFormatter : IValueFormatter
{
    public bool CanHandle(object value) => value is Money;

    public void Format(object value, FormattedObjectGraph formattedGraph,
        FormattingContext context, FormatChild formatChild)
    {
        var money = (Money)value;
        formattedGraph.AddFragment($"{money.Amount} {money.Currency}");
    }
}

Formatter.AddFormatter(new MoneyFormatter());     // once, e.g. in a fixture
// Formatter.RemoveFormatter(instance) to undo
```

A custom equivalency step implements `IEquivalencyStep` (or derives from `EquivalencyStep<T>` to handle
only expectations assignable to `T`) and is registered per assertion with `o.Using(new MyStep())` or
globally through `AssertionOptions.EquivalencyPlan`, whose `Add`, `AddAfter`, `Insert`, `InsertBefore`,
`Remove`, `Clear` and `Reset` members position it against the built-in steps.

## Examples

A whole test class, asserting on the result of a service call:

```csharp
using SilverAssertions;
using Xunit;

public class UserServiceTests
{
    [Fact]
    public void GetUser_returns_the_expected_user()
    {
        // Arrange
        var service = new UserService();

        // Act
        var user = service.GetUser(1);

        // Assert
        user.Should().NotBeNull();
        user.Name.Should().Be("Alice");
        user.Age.Should().BeGreaterThan(0).And.BeLessThan(150);
        user.Email.Should().Contain("@").And.EndWith(".com");
        user.Roles.Should().NotBeEmpty().And.Contain("admin");
    }
}
```

Async assertions - throwing, completing within a time span, and producing a result:

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SilverAssertions;
using Xunit;

public class UserServiceAsyncTests
{
    [Fact]
    public async Task LoadAsync_rejects_a_negative_id()
    {
        var service = new UserService();

        Func<Task> act = () => service.LoadAsync(-1);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("id");
    }

    [Fact]
    public async Task GetUsersAsync_completes_quickly_and_is_sorted()
    {
        var service = new UserService();

        Func<Task<IReadOnlyList<User>>> act = () => service.GetUsersAsync();

        var users = (await act.Should()
            .CompleteWithinAsync(TimeSpan.FromSeconds(2))).Which;

        users.Should().HaveCountGreaterThan(0);
        users.Should().OnlyContain(u => u.IsActive);
        users.Should().BeInAscendingOrder(u => u.Name);
    }

    [Fact]
    public async Task CountAsync_returns_three()
    {
        var service = new UserService();

        Func<Task<int>> act = () => service.CountAsync();

        await act.Should().CompleteWithinAsync(TimeSpan.FromSeconds(2))
            .WithResult(3);
    }
}
```

Object-graph comparison, first with an anonymous partial expectation and then with options:

```csharp
using SilverAssertions;
using Xunit;

public class OrderServiceTests
{
    [Fact]
    public void CreateOrder_returns_the_expected_order()
    {
        var service = new OrderService();

        var order = service.CreateOrder(userId: 1, productId: 42,
                                        quantity: 3);

        // The anonymous expectation names only the members that matter,
        // so Id and CreatedAt are simply not compared.
        order.Should().BeEquivalentTo(new
        {
            UserId = 1,
            ProductId = 42,
            Quantity = 3,
            Status = OrderStatus.Pending
        });
    }

    [Fact]
    public void CreateOrder_matches_the_prototype_except_for_identity()
    {
        var service = new OrderService();
        var expected = OrderPrototype.Pending(userId: 1, productId: 42);

        var order = service.CreateOrder(userId: 1, productId: 42,
                                        quantity: 3);

        // Here the expectation IS an Order, so Excluding can name its
        // members - and every other member is compared, including
        // nested Customer and the Lines collection.
        order.Should().BeEquivalentTo(expected, options => options
            .Excluding(o => o.Id)
            .Excluding(o => o.CreatedAt)
            .WithStrictOrderingFor(o => o.Lines)
            .ComparingByMembers<Money>());
    }
}
```

Event monitoring end to end, and a collection validated inside an `AssertionScope` so that every
violated rule is reported at once:

```csharp
using System.ComponentModel;
using SilverAssertions;
using Xunit;

public class PersonTests
{
    [Fact]
    public void Setting_the_name_raises_PropertyChanged()
    {
        // Arrange
        var person = new Person { Name = "Bob" };

        using var monitor = person.Monitor();

        // Act
        person.Name = "Alice";

        // Assert
        monitor.Should().Raise(nameof(INotifyPropertyChanged.PropertyChanged))
            .WithSender(person)
            .WithArgs<PropertyChangedEventArgs>(
                args => args.PropertyName == nameof(Person.Name));

        monitor.Should().NotRaise("Deleted");
        monitor.GetRecordingFor("PropertyChanged").Should().HaveCount(1);
    }
}
```

```csharp
using System;
using SilverAssertions;
using SilverAssertions.Execution;
using Xunit;

public class ProcessingTests
{
    [Fact]
    public void All_processed_items_are_valid()
    {
        var items = Pipeline.GetProcessedItems();

        using (new AssertionScope("the processed items"))
        {
            items.Should().NotBeEmpty();
            items.Should().OnlyHaveUniqueItems(i => i.Id);
            items.Should().AllSatisfy(item =>
            {
                item.Name.Should().NotBeNullOrWhiteSpace();
                item.Price.Should().BePositive();
                item.CreatedAt.Should().BeBefore(DateTime.UtcNow);
            });
        }
        // every violated rule is reported, not just the first
    }
}
```

A custom assertion class for a domain type, with its `Identifier`, the `[CustomAssertion]` marker, the
`Given`/`Then`/`ClearExpectation` chain and its own `.Should()`:

```csharp
using SilverAssertions;
using SilverAssertions.Execution;
using SilverAssertions.Primitives;
using Xunit;

public class Invoice
{
    public string Number { get; set; }
    public decimal Total { get; set; }
    public bool IsPaid { get; set; }
}

public class InvoiceAssertions
    : ReferenceTypeAssertions<Invoice, InvoiceAssertions>
{
    public InvoiceAssertions(Invoice subject) : base(subject) { }

    protected override string Identifier => "invoice";

    [CustomAssertion]
    public AndConstraint<InvoiceAssertions> BeSettled(
        string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Expected {context:invoice} to be "
                             + "settled{reason}, ")
            .Given(() => Subject)
            .ForCondition(invoice => invoice is not null)
            .FailWith("but it was <null>.")
            .Then
            .ForCondition(invoice => invoice.IsPaid)
            .FailWith("but {0} is still outstanding on {1}.",
                      invoice => invoice.Total,
                      invoice => invoice.Number)
            .Then
            .ClearExpectation();

        return new AndConstraint<InvoiceAssertions>(this);
    }
}

public static class InvoiceExtensions
{
    public static InvoiceAssertions Should(this Invoice invoice)
        => new InvoiceAssertions(invoice);
}

public class InvoiceTests
{
    [Fact]
    public void A_paid_invoice_is_settled()
    {
        var invoice = new Invoice
        {
            Number = "INV-1", Total = 0m, IsPaid = true
        };

        invoice.Should().BeSettled().And.NotBeNull();
    }
}
```

## Using it in a CodeBrix.Platform application

Nothing special is needed. A CodeBrix.Platform application is tested the way any other .NET application
is: a `net10.0` test project references this package plus the test framework and runner of your choice,
and asserts on the view models, services and libraries the application is built from. The library has no
native dependencies and no OS restrictions, so the same test project runs on Windows, macOS and Linux.

Here is that project file, in full:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit.v3" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="SilverAssertions.ApacheLicenseForever" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MyProject\MyProject.csproj" />
  </ItemGroup>

</Project>
```

## Pitfalls

- The package id is `SilverAssertions.ApacheLicenseForever`; the namespace is `SilverAssertions`. Do not
  use the package id as a using.
- Two `.Should()` extension sets on the same type are ambiguous and the project will not compile. Add
  one fluent assertion library to a test project, not two.
- `using SilverAssertions;` alone is not enough for `AssertionScope` (`.Execution`),
  `CSharpAccessModifier` (`.Common`), `AllTypes` / `TypeSelector` (`.Types`), `Formatter`
  (`.Formatting`) or the `5.Seconds()`-style helpers (`.Extensions`).
- Forgetting to await an async assertion silently passes. `ThrowAsync`, `NotThrowAsync`,
  `CompleteWithinAsync`, `NotCompleteWithinAsync`, `ThrowWithinAsync`, `ThrowExactlyAsync`,
  `NotThrowAfterAsync` and the awaitable `WithMessage` / `WithParameterName` / `WithResult` all return a
  `Task` that must be awaited.
- `BeCloseTo` and `BeApproximately` are not interchangeable. `BeCloseTo` exists only for the integral
  numeric types and takes an unsigned delta (`42.Should().BeCloseTo(45, 5u)`); for `float`, `double` and
  `decimal` use `BeApproximately(expectedValue, precision)`. `DateTime`, `TimeSpan`, `TimeOnly` and
  `DateTimeOffset` do have their own `BeCloseTo`, taking a `TimeSpan` precision.
- In `BeEquivalentTo` the options lambda is typed on the expectation. If the expectation is an anonymous
  object, `options.Excluding(o => o.Id)` does not compile unless that anonymous object has an `Id`.
- `BeEquivalentTo` ignores collection order by default. Use `WithStrictOrdering()`, or `Equal` /
  `ContainInOrder` / `ContainInConsecutiveOrder` on the collection assertion, when order is part of the
  contract.
- The filters on a plain `IEnumerable<Type>` return `IEnumerable<Type>`, whose `.Should()` is a
  collection assertion. To reach `BeSealed` / `BeInNamespace` / `BeDecoratedWith` you need a
  `TypeSelector` - start from `AllTypes.From(assembly)` or `new TypeSelector(types)`.
- Reflection assertions run against runtime metadata, and need full signatures. `HaveIndexer` takes the
  indexer type and the parameter types; `HaveExplicitMethod` takes the interface, the name and the
  parameter types; `[Serializable]` is a metadata flag rather than a stored custom attribute, so
  `BeDecoratedWith<SerializableAttribute>()` fails even for types that carry it in source.
- An `async void` method cannot be asserted as an `Action`: the delegate assertions detect the
  compiler-generated state machine and throw `InvalidOperationException`. Assign it to a `Func<Task>` and
  use the async assertions.
- `ThrowExactly<T>` does not unwrap `AggregateException` while `Throw<T>` does, so a Task-based API that
  wraps its failure will be reported as `AggregateException`.
- `AssertionOptions` and `Formatter` changes are global and persist for the whole test run. Reset them in
  teardown or tests will influence each other, especially under parallel execution.
- Mark helper methods that wrap assertions with `[CustomAssertion]`, or caller identification names your
  helper's local variable in the failure message instead of the subject under test.
- Dispose an event monitor - prefer `using var monitor = x.Monitor();`. It subscribes to every event on
  the subject until disposed.
- In a custom assertion, the `ForCondition` argument is evaluated eagerly even after an earlier condition
  failed, so a null check followed by a member access throws `NullReferenceException`. Use `Given<T>`,
  whose conditions are lambdas evaluated only while the chain is still succeeding.
- Inside a custom `IValueFormatter.Format`, do not call `Formatter.ToString`; call the supplied
  `FormatChild` delegate so cyclic references stay detected. Attribute-based `[ValueFormatter]` methods
  are not found at all unless detection is enabled - the default is `Disabled`.
- Do not pass `--nologo` to `dotnet test` in Microsoft Testing Platform mode. It is a VSTest-only switch;
  the SDK forwards it to the test application, which rejects it and exits before discovery, so the run
  reports "Zero tests ran" and nothing executes. The spelling to use is `dotnet test -- --no-banner`.
- `BeEquivalentTo` walks the whole object graph with reflection and is the most expensive assertion in
  the library. For a single value `x.Should().Be(y)` is far cheaper; narrow a graph with `Including` /
  `Excluding` or an anonymous expectation rather than accepting the whole walk.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/SilverAssertions/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/SilverAssertions/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/SilverAssertions/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API, one file per assertion group) | [tests/SilverAssertions.Tests](https://github.com/ellisnet/SilverAssertions/tree/main/tests/SilverAssertions.Tests) |
| Equivalency tests (object graphs, collections, records, member rules) | [tests/SilverAssertions.Equivalency.Tests](https://github.com/ellisnet/SilverAssertions/tree/main/tests/SilverAssertions.Equivalency.Tests) |
| Per-framework adapter tests | [tests/TestFrameworks](https://github.com/ellisnet/SilverAssertions/tree/main/tests/TestFrameworks) |

The repository ships no sample applications: the test projects are the worked examples, and the
AGENT-README maps each feature to the test file that demonstrates it.

## License

SilverAssertions is licensed under the Apache License 2.0; the license is also named in the package ID
(`SilverAssertions.ApacheLicenseForever`). For the provenance and licensing of open source code included
in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/SilverAssertions/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.TestMocks](CodeBrix.TestMocks.md) - mocks, generated test data and xUnit v3 data attributes, the natural partner for this package
- [Testing your application](../platform/10-testing-your-application.md) - how a CodeBrix.Platform application is tested end to end
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/SilverAssertions on GitHub](https://github.com/ellisnet/SilverAssertions) - source and tests
