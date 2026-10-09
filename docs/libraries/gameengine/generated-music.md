<sub>[CodeBrix](../../../README.md) › [Libraries](../README.md) › [CodeBrix.Platform.GameEngine](../CodeBrix.Platform.GameEngine.md) › Generated music</sub>

# Generated music

**The generated music add-on gives a [CodeBrix.Platform.GameEngine](../CodeBrix.Platform.GameEngine.md)
game endless music that an AI model writes while the game runs, with one call: the music fades in, never
ends, never loops audibly, and changes character for a new level or a boss at the next bar line.** It is
the bridge between the engine and [CodeBrix.Audio.MusicGeneration](../CodeBrix.Audio.MusicGeneration.md),
where a model writes MIDI and an instrument library plays it. The music plays on the engine's own music
bus, so fades, crossfades, ducking, stingers, the music volume slider and the global engine pause all
work on it exactly as on a music file, and it opens no audio device of its own.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Platform.GameEngine](https://github.com/ellisnet/CodeBrix.Platform.GameEngine) |
| **Package** | [`CodeBrix.Platform.GameEngine.GeneratedMusic.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.GameEngine.GeneratedMusic.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later. The engine package and [`CodeBrix.Audio.MusicGeneration.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MusicGeneration.MitLicenseForever) arrive automatically, bringing CodeBrix.Audio, the synthesized General MIDI instrument library and the model runner. A model package is the game's choice |
| **Use it from** | A CodeBrix.Platform game, in either hosting mode |
| **Platforms** | Every platform the engine runs on: Windows, macOS, Linux and the Linux frame buffer. No Python, no download at run time and nothing to install on the target machine |

## What it does

- Starts endless generated music with one call, `Engine.Instance.UseGeneratedMusic(options)`, which
  registers a provider as the engine's streaming music provider and plays it through `MusicManager` as an
  ordinary `StreamingMusicTrack`.
- Chooses what plays from `GeneratedMusicOptions`, every property optional: the generator (the model),
  the instrument library, a preset or character words, a tempo held for the whole session, a seed that
  writes the same music again, the crossfade where the music moves on to a fresh piece, the music's own
  level and the fade-in.
- Moves the music on while it plays: `FollowUp` asks the same generator for new music - a preset,
  character words such as `"dark driving"`, or new options with a seed and a tempo - which takes over at
  a bar line once it is ready, so the music never stops.
- Switches the model, the instrument library or any session setting when `UseGeneratedMusic` is called
  again; the earlier provider is stopped, unregistered and disposed.
- Degrades instead of failing: with no model registered a recorded piece that ships inside the music
  package plays in its place and the engine log says so in one line naming the fix; with no instrument
  library there is silence, a faulted state naming the fix, and the game runs on.
- Reports what is really playing (`ActiveSource`, `ActiveSourceSummary`), how it is doing (`State`,
  `Diagnostics`, `Fault`, `GenerationError`) and raises `StateChanged` on every change.
- Gives a game's music policy test seams, so it is unit-tested with scripted fakes and no model or audio
  device: the provider is an `IGeneratedMusicSession`, starting one goes through
  `IGeneratedMusicStarter`, and the engine's `MusicManager` is an `IMusicManager`.

## When to use it

Reach for it when a game should have music from the first screen to the last without anyone composing,
recording or shipping a soundtrack, and when that music should follow the game - a calmer menu, a new
level, a boss. It is also the first implementation of the engine's streaming-music provider contract, so
it is the worked example for a provider of your own; [Music](../CodeBrix.Platform.GameEngine.md#music) on
the engine page describes that seam.

Title music decided in advance is a different job: render it to a file ahead of time with
`MusicSession.RenderToFileAsync` from CodeBrix.Audio.MusicGeneration and play the file as a
`FileMusicTrack`. This package does not render to a file.

What it deliberately does not do: it registers no instrument library and no generator, ever; it carries
no model and no instruments, which are their own packages; it opens no audio device; and it does not
promise gapless music on a machine that composes more slowly than it plays - there the music waits, in
whole bars, and `Diagnostics` says so.

## Getting started

Add the add-on, a model and - when the game wants recorded instruments instead of synthesized ones - a
sampled instrument library, to the shared game library:

```bash
dotnet add package CodeBrix.Platform.GameEngine.GeneratedMusic.MitLicenseForever
dotnet add package CodeBrix.Audio.MusicGeneration.SkyTNT.ApacheLicenseForever   # electronica, with drums
dotnet add package CodeBrix.Audio.MusicGeneration.MuPT.ApacheLicenseForever     # folk and classical, no drums
dotnet add package CodeBrix.Audio.Samples.FluidR3Gm.MitLicenseForever           # optional: recorded GM instruments
```

At start-up, before the music starts: pin the engine's audio format, register an instrument library and a
model, then make the one call. Every `Register()` is idempotent and loads nothing; a model loads in the
background when the music first asks for it, and the game never waits for it.

```csharp
// From src/CodeBrix.Platform.GameEngine.GeneratedMusic/AGENT-README.txt
using CodeBrix.Audio.ModestSynth;                    // the instruments
using CodeBrix.Audio.MusicGeneration.SkyTNT;         // a model
using CodeBrix.Platform.GameEngine.Audio;            // AudioSystem, MusicManager
using CodeBrix.Platform.GameEngine.GeneratedMusic;   // this package

AudioSystem.Initialize(48000, 2);               // the engine's output format
GeneralMidiInstrumentLibrary.Register();        // "ModestSynthGm"
SkyTNTModel.Register();                         // "SkyTNT"

GeneratedMusicProvider music = Engine.Instance.UseGeneratedMusic(
    new GeneratedMusicOptions
    {
        Generator         = SkyTNTModel.GeneratorName,   // optional
        InstrumentLibrary = "ModestSynthGm",             // name it!
        Preset            = "ClubArrangement",
    });

// Later - a new level, at the next bar line:
music.FollowUp("FourOnTheFloor");
```

In a `CodeBrixGameHost`, the natural places are the `OnInitializing` override for
`AudioSystem.Initialize`, so sound effects preload at the same rate, and the `OnEngineStarted` override
for the call that starts the music; that is where
[BrixInvaders](https://github.com/ellisnet/CodeBrix.Samples/tree/main/BrixInvaders) makes them.

The namespaces a game reaches for:

```csharp
using CodeBrix.Platform.GameEngine.GeneratedMusic;  // UseGeneratedMusic, GeneratedMusicOptions,
                                                    //   GeneratedMusicProvider,
                                                    //   IGeneratedMusicSession,
                                                    //   IGeneratedMusicStarter,
                                                    //   EngineGeneratedMusicStarter,
                                                    //   GeneratedMusicSourceInfo
using CodeBrix.Platform.GameEngine.Audio;           // MusicManager, StreamingMusicState,
                                                    //   StreamingMusicTrack
using CodeBrix.Audio.ModestSynth;                   // GeneralMidiInstrumentLibrary
using CodeBrix.Audio.MusicGeneration;               // ActiveMusicSource, MusicDiagnostics,
                                                    //   SegmentPriming, MusicGenerationException
using CodeBrix.Audio.MusicGeneration.Presets;       // SkyTNTPresets, MuPTPresets
```

## Key concepts

### What `UseGeneratedMusic` does

It creates a `GeneratedMusicProvider` with a copy of the options, registers it as the engine's streaming
music provider (`Engine.Instance.Managers.StreamingMusic`), plays it through `MusicManager.Instance` as a
`StreamingMusicTrack` keyed `TrackKey` with a fade over `FadeIn` when `StartImmediately` is on (the
default), and returns the provider for follow-ups, diagnostics and state. The music renders at the
sample rate `AudioSystem.Initialize` pinned. While the model loads and writes its first bars the engine
plays silence, which is normal and never an error.

To register the music without playing it - a title screen that is silent until the player presses a key -
set `StartImmediately = false` and call `MusicManager.Instance.PlayStreaming(fadeIn)` later.
`PlayStreaming` is idempotent while the music streams, so calling it on every screen change keeps the
music going rather than restarting it. Everything else - `Stop(fadeOut)`, pausing, `PushDuck` under
dialogue, `PlayStinger` for a jingle, the music slider and the global pause - is the engine's music
system, unchanged.

### `GeneratedMusicOptions`

Names are resolved when the music starts, not when the options are built.

| Property | Default | Meaning |
| --- | --- | --- |
| `Generator` | null | The registered generator to play. Null means the first registered generator that is not a built-in replay, and the embedded replay when there is none |
| `InstrumentLibrary` | null | The registered instrument library. Null means the registry's default, which is the first one registered |
| `Preset` | null | A preset of the generator's family; its suggested voicing comes with it |
| `Text` | null | Character words; used instead of `Preset` when set |
| `BeatsPerMinute` | null | Holds the whole session at this tempo; it is never sent to the generator, so it works with any generator |
| `Seed` | null | The same options write the same music again |
| `SeamCrossfade` | 4 seconds | The overlap where the music moves on to a fresh piece; `TimeSpan.Zero` cuts at a bar line |
| `SegmentPriming` | null | `Primed`, `Fresh` or `Alternate`; null means what suits the generator's family |
| `MasterVolume` | 1 | The music's own level, under the engine's music bus, ducking and master volume |
| `StartImmediately` | true | Also play it now through `MusicManager` |
| `TrackKey` | `"generated-music"` | The music track's key |
| `FadeIn` | 1.5 seconds | The fade-in `StartImmediately` uses |

`Clone()` makes an independent copy, and `DefaultTrackKey`, `DefaultFadeIn` and `DefaultSeamCrossfade`
are the constants behind three of the defaults.

### Which music plays

**The generator.** Left null, the first registered model plays, so a game that registered one model
package hears it without naming it. Name the generator whenever more than one model is registered.

**No model means the embedded replay.** With no model registered and none named, a recording of what a
model once wrote plays, the same piece every time, looping. It is the expected degraded path, not an
error; one information line in the engine log says so and names the fix, the model-only options
(`Preset`, `Text`, `Seed`) are left out, and `provider.ActiveSource.IsReplay` is true.

**The instrument library.** Left null, the registry's default plays, and the default is the first library
registered - there is no priority. Name it (`"ModestSynthGm"` or `"FluidR3Gm"`) whenever more than one is
registered, or move the default with `InstrumentLibraryRegistry.SetDefault(name)`.

**No library means silence and a fault.** The provider reports `StreamingMusicState.Faulted` with a
`Fault` naming `GeneralMidiInstrumentLibrary.Register()`, the engine log carries the same line, and the
game keeps running in silence.

### Presets and character words

A preset is a ready-made request written for one generator family, matched without regard to case.

| Family | Presets |
| --- | --- |
| SkyTNT - electronica, with drums unless the preset says otherwise | `FourOnTheFloor`, `ClubArrangement`, `AmbientElectronica` |
| MuPT - folk and classical tunes in parts, with no drums at all | `ReelInGMinor`, `JigInD`, `WaltzInAMinor`, `AirInDMixolydian`, `HornpipeInG`, `OpenInC`, `DuetInC`, `WaltzDuetInAMinor` (the two duets write two-part music) |

`SkyTNTPresets.All` and `MuPTPresets.All` list them at run time. A name that is no preset of any family is
refused at once by `UseGeneratedMusic` with an `ArgumentException` listing every preset; a preset of the
other family faults the music when it starts.

`Text` holds character words instead - `"dark"`, `"driving"`, `"calm, minor"`, `"heroic"` - which both
model families turn into a tempo, a mode or both; the vocabulary is
`CodeBrix.Audio.MusicGeneration.Generation.MusicCharacterWords.Known`. Neither model reads prose: free text
is refused by name, which faults the music at start-up (never the game) and throws from a follow-up while
the music plays on.

### Changing the music while the game runs

Two ways, for two different needs.

**Same generator, new music** - a new level, a boss, a calmer menu: `provider.FollowUp(...)`, with a
preset or character words, or with `GeneratedMusicOptions` carrying `Generator`, `Preset`, `Text`, `Seed`
and `BeatsPerMinute` (which, here, is asked of the generator). The new music is generated alongside what is
playing and takes over at a bar line once it has its own pre-roll; it is not instant, and there is no
fixed latency. A follow-up made while the music is still starting is kept and made as soon as it plays.
It keeps the session's instrument library, volume, seam and priming. `FollowUp` throws, and the music
carries on as it was, for blank words or a preset that does not fit, when the music is not started or has
faulted, when a named generator is not registered, or when the generator refuses the request; a follow-up
that fails later, inside the generator, lands in `GenerationError`.

**A different model, library or session setting** - a settings screen: call `UseGeneratedMusic` again
with the new options. The new music starts from its own beginning, with its own fade-in.

### States

`provider.State`, which the `StreamingMusicTrack` playing it reports too, is one of:

| State | Meaning |
| --- | --- |
| `Stopped` | Not running: before `Start`, after `Stop`, after `Dispose` |
| `Starting` | Loading the model and writing the first bars; the engine plays silence. Normal, and longer on a slow machine |
| `Playing` | Music is being heard |
| `Starved` | The generator fell behind and the music waits; it returns to `Playing` by itself. Normal, never a failure: the game comes first. On a machine that cannot compose as fast as it plays, the music switches to whole phrases with rests between them, which sounds intentional |
| `Faulted` | The music cannot play, and `provider.Fault` says why. Never thrown into the engine; the track plays silence until the game stops it or calls `UseGeneratedMusic` again |

`StateChanged` is raised after every change on any thread, the engine's audio fill thread included. Keep
handlers quick and marshal to the game with `Engine.Instance.EngineDispatcher.Post` before touching game
state. The engine also logs every change.

### Ownership, threads and shipping

The provider `UseGeneratedMusic` created is disposed by the next `UseGeneratedMusic` call or when the engine
is disposed; the game never disposes it. `Release()` gives a model's memory back while it stays registered,
and is refused while the music is starting or playing. `FollowUp`, `Release` and `Dispose` may block
briefly while the session changes over, so call them from game code, never from a `StateChanged` handler
on the audio thread. Each model takes a conservative share of the processors by default, because
inference shares the machine with the game loop.

SkyTNT runs fully managed. MuPT runs on the model runner's bundled native engine, carried in the standard
`runtimes/<rid>/native` layout for `win-x64`, `win-arm64`, `osx-x64`, `osx-arm64`, `linux-x64`,
`linux-arm64` and `linux-riscv64`; keep the native library for the target when trimming or publishing
self-contained. The model packages and the FluidR3Gm package copy their files - model weights, the
SoundFont - into the build and publish output beside the executable, so deploy those folders with the
game. The size of what ships is the game's decision: the model and SoundFont packages are large, while the
synthesized library and the embedded replay cost almost nothing. A loaded model costs working memory well
beyond its file size, so measure on the hardware you ship to.

## Examples

Start the music registered but silent, and begin it when the player leaves the title screen:

```csharp
// From src/CodeBrix.Platform.GameEngine.GeneratedMusic/AGENT-README.txt
Engine.Instance.UseGeneratedMusic(new GeneratedMusicOptions
{
    Preset = "AmbientElectronica",
    StartImmediately = false,              // registered, not playing
});

// ... later:
MusicManager.Instance.PlayStreaming(TimeSpan.FromSeconds(2));
```

Follow up for each level, in the three forms:

```csharp
// From src/CodeBrix.Platform.GameEngine.GeneratedMusic/AGENT-README.txt
music.FollowUp("FourOnTheFloor");          // a preset of the playing
                                           // generator's family
music.FollowUp("dark driving");            // character words
music.FollowUp(new GeneratedMusicOptions   // preset + seed + tempo, or
{                                          // another generator
    Preset = "ClubArrangement",
    Seed = level,
    BeatsPerMinute = 132,                  // ASKED OF THE GENERATOR here
});
```

Switch the model and the instruments from a settings screen:

```csharp
// From src/CodeBrix.Platform.GameEngine.GeneratedMusic/AGENT-README.txt
Engine.Instance.UseGeneratedMusic(new GeneratedMusicOptions
{
    Generator = MuPTModel.GeneratorName,
    InstrumentLibrary = "FluidR3Gm",
    Preset = "JigInD",
});
```

Put the game's music policy behind the session seams, so the game hands it the real engine objects and a
test hands it a fake starter returning a scripted session, a recording `IMusicManager`, and an
`IEngineDispatcher` that runs the action at once:

```csharp
// From src/CodeBrix.Platform.GameEngine.GeneratedMusic/AGENT-README.txt
public sealed class MusicDirector(IGeneratedMusicStarter starter, IMusicManager music,
                                  IEngineDispatcher dispatcher)
{
    private IGeneratedMusicSession? _session;

    public void Start(GeneratedMusicOptions options)
    {
        var session = starter.Start(options);
        _session = session;
        //StateChanged arrives on any thread: marshal before touching game state
        session.StateChanged += (_, _) => dispatcher.Post(() => Log(session.State));
    }

    public void OnLevel(string preset) => _session?.FollowUp(preset);
}

//In the game:
new MusicDirector(new EngineGeneratedMusicStarter(), MusicManager.Instance,
                  Engine.Instance.EngineDispatcher);
```

`new EngineGeneratedMusicStarter()` reads `Engine.Instance` when `Start` is called, so it can be created
before the engine starts; `new EngineGeneratedMusicStarter(engine)` pins one. Everything on
`IGeneratedMusicSession` is a plain value - strings, counts and `GeneratedMusicSourceInfo` - so a fake can
script all of it.

## Pitfalls

- **No instrument library registered means silence.** This package, the music package and the model
  packages register none. Call `GeneralMidiInstrumentLibrary.Register()` at start-up.
- **Registration order chooses the instruments.** The first library registered is the default. Name
  `InstrumentLibrary` when more than one is registered.
- **No model registered means the replay.** Check `provider.ActiveSource.IsReplay`, or the one log line,
  before believing a model is playing, and log whether each model's files were found so a silent fallback
  is visible.
- **MuPT writes no drums.** Its notation has no percussion; pick a SkyTNT preset for drums.
- **Neither model reads prose.** Use a preset or character words.
- **`Starting` and `Starved` are not failures.** Only `Faulted` is, and it never stops the game.
- **A follow-up is not heard at once.** It takes over at a bar line once its music is ready, so ask for
  it early - at a level briefing or a boss warning - not at the moment it should be heard.
- **`FollowUp` cannot change the model or the library.** Call `UseGeneratedMusic` again for that.
- **`Release()` is refused while the music plays.** Stop it, or dispose the provider, first.
- **One provider is one stream.** A second track over the same provider restarts it; crossfading
  generated music with a file or a MIDI track works as usual.
- **Pin the audio format.** Call `AudioSystem.Initialize(rate, channels)` at start-up; the music renders
  at whatever the engine's output runs at.
- **`StateChanged` can arrive on the audio thread.** Post to the engine dispatcher before touching game
  state.
- **Apply the player's music level once.** Drive the engine's music bus (`AudioMixer.MusicVolume`) or the
  session's `MasterVolume`, not both, or the two multiply.
- **A library that never loads the models can switch off their file copy.** The model and SoundFont
  packages copy their files into the output of every project that reaches them; a library that only
  references them sets `CodeBrixSkyTNTCopyAssetsToOutput`, `CodeBrixMuPTCopyAssetsToOutput` or
  `CodeBrixFluidR3GmCopyAssetsToOutput` to false, and the application heads still receive the files.

## Samples and reference applications

No sample in the engine repository plays generated music; the package's tests are its headless worked
examples. The complete reference is
[BrixInvaders](https://github.com/ellisnet/CodeBrix.Samples/tree/main/BrixInvaders) in
[CodeBrix.Samples](https://github.com/ellisnet/CodeBrix.Samples): music from the first screen to the last
through this add-on, both model packages and both instrument libraries registered, a per-sector table of
which preset each level plays, follow-ups for each sector and boss, a music policy behind an interface its
tests fake, and a settings screen that chooses the model and the instrument library. Its recipes are
[Start endless generated music with one call](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-GameEngine.md#start-endless-generated-music-with-one-call),
[Put the music policy behind an interface and test it against fakes of the engine's music interfaces](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-GameEngine.md#put-the-music-policy-behind-an-interface-and-test-it-against-fakes-of-the-engines-music-interfaces),
[Keep the music for each level in a table the tests can read](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-GameEngine.md#keep-the-music-for-each-level-in-a-table-the-tests-can-read)
and
[Show the player which model and instruments are really playing](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-GameEngine.md#show-the-player-which-model-and-instruments-are-really-playing),
listed with the other [game-engine blueprints](../../samples/blueprints.md#hosting-a-game-engine).

The engine's music system underneath - buses, fades, ducking, stingers and quantized transitions - has its
own references: `samples/MusicDemo` in the engine repository and
[GameEngineMusicDemo](https://github.com/ellisnet/CodeBrix.Samples/tree/main/GameEngineMusicDemo) in
CodeBrix.Samples. How the music itself is generated, and why its waits are ordinary states, is covered on
the [CodeBrix.Audio.MusicGeneration](../CodeBrix.Audio.MusicGeneration.md) page.

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide for the generated music package (ships inside the package too) | [src/CodeBrix.Platform.GameEngine.GeneratedMusic/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/blob/main/src/CodeBrix.Platform.GameEngine.GeneratedMusic/AGENT-README.txt) |
| Generated music tests - the one call through the engine, the provider driven as the audio thread drives it, the option mapping, and the session seams with scripted fakes | [tests/CodeBrix.Platform.GameEngine.GeneratedMusic.Tests](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/tree/main/tests/CodeBrix.Platform.GameEngine.GeneratedMusic.Tests) |
| The music generation library: models, instrument libraries, presets and sessions | [CodeBrix.Audio.MusicGeneration](../CodeBrix.Audio.MusicGeneration.md) |

The tests that play the real models through both instrument libraries are opt-in, and run only when the
`GENERATEDMUSIC_MODEL_TESTS` environment variable is `1`.

## License

The generated music package (`CodeBrix.Platform.GameEngine.GeneratedMusic.MitLicenseForever`) is MIT. The
model packages a game adds are licensed on their own terms, named in their package IDs
(`CodeBrix.Audio.MusicGeneration.SkyTNT.ApacheLicenseForever`,
`CodeBrix.Audio.MusicGeneration.MuPT.ApacheLicenseForever`), and the recorded instrument library is
`CodeBrix.Audio.Samples.FluidR3Gm.MitLicenseForever`. For what the engine repository incorporates, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/blob/main/THIRD-PARTY-NOTICES.txt).

---

**Where to go next**

- [CodeBrix.Audio.MusicGeneration](../CodeBrix.Audio.MusicGeneration.md) - the models, instrument libraries and sessions underneath
- [CodeBrix.Platform.GameEngine](../CodeBrix.Platform.GameEngine.md#music) - the music system the music plays on: buses, ducking, stingers and the global pause
- [Kenney assets](kenney-assets.md) - the sound effects and art that go with it
- [Cards and dice](cards-and-dice.md) - the add-on for card, dice and board-style games
