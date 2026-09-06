<sub>[CodeBrix](../../../README.md) › [Libraries](../README.md) › [CodeBrix.Audio](../CodeBrix.Audio.md) › Reading and writing files</sub>

# Reading and writing audio files

**This page covers the file layer of [CodeBrix.Audio](../CodeBrix.Audio.md): reading WAV, MP3, Ogg
Vorbis, FLAC and AIFF, writing WAV, AIFF and Standard MIDI Files, reading ID3v2 and Vorbis-comment
tags, and the reader registry that lets another package add a format by file name.** All decoding is
managed code with no platform-specific interop, so a file behaves identically on Windows, macOS and
Linux. Formats reach the players through a separate seam, described in [Playback](playback.md).

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Audio](https://github.com/ellisnet/CodeBrix.Audio) |
| **Packages** | [`CodeBrix.Audio.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |

## What it does

- Reads WAV (`.wav`): 8/16/24/32-bit PCM and 32/64-bit IEEE float, including files written as
  `WAVE_FORMAT_EXTENSIBLE`, which is how most 24- and 32-bit WAVs are produced.
- Reads MP3 (`.mp3`) through a fully managed MPEG audio decoder covering MPEG-1/2/2.5 Layer I/II/III,
  and trims the encoder delay and padding so a decoded MP3 lines up with the WAV it was encoded from.
- Reads Ogg Vorbis (`.ogg`) with exact `TotalTime` and sample-accurate seeking, because a Vorbis
  stream records its own length.
- Reads FLAC (`.flac`) losslessly, at the file's own bit depth widened to the next standard container.
- Reads and writes AIFF (`.aiff`), and writes WAV.
- Reads and writes Standard MIDI Files - see [MIDI files](midi-files.md).
- Reads MP3 ID3v2 tag blocks, and the Vorbis comments an Ogg Vorbis or FLAC stream carries.
- Opens a file by extension through `AudioFileReader`, and lets another package add an extension to
  that path through `AudioFileReaderRegistry`.

## When to use it

Use the readers when you want the samples themselves - to analyze them, to convert them, to feed them
into a provider chain, or to hand them to something that takes an `IWaveProvider`. When you want the
audio to come out of the speakers, [Playback](playback.md) is the page you want: the players decode
through the bundled engine, convert sample rates for you, and stream a long file from disk instead of
holding it in memory.

Writing is deliberately narrow. This library writes `.wav` (`WaveFileWriter`), `.aiff`
(`AiffFileWriter`) and Standard MIDI Files (`MidiFile.Export`), and nothing else. There is no lossy or
lossless encoding of compressed formats here; for `.opus` writing, take
[CodeBrix.Audio.Opus](../CodeBrix.Audio.Opus.md). There is no editing of tags in place either -
`Id3v2Tag` reads a tag from a stream and `Id3v2Tag.Create` builds one from key/value pairs, and Vorbis
comments are read-only. And there is no streaming from a URL: readers take a file name or a `Stream`,
and fetching over the network is your code's job.

## Getting started

```bash
dotnet add package CodeBrix.Audio.MitLicenseForever
```

```csharp
using CodeBrix.Audio.Wave;       // readers/writers, WaveFormat, MP3 frames, ID3
```

The shortest useful path reads any of the four built-in formats as 32-bit float samples:

```csharp
using CodeBrix.Audio.Wave;

using var reader = new AudioFileReader("track.ogg");   // .wav / .mp3 / .ogg / .flac
// reader.WaveFormat is 32-bit IEEE float; .SampleRate, .Channels available.
var buffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];
int samplesRead;
while ((samplesRead = reader.Read(buffer)) > 0)
{
    // buffer[0..samplesRead] holds interleaved float samples in [-1, 1]
}
// reader.Volume = 0.5f;  // optional gain applied to returned samples
```

`AudioFileReader` picks the decoder from the extension and always exposes float. Every other reader
hands back raw PCM bytes, which is the first thing to know about them.

## Key concepts

### One reader per format, and one that dispatches

`WaveFileReader`, `Mp3FileReader`, `OggVorbisFileReader`, `FlacFileReader` and `AiffFileReader` each
open one format as a `WaveStream`. `AudioFileReader` opens `.wav`, `.mp3`, `.ogg` or `.flac` by file
extension and exposes 32-bit float samples.

The specialized readers carry what only their format has. `OggVorbisFileReader` reports exact
`TotalTime`, seeks sample-accurately, and exposes the stream's Vorbis comments as `.Tags` plus
`.EncoderVendor`. `FlacFileReader` exposes `.Tags` and `.SourceBitsPerSample`. `Mp3FileReader` exposes
`.EncoderDelay`, `.EncoderPadding` and `.GaplessTrimming`.

To turn any `WaveStream` into float samples, call `.ToSampleProvider()`:

```csharp
using var wav = new WaveFileReader("clip.wav");
var samples = wav.ToSampleProvider();                  // ISampleProvider (float)
var buf = new float[4096];
int n = samples.Read(buf);
```

### MP3 is gapless by default

Where an MP3 carries the Xing, LAME or Lavc encoder-delay fields, the priming samples at the front and
the padding at the end are trimmed. Sample position 0 is then the first sample of the original audio
and `Length` excludes the padding, so a decoded MP3 lines up with the WAV it was encoded from, sample
for sample.

`.EncoderDelay` and `.EncoderPadding` report what the file declared. `.GaplessTrimming` turns the
behavior off, and must be set before the first read - afterwards it throws. A file with no such fields
is unaffected either way.

### FLAC bit depth

`FlacFileReader` hands back the file's own depth widened to the next standard container: 16-bit for
depths up to 16, 24-bit for 17 to 24, and 32-bit above that. Depths that are not a whole number of
bytes are left-shifted into that container, so a 12-bit file's values are scaled up by 16 - the same
thing other FLAC-to-WAV converters do. The PCM is exactly what was encoded; FLAC is lossless and seeks
exactly.

### Writing a WAV

```csharp
var format = new WaveFormat(rate: 44100, bits: 16, channels: 1);
using (var writer = new WaveFileWriter("out.wav", format))
{
    float[] mono = GenerateSamples();                  // your samples in [-1, 1]
    writer.WriteSamples(mono, 0, mono.Length);
}
// Or pipe an ISampleProvider straight to disk:
// WaveFileWriter.CreateWaveFile16("out.wav", someSampleProvider);
```

`WaveFormat` carries the sample rate, channel count, bit depth and encoding, and
`WaveFormat.CreateIeeeFloatWaveFormat(rate, channels)` builds the float form.
`WaveFormatExtensible`, `Mp3WaveFormat` and `AdpcmWaveFormat` are the subclasses you meet when
inspecting a file's header rather than its samples.

AIFF sits beside the WAV pair: `AiffFileReader` reads and `AiffFileWriter` writes, with
`AiffFileWriter.CreateAiffFile(filename, WaveStream)` as the one-liner.

### Tags

`Id3v2Tag.ReadTag(Stream)` reads an ID3v2 tag block from an MP3 stream and returns null when the file
has none:

```csharp
using var fs = File.OpenRead("song.mp3");
var tag = Id3v2Tag.ReadTag(fs);                         // null if no ID3v2 tag
if (tag != null) { /* tag.RawData is the raw tag bytes */ }
```

Vorbis comments are exposed as `.Tags` on `OggVorbisFileReader` and `FlacFileReader`, mapping an
uppercase field name to its values.

### The reader registry

`AudioFileReaderRegistry` is the by-extension seam. A package that adds a format registers a
`WaveStream` factory for its extension, and `AudioFileReader` opens that extension afterwards, as do
`AudioFileReaderRegistry.OpenFile` and everything else built on the registry:

```csharp
AudioFileReaderRegistry.Register(".opus", s => new OpusFileReader(s));
```

`AudioFileReaderRegistry.Supports(fileNameOrExtension)` asks whether an extension is covered.

> [!IMPORTANT]
> The factory is handed a stream it does **not** own. Do not close it, and do not make your reader
> close it either; the registry opened the file and keeps the handle. `OpenFile` therefore returns a
> `FileOwningWaveStream` pairing the two: disposing it disposes your reader and *then* closes the
> file, and its `.Reader` property gets callers back to your concrete type.

The other half of adding a format - teaching the players to identify it by content - is the codec
seam, described in [Playback](playback.md). [CodeBrix.Audio.Opus](../CodeBrix.Audio.Opus.md) is the
worked example of a package that does both with one call.

### The companding codecs

`ALawEncoder` / `ALawDecoder` and `MuLawEncoder` / `MuLawDecoder` in `CodeBrix.Audio.Codecs` convert
companded 8-bit telephony audio, one sample at a time (`LinearToALawSample` / `ALawToLinearSample`) or
in bulk (`Decode(ReadOnlySpan<byte>, Span<short>)`). They are callable directly and are **not** wired
into the WAV reader, so converting an A-law WAV means calling them yourself.

### The plumbing types

`RawSourceWaveStream` wraps headerless PCM - a `byte[]` or a `Stream` plus a `WaveFormat` - as a
`WaveStream`. `IgnoreDisposeStream` hands a `Stream` to something that disposes what it is given
without losing the stream. `WaveRecorder` is a pass-through `IWaveProvider` that also writes
everything read through it to a `.wav` file.

## Examples

Decoding one specific format explicitly, with the members only that format has:

```csharp
using var mp3 = new Mp3FileReader("song.mp3");           // WaveStream of PCM
var floats = mp3.ToSampleProvider();

using var ogg = new OggVorbisFileReader("music.ogg");    // WaveStream of 32-bit float
var duration = ogg.TotalTime;                            // exact, no scanning
ogg.Position = ogg.WaveFormat.AverageBytesPerSecond * 30; // seek to 0:30

using var flac = new FlacFileReader("album-track.flac"); // WaveStream of PCM, lossless
var depth = flac.SourceBitsPerSample;                    // 16 / 24 / ...
var title = flac.Tags.TryGetValue("TITLE", out var t) ? t[0] : null;
```

Opening a registered extension through the registry, and disposing the pair correctly:

```csharp
using CodeBrix.Audio.Opus;
using CodeBrix.Audio.Wave;

CodeBrixAudioOpus.Register();

using var stream = AudioFileReaderRegistry.OpenFile("clip.opus");
// A FileOwningWaveStream: disposing it disposes the OpusFileReader and THEN
// closes the file. `using` it, or the file stays locked on Windows.
// stream.WaveFormat is 48 kHz 32-bit float.
```

## Pitfalls

- **Float versus bytes.** `WaveFileReader` and `Mp3FileReader` are `WaveStream`s that yield raw PCM
  bytes. To get normalized float samples call `.ToSampleProvider()`, or use `AudioFileReader`, which
  always exposes 32-bit float.
- **Non-PCM WAV files throw.** PCM and IEEE float are supported, `WAVE_FORMAT_EXTENSIBLE` included,
  but A-law, mu-law and other genuinely non-PCM WAV files throw `InvalidOperationException`. There is
  no managed codec conversion, and the companding codecs are not auto-wired.
- **Dispose readers and writers.** A `WaveFileWriter` flushes a valid RIFF header only on `Dispose`,
  so an undisposed writer produces a corrupt file. Use `using`.
- **Files opened through the reader registry stay locked until you dispose what you got back.**
  Dropping the reference without disposing leaves the file locked on Windows until the finalizer runs,
  so a later `File.Delete` or `File.Move` throws `IOException` saying the file is in use by another
  process - and it looks intermittent, because it depends on garbage-collection timing. This covers
  `SfzSampleData.Load` and `AudioFileReader` for any extension added with `Register`; the four
  built-in extensions take a different path inside `AudioFileReader`.
- **There is no resampler in the reader layer.** The `WaveStream` readers hand back audio at the
  file's own rate and never convert it. The playback types do convert - see [Playback](playback.md).
- **Seeking a Vorbis stream in the managed reader.** `OggVorbisFileReader.Position` is exact, but
  seeking into the middle of a Vorbis packet leaves the decoder without the previous packet's overlap
  history, so up to one block after a seek can differ from a sequential read of the same region before
  the two converge. That is fine for scrubbing a transport; for a seamless loop point, play through
  the engine, whose native decoder reconstructs the overlap. FLAC has no such caveat.
- **A single reader or stream instance is not thread-safe.** Give each thread its own reader.
- **Invalid or corrupt files throw standard exceptions** - `FormatException`,
  `EndOfStreamException`, `ArgumentException`.

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/AGENT-README.txt) |
| Reader and writer tests | [tests/CodeBrix.Audio.Tests](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tests/CodeBrix.Audio.Tests) |
| What each audio fixture is for | [tests/Assets/audio/AUDIO-FIXTURES.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/tests/Assets/audio/AUDIO-FIXTURES.txt) |

## License

CodeBrix.Audio is licensed under the MIT License, and the license is also named in the package ID
(`CodeBrix.Audio.MitLicenseForever`). For the provenance and licensing of open source code included in
this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [Playback](playback.md) - getting those files out of the speakers, and the codec seam
- [MIDI files](midi-files.md) - the other file format this library reads and writes
- [CodeBrix.Audio](../CodeBrix.Audio.md) - the front door of the audio section
