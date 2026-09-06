<sub>[CodeBrix](../README.md) › Licensing</sub>

# Licensing

**Every CodeBrix package names its license in its own package ID, and that package ID is bound to
that license permanently.** You can read a project file and know the licensing position of the
application without opening a single `LICENSE` file. This page states the guarantee, lists the
license families in use, gives the license of every library, and sets out what the copyleft
packages ask of you.

## The package ID names the license

Every package ID ends in a `.{license}LicenseForever` suffix -
`CodeBrix.Audio.MitLicenseForever`, `CodeBrix.Sqlite.ApacheLicenseForever`,
`FreePPlus.LgplLicenseForever`. The suffix is a permanent guarantee:

> a package with that exact package ID will never, ever have its license change.

The disclaimer that comes with it is equally precise. The license of the source code behind a
package could change. If it did, new versions could not be published under the old package ID -
they would need a new package ID whose suffix names the new license, and the old ID stays locked
to its license, with its published versions available as-is.

> [!IMPORTANT]
> The suffix appears in package IDs only. Namespaces do not carry it. You reference
> `CodeBrix.Audio.MitLicenseForever` and you write `using CodeBrix.Audio;` - there is no package
> named plain `CodeBrix.Audio`, and there is no namespace containing the word `LicenseForever`.

Several packages set NuGet's license-acceptance flag, so a restore asks you to accept the license
before it completes. That is expected, not an error.

## The license families

| Package-ID suffix | License | Kind |
| --- | --- | --- |
| `.MitLicenseForever` | MIT | Permissive |
| `.ApacheLicenseForever` | Apache License 2.0 | Permissive |
| `.BsdLicenseForever` | A BSD license - 2-Clause or 3-Clause, as the library's own page and its packaged `LICENSE` state | Permissive |
| `.MsplLicenseForever` | Microsoft Public License (Ms-PL) | Permissive |
| `.ZlibLicenseForever` | zlib License | Permissive |
| `.OflLicenseForever` | SIL Open Font License 1.1 | Font license |
| `.LgplLicenseForever` | GNU Lesser General Public License - version 2.1 or later, or version 3 or later, as stated per library below | Copyleft |
| `.GplLicenseForever` | GNU General Public License, version 3 only | Copyleft |

