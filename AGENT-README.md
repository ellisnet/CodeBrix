<sub>[CodeBrix](README.md) › For AI agents</sub>

# CodeBrix for AI coding agents

**This page is the entry point for an AI coding agent asked to build or modify a CodeBrix application.** It gives the order to read the guide in, the rules that make CodeBrix.Platform code correct, how to fetch the authoritative API reference for any library in one request, and a table of every library documented here. Read this page first, then fetch only what the task needs.

## What this repository is

This repository is documentation and nothing else. It holds no code, no project files and no packages. It exists so that a human or an agent can learn the whole CodeBrix universe from one place and then go straight to the right repository, the right package and the right API guide.

The universe has two halves, and a task usually lives in one of them.

- **CodeBrix.Platform** is a cross-platform desktop UI application framework. An application is written once against the WinUI XAML API surface and runs on six platform heads across Windows, Linux and macOS. Its curriculum is [Build a CodeBrix.Platform application](docs/platform/README.md).
- **The standalone libraries** are ordinary .NET libraries - PDF, spreadsheets, audio, video, imaging, SVG, cryptography, SQLite, Redis, SSH, Docker, parsing, charts, testing and more. They are usable from a CodeBrix.Platform application, a console application, a service or a native WinUI, WPF or .NET MAUI application. Their catalog is [Libraries](docs/libraries/README.md).

Everything requires .NET 10 or later.

## Reading order for building a CodeBrix.Platform application

Read in this order the first time. Each page is self-contained afterwards.

| Read | Page | What you get from it |
| --- | --- | --- |
| 1 | [01 - What CodeBrix.Platform is](docs/platform/01-what-is-codebrix-platform.md) | The API surface, the two packages an application starts with, and what is out of scope |
| 2 | [02 - Runs on every laptop](docs/platform/02-runs-on-every-laptop.md) | Each head's package, bootstrap call, operating-system requirements, render paths and limits |
| 3 | [03 - Your first application](docs/platform/03-your-first-application.md) | Every file of a working application, verbatim - copy these rather than inventing project files |
| 4 | [04 - Project architecture](docs/platform/04-project-architecture.md) | Which package goes where, project and head naming, and libraries under `src/libs` |
| 5 | [05 - MVVM the right way](docs/platform/05-mvvm-the-right-way.md) | View models, commands, the design-mode guard, and marshalling background work back to the UI thread |
| 6 | [06 - Views and styling](docs/platform/06-views-and-styling.md) | XAML pages, converters, theming, fonts and controls you write yourself |
| 7 | [07 - Platform services](docs/platform/07-platform-services.md) | Windowing, dispatching, pickers, the clipboard and dialogs, and the bridges a view model reaches them through |
| 8 | [08 - Add-ins](docs/platform/08-add-ins.md) | Every add-in package and the heads each one is live on - then read only the add-in page the task needs |

If the task is narrow and the solution already exists, the three chapters that decide whether the result is correct are [03 - Your first application](docs/platform/03-your-first-application.md), [04 - Project architecture](docs/platform/04-project-architecture.md) and [05 - MVVM the right way](docs/platform/05-mvvm-the-right-way.md).

Then, by task: [09 - Graphics, media and vision](docs/platform/09-graphics-media-and-vision.md), [10 - Testing your application](docs/platform/10-testing-your-application.md), [11 - Packaging and shipping](docs/platform/11-packaging-and-shipping.md), [13 - Reference applications](docs/platform/13-reference-applications.md), [14 - Sharing code with native frameworks](docs/platform/14-sharing-code-with-native-frameworks.md).

Two pages are worth loading before debugging anything. [12 - Troubleshooting](docs/platform/12-troubleshooting.md) lists the failures that have a known cause - blank windows, missing pickers, duplicate types, missing engines. [Blueprints](docs/samples/blueprints.md) maps a task ("show a file dialog from a view model", "run a long job with progress and cancel") onto a recipe mined from a working application, with the file it came from.

