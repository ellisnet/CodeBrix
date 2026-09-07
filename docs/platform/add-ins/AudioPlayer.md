<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Add-ins](../08-add-ins.md) › AudioPlayer</sub>

# AudioPlayer

**The AudioPlayer add-in gives your application audio playback, overlapping sound effects and MIDI music, as XAML-declarable elements plus one static class.** It plays WAV, MP3, Ogg Vorbis and FLAC, and Opus once the application registers the separate codec package. MIDI music is synthesized live through a SoundFont, an SFZ or a Decent Sampler instrument, with a beat clock, the MIDI Polyphonic Expression settings and, for a Decent Sampler preset, that preset's own knobs as a live control model. There is no native setup at all: no per-OS engine and nothing to install, so the add-in is live on all six heads.

| | |
| --- | --- |
| **Package** | [`CodeBrix.Platform.AudioPlayer.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.AudioPlayer.ApacheLicenseForever) |
| **Adds** | `AudioPlayer` and `MidiPlayer` (non-visual XAML elements), the static `SoundEffect`, the `MidiInstrumentKind` enumeration, and `AudioPlayerFailedEventArgs` |
| **Heads** | All six - Windows Win32-Skia, Windows WPF-Skia, Linux X11, Linux Wayland, Linux frame buffer and macOS |
| **Requires** | Nothing beyond the framework's own requirements. Two optional add-on packages that the application references and registers itself: [`CodeBrix.Audio.Opus.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.Opus.BsdLicenseForever) for `.opus`, and [`CodeBrix.Audio.ModestSynth.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.ModestSynth.MitLicenseForever) for the oscillators and creative effects a Decent Sampler preset may ask for |

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

Two dependencies flow in automatically, with no separate install: `CodeBrix.Platform.ApacheLicenseForever` and [`CodeBrix.Audio.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MitLicenseForever), the managed audio engine that does the WAV, MP3, Ogg Vorbis and FLAC decoding, the MIDI handling and the SoundFont, SFZ and Decent Sampler synthesis. Its bundled audio backend covers Windows, Linux and macOS on x64 and arm64.

