<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.VideoPlayback.Dav1d</sub>

# CodeBrix.VideoPlayback.Dav1d

**CodeBrix.VideoPlayback.Dav1d is AV1 video decoding for CodeBrix.VideoPlayback, for applications that
play AV1 video files.** CodeBrix.VideoPlayback deliberately ships no video decoder of its own, because a
decoder carries a license and a set of native binaries that not every application wants; an application
that plays AV1 references this package and makes one call at start-up, and nothing else in it ever names
a decoder type. Use it from any .NET 10 application, or from a CodeBrix.Platform application.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.VideoPlayback.Dav1d](https://github.com/ellisnet/CodeBrix.VideoPlayback.Dav1d) |
| **Packages** | [`CodeBrix.VideoPlayback.Dav1d.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoPlayback.Dav1d.BsdLicenseForever) |
| **License** | BSD 2-Clause; see [License](#license) |
| **Requires** | .NET 10 or later, and a presenter to draw the frames. No native-asset package to add |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Windows x64 and ARM64, macOS on Intel and Apple Silicon, Linux x64, ARM64 and RISC-V 64 |

## What it does

- Bundles the dav1d AV1 decoder and registers it into
  [CodeBrix.VideoPlayback](CodeBrix.VideoPlayback.md)'s decoder seam with one call, so AV1 plays from
  WebM, Matroska and the `.cbv` container wherever the playback library can read them.
- Carries native decoder libraries for every supported platform inside the package, found automatically
  whether an application publishes for one runtime identifier or for none.
- Decodes straight into the playback session's own frame-buffer pool. There is no copy between what the
  decoder produces and what a presenter uploads to the graphics device, and playback allocates no
  buffers once it is warm. That is the contract, and the test suite proves it.
- Describes a stream before a single frame is decoded, through `Dav1dDecoderFactory.TryProbe`:
  dimensions, plane layout, bit depth and color, so a host can size its surface first.
- Handles 8, 10 and 12-bit content in 4:2:0, 4:2:2, 4:4:4 and monochrome, with film grain synthesized by
  the decoder and HDR metadata carried through to the frame.
- Checks decoded output against AV1 conformance streams covering 8-bit and 10-bit, 4:2:0 and 4:4:4, an
  odd frame size, and film grain both applied and not.
- Loads the native library and checks its API version at `Register()` time, so a missing or wrong native
  fails at start-up with a message naming every path that was looked in - not later, in the middle of
  opening a video.

## When to use it

Add this package to any application that has to play AV1, which is every application that plays a
`.cbv` file or a WebM file this family authors. It brings
[`CodeBrix.VideoPlayback.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoPlayback.MitLicenseForever)
with it, which in turn brings
[`CodeBrix.Audio.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MitLicenseForever);
those are the whole of what a video player needs to read and decode a file.

Two things it does not bring, and an application usually needs:

- A presenter, to draw the decoded frames -
  [`CodeBrix.VideoPlayback.Skia.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoPlayback.Skia.MitLicenseForever)
  for a SkiaSharp application, or the CodeBrix.Platform video player element. Without one, frames are
  decoded and never shown.
- An Opus decoder, when the files carry Opus audio:
  [`CodeBrix.Audio.Opus.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.Opus.BsdLicenseForever).
  Vorbis audio needs nothing extra.

What is out of scope:

- No hardware decoding. This is a software decoder, with no VA-API, VideoToolbox or D3D11 path.
- No other codec. It serves `"av01"` and nothing else; another codec would be a separate package
  registering the same way.
- No encoding.
- No presenting, drawing or color conversion. Frames come out as planar YUV with their color metadata
  attached; turning that into pixels on a screen is a presenter's job.
- No AV1 alpha, and no HDR tone-mapping. HDR metadata is decoded and carried on the frame; nothing maps
  it down to a standard-range display.
- No container reading. That is CodeBrix.VideoPlayback's work; this package sees packets, never files.

There is deliberately no module initializer either: one would run whenever the assembly was touched,
keeping the decoder and every native library beside it alive through a trimmed publish even in an
application that never plays a video. That is why registration is an explicit call.

## Getting started

```bash
dotnet add package CodeBrix.VideoPlayback.Dav1d.BsdLicenseForever
```

```csharp
using CodeBrix.VideoPlayback;            // VideoPlaybackSession, options
using CodeBrix.VideoPlayback.Dav1d;      // the registration call and options
using CodeBrix.VideoPlayback.Decoding;   // VideoStreamInfo, VideoCodecIds
using CodeBrix.VideoPlayback.Frames;     // VideoFrame, if you handle frames
```

Everything an ordinary application touches is in `CodeBrix.VideoPlayback.Dav1d`. A tool that drives a
decoder by hand also takes `using CodeBrix.VideoPlayback.Dav1d.Decoding;`, which holds the public
decoder type itself.

This is the whole of the integration.

```csharp
using CodeBrix.VideoPlayback;
using CodeBrix.VideoPlayback.Dav1d;

CodeBrixVideoPlaybackDav1d.Register();      // once, at start-up

VideoPlaybackSession session = new VideoPlaybackSession();
session.Open("clip.webm");
session.Play();
```

Notice what is absent: no decoder type is named, no path is configured, and no native asset package is
added. The native libraries arrive automatically - for an ordinary build they land in
`runtimes/<rid>/native/` beside the application, and for a runtime-specific publish the build system
copies the one that is needed. Both layouts are found.

## Key concepts

### Registration is the front door

```csharp
static void Register()
static void Register(VideoPlaybackSession session)
static bool IsRegistered { get; }
static bool Unregister()
static Dav1dDecoderFactory Factory { get; }
```

`Register()` makes AV1 decoding available to every playback session in the process. Call it once at
start-up; it is safe to call again, because it registers once, and it is safe from any thread. The
session overload makes AV1 available to one session without touching the process-wide registry, which
suits an application that plays several things at once with different needs, and suits tests.
`Register(session)` does not set `IsRegistered`, because it does not change the process-wide registry.
`Unregister()` takes the factory back out and clears `IsRegistered`; an application has no reason to
call it - it is there so a test can undo itself. `Factory` is the single factory instance, for handing
to `VideoDecoders.Register` or `VideoPlaybackSession.RegisterDecoderFactory` when the application manages
the decoder list by hand.

Three diagnostics sit beside them: `NativeVersion`, `NativeApiVersion` and `NativeLibraryPath`, which is
where the native was actually loaded from. The first two load the native library if it is not loaded;
the third reads null until something has.

### The factory and the sequence-header probe

`Dav1dDecoderFactory` reports `FactoryId` of `"CodeBrix.VideoPlayback.Dav1d"`, `SupportedCodecIds` of
`{ "av01" }` and `Priority` of `0`.

```csharp
static bool TryProbe(ReadOnlySpan<byte> data, out VideoStreamInfo info)
static bool TryProbe(VideoPacket packet, out VideoStreamInfo info)
```

`TryProbe` reads an AV1 sequence header and describes the stream without decoding anything: width,
height, plane layout, bit depth, color primaries, transfer, matrix, range and chroma siting. Use it to
size a surface, choose a texture format, or decide whether to play a file at all, before frame one. Feed
it a track's codec-private data - an av1C record, whose four-byte record header is recognized and
stepped over - or the first packet of the track, because every AV1 key frame carries a sequence header.
Data with no sequence header answers false rather than throwing.

A sequence header states the coded picture size, so `Width`/`Height` and `DisplayWidth`/`DisplayHeight`
come back equal from the probe. A render size that differs from the coded size is a property of a frame
header, so it appears only once frames arrive, on `VideoFrame`'s own `DisplayWidth`/`DisplayHeight`. Size
a surface from the probe and be ready to accept a different display size from the first frame.

### The decoder type

`Dav1dVideoDecoder : IVideoDecoder` lives in the `CodeBrix.VideoPlayback.Dav1d.Decoding` namespace. A
playback session creates one for you, and `Dav1dDecoderFactory.CreateDecoder` returns one as an
`IVideoDecoder`; construct it directly only in a tool that has its own packet source. The decode loop -
`SendPacket`, `TryReceiveFrame`, `Drain`, `Flush`, `Dispose` - is `IVideoDecoder`'s, and is the same
object however you obtained it. One thread at a time, like every decoder; the frames it produces may be
read from any thread.

The constructor is `Dav1dVideoDecoder(string codecId, ReadOnlyMemory<byte> codecPrivate,
VideoDecoderOptions options)`, and `options.BufferPool` must be set - this decoder writes into a pool
supplied by its host, and a null pool is a `Dav1dException` naming the property. A session sets it for
you. `codecPrivate` may be an av1C record, bare configuration OBUs, or nothing at all.

Beyond `IVideoDecoder`, the type carries `Info` (parsed from codec-private data at construction, then
kept up to date as frames arrive), `CodecId`, `SupportsExternalBuffers` (always true), `ThreadCount`
(0 means the decoder counted the logical cores itself), `FrameDelay` (how many frames are buffered
internally, never less than 1) and `LastLogMessage` (the most recent native diagnostic, or null; the same
text is folded into a `Dav1dException` when decoding fails). Casting from `IVideoDecoder` is needed only
for those last three.

### Decoder options

Set a `Dav1dDecoderOptions` as `VideoPlaybackOptions.DecoderOptions` before constructing a session; a
session given the plain `VideoDecoderOptions` gets the defaults for everything below.

```csharp
VideoPlaybackOptions options = new VideoPlaybackOptions
{
    DecoderOptions = new Dav1dDecoderOptions
    {
        MaxFrameDelay = 1,   // first frame out as soon as it is decoded
        Threads = 2,
    },
};

VideoPlaybackSession session = new VideoPlaybackSession(options);
```

Inherited from `VideoDecoderOptions`: `Threads` (0 counts the logical cores, 1 to 256 otherwise),
`MaxFrameDelay` (0 chooses for throughput; 1 gives the first frame as soon as it is decoded),
`ApplyFilmGrain` (true by default), `FrameSizeLimit` (8192 x 8192 luma samples by default, the guard
against a hostile file) and `BufferPool`.

Added by this package: `OperatingPoint` (0 to 31, default 0 - which operating point of a scalable stream
to decode), `AllLayers` (true, output every spatial layer), `StrictStdCompliance` (false; true refuses a
stream over compliance violations that do not affect decoding - a tool's setting, not a player's),
`OutputInvisibleFrames` (false; an analysis setting, with which some pictures appear twice) and
`Logger`.

`MaxFrameDelay` is a trade, not a quality setting: 1 shortens the delay between a packet going in and a
frame coming out, at the cost of throughput, because frame-level parallelism has nowhere to work. Use it
for short clips that must start instantly and leave it at 0 for long-form playback. Leave `Threads` at 0
unless you have measured otherwise.

### Back-pressure in the decode loop

`SendPacket` returning false is not an error, and the packet has not been taken. It means the decoder is
holding as much data as it can and wants frames pulled out first. Pull, then offer the same packet
again. Treating false as a failure loses a frame; treating it as success loses a frame silently, which
is worse. In short: `SendPacket` returning false means full, so drain and re-offer; `TryReceiveFrame`
returning false means nothing is ready yet; call `Drain` and then pull until false at end of stream; call
`Flush` after a seek.

### The zero-copy frame contract

The frames are already where you want them. Do not copy a frame's planes before uploading them: they are
64-byte aligned, their strides are multiples of 64 bytes, and 10-bit and 12-bit samples are little-endian
16-bit words justified towards the least significant bit - which is exactly what an R8 or R16 texture
upload wants. Copying undoes the whole point of the package.

For asynchronous uploads, use the fence hook. Put an `IVideoFrameFence`, or a `Func<bool>`, in
`frame.Buffer.Tag` before starting an upload and dispose the frame as usual; the pool holds the memory
back until the fence signals. It is the pool's slot to read and the presenter's to write - this package
never touches it.

Playback allocates nothing once it is warm: the frame buffers, the frame objects over them, the pinned
blocks packets are handed to the decoder in, and the binding's own per-frame bookkeeping all come from
pools. `PinnedFrameBufferPool.GetStatistics().Allocations` is the number to watch - in a healthy steady
state it stops rising after the first few frames.

### Frame reference counting

A frame is reference-counted: whoever obtains one owns one reference and must dispose it, and anyone who
needs it to outlive that scope calls `Retain()` and disposes the result in turn. Reading a frame after
its last reference has gone reads somebody else's picture, because both the buffer and the frame object
have been recycled by then.

A buffer does not come back the moment you dispose a frame, and that is correct: the decoder keeps
decoded pictures alive as prediction references for later frames, so the buffer returns to the pool when
the application and the decoder have both finished with it. Expect the pool's `Live` count to sit at
several buffers throughout playback, and expect the last few to come back only when the decoder is
disposed.

### Errors

`Dav1dException : VideoPlaybackException` carries `ErrorName` - the C errno name the decoder returned:
`"EPERM"`, `"ENOENT"`, `"EIO"`, `"ENOMEM"`, `"EINVAL"`, `"ERANGE"`, `"ENOPROTOOPT"`, with anything else
reported by its number - and `ErrorCode`, the raw negative value. Catching `VideoPlaybackException`
catches these too, so an application does not have to know which decoder package is installed.

## Examples

Size a surface before anything is decoded, from the track's codec-private data.

```csharp
using CodeBrix.VideoPlayback;
using CodeBrix.VideoPlayback.Dav1d;
using CodeBrix.VideoPlayback.Decoding;

session.Open("clip.webm");

if (Dav1dDecoderFactory.TryProbe(session.VideoTrack.CodecPrivate.Span, out VideoStreamInfo info))
{
    // info.Width, info.Height, info.BitDepth, info.Layout, info.Color,
    // info.MaxSampleValue (255, 1023 or 4095), info.ChromaShiftX/Y
    AllocateSurface(info.Width, info.Height);
}
```

Refuse an unreasonable file. `FrameSizeLimit` refuses the whole stream rather than scaling anything
down, and the message names the limit that was set and what the file asked for.

```csharp
VideoPlaybackOptions options = new VideoPlaybackOptions
{
    DecoderOptions = new Dav1dDecoderOptions
    {
        FrameSizeLimit = 1920L * 1080L,   // nothing bigger than 1080p
    },
};

try
{
    session.Open(untrustedFile);
    session.Play();
}
catch (Dav1dException failure) when (failure.ErrorName == "ERANGE")
{
    // The message names the limit that was set and what the file asked for.
}
```

Drive the decoder directly, with the complete back-pressure loop. This is what a transcoding or
analysis tool with its own packet source writes.

```csharp
PinnedFrameBufferPool pool = new PinnedFrameBufferPool();
VideoDecoderOptions options = new VideoDecoderOptions { BufferPool = pool };

using IVideoDecoder decoder = CodeBrixVideoPlaybackDav1d.Factory
    .CreateDecoder(VideoCodecIds.Av1, codecPrivate, options);

// The object is a Dav1dVideoDecoder. Cast to it only for the three things
// IVideoDecoder does not carry: ThreadCount, FrameDelay, LastLogMessage.

foreach (VideoPacket packet in packets)
{
    // FALSE means the decoder is full, not that anything failed. Pull a
    // frame and offer THE SAME packet again.
    while (!decoder.SendPacket(packet))
    {
        if (decoder.TryReceiveFrame(out VideoFrame parked))
        {
            using (parked) Handle(parked);
        }
    }

    while (decoder.TryReceiveFrame(out VideoFrame frame))
    {
        using (frame) Handle(frame);
    }
}

decoder.Drain();
while (decoder.TryReceiveFrame(out VideoFrame frame))
{
    using (frame) Handle(frame);
}
```

The minimum viable project file is two lines of package reference, and the presenter is the second one.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.VideoPlayback.Dav1d.BsdLicenseForever" Version="*" />
    <!-- and a presenter, and CodeBrix.Audio.Opus.BsdLicenseForever for Opus audio -->
  </ItemGroup>
</Project>
```

## Using it in a CodeBrix.Platform application

The registration call is the whole of the integration, on every head: the native libraries are found for
both an ordinary build and a runtime-specific publish, on every supported runtime identifier, with no
head-specific step. What a CodeBrix.Platform application still has to choose is the presenter that draws
the decoded frames - either the [VideoPlayer add-in](../platform/add-ins/VideoPlayer.md), which gives you
a video player element to put in a page, or
[`CodeBrix.VideoPlayback.Skia.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoPlayback.Skia.MitLicenseForever)
drawing into a canvas the application owns.

> [!IMPORTANT]
> Registering a decoder does not draw anything. Frames that are decoded and never taken from the
> presenter are disposed and recycled, so the symptom of a missing presenter is a video that plays
> perfectly and shows nothing.

## Pitfalls

- Treating `SendPacket` returning false as a failure. It means the decoder is full and the packet was
  not taken: pull frames, then offer the same packet again.
- Comparing decoded output against a recorded hash without saying which output you meant. AV1 film grain
  is synthesized by the decoder from parameters in the bitstream, which means one stream has two
  different correct outputs - with grain and without.
- Lowering `FrameSizeLimit` and forgetting that it refuses the whole stream. A legal file above the
  limit stops playing, with `ERANGE` and a message naming the limit you set.
- Expecting log messages to carry their numbers. They arrive with their printf conversions unexpanded,
  so you see `Frame size %dx%d exceeds limit %u` rather than the values; expanding a C variadic argument
  list from managed code is not portable across the architectures this package supports, so the binding
  does not try. Where a number really matters - the frame-size limit - the exception message states it
  itself.
- Holding a frame past its `Dispose`. Call `Retain()` and dispose the result when it must outlive the
  scope.
- Expecting a buffer back the moment a frame is disposed. The pool's `Live` count sits at several
  buffers throughout playback, and the last few return only when the decoder is disposed. That is
  correct.
- Handing an av1C record to something that wants bare OBUs. A Matroska or `.cbv` track's codec-private
  data is an av1C configuration record: four bytes of its own, then the configuration OBUs. `TryProbe`
  recognizes and steps over that header; code that hands the record to something else may need to skip
  it by hand.
- Forgetting `options.BufferPool` when constructing a decoder by hand. A null pool is a `Dav1dException`
  naming the property.
- Sizing anything from the probe's display size. The probe reports coded sizes only; be ready to accept
  a different display size from the first frame.
- Copying a frame's planes before uploading them. They are already aligned and laid out for a texture
  upload; copying undoes the point of the package.
- Turning `ApplyFilmGrain` off for speed. It changes what you see, so it is a content decision, not only
  a performance one.
- Using one decoder from several threads at once. One thread at a time; the frames it produces may be
  read from anywhere.

## Samples and tools in the repository

This repository has no sample applications. Three folders hold things that never ship: the tooling that
builds the native libraries, the streams the binding is checked against, and the files the end-to-end
tests play.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Native build tooling | Everything needed to build the native decoder libraries, self-contained - no clone, no download, no fetch during a build. No build in this repository compiles any of it: the libraries are built where they can be built, and committed | [`dav1d-native-tools`](https://github.com/ellisnet/CodeBrix.VideoPlayback.Dav1d/tree/main/dav1d-native-tools) |
| Conformance test vectors | Small AV1 bitstreams in IVF containers plus `EXPECTED.md5`, covering 8-bit and 10-bit, 4:2:0 and 4:4:4, an odd frame size, and film grain both applied and not; used by every native build and by the managed test suite | [`dav1d-native-tools/test-vectors`](https://github.com/ellisnet/CodeBrix.VideoPlayback.Dav1d/tree/main/dav1d-native-tools/test-vectors) |
| End-to-end assets | Three short WebM files - AV1 with Opus audio, with Vorbis audio, and with no audio - played whole through a `VideoPlaybackSession` | [`tests/assets`](https://github.com/ellisnet/CodeBrix.VideoPlayback.Dav1d/tree/main/tests/assets) |

The audible test is opt-in: it runs only when `CODEBRIX_AUDIO_RUN_PLAYBACK_TESTS=1` is set in the
environment, and without it the test skips itself with a message saying so. That is the family's
convention for anything that needs real hardware - a headless machine, a container, or a laptop with the
speakers muted must be able to run the whole suite green.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.VideoPlayback.Dav1d/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.VideoPlayback.Dav1d/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.VideoPlayback.Dav1d/blob/main/README-INDEX.txt) |
| The three folders that never ship | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.VideoPlayback.Dav1d/blob/main/EXTRAS-README.txt) |
| Tests (the fullest set of worked examples: the decode loop with back-pressure, the probe, the film-grain options, the frame-size guard, and whole files played through a session) | [tests/CodeBrix.VideoPlayback.Dav1d.Tests](https://github.com/ellisnet/CodeBrix.VideoPlayback.Dav1d/tree/main/tests/CodeBrix.VideoPlayback.Dav1d.Tests) |

Opening files, demuxing, timing, audio and presenting frames belong to
[CodeBrix.VideoPlayback](CodeBrix.VideoPlayback.md); read that package's own `AGENT-README.txt` for the
session, container and presenter model. XML documentation ships alongside the assembly.

## License

CodeBrix.VideoPlayback.Dav1d is licensed under the BSD 2-Clause License; the license is also named in
the package ID (`CodeBrix.VideoPlayback.Dav1d.BsdLicenseForever`). The package carries its own LICENSE
at its root, and every runtime-identifier folder carries a `LICENSE-Dav1d.txt`, because a file named
plainly `LICENSE` collides in a consuming application's output folder. For the provenance and licensing
of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.VideoPlayback.Dav1d/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.VideoPlayback](CodeBrix.VideoPlayback.md) - the session, the containers and the presenter this decoder plugs into
- [CodeBrix.Audio.Opus](CodeBrix.Audio.Opus.md) - the other registration call, for files whose sound is Opus
- [VideoPlayer add-in](../platform/add-ins/VideoPlayer.md) - the video player element for a CodeBrix.Platform page
- [ellisnet/CodeBrix.VideoPlayback.Dav1d on GitHub](https://github.com/ellisnet/CodeBrix.VideoPlayback.Dav1d) - source and tests
