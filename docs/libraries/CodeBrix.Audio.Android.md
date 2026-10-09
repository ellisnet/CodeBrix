<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Audio.Android</sub>

# CodeBrix.Audio.Android

**CodeBrix.Audio.Android is the Android platform package for [CodeBrix.Audio](CodeBrix.Audio.md): it
gives the shared managed audio library device playback and capture through Android's native
low-latency audio path, native WAV, MP3, FLAC and Ogg Vorbis codecs, audio-focus and device-change
helpers, MIDI 1.0 ports, and one-call extraction of packaged sample libraries out of the APK.** An
Android application references it *instead of* the desktop package, makes one start-up call, and every
existing CodeBrix.Audio API - playback, mixing, sample providers, effects, metadata, MIDI files,
synthesis and the file readers and writers - works unchanged. It is also the audio backend underneath
the Android packages of the AudioPlayer and VideoPlayer add-ins in
[CodeBrix.Android](CodeBrix.Android.md).

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Audio.Android](https://github.com/ellisnet/CodeBrix.Audio.Android) |
| **Packages** | [`CodeBrix.Audio.Android.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.Android.ApacheLicenseForever) |
| **License** | Apache 2.0; see [License](#license) |
| **Requires** | .NET 10 with the `android` workload. The package targets `net10.0-android36.1`, so it is referenced from an Android application or Android class-library project - a plain `net10.0` project cannot reference it. [`CodeBrix.Audio.Core.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.Core.MitLicenseForever) arrives with it automatically; do not pin Core yourself |
| **Use it from** | An Android application, or the Android head of a CodeBrix.Platform application - directly, or through the Android AudioPlayer and VideoPlayer add-in packages of [CodeBrix.Android](CodeBrix.Android.md), which depend on it |
| **Platforms** | Android 13 (API level 33) and later, on `arm64-v8a` and `x86_64` devices. The native library supports 16 KB memory pages. There is no `armeabi-v7a` or `x86` build |

## What it does

- Installs an Android backend behind CodeBrix.Audio's shared output with one call,
  `CodeBrixAndroidAudio.Initialize(context)`, after which `AudioFilePlayer`, `SoundEffectClip`, the
  sampler and synthesizer players, the recorders and the file writers all run on Android.
- Provides `AndroidAudioEngine` for applications that drive the engine-level API directly: playback,
  capture and full-duplex devices, device enumeration, hot-plug notification, and switching a running
  device to another route while keeping its mixer components and subscriptions.
- Decodes WAV, MP3, FLAC and Ogg Vorbis, and encodes WAV, natively, through the same codec seam the
  desktop package uses; Core's managed codecs and the [CodeBrix.Audio.Opus](CodeBrix.Audio.Opus.md)
  add-on plug in exactly as on the desktop.
- Captures from the microphone once the application holds the `RECORD_AUDIO` permission, and captures
  other applications' playback through an application-owned `MediaProjection`, within Android's
  capture policy.
- Exposes the stream options Android offers through `AndroidDeviceConfig`: exclusive low-latency
  access, buffer size in hardware bursts, playback usage, capture preset, and automatic recovery of a
  disconnected stream on the default route.
- Wraps audio focus in `AndroidAudioFocus`, an explicit request and abandon helper with a
  focus-change event the application reacts to.
- Reports per-device diagnostics through `IAndroidAudioDevice.GetDiagnostics()`: callback count,
  underruns, burst and buffer sizes, the selected device, native and managed errors, and the
  allocations and longest time observed inside callbacks.
- Opens MIDI 1.0 byte-stream input and output ports (USB, virtual and paired Bluetooth devices)
  through the engine's shared MIDI API, with running status, fragmented SysEx and interleaved
  real-time messages handled.
- Turns an Android asset back into a file with `AndroidPackagedAssets`: a file or folder packaged in
  the APK is copied into private storage once per installed build, and its path handed back - the
  bridge between an instrument-library package that delivers a SoundFont, SFZ or Decent Sampler
  folder as files and an APK that holds them only as assets.

## When to use it

Reach for CodeBrix.Audio.Android whenever CodeBrix.Audio has to make sound, or record it, on Android.
It is the Android counterpart of `CodeBrix.Audio.MitLicenseForever`, the desktop package: an
Android application references this package in its place, and never both. A CodeBrix.Platform
application that plays audio through the [AudioPlayer add-in](../platform/add-ins/AudioPlayer.md) or
video through the [VideoPlayer add-in](../platform/add-ins/VideoPlayer.md) gets this package
automatically on its Android head, through the matching CodeBrix.Android add-in package, which
initializes it at start-up - see [Android and Googlebooks](../platform/15-android.md#add-ins-on-android).

The optional add-ons - [CodeBrix.Audio.ModestSynth](CodeBrix.Audio.ModestSynth.md) and
[CodeBrix.Audio.Opus](CodeBrix.Audio.Opus.md) - depend on Core only and work with this package as they
do on the desktop; there is no Android variant of either. The recorded General MIDI instrument library,
[CodeBrix.Audio.Samples.FluidR3Gm](CodeBrix.Audio.Samples.FluidR3Gm.md), works too, once its SoundFont
is packaged as an Android asset and extracted with `AndroidPackagedAssets`.

What the package deliberately leaves to the application:

- It never asks for a permission, shows a consent dialog, requests audio focus on its own, creates a
  foreground service or a notification, or reacts to the activity lifecycle. Those decisions, and
  Android's rules for them, belong to the application.
- It ships no 32-bit ABIs and does not support Android older than API level 33.
- It decodes only WAV, MP3, FLAC and Ogg Vorbis natively and encodes only WAV; Core's managed codecs
  and any add-on registered on the engine extend those lists.
- It does not lock the playback and capture clocks of a full-duplex device together.
- It does not expose MIDI 2.0 (UMP) ports, perform Bluetooth MIDI discovery or pairing, or handle USB
  permission prompts.
- It cannot capture protected audio, calls, or applications that opt out of playback capture -
  Android excludes them before the package sees any audio.
- It does not tell an instrument library where an extracted file is by itself: the application hands
  the path from `AndroidPackagedAssets` to the library's own "use the file at this path" call.

## Getting started

```bash
dotnet add package CodeBrix.Audio.Android.ApacheLicenseForever
```

The package ID carries the license suffix; the namespace does not. You install
`CodeBrix.Audio.Android.ApacheLicenseForever` and you write `using CodeBrix.Audio.Android;`.

A minimal Android application that plays a file is a project file and one activity. The project
targets the same framework as the package, and the minimum OS version is API level 33:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-android36.1</TargetFramework>
    <SupportedOSPlatformVersion>33.0</SupportedOSPlatformVersion>
    <OutputType>Exe</OutputType>
    <ApplicationId>com.example.audiodemo</ApplicationId>
    <RuntimeIdentifiers>android-arm64;android-x64</RuntimeIdentifiers>
    <Nullable>disable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference
      Include="CodeBrix.Audio.Android.ApacheLicenseForever" />
  </ItemGroup>
</Project>
```

```csharp
using Android.App;
using Android.OS;
using CodeBrix.Audio.Android;
using CodeBrix.Audio.Playback;

[Activity(MainLauncher = true)]
public sealed class MainActivity : Activity
{
    private AudioFilePlayer _player;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        CodeBrixAndroidAudio.Initialize(this);   // once per process
        _player = new AudioFilePlayer();
        _player.Load(System.IO.Path.Combine(FilesDir.AbsolutePath,
                                            "clip.wav"));
        _player.Play();
    }

    protected override void OnDestroy()
    {
        _player?.Dispose();
        CodeBrix.Audio.Wave.SharedAudioOutput.Shutdown();
        base.OnDestroy();
    }
}
```

`Initialize` opens no device, asks for no permission, takes no audio focus and starts no service, and
calling it again is harmless. Everything above the engine is the ordinary CodeBrix.Audio API. The
example assumes `clip.wav` already exists in the application's files folder; a real application also
requests focus before playing and stops in `OnStop` when it has no foreground service.

## Key concepts

### The shared output, or an explicit engine

Most applications need nothing beyond `CodeBrixAndroidAudio.Initialize(context)`: it installs an
`AndroidAudioEngine` behind `SharedAudioOutput`, and every player built on the shared output uses it.
Call it once, from `Application.OnCreate` or the first activity's `OnCreate`, before any player is
constructed. Only the application context is retained.

An application that drives the engine-level API constructs `AndroidAudioEngine(context)` itself and
opens devices on it with `InitializePlaybackDevice`, `InitializeCaptureDevice` and
`InitializeFullDuplexDevice`. Passing `null` as the device, or the entry with `Id` 0, means the
system default route. The device lists, `PlaybackDevices` and `CaptureDevices`, each start with a
"System default" entry, and `SupportedDataFormats` is empty for every entry by design: ask for the
format you want and let the stream convert. `SwitchDevice` moves a running device to another route
and returns the replacement - keep the return value, because the old device has been disposed.

Prefer one engine per application. Constructing a second `AndroidAudioEngine` does not configure the
one the shared output already owns.

### Stream options

`AndroidDeviceConfig` is passed as the `config` argument when a device is opened, and every property
is a request that hardware and Android policy may adjust:

| Property | Default | What it asks for |
| --- | --- | --- |
| `PreferExclusive` | `true` | Exclusive, lowest-latency access; Android may grant shared |
| `BufferBursts` | `2` | Buffer size in hardware bursts, 1 to 16. Raise it when diagnostics show underruns |
| `Usage` | `AudioUsageKind.Media` | The Android usage attribute of playback streams |
| `InputPreset` | `AudioSource.Unprocessed` | The capture preset; `VoiceRecognition` or `VoiceCommunication` turn voice processing on |
| `RecoverDisconnectedStreams` | `true` | Reopen a stream whose route vanished on the default route, keeping its mixer components and subscriptions |

Any other `DeviceConfig` subclass is rejected with an `ArgumentException`.

### What the application owns

Audio focus, permissions and the activity lifecycle are application decisions. `AndroidAudioFocus`
prepares a focus request without acquiring it; `Request()` returns whether focus was granted, and a
denied request is not permission to play. `FocusChanged` arrives on the main thread with the new
state, and the helper never touches playback itself. Capture needs the `RECORD_AUDIO` manifest entry
*and* a runtime grant, which the library checks rather than requests. Playback capture additionally
needs the user's `MediaProjection` consent and the foreground service Android requires for it.

### Threading

Audio callbacks run on Android's real-time audio thread: no blocking, no allocation and no Android API
calls there. Start, stop, dispose, switch, seek, diagnostics and opening MIDI ports are control-thread
operations - any thread except an audio callback - and the Android backend's own control methods throw
`InvalidOperationException` when called from a callback. `AudioDevicesChanged` and
`AndroidAudioFocus.FocusChanged` arrive on the main thread.

A managed exception thrown inside an audio callback is caught: the buffer is silenced, the stream stops,
and the exception surfaces through the device's diagnostics and the engine's `LastBackendError`.
Rendering does not resume by itself.

### Packaged sample libraries

An instrument-library package delivers its samples as files and opens them by path. On the desktop
its build targets copy the files beside the executable; in an APK they can only be assets.
`AndroidPackagedAssets.Materialize` and `MaterializeAsync` copy an asset - a file, or a folder with
everything under it - into a private folder the first time, reuse that copy on every later launch, and
re-extract after the application is updated. The copy is written under a temporary name and renamed
into place only when complete, so a cancellation or a crash never leaves a half-written file that
passes as done. An extracted asset exists twice on the device, inside the APK and copied out;
`RemoveAll` reclaims the space.

## Examples

Driving the engine directly, from the package README:

```csharp
using CodeBrix.Audio.Android;
using CodeBrix.Audio.Engine.Structs;

using var engine = new AndroidAudioEngine(applicationContext);

var config = new AndroidDeviceConfig { PreferExclusive = true, BufferBursts = 2 };
using var output = engine.InitializePlaybackDevice(null, AudioFormat.DvdHq, config);
output.MasterMixer.AddComponent(mySoundComponent);   // any CodeBrix.Audio.Engine component
output.Start();
```

Optional codec add-ons register on an explicit engine as well as on the shared output, for example
`CodeBrixAudioOpus.Register(engine)`.

Taking and releasing audio focus around playback:

```csharp
using CodeBrix.Audio.Android;
using Android.Media;

using var focus = new AndroidAudioFocus(applicationContext);
focus.FocusChanged += change => { if (change < AudioFocus.None) output.Stop(); };

if (!focus.Request()) return;   // a denied request is not permission to play
output.Start();
// ... later, when playback ends:
focus.Abandon();
```

Recording from the microphone, once the application holds the runtime grant:

```csharp
using CodeBrix.Audio.Android;
using CodeBrix.Audio.Engine.Structs;

// The application has already obtained the RECORD_AUDIO runtime grant.
using var microphone = engine.InitializeCaptureDevice(null, AudioFormat.DvdHq);
microphone.OnAudioProcessed += (samples, capability) =>
{
    // interleaved 32-bit floats; copy them out - do no blocking work here
};
microphone.Start();
```

Using the recorded General MIDI library on Android. In the application's project file, switch off the
package's copy-to-output delivery and package its assets folder - the SoundFont and its notices - as
Android assets:

```xml
<PropertyGroup>
  <CodeBrixFluidR3GmCopyAssetsToOutput>false</CodeBrixFluidR3GmCopyAssetsToOutput>
</PropertyGroup>
<ItemGroup>
  <PackageReference
    Include="CodeBrix.Audio.Samples.FluidR3Gm.MitLicenseForever"
    GeneratePathProperty="true" />
  <AndroidAsset
    Include="$(PkgCodeBrix_Audio_Samples_FluidR3Gm_MitLicenseForever)/assets/*"
    Link="soundfont/%(Filename)%(Extension)" />
</ItemGroup>
```

Then, after `CodeBrixAndroidAudio.Initialize(context)`, in the application's single asynchronous
preparation task:

```csharp
using System.IO;
using CodeBrix.Audio.Android;
using CodeBrix.Audio.Samples.FluidR3Gm;

var folder = await AndroidPackagedAssets.MaterializeAsync("soundfont");
if (!FluidR3GmInstrumentLibrary.IsLoaded)
    FluidR3GmInstrumentLibrary.UseSoundFontAt(Path.Combine(folder,
        FluidR3GmInstrumentLibrary.SoundFontFileName));
FluidR3GmInstrumentLibrary.Register();
```

Reading a device's diagnostics, from a control or UI thread:

```csharp
using CodeBrix.Audio.Android;

// From a control or UI thread, never from inside a callback.
if (output is IAndroidAudioDevice device)
{
    var d = device.GetDiagnostics();
    Log($"callbacks {d.CallbackCount}, underruns {d.XRunCount}, " +
        $"longest callback {d.MaximumCallbackMicroseconds:F0} us, " +
        $"allocated in callbacks {d.CallbackAllocatedBytes} bytes");
}
```

## Using it in a CodeBrix.Platform application

A CodeBrix.Platform application's Android head is built with [CodeBrix.Android](CodeBrix.Android.md).
When the application uses the AudioPlayer or VideoPlayer add-in, the Android package of that add-in -
`CodeBrix.Android.AudioPlayer.ApacheLicenseForever` or `CodeBrix.Android.VideoPlayer.ApacheLicenseForever` -
brings this package with it and initializes it at start-up, so the application's own code and XAML do
not change. Do not reference the desktop `CodeBrix.Audio.MitLicenseForever` package from the Android
head. Codec add-ons such as Opus are referenced and registered exactly as on the desktop. The
per-add-in notes are in [Android and Googlebooks](../platform/15-android.md#add-ins-on-android).

## Pitfalls

- **Calling `Initialize` too late, or not at all.** Anything that plays through `SharedAudioOutput`
  before it has run tries to open a desktop engine that does not exist on Android.
- **Constructing the desktop engine on Android.** Construct `AndroidAudioEngine`, or use the shared
  output.
- **Control operations inside a callback.** Device `Stop`, `Dispose` and `GetDiagnostics` throw there;
  post transport commands to the main or a control thread, and never seek from a callback.
- **Losing the device after `SwitchDevice`.** It disposes the old device and returns the new one:
  write `output = engine.SwitchDevice(output, info);`.
- **Treating a denied focus request as fine, or handling no focus loss.** A loss with no handler means
  the music keeps playing over a phone call.
- **Forgetting the runtime permission.** The manifest entry is not enough; capture without the grant is
  an `UnauthorizedAccessException`, not silence, and a permission revoked while recording stops the
  device.
- **Expecting a callback exception to resume.** The stream stays stopped until you `Start()` it again.
- **Expecting a recovered stream to keep its route.** Disconnection recovery reopens on the default
  route; subscribe to `AudioDevicesChanged` and `SwitchDevice` to get a specific route back. The native
  diagnostics counters reset on recovery; only the managed ones are lifetime totals.
- **Calling the consent-free loopback overload.** `InitializeLoopbackDevice(format)` always throws on
  Android; only the `MediaProjection` overload works. Stop and dispose that device before stopping the
  projection, which the device never stops for you.
- **MIDI direction.** An Android "output" port is where your application receives from, so it is
  listed under `MidiInputDevices`. Refresh the lists after a hot-plug: a stale `MidiDeviceInfo` is an
  `ArgumentException`.
- **Extracting a sample library on the UI thread.** `Materialize` copies what may be a very large file;
  call it from a background thread or use `MaterializeAsync`, and configure the library's path before
  its first sound.
- **Expecting an instrument package's desktop delivery to work on Android.** Copy-to-output puts nothing
  in an APK; the file has to be an `AndroidAsset`, extracted, and the library told the path.
- **Nothing pauses for you.** Without a foreground service, stop in `OnStop`.
- **Judging latency, routing or capture on an emulator.** None of it is meaningful there; measure a
  Release build on a physical device.

## Samples and tools in the repository

The repository ships one package. Everything below is a sample, developer tooling or test data: none
of it is packed, and none of it is needed to consume the package. Nothing in `tools/` installs anything.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| AudioDiagnostics | A single-activity Android application that exercises the package on a physical device: native-codec and Opus playback, seeking, pause and resume, synthesis workloads, extracting a packaged asset and playing it by path, stepping through output routes, microphone record and replay, audio focus, audio and MIDI enumeration, and a live diagnostics panel. It consumes the package rather than the project, so it verifies the packed artifact | [`samples/AudioDiagnostics`](https://github.com/ellisnet/CodeBrix.Audio.Android/tree/main/samples/AudioDiagnostics) |
| Test project | The codec path against a host build of the same native sources, the MIDI byte parser, and the asset-path and per-install stamp logic behind `AndroidPackagedAssets`; it runs on the development machine, not on Android | [`tests/CodeBrix.Audio.Android.Tests`](https://github.com/ellisnet/CodeBrix.Audio.Android/tree/main/tests/CodeBrix.Audio.Android.Tests) |
| Audio fixtures | Synthetic tones in every format the codec tests decode | [`tests/Assets`](https://github.com/ellisnet/CodeBrix.Audio.Android/tree/main/tests/Assets) |
| Build and verification scripts | Build the native library for both ABIs, rebuild the host variant the tests load, and verify the vendored sources, the native binaries and the packed package | [`tools`](https://github.com/ellisnet/CodeBrix.Audio.Android/tree/main/tools) |

Run the tests with `dotnet test CodeBrix.Audio.Android.slnx`. The codec tests run on Linux x64 and
skip elsewhere; the MIDI parser tests run everywhere.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Audio.Android/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio.Android/blob/main/AGENT-README.txt) |
| The sample, tools and other non-package content | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Audio.Android/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Audio.Android/blob/main/README-INDEX.txt) |
| Provenance and licensing of included open source code | [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio.Android/blob/main/THIRD-PARTY-NOTICES.txt) |
| Tests (worked examples) | [tests/CodeBrix.Audio.Android.Tests](https://github.com/ellisnet/CodeBrix.Audio.Android/tree/main/tests/CodeBrix.Audio.Android.Tests) |
| The companion guide for the engine, device, mixer, component and MIDI types this package plugs into | [CodeBrix.Audio AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/AGENT-README.txt) |

XML documentation ships alongside the assembly.

## License

CodeBrix.Audio.Android is licensed under the Apache License 2.0, and the license is also named in the
package ID (`CodeBrix.Audio.Android.ApacheLicenseForever`). License acceptance is required. Note that
the package it depends on, `CodeBrix.Audio.Core.MitLicenseForever`, is licensed under the MIT License.
For the provenance and licensing of open source code included in this library, including the native
library it builds, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio.Android/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Audio](CodeBrix.Audio.md) - the shared library whose every API this package brings to Android
- [Android and Googlebooks](../platform/15-android.md) - adding an Android head to a CodeBrix.Platform application
- [CodeBrix.Android](CodeBrix.Android.md) - the Android framework and add-in packages, two of which depend on this one
- [CodeBrix.Audio.Samples.FluidR3Gm](CodeBrix.Audio.Samples.FluidR3Gm.md) - recorded General MIDI instruments, packaged for Android as shown above
- [ellisnet/CodeBrix.Audio.Android on GitHub](https://github.com/ellisnet/CodeBrix.Audio.Android) - source, sample, tests and tools
