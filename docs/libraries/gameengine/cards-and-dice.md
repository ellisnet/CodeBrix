<sub>[CodeBrix](../../../README.md) › [Libraries](../README.md) › [CodeBrix.Platform.GameEngine](../CodeBrix.Platform.GameEngine.md) › Cards and dice</sub>

# Cards and dice

**The cards and dice add-on gives a [CodeBrix.Platform.GameEngine](../CodeBrix.Platform.GameEngine.md)
game cards, decks, piles, dice and an animated tabletop, with the artwork built in, so a card game, a dice
game or a board-style game is mostly its own rules.** Decks of any size, hands and piles, n-sided and
symbolic dice, shuffling, dealer-style sequential dealing, flipping, rolling, selection and drag-and-drop
all animate on the table, while every outcome is decided once, up front, by the logic. The logical
objects - `Deck`, `CardPile`, `Die` - are ordinary classes that work and unit-test without a table, an
engine or a display. One package carries two assemblies, and the second embeds traditional and simple
playing cards, historical tarot cards, card backs, dice, symbols and optional sound effects, so a game
ships no loose artwork at all.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Platform.GameEngine](https://github.com/ellisnet/CodeBrix.Platform.GameEngine) |
| **Package** | [`CodeBrix.Platform.GameEngine.CardsAndDice.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.GameEngine.CardsAndDice.MitLicenseForever) - the `CodeBrix.Platform.GameEngine.CardsAndDice` and `CodeBrix.Platform.GameEngine.CardsAndDice.Assets` assemblies in one package |
| **License** | MIT for the code; the embedded artwork and sounds are CC0, public domain or original artwork under the package's MIT license. See [License](#license) |
| **Requires** | .NET 10 or later. The engine package arrives automatically |
| **Use it from** | A CodeBrix.Platform game in the scene-pipeline hosting mode; the logical classes from any .NET 10 code, tests included |
| **Platforms** | All six CodeBrix.Platform heads |

## What it does

- One call puts a table on an engine view: `Engine.Instance.UseCardsAndDice(host, view, bounds)`. It
  hooks pause-safe engine time, drawing and mouse input, and disposes the table with the engine.
- Cards of any kind. A `CardDefinition` names a card - key, display name, face and back artwork, value
  and optional data - and a `Deck` makes physical `Card` instances from definitions, any number of them,
  with any number of copies. Every card has its own `Id`, origin deck, current pile, face state, rotation
  and selection, and several decks can share a table and mix in one pile.
- Built-in decks: `BuiltInDecks.PlayingCards` in a traditional or a simple theme, with or without
  jokers, and `BuiltInDecks.Tarot`, the traced historical Rider-Waite-Smith faces.
- Custom card faces: `CardComposer.Compose` builds a card face as SVG from a bundled symbol, a title, a
  value and a description, and `RegisterSvg` puts any SVG artwork - a face or a back - on the table under a
  key of the game's choosing.
- Piles with the operations card games need: draw, add, collect, cut and an unbiased shuffle, with
  ownership always transferred and never inferred from the artwork.
- Six layouts for an area of the table - `Stack`, `Row`, `Fan`, `Grid`, `Circle` and `Manual`, the last
  placing each card at a center and an angle of the game's choosing.
- Animation that never decides anything: a split, riffle and square shuffle timed to its sound, deals,
  flips, rotations and dice that tumble and settle on a result already chosen.
- Dealer-style deals: `DealRoundRobin` deals one card to each player in turn, `DealTo` tosses one card
  to a pose on a free board, and both queue behind a shuffle with no game timers, background tasks or
  asynchronous callbacks.
- Dice: a traditional six-sided die with ivory faces and dark pips, numbered dice of any side count from
  two up, symbolic and weighted dice from custom faces, held dice, and percentile dice.
- Input: click, right-click to inspect, drag with the grab offset kept, drop rules per area, selection
  in a single or a multiple mode, and click-to-hold dice.
- Artwork preparation off the engine thread with progress, so a large deck is rasterized while the table
  keeps drawing.
- Optional sounds through the engine's own audio system, opt-in through `SoundEnabled`.
- An asset catalog, `AssetCatalog`, that lists every embedded file with its category, source, license and
  hashes.

## When to use it

Reach for it for anything played with cards or dice: a solitaire or a trick-taking game, a deck-builder,
a dice game, a fortune table, or a board game whose turns are a roll and a choice of cards. The add-on
owns the table - layout, motion, picking, dragging and the artwork - and the game keeps the rules. The
cleanest shape keeps those rules in a library of their own with no engine types in it, and lets a session
object turn each rules event into a table action;
[GoddessTempleDiscovery](#the-showcase-goddesstemplediscovery) is built that way.

What it deliberately leaves to the game: the rules and the scoring; the inspection UI shown when a card
is right-clicked; touch and controller input, which a game forwards to the table's `Pointer` method
itself; and 3D dice - the dice are animated 2D visuals, not rigid-body physics.

## Getting started

Add the package to the shared game library, next to the engine package:

```bash
dotnet add package CodeBrix.Platform.GameEngine.CardsAndDice.MitLicenseForever
```

There is no separate assets package, no content files, no extraction step and no download: the artwork
is embedded in the package's second assembly, and one package reference brings both.

The namespaces carry no license suffix:

```csharp
using CodeBrix.Platform.GameEngine.CardsAndDice;          // UseCardsAndDice (engine extension)
using CodeBrix.Platform.GameEngine.CardsAndDice.Cards;    // CardDefinition, Card, CardPile, Deck,
                                                          //   BuiltInDecks, CardComposer
using CodeBrix.Platform.GameEngine.CardsAndDice.Dice;     // Die, DieFace
using CodeBrix.Platform.GameEngine.CardsAndDice.Layout;   // CardLayout, CardLayouts, CardPose,
                                                          //   OccupiedDropRule
using CodeBrix.Platform.GameEngine.CardsAndDice.Table;    // CardsAndDiceTable, TableArea, TableDie,
                                                          //   CardSelectionMode, DealStyle,
                                                          //   DealAnimation, DealSequence,
                                                          //   ArtworkPreparation
using CodeBrix.Platform.GameEngine.CardsAndDice.Assets;   // AssetCatalog, AssetEntry
using System.Drawing;                                      // Rectangle (table bounds)
using System.Numerics;                                     // Vector2 (positions)
using SkiaSharp;                                           // SKRect (area bounds)
```

Rules code and tests that never touch the engine need only `.Cards`, `.Dice` and `.Layout`.

A table goes on a view once the engine, the scene, the render host and the mouse adapter exist. In a
`CodeBrixGameHost`, the mouse adapter is wired before the content hooks run, so `CreateDirectDrawings` is
the place:

```csharp
// Adapted from src/CodeBrix.Platform.GameEngine.CardsAndDice/AGENT-README.txt
public sealed class TableGameHost : CodeBrixGameHost
{
    private CardsAndDiceTable _table = null!;

    public TableGameHost(GameSurfaceCanvas canvas) : base(canvas) { }

    protected override Scene CreateInitialScene()
    {
        var scene = new Scene();
        scene.AddPixelLayer(1280, 800);
        return scene;
    }

    protected override void CreateInitialViews()
        => RenderSurface.Host.ViewManager.ConfigureSingleFullView();

    protected override void CreateDirectDrawings()
    {
        _table = Engine.UseCardsAndDice(RenderSurface.Host,
            RenderSurface.Host.ViewManager.Views[0], new Rectangle(0, 0, 1280, 800));

        var deck = BuiltInDecks.PlayingCards();
        var hand = new CardPile("Hand");
        _table.AddArea(deck.DrawPile, new SKRect(30, 200, 230, 530), CardLayout.Stack);
        _table.AddArea(hand, new SKRect(270, 200, 1100, 530), CardLayout.Fan);
        _table.AddDie(new Die(20), new Vector2(650, 650));

        deck.Shuffle();
        _table.Deal(deck, hand, 5);
        _table.Roll();
        _table.CardClicked += card => card.IsSelected = !card.IsSelected;
    }
}
```

Pin the canvas to the table's size with `canvas.SetRenderResolution(1280, 800)` before the host is
created, as the engine page's [Getting started](../CodeBrix.Platform.GameEngine.md#getting-started)
shows, so the table is letterboxed into any window. The table disposes itself when the engine is
disposed.

## Key concepts

### Logic first, animation second

Every outcome - the order of a shuffled pile, which card is dealt where, a die's face - is decided once,
by the logic, at the moment it is asked for; the animation then plays that outcome and consumes no
randomness. `ReducedMotion` completes motion without changing results, and `AnimationSpeed` changes
presentation only. That is why the same seeded game gives the same results with or without a table, and
why rules tests need no engine.

`Deck` and `CardsAndDiceTable` both take an optional seed, and `Deck.Random` and `table.Random` are
ordinary `System.Random` for repeatable games and tests - not gambling-grade randomness.

### Decks, piles and cards

A `CardPile`'s `Cards` are top-first and read-only. `Add` transfers ownership, `Draw` removes the top card
or returns null when empty, `Collect` takes every card from another pile, `Cut` moves the top N under the
rest, `Peek` looks at the top, and `Shuffle` is an unbiased Fisher-Yates shuffle. A `Deck` owns a
`DrawPile` and a `DiscardPile`; `Deck.Draw` and `Deck.Deal` place cards in any target pile, `Deal` returns
however many remain and never silently recycles the discards, `Discard` checks that the card belongs to
that deck, and `Reset` gathers only that deck's cards, even out of mixed hands and discards.

Definition keys may repeat - a deck of twelve identical "Fire" cards is fine - because a physical card's
identity is its `Id`, never its artwork or rank. There is no fixed deck size, an empty deck is valid, and
`copies` makes repeated packs.

### The table and its areas

`AddArea(pile, bounds, layout)` displays a pile and returns a `TableArea`, whose `Bounds`, `Layout`,
`CardWidth` and `CardHeight` stay adjustable. Bounds are placement bounds, not a clipping rectangle, so
choose sizes that fit the scene. `AcceptsDrops` and `CanDrop` control what may be dropped there; a
`SingleCard` area chooses what an occupied drop does through `OccupiedRule` - `Reject`, `Stack`, `Swap`,
or `Displace` into `DisplacementPile`. In a `Manual` area, `ManualPoses` maps card ids to a `CardPose`
(a center and an angle), and a card dropped there keeps the released position and angle.

`AddDie(die, center, size)` puts a die on the table. `Shuffle`, `Deal`, `Flip`, `Rotate` and `Roll` are
the table's animated actions on the logical objects.

### Input, selection and dragging

The table handles the mouse itself once attached. A click raises `CardClicked`, a right-click raises
`InspectRequested`, and clicking a die toggles its `IsHeld` and raises `DieClicked`. Selection and
dragging need only settings:

```csharp
// From src/CodeBrix.Platform.GameEngine.CardsAndDice/AGENT-README.txt
table.SelectionMode = CardSelectionMode.Multiple; // or Single
table.CanSelect = card => card.Pile == p1.Pile;    // optional game rule
table.CanDrag = card => card.Pile != deck.DrawPile;
```

Dragging is on by default and keeps the grab offset. `CardDropped` fires after a successful transfer,
`CancelDrag` returns the card without transferring it, `InputEnabled = false` silences the table while a
modal UI is up, and `DragEnabled = false` keeps clicks working. `None`, the default selection mode, leaves
`CardClicked` entirely to the game. A custom touch or controller adapter calls
`Pointer(point, pressed, released, inspect)` in table coordinates.

### Shuffling, dealing and the animation clock

A table shuffle splits the pile into two packets, riffles them together, squares the stack and puts it
down, over `CardsAndDiceTable.ShuffleDurationSeconds` - the length of the embedded shuffle sound - in real
seconds whatever `AnimationSpeed` says, so the sound and the motion finish together. `ReducedMotion`
skips both the wait and the long sound.

`AnimationsCompleted` fires when the whole table settles; it is not a per-card callback. `IsAnimating`
and `IsDealing` report what is in flight. `table.Deal(deck, pile, count)` is an immediate logical batch
with simultaneous motion; for dealer-style cadence use the queued deals:

- `DealRoundRobin(deck, players, cardsPerPlayer, faceUp, animation)` snapshots the player order and
  queues the rounds, one card per player in turn.
- `DealTo(deck, area, pose, faceUp, animation)` queues one card, and with a pose lands it at an arbitrary
  center and angle in a `Manual` area.

Both wait for a shuffle and for earlier queued deals, draw the actual top card at each departure, hide
the face in flight and flip it after landing unless `faceUp` is false, and raise `CardDealt` after each
landing and reveal. Both return a `DealSequence` whose `Cards`, `IsComplete` and `IsCanceled` report
progress. `DealAnimation` chooses `Slide`, `Arc` (the default) or `Toss`, with `Duration`, `Interval` and
`ArcHeight`. `CancelDeals`, `Clear` and `Dispose` cancel pending work, leaving launched cards where they
landed and the rest in the deck.

### Dice

`Die.Traditional()` is a six-sided die with ivory SVG faces and dark pips that tumbles through dotted faces
and settles on its result. `new Die(n)` is a numbered die of `n` sides, for any `n` of two or more;
standard d4, d6, d8, d10, d12 and d20 artwork is supplied and other side counts use generic numbered
artwork. `new Die(faces)` with custom `DieFace` values, labels and optional SVG keys makes symbolic dice,
or weighted dice by repeating a face. `table.Roll()` rolls every die that is not held and raises
`DiceRolled`; `Die.Result` is null until a die has been rolled. `Die.PercentileTens()`,
`Die.PercentileUnits()` and `Die.PercentileResult(tens, units)` give 01 to 100, with 00 and 0 read as 100.

### Artwork: built-in, composed and registered

Every card, back, die and symbol is addressed by an artwork key. The bundled ones come from the embedded
catalog - for example `backs/royal.svg`, `backs/celestial.svg`, `dice/pips/1.svg` and
`symbols/original/fire.svg` - and `AssetCatalog.All`, `Search`, `Open` and `ReadSvg` list and read them;
keys are case-sensitive, and the categories are playing cards, tarot, backs, dice, symbols and sounds.
List `AssetCatalog.All` for the current set rather than relying on a fixed count.

A game's own artwork is registered on the table under a key of its own and then used exactly like a
bundled key, for faces and backs alike:

```csharp
// From src/CodeBrix.Platform.GameEngine.CardsAndDice/AGENT-README.txt
var svg = CardComposer.Compose("Fire", "symbols/original/fire.svg", 6,
    "An elemental token", accent: "#e69a70");
table.RegisterSvg("my/fire", svg);
var custom = new Deck(new[] {
    new CardDefinition("fire", "Fire", "my/fire", value: 6)
}, copies: 12);

table.RegisterSvg("my/back", yourBackSvg);
var playing = BuiltInDecks.PlayingCards(back: "my/back", jokers: true);
var tarot = BuiltInDecks.Tarot(back: "my/back");
```

`RegisterSvg` takes SVG text or a caller-owned stream and parses and rasterizes it at once, so bad art
fails during setup rather than inside a frame. Supply trusted local artwork. Omit `back` to get the royal
back for custom and playing cards and the celestial back for tarot.

### Preparing artwork

Cards are rasterized lazily, at up to 480 by 840 pixels per card and 144 pixels wide per symbol, and the
cache lives as long as the table; `Clear` removes placements, not cached art. Shuffles, queued deals,
batch deals and flips prepare the cards they are about to show before they start, so first-use SVG work
never lands in the middle of a reveal. For a large deck, prepare ahead:

- `table.BeginPrepareCards(deck.Cards)`, called on the engine thread, returns an `ArtworkPreparation` with
  `Progress`, `Completed`, `Total`, `IsComplete`, `IsCanceled` and `Error`. Only the isolated SVG
  rendering runs on a worker; the table installs the images during `Update`, so keep updating and drawing
  while it loads, wait for `IsComplete` with no error before shuffling or dealing, and `Cancel` it on a
  mode change.
- `table.PrepareCards(deck.Cards)` is the synchronous form for a blocking loading phase.

### Running a table yourself

For headless rules tests or custom rendering, construct `new CardsAndDiceTable(seed)` directly, call
`Update(seconds)` and `Draw(drawList)` each frame, and publish that engine draw list yourself; positions
in it are centers. `CompleteAnimations()` finishes every motion at once, which suits tests and
reduced-motion workflows. The `CardsAndDiceDemo` sample drives its table this way, from the engine's
`BeforeFrameRender` event with time read from `TotalSecondsEngineRunning`.

## Examples

A dealer table: three players, a shuffle, then three rounds dealt in turn, with the middle card of each
hand dealt face down so a click reveals it:

```csharp
// Adapted from src/CodeBrix.Platform.GameEngine.CardsAndDice/AGENT-README.txt
var deck = BuiltInDecks.PlayingCards();
table.AddArea(deck.DrawPile, new SKRect(565, 240, 715, 440), CardLayout.Stack);
var p1 = table.AddArea(new CardPile("Player 1"), new SKRect(50, 200, 400, 500));
var p2 = table.AddArea(new CardPile("Player 2"), new SKRect(850, 200, 1200, 500));
var players = new[] { p1, p2 };

table.Shuffle(deck.DrawPile);                               // the deals wait for it
table.DealRoundRobin(deck, players, cardsPerPlayer: 1, faceUp: true);
table.DealRoundRobin(deck, players, cardsPerPlayer: 1, faceUp: false);
table.DealRoundRobin(deck, players, cardsPerPlayer: 1, faceUp: true);

table.CardClicked += card =>
{
    if (!card.IsFaceUp && card.Pile != deck.DrawPile) table.Flip(card);
};
```

A card tossed onto a free board at an exact position and angle:

```csharp
// From src/CodeBrix.Platform.GameEngine.CardsAndDice/AGENT-README.txt
var board = table.AddArea(new CardPile("Board"),
    new SKRect(20, 170, 1260, 650), CardLayout.Manual);
table.DealTo(deck, board, new CardPose(new Vector2(700, 350), 25),
    animation: new DealAnimation { Style = DealStyle.Toss, Duration = .65 });
```

A set of dice to roll, hold and roll again:

```csharp
// Adapted from samples/CardsAndDiceDemo
for (int i = 0; i < 5; i++)
    table.AddDie(Die.Traditional(), new Vector2(250 + i * 185, 380), 115);

table.Roll();                                  // clicking a die holds it
// ... the player holds some dice, then:
table.Roll();                                  // held dice are skipped
int total = table.Dice.Sum(d => d.Die.Result?.Value ?? 0);
```

Rules tested with no table at all, through a seeded deck:

```csharp
var deck = BuiltInDecks.PlayingCards(seed: 2718);
var hand = new CardPile("Hand");
deck.Shuffle();
deck.Deal(5, hand);
int score = hand.Cards.Sum(c => c.Definition.Value);   // the same on every run
```

## Pitfalls

- **Mutate the table on the engine thread, or before the engine starts.** Table events raised by the
  mouse arrive on the engine thread already.
- **`UseCardsAndDice` needs the mouse adapter.** It subscribes to the engine's mouse poller, so the host's
  input adapters must exist first - `CodeBrixGameHost` wires them before its content hooks run.
- **Never dispose the images `Image(key)` returns.** The table owns its cached `SKImage`s.
- **An artwork key can be registered once.** `RegisterSvg` throws `ArgumentException` for a key already
  registered or already used by a bundled asset; a game that may register the same art twice keeps its
  own set of registered keys.
- **A pile can have one area, and a die can be placed once.** `AddArea` and `AddDie` throw otherwise.
- **`AnimationsCompleted` is table-wide.** It fires when everything settles, not when one card lands; use
  `CardDealt` for each landing of a queued deal.
- **The shuffle runs in real seconds.** `AnimationSpeed` does not shorten it, so the sound and the motion
  end together. Wait for it - or let the queued deals wait for it - before dealing.
- **Deals do not recycle the discards.** When the deck runs out, the remaining queued requests simply
  finish; reshuffle the discards yourself.
- **Queued deals need displayed piles.** The source and the destination must both be areas on the table,
  and an explicit pose needs a `Manual` area. Scripted deals bypass the drag-and-drop acceptance rules.
- **Layout bounds do not clip.** Choose bounds and card sizes that fit the scene.
- **Tarot values are illustrative.** The built-in tarot deck numbers its cards 1 to 13 in catalog order,
  with no traditional meaning; define your own `CardDefinition`s for your rules.
- **The historical tarot faces cost more memory than the simple art.** They are traced vector paths that
  keep the original paper and print texture. Prepare every deck in advance when several animate together.
- **The randomness is `System.Random`.** Fine for games and repeatable tests, not for gambling.
- **Building the add-on from source needs both projects.** Consuming the source through a
  `ProjectReference` rather than the package, reference the `CodeBrix.Platform.GameEngine.CardsAndDice.Assets`
  project explicitly too when the game uses `AssetCatalog`; package consumers need only the one package.

## Samples and reference applications

| Name | What it demonstrates | Where |
| --- | --- | --- |
| CardsAndDiceDemo | The reference consumer for this package: a playable score game across several tables - traditional and simple playing cards, historical tarot, custom elemental decks composed with `CardComposer`, dice sets, and a dealer table using `DealRoundRobin` and `DealTo` - plus a paged gallery of the embedded artwork with a large inspector. It shows shuffle-then-deal without blocking the engine thread, background artwork preparation with a progress bar, selection, dragging between areas and onto a free board, click-to-hold dice, and a layout that reflows on resize. All of its artwork comes from the package, so the sample has no asset folder | [`samples/CardsAndDiceDemo`](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/tree/main/samples/CardsAndDiceDemo) |

Run it from the engine repository root, swapping the head for `CardsAndDiceDemo.Win32Skia` or
`CardsAndDiceDemo.MacOS` on the other platforms:

```bash
dotnet run --project samples/CardsAndDiceDemo/src/CardsAndDiceDemo.LinuxX11
```

The shared game is `src/libs/CardsAndDiceDemo.Game/FortuneTableGame.cs`. Setting `CARDSDICE_WALKTHROUGH=1`
runs an unattended pass over every mode on a graphical desktop.

### The showcase: GoddessTempleDiscovery

[GoddessTempleDiscovery](https://github.com/ellisnet/CodeBrix.Samples/tree/main/GoddessTempleDiscovery), in
[CodeBrix.Samples](https://github.com/ellisnet/CodeBrix.Samples), is the showcase application for this
add-on: a complete, educational cards-and-dice board game built against the published packages. Two to four
fictional expeditions, each seat a human or a computer, dig at Warka, the ancient Uruk - the city of Holy
Inanna and Her Eanna precinct - through the twelve real pre-war seasons of the German excavations. Each
season a Season card tells what really happened that winter and applies one effect to every team; each turn
a team rolls two dice, its work crews, and spends them to dig trenches in a row of face-down sites, recruit
specialists, study tablets or survey the row, then publishes three or more finds as a numbered Preliminary
Report. Doubles draw a Favor of the Goddess. Every card carries a short, sourced text about the real
buildings, finds and people, and every discovery opens as an EXTRA! page of a period newspaper. When the
last season ends the reports are scored and the game is bound into a Field Journal PDF. The teams and
specialist roles are fictional; the history on the cards is not.

Its solution keeps three libraries apart, and the split is the lesson:

| Library | What it holds | Packages |
| --- | --- | --- |
| `GoddessTempleDiscovery.Rules` | The rules engine (`Engine/GameEngine*.cs`): setup, seasons, phases, legal actions, events, scoring and the Field Journal's entries; the content of every deck as data (`Content/Catalog*.cs`); and the computer opponents (`Brains/ComputerBrain.cs`) | None - no engine, add-on or UI types |
| `GoddessTempleDiscovery.Game` | The game host over the page's canvas, the table session, the card-face composer, the heads-up display, the Field Journal PDF and the stored settings | This add-on, the engine, [CodeBrix.PdfDocuments](../CodeBrix.PdfDocuments.md), the [AppSettings](../../platform/add-ins/AppSettings.md) add-in |
| `GoddessTempleDiscovery.Assets` | The original vector art (buildings, objects, symbols, deities, scenes, traced plates, chrome and Art Deco pieces) and the embedded Merriweather and Noto Sans Cuneiform fonts | None |

**The rules decide, the table shows.** The engine is deterministic for a seed and an action sequence: one
seeded generator draws every shuffle and every die, a state hash pins whole games in golden unit tests,
and the computer teams choose only from `GameEngine.LegalActions`, so they never make an illegal move.
`TableSession` owns one game on the table. It builds one add-on `Deck` per rules deck, with the rules card
carried in `CardDefinition.Data` and every card on the bundled celestial back:

```csharp
_siteDeck = BuildDeck("Site deck", AllDiscoveries(state), d => new CardDefinition(d.Id, d.Title, CardFaceComposer.FaceKey(d.Id), CelestialBack) { Data = d });
_tabletDeck = BuildDeck("Tablet deck", AllTablets(state), t => new CardDefinition(t.Id, t.Title, CardFaceComposer.FaceKey(t.Id), CelestialBack) { Data = t });
```

It then lays out `TableArea`s for the site deck, the five single-card site slots, the four expedition
slots, the tablet and favor decks, a `Fan` for the hand, a `Row` for the specialist shelf and a stack of
reports per team. Only the current team's hand and shelf are on the table; the other teams' cards wait in
piles with no area and change places when a turn starts. Every engine event becomes a queued table action -
a deal, a flip, a roll, a gather - drained one step at a time, and the session never decides a rule:
actions go to the engine, and only the events that come back move cards.

**Card faces composed as SVG at run time.** `CardFaceComposer` builds every face - discoveries, tablets,
specialists, favors and seasons - as a self-contained 250 x 400 SVG: the Art Deco frame from the Assets
library, the kind's colour band, the card's art inlined through `SvgInliner`, and every line of text set as
vector paths by `VectorText`, which turns the embedded Merriweather and Noto Sans Cuneiform glyphs into SVG
path data, so a face needs no font. Composing is pure work, so the host runs it on a worker in
`LoadAssets` and registers the results with `RegisterSvg` a few faces per frame, inside a time budget, while
a progress bar fills:

```csharp
var budget = Stopwatch.StartNew();
while (_registeredFaces < _pendingFaces.Count && budget.Elapsed.TotalMilliseconds < RegistrationBudgetMs)
{
    var key = _pendingFaces[_registeredFaces++];
    _artwork.Register(key, _faces.Faces[key]);
}
```

Before each new game's first deal the host also calls `BeginPrepareCards` over every card on the table, so
the backs and anything not yet cached are rasterized before anything moves.

**Dice that land on the rules engine's result.** The rules engine rolls. The table's traditional dice
tumble through `table.Roll()`, and each die's logical face is then set to the engine's value by rolling it
once more through `Die.Roll(Random)` with a `Random` that can only answer that face:

```csharp
public sealed class FixedFaceRandom : Random
{
    ...
    public override int Next(int maxValue) => maxValue <= 0 ? 0 : Math.Min(_index, maxValue - 1);
```

A third die joins only for the turn after the Favor that grants one; the dice and cards report clicks
through the table's `CardClicked`, `DieClicked` and `InspectRequested` events, which the host turns into
legal actions.

**Computer players at animation pace.** When the current seat is a computer, the host waits until the
table is idle and no inspector is open, pauses between actions by the table's animation speed, and asks the
brain for one legal action. The brain's jitter comes from a `Random` seeded with the game's seed, so a
seeded game plays its computer turns the same way. The three temperaments - Surveyor, Deep Digger and
Scholar - are one decision procedure with three weightings, and double as the opponents' difficulty.

```csharp
var pause = _table.ReducedMotion ? 0.1 : ComputerPauseSeconds / Math.Max(0.25, _table.AnimationSpeed);
...
Act(ComputerBrain.Choose(session.Engine, team.Temperament, _brainRandom));
```

**XAML chrome over the game canvas.** The page is a `GameSurfaceCanvas` under XAML panes: title, setup,
prologue, the newspaper inspector, the Field Journal, settings, scores, How to Play, History and Credits.
The game host talks back to `MainViewModel` only through the `IInspectorBridge` and `ISessionBridge`
interfaces, on the UI thread, and the view model asks the page for a save path through `IJournalFileBridge`.
Sound, reduced motion and animation speed are stored with the AppSettings add-in and applied straight to the
table's `SoundEnabled`, `ReducedMotion` and `AnimationSpeed`; the last setup's seats are stored beside them.

**The Field Journal PDF.** `JournalPdfBuilder` binds the game's editions - a cover, a front page per season,
an EXTRA! page per discovery, the tablets, favors and reports as notices, the final results and the sources -
into a PDF with [CodeBrix.PdfDocuments](../CodeBrix.PdfDocuments.md), and `SvgToPdf` draws the same SVG art
the table shows onto the pages as vector shapes.

Its heads are Linux X11, Linux Wayland, Windows Win32-Skia and macOS. Each library has its own xunit.v3
test project, and `tests/GoddessTempleDiscovery.PlayTests` drives the real page; the application's
[`DESIGN.md`](https://github.com/ellisnet/CodeBrix.Samples/blob/main/GoddessTempleDiscovery/DESIGN.md)
gives every number the rules use. Run it from the application folder, swapping the head on the other
platforms:

```bash
dotnet run --project src/GoddessTempleDiscovery.LinuxX11
```

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide for the cards and dice package (ships inside the package too) | [src/CodeBrix.Platform.GameEngine.CardsAndDice/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/blob/main/src/CodeBrix.Platform.GameEngine.CardsAndDice/AGENT-README.txt) |
| Cards and dice tests - ownership, seeded rules, layouts, input and animation, queued dealing, artwork preparation, every embedded asset's hash, and rendering every embedded SVG | [tests/CodeBrix.Platform.GameEngine.CardsAndDice.Tests](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/tree/main/tests/CodeBrix.Platform.GameEngine.CardsAndDice.Tests) |
| The demo's tables, controls and walkthrough | [samples/CardsAndDiceDemo/README.md](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/blob/main/samples/CardsAndDiceDemo/README.md) |
| The scripts that maintain the embedded assets and their provenance notices (maintainer material) | [tools/cards-and-dice](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/tree/main/tools/cards-and-dice) |

## License

The cards and dice package (`CodeBrix.Platform.GameEngine.CardsAndDice.MitLicenseForever`) is MIT. Every
embedded asset is CC0, public domain, or original artwork covered by the package's MIT license: Kenney's
board-game and fantasy symbols and sounds and the traditional playing cards are CC0, the historical tarot
sources and the Star of Ishtar medallion the bundled backs share are public domain, and the original
elemental and geometric artwork is covered by the MIT license. There is therefore no required
artist-credit screen; retain the normal MIT notices. The exact provenance, permission and
hashes of every embedded file are in
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.GameEngine/blob/main/THIRD-PARTY-NOTICES.txt),
which also ships inside the package.

---

**Where to go next**

- [CodeBrix.Platform.GameEngine](../CodeBrix.Platform.GameEngine.md) - hosting, the engine thread, draw lists and input underneath the table
- [Kenney assets](kenney-assets.md) - more art and sound, straight from downloaded Kenney zips
- [Generated music](generated-music.md) - endless music under the table
- [Gamepads](gamepads.md) - controllers, for a game that forwards them to the table's `Pointer`
