<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Platform.MediaPlayerCore</sub>

# CodeBrix.Platform.MediaPlayerCore

**A managed, cross-platform audio and video media library family for .NET: it renders video, outputs
audio, captures from a webcam and controls playback on Windows, Linux and macOS desktops.** The heart of
the repository is a managed binding to the native `libvlc` engine, narrowed to the cross-platform
managed core, and it produces three packages - the engine, the video-view seam a player UI is built on,
and webcam capture. You reach for them from any .NET 10 desktop application, or from a CodeBrix.Platform
application, where the [MediaPlayer add-in](../platform/add-ins/MediaPlayer.md) builds its element on
the seam package.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Platform.MediaPlayerCore](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore) |
| **Packages** | [`CodeBrix.MediaCore.LgplLicenseForever`](https://www.nuget.org/packages/CodeBrix.MediaCore.LgplLicenseForever)<br>[`CodeBrix.Platform.MediaPlayerCore.LgplLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.MediaPlayerCore.LgplLicenseForever)<br>[`CodeBrix.Webcam.LgplLicenseForever`](https://www.nuget.org/packages/CodeBrix.Webcam.LgplLicenseForever) |
| **License** | LGPL-2.1-or-later; see [License](#license) |
| **Requires** | .NET 10 or later, plus a native prerequisite that differs by package and platform - see [Getting started](#getting-started) |
| **Use it from** | Any .NET 10 desktop application, or a CodeBrix.Platform application, through the [MediaPlayer add-in](../platform/add-ins/MediaPlayer.md) |
| **Platforms** | Windows, Linux and macOS desktops. There are no mobile targets, and the webcam package throws `PlatformNotSupportedException` on any other operating system |

## What it does

| Package | Assembly | Its role |
| --- | --- | --- |
| `CodeBrix.MediaCore.LgplLicenseForever` | `CodeBrix.MediaCore.dll` | The media engine: open, play, seek, decode, and hand frames or PCM back to your code |
| `CodeBrix.Platform.MediaPlayerCore.LgplLicenseForever` | `CodeBrix.Platform.MediaPlayerCore.dll` | The seam a player UI is built on: the shared `IVideoView` type identity and the `MediaPosition` helper |
| `CodeBrix.Webcam.LgplLicenseForever` | `CodeBrix.Webcam.dll` | Webcam capture: devices, live frames, photos, recording and overlays, with no engine type in sight |

All three types live in the `CodeBrix.Platform.MediaPlayerCore` namespace family; the assembly names and
the namespaces deliberately differ, which is the first thing to know before writing a `using` line.

### The media engine

- Plays every media file format, every codec and every streaming protocol libvlc supports.
- Decodes with hardware acceleration, up to 8K.
- Browses distant file systems (SMB, FTP, SFTP, NFS) and servers (UPnP, DLNA).
- Plays Audio CD, DVD and Bluray with menu navigation.
- Handles HDR, including tonemapping for SDR streams.
- Passes audio through over SPDIF and HDMI, including the HD codecs (DD+, TrueHD, DTS-HD).
- Applies video and audio filters, plays 360 video and 3D audio including Ambisonics.
- Casts and streams to distant renderers (Chromecast and UPnP renderers).
- Renders video without a windowing system: `VideoFrameSink` hands decoded BGRA frames back in memory,
  which is what makes video work on Wayland and frame-buffer hosts where libvlc has no window-embedding
  API.
- Pushes frames the other way with `VideoFrameSource`: caller-supplied BGRx frames go into the engine
  through its in-memory input and out through a stream-output chain.
- Captures decoded audio through callbacks, discovers local media sources and network renderers, applies
  an audio equalizer, and routes the engine's user dialogs and log output to managed code.

### The video-view seam

The seam package's public surface is exactly two types, and that is the point of it: the shared type
identity of `IVideoView`, so a control library and a player library agree on one interface without
referencing each other, and the `MediaPosition` helper that turns a playback position into the numbers
and the text a transport bar shows. Everything else in the package - the `MediaPlayerElement`
management layer - is internal implementation detail.

### Webcam capture

- Enumerates devices asynchronously and richly: every camera with its full format x resolution x
  frame-rate capability matrix, its adjustable controls (brightness, focus, exposure, zoom and the
  rest), its USB hardware ids, and the microphone physically paired with it.
- Runs a live capture session that delivers 32-bit BGRA frames to any UI stack - pushed through an
  event, or pulled on demand.
- Captures frame photos as tightly packed BGRA, optionally mirrored.
- Records video to MP4/H.264, with an in-file AAC track when a microphone is captured, or to
  MJPEG-passthrough AVI.
- Burns a live-updatable transparent overlay into photos and recordings, and optionally into the
  preview.
- Captures the camera's paired microphone automatically, forceable off for silent files, with
  muted-by-default live monitoring.
- Exposes no engine type anywhere in its public API - a reflection test enforces that permanently.

## When to use it

Pick the package by what you are building:

- **Playing media, or rendering video frames into your own surface** - reference
  [`CodeBrix.MediaCore.LgplLicenseForever`](https://www.nuget.org/packages/CodeBrix.MediaCore.LgplLicenseForever)
  alone.
- **Building a player UI** (state, seek bar, volume, track selection) - reference
  [`CodeBrix.Platform.MediaPlayerCore.LgplLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.MediaPlayerCore.LgplLicenseForever);
  the engine comes with it.
- **Capturing from a webcam** - reference
  [`CodeBrix.Webcam.LgplLicenseForever`](https://www.nuget.org/packages/CodeBrix.Webcam.LgplLicenseForever);
  the engine comes with it too, but you never touch an engine type.
- **Wanting a ready-made XAML media element** - use the
  [MediaPlayer add-in](../platform/add-ins/MediaPlayer.md), which references the seam package for you.

The three packages always publish together and must never be pinned to different versions of each other.

> [!WARNING]
> All three packages are LGPL-2.1-or-later. Consume them through `<PackageReference>` and never merge
> the assemblies into another one: merging forfeits the relinkability that LGPL-2.1 section 6 requires.

What the engine deliberately does not do:

- It does not bundle native libvlc on any platform.
- It contains no UI: no XAML control and no view for any UI stack. The
  [MediaPlayer add-in](../platform/add-ins/MediaPlayer.md) builds a control on top of it.
- It has no media-list player, no playlist sequencing and no playback queue: `MediaList` is a container
  only, and you drive a `MediaPlayer` from it yourself.
- Native window embedding is unavailable where libvlc has no embedding API - Wayland and frame-buffer
  hosts. Use `VideoFrameSink` there.
- It does not capture from cameras. That is the webcam package.

What the seam package deliberately does not do: it does not render video, create windows or draw
controls - `IVideoView` is a contract, and the rendering is the implementer's - it does not bundle
native libvlc, and it adds no playback API of its own, because every Play, Pause and Seek call is on the
engine's `MediaPlayer`.

What the webcam package deliberately does not do: it does not encode images (`WebcamPhoto` is raw BGRA;
PNG and JPEG encoding is an image library's job, such as [CodeBrix.Imaging](CodeBrix.Imaging.md)); it
does not mux sidecar audio into frame-path recordings (combine the WAV and the MP4 with a stream-copy
mux, such as [CodeBrix.VideoProcessing](CodeBrix.VideoProcessing.md)); it ships no camera-preview
control for any UI stack; it does not support MJPEG-passthrough AVI on Windows; it exposes only mode
selectors as camera controls on macOS; and it does not play media files or stream from the network -
that is the engine package.

## Getting started

```bash
dotnet add package CodeBrix.MediaCore.LgplLicenseForever
dotnet add package CodeBrix.Platform.MediaPlayerCore.LgplLicenseForever
dotnet add package CodeBrix.Webcam.LgplLicenseForever
```

The native prerequisite differs by package family and by platform, and the answers are genuinely
different for each - this is the single most common source of a first-run failure.

| | Playback packages (engine and seam) | Webcam package |
| --- | --- | --- |
| **Windows** | The consuming application references [`VideoLAN.LibVLC.Windows`](https://www.nuget.org/packages/VideoLAN.LibVLC.Windows), which drops the native files next to the executable. An installed VLC desktop application is not used and not searched | Nothing to install and no VideoLAN package: capture and recording use the operating system's built-in Media Foundation engine. Windows 'N' editions need the Media Feature Pack, and the failure message says so |
| **Linux** | `sudo apt install libvlc5 vlc-plugin-base` on Debian-based distributions. Neither the VLC desktop application nor the development package is required | The same runtime libraries |
| **macOS** | The VLC media player application must be installed in `/Applications`; the loader finds it and points libvlc at the bundle's plugin directory. An application may instead ship the libraries in its own output, and app-bundled libraries win | The VLC media player application must be installed |

> [!IMPORTANT]
> On Windows the reference to the native package must be in the **application** project, not in a class
> library: the native files have to land next to the executable.

Device enumeration in the webcam package needs none of the above on any platform, needs no camera
permission, and works headless.

The namespaces to import:

```csharp
using CodeBrix.Platform.MediaPlayerCore;             // almost everything
using CodeBrix.Platform.MediaPlayerCore.Structures;  // five DTO structs
```

```csharp
using CodeBrix.Platform.MediaPlayerCore;                    // IVideoView
using CodeBrix.Platform.MediaPlayerCore.MediaPlayerElement; // MediaPosition
```

```csharp
using CodeBrix.Webcam;           // WebcamDevices, WebcamSession,
                                 // WebcamSessionOptions, AudioCaptureMode,
                                 // WebcamException
using CodeBrix.Webcam.Devices;   // IImagingMediaDevice and its parts
using CodeBrix.Webcam.Capture;   // frames, photos, overlays, recording
```

The first working example plays a file to the end from a console application, and shows the completion
pattern the engine requires:

```csharp
using System;
using System.Threading.Tasks;
using CodeBrix.Platform.MediaPlayerCore;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        Core.Initialize();                       // throws VLCException if
                                                 // libvlc is not installed
        using var libVLC = new LibVLC();
        using var media = new Media(libVLC, args[0]);   // FromType.FromPath
        using var player = new MediaPlayer(media);

        // RunContinuationsAsynchronously is ESSENTIAL: the TCS is completed
        // from a libvlc event thread, and an inline continuation would run
        // the rest of Main (including Stop()) on that thread -> deadlock.
        var finished = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        long lengthMs = 0;

        player.LengthChanged += (_, e) => lengthMs = e.Length;
        player.TimeChanged += (_, e) =>
            Console.Write($"\r{e.Time / 1000}s / {lengthMs / 1000}s   ");
        player.EndReached += (_, _) => finished.TrySetResult(true);
        player.EncounteredError += (_, _) => finished.TrySetResult(false);

        if (!player.Play())
        {
            Console.Error.WriteLine("libvlc refused to start playback.");
            return 1;
        }
        bool ok = await finished.Task;
        player.Stop();                           // safe here: we are back on
                                                 // a thread-pool thread
        Console.WriteLine(ok ? "\nDone." : "\nPlayback failed.");
        return ok ? 0 : 1;
    }
}
```

Notice the two rules that make the difference between working code and a hang: `Core.Initialize()` runs
once before any `LibVLC` is constructed, and the completion source is created with
`RunContinuationsAsynchronously` because it is completed from an engine thread.

## Key concepts

### The namespace and package names deliberately differ

The engine assembly and package are named `CodeBrix.MediaCore`, but every type in them lives in the
`CodeBrix.Platform.MediaPlayerCore` namespace and its two sub-namespaces. The mismatch is deliberate and
permanent. Never write `using CodeBrix.MediaCore;` - that namespace does not exist, and the line does not
compile.

`.Structures` holds only `AudioOutputDescription`, `AudioOutputDevice`, `ChapterDescription`,
`ModuleDescription` and `TrackDescription`; everything else is in the root namespace. A `.Helpers`
namespace exists but holds nothing public - never import it. There is no `.Core` and no `.Events`
namespace. The seam package uses the same root namespace, so the two packages share it.

### `Core` and `LibVLC` lifetime

`public static void Initialize(string? libvlcDirectoryPath = null)` loads the native libraries. The
optional path overrides the search location on Windows and macOS, and is not supported on Linux, where
`LD_LIBRARY_PATH` does that job. When the native library is missing, `Initialize` throws `VLCException`
listing the paths it tried.

`LibVLC` is the root handle. Create one per application, because creating many is expensive; its
constructors take libvlc command-line switches, though per-`Media` options (`":option"`) are preferred.
It carries `Version`, `Changeset`, `LibVLCCompiler`, `Clock`, `LastLibVLCError`, `ClearLibVLCError()`,
`AddInterface`, `SetExitHandler`, `SetUserAgent`, `SetAppId`, `AudioFilters`, `VideoFilters`,
`AudioOutputs`, `AudioOutputDevices(string)`, `MediaDiscoverers(MediaDiscovererCategory)`,
`RendererList`, `SetDialogHandlers`, `UnsetDialogHandlers` and `DialogHandlersSet`.

Dispose in order: stop the player, dispose the sink, dispose the `Media`, dispose the `MediaPlayer`,
dispose `LibVLC` last. Disposing `LibVLC` while players still exist crashes inside native code.

### `Media`, parsing, tracks and slaves

`Media` is one playable resource plus its metadata and tracks. Constructors take an MRL with `FromType`,
a `Uri`, an open file descriptor, a `MediaInput` or a `MediaList`.

```csharp
public async Task<MediaParsedStatus> Parse(MediaParseOptions options = MediaParseOptions.ParseLocal,
    int timeout = -1, CancellationToken cancellationToken = default)
```

`Tracks`, `Duration` and `SubItems` are empty until the media has been parsed or played, and
`ParseLocal` deliberately skips network URLs - use `ParseNetwork` for those. A `timeout` of -1 uses the
default and 0 waits forever; check the returned `MediaParsedStatus` rather than assuming `Done`, because
it may be `Timeout` or `Skipped`.

External subtitle and audio files attach through `AddSlave(MediaSlaveType, uint priority, string uri)`
and its `Uri` overload, with `ClearSlaves()` and `Slaves`. Media events - `MetaChanged`,
`ParsedChanged`, `SubItemAdded`, `DurationChanged`, `MediaFreed`, `StateChanged` and `SubItemTreeAdded` -
are all raised on libvlc threads.

### `MediaPlayer`

One `MediaPlayer` plays one `Media` at a time; reuse it by assigning `Media`. Its surface covers:

- **Transport**: `Play()`, `Play(Media)`, `Pause()` (which toggles), `SetPause(bool)`, `Stop()`,
  `IsPlaying`, `State`, `WillPlay`, `CanPause`, `IsSeekable`, `Length`, `Time`, `Position`,
  `SeekTo(TimeSpan)`, `Rate`, `SetRate(float)`, `Fps`, `NextFrame()`.
- **Native window embedding**: `Hwnd`, `XWindow`, `NsObject`, `Fullscreen`, `ToggleFullscreen()`,
  `EnableKeyInput`, `EnableMouseInput`, `VoutCount`, `Size(...)`, `Cursor(...)` - none of it available
  on Wayland or frame-buffer hosts.
- **Geometry**: `Scale`, `AspectRatio`, `CropGeometry`, `ApplyUniformScale(...)`, `SetDeinterlace(...)`,
  `TakeSnapshot(...)`, and `Viewpoint` / `UpdateViewpoint(...)` for 360 video.
- **Audio**: `Volume` (0..100), `Mute`, `AudioDelay` in microseconds, track selection, `Channel`,
  `SetAudioOutput`, `SetOutputDevice`, `AudioOutputDeviceEnum`, `SetEqualizer`, `UnsetEqualizer`.
- **Video filters**: the `Marquee*`, `Logo*` and `Adjust*` families.
- **Per-media shortcuts** that apply to the current `Media`: `EnableHardwareDecoding`, `FileCaching`
  and `NetworkCaching`, both in milliseconds.

Event-args members are public readonly **fields**, not properties: `e.Time`, `e.Cache`, `e.Filename`.

> [!WARNING]
> Calling `MediaPlayer` members from inside a `MediaPlayer`, `Media`, `VideoFrameSink` or audio-callback
> handler deadlocks or crashes. In particular, never call `Stop()` or `Dispose()` from `EndReached`.

### `VideoFrameSink` - frames out

`VideoFrameSink` renders a player's decoded video into CPU memory and raises `FrameReady` per displayed
frame as 32-bit BGRA. It works on every desktop platform and windowing system - including Wayland and
bare frame-buffer hosts, where libvlc has no window-embedding API - and needs only libvlc's base plugin
set. That makes it the path a CodeBrix.Platform video host takes.

Construct it with `VideoFrameSink(MediaPlayer)` for a three-buffer ring, or
`VideoFrameSink(MediaPlayer, int bufferCount)` for one to eight. It exposes `FrameReady` and
`FormatChanged`, the properties `MediaPlayer`, `BufferCount`, `Width`, `Height` and `PitchBytes`, and
`Dispose()`.

Three rules govern it. Construct it **before** `Play()` - attaching permanently switches the player to
memory rendering for its lifetime. It cannot be detached. And dispose it only after the player is
stopped or disposed.

`VideoFrameReadyEventArgs` is sealed and **the instance is reused for every frame**: `Plane` points at
the first pixel, BGRA top-down, and is valid only until the handler returns. Never store the args or the
pointer. `VideoFrameFormatChangedEventArgs` adds `Lines`, the scanlines allocated per buffer, which is a
multiple of 32 - the buffer size is `PitchBytes * Lines`. Rows are `PitchBytes` apart in the source, not
`Width * 4`, so copy with the pitch.

### `VideoFrameSource` - frames in

`VideoFrameSource` is the push-model mirror: construct, `Start`, `PushFrame...`, `Complete`,
`WaitForCompletion`, `Dispose`, once. `IsSupported(LibVLC)` probes the engine's in-memory input and
caches the answer; `EnsureSupported(LibVLC)` throws `VLCException` instead. `PushFrame` applies
back-pressure and blocks when the encoder is behind, so produce frames from a dedicated thread.

> [!IMPORTANT]
> The encoder setting matters. With x264 defaults the transcode chain buffers around forty frames of
> lookahead and does not drain them at end of stream, so a short clip yields an empty file. Always pass
> a live-tuned chain.

### The `IVideoView` seam

What the seam package gives a consumer is a shared type identity, so that a control library and a player
library agree on one interface without referencing each other:

```csharp
namespace CodeBrix.Platform.MediaPlayerCore;

public interface IVideoView
{
    MediaPlayer? MediaPlayer { get; set; }
}
```

The interface defines only the property; what the setter *does* is entirely the implementer's
responsibility - detaching the old player, attaching a sink to the new one, and raising whatever the
host wants raised. The engine ships `MediaPlayerChangedEventArgs` and `MediaPlayerChangingEventArgs` for
that purpose, and nothing in either package raises them.

### `MediaPosition`

```csharp
namespace CodeBrix.Platform.MediaPlayerCore.MediaPlayerElement;

public class MediaPosition
{
    public MediaPosition(float position, double seekBarPosition, long length);
    public float    Position          { get; }   // 0.0 .. 1.0, as given
    public double   SeekBarPosition   { get; }   // as given (your slider scale)
    public TimeSpan ElapsedTime       { get; }   // position * length ms
    public TimeSpan RemainingTime     { get; }   // (length - elapsed) ms
    public string   ElapsedTimeText   { get; }   // formatted, see below
    public string   RemainingTimeText { get; }
}
```

`length` is in **milliseconds**. The text properties format as `"mm:ss"` under one hour, `"hh:mm:ss"`
when `Hours` is non-zero, and `"d.hh:mm:ss"` when `Days` is non-zero. The object is immutable; construct
a new one per position update. With a length of 0 both times are zero, and with a position above 1
`RemainingTime` is negative - clamp your inputs.

### Discovery, renderers, equalizer and dialogs

`MediaDiscoverer(LibVLC, string name)` has `Start`, `Stop`, `IsRunning`, `LocalizedName`, `MediaList`
and the `Started` / `Stopped` events, over the categories `Devices = 0`, `Lan = 1`, `Podcasts = 2` and
`Localdirs = 3`. `RendererDiscoverer(LibVLC, string? name = null)` produces `RendererItem`s carrying
`Name`, `Type` (for example "chromecast"), `IconUri`, `CanRenderVideo` and `CanRenderAudio`; hand one to
`MediaPlayer.SetRenderer(item)` **before** `Play()` to cast, and keep both the item and its discoverer
alive for as long as casting continues, because the item is a native handle.

`Equalizer()` builds a flat equalizer and `Equalizer(uint index)` one from a preset, over `SetPreamp`,
`Preamp`, `SetAmp`, `Amp`, `PresetCount`, `PresetName`, `BandCount` and `BandFrequency`. Changes to an
equalizer that is already set require calling `SetEqualizer` again.

The engine also asks the application questions - credentials, certificate acceptance, progress. Opt in
with `LibVLC.SetDialogHandlers`; each handler returns a `Task` and answers through the `Dialog` instance
it receives, which offers `PostLogin`, `PostAction(int actionIndex)` (1 is the first action, 2 the
second) and `Dismiss()`.

### The log surface and audio callbacks

`LibVLC` raises `Log` with `LogEventArgs` carrying `Level`, `Message`, `Module`, `SourceFile`,
`SourceLine` and `FormattedLog`. Subscribing **replaces** the default logger, so nothing is printed to
the console any more; unsubscribing the last handler restores it. `SetLogFile(path)` and
`CloseLogFile()` route the native log to a file, and `new LibVLC(enableDebugLogs: true)` raises native
verbosity so debug entries reach the event. If all you want is more console output, use that constructor
or `"--verbose=2"` rather than subscribing.

To receive decoded PCM instead of playing it, fix the format and set the callbacks before `Play()`.
Setting audio callbacks disables all engine audio output - you own playback from then on.

### Webcam devices and sessions

`WebcamDevices.GetImagingMediaDeviceListAsync()` runs off the calling thread, returns an empty list when
no camera is present, and throws `PlatformNotSupportedException` on operating systems other than
Windows, Linux and macOS. Each `IImagingMediaDevice` exposes `Id` (a stable operating-system identity
worth persisting - a device node on Linux, the DirectShow device path on Windows, the capture device's
unique id on macOS), `FriendlyName`, `Hardware`, `Capabilities`, `Controls` and `PairedMicrophone`.

`ImagingMediaCapability` carries `PixelFormat`, `FourCc` ("MJPG", "YUYV"), `Width`, `Height`,
`FrameRates` (highest first, or two range endpoints) and `IsFrameRateRange`, and renders as
"MJPG 1920x1080 @ 30, 24, 15 fps". `ImagingPixelFormat` is
`Unknown = 0, Mjpeg, Yuyv, Nv12, H264, Rgb24, Rgb32, Grey`; formats not named still enumerate, with
`PixelFormat = Unknown` and the real `FourCc`. `IImagingDeviceControl` exposes `Kind`, `Name`, `RawId`,
`ControlType`, `Minimum`, `Maximum`, `Step`, `DefaultValue`, `SupportsAuto`, `GetValue()`,
`SetValue(int)`, `GetAuto()` and `SetAuto(bool)`.

A `WebcamSession` is one live session on one camera, bound to its device - create a new session to
switch cameras. Its properties are `Device`, `IsRunning`, `IsRecording`, `IsAudioCaptureActive`,
`IsOverlayRecordingSupported`, `FrameWidth`, `FrameHeight`, `MonitorAudio` (false by default) and
`MonitorVolume`; its methods are `Start()`, `Stop()`, `TryCopyLatestFrame(...)`, `CapturePhoto(...)` and
its `mirrorHorizontally` overload, `SetOverlay(WebcamOverlay)`, `ClearOverlay()`,
`StartRecording(WebcamRecordingOptions)`, `StopRecording()` and `Dispose()`. Control methods are safe
from any one thread at a time, but must not be called from inside a `FrameReceived` handler.

### Push or pull: two ways to consume frames

**Push** - subscribe `FrameReceived`, raised on an internal capture thread. `Width`, `Height`,
`PitchBytes`, `PixelPlane` and `CopyTo(byte[])` are valid only until the handler returns. This is the
right choice for a pipeline that wants every frame.

**Pull** - call `TryCopyLatestFrame` from wherever repainting happens. It copies the most recent frame,
tightly packed BGRA, into a reusable buffer, reallocating only on size changes, and is safe from any
thread. The internal cache is off until the first call, so a session that never pulls pays no per-frame
copy - which is why the first call returns false. Treat that as "nothing yet", not as an error.

Users expect a mirror: mirror the preview at render time and mirror stills with
`CapturePhoto(mirrorHorizontally: true)`. Computer-vision results computed on unmirrored frames must be
mirrored too (`x' = 1 - x`).

### Recording: the direct pipeline and the frame path

`WebcamVideoFormat.Mp4H264 = 0` with no overlay involvement takes the **direct pipeline**: the backend
records the camera stream itself, with in-file audio when a microphone is captured, and overlays cannot
be introduced until it stops. With an overlay set - or with `options.AllowLiveOverlay = true` - it takes
the **frame path**: frames flow through managed compositing into the encoder without interrupting the
preview, and captured audio arrives as a sidecar WAV named by `WebcamRecordingResult.AudioFilePath`.

`MjpegAvi` muxes the camera's native MJPEG into AVI with no transcoding and near-zero CPU. It needs an
MJPEG-streaming camera, allows no overlay and no audio track, and is Linux and macOS only.

`WebcamRecordingResult` returns `VideoFilePath`, `AudioFilePath` (the sidecar WAV, or null),
`EstimatedAudioOffset` (audio start minus first video frame; positive means audio first), `Duration` as
wall clock, and `FramesRecorded`, which counts frame-path frames and is 0 for the direct pipeline.

### Overlays

`WebcamOverlay(byte[] pixelsBgra32, int width, int height, int strideBytes = 0)` takes **straight**
(non-premultiplied) alpha BGRA sized exactly to the video. The constructor copies the pixels, so the
source buffer can be reused, and an instance is immutable and shareable. It throws
`ArgumentNullException` or `ArgumentOutOfRangeException` for null pixels, zero dimensions, a stride below
`Width * 4`, or a buffer too small for the dimensions and stride.

When producing the buffer from SkiaSharp, render to `Bgra8888` and read the pixels with
`SKAlphaType.Unpremul` - Skia's default is premultiplied and blends wrongly here.

### macOS camera consent

macOS gates camera and microphone capture behind per-application user consent. The underlying capture
modules only check the authorization status, so `WebcamSession.Start()` requests consent itself, and a
denial surfaces as a `WebcamException` pointing at System Settings > Privacy & Security > Camera (or
Microphone).

A bundled `.app` must declare `NSCameraUsageDescription` - and `NSMicrophoneUsageDescription` when audio
is captured - in its `Info.plist`, or macOS refuses access outright. A bare `dotnet run` process
attaches consent to the hosting terminal application and needs no usage-description string. Enumeration
does not require consent; only opening a session does. A non-interactive context cannot show the prompt:
the request is denied instantly and the status stays "not determined", so a human must click Allow once
from an interactive session.

## Examples

Pulling video frames into memory works on every windowing system, and is the shape a Skia-drawing host
copies. The handler copies and leaves; the drawing happens on the UI thread:

```csharp
using System;
using System.Runtime.InteropServices;
using CodeBrix.Platform.MediaPlayerCore;
using SkiaSharp;                         // any BGRA-capable bitmap works

public sealed class FramePump : IDisposable
{
    private readonly LibVLC _libVLC = new LibVLC("--no-audio");
    private readonly MediaPlayer _player;
    private readonly VideoFrameSink _sink;
    private readonly object _sync = new object();
    private byte[] _packed;              // tightly packed BGRA copy
    private int _width, _height;
    private bool _dirty;

    public FramePump()
    {
        Core.Initialize();
        _player = new MediaPlayer(_libVLC);
        _sink = new VideoFrameSink(_player);       // BEFORE Play()
        _sink.FormatChanged += (_, e) =>
        {
            lock (_sync)
            {
                _width = (int)e.Width;
                _height = (int)e.Height;
                _packed = new byte[_width * _height * 4];
            }
        };
        _sink.FrameReady += (_, e) =>
        {
            // libvlc thread: copy and leave. Rows are PitchBytes apart in
            // the source (32-byte aligned) but Width*4 apart in _packed.
            lock (_sync)
            {
                if (_packed == null) { return; }
                int row = (int)e.Width * 4;
                for (int y = 0; y < e.Height; y++)
                {
                    Marshal.Copy(e.Plane + (int)(y * e.PitchBytes),
                        _packed, y * row, row);
                }
                _dirty = true;
            }
            // signal the UI to repaint here (e.g. Dispatcher.Post(...));
            // do NOT draw from this thread.
        };
    }

    public void Open(string path)
    {
        _player.Media = new Media(_libVLC, path);
        _player.Play();
    }

    // Call from the UI thread's paint handler.
    public void Paint(SKCanvas canvas)
    {
        lock (_sync)
        {
            if (!_dirty || _packed == null) { return; }
            using var bitmap = new SKBitmap(new SKImageInfo(_width, _height,
                SKColorType.Bgra8888, SKAlphaType.Opaque));
            Marshal.Copy(_packed, 0, bitmap.GetPixels(), _packed.Length);
            canvas.DrawBitmap(bitmap, 0, 0);
            _dirty = false;
        }
    }

    public void Dispose()
    {
        _player.Stop();                  // stop BEFORE disposing the sink
        _sink.Dispose();
        _player.Media?.Dispose();
        _player.Dispose();
        _libVLC.Dispose();
    }
}
```

Inspecting a file before playing it means parsing it first, and then walking the tracks:

```csharp
using var libVLC = new LibVLC();
using var media = new Media(libVLC, path);
MediaParsedStatus status = await media.Parse(MediaParseOptions.ParseLocal);
if (status == MediaParsedStatus.Done)
{
    Console.WriteLine($"{media.Meta(MetadataType.Artist)} - " +
                      $"{media.Meta(MetadataType.Title)}  {media.Duration} ms");
    foreach (MediaTrack track in media.Tracks)
    {
        switch (track.TrackType)
        {
            case TrackType.Video:
                VideoTrack v = track.Data.Video;
                Console.WriteLine($"video {v.Width}x{v.Height} " +
                    $"{v.FrameRateNum}/{v.FrameRateDen} fps {v.Orientation}");
                break;
            case TrackType.Audio:
                AudioTrack a = track.Data.Audio;
                Console.WriteLine($"audio {a.Channels}ch {a.Rate} Hz " +
                    $"{track.Language}");
                break;
            case TrackType.Text:
                Console.WriteLine($"subtitle {track.Language} " +
                    $"{track.Data.Subtitle.Encoding}");
                break;
        }
    }
}
```

Driving a seek bar from the seam package means computing an immutable snapshot on the engine thread and
posting it to the UI:

```csharp
using System;
using CodeBrix.Platform.MediaPlayerCore;
using CodeBrix.Platform.MediaPlayerCore.MediaPlayerElement;

public sealed class TransportViewModel
{
    private long _lengthMs;
    public MediaPosition Current { get; private set; } =
        new MediaPosition(0f, 0d, 0L);
    public event EventHandler PositionUpdated;        // bind to UI

    public void Bind(MediaPlayer player)
    {
        player.LengthChanged += (_, e) => _lengthMs = e.Length;
        player.PositionChanged += (_, e) =>
        {
            // libvlc thread: compute the immutable snapshot, post to UI
            float p = Math.Clamp(e.Position, 0f, 1f);
            Current = new MediaPosition(p, p * 100d, _lengthMs);  // 0..100 slider
            PositionUpdated?.Invoke(this, EventArgs.Empty);
        };
    }

    // UI: slider.Value = Current.SeekBarPosition;
    //     elapsed.Text = Current.ElapsedTimeText;      e.g. "03:07"
    //     remaining.Text = "-" + Current.RemainingTimeText;
    // Seeking back: player.Position = (float)(slider.Value / 100d);
}
```

Opening a camera starts with enumerating what the machine has, and then asking for exactly one of its
advertised modes:

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Webcam;
using CodeBrix.Webcam.Devices;

var devices = await WebcamDevices.GetImagingMediaDeviceListAsync();
if (devices.Count == 0) { throw new InvalidOperationException("no camera"); }

foreach (IImagingMediaDevice cam in devices)
{
    Console.WriteLine($"{cam.FriendlyName}  [{cam.Id}]  " +
        $"vid={cam.Hardware.VendorId:X4} pid={cam.Hardware.ProductId:X4} " +
        $"mic={cam.PairedMicrophone?.FriendlyName ?? "none"}");
    foreach (ImagingMediaCapability cap in cam.Capabilities)
    {
        Console.WriteLine($"   {cap}");        // "MJPG 1920x1080 @ 30 fps"
    }
    foreach (IImagingDeviceControl c in cam.Controls)
    {
        Console.WriteLine($"   {c.Kind} '{c.Name}' {c.Minimum}..{c.Maximum} " +
            $"step {c.Step} default {c.DefaultValue} auto={c.SupportsAuto}");
    }
}

// Pick the highest-resolution MJPEG mode and open exactly that.
IImagingMediaDevice device = devices[0];
ImagingMediaCapability best = device.Capabilities
    .Where(c => c.PixelFormat == ImagingPixelFormat.Mjpeg)
    .OrderByDescending(c => c.Width * c.Height)
    .First();
var options = new WebcamSessionOptions
{
    RequestedWidth = best.Width,
    RequestedHeight = best.Height,
    RequestedFrameRate = best.FrameRates[0],          // highest first
    PreferredFormat = best.PixelFormat,
    AudioCapture = AudioCaptureMode.Auto,
};
using var session = new WebcamSession(device, options);
session.Start();                                       // may throw WebcamException
Console.WriteLine($"streaming {session.FrameWidth}x{session.FrameHeight}");
```

Painting the preview is a pull from wherever the repaint happens - aspect-fit, centered, optionally
mirrored:

```csharp
using System;
using CodeBrix.Webcam;
using SkiaSharp;

public sealed class WebcamFrameRenderer
{
    private byte[] _frameBuffer;
    private SKBitmap _bitmap;

    public void Render(SKSurface surface, SKImageInfo info, WebcamSession session, bool mirror)
    {
        SKCanvas canvas = surface.Canvas;
        canvas.Clear(SKColors.Black);
        if (session == null
            || !session.TryCopyLatestFrame(ref _frameBuffer, out int width, out int height)
            || width <= 0 || height <= 0) { return; }

        if (_bitmap == null || _bitmap.Width != width || _bitmap.Height != height)
        {
            _bitmap?.Dispose();
            _bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque));
        }
        System.Runtime.InteropServices.Marshal.Copy(_frameBuffer, 0, _bitmap.GetPixels(), width * height * 4);

        float scale = Math.Min((float)info.Width / width, (float)info.Height / height);
        float destWidth = width * scale;
        float destHeight = height * scale;
        float destX = (info.Width - destWidth) / 2f;
        float destY = (info.Height - destHeight) / 2f;

        int restoreTo = canvas.Save();
        if (mirror) { canvas.Scale(-1, 1, destX + (destWidth / 2f), 0); }
        canvas.DrawBitmap(_bitmap, new SKRect(destX, destY, destX + destWidth, destY + destHeight),
            new SKSamplingOptions(SKFilterMode.Linear));
        canvas.RestoreToCount(restoreTo);
    }
}
```

## Using it in a CodeBrix.Platform application

The seam package is the shared type identity a CodeBrix.Platform control library and a player library
agree on: if you are writing a control that hosts video, or a player layer that talks to such controls
through an interface, reference it. If you want a ready-made XAML media element, use the
[MediaPlayer add-in](../platform/add-ins/MediaPlayer.md)
([`CodeBrix.Platform.MediaPlayer.LgplLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.MediaPlayer.LgplLicenseForever)),
which is the production consumer of this package and references it for you.

