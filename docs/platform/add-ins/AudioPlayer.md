<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Add-ins](../08-add-ins.md) › AudioPlayer</sub>

# AudioPlayer

**The AudioPlayer add-in gives your application audio playback, overlapping sound effects and MIDI music, as XAML-declarable elements plus one static class.** It plays WAV, MP3, Ogg Vorbis and FLAC, and Opus once the application registers the separate codec package. There is no native setup at all: no per-OS engine and nothing to install, so the add-in is live on all six heads.

| | |
| --- | --- |
| **Package** | [`CodeBrix.Platform.AudioPlayer.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.AudioPlayer.ApacheLicenseForever) |
| **Adds** | `AudioPlayer` and `MidiPlayer` (non-visual XAML elements), the static `SoundEffect`, and `AudioPlayerFailedEventArgs` |
| **Heads** | All six - Windows Win32-Skia, Windows WPF-Skia, Linux X11, Linux Wayland, Linux frame buffer and macOS |
| **Requires** | Nothing beyond the framework's own requirements. [`CodeBrix.Audio.Opus.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.Opus.BsdLicenseForever) is optional, and only for `.opus` |

## Add it to your application

Reference the package from the project that carries your framework package references - the application's `.Core` project in the standard layout. The XAML in the shared `.UI` project then resolves the `audio:` namespace.

```bash
dotnet add package CodeBrix.Platform.AudioPlayer.ApacheLicenseForever
```

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>MyApp</RootNamespace>
    <DefineConstants>$(DefineConstants);HAS_CODEBRIX;HAS_CODEBRIX_WINUI</DefineConstants>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.Platform.ApacheLicenseForever" />
    <PackageReference Include="CodeBrix.Platform.AudioPlayer.ApacheLicenseForever" />
    <!-- only if you play .opus: -->
    <!-- <PackageReference Include="CodeBrix.Audio.Opus.BsdLicenseForever" /> -->
  </ItemGroup>
  <ItemGroup>
    <!-- Assets/song.mp3 must be a Content item so ms-appx:///Assets/song.mp3 exists -->
    <Content Include="Assets\**" />
  </ItemGroup>
</Project>
```

Two dependencies flow in automatically, with no separate install: `CodeBrix.Platform.ApacheLicenseForever` and [`CodeBrix.Audio.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MitLicenseForever), the managed audio engine that does the WAV, MP3, Ogg Vorbis and FLAC decoding, the MIDI handling and the SoundFont and SFZ synthesis. Its bundled audio backend covers Windows, Linux and macOS on x64 and arm64.

Declare the XAML namespace and, in code, the matching using:

```xml
xmlns:audio="using:CodeBrix.Platform.UI.AudioPlayer.Skia"
```

```csharp
using CodeBrix.Platform.UI.AudioPlayer.Skia;
```

Every public type of this package lives in that one namespace. Two audio-engine namespaces come up only at the edges:

```csharp
using CodeBrix.Audio.Wave;     // SharedAudioOutput - only if you pin the
                               // output sample rate yourself (see SAMPLE
                               // RATES below)
using CodeBrix.Audio.Synth;    // CodeBrix.Audio's MIDI-music types; a
                               // lambda assigned to MidiMessageProcessed
                               // needs no using at all
```

There is no engine to install, no head-specific code, and no start-up call - unless you add Opus.

> [!TIP]
> Both players are non-visual elements: they render nothing and take no space. Declare them anywhere in a page's tree, typically as the first child of the root `Grid`, and give them no `Width`, `Height` or `Margin`.

## Using it

### A player, a transport and a scrubber

The whole point of the element's design is the scrubber: `Position` and `PositionSeconds` update on the UI thread while playing and are two-way bindable, so a `Slider` both follows playback and seeks it.

