<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Add-ins](../08-add-ins.md) › AppSettings</sub>

# AppSettings

**The AppSettings add-in is a persistent application-settings system, and the one add-in that is not a UI control.** It stores every configurable value as JSON text in one portable SQLite file, `settings.sqlite`, and provides no settings screen: your application builds its own, or has none and saves in the background. It draws nothing and references no XAML type, so it can equally be referenced from a plain class library that a CodeBrix.Platform application consumes.

| | |
| --- | --- |
| **Package** | [`CodeBrix.Platform.AppSettings.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.AppSettings.ApacheLicenseForever) |
| **Adds** | `AppSettingsService`, `AppSettingsStore`, `AppSettingProperty<T>`, `AppSettingProperty`, `AppSettingChangedEventArgs`, `AppSettingLoggingService`, `AppSettingLogLevel` - no XAML elements |
| **Heads** | All six. The store is a plain file, so the same `settings.sqlite` can be copied between machines |
| **Requires** | A writable per-user configuration folder, or any folder you pass to `Initialize` |

## Add it to your application

Reference the package from the project that carries your other framework package references - the application's `.Core` project in the standard layout:

```bash
dotnet add package CodeBrix.Platform.AppSettings.ApacheLicenseForever
```

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>MyApp</RootNamespace>
    <DefineConstants>$(DefineConstants);HAS_CODEBRIX;HAS_CODEBRIX_WINUI</DefineConstants>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.Platform.ApacheLicenseForever" />
    <PackageReference Include="CodeBrix.Platform.AppSettings.ApacheLicenseForever" />
  </ItemGroup>
</Project>
```

Three dependencies flow in automatically: `CodeBrix.Platform.ApacheLicenseForever`, which carries the ambient logger the service forwards to; [`CodeBrix.Sqlite.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Sqlite.ApacheLicenseForever), the SQLite engine behind `settings.sqlite`; and [`Microsoft.Extensions.Logging.Abstractions`](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions) for `ILogger` and `ILoggerFactory`.

There is one using, and no XAML namespace, because nothing here is declarable in markup:

```csharp
using CodeBrix.Platform.AppSettings;
```

The application must call `Initialize` once, before any UI renders, in the `App` constructor or in `Program.Main`:

```csharp
using CodeBrix.Platform.AppSettings;

public App()
{
    AppSettingsService.Initialize("MyApp");
    InitializeComponent();
}
```

After that, `AppSettingsService.Get` and `Set` work from any view model. Nothing else is required: no schema, no migration step, no file management.

## Using it

### Start-up and the file location

```csharp
static void   AppSettingsService.Initialize(string appName)
static void   AppSettingsService.Initialize(string appName, string directoryPath)
static void   AppSettingsService.Shutdown()
static bool   AppSettingsService.IsInitialized
static string AppSettingsService.DirectoryPath              // after Initialize
static string AppSettingsService.GetDefaultDirectory(string appName)
```

The default location is `{per-user configuration folder}/CodeBrix/{appName}/settings/settings.sqlite`. The application name becomes a folder name, so it must be non-blank and contain no characters that are invalid in a file name, or `ArgumentException` is thrown.

`Initialize` constructs the store, and construction runs the whole start-up sequence: adopt a staged import if one is waiting, open the file with an integrity check and recovery, then take the automatic backup and prune old ones. A second `Initialize` throws `InvalidOperationException`. `Shutdown()` closes the store and permits a later `Initialize`; it is a no-op when the service was never initialized, so it is safe to call unconditionally at teardown.

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

### Reading and writing

```csharp
static T    AppSettingsService.Get<T>(string key, T defaultValue)
static T?   AppSettingsService.Get<T>(string key)
static void AppSettingsService.Set(string key, object? value)
static bool AppSettingsService.HasValue(string key)
```

There is no generic constraint on `T`, and `Set` is not generic: it takes `object?` and serializes the value by its runtime type with `System.Text.Json`, using the default options plus enums-as-strings. Every `Set` writes through to `settings.sqlite` immediately and synchronously, and a null value removes the key.

`Get<T>(key, defaultValue)` returns the default when the key is unset, when the stored JSON deserializes to null, or when it cannot be read as `T`. In that last case the mismatch is logged as a warning and the default is returned, never thrown. `Get<T>(key)` is the same with `default(T)`.

Keys are ordinal, case-sensitive strings. The one key the package uses for itself is `AppSettingsStore.AutoBackupRetentionKey`, `"CodeBrix.Platform.AppSettings.AutoBackupRetention"`; prefix your own with the application name, as in `"MyApp.Window.Width"`, to keep them apart.

Anything `System.Text.Json` serializes with default options round-trips: strings, integers, booleans, enums (stored as their name, so `DayOfWeek.Friday` is the text `"Friday"`), `byte[]` (stored as a base64 JSON string), and any value type or plain object with public get and set properties.

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

### Typed handles

An `AppSettingProperty<T>` is a typed, in-memory-cached handle over one key, with its own `Changed` event and optional old-key migration.

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

There is no public constructor: obtain a handle through `AppSettingProperty.Create`, which takes the optional old key, or through `AppSettingsService.Wrap`, which is the same handle without migration. Gathering them in one static class keeps every key in the application in one place.

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

A handle caches its value in memory: reading `Value` reads a field, and setting `Value` or calling `Set` writes through to the store, raises the store's change notifications and raises the handle's own `Changed` - but only when the value really changed, compared with `EqualityComparer<T>.Default`.

Old-key migration happens in `Create` when `oldKey` is given: a value stored under the old key is copied to the new one, only when the new key has no value yet, and the old key is removed either way. That is the whole rename story for a setting - change the key, pass the previous name as `oldKey`, and the next run migrates silently.

### Change notification

There are three levels: per-key handlers on the facade or the store, the store's global `SettingChanged`, and the per-handle `Changed`.

```csharp
// per key - facade and store:
static void AppSettingsService.AddSettingHandler(string key,
                EventHandler<AppSettingChangedEventArgs> handler)
static void AppSettingsService.RemoveSettingHandler(string key,
                EventHandler<AppSettingChangedEventArgs> handler)
void AppSettingsStore.AddSettingHandler(string key,
                EventHandler<AppSettingChangedEventArgs> handler)
void AppSettingsStore.RemoveSettingHandler(string key,
                EventHandler<AppSettingChangedEventArgs> handler)

// global - store only:
event EventHandler<AppSettingChangedEventArgs>? AppSettingsStore.SettingChanged

// per handle:
event EventHandler? AppSettingProperty<T>.Changed
```

`Set` raises the store's `SettingChanged` first, then the handlers registered for that key, and only when the stored JSON actually changed - writing the same value again raises nothing. Both are raised synchronously on the thread that called `Set`.

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

`AppSettingChangedEventArgs.OldValue` is the previous value as stored JSON text, or null when the key was not set before; `NewValue` is the object you passed to `Set`, or null when the key was removed. Read the typed current value with `Get<T>` inside the handler rather than casting `NewValue`.

### The store

`AppSettingsService.Store` is the application's store. Its `Set` returns a bool the facade's does not, and its three `Was*` flags tell a start-up routine whether to show a "settings were restored, imported or reset" notice.

```csharp
sealed class AppSettingsStore : IDisposable
    AppSettingsStore(string appName)
    AppSettingsStore(string appName, string directoryPath)
    static string GetDefaultDirectory(string appName)

    string AppName            { get; }
    string DirectoryPath      { get; }     // the settings folder
    string DatabaseFilePath   { get; }     // .../settings.sqlite
    bool   WasCreatedFresh       { get; }  // no usable file existed this start
    bool   WasRestoredFromBackup { get; }  // corrupt file replaced by newest backup
    bool   WasReplacedByImport   { get; }  // a staged import was adopted this start
    int    AutoBackupRetention   { get; set; }   // 0..10, default 5

    bool HasValue(string key)
    T?   Get<T>(string key)
    T    Get<T>(string key, T defaultValue)
    bool Set(string key, object? value)          // true when it changed
    void AddSettingHandler(...)  /  void RemoveSettingHandler(...)
    event EventHandler<AppSettingChangedEventArgs>? SettingChanged

    void ExportToFile(string destinationFilePath)
    void StageIncomingFile(string sourceFilePath)
    void Dispose()
```

Constructing an `AppSettingsStore` yourself gives a second, independent settings file - per-document or per-profile settings. It has the same surface, the facade knows nothing about it, and you dispose it when done.

### Backups and recovery

The file lifecycle runs inside the constructor, which means inside `Initialize`:

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

Every step logs through `AppSettingLoggingService`, and a failed backup or adoption is logged and never prevents the application from starting. So the settings folder holds:

```text
settings.sqlite                          the live store
settings_auto_backup_<stamp>.sqlite      newest N kept (N = AutoBackupRetention)
settings_corrupt_<stamp>.sqlite          quarantined; never pruned
settings_old_<stamp>.sqlite              replaced by an import; never pruned
settings_incoming.sqlite                 staged import; adopted on next start
<stamp> = local time, yyyy-MM-dd_HH-mm-ss
```

`AutoBackupRetention` is itself a setting, clamped to 0..10 on read and write, with `DefaultAutoBackupRetention = 5` and `MaxAutoBackupRetention = 10`. Because the backup-and-prune pass runs during construction, a new value takes effect on the next start; 0 disables automatic backups.

### Export and import

`ExportToFile` writes a safe, complete, self-contained copy - quiesce, WAL checkpoint, SQLite online backup - so the single file is the whole database and needs no companion files. `StageIncomingFile` copies the chosen file to a private temporary location, checks that it is a SQLite database that passes `integrity_check` and holds a readable `Setting` table, and stages a clean copy to be adopted on the next start.

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

The user's file is never opened in place. `StageIncomingFile` throws `FileNotFoundException` for a missing file and `InvalidDataException` for anything that fails validation, and nothing about the running store changes until the application restarts.

### Logging

```csharp
enum AppSettingLogLevel { Info, Warning, Error }

static class AppSettingLoggingService
    const  string LogCategory = "CodeBrix.Platform.AppSettings"
    static bool   ConsoleOutput { get; set; }        // default TRUE
    static void   AddSink(Action<string> sink)                          // replayed
    static void   AddSink(Action<AppSettingLogLevel, string> sink)      // not replayed
    static bool   RemoveSink(Action<string> sink)
    static bool   RemoveSink(Action<AppSettingLogLevel, string> sink)
    static void   LogInfo(string message)
    static void   LogWarning(string message)
    static void   LogError(string message)
    static void   LogError(string message, Exception ex)
```

Every logged line goes to three places: the console while `ConsoleOutput` is true, the framework's ambient logger, and your own sinks. Console lines look like `[HH:mm:ss.fff] INFO : message`, with the labels `INFO`, `WARN` and `ERROR`. The ambient logger is an `ILogger` for the category `CodeBrix.Platform.AppSettings`, at Information, Warning and Error.

`AddSink(Action<string>)` receives the formatted line and is replayed every line logged before it registered, so a diagnostics page opened late still shows the start-up sequence. `AddSink(Action<AppSettingLogLevel, string>)` receives severity plus the bare message so it can filter, and is not replayed. Sinks may be called from any thread.

```csharp
// Replayed: the start-up lines appear even though the page opened late.
AppSettingLoggingService.AddSink(line =>
    DispatcherQueue.TryEnqueue(() => LogLines.Add(line)));

// Severity-aware, not replayed:
AppSettingLoggingService.AddSink((level, message) =>
{
    if (level == AppSettingLogLevel.Error)
        DispatcherQueue.TryEnqueue(() => ErrorBanner.Text = message);
});
```

The four `Log*` methods are public so your own settings screen can log into the same stream.

### Debouncing bursts of writes

Every `Set` is a synchronous write, so coalesce settings that change in bursts: window bounds, splitter positions, scroll offsets.

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

### Testing against a temporary store

The two-argument `Initialize` points the store at any folder, which is what makes the add-in testable.

```csharp
var folder = Path.Combine(Path.GetTempPath(), "MyApp.Tests", Guid.NewGuid().ToString("N"));
AppSettingsService.Initialize("MyApp", folder);
try
{
    AppSettingsService.Set("MyApp.Test", 1);
    Assert.Equal(1, AppSettingsService.Get("MyApp.Test", 0));
}
finally
{
    AppSettingsService.Shutdown();      // permits the next Initialize
    Directory.Delete(folder, recursive: true);
}
```

### What the add-in does not do

- No settings screen, no XAML controls, no bindable settings view model. It is the storage layer your own settings page writes through.
- No encryption and no secure storage: `settings.sqlite` is a plain, portable SQLite file. Do not put secrets in it.
- No binary values: everything is JSON text, and `byte[]` round-trips as base64 at the base64 cost.
- No cross-process synchronization: one process owns the file, and a second process opening the same folder gets its own in-memory view and its own start-up backup pass.
- No schema versioning or typed migrations beyond the per-key rename in `AppSettingProperty.Create`. A change of shape is handled by reading the old form once and writing the new one.
- No roaming or cloud sync. `ExportToFile` and `StageIncomingFile` are the manual transfer path, and an import is never applied live - always on the next start.

## Per-head notes

The add-in works the same on all six heads; only the default folder differs by operating system. On Linux the store is at `~/.config/CodeBrix/{appName}/settings/settings.sqlite`, and on Windows and macOS it is the equivalent per-user application-data location (`Environment.SpecialFolder.ApplicationData`). Pass a folder to `Initialize` when you want it somewhere else.

## Pitfalls

- A second `Initialize` throws `InvalidOperationException`; call `Shutdown` first. `Get`, `Set`, `Store`, `Wrap` and `Create` before `Initialize` also throw - the facade is not lazily initialized.
- Creating a typed handle requires the service to be initialized, because it reads the current value immediately. Do not build handles in static field initializers that run before `Initialize`.
- A handle caches in memory, so a direct `AppSettingsService.Set` on the same key is not reflected in an existing handle and does not raise its `Changed`. Use one access path per key.
- `Get<T>(key, default)` swallows a type mismatch with a warning and the default. If a setting "keeps resetting itself", look for that warning: the stored JSON is probably a different shape than `T`.
- Enums are stored by name, not by number. Renaming a member orphans values stored under the old name, which then fall back to the default. Keep old member names, or migrate with a one-off `Get<string>` and `Set`.
- `Set` takes `object?`, so passing a value through a variable typed as `object` is fine - the runtime type is serialized - but passing null removes the key. A nullable property that is null does not store null, it deletes.
- `AppSettingChangedEventArgs.OldValue` is JSON text, not the previous typed value. Re-read with `Get<T>` for a typed current value.
- Change handlers run synchronously on the thread that called `Set`, so UI work in a handler needs `DispatcherQueue.TryEnqueue` when `Set` can come from a background thread.
- Keys are case-sensitive ordinal strings: `"MyApp.theme"` and `"MyApp.Theme"` are two settings.
- The application name must be a valid folder name; `"My/App"` throws.
- `AutoBackupRetention` is clamped to 0..10 and takes effect on the next start, so lowering it does not delete existing backups on the spot.
- `ExportToFile` refuses a destination inside the settings folder, which holds only the live store and its own backups.
- `StageIncomingFile` changes nothing until the next start; do not expect `Get` to return imported values in the same session.
- `AppSettingLoggingService` writes to the console by default. It also forwards to the framework's ambient logger under the category `CodeBrix.Platform.AppSettings`, so the usual `AddFilter("CodeBrix.Platform", LogLevel.Warning)` line hides its informational lines unless a more specific filter for `AppSettingLoggingService.LogCategory` is added.
- After `Dispose` on a store you constructed yourself, writes throw `ObjectDisposedException`.
- The whole `Setting` table is loaded into memory when the store opens, so `Get` never touches the disk - but it does deserialize the JSON on every call. For a value read on a hot path, hold an `AppSettingProperty<T>`, which caches the typed value.
- Values are JSON text and `byte[]` is base64: keep large binary such as thumbnails or document contents in your own files, and store the path in settings.

## Related

- [CodeBrix.Sqlite](../../libraries/CodeBrix.Sqlite.md) - the SQLite engine behind `settings.sqlite`, and what to use directly when your data outgrows key-value settings
- [Platform services](../07-platform-services.md) - the pickers and window services a settings page is usually built from
- GitHubIssueFinder, KenneyAssetBrowser and Pinta.Brix in [the sample applications](../../samples/README.md) - each wraps the add-in in one application-named facade, then persists through it what its own window has to reopen with: a picked folder, a window size, a palette, per-tool options, or the search terms and the chosen color scheme
- [AdvancedTextEdit](AdvancedTextEdit.md) - a control whose font, tab size and word-wrap state are natural settings to persist

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.AppSettings/AGENT-README.txt) |
| Add-in source | [src/AddIns/Platform.AppSettings](https://github.com/ellisnet/CodeBrix.Platform/tree/main/src/AddIns/Platform.AppSettings) |
| Tests, the best worked examples of every API | [src/AddIns/Platform.AppSettings.Tests](https://github.com/ellisnet/CodeBrix.Platform/tree/main/src/AddIns/Platform.AppSettings.Tests) |
| Package | [`CodeBrix.Platform.AppSettings.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.AppSettings.ApacheLicenseForever) |

---

**Where to go next**

- [Platform services](../07-platform-services.md) - file and folder pickers, windows and dispatchers, the rest of what a settings page needs
- [CodeBrix.Sqlite](../../libraries/CodeBrix.Sqlite.md) - the storage library underneath, for application data that is more than settings
- [All add-ins](../08-add-ins.md) - the whole set at a glance
