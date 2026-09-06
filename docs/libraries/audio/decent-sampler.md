<sub>[CodeBrix](../../../README.md) › [Libraries](../README.md) › [CodeBrix.Audio](../CodeBrix.Audio.md) › Decent Sampler</sub>

# Decent Sampler instruments

**A Decent Sampler instrument is an XML preset plus the samples beside it, and
[CodeBrix.Audio](../CodeBrix.Audio.md) reads that format and plays it: the whole sampler, the
parameter and binding model behind a preset's knobs, its effect chains and buses, its modulators, its
MIDI handlers, its note sequences and its arpeggiator.** "Decent Sampler" is Decidedly LLC's name for
the format and its player, and appears here only to say what the files are. Sample libraries stream
from disk under a memory budget, archives are read in place rather than unpacked, and anything the
engine could not honor is reported per instrument instead of thrown.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Audio](https://github.com/ellisnet/CodeBrix.Audio) |
| **Packages** | [`CodeBrix.Audio.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MitLicenseForever)<br>`CodeBrix.Audio.ModestSynth.MitLicenseForever` for oscillators and creative effects |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |

## What it does

- Loads a `.dspreset`, a `.dslibrary`, a `.dsbundle` or the folder holding a preset, reading an
  archive in place rather than unpacking it.
- Plays the whole sampler: trigger modes, round robins, tags with polyphony and voice muting, legato
  and glide, loops with crossfades, envelopes with curves, CC filters, release triggers, note delays
  and retriggers.
- Turns a preset's `<ui>` section into a **live** control surface, so writing a control fires the same
  bindings a user turning the knob would.
- Runs effect chains in all three places the format allows - per sounding voice under a group, once
  per block on a bus, and once per block on the finished mix - across sixteen buses and sixteen
  auxiliary stereo pairs.
- Runs the seven modulator kinds with four behaviors and two scopes, all of them bindable themselves.
- Runs the `<midi>` element with key switches, note sequences and the arpeggiator.
- Reads an MPE performance out of a MIDI file, on the same surface as the other two engines.
- Decides per sample file whether to hold it in memory or stream it from disk, under a tunable budget.
- Reports what could not be honored (`Problems`, `UnsupportedFeatures`) and what the engine does not
  implement at all (`DecentSamplerSupportedFeatures`, `DecentSamplerResidualTable`).
- Parses a preset into a typed document without opening a single audio file, for tooling.

## When to use it

Use this format when you have Decent Sampler libraries to play, or when you want a sampled instrument
whose interface parameters are addressable from code: no other format in the package carries a live
parameter model like it. `DecentSamplerSynthesizer` sits on the same `IMidiSynthesizer` contract as
[SoundFont and SFZ](soundfont-and-sfz.md), so `MidiSequencer`, `MidiMusicPlayer`,
[`MultiTrackPlayer`](multi-track-and-suno.md) and `SoundFontRenderer` all take it, and
`MidiMusicPlayer.Load(path, midi)` picks the synthesizer by extension.

What it is not: there is no instrument **user interface** here. A preset's `<ui>` section is parsed
completely and becomes a live control model - every knob, button, menu, pad, label, image and color is
readable and writable, and writing one drives the sound exactly as a user would - but nothing here
draws it. There is also no store, no licensing and no copy protection: a library's `DSLibraryInfo`
`productId` marks it as distributed through a store, and it is reported and never acted on.

## Getting started

```bash
dotnet add package CodeBrix.Audio.MitLicenseForever
```

The surface spans a root namespace and five more, one per job. Type names all begin "DecentSampler",
so nothing collides:

```csharp
using CodeBrix.Audio.Synth.DecentSampler;             // the instrument and the synthesizer
using CodeBrix.Audio.Synth.DecentSampler.Containers;  // DecentSamplerContainer, DecentSamplerLibraryInfo
using CodeBrix.Audio.Synth.DecentSampler.Engine;      // the registration seam, IVoiceSource, IInstrumentEffect
using CodeBrix.Audio.Synth.DecentSampler.Streaming;   // DecentSamplerStreamingMode
using CodeBrix.Audio.Synth.DecentSampler.Effects;     // DecentSamplerCoreEffects
using CodeBrix.Audio.Synth.DecentSampler.Model;       // the typed document, for tooling
```

Four lines play an instrument, and everything after this section is the detail behind them:

```csharp
using CodeBrix.Audio.Synth.DecentSampler;

using var instrument = DecentSamplerInstrument.Load(@"D:\Libraries\Choir.dslibrary");
using var player = new MidiMusicPlayer();
player.Load(instrument, new MidiSequence("song.mid"));
player.Play();
```

`Load(instrumentPath, midiFilePath)` dispatches on the extension, so a `.dspreset`, a `.dslibrary`, a
`.dsbundle` and a folder join `.sf2` and `.sfz` with no other change; the path form goes through
`MidiMusicPlayer.SharedDecentSamplerCache`.

## Key concepts

### What you can hand to Load

| Form | What happens |
| --- | --- |
| `.dspreset` | The preset file itself. `Samples/` and `Resources/` sit beside it |
| A folder | Holding a `.dspreset`. The first one found is used |
| `.dslibrary` | A zip container, read **in place** - nothing is ever unpacked to disk |
| `.dsbundle` | The same, and the macOS folder form of it also works |

A container may hold several presets. `DecentSamplerContainer.Open(path).FindPresets()` lists them,
and `DecentSamplerLoadOptions.PresetName` picks one:

```csharp
using CodeBrix.Audio.Synth.DecentSampler.Containers;   // the container lives here

using var container = DecentSamplerContainer.Open(path);
foreach (var entry in container.FindPresets())
{
    Console.WriteLine(Path.GetFileNameWithoutExtension(entry));
}

using var chosen = DecentSamplerInstrument.Load(
    path, new DecentSamplerLoadOptions { PresetName = "Cantores Oohs" });
```

A library that ships a `DSLibraryInfo.xml` beside or above its preset gets it read for free:
`instrument.LibraryInfo` carries the library's name, version, cover art and preset menu.

`instrument.Container` is the container the preset came from, so a host can resolve and open a file
the preset names beside itself - an image, a text file - exactly the way the engine resolves a sample.
`TryResolve` turns a preset-relative path into a key, where capitalization need not match the disk and
either slash works, and `OpenFile` reads it. The instrument **owns** the container; do not dispose it.

### The instrument, and sharing it

`DecentSamplerInstrument` is `IDisposable` and owns its decoded audio. Loading the same library twice
decodes it twice, which for a sampled library is the expensive thing in the whole system - so share
instruments through a cache:

```csharp
var cache = new DecentSamplerInstrumentCache();
var choir = cache.Get(@"D:\Libraries\Choir.dslibrary");
```

A cache shares **sample data** between the instruments it holds, so a library whose presets differ only
in their interface costs its recordings once however many of its presets are open.
`SharedSampleByteCount` and `SharedSampleCount` say what that comes to; `Clear()` keeps the shared
audio and `Dispose()` releases it. `MidiMusicPlayer.SharedDecentSamplerCache` is the process-wide cache
behind the path form of `Load`, exposed so an application can pre-load a library or clear it - the
player never disposes it.

> [!IMPORTANT]
> One instrument per independent performance. A preset's knob positions, controller values and
> modulated parameters are **instrument** state, because that is where the format puts them: a binding
> writes the group's volume, not the synthesizer's. Two synthesizers over one instrument therefore
> share every knob and every live parameter. That is exactly right for sharing one loaded library
> across several players of the same sound, and wrong when two parts must move their own knobs - give
> each its own instrument then.

The parsed model is `Groups`, `Zones`, `Effects`, `Buses`, `MidiHandlers`, `Modulators`, `Sequences`,
`Arpeggiator`, `Tags` and `Ui`; the live model is `Controls` and `TagStates`; `Problems` and
`UnsupportedFeatures` say what could not be honored; and `MemoryPolicySummary`, `DecodedByteCount` and
`StreamedSampleCount` say what it costs.

### Note names: C3 is 60, which is not what SFZ says

A Decent Sampler preset writes note names in the Yamaha convention: `midi = (octave + 2) * 12 + pitch
class`. So `rootNote="C3"` is MIDI 60, `"C4"` is 72 and `"A4"` is 81. The SFZ engine in this same
package uses scientific pitch notation, where C4 is 60. The two parsers are deliberately separate, and
a bare integer is a MIDI number in both.

### What could not be honored

Loading a preset throws only for XML that is not well formed - `DecentSamplerParseException`, with the
line and column. Everything else is reported and the instrument still plays:

| Member | What it holds |
| --- | --- |
| `instrument.Problems` | One human-readable line per thing that could not be honored, in the order it was found |
| `instrument.UnsupportedFeatures` | The **names** of what the engine did not understand - an element, an attribute, an effect type, a waveform - sorted, each once |

Unknown attributes and elements are also kept verbatim on the model, so nothing in a preset is lost
even when nothing here reads it. `Problems` can gain lines **after** loading - a streaming underrun, a
sample decoded on first use - so read the property rather than caching the list.

The engine falls back the way the reference player does, on purpose. An unreadable attribute value
uses the documented default and says so. A missing sample leaves its zone in the model and silent. An
unknown effect type is bypassed, not fatal. An unknown oscillator waveform sounds a sine when the
add-on is registered, and is silent without it. A round-robin position with no zone plays nothing.
Real libraries carry real typos - a reverb written `damping="O.2"` with a capital letter O turns up in
several separate libraries - and each is reported rather than worked around.

### The control model: a preset's knobs

A preset's `<ui>` section is parsed into a live control surface. Nothing here draws an interface, but
every control is real: its value drives the instrument's initial state at load, and writing it fires
the same bindings a user turning the knob would.

```csharp
foreach (var control in instrument.Controls)
{
    Console.WriteLine($"{control.Index} {control.Kind} {control.Name} = {control.Value}");
}

var attack = instrument.GetControl("ATTACK");
attack.SetValue(0.4);                       // fires its bindings at once
attack.Changed += (_, e) => Console.WriteLine(e.PropertyName);
```

`Controls` is **every** element under every tab, in the order a binding's `controlIndex` counts them -
labels, images and rectangles included. Do not filter it. `GetControl(name)` finds the first by name,
case-insensitively.

| Call | For |
| --- | --- |
| `SetValue(double)` | A knob or a slider |
| `SetXValue` / `SetYValue` | An X-Y pad's axes, 0 to 1 |
| `Select(int)` / `Select(string)` | A button state, a multi-state, or a menu option. The index is 0-based for every kind |

A control also exposes what a renderer would need - `Visible`, `Enabled`, `Text`, `Path`, `Opacity`,
`CurrentFrame`, the colors, the geometry - and a binding moves all of it. Reading and writing those
changes nothing about the sound.

To notice a change without allocating, `instrument.ParameterChanged` fires off the audio thread with
the target, the parameter and the new value, and `instrument.ParameterVersion` is an integer that
counts every change: read it once a block and compare with the last value you saw.

### Tags

`instrument.TagStates` has one entry per tag named anywhere in the preset, each with `Enabled`,
`Volume`, `Pan` and `Polyphony`. `TAG_ENABLED` switches a whole layer - its zones and the effects
carrying that tag - off together; `TAG_VOLUME` scales it; `TAG_POLYPHONY` caps how many of its voices
may sound, oldest first. Several tag volumes on one zone multiply. Tag pan is parsed and exposed, and
moves no voice.

### Effects, buses and auxiliary outputs

A preset can put an effect chain in three places, and each is honored:

| Where | How often it runs |
| --- | --- |
| `<effects>` under a `<group>` | **One instance per sounding voice**. That is what the format says and what the reference player does, and it is the expensive one: a chain with a reverb in it costs a reverb per voice. Chains are pooled and reset rather than rebuilt |
| `<effects>` under a `<bus>` | Once per block, on the bus's own mix |
| `<effects>` at the top level | Once per block, on the finished main mix |

This package carries the mixing and room effects: `lowpass` (and its legacy spelling `lowpass_4pl`),
`lowpass_1pl`, `bandpass`, `highpass`, `notch`, `peak`, `gain`, `reverb`, `delay`, `chorus`,
`convolution` and the compressor. The creative ones - `phaser`, `pitch_shift`, `wave_folder`,
`wave_shaper`, `stereo_simulator`, `bit_crusher` and `gate` - come from
[CodeBrix.Audio.ModestSynth](../CodeBrix.Audio.ModestSynth.md). An effect type nothing supplies is
bypassed and named in `Problems`; the preset still plays.

Effect parameters are live: the parsed `<effect>` element is the single source of truth, and a knob, a
MIDI CC or a modulator reaches the audio within one block. An effect whose tag is disabled is bypassed
while that tag is off.

There are up to sixteen buses, each with its own chain, its own volume and up to eight sends. A bus
may feed another bus, and they are processed in an order that makes every feed arrive before the bus
that receives it, whatever order they are declared in. A routing cycle drops the send that closes it
and says so; a send to a bus the preset never declares is folded into the main output and said so.

A preset may also route a group or a bus to `AUX_STEREO_OUTPUT_1` through `AUX_STEREO_OUTPUT_16` - a
wet layer, a mic position, a stem. By default they are folded into the stereo mix, so nothing an
instrument makes is lost:

```csharp
player.DropAuxiliaryOutputs = true;   // hear only the main output instead
```

To take them separately, the synthesizer implements `IMultiOutputRenderer` (in
`CodeBrix.Audio.Synth`). The main output is a pair of spans; the auxiliary channels are one buffer per
pair, so both auxiliary arguments are `float[][]`:

```csharp
var renderer = (IMultiOutputRenderer)synthesizer;
var pairs = renderer.AuxiliaryOutputCount;      // the HIGHEST pair named

var auxLeft = new float[pairs][];
var auxRight = new float[pairs][];
for (int i = 0; i < pairs; i++)
{
    auxLeft[i] = new float[left.Length];        // every buffer the same length
    auxRight[i] = new float[left.Length];       //   as the main output's
}

renderer.RenderWithAuxiliary(left, right, auxLeft, auxRight);
```

A shorter array discards the pairs beyond it and null discards every pair. `RenderWithAuxiliary` never
folds, whatever the folding setting says: asking for the pairs separately is the whole point of
calling it.

### Modulators

A preset's `<modulators>` run whenever the instrument does: an LFO, an envelope, a MIDI CC, note
velocity, MPE timbre, MPE pressure and a random source, each with its own bindings, its own depth and
one of four behaviors - set, add, modulate, multiply. Scope is global (one value for the instrument)
or voice (its own value per note); the defaults differ per element, as the format documents them, with
`lfo`, `midiCC` and `random` global and `envelope`, `midiVelocity`, `mpeTimbre` and `mpePressure` per
voice.

A voice-scope modulator reaches anything read **inside** that voice's render - its gain, its pan, its
tuning, its own group effect chain. Something read after every voice has rendered, such as the
instrument's own effect chain, sees the global value.

An `<envelope>` modulator is a four-stage ADSR in seconds whose `sustain` is a linear amplitude and
whose release runs for exactly its own time and ends at zero. With no curve attributes written, every
stage follows the shape the reference uses; unlike the reference, this engine also honors
`attackCurve`, `decayCurve` and `releaseCurve` when a preset writes them.

A modulator's own parameters are themselves bindable, so a knob can move a running LFO's rate or an
envelope's attack. Every random draw is seeded, so a modulated preset renders the same bytes from the
same events, and `settings.EnableModulators = false` plays the preset as the sampler alone.

### The MIDI element and key switches

A preset's `<midi>` section listens for controllers, notes and velocity, and runs its bindings
**before** the note is played. It is live:

| Element | Behavior |
| --- | --- |
| `<cc number="1">` | Fires on a **change** of that controller's value only, and every controller starts at 0, so a first message of 0 does nothing. Not channel-specific |
| `<note note="24-35" swallowNotes="true">` | A **key switch**: the note works the bindings and is then consumed. No voice starts, the key is not counted as held, and nothing downstream ever sees it |
| `<velocity>` | Fires on every note-on with the velocity as its source |

Send controllers through the player or the synthesizer as usual:

```csharp
player.SendMidiMessage(0, 0xB0, 1, 96);     // CC 1 -> the preset's knob
synthesizer.ProcessMidiMessage(0, 0xB0, 1, 96);
```

### Note sequences and the arpeggiator

Both generate notes into the same note path a MIDI file's notes take, so they work a key switch and
reach every group.

A `<noteSequences>` sequence is started by a binding with no `parameter` - an **action** rather than a
value. `seqTriggerBehavior` decides what starts it: `midi_key` (start on the key down, stop on the key
up, the default), `on`, or `off`. A player is tracked under `seqPlayerIdentifier`, or one per (channel,
key) when the binding names none, so a handler covering a range of key switches runs a different
sequence for every key. Speed is the sequence's own rate against the synthesizer's `TempoSource`, read
every block, so a rate binding changes the speed of a sequence that is already running.

Three things about a sequence's timing surprise people, all of them the reference player's own
behavior:

- the **first note sounds the instant the key goes down**, whatever beat it is written at, and the
  rest of the grid moves with it. A sequence whose first note is at beat 2 does not wait two beats.
- a note's `position` and its `length` are **truncated to whole beats**. A sequence written with
  fractional positions plays chords, not a rhythm, and a length of 0 means one step. The sequence's
  declared `length` truncates it too: a note written at or past it never plays.
- lifting the key **cuts** the note the sequence is sounding, even one written longer than the key was
  held, and schedules no further step.

The `<arpeggiator>` is a singleton and, when enabled, **consumes** every note played and emits its own
instead - it takes over rather than layering. Its nine orders, its octave range and mode, its step
count, its gate length and its clock are all live and modulatable, and a change is heard on the next
step. An MPE member channel travels with the notes it generates, so a bend or a slide still reaches
them.

An `ALL_NOTES_OFF` binding releases every voice, stops every sequence player and empties the
arpeggiator.

### Memory, streaming and lazy loading

Sample libraries are large. A preset's samples decode to 32-bit float, and a 96 kHz stereo library
reaches gigabytes, so the loader decides per **file** whether to hold it in memory or stream it from
disk:

| `playbackMode` | What happens |
| --- | --- |
| `"memory"` | Held, whatever its size |
| `"disk_streaming"` | Streamed, however small |
| `"auto"` (the default) | Streamed when its decoded size passes `StreamingSampleThresholdBytes`, which defaults to `8 MB` |

Then, if what is left still passes `InstrumentMemoryBudgetBytes`, which defaults to `1 GB`, the
`"auto"` files that save the most stream until it fits, largest saving first. Both numbers are on
`DecentSamplerLoadOptions`, both are tunable, and 0 turns a rule off. `PlaybackModeOverride` replaces
every `playbackMode` in the preset before any of it runs.

`instrument.MemoryPolicySummary` is one line saying what was decided, `DecodedByteCount` is what the
instrument holds (a streamed sample counts only its preload head), and `StreamedSampleCount` says how
many files stream. When the budget forced files to stream that the format's own default would have
kept in memory, that summary is also added to `Problems` - something the preset asked for was not done
as written.

A streamed file keeps a **preload head** in memory - its first `StreamingPreloadFrames` frames - so a
note starting at the beginning of the file sounds without waiting for a disk read. A folder library
streams strictly better than the same library inside a `.dslibrary`, because a backward seek inside a
zip entry costs a walk from the start of it.

`DecodeSamples = false` means **load on first use**, not "never": every path is resolved and every
problem is still found, the whole model is built, and no audio file is opened until a note wants one.
That is what a library browser or a validity check wants. The first note that needs a file is silent
and the instrument says so in `Problems`; every note after it sounds.

`SAMPLE_START`, `SAMPLE_END`, `LOOP_START` and `LOOP_END` take effect on the **next note**. A binding
that moves one of them changes what the next note-on reads; a voice already sounding keeps the bounds
it started with, so dragging the knob under a held chord is silent until the next note.

> [!WARNING]
> Streaming mode is the one setting an offline render must get right.
> `DecentSamplerSynthesizerSettings.StreamingMode` decides who reads a streamed sample off the disk.
> `RealTime`, the default, fills the voices' buffers from a background reader thread, so the render
> call never touches a file - the only safe choice when the render call is a real audio callback.
> `Offline` makes the render call fill its own buffers first, so a streamed voice can never fall
> behind however fast the renderer runs. A render that runs faster than real time outruns the
> background reader, and a starved block is written as silence and reported, so a non-callback render
> in the real-time mode is not reproducible.

```csharp
using CodeBrix.Audio.Synth.DecentSampler;
using CodeBrix.Audio.Synth.DecentSampler.Streaming;

var settings = new DecentSamplerSynthesizerSettings(48000)
{
    StreamingMode = DecentSamplerStreamingMode.Offline,
};
```

`SoundFontRenderer` does it for you - every render it makes is offline, and it switches a synthesizer
you hand it over for the render and back afterwards. `DecentSamplerSynthesizer.StreamingMode` is
settable, so a synthesizer built for a device can be borrowed for a bounce. Never leave `Offline` on a
synthesizer feeding a live device: the render call then opens and reads files.

### The add-on: oscillators and creative effects

A group can hold an `<oscillator>` instead of a `<sample>`, and a chain can name an effect this
package does not carry. Both come from a second package,
[CodeBrix.Audio.ModestSynth](../CodeBrix.Audio.ModestSynth.md):

```bash
dotnet add package CodeBrix.Audio.ModestSynth.MitLicenseForever
```

```csharp
using CodeBrix.Audio.ModestSynth;

ModestSynth.Register();                    // ONCE, BEFORE you load anything
using var instrument = DecentSamplerInstrument.Load(path);
```

> [!WARNING]
> Register before loading. Factories are looked up when an instrument is **built**, not when its file
> is parsed, so a late registration does not retrofit an instrument that is already loaded - or a
> synthesizer already built over one. Nothing throws; the oscillator group falls silent and the
> effect bypassed. Reload the instrument if you registered too late.

Without the add-on a preset still loads and every sample group still plays. `Problems` then carries one
line per missing feature, naming the package and the call, and `UnsupportedFeatures` lists the names,
so a host can decide whether to warn.

A host that wants its own oscillator or effect registers it the same way:
`DecentSamplerExtensions.RegisterOscillator(name, factory)` and `RegisterEffect(type, factory)`,
against `IVoiceSource` and `IInstrumentEffect`.

### What is supported, and what is not

`DecentSamplerSupportedFeatures` is the single source of truth: every element, attribute, binding type,
binding level, binding parameter, translation mode, effect type, modulator type, waveform and
enumeration value the format documents, each marked `Parsed` or `Implemented`.

```csharp
DecentSamplerSupportedFeatures.IsAttribute("group", "silencingMode");
DecentSamplerSupportedFeatures.StatusOf(
    DecentSamplerFeatureCategory.EffectType, string.Empty, "reverb");
```

Registering the add-on changes what that list answers, because `StatusOf` has an overload taking a
registry:

```csharp
DecentSamplerSupportedFeatures.StatusOf(
    DecentSamplerFeatureCategory.Waveform, string.Empty, "fm6op",
    DecentSamplerExtensions.Shared);      // Implemented once registered
```

Everything not implemented is grouped and explained in the residual table, which is derived from the
feature list rather than written by hand, so it cannot drift:

```csharp
Console.WriteLine(DecentSamplerResidualTable.Describe());

foreach (var entry in DecentSamplerResidualTable.Entries)
{
    Console.WriteLine($"{entry.Name}: {entry.Features.Count}");
}
```

The residual table, in one line each:

| Group | Why it is there |
| --- | --- |
| User interface appearance | The `<ui>` section becomes a live control model and nothing here draws it, so positions, colors, images, fonts, skins, tooltips and frame animations are stored rather than rendered |
| Sound generation and creative effects | The add-on package supplies them; the entry's `IsSuppliedByAddOn` is true and one call turns them all on |
| Library catalog and store distribution | `DSLibraryInfo` is read and exposed; `productId` is reported and never acted on |
| Format version negotiation | `minVersion` and `pluginVersion` are read, and reported when a preset asks for more than this engine documents |
| Sample attributes an oscillator zone inherits | The start and end offsets, the loop points and `playbackMode` have nothing to act on when a zone generates its audio instead of reading it |
| The filter envelope | A filter's five `envelope_*` attributes are parsed and kept; the reference player has no audible consumer for them |
| Effect spellings the guide's tables never define | `shape` and `wetDryMix` are recognized so a preset carrying one is not called broken, and are not read |
| Tag pan | Parsed and exposed on the tag state; no voice is moved by it |
| Editor bookkeeping | A `<sample>`'s `length` is what the editor recorded; the engine reads the real length from the file |

One waveform is outside the list entirely: a newer version of the reference player adds a `formant`
oscillator that the published format guide does not document. A preset naming it loads and reports the
waveform as unrecognized, and - with the add-on registered - sounds the fixed tone the reference makes
for it. It has **no** attributes; the reference ignores every `formant-*` attribute written beside it,
and so does this engine, which reports each one.

### Where this engine and the reference player differ

The format has no specification: the reference player defines the sound, and this engine was measured
against it rather than guessed at. Whole presets were compared against recordings of the reference
player bar by bar, and a straight sampled instrument lands within a tenth of a decibel of its absolute
level and under a decibel on every bar, which is what most libraries are made of. These are the
differences that remain, published rather than hidden:

| Case | The difference |
| --- | --- |
| Undeclared bus | A group sent to a bus the preset never declares is silent in both, and this engine also reports it in `Problems`. So is a bus whose own output target names another bus: the format does not let a bus feed a bus, and the reference says nothing about it |
| `<velocity>` bindings | They reach the group their `groupIndex` names, and reach a group's or the instrument's `AMP_VOLUME`. The reference applies such a binding to every group whatever its `groupIndex` says, and does nothing at all on `AMP_VOLUME`. Both are reference defects, and doing less was not worth reproducing |
| `midiCC` scope | A voice-scope `<midiCC>` modulator reads its controller here; the reference reads zero for one, so a preset written the documented way does nothing there and works here |
| `GLOBAL_TUNING` | A modulator binding at `level="instrument"` moves the tuning here; the reference ignores it and leaves the pitch alone |
| Continuous zones | `trigger="continuous"` sounds here, and with no loop points of its own it loops the whole file. The reference produced no sound at all for such a zone |
| Retrigger | The interval is exact here; the reference rounds it up to a whole 512-frame processing block, which stretches a one-second retrigger |
| `no_loop` | A note sequence set to `no_loop` stops after one pass here, at every declared length. The reference fails to stop one whose declared length is 2, which is a defect and is deliberately not reproduced |
| Random sequences | They draw from every note. Both of the reference's random loop modes measurably never draw one note of a four-note sequence, which is likewise a defect and deliberately not reproduced |
| Envelope curves | `attackCurve`, `decayCurve` and `releaseCurve` on an `<envelope>` **modulator** bend its stages here. The reference accepts and ignores all three, even though the group amplitude envelope's own curves do work there. The default shape is the reference's either way |
| First sample frame | A sample whose very first frame carries the signal sounds here; the reference ramps a voice in over its first frames and loses it |
| Noise | The anti-imaging rolloff above 8 kHz is reproduced, and the power centroid lands slightly darker than the reference's - which is as close as the reference's own two figures allow |
| Formant | The fixed tone's spectrum is reproduced closely; the phase of each partial could not be measured, so the waveform's shape is this engine's own |
| `fm6op` detune | It follows the measured power law over the range it was measured across; outside that range it is an extrapolation |
| Heavily layered presets | A preset built almost entirely of tag-gated layers measured about a decibel louder than the reference over eight bars. Every individual rule the preset uses was measured and matches on its own, so what is left is a level relationship between one family of layers and the rest. A master gain closes it if a render has to match the reference exactly |

None of these is silent: each is either audible in a way the table describes or reported in
`Problems`. Most of them are places where the reference does *less* than the format documents and this
engine does what the documentation says.

## Examples

Open a library, list its presets, load one, set a knob, play a MIDI file, and export the same
performance to a WAV file:

```csharp
using CodeBrix.Audio.Midi;
using CodeBrix.Audio.ModestSynth;
using CodeBrix.Audio.Playback;
using CodeBrix.Audio.Synth;
using CodeBrix.Audio.Synth.DecentSampler;
using CodeBrix.Audio.Synth.DecentSampler.Containers;
using CodeBrix.Audio.Synth.Mpe;

// The add-on, once, before anything is loaded. Skip it if you know the
// libraries you play hold no oscillators and no creative effects.
ModestSynth.Register();

const string libraryPath = @"D:\Libraries\Winter Voices.dslibrary";

// What is in it?
using (var container = DecentSamplerContainer.Open(libraryPath))
{
    foreach (var entry in container.FindPresets())
    {
        Console.WriteLine(Path.GetFileNameWithoutExtension(entry));
    }
}

// Load one, under a memory budget of your choosing.
using var instrument = DecentSamplerInstrument.Load(libraryPath,
    new DecentSamplerLoadOptions
    {
        PresetName = "WV NATURAL TONES",
        InstrumentMemoryBudgetBytes = 512L * 1024 * 1024,
    });

Console.WriteLine(instrument.MemoryPolicySummary);

foreach (var problem in instrument.Problems)
{
    Console.WriteLine(problem);            // never thrown; often empty
}

foreach (var name in instrument.UnsupportedFeatures)
{
    Console.WriteLine("not understood: " + name);
}

// Turn a knob the preset offers, exactly as a user would.
var attack = instrument.GetControl("ATTACK");
attack?.SetValue(0.35);

// Play it.
var sequence = new MidiSequence("song.mid");

using (var player = new MidiMusicPlayer())
{
    player.MpeMode = MpeMode.Auto;         // the file may be an MPE clip
    player.Volume = 0.8f;
    player.Load(instrument, sequence);
    player.Play();

    Console.ReadLine();
}

// Export the same performance. The renderer runs offline, so it puts the
// instrument's streamed samples into the offline mode for the render.
SoundFontRenderer.RenderToWavFile(
    instrument, sequence, "bounce.wav", 48000, TimeSpan.FromSeconds(3));
```

Loading under a tighter budget, and reading back what the memory policy decided:

```csharp
using var big = DecentSamplerInstrument.Load(path, new DecentSamplerLoadOptions
{
    InstrumentMemoryBudgetBytes = 512L * 1024 * 1024,
    StreamingSampleThresholdBytes = 4L * 1024 * 1024,
});

Console.WriteLine(big.MemoryPolicySummary);
```

Driving the synthesizer yourself, without a transport:

```csharp
var synthesizer = new DecentSamplerSynthesizer(instrument, settings);
synthesizer.NoteOn(0, 60, 100);
synthesizer.Render(left, right);
```

A synthesizer never disposes its instrument, and is not thread-safe. The defaults worth knowing:
192-voice polyphony, because a preset routinely layers a dozen groups; 64-frame blocks; a master
volume of 0.5; and a seeded random stream, so the same instrument and the same events render the same
bytes every run.

## Pitfalls

- **Register the add-on before you load.** A preset whose group holds an `<oscillator>`, or whose
  chain names one of the creative effects, needs
  [CodeBrix.Audio.ModestSynth](../CodeBrix.Audio.ModestSynth.md): reference it and call
  `ModestSynth.Register()` *before* `DecentSamplerInstrument.Load`. Factories are resolved when an
  instrument is built, so registering afterwards does not retrofit one that is already loaded, or a
  synthesizer already built over it. Nothing throws: the oscillator group is silent, the effect is
  bypassed, and `Problems` names the package and the call.
- **Note names are Yamaha, not scientific.** `rootNote="C3"` is MIDI 60 here, where the SFZ engine in
  this same package reads C4 as 60.
- **An offline render of an instrument that streams its samples must use
  `DecentSamplerStreamingMode.Offline`.** `SoundFontRenderer` sets it for you, including on a
  synthesizer you hand it; a render loop of your own has to say so. Never set it on a synthesizer
  feeding a live device.
- **One instrument per independent performance.** Two synthesizers over one instrument share every
  knob, because a preset's live parameters belong to the instrument.
- **A group with its own `<effects>` costs one instance per sounding voice,** which is what the format
  specifies. A reverb at group level is a reverb per voice; put it on the instrument or on a bus
  unless the per-voice behavior is what the preset is for.
- **Do not dispose an instrument a synthesizer is still playing,** and do not dispose the `Container`
  it hands you: the instrument owns it and closes it. `MidiMusicPlayer.SharedDecentSamplerCache` holds
  instruments for the life of the process.
- **`Problems` can gain lines after loading** - a streaming underrun, a sample decoded on first use.
  Read the property; do not cache the list you got at load time.
- **A `<cc>` binding fires on a change only,** and every controller starts at 0, so a first message of
  0 does nothing.

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/AGENT-README.txt) |
| The engine's tests, by subject folder | [tests/CodeBrix.Audio.Tests](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tests/CodeBrix.Audio.Tests) |
| The tool that measures format coverage over a corpus of real libraries | [tools/ds_feature_survey](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tools/ds_feature_survey) |

## License

CodeBrix.Audio is licensed under the MIT License, and the license is also named in the package ID
(`CodeBrix.Audio.MitLicenseForever`). The add-on that supplies oscillators and creative effects is MIT
as well. For the provenance and licensing of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Audio.ModestSynth](../CodeBrix.Audio.ModestSynth.md) - the oscillators and creative effects a preset reaches for
- [MPE from MIDI files](mpe.md) - the expression contract this engine shares with the other two
- [SoundFont and SFZ instruments](soundfont-and-sfz.md) - the other two sampled formats
- [CodeBrix.Audio](../CodeBrix.Audio.md) - the front door of the audio section
