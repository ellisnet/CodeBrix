<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Audio</sub>

# CodeBrix.Audio

**CodeBrix.Audio is a fully managed, cross-platform audio file library for .NET: it reads WAV, MP3,
Ogg Vorbis and FLAC, reads and writes Standard MIDI Files, plays media and sound effects, renders
SoundFonts and SFZ instruments, and exposes DSP primitives for analysis.** All file decoding and all
synthesis are managed code with no platform-specific interop, so a track behaves identically on
Windows, macOS and Linux; playback goes through a bundled engine and its native backend. You reach for
it from any .NET 10 application, or from a CodeBrix.Platform application that needs audio the UI layer
does not provide.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Audio](https://github.com/ellisnet/CodeBrix.Audio) |
| **Packages** | [`CodeBrix.Audio.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later. No system audio package and no system-wide codec is required on Windows, macOS or Linux |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Windows, macOS and Linux. The bundled native backend ships for `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `linux-riscv64`, `osx-x64` and `osx-arm64` |

## What it does

- Reads WAV (`.wav`): 8/16/24/32-bit PCM and 32/64-bit IEEE float, including `WAVE_FORMAT_EXTENSIBLE`
  files.
- Reads MP3 (`.mp3`) through a fully managed MPEG audio decoder covering MPEG-1/2/2.5 Layer I/II/III -
  no ACM, no DMO, no native code.
- Reads Ogg Vorbis (`.ogg`) with exact duration and sample-accurate seeking, and reads FLAC (`.flac`)
  losslessly at 16-, 24- and 32-bit depths.
- Writes WAV, reads and writes AIFF (`.aiff`) through `AiffFileReader` / `AiffFileWriter`, and reads
  and writes Standard MIDI Files (`.mid`).
- Reads MP3 ID3v2 tags and Ogg/FLAC Vorbis comments.
- Plays audio two ways: a media player with transport and seeking (`AudioFilePlayer`) and decode-once
  sound effects that overlap freely (`SoundEffectClip`).
- Renders SoundFonts (`.sf2`) with the full generator **and** modulator model, per-voice LFOs and
  filter, reverb and chorus - driven live by `MidiMusicPlayer` or rendered offline to a WAV file by
  `SoundFontRenderer`.
- Plays SFZ (`.sfz`) instruments through a managed SFZ engine: region selection by key, velocity,
  controller and program, round robins, random layers, key switches, release triggers, exclusive off
  groups, polyphony limits, crossfades, the full modulation stack and the `_onccN` / `_curveccN` /
  `_smoothccN` CC matrix with `<curve>` support.
- Plays audio that arrives as codec packets rather than as a file (`PacketAudioPlayer`) - the shape a
  media container's demultiplexer hands out. Ogg Vorbis packets are built in; other codecs plug into
  the same packet codec seam.
- Carries the MIDI controls a sequence needs and a decoded file does not: playback speed (tempo without
  pitch change), per-channel volume, pan and program for mixing a layered arrangement live, arbitrary
  MIDI messages sent safely from any thread, and two message hooks.
- Supplies audio analysis building blocks: FFT, biquad filters, an envelope follower and voice-activity
  detection.
- Adds device playback and recording, effects, editing and mixing, MIDI, synthesis and visualization
  through the bundled `CodeBrix.Audio.Engine` assembly.

## When to use it

Reach for CodeBrix.Audio when an application needs to open an audio file, play it, or make sound of its
own, and you want that to work the same way on all three desktop operating systems without asking the
machine to supply a codec. It is the audio layer the rest of the family builds on: register a codec
with it and the new format reaches every player in the family, including the
[CodeBrix.Platform AudioPlayer add-in](../platform/add-ins/AudioPlayer.md) and the
[CodeBrix.Platform GameEngine](CodeBrix.Platform.GameEngine.md).

Opus is not in this package. `.opus` files are recognized - metadata, duration, channels and rate all
read correctly - but do not decode, and fail with a message saying so. Opus is BSD-3-Clause rather than
MIT, so it ships as the separate add-on [CodeBrix.Audio.Opus](CodeBrix.Audio.Opus.md); one call wires
it in.

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
  transcriber. Build those on the FFT, the filters and the envelope follower.
- No visualization widgets. There is an FFT and there are filters; drawing a spectrum or a waveform is
  your UI framework's job.
- No effects, mixing, multi-track editing or recording in the `CodeBrix.Audio` assembly - those live in
  the bundled Engine. `CodeBrix.Audio` itself is files, formats, DSP primitives and playback facades.

## Getting started

```bash
dotnet add package CodeBrix.Audio.MitLicenseForever
```

```csharp
using CodeBrix.Audio.Wave;       // readers/writers, WaveFormat, MP3 frames, ID3,
                                 //   playback (WaveOutEvent, SharedAudioOutput)
using CodeBrix.Audio.Playback;   // media player (AudioFilePlayer) and one-shot
                                 //   sound effects (SoundEffectClip)
using CodeBrix.Audio.Midi;       // MIDI file read/write + event hierarchy
using CodeBrix.Audio.Dsp;        // FFT, biquad filters, analysis primitives
using CodeBrix.Audio.Synth;      // SoundFont (.sf2) rendering + MIDI music
                                 //   playback — see "TWO SOUNDFONT PATHS"
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

`Duration` is available as soon as `Load` returns, and the extension picks the decoder - the same code
plays every built-in format.

## Key concepts

### Two assemblies from one package

`CodeBrix.Audio.MitLicenseForever` ships two assemblies: `CodeBrix.Audio` (files, formats, DSP
primitives and playback facades) and `CodeBrix.Audio.Engine` (a full audio engine). Both are referenced
automatically; you do not add a second `PackageReference` for the Engine, and there is no separate
Engine package to find.

The two share no types, and there is deliberate feature overlap - both read audio files, both have
MIDI, both have an FFT. Picking which library to use for a given task is left to the consumer. For
ordinary playback you do not need to touch the Engine at all: `WaveOutEvent`, `AudioFilePlayer` and
`SharedAudioOutput` in `CodeBrix.Audio` wrap it for you.

### The bundled native backend

Unlike `CodeBrix.Audio`, the Engine calls into a bundled native library, shipped under
`runtimes/<rid>/native/` for `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `linux-riscv64`,
`osx-x64` and `osx-arm64`. The right one loads at run time with no configuration. Those runtime
identifiers are the supported set: an application published for any other one will start, but will
throw as soon as it opens an audio device.

> [!IMPORTANT]
> The native payload must travel with your application. A normal framework-dependent or self-contained
> publish handles this; if you publish single-file, make sure your publish settings keep native
> libraries available to the host. The license notice that ships beside each native binary travels into
> the output folder with it - keep it there when you publish.

### Two SoundFont paths - which one you want

Read this before writing any SoundFont or MIDI-synthesis code. The package contains two things that can
play a SoundFont, and they are not interchangeable. Choosing wrong does not fail loudly - it produces
audio that is subtly wrong.

| Job | Namespace | Types |
| --- | --- | --- |
| Play a `.sf2` | `CodeBrix.Audio.Synth` | `SoundFontSynthesizer`, `MidiSequence`, `MidiSequencer`, `MidiMusicPlayer` |
| Build a synthesized instrument | `CodeBrix.Audio.Engine.Synthesis` | `Synthesizer`, `Sequencer`, `SoundFontBank`, `MultiInstrumentBank`, the `Generators/` and `Voices/` types |

`CodeBrix.Audio.Synth` is a spec-faithful SF2 renderer: it implements the SoundFont generator and
modulator model, per-voice LFOs, a per-voice lowpass filter, volume and modulation envelopes, reverb
and chorus. `CodeBrix.Audio.Engine.Synthesis` has none of the modulators, no per-voice LFO and no
per-voice filter, and is the right choice for oscillators, custom banks, MPE, MIDI modifiers and
arpeggiators. Neither replaces the other; they do different jobs.

SFZ has exactly one path, `CodeBrix.Audio.Synth.Sfz` (`SfzInstrument`, `SfzSynthesizer`); the Engine
has no SFZ support. `SfzSynthesizer` and `SoundFontSynthesizer` implement the same `IMidiSynthesizer`
contract, which is why `MidiSequencer`, `MidiMusicPlayer` and `SoundFontRenderer` drive either format
without caring which - consumers choose a file format, not an API.

### Reading audio

`WaveFileReader`, `Mp3FileReader`, `OggVorbisFileReader` and `FlacFileReader` each open one format;
`AudioFileReader` opens `.wav`, `.mp3`, `.ogg` or `.flac` by extension and exposes 32-bit float samples.
`OggVorbisFileReader` reports exact `TotalTime`, seeks sample-accurately and exposes `.Tags` and
`.EncoderVendor`; `FlacFileReader` hands back PCM at the file's own bit depth widened to the next
standard container, and exposes `.Tags` and `.SourceBitsPerSample`.

```csharp
using CodeBrix.Audio.Wave;

// .wav, .mp3, .ogg or .flac - the extension picks the decoder
using var reader = new AudioFileReader("track.ogg");
float[] buffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];
int read = reader.Read(buffer);
```

Invalid or corrupt files throw standard exceptions - `FormatException`, `EndOfStreamException`,
`ArgumentException`. Readers and writers are `IDisposable`; dispose them, or use `using`, to release the
underlying stream.

### Playback types and the shared output

`SharedAudioOutput` is the one shared output that `WaveOutEvent`, `AudioFilePlayer` and
`PacketAudioPlayer` mix into. Every `WaveOutEvent` is a voice in that one device rather than a device of
its own, so overlapping many sounds is cheap mixing rather than many device opens.

- `SoundEffectClip` decodes a short sound once into memory and plays it as often as you like, including
  many times at once: `Load` (path, bytes or stream), `Play(volume)`, `StopAll`, `Duration`,
  `ActiveVoiceCount`. Right for effects, wrong for a soundtrack.
- `AudioFilePlayer` is the long-running player with media-transport controls: `Load`, `Play`, `Pause`,
  `Stop`, `Seek`, `Volume`, and readable `Position` and `Duration`. It streams from disk.
- `PacketAudioPlayer` plays audio arriving as bare codec packets: `Open`, `Play`, `Pause`, `Stop`,
  `Seek`, `Volume`, `Position`, and end-of-track trimming.
- `SharedAudioOutput.Configure(sampleRate[, channels])` pins the output format once at start-up, and
  `Shutdown()` releases it.

### The two MIDI message hooks - which one you want

`MidiMusicPlayer.MidiMessageProcessed` is observe-only, runs after the message has been delivered and
cannot break playback. This is almost always the one you want. `MidiMusicPlayer.MidiMessageFilter`
*replaces* delivery: your hook now owns sending the message on.

> [!WARNING]
> A filter that inspects a message and returns without calling `ProcessMidiMessage` on the synthesizer
> it was handed silences the music completely.

Both hooks run on the real-time audio thread. Both must be fast and allocation-free, must never block
or touch UI, and must not call back into the player - that takes the same lock the audio thread is
already holding, and deadlocks. The synthesizer passed to a filter is safe to use from inside that call
only; never store it. To send messages from your own thread use `MidiMusicPlayer.SendMidiMessage` and
the `SetChannel*` helpers, which take the lock properly. There is deliberately no property handing back
the `IMidiSynthesizer`.

### Two types named for MIDI files

`CodeBrix.Audio.Midi.MidiFile` is the editable file model: read it, edit the event collection, write it
back out. `CodeBrix.Audio.Synth.MidiSequence` is the immutable decoded sequence you play - flattened
absolute-time messages, no tracks, no meta events, no editing, no writing. Convert with
`MidiSequence.FromEvents(MidiEventCollection)`; there is deliberately no reverse conversion, because
the sequence has already discarded track structure and non-playable meta events.

`MidiSequence` does not carry tempo, time signature, markers or track names. It consumes tempo changes
while merging tracks - they are baked into the message time stamps and dropped - and never parses the
rest at all. Read them from `MidiFile` instead:

```csharp
var file = new MidiFile(path, strictChecking: false);   // CodeBrix.Audio.Midi
var bpm  = file.Events[0].OfType<TempoEvent>().First().Tempo;
var sig  = file.Events[0].OfType<TimeSignatureEvent>().FirstOrDefault();
```

Parsing the same file twice - once as `MidiSequence` to play, once as `MidiFile` to inspect - is the
intended pattern. MIDI files are kilobytes; this costs nothing.

### Five type names collide across the two assemblies

`MidiFile`, `MidiSequence`, `PlaybackState`, `VoiceActivityDetector` and `MetaEventType` each exist in
both `CodeBrix.Audio.*` and `CodeBrix.Audio.Engine.*`. They are not the same types and there is no
conversion between them. A file that imports both namespaces of a pair gets CS0104, which is a compile
error rather than a wrong result, so it cannot bite silently. Import one, alias the other - the remedy
the library's own source uses:

```csharp
using CodeBrix.Audio.Playback;
using CodeBrix.Audio.Wave;      // PlaybackState, the one the players return
using EnginePlaybackState = CodeBrix.Audio.Engine.Enums.PlaybackState;
// and deliberately NOT: using CodeBrix.Audio.Engine.Enums;

PlaybackState state = player.PlaybackState;        // CodeBrix.Audio.Wave
EnginePlaybackState engineState = EnginePlaybackState.Playing;
```

Importing both namespaces and adding the alias does not help.

### SFZ instruments

`SfzInstrument` is a playable SFZ instrument: typed regions, decoded samples (WAV, FLAC and Ogg through
the reader registry, with wrong-case and backslash paths resolved), modulation curves and initial
controller state. Loading is tolerant - missing samples land in `.Problems` and unimplemented opcodes in
`.UnsupportedOpcodes`, which is the first place to look when a library sounds wrong. Samples decode
eagerly, so memory follows the library's size; `SfzInstrumentCache` loads an instrument once and shares
it, keyed by path, and is the `SoundFontCache` of SFZ.

`SfzSynthesizer` is a peer of `SoundFontSynthesizer` on the same contract and equally not thread-safe.
Underneath the playable layer sits a structural one for tooling rather than playback: `SfzParser`
(`ParseFile` / `ParseText`, reading headers, opcodes, `#define` and `#include`) with `SfzFile`,
`SfzSection` and `SfzOpcode`, where `SfzFile.Resolve(region)` applies region to group to master to
global inheritance. Unknown opcodes are carried, never fatal.

### Adding a codec from another package

CodeBrix.Audio is MIT and stays that way, so a codec under a different license belongs in its own
package that depends on this one. Everything such a package needs is public API; nothing here has to
change to accept one. There are two stream seams:

- **Playback**, which identifies formats by content: supply an `ICodecFactory` and call
  `SharedAudioOutput.RegisterCodecFactory(...)`. That reaches `AudioFilePlayer`, `SoundEffectClip`,
  `WaveOutEvent` and the GameEngine's audio stack. The registration is remembered for the process, so
  it survives `SharedAudioOutput.Shutdown()` and is re-applied to every engine started afterwards.
- **Reading by file name**, which dispatches on extension:
  `AudioFileReaderRegistry.Register(".opus", s => new OpusFileReader(s));`

Build a decoder on `ManagedSoundDecoder` (public, in `CodeBrix.Audio.Codecs`): derive from it, supply
`ReadSourceSamples`, `SeekSource` and `DisposeCore`, and call `Initialize(channels, sampleRate,
totalFrames)` once the file's format is known. `OggCodecSniffer` identifies which codec an Ogg container
carries without disturbing the stream position.

Two rules govern the stream seam. The factory is handed a stream it does **not** own - do not close it,
and do not make your reader close it either; the registry opened the file and keeps the handle. And the
metadata layer reports the format identifier `ogg` for every Ogg stream whatever codec is inside, so an
Ogg-capable factory is offered Vorbis, Opus and Ogg FLAC alike: check what you were handed with
`OggCodecSniffer`, return null for anything else, and reset the stream position on entry when
`stream.CanSeek`, because the engine does not rewind between factories on that path. The built-in
native factory sits at priority 0 and the managed fallbacks at -10.

### The packet seam

A demultiplexer hands out bare codec packets with no framing of their own, and that is what
`PacketAudioPlayer` consumes. `IPacketSoundDecoder` decodes one packet at a time:

```csharp
int DecodePacket(ReadOnlySpan<byte> packet, Span<float> output)
int MaxSamplesPerPacket { get; }   // size `output` to this
int PreSkipSamples { get; }        // codec priming, per channel
void Reset()                       // after the source jumps
int Channels { get; } int SampleRate { get; }
int ConcealLoss(int lostFrames, Span<float> output)   // a gap
bool SupportsLossConcealment { get; }
```

`DecodePacket` may return zero, and that is success rather than an ending: a lapped-transform codec
finalizes a packet's samples only once the next packet has been overlapped onto it, so the first packet
after construction or `Reset()` yields nothing. `IPacketCodecFactory` mirrors `ICodecFactory` exactly,
except that `SupportedCodecIds` names the codec ("vorbis", "opus") rather than the container.
`SharedAudioOutput.RegisterPacketCodecFactory(...)` lasts for the process and de-duplicates on the
instance, so keep one factory instance per add-on package.

To ask whether a codec is available without starting anything, use
`SharedAudioOutput.IsPacketCodecSupported("opus")` or `SharedAudioOutput.SupportedPacketCodecIds` -
both match case-insensitively and neither opens the audio device.
`SharedAudioOutput.CreatePacketDecoder(...)` does open it, because the codec registry lives on the
running engine.

### The packet feed is pulled, and seeking is a contract

Your application implements `IAudioPacketSource`, and the player asks it for the next packet on the
audio thread, exactly when it needs one:

```csharp
public interface IAudioPacketSource
{
    bool TryReadPacket(out AudioPacket packet);   // false = none ready
    bool EndOfStream { get; }                     // true = no more, ever
}
```

Both members must return immediately. Read ahead on your own thread into a bounded queue and hand
packets out of that queue; never block and never do I/O here. Running dry is not an error: return false
with `EndOfStream` still false and the player plays silence for that moment and keeps the voice alive.
Playback ends only when `EndOfStream` is true and the decoded audio has run out, at which point
`PlaybackEnded` is raised away from the audio thread.

`Position` is the clock. It counts the audio actually handed to the mixer since the last `Seek`, at the
codec's own sample rate, and is readable from any thread; silence played through an underrun does not
advance it, but samples discarded as codec priming or seek pre-roll do, because they are media time.
Because the player has no container to seek in, seeking is a contract: move your own source first, then
tell the player where it now is. Calling `Seek` while the old packets are still queued dates the clock
to the new position and then plays the old audio against it. `preRoll` is how much audio to decode and
throw away before any is heard - a codec carrying state between packets cannot decode correctly at a
jump, so start a little before the real target and pass the gap as `preRoll`.

### Trimming the end of a track, and reporting loss

An encoder pads the end of what it encodes, and the container - not the codec - records how much.
Without that being applied, the padding plays: tens of milliseconds of encoder tail at the end of every
track. Apply it with `player.SetTrailingTrim(TimeSpan)` or `SetTrailingTrimFrames(int)`, or per packet
with `AudioPacket.DiscardPadding`; both may be used, and per-packet padding is applied as the larger of
the two. Frames are the exact form, counted per channel at the decoder's own rate. `Seek` clears what is
in hand but keeps the trim, and so does `Open` - set the trim again, or to `TimeSpan.Zero`, when you
open a different track.

When your demultiplexer can see that packets are missing - a jump in the timestamps, a container-level
loss marker, a network read that gave up - say so, with the length: `AudioPacket.Loss(TimeSpan)` or
`AudioPacket.Loss(int frames)`. The player asks the decoder to conceal exactly that much and fills
whatever the decoder cannot with silence. Concealed audio is media time: it advances `Position` and
flows through the trailing-trim hold-back like any other audio. Do not use it for an underrun - a moment
when your reader has not kept up is not lost audio.

### DSP and the provider toolbox

`FastFourierTransform` and `Complex` give forward and inverse FFT; `BiQuadFilter` covers low-pass,
high-pass, band-pass, peaking and shelving; `EnvelopeFollower` tracks an amplitude envelope and
`VoiceActivityDetector` does energy-based activity detection. Alongside them sit `FftProcessor` /
`FftWindowType` for a windowed FFT over a stream of samples, `Decibels`, `CircularBuffer` and
`IgnoreDisposeStream`.

Between a reader and an output you compose providers: `BufferedWaveProvider`, `MixingWaveProvider32`,
`VolumeSampleProvider`, `PanningSampleProvider`, `OffsetSampleProvider`, `FadeInOutSampleProvider`,
`ConcatenatingSampleProvider`, `MultiplexingSampleProvider`, `MonoToStereoSampleProvider` /
`StereoToMonoSampleProvider`, `SilenceProvider`, `SignalGenerator`, `WaveChannel32`,
`RawSourceWaveStream` and `WaveRecorder`, over the `IWavePlayer` and `IWavePosition` interfaces. The
A-law and mu-law codecs in `CodeBrix.Audio.Codecs` are callable directly, one sample at a time
(`LinearToALawSample` / `ALawToLinearSample`) or in bulk
(`Decode(ReadOnlySpan<byte>, Span<short>)`).

## Examples

Sound effects are decoded once and then triggered as often as you like, overlapping freely:

```csharp
using CodeBrix.Audio.Playback;

using var laser = SoundEffectClip.Load("laser.ogg");   // decoded once, into memory
laser.Play();                                          // fire and forget
laser.Play(0.4f);                                      // again, quieter, overlapping the first
```

MIDI music through a SoundFont has the same transport surface as the file player, plus the controls a
sequence needs - and the observe-only hook lets the rest of the application react to the notes:

```csharp
var music = new MidiMusicPlayer();
music.Load(soundFonts.Get("GeneralUser.sf2"), new MidiSequence("battle.mid"));

// Observe-only: runs on the audio thread and cannot break playback. This is the
// hook for driving something outside the audio - a screen shake on a drum hit,
// a particle on a note, a rhythm-game display.
music.MidiMessageProcessed = (channel, command, note, velocity) =>
{
    if (command == 0x90 && velocity > 0 && channel == 9)    // channel 10 = drums
        Volatile.Write(ref _drumHitPending, 1);             // your own thread reads this
};

music.Play();

music.SetChannelVolume(3, 0f);   // fade a layer out of the arrangement...
music.SetChannelVolume(3, 1f);   // ...and back in
music.Speed = 0.75f;             // slow motion, unchanged pitch
```

Rendering the same sequence offline needs no audio device and runs faster than real time, so bouncing a
sequence to a `.wav` once and playing the `.wav` is cheaper than synthesizing it on every playthrough:

```csharp
using CodeBrix.Audio.Synth;

SoundFontRenderer.RenderToWavFile(
    new SoundFont("GeneralUser.sf2"),
    new MidiSequence("level1.mid"),
    "level1.wav",
    tail: TimeSpan.FromSeconds(2));             // let the reverb decay rather than cutting it

// The same renderer takes an SfzInstrument in place of the SoundFont.
```

Audio lifted out of a media container arrives as packets, and `PacketAudioPlayer` plays them:

```csharp
using CodeBrix.Audio.Playback;
using CodeBrix.Audio.Wave;

SharedAudioOutput.Configure(48000);          // what media containers carry

var player = new PacketAudioPlayer();
player.PlaybackEnded += (s, e) => { /* the track finished */ };

// codecPrivate is the setup data the container carries for the track; mySource
// is your IAudioPacketSource, which the player pulls from on the audio thread.
player.Open("vorbis", codecPrivate, mySource);
player.SetTrailingTrim(TimeSpan.FromMilliseconds(12));   // drop the encoder padding
player.Volume = 0.8f;
player.Play();

TimeSpan where = player.Position;            // the audio clock; readable from any thread
```

Writing and reading a Standard MIDI File is a matter of building an event collection and exporting it:

```csharp
using CodeBrix.Audio.Midi;
using System.Linq;

var events = new MidiEventCollection(midiFileType: 0, deltaTicksPerQuarterNote: 480);
var track = events.AddTrack();
track.Add(new TempoEvent(microsecondsPerQuarterNote: 500000, absoluteTime: 0)); // 120 BPM
track.Add(new NoteOnEvent(absoluteTime: 0, channel: 1, noteNumber: 60,
                          velocity: 100, duration: 480));                        // middle C
events.PrepareForExport();              // REQUIRED before Export (adds note-offs + end-of-track)
MidiFile.Export("out.mid", events);

var midi = new MidiFile("out.mid", strictChecking: false);
foreach (var noteOn in midi.Events[0].OfType<NoteOnEvent>())
    Console.WriteLine($"{noteOn.NoteName} vel={noteOn.Velocity} @ {noteOn.AbsoluteTime}");
```

## Using it in a CodeBrix.Platform application

CodeBrix.Audio is a standalone library with no CodeBrix.Platform dependency: a CodeBrix.Platform
application references it the same way any other .NET 10 application does. What connects it to the rest
of the family is the codec registry. A codec registered with `SharedAudioOutput` reaches
`AudioFilePlayer`, `SoundEffectClip`, `WaveOutEvent` and the GameEngine's audio stack, which is why
adding [CodeBrix.Audio.Opus](CodeBrix.Audio.Opus.md) and calling its `Register()` makes `.opus` play
through the [AudioPlayer add-in](../platform/add-ins/AudioPlayer.md) and the
[GameEngine](CodeBrix.Platform.GameEngine.md) as well. The consuming application takes that dependency
and makes the call - the add-ins never do.

The publishing note matters on every head: the native payload has to travel with the application, so a
single-file publish must keep native libraries available to the host.

## Pitfalls

- **Float versus bytes.** `WaveFileReader` and `Mp3FileReader` are `WaveStream`s that yield raw PCM
  bytes. To get normalized float samples call `.ToSampleProvider()`, or use `AudioFileReader`.
- **Non-PCM WAV files throw.** PCM and IEEE float are supported, including `WAVE_FORMAT_EXTENSIBLE`,
  but A-law, mu-law and other genuinely non-PCM WAV files throw `InvalidOperationException` - there is
  no managed codec conversion, and the companding codecs in `CodeBrix.Audio.Codecs` are not wired into
  the WAV reader.
- **Dispose readers and writers.** A `WaveFileWriter` only flushes a valid RIFF header on `Dispose`, so
  an undisposed writer produces a corrupt file.
- **Files opened through the reader registry stay locked until you dispose what you got back.**
  Dropping the reference without disposing leaves the file locked on Windows until the finalizer runs,
  so a later `File.Delete` or `File.Move` throws `IOException` - and it looks intermittent, because it
  depends on garbage-collection timing. This covers `SfzSampleData.Load` and `AudioFileReader` for any
  extension added with `Register`. To get the concrete reader type back, reach it through the `.Reader`
  property rather than casting the returned stream.
- **`WaveOutEvent` does not resample.** The shared output adopts the rate of the first sound played
  unless you pin it, and a source whose sample rate differs from the running output is rejected by
  `Init` rather than played at the wrong pitch. Mono and stereo sources are matched automatically.
- **Call `SharedAudioOutput.Configure(48000)` at start-up.** Media containers carry 48 kHz, the only
  rate conversion in this package is linear interpolation, and when the device runs at the media's rate
  no conversion runs at all. Without it, an application that has already played a 44.1 kHz sound effect
  will have started the output at 44.1 kHz, and then every video plays through the interpolator.
- **Seeking a Vorbis stream in the managed reader.** Seeking into the middle of a Vorbis packet leaves
  the decoder without the previous packet's overlap history, so up to one block after a seek can differ
  from a sequential read of the same region before the two converge. For a seamless loop point, play
  through the engine. FLAC has no such caveat - it is lossless and seeks exactly.
- **MIDI export order.** Call `MidiEventCollection.PrepareForExport()` before `MidiFile.Export()`. A
  type-0 collection may contain only one track (`Export` throws otherwise); use type 1 for multi-track
  files. `NoteOnEvent` auto-creates its paired note-off.
- **Re-opening a reader per trigger is the single most expensive mistake available in this library.**
  Use `SoundEffectClip.Load` for anything triggered repeatedly, and do not pre-load a media library at
  start-up - load the one track you are about to play, when you are about to play it.
- **Share the big assets.** A `.sf2` SoundFont runs to tens of megabytes and an SFZ library decodes all
  of its samples eagerly at load. `SoundFontCache` and `SfzInstrumentCache` exist precisely so one copy
  serves every player.
- **Do not build a pool of `WaveOutEvent` instances.** Each one is a voice, not a device; overlapping
  dozens of them is mixing inside one already-open device.
- **FFT sizes are powers of two,** and `FastFourierTransform.FFT` transforms in place over a `Complex[]`
  you own - allocate the array once and reuse it rather than per frame.
- **A single reader or stream instance is not thread-safe.** Give each thread its own reader.
- **A few Engine entry points are synchronous wrappers over async I/O** - `SoundMetadataReader.Read`,
  `SoundMetadataWriter.WriteTags` / `RemoveTags`, `Recorder.StopRecording`, and anything that opens a
  source through them, including `AudioFilePlayer.Load`. They still do blocking disk or network I/O, so
  on a UI thread prefer the `*Async` overloads where they exist, or do the work on a background thread.
- **`SoundFontSynthesizer` and `SfzSynthesizer` are not thread-safe by design** - rendering and note
  events must not overlap.

## Samples and tools in the repository

The repository ships one package and has no sample applications and no demo projects. Everything below
is developer tooling and test data: none of it is packed, and none of it is needed to consume the
package. Nothing in `tools/` installs anything on your machine - every script checks for what it needs,
names anything missing, prints the command that would install it, and stops.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Native library build scripts | Builds the native audio backend for every shipped runtime identifier, in containers on Linux | [`tools/build_native_libraries`](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tools/build_native_libraries) |
| Test fixture generator | Regenerates the synthesized audio and SoundFont fixtures the tests run against | [`tools/make_test_fixtures`](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tools/make_test_fixtures) |
| SFZ opcode survey | Decides the scope of SFZ support by counting rather than guessing, parsing a folder of real SFZ libraries with the library's own parser | [`tools/sfz_opcode_survey`](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tools/sfz_opcode_survey) |
| Codec sources | Worked examples of the decoder and factory shapes an add-on package implements | [`src/CodeBrix.Audio/Codecs`](https://github.com/ellisnet/CodeBrix.Audio/tree/main/src/CodeBrix.Audio/Codecs) |

The test suites are the best worked examples of every public API. Run them with
`dotnet test CodeBrix.Audio.slnx`. Tests that make sound are opt-in, so an ordinary run is silent and
headless-safe:

```bash
CODEBRIX_AUDIO_RUN_PLAYBACK_TESTS=1 dotnet test
CODEBRIX_AUDIO_ENGINE_RUN_PLAYBACK_TESTS=1 dotnet test
```

Test audio is synthesized in the repository - sine tones, sweeps, noise and silence - and the SoundFont
fixture is a minimal, fully synthetic file built from sine tones. No third-party audio, no real
SoundFont and no SFZ library is committed anywhere in the repository.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Audio/blob/main/README.md) |
| Complete API guide, covering both assemblies (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/AGENT-README.txt) |
| Tools and other non-package content | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.Audio.Tests](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tests/CodeBrix.Audio.Tests) |
| Engine tests | [tests/CodeBrix.Audio.Engine.Tests](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tests/CodeBrix.Audio.Engine.Tests) |

XML documentation ships alongside both assemblies, and one `AGENT-README.txt` covers both, because one
package ships both.

## License

CodeBrix.Audio is licensed under the MIT License, and the license is also named in the package ID
(`CodeBrix.Audio.MitLicenseForever`). License acceptance is required at install time, and the license
notice for the bundled native backend travels with the native binaries into your application's output
folder. For the provenance and licensing of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Audio.Opus](CodeBrix.Audio.Opus.md) - adds Ogg Opus decoding and encoding with one call
- [CodeBrix.Platform.GameEngine](CodeBrix.Platform.GameEngine.md) - builds its audio, music and sound-effect systems on this library
- [AudioPlayer add-in](../platform/add-ins/AudioPlayer.md) - playing audio from a CodeBrix.Platform application
- [ellisnet/CodeBrix.Audio on GitHub](https://github.com/ellisnet/CodeBrix.Audio) - source, tests and tools