Most of the family is permissive. Only the packages listed under
[Copyleft packages](#copyleft-packages) carry obligations that reach into how you build and
distribute your own application.

## Every library and its license

Where a repository produces packages under more than one license, the differing package IDs are
named in the same cell.

| Library | License |
| --- | --- |
| [CodeBrix.ArgumentParser](libraries/CodeBrix.ArgumentParser.md) | MIT |
| [CodeBrix.AssemblyTools](libraries/CodeBrix.AssemblyTools.md) | MIT |
| [CodeBrix.Audio](libraries/CodeBrix.Audio.md) | MIT |
| [CodeBrix.Audio.ModestSynth](libraries/CodeBrix.Audio.ModestSynth.md) | MIT, like the CodeBrix.Audio package it depends on; the two are published together at the same version |
| [CodeBrix.Audio.Opus](libraries/CodeBrix.Audio.Opus.md) | BSD 3-Clause; the CodeBrix.Audio package it depends on is MIT |
| [CodeBrix.Compression](libraries/CodeBrix.Compression.md) | MIT |
| [CodeBrix.Cryptography](libraries/CodeBrix.Cryptography.md) | MIT |
| [CodeBrix.Docker](libraries/CodeBrix.Docker.md) | MIT |
| [CodeBrix.Imaging](libraries/CodeBrix.Imaging.md) | Apache 2.0 |
| [CodeBrix.Imaging.Drawing](libraries/CodeBrix.Imaging.Drawing.md) | Apache 2.0, for both the Skia-backed and the managed package |
| [CodeBrix.Json.Extensions](libraries/CodeBrix.Json.Extensions.md) | MIT |
| [CodeBrix.LilyPort](libraries/CodeBrix.LilyPort.md) | GPL-3.0-only - see [Copyleft packages](#copyleft-packages) |
| [CodeBrix.LilyScheme](libraries/CodeBrix.LilyScheme.md) | LGPL-3.0-or-later - see [Copyleft packages](#copyleft-packages) |
| [CodeBrix.MarkupParse](libraries/CodeBrix.MarkupParse.md) | MIT |
| [CodeBrix.NotionApi](libraries/CodeBrix.NotionApi.md) | MIT |
| [CodeBrix.PdfDocuments](libraries/CodeBrix.PdfDocuments.md) | MIT, for all five packages |
| [CodeBrix.Platform](libraries/CodeBrix.Platform.md) | Apache 2.0, except `CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever` (MIT) and `CodeBrix.Platform.MediaPlayer.LgplLicenseForever` (LGPL-2.1-or-later) |
| [CodeBrix.Platform.Extensions](libraries/CodeBrix.Platform.Extensions.md) | Apache 2.0 |
| [CodeBrix.Platform.Fonts.Fluent](libraries/CodeBrix.Platform.Fonts.Fluent.md) | Apache 2.0, for the packaging and the bundled font alike |
| [CodeBrix.Platform.Fonts.Merriweather](libraries/CodeBrix.Platform.Fonts.Merriweather.md) | SIL OFL 1.1 |
| [CodeBrix.Platform.Fonts.NotoMusic](libraries/CodeBrix.Platform.Fonts.NotoMusic.md) | SIL OFL 1.1 |
| [CodeBrix.Platform.Fonts.OpenSans](libraries/CodeBrix.Platform.Fonts.OpenSans.md) | Apache 2.0 and SIL OFL 1.1 - the packaging is Apache 2.0, the bundled fonts are OFL |
| [CodeBrix.Platform.Fonts.Roboto](libraries/CodeBrix.Platform.Fonts.Roboto.md) | SIL OFL 1.1 |
| [CodeBrix.Platform.Fonts.RobotoMono](libraries/CodeBrix.Platform.Fonts.RobotoMono.md) | SIL OFL 1.1 |
| [CodeBrix.Platform.GameEngine](libraries/CodeBrix.Platform.GameEngine.md) | MIT; the optional `CodeBrix.Platform.GameEngine.Sdl2.ZlibLicenseForever` is MIT and zlib |
| [CodeBrix.Platform.LinuxDBus](libraries/CodeBrix.Platform.LinuxDBus.md) | MIT |
| [CodeBrix.Platform.MediaPlayerCore](libraries/CodeBrix.Platform.MediaPlayerCore.md) | LGPL-2.1-or-later, for all three packages - see [Copyleft packages](#copyleft-packages) |
| [CodeBrix.Platform.OpenGL](libraries/CodeBrix.Platform.OpenGL.md) | MIT |
| [CodeBrix.Platform.TclTk](libraries/CodeBrix.Platform.TclTk.md) | BSD 2-Clause, for all three packages |
| [CodeBrix.Platform.Unicode](libraries/CodeBrix.Platform.Unicode.md) | Apache 2.0 and Unicode 3.0 - the packaging and wrapper assemblies are Apache 2.0, the bundled ICU binaries and data archive are Unicode 3.0 |
| [CodeBrix.Plotter](libraries/CodeBrix.Plotter.md) | MIT |
| [CodeBrix.PolygonTools](libraries/CodeBrix.PolygonTools.md) | MIT and BSL-1.0 - the CodeBrix additions and the combined work are MIT, and the derived geometry code remains under the Boost Software License 1.0 |
| [CodeBrix.Python](libraries/CodeBrix.Python.md) | MIT |
| [CodeBrix.Redis](libraries/CodeBrix.Redis.md) | MIT |
| [CodeBrix.ServiceLocator](libraries/CodeBrix.ServiceLocator.md) | Ms-PL |
| [CodeBrix.SkiaSvg](libraries/CodeBrix.SkiaSvg.md) | MIT |
| [CodeBrix.Sqlite](libraries/CodeBrix.Sqlite.md) | Apache 2.0 |
| [CodeBrix.SSH](libraries/CodeBrix.SSH.md) | MIT |
| [CodeBrix.StyleSheetParse](libraries/CodeBrix.StyleSheetParse.md) | MIT |
| [CodeBrix.SvgParse](libraries/CodeBrix.SvgParse.md) | Ms-PL |
| [CodeBrix.Templating](libraries/CodeBrix.Templating.md) | BSD 2-Clause |
| [CodeBrix.Terminal](libraries/CodeBrix.Terminal.md) | MIT |
| [CodeBrix.TestMocks](libraries/CodeBrix.TestMocks.md) | Apache 2.0 |
| [CodeBrix.Texinfo](libraries/CodeBrix.Texinfo.md) | MIT, for both packages |
| [CodeBrix.VideoPlayback](libraries/CodeBrix.VideoPlayback.md) | MIT, for all three packages |
| [CodeBrix.VideoPlayback.Dav1d](libraries/CodeBrix.VideoPlayback.Dav1d.md) | BSD 2-Clause |
| [CodeBrix.VideoProcessing](libraries/CodeBrix.VideoProcessing.md) | MIT |
| [CodeBrix.VideoProcessing.OpenCV5](libraries/CodeBrix.VideoProcessing.OpenCV5.md) | Apache 2.0, for the managed binding, the WPF bridge and every native runtime package |
| [CodeBrix.YamlParse](libraries/CodeBrix.YamlParse.md) | MIT |
| [FreePPlus](libraries/FreePPlus.md) | LGPL-3.0-or-later - see [Copyleft packages](#copyleft-packages) |
| [SilverAssertions](libraries/SilverAssertions.md) | Apache 2.0 |

## Copyleft packages

A small number of package IDs across the family are copyleft. They are safe to use, and the
sources are exact about what each one asks. Read this section before you reference one.

### LGPL packages

The LGPL packages are `CodeBrix.LilyScheme.LgplLicenseForever` and `FreePPlus.LgplLicenseForever`
(LGPL-3.0-or-later), and `CodeBrix.MediaCore.LgplLicenseForever`,
`CodeBrix.Platform.MediaPlayerCore.LgplLicenseForever`, `CodeBrix.Webcam.LgplLicenseForever` and
`CodeBrix.Platform.MediaPlayer.LgplLicenseForever` (LGPL-2.1-or-later).

- Referencing the unmodified NuGet package from proprietary software is permitted. If you modify
  the library's source, you must make those modifications available under the LGPL.
- Consume the package through a `<PackageReference>`, and keep the assembly replaceable. Do not
  merge the DLL into another assembly, and do not ship it only as a trimmed or single-file
  artifact from which it cannot be replaced - either one forfeits the relinking right the license
  is built around.
- Ship the license files the package carries onward with your application. The LGPL-3 packages
  carry both the LGPL text and the GPL text, because LGPL-3 incorporates the GPL by reference and
  the LGPL text alone would be incomplete.

> [!WARNING]
> Merging an LGPL assembly into another assembly, or shipping it in a form the user cannot
> replace, violates the license. If your build produces a single merged artifact, do not
> reference an LGPL package.

If an application must stay clear of LGPL entirely, the family offers permissive alternatives for
the same jobs: [CodeBrix.Audio](libraries/CodeBrix.Audio.md) and the
[AudioPlayer add-in](platform/add-ins/AudioPlayer.md) for audio playback, and
[CodeBrix.VideoPlayback](libraries/CodeBrix.VideoPlayback.md) with the
[VideoPlayer add-in](platform/add-ins/VideoPlayer.md) for video.

### The GPL package

`CodeBrix.LilyPort.GplLicenseForever` is GPL-3.0-only - deliberately not "or later". The package
bundles material licensed under GPL version 3 only; GPL-3-only and GPL-3-or-later material
combine legally, but the combined work can then be conveyed only under GPL version 3 exactly.

- Referencing this package makes your application a GPL-3.0 work when distributed. Decide that
  before writing code against it.
- Your application becomes a work based on the package and, when conveyed to anyone, must be
  licensed under GPL version 3 with its complete corresponding source available. Internal use
  with no distribution carries no obligation.
- The repository's own source files are GPL-3.0-or-later, while the package conveys as
  GPL-3.0-only. Both statements are true at once.
- The Scheme interpreter it depends on is LGPL-3.0-or-later, and a GPL work may consume it
  freely.
- `LICENSE`, `LICENSE.OFL` and `THIRD-PARTY-NOTICES.txt` travel in the package root; keep them
  with any redistribution.
- The engraved SVG the engine writes contains no font data - glyphs are outline paths and text is
  text with a family name - so a rendered document does not carry a font license with it.

### The reference applications

The reference applications are split across three repositories by license, so that the terms of
one application never set the terms for the rest.

| Repository | License |
| --- | --- |
| [CodeBrix.Samples](https://github.com/ellisnet/CodeBrix.Samples) | Apache License 2.0 |
| [CodeBrix.Samples.Gpl2](https://github.com/ellisnet/CodeBrix.Samples.Gpl2) | GNU General Public License, version 2 |
| [CodeBrix.Samples.Gpl3](https://github.com/ellisnet/CodeBrix.Samples.Gpl3) | GNU General Public License, version 3 |

The GPL-3 repository also demonstrates the aggregation rule you will meet if you build on a
copyleft library: an aggregate takes the narrower of the terms it combines. Fresco.Brix's own
sources are GPL-3.0-or-later, but the application as conveyed is GPL-3.0-only, because the
engraving library it links is conveyed on those terms.

## Fonts

The font packages redistribute font files, and the license covers the whole package - the wrapper
assembly, the MSBuild files and the font binaries alike. Under the SIL Open Font License 1.1 the
font files ship with their license text inside the package; keep those files with any
redistribution of your application. Where a package bundles fonts from more than one project, it
carries one license text per project at the package root.

## Provenance

For the provenance and licensing of open source code included in a library, see
`THIRD-PARTY-NOTICES.txt` in that library's repository. Every repository carries one, every
package packs it, and it is the authoritative record of what came from where and under which
license. The [repository directory](repo-directory.md) links each one directly.

---

**Where to go next**

- [Libraries](libraries/README.md) - every library by topic, with its packages and license
- [Repository directory](repo-directory.md) - every repository's documents, tests and samples
- [Build a CodeBrix.Platform application](platform/README.md) - the curriculum, in order