## Finding the authoritative API guide for a library

Every CodeBrix package ships an `AGENT-README.txt`: a complete API reference and usage guide written for machine readers, listing types, members, signatures, the first working example, the pitfalls and the test file that demonstrates each feature area. It is the authoritative source for that library's API - ahead of any page in this repository, which summarizes it.

The same file exists in two places, and either is fine:

- **In the repository**, at the root: `https://github.com/ellisnet/<Repo>/blob/main/AGENT-README.txt`.
- **Inside the package**, in the package root, so a restored package on disk already has it.

A repository that produces more than one package carries one `AGENT-README.txt` per package - the root file covers the primary package, and the others sit beside their own project, for example `src/CodeBrix.PdfDocCreate/AGENT-README.txt`. [The repository directory](docs/repo-directory.md) lists every one of them as a direct link, including the per-add-in guides inside the CodeBrix.Platform repository.

Two more root files are worth knowing: `README-INDEX.txt` maps every document in a repository, and `THIRD-PARTY-NOTICES.txt` is the provenance and licensing record for the open source code the packages include.

> [!TIP]
> A repository's test project is its worked-example set. Each `AGENT-README.txt` names the test file that demonstrates each feature area, so "how do I do X" is answered by opening the test file named for X, under `tests/` in that repository.

## Fetching a file from GitHub

Every in-scope repository lives at `https://github.com/ellisnet/<Repo>` on branch `main`. Three URL forms cover everything.

```text
raw file    https://raw.githubusercontent.com/ellisnet/<Repo>/main/<path>
file page   https://github.com/ellisnet/<Repo>/blob/main/<path>
folder      https://github.com/ellisnet/<Repo>/tree/main/<folder>
```

Fetch the raw form when you want the content and the file page when you want a link a human will click.

```text
https://raw.githubusercontent.com/ellisnet/CodeBrix.Audio/main/AGENT-README.txt
https://raw.githubusercontent.com/ellisnet/CodeBrix.Platform/main/AGENT-README.txt
https://raw.githubusercontent.com/ellisnet/CodeBrix.Samples/main/BLUEPRINTS-Index.md
```

A repository that produces more than one package carries one `AGENT-README.txt` per package, and the
extra ones sit beside the project they document rather than at the repository root:

```text
https://raw.githubusercontent.com/ellisnet/CodeBrix.Audio/main/src/CodeBrix.Audio.ModestSynth/AGENT-README.txt
```

Each repository's `README-INDEX.txt` is the map of which file covers which package.

A package's page on nuget.org is `https://www.nuget.org/packages/<PackageId>`.

## Package IDs and how to reference them

These rules are absolute, and getting one wrong is the most common way an otherwise correct file fails to restore.

- **Every package ID ends in a `.{license}LicenseForever` suffix** - `CodeBrix.Audio.MitLicenseForever`, `CodeBrix.Sqlite.ApacheLicenseForever`, `FreePPlus.LgplLicenseForever`. There is no package named plain `CodeBrix.Audio`, and none named plain `CodeBrix.Platform`.
- **Namespaces never carry the suffix.** You reference `CodeBrix.Audio.MitLicenseForever` and you write `using CodeBrix.Audio;`. No namespace contains the word `LicenseForever`.
- **Do not guess a package ID.** Every ID in the family is listed with its repository in [the repository directory](docs/repo-directory.md) and by topic in [the library catalog](docs/libraries/README.md). If an ID is not there, it does not exist.
- **Reference packages with no `Version` attribute** and let NuGet resolve them. The family is published together.
- **The suffix is a permanent guarantee**: a package with that exact ID will never have its license change. That is what lets you read a project file and know the licensing position of the whole application. [Licensing](docs/licensing.md) states the guarantee, lists every library's license, and sets out what the LGPL and GPL packages require.
- **Some packages set NuGet's license-acceptance flag**, so a restore may ask for acceptance before it completes. That is expected, not an error.
- **A run-time message may name a package informally.** Match the name it gives against the catalog and install the verified ID.

