<sub>[CodeBrix](../../../README.md) › [Build a CodeBrix.Platform application](../README.md) › [Add-ins](../08-add-ins.md) › WebView</sub>

# WebView

**WebView is the add-on that makes the XAML `WebView2` control work on all six Skia heads.** You program against the standard WebView2 contract - `Microsoft.UI.Xaml.Controls.WebView2`, backed by `Microsoft.Web.WebView2.Core.CoreWebView2` - which lives in the core framework package; this add-on supplies the per-head engine behind it. Reference it once in your `.Core` project and a browser appears on Linux, Windows and macOS alike, with navigation, script execution, page-to-host messaging, virtual host mapping and file downloads.

| | |
| --- | --- |
| **Package** | [`CodeBrix.Platform.WebView.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.WebView.ApacheLicenseForever) |
| **Adds** | No new application-facing type. It supplies the engine behind the `WebView2` control and the `CoreWebView2` facade that the core framework already owns |
| **Heads** | All six: Windows (Win32), Skia-on-WPF, Linux X11, Linux native Wayland, Linux frame buffer, and macOS |
| **Requires** | Linux: the system WPE WebKit libraries, installed with apt. Windows: the Microsoft Edge WebView2 runtime on the end-user machine. macOS: nothing (downloads there need macOS 11.3 or later) |

## Add it to your application

Add the package:

```bash
dotnet add package CodeBrix.Platform.WebView.ApacheLicenseForever
```

Reference it once, in your application's `.Core` project, like the other extension add-ons. Every head gets it transitively:

- It activates the WPE path on the three Linux heads.
- It delivers the Microsoft Edge WebView2 payload to the application output on the Windows (Win32) and Skia-on-WPF heads. The Windows-head runtime packages flag themselves so that the package's build logic applies only there; there is nothing for you to configure.
- It is inert on macOS, which already uses the operating system's own WKWebView.

Never reference it from a head project, and never look for a per-head variant. Head projects need nothing extra.

Two things flow in with it: [`CodeBrix.Platform.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.ApacheLicenseForever), which owns the control and the facade, and the Skia package the framework already carries.

> [!IMPORTANT]
> This package and `CodeBrix.Platform.ApacheLicenseForever` must be the same version. The add-in implements internal framework seams, so the core's `InternalsVisibleTo` grants have to match; the whole family is published together for exactly this reason.

There is no XAML namespace to declare: the control is in the default XAML xmlns, so `<WebView2 />` resolves with no prefix. In code-behind:

```csharp
using Microsoft.UI.Xaml.Controls;     // WebView2 (the control; default XAML xmlns)
using Microsoft.Web.WebView2.Core;    // CoreWebView2, CoreWebView2Settings,
                                      // CoreWebView2NavigationStartingEventArgs,
                                      // CoreWebView2NavigationCompletedEventArgs,
                                      // CoreWebView2WebMessageReceivedEventArgs,
                                      // CoreWebView2NewWindowRequestedEventArgs,
                                      // CoreWebView2DownloadStartingEventArgs,
                                      // CoreWebView2DownloadOperation,
                                      // CoreWebView2DownloadState,
                                      // CoreWebView2HostResourceAccessKind,
                                      // CoreWebView2WebErrorStatus
```

Nothing from the add-in's own namespaces is referenced by application code. There is no registration call either. The one relevant global switch is `FeatureConfiguration.ApiInformation.IsFailWhenNotImplemented`, which turns a not-implemented member from a logged message into a thrown `NotImplementedException` - useful while developing.

On Linux, install the engine first:

```bash
sudo apt install libwpewebkit-2.0-1 libwpebackend-fdo-1.0-1 libwpe-1.0-1
```

When any of the three libraries is missing, creating a WebView throws `PlatformNotSupportedException` naming the missing library, its Debian package and that exact apt command.

## Using it

### Put a browser on the page

The whole shared page, working on all six heads:

```xml
<Page x:Class="MyApp.Views.MainPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <WebView2 x:Name="Browser" Source="https://example.com" />
</Page>
```

With an address bar and history buttons:

