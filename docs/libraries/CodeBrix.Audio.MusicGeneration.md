<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Audio.MusicGeneration</sub>

# CodeBrix.Audio.MusicGeneration

**CodeBrix.Audio.MusicGeneration produces music - from a piece it carries, or from a music model an
application registers - as MIDI events that arrive while the music is still being written, voices them
through a [CodeBrix.Audio](CodeBrix.Audio.md) instrument library, and either plays them as they appear
or renders them to an audio file.** The music does not stop: when a model reaches the end of what it
was writing, it is asked to carry on at the next bar line, for as long as anybody is listening. You
reach for it from any .NET 10 application, or from a CodeBrix.Platform game through the GameEngine's
GeneratedMusic add-in.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Audio.MusicGeneration](https://github.com/ellisnet/CodeBrix.Audio.MusicGeneration) |
| **Packages** | [`CodeBrix.Audio.MusicGeneration.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MusicGeneration.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later. The package depends on [`CodeBrix.Audio.Core.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.Core.MitLicenseForever), [CodeBrix.Audio.ModestSynth](CodeBrix.Audio.ModestSynth.md) and the ModelRunner library from CodeBrix.Ollama, which it pulls in automatically. To play through a speaker, the application also references the desktop package, [`CodeBrix.Audio.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MitLicenseForever) |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application, through the GameEngine's GeneratedMusic add-in in a game |
| **Platforms** | Windows, Linux and macOS for playback, through the desktop package. Generating music and rendering it to a file need no audio device |

## What it does

- **Music that arrives while it is still being written.** MIDI events are released one at a time,
  each carrying the point the music has settled to, ready to be played or recorded as they appear.
- **Continuous music from a model.** Adapters for the SkyTNT model, which writes MIDI events, and the
  MuPT model, which writes ABC notation, turn what a model writes into music as it is written. Each
  new segment starts at a bar line, and a follow-up prompt takes over at a bar line while the music
  plays.
- **Seams chosen by the application.** Each new segment is primed with the music so far, started
  fresh as a new piece, or the two take turns; a fresh seam can crossfade so the new piece takes over
  from the old one; and a session tempo holds one pulse across fresh pieces a model wrote at tempos of
  their own.
- **A streaming lifecycle built for applications that come first.** A pre-roll, a generate-ahead
  window, diagnostics a game loop can read every frame, and rests rather than garbled timing on a
  machine that cannot keep up.
- **Rendering to a file.** The same music renders offline to an audio file or a stream, in whatever
  format a writer is registered for, optionally to an exact length with a fade, a hard cut or the
  generator's own ending, and hands back the MIDI of what it rendered.
- **Long renders from a MuseCoco model you stage yourself.** A MuseCoco adapter reads model bundles
  the caller stages once with ModelManager; that model writes varied, multi-instrument music from
  musical attributes or a sentence, too slowly to stream, so it is a render job.
- **Music with no model at all.** Pieces of music built into the package play with no model, no
  download and no configuration, and the session always reports that a replay, not a model, is
  playing. An application's own MIDI file, MIDI stream or ABC tune replays the same way, under a name
  of its own.
- **Voicing as data.** A rendition gives each part an instrument and a level, optionally with one
  layer under it; built-in renditions were chosen by listening, and yours sit beside them.
- **One request for every generator.** Free text, notation the generator reads itself, a MIDI primer,
  musical intent (key, mode, meter, tempo, number of parts, drum kit), instrument hints, a seed,
  generation controls and a continuation - turned into what each model really reads.
- **Presets and character words.** Named presets are the requests listening was done on, each with
  the voicing its music was rated through; character words such as "calm" or "driving" name a tempo,
  a mode or both.
- **Refusal by name.** A generator declares what it acts on, and a request that relies on anything
  else is refused before a note is written, never quietly ignored.

## When to use it

Reach for CodeBrix.Audio.MusicGeneration when an application needs background music that never runs
out - under a game, a waiting room or an installation - or a long piece of a known length rendered
ahead of time, and you want the music to come from a model rather than from a recording you ship.
For playing music you already have as a file or a MIDI sequence, [CodeBrix.Audio](CodeBrix.Audio.md)
on its own is the simpler tool.

Choose the model by the music. MuPT writes folk and dance tunes in parts, with no drums, and streams
comfortably on a desktop processor. SkyTNT writes electronica with drums and synthesizers; dense music
can fall behind real time on a small board or a busy server, so measure on the machine you ship to.
MuseCoco is for rendering ahead of time only.

What it does not do: it registers no instrument library and downloads no model, it does not read prose
(free text is refused by both SkyTNT and MuPT), it does not choose sounds from a prompt - what plays a
part is the rendition's business - and it does not promise an inaudible join between two pieces or
gapless music on a machine that generates slower than it plays. It measures that, reports it, and
degrades on purpose. It does not encode audio itself: a render hands its samples to whatever writer
CodeBrix.Audio's registry has for the extension.

## Getting started

```bash
dotnet add package CodeBrix.Audio.MusicGeneration.MitLicenseForever
```

The package depends on CodeBrix.Audio Core, which generates and renders but carries no audio device
backend. A program that plays the music adds the desktop package itself:

```xml
<!-- MusicDemo.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>disable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.Audio.MusicGeneration.MitLicenseForever" />
    <!-- the desktop audio device backend - needed to HEAR it, not to generate or render -->
    <PackageReference Include="CodeBrix.Audio.MitLicenseForever" />
  </ItemGroup>
</Project>
```

```csharp
using CodeBrix.Audio.MusicGeneration;             // MusicSession,
                                                  // MusicGenerationOptions,
                                                  // MusicDiagnostics,
                                                  // ActiveMusicSource,
                                                  // EndOfPiecePolicy,
                                                  // MusicDeliveryMode,
                                                  // SegmentPriming,
                                                  // MusicSegmentKind,
                                                  // IMusicGenerator,
                                                  // MusicGeneratorRegistry,
                                                  // MusicGenerationException
using CodeBrix.Audio.MusicGeneration.Generation;  // MusicRequest and what it
                                                  // is made of - MusicIntent,
                                                  // MusicCharacterWords;
                                                  // GeneratedMusicEvent
using CodeBrix.Audio.MusicGeneration.Replay;      // ReplayMusicGenerator,
                                                  // EmbeddedReplay
using CodeBrix.Audio.MusicGeneration.Rendition;   // MusicRendition,
                                                  // RenditionVoice,
                                                  // MusicRenditionRegistry,
                                                  // BuiltInRenditions
using CodeBrix.Audio.MusicGeneration.Rendering;   // MusicRenderOptions,
                                                  // RenderEnding, MusicFade,
                                                  // MusicRenderResult
using CodeBrix.Audio.MusicGeneration.Models;      // MuPTMusicGenerator,
                                                  // SkyTNTMusicGenerator and
                                                  // their options
using CodeBrix.Audio.MusicGeneration.Presets;     // MuPTPresets,
                                                  // SkyTNTPresets,
                                                  // MusicPreset

using CodeBrix.Audio.ModestSynth;                 // GeneralMidiInstrumentLibrary
using CodeBrix.Audio.Midi;                        // the MIDI events themselves
using CodeBrix.Audio.Instruments;                 // InstrumentLibraryRegistry
```

Registering an instrument library is the one thing you must do: this library registers none, because
which instruments an application uses is the application's decision. The General MIDI library comes
with the dependencies and is one line. Then a session plays the music that comes in the package:

```csharp
using CodeBrix.Audio.ModestSynth;
using CodeBrix.Audio.MusicGeneration;

GeneralMidiInstrumentLibrary.Register();        //your decision, always

using var music = new MusicSession();           //nothing specified
music.Play();

Console.WriteLine(music.ActiveSource);          //what is really playing
Console.ReadLine();
```

`ActiveSource` says outright that an embedded replay is playing - a recording of what a model once
wrote, the same piece every time - so no application ships believing a model is playing when it is
not. Name a generator on the options and a model plays instead.

## Key concepts

### Generators, and registering is not specifying

A generator is "a thing that produces music", not a model file: it may be backed by no model, by one,
or by two. The built-in replays are `EmbeddedReplay.Midi` (the default), `EmbeddedReplay.MidiSecond`,
`EmbeddedReplay.Abc` and `EmbeddedReplay.AbcSecond`. Registering a generator with
`MusicGeneratorRegistry` makes its name resolvable and loads nothing; the session plays what its
options **name**, and with nothing named the embedded replay plays, however many generators are
registered. `Generator`, `InstrumentLibrary` and `Rendition` are the three strings that do almost all
of the work.

### The model adapters and the model packages

An adapter is code and a model is files. The SkyTNT and MuPT adapters are in this package and take a
path you supply: a folder, or a map of file names against the paths they are really at, for SkyTNT,
and a single file for MuPT. The two models also come as packages of their own that register a shared
instance with one call, and the recorded General MIDI instrument set, FluidR3Gm, is a package of its own
too. The blueprint file registers everything an application might ask for, once, early:

```csharp
using CodeBrix.Audio.ModestSynth;
using CodeBrix.Audio.MusicGeneration;
using CodeBrix.Audio.MusicGeneration.MuPT;
using CodeBrix.Audio.MusicGeneration.Rendition;
using CodeBrix.Audio.MusicGeneration.SkyTNT;
using CodeBrix.Audio.Samples.FluidR3Gm;

public static class MusicSetup
{
    // Call once at start-up. Registering makes a name resolvable; it loads nothing
    // and it chooses nothing.
    public static void RegisterEverything()
    {
        GeneralMidiInstrumentLibrary.Register();    // "ModestSynthGm": synthesized, nothing to ship
        FluidR3GmInstrumentLibrary.Register();      // "FluidR3Gm": recorded instruments, a large package
        SkyTNTModel.Register();                     // "SkyTNT": writes MIDI events
        MuPTModel.Register();                       // "MuPT": writes ABC notation
    }

    // The three strings that decide what is heard. Name all three.
    public static MusicGenerationOptions CreateOptions(string generator, string instrumentLibrary) =>
        new MusicGenerationOptions
        {
            Generator = generator,                      // SkyTNTModel.GeneratorName or MuPTModel.GeneratorName
            InstrumentLibrary = instrumentLibrary,      // GeneralMidiInstrumentLibrary.LibraryName or FluidR3GmInstrumentLibrary.LibraryName
            Rendition = BuiltInRenditions.Automatic,    // or a preset's suggested rendition
        };
}
```

A model loads on first use; `PreloadAsync` moves that cost to a loading screen. Once loaded it stays
loaded in the registry and serves every later session: disposing a session releases nothing, and
`Release()` gives the memory back. No Python and no ModelManager run in the application.

### Instrument libraries

An instrument library is a named set of instruments addressed the way MIDI addresses them. `ModestSynthGm`
is the whole General MIDI set, synthesized, with nothing to ship; any `.sf2` becomes a named library; the
FluidR3Gm package gives recorded instruments at the cost of a larger download; and a mapped library
replaces one voice at a time. The first library registered is the default for a session that names
none, so name the library whenever more than one is registered.

### Renditions

A rendition says how a piece is voiced: an ordered list of voices, each an instrument and its level,
optionally with one layer under it, plus a percussion gain and a master gain. Voices go to parts in the
order the parts first sound. `BuiltInRenditions` carries `Automatic`, `AmbientDuet`, `MelodyOverPad`,
`VibesAndStrings`, `HarpAndCello` and `Neutral`. `ActiveSource.Voicing` says, part by part, what each
part plays, at what gain, and why.

### Requests, presets and refusal by name

`MusicGenerationOptions.Request` says what the music should be. `MusicIntent` carries a key, mode,
meter, tempo, number of parts and drum kit, and each model turns it into what it reads: an ABC header
and a two-part opening for MuPT, a key signature, tempo and drum kit for SkyTNT. A part of a request the
chosen generator does not act on is refused by name before anything starts - MuPT refuses a drum kit
and instrument hints, SkyTNT refuses a voice count and a unit note length, and both refuse free text.
`MuPTPresets` (reels, jigs, hornpipes, airs, waltzes and two duets) and `SkyTNTPresets`
(`AmbientElectronica`, `ClubArrangement`, `FourOnTheFloor`) are requests to start from; a preset names no
generator, and `CreateRequest()` hands back a new request every call.

### Segments, seams and one pulse

A segment is one pass of the model. When a pass ends, the same model is asked again with a new derived
seed, and the next segment starts on the next bar line with the tempo, meter, key and each part's
instrument carried across. `SegmentPriming` decides how it starts: `Primed` (the default) shows the
model the last bars it wrote, `Fresh` asks for a new piece in the same character, and `Alternate` takes
turns. `SeamCrossfade` overlaps the outgoing and incoming pieces at a fresh seam, and is shortened
rather than ever costing the music a gap. `TempoPolicy` holds a session tempo: `Carry` plays every fresh
piece at it, and `CarryOutsideBand` carries only pieces further than `TempoBand` (15% by default) away.
Only a run of three empty or failed passes in a row stops the music, with the reason in
`GenerationError`.

### The streaming lifecycle and its diagnostics

The play head waits for `Preroll` of settled music (about five seconds by default) before it moves, and
generation stops when `GenerateAhead` (thirty seconds by default) is waiting in front of it. That wait
is silence on purpose: the application comes first and the music waits. On a machine that generates
slower than real time the engine switches to delivering whole segments with rests between them, and
back again when the rate recovers. `Diagnostics` is a cheap snapshot to read every frame -
`StarvationGapCount`, `RealTimeFactor` (null until measured, never "slow"), `Mode`, `Lead`,
`SegmentCount` and the crossfade and tempo counts. A model takes a quarter of the processors by
default, at least one and at most four; `InferenceThreadCount` changes it.

### Rendering to a file

`RenderToFileAsync` and `RenderToStreamAsync` take the same options as playback and add
`MusicRenderOptions`: the file's extension picks the writer through CodeBrix.Audio's registry, a
`TargetLength` gives an exact length, and `RenderEnding` chooses `Fade`, `HardCut` or `NaturalStop`. A
render opens no audio device, needs no `Play()`, and returns a `MusicRenderResult` whose `Music` is the
MIDI of what was rendered. A canceled or failed render deletes the file it created.

## Examples

Rendering the same music to a file instead, with a length and a fade, from the README:

```csharp
using CodeBrix.Audio.ModestSynth;
using CodeBrix.Audio.MusicGeneration;
using CodeBrix.Audio.MusicGeneration.Rendering;

GeneralMidiInstrumentLibrary.Register();

using var music = new MusicSession();

var result = await music.RenderToFileAsync("theme.wav",
    new MusicRenderOptions { TargetLength = TimeSpan.FromMinutes(6.0) },
    CancellationToken.None);

Console.WriteLine(result);                      //the file, its exact length
```

Replaying an application's own music under its own name, voiced by a built-in rendition:

```csharp
using CodeBrix.Audio.MusicGeneration;
using CodeBrix.Audio.MusicGeneration.Replay;

var ownTune = ReplayMusicGenerator.FromMidiFile("MyTheme", "theme.mid");
ownTune.PacingRate = 1.0;                       //released at the speed it plays
MusicGeneratorRegistry.Register(ownTune);

using var music = new MusicSession(new MusicGenerationOptions
{
    Generator = "MyTheme",                      //registering is not specifying
    Rendition = "AmbientDuet",                  //how the parts are voiced
});
```

Priming and crossfading the seams of a model the application registered - fresh and primed segments
taking turns, with a four-second crossfade at every fresh seam:

```csharp
using var music = new MusicSession(new MusicGenerationOptions
{
    Generator = "MyModel",                      //a model generator the application registered
    SegmentPriming = SegmentPriming.Alternate,  //primed, fresh, primed, fresh...
    SeamCrossfade = TimeSpan.FromSeconds(4.0),  //fresh seams only; zero is a hard join
});
music.Play();

Console.WriteLine(music.Diagnostics);           //what is generating, and the crossfades so far
```

When a game or a mixer already owns the audio device, the session opens none and your mixer pulls
stereo samples from it:

```csharp
using CodeBrix.Audio.ModestSynth;
using CodeBrix.Audio.MusicGeneration;

GeneralMidiInstrumentLibrary.Register();

using var music = new MusicSession(new MusicGenerationOptions
{
    ApplicationOwnsAudioOutput = true,
    SampleRate                 = 48000,     // the rate YOUR mixer pulls at
    MasterVolume               = 0.8F,
});
music.Play();                               // no device is opened

var left = new float[1024];
var right = new float[1024];

// ... in your own mixer callback, for as long as you want music:
music.Renderer.Render(left, right);         // one block of stereo samples
```

The play head moves only as audio is pulled, so a mixer that stops pulling stops the music's clock,
exactly as a paused game should.

## Using it in a CodeBrix.Platform application

A CodeBrix.Platform game reaches this library through the GeneratedMusic add-in of
[CodeBrix.Platform.GameEngine](CodeBrix.Platform.GameEngine.md), the engine's streaming-music provider
built on this library. The game still makes the two decisions this
library never makes for it - which instrument library and which model to register - one `Register()`
line each.

[BrixInvaders](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BrixInvaders/README.md) in
CodeBrix.Samples is the complete reference: endless generated music through the add-in, a music policy
behind an interface the tests fake, a duck under the pause menu, a per-sector table of what plays, and
settings that choose the model and the instrument library.

## Pitfalls

- **No sound is not a bug.** This library registers no instrument library; call
  `GeneralMidiInstrumentLibrary.Register()`, or register another, before you expect anything audible.
- **Playing with only Core on the machine.** Core has no audio device backend. Rendering to a file works
  without one; opening the audio device needs the desktop package, `CodeBrix.Audio.MitLicenseForever`,
  referenced from the application.
- **Expecting a registered generator to play.** Registering is not specifying: ask for your generator by
  name through `MusicGenerationOptions.Generator`.
- **Letting registration order choose the instruments.** The first library registered is the default, so
  an application with two start-up paths can sound different depending on which ran first.
- **Reading `Renderer` before `Play()`.** It does not exist until the music starts.
- **Expecting the music to stop when the piece ends.** The generator is asked to carry on; set
  `EndOfPiece = EndOfPiecePolicy.Stop` for music that ends.
- **Expecting a follow-up to be heard at once.** It takes over at a safe bar line after its own pre-roll
  is ready, and `ActiveSource` changes at the switch, not at the call.
- **Treating a starvation gap as a fault.** Act on a `RealTimeFactor` that stays below one, not on a
  single gap, and test it against a number rather than for zero, because it is null until measured.
- **Expecting `Dispose()` to free a model.** A loaded generator belongs to the registry; call
  `Release()` when you want the memory back.
- **Reading the size of a model package as its memory use.** Inference can take far more than the files
  on disk; measure peak memory over a long run on the machine you ship to.
- **Running a render beside a live session.** A render takes everything the processors can give, and a
  playing session on the same machine falls back to whole segments.
- **Rendering to `.opus` without the encoder.** This package never references
  [CodeBrix.Audio.Opus](CodeBrix.Audio.Opus.md); the application references it and calls
  `CodeBrixAudioOpus.Register()`.
- **Handing a WAV render a stream that cannot seek.** A WAV patches its header at the end.
- **Asking a notation model for drums.** ABC has no percussion, so MuPT refuses a drum kit by name; the
  SkyTNT adapter is the one that takes a kit.

## Samples and tools in the repository

The repository ships one package and no sample applications, demos or command-line tools. The
blueprint file is where the worked recipes are: every code block in it is a whole C# file, complete
enough to compile against the packages the text names. The recipes are listed on the site under
[The music-generation blueprints](../samples/blueprints.md#the-music-generation-blueprints), and the file
itself is
[BLUEPRINTS-GeneratingMusic.md](https://github.com/ellisnet/CodeBrix.Audio.MusicGeneration/blob/main/BLUEPRINTS-GeneratingMusic.md).

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Test project | The library's first consumer: it registers an instrument library and the Opus encoder exactly as an application would, and exercises every feature area | [`tests/CodeBrix.Audio.MusicGeneration.Tests`](https://github.com/ellisnet/CodeBrix.Audio.MusicGeneration/tree/main/tests/CodeBrix.Audio.MusicGeneration.Tests) |
| Embedded music | The pieces compiled into the package that the built-in replays play | [`src/CodeBrix.Audio.MusicGeneration/EmbeddedMusic`](https://github.com/ellisnet/CodeBrix.Audio.MusicGeneration/tree/main/src/CodeBrix.Audio.MusicGeneration/EmbeddedMusic) |

Tests that make sound, hold the audio device, need a caller-staged model or take minutes are opt-in
behind environment variables, listed in
[MAINTAINER-README.txt](https://github.com/ellisnet/CodeBrix.Audio.MusicGeneration/blob/main/MAINTAINER-README.txt);
an ordinary `dotnet test` skips them and says why.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Audio.MusicGeneration/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio.MusicGeneration/blob/main/AGENT-README.txt) |
| How-to recipes, with compiled code | [BLUEPRINTS-GeneratingMusic.md](https://github.com/ellisnet/CodeBrix.Audio.MusicGeneration/blob/main/BLUEPRINTS-GeneratingMusic.md) |
| Tests and other non-package content | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Audio.MusicGeneration/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Audio.MusicGeneration/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.Audio.MusicGeneration.Tests](https://github.com/ellisnet/CodeBrix.Audio.MusicGeneration/tree/main/tests/CodeBrix.Audio.MusicGeneration.Tests) |

XML documentation ships alongside the assembly.

## License

CodeBrix.Audio.MusicGeneration is licensed under the MIT License; the license is also named in the
package ID (`CodeBrix.Audio.MusicGeneration.MitLicenseForever`). Model files are never in the package:
they are external assets the application obtains and deploys. For the provenance and licensing of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio.MusicGeneration/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Audio](CodeBrix.Audio.md) - the library this builds on: instrument libraries, MIDI and the players
- [CodeBrix.Platform.GameEngine](CodeBrix.Platform.GameEngine.md) - endless generated music in a game, through the GeneratedMusic add-in
- [The music-generation blueprints](../samples/blueprints.md#the-music-generation-blueprints) - every recipe, by task
- [ellisnet/CodeBrix.Audio.MusicGeneration on GitHub](https://github.com/ellisnet/CodeBrix.Audio.MusicGeneration) - source, tests and blueprints
