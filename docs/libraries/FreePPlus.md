<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › FreePPlus</sub>

# FreePPlus

**FreePPlus reads and writes Excel files (`.xlsx` and `.xlsm`) using the Office Open XML format.** It
does not require Microsoft Excel, COM interop, or any Office component to be installed: it manipulates
the OOXML package directly, so it runs on Windows, Linux and macOS and inside containers. Everything a
real spreadsheet needs is here - styles, formulas that calculate in process, tables, pivot tables,
charts, sparklines, conditional formatting, data validation, protection, encryption and VBA - and you
use it from any .NET 10 application or from a CodeBrix.Platform application.

| | |
| --- | --- |
| **Repository** | [ellisnet/FreePPlus](https://github.com/ellisnet/FreePPlus) |
| **Packages** | [`FreePPlus.LgplLicenseForever`](https://www.nuget.org/packages/FreePPlus.LgplLicenseForever) |
| **License** | LGPL-3.0-or-later; see [License](#license) |
| **Requires** | .NET 10 or later. No native libraries and no operating-system setup, except that on Linux `AutoFitColumns()` and `ExcelFont.SetFromFont()` need a font family with bold and italic faces installed |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Windows, Linux and macOS, and inside containers |

## What it does

- Creates workbooks from scratch, opens existing ones, fills a template workbook, and saves to a file,
  a stream or a `byte[]` - synchronously or asynchronously, with optional file encryption.
- Manages worksheets: add, copy, delete, move, hide, and chart sheets.
- Addresses cells and ranges by A1 string, by row and column, or by a four-corner rectangle, with typed
  reads through `GetValue<T>()`.
- Gives `ExcelRange` the workhorse members: `Copy`, `Clear`, `Offset`, `Sort`, `Merge`, `AutoFilter`,
  `AutoFitColumns`, array formulas and R1C1 addressing - and makes a range `IEnumerable<ExcelRangeBase>`
  so LINQ over cells works.
- Styles cells through `range.Style`: font, fill, border, number format, alignment, plus named styles
  defined once and applied by name.
- Holds several differently formatted runs inside one cell as rich text.
- Calculates formulas in process with a built-in function set, and lets you extend that set with your
  own worksheet functions.
- Loads data in bulk from a collection, a `DataTable`, an `IDataReader`, `object[]` arrays, or delimited
  text.
- Builds tables with table styles and totals rows, and pivot tables with row, column, page and data
  fields plus numeric and date grouping.
- Draws charts with titles, legends, axes, series, secondary axes and combo types, and adds sparklines
  in line, column and stacked form.
- Places pictures, preset shapes and background images, adds cell comments, and attaches external or
  in-workbook hyperlinks.
- Validates input with integer, decimal, list, text-length, date, time and custom rules.
- Applies conditional formatting through rule families for averages and standard deviation, top and
  bottom, time periods, cell-value comparisons, text and blank/error tests, unique and duplicate values,
  color scales, icon sets and data bars.
- Protects a worksheet or a workbook, defines protected ranges, and writes a genuinely
  password-to-open encrypted file.
- Creates and edits VBA projects, standard and class modules, the built-in workbook and worksheet code
  modules, VBA passwords and digital signatures.
- Sets headers, footers, print settings, freeze panes and workbook or worksheet views, and reads and
  writes core, extended and custom document properties.

## When to use it

Reach for FreePPlus whenever a .NET program has to produce or consume a real Excel workbook -
a report a finance team opens, an export a user asked for, a template a business already maintains, an
upload to parse - and installing Office on the machine is not an option. Server-side generation is the
central case: the library builds the whole workbook in memory and hands you a `byte[]`.

What it deliberately does not do:

- Legacy `.xls` (BIFF / Excel 97-2003) reading or writing. Only `.xlsx` and `.xlsm`.
- ODS, Google Sheets, or any non-OOXML format.
- Exporting to PDF, HTML or images. Rendering is not its job: it never draws a chart, an image or a
  page; it writes the OOXML that Excel renders. For PDFs, see
  [CodeBrix.PdfDocuments](CodeBrix.PdfDocuments.md).
- Automating a running copy of Excel, Excel add-ins, Excel Services or SharePoint integration.
- Power Query, Power Pivot, the data model, or slicers.
- Computing pivot table results. FreePPlus writes the pivot definition and an empty cache marked
  "refresh on load"; Excel fills in the cells when the file is opened.
- Writing CSV. `LoadFromText` imports delimited text, but there is no CSV export.
- Evaluating formula functions outside the built-in set. Anything else still round-trips into the file
  for Excel to evaluate; extend the parser yourself when you need it computed in process.
- Range-level `Insert` and `Delete`, `ToText` / `ToDataTable`, and fluent sort builders. Insert and
  delete whole rows and columns on the worksheet instead.

## Getting started

```bash
dotnet add package FreePPlus.LgplLicenseForever
```

Three different names are all correct in their own place: the package is `FreePPlus.LgplLicenseForever`,
the assembly is `FreePPlus.OfficeOpenXml`, and the namespace root is `OfficeOpenXml`. There is no
`FreePPlus` namespace.

```csharp
using OfficeOpenXml;                        // ExcelPackage, ExcelWorkbook,
                                            // ExcelWorksheet, ExcelRange
using OfficeOpenXml.Style;                  // fonts, fills, borders,
                                            // alignment, rich text
using OfficeOpenXml.Style.Dxf;              // differential styles used by
                                            // conditional formatting
using OfficeOpenXml.Table;                  // ExcelTable, TableStyles
using OfficeOpenXml.Table.PivotTable;       // pivot tables
using OfficeOpenXml.Drawing;                // pictures, shapes
using OfficeOpenXml.Drawing.Chart;          // charts
using OfficeOpenXml.ConditionalFormatting;  // conditional formatting
using OfficeOpenXml.ConditionalFormatting.Contracts;  // the rule interfaces
using OfficeOpenXml.DataValidation;         // validation rules
using OfficeOpenXml.DataValidation.Contracts;
using OfficeOpenXml.Sparkline;              // sparkline groups
using OfficeOpenXml.VBA;                    // VBA project / modules
using OfficeOpenXml.FormulaParsing;         // FormulaParserManager,
                                            // ExcelCalculationOption
using OfficeOpenXml.FormulaParsing.Excel.Functions;
using OfficeOpenXml.FormulaParsing.ExpressionGraph;   // CompileResult,
                                                      // DataType
using CodeBrix.Imaging;                     // Color, Image, Font
```

The minimum for most tasks is `using OfficeOpenXml;` plus `using OfficeOpenXml.Style;` for anything that
touches `.Style`. No registration call is required.

```csharp
using System;
using System.IO;
using OfficeOpenXml;

using var package = new ExcelPackage();
var ws = package.Workbook.Worksheets.Add("Sheet1");
ws.Cells["A1"].Value = "Hello, Excel!";
ws.Cells["B1"].Value = 42;
ws.Cells["C1"].Formula = "B1*2";
ws.Calculate();
ws.Cells.AutoFitColumns();
package.SaveAs(new FileInfo("output.xlsx"));
Console.WriteLine("Created output.xlsx");
```

Two details in that program repay attention. `ExcelPackage` is `IDisposable` and holds an open zip
package, so the `using` is not optional. And the formula is written without a leading `=`; until
`Calculate()` runs, a formula cell's `Value` is null.

## Key concepts

### The package lifecycle

`ExcelPackage` has constructors for an empty in-memory workbook, a `FileInfo`, a `FileInfo` plus a
template, a `Stream`, a `Stream` plus a template stream, and a password variant of each. On the way out
there are `Save`, `SaveAs`, their async counterparts `SaveAsync` and `SaveAsAsync`, `GetAsByteArray()`
for a web response, and `Load(Stream)` for reading one back. Beyond the workbook itself it exposes
`Encryption`, `Compatibility`, `Compression` (a `CompressionLevel` trading size against time),
`DoAdjustDrawings`, and the format constants `MaxRows` (1048576) and `MaxColumns` (16384).

### Cells, ranges and addressing

`ws.Cells` is an `ExcelRange` with three indexers: `this[string Address]` accepting `"A1"`, `"A1:D10"` or
`"A1:A5,C1:C5"`; `this[int Row, int Col]`; and `this[int FromRow, int FromCol, int ToRow, int ToCol]`.
Read `.Value` for the raw value, `.Text` for the value formatted by its number format, and
`.GetValue<int>()` or `.GetValue<int?>()` for a typed read where a blank becomes null. `ExcelCellBase`,
which every range derives from, carries the static address helpers `GetAddress`, `GetAddressRow`,
`GetAddressCol`, `GetFullAddress`, `IsValidAddress`, `IsValidCellAddress`, `TranslateFromR1C1` and
`TranslateToR1C1`, and `ExcelCellAddress.GetColumnLetter(int column)` completes the set. Because
`ExcelRangeBase` derives from `ExcelAddress`, anywhere the API asks for an `ExcelAddress` you may pass
`ws.Cells["A1:A5"]` directly.

Cell row and column indices are always 1-based: `ws.Cells[1, 1]` is A1 and `ws.Cells[0, 0]` throws. The
*worksheet collection* is the exception - it is 0-based by default, and switchable with
`package.Compatibility.IsWorksheets1Based = true;` or, for a whole application, through an optional
`appsettings.json` next to the executable.

```json
{
  "FreePPlus": {
    "ExcelPackage": {
      "Compatibility": {
        "IsWorksheets1Based": true
      }
    }
  }
}
```

### Styling

Everything hangs off `range.Style`: `Font`, `Fill`, `Border`, `Numberformat`, `HorizontalAlignment`,
`VerticalAlignment`, `WrapText`, `ShrinkToFit`, `Indent`, `TextRotation`, `Locked`, `Hidden`,
`QuotePrefix` and `ReadingOrder`. `ExcelColor` has exactly two setters, `SetColor(Color color)` and
`SetColor(int alpha, int red, int green, int blue)`, and the `Color`, `Image` and `Font` types come from
`CodeBrix.Imaging`, not from `System.Drawing`. Define a style once and apply it by name rather than
touching cells individually:

```csharp
var named = package.Workbook.Styles.CreateNamedStyle("Money");
named.Style.Numberformat.Format = "#,##0.00";
named.Style.Font.Bold = true;
ws.Cells["C2:C100"].StyleName = "Money";

// derive one named style from another
var bold = package.Workbook.Styles.CreateNamedStyle("Bold", named.Style);
```

### Formulas and calculation

Set formulas without a leading `=`. Assigning a formula to a multi-cell range fills the whole range and
shifts the relative references per row and column, exactly like filling down in Excel:
`ws.Cells["D2:D10"].Formula = "B2*C2";`. R1C1 is available as `FormulaR1C1`, and array formulas through
`ws.Cells["B1:B3"].CreateArrayFormula("A1:A3");`.

```csharp
void Calculate(this ExcelWorkbook workbook)
void Calculate(this ExcelWorkbook workbook, ExcelCalculationOption options)
void Calculate(this ExcelWorksheet worksheet)
void Calculate(this ExcelWorksheet worksheet, ExcelCalculationOption options)
void Calculate(this ExcelRangeBase range)
void Calculate(this ExcelRangeBase range, ExcelCalculationOption options)
object Calculate(this ExcelWorksheet worksheet, string formula)
object Calculate(this ExcelWorksheet worksheet, string formula,
                 ExcelCalculationOption options)
```

These are extension methods on `CalculationExtension`, which lives in the root `OfficeOpenXml`
namespace, so `using OfficeOpenXml;` is all `ws.Calculate()` needs. `ExcelCalculationOption` has one
property, `bool AllowCirculareReferences`. At the workbook level,
`package.Workbook.CalcMode = ExcelCalcMode.Automatic;` and `package.Workbook.FullCalcOnLoad = true;` ask
Excel to recalculate when it opens the file.

<details>
<summary>The complete set of functions FreePPlus can compute in process, all case-insensitive in a formula</summary>

```text
abs acos acosh address and asin asinh atan atan2 atanh average averagea
averageif averageifs ceiling char choose column columns concatenate cos cosh
count counta countblank countif countifs date datevalue daverage day days360
dcount dcounta degrees dget dmax dmin dsum dvar dvarp edate eomonth
error.type exact exp fact false find fixed floor hlookup hour hyperlink if
iferror ifna index indirect int isblank iserr iserror iseven islogical isna
isnontext isnumber isodd isoweeknum istext large left len ln log log10
lookup lower match max maxa median mid min mina minute mod month n na
networkdays networkdays.intl not now offset or pi pmt power product proper
quotient rand randbetween rank rank.avg rank.eq replace rept right round
rounddown roundup row rows search second sign sin sinh small sqrt sqrtpi
stdev stdev.p stdevp stdev.s substitute subtotal sum sumif sumifs sumproduct
sumsq t tan tanh text time timevalue today true trunc upper value var varp
vlookup weekday weeknum workday year yearfrac
```

Anything outside that list is written to the file verbatim and evaluated by Excel when the file is
opened, but `Calculate()` cannot produce a value for it.

</details>

### Bulk loading

`LoadFromCollection` is the fastest and most compact way to fill a sheet. Its header rules, verified in
source, are: a `[DescriptionAttribute("...")]` wins if present; otherwise a `[DisplayNameAttribute("...")]`;
otherwise the member name with `_` replaced by a space. With inheritance, derived-class properties are
emitted before base-class properties, and that is the column order you get. Passing an empty
`MemberInfo[]` throws; pass null to mean "all properties". The return value is the filled range, which
is what you feed to `AutoFitColumns` or to a pivot table.

Beside it sit `LoadFromDataTable`, `LoadFromDataReader`, `LoadFromArrays` and six `LoadFromText`
overloads for delimited text, driven by an `ExcelTextFormat` carrying `Delimiter`, `TextQualifier`,
`EOL`, `Culture`, `Encoding`, `SkipLinesBeginning`, `SkipLinesEnd` and per-column `DataTypes`.

### Conditional formatting

Two entry points share the same rule objects: `ws.ConditionalFormatting`, where every `Add*` method takes
an `ExcelAddress` first, and `range.ConditionalFormatting`, which is the same set with no address
argument. Every rule implements `IExcelConditionalFormattingRule` with `Type`, `Address`, `Priority`,
`StopIfTrue`, `Style` and `Node` - program against the interfaces; you never need to construct a rule
class directly. Lower `Priority` wins, and `StopIfTrue` halts later rules for a matching cell.

> [!IMPORTANT]
> A rule's `Style` is a *differential* style from `OfficeOpenXml.Style.Dxf`, not the `ExcelStyle` used
> for cells. Its members are nullable, its colors are `ExcelDxfColor`, and there is no
> `SetColor(a, r, g, b)` on it - assign its `Color` property instead.

The families are averages and standard deviation (`AddAboveAverage`, `AddBelowAverage`,
`AddAboveStdDev`, `AddBelowStdDev` and their or-equal variants); top and bottom (`AddTop`,
`AddTopPercent`, `AddBottom`, `AddBottomPercent`, each with a `Rank`); time periods (`AddLast7Days`
through `AddTomorrow`); cell-value comparisons (`AddEqual` through `AddNotBetween`, plus
`AddExpression`); text and blank or error tests (`AddContainsText`, `AddBeginsWith`, `AddEndsWith`,
`AddContainsBlanks`, `AddContainsErrors` and their negations); unique and duplicate values; color scales
(`AddTwoColorScale`, `AddThreeColorScale`); icon sets (`AddThreeIconSet`, `AddFourIconSet`,
`AddFiveIconSet`); and data bars (`AddDatabar`).

### Pivot tables

`ws.PivotTables.Add(ExcelAddressBase Range, ExcelRangeBase Source, string Name)` places a pivot table.
`Range` is where the table is *placed* and must be on the worksheet you call it on; `Source` is the data
range including its header row, usually on another sheet. Names must be unique across the workbook, and
the placement range must not collide with an existing pivot table. Fields sort into `Fields`,
`RowFields`, `ColumnFields`, `PageFields` and `DataFields`, and a field groups with
`AddNumericGrouping` or one of the three `AddDateGrouping` overloads, where `eDateGroupBy` is a flags
enum you combine with `|`. `ExcelPivotCacheDefinition.SourceRange` is settable, so a pivot can be
repointed at a different range.

### Charts, sparklines and drawings

Charts are created through `ExcelDrawings.AddChart` or `ExcelWorksheets.AddChart`, each with an overload
taking a pivot table so the chart is fed by it. Position and size come from `ExcelDrawing`, the base
class of every chart, picture and shape: `SetPosition(row, rowOffsetPixels, column, columnOffsetPixels)`
or `SetPosition(pixelTop, pixelLeft)`, and `SetSize(pixelWidth, pixelHeight)` or `SetSize(percent)`.
`ExcelChart` exposes `Title`, `Series`, `XAxis`, `YAxis`, `Legend`, `PlotArea`, `Border`, `Fill`,
`View3D`, `Style`, `UseSecondaryAxis` and the rest; combo charts and secondary axes go through
`PlotArea.ChartTypes`. Cast to `ExcelBarChart`, `ExcelLineChart`, `ExcelPieChart`, `ExcelScatterChart`
and their siblings for the type-specific members.

Sparklines are added with
`ws.SparklineGroups.Add(eSparklineType type, ExcelAddressBase locationRange, ExcelAddressBase dataRange)`,
where `eSparklineType` is `Line`, `Column` or `Stacked`. The two ranges must line up: one sparkline is
produced per row (or per column) of the data range, mapped onto the cells of the location range.

Pictures and shapes go through `ws.Drawings`, with `AddPicture` overloads taking a `CodeBrix.Imaging`
`Image` or a `FileInfo`, and `AddShape` taking an `eShapeStyle` preset. A worksheet background image is
`ws.BackgroundImage.SetFromFile(...)`, shown on screen and not printed by Excel, and header and footer
pictures go through `ws.HeaderFooter.OddHeader.InsertPicture(...)`.

### Protection and encryption

Worksheet protection is `ws.Protection.SetPassword(...)` plus the `Allow*` switches, and it stops
editing in Excel - it does not encrypt the file. Cells are protected only when the sheet is protected
*and* the cell style is locked, and `Style.Locked` defaults to true, so unlock the ranges you want
editable first. Named protected ranges come from `ws.ProtectedRanges.Add(...)`. Workbook protection
(`LockStructure`, `LockWindows`, `LockRevision`) is separate again. Real encryption is
`package.Encryption`, with `IsEncrypted`, `Password`, an `EncryptionAlgorithm` of AES128, AES192 or
AES256, and an `EncryptionVersion` of `Standard` or `Agile`; reading such a file back requires the same
password in the constructor or in `Load()`.

### Extending the formula parser

`package.Workbook.FormulaParserManager` exposes `LoadFunctionModule(IFunctionModule)`,
`AddOrReplaceFunction(string, ExcelFunction)`, `CopyFunctionsFrom(ExcelWorkbook)`,
`GetImplementedFunctionNames()`, `GetImplementedFunctions()`, `Parse(string)` and the logger
attachments. Write a function by deriving from `ExcelFunction` and overriding one method:

```csharp
public abstract CompileResult Execute(IEnumerable<FunctionArgument> arguments, ParsingContext context)
```

`ExcelFunction` gives you the protected helpers `ValidateArguments`, `ArgToInt`, `ArgToDecimal`,
`ArgToString`, `ArgToBool`, `ArgToAddress`, `ArgsToDoubleEnumerable`, `ArgsToObjectEnumerable`,
`CreateResult`, `GetResultByObject`, `ThrowExcelErrorValueException` and `ThrowArgumentExceptionIf`.
Group several functions in a module by deriving from `FunctionsModule`, and register function names in
lower case - formula lookup is case-insensitive.

## Examples

A sales report: styled header, a formula filled down a column, a number format and frozen panes.

```csharp
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;

using var package = new ExcelPackage();
var ws = package.Workbook.Worksheets.Add("Sales Report");

ws.Cells["A1"].Value = "Product";
ws.Cells["B1"].Value = "Quantity";
ws.Cells["C1"].Value = "Unit Price";
ws.Cells["D1"].Value = "Total";

ws.Cells["A2"].Value = "Widget"; ws.Cells["B2"].Value = 25;
ws.Cells["C2"].Value = 3.50;
ws.Cells["A3"].Value = "Gadget"; ws.Cells["B3"].Value = 10;
ws.Cells["C3"].Value = 12.99;
ws.Cells["A4"].Value = "Gizmo";  ws.Cells["B4"].Value = 50;
ws.Cells["C4"].Value = 1.75;

ws.Cells["D2:D4"].Formula = "B2*C2";

using (var header = ws.Cells["A1:D1"])
{
    header.Style.Font.Bold = true;
    header.Style.Fill.PatternType = ExcelFillStyle.Solid;
    header.Style.Fill.BackgroundColor.SetColor(255, 0, 51, 102);
    header.Style.Font.Color.SetColor(255, 255, 255, 255);
}

ws.Cells["C2:D4"].Style.Numberformat.Format = "#,##0.00";
ws.Cells["A1:D4"].AutoFitColumns();
ws.View.FreezePanes(2, 1);

package.SaveAs(new FileInfo("SalesReport.xlsx"));
```

A whole workbook for a web response, with no temporary file anywhere: `LoadFromCollection` fills the
sheet and `GetAsByteArray()` hands back the bytes.

```csharp
using OfficeOpenXml;

public byte[] GenerateExcelReport(IEnumerable<OrderDto> orders)
{
    using var package = new ExcelPackage();
    var ws = package.Workbook.Worksheets.Add("Orders");
    ws.Cells["A1"].LoadFromCollection(orders, true);
    ws.Cells.AutoFitColumns();
    return package.GetAsByteArray();
}
```

Reading an existing workbook and writing a column back into it. `Dimension` is null on an empty sheet,
which is why the guard is there.

```csharp
using System;
using System.IO;
using OfficeOpenXml;

using var package = new ExcelPackage(new FileInfo("input.xlsx"));
var ws = package.Workbook.Worksheets["Sheet1"];

if (ws.Dimension != null)
{
    for (int row = 2; row <= ws.Dimension.End.Row; row++)
    {
        var name = ws.Cells[row, 1].Text;
        var value = ws.Cells[row, 2].GetValue<int>();
        Console.WriteLine($"{name}: {value}");
        ws.Cells[row, 3].Value = "Processed";
    }
    ws.Cells[1, 3].Value = "Status";
}

package.SaveAs(new FileInfo("output.xlsx"));
```

A pivot table over a loaded collection, with a chart fed by the pivot.

```csharp
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using OfficeOpenXml.Table.PivotTable;

var sales = new[]
{
    new { Region = "North", Category = "Tools", Amount = 1200m },
    new { Region = "North", Category = "Parts", Amount =  800m },
    new { Region = "South", Category = "Tools", Amount = 1500m },
    new { Region = "South", Category = "Parts", Amount =  650m },
    new { Region = "West",  Category = "Tools", Amount =  900m },
};

using var package = new ExcelPackage();

var wsData = package.Workbook.Worksheets.Add("SalesData");
var dataRange = wsData.Cells["A1"].LoadFromCollection(sales, true,
                                                      TableStyles.Medium2);
wsData.Cells.AutoFitColumns();

var wsPivot = package.Workbook.Worksheets.Add("Pivot");
var pivot = wsPivot.PivotTables.Add(wsPivot.Cells["A1"], dataRange,
                                    "SalesByRegion");

pivot.RowFields.Add(pivot.Fields["Region"]);
pivot.ColumnFields.Add(pivot.Fields["Category"]);

var amount = pivot.DataFields.Add(pivot.Fields["Amount"]);
amount.Function = DataFieldFunctions.Sum;
amount.Format = "#,##0.00";
amount.Name = "Total";

pivot.DataOnRows = false;
pivot.RowGrandTotals = true;
pivot.ColumnGrandTotals = true;
pivot.TableStyle = TableStyles.Medium9;

// a chart fed by the pivot table
var chart = wsPivot.Drawings.AddChart("PivotChart",
                                      eChartType.ColumnClustered, pivot);
chart.SetPosition(1, 0, 6, 0);
chart.SetSize(600, 380);

package.SaveAs(new FileInfo("Pivot.xlsx"));
```

Six conditional-formatting rules over one sheet, showing the differential style, the range entry point
and rule priority.

```csharp
using CodeBrix.Imaging;
using OfficeOpenXml;
using OfficeOpenXml.ConditionalFormatting;
using OfficeOpenXml.Style;

// 1. cell-value rule with a differential style
var risk = ws.ConditionalFormatting.AddContainsText(
               new ExcelAddress("D2:D11"));
risk.Text = "AT RISK";
risk.Style.Fill.PatternType = ExcelFillStyle.Solid;
risk.Style.Fill.BackgroundColor.Color = Color.MistyRose;
risk.Style.Font.Color.Color = Color.DarkRed;
risk.Style.Font.Bold = true;

// 2. top 3 scores
var top = ws.ConditionalFormatting.AddTop(new ExcelAddress("B2:B11"));
top.Rank = 3;
top.Style.Font.Bold = true;

// 3. three-color scale across the same column
var scale = ws.ConditionalFormatting.AddThreeColorScale(
                new ExcelAddress("B2:B11"));
scale.LowValue.Type = eExcelConditionalFormattingValueObjectType.Min;
scale.MiddleValue.Type =
    eExcelConditionalFormattingValueObjectType.Percentile;
scale.MiddleValue.Value = 50;
scale.HighValue.Type = eExcelConditionalFormattingValueObjectType.Max;

// 4. data bar
var bar = ws.ConditionalFormatting.AddDatabar(
              new ExcelAddress("C2:C11"), Color.SteelBlue);
bar.ShowValue = true;

// 5. icon set, applied through the range API
var icons = ws.Cells["C2:C11"].ConditionalFormatting.AddThreeIconSet(
                eExcelconditionalFormatting3IconsSetType.Arrows);
icons.Reverse = false;

// 6. duplicate detection, evaluated first and stopping the rest
var dup = ws.ConditionalFormatting.AddDuplicateValues(
              new ExcelAddress("A2:A11"));
dup.Style.Font.Italic = true;
dup.Priority = 1;
dup.StopIfTrue = true;
```

A custom worksheet function registered through `FormulaParserManager`, computed in process.

```csharp
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing;
using OfficeOpenXml.FormulaParsing.Excel.Functions;
using OfficeOpenXml.FormulaParsing.ExpressionGraph;

public sealed class SumOfSquares : ExcelFunction
{
    public override CompileResult Execute(
        IEnumerable<FunctionArgument> arguments, ParsingContext context)
    {
        ValidateArguments(arguments, 1);
        var result = 0d;
        foreach (var n in ArgsToDoubleEnumerable(arguments, context))
            result += n * n;
        return CreateResult(result, DataType.Decimal);
    }
}

public sealed class MyFunctions : FunctionsModule
{
    public MyFunctions()
    {
        Functions.Add("sumofsquares", new SumOfSquares());
    }
}

using var package = new ExcelPackage();
package.Workbook.FormulaParserManager.LoadFunctionModule(new MyFunctions());

var ws = package.Workbook.Worksheets.Add("Custom");
ws.Cells["A1"].Value = 3;
ws.Cells["A2"].Value = 4;
ws.Cells["A3"].Formula = "SUMOFSQUARES(A1:A2)";
ws.Calculate();
// ws.Cells["A3"].Value is now 25

package.SaveAs(new FileInfo("CustomFunction.xlsx"));
```

## Using it in a CodeBrix.Platform application

FreePPlus is not a CodeBrix.Platform add-in and has no UI surface, so a CodeBrix.Platform application
references it from its `.Core` library exactly as any other .NET project would. Its only CodeBrix
coupling is [`CodeBrix.Imaging.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Imaging.ApacheLicenseForever),
which supplies the `Color`, `Image` and `Font` types that appear in the public API; the remaining
dependencies arrive automatically and need no version pinning in the consuming project.

One caveat matters on Linux desktops and in slim containers, and it is head-independent:
`AutoFitColumns()` and `ExcelFont.SetFromFont()` measure text with real font metrics, so they need a
font family with bold and italic faces installed.

```bash
sudo apt install fonts-dejavu
```

## Pitfalls

- Package id, assembly and namespace are three different names. Package: `FreePPlus.LgplLicenseForever`.
  Assembly: `FreePPlus.OfficeOpenXml`. Namespace: `OfficeOpenXml`. There is no `FreePPlus` namespace.
- There is no `ws.SetFormula(...)` method. Set formulas through the range:
  `ws.Cells[row, col].Formula = "..."`. `ws.SetValue(row, col, value)` does exist and sets values only.
- `LoadFromText(string, ...)` takes the CSV *text*, not a path. Passing a file name silently writes that
  file name into the first cell; use the `LoadFromText(FileInfo, ...)` overloads for files.
- Cell indices are always 1-based; `ws.Cells[0, 0]` throws. The worksheet collection is 0-based by
  default and configurable.
- `ws.Dimension` is null on an empty worksheet. Check it before reading `ws.Dimension.End.Row`.
- Formulas are written without a leading `=`, and a formula cell has a null `Value` until `Calculate()`
  runs or Excel opens the file.
- Setting `Value` on a cell that has a `Formula` clears the formula, and vice versa. Pick one per cell.
- Only the listed functions are computable in process. Any other function still round-trips into the
  file, but `Calculate()` cannot evaluate it.
- Do not set `ExcelTable.ShowTotal = true` on a table created through `ws.Tables.Add()`. Excel reports
  the resulting file as corrupted; use `ShowFilter`, `ShowHeader` and `TableStyle` instead.
- `LoadFromCollection` with an empty `MemberInfo[]` throws. Pass null to mean "all public instance
  properties". With inheritance it emits derived-class properties first, then base-class properties -
  that is the column order you get.
- `Color`, `Image` and `Font` come from `CodeBrix.Imaging`, not `System.Drawing`. If `Color` does not
  resolve, the `using CodeBrix.Imaging;` line is missing.
- Conditional formatting styles are differential styles, not the `ExcelStyle` used for cells, and rule
  order is `Priority` (lower runs first), not insertion order.
- Sort column indices are zero-based *within the range* - 0 is the range's leftmost column - and an
  index outside the range throws `ArgumentException`.
- Range-level `Insert` and `Delete` do not exist. Use `ws.InsertRow`, `ws.DeleteRow`, `ws.InsertColumn`
  and `ws.DeleteColumn`.
- A worksheet's cells are only protected when the sheet is protected *and* the cell style is `Locked`,
  which defaults to true. Unlock the ranges you want editable before calling
  `ws.Protection.SetPassword(...)` - and remember that sheet protection is not encryption.
- Save a workbook that has a VBA project with the `.xlsm` extension. The content type is switched for
  you, but Excel keys off the file extension.
- Pivot table cells are computed by Excel on open, not by FreePPlus. Reading a pivot table's values back
  through this library gives you nothing.
- Chart type names are `XYScatter`, `XYScatterLines` and `XYScatterSmooth` - there are no members named
  `Scatter`, `ScatterLines` or `ScatterSmooth`.
- `ExcelDrawingFill.Style` reads as `NoFill`, `SolidFill`, `GradientFill`, `PatternFill`, `BlipFill` or
  `GroupFill`, but only `NoFill` and `SolidFill` can be assigned; anything else throws
  `NotImplementedException`. Setting `Fill.Color` when the style is already something other than
  `SolidFill` throws, so set `Fill.Style = eFillStyle.SolidFill` first.
- Five drawing enums - `eShapeStyle`, `eTextAlignment`, `eFillStyle`, `eEndStyle` and `eEndSize` - are
  declared in the global namespace rather than under `OfficeOpenXml.Drawing`. They resolve with no
  `using` directive at all, and adding one does not help if you cannot find them.
- An image constructed in memory has no known encoded format, so PNG is used when FreePPlus stores it.
- Only `.xlsx` and `.xlsm` are supported; legacy `.xls` will not open.
- `ExcelPackage` is `IDisposable` and holds a zip package - and a file handle when constructed from a
  `FileInfo`. Not disposing it leaves files locked.

For performance: prefer the bulk loaders over per-cell loops, style ranges or named styles rather than
individual cells, and call `AutoFitColumns` last and scoped to the used range, because auto-fit measures
real font metrics and is the most expensive routine in a typical report. Use `GetAsByteArray()` or
`SaveAs(Stream)` for web responses and the async saves on request paths, call `Calculate()` only when
the code needs the results (otherwise set `FullCalcOnLoad`), and read `.Value` rather than `.Text` when
the number format does not matter.

## Samples and tools in the repository

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Sample application | A console application covering the library end to end, run one scenario at a time | [`samples/FreePPlus.OfficeOpenXml.SampleApp`](https://github.com/ellisnet/FreePPlus/tree/main/samples/FreePPlus.OfficeOpenXml.SampleApp) |
| Test project | The best worked set of usage examples in the repository | [`tests/FreePPlus.OfficeOpenXml.Tests`](https://github.com/ellisnet/FreePPlus/tree/main/tests/FreePPlus.OfficeOpenXml.Tests) |

Run the sample application from its own folder, naming the scenarios you want:

```bash
dotnet run -- --run:1,5,9
```

Each run creates a timestamped sub-folder under the output folder for the workbooks it produces, unknown
sample numbers are reported and skipped, and a failure in one sample does not stop the rest.

> [!NOTE]
> The sample application expects its output folder to exist and refuses to start without it. The folder
> is the hard-coded constant `Program.TempFolder` - edit that constant, and `Program.AdvWorksConnectString`
> if you want the database-backed samples, before running on Linux or macOS. With no `--run` argument
> the application prints usage and exits.

<details>
<summary>What each numbered scenario in the sample application covers</summary>

- **1** - creates a workbook from scratch: an inventory list on a single worksheet.
- **2** - opens an existing workbook and reads values and document properties.
- **3** - populates a workbook from a SQL Server database, including a named "HyperLink" style.
- **4** - fills a template workbook that already contains a chart with exchange rates and points three
  series at the new data.
- **5** - reopens the sample 1 output, inserts rows and adds a pie chart.
- **6** - walks the file system and builds a report with pictures, freeze panes and hyperlinks.
- **7** - loads many rows, styles them, inserts a header row, freezes panes and protects the sheet so
  only two columns stay editable.
- **8** - LINQ over the `Cells` collection.
- **9** - loads two delimited-text files with `LoadFromText`, turns them into tables, and adds charts
  that combine two chart types and a secondary axis.
- **10** - workbook and worksheet protection plus file encryption, including a second workbook written
  with `EncryptionAlgorithm.AES192`.
- **11** - data validation: integer, list and the other validation types.
- **12** - pivot tables: a simple pivot with one row field and one data field, and a second with date
  grouping by year and quarter, a page field and several formatted data fields, plus a pivot-driven
  chart.
- **13** - the loading APIs side by side: `LoadFromDataTable`, `LoadFromCollection` with an anonymous
  type, and `LoadFromCollection` with a `List<T>`.
- **14** - conditional formatting end to end: color scales, above and below average, above and below
  standard deviation, top and bottom, time-period rules, text rules, icon sets and data bars, with rule
  priorities and `StopIfTrue`.
- **15** - VBA: creates a project, writes workbook and worksheet code modules, adds standard and class
  modules from the `.txt` files in `VBA-Code/`, sets a VBA project password and saves `.xlsm` workbooks.
- **16** - sparklines: loads a semicolon-delimited currency file with a Swedish culture, then adds
  column, line and stacked sparkline groups.

Two more samples are not wired into the menu and are called directly: `Sample_FormulaCalc.cs` shows
worksheet, workbook and range `Calculate()` and evaluating a formula string without calculating
dependent cells, and `Sample_AddFormulaFunction.cs` registers custom worksheet functions through
`FormulaParserManager`, both as a `FunctionsModule` and one at a time with `AddOrReplaceFunction`,
including overriding the built-in `TEXT` function.

Samples 3, 4 and 12 need a local SQL Server carrying the AdventureWorks LT sample database; without it
they fail with a connection error that the sample runner catches and reports.

</details>

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/FreePPlus/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/FreePPlus/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/FreePPlus/blob/main/README-INDEX.txt) |
| Non-package content in the repository | [EXTRAS-README.txt](https://github.com/ellisnet/FreePPlus/blob/main/EXTRAS-README.txt) |
| Tests (worked examples of every API) | [tests/FreePPlus.OfficeOpenXml.Tests](https://github.com/ellisnet/FreePPlus/tree/main/tests/FreePPlus.OfficeOpenXml.Tests) |
| Samples | [samples/FreePPlus.OfficeOpenXml.SampleApp](https://github.com/ellisnet/FreePPlus/tree/main/samples/FreePPlus.OfficeOpenXml.SampleApp) |

`WorksheetTests.cs` is the single most comprehensive test file - worksheets end to end, values, rows and
columns, formulas, styling, merging, AutoFilter, freeze panes, comments, data validation, protection,
hyperlinks, rich text, delimited-text loading, headers and footers and print settings. Beside it,
`LoadFromCollectionTests.cs`, `CalculationTests.cs`, `DrawingTests.cs`, `SparkLineTests.cs`,
`ConditionalFormatting/ConditionalFormattingTests.cs`, `EncryptTests.cs` and `VBATests.cs` each cover
one area in depth. XML documentation ships alongside the assembly, and the package carries
`AGENT-README.txt` - a complete API reference written for AI coding agents.

## License

> [!WARNING]
> FreePPlus is licensed under the GNU Lesser General Public License version 3 or later, and the license
> is also named in the package ID (`FreePPlus.LgplLicenseForever`). Referencing the unmodified NuGet
> package from proprietary software is permitted; if you modify FreePPlus source you must make those
> modifications available under the LGPL.

For the provenance and licensing of open source code included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/FreePPlus/blob/main/THIRD-PARTY-NOTICES.txt) in
the repository.

---

**Where to go next**

- [CodeBrix.Imaging](CodeBrix.Imaging.md) - the source of the `Color`, `Image` and `Font` types in this API
- [CodeBrix.PdfDocuments](CodeBrix.PdfDocuments.md) - the document side of the same topic group, for PDFs rather than workbooks
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/FreePPlus on GitHub](https://github.com/ellisnet/FreePPlus) - source, tests and samples