The head-specific note that matters most: native window embedding is unavailable on Wayland and
frame-buffer hosts, so a CodeBrix.Platform video host uses `VideoFrameSink` and paints the BGRA frames
itself. That is what the implementation below does, and it is the pattern the add-in follows:

```csharp
using System;
using System.Runtime.InteropServices;
using CodeBrix.Platform.MediaPlayerCore;
using SkiaSharp;

public sealed class SkiaVideoHost : IVideoView, IDisposable
{
    private readonly object _sync = new object();
    private MediaPlayer _player;
    private VideoFrameSink _sink;
    private byte[] _packed;
    private int _width, _height;

    public event EventHandler<MediaPlayerChangedEventArgs> MediaPlayerChanged;
    public event EventHandler FrameArrived;      // UI: call Invalidate()

    public MediaPlayer MediaPlayer
    {
        get => _player;
        set
        {
            if (ReferenceEquals(_player, value)) { return; }
            MediaPlayer old = _player;
            Detach();
            _player = value;
            if (value != null)
            {
                // must happen BEFORE the player's first Play()
                _sink = new VideoFrameSink(value);
                _sink.FormatChanged += OnFormatChanged;
                _sink.FrameReady += OnFrameReady;
            }
            MediaPlayerChanged?.Invoke(this,
                new MediaPlayerChangedEventArgs(old, value));
        }
    }

    private void OnFormatChanged(object sender, VideoFrameFormatChangedEventArgs e)
    {
        lock (_sync)
        {
            _width = (int)e.Width;
            _height = (int)e.Height;
            _packed = new byte[_width * _height * 4];
        }
    }

    private void OnFrameReady(object sender, VideoFrameReadyEventArgs e)
    {
        lock (_sync)                       // libvlc thread: copy and leave
        {
            if (_packed == null) { return; }
            int row = (int)e.Width * 4;
            for (int y = 0; y < e.Height; y++)
            {
                Marshal.Copy(e.Plane + (int)(y * e.PitchBytes), _packed, y * row, row);
            }
        }
        FrameArrived?.Invoke(this, EventArgs.Empty);   // marshal to UI there
    }

    public void Paint(SKCanvas canvas, int viewWidth, int viewHeight)
    {
        lock (_sync)
        {
            canvas.Clear(SKColors.Black);
            if (_packed == null) { return; }
            using var bitmap = new SKBitmap(new SKImageInfo(_width, _height,
                SKColorType.Bgra8888, SKAlphaType.Opaque));
            Marshal.Copy(_packed, 0, bitmap.GetPixels(), _packed.Length);
            float scale = Math.Min((float)viewWidth / _width, (float)viewHeight / _height);
            float w = _width * scale, h = _height * scale;
            canvas.DrawBitmap(bitmap, new SKRect((viewWidth - w) / 2, (viewHeight - h) / 2,
                (viewWidth + w) / 2, (viewHeight + h) / 2));
        }
    }

    private void Detach()
    {
        if (_sink == null) { return; }
        _sink.FrameReady -= OnFrameReady;
        _sink.FormatChanged -= OnFormatChanged;
        _player?.Stop();                   // stop before disposing the sink
        _sink.Dispose();
        _sink = null;
    }

    public void Dispose() => Detach();
}
```

