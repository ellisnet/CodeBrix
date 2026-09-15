<sub>[CodeBrix](../../README.md) › Libraries</sub>

# Libraries

**This is the catalog of every CodeBrix library, grouped by topic.** Each row gives what the
library does in a line, the packages it produces and its license, and links to the library's own
page - where you will find its capabilities, its first working example and its pitfalls. Most of
these libraries are usable from any .NET 10 application as well as from a CodeBrix.Platform
application; where one also has a CodeBrix.Platform add-in that wraps it in a XAML element, the
pairing is listed at the end of this page.

## Choosing a package family

The family has three parts, and which one you need depends on the kind of application you are
building.

| What you are building | What to reference |
| --- | --- |
| A cross-platform desktop application (Windows, Linux, and/or macOS) from one shared codebase | The CodeBrix.Platform framework family: your `.Core` library references [`CodeBrix.Platform.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.ApacheLicenseForever) plus any optional add-ins, and each per-platform head executable references exactly one head package |
| A native WinUI 3 / Windows App SDK application | The `CodeBrix.Platform.WinUI.*` toolkit family |
| A WPF application | [`CodeBrix.Platform.WPF.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.WPF.ApacheLicenseForever) |
| A .NET MAUI application | [`CodeBrix.Platform.Mobile.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Mobile.ApacheLicenseForever) |
| Any .NET 10 application that needs a specific capability | The matching library from the catalog below - these are UI-framework-agnostic |

Two rules govern the first four rows:

- Do not mix the two UI-framework families in one application head. A given application head
  (executable) uses one family or the other, never both.
- The two families share an identical "Simple" MVVM API across the Skia-based framework, WinUI,
  WPF and MAUI, so an application shipping the CodeBrix.Platform heads and/or the native WinUI,
  WPF and MAUI heads can share its view models across all of them. The native heads add the
  matching toolkit package to get it; [14 - Sharing code with native frameworks](../platform/14-sharing-code-with-native-frameworks.md)
  shows the pattern end to end.

> [!NOTE]
> Several libraries in the catalog carry a `CodeBrix.Platform.*` name because they exist primarily
> to support the framework - the font packages, the Unicode/ICU packages and
> [CodeBrix.Platform.Extensions](CodeBrix.Platform.Extensions.md). Others with that name -
> [CodeBrix.Platform.MediaPlayerCore](CodeBrix.Platform.MediaPlayerCore.md),
> [CodeBrix.Platform.LinuxDBus](CodeBrix.Platform.LinuxDBus.md) and
> [CodeBrix.Platform.OpenGL](CodeBrix.Platform.OpenGL.md) - are fully usable on their own in any
> .NET 10 application.

Every library on this page requires .NET 10 or later.

## Platform companions

| Library | What it is | Packages | License |
| --- | --- | --- | --- |
| [CodeBrix.Platform](CodeBrix.Platform.md) | Cross-platform desktop UI framework: write once against the WinUI XAML API surface, render natively on Windows, Linux and macOS | [`CodeBrix.Platform.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.ApacheLicenseForever), plus the head, add-in and native-toolkit packages listed below | Apache 2.0, with two exceptions named below |
| [CodeBrix.Platform.Extensions](CodeBrix.Platform.Extensions.md) | Functional helpers, a disposables toolkit, collection extensions, comparers, `ILogger` extensions and lock-free threading primitives in one assembly | [`CodeBrix.Platform.Extensions.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Extensions.ApacheLicenseForever) | Apache 2.0 |
| [CodeBrix.ServiceLocator](CodeBrix.ServiceLocator.md) | An abstraction over IoC containers: resolve services by type, or by type plus a string key, with no hard reference on a container | [`CodeBrix.ServiceLocator.MsplLicenseForever`](https://www.nuget.org/packages/CodeBrix.ServiceLocator.MsplLicenseForever) | Ms-PL |
| [CodeBrix.Platform.Unicode](CodeBrix.Platform.Unicode.md) | Delivers the ICU native runtime and its data archive to a .NET application on Windows and on macOS | [`CodeBrix.Platform.Unicode.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Unicode.ApacheLicenseForever)<br>[`CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever) | Apache 2.0 and Unicode 3.0 |
| [CodeBrix.Platform.LinuxDBus](CodeBrix.Platform.LinuxDBus.md) | Low-level D-Bus protocol library for Linux: connect to the session or system bus and send, receive and dispatch raw messages | [`CodeBrix.Platform.LinuxDBus.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.LinuxDBus.MitLicenseForever) | MIT |

