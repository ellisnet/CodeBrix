<sub>[CodeBrix](../../../README.md) › [Libraries](../README.md) › [CodeBrix.Platform.GameEngine](../CodeBrix.Platform.GameEngine.md) › Kenney assets</sub>

# Kenney assets

**The Kenney assets add-on lets a [CodeBrix.Platform.GameEngine](../CodeBrix.Platform.GameEngine.md)
game use the asset bundles downloaded from [kenney.nl](https://kenney.nl) exactly as they were
downloaded: one call registers the zips, and from then on every picture, sprite atlas, sound, font, SVG,
Tiled map and glTF model in them has a key the engine turns into an ordinary engine object.** Nothing
is unpacked, renamed or repacked, there is no asset pipeline to build, and a game addresses a Kenney
asset exactly the way it addresses art it made itself. Kenney publishes game art, audio, fonts and 3D
models as zip bundles under CC0; the package is the reader and carries no art of its own.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Platform.GameEngine](https://github.com/ellisnet/CodeBrix.Platform.GameEngine) |
| **Package** | [`CodeBrix.Platform.GameEngine.KenneyAssets.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.GameEngine.KenneyAssets.MitLicenseForever) |
| **License** | MIT; the Kenney bundles a game reads are CC0 and are the game's own content. See [License](#license) |
| **Requires** | .NET 10 or later. The engine package and [`CodeBrix.Graphics3D.Gltf2.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Graphics3D.Gltf2.MitLicenseForever), the glTF reader ([its page](../CodeBrix.Graphics3D.Gltf2.md)), arrive automatically. No native prerequisite and nothing to install on the target machine |
| **Use it from** | A CodeBrix.Platform game, in either hosting mode |
| **Platforms** | All six CodeBrix.Platform heads, and headless in tests |

## What it does

- Reads a Kenney bundle where it lies - a downloaded zip, a folder extracted from one, or a folder
  holding many extracted bundles - read-only, with nothing written beside it.
- One call attaches it: `Engine.Instance.UseKenneyAssets(paths)` builds a provider, catalogs every
  bundle and registers the provider with the engine's asset-provider registry at
  `Engine.Managers.AssetProviders`.
- Gives every asset one key, such as `kenney:pixel-platformer/Tiles/tile_0001`, built from the pack's
  name and the asset's path inside the bundle.
- Turns a key into the engine object the asset is for, registered under that same key:
  - a loose image or an SVG into a `Tilesheet` whose picture is `sheet[0, 0]`;
  - a sprite atlas (the bundle's TextureAtlas XML beside its sheet image) into one `Tilesheet` with a
    region per frame, addressed by the frame's name;
  - a `.ogg`, `.wav`, `.mp3` or `.flac` sound into an `AudioResource`;
  - a `.ttf` or `.otf` font into an `SKTypeface` in the engine's `FontManager`;
  - an orthogonal Tiled map into scene layers of a scene the game owns, with its object layers handed
    back as data;
  - a glTF model either into plain model data or into sprite frames pre-rendered from several camera
    directions, for a 2D game that wants 3D art.
- Lists everything else a bundle holds - documents, nested archives, Flash exports, other engines'
  project files, the model formats it does not read - so a browser can show a bundle honestly, and
  opens any of them as a raw stream.
- Makes credits easy: every pack carries a ready-made credit line, and `CreditLines` gives one per
  registered pack.
- Helps a game keep its hand-written keys right: `CheckKeys` checks them against the catalog without
  loading anything, `GetKeyCatalog` writes a readable listing of every key with the frame names inside
  each atlas, and a key that fails to load names its closest real keys in a "Did you mean" message.
- Needs no GPU: zip reading, decoding and the model renderer are managed code, so a pre-rendered model
  sheet works headless.

## When to use it

Reach for it when a game's art, sound or lettering comes from Kenney packs - a prototype built on a
handful of downloaded zips today, or a finished game that ships them. It is also the first
implementation of the engine's asset-provider contract, so it is the worked example to read before
writing a provider for some other third-party layout; [Assets](../CodeBrix.Platform.GameEngine.md#assets)
on the engine page describes that seam.

What it deliberately does not do:

- It ships no assets. A game brings the bundles it uses.
- It never writes: nothing is extracted to disk or cached on disk, and it never reaches the network -
  it reads files the game already has.
- It imports orthogonal, finite Tiled maps only. Isometric, hexagonal, staggered and infinite maps, and
  maps built on a tile set that holds one image per tile, are refused with the feature named; the
  individual pictures of such a tile set are still addressable as ordinary images.
- No per-tile collision shapes (a tile's collision box is a rectangle) and no per-layer opacity; both
  are reported as warnings.
- It spawns nothing from a map's object layers and never creates sprites: it hands back layers, sheets
  and data, and the game builds its own objects.
- Only the glTF forms of a model (`.glb`, `.gltf`) are read; `.fbx`, `.obj`, `.mtl`, `.dae` and `.stl`
  are listed and refused. There is no runtime 3D pipeline in the engine: a model is either data the game
  renders itself or sprite frames rendered once at load time.
- Nested archives inside a bundle, such as Kenney's web-font zips, are listed and never opened.
- It does not register itself. Nothing happens until a game calls `UseKenneyAssets` or
  `RegisterKenneyAssets`.

## Getting started

Add the package to the shared game library, next to the engine package:

```bash
dotnet add package CodeBrix.Platform.GameEngine.KenneyAssets.MitLicenseForever
```

Ship the bundles beside the executable, as the files they were downloaded as:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>MyGame</RootNamespace>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.Platform.ApacheLicenseForever" />
    <PackageReference Include="CodeBrix.Platform.Fonts.OpenSans.ApacheLicenseForever" />
    <PackageReference Include="CodeBrix.Platform.GameEngine.MitLicenseForever" />
    <PackageReference Include="CodeBrix.Platform.GameEngine.KenneyAssets.MitLicenseForever" />
  </ItemGroup>
  <ItemGroup>
    <!-- The bundles the game ships, beside the executable. -->
    <None Include="assets\*.zip" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
</Project>
```

Then register them in the game host's `LoadAssets` override, which runs once the engine exists and
before `LoadTilesheets`, and load by key from there on:

```csharp
// Adapted from src/CodeBrix.Platform.GameEngine.KenneyAssets/AGENT-README.txt
using CodeBrix.Platform.GameEngine;
using CodeBrix.Platform.GameEngine.Assets.Providers;
using CodeBrix.Platform.GameEngine.Drawing.Tilesheets;
using CodeBrix.Platform.GameEngine.Host.Hosting;
using CodeBrix.Platform.GameEngine.Host.Rendering;
using CodeBrix.Platform.GameEngine.KenneyAssets;
using CodeBrix.Platform.GameEngine.Scenes;
using System;
using System.Drawing;
using System.IO;

namespace MyGame;

public sealed class MyGameHost : CodeBrixGameHost
{
    private const string TilesKey =
        "kenney:pixel-platformer/Tilemap/tilemap-backgrounds_packed";

    public MyGameHost(GameSurfaceCanvas renderSurface) : base(renderSurface) { }

    protected override void LoadAssets()
    {
        Engine.Instance.UseKenneyAssets(
            Path.Combine(AppContext.BaseDirectory, "assets",
                "kenney_pixel-platformer.zip"));
    }

    protected override void LoadTilesheets()
    {
        // The asset key IS the tilesheet's registry key from here on.
        Tilesheet tiles = Engine.Instance.Managers.AssetProviders.LoadTilesheet(
            TilesKey,
            new TilesheetMaterializeOptions { TileSize = new Size(24, 24) });
    }

    protected override Scene CreateInitialScene()
    {
        Scene scene = new Scene();
        Tilesheet tiles = TilesheetRegistry.Instance[TilesKey];
        SceneLayer layer = scene.AddLayer(30, 17, 24, 24);
        layer[0, 16]!.CurrentFrame = tiles["grid", 0, 0];
        return scene;
    }
}
```

Address the bundles from `AppContext.BaseDirectory`, as above, so the path does not depend on the
process working directory. A game in the software-rendered mode registers in `OnLoadContent` instead.

The package's own namespaces are few, because almost everything a game touches is the engine's:

```csharp
using CodeBrix.Platform.GameEngine.KenneyAssets;          // UseKenneyAssets, RegisterKenneyAssets,
                                                          //   KenneyGameAssetProvider,
                                                          //   KenneyAssetsOptions, KenneyPackSummary,
                                                          //   KenneyAssetsRegistration,
                                                          //   KenneySourceResult, KenneyKeyCheck
using CodeBrix.Platform.GameEngine.KenneyAssets.Sources;  // KenneyAssetProperties
using CodeBrix.Platform.GameEngine.KenneyAssets.Parsing;  // TiledMapParseException

// From the engine package:
using CodeBrix.Platform.GameEngine.Assets.Providers;      // GameAssetDescriptor, GameAssetQuery,
                                                          //   GameAssetKind, the materialize and
                                                          //   import options, TiledMapImport
using CodeBrix.Platform.GameEngine.Assets.Models;         // GameModel, GameModelAnimationClip
```

## Key concepts

### Two layers: the catalog and materializing

Registering a bundle builds a catalog that answers "what is in here?" without opening a single picture:
it reads the archive's file listing plus the few small XML documents that decide what an asset is, so a
catalog of a whole Kenney collection is built in well under a second. Materializing turns one catalogued
asset into the engine object it is for and registers it under its own key, in the engine's own
registries - `TilesheetRegistry`, `AudioResourceManager`, `FontManager` - which are the only cache.
Register every bundle at start-up, and materialize when a level needs an asset.

Load through the registry, `Engine.Instance.Managers.AssetProviders`, rather than through the provider:
`LoadTilesheet`, `LoadAudio`, `LoadFont`, `ImportTiledMap`, `LoadModel` and `LoadModelAnimation` resolve
the provider from the key's prefix, so game code does not have to hold the provider at all. `Describe`
and `TryDescribe` on the registry search every registered provider at once.

### Asset keys

Every asset is one string, `<providerId>:<pack-slug>/<path-inside-the-pack>`:

```text
kenney:puzzle-pack/PNG/Double/ballBlue
kenney:puzzle-pack/Spritesheet/spritesheet_default
kenney:sci-fi-sounds/Audio/laserSmall_000
kenney:blocky-characters/Models/GLB format/character-a
kenney:blocky-characters/Models/FBX format/character-a.fbx
```

- The provider id is `kenney` unless the game chose another (`KenneyAssetsOptions.ProviderId`).
- The pack slug comes from the title line of the pack's license file, lower-cased and hyphenated, so
  `Puzzle Pack (1.1)` gives `puzzle-pack` - not from the zip's file name, which is used only when a pack
  carries no license file. A second pack wanting the same slug gets `-2`, `-3` and so on.
- The path keeps the bundle's own folders, which is what tells `PNG/Default/ballBlue` from
  `PNG/Double/ballBlue`.
- The extension is dropped from an asset the provider can materialize and kept on one that is listed
  only, because a Kenney model ships as one `.glb`, `.fbx`, `.obj` and `.mtl` that would otherwise want
  one key.
- Lookup is case-insensitive, and the key is also the registry key of the materialized object.

A sprite atlas is one asset, keyed by its XML document; its sheet image is not listed separately. A
Tiled tile set is reached through the map that uses it. Read real keys from `Describe`, from
`GetKeyCatalog` or from the "Did you mean" message, rather than guessing them.

### Addressing what comes back

| What was loaded | How to address it |
| --- | --- |
| A loose image or an SVG | `sheet[0, 0]` is the picture. `TileSize` in the options adds a `grid` region over the same image: `sheet["grid", column, row]` |
| A sprite atlas | `sheet["ballBlue", 0, 0]` - one single-tile region per frame, named after the frame with its image extension stripped. The atlas sheet's `default` region holds no tiles |
| A Tiled tile set | `sheet["tiles", column, row]`, plus a pre-baked region per flip combination the map uses (`tiles-fh`, `tiles-fv`, `tiles-fd` and so on). The sheet's key is `<map key>#<tile set name>` |
| A pre-rendered model | `sheet["walk", frame, direction]` - one region per rendered animation, columns are frames and rows are camera directions - and the rest pose at `sheet[ModelRenderOptions.RestPoseRegionName, 0, direction]` |

### The first materialization of a key wins

A second load of a key the engine already holds returns that object and ignores the second call's
options - the volume and pan of a sound, the tile size, raster size and model-render settings of a
tilesheet - because re-registering would dispose the object the game is using and every `Frame` handed
out from it. To get a variant, give it a key of its own with `TilesheetMaterializeOptions.RegisterAs`:
one asset, two keys, two objects. An asset the game unloads from the engine's registry is materialized
again on the next call.

### Fonts

A font becomes an `SKTypeface` registered in `FontManager` under its key. Identify it by that key, or by
`FontManager.GetFamilyName(key)`, which reads the family name from the font file and is the same on
every platform - never by `SKTypeface.FamilyName`, which the platform's native font back end reports and
which differs between Windows and Linux for the same file. Kenney's input-prompt fonts are icon fonts
with keyboard, mouse, gamepad and touch glyphs and no basic Latin at all: they draw a "press this button"
prompt and never a word.

### Tiled maps

`ImportTiledMap(key, scene, options)` adds one `SceneLayer` per tile layer to a scene the caller owns,
registers one `Tilesheet` per tile set and returns a `TiledMapImport` with the `Layers` (bottom-most
first), `Tilesheets`, `ObjectGroups`, `MapSizePx`, `TileSize` and the import's own `Warnings`.
`TiledMapImportOptions` carries `ZOrderBase`, a default `Parallax`, a `CollisionSelector`, a
`CollisionProfileName` applied to every imported layer, a `LayerFilter` and `ImportObjectLayers`. With no
selector, a tile collides when it or its tile set carries a `collision` custom property of `blocking`,
`true` or `trigger`.

Object layers are data, never scene objects: each `TiledObject` has its `Id`, `Name`, `Type`, unrotated
`Bounds`, `Rotation` and `Properties`, and spawning anything from them is the game's job. A map the
catalog already knows it cannot import carries the reason in its descriptor's `tiledMapError` property,
which is the cheap way to ask before trying.

### 3D models: data or pre-rendered sprites

`LoadModel` returns a `GameModel` - meshes as flat float arrays with `uint` indices, materials with their
base-color texture as RGBA8888 bytes, bounds and the names of its animations - that belongs to the caller
and is registered nowhere. Nothing is baked unless asked for: `ModelMaterializeOptions` names the clips,
or `LoadModelAnimation(key, name, framesPerSecond)` adds one later without re-reading the model, and a
clip's frames always line up with the model's meshes.

For a 2D game, `LoadTilesheet` on a model key with `TilesheetMaterializeOptions.ModelRender` renders the
model once, at load time, through a software rasterizer, into a sheet whose layout is fixed: one region
per animation plus `rest`, columns are frames in playback order, rows are camera directions. With the
default eight directions, row 0 faces the viewer, row 2 is screen-left, row 4 faces away and row 6 is
screen-right. Every cell is exactly `FrameSize`, one common fit is shared by every direction and frame so
the model never appears to breathe as it turns, and a sheet larger than 8192 pixels in either dimension
is refused with a message saying which setting to reduce.

### Registering more than once, and a second catalog

Calling `UseKenneyAssets` again adds sources to the provider already serving that id, which is how
downloadable content or a mod folder joins the catalog later. It adds every path it is given, so the same
zip passed twice gets a second slug and a second set of keys. When registration can run more than once -
a loading screen that can be re-entered, a game and its tests sharing one engine - call
`RegisterKenneyAssets`, which skips a path already registered and returns a `KenneyAssetsRegistration`
saying per path whether it was `Read`, `AlreadyRegistered`, `Missing` or `Unreadable`. A set of assets
that must stay apart takes a provider id of its own through `KenneyAssetsOptions`, whose
`RecursiveFolders` reads a folder that holds pack folders, the shape of Kenney's all-in-one download.

By default a missing or damaged bundle costs the game those assets, not its start-up: it is written to the
engine log and to the provider's `Warnings`, and the other sources still register. Set
`IgnoreUnreadableSources = false` while developing to make it throw instead.

### Disposal and threads

Registering a provider hands it to the engine, which disposes it on `Unregister`, `Clear` or shutdown; a
game does not dispose it. After disposal the catalog still answers, and the tilesheets, sounds and
typefaces already handed to the engine keep working. Everything public is safe to call from more than one
thread, and adding sources while another thread looks up a key is safe, but materializing is load-time
work and does not belong in a per-frame path.

## Examples

Register, and log what each pack delivered and what could not be done:

```csharp
// From src/CodeBrix.Platform.GameEngine.KenneyAssets/AGENT-README.txt
using CodeBrix.Platform.GameEngine;
using CodeBrix.Platform.GameEngine.Assets.Providers;
using CodeBrix.Platform.GameEngine.KenneyAssets;

string assets = Path.Combine(AppContext.BaseDirectory, "assets");

KenneyGameAssetProvider kenney = Engine.Instance.UseKenneyAssets(
    Path.Combine(assets, "kenney_pixel-platformer.zip"),
    Path.Combine(assets, "kenney_sci-fi-sounds.zip"));

foreach (KenneyPackSummary pack in kenney.Packs)
{
    Engine.Logger.LogInformation(
        "{Pack}: {Assets} asset(s), credit as {Credit}",
        pack.Slug, pack.AssetCount, pack.CreditLine);
}

foreach (string warning in kenney.Warnings)
{
    Engine.Logger.LogWarning("Kenney assets: {Warning}", warning);
}
```

An atlas sprite, a sound and a heads-up display in a Kenney font:

```csharp
// From src/CodeBrix.Platform.GameEngine.KenneyAssets/AGENT-README.txt
GameAssetProviderRegistry providers = Engine.Instance.Managers.AssetProviders;

Tilesheet atlas = providers.LoadTilesheet(
    "kenney:puzzle-pack/Spritesheet/spritesheet_default");
Sprite ball = SpriteManager.Instance.CreateSprite(layer, atlas["ballBlue", 0, 0]);

AudioResource pickUp = providers.LoadAudio(
    "kenney:sci-fi-sounds/Audio/laserSmall_000", volume: 0.7f);

SKTypeface face = providers.LoadFont("kenney:simulated-bundle/Fonts/Kenney Space");
TextBlock score = new TextBlock(host, view, new Rectangle(8, 8, 240, 32), "score")
    .SetFont(face, 24f);
```

A whole level from a Tiled map, with the game deciding what the map's objects are:

```csharp
// From src/CodeBrix.Platform.GameEngine.KenneyAssets/AGENT-README.txt
Scene scene = new Scene();

TiledMapImport import = Engine.Instance.Managers.AssetProviders.ImportTiledMap(
    "kenney:simulated-bundle/Tiled/tilemap-example-a",
    scene,
    new TiledMapImportOptions
    {
        ZOrderBase = 0,
        CollisionProfileName = CollisionProfileNames.World,
    });

// The map says where things belong; the game decides what they are.
foreach (TiledObjectGroup group in import.ObjectGroups)
{
    foreach (TiledObject spawn in group.Objects)
    {
        if (spawn.Type == "player")
        {
            PlacePlayer(spawn.Bounds.X + group.Offset.X,
                        spawn.Bounds.Y + group.Offset.Y);
        }
    }
}

host.Bind(scene);
```

A 3D character pre-rendered into sprite frames, and one direction's walk cycle built from them:

```csharp
// From src/CodeBrix.Platform.GameEngine.KenneyAssets/AGENT-README.txt
Tilesheet hero = providers.LoadTilesheet(
    "kenney:blocky-characters/Models/GLB format/character-a",
    new TilesheetMaterializeOptions
    {
        ModelRender = new ModelRenderOptions
        {
            FrameSize = new Size(96, 96),
            Directions = 8,
            AnimationNames = ["idle", "walk"],
            AnimationFramesPerSecond = 12,
        },
    });

// Columns are frames, rows are directions: this is row 0's walk cycle.
FrameSequence walkFacingViewer = new FrameSequence();
TilesheetRegion walk = hero.GetRegion("walk")!;
for (int column = 0; column < walk.Columns; column++)
{
    walkFacingViewer.AddFrame(hero, "walk", column, 0);
}
walkFacingViewer.SequenceCycleType = CycleType.Repeating;
Cycle cycle = new Cycle(walkFacingViewer, 1.0 / 12.0, "hero-walk-0");
```

Registration that may run more than once, with a report per zip:

```csharp
// From src/CodeBrix.Platform.GameEngine.KenneyAssets/AGENT-README.txt
string folder = Path.Combine(AppContext.BaseDirectory, "assets", "kenney");
string[] zips = ["kenney_planets.zip", "kenney_sci-fi-sounds.zip"];

KenneyAssetsRegistration registration = Engine.Instance.RegisterKenneyAssets(
    [.. zips.Select(zip => Path.Combine(folder, zip))]);

if (registration.Packs.Count == 0)
{
    throw new InvalidOperationException($"No Kenney pack could be read from '{folder}'.");
}

foreach (KenneySourceResult missing in registration.Unavailable)
{
    Engine.Logger.LogWarning("Kenney: {File} is missing; its assets are too.", missing.SourcePath);
}
```

A test that proves every key the game wrote down still resolves, without loading anything:

```csharp
// From src/CodeBrix.Platform.GameEngine.KenneyAssets/AGENT-README.txt
KenneyKeyCheck check = kenney.CheckKeys(MyAssetKeys.All);

check.MissingKeys.Should().BeEmpty();
check[MyAssetKeys.MainAtlas].Kind.Should().Be(GameAssetKind.SpriteAtlas);
```

`Console.Write(kenney.GetKeyCatalog())` prints every key grouped by pack, with the frame names under
each sprite atlas, to copy exact spellings from; it is plain text for reading, not a format to parse.

## Pitfalls

- **An atlas frame is not `sheet[0, 0]`.** On a sprite-atlas sheet the `default` region deliberately
  holds no tiles; address frames by name. On a loose image `sheet[0, 0]` is the picture. The two look
  alike and behave differently.
- **The `default` region of a Tiled tile-set sheet and of a model sheet is meaningless.** Use `tiles`
  and its flip variants for a map, and the animation's own name, or `rest`, for a model.
- **A materializable key has no extension; a listed-only key does.** Asking for `.../ballBlue.png`
  fails, and asking for `.../character-a` when you meant the `.fbx` silently gives you the glTF model.
- **Options on the second load of a key are ignored.** Use `RegisterAs` for a variant.
- **Pass the key the provider gave you.** Lookup is case-insensitive but the tilesheet registry is
  ordinal, so two spellings of one key would register two tilesheets for one asset.
- **Importing a map twice into one scene adds its layers twice.** Sheets are reused, layers are not;
  import into a fresh scene.
- **Never draw text with a Kenney input-prompt font.** It is an icon font with no letters, and the text
  comes out blank with no error anywhere.
- **Never branch on `SKTypeface.FamilyName`.** It is platform-specific; use the key or
  `FontManager.GetFamilyName(key)`.
- **A pack's slug comes from its license title, not its file name.** Read slugs from `Packs` or from a
  descriptor, and keep them in constants a test holds.
- **The same pack registered twice gets a `-2` slug.** A zip and the folder extracted from it, or the
  same path passed to `UseKenneyAssets` twice, give a second set of keys; `RegisterKenneyAssets` skips a
  path it already holds.
- **An unreadable bundle is silent by default.** It is in `Warnings` and the log, not an exception.
- **Model data is registered nowhere.** `LoadModel` builds a new model each call; hold the one you got.
- **A model sheet reaches the 8192-pixel limit sooner than it looks.** Eight directions of a two-second
  clip at 24 frames per second in 128-pixel cells is already 6144 by 1024.
- **An SVG that declares only a `viewBox`, with no width and height, can rasterize to a fully transparent
  bitmap.** That is a property of the document; add width and height to the art.
- **Ship the zips as `None` items from a non-head project, not `Content`.** The CodeBrix.Platform build
  targets re-root a non-head project's copy-to-output `Content` under a folder named for the assembly,
  so every head would carry a second copy of each zip.

## Samples and reference applications

| Name | What it demonstrates | Where |
| --- | --- | --- |
| KenneyAssetsDemo | The worked example of this package, end to end: two Kenney bundles registered with one `UseKenneyAssets` call, a Tiled map imported into scene layers, a glTF character pre-rendered into eight-direction sprite frames and walked with the keyboard, atlas sprites as collectible gems, a pick-up sound, HUD text in Kenney fonts, a rasterized SVG badge, and a start-up report with one console line per loading step | [`samples/KenneyAssetsDemo`](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/tree/main/samples/KenneyAssetsDemo) |
| BrixInvaders | A complete arcade game whose every picture, sound and letter comes from Kenney zips shipped as downloaded, registered through `RegisterKenneyAssets`, with every key kept in one class and proved in a test through `CheckKeys` | [CodeBrix.Samples/BrixInvaders](https://github.com/ellisnet/CodeBrix.Samples/tree/main/BrixInvaders) |
| KenneyAssetBrowser | A desktop browser for looking inside downloaded Kenney zips before building anything from them. It reads the zips with [CodeBrix.Compression](../CodeBrix.Compression.md) rather than with this package | [CodeBrix.Samples/KenneyAssetBrowser](https://github.com/ellisnet/CodeBrix.Samples/tree/main/KenneyAssetBrowser) |

The BrixInvaders recipe is
[Register downloaded Kenney zip files with RegisterKenneyAssets and load sprites, sounds and fonts from them](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-GameEngine.md#register-downloaded-kenney-zip-files-with-registerkenneyassets-and-load-sprites-sounds-and-fonts-from-them),
listed with the other [game-engine blueprints](../../samples/blueprints.md#hosting-a-game-engine).

Run the demo from the repository root, swapping the head project for `KenneyAssetsDemo.Win32Skia` or
`KenneyAssetsDemo.MacOS` on the other platforms:

```bash
dotnet run --project samples/KenneyAssetsDemo/src/KenneyAssetsDemo.LinuxX11/KenneyAssetsDemo.LinuxX11.csproj
```

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide for the Kenney assets package (ships inside the package too) | [src/CodeBrix.Platform.GameEngine.KenneyAssets/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/blob/main/src/CodeBrix.Platform.GameEngine.KenneyAssets/AGENT-README.txt) |
| Kenney assets tests - registration through the engine's registry, the key scheme, every per-kind route, the map import and the model routes | [tests/CodeBrix.Platform.GameEngine.KenneyAssets.Tests](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/tree/main/tests/CodeBrix.Platform.GameEngine.KenneyAssets.Tests) |
| The demo's controls, start-up report and the provenance of its bundles | [samples/KenneyAssetsDemo/README.md](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/blob/main/samples/KenneyAssetsDemo/README.md) |

The test project also carries an opt-in scan over a whole Kenney collection, skipped unless the
`KENNEY_ALLIN1_DIR` environment variable points at one.

## License

The Kenney assets package (`CodeBrix.Platform.GameEngine.KenneyAssets.MitLicenseForever`) is MIT, and its
license covers its code only. Kenney's bundles are released under the
[Creative Commons Zero (CC0 1.0 Universal)](http://creativecommons.org/publicdomain/zero/1.0/) public
domain dedication: they may be used in personal, educational and commercial projects and need no
attribution. Crediting the source is good practice nonetheless, and the provider makes it a one-liner:
every pack's license title line is in `KenneyPackSummary.LicenseTitle`, and `CreditLines` gives
`"<license title> - Kenney (CC0)"` per registered pack for a credits screen built from what is actually
loaded. Each bundle also carries Kenney's own `License.txt`, which is listed as an asset and can be shown
verbatim through `OpenRaw`. For what the package incorporates, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/blob/main/THIRD-PARTY-NOTICES.txt).

---

**Where to go next**

- [CodeBrix.Platform.GameEngine](../CodeBrix.Platform.GameEngine.md) - tilesheets, sprites, scenes and the asset-provider seam this package fills
- [Generated music](generated-music.md) - endless music for a game whose effects come from a Kenney sound pack
- [Cards and dice](cards-and-dice.md) - the add-on that embeds its own card, dice and symbol art, Kenney's board-game symbols among it
- [Gamepads](gamepads.md) - the SDL2 add-on, for the controllers Kenney's input-prompt fonts draw