```xml
<Page ...
    xmlns:audio="using:CodeBrix.Platform.UI.AudioPlayer.Skia">
  <StackPanel Spacing="8" Padding="16">
    <audio:AudioPlayer x:Name="Player"
        Source="ms-appx:///Assets/song.mp3"
        Volume="0.8"
        PlaybackEnded="OnPlaybackEnded"
        MediaFailed="OnMediaFailed" />

    <StackPanel Orientation="Horizontal" Spacing="8">
      <Button Content="Play"  Click="OnPlay" />
      <Button Content="Pause" Click="OnPause" />
      <Button Content="Stop"  Click="OnStop" />
      <ToggleSwitch Header="Loop"
          IsOn="{Binding IsLooping, ElementName=Player, Mode=TwoWay}" />
    </StackPanel>

    <Slider
        Maximum="{Binding DurationSeconds, ElementName=Player}"
        Value="{Binding PositionSeconds, ElementName=Player, Mode=TwoWay}" />

    <TextBlock x:Name="Status"
        Text="{Binding Position, ElementName=Player}" />
  </StackPanel>
</Page>
```

Writes to `Position` and `PositionSeconds` seek the audio, debounced 200 ms, so a slider drag lands one seek where the user releases the thumb. The code-behind is the transport plus the two events.

```csharp
using System;
using Microsoft.UI.Xaml;
using CodeBrix.Platform.UI.AudioPlayer.Skia;

public sealed partial class MainPage : Page
{
    public MainPage() => InitializeComponent();

    private void OnPlay(object sender, RoutedEventArgs e)  => Player.Play();
    private void OnPause(object sender, RoutedEventArgs e) => Player.Pause();
    private void OnStop(object sender, RoutedEventArgs e)  => Player.Stop();

    private void OnPlaybackEnded(object sender, EventArgs e)
        => Status.Text = "Finished";

    private void OnMediaFailed(object sender, AudioPlayerFailedEventArgs e)
        => Status.Text = e.Message;     // e.Error holds the exception

    // Jump 10 s ahead, immediately (no debounce):
    private void OnSkip(object sender, RoutedEventArgs e)
        => Player.Seek(Player.Position + TimeSpan.FromSeconds(10));
}
```

`Seek(TimeSpan)` bypasses the debounce and is clamped to `0..Duration`. `PlaybackEnded` is the natural end of the file, and is not raised when `IsLooping` is true, when `Stop()` is called, or when playback fails. `MediaFailed` carries an `AudioPlayerFailedEventArgs` with a `Message` and the underlying `Error`; load and play failures raise it and log at Error rather than throwing into a binding path.

The rest of the surface is `Source` (setting it loads the file synchronously, so `Duration` is valid immediately; an empty string unloads), `AutoPlay`, `Duration` and `DurationSeconds`, `IsPlaying`, `Volume` (0.0..1.0, clamped, default 1.0), `IsLooping`, `PositionUpdateInterval` (the refresh cadence of `Position` while playing, default 150 ms), and `SetSourceStream(Stream)`.

### Source forms

`AudioPlayer.Source`, `MidiPlayer`'s `Source` and `.sf2` `Instrument`, and `SoundEffect.Play` and `Preload` all resolve source strings the same way.

```text
a filesystem path                 "/home/me/music/song.mp3", "C:\...\song.mp3"
a file:// URI                     "file:///home/me/music/song.mp3"
an ms-appx:/// asset URI          "ms-appx:///Assets/theme.mid" - resolved
                                  under the application's installed folder.
                                  The two-slash form ms-appx://LibraryName/x
                                  (an asset that arrived in a library package)
                                  names the same thing. Spaces and other
                                  escaped characters are unescaped, so an
                                  asset called "My Song.mp3" resolves.
an embedded:// URI                "embedded://AssemblyName/Manifest.Resource.Name"
                                  - an embedded resource, the same scheme the
                                  SVG and Lottie add-ins use. "." as the
                                  assembly name means the application
                                  assembly; "(assembly)" inside the resource
                                  name is replaced with the resolved assembly
                                  name.
a Stream                          AudioPlayer.SetSourceStream(Stream) and
                                  SoundEffect.Play(Stream, double).
```

