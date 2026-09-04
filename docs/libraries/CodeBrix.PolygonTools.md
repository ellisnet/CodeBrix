<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.PolygonTools</sub>

# CodeBrix.PolygonTools

**CodeBrix.PolygonTools performs the four boolean set operations - intersection, union, difference
and exclusive-or - on arbitrary sets of polygons and open paths, and inflates or deflates polygons
with miter, round or square joins.** It is fully managed, dependency-free and cross-platform, and it
operates on 64-bit integer coordinates so that its arithmetic is exact and its results are robust:
self-intersecting polygons, holes, and polygons with coincident or collinear edges all come out
correct. Use it from any .NET 10 application, or from a CodeBrix.Platform application, wherever 2D
geometry has to be combined, cut apart, grown or shrunk.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.PolygonTools](https://github.com/ellisnet/CodeBrix.PolygonTools) |
| **Packages** | [`CodeBrix.PolygonTools.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.PolygonTools.MitLicenseForever) |
| **License** | MIT, with portions under the Boost Software License 1.0; see [License](#license) |
| **Requires** | .NET 10 or later, and nothing else - the package references no other NuGet package and no native library |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Any platform .NET 10 runs on; there is no OS-specific code, no P/Invoke, no `unsafe` block and no reflection |

## What it does

- Runs all four boolean clipping operations on sets of polygons: intersection, union, difference
  and exclusive-or.
- Offers four filling rules - even-odd, non-zero, positive and negative - so the library
  interoperates with GDI+, Cairo, OpenGL, SVG and similar rendering models.
- Offsets polygons (inflating and deflating) with miter, round and square joins, and butt, square
  or round line ends.
- Clips open paths (polylines) as well as closed polygons.
- Handles self-intersecting polygons, holes, and polygons with coincident or collinear edges.
- Returns results either as a flat list of paths or as a `PolyTree` that preserves the parent/child
  nesting of outer polygons and their holes.
- Adds supporting operations: `SimplifyPolygon`, `SimplifyPolygons`, `CleanPolygon`,
  `CleanPolygons`, `Orientation`, `Area`, `PointInPolygon`, `ReversePaths`, `MinkowskiSum`,
  `MinkowskiDiff`, and the three `PolyTree` flattening helpers.
- Uses exact 64-bit integer arithmetic, with no floating-point rounding artifacts in the clipping
  result.

## When to use it

Reach for CodeBrix.PolygonTools whenever two or more shapes have to be combined into one answer:
computing the visible part of a room against a viewport, subtracting a cut-out from a plate, merging
overlapping selection regions, growing a shape into a tolerance band, converting a stroked polyline
into a fillable ribbon, or deciding which parts of a route fall inside a zone. It is a geometry
kernel, so it is equally at home in a console tool, a background service and an interactive
application - there is nothing to initialize, register or configure at start-up. Construct a
`PolyClip` or a `PolyClipOffset` and use it.

It does no rendering. It produces coordinate lists; drawing them is your job. There is no dependency
on SkiaSharp, `System.Drawing`, WPF or any other graphics stack, and no conversion helper to their
point types - you convert your own vertices in and out.

The rest of what it deliberately leaves out:

- No floating-point coordinate API. Everything is `long`; you do the scaling.
- No curves. Beziers and arcs must be flattened to line segments before they reach this library, and
  come back flattened.
- No 3D, no meshes, no triangulation, no convex hull, no Delaunay, no Voronoi, no polygon
  decomposition into convex parts.
- No boolean operations on anything but polygons and polylines - no regions, no rasters, no distance
  fields.
- No SVG, DXF, WKT or GeoJSON parsing or writing. Bring your own I/O.
- No geographic or geodetic awareness: coordinates are plain Cartesian integers, with no projection,
  datum or spherical geometry.
- No async API and no cancellation. Every operation is synchronous and runs to completion; a very
  large clip will block its thread.
- No thread safety on an engine instance, and no internal parallelism.
- No `IDisposable`, no finalizers, no unmanaged resources. Let instances go out of scope.
- No public exception type, no `Span<T>` or `Memory<T>` overloads, and no `IEnumerable<IntPoint>`
  overloads - `List<>` is the currency throughout.

## Getting started

```bash
dotnet add package CodeBrix.PolygonTools.MitLicenseForever
```

The package ID carries the `.MitLicenseForever` suffix but the assembly and the namespaces do not -
they are `CodeBrix.PolygonTools`. There is no package named plain `CodeBrix.PolygonTools`.
XML documentation (IntelliSense) ships alongside the assembly.

Three namespaces cover the public surface:

```csharp
using CodeBrix.PolygonTools;                //PolyClip, PolyClipBase,
                                            //PolyClipOffset
using CodeBrix.PolygonTools.Enumerations;   //ClipType, PolyType,
                                            //PolyFillType, JoinType,
                                            //EndType
using CodeBrix.PolygonTools.Models;         //IntPoint, IntRect,
                                            //DoublePoint, PolyNode,
                                            //PolyTree, IntersectNode,
                                            //MyIntersectNodeSort
```

Almost every real program needs all three, plus `using System.Collections.Generic;` for the `List<>`
types that make up paths.

> [!IMPORTANT]
> A fourth namespace, `CodeBrix.PolygonTools.Internal`, holds the implementation detail of the
> clipping algorithm. Everything in it is `internal` and is not part of the public API. Never write
> `using CodeBrix.PolygonTools.Internal;`.

A complete console project - the whole project file, with nothing else in it:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>disable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CodeBrix.PolygonTools.MitLicenseForever" />
  </ItemGroup>

</Project>
```

And `Program.cs`, which intersects two overlapping squares:

```csharp
using System;
using System.Collections.Generic;
using CodeBrix.PolygonTools;
using CodeBrix.PolygonTools.Enumerations;
using CodeBrix.PolygonTools.Models;

namespace MyPolygonApp;

internal static class Program
{
    private static void Main()
    {
        var subject = new List<IntPoint>
        {
            new IntPoint(0, 0), new IntPoint(100, 0),
            new IntPoint(100, 100), new IntPoint(0, 100)
        };
        var clip = new List<IntPoint>
        {
            new IntPoint(50, 50), new IntPoint(150, 50),
            new IntPoint(150, 150), new IntPoint(50, 150)
        };

        var polyClip = new PolyClip();
        polyClip.AddPath(subject, PolyType.ptSubject, true);
        polyClip.AddPath(clip, PolyType.ptClip, true);

        var solution = new List<List<IntPoint>>();
        polyClip.Execute(ClipType.ctIntersection, solution,
                         PolyFillType.pftNonZero, PolyFillType.pftNonZero);

        foreach (var path in solution)
        {
            Console.WriteLine(string.Join(" ",
                path.ConvertAll(p => $"({p.X},{p.Y})")));
        }
    }
}

//Output: the four corners of the 50x50 overlap square -
//(50,50) (100,50) (100,100) (50,100), in some rotation
```

`dotnet run` prints the overlap square. Nothing else is required - no initialization call, no
configuration file, no native dependency.

## Key concepts

### The path model

There is no `Path` or `Paths` type in the public API. Paths are plain lists:

```text
a path  (one polygon or one polyline)  ==  List<IntPoint>
a set of paths                         ==  List<List<IntPoint>>
```

Everything you pass in and get back is a plain `List<IntPoint>` or `List<List<IntPoint>>`. Declare
your own aliases if the generic types get noisy:

```csharp
using Path = System.Collections.Generic.List<CodeBrix.PolygonTools.Models.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<CodeBrix.PolygonTools.Models.IntPoint>>;
```

A closed polygon is not repeated at the end: do not append a copy of the first vertex to close the
ring. A square is four points, not five.

Vertex order determines orientation. With the non-zero, positive or negative filling rules,
orientation decides whether a ring is an outer boundary or a hole. `PolyClip.Orientation(path)`
returns true for a path whose area is positive - counter-clockwise in a conventional Y-up coordinate
system, clockwise when Y points down as it does on most screens.

### Coordinate model and scaling

Coordinates are 64-bit signed integers: `IntPoint.X` and `IntPoint.Y` are `long`. That is what makes
the clipping arithmetic exact and the results robust, and it is also the single largest source of
consumer mistakes. Two range constants define the limits, exposed on `PolyClipBase` and therefore
reachable as `PolyClip.loRange` and `PolyClip.hiRange` too:

```csharp
public const long loRange = 0x3FFFFFFF;             //1,073,741,823
public const long hiRange = 0x3FFFFFFFFFFFFFFFL;    //4,611,686,018,427,387,903
```

Any coordinate whose absolute value exceeds `hiRange` is rejected: `PolyClipBase.AddPath` throws
with the message `Coordinate outside allowed range`. Inside `loRange` the engine uses ordinary
64-bit arithmetic. Beyond `loRange` but within `hiRange` it switches to wide arithmetic
automatically; that path is correct but slower, and offsetting in particular is only fully reliable
inside `loRange`. The practical rule: keep every coordinate, and every coordinate a computed result
could reach - offsetting grows the bounds by the delta - inside `loRange`.

To use floating-point geometry, multiply by a fixed scale, convert to `long`, clip, then divide back
down. Pick the scale from the precision you need: a scale of 1,000 keeps three decimal places, a
scale of 1,000,000 keeps six.

`IntPoint` has a `double` constructor, `new IntPoint(double x, double y)`, but it truncates each
component toward zero (`X = (long)x`) - it does not round. That is asymmetric about the origin: 1.9
becomes 1 and -1.9 becomes -1. When you need round-half-away-from-zero, do it yourself:

```csharp
new IntPoint((long)Math.Round(x * scale, MidpointRounding.AwayFromZero),
             (long)Math.Round(y * scale, MidpointRounding.AwayFromZero))
```

### The value types

`IntPoint` is a single 2D vertex with 64-bit integer coordinates. Public fields, not properties, so
they are directly assignable:

```csharp
public long X;
public long Y;

public IntPoint(long x, long y)
public IntPoint(double x, double y)   //TRUNCATES toward zero
public IntPoint(IntPoint pt)          //copy constructor

public static bool operator ==(IntPoint a, IntPoint b)
public static bool operator !=(IntPoint a, IntPoint b)
public override bool Equals(object obj)
public override int GetHashCode()
```

Equality is value equality on X and Y, and `Equals(object)` also accepts a boxed `IntPoint`. Because
it is a struct, `default(IntPoint)` is (0, 0) and assigning one `IntPoint` to another copies it.

`IntRect` is an axis-aligned bounding box, and note the lower-case field names:

```csharp
public long left;
public long top;
public long right;
public long bottom;

public IntRect(long l, long t, long r, long b)
public IntRect(IntRect ir)            //copy constructor
```

It is produced by `PolyClipBase.GetBounds(List<List<IntPoint>> paths)` and is a plain value carrier:
there is no `Width`, `Height`, `Contains` or `Intersects` member, and no equality operator. Compute
those yourself (`rect.right - rect.left`, and so on). The bounds of an empty path set are all zeros.

`DoublePoint` is a floating-point 2D point, used internally for edge normals and available to you as
a scratch type when scaling:

```csharp
public double X;
public double Y;

public DoublePoint(double x = 0, double y = 0)
public DoublePoint(DoublePoint dp)    //copy constructor
public DoublePoint(IntPoint ip)       //widens a vertex to double
```

There is no implicit conversion back to `IntPoint`; construct one with `new IntPoint(dp.X, dp.Y)`,
which truncates toward zero.

### Enumerations

| Enumeration | Members |
| --- | --- |
| `ClipType` | `ctIntersection`, `ctUnion`, `ctDifference`, `ctXor` |
| `PolyType` | `ptSubject`, `ptClip` |
| `PolyFillType` | `pftEvenOdd`, `pftNonZero`, `pftPositive`, `pftNegative` |
| `JoinType` | `jtSquare`, `jtRound`, `jtMiter` |
| `EndType` | `etClosedPolygon`, `etClosedLine`, `etOpenButt`, `etOpenSquare`, `etOpenRound` |

The short prefixes are part of the member names. Do not "modernize" them in your code - they are the
actual member names.

`ctDifference` subtracts the clip paths from the subject paths, in that order; swapping which set
you add as `ptSubject` reverses the result. `pftEvenOdd` is the default for every `Execute` and
`SimplifyPolygon` overload that takes a single fill type, and it ignores orientation - use
`pftNonZero` when your rings carry meaningful winding, which is what you want for most union and
offsetting work. `etClosedPolygon` offsets a closed ring outward or inward; `etClosedLine` treats a
closed path as a line and offsets both sides of it, producing a ring-shaped result; the three
`etOpen*` values offset an open polyline and differ only in how the two ends are capped.

### PolyClipBase, where geometry goes in

`PolyClipBase` is the base class of `PolyClip`. It has an internal constructor, so you never
instantiate it directly, but it carries the members that add geometry and you call them through a
`PolyClip` instance:

```csharp
public bool AddPath(List<IntPoint> pg, PolyType polyType, bool Closed)
public bool AddPaths(List<List<IntPoint>> ppg, PolyType polyType,
                     bool closed)
public virtual void Clear()
public static IntRect GetBounds(List<List<IntPoint>> paths)
public bool PreserveCollinear { get; set; }
public void Swap(ref long val1, ref long val2)
public const long loRange = 0x3FFFFFFF;
public const long hiRange = 0x3FFFFFFFFFFFFFFFL;
```

`AddPath` returns false - it does not throw - when a path has too few distinct vertices to
contribute to the result: fewer than 2 after duplicate stripping, or fewer than 3 for a closed path.
Check the return value if a silently ignored input would be a bug in your program. It does throw
when an open path is supplied as clip geometry (`AddPath: Open paths must be subject.`) and when a
coordinate exceeds `hiRange` (`Coordinate outside allowed range`). `AddPaths` returns true when at
least one of its paths was accepted.

`Clear()` removes every subject and clip path; `PolyClip` does not override it, so the inherited
implementation is what runs. `PreserveCollinear` keeps vertices that lie on a straight line between
their neighbors instead of removing them - set it before adding paths. `GetBounds` is static and
takes a path set, not a single path, so wrap a lone path:
`PolyClipBase.GetBounds(new List<List<IntPoint>> { path })`.

### PolyClip, the clipping engine

`PolyClip : PolyClipBase` is the clipping engine. Its constructor takes a bitwise OR of three
initialization constants:

```csharp
public PolyClip(int InitOptions = 0)

    public const int ioReverseSolution  = 1;
    public const int ioStrictlySimple   = 2;
    public const int ioPreserveCollinear = 4;
```

For example, `new PolyClip(PolyClip.ioStrictlySimple | PolyClip.ioPreserveCollinear)`. Each constant
sets the matching property, so the property setters are an equally good way to configure an
instance: `ReverseSolution` reverses the orientation of the solution paths (default false),
`StrictlySimple` guarantees that no polygon touches or overlaps another at a vertex (default false,
because enforcing it is comparatively expensive), and `PreserveCollinear` is inherited from the base
class.

There are four `Execute` overloads:

```csharp
public bool Execute(ClipType clipType,
                    List<List<IntPoint>> solution,
                    PolyFillType FillType = PolyFillType.pftEvenOdd)
public bool Execute(ClipType clipType, PolyTree polytree,
                    PolyFillType FillType = PolyFillType.pftEvenOdd)
public bool Execute(ClipType clipType,
                    List<List<IntPoint>> solution,
                    PolyFillType subjFillType,
                    PolyFillType clipFillType)
public bool Execute(ClipType clipType, PolyTree polytree,
                    PolyFillType subjFillType,
                    PolyFillType clipFillType)
```

The two-fill-type overloads are the primitives; the single-fill-type overloads forward to them,
passing the one value for both. The solution collection - list or tree - is cleared before it is
populated, so an already-populated container is replaced, not appended to. `Execute` returns false
rather than throwing when the same instance is already inside an `Execute` call. The
`List<List<IntPoint>>` overloads throw with the message
`Error: PolyTree struct is needed for open path clipping.` when any open subject path was added; use
a `PolyTree` overload in that case.

### The static helpers

`PolyClip` also carries the supporting operations, all static:

```csharp
public static void ReversePaths(List<List<IntPoint>> polys)
public static bool Orientation(List<IntPoint> poly)
public static double Area(List<IntPoint> poly)
public static int PointInPolygon(IntPoint pt, List<IntPoint> path)
    Returns 0 outside, -1 on an edge or vertex, +1 inside.
public static List<List<IntPoint>> SimplifyPolygon(
    List<IntPoint> poly,
    PolyFillType fillType = PolyFillType.pftEvenOdd)
public static List<List<IntPoint>> SimplifyPolygons(
    List<List<IntPoint>> polys,
    PolyFillType fillType = PolyFillType.pftEvenOdd)
public static List<IntPoint> CleanPolygon(
    List<IntPoint> path, double distance = 1.415)
public static List<List<IntPoint>> CleanPolygons(
    List<List<IntPoint>> polys, double distance = 1.415)
public static List<List<IntPoint>> MinkowskiSum(
    List<IntPoint> pattern, List<IntPoint> path, bool pathIsClosed)
public static List<List<IntPoint>> MinkowskiSum(
    List<IntPoint> pattern, List<List<IntPoint>> paths,
    bool pathIsClosed)
public static List<List<IntPoint>> MinkowskiDiff(
    List<IntPoint> poly1, List<IntPoint> poly2)
public static List<List<IntPoint>> PolyTreeToPaths(PolyTree polytree)
public static List<List<IntPoint>> OpenPathsFromPolyTree(
    PolyTree polytree)
public static List<List<IntPoint>> ClosedPathsFromPolyTree(
    PolyTree polytree)
```

`Area` is signed - negative for a path wound the other way - so take `Math.Abs(...)` when you want
magnitude. `SimplifyPolygon` and `SimplifyPolygons` internally run a union with `StrictlySimple`
set, so they both remove self-intersections and merge overlapping regions. `CleanPolygon` and
`CleanPolygons` remove vertices that are closer together than `distance` (the default 1.415 is
over the diagonal of a unit cell) and vertices that are effectively collinear; that is a cleanup
pass, not a simplification pass. The three `PolyTree` flattening helpers are how you get a flat list
back out of a tree: `PolyTreeToPaths` returns everything, `ClosedPathsFromPolyTree` only the closed
polygons, and `OpenPathsFromPolyTree` only the open paths.

Bounds, winding, area and hit testing in one pass:

```csharp
var paths = new List<List<IntPoint>> { outer, hole };

IntRect bounds = PolyClipBase.GetBounds(paths);
Console.WriteLine($"{bounds.left},{bounds.top} .. "
    + $"{bounds.right},{bounds.bottom}   "
    + $"{bounds.right - bounds.left} x {bounds.bottom - bounds.top}");

Console.WriteLine(PolyClip.Orientation(outer));       //winding direction
Console.WriteLine(PolyClip.Area(outer));              //signed area

Console.WriteLine(PolyClip.PointInPolygon(new IntPoint(150, 150), outer));
//  1 = inside, 0 = outside, -1 = exactly on an edge or vertex

PolyClip.ReversePaths(paths);                         //in place
```

`MinkowskiSum` sweeps one shape along another, which is how you build a swept volume or a stroked
outline of arbitrary brush shape:

```csharp
var brush = new List<IntPoint>
{
    new IntPoint(-5, -5), new IntPoint(5, -5),
    new IntPoint(5, 5), new IntPoint(-5, 5)
};
var stroke = new List<IntPoint>
{
    new IntPoint(0, 0), new IntPoint(200, 0), new IntPoint(200, 200)
};

var swept = PolyClip.MinkowskiSum(brush, stroke, false);  //open path
var union = PolyClip.SimplifyPolygons(swept, PolyFillType.pftNonZero);
```

`MinkowskiDiff(poly1, poly2)` gives the difference (erosion) form; it takes two paths and no
closed/open flag.

### PolyClipOffset, the offsetting engine

`PolyClipOffset` is the offsetting (inflate/deflate) engine. It does not derive from `PolyClipBase`,
so it has its own `AddPath`, `AddPaths` and `Clear` with different signatures:

```csharp
public PolyClipOffset(double miterLimit = 2.0, double arcTolerance = 0.25)

public double MiterLimit { get; set; }
public double ArcTolerance { get; set; }

public void AddPath(List<IntPoint> path, JoinType joinType,
                    EndType endType)
public void AddPaths(List<List<IntPoint>> paths, JoinType joinType,
                     EndType endType)
public void Clear()
public void Execute(ref List<List<IntPoint>> solution, double delta)
public void Execute(ref PolyTree solution, double delta)
```

`MiterLimit` is the limit beyond which a mitered join is squared off instead, expressed as a
multiple of the offset delta; the default is 2.0. `ArcTolerance` is the maximum distance by which a
rounded join may deviate from the true arc; the default is 0.25, and smaller values produce more
segments. `ArcTolerance` only matters for `jtRound` joins and `etOpenRound` / `etClosedLine` ends,
and `MiterLimit` only matters for `jtMiter`.

Both `Execute` overloads take their solution by ref, and both clear it first: you still pass a
constructed instance, and the same instance comes back populated. `delta` is in the same integer
units as the coordinates - positive inflates a closed polygon, negative deflates it, and for the
`etOpen*` end types the magnitude is the half-width of the resulting ribbon. `Clear()` removes every
added path and returns the instance to its initial state, keeping `MiterLimit` and `ArcTolerance`.

```csharp
var offset = new PolyClipOffset();          //miterLimit 2.0, arcTol 0.25
offset.AddPath(subject, JoinType.jtMiter, EndType.etClosedPolygon);

var inflated = new List<List<IntPoint>>();
offset.Execute(ref inflated, 10.0);         //grow by 10 units

offset.Clear();                             //MANDATORY before reuse
offset.AddPath(subject, JoinType.jtRound, EndType.etClosedPolygon);

var deflated = new List<List<IntPoint>>();
offset.Execute(ref deflated, -10.0);        //shrink by 10 units

Console.WriteLine($"{inflated.Count} / {deflated.Count}");
```

Deflating far enough makes a polygon disappear: the solution comes back with zero paths rather than
throwing. Always check `solution.Count` before indexing.

### PolyTree and PolyNode

Ask for a `PolyTree` solution when you need to know which rings are holes inside which outer
polygons. `PolyNode` is one node of that tree; nodes are created by the engine and you read them:

```csharp
public List<IntPoint> Contour { get; }   //this node's path
public List<PolyNode> Childs { get; }    //note the spelling
public int ChildCount { get; }           //== Childs.Count
public PolyNode Parent { get; }          //null for the tree root
public bool IsHole { get; }              //computed from nesting depth
public bool IsOpen { get; set; }         //an open path, not a polygon
public PolyNode GetNext()                //next node in a depth-first
                                         //walk, or null at the end
```

`Contour` returns the live list the engine built - it is not a defensive copy, so do not mutate it in
place if you intend to keep walking the tree. `IsHole` is derived by counting parents up to the root:
a node at odd depth is a hole, a node at even depth is an outer polygon, so holes and outer polygons
alternate with each level of nesting. `Childs` is spelled without the "r"; that is the public API
name.

`PolyTree : PolyNode` is the root of a solution tree and the type you pass to the `PolyTree`
overloads of `Execute`. Construct it yourself with `new PolyTree()`:

```csharp
public PolyNode GetFirst()   //first child, or null when empty
public int Total { get; }    //node count, excluding the root
public void Clear()          //empties the tree
```

`PolyTree` inherits every `PolyNode` member, so the root also has `Childs`, `ChildCount` and
`Contour` - but the root's own `Contour` is empty and its `IsHole` is meaningless. Start from
`GetFirst()` and walk with `GetNext()`, or recurse over `Childs`. `Total` compensates for the hidden
outer polygon that a negative offset can introduce, so it can be one less than the raw node count.

Cutting a square hole out of a square, then walking the result:

```csharp
var outer = new List<IntPoint>
{
    new IntPoint(0, 0), new IntPoint(300, 0),
    new IntPoint(300, 300), new IntPoint(0, 300)
};
var hole = new List<IntPoint>
{
    new IntPoint(100, 100), new IntPoint(200, 100),
    new IntPoint(200, 200), new IntPoint(100, 200)
};

var polyClip = new PolyClip();
polyClip.AddPath(outer, PolyType.ptSubject, true);
polyClip.AddPath(hole, PolyType.ptClip, true);

var tree = new PolyTree();
polyClip.Execute(ClipType.ctDifference, tree,
                 PolyFillType.pftNonZero, PolyFillType.pftNonZero);

Console.WriteLine($"nodes: {tree.Total}");

for (var node = tree.GetFirst(); node != null; node = node.GetNext())
{
    var kind = node.IsOpen ? "open" : (node.IsHole ? "hole" : "outer");
    Console.WriteLine($"  {kind}, {node.Contour.Count} vertices, "
        + $"{node.ChildCount} child(ren)");
}

//Or flatten the tree instead of walking it:
var closed = PolyClip.ClosedPathsFromPolyTree(tree);
```

Recursion over `Childs` works too, and is easier when you care about depth:

```csharp
static void Walk(PolyNode node, int depth)
{
    Console.WriteLine(new string(' ', depth * 2)
        + (node.IsHole ? "hole" : "outer"));
    foreach (var child in node.Childs) { Walk(child, depth + 1); }
}
foreach (var child in tree.Childs) { Walk(child, 0); }
```

### Errors, and the exception you cannot catch by type

`PolyClipException` is declared with no access modifier, so it is internal to the assembly. You
cannot write `catch (PolyClipException)` in your own code; catch `System.Exception` - or inspect
`ex.GetType().Name` if you must distinguish it - and read `ex.Message`, which carries the diagnostic
strings quoted throughout this page.

`IntersectNode` is public but opaque: every field on it is `internal`, and instances are created and
consumed by the clipping algorithm itself. `MyIntersectNodeSort` is the `IComparer<IntersectNode>`
the engine uses to order intersections within a scanbeam
(`public int Compare(IntersectNode node1, IntersectNode node2)`; it throws `ArgumentNullException` on
a null argument). Neither type is useful to a consumer.

### The five rules

```text
1. Integers only - scale doubles up, scale results back down.
2. Clear() between operations.
3. Open paths need a PolyTree solution.
4. PolyClipOffset.Execute takes solution by ref, and clears it.
5. Simplify before you offset.
```

## Examples

The union of two overlapping squares, with an area check that proves the overlap was merged rather
than double-counted:

```csharp
using System;
using System.Collections.Generic;
using CodeBrix.PolygonTools;
using CodeBrix.PolygonTools.Enumerations;
using CodeBrix.PolygonTools.Models;

var subject = new List<IntPoint>
{
    new IntPoint(0, 0), new IntPoint(100, 0),
    new IntPoint(100, 100), new IntPoint(0, 100)
};

var clip = new List<IntPoint>
{
    new IntPoint(50, 50), new IntPoint(150, 50),
    new IntPoint(150, 150), new IntPoint(50, 150)
};

var solution = new List<List<IntPoint>>();

var polyClip = new PolyClip();
polyClip.AddPath(subject, PolyType.ptSubject, true);
polyClip.AddPath(clip, PolyType.ptClip, true);
var ok = polyClip.Execute(ClipType.ctUnion, solution,
                          PolyFillType.pftNonZero, PolyFillType.pftNonZero);

Console.WriteLine($"{ok} - {solution.Count} path(s), "
    + $"area {Math.Abs(PolyClip.Area(solution[0]))}");
//True - 1 path(s), area 17500
//(two 100x100 squares, 10000 + 10000, less the 2500 they share)
```

Do not assume a particular starting vertex or winding direction in the solution: the engine emits
the ring it computed, and only the set of vertices and the enclosed area are contractual. Normalize
yourself if you need a canonical order.

Three boolean operations from one reused instance, showing the mandatory `Clear()` between them:

```csharp
var polyClip = new PolyClip();
var result = new List<List<IntPoint>>();

foreach (var op in new[] { ClipType.ctIntersection, ClipType.ctDifference,
                           ClipType.ctXor })
{
    polyClip.Clear();                       //MANDATORY between operations
    polyClip.AddPath(subject, PolyType.ptSubject, true);
    polyClip.AddPath(clip, PolyType.ptClip, true);
    polyClip.Execute(op, result, PolyFillType.pftNonZero,
                     PolyFillType.pftNonZero);

    Console.WriteLine($"{op}: {result.Count} path(s)");
}
```

The round trip you need whenever your source geometry is `double`, `float`, `PointF`, `SKPoint` or
similar - scale up, clip, scale back down:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using CodeBrix.PolygonTools;
using CodeBrix.PolygonTools.Enumerations;
using CodeBrix.PolygonTools.Models;

const double Scale = 1000.0;   //keeps three decimal places

static List<IntPoint> ToIntPath(IEnumerable<(double X, double Y)> pts) =>
    pts.Select(p => new IntPoint(p.X * Scale, p.Y * Scale)).ToList();

static List<(double X, double Y)> ToDoublePath(List<IntPoint> path) =>
    path.Select(p => (p.X / Scale, p.Y / Scale)).ToList();

var subjectF = new[] { (0.0, 0.0), (10.5, 0.0), (10.5, 10.5), (0.0, 10.5) };
var clipF    = new[] { (5.25, 5.25), (15.0, 5.25), (15.0, 15.0),
                       (5.25, 15.0) };

var polyClip = new PolyClip();
polyClip.AddPath(ToIntPath(subjectF), PolyType.ptSubject, true);
polyClip.AddPath(ToIntPath(clipF), PolyType.ptClip, true);

var scaled = new List<List<IntPoint>>();
polyClip.Execute(ClipType.ctIntersection, scaled,
                 PolyFillType.pftNonZero, PolyFillType.pftNonZero);

foreach (var path in scaled)
{
    foreach (var (x, y) in ToDoublePath(path))
    {
        Console.WriteLine($"({x}, {y})");
    }
}
//the 5.25 x 5.25 overlap square, as the four points
//(5.25, 5.25) (10.5, 5.25) (10.5, 10.5) (5.25, 10.5) in some rotation

//Sanity check: the scaled area is Scale*Scale times the real area, so
//divide by Scale*Scale to get back to source units.
var realArea = Math.Abs(PolyClip.Area(scaled[0])) / (Scale * Scale);
Console.WriteLine(realArea);     //27.5625
```

Note the choice of scale. `Scale` must be large enough for your precision and small enough that
`maxCoordinate * Scale` stays inside `loRange` (1,073,741,823). With a scale of 1,000 that allows
source coordinates up to about +/-1,073,741.

Turning an open polyline into a fillable polygon - a stroked line becomes a ribbon:

```csharp
var polyline = new List<IntPoint>
{
    new IntPoint(0, 0), new IntPoint(100, 0), new IntPoint(100, 100)
};

var offset = new PolyClipOffset();
offset.AddPath(polyline, JoinType.jtRound, EndType.etOpenRound);

var ribbon = new List<List<IntPoint>>();
offset.Execute(ref ribbon, 5.0);   //a 10-unit-wide stroke with round caps
```

Swap `EndType.etOpenButt` for flat caps and `EndType.etOpenSquare` for caps that project half the
width past each end.

Clipping an open path against a polygon, which requires a `PolyTree` solution:

```csharp
var line = new List<IntPoint>
{
    new IntPoint(-50, 50), new IntPoint(350, 50)
};

var polyClip = new PolyClip();
polyClip.AddPath(line, PolyType.ptSubject, false);   //Closed: false
polyClip.AddPath(outer, PolyType.ptClip, true);

var tree = new PolyTree();
polyClip.Execute(ClipType.ctIntersection, tree,
                 PolyFillType.pftNonZero, PolyFillType.pftNonZero);

var segments = PolyClip.OpenPathsFromPolyTree(tree);
//segments holds the portion(s) of the line that fall inside the polygon
```

Passing a `List<List<IntPoint>>` here throws instead:
`Error: PolyTree struct is needed for open path clipping.`

Cleaning self-intersecting input before offsetting it, which is rule five in practice:

```csharp
var bowtie = new List<IntPoint>
{
    new IntPoint(0, 0), new IntPoint(100, 100),
    new IntPoint(100, 0), new IntPoint(0, 100)
};

var simple = PolyClip.SimplifyPolygon(bowtie, PolyFillType.pftNonZero);
var cleaned = PolyClip.CleanPolygons(simple);      //default distance 1.415

var offset = new PolyClipOffset();
offset.AddPaths(cleaned, JoinType.jtMiter, EndType.etClosedPolygon);

var grown = new List<List<IntPoint>>();
offset.Execute(ref grown, 5.0);
```

## Pitfalls

- Coordinates are integers. Scale floating-point input up before converting, and scale results back
  down afterwards. Forgetting this collapses your geometry to a handful of lattice points and
  produces an empty or nonsense result. Coordinate magnitudes must not exceed `hiRange`, and results
  are only guaranteed free of rounding artifacts within `loRange`.
- Call `Clear()` between operations. An instance retains its paths otherwise, and the next `Execute`
  silently includes them. This is the most common bug in code that uses this library.
- For a union, add the new geometry as `ptClip` and the existing geometry as `ptSubject`.
- `PolyClipOffset.Execute` takes its solution parameter by ref. Passing an already-populated list
  does not append - the list is cleared and replaced. The same is true of `PolyClip.Execute`, which
  clears its (non-ref) solution container.
- Open paths require a `PolyTree` solution. The `List<List<IntPoint>>` overloads throw when any open
  subject path was added.
- Range validation is asymmetric. `PolyClipBase.AddPath` rejects coordinates beyond `hiRange`, but
  `PolyClipOffset.AddPath` performs no equivalent range check of its own; validate offset input
  yourself if it may be out of range.
- `AddPath` returns `false` for degenerate input instead of throwing. If you ignore the return value,
  a mistyped path vanishes from the result.
- Do not repeat the first vertex at the end of a closed path. The library closes rings implicitly; a
  repeated vertex is stripped as a duplicate at best and skews `CleanPolygon` at worst.
- `PolyClipException` is not public. `catch (PolyClipException)` will not compile in your code -
  catch `System.Exception`.
- `IntRect` fields are lower-case (`left`, `top`, `right`, `bottom`) and `PolyNode.Childs` is spelled
  without the "r". Both are the actual public names, not typos to be corrected.
- `PolyClip.Area` is signed. Comparing it directly to an expected magnitude fails for clockwise
  input; use `Math.Abs`.
- `PolyNode.Contour` hands back the engine's live list, not a copy. Mutating it while walking the
  tree corrupts the walk.
- An offset that shrinks a shape out of existence returns an empty solution rather than throwing.
  Check `Count` before indexing.
- The default fill rule on the single-fill-type `Execute` overloads is `pftEvenOdd`, which ignores
  orientation. If you are relying on winding to distinguish holes from outer rings, pass
  `pftNonZero` explicitly.
- Offsetting assumes simple, non-self-intersecting input. Feed self-intersecting geometry through
  `PolyClip.SimplifyPolygons` first or the offset result will be wrong in ways that are hard to
  spot.
- `MiterLimit` and `ArcTolerance` are set on the instance, not per path, and they survive `Clear()`.
  Reset them explicitly if a later operation needs different values.
- Neither engine is thread-safe. Give each thread its own instance. The static helpers on `PolyClip`
  hold no shared mutable state - the ones that need an engine (`SimplifyPolygon(s)`, `MinkowskiSum`,
  `MinkowskiDiff`) allocate a private `PolyClip` per call - so they are safe to call concurrently.

> [!TIP]
> For speed, keep coordinates inside `loRange` and choose the smallest scale factor that preserves
> the precision you need; leave `StrictlySimple` off unless you need the guarantee; reuse one
> `PolyClip` (or `PolyClipOffset`) across many operations with `Clear()` between them; batch with
> `AddPaths` rather than looping over `AddPath`; prefer `jtMiter` or `jtSquare` over `jtRound` when
> the join style is not visually important, because round joins emit many more vertices; ask for a
> `PolyTree` only when you need the nesting; and pre-size a `List<IntPoint>` with the capacity
> constructor when you know the vertex count, since `IntPoint` is a struct and paths are contiguous
> value storage. Cost scales with the number of edges and with the number of intersections between
> them - roughly O((n + k) log n) for n edges and k intersections - so pre-reducing vertex counts
> with `CleanPolygons` before a heavy clip usually pays for itself, though not on already-minimal
> geometry.

One scaling subtlety worth repeating: `ArcTolerance` is measured in the same integer units as your
coordinates, so after scaling up by 1,000 the default 0.25 produces 1,000x more segments than it
would unscaled. Scale `ArcTolerance` along with your coordinates, and raise it when offsetting with
`jtRound` if the arcs are finer than your output device can show.

## Documentation and source

The repository ships no samples, no demo applications and no tools: it contains exactly one packable
project and one test project. The test suite is the executable specification for everything on this
page, with one behavior per test, all named for the behavior they lock down - `PolyClipTests.cs` for
the four boolean operations, the four filling rules, the constructor options and every static
helper; `PolyClipBaseTests.cs` for path acceptance, rejection, coordinate range validation, `Clear()`
semantics and `GetBounds`; `PolyClipOffsetTests.cs` for each join and end style, `MiterLimit` and
`ArcTolerance`, the by-ref overloads and reuse after `Clear`; `PolyTreeTests.cs` for nesting, hole
detection and tree walking; and `IntPointTests.cs`, `IntRectTests.cs` and `DoublePointTests.cs` for
the value types. `PolygonFactory.cs` is not a test - it is the shared geometry helper the suite
builds its squares, rectangles, reversed squares and self-intersecting bow-ties with, and it is a
compact worked example of constructing paths. The suite is deterministic and needs no test-data
files, no environment variables, no opt-in switches and no network access, and it writes nothing
into the working tree.

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.PolygonTools/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.PolygonTools/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.PolygonTools/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.PolygonTools.Tests](https://github.com/ellisnet/CodeBrix.PolygonTools/tree/main/tests/CodeBrix.PolygonTools.Tests) |
| Library source | [src/CodeBrix.PolygonTools](https://github.com/ellisnet/CodeBrix.PolygonTools/tree/main/src/CodeBrix.PolygonTools) |

Run the suite from the repository root:

```bash
dotnet test CodeBrix.PolygonTools.slnx
```

## License

CodeBrix.PolygonTools is licensed under the MIT License, and portions of it remain subject to the
Boost Software License 1.0; the MIT license is also named in the package ID
(`CodeBrix.PolygonTools.MitLicenseForever`). Both license texts ship inside the package and sit at
the repository root, as
[LICENSE](https://github.com/ellisnet/CodeBrix.PolygonTools/blob/main/LICENSE) and
[license-boost.txt](https://github.com/ellisnet/CodeBrix.PolygonTools/blob/main/license-boost.txt).
The package sets `PackageRequireLicenseAcceptance`, so a restore asks you to accept the license. For
the provenance and licensing of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.PolygonTools/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Imaging.Drawing](CodeBrix.Imaging.Drawing.md) - a fully managed 2D drawing engine, for
  when coordinate lists have to become pixels
- [CodeBrix.SvgParse](CodeBrix.SvgParse.md) - the SVG document object model, when the geometry
  arrives as or leaves as SVG
- [Graphics, media and vision](../platform/09-graphics-media-and-vision.md) - where 2D work fits in a
  CodeBrix.Platform application
- [ellisnet/CodeBrix.PolygonTools on GitHub](https://github.com/ellisnet/CodeBrix.PolygonTools) - source and tests
