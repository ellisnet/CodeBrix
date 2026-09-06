<sub>[CodeBrix](../../../README.md) › [Libraries](../README.md) › [CodeBrix.Audio](../CodeBrix.Audio.md) › MIDI files</sub>

# MIDI files

**This page covers the MIDI side of [CodeBrix.Audio](../CodeBrix.Audio.md): reading and writing
Standard MIDI Files, the event hierarchy, tolerant reading and the `Problems` report, tempo maps, the
two types named for MIDI files and which one you want, and the two message hooks a player exposes onto
the notes as they play.** MIDI arrives as a file, or as messages your own code sends; nothing here
opens a MIDI input or output port.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Audio](https://github.com/ellisnet/CodeBrix.Audio) |
| **Packages** | [`CodeBrix.Audio.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |

## What it does

- Reads a Standard MIDI File into an editable model - `MidiFile` over `MidiEventCollection` - and
  writes one back out with `MidiFile.Export`.
- Reads a file into an immutable, playable sequence - `MidiSequence` - whose absolute-time messages a
  sequencer and a player consume.
- Reads tolerantly by default, so a file that departs from the specification loads and reports what
  was odd about it rather than refusing.
- Validates instead, on request, through `MidiReadMode.Strict`.
- Carries the tempo map a sequence was flattened with, and the textual meta events a file held.
- Exposes the whole event hierarchy: notes, controllers, program changes, pitch wheel, aftertouch,
  system exclusive, and the meta events for tempo, time signature, key signature and text.
- Offers two hooks onto the messages a sequence plays - one that observes and one that replaces
  delivery.

## When to use it

Reach for `MidiFile` when the file is the subject: you are reading it, inspecting it, editing it or
writing one. Reach for `MidiSequence` when the music is the subject and something is going to play it
through a [SoundFont, SFZ or Decent Sampler instrument](soundfont-and-sfz.md).

Both together is the intended pattern, not a workaround. Parse the same file twice - once as
`MidiSequence` to play it, once as `MidiFile` to read its tempo, time signature or markers. MIDI files
are kilobytes; this costs nothing.

What this layer does not do: it opens no MIDI device. An expressive performance reaches the engines by
being recorded in a sequencer, exported as a file and played - see [MPE from MIDI files](mpe.md).

## Getting started

```bash
dotnet add package CodeBrix.Audio.MitLicenseForever
```

```csharp
using CodeBrix.Audio.Midi;       // MIDI file read/write + event hierarchy
using CodeBrix.Audio.Synth;      // MidiSequence, the playable form
```

Writing a file and reading it back:

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

## Key concepts

### Two types named for MIDI files

| Type | Namespace | What it is |
| --- | --- | --- |
| `MidiFile` | `CodeBrix.Audio.Midi` | The editable file model. Read it, edit the event collection, write it back out |
| `MidiSequence` | `CodeBrix.Audio.Synth` | The immutable decoded sequence. You play it: flattened absolute-time messages, no tracks, no meta events, no editing, no writing |

Convert with `MidiSequence.FromEvents(MidiEventCollection)` - build or edit in the `Midi` model, then
play it. There is deliberately no reverse conversion: the sequence has already discarded track
structure and non-playable meta events, so converting back would silently lose them.

```csharp
using CodeBrix.Audio.Midi;
using CodeBrix.Audio.Synth;

var events = new MidiEventCollection(1, 120);
events.AddEvent(new NoteOnEvent(0, 1, 60, 100, 120), 1);
events.AddEvent(new NoteEvent(120, 1, MidiCommandCode.NoteOff, 60, 0), 1);
events.PrepareForExport();

var sequence = MidiSequence.FromEvents(events);   // editable model -> playable sequence
// ...then hand `sequence` to MidiMusicPlayer.Load.
```

### The event hierarchy

`MidiEvent` is the base of `NoteOnEvent`, `NoteEvent`, `ControlChangeEvent` (with the
`MidiController` enum), `PatchChangeEvent`, `PitchWheelChangeEvent`, `ChannelAfterTouchEvent` and
`SysexEvent`, plus the `MetaEvent` subclasses `TextEvent`, `TempoEvent`, `TimeSignatureEvent` and
`KeySignatureEvent`. `MidiCommandCode` is the status-byte enum and `MidiMessage` builds a raw message
from its parts. `MidiEventCollection` is the per-track collection used for both reading and writing.

### Reading a file that breaks the rules

Plenty of real MIDI files depart from the specification, and a machine-written one almost always does:
a key signature outside the legal range, a note-on with no note-off, a meta event whose payload does
not match its declared length, a chunk that is not `MTrk`. Both readers - `MidiFile` and
`MidiSequence` - read such a file rather than refusing it, and say what was odd about it:

```csharp
var file = new MidiFile("machine-written.mid");     // does not throw
foreach (var problem in file.Problems) { ... }      // one line each
```

The `Problems` contract, which this library shares with the SFZ engine and with a loaded stems song:

- one human-readable line per departure, in the order they were found;
- empty means the file followed the specification;
- never thrown, and capped so a corrupt file cannot become a memory problem;
- floods are aggregated per track - "Track 1: 12 note on event(s) had no note off" - rather than one
  line per note;
- a meta event this package does not model is not a problem: its bytes are kept verbatim and written
  back unchanged, so nothing was lost.

The two readers legitimately report different problems for one file, because they model different
things. `MidiFile` models key signatures and reports an out-of-range one; `MidiSequence` does not
model them at all and has nothing to say. Do not compare the two lists.

> [!WARNING]
> Tolerance changes what you get back where that is the only way to be useful. A dangling note-on has
> a note-off synthesized at the end of its track and inserted into the collection, so the file exports
> again as a valid file and note length works. Anything counting events must expect that, or read with
> `MidiReadMode.Strict`.

Strict mode is there for a tool that wants to validate rather than play:

```csharp
new MidiFile(path, MidiReadMode.Strict)        // throws on the first fault
new MidiSequence(path, MidiReadMode.Strict)
```

The older `new MidiFile(path, strictChecking: true)` means exactly the same. `MidiReadMode` -
`Tolerant`, the default everywhere, or `Strict` - is the one option both readers share. Both readers
need a seekable stream.

### Tempo maps, text metas and what a sequence drops

`MidiSequence` does not carry tempo, time signature, markers or track names. It consumes tempo changes
while merging tracks - they are baked into the message time stamps and dropped - and never parses time
signature, markers or track names at all. Read those from `MidiFile`:

```csharp
var file = new MidiFile(path, strictChecking: false);   // CodeBrix.Audio.Midi
var bpm  = file.Events[0].OfType<TempoEvent>().First().Tempo;
var sig  = file.Events[0].OfType<TimeSignatureEvent>().FirstOrDefault();
```

Two members survive the flattening on the sequence itself. `MidiSequence.TextMetas` holds the textual
meta events the file carried, bytes preserved, so a caller can identify a file without reading it
twice. `MidiSequence.TempoMap` holds the tempo map the loader applied while flattening the file to
absolute time - and because loading a sequence consumes the tempo events, that property is the only
place the map survives. `MidiTempoMap` and `MidiTempoChange` are its types, and
`CodeBrix.Audio.Synth.TempoSource` is how live musical time reaches a synthesizer that needs it, such
as the [Decent Sampler](decent-sampler.md) arpeggiator.

### The two message hooks

`MidiMusicPlayer` exposes two hooks onto the messages a sequence plays. They look similar and do
opposite things.

| What you want | The hook |
| --- | --- |
| To **react** to the music - a drum hit shaking the screen, a note spawning a particle, karaoke, a rhythm game | `.MidiMessageProcessed`, observe-only, runs after the message has been delivered, cannot break playback. Almost always the one you want |
| To **change** the music as it plays - transpose, re-channel, suppress, remap | `.MidiMessageFilter`, which replaces delivery: your hook now owns sending the message on |

> [!WARNING]
> The filter was always a modifying hook: when it is set, the sequencer does not call the synthesizer
> itself. A filter that inspects a message and returns without calling `ProcessMidiMessage` on the
> synthesizer it was handed silences the music completely. That reads as a fault in the player rather
> than in the hook, which is exactly why the observe-only hook exists next to it.

Both run on the real-time audio thread, so both must be fast, allocation-free, and must never block or
touch UI. Do not call back into the player from either one; it takes the same lock the audio thread is
already holding, and deadlocks. Hand data to your own thread and act on it there.

The synthesizer passed to a filter is safe to use from inside that call only - the lock that
serializes it against rendering is held for the duration. Never store it for later. To send messages
from your own thread, use `MidiMusicPlayer.SendMidiMessage` and the `SetChannel*` helpers, which take
the lock properly. There is deliberately no property handing back the `IMidiSynthesizer`: it is not
thread-safe, and the lock that makes it safe is not reachable from outside the library.

## Examples

Reacting to the notes as they play, and mixing a layer live:

```csharp
using CodeBrix.Audio.Playback;
using CodeBrix.Audio.Synth;

var music = new MidiMusicPlayer();
music.Load(soundFonts.Get("GeneralUser.sf2"), new MidiSequence("battle.mid"));

// Observe-only: cannot break playback. Runs on the AUDIO THREAD, so do the
// least possible here and let your own thread do the work.
music.MidiMessageProcessed = (channel, command, note, velocity) =>
{
    if (command == 0x90 && velocity > 0 && channel == 9)   // channel 10 = drums
        Volatile.Write(ref _drumHitPending, 1);            // your thread reads this
};

music.Play();

music.SetChannelVolume(3, 0.0f);   // drop the lead layer out...
music.SetChannelVolume(3, 1.0f);   // ...and bring it back
music.Speed = 0.75f;               // slow-motion, same pitch
```

Transposing the whole sequence up an octave with the other hook - note that this one owns delivery:

```csharp
music.MidiMessageFilter = (synth, channel, command, data1, data2) =>
    synth.ProcessMidiMessage(
        channel, command,
        command is 0x90 or 0x80 ? data1 + 12 : data1,
        data2);
```

## Pitfalls

- **Call `MidiEventCollection.PrepareForExport()` before `MidiFile.Export()`,** every time. A type-0
  collection may contain only one track - `Export` throws otherwise - so use type 1 for multi-track
  files. `NoteOnEvent` auto-creates its paired note-off.
- **Tolerant reading changes the event list.** Reading tolerantly, which is the default, closes a
  dangling note-on by inserting a synthesized note-off before the end of its track. Code that counts
  events, or that assumes the reader hands back exactly what the file held, must read with
  `MidiReadMode.Strict` or expect the extra event.
- **Do not compare `MidiFile.Problems` with `MidiSequence.Problems`.** They model different things.
- **The audio thread is real-time.** Both player hooks run on it: no blocking, no I/O, no UI.
- **A filter that does not re-deliver silences the music.** Use `MidiMessageProcessed` when you only
  want to watch.
- **Read tempo and time signature from `MidiFile`, not from `MidiSequence`.** The sequence has already
  consumed the tempo events; only `TempoMap` survives.

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/AGENT-README.txt) |
| MIDI tests, one file per event type, plus the leniency suites | [tests/CodeBrix.Audio.Tests](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tests/CodeBrix.Audio.Tests) |

## License

CodeBrix.Audio is licensed under the MIT License, and the license is also named in the package ID
(`CodeBrix.Audio.MitLicenseForever`). For the provenance and licensing of open source code included in
this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [SoundFont and SFZ instruments](soundfont-and-sfz.md) - what turns a sequence into sound
- [MPE from MIDI files](mpe.md) - reading an expressive performance out of a Standard MIDI File
- [Multi-track songs and stems](multi-track-and-suno.md) - several MIDI parts on one transport
- [CodeBrix.Audio](../CodeBrix.Audio.md) - the front door of the audio section
