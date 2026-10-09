<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Ollama.ModelManager</sub>

# CodeBrix.Ollama.ModelManager

**CodeBrix.Ollama.ModelManager downloads large language models and keeps them in a local model store,
then turns a model name into the files on disk that an in-process runner loads.** The store is laid out
exactly as Ollama's own, so a store this library fills and one a local Ollama install fills are the same
directory. It pulls from any registry that speaks Ollama's manifest-and-blob protocol, and also obtains
the files of a model no such registry serves - a Hugging Face file repository, a list of HTTPS
addresses, a folder on disk - into the same store. From models already in the store it can derive new
ones: an ONNX export, a smaller ONNX graph, a GGUF model converted from a checkpoint, and a quantized copy
of a GGUF model. It is pure managed code with no server and no Ollama installation. Use it from any .NET
10 application, or from a CodeBrix.Platform application that works with local models.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Ollama](https://github.com/ellisnet/CodeBrix.Ollama) |
| **Packages** | [`CodeBrix.Ollama.ModelManager.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Ollama.ModelManager.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later. One NuGet dependency arrives with the package, [`CodeBrix.Python.MitLicenseForever`](CodeBrix.Python.md), and it stays unloaded until a Python feature runs. Outbound HTTPS for pulls only; everything else works offline against the local store. A CPython shared library, with the export tooling installed in it, only for exporting a checkpoint to ONNX |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Windows, macOS and Linux - no native library ships in the package |

## What it does

- Keeps a local model store in Ollama's on-disk layout - `blobs/sha256-<hex>` and
  `manifests/<host>/<namespace>/<model>/<tag>` - defaulting to the directory a local Ollama install uses
  (`OLLAMA_MODELS`, else `~/.ollama/models`), so models pulled by either are visible to both.
- Pulls models from `registry.ollama.ai` by default, from Hugging Face through `hf.co/<user>/<repo>:<quant>`
  names, or from any registry that answers the same protocol, with parallel byte-range downloads,
  resumption after an interruption, sha256 verification of every layer and a progress stream.
- Pulls a *bundle* - the files of a model no such registry serves - from a Hugging Face file repository
  or from any list of HTTPS addresses, with the same resumable, hash-verified downloads and include and
  exclude filters over the publisher's own file paths; imports a folder already on disk as a bundle.
- Materializes a bundle as the file tree its publisher wrote, hard-linked by default so laying it out
  costs no disk space.
- Lists, shows, checks for, copies and deletes models, removing blobs no manifest references any more,
  and creates models from Modelfiles (`FROM`, `TEMPLATE`, `SYSTEM`, `PARAMETER`, `LICENSE`, `MESSAGE`,
  `ADAPTER`).
- Resolves a name to everything an in-process runner needs: the GGUF weights and shards, projector and
  adapter files, the template and system text, default parameters, licenses and preset messages - or,
  for a bundle, a name-and-path pair per file.
- Parses Modelfiles exactly as Ollama does, with typed parameters, and reads GGUF headers (versions 1 to
  3, both byte orders) without ever reading tensor data.
- Exports a bundle to ONNX and keeps the result as a *derived bundle* that records what made it and from
  what: graphs a publisher already ships are passed through with no conversion and no Python, and a
  checkpoint is converted by external export tooling running in a CPython the machine already has.
- Makes the ONNX graphs a bundle holds smaller - dynamic INT8, or block-wise four- or eight-bit weights -
  with the weight-only modes done by the library's own managed code and nothing installed.
- Converts a llama-family checkpoint (`safetensors` or PyTorch containers, one file or several shards)
  into an ordinary GGUF model in the same store, with nothing installed and no Python, holding one
  tensor's worth of buffers at a time rather than the whole file.
- Stores a quantized copy of a GGUF model under `<name>:gguf-<type>`, carrying the source's template,
  parameters, messages and license over and recording the provenance; the quantizer itself is a
  delegate the application supplies.
- Reports the license a publisher states - the identifier, where it was read from, and the `LICENSE`
  text a bundle ships - and applies no rule of its own.

## When to use it

Reach for CodeBrix.Ollama.ModelManager when an application needs a model's files on disk without
installing Ollama: fetching a model by name, keeping a cache of models, sharing a store with a local
Ollama install, or preparing a model in a form a runner can load.

What it does not do:

- It does not run a model, render a chat template, tokenize or embed. It hands you file paths;
  [CodeBrix.Ollama.ModelRunner](CodeBrix.Ollama.ModelRunner.md) is one in-process runner that takes them,
  and any other runner works the same way.
- It is not a client for a running Ollama and not a server. It never starts, looks for or calls an
  Ollama process; `OLLAMA_MODELS` is the one environment variable of Ollama's it reads.
- It does not push models, and it does not authenticate to a private Ollama registry beyond a bearer
  token you supply, offered only after a `401`.
- It does not ship, install or download CPython or any pip module. The one feature that needs Python
  finds an interpreter that is already there and says which module is missing when one is.
- It does not decide anything about a license. It reports what a source states; what may be used or
  shipped is yours to decide.
- It does not garbage-collect on its own, and it offers no synchronous API.

## Getting started

```bash
dotnet add package CodeBrix.Ollama.ModelManager.MitLicenseForever
```

The package ID carries the license suffix; the namespace does not. You install
`CodeBrix.Ollama.ModelManager.MitLicenseForever` and you write `using CodeBrix.Ollama.ModelManager;`.
The library declares exactly one namespace and every public type lives in it - the folders in the
repository are file organization, so `using CodeBrix.Ollama.ModelManager.Store;` is a compile error.

```csharp
using CodeBrix.Ollama.ModelManager;

using var store = new ModelStore();   // ~/.ollama/models, or OLLAMA_MODELS

await foreach (var progress in store.PullAsync("smollm:135m"))
{
    if (progress.Digest != null)
        Console.WriteLine($"{progress.Status} {progress.Percent:F0}%");
    else
        Console.WriteLine(progress.Status);
}

var resolved = await store.ResolveAsync("smollm:135m");
Console.WriteLine($"GGUF: {resolved.ModelPath}");
Console.WriteLine($"Template: {resolved.Template}");
```

`PullAsync` returns an `IAsyncEnumerable<PullProgress>` and is lazy: nothing is requested until you
iterate it. `ResolveAsync` reads only the local store, so pull first.

## Key concepts

### The store

`ModelStore` is the entry point and the one implementation of `IModelStore`; the interface is there so
tests can substitute their own. Create one, keep it, and dispose it at the end - disposing releases the
registry client and its pooled connections and leaves the store directory alone.

```csharp
using var store = new ModelStore(new ModelStoreOptions
{
    StoreDirectory = "/data/models",
    MaxConcurrentParts = 8,
});
```

Every `ModelStoreOptions` property defaults to Ollama's behaviour: `StoreDirectory` (null means
`OLLAMA_MODELS`, else `~/.ollama/models`), `DefaultRegistryHost` (`registry.ollama.ai`),
`DefaultNamespace` (`library`), `DefaultTag` (`latest`), `AllowInsecureHttp` (false), `BearerToken`,
`MaxConcurrentParts` (16 byte ranges of one blob in flight), `MinPartSize` and `MaxPartSize`,
`StallTimeout` (30 seconds), `MaxRetries` (6, with exponential back-off), `ProgressInterval`,
`HttpMessageHandler`, `UserAgent` and `Python` (where a CPython is, for the one feature that needs one).

Nothing reaches the network except through the `HttpMessageHandler`, so supplying one gives you a
complete in-memory registry - which is exactly how the library's own offline test suite runs. The store
does not dispose a handler you supplied.

The operations are all async and take a `CancellationToken` last, with a default:

```csharp
IAsyncEnumerable<PullProgress> PullAsync(string name, CancellationToken)
IAsyncEnumerable<PullProgress> PullAsync(string name, PullOptions options, CancellationToken)
Task ImportBundleAsync(string name, string directory, ImportOptions options = null, CancellationToken)
Task<IReadOnlyList<string>> MaterializeAsync(string name, string targetDirectory,
                                             MaterializeOptions options = null, CancellationToken)
Task<IReadOnlyList<ModelSummary>> ListAsync(CancellationToken)
Task<bool>          ExistsAsync(string name, CancellationToken)
Task<ModelInfo>     ShowAsync(string name, CancellationToken)
Task<ResolvedModel> ResolveAsync(string name, CancellationToken)
Task CopyAsync(string sourceName, string destinationName, CancellationToken)
Task DeleteAsync(string name, CancellationToken)
Task CreateAsync(string name, Modelfile modelfile, CreateOptions options = null, CancellationToken)
Task<IReadOnlyList<string>> PruneAsync(TimeSpan gracePeriod, CancellationToken)
Task<ExportResult>       ExportToOnnxAsync(string name, ExportOptions options = null, ...)
Task<ReduceResult>       ReduceOnnxAsync(string name, ReduceOptions options = null, ...)
Task<ConvertResult>      ConvertToGgufAsync(string name, ConvertOptions options = null, ...)
Task<QuantizeGgufResult> QuantizeGgufAsync(string name, QuantizeGgufOptions options, ...)
```

The four derived-model operations also take an optional `IProgress<PullProgress>` before the token.

### Model names

Every operation takes a name in Ollama's grammar, `[scheme://][host/][namespace/]model[:tag]`, with
absent parts filled from the store's options:

```text
smollm:135m                     library/smollm, tag 135m
smollm                          tag defaults to latest
myorg/mymodel:v3                namespace myorg
hf.co/user/repo:Q8_0            a Hugging Face repository
registry.local:5000/team/m:v1   a registry on a port
http://registry.local/t/m:v1    plain HTTP; refused unless AllowInsecureHttp is set
```

`ModelName` is the readonly struct behind this. `ModelName.Parse` never throws and can return nonsense,
so validate with `TryParse` or `IsValid`. A name with an `@digest` suffix is invalid - name a tag. The
store methods themselves throw `InvalidModelNameException` for a name that does not parse, before
anything else happens.

### Pulling from a registry

The progress stream of a registry pull arrives in a fixed order: `pulling manifest`, one stream of
`pulling <12 hex>` reports per layer, `verifying sha256 digest`, `writing manifest`, sometimes
`removing unused layers`, and always `success` last. A layer report carries `Digest`, `TotalBytes`,
`CompletedBytes` and `Percent`; a status-only report has `Digest` null.

A blob is downloaded to a sidecar, hashed and only then moved into place, and a manifest is written to a
temporary file and then moved, so a concurrent reader sees the old state or the new one and never a
half-written file. A second pull of a model already present is a cache hit.

### Bundles: models no registry serves

A bundle pull is always asked for. `PullAsync(name)` with no options means the registry protocol and
nothing else; the explicit overload takes `PullOptions`, built with `PullOptions.ForRegistry()`,
`PullOptions.ForHuggingFace(repository, revision, filter)` or `PullOptions.ForFileList(files, filter)`.
`PullSource` is `Registry`, `HuggingFaceFiles` or `FileList`.

Each file is stored as an ordinary blob and recorded as a layer carrying the publisher's relative path,
so listing, showing, resolving, copying, deleting and pruning work on a bundle unchanged. For a Hugging
Face repository the revision is pinned to the commit the branch resolved to, and the manifest records the
commit. `FileFilter` keeps or drops files by the publisher's paths - `FileFilter.ExcludeTrainingArtifacts`
is one ready-made filter - and the license and readme are kept whatever the filter says.

`ImportBundleAsync` takes a folder already on disk, which is the route for anything a publisher keeps
behind a sign-in. `MaterializeAsync` writes a bundle back out as the publisher's file tree and returns
the paths it wrote; `MaterializeOptions.Link` is `Hardlink` by default, and a hard link the file system
will not make quietly becomes a copy.

The sources can also be used on their own, to report the size of a pull before starting it:
`HuggingFaceHubSource` and `HttpFileListSource` implement `IBundleSource.ListAsync`, returning a
`BundleListing` with the files, the resolved revision, the license and `TotalBytes`.
`GoogleCloudStorageListing` turns a public bucket prefix into a list of addresses.

### Resolving: the seam to a runner

`ResolveAsync` returns a `ResolvedModel`: `ModelPath`, `ModelShardPaths`, `ProjectorPaths`,
`AdapterPaths` and `DraftPath` for a GGUF model, plus the `Template`, `System` and `Parameters` text the
Modelfile layers carry. These are ordinary files that any in-process GGUF runner loads. For a bundle,
`ModelPath` is null and `Files` lists a `ResolvedFile` per file - `Name` (the publisher's relative path),
`BlobPath`, `Size` and `Digest` - which is what a multi-file loader takes. `Format` says which kind of
model it is: `gguf`, or one of `huggingface`, `pytorch`, `onnx`, `tensorflow-checkpoint`, `mixed`,
`files`, `imported` or `derived`. Paths are computed from digests and no file is opened.

### Showing and listing

`ListAsync` reads only manifests and the small config blobs and returns `ModelSummary` values
(`DisplayName`, `Size`, `Digest`, `Config`). `ShowAsync` returns a `ModelInfo` that also opens the GGUF
header - `Metadata.Architecture`, `Metadata.ContextLength`, `Capabilities`, `Template`, `System`,
`Parameters`, `ModelfileText` - and, for a bundle, `Format`, `License`, and for a derived bundle `Tool`,
`ToolVersion` and `DerivedFrom`.

### Modelfiles and GGUF headers

```csharp
var modelfile = Modelfile.Parse("""
    FROM smollm:135m
    SYSTEM You are a terse assistant.
    PARAMETER temperature 0.2
    PARAMETER stop <|im_end|>
    """);

await store.CreateAsync("my-smollm", modelfile);
```

`FROM` names a model in the store or a GGUF file on disk; a relative path resolves against
`CreateOptions.BaseDirectory`. `Modelfile.Parse` accepts what Ollama's parser accepts, with the same
messages and line numbers, and `ModelfileParseException.LineNumber` says where.

`GgufMetadata.ReadAsync(path)` reads a GGUF header - every key-value and tensor descriptor - and never
tensor data, so describing a multi-gigabyte file costs a few tens of kilobytes. `GgufReadOptions` caps
how much of a long array (such as a token vocabulary) is retained.

### Derived models

Four operations make a new model from one already in the store and keep it beside the source, recording
what made it. Each result is a *snapshot*: deleting or re-pulling the source leaves it unchanged.

| Operation | What it does | What the machine needs |
| --- | --- | --- |
| `ExportToOnnxAsync` | Turns a bundle into ONNX. `ExportRoute.PublisherOnnx` passes a publisher's own `.onnx` files through without copying a byte; `GenAiBuilder` and `Optimum` run external export tooling; `Auto` decides | Nothing for the pass-through; a CPython with the export modules for the other routes |
| `ReduceOnnxAsync` | Makes a bundle's ONNX graphs smaller. `ReduceMode` is `DynamicInt8` (the default), `WeightOnlyInt8`, `WeightOnlyInt4` or `PreprocessOnly`; `ReduceEngine` chooses the managed engine or the Python one | Nothing for the weight-only modes, or for dynamic INT8 on a graph that has already been prepared |
| `ConvertToGgufAsync` | Converts a llama-family checkpoint into an ordinary GGUF model | Nothing at all - no Python and no native library |
| `QuantizeGgufAsync` | Stores a smaller copy of a single-file GGUF model under `<name>:gguf-<type>` | A quantizer delegate you supply |

`QuantizeGgufAsync` takes its quantizer as a `GgufQuantizer` delegate,
`Task GgufQuantizer(string inputPath, string outputPath, CancellationToken cancellationToken)`, because
this package depends on no inference engine. The store finds the file, names the result, carries the
source's layers over and records the provenance; your delegate writes the file.
[CodeBrix.Ollama.ModelRunner](CodeBrix.Ollama.ModelRunner.md)'s `QuantizeAsync` fits the shape, and so
does a command-line tool you start yourself.

### The Python corner

Exporting a checkpoint to ONNX (and preparing a graph for the Python quantizer) is the only work that
needs CPython. Everything else - pulling, importing, listing, showing, resolving, materializing,
Modelfiles, GGUF headers, the pass-through export, the managed reduction engine and the GGUF conversion -
never loads the CodeBrix.Python assembly, and the repository has tests that assert exactly that from a
separate process.

```csharp
PythonSupportReport report = PythonSupport.Check(null, "onnx");
```

`PythonSupport.Check` never throws: it reports the library it found, the version, the virtual
environment, which modules are installed and a list of `Problems`. A Python route calls
`PythonSupport.Require` before it lays anything out, so a missing module is reported in a second.
Point the store at an interpreter with `ModelStoreOptions.Python` - a `PythonOptions` with
`VirtualEnvironment` and `LibraryPath` - or with the `CODEBRIX_OLLAMA_PYTHON_VENV` and
`PYTHONNET_PYDLL` environment variables; code always wins over an environment variable. Call
`PythonSupport.Shutdown()` once, at the end of the application, after all Python work has finished.

### The error model

Everything the library raises derives from `ModelManagerException`:

| Exception | Means |
| --- | --- |
| `RegistryException` | an error status, a `401`, a transport failure that survived every retry, an insecure `http://` name, or an answer that is not a manifest; `StatusCode` and `ResponseBody` say more |
| `ModelNotFoundException` | no manifest, locally or on the registry |
| `DigestMismatchException` | the bytes do not match the stated hash; the bad file is already gone |
| `InvalidModelNameException` | a name that does not parse |
| `ModelfileParseException` | a Modelfile error, with `LineNumber` |
| `GgufFormatException` | not a GGUF file, or a header the reader cannot interpret |
| `CheckpointFormatException` | a checkpoint being converted is not what it claims to be |
| `PickleRefusedException` | a PyTorch checkpoint asks the restricted reader for something it will not do |
| `PythonNotAvailableException` / `PythonModuleNotInstalledException` / `PythonScriptException` | the Python feature could not run, and why |

A `RegistryException` whose `StatusCode` is `HttpStatusCode.Unauthorized` is the one to filter on when
you want to prompt for credentials.

### Thread safety

`ListAsync`, `ExistsAsync`, `ShowAsync` and `ResolveAsync` may be called concurrently on one store, and
different models may be pulled in parallel. Two pulls of the *same* model are not coordinated - not
between threads, processes, or this library and a real Ollama install - so serialize them yourself, and
the same goes for two exports, reductions or conversions of one model.

## Examples

A private store, a Hugging Face file repository, and the publisher's own tree laid out on disk.

```csharp
using CodeBrix.Ollama.ModelManager;

using var store = new ModelStore();

const string name = "hf.co/example-org/example-model";

var options = PullOptions.ForHuggingFace(null, null, FileFilter.ExcludeTrainingArtifacts);

await foreach (var progress in store.PullAsync(name, options))
    Console.WriteLine(progress.Status);

var info = await store.ShowAsync(name);
Console.WriteLine($"{info.Format}, licence {info.License.LicenseId ?? "not stated"}");

foreach (var path in await store.MaterializeAsync(name, "/work/example-model"))
    Console.WriteLine(path);
```

Converting a checkpoint to GGUF, with nothing installed, and resolving the result like any other model.

```csharp
using CodeBrix.Ollama.ModelManager;

using var store = new ModelStore();

const string name = "hf.co/example-org/example-model";
await foreach (var progress in store.PullAsync(
    name, PullOptions.ForHuggingFace(null, null, FileFilter.ExcludeTrainingArtifacts)))
{
    Console.WriteLine(progress.Status);
}

ConvertResult result = await store.ConvertToGgufAsync(name);

Console.WriteLine($"{result.Name}: {result.TensorCount} tensors, " +
                  $"{result.OutputBytes} bytes, {result.TypeWritten}");

var resolved = await store.ResolveAsync(result.Name);
Console.WriteLine(resolved.ModelPath);
```

Exporting a bundle to ONNX and making it smaller. The reduction to block-wise four-bit weights runs in
the library's own managed code.

```csharp
using CodeBrix.Ollama.ModelManager;

using var store = new ModelStore();

ExportResult exported = await store.ExportToOnnxAsync("hf.co/example-org/example-model");
Console.WriteLine($"{exported.Name} by {exported.Tool} ({exported.RouteUsed})");

ReduceResult reduced = await store.ReduceOnnxAsync(
    exported.Name, new ReduceOptions { Mode = ReduceMode.WeightOnlyInt4 });

Console.WriteLine($"{reduced.Name}: {reduced.SourceBytes} -> {reduced.ReducedBytes} bytes " +
                  $"({reduced.EngineUsed})");

var info = await store.ShowAsync(reduced.Name);
Console.WriteLine($"{info.Tool} {info.ToolVersion} made this from {info.DerivedFrom}");
```

Storing a quantized copy of a GGUF model, with the quantizer written in the application's own code - here
the related [CodeBrix.Ollama.ModelRunner](CodeBrix.Ollama.ModelRunner.md) package's `QuantizeAsync`.

```csharp
using CodeBrix.Ollama.ModelManager;
using CodeBrix.Ollama.ModelRunner;

using var store = new ModelStore();

QuantizeGgufResult result = await store.QuantizeGgufAsync(
    "hf.co/example-org/example-model:gguf",
    new QuantizeGgufOptions
    {
        Type = "q4_k_m",
        Tool = "CodeBrix.Ollama.ModelRunner",
        Quantizer = (input, output, ct) => ModelRunner.QuantizeAsync(
            input, output, GgufQuantizationType.Q4_K_M, null, ct),
    });

Console.WriteLine($"{result.Name}: {result.SourceBytes} -> {result.OutputBytes} bytes");
```

## Pitfalls

- `PullAsync` is lazy. Dropping its result, or never iterating it, downloads nothing.
- `ResolveAsync` does not download. It reads the local store and throws `ModelNotFoundException` when the
  model is absent; pull first, or guard with `ExistsAsync`.
- `PullAsync(name)` never fetches a bundle. A Hugging Face repository with no GGUF fails on the registry
  protocol exactly as it always has; ask for the bundle with `PullOptions.ForHuggingFace` or
  `ForFileList`.
- The library is compiled with nullable reference types off, so the compiler will not warn you:
  `Template`, `System`, `Parameters`, `Metadata`, `ModelPath` and `DraftPath` can all be null.
- `ModelName.Parse` is not validation. It never throws; use `TryParse` or `IsValid`.
- A `PARAMETER` in a derived Modelfile replaces the inherited value whole, so one `PARAMETER stop` line
  discards the base model's stop list.
- `ShowAsync` opens GGUF headers; calling it in a loop over a whole store is far slower than `ListAsync`.
- `ModelSummary.Digest` is the sha256 of the manifest file, hex with no `sha256:` prefix, not a blob
  digest.
- Two pulls of the same model at once share a partial file and nothing coordinates them.
- `MaterializeAsync` on a model pulled from a registry is an `InvalidOperationException`: it has no
  publisher file tree. Materializing also refuses to replace a file unless `MaterializeOptions.Overwrite`
  is set.
- A hard link cannot cross a volume, and materializing then copies, which costs the space.
- A reported license is not permission. A source that states nothing reports `LicenseRecord.None`.
- `PythonSupport.Shutdown()` cannot be undone: CPython cannot be restarted in a process. Call it once,
  at the end. It does nothing to an interpreter your own application started.
- `ExportOptions.AllowRemoteCode` runs the checkpoint's own Python files in your process; leave it false
  for a model you did not choose yourself.
- Exporting, reducing and converting write whole models into a temporary folder. On a Linux desktop whose
  `/tmp` is in memory, point `TMPDIR` at a real file system first.
- A reduced model is an approximation of the one it came from. Measure it on your own inputs before you
  ship it.
- `QuantizeGgufAsync` has no quantizer of its own; omitting `QuantizeGgufOptions.Quantizer` is an
  `ArgumentException`. The `Type` is a tag for the name and the provenance only - spell it the same as
  what your delegate actually writes.
- `ConvertToGgufAsync` converts the llama family only. Anything else is refused by name rather than
  approximated.

## Samples and tools in the repository

The test suite is the largest body of working usage. It runs offline: an in-memory registry double and a
temporary store directory stand in for the network and the disk.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Offline tests | Names, pulls, the registry client, blob downloads, resolving, listing and showing, Modelfiles, GGUF headers, bundles, sources, export, reduction, conversion, quantization and the Python reports | [`tests/CodeBrix.Ollama.ModelManager.Tests`](https://github.com/ellisnet/CodeBrix.Ollama/tree/main/tests/CodeBrix.Ollama.ModelManager.Tests) |
| Python tests | A second, gated test executable for exporting and reducing real published models in a real CPython | [`tests/CodeBrix.Ollama.ModelManager.Python.Tests`](https://github.com/ellisnet/CodeBrix.Ollama/tree/main/tests/CodeBrix.Ollama.ModelManager.Python.Tests) |
| Probe | A console application the tests run as a child process to observe what a whole process loads | [`tests/CodeBrix.Ollama.ModelManager.Probe`](https://github.com/ellisnet/CodeBrix.Ollama/tree/main/tests/CodeBrix.Ollama.ModelManager.Probe) |
| End-to-end tests | The gated project where a model this store made is run by the related ModelRunner package | [`tests/CodeBrix.Ollama.EndToEnd.Tests`](https://github.com/ellisnet/CodeBrix.Ollama/tree/main/tests/CodeBrix.Ollama.EndToEnd.Tests) |
| ModelQueryTool | A CodeBrix.Platform application, with its own libraries and tests, that stages a model into a store and chats with it in a terminal view; the repository describes it as a work in progress | [`samples/ModelQueryTool`](https://github.com/ellisnet/CodeBrix.Ollama/tree/main/samples/ModelQueryTool) |

Test files worth opening first, one per feature area:

| Feature | Test file |
| --- | --- |
| Model names | [`Names/ModelNameTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelManager.Tests/Names/ModelNameTests.cs) |
| Pulling from a registry | [`Store/ModelStorePullTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelManager.Tests/Store/ModelStorePullTests.cs) |
| Resolving | [`Store/ModelStoreResolveTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelManager.Tests/Store/ModelStoreResolveTests.cs) |
| Creating from a Modelfile | [`Store/ModelStoreCreateTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelManager.Tests/Store/ModelStoreCreateTests.cs) |
| Modelfile parsing | [`Modelfile/ModelfileTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelManager.Tests/Modelfile/ModelfileTests.cs) |
| GGUF headers | [`Gguf/GgufMetadataTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelManager.Tests/Gguf/GgufMetadataTests.cs) |
| Pulling a bundle | [`Store/ModelStoreBundlePullTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelManager.Tests/Store/ModelStoreBundlePullTests.cs) |
| Resolving and materializing a bundle | [`Store/ModelStoreBundleResolveTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelManager.Tests/Store/ModelStoreBundleResolveTests.cs) |
| Importing a folder | [`Store/ModelStoreBundleImportTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelManager.Tests/Store/ModelStoreBundleImportTests.cs) |
| Exporting to ONNX | [`Store/ModelStoreExportTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelManager.Tests/Store/ModelStoreExportTests.cs) |
| Reducing an ONNX model | [`Store/ModelStoreReduceTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelManager.Tests/Store/ModelStoreReduceTests.cs) |
| Converting to GGUF | [`Store/ModelStoreConvertTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelManager.Tests/Store/ModelStoreConvertTests.cs) |
| Quantizing a stored GGUF model | [`Store/ModelStoreQuantizeTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelManager.Tests/Store/ModelStoreQuantizeTests.cs) |
| Python reports | [`Python/PythonSupportTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelManager.Tests/Python/PythonSupportTests.cs) |
| An in-memory registry for your own tests | [`Infrastructure/FakeRegistryHandler.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelManager.Tests/Infrastructure/FakeRegistryHandler.cs) |

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [src/CodeBrix.Ollama.ModelManager/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/src/CodeBrix.Ollama.ModelManager/AGENT-README.txt) |
| Staging and quantizing the MuseCoco music models (ships inside the package too) | [MUSECOCO-README.txt](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/MUSECOCO-README.txt) |
| Non-package content in the repository | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.Ollama.ModelManager.Tests](https://github.com/ellisnet/CodeBrix.Ollama/tree/main/tests/CodeBrix.Ollama.ModelManager.Tests) |

The repository also produces [CodeBrix.Ollama.ModelRunner](CodeBrix.Ollama.ModelRunner.md), which has its
own guide. The two packages are released together; when an application uses both, install them at the
same version - a mixture builds and then fails at run time with a message saying so. XML documentation
ships alongside the assembly.

## License

CodeBrix.Ollama.ModelManager is licensed under the MIT License, and the license is also named in the
package ID (`CodeBrix.Ollama.ModelManager.MitLicenseForever`). The package carries the `LICENSE` file at
its root and requires license acceptance. For the provenance and licensing of open source code included
in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Ollama.ModelRunner](CodeBrix.Ollama.ModelRunner.md) - related: runs a GGUF model or an ONNX graph in-process from the paths this store resolves
- [CodeBrix.Python](CodeBrix.Python.md) - the embedded CPython behind the one feature that needs Python
- [CodeBrix.Audio.MusicGeneration](CodeBrix.Audio.MusicGeneration.md) - music generated from a model and played through an instrument library
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.Ollama on GitHub](https://github.com/ellisnet/CodeBrix.Ollama) - source and tests
