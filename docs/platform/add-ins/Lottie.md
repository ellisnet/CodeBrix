<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Add-ins](../08-add-ins.md) › Lottie</sub>

# Lottie

**Plays Lottie (Bodymovin JSON) vector animations inside a CodeBrix.Platform XAML application, and makes the core framework's `ProgressRing` spin.** The core already ships the `AnimatedVisualPlayer` control and the `IAnimatedVisualSource` contract it plays; this add-in supplies the source side of that contract. Frames are drawn on a Skia canvas element that the source adds as the player's child, so the animation is real vector output at any size, and there is nothing to call at startup - referencing the package activates it.

| | |
| --- | --- |
| **Package** | [`CodeBrix.Platform.Lottie.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Lottie.ApacheLicenseForever) |
| **Adds** | `LottieVisualSource`, `ThemableLottieVisualSource` (run-time recoloring), `LottieVisualSourceProvider` (the registration that makes `ProgressRing` work) |
| **Heads** | All six. One Skia runtime assembly serves every head. |
| **Requires** | .NET 10 or later, and a running CodeBrix.Platform application. No system package to install. |

Dependencies arrive automatically: the core framework package, [`CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.SkiaSharp.Views.MitLicenseForever), [`CodeBrix.Platform.Graphics2DSK.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Graphics2DSK.ApacheLicenseForever) for the canvas element the animation is drawn on, and the Skottie decoder.

## Add it to your application

Reference the package once, in the application's `.Core` project. Every head inherits it through the `.Core` project reference; never add it to a head project.

```bash
dotnet add package CodeBrix.Platform.Lottie.ApacheLicenseForever
```

A complete `.Core` project file, with the animation shipped as an embedded resource:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>MyApp</RootNamespace>
    <AssemblyName>MyApp.Core</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.Platform.ApacheLicenseForever" />
    <PackageReference Include="CodeBrix.Platform.Lottie.ApacheLicenseForever" />
  </ItemGroup>
  <ItemGroup>
    <EmbeddedResource Include="Assets\anim.json" />
  </ItemGroup>
</Project>
```

The XAML namespace for the source types - `AnimatedVisualPlayer` itself is in the default namespace and needs no prefix:

```xml
xmlns:lottie="using:CommunityToolkit.WinUI.Lottie"
```

The equivalent assembly-qualified form, if you need it:

```xml
xmlns:lottie="clr-namespace:CommunityToolkit.WinUI.Lottie;assembly=CodeBrix.Platform.UI.Lottie"
```

And the usings for code-behind:

```csharp
using CommunityToolkit.WinUI.Lottie;   // LottieVisualSource,
                                       // ThemableLottieVisualSource,
                                       // LottieVisualSourceBase,
                                       // LottieVisualOptions
using Microsoft.UI.Xaml.Controls;      // AnimatedVisualPlayer (core),
                                       // IAnimatedVisualSource,
                                       // IThemableAnimatedVisualSource,
                                       // ILottieVisualSourceProvider
using CodeBrix.Platform.UI.Lottie;     // LottieVisualSourceProvider
                                       // (only if you call it yourself)
```

There is no registration call. The assembly carries an `[assembly: ApiExtension(...)]` registration for `ILottieVisualSourceProvider`; the XAML source generator scans every referenced assembly for that attribute while compiling the application and emits the `ApiExtensibility.Register(...)` call into the generated `App` code, so the reference alone wires the core's `ProgressRing` to this package. The `LottieVisualSource` types themselves are ordinary classes you use directly.

> [!NOTE]
> The package's MSBuild targets run in each head project and fail the build if the `SkiaSharp.Skottie` assembly is not among the head's references. The package's own dependency normally satisfies this. If the error appears - after a package-reference cleanup, for instance - add `SkiaSharp.Skottie` to `.Core` explicitly, or set the MSBuild property `CodeBrixDisableLottieSkiaVersionCheck=true` to skip the check.

## Using it

### An autoplaying looped animation

Put the source inside the player: `Source` is the player's content property, so it can be written as the element's child.

```xml
<ItemGroup>
  <EmbeddedResource Include="Assets\star_icon.json" />
</ItemGroup>
```

```xml
<Page x:Class="MyApp.Views.MainPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:lottie="using:CommunityToolkit.WinUI.Lottie">
    <AnimatedVisualPlayer x:Name="Player"
                          AutoPlay="True"
                          Stretch="Uniform"
                          Width="120" Height="120">
        <lottie:LottieVisualSource
            UriSource="embedded://MyApp.Core/MyApp.Assets.star_icon.json" />
    </AnimatedVisualPlayer>
</Page>
```

Notice the two halves of the `embedded://` URI: the host is the *assembly* name, and the path is the *manifest resource* name, which follows the MSBuild rule `<RootNamespace>.<folder>.<file>`. Getting one of the two wrong is the most common reason a player stays blank.

### The player's surface

`AnimatedVisualPlayer` is a core control, not a type from this package.

```csharp
public IAnimatedVisualSource Source { get; set; }    // the Lottie source
public bool     AutoPlay        { get; set; }        // default true
public Stretch  Stretch         { get; set; }        // default Uniform
public double   PlaybackRate    { get; set; }        // default 1.0
public DataTemplate FallbackContent { get; set; }    // stored, never shown
public bool     IsPlaying              { get; }      // read-only, bindable
public bool     IsAnimatedVisualLoaded { get; }      // read-only, bindable
public TimeSpan Duration               { get; }      // read-only, set on load

public IAsyncAction PlayAsync(double fromProgress, double toProgress, bool looped);
public void Pause();
public void Resume();
public void Stop();
public void SetProgress(double progress);
```

Every property above has a dependency property behind it, so all of them bind. Every method forwards to `Source`, and with no `Source` they are no-ops. Loading is triggered when the player enters the visual tree; unloading pauses playback and reloading resumes it.

> [!IMPORTANT]
> `PlayAsync` starts playback and returns an already-completed action. It does not wait for the animation to reach `toProgress`. Use `IsPlaying` - a dependency property - to observe the end of a non-looped segment.

### Playing one segment and reacting to the end

```csharp
using CommunityToolkit.WinUI.Lottie;
using Microsoft.UI.Xaml.Controls;

var player = new AnimatedVisualPlayer
{
    AutoPlay = false,
    Source = new LottieVisualSource
    {
        UriSource = new Uri("ms-appx:///Assets/checkmark.json")
    }
};
root.Children.Add(player);

// IsPlaying is a dependency property: watch it to learn when the
// non-looped segment has finished (PlayAsync completes immediately).
player.RegisterPropertyChangedCallback(AnimatedVisualPlayer.IsPlayingProperty,
    (s, dp) =>
    {
        if (!player.IsPlaying && player.IsAnimatedVisualLoaded)
            StatusText.Text = "done";
    });

// Safe to call before the JSON has arrived: the segment is applied on load.
_ = player.PlayAsync(0.0, 0.5, looped: false);
```

A `Play()` issued before the JSON has been decoded is remembered and applied as soon as it arrives, which is why the last line is safe. When a non-looped segment reaches its end the source stops itself and the frame at `to` stays visible; a looped segment restarts its stopwatch for a seamless loop.

### Scrubbing

`SetProgress(p)` clamps `p` to the range 0 to 1, stops playback and repaints that frame - the scrubber primitive.

```xml
<AnimatedVisualPlayer x:Name="Player" AutoPlay="False" Height="200">
    <lottie:LottieVisualSource UriSource="ms-appx:///Assets/intro.json" />
</AnimatedVisualPlayer>
<Slider Minimum="0" Maximum="1" StepFrequency="0.01"
        ValueChanged="Scrub_ValueChanged" />
```

```csharp
private void Scrub_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    => Player.SetProgress(e.NewValue);   // stops playback, shows that frame
```

### Speed, pause and resume

`PlaybackRate` is read every frame, so a change takes effect on the next one, mid-playback.

```csharp
Player.PlaybackRate = 2.0;   // applied on the next frame, mid-playback
Player.Pause();
Player.Resume();
Player.Stop();
```

`Pause()` halts the timer and the stopwatch and clears `IsPlaying`; `Resume()` continues from the same frame; `Stop()` clears the play state.

### Recoloring at run time

`ThemableLottieVisualSource` rewrites shape colors named in the Lottie file itself. The bindings live in each shape's `nm` (name) property, written as `{ Color : var(Foreground) }`, or with several names separated by semicolons. Only the property name `Color` is honored.

```xml
<AnimatedVisualPlayer x:Name="Player" AutoPlay="True">
    <lottie:ThemableLottieVisualSource x:Name="Themed"
        UriSource="embedded://./(assembly).Assets.spinner.json" />
</AnimatedVisualPlayer>
```

```csharp
// code-behind; works before or after the file has loaded
Themed.SetColorThemeProperty("Foreground", Windows.UI.Color.FromArgb(255, 0, 120, 215));
var current = Themed.GetColorThemeProperty("Foreground");   // Color?
```

`SetColorThemeProperty` rewrites the color array of every bound shape, rebuilds the JSON and re-decodes the animation, re-applying the current segment and looping state. Because every change re-parses and re-decodes, treat color changes as theme-level events, not as per-frame animation. The walk covers the document's top-level layers, their shapes and nested groups recursively; shapes inside precomposition assets are not visited, so put bound shapes on ordinary layers.

Note the two shorthands in the URI above: `.` as the host means the assembly that contains your `App` class, and the literal token `(assembly)` inside the resource name is replaced by that assembly's name.

### Where a source can load from

| Form | Behavior |
| --- | --- |
| `embedded://<AssemblyName>/<Manifest.Resource.Name>` | The assembly is loaded by simple name; `.` means the assembly holding your `App` class, and `(assembly)` in the path is replaced by that name. A missing resource logs a warning and the player stays empty. |
| `ms-appx:///Assets/<file>.json` | A file in the application's install folder, shipped as content copied to the output. A library's own asset is `ms-appx:///<AssemblyName>/Assets/<file>`. |
| `ms-appdata://local/...` | A file under the application's data folders, opened from disk. |
| `http://`, `https://` | Downloaded - but only by `ThemableLottieVisualSource`. A plain `LottieVisualSource` treats a web URI as unloadable and logs the failure. |

Neither source supports `file://` URIs or relative URIs. Prefer `embedded://` or `ms-appx:///` for bundled assets: both read from local storage, while web URIs go through a shared client with no caching, on every load.

### Making ProgressRing spin

The core's `ProgressRing` resolves `ILottieVisualSourceProvider` in its constructor and plays two Lottie files through an `AnimatedVisualPlayer` in its template. Without this package the ring renders nothing and an error is logged saying the control needs an additional package; referencing `CodeBrix.Platform.Lottie.ApacheLicenseForever` in `.Core` is the fix.

The ring's colors come from its own `Foreground` and `Background` brushes - solid-color brushes only - pushed into the themable source as the bindings `Foreground` and `Background`. The animations are swappable through `ProgressRing.IndeterminateSource` and `ProgressRing.DeterminateSource`, and a plain `LottieVisualSource` you assign is upgraded to a themable one automatically so those bindings still apply. To re-skin every ring in the application, point `FeatureConfiguration.ProgressRing.ProgressRingAsset` and `.DeterminateProgressRingAsset` (namespace `CodeBrix.Platform.UI`) at your own JSON at startup.

To reach the provider without naming a concrete type:

```csharp
if (ApiExtensibility.CreateInstance<ILottieVisualSourceProvider>(this, out var provider))
    player.Source = provider.CreateThemableFromLottieAsset(uri);
// ApiExtensibility: namespace CodeBrix.Platform.Foundation.Extensibility
```

### What it does not do

It renders no Lottie file through the Windows composition pipeline: there is no `IAnimatedVisual`, no `ProgressObject` and no diagnostics, and everything is drawn on a Skia canvas. It does not support dotLottie (`.lottie`) archives, external image assets or any non-JSON input. It offers no per-frame events, frame counts, markers or completion event - the observable state is `IsPlaying`, `IsAnimatedVisualLoaded` and `Duration`. It does not cache decoded animations across source instances. And it brings no authoring or conversion tool; export plain Bodymovin JSON with embedded assets.

Several members exist but do nothing: `Options` and `LottieVisualOptions` are stored and ignored, and `CreateFromString` and `TryCreateAnimatedVisual` throw `NotImplementedException`. On the player, `ProgressObject` throws when read, and `Diagnostics` and `AnimationOptimization` are not implemented. Do not write code that relies on any of them.

## Per-head notes

There are none. One Skia runtime assembly serves every head, and no head-specific behavior is documented.

The vendored `System.Json` types (`JsonValue`, `JsonObject`, `JsonArray`, `JsonPrimitive`, `JsonType` in namespace `System.Json`) are public but are not for application code - they are the themable source's internal document model. Use `System.Text.Json` in your application.

## Pitfalls

- **Load failures are silent.** A wrong resource name, an unreadable file or JSON the decoder rejects logs an error - "Failed to update lottie player for [uri]" or "Unable to find embedded resource named ..." - and leaves the player blank. Nothing is thrown to application code, so check the log first.
- **`FallbackContent` is stored but never displayed** on these heads.
- **`PlayAsync` completes immediately.** Awaiting it does not wait for the segment; watch `IsPlaying` instead. `SetSourceAsync(Uri)` also completes immediately - it only sets `UriSource`.
- **The manifest resource name is `<RootNamespace>.<link path with dots>`**, not the assembly name plus the file name. Check the `RootNamespace` of the project that embeds the file; the assembly name goes in the host part. The XAML editor may flag the URI, but it is correct.
- **A plain `LottieVisualSource` will not fetch `http(s)` URIs.** Use `ThemableLottieVisualSource` for a downloaded animation.
- **`Play()` off the UI thread fails**, because it creates a dispatcher timer for the calling thread. Marshal to the dispatcher first. `Stop()` marshals itself; `Pause`, `Resume` and `SetProgress` do not.
- **JSON only.** The loader reads the payload as UTF-8 text, so a zipped `.lottie` bundle, or JSON that references external image files, cannot work.
- **Color bindings inside precompositions are not found**, and only the `Color` property name is honored.
- **`AutoPlay` defaults to true.** A player with a source starts looping as soon as it loads unless you set `AutoPlay="False"`.
- **Never add the package to a head project.** `.Core` only.
- **A hidden-but-loaded player keeps ticking.** The frame timer runs at the lower of 120 Hz and the file's own frame rate, so a 30 fps file costs 30 repaints a second per player. Unloading and reloading pause and resume automatically when the player leaves and re-enters the tree.
- **There is no cross-instance cache.** Each source instance reads and decodes its JSON on every load, and every `UriSource` change re-decodes. Keep a source instance alive and re-attach it rather than re-creating it per show.
- **Do not animate colors** through `SetColorThemeProperty`: each call re-parses, rewrites and re-decodes the whole document. Batch the changes.

## Related

- [Svg](Svg.md) - the companion add-in for static SVG images
- [Graphics2DSK](Graphics2DSK.md) - the canvas element this add-in draws the animation on, and one of its dependencies
- [SkiaSharp views](SkiaSharpViews.md) - the other view package this add-in depends on
- [JustBetweenUs](https://github.com/ellisnet/CodeBrix.Samples/tree/main/JustBetweenUs) in [CodeBrix.Samples](../../samples/README.md) - plays the same animation on a Skia head and in a native WinUI head; the in-repository copy at [samples/CodeBrixPlatform/JustBetweenUs](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/JustBetweenUs) shows the player inside a `Button` and the `.Core` project file that embeds the JSON

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.Lottie/AGENT-README.txt) |
| The core framework and the six heads | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/AGENT-README.txt) |
| What a "not implemented" exception means | [NOT-IMPLEMENTED.md](https://github.com/ellisnet/CodeBrix.Platform/blob/main/NOT-IMPLEMENTED.md) |
| Map of every README in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/README-INDEX.txt) |
| Package | [`CodeBrix.Platform.Lottie.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Lottie.ApacheLicenseForever) |

---

**Where to go next**

- [Svg](Svg.md) - the next add-in: static vector images through `SvgImageSource`
- [Views and styling](../06-views-and-styling.md) - where `ProgressRing` and the rest of the control set are covered
- [All add-ins](../08-add-ins.md) - the whole set at a glance
