<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Audio.Samples.FluidR3Gm</sub>

# CodeBrix.Audio.Samples.FluidR3Gm

**CodeBrix.Audio.Samples.FluidR3Gm is a recorded General MIDI instrument library for
[CodeBrix.Audio](CodeBrix.Audio.md): the package carries the FluidR3_GM SoundFont and offers it to
CodeBrix.Audio's instrument registry under the name `FluidR3Gm`, so an application that already plays
MIDI swaps its whole sound for recorded instruments in one line.** Every General MIDI program and the
General MIDI percussion kit are covered, one SoundFont is shared by every part of an arrangement, and
the file is loaded on first use. The SoundFont arrives with the package and is landed in the output
folder of every project that reaches it, so the usual case needs no download and no configuration. You
reach for it from any .NET 10 application that plays or renders MIDI through CodeBrix.Audio, and from
[CodeBrix.Audio.MusicGeneration](CodeBrix.Audio.MusicGeneration.md) and the GameEngine's
[GeneratedMusic add-in](gameengine/generated-music.md), which ask for instrument libraries by name.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Audio.Samples.FluidR3Gm](https://github.com/ellisnet/CodeBrix.Audio.Samples.FluidR3Gm) |
| **Packages** | [`CodeBrix.Audio.Samples.FluidR3Gm.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.Samples.FluidR3Gm.MitLicenseForever) |
| **License** | MIT, for the library and for the SoundFont alike; see [License](#license) |
| **Requires** | .NET 10 or later. [`CodeBrix.Audio.Core.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.Core.MitLicenseForever) arrives automatically and is the only dependency. Device playback also needs the platform package: [`CodeBrix.Audio.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MitLicenseForever) on the desktop, or [CodeBrix.Audio.Android](CodeBrix.Audio.Android.md) on Android. Offline synthesis and rendering need Core only. No native library, no service and no network access |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application, wherever CodeBrix.Audio resolves an instrument library by name |
| **Platforms** | Everywhere CodeBrix.Audio runs. On Android the SoundFont is packaged as an Android asset and extracted before first use - see [Android](#android) |

## What it does

- Covers every General MIDI program and the General MIDI percussion kit from recorded instruments; the
  kit carries drum notes above and below the range General MIDI pins down.
- Registers under the name `FluidR3Gm` in one line, so instruments are chosen by name and the same
  application swaps between this library and the synthesized `ModestSynthGm` library by changing that
  name.
- Hands back one synthesizer per part, pinned to its program, for an arrangement that voices each part
  deliberately, and a percussion synthesizer that sounds on whatever channel it is given.
- Hands back one multi-timbral synthesizer that honors the program changes a MIDI file carries and
  treats channel 10 as percussion - the "play this `.mid` with no configuration" road.
- Shares one SoundFont between every synthesizer it ever creates, so a twelve-part arrangement holds
  one copy of the samples.
- Loads nothing at registration: the first call that needs a sound reads the file.
- Reports coverage read from the SoundFont itself rather than claimed.
- Plays from a SoundFont kept wherever the application keeps its assets, with one build property and
  one line of code.

## When to use it

Reach for it when MIDI music should sound like recorded instruments rather than synthesis - the solo
violin, concert piano, orchestral harp and cello are where it is clearly better. The synthesized General
MIDI library in [CodeBrix.Audio.ModestSynth](CodeBrix.Audio.ModestSynth.md) covers exactly the same
surface and costs neither load time nor memory; the two are alternatives, not layers, and neither
depends on the other. An application that wants both registers both packages and lets the user choose.

It is also a good base for building a sound of your own: CodeBrix.Audio's `MappedInstrumentLibrary`
takes this library as its base and replaces one program at a time with a Decent Sampler pack, an SFZ
instrument or another SoundFont, so a piece is never silent while you work.

What it does not do:

- It does not register itself. Registration is always the application's job.
- It does not play or schedule anything; it hands back synthesizers and CodeBrix.Audio plays, sequences
  and renders them.
- It cannot unload the SoundFont: every part of every arrangement shares it.
- It does not change the sound. The SoundFont is redistributed exactly as its author published it - not
  converted, trimmed, re-mapped or re-encoded.
- It is not a general SoundFont player. For a SoundFont of your own, use CodeBrix.Audio's
  `SoundFontInstrumentLibrary` directly, which takes a name, a description and a path; playing any
  `.sf2` without a library is covered in [SoundFont and SFZ instruments](audio/soundfont-and-sfz.md).
- It does not depend on, and has nothing to do with, music generation.

## Getting started

```bash
dotnet add package CodeBrix.Audio.Samples.FluidR3Gm.MitLicenseForever
```

The package ID carries the license suffix; the namespace does not. You install
`CodeBrix.Audio.Samples.FluidR3Gm.MitLicenseForever` and you write
`using CodeBrix.Audio.Samples.FluidR3Gm;`.

A complete console application, from the package's guide:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.Audio.Samples.FluidR3Gm.MitLicenseForever" />
  </ItemGroup>
</Project>
```

```csharp
using System;
using CodeBrix.Audio.Instruments;
using CodeBrix.Audio.Midi;
using CodeBrix.Audio.Samples.FluidR3Gm;

FluidR3GmInstrumentLibrary.Register();

var library = InstrumentLibraryRegistry.Resolve("FluidR3Gm");
var piano = library.CreateSynthesizer((int)GeneralMidiProgram.AcousticGrandPiano, 44100);

Console.WriteLine($"Playing from {FluidR3GmInstrumentLibrary.SoundFontPath}");
Console.WriteLine(library.Coverage);
```

Nothing else is needed: the SoundFont is in the output folder beside the executable, put there by the
package. The package adds exactly one public type, `FluidR3GmInstrumentLibrary`; the registry, the
instrument-library interface, the synthesizers and the renderer all belong to CodeBrix.Audio.

## Key concepts

### Registering, and which library is the default

CodeBrix.Audio ships no instruments and registers nothing, so `FluidR3GmInstrumentLibrary.Register()`
is always the application's call. It is idempotent, safe from any thread, and loads nothing; registering
a different library under the same name is an error. Names are matched case-insensitively, and the
**first** library registered is the default - there is no priority. Ask for the library by name when it
matters, or make it the default explicitly:

```csharp
using CodeBrix.Audio.Instruments;
using CodeBrix.Audio.ModestSynth;
using CodeBrix.Audio.Samples.FluidR3Gm;

FluidR3GmInstrumentLibrary.Register();      // first, so it is the default
GeneralMidiInstrumentLibrary.Register();    // CodeBrix.Audio.ModestSynth

// or, whatever the order was:
InstrumentLibraryRegistry.SetDefault("FluidR3Gm");
```

### The library's members

`FluidR3GmInstrumentLibrary` has a private constructor and one shared `Instance`. Its static members
are `LibraryName` (`"FluidR3Gm"`), `SoundFontFileName`, `Register()`, `IsRegistered`, `SoundFontPath`,
`IsSoundFontAvailable`, `IsLoaded` and `UseSoundFontAt(path)`. Through the instrument-library interface
it offers `Coverage`, `SupportsPerPart` and `SupportsMultiTimbral` (both true), the shared `SoundFont`,
and the three synthesizer factories:

| Factory | What it hands back |
| --- | --- |
| `CreateSynthesizer(program, sampleRate)` | One part, pinned to that program: a program change in the music does not re-voice it |
| `CreatePercussionSynthesizer(sampleRate)` | The kit, sounding on whatever channel it is given |
| `CreateMultiTimbralSynthesizer(sampleRate)` | One synthesizer for a whole piece, honoring program changes, with channel 10 as percussion |

Each throws `FileNotFoundException` when the SoundFont is not at `SoundFontPath`, with a message naming
the usual cause.

### Loading is lazy, and happens once

`Register()` reads nothing. The first call that needs a sound - creating a synthesizer, or reading
`Coverage` - reads the file, and every synthesizer the library hands back afterwards renders from that
one SoundFont. `IsSoundFontAvailable` is the cheap question "is the file there?"; reading `Coverage`
loads the SoundFont to answer. To take the load where it does not show, touch the library during a
loading screen:

```csharp
_ = InstrumentLibraryRegistry.Resolve("FluidR3Gm").Coverage;
```

The SoundFont is about 140 MiB of recorded audio, and all of it is held while the library is in use.

### Keeping the SoundFont somewhere else

The package lands the SoundFont, and the author's license and release notice beside it, in the output
folder of every project that reaches it - including an application that reaches the package only
through a library of its own. For an application built from several projects that means several copies
of a large file. Switch the copying off in the build and name the file at start-up, before the first
sound:

```xml
<PropertyGroup>
  <CodeBrixFluidR3GmCopyAssetsToOutput>false</CodeBrixFluidR3GmCopyAssetsToOutput>
</PropertyGroup>
```

```csharp
FluidR3GmInstrumentLibrary.UseSoundFontAt("/opt/mygame/assets/FluidR3_GM.sf2");
FluidR3GmInstrumentLibrary.Register();
```

Ship `LICENSE-FluidR3_GM.txt` and `README-FluidR3_GM.txt` with the SoundFont wherever it goes: the
license requires the notice to travel with the samples. `CodeBrixFluidR3GmAssetsDirectory` changes
where the build copies the assets from, for a build that relocates package content.

### Android

A file copied to a build output folder is not put into an APK. On Android, package the SoundFont and
its notices as Android assets, extract them into private storage with
[CodeBrix.Audio.Android](CodeBrix.Audio.Android.md)'s `AndroidPackagedAssets`, and call
`UseSoundFontAt` before the first sound. The project-file items and the start-up code are on the
[CodeBrix.Audio.Android page](CodeBrix.Audio.Android.md#examples).

## Examples

Rendering a MIDI file to a WAV file with recorded instruments, with no audio device:

```csharp
using CodeBrix.Audio.Instruments;
using CodeBrix.Audio.Samples.FluidR3Gm;
using CodeBrix.Audio.Synth;

FluidR3GmInstrumentLibrary.Register();

var library = InstrumentLibraryRegistry.Resolve(FluidR3GmInstrumentLibrary.LibraryName);
var synthesizer = library.CreateMultiTimbralSynthesizer(44100);

SoundFontRenderer.RenderToWavFile(
    synthesizer, new MidiSequence("piece.mid"), "piece.wav");
```

Offering the player both sounds and switching between them:

```csharp
using System;
using CodeBrix.Audio.Instruments;
using CodeBrix.Audio.ModestSynth;
using CodeBrix.Audio.Samples.FluidR3Gm;

GeneralMidiInstrumentLibrary.Register();    // synthesized; registered first,
FluidR3GmInstrumentLibrary.Register();      // so it is the default

foreach (var name in InstrumentLibraryRegistry.RegisteredNames)
{
    var choice = InstrumentLibraryRegistry.Resolve(name);
    Console.WriteLine($"{choice.Name}: {choice.Description}");
}

// the player chose; nothing else in the application changes
var library = InstrumentLibraryRegistry.Resolve(chosenName);
```

Checking at start-up that the SoundFont is there, and falling back to synthesis when it is not:

```csharp
using System;
using CodeBrix.Audio.Instruments;
using CodeBrix.Audio.Samples.FluidR3Gm;

FluidR3GmInstrumentLibrary.Register();

if (!FluidR3GmInstrumentLibrary.IsSoundFontAvailable)
{
    Console.Error.WriteLine(
        $"No SoundFont at {FluidR3GmInstrumentLibrary.SoundFontPath} - falling back to the "
        + "synthesized instruments.");
    InstrumentLibraryRegistry.SetDefault("ModestSynthGm");
}
```

Starting from this library and replacing voices one at a time with instruments of your own:

```csharp
using CodeBrix.Audio.Instruments;
using CodeBrix.Audio.Midi;
using CodeBrix.Audio.Samples.FluidR3Gm;

FluidR3GmInstrumentLibrary.Register();

var mine = new MappedInstrumentLibrary(
    "MyGame",
    "FluidR3Gm, with a piano and a cello of my own.",
    FluidR3GmInstrumentLibrary.LibraryName);       // the base, by name

mine.SetInstrument(GeneralMidiProgram.AcousticGrandPiano, "/packs/Piano.dspreset");
mine.SetInstrument(GeneralMidiProgram.Cello, "/packs/Cello.sfz");
mine.Register();

var library = InstrumentLibraryRegistry.Resolve("MyGame");
```

Every program you have not set still plays from this library.

## Pitfalls

- **Forgetting to register.** Nothing registers on your behalf, and with an empty registry nothing
  plays.
- **Assuming this library is the default.** The first library registered is; ask by name or call
  `InstrumentLibraryRegistry.SetDefault("FluidR3Gm")`.
- **Excluding the package's build assets.** `PrivateAssets` or `ExcludeAssets` covering
  `buildtransitive` stops the SoundFont being copied, and the failure shows at the first note rather
  than at build time.
- **Reading `Coverage` to find out whether the file is there.** It loads the SoundFont;
  `IsSoundFontAvailable` touches nothing.
- **Calling `UseSoundFontAt` too late.** It refuses once the SoundFont has been read; `IsLoaded` says
  whether that moment has passed.
- **Putting a melodic part on channel 10.** The SoundFont format hard-wires channel 10 to the drum
  bank, whatever program the part was created with.
- **Expecting a pinned part to follow the music.** `CreateSynthesizer` ignores program changes on
  purpose; use `CreateMultiTimbralSynthesizer` when the music should decide.
- **Sharing a synthesizer between threads.** The synthesizers are single-threaded by contract; one
  synthesizer per part, rendered by one thread.
- **Shipping the samples without the notice.** Keep the two text files beside the SoundFont.

## Samples and tools in the repository

The repository ships one package and no sample applications. Everything below exists to test the
package or to produce the SoundFont it ships; none of it is packed.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Test project | Registration and its rules, lazy loading, where the SoundFont is read from, coverage of every program and the kit, both synthesizer shapes, the one shared SoundFont, and non-silent offline renders. The tests that need a sound skip, saying what to run, when nothing has been staged | [`tests/CodeBrix.Audio.Samples.FluidR3Gm.Tests`](https://github.com/ellisnet/CodeBrix.Audio.Samples.FluidR3Gm/tree/main/tests/CodeBrix.Audio.Samples.FluidR3Gm.Tests) |
| Asset stager | A console application that reproduces the shipped SoundFont from its recorded upstream source archive, with nothing but .NET installed, and records sizes and hashes in `ASSET-PROVENANCE.json` | [`tools/AssetStager`](https://github.com/ellisnet/CodeBrix.Audio.Samples.FluidR3Gm/tree/main/tools/AssetStager) |

The SoundFont is not in git - it is far larger than a git host accepts - and the package cannot be
packed until the stager has run.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Audio.Samples.FluidR3Gm/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio.Samples.FluidR3Gm/blob/main/AGENT-README.txt) |
| Tests, tools and other non-package content | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Audio.Samples.FluidR3Gm/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Audio.Samples.FluidR3Gm/blob/main/README-INDEX.txt) |
| Provenance and licensing of the SoundFont | [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio.Samples.FluidR3Gm/blob/main/THIRD-PARTY-NOTICES.txt) |
| Where the SoundFont came from, with sizes and hashes | [ASSET-PROVENANCE.json](https://github.com/ellisnet/CodeBrix.Audio.Samples.FluidR3Gm/blob/main/ASSET-PROVENANCE.json) |
| Tests (worked examples) | [tests/CodeBrix.Audio.Samples.FluidR3Gm.Tests](https://github.com/ellisnet/CodeBrix.Audio.Samples.FluidR3Gm/tree/main/tests/CodeBrix.Audio.Samples.FluidR3Gm.Tests) |

XML documentation ships alongside the assembly.

## License

CodeBrix.Audio.Samples.FluidR3Gm is licensed under the MIT License, and the license is also named in
the package ID (`CodeBrix.Audio.Samples.FluidR3Gm.MitLicenseForever`). License acceptance is required.
The library's own source code incorporates no third-party source code. The sound content it
redistributes - the FluidR3_GM SoundFont, unmodified - is third-party work, released by its author
under the MIT License and redistributed on those terms. The author's license text and release
statement travel with the SoundFont as `LICENSE-FluidR3_GM.txt` and `README-FluidR3_GM.txt`, and
both are reproduced in full in
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio.Samples.FluidR3Gm/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Audio](CodeBrix.Audio.md) - the library that plays, sequences and renders these instruments
- [SoundFont and SFZ instruments](audio/soundfont-and-sfz.md) - playing any SoundFont, and the two SoundFont paths
- [CodeBrix.Audio.ModestSynth](CodeBrix.Audio.ModestSynth.md) - the synthesized General MIDI library, the alternative to this one
- [CodeBrix.Audio.MusicGeneration](CodeBrix.Audio.MusicGeneration.md) - generated music voiced through either library
- [ellisnet/CodeBrix.Audio.Samples.FluidR3Gm on GitHub](https://github.com/ellisnet/CodeBrix.Audio.Samples.FluidR3Gm) - source, tests and the asset stager
