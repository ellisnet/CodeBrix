<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Imaging</sub>

# CodeBrix.Imaging

**CodeBrix.Imaging loads, saves, converts, resizes, transforms, filters, quantizes, composites and
annotates raster images, and it rasterizes TrueType/CFF text directly onto those images.** It is
100% managed code: there are no native libraries to deploy and no platform-specific packages to
reference. Use it from any .NET 10 application, or from a CodeBrix.Platform application, wherever
pixels have to be produced, measured or changed.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Imaging](https://github.com/ellisnet/CodeBrix.Imaging) |
| **Packages** | [`CodeBrix.Imaging.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Imaging.ApacheLicenseForever) |
| **License** | Apache 2.0; see [License](#license) |
| **Requires** | .NET 10 or later; no NuGet dependencies, no native libraries, nothing to initialize |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Windows, Linux and macOS - the same code runs identically on each |

## What it does

- Loads, saves and converts eight raster formats - BMP, GIF, JPEG, PBM, PNG, TGA, TIFF and WebP.
  On load the format is detected from the byte signature; on save it is inferred from the file
  extension unless you pass an encoder.
- Runs a processing pipeline through `Mutate()` (in place) and `Clone()` (new image): resize, crop,
  pad, entropy crop, flip, rotate, rotate-flip, auto-orient, skew, swizzle, and affine and
  projective transforms.
- Applies color and tone filters: `BlackWhite`, `Grayscale`, `Invert`, `Sepia`, `Kodachrome`,
  `Polaroid`, `Lomograph`, `Brightness`, `Contrast`, `Saturate`, `Lightness`, `Hue`, `Opacity`,
  `ColorBlindness`, and an arbitrary 5x4 `Filter(ColorMatrix)`.
- Blurs, sharpens and convolves: `GaussianBlur`, `GaussianSharpen`, `BoxBlur`, `BokehBlur`, and
  `DetectEdges` with the 2D, single-kernel and compass detectors in `KnownEdgeDetectorKernels`.
- Adds effects - `OilPaint`, `Pixelate`, `Vignette`, `Glow` - and hosts your own per-pixel effect
  inside the pipeline through `ProcessPixelRowsAsVector4`.
- Normalizes tone with `HistogramEqualization()` in Global, AdaptiveTileInterpolation and
  AdaptiveSlidingWindow modes.
- Quantizes (`Octree`, `Wu`, `WebSafe`, `Werner`, and `PaletteQuantizer` over a palette you supply),
  dithers (the ordered Bayer family plus Atkinson, Burks, FloydSteinberg, JarvisJudiceNinke,
  Sierra, StevensonArce and Stucki error diffusion) and binarizes (`BinaryThreshold`,
  `BinaryDither`, `AdaptiveThreshold`).
- Composites one image onto another with `DrawImage` - blending mode, alpha composition mode,
  location and opacity - and flattens transparency with `BackgroundColor`.
- Rasterizes text onto an image with the `DrawText` and `MeasureText` extension methods and the
  `TextMeasurer` API, with wrapping, alignment, justification, kerning, hinting, line spacing,
  text direction, OpenType feature tags and per-range `TextRun` formatting.
- Handles fonts through `SystemFonts`, `FontCollection`, `FontFamily`, `Font` and `FontStyle`,
  reading TrueType (.ttf) including variable fonts, CFF/Type2 outlines, color (COLR/CPAL) fonts,
  WOFF and WOFF2, and TrueType collections (.ttc) through `AddCollection`.
- Gives pixel access at three levels: the bounds-checked `[x, y]` indexer, `ProcessPixelRows` with
  a pinned `PixelAccessor<TPixel>`, and the bulk and dangerous accessors (`CopyPixelDataTo`,
  `DangerousTryGetSinglePixelMemory`, `DangerousGetPixelRowMemory`, `GetPixelMemoryGroup`).
- Carries a wide pixel-format set in `CodeBrix.Imaging.PixelFormats` - 8-bit-per-channel color,
  grayscale and alpha-only, 16-bit-per-channel and HDR, packed low-bit-depth, and signed
  non-color data - all implementing `IPixel<TPixel>`.
- Converts across color spaces (`CieLab`, `CieLch`, `CieLchuv`, `CieLuv`, `CieXyy`, `CieXyz`,
  `Cmyk`, `Hsl`, `Hsv`, `HunterLab`, `LinearRgb`, `Lms`, `Rgb`, `YCbCr`) with
  `ColorSpaceConverter`, `Illuminants`, `RgbWorkingSpaces` and the companding helpers.
- Reads and writes metadata: EXIF through `ExifProfile` and strongly typed `ExifTag<T>` properties,
  plus XMP, ICC and IPTC profiles and per-format accessors such as `GetPngMetadata()`.
- Builds and edits frames: `ImageFrameCollection` with `AddFrame`, `InsertFrame`, `RemoveFrame`,
  `MoveFrame`, `CreateFrame`, `ExportFrame` and `CloneFrame`, and authors animated GIFs through
  per-frame `FrameDelay` and image-level `RepeatCount`.
- Exports 8bpp indexed grayscale BMP through `BmpFormatHelper`, for document-imaging pipelines,
  scanner integrations and legacy systems that demand that exact layout.
- Imports raw BGRA buffers with SIMD channel reordering (`Image.LoadPixelDataFromBgra`) and wraps
  caller-owned memory with no copy at all (`Image.WrapMemory`).

## When to use it

Reach for CodeBrix.Imaging whenever an application has to produce or change raster pixels: server
thumbnails, format conversion, watermarking, scanned-document pipelines, sprite and texture
preparation, or turning a native renderer's buffer into a PNG. It is a plain class library with no
initialization step, so it works the same in a console tool, a web service, a container and a
desktop application.

It deliberately has no vector drawing surface. There are no `DrawLine` / `DrawPolygon` / `FillPath`
/ `Brush` / `Pen` APIs here; compositing (`DrawImage`) and text (`DrawText`) are the ways to draw
onto an image in this package. For shapes, arrows, highlighter strokes and freehand pointer input,
add [CodeBrix.Imaging.Drawing](CodeBrix.Imaging.Drawing.md), whose `DrawingSession` exports straight
back to an `Image<Rgba32>`. For SVG, use [CodeBrix.SkiaSvg](CodeBrix.SkiaSvg.md) or
[CodeBrix.SvgParse](CodeBrix.SvgParse.md). For PDF, use
[CodeBrix.PdfDocuments](CodeBrix.PdfDocuments.md).

Other things the package does not do: RAW camera formats (.CR2, .NEF, .ARW, .DNG), HEIF/HEIC/AVIF,
JPEG 2000, JPEG XL, ICO, PSD or DICOM; animated WebP (encoding is single-frame only, and decoding
an animated WebP throws `NotSupportedException` - use GIF for animation); video decoding or frame
extraction; camera capture; OCR, barcode reading or any ML inference; GPU-accelerated processing
(everything is CPU SIMD plus parallel row iteration); any UI, window or screen-capture surface;
color management beyond carrying the ICC profile bytes; and rich text layout beyond a single
`TextOptions` block.

## Getting started

```bash
dotnet add package CodeBrix.Imaging.ApacheLicenseForever
```

The three namespaces most code needs, and the format namespaces beside them:

```csharp
using CodeBrix.Imaging;
using CodeBrix.Imaging.PixelFormats;
using CodeBrix.Imaging.Processing;
```

```csharp
using CodeBrix.Imaging.Formats.Png;
using CodeBrix.Imaging.Formats.Jpeg;
using CodeBrix.Imaging.Formats.Bmp;
using CodeBrix.Imaging.Formats.Gif;
using CodeBrix.Imaging.Formats.Webp;
using CodeBrix.Imaging.Formats.Tiff;
using CodeBrix.Imaging.Formats.Tga;
using CodeBrix.Imaging.Formats.Pbm;
```

A complete console project needs the package reference and nothing else:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>disable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.Imaging.ApacheLicenseForever" />
  </ItemGroup>
</Project>
```

This program loads a photo, resizes it to a fixed width with the aspect ratio preserved, converts
it to grayscale and writes a JPEG at a chosen quality:

```csharp
using CodeBrix.Imaging;
using CodeBrix.Imaging.Formats.Jpeg;
using CodeBrix.Imaging.Processing;

using var image = Image.Load("input.jpg");

image.Mutate(x => x
    .Resize(800, 0)             // 0 preserves the aspect ratio
    .Grayscale());

image.Save("output.jpg", new JpegEncoder { Quality = 85 });

Console.WriteLine($"Wrote {image.Width}x{image.Height}");
```

Notice that the whole pipeline is one `Mutate()` call, that the encoder is an ordinary object
initializer, and that there is no codec registration, no feature flag and no warm-up anywhere in
the program.

## Key concepts

### The `Image` and `Image<TPixel>` types

`Image` is the abstract, pixel-type-agnostic base; `Image<TPixel>` is the concrete generic. Both
implement `IDisposable`. Constructors take width and height plus an optional background pixel, an
optional `Configuration` and an optional `IImageFormat expectedFormat`. `Width`, `Height`,
`PixelType`, `Metadata`, `Format`, `HorizontalResolution`, `VerticalResolution` and `Frames` are
the properties you read; `Clone()`, `Clone(Configuration)`, `CloneAs<TPixel2>()` and
`CloneAs<TPixel2>(Configuration)` copy and convert.

Always dispose images with `using`. They own pooled buffers, and `Dispose()` is idempotent, so
disposing twice does not throw.

### Loading, saving and format detection

`Image.Load` has overloads for file paths, streams, byte arrays and `ReadOnlySpan<byte>`, in generic
and non-generic form, with an optional `Configuration`, an optional explicit `IImageDecoder` and an
optional `out IImageFormat format`; path and stream sources also have async variants. `Save` exists
as an instance method taking a stream and an encoder, and as extension methods taking a path, a path
and encoder, or a stream and format, alongside `SaveAsync`, `ToBase64String`, `ToByteArray` and
`ToByteArrayAsync`. Per-format shorthands `SaveAsBmp` / `Gif` / `Jpeg` / `Pbm` / `Png` / `Tga` /
`Tiff` / `Webp` each have an async twin and four shapes.

`Save(path)` throws `NotSupportedException` if the extension is not one of the eight supported
formats; call `DetectEncoder` first to test. `DetectFormat` and `Identify` read only the header and
give you an `IImageInfo` with `Width`, `Height`, `PixelType`, `Metadata`, `Format` and the two
resolutions, plus the `info.Size()` and `info.Bounds()` extension methods.

### Stream position and ReadOrigin

With the default configuration `Configuration.ReadOrigin` is `ReadOrigin.Current`, so every
`Load` / `Identify` / `DetectFormat` starts reading at the stream's current position and leaves it
advanced. Reset `stream.Position = 0` between calls, or set
`configuration.ReadOrigin = ReadOrigin.Begin` on your own `Configuration` and the library rewinds
seekable streams for you. Non-seekable streams, such as an HTTP request body, are buffered
internally, so they work without either step - but only once.

### Mutate versus Clone

`Mutate` returns void and edits the receiver in place. `Clone` returns a new image, which you must
dispose, and leaves the receiver untouched. Use `Clone` when you need thumbnails or variants beside
the original.

```csharp
using var original  = Image.Load("photo.jpg");
using var thumbnail = original.Clone(x => x.Resize(200, 200));
// original still has its full size
```

Both have `(Action<IImageProcessingContext>)`, `(Configuration, Action<...>)` and
`(params IImageProcessor[])` shapes.

### Resize and ResizeOptions

There are eight `Resize` overloads, and passing 0 for one dimension preserves the aspect ratio for
that axis. `ResizeOptions` carries `Mode` (default `ResizeMode.Crop`), `Position` (default
`AnchorPositionMode.Center`), `CenterCoordinates`, `Size`, `Sampler` (default
`KnownResamplers.Bicubic`), `Compand` (default false), `TargetRectangle`, `PremultiplyAlpha`
(default true) and `PadColor`. `ResizeMode` offers Crop, Pad, BoxPad, Max, Min, Stretch and Manual;
`KnownResamplers` offers Bicubic, Box, CatmullRom, Hermite, Lanczos2, Lanczos3, Lanczos5, Lanczos8,
MitchellNetravali, NearestNeighbor, Robidoux, RobidouxSharp, Spline, Triangle and Welch.

### Geometric transforms

`Rotate(90/180/270 as a float)` and `RotateMode.Rotate90` / `Rotate270` swap width and height;
`Rotate(float)` with an arbitrary angle grows the canvas to fit the rotated bounds.
`AffineTransformBuilder` builds a `Matrix3x2` and `ProjectiveTransformBuilder` builds a `Matrix4x4`,
adding `PrependTaper` / `AppendTaper` with `TaperSide` and `TaperCorner`. Every builder method
returns the builder so calls chain, and each has a `Prepend*` and an `Append*` form. `Transform`
throws `DegenerateTransformException` if the composed matrix collapses the image to zero area.

### Text rendering is on the image

Text rendering needs `using CodeBrix.Imaging.Fonts;` plus `using CodeBrix.Imaging.Fonts.Rendering;`.
`DrawText` is an extension on `Image` and `Image<TPixel>`, not on the `Mutate()` processing context:

```csharp
image.DrawText("hello", font, Color.White, 10f, 10f);      // CORRECT
image.Mutate(x => x.DrawText(...));                        // WRONG —
                                                           // does not compile
```

> [!IMPORTANT]
> `CodeBrix.Imaging.Drawing` is a separate package with its own namespaces; adding a
> `using CodeBrix.Imaging.Drawing;` does not make text rendering compile, and it does not create a
> shape API in this package.

Every overload returns the same image instance so calls chain. Null or empty text is a no-op; a null
image, font or options throws `ArgumentNullException`. The non-generic `Image` overloads recover the
pixel type internally, so you do not need to know it. `forceMonoColor` matters only for color fonts
(COLR/CPAL, an emoji font for instance): false, the default, renders the font's own glyph colors,
and true forces the color you passed. On the measuring side, `Measure` returns the advance-based
size and `MeasureBounds` returns the tight ink bounds, both as a `FontRectangle`.

```csharp
using CodeBrix.Imaging;
using CodeBrix.Imaging.Fonts;
using CodeBrix.Imaging.Fonts.Rendering;

using var image = Image.Load("photo.jpg");

var font = SystemFonts.CreateFont("Arial", 36f);

image.DrawText("Hello, world!", font, Color.White, 10f, 10f);

image.Save("photo-with-text.jpg");
```

### Fonts and glyph fallback

If any code point is missing from the primary `Font`, layout retries the whole string against each
family in `TextOptions.FallbackFontFamilies`, in order, at the same size and style, until one run
completes. Code points still unresolved after that render as the font's glyph 0 (.notdef). Set
`FallbackFontFamilies` explicitly for multi-script or emoji text - there is no automatic
system-wide fallback. Below the layout API, `TextRenderer` drives any `IGlyphRenderer`, and
`ImageGlyphRenderer<TPixel>` is the built-in one that rasterizes onto an `Image<TPixel>`. Outlines
are filled with the non-zero winding rule, so a glyph counter - the hole in an "o" - comes from an
oppositely wound contour, and coverage is alpha-blended over the destination.

### Pixel access

The `[x, y]` indexer exists only on the generic type; with a non-generic `Image`, call
`CloneAs<Rgba32>()` or load with `Image.Load<Rgba32>` first. `ProcessPixelRows` has one-, two- and
three-image forms on both `Image<TPixel>` and `ImageFrame<TPixel>`, and hands you a
`PixelAccessor<TPixel>` - a `ref struct` with `Width`, `Height` and `GetRowSpan(int rowIndex)`. The
buffer is pinned for the duration of the callback: do not let the accessor or any `Span` escape it,
and do not resize the image inside it. `DangerousTryGetSinglePixelMemory` succeeds only when the
backing buffer is contiguous, which means setting `PreferContiguousImageBuffers = true` on a
non-global `Configuration` instance before loading.

### Pixel formats and Color

`Rgba32` is the default and the format most APIs work in. `Bgra32` is binary compatible with
`System.Drawing.Imaging.PixelFormat.Format32bppArgb`, so it round-trips through `LockBits` buffers
byte for byte. Converting between pixel formats is a whole-image `CloneAs<TPixel2>()`.

`Color` is a pixel-format-independent value type with hex parsing (3, 4, 6 or 8 digits, with or
without a leading `#`), case-insensitive W3C named-color parsing, `WithAlpha`, `ToPixel<TPixel>`,
and the ready-made `Color.WebSafePalette` and `Color.WernerPalette`.

### Raw pixel import and zero-copy wrapping

`Image.LoadPixelData` requires a fourth argument here - the `IImageFormat` the resulting image
should be associated with - and passing only three arguments is a compile error (CS1501). That
`expectedFormat` sets `image.Metadata.ExpectedFormat`; it does not restrict what you may later save
as. For BGRA sources such as a PDF rasterizer, Direct2D, Cairo or GDI+, use
`Image.LoadPixelDataFromBgra`, which always returns `Image<Rgba32>` and reorders the channels
internally with SIMD straight into the image buffer - no scalar swap loop and no intermediate array.

`Image.WrapMemory<TPixel>` accepts `Memory<TPixel>`, `IMemoryOwner<TPixel>`, `Memory<byte>`,
`IMemoryOwner<byte>` and an unsafe `void*`, each with a required `IImageFormat expectedFormat`. The
image does not copy and does not free the buffer, so the buffer must be exactly width * height
pixels and must stay alive and unmoved for the lifetime of the image.

### Metadata and profiles

`image.Metadata` is an `ImageMetadata` and `image.Frames[i].Metadata` is an `ImageFrameMetadata`.
Any profile property may be null, so always null-check before reading. `ExifProfile` exposes its
tags as strongly typed static properties on `ExifTag`, grouped by value type so that `SetValue` and
`GetValue` are type-checked, and `InvalidTags` lists tags that could not be parsed from a malformed
source - truncated or corrupt EXIF blocks are tolerated rather than throwing. With `IptcProfile`,
call `UpdateData()` after mutating values if you intend to read `Data` directly.

### Frames and animation

Every image has at least one frame. `ExportFrame` removes the frame from the source image and hands
it to you as a standalone `Image`; `CloneFrame` copies it and leaves the collection intact.
`RemoveFrame`, and therefore `ExportFrame`, throws
`InvalidOperationException("Cannot remove last frame.")` if only one frame is left. To decode only
the first frame of a multi-frame GIF or TIFF, set `DecodingMode = FrameDecodingMode.First` on
`GifDecoder` or `TiffDecoder`, or cap `GifDecoder.MaxFrames`.

### 8bpp grayscale BMP export

`BmpFormatHelper`, in `CodeBrix.Imaging.Helpers`, writes an 8bpp indexed grayscale BMP.
`BmpIndexingMode.Normal` uses a 256-entry linear grayscale palette where index 0 is black and 255 is
white; `BmpIndexingMode.SystemDrawingCompatible` uses a 224-entry GDI+ halftone palette with
empirically matched quantization, so the bytes match what `System.Drawing` produced for
`Format8bppIndexed`. Three weighting matrices ship as public static readonly `ColorMatrix` fields:
`DefaultGrayscaleColorMatrix`, `Bt601GrayscaleColorMatrix` and `Bt709GrayscaleColorMatrix`. A custom
`ColorMatrix` controls only how the RGB channels are weighted into a single intensity - the output
is always grayscale, never color.

These are export methods, not save methods: they bypass the encoder pipeline, do not update
`Metadata.ExpectedFormat`, and leave the in-memory image unchanged.

### Configuration, memory and parallelism

`Configuration.Default` is the shared global instance; clone it or construct your own before
changing anything in a library or a server. `CreateSandboxed(int allocationLimitMegabytes)` returns
a configuration whose allocator refuses to exceed the given budget - the right way to process
untrusted uploads without a decompression bomb exhausting memory.
`MemoryAllocator.Default.ReleaseRetainedResources()` drops pooled buffers after a burst of
large-image work in a long-running process, and `MemoryDiagnostics` exposes the
`UndisposedAllocation` event and `TotalUndisposedAllocationCount` for leak hunting. A
`MaxDegreeOfParallelism` of 0, or below -1, throws `ArgumentOutOfRangeException`.

A default encoder can also be registered once, for every subsequent extension-based `Save`:

```csharp
Configuration.Default.ImageFormatsManager.SetEncoder(
    JpegFormat.Instance, new JpegEncoder { Quality = 80 });
```

### Exceptions

`ImageFormatException` is thrown when the library is asked to load an image whose format or content
is invalid or unsupported; `InvalidImageContentException` means the format was recognized but the
content is corrupt, and `UnknownImageFormatException` means the byte signature matched no registered
format. Both derive from `ImageFormatException`, so catching that one covers every "this file is not
usable" case at once. `ImageProcessingException` reports a processor that failed inside
`Mutate()` / `Clone()`, `InvalidMemoryOperationException` lives in `CodeBrix.Imaging.Memory`,
`DegenerateTransformException` comes from the transform processors, and the font layer adds
`FontException`, `FontFamilyNotFoundException`, `GlyphMissingException`, `InvalidFontFileException`,
`InvalidFontTableException` and `MissingFontTableException`.

## Examples

A web or API thumbnail pipeline that never touches the file system and caps how much memory an
upload may claim:

```csharp
using CodeBrix.Imaging;
using CodeBrix.Imaging.Formats.Jpeg;
using CodeBrix.Imaging.Processing;

public static async Task<byte[]> MakeThumbnailAsync(
    Stream upload, CancellationToken token)
{
    // Sandbox the allocator so a decompression bomb cannot exhaust
    // memory. CreateSandboxed already clones, so Default is untouched.
    var sandboxed = Configuration.Default.CreateSandboxed(
        allocationLimitMegabytes: 256);

    using var image = await Image.LoadAsync(sandboxed, upload, token);

    image.Mutate(x => x.Resize(new ResizeOptions
    {
        Size = new Size(320, 320),
        Mode = ResizeMode.Max
    }));

    using var output = new MemoryStream();
    await image.SaveAsync(output, new JpegEncoder { Quality = 75 }, token);
    return output.ToArray();
}
```

Sniffing a file before committing to a decode, which is also the clearest illustration of the
stream-position rule:

```csharp
using CodeBrix.Imaging;

using var stream = File.OpenRead("unknown-file");

var format = Image.DetectFormat(stream);
if (format is null)
{
    return;                       // not an image this library understands
}

stream.Position = 0;              // DetectFormat consumed the header
var info = Image.Identify(stream);
if ((long)info.Width * info.Height > 50_000_000)
{
    return;                       // refuse absurd dimensions
}

stream.Position = 0;
using var image = Image.Load(stream);
```

Per-pixel work over the whole image, using the pinned row accessor rather than the indexer:

```csharp
using CodeBrix.Imaging;
using CodeBrix.Imaging.PixelFormats;

using var image = Image.Load<Rgba32>("photo.png");

image.ProcessPixelRows(accessor =>
{
    for (var y = 0; y < accessor.Height; y++)
    {
        var row = accessor.GetRowSpan(y);
        for (var x = 0; x < row.Length; x++)
        {
            ref var p = ref row[x];
            if (p.R > 200 && p.G < 60 && p.B < 60)
            {
                p = new Rgba32(0, 0, 0, 0);      // knock out reds
            }
        }
    }
});

image.Save("keyed.png");
```

Building an animated GIF frame by frame, with per-frame delays and a disposal method:

```csharp
using CodeBrix.Imaging;
using CodeBrix.Imaging.Formats.Gif;
using CodeBrix.Imaging.PixelFormats;
using CodeBrix.Imaging.Processing;

using var animation = new Image<Rgba32>(200, 200, Color.Black.ToPixel<Rgba32>());
animation.Metadata.GetGifMetadata().RepeatCount = 0;             // loop forever
animation.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = 8;

for (var i = 1; i < 12; i++)
{
    using var step = new Image<Rgba32>(200, 200);
    step.Mutate(x => x.BackgroundColor(Color.FromRgb((byte)(i * 20), 40, 90)));

    var frame = animation.Frames.AddFrame(step.Frames.RootFrame);
    frame.Metadata.GetGifMetadata().FrameDelay = 8;               // 0.08s
    frame.Metadata.GetGifMetadata().DisposalMethod =
        GifDisposalMethod.RestoreToBackground;
}

animation.SaveAsGif("animation.gif");
```

## Using it in a CodeBrix.Platform application

There is nothing special to do. CodeBrix.Imaging is a plain .NET 10 class library with no native or
platform-specific dependencies, so a CodeBrix.Platform application references the package in its
`.Core` library and calls it like any other .NET code - no add-in, no registration, no head-specific
note. A Platform application that draws with a `DrawingSession` from
[CodeBrix.Imaging.Drawing](CodeBrix.Imaging.Drawing.md) pulls this library in transitively, because
that package depends on it for colors, sizes, image decoding and encoding, and the `Image<Rgba32>`
export type.

## Pitfalls

- Do not call `DrawText` inside `Mutate()`. `image.Mutate(x => x.DrawText(...))` does not compile -
  `DrawText` is an extension on `Image` / `Image<TPixel>`: `image.DrawText(text, font, color, x, y);`
- Do not write `using CodeBrix.Imaging.Drawing;` for text rendering. That namespace is not part of
  this package. Text rendering here is `using CodeBrix.Imaging.Fonts;` plus
  `using CodeBrix.Imaging.Fonts.Rendering;`.
- Do not try to draw shapes or freehand strokes with this package. There is no `DrawLine` /
  `DrawPolygon` / `FillPath` / `Brush` / `Pen` here. That surface ships in the separate
  [CodeBrix.Imaging.Drawing](CodeBrix.Imaging.Drawing.md) packages: the entry point is
  `DrawingSession`, the model types live in `CodeBrix.Imaging.Drawing.Models`, and the bridge back
  is `using CodeBrix.Imaging.Drawing.Extensions;` plus `session.ExportImagingImage()`, which returns
  an `Image<Rgba32>`.
- Do not call `.Saturation(...)`. The method is `.Saturate(float amount)`.
- Do not confuse the package ID with the namespace. Package:
  `CodeBrix.Imaging.ApacheLicenseForever`. Namespace: `CodeBrix.Imaging`.
- Do not forget `using CodeBrix.Imaging.Processing;`. Without it `Resize`, `Crop`, `Grayscale`,
  `Mutate` and `Clone` are not visible, and the error ("no definition for 'Mutate'") does not
  name the missing namespace.
- Do not call `Image.LoadPixelData` with three arguments. The `IImageFormat` fourth argument is
  mandatory here; pass `PngFormat.Instance` or whichever format applies.
- Do not feed BGRA bytes to `LoadPixelData<Rgba32>`. Red and blue will be swapped. Use
  `LoadPixelDataFromBgra`.
- Do not confuse stride with width when copying from a native buffer. Stride is often larger than
  width * bytesPerPixel because of alignment padding; index the source by stride and the destination
  by width * 4.
- Do not forget `stream.Position = 0` after `DetectFormat` / `Identify` before loading from the same
  stream. `Configuration.ReadOrigin` defaults to `ReadOrigin.Current`, so nothing rewinds the stream
  for you unless you set `ReadOrigin.Begin`.
- Do not use the `[x, y]` indexer on a non-generic `Image` - it only exists on `Image<TPixel>`.
- Do not let a `PixelAccessor` or a row `Span` escape the `ProcessPixelRows` callback, and do not
  dispose the image while a `Memory` obtained from `DangerousTryGetSinglePixelMemory` is still in
  use.
- Do not assume system fonts exist. Containers and CI agents frequently have none.
  `SystemFonts.Get(name)` and `FontCollection.Get(name)` throw `FontFamilyNotFoundException` when
  the family is missing; prefer `SystemFonts.TryGet(...)` with a `FontCollection` fallback, and ship
  the .ttf files you depend on as embedded resources or content.
- Do not expect automatic font fallback. Unresolved code points render as .notdef unless you
  populate `TextOptions.FallbackFontFamilies` yourself.
- Do not save an image with alpha to JPEG and expect transparency. Flatten it first with
  `.BackgroundColor(Color.White)`.
- Do not use `ExportFrame` when you meant `CloneFrame`. `ExportFrame` removes the frame from the
  source image, and removing the only remaining frame throws `InvalidOperationException`.
- Do not expect `ExportAs8bppGrayscaleBmpFormat` to behave like `Save`. It bypasses the encoder
  pipeline, does not update `Metadata.ExpectedFormat`, and leaves the in-memory image unchanged.
- Do not mutate `Configuration.Default` in library or server code. Clone it, or use
  `CreateSandboxed`, so you do not change global behavior for everyone else in the process.
- Do not skip disposal. `Image`, `ImageFrame` and `IMemoryOwner<T>` all hold pooled buffers.
  `Dispose` is idempotent, so `using` everywhere is safe.
- Do not decode untrusted input without limits. Use `Configuration.CreateSandboxed(...)`, check
  `Image.Identify` dimensions first, and set `GifDecoder.MaxFrames` or `FrameDecodingMode.First` for
  multi-frame formats.
- `Color.Empty` is a constant only and is not in the name lookup used by `Color.Parse`.

> [!TIP]
> Chain every operation inside one `Mutate()`; resize first when shrinking and last when enlarging;
> answer header-only questions with `Identify()` or `DetectFormat()`; pick the narrowest pixel type
> that carries your data; and quantize before saving an indexed PNG, GIF or BMP.

## Documentation and source

The repository has no samples folder and no tools folder: everything that ships is the library
project, and the test project doubles as the worked-example set, mapping each feature area to the
file that exercises it.

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Imaging/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Imaging/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Imaging/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.Imaging.Tests](https://github.com/ellisnet/CodeBrix.Imaging/tree/main/tests/CodeBrix.Imaging.Tests) |
| Library source | [src/CodeBrix.Imaging](https://github.com/ellisnet/CodeBrix.Imaging/tree/main/src/CodeBrix.Imaging) |

Run the suite from the repository root:

```bash
dotnet test CodeBrix.Imaging.slnx
```

## License

CodeBrix.Imaging is licensed under the Apache License 2.0; the license is also named in the package
ID (`CodeBrix.Imaging.ApacheLicenseForever`). For the provenance and licensing of open source code
included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Imaging/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Imaging.Drawing](CodeBrix.Imaging.Drawing.md) - shapes, arrows and freehand strokes over
  these images, exported straight back to an `Image<Rgba32>`
- [CodeBrix.SkiaSvg](CodeBrix.SkiaSvg.md) - when the input is SVG rather than raster pixels
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.Imaging on GitHub](https://github.com/ellisnet/CodeBrix.Imaging) - source and tests
