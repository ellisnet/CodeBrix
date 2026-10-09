<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Graphics3D.Gltf2</sub>

# CodeBrix.Graphics3D.Gltf2

**CodeBrix.Graphics3D.Gltf2 is a fully managed library for reading, writing and building Khronos glTF
2.0 3D models - the JSON form (`.gltf`, with satellite or embedded buffers and images) and the binary
form (`.glb`).** One assembly holds three layers: a complete glTF 2.0 document object model with
loading, saving and validation; builders for authoring meshes, materials and scenes from code; and
runtime helpers that turn a loaded model into what a renderer needs - decoded vertex streams, rigid,
skinned and GPU-instanced transforms, and animation playback. It draws nothing itself. You reach for it
from any .NET 10 application that loads or produces 3D models, and from a CodeBrix.Platform
application that draws them through the [Graphics3DGL add-in](../platform/add-ins/Graphics3DGL.md) or
reads them through the GameEngine's [Kenney assets](gameengine/kenney-assets.md) add-on.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Graphics3D.Gltf2](https://github.com/ellisnet/CodeBrix.Graphics3D.Gltf2) |
| **Packages** | [`CodeBrix.Graphics3D.Gltf2.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Graphics3D.Gltf2.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later. No NuGet dependencies - JSON handling uses the in-box `System.Text.Json` - and no native library |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Any operating system .NET 10 runs on. The assembly is marked trim- and AOT-compatible; it uses unsafe code internally, and consumers do not need `<AllowUnsafeBlocks>` |

## What it does

- Loads and saves glTF 2.0 models as `.gltf` and `.glb`, from files, streams, byte arrays, in-memory
  dictionaries and zip archives; `Load` detects which form a file is from its content.
- Models the whole glTF 2.0 document: scenes, nodes, meshes, primitives, accessors, buffers, buffer
  views, materials, textures, images, samplers, skins, cameras, animations and punctual lights, with
  both a logical view (every object in document order) and a visual one (the scene graph as rendered).
- Supports the common Khronos and vendor extensions, registered automatically: the PBR material
  extensions (clearcoat, transmission, volume, sheen, specular, IOR, iridescence, anisotropy,
  dispersion, emissive strength, diffuse transmission, unlit), texture transforms, KTX2, WebP, DDS and
  ASTC textures, punctual lights, mesh quantization, GPU mesh instancing, animation pointers, node
  visibility and XMP metadata.
- Validates on load, in `Strict`, `TryFix` and `Skip` modes, with exceptions that name the offending
  object and the generator that wrote the file.
- Authors models from code with `MeshBuilder`, `MaterialBuilder`, `NodeBuilder` and `SceneBuilder`,
  including skinned meshes, morph targets and animation curves.
- Prepares models for rendering with `SceneTemplate`, lightweight animated `SceneInstance`s, and
  `MeshDecoder`'s renderer-friendly vertex streams.
- Evaluates a scene into world-space triangles, computes bounding boxes and spheres, and exports to
  Wavefront OBJ.

## When to use it

Reach for it whenever a .NET application has to open a glTF or GLB file, inspect it, change it, write
one, or feed one to a renderer. It is the model-data layer, deliberately: drawing is your renderer's
job - the [Graphics3DGL add-in](../platform/add-ins/Graphics3DGL.md) and
[CodeBrix.Platform.OpenGL](CodeBrix.Platform.OpenGL.md) in a CodeBrix.Platform application.

What it does not do:

- Render anything. It prepares data for a renderer.
- Decode or encode image pixels. Images, including KTX2, WebP and DDS, are opaque byte payloads written
  back as they were read.
- Read Draco- or meshopt-compressed geometry; such files cannot be loaded.
- Support `KHR_materials_variants`, `MSFT_lod`, `EXT_lights_image_based`, `KHR_techniques_webgl`, or
  the AGI and Cesium 3D Tiles vendor extensions.
- Read glTF 1.0 or any other 3D format. Wavefront OBJ is export only.
- Download anything. URIs resolve against local files, dictionaries or zip archives only.

## Getting started

```bash
dotnet add package CodeBrix.Graphics3D.Gltf2.MitLicenseForever
```

The package ID carries the license suffix; the namespaces do not. You install
`CodeBrix.Graphics3D.Gltf2.MitLicenseForever` and you write
`using CodeBrix.Graphics3D.Gltf2.Schema2;` for the document model.

A complete console program that loads a model, reports what is in it and re-saves it as GLB:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.Graphics3D.Gltf2.MitLicenseForever" />
  </ItemGroup>
</Project>
```

```csharp
using System;
using CodeBrix.Graphics3D.Gltf2.Schema2;

var model = ModelRoot.Load(args[0]);
Console.WriteLine($"{model.LogicalMeshes.Count} meshes, " +
                  $"{model.LogicalNodes.Count} nodes, " +
                  $"{model.LogicalAnimations.Count} animations");
model.SaveGLB(System.IO.Path.ChangeExtension(args[0], ".converted.glb"));
```

Most programs need `Schema2`, plus `Scenes`, `Geometry`, `Geometry.VertexTypes` and `Materials` to
author models, or `Runtime` and `Transforms` to render them:

| Namespace | What it holds |
| --- | --- |
| `CodeBrix.Graphics3D.Gltf2.Schema2` | `ModelRoot` and the document model; `ReadSettings`, `WriteSettings`, `ReadContext`, `WriteContext`; the authoring and conversion extension methods |
| `CodeBrix.Graphics3D.Gltf2.Scenes` | `SceneBuilder`, `NodeBuilder`, `InstanceBuilder`, `CameraBuilder`, `LightBuilder` |
| `CodeBrix.Graphics3D.Gltf2.Geometry` | `MeshBuilder`, `PrimitiveBuilder`, `VertexBuilder` |
| `CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes` | `VertexPosition`, `VertexPositionNormal`, `VertexTexture1`, `VertexJoints4`, `VertexEmpty` and the other vertex fragments |
| `CodeBrix.Graphics3D.Gltf2.Materials` | `MaterialBuilder`, `KnownChannel`, `ImageBuilder`, `ChannelBuilder` |
| `CodeBrix.Graphics3D.Gltf2.Runtime` | `SceneTemplate`, `SceneInstance`, `MeshDecoder`, `RuntimeOptions` |
| `CodeBrix.Graphics3D.Gltf2.Transforms` | `AffineTransform`, `RigidTransform`, `SkinnedTransform`, `InstancingTransform`, `SparseWeight8` |
| `CodeBrix.Graphics3D.Gltf2.Memory` | `MemoryImage` and the typed accessor arrays |
| `CodeBrix.Graphics3D.Gltf2.Animations` | Curve sampling |
| `CodeBrix.Graphics3D.Gltf2.Validation` | `ValidationMode`, `ModelException` and its subclasses |
| `CodeBrix.Graphics3D.Gltf2.IO` | `ZipReader`, `ZipWriter` |

Vector and matrix types are `System.Numerics` (`Vector2`, `Vector3`, `Vector4`, `Quaternion`,
`Matrix4x4`).

## Key concepts

### The document model

`ModelRoot` is the glTF document. `ModelRoot.Load`, `ReadGLB` and `ParseGLB` read one; `SaveGLB`,
`SaveGLTF`, `Save` (by extension) and `WriteGLB` write one; `ModelRoot.CreateModel()` starts an empty
one. A `ValidationMode` converts implicitly to `ReadSettings`, so `Load(path, ValidationMode.TryFix)`
works. `ReadContext.CreateFromDirectory`, `CreateFromDictionary` and `Create(callback)` control where
satellite files come from, and a `ReadContext` is itself a `ReadSettings`.

There are two ways through a document. The logical collections - `LogicalMeshes`, `LogicalNodes`,
`LogicalMaterials`, `LogicalAnimations` and the rest - list every object as stored. The visual path -
`model.DefaultScene.VisualChildren`, then each node's `VisualChildren` - is the scene graph as rendered.

`WriteSettings.ImageWriting` controls images only: the buffers of a `.gltf` are always written as
satellite `.bin` files, and a `.glb` embeds everything.

### The builders

`MaterialBuilder`, `MeshBuilder` and `SceneBuilder` are the easy way to author. A mesh builder is
generic over three vertex fragments - geometry, material and skinning - so
`MeshBuilder<VertexPositionNormal, VertexTexture1>` is a lit, textured mesh with no skin. Primitives are
grouped by material through `mesh.UsePrimitive(material)`, and a `SceneBuilder` places meshes with
`AddRigidMesh` and `AddSkinnedMesh` and creates the nodes for you. `scene.ToGltf2()` produces a
`ModelRoot`; `SceneBuilderSchema2Settings.WithGpuInstancing` emits GPU instancing instead of duplicate
nodes for many copies of one mesh.

The document model is effectively append-only. To edit a loaded model, convert it with
`ToSceneBuilder()`, change the builder, and write a new model with `ToGltf2()`.

### Preparing a model for rendering

A `SceneTemplate` is created once per model and is immutable and shareable; any number of
`SceneInstance`s come from it, each animating independently through its `Armature`. Enumerating an
instance yields drawables, each with the index of its logical mesh and a transform. Decode the meshes
once with `model.LogicalMeshes.Decode()` and keep the result; one decoded mesh serves every instance.

The transform comes in three kinds, and the order you test them in matters: `InstancingTransform`
derives from `RigidTransform`, so test `InstancingTransform` first, then `SkinnedTransform`, then
`RigidTransform`.

### Errors

Every validation exception derives from `ModelException`: `SchemaException` for malformed JSON or a
schema violation, `LinkException` for broken references between objects, and `DataException` for
invalid buffer or accessor contents. `Load` of a path that does not exist throws `ArgumentException`,
not `FileNotFoundException`; a missing satellite file does throw `FileNotFoundException`.

## Examples

Loading, inspecting and converting, with exporter mistakes repaired on the way in:

```csharp
using System;
using CodeBrix.Graphics3D.Gltf2.Schema2;
using CodeBrix.Graphics3D.Gltf2.Validation;

var model = ModelRoot.Load("model.gltf", ValidationMode.TryFix);

Console.WriteLine($"generator: {model.Asset.Generator}");
foreach (var mesh in model.LogicalMeshes)
{
    foreach (var prim in mesh.Primitives)
    {
        var positions = prim.GetVertexAccessor("POSITION").AsVector3Array();
        Console.WriteLine($"{mesh.Name}: {positions.Count} vertices, " +
                          $"{prim.GetIndices().Count} indices");
    }
}

model.SaveGLB("model.glb");
```

Building a model from code:

```csharp
using System.Numerics;
using CodeBrix.Graphics3D.Gltf2.Geometry;
using CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes;
using CodeBrix.Graphics3D.Gltf2.Materials;
using CodeBrix.Graphics3D.Gltf2.Scenes;

var material = new MaterialBuilder("red")
    .WithDoubleSide(true)
    .WithMetallicRoughnessShader()
    .WithBaseColor(new Vector4(1, 0, 0, 1));

var mesh = new MeshBuilder<VertexPosition>("triangle");
mesh.UsePrimitive(material).AddTriangle(
    new VertexPosition(-10, 0, 0),
    new VertexPosition(10, 0, 0),
    new VertexPosition(0, 10, 0));

var scene = new SceneBuilder();
scene.AddRigidMesh(mesh, Matrix4x4.Identity);

scene.ToGltf2().SaveGLB("triangle.glb");
```

Render-loop data for a game, with two independently animated instances of one model:

```csharp
using System.Numerics;
using CodeBrix.Graphics3D.Gltf2.Runtime;
using CodeBrix.Graphics3D.Gltf2.Schema2;
using CodeBrix.Graphics3D.Gltf2.Transforms;

var model = ModelRoot.Load("character.glb");

// upload once: one GPU mesh per logical mesh
var decoded = model.LogicalMeshes.Decode();

var template = SceneTemplate.Create(model.DefaultScene);
var player = template.CreateInstance();
var enemy = template.CreateInstance();

player.Armature.SetAnimationFrame(0, 0.25f);
enemy.Armature.SetAnimationFrame(0, 1.75f);

foreach (var drawable in player)
{
    var gpuMesh = decoded[drawable.Template.LogicalMeshIndex];

    switch (drawable.Transform)
    {
        case InstancingTransform instancing:
            // draw gpuMesh once per instancing.WorldTransforms[i].WorldMatrix
            break;
        case SkinnedTransform skinned:
            // upload skinned.SkinMatrices as the bone palette
            break;
        case RigidTransform rigid:
            // draw gpuMesh with rigid.WorldMatrix
            break;
    }
}
```

In memory, as bytes, and through a zip archive:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using CodeBrix.Graphics3D.Gltf2.IO;
using CodeBrix.Graphics3D.Gltf2.Schema2;

var model = ModelRoot.Load("model.gltf");

var files = new Dictionary<string, ArraySegment<byte>>();
model.Save("copy.gltf", WriteContext.CreateFromDictionary(files));
var copy = ModelRoot.Load("copy.gltf", ReadContext.CreateFromDictionary(files));

ArraySegment<byte> glb = model.WriteGLB();
var fromBytes = ModelRoot.ParseGLB(glb);

using (var zip = new ZipWriter("models.zip")) { zip.AddModel("model.gltf", model); }
var fromZip = ZipReader.LoadModelFromZip("models.zip");
```

`ZipWriter(path)` creates a new archive and throws `IOException` if the file already exists.

## Using it in a CodeBrix.Platform application

The library has no CodeBrix.Platform dependency; a CodeBrix.Platform application references it like
any other .NET 10 application, parses the model off the UI thread, and hands the data to a renderer.
In the family it is used three ways:

- The GameEngine's [Kenney assets](gameengine/kenney-assets.md) add-on depends on it and brings it in
  automatically, to read the glTF models in a Kenney bundle into model data or pre-rendered sprite
  frames.
- [PolyHavenBrowser](https://github.com/ellisnet/CodeBrix.Samples/tree/main/PolyHavenBrowser) and
  [PolyHavenBrowser_viewer_only](https://github.com/ellisnet/CodeBrix.Samples/tree/main/PolyHavenBrowser_viewer_only)
  in [CodeBrix.Samples](../samples/README.md) load downloaded glTF models with it in their rendering
  libraries and draw them through the [Graphics3DGL add-in](../platform/add-ins/Graphics3DGL.md).
- [KenneyAssetBrowser](https://github.com/ellisnet/CodeBrix.Samples/tree/main/KenneyAssetBrowser)
  previews the 3D models inside Kenney packs the same way.

## Pitfalls

- **Testing `RigidTransform` before `InstancingTransform`.** The instancing case becomes unreachable,
  and the compiler reports CS8120.
- **Expecting `FileNotFoundException` from `Load` for a missing model file.** It throws
  `ArgumentException`; check `File.Exists` first if you need a specific message.
- **Expecting to remove objects from a `ModelRoot`.** Edit through `ToSceneBuilder()` and `ToGltf2()`.
- **Using the specular-glossiness members.** `KnownChannel.Diffuse`,
  `KnownChannel.SpecularGlossiness`, `WithSpecularGlossinessShader()` and `WithPBRSpecularGlossiness()`
  target a deprecated extension, are marked `[Obsolete]` and produce CS0618. Use `BaseColor` with
  `SpecularColor` and `SpecularFactor`.
- **Experimental APIs.** Gaussian-splat primitives and parts of the runtime template API raise
  diagnostics `GLTFRT1000`, `GLTFRT1001` or `GLTFRT1002`; suppress the specific ID only if you accept
  that the API may change.
- **Ambiguous `AlphaMode`.** Both `Materials` and `Schema2` declare it; `MaterialBuilder.WithAlpha`
  takes the `Materials` one, so fully qualify it or alias it when both namespaces are imported.
- **A `using` alias with relative names.** `using VERTEX = VertexBuilder<...>;` must be fully qualified,
  because aliases ignore the other using directives.
- **Expecting `ImageWriting` to embed buffers.** It affects images only; save as `.glb` for one
  self-contained file.
- **Invalid node graphs from some exporters.** A node listed as a scene root that is also a child
  throws `LinkException` under both `Strict` and `TryFix`; `ValidationMode.Skip` loads it, but the
  document is not guaranteed to be consistent.
- **`EvaluateTriangles` in a render loop.** It allocates per triangle and is meant for export and
  inspection; decode meshes once instead.

## Samples and tools in the repository

The repository ships one package and no sample applications, demos or command-line tools. The test
project is the worked example of every feature area.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Test project | Loading and saving, document-model authoring, the scene, mesh and material builders, animation curves, the runtime helpers, transforms, memory and accessors, Wavefront export, and real-world models | [`tests/CodeBrix.Graphics3D.Gltf2.Tests`](https://github.com/ellisnet/CodeBrix.Graphics3D.Gltf2/tree/main/tests/CodeBrix.Graphics3D.Gltf2.Tests) |
| Test assets | A pinned subset of the Khronos glTF sample models, CC0 textures and models, and hand-authored glTF files, each with its license recorded | [`tests/CodeBrix.Graphics3D.Gltf2.Tests/Assets`](https://github.com/ellisnet/CodeBrix.Graphics3D.Gltf2/tree/main/tests/CodeBrix.Graphics3D.Gltf2.Tests/Assets) |

The CodeBrix.Platform applications listed under
[Using it in a CodeBrix.Platform application](#using-it-in-a-codebrixplatform-application) are the
complete consumers.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Graphics3D.Gltf2/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Graphics3D.Gltf2/blob/main/AGENT-README.txt) |
| Tests and other non-package content | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Graphics3D.Gltf2/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Graphics3D.Gltf2/blob/main/README-INDEX.txt) |
| Provenance and licensing of included open source code and test assets | [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Graphics3D.Gltf2/blob/main/THIRD-PARTY-NOTICES.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.Graphics3D.Gltf2.Tests](https://github.com/ellisnet/CodeBrix.Graphics3D.Gltf2/tree/main/tests/CodeBrix.Graphics3D.Gltf2.Tests) |

XML documentation ships alongside the assembly.

## License

CodeBrix.Graphics3D.Gltf2 is licensed under the MIT License, and the license is also named in the
package ID (`CodeBrix.Graphics3D.Gltf2.MitLicenseForever`). License acceptance is required. For the
provenance and licensing of open source code included in this library, and of the test assets in the
repository, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Graphics3D.Gltf2/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [Graphics3DGL add-in](../platform/add-ins/Graphics3DGL.md) - drawing a loaded model with OpenGL in a CodeBrix.Platform page
- [CodeBrix.Platform.OpenGL](CodeBrix.Platform.OpenGL.md) - the managed OpenGL binding underneath
- [Kenney assets](gameengine/kenney-assets.md) - the GameEngine add-on that reads Kenney glTF models through this library
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.Graphics3D.Gltf2 on GitHub](https://github.com/ellisnet/CodeBrix.Graphics3D.Gltf2) - source and tests
