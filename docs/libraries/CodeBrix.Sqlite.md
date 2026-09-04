<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Sqlite</sub>

# CodeBrix.Sqlite

**CodeBrix.Sqlite is a fully managed, cross-platform SQLite library that adds selective encryption to
an ordinary SQLite file.** It opens databases with modern defaults, maps rows onto your own types
through CRUD extension methods, encrypts the values you mark, searches encrypted data through an HMAC
blind index, and backs up a live database safely. You reach for it from any .NET 10 application, or
from a CodeBrix.Platform application; the encryption features are entirely optional, and with no crypt
engine the library is a convenience layer plus a mapper.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Sqlite](https://github.com/ellisnet/CodeBrix.Sqlite) |
| **Packages** | [`CodeBrix.Sqlite.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Sqlite.ApacheLicenseForever) |
| **License** | Apache 2.0; see [License](#license) |
| **Requires** | .NET 10 or later. No native prerequisites: the SQLite engine itself ships inside the `SQLitePCLRaw` bundle the package brings with it |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Windows, Linux and macOS, with nothing installed on the machine |

## What it does

- Opens SQLite databases with modern defaults - write-ahead logging and enforced foreign keys -
  through the `SqliteDatabase` entry point, with synchronous and asynchronous APIs throughout.
- Maps result rows onto your own types with CRUD extension methods on `SqliteConnection`, binding
  columns case-insensitively **and** ignoring underscores, so a `customer_tier` column fills a
  `CustomerTier` property with no alias, no attribute and no configuration.
- Encrypts individual column values and whole CLR objects through a pluggable crypt engine, with a
  ready-to-use AES-GCM engine included.
- Gives you a typed encrypted table: `EncryptedTable<T>` stores each item as an encrypted object,
  keeps a searchable index, and batches writes behind a cache you flush in one call.
- Searches encrypted values without decrypting the table, through HMAC-SHA256 blind-index columns that
  SQLite itself indexes.
- Backs up a live database safely: maintenance mode, a WAL checkpoint, the SQLite online backup, and
  `VACUUM INTO` snapshots.
- Reads and writes SQLite's `user_version` so database schema upgrades can be versioned.
- Stays useful with no encryption at all. Add a crypt engine later and the encryption features light
  up without changing existing code.
- Carries its whole SQLite dependency graph for you: `Microsoft.Data.Sqlite` and the
  `SQLitePCLRaw.bundle_e_sqlite3` native bundle arrive with the package, and the consuming project
  needs no pin of its own.

## When to use it

Reach for CodeBrix.Sqlite when an application keeps a local SQLite database that needs some values
encrypted at rest, wants to search those values without decrypting the whole table, and has to be
backed up while it is running. It is equally at home as a plain convenience layer: construct
`SqliteDatabase` with no crypt engine and you get the pragma defaults, the mapper and the backup
orchestration on their own.

This is selective column and object encryption on top of a normal SQLite file, not page-level
encryption of the file. The database file, its schema, its table and column names, and every plaintext
column stay readable by any SQLite tool. Encrypt the values that need protecting, and treat the file
itself as unprotected.

What is deliberately out of scope:

- Range, ordering and `LIKE` queries over encrypted data in SQL. Encrypted columns are opaque to
  SQLite, so searching happens through the in-memory searchable index or through blind-index exact
  equality.
- Key management. There is no key storage, no rotation and no escrow beyond the PBKDF2 and HKDF
  derivation described below.
- Object-relational mapping. There is no change tracking outside the `EncryptedTable<T>` write-behind
  cache, no LINQ provider, no lazy loading, no relationships and no migrations engine - schema
  versioning is `user_version` read and write.
- Multi-mapping across several result types, and dynamic parameter objects.
- Database engines other than SQLite, or SQLite through a provider other than `Microsoft.Data.Sqlite`.
- Cross-process coordination. Maintenance mode gates operations issued through one `SqliteDatabase`
  instance; another process holding the same file is unaffected.
- Backup scheduling, retention and compression. `BackupToFile` and `SnapshotToFile` produce a file;
  what happens to it afterwards is yours - [CodeBrix.Compression](CodeBrix.Compression.md) is one way
  to zip it.

## Getting started

```bash
dotnet add package CodeBrix.Sqlite.ApacheLicenseForever
```

The package ID carries the license suffix; the namespaces do not. There is no package named plain
`CodeBrix.Sqlite`, and there is nothing to register - constructing `SqliteDatabase` is the only entry
point.

```csharp
using CodeBrix.Sqlite;                  // SqliteDatabase,
                                        //   SqliteDatabaseOptions,
                                        //   SqliteMapper (the
                                        //   extension methods),
                                        //   SqliteGridReader,
                                        //   EncryptedValue,
                                        //   EncryptedColumnAttribute
using CodeBrix.Sqlite.Cryptography;     // IObjectCryptEngine,
                                        //   AesGcmCryptEngine,
                                        //   IBlindIndexProvider,
                                        //   IObjectSerializer,
                                        //   JsonObjectSerializer
using CodeBrix.Sqlite.EncryptedTables;  // EncryptedTable<T>,
                                        //   EncryptedTableItem,
                                        //   TableItemStatus, the property
                                        //   attributes, TableSearch,
                                        //   TableSearchItem, TableColumn,
                                        //   TableIndex
using CodeBrix.Sqlite.Extensions;       // SqliteCommand /
                                        //   SqliteDataReader extensions
using CodeBrix.Sqlite.Enumerations;     // DbNullHandling
using CodeBrix.Sqlite.Exceptions;       // CodeBrixSqliteException family
using Microsoft.Data.Sqlite;            // SqliteConnection, SqliteCommand
```

The plain case uses none of the encryption surface: open a database, run DDL, insert with named
parameters, and read rows back into a class.

```csharp
using CodeBrix.Sqlite;

using var db = new SqliteDatabase("app.db");
db.SafeOpen(); // creates the file if missing; opens only if not already open
db.ExecuteNonQuery(
    "CREATE TABLE IF NOT EXISTS tickets (id INTEGER PRIMARY KEY, title TEXT, customer_tier TEXT);");

// The mapper methods are extension methods on SqliteConnection,
// so they are reached through the Connection property:
db.Connection.Execute(
    "INSERT INTO tickets (title, customer_tier) VALUES (@Title, @CustomerTier);",
    new { Title = "Investigate timeout", CustomerTier = "gold" });

// 'customer_tier' binds to 'CustomerTier' with no alias and no attribute:
List<Ticket> tickets = db.Connection
    .Query<Ticket>("SELECT id, title, customer_tier FROM tickets ORDER BY id")
    .ToList();

public class Ticket
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string CustomerTier { get; set; }
}
```

Notice that the mapper hangs off `SqliteDatabase.Connection`: the extension methods extend
`SqliteConnection`, and reaching them through the database is what makes them honor the
maintenance-mode gate and pick up the crypt engine ambiently.

<details>
<summary>The complete minimum project: csproj and Program.cs</summary>

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>disable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.Sqlite.ApacheLicenseForever" />
  </ItemGroup>
</Project>
```

```csharp
using System;
using CodeBrix.Sqlite;
using CodeBrix.Sqlite.Cryptography;
using CodeBrix.Sqlite.EncryptedTables;

public class Note : EncryptedTableItem
{
    [NotEncrypted] public string Folder { get; set; }
    [Searchable] public string Title { get; set; }
    public string Body { get; set; }        // encrypted, not searchable
}

public static class Program
{
    public static int Main()
    {
        using var crypt = new AesGcmCryptEngine("demo passphrase");
        using var db = new SqliteDatabase("vault.db", crypt);
        db.SafeOpen();

        using var notes = new EncryptedTable<Note>(db);
        notes.AddItem(new Note
        {
            Folder = "inbox",
            Title = "Reset the staging key",
            Body = "rotate on Friday"
        });
        notes.WriteItemChanges();

        var search = new TableSearch(
            new TableSearchItem(nameof(Note.Folder), "inbox"));

        foreach (Note n in notes.GetItems(search))
        {
            Console.WriteLine($"{n.Id}: {n.Title} - {n.Body}");
        }

        return 0;
    }
}
```

</details>

## Key concepts

### SqliteDatabase, the entry point

`SqliteDatabase` is `IDisposable` and takes the file path, an optional crypt engine and optional
options: `SqliteDatabase(string databaseFilePath, IObjectCryptEngine cryptEngine = null,
SqliteDatabaseOptions options = null)`. It exposes `DatabaseFilePath`, `Connection`, `CryptEngine`,
`Serializer`, `Options`, `State` and `IsInMaintenanceMode`.

`Open()` assumes a closed connection and throws if it is already open; `SafeOpen()` is the idempotent
form, and both apply the configured pragmas. `CreateCommand`, `ExecuteNonQuery` and `ExecuteScalar`
(with their async forms) each take a `bool forMaintenance = false` argument that decides whether the
call is allowed while the database is quiesced.

`SqliteDatabaseOptions` carries `UseWriteAheadLogging` (default true), `EnforceForeignKeys` (default
true), `CreateIfMissing` (default true - false makes opening a missing file fail) and a `Serializer`.
Crypt engines carry their own serializer for encrypt and decrypt work.

### The crypt engine seam

Three small interfaces are the whole extension surface, so a custom engine - a hardware-backed one,
say - drops straight in.

```csharp
public interface IObjectCryptEngine : IDisposable
{
    string EncryptObject(object valueToEncrypt);   // null in -> null out
    T DecryptObject<T>(string encryptedValue);
}

public interface IBlindIndexProvider
{
    string ComputeBlindIndex(string value);        // null in -> null out
}

public interface IObjectSerializer
{
    string Serialize(object value);
    T Deserialize<T>(string serialized);
}
```

`AesGcmCryptEngine` implements the first two. Its passphrase constructor -
`(string passphrase, byte[] salt = null, IObjectSerializer serializer = null)` - derives the AES key
with PBKDF2 (SHA-256, 100,000 iterations); a null salt uses a fixed library-default salt, so pass an
application-specific salt to isolate applications that share a passphrase. The other constructor takes
a raw 32-byte key, which it copies. Values are stored as AES-GCM with a random 12-byte nonce per
value: `Base64(nonce || tag || ciphertext)`. The blind index is HMAC-SHA256 under a secondary key
derived from the master key with HKDF, so it is deterministic without revealing the plaintext.

`JsonObjectSerializer` is the default serializer, with a parameterless constructor (`IncludeFields`
is true) and one that takes `System.Text.Json.JsonSerializerOptions`.

### Encrypting a column without a typed table

The extensions in `CodeBrix.Sqlite.Extensions` encrypt on the way in and decrypt on the way out of an
ordinary command. `SqliteCommandExtensions` adds `AddEncryptedParameter`, `ExecuteDecrypt<T>`,
`ExecuteDecryptAsync<T>` and `ExecuteReturnRowId` (an INSERT followed by `last_insert_rowid()`);
`SqliteDataReaderExtensions` adds `GetDecrypted<T>` and `TryDecrypt<T>`, each by ordinal and by column
name. `DbNullHandling` decides what a NULL column does: `ThrowDbNullException` (the default, raising
`DbNullValueException`) or `ReturnTypeDefaultValue`.

### Typed encrypted tables

An item type derives from `EncryptedTableItem` and its properties are tagged with attributes that
decide how each value is stored.

```csharp
public class Contact : EncryptedTableItem
{
    [NotEncrypted] public string Category { get; set; }
    [Searchable] public string FullName { get; set; }
    [Searchable, BlindIndexed] public string Email { get; set; }
    public string PrivateNotes { get; set; } // encrypted, not searchable
}
```

`[NotEncrypted]` makes a real plaintext column and combines with `[ColumnName("X")]`, `[NotNull]` and
`[ColumnDefaultValue("v")]`. `[Searchable]` encrypts the value but also places it in the in-memory
searchable index. `[BlindIndexed]` adds a deterministic HMAC column backed by a real SQLite index for
exact-equality search, and requires a crypt engine that implements `IBlindIndexProvider`. An untagged
property is encrypted and not searchable.

The generated table is `Id INTEGER PRIMARY KEY AUTOINCREMENT`, the plaintext columns, one
`BlindIndex_<Prop>` column per blind-indexed property, `Encrypted_Searchable`, and `Encrypted_Object`
holding the whole serialized and encrypted item. Plaintext properties participate in the searchable
index too, so a search can match on them with no `[Searchable]` marker.

### Reading, writing and the write-behind cache

`EncryptedTable<T>` is constructed against a `SqliteDatabase` (`checkDbTable` defaults to true and
creates the table and its blind indexes when they are missing; `tableName` defaults to the item type's
name). `AddItem`, `UpdateItem` and `RemoveItem` touch only the in-memory `TempItems` cache, and one
`WriteItemChanges()` persists them all; `Dispose()` flushes when `WriteChangesOnDispose` is true.
Each item's `SyncStatus` moves through `New`, `Unchanged`, `Modified` and `DeletePending`, and a new
item's `Id` is -1 until a write assigns the real row id.

Reads are `GetItem(long itemId)`, `GetItems(TableSearch search)` and
`FindByBlindIndex(string propertyName, string value)`, each with an async form. `GetItems` and
`FindByBlindIndex` write pending changes first by default so searches see them.

### Searching encrypted data

There are two search paths, and picking the right one is the main design decision on this library.

```csharp
public enum TableSearchType
{
    MatchAll = 0,   // logical AND across criteria (the default)
    MatchAny = 1    // logical OR across criteria
}

public enum SearchItemMatchType
{
    IsEqualTo = 0,
    IsNotEqualTo = 1,
    Contains = 2,
    DoesNotContain = 3,
    StartsWith = 4,
    EndsWith = 5
}
```

`GetItems` evaluates a `TableSearch` against the TTL-cached in-memory index, which is built by
decrypting only the `Encrypted_Searchable` column of every row, and then decrypts the full object of
matching items only. The index lifetime is `IndexLifetimeSeconds` (0 rebuilds on every use), and
`BuildFullTableIndex`, `DropFullTableIndex` and `CheckFullTableIndex` manage it explicitly. A
`TableSearch` is case-insensitive and trims values by default; `CaseSensitive` and `TrimValues` change
that.

`FindByBlindIndex` runs a SQL-indexed equality lookup on the HMAC column and never decrypts
non-matching rows. Matching is exact and case-sensitive whatever the `TableSearch` settings say,
because a blind index is a hash rather than a comparison. Prefer it for exact-equality lookups on
large tables.

### The mapper

Every CRUD extension method shares the same tail:
`(this SqliteConnection connection, string sql, object param = null, SqliteTransaction transaction = null, IObjectCryptEngine cryptEngine = null)`,
and every async form appends a `CancellationToken`. The synchronous set is `Query<T>`, `Query`,
`QueryFirst<T>`, `QueryFirstOrDefault<T>`, `QuerySingle<T>`, `QuerySingleOrDefault<T>`, `Execute`,
`ExecuteScalar<T>`, `ExecuteReader` and `QueryMultiple`, with `Async` peers of identical semantics.

Parameters may be anonymous objects, POCOs or `IDictionary<string, object>`; parameters the SQL does
not reference are skipped, and an `IEnumerable` value expands for an `IN` clause
(`"WHERE Id IN @ids"`), where an empty list matches no rows. A connection that was closed is opened
for the call and closed after it. Result columns bind to writable public properties
case-insensitively and with underscores ignored on both sides; a column with no matching property is
ignored, and a NULL column leaves the property at its type's default. Conversion runs through
`Convert.ChangeType` under `InvariantCulture`, with special handling for enums, `Guid`, `DateTime`,
`DateTimeOffset`, `TimeSpan` and `char`, and an INTEGER 0/1 column binds to a `bool`.

The mapper understands encryption. A result type deriving from `EncryptedTableItem` is materialized by
decrypting the row's `Encrypted_Object` column; a POCO property marked `[EncryptedColumn]` is
decrypted on read; and a parameter value wrapped in `new EncryptedValue(obj)` is encrypted on bind.
`QueryMultiple` returns a `SqliteGridReader` whose `Read<T>()` materializes the current result set and
advances to the next.

### Maintenance mode, backups and schema version

`BeginMaintenanceMode()` and `EndMaintenanceMode()` quiesce the database: while it is in maintenance
mode, normal operations throw `DatabaseMaintenanceException` and only `forMaintenance` operations run.
`GetSchemaVersion` and `SetSchemaVersion` read and write SQLite's `user_version`, and the setter runs
inside maintenance mode automatically; both preserve the open or closed state of the connection.

`BackupToFile(string destinationFilePath)` is the safe orchestration - quiesce, then
`PRAGMA wal_checkpoint(TRUNCATE)`, then the SQLite online backup, then resume - and it overwrites an
existing destination. `SnapshotToFile(string destinationFilePath)` is a one-statement consistent
snapshot through `VACUUM INTO`, and the destination must not exist or it throws `IOException`.

### Errors

Every library exception derives from `CodeBrixSqliteException`, and each type offers `(string message)`
and `(string message, Exception innerException)` constructors.

| Exception | Raised when |
| --- | --- |
| `DatabaseMaintenanceException` | a normal operation is attempted while the database is quiesced |
| `ObjectCryptographyException` | encryption, decryption or serialization fails |
| `EncryptedTableException` | table mapping, table name, search or missing-item problems |
| `DbNullValueException` | a NULL column is read under `ThrowDbNullException` |

Argument validation uses the standard system exception types (`ArgumentNullException`,
`ArgumentException`, `ObjectDisposedException`, `InvalidOperationException`, `IOException`).

## Examples

Encrypting a column value and then backing the live database up, with no typed table involved.

```csharp
using CodeBrix.Sqlite;
using CodeBrix.Sqlite.Cryptography;
using CodeBrix.Sqlite.Extensions;

using var cryptEngine = new AesGcmCryptEngine("my secret passphrase");
using var database = new SqliteDatabase("/data/mydatabase.sqlite", cryptEngine);
database.Open(); // WAL mode + foreign keys enabled by default

database.ExecuteNonQuery(
    "CREATE TABLE IF NOT EXISTS [Notes] (Id INTEGER PRIMARY KEY AUTOINCREMENT, Secret ENCRYPTED);");

using (var command = database.CreateCommand("INSERT INTO [Notes] (Secret) VALUES (@secret);"))
{
    command.AddEncryptedParameter("@secret", "This text is encrypted at rest.", cryptEngine);
    long rowId = command.ExecuteReturnRowId();
}

using (var command = database.CreateCommand("SELECT [Secret] FROM [Notes] LIMIT 1;"))
{
    string decrypted = command.ExecuteDecrypt<string>(cryptEngine);
}

// Safe backup: quiesce -> WAL checkpoint -> online backup -> resume
database.BackupToFile("/backups/mydatabase-backup.sqlite");
```

The full encrypted-table workflow: one engine, one database, write-behind adds, an indexed search, a
blind-index lookup, and both backup forms.

```csharp
using System;
using System.Collections.Generic;
using CodeBrix.Sqlite;
using CodeBrix.Sqlite.Cryptography;
using CodeBrix.Sqlite.EncryptedTables;

//The stored item type: two plaintext columns, one searchable encrypted
//  column, one searchable + blind-indexed column, one fully private column
public class ContactItem : EncryptedTableItem
{
    [NotEncrypted] public string Category { get; set; }

    [NotEncrypted, ColumnName("ContactAge")] public int Age { get; set; }

    [Searchable] public string FullName { get; set; }

    [Searchable, BlindIndexed] public string Email { get; set; }

    public string PrivateNotes { get; set; }
}

//1. the crypt engine (passphrase form; give it an app-specific salt)
using var crypt = new AesGcmCryptEngine(
    "correct horse battery staple",
    salt: System.Text.Encoding.UTF8.GetBytes("my-app-v1-salt"));

//2. the database, with the engine attached
using var db = new SqliteDatabase("contacts.db", crypt);
db.SafeOpen();

//3. the table - created (with its blind index) on first construction
using (var table = new EncryptedTable<ContactItem>(db))
{
    //4. write-behind adds, then one flush
    table.AddItem(new ContactItem
    {
        Category = "Pioneers", Age = 36, FullName = "Ada Lovelace",
        Email = "ada@example.com", PrivateNotes = "analytical engine"
    });
    table.AddItem(new ContactItem
    {
        Category = "Pioneers", Age = 85, FullName = "Grace Hopper",
        Email = "grace@example.com", PrivateNotes = "compilers"
    });
    table.AddItem(new ContactItem
    {
        Category = "Modern", Age = 50, FullName = "Anita Borg",
        Email = "anita@example.com", PrivateNotes = "systers"
    });
    int written = table.WriteItemChanges();
    Console.WriteLine($"{written} rows written");

    //5a. indexed search over searchable + plaintext properties
    var search = new TableSearch(
        new TableSearchItem(nameof(ContactItem.Category), "Pioneers"),
        new TableSearchItem(nameof(ContactItem.FullName), "Ada",
                            SearchItemMatchType.StartsWith));

    List<ContactItem> matches = table.GetItems(search);
    foreach (ContactItem c in matches)
    {
        Console.WriteLine($"{c.Id}: {c.FullName} <{c.Email}>");
    }

    //5b. exact equality through the blind index (no full-table decrypt)
    List<ContactItem> byEmail = table.FindByBlindIndex(
        nameof(ContactItem.Email), "grace@example.com");
    Console.WriteLine($"blind-index hits: {byEmail.Count}");

    //5c. fetch by id
    ContactItem first = table.GetItem(matches[0].Id);
    Console.WriteLine(first.PrivateNotes);   // decrypted on read
}   //Dispose flushes any pending changes (WriteChangesOnDispose)

//6. safe backup of the live database, and a VACUUM INTO snapshot
db.BackupToFile("contacts.backup.db");        // overwrites if present
db.SnapshotToFile("contacts.snapshot.db");    // must NOT already exist
```

Notice that `AddItem` returns immediately and nothing reaches the file until `WriteItemChanges()`, and
that the two searches decrypt very different amounts of data.

Ordinary CRUD through the mapper, including an expanded `IN` list and a two-result-set batch.

```csharp
using System.Collections.Generic;
using System.Linq;
using CodeBrix.Sqlite;

public class Ticket
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string CustomerTier { get; set; }   // binds to customer_tier
    public bool HasMitigation { get; set; }    // binds to INTEGER 0/1
}

using var db = new SqliteDatabase("app.db");
db.SafeOpen();

db.Connection.Execute(
    "INSERT INTO tickets (title, customer_tier, has_mitigation) " +
    "VALUES (@Title, @Tier, @Mitigated);",
    new { Title = "Timeout", Tier = "gold", Mitigated = true });

long newId = db.Connection.ExecuteScalar<long>(
    "SELECT last_insert_rowid();");

var ids = new[] { 1L, 2L, 3L };
List<Ticket> some = db.Connection
    .Query<Ticket>("SELECT * FROM tickets WHERE id IN @ids;",
                   new { ids })
    .ToList();

using (SqliteGridReader grid = db.Connection.QueryMultiple(
    "SELECT * FROM tickets; SELECT COUNT(*) FROM tickets;"))
{
    List<Ticket> all = grid.Read<Ticket>().ToList();
    long count = grid.Read<long>().First();
}
```

Versioning a schema change, with the DDL running inside maintenance mode.

```csharp
using CodeBrix.Sqlite;

using var db = new SqliteDatabase("app.db");
db.SafeOpen();

if (db.GetSchemaVersion() < 2)
{
    db.BeginMaintenanceMode();
    try
    {
        //only forMaintenance operations run while quiesced
        db.ExecuteNonQuery(
            "ALTER TABLE tickets ADD COLUMN closed_on TEXT;",
            forMaintenance: true);
    }
    finally
    {
        db.EndMaintenanceMode();
    }

    db.SetSchemaVersion(2);   // runs inside maintenance mode itself
}
```

## Pitfalls

> [!WARNING]
> Losing the passphrase or key loses the data. There is no recovery path, no key escrow and no
> key-rotation helper: re-encrypting means reading every item with the old engine and writing it with
> the new one. A custom salt is part of the key, so changing the salt makes existing rows
> undecryptable - choose one per application and keep it.

- Construct one `AesGcmCryptEngine` and reuse it. The engine derives its key once in the constructor,
  and PBKDF2 with 100,000 iterations is deliberately slow, so constructing one per operation is the
  single most expensive mistake you can make with this library.
- Keep one `SqliteDatabase` per database file for the lifetime of the work. Opening applies the
  pragmas each time, and the mapper opens and closes a closed connection around every call.
- Transactions need an already-open connection: call `SafeOpen()` before
  `Connection.BeginTransaction()`, and pass the transaction as the mapper's `transaction` argument for
  a bulk insert.
- Raw ADO.NET work issued straight against `Connection` bypasses the maintenance-mode gate. Use the
  `SqliteDatabase` methods, or the mapper, where an equivalent exists.
- `EncryptedTable<T>.GetItem()` returns the tracked in-memory instance when an item with that id is in
  `TempItems`, not a fresh copy from the table. Flush or clear the cache first to verify what is on
  disk.
- An empty `TableSearch` matches every item under `MatchAll` and no items under `MatchAny`.
- Blind-index matching is exact and case-sensitive whatever the `TableSearch` settings say. For
  case-insensitive equality, normalize the value the same way when storing and when searching.
- A searchable-index build decrypts the `Encrypted_Searchable` column of every row - an O(n) scan,
  cached with a TTL. That is fine at application-local sizes; use `[BlindIndexed]` for equality
  lookups on large tables, and keep `[Searchable]` to the properties actually searched.
- `Id`, `Encrypted_Searchable`, `Encrypted_Object` and the `BlindIndex_` prefix are reserved column
  names on an encrypted table; a colliding `[ColumnName]` throws `EncryptedTableException` when the
  table is constructed.
- A new `EncryptedTableItem` has `Id == -1` until `WriteItemChanges()` assigns the real row id. Do not
  persist the temporary negative id anywhere.
- `[NotNull]` columns are not satisfied by `[ColumnDefaultValue]` on insert, because the INSERT lists
  every column explicitly. Give NOT NULL properties real values.
- `BackupToFile()` overwrites an existing destination file; `SnapshotToFile()` refuses one and throws
  `IOException`.
- A `SqliteGridReader` is single-pass: `Read<T>()` advances to the next result set and throws
  `InvalidOperationException` once every set has been consumed. Read them in the order the SQL
  declares them.
- `Query<T>()` buffers its rows; for a very large result set prefer `ExecuteReader()` and read
  incrementally. Cache `TableColumns` and `FullTableIndex` if you use them in a loop, because both
  build a fresh object on every access.
- C# `required` members are fine on mapped types: the query methods materialize reflectively and carry
  no `new()` constraint, so rows round-trip into them correctly. Do not strip `required` defensively.

## Samples and tools in the repository

The repository ships no sample applications or tools. Its test project is the worked-example corpus,
and two of its files are worth reading before you write your own item type.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Test project | Every API, in runnable form; needs no network access, environment variables or setup | [`tests/CodeBrix.Sqlite.Tests`](https://github.com/ellisnet/CodeBrix.Sqlite/tree/main/tests/CodeBrix.Sqlite.Tests) |
| SampleItems.cs | Item types, a counting `IObjectSerializer` and a plaintext `IObjectCryptEngine` test double | [`tests/CodeBrix.Sqlite.Tests/SampleItems.cs`](https://github.com/ellisnet/CodeBrix.Sqlite/blob/main/tests/CodeBrix.Sqlite.Tests/SampleItems.cs) |
| TempFolder.cs | A disposable per-test temporary folder for database files | [`tests/CodeBrix.Sqlite.Tests/TempFolder.cs`](https://github.com/ellisnet/CodeBrix.Sqlite/blob/main/tests/CodeBrix.Sqlite.Tests/TempFolder.cs) |

The test project also references the `CodeBrix.Compression` package so it can exercise a backup, zip,
unzip, restore and read round trip. That reference is for the tests only and is not a dependency of
the shipped package.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Sqlite/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Sqlite/blob/main/AGENT-README.txt) |
| Non-package content in the repository | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Sqlite/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Sqlite/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.Sqlite.Tests](https://github.com/ellisnet/CodeBrix.Sqlite/tree/main/tests/CodeBrix.Sqlite.Tests) |

XML documentation ships alongside the assembly, so every type and member described here is available
to IntelliSense.

## License

CodeBrix.Sqlite is licensed under the Apache License 2.0, and the license is also named in the package
ID (`CodeBrix.Sqlite.ApacheLicenseForever`). For the provenance and licensing of open source code
included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Sqlite/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Compression](CodeBrix.Compression.md) - compress the file that `BackupToFile` or `SnapshotToFile` produced
- [CodeBrix.Cryptography](CodeBrix.Cryptography.md) - a general-purpose cryptography toolbox when the built-in AES-GCM engine is not enough
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.Sqlite on GitHub](https://github.com/ellisnet/CodeBrix.Sqlite) - source and tests
