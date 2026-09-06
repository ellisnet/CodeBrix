<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Audio.ModestSynth</sub>

# CodeBrix.Audio.ModestSynth

**CodeBrix.Audio.ModestSynth adds synthesis to [CodeBrix.Audio](CodeBrix.Audio.md): oscillators that
generate sound from a description instead of playing back a recording, and the creative effects a
synth is expected to have.** Band-limited classic waveforms, seeded white noise, a waveguide plucked
string, a multi-frame wavetable, a 64-partial additive oscillator, a six-operator FM engine and a
formant tone, plus a phaser, a pitch shifter, two distortion curves, a stereo widener, a bit crusher
and a stutter gate. Use them on their own, play a whole patch from MIDI through a small polyphonic
synthesizer, or let one call hand them to the
[Decent Sampler engine](audio/decent-sampler.md) for a preset whose group holds an `<oscillator>`
rather than a `<sample>`. It is pure managed code with no native payload of its own.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Audio](https://github.com/ellisnet/CodeBrix.Audio) |
| **Packages** | `CodeBrix.Audio.ModestSynth.MitLicenseForever` |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later, and [`CodeBrix.Audio.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MitLicenseForever), which the package pulls in automatically |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Everywhere CodeBrix.Audio works. Rendering is pure managed code: no P/Invoke, no `runtimes/` folder, nothing to rebuild when a platform is added |

## What it does

- Generates ten waveforms: `sine`, `saw`, `square`, `triangle`, `noise` (also spelled `white_noise`),
  `pluck1`, `wavetable`, `harmonic`, `fm6op` and `formant`.
- Processes seven creative effects: `phaser`, `pitch_shift`, `wave_folder`, `wave_shaper`,
  `stereo_simulator`, `bit_crusher` and `gate`.
- Band-limits what needs it: the classic shapes are corrected at their edges, a wavetable is
  mip-mapped per octave when it loads, and the additive oscillator never sounds a partial that would
  alias.
- Carries `ModestPatch`, a parameter model that mirrors every documented oscillator attribute one for
  one, so a patch written for a sampler `<oscillator>` element plays here unchanged.
- Carries `ModestSynthesizer`, a polyphonic synthesizer that plays a patch from MIDI events with no
  preset file involved, on the same contract as the SoundFont, SFZ and Decent Sampler engines - so
  every player and offline renderer in CodeBrix.Audio takes it.
- Carries `ModestSynthPresets`, six worked example patches with the envelope settings that go with
  them.
- Registers all ten waveforms and all seven effects with the Decent Sampler engine in one call.
- Answers to both the attribute names and the `FX_*` binding names the sampler format uses, matched
  without regard to case or punctuation.
- Allocates nothing while rendering: `Render` and `Process` both lock nothing, open nothing and
  allocate nothing, so a voice or an effect can run straight from an audio callback.

## When to use it

There are three ways to use this package, and they are independent of one another.

| Way | What it looks like |
| --- | --- |
| **One oscillator or one effect** | Create an oscillator, tell it the sample rate and the pitch, and ask it for blocks of samples; or create an effect, prepare it at your sample rate and push blocks through it. Nothing needs registering, nothing needs a file, and CodeBrix.Audio's players and writers take the samples from there |
| **The standalone synthesizer** | `ModestSynthesizer` plays a `ModestPatch` from MIDI events - sixteen channels, velocity, the sustain pedal, pitch bend, voice stealing and an amplitude envelope. No preset file and no registration are involved |
| **Inside a Decent Sampler instrument** | A group can hold an `<oscillator>` element instead of, or as well as, samples, and an effect chain can name a type the core does not carry. One call at start-up hands both sets over |

Reach for it whenever a sound has to be *made* rather than played back: a tone generator, a test
signal, a synthesized layer in a game score, or a sampler library that leans on oscillator groups.
Leave it out when every library you play is sampled - CodeBrix.Audio runs the mixing and room effects
on its own, and a preset that uses only those plays in full without this package.

What it is not: there are no filters, no modulators, no layers and no key or velocity zones in the
standalone synthesizer. Those belong to an instrument format, and a Decent Sampler preset played
through `DecentSamplerSynthesizer` is where they live.

> [!IMPORTANT]
> The two packages move together. `CodeBrix.Audio.ModestSynth.MitLicenseForever` is built and
> published from the same repository as `CodeBrix.Audio.MitLicenseForever`, at the same version, so
> the two always match. It depends on that package and on nothing else, and it is separate because
> synthesis is not something every application that reads a WAV file needs to carry.

## Getting started

```bash
dotnet add package CodeBrix.Audio.ModestSynth.MitLicenseForever
```

The package ID carries the `.MitLicenseForever` suffix; the namespace is `CodeBrix.Audio.ModestSynth`,
with no suffix. The public surface is spread over seven namespaces, and everything else in the
assembly is internal:

```csharp
using CodeBrix.Audio.ModestSynth;              // ModestSynth.Register()
using CodeBrix.Audio.ModestSynth.Oscillators;  // the oscillators themselves
using CodeBrix.Audio.ModestSynth.Fm;           // the six-operator FM engine
using CodeBrix.Audio.ModestSynth.Patch;        // ModestPatch, the FM operator model
using CodeBrix.Audio.ModestSynth.Wavetable;    // WavetableOscillator, WavetableFile
using CodeBrix.Audio.ModestSynth.Harmonic;     // HarmonicOscillator
using CodeBrix.Audio.ModestSynth.Effects;      // the seven creative effects
using CodeBrix.Audio.ModestSynth.Integration;  // ModestVoiceSource, the adapter
```

An effect also needs the contract it implements, which lives in the core:

```csharp
using CodeBrix.Audio.Synth.DecentSampler.Engine;   // IInstrumentEffect
```

The shortest useful thing is one oscillator, rendered into a block:

```csharp
using CodeBrix.Audio.ModestSynth.Oscillators;

var saw = new SawOscillator();
saw.SetSampleRate(48000);
saw.SetFrequency(220.0);      // A3
saw.Reset(0.0);               // note-on

var block = new float[512];
saw.Render(block);            // mono, nominally -1 to 1
```

An oscillator's output is mono float, nominally in the range -1 to 1, because a voice's panning and
width belong to whatever mixes the voices. An effect is stereo and works in place, on a pair of float
buffers, because that is where it sits in the signal path.

> [!NOTE]
> The type `ModestSynth` and the namespace `CodeBrix.Audio.ModestSynth` share a name. That is legal
> and it resolves the way you would want - `ModestSynth.Register()` in a file that has the using
> directive is the static class - but if a compiler ever disagrees with you about it, write
> `CodeBrix.Audio.ModestSynth.ModestSynth.Register()` and move on.

## Key concepts

### Registering with the sampler engine

```csharp
using CodeBrix.Audio.ModestSynth;

ModestSynth.Register();     // once, at application start-up
```

One call registers **both** sets: every waveform this package generates and every creative effect it
supplies. After it, a group holding

```xml
<oscillator waveform="fm6op" fmAlgorithm="5" fmOp2Level="0.7" />
```

sounds, an `<effect type="phaser" />` in any chain processes, and every `OSCILLATOR_*` and `FX_*`
binding in the preset moves what you hear.

> [!WARNING]
> Call it **before** loading an instrument that needs it. Waveforms and effect types are resolved
> while an instrument is being *built*, not while its file is being parsed, so registering later does
> not retrofit an instrument that is already loaded - reload it. Without the call, an oscillator group
> is silent and an add-on effect is bypassed, the rest of the preset plays normally, and the
> synthesizer's `Problems` list carries one line per missing feature naming this package and this
> call. Nothing throws.

| Member | What it is for |
| --- | --- |
| `ModestSynth.Register()` | Idempotent. Calling it twice does nothing the second time, from any thread |
| `ModestSynth.IsRegistered` | Whether it has run |
| `ModestSynth.RegisteredOscillatorWaveforms` | The waveform names that were offered, spelled as a preset spells them, including every accepted synonym. Empty until `Register()` has run |
| `ModestSynth.RegisteredEffectTypes` | The effect type names that were offered - the seven creative ones, never the core's own. Also empty until `Register()` has run |
| `ModestSynth.Register(DecentSamplerExtensionRegistry)` | Fills a registry of your own instead of the process-wide one - the value you would put in `DecentSamplerSynthesizerSettings.Extensions`, or an isolated registry in a test. It leaves `IsRegistered` and `RegisteredEffectTypes` alone, because those describe the process-wide registration |

There is deliberately no module initializer doing this for you. A module initializer only runs once
something in the assembly is touched, which trimming and lazy assembly loading make unreliable: the
package would work in a debug build and silently fail to register in a trimmed publish. The explicit
call is the contract, and it keeps the package free of reflection. The **application** takes the
dependency and makes the call; a library that merely renders audio should not decide this for its
host.

Registration also changes what the engine says about itself:
`DecentSamplerSupportedFeatures.StatusOf` answers `Implemented` for a waveform or an add-on effect
type as soon as a registry has a factory for the name, and `Parsed` while nothing supplies it - so a
host can report honestly whether this package is present.

The standalone API needs none of this. `ModestSynthesizer`, `ModestPatch`, the oscillators and the
effects all work with nothing called.

### What registration gives a zone

`ModestVoiceSource`, in `CodeBrix.Audio.ModestSynth.Integration`, is what the sampler engine builds
per voice. You never construct one unless you are filling a registry by hand, but what it does is
worth knowing.

- It reads **every** oscillator attribute off the resolved zone - waveform, damping, pluck type,
  random phase, the wavetable file, frame size, position and frame interpolation, the partial count,
  tilt, odd/even balance, normalization, all sixty-four partial levels, the FM algorithm and all
  twenty parameters of each of the six operators.
- The zone stays the single source of truth while the note sounds. An `OSCILLATOR_*` binding writes
  the zone; the source reads it back at the top of the next block, so a knob or a modulator moves a
  note that is already playing. The re-read is gated on the instrument's `ParameterVersion`, so a
  block during which nothing moved costs one integer comparison.
- Pitch comes from the engine: the MIDI note in equal temperament with A4 at 440, plus the zone's
  tuning, its glide ramp and the channel's pitch bend, handed over once per block.
- Velocity goes to the oscillator as well as to the amplitude, which is what makes an `fm6op`
  operator's velocity sensitivity work.
- The group's ADSR gates the oscillator, because only `pluck1` and `fm6op` stop by themselves.
- Every oscillator is trimmed by one level constant, `ModestVoiceSource.ReferenceOscillatorGain`,
  which is where the reference player puts an oscillator zone relative to a sample zone. It is one
  trim over all of them, so the measured level *relationships* between the waveforms are the
  oscillators' own.
- A wavetable file is resolved through the preset's own container, so a path works from a folder and
  from inside a `.dslibrary` archive, and its capitalization does not have to match the disk. The
  first voice of a wavetable group reads the file; every voice after it shares the decoded table
  through `WavetableFileCache`.
- Rendering allocates nothing. Building a source, and starting a note on a waveform that source has
  not played before, do allocate; the engine's per-zone voice pooling is what keeps that off the
  steady state.

### The oscillator contracts

`IModestOscillator`, in `CodeBrix.Audio.ModestSynth.Oscillators`, is one voice's worth of
sound-generating state:

```csharp
string Waveform             // the name this oscillator implements, e.g. "pluck1"
int    SampleRate           // the rate blocks are rendered at
double Frequency            // the pitch being generated, in Hz
void   SetSampleRate(int)   // call before rendering; this is where allocation happens
void   SetFrequency(double) // safe between blocks - this is how glide and vibrato work
void   Reset(double phase)  // note-on; phase is in CYCLES, so only its fraction matters
void   Render(Span<float>)  // fills the block, overwriting it
```

The lifecycle is `SetSampleRate`, `SetFrequency`, `Reset`, then `Render` as often as you like.
Retuning never restarts the waveform; `Reset` always does. An oscillator belongs to the thread
rendering it - nothing is thread-safe and nothing needs to be, so give every voice its own.

`IModestVoiceOscillator : IModestOscillator` adds what a waveform played as a **note** needs:

```csharp
bool IsKeyDown              // true between NoteOn and NoteOff
bool IsFinished             // the key is up and nothing audible is left
void NoteOn(int velocity)   // MIDI velocity 0..127, clamped; restarts every
                            // envelope but does NOT touch the phase
void NoteOff()              // releases the note; keep rendering until IsFinished
```

Every waveform in this package implements it, so anything that plays notes has one contract to drive.
The default behavior is the one a waveform that merely runs wants: `NoteOn` records the velocity and
marks the key down, `NoteOff` marks it up, and `IsFinished` is never true - a sine has no end of its
own, so whatever owns the envelope decides when the note stops. Two waveforms end by themselves:
`pluck1`, once the string has decayed below an inaudible level whether the key is still down or not,
and `fm6op`, once the key is up and every carrier that can be heard has finished its envelope.

`ModestOscillatorFactory` builds one by enum or by name - `SupportedWaveforms`,
`IsSupported(ModestWaveform)`, `IsSupported(string)`, `Create(ModestWaveform)` and
`TryCreate(string, out IModestOscillator)`. Use `TryCreate` when the name came from a file.
`ModestWaveform` is the enum - `Sine`, `Saw`, `Square`, `Triangle`, `Noise`, `Pluck1`, `Wavetable`,
`Harmonic`, `Fm6Op`, `Formant` - and `ModestWaveforms` holds the string spellings as constants and
converts, case-insensitively, trimming whitespace and accepting `white_noise` as a synonym for
`noise`.

### ModestPatch, the parameter model

`ModestPatch`, in `CodeBrix.Audio.ModestSynth.Patch`, is a plain settings object. It holds no audio
state, so one patch can build any number of voices, and it is safe to read from several threads as
long as nothing is writing to it. Every property is named for the sampler attribute it mirrors, and
every default is the format's documented default.

| Group | Members |
| --- | --- |
| Shape and voice | `Waveform` (default `Sine`), `Damping` (0..1, 0.5), `PluckType` (0..1, 0.5), `RandomPhase` (false), `Seed` (0 = each oscillator's own default seed) |
| Wavetable | `WavetableFile`, `WavetableTable`, `WavetableFrameSize` (2048), `WavetablePosition` (0.0), `WavetableFrameInterpolation` (true) |
| Additive | `NumPartials` (8), `HarmonicTilt` (0.0), `HarmonicOddEvenBalance` (0.5), `HarmonicNormalization` (0.0), `GetPartialLevel(n)` / `SetPartialLevel(n, level)` for partials 1..64, each 0.0 |
| FM | `FmAlgorithm` (1), `FmOperators` (six `ModestFmOperator` settings objects), `GetFmOperator(1..6)` |
| Building a voice | `CanCreateOscillator`, `CreateOscillator()`, `CreateOscillator(rate)`, `GetStartPhase(voiceSeed)` |

`WavetableTable` is a `WavetableFile` you already have, and it is used **instead** of reading
`WavetableFile` - which is how a table built in code is played, and how `CreateOscillator` is kept off
the disk entirely. Otherwise `CreateOscillator` loads `WavetableFile` through the shared cache.

`CreateOscillator` hands back an oscillator at the requested rate, carrying the patch's parameters, at
its default pitch and **not** reset. Set the pitch, then `Reset` with `GetStartPhase` at note-on.
Out-of-range values are clamped rather than rejected, because an attribute out of range is never worth
an exception, and non-finite values are ignored, leaving the previous value in place.

`ModestFmOperator`, `ModestFmOperatorMode` and `ModestFmEnvelopeType` are the six-operator parameter
model: `Ratio`, `Detune` (-7..7), `Mode` (`Ratio` or `Fixed`), `FixedFrequency`, `Level`,
`VelocitySensitivity` (0..7), `Feedback`, `Attack` / `Decay` / `Sustain` / `Release`, `EnvelopeType`
(`Adsr` or `Dx7`) and `EgRate1..4` / `EgLevel1..4` (0..99). `Attack` and `Release` accept -1, the
sentinel meaning "no envelope of my own - the group's envelope gates me". A patch's six operators are
a **template**: `CreateOscillator` copies their values into the voice it builds.

### The standalone synthesizer

`ModestSynthesizer` plays a `ModestPatch` from MIDI events. There is no preset file, nothing to
register and nothing to resolve: a patch, some settings, and the same `IMidiSynthesizer` contract the
SoundFont, SFZ and Decent Sampler engines implement, so every player and renderer in CodeBrix.Audio
takes it.

```csharp
new ModestSynthesizer(patch, sampleRate)
new ModestSynthesizer(patch, settings)

ModestPatch Patch            // the sound being played
int  SampleRate / BlockSize / MaximumPolyphony / ChannelCount (16)
int  ActiveVoiceCount
float MasterVolume           // 0.5 by default, as the whole family uses
void ProcessMidiMessage(channel, command, data1, data2)
void NoteOn(channel, key, velocity)      // velocity 0 is a note-off
void NoteOff(channel, key)
void NoteOffAll(bool immediate)
void NoteOffAll(int channel, bool immediate)
void Reset()
void Render(Span<float> left, Span<float> right)
```

It handles sixteen channels; note-on and note-off with velocity; the sustain pedal (CC 64), which
holds a released note until the pedal lifts; pitch bend over the settings' range; all sound off
(CC 120), reset all controllers (CC 121) and all notes off (CC 123); and a fixed voice pool that
steals the oldest **released** voice first, then the oldest voice of any kind.

The patch is read when the synthesizer is built. Every voice gets its own oscillator, configured then;
changing the patch afterwards changes nothing, so build another synthesizer for another sound.
Rendering allocates nothing and takes no lock; like every synthesizer in this family it is not
thread-safe, so MIDI events and rendering must not overlap.

`ModestSynthesizerSettings` says how a patch is played, where the patch says what it sounds like. They
are separate because `ModestPatch` mirrors the sampler format's own oscillator model, which has no
envelope of its own - in a preset the group's envelope does that job, and here these do.

| Setting | Range | Default |
| --- | --- | --- |
| `SampleRate` | 8,000..192,000 | 44,100 |
| `BlockSize` | 8..1,024 frames | 64 |
| `MaximumPolyphony` | 1..1,024 voices | 32 |
| `Attack` / `Decay` | seconds | 0 / 0 |
| `Sustain` | 0..1 amplitude | 1 |
| `Release` | seconds | 0.1 |
| `GlideSeconds` | 0 turns glide off; linear in pitch, and this long whatever the interval | 0 |
| `PitchBendSemitones` | 0..48 | 2 |
| `VelocityTracking` | 0..1; gain is `(1 - t) + t * vel/127`, the format's velocity-tracking law | 1 |
| `MasterVolume` | | 0.5 |
| `RandomSeed` | | 12345 |

Every value clamps rather than throwing, except `SampleRate`, `BlockSize` and `MaximumPolyphony`,
which are structural and throw. The settings are **copied** by the constructor, so changing them
afterwards has no effect. `Clone()` copies one.

### The example patches

`ModestSynthPresets` holds six worked examples, each a new object every time you ask:

| Preset | What it is |
| --- | --- |
| `SubSine()` | A plain sine - a sub-bass, and the tone to check a signal path with |
| `SawLead()` | A band-limited sawtooth with random phase on, so stacking two synthesizers thickens rather than doubles |
| `PluckedString()` | A waveguide string, with a long ring and a mostly smooth excitation |
| `ElectricPiano()` | An FM patch on algorithm 5: a high-ratio tine decaying in a quarter of a second over a soft body, with the tine's velocity sensitivity doing the brightness |
| `WavetablePad()` | A wavetable on a four-frame table **built in code**, through `ModestPatch.WavetableTable`, so it touches no file |
| `AdditiveOrgan()` | The drawbar partials 1, 2, 3, 4, 6 and 8 with the loudness compensation on |

`Names` lists them, `Create(string)` builds one case-insensitively, and
`SettingsFor(string, rate)` returns the envelope, glide and volume that go with it - because a patch
carries no envelope, and a pad wants a slow attack and a long release where a plucked string wants
neither. An unknown name throws `ArgumentException` from both.

### The classic waveforms

| Waveform | Type | Parameters beyond pitch |
| --- | --- | --- |
| `sine` | `SineOscillator` | None. One partial, so it cannot alias at any pitch below Nyquist. This is the default an `<oscillator>` with no waveform attribute gets |
| `saw` | `SawOscillator` | None. Both odd and even harmonics; the falling edge is band-limited with a polyBLEP correction. Expect a small overshoot past 1 at the corrected edge - that is the correction doing its job |
| `square` | `SquareOscillator` | `PulseWidth` (0.01..0.99, default 0.5). Both edges are band-limited. Pulse width is a **standalone extra**: the `<oscillator>` element has no pulse-width attribute, so an oscillator built from a preset always runs at half |
| `triangle` | `TriangleOscillator` | None. Odd harmonics falling away far faster than a square's. Its discontinuity is in the slope rather than the value, so it is corrected with a polyBLAMP at each corner, which keeps the level and the shape right at every pitch |
| `noise` / `white_noise` | `NoiseOscillator` | `Seed` (uint) and `Amplitude` (0..1). Every sample drawn independently and uniformly, for a flat spectrum. The default amplitude is measured: it puts this oscillator's RMS below a full-amplitude sine's, where the reference player's noise oscillator sits. Set `Amplitude` to 1.0 for a full-scale noise source |

Noise has no pitch: `SetFrequency` is accepted and recorded so a voice can drive every oscillator the
same way, and changes nothing you can hear. `Reset` restarts the sequence from `Seed` and ignores the
phase it is handed, because noise has no phase. The same seed renders the same samples on every run
and every platform - so two noise voices with the **same** seed are one louder copy, not a wider
sound.

### pluck1

`Pluck1Oscillator` takes `Damping` (0..1, default 0.5), `PluckType` (0..1, default 0.5) and `Seed`
(uint), and exposes `DecayTimeSeconds` read-only - what the present `Damping` maps to.

It is a waveguide plucked string: a delay line one period long fed back through a gentle low-pass, so
high partials die away before low ones exactly as they do on a real string. It is excited once at
`Reset` and decays from there; every `Reset` is a new pluck.

`Damping` is the **decay**, not a filter cutoff: 0.0 is heavily damped and short, 1.0 barely damped
and long. It sets the fraction the string keeps on each trip round the waveguide, so the decay is
pitch-dependent by design and a low note rings longer than a high one at the same setting, as on a
real instrument. Read `DecayTimeSeconds` for the number at the current setting and pitch. It takes
effect on the next rendered sample, so a knob can shorten a ringing string.

`PluckType` is the **excitation**: 0.0 fills the string with a smooth triangle for a soft, mellow
sound, 1.0 with a noise burst for a bright, aggressive one, and values between blend the two linearly.
It is read only at note-on, so changing it mid-note does nothing until the next `Reset`.

The pitch is tuned with a first-order all-pass inside the loop, so the fundamental lands on the
requested frequency rather than on the nearest whole number of samples. It is tunable from 8 Hz up to
a third of the sample rate - which covers every MIDI note at every ordinary sample rate - and asking
for anything outside that throws `ArgumentOutOfRangeException`.

The format's own guidance for settings: a bass guitar wants damping 0.3 to 0.5 with pluck type 0.6 to
0.8; an acoustic guitar damping 0.5 to 0.7 with pluck type 0.4 to 0.6; a harp or lyre damping 0.7 to
0.9 with pluck type 0.2 to 0.4.

### wavetable

`WavetableOscillator`, in `CodeBrix.Audio.ModestSynth.Wavetable`, takes `Table` (a `WavetableFile`, or
null), `Position` (0..1, default 0.0) and `FrameInterpolation` (default true), exposes `HasTable`,
`Problem`, `MipLevel` and `Table` read-only, and offers `TryLoad(path, frameSize)` and
`ReportProblem(reason)`.

It reads one cycle per period from a frame of a multi-frame `.wav` file, so moving `Position` walks
the table from timbre to timbre. `Position` 0.0 is the first frame and 1.0 the last, mapping to frame
index `position * (FrameCount - 1)`. With `FrameInterpolation` on, adjacent frames are linearly
crossfaded, which is what a morphing table wants; with it off the position snaps to the nearest whole
frame, which is what a table of unrelated shapes wants. A position change is **ramped** across the
block it arrives in, so a modulator scanning the table does not click once per block, and the property
reads back the target. `Reset` snaps instead of ramping.

The file may be any `.wav` CodeBrix.Audio can read - any bit depth, any sample rate, mono or
multi-channel. A multi-channel file is downmixed, because an oscillator is one mono voice, and the
file's own sample rate is irrelevant, since a frame is one cycle whatever rate it was recorded at.

The frame size is decided in the order the format gives: a Serum-compatible `clm ` RIFF chunk wins if
the file carries one; otherwise the `wavetableFrameSize` you asked for; otherwise the format's
documented default. `HasClmChunk` and `DeclaredFrameSize` on the loaded `WavetableFile` say which
happened.

**No table means a sine.** That is measured, not a fallback of convenience: the reference player
renders a wavetable oscillator with no file as a pure sine at the note's frequency and at a sine's own
level. A missing or unreadable file behaves the same way, and `Problem` then carries one line for a
`Problems` list. Nothing throws, ever.

`WavetableFile` is an immutable decoded wavetable - the frames a file holds, plus the band-limited
copies the oscillator plays from - so one table serves every voice. It is built by
`TryLoad(path, frameSize, out file, out problem)`, by the `Stream` overload for bytes that are not a
file on disk (an entry inside a `.dslibrary`, for instance), or by
`FromSamples(ReadOnlySpan<float>, frameSize, name)` for a table written in code. It exposes
`SourcePath`, `FrameSize`, `FrameCount`, `SampleCount`, `HasClmChunk`, `DeclaredFrameSize`,
`SourceSampleRate`, `SourceChannels`, `ApproximateSizeInBytes` and `GetFrame(int)`, plus the constants
`DefaultFrameSize`, `MinimumFrameSize`, `MaximumFrameSize` and `MaximumSampleCount`.

`WavetableFileCache` is the static cache: `TryGetOrLoad` by path or by cache key plus an opener,
`GetOrLoad(path, frameSize)`, `Clear()`, `Count` and `ApproximateSizeInBytes`. One decode per full
path and requested frame size, shared by every voice. It is thread-safe, none of it belongs on the
audio thread, failures are not cached so a file that appears later is picked up, and nothing is
evicted until you call `Clear()`.

The table is band-limited per octave when it loads, so a frame full of harmonics stays clean at the
top of the keyboard - measured on a sawtooth frame, better at every pitch than this package's own
polyBLEP sawtooth. Those band-limited copies cost about four and a half times the frame data and are
built once, which is why loading through the cache matters.

### harmonic

`HarmonicOscillator`, in `CodeBrix.Audio.ModestSynth.Harmonic`, takes `NumPartials` (1..64, default
8), `Tilt` (-1..1, default 0), `OddEvenBalance` (0..1, default 0.5), `Normalization` (0..1, default
0), `GetPartialLevel(n)` / `SetPartialLevel(n, level)` for partials 1..64 and `ClearPartialLevels()`,
and exposes `IsPureSineFallback`, `ActivePartialCount` and `GetPartialGain(n)` read-only.

It is additive synthesis: up to 64 sine partials at whole multiples of the note's frequency, summed.
Every parameter can be changed between blocks and is meant to be - they are the targets of the
`OSCILLATOR_HARMONIC_*` bindings.

**Nothing set means a sine.** Every partial level defaults to 0, and the reference player renders an
oscillator carrying no harmonic attributes as a pure sine at a sine's own level, so "no partial has a
level" is treated as "the fundamental is at full level". Set any partial's level and that stops: what
you set is what sounds, and `IsPureSineFallback` says which state you are in. `ClearPartialLevels()`
puts it back. `NumPartials` is a **ceiling**, not a count of what sounds: a partial inside the limit
whose level is 0 contributes nothing, and a partial above the limit is silent whatever its level.

The three shaping laws are the reference player's own behavior, established by measurement:

| Attribute | The law |
| --- | --- |
| Tilt | Partial *k* is multiplied by *k* to the power of `-2 * tilt` - 12 dB per octave per unit, pivoting on the fundamental, so a tilt of +0.5 gives exactly a sawtooth's slope and +1 a triangle's. The fundamental never changes. A **negative** tilt boosts hard, so pair it with normalization unless you want the level to climb |
| Odd/even balance | Odd partials are multiplied by `min(1, 2 * (1 - balance))` and even ones by `min(1, 2 * balance)`. A balance of 0.5 leaves **both** at full level, so it is genuinely neutral rather than a half-and-half mix; 0.0 silences the even partials and 1.0 the odd ones, each fading linearly over its half of the range. The fundamental counts as odd |
| Normalization | The gain is `(1 - n) + n / sum`, where `sum` is the total of the partial gains - a plain sum divide blended linearly toward unity. Full compensation therefore holds the summed **peak** where a single full-level partial's would be rather than the RMS, and the blend is linear in gain rather than in decibels |

A partial whose frequency reaches Nyquist is not summed at all, so a note sweeping up the keyboard
loses partials one at a time instead of folding them back down; `ActivePartialCount` says how many are
sounding, and they come back when the pitch falls. Only the partials that actually sound are summed,
and `Render` allocates nothing even on the block where a parameter changed.

### fm6op

`Fm6OpOscillator`, in `CodeBrix.Audio.ModestSynth.Fm`, is six sine operators wired together by one of
32 algorithms. An operator whose output is **heard** is a carrier; one whose output is added to another
operator's **phase** is a modulator, and that phase bending is what turns a sine into a timbre. The
algorithm decides which is which.

```csharp
var voice = new Fm6OpOscillator { Algorithm = 5 };
voice.SetSampleRate(48000);
voice.SetFrequency(261.63);
voice.GetOperator(1).Level = 0.9;      // carrier
voice.GetOperator(2).Level = 0.55;     // its modulator
voice.GetOperator(2).Ratio = 14.0;
voice.Reset(0.0);                      // phases
voice.NoteOn(100);                     // velocity
voice.Render(block);                   // ... and again, and again
voice.NoteOff();                       // render on until voice.IsFinished
```

The oscillator carries `Algorithm` (1..32, clamped, default 1, and safe to change mid-note because
phases and envelopes are kept), `Topology` (the routing the algorithm selects), `OutputGain`,
`ModulationDepthCycles`, `FeedbackDepthCycles`, `UseOperator6FeedbackFallback`, `RateScaling` (0..7,
default 0), `EffectiveFeedback`, `Velocity`, `IsKeyDown`, `IsFinished`, `UsesOuterRelease`,
`OperatorUsesOuterRelease(1..6)`, `GetOperator(1..6)`, `Operators`,
`GetOperatorFrequencyHz(1..6)` and `ApplyPatch(ModestPatch)`. Parameters are read **once per block**,
at the top of `Render`; nothing is read per sample and nothing allocates.

| Per-operator parameter | Range and default |
| --- | --- |
| `Ratio` | Multiplies the played note's frequency. **Defaults to the operator's own number**, which is measured: with no ratios written, algorithm 32's six carriers land on 1 to 6 times the note. Negative and non-finite values are ignored |
| `Detune` | -7..7, clamped, default 0. Measured as a power law in the operator's own frequency, so it is neither a fixed number of hertz nor a fixed interval - a strong detune low down and a slight one high up |
| `Mode` | `Ratio` (default) or `Fixed` |
| `FixedFrequency` | In Hz, default 440. Used only in `Fixed` mode, where the note played is ignored. Detune still applies |
| `Level` | 0..1, clamped. What the operator contributes: audio if the algorithm makes it a carrier, modulation if it makes it a modulator. Default 1.0 on **operator 1** and 0.0 on operators 2 to 6 |
| `VelocitySensitivity` | 0..7, clamped, default 0 - no velocity response at all. Measured as a five-point decibel table scaled by the sensitivity number, whose **neutral point is velocity 96**, not 127, so a hard note is pushed up rather than merely left alone |
| `Feedback` | 0..1, clamped, default 0 |
| `Attack` / `Decay` | In seconds, default 0.0 each |
| `Sustain` | 0..1 amplitude, default 1.0 |
| `Release` | In seconds, default -1.0, the sentinel |
| `EnvelopeType` | `Adsr` (default) or `Dx7` |
| `EgRate1..4` | 0..99, defaults 99, 99, 0, 99 |
| `EgLevel1..4` | 0..99, defaults 99, 99, 99, 0 |

The default operator levels differ from the format's own attribute table, which says every operator
defaults to 1.0. Three things say that table is wrong: the reference player renders a **pure sine**
for an FM oscillator carrying no attributes, which six operators at full level could not produce; the
format's own tutorial says to raise the second operator's level "from 0 to 1 to hear FM modulation
build from a sine wave"; and a measurement round confirmed operator 1 at 1.0 and the rest at 0.0
outright. CodeBrix.Audio's own zone resolution uses the same defaults, so a preset played through the
sampler engine and a patch built here agree.

`Fm6OpAlgorithms.Get(1..32)` returns an `Fm6OpAlgorithm` describing the routing - `Carriers`,
`Modulators`, `RenderOrder`, `FeedbackSource`, `FeedbackDestination`, `IsCarrier(n)`,
`Modulates(m, n)` and `GetModulatorsOf(n)`. The numbering is the published chart's numbering, so an
algorithm number from a vintage patch bank can be used as it stands. Carrier counts run from one to
six, and there is **no** normalization by the number of carriers, which is why six loud operators
clip: turn the carriers down, or the group's volume.

Each algorithm has exactly one feedback loop, and two of the loops span several operators, which is
why `FeedbackSource` and `FeedbackDestination` are separate. The format says both that only the
algorithm's own feedback operator is audible and that operator 6's feedback works in all 32
algorithms. The first is what the reference does - feedback written on any operator but the
algorithm's own is silently ignored, and the value is clamped at 1.0 - so
`UseOperator6FeedbackFallback` is off by default; set it true for a patch bank that relies on the
second reading, and `EffectiveFeedback` reports which value won. Light feedback warms the tone,
moderate feedback turns it sawtooth-like, and heavy feedback breaks into noise, as the format
describes.

There are two envelope shapes per operator. The ADSR form, which is the default, takes `Attack`,
`Decay` and `Release` in **seconds** with `Sustain` as an amplitude, and runs straight-line segments.
The four-stage form takes the hardware's 0..99 rates and levels, which can be copied straight out of a
vintage patch bank: R1 climbs from silence to L1, R2 moves to L2, R3 moves to L3 and holds there while
the key is down, and R4 takes it to L4 after note-off. Setting the envelope type to the four-stage
form makes the operator ignore its `Attack`, `Decay`, `Sustain` and `Release`. `Dx7Tables` exposes the
conversion: `FullSweepSeconds(rate)`, `LevelUnitsPerSecond`, `LevelUnitsToAmplitude`,
`AmplitudeToLevelUnits`, `DetuneHz`, `VelocityScale`, `VelocityDecibels` and `RateScalingUnits`.

> [!WARNING]
> Rate 0 does **not** hold - it crawls, covering the whole range slowly, where the format's own
> description promises a hold. The default third rate of 0 therefore drifts rather than sitting still:
> give R3 a real rate and L3 a real level if you want a stage that stays put. An L4 above 0 likewise
> means the operator never goes silent by itself.

`Release = -1`, the **default**, means "I have no release of my own - the group's envelope decides
when this note ends". After `NoteOff` such an operator holds its level and `IsFinished` never becomes
true, because the thing that decides is outside the oscillator. `Attack = -1` goes further: the
operator has no envelope at all and sits at full level, gated only by the group. Any negative value
becomes the sentinel, and both apply to the ADSR form only. `UsesOuterRelease` is the flag to honor:
it is true when a carrier that can be heard is in that state, which is exactly when `IsFinished` will
never fire and something else has to stop the voice.

### formant

The `formant` waveform is a waveform the published format guide does not document at all, and a newer
reference player accepts it. Measurement settles what it is: it takes **no attributes at all**, and it
sounds one formant region near 2.4 kHz over the note's own fundamental. `FormantOscillator` renders
that tone from the reference's own measured spectrum, read as a spectral envelope in hertz so the
resonance holds still while the harmonics move through it. Every `formant-*` attribute a preset writes
is reported, because the reference ignores them all.

Two things about it are worth knowing before you use it. It is **loud** - the reference's own tone
reads well above what the same player's sine renders at the same group volume - so a preset that leans
on it wants its own volume attribute. And the **phase** of each partial cannot be recovered from a
magnitude spectrum, so the waveform's shape is this package's own even though its spectrum is the
reference's.

The core's own feature list still reports the waveform as unrecognized when a preset names it, because
the guide that list is built from does not document it. That line in `Problems` is expected and the
group sounds anyway.

### The creative effects

Seven effects live in `CodeBrix.Audio.ModestSynth.Effects`: `phaser`, `pitch_shift`, `wave_folder`,
`wave_shaper`, `stereo_simulator`, `bit_crusher` and `gate`. They are the nonlinear and creative half
of the sampler format's effect set; the mixing and room half - the filters, gain, reverb, delay,
chorus, convolution and the compressor - belongs to CodeBrix.Audio itself, so a sample library that
uses only those plays in full without this package.

Every effect implements CodeBrix.Audio's `IInstrumentEffect`, from
`CodeBrix.Audio.Synth.DecentSampler.Engine`, so the same object serves a sampler chain and your own
code:

```csharp
bool Enabled                  // false bypasses; the block passes through and the
                              // effect's internal state is left as it was
IReadOnlyList<string> Tags    // the <effect> element's tags. Never null; setting
                              // null clears it
void Prepare(int sampleRate)  // allocates. Call it once, off the audio thread,
                              // before the first block
void Reset()                  // clears every buffer, leaves every parameter
void Process(float[] left, float[] right, int frames)      // in place
bool TrySetParameter(string name, double value)
bool TryGetParameter(string name, out double value)
bool TrySetParameter(string name, string value)
```

`ModestEffectBase` adds `SampleRate` and `IsPrepared`.

Every effect answers to **both** spellings of each parameter: the XML attribute the effect page
documents, such as `modRate`, and the binding parameter the appendix documents, such as `FX_MOD_RATE`.
Matching ignores case, punctuation and the `FX_` prefix, so `modrate`, `MOD-RATE` and `fx mod rate`
are all the same parameter, and `ENABLED` works on every effect as a number or as a word. An unknown
name returns false rather than being swallowed, which is how a host can tell a typo from a parameter
it does not use; an out-of-range value is clamped; and a non-finite value leaves the parameter where
it was and still returns true if the name was one the effect knows.

Every effect has a parameterless constructor carrying the developer guide's documented defaults, so
`new PhaserEffect()` is exactly what `<effect type="phaser" />` means. `ModestEffectFactory.Create`,
`TryCreate`, `IsSupported` and `SupportedTypes` build one from a type name, and `ModestEffectTypes`
holds the seven names as constants. `Create` **throws** `NotSupportedException` for a core effect name
like `reverb`, with a message saying where it actually lives.

Six of the seven have a `Mix`, and it always means the same thing:
`out = (1 - mix) * dry + mix * wet`. A mix of exactly 0 passes the block through bit for bit. The
stereo simulator has no mix; its width does that job.

| Effect | Parameters, with defaults |
| --- | --- |
| `phaser` | `mix` (0..1, 0.5), `modDepth` (0..1, 0.2), `modRate` (0..10 Hz, 0.2), `centerFrequency` (20..22000, 400 Hz), `feedback` (-1..1, 0.7) |
| `pitch_shift` | `pitchShift` (-24..24 semitones, 0), `mix` (0..1, 0.5) |
| `wave_folder` | `drive` (1..100, 1), `threshold` (0..10, 0.25), `mix` (0..1, 1.0) |
| `wave_shaper` | `drive` (1..1000, 1), `driveBoost` (0..1, 1), `outputLevel` (0..8, 0.1), `highQuality` (false), `mix` (0..1, 1.0) |
| `stereo_simulator` | `algorithm` (`lauridsen`, `schroeder` or `adt`; `adt`), `width` (0..1, 0.5), `delayTime` (0.001..0.030 s, 0.005), `modRate` (0.1..10 Hz, 0.5, `adt` only), `modDepth` (0..1, 0.3, `adt` only) |
| `bit_crusher` | `bitDepth` (1..24, 24), `sampleRateReduction` (1..32, 1), `mix` (0..1, 1.0) |
| `gate` | `amount` (0..1, 0.5), `mix` (0..1, 1.0), plus `Seed` and `WindowSeconds` as standalone extras |

**The phaser** is a cascade of six first-order all-pass sections all tuned to the center frequency,
added back to the dry signal, which puts three moving notches in the spectrum. The sweep is
exponential and reset at every note-on -
`fc(t) = centerFrequency * 2^(5 * modDepth * -sin(2 * pi * modRate * t))`, five octaves each way at
full depth, and downward first - so a mod rate of 0 freezes the chain at the center frequency exactly,
whatever the depth says. `PhaserEffect.AllPassStageCount` and `PhaserEffect.SweepOctavesAtFullDepth`
are those two numbers as constants. Feedback is **subtracted**, not added, which is what turns the
notches into resonant peaks and costs a little level elsewhere; a negative feedback acts as none, and
the value is held a little below unity while processing, because an all-pass loop at a gain of exactly one
never decays.

**The pitch shifter** is the guide's "old-school" design: a 50 ms delay line read by two grains half a
grain apart, each sliding towards or away from the write pointer at `2^(semitones/12) - 1` samples per
sample. Each grain is windowed by a raised cosine that reaches zero exactly where it wraps, and the
pair sums to one, so nothing steps and nothing drops out. `GrainLengthSamples` reports the grain at the
prepared rate. The tuning is exact and the level is preserved. This one does **not** delay the dry
path, so at a mix between 0 and 1 the two are a grain apart and comb against each other, the way an
analogue-era shifter does; the wet path therefore carries about 50 ms of latency at any setting. The
classic cost of the classic method is a warble at the grain rate - that is the method, not a defect.

**The wave folder** is a closed form. With `k = drive / (2 * sqrt(2) * threshold)`:

```text
foldOnce(u) = u for |u| <= 1, else sign(u) * (2 - |u|)
out = foldOnce(in * k) / k
```

Three things in that are not the textbook triangle folder. It is a **single** reflection that keeps
going down past zero rather than a repeated triangle; it is **not** clamped, so the output is not
bounded by the threshold; and the output is divided by the same `k` the input was multiplied by, so
**below the fold point the effect is a bit-identical pass-through** - drive does not even amplify
until the signal reaches the fold. The fold point is at `1/k`, and an input of `2/k` folds to exactly
zero. The guide gives the wave folder no `mix` attribute; one is accepted here and defaults to 1.0, so
a preset written with only the documented attributes behaves exactly as documented. Because folding is
per-voice by nature, the guide's own advice is to put a wave folder at **group** level, where the
engine builds one per voice.

**The wave shaper** is a saturating curve, `out = tanh(drive * (1 + driveBoost) * in) * outputLevel`.
Its output level defaults to 0.1, a 20 dB cut: that is the format's own default, confirmed by
measurement, and a preset that raises it is asking for a large boost. `HighQuality` is four-times
oversampling through a pair of half-band filters in each direction, which pushes the aliases a hard
drive would fold back into the audible band well down and costs four evaluations of the curve plus
twelve filter runs per sample - the guide's own warning about CPU - and adds a little latency to the
wet path that the dry path does not carry. The effect page gives `outputLevel` a range of 0 to 1 and
the binding appendix 0 to 8; the wider one is honored, so a binding written to the appendix is not
silently clipped. Like the folder, this belongs at group level in a preset.

**The stereo simulator** turns one signal into two: the input is summed to mono, a decorrelated copy
of it is added on the left and subtracted on the right, and `width` decides how much of the original
is left in the middle - `middle = (1 - width) * mono`, `side = width * sideGain * decorrelated`. A
**stereo input is summed to mono first**, so it loses its own side signal; that is what converting a
mono signal into a pseudo-stereo one means, and it is why a width of 0 is documented as mono and dry.
The three algorithms differ only in how far they decorrelate: `lauridsen` fully, `adt` in the middle
and the default, and `schroeder` the subtlest. At a width of 1 the middle is removed entirely and the
mono sum is silent.

**The bit crusher** quantizes and holds. Bit depth is a mid-tread quantizer over -1..1 - the signal is
rounded to the nearest of `2^bitDepth` evenly spaced levels - and sample-rate reduction is a
sample-and-hold, so a factor of four holds each sample for four. Both accept fractional values,
because a knob bound to them sweeps through, and the hold length then alternates rather than jumping.
The defaults are transparent: 24 bits is finer than a float can hold and a reduction of 1 holds
nothing.

**The gate** flips a weighted coin roughly every fifty milliseconds on whether to let the signal
through or cut it to silence, crossfading at each decision so the transitions do not click. `amount`
is the probability of **closing**, so 0 never gates and 1 gates constantly. The coin is a seeded
generator: two gates with the same `Seed` render the same dropouts, `Reset` restarts the sequence, and
setting `Seed` restarts it immediately. Built from a preset, the seed is derived from the effect's
position in its chain, so two gates in one instrument do not stutter in lock step and the same
instrument rendered twice stutters identically. `CurrentGain` reports the gain the gate is applying
right now.

`Process` allocates nothing, locks nothing and opens nothing once `Prepare` has run. An effect that is
processed without ever having been prepared prepares itself at 44,100 Hz on its first block rather
than throwing - a convenience, not a license: if you are rendering at another rate, that block is the
wrong rate and every one after it. An effect instance belongs to the thread rendering it. And
`Process` writes **both** channels, so passing the same array twice throws `ArgumentException` rather
than silently processing it twice.

### How close it is to the reference

The waveforms and effects were measured against recordings of the reference player rather than guessed
at, and the documentation says so where a measurement could not settle something.

The four classic shapes match the reference's power-weighted spectral centroid to within a fraction of
a decibel. `pluck1` is close on every setting the reference was recorded at, and its decay time now
lands within a few percent at each of the damping settings measured, because the loss per trip round
the string turned out to be cubic in one minus damping rather than linear in the gain. White noise
carries the reference's own anti-imaging rolloff above 8 kHz, reproduced closely at every measured
band; its power centroid lands slightly darker than the reference's, and the reference's own two
figures disagree by more than that, so no single response can meet both.

`wavetable` and `harmonic` have been measured properly, and most of what they do is the reference
player's own behavior rather than a reading of the guide: the frame a position maps to and its linear
crossfade; a partial's level as an amplitude; the partial count as a ceiling; no levels at all meaning
a pure sine; the tilt law; the odd/even balance; and the loudness compensation, which is a plain sum
divide blended linearly toward unity rather than a root-sum-of-squares law. A wavetable's frame snap
rounds half up, the file's `clm ` chunk beats any frame size you pass, and a stereo wavetable is read
as its **left** channel - all three measured.

`fm6op` is measured throughout: the level an attribute-free FM oscillator renders at, the modulation
index a level of 1.0 buys and the whole sideband pattern that produces, the carrier set and direct
modulation matrix of all 32 algorithms, the ratio default, the absence of carrier-count normalization,
the five-point velocity table with its neutral point at velocity 96, the four-stage rate scale with
rate 0 crawling, the detune power law over the range it was measured across, and feedback acting only
on the algorithm's own operator and clamping at 1. What is still reasoned rather than measured, each a
single named constant: the feedback depth in cycles, the 0..99 level scale's curve, the
modulator-to-modulator chains inherited from the published chart, the detune law outside the measured
range, and rate scaling, which the format has no attribute for at all.

The effects are measured where a measurement exists and honest where one does not. The wave shaper's
drive law reproduces the reference's measured boost closely; the stereo simulator reproduces both of
its measured figures, including the decorrelation ordering; the gate's level loss matches; the pitch
shifter is accurate to the semitone; the wave folder's closed form reproduces every measured
root-mean-square point and the harmonic structure of three cases; and the phaser's structure, sweep
and subtracted feedback are all measured. What is not measured is named as such on each effect.

## Examples

Plucking a string, and building a voice from a patch:

```csharp
using CodeBrix.Audio.ModestSynth.Oscillators;

var string1 = new Pluck1Oscillator
{
    Damping = 0.7,            // longer ring - an acoustic guitar rather than a muted bass
    PluckType = 0.45,         // mostly the smooth excitation, with some bite
};
string1.SetSampleRate(48000);
string1.SetFrequency(146.83); // D3
string1.Reset(0.0);           // every Reset is a new pluck

var block = new float[48000];
string1.Render(block);        // one second of a decaying string
```

```csharp
using CodeBrix.Audio.ModestSynth.Oscillators;
using CodeBrix.Audio.ModestSynth.Patch;

var patch = new ModestPatch
{
    Waveform = ModestWaveform.Pluck1,
    Damping = 0.6,
    PluckType = 0.8,
    RandomPhase = true,       // layered voices should not all start together
};

uint voiceNumber = 0;
IModestOscillator voice = patch.CreateOscillator(48000);
voice.SetFrequency(220.0);
voice.Reset(patch.GetStartPhase(voiceNumber++));
```

Scanning a wavetable, with the table shared through the cache:

```csharp
using CodeBrix.Audio.ModestSynth.Wavetable;

// One decode, shared by every voice that plays this file.
var table = WavetableFileCache.GetOrLoad("Wavetables/Growl 01.wav", 2048);

var voice = new WavetableOscillator { Table = table, FrameInterpolation = true };
voice.SetSampleRate(48000);
voice.SetFrequency(110.0);     // A2
voice.Reset(0.0);

var block = new float[512];
for (int i = 0; i < 64; i++)
{
    voice.Position = i / 63.0;  // sweep the table; the change is ramped, not stepped
    voice.Render(block);
}
```

Building a tone out of partials:

```csharp
using CodeBrix.Audio.ModestSynth.Harmonic;

var organ = new HarmonicOscillator { NumPartials = 16, Normalization = 1.0 };
organ.SetPartialLevel(1, 1.0);
organ.SetPartialLevel(2, 0.5);
organ.SetPartialLevel(4, 0.35);
organ.SetPartialLevel(8, 0.2);

organ.SetSampleRate(48000);
organ.SetFrequency(261.63);   // middle C
organ.Reset(0.0);

var block = new float[512];
organ.Render(block);          // partials that would alias are simply not sounded
```

Crushing and stuttering a stereo buffer:

```csharp
using CodeBrix.Audio.ModestSynth.Effects;

var crusher = new BitCrusherEffect { BitDepth = 6, SampleRateReduction = 4 };
crusher.Prepare(48000);          // the only call that allocates

var stutter = new GateEffect { Amount = 0.4, Seed = 12345 };
stutter.Prepare(48000);

var left = new float[512];
var right = new float[512];
// ... fill both buffers ...

crusher.Process(left, right, 512);   // in place, on both channels
stutter.Process(left, right, 512);
```

Rendering an electric piano phrase to a WAV file through the standalone synthesizer:

```csharp
using CodeBrix.Audio.Midi;
using CodeBrix.Audio.ModestSynth;
using CodeBrix.Audio.Synth;

// 1. The sound, and how it is played.
var patch = ModestSynthPresets.ElectricPiano();
var settings = ModestSynthPresets.SettingsFor(
    ModestSynthPresets.ElectricPianoName, 48000);
settings.MaximumPolyphony = 16;

var synthesizer = new ModestSynthesizer(patch, settings);

// 2. Something to play. Any MidiSequence will do - one read from a .mid
//    file, one built by MultiTrackPlayer, or one written by hand.
var events = new MidiEventCollection(1, 480);
foreach (var (tick, note) in new[] { (0, 60), (480, 64), (960, 67) })
{
    events.AddEvent(new NoteEvent(tick, 1, MidiCommandCode.NoteOn, note, 100), 1);
    events.AddEvent(
        new NoteEvent(tick + 440, 1, MidiCommandCode.NoteOff, note, 0), 1);
}

events.PrepareForExport();
var sequence = MidiSequence.FromEvents(events);

// 3. Render it offline, leaving half a second for the release tails.
SoundFontRenderer.RenderToWavFile(
    synthesizer, sequence, "piano.wav", TimeSpan.FromSeconds(0.5));

// Or play it live, letting the player build the synthesizer at the device's
// own rate - which is the overload to prefer, because a synthesizer built at
// the wrong rate is transposed:
//
//     using var player = new MidiMusicPlayer();
//     player.Load(rate => new ModestSynthesizer(patch, ModestSynthPresets
//         .SettingsFor(ModestSynthPresets.ElectricPianoName, rate)), sequence);
//     player.Play();
```

Turning it on for Decent Sampler instruments:

```csharp
using CodeBrix.Audio.ModestSynth;

ModestSynth.Register();       // once, at application start-up, BEFORE loading an instrument
```

## Using it in a CodeBrix.Platform application

This package has no CodeBrix.Platform add-in of its own: a CodeBrix.Platform application references it
like any other library, and the sound it makes reaches the speakers through
[CodeBrix.Audio's players](audio/playback.md). The
[AudioPlayer add-in](../platform/add-ins/AudioPlayer.md) plays what those players play. As with the
Opus package, the consuming **application** takes this dependency and makes the `Register()` call -
the add-ins never do.

## Pitfalls

- **Register before loading.** Waveforms and effect types are resolved while an instrument is built
  into a synthesizer, not while its file is parsed. `ModestSynth.Register()` after the fact retrofits
  neither an instrument that is already loaded nor a synthesizer that already exists - reload the
  instrument and build the synthesizer again. The symptom is a silent oscillator group and a bypassed
  effect, with the reason in `Problems`: nothing throws.
- **One oscillator per voice.** An oscillator is per-voice state; sharing one across two notes
  produces one note with a confused pitch.
- **`SetSampleRate` before the first `Render`, and not between blocks.** That call is where allocation
  happens; `Render` is deliberately allocation-free and stays that way only if the rate was settled
  first.
- **`pluck1`'s `Reset` **is** the note-on.** Without it the string is silent, because there is nothing
  in the delay line to ring. Rendering a `pluck1` that was never reset returns zeros, not a hum.
- **`pluck1` pitch has limits.** Below 8 Hz or above a third of the sample rate, `SetFrequency`
  throws. That range covers every MIDI note at every ordinary sample rate, but clamp before you call
  it if your source of pitch can produce anything at all.
- **Damping is a decay time, not a tone control.** For a darker string, lower the pluck type or filter
  the output; lowering damping shortens the note.
- **Seeds decide whether layers thicken or double.** Two noise or `pluck1` voices with the same seed
  and the same pitch render the same samples and sum to one louder copy. Set `ModestPatch.Seed` per
  voice, or turn on `RandomPhase`.
- **`RandomPhase` means nothing to noise.** It has no phase to randomize. It matters to sine, saw,
  square, triangle, wavetable and to a `pluck1` excitation, and the format's own guidance is to turn it
  **on** whenever wavetable groups are layered, because voices that all start at phase zero cancel
  each other instead of thickening.
- **A wavetable path is relative to the preset, not to your process.** The format writes it relative to
  the preset file, and `ModestPatch` hands it to the loader as it stands, where a relative path
  resolves against the current directory. Combine it with the preset's own folder first, or set
  `ModestPatch.WavetableTable` and skip the loader entirely. Inside a preset this is already done for
  you: the adapter resolves the path through the instrument's own container, archives included.
- **The first note of a wavetable group reads a file.** Voices are pooled per zone, so a group's first
  few notes build their sources and the first of them decodes the table; after that everything is
  cached and the render path touches nothing. If that first note falls where file I/O is unacceptable,
  warm `WavetableFileCache` before you start playing.
- **A wavetable with no usable file is a sine, not silence,** and it does not throw. Check
  `WavetableOscillator.Problem` and report it; the sound is otherwise indistinguishable from a
  deliberate sine oscillator.
- **Load wavetables through `WavetableFileCache`.** A table costs several times its frame data in
  memory and takes real time to band-limit; doing that per voice instead of per file is the whole
  difference.
- **A harmonic oscillator with no partial levels is a sine at full level,** which is the reference
  player's behavior. Setting any one level ends that, so setting only the fourth partial's level gives
  you the fourth partial alone, not a fourth partial added to a fundamental. And the partial count does
  not add partials, it removes them.
- **Band-limited shapes overshoot slightly.** A saw or square corrected at its edges can exceed 1 for a
  sample or two. Leave headroom rather than assuming a hard bound.
- **`formant` is loud, and it has no parameters.** Give the group its own volume attribute. Every
  `formant-*` attribute a preset writes is ignored, by the reference and by this package alike, and the
  core's feature list still reports the waveform as unrecognized.
- **`fm6op` needs a note, and not only a `Reset`.** `Reset` starts it at full velocity so an
  `IModestOscillator` consumer still gets sound, but velocity only means something once `NoteOn` has
  been called, and nothing is released until `NoteOff`.
- **`fm6op`'s `IsFinished` is usually false forever, and that is correct.** The format's default
  release is the -1 sentinel, which hands the ending to the group's envelope. A standalone caller that
  wants the voice to stop on its own must give at least one carrier a real release, or a four-stage
  envelope whose fourth level is 0.
- **`fm6op` operators 2 to 6 start silent,** and it does not normalize by carrier count. Set the levels
  of the operators your algorithm actually uses, and lower the carriers or the group volume when many
  of them are loud.
- **Out-of-range parameters clamp silently.** That is deliberate and matches the rest of the family,
  but it means a value you set may not be the value you read back.
- **The zone wins over `TrySetParameter`.** Inside a preset the resolved zone is the source of truth,
  and the adapter re-reads it whenever a binding fires, so a value written straight onto a voice stands
  until the next binding change and is then replaced. Drive a preset through its bindings and controls;
  `TrySetParameter` is for a host that has no bindings at all.
- **`ModestSynthesizer` reads the patch once.** Changing the patch afterwards changes nothing you hear
  - build another synthesizer.
- **A synthesizer built at the wrong rate is transposed.** `MidiMusicPlayer` renders through the shared
  device at whatever rate that device settled on, so prefer the factory overload, which is handed that
  rate, over the one that takes a synthesizer you already built.
- **An additive stack still needs headroom.** Full normalization holds the summed peak where a single
  full-level partial's would be; it says nothing about a chord, whose peaks add on top of that.
- **Prepare an effect before the first block.** An effect that was never prepared prepares itself at
  44,100 Hz, which is silently wrong at any other rate, and `Prepare` is the only member that
  allocates.
- **An effect needs two different buffers.** Passing the same array as both channels throws.
- **`wave_shaper`'s output level is 0.1 by default, a 20 dB cut.** If a shaper sounds too quiet, that
  is the format's default doing its job.
- **`highQuality` is not free.** Turn it on where the drive is high and leave it off everywhere else.
- **`stereo_simulator` sums its input to mono.** Putting one at the end of an instrument chain
  collapses whatever stereo image was already there.
- **A gate is random, but it is not unpredictable.** Give every gate its own `Seed` if you want two of
  them to stutter differently, and expect the same dropouts every time you render - which is the point.
- **A phaser with feedback is not level-neutral.** The reference subtracts its feedback, which turns
  the notches into resonant peaks and costs a little level elsewhere; at a mix of 1.0 the resonances
  are all that is left.
- **The wave folder is a pass-through until the signal reaches its fold point.** A preset that raises
  drive and hears nothing change has not reached it yet.

## Samples and tools in the repository

The repository ships two packages and has no sample applications and no demo projects. Everything in
`tools/` is developer tooling and test data: none of it is packed, and none of it is needed to consume
either package. The test suite is the best worked example of every public API.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Add-on tests | Every waveform and every effect, including the spectral assertions that pin the measured behavior | [`tests/CodeBrix.Audio.ModestSynth.Tests`](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tests/CodeBrix.Audio.ModestSynth.Tests) |
| Format coverage survey | Measures which format features a corpus of real libraries actually uses, and how much of it the engine recognizes | [`tools/ds_feature_survey`](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tools/ds_feature_survey) |

Run the tests with `dotnet test CodeBrix.Audio.slnx`. The tests that open a real audio device and make
sound are opt-in, so an ordinary run is silent and headless-safe:

```bash
CODEBRIX_AUDIO_RUN_PLAYBACK_TESTS=1 dotnet test
```

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [src/CodeBrix.Audio.ModestSynth/README.md](https://github.com/ellisnet/CodeBrix.Audio/blob/main/src/CodeBrix.Audio.ModestSynth/README.md) |
| Complete API guide (ships inside the package too) | [src/CodeBrix.Audio.ModestSynth/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/src/CodeBrix.Audio.ModestSynth/AGENT-README.txt) |
| The guide for the library this adds synthesis to | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.Audio.ModestSynth.Tests](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tests/CodeBrix.Audio.ModestSynth.Tests) |

XML documentation ships alongside the assembly.

## License

CodeBrix.Audio.ModestSynth is licensed under the MIT License, and the license is also named in the
package ID (`CodeBrix.Audio.ModestSynth.MitLicenseForever`). License acceptance is required at install
time. The package it depends on, `CodeBrix.Audio.MitLicenseForever`, is MIT as well, and the two are
built and published from one repository at the same version. For the provenance and licensing of open
source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [Decent Sampler instruments](audio/decent-sampler.md) - the engine this package extends, and the register-before-loading rule in context
- [CodeBrix.Audio](CodeBrix.Audio.md) - the library it adds synthesis to
- [SoundFont and SFZ instruments](audio/soundfont-and-sfz.md) - the other engines the standalone synthesizer stands beside
- [ellisnet/CodeBrix.Audio on GitHub](https://github.com/ellisnet/CodeBrix.Audio) - source, tests and tools
