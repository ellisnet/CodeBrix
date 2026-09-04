<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Add-ins](../08-add-ins.md) › Svg</sub>

# Svg

**Makes the core framework's `SvgImageSource` actually render.** The core already defines `SvgImageSource`, its `UriSource`, `RasterizePixelWidth` and `RasterizePixelHeight` properties, its `Opened` and `OpenFailed` events, and the `Image` control's handling of it - but the core has no SVG parser. This add-in plugs one in and hands the `Image` control a canvas element that draws the parsed picture, as vectors, at whatever size the `Image` is arranged to.

It is an invisible add-in. Application code never names a type from this package: you reference it once, then use the core's `SvgImageSource` in XAML and in code with no add-in type in sight.

| | |
| --- | --- |
| **Package** | [`CodeBrix.Platform.Svg.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Svg.ApacheLicenseForever) |
| **Adds** | The SVG parser behind the core's `SvgImageSource`. No new XAML element and no new XAML namespace. |
| **Heads** | All six. One Skia runtime assembly serves every head. |
| **Requires** | .NET 10 or later, and a running CodeBrix.Platform application. No system package to install. |

Dependencies arrive automatically: the core framework package (where `SvgImageSource` lives), [`CodeBrix.SkiaSvg.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.SkiaSvg.MitLicenseForever) as the parser and renderer, [`CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever), and [`CodeBrix.Platform.Graphics2DSK.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Graphics2DSK.ApacheLicenseForever) for the canvas element the picture is drawn on.

## Add it to your application

Reference the package once, in the application's `.Core` project, next to the core framework package. Every head inherits it through the `.Core` project reference. Never add it to a head project.

```bash
dotnet add package CodeBrix.Platform.Svg.ApacheLicenseForever
```

A complete `.Core` project file with an SVG asset:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>MyApp</RootNamespace>
    <AssemblyName>MyApp.Core</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.Platform.ApacheLicenseForever" />
    <PackageReference Include="CodeBrix.Platform.Svg.ApacheLicenseForever" />
  </ItemGroup>
  <ItemGroup>
    <Content Include="Assets\logo.svg" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
</Project>
```

There is no XAML namespace to declare: `SvgImageSource` and `Image` are both in the default XAML namespace. The usings for code-behind are the core's own:

```csharp
using Microsoft.UI.Xaml.Media.Imaging;   // SvgImageSource (core),
                                         // SvgImageSourceLoadStatus,
                                         // SvgImageSourceOpenedEventArgs,
                                         // SvgImageSourceFailedEventArgs
using Microsoft.UI.Xaml.Controls;        // Image (core)
using Windows.Storage.Streams;           // IRandomAccessStream,
                                         // InMemoryRandomAccessStream
                                         // (for SetSourceAsync)
```

There is nothing to call and nothing to configure. The assembly carries `[assembly: ApiExtension(typeof(ISvgProvider), typeof(SvgProvider))]`; the XAML source generator finds that attribute while compiling the application and emits the registration into the generated `App` code, so every `SvgImageSource` constructed afterwards gets a provider.

> [!WARNING]
> Without the package, `SvgImageSource` still exists - it is a core type - but every instance logs the error "To use SVG on this platform, make sure to install the CodeBrix.Platform.WinUI.Svg package." and the `Image` stays blank. That log line is the symptom of a missing or misplaced reference; the package you install is `CodeBrix.Platform.Svg.ApacheLicenseForever`, in `.Core`.

## Using it

### A vector image from an application asset

The long form declares the source explicitly:

```xml
<Image Width="96" Height="96" Stretch="Uniform">
    <Image.Source>
        <SvgImageSource UriSource="ms-appx:///Assets/logo.svg" />
    </Image.Source>
</Image>
```

The short form does the same thing. Assigning a string or `Uri` whose path ends in `.svg` or `.svgz` to an `ImageSource`-typed property creates an `SvgImageSource` automatically; any other extension creates a `BitmapImage`.

```xml
<Image Width="96" Height="96" Source="ms-appx:///Assets/logo.svg" />
```

Ship the asset as content copied to the output:

```xml
<ItemGroup>
  <Content Include="Assets\logo.svg" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

### The `SvgImageSource` surface

This is the whole of what application code touches.

```csharp
public SvgImageSource();
public SvgImageSource(Uri uriSource);

public Uri    UriSource            { get; set; }   // UriSourceProperty
public double RasterizePixelWidth  { get; set; }   // default NaN (logical px)
public double RasterizePixelHeight { get; set; }   // default NaN (logical px)

public IAsyncOperation<SvgImageSourceLoadStatus>
    SetSourceAsync(IRandomAccessStream streamSource);

public event TypedEventHandler<SvgImageSource, SvgImageSourceOpenedEventArgs> Opened;
public event TypedEventHandler<SvgImageSource, SvgImageSourceFailedEventArgs> OpenFailed;

public enum SvgImageSourceLoadStatus { Success, NetworkError, InvalidFormat, Other }
public partial class SvgImageSourceFailedEventArgs
    { public SvgImageSourceLoadStatus Status { get; } }
public partial class SvgImageSourceOpenedEventArgs { }   // no members
```