An `.sfz` instrument is the one exception: it takes only a filesystem path or an `ms-appx:///` URI.

Loading from a stream is one line, and the player takes ownership of the stream.

```csharp
// Any readable, seekable stream; the player owns it from here on.
Player.SetSourceStream(File.OpenRead(pathChosenByUser));
Player.Play();
```

### Sound effects

`SoundEffect` is a static class for fire-and-forget audio. Each call is one voice in the application's single shared output, so effects overlap each other and the `AudioPlayer` cheaply.

```csharp
static bool Play(string source, double volume = 1.0)
static bool Play(Stream stream, double volume = 1.0)
static void Preload(string source)
static void ClearCache()
```

An effect is decoded once, on its first play, and the decoded audio is kept, so a sound triggered repeatedly costs nothing but mixing, and no file access or decoding ever happens on the real-time audio thread.

```csharp
using CodeBrix.Platform.UI.AudioPlayer.Skia;

// Optional: read the bytes during a loading screen so the first play
// does no file access.
SoundEffect.Preload("ms-appx:///Assets/Sfx/laser.ogg");
SoundEffect.Preload("embedded://./MyApp.Assets.Sfx.explosion.wav");

// Fire and forget; overlapping calls each get their own voice.
if (!SoundEffect.Play("ms-appx:///Assets/Sfx/laser.ogg", volume: 0.6))
{
    // Already logged at Error; decide whether to tell the user.
}

// Releasing everything (e.g. when leaving a game level):
SoundEffect.ClearCache();
```

`Play` returns false and logs at Error instead of throwing when an effect fails to resolve, decode or start, so a missing effect never crashes the application. `Play(Stream)` reads the stream in full before returning, so you can dispose it immediately afterwards - but a stream has no identity to cache under, so it decodes on every call. Use the string overload for anything played repeatedly.

### MIDI music through a SoundFont or SFZ instrument

`MidiPlayer` synthesizes a MIDI file through a SoundFont (`.sf2`) or SFZ (`.sfz`) instrument, and its transport is `AudioPlayer`'s member for member - which means the same scrubber markup drives either player. Those are the two instrument formats this element accepts; [SoundFont and SFZ instruments](../../libraries/audio/soundfont-and-sfz.md) covers what each of them can express, and [MIDI files](../../libraries/audio/midi-files.md) covers the file the element reads.

```xml
<audio:MidiPlayer x:Name="Music"
    Source="ms-appx:///Assets/theme.mid"
    Instrument="ms-appx:///Assets/Piano/Piano.sfz"
    AutoPlay="True"
    MediaOpened="OnMusicOpened"
    MediaFailed="OnMediaFailed" />
<ProgressRing IsActive="{Binding IsLoading, ElementName=Music}" />
<Ellipse x:Name="BeatIndicator" Width="24" Height="24" Fill="Orange" Opacity="0.2" />
<TextBlock x:Name="Status" />
<Slider Maximum="{Binding DurationSeconds, ElementName=Music}"
        Value="{Binding PositionSeconds, ElementName=Music, Mode=TwoWay}" />
<Slider Minimum="0.25" Maximum="2" StepFrequency="0.05"
        Value="{Binding Speed, ElementName=Music, Mode=TwoWay}" />
```

Instruments are large, so loading is asynchronous: setting `Source` or `Instrument` raises `IsLoading`, loads on a thread-pool thread, and raises `MediaOpened` when the transport is live. `Duration` is valid from `MediaOpened` onward, not from the property set, and `Play()` before that is a no-op - set `AutoPlay` instead. Setting `Source` and `Instrument` one after the other queues one load that covers both, and a newer set while a load is running supersedes it. Instruments are cached process-wide, so a second player sharing one pays nothing.

