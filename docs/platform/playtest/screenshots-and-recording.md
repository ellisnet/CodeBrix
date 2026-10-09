<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Testing your application](../10-testing-your-application.md) › [PlayTest](README.md) › Screenshots and recording</sub>

# Screenshots and recording

**PlayTest captures the virtual screen - the whole page or one element - as PNG bytes a test can save or measure, and one command-line switch records a screenshot after every step of every test in a run.** Screenshots always contain the virtual screen itself at its exact logical size, in both preview modes, never the preview window or its letterbox bars, and capturing needs no desktop screen-recording permission.

## Page and element screenshots

```csharp
byte[] page = await Page.ScreenshotAsync(new() { Path = "TestResults/PlayTest/page.png", Stable = true });
byte[] chart = await Page.GetByTestId("Chart").ScreenshotAsync(new() { Path = "TestResults/PlayTest/chart.png" });
```

| Option | Meaning |
| --- | --- |
| `Path` | Also write the PNG to this file. A relative path resolves against the current directory, and missing folders are created |
| `Stable` | Repeat the capture until two consecutive captures are identical - transitions and animations have settled - failing at the timeout |
| `Timeout` | How long `Stable`, and an element's wait for visible, stable bounds, may take |

Both return the PNG bytes whether or not `Path` is set. An element screenshot waits until the element is visible at stable bounds, brings a partly scrolled-out element into view, and crops to the element's bounds in whole pixels, clipped to the virtual screen.

> [!WARNING]
> An endless animation - a spinner, a playing video - inside the captured area never settles, so `Stable = true` times out. Capture an area without it, or leave `Stable` off.

The sample suites keep a one-line helper for the pictures they want a person to look at after a run:

```csharp
// From CodeBrix.Samples/PlayTestSupport/SampleFixture.cs
protected Task SnapshotAsync(string name) => Page.ScreenshotAsync(new()
{
    Path = Path.Combine("TestResults", "PlayTest", name + ".png"),
});
```

A failed action writes its own `failure-<id>.png` into `PlayTestOptions.ArtifactsDirectory` (`TestResults/PlayTest` by default) and names it in the exception message.

## Measuring pixels

A screenshot is also data. Because rendering is software Skia with embedded fonts, the pixels of the virtual screen are the same on every operating system, so a test can check an exact pixel where the application controls the color.

```csharp
// From CodeBrix.Platform/samples/CodeBrixPlatform/PlayTestDemo/tests/PlayTestDemo.PlayTests/ApiLabTests.cs
var png = await Page.GetByTestId("Swatch").ScreenshotAsync();
using var image = SKBitmap.Decode(png);
(image.Width, image.Height).Should().Be((160, 80));
image.GetPixel(5, 5).Should().Be(new SKColor(0x2E, 0x7D, 0x32));
image.GetPixel(80, 40).Should().Be(SKColors.White);
```

SkiaSharp comes with the PlayTest package, so `SKBitmap.Decode` is available in every PlayTests project. To find a point on an element in a full-page capture, add the element's `BoundingBoxAsync()` offset:

```csharp
// From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/tests/PainDiagram.PlayTests/ApplicationTests.cs
var box = await Canvas.BoundingBoxAsync();
var rect = await DrawingRectAsync();
using var bitmap = SKBitmap.Decode(await Page.ScreenshotAsync());
var pixel = bitmap.GetPixel((int)(box.X + rect.Left + rect.Width * normalizedX),
    (int)(box.Y + rect.Top + rect.Height * normalizedY));
```

Where exact values are not the point - a drawing has appeared, a region is not blank, an input changed the picture - the `PixelStats` type in the PlayTest package measures a capture's structure: `Coverage(color)`, `Bounds(background)`, `Centroid(background)`, `IsBlank`, `IsUniform`, `MeanLuminance`, `MirrorSymmetry` and `Difference(other)` between two captures of the same test. It reads only the bytes it is given, with no files and no baselines. [OpenGL](opengl.md#asserting-gl-content-with-pixelstats) documents it in full, because GL content - which has no visual tree - is where it matters most.