Two further packages are deliberately not dependencies. Both are add-ons the application references and registers itself, because this add-in depends on nothing outside its own license bar and both reach the engine anyway - codecs through the shared audio output, instrument features through a process-wide registry. [CodeBrix.Audio.Opus](../../libraries/CodeBrix.Audio.Opus.md) adds `.opus`, and [CodeBrix.Audio.ModestSynth](../../libraries/CodeBrix.Audio.ModestSynth.md) adds the oscillators and creative effects a Decent Sampler preset may ask for. See [Adding Opus](#adding-opus) and [Adding oscillators and creative effects](#adding-oscillators-and-creative-effects).

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

There is no engine to install and no head-specific code, and no start-up call unless you add one of the two add-on packages.

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

Instruments that must name a real place on disk are the one exception. An `.sfz` instrument, and every Decent Sampler form - `.dspreset`, `.dslibrary`, `.dsbundle`, or a folder holding a preset - take only a filesystem path or an `ms-appx:///` URI, never `embedded://` and never a stream. A `.sf2` SoundFont takes every form above; [MIDI music through a sampled instrument](#midi-music-through-a-sampled-instrument) has the reason for each format and the `MediaFailed` message that says so.

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

### MIDI music through a sampled instrument

`MidiPlayer` synthesizes a MIDI file through a sampled instrument, and its transport is `AudioPlayer`'s member for member - which means the same scrubber markup drives either player.

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

What `Instrument` names decides which synthesizer runs - the file extension first, then a folder that holds a preset - and `InstrumentKind` reports what it turned out to be.

| What `Instrument` names | The instrument | `InstrumentKind` |
| --- | --- | --- |
| `.sf2` | A SoundFont | `SoundFont` |
| `.sfz` | An SFZ instrument | `Sfz` |
| `.dspreset` | A Decent Sampler preset | `DecentSampler` |
| `.dslibrary` or `.dsbundle` | A Decent Sampler container | `DecentSampler` |
| A folder holding a `.dspreset` | The same, and the shape a `.dsbundle` has on macOS and an unpacked library has everywhere | `DecentSampler` |

"Decent Sampler" is Decidedly LLC's name for the format and its player, and appears here only to say what the files are. [SoundFont and SFZ instruments](../../libraries/audio/soundfont-and-sfz.md) and [Decent Sampler instruments](../../libraries/audio/decent-sampler.md) cover what each format can express, and [MIDI files](../../libraries/audio/midi-files.md) covers the file the element reads.

An `.sfz` and every Decent Sampler form must be a real place on disk, reached by path or `ms-appx:///`, because each of them needs one: an `.sfz` references its samples as separate files beside it and may include others, a `.dspreset` references its samples as separate files beside it, and a `.dslibrary` or `.dsbundle`, though it is one file, is read in place from the archive on disk rather than unpacked. An `embedded://` instrument in any of those formats fails with `MediaFailed`, and the message names the format it was handed and says why. Nothing is opened to work that out, so the failure is about the form, not about a resource that happens to be missing.

Instruments are large, so loading is asynchronous: setting `Source`, `Instrument` or `InstrumentPreset` raises `IsLoading`, loads on a thread-pool thread, and raises `MediaOpened` when the transport is live. `Duration` is valid from `MediaOpened` onward, not from the property set, and `Play()` before that is a no-op - set `AutoPlay` instead. Setting all three one after the other queues one load that covers them all, and a newer set while a load is running supersedes it.

Beyond the shared transport, `MidiPlayer` adds these dependency properties.

| Property | What it is |
| --- | --- |
| `Instrument` | The instrument to render through. Loading starts once both `Source` and `Instrument` are non-empty |
| `InstrumentPreset` | Which preset to play from a Decent Sampler container that holds several. Empty, the default, takes the container's first; the other two formats ignore it |
| `InstrumentKind` | Read-only: `None`, `SoundFont`, `Sfz` or `DecentSampler`, and `None` until a load completes |
| `IsLoading` | Read-only: true while the background load runs |
| `Speed` | A tempo multiplier, default 1.0, that does not change pitch |
| `ActiveVoiceCount` | Read-only: synthesizer voices sounding now, refreshed with `Position` |
| `BeatsPerMinute` | Read-only: the music's own tempo where the transport is now, so a sequence with tempo changes reports the one in force rather than the one it started at. It takes no notice of `Speed`, which changes how fast the transport travels rather than what the music says |
| `BeatPosition` | Read-only: how far in, in beats - the musical counterpart of `PositionSeconds`, for a beat indicator or for lining something up with the bar |
| `DropAuxiliaryOutputs` | False by default, so nothing an instrument makes goes unheard. A Decent Sampler preset can route a group, a zone or a bus to an auxiliary stereo output, and this player has one stereo pair, so it folds them into the mix. Set it when a preset uses them for something a listener should not hear |

`IsLooping` repeats from the sequence's own loop point when it carries one, `Stop()` also silences every sounding voice and clears the controller state the sequence had set, and `Seek` replays controller state up to the target so instruments sound right - though notes already sounding there do not resume, so a seek into the middle of a held chord starts from silence.

`InstrumentLoadOptions` is a plain property rather than a dependency property, because it is configuration for the load rather than something a page binds to. It takes a `DecentSamplerLoadOptions`: the memory budget, the size above which a sample is streamed from disk instead of decoded, whether samples are decoded at all, and where the decode cache lives. Null, the default, takes the engine's own defaults. The element loads from a copy, so changing the object afterwards affects only later loads, and a preset name inside it is overridden by `InstrumentPreset` whenever that is set.

Five plain properties, all valid from `MediaOpened` onward, say what the load found.

| Property | What it carries |
| --- | --- |
| `InstrumentProblems` | What the loaded instrument could not make sense of - a referenced sample file that is missing, for example - for all three formats. The instrument still loads, and the regions it could not build are silent |
| `UnsupportedInstrumentOpcodes` | SFZ opcodes the instrument uses that the synthesizer does not implement. Empty for the other two formats |
| `UnsupportedInstrumentFeatures` | The Decent Sampler equivalent: elements, attributes, effect types and waveforms the engine did not understand. Some entries name the oscillator add-on package, and the preset still plays what it can |
| `InstrumentMemorySummary` | How the loaded Decent Sampler instrument decided to hold its samples - what was decoded, what streams, and against which budget - in the engine's own words, for a diagnostics panel or a log |
| `SourceProblems` | Anything the MIDI file itself needed forgiving. A Standard MIDI File that breaks a rule is read leniently rather than refused and says so here, and the sequence plays either way |

All of them empty means fully supported, read exactly as written. Show them rather than guessing when an instrument or a file sounds wrong.

Instruments are cached process-wide, so a second player naming the same one pays nothing: an `.sfz` by its resolved path, an `.sf2` by path when it was given as a path or an `ms-appx:///` URI, and a Decent Sampler instrument through the audio engine's own process-wide cache - the same one [CodeBrix.Audio](../../libraries/CodeBrix.Audio.md)'s path-based loading uses. An application that pre-loaded a library and a XAML `MidiPlayer` naming the same path therefore share one copy, and two presets of one library share that library's decoded samples. `InstrumentPreset` is part of what that cache keys on.

### A Decent Sampler preset's control model

A preset's interface section is a live control surface, and the element exposes it as the instrument's own objects rather than wrapping them. Writing a control fires the bindings behind it at once, exactly as turning that knob in a player would.

| Member | What it is |
| --- | --- |
| `InstrumentControls` | Every element of the preset's interface, in the order a binding counts them - knobs, sliders, menus, buttons, labels and images alike. Do not filter it, because an index in a binding counts them all |
| `InstrumentTagStates` | The live state of every tag the preset names: enabled, volume, pan and polyphony |
| `GetInstrumentControl(name)` | The first control of that name, case-insensitively, or null |
| `InstrumentControlChanged` | Raised on the UI thread when a control moves, whether something set it or a modulator or MIDI binding inside the preset moved it while the music played |

Those last changes arrive on the real-time audio thread and can move one knob on every rendered block, so the event is coalesced: one raise per control per dispatcher pass, carrying the last change seen for that control. Read the current value from the control the event args carry.

```xml
<audio:MidiPlayer x:Name="Music"
    Source="ms-appx:///Assets/theme.mid"
    Instrument="ms-appx:///Assets/Choir/Choir.dslibrary"
    InstrumentPreset="Cantores Oohs"
    MpeMode="Auto"
    MediaOpened="OnMusicOpened"
    InstrumentControlChanged="OnInstrumentControlChanged"
    MediaFailed="OnMediaFailed" />
<Slider x:Name="Knob" IsEnabled="False" ValueChanged="OnKnobMoved" />
<TextBlock x:Name="KnobName" />
```

```csharp
using CodeBrix.Audio.Synth.DecentSampler;
using CodeBrix.Platform.UI.AudioPlayer.Skia;

private DecentSamplerControl _attack;
private bool _writingFromInstrument;

private void OnMusicOpened(object sender, EventArgs e)
{
    // What actually loaded, and what it could not do. All three are valid
    // only from here on.
    Status.Text = $"{Music.InstrumentKind}: "
        + $"{Music.InstrumentProblems.Count} problem(s), "
        + $"{Music.UnsupportedInstrumentFeatures.Count} unsupported feature(s). "
        + Music.InstrumentMemorySummary;

    // A named knob of the preset. Setting its value fires the preset's own
    // bindings at once, exactly as turning it in a player would.
    _attack = Music.GetInstrumentControl("ATTACK");
    if (_attack is not null)
    {
        KnobName.Text = _attack.Name;
        Knob.Minimum = _attack.MinValue;
        Knob.Maximum = _attack.MaxValue;
        Knob.Value = _attack.Value;
        Knob.IsEnabled = true;
    }
}

private void OnKnobMoved(object sender, RangeBaseValueChangedEventArgs e)
{
    if (!_writingFromInstrument)
    {
        _attack?.SetValue(e.NewValue);
    }
}

// The other direction: the preset's own modulators and MIDI bindings move
// its knobs while the music plays. Already on the UI thread, and already
// coalesced to one raise per control per dispatcher pass.
private void OnInstrumentControlChanged(
    object sender, DecentSamplerControlChangedEventArgs e)
{
    if (ReferenceEquals(e.Control, _attack))
    {
        _writingFromInstrument = true;
        Knob.Value = e.Control.Value;
        _writingFromInstrument = false;
    }
}
```

Both the control types and the event args are `CodeBrix.Audio` types, from `CodeBrix.Audio.Synth.DecentSampler`, and that is the point: what a page binds to is the instrument's own parameter model. [Decent Sampler instruments](../../libraries/audio/decent-sampler.md) is the full treatment of that model, including tags, the memory policy and what a preset may ask for that the engine does not carry on its own.

### MIDI Polyphonic Expression

A performance recorded from an expressive controller spreads each note onto its own MIDI channel so that it can bend, brighten and swell alone, and all three instrument formats read such a file the same way. The element adds nothing to those semantics - [MIDI Polyphonic Expression](../../libraries/audio/mpe.md) is the contract - and only surfaces the settings.

| Member | What it does |
| --- | --- |
| `MpeMode` | `Off` (the default), `LowerZone`, `UpperZone`, `Both` or `Auto`. An exporter usually leaves out the message that configures the zones, which is what `Auto` is for |
| `MpeMemberBendRange` | How far a member channel's bend reaches, in semitones, when the music never says. A bend-range message in the music overrides it |
| `MpeLowerZoneMemberCount` and `MpeUpperZoneMemberCount` | 0, the default, means automatic. Pin one when a file is read differently from the way it was played |
| `MpeLowerZone` and `MpeUpperZone` | Read-only plain properties: the zones as the loaded instrument reads them now - whether each is active, its master and member channels, and the bend ranges in force |
| `GetReleaseVelocity(channel, key)` | The lift velocity of the last note-off for a key on a channel, the channel 0-based, or -1 when nothing is loaded. No instrument format defines what release velocity should do, so it changes nothing about how a preset sounds; it is there for an application to react to |

The four settings are settable in XAML before a load, changeable while the music plays, and they survive a load, reaching whichever instrument is loaded. To watch lifts as they happen rather than asking for the last one, use `MidiMessageProcessed`: a note-off carries the lift as its second data byte.

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

### Adding oscillators and creative effects

A Decent Sampler preset may ask for a synthesized oscillator in place of a sample, or for a creative effect the base engine does not carry. Those live in [CodeBrix.Audio.ModestSynth](../../libraries/CodeBrix.Audio.ModestSynth.md), which the application references and registers the same way it does Opus. This add-in references neither.

```csharp
// In the application project: reference CodeBrix.Audio.ModestSynth.MitLicenseForever
// and register it at start-up, BEFORE the first instrument loads.
using CodeBrix.Audio.ModestSynth;
ModestSynth.Register();
```

The order matters more here than it does for Opus. The engine looks those factories up when an instrument is built, so a registration that arrives afterwards reaches neither an instrument already loaded nor a synthesizer already made: the oscillator group stays silent and the effect stays bypassed until the instrument is loaded again, and nothing throws either way. Without the package a preset still loads and every sample group still plays; what it asked for and could not have is named in `InstrumentProblems` and `UnsupportedInstrumentFeatures`, and the element logs one warning carrying the engine's own advice line.

### Sample rates

Effects do not have to share one sample rate. Each is converted to the output's format when it is decoded, so an asset pack mixing 22 kHz and 44.1 kHz files works unchanged, and so does `AudioPlayer`. Feeding the audio engine's `WaveOutEvent` yourself is different: it has no resampler and rejects a source whose rate differs from the running output. If you drive `WaveOutEvent` directly alongside this add-in, pin the output format once at start-up, before the first sound plays.

```csharp
using CodeBrix.Audio.Wave;                       // CodeBrix.Audio.MitLicenseForever
SharedAudioOutput.Configure(sampleRate: 48000);  // Configure(sampleRate[, channels])
```

`SharedAudioOutput` is the one shared output device every player, effect and `WaveOutEvent` in the process mixes into. `Configure` is optional: left alone, the output adopts the format of the first sound played. [Playback and sound effects](../../libraries/audio/playback.md) is the full treatment, including the codec seam an add-on package registers through.

### Shipping an instrument as a package

An instrument can travel as a NuGet package of its own, using the same machinery the font packages use. Build a library project with `GenerateLibraryLayout=true` and the instrument tree as `Content` items with target paths, which packs to `lib/<tfm>/<AssemblyName>/...` beside an empty `<AssemblyName>.uprimarker` file. At head-build time the framework's asset expansion globs that whole folder recursively and copies it into the application output with its shape intact, so an `.sfz` keeps its `Samples/` and `Data/` neighbors and is addressed as `ms-appx:///<AssemblyName>/<name>.sfz`. Sample formats follow the audio engine: WAV, FLAC and Ogg work as they are, and an instrument built on `.opus` samples additionally needs the application to register the Opus package.

The formats pack differently, and the difference is only ever how many files there are - all of them are addressed by path once they are laid out.

| The instrument | What packs |
| --- | --- |
| `.sf2` | One file, and the only format that can be an embedded resource instead if you would rather not have a loose file |
| `.sfz` | The `.sfz` plus its `Samples/`, and any folder it includes, as a tree with its shape intact |
| `.dspreset` | The `.dspreset` plus its `Samples/`, and `Resources/` when the preset draws on one, the same way |
| `.dslibrary` or `.dsbundle` | One file, which packs trivially - but still as `Content` rather than an embedded resource, because the engine reads the container in place from the archive on disk |

The same rule covers an instrument that ships with the application rather than in a package: make it `Content` with `CopyToOutputDirectory`, keep the folder shape, and address it as `ms-appx:///Assets/<folder>/<name>.<extension>`.

### What the add-in does not do

- No Opus decoding, and no synthesized oscillators or creative effects, on its own. Each is one more package the application references plus one `Register()` call.
- No modifying MIDI hook - transposing or re-channeling as it plays. Only the observe-only `MidiMessageProcessed` is exposed.
- No recording, no microphone input, no audio analysis or DSP. Playback only.
- No visual chrome: no transport control, no waveform, no volume slider. The players are non-visual elements, and you compose the UI from ordinary controls bound to their properties.
- No rendering of a Decent Sampler preset's own interface. Its knobs, images, background and keyboard colors are exposed as a live control model, with all the geometry and color a renderer would need, and nothing here draws them: a page composes ordinary controls over `InstrumentControls` instead. A preset's own files, such as cover art or a text file, are reachable through the instrument's container when a page wants to show one.
- No stems or multi-track element: one sequence per `MidiPlayer`, one file per `AudioPlayer`. The library's own [multi-track player](../../libraries/audio/multi-track-and-suno.md) plays a song whose parts are separate files, including a download of separated stems.
- No MIDI device input. `MidiPlayer` plays MIDI files, and nothing here opens a controller; an expressive performance arrives as a recorded file.
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
- An `.sfz` and every Decent Sampler form must be a real place on disk, reached by path or `ms-appx:///`. An `.sfz` and a `.dspreset` reference their samples as separate files beside them; a `.dslibrary` or `.dsbundle` is one file, but it is read in place from the archive rather than unpacked. An `embedded://` instrument in any of them fails with a `MediaFailed` message that names the format and says why, and a `.sf2` works in every form. Packing one as an `EmbeddedResource` is the mistake that message is there to catch.
- Two players naming one Decent Sampler preset share its knobs. A preset keeps its knob positions, controller values and modulated parameters on the instrument, because that is where the format puts them - a binding writes the group's volume, not the synthesizer's - and instruments are shared. That is right for two players of the same sound and wrong when two parts must move their own knobs. There is no per-element instrument option, so load the second part through the library's own API with an instrument of its own.
- `InstrumentControls`, `InstrumentTagStates` and the other reporting properties are empty until `MediaOpened`. Reading them in the handler that set `Instrument` reads the previous load's state, or nothing at all.
- `ModestSynth.Register()` has to run before the instrument loads. Registering afterwards throws nothing and changes nothing: the oscillator group stays silent and the effect stays bypassed until the instrument is loaded again.
- A control's value belongs to the instrument, not to the element, so it survives the element and outlives an unload. Read it from `InstrumentControls` after `MediaOpened` rather than assuming a preset starts where it did last time.
- `InstrumentControlChanged` is coalesced onto the dispatcher, so a control set from code reports back on a later pass rather than inside the `SetValue` call. Do not write a round-trip check that reads the event synchronously.
- A knob is free to turn during playback with one exception: a control whose binding moves a sample's start or loop points takes effect on the next note, so dragging it under a held chord changes nothing you can hear until then.
- A Decent Sampler instrument decides per file whether to hold a sample in memory or stream it from disk, against a per-sample threshold and an instrument-wide budget. Both are on `InstrumentLoadOptions`, and `InstrumentMemorySummary` says what was decided - read it before tuning anything. A folder instrument streams better than the same library inside a `.dslibrary`, because seeking backwards inside a zip entry costs a walk from the start of it.
- `InstrumentLoadOptions.DecodeSamples = false` means load on first use, not never: every path is resolved and every problem is still found, and no audio file is opened until a note wants one. The first note that needs a file is silent and the instrument says so in `InstrumentProblems`; every note after it sounds.
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

- [CodeBrix.Audio](../../libraries/CodeBrix.Audio.md) - the managed audio engine underneath, usable directly from any .NET 10 application for readers, writers, synthesis and DSP. Its section goes past what this element exposes: multi-track songs and stems, offline rendering to a file, recording, and the DSP primitives
- [Playback and sound effects](../../libraries/audio/playback.md) - the players this add-in wraps, and the shared output every voice mixes into
- [SoundFont and SFZ instruments](../../libraries/audio/soundfont-and-sfz.md) - two of the three instrument formats `MidiPlayer` accepts, in full
- [Decent Sampler instruments](../../libraries/audio/decent-sampler.md) - the third format: its live control model, its tags, its effects and its memory policy
- [MIDI Polyphonic Expression](../../libraries/audio/mpe.md) - what the `Mpe` settings mean, and how a performance recorded onto one channel per note is read
- [Multi-track songs and stems](../../libraries/audio/multi-track-and-suno.md) - the library's own player for a song whose parts are separate files, which no element here wraps
- [CodeBrix.Audio.Opus](../../libraries/CodeBrix.Audio.Opus.md) - the separate BSD-3-Clause package that adds `.opus`
- [CodeBrix.Audio.ModestSynth](../../libraries/CodeBrix.Audio.ModestSynth.md) - the add-on an application registers for the oscillators and creative effects a preset may ask for
- [MediaPlayer](MediaPlayer.md) - video playback and the standard `MediaPlayerElement` contract, when audio alone is not enough
- [AudioPlayerDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/AudioPlayerDemo) in CodeBrix.Platform - three panes on six heads: a song player with a format and source-form selector, sound effects, and a MIDI pane that plays one piece through either of two instruments, a sampled SFZ piano or a small hand-written Decent Sampler preset that ships with the sample, with a slider driving the preset's first knob through `InstrumentControls` and `InstrumentControlChanged`, an MPE drop-down two-way bound to `MpeMode`, a beat readout, and a status line showing `InstrumentKind`, `InstrumentMemorySummary` and the MIDI file's own problems
- [KenneyAssetBrowser](https://github.com/ellisnet/CodeBrix.Samples/tree/main/KenneyAssetBrowser) in [the sample applications](../../samples/README.md) - plays audio straight out of an archive in memory, through a bridge of settable delegates so a head with no player degrades to a viewer that says so

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.AudioPlayer.Skia/AGENT-README.txt) |
| Add-in source | [src/AddIns/Platform.UI.AudioPlayer.Skia](https://github.com/ellisnet/CodeBrix.Platform/tree/main/src/AddIns/Platform.UI.AudioPlayer.Skia) |
| Reference application | [samples/CodeBrixPlatform/AudioPlayerDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/AudioPlayerDemo) |
| A small example instrument | [samples/assets/DemoSampler](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/assets/DemoSampler) - the hand-written Decent Sampler preset the demo plays, with a file header explaining every element in it |
| Package | [`CodeBrix.Platform.AudioPlayer.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.AudioPlayer.ApacheLicenseForever) |

---

**Where to go next**

- [MediaPlayer](MediaPlayer.md) - video and the standard playback contract, on five of the six heads
- [CodeBrix.Audio](../../libraries/CodeBrix.Audio.md) - the front door of the audio section: decoding, sampled instruments, multi-track songs, synthesis and analysis outside a UI
- [All add-ins](../08-add-ins.md) - the whole set at a glance
