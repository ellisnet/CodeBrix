<sub>[CodeBrix](../README.md) › Repository directory</sub>

# Repository directory

**Every CodeBrix repository with its packages, its documents, its tests and its samples.**
Use this page when you know the name of a thing and want the source, the test that exercises
it, or the guide that ships inside its package. The library repositories come first, then the
three sample-application repositories; both lists are alphabetical, and every link points at
branch `main`.

Each entry follows the same shape. Every library repository carries these documents at its
root:

| Document | What it is |
| --- | --- |
| `README.md` | The human-facing overview, shown on GitHub and on nuget.org |
| `AGENT-README.txt` | The complete API reference and usage guide, written for AI coding agents - one per package, and it ships inside the package too |
| `README-INDEX.txt` | The map of every document in the repository |
| `THIRD-PARTY-NOTICES.txt` | The provenance and licensing record for the open source code the packages include |

> [!TIP]
> A repository's test project is its worked-example set. Each `AGENT-README.txt` names the test
> file that demonstrates each feature area, so "how do I do X" is answered by the test file
> named for X.

## Library repositories

### CodeBrix.ArgumentParser

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.ArgumentParser](https://github.com/ellisnet/CodeBrix.ArgumentParser) |
| **Packages** | [`CodeBrix.ArgumentParser.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.ArgumentParser.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.ArgumentParser/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.ArgumentParser/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.ArgumentParser/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.ArgumentParser/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.ArgumentParser/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.ArgumentParser](libraries/CodeBrix.ArgumentParser.md) |

### CodeBrix.AssemblyTools

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.AssemblyTools](https://github.com/ellisnet/CodeBrix.AssemblyTools) |
| **Packages** | [`CodeBrix.AssemblyTools.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.AssemblyTools.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.AssemblyTools/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.AssemblyTools/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.AssemblyTools/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.AssemblyTools/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.AssemblyTools/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.AssemblyTools](libraries/CodeBrix.AssemblyTools.md) |

### CodeBrix.Audio

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Audio](https://github.com/ellisnet/CodeBrix.Audio) |
| **Packages** | [`CodeBrix.Audio.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.MitLicenseForever) - ships the `CodeBrix.Audio` and `CodeBrix.Audio.Engine` assemblies<br>[`CodeBrix.Audio.ModestSynth.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.ModestSynth.MitLicenseForever) - the synthesis add-on, built and published from this repository at the same version |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Audio/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/THIRD-PARTY-NOTICES.txt) · the add-on's [README.md](https://github.com/ellisnet/CodeBrix.Audio/blob/main/src/CodeBrix.Audio.ModestSynth/README.md) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/AGENT-README.txt) for the main package · [src/CodeBrix.Audio.ModestSynth/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio/blob/main/src/CodeBrix.Audio.ModestSynth/AGENT-README.txt) for the add-on |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tests) |
| **Samples and tools** | [tools](https://github.com/ellisnet/CodeBrix.Audio/tree/main/tools) |
| **This site** | [CodeBrix.Audio](libraries/CodeBrix.Audio.md) and its [Audio section](libraries/audio/reading-and-writing-files.md) · [CodeBrix.Audio.ModestSynth](libraries/CodeBrix.Audio.ModestSynth.md) |

### CodeBrix.Audio.Opus

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Audio.Opus](https://github.com/ellisnet/CodeBrix.Audio.Opus) |
| **Packages** | [`CodeBrix.Audio.Opus.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.Audio.Opus.BsdLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Audio.Opus/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Audio.Opus/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Audio.Opus/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Audio.Opus/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Audio.Opus/tree/main/tests) |
| **Samples and tools** | [tools](https://github.com/ellisnet/CodeBrix.Audio.Opus/tree/main/tools) |
| **This site** | [CodeBrix.Audio.Opus](libraries/CodeBrix.Audio.Opus.md) |

### CodeBrix.Compression

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Compression](https://github.com/ellisnet/CodeBrix.Compression) |
| **Packages** | [`CodeBrix.Compression.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Compression.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Compression/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Compression/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Compression/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Compression/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Compression/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.Compression](libraries/CodeBrix.Compression.md) |

### CodeBrix.Cryptography

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Cryptography](https://github.com/ellisnet/CodeBrix.Cryptography) |
| **Packages** | [`CodeBrix.Cryptography.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Cryptography.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Cryptography/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Cryptography/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Cryptography/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Cryptography/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Cryptography/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.Cryptography](libraries/CodeBrix.Cryptography.md) |

### CodeBrix.Docker

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Docker](https://github.com/ellisnet/CodeBrix.Docker) |
| **Packages** | [`CodeBrix.Docker.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Docker.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Docker/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Docker/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Docker/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Docker/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Docker/tree/main/tests) |
| **Samples and tools** | [samples/RedisSetupTool](https://github.com/ellisnet/CodeBrix.Docker/tree/main/samples/RedisSetupTool) |
| **This site** | [CodeBrix.Docker](libraries/CodeBrix.Docker.md) |

### CodeBrix.Imaging

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Imaging](https://github.com/ellisnet/CodeBrix.Imaging) |
| **Packages** | [`CodeBrix.Imaging.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Imaging.ApacheLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Imaging/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Imaging/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Imaging/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Imaging/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Imaging/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.Imaging](libraries/CodeBrix.Imaging.md) |

### CodeBrix.Imaging.Drawing

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Imaging.Drawing](https://github.com/ellisnet/CodeBrix.Imaging.Drawing) |
| **Packages** | [`CodeBrix.Imaging.Drawing.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Imaging.Drawing.ApacheLicenseForever)<br>[`CodeBrix.Imaging.Drawing.NoSkia.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Imaging.Drawing.NoSkia.ApacheLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Imaging.Drawing/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Imaging.Drawing/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Imaging.Drawing/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README-SKIA.txt](https://github.com/ellisnet/CodeBrix.Imaging.Drawing/blob/main/AGENT-README-SKIA.txt) · [AGENT-README-NOSKIA.txt](https://github.com/ellisnet/CodeBrix.Imaging.Drawing/blob/main/AGENT-README-NOSKIA.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Imaging.Drawing/tree/main/tests) |
| **Samples and tools** | [samples/PainDiagram](https://github.com/ellisnet/CodeBrix.Imaging.Drawing/tree/main/samples/PainDiagram) |
| **This site** | [CodeBrix.Imaging.Drawing](libraries/CodeBrix.Imaging.Drawing.md) |

### CodeBrix.Json.Extensions

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Json.Extensions](https://github.com/ellisnet/CodeBrix.Json.Extensions) |
| **Packages** | [`CodeBrix.Json.Extensions.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Json.Extensions.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Json.Extensions/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Json.Extensions/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Json.Extensions/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Json.Extensions/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Json.Extensions/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.Json.Extensions](libraries/CodeBrix.Json.Extensions.md) |

### CodeBrix.LilyPort

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.LilyPort](https://github.com/ellisnet/CodeBrix.LilyPort) |
| **Packages** | [`CodeBrix.LilyPort.GplLicenseForever`](https://www.nuget.org/packages/CodeBrix.LilyPort.GplLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.LilyPort/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.LilyPort/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.LilyPort/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.LilyPort/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.LilyPort/tree/main/tests) |
| **Samples and tools** | [tools](https://github.com/ellisnet/CodeBrix.LilyPort/tree/main/tools) |
| **This site** | [CodeBrix.LilyPort](libraries/CodeBrix.LilyPort.md) |

### CodeBrix.LilyScheme

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.LilyScheme](https://github.com/ellisnet/CodeBrix.LilyScheme) |
| **Packages** | [`CodeBrix.LilyScheme.LgplLicenseForever`](https://www.nuget.org/packages/CodeBrix.LilyScheme.LgplLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.LilyScheme/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.LilyScheme/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.LilyScheme/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.LilyScheme/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.LilyScheme/tree/main/tests) |
| **Samples and tools** | [tools/unicode-names](https://github.com/ellisnet/CodeBrix.LilyScheme/tree/main/tools/unicode-names) |
| **This site** | [CodeBrix.LilyScheme](libraries/CodeBrix.LilyScheme.md) |

### CodeBrix.MarkupParse

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.MarkupParse](https://github.com/ellisnet/CodeBrix.MarkupParse) |
| **Packages** | [`CodeBrix.MarkupParse.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.MarkupParse.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.MarkupParse/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.MarkupParse/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.MarkupParse/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.MarkupParse/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.MarkupParse/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.MarkupParse](libraries/CodeBrix.MarkupParse.md) |

### CodeBrix.NotionApi

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.NotionApi](https://github.com/ellisnet/CodeBrix.NotionApi) |
| **Packages** | [`CodeBrix.NotionApi.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.NotionApi.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.NotionApi/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.NotionApi/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.NotionApi/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.NotionApi/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.NotionApi/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.NotionApi](libraries/CodeBrix.NotionApi.md) |

### CodeBrix.PdfDocuments

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.PdfDocuments](https://github.com/ellisnet/CodeBrix.PdfDocuments) |
| **Packages** | [`CodeBrix.PdfDocuments.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.PdfDocuments.MitLicenseForever)<br>[`CodeBrix.PdfDocCreate.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.PdfDocCreate.MitLicenseForever)<br>[`CodeBrix.PdfDocCreate.Html2Pdf.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.PdfDocCreate.Html2Pdf.MitLicenseForever)<br>[`CodeBrix.PdfDocCreate.Markdown2Pdf.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.PdfDocCreate.Markdown2Pdf.MitLicenseForever)<br>[`CodeBrix.PdfRasterizer.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.PdfRasterizer.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.PdfDocuments/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.PdfDocuments/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.PdfDocuments/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.PdfDocuments/blob/main/AGENT-README.txt) · [src/CodeBrix.PdfDocCreate/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.PdfDocuments/blob/main/src/CodeBrix.PdfDocCreate/AGENT-README.txt) · [src/CodeBrix.PdfRasterizer/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.PdfDocuments/blob/main/src/CodeBrix.PdfRasterizer/AGENT-README.txt) · [src/CodeBrix.PdfDocCreate.Html2Pdf/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.PdfDocuments/blob/main/src/CodeBrix.PdfDocCreate.Html2Pdf/AGENT-README.txt) · [src/CodeBrix.PdfDocCreate.Markdown2Pdf/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.PdfDocuments/blob/main/src/CodeBrix.PdfDocCreate.Markdown2Pdf/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.PdfDocuments/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.PdfDocuments](libraries/CodeBrix.PdfDocuments.md) |

### CodeBrix.Platform

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Platform](https://github.com/ellisnet/CodeBrix.Platform) |
| **Packages** | The framework, head, add-in and native-toolkit packages - listed in full in the [library catalog](libraries/README.md#platform-companions) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Platform/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/README-INDEX.txt) · [CODEBRIX-PLATFORM-README.md](https://github.com/ellisnet/CodeBrix.Platform/blob/main/CODEBRIX-PLATFORM-README.md) · [NOT-IMPLEMENTED.md](https://github.com/ellisnet/CodeBrix.Platform/blob/main/NOT-IMPLEMENTED.md) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | One per package family - see the list below |
| **Tests** | [src/Platform.UI.RuntimeTests](https://github.com/ellisnet/CodeBrix.Platform/tree/main/src/Platform.UI.RuntimeTests) · [src/Platform.UI.Toolkit.Tests](https://github.com/ellisnet/CodeBrix.Platform/tree/main/src/Platform.UI.Toolkit.Tests) |
| **Samples and tools** | [samples/CodeBrixPlatform](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform) · [samples/Platforms](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/Platforms) · [tools](https://github.com/ellisnet/CodeBrix.Platform/tree/main/tools) |
| **This site** | [CodeBrix.Platform](libraries/CodeBrix.Platform.md) |

<details>
<summary>Every AGENT-README.txt in the CodeBrix.Platform repository</summary>

- [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/AGENT-README.txt) - the core framework and every platform head
- [src/AddIns/Platform.WinUI.Graphics2DSK/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.WinUI.Graphics2DSK/AGENT-README.txt) - Graphics2DSK
- [src/AddIns/Platform.WinUI.Graphics3DGL/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.WinUI.Graphics3DGL/AGENT-README.txt) - Graphics3DGL
- [src/AddIns/Platform.UI.Lottie/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.Lottie/AGENT-README.txt) - Lottie
- [src/AddIns/Platform.UI.Svg/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.Svg/AGENT-README.txt) - Svg
- [src/AddIns/CodeBrix.Platform.SkiaSharp.Views/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/CodeBrix.Platform.SkiaSharp.Views/AGENT-README.txt) - SkiaSharp.Views
- [src/AddIns/Platform.UI.TextLayout/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.TextLayout/AGENT-README.txt) - TextLayout
- [src/AddIns/Platform.UI.AdvancedTextEdit/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.AdvancedTextEdit/AGENT-README.txt) - AdvancedTextEdit
- [src/AddIns/Platform.UI.FlexPanel/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.FlexPanel/AGENT-README.txt) - FlexPanel
- [src/AddIns/Platform.UI.CommandBar/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.CommandBar/AGENT-README.txt) - CommandBar
- [src/AddIns/Platform.UI.TerminalView/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.TerminalView/AGENT-README.txt) - TerminalView
- [src/AddIns/Platform.UI.PlotterView/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.PlotterView/AGENT-README.txt) - PlotterView
- [src/AddIns/Platform.AppSettings/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.AppSettings/AGENT-README.txt) - AppSettings
- [src/AddIns/Platform.UI.WebView.Skia/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.WebView.Skia/AGENT-README.txt) - WebView
- [src/AddIns/Platform.UI.MediaPlayer.Skia/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.MediaPlayer.Skia/AGENT-README.txt) - MediaPlayer
- [src/AddIns/Platform.UI.AudioPlayer.Skia/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.AudioPlayer.Skia/AGENT-README.txt) - AudioPlayer
- [src/AddIns/Platform.UI.VideoPlayer.Skia/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.VideoPlayer.Skia/AGENT-README.txt) - VideoPlayer
- [src-platforms/Platform.WinUI/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src-platforms/Platform.WinUI/AGENT-README.txt) - the WinUI toolkit family
- [src-platforms/Platform.WPF/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src-platforms/Platform.WPF/AGENT-README.txt) - the WPF toolkit
- [src-platforms/Platform.Mobile/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src-platforms/Platform.Mobile/AGENT-README.txt) - the MAUI toolkit

</details>

The curriculum for this repository is the [CodeBrix.Platform guide](platform/README.md), and every add-in has its own page under [Add-ins](platform/08-add-ins.md).

### CodeBrix.Platform.Extensions

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Platform.Extensions](https://github.com/ellisnet/CodeBrix.Platform.Extensions) |
| **Packages** | [`CodeBrix.Platform.Extensions.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Extensions.ApacheLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Platform.Extensions/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform.Extensions/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.Extensions/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.Extensions/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Platform.Extensions/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.Platform.Extensions](libraries/CodeBrix.Platform.Extensions.md) |

### CodeBrix.Platform.Fonts.Fluent

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Platform.Fonts.Fluent](https://github.com/ellisnet/CodeBrix.Platform.Fonts.Fluent) |
| **Packages** | [`CodeBrix.Platform.Fonts.Fluent.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.Fluent.ApacheLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Platform.Fonts.Fluent/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.Fluent/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.Fluent/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.Fluent/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Platform.Fonts.Fluent/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.Platform.Fonts.Fluent](libraries/CodeBrix.Platform.Fonts.Fluent.md) |

### CodeBrix.Platform.Fonts.Merriweather

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Platform.Fonts.Merriweather](https://github.com/ellisnet/CodeBrix.Platform.Fonts.Merriweather) |
| **Packages** | [`CodeBrix.Platform.Fonts.Merriweather.OflLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.Merriweather.OflLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Platform.Fonts.Merriweather/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.Merriweather/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.Merriweather/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.Merriweather/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Platform.Fonts.Merriweather/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.Platform.Fonts.Merriweather](libraries/CodeBrix.Platform.Fonts.Merriweather.md) |

### CodeBrix.Platform.Fonts.NotoMusic

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Platform.Fonts.NotoMusic](https://github.com/ellisnet/CodeBrix.Platform.Fonts.NotoMusic) |
| **Packages** | [`CodeBrix.Platform.Fonts.NotoMusic.OflLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.NotoMusic.OflLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Platform.Fonts.NotoMusic/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.NotoMusic/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.NotoMusic/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.NotoMusic/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Platform.Fonts.NotoMusic/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.Platform.Fonts.NotoMusic](libraries/CodeBrix.Platform.Fonts.NotoMusic.md) |

### CodeBrix.Platform.Fonts.OpenSans

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Platform.Fonts.OpenSans](https://github.com/ellisnet/CodeBrix.Platform.Fonts.OpenSans) |
| **Packages** | [`CodeBrix.Platform.Fonts.OpenSans.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.OpenSans.ApacheLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Platform.Fonts.OpenSans/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.OpenSans/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.OpenSans/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.OpenSans/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Platform.Fonts.OpenSans/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.Platform.Fonts.OpenSans](libraries/CodeBrix.Platform.Fonts.OpenSans.md) |

### CodeBrix.Platform.Fonts.Roboto

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Platform.Fonts.Roboto](https://github.com/ellisnet/CodeBrix.Platform.Fonts.Roboto) |
| **Packages** | [`CodeBrix.Platform.Fonts.Roboto.OflLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.Roboto.OflLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Platform.Fonts.Roboto/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.Roboto/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.Roboto/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.Roboto/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Platform.Fonts.Roboto/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.Platform.Fonts.Roboto](libraries/CodeBrix.Platform.Fonts.Roboto.md) |

### CodeBrix.Platform.Fonts.RobotoMono

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Platform.Fonts.RobotoMono](https://github.com/ellisnet/CodeBrix.Platform.Fonts.RobotoMono) |
| **Packages** | [`CodeBrix.Platform.Fonts.RobotoMono.OflLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.RobotoMono.OflLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Platform.Fonts.RobotoMono/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.RobotoMono/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.RobotoMono/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.RobotoMono/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Platform.Fonts.RobotoMono/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.Platform.Fonts.RobotoMono](libraries/CodeBrix.Platform.Fonts.RobotoMono.md) |

### CodeBrix.Platform.GameEngine

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Platform.GameEngine](https://github.com/ellisnet/CodeBrix.Platform.GameEngine) |
| **Packages** | [`CodeBrix.Platform.GameEngine.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.GameEngine.MitLicenseForever)<br>[`CodeBrix.Platform.GameEngine.Sdl2.ZlibLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.GameEngine.Sdl2.ZlibLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/blob/main/AGENT-README.txt) · [src/CodeBrix.Platform.GameEngine.Sdl2/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/blob/main/src/CodeBrix.Platform.GameEngine.Sdl2/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/tree/main/tests) |
| **Samples and tools** | [samples](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/tree/main/samples) · [tools](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/tree/main/tools) |
| **This site** | [CodeBrix.Platform.GameEngine](libraries/CodeBrix.Platform.GameEngine.md) |

### CodeBrix.Platform.LinuxDBus

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Platform.LinuxDBus](https://github.com/ellisnet/CodeBrix.Platform.LinuxDBus) |
| **Packages** | [`CodeBrix.Platform.LinuxDBus.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.LinuxDBus.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Platform.LinuxDBus/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform.LinuxDBus/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.LinuxDBus/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.LinuxDBus/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Platform.LinuxDBus/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.Platform.LinuxDBus](libraries/CodeBrix.Platform.LinuxDBus.md) |

### CodeBrix.Platform.MediaPlayerCore

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Platform.MediaPlayerCore](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore) |
| **Packages** | [`CodeBrix.MediaCore.LgplLicenseForever`](https://www.nuget.org/packages/CodeBrix.MediaCore.LgplLicenseForever)<br>[`CodeBrix.Platform.MediaPlayerCore.LgplLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.MediaPlayerCore.LgplLicenseForever)<br>[`CodeBrix.Webcam.LgplLicenseForever`](https://www.nuget.org/packages/CodeBrix.Webcam.LgplLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore/blob/main/AGENT-README.txt) · [src/CodeBrix.Platform.MediaPlayerCore/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore/blob/main/src/CodeBrix.Platform.MediaPlayerCore/AGENT-README.txt) · [src/CodeBrix.Webcam/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore/blob/main/src/CodeBrix.Webcam/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore/tree/main/tests) |
| **Samples and tools** | [samples/WebcamViewer](https://github.com/ellisnet/CodeBrix.Platform.MediaPlayerCore/tree/main/samples/WebcamViewer) |
| **This site** | [CodeBrix.Platform.MediaPlayerCore](libraries/CodeBrix.Platform.MediaPlayerCore.md) |

### CodeBrix.Platform.OpenGL

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Platform.OpenGL](https://github.com/ellisnet/CodeBrix.Platform.OpenGL) |
| **Packages** | [`CodeBrix.Platform.OpenGL.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.OpenGL.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Platform.OpenGL/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform.OpenGL/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.OpenGL/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.OpenGL/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Platform.OpenGL/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.Platform.OpenGL](libraries/CodeBrix.Platform.OpenGL.md) |

### CodeBrix.Platform.TclTk

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Platform.TclTk](https://github.com/ellisnet/CodeBrix.Platform.TclTk) |
| **Packages** | [`CodeBrix.Platform.TclTk.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.TclTk.BsdLicenseForever)<br>[`CodeBrix.Platform.TclTk.Extras.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.TclTk.Extras.BsdLicenseForever)<br>[`CodeBrix.Platform.TkCanvas.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.TkCanvas.BsdLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Platform.TclTk/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform.TclTk/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.TclTk/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.TclTk/blob/main/AGENT-README.txt) · [src/CodeBrix.Platform.TclTk.Extras/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.TclTk/blob/main/src/CodeBrix.Platform.TclTk.Extras/AGENT-README.txt) · [src/CodeBrix.Platform.TkCanvas/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.TclTk/blob/main/src/CodeBrix.Platform.TkCanvas/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Platform.TclTk/tree/main/tests) |
| **Samples and tools** | [samples/DRAKON.Brix](https://github.com/ellisnet/CodeBrix.Platform.TclTk/tree/main/samples/DRAKON.Brix) · [samples/TkCanvas_Testing](https://github.com/ellisnet/CodeBrix.Platform.TclTk/tree/main/samples/TkCanvas_Testing) · [tools/layout-oracle](https://github.com/ellisnet/CodeBrix.Platform.TclTk/tree/main/tools/layout-oracle) |
| **This site** | [CodeBrix.Platform.TclTk](libraries/CodeBrix.Platform.TclTk.md) |

### CodeBrix.Platform.Unicode

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Platform.Unicode](https://github.com/ellisnet/CodeBrix.Platform.Unicode) |
| **Packages** | [`CodeBrix.Platform.Unicode.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Unicode.ApacheLicenseForever)<br>[`CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Platform.Unicode/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform.Unicode/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.Unicode/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.Unicode/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Platform.Unicode/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.Platform.Unicode](libraries/CodeBrix.Platform.Unicode.md) |

### CodeBrix.Plotter

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Plotter](https://github.com/ellisnet/CodeBrix.Plotter) |
| **Packages** | [`CodeBrix.Plotter.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Plotter.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Plotter/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Plotter/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Plotter/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Plotter/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Plotter/tree/main/tests) |
| **Samples and tools** | [samples/PicoScope](https://github.com/ellisnet/CodeBrix.Plotter/tree/main/samples/PicoScope) |
| **This site** | [CodeBrix.Plotter](libraries/CodeBrix.Plotter.md) |

### CodeBrix.PolygonTools

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.PolygonTools](https://github.com/ellisnet/CodeBrix.PolygonTools) |
| **Packages** | [`CodeBrix.PolygonTools.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.PolygonTools.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.PolygonTools/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.PolygonTools/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.PolygonTools/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.PolygonTools/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.PolygonTools/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.PolygonTools](libraries/CodeBrix.PolygonTools.md) |

### CodeBrix.Python

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Python](https://github.com/ellisnet/CodeBrix.Python) |
| **Packages** | [`CodeBrix.Python.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Python.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Python/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Python/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Python/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Python/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Python/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.Python](libraries/CodeBrix.Python.md) |

### CodeBrix.Redis

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Redis](https://github.com/ellisnet/CodeBrix.Redis) |
| **Packages** | [`CodeBrix.Redis.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Redis.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Redis/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Redis/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Redis/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Redis/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Redis/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.Redis](libraries/CodeBrix.Redis.md) |

### CodeBrix.ServiceLocator

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.ServiceLocator](https://github.com/ellisnet/CodeBrix.ServiceLocator) |
| **Packages** | [`CodeBrix.ServiceLocator.MsplLicenseForever`](https://www.nuget.org/packages/CodeBrix.ServiceLocator.MsplLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.ServiceLocator/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.ServiceLocator/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.ServiceLocator/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.ServiceLocator/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.ServiceLocator/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.ServiceLocator](libraries/CodeBrix.ServiceLocator.md) |

### CodeBrix.SkiaSvg

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.SkiaSvg](https://github.com/ellisnet/CodeBrix.SkiaSvg) |
| **Packages** | [`CodeBrix.SkiaSvg.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.SkiaSvg.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.SkiaSvg/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.SkiaSvg](libraries/CodeBrix.SkiaSvg.md) |

### CodeBrix.Sqlite

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Sqlite](https://github.com/ellisnet/CodeBrix.Sqlite) |
| **Packages** | [`CodeBrix.Sqlite.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Sqlite.ApacheLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Sqlite/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Sqlite/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Sqlite/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Sqlite/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Sqlite/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.Sqlite](libraries/CodeBrix.Sqlite.md) |

### CodeBrix.SSH

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.SSH](https://github.com/ellisnet/CodeBrix.SSH) |
| **Packages** | [`CodeBrix.SSH.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.SSH.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.SSH/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.SSH/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.SSH/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.SSH/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.SSH/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.SSH](libraries/CodeBrix.SSH.md) |

### CodeBrix.StyleSheetParse

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.StyleSheetParse](https://github.com/ellisnet/CodeBrix.StyleSheetParse) |
| **Packages** | [`CodeBrix.StyleSheetParse.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.StyleSheetParse.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.StyleSheetParse/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.StyleSheetParse/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.StyleSheetParse/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.StyleSheetParse/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.StyleSheetParse/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.StyleSheetParse](libraries/CodeBrix.StyleSheetParse.md) |

### CodeBrix.SvgParse

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.SvgParse](https://github.com/ellisnet/CodeBrix.SvgParse) |
| **Packages** | [`CodeBrix.SvgParse.MsplLicenseForever`](https://www.nuget.org/packages/CodeBrix.SvgParse.MsplLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.SvgParse/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.SvgParse/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.SvgParse/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.SvgParse/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.SvgParse/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.SvgParse](libraries/CodeBrix.SvgParse.md) |

### CodeBrix.Templating

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Templating](https://github.com/ellisnet/CodeBrix.Templating) |
| **Packages** | [`CodeBrix.Templating.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.Templating.BsdLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Templating/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Templating/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Templating/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Templating/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.Templating](libraries/CodeBrix.Templating.md) |

### CodeBrix.Terminal

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Terminal](https://github.com/ellisnet/CodeBrix.Terminal) |
| **Packages** | [`CodeBrix.Terminal.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Terminal.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Terminal/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Terminal/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Terminal/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Terminal/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Terminal/tree/main/tests) |
| **Samples and tools** | [samples](https://github.com/ellisnet/CodeBrix.Terminal/tree/main/samples) |
| **This site** | [CodeBrix.Terminal](libraries/CodeBrix.Terminal.md) |

### CodeBrix.TestMocks

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.TestMocks](https://github.com/ellisnet/CodeBrix.TestMocks) |
| **Packages** | [`CodeBrix.TestMocks.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.TestMocks.ApacheLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.TestMocks/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.TestMocks/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.TestMocks/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.TestMocks/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.TestMocks/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.TestMocks](libraries/CodeBrix.TestMocks.md) |

### CodeBrix.Texinfo

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Texinfo](https://github.com/ellisnet/CodeBrix.Texinfo) |
| **Packages** | [`CodeBrix.Texinfo2Html.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Texinfo2Html.MitLicenseForever)<br>[`CodeBrix.Texinfo2Pdf.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Texinfo2Pdf.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Texinfo/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Texinfo/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Texinfo/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Texinfo/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.Texinfo/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.Texinfo](libraries/CodeBrix.Texinfo.md) |

### CodeBrix.VideoPlayback

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.VideoPlayback](https://github.com/ellisnet/CodeBrix.VideoPlayback) |
| **Packages** | [`CodeBrix.VideoPlayback.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoPlayback.MitLicenseForever)<br>[`CodeBrix.VideoPlayback.Skia.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoPlayback.Skia.MitLicenseForever)<br>[`CodeBrix.VideoPlayback.Authoring.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoPlayback.Authoring.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.VideoPlayback/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.VideoPlayback/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.VideoPlayback/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.VideoPlayback/blob/main/AGENT-README.txt) · [src/CodeBrix.VideoPlayback.Skia/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.VideoPlayback/blob/main/src/CodeBrix.VideoPlayback.Skia/AGENT-README.txt) · [src/CodeBrix.VideoPlayback.Authoring/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.VideoPlayback/blob/main/src/CodeBrix.VideoPlayback.Authoring/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.VideoPlayback/tree/main/tests) |
| **Samples and tools** | [samples](https://github.com/ellisnet/CodeBrix.VideoPlayback/tree/main/samples) · [tools](https://github.com/ellisnet/CodeBrix.VideoPlayback/tree/main/tools) |
| **This site** | [CodeBrix.VideoPlayback](libraries/CodeBrix.VideoPlayback.md) |

### CodeBrix.VideoPlayback.Dav1d

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.VideoPlayback.Dav1d](https://github.com/ellisnet/CodeBrix.VideoPlayback.Dav1d) |
| **Packages** | [`CodeBrix.VideoPlayback.Dav1d.BsdLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoPlayback.Dav1d.BsdLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.VideoPlayback.Dav1d/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.VideoPlayback.Dav1d/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.VideoPlayback.Dav1d/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.VideoPlayback.Dav1d/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.VideoPlayback.Dav1d/tree/main/tests) |
| **Samples and tools** | [dav1d-native-tools](https://github.com/ellisnet/CodeBrix.VideoPlayback.Dav1d/tree/main/dav1d-native-tools) |
| **This site** | [CodeBrix.VideoPlayback.Dav1d](libraries/CodeBrix.VideoPlayback.Dav1d.md) |

### CodeBrix.VideoProcessing

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.VideoProcessing](https://github.com/ellisnet/CodeBrix.VideoProcessing) |
| **Packages** | [`CodeBrix.VideoProcessing.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.VideoProcessing/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.VideoProcessing/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.VideoProcessing/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.VideoProcessing/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.VideoProcessing/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.VideoProcessing](libraries/CodeBrix.VideoProcessing.md) |

### CodeBrix.VideoProcessing.OpenCV5

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.VideoProcessing.OpenCV5](https://github.com/ellisnet/CodeBrix.VideoProcessing.OpenCV5) |
| **Packages** | [`CodeBrix.VideoProcessing.OpenCV5.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.OpenCV5.ApacheLicenseForever)<br>[`CodeBrix.VideoProcessing.OpenCV5.Wpf.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.OpenCV5.Wpf.ApacheLicenseForever)<br>[`CodeBrix.VideoProcessing.OpenCV5.LinuxX64.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.OpenCV5.LinuxX64.ApacheLicenseForever)<br>[`CodeBrix.VideoProcessing.OpenCV5.LinuxArm64.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.OpenCV5.LinuxArm64.ApacheLicenseForever)<br>[`CodeBrix.VideoProcessing.OpenCV5.LinuxRiscv64.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.OpenCV5.LinuxRiscv64.ApacheLicenseForever)<br>[`CodeBrix.VideoProcessing.OpenCV5.MacOSX64.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.OpenCV5.MacOSX64.ApacheLicenseForever)<br>[`CodeBrix.VideoProcessing.OpenCV5.MacOSArm64.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.OpenCV5.MacOSArm64.ApacheLicenseForever)<br>[`CodeBrix.VideoProcessing.OpenCV5.WindowsX64.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.OpenCV5.WindowsX64.ApacheLicenseForever)<br>[`CodeBrix.VideoProcessing.OpenCV5.WindowsArm64.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.OpenCV5.WindowsArm64.ApacheLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.VideoProcessing.OpenCV5/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.VideoProcessing.OpenCV5/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.VideoProcessing.OpenCV5/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.VideoProcessing.OpenCV5/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.VideoProcessing.OpenCV5/tree/main/tests) |
| **Samples and tools** | [tools/build_native_libraries](https://github.com/ellisnet/CodeBrix.VideoProcessing.OpenCV5/tree/main/tools/build_native_libraries) |
| **This site** | [CodeBrix.VideoProcessing.OpenCV5](libraries/CodeBrix.VideoProcessing.OpenCV5.md) |

### CodeBrix.YamlParse

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.YamlParse](https://github.com/ellisnet/CodeBrix.YamlParse) |
| **Packages** | [`CodeBrix.YamlParse.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.YamlParse.MitLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.YamlParse/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.YamlParse/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.YamlParse/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.YamlParse/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/CodeBrix.YamlParse/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [CodeBrix.YamlParse](libraries/CodeBrix.YamlParse.md) |

### FreePPlus

| | |
| --- | --- |
| **GitHub** | [ellisnet/FreePPlus](https://github.com/ellisnet/FreePPlus) |
| **Packages** | [`FreePPlus.LgplLicenseForever`](https://www.nuget.org/packages/FreePPlus.LgplLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/FreePPlus/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/FreePPlus/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/FreePPlus/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/FreePPlus/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/FreePPlus/tree/main/tests) |
| **Samples and tools** | [samples/FreePPlus.OfficeOpenXml.SampleApp](https://github.com/ellisnet/FreePPlus/tree/main/samples/FreePPlus.OfficeOpenXml.SampleApp) |
| **This site** | [FreePPlus](libraries/FreePPlus.md) |

### SilverAssertions

| | |
| --- | --- |
| **GitHub** | [ellisnet/SilverAssertions](https://github.com/ellisnet/SilverAssertions) |
| **Packages** | [`SilverAssertions.ApacheLicenseForever`](https://www.nuget.org/packages/SilverAssertions.ApacheLicenseForever) |
| **Documents** | [README.md](https://github.com/ellisnet/SilverAssertions/blob/main/README.md) · [README-INDEX.txt](https://github.com/ellisnet/SilverAssertions/blob/main/README-INDEX.txt) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/SilverAssertions/blob/main/THIRD-PARTY-NOTICES.txt) |
| **AGENT-README.txt** | [AGENT-README.txt](https://github.com/ellisnet/SilverAssertions/blob/main/AGENT-README.txt) |
| **Tests** | [tests](https://github.com/ellisnet/SilverAssertions/tree/main/tests) |
| **Samples and tools** | None |
| **This site** | [SilverAssertions](libraries/SilverAssertions.md) |

## Sample application repositories

The three sample repositories hold complete, runnable reference applications rather than
snippets. They are split by license so that the terms of one application never set the terms
for the rest. Every application folder carries its own `README.md` and
`THIRD-PARTY-NOTICES.txt`, and each repository's blueprint files collect the how-tos mined
from all of its applications.

### CodeBrix.Samples

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Samples](https://github.com/ellisnet/CodeBrix.Samples) |
| **Packages** | None; these are applications |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Samples/blob/main/README.md) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Samples/blob/main/THIRD-PARTY-NOTICES.txt) |
| **License** | Apache License 2.0 |
| **Blueprints** | [BLUEPRINTS-Index.md](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-Index.md) and the topic files listed below |
| **Applications** | [CodeBrixVideoTool](https://github.com/ellisnet/CodeBrix.Samples/tree/main/CodeBrixVideoTool) · [DRAKON.Brix](https://github.com/ellisnet/CodeBrix.Samples/tree/main/DRAKON.Brix) · [GameEngineMusicDemo](https://github.com/ellisnet/CodeBrix.Samples/tree/main/GameEngineMusicDemo) · [GitHubIssueFinder](https://github.com/ellisnet/CodeBrix.Samples/tree/main/GitHubIssueFinder) · [InannaRosette](https://github.com/ellisnet/CodeBrix.Samples/tree/main/InannaRosette) · [JustBetweenUs](https://github.com/ellisnet/CodeBrix.Samples/tree/main/JustBetweenUs) · [KenneyAssetBrowser](https://github.com/ellisnet/CodeBrix.Samples/tree/main/KenneyAssetBrowser) · [MediaPlayerDemo](https://github.com/ellisnet/CodeBrix.Samples/tree/main/MediaPlayerDemo) · [NotionDocumentCreator](https://github.com/ellisnet/CodeBrix.Samples/tree/main/NotionDocumentCreator) · [PainDiagram](https://github.com/ellisnet/CodeBrix.Samples/tree/main/PainDiagram) · [PalmVisualizer](https://github.com/ellisnet/CodeBrix.Samples/tree/main/PalmVisualizer) · [PdfSideBySide](https://github.com/ellisnet/CodeBrix.Samples/tree/main/PdfSideBySide) · [PicoScope.Brix](https://github.com/ellisnet/CodeBrix.Samples/tree/main/PicoScope.Brix) · [Pinta.Brix](https://github.com/ellisnet/CodeBrix.Samples/tree/main/Pinta.Brix) · [PolyHavenBrowser](https://github.com/ellisnet/CodeBrix.Samples/tree/main/PolyHavenBrowser) · [PolyHavenBrowser_viewer_only](https://github.com/ellisnet/CodeBrix.Samples/tree/main/PolyHavenBrowser_viewer_only) · [RedisSetupTool](https://github.com/ellisnet/CodeBrix.Samples/tree/main/RedisSetupTool) · [SimpleCbxVideoPlayer](https://github.com/ellisnet/CodeBrix.Samples/tree/main/SimpleCbxVideoPlayer) · [WebcamPainter](https://github.com/ellisnet/CodeBrix.Samples/tree/main/WebcamPainter) · [WebcamViewer](https://github.com/ellisnet/CodeBrix.Samples/tree/main/WebcamViewer) · [WikipediaPublisher](https://github.com/ellisnet/CodeBrix.Samples/tree/main/WikipediaPublisher) |
| **This site** | [Reference applications](samples/README.md) · [Blueprints](samples/blueprints.md) |

<details>
<summary>The blueprint files in CodeBrix.Samples</summary>

- [BLUEPRINTS-Index.md](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-Index.md)
- [BLUEPRINTS-AppStructureAndStartup.md](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-AppStructureAndStartup.md)
- [BLUEPRINTS-MVVM.md](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-MVVM.md)
- [BLUEPRINTS-PlatformServices.md](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-PlatformServices.md)
- [BLUEPRINTS-ViewsAndControls.md](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-ViewsAndControls.md)
- [BLUEPRINTS-ThemingAndStyling.md](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-ThemingAndStyling.md)
- [BLUEPRINTS-GraphicsAndRendering.md](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-GraphicsAndRendering.md)
- [BLUEPRINTS-MediaAndVision.md](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-MediaAndVision.md)
- [BLUEPRINTS-DocumentsAndData.md](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-DocumentsAndData.md)
- [BLUEPRINTS-SettingsAndPersistence.md](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-SettingsAndPersistence.md)
- [BLUEPRINTS-TextEditing.md](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-TextEditing.md)
- [BLUEPRINTS-GameEngine.md](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-GameEngine.md)
- [BLUEPRINTS-Testing.md](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-Testing.md)
- [BLUEPRINTS-ProjectLayoutAndPackaging.md](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-ProjectLayoutAndPackaging.md)
- [BLUEPRINTS-NotYetCovered.md](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-NotYetCovered.md)

</details>

### CodeBrix.Samples.Gpl2

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Samples.Gpl2](https://github.com/ellisnet/CodeBrix.Samples.Gpl2) |
| **Packages** | None; these are applications |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Samples.Gpl2/blob/main/README.md) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Samples.Gpl2/blob/main/THIRD-PARTY-NOTICES.txt) |
| **License** | GNU General Public License, version 2 |
| **Blueprints** | [BLUEPRINTS.md](https://github.com/ellisnet/CodeBrix.Samples.Gpl2/blob/main/BLUEPRINTS.md) |
| **Applications** | [Doom.Brix](https://github.com/ellisnet/CodeBrix.Samples.Gpl2/tree/main/Doom.Brix) · [Wolfenstein.Brix](https://github.com/ellisnet/CodeBrix.Samples.Gpl2/tree/main/Wolfenstein.Brix) |
| **This site** | [Reference applications](samples/README.md) · [Blueprints](samples/blueprints.md) |

### CodeBrix.Samples.Gpl3

| | |
| --- | --- |
| **GitHub** | [ellisnet/CodeBrix.Samples.Gpl3](https://github.com/ellisnet/CodeBrix.Samples.Gpl3) |
| **Packages** | None; these are applications |
| **Documents** | [README.md](https://github.com/ellisnet/CodeBrix.Samples.Gpl3/blob/main/README.md) · [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Samples.Gpl3/blob/main/THIRD-PARTY-NOTICES.txt) |
| **License** | GNU General Public License, version 3 |
| **Blueprints** | [BLUEPRINTS.md](https://github.com/ellisnet/CodeBrix.Samples.Gpl3/blob/main/BLUEPRINTS.md) |
| **Applications** | [Fresco.Brix](https://github.com/ellisnet/CodeBrix.Samples.Gpl3/tree/main/Fresco.Brix) |
| **This site** | [Reference applications](samples/README.md) · [Blueprints](samples/blueprints.md) |

---

**Where to go next**

- [Libraries](libraries/README.md) - the same repositories grouped by what they do
- [Reference applications](samples/README.md) - what each sample application demonstrates
- [Licensing](licensing.md) - the license families and what the package-ID suffix guarantees
- [Build a CodeBrix.Platform application](platform/README.md) - the curriculum, in order
