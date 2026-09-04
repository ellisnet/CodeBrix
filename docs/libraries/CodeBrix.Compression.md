<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Compression</sub>

# CodeBrix.Compression

**CodeBrix.Compression creates, reads, updates and extracts Zip, GZip, Tar and BZip2 archives, and decompresses raw PKWARE DCL "imploded" and LZW ".Z" data.** It supports AES-128 and AES-256 encryption for Zip, Zip64 for large files, and streaming operations for both creation and extraction. It is fully managed with no dependencies beyond .NET, and it is used from any .NET 10 application or from a CodeBrix.Platform application.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Compression](https://github.com/ellisnet/CodeBrix.Compression) |
| **Packages** | [`CodeBrix.Compression.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Compression.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later; no other dependencies |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Any platform .NET 10 supports - fully managed, no native libraries, no OS restrictions |

## What it does

- Handles Zip archives end to end: create, read, extract, and update in place - adding, replacing and deleting entries in an existing archive without rewriting it by hand.
- Encrypts Zip entries with AES-256, AES-128, or the legacy ZipCrypto for compatibility with old readers.
- Writes and reads Zip64 extensions, for files larger than four gigabytes.
- Creates, reads and extracts GZip, Tar and BZip2.
- Decompresses raw PKWARE Data Compression Library "imploded" streams - the format used by many MS-DOS-era installers, and the compression used inside MPQ game archives.
- Decompresses the LZW/LZC ".Z" format written by the classic Unix `compress` utility, which nests with Tar to read `.tar.Z` archives.
- Works on non-seekable streams in both directions, and entirely in memory when you never touch the file system.
- Computes CRC-32, Adler32 and the BZip2 CRC through a small checksum API you can use on its own.
- Preserves Unicode file names, directory structure and, on extraction, timestamps.
- Offers a one-call convenience API for whole-directory work alongside the fine-grained stream API.
- Adds no NuGet dependencies and no native libraries; XML documentation (IntelliSense) ships alongside the assembly.

## When to use it

Use CodeBrix.Compression when you need archive formats that the .NET base class library does not cover, when a Zip has to be encrypted or updated in place, or when you have to read data written by an MS-DOS-era installer or by a Unix `compress` run. Brotli is the one common format it leaves alone: `System.IO.Compression.BrotliStream` in the base class library covers that.

Inside the CodeBrix family, [CodeBrix.Imaging](CodeBrix.Imaging.md) is the library for image format conversion, and [CodeBrix.PdfDocuments](CodeBrix.PdfDocuments.md) is the one for creating PDFs; neither job belongs here.

These are the things it deliberately does not do:

- No RAR or 7z archives, in either direction.
- No XZ, Zstandard, LZ4 or Snappy compression, and no Brotli - use `System.IO.Compression.BrotliStream` for the last of those.
- No extraction of Zip entries stored with the legacy Zip "Implode" (method 6) or "Shrink" (method 1) compression methods. The separate PKWARE DCL "implode" raw stream format *is* supported, and it is a different thing.
- No compression to DCL: the imploded format is decompression only.
- No compression to LZW ".Z": there is an equivalent of `uncompress` here, and none of `compress`.
- No in-place update of GZip, Tar or BZip2 archives - only Zip supports update.
- No file encryption outside a Zip archive; the AES support is Zip-specific.
- No disk images (ISO, VHD) and no self-extracting archives.

## Getting started

```bash
dotnet add package CodeBrix.Compression.MitLicenseForever
```

The package ID and the namespace are different, and there is no package named plain `CodeBrix.Compression`: the assembly and root namespace are `CodeBrix.Compression`, and the `.MitLicenseForever` suffix belongs to the package ID only. Import the namespace for the format you are working with.

```csharp
using CodeBrix.Compression.Zip;         // Zip archive operations
using CodeBrix.Compression.GZip;        // GZip compression/decompression
using CodeBrix.Compression.Tar;         // Tar archive operations
using CodeBrix.Compression.BZip2;       // BZip2 compression/decompression
using CodeBrix.Compression.Dcl;         // PKWARE DCL "implode" decompression
using CodeBrix.Compression.Checksum;    // Crc32, Adler32
using CodeBrix.Compression.Core;        // Name transforms, filters, scanning
using CodeBrix.Compression.Encryption;  // Zip AES transform/stream internals
using CodeBrix.Compression.Lzw;         // LZW (.Z) decompression stream
```

The root `CodeBrix.Compression` namespace holds `CompressionExceptionBase`, the base class most library exceptions derive from, and the static `CompressionOptions` class of global settings. `CodeBrix.Compression.Zip.Compression` and `CodeBrix.Compression.Zip.Compression.Streams` hold the raw Deflate and Inflate layer that the Zip and GZip streams are built on.

This is a complete Zip write: open the output, set a level, start an entry, write bytes, close the entry, finish the archive.

```csharp
using CodeBrix.Compression.Zip;

using var fileStream = File.Create("archive.zip");
using var zipStream = new ZipOutputStream(fileStream);

zipStream.SetLevel(9); // 0-9, 9 being the highest level of compression

var entry = new ZipEntry("document.txt")
{
    DateTime = DateTime.Now
};

zipStream.PutNextEntry(entry);

var buffer = File.ReadAllBytes("document.txt");
zipStream.Write(buffer, 0, buffer.Length);

zipStream.CloseEntry();
zipStream.Finish();
```

Notice the two calls that are most often forgotten: `CloseEntry()` after each entry, and `Finish()` before the stream closes. Nothing is registered at start-up, and there is no builder or factory to configure first.

## Key concepts

### Formats and what each one supports

| Format | Create | Read | Extract | Update | Encrypt |
| --- | --- | --- | --- | --- | --- |
| Zip | Yes | Yes | Yes | Yes | Yes (AES-128, AES-256, ZipCrypto) |
| GZip | Yes | Yes | Yes | No | No |
| Tar | Yes | Yes | Yes | No | No |
| BZip2 | Yes | Yes | Yes | No | No |
| DCL | No | Yes | Yes | No | No |
| LZW (.Z) | No | Yes | Yes | No | No |

"Update" for Zip means adding, replacing and deleting entries in an existing archive without rewriting it by hand.

### Stream ownership

By default, closing a compression stream closes the underlying stream. Set `IsStreamOwner = false` when you need to keep using the underlying stream afterwards.

```csharp
zipStream.IsStreamOwner = false;
```

This matters most with `MemoryStream` and with shared streams, and forgetting it is a common source of defects where the underlying stream is closed unexpectedly. The same property is on `ZipOutputStream`, `ZipInputStream`, `GZipOutputStream`, `GZipInputStream`, `BZip2OutputStream`, `BZip2InputStream`, `TarArchive`, `DclInputStream` and `LzwInputStream`, and the static `GZip`, `BZip2` and `Dcl` helpers take the equivalent `isStreamOwner` argument.

### Three ways into a Zip archive

`ZipOutputStream` and `ZipInputStream` are the stream-based path: sequential, no seeking required, and full control over each entry. `ZipOutputStream` carries `SetLevel(0-9)`, `Password`, `PutNextEntry(entry)`, `CloseEntry()` and `Finish()`; `ZipInputStream` carries `GetNextEntry()`, which returns null when the archive is exhausted, and `Password`.

`ZipFile` is the random-access path and requires a seekable stream. Construct it over a path or a stream, or create a new empty archive with `ZipFile.Create`.

```csharp
using CodeBrix.Compression.Zip;

using var zipFile = new ZipFile("archive.zip");

foreach (ZipEntry entry in zipFile)
{
    if (!entry.IsFile) continue;

    Console.WriteLine($"{entry.Name} - {entry.Size} bytes");

    using var stream = zipFile.GetInputStream(entry);
    using var reader = new StreamReader(stream);
    var content = reader.ReadToEnd();
    Console.WriteLine(content);
}
```

Beyond the enumerator, `ZipFile` exposes `Count`, an integer indexer, `Password`, `UseZip64` and `UpdateMode`, and it can verify an archive:

```csharp
bool ok = zipFile.TestArchive(testData: true);

// Or with a strategy and a per-entry result callback:
bool ok2 = zipFile.TestArchive(
    testData: true,
    strategy: TestStrategy.FindAllErrors,
    resultHandler: (status, message) => Console.WriteLine(message));
```

`TestStrategy` is `FindFirstError` or `FindAllErrors`, and the result handler is the `ZipTestResultHandler` delegate, `void (TestStatus status, string message)`.

`FastZip` is the one-call path for whole directories, and it is the right default when you do not need per-entry control.

### Compression levels and Zip entries

`SetLevel(0)` stores without compressing, `SetLevel(1)` is fastest, `SetLevel(5)` is balanced and `SetLevel(9)` is maximum and slowest. Level 0 is the sensible choice for content that is already compressed, such as JPEG images.

A `ZipEntry` carries the metadata for one item in the archive.

```csharp
var entry = new ZipEntry("filename.txt");

entry.DateTime          // File modification date/time
entry.Size              // Uncompressed size
entry.CompressedSize    // Compressed size
entry.AESKeySize        // AES key size (0, 128, or 256)
entry.IsFile            // true if entry is a file
entry.IsDirectory       // true if entry is a directory
entry.Name              // Entry name/path within archive
```

### Updating an existing Zip archive

`ZipFile` is the only type here that can update an archive in place. Every change is bracketed by `BeginUpdate()` and `CommitUpdate()`, or discarded with `AbortUpdate()`.

```csharp
using CodeBrix.Compression.Zip;

using var zipFile = new ZipFile("archive.zip");

zipFile.BeginUpdate();
zipFile.Add("newfile.txt");                 // add from the file system
zipFile.Add("localname.txt", "in/zip.txt"); // add under a different entry name
zipFile.AddDirectory("subfolder");          // add a directory entry
zipFile.Delete("obsolete.txt");             // delete by entry name
zipFile.CommitUpdate();
```

The same API builds an archive from nothing:

```csharp
using var zipFile = ZipFile.Create("new.zip");   // also: ZipFile.Create(Stream)
zipFile.BeginUpdate();
zipFile.Add("data.bin");
zipFile.CommitUpdate();
```

The full update surface:

```csharp
void BeginUpdate()
void BeginUpdate(IArchiveStorage archiveStorage)
void BeginUpdate(IArchiveStorage archiveStorage, IDynamicDataSource dataSource)
void CommitUpdate()
void AbortUpdate()
void Add(string fileName)
void Add(string fileName, string entryName)
void Add(string fileName, CompressionMethod compressionMethod)
void Add(string fileName, CompressionMethod compressionMethod, bool useUnicodeText)
void Add(ZipEntry entry)
void Add(IStaticDataSource dataSource, string entryName)
void Add(IStaticDataSource dataSource, string entryName, CompressionMethod compressionMethod)
void Add(IStaticDataSource dataSource, string entryName, CompressionMethod compressionMethod, bool useUnicodeText)
void Add(IStaticDataSource dataSource, ZipEntry entry)
void AddDirectory(string directoryName)
bool Delete(string fileName)      // returns false when no such entry
void Delete(ZipEntry entry)
```

To add content that is not a file on disk, implement `IStaticDataSource`, whose single member is `Stream GetSource()`, and hand it to one of the `Add` overloads. Have `GetSource()` open a new stream each time it is called, to avoid locking problems.

Do all adds and deletes between one `BeginUpdate()` and one `CommitUpdate()`. Committing after every entry rewrites the archive each time, and without a commit the batch is discarded.

### Update staging and Zip64

An update is staged through an `IArchiveStorage`. `DiskArchiveStorage(ZipFile file)` and `MemoryArchiveStorage()` both default to safe mode, and both take an explicit `FileUpdateMode` overload.

| Mode | Behavior |
| --- | --- |
| `FileUpdateMode.Safe` | All updates go through temporary files, so the original archive survives a failure. This is the default |
| `FileUpdateMode.Direct` | The archive is updated in place: faster, but a failure can leave it damaged |

The no-argument `BeginUpdate()` uses disk storage in safe mode for a file-backed `ZipFile`; pass a `MemoryArchiveStorage` explicitly when the archive lives in a `MemoryStream`. Keep the default safe mode unless you have measured a problem.

Zip64 behavior during an update is controlled by `ZipFile.UseZip64`, whose values are `UseZip64.Off`, `UseZip64.On` and `UseZip64.Dynamic`.

### FastZip

`FastZip` reduces a whole directory to one call in each direction: `CreateZip(zip, dir, recurse, filter)` and `ExtractZip(zip, dir, filter)`. The `fileFilter` and `directoryFilter` arguments are regular-expression filters, and the same filter syntax is available directly through the name and path filter types in `CodeBrix.Compression.Core`.

Its options are `CreateEmptyDirectories` (preserve empty directory structure), `RestoreDateTimeOnExtract` (preserve file timestamps), `Password` and `EntryEncryptionMethod`.

### Encryption

Set `Password` on the `ZipOutputStream`, `ZipInputStream` or `ZipFile` - or on `FastZip`, together with `EntryEncryptionMethod` - and the library selects the transform for you.

| `ZipEncryptionMethod` | Notes |
| --- | --- |
| `None` | No encryption; the default |
| `ZipCrypto` | Traditional PKZIP encryption: legacy and weak, marked `[Obsolete]`, so referencing it produces a compiler warning. Use it only for compatibility |
| `AES128` | AES 128-bit; the salt is handled internally |
| `AES256` | AES 256-bit; the most secure option, and the recommended one |

On the stream API the choice is per entry: set `entry.AESKeySize` to `256` or `128`. When `Password` is set and `AESKeySize` is not, ZipCrypto is used.

`CodeBrix.Compression.Encryption` exposes the machinery behind this - `ZipAESTransform`, which handles the AES transforms and whose `PwdVerifier` returns a two-byte verification array, and `ZipAESStream`. Most consuming code never touches either type. `ZipAESStream` supports `CryptoStreamMode.Read` only, and constructing it in write mode throws.

### GZip

Two static helpers cover the common cases, and two streams cover the rest.

```csharp
GZip.Compress(inStream, outStream, isStreamOwner, bufferSize = 512, level = 6)
GZip.Decompress(inStream, outStream, isStreamOwner)
GZipOutputStream(stream)          Compression stream
GZipInputStream(stream)           Decompression stream
```

The defaults are a buffer size of 512 and level 6; pass level 9 for maximum compression or 1 for the fastest. `GZip.Decompress` has no such parameters. Because `level` is the fifth parameter and `isStreamOwner` is the third, pass `level` by name.

### Tar

`TarArchive` is the high-level API: `TarArchive.CreateOutputTarArchive(stream)` and `TarArchive.CreateInputTarArchive(stream, nameEncoding)`, with `WriteEntry(entry, recurse)`, `ExtractContents(path)`, `RootPath` (the base path for entries), `IsStreamOwner` and `Close()`.

`TarOutputStream` and `TarInputStream` are the low-level API. `TarOutputStream(stream, nameEncoding)` and `TarOutputStream(stream, blockFactor, nameEncoding)` carry `PutNextEntry(entry)` and `CloseEntry()`; `TarInputStream(stream, nameEncoding)` carries `GetNextEntry()` and `GetNextEntryAsync(ct)`. The block factor controls the record size and accepts values from 1 to 64.

```csharp
using CodeBrix.Compression.Tar;

using var outStream = File.Create("archive.tar");
using var tarOut = new TarOutputStream(outStream, nameEncoding: null);

var entry = TarEntry.CreateTarEntry("myfile.txt");
entry.Size = fileData.Length; // MUST set size before writing
entry.ModTime = DateTime.Now;

tarOut.PutNextEntry(entry);
tarOut.Write(fileData, 0, fileData.Length);
tarOut.CloseEntry();
```

Entries come from `TarEntry.CreateTarEntry("name")` or `TarEntry.CreateEntryFromFile(path)`, and carry `Name`, `Size`, `ModTime`, `UserId`, `GroupId`, `UserName`, `GroupName`, `IsDirectory` and `File`, plus `Clone()` for a deep copy. `entry.TarHeader` exposes `Mode`, `LinkName`, `Magic`, `Version`, `DevMajor`, `DevMinor`, `Checksum` and `IsChecksumValid`.

Four rules are worth memorizing: `Size` must be set before any data is written, or the archive is corrupt; `ModTime` stores seconds precision only; long file names are handled automatically through extended headers; and setting `Name` to null, `Size` to a negative value, or `ModTime` to `DateTime.MinValue` throws. `GroupName` is the exception that tolerates null - it defaults to `"None"`.

Reading is available asynchronously through `await tarInputStream.GetNextEntryAsync(CancellationToken.None)` and `await tarInputStream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None)`.

For non-UTF-8 file names, pass an `Encoding` to the stream constructors and register the code-pages provider first:

```csharp
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
```

### BZip2

```csharp
BZip2.Compress(inStream, outStream, isStreamOwner, level)   // level required
BZip2.Decompress(inStream, outStream, isStreamOwner)
BZip2OutputStream(stream)
BZip2InputStream(stream)
```

The level runs from 1 to 9, with 9 the maximum. Unlike `GZip.Compress`, the level argument here is required.

### DCL decompression

DCL is the stream format produced by the `implode()` function of the PKWARE Data Compression Library. It was licensed to many MS-DOS-era products and is common in installer archives and self-extracting shareware distributions of that period; it is also the compression used inside MPQ game archives.

A DCL stream is a raw compressed stream with no container or archive structure around it, and it is *not* the Zip archive "Imploded" compression method (method 6) - PKZIP does not produce it. Its header is two bytes: byte 0 is 0 when literals are uncoded and 1 when they are Huffman coded; byte 1 is 4, 5 or 6, the base-2 logarithm of the dictionary size minus 6, giving 1024, 2048 or 4096 bytes.

```csharp
using CodeBrix.Compression.Dcl;

// Decompress a DCL "imploded" stream (e.g. data produced by the implode()
// function of the PKWARE Data Compression Library, common in MS-DOS-era
// installer archives). Note: this is NOT the Zip "Imploded" (method 6) format.
using var inStream = new DclInputStream(File.OpenRead("data.imploded"));
using var outStream = File.Create("data.bin");
inStream.CopyTo(outStream);
```

The static `Dcl.Decompress(inStream, outStream, isStreamOwner)` is the one-call form. `DclInputStream` is read-only and forward-only: `CanSeek` is false, and `Length`, `Seek`, `SetLength` and `Write` all throw `NotSupportedException`. Malformed or truncated input throws `DclException`. The format carries no checksum, so integrity verification means comparing the output against an externally known length or checksum.

When your own code lives under a namespace segment named `Dcl`, the bare name `Dcl` can resolve to the namespace rather than the class. Alias it:

```csharp
using DclHelper = CodeBrix.Compression.Dcl.Dcl;
```

### LZW (.Z) decompression

LZW here means the LZC variant of Lempel-Ziv-Welch written by the classic Unix `compress` utility, whose output normally carries the `.Z` extension. Like DCL it is a raw compressed stream and not an archive: one `.Z` stream holds a single file's worth of data and carries no file name, timestamp or directory structure. Archives of that era pair the two formats - `archive.tar.Z` is a Tar archive wrapped in a single `.Z` stream, so you wrap one stream in the other.

Decompression is supported and compression is not: there is no `LzwOutputStream` and no static `Lzw` helper class, and `LzwInputStream` is the entire public surface. The header is three bytes: bytes 0 and 1 are the magic marker `0x1f 0x9d`, and byte 2 is a flags byte whose low five bits hold `max_bits` (16 at most), whose high bit is the block-mode flag, and whose reserved bits must be clear. `LzwConstants` exposes those header constants - `MAGIC`, `MAX_BITS`, `BIT_MASK`, `BLOCK_MODE_MASK`, `HDR_SIZE`, `INIT_BITS` - if you want to sniff a stream before opening it.

A bad header or a corrupt code sequence throws `LzwException`, as does an empty or truncated stream. The format carries no checksum.

> [!WARNING]
> `LzwInputStream.Length` and `Position` do not mean what they appear to and, unlike the other read-only streams here, do not throw. `Length` returns the byte count from the most recent internal fill of the compressed input buffer - not the decompressed size, not the compressed size, and not a running total. `Position` returns the position of the underlying compressed stream, not the number of decompressed bytes produced. Never size a buffer or track progress with either one; read until `Read` returns 0, or use `CopyTo`.

### Checksums

`Crc32` and `Adler32` live in `CodeBrix.Compression.Checksum` and are usable on their own, outside any archive.

```csharp
using CodeBrix.Compression.Checksum;

var crc = new Crc32();
crc.Update(buffer);
long checksum = crc.Value;
```

### Errors

`CompressionExceptionBase`, in the root `CodeBrix.Compression` namespace, is the base class most library exceptions derive from; `DclException` and `LzwException` are two of them. `CompressionOptions`, also in the root namespace, is the static class of global settings.

## Examples

Create a password-protected archive holding several files, each entry encrypted with AES-256:

```csharp
using CodeBrix.Compression.Zip;

using var fileStream = File.Create("secure-archive.zip");
using var zipStream = new ZipOutputStream(fileStream);

zipStream.SetLevel(9);
zipStream.Password = "strong-password-123";

string[] files = { "report.pdf", "data.csv", "config.json" };

foreach (var filePath in files)
{
    var entry = new ZipEntry(Path.GetFileName(filePath))
    {
        AESKeySize = 256,
        DateTime = File.GetLastWriteTime(filePath)
    };

    zipStream.PutNextEntry(entry);

    using var inputStream = File.OpenRead(filePath);
    inputStream.CopyTo(zipStream);

    zipStream.CloseEntry();
}

zipStream.Finish();
```

Zip an entire folder and unzip it again with `FastZip`, encryption included:

```csharp
using CodeBrix.Compression.Zip;

var fastZip = new FastZip
{
    CreateEmptyDirectories = true,
    Password = "optional-password",
    EntryEncryptionMethod = ZipEncryptionMethod.AES256
};

// Create a zip from a directory
fastZip.CreateZip("backup.zip", @"C:\MyFolder", recurse: true, fileFilter: null);

// Extract a zip to a directory
fastZip.ExtractZip("backup.zip", @"C:\Extracted", fileFilter: null);
```

Build an archive in memory and read it back without touching the file system - note `IsStreamOwner = false`, which keeps the `MemoryStream` alive after the zip stream closes:

```csharp
using CodeBrix.Compression.Zip;

// Create in memory
var memStream = new MemoryStream();
using (var zipOut = new ZipOutputStream(memStream))
{
    zipOut.IsStreamOwner = false;
    zipOut.SetLevel(5);

    var entry = new ZipEntry("hello.txt") { DateTime = DateTime.Now };
    zipOut.PutNextEntry(entry);

    var bytes = Encoding.UTF8.GetBytes("Hello from memory!");
    zipOut.Write(bytes, 0, bytes.Length);
    zipOut.CloseEntry();
    zipOut.Finish();
}

// Read from memory
memStream.Position = 0;
using var zipFile = new ZipFile(memStream);

foreach (ZipEntry entry in zipFile)
{
    if (!entry.IsFile) continue;

    using var stream = zipFile.GetInputStream(entry);
    using var reader = new StreamReader(stream);
    Console.WriteLine(reader.ReadToEnd());
}
```

Nest the streams to write a `tar.gz` backup of a directory tree:

```csharp
using CodeBrix.Compression.GZip;
using CodeBrix.Compression.Tar;

using var fileStream = File.Create("backup.tar.gz");
using var gzipStream = new GZipOutputStream(fileStream);
using var tarArchive = TarArchive.CreateOutputTarArchive(gzipStream);

tarArchive.RootPath = "/path/to/backup/source";

var entry = TarEntry.CreateEntryFromFile("/path/to/backup/source");
tarArchive.WriteEntry(entry, recurse: true);
```

Nest them the other way to read a `.tar.Z` archive from the `compress` era:

```csharp
using CodeBrix.Compression.Lzw;
using CodeBrix.Compression.Tar;

using var lzwStream = new LzwInputStream(File.OpenRead("archive.tar.Z"));
using var tarArchive = TarArchive.CreateInputTarArchive(lzwStream, null);
tarArchive.ExtractContents("output-folder");
```

## Using it in a CodeBrix.Platform application

Add the package to your `.Core` library and use it from your services and view models. There is no UI surface, no native library and nothing to register at start-up, so the behavior is identical on every head.

One note for a cross-platform application: if you create or read Tar archives whose entry names are not UTF-8, call `Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)` once during start-up, before the first archive operation.

## Pitfalls

- The package ID is `CodeBrix.Compression.MitLicenseForever`; the namespaces are `CodeBrix.Compression.Zip`, `CodeBrix.Compression.GZip` and their siblings. There is no package named plain `CodeBrix.Compression`.
- All namespaces here are `CodeBrix.Compression.*`. Do not add a second compression library with clashing type names to the project.
- Call `CloseEntry()` after writing each Zip or Tar entry, and `Finish()` on a `ZipOutputStream` before it closes.
- Set `entry.Size` on a `TarEntry` before writing its data. Skipping it produces a corrupt archive.
- Set `IsStreamOwner = false` whenever the underlying stream has to outlive the compression stream.
- `ZipInputStream` cannot do everything: random access to entries needs `ZipFile` over a seekable stream.
- Call `Add`, `Delete` and `AddDirectory` only between `BeginUpdate()` and `CommitUpdate()`, and do not forget the commit - without it the changes are discarded.
- `ZipFile.Delete(string)` returns false when the entry is missing rather than throwing; the `ZipEntry` overload returns void.
- In `GZip.Compress`, the third parameter is `isStreamOwner` and `level` is the fifth, after `bufferSize`. Use named arguments.
- The `level` argument of `BZip2.Compress` is required, unlike the optional one on `GZip.Compress`.
- `ZipAESStream` supports read mode only; constructing it in write mode throws.
- Tar `ModTime` has seconds precision only - do not expect millisecond or tick-level accuracy.
- `ZipEncryptionMethod.ZipCrypto` is marked `[Obsolete]`, so referencing it produces a compiler warning. Prefer AES-256 for new archives.
- Neither DCL nor LZW carries a checksum, so verify the output against an externally known length or checksum when integrity matters.
- Under a namespace segment named `Dcl`, alias the static class: `using DclHelper = CodeBrix.Compression.Dcl.Dcl;`.
- Do not use `LzwInputStream.Length` or `Position` to size a buffer or measure progress.
- Do not copy test code from the repository verbatim: the test project has `InternalsVisibleTo` access to the library, so some of the helpers it uses are internal and are not available to package consumers.

> [!WARNING]
> Entry names come from the archive, so an extracted name is not safe to hand straight to `File.Create`. Join it onto your output directory and verify that the result stays inside that directory before writing anything.

## Samples and tools in the repository

This repository contains no sample applications, demo projects, tools or optional test-data downloads. It builds one library and one test project, and the test project's files double as the worked examples that the API guide links to. No environment variables, downloads or fixtures are required.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Zip suite | `FastZip`, streaming input and output, `ZipFile` random access and update, entries and factories, encryption, extra data, name transforms, corruption handling, async | [`tests/CodeBrix.Compression.Tests/Zip`](https://github.com/ellisnet/CodeBrix.Compression/tree/main/tests/CodeBrix.Compression.Tests/Zip) |
| Encryption suite | AES encryption internals: transforms, streams, salt and block validation | [`tests/CodeBrix.Compression.Tests/Encryption`](https://github.com/ellisnet/CodeBrix.Compression/tree/main/tests/CodeBrix.Compression.Tests/Encryption) |
| GZip suite | Compression and decompression, stream ownership, flushing, error handling, async | [`tests/CodeBrix.Compression.Tests/GZip`](https://github.com/ellisnet/CodeBrix.Compression/tree/main/tests/CodeBrix.Compression.Tests/GZip) |
| Tar suite | Create, read and extract, long names, encoding, entry properties, checksums, stream ownership, `tar.gz` integration | [`tests/CodeBrix.Compression.Tests/Tar`](https://github.com/ellisnet/CodeBrix.Compression/tree/main/tests/CodeBrix.Compression.Tests/Tar) |
| BZip2 suite | BZip2 compression and decompression | [`tests/CodeBrix.Compression.Tests/BZip2`](https://github.com/ellisnet/CodeBrix.Compression/tree/main/tests/CodeBrix.Compression.Tests/BZip2) |
| DCL suite | Decompressing imploded streams, including malformed input | [`tests/CodeBrix.Compression.Tests/Dcl`](https://github.com/ellisnet/CodeBrix.Compression/tree/main/tests/CodeBrix.Compression.Tests/Dcl) |
| LZW suite | Decompressing `.Z` streams, including bad headers and corrupt codes | [`tests/CodeBrix.Compression.Tests/Lzw`](https://github.com/ellisnet/CodeBrix.Compression/tree/main/tests/CodeBrix.Compression.Tests/Lzw) |
| Checksum suite | CRC-32 and Adler32 | [`tests/CodeBrix.Compression.Tests/Checksum`](https://github.com/ellisnet/CodeBrix.Compression/tree/main/tests/CodeBrix.Compression.Tests/Checksum) |
| Raw layer suite | The Deflate and Inflate layer the Zip and GZip streams are built on | [`tests/CodeBrix.Compression.Tests/Base`](https://github.com/ellisnet/CodeBrix.Compression/tree/main/tests/CodeBrix.Compression.Tests/Base) |
| Core suite | Name and path filters, file-system scanning, name transforms | [`tests/CodeBrix.Compression.Tests/Core`](https://github.com/ellisnet/CodeBrix.Compression/tree/main/tests/CodeBrix.Compression.Tests/Core) |
| Serialization suite | Exception serialization | [`tests/CodeBrix.Compression.Tests/Serialization`](https://github.com/ellisnet/CodeBrix.Compression/tree/main/tests/CodeBrix.Compression.Tests/Serialization) |

Run the suite with `dotnet test CodeBrix.Compression.slnx`. One optional external tool widens it: `tests/CodeBrix.Compression.Tests/TestSupport/SevenZip.cs` looks for a 7-Zip binary (`7z` or `7za`) on the PATH and in the default Windows install locations, and cross-verifies archives with it. It is not required - when no binary is found, that step is skipped with a message and the tests still pass.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Compression/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Compression/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Compression/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.Compression.Tests](https://github.com/ellisnet/CodeBrix.Compression/tree/main/tests/CodeBrix.Compression.Tests) |
| Library source | [src/CodeBrix.Compression](https://github.com/ellisnet/CodeBrix.Compression/tree/main/src/CodeBrix.Compression) |

## License

CodeBrix.Compression is licensed under the MIT License; the license is also named in the package ID (`CodeBrix.Compression.MitLicenseForever`). For the provenance and licensing of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Compression/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.PdfDocuments](CodeBrix.PdfDocuments.md) - the library for creating documents, rather than packing them
- [Project architecture](../platform/04-project-architecture.md) - where a library package belongs in a CodeBrix.Platform solution
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.Compression on GitHub](https://github.com/ellisnet/CodeBrix.Compression) - source, tests and samples
