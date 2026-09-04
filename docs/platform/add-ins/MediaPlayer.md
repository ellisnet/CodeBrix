<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Add-ins](../08-add-ins.md) › MediaPlayer</sub>

# MediaPlayer

**The MediaPlayer add-in makes the XAML `MediaPlayerElement` control play audio and video on every CodeBrix.Platform head except macOS.** One package covers the Win32, WPF, X11, Wayland and frame-buffer heads. You program against the standard `MediaPlayerElement`, `Windows.Media.Playback.MediaPlayer` and `Windows.Media.Core.MediaSource` contract that already lives in the core framework package; this add-in supplies the engine underneath it.

| | |
| --- | --- |
| **Package** | [`CodeBrix.Platform.MediaPlayer.LgplLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.MediaPlayer.LgplLicenseForever) |
| **Adds** | Working playback for `MediaPlayerElement`, `MediaPlayer`, `MediaPlaybackSession`, `MediaPlaybackItem`, `MediaPlaybackList` and `MediaSource`; the add-in's own `SkiaMediaPlayerExtension.PreloadVlc()` warm-up call |
| **Heads** | Win32, WPF, X11, Wayland, frame buffer. On macOS the add-in is inert: the macOS head has built-in AVFoundation media support and needs no libvlc |
| **Requires** | The native libvlc runtime, per operating system: `sudo apt install libvlc5 vlc-plugin-base` on Linux, the [`VideoLAN.LibVLC.Windows`](https://www.nuget.org/packages/VideoLAN.LibVLC.Windows) package in the Windows head projects, nothing on macOS |

## Add it to your application

Add the package once, to your application's `.Core` project:

```bash
dotnet add package CodeBrix.Platform.MediaPlayer.LgplLicenseForever
```

Every head project inherits it transitively. Do not reference it from a head project, and do not look for a per-head variant: there is none. The WebView add-in follows the same rule.

The Windows heads (Win32 and Skia-on-WPF) and the Linux heads (X11, Wayland, frame buffer) activate it automatically, through OS-gated `ApiExtension` registrations that the XAML source generator emits into your application. On macOS neither registration matches, so the add-in is inert.

The `.Core` project ends up like this:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.Platform.ApacheLicenseForever" />
    <PackageReference Include="CodeBrix.Platform.MediaPlayer.LgplLicenseForever" />
  </ItemGroup>
</Project>
```

The package ships managed code only. You must provide the native libvlc runtime per operating system. On Linux the base plugin set is enough; the full VLC application is not needed. On Windows, add one package to the Windows head projects only, which copies `libvlc.dll`, `libvlccore.dll` and the plugins folder into the head's output:

```xml
  <ItemGroup>
    <PackageReference Include="VideoLAN.LibVLC.Windows" />
  </ItemGroup>
```

There is no XAML namespace to declare: `MediaPlayerElement` comes from the default XAML xmlns. The consumer usings all live in the core framework package:

```csharp
using Microsoft.UI.Xaml.Controls;   // MediaPlayerElement, MediaTransportControls
using Microsoft.UI.Xaml.Media;      // Stretch
using Windows.Media.Playback;       // MediaPlayer, MediaPlaybackSession,
                                    // MediaPlaybackState, MediaPlaybackItem,
                                    // MediaPlaybackList, MediaPlayerFailedEventArgs
using Windows.Media.Core;           // MediaSource
```

The add-in's own namespace is needed only for the optional warm-up call:

```csharp
using CodeBrix.Platform.UI.MediaPlayer.Skia;   // SkiaMediaPlayerExtension
```

Package ids carry the license suffix; namespaces do not.

> [!IMPORTANT]
> This is the only published CodeBrix.Platform package that is not Apache-2.0. It is LGPL-2.1-or-later, and the `.LgplLicenseForever` suffix is deliberate truth-in-labeling so that nobody adds an LGPL dependency to an application by accident. If your application must stay LGPL-free, do not use this package: on macOS you do not need it at all, and for audio-only playback the Apache-2.0 [AudioPlayer](AudioPlayer.md) add-in is the alternative.

## Using it

### Declaring the player

The element with the built-in transport controls is all the markup a player needs.

```xml
<Page x:Class="MyApp.Views.MainPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <MediaPlayerElement x:Name="Player"
                            AutoPlay="True"
                            AreTransportControlsEnabled="True"
                            Stretch="Uniform" />
    </Grid>
</Page>
```

`AreTransportControlsEnabled` gives you play, pause and a seek bar without writing a command; `Stretch` is applied at paint time by the add-in, not by the media engine.

The code-behind sets the source from a URI.

```csharp
// MainPage.xaml.cs
using Microsoft.UI.Xaml.Controls;
using Windows.Media.Core;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
        Player.Source = MediaSource.CreateFromUri(
            new Uri("https://example.com/clip.mp4"));
    }
}
```

Setting `Source` stops the current media, loads the new one, and plays it when `AutoPlay` is true.

### Source URI forms

`MediaSource.CreateFromUri(Uri uri)` is the only implemented factory on the Skia heads. These schemes are resolved before the URI is handed to the media engine.

| Form | Resolves to |
| --- | --- |
| `http://`, `https://` | Network streams and progressive-download files, plus any other scheme libvlc itself can open |
| `file:///absolute/path` | A file on disk |
| `ms-appx:///Assets/x.mp4` | A file under the application's install folder - a Content item copied to the output |
| `ms-appdata://local/x.mp4` | A file in the application's local data folder |
| A relative or scheme-less URI | Treated as `ms-appx:///<value>` |

Ship media you install with the application as Content in the `.Core` project - `<Content Include="Assets\intro.mp4" CopyToOutputDirectory="PreserveNewest" />` - and address it as `ms-appx:///Assets/intro.mp4`. Codecs and containers are whatever the installed libvlc plugin set decodes.

### Driving playback from code

The element creates its own `MediaPlayer` when its template is applied, so reach it from `Loaded` or later.

```csharp
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Media.Core;
using Windows.Media.Playback;

public sealed partial class MainPage : Page
{
    private MediaPlayer _player;

    public MainPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Player.MediaPlayer exists once the element's template is applied.
        _player = Player.MediaPlayer;
        _player.MediaOpened += (p, _) =>
            Status.Text = $"Opened, {p.NaturalDuration:mm\\:ss}, video={p.IsVideo}";
        _player.MediaEnded += (p, _) => Status.Text = "Ended";
        _player.MediaFailed += (p, args) =>
            Status.Text = $"Failed: {args.ErrorMessage}";
        _player.PlaybackSession.PositionChanged += (s, _) =>
            Elapsed.Text = s.Position.ToString(@"mm\:ss");
        _player.PlaybackSession.PlaybackStateChanged += (s, _) =>
            State.Text = s.PlaybackState.ToString();

        _player.Volume = 0.8;
        _player.IsLoopingEnabled = true;
        _player.Source = MediaSource.CreateFromUri(
            new Uri("ms-appx:///Assets/intro.mp4"));
        _player.Play();
    }

    private void PauseButton_Click(object sender, RoutedEventArgs e)
        => _player.Pause();

    private void SeekButton_Click(object sender, RoutedEventArgs e)
        => _player.PlaybackSession.Position = TimeSpan.FromSeconds(30);
}
```

Every event this engine raises arrives on the UI thread: `SourceChanged`, `MediaOpened`, `MediaEnded`, `MediaFailed`, `VolumeChanged`, `IsMutedChanged` and `NaturalVideoDimensionChanged` on the player, and `PositionChanged`, `PlaybackStateChanged`, `NaturalDurationChanged`, `PlaybackRateChanged` and `BufferingProgressChanged` on the session.

The transport surface is the standard one: `Play()`, `Pause()`, `Stop()`, `StepForwardOneFrame()`, `NextTrack()`, `PreviousTrack()`, `Dispose()`, with `Volume` (0.0..1.0), `IsMuted`, `Position`, `NaturalDuration`, `PlaybackRate`, `IsLoopingEnabled`, `IsLoopingAllEnabled`, `CanPause`, `CanSeek`, `IsVideo` and `PlaybackSession`. `MediaPlaybackSession.Position` is clamped to `0..NaturalDuration`, and `PlaybackState` is one of `None`, `Opening`, `Buffering`, `Playing`, `Paused`.

### Supplying your own player

Constructing a `MediaPlayer` yourself creates the engine at that moment, which is useful when you want the player to outlive one page.

```csharp
var player = new MediaPlayer { AutoPlay = false };   // engine created here
player.Source = MediaSource.CreateFromUri(new Uri("file:///home/me/a.mp3"));
Player.SetMediaPlayer(player);      // or: Player.MediaPlayer = player;
player.Play();
```

`AutoPlay` on the element is copied to the player when the element loads, so set it yourself on a player you swap in later.

### Playlists

A `MediaPlaybackList` set as the element's source advances automatically at the end of each item.

```csharp
var list = new MediaPlaybackList();
foreach (var url in new[] { "https://example.com/1.mp4", "https://example.com/2.mp4" })
{
    list.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromUri(new Uri(url))));
}
Player.MediaPlayer.IsLoopingAllEnabled = true;   // wrap at the end
Player.Source = list;                           // first item loads; AutoPlay plays it
// later: Player.MediaPlayer.NextTrack();
```

`IsLoopingAllEnabled` wraps from the last item back to the first; `NextTrack()` and `PreviousTrack()` move by hand.

### Full-window playback

Setting `IsFullWindow` asks the hosting window to enter full-screen mode and moves the layout root into the XamlRoot's full-window media root.

```csharp
private void FullScreen_Click(object sender, RoutedEventArgs e)
    => Player.IsFullWindow = !Player.IsFullWindow;
```

The element must already be loaded in the visual tree, otherwise a warning is logged and nothing happens. Full-screen is the host window's full-screen mode; there is no separate full-screen window and no multi-monitor targeting.

### Stretch and frame presentation

The add-in paints the latest decoded frame into the presenter's area, centered, on a black background, with linear filtering.

| `Stretch` | Result |
| --- | --- |
| `Uniform` | Fit inside, keep aspect ratio (the default) |
| `UniformToFill` | Cover the area, keep aspect ratio, clip the edges |
| `Fill` | Distort to fill |
| `None` | One-to-one pixels, centered |

While nothing is playing, or for audio-only media, the video area is left transparent and the video element is collapsed; `PosterSource` shows if it is set. After metadata is parsed the engine seeks to the first frame, so a freshly loaded video shows its first picture instead of black.

### Warming up the runtime

The first playback pays for loading the native runtime and its plugin cache. `PreloadVlc()` moves that cost to start-up by playing a tiny embedded sample on a background thread.

```csharp
using CodeBrix.Platform.UI.Hosting;
using CodeBrix.Platform.UI.MediaPlayer.Skia;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        SkiaMediaPlayerExtension.PreloadVlc();   // optional: warm up libvlc

        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseLinuxX11()
            .Build();
        host.Run();
    }
}
```

This is the one member of the add-in's own types that application code calls. The two public types it does have, `SkiaMediaPlayerExtension` and `SkiaMediaPlayerPresenterExtension`, are instantiated by the framework through the `ApiExtension` registrations; never construct them yourself.

### How playback reaches the screen

The media engine decodes, and decoded video frames are delivered into memory through libvlc's windowing-system-agnostic memory output, surfaced by the [CodeBrix.Platform.MediaPlayerCore](../../libraries/CodeBrix.Platform.MediaPlayerCore.md) library's `VideoFrameSink`. The add-in copies each frame into a Skia image and paints it directly into the Skia scene, exactly like any other XAML content. Four consequences follow:

- No native child windows, so no airspace problems: clipping, transforms, opacity and z-order all behave normally, and XAML content can be drawn on top of the video.
- No XWayland: the Wayland head stays native.
- The frame-buffer head, which has no windowing system at all, is covered too.
- The XAML `Stretch` mode is applied at paint time by the add-in.

### What the engine does not implement

Stated exactly as the package states it, so you can plan around it:

- `MediaSource.CreateFromStream(...)`, `CreateFromStreamReference(...)` and `CreateFromStorageFile(...)` throw `NotImplementedException`. Write embedded content to a file first and play it by URI.
- `MediaPlaybackSession.NaturalVideoWidth` and `NaturalVideoHeight` throw `NotImplementedException`. Use `MediaPlayer.IsVideo` and the `NaturalVideoDimensionChanged` event to learn that a video track exists.
- `StepBackwardOneFrame()` throws `NotImplementedException`; the engine steps forward only.
- `ToggleCompactOverlay(bool)` is a no-op on the Skia heads.
- `BufferingProgress` is always 0, and `MediaPlayerFailedEventArgs.Error` is always `MediaPlayerError.Unknown`; read `ErrorMessage` and `ExtendedErrorCode` instead.
- These events are declared on `MediaPlayer` but not driven by this engine: `SeekCompleted`, `BufferingStarted`, `BufferingEnded`, `CurrentStateChanged`, `MediaPlayerRateChanged`, `PlaybackMediaMarkerReached`, `VideoFrameAvailable`, `SubtitleFrameChanged`. Watch `PlaybackSession.PositionChanged` after a seek instead of `SeekCompleted`.
- Frames always pass through system memory; there is no hardware-accelerated zero-copy presentation. That is what makes the add-in head-agnostic.

## Per-head notes

| Head | Notes |
| --- | --- |
| Win32, WPF | Activate automatically. The head project needs the `VideoLAN.LibVLC.Windows` package. `MediaPlayer.Stop()` is dispatched to a thread-pool thread, because calling the engine's stop on the UI thread deadlocks there, so `Stop()` returns before playback has actually stopped |
| X11, Wayland, frame buffer | Activate automatically. Install the runtime with `sudo apt install libvlc5 vlc-plugin-base`. The Wayland head stays native - no XWayland |
| macOS | The add-in is inert; the head plays media with AVFoundation on its own. Nothing to install |

On all three Linux heads the add-in contains no head-specific audio code: libvlc picks its own audio output (PulseAudio, PipeWire or ALSA), including on the frame-buffer head, which has no desktop session. The operating system's audio server may restore a saved per-application or per-media-role mute onto the engine's output stream, so media plays silently although the application never asked for mute. The add-in detects this once per media and logs a warning, and deliberately does not unmute, because the user may have muted it on purpose.

On Linux the add-in creates libvlc with software decoding requested (`--avcodec-hw=none`), because frames must land in system memory for the memory output. With only `vlc-plugin-base` installed, the hardware-decode probes fail and the engine falls back to software decoding; playback works, at the cost of extra start-up and log noise. Installing `vlc-plugin-video-output` adds hardware decoding with copy-back.

> [!WARNING]
> The engine is created the first time a `MediaPlayer` is constructed, which `MediaPlayerElement` does itself when its template is applied, at first layout. If the native runtime cannot be loaded, that construction throws `PlatformNotSupportedException` whose message names the fix, with the MediaPlayerCore `VLCException` as its `InnerException`. On Linux the message reads "The native libvlc runtime was not found. Install it via the system package manager: sudo apt install libvlc5 vlc-plugin-base (Debian/Ubuntu)."; on Windows, "The native libvlc runtime was not found. Add the VideoLAN.LibVLC.Windows package to your Windows head project(s)." The same exception is logged, not thrown, from `PreloadVlc()`, which runs on a background thread.

## Pitfalls

- Reference the package once, in `.Core`. Referencing it from a head project, or looking for a per-head package, is the most common mistake. The Windows heads still need the separate `VideoLAN.LibVLC.Windows` package, and the Linux machines still need the apt packages.
- `Player.MediaPlayer` is null before the element's template is applied. Subscribe to its events from `Loaded` or later, or create your own `MediaPlayer` and assign it with `SetMediaPlayer`.
- `AutoPlay` on the element is copied to the player when the element loads. If you swap in your own `MediaPlayer` afterwards, set its `AutoPlay` yourself.
- On the Windows heads, do not assume the state is `None` immediately after `Stop()`; watch `PlaybackStateChanged` instead.
- On Linux, silent playback with no error is usually an operating-system-level mute. Check the log for the muted-at-the-OS-audio-layer warning and unmute in the system mixer; the application will not override it.
- `X11HostBuilder.PreloadMediaPlayer(bool)` and `Win32HostBuilder.PreloadMediaPlayer(bool)` have no effect with this package. Call `SkiaMediaPlayerExtension.PreloadVlc()` yourself. The Wayland, frame-buffer and WPF host builders have no such option at all.
- Setting `IsFullWindow` before the element is in the visual tree does nothing; a warning is logged. Toggle it from a handler that runs after `Loaded`.
- `MediaFailed` always reports `MediaPlayerError.Unknown`. Inspect `ErrorMessage` and the engine's log output for the real cause.
- `SetUriSource`, `SetFileSource`, `SetStreamSource` and `SetMediaSource` are deprecated and throw. Set `MediaPlayer.Source` instead.
- Do not construct `SkiaMediaPlayerExtension` or `SkiaMediaPlayerPresenterExtension` yourself; the framework does.
- Every video frame is a full BGRA copy into a Skia image on an engine thread, then a blit on the UI thread. The cost scales with the video's pixel size, not the element's size.
- Do not poll `Position` on a timer; subscribe to `PlaybackSession.PositionChanged`.

## Related

- [CodeBrix.Platform.MediaPlayerCore](../../libraries/CodeBrix.Platform.MediaPlayerCore.md) - the managed engine binding this add-in plays through; it arrives automatically with the package
- [AudioPlayer](AudioPlayer.md) - the Apache-2.0 add-in for audio-only playback, sound effects and MIDI, with no native runtime to install
- [VideoPlayer](VideoPlayer.md) - the add-in for the family's own video format, when you control the encoding
- [MediaPlayerDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/MediaPlayerDemo) in CodeBrix.Platform - a URL box, a Load button and a Stretch selector over a `MediaPlayerElement` with transport controls, on six heads
- [MediaPlayerDemo](https://github.com/ellisnet/CodeBrix.Samples/tree/main/MediaPlayerDemo) in [the sample applications](../../samples/README.md) - the same idea done MVVM-first, with the view model owning the address string and exposing the built source as `IMediaPlaybackSource`

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.MediaPlayer.Skia/AGENT-README.txt) |
| Add-in source | [src/AddIns/Platform.UI.MediaPlayer.Skia](https://github.com/ellisnet/CodeBrix.Platform/tree/main/src/AddIns/Platform.UI.MediaPlayer.Skia) |
| Reference application | [samples/CodeBrixPlatform/MediaPlayerDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/MediaPlayerDemo) |
| Package | [`CodeBrix.Platform.MediaPlayer.LgplLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.MediaPlayer.LgplLicenseForever) |

---

**Where to go next**

- [AudioPlayer](AudioPlayer.md) - audio, sound effects and MIDI with nothing native to install
- [Graphics, media and vision](../09-graphics-media-and-vision.md) - where playback fits among the family's media libraries
- [All add-ins](../08-add-ins.md) - the whole set at a glance
