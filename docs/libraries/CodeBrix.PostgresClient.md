<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.PostgresClient</sub>

# CodeBrix.PostgresClient

**CodeBrix.PostgresClient is a fully managed ADO.NET data provider for PostgreSQL, with a built-in
lightweight object mapper.** It speaks the PostgreSQL wire protocol directly, with no native client
library, and covers the whole ADO.NET surface synchronously and asynchronously: connections and pooled
data sources, commands, readers, batches, transactions, binary and text COPY, LISTEN / NOTIFY, logical and
physical replication, multi-host failover and rich PostgreSQL type mapping. `PgSqlMapper` adds query and
command extension methods that turn rows into objects without command and reader boilerplate. Use it from
any .NET 10 application, or from a CodeBrix.Platform application that talks to a PostgreSQL server.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.PostgresClient](https://github.com/ellisnet/CodeBrix.PostgresClient) |
| **Packages** | [`CodeBrix.PostgresClient.PostgreSqlLicenseForever`](https://www.nuget.org/packages/CodeBrix.PostgresClient.PostgreSqlLicenseForever) |
| **License** | PostgreSQL License; see [License](#license) |
| **Requires** | .NET 10 or later, and a reachable PostgreSQL server. One Microsoft-published dependency arrives with the package: `Microsoft.Extensions.Logging.Abstractions` |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Any operating system .NET 10 runs on - fully managed, with no native assets |

## What it does

- Implements the ADO.NET surface: `PgSqlConnection`, `PgSqlCommand`, `PgSqlDataReader`,
  `PgSqlTransaction`, `PgSqlParameter`, `PgSqlBatch`, `PgSqlDataAdapter`, `PgSqlCommandBuilder` and
  `PgSqlFactory`, each with synchronous and asynchronous forms and cancellation.
- Provides `PgSqlDataSource`, a thread-safe connection factory that owns the pool, the type mappings,
  logging, SSL and authentication setup, built with `PgSqlDataSourceBuilder`.
- Connects to several hosts with failover, load balancing and target session attributes (primary,
  standby, read-write, read-only), and multiplexes commands from many callers over a few physical
  connections.
- Prepares statements explicitly or automatically, and batches several statements into one round trip.
- Moves rows in bulk with binary and text COPY, import and export.
- Streams logical replication (the `pgoutput` and `test_decoding` plugins) and physical replication, and
  receives LISTEN / NOTIFY notifications.
- Maps PostgreSQL types richly: arrays, ranges and multiranges, composites, enums, domains, records, JSON
  and JSONB (including `System.Text.Json` serialization of your own types), hstore, ltree, cube, network,
  geometric and bit-string types, intervals, full-text search, and date and time types with infinity
  handling.
- Authenticates with cleartext, MD5 and SCRAM-SHA-256 passwords, SSL/TLS with client certificates and
  channel binding, Kerberos/GSSAPI, and `.pgpass` files.
- Takes part in two-phase commit and `System.Transactions`, including distributed transactions.
- Emits tracing through `System.Diagnostics.ActivitySource` and metrics through
  `System.Diagnostics.Metrics`, both named `PgSql`.
- Maps rows to objects with `PgSqlMapper`: `Query`, `QueryFirst`, `QuerySingle` (and their `OrDefault`
  forms), `Execute`, `ExecuteScalar`, `ExecuteReader` and `QueryMultiple`, synchronous and asynchronous,
  with anonymous-object parameters and IN-list expansion.

## When to use it

Use CodeBrix.PostgresClient whenever a .NET application talks to PostgreSQL - from simple queries
through bulk COPY, notifications, logical replication and multi-host deployments - with `PgSqlMapper` for
everyday row-to-object code and the ADO.NET types when you need full control.

What it does not do:

- It is not an Entity Framework Core provider and not a full ORM. `PgSqlMapper` has no change tracking,
  LINQ provider, relationships, multi-mapping or SQL generation - you write the SQL.
- It ships no plugin type mappings for third-party libraries (geometry objects, alternative date-time or
  JSON libraries). PostGIS values can still travel as text or WKB bytes; JSON goes through
  `System.Text.Json`.
- It has no OpenTelemetry package and no dependency-injection helpers. It emits plain `ActivitySource`
  and `Meter` data named `PgSql`, and you register the data source yourself.
- It does not run, embed or manage a PostgreSQL server, migrations or schema. It is a client.
- It speaks only the PostgreSQL protocol, and its multi-host support chooses which listed server to
  connect to; it does not promote standbys or manage a cluster.

## Getting started

```bash
dotnet add package CodeBrix.PostgresClient.PostgreSqlLicenseForever
```

The package ID carries the license suffix; the namespaces do not. You install
`CodeBrix.PostgresClient.PostgreSqlLicenseForever` and you write `using CodeBrix.PostgresClient;`. The
types carry the `PgSql` prefix - `PgSqlConnection`, `PgSqlCommand`, `PgSqlDbType`.

```csharp
using System;
using CodeBrix.PostgresClient;

//One data source for the application lifetime
await using var dataSource = PgSqlDataSource.Create(
    "Host=localhost;Username=app;Password=secret;Database=shop");

//A data-source command needs no connection handling
await using (var ddl = dataSource.CreateCommand(
    "CREATE TABLE IF NOT EXISTS products (id serial PRIMARY KEY, " +
    "name text NOT NULL, unit_price numeric(10,2) NOT NULL, " +
    "created_at timestamptz NOT NULL DEFAULT now())"))
{
    await ddl.ExecuteNonQueryAsync();
}

await using var connection = await dataSource.OpenConnectionAsync();

//Named parameters
await using (var insert = new PgSqlCommand(
    "INSERT INTO products (name, unit_price) VALUES (@name, @price) " +
    "RETURNING id", connection))
{
    insert.Parameters.AddWithValue("name", "Widget");
    insert.Parameters.AddWithValue("price", 9.99m);
    int newId = (int)await insert.ExecuteScalarAsync();
    Console.WriteLine($"inserted {newId}");
}

//Positional parameters ($1) with a typed, non-boxing parameter
await using (var query = new PgSqlCommand(
    "SELECT id, name, unit_price, created_at FROM products " +
    "WHERE unit_price < $1", connection))
{
    query.Parameters.Add(new PgSqlParameter<decimal> { TypedValue = 20m });

    await using var reader = await query.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        int id = reader.GetInt32(0);
        string name = reader.GetString(1);
        decimal price = reader.GetFieldValue<decimal>(2);
        DateTime created = reader.GetFieldValue<DateTime>(3); // Kind=Utc
        Console.WriteLine($"{id} {name} {price} {created:O}");
    }
}
```

The other namespaces name the areas they cover; ordinary code needs only the first two.

| Namespace | What it holds |
| --- | --- |
| `CodeBrix.PostgresClient` | Connections, data sources, commands, parameters, readers, batches, transactions, exceptions, `PgSqlMapper`, `TargetSessionAttributes`, `SslMode` |
| `CodeBrix.PostgresClient.PgSqlTypes` | `PgSqlDbType`, `PgSqlRange<T>`, `PgSqlInterval`, and the geometric, network and full-text types |
| `CodeBrix.PostgresClient.Replication` | The replication connections, `ReplicationMessage`, `XLogDataMessage` |
| `CodeBrix.PostgresClient.Replication.PgOutput` (and `.Messages`) | `pgoutput` slots and options, and `InsertMessage`, `UpdateMessage` and the rest |
| `CodeBrix.PostgresClient.Replication.TestDecoding` | The `test_decoding` plugin |
| `CodeBrix.PostgresClient.NameTranslation` | The snake_case name translator |
| `CodeBrix.PostgresClient.Schema` | `PgSqlDbColumn` |
| `CodeBrix.PostgresClient.PostgresTypes` | `PostgresType` and its subclasses |
| `CodeBrix.PostgresClient.TypeMapping` | `IPgSqlTypeMapper` |

## Key concepts

### One data source per connection string

`PgSqlDataSource` owns the connection pool and all configuration. Create one per connection string for
the application's lifetime - register it as a singleton - and take connections or commands from it.
Opening a connection from the pool is cheap; holding one is not, so open a connection per unit of work
and dispose it promptly.

```csharp
static PgSqlDataSource Create(string connectionString)
static PgSqlDataSource Create(PgSqlConnectionStringBuilder builder)

PgSqlConnection CreateConnection()               // not yet open
PgSqlConnection OpenConnection()
ValueTask<PgSqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
PgSqlCommand CreateCommand(string commandText = null)   // runs on its own pooled connection
PgSqlBatch CreateBatch()
void ReloadTypes()  /  Task ReloadTypesAsync(CancellationToken = default)
```

A command created by the data source needs no connection handling and cannot take part in a
transaction. `ReloadTypes` re-reads the database's types after a `CREATE TYPE` or `CREATE EXTENSION`.

`PgSqlDataSourceBuilder` configures everything else and every method returns the builder, so calls
chain: `UseLoggerFactory`, `EnableParameterLogging`, `ConfigureTracing`, `MapEnum<TEnum>`,
`MapComposite<T>`, `ConfigureJsonOptions`, `EnableDynamicJson`, the certificate and password-provider
callbacks (including `UsePeriodicPasswordProvider` for rotating cloud tokens),
`UsePhysicalConnectionInitializer`, then `Build()` or `BuildMultiHost()`.
`PgSqlSlimDataSourceBuilder` is the trimming- and NativeAOT-friendly variant: it starts with only the
basic types and you opt in to arrays, ranges, JSON and the rest.

### Parameters: named or positional, never both

- Named placeholders - `WHERE id = @id` (`:id` is also accepted) - take parameters with a
  `ParameterName`. The SQL is rewritten to PostgreSQL's native `$1` form, and a command may hold several
  `;`-separated statements.
- Positional placeholders - `WHERE id = $1` - take parameters with no name, added in order. The SQL is
  sent as written, which is slightly faster, and the command must hold exactly one statement.

`PgSqlParameter` carries `Value` (use `DBNull.Value` for SQL NULL - a null `Value` throws),
`PgSqlDbType` or `DataTypeName` to force a PostgreSQL type, and `DbType`. `PgSqlParameter<T>` with
`TypedValue` avoids boxing. Arrays and ranges are combined flags: `PgSqlDbType.Array | PgSqlDbType.Integer`
is `integer[]`, and an `int[]` value infers it with no type needed.

### Reading results

`PgSqlDataReader.GetFieldValue<T>` is the strongly typed path for every mapped type - `int[]`,
`PgSqlRange<int>`, `DateOnly`, `TimeOnly`, `Guid`, `JsonDocument`, `Dictionary<string,string>` for
hstore, your mapped enums and composites. Use `CommandBehavior.SequentialAccess` for very large columns
and read them with `GetStream` or `GetTextReader`.

### Batches and transactions

`PgSqlBatch` sends several statements, each with its own parameters, in one round trip. Without error
barriers the whole batch runs in an implicit transaction, so a failure rolls all of it back; set
`EnableErrorBarriers` to let later commands run after a failure.

`BeginTransaction` / `BeginTransactionAsync` on an open connection starts an explicit transaction, with
savepoints through `Save`, `Rollback(name)` and `Release`. Disposing an uncommitted transaction rolls it
back. With `Enlist=true` (the default) a connection opened inside a `TransactionScope` enlists
automatically, and two connections in one scope escalate to a two-phase transaction.

### COPY

All COPY APIs hang off an open `PgSqlConnection`, and while a COPY is in progress the connection can do
nothing else. `BeginBinaryImport` returns a `PgSqlBinaryImporter` (`StartRow`, `Write`, `WriteNull`,
`WriteRow`, `Complete`); `BeginBinaryExport` returns a `PgSqlBinaryExporter`; `BeginTextImport` and
`BeginTextExport` return a writer and a reader over tab-separated or CSV lines; and
`BeginRawBinaryCopy` gives raw bytes for piping one server's COPY straight into another.

### Notifications and replication

LISTEN / NOTIFY delivery happens on a dedicated connection you keep open: subscribe to its
`Notification` event, execute `LISTEN channel_name`, and loop on `Wait` or `WaitAsync`. Notifications
arrive only while the connection is waiting or executing a command. The `Notice` event receives server
notices such as `RAISE NOTICE` output.

Replication uses its own connection types, `LogicalReplicationConnection` and
`PhysicalReplicationConnection`, in `CodeBrix.PostgresClient.Replication`. They open with
`Open(cancellationToken)` - there is no `OpenAsync` - and report progress back to the server with
`SetReplicationStatus` and `SendStatusUpdate`.

### Multi-host

List several hosts in `Host=` and build with `BuildMultiHost()` to get a `PgSqlMultiHostDataSource`.
`OpenConnectionAsync(TargetSessionAttributes)` picks a suitable server, and
`WithTargetSession(TargetSessionAttributes.Standby)` returns a cached data-source view - handy to inject
as "the read pool". Hosts are tried in order, or round-robin with `Load Balance Hosts=true`, and a host
that fails is skipped until `Host Recheck Seconds` passes.

### Connection strings

`Key=Value` pairs separated by `;`, with case-insensitive keys; either the key name (`Maximum Pool
Size`) or the builder property name (`MaxPoolSize`) is accepted, and `PgSqlConnectionStringBuilder`
exposes each as a typed property. The keys cover connection (`Host`, `Port`, `Database`, `Username`,
`Password`, `Passfile`, `Application Name`, `Search Path`), security (`SSL Mode`, `Channel Binding`,
`Include Error Detail`), pooling (`Maximum Pool Size` defaults to 100), timeouts (`Timeout` 15 seconds,
`Command Timeout` 30), performance (`Max Auto Prepare`, `Multiplexing`) and multi-host settings. A
handful of standard PostgreSQL environment variables - `PGUSER`, `PGPASSWORD`, `PGPASSFILE` and others -
fill settings the string leaves out; host, port and database are never read from the environment.

### PgSqlMapper

Extension methods on `PgSqlConnection`, in the root namespace. Every method takes
`(string sql, object param = null, PgSqlTransaction transaction = null)`, and every async form adds a
`CancellationToken`.

- A closed connection is opened for the call and closed again; an already-open one is left open, so open
  it yourself to run several calls on one connection or inside a transaction. `Query<T>` results are
  fully buffered.
- Parameters are an anonymous object, a POCO or an `IDictionary<string, object>`, referenced as `@name`
  (`:name` is not bound by the mapper). Any sequence other than a string or `byte[]` expands to an
  IN-list: write `IN @ids`, without parentheses. A value that is a `PgSqlParameter` is added as-is.
- Simple result types take the first column of each row; any other `T` is built as a POCO with a public
  parameterless constructor, binding columns to writable public properties case-insensitively and
  ignoring underscores, so `unit_price` fills `UnitPrice` with no alias.
- `QueryMultiple` returns a `PgSqlGridReader` whose `Read<T>()` materializes one result set at a time.

### Logging, tracing and metrics

Give the data source an `ILoggerFactory` with `UseLoggerFactory`; categories are `PgSql.Connection`,
`PgSql.Command`, `PgSql.Transaction`, `PgSql.Copy`, `PgSql.Replication` and `PgSql.Exception`, and the
connection string is never passed to the logger. Commands, batches and COPY operations emit activities
from the `ActivitySource` named `PgSql`, following the OpenTelemetry database conventions, and the
`Meter` named `PgSql` publishes operation durations, failures and pool counts tagged with the data
source's `Name`.

### The error model

| Exception | Means |
| --- | --- |
| `PgSqlException` | the base, a `DbException`; `IsTransient` is true for network and timeout faults and for retryable server errors, and `BatchCommand` names the batch command that failed |
| `PostgresException` | an error reported by the server, with `SqlState`, `MessageText`, `Detail`, `Hint`, `TableName`, `ColumnName`, `ConstraintName` and the rest |
| `PgSqlOperationInProgressException` | a second command was started on a busy connection |

`PostgresErrorCodes` holds the `SqlState` strings as constants - `UniqueViolation`,
`ForeignKeyViolation`, `SerializationFailure`, `DeadlockDetected` and so on. `PostgresException.Detail`
is empty unless the connection string sets `Include Error Detail=true`, because it can contain row data.

## Examples

Mapping rows to objects, with an IN-list and a write.

```csharp
using CodeBrix.PostgresClient;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal UnitPrice { get; set; }   // filled from a unit_price column
}

await using var connection = new PgSqlConnection(
    "Host=localhost;Username=app;Password=secret;Database=shop");

var cheap = await connection.QueryAsync<Product>(
    "SELECT id, name, unit_price FROM products WHERE unit_price < @max ORDER BY name",
    new { max = 20m });

var selected = await connection.QueryAsync<Product>(
    "SELECT id, name, unit_price FROM products WHERE id IN @ids",
    new { ids = new[] { 3, 5, 8 } });

int updated = await connection.ExecuteAsync(
    "UPDATE products SET unit_price = unit_price * 1.1 WHERE id = @id",
    new { id = 3 });
```

A bulk import with binary COPY. `CompleteAsync` commits the import; disposing without it discards every
row.

```csharp
using CodeBrix.PostgresClient;
using CodeBrix.PostgresClient.PgSqlTypes;

await using var connection = await dataSource.OpenConnectionAsync();
await using var importer = await connection.BeginBinaryImportAsync(
    "COPY products (name, unit_price) FROM STDIN (FORMAT BINARY)");

foreach (var (name, price) in rows)
{
    await importer.StartRowAsync();
    await importer.WriteAsync(name, PgSqlDbType.Text);
    await importer.WriteAsync(price, PgSqlDbType.Numeric);
}

await importer.CompleteAsync();
```

A read pool and a write pool from one list of hosts.

```csharp
using CodeBrix.PostgresClient;

var builder = new PgSqlDataSourceBuilder(
    "Host=pg1,pg2,pg3;Username=app;Password=secret;Database=shop;" +
    "Load Balance Hosts=true");
await using PgSqlMultiHostDataSource cluster = builder.BuildMultiHost();

PgSqlDataSource readPool = cluster.WithTargetSession(TargetSessionAttributes.PreferStandby);

await using var writer = await cluster.OpenConnectionAsync(TargetSessionAttributes.Primary);
```

## Pitfalls

- `DateTime` and `timestamptz`. A `DateTime` with `Kind=Utc` writes as `timestamptz`; `Local` or
  `Unspecified` writes as `timestamp` without time zone, and a local value inserted into a `timestamptz`
  column is shifted by the server's session time zone. Store instants as UTC, use `timestamp` only for
  wall-clock values, and expect `timestamptz` to read back as `Kind=Utc`.
- Forgetting to dispose. A connection that is not disposed never returns to the pool, and opening
  eventually fails with a pool-exhausted error. Cache the `PgSqlDataSource`, never an open connection.
- One command at a time per connection. Starting a second while a reader is open throws
  `PgSqlOperationInProgressException`; a connection is not thread-safe, so never share one across
  parallel tasks.
- Multiplexing limits. With `Multiplexing=true`, synchronous execution throws (the mapper's synchronous
  methods included), explicit `Prepare()` is not supported, transactions must start with
  `BeginTransaction`, and it cannot be combined with multiple hosts.
- Mixing named and positional placeholders in one command throws, and a positional command may hold only
  one statement - use `PgSqlBatch` for several.
- The mapper expands every sequence into an IN-list, so `= ANY(@ids)` with an `int[]` becomes invalid
  SQL. Write `IN @ids`, or pass a `PgSqlParameter` as the value to bind one array. With a plain
  `PgSqlCommand` it is the other way round: an `int[]` binds as one array, so write `= ANY(@ids)`.
- The mapper binds .NET enums as their underlying integer, which fails against a PostgreSQL enum column.
  Pass a `PgSqlParameter` (with `MapEnum` on the data source), or pass the label text and cast in SQL.
- Type mappings live on the data source. `MapEnum`, `MapComposite` and `EnableDynamicJson` apply only to
  connections from the data source built with them, not to a separate `new PgSqlConnection(...)`.
- `PgSqlMapper` uses reflection and is marked for it; in a trimmed or NativeAOT application use
  `PgSqlCommand` and `PgSqlDataReader` with `PgSqlSlimDataSourceBuilder` instead.
- `QuerySingle` and `QueryFirst` throw on an empty result; use the `OrDefault` forms when no row is
  normal.
- Abandoning a permanent replication slot makes the server keep WAL forever. Call
  `SetReplicationStatus` as you process messages, and drop slots you no longer use.
- The converter and type-resolver extension points are `[Experimental]` and raise `NPG9001`, `NPG9002`
  and `NPG9003` as errors until you opt in with `<NoWarn>`. Ordinary type mapping never needs them.

## Samples and tools in the repository

The repository ships no sample applications. Its non-package content is the build-time source generator
and the test suite, which is the executable documentation for the package.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Tests | The whole provider and the mapper, against a real server | [`tests/CodeBrix.PostgresClient.Tests`](https://github.com/ellisnet/CodeBrix.PostgresClient/tree/main/tests/CodeBrix.PostgresClient.Tests) |
| Source generator | The build-time generator that writes part of `PgSqlConnectionStringBuilder`; not packed | [`src/CodeBrix.PostgresClient.SourceGenerators`](https://github.com/ellisnet/CodeBrix.PostgresClient/tree/main/src/CodeBrix.PostgresClient.SourceGenerators) |

The database tests run against a throw-away PostgreSQL container that the test run starts on first use
through [CodeBrix.Docker](CodeBrix.Docker.md) and removes at the end, so they need a reachable Docker
daemon in Linux-container mode. Tests that never touch the database never start it.

Test files worth opening first, one per feature area:

| Feature | Test file |
| --- | --- |
| `PgSqlMapper` | [`PgSqlMapperTests.cs`](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/tests/CodeBrix.PostgresClient.Tests/PgSqlMapperTests.cs) |
| Connections and pooling | [`ConnectionTests.cs`](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/tests/CodeBrix.PostgresClient.Tests/ConnectionTests.cs) |
| Data sources and the builder | [`DataSourceTests.cs`](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/tests/CodeBrix.PostgresClient.Tests/DataSourceTests.cs) |
| Commands and parameters | [`CommandTests.cs`](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/tests/CodeBrix.PostgresClient.Tests/CommandTests.cs) |
| Readers | [`ReaderTests.cs`](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/tests/CodeBrix.PostgresClient.Tests/ReaderTests.cs) |
| Batching | [`BatchTests.cs`](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/tests/CodeBrix.PostgresClient.Tests/BatchTests.cs) |
| Prepared statements | [`PrepareTests.cs`](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/tests/CodeBrix.PostgresClient.Tests/PrepareTests.cs) |
| Transactions and savepoints | [`TransactionTests.cs`](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/tests/CodeBrix.PostgresClient.Tests/TransactionTests.cs) |
| COPY | [`CopyTests.cs`](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/tests/CodeBrix.PostgresClient.Tests/CopyTests.cs) |
| LISTEN / NOTIFY | [`NotificationTests.cs`](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/tests/CodeBrix.PostgresClient.Tests/NotificationTests.cs) |
| Multi-host | [`MultipleHostsTests.cs`](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/tests/CodeBrix.PostgresClient.Tests/MultipleHostsTests.cs) |
| Logical replication | [`Replication/PgOutputReplicationTests.cs`](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/tests/CodeBrix.PostgresClient.Tests/Replication/PgOutputReplicationTests.cs) |
| Type mapping (one file per type family) | [`Types/`](https://github.com/ellisnet/CodeBrix.PostgresClient/tree/main/tests/CodeBrix.PostgresClient.Tests/Types) - for example [`DateTimeTests.cs`](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/tests/CodeBrix.PostgresClient.Tests/Types/DateTimeTests.cs) and [`JsonTests.cs`](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/tests/CodeBrix.PostgresClient.Tests/Types/JsonTests.cs) |
| Tracing and metrics | [`TracingTests.cs`](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/tests/CodeBrix.PostgresClient.Tests/TracingTests.cs) · [`MetricTests.cs`](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/tests/CodeBrix.PostgresClient.Tests/MetricTests.cs) |

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/AGENT-README.txt) |
| Non-package content in the repository | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.PostgresClient.Tests](https://github.com/ellisnet/CodeBrix.PostgresClient/tree/main/tests/CodeBrix.PostgresClient.Tests) |

XML documentation ships alongside the assembly, and every public member is documented.

## License

CodeBrix.PostgresClient is licensed under the PostgreSQL License (SPDX `PostgreSQL`), and the license is
also named in the package ID (`CodeBrix.PostgresClient.PostgreSqlLicenseForever`). The package requires
license acceptance. The repository's
[LICENSE](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/LICENSE) carries an earlier
copyright notice alongside the CodeBrix one, as the license requires. For the provenance and licensing
of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.PostgresClient/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Sqlite](CodeBrix.Sqlite.md) - the local-storage sibling, for data that lives with the application
- [CodeBrix.Redis](CodeBrix.Redis.md) - a managed Redis client, for caching and messaging beside the database
- [CodeBrix.Docker](CodeBrix.Docker.md) - stand a PostgreSQL server up in a container from .NET
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.PostgresClient on GitHub](https://github.com/ellisnet/CodeBrix.PostgresClient) - source and tests