## Rules for writing CodeBrix.Platform code

A CodeBrix.Platform solution is built from three kinds of project, and the packaging rules below are what keep it correct. [04 - Project architecture](docs/platform/04-project-architecture.md) is the full treatment.

**Which package goes where**

- The `.Core` project - a class library - holds application logic, view models, services, and ALL NuGet package references for the framework and its add-ins. It never references a platform head package.
- Each head project references EXACTLY ONE `CodeBrix.Platform.Runtime.Skia.*` package, plus the `.Core` project and the `.UI` shared project, and nothing else that is UI-related. One head project equals one head package.
- Register add-ins in `.Core`: an add-in package is referenced exactly once, there, and flows to every head transitively. Never add an add-in to a head project. An add-in that only works on some heads is inert on the others and never breaks a build.
- The shared XAML lives in the `.UI` shared project - a `.shproj` with a sibling `.projitems`, imported by each head. Do not move the Views into `.Core`: the XAML source-generator and build-task wiring does not flow across a `ProjectReference`, so the XAML must be compiled INTO each head.

**Project and build settings**

- Define `HAS_CODEBRIX` and `HAS_CODEBRIX_WINUI` in `.Core` and in every head.
- Declare `.xaml` files as `<Page>` items in each head, and import the `.UI` `.projitems`.
- The WPF head targets the Windows-specific target framework moniker rather than the plain one, and must NOT set `<UseWPF>true</UseWPF>`. Its project file in [03 - Your first application](docs/platform/03-your-first-application.md) shows the exact moniker. Force the software render surface on that head right after `Build()`; the default OpenGL renderer produces a blank window on many systems.
- Head projects are never named `MyApp.Windows`. A segment matching a top-level SDK namespace - above all `Windows`, and also `System` - shadows the global namespace on that one head. The Skia-on-Win32 head is named `.Win32Skia`.

**Application code**

- Call `App.InitializeLogging()` from each head's `Program.Main` BEFORE `CodeBrixPlatformHostBuilder.Create()`.
- Write standard WinUI XAML with the standard namespace URIs, `http://schemas.microsoft.com/winfx/2006/xaml/presentation` and `xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"`. Toolkit and converter types use `using:` XAML namespaces.
- All UI access happens on the UI thread. Capture `DispatcherQueue` on the UI thread and marshal back with `TryEnqueue`.
- Set `XamlRoot` on every `ContentDialog` before `ShowAsync()`; the framework does not fill it in, and a second dialog shown while one is open throws.
- Set `Application.RequestedTheme` only in the `App` constructor before `InitializeComponent()`; afterwards the setter throws. Per-element, `FrameworkElement.RequestedTheme` can change at run time.
- On the frame-buffer head, pickers, the software keyboard and the text clipboard are opt-in on the host builder. Without them the standard picker APIs throw on that head.
- Every WinUI and UWP type and member exists so that code and XAML compile unchanged, but some are not backed by an implementation and throw a "not implemented" exception naming the exact member. Read the member name out of the message and use an implemented alternative or guard the call.

> [!IMPORTANT]
> Do not mix the two UI-framework families in one application head. A given executable uses the CodeBrix.Platform head packages or the native WinUI, WPF and .NET MAUI toolkit packages, never both. The two families share an identical "Simple" MVVM API, so one view model can drive heads from both - see [14 - Sharing code with native frameworks](docs/platform/14-sharing-code-with-native-frameworks.md).

## Every library page

One page per library. Each page carries the library's capabilities, its first working example, its key concepts, its pitfalls, and links to its repository, its `AGENT-README.txt`, its tests and its samples. A repository that produces an add-on package alongside its main one has a page for each, and a library with a large surface has sub-pages under its own folder - [CodeBrix.Audio](docs/libraries/CodeBrix.Audio.md) is the one that does.