```xml
<Page x:Class="MyApp.Views.MainPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>
        <StackPanel Orientation="Horizontal" Spacing="8">
            <TextBox x:Name="AddressBox" Width="400" Text="https://example.com" />
            <Button Content="Go" Click="Go_Click" />
            <Button Content="Back" Click="Back_Click" />
        </StackPanel>
        <WebView2 x:Name="Browser" Grid.Row="1"
                  Source="https://example.com" />
    </Grid>
</Page>
```

### Navigate, read the live URL, and run script

The control's `Source` is a dependency property: set it to navigate, and it mirrors the engine only after navigation completes. To learn where you actually are, read `CoreWebView2.Source` - the authoritative current top-level document URL - or the `SourceFromCore` shortcut. `CoreWebView2` is never null; it is created in the control's constructor, and `EnsureCoreWebView2Async()` completes once the native engine view exists.

```csharp
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
        Browser.NavigationStarting += Browser_NavigationStarting;
        Browser.NavigationCompleted += Browser_NavigationCompleted;
        Browser.WebMessageReceived += Browser_WebMessageReceived;
        Loaded += MainPage_Loaded;
    }

    private async void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        await Browser.EnsureCoreWebView2Async();
        Browser.CoreWebView2.Settings.UserAgent = "MyApp/1.0";
        Browser.CoreWebView2.DownloadStarting += CoreWebView2_DownloadStarting;
    }

    private void Go_Click(object sender, RoutedEventArgs e)
        => Browser.Source = new Uri(AddressBox.Text);   // absolute URIs only

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (Browser.CanGoBack) Browser.GoBack();
    }

    private void Browser_NavigationStarting(WebView2 sender,
        CoreWebView2NavigationStartingEventArgs args)
    {
        if (args.Uri is { } uri && uri.StartsWith("http://", StringComparison.Ordinal))
        {
            args.Cancel = true;   // example policy: refuse plain http
        }
    }

    private async void Browser_NavigationCompleted(WebView2 sender,
        CoreWebView2NavigationCompletedEventArgs args)
    {
        // Read the live URL from the engine, not the Source DP.
        AddressBox.Text = sender.CoreWebView2.Source;
        if (args.IsSuccess)
        {
            var title = await Browser.ExecuteScriptAsync("document.title");
            // same value as sender.CoreWebView2.DocumentTitle
        }
        else
        {
            // args.WebErrorStatus / args.HttpStatusCode explain the failure
        }
    }

    private void Browser_WebMessageReceived(WebView2 sender,
        CoreWebView2WebMessageReceivedEventArgs args)
    {
        var text = args.TryGetWebMessageAsString();
        // page called window.chrome.webview.postMessage(text)
    }

    private void CoreWebView2_DownloadStarting(CoreWebView2 sender,
        CoreWebView2DownloadStartingEventArgs args)
    {
        var op = args.DownloadOperation;
        op.StateChanged += (o, _) =>
        {
            if (o.State == CoreWebView2DownloadState.Completed)
            {
                // o.ResultFilePath now exists on disk
            }
        };
        // leave args untouched to accept the default Downloads-folder path,
        // or set args.ResultFilePath / args.Cancel.
    }
}
```

Notice `EnsureCoreWebView2Async()` in `Loaded`: subscribe to `CoreWebView2` events there, or in the constructor, so the very first navigation is not missed. `CanGoBack` and `CanGoForward` are dependency properties kept in sync from `HistoryChanged`. The control also carries `IsScrollEnabled` (default true), `Reload()`, `GoBack()`, `GoForward()`, `NavigateToString(string)`, `NavigateToGoddessUrl()` - which sets `Source` to the built-in default page - and the opt-in `NavigateToGoddessUrlOnLaunch`, which navigates to that page on launch when `Source` is unset.

### Talk to the page, and let the page talk back

Page-to-host messaging supports both idioms; use either from your page script, on any head, and the host receives it in `WebMessageReceived` on the UI thread.

```javascript
window.chrome.webview.postMessage("hello");
window.webkit.messageHandlers.codebrixWebView.postMessage("hello");
```

Host-to-page goes through `ExecuteScriptAsync`. `CoreWebView2.PostWebMessageAsString` and `PostWebMessageAsJson` are not implemented on the Skia heads: they log a not-implemented message, or throw `NotImplementedException` when `FeatureConfiguration.ApiInformation.IsFailWhenNotImplemented` is true.

