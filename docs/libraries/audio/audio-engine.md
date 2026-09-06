<sub>[CodeBrix](../../../README.md) › [Libraries](../README.md) › [CodeBrix.Audio](../CodeBrix.Audio.md) › The bundled audio engine</sub>

# The bundled audio engine

**The `CodeBrix.Audio.MitLicenseForever` package ships a second assembly,
`CodeBrix.Audio.Engine`, alongside `CodeBrix.Audio`: a full cross-platform audio engine whose device
playback and recording, effects, editing and mixing, MIDI, metadata, synthesis and visualization all
live in its own namespaces.** Both assemblies are referenced automatically, and there is no separate
Engine package to find. For ordinary playback you never need to touch it - `WaveOutEvent`,
`AudioFilePlayer` and `SharedAudioOutput` in [CodeBrix.Audio wrap it for you](playback.md) - so this
page is about the jobs only the Engine does, and about the two places where having two assemblies
shows.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Audio](https://github.com/ellisnet/CodeBrix.Audio) |
| **Packages** | [`CodeBrix.Audio.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MitLicenseForever) - one package, two assemblies |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later. No system audio package and no system-wide codec is required on Windows, macOS or Linux |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Windows, macOS and Linux. The bundled native backend ships for `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `linux-riscv64`, `osx-x64` and `osx-arm64` |

## What it does

- Opens audio **devices** for playback and for capture, through a bundled native backend that needs
  nothing installed on any of the three desktop operating systems.
- **Records** audio: `Recorder` over an `AudioCaptureDevice`, writing through a codec factory - which
  is how [CodeBrix.Audio.Opus](../CodeBrix.Audio.Opus.md) makes `new Recorder(device, stream, "opus")`
  write Ogg Opus.
- Applies **effects**, and carries an editing and mixing layer that `CodeBrix.Audio` itself does not
  have.
- Reads and writes sound **metadata**, through `SoundMetadataReader` and `SoundMetadataWriter`.
- Holds a general-purpose **synthesis** architecture - `CodeBrix.Audio.Engine.Synthesis` - for building
  instruments: oscillators, custom banks, MIDI modifiers, arpeggiators, and sample playback of SF2
  presets.
- Has its own MIDI model, its own FFT and its own **visualization** components, separate from the ones
  in `CodeBrix.Audio`.
- Exposes a per-engine codec registry, so a consumer driving its own `AudioEngine` can register,
  unregister, re-prioritize and enumerate codecs on that engine alone.

## When to use it

Reach into `CodeBrix.Audio.Engine` for something the Engine alone offers: recording, effects, the
editing and mixing layer, metadata writing, or building a synthesized instrument. For everything else
- opening files, playing them, sound effects, MIDI music through a sampled instrument, DSP primitives
- stay in `CodeBrix.Audio`, which is the friendlier surface and wraps the Engine where it needs it.

The two assemblies share no types, and there is deliberate feature overlap: both read audio files,
both have MIDI, both have an FFT. Picking which library to use for a given task is left to the
consumer, and the two sections below are the two places where that choice becomes visible.

### Two SoundFont paths

`CodeBrix.Audio.Synth` is the renderer of record for playing a `.sf2`, and
`CodeBrix.Audio.Engine.Synthesis` is the architecture for **building** an instrument. They are not
interchangeable, and choosing wrong produces audio that is subtly wrong rather than an error. The
comparison, and the rule, are on [SoundFont and SFZ instruments](soundfont-and-sfz.md).

The Engine's own `Synthesizer` has an MPE mode of its own, but it belongs to the instrument-building
path and is driven by that side's routing rather than by a `MidiSequence`. Playing a MIDI file that
carries an expressive performance is the other path's job - see [MPE from MIDI files](mpe.md).

The Engine has no support for SFZ or Decent Sampler at all, so for those two formats there is no wrong
turn to take.

### Five type names collide across the two assemblies

Each assembly has its own type for several of the same ideas. The names are identical; only the
namespaces differ.

| Name | In `CodeBrix.Audio` | In `CodeBrix.Audio.Engine` |
| --- | --- | --- |
| `MidiFile` | `CodeBrix.Audio.Midi` | `CodeBrix.Audio.Engine.Metadata.Midi` |
| `MidiSequence` | `CodeBrix.Audio.Synth` | `CodeBrix.Audio.Engine.Editing` |
| `PlaybackState` | `CodeBrix.Audio.Wave` | `CodeBrix.Audio.Engine.Enums` |
| `VoiceActivityDetector` | `CodeBrix.Audio.Dsp` | `CodeBrix.Audio.Engine.Components` |
| `MetaEventType` | `CodeBrix.Audio.Midi` | `CodeBrix.Audio.Engine.Metadata.Midi.Enums` |

They are not the same type and there is no conversion between them. A file that imports both
namespaces of a pair gets CS0104 on the bare name - a compile error, not a wrong result, so it cannot
bite silently. The remedy is a using **alias** for the one you need less often, and **not** a second
namespace import, which is exactly what the library's own source does when it has to hold both:

```csharp
using CodeBrix.Audio.Playback;
using CodeBrix.Audio.Wave;      // PlaybackState, the one the players return
using EnginePlaybackState = CodeBrix.Audio.Engine.Enums.PlaybackState;
// and deliberately NOT: using CodeBrix.Audio.Engine.Enums;

PlaybackState state = player.PlaybackState;        // CodeBrix.Audio.Wave
EnginePlaybackState engineState = EnginePlaybackState.Playing;
```

Importing **both** namespaces and adding the alias does not help: the alias gives the other type a
second name, it does not remove the first one from the bare name's candidates. Import one, alias the
other.

## Getting started

```bash
dotnet add package CodeBrix.Audio.MitLicenseForever
```

Both assemblies arrive with that one reference. Import an Engine namespace only for something the
Engine alone offers, and alias the collision if the file also needs the `CodeBrix.Audio` type of the
same name.

Recording straight to a file, once a codec factory that can encode the format is registered:

```csharp
using System.IO;
using CodeBrix.Audio.Engine.Components;
using CodeBrix.Audio.Opus;

CodeBrixAudioOpus.Register();

// captureDevice is a CodeBrix.Audio.Engine.Abstracts.Devices.AudioCaptureDevice
using var stream = File.Create("recording.opus");
var recorder = new Recorder(captureDevice, stream, "opus");
recorder.StartRecording();
```

## Key concepts

### The native backend

Unlike `CodeBrix.Audio`, the Engine calls into a bundled native library, shipped under
`runtimes/<rid>/native/` for `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `linux-riscv64`,
`osx-x64` and `osx-arm64`. The right one loads at run time with no configuration on your part, and an
Ogg Vorbis decoder is compiled into it. Those runtime identifiers are the supported set: an
application published for any other one will start, but will throw as soon as it opens an audio
device. The `linux-riscv64` binary is there for the experimental .NET builds for RISC-V.

> [!IMPORTANT]
> The native payload must travel with your application. A normal framework-dependent or self-contained
> publish handles this; if you publish single-file, make sure your publish settings keep native
> libraries available to the host. A license notice sits beside each of the native binaries and
> travels with them into your application's output folder - keep it there when you publish.

No system audio package and no system-wide codec is required on Windows, macOS or Linux; playback is
self-contained. The backend is built from sources vendored in the repository and can be rebuilt from
them.

### Which decoder plays a file

The engine prefers its bundled native library and falls back to the managed decoders in
`CodeBrix.Audio` only where the native one cannot handle a format, so `.ogg` and `.flac` play
everywhere either way and the managed path merely costs more CPU. Nothing has to be configured:
`SharedAudioOutput` registers the managed fallbacks itself. Only if you construct your **own**
`AudioEngine` do you need `ManagedCodecs.RegisterAll(engine)` to get the same safety net.

### Driving your own engine

An application that builds its own `AudioEngine` rather than using `SharedAudioOutput` gets a wider
codec surface on it:

```csharp
engine.RegisterCodecFactory(ICodecFactory factory)
engine.RegisterPacketCodecFactory(IPacketCodecFactory factory)
bool engine.UnregisterPacketCodecFactory(string factoryId)
bool engine.SetPacketCodecPriority(string factoryId, int newPriority)
IReadOnlyList<IPacketCodecFactory> engine.GetRegisteredPacketCodecs(string codecId)
IPacketSoundDecoder engine.CreatePacketDecoder(string codecId,
    ReadOnlyMemory<byte> codecPrivate, AudioFormat? hint = null)
```

`UnregisterPacketCodecFactory` and `SetPacketCodecPriority` both match on `FactoryId` and both return
false when nothing carried that id; `SetPacketCodecPriority` overrides the factory's own priority for
that engine only. `GetRegisteredPacketCodecs` answers for one codec id, highest priority first, and
returns an empty list rather than null. Registering the same factory twice on an engine **adds it
twice** - the de-duplication belongs to `SharedAudioOutput`, not to the engine. And a factory
registered directly on an engine of your own is not visible to
`SharedAudioOutput.IsPacketCodecSupported`, because that engine is not the shared output's.

### The packet decoder interfaces live here

`IPacketSoundDecoder` and `IPacketCodecFactory` are in `CodeBrix.Audio.Engine.Interfaces`, even though
the player that consumes them, `PacketAudioPlayer`, is a `CodeBrix.Audio` type. Their contract is on
[Playback](playback.md).

## Pitfalls

- **Import one namespace of a colliding pair and alias the other.** Importing both and adding an alias
  does not remove the ambiguity.
- **A few Engine entry points are synchronous wrappers that do async I/O internally** -
  `SoundMetadataReader.Read`, `SoundMetadataWriter.WriteTags` and `RemoveTags`,
  `Recorder.StopRecording`, and anything that opens a source through them, which includes
  `AudioFormat.GetFormatFromStream`, the data providers, and therefore `AudioFilePlayer.Load`. They
  still do blocking disk or network I/O, so on a UI thread prefer the `*Async` overloads where they
  exist, or do the work on a background thread.
- **The seven shipped runtime identifiers are the supported set.** An application published for
  another one starts and then throws when it opens an audio device.
- **`ManagedCodecs.RegisterAll(engine)` is only for an engine you built yourself.**
  `SharedAudioOutput` already does it.
- **The Engine's synthesis path is the wrong tool for reproducing somebody's `.sf2`.** It has no
  modulators, no per-voice LFO and no per-voice filter; use `CodeBrix.Audio.Synth` for that.

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide, covering both assemblies (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/AGENT-README.txt) |
| Engine tests: the native decode path, the length arithmetic a transport depends on, and the engine-level packet registry | [tests/CodeBrix.Audio.Engine.Tests](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tests/CodeBrix.Audio.Engine.Tests) |
| How the native backends are built and verified | [tools/build_native_libraries](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tools/build_native_libraries) |

## License

CodeBrix.Audio is licensed under the MIT License, and the license is also named in the package ID
(`CodeBrix.Audio.MitLicenseForever`), which covers both bundled assemblies. The license notice for the
bundled native backend travels with the native binaries into your application's output folder. For the
provenance and licensing of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [Playback](playback.md) - the friendlier surface that wraps this engine
- [SoundFont and SFZ instruments](soundfont-and-sfz.md) - the two-SoundFont-paths decision in full
- [DSP building blocks](dsp.md) - the analysis primitives on the other side of the pair
- [CodeBrix.Audio](../CodeBrix.Audio.md) - the front door of the audio section
