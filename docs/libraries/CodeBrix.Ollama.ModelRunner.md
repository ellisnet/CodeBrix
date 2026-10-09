<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Ollama.ModelRunner</sub>

# CodeBrix.Ollama.ModelRunner

**CodeBrix.Ollama.ModelRunner loads a model file and runs it inside your own process - a GGUF large
language model over a native inference engine that ships in the package, or an ONNX graph on a managed
interpreter that needs nothing installed at all.** On the GGUF road it exposes the model through one
interface: completion, streaming chat through the model's own chat template, reasoning kept apart from
the answer, tool calling, embeddings, grammar- and JSON-schema-constrained output, tokenization and
model metadata, and it can rewrite a model file at a smaller quantization with the same engine. On the
ONNX road it runs a graph tensors-in and tensors-out, generates MIDI music and streams the events as it
writes them, and generates text through the same interface as the GGUF road. There is no server, no
Ollama installation and no NuGet dependency. Use it from any .NET 10 application, or from a
CodeBrix.Platform application that talks to a local model.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Ollama](https://github.com/ellisnet/CodeBrix.Ollama) |
| **Packages** | [`CodeBrix.Ollama.ModelRunner.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Ollama.ModelRunner.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later, a model file on disk, and the memory to hold it. No NuGet dependencies: the dependency group is empty and the native engine is inside the package. No GPU - CPU inference is the tested path |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | GGUF models: Windows x64 and ARM64, macOS on Intel and Apple Silicon, Linux x64, ARM64 and RISC-V 64. ONNX models: anywhere .NET 10 runs, including platforms the package ships no native library for |

## What it does

- Loads a GGUF file into your process with `ModelRunner.LoadAsync` and hands back an `IRunningModel`, or
  describes a file without loading its weights with `ModelRunner.ProbeAsync`.
- Completes a raw prompt, streamed token by token (`GenerateAsync`) or awaited whole
  (`GenerateToEndAsync`), with stop sequences, a token limit and Ollama's own sampling defaults.
- Holds a chat through the model's own chat template (`ChatAsync` / `ChatToEndAsync`) in three dialects:
  the Jinja template embedded in the GGUF file, an Ollama Go template (a Modelfile `TEMPLATE`), or the
  engine's built-in templates - `Auto` picks one at load. `RenderChatPromptAsync` returns the rendered
  prompt without generating.
- Keeps a thinking model's reasoning apart from its answer, so it arrives as `ThinkingDelta` and never in
  the content, with a per-request `Think` switch.
- Offers tools and reads back `ToolCall`s with their arguments as JSON, in whichever convention the
  template uses.
- Constrains output during sampling - a GBNF grammar, a JSON schema converted into one, or plain JSON
  mode - so structured output parses the first time with no retry loop.
- Embeds text (`EmbedAsync`), tokenizes and detokenizes, and keeps a prefix cache so the next turn of a
  conversation re-evaluates only what changed.
- Applies LoRA adapters at load or swaps them at run time, reports load progress, cancels even a large
  model's prompt evaluation, and hands you the engine's own log through `ModelRunner.SetLogHandler`.
- Rewrites a GGUF file at a smaller quantization with `ModelRunner.QuantizeAsync` - `Q4_K_M`, `Q8_0`,
  `Q6_K` and the rest - with nothing installed and no partial file ever left behind.
- Exposes its template engines, parsers and schema converter as standalone utilities: `JinjaTemplate`,
  `OllamaTemplate`, `ThinkingParser`, `ToolCallParser`, `ToolCallFormat` and `JsonSchemaGrammar`.
- Runs an ONNX graph with nothing installed (`OnnxModel`): tensors in by name, tensors out by name, the
  graph's own metadata, and a decoder's key/value cache carried from step to step without a copy.
  Full-precision and quantized graphs alike; a four-bit graph keeps four-bit weights in memory.
- Generates MIDI music from an ONNX music model (`MidiGenerationModel`), streamed as it is written, so an
  application can start playing a piece whose end does not exist yet, and saves it as a Standard MIDI
  File; generates music from text or musical attributes with the MuseCoco models (`MuseCocoTextModel`,
  `MuseCocoMusicModel`).
- Generates text from an ONNX bundle (`OnnxCausalLmModel`) through the same `IRunningModel` contract,
  options and sampling as the GGUF road.

## When to use it

Reach for CodeBrix.Ollama.ModelRunner when an application should run a language model, an embedding
model or a music model itself, on the user's machine, with no service to install or start. It takes a
path; where the file comes from is up to you - any GGUF file on disk works.

What it does not do:

- It does not download models or keep a store of them. Any file works;
  [CodeBrix.Ollama.ModelManager](CodeBrix.Ollama.ModelManager.md) is a related package that resolves a
  model name to the paths this one takes.
- It is not a server and not a client: no listener, no port, no HTTP anywhere, and it never looks for
  Ollama.
- It does not run multimodal (vision or audio) models, decode speculatively with a draft model, save or
  restore a session, or serve several conversations from one loaded model at once - several instances
  run in parallel instead.
- It does not convert a training checkpoint into a model file, and it does not make an ONNX graph
  smaller; the ONNX road reads a graph and runs it.
- It does not train, and it does not run an ONNX graph on an accelerator - that road is CPU-only by
  design, because that is what runs everywhere with nothing installed.
- It offers no synchronous API.

## Getting started

```bash
dotnet add package CodeBrix.Ollama.ModelRunner.MitLicenseForever
```

The package ID carries the license suffix; the namespace does not. You install
`CodeBrix.Ollama.ModelRunner.MitLicenseForever` and you write `using CodeBrix.Ollama.ModelRunner;`. The
library declares one namespace, and every public type on both roads lives in it.

```csharp
using CodeBrix.Ollama.ModelRunner;

await using IRunningModel model = await ModelRunner.LoadAsync(new ModelRunnerOptions
{
    ModelPath = "/models/smollm-360m-instruct-q8_0.gguf",
    ContextSize = 4096,
});

var request = new ChatRequest();
request.Messages.Add(new ChatMessage(ChatRole.System, "Answer in one short sentence."));
request.Messages.Add(new ChatMessage(ChatRole.User, "What colour is the sky?"));

ChatResponse reply = await model.ChatToEndAsync(request);

Console.WriteLine(reply.Message.Content);
Console.WriteLine($"{reply.Statistics.TokensPerSecond:F1} tokens/s");
```

The chat template embedded in the model file rendered the conversation, because that is what `Auto`
resolves to for a file that carries one. The reply was generated on a thread the model owns, so awaiting
it never blocked yours. And `await using` unloaded the model at the end of the scope - an
`IRunningModel` holds the weights, a context, a thread and possibly a mapped file.

## Key concepts

### The two roads

Which road you take is decided by the file you have. `ModelRunner.LoadAsync` reads a GGUF file and runs
it on the native engine; `OnnxModel`, `MidiGenerationModel` and `OnnxCausalLmModel` read ONNX graphs and
run them on managed code. They share the package and nothing else: neither reads the other's format, and
each says so when handed the wrong one. Nothing of the native engine is loaded to run an ONNX graph.

### The ModelRunner static class

Five members are the only way into the native engine:

```csharp
static Task<IRunningModel>  LoadAsync(ModelRunnerOptions options, CancellationToken)
static Task<ModelDetails>   ProbeAsync(string modelPath, CancellationToken)
static Task<QuantizeResult> QuantizeAsync(string inputPath, string outputPath,
                                          GgufQuantizationType type, QuantizeOptions options,
                                          CancellationToken)
static NativeRuntimeInfo    GetNativeRuntimeInfo()
static void                 SetLogHandler(Action<ModelRunnerLogLevel, string> handler)
```

`ProbeAsync` reads architecture, parameter count, the memory the weights would need, the training
context and the embedded chat template without loading the weights - seconds on a twenty-gigabyte file -
which makes it the right call before deciding what to pass to `LoadAsync`. `GetNativeRuntimeInfo`
reports the loaded library's path, runtime identifier, build and CPU features in use.

### Loading a model

Only `ModelPath` is required on `ModelRunnerOptions`. A null on a nullable numeric property means "leave
it to the engine", which usually means "read it from the model file" and is not the same as zero. The
settings that matter most:

- `ContextSize` is the memory dial. Null asks for the model's *trained* context, which on a modern model
  can be hundreds of thousands of tokens; for anything above a few billion parameters, set it.
  `KeyCacheType` and `ValueCacheType` are the second dial - `Q8_0` halves the cache against the default.
- `LoadMode` defaults to `MemoryMap`, which pages the weights in on demand and is the only mode that lets
  a model close to the size of physical memory load at all. The others are `Read`, `LockInMemory`,
  `MemoryMapAndLock` and `DirectIo`.
- `Threads`, `BatchThreads` and `MaxThreads` default to one thread per physical core; set `MaxThreads`
  to put a ceiling on the library's own choice on machines you have never seen.
- `UseExtraBufferTypes` (true) lets the CPU backend repack weights into faster layouts at the cost of a
  second copy of much of the model in memory; turn it off on a machine without the headroom.
- `ChatTemplateDialect`, `OllamaTemplate` and `JinjaTemplate` choose or replace the chat template;
  `LoraAdapters`, `EmbeddingsMode`, `EmbeddingPooling` and `LoadProgress` cover the rest.

Options out of range are an `ArgumentException` before anything native is touched.

### The IRunningModel contract

```csharp
ModelDetails        Details { get; }
ModelRunnerOptions  Options { get; }
ChatTemplateDialect ChatTemplateDialect { get; }

Task<IReadOnlyList<int>> TokenizeAsync(string text, bool addSpecialTokens = true,
                                       bool parseSpecialTokens = true, CancellationToken)
Task<string>             DetokenizeAsync(IReadOnlyList<int> tokens, bool renderSpecialTokens = false,
                                         CancellationToken)

IAsyncEnumerable<GenerationUpdate> GenerateAsync(string prompt, GenerationOptions options = null, CancellationToken)
Task<GenerationResult>             GenerateToEndAsync(string prompt, GenerationOptions options = null, CancellationToken)

IAsyncEnumerable<ChatUpdate> ChatAsync(ChatRequest request, CancellationToken)
Task<ChatResponse>           ChatToEndAsync(ChatRequest request, CancellationToken)
Task<string>                 RenderChatPromptAsync(ChatRequest request, CancellationToken)

Task<EmbeddingResult> EmbedAsync(IReadOnlyList<string> inputs, CancellationToken)
Task SetLoraAdaptersAsync(IReadOnlyList<LoraAdapterOptions> adapters, CancellationToken)
Task ClearCacheAsync(CancellationToken)
```

A streaming call yields an update for every piece of text that becomes unambiguous and always one final
update whose `IsFinal` is true, carrying the `FinishReason` (`Stop`, `StopSequence`, `Length`,
`ContextFull` or `ToolCall`) and the `Statistics`. Application code and tests can implement the interface
themselves to stand in for a real model; the library's own suite does exactly that.

### Generation and sampling

`GenerationOptions` controls one request: `MaxTokens`, `StopSequences`, `Sampling`, `Grammar`,
`JsonSchema` and `JsonMode`. `SamplingOptions` defaults are Ollama's - `Temperature` 0.8, `TopK` 40,
`TopP` 0.9, `RepeatPenalty` 1.1 and the rest - so a model behaves here the way it behaves under Ollama.
`Seed` is null for a fresh seed per request, or a fixed value for a fixed random stream; a `Temperature`
of 0 or less is greedy.

### Chat templates, thinking and tools

A chat template turns messages into the one string a model was trained to answer, and getting it wrong is
the most common reason a local model produces nonsense. `ChatTemplateDialect` is `Jinja` (the template
the model file embeds), `Ollama` (a Modelfile `TEMPLATE`), `Native` (the engine's own matching of
well-known families, a last resort) or `Auto`, which chooses once at load.

Set `ChatRequest.Think` to ask a reasoning model to think, and read `ThinkingDelta` and `ContentDelta`
separately. Offer tools by adding `ToolDefinition`s to `ChatRequest.Tools`; the calls arrive complete on
the final update, as `ToolCall` values with `Name` and `ArgumentsJson`, and the finish reason is then
`ToolCall`.

### Structured output

Four constraints win in this order when more than one is set: `GenerationOptions.Grammar` (GBNF text),
`GenerationOptions.JsonSchema` (converted into a grammar), `GenerationOptions.JsonMode`, and
`ChatRequest.ResponseFormat` (`ResponseFormat.Json` or `ResponseFormat.FromJsonSchema(...)`). The
constraint is enforced by the sampler token by token, so the model cannot produce anything the grammar
rejects. The schema converter handles local `$ref`, `oneOf`, `anyOf`, `allOf`, `enum`, `const`, string,
number, array and object constraints; a remote `$ref` is refused rather than fetched.

### Quantizing a GGUF model

```csharp
QuantizeResult result = await ModelRunner.QuantizeAsync(
    "/models/mymodel-f16.gguf",
    "/models/mymodel-q4_k_m.gguf",
    GgufQuantizationType.Q4_K_M);

Console.WriteLine($"{result.InputBytes:N0} -> {result.OutputBytes:N0}"
                  + $" in {result.Elapsed.TotalSeconds:F1} s");
```

The quantizer is the engine already inside the package, so nothing is downloaded or installed, and with
no options the output is byte for byte what the engine's own command-line quantizer writes. The input is
never modified, and the output must be a different path.

### The ONNX road

`OnnxModel` is the raw surface, with three named load methods rather than overloads:
`LoadAsync(modelPath)` for one `.onnx` file, `LoadFromDirectoryAsync(bundleDirectory, modelFileName)`
for a graph inside a folder, and `LoadFromFilesAsync(files, modelFileName)` for logical-name-to-path
pairs, which is what a content-addressed store hands you. `IOnnxModel.Run` takes and returns
`OnnxTensor`s by name - build one with `FromFloats`, `FromInt32`, `FromInt64` or `FromBooleans` - and
`RunAsync` is the same work off the calling thread. Arrays are never copied in either direction, so
feeding a decoder's `present` outputs back as the next step's `past` inputs costs nothing.

`MidiGenerationModel.LoadFromDirectoryAsync` or `LoadFromFilesAsync` loads a two-graph MIDI model and
yields `MidiEvent`s as it makes them. Every event carries its absolute `Tick`; a note carries its own
`DurationTicks`; and `HorizonTicks` says how far the piece is settled, which is what a streaming player
needs to start before the end exists. `SaveAsync` writes the events as a Standard MIDI File.

`OnnxCausalLmModel.LoadFromDirectoryAsync` loads a text decoder and returns an `IOnnxCausalLmModel`,
which is an `IRunningModel`. What an ONNX bundle cannot honour - chat templates, embeddings, adapters and
grammar-constrained output - is refused with `NotSupportedException` rather than answered wrongly.

`OnnxRunnerOptions` carries `Threads`, `MaxThreads`, `KernelPath` and `ReuseBuffers`. The road is
CPU-only, and a GGUF file of the same model through the native engine can be expected to be faster.
While a large graph is being read, peak memory can reach about twice what the loaded graph then settles
at, so size the machine for the load rather than the steady state.

### The native library

One library per runtime identifier ships under `runtimes/<rid>/native/` - `win-x64`, `win-arm64`,
`osx-x64`, `osx-arm64`, `linux-x64`, `linux-arm64` and `linux-riscv64` - and NuGet gives your application
the one it needs. Nothing is compiled or downloaded on the machine. The backend is the CPU everywhere,
plus Metal on `osx-arm64`; there is no CUDA, ROCm or Vulkan build, so `GpuLayers` has nothing to offload
to elsewhere. When the library cannot be found, the `NativeLibraryException` lists every path that was
tried - the answer to nearly every packaging question.

### The error model

Everything the library raises derives from `ModelRunnerException`:

| Exception | Means |
| --- | --- |
| `NativeLibraryException` | the native engine could not be loaded, or the one found is the wrong build or runtime identifier |
| `ModelLoadException` | no file at the path, a file the engine will not read, no context could be made, an adapter that will not apply, a quantization refused; on the ONNX road, a graph or bundle the managed engine refuses to load |
| `InferenceException` | a decode failed or ran out of context, a prompt longer than the context, an embedding input too long; on the ONNX road, a run that could not proceed or a second run while one is in flight |
| `ChatTemplateException` | a chat template would not parse or render this request |
| `GrammarException` | a grammar or schema that cannot be used |

`NotSupportedException` - what an ONNX bundle cannot honour - is a framework exception, and catching
`ModelRunnerException` does not catch it. Most messages end with the engine's own last log lines, under
"The engine's log for this operation:"; read them, they usually name the problem exactly.

## Examples

Streaming a reply with the reasoning kept apart.

```csharp
using CodeBrix.Ollama.ModelRunner;

var request = new ChatRequest { Think = true };
request.Messages.Add(new ChatMessage(ChatRole.User, "Why is the sky blue?"));

await foreach (ChatUpdate update in model.ChatAsync(request))
{
    if (update.ThinkingDelta != null) Console.Write(update.ThinkingDelta);
    if (update.ContentDelta != null) Console.Write(update.ContentDelta);

    if (update.IsFinal)
        Console.WriteLine($"[{update.FinishReason}] {update.Statistics.GeneratedTokens} tokens");
}
```

Asking for a tool.

```csharp
using CodeBrix.Ollama.ModelRunner;

var request = new ChatRequest();
request.Tools.Add(new ToolDefinition
{
    Name = "get_weather",
    Description = "Look up the current weather in a city.",
    ParametersJsonSchema =
        @"{""type"":""object"",""properties"":{""city"":{""type"":""string""}},""required"":[""city""]}",
});
request.Messages.Add(new ChatMessage(ChatRole.User, "What is the weather in Paris?"));

ChatResponse reply = await model.ChatToEndAsync(request);

foreach (ToolCall call in reply.Message.ToolCalls)
    Console.WriteLine($"{call.Name}({call.ArgumentsJson})");   // get_weather({"city":"Paris"})
```

Output constrained to a JSON schema, valid the first time.

```csharp
using CodeBrix.Ollama.ModelRunner;

var options = new GenerationOptions
{
    MaxTokens = 200,
    JsonSchema =
        @"{""type"":""object"",""properties"":{""city"":{""type"":""string""},
           ""country"":{""type"":""string""}},""required"":[""city"",""country""]}",
};

