<sub>[CodeBrix](../../../README.md) › [Libraries](../README.md) › [CodeBrix.Audio](../CodeBrix.Audio.md) › Playback</sub>

# Playback and sound effects

**This page covers getting audio out of the speakers with
[CodeBrix.Audio](../CodeBrix.Audio.md): the one shared output every voice mixes into, the media
transport `AudioFilePlayer`, the decode-once `SoundEffectClip`, the low-level `WaveOutEvent`, and
`PacketAudioPlayer` for audio that arrives as bare codec packets out of a media container.** It also
covers the codec seam - how a package under a different license teaches every one of those players a
new format. Playback goes through the bundled engine and its native backend; nothing has to be
installed on Windows, macOS or Linux.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Audio](https://github.com/ellisnet/CodeBrix.Audio) |
| **Packages** | [`CodeBrix.Audio.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later. No system audio package and no system-wide codec is required on Windows, macOS or Linux |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application, through the [AudioPlayer add-in](../../platform/add-ins/AudioPlayer.md) |
| **Platforms** | Windows, macOS and Linux. The bundled native backend ships for `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `linux-riscv64`, `osx-x64` and `osx-arm64` |

## What it does

- Plays a long track with a media transport: `AudioFilePlayer` streams from disk, resamples to the
  device, and exposes `Load`, `Play`, `Pause`, `Stop`, `Seek`, `Volume`, `IsLooping`, `Position`,
  `Duration` and `PlaybackEnded`.
- Plays short sounds that overlap freely: `SoundEffectClip` decodes once into memory and plays as
  often as you like, at any source sample rate.
- Pushes an `IWaveProvider` or `ISampleProvider` at the speakers: `WaveOutEvent`, where every instance
  is a voice in one shared device rather than a device of its own.
- Plays audio that arrives as bare codec packets rather than as a file: `PacketAudioPlayer`, with the
  playback clock, seeking by contract, end-of-track trimming and packet-loss handling.
- Mixes everything into one shared output, `SharedAudioOutput`, whose format you can pin once at
  start-up.
- Accepts a codec from another package through two registration seams, so a new format reaches every
  player above without any of them changing.

## When to use it

Pick the player by the shape of the sound, not by the format.

| The sound | The type |
| --- | --- |
| A song, a podcast, a soundtrack - long, one at a time, with a scrubber | `AudioFilePlayer` |
| A laser, a footstep, a UI click - short, triggered often, overlapping | `SoundEffectClip` |
| Something you already have as an `IWaveProvider` | `WaveOutEvent` |
| An audio track a demultiplexer lifted out of a video container | `PacketAudioPlayer` |
| A MIDI performance through a sampled instrument | `MidiMusicPlayer` - see [SoundFont and SFZ](soundfont-and-sfz.md) |
| Several parts on one transport | `MultiTrackPlayer` - see [Multi-track songs and stems](multi-track-and-suno.md) |

In a CodeBrix.Platform application the [AudioPlayer add-in](../../platform/add-ins/AudioPlayer.md)
wraps `AudioFilePlayer` and the sound-effect path as XAML elements with a bindable position, which is
what a page wants. Reach for the types on this page directly when the audio does not belong to a page.

## Getting started

```bash
dotnet add package CodeBrix.Audio.MitLicenseForever
```

```csharp
using CodeBrix.Audio.Playback;   // AudioFilePlayer, SoundEffectClip, PacketAudioPlayer
using CodeBrix.Audio.Wave;       // WaveOutEvent, SharedAudioOutput
```

Playing a long track with a transport is the common case:

```csharp
using CodeBrix.Audio.Playback;

var media = new AudioFilePlayer();
media.Load("song.flac");                 // any supported format; Duration is available now
media.PlaybackEnded += (s, e) => { /* reached the natural end */ };
media.Play();
// media.Position and media.Duration are TimeSpans → drive a scrubber/tracker UI.
// media.Seek(TimeSpan.FromSeconds(83));  // jump to 1:23
// media.Volume = 0.7f;  media.Pause();  media.Stop();  media.IsLooping = true;
// media.Dispose() when finished.
```

`Duration` is valid as soon as `Load` returns, because opening a file reads its headers plus a few
seconds of audio rather than the whole thing.

## Key concepts

### The shared output

`SharedAudioOutput` is the one output device that `WaveOutEvent`, `AudioFilePlayer` and
`PacketAudioPlayer` all mix into. Every `WaveOutEvent` is a *voice* in that device, not a device of
its own, so overlapping many sounds is cheap mixing rather than many device opens - do not build a
pool to avoid creating them.

`SharedAudioOutput.Configure(sampleRate[, channels])` pins the output format once at start-up, and
`Shutdown()` releases the device. Left alone, the output adopts the format of the first sound played.

> [!TIP]
> Call `SharedAudioOutput.Configure(48000)` at start-up in any application that plays media
> containers. Media containers carry 48 kHz, the only rate conversion in this package is linear
> interpolation, and when the device runs at the media's rate no conversion runs at all. Without it,
> an application that has already played a 44.1 kHz sound effect will have started the output at
> 44.1 kHz, and then every video plays through the interpolator.

### Playing a whole downloaded song

A downloaded song - a `.wav`, `.mp3`, `.ogg` or `.flac` - is a long recording like any other, and
`AudioFilePlayer` is the type for it. Do not use `SoundEffectClip`: that decodes a clip whole into
memory for short, overlapping one-shots.

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

What to expect:

- `Load` identifies the format from the file's content, not its extension, so a WAV, an MP3, an Ogg
  Vorbis or a FLAC download all work the same way. `Duration` is known as soon as `Load` returns.
- The file's sample rate does not matter: the player resamples to the output device, so a 48 kHz
  download plays on a 44.1 kHz device.
- The file is decoded in chunks as it plays. A four-minute WAV is not held in memory.
- An MP3's encoder delay and padding are trimmed, so `Position` zero is the first real sample and
  `Duration` excludes the padding.
- `PlaybackEnded` is raised on the `SynchronizationContext` that was current when the file was loaded
  - the UI thread, in an application that loaded it from the UI - and on the engine's thread when
  there was none. Do not do slow work in the handler.
- `Volume` and `IsLooping` persist across loads. Load a second file on the same player to move on to
  the next song; dispose the player when done.
- In a CodeBrix.Platform application, the [AudioPlayer add-in](../../platform/add-ins/AudioPlayer.md)
  wraps this same player as a XAML element with a bindable position.

### Sound effects

`SoundEffectClip` decodes a short sound once into memory and then plays it as often as you like,
including many times at once. `Load` takes a path, a `byte[]` or a `Stream`, and the surface is
`Play(volume)`, `StopAll`, `Duration` and `ActiveVoiceCount`. A clip's own sample rate does not have
to match the output device - the decode step converts it - so an asset pack that mixes rates works
unchanged.

```csharp
using CodeBrix.Audio.Playback;

using var laser = SoundEffectClip.Load("laser.ogg");  // decoded once, to the output format
laser.Play();                                          // fire and forget
laser.Play(0.4f);                                      // again, quieter, over the first
// laser.Duration, laser.ActiveVoiceCount, laser.StopAll()
```

`SoundEffectClip.PlayOnce(path[, volume])` loads, plays and cleans up in one call, with `Stream` and
`byte[]` overloads beside it. It decodes every time, so hold a `SoundEffectClip.Load(...)` for
anything you trigger often.

### WaveOutEvent

`WaveOutEvent` plays an `IWaveProvider` or `ISampleProvider` to the output device with
`Init`, `Play`, `Pause`, `Stop`, `Volume` and `PlaybackStopped`. It is the right type when you already
have a provider chain and want to hear it.

```csharp
using CodeBrix.Audio.Wave;

var player = new WaveOutEvent();
player.Init(new WaveFileReader("clip.wav"));   // any IWaveProvider/ISampleProvider
player.PlaybackStopped += (s, e) => { /* ended; e.Exception is null on normal end */ };
player.Play();                                 // Play / Pause / Stop; player.Volume = 0.5f;
// ... player.Dispose() when finished.
```

It has no resampler, so a source whose rate differs from the running output is rejected by `Init`
rather than played at the wrong pitch. Mono and stereo sources are matched to the output
automatically. `IWavePlayer` and `IWavePosition` are the interfaces it implements.

### Composing providers between a reader and an output

`BufferedWaveProvider` pushes audio in from one thread and pulls it out from the audio thread;
`MixingWaveProvider32` sums several 32-bit float sources; `VolumeSampleProvider` applies gain and
`PanningSampleProvider` a stereo pan; `OffsetSampleProvider` skips, pads or takes a section;
`FadeInOutSampleProvider` does timed fades; `ConcatenatingSampleProvider` plays sources back to back;
`MultiplexingSampleProvider` routes input channels to output channels;
`MonoToStereoSampleProvider` and `StereoToMonoSampleProvider` change the channel count;
`SilenceProvider` is a source of silence and `SignalGenerator` produces sine, square, saw, noise and
sweep test tones; `WaveChannel32` promotes a `WaveStream` to 32-bit float with volume and panning.

### Adding a codec from another package

CodeBrix.Audio is MIT and stays that way, so a codec under a different license belongs in its own
package that depends on this one. Everything such a package needs is public API; nothing here has to
change to accept one. There are two seams, because there are two ways audio gets opened.

Playback identifies formats by **content**. Supply an `ICodecFactory` and register it:

```csharp
SharedAudioOutput.RegisterCodecFactory(new OpusCodecFactory());
```

That reaches `AudioFilePlayer`, `SoundEffectClip`, `WaveOutEvent` and the
[GameEngine](../CodeBrix.Platform.GameEngine.md)'s audio stack. The registration is remembered for the
process, so it survives `SharedAudioOutput.Shutdown()` and is re-applied to every engine started
afterwards. A consumer driving its own `AudioEngine` calls `engine.RegisterCodecFactory(...)` instead,
and `ManagedCodecs.RegisterAll(engine)` adds the built-in managed codecs to it.

Reading by file name dispatches on **extension**, and is the registry described in
[Reading and writing files](reading-and-writing-files.md).

Build a decoder on `ManagedSoundDecoder` (public, in `CodeBrix.Audio.Codecs`). It handles the part
every codec otherwise reimplements - converting the file's channel count and sample rate to what the
engine asked for. Derive from it, supply `ReadSourceSamples`, `SeekSource` and `DisposeCore`, and call
`Initialize(channels, sampleRate, totalFrames)` once the file's format is known.

> [!IMPORTANT]
> The metadata layer reports the format identifier `ogg` for **every** Ogg stream, whatever codec is
> inside, so an Ogg-capable factory is offered Vorbis, Opus and Ogg FLAC alike. Your factory must
> check what it was actually handed with `OggCodecSniffer` and return null for anything else -
> returning null lets the engine move on to the next factory, while throwing, or accepting and then
> failing, does not. Reset the stream position on entry when `stream.CanSeek`, because the engine does
> not rewind between factories on that path.

The built-in native factory sits at priority 0 and the managed fallbacks at -10. Call the
registrations once at start-up; a static `Register()` entry point on the add-on package is the
friendliest shape, and a module initializer is not, because it runs only once something in the
assembly is touched.

### Audio that arrives as packets

Everything above assumes the audio is a file - something with a container around it that a reader can
open and seek in. Audio pulled out of a video container does not arrive that way: a demultiplexer
hands out bare codec packets, a few hundred a second, with no framing of their own. That is a
different seam, with three pieces.

`IPacketSoundDecoder` (in `CodeBrix.Audio.Engine.Interfaces`) decodes one packet at a time:

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
finalizes a packet's samples only once the next packet has been overlapped onto it, so the first
packet after construction or `Reset()` yields nothing. And the decoder does not trim the end of the
stream - the container knows where the audio really stops, so applying that is the caller's job.

`ConcealLoss` and `SupportsLossConcealment` have default implementations, so an existing decoder keeps
compiling and working untouched. The default forwards to `DecodePacket` with an empty packet, which is
the long-standing way of saying "one packet was lost", and reports `SupportsLossConcealment` as false.
A decoder whose codec really can synthesize audio across a gap overrides both.

`IPacketCodecFactory` mirrors `ICodecFactory` exactly, except that `SupportedCodecIds` names the
**codec** ("vorbis", "opus") rather than the container:

```csharp
string FactoryId { get; }
IReadOnlyCollection<string> SupportedCodecIds { get; }
int Priority { get; }
IPacketSoundDecoder CreateDecoder(string codecId,
                                  ReadOnlyMemory<byte> codecPrivate,
                                  AudioFormat? hint)
```

`SharedAudioOutput.RegisterPacketCodecFactory(...)` installs one; the registration lasts for the
process and de-duplicates on the instance, so keep one factory instance per add-on package.
`SharedAudioOutput.RegisteredPacketCodecFactories` lists what you registered, in registration order,
and deliberately not the built-ins. Vorbis packets are built in and always registered, through the
public `VorbisPacketCodecFactory`; Opus packets come with
[CodeBrix.Audio.Opus](../CodeBrix.Audio.Opus.md).

To ask whether a codec is available without starting anything, use
`SharedAudioOutput.IsPacketCodecSupported("opus")` or `SharedAudioOutput.SupportedPacketCodecIds`.
Both match case-insensitively and neither opens the audio device.
`SharedAudioOutput.CreatePacketDecoder(...)` **does** open it, because the codec registry lives on the
running engine.

A consumer driving its own `AudioEngine` gets more of the surface than the shared output exposes:
`engine.RegisterPacketCodecFactory`, `engine.UnregisterPacketCodecFactory(factoryId)`,
`engine.SetPacketCodecPriority(factoryId, newPriority)`, `engine.GetRegisteredPacketCodecs(codecId)`
and `engine.CreatePacketDecoder(codecId, codecPrivate[, hint])`. Registering the same factory twice on
an engine adds it twice - the de-duplication belongs to `SharedAudioOutput`, not to the engine.

### The packet feed is pulled, not pushed

`PacketAudioPlayer` is the supported route from packet audio to the speakers, because `WaveOutEvent`
refuses a source whose rate does not match the running output while this player decodes through the
engine's own conversion. You implement `IAudioPacketSource` and the player asks it for the next packet
**on the audio thread**, exactly when it needs one:

```csharp
public interface IAudioPacketSource
{
    bool TryReadPacket(out AudioPacket packet);   // false = none ready
    bool EndOfStream { get; }                     // true = no more, ever
}
```

Both members must return immediately. Read ahead on your own thread into a bounded queue and hand
packets out of that queue; never block and never do I/O here. Running dry is not an error: return
false with `EndOfStream` still false and the player plays silence for that moment and keeps the voice
alive. Playback ends only when `EndOfStream` is true and the decoded audio has run out, at which point
`PlaybackEnded` is raised away from the audio thread.

`AudioPacket` is a small struct carrying `ReadOnlyMemory<byte> Data`, an optional `Timestamp`, an
optional `DiscardPadding` and, for a packet that reports a gap rather than delivering audio,
`IsLoss` / `LossDuration` / `LossFrames`. The memory must stay valid until the next `TryReadPacket`
call, so handing out slices of a rolling buffer is fine.

The player's own surface is small and thread-safe: `Open` (by codec id, or with a decoder you already
have), `Play`, `Pause`, `Stop`, `Seek`, `Dispose`, plus `IsOpen`, `PlaybackState`, `Position`,
`Volume`, `SampleRate`, `Channels`, `TrailingTrim`, `SetTrailingTrim` / `SetTrailingTrimFrames` and
the `PlaybackEnded` event.

### The clock, seeking, trimming and loss

`Position` is the clock. It counts the audio actually handed to the mixer since the last `Seek`, at
the codec's own sample rate, and is readable from any thread - so anything being synchronized to the
audio should read it rather than keeping a clock of its own. Silence played through an underrun does
not advance it; samples discarded as codec priming or seek pre-roll do, because they are media time.

Seeking is a contract, because the player has no container to seek in. Move your own source first,
then tell the player where it now is:

```csharp
myPacketSource.MoveTo(keyframeBefore(target));      // your reader
player.Seek(firstPacketTimestamp, preRoll: gap);    // then the player
```

`preRoll` is how much audio to decode and throw away before any is heard - a codec carrying state
between packets cannot decode correctly at a jump, so start a little *before* the real target and pass
the gap as `preRoll`. Calling `Seek` while the old packets are still queued dates the clock to the new
position and then plays the old audio against it, so order matters. A source that had reported
`EndOfStream` is expected to report false again once it has been repositioned.

An encoder pads the end of what it encodes, and the container - not the codec - records how much.
Without that being applied, the padding plays. Apply it with `SetTrailingTrim(TimeSpan)` or
`SetTrailingTrimFrames(int)`, or let the packets carry it through `AudioPacket.DiscardPadding`; a
per-packet value is applied as the larger of the two. The last `trim` worth of everything the source
will ever deliver is held back and then thrown away, at a cost of exactly `trim` in latency and no
allocation while playing. `Position` never counts trimmed audio. `Seek` clears what is in hand but
keeps the trim, and so does `Open` - set the trim again, or to `TimeSpan.Zero`, when you open a
different track.

When your demultiplexer can see that packets are missing - a jump in the timestamps, a container-level
loss marker, a network read that gave up - say so, with the length:

```csharp
packet = AudioPacket.Loss(TimeSpan.FromMilliseconds(60));   // a duration
packet = AudioPacket.Loss(2880);                            // frames/channel
```

The player asks the decoder to conceal exactly that much and fills whatever the decoder cannot with
silence, so the gap comes out the length it really was and the audio after it keeps its position.
Concealed audio is media time: it advances `Position` and flows through the trailing-trim hold-back
like any other audio. Do not use it for an underrun - a moment when your reader has not kept up is not
lost audio.

## Examples

Overlapping sound effects without keeping a clip around:

```csharp
using CodeBrix.Audio.Playback;

SoundEffectClip.PlayOnce("beep.wav");          // loads, plays, cleans up
SoundEffectClip.PlayOnce("beep.wav", 0.4f);    // quieter
// Also PlayOnce(Stream, float) and PlayOnce(byte[], float). Convenient, but
// it decodes every time - for a sound you trigger often, hold a
// SoundEffectClip.Load(...) instead.
```

Playing packets from a container, end to end:

```csharp
using CodeBrix.Audio.Playback;
using CodeBrix.Audio.Wave;

SharedAudioOutput.Configure(48000);          // see the rate advice below

var player = new PacketAudioPlayer();
player.PlaybackEnded += (s, e) => { /* the track finished */ };
player.Open("vorbis", codecPrivate, myPacketSource);
player.Volume = 0.8f;
player.Play();

TimeSpan where = player.Position;             // the clock; any thread
```

Trimming the encoder padding off the end of that track:

```csharp
player.SetTrailingTrim(TimeSpan.FromMilliseconds(12));   // a duration
player.SetTrailingTrimFrames(576);                       // frames/channel
TimeSpan trim = player.TrailingTrim;                     // what is in effect
```

A complete console application that plays a file to the end and prints its duration:

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

## Using it in a CodeBrix.Platform application

The [AudioPlayer add-in](../../platform/add-ins/AudioPlayer.md) wraps this page's players as
non-visual XAML elements with a two-way bindable position, plus a static sound-effect class, and
brings CodeBrix.Audio in as an automatic dependency. Use the add-in when the audio belongs to a page,
and these types directly when it does not.

The codec registry is what ties the two together. A codec registered with `SharedAudioOutput` reaches
`AudioFilePlayer`, `SoundEffectClip`, `WaveOutEvent` and the
[GameEngine](../CodeBrix.Platform.GameEngine.md)'s audio stack, which is why adding
[CodeBrix.Audio.Opus](../CodeBrix.Audio.Opus.md) and calling its `Register()` makes `.opus` play
through the add-in as well. The consuming application takes that dependency and makes the call - the
add-ins never do.

## Pitfalls

- **Re-opening a reader per trigger is the single most expensive mistake available in this library.**
  `SoundEffectClip.Load` decodes to the output format one time and then plays at no further decode
  cost. The trade is memory: a clip holds its decoded PCM, so it is right for effects and wrong for a
  soundtrack.
- **Stream anything long.** `AudioFilePlayer` streams from disk, so a two-hour podcast costs about
  what a ten-second one does in memory. Do not pre-load a media library at start-up - load the one
  track you are about to play, when you are about to play it.
- **`WaveOutEvent` does not resample.** The shared output adopts the rate of the first sound played
  unless you pin it, and a source whose rate differs from the running output is rejected by `Init`.
  `AudioFilePlayer` and `SoundEffectClip` both convert, so only the `WaveOutEvent` path needs a match.
- **Keep the audio thread clean.** A source's `Read` runs on the real-time audio callback: no disk
  I/O, no locks another thread holds for long, no avoidable allocation, no UI marshalling. Hand data
  to your own thread and act on it there.
- **A packet source is pulled on the audio thread and must never block.** An empty return is an
  underrun - silence, and playback continues - not the end. Only `EndOfStream` ends it.
- **`SharedAudioOutput.CreatePacketDecoder` opens the audio device,** because the codec registry lives
  on the running engine. To ask whether a codec is available without starting anything, use
  `IsPacketCodecSupported`.
- **The container, not the codec, knows where a track really stops.** Apply it with
  `SetTrailingTrim`, or with `AudioPacket.DiscardPadding`, or the encoder's padding plays.
- **A few Engine entry points are synchronous wrappers over async I/O** - including
  `AudioFilePlayer.Load`. They still do blocking disk or network I/O, so on a UI thread prefer the
  `*Async` overloads where they exist, or do the work on a background thread.
- **The native decoder is the fast path.** The engine prefers its bundled native library and falls
  back to the managed decoders only where the native one cannot handle a format. That is a reason to
  play through `AudioFilePlayer` or `SoundEffectClip` rather than pumping a managed `WaveStream` by
  hand when you have the choice.

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/AGENT-README.txt) |
| Player and packet-seam tests | [tests/CodeBrix.Audio.Tests](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tests/CodeBrix.Audio.Tests) |
| Worked examples of the decoder and factory shapes an add-on implements | [src/CodeBrix.Audio/Codecs](https://github.com/ellisnet/CodeBrix.Audio/tree/main/src/CodeBrix.Audio/Codecs) |

## License

CodeBrix.Audio is licensed under the MIT License, and the license is also named in the package ID
(`CodeBrix.Audio.MitLicenseForever`). The license notice for the bundled native backend travels with
the native binaries into your application's output folder. For the provenance and licensing of open
source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [Reading and writing files](reading-and-writing-files.md) - the readers and the by-extension registry
- [The bundled audio engine](audio-engine.md) - devices, recording, effects, editing and mixing
- [CodeBrix.Audio.Opus](../CodeBrix.Audio.Opus.md) - the worked example of a codec package on both seams
- [CodeBrix.Audio](../CodeBrix.Audio.md) - the front door of the audio section
