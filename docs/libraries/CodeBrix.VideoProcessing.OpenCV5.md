<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.VideoProcessing.OpenCV5</sub>

# CodeBrix.VideoProcessing.OpenCV5

**CodeBrix.VideoProcessing.OpenCV5 is a fully managed .NET binding for OpenCV 5: image processing, video
capture and analysis, camera calibration, object detection, machine learning, and the contrib extra
modules.** The managed core has no managed dependencies of its own; it calls into a single native library
that statically links OpenCV 5 plus the contrib modules, and it ships Roslyn analyzers that catch the
`Mat` mistakes that a compiler otherwise cannot see. Use it from any .NET 10 application, or from a
CodeBrix.Platform application, on Windows, Linux and macOS.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.VideoProcessing.OpenCV5](https://github.com/ellisnet/CodeBrix.VideoProcessing.OpenCV5) |
| **Packages** | [`CodeBrix.VideoProcessing.OpenCV5.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.OpenCV5.ApacheLicenseForever) - the managed core<br>[`CodeBrix.VideoProcessing.OpenCV5.Wpf.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.OpenCV5.Wpf.ApacheLicenseForever) - WPF converters<br>plus a native runtime package per OS and CPU pair, listed in [Getting started](#getting-started) |
| **License** | Apache License 2.0; see [License](#license) |
| **Requires** | .NET 10 or later, and a native runtime package for every platform the application runs on. On Linux, the GTK-3 runtime and Xlib |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Windows x64 and ARM64, Linux x64, ARM64 and RISC-V 64, macOS x64 and Apple Silicon |

## What it does

- Exposes the OpenCV 5 core, imgproc, imgcodecs, videoio, video, calib3d, features, flann, dnn, ml,
  objdetect, photo, stitching and highgui modules through the `Cv2` static class and the `Mat` family of
  types.
- Exposes the contrib extra modules: aruco, barcode, face, img_hash, line_descriptor, quality, saliency,
  shape, text, tracking, wechat_qrcode, xfeatures2d, ximgproc, xphoto, dnn_superres and more.
- Runs on Windows x64 and ARM64, Linux x64, ARM64 and RISC-V 64, and macOS x64 and Apple Silicon,
  through per-platform native runtime packages.
- Adds fast managed access to pixel data that the plain binding surface does not have: `Mat.ToArray<T>()`
  copies a continuous `Mat` of any dimensionality in row-major order, which is how you read an
  N-dimensional network output tensor; `Mat.AsRows<T>()`, `Mat.AsSpan<T>()` and `Mat.RowSpan<T>()` reach
  the pixels with no P/Invoke per row; and `NetExtensions.ForwardAll` reads every output of a
  multi-output network.
- Ships Roslyn analyzers that run in your build automatically and flag the `Mat` usage mistakes that
  produce silently wrong results - an undisposed submatrix, a property call in a loop condition, a row
  header allocated per iteration.
- Converts between `Mat` and `BitmapSource` / `WriteableBitmap` in a WPF application, with an in-place
  overload so a live preview reuses one bitmap instead of allocating per frame.
- Turns a failed native load into an actionable diagnostic instead of a raw `DllNotFoundException`,
  naming the exact runtime package to add for the current runtime identifier.

## When to use it

Reach for CodeBrix.VideoProcessing.OpenCV5 when the work is *pixels in process*: analyzing frames,
detecting and tracking objects, calibrating a camera, running an ONNX or TFLite model, or building any
pipeline where a managed loop touches image data directly. It is a computer-vision library that also
happens to read and write video files.

It is not a media-job runner and not a player. To build and run FFmpeg command lines - transcoding,
probing, snapshots, filter chains - use [CodeBrix.VideoProcessing](CodeBrix.VideoProcessing.md). To play
video in an application, use [CodeBrix.VideoPlayback](CodeBrix.VideoPlayback.md).

What is out of scope:

- No CUDA modules: there is no `cv::cuda` surface and no GPU-accelerated variant of the algorithms.
  Network inference runs on the CPU backend; the `Backend` and `Target` enums exist, but only the targets
  the shipped native build supports are available - query `Cv2.Dnn.GetAvailableBackends()`.
- Some members are not implemented and throw `NotImplementedException`: `ml.TrainData`,
  `StatModel.Train(TrainData, int)`, `StatModel.CalcError`, `SVM.TrainAuto`, and `Stitcher`'s
  pipeline-component properties.
- No Python, JavaScript or WASM bindings, and no browser-WASM runtime package.
- No native binaries beyond the listed runtime identifiers: no 32-bit Windows, no musl Linux, no Android
  or iOS runtime packages, no 32-bit ARM.
- No graphical toolkit of its own beyond OpenCV's own highgui windows; the only user-interface interop
  shipped is the WPF converter package.
- It downloads no models, cascades or OCR data for you - supply your own `.onnx`, `.tflite`, `.xml` and
  traineddata files.
- It bundles no sample data set and no test images, and it installs no system libraries: on Linux the
  GTK-3 and X11 runtime is your responsibility.

## Getting started

Two package references, always: the managed core, which is the only one with an assembly to compile
against, and a native runtime package for the platform you run on.

```bash
dotnet add package CodeBrix.VideoProcessing.OpenCV5.ApacheLicenseForever
dotnet add package CodeBrix.VideoProcessing.OpenCV5.LinuxX64.ApacheLicenseForever
```

Neither package depends on the other, in either direction, so both references are required.

<details>
<summary>The native runtime package for each runtime identifier</summary>

| Runtime identifier | Package |
| --- | --- |
| `win-x64` | [`CodeBrix.VideoProcessing.OpenCV5.WindowsX64.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.OpenCV5.WindowsX64.ApacheLicenseForever) |
| `win-arm64` | [`CodeBrix.VideoProcessing.OpenCV5.WindowsArm64.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.OpenCV5.WindowsArm64.ApacheLicenseForever) |
| `linux-x64` | [`CodeBrix.VideoProcessing.OpenCV5.LinuxX64.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.OpenCV5.LinuxX64.ApacheLicenseForever) |
| `linux-arm64` | [`CodeBrix.VideoProcessing.OpenCV5.LinuxArm64.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.OpenCV5.LinuxArm64.ApacheLicenseForever) |
| `linux-riscv64` | [`CodeBrix.VideoProcessing.OpenCV5.LinuxRiscv64.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.OpenCV5.LinuxRiscv64.ApacheLicenseForever) |
| `osx-x64` | [`CodeBrix.VideoProcessing.OpenCV5.MacOSX64.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.OpenCV5.MacOSX64.ApacheLicenseForever) |
| `osx-arm64` | [`CodeBrix.VideoProcessing.OpenCV5.MacOSArm64.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.VideoProcessing.OpenCV5.MacOSArm64.ApacheLicenseForever) |

</details>

On a headless or container Linux image, install the system libraries the native binary links
dynamically:

```bash
sudo apt-get install -y libgtk-3-0 libx11-6
```

`libgtk-3-0` pulls in gdk, pango, cairo, gdk-pixbuf, glib, freetype and harfbuzz; on distributions that
completed the 64-bit `time_t` transition the GTK package is named `libgtk-3-0t64`. A desktop image
already has all of it, and the macOS and Windows binaries need nothing extra.

The namespaces follow the modules. There is no registration call.

```csharp
using CodeBrix.VideoProcessing.OpenCV5;             // Cv2, Mat, Scalar,
                                                    // Size, Rect, Point,
                                                    // VideoCapture, ...
using CodeBrix.VideoProcessing.OpenCV5.Dnn;         // Net, Model, blobs
using CodeBrix.VideoProcessing.OpenCV5.ML;          // SVM, RTrees, KNearest
using CodeBrix.VideoProcessing.OpenCV5.Flann;       // FLANN index params
using CodeBrix.VideoProcessing.OpenCV5.Aruco;       // ArUco / ChArUco
using CodeBrix.VideoProcessing.OpenCV5.Face;        // face recognition
using CodeBrix.VideoProcessing.OpenCV5.Tracking;    // TrackerCSRT/KCF
using CodeBrix.VideoProcessing.OpenCV5.Text;        // OCR / text detection
using CodeBrix.VideoProcessing.OpenCV5.XImgProc;    // extended imgproc
using CodeBrix.VideoProcessing.OpenCV5.XFeatures2D; // SURF, BRISK, KAZE...
using CodeBrix.VideoProcessing.OpenCV5.XPhoto;      // white balance, ...
using CodeBrix.VideoProcessing.OpenCV5.ImgHash;     // perceptual hashes
using CodeBrix.VideoProcessing.OpenCV5.Quality;     // PSNR/SSIM/BRISQUE
using CodeBrix.VideoProcessing.OpenCV5.Saliency;    // saliency detectors
using CodeBrix.VideoProcessing.OpenCV5.LineDescriptor; // LSDDetector
using CodeBrix.VideoProcessing.OpenCV5.DnnSuperres; // DnnSuperResImpl
using CodeBrix.VideoProcessing.OpenCV5.Segmentation; // IntelligentScissorsMB
using CodeBrix.VideoProcessing.OpenCV5.XImgProc.Segmentation;
                                                    // graph / selective
                                                    // search segmentation
using CodeBrix.VideoProcessing.OpenCV5.Detail;      // stitching internals
using CodeBrix.VideoProcessing.OpenCV5.Extensions;  // CvExtensions
using CodeBrix.VideoProcessing.OpenCV5.Wpf;         // WPF converters
                                                    // (separate package)
```

Several modules that have their own namespace elsewhere live in the root namespace here, so no extra
`using` is needed for `VideoCapture`, `VideoWriter`, `FourCC`, `CascadeClassifier`, `QRCodeDetector`,
`HOGDescriptor`, `FaceDetectorYN`, `Tracker`, `TrackerMIL`, `KalmanFilter`, the background subtractors,
`SIFT`, `ORB`, `BFMatcher`, `FlannBasedMatcher`, `Stitcher`, the photo-module tonemap and merge types,
`Subdiv2D`, `CLAHE`, or any geometry or value struct.

This is a whole program: read an image, find its edges, save the result.

```csharp
using CodeBrix.VideoProcessing.OpenCV5;

using var src = Cv2.ImRead("input.jpg", ImreadModes.Grayscale);
using var dst = new Mat();

Cv2.Canny(src, dst, 50, 200);
Cv2.ImWrite("edges.png", dst);
```

Notice the two `using` declarations. Every native-backed type is `IDisposable`, and `using var` is the
house style throughout this library.

## Key concepts

### Which packages to reference

Always reference the managed core: it is the only package that contains an assembly, and it is what your
code compiles against. Then reference the native runtime package for every platform the application must
*run* on. A portable, runtime-identifier-less application - a plain `dotnet build`, a `dotnet run`, or a
framework-dependent publish with no `-r` - references every platform you intend to support, and because
the native binaries are large it is worth trimming that list to the platforms you actually ship to. For a
runtime-identifier-specific publish, the matching package is the only one whose payload is deployed, so
referencing only that one keeps restore small; referencing them all is also fine.

Reference the `.Wpf` package only from a `net10.0-windows` WPF application. It depends on the managed
core and on `System.Drawing.Common`. The managed core has no NuGet dependencies at all, and neither does
any native runtime package - in particular a native package does not depend on the core, which is why
both references are needed.

### How the right native binary is found

Each native package places its binary at `runtimes/<rid>/native/` inside the package. There is no
MSBuild logic and no import resolver: selection is the .NET runtime's ordinary native-library probing
over that layout. The SDK records the assets in the application's `.deps.json`, the host adds the
directory matching the running runtime identifier to `NATIVE_DLL_SEARCH_DIRECTORIES`, and the first
P/Invoke loads the library from there. The binding declares
`[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]`, so on Windows the
application directory and PATH are searched as well.

```mermaid
flowchart LR
  Core[Managed core assembly] --> Probe[Runtime native probing]
  Native[Native runtime package] --> Layout[runtimes rid native]
  Layout --> Probe
  Probe --> Load[Native binding library loaded]
```

### The `Mat` type

`Mat` is the n-dimensional dense array that carries every image and matrix. It wraps native memory and
implements `IDisposable` - always dispose it, and dispose every `Mat` that a method hands back to you.

Build one with a constructor, with the `Zeros` / `Ones` / `Eye` statics, or from memory you already have
with `FromPixelData`, which is the supported way to wrap an existing buffer. Decode compressed bytes with
`ImDecode`, `FromImageData` or `FromStream`. Shape and buffer queries are `Rows`/`Height`,
`Cols`/`Width`, `Dims`, `Shape()`, `Size()`, `Type()`, `Depth()`, `Channels()`, `ElemSize()`, `Step()`,
`Total()`, `Empty()`, `IsContinuous()`, `Data` and `DataPointer`. `Rows` and `Cols` report -1 when
`Dims > 2`.

Element access comes in three speeds, and the difference matters in a per-frame loop:

- `Get<T>` / `Set<T>` and `At<T>` read one element and cost one P/Invoke per call. `T` is the whole
  element type: `byte` for `CV_8UC1`, `Vec3b` for `CV_8UC3`, `Vec4b` for `CV_8UC4`, `float` for
  `CV_32FC1`, `Vec3f` for `CV_32FC3`.
- `AsRows<T>()` captures the data pointer, the row step and the dimensions once and returns a ref struct
  with a `Count` and an indexer giving each row as a `Span<T>` - zero P/Invoke and zero allocation per
  row, for two-dimensional Mats. Because it is a ref struct it cannot be captured by a lambda, so inside
  a `Parallel.For` call `mat.AsRows<T>()` again in each iteration.
- `AsSpan<T>()` gives the whole continuous buffer, and returns an empty span when the `Mat` is not
  continuous. `ToArray<T>()` copies a continuous `Mat` of any dimensionality out in row-major order, and
  throws `OpenCvSharpException` when the `Mat` is not continuous.

`GetArray<T>` and `GetRectangularArray<T>` size their result from `Rows x Cols`, which are -1 when
`Dims > 2`, so they cannot read an N-dimensional `Mat`; `ToArray<T>()` is the one that can.

Every one of `Row`, `Col`, `RowRange`, `ColRange`, `SubMat` and the rectangle and range indexers returns
a new `Mat` header that shares the parent's pixels and must be disposed. `Clone()` copies the pixels;
`SubMat()` does not.

`MatType` is a readonly record struct over the OpenCV type code, with static fields for the whole
depth-and-channel grid, the `CV_8UC(int ch)`-style factories, `MakeType`, and `Value`, `Depth`,
`Channels` and `IsInteger`.

### The array adapters

Most `Cv2` methods take `InputArray`, `OutputArray` or `InputOutputArray`. They are readonly ref structs
with implicit conversions from `Mat`, `UMat`, `MatExpr`, `Scalar`, `double` and every `Vec` struct, so in
practice you pass a `Mat` and never name them:
`Cv2.CvtColor(src, dst, ColorConversionCodes.BGR2GRAY);`. Because they are ref structs, they cannot be
stored in a field, captured by a lambda, used as a generic type argument, or held across an `await`. Keep
them as arguments and locals, and write `default`, not `null`, for an omitted optional `InputArray`.

The geometry and value types - `Point`, `Size`, `Rect`, `Range`, `RotatedRect`, `TermCriteria`,
`Scalar`, `KeyPoint`, `DMatch`, the `Vec*` family, `Moments` and the line and circle segments - are
record structs in the root namespace. `Scalar` carries the named drawing colors, plus `Scalar.All`,
`Scalar.FromRgb` and `Scalar.RandomColor()`. The channel order of an 8UC3 image is B, G, R, and
`FromRgb` reorders for you.

### Images in, images out

`Cv2.ImRead`, `ImReadMulti`, `ImWrite`, `ImDecode`, `ImEncode`, `HaveImageReader` and `HaveImageWriter`
cover file and byte-buffer I/O, with `ImreadModes` choosing grayscale, color, unchanged, reduced or
GDAL-backed loading, and `ImageEncodingParam` carrying `JpegQuality`, `PngCompression`, `WebPQuality` or
`TiffCompression`. `ImRead` returns a `Mat` whose `Empty()` is true when the file could not be read - it
does not throw, so check every load.

The imgproc surface is the usual one, reached through `Cv2`: `Resize`, `WarpAffine`, `WarpPerspective`,
`Remap`, `PyrDown` and `PyrUp` for geometry; `CvtColor` for color; `GaussianBlur`, `MedianBlur`,
`BilateralFilter`, `Filter2D`, `Sobel`, `Scharr`, `Laplacian` and `Canny` for filtering and edges;
`Threshold`, `AdaptiveThreshold`, `GetStructuringElement`, `Dilate`, `Erode`, `MorphologyEx`,
`EqualizeHist`, `CalcHist` and `CLAHE` for thresholding, morphology and histograms; `FindContours`,
`DrawContours`, `ApproxPolyDP`, `ContourArea`, `BoundingRect`, `MinAreaRect`, `ConvexHull`,
`ConnectedComponents`, `Watershed`, `GrabCut`, `DistanceTransform` and `FloodFill` for shape analysis;
and `GoodFeaturesToTrack`, `CornerSubPix`, `CornerHarris`, `HoughLines`, `HoughLinesP`, `HoughCircles`
and `MatchTemplate` for corners, lines and template matching. Use `InterpolationFlags.Area` when
shrinking and `Cubic` or `Linear` when enlarging.

Drawing is `Cv2.Line`, `Rectangle`, `Circle`, `Ellipse`, `Polylines`, `FillPoly`, `PutText` and
`GetTextSize`, with the Hershey fonts and `LineTypes.AntiAlias`; `Cv2.FILLED` as the thickness fills the
shape. A TrueType-capable text path exists as well, through `FontFace` and the matching `PutText` and
`GetTextSize` overloads.

### Video capture and writing

`VideoCapture` opens a camera index or a file, through a constructor or the `FromCamera` / `FromFile`
statics, and gives you `IsOpened()`, `Read`, `Grab`, `Retrieve`, `Release`, `Set`, `Get` and typed
property shortcuts for `FrameWidth`, `FrameHeight`, `Fps`, `FrameCount`, `FourCC`, the position
properties and the camera controls. Pass `VideoCaptureAPIs.ANY` unless you must pin a backend.

`VideoWriter` takes a file name, a `FourCC`, a frame rate and a frame size, and every frame handed to
`Write` must have exactly that size and, unless it was opened with `isColor: false`, three 8-bit
channels. `FourCC` is a readonly struct with implicit conversions to and from `int`, named codes such as
`FourCC.MJPG`, `X264`, `H264`, `HEVC` and `MP4V`, and the `FromString` and `FromFourChars` factories.

### Detection, tracking and machine learning

Feature detectors and descriptors derive from `Feature2D` - `SIFT`, `ORB`, `AKAZE`, `KAZE`, `BRISK`,
`SURF`, `MSER`, `SimpleBlobDetector` and the rest - each created through a static `Create` with named
parameters, and matched with `BFMatcher` or `FlannBasedMatcher`. Use `NormTypes.Hamming` for binary
descriptors and `NormTypes.L2` for float descriptors.

Calibration and 3D geometry are `FindChessboardCorners`, `FindCirclesGrid`, `CalibrateCamera`,
`Undistort`, `SolvePnP`, `SolvePnPRansac`, `ProjectPoints`, `Rodrigues`, `FindHomography`,
`StereoCalibrate` and `StereoRectify`, with `StereoBM` and `StereoSGBM` for stereo matching and the
fisheye variants under the nested `Cv2.FishEye` class.

Object detection covers `CascadeClassifier` for Haar and LBP cascades, `QRCodeDetector`,
`HOGDescriptor`, `FaceDetectorYN`, `BarcodeDetector` and `WeChatQRCode`. Motion and tracking cover
`CalcOpticalFlowPyrLK`, `CalcOpticalFlowFarneback`, `CamShift`, `MeanShift`, `FindTransformECC`, the
`BackgroundSubtractor` family and `TrackerMIL` / `TrackerKCF` / `TrackerCSRT`, alongside `KalmanFilter`.

Machine-learning models all derive from `StatModel`, with `Train(InputArray samples, SampleTypes layout,
InputArray responses)`, `Predict`, `GetVarCount()`, `IsTrained()` and `IsClassifier()`; `SVM`, `RTrees`,
`DTrees`, `KNearest`, `ANN_MLP`, `Boost`, `EM`, `LogisticRegression` and `NormalBayesClassifier` each
offer `Create`, `Load` and `LoadFromString`.

> [!IMPORTANT]
> `ml.TrainData` is not implemented: its constructor throws `NotImplementedException`, and so do the
> members that take one - `StatModel.Train(TrainData, int)`, `StatModel.CalcError` and `SVM.TrainAuto`.
> Train through `Train(InputArray samples, SampleTypes layout, InputArray responses)` with two `Mat`s
> you build yourself, and evaluate with `Predict`.

### The network module

Load a network with `Cv2.Dnn.ReadNet`, `ReadNetFromONNX`, `ReadNetFromTFLite`, `ReadNetFromTensorflow`
or `ReadNetFromModelOptimizer` - the ONNX and TFLite readers each take a path, a `byte[]`, a
`ReadOnlySpan<byte>` or a `Stream`, and the same factories are static members of `Net` itself. Prepare
input with `Cv2.Dnn.BlobFromImage` or `BlobFromImages`, and post-process with `NMSBoxes`,
`NMSBoxesBatched` or `SoftNMSBoxes`. The high-level wrappers - `ClassificationModel`, `DetectionModel`,
`SegmentationModel`, `KeypointsModel`, `TextDetectionModel` and `TextRecognitionModel` - wrap all of that
behind `SetInputParams` and one call.

Feeding a managed frame in looks like this:

```csharp
using var bgra = Mat.FromPixelData(height, width, MatType.CV_8UC4, pixelBytes);
using var bgr = new Mat();
Cv2.CvtColor(bgra, bgr, ColorConversionCodes.BGRA2BGR);
using var blob = Cv2.Dnn.BlobFromImage(bgr, 1.0 / 255, new Size(192, 192),
    new Scalar(0, 0, 0), swapRB: true, crop: false);
net.SetInput(blob);
```

`BlobFromImage` accepts one- and three-channel inputs, not four, which is why the BGRA frame is converted
first. `swapRB: true` feeds RGB, which most models expect. Letterbox - pad while preserving aspect -
before `BlobFromImage` when the model assumes it, because plain stretching degrades accuracy noticeably.

The TFLite reader works out of the box, but operator coverage is partial and an unsupported operator
throws at load time with `Unsupported operator type XYZ in function 'populateNet'`; GATHER and DENSIFY
are two that fail. That reader also registers only one output, so `Net.Forward(outputBlobs,
outBlobNames)` - which requires the requested names to match the network's registered unconnected
outputs exactly - always throws for a multi-output TFLite model. Read those with the extension method
instead:

```csharp
net.SetInput(blob);
var outputs = net.ForwardAll("Identity", "Identity_1");
```

`ForwardAll` issues sequential single-name `Forward` calls, and with an unchanged input OpenCV reuses the
already-computed layers, so the extra outputs are close to free. Dispose every returned `Mat`. To
discover a model's output names, probe candidates with single `Forward(name)` calls or list
`net.GetLayerNames()` and `net.GetUnconnectedOutLayersNames()`.

Network outputs are usually N-dimensional, so read them with `ToArray<float>()` and index the row-major
array yourself. And do not assume every "score" output is a logit: probe a new model with a real positive
and a blank input and check which interpretation separates them.

### The bundled analyzers

The core package ships four Roslyn analyzers, which run in your build automatically once you reference
it. All are warnings, and each marks a real defect class - fix the code rather than suppressing them
wholesale.

| ID | Category | What it catches |
| --- | --- | --- |
| `OCVS001` | Correctness | `At<T>(int)` called on a `Mat` row submatrix. `mat.Row(i)` returns a 1xN two-dimensional submatrix, so `At<T>(int i0)` treats `i0` as a row index, not a column index |
| `OCVS002` | Performance | A `Mat` property - `Rows`, `Cols`, `Dims`, `Width`, `Height` - evaluated in a loop condition. Each one is a P/Invoke; cache it in a local |
| `OCVS003` | Performance | `Mat.Row()` or `Mat.Col()` called inside a loop body, allocating a native header on every iteration |
| `OCVS004` | Reliability | A submatrix from `Row`, `Col`, `RowRange` or `ColRange` that is never disposed |

### When the native library will not load

The managed core calls into one native library, loaded lazily inside a static constructor that runs the
first time you touch any API - typically `new Mat()`. When that load fails you do not get the runtime's
raw `DllNotFoundException`. You get an `OpenCvSharpException` with the original exception as its
`InnerException` and a root-cause-first message that distinguishes four cases:

1. Not found in any probing directory. The message lists every directory probed and names the exact
   runtime package to add for the current runtime identifier.
2. Found, but the operating-system loader rejected it. The message surfaces the loader's own error, which
   almost always names a missing native dependency, and adds the right inspection command for the
   platform - `ldd`, `otool -L` or `dumpbin /dependents`. On Linux this is nearly always the GTK-3 and
   X11 stack.
3. Found, but the wrong CPU architecture for this process.
4. Found and loads fine on its own - a probing or deployment mismatch. Ship it through the runtime
   package, or pre-load it yourself with `NativeLibrary.Load(path)`.

Two pre-flight hooks are public: `NativeMethods.LoadLibraries()` forces the load now, and
`NativeMethods.TryPInvoke()` throws the same diagnostic-bearing exception and also writes it to the
console and the debugger. Call `TryPInvoke()` after installing any custom loading to fail fast with a
readable message. Nothing else in that namespace is for application code.

### Errors and disposal

`OpenCVException` is thrown when native OpenCV raises an error, and carries `Status`, `FuncName`,
`ErrMsg`, `FileName` and `Line` beside the `Message`: catch it for "the image was the wrong type, the
file did not decode, the assertion failed". `OpenCvSharpException` is the managed-side exception, used
for binding-level failures such as a native library that would not load. Ordinary
`ArgumentNullException`, `ArgumentException` and `ObjectDisposedException` come from the managed argument
checks. Details of a native error are captured on the calling thread directly from the thrown C++
exception, and no managed error callback is installed by default, which keeps the default path friendly
to trimming and ahead-of-time compilation; `Cv2.SetErrorHandler` installs one when you want it.

Every native-backed type derives from `CvObject : IDisposable` and exposes `IsDisposed`, `CvPtr` and
`ThrowIfDisposed()`. `Mat`, `UMat`, `VideoCapture`, `VideoWriter`, `Net`, every `Feature2D`,
`DescriptorMatcher`, `StatModel`, `Tracker` and `BackgroundSubtractor`, `CascadeClassifier` and `Window`
must all be disposed, and so must every `Mat` returned from `Row`, `Col`, `SubMat`, `Clone`, `Split` or
`ForwardAll`.

### highgui windows

`Cv2.ImShow`, `WaitKey`, `NamedWindow`, `DestroyWindow`, `DestroyAllWindows`, `ResizeWindow`,
`MoveWindow`, `SetWindowTitle`, `SetMouseCallback` and `CreateTrackbar` are all present, with an
object-oriented `Window` wrapper and `CvTrackbar` beside them. They need a graphical session, and on
Linux the GTK-3 stack and a running display server; in a headless container they fail. Everything else in
the library works headless.

### WPF interop

The `.Wpf` package adds two static extension classes in `CodeBrix.VideoProcessing.OpenCV5.Wpf`.
`WriteableBitmapConverter` converts a `Mat` to a `WriteableBitmap` and back, including an in-place
`ToWriteableBitmap(Mat src, WriteableBitmap dst)` overload; `BitmapSourceConverter` does the same for
`BitmapSource`. The parameterless overloads pick the pixel format from the `MatType`, and an unsupported
type throws `ArgumentOutOfRangeException("Not supported MatType")`. The in-place overloads require
matching sizes and channel counts and throw `ArgumentException` otherwise - but they let you reuse one
`WriteableBitmap` for every video frame instead of allocating per frame, which is what a live preview
wants.

## Examples

Find edges and contours, then annotate the original.

```csharp
using var src = Cv2.ImRead("shapes.png", ImreadModes.Color);
using var gray = new Mat();
Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

using var blurred = new Mat();
Cv2.GaussianBlur(gray, blurred, new Size(5, 5), 1.5);

using var edges = new Mat();
Cv2.Canny(blurred, edges, 80, 160);

Cv2.FindContours(edges, out Point[][] contours,
    out HierarchyIndex[] hierarchy,
    RetrievalModes.External, ContourApproximationModes.ApproxSimple);

foreach (var contour in contours)
{
    if (Cv2.ContourArea(contour) < 100)
        continue;

    Rect box = Cv2.BoundingRect(contour);
    Cv2.Rectangle(src, box, Scalar.Lime, 2);
    Cv2.PutText(src, $"{Cv2.ContourArea(contour):F0}",
        new Point(box.X, box.Y - 4),
        HersheyFonts.HersheySimplex, 0.5, Scalar.Yellow, 1,
        LineTypes.AntiAlias);
}

Cv2.DrawContours(src, contours, -1, Scalar.Red, 1);
Cv2.ImWrite("annotated.png", src);
```

Run a webcam preview with a frame-rate readout. The one `Mat` is allocated before the loop and reused for
every frame, which is the single biggest avoidable cost in a capture loop.

```csharp
using var capture = VideoCapture.FromCamera(0);
if (!capture.IsOpened())
    throw new InvalidOperationException("No camera at index 0.");

capture.Set(VideoCaptureProperties.FrameWidth, 1280);
capture.Set(VideoCaptureProperties.FrameHeight, 720);

using var frame = new Mat();          //reuse the SAME Mat every frame
long frequency = (long)Cv2.GetTickFrequency();
long previous = Cv2.GetTickCount();

while (true)
{
    if (!capture.Read(frame) || frame.Empty())
        break;

    long now = Cv2.GetTickCount();
    double fps = frequency / (double)(now - previous);
    previous = now;

    Cv2.PutText(frame, $"{fps:F1} fps", new Point(12, 32),
        HersheyFonts.HersheySimplex, 1.0, Scalar.Lime, 2);

    Cv2.ImShow("camera", frame);
    if (Cv2.WaitKey(1) == 27)         //Esc
        break;
}
Cv2.DestroyAllWindows();
```

Read every pixel of an image three ways, in increasing order of speed.

```csharp
using var image = Cv2.ImRead("photo.png", ImreadModes.Color); //CV_8UC3

//(a) one element - convenient, one P/Invoke per call
Vec3b pixel = image.Get<Vec3b>(10, 20);      //(row, col)

//(b) whole rows - no P/Invoke, no allocation (2-D Mats only)
var rows = image.AsRows<Vec3b>();
int rowCount = rows.Count;                   //cache; never in the
for (int y = 0; y < rowCount; y++)           //loop condition
{
    Span<Vec3b> row = rows[y];
    for (int x = 0; x < row.Length; x++)
        row[x] = new Vec3b(row[x].Item2, row[x].Item1, row[x].Item0);
}

//(c) the whole buffer as one managed array (continuous Mats only)
byte[] raw = image.ToArray<byte>();
```

Detect objects with an ONNX model through the high-level wrapper, which does the blob, the forward pass
and the non-maximum suppression for you.

```csharp
using CodeBrix.VideoProcessing.OpenCV5.Dnn;

using var model = new DetectionModel("yolo.onnx");
model.SetInputParams(scale: 1.0 / 255, size: new Size(640, 640),
    mean: new Scalar(0, 0, 0), swapRB: true, crop: false);

using var image = Cv2.ImRead("street.jpg", ImreadModes.Color);
model.Detect(image, out int[] classIds, out float[] confidences,
    out Rect[] boxes, confThreshold: 0.4f, nmsThreshold: 0.45f);

for (int i = 0; i < boxes.Length; i++)
{
    Cv2.Rectangle(image, boxes[i], Scalar.Lime, 2);
    Cv2.PutText(image, $"{classIds[i]} {confidences[i]:F2}",
        new Point(boxes[i].X, boxes[i].Y - 6),
        HersheyFonts.HersheySimplex, 0.5, Scalar.Yellow, 1);
}
Cv2.ImWrite("detections.png", image);
```

## Using it in a CodeBrix.Platform application

This library has no element, control or add-in for a page: it is a plain .NET library you call from a
service or a view model, and the only user-interface interop it ships is the WPF converter package. To
show a result on a CodeBrix.Platform head, get the pixels out of the `Mat` - `ToArray<byte>()`,
`AsSpan<T>()` or `ToBytes(".png")` - and hand them to whatever the head draws with. Keep
`Cv2.ImShow` and the `Window` class out of it: they need a graphical session and, on Linux, the GTK-3
stack, while every other part of the library works headless.

Two deployment facts follow the application onto each head. The managed core and one native runtime
package per platform must both be referenced, because neither pulls in the other. And on Linux the GTK-3
and X11 runtime must be installed, or the native library does not load at all - the diagnostic message
will surface the loader's own error naming what is missing.

The `.Wpf` package is meaningful only in a `net10.0-windows` WPF application, where reusing one bitmap
per frame is the pattern to copy:

```csharp
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using CodeBrix.VideoProcessing.OpenCV5;
using CodeBrix.VideoProcessing.OpenCV5.Wpf;

public partial class MainWindow : Window
{
    private readonly VideoCapture capture = new(0);
    private readonly Mat frame = new();
    private WriteableBitmap? bitmap;

    private void OnTick(object sender, EventArgs e)
    {
        if (!capture.Read(frame) || frame.Empty())
            return;

        //Allocate once, then reuse the same WriteableBitmap.
        bitmap ??= frame.ToWriteableBitmap();
        WriteableBitmapConverter.ToWriteableBitmap(frame, bitmap);
        PreviewImage.Source = bitmap;   //<Image x:Name="PreviewImage"/>
    }
}
```

## Pitfalls

- Forgetting the native runtime package. The core package alone compiles fine and then throws
  `OpenCvSharpException` at the first `new Mat()`. The exception message names the exact package to add -
  read it.
- Assuming a native runtime package pulls in the core, or the other way round. Neither depends on the
  other; reference both.
- Expecting `Cv2.ImRead` to throw on a missing or unreadable file. It returns a `Mat` whose `Empty()` is
  true. Check every load.
- Not disposing the Mats returned by `Row()`, `Col()`, `RowRange()`, `ColRange()`, `SubMat()`, `Clone()`,
  `Cv2.Split()` or `net.ForwardAll()`. They are native allocations, and the finalizer is not a plan.
- Writing `mat.Row(i).At<T>(j)`. `Row(i)` returns a 1xN two-dimensional `Mat`, so `At<T>(int)` indexes its
  row dimension. Use `mat.At<T>(i, j)` or `mat.AsRows<T>()`.
- Reading an N-dimensional `Mat` with `GetArray<T>` or `GetRectangularArray<T>`. They size from
  `Rows x Cols`, both -1 when `Dims > 2`. Use `ToArray<T>()` for network output tensors - and remember it
  requires a continuous `Mat`, so `Clone()` a submatrix or region-of-interest view first.
- Calling `AsSpan<T>()` on a non-continuous `Mat`. It returns an empty span; check `IsContinuous()` first.
- Storing an `InputArray` / `OutputArray` / `InputOutputArray`. They are ref structs: no fields, no
  lambdas, no generic type arguments, nothing across an `await`.
- Forgetting that the channel order is BGR, not RGB. `Scalar.FromRgb` and the named colors already
  account for it; raw byte triples do not, and models almost always want `swapRB: true`.
- Handing a four-channel BGRA frame to `Cv2.Dnn.BlobFromImage`. Convert it with
  `ColorConversionCodes.BGRA2BGR` first.
- Ignoring `VideoWriter.IsOpened()`, or writing a frame whose size differs from the `frameSize` the writer
  was opened with. Either one produces an unusable file, silently.
- Treating `VideoCapture.Read` returning false as end of stream. It also returns false on a transient
  grab failure; test `frame.Empty()` as well.
- Sharing a `Net` between threads. They are not thread-safe: one per worker thread, and a single
  latest-wins pending frame slot in front of it, so slow inference drops stale frames instead of queueing
  them.
- Adding a second OpenCV binding to the project. This library carries its own; the type names are
  identical, so mixing two bindings produces ambiguous-reference errors rather than a clear failure.
- Expecting `Stitcher`'s component properties - `FeaturesFinder`, `Blender`, `Warper` and the rest - to
  work. They throw `NotImplementedException`; only the high-level `Stitch`, `EstimateTransform` and
  `ComposePanorama` path works.
- Forgetting that the package IDs carry `.ApacheLicenseForever` while the namespaces never do.

> [!TIP]
> Allocate Mats once and reuse them across frames - per-frame `new Mat()` is the single biggest
> avoidable cost. Convert to grayscale before detection or feature work, prefer
> `InterpolationFlags.Area` for downscaling, bound the internal thread pool with `Cv2.SetNumThreads(n)`,
> and time with `Cv2.GetTickCount()` and `Cv2.GetTickFrequency()`. `FourCC.MJPG` is cheap and
> universally available; `FourCC.X264` or `H264` gives far smaller files but leans on the platform's
> codec support.

## Samples and tools in the repository

There are no sample or demo applications in this repository. The non-package content is the native build
pipeline, the native binaries themselves, and the test projects - and the test suite is the largest body
of worked API examples in the repository, one folder per module.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Native build pipeline | Builds the Linux native binding library for x64, ARM64 and RISC-V 64 inside a container so the system-library baseline is the oldest practical one, statically linking everything but the universal system libraries. A build must pass a required-features check, a check that nothing forbidden is dynamically linked, and a C smoke test that loads the library the way .NET's `NativeLibrary` does | [`tools/build_native_libraries`](https://github.com/ellisnet/CodeBrix.VideoProcessing.OpenCV5/tree/main/tools/build_native_libraries) |
| Library test suite | One folder per module - core, imgcodecs, imgproc, videoio, features, calib3d, objdetect, ml, video and tracking, dnn, photo and the contrib modules - with test images and model fixtures, plus the native-load diagnostics tests | [`tests/CodeBrix.VideoProcessing.OpenCV5.Tests`](https://github.com/ellisnet/CodeBrix.VideoProcessing.OpenCV5/tree/main/tests/CodeBrix.VideoProcessing.OpenCV5.Tests) |
| Analyzer tests | Unit tests for `OCVS001` to `OCVS004`, using the Roslyn testing harness; no native library needed | [`tests/CodeBrix.VideoProcessing.OpenCV5.Analyzers.Tests`](https://github.com/ellisnet/CodeBrix.VideoProcessing.OpenCV5/tree/main/tests/CodeBrix.VideoProcessing.OpenCV5.Analyzers.Tests) |
| WPF converter tests | Windows-only, and an empty assembly on other hosts, like the `.Wpf` library itself | [`tests/CodeBrix.VideoProcessing.OpenCV5.Wpf.Tests`](https://github.com/ellisnet/CodeBrix.VideoProcessing.OpenCV5/tree/main/tests/CodeBrix.VideoProcessing.OpenCV5.Wpf.Tests) |

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.VideoProcessing.OpenCV5/blob/main/README.md) |
| Complete API guide for every package in the family (ships inside each of them too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.VideoProcessing.OpenCV5/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.VideoProcessing.OpenCV5/blob/main/README-INDEX.txt) |
| Non-package content in the repository | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.VideoProcessing.OpenCV5/blob/main/EXTRAS-README.txt) |
| Tests (worked examples of every module) | [tests](https://github.com/ellisnet/CodeBrix.VideoProcessing.OpenCV5/tree/main/tests) |

One `AGENT-README.txt` covers the managed core, the WPF converters and every native runtime package, and
the same file ships inside all of them. XML documentation ships alongside the assembly, and the analyzers
are inside the core package, so there is nothing extra to add for either.

## License

CodeBrix.VideoProcessing.OpenCV5 is licensed under the Apache License 2.0, and every package in the
family carries the same terms - the license is named in each package ID, from
`CodeBrix.VideoProcessing.OpenCV5.ApacheLicenseForever` through the `.Wpf` package to each native runtime
package. For the provenance and licensing of open source code included in this library, including the
native libraries it carries, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.VideoProcessing.OpenCV5/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.VideoProcessing](CodeBrix.VideoProcessing.md) - the other half of the media story: FFmpeg command lines built and run from C#
- [CodeBrix.VideoPlayback](CodeBrix.VideoPlayback.md) - playing video in an application, rather than analyzing it
- [Graphics, media and vision](../platform/09-graphics-media-and-vision.md) - where vision work sits in a CodeBrix.Platform application
- [ellisnet/CodeBrix.VideoProcessing.OpenCV5 on GitHub](https://github.com/ellisnet/CodeBrix.VideoProcessing.OpenCV5) - source and tests