Beyond the shared transport, `MidiPlayer` adds `Instrument` (the file extension decides which synthesizer runs; loading starts once both `Source` and `Instrument` are non-empty), `IsLoading`, `Speed` (a tempo multiplier, default 1.0, that does not change pitch) and `ActiveVoiceCount`. `IsLooping` repeats from the sequence's own loop point when it carries one, `Stop()` also silences every sounding voice and clears the controller state the sequence had set, and `Seek` replays controller state up to the target so instruments sound right - though notes already sounding there do not resume, so a seek into the middle of a held chord starts from silence.

Two plain properties, valid from `MediaOpened` onward, tell you what the instrument could not do: `InstrumentProblems` lists what the loaded instrument could not make sense of, such as a referenced sample file that is missing, and `UnsupportedInstrumentOpcodes` lists SFZ opcodes the instrument uses that the synthesizer does not implement (always empty for a SoundFont). Both empty means fully supported. Show them rather than guessing when an instrument sounds wrong.

### Live mixing and the message hook

The mixing methods are safe from any thread and are no-ops until a load has completed.

```csharp
private void OnMusicOpened(object sender, EventArgs e)
{
    Status.Text = $"Loaded, {Music.Duration:mm\\:ss}, "
        + $"{Music.InstrumentProblems.Count} problems, "
        + $"{Music.UnsupportedInstrumentOpcodes.Count} unsupported opcodes";

    // Observe-only hook. Runs on the AUDIO THREAD: do the minimum here and
    // marshal to the UI thread through the element's DispatcherQueue.
    Music.MidiMessageProcessed = (channel, command, note, velocity) =>
    {
        if (command == 0x90 && velocity > 0 && channel == 9)   // drums
        {
            Music.DispatcherQueue.TryEnqueue(() => BeatIndicator.Opacity = 1.0);
        }
    };
}

// Mixing a layered arrangement live while it plays:
Music.SetChannelVolume(3, 0.0);      // drop the lead layer out...
Music.SetChannelVolume(3, 1.0);      // ...and bring it back
Music.SetChannelPan(1, -0.5);        // bass a little to the left
Music.SetChannelProgram(2, 48);      // strings on channel 3 (0-based 2)
Music.SendMidiMessage(0, 0x90, 60, 100);   // middle C, note-on, channel 1
```

`SendMidiMessage(channel, command, data1, data2)` takes a channel 0-15, the command nibble (`0x80` note-off, `0x90` note-on, `0xB0` control change, `0xC0` program change, `0xE0` pitch bend) and data bytes 0-127. `SetChannelVolume` is control change 7 and `SetChannelPan` is control change 10, with pan running from -1.0 (full left) through 0.0 (center) to 1.0 (full right). `SetChannelProgram` sends a program change; which sound a number selects is the loaded instrument's business.

`MidiMessageProcessed` is raised after each MIDI message has reached the synthesizer, so it cannot break playback. It runs on the real-time audio thread: keep it fast and allocation-free, do not touch the UI in it, and do not call back into the player. The audio engine's other hook - the modifying one, which replaces delivery and silences the music if a caller does not re-deliver - is deliberately not exposed by this element.

### Adding Opus

Opus is BSD-3-Clause rather than MIT, so it ships as a separate package. The application references it and makes one call; this add-in needs no change and no reference to it, because playback resolves codecs through the shared audio output.

```csharp
// In the application project: reference CodeBrix.Audio.Opus.BsdLicenseForever
// and register it once, before the first .opus source is set.
CodeBrixAudioOpus.Register();
Player.Source = "ms-appx:///Assets/voice.opus";   // now plays
```

From then on `.opus` plays through `AudioPlayer` and `SoundEffect` like any other format. An `.opus` file played without that registration fails with a `MediaFailed` message naming Opus and saying what to do. The add-in supplies that explanation itself, because the engine's own message names the container instead - "No registered and working codec factory found for decoding format \"ogg\"" - and Ogg is the same container for Vorbis, Opus and Ogg FLAC.