GenerationResult result = await model.GenerateToEndAsync(
    "Give me the capital of France as JSON.", options);

Console.WriteLine(result.Text);
```

Embeddings, one vector per input.

```csharp
using CodeBrix.Ollama.ModelRunner;

EmbeddingResult result = await model.EmbedAsync(new[]
{
    "a cat sat on the mat",
    "a feline rested on the rug",
});

Console.WriteLine($"{result.Embeddings.Count} vectors of {result.Dimensions} dimensions, "
                  + $"{result.PromptTokens} tokens in total");
```

Running an ONNX graph, and generating text from an ONNX bundle, with nothing installed.

```csharp
using CodeBrix.Ollama.ModelRunner;

await using IOnnxModel graph = await OnnxModel.LoadAsync("/models/example/model.onnx");

foreach (OnnxValueMetadata input in graph.Metadata.Inputs)
    Console.WriteLine($"{input.Name} {input.ElementType}");

var outputs = graph.Run(new Dictionary<string, OnnxTensor>
{
    ["input_ids"] = OnnxTensor.FromInt64(new long[] { 1, 2, 3 }, 1, 3),
});

Console.WriteLine(outputs["logits"].Floats.Length);

await using IOnnxCausalLmModel model =
    await OnnxCausalLmModel.LoadFromDirectoryAsync("/models/example-onnx");

