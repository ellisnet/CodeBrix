<sub>[CodeBrix](../../README.md) › [Build a CodeBrix.Platform application](README.md) › Platform services</sub>

# Platform services

**By the end of this chapter you will be able to show a dialog, open a native file picker, read and write the clipboard, repaint a canvas, run a timer, drive an embedded browser and an audio transport from a view model that names no UI type - and store the settings that survive a restart.** This is the seam between a view model and the capabilities only a hosting page or a head can supply, and it is the same shape every time.

## One shape for every capability

The view model declares a small interface holding a delegate, implements it itself, and the page fills the delegate in when the data context arrives. One piece of shared code then runs both on a head that supplies the capability and on a head that does not.

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.Core/Services/IMediaFileBridge.cs
/// <summary>
/// The one thing the main view model cannot do for itself: ask a person which file to open. Only a
/// head knows how to show a file dialog, so the page fills this in.
/// </summary>
public interface IMediaFileBridge
{
    Func<Task<string>> PickMediaFileAsync { get; set; }
}
```

Notice where the interface lives: in the view model's own file, not in a head assembly. That is what lets four unrelated UI stacks satisfy it - the Skia heads, a native WinUI head, a WPF head and a mobile head each fill in the same delegate with their own API.

Five rules make the shape work, and they apply to every recipe in this chapter.

- **Wire in `DataContextChanged`, subscribed before `InitializeComponent()`**, because on these heads it is `InitializeComponent()` that sets the data context. Reference pages carry `//Leave this line last` on that call.
- **Cast with `as` and call through `?.`**, so a page whose data context is something else - or a design-time context - does nothing at all.
- **Handle two distinct "no capability" signals.** A null delegate means no head ever wired one; a `NotSupportedException` from the platform means the head wired one but the platform refuses. The first deserves an explanation, the second is usually silent.
- **Never throw at the null path.** A head that supplies nothing still runs, and tells the user why the button did nothing.
- **Null every delegate in `Dispose()`**, or the page stays alive through the view model.

