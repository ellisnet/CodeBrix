<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Platform.Unicode</sub>

# CodeBrix.Platform.Unicode

**These two packages deliver the International Components for Unicode (ICU) native runtime to a .NET
application on Windows and on macOS.** Neither has a managed API: each is a carrier for the ICU
"common" native library and its data-shim companion, plus `icudt.dat` - the ICU data archive - and a
`buildTransitive` `.targets` pair that delivers that archive into the consuming build automatically.
Reference one from a CodeBrix.Platform head, or from any .NET 10 executable that drives the
framework's text engine.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Platform.Unicode](https://github.com/ellisnet/CodeBrix.Platform.Unicode) |
| **Packages** | [`CodeBrix.Platform.Unicode.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.Unicode.ApacheLicenseForever) (Windows)<br>[`CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever) (macOS) |
| **License** | Apache 2.0 and Unicode License Version 3; see [License](#license) |
| **Requires** | .NET 10 or later; no NuGet dependencies; a build that resolves a matching runtime identifier if the native binaries are wanted in the output |
| **Use it from** | A CodeBrix.Platform head, or any .NET 10 executable that uses the framework's text engine |
| **Platforms** | Windows (`win-x64`, `win-arm64`) and macOS (universal, under the `osx` RID). Linux ships its own ICU and uses that. |

## What it does

- Carries the ICU "common" native library and its data-shim companion under `runtimes/<rid>/native/`,
  which is where NuGet's native-asset mechanism finds them.
- Carries `icudt.dat`, the ICU data archive: Unicode character properties, normalization,
  bidirectional data, case mapping, break iteration, CLDR locale data, time zones and
  transliteration.
- Delivers that archive into the consuming build with no code and no configuration, through a
  `buildTransitive` `.targets` pair - embedding it into a CodeBrix.Platform head assembly, or copying
  it beside a non-head executable.
- Shares one sentinel property between the two packages, so a build that sees both delivers
  `icudt.dat` exactly once.
- Ships separate x64 and ARM64 binaries for Windows, and one universal dylib pair for macOS that
  serves both Intel and Apple-silicon Macs from a single folder.

Windows and macOS have no system ICU. Without one of the two delivery routes, the first call into
anything that needs a Unicode property table - line breaking, for example - fails with
`U_MISSING_RESOURCE_ERROR`.

## When to use it

Reference the Windows package from a Windows head, console tool or test project, and the macOS
package from a macOS head, console tool or test project. Reference neither from a Linux head: Linux
distributions ship their own ICU and a CodeBrix.Platform Linux head uses that. Nothing here is
shipped for Android, iOS or browser-wasm targets.

These packages serve the CodeBrix.Platform ICU engine, not `System.Globalization`. Referencing them
does not switch the .NET runtime to app-local ICU and makes no `runtimeconfig` change of any kind:
`CultureInfo`, string comparison and `DateTime` formatting behave exactly as they did before.

They carry no managed API of any kind - no P/Invoke wrappers, no typed accessors over the data
archive, no ICU facade; the carrier assemblies export zero public types. They also carry only ICU's
"common" library and the data shim, so the number, date, message and collation *formatting* entry
points that live in ICU's i18n library are absent.

> [!NOTE]
> The ICU "ICU" trademark is not reused for these packages, and a modified redistribution must not
> imply endorsement by the Unicode Consortium.

## Getting started

Pick the package that matches the operating system the project builds for:

```bash
# Windows apps:
dotnet add package CodeBrix.Platform.Unicode.ApacheLicenseForever

# macOS apps:
dotnet add package CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever
```

There is no namespace to import and nothing to call. Everything a consumer touches is a native
library or an MSBuild property. In a CodeBrix.Platform head, the reference is the whole integration:

```xml
<PackageReference
    Include="CodeBrix.Platform.Unicode.ApacheLicenseForever" />
```

`$(IsCodeBrixHead)` is already `'true'` in a head, so the embed target fires during
`BeforeBuild`/`BeforeCompile` and `icudt.dat` is embedded in the head assembly. No code changes, no
runtime configuration, no `using` directive. The macOS package works identically through its own
target.

## Key concepts

### Which package to reference

The two packages are per-OS, not alternatives.

| Project | Package |
| --- | --- |
| Windows head, console tool or test project | `CodeBrix.Platform.Unicode.ApacheLicenseForever` |
| macOS head, console tool or test project | `CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever` |
| Linux head (X11, Wayland, frame buffer) | Neither - Linux has a system ICU |
| Android, iOS, browser-wasm | Neither - not shipped |

In a multi-head solution, reference the Windows package from the Windows head project and the macOS
package from the macOS head project. Referencing both from one project is also safe and explicitly
supported: the two packages share one sentinel so `icudt.dat` is delivered exactly once, and their
MSBuild target names are deliberately distinct, so importing both produces no target-redefinition
error. Which package's copy of `icudt.dat` wins is not defined and does not matter - the two archives
are byte-identical.

A class library may carry the reference - that is how a host-free CodeBrix.Platform add-in makes ICU
available to whatever consumes it, because the `.targets` sit in `buildTransitive/` and flow on to
the downstream project - but the library's own build gets neither delivery route.

### The MSBuild contract, and the three properties that drive it

The API of these packages is the MSBuild contract in their `buildTransitive/` folder. Each package
ships an auto-imported `.targets` file named exactly after its package ID, which does nothing but
import the package's `Common.targets`:

```text
buildTransitive/CodeBrix.Platform.Unicode.ApacheLicenseForever.targets
    -> imports CodeBrix.Platform.Unicode.Common.targets

buildTransitive/CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever.targets
    -> imports CodeBrix.Platform.UnicodeMacOs.Common.targets
```

Each `Common.targets` defines two targets, one per delivery route, and both are gated on the same
properties.

- `$(IsCodeBrixHead)` is `'true'` when the consuming project is a CodeBrix.Platform application head.
  It is set by the head's own build; these packages only read it. It is the route selector: `'true'`
  picks the embed route, anything else picks the copy-beside route.
- `$(CodeBrixIcuDataIncluded)` is the embed-once sentinel. Both routes require it to be not `'true'`
  before they fire, and both set it to `'true'` after firing. It is shared verbatim between the two
  packages.
- `$(OutputType)` is read by the copy-beside route only, which requires `Exe` or `WinExe`.

### Route 1 - embed into a head

Target `AddCodeBrixPlatformUnicodeEmbeddedResource` (or
`AddCodeBrixPlatformUnicodeMacOsEmbeddedResource`) runs `BeforeTargets: BeforeBuild;BeforeCompile`
when `'$(IsCodeBrixHead)' == 'true'` and the sentinel is not yet set. Its whole effect is one item,
followed by the sentinel write:

```xml
<EmbeddedResource Include="...buildTransitive/icudt.dat" />
```

The archive becomes part of the head assembly. The head's own build also generates a module
initializer that names that assembly to the ICU engine, so nothing in application code has to locate
or register the archive; these packages contribute only the `EmbeddedResource` item.

### Route 2 - copy beside a non-head executable

Target `AddCodeBrixPlatformUnicodeDataFile` (or `AddCodeBrixPlatformUnicodeMacOsDataFile`) runs
`BeforeTargets: AssignTargetPaths` when `$(IsCodeBrixHead)` is not `'true'`, the sentinel is not yet
set, and `$(OutputType)` is `Exe` or `WinExe`:

```xml
<None Include="...buildTransitive/icudt.dat"
      Link="icudt.dat"
      CopyToOutputDirectory="PreserveNewest"
      Visible="false" />
```

`icudt.dat` lands beside the built application, where the ICU engine looks for it when no assembly
carries an embedded copy. This is the route that makes host-free consumers work - a test project, a
console tool, an image or text pipeline: anything that is an executable but is not a
CodeBrix.Platform head. It is what lets the host-free text add-ins,
[TextLayout](../platform/add-ins/TextLayout.md) and
[AdvancedTextEdit](../platform/add-ins/AdvancedTextEdit.md), be exercised outside an application.

The `BeforeTargets` choice is load-bearing: a `None` item must carry a target path before
`GetCopyToOutputDirectoryItems` reads it, so the target hooks `AssignTargetPaths` rather than
`BeforeBuild`. Hooking it later would silently skip the copy.

### Class libraries get neither route

A project with `OutputType` `Library` matches neither condition: it is not a head, and it is not
`Exe` or `WinExe`. That is deliberate - whatever roots the build supplies the archive once, instead
of every library in the dependency graph dropping its own copy into the output folder. If a library
needs ICU at test time, its test project - an executable - picks up route 2.

```text
IsCodeBrixHead   OutputType       Result
--------------   --------------   ---------------------------------------
true             (any)            icudt.dat embedded in the head assembly
not true         Exe / WinExe     icudt.dat copied beside the executable
not true         Library          nothing delivered (by design)
```

In every row the sentinel must be not `'true'` when the target runs.

### How the runtime finds icudt.dat

The ICU engine looks for the data archive in this order: first as a manifest resource of the assembly
that the head's generated module initializer named to it (route 1); then as a file named `icudt.dat`
sitting beside the running application (route 2). If neither is present, ICU has no data tables at
all and the first operation that needs one fails with `U_MISSING_RESOURCE_ERROR`. That error is the
canonical signal that `icudt.dat` was not delivered - the project is a `Library`, the sentinel was
already `'true'` when the targets ran, or the package reference is missing from the project that
roots the build.

Do not rename, relocate or compress `icudt.dat` in the output folder: route 2 places it beside the
application under exactly that name, and that is where the engine looks.

### Runtime identifiers and the native binaries

Delivery of `icudt.dat` is RID-independent - it happens on any build. The native binaries are
different: they sit under `runtimes/<rid>/native/`, and NuGet copies those to the output only when
the build resolves a matching runtime identifier. There is no AnyCPU or RID-less fallback copy in
either package.

Windows ships two RIDs, `win-x64` and `win-arm64`, with genuinely separate binaries. A build that
targets both must produce RID-specific outputs and publish per RID; a single RID-less build cannot
carry both:

```xml
<RuntimeIdentifiers>win-x64;win-arm64</RuntimeIdentifiers>
```

macOS ships one RID, `osx`. Because `osx` is the RID-graph parent of `osx-x64` and `osx-arm64`, a
build for either resolves the `osx` folder, and the universal dylibs contain both architecture
slices. There is never a need for an `osx-x64` or `osx-arm64` variant of the package.

### Opting out

There is no dedicated opt-out property; the sentinel is the lever. Set it at project scope - a plain
`PropertyGroup` in the csproj, so it is evaluated before targets run - and both targets are skipped
for that project:

```xml
<PropertyGroup>
  <CodeBrixIcuDataIncluded>true</CodeBrixIcuDataIncluded>
</PropertyGroup>
```

Do this only when you are delivering `icudt.dat` yourself, for example when a custom publish step
places the archive beside the application. Setting it without supplying the archive some other way
produces the `U_MISSING_RESOURCE_ERROR` symptom above. There is no property that turns the native
`runtimes/` assets off either; use standard NuGet `ExcludeAssets` on the `PackageReference` for that.

To force the embed route on a project that is not a head, set `<IsCodeBrixHead>true</IsCodeBrixHead>`
at project scope - but route 1 only adds the `EmbeddedResource`, and the module initializer that
names the carrying assembly to the engine comes from the head build, so a hand-rolled head must
arrange that itself. Prefer route 2 for non-head executables.

## Examples

One project that must build for both desktop operating systems can condition the references.
Conditioning is an optimization - it keeps the unused OS's natives out of the graph - not a
requirement, because referencing both is safe.

```xml
<ItemGroup Condition="'$(OS)' == 'Windows_NT'">
  <PackageReference
      Include="CodeBrix.Platform.Unicode.ApacheLicenseForever" />
</ItemGroup>
<ItemGroup Condition="'$(OS)' != 'Windows_NT'">
  <PackageReference
      Include="CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever" />
</ItemGroup>
```

A test project that exercises text layout is an executable and is not a head, so route 2 fires and
`icudt.dat` is copied next to the test binary. Without the package reference, the first line-breaking
call inside a test fails with `U_MISSING_RESOURCE_ERROR`.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="xunit.v3" />
    <PackageReference
        Include="CodeBrix.Platform.Unicode.ApacheLicenseForever" />
  </ItemGroup>
</Project>
```

A console tool is the smallest complete route-2 project. No ICU code of your own is needed; the tool
calls whatever CodeBrix.Platform text API it uses, and the package's only job is that the archive is
on disk before that call happens.

```xml
<!-- TextTool.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference
        Include="CodeBrix.Platform.Unicode.ApacheLicenseForever" />
  </ItemGroup>
</Project>
```

The build output shows both mechanisms at work - the archive from the copy-beside route, and the
native binaries from the resolved RID:

```text
Result in bin/Debug/net10.0/win-x64/:

    TextTool.exe
    TextTool.dll
    icudt.dat            <- route 2 (PreserveNewest)
    icuuc77.dll          <- runtimes/win-x64/native
    icudt77.dll          <- runtimes/win-x64/native
```

Swap the package ID for `CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever` and the RID for
`osx-arm64` or `osx-x64` to get the macOS equivalent, with `libicuuc.dylib` and `libicudata.dylib` in
place of the DLLs.

## Using it in a CodeBrix.Platform application

A head needs only the `PackageReference`. `$(IsCodeBrixHead)` is already `'true'` in a head, so the
embed route fires and `icudt.dat` becomes part of the head assembly - no code changes, no runtime
configuration, no `using` directive.

Reference the Windows package from the Windows heads and the macOS package from the macOS head. The
Linux heads - X11, Wayland and frame buffer - reference neither, because Linux has a system ICU. A
solution that ships all six heads therefore carries two of these references in total, one per
non-Linux head, and nothing changes anywhere else.

## Pitfalls

- Do not assume a class library gets the archive. It does not, by design. If a library's consumers
  hit `U_MISSING_RESOURCE_ERROR`, the fix is a package reference in the application or test project
  that roots the build, not a workaround in the library.
- Do not expect the native binaries without a runtime identifier. `runtimes/<rid>/native/` assets are
  RID-resolved. A RID-less build still gets `icudt.dat`, because that route is RID-independent, but
  no native library - a combination that looks like the package did nothing.
- Do not set `$(CodeBrixIcuDataIncluded)` to `'true'` as a cleanup for a duplicate reference. It
  suppresses delivery entirely for that project. Duplicate references are already safe; the sentinel
  is what makes them safe.
- Do not copy the data-shim library and call it the data. `icudt77.dll` and `libicudata.dylib` are
  tiny shims; all ICU data lives in `icudt.dat`. Copying only the native binaries leaves ICU with no
  tables.
- Do not rename or relocate `icudt.dat` in the output folder, and do not add your own duplicate item
  with `CopyToOutputDirectory` set.
- Do not add an `osx-x64` or `osx-arm64` specific package. There is only an `osx` RID folder, and the
  dylibs are universal binaries that a more specific RID resolves to through the RID graph.
- Do not expect ICU formatting or collation entry points. The shipped binaries are the "common"
  library only: `ubrk_*`, `unorm2_*`, `ucnv_*`, `uloc_*` and `u_*` are present, while `ucol_open`,
  `udat_open` and `unum_open` live in ICU's i18n library and are not shipped.
- Do not expect .NET's own globalization to change. Nothing in either package sets the app-local ICU
  switch or `InvariantGlobalization`, and the binaries are not placed where the .NET host looks for
  an app-local ICU.
- Do not strip `UNICODE-LICENSE.txt` or `THIRD-PARTY-NOTICES.txt` from a redistribution of your own
  application. The Unicode half of the license expression requires the notice to travel with the
  binaries.
- If a Linux target reports missing ICU, install the distribution's own ICU runtime package rather
  than referencing anything from here.
- Deliver the archive once per build root. That is exactly why class libraries are excluded and why
  the sentinel is shared between the two packages; do not defeat either mechanism by copying the
  archive yourself in addition. If a publish targets one architecture, publish with that single RID
  rather than shipping both Windows RID folders.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Platform.Unicode/blob/main/README.md) |
| Complete API guide for both packages (ships inside them too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform.Unicode/blob/main/AGENT-README.txt) |
| Samples, tools and other non-package content | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Platform.Unicode/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Platform.Unicode/blob/main/README-INDEX.txt) |
| The full Unicode License Version 3 | [UNICODE-LICENSE.txt](https://github.com/ellisnet/CodeBrix.Platform.Unicode/blob/main/UNICODE-LICENSE.txt) |
| The shipped delivery targets (Windows) | [src/CodeBrix.Platform.Unicode/buildTransitive](https://github.com/ellisnet/CodeBrix.Platform.Unicode/tree/main/src/CodeBrix.Platform.Unicode/buildTransitive) |
| The shipped delivery targets (macOS) | [src/CodeBrix.Platform.UnicodeMacOs/buildTransitive](https://github.com/ellisnet/CodeBrix.Platform.Unicode/tree/main/src/CodeBrix.Platform.UnicodeMacOs/buildTransitive) |
| Tests (the executable specification of the MSBuild contract) | [tests/CodeBrix.Platform.Unicode.Tests](https://github.com/ellisnet/CodeBrix.Platform.Unicode/tree/main/tests/CodeBrix.Platform.Unicode.Tests) |

The supported ICU version is listed in the repository's `AGENT-README.txt`.

The repository contains no samples, demo applications or tools; the two test projects are the only
non-package content. They are asset and MSBuild-contract tests rather than behavior tests - they
never load ICU or call a native entry point. Each copies its package's payload into a `TestAssets/`
folder mirroring the in-package folder shape, then asserts that the files are present, that the
binaries carry the right file-format magic bytes, that the `.targets` declare the expected target
names, conditions and items, and that the carrier assembly exports no public types. Because a test
project is an executable and is not a head, each suite is also a live example of the copy-beside
route: building it drops `icudt.dat` next to the test binary.

```bash
dotnet test CodeBrix.Platform.Unicode.slnx
```

## License

Both packages are published under the SPDX expression `Apache-2.0 AND Unicode-3.0`. The packaging,
the `.targets` files and the carrier assemblies are Apache 2.0 - the license is also named in the
package IDs (`CodeBrix.Platform.Unicode.ApacheLicenseForever` and
`CodeBrix.Platform.UnicodeMacOs.ApacheLicenseForever`) - while the bundled ICU native binaries and
`icudt.dat` are under the Unicode License, Version 3. Both packages require license acceptance, and
both ship `UNICODE-LICENSE.txt` and `THIRD-PARTY-NOTICES.txt` inside the package, which is what
satisfies the Unicode redistribution conditions: do not strip those files from a redistribution of
your own application. For the provenance and licensing of open source code included in this library,
see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Platform.Unicode/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Platform](CodeBrix.Platform.md) - the framework whose heads set `$(IsCodeBrixHead)` and
  whose text engine reads the archive
- [TextLayout](../platform/add-ins/TextLayout.md) - the host-free text engine that route 2 exists to
  make testable
- [Packaging and shipping](../platform/11-packaging-and-shipping.md) - runtime identifiers and what
  lands in the published output
- [ellisnet/CodeBrix.Platform.Unicode on GitHub](https://github.com/ellisnet/CodeBrix.Platform.Unicode) - source and tests
