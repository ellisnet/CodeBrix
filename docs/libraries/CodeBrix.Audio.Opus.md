<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Audio.Opus</sub>

# CodeBrix.Audio.Opus

**CodeBrix.Audio.Opus adds Ogg Opus (`.opus`) decoding and encoding to
[CodeBrix.Audio](CodeBrix.Audio.md), in pure managed code with no native binaries on any platform.**
One call at start-up - `CodeBrixAudioOpus.Register();` - wires it in, and after that `.opus` reaches
every path a built-in format reaches: the players, the file readers, the recorder, and the packet seam a
media container's demultiplexer feeds. You add it to any .NET 10 application, or to a CodeBrix.Platform
application, whenever Opus has to be one of the formats your audio handles.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Audio.Opus](https://github.com/ellisnet/CodeBrix.Audio.Opus) |
| **Packages** | [`CodeBrix.Audio.Opus.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.Opus.BsdLicenseForever) |
| **License** | BSD-3-Clause; see [License](#license) |
| **Requires** | .NET 10 or later, and [`CodeBrix.Audio.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MitLicenseForever), which the package pulls in automatically |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Everywhere CodeBrix.Audio works. Nothing is called through P/Invoke and no binaries ship, so the package places no restriction on the runtime identifiers your application may publish for |

## What it does

- Decodes and plays Ogg Opus (`.opus`) files - voice notes, podcasts and downloaded audio - through
  every [player CodeBrix.Audio offers](audio/playback.md).
- Decodes the bare Opus packets a media container carries, through CodeBrix.Audio's packet seam.
  `OpusPacketCodecFactory` is what teaches that seam Opus, and one `Register()` call installs it.
- Performs real packet-loss concealment on the packet path, so a source that drops packets is filled in
  rather than clicking.
- Encodes to Ogg Opus from any input sample rate, and records straight to `.opus` through the
  CodeBrix.Audio engine's `Recorder`.
- Seeks exactly, and reports durations that account for the encoder's pre-skip.
- Reads and writes Opus tags (`TITLE`, `ARTIST`, and the rest).
- Handles mono and stereo - Opus channel mapping family 0, which covers ordinary Opus files.
- Applies the identification header's output gain on both paths, as RFC 7845 requires of a decoder, so
  the file reader and the packet decoder produce identical samples for identical packets.

After `Register()`, `.opus` plays through `AudioFilePlayer`, `SoundEffectClip`, `WaveOutEvent`, the
[CodeBrix.Platform AudioPlayer add-in](../platform/add-ins/AudioPlayer.md) and the
[CodeBrix.Platform GameEngine](CodeBrix.Platform.GameEngine.md); opens by file name through
`AudioFileReader`; and can be written by the engine's `Recorder`.

## When to use it

Take this package when your application must open or produce `.opus`, or when a container you
demultiplex carries an Opus track. Without it, CodeBrix.Audio recognizes `.opus` - metadata, duration,
channels and rate all read correctly - but does not decode it.

The separation is deliberate. CodeBrix.Audio holds a license bar of MIT or more permissive, and its
package ID says so out loud. Opus cannot clear that bar, so it gets its own package: a license that adds
a condition gets its own package, and that decision is the standing precedent for the family. What it
means for your application is that taking this dependency means accepting BSD-3-Clause terms alongside
MIT ones - its third clause forbids using the copyright holders' names to endorse your product without
permission, and the notice text must travel with any redistribution. Add this package and CodeBrix.Audio's
promise is unchanged; skip it and nothing about CodeBrix.Audio changes either.

What the package deliberately does not do:

- It does not decode or encode multichannel Opus (channel mapping family 1). Mono and stereo, mapping
  family 0, on both seams.
- It does not read any container but Ogg. It decodes the Opus packets a container carried, once
  something else has lifted them out and handed over the identification header - it does not parse
  Matroska, WebM, RTP or anything else, and it has no demultiplexer of its own. Encoding is Ogg Opus
  files only.
- It does not call a native Opus library, ship native binaries, or provide a native path.
- It does not play audio. Playback, devices, mixing, transports and recording all belong to
  CodeBrix.Audio and its bundled engine; this package only makes `.opus` one of the formats those can
  handle.
- It does not resample on the way out. The reader hands back 48 kHz, because that is what Opus decodes
  to.
- It does not expose the codec's streaming controls - forward error correction, packet-loss percentage,
  discontinuous transmission, bandwidth ceilings, frame duration. Those are for live streams, not for
  files.
- It does not edit tags in place. Tags are read from a stream and written when a stream is created;
  there is no re-tag-this-file operation.
- It registers nothing on its own. Without your `CodeBrixAudioOpus.Register()` call, taking this
  dependency changes nothing about how your application behaves.

## Getting started

```bash
dotnet add package CodeBrix.Audio.Opus.BsdLicenseForever
```

```csharp
using CodeBrix.Audio.Opus;      // everything a consumer needs
```

Consumers of the playback and file surfaces also want the CodeBrix.Audio namespaces they already use:

```csharp
using CodeBrix.Audio.Wave;      // AudioFileReader, WaveOutEvent,
                                //   SharedAudioOutput, WaveFileWriter
using CodeBrix.Audio.Playback;  // AudioFilePlayer, SoundEffectClip
```

One call at start-up is the whole integration:

```csharp
using CodeBrix.Audio.Opus;

CodeBrixAudioOpus.Register();
```

`Register()` registers both codec factories with `SharedAudioOutput` - the stream one, which opens
`.opus` files, and the packet one, which decodes the bare Opus packets a media container carries - and
registers `".opus"` with [`AudioFileReaderRegistry`](audio/reading-and-writing-files.md). It is
idempotent and thread-safe; calling it twice does nothing. There is deliberately no module initializer doing this for you: a module initializer only
runs once something in the assembly is touched, so under trimming or lazy assembly loading the package
would work in a debug build and silently fail to register in a trimmed publish.

A complete console application that plays a `.opus` file to the end:

```csharp
using System;
using System.Threading;
using CodeBrix.Audio.Opus;
using CodeBrix.Audio.Playback;

CodeBrixAudioOpus.Register();

var finished = new ManualResetEventSlim(false);

using var media = new AudioFilePlayer();
media.PlaybackEnded += (s, e) => finished.Set();
media.Load(args[0]);                       // path to a .opus file

Console.WriteLine($"Playing {media.Duration}");
media.Play();
finished.Wait();
```

Nothing else is added to the project: one `PackageReference`, no native-asset package and no
platform-specific payload.

## Key concepts

### A small, fully enumerated public surface

Every public type is listed in the package's own guide: `CodeBrixAudioOpus`, `OpusFileReader`,
`OpusFileWriter`, `OpusFileWriterOptions`, `OpusEncodingProfile`, `OpusCodecFactory` and
`OpusPacketCodecFactory`. The sub-namespaces `CodeBrix.Audio.Opus.Codec` and `CodeBrix.Audio.Opus.Ogg`
are entirely internal, and in `CodeBrix.Audio.Opus.Codecs` only the two factories are public.

### `CodeBrixAudioOpus`

```csharp
static void Register()
static void Register(AudioEngine engine)
static bool IsRegistered { get; }
```

`Register(AudioEngine)` is for a consumer driving its own engine rather than the shared output: it
registers both factories with that engine only, and does not affect `SharedAudioOutput`. Pair it with
CodeBrix.Audio's `ManagedCodecs.RegisterAll(engine)`. It throws `ArgumentNullException` on a null engine.

### `OpusFileReader`

`OpusFileReader : WaveStream` is constructed from a file name or a `Stream`, and exposes `WaveFormat`,
`Length`, `Position`, `TotalTime`, `EncoderInputSampleRate`, `PreSkip`, `Tags`, `EncoderVendor`, the two
`Read` overloads and `Dispose`. `WaveFormat` is always 48 kHz 32-bit IEEE float; `Channels` comes from
the file, and is 1 or 2.

Ownership differs by constructor. The string overload opens the file and the reader owns it. The
`Stream` overload does **not** own the stream: the caller disposes it. That is the contract
CodeBrix.Audio's reader registry relies on, which is why `Register()` hands the registry the `Stream`
overload.

`Length` and `Position` are in **bytes**, like every `WaveStream` - one frame is
`Channels * sizeof(float)` bytes - and setting `Position` seeks to a sample boundary. `Length` and
`TotalTime` exclude the encoder's pre-skip, and `Length` is 0 when the stream does not report a total
sample count. `Tags` are the stream's Vorbis comments, keyed case-insensitively by field name and each
mapping to the list of values for that field, because an Opus comment header may repeat a field.

### `OpusFileWriter` and its options

`OpusFileWriter : IDisposable` takes a file name or a `Stream` plus `sampleRate`, `channels` and
optional `OpusFileWriterOptions`, and exposes `PreSkip`, two `Write` overloads, `Finish()` and
`Dispose()`. Samples are interleaved floats in [-1, 1] and `channels` must be 1 or 2. Any input
`sampleRate` is accepted and resampled to the 48 kHz Opus encodes at; the rate you declare is still
recorded in the file header as the rate the encoder was given.

```csharp
int Bitrate { get; init; }                       // default 96_000 bps
OpusEncodingProfile Profile { get; init; }       // default Music
bool UseVariableBitrate { get; init; }           // default true
int Complexity { get; init; }                    // default 10, range 0-10
IDictionary<string, string> Tags { get; }        // case-insensitive keys
void Validate()
```

`Validate()` throws `ArgumentOutOfRangeException` for a `Bitrate` outside 500..512000 or a `Complexity`
outside 0..10; the writer's constructor calls it for you, so an invalid option throws there.
`OpusEncodingProfile` has two values - `Music` for general audio (music, podcasts, game soundtracks) and
`Voice` for speech (voice notes, push-to-talk, in-app memos) - and is fixed when the encoder is
constructed, so it cannot be changed on an open writer.

The option set is deliberately small. Opus also exposes forward error correction, packet-loss
percentage, discontinuous transmission, bandwidth ceilings and frame duration, but those are streaming
concerns: a file on disk drops no packets, and discontinuous transmission writes gaps into one.

`Finish()` pads and flushes the final partial frame and writes the closing page carrying the true sample
count. `Dispose()` calls it, and it is safe to call twice.

### The two factories

`OpusCodecFactory : ICodecFactory` serves the stream seam: `OpusFormatId` is "opus", `OggFormatId` is
"ogg", `FactoryId` is "CodeBrix.Audio.Opus.ManagedOpus", `SupportedFormatIds` is `["ogg", "opus"]` and
`Priority` is -10, which sits below the engine's built-in native factory at 0, matching CodeBrix.Audio's
own managed Vorbis and FLAC factories. `CreateEncoder` is what the [bundled engine](audio/audio-engine.md)'s `Recorder` reaches, so
`new Recorder(captureDevice, stream, "opus")` records straight to Ogg Opus once the factory is
registered; it accepts only the "opus" format id and only 1 or 2 channels, returning null otherwise.

`OpusPacketCodecFactory : IPacketCodecFactory` serves the packet seam: `OpusCodecId` is "opus",
`FactoryId` is "CodeBrix.Audio.Opus.ManagedOpus.Packets", `SupportedCodecIds` is `["opus"]` and
`Priority` is 0 - not -10, because there is nothing for it to sit below: the engine's bundled native
library decodes Ogg streams, not loose packets, so nothing competes for the "opus" codec id. It returns
null for a request it cannot serve at all - another codec's id, or codec-private data that is not a
well-formed identification header - and throws `NotSupportedException` for a header it understands but
cannot decode, which is the multichannel case.

Both factories are public so a consumer can register one by hand -
`SharedAudioOutput.RegisterCodecFactory(new OpusCodecFactory())` - but `Register()` is the friendly path
and reuses one instance of each.

### Opus that arrives as packets

Audio lifted out of a media container does not arrive as a file: a demultiplexer hands out bare Opus
packets, fifty a second for the usual 20 ms frame, with no framing of their own. CodeBrix.Audio calls
that its [packet seam](audio/playback.md) - `IPacketSoundDecoder`, `IPacketCodecFactory`,
`IAudioPacketSource` and `PacketAudioPlayer` are all its types - and `Register()` teaches it Opus.

The codec-private data is the `OpusHead` bytes. A Matroska or WebM track stores the Opus identification
header verbatim in its CodecPrivate element, so that element's bytes are what you hand over: no
unwrapping, no re-framing. The codec id is "opus" - the codec, not the container - so a container of
your own that stores the same identification header works the same way.

The decoder reports `SampleRate` 48000 always, `Channels` 1 or 2 from the header, `SampleFormat` F32,
`MaxSamplesPerPacket` of `5760 * Channels` (a 120 ms packet, the longest Opus defines - size the output
buffer to it once and reuse it, and then no packet can ever be too big), `PreSkipSamples` counted per
channel, and `SupportsLossConcealment` true. The hint is ignored: an Opus stream decodes at 48 kHz and
at its own channel count, and this decoder converts neither.

> [!IMPORTANT]
> The pre-skip is reported, not applied. The stream reader discards the encoder's priming for you
> because it knows it is at the start of a file; a packet decoder does not know that. Discard
> `PreSkipSamples` frames at the start of the stream and nowhere else. `PacketAudioPlayer` does this for
> you. The tail is not trimmed either - the encoder pads its last frame and the container states where
> the audio really stops, so applying that trim is the caller's job. There is deliberately no flush or
> drain call.

### Seeking: `Reset` and pre-roll

Opus carries state between packets, so after repositioning the source you call `Reset()` and then feed
packets from before your target, discarding what comes back until you reach it. 80 ms is the standard
answer - RFC 7845 section 4.2 asks for it, and Matroska records it as an Opus track's SeekPreRoll - and
it is what `PacketAudioPlayer`'s `preRoll` argument is usually given. An application that wants a seek
to be indistinguishable from continuous playback should ask for more; ordinary music and speech converge
quickly.

### Reporting loss, and concealing it

Opus conceals loss for real. A codec with no concealment of its own can only answer a gap with silence
of the right length; Opus synthesizes a plausible continuation of the audio that went missing - the
pitch and the spectral shape carry on, and fade as the gap runs long.

There are two ways to say a packet went missing: an empty packet, the lengthless form, whose length is
taken to be as long as the last real packet decoded (or 20 ms before anything has been decoded); and
`AudioPacket.Loss(...)` with a duration or a frame count. `PacketAudioPlayer` routes it to the decoder
for you, in helpings, and fills anything the decoder declines with silence, so the audio after a gap
stays where it belongs instead of sliding earlier by the length of what was lost. Frames are counted per
channel at 48 kHz, the same unit as `PreSkipSamples`. A gap the source does not know about needs
nothing - feed the next packet you have - and an underrun is not loss: a reader that has not kept up is
a hiccup, not a hole in the timeline.

Calling `ConcealLoss` yourself, without a player, means looping. Concealment runs in whole 2.5 ms steps
(120 frames at 48 kHz) and never covers more than 120 ms (5760 frames) in one call, so a long gap takes
several calls. A chunk is rounded down to whole steps, so a 1000-frame gap comes back as 960 and then
40; that last call runs a whole 2.5 ms step but reports only the 40 frames that were asked for - the
return value is the length you may use, and the surplus sits past it in the buffer to be ignored. A
buffer too small to hold one 2.5 ms step conceals nothing and returns 0 rather than throwing. After
`Reset()` and before the first packet, concealment is silence: there is no previous audio to continue,
so the codec answers with zeros of exactly the length asked for. Concealment is media time - it advances
the decoder's state exactly as a decoded packet does, so no `Reset()` is needed afterwards, though the
first packet after a gap is decoded against the state the concealment left behind.

### Mono and stereo only

A family-1 (surround) header does not return null - null means "not my codec" and would end up reported
as "no registered packet decoder" - it throws `NotSupportedException`:

```text
Opus channel mapping family 1 (surround, 6 channels) is not supported by
this decoder; only mapping family 0 (mono/stereo) is supported.
```

The engine logs a factory exception and moves on to the next factory, so that sentence reaches the
engine log rather than being lost. Catch it yourself if you create the decoder directly through the
factory.

### The error model

An unusable stream throws `InvalidDataException` with a message that names the problem, and a corrupt
packet throws the same exception with a message saying the packet could not be decoded. A null stream
argument throws `ArgumentNullException`; a null or blank file name throws `ArgumentException`. Using a
disposed reader or writer throws `ObjectDisposedException`. Readers and writers are `IDisposable`;
dispose them.

## Examples

Playing a `.opus` file uses the ordinary media transport - the only Opus-specific line is the
registration:

```csharp
using CodeBrix.Audio.Opus;
using CodeBrix.Audio.Playback;

CodeBrixAudioOpus.Register();          // once, at start-up

var media = new AudioFilePlayer();
media.Load("voice-note.opus");         // Duration is available as soon as Load returns
media.Play();
```

Reading a file as float samples goes through `OpusFileReader` directly, and shows why the header's
sample rate must never drive a conversion:

```csharp
using CodeBrix.Audio.Opus;

using var reader = new OpusFileReader("music.opus");
// reader.WaveFormat is 48 kHz 32-bit IEEE float - Opus decodes at 48 kHz, always.
// reader.EncoderInputSampleRate is what the ENCODER was given (often 16000 for a voice
// note); it is informational and never used to convert anything.

var buffer = new byte[reader.WaveFormat.AverageBytesPerSecond];
int read;
while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
{
    // interleaved 32-bit floats in [-1, 1]
}
```

Encoding takes options and tags, and the `using` block is what finishes the file:

```csharp
using CodeBrix.Audio.Opus;

var options = new OpusFileWriterOptions
{
    Bitrate = 64_000,
    Profile = OpusEncodingProfile.Voice,
    UseVariableBitrate = true,
    Complexity = 8
};
options.Tags["TITLE"] = "Field recording";
options.Tags["ARTIST"] = "Me";

using (var writer = new OpusFileWriter("memo.opus", sampleRate: 48000,
                                       channels: 1, options))
{
    float[] mono = GenerateSamples();          // interleaved, in [-1, 1]
    writer.Write(mono, 0, mono.Length);
}
// Dispose (here, the end of the `using`) is what finishes the file.
```

Transcoding streams straight through, at whatever rate the source happens to be, because the writer
resamples:

```csharp
using CodeBrix.Audio.Opus;
using CodeBrix.Audio.Wave;

using var source = new AudioFileReader("input.wav");   // 32-bit float
using var writer = new OpusFileWriter("output.opus",
    source.WaveFormat.SampleRate, source.WaveFormat.Channels);

var buffer = new float[source.WaveFormat.SampleRate * source.WaveFormat.Channels];
int n;
while ((n = source.Read(buffer, 0, buffer.Length)) > 0)
{
    writer.Write(buffer, 0, n);
}
```

Packets lifted from a container play through `PacketAudioPlayer`, and the same registration serves both
seams:

```csharp
using CodeBrix.Audio.Opus;
using CodeBrix.Audio.Playback;
using CodeBrix.Audio.Wave;

CodeBrixAudioOpus.Register();           // the same call registers the packet seam
SharedAudioOutput.Configure(48000);     // Opus's only rate

var player = new PacketAudioPlayer();
player.Open("opus", codecPrivate, packetSource);   // your demultiplexer supplies both
player.Play();

// ...or, to decode packets without playing them:
using var decoder = SharedAudioOutput.CreatePacketDecoder("opus", codecPrivate);
```

## Using it in a CodeBrix.Platform application

There is no CodeBrix.Platform add-in to install: this is an add-on to CodeBrix.Audio, and a
CodeBrix.Platform application references it like any other library. After
`CodeBrixAudioOpus.Register()`, `.opus` plays through the
[AudioPlayer add-in](../platform/add-ins/AudioPlayer.md) and the
[GameEngine](CodeBrix.Platform.GameEngine.md) as well as the CodeBrix.Audio players. The consuming
application takes this dependency and makes the call - the add-ins never do.

In the GameEngine, `.opus` reaches every path a built-in format reaches: `AudioResourceManager` loads,
`SoundChannel` clips, `CachedSound` decode-once preload, the SFX voice pool, music tracks, and
`PlatformAudioFactory.Supports`. That falls out of how the engine resolves an extension - its own table
first, then CodeBrix.Audio's `AudioFileReaderRegistry` - so no engine code names Opus, and none needs
to. A `.opus` file preloads exactly like a `.ogg` rather than decoding on the audio thread.

> [!IMPORTANT]
> In the GameEngine, register before the first **load**, not before the first **play**. The engine
> resolves an audio extension when an asset is loaded, so `CodeBrixAudioOpus.Register()` has to run
> ahead of every `.opus` load - which includes any audio an `AssetsFile` brings in at start-up, before a
> line of game code runs. Get the order wrong and the load throws `NotSupportedException`; that message
> names this package and this call by name.

## Pitfalls

- **The 48 kHz rule.** An Opus stream always decodes at 48 kHz. The sample rate in an Opus header is the
  rate the encoder was given - 16000 for a typical messenger voice note, and permitted to be 0 - and
  RFC 7845 marks it informational. It is surfaced as `OpusFileReader.EncoderInputSampleRate` and must
  never drive a conversion: treat a 16 kHz voice note as 16 kHz and it plays three times too
  slow.
- **Call `SharedAudioOutput.Configure(48000)` at start-up.** When the shared output runs at the media's
  rate no conversion runs at all. Without it, an application that has already played a 44.1 kHz sound
  effect has started the output at 44.1 kHz, and every Opus track then plays through the interpolator.
- **`WaveOutEvent` does not resample.** Mixing a 44.1 kHz WAV and a `.opus` through `WaveOutEvent` sees
  one of them rejected by `Init`. Pin the rate, or use `AudioFilePlayer` / `SoundEffectClip`, which both
  convert.
- **The pre-skip.** An Ogg Opus granule position counts 48 kHz samples *including* the encoder's priming
  samples, so audible length is the final granule minus the pre-skip, and the first samples decoded are
  discarded. Get this wrong and every file reads a few milliseconds long and starts early, with a click.
  Both the reader and the writer handle it - which is why you should take `Length` and `TotalTime` from
  the reader rather than computing them from the file yourself.
- **Dispose the writer.** `OpusFileWriter` only produces a complete, correctly described file on
  `Dispose()`. An undisposed writer leaves a file that is missing its tail and misreports its length.
- **Stream ownership is not symmetric between the two constructors.** The file-name overload owns the
  file; the `Stream` overload does not. Dispose the stream you opened, and do not assume disposing the
  reader closed it.
- **The Ogg format-id sharing rule.** CodeBrix.Audio's metadata layer stamps every Ogg stream with the
  format identifier "ogg", whatever codec is inside, so `OpusCodecFactory` is offered Vorbis and Ogg
  FLAC streams and `VorbisCodecFactory` is offered Opus streams. Each sniffs with `OggCodecSniffer` and
  returns null for anything else, and each resets the stream position on entry, because the engine does
  not rewind between factories on that path.
- **Encoding is selected by the "opus" format id, not "ogg".** An encoder cannot sniff what it has not
  written yet, and "ogg" would not say which codec was meant.
- **`Register()` holds one instance of each factory on purpose.** `SharedAudioOutput.RegisterCodecFactory`
  and `RegisterPacketCodecFactory` both de-duplicate on the instance, so handing either a freshly
  constructed factory per call would register the codec repeatedly. If you register a factory by hand,
  hold your own single instance of each for the same reason.
- **`Register(AudioEngine)` is not `Register()`.** The overload registers both codec factories on the
  engine you pass, but does not register the `".opus"` file extension with `AudioFileReaderRegistry`, so
  `AudioFileReader` still will not open a `.opus` after it. Call the parameterless `Register()` for the
  shared-output path.
- **`Position` and `Length` are bytes,** not samples and not seconds. Divide by
  `Channels * sizeof(float)` for frames, or use `TotalTime`.
- **Register once, at start-up.** `Register()` takes a lock and is cheap after the first call, but
  calling it per file is pointless work in a hot path.
- **Prefer `SoundEffectClip` for short, repeated sounds.** Decoding Opus on every trigger is far more
  expensive than the playback itself; decoding is managed code with no native fast path.
  `AudioFilePlayer` is the right choice for long tracks.
- **Read in reasonably sized blocks.** Every `Read()` allocates a float array sized to the request, so
  thousands of tiny reads produce thousands of small allocations.
- **Seeking costs decoding.** Setting `Position` seeks the underlying Ogg stream by granule position; it
  is not free, and a scrubber that sets `Position` on every pointer-move event will do real work per
  event. Throttle it.
- **Encoder settings.** `Complexity` trades CPU for quality at a fixed bitrate; the default of 10 is
  right for offline encoding, and you drop it for real-time encoding on a slow device. Leave
  `UseVariableBitrate` on - constant bitrate spends the same bits on silence as on a chorus. Use the
  `Voice` profile for speech at low bitrates, where the `Music` profile would not stay clear.

## Samples and tools in the repository

The repository ships one package and has no sample applications and no demos. Everything below is
developer tooling and test data: none of it is packed, and none of it is needed to consume the package.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Test fixture generator | The one script that produced every generated audio fixture, and rewrites their manifest | [`tools/make_test_fixtures`](https://github.com/ellisnet/CodeBrix.Audio.Opus/tree/main/tools/make_test_fixtures) |
| Audio fixtures | The everyday stereo case, the voice-note shape where the declared rate and the decode rate disagree, a sweep whose instantaneous frequency identifies the position so a seek can be verified from the audio itself, a truncated file that must fail cleanly, and a Vorbis file the factory must decline | [`tests/Assets/audio`](https://github.com/ellisnet/CodeBrix.Audio.Opus/tree/main/tests/Assets/audio) |
| Test suite | The best worked example of every public API | [`tests/CodeBrix.Audio.Opus.Tests`](https://github.com/ellisnet/CodeBrix.Audio.Opus/tree/main/tests/CodeBrix.Audio.Opus.Tests) |

The fixture script installs nothing; it needs an FFmpeg build carrying the Opus and Vorbis encoders, and
says so. Regenerate deliberately rather than as a side effect of adding one fixture: the Ogg files never
come out byte-identical between runs, because an Ogg muxer assigns a random stream serial number each
time and the encoder version lands in the vendor string. Never write a test that assumes those bytes are
stable.

Run the tests with `dotnet test CodeBrix.Audio.Opus.slnx`. Tests that make sound are opt-in through the
same environment variable CodeBrix.Audio's own sounding tests use, so one switch governs the family:

```bash
CODEBRIX_AUDIO_RUN_PLAYBACK_TESTS=1 dotnet test CodeBrix.Audio.Opus.slnx
```

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Audio.Opus/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio.Opus/blob/main/AGENT-README.txt) |
| Tools and other non-package content | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Audio.Opus/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Audio.Opus/blob/main/README-INDEX.txt) |
| Provenance and licensing of included open source code | [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio.Opus/blob/main/THIRD-PARTY-NOTICES.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.Audio.Opus.Tests](https://github.com/ellisnet/CodeBrix.Audio.Opus/tree/main/tests/CodeBrix.Audio.Opus.Tests) |
| The companion guide for the library this builds on | [CodeBrix.Audio AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/AGENT-README.txt) |

XML documentation ships alongside the assembly.

## License

CodeBrix.Audio.Opus is licensed under the BSD 3-Clause License, and the license is also named in the
package ID (`CodeBrix.Audio.Opus.BsdLicenseForever`). License acceptance is required. Note that the
package it depends on, `CodeBrix.Audio.MitLicenseForever`, is licensed under the MIT License, while this
one is BSD 3-Clause. For the provenance and licensing of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio.Opus/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Audio](CodeBrix.Audio.md) - the library this extends, and the owner of every player it reaches
- [Playback and sound effects](audio/playback.md) - the two codec seams this package registers with, in full
- [CodeBrix.Platform.GameEngine](CodeBrix.Platform.GameEngine.md) - where the register-before-first-load rule applies
- [AudioPlayer add-in](../platform/add-ins/AudioPlayer.md) - playing audio from a CodeBrix.Platform application
- [ellisnet/CodeBrix.Audio.Opus on GitHub](https://github.com/ellisnet/CodeBrix.Audio.Opus) - source, tests and tools
