<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Add-ins](../08-add-ins.md) › VideoPlayer</sub>

# VideoPlayer

**VideoPlayer adds one XAML-declarable element that plays AV1 video from WebM and Matroska containers and from CodeBrix `.cbv` video files, with Ogg Vorbis or Opus sound.** There is no per-OS engine and nothing to install: the container readers, the demultiplexer, the clock and the sound are fully managed, and the picture is composed with SkiaSharp - on the graphics device wherever the running head can supply one, and on the processor everywhere else. The element is live on all six heads.

| | |
| --- | --- |
| **Package** | [`CodeBrix.Platform.VideoPlayer.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.VideoPlayer.ApacheLicenseForever) |
| **Adds** | `VideoPlayer`, a bindable `Panel`; `IVideoLayer`, the drawing seam for a reusable layer; and the event argument types `VideoComposingEventArgs`, `VideoPlayerFailedEventArgs`, `VideoPlayerRenderPathChangedEventArgs` |
| **Heads** | All six: Windows Win32-Skia, Windows WPF-Skia, Linux X11, Linux Wayland, Linux frame buffer and macOS |
| **Requires** | The application references and registers [`CodeBrix.VideoPlayback.Dav1d.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoPlayback.Dav1d.BsdLicenseForever) for AV1, and [`CodeBrix.Audio.Opus.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.Opus.BsdLicenseForever) for Opus soundtracks. No head project changes, no native prerequisite, no GPU wiring |

## Add it to your application

Add the add-in and the two codec packages the application registers itself:

```bash
dotnet add package CodeBrix.Platform.VideoPlayer.ApacheLicenseForever
dotnet add package CodeBrix.VideoPlayback.Dav1d.BsdLicenseForever
dotnet add package CodeBrix.Audio.Opus.BsdLicenseForever        (Opus sound only)
```

Reference all three from the project that carries your framework package references - the application's `.Core` project in the standard CodeBrix.Platform layout. The XAML in the shared `.UI` project then resolves the `video:` namespace.

Three dependencies flow in with the add-in: [`CodeBrix.Platform.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.ApacheLicenseForever) (the core framework), [`CodeBrix.Platform.Graphics3DGL.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Graphics3DGL.ApacheLicenseForever) (the off-screen graphics-device Skia context this element takes wherever a head can give one) and [`CodeBrix.VideoPlayback.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoPlayback.MitLicenseForever) (containers, demultiplexer, clock, transport, captions, chapters, the effect chain and the color-shader source). Bringing Graphics3DGL in is what makes `<video:VideoPlayer Source="…"/>` need zero GPU wiring from the application; the cost, stated plainly, is that a consumer also gets [`CodeBrix.Platform.OpenGL.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.OpenGL.MitLicenseForever) and the bundled ANGLE libraries on macOS.

The XAML namespace and the C# using:

```xml
xmlns:video="using:CodeBrix.Platform.UI.VideoPlayer.Skia"
```

```csharp
using CodeBrix.Platform.UI.VideoPlayer.Skia;
```

The element surfaces the playback engine's own types rather than wrapping them, so nothing has to be converted. Import the ones you use:

```csharp
using CodeBrix.VideoPlayback.Rendering;        // VideoRenderPath, VideoRenderBackend,
                                              //   VideoCompositionContext
using CodeBrix.VideoPlayback.Effects;          // IVideoFrameEffect, LutEffect
using CodeBrix.VideoPlayback.Captions;         // CaptionTrack, CaptionCue
using CodeBrix.VideoPlayback.Chapters;         // Chapter
using CodeBrix.VideoPlayback.Playback;         // ChapterChangedEventArgs
using CodeBrix.VideoPlayback.Sources;          // FileSourceMode
using CodeBrix.VideoPlayback.Containers;       // MediaTrackInfo
using CodeBrix.VideoPlayback.Presentation;     // VideoFramePresenterStatistics
```

`Stretch` is the XAML one, so it reads exactly as it does on an `Image`.

### Register the codecs

This is the one add-in here that requires calls from the application. Both go in start-up, once, before a source is opened; there is deliberately no module initializer doing it for you, because that works in a debug build and silently does not run in a trimmed publish.

```csharp
using CodeBrix.VideoPlayback.Dav1d;
using CodeBrix.Audio.Opus;

public App()
{
    CodeBrixVideoPlaybackDav1d.Register();   // AV1 - needed for every coded file
    CodeBrixAudioOpus.Register();            // only for Opus soundtracks
    InitializeComponent();
}
```

The first is needed for every AV1 file, which is every file this family authors: no coded video decodes without it. The second is needed only for a soundtrack encoded as Opus - Vorbis needs nothing. Until the calls are made, `MediaFailed` carries a message naming the package and the call.

> [!WARNING]
> AV1 decoding is BSD-2-Clause and Opus is BSD-3-Clause; this add-in is Apache-2.0 and takes neither as a dependency. That is why the application references and registers them, rather than getting them transitively.

## Using it

### Declare the element

Unlike the AudioPlayer element, this one is visual: give it a place with a size - a `Grid` cell, or `Width` and `Height` - or you will see nothing.

```xml
<video:VideoPlayer x:Name="Player"
                   Source="ms-appx:///Assets/video/clip.cbv"
                   Stretch="Uniform" />
```

That, plus the two registration calls, is a working video player:

```xml
<Page xmlns:video="using:CodeBrix.Platform.UI.VideoPlayer.Skia" ...>
    <Grid>
        <video:VideoPlayer Source="ms-appx:///Assets/video/clip.cbv"
                           AutoPlay="True" Stretch="Uniform" />
    </Grid>
</Page>
```

`Stretch` is applied at paint time, so changing it costs a repaint and nothing else. `Uniform` letterboxes; `UniformToFill` covers and crops. The picture keeps its display aspect ratio, so a portrait recording is drawn portrait inside a landscape window with bars either side.

### Transport and a scrubber

The transport surface is a superset of the AudioPlayer element's - the same members plus `IsMuted` - so one scrubber markup drives either kind of player. `Position` and `PositionSeconds` are refreshed on the UI thread by a dispatcher timer while playing, and writing either one seeks. Those writes are debounced, so a whole slider drag lands one seek when the thumb is released; `Seek(TimeSpan)` is immediate and not debounced. A seek while paused still puts the new frame on screen.

```xml
<Grid RowDefinitions="*,Auto,Auto">
    <video:VideoPlayer x:Name="Player" Grid.Row="0"
                       Source="ms-appx:///Assets/video/clip.webm"
                       MediaFailed="Player_MediaFailed" />

    <StackPanel Grid.Row="1" Orientation="Horizontal" Spacing="8">
        <TextBlock Text="{Binding Position, ElementName=Player}" Width="80" />
        <Slider Width="420"
                Maximum="{Binding DurationSeconds, ElementName=Player}"
                Value="{Binding PositionSeconds, ElementName=Player, Mode=TwoWay}"
                StepFrequency="0.1" />
        <TextBlock Text="{Binding Duration, ElementName=Player}" Width="80" />
    </StackPanel>

    <StackPanel Grid.Row="2" Orientation="Horizontal" Spacing="8">
        <Button Content="Play"  Click="Play_Click" />
        <Button Content="Pause" Click="Pause_Click" />
        <Button Content="Stop"  Click="Stop_Click" />
        <ToggleSwitch OnContent="Looping" OffContent="Loop"
                      IsOn="{Binding IsLooping, ElementName=Player, Mode=TwoWay}" />
        <TextBlock Text="{Binding ActiveRenderPath, ElementName=Player}"
                   VerticalAlignment="Center" />
    </StackPanel>
</Grid>
```

Notice the last `TextBlock`: `ActiveRenderPath` reports what is actually running, which is not necessarily what `RenderPath` asked for.

### Sources, and how a local file is read

`Source` accepts a file path, a `file://` URI, an `http://` or `https://` address, an `ms-appx:///Assets/...` application-asset URI, or an `embedded://AssemblyName/Manifest.Resource.Name` embedded resource. In the embedded form, `"."` means the application's own assembly, and `"(assembly)"` inside the resource name is replaced with the resolved assembly's name. Setting `Source` opens the file - `Duration` is available the moment it returns - and, when `AutoPlay` is true, starts playback. Set `""` to unload. For bytes that are neither a file nor an embedded resource, call `SetSourceStream(Stream)`; the stream should be seekable, because a forward-only one plays but cannot seek.

`SourceMode` decides how a **local file** is read: `Streaming` (the default), `MemoryMapped` or `Preloaded`. Preloaded reads the whole file into memory once, which is what makes a short clip loop with no disk access at all. It is read at open, so set it before `Source`.

```csharp
Player.SourceMode = FileSourceMode.Preloaded;   // BEFORE Source
Player.IsLooping = true;
Player.Source = "ms-appx:///Assets/video/logo_sting.cbv";
Player.Play();
```

Preloaded is for short clips. A feature-length file read into memory is exactly as expensive as it sounds, which is why `Streaming` is the default.

### The render path

`RenderPath` states the intent: `GpuAuto` (the default) takes the graphics device wherever a context can be created and quietly falls back to the processor where it cannot - no exception, no user-facing error. `GpuNoFallback` fails with a message instead of degrading, for an application whose picture is meaningless without its effect chain. `Cpu` forces the processor path even where a graphics device exists.

```csharp
// Before any Source is set.
Player.RenderPath = VideoRenderPath.GpuNoFallback;
Player.MediaFailed += (_, e) => Status.Text = e.Message;   // says why, plainly
```

`RenderPath` must be set before a source is opened; changing it while a source is open throws `InvalidOperationException`, so close the source first by setting `Source = ""`. Read `ActiveRenderPath` and `EffectsActive` - and subscribe to `RenderPathChanged` - to know what is really happening.

Neither path is a degraded version of the other. On the graphics path the three color planes are uploaded as single-channel textures and one shader does the color conversion and the whole effect chain in a single pass at full precision. On the processor path the playback core's vector converter turns the frame into BGRA pixels; that is the right answer on a machine with no usable graphics device, and it is a tested, benchmarked configuration in its own right.

### Color grading with an effect chain

`Effects` is an ordered chain applied to the picture - color lookup tables first and foremost. However many are in the chain, they are composed into one resultant table and cost a single lookup per pixel. The collection belongs to the element and keeps its identity for the element's whole life, so a binding to it never goes stale.

```csharp
using CodeBrix.VideoPlayback.Effects;

// Before Play, or while paused: the chain is composed once, not per frame.
Player.Effects.Add(LutEffect.FromCubeFile(lutPath, applyAtPercent: 40));

Player.RenderPathChanged += (_, e) =>
    Status.Text = e.EffectsActive
        ? $"grading on the {e.ActiveRenderPath} path"
        : $"running on the {e.ActiveRenderPath} path - the grade is not applied";
```

A change reaches the screen straight away whether playback is running or not: while playing, the next frame is composed through the new chain; while paused, the element composes the frame already on screen again. No seek and no `Play` is needed to see a grade you have dialed in. The same is true of `Layers` and of `AllowEffectsOnCpu`.

`AllowEffectsOnCpu` defaults to false. Left false, a configured chain stays configured but is not applied when the processor path is running. Setting it true applies the chain there too, at the cost of a table lookup per pixel of every frame.

### Drawing inside the picture

`Layers` and the `Composing` event draw over the video, *inside* the composition, in video coordinates - so what they draw is part of the picture, and `CapturePresentedFrame()` captures it. Subtitles, a heads-up display, annotation, a webcam picture-in-picture. Use `Composing` for a one-off; write an `IVideoLayer` for anything reusable.

```csharp
Player.Composing += (_, e) =>
{
    // e.Canvas is the composition surface; its coordinates are the VIDEO's, and
    // e.Context carries the video rect, the timestamp, the frame number, the
    // running backend and whether effects are active.
    using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true };
    e.Canvas.DrawCircle(e.Context.VideoRect.MidX, e.Context.VideoRect.MidY, 24, paint);
};
```

### Capturing what is on screen

`CapturePresentedFrame()` returns an independent copy of the composed frame - effect chain and every layer already in it - which the caller owns and must dispose. It is null before the first frame, and safe from any thread.

```csharp
using SKImage? shot = Player.CapturePresentedFrame();
if (shot is not null)
{
    using SKData png = shot.Encode(SKEncodedImageFormat.Png, 100);
    File.WriteAllBytes(path, png.ToArray());
}
```

This is the screenshot hook, and the way a headless verification proves pixels flowed.

### Captions and chapters as data

Captions are data: the player says which cues are current and the application decides how - and whether - to draw them. An `IVideoLayer` is the natural place.

| Member | What it gives you |
| --- | --- |
| `CaptionTracks`, `SelectedCaptionTrack` | The text tracks in the file; null selection means captions off |
| `ShowForcedCaptions` | Default true |
| `ActiveCues`, `CaptionCuesChanged` | The cues current at this instant, and when that set changes |
| `Chapters`, `CurrentChapter`, `ChapterChanged` | Flat, single-edition chapters with per-language titles |
| `SeekToChapter(int)`, `NextChapter()`, `PreviousChapter()` | Chapter navigation |
| `Tracks` | The container's track list |
| `FrameStatistics` | Posted, presented, superseded, late and dropped frame counters |

### Formats it plays

- **Video.** AV1 (`"av01"`), 8/10/12-bit, 4:2:0 / 4:2:2 / 4:4:4, through the separately-registered Dav1d package; and uncompressed video (`V_UNCOMPRESSED`), which the playback core decodes itself with no package at all - a test and tooling format, enormous on disk. There is no H.264, no HEVC and no MP4: that is the point of this family - nothing royalty-bearing and nothing copyleft in a shipped application.
- **Sound.** Ogg Vorbis, built in; Opus, once the Opus package is registered.
- **Containers.** WebM and Matroska (`.webm`, `.mkv`), and CodeBrix `.cbv` video in both its flavors: the WebM-profile one, which any browser also opens, and the bespoke one.
- **Captions.** WebVTT and SRT text tracks, carried as data. **Chapters.** Flat, single-edition, with per-language titles.

### Teardown

`Close()` stops playback and releases everything the player owns: the decode threads, the soundtrack, the composition surface and the graphics context. The element cannot play again afterwards. It is optional - leaving the visual tree already releases the graphics resources and pauses playback, and re-entering the tree brings the picture back - so call it when a page wants the decode threads and the audio device gone at a moment of its choosing.

### The whole integration

```xml
<PackageReference Include="CodeBrix.Platform.VideoPlayer.ApacheLicenseForever" />
<PackageReference Include="CodeBrix.VideoPlayback.Dav1d.BsdLicenseForever" />
<PackageReference Include="CodeBrix.Audio.Opus.BsdLicenseForever" />
<Content Include="..\assets\clip.cbv" Link="Assets\video\clip.cbv"
         CopyToOutputDirectory="PreserveNewest" />
```

No head project changes, no native prerequisite, no GPU wiring.

<details>
<summary>The whole application-facing surface of the VideoPlayer element</summary>

```csharp
[Bindable] sealed class VideoPlayer : Panel
    string   Source                     TimeSpan Duration          (ro)
    FileSourceMode SourceMode           double   DurationSeconds   (ro)
    bool     AutoPlay                   bool     IsPlaying         (ro)
    TimeSpan Position          (2-way)  double   Volume            0..1
    double   PositionSeconds   (2-way)  bool     IsMuted
    bool     IsLooping                  TimeSpan PositionUpdateInterval
    Stretch  Stretch                    VideoRenderPath RenderPath  (before Source!)
    bool     AllowEffectsOnCpu          VideoRenderBackend ActiveRenderPath (ro)
                                        bool     EffectsActive     (ro)
    ObservableCollection<IVideoFrameEffect> Effects
    ObservableCollection<IVideoLayer>       Layers
    CaptionTrack? SelectedCaptionTrack   bool ShowForcedCaptions
    IReadOnlyList<CaptionTrack> CaptionTracks   (ro)
    IReadOnlyList<CaptionCue>   ActiveCues      (ro)
    IReadOnlyList<Chapter>      Chapters        (ro)
    Chapter? CurrentChapter                     (ro)
    IReadOnlyList<MediaTrackInfo> Tracks        (ro)
    VideoFramePresenterStatistics FrameStatistics (ro)
    void Play();  void Pause();  void Stop();  void Seek(TimeSpan position);
    void SetSourceStream(Stream stream);        void Close();
    void SeekToChapter(int index);  bool NextChapter();  bool PreviousChapter();
    SKImage? CapturePresentedFrame();           // caller owns it
    event EventHandler MediaOpened;             event EventHandler PlaybackEnded;
    event EventHandler<VideoPlayerFailedEventArgs> MediaFailed;
    event EventHandler<VideoPlayerRenderPathChangedEventArgs> RenderPathChanged;
    event EventHandler CaptionCuesChanged;
    event EventHandler<ChapterChangedEventArgs> ChapterChanged;
    event EventHandler<VideoComposingEventArgs> Composing;

sealed class VideoPlayerFailedEventArgs : EventArgs
    string Message { get; }     Exception? Error { get; }

sealed class VideoPlayerRenderPathChangedEventArgs : EventArgs
    VideoRenderBackend ActiveRenderPath { get; }    bool EffectsActive { get; }
```

</details>

## Per-head notes

- **Graphics backend.** The element creates its off-screen graphics context lazily, on the first frame after it is loaded and has a live `XamlRoot`. One informational log line names the backend it chose - OpenGL or OpenGL ES on the Windows and Linux heads, Metal on macOS - and one warning explains a failure.
- **Windows.** A missing OpenGL driver is the usual cause of that failure; installing the OpenCL and OpenGL Compatibility Pack from the Microsoft Store can supply one, and the warning says so.
- **Windows, WGL.** The context is torn down when the element leaves the visual tree, not when it is disposed, and rebuilt lazily when it comes back. That is not an optimization: on WGL the off-screen context is built on the window's own device context, so once the window is destroyed the graphics resources can never be released at all.
- **Frame buffer.** The processor path is the right answer on a machine with no usable graphics device, and it is a tested, benchmarked configuration in its own right.

## Pitfalls

- Forgetting `CodeBrixVideoPlaybackDav1d.Register()`. No AV1 file plays without it - and every file this family authors is AV1 - so it is the single most likely reason a first attempt shows a black rectangle. An uncompressed test clip does play without it, which is exactly how a missing registration can hide until the first real file. Handle `MediaFailed` while developing: its message names the package and the call.
- Setting `RenderPath` after `Source`. It throws, on purpose: the path is chosen once, before anything is opened.
- Setting `SourceMode` after `Source`. It does not throw; it has no effect until the next open, because the mode is read when the file is opened.
- Giving the element no size. It is a visual element: in a `StackPanel` with no `Height` it is measured to nothing and shows nothing.
- Adding children to it. It is a `Panel` because it hosts its own picture surface. Put an overlay in a `Grid` cell above it, or draw inside the picture with `Layers` or `Composing`.
- Disposing what the element hands you. `CapturePresentedFrame()` returns an image the **caller** owns and must dispose; everything else the element exposes belongs to the element.
- Assuming `ActiveRenderPath` is `Gpu` because `RenderPath` is `GpuAuto`. `GpuAuto` is permission, not a promise.
- Expecting an `Effects` change to be applied on the processor path. It is not, unless `AllowEffectsOnCpu` is true - that is deliberate and silent, and `EffectsActive` is what says which is happening. A change while paused does reach the screen; the element recomposes the frame it is holding.
- Expecting captions to appear on screen. They are data. Drawing them is the application's job, through an `IVideoLayer`.
- `Close()` is terminal: the element cannot play again afterwards.
- Set `PositionUpdateInterval` no finer than a scrubber can show; its default is already several layout passes a second.
- Resizing the window suppresses live presents briefly after the last size change and re-blits the picture letterboxed instead, deliberately: a backlog of full-size blits is what makes a resize go chunky and then catch up. Nothing to configure.
- A repaint - overlap, theme change, resize - re-blits the picture already presented. It never asks the decoder for anything.
- Out of scope by design: no MP4 or ISOBMFF and no H.264, HEVC or AAC; no hardware video **decoding** (the graphics device does color conversion, grading and compositing, not decoding); no playback rate other than 1.0, no HDR tone-mapping, no rotation metadata for foreign files, no DRM; no caption rendering and no built-in transport chrome; no playlist or queue - one source per element, so chain `PlaybackEnded` or declare several elements; and no streaming protocol beyond plain progressive or range HTTP(S).

## Related

- [CodeBrix.VideoPlayback](../../libraries/CodeBrix.VideoPlayback.md) - the playback engine behind the element, and the library that authors the files it reads
- [CodeBrix.VideoPlayback.Dav1d](../../libraries/CodeBrix.VideoPlayback.Dav1d.md) - AV1 decoding, referenced and registered by the application
- [CodeBrix.Audio.Opus](../../libraries/CodeBrix.Audio.Opus.md) - Opus soundtracks, registered the same way
- [AudioPlayer](AudioPlayer.md) - the element whose transport surface this one extends, so one scrubber markup drives either
- [VideoPlayerDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/VideoPlayerDemo) - the element filling the window, a drop-down of sample clips in landscape and portrait across all three container flavors including a chaptered one, a free-text path box, the AudioPlayerDemo scrubber markup unchanged, a render-path drop-down, a preloaded-loop option, an `ActiveRenderPath` / `EffectsActive` readout and a chapter list
- **CodeBrixVideoTool** in [CodeBrix.Samples](https://github.com/ellisnet/CodeBrix.Samples/tree/main/CodeBrixVideoTool) - a media library, player and converter that hosts this element behind a view-model-facing surface, with chapters and captions driven from MVVM

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.VideoPlayer.Skia/AGENT-README.txt) |
| Add-in source (`VideoPlayer.cs` and `Internal/`, fully XML-documented) | [src/AddIns/Platform.UI.VideoPlayer.Skia](https://github.com/ellisnet/CodeBrix.Platform/tree/main/src/AddIns/Platform.UI.VideoPlayer.Skia) |
| Sample application - start at `VideoPlayerDemo.UI/Views/MainPage.xaml` | [samples/CodeBrixPlatform/VideoPlayerDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/VideoPlayerDemo) |
| Shared media assets | [samples/assets/video](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/assets/video) |
| Package | [`CodeBrix.Platform.VideoPlayer.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.VideoPlayer.ApacheLicenseForever) |

---

**Where to go next**

- [WebView](WebView.md) - the other add-in that puts a full engine inside the Skia scene
- [Graphics, media and vision](../09-graphics-media-and-vision.md) - where video sits among the family's media capabilities
- [All add-ins](../08-add-ins.md) - the whole set at a glance