Here is a page filling in three bridges at once:

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml.cs
DataContextChanged += (_, _) =>
{
    (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

    if (DataContext is IFileSaveBridge fileSave)
    {
        fileSave.PickSaveJpegPathAsync = PickSaveJpegPathAsync;
    }

    if (DataContext is ICanvasBridge canvasBridge)
    {
        //Frames and tracking results arrive on capture/worker threads - marshal
        //  the repaints onto the UI thread
        canvasBridge.InvalidateMainCanvas = () => DispatcherQueue?.TryEnqueue(() => MainCanvas?.Invalidate());
        canvasBridge.InvalidateSelfView = () => DispatcherQueue?.TryEnqueue(() => SelfViewCanvas?.Invalidate());
    }
};

InitializeComponent();
```

Notice that the page marshals to the UI thread, not the view model: the delegate is where the thread knowledge belongs, because a native WPF head has to marshal differently. The null-conditional chain inside each delegate matters too - these can fire while the page is being torn down.

> [!TIP]
> Wire the `XamlRoot` getter even in an application that has no dialogs yet. It costs one line, and it is the prerequisite for every dialog helper a view model will ever call.

## The XamlRoot getter

`SimpleViewModel` implements `IXamlRootGetter`. The page's job is to hand it a getter - not the value, a getter:

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.UI/Views/MainPage.xaml.cs
using CodeBrix.Platform.Simple;
using Microsoft.UI.Xaml.Controls;

namespace MediaPlayerDemo.Views;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        DataContextChanged += (_, _) =>
        {
            //Give the view model's SimpleDialog helpers a XamlRoot to attach dialogs to
            (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);
        };

        this.InitializeComponent(); //Leave this line last
    }
}
```

Notice it is a lambda rather than the value. The page's `XamlRoot` is null until the page is in the visual tree, so the view model has to re-read it at the moment it needs it. A native head satisfies the same interface with whatever its own dialog API anchors to:

```csharp
// From CodeBrix.Samples/JustBetweenUs/Mobile/Views/MainPage.xaml.cs
(BindingContext as IXamlRootGetter)?.SetXamlRootGetter(() => this);
```

A native WPF head skips this entirely - WPF has no `XamlRoot` - and its dialogs still work, so shared view-model code must never assume the getter was supplied.

The same getter serves platform services that need a root of their own, such as creating an off-screen GL context.

## Dialogs

`ContentDialog` is the standard WinUI type, with one framework-specific requirement:

```csharp
var dialog = new ContentDialog
{
    Title = "Delete file?",
    Content = "This cannot be undone.",
    PrimaryButtonText = "Delete",
    CloseButtonText = "Cancel",
    XamlRoot = this.XamlRoot,          // REQUIRED on this framework
};
var result = await dialog.ShowAsync();
if (result == ContentDialogResult.Primary) { ... }
```

> [!IMPORTANT]
> You must set `XamlRoot`; the framework does not fill it in for you. Calling `ShowAsync` on a dialog that is already showing throws `InvalidOperationException` with the message "A ContentDialog is already opened." Keep one dialog on screen at a time and await the result before showing the next.

Most application code never writes that block, because `SimpleViewModel` supplies awaitable `ConfirmDialog`, `ShowInfo`, `ShowError` and `CreateDialog` helpers on top of it - see [05 - MVVM the right way](05-mvvm-the-right-way.md). What this chapter adds is the case where the code that needs to ask a question is a library that must stay free of UI types.

### Install dialog handlers into a headless model

Let the model expose `Initialize*` methods that take delegates, and let the UI layer install them once it has a root:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/ChromeManager.cs
public delegate Task<ErrorDialogResponse> ErrorDialogHandler (string message, string body, string details);
public delegate Task MessageDialogHandler (string message, string body);
public delegate Task<bool> SimpleEffectDialogHandler (BaseEffect effect, IWorkspaceService workspace);

public interface IProgressDialog
{
    void Show ();
    void Hide ();
    string Title { get; set; }
    string Text { get; set; }
    double Progress { get; set; }
    event EventHandler Canceled;
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs
//Chrome wiring: dialogs need a XamlRoot, so this happens on Loaded
PintaCore.Chrome.InitializeErrorDialogHandler(ShowErrorDialogAsync);
PintaCore.Chrome.InitializeMessageDialog(ShowMessageDialogAsync);
PintaCore.Chrome.InitializeProgessDialog(new ContentProgressDialog(() => XamlRoot));
```

Notice that this wiring happens on `Loaded`, not in the constructor: a dialog needs a `XamlRoot` and there is none before then. The progress dialog takes a `Func<XamlRoot?>` for the same reason - it is constructed before the page has a root. Routing a request to the right dialog is a type switch in the UI layer, which keeps the whole decision in one place.

### A progress dialog driven by synchronous code

A long operation driven by a synchronous loop still has to show progress and offer cancel. Show the dialog without awaiting it:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/ContentProgressDialog.cs
public void Show ()
{
    if (showing)
        return;

    XamlRoot? root = xaml_root_getter ();

    if (root is null)
        return; // No visual tree yet - degrade to no feedback rather than throwing.

    StackPanel panel = new () { Spacing = 12 };
    panel.Children.Add (text_block);
    panel.Children.Add (progress_bar);

    dialog = new ContentDialog {
        Title = Title,
        Content = panel,
        CloseButtonText = "Cancel",
        XamlRoot = root,
    };

    dialog.CloseButtonClick += (_, _) => Canceled?.Invoke (this, EventArgs.Empty);

    showing = true;

    // Deliberately not awaited: the caller is a synchronous engine loop that
    // keeps running while this is on screen, and it calls Hide when done.
    _ = dialog.ShowAsync ();
}
```

Notice the discarded task. Awaiting the show call would block the very loop that is producing the progress. A null root degrades to no feedback rather than throwing, and because the model's progress runs 0 to 1 while the control runs 0 to 100, the adapter does the scaling and clamping in one place.

## Native pickers

The pickers are the standard `Windows.Storage.Pickers` types, and the library that carries CodeBrix.Platform already provides them - no extra package.

```csharp
FileOpenPicker:  IList<string> FileTypeFilter;
                 IAsyncOperation<StorageFile?> PickSingleFileAsync();
                 IAsyncOperation<IReadOnlyList<StorageFile>> PickMultipleFilesAsync();
FileSavePicker:  IAsyncOperation<StorageFile?> PickSaveFileAsync();
FolderPicker:    IAsyncOperation<StorageFolder?> PickSingleFolderAsync();
```

Every head provides the pickers natively - Win32, X11, Wayland and macOS each register their own picker extension. The frame-buffer head does so only after `EnableFileOpenPicker`, `EnableFileSavePicker` or `EnableFolderPicker` on its host builder; without those the picker APIs throw `NotSupportedException`. That is why the recipes below treat "no picker" as a case rather than a crash.

### Open a file

The view model owns the decision, the page owns the dialog:

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.Core/ViewModels/MainViewModel.cs
public SimpleCommand OpenCommand => field ??= new SimpleCommand(
    () => !IsBusy, (Func<object, Task>)(_ => DoOpenAsync()));

private async Task DoOpenAsync()
{
    if (PickMediaFileAsync is null)
    {
        StatusText = "This head has no file dialog, so a file cannot be chosen by hand.";
        return;
    }

    var path = await PickMediaFileAsync();
    if (string.IsNullOrWhiteSpace(path))
    {
        return;
    }

    await AddAsync(path, CancellationToken.None);
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs
private static async Task<string> PickMediaFileAsync()
{
    try
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.VideosLibrary
        };

        foreach (var extension in MediaFormats.ImportExtensions)
        {
            picker.FileTypeFilter.Add(extension);
        }

        picker.FileTypeFilter.Add(".mkv");
        picker.FileTypeFilter.Add(".webm");
        picker.FileTypeFilter.Add(".cbv");

        var file = await picker.PickSingleFileAsync();
        return file?.Path;
    }
    catch (NotSupportedException)
    {
        //A head with no windowing system registers no picker extensions.
        return null;
    }
}
```

Notice that the exception is caught in the page and turned into `null`, so it never reaches the view model. `FileTypeFilter` entries carry the leading dot, and a filter list is only a first pass - validate the chosen file afterwards anyway. A delegate bridge is also trivially substitutable in a scripted run: assign `() => Task.FromResult(knownPath)` and the command runs with no dialog at all.

### Save a file

The save side adds a suggested name, and a second degradation path for a head that wired a delegate but whose platform refuses:

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
/// <summary>
/// Lets the hosting page give the view model a native "Save PDF as…" file dialog. Each head
/// wires this up with the file dialog appropriate to its UI stack (the CodeBrix.Platform
/// <c>FileSavePicker</c> on the Skia heads).
/// </summary>
public interface IFileSaveBridge
{
    /// <summary>
    /// Shows a "save PDF" dialog seeded with suggestedFileName and returns the
    /// full path the user chose, or <c>null</c> if they cancelled. The head leaves this null when
    /// it has no file dialog, in which case the user types the path directly into the box.
    /// Signature: <c>Func&lt;suggestedFileName, Task&lt;chosenPathOrNull&gt;&gt;</c>.
    /// </summary>
    Func<string, Task<string>> PickSavePdfPathAsync { get; set; }
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
private async Task DoSelectOutputFile()
{
    if (!CanSelectOutputFile()) { return; }

    if (PickSavePdfPathAsync == null)
    {
        //No native file dialog on this head — the user types the destination
        //  path directly into the box instead.
        await ShowInfo(
            "This head has no file dialog. Type the full path (including the .pdf file name) " +
            "for the PDF into the “Save PDF to” box.");
        return;
    }

    try
    {
        var chosenPath = await PickSavePdfPathAsync(GetSuggestedFileName());
        if (!string.IsNullOrWhiteSpace(chosenPath))
        {
            OutputFilePath = chosenPath.Trim();
            StatusText = $"Will save to: {OutputFilePath}";
        }
    }
    catch (NotSupportedException)
    {
        //Some heads register no picker — there is no window to host a dialog
        await ShowInfo(
            "File dialogs are not supported on this head. Type the full path (including the " +
            ".pdf file name) for the PDF into the “Save PDF to” box.");
    }
    catch (Exception e)
    {
        await ShowError($"Could not open the file dialog: {e.Message}");
    }
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.UI/Views/MainPage.xaml.cs
private static async Task<string> PickSavePdfPathAsync(string suggestedFileName)
{
    var picker = new FileSavePicker
    {
        SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        SuggestedFileName = suggestedFileName,
        DefaultFileExtension = ".pdf"
    };
    picker.FileTypeChoices.Add("PDF document", new List<string> { ".pdf" });

    var file = await picker.PickSaveFileAsync();
    if (file == null) { return null; }

    //Some heads percent-encode the path they return, which would save "My Book.pdf" as
    //  "My%20Book.pdf"; decode it before anything touches the disk.
    var path = FileDialogHelper.ToFileSystemPath(file.Path);

    FileDialogHelper.RemoveEmptyPlaceholder(path);
    return path;
}
```

The suggested name is the view model's to compute, and it has to be sanitized before the picker sees it:

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
/// <summary>A sensible default PDF file name: the first checked page's title.</summary>
private string GetSuggestedFileName()
{
    var name = Flatten().FirstOrDefault(n => !n.IsPlaceholder && n.IsChecked)?.Title;
    if (string.IsNullOrWhiteSpace(name)) { name = "NotionBook"; }

    var invalid = Path.GetInvalidFileNameChars();
    var cleaned = new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
    return (cleaned.Length == 0 ? "NotionBook" : cleaned) + ".pdf";
}
```

Two variations are worth knowing. A bridge can carry an extension as well as a name (`Func<string, string, Task<string>>`), so the view model can write beside the source file when no dialog exists. And where an application would rather write a file than refuse, the null-delegate branch picks a path itself:

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
string outputPath;

if (PickSaveJpegPathAsync == null)
{
    //No native file dialog on this head (e.g. the Linux framebuffer head) -
    //  save to a default location instead
    outputPath = GetDefaultSavePath();
}
else
{
    outputPath = await PickSaveJpegPathAsync(GetSuggestedFileName());
    if (String.IsNullOrWhiteSpace(outputPath))
    {
        return; //the user cancelled the dialog
    }
    outputPath = outputPath.Trim();

    //Confirm before clobbering an existing file (the head's own overwrite
    //  prompt is suppressed so this is the single confirmation)
    if (File.Exists(outputPath))
    {
        var replace = await ConfirmDialog(
            $"A file already exists at:\n{outputPath}\n\nDo you want to replace it?",
            "Replace existing file?");
        if (!replace)
        {
            StatusText = "Save cancelled - the existing file was kept.";
            return;
        }
    }
}

IsBusy = true;

var jpeg = _paintSession.ExportJpeg();
await File.WriteAllBytesAsync(outputPath, jpeg);
```

Notice the ordering: the busy flag is set only after the dialog closes, so a modal picker does not sit on top of a disabled UI. The page code-behind that hosts a picker needs `using System;` for the awaiter extension that makes the picker awaitable - the using looks unused, and several of these files carry a comment saying why it is there.

### Clean up the path a picker returns

Two small static helpers in the shared library make every head hand the view model the same kind of path:

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/Helpers/FileDialogHelper.cs
/// <summary>
/// Turns the path a picker hands back into a real file-system path. The Linux Skia heads
/// build theirs out of the desktop portal's <c>file://</c> URI and leave it
/// percent-encoded, so a name with a space in it arrives as <c>My%20Book.pdf</c> and
/// would be written to disk under that literal name; accented names fare worse still
/// (<c>Ölberg</c> arrives as <c>%C3%96lberg</c>). Nothing is decoded unless the text
/// really does carry escapes, so paths from heads that already return a plain one — the
/// Win32 and WPF save dialogs — pass through untouched.
/// </summary>
public static string ToFileSystemPath(string path)
{
    if (string.IsNullOrWhiteSpace(path)) { return path; }

    //A head that hands back the whole URI rather than just its path.
    if (path.StartsWith("file:", StringComparison.OrdinalIgnoreCase)
        && Uri.TryCreate(path, UriKind.Absolute, out var uri)
        && uri.IsFile)
    {
        return uri.LocalPath;
    }

    return HasPercentEscape(path) ? Uri.UnescapeDataString(path) : path;
}

//True when the text holds at least one "%" followed by two hex digits. A literal percent
//  sign that is not the start of an escape (say "100% done.pdf") leaves the path alone.
private static bool HasPercentEscape(string text)
{
    for (var i = 0; i + 2 < text.Length; i++)
    {
        if (text[i] == '%' && Uri.IsHexDigit(text[i + 1]) && Uri.IsHexDigit(text[i + 2]))
        {
            return true;
        }
    }

    return false;
}
```

```csharp
// From CodeBrix.Samples/PainDiagram/Shared/Helpers/FileDialogHelper.cs
/// <summary>
/// The WinRT <c>FileSavePicker</c> (Skia heads and native WinUI) creates an empty
/// placeholder file at the chosen path for a brand-new name. Remove it - but only when it
/// is genuinely empty - so a chosen path behaves like a pure destination and the app's own
/// "replace existing file?" prompt fires only for a real, non-empty file. A file that has
/// content is never deleted, so no user data is lost before the save-time confirmation.
/// </summary>
public static void RemoveEmptyPlaceholder(string path)
{
    if (string.IsNullOrWhiteSpace(path)) { return; }

    try
    {
        var info = new FileInfo(path);
        if (info.Exists && info.Length == 0)
        {
            info.Delete();
        }
    }
    catch
    {
        //Leave the file in place if it cannot be removed; the save-time overwrite
        //  prompt will simply ask about it.
    }
}
```

Notice that decoding is conditional. Unconditional decoding would corrupt a legitimate name containing a percent sign, which is why the helper looks for a real escape first. The placeholder is deleted only when its length is zero, and a failure to delete is deliberately swallowed - the worst case is one extra confirmation prompt, never lost data. Call both helpers in the page, before the path reaches the view model, so the view model only ever sees real paths.

A folder picker needs the same decoding, incidentally: a folder called "My Models" would otherwise send every download to a literally named `My%20Models`.

### One replace prompt, not two

If your application asks "replace this file?" and the dialog asks as well, the user is asked twice. The bridge delegate is the seam: each head configures its own dialog to stay silent, and the single point of confirmation is a dialog helper in the view model's command, so the behavior is identical on every head.

```csharp
// From CodeBrix.Samples/PainDiagram/PainDiagram.Wpf/Views/MainWindow.xaml.cs
var dialog = new Microsoft.Win32.SaveFileDialog
{
    Title = "Save PNG as",
    Filter = "PNG image (*.png)|*.png|All files (*.*)|*.*",
    DefaultExt = ".png",
    AddExtension = true,
    FileName = suggestedFileName,
    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
    OverwritePrompt = false   //The app does its own replace prompt via SimpleDialog
};
```

On the Skia heads the picker cannot be told to stay quiet, so pair it with the empty-placeholder cleanup above instead. A native WinUI head can drop to the Win32 common item dialog through COM interop and clear the option itself, which also avoids the placeholder file; that is a native-head detail covered in [14 - Sharing code with native frameworks](14-sharing-code-with-native-frameworks.md).

### Remember the folder the user chose

A picker result is worth persisting, and the derived properties that swap a first-launch prompt for real content are worth notifying together:

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
public bool HasAssetsFolder => !string.IsNullOrWhiteSpace(_assetsFolder);

public string AssetsFolderLabel => HasAssetsFolder ? _assetsFolder : "Choose assets folder…";

public Visibility FolderPromptVisibility => HasAssetsFolder ? Visibility.Collapsed : Visibility.Visible;

public Visibility CatalogAreaVisibility => HasAssetsFolder ? Visibility.Visible : Visibility.Collapsed;

public SimpleCommand PickFolderCommand => field ??=
    new SimpleCommand((Func<object, Task>)(_ => PickFolderAsync()));

private async Task PickFolderAsync()
{
    var picker = new FolderPicker
    {
        SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
    };
    picker.FileTypeFilter.Add("*");

    var folder = await picker.PickSingleFolderAsync();
    if (folder == null) { return; }

    _assetsFolder = folder.Path;
    SettingsService.Set(AssetsFolderKey, _assetsFolder);
    NotifyPropertyChanged(nameof(HasAssetsFolder));
    NotifyPropertyChanged(nameof(AssetsFolderLabel));
    NotifyPropertyChanged(nameof(FolderPromptVisibility));
    NotifyPropertyChanged(nameof(CatalogAreaVisibility));

    await ReloadCatalogAsync();
}
```

Notice the filter call: it is required even for a folder picker. A canceled picker returns null and the command returns without touching state. Bind the same command from both the first-launch prompt and the header button, so there is one code path either way.

## Files and folders

`Windows.Storage` gives you `StorageFile` and `StorageFolder`, which is what a picker hands back:

```csharp
var picker = new FileOpenPicker();
picker.FileTypeFilter.Add(".png");
picker.FileTypeFilter.Add(".jpg");
var file = await picker.PickSingleFileAsync();
if (file is not null) { using var stream = await file.OpenReadAsync(); ... }
```

For a file the application ships as an asset rather than one the user chose, the toolkit's `StorageFileHelper.ExistsInPackage(string fileName)` answers whether an item such as `"Assets/x.png"` is present. Everything else - reading, writing, enumerating - is ordinary .NET file I/O over the `Path` a picker returned, which is why the path-hygiene helpers above matter so much: they are what make `File.Exists` tell the truth on every head.

## The clipboard

`Windows.ApplicationModel.DataTransfer.Clipboard` is a static class:

```csharp
public static void SetContent(DataPackage content);
public static DataPackageView? GetContent();
public static void Clear();  public static void Flush();
public static event EventHandler<object> ContentChanged;
```

```csharp
var package = new DataPackage();
package.SetText("copied");
Clipboard.SetContent(package);
var text = await Clipboard.GetContent()?.GetTextAsync();
```

Rich formats - text, HTML, PNG images, file lists and custom formats - work on the desktop heads. The frame-buffer head has only the opt-in text clipboard, enabled with `EnableSimpleTextClipboard()` on its host builder; without that it has no clipboard at all.

Because three UI stacks use three different clipboard APIs, the clipboard is a bridge like any other:

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
public interface ICopyToClipboard { Action<string> CopyTextToClipboard { get; set; }}

// ...

public class MainViewModel : SimpleViewModel, ICopyToClipboard
{
    // ...
    private async Task DoCopyToClipboard()
    {
        if (CanCopyToClipboard())
        {
            if (CopyTextToClipboard != null)
            {
                InvokeOnMainThread(() => CopyTextToClipboard(ProcessedText));
                if (!_copyMessageShown)
                {
                    _copyMessageShown = true;
                    await ShowInfo("The processed text has been copied to the system clipboard.");
                }
            }
            else
            {
                await ShowError(
                    "This platform implementation does not have the Copy-to-clipboard functionality enabled.");
            }
        }
    }

    public Action<string> CopyTextToClipboard { get; set; }
}
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml.cs
public MainPage()
{
    //Doing this before InitializeComponent() - in case InitializeComponent()
    //  is the thing that sets the data context.
    DataContextChanged += (sender, args) =>
    {
        (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

        if (DataContext is ICopyToClipboard copy)
        {
            copy.CopyTextToClipboard = (text) =>
            {
                if (!string.IsNullOrEmpty(text))
                {
                    var clipData = new DataPackage();
                    clipData.SetText(text);
                    Clipboard.SetContent(clipData);
                }
            };
        }
    };

    InitializeComponent();
}
```

The same view model, unchanged, is satisfied by a WPF head in two lines - `copy.CopyTextToClipboard = Clipboard.SetText;` - and by a mobile head with its own asynchronous clipboard.

### A no-op default for a headless model

When the caller is a library rather than a view model, give the interface a null-object implementation from the start so every call site can be unconditional:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/IClipboardService.cs
public interface IClipboardService
{
    void SetText (string text);

    Task<string?> GetTextAsync ();

    void SetImage (ImageSurface surface);

    Task<ImageSurface?> GetImageAsync ();
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/PintaCore.cs
/// <summary>
/// Installs the UI-layer clipboard implementation. Call once at startup.
/// </summary>
/// <remarks>
/// Until this is called the clipboard is a no-op that reports nothing
/// available, so engine code can call it unconditionally.
/// </remarks>
public static void InitializeClipboard (IClipboardService clipboard)
{
    Clipboard = clipboard;
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/PlatformServices.cs
public void SetImage (ImageSurface surface)
{
    // Encode as PNG and hand the platform a stream reference.
    // (Image WRITE is not yet supported by the X11 clipboard backend;
    // this degrades gracefully there.)
    using SKImage image = SKImage.FromBitmap (surface.Bitmap);
    using SKData data = image.Encode (SKEncodedImageFormat.Png, 100);

    InMemoryRandomAccessStream stream = new ();
    using (Stream outStream = stream.AsStreamForWrite ()) {
        data.SaveTo (outStream);
        outStream.Flush ();
    }
    stream.Seek (0);

    DataPackage package = new ();
    package.SetBitmap (RandomAccessStreamReference.CreateFromStream (stream));
    Clipboard.SetContent (package);
}
```

Notice the asymmetry: the reads are asynchronous and the writes are not. Keep it rather than forcing a shape the platform does not have. An image goes through an in-memory random-access stream holding the encoded bytes, seeked back to zero before the package is set.

## Opening a URL in the browser

`Windows.System.Launcher.LaunchUriAsync` hands a URI to whatever application the desktop has registered for its scheme, and it is one of the few platform capabilities a view model calls directly: there is no bridge interface and no page involvement. It does need wrapping, because it has two ways to fail - it returns `false` when the desktop has nothing registered, and it throws when the URI will not parse or the head has no launcher. Write one private method for the whole application, catch both, and report the failure the way the rest of the page reports failure. Items in a list reach that method through a `Func<string, Task>` they were handed when they were built, so no row and no group holds a reference to the view model that made it. The recipe is [Open a URL in the default browser from a view model](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-PlatformServices.md#open-a-url-in-the-default-browser-from-a-view-model).

## Repainting a canvas

Background work changes what should be drawn, and the view model has to trigger a repaint without holding a control reference. One `Action` per canvas is the whole interface:

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
/// <summary>
/// Lets the hosting page hand the view model the invalidate (repaint) delegates for the two
/// Skia canvases. Frames and tracking results arrive on capture/worker threads; the page's
/// delegates are responsible for marshalling their invalidates onto the UI thread.
/// </summary>
public interface ICanvasBridge
{
    Action InvalidateMainCanvas { get; set; }
    Action InvalidateSelfView { get; set; }
}
```

Where a library raises its own "I changed, repaint me" event, the view model subscribes once and forwards - no timer, no per-frame polling anywhere:

```csharp
// From CodeBrix.Samples/PainDiagram/Shared/ViewModels/MainViewModel.cs
public interface ICanvasInvalidator
{
    /// <summary>Invalidates the hosting page's drawing canvas (null before the page wires it up).</summary>
    Action InvalidateCanvas { get; set; }
}

// ... in the constructor:
_session.RedrawRequested += (_, _) => InvalidateCanvas?.Invoke();
_session.DrawingChanged += (_, _) => InvokeOnMainThread(() => HasDrawing = _session.HasStrokes);
```

Notice that the two events get two treatments. A cheap repaint request invokes the delegate directly and lets the delegate marshal; an event that writes a bound property goes through `InvokeOnMainThread` in the view model. A native WPF head marshals differently again, which is exactly why the bridge is a delegate rather than a method the view model could call:

```csharp
// From CodeBrix.Samples/PainDiagram/PainDiagram.Wpf/Views/MainWindow.xaml.cs
private void InvalidateDrawCanvas()
{
    if (DrawCanvas.Dispatcher.CheckAccess())
    {
        DrawCanvas.InvalidateVisual();
    }
    else
    {
        DrawCanvas.Dispatcher.BeginInvoke(DrawCanvas.InvalidateVisual);
    }
}
```

Call the invalidate from the `finally` of a load path too, so a failure still repaints.

## Dispatching and timers

Every `DependencyObject` exposes a `DispatcherQueue`, and so does `Window`. All UI access happens on the UI thread:

```csharp
public bool TryEnqueue(DispatcherQueueHandler callback);
public bool TryEnqueue(DispatcherQueuePriority priority, DispatcherQueueHandler callback);
public bool HasThreadAccess { get; }
public DispatcherQueueTimer CreateTimer();
public static DispatcherQueue GetForCurrentThread();
```

```csharp
var queue = this.DispatcherQueue;           // captured on the UI thread
_ = Task.Run(async () =>
{
    var result = await LoadAsync();
    queue.TryEnqueue(() => StatusText.Text = result);
});
```

Notice that the queue is captured on the UI thread before the background work starts. A view model has `InvokeOnMainThread` for the same purpose; [05 - MVVM the right way](05-mvvm-the-right-way.md) covers the view-model side.

A library that needs a periodic tick on the UI thread should not reference the dispatcher at all. Declare a one-method interface returning a handle:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/ITimerService.cs
public interface ITimerService
{
    /// <summary>
    /// Starts a repeating timer on the UI thread. The callback returns true
    /// to keep ticking or false to stop; disposing the returned handle also
    /// stops the timer.
    /// </summary>
    IDisposable Start (uint intervalMilliseconds, Func<bool> callback);
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/PlatformServices.cs
public IDisposable Start (uint intervalMilliseconds, Func<bool> callback)
{
    Handle handle = new ();
    DispatcherQueueTimer timer = dispatcher.CreateTimer ();
    handle.Timer = timer;
    timer.Interval = TimeSpan.FromMilliseconds (intervalMilliseconds);
    timer.Tick += (_, _) => {
        if (!callback ())
            handle.Dispose ();
    };
    timer.Start ();
    return handle;
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/NullServices.cs
/// <summary>
/// Forwards to the timer service the UI layer installs; before that, started
/// timers never tick.
/// </summary>
public sealed class TimerServiceProxy : ITimerService
{
    public ITimerService? Inner { get; set; }

    public IDisposable Start (uint intervalMilliseconds, Func<bool> callback)
        => Inner?.Start (intervalMilliseconds, callback) ?? new NullHandle ();
}
```

Notice that this one is a proxy rather than a null object. The real implementation arrives after the model has already handed the service to other components, so those references have to stay valid. Both stop signals matter - the callback's `bool` return and disposing the handle - because callers use `using`. The application installs the real service with the window's queue: `PintaCore.InitializeTimer(new DispatcherTimerService(MainWindow.DispatcherQueue))`.

## The cursor

The model decides which cursor is right; the view maps it to a platform cursor in one switch:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs
public ToolCursor? Cursor {
    get => tool_cursor;
    set {
        tool_cursor = value;
        ProtectedCursor = InputSystemCursor.Create (MapCursor (value));
    }
}

private static InputSystemCursorShape MapCursor (ToolCursor? cursor)
{
    if (cursor is null)
        return InputSystemCursorShape.Arrow;

    // Icon/image cursors are approximated with a crosshair until custom
    // bitmap cursors are supported platform-side; tools also draw brush
    // outlines as canvas overlays, which carries most of the meaning.
    if (cursor.IconName is not null || cursor.Image is not null)
        return InputSystemCursorShape.Cross;

    return cursor.Shape switch {
        StandardCursor.Crosshair => InputSystemCursorShape.Cross,
        StandardCursor.Hand => InputSystemCursorShape.Hand,
        StandardCursor.Move => InputSystemCursorShape.SizeAll,
        StandardCursor.IBeam => InputSystemCursorShape.IBeam,
        StandardCursor.NotAllowed => InputSystemCursorShape.UniversalNo,
        StandardCursor.SizeNWSE => InputSystemCursorShape.SizeNorthwestSoutheast,
        // ...
        _ => InputSystemCursorShape.Arrow,
    };
}
```

`ProtectedCursor` is a protected member of `UIElement`, so setting a cursor means working from a subclass. Custom bitmap cursors are not available, so an image-based cursor degrades to the nearest shape - plan for that rather than assuming a bitmap.

## The window

`Window` and `AppWindow` are the standard WinUI types.

| Type | The members an application reaches for |
| --- | --- |
| `Window` | `Title`, `Content`, `Activate()`, `Close()`, `ExtendsContentIntoTitleBar`, `AppWindow`, `DispatcherQueue`, and the `Activated`, `SizeChanged` and `VisibilityChanged` events |
| `AppWindow` | `Title`, `Size`, `ClientSize`, `Position`, `IsVisible`, `Presenter`, `TitleBar`, `Show()`, `Move()`, `Resize()`, `SetPresenter()`, `SetIcon()`, and the `Changed` and `Closing` events |
| `OverlappedPresenter` | `IsAlwaysOnTop`, `IsMaximizable`, `IsMinimizable`, `IsModal`, `IsResizable`, `HasBorder`, `HasTitleBar`, the preferred minimum and maximum sizes, `State`, `Maximize()`, `Minimize()`, `Restore()`, `SetBorderAndTitleBar()` |

```csharp
MainWindow.AppWindow.Resize(new Windows.Graphics.SizeInt32(1280, 800));
if (MainWindow.AppWindow.Presenter is OverlappedPresenter p)
{
    p.IsResizable = false;
    p.Maximize();
}
```

> [!WARNING]
> Several of these are permanent no-ops on the Wayland head, because the protocol gives them to the compositor rather than the client: window positioning and position readback, forced resize, always-on-top, and minimized-state readback. Design the window so it does not need them, or accept that they work on X11, Windows and macOS only. [02 - Runs on every laptop](02-runs-on-every-laptop.md) has the full per-head list.

### Set the launch size and a minimum size

Two seams decide how big the window is, and each has exactly one right place.

The launch size goes in the `App` constructor, before `InitializeComponent()`. Every desktop head reads `ApplicationView.PreferredLaunchViewSize` while it creates the native window, and falls back to the platform's own 1024 by 640 when the value is empty, so nothing written in `OnLaunched` happens early enough to decide it.

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/App.xaml.cs
Windows.UI.ViewManagement.ApplicationView.PreferredLaunchViewSize =
    new Windows.Foundation.Size(LaunchWidth, LaunchHeight);
```

Set it unconditionally on every launch. The setter writes the two numbers into `ApplicationData.Current.LocalSettings`, so the value survives between runs, per head, and an unconditional set keeps that stored copy in step with the constants in your source.

The minimum size goes in `OnLaunched`, immediately after the `Window` is constructed and before `Activate()`.

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/App.xaml.cs
if (MainWindow.AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
{
    presenter.PreferredMinimumWidth = MinimumWidth;
    presenter.PreferredMinimumHeight = MinimumHeight;
}
```

Notice three things. The `is` test succeeds the instant `new Window()` returns, because the constructor builds the native window once the application has finished starting and installs the default `OverlappedPresenter`; setting the constraint before `Activate()` hands it to the window manager before the window is ever shown. `Window.AppWindow` is public here because the core package's build targets define `HAS_CODEBRIX_WINUI` in your project, so nothing has to be declared by hand. And no maximum is set: an unset maximum is the largest possible value, which is what keeps maximize working.

The heads do not agree on the units. The X11 and Win32 heads read both numbers as native pixels, the Wayland and macOS heads read them as logical units, and the WPF head reads the launch size as device-independent units and the minimum as native pixels; the heads also differ on whether the numbers describe the client area or the whole framed window. At a display scale of 1 they all read the same, which is why one pair of constants is the right thing to write. Do not try to correct for the scale from application code: the scale is not knowable until the `XamlRoot` exists, which is after the native window has been created, so any correction is a visible resize of an existing window, and a correction that suits one head is wrong on another.

Two applications show the whole shape, constants included: [GitHubIssueFinder App.xaml.cs](https://github.com/ellisnet/CodeBrix.Samples/blob/main/GitHubIssueFinder/src/GitHubIssueFinder.UI/App.xaml.cs) and [Fresco.Brix App.xaml.cs](https://github.com/ellisnet/CodeBrix.Samples.Gpl3/blob/main/Fresco.Brix/src/Fresco.Brix.UI/App.xaml.cs). To reopen at the size the user left instead of a fixed one, feed the same property from your settings store; see [Restore a window size before any window exists](#restore-a-window-size-before-any-window-exists).

### Veto a close until unsaved work is handled

`Closed` is the platform's cancellable-close event: setting `Handled` vetoes the close, and the X11 head reports `SupportsClosingCancellation`.

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs
//Window-close save prompt. Closed is the platform's cancellable-close
//event: setting Handled vetoes the close, and the X11 head reports
//SupportsClosingCancellation. The save-prompt loop is async, so when
//dirty documents exist the close is vetoed first and re-issued once
//the user has decided.
MainWindow.Closed += async (_, e) =>
{
    if (windowCloseConfirmed) { return; }

    if (!Pinta.Brix.Engine.PintaCore.Workspace.OpenDocuments.Any(d => d.IsDirty)) { return; }

    e.Handled = true;

    try
    {
        if (Views.MainPage.Current is { } page && await page.ConfirmCloseApplicationAsync())
        {
            windowCloseConfirmed = true;
            MainWindow.Close();
        }
    }
    catch (Exception)
    {
        //A failed prompt must never take the window down with unsaved
        //work - the veto above stands and the application stays open.
    }
};
```

Notice the re-entrancy guard. The confirmed `Close()` raises `Closed` again, and without the flag the prompt loops forever. The prompt is asynchronous while the event is not, which is what forces the veto-then-reissue shape. Remember too that not every head has window chrome: an application whose only exit is the window button has no exit path at all on the frame-buffer head.

## The desktop's light or dark preference

`Windows.UI.ViewManagement.UISettings` is the live report of the desktop's own appearance. `GetColorValue(UIColorType.Background)` comes back white when the desktop prefers light and black when it prefers dark, and `ColorValuesChanged` is raised when the user flips the preference, on startup as well when the desktop answers asynchronously. Keep the `UISettings` instance in a field of the page - the platform holds only a weak reference to it, so a local is collected and the event stops arriving - and forward each change to the view model. Whether the platform follows the preference at all is decided elsewhere: the framework follows it only while `Application.RequestedTheme` has never been assigned, so an application that offers a "System default" choice leaves it unassigned for that one choice and sets it in the `App` constructor for every other. [Follow or override the desktop appearance and check it from a shell](https://github.com/ellisnet/CodeBrix.Samples/blob/main/BLUEPRINTS-ThemingAndStyling.md#follow-or-override-the-desktop-appearance-and-check-it-from-a-shell) covers both halves, including how to flip the preference from a shell so the behavior can be proved without logging out.

## An embedded browser

Every Skia head can host a `WebView2`. The Windows, Skia-on-WPF and macOS runtimes have one built in; the Linux heads get one from [CodeBrix.Platform.WebView.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.WebView.ApacheLicenseForever), which is built on WPE WebKit. Reference the add-in once, in the library that carries the application's packages: it is inert where a browser already exists, so one reference covers every head.

```xml
<!-- From CodeBrix.Samples/WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml -->
<WebView2 Grid.Row="1" x:Name="Browser" />
```

The view model never names a browser type. It declares an action to navigate and a method the page calls back:

```csharp
// From CodeBrix.Samples/WikipediaPublisher/Shared/ViewModels/MainViewModel.cs
public interface IWebViewBridge
{
    /// <summary>Navigates the embedded browser to the given URL (null when no WebView).</summary>
    Action<string> NavigateToUrl { get; set; }

    /// <summary>Called by the page whenever the embedded browser lands on a new URL.</summary>
    void SetCurrentBrowserUrl(string url);
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/Shared/ViewModels/MainViewModel.cs
private Task DoSearch()
{
    //Every head has an embedded WebView: browse the real Wikipedia search page; the user
    //  picks an article by navigating to it, and Publish uses whatever page is displayed.
    if (CanSearch() && NavigateToUrl != null)
    {
        var searchUrl =
            $"https://{WikiHost}/w/index.php?search={Uri.EscapeDataString(SearchTerms.Trim())}";
        InvokeOnMainThread(() => NavigateToUrl(searchUrl));
        StatusText = "Browse to the article you want, then click Publish.";
    }

    return Task.CompletedTask;
}

public void SetCurrentBrowserUrl(string url)
{
    if (string.IsNullOrWhiteSpace(url)) { return; }

    InvokeOnMainThread(() =>
    {
        ArticleUrl = url;
        StatusText = IsPublishableArticleUrl(url)
            ? "Ready to publish this article."
            : "Browse to an article page to enable publishing.";
    });
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml.cs
private void InitializeBrowser()
{
    if (_browserInitialized || DataContext is not MainViewModel viewModel) { return; }
    _browserInitialized = true;

    //Use CoreWebView2.Source (the authoritative current URL after redirects / user
    //  navigation); the XAML Browser.Source property does not reliably reflect those.
    Browser.NavigationCompleted += (_, _) =>
        viewModel.SetCurrentBrowserUrl(Browser.CoreWebView2?.Source ?? Browser.Source?.AbsoluteUri);

    viewModel.NavigateToUrl = url =>
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            Browser.Source = new Uri(url);
        }
    };

    Browser.Source = new Uri(MainViewModel.HomeUrl);
}
```

Notice that the current URL is read from the core browser object rather than the XAML `Source` property, which does not reliably reflect redirects or user navigation - all three head implementations carry that same comment. The Skia head wires the browser in a `Loaded` handler behind a guard flag, because `Loaded` can fire more than once.

On Linux the browser engine is a run-time dependency, not a build one: the build succeeds on a machine that cannot run a WebView. Install it with:

```bash
sudo apt install libwpewebkit-2.0-1 libwpebackend-fdo-1.0-1 libwpe-1.0-1
```

The [WebView add-in page](add-ins/WebView.md) has the element's full surface.

## An audio transport

Values that tick many times a second - position, duration, volume, mute - are the documented exception to routing everything through the view model. They are dependency properties on the add-in's element, so the transport binds to them by `ElementName` and the view model owns only the decisions:

```xml
<!-- From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml -->
<!-- Position tracker: elapsed · scrubber · duration, bound
     straight to the AudioPlayer element rather than through the
     view model. The Slider follows playback via the two-way
     PositionSeconds binding, and dragging it seeks the clip
     (the add-in debounces to one seek on thumb release). -->
<StackPanel Orientation="Horizontal" Spacing="10"
            HorizontalAlignment="Center">
    <TextBlock Width="52" VerticalAlignment="Center"
               FontSize="12" TextAlignment="Right"
               Foreground="{StaticResource TextSecondaryBrush}"
               Text="{d:Binding Position, ElementName=AudioElement, Converter={StaticResource TimecodeConverter}}" />
    <Slider Width="300" VerticalAlignment="Center"
            StepFrequency="0.01"
            Maximum="{d:Binding DurationSeconds, ElementName=AudioElement}"
            Value="{d:Binding PositionSeconds, ElementName=AudioElement, Mode=TwoWay}" />
    <TextBlock Width="52" VerticalAlignment="Center"
               FontSize="12"
               Foreground="{StaticResource TextTertiaryBrush}"
               Text="{d:Binding Duration, ElementName=AudioElement, Converter={StaticResource TimecodeConverter}}" />
</StackPanel>
```

Notice that the element exposes the same value twice - a `TimeSpan` for the labels, through a converter, and a `double` in seconds for the slider - so nothing has to convert both ways. Dragging the thumb seeks, and the add-in debounces a drag down to one seek on release, so a two-way binding does not flood the decoder. The transport bar's own visibility still comes from the view model, so the rule about when a transport exists stays testable even though the values inside it do not go through it.

What the view model does own is policy. "Play" on a clip that has run to its end should replay it, and that decision belongs with the state, not in the page:

```csharp
// Adapted from CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs
// The sample implements this in the page; the logic is unchanged, but here the state and
// the decision live on the view model, and the bridge grows read-only transport facts.
public interface IAudioPlayerBridge
{
    // ... LoadAudioSource, PlayAudio, PauseAudio, StopAudio, SetAudioLooping ...

    /// <summary>Whether the player is currently advancing.</summary>
    Func<bool> IsAudioPlaying { get; set; }

    /// <summary>The player's position and the clip's duration.</summary>
    Func<TimeSpan> AudioPosition { get; set; }
    Func<TimeSpan> AudioDuration { get; set; }

    /// <summary>Moves the player to a position.</summary>
    Action<TimeSpan> SeekAudio { get; set; }
}

//How close to the duration still counts as "parked at the end". The player refreshes its
//position on an interval, so the last value it reports before ending can sit just short
//of the duration.
private static readonly TimeSpan AudioEndTolerance = TimeSpan.FromMilliseconds(250);
private bool _audioPlaybackEnded;

public SimpleCommand PlayAudioCommand => field ??= new SimpleCommand(() =>
{
    //A clip that has played through to its end leaves the transport parked at the end,
    //where Play alone has nothing left to play - so rewind first and let one click replay
    //the clip. Two things deliberately do NOT rewind: a player that is still going (a
    //looping clip raises PlaybackEnded on every pass), and a clip the user has scrubbed
    //away from the end since it finished - there, the thumb is the intent.
    if (_audioPlaybackEnded
        && IsAudioPlaying?.Invoke() == false
        && AudioDuration?.Invoke() > TimeSpan.Zero
        && AudioPosition?.Invoke() >= AudioDuration.Invoke() - AudioEndTolerance)
    {
        SeekAudio?.Invoke(TimeSpan.Zero);
    }

    _audioPlaybackEnded = false;
    PlayAudio?.Invoke();
});
```

```csharp
// Adapted from CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs
AudioElement.PlaybackEnded += (_, _) => ViewModel?.NotifyAudioPlaybackEnded();
viewModel.IsAudioPlaying = () => AudioElement?.IsPlaying ?? false;
viewModel.AudioPosition = () => AudioElement?.Position ?? TimeSpan.Zero;
viewModel.AudioDuration = () => AudioElement?.Duration ?? TimeSpan.Zero;
viewModel.SeekAudio = position => AudioElement?.Seek(position);
```

Both blocks are adapted from the sample, which implements this in the page: the logic is the sample's, the placement is the shape to prefer. Notice the tolerance window - the player refreshes its reported position on an interval, so the last value before the end can sit slightly short of the duration - and the "is it playing" check, which is what stops a looping clip being rewound mid-play. Loading a new source and stopping both clear the flag. The element itself is on the [AudioPlayer add-in page](add-ins/AudioPlayer.md).

## Say why a capability is missing

A pane that stays empty looks like a bug. When a hardware capability fails to start, ask the view for its state and let the view model own the message:

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
/// <summary>
/// Shows a dialog explaining why the 3D preview cannot render. Called from the view when
/// the Model View is active and the preview's GLCanvasElement reports that its OpenGL
/// initialization failed (e.g. on systems without OpenGL 3.0+ support, where the preview
/// would otherwise just be an empty pane).
/// </summary>
public async Task ShowRenderingUnavailableAsync(GLInitializationState state)
{
    var message =
        "The interactive 3D model preview is not available on this system, so the preview " +
        "pane will stay empty.\n\n";

    //On Windows, the usual cause is a missing OpenGL driver; Microsoft's free "OpenCL and
    //OpenGL Compatibility Pack" adds one. Only show this hint when actually on Windows.
    var osInfo = await SimpleOsInfo.GatherInfo(withConsoleOutput: false);
    if (osInfo.IsWindows)
    {
        message += "On Windows, you may be able to fix this by installing the free Microsoft " +
            "\"OpenCL and OpenGL Compatibility Pack\"...\n\n";
    }

    message += $"Details:\nStatus: {state.Status}\n{state.FailedReason ?? "(none reported)"}";

    using var dialog = CreateDialog(message, "3D Preview Unavailable");
    _ = await dialog.ShowAsync();
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs
//The canvas may only attempt its OpenGL initialization when it loads into the visual
//tree, which can happen after IsModelViewActive is set - so check at both moments.
ModelCanvas.Loaded += (_, _) => _ = MaybeReportRenderingUnavailableAsync();

//When the Model View is active and the preview canvas reports failed OpenGL initialization,
//surface the failure (status + reason) in a dialog instead of leaving a silently empty pane.
private async Task MaybeReportRenderingUnavailableAsync()
{
    if (_renderingUnavailableReported || ViewModel is not { IsModelViewActive: true } viewModel)
    {
        return;
    }

    var state = ModelCanvas.GetGLInitializationState();
    if (state.Status == GLInitializationStatus.InitializationFailed)
    {
        _renderingUnavailableReported = true;
        await viewModel.ShowRenderingUnavailableAsync(state);
    }
}
```

Notice three habits worth copying. Check at two moments - the canvas's `Loaded` and the view's activation - because a collapsed canvas may not attempt initialization until it enters the visual tree. A page-level flag reports the failure once per run, or the dialog reappears on every item the user opens. And the operating-system-specific hint is decided at run time with `SimpleOsInfo` rather than compiled in, so the same message code runs on every head. On Windows the fix that hint points at is the OpenGL Compatibility Pack from the Microsoft Store.

## Persisted settings

Anything that must survive a restart - a chosen folder, a window size, a palette, a recent list - goes through the AppSettings add-in. It is the one add-in that is not a UI control: it draws nothing, references no XAML type, and can be used from a plain class library.

```bash
dotnet add package CodeBrix.Platform.AppSettings.ApacheLicenseForever
```

Reference [CodeBrix.Platform.AppSettings.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Platform.AppSettings.ApacheLicenseForever) from the project that carries your other framework packages - `.Core` in the standard layout. Its dependencies flow in automatically, [CodeBrix.Sqlite.ApacheLicenseForever](https://www.nuget.org/packages/CodeBrix.Sqlite.ApacheLicenseForever) among them, because the store is one portable SQLite file called `settings.sqlite`. There is one `using` and no XAML namespace:

```csharp
    using CodeBrix.Platform.AppSettings;
```

The add-in stores every configurable value as JSON text, and provides no settings screen: your application builds its own, or has none and saves in the background. It works on all six heads, and because the store is a plain file the same `settings.sqlite` can be copied between machines.

### Open the store first

```csharp
    using CodeBrix.Platform.AppSettings;

    public App()
    {
        AppSettingsService.Initialize("MyApp");
        InitializeComponent();
    }
```

`Initialize` constructs the store, and construction runs the whole start-up sequence: adopt a staged import if one is waiting, open with an integrity check and recovery, then take the automatic backup and prune old ones. That is why it belongs before any UI renders and before anything reads a setting.

> [!IMPORTANT]
> The failure mode here is quiet and order-dependent. A static constructor that runs before the store exists gets defaults instead of the user's values, with no error anywhere. Open the store as the first real step of the `App` constructor, and always before `InitializeComponent()`.

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs
//Open (or silently create) the single portable settings.sqlite store -
//including its startup auto-backup and pruning - before anything reads
//a setting. PintaCore's static constructor builds the palette manager,
//which reads settings, so this must come first.
Pinta.Brix.Settings.SettingsService.Initialize();
```

The default location is `{per-user configuration folder}/CodeBrix/{appName}/settings/settings.sqlite`, which on Linux is `~/.config/CodeBrix/{appName}/settings/settings.sqlite` and the equivalent per-user application-data location on Windows and macOS. A two-argument overload takes a folder of your own. The application name becomes a folder name, so it must be non-blank and free of characters that are invalid in a file name. A second `Initialize` throws; `AppSettingsService.Shutdown()` closes the store and permits a later one, which is what a test host needs between cases.

### Read and write

```csharp
    static T    AppSettingsService.Get<T>(string key, T defaultValue)
    static T?   AppSettingsService.Get<T>(string key)
    static void AppSettingsService.Set(string key, object? value)
    static bool AppSettingsService.HasValue(string key)
```

```csharp
    using CodeBrix.Platform.AppSettings;

    // App constructor or Program.Main - once, before any UI renders:
    AppSettingLoggingService.ConsoleOutput = false;   // we have framework logging
    AppSettingsService.Initialize("MyApp");
    // -> {per-user config}/CodeBrix/MyApp/settings/settings.sqlite

    if (AppSettingsService.Store.WasRestoredFromBackup)
        ShowNotice("Your settings were restored from a backup.");

    // Anywhere, on any thread:
    AppSettingsService.Set("MyApp.Window.Width", 1280);
    var width = AppSettingsService.Get("MyApp.Window.Width", 1024);
    if (AppSettingsService.HasValue("MyApp.AssetsFolder")) { ... }
    AppSettingsService.Set("MyApp.AssetsFolder", null);     // removes the key
```

`Set` is not generic: it takes `object?` and serializes by the value's runtime type, with enums written as their names. Every `Set` writes through to the file immediately and synchronously, and a null value removes the key rather than storing null. `Get<T>(key, defaultValue)` returns the default when the key is unset, when the stored JSON deserializes to null, or when it cannot be read as `T` - that last case is logged as a warning and never thrown. Keys are ordinal and case-sensitive; prefix your own with the application name so they stay apart from the one key the package uses for itself.

Anything `System.Text.Json` serializes with default options round-trips, which means a plain object with public get and set properties is a single setting:

```csharp
    // Any plain object with public get/set properties is one JSON value.
    public sealed class EditorOptions
    {
        public string FontFamily { get; set; } = "Roboto Mono";
        public int    FontSize   { get; set; } = 13;
        public bool   WordWrap   { get; set; }
    }

    var options = AppSettingsService.Get("MyApp.Editor", new EditorOptions());
    options.WordWrap = true;
    AppSettingsService.Set("MyApp.Editor", options);   // whole object rewritten
```

Values are text, so a `byte[]` round-trips as base64 and pays that cost. Keep thumbnails and document contents in your own files and store the path in settings.

### Typed handles

For a value read on a hot path, hold a handle: it caches the typed value in memory, writes through on assignment, and raises `Changed` only when the value really changed.

```csharp
    abstract class AppSettingProperty<T>
        T    Value { get; set; }
        bool Set(T newValue)                      // true when it actually changed
        event EventHandler? Changed
        static implicit operator T(AppSettingProperty<T> property)

    abstract class AppSettingProperty                          // the factory
        static AppSettingProperty<T> Create<T>(string key, T defaultValue,
                                               string? oldKey = null)

    static AppSettingProperty<T> AppSettingsService.Wrap<T>(string key, T defaultValue)
```

```csharp
    using CodeBrix.Platform.AppSettings;

    static class Prefs
    {
        // Renamed from "MyApp.Editor.Font" - the old value carries over once.
        public static readonly AppSettingProperty<string> EditorFont =
            AppSettingProperty.Create("MyApp.Editor.FontFamily", "Roboto Mono",
                                      oldKey: "MyApp.Editor.Font");

        public static readonly AppSettingProperty<int> TabSize =
            AppSettingsService.Wrap("MyApp.Editor.TabSize", 4);

        public static readonly AppSettingProperty<DayOfWeek> WeekStart =
            AppSettingsService.Wrap("MyApp.Calendar.WeekStart", DayOfWeek.Monday);
    }

    // ...after AppSettingsService.Initialize("MyApp") has run:
    int tabs = Prefs.TabSize;                // implicit conversion to T
    Prefs.TabSize.Value = 2;                 // writes through, raises Changed
    if (Prefs.TabSize.Set(2)) { /* not reached: unchanged */ }
    Prefs.EditorFont.Changed += (_, _) => ApplyFont(Prefs.EditorFont.Value);
```

Notice the `oldKey` argument: that is the whole rename story for a setting. If a value is stored under the old key it is copied across - only when the new key has no value yet - and the old key is removed either way. Creating a handle requires the service to be initialized, because it reads the current value immediately, so do not build handles in static field initializers that run before `Initialize`. And a handle caches: a direct `AppSettingsService.Set` on the same key is not reflected in an existing handle and does not raise its `Changed`, so pick one access path per key.

### React to a change

```csharp
    using CodeBrix.Platform.AppSettings;

    // One key:
    AppSettingsService.AddSettingHandler("MyApp.Theme", OnThemeChanged);

    void OnThemeChanged(object? sender, AppSettingChangedEventArgs e)
    {
        var theme = AppSettingsService.Get("MyApp.Theme", "Light");
        DispatcherQueue.TryEnqueue(() => ApplyTheme(theme));
    }

    // Every key (for a "settings changed" indicator, or syncing to a server):
    AppSettingsService.Store.SettingChanged += (_, e) =>
        Log($"{e.Key} -> {e.NewValue ?? "(removed)"}");

    // Unsubscribe with the same delegate instance:
    AppSettingsService.RemoveSettingHandler("MyApp.Theme", OnThemeChanged);
```

`Set` raises the store's global `SettingChanged` first and then the handlers registered for that key, and only when the stored JSON actually changed. Both are raised synchronously on the thread that called `Set`, so a handler that touches the UI marshals for itself. `OldValue` on the event args is the previous value as stored JSON text, not a typed value; read the typed current value with `Get<T>` inside the handler.

### Debounce a burst of writes

```csharp
    // Set writes to disk synchronously; coalesce a burst of changes.
    private DispatcherQueueTimer? _saveTimer;

    private void OnSizeChanged(object sender, WindowSizeChangedEventArgs e)
    {
        _saveTimer ??= DispatcherQueue.CreateTimer();
        _saveTimer.Interval = TimeSpan.FromMilliseconds(500);
        _saveTimer.IsRepeating = false;
        _saveTimer.Tick -= SaveSize;
        _saveTimer.Tick += SaveSize;
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    private void SaveSize(DispatcherQueueTimer sender, object args)
    {
        AppSettingsService.Set("MyApp.Window.Width", (int)Bounds.Width);
        AppSettingsService.Set("MyApp.Window.Height", (int)Bounds.Height);
    }
```

Writing on every change is cheaper than it sounds - the store does nothing at all when the value has not changed - but it still serializes the value to find that out, so debounce anything that changes in bursts: window bounds, splitter positions, scroll offsets.

### Restore a window size before any window exists

The heads consult the preferred launch size when they create the native window, so the read happens in the `App` constructor:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs
//Restore the persisted window size BEFORE any window exists - the
//Skia heads consult ApplicationView.PreferredLaunchViewSize when they
//create the native window, and that is the only public seam for the
//initial size. The maximized flag is not restored: the platform exposes
//no public presenter state on the Skia heads.
int windowWidth = Pinta.Brix.Settings.SettingsService.Get("window-size-width", 1100);
int windowHeight = Pinta.Brix.Settings.SettingsService.Get("window-size-height", 750);
Windows.UI.ViewManagement.ApplicationView.PreferredLaunchViewSize =
    new Windows.Foundation.Size(windowWidth, windowHeight);
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs
//Write-through persistence of the window size; the store ignores
//writes when the value is unchanged. args.Size is in logical units
//but the X11 head consumes PreferredLaunchViewSize as NATIVE pixels,
//so the stored value must be native pixels or every restart would
//rescale the window by the display-scale factor.
MainWindow.SizeChanged += (_, args) =>
{
    if (MainWindow.Content?.XamlRoot is not { } root) { return; }

    double scale = root.RasterizationScale;
    Pinta.Brix.Settings.SettingsService.Set("window-size-width", (int)Math.Round(args.Size.Width * scale));
    Pinta.Brix.Settings.SettingsService.Set("window-size-height", (int)Math.Round(args.Size.Height * scale));
};
```

Notice the scale conversion, which is the part that most often goes wrong: the size-changed event reports logical units while the preferred launch size is consumed as native pixels on the X11 head. Multiply by the root's rasterization scale on the way in, or the window shrinks or grows at every restart on a scaled display.

The comment in the first block records that application's own decision not to restore a maximized flag. The presenter itself is reachable from application code: `MainWindow.AppWindow.Presenter` is an `OverlappedPresenter` as soon as the `Window` is constructed, and it is where a minimum or maximum window size goes. See [Set the launch size and a minimum size](#set-the-launch-size-and-a-minimum-size).

### Name the store after your application

One static facade in a small library of its own forwards every call to the add-in. View models call the facade by key, and nothing else in the application talks to the add-in directly.

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Settings/SettingsService.cs
public static class SettingsService
{
    /// <summary>The application name the settings store is registered under.</summary>
    public const string AppName = "KenneyAssetBrowser";

    public static bool IsInitialized => AppSettingsService.IsInitialized;
    public static AppSettingsStore Store => AppSettingsService.Store;
    public static string DefaultDirectory => AppSettingsService.GetDefaultDirectory(AppName);

    /// <summary>
    /// Opens the settings store in the default folder, running the startup
    /// auto-backup and pruning sequence. Call once, before any UI renders.
    /// </summary>
    public static void Initialize() => AppSettingsService.Initialize(AppName);

    public static void Initialize(string directoryPath) =>
        AppSettingsService.Initialize(AppName, directoryPath);

    /// <summary>Closes the store and permits a later Initialize() (test hosts).</summary>
    public static void Shutdown() => AppSettingsService.Shutdown();

    public static AppSettingProperty<T> Wrap<T>(string property, T defaultValue) =>
        AppSettingsService.Wrap(property, defaultValue);

    public static T Get<T>(string property) => AppSettingsService.Get<T>(property);
    public static void Set(string key, object val) => AppSettingsService.Set(key, val);

    public static void AddPropertyHandler(string propertyName, EventHandler<AppSettingChangedEventArgs> handler) =>
        AppSettingsService.AddSettingHandler(propertyName, handler);
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
/// <summary>The settings.sqlite key holding the user's chosen assets folder.</summary>
public const string AssetsFolderKey = "KenneyAssetBrowser.Settings.AssetsFolder";

/// <summary>The settings.sqlite key holding the file name of the last-browsed bundle.</summary>
public const string LastBundleKey = "KenneyAssetBrowser.Settings.LastBundleFile";
```

Notice that every member is a one-line forward. The add-in supplies the whole store - typed properties, change events, start-up auto-backup and pruning, corruption recovery, import and export - so do not re-implement any of it; the facade exists only to name it after your application. Keep keys as constants on the type that owns them, gathered in one place. Keep the layering rule in a project-file comment, too: every persisted value goes through the settings library, and it is the only project that takes the storage dependency.

### The file lifecycle

The store looks after its own file, inside `Initialize`:

```text
  1. If settings_incoming.sqlite is present (staged by StageIncomingFile on a
     previous run), the current settings.sqlite is renamed to
     settings_old_<timestamp>.sqlite (never pruned) and the incoming file takes
     its place; WasReplacedByImport = true.
  2. settings.sqlite is opened and integrity-checked. A missing file is created
     silently (WasCreatedFresh). A file that fails to open or fails
     PRAGMA integrity_check is moved aside as settings_corrupt_<timestamp>.sqlite
     and the newest settings_auto_backup_*.sqlite is copied into place
     (WasRestoredFromBackup); with no usable backup a fresh store is created.
  3. If AutoBackupRetention > 0, a settings_auto_backup_<timestamp>.sqlite is
     written (a checkpointed, self-contained copy) and all but the newest N
     are deleted. Recency comes from the timestamp in the file name (local
     time, TimestampFormat), never from file-system dates; files that do not
     match the naming scheme exactly - a manual copy, a settings_old_ file -
     are never deleted.
```

The three `Was*` flags on `AppSettingsService.Store` are what a start-up routine reads to show a "settings were restored" or "settings were imported" notice. `AutoBackupRetention` is itself a setting, clamped between 0 and 10 with a default of 5; because the backup-and-prune pass runs during construction, a new value takes effect on the next start, and 0 disables automatic backups. Every step logs, and a failed backup never prevents the application from starting.

Export and import are the manual transfer path an application's own settings page offers:

```csharp
    // Export: the user picked a destination with the file-save picker.
    try
    {
        AppSettingsService.Store.ExportToFile(chosenPath);
    }
    catch (InvalidOperationException ex)   // destination inside the settings folder
    {
        ShowError(ex.Message);
    }

    // Import: validated now, adopted on the next start.
    try
    {
        AppSettingsService.Store.StageIncomingFile(pickedFile);
        ShowNotice("Settings will be applied the next time the app starts.");
    }
    catch (InvalidDataException ex)
    {
        ShowError(ex.Message);   // not a SQLite file / no Setting table / corrupt
    }
```

`ExportToFile` writes a safe, complete, self-contained copy - one file, no companions - and refuses a destination inside the settings folder. `StageIncomingFile` copies the chosen file to a private temp location, checks that it is a database that passes its integrity check and holds a readable settings table, and stages it for the next start. An import is never applied live.

### Settings pitfalls

- Calling `Get`, `Set`, `Store`, `Wrap` or `Create` before `Initialize` throws; the facade is not lazily initialized. A second `Initialize` throws too - call `Shutdown` first.
- Keys are case-sensitive: `MyApp.theme` and `MyApp.Theme` are two settings.
- Enums are stored by name, so renaming a member orphans values stored under the old name, and they fall back to the default with a warning. If a setting "keeps resetting itself", look for that warning: the stored JSON is a different shape than `T`.
- Passing `null` to `Set` removes the key. A nullable property that happens to be null deletes rather than storing null.
- There is no encryption: `settings.sqlite` is a plain, portable file. Do not put secrets in it.
- There is no cross-process synchronization. One process owns the file; a second process opening the same folder gets its own in-memory view and its own start-up backup pass.
- The add-in's own log category begins with `CodeBrix.Platform`, so the usual filter that hides framework chatter at Warning also hides its informational lines unless you add a more specific filter. Its console sink is on by default - set `AppSettingLoggingService.ConsoleOutput = false` in a build that already routes framework logging somewhere.

Flush deferred state at points where it has naturally settled - a tool change, a document close - rather than only at exit. A quit-only flush loses everything on a head with no quit command, and wrapping the flush means a failing subscriber cannot take the application down:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/SettingsManager.cs
/// <remarks>
/// Safe and cheap to call often: each PutSetting is a single upsert, and the
/// store does nothing at all when the value has not changed.
/// </remarks>
public void DoSaveSettingsBeforeQuit ()
{
    try {
        SaveSettingsBeforeQuit?.Invoke (this, EventArgs.Empty);
    } catch (Exception ex) {
        // Flushing settings must never take the application down.
        LoggingService.LogError ("Settings could not be saved", ex);
    }
}
```

The [AppSettings add-in page](add-ins/AppSettings.md) has the full API surface.

## Every recipe and the file that shows it

| Recipe | Shown by |
| --- | --- |
| Hand the view model a `XamlRoot` getter | [MediaPlayerDemo MainPage code-behind](https://github.com/ellisnet/CodeBrix.Samples/blob/main/MediaPlayerDemo/src/MediaPlayerDemo.UI/Views/MainPage.xaml.cs) |
| Install dialog handlers into a headless model | [Pinta.Brix ChromeManager](https://github.com/ellisnet/CodeBrix.Samples/blob/main/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/ChromeManager.cs) |
| A progress dialog driven by synchronous code | [Pinta.Brix ContentProgressDialog](https://github.com/ellisnet/CodeBrix.Samples/blob/main/Pinta.Brix/src/libs/Pinta.Brix.Controls/ContentProgressDialog.cs) |
| Open a file through a bridge | [CodeBrixVideoTool IMediaFileBridge](https://github.com/ellisnet/CodeBrix.Samples/blob/main/CodeBrixVideoTool/src/CodeBrixVideoTool.Core/Services/IMediaFileBridge.cs) |
| Save a file through a bridge | [NotionDocumentCreator MainViewModel](https://github.com/ellisnet/CodeBrix.Samples/blob/main/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs) |
| Write beside the source when no dialog exists | [CodeBrixVideoTool IOutputPathBridge](https://github.com/ellisnet/CodeBrix.Samples/blob/main/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/IOutputPathBridge.cs) |
| Save to a default location when no dialog exists | [WebcamPainter MainViewModel](https://github.com/ellisnet/CodeBrix.Samples/blob/main/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs) |
| Decode a picker path and drop its placeholder file | [NotionDocumentCreator FileDialogHelper](https://github.com/ellisnet/CodeBrix.Samples/blob/main/NotionDocumentCreator/src/NotionDocumentCreator.Core/Helpers/FileDialogHelper.cs) |
| Suppress a dialog's own overwrite prompt | [PainDiagram MainWindow code-behind](https://github.com/ellisnet/CodeBrix.Samples/blob/main/PainDiagram/PainDiagram.Wpf/Views/MainWindow.xaml.cs) |
| Choose a folder and remember it | [KenneyAssetBrowser MainViewModel](https://github.com/ellisnet/CodeBrix.Samples/blob/main/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs) |
| Copy to the clipboard from a command | [JustBetweenUs MainViewModel](https://github.com/ellisnet/CodeBrix.Samples/blob/main/JustBetweenUs/Shared/ViewModels/MainViewModel.cs) |
| Open a URL in the default browser | [GitHubIssueFinder MainViewModel](https://github.com/ellisnet/CodeBrix.Samples/blob/main/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs) |
| Follow the desktop's light or dark preference | [GitHubIssueFinder MainPage code-behind](https://github.com/ellisnet/CodeBrix.Samples/blob/main/GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml.cs) |
| A platform service with a no-op default | [Pinta.Brix IClipboardService](https://github.com/ellisnet/CodeBrix.Samples/blob/main/Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/IClipboardService.cs) |
| Invalidate a canvas from a view model | [WebcamPainter MainPage code-behind](https://github.com/ellisnet/CodeBrix.Samples/blob/main/WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml.cs) |
| Marshal a repeating timer into a headless model | [Pinta.Brix ITimerService](https://github.com/ellisnet/CodeBrix.Samples/blob/main/Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/ITimerService.cs) |
| Set the cursor from a model-owned descriptor | [Pinta.Brix PintaCanvas](https://github.com/ellisnet/CodeBrix.Samples/blob/main/Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs) |
| Set the window launch size and a minimum size | [GitHubIssueFinder App.xaml.cs](https://github.com/ellisnet/CodeBrix.Samples/blob/main/GitHubIssueFinder/src/GitHubIssueFinder.UI/App.xaml.cs) |
| Veto a window close until work is saved | [Pinta.Brix App.xaml.cs](https://github.com/ellisnet/CodeBrix.Samples/blob/main/Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs) |
| Drive an embedded browser from a command | [WikipediaPublisher MainViewModel](https://github.com/ellisnet/CodeBrix.Samples/blob/main/WikipediaPublisher/Shared/ViewModels/MainViewModel.cs) |
| An audio transport bound to the element | [KenneyAssetBrowser MainPage](https://github.com/ellisnet/CodeBrix.Samples/blob/main/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml) |
| Explain a failed graphics initialization | [PolyHavenBrowser MainViewModel](https://github.com/ellisnet/CodeBrix.Samples/blob/main/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs) |
| An application-named settings facade | [KenneyAssetBrowser SettingsService](https://github.com/ellisnet/CodeBrix.Samples/blob/main/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Settings/SettingsService.cs) |
| Open the store before anything reads a setting | [Pinta.Brix App.xaml.cs](https://github.com/ellisnet/CodeBrix.Samples/blob/main/Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs) |
| Persist a palette and a recent list | [Pinta.Brix PaletteManager](https://github.com/ellisnet/CodeBrix.Samples/blob/main/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/PaletteManager.cs) |
| Flush deferred settings at natural points | [Pinta.Brix SettingsManager](https://github.com/ellisnet/CodeBrix.Samples/blob/main/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/SettingsManager.cs) |

## Checklist

- [ ] Every bridge interface is declared in the view model's own file, not in a head project
- [ ] Bridges are assigned in `DataContextChanged`, subscribed before `InitializeComponent()`
- [ ] The `XamlRoot` getter is a lambda, wired on every page that could ever show a dialog
- [ ] Every `ContentDialog` sets `XamlRoot`, and only one is on screen at a time
- [ ] A null delegate explains itself; a `NotSupportedException` is caught in the page and returned as null
- [ ] Suggested file names are sanitized before the picker sees them
- [ ] Picker paths are decoded and their empty placeholder removed in the page, before the view model sees them
- [ ] The busy flag is set after a modal picker closes, not before it opens
- [ ] Repaint delegates marshal to the UI thread inside the delegate, with null-conditional calls throughout
- [ ] Every bridge delegate is nulled in `Dispose()`
- [ ] The frame-buffer head opts in to the pickers, on-screen keyboard and clipboard it needs
- [ ] No window behavior the Wayland head cannot provide is load-bearing
- [ ] The launch size is set in the `App` constructor, and the minimum size on the presenter before `Activate()`
- [ ] `AppSettingsService.Initialize` runs before `InitializeComponent()` and before any static constructor reads a setting
- [ ] Setting keys are constants, prefixed with the application name, in one place per owning type
- [ ] Bursty writes are debounced; a window size is stored in native pixels
- [ ] No secret is written to the settings store

---

**Where to go next**

- [08 - Add-ins](08-add-ins.md) - the next chapter: every add-in package and the heads it is live on
- [06 - Views and styling](06-views-and-styling.md) - the page these bridges are wired into
- [AppSettings add-in](add-ins/AppSettings.md) - the settings store's full API
- [12 - Troubleshooting](12-troubleshooting.md) - what a missing picker, a blank browser or a silent dialog usually means