<details>
<summary>Every package produced from the CodeBrix.Platform repository</summary>

The core framework, required by every CodeBrix.Platform application:

- [`CodeBrix.Platform.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.ApacheLicenseForever)

The platform heads - one per head executable:

- [`CodeBrix.Platform.Runtime.Skia.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.ApacheLicenseForever) - the shared Skia runtime every head builds on; application projects never reference it directly
- [`CodeBrix.Platform.Runtime.Skia.Win32.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.Win32.ApacheLicenseForever)
- [`CodeBrix.Platform.Runtime.Skia.Wpf.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.Wpf.ApacheLicenseForever)
- [`CodeBrix.Platform.Runtime.Skia.X11.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.X11.ApacheLicenseForever)
- [`CodeBrix.Platform.Runtime.Skia.Wayland.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.Wayland.ApacheLicenseForever)
- [`CodeBrix.Platform.Runtime.Skia.FrameBuffer.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.FrameBuffer.ApacheLicenseForever)
- [`CodeBrix.Platform.Runtime.Skia.MacOS.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.MacOS.ApacheLicenseForever)
- [`CodeBrix.Platform.Runtime.Skia.FrameBuffer.Emulated.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Runtime.Skia.FrameBuffer.Emulated.ApacheLicenseForever) - an off-screen variant of the frame-buffer head used by tooling; never reference it directly

The add-ins - added once in `.Core`:

- [`CodeBrix.Platform.Graphics2DSK.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Graphics2DSK.ApacheLicenseForever)
- [`CodeBrix.Platform.Graphics3DGL.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Graphics3DGL.ApacheLicenseForever)
- [`CodeBrix.Platform.Lottie.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Lottie.ApacheLicenseForever)
- [`CodeBrix.Platform.Svg.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Svg.ApacheLicenseForever)
- [`CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever)
- [`CodeBrix.Platform.TextLayout.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.TextLayout.ApacheLicenseForever)
- [`CodeBrix.Platform.AdvancedTextEdit.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.AdvancedTextEdit.ApacheLicenseForever)
- [`CodeBrix.Platform.FlexPanel.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.FlexPanel.ApacheLicenseForever)
- [`CodeBrix.Platform.CommandBar.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.CommandBar.ApacheLicenseForever)
- [`CodeBrix.Platform.TerminalView.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.TerminalView.ApacheLicenseForever)
- [`CodeBrix.Platform.PlotterView.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.PlotterView.ApacheLicenseForever)
- [`CodeBrix.Platform.AppSettings.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.AppSettings.ApacheLicenseForever)
- [`CodeBrix.Platform.WebView.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.WebView.ApacheLicenseForever)
- [`CodeBrix.Platform.MediaPlayer.LgplLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.MediaPlayer.LgplLicenseForever)
- [`CodeBrix.Platform.AudioPlayer.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.AudioPlayer.ApacheLicenseForever)
- [`CodeBrix.Platform.VideoPlayer.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.VideoPlayer.ApacheLicenseForever)

The native-framework toolkits, for applications built on Microsoft's own UI frameworks:

- [`CodeBrix.Platform.WinUI.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.WinUI.ApacheLicenseForever)
- [`CodeBrix.Platform.WinUI.Skia.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.WinUI.Skia.ApacheLicenseForever)
- [`CodeBrix.Platform.WinUI.Lottie.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.WinUI.Lottie.ApacheLicenseForever)
- [`CodeBrix.Platform.WPF.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.WPF.ApacheLicenseForever)
- [`CodeBrix.Platform.Mobile.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Mobile.ApacheLicenseForever)

Every one of these is Apache 2.0 except `CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever`
(MIT) and `CodeBrix.Platform.MediaPlayer.LgplLicenseForever` (LGPL-2.1-or-later) - and, as
everywhere in the family, the package ID says which.

</details>

## Fonts

Font packages ship font binaries as build-time content assets. They contain no managed API you
call: you add a `PackageReference` and then reference a font by URI.