### Sample rates

Effects do not have to share one sample rate. Each is converted to the output's format when it is decoded, so an asset pack mixing 22 kHz and 44.1 kHz files works unchanged, and so does `AudioPlayer`. Feeding the audio engine's `WaveOutEvent` yourself is different: it has no resampler and rejects a source whose rate differs from the running output. If you drive `WaveOutEvent` directly alongside this add-in, pin the output format once at start-up, before the first sound plays.

```csharp
using CodeBrix.Audio.Wave;                       // CodeBrix.Audio.MitLicenseForever
SharedAudioOutput.Configure(sampleRate: 48000);  // Configure(sampleRate[, channels])
```

`SharedAudioOutput` is the one shared output device every player, effect and `WaveOutEvent` in the process mixes into. `Configure` is optional: left alone, the output adopts the format of the first sound played. [Playback and sound effects](../../libraries/audio/playback.md) is the full treatment, including the codec seam an add-on package registers through.

### Shipping an instrument as a package

An instrument can travel as a NuGet package of its own, using the same machinery the font packages use. Build a library project with `GenerateLibraryLayout=true` and the instrument tree as `Content` items with target paths, which packs to `lib/<tfm>/<AssemblyName>/...` beside an empty `<AssemblyName>.uprimarker` file. At head-build time the framework's asset expansion globs that whole folder recursively and copies it into the application output with its shape intact, so an `.sfz` keeps its `Samples/` and `Data/` neighbors and is addressed as `ms-appx:///<AssemblyName>/<name>.sfz`. Sample formats follow the audio engine: WAV, FLAC and Ogg work as they are, and an instrument built on `.opus` samples additionally needs the application to register the Opus package.

### What the add-in does not do

- No Opus decoding on its own: that is the separate package plus one `Register()` call.
- No modifying MIDI hook - transposing or re-channelling as it plays. Only the observe-only `MidiMessageProcessed` is exposed.
- No recording, no microphone input, no audio analysis or DSP. Playback only.
- No visual chrome: no transport control, no waveform, no volume slider. The players are non-visual elements, and you compose the UI from ordinary controls bound to their properties.
- No playlist or queue: one source per element. Chain `PlaybackEnded` handlers or declare several elements.
- No streaming from `http` or `https` URLs. Sources are files, application assets, embedded resources or streams you open.
- Not a per-element output device: everything mixes into the process's one shared output.

## Per-head notes

None. Playback is fully managed through the audio engine package, whose bundled backend covers Windows, Linux and macOS on x64 and arm64, so the add-in behaves identically on all six heads. Unlike the [WebView](WebView.md) and [MediaPlayer](MediaPlayer.md) add-ins there is no per-OS engine and nothing to install.

## Pitfalls

