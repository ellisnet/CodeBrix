<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Platform.Fonts.NotoMusic</sub>

# CodeBrix.Platform.Fonts.NotoMusic

**CodeBrix.Platform.Fonts.NotoMusic ships the Noto Music font as a build-time content asset: a
musical-notation symbols face, not a text face.** It carries clefs, noteheads, rests, accidentals,
dynamics, articulations, Byzantine neumes and ancient Greek vocal and instrumental notation, plus a
supporting set of Latin letters, digits and punctuation for notation labels. Reference it alongside
one of the family's text font packages, never instead of one; it is equally usable as a plain
content-files NuGet in any .NET 10 project that wants the font binary.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Platform.Fonts.NotoMusic](https://github.com/ellisnet/CodeBrix.Platform.Fonts.NotoMusic) |
| **Packages** | [`CodeBrix.Platform.Fonts.NotoMusic.OflLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Fonts.NotoMusic.OflLicenseForever) |
| **License** | `OFL-1.1`; see [License](#license) |
| **Requires** | .NET 10 or later; the package has no NuGet dependencies of its own |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Platform-neutral - no native libraries, no OS-specific components |

## What it does

- Ships one font file, `NotoMusic.ttf`: the single face - Regular, upright, Normal stretch, static -
  addressable as `ms-appx:///CodeBrix.Platform.Fonts.NotoMusic/Fonts/NotoMusic.ttf`.
- Ships one `.ttf.manifest` JSON file with a single Normal / 400 / Normal entry pointing at that
  file.
- Ships a `.uprimarker` file that CodeBrix.Platform build pipelines use to discover font asset
  packages.
- Covers the Byzantine, Western, ancient-Greek and miscellaneous-symbols music blocks, plus the Latin
  letters, digits and punctuation a notation label needs.
- Injects nothing into your build. This is the family's structurally simplest font package.

## When to use it

Use it when an application has to put musical symbols on screen: a clef beside a heading, an
accidental inside a sentence, a legend of note values, Byzantine neumes, ancient Greek notation.

It is not a text face, and it must not be an application's default font. The supporting Latin exists
for notation labels - `Allegro`, `8va`, rehearsal letters - not for body text. Pair it with one of
the family's text font packages:
[CodeBrix.Platform.Fonts.OpenSans](CodeBrix.Platform.Fonts.OpenSans.md),
[CodeBrix.Platform.Fonts.Roboto](CodeBrix.Platform.Fonts.Roboto.md),
[CodeBrix.Platform.Fonts.RobotoMono](CodeBrix.Platform.Fonts.RobotoMono.md) or
[CodeBrix.Platform.Fonts.Merriweather](CodeBrix.Platform.Fonts.Merriweather.md).

It also does not render or lay out music. It is a set of glyphs, not a notation engine: no staves are
drawn for you, no beaming, no spacing, no MusicXML or MIDI handling. It ships no variable font, no
italics and no weights other than Regular. It exposes no public managed types, no font-loading helper
and no glyph-metrics API - referencing it gives you a file, not objects. It does not install fonts
into the operating system, and it has no runtime dependency on CodeBrix.Platform.

One thing that is easy to assume and wrong: **there is no Greek or Cyrillic prose coverage in this
font.** The "Greek" here is the ancient-notation block `U+1D200`-`U+1D245`, not the Greek alphabet.

## Getting started

```bash
dotnet add package CodeBrix.Platform.Fonts.NotoMusic.OflLicenseForever
```

There is nothing to `using`. The package exposes no public managed types, so no namespace import is
ever required to consume it. One content URI is the entire addressable surface of the package:

```text
ms-appx:///CodeBrix.Platform.Fonts.NotoMusic/Fonts/NotoMusic.ttf
```

A single music glyph - the G clef:

```xml
<TextBlock Text="&#x1D11E;"
           FontFamily="ms-appx:///CodeBrix.Platform.Fonts.NotoMusic/Fonts/NotoMusic.ttf"
           FontSize="48" />
```

There is no registration call and no MSBuild property. Unlike its siblings, this package injects
nothing into your build - and it needs no `SupportsFontManifest` setting, because its font is always
in the payload.

## Key concepts

### One face, and what follows from it

The package ships one font file and one manifest. `NotoMusic.ttf` is the dash-free name - the slot
the sibling packages fill with a variable font - and it is the static Regular instance itself; the
manifest's weight-400 entry points at it directly.

Two consequences:

- **There is exactly one face.** `FontWeight`, `FontStyle` and `FontStretch` have nothing else to
  resolve to; a request for bold or italic gets whatever synthesis the platform applies, which on a
  notation font usually looks wrong. Leave those properties alone.
- **There is no `NotoMusic-Regular.ttf`.** The dash-free name is the only name; a URI containing a
  dash will not resolve.

There is also no `buildTransitive` `.targets` file. The sibling packages use one to prune their
dash-bearing static instances at consumer-build time on platforms without manifest support; this
package's only font is dash-free and must always be present, so there is nothing to prune. Nothing
this package does can remove its font from your build.

### The manifest

`NotoMusic.ttf.manifest` is a JSON object with a `fonts` array holding exactly one entry:

```json
{
  "font_style":   "Normal",
  "font_weight":  400,
  "font_stretch": "Normal",
  "family_name":  "ms-appx:///CodeBrix.Platform.Fonts.NotoMusic/Fonts/NotoMusic.ttf"
}
```

`family_name` holds the URI of the font file, not a typographic family name - do not be misled by the
member name. There is nothing to choose: any weight, style or stretch request that resolves through
this package lands on the one file.

### Glyph coverage

The musical blocks, as the font's own `cmap` gives them:

```text
U+1D000..U+1D0F5   Byzantine Musical Symbols (the whole
                   assigned range of the block)
U+1D100..U+1D126   Musical Symbols - clefs and staff signs
U+1D129..U+1D1EA   Musical Symbols - the rest of the assigned
                   range, up to and including the Kievan and
                   Persian additions (U+1D127 and U+1D128 are
                   unassigned in Unicode, hence the gap)
U+1D200..U+1D245   Ancient Greek Musical Notation (the whole
                   assigned range of the block)
U+2669..U+266F     Miscellaneous Symbols - the seven music
                   characters
```

Landmarks verified present in the font: `U+1D11E` G CLEF, `U+1D121` C CLEF, `U+1D122` F CLEF,
`U+1D12A` DOUBLE SHARP, `U+1D12B` DOUBLE FLAT, `U+1D13B` WHOLE REST, `U+1D13C` HALF REST, `U+1D13D`
QUARTER REST, `U+1D18F` DYNAMIC PIANO, `U+1D183` ARPEGGIATO UP, `U+1D1DE` KIEVAN C CLEF, `U+1D15D`
WHOLE NOTE, `U+1D15E` HALF NOTE, `U+1D15F` QUARTER NOTE, `U+1D160` EIGHTH NOTE, `U+1D161` SIXTEENTH
NOTE, `U+1D162` THIRTY-SECOND NOTE, `U+1D158` NOTEHEAD BLACK, `U+1D16D` COMBINING AUGMENTATION DOT,
`U+1D1AA` COMBINING DOWN BOW and `U+1D1EA` KORON, plus `U+2669`, `U+266A`, `U+266B`, `U+266C`,
`U+266D`, `U+266E` and `U+266F`.

> [!TIP]
> The `U+266x` seven are the ones that fit in a single UTF-16 `char` and need no surrogate handling.
> Prefer them for simple inline marks such as a sharp or flat next to a note name.

The supporting text characters are Basic Latin `U+0020`-`U+007E` in full, most of Latin-1 Supplement,
most of Latin Extended-A, a handful of Latin Extended-B (`U+0218`-`U+021B`, `U+0237`), modifier
letters `U+02C6`-`U+02DD`, combining diacritics `U+0300`-`U+0328`, `U+1E80`-`U+1E85`, `U+1E9E`,
`U+1EF2`-`U+1EF3`, the common General Punctuation marks - en and em dash, quotes, bullet, ellipsis,
guillemets - `U+20AC` EURO SIGN, `U+2122` TRADE MARK SIGN, `U+2212` MINUS SIGN, and `U+25CC` DOTTED
CIRCLE, the placeholder ring for combining marks.

The CodeBrix family never falls back to a system font, so a codepoint outside the coverage above
renders as `.notdef` rather than borrowing a glyph from the OS - and this font draws a box for
`.notdef`, so a miss shows up as a tofu box rather than vanishing.

### Writing music characters

Every codepoint in the `U+1D0xx`-`U+1D2xx` blocks is astral - above `U+FFFF`. That has two practical
consequences: how you write the character, and how .NET counts it.

In XAML, use an XML numeric character reference in hexadecimal form:

```xml
Text="&#x1D11E;"                       <!-- G clef            -->
Text="&#x1D15F;&#x1D16D;"              <!-- dotted quarter    -->
Text="&#x266F;"                        <!-- sharp sign (BMP)  -->
```

The `&#x....;` form takes the full Unicode scalar value - do not write a surrogate pair as two
references. XAML has no `\u` escape; a backslash escape in a XAML attribute is a literal backslash.
Decimal references work too but are unreadable; prefer hex. Pasting the literal character into a
UTF-8 XAML file also works.

In C#, use the eight-digit `\U` escape - capital U - or build the string from the scalar value:

```csharp
string gClef   = "\U0001D11E";                   // G clef
string sharp   = "\u266F";                       // BMP, 4-digit \u
string quarter = char.ConvertFromUtf32(0x1D15F);  // computed at run time
```

`\U` takes exactly eight hex digits; `\u` takes exactly four and cannot express an astral codepoint
on its own. The surrogate-pair form is equivalent, but the `\U` form is clearer.

What .NET counts is the surprise: `"\U0001D11E".Length` is 2, because an astral character is one
Unicode scalar stored as two UTF-16 `char` values. Never split, truncate or index such a string by
`char` position - `Substring`, `s[i]`, a manual reverse - or you will cut a surrogate pair in half
and produce an unrenderable lone surrogate. Enumerate with `System.Globalization.StringInfo`,
`text.AsSpan().EnumerateRunes()` or `char.ConvertToUtf32`, and build with
`char.ConvertFromUtf32(int)`. A `TextBox` `MaxLength` of 1 cannot hold one of these characters.

### Using it next to a text face

This package supplies no text face and no fallback wiring of its own, and no sibling text package
lists it as a fallback. Put the music glyphs on screen by switching `FontFamily` on the `<Run>` that
carries them. A `<Run>` inherits everything from the parent `TextBlock` except what it overrides, so
the surrounding prose keeps the text font and only the symbol run uses Noto Music. The same applies
to any element that takes a `FontFamily`: `TextBlock`, `Run`, `TextBox`, `Button` content, and so on.

Two things a per-run switch cannot do:

- **Optical matching.** Noto Music's symbols are not designed to match a particular text face's
  weight or x-height; expect to tune `FontSize`, and sometimes a small vertical offset, rather than
  assuming the glyph will sit correctly at the text's size.
- **Automatic selection.** Nothing makes a text font reach this font for a codepoint it lacks unless
  the platform's fallback chain is configured to do so, and this package does not configure it.
  Assume you must name the font on the run.

## Examples

A short notation legend - clefs, note values and accidentals, each group in its own block:

```xml
<StackPanel Orientation="Horizontal" Spacing="12">
  <TextBlock FontFamily="ms-appx:///CodeBrix.Platform.Fonts.NotoMusic/Fonts/NotoMusic.ttf"
             FontSize="32"
             Text="&#x1D11E; &#x1D121; &#x1D122;" />
  <TextBlock FontFamily="ms-appx:///CodeBrix.Platform.Fonts.NotoMusic/Fonts/NotoMusic.ttf"
             FontSize="32"
             Text="&#x1D15D; &#x1D15E; &#x1D15F; &#x1D160;" />
  <TextBlock FontFamily="ms-appx:///CodeBrix.Platform.Fonts.NotoMusic/Fonts/NotoMusic.ttf"
             FontSize="32"
             Text="&#x266D; &#x266E; &#x266F;" />
</StackPanel>
```

Symbols inside prose, with a per-run `FontFamily` switch against a text face. Only the runs carrying
glyphs name the music font.

```xml
<TextBlock FontFamily="ms-appx:///CodeBrix.Platform.Fonts.Merriweather/Fonts/Merriweather.ttf"
           FontSize="16">
  <Run Text="The key signature adds one sharp (" />
  <Run Text="&#x266F;"
       FontFamily="ms-appx:///CodeBrix.Platform.Fonts.NotoMusic/Fonts/NotoMusic.ttf" />
  <Run Text=") and the theme opens on a dotted quarter " />
  <Run Text="&#x1D15F;&#x1D16D;"
       FontFamily="ms-appx:///CodeBrix.Platform.Fonts.NotoMusic/Fonts/NotoMusic.ttf" />
  <Run Text="." />
</TextBlock>
```

The same text built in C#:

```csharp
const string MusicFont =
    "ms-appx:///CodeBrix.Platform.Fonts.NotoMusic/Fonts/NotoMusic.ttf";

// "\U0001D15F" is the quarter note; "\U0001D16D" the augmentation dot.
string dottedQuarter = "\U0001D15F\U0001D16D";

var block = new TextBlock();
block.Inlines.Add(new Run { Text = "Opens on a dotted quarter " });
block.Inlines.Add(new Run
{
    Text = dottedQuarter,
    FontFamily = new FontFamily(MusicFont),
});
```

`TextBlock`, `Run` and `FontFamily` are CodeBrix.Platform types, not types from this package - take
their namespaces and their exact constructors from that package's own documentation. What this
package guarantees is the URI string and the codepoints.

Reading the font bytes without a CodeBrix.Platform host:

```csharp
// ms-appx:/// does not resolve in a console app or a unit test.
// Locate the restored package content on disk instead:
//   <nuget-cache>/codebrix.platform.fonts.notomusic.ofllicenseforever/
//       <version>/lib/net10.0/
//       CodeBrix.Platform.Fonts.NotoMusic/Fonts/NotoMusic.ttf
byte[] fontBytes = System.IO.File.ReadAllBytes(pathToNotoMusicTtf);
```

The minimum project file - the music font plus a text font:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CodeBrix.Platform.Fonts.NotoMusic.OflLicenseForever" />
    <!-- a text face for the prose around the symbols -->
    <PackageReference Include="CodeBrix.Platform.Fonts.Merriweather.OflLicenseForever" />
  </ItemGroup>

</Project>
```

A minimum page that puts both to work:

```xml
<Page x:Class="MyApp.MainPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <StackPanel Padding="24" Spacing="8">

    <TextBlock Text="&#x1D11E;"
               FontFamily="ms-appx:///CodeBrix.Platform.Fonts.NotoMusic/Fonts/NotoMusic.ttf"
               FontSize="64" />

    <TextBlock FontFamily="ms-appx:///CodeBrix.Platform.Fonts.Merriweather/Fonts/Merriweather.ttf"
               FontSize="16">
      <Run Text="Treble clef, then a natural sign " />
      <Run Text="&#x266E;"
           FontFamily="ms-appx:///CodeBrix.Platform.Fonts.NotoMusic/Fonts/NotoMusic.ttf" />
      <Run Text=" inline." />
    </TextBlock>

  </StackPanel>
</Page>
```

If the glyphs come out blank, the font is not in the payload or the URI is wrong - check the spelling
of the URI before suspecting the font.

## Using it in a CodeBrix.Platform application

Reference the package alongside a text font package. The music font's file is always in the payload,
because the package injects no MSBuild target and needs no `SupportsFontManifest` setting, and it has
no head-specific behavior at all.

Nothing selects this font automatically, so put the music glyphs on screen by switching `FontFamily`
on the `<Run>` that carries them - or on a whole `TextBlock` when the block is nothing but symbols.

> [!WARNING]
> Do not assign this font to `DefaultTextFontFamily`. Its Latin coverage exists for notation labels;
> prose set in it will be missing characters, and everything else in the application inherits the
> symbols font.

Outside a CodeBrix.Platform host, read the file from the restored package folder as in the example
above.

## Pitfalls

- **Do not set Noto Music as an application's default text font**, and do not assign its URI to
  `DefaultTextFontFamily`.
- **Never add a `#FamilyName` fragment to the font URI.** CodeBrix.Platform strips it during font
  resolution, and on `DefaultTextFontFamily` it silently disables the startup manifest preload - the
  appended `.manifest` lands inside the fragment and is dropped by `Uri.PathAndQuery`.
- **Do not write `NotoMusic-Regular.ttf` in a URI.** There is no such file in this package; the file
  is `NotoMusic.ttf`.
- **Do not request bold or italic.** There is one face; any "bold" you get is synthetic and will not
  look like notation type.
- **Do not index or truncate strings containing astral music characters by `char` position** - you
  will split a surrogate pair.
- **Do not expect Greek or Cyrillic prose glyphs from this font.** The shipped `cmap` has none; the
  Greek in this font is the ancient musical-notation block.
- **There is no system-font fallback anywhere in the CodeBrix family.** A codepoint this font lacks
  will not be borrowed from the OS; it renders as `.notdef`, which this font draws as a box.
- **`ms-appx:///` is a CodeBrix.Platform concept, not a .NET one.** In a console application or a
  unit test the URI will not resolve; locate the file on disk instead.
- **Do not expect this package to add a `.targets` file to your build.** It has none by design.

Two habits help: prefer the BMP characters `U+2669`-`U+266F` over their astral equivalents when
either will do, because there are no surrogate pairs to mishandle and the strings are shorter; and
group symbol runs, because one `<Run>` carrying several glyphs costs less layout work than one
`<Run>` per glyph and avoids repeated `FontFamily` switches inside a paragraph.

## Samples and tools in the repository

The repository contains no sample applications, demo apps, tools, scripts or optional test-data
downloads. It is a single font asset package plus the test project that guards it.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Test project | xUnit v3 and SilverAssertions suite that pins the package's contents - the single font file, the one-entry manifest, the `.uprimarker` and the assembly metadata | [`tests/CodeBrix.Platform.Fonts.NotoMusic.Tests`](https://github.com/ellisnet/CodeBrix.Platform.Fonts.NotoMusic/tree/main/tests/CodeBrix.Platform.Fonts.NotoMusic.Tests) |

The three classes:

- `ContentManifestTests.cs` - the manifest deserializes, holds exactly one entry, covers only weight
  400, Normal stretch and upright, points at the dash-free `NotoMusic.ttf`, is rooted at
  `ms-appx:///CodeBrix.Platform.Fonts.NotoMusic/Fonts/` and names a file that exists.
- `ContentFilePresenceTests.cs` - exactly one `.ttf` ships, no dash-bearing file survives, and the
  `.uprimarker` is present and empty.
- `AssemblyMetadataTests.cs` - the assembly is named `CodeBrix.Platform.Fonts.NotoMusic`, targets
  .NET 10 and exports no public types.

Run them from the repository root:

```bash
dotnet test CodeBrix.Platform.Fonts.NotoMusic.slnx
```

No opt-in environment variables, no downloads, no special preparation.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Platform.Fonts.NotoMusic/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.NotoMusic/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.NotoMusic/blob/main/README-INDEX.txt) |
| Non-package content in the repository | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.NotoMusic/blob/main/EXTRAS-README.txt) |
| Tests (worked examples of the manifest and the asset contract) | [tests/CodeBrix.Platform.Fonts.NotoMusic.Tests](https://github.com/ellisnet/CodeBrix.Platform.Fonts.NotoMusic/tree/main/tests/CodeBrix.Platform.Fonts.NotoMusic.Tests) |

## License

CodeBrix.Platform.Fonts.NotoMusic is licensed under `OFL-1.1`, the SIL Open Font License 1.1, and the
license is also named in the package ID (`CodeBrix.Platform.Fonts.NotoMusic.OflLicenseForever`). The
whole package - packaging wrapper and bundled font alike - is published under that one SPDX
expression, `OFL.txt` is packed at the root of the nupkg, and the package sets
`PackageRequireLicenseAcceptance`, so a restore in an interactive tool asks you to accept the
license.

Noto Music declares no Reserved Font Name, so SIL OFL 1.1 condition 3 places no naming restriction
here. For the provenance and licensing of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.Fonts.NotoMusic/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Platform.Fonts.Merriweather](CodeBrix.Platform.Fonts.Merriweather.md) - the serif face
  used for the prose in every example above
- [Views and styling](../platform/06-views-and-styling.md) - where fonts and runs fit into a
  CodeBrix.Platform page
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.Platform.Fonts.NotoMusic on GitHub](https://github.com/ellisnet/CodeBrix.Platform.Fonts.NotoMusic) - source and tests