| Library | What it is | Packages | License |
| --- | --- | --- | --- |
| [CodeBrix.Platform.Fonts.Fluent](CodeBrix.Platform.Fonts.Fluent.md) | The default symbols (icon) font behind `SymbolIcon`, `FontIcon` and the `SymbolThemeFontFamily` theme resource | [`CodeBrix.Platform.Fonts.Fluent.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.Fluent.ApacheLicenseForever) | Apache 2.0 |
| [CodeBrix.Platform.Fonts.Merriweather](CodeBrix.Platform.Fonts.Merriweather.md) | The Merriweather serif family, with matching serif companions for the scripts it does not cover | [`CodeBrix.Platform.Fonts.Merriweather.OflLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.Merriweather.OflLicenseForever) | SIL OFL 1.1 |
| [CodeBrix.Platform.Fonts.NotoMusic](CodeBrix.Platform.Fonts.NotoMusic.md) | A musical-notation symbols font - clefs, noteheads, rests, accidentals, dynamics - referenced alongside a text font, never instead of one | [`CodeBrix.Platform.Fonts.NotoMusic.OflLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.NotoMusic.OflLicenseForever) | SIL OFL 1.1 |
| [CodeBrix.Platform.Fonts.OpenSans](CodeBrix.Platform.Fonts.OpenSans.md) | The Open Sans variable font plus a curated set of static instances | [`CodeBrix.Platform.Fonts.OpenSans.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.OpenSans.ApacheLicenseForever) | Apache 2.0 and SIL OFL 1.1 |
| [CodeBrix.Platform.Fonts.Roboto](CodeBrix.Platform.Fonts.Roboto.md) | The Roboto family, with matching sans companions for the scripts it does not cover | [`CodeBrix.Platform.Fonts.Roboto.OflLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.Roboto.OflLicenseForever) | SIL OFL 1.1 |
| [CodeBrix.Platform.Fonts.RobotoMono](CodeBrix.Platform.Fonts.RobotoMono.md) | The Roboto Mono monospace family plus three companion families, for terminals and code editors | [`CodeBrix.Platform.Fonts.RobotoMono.OflLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.RobotoMono.OflLicenseForever) | SIL OFL 1.1 |

## SVG, imaging and drawing

