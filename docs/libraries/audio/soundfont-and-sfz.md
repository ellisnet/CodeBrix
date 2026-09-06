<sub>[CodeBrix](../../../README.md) › [Libraries](../README.md) › [CodeBrix.Audio](../CodeBrix.Audio.md) › SoundFont and SFZ</sub>

# SoundFont and SFZ instruments

**This page covers two of the three sampled instrument formats
[CodeBrix.Audio](../CodeBrix.Audio.md) plays: SoundFont (`.sf2`) and SFZ (`.sfz`).** It explains which
of the two SoundFont paths in the package you want, what the SFZ engine implements and how to find out
what a library asked for that it does not, and how `MidiMusicPlayer` and `SoundFontRenderer` drive
either format - live through a device, or offline with no device at all. The third format has its own
page, [Decent Sampler instruments](decent-sampler.md).

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Audio](https://github.com/ellisnet/CodeBrix.Audio) |
| **Packages** | [`CodeBrix.Audio.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application, through the [AudioPlayer add-in](../../platform/add-ins/AudioPlayer.md) |

## What it does

- Renders SoundFonts with the full generator **and** modulator model, per-voice LFOs, a per-voice
  lowpass filter, volume and modulation envelopes, reverb and chorus.
- Exposes a parsed `.sf2` as an object model, so you can enumerate presets and key ranges without
  rendering a note.
- Plays SFZ instruments through a managed SFZ engine measured against a corpus of real free
  instruments with no unimplemented opcodes left over.
- Reports what an SFZ library asked for that the engine does not implement, per instrument, instead of
  failing or falling silent.
- Drives either format from one transport-style player, `MidiMusicPlayer`, with the controls a
  sequence needs and a decoded file does not.
- Renders either format offline through `SoundFontRenderer`, faster than real time, with no audio
  device involved.
- Reads the structural layer of an SFZ file for tooling: headers, opcodes, `#define` and `#include`,
  with region-to-group-to-master-to-global inheritance resolved on request.

## When to use it

All three sampled formats implement one contract, `IMidiSynthesizer`, which is why `MidiSequencer`,
`MidiMusicPlayer`, `MultiTrackPlayer` and `SoundFontRenderer` drive any of them without caring which.
Consumers choose a *file format*, not an API:

| The file | The types |
| --- | --- |
| A `.sf2` | `SoundFont` / `SoundFontSynthesizer` |
| A `.sfz` | `SfzInstrument` / `SfzSynthesizer` |
| A `.dspreset`, `.dslibrary` or `.dsbundle` | `DecentSamplerInstrument` / `DecentSamplerSynthesizer` - see [Decent Sampler instruments](decent-sampler.md) |

`MidiMusicPlayer.Load(instrumentPath, midiFilePath)` dispatches on the extension, so switching a
project from one sampled format to another is a change of file name.

### Two SoundFont paths - which one you want

Read this before writing any SoundFont or MIDI-synthesis code. The package contains two things that
can play a SoundFont, and they are not interchangeable. Choosing wrong does not fail loudly - it
produces audio that is subtly wrong.

| Job | Namespace | Types |
| --- | --- | --- |
| To **play** a `.sf2` - the renderer of record | `CodeBrix.Audio.Synth` | `SoundFontSynthesizer`, `MidiSequence`, `MidiSequencer`, `MidiMusicPlayer` |
| To **build** a synthesized instrument - oscillators, custom banks, MIDI modifiers, arpeggiators | `CodeBrix.Audio.Engine.Synthesis` | `Synthesizer`, `Sequencer`, `SoundFontBank`, `MultiInstrumentBank`, the `Generators/` and `Voices/` types |

`CodeBrix.Audio.Synth` is a specification-faithful SF2 renderer: it implements the SoundFont generator
and modulator model, per-voice LFOs, a per-voice lowpass filter, volume and modulation envelopes,
reverb and chorus. That modulator model is central to how a SoundFont is meant to sound.
`CodeBrix.Audio.Engine.Synthesis` is a general-purpose synthesis architecture that can sample-play SF2
presets; it has no modulators, no per-voice LFO and no per-voice filter. It is the better tool for
building instruments, and the wrong tool for faithfully reproducing somebody's `.sf2`. Neither was
retired in favor of the other - see [The bundled audio engine](audio-engine.md).

SFZ and Decent Sampler have exactly one path each, and it is the first one. The Engine has no support
for either, so there is no wrong turn to take.

## Getting started

```bash
dotnet add package CodeBrix.Audio.MitLicenseForever
```

```csharp
using CodeBrix.Audio.Playback;   // MidiMusicPlayer
using CodeBrix.Audio.Synth;      // SoundFont, MidiSequence, SoundFontRenderer
using CodeBrix.Audio.Synth.Sfz;  // SfzInstrument, SfzSynthesizer
```

MIDI music through a SoundFont has the same transport surface as the file player:

```csharp
using CodeBrix.Audio.Playback;
using CodeBrix.Audio.Synth;

// Share one SoundFont across every player: a .sf2 runs to tens of megabytes.
var soundFonts = new SoundFontCache();

var music = new MidiMusicPlayer();
music.Load(soundFonts.Get("GeneralUser.sf2"), new MidiSequence("level1.mid"));
music.IsLooping = true;
music.Play();
// Same surface as AudioFilePlayer: Position, Duration, Seek, Volume, Pause, Stop, Dispose.
```

## Key concepts

### The SoundFont types

`SoundFont` is a parsed `.sf2` with a public object model - `SoundFontInfo`, `Preset`, `PresetRegion`,
`SoundFontInstrument`, `InstrumentRegion`, `SampleHeader` and `LoopMode` - so you can enumerate a
SoundFont's presets and key ranges without rendering.

`SoundFontCache` loads a `.sf2` once and shares it. SoundFonts run to tens of megabytes; never reload
one per track. `SoundFontSynthesizer` is the renderer of record, and is **not** thread-safe by design:
rendering and note events must not overlap. It also reads the MPE content of a performance - see
[MPE from MIDI files](mpe.md).

`MidiSequencer` drives a synthesizer from a sequence, with `Play`, `Stop` and `Seek`.

### The SFZ engine

`SfzInstrument` is a playable SFZ instrument: typed regions, decoded samples (WAV, FLAC and Ogg
through the [reader registry](reading-and-writing-files.md), with wrong-case and backslash paths
resolved), modulation curves, and initial controller state. Loading is tolerant - missing samples land
in `.Problems`, and opcodes the engine does not implement land in `.UnsupportedOpcodes` and in the
debug log, once per name. Samples decode eagerly, so memory follows the library's size.

`SfzInstrumentCache` loads an instrument once and shares it, keyed by path - the `SoundFontCache` of
SFZ. Use it.

`SfzSynthesizer` is a peer of `SoundFontSynthesizer` on the same `IMidiSynthesizer` contract and
equally not thread-safe. It implements the SFZ articulation model:

- region selection by key, velocity, controller and program; round robins and random layers,
  deterministic by seed through `SfzSynthesizerSettings.RandomSeed`;
- key switches including `sw_lolast` / `sw_hilast` ranges and `sw_vel=previous`;
- trigger modes including release samples with `rt_decay`;
- off groups with fast, normal and timed chokes (`off_time`, `off_shape`), `polyphony` and
  `note_polyphony` limits;
- CC-triggered regions, and key, velocity and controller crossfades (`xfin` / `xfout`, gain or
  equal-power law).

Per voice it runs the DAHDSR amplifier envelope with shape curvature, `vel2*` velocity timing and
`ampeg_dynamic` retiming; the filter and pitch envelopes (`fileg`, `pitcheg`); flexible envelopes
(`egN`, including the key-delta portamento idiom); the SFZ v1 `amplfo` / `fillfo` / `pitchlfo` blocks
and the v2 `lfoN` LFOs with sub-waveforms, cross-LFO frequency modulation and EQ routing; two filters
in series; a three-band parametric EQ; ARIA variators (`varNN`); stereo width; region delay and the
delay, offset, amplitude and filter randoms; and the `_onccN` / `_curveccN` / `_smoothccN` modulation
matrix with the ARIA extended sources - pitch bend, aftertouch, velocity, note-off velocity, note, key
gate, per-voice randoms, alternate and key delta.

`SfzRegion` is one region with opcodes resolved and typed and specification defaults filled in, and
the block families come typed too: `SfzEqBand`, `SfzLfo`, `SfzFlexEg`, `SfzModEnvelope` and
`SfzVariator`. `SfzSupportedOpcodes` is the exact implemented set, in canonical index-folded names -
block indices fold too, so `lfo01_freq` and `lfo3_freq` are both `lfoN_freq`.

### The SFZ structural layer

Underneath the playable layer sits a layer for tooling rather than playback. `SfzParser.ParseFile` and
`SfzParser.ParseText` read SFZ structure - headers, opcodes, `#define` and `#include` - into
`SfzFile`, `SfzSection` and `SfzOpcode`. `SfzFile.Resolve(region)` applies region to group to master
to global inheritance:

```csharp
using CodeBrix.Audio.Synth.Sfz;

var sfz = SfzParser.ParseFile("piano.sfz");
foreach (var region in sfz.Regions)
{
    var resolved = sfz.Resolve(region);      // region -> group -> master -> global
    var sample = resolved["sample"].Value;
    var lowKey = resolved.TryGetValue("lokey", out var lo) ? lo.AsNoteNumber() : 0;
}
// sfz.Problems lists anything odd (a missing #include, an opcode outside any header).
// Unknown opcodes are carried, not rejected.
```

Unknown opcodes are carried, never fatal: files routinely carry opcodes meant for other players, and a
file must load with what is understood.

### The transport player

`MidiMusicPlayer` (in `CodeBrix.Audio.Playback`) is shaped exactly like `AudioFilePlayer` - `Load`,
`Play`, `Pause`, `Stop`, `Seek`, `Volume`, `IsLooping`, `Position`, `Duration`, `PlaybackEnded` - plus
the controls a sequence needs that a decoded file does not:

| Member | What it does |
| --- | --- |
| `.Speed` | Tempo multiplier, 1.0 by default; scales tempo without changing pitch |
| `.Sequence` | The loaded `MidiSequence` - the only way to reach it after the two-path `Load` |
| `SendMidiMessage()` | Send alongside the sequence, from any thread, safely |
| `SetChannelVolume()` | Control change 7 - how a layered arrangement is mixed live |
| `SetChannelPan()` | Control change 10 |
| `SetChannelProgram()` | Program change |
| `.MidiMessageProcessed` | The observe-only note hook - see [MIDI files](midi-files.md) |
| `.MidiMessageFilter` | The modifying hook, which replaces delivery |
| `.MpeMode` and the rest of the MPE surface | See [MPE from MIDI files](mpe.md) |

`Load` has overloads taking a `SoundFont`, an `SfzInstrument`, a `DecentSamplerInstrument`, an
`IMidiSynthesizer`, a `Func<int, IMidiSynthesizer>` factory, or two paths.

### Rendering offline

`SoundFontRenderer` renders with no audio device involved, and faster than real time - so bouncing a
sequence to a `.wav` once and playing the `.wav` is cheaper than synthesizing it on every playthrough.
`Render(...)` returns interleaved stereo floats; `RenderToWavFile(...)` and `RenderToWavStream(...)`
write the result out. It takes a `SoundFont`, an `SfzInstrument`, a `DecentSamplerInstrument` or an
`IMidiSynthesizer`.

```csharp
using CodeBrix.Audio.Synth;

var soundFont = new SoundFont("GeneralUser.sf2");
var sequence = new MidiSequence("level1.mid");

SoundFontRenderer.RenderToWavFile(soundFont, sequence, "level1.wav", 44100,
                                 tail: TimeSpan.FromSeconds(2));  // let reverb decay
// Or SoundFontRenderer.Render(...) for interleaved stereo floats in memory.
```

## Examples

Playing an SFZ instrument, and checking first what the library asked for that the engine does not
implement:

```csharp
using CodeBrix.Audio.Playback;
using CodeBrix.Audio.Synth;
using CodeBrix.Audio.Synth.Sfz;

// Samples decode once at load: share one instrument across every player.
var instruments = new SfzInstrumentCache();
var piano = instruments.Get("VirtualPiano.sfz");

// If a library sounds wrong, look here FIRST: these are the opcodes it uses
// that the engine does not implement (canonical names, e.g. "eq1_freq").
foreach (var missing in piano.UnsupportedOpcodes) Console.WriteLine(missing);
foreach (var problem in piano.Problems) Console.WriteLine(problem);

var music = new MidiMusicPlayer();
music.Load(piano, new MidiSequence("song.mid"));
music.Play();
// SoundFontRenderer.Render / RenderToWavFile also accept an SfzInstrument
// for offline bounces, and SfzSynthesizer can be driven directly with
// ProcessMidiMessage / NoteOn / NoteOff / Render for interactive use.
```

The whole application, for MIDI music through a SoundFont, is two files and one package reference -
swap `AudioFilePlayer` for `MidiMusicPlayer` and use the two-argument `Load`:

```csharp
using CodeBrix.Audio.Playback;
using CodeBrix.Audio.Synth;

using var music = new MidiMusicPlayer();
music.PlaybackEnded += (s, e) => finished.Set();
music.Load("GeneralUser.sf2", "level1.mid");   // .sf2 or .sfz, then the .mid
music.Play();
```

And with no player and no device at all:

```csharp
using CodeBrix.Audio.Synth;

SoundFontRenderer.RenderToWavFile(
    new SoundFont("GeneralUser.sf2"),
    new MidiSequence("level1.mid"),
    "level1.wav");
```

## Pitfalls

- **Read the two-SoundFont-paths table before choosing a namespace.** The wrong one produces audio
  that is subtly wrong rather than an error.
- **Share the big assets.** A `.sf2` runs to tens of megabytes and an SFZ library decodes all of its
  samples eagerly at load. `SoundFontCache` and `SfzInstrumentCache` exist precisely so one copy
  serves every player - hold one cache for the application and call `.Get(path)` rather than
  constructing `SoundFont` or `SfzInstrument` yourself.
- **`SoundFontSynthesizer` and `SfzSynthesizer` are not thread-safe by design.** Rendering and note
  events must not overlap.
- **A synthesizer factory must return a new synthesizer every time.** It is called once per track per
  render context and may be called from a worker thread. Share the `SoundFont` or the `SfzInstrument`
  behind them - those are the expensive part and are safe to share.
- **Check `UnsupportedOpcodes` first when an SFZ library sounds wrong.** It names, in canonical form,
  every opcode the file used that the engine does not implement.
- **Rendering offline beats rendering live.** `SoundFontRenderer` runs faster than real time with no
  device involved.

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/AGENT-README.txt) |
| SoundFont, SFZ and MIDI-music tests | [tests/CodeBrix.Audio.Tests](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tests/CodeBrix.Audio.Tests) |
| The tool that measures SFZ opcode coverage over a corpus | [tools/sfz_opcode_survey](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tools/sfz_opcode_survey) |

## License

CodeBrix.Audio is licensed under the MIT License, and the license is also named in the package ID
(`CodeBrix.Audio.MitLicenseForever`). For the provenance and licensing of open source code included in
this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [Decent Sampler instruments](decent-sampler.md) - the third sampled format, and its live parameter model
- [MPE from MIDI files](mpe.md) - one expressive performance through all three engines
- [CodeBrix.Audio.ModestSynth](../CodeBrix.Audio.ModestSynth.md) - oscillators and creative effects, and a synthesizer that plays a patch from MIDI
- [CodeBrix.Audio](../CodeBrix.Audio.md) - the front door of the audio section