| Page | What the library does |
| --- | --- |
| [CodeBrix.ArgumentParser](docs/libraries/CodeBrix.ArgumentParser.md) | Turns a `string[] args` into invocations of callbacks you register, generates the matching `--help` text, and models whole sub-command suites |
| [CodeBrix.AssemblyTools](docs/libraries/CodeBrix.AssemblyTools.md) | Reads, writes and rewrites .NET assemblies: IL, metadata and debug symbols, in a single managed assembly |
| [CodeBrix.Audio](docs/libraries/CodeBrix.Audio.md) | Reads WAV, MP3, Ogg Vorbis, FLAC and AIFF, reads and writes Standard MIDI Files, plays media and sound effects, plays SoundFont, SFZ and Decent Sampler instruments, and plays a multi-track song of recordings and MIDI performances |
| [CodeBrix.Audio.ModestSynth](docs/libraries/CodeBrix.Audio.ModestSynth.md) | Adds synthesis to CodeBrix.Audio: ten oscillator waveforms, seven creative effects, a patch model and a polyphonic synthesizer that plays a patch from MIDI |
| [CodeBrix.Audio.Opus](docs/libraries/CodeBrix.Audio.Opus.md) | Adds Ogg Opus decoding and encoding to CodeBrix.Audio, in pure managed code with no native binaries |
| [CodeBrix.Compression](docs/libraries/CodeBrix.Compression.md) | Creates, reads and extracts Zip, GZip, Tar and BZip2 archives, with AES encryption and Zip64, and decompresses legacy DCL and `.Z` data |
| [CodeBrix.Cryptography](docs/libraries/CodeBrix.Cryptography.md) | General-purpose cryptography: ciphers, digests, MACs, signatures, key agreement, post-quantum algorithms, TLS and DTLS, OpenPGP, CMS and X.509 |
| [CodeBrix.Docker](docs/libraries/CodeBrix.Docker.md) | A typed, async client for the Docker Engine API over a Unix socket, a Windows named pipe, TCP or an SSH tunnel |
| [CodeBrix.Imaging](docs/libraries/CodeBrix.Imaging.md) | Loads, saves, converts, resizes, transforms, filters, quantizes, composites and annotates raster images, and rasterizes text onto them |
| [CodeBrix.Imaging.Drawing](docs/libraries/CodeBrix.Imaging.Drawing.md) | Captures pointer input as calibrated strokes on named, colored layers and renders them with translucent highlighter compositing |
| [CodeBrix.Json.Extensions](docs/libraries/CodeBrix.Json.Extensions.md) | Adds attribute-driven polymorphic deserialization and object-identity preservation to `System.Text.Json`, using only its public surface |
| [CodeBrix.LilyPort](docs/libraries/CodeBrix.LilyPort.md) | A managed music engraving engine: notation source in, SVG pages and Standard MIDI Files out, entirely in process |
| [CodeBrix.LilyScheme](docs/libraries/CodeBrix.LilyScheme.md) | A managed Scheme implementation: `syntax-case` macro expansion, the module system, the numeric tower, ports and a modern exception API |
| [CodeBrix.MarkupParse](docs/libraries/CodeBrix.MarkupParse.md) | Parses HTML into a navigable DOM tree you query with CSS selectors, traverse, modify and serialize back to HTML |
| [CodeBrix.NotionApi](docs/libraries/CodeBrix.NotionApi.md) | A managed client for the Notion API: every endpoint group, the full object model, and high-level authoring helpers |
| [CodeBrix.PdfDocuments](docs/libraries/CodeBrix.PdfDocuments.md) | A PDF stack that runs from drawing a glyph at an exact coordinate to turning a finished page back into a PNG |
| [CodeBrix.Platform](docs/libraries/CodeBrix.Platform.md) | The cross-platform desktop UI framework: write once against the WinUI XAML API surface, render natively on Windows, Linux and macOS |
| [CodeBrix.Platform.Extensions](docs/libraries/CodeBrix.Platform.Extensions.md) | Functional helpers, a disposables toolkit, collection extensions, comparers, `ILogger` extensions and lock-free threading primitives |
| [CodeBrix.Platform.Fonts.Fluent](docs/libraries/CodeBrix.Platform.Fonts.Fluent.md) | The default symbols (icon) font behind `SymbolIcon`, `FontIcon` and the `SymbolThemeFontFamily` theme resource |
| [CodeBrix.Platform.Fonts.Merriweather](docs/libraries/CodeBrix.Platform.Fonts.Merriweather.md) | The Merriweather serif family, with matching serif companions for the scripts it does not cover |
| [CodeBrix.Platform.Fonts.NotoMusic](docs/libraries/CodeBrix.Platform.Fonts.NotoMusic.md) | A musical-notation symbols font, referenced alongside a text font and never instead of one |
| [CodeBrix.Platform.Fonts.OpenSans](docs/libraries/CodeBrix.Platform.Fonts.OpenSans.md) | The Open Sans variable font plus a curated set of static instances |
| [CodeBrix.Platform.Fonts.Roboto](docs/libraries/CodeBrix.Platform.Fonts.Roboto.md) | The Roboto family, with matching sans companions for the scripts it does not cover |
| [CodeBrix.Platform.Fonts.RobotoMono](docs/libraries/CodeBrix.Platform.Fonts.RobotoMono.md) | The Roboto Mono monospace family plus three companion families, for terminals and code editors |
| [CodeBrix.Platform.GameEngine](docs/libraries/CodeBrix.Platform.GameEngine.md) | A managed 2D / 2.5D game engine: tile maps, sprites, layered scenes, cameras, animation, physics, input, audio and a save system |
| [CodeBrix.Platform.LinuxDBus](docs/libraries/CodeBrix.Platform.LinuxDBus.md) | Low-level D-Bus protocol library for Linux: connect to the session or system bus and send, receive and dispatch raw messages |
| [CodeBrix.Platform.MediaPlayerCore](docs/libraries/CodeBrix.Platform.MediaPlayerCore.md) | Renders video, outputs audio, captures from a webcam and controls playback on Windows, Linux and macOS desktops |
| [CodeBrix.Platform.OpenGL](docs/libraries/CodeBrix.Platform.OpenGL.md) | A managed OpenGL Core-profile binding, with native-library resolution and the vector, matrix and scalar math the signatures need |
| [CodeBrix.Platform.TclTk](docs/libraries/CodeBrix.Platform.TclTk.md) | A managed Tcl interpreter, its `sqlite3` and `pdf4tcl` command extensions, and the classic Tk widget toolkit drawn on SkiaSharp |
| [CodeBrix.Platform.Unicode](docs/libraries/CodeBrix.Platform.Unicode.md) | Delivers the ICU native runtime and its data archive to a .NET application on Windows and on macOS |
| [CodeBrix.Plotter](docs/libraries/CodeBrix.Plotter.md) | Draws charts from a `PlotModel` of axes, series, annotations and legends onto a SkiaSharp canvas, or exports them to PNG, JPEG, PDF or SVG |
| [CodeBrix.PolygonTools](docs/libraries/CodeBrix.PolygonTools.md) | 2D polygon clipping and offsetting: the four boolean operations, and inflate or deflate with miter, round or square joins |
| [CodeBrix.Python](docs/libraries/CodeBrix.Python.md) | Embeds a CPython interpreter in a .NET process, marshals values across the boundary, and lets Python code call .NET APIs |
| [CodeBrix.Redis](docs/libraries/CodeBrix.Redis.md) | A managed Redis client: the RESP protocol, the connection multiplexer, the full command surface, and a distributed lock |
| [CodeBrix.ServiceLocator](docs/libraries/CodeBrix.ServiceLocator.md) | An abstraction over IoC containers: resolve services by type, or by type plus a string key, with no hard reference on a container |
| [CodeBrix.SkiaSvg](docs/libraries/CodeBrix.SkiaSvg.md) | Loads SVG documents and Android VectorDrawable XML and renders them to SkiaSharp canvases, bitmaps and documents, with hit testing and animation |
| [CodeBrix.Sqlite](docs/libraries/CodeBrix.Sqlite.md) | A managed SQLite library that adds selective encryption, a row mapper, blind-index search and safe live backup to an ordinary SQLite file |
| [CodeBrix.SSH](docs/libraries/CodeBrix.SSH.md) | A managed SSH-2 client: remote command execution, an interactive shell, SFTP, SCP, port forwarding and NETCONF |
| [CodeBrix.StyleSheetParse](docs/libraries/CodeBrix.StyleSheetParse.md) | Turns CSS text into a strongly typed object model you can query, manipulate and serialize back to CSS, with selector specificity |
| [CodeBrix.SvgParse](docs/libraries/CodeBrix.SvgParse.md) | A renderer-agnostic SVG document object model: parse, query, style, modify and re-serialize SVG |
| [CodeBrix.Templating](docs/libraries/CodeBrix.Templating.md) | Parses templates, binds them to a .NET object model and renders them to text - code generation, HTML, e-mail bodies, reports, SQL |
| [CodeBrix.Terminal](docs/libraries/CodeBrix.Terminal.md) | An in-memory VT100/VT220/xterm terminal engine: escape-sequence parsing, buffer and scrollback, mouse tracking and Unicode text tools |
| [CodeBrix.TestMocks](docs/libraries/CodeBrix.TestMocks.md) | Mocking, dynamic proxies, auto-generated test data and xUnit v3 data attributes in one package |
| [CodeBrix.Texinfo](docs/libraries/CodeBrix.Texinfo.md) | Turns GNU Texinfo documentation into a formatted PDF, in managed code, with nothing to install on any operating system |
| [CodeBrix.VideoPlayback](docs/libraries/CodeBrix.VideoPlayback.md) | Plays AV1 video from WebM and Matroska and from the `.cbv` container, with Ogg Vorbis or Opus sound and text caption tracks |
| [CodeBrix.VideoPlayback.Dav1d](docs/libraries/CodeBrix.VideoPlayback.Dav1d.md) | AV1 decoding for CodeBrix.VideoPlayback: it bundles the dav1d AV1 decoder and is wired in with one call at start-up |
| [CodeBrix.VideoProcessing](docs/libraries/CodeBrix.VideoProcessing.md) | Analyzes, converts, transcodes and muxes media and pipes raw frames, by driving the external `ffmpeg` and `ffprobe` executables |
| [CodeBrix.VideoProcessing.OpenCV5](docs/libraries/CodeBrix.VideoProcessing.OpenCV5.md) | A managed binding for OpenCV 5: image processing, video capture, camera calibration, object detection, machine learning and the contrib modules |
| [CodeBrix.YamlParse](docs/libraries/CodeBrix.YamlParse.md) | Reads and writes YAML at three levels: object serialization, a document tree, and a constant-memory streaming scanner, parser and emitter |
| [FreePPlus](docs/libraries/FreePPlus.md) | Reads and writes Excel `.xlsx` and `.xlsm` files through the Office Open XML format, with no Office component installed |
| [SilverAssertions](docs/libraries/SilverAssertions.md) | States the expected outcome of a unit test as one readable, chainable expression, and throws your test framework's assertion exception |

The add-in pages - the XAML elements and services these libraries appear as inside a CodeBrix.Platform application - are indexed in [08 - Add-ins](docs/platform/08-add-ins.md).

---

**Where to go next**

- [Build a CodeBrix.Platform application](docs/platform/README.md) - the curriculum, in the order above
- [Blueprints](docs/samples/blueprints.md) - a task-to-recipe index over the reference applications, with the file each recipe came from
- [Repository directory](docs/repo-directory.md) - every repository's `AGENT-README.txt`, tests and samples as direct links
- [Licensing](docs/licensing.md) - what the package-ID suffix guarantees and what the copyleft packages require