| Library | What it is | Packages | License |
| --- | --- | --- | --- |
| [CodeBrix.Imaging](CodeBrix.Imaging.md) | Loads, saves, converts, resizes, transforms, filters, quantizes, composites and annotates raster images, and rasterizes text onto them | [`CodeBrix.Imaging.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Imaging.ApacheLicenseForever) | Apache 2.0 |
| [CodeBrix.Imaging.Drawing](CodeBrix.Imaging.Drawing.md) | Captures pointer input as calibrated strokes on named, colored layers and renders them with translucent highlighter compositing | [`CodeBrix.Imaging.Drawing.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Imaging.Drawing.ApacheLicenseForever)<br>[`CodeBrix.Imaging.Drawing.NoSkia.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Imaging.Drawing.NoSkia.ApacheLicenseForever) | Apache 2.0 |
| [CodeBrix.SkiaSvg](CodeBrix.SkiaSvg.md) | Loads SVG documents and Android VectorDrawable XML and renders them to SkiaSharp canvases, bitmaps and documents, with hit testing and animation | [`CodeBrix.SkiaSvg.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.SkiaSvg.MitLicenseForever) | MIT |
| [CodeBrix.SvgParse](CodeBrix.SvgParse.md) | A renderer-agnostic SVG document object model: parse, query, style, modify and re-serialize SVG | [`CodeBrix.SvgParse.MsplLicenseForever`](https://www.nuget.org/packages/CodeBrix.SvgParse.MsplLicenseForever) | Ms-PL |
| [CodeBrix.PolygonTools](CodeBrix.PolygonTools.md) | 2D polygon clipping and offsetting: the four boolean operations, and inflate or deflate with miter, round or square joins | [`CodeBrix.PolygonTools.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.PolygonTools.MitLicenseForever) | MIT and BSL-1.0 |

## Audio, media core, graphics and games

| Library | What it is | Packages | License |
| --- | --- | --- | --- |
| [CodeBrix.Audio](CodeBrix.Audio.md) | Reads WAV, MP3, Ogg Vorbis, FLAC and AIFF, reads and writes Standard MIDI Files, plays media and sound effects, plays SoundFont, SFZ and Decent Sampler instruments, and plays a multi-track song of recordings and MIDI performances | [`CodeBrix.Audio.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MitLicenseForever) | MIT |
| [CodeBrix.Audio.ModestSynth](CodeBrix.Audio.ModestSynth.md) | Adds synthesis to CodeBrix.Audio: ten oscillator waveforms, seven creative effects, a patch model and a polyphonic synthesizer that plays a patch from MIDI | [`CodeBrix.Audio.ModestSynth.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.ModestSynth.MitLicenseForever) | MIT |
| [CodeBrix.Audio.Opus](CodeBrix.Audio.Opus.md) | Adds Ogg Opus decoding and encoding to CodeBrix.Audio, in pure managed code with no native binaries | [`CodeBrix.Audio.Opus.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.Opus.BsdLicenseForever) | BSD 3-Clause |
| [CodeBrix.Platform.MediaPlayerCore](CodeBrix.Platform.MediaPlayerCore.md) | Renders video, outputs audio, captures from a webcam and controls playback on Windows, Linux and macOS desktops | [`CodeBrix.MediaCore.LgplLicenseForever`](https://www.nuget.org/packages/CodeBrix.MediaCore.LgplLicenseForever)<br>[`CodeBrix.Platform.MediaPlayerCore.LgplLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.MediaPlayerCore.LgplLicenseForever)<br>[`CodeBrix.Webcam.LgplLicenseForever`](https://www.nuget.org/packages/CodeBrix.Webcam.LgplLicenseForever) | LGPL-2.1-or-later |
| [CodeBrix.Platform.OpenGL](CodeBrix.Platform.OpenGL.md) | A managed OpenGL Core-profile binding, with the native-library resolution and the vector, matrix and scalar math the signatures need | [`CodeBrix.Platform.OpenGL.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.OpenGL.MitLicenseForever) | MIT |
| [CodeBrix.Platform.GameEngine](CodeBrix.Platform.GameEngine.md) | A managed 2D / 2.5D game engine: tile maps, sprites, layered scenes, cameras, animation, physics, input, audio and a save system | [`CodeBrix.Platform.GameEngine.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.GameEngine.MitLicenseForever)<br>[`CodeBrix.Platform.GameEngine.Sdl2.ZlibLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.GameEngine.Sdl2.ZlibLicenseForever) | MIT; the gamepad package is MIT and zlib |

## Video playback, processing and vision

| Library | What it is | Packages | License |
| --- | --- | --- | --- |
| [CodeBrix.VideoPlayback](CodeBrix.VideoPlayback.md) | Plays AV1 video from WebM and Matroska and from the `.cbv` container, with Ogg Vorbis or Opus sound and text caption tracks | [`CodeBrix.VideoPlayback.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoPlayback.MitLicenseForever)<br>[`CodeBrix.VideoPlayback.Skia.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoPlayback.Skia.MitLicenseForever)<br>[`CodeBrix.VideoPlayback.Authoring.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoPlayback.Authoring.MitLicenseForever) | MIT |
| [CodeBrix.VideoPlayback.Dav1d](CodeBrix.VideoPlayback.Dav1d.md) | AV1 decoding for CodeBrix.VideoPlayback: it bundles the dav1d AV1 decoder and is wired in with one call at start-up | [`CodeBrix.VideoPlayback.Dav1d.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoPlayback.Dav1d.BsdLicenseForever) | BSD 2-Clause |
| [CodeBrix.VideoProcessing](CodeBrix.VideoProcessing.md) | Analyzes, converts, transcodes and muxes media, extracts snapshots and pipes raw frames, by driving the external `ffmpeg` and `ffprobe` executables | [`CodeBrix.VideoProcessing.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.MitLicenseForever) | MIT |
| [CodeBrix.VideoProcessing.OpenCV5](CodeBrix.VideoProcessing.OpenCV5.md) | A managed binding for OpenCV 5: image processing, video capture, camera calibration, object detection, machine learning and the contrib modules | [`CodeBrix.VideoProcessing.OpenCV5.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.OpenCV5.ApacheLicenseForever), a WPF bridge package and one native runtime package per OS and CPU pair - see the library page | Apache 2.0 |

## Documents, PDF and spreadsheets

| Library | What it is | Packages | License |
| --- | --- | --- | --- |
| [CodeBrix.PdfDocuments](CodeBrix.PdfDocuments.md) | A PDF stack that runs from drawing a glyph at an exact coordinate to turning a finished page back into a PNG | [`CodeBrix.PdfDocuments.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.PdfDocuments.MitLicenseForever)<br>[`CodeBrix.PdfDocCreate.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.PdfDocCreate.MitLicenseForever)<br>[`CodeBrix.PdfDocCreate.Html2Pdf.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.PdfDocCreate.Html2Pdf.MitLicenseForever)<br>[`CodeBrix.PdfDocCreate.Markdown2Pdf.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.PdfDocCreate.Markdown2Pdf.MitLicenseForever)<br>[`CodeBrix.PdfRasterizer.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.PdfRasterizer.MitLicenseForever) | MIT |
| [CodeBrix.Texinfo](CodeBrix.Texinfo.md) | Turns GNU Texinfo documentation into a formatted PDF, in managed code, with nothing to install on any operating system | [`CodeBrix.Texinfo2Html.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Texinfo2Html.MitLicenseForever)<br>[`CodeBrix.Texinfo2Pdf.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Texinfo2Pdf.MitLicenseForever) | MIT |
| [CodeBrix.Templating](CodeBrix.Templating.md) | Parses templates, binds them to a .NET object model and renders them to text - code generation, HTML, e-mail bodies, reports, SQL | [`CodeBrix.Templating.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.Templating.BsdLicenseForever) | BSD 2-Clause |
| [FreePPlus](FreePPlus.md) | Reads and writes Excel `.xlsx` and `.xlsm` files through the Office Open XML format, with no Office component installed | [`FreePPlus.LgplLicenseForever`](https://www.nuget.org/packages/FreePPlus.LgplLicenseForever) | LGPL-3.0-or-later |
| [CodeBrix.NotionApi](CodeBrix.NotionApi.md) | A managed client for the Notion API: every endpoint group, the full object model, and high-level authoring helpers | [`CodeBrix.NotionApi.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.NotionApi.MitLicenseForever) | MIT |

## Parsing and compression

| Library | What it is | Packages | License |
| --- | --- | --- | --- |
| [CodeBrix.MarkupParse](CodeBrix.MarkupParse.md) | Parses HTML into a navigable DOM tree you query with CSS selectors, traverse, modify and serialize back to HTML | [`CodeBrix.MarkupParse.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.MarkupParse.MitLicenseForever) | MIT |
| [CodeBrix.StyleSheetParse](CodeBrix.StyleSheetParse.md) | Turns CSS text into a strongly typed object model you can query, manipulate and serialize back to CSS, with selector specificity | [`CodeBrix.StyleSheetParse.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.StyleSheetParse.MitLicenseForever) | MIT |
| [CodeBrix.YamlParse](CodeBrix.YamlParse.md) | Reads and writes YAML at three levels: object serialization, a document tree, and a constant-memory streaming scanner, parser and emitter | [`CodeBrix.YamlParse.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.YamlParse.MitLicenseForever) | MIT |
| [CodeBrix.Json.Extensions](CodeBrix.Json.Extensions.md) | Adds attribute-driven polymorphic deserialization and object-identity preservation to `System.Text.Json`, using only its public surface | [`CodeBrix.Json.Extensions.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Json.Extensions.MitLicenseForever) | MIT |
| [CodeBrix.Compression](CodeBrix.Compression.md) | Creates, reads and extracts Zip, GZip, Tar and BZip2 archives, with AES encryption and Zip64, and decompresses legacy DCL and `.Z` data | [`CodeBrix.Compression.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Compression.MitLicenseForever) | MIT |

## Data, storage, networking and services

| Library | What it is | Packages | License |
| --- | --- | --- | --- |
| [CodeBrix.Sqlite](CodeBrix.Sqlite.md) | A managed SQLite library that adds selective encryption, a row mapper, blind-index search and safe live backup to an ordinary SQLite file | [`CodeBrix.Sqlite.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Sqlite.ApacheLicenseForever) | Apache 2.0 |
| [CodeBrix.Redis](CodeBrix.Redis.md) | A managed Redis client: the RESP protocol, the connection multiplexer with cluster, sentinel and replica awareness, the full command surface, and a distributed lock | [`CodeBrix.Redis.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Redis.MitLicenseForever) | MIT |
| [CodeBrix.Docker](CodeBrix.Docker.md) | A typed, async client for the Docker Engine API over a Unix socket, a Windows named pipe, TCP or an SSH tunnel | [`CodeBrix.Docker.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Docker.MitLicenseForever) | MIT |
| [CodeBrix.SSH](CodeBrix.SSH.md) | A managed SSH-2 client: remote command execution, an interactive shell, SFTP, SCP, port forwarding and NETCONF | [`CodeBrix.SSH.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.SSH.MitLicenseForever) | MIT |
| [CodeBrix.Cryptography](CodeBrix.Cryptography.md) | General-purpose cryptography: ciphers, digests, MACs, signatures, key agreement, post-quantum algorithms, TLS and DTLS, OpenPGP, CMS and X.509 | [`CodeBrix.Cryptography.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Cryptography.MitLicenseForever) | MIT |

## Music notation, Scheme, scripting and terminals

| Library | What it is | Packages | License |
| --- | --- | --- | --- |
| [CodeBrix.LilyPort](CodeBrix.LilyPort.md) | A managed music engraving engine: notation source in, SVG pages and Standard MIDI Files out, entirely in process | [`CodeBrix.LilyPort.GplLicenseForever`](https://www.nuget.org/packages/CodeBrix.LilyPort.GplLicenseForever) | GPL-3.0-only |
| [CodeBrix.LilyScheme](CodeBrix.LilyScheme.md) | A managed Scheme implementation: `syntax-case` macro expansion, the module system, the numeric tower, ports and a modern exception API | [`CodeBrix.LilyScheme.LgplLicenseForever`](https://www.nuget.org/packages/CodeBrix.LilyScheme.LgplLicenseForever) | LGPL-3.0-or-later |
| [CodeBrix.Platform.TclTk](CodeBrix.Platform.TclTk.md) | A managed Tcl interpreter, its `sqlite3` and `pdf4tcl` command extensions, and the classic Tk widget toolkit drawn on SkiaSharp | [`CodeBrix.Platform.TclTk.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.TclTk.BsdLicenseForever)<br>[`CodeBrix.Platform.TclTk.Extras.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.TclTk.Extras.BsdLicenseForever)<br>[`CodeBrix.Platform.TkCanvas.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.TkCanvas.BsdLicenseForever) | BSD 2-Clause |
| [CodeBrix.Python](CodeBrix.Python.md) | Embeds a CPython interpreter in a .NET process, marshals values across the boundary, and lets Python code call .NET APIs | [`CodeBrix.Python.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Python.MitLicenseForever) | MIT |
| [CodeBrix.Terminal](CodeBrix.Terminal.md) | An in-memory VT100/VT220/xterm terminal engine: escape-sequence parsing, buffer and scrollback, mouse tracking, input encoding and Unicode text tools | [`CodeBrix.Terminal.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Terminal.MitLicenseForever) | MIT |

## Testing, command line, assemblies and charts

| Library | What it is | Packages | License |
| --- | --- | --- | --- |
| [SilverAssertions](SilverAssertions.md) | States the expected outcome of a unit test as one readable, chainable expression, and throws your own test framework's assertion exception | [`SilverAssertions.ApacheLicenseForever`](https://www.nuget.org/packages/SilverAssertions.ApacheLicenseForever) | Apache 2.0 |
| [CodeBrix.TestMocks](CodeBrix.TestMocks.md) | Mocking, dynamic proxies, auto-generated test data and xUnit v3 data attributes in one package | [`CodeBrix.TestMocks.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.TestMocks.ApacheLicenseForever) | Apache 2.0 |
| [CodeBrix.ArgumentParser](CodeBrix.ArgumentParser.md) | Turns a `string[] args` into invocations of callbacks you register, generates the matching `--help` text, and models whole sub-command suites | [`CodeBrix.ArgumentParser.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.ArgumentParser.MitLicenseForever) | MIT |
| [CodeBrix.AssemblyTools](CodeBrix.AssemblyTools.md) | Reads, writes and rewrites .NET assemblies: IL, metadata and debug symbols, in a single managed assembly | [`CodeBrix.AssemblyTools.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.AssemblyTools.MitLicenseForever) | MIT |
| [CodeBrix.Plotter](CodeBrix.Plotter.md) | Draws charts, graphs and plots from a `PlotModel` of axes, series, annotations and legends onto a SkiaSharp canvas, or exports them to PNG, JPEG, PDF or SVG | [`CodeBrix.Plotter.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Plotter.MitLicenseForever) | MIT |

## Platform add-ins that host these libraries

An add-in is a package you reference once in your `.Core` library; it brings a XAML element or a
service, and it brings the standalone library underneath it as an automatic dependency. Use the
add-in when the capability belongs on a page, and the library directly when it does not.

| Add-in | Package | Library underneath |
| --- | --- | --- |
| [Svg](../platform/add-ins/Svg.md) | `CodeBrix.Platform.Svg.ApacheLicenseForever` | [CodeBrix.SkiaSvg](CodeBrix.SkiaSvg.md) |
| [TerminalView](../platform/add-ins/TerminalView.md) | `CodeBrix.Platform.TerminalView.ApacheLicenseForever` | [CodeBrix.Terminal](CodeBrix.Terminal.md) |
| [PlotterView](../platform/add-ins/PlotterView.md) | `CodeBrix.Platform.PlotterView.ApacheLicenseForever` | [CodeBrix.Plotter](CodeBrix.Plotter.md) |
| [AppSettings](../platform/add-ins/AppSettings.md) | `CodeBrix.Platform.AppSettings.ApacheLicenseForever` | [CodeBrix.Sqlite](CodeBrix.Sqlite.md) |
| [AudioPlayer](../platform/add-ins/AudioPlayer.md) | `CodeBrix.Platform.AudioPlayer.ApacheLicenseForever` | [CodeBrix.Audio](CodeBrix.Audio.md); add [CodeBrix.Audio.Opus](CodeBrix.Audio.Opus.md) yourself for `.opus`, and [CodeBrix.Audio.ModestSynth](CodeBrix.Audio.ModestSynth.md) for oscillator groups in a sampler preset |
| [VideoPlayer](../platform/add-ins/VideoPlayer.md) | `CodeBrix.Platform.VideoPlayer.ApacheLicenseForever` | [CodeBrix.VideoPlayback](CodeBrix.VideoPlayback.md); add [CodeBrix.VideoPlayback.Dav1d](CodeBrix.VideoPlayback.Dav1d.md) yourself for AV1 |
| [MediaPlayer](../platform/add-ins/MediaPlayer.md) | `CodeBrix.Platform.MediaPlayer.LgplLicenseForever` | [CodeBrix.Platform.MediaPlayerCore](CodeBrix.Platform.MediaPlayerCore.md) |
| [Graphics3DGL](../platform/add-ins/Graphics3DGL.md) | `CodeBrix.Platform.Graphics3DGL.ApacheLicenseForever` | [CodeBrix.Platform.OpenGL](CodeBrix.Platform.OpenGL.md) |
| [TextLayout](../platform/add-ins/TextLayout.md) | `CodeBrix.Platform.TextLayout.ApacheLicenseForever` | [CodeBrix.Platform.Unicode](CodeBrix.Platform.Unicode.md) supplies the ICU natives it needs |
| [AdvancedTextEdit](../platform/add-ins/AdvancedTextEdit.md) | `CodeBrix.Platform.AdvancedTextEdit.ApacheLicenseForever` | None; it builds on the TextLayout add-in |
| [CommandBar](../platform/add-ins/CommandBar.md) | `CodeBrix.Platform.CommandBar.ApacheLicenseForever` | None; it brings the Svg add-in, and through it [CodeBrix.SkiaSvg](CodeBrix.SkiaSvg.md), to draw vector icons |
| [Graphics2DSK](../platform/add-ins/Graphics2DSK.md) | `CodeBrix.Platform.Graphics2DSK.ApacheLicenseForever` | None |
| [SkiaSharpViews](../platform/add-ins/SkiaSharpViews.md) | `CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever` | None |
| [Lottie](../platform/add-ins/Lottie.md) | `CodeBrix.Platform.Lottie.ApacheLicenseForever` | None |
| [FlexPanel](../platform/add-ins/FlexPanel.md) | `CodeBrix.Platform.FlexPanel.ApacheLicenseForever` | None |
| [WebView](../platform/add-ins/WebView.md) | `CodeBrix.Platform.WebView.ApacheLicenseForever` | None; on Linux it drives the system-installed WPE WebKit engine |

---

**Where to go next**

- [Build a CodeBrix.Platform application](../platform/README.md) - the curriculum that puts these libraries to work
- [Add-ins](../platform/08-add-ins.md) - every add-in at a glance, with what each one adds to a page
- [Repository directory](../repo-directory.md) - each repository's source, tests, samples and documents
- [Licensing](../licensing.md) - the license families and what the package-ID suffix guarantees
