<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Audio</sub>

# CodeBrix.Audio

**CodeBrix.Audio is a fully managed, cross-platform audio library for .NET: it reads WAV, MP3, Ogg
Vorbis, FLAC and AIFF, reads and writes Standard MIDI Files, plays media and sound effects, plays
sampled instruments in three formats, plays a multi-track song whose parts each come from a recording
or from a MIDI performance, and exposes DSP primitives for analysis.** All file decoding and all
synthesis are managed code with no platform-specific interop, so a track behaves identically on
Windows, macOS and Linux; playback goes through a bundled engine and its native backend, and nothing
has to be installed. You reach for it from any .NET 10 application, or from a CodeBrix.Platform
application that needs audio the UI layer does not provide.

This page is the front door of a section. Each capability area below has a page of its own with the
detail, the examples and the pitfalls.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Audio](https://github.com/ellisnet/CodeBrix.Audio) |
| **Packages** | [`CodeBrix.Audio.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MitLicenseForever) - ships the `CodeBrix.Audio` and `CodeBrix.Audio.Engine` assemblies<br>[`CodeBrix.Audio.ModestSynth.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.ModestSynth.MitLicenseForever) - the synthesis add-on, on its own [page](CodeBrix.Audio.ModestSynth.md) |
| **License** | MIT for both; see [License](#license) |
| **Requires** | .NET 10 or later. No system audio package and no system-wide codec is required on Windows, macOS or Linux |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application, through the [AudioPlayer add-in](../platform/add-ins/AudioPlayer.md) |
| **Platforms** | Windows, macOS and Linux. The bundled native backend ships for `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `linux-riscv64`, `osx-x64` and `osx-arm64` |

## What it does

Every bullet is a page.

- **[Reads and writes audio files](audio/reading-and-writing-files.md)** - WAV, MP3 with gapless
  trimming, Ogg Vorbis, FLAC and AIFF; writes WAV, AIFF and Standard MIDI Files; reads ID3v2 tags and
  Vorbis comments; and lets another package add a format to the by-extension reader registry.
- **[Plays audio](audio/playback.md)** - a media transport with a scrubber (`AudioFilePlayer`),
  decode-once overlapping sound effects (`SoundEffectClip`), a provider-driven output (`WaveOutEvent`),
  one shared output device, and a player for audio that arrives as bare codec packets out of a media
  container (`PacketAudioPlayer`), plus the codec seam a package under another license plugs into.
- **[Reads and writes MIDI files](audio/midi-files.md)** - an editable file model and an immutable
  playable sequence, tolerant reading that reports what it could not honor rather than refusing, tempo
  maps, and two hooks onto the messages a sequence plays.
- **[Plays SoundFont and SFZ instruments](audio/soundfont-and-sfz.md)** - a specification-faithful SF2
  renderer with the full generator **and** modulator model, and a managed SFZ engine measured against a
  corpus of real free instruments with no unimplemented opcodes left over. One transport-style player
  and one offline renderer drive either.
- **[Plays Decent Sampler instruments](audio/decent-sampler.md)** - the whole documented format, with
  the live parameter model behind a preset's knobs, effect chains and buses, modulators, key switches,
  note sequences and an arpeggiator, and samples that stream from disk under a memory budget.
- **[Plays a multi-track song, and a stems download](audio/multi-track-and-suno.md)** - several parts
  on one transport, each a recording, a MIDI performance or both at once, with a crossfade between
  them, level matching, an offline bounce and a merged General MIDI export.
- **[Reads MPE out of a MIDI file](audio/mpe.md)** - zones, per-note bend, slide, press and lift,
  applied identically across all three sampled instrument formats.
- **[Bundles a full audio engine](audio/audio-engine.md)** - devices, recording, effects, editing and
  mixing, metadata, an instrument-building synthesis architecture and visualization, in the second
  assembly the same package ships.
- **[Supplies DSP building blocks](audio/dsp.md)** - FFT, biquad filters, an envelope follower and
  voice-activity detection.

Synthesis - oscillators that generate sound instead of playing a recording, and the creative effects a
synth is expected to have - lives in the add-on package,
[CodeBrix.Audio.ModestSynth](CodeBrix.Audio.ModestSynth.md).

## When to use it

Reach for CodeBrix.Audio when an application needs to open an audio file, play it, or make sound of
its own, and you want that to work the same way on all three desktop operating systems without asking
the machine to supply a codec. It is the audio layer the rest of the family builds on: register a
codec with it and the new format reaches every player in the family, including the
[AudioPlayer add-in](../platform/add-ins/AudioPlayer.md) and the
[GameEngine](CodeBrix.Platform.GameEngine.md).

Opus is not in this package. `.opus` files are recognized - metadata, duration, channels and rate all
read correctly - but do not decode, and fail with a message saying so. Opus is BSD-3-Clause rather
than MIT, so it ships as the separate add-on [CodeBrix.Audio.Opus](CodeBrix.Audio.Opus.md); one call
wires it in.

What the library deliberately does not do:

- No lossy or lossless encoding of compressed formats. It reads MP3, Ogg Vorbis and FLAC, but writes
  only `.wav` (`WaveFileWriter`), `.aiff` (`AiffFileWriter`) and Standard MIDI Files
  (`MidiFile.Export`).
- No operating-system codec paths: no Windows ACM / DMO / Media Foundation, no macOS AVFoundation, no
  GStreamer route.
- No streaming from a URL. Readers take a file name or a `Stream`; fetching over the network is your
  code's job.
- No editing of tags in place. `Id3v2Tag` reads a tag from a stream and `Id3v2Tag.Create` builds one
  from key/value pairs, but there is no re-tag-this-file-on-disk operation, and Vorbis comments are
  read-only.
- No general-purpose resampler is exposed: the playback types do it internally, and the reader layer
  does not do it at all.
- DSP is primitives only - there is no turnkey onset, pitch or beat detector and no audio-to-MIDI
  transcriber.
- No visualization widgets in `CodeBrix.Audio`. There is an FFT and there are filters; drawing a
  spectrum or a waveform is your UI framework's job.
- No effects, mixing, multi-track editing or recording in the `CodeBrix.Audio` assembly - those live
  in the [bundled Engine](audio/audio-engine.md).
- No instrument user interface. A Decent Sampler preset's interface section is parsed completely and
  becomes a live control model, and nothing here draws it.
- No store, no licensing and no copy protection, and no plugin hosting: this is a library, not a
  plugin host and not a plugin. It plays instrument **files**.
- No MIDI devices. MIDI arrives as a Standard MIDI File, or as messages your code sends.

## Getting started

```bash
dotnet add package CodeBrix.Audio.MitLicenseForever
```

The package has no NuGet dependencies of its own. Everything it needs, including the second assembly
it ships and that assembly's native backend, is inside it, and both assemblies are referenced
automatically.

```csharp
using CodeBrix.Audio.Wave;       // readers/writers, WaveFormat, MP3 frames, ID3,
                                 //   playback (WaveOutEvent, SharedAudioOutput)
using CodeBrix.Audio.Playback;   // AudioFilePlayer, SoundEffectClip, MidiMusicPlayer,
                                 //   MultiTrackPlayer, PacketAudioPlayer
using CodeBrix.Audio.Playback.Suno;          // a stems download, as a song
using CodeBrix.Audio.Midi;       // MIDI file read/write + event hierarchy
using CodeBrix.Audio.Dsp;        // FFT, biquad filters, analysis primitives
using CodeBrix.Audio.Synth;      // SoundFont rendering + MIDI music playback
using CodeBrix.Audio.Synth.Sfz;  // SFZ instruments
using CodeBrix.Audio.Synth.DecentSampler;    // Decent Sampler instruments
using CodeBrix.Audio.Synth.Mpe;  // MpeMode and the zone model
```

A complete console application needs one package reference and nothing else - no native-asset package,
no runtime identifier list, no system dependency:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>disable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.Audio.MitLicenseForever" />
  </ItemGroup>
</Project>
```

Its `Program.cs` plays a file to the end and prints the duration:

```csharp
using System;
using System.Threading;
using CodeBrix.Audio.Playback;

var finished = new ManualResetEventSlim(false);

using var media = new AudioFilePlayer();
media.PlaybackEnded += (s, e) => finished.Set();
media.Load(args[0]);                  // .wav / .mp3 / .ogg / .flac

Console.WriteLine($"Duration: {media.Duration}");
media.Play();
finished.Wait();
```

`Duration` is available as soon as `Load` returns, and the format is identified from the file's
content - the same code plays every built-in format.

## Key concepts

The section is nine pages. Read the one that matches the job.

### Reading and writing files

The readers hand back the samples themselves: one type per format, one that dispatches on extension,
and a writer for WAV, AIFF and Standard MIDI Files. MP3 is gapless by default, Ogg Vorbis and FLAC
report exact durations and seek accurately, and tags come off MP3, Ogg Vorbis and FLAC. The
by-extension reader registry is one of the two seams another package uses to add a format.
→ **[Reading and writing files](audio/reading-and-writing-files.md)**

### Playback and sound effects

Pick the player by the shape of the sound: `AudioFilePlayer` for a long track with a transport,
`SoundEffectClip` for short overlapping one-shots, `WaveOutEvent` for a provider chain you already
have, and `PacketAudioPlayer` for audio a demultiplexer lifted out of a container. They all mix into
one `SharedAudioOutput`. The codec seam is here too, with the rules an add-on codec package must obey.
→ **[Playback and sound effects](audio/playback.md)**

### MIDI files

Two types are named for MIDI files and they do different jobs: `MidiFile` is the editable model and
`MidiSequence` is the immutable playable one. Reading is tolerant by default and reports what it could
not honor in `Problems`; strict mode is there for a validator. The two message hooks a player exposes
look similar and do opposite things.
→ **[MIDI files](audio/midi-files.md)**

### SoundFont and SFZ instruments

The package contains two things that can play a SoundFont, and choosing wrong produces audio that is
subtly wrong rather than an error - that decision is the first thing on the page. SFZ has exactly one
path, a managed engine with the whole articulation and modulation model, which reports the opcodes a
library used that it does not implement.
→ **[SoundFont and SFZ instruments](audio/soundfont-and-sfz.md)**

### Decent Sampler instruments

The third sampled format, and the only one with a live parameter model: a preset's knobs are
addressable, and writing one fires the same bindings a user turning it would. The page covers
containers, tags, effect chains and buses, auxiliary outputs, modulators, key switches, note sequences
and the arpeggiator, the streaming and memory policy, the residual table, and where this engine and
the reference player differ.
→ **[Decent Sampler instruments](audio/decent-sampler.md)**

### Multi-track songs and stems

`MultiTrackPlayer` plays several parts on one transport, and a part can be a recording, a MIDI
performance or both at once, with a crossfade rather than a restart between them. A stems download
loads straight into that model, with tolerant MIDI reading, coverage figures for near-empty
transcriptions, and a measurement of how far each transcription sits from its own recording. The plain
way to play a whole downloaded song is on the same page.
→ **[Multi-track songs and stems](audio/multi-track-and-suno.md)**

### MPE from MIDI files

An expressive performance recorded as MIDI Polyphonic Expression plays the same way through a
SoundFont, an SFZ library and a Decent Sampler instrument, because the whole contract lives in one
shared model. `MpeMode.Off` is the default; `MpeMode.Auto` is where to start for a clip exported from
a sequencer, which normally carries no configuration message.
→ **[MPE from MIDI files](audio/mpe.md)**

### The bundled audio engine

The same package ships a second assembly with device playback and recording, effects, an editing and
mixing layer, metadata, an instrument-building synthesis architecture and visualization. You do not
need it for ordinary playback. The page covers what only it does, the native backend, and the five
type names that collide across the two assemblies.
→ **[The bundled audio engine](audio/audio-engine.md)**

### DSP building blocks

FFT, biquad filters, an envelope follower and energy-based voice-activity detection, plus a windowed
FFT over a stream and a few utilities. Primitives, not finished detectors.
→ **[DSP building blocks](audio/dsp.md)**

## Examples

Sound effects are decoded once and then triggered as often as you like, overlapping freely:

```csharp
using CodeBrix.Audio.Playback;

using var laser = SoundEffectClip.Load("laser.ogg");   // decoded once, into memory
laser.Play();                                          // fire and forget
laser.Play(0.4f);                                      // again, quieter, overlapping the first
```

MIDI music through a SoundFont has the same transport surface as the file player, and the same
`Load(instrumentPath, midiFilePath)` overload takes a `.sfz` or a Decent Sampler preset instead:

```csharp
using CodeBrix.Audio.Playback;
using CodeBrix.Audio.Synth;

var soundFonts = new SoundFontCache();          // a .sf2 is large - load it once, share it

var music = new MidiMusicPlayer();
music.Load(soundFonts.Get("GeneralUser.sf2"), new MidiSequence("level1.mid"));
music.IsLooping = true;
music.Play();                                   // same transport surface as AudioFilePlayer
```

Playing a Decent Sampler instrument, with the add-on registered first in case a preset generates
rather than samples:

```csharp
using CodeBrix.Audio.ModestSynth;               // only if a preset generates rather than samples
using CodeBrix.Audio.Playback;
using CodeBrix.Audio.Synth;
using CodeBrix.Audio.Synth.DecentSampler;

ModestSynth.Register();                         // BEFORE any instrument is loaded

using var instrument = DecentSamplerInstrument.Load("Choir.dslibrary");
instrument.GetControl("ATTACK")?.SetValue(0.35);   // the preset's own knob

using var music = new MidiMusicPlayer();
music.Load(instrument, new MidiSequence("song.mid"));
music.Play();                                   // the same transport drives .sf2, .sfz and .dspreset

// instrument.Problems and instrument.UnsupportedFeatures say what a preset asked for
// that could not be honored - the first thing to check if a library sounds off.
```

Playing a stems download as one song, and swapping a part to its MIDI while it plays:

```csharp
using CodeBrix.Audio.Playback;
using CodeBrix.Audio.Playback.Suno;

var song = SunoStemsLoader.Load("My Song Stems.zip");   // the zip, or the folder

using var player = song.CreatePlayer(new SunoPlayerOptions
{
    GeneralMidiSoundFontPath = "FluidR3_GM.sf2",        // instruments for the MIDI stems
    AutoSetRelativeTrackLevels = true,                  // synthesized parts sit where the recordings sat
});

var bass = player["Bass"];                              // by name, like song["Bass"]
if (bass.HasMidiSource)                                 // switching a part is a
{                                                       //   crossfade, not a restart
    bass.ActiveSource = TrackSource.Midi;
}

player.Prepare();
await player.LevelMeasurement;                          // the levels are in when this
                                                        //   returns - never null, so
                                                        //   no guard is needed
player.Play();
```

Rendering a sequence offline needs no audio device and runs faster than real time:

```csharp
using CodeBrix.Audio.Synth;

SoundFontRenderer.RenderToWavFile(
    new SoundFont("GeneralUser.sf2"),
    new MidiSequence("level1.mid"),
    "level1.wav",
    tail: TimeSpan.FromSeconds(2));             // let the reverb decay rather than cutting it

// The same renderer takes an SfzInstrument or a DecentSamplerInstrument in place of the SoundFont.
```

## Using it in a CodeBrix.Platform application

CodeBrix.Audio is a standalone library with no CodeBrix.Platform dependency: a CodeBrix.Platform
application references it the same way any other .NET 10 application does. What connects it to the
rest of the family is the codec registry. A codec registered with `SharedAudioOutput` reaches
`AudioFilePlayer`, `SoundEffectClip`, `WaveOutEvent` and the GameEngine's audio stack, which is why
adding [CodeBrix.Audio.Opus](CodeBrix.Audio.Opus.md) and calling its `Register()` makes `.opus` play
through the [AudioPlayer add-in](../platform/add-ins/AudioPlayer.md) and the
[GameEngine](CodeBrix.Platform.GameEngine.md) as well. The consuming application takes that dependency
and makes the call - the add-ins never do.

The [AudioPlayer add-in](../platform/add-ins/AudioPlayer.md) wraps the file player, the sound-effect
path and MIDI music through any of the three sampled instrument formats as non-visual XAML elements
with a two-way bindable position, which is what a page wants; it surfaces a Decent Sampler preset's
control model and the MPE settings on the element as well. It does not wrap the multi-track player or
the stems loader, and it does not render offline. Reach for the library's own types for those, and
whenever the audio does not belong to a page.

The publishing note matters on every head: the native payload has to travel with the application, so a
single-file publish must keep native libraries available to the host.

## Pitfalls

Each page carries the pitfalls for its own area. These are the ones that catch people first,
whichever part of the library they start with.

- **Float versus bytes.** `WaveFileReader` and `Mp3FileReader` are `WaveStream`s that yield raw PCM
  bytes. Call `.ToSampleProvider()`, or use `AudioFileReader`, which always exposes float.
- **Dispose readers and writers.** A `WaveFileWriter` flushes a valid RIFF header only on `Dispose`,
  and a stream from the reader registry keeps the file locked until you dispose it.
- **There is no resampler in the reader layer, and `WaveOutEvent` does not resample.**
  `AudioFilePlayer` and `SoundEffectClip` both convert; pin the output format at start-up with
  `SharedAudioOutput.Configure(48000)` when you are playing media containers.
- **Two SoundFont paths, two MIDI hooks, two MIDI file models.** Read the decision guide on the
  relevant page before choosing one - the wrong choice in each of those pairs fails quietly rather
  than loudly.
- **The audio thread is real-time.** Every source `Read` and both `MidiMusicPlayer` hooks run on it:
  no blocking, no I/O, no UI marshalling.
- **Re-opening a reader per trigger is the single most expensive mistake available in this library.**
  Use `SoundEffectClip.Load` for anything triggered repeatedly, and share the big instrument assets
  through `SoundFontCache`, `SfzInstrumentCache` or `DecentSamplerInstrumentCache`.
- **A synthesizer factory must return a new synthesizer every time.** Share the instrument behind
  them, never the synthesizer, and remember that no synthesizer here is thread-safe.
- **Register an add-on package before you load.** `ModestSynth.Register()` must run before a Decent
  Sampler instrument is built, and `CodeBrixAudioOpus.Register()` before a `.opus` file is opened.
- **`.opus` needs the [CodeBrix.Audio.Opus](CodeBrix.Audio.Opus.md) add-on package** and one
  `CodeBrixAudioOpus.Register()` call.

## Samples and tools in the repository

The repository ships two packages and has no sample applications and no demo projects. Everything
below is developer tooling and test data: none of it is packed, and none of it is needed to consume
either package. Nothing in `tools/` installs anything on your machine - every script checks for what
it needs, names anything missing, prints the command that would install it, and stops.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Native library build scripts | Builds the native audio backend for every shipped runtime identifier, in containers on Linux | [`tools/build_native_libraries`](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tools/build_native_libraries) |
| Test fixture generator | Regenerates the synthesized audio and SoundFont fixtures the tests run against | [`tools/make_test_fixtures`](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tools/make_test_fixtures) |
| SFZ opcode survey | Decides the scope of SFZ support by counting rather than guessing, parsing a folder of real SFZ libraries with the library's own parser | [`tools/sfz_opcode_survey`](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tools/sfz_opcode_survey) |
| Decent Sampler feature survey | The same idea for the other format: what a corpus of real libraries actually uses, against what the engine recognizes | [`tools/ds_feature_survey`](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tools/ds_feature_survey) |
| Codec sources | Worked examples of the decoder and factory shapes an add-on package implements | [`src/CodeBrix.Audio/Codecs`](https://github.com/ellisnet/CodeBrix.Audio/tree/main/src/CodeBrix.Audio/Codecs) |

The test suites are the best worked examples of every public API. Run them with
`dotnet test CodeBrix.Audio.slnx`. Tests that make sound are opt-in, so an ordinary run is silent and
headless-safe, and so are the tests that need a corpus of real material you supply:

```bash
CODEBRIX_AUDIO_RUN_PLAYBACK_TESTS=1 dotnet test
CODEBRIX_AUDIO_ENGINE_RUN_PLAYBACK_TESTS=1 dotnet test
```

Test audio is synthesized in the repository - sine tones, sweeps, noise and silence - and the
SoundFont fixture is a minimal, fully synthetic file built from sine tones. No third-party audio, no
real SoundFont and no sampler library is committed anywhere in the repository.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Audio/blob/main/README.md) |
| Complete API guide, covering both bundled assemblies (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/AGENT-README.txt) |
| The add-on's own guide | [src/CodeBrix.Audio.ModestSynth/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/src/CodeBrix.Audio.ModestSynth/AGENT-README.txt) |
| Tools and other non-package content | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.Audio.Tests](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tests/CodeBrix.Audio.Tests) |
| Engine tests | [tests/CodeBrix.Audio.Engine.Tests](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tests/CodeBrix.Audio.Engine.Tests) |
| Add-on tests | [tests/CodeBrix.Audio.ModestSynth.Tests](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tests/CodeBrix.Audio.ModestSynth.Tests) |

XML documentation ships alongside every assembly, and one `AGENT-README.txt` covers both bundled
assemblies, because one package ships both.

## License

CodeBrix.Audio is licensed under the MIT License, and the license is also named in the package ID
(`CodeBrix.Audio.MitLicenseForever`), which covers both bundled assemblies. The synthesis add-on,
`CodeBrix.Audio.ModestSynth.MitLicenseForever`, is MIT as well, and the two packages are built and
published from this one repository at the same version. License acceptance is required at install
time, and the license notice for the bundled native backend travels with the native binaries into your
application's output folder. For the provenance and licensing of open source code included in this
library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [Reading and writing files](audio/reading-and-writing-files.md) - the first page of the section
- [CodeBrix.Audio.ModestSynth](CodeBrix.Audio.ModestSynth.md) - the synthesis add-on, oscillators and creative effects
- [CodeBrix.Audio.Opus](CodeBrix.Audio.Opus.md) - adds Ogg Opus decoding and encoding with one call
- [AudioPlayer add-in](../platform/add-ins/AudioPlayer.md) - playing audio from a CodeBrix.Platform application
- [ellisnet/CodeBrix.Audio on GitHub](https://github.com/ellisnet/CodeBrix.Audio) - source, tests and tools