```csharp
Browser.NavigateToString("""
    <html><body>
      <button onclick="window.chrome.webview.postMessage('clicked')">Click</button>
      <div id="out"></div>
      <script>
        window.receiveFromHost = function (s) {
          document.getElementById('out').textContent = s;
        };
      </script>
    </body></html>
    """);

Browser.WebMessageReceived += async (s, args) =>
{
    if (args.TryGetWebMessageAsString() == "clicked")
    {
        await Browser.ExecuteScriptAsync("window.receiveFromHost('host says hi')");
    }
};
```

`Settings.IsWebMessageEnabled` must be true, which is the default. `TryGetWebMessageAsString()` throws `ArgumentException` when the posted value is not a string; `WebMessageAsJson` gives you the raw form. Note that after `NavigateToString`, `NavigationStarting` reports a `data:text/html;charset=utf-8;base64` URI.

### Serve application-local content

Map a host name onto a folder of content files copied to the application output, then navigate to it.

```csharp
await Browser.EnsureCoreWebView2Async();
Browser.CoreWebView2.SetVirtualHostNameToFolderMapping(
    "app.local", "WebContent", CoreWebView2HostResourceAccessKind.Allow);
Browser.Source = new Uri("https://app.local/index.html");
// "WebContent" is a folder of Content files copied to the app output.
```

`ClearVirtualHostNameToFolderMapping(hostName)` removes a mapping again.

### Downloads

File downloads work on every head through the standard WebView2 contract. A response the engine cannot display, one whose `Content-Disposition` is `attachment`, or an anchor with the HTML5 `download` attribute becomes a download instead of a dead-ended navigation. Left unhandled, the file is saved silently to the user's Downloads folder - the XDG download directory on Linux, `~/Downloads` elsewhere - under a collision-free name, using the `name (1).ext` auto-rename scheme.

`DownloadStarting` is raised on the UI thread and gives you four levers: `Cancel` to refuse, `ResultFilePath` to change the target file, `Handled`, and `GetDeferral()` to decide asynchronously while the download is parked.

```csharp
private async void CoreWebView2_DownloadStarting(CoreWebView2 sender,
    CoreWebView2DownloadStartingEventArgs args)
{
    var deferral = args.GetDeferral();
    try
    {
        var path = await ChooseSavePathAsync();   // your picker code
        if (path is null)
        {
            args.Cancel = true;
        }
        else
        {
            args.ResultFilePath = path;
        }
    }
    finally
    {
        deferral.Complete();   // the parked download proceeds (or is refused)
    }
}
```

`args.DownloadOperation` carries `Uri`, `MimeType`, `ContentDisposition`, `TotalBytesToReceive`, `BytesReceived`, `EstimatedEndTime`, `ResultFilePath`, `State` (`InProgress`, `Interrupted`, `Completed`), `InterruptReason` and `Cancel()`, plus the `BytesReceivedChanged`, `EstimatedEndTimeChanged` and `StateChanged` events. `Pause()` and `Resume()` are not implemented on any head, and `CanResume` is always false; the `DefaultDownloadDialog` APIs are stubs, because the Skia heads draw no built-in download UI.

### A custom User-Agent

On every head, application code can set the User-Agent string the WebView sends; an empty string restores the engine's default. It may be set before or after the control loads, and applies to the next request.

```csharp
myWebView.CoreWebView2.Settings.UserAgent = "MyApp/1.0";
```

This is backed natively everywhere: WPE WebKit on Linux, Edge WebView2 on Windows and WPF, and WKWebView's own custom-user-agent facility on macOS. With no value set, each engine sends its own desktop User-Agent.

### UI-thread rules

`WebView2` is a XAML `Control`: create it, set `Source` and call its methods on the UI thread, like any other control. Every event - `NavigationStarting`, `NavigationCompleted`, `WebMessageReceived`, `DocumentTitleChanged`, `HistoryChanged`, `SourceChanged`, `NewWindowRequested`, `DownloadStarting` and the download-operation progress events - is raised on the UI thread on every head; the Linux engine runs on its own thread and the add-in marshals everything back. You may touch other controls from those handlers directly.