GenerationResult result = await model.GenerateToEndAsync(
    "the prompt", new GenerationOptions { MaxTokens = 64 });

Console.WriteLine(result.Text);
```

Generating MIDI music and saving it. The player is the application's own; events arrive as they are made,
so buffer enough of the piece before starting playback.

```csharp
using CodeBrix.Ollama.ModelRunner;

await using IMidiGenerationModel model =
    await MidiGenerationModel.LoadFromDirectoryAsync("/models/example-midi-onnx");

var options = new MidiGenerationOptions { MaximumEvents = 512, Seed = 42 };
var events = new List<MidiEvent>();

await foreach (MidiEvent e in model.GenerateAsync(options))
{
    events.Add(e);
    // hand e to your own player; e.HorizonTicks is how far the piece is settled
}

await model.SaveAsync("/tmp/piece.mid", events);
```

## Pitfalls

- Leaving `ContextSize` null for a large model. The cache for the trained context can be larger than the
  weights, and the load fails with a memory error that reads like a bug and is a setting.
- Dropping the result of `GenerateAsync` or `ChatAsync`. Nothing happens until you iterate, and the
  request holds the model's turn until the enumeration finishes or is disposed.
- Calling another member of the same model from inside an `await foreach` over it throws
  `InvalidOperationException`. Collect what you need, let the enumeration end, then call.
- The library is compiled with nullable reference types off, so the compiler will not warn you:
  `ModelDetails.ChatTemplate`, `ModelDetails.Name`, `ChatMessage.Thinking`, `ToolCall.Id` and the
  `Statistics` of a non-final update can all be null.
- Applying a chat template by hand and then calling `ChatAsync` renders it twice. Hand-built prompt text
  belongs in `GenerateAsync`, which applies no template.
- Tool calls arrive only on the final update. The `Native` dialect cannot express tools and refuses them
  with `ChatTemplateException`; and a grammar or JSON format constrains the whole output, so do not
  combine one with a tool-calling turn.
- `ClearCacheAsync` does not save memory. The cache is allocated at load; clearing it changes
  correctness and speed, not footprint.
- An embedding is computed in one physical batch, so the per-input limit is the smaller of the embedding
  context and `PhysicalBatchSize`. Raise it or split the text.
- A `SetLogHandler` handler is called on engine threads, line by line, during a load. Queue the line and
  return.
- A `CancellationToken` cannot stop a quantization once the native quantizer has started; it is honoured
  only before.
- Quantizing an already-quantized file needs `AllowRequantize` and gives measurably worse results than
  quantizing the F16 or BF16 file once.
- Setting `Threads` to `Environment.ProcessorCount` costs real speed, and `MaxThreads` does nothing once
  `Threads` is set.
- A trimmed or single-file publish, or a hand copy of the managed assembly alone, can drop the native
  library. The exception lists every path it tried.
- Installing this package and the related ModelManager package at different versions builds without
  complaint and then fails at run time with a message saying so. They are released together; install
  them at the same version.

## Samples and tools in the repository

The default test suite is offline and downloads nothing: the template engines, parsers, grammar
converter and option mapping are tested over fixtures, and the decode path runs against a synthetic
conformance model generated in the repository. Only the live tests touch the network, and they are gated
behind environment variables.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Tests | Both roads: the engine, chat, templates, parsers, grammars, quantizing, the native library, ONNX graphs, MIDI and text drivers | [`tests/CodeBrix.Ollama.ModelRunner.Tests`](https://github.com/ellisnet/CodeBrix.Ollama/tree/main/tests/CodeBrix.Ollama.ModelRunner.Tests) |
| End-to-end tests | The gated project where a model the related ModelManager package made is loaded and run here | [`tests/CodeBrix.Ollama.EndToEnd.Tests`](https://github.com/ellisnet/CodeBrix.Ollama/tree/main/tests/CodeBrix.Ollama.EndToEnd.Tests) |
| Native build tooling | Everything needed to build the native inference libraries for every runtime identifier; nothing in a `dotnet build` compiles it | [`llama-native-tools`](https://github.com/ellisnet/CodeBrix.Ollama/tree/main/llama-native-tools) |
| ModelQueryTool | A CodeBrix.Platform application, with its own libraries and tests, that stages a model and chats with it in a terminal view; the repository describes it as a work in progress | [`samples/ModelQueryTool`](https://github.com/ellisnet/CodeBrix.Ollama/tree/main/samples/ModelQueryTool) |

Test files worth opening first, one per feature area:

| Feature | Test file |
| --- | --- |
| The decode loop, offline | [`Engine/RunningModelConformanceTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelRunner.Tests/Engine/RunningModelConformanceTests.cs) |
| Standing in for a real model in your own tests | [`Engine/FakeRunningModel.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelRunner.Tests/Engine/FakeRunningModel.cs) |
| Reasoning and tool calls taken apart | [`Engine/ChatPipelineTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelRunner.Tests/Engine/ChatPipelineTests.cs) |
| The Jinja engine | [`Templates/Jinja/JinjaTemplateTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelRunner.Tests/Templates/Jinja/JinjaTemplateTests.cs) |
| Tool-call parsing | [`Parsing/ToolCallParserTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelRunner.Tests/Parsing/ToolCallParserTests.cs) |
| JSON schema to grammar | [`Grammar/JsonSchemaGrammarTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelRunner.Tests/Grammar/JsonSchemaGrammarTests.cs) |
| Quantizing | [`Engine/ModelQuantizerTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelRunner.Tests/Engine/ModelQuantizerTests.cs) |
| Finding the native library | [`Native/NativeLibraryLoaderTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelRunner.Tests/Native/NativeLibraryLoaderTests.cs) |
| Running an ONNX graph | [`Onnx/OnnxModelTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelRunner.Tests/Onnx/OnnxModelTests.cs) |
| Generating MIDI | [`Drivers/SkyTnt/MidiGenerationModelTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelRunner.Tests/Drivers/SkyTnt/MidiGenerationModelTests.cs) |
| Standard MIDI Files | [`Midi/MidiFileTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelRunner.Tests/Midi/MidiFileTests.cs) |
| Text from an ONNX bundle | [`Drivers/CausalLm/OnnxCausalLmModelTests.cs`](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/tests/CodeBrix.Ollama.ModelRunner.Tests/Drivers/CausalLm/OnnxCausalLmModelTests.cs) |

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/AGENT-README.txt) |
| MuseCoco text- and attribute-to-MIDI guide (ships inside the package too) | [MUSECOCO-README.txt](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/MUSECOCO-README.txt) |
| Non-package content in the repository | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.Ollama.ModelRunner.Tests](https://github.com/ellisnet/CodeBrix.Ollama/tree/main/tests/CodeBrix.Ollama.ModelRunner.Tests) |

The repository also produces [CodeBrix.Ollama.ModelManager](CodeBrix.Ollama.ModelManager.md), which has
its own guide. XML documentation ships alongside the assembly.

## License

CodeBrix.Ollama.ModelRunner is licensed under the MIT License, and the license is also named in the
package ID (`CodeBrix.Ollama.ModelRunner.MitLicenseForever`). The package carries the `LICENSE` file at
its root and requires license acceptance, and every runtime-identifier folder carries the license of the
native engine beside its library. For the provenance and licensing of open source code included in this
library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Ollama/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Ollama.ModelManager](CodeBrix.Ollama.ModelManager.md) - related: a local model store that resolves a model name to the files this package loads
- [CodeBrix.Audio.MusicGeneration](CodeBrix.Audio.MusicGeneration.md) - generated MIDI voiced through an instrument library and played
- [CodeBrix.Audio](CodeBrix.Audio.md) - reads, writes and plays Standard MIDI Files and instruments
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.Ollama on GitHub](https://github.com/ellisnet/CodeBrix.Ollama) - source and tests