Notice the ordering the setter enforces: the old player's rendering stops before the field changes, and
the sink is attached before the new player is ever played. Assign the player to the view first, then
call `Play()`.

The repository's own `WebcamViewer` sample is built as a CodeBrix.Platform application for the Linux
X11, Linux Wayland, Linux frame-buffer, macOS, Win32-Skia and WPF-Skia heads, plus native WinUI and WPF
heads, sharing one view model and one Skia video canvas across all of them. Its CodeBrix.Platform core
project references
[`CodeBrix.Platform.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.ApacheLicenseForever),
[`CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever),
[`CodeBrix.Platform.Fonts.OpenSans.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.OpenSans.ApacheLicenseForever)
and [CodeBrix.Imaging](CodeBrix.Imaging.md) for PNG encoding.

## Pitfalls

### The engine

- **Missing native libvlc.** `Core.Initialize()` or `new LibVLC()` throws `VLCException` listing the
  paths it searched. On Windows the native package must be referenced from the application project,
  because the native files must land next to the executable.
- **Never call back into the player from a handler.** `MediaPlayer`, `Media`, `VideoFrameSink` and
  audio-callback handlers all run on engine threads; calling `Stop()` or `Dispose()` from `EndReached`
  deadlocks or crashes.
- **Awaiting a `TaskCompletionSource` that an engine event completes** needs
  `TaskCreationOptions.RunContinuationsAsynchronously`, or the continuation runs on the engine thread.
- **`VideoFrameSink` must be constructed before the first `Play()`** on that player, and it cannot be
  detached. Dispose it only after the player is stopped or disposed.
- **The `FrameReady` event-args instance is reused for every frame** and the `Plane` pointer dies when
  the handler returns. Never store the args or the pointer.
- **Copy with the pitch.** Frames are `PitchBytes` wide, 32-byte aligned, not `Width * 4`.
- **A slow `FrameReady` handler makes the engine overwrite a buffer that is still being read.** Raise
  `bufferCount` if handlers are occasionally slow, and never decode, encode or draw there.
- **Memory rendering costs.** `VideoFrameSink` forces CPU pixel-format conversion and disables or slows
  hardware decoding; prefer window embedding where the windowing system allows it.
- **`TimeChanged` fires many times per second.** Coalesce UI updates into one repaint per frame.
- **`using CodeBrix.MediaCore;` does not compile.** The namespace is
  `CodeBrix.Platform.MediaPlayerCore`.
- **`Tracks` and `Duration` are empty until parsed or played,** and `ParseLocal` deliberately skips
  network URLs.
- **Subscribing to `LibVLC.Log` silences the native console output.**
- **Audio callbacks silence the engine.** Once `SetAudioCallbacks` is set, no audio is output through
  the system; you own playback. Keep the captured delegate in a field or local that outlives playback -
  do not let the only reference be a temporary.
- **`MediaList` is not a player.** Drive a `MediaPlayer` yourself from the list.
- **Renderer items are native handles.** Keep the `RendererItem` passed to `SetRenderer` alive, and its
  discoverer running, for as long as casting continues.
- **Version skew is by design.** The package's major version tracks the major version of the native
  engine it binds, so installing a different native major makes every `LibVLC` construction throw
  `VLCException`.

### The seam

- **The management layer is internal.** `MediaPlayerElementManager`, `StateManager`, `SeekBarManager`,
  `VolumeManager`, `AspectRatioManager`, `AudioTracksManager`, `VideoTracksManager`,
  `SubtitlesTracksManager`, `AutoHideNotifier`, `BufferingProgressNotifier`, `CastRenderersDiscoverer`,
  `DeviceAwakeningManager`, `TracksManager`, `IDispatcher`, `IDisplayRequest`, `IDisplayInformation`,
  `IVideoControl`, `AspectRatio` and `FontAwesomeIcons` are all `internal`. Code that names them will
  not compile against the package.
- **Assign the player to the view before calling `Play()`** when the host uses `VideoFrameSink`.
- **Swapping players without detaching** leaves frames from the old player arriving: the setter must
  stop the old player's rendering before the field changes.
- **`MediaPlayer?` in the interface is an annotation from the source, not a different type.** With
  nullable reference types off it is implemented as `public MediaPlayer MediaPlayer { get; set; }`.
- **`MediaPosition` takes milliseconds for `length`.** Passing seconds makes `ElapsedTime` a thousand
  times too small.
- **This package and the engine package must be at the same version.** Updating one without the other
  produces a `MissingMethodException` or a `TypeLoadException` at run time.

### The webcam

- **Do no work in `FrameReceived`.** It runs on the capture thread and `PixelPlane` dies when the
  handler returns. Copy and leave; never touch UI objects or call session methods from inside it, and
  adjust camera controls from your own thread.
- **The first `TryCopyLatestFrame` returns false by design** - the cache switches on with that call.
- **Mirror the preview.** Users expect it; mirror at render time, use
  `CapturePhoto(mirrorHorizontally: true)`, and mirror vision coordinates too.
- **Overlay alpha must be straight, not premultiplied.** Skia's default is premultiplied, so read pixels
  with `SKAlphaType.Unpremul` or the edges blend wrong.
- **The overlay size must equal `FrameWidth` x `FrameHeight` exactly** once frames flow, or `SetOverlay`
  throws `WebcamException`. Both are 0 before frames flow.
- **`SetOverlay` during a direct recording throws.** Start the recording with the overlay already set,
  or with `AllowLiveOverlay = true`.
- **`MjpegAvi` on Windows throws.** Use `Mp4H264`.
- **"I can list cameras but cannot open one" usually means the native engine is missing** on Linux or
  macOS: enumeration still works, and `Start()` throws `WebcamException` with the per-platform fix in
  the message.
- **macOS consent.** A bundled application without `NSCameraUsageDescription` is refused outright, and a
  non-interactive shell is denied instantly with no prompt. A human must click Allow once from an
  interactive session.
- **Windows camera controls are best-effort while another application holds the camera.** Adjust between
  sessions if a driver ignores live changes.
- **Audio monitoring feeds back.** `MonitorAudio = true` with the camera and speakers in the same room
  is audible feedback; leave it false by default, and note that recordings capture the microphone
  regardless.
- **Sidecar audio needs muxing.** Frame-path recordings put audio in a WAV next to the video; combine
  them yourself using `EstimatedAudioOffset`. Only the direct MP4 pipeline has in-file audio.
- **A session is bound to one device.** Dispose it and create a new one to switch cameras.
- **Request MJPEG for high resolutions at full frame rate over USB.** Uncompressed YUYV saturates USB 2
  at 720p and above.
- **A USB 3 camera attached at USB 2.0 speed may present no USB audio interface** on macOS, so
  `PairedMicrophone` is legitimately null. Reconnect to a USB 3 port.

## Samples and tools in the repository

| Name | What it demonstrates | Where |
| --- | --- | --- |
| WebcamViewer | A live webcam viewer across every UI stack the family targets, with one shared view model and one shared Skia video canvas: a camera dropdown from the async device list, live preview with aspect-fit and black letterbox, an audio-monitor checkbox enabled only when the camera has a paired microphone and muted by default, and frame photos saved as PNG | [`samples/WebcamViewer`](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore/tree/main/samples/WebcamViewer) |
| Engine tests | Worked examples of the sink, the source, the player, media, media lists, discoverers, the equalizer, dialogs, events and loading | [`tests/CodeBrix.Platform.MediaPlayerCore.Tests`](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore/tree/main/tests/CodeBrix.Platform.MediaPlayerCore.Tests) |
| Webcam tests | Live-camera, device, options, overlay, photo, compositor and public-API-leak suites | [`tests/CodeBrix.Webcam.Tests`](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore/tree/main/tests/CodeBrix.Webcam.Tests) |

The sample carries a `.slnx` for the CodeBrix.Platform heads and a second one that adds the native
Windows heads, and it runs one head at a time:

```bash
cd samples/WebcamViewer
dotnet run --project CodeBrixPlatform/WebcamViewer.LinuxX11        # or
dotnet run --project CodeBrixPlatform/WebcamViewer.LinuxWayland
dotnet run --project CodeBrixPlatform/WebcamViewer.LinuxFrameBuffer
dotnet run --project CodeBrixPlatform/WebcamViewer.MacOS
dotnet run --project CodeBrixPlatform/WebcamViewer.Win32Skia
dotnet run --project CodeBrixPlatform/WebcamViewer.WinWpfSkia
dotnet run --project WebcamViewer.Wpf                                # Windows
```

The reference application in [CodeBrix.Samples](https://github.com/ellisnet/CodeBrix.Samples) is
[WebcamViewer](https://github.com/ellisnet/CodeBrix.Samples/tree/main/WebcamViewer), a six-head CodeBrix.Platform viewer built
against the published packages, with a camera picker, an audio-monitor switch, a folder picker and a
Photo button that writes the frame on screen as a PNG.

Overlay burn-in and video recording are deliberately not in the sample. For the two-mode
capture-then-paint flow with mirrored UX and hand tracking, see the WebcamPainter application in the
[sample repositories](../samples/README.md).

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore/blob/main/README.md) |
| Engine API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore/blob/main/AGENT-README.txt) |
| Seam API guide | [src/CodeBrix.Platform.MediaPlayerCore/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore/blob/main/src/CodeBrix.Platform.MediaPlayerCore/AGENT-README.txt) |
| Webcam API guide | [src/CodeBrix.Webcam/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore/blob/main/src/CodeBrix.Webcam/AGENT-README.txt) |
| Samples and other non-package content | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore/blob/main/README-INDEX.txt) |
| Samples | [samples/WebcamViewer](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore/tree/main/samples/WebcamViewer) |

Each package carries its own `AGENT-README.txt` inside it, so an agent can be pointed at the file for
the package it is writing code against.

## License

`CodeBrix.MediaCore.LgplLicenseForever`, `CodeBrix.Platform.MediaPlayerCore.LgplLicenseForever` and
`CodeBrix.Webcam.LgplLicenseForever` are all licensed under the GNU Lesser General Public License,
version 2.1 or later, and the license is also named in each package ID. Consume them through
`<PackageReference>` and never merge the assemblies into another one - doing so forfeits the
relinkability LGPL-2.1 section 6 requires. The `LICENSE` file and the notices file ship inside all three
packages. For the provenance and licensing of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [MediaPlayer add-in](../platform/add-ins/MediaPlayer.md) - the ready-made XAML element built on the seam package
- [CodeBrix.Imaging](CodeBrix.Imaging.md) - turns a webcam photo's raw BGRA into PNG or JPEG
- [Graphics, media and vision](../platform/09-graphics-media-and-vision.md) - where media sits in a CodeBrix.Platform application
- [ellisnet/CodeBrix.Platform.MediaPlayerCore on GitHub](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore) - source, tests and samples
