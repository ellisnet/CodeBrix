<sub>[CodeBrix](../../README.md) › Build a CodeBrix.Platform application</sub>

# Build a CodeBrix.Platform application

**This track takes you from an empty folder to a desktop application that runs natively on Windows, Linux and macOS from one codebase.** You write the application once against the WinUI XAML API surface - the same `Microsoft.UI.Xaml.*` controls, XAML, code-behind and data binding - and CodeBrix.Platform renders it through a Skia-based engine on six platform heads. Read the chapters in order the first time; afterwards each one stands on its own.

## The chapters

| Chapter | What you get from it |
| --- | --- |
| [01 - What CodeBrix.Platform is](01-what-is-codebrix-platform.md) | The API surface, the two packages an application starts with, the `.Core` / `.UI` / heads shape, and what is deliberately out of scope |
| [02 - Runs on every laptop](02-runs-on-every-laptop.md) | The six heads one by one: package, bootstrap call, GPU and software render paths, operating-system prerequisites, and each head's limits |
| [03 - Your first application](03-your-first-application.md) | Every file, verbatim, from `dotnet new sln` to a window on screen on each head, then one add-in |
| [04 - Project architecture](04-project-architecture.md) | Which package goes where, project and head naming, shared projects, libraries under `src/libs` |
| [05 - MVVM the right way](05-mvvm-the-right-way.md) | View models, commands, the design-mode guard, background work marshalled back to the UI thread |
| [06 - Views and styling](06-views-and-styling.md) | XAML pages, converters, theming, fonts, and the controls you write yourself |
| [07 - Platform services](07-platform-services.md) | Windowing, dispatching, pickers, the clipboard, dialogs and the bridges a view model reaches them through |
| [08 - Add-ins](08-add-ins.md) | Every add-in package in one table, and which heads each one is live on |
| [09 - Graphics, media and vision](09-graphics-media-and-vision.md) | 2D and 3D drawing surfaces, video and audio playback, camera capture and on-device vision |
| [10 - Testing your application](10-testing-your-application.md) | Test projects, fixtures, headless graphics and golden-image comparison |
| [11 - Packaging and shipping](11-packaging-and-shipping.md) | Solutions per operating system, native payloads, and the notices file every application carries |
| [12 - Troubleshooting](12-troubleshooting.md) | The failures that have a known cause: blank windows, missing pickers, duplicate types, missing engines |
| [13 - Reference applications](13-reference-applications.md) | The complete applications you can open, build and read |
| [14 - Sharing code with native frameworks](14-sharing-code-with-native-frameworks.md) | Running one view model on the Skia heads and on native Windows and mobile heads |

## If you only read three chapters

Read [03 - Your first application](03-your-first-application.md) with a terminal open: it contains every file you need and nothing you do not. Then read [04 - Project architecture](04-project-architecture.md), which is the chapter that stops a solution going wrong later - one head project, one head package, every add-in referenced once in `.Core`. Then read [05 - MVVM the right way](05-mvvm-the-right-way.md), because everything the user touches lives in a view model.

Keep [12 - Troubleshooting](12-troubleshooting.md) bookmarked. Most first-run surprises are on it.

---

**Where to go next**

- [01 - What CodeBrix.Platform is](01-what-is-codebrix-platform.md) - start of the track
- [The standalone libraries](../libraries/README.md) - the other track: libraries usable from any .NET 10 application
- [Reference applications](../samples/README.md) - complete applications to read alongside the chapters
- [CodeBrix.Platform on GitHub](https://github.com/ellisnet/CodeBrix.Platform) - source, tests and samples