The control's `NavigationStarting`, `NavigationCompleted` and `WebMessageReceived` forward the `CoreWebView2` events of the same name, so subscribe on whichever is handier. `CoreProcessFailed` is declared for contract parity and is never raised on the Skia heads.

### The minimum project

The package is referenced in exactly one place:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.Platform.ApacheLicenseForever" />
    <PackageReference Include="CodeBrix.Platform.WebView.ApacheLicenseForever" />
  </ItemGroup>
</Project>
```

The Win32 head has one requirement of its own - an STA UI thread, which means a synchronous `Main`:

```csharp
using CodeBrix.Platform.UI.Hosting;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseWindowsWin32()
            .Build();
        host.Run();          // synchronous - NOT 'await host.RunAsync()'
    }
}
```

## Per-head notes

- **Linux (X11, Wayland, frame buffer).** Web content is rendered off-screen by the system-installed WPE WebKit engine and composited directly into the Skia scene - no native child windows, no airspace problems: clipping, transforms and z-order behave like any other XAML content. The machine needs `sudo apt install libwpewebkit-2.0-1 libwpebackend-fdo-1.0-1 libwpe-1.0-1`. By design there is no IME, so composed CJK and dead-key text input is unavailable; popup and new-window requests navigate the current view, with `NewWindowRequested` as the hook to intercept them; and the mouse cursor does not change shape over links.
- **Windows (Win32) and Skia-on-WPF.** The Win32 head creates the Edge environment with the user data folder `ApplicationData.Current.LocalFolder\WebView2`; the WPF head hosts the Edge WebView2 WPF control with that control's own defaults. The user data folder is not configurable through the contract: `CoreWebView2Environment.UserDataFolder`, `CreateAsync()` and `GetAvailableBrowserVersionString(...)` are not-implemented stubs, so do not use them for runtime detection. If the Edge WebView2 runtime is absent, environment creation fails and that exception surfaces when the WebView's native view is created, at the control's first layout.
- **Windows (Win32), threading.** The Win32 head requires an STA UI thread. Use a synchronous `[STAThread] static void Main` that calls `host.Run()`; an `async Task Main` silently drops `[STAThread]` and the WebView fails with an `InvalidOperationException` explaining exactly this.
- **macOS.** Inert: WKWebView is built into the operating system and the macOS head already uses it. Nothing to install.
- **Downloads per head.** The Windows heads pass the native Edge WebView2 download straight through. The Linux heads use WebKit's asynchronous decide-destination, which needs a current WPE WebKit build - the Debian packages in the apt line above qualify. macOS uses WKDownload and requires the current `libCodeBrixNativeMac.dylib` from the macOS head package; with an older one the WebView still works and a one-time warning says downloads are disabled.
- **Virtual hosts per head.** On the Windows heads the mapping is passed to the Edge engine. On the Linux heads the add-in resolves navigations to a mapped host itself - `hostName/path` becomes `file://<app install folder>/<folderPath>/<path>` - and the `accessKind` is accepted but not enforced there.

## Pitfalls

