<sub>[CodeBrix](../../../README.md) › [Libraries](../README.md) › [CodeBrix.Audio](../CodeBrix.Audio.md) › MPE</sub>

# MPE from MIDI files

**A performance played on an expressive controller - one where a finger can bend, brighten and swell
each note on its own - is recorded as MIDI Polyphonic Expression: every note gets its own MIDI
channel, and the bends, the slide and the press on that channel belong to that note alone.** A
Standard MIDI File carries all of it, and every sampled instrument format in
[CodeBrix.Audio](../CodeBrix.Audio.md) plays such a file the same way, so one recording sounds alike
through a SoundFont, an SFZ library and a Decent Sampler instrument.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Audio](https://github.com/ellisnet/CodeBrix.Audio) |
| **Packages** | [`CodeBrix.Audio.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |

## What it does

- Reads the MPE content of a Standard MIDI File: zones, per-channel bend and bend range, tuning,
  slide, press, master gestures and lift.
- Applies it identically across the SoundFont, SFZ and Decent Sampler engines, because the whole
  contract lives in one shared model.
- Detects a zone layout automatically when a file carries no configuration message, which is the
  usual case for a clip exported from a sequencer.
- Lets you pin the layout instead when the automatic reading is not what the performance meant.
- Reports what it decided, so a host can show it.
- Opens the same surface to a synthesizer of your own through `IMpeSynthesizer`.

## When to use it

Turn MPE on when the file you are playing was recorded on an expressive controller. Leave it off -
which is the default - when you want the file to play as ordinary MIDI, whatever it looks like. `Off`
is not merely "do nothing": it is the way to **insist** a file plays as plain MIDI, with two semitones
of bend unless RPN 0 says otherwise and no zones at all.

There are no MIDI devices here. This is about MIDI **files**: record the performance in a sequencer,
export the clip, and play the file.

## Getting started

```bash
dotnet add package CodeBrix.Audio.MitLicenseForever
```

```csharp
using CodeBrix.Audio.Synth.Mpe;  // MpeMode and the zone model
```

Two properties turn it on, on `MidiMusicPlayer` and on each synthesizer:

```csharp
music.MpeMode = MpeMode.Auto;         // CodeBrix.Audio.Synth.Mpe
music.MpeMemberBendRange = 48;        // semitones; the controller default
```

## Key concepts

### The modes

| Mode | What it means |
| --- | --- |
| `MpeMode.Off` | Every channel is ordinary MIDI: two semitones of bend unless RPN 0 says otherwise, and no zones. **The default**, and the way to insist a file plays as plain MIDI |
| `MpeMode.Auto` | Read the zones from the file's own configuration message; failing that, infer a lower zone from notes spread over channels 2 to 16 with per-channel bends and nothing on channel 1. **Start here** |
| `MpeMode.LowerZone` | Channel 1 is the master, 2 upward its members |
| `MpeMode.UpperZone` | Channel 16 is the master, 15 downward its members |
| `MpeMode.Both` | Both zones, splitting the fourteen middle channels |

`.MpeLowerZoneMemberCount` and `.MpeUpperZoneMemberCount` pin how many member channels an explicit
mode gets; 0, the default, means fifteen for one zone and seven each for two.

`.MpeLowerZone` and `.MpeUpperZone` read back what a configuration message, or the automatic detector,
actually decided. `MpeZoneInfo` carries `IsActive`, `MasterChannel`, `FirstMemberChannel`,
`MemberCount`, `MasterBendRange` and `MemberBendRange`, with channel numbers 1-based as every
controller manual writes them.

> [!IMPORTANT]
> A file exported from a sequencer normally carries **no** configuration message - the notes are
> spread across channels 2 upward, and nothing more. That is what `MpeMode.Auto` is for, and it is the case to
> expect. If the detector reads a particular file differently from how it was played, pin it with
> `MpeMode.LowerZone` and a member count.

### What a file can say, and what is done with it

| What the file carries | What happens |
| --- | --- |
| Zones | RPN 6 on channel 1 or 16, the MPE Configuration Message, honored live and mid-file in every mode but `Off` |
| Bend | Per channel, fourteen-bit. A member's bend and its zone master's **add**, each over its own range, which is how a global glide rides on top of a per-note one |
| Bend range | RPN 0, per channel, semitones and cents. Sent on a member it configures every member of that zone. Without it, members of an active zone use `MpeMemberBendRange` and everything else uses MIDI's own two semitones |
| Tuning | RPN 1 (fine) and RPN 2 (coarse), added to the note's pitch. RPN null and the data increment and decrement controllers behave as the MIDI specification says |
| Slide | CC 74 per channel, read as that note's timbre |
| Press | Channel pressure per channel, and polyphonic key pressure honored as per-note pressure when a file carries it |
| Master gestures | A zone master's controllers reach every note in the zone: a pedal or other switch applies zone-wide, and a continuous controller applies to any member that has not sent its own |
| Lift | Note-off velocity, captured per note and readable through `GetReleaseVelocity(channel, key)` - 0-based channels, as MIDI messages carry them, and -1 when nothing is loaded. The observe-only message hook sees it live, as a note-off's second data byte |

Overlapping notes on one member channel follow the MPE rule that the **newest** note owns the channel:
an older note freezes its bend and its expression where they were when it lost the channel, and gets
them back if it outlives the newer one.

### What each format does with the expression

A format can only use what it declares. Bend, tuning, zones, master combination and lift are identical
in all three; the difference is what a preset can wire the slide and the press **to**.

| Format | What it does |
| --- | --- |
| SFZ | Everything. CC 74 and the aftertouch sources feed the region's own `_onccN` modulation matrix and its `locc` / `hicc` ranges, so a library that already responds to a controller's slide and press responds to a performance's. A region's `bend_up` / `bend_down` still says how far it bends; an MPE range **scales** that against MIDI's two semitones, so a region asking for an octave still bends six times as far as a plain one |
| SoundFont | Press deepens a note's vibrato, which is the destination the SoundFont modulator model itself names for channel pressure. The format names no destination for CC 74, so the slide is stored and readable (`MpeTimbre`) but changes no sound. Volume, expression, pan, modulation and the pedal follow the zone rules |
| Decent Sampler | The `mpeTimbre` and `mpePressure` modulators read the note's own slide and press, and a `midiCC` modulator written with `channel="voice"` reads any controller the same way |

No sampled format defines what release velocity should **do**, so a lift changes no sound anywhere; it
is captured for the host to use.

### Where the surface lives

`MidiMusicPlayer` carries `MpeMode`, `MpeMemberBendRange`, `MpeLowerZoneMemberCount`,
`MpeUpperZoneMemberCount`, `MpeLowerZone`, `MpeUpperZone` and `GetReleaseVelocity(channel, key)`, and
forwards them to whichever instrument format is loaded.

Each synthesizer carries the same members itself, so you can set them without a player.
`SoundFontSynthesizer`, `SfzSynthesizer` and `DecentSamplerSynthesizer` all expose `MpeMode`,
`MpeMemberBendRange`, the member counts, `MpeLowerZone`, `MpeUpperZone`, `ReleaseVelocity`,
`MpeTimbre` and `MpePressure`.

A synthesizer of your own joins in by implementing `CodeBrix.Audio.Synth.Mpe.IMpeSynthesizer`.
`MidiMusicPlayer` then forwards the same settings to it and reads its zones and lift velocities.

## Examples

Playing an exported clip that carries no configuration message, letting the detector decide, and
reading back what it decided:

```csharp
using CodeBrix.Audio.Playback;
using CodeBrix.Audio.Synth;
using CodeBrix.Audio.Synth.Mpe;

using var music = new MidiMusicPlayer();
music.MpeMode = MpeMode.Auto;         // the file may be an MPE clip
music.MpeMemberBendRange = 48;        // semitones, when the file is silent
music.Load(instrument, new MidiSequence("performance.mid"));
music.Play();
```

Setting the same two properties straight on a synthesizer, without a transport:

```csharp
synthesizer.MpeMode = MpeMode.Auto;        // also on MidiMusicPlayer
synthesizer.MpeMemberBendRange = 48;       // semitones, when the file is silent
```

## Pitfalls

- **`MpeMode.Off` is the default.** A file recorded on an expressive controller plays as ordinary MIDI
  until you say otherwise, which is deliberate - it means nothing changes for a file that was never
  meant to be read that way.
- **`Auto` is a detector, not an oracle.** If it reads a particular file differently from how it was
  played, pin the layout with `LowerZone`, `UpperZone` or `Both` plus a member count.
- **Channel numbering differs between the zone model and the messages.** `MpeZoneInfo` uses 1-based
  channels, as controller manuals write them; `GetReleaseVelocity` takes 0-based channels, as MIDI
  messages carry them.
- **A lift changes no sound.** No sampled format defines a consumer for release velocity, so it is
  captured and reported rather than heard.
- **There is no MIDI device input.** Record the performance in a sequencer, export the clip, and play
  the file.

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/AGENT-README.txt) |
| MPE tests: one performance through each instrument format, with the pitches measured against the bend arithmetic | [tests/CodeBrix.Audio.Tests](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tests/CodeBrix.Audio.Tests) |

## License

CodeBrix.Audio is licensed under the MIT License, and the license is also named in the package ID
(`CodeBrix.Audio.MitLicenseForever`). For the provenance and licensing of open source code included in
this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [SoundFont and SFZ instruments](soundfont-and-sfz.md) - two of the three engines this applies to
- [Decent Sampler instruments](decent-sampler.md) - the third, and its `mpeTimbre` and `mpePressure` modulators
- [MIDI files](midi-files.md) - reading the file the performance arrives in
- [CodeBrix.Audio](../CodeBrix.Audio.md) - the front door of the audio section
