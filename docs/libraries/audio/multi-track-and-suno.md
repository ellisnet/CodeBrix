<sub>[CodeBrix](../../../README.md) › [Libraries](../README.md) › [CodeBrix.Audio](../CodeBrix.Audio.md) › Multi-track songs and stems</sub>

# Multi-track songs and stems

**`MultiTrackPlayer` plays several tracks as one song on one transport, sample-accurately: a track can
be a recording, a MIDI performance through any of the three sampled instrument formats, or both at
once - and when it holds both, which one is heard is a property you can change while the music runs.**
That is what a stems set is, one track per part with the recording and the transcription side by side,
and `SunoStemsLoader` turns a stems download into exactly that model. This page also shows the plain
way to play a whole downloaded song, which needs none of it.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Audio](https://github.com/ellisnet/CodeBrix.Audio) |
| **Packages** | [`CodeBrix.Audio.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later. A MIDI part needs an instrument you supply - a General MIDI SoundFont, or an SFZ or Decent Sampler instrument of your own |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |

## What it does

- Plays several tracks in lockstep inside one component on the shared output, so tracks cannot drift
  and a seek moves all of them at once.
- Gives every track both a recording and a MIDI performance where both exist, and crossfades between
  them rather than restarting.
- Carries per-track gain, pan, mute, solo and signed offsets, plus a separate offset that moves only
  the MIDI rendition.
- Applies a percussion rule for the zero-length notes drum parts are written with.
- Measures, on request, how loud each track's recording is against a render of that same track's MIDI,
  so a synthesized part sits where the recording sat in the mix.
- Renders the whole mix offline, with no audio device, to a buffer or to a WAV file.
- Merges every MIDI part into one General MIDI file.
- Loads a stems download - the zip or the extracted folder - into a song model, reads each part's
  MIDI tolerantly, reports note counts and coverage, and measures how far each transcription sits from
  its own recording.

## When to use it

Use `MultiTrackPlayer` when the song is made of parts you want to control separately: a stems set, a
backing track with a soloable melody, a game arrangement whose layers come in and out. Use
`SunoStemsLoader` when those parts arrive as a stems download, because it does the file discovery, the
tolerant MIDI reading, the vocabulary defaults and the alignment measurement for you. "Suno" is
Suno, Inc.'s name, and appears here only to say what the files are.

Use neither to play a whole downloaded song: that is an ordinary MP3 or WAV and
[`AudioFilePlayer`](playback.md) plays it. The next section shows exactly how.

### Playing a whole downloaded song

A whole song is the ordinary `<Title>.wav` or `<Title>.mp3` download, and it is a long recording like
any other. Play it with `AudioFilePlayer`, the media player with a transport. Do not use
`SoundEffectClip` for it: that type decodes a clip whole into memory for short, overlapping one-shots.

```csharp
using CodeBrix.Audio.Playback;

using var player = new AudioFilePlayer();
player.PlaybackEnded += (sender, e) => Console.WriteLine("finished");
player.Load("/music/My Song.wav");        // or "/music/My Song.mp3"
player.Volume = 0.8f;                     // 1.0 is unity gain
player.Play();

// The transport, whenever you want it:
player.Pause();                           // Play() resumes from here
player.Seek(TimeSpan.FromSeconds(60));    // jump to 1:00
Console.WriteLine($"{player.Position} of {player.Duration}");
player.IsLooping = true;                  // wrap at the end instead of ending
player.Stop();                            // back to the start, stopped
```

`Load` identifies the format from the file's content rather than its extension; `Duration` is known as
soon as it returns; the file is decoded in chunks as it plays, so a four-minute WAV is not held in
memory; the player resamples to the output device, so the download's own rate does not matter; an
MP3's encoder delay and padding are trimmed; and `PlaybackEnded` is raised on the
`SynchronizationContext` that was current when the file was loaded, so do not do slow work in the
handler. For a whole song either format is fine - the WAV advice below matters for **stems**, which
have to line up with each other. In a CodeBrix.Platform application, the
[AudioPlayer add-in](../../platform/add-ins/AudioPlayer.md) wraps this same player as a XAML element
with a bindable position.

## Getting started

```bash
dotnet add package CodeBrix.Audio.MitLicenseForever
```

```csharp
using CodeBrix.Audio.Playback;       // MultiTrackPlayer, AudioTrack, MidiTrack, TrackSource
using CodeBrix.Audio.Playback.Suno;  // SunoStemsLoader and the song model
```

Building a song by hand is four lines:

```csharp
var player = new MultiTrackPlayer();
player.Add(new AudioTrack("drums.wav", "Drums"));
player.Add(new MidiTrack(sequence,
    rate => new SoundFontSynthesizer(generalMidi, rate), "Bass"));
player.Prepare();
player.Play();
```

## Key concepts

### The track model

| Type or member | What it is |
| --- | --- |
| `AudioTrack(path or Stream)` | A recording: WAV, MP3, Ogg Vorbis or FLAC, identified from the file's **content** rather than its extension, plus anything registered with `AudioFileReaderRegistry` |
| `MidiTrack(sequence, factory)` | A performance plus the instrument that renders it. The factory is called with the sample rate the synthesizer must render at, and may be called more than once and from a worker thread |
| `track.SetAudioSource(...)` | Gives a MIDI track a recording as well |
| `track.SetMidiSource(...)` | Gives an audio track a performance as well |
| `track.ActiveSource` | `TrackSource.Audio` or `TrackSource.Midi`. Setting it while playing is a 20 ms crossfade, not a restart: both sources are rendered every block, so the silent one is always at the right position |

### Per-track controls

| Control | What it does |
| --- | --- |
| `Gain` | Linear, 1.0 unity. Not decibels |
| `MidiSourceGain` | An extra linear gain applied only while the MIDI source is the one being heard, so a synthesized part can be matched to the recording without disturbing `Gain` |
| `Mute` / `Solo` | While any track is soloed only soloed tracks sound; `Mute` wins |
| `Pan` | -1 left, 0 center, +1 right. A **balance** law: it attenuates the far channel and leaves the near one alone, so a centered stereo track passes through untouched |
| `Offset` | Signed; positive **delays** the whole track. Read at `Prepare`, `Play`, `Stop` and `Seek`, not per block |
| `MidiSourceOffset` | Signed; positive delays the MIDI rendition **relative to** the track's own recording. This is the alignment lever, and it is separate from `Offset` because a set of recordings is already in step with itself and must not be moved |
| `MinimumNoteHold` | The percussion rule, 60 ms by default: a note-off that arrives sooner than this is deferred until the note has sounded that long. A note written longer is untouched |
| `IgnoreNoteOff` | Discards note-offs entirely. Right for one-shot percussion whose samples already have the correct length; wrong for a sustaining instrument, which will then never stop a note |
| `GmProgram` / `IsPercussion` | Used by the merged MIDI export, not by playback |

The percussion rule exists because drum parts are routinely written with zero-length notes: the
note-off arrives a tick or two after the note-on, because a hit has no duration. Played literally,
every drum is a click cut off in its attack.

### Reaching one track

`player["Drums"]` returns the track of that name, matched case-insensitively and ignoring surrounding
space - the same matching the song model uses. Two tracks of one name resolve to the first one added.
`player.FindTrack(name)` does the same and returns null when there is no such track;
`player.TryGetTrack(name, out var track)` does it as a bool; and `player.Tracks` is every track, in the
order they were added.

The indexer **throws** `KeyNotFoundException` when the name is not in the song, and the message lists
the names that are - a mistyped stem name is a mistake in your code, not a state to handle.

```csharp
player["Bass"].ActiveSource = TrackSource.Midi;
if (player.TryGetTrack("Backing Vocals", out var backing))
{
    backing.Gain = 0.6f;
}
```

### The transport

`Prepare`, `Play`, `Pause`, `Stop`, `Seek`, `Position`, `Duration`, `IsLooping`, `Volume` and
`PlaybackEnded` behave as they do on the other players. `Duration` is the last moment any track
reaches, offsets included, and a track contributes the **longer** of its two sources - so switching a
source never changes the song's length. `Tail`, 400 ms by default, is how much longer the mix keeps
rendering past `Duration` so release tails ring out; set it to `TimeSpan.Zero` when you want exact
arithmetic. `TempoSource` carries live musical time, fed from the first MIDI track's tempo map, and a
song with no MIDI in it reports 120 BPM.

### Matching the levels

`AutoSetRelativeTrackLevels`, false by default, measures on a worker the RMS of each track's recording
against an offline render of that same track's MIDI, and writes the ratio into `MidiSourceGain`. The
balance between the parts then follows the original recording whichever source each track is playing.
`Gain` is never touched, and while the option is off nothing writes a gain you did not set.

It costs a full decode and a full synthesis pass per track, so with the option on `Prepare` starts it
on a worker and does not wait for it. `LevelMeasurement` is the task it runs on, and awaiting that is
how you know the gains are in:

```csharp
player.AutoSetRelativeTrackLevels = true;
player.Prepare();
await player.LevelMeasurement;              // the gains are in when this
player.Play();                              //   returns
```

`LevelMeasurement` is **never** null. Until a measurement starts it is an already-completed task, so
that await is safe on any player and returns at once when there is nothing to wait for - no null
check, no guard. `MeasureRelativeTrackLevelsAsync()` runs it on a worker with the option left off, and
`MeasureRelativeTrackLevels()` blocks the calling thread until done. Call either again after changing
an instrument.

### Offline, with no audio device

`Render(sampleRate)` returns the whole mix as interleaved stereo float, and
`RenderToWav(path or Stream)` writes it out. Both build their own decoders and synthesizers at the rate
you ask for, render, and release them, so rendering while the same song plays is legitimate. `Volume`
and every per-track control apply; nothing is limited or normalized, so a mix that adds up past 1.0
comes back past 1.0, and a whole song comes back as one large float array.

### One merged General MIDI file

`ExportMergedMidi(path or Stream)` writes every MIDI track into one type 1 Standard MIDI File at 480
ticks per quarter note: the shared tempo map, one track per part on its own channel with a track-name
meta event and a program change, percussion on channel 10, and each track's `Offset` and
`MidiSourceOffset` already applied. The percussion rule is applied to the exported notes as well,
because a file full of zero-length notes is silent in every sequencer that opens it. It returns one
human-readable line per thing that could not be honored and throws nothing.

### Loading a stems download

A stems download is a `<Title> Stems.zip` holding one WAV, and optionally one MP3 and for some parts
one `.mid`, per instrument. `SunoStemsLoader` reads that zip **in place** - or the same files extracted
to a folder - and hands you a model `MultiTrackPlayer` plays.

```csharp
var song = SunoStemsLoader.Load(path);                 // zip or folder
var song = SunoStemsLoader.Load(path, options);
var song = await SunoStemsLoader.LoadAsync(path);      // use this from a UI
```

Loading lists the files, reads each `.mid`, and reads each WAV's **header**. It does not decompress
any audio, so a full-length export loads in well under a second, and a stem's audio is materialized
the first time something asks for it.

`Load` throws only for a path that is not there, a blank path, and a file that is not a readable zip.
Nothing about the **content** of an export throws: everything that could not be honored is one
human-readable line in `song.Problems`, and for a line about one part, on that stem's own `Problems`.

| Type | What it carries |
| --- | --- |
| `SunoSong` | `Title` (taken from the file names, never from the MIDI meta), `Stems`, `AudioStems`, `MidiStems`, `Duration`, `TempoMap`, `InitialBeatsPerMinute`, `FullMixPath`, `CacheFolder`, `Problems`, `Options`, `song["Drums"]` by name, and `ClearCache()` |
| `SunoStem` | `Name`, `HasWav` / `HasMp3` / `HasAudio` / `HasMidi`, `Midi` (a `MidiSequence`), `GmProgram`, `Channel`, `IsPercussion`, `NoteCount`, `MidiCoverage`, `Duration`, `AudioSampleRate`, `AudioChannels`, the alignment members below, and `GetAudioPath()` / `OpenAudio()` - WAV preferred, MP3 fallback - with the WAV- and MP3-specific forms of both |
| `SunoLoadOptions` | What to do while loading: extraction, alignment, note hold, tolerances, and the General MIDI SoundFont |
| `SunoPlayerOptions` | What to do when building a player: instruments, alignment, percussion, level matching |
| `SunoStemDefaults` | The twelve known stem names and their General MIDI defaults |

The stem vocabulary is **open**. The twelve known names - Vocals, Backing Vocals, Drums, Percussion,
Bass, Guitar, Keyboard, Piano, Synth, Strings, Brass, FX - carry a General MIDI program and channel
each; a name outside the list still loads and plays, gets program 0 on channel 1, and is reported once.
Where a stem has MIDI, the program change and channel in that file win over the vocabulary, and a
channel of 10 also sets `IsPercussion`.

### Getting the download right

In the exporting service, choose "Extract Stems and MIDI", with split mode "Auto split" - the service's
own list of twelve instruments, and the only split mode supported - range "Full Song", and download
options with WAV and MIDI checked. MP3 is optional. Set tempo to "Follow tempo changes": it keeps every
tempo shift and is truer to the song, where a fixed tempo flattens the map. Keep the resulting
`<Title> Stems.zip` as it is or extract it; either form loads.

> [!TIP]
> Get the WAV files. They are a significant quality-of-life improvement over the MP3s and they line up
> exactly with each other, where an MP3 carries encoder delay and padding - which this package trims,
> but which is one more thing between you and the music. A stems set with no WAV loads and plays; it
> is a worse starting point.

### Near-empty MIDI stems, and what coverage is for

A `.mid` beside a stem says nothing about whether it is worth playing. Real exports contain a backing
vocal transcription of three notes next to a full-length recording. `MidiCoverage` is the fraction of
the song during which that stem has a note sounding, 0 to 1, and it is the number to decide with:
three notes in four minutes measures about 0.001, while a complete part sits between 0.1 and 0.9.
`NoteCount` is the raw count. Every note counts as sounding for at least
`SunoLoadOptions.MinimumNoteHold`, or a complete drum transcription would measure as covering none of
the song.

### Alignment

A machine transcription does not land exactly on the audio it was transcribed from. The offset is
**constant** across a song - it does not drift - but it differs from song to song and from part to
part within one song, by as much as a quarter of a second either way. So the loader measures it, per
stem, against that stem's own recording, on worker threads.

| Member | What it says |
| --- | --- |
| `stem.AlignmentOffset` | The offset to apply. Settable: this is where you put a hand-measured or user-entered value |
| `stem.MeasuredAlignmentOffset` | What this stem's own measurement said |
| `stem.AlignmentMeasured` | Whether it was measured at all |
| `stem.AlignmentConfidence` | 0 to 1 |
| `stem.AlignmentIsReliable` | Whether the estimator stands behind it |
| `stem.AlignmentIsFallback` | Whether `AlignmentOffset` holds the song's fallback rather than this stem's own answer |
| `stem.AlignmentWindow` | How far the search looked, either way |

The sign convention is the one thing to get right:

```text
audioTime = midiTime + AlignmentOffset
```

A **positive** offset means the audio lags the MIDI, so the MIDI has to be delayed by that much to
line up. A player built from the song puts it straight into each track's `MidiSourceOffset`, which
moves the synthesized rendition and leaves the recording where it is.

A stem whose own measurement is not reliable - and a stem with MIDI and no audio of its own - takes
the song's **fallback**: the median of the offsets of the stems that were measured reliably, or zero
when none were. In practice the drum stem is the one that measures reliably and the rest of the song
follows it, which is usually right and is always visible through `AlignmentIsFallback`.

> [!WARNING]
> Cross-correlating a part against its own recording cannot always tell one beat from the next: on a
> repeating pattern every subdivision is a peak of the same comb, and the tallest is not always the
> true one. The estimator arbitrates between near-equal peaks on a coarser measure - the shape of
> where the part plays against where the recording has energy - and where that cannot decide, it
> lowers the confidence rather than pretending. Treat a reliable measurement on a drum or percussion
> part as trustworthy and anything else as advisory. The value is exposed and overridable per stem and
> per track, and `MeasureAlignment = false` turns the whole thing off.

The estimator is public on its own: `MidiAudioAlignment.Estimate(...)` takes note-on times and mono or
interleaved audio and returns a `MidiAudioAlignmentResult` carrying `OffsetSeconds`, `Confidence`,
`IsReliable` and `CandidatePeakCount`, and `MidiAudioAlignment.GetNoteOnTimesSeconds(MidiSequence)`
supplies the first argument. Call it on a worker: it is far too slow for a render callback.

### Building the player from a song

```csharp
var player = song.CreatePlayer();                    // recordings only
var player = song.CreatePlayer(instrumentFactory);   // parts can play as MIDI
var player = song.CreatePlayer(new SunoPlayerOptions { ... });
var player = MultiTrackPlayer.Load(song, options);   // the same thing
```

You get one track per stem, in the model's own order, named after the stem. Every track starts on its
stem's **recording**, which is the default mix: the song as it was downloaded. Where a stem also has
MIDI and an instrument can be built for it, the same track carries the MIDI as its second source, so
switching a part to a synthesized rendition is a property change. A stem with MIDI and no recording
becomes a MIDI-only track; a stem with MIDI, no recording and no instrument is not added at all.

| `SunoPlayerOptions` member | What it does |
| --- | --- |
| `InstrumentFactory` | `Func<SunoStem, int, IMidiSynthesizer>`: build the instrument for one stem at one sample rate. It may be called more than once and from a worker, so never hand out the same synthesizer twice - share the SoundFont or the SFZ instrument behind them instead |
| `GeneralMidiSoundFontPath` | Used when there is no factory. Falls back to the path on `SunoLoadOptions`, so a song loaded with one needs no player options at all. The SoundFont is loaded once through a `SoundFontCache` and shared by every track |
| `SoundFontCache` | Which cache to load it through; null uses `SunoPlayerOptions.SharedSoundFonts`, a process-wide one you can `Clear()` |
| `IncludeMidiSources` | Attach the MIDI at all. True by default. A track holding two sources renders both on every block, which is what makes a switch seamless and is also the main cost; turn it off for a player that will only ever play the recordings |
| `ApplyAlignmentOffsets` | True by default |
| `IgnoreNoteOffOnPercussion` | False by default: percussion parts hold each note for `SunoLoadOptions.MinimumNoteHold` instead |
| `AutoSetRelativeTrackLevels` | Passed to the player |

### The General MIDI SoundFont

A MIDI stem needs an instrument, and the ordinary choice is one General MIDI SoundFont for the whole
song, applying each stem's own program and channel. FluidR3_GM is the recommended one: it is MIT
licensed, may be redistributed with your application, and covers all 128 programs and the standard
drum kit that a stems export's program numbers refer to. TimGM6mb is smaller and is what many Linux
distributions install, but it is GPL - nothing in this family ships it, no fixture references it, and
you should not put it in a package either. Point the loader or the player at whichever file **your**
application is licensed to distribute.

Percussion goes to channel 10 - a stems export writes its drum and percussion parts there already -
and those parts get the percussion rule, because their notes are written with no length at all.

### Where the extracted files go

Audio readers need a seekable stream, so a zip's entries are extracted on demand. By default they go
to a per-song folder under `SunoStemsLoader.DefaultCacheRoot`, which is
`<temp>/CodeBrix.Audio/SunoStems`, keyed by the download's path, size and last-write time, so loading
the same download again extracts nothing. `song.CacheFolder` says where; `song.ClearCache()` removes
what that song extracted and `SunoStemsLoader.ClearCache()` removes the whole root. Point
`SunoLoadOptions.CacheFolder` somewhere else to keep the files with your project.

`SunoZipExtraction.Memory` decompresses entries into memory instead and writes nothing to disk. A
full-length export is a great deal of WAV, and `GetWavPath` / `GetAudioPath` then throw because there
is no path to give, so this is for hosts that cannot write to disk rather than a default.

### Taking the song somewhere else

`song.ExportMergedMidi(path)` writes every MIDI stem into **one** General MIDI file - each part on its
own channel, percussion on channel 10, the shared tempo map, each part's alignment already applied. It
needs no instrument and no audio, so it works on a song loaded with nothing configured.

## Examples

Play the vocal from its recording and every other part through a General MIDI SoundFont, with the
levels of the original mix:

```csharp
using CodeBrix.Audio.Playback;
using CodeBrix.Audio.Playback.Suno;

var song = SunoStemsLoader.Load(@"D:\Downloads\My Song Stems.zip");
foreach (var problem in song.Problems)
{
    Console.WriteLine(problem);          // never thrown; usually empty
}

using var player = song.CreatePlayer(new SunoPlayerOptions
{
    GeneralMidiSoundFontPath = @"D:\SoundFonts\FluidR3_GM.sf2",
    AutoSetRelativeTrackLevels = true,
});

foreach (var track in player.Tracks)
{
    var stem = song[track.Name];
    var worthPlaying = track.HasMidiSource && stem.MidiCoverage > 0.02;
    if (worthPlaying && track.Name != "Vocals")
    {
        track.ActiveSource = TrackSource.Midi;
    }
}

player.Prepare();
await player.LevelMeasurement;           // the levels are in when this returns
player.Play();
```

One named part, without walking the list:

```csharp
var bass = player["Bass"];
if (bass.HasMidiSource)                  // not every stem has a .mid beside it
{
    bass.ActiveSource = TrackSource.Midi;
}
```

Giving one part an instrument of its own instead of the General MIDI SoundFont:

```csharp
var strings = sfzCache.Get(@"D:\Libraries\Strings\strings.sfz");
using var player = song.CreatePlayer((stem, rate) => stem.Name == "Synth"
    ? new SfzSynthesizer(strings, rate)
    : new SoundFontSynthesizer(generalMidi, rate));
```

Correcting one part's alignment by hand, then bouncing the result to a file:

```csharp
song["Drums"].AlignmentOffset = TimeSpan.FromMilliseconds(-120);
using var bounce = song.CreatePlayer(options);
bounce.RenderToWav("mix.wav", 44100);
```

## Pitfalls

- **`Offset` moves a whole track; `MidiSourceOffset` moves only its MIDI rendition.** An alignment
  measurement belongs in `MidiSourceOffset`. Putting it in `Offset` drags the recording out of step
  with the rest of the song, which is exactly what a stems set must not have done to it.
- **An alignment estimate is advisory outside drums and percussion.** A repeating pattern gives the
  search several equally good answers a beat apart, and confidence measures how much one peak stands
  out, not whether it is the right one. Check `IsReliable`, look at `CandidatePeakCount`, and remember
  the value is overridable per stem and per track.
- **A track holding two sources renders both on every block,** whichever one is heard. That is what
  makes a source switch a crossfade rather than a restart, and it is the player's main cost: a track
  that will never switch should hold only the source it needs.
- **One synthesizer per track, never one shared.** A synthesizer factory is called once per track per
  render context and may be called from a worker thread, and `IMidiSynthesizer` is explicitly not
  thread-safe. Share the SoundFont or the SFZ instrument behind them - those are the expensive part
  and are safe to share - and return a **new** synthesizer every time.
- **Extracting a stems zip to memory makes every stem a `Stream`,** and a `Stream` audio source is
  copied into memory when the track is built - the audio twice over. Use the default cache folder
  unless the host genuinely cannot write to disk.
- **The player's indexer throws when a name is absent,** listing the names that are present. Use
  `FindTrack` or `TryGetTrack` when the name might legitimately be missing. The song model's own
  indexer returns null instead, because a stems export genuinely may not contain the part you asked
  for.
- **`MidiAudioAlignment.Estimate` is far too slow for a render callback.** Call it on a worker.

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/AGENT-README.txt) |
| Multi-track, stems and alignment tests, including a synthetic stems export built in code | [tests/CodeBrix.Audio.Tests](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tests/CodeBrix.Audio.Tests) |

## License

CodeBrix.Audio is licensed under the MIT License, and the license is also named in the package ID
(`CodeBrix.Audio.MitLicenseForever`). No third-party audio and no SoundFont is committed anywhere in
the repository; the instrument a MIDI part plays through is one your application supplies and is
licensed to distribute. For the provenance and licensing of open source code included in this library,
see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [Playback](playback.md) - the transport a whole song plays through
- [SoundFont and SFZ instruments](soundfont-and-sfz.md) - what renders a MIDI part
- [MIDI files](midi-files.md) - tolerant reading, and the `Problems` contract a stems load shares
- [CodeBrix.Audio](../CodeBrix.Audio.md) - the front door of the audio section