Setting `UriSource` unloads any previous SVG and starts loading the new one; setting it to `null` unloads. Parsing runs on a thread-pool thread, so a large document does not stall input - and so the `Opened` and `OpenFailed` handlers may run off the UI thread. Dispatch any UI work you do in them.

```csharp
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

var svg = new SvgImageSource(new Uri("ms-appx:///Assets/diagram.svg"));
svg.Opened += (s, e) => Status.Text = "loaded";
svg.OpenFailed += (s, e) => Status.Text = $"failed: {e.Status}";   // InvalidFormat, ...

var image = new Image { Source = svg, Width = 400, Stretch = Stretch.Uniform };
root.Children.Add(image);
```

Notice the asymmetry in that handler pair: a document the parser rejects raises `OpenFailed` with `Status = InvalidFormat`, but a URI that cannot be *fetched* at all produces no picture and raises nothing at all - the `Image` stays blank. Verify the path first when an image never appears.

### Loading an embedded resource

`SvgImageSource` has no embedded-resource resolver, so `embedded://` is not a URI form here. Open the manifest stream yourself and hand it to `SetSourceAsync` - this is the way to load an embedded or any in-memory SVG.

```xml
<ItemGroup>
  <EmbeddedResource Include="Assets\padlock-icon.svg" />
</ItemGroup>
```

```csharp
using System.Reflection;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media.Imaging;

static async Task<SvgImageSource> LoadEmbeddedSvgAsync(string resourceName)
{
    var assembly = typeof(App).Assembly;    // or Assembly.Load("MyApp.Core")
    await using var resource = assembly.GetManifestResourceStream(resourceName)
        ?? throw new InvalidOperationException($"Resource '{resourceName}' not found.");

    var ras = new InMemoryRandomAccessStream();
    var writer = ras.AsStreamForWrite();
    await resource.CopyToAsync(writer);
    await writer.FlushAsync();
    ras.Seek(0);

    var svg = new SvgImageSource();
    var status = await svg.SetSourceAsync(ras);      // SvgImageSourceLoadStatus
    if (status != SvgImageSourceLoadStatus.Success)
        throw new InvalidOperationException($"SVG load failed: {status}");
    return svg;
}

// usage
Logo.Source = await LoadEmbeddedSvgAsync("MyApp.Assets.padlock-icon.svg");
```

Do not dispose the `InMemoryRandomAccessStream` right after `SetSourceAsync`: it is managed memory and is collected once the source releases it. Passing `null` to `SetSourceAsync` throws `ArgumentException`.

### Vector mode and rasterized mode

While both `RasterizePixelWidth` and `RasterizePixelHeight` are `NaN` - the default - the SVG is drawn as vectors at the arranged size, crisp at every size and display density. When *both* are set, the provider pre-renders the picture into a bitmap of that logical size, multiplied by the display scale, and the `Image` draws that bitmap stretched to its arranged size. So 32 logical pixels becomes a 64-pixel bitmap on a 200-percent display. Setting only one of the two changes nothing.

```xml
<Image Width="32" Height="32">
    <Image.Source>
        <SvgImageSource UriSource="ms-appx:///Assets/icon.svg"
                        RasterizePixelWidth="32"
                        RasterizePixelHeight="32" />
    </Image.Source>
</Image>
```

Use this for a large, complex SVG shown at a constant size, where each repaint becomes a bitmap copy instead of a re-draw of the picture. Changing either value after load re-rasterizes immediately, on the thread that sets the property - so do not animate them.

> [!WARNING]
> The two values are applied crosswise when the bitmap is allocated: the height value sizes the bitmap's width and the width value sizes its height. Use equal values for a square raster unless you have verified the result at your aspect ratio.

### Swapping the image at run time

```csharp
var svg = (SvgImageSource)Logo.Source;
svg.UriSource = new Uri("ms-appx:///Assets/logo-dark.svg");   // reloads
svg.UriSource = null;                                           // unloads
```

### Where a source can load from

| Form | Behavior |
| --- | --- |
| `ms-appx:///Assets/<file>.svg` | A file in the application's install folder, shipped as content copied to the output. A library's own asset is `ms-appx:///<AssemblyName>/Assets/<file>.svg`. |
| `ms-appdata://local/<file>.svg` | A file under the application's data folders. |
| `http://`, `https://` | Downloaded. |
| `file:///absolute/path.svg` | Read from disk. |
| A relative string set from code | `Assets/logo.svg` or `/Assets/logo.svg` is normalized to `ms-appx:///Assets/logo.svg`. |

In XAML a value with a leading `/` is likewise prefixed with `ms-appx:///`, but a bare relative value is resolved against the XAML file's own location - write the absolute `ms-appx:///` form in XAML to avoid surprises.

### How the Image control uses it