```csharp
var before = PixelStats.FromPng(await Page.GetByTestId("Canvas").ScreenshotAsync(new() { Stable = true }));
await Button("Rotate").ClickAsync();
var after = PixelStats.FromPng(await Page.GetByTestId("Canvas").ScreenshotAsync(new() { Stable = true }));
before.Difference(after, tolerance: 8).ChangedFraction.Should().BeGreaterThan(0.05);
```

> [!NOTE]
> PlayTest deliberately has no comparison against saved golden images. Assert the structure of the picture in the test, and leave the looks to the screenshots for a person to review. For drawing code you test outside the UI, [10 - Testing your application](../10-testing-your-application.md#golden-images-and-generated-documents) covers golden-image comparison.

## Recording a whole run

In an xUnit v3 project, the `--screenshotfolder` switch records the whole run with no test, fixture or project changes:

```bash
mkdir -p "$HOME/Temp/run-1"
dotnet test --project tests/MyApp.PlayTests/MyApp.PlayTests.csproj --screenshotfolder="$HOME/Temp/run-1"
```

The folder must **already exist** and be **empty** - hidden files and empty subfolders count. Relative paths, absolute paths and a leading `~/` are accepted; quote a value that contains spaces. An invalid or non-empty folder fails the run before any test starts, and the recorder never empties or overwrites anything, so use a fresh empty folder for every run and for every test project.

For a test in namespace `MyApp.PlayTests.SendButton` (root namespace `MyApp.PlayTests`), class `ClickTests`, method `clicking_button_adds_text`, the folder receives:

```text
screenshot-index.json
SendButton/ClickTests/clicking_button_adds_text/
  screenshot-start.png      after test setup, before the body
  screenshot-1.png          after each PlayTest action, wait or assertion
  screenshot-2.png
  ...
  screenshot-final.png      before cleanup - also when the test failed
```

- **One image per PlayTest API call.** Nested calls and retry polls do not add images. Changes made by background work appear at the next capture.
- **Theory rows** get `test-case-1/`, `test-case-2/` and so on, in execution order within the method for that run - use the IDs and arguments recorded in the index to identify a row. Re-runs of the same test get `attempt-2/` and on.
- **Folder names** omit the project's root namespace prefix; names unsafe on Windows are escaped; long names and collisions get a short hash suffix.
- **Skipped tests** produce no entry. A test without a running application, or whose setup failed, gets metadata and a note but no images.

`screenshot-index.json` is replaced atomically as the run proceeds. It records the run - its ID and status, local timestamps with UTC offsets, operating system, architecture, runtime, time zone, machine name and process ID - the test, application and PlayTest assemblies, the command-line and environment preferences, the resolved preview, theme, orientation and action delay, and per test its ID, display name, namespace, class, method, arguments, outcome, errors and duration, with every PNG's kind, step, operation, locator, size, orientation, themes, and the calling source file and line when PDBs allow. A run that finished normally is marked `completed` even if tests failed; an interrupted run leaves a valid index marked `running`.

Recording works only under the reflection-based xUnit v3 runner; other Microsoft Testing Platform runners reject `--screenshotfolder` with an explanation. It costs one PNG per API call, so turn it on for the runs you will inspect.

> [!TIP]
> When `dotnet test` reports "Zero tests ran" with `--screenshotfolder`, the folder was probably rejected and the message hidden. Run the test program directly - `dotnet exec MyApp.PlayTests.dll --screenshotfolder=...` from its output folder - to see the diagnostic.

---

**Where to go next**

- [Running, switches and the preview](running-and-switches.md) - every switch, environment variable and project preference
- [OpenGL](opengl.md) - `PixelStats` in full, for content with no visual tree
- [PlayTest](README.md) - the hub page