- Missing engine on Linux. Creating a WebView throws `PlatformNotSupportedException` such as: "WebView on Linux requires the system WPE WebKit engine, and the library 'libWPEWebKit-2.0.so.1' (Debian package 'libwpewebkit-2.0-1') was not found. To install everything needed on Debian-based distros, run: sudo apt install libwpewebkit-2.0-1 libwpebackend-fdo-1.0-1 libwpe-1.0-1". The library named is the first missing one of the three.
- Mismatched generations. This package and `CodeBrix.Platform.ApacheLicenseForever` must be the same version, because of the internal seams.
- Reading `WebView2.Source` to learn where you are. It is the set-and-bind target and lags redirects and link navigations. Read `CoreWebView2.Source`, or the `SourceFromCore` shortcut, inside navigation callbacks.
- `CoreWebView2.Navigate(string)` requires an absolute URI and throws `ArgumentException` otherwise. The same goes for `new Uri(...)` into `Source`.
- `PostWebMessageAsString` and `PostWebMessageAsJson` do nothing on the Skia heads. Use `ExecuteScriptAsync` for host-to-page.
- Subscribing to `CoreWebView2.DownloadStarting`, or other `CoreWebView2` events, only after the first navigation has finished misses that navigation. Subscribe in the constructor, or right after `EnsureCoreWebView2Async()` in `Loaded`.
- Win32 head with an `async Task Main`: the WebView cannot initialize on an MTA thread. Use `[STAThread]`, a synchronous `Main` and `host.Run()`.
- `CoreWebView2Environment.UserDataFolder`, `CreateAsync` and `GetAvailableBrowserVersionString` are not-implemented stubs. There is no contract-level runtime detection; handle the initialization exception instead.
- Do not reference the package from a head project, and do not try to construct the add-in's own provider types yourself. The framework wires them; they are internal seams, not an API.
- `CoreWebView2.ExecuteScriptAsync` returns null when there is no native view yet.
- Performance on Linux: each rendered web frame is copied into a Skia image and composited, so cost scales with the WebView's pixel size. Keep very large WebViews to what the page needs, and collapse hidden ones rather than leaving them rendering behind other content.
- Performance everywhere: `ExecuteScriptAsync` round-trips to the engine thread on Linux, and to the Edge process on Windows. Batch work into one script call rather than many small ones inside tight loops. `CoreWebView2.Source` and `DocumentTitle` are plain property reads and need no script call. Keep one `WebView2` alive and re-navigate it instead of creating a fresh control per page: engine initialization is the expensive step.
- This add-in does not change the WebView2 API surface, and it ships no browser engine: Linux uses the distribution's WPE WebKit, Windows uses the end user's Microsoft Edge WebView2 runtime, macOS uses the operating system's WKWebView.

## Related

- **WikipediaPublisher** in [CodeBrix.Samples](https://github.com/ellisnet/CodeBrix.Samples/tree/main/WikipediaPublisher) - browses an online encyclopedia in an embedded WebView and publishes the displayed page, driving navigation from a view model that never names a WebView type: the page owns the control and forwards an `Action<string>` and a current-URL callback
- [WebViewDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/WebViewDemo) - an address box with Go, Back and Forward, a modal dialog over the WebView, `NavigationCompleted` reading `CoreWebView2.Source` and `DocumentTitle`, and `DownloadStarting` wired into a status line with `BytesReceivedChanged` and `StateChanged`; a scripted self-test exits PASS or FAIL when a download finishes
- [Platform services](../07-platform-services.md) - the other head-supplied capabilities an application reaches through one contract
- [VideoPlayer](VideoPlayer.md) - the other add-in that composites a full engine's output into the Skia scene

## Documentation and source

| Document | Where |
| --- | --- |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Platform/blob/main/src/AddIns/Platform.UI.WebView.Skia/AGENT-README.txt) |
| The WebView2 contract in the core framework (control plus `CoreWebView2` facade) | [src/Platform.UI/UI/Xaml/Controls/WebView](https://github.com/ellisnet/CodeBrix.Platform/tree/main/src/Platform.UI/UI/Xaml/Controls/WebView) |
| Add-in source (internal seams; the Linux engine binding) | [src/AddIns/Platform.UI.WebView.Skia](https://github.com/ellisnet/CodeBrix.Platform/tree/main/src/AddIns/Platform.UI.WebView.Skia) |
| Sample application - the shared page is `WebViewDemo.UI/Views/MainPage.xaml` | [samples/CodeBrixPlatform/WebViewDemo](https://github.com/ellisnet/CodeBrix.Platform/tree/main/samples/CodeBrixPlatform/WebViewDemo) |
| The not-implemented surface, listed in one place | [NOT-IMPLEMENTED.md](https://github.com/ellisnet/CodeBrix.Platform/blob/main/NOT-IMPLEMENTED.md) |
| Package | [`CodeBrix.Platform.WebView.ApacheLicenseForever`](https://www.nuget.org/packages/CodeBrix.Platform.WebView.ApacheLicenseForever) |

---

**Where to go next**

- [Platform services](../07-platform-services.md) - the next chapter, on reaching head capabilities through one contract
- [Reference applications](../13-reference-applications.md) - complete applications that put a WebView to work
- [All add-ins](../08-add-ins.md) - the whole set at a glance