The `Image` asks the source for its canvas and adds it as a child; once the SVG has parsed, the `Image` measures itself from the source's intrinsic size - the parsed picture's bounds - and arranges the canvas to the size `Image.Stretch` produces from that. `Image.ImageOpened` fires when the SVG has parsed, and `Image.ImageFailed` fires with the message "Failed to load Svg source" when `OpenFailed` fires.

Other `ImageSource` consumers do not get the vector canvas. An `ImageBrush`, for instance, receives a bitmap rendered once at the SVG's intrinsic size, so use an `Image` element wherever sharpness at large sizes matters.

### What it does not do

This add-in is not an SVG API. It exposes no document model, no element access, no hit testing and no export: it renders a whole SVG file into an `Image`. For programmatic SVG work use [CodeBrix.SkiaSvg](../../libraries/CodeBrix.SkiaSvg.md) directly - it is already a dependency - and for a document object model you can build and edit, use [CodeBrix.SvgParse](../../libraries/CodeBrix.SvgParse.md).

It supports the SVG feature subset that CodeBrix.SkiaSvg supports, no more; that library's own guide lists the unsupported elements, filters and CSS constructs. It does not animate SVGs through SMIL or CSS - use the [Lottie](Lottie.md) add-in for animation. It does not resolve `embedded://` URIs, and it does not cache parsed pictures across `SvgImageSource` instances.

## Per-head notes

There are none. One Skia runtime assembly serves every head, and no head-specific behavior is documented.

## Pitfalls

- **Blank image plus "make sure to install the CodeBrix.Platform.WinUI.Svg package" in the log** means the add-in is not referenced by the project chain that compiles the application. Put `CodeBrix.Platform.Svg.ApacheLicenseForever` in `.Core`.
- **`embedded://` is not understood by `SvgImageSource`.** Use `SetSourceAsync` with the manifest stream.
- **Loading errors do not throw.** A bad path or an unreachable URI leaves the `Image` blank and raises nothing; only a malformed document raises `OpenFailed` and `Image.ImageFailed`.
- **`RasterizePixelWidth` and `RasterizePixelHeight` only take effect when both are non-`NaN`**, and they are applied crosswise when the bitmap is allocated.
- **Rasterized output is stretched to the arranged size.** A 32-pixel raster shown at 128 pixels is blurry. Rasterize at the largest size you will show, or stay in vector mode.
- **The intrinsic size comes from the parsed picture's bounds.** Give the `Image` an explicit `Width` and `Height`, or rely on `Stretch` inside a sized container, when the SVG's own `width`, `height` or `viewBox` is not what you want on screen.
- **`SetSourceAsync(null)` throws `ArgumentException`.**
- **`Opened` and `OpenFailed` may run off the UI thread.** Dispatch any UI work.
- **Do not implement `ISvgProvider` or construct `SvgProvider` yourself.** The core documents that interface as internal plumbing whose signature may change.
- **Never add the package to a head project.** `.Core` only.
- **Reuse one source for repeated images of the same file.** There is no shared cache: each `SvgImageSource` instance parses independently.
- **Keep SVGs simple.** Filters, masks and text are the expensive constructs in any Skia-based SVG renderer.

## Related

- [CodeBrix.SkiaSvg](../../libraries/CodeBrix.SkiaSvg.md) - the parser and renderer behind this add-in, and the API to use for hit testing, scene-graph mutation, SMIL playback and export
- [CodeBrix.SvgParse](../../libraries/CodeBrix.SvgParse.md) - the renderer-agnostic SVG document object model, for building or editing SVG markup
- [Lottie](Lottie.md) - the companion add-in for animated vector content
- [Graphics2DSK](Graphics2DSK.md) - the canvas element the parsed picture is drawn on, and one of this add-in's dependencies
- [JustBetweenUs](https://github.com/ellisnet/CodeBrix.Samples/tree/main/JustBetweenUs) in [CodeBrix.Samples](../../samples/README.md) - an `EmbeddedImage` control that loads `.svg` embedded resources through `SetSourceAsync` and `ms-appx` and `https` URIs through `UriSource`; the in-repository copy is at [samples/CodeBrixPlatform/JustBetweenUs](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/JustBetweenUs)
- [KenneyAssetBrowser](https://github.com/ellisnet/CodeBrix.Samples/tree/main/KenneyAssetBrowser) in [CodeBrix.Samples](../../samples/README.md) - previews SVG art alongside images, fonts, maps, models and audio

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.Svg/AGENT-README.txt) |
| The core framework and the six heads | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/AGENT-README.txt) |
| The parser's own guide | [CodeBrix.SkiaSvg AGENT-README.txt](https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/AGENT-README.txt) |
| Map of every README in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/README-INDEX.txt) |
| Package | [`CodeBrix.Platform.Svg.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Svg.ApacheLicenseForever) |

---

**Where to go next**

- [SkiaSharp views](SkiaSharpViews.md) - the next add-in: `SKXamlCanvas` and the conversion helpers
- [CodeBrix.SkiaSvg](../../libraries/CodeBrix.SkiaSvg.md) - the full SVG library, for work that is not an `Image` in a page
- [All add-ins](../08-add-ins.md) - the whole set at a glance
