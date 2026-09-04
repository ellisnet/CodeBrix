<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.SvgParse</sub>

# CodeBrix.SvgParse

**CodeBrix.SvgParse is a renderer-agnostic SVG document object model.** It parses SVG documents into
a strongly typed element tree, models every SVG element family - shapes, paths, paint servers, text,
fonts and glyphs, transforms, clipping and masking, filter effects, markers and animation elements -
applies CSS styling, and serializes the tree back to SVG text. It does not depend on any rendering
engine, on `System.Drawing`, or on any external geometry or color package: every geometric and color
value type it exposes is library-native, which is what lets it serve as the front half of any SVG
rendering backend, in any .NET 10 application or in a CodeBrix.Platform application.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.SvgParse](https://github.com/ellisnet/CodeBrix.SvgParse) |
| **Packages** | [`CodeBrix.SvgParse.MsplLicenseForever`](https://www.nuget.org/packages/CodeBrix.SvgParse.MsplLicenseForever) |
| **License** | Microsoft Public License (Ms-PL); see [License](#license) |
| **Requires** | .NET 10 or later; one NuGet dependency, [`CodeBrix.StyleSheetParse.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.StyleSheetParse.MitLicenseForever), restored automatically. No native libraries |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Anywhere .NET 10 runs; there is no OS-specific code |

## What it does

- Loads SVG documents from files, streams, strings and `XmlReader`s, through two loaders: the
  classic `SvgDocument` static loaders and a browser-compatibility loader.
- Models the complete SVG 1.1 element hierarchy - shapes, text, gradients, patterns, filters,
  markers, fonts and glyphs, and the animation elements.
- Applies CSS styling with specificity-based cascading, over the document's own `<style>` content or
  extra CSS you supply per load.
- Parses and manipulates path data: a `d` string becomes a typed segment list you can read, edit and
  re-emit.
- Models transforms - translate, rotate, scale, skew, matrix - as a typed, ordered collection that
  serializes itself back to the attribute value.
- Models clipping and masking, and filter effects: blur, blend, color matrix, composite, convolve,
  displacement, morphology, turbulence, lighting and the component-transfer functions.
- Models linear and radial gradients with stop colors, and pattern fills.
- Models text elements with font properties and text paths, and markers on shapes.
- Deep-copies elements and whole documents.
- Controls external resource loading, so an untrusted document cannot reach the network or the disk.
- Serializes back to SVG text, whole documents or one subtree at a time through `GetXML()`.

## When to use it

Use CodeBrix.SvgParse when you need to read, query, restyle, build or rewrite SVG as data: extracting
one icon from a sprite sheet, recoloring a diagram before it is rendered, auditing what a file
contains, generating SVG from scratch, or writing your own rendering backend on a stable typed tree.
Parsing is the expensive step, so load a document once and query it many times.

Nothing is drawn. The library does not render or rasterize; it does not export to PNG, JPEG, PDF,
EPS or any other format; it does not run SVG animations, though it models them completely; it does
not handle interaction - the mouse events on `SvgElement` are plumbing for a host that raises them,
and there is no hit testing here; it does not measure text or resolve real fonts, so the `<font>`
element family is modeled as data with no typeface loaded and no glyph shaped; it computes no
geometry at all, so there are no bounding boxes, no path flattening, no intersection and no
composition of transforms into a matrix; it does not optimize or minify; it offers no fluent drawing
API, though you can construct elements programmatically; it cannot be extended with new element
types from a consumer assembly; and it never executes `<script>` content.

Rendering is the job of a backend built on top of this model. [CodeBrix.SkiaSvg](CodeBrix.SkiaSvg.md)
is one such backend - it depends on this package and re-exposes its types - and the managed SVG
renderer in [CodeBrix.Imaging.Drawing](CodeBrix.Imaging.Drawing.md) is another.

## Getting started

```bash
dotnet add package CodeBrix.SvgParse.MsplLicenseForever
```

Nearly the whole public surface lives in the base namespace, so a single `using CodeBrix.SvgParse;`
covers most consumer code:

```csharp
using CodeBrix.SvgParse;               // SvgDocument, SvgElement, all
                                       //   shape/structure/text/font/
                                       //   animation elements, all paint
                                       //   servers, SvgColor, SvgUnit,
                                       //   SvgPoint, SvgPointF,
                                       //   SvgRectangleF, SvgSizeF,
                                       //   SvgViewBox, SvgAspectRatio,
                                       //   SvgOrient, the presentation
                                       //   enums, the type converters,
                                       //   the event-args classes and
                                       //   SvgException / SvgIDException.
using CodeBrix.SvgParse.Pathing;       // SvgPathSegment and the six
                                       //   segment types, SvgPathSegmentList,
                                       //   ISvgPathElement, SvgArcSize,
                                       //   SvgArcSweep.
using CodeBrix.SvgParse.Transforms;    // SvgTransform, SvgTranslate,
                                       //   SvgRotate, SvgScale, SvgSkew,
                                       //   SvgShear, SvgMatrix,
                                       //   SvgTransformCollection,
                                       //   SvgTransformConverter.
using CodeBrix.SvgParse.FilterEffects; // SvgFilter, SvgFilterPrimitive and
                                       //   every fe* primitive, the light
                                       //   sources, ISvgFilterable and the
                                       //   filter enums.
using CodeBrix.SvgParse.Primitives;    // SvgOrientConverter,
                                       //   SvgPreserveAspectRatioConverter,
                                       //   SvgColorInterpolation,
                                       //   SvgMarkerUnits.
using CodeBrix.SvgParse.Exceptions;    // SvgMemoryException (only).
using CodeBrix.SvgParse.ExtensionMethods; // UriExtensions.
```

Folder names in the repository are not namespaces. There is no `CodeBrix.SvgParse.Painting`
namespace: `SvgPaintServer`, `SvgColorServer`, `SvgGradientServer`, `SvgLinearGradientServer`,
`SvgRadialGradientServer`, `SvgPatternServer`, `SvgGradientStop`, `SvgMarker`,
`SvgDeferredPaintServer` and `SvgFallbackPaintServer` are all in the base namespace even though
their source files sit in a "Painting" folder, and the same is true of "Text", "Animation",
"Document Structure", "Basic Shapes", "Clipping and Masking", "Metadata", "Linking", "Scripting" and
"Interaction". `ISvgTransformable` is in `CodeBrix.SvgParse`, not in `CodeBrix.SvgParse.Transforms`.
`CodeBrix.SvgParse.Exceptions` contains exactly one public type, `SvgMemoryException`; the other
exception types are in the base namespace. `CodeBrix.SvgParse.Css` exists but contains only internal
types - do not add a using for it.

There is nothing to register at start-up. The one setup step worth making a habit is locking down
external resolution before you load anything you do not control:

```csharp
// Keep external entity and image resolution locked down for untrusted input.
SvgDocument.ResolveExternalXmlEntities = ExternalType.None;
SvgDocument.ResolveExternalImages = ExternalType.None;
SvgDocument.ResolveExternalElements = ExternalType.None;
```

This program loads a file with the compatibility loader and counts what is in it:

```csharp
using System;
using System.Linq;
using CodeBrix.SvgParse;

// Keep external entity and image resolution locked down for untrusted input.
SvgDocument.ResolveExternalXmlEntities = ExternalType.None;
SvgDocument.ResolveExternalImages = ExternalType.None;
SvgDocument.ResolveExternalElements = ExternalType.None;

if (args.Length == 0)
{
    Console.Error.WriteLine("usage: MySvgTool <file.svg>");
    return 1;
}

var doc = SvgDocumentCompatibilityLoader.Open<SvgDocument>(args[0], new SvgOptions());

Console.WriteLine($"size   : {doc.Width} x {doc.Height}");
Console.WriteLine($"viewBox: {doc.ViewBox}");

var counts = doc.Descendants()
                .GroupBy(e => e.GetType().Name)
                .OrderByDescending(g => g.Count());

foreach (var group in counts)
{
    Console.WriteLine($"{group.Count(),5}  {group.Key}");
}

return 0;
```

Notice that elements are grouped by CLR type: that is how you identify an element in this library,
because the SVG element name is not public API.

## Key concepts

### Two loaders

Both loaders return an `SvgDocument`, or a subclass of your own:

```csharp
static SvgDocument Open(string path)
static T Open<T>(string path)                        where T : SvgDocument, new()
static T Open<T>(string path, SvgOptions svgOptions) where T : SvgDocument, new()
static T Open<T>(Stream stream)                      where T : SvgDocument, new()
static T Open<T>(Stream stream, SvgOptions svgOptions) where T : SvgDocument, new()
static T Open<T>(XmlReader reader)                   where T : SvgDocument, new()
static SvgDocument Open(XmlDocument document)
static T FromSvg<T>(string svg)                      where T : SvgDocument, new()
```

`SvgDocumentCompatibilityLoader` offers `Open<T>` over a path, a stream or an `XmlReader`, and
`FromSvg<T>`. It builds the same element tree but captures an absolute document base URI before
reading, so relative stylesheet references resolve the way a browser would, and it preserves the raw
`<style>` text so a stricter CSS pass can run after the tree is built. Prefer it whenever CSS
correctness matters - external stylesheets, `@import`, selector specificity.

`SvgOptions` configures a load and is itself an `IDictionary<string, string>`: construct it empty,
with a dictionary of custom XML entities, with a string of extra CSS, or with both.

### SvgDocument and the security controls

`SvgDocument` is the root of the model and inherits every viewport property from `SvgFragment`. Its
instance members are `Ppi`, `BaseUri` (which must be absolute or the setter throws
`ArgumentException`), `ExternalCSSHref`, `EnableEmitNamedColorsOnSerialization`, `GetElementById`
and its generic twin, `OverwriteIdManager`, `RasterizeDimensions`, and the three `Write` overloads.

Five statics are library-wide and affect every document: `ResolveExternalXmlEntities` (default
`None`), `ResolveExternalImages` and `ResolveExternalElements` (both default `Local|Remote`),
`DisableDtdProcessing` (default false) and `PointsPerInch`. `ExternalType` is a `[Flags]` enum of
`None`, `Local` and `Remote`, and `ExternalTypeExtensions.AllowsResolving` tests a URI against a
value.

A sixth static, `EmitNamedColorsOnSerialization`, decides whether a color that matches a W3C or CSS3
name is written out by name (`fill="red"`) instead of hex (`fill="#ff0000"`). It is snapshotted into
each new document's `EnableEmitNamedColorsOnSerialization` at construction time, so changing the
static afterwards does not retrofit existing documents.

> [!IMPORTANT]
> The property is `ResolveExternalXmlEntities`, with the "i" in "Entities". Leaving it at
> `ExternalType.None` both prevents XML external entity attacks and removes a class of network and
> disk access from the parse path.

### SvgElement: tree, attributes and traversal

`SvgElement` is the abstract base for every element. Identity and structure come from `ID`,
`Parent`, `Children`, `Nodes` (mixed element and text nodes), `OwnerDocument`, `Content`,
`Namespaces`, `CustomAttributes`, `Transforms` and `SpaceHandling`. Traversal is `Descendants()`
(depth-first, excluding self), `Parents` (the parent chain, nearest first), `ParentsAndSelf` and
`HasChildren()`.

At the string level you have `ContainsAttribute(string name)` and
`TryGetAttribute(string name, out string value)`. The strongly typed attribute collection behind
them is `protected internal`, and so is the element's SVG element name: from a consumer assembly you
have those two helpers, `CustomAttributes` for anything the library does not model, the typed CLR
properties, and the CLR type itself. There is no public `element.Attributes` and no public
`element.ElementName` - although `NonSvgElement.Name` does read back a foreign-namespace tag name.

Cloning is `DeepCopy()`, `DeepCopy<T>()` and `Clone()`. Styling is `AddStyle(name, value,
specificity)`, `FlushStyles(bool children = false)` and `InvalidateChildPaths()`. A per-element
animation hook - `GetAnimationValue`, `TrySetAnimationValue`, `ClearAnimationValue` - lets a host
drive a timeline itself. Events cover `Load`, `ChildAdded`, `AttributeChanged`, `ContentChanged` and
the mouse family, with `AutoPublishEvents`, `RegisterEvents` and `UnregisterEvents`.

### Presentation properties and inheritance

All SVG presentation attributes are declared on `SvgElement` itself, so every element - container
elements included - exposes them, and reading one walks up the parent chain when the element does
not set it. Paint and stroke are `Fill`, `Stroke`, `Color`, `FillRule`, `Opacity`, `FillOpacity`,
`StrokeOpacity`, `StrokeWidth`, `StrokeLineCap`, `StrokeLineJoin`, `StrokeMiterLimit`,
`StrokeDashArray` and `StrokeDashOffset`. Rendering hints and visibility are `ShapeRendering`,
`ColorInterpolation`, `ColorInterpolationFilters`, and `Visibility` and `Display` as raw strings.
Text and font properties - `FontFamily`, `FontSize`, `FontStyle`, `FontVariant`, `FontWeight`,
`FontStretch`, the `Font` shorthand, `TextAnchor`, `DominantBaseline`, `BaselineShift` (a raw
string, not an `SvgUnit`), `TextDecoration` and `TextTransformation` - are usable on any element and
inherit down into text. Most of the presentation enums include an explicit `Inherit` member:
"inherit" is a real, distinct value here, not a null.

`SvgVisualElement` adds what graphics-producing elements need on top: `Clip`, `ClipPath` (a
`url(#id)` reference), `ClipRule`, `Filter`, `Visible` and `EnableBackground`. This is the
inheritance chain that matters when you write type tests:

```text
SvgElement
  -> SvgVisualElement                (ISvgStylable)
       -> SvgPathBasedElement
            -> SvgMarkerElement      (marker-start/mid/end)
                 -> SvgGroup, SvgLine, SvgPath, SvgPolygon
                      -> SvgPolyline (derives from SvgPolygon)
            -> SvgCircle, SvgEllipse, SvgRectangle, SvgMarker, SvgGlyph
       -> SvgUse, SvgImage, SvgSymbol, SvgSwitch, SvgForeignObject,
          SvgTextBase (-> SvgText, SvgTextSpan, SvgTextPath, SvgTextRef)
  -> SvgFragment/SvgDocument, SvgDefinitionList, SvgClipPath, SvgMask,
     SvgGradientStop, SvgPaintServer (and its gradient/pattern subclasses),
     SvgFilter and the filter primitives, the font/glyph elements, the
     animation elements, SvgTitle, SvgDescription, SvgAnchor, SvgScript,
     SvgDocumentMetadata, SvgUnknownElement, NonSvgElement
```

### Shapes, paths and path data

`SvgRectangle` carries `X`, `Y`, `Width`, `Height`, `CornerRadiusX`, `CornerRadiusY` and
`Location`; `SvgCircle` has `CenterX`, `CenterY`, `Radius` and `Center`; `SvgEllipse` has two radii;
`SvgLine` has `StartX`, `StartY`, `EndX` and `EndY`, with `Fill` overridden to default to
`SvgPaintServer.None`; `SvgPolygon` carries `Points`, and `SvgPolyline` derives from it.

`SvgPath` carries its `PathData` as an `SvgPathSegmentList`, plus `PathLength` and
`OnPathUpdated()`. The segment types are `SvgMoveToSegment`, `SvgLineSegment`,
`SvgCubicCurveSegment`, `SvgQuadraticCurveSegment`, `SvgArcSegment` and `SvgClosePathSegment`, each
with `IsRelative`, `Start`, `End` and `Clone()`. `SvgPathBuilder.Parse(ReadOnlySpan<char>)` turns a
`d` string into a segment list - it is a parser, not a fluent builder, so to construct a path
programmatically you new up segments and add them to a list. A segment's `ToString()` emits its SVG
command text, and the list's `ToString()` emits a complete `d` attribute value.

### Painting: paint servers and colors

`SvgPaintServer` is the abstract base for everything that can fill or stroke, and it carries three
sentinels: `None` (paint explicitly turned off), `Inherit` and `NotSet` (the attribute was absent).
They are reference-compared - test with `ReferenceEquals(element.Fill, SvgPaintServer.None)` or `==`,
not by inspecting a color. `SvgColorServer` wraps an `SvgColor`; `SvgGradientServer` carries `Stops`,
`SpreadMethod`, `GradientUnits`, `GradientTransform`, `InheritGradient`, `StopColor` and
`StopOpacity`, with `SvgLinearGradientServer` adding the two endpoints and
`SvgRadialGradientServer` adding center, radius and focal point; `SvgPatternServer` carries the
pattern geometry, units, transform and viewport.

The color parser accepts, for `fill`, `stroke`, `color`, `stop-color`, `flood-color` and
`lighting-color` alike: named colors, case-insensitive, with British "grey" aliases included;
`#rgb`, `#rgba`, `#rrggbb` and `#rrggbbaa`; `rgb(r, g, b)` and `rgba(r, g, b, a)`; the space form
`rgb(r g b / a)`; and `hsl(h, s%, l%)`. Each channel may be a 0-255 number, fractions allowed, or a
percentage, and the two may be mixed; the alpha may be a percentage, a 0-1 number, or - tolerated
for legacy content - a 0-255 number when it is greater than 1. Out-of-range values clamp; they never
throw.

`SvgColor` is a readonly struct of 8-bit ARGB with `FromRgb`, `FromRgba` and `FromArgb` factories,
`ParseHex`, `TryParseHex`, `Parse`, `TryParse` and `TryFromName` for input, and `ToHex()`,
`ToString()` and `GetKnownName()` for output, plus named-color constants spanning the W3C and CSS3
set. Name lookup is case-insensitive, the British spellings resolve to the same values as the
American ones, `aqua` equals `cyan` and `fuchsia` equals `magenta`.

### Deferred and fallback paint

When a fill or stroke is written as `url(#id)` - possibly with a fallback color - the parsed value is
not the target paint server itself. It is an `SvgDeferredPaintServer`, which carries `Document`,
`DeferredId`, `FallbackServer`, an `EnsureServer(SvgElement styleOwner)` method that resolves the
reference now, and the static `TryGet<T>`, which is the safe way to read a fill: it resolves a
deferred server and returns null when the reference does not point at a `T`.
`SvgFallbackPaintServer` holds a primary server plus its fallbacks. A document loaded with an
unresolved reference re-serializes with that reference intact.

### Text elements

On `SvgTextBase`, `X`, `Y`, `Dx` and `Dy` are `SvgUnitCollection` per-glyph lists, not single units,
and `Rotate` is the raw rotate list as a string. For the common single-value case, read `text.X[0]`
and write `text.X.Add(new SvgUnit(10f))`. `TextLength`, `LengthAdjust`, `LetterSpacing`,
`WordSpacing` and `SpaceHandling` complete the layout surface, and `Fill` is overridden to default
to black rather than inheriting. The concrete types are `SvgText`, `SvgTextSpan`, `SvgTextRef` and
`SvgTextPath`, the last carrying `ReferencedPath`, `StartOffset`, `Method` and `Spacing`.

The `<font>` element family - `SvgFont`, `SvgFontFace`, `SvgFontFaceSrc`, `SvgFontFaceUri`,
`SvgGlyph`, `SvgMissingGlyph`, `SvgKern`, `SvgHorizontalKern` and `SvgVerticalKern` - is parsed and
re-serialized as data. No typeface is built from it.

### Transforms

`SvgTransform` is abstract, with `WriteToString()` and `Clone()`. The concrete transforms are
`SvgTranslate`, `SvgRotate` (with an optional center), `SvgScale`, `SvgSkew`, `SvgShear` and
`SvgMatrix`. There is no `SvgSkewX` and no `SvgSkewY` type: `skewX(a)` is `new SvgSkew(a, 0f)` and
`skewY(a)` is `new SvgSkew(0f, a)`, and `SvgSkew` writes itself back as `skewX(...)` when its Y
angle is 0 and as `skewY(...)` otherwise. `SvgShear` has no SVG attribute equivalent and serializes
as `shear(x, y)`.

`SvgTransformCollection` is a `List<SvgTransform>` whose mutating members raise `TransformChanged`
and whose `ToString()` emits the whole transform attribute value.
`SvgTransformConverter.Parse(ReadOnlySpan<char>)` parses one, as in
`SvgTransformConverter.Parse("translate(10,20) rotate(45) scale(2)")`.

### Clipping, masking and markers

`SvgClipPath` carries `ClipPathUnits` and `SvgMask` carries its geometry plus `MaskUnits` and
`MaskContentUnits`. Elements reference a clip path through `SvgVisualElement.ClipPath` and carry
`clip-rule` in `ClipRule`. There is no typed `Mask` property: a `mask="url(#id)"` attribute is kept
verbatim in the element's custom attributes, so read it with `element.CustomAttributes["mask"]` or
`element.TryGetAttribute("mask", out var value)`.

Two marker types are easy to confuse. `SvgMarker` is the `<marker>` element itself, carrying `RefX`,
`RefY`, `MarkerWidth`, `MarkerHeight`, `MarkerUnits`, `Orient`, `ViewBox`, `AspectRatio` and
`Overflow`. `SvgMarkerElement` is the abstract shape base that can carry markers -
`SvgLine`, `SvgPath`, `SvgPolygon`, `SvgPolyline` and `SvgGroup` derive from it - and it carries
`MarkerStart`, `MarkerMid` and `MarkerEnd`.

### Filter effects

`SvgFilter` carries the filter region and units, and `SvgFilterPrimitive` is the base for every
`fe*` element with `X`, `Y`, `Width`, `Height`, `Input`, `Result`, and the well-known input names
`SourceGraphic`, `SourceAlpha`, `BackgroundImage`, `BackgroundAlpha`, `FillPaint` and `StrokePaint`
as constants. The primitives are `SvgGaussianBlur`, `SvgOffset`, `SvgBlend`, `SvgColorMatrix`,
`SvgComposite`, `SvgConvolveMatrix`, `SvgDisplacementMap`, `SvgFlood`, `SvgMorphology`,
`SvgTurbulence`, `SvgTile`, `SvgMerge` with its `SvgMergeNode` children, `SvgComponentTransfer` with
its `SvgFuncR` / `SvgFuncG` / `SvgFuncB` / `SvgFuncA` children, `SvgDiffuseLighting`,
`SvgSpecularLighting`, and the `feImage` primitive - which is a different type from the `<image>`
element of the same short name. The light sources are `SvgDistantLight`, `SvgPointLight` and
`SvgSpotLight`.

### Animation elements are modeled, never run

SVG animation elements parse into a full typed model, and nothing moves. `SvgAnimationElement`
carries the shared timing surface - `Begin`, `Duration`, `End`, `Minimum`, `Maximum`, `Restart`,
`RepeatCount`, `RepeatDuration`, `AnimationFill` and the script hooks -
`SvgAnimationAttributeElement` adds the target attribute name and type, and
`SvgAnimationValueElement` adds `CalcMode`, `Values`, `KeyTimes`, `KeySplines`, `From`, `To`, `By`,
`Additive` and `Accumulate`. The concrete elements are `SvgAnimate`, `SvgAnimateColor`,
`SvgAnimateTransform`, `SvgAnimateMotion`, `SvgSet` and `SvgMPath`. To drive a frame yourself, push
a value into the target element by attribute name with `TrySetAnimationValue`, read it back with
`GetAnimationValue`, and remove it with `ClearAnimationValue`.

### Data types

Every value type is library-native. `SvgUnit` is a struct of `Value` plus `SvgUnitType`, with
`IsEmpty`, `IsNone`, `ToPercentage()`, the `Empty` and `None` statics and an implicit conversion
from `float`; equality compares value and type, so a pixel 10 does not equal a user-space 10. The
unit enum is `None, Pixel, Em, Ex, Percentage, User, Inch, Centimeter, Millimeter, Pica, Point` -
those names, in that order, are the whole enum, so `10px` parses to `SvgUnitType.Pixel` and there is
no `Px`, `In`, `Cm`, `Mm`, `Pt` or `Pc` member.

`SvgUnitCollection` is an `ObservableCollection<SvgUnit>` used for `stroke-dasharray` and the text
`x`/`y`/`dx`/`dy` lists. `SvgNumberCollection` is a `List<float>` used for `stdDeviation`,
`keyTimes`, kernel matrices and the like. `SvgPoint` is a unit-bearing 2D point, while `SvgPointF`
is a raw float point used by path segments, with an `SvgPointF.NaN` placeholder for an omitted
control point in a smooth curve command. `SvgPointCollection` is a flat list of `SvgUnit` values in
x0, y0, x1, y1 order - not a list of points - so `polygon.Points[0]` is the first x and `Count` is
twice the vertex count. `SvgRectangleF`, `SvgSizeF` and `SvgViewBox` round out the geometry, with
implicit conversions between a view box and a rectangle. `SvgAspectRatio` and `SvgOrient` are
classes rather than structs.

### Serialization

Write a whole document with `Write(XmlWriter)`, `Write(Stream, bool useBom = true)` or
`Write(string path, bool useBom = true)`. Write to a string with `SvgExtensions.GetXML()`, which
exists for both a document and a single element - the element form serializes only that subtree,
which is what makes it handy for round-trip assertions and for extracting one icon out of a sprite
sheet. A single element can also be written into an `XmlWriter` with `SvgElement.Write`, and
`ShouldWriteElement()` returning false skips it.

### Exceptions

`SvgException` derives from `FormatException` and reports a malformed SVG value. `SvgIDException`
also derives from `FormatException` directly, not from `SvgException`, and its subclasses are
`SvgIDExistsException` and `SvgIDWrongFormatException`. `SvgMemoryException` lives in
`CodeBrix.SvgParse.Exceptions`. Loading also surfaces the ordinary base-class-library exceptions:
`ArgumentNullException` for a null path, stream or string, `FileNotFoundException` from `Open(path)`,
`ArgumentException` from the `BaseUri` setter when the URI is relative, and `XmlException` from the
underlying reader for malformed XML.

## Examples

Parsing SVG from a string, finding an element by id, and enumerating the drawable elements with
their fills:

```csharp
using System;
using System.Linq;
using CodeBrix.SvgParse;

var svg = @"<svg xmlns='http://www.w3.org/2000/svg' width='200' height='200'>
    <rect id='bg' x='0' y='0' width='200' height='200' fill='white'/>
    <circle id='dot' cx='100' cy='100' r='50' fill='red'/>
    <text x='100' y='180' text-anchor='middle'>Hello</text>
</svg>";

var document = SvgDocument.FromSvg<SvgDocument>(svg);

// Find by id
var circle = document.GetElementById<SvgCircle>("dot");
Console.WriteLine($"Circle radius: {circle.Radius}");

// Find all drawable elements
foreach (var shape in document.Descendants().OfType<SvgVisualElement>())
{
    var fill = shape.Fill as SvgColorServer;
    var text = fill?.ColorValue.ToHex() ?? "(not a solid color)";
    Console.WriteLine($"{shape.GetType().Name}: fill={text}");
}
```

Loading with extra CSS through `SvgOptions`, and the same document through the
browser-compatibility loader:

```csharp
using System.IO;
using CodeBrix.SvgParse;

// Extra CSS applied on top of the document's own styles
var options = new SvgOptions("circle { fill: blue; } rect { stroke: red; }");

using var stream = File.OpenRead("image.svg");
var document = SvgDocument.Open<SvgDocument>(stream, options);

// The browser-compatibility loader: same tree, browser-aligned CSS and a
// real document base URI for relative stylesheet references.
var compat = SvgDocumentCompatibilityLoader.Open<SvgDocument>("image.svg", options);
```

Inspecting path data, parsing a `d` string, and building a segment list by hand:

```csharp
using System;
using System.Linq;
using CodeBrix.SvgParse;
using CodeBrix.SvgParse.Pathing;

var document = SvgDocument.Open("icon.svg");

foreach (var path in document.Descendants().OfType<SvgPath>())
{
    Console.WriteLine($"{path.ID}: {path.PathData.Count} segments");
    foreach (var segment in path.PathData)
    {
        Console.WriteLine($"  {segment.GetType().Name} -> {segment.End.X},{segment.End.Y}");
    }
}

// Parse a "d" string into a segment list
var segments = SvgPathBuilder.Parse("M 0 0 L 100 0 L 100 100 Z".AsSpan());

// Or build one by hand
var built = new SvgPathSegmentList();
built.Add(new SvgMoveToSegment(new SvgPointF(0f, 0f)));
built.Add(new SvgLineSegment(new SvgPointF(0f, 0f), new SvgPointF(100f, 0f)));
built.Add(new SvgClosePathSegment());

var newPath = new SvgPath { PathData = built };
document.Children.Add(newPath);
```

Reading gradients, and resolving a `url(#id)` fill through the deferred paint server:

```csharp
using System;
using System.Linq;
using CodeBrix.SvgParse;

var document = SvgDocument.Open("gradient.svg");

foreach (var gradient in document.Descendants().OfType<SvgLinearGradientServer>())
{
    Console.WriteLine($"({gradient.X1},{gradient.Y1}) -> ({gradient.X2},{gradient.Y2})");
    foreach (var stop in gradient.Stops)
    {
        Console.WriteLine($"  stop {stop.Offset}: {stop.GetColor(gradient).ToHex()}");
    }
}

// A fill written as url(#grad) parses to an SvgDeferredPaintServer.
// TryGet resolves it and returns null when it is not the type you want.
foreach (var shape in document.Descendants().OfType<SvgVisualElement>())
{
    var resolved = SvgDeferredPaintServer.TryGet<SvgLinearGradientServer>(
        shape.Fill, shape);
    if (resolved is not null)
    {
        Console.WriteLine($"{shape.ID} is filled with gradient {resolved.ID}");
    }
}
```

Building a document from scratch and writing it to a file, a stream and a string:

```csharp
using System.IO;
using CodeBrix.SvgParse;

var doc = new SvgDocument
{
    Width = new SvgUnit(SvgUnitType.Pixel, 120f),
    Height = new SvgUnit(SvgUnitType.Pixel, 120f),
    ViewBox = new SvgViewBox(0f, 0f, 120f, 120f),
};

var group = new SvgGroup { ID = "content" };
doc.Children.Add(group);

group.Children.Add(new SvgRectangle
{
    X = new SvgUnit(10f),
    Y = new SvgUnit(10f),
    Width = new SvgUnit(100f),
    Height = new SvgUnit(100f),
    CornerRadiusX = new SvgUnit(8f),
    CornerRadiusY = new SvgUnit(8f),
    Fill = new SvgColorServer(SvgColor.FromRgb(0x33, 0x66, 0xCC)),
    Stroke = new SvgColorServer(SvgColor.Black),
    StrokeWidth = new SvgUnit(2f),
});

var label = new SvgText("CodeBrix");
label.X.Add(new SvgUnit(60f));      // x/y are SvgUnitCollection lists
label.Y.Add(new SvgUnit(65f));
label.TextAnchor = SvgTextAnchor.Middle;
label.FontFamily = "sans-serif";
label.FontSize = new SvgUnit(14f);
group.Children.Add(label);

doc.Write("out.svg");                    // to a file
using var ms = new MemoryStream();
doc.Write(ms, useBom: false);            // to a stream
string xml = doc.GetXML();               // to a string
```

Applying style declarations programmatically, with specificity, and flushing the cascade once:

```csharp
using System.Linq;
using CodeBrix.SvgParse;

var document = SvgDocument.Open("chart.svg");

foreach (var rect in document.Descendants().OfType<SvgRectangle>())
{
    // specificity follows CSS rules: higher wins
    rect.AddStyle("fill", "#3366cc", 100);
    rect.AddStyle("stroke-width", "2", 100);
}

// Stage everything first, then flush ONCE for the whole subtree.
document.FlushStyles(true);
```

## Pitfalls

- Do not confuse the package ID with the namespace. Package: `CodeBrix.SvgParse.MsplLicenseForever`.
  Namespace: `CodeBrix.SvgParse`.
- Do not expect a public `element.ElementName` or `element.Attributes`; both are `protected
  internal`. Use the CLR type (`is SvgRectangle`, `OfType<SvgPath>()`, `GetType().Name`),
  `ContainsAttribute` / `TryGetAttribute` for raw attribute strings, `CustomAttributes` for anything
  the library does not model, and `NonSvgElement.Name` for foreign-namespace tag names.
- Do not write `[SvgAttribute("x")]` with one argument in your own code - that constructor is
  internal. Use the two-argument form,
  `[SvgAttribute("x", SvgNamespaces.SvgNamespace)]`.
- Do not expect your own `[SvgElement("...")]` classes to be produced by the parser. The
  element-name table is generated at build time inside this package. Unknown SVG-namespace elements
  become `SvgUnknownElement` and foreign ones become `NonSvgElement`, with their attributes
  preserved in `CustomAttributes`.
- Do not assume container elements are not visual elements. `SvgGroup` is an `SvgVisualElement`. The
  elements that are not include `SvgDefinitionList`, `SvgClipPath`, `SvgMask`, `SvgGradientStop`,
  the paint servers, `SvgFilter` and the filter primitives, the font and glyph elements, and the
  animation elements.
- Do not assume `SvgText.X` and `SvgText.Y` are `SvgUnit`. On `SvgTextBase` they are
  `SvgUnitCollection` per-glyph lists, and `Rotate` is a raw string.
- Do not treat `SvgPointCollection` as a list of points. It is a flat list of x, y, x, y values, so
  its count is twice the vertex count.
- Do not look for `SvgSkewX` or `SvgSkewY`. The type is `SvgSkew(float x, float y)`.
- Do not confuse `SvgMarker` (the `<marker>` element) with `SvgMarkerElement` (the abstract shape
  base that carries `MarkerStart` / `MarkerMid` / `MarkerEnd`).
- Do not confuse the two `SvgImage` types: `CodeBrix.SvgParse.SvgImage` is the `<image>` element and
  `CodeBrix.SvgParse.FilterEffects.SvgImage` is the `feImage` filter primitive. If both usings are
  in scope you must qualify.
- Do not misspell `ResolveExternalXmlEntities`.
- Do not set `Fill` or `Stroke` to an `SvgColor`. They are `SvgPaintServer`; wrap the color:
  `element.Fill = new SvgColorServer(SvgColor.Red)`.
- Do not read a `url(#id)` fill as if it were the target paint server. It parses to an
  `SvgDeferredPaintServer`; resolve it with `SvgDeferredPaintServer.TryGet<T>(shape.Fill, shape)` or
  by calling `EnsureServer(styleOwner)`.
- Do not forget SVG attribute inheritance. `fill`, `stroke`, `font-family` and most other
  presentation properties inherit from the parent chain, so a property that looks unset on a child
  may be resolved from an ancestor.
- Do not modify `element.Children` while enumerating it, or while enumerating `Descendants()`.
  Snapshot with `ToList()` first.
- Do not mix `SvgUnit` types carelessly. Equality compares value and type, so
  `new SvgUnit(SvgUnitType.Pixel, 10f) != new SvgUnit(SvgUnitType.User, 10f)`.
- Do not expect `SvgDocument.EmitNamedColorsOnSerialization` to affect documents that already exist;
  set the instance property instead.
- Do not use `System.Drawing` types with this library. Use `SvgColor`, `SvgPointF`, `SvgRectangleF`
  and `SvgSizeF`, and add your own conversion helpers at the boundary if your application needs
  them.
- Do not expect `SvgColor.Empty` and `SvgColor.Transparent` to be distinguishable - both are packed
  ARGB 0. Use the `SvgPaintServer` sentinels when "unset" and "transparent" must differ.
- Do not assume `polyline is not SvgPolygon`. `SvgPolyline` derives from `SvgPolygon`, so test for
  `SvgPolyline` first.
- Do not set `SvgDocument.BaseUri` to a relative URI; the setter throws `ArgumentException`.
- Do not enable `ResolveExternalImages = Remote`, or `ResolveExternalElements = Remote`, for
  untrusted documents. That permits the parser to reach out over the network.
- Do not expect the animation elements to animate. They are parsed and re-serialized; the timeline
  is yours to run.
- Event-argument classes carry public fields, not properties.

> [!TIP]
> Use `GetElementById()` instead of scanning `Descendants()` when you know the id - the document
> keeps an id index, while `Descendants()` walks the whole tree. Prefer `Open<SvgDocument>(stream)`
> over `FromSvg<SvgDocument>(string)` for large files so the file never has to be materialized as
> one big string. Copy the smallest subtree that gives you what you need, and stage every
> `AddStyle()` call before one `FlushStyles(true)`, because flushing per declaration re-resolves the
> cascade every time.

## Documentation and source

There are no sample applications, demo applications or tools in this repository: everything here
either ships in the package or exists to build and test it. The test project is the best available
body of compiling example code - every test builds an SVG document from an inline string and asserts
against the resulting object model, with no fixture files, no environment variables and no network
access.

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.SvgParse/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.SvgParse/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.SvgParse/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.SvgParse.Tests](https://github.com/ellisnet/CodeBrix.SvgParse/tree/main/tests/CodeBrix.SvgParse.Tests) |
| Library source | [src/CodeBrix.SvgParse](https://github.com/ellisnet/CodeBrix.SvgParse/tree/main/src/CodeBrix.SvgParse) |

Run the suite from the repository root:

```bash
dotnet test CodeBrix.SvgParse.slnx
```

## License

CodeBrix.SvgParse is licensed under the Microsoft Public License (Ms-PL); the license is also named
in the package ID (`CodeBrix.SvgParse.MsplLicenseForever`), and the package requires license
acceptance. For the provenance and licensing of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.SvgParse/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.SkiaSvg](CodeBrix.SkiaSvg.md) - a rendering backend built on this document object model
- [CodeBrix.StyleSheetParse](CodeBrix.StyleSheetParse.md) - the CSS parser and selector engine behind
  the styling support
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.SvgParse on GitHub](https://github.com/ellisnet/CodeBrix.SvgParse) - source and tests