- Writing `Position` or `PositionSeconds` seeks with a 200 ms debounce - that is what makes slider drags land one seek. For an immediate jump call `Seek(TimeSpan)`.
- The players pause themselves when they are unloaded from the visual tree, so navigating away from a page pauses its audio. Audio that must survive navigation belongs to an element on a page that stays in the tree, such as the shell.
- `AudioPlayer.Duration` is valid the moment `Source` is set; `MidiPlayer.Duration` is valid only from `MediaOpened`. `Play()` on a `MidiPlayer` before `MediaOpened` is a silent no-op - set `AutoPlay="True"` or call `Play()` in the `MediaOpened` handler.
- A `MidiPlayer` needs both `Source` and `Instrument` before anything loads. With one of them empty it sits unloaded, and `IsLoading` stays false.
- An `.sfz` must be a real file on disk, reached by path or `ms-appx:///`. It is not one file - it references its samples as separate files beside it, and may include others - so it needs a real directory to resolve against. An embedded `.sfz` fails with a `MediaFailed` message saying exactly that; a `.sf2` works in every form.
- `SetSourceStream` hands the stream to the player. Do not dispose it yourself afterwards, and do not reuse it for another player.
- `SoundEffect.Play(Stream)` is uncached and re-decodes on every call, so a hot effect through that overload costs decoding every trigger.
- An `.opus` source without the Opus package registered fails. Read `AudioPlayerFailedEventArgs.Message`, which names Opus and the fix.
- `SharedAudioOutput.Configure` is only needed when you feed `WaveOutEvent` directly with mixed sample rates; this add-in's own types never need it.
- `Speed` below 0 clamps to 0, which freezes the transport while sounding notes ring out. Use `Pause()` to pause.
- `SetChannelVolume` is MIDI control change 7, so a track that automates its own volume overwrites your value at its next such event.
- Load and play failures never throw from the players; they raise `MediaFailed` and log. Subscribe to `MediaFailed` or you will not know.
- A `MidiMessageProcessed` lambda that touches UI directly crashes or corrupts state. Always go through `DispatcherQueue.TryEnqueue`.
- `SendMidiMessage` and `SetChannelProgram` throw `ArgumentOutOfRangeException` for a channel outside 0-15 or a program outside 0-127, and `SetSourceStream` and `SoundEffect.Play(Stream)` throw `ArgumentNullException` on null.
- `ClearCache()` releases decoded audio. Call it at level boundaries in an asset-heavy game rather than letting every effect ever played stay resident.
- `PositionUpdateInterval` is the UI refresh cadence: raise it for a page that only shows a coarse indicator, lower it for a tight visualizer.

## Related

- [CodeBrix.Audio](../../libraries/CodeBrix.Audio.md) - the managed audio engine underneath, usable directly from any .NET 10 application for readers, writers, synthesis and DSP. Its section goes well past what this element exposes: Decent Sampler instruments, multi-track songs and stems, MPE read out of a MIDI file, recording, and the DSP primitives
- [Playback and sound effects](../../libraries/audio/playback.md) - the players this add-in wraps, and the shared output every voice mixes into
- [SoundFont and SFZ instruments](../../libraries/audio/soundfont-and-sfz.md) - the two instrument formats `MidiPlayer` accepts, in full
- [CodeBrix.Audio.Opus](../../libraries/CodeBrix.Audio.Opus.md) - the separate BSD-3-Clause package that adds `.opus`
- [MediaPlayer](MediaPlayer.md) - video playback and the standard `MediaPlayerElement` contract, when audio alone is not enough
- [AudioPlayerDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/AudioPlayerDemo) in CodeBrix.Platform - three panes on six heads: a song player with a format and source-form selector, sound effects, and a MIDI pane synthesizing through an SFZ instrument, with the transport bound to `Position`, `Duration`, `Speed` and `ActiveVoiceCount`
- [KenneyAssetBrowser](https://github.com/ellisnet/CodeBrix.Samples/tree/main/KenneyAssetBrowser) in [the sample applications](../../samples/README.md) - plays audio straight out of an archive in memory, through a bridge of settable delegates so a head with no player degrades to a viewer that says so

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.AudioPlayer.Skia/AGENT-README.txt) |
| Add-in source | [src/AddIns/Platform.UI.AudioPlayer.Skia](https://github.com/ellisnet/CodeBrix.Platform/tree/main/src/AddIns/Platform.UI.AudioPlayer.Skia) |
| Reference application | [samples/CodeBrixPlatform/AudioPlayerDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/AudioPlayerDemo) |
| Package | [`CodeBrix.Platform.AudioPlayer.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.AudioPlayer.ApacheLicenseForever) |

---

**Where to go next**

- [MediaPlayer](MediaPlayer.md) - video and the standard playback contract, on five of the six heads
- [CodeBrix.Audio](../../libraries/CodeBrix.Audio.md) - the front door of the audio section: decoding, sampled instruments, multi-track songs, synthesis and analysis outside a UI
- [All add-ins](../08-add-ins.md) - the whole set at a glance
