<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.SSH</sub>

# CodeBrix.SSH

**CodeBrix.SSH is a fully managed, cross-platform Secure Shell (SSH-2) client library.** It provides
remote command execution, an interactive shell with or without a pseudo-terminal, SFTP file transfer
and remote file system operations, SCP upload and download, local, remote and dynamic (SOCKS) port
forwarding, and a NETCONF-over-SSH subsystem client. Use it from any .NET 10 application, or from a
CodeBrix.Platform application that automates or opens a shell on a remote host.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.SSH](https://github.com/ellisnet/CodeBrix.SSH) |
| **Packages** | [`CodeBrix.SSH.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.SSH.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later. No native prerequisites: everything is managed code, with no P/Invoke into an SSH installation. Two dependencies arrive with the package - [`CodeBrix.Cryptography.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Cryptography.MitLicenseForever) and `Microsoft.Extensions.Logging.Abstractions` |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Anywhere .NET 10 runs. The only operating-system-specific behavior is the default known-hosts path |

## What it does

- Executes remote commands synchronously and asynchronously, with streaming output, an exit status, a
  signal name, and a stdin stream you can write into.
- Opens an interactive shell as a `Stream`, with or without a pseudo-terminal, and resizes it while it
  is live.
- Transfers files over SFTP, synchronously and asynchronously, with progress reporting, seekable remote
  streams, and a method surface shaped like `System.IO.File`.
- Transfers files over SCP, including whole directories.
- Forwards ports three ways: local, remote, and dynamic as a local SOCKS4/SOCKS5 proxy.
- Authenticates by public key, password or keyboard-interactive, including multi-factor sequences in
  the order *you* supply them.
- Reads private keys in the OpenSSH, PKCS#1 (OpenSSL traditional PEM), PKCS#8, PuTTY and ssh.com
  formats, encrypted or not, including OpenSSH certificates.
- Connects through a SOCKS4, SOCKS5 or HTTP proxy.
- Verifies host keys against an OpenSSH `known_hosts` file, with ready-made strict and
  trust-on-first-use policies.
- Speaks a NETCONF-over-SSH subsystem through `NetConfClient`.
- Negotiates a broad, mutable algorithm set: post-quantum hybrid and elliptic-curve key exchange,
  AEAD and CTR ciphers, SHA-2 MACs including encrypt-then-MAC, and Ed25519, ECDSA and RSA host keys
  with their certificate variants.

<details>
<summary>The negotiated algorithms, by family</summary>

| Family | Algorithms |
| --- | --- |
| Key exchange | `mlkem768x25519-sha256`, `sntrup761x25519-sha512`, `sntrup761x25519-sha512@openssh.com`, `curve25519-sha256`, `curve25519-sha256@libssh.org`, `ecdh-sha2-nistp256`, `ecdh-sha2-nistp384`, `ecdh-sha2-nistp521`, `diffie-hellman-group-exchange-sha256`, `diffie-hellman-group16-sha512`, `diffie-hellman-group18-sha512`, `diffie-hellman-group14-sha256`, `diffie-hellman-group-exchange-sha1`, `diffie-hellman-group14-sha1`, `diffie-hellman-group1-sha1` |
| Encryption | `aes128-ctr`, `aes192-ctr`, `aes256-ctr`, `aes128-gcm@openssh.com`, `aes256-gcm@openssh.com`, `chacha20-poly1305@openssh.com`, `aes128-cbc`, `aes192-cbc`, `aes256-cbc`, `3des-cbc` |
| Host keys | `ssh-ed25519`, `ecdsa-sha2-nistp256`, `ecdsa-sha2-nistp384`, `ecdsa-sha2-nistp521`, `rsa-sha2-512`, `rsa-sha2-256`, `ssh-rsa`, each with its `-cert-v01@openssh.com` certificate variant |
| MAC | `hmac-sha2-256`, `hmac-sha2-512`, `hmac-sha1`, `hmac-sha2-256-etm@openssh.com`, `hmac-sha2-512-etm@openssh.com`, `hmac-sha1-etm@openssh.com` |
| Compression | `none`, which is the default, and `zlib@openssh.com` |

Private keys are read as RSA from traditional PEM, PKCS#8, ssh.com, OpenSSH and PuTTY files; ECDSA
over the nistp256, nistp384 and nistp521 curves from traditional PEM, PKCS#8, OpenSSH and PuTTY; and
ED25519 from PKCS#8, OpenSSH and PuTTY.

</details>

## When to use it

Reach for CodeBrix.SSH when a .NET program has to *be* the SSH client: run a command on a fleet of
hosts, ship a build artifact over SFTP, tunnel a database port through a bastion, or host a live
terminal against a remote machine. It is a client library and nothing else.

What it does not do:

- It is not a server. There is no host-side session handling and no way to accept inbound connections.
- It does not read the OpenSSH client configuration file. Hosts, ports, users, identity files and
  every other client option are supplied programmatically; only the `known_hosts` *format* is
  understood.
- It has no agent support. Keys come from files, streams or `Key` instances, and agent forwarding is
  not implemented.
- It does not validate a host certificate against a certificate authority. Client-side certificate
  authentication is supported, and a host certificate the server sends is exposed to you, but nothing
  checks its chain - that is your code's job.
- It has no GSSAPI or Kerberos authentication, and no host-based authentication.
- It has no multi-hop chaining as a feature. Reaching a second host means either a SOCKS or HTTP proxy
  through `ProxyTypes`, or a local forwarded port on the first connection that a second client
  connects through.
- It does not synchronize directories in any general sense, does not resume transfers, and does not
  limit bandwidth.
- It does not pool connections, reconnect after a failure, or recover a session. If a session drops,
  you construct and connect a new client.
- It does not forward X11.

For managing containers on a remote host, [CodeBrix.Docker](CodeBrix.Docker.md) reaches a remote daemon
over its own `ssh://` transport, which runs the operating system's SSH binary rather than this library.

## Getting started

```bash
dotnet add package CodeBrix.SSH.MitLicenseForever
```

The package ID carries the license suffix; the namespaces do not. You install
`CodeBrix.SSH.MitLicenseForever` and you write `using CodeBrix.SSH;`. Almost all consumer code needs
only these four:

```csharp
using CodeBrix.SSH;             // clients, connection info, auth methods,
                                // forwarded ports, PrivateKeyFile,
                                // SshCommand, Shell, ShellStream
using CodeBrix.SSH.Common;      // exceptions, event args, TerminalModes
using CodeBrix.SSH.Sftp;        // ISftpFile, SftpFile, SftpFileAttributes,
                                // SftpFileStream, SftpFileSystemInformation
using CodeBrix.SSH.KnownHosts;  // KnownHostsStore and the verification
                                // policy extension methods
```

There is no registration call of any kind. Connect, run a command, read the result:

```csharp
using CodeBrix.SSH;

using (var client = new SshClient("sftp.foo.com", "guest", new PrivateKeyFile("path/to/my/key")))
{
    client.Connect();
    using SshCommand cmd = client.RunCommand("echo 'Hello World!'");
    Console.WriteLine(cmd.Result); // "Hello World!\n"
}
```

Two things in those five lines matter beyond the obvious. Every client is `IDisposable` and `Dispose()`
disconnects, so `using` is not optional. And the command object is disposable too: it holds a channel,
and leaking channels is the most common way to hang a program against this library.

> [!WARNING]
> With no `HostKeyReceived` subscriber the client accepts **any** host key, because
> `HostKeyEventArgs.CanTrust` starts as `true`. That is a man-in-the-middle exposure. Decide a policy
> deliberately - see [Host key verification](#host-key-verification) - before the first `Connect()` in
> anything that is not a throwaway tool.

Deeper namespaces exist for the algorithm layer (`CodeBrix.SSH.Security` and its children), the wire
messages (`CodeBrix.SSH.Messages`), compression, connection plumbing and NETCONF, but a consumer
rarely names them.

<details>
<summary>The minimum viable project</summary>

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CodeBrix.SSH.MitLicenseForever" />
  </ItemGroup>
</Project>
```

The library carries its own cryptography; do not add a separate cryptography package to the project.

</details>

## Key concepts

### The client family

`SshClient`, `SftpClient`, `ScpClient` and `NetConfClient` all derive from `BaseClient`, which owns the
connection lifecycle and is where the connection-level events live.

```csharp
public abstract class BaseClient : IBaseClient
{
    public ConnectionInfo ConnectionInfo { get; }
    public virtual bool IsConnected { get; }
    public TimeSpan KeepAliveInterval { get; set; }

    public event EventHandler<ExceptionEventArgs> ErrorOccurred;
    public event EventHandler<HostKeyEventArgs> HostKeyReceived;
    public event EventHandler<SshIdentificationEventArgs> ServerIdentificationReceived;

    public void Connect();
    public Task ConnectAsync(CancellationToken cancellationToken);
    public void Disconnect();
    public void SendKeepAlive();
    public void Dispose();
}
```

Each concrete client offers the same five constructor shapes: a `ConnectionInfo`; host, username and
password; host, port, username and password; host, username and key files; host, port, username and
key files. The convenience constructors build a `PasswordConnectionInfo` or a
`PrivateKeyConnectionInfo` internally and own it, so it is disposed with the client. Pass a
`ConnectionInfo` explicitly when you need multi-factor authentication, a proxy, altered timeouts or a
restricted algorithm set.

`KeepAliveInterval` is disabled by default and must be set before `Connect()`; 30 seconds is a
reasonable value for a long-lived interactive session, and it is what keeps NAT and firewall state
alive on an idle connection. `ErrorOccurred` surfaces errors raised on the session's background
threads - errors that have no call to throw out of. `IsConnected` on `SftpClient` is stricter than on
the others: it also requires the SFTP subsystem session to be open.

The client identification string this library sends is rooted at `SSH-2.0-CodeBrix.SSH.SshClient.`, so
a server that filters on the client identification string has to allow it.

### Running commands

```csharp
public sealed class SshCommand : IDisposable
{
    public string CommandText { get; }
    public TimeSpan CommandTimeout { get; set; }   // default: infinite

    public int? ExitStatus { get; }                // null until reported
    public string ExitSignal { get; }              // e.g. "TERM", "KILL"

    public Stream OutputStream { get; }            // stdout, a PipeStream
    public Stream ExtendedOutputStream { get; }    // stderr
    public Stream CreateInputStream();             // stdin

    public string Result { get; }                  // reads OutputStream
    public string Error { get; }                   // reads ExtendedOutputStream

    public string Execute();
    public string Execute(string commandText);
    public Task ExecuteAsync(CancellationToken cancellationToken = default);

    public IAsyncResult BeginExecute();
    public IAsyncResult BeginExecute(AsyncCallback callback);
    public IAsyncResult BeginExecute(AsyncCallback callback, object state);
    public IAsyncResult BeginExecute(string commandText, AsyncCallback callback,
                                     object state);
    public string EndExecute(IAsyncResult asyncResult);

    public void CancelAsync(bool forceKill = false, int millisecondsTimeout = 500);
    public void Dispose();
}
```

`RunCommand(text)` is `CreateCommand(text)` followed by `Execute()`, and returns the completed command
for you to dispose. `Result` and `Error` are lazy: the first read drains the corresponding stream to
the end and caches the string, so read each once. `ExitStatus` is null when the server reported none,
and a command killed by a signal reports `ExitSignal` instead.

Cancellation does not abandon the command locally - it sends a `TERM` signal to the remote process and
then completes the task as canceled; `CancelAsync` does the same explicitly, with `forceKill`
selecting `KILL` over `TERM`.

Each command opens a fresh channel on the already-authenticated connection, so it is safe to run a
probing command while an interactive shell is live on the same client: the two multiplex without
interfering in either direction. `ConnectionInfo.MaxSessions` caps how many session channels may be
open at once, and an attempt past the cap blocks until one closes.

### Authentication

`ConnectionInfo` carries the host, port, username, proxy settings, timeouts, encoding, the negotiable
algorithm sets, and the ordered list of authentication methods to try. Its defaults are a 30-second
`Timeout`, a one-second `ChannelCloseTimeout`, UTF-8 `Encoding`, ten `RetryAttempts` and ten
`MaxSessions`, with port 22 in the short constructor overloads.

The methods are `PasswordAuthenticationMethod`, `PrivateKeyAuthenticationMethod`,
`KeyboardInteractiveAuthenticationMethod` and `NoneAuthenticationMethod`. The last one is chiefly a
probe: servers answer it with the list of authentications they will accept, which lands in
`AllowedAuthentications`.

Passing several methods drives multi-factor authentication. The library tries *your* methods in the
order you supplied them, filtered to the ones the server currently allows - your order wins over the
server's - and a partial success re-reads the server's updated list and continues. Exhausting the
methods throws `SshAuthenticationException`.

Keyboard-interactive prompts arrive as an `AuthenticationPrompt` list on an event; answer every one,
because leaving a `Response` null throws, and note that prompts are answered in ascending `Id` order
regardless of list order.

The algorithm dictionaries are ordered and mutable: remove an entry to refuse that algorithm, or
re-add it to change preference order. They are filled with the library's defaults by the constructor,
so mutate them *after* constructing the `ConnectionInfo` and *before* connecting.

```csharp
var info = new ConnectionInfo("host", "user", new PasswordAuthenticationMethod("user", "pw"));
_ = info.Encryptions.Remove("3des-cbc");
_ = info.HmacAlgorithms.Remove("hmac-sha1");
_ = info.KeyExchangeAlgorithms.Remove("diffie-hellman-group1-sha1");
```

After the key exchange, `ConnectionInfo` reports what was actually negotiated through
`CurrentKeyExchangeAlgorithm`, `CurrentHostKeyAlgorithm`, `CurrentClientEncryption`,
`CurrentServerEncryption` and their MAC and compression peers - which is what to log when a connection
succeeds but you want to know how.

### Private keys and certificates

`PrivateKeyFile` reads a key from a file name, a `Stream` or an existing `Key`, with or without a
passphrase, and optionally alongside a certificate file. It exposes the parsed `Key`, the
`Certificate` when one was supplied, and the `HostKeyAlgorithms` it can offer. An encrypted key opened
with a null or empty passphrase throws `SshPassPhraseNullOrEmptyException`.

Passing a certificate alongside the key enables OpenSSH certificate authentication: the client then
offers the `-cert-v01@openssh.com` host key algorithms. `Certificate` exposes the serial, the type,
the key id, the valid principals, the validity window, critical options, extensions and the
certificate authority key with its fingerprint.

### Host key verification

`HostKeyEventArgs.CanTrust` is initialized to `true`. The accept-everything policy is therefore the
default, and it is one line to make explicit for a throwaway development tool:

```csharp
client.HostKeyReceived += (sender, e) => e.CanTrust = true;
```

For anything else, the `CodeBrix.SSH.KnownHosts` namespace has the policy ready-made. The extension
methods hang off `BaseClient`, so the SSH, SFTP, SCP and NETCONF clients all get them:

```csharp
using CodeBrix.SSH.KnownHosts;

var store = new KnownHostsStore(KnownHostsStore.DefaultFilePath);

client.UseStrictHostKeyVerification(store);   // known keys only
client.UseTrustOnFirstUse(store);             // TOFU

client.UseTrustOnFirstUse(store, mismatch =>
{
    // Changed or revoked key -- warn, return true to proceed anyway.
    ShowHostKeyWarning(mismatch.Host, mismatch.Port,
                       mismatch.FingerPrintSHA256, mismatch.Result);
    return false;
});
```

Call one policy method per client, after construction and before `Connect()`. Each call adds a
`HostKeyReceived` handler, so calling twice stacks handlers and the most restrictive outcome wins -
any handler can veto trust.

Strict verification trusts only keys already in the store and never writes to it; unknown, mismatched
and revoked keys are all rejected. Trust-on-first-use trusts known keys, trusts and records unknown
keys (saving immediately), and rejects a mismatch or a revocation - or defers those to the callback
overload, whose return value decides. Accepting a mismatch never modifies the store.

`KnownHostsStore` reads and writes the OpenSSH `known_hosts` format: plain and `[host]:port` entries,
comma-separated host lists, `*` and `?` wildcards, `!` negation, hashed host lines and the `@revoked`
marker are all honored when reading, and entries it appends are plain-hostname lines.
`@cert-authority` lines are ignored. `DefaultFilePath` resolves the user's `.ssh/known_hosts` on every
supported operating system; point the store there, or at an application-specific file, which `Save()`
creates on demand with owner-only permissions where the file system supports them. A missing file is
not an error - it is an empty store - and `Verify`, `Add` and `Save` are thread-safe.

### SFTP

`SftpClient` is the file-transfer client, and its method surface deliberately mirrors `System.IO.File`:
`Open`, `OpenRead`, `OpenWrite`, `Create`, `OpenText`, `CreateText`, `AppendText`, `ReadAllBytes`,
`ReadAllText`, `ReadAllLines`, `ReadLines`, `WriteAllBytes`, `WriteAllText`, `WriteAllLines`,
`AppendAllText` and `AppendAllLines`, alongside `UploadFile` / `DownloadFile` and their asynchronous
and progress-reporting overloads. Navigation and metadata are `ChangeDirectory`, `Get`, `Exists`,
`GetAttributes`, `SetAttributes`, `ChangePermissions`, the last-access and last-write time members and
`GetStatus`; directory work is `ListDirectory`, `ListDirectoryAsync`, `CreateDirectory`,
`DeleteDirectory`, `DeleteFile`, `Delete`, `RenameFile` and `SymbolicLink`.

`BufferSize` defaults to 32 KB and is the single biggest transfer knob, because each read or write is
a request and a response. There is a ceiling: the effective length is bounded by the channel's packet
size, and the local packet size is 64 KB, so raising `BufferSize` past that buys nothing for reads.
`OperationTimeout` defaults to infinite, which an unattended process should not leave alone.

`SftpFileStream` is a real seekable stream over a remote file, so it composes with `StreamReader`,
`StreamWriter`, `JsonSerializer`, `CopyToAsync` and anything else that takes a `Stream` - which is how
you move a large file without materializing it. `ISftpFile` exposes the attributes, the type flags and
the permission bits; its setters mutate the in-memory attributes only, and `UpdateStatus()` pushes
them to the server and re-reads them.

`SynchronizeDirectories` is the one synchronization helper and is deliberately minimal: it is one-way
from local to remote, non-recursive, compares by file size only, never deletes anything on the remote
side, and returns the `FileInfo` of each file it uploaded.

### SCP

`ScpClient` uploads and downloads a `Stream`, a `FileInfo` or a `DirectoryInfo`, reports progress
through `Uploading` and `Downloading` events, and buffers at 16 KB by default. It is synchronous only -
there are no asynchronous overloads, so use SFTP when you need cancellation or an `IProgress<T>`.

The remote path is interpolated into a shell command line on the server, which is what
`RemotePathTransformation` exists to make safe. It defaults to `RemotePathTransformation.DoubleQuote`;
`ShellQuote` is the safest general choice, and `None` with untrusted input is a command injection.
`UseDirectoryFlag` controls an undocumented flag on the remote command line: leave it true, and set it
false only for servers whose `scp` rejects the flag.

### Interactive shell

`ShellStream` is a `Stream` over an interactive shell session, obtained from
`SshClient.CreateShellStream` or `CreateShellStreamNoTerminal` - the latter requests a shell without a
pseudo-terminal. It serves two distinct consumption styles, and mixing them is where the surprises
are.

**Scripted automation** uses `Expect`, `ReadLine` and `WriteLine`. `Expect` blocks until the pattern
appears in the buffered text and returns everything up to and including the match; the `ExpectAction`
overloads dispatch to the first pattern that matches.

```csharp
shell.Expect("$ ");
shell.WriteLine("sudo -k systemctl restart nginx");
shell.Expect(new ExpectAction("[Pp]assword", _ => shell.WriteLine(password)),
             new ExpectAction("$ ", _ => { }));
```

**Terminal hosting** uses a dedicated reader thread in a blocking `Read()` loop. `Read()` blocks until
output arrives and returns 0 when the channel closes, which doubles as the disconnect signal.

```csharp
var buffer = new byte[4096];
int n;
while ((n = shell.Read(buffer, 0, buffer.Length)) > 0)
{
    terminal.Feed(buffer, 0, n);    // render the chunk
}
// n == 0: channel closed -- tear down the session UI here.
```

Four members decide how a hosted terminal behaves:

- `AutoFlush` and the `WriteAndFlush` overloads. The byte-oriented writes accumulate in a write buffer
  and reach the wire only on `Flush()` or when the buffer fills; nothing fails when `Flush` is
  forgotten, and the session sits silent. `Write(string)` and `WriteLine(string)` always flush.
- `ChangeWindowSize(columns, rows, width, height)` sends the window-change request for live resizing.
  Call it whenever the hosting control is resized; after `Read` and `Write` it is the most important
  member for interactive use, and column and row dimensions override the pixel dimensions when
  non-zero.
- `DisableReadBuffering`, which delivers incoming data solely through `DataReceived` and never commits
  it to the read buffer. While it is set, the buffer-reading members throw `InvalidOperationException`
  rather than block on a buffer that will never fill. Set it once, immediately after creating the
  stream.
- The terminal mode map. The seven-parameter `CreateShellStream` overload takes
  `IDictionary<TerminalModes, uint>`, so passing
  `new Dictionary<TerminalModes, uint> { { TerminalModes.ECHO, 0 } }` requests a terminal with echo
  off - which is what you want when the host application is already echoing keystrokes.

`WriteLine` sends `\r` after the text on a pseudo-terminal stream and `\n` on a stream created without
one, matching what each remote side expects. The older `Shell` class, which pumps input, output and
extended-output streams you supply up front, is still there; prefer `ShellStream` unless you
specifically want that shape.

### Port forwarding

Three classes, one lifecycle. `ForwardedPortLocal` is the local-to-remote tunnel, `ForwardedPortRemote`
the reverse, and `ForwardedPortDynamic` a local SOCKS4/SOCKS5 proxy. All derive from `ForwardedPort`
with `Start()`, `Stop()`, `Dispose()` and the `Closing`, `Exception` and `RequestReceived` events.

The order is fixed: connect the client, `AddForwardedPort`, then `Start()`. `AddForwardedPort` throws
if the client is not connected, and `Start()` throws if the port was never added to a client. Tear
down with `Stop()` and `RemoveForwardedPort`; disconnecting or disposing the client stops its
forwarded ports for you.

Two details worth knowing: passing a bound port of 0 to `ForwardedPortLocal` asks the operating system
for a free port, which you read back from `BoundPort` after `Start()`; and the overload that leaves the
bound host empty resolves it broadly, so pass `"127.0.0.1"` explicitly when a tunnel must stay private
to the machine. `ForwardedPortDynamic` with no host binds every local interface.

Errors on a tunnel's own background threads surface only through the `Exception` event. There is no
call to catch them from, so always subscribe.

### Logging and the error model

Logging is opt-in and a no-op until you provide a factory: `SshNetLoggingConfiguration.InitializeLogging`
sets one process-wide, and `ConnectionInfo.LoggerFactory` sets one per connection, taking precedence
for that connection. Logger categories are full type names rooted at `CodeBrix.SSH.`, so filter on that
prefix - and do not wire a verbose logger into a hot path, because the library logs every message sent
and received at trace level.

All library exceptions derive from `CodeBrix.SSH.Common.SshException`:

| Exception | Means |
| --- | --- |
| `SshConnectionException` | the connection failed or was closed; carries `DisconnectReason` |
| `SshAuthenticationException` | authentication was refused, or no suitable method was found |
| `SshOperationTimeoutException` | an operation exceeded its timeout |
| `SshPassPhraseNullOrEmptyException` | an encrypted key was opened with no passphrase |
| `SftpException` | an SFTP status code; `SftpPathNotFoundException` and `SftpPermissionDeniedException` derive from it |
| `ScpException`, `ProxyException`, `NetConfServerException` | the SCP, proxy and NETCONF layers |

Argument validation throws the usual base-class-library types, a disposed client throws
`ObjectDisposedException`, and misuse of the streaming APIs throws `InvalidOperationException`. Errors
raised on the library's background threads cannot be caught at a call site: they arrive on
`BaseClient.ErrorOccurred`, `ShellStream.ErrorOccurred`, `Shell.ErrorOccurred` or
`ForwardedPort.Exception`, each carrying a single `Exception`.

### Testing seams

Every client is constructed against a concrete socket, so unit tests should depend on the interfaces
rather than the classes. `ISshClient` declares the forwarded-port members, `CreateCommand`,
`RunCommand` and every shell overload; `ISftpClient` declares the whole SFTP surface plus
`BufferSize`, `OperationTimeout`, `ProtocolVersion` and `WorkingDirectory`. Take those in your own
types, mock them, and construct the concrete client only at the composition root.

`ScpClient` and `NetConfClient` have no interface and their members are not virtual, so wrap them in an
interface of your own if you need to test around them. `IRemotePathTransformation` is a one-method
interface and `ForwardedPort` an abstract class, so both are substitutable.

## Examples

Uploading a file and listing a directory over SFTP.

```csharp
using CodeBrix.SSH;
using CodeBrix.SSH.Sftp;

using (var client = new SftpClient("sftp.foo.com", "guest", "pwd"))
{
    client.Connect();

    using (FileStream fs = File.OpenRead("/tmp/test-file.txt"))
    {
        client.UploadFile(fs, "/home/guest/test-file.txt");
    }

    foreach (ISftpFile file in client.ListDirectory("/home/guest/"))
    {
        Console.WriteLine($"{file.FullName} {file.LastWriteTime}");
    }
}
```

Streaming a long-running command instead of buffering it, which is the difference between watching a
log and waiting for one.

```csharp
using SshCommand cmd = client.CreateCommand("tail -n 200 -f /var/log/syslog");
Task running = cmd.ExecuteAsync(cancellationToken);

using (var reader = new StreamReader(cmd.OutputStream, Encoding.UTF8))
{
    string line;
    while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
    {
        Console.WriteLine(line);
    }
}

await running;
```

A local port forward, start to finish, with the lifecycle in the only order that works.

```csharp
using System;
using CodeBrix.SSH;

namespace Demo;

public static class TunnelDemo
{
    public static void WithTunnel(string gateway, string user, string keyPath,
                                  Action<uint> useLocalPort)
    {
        using var client = new SshClient(gateway, user, new PrivateKeyFile(keyPath));
        client.HostKeyReceived += (sender, e) => e.CanTrust = true;
        client.Connect();                    // must be connected BEFORE AddForwardedPort

        // boundPort 0 -> let the OS pick; read BoundPort after Start().
        var tunnel = new ForwardedPortLocal("127.0.0.1", 0, "db.internal", 5432);
        tunnel.Exception += (sender, e) =>
            Console.Error.WriteLine($"tunnel error: {e.Exception.Message}");
        tunnel.RequestReceived += (sender, e) =>
            Console.WriteLine($"client {e.OriginatorHost}:{e.OriginatorPort}");

        client.AddForwardedPort(tunnel);
        tunnel.Start();

        try
        {
            // Connect your database driver to 127.0.0.1:tunnel.BoundPort
            useLocalPort(tunnel.BoundPort);
        }
        finally
        {
            tunnel.Stop();
            client.RemoveForwardedPort(tunnel);
            tunnel.Dispose();
        }
    }
}
```

That sample trusts any host key so it stays short; in real code, replace that line with one of the
[host key policies](#host-key-verification).

## Using it in a CodeBrix.Platform application

CodeBrix.SSH is a managed network client with no UI, no native assets and no head-specific behavior,
so it goes wherever your other services go - typically the `.Core` library, behind an interface, with
no registration step of its own.

Hosting a live terminal is the one place where the UI shapes the code. Read on a dedicated thread in a
blocking `Read()` loop and hand each chunk to the control that renders it; set `AutoFlush` or use
`WriteAndFlush` so keystrokes reach the wire; call `ChangeWindowSize` from the control's size-changed
handler; and request `TerminalModes.ECHO = 0` when the control echoes locally. The bytes coming back
are terminal output, escape sequences and all, so something has to interpret them - that is what the
[TerminalView add-in](../platform/add-ins/TerminalView.md) is for.

## Pitfalls

- Host keys are trusted by default. A client with no `HostKeyReceived` subscriber accepts any host
  key, including an attacker's. Wire a policy - a known-hosts extension, a pinned fingerprint, or an
  accept-all you have consciously chosen.
- `ShellStream` byte writes are buffered. `Write`, `WriteByte` and `WriteAsync` do not reach the wire
  until `Flush()` or a full buffer; nothing throws, and the session appears dead instead. Set
  `AutoFlush = true` or use `WriteAndFlush`.
- Subscribing to `ShellStream.DataReceived` does not stop the internal read buffer from filling. Both
  happen. Either drain with `Read()`, or set `DisableReadBuffering = true` - and then never call
  `Read`, `ReadLine` or `Expect`, which throw while it is set.
- `AddForwardedPort` throws unless the client is already connected, and `Start()` throws unless the
  port has been added to a connected client. The order is connect, add, start.
- Errors on background threads never reach your call site. Subscribe to `BaseClient.ErrorOccurred`,
  `ForwardedPort.Exception`, `ShellStream.ErrorOccurred` and `Shell.ErrorOccurred`, or you will debug
  silent tunnels.
- `SftpClient.UploadFileAsync`'s `canOverride` overload takes the progress argument *before* the
  cancellation token. Pass `cancellationToken: token` by name rather than positionally.
- `SshCommand.Result` and `.Error` each drain their stream once and cache the result. Read each once,
  and read `Result` before you need `Error`.
- A stdin stream from `CreateInputStream` must be disposed to signal end of file. Without it the remote
  command may wait for input forever and the task never completes. It is also valid only while the
  command is executing, and only once per command.
- `SshCommand`, `ShellStream`, `Shell`, `SftpFileStream` and every client are `IDisposable` and hold a
  channel or a socket. Leaking them exhausts the session limit, and the next command or shell then
  blocks indefinitely waiting for a slot - a hang that looks like a server problem but is a missing
  `Dispose`.
- `SynchronizeDirectories` is not a mirror: one-way, non-recursive, size comparison only, and it never
  deletes anything on the remote side. Same-size edits are not re-uploaded.
- `Expect` and `ReadLine` without a `TimeSpan` block forever if the expected text never arrives. Pass a
  timeout in unattended code, and remember that `Expect` matches against buffered text, so a prompt
  already consumed by a previous `Read()` will not match.
- `ConnectionInfo` algorithm dictionaries are filled by the constructor. Mutating them before
  construction is impossible and after `Connect()` is pointless; do it in between.
- `CreateCommand(text, encoding)` assigns `ConnectionInfo.Encoding` as a side effect, changing the
  encoding for later commands on that client. Set `ConnectionInfo.Encoding` once if you want one
  encoding throughout.
- The library ships no nullable-reference annotations, so a consumer that enables them gets no
  warnings and also no protection. Null-check what is documented as nullable: `ExitStatus` really is
  null until the server reports one, and `ExitSignal`, `HostKeyEventArgs.Certificate` and
  `PrivateKeyFile.Certificate` are null when absent.
- `ScpClient` is synchronous and has no interface. If you need cancellation, progress objects or
  mockability, use `SftpClient`.
- Disposing a `ConnectionInfo` you constructed is your job. The convenience constructors own the one
  they build internally, but one you build yourself - along with the authentication methods in it - is
  not owned by the client.
- Reconnecting per operation is the expensive mistake. A connect is a TCP handshake, a version
  exchange, a key exchange and an authentication; commands, SFTP operations and shells all multiplex
  over channels on one authenticated session, so hold a client open and open channels instead. For a
  directory tree, prefer many parallel operations on one client over one operation on many clients.
- Leaving every timeout at its default hangs an unattended process. `OperationTimeout` on `SftpClient`,
  `ScpClient` and `NetConfClient` and `SshCommand.CommandTimeout` are all infinite by default;
  `ConnectionInfo.Timeout` covers only the connect and per-message waits.
- Prefer the asynchronous methods on hot paths. The synchronous SFTP calls block a thread pool thread
  for the whole round trip, and the older begin/end methods offer nothing the task-based ones do not.

## Samples and tools in the repository

The repository ships no sample applications, demo applications, benchmarks or tools. It builds one
library project and one test project.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Test suite | Every feature area, offline: it needs no SSH server, no containers and no outbound connections, and doubles as the worked-example corpus | [`tests/CodeBrix.SSH.Tests`](https://github.com/ellisnet/CodeBrix.SSH/tree/main/tests/CodeBrix.SSH.Tests) |
| Feature test files | Clients, commands, authentication, key formats, known hosts, shell streams, SFTP, SCP and forwarded ports, one file per behavior | [`tests/CodeBrix.SSH.Tests/Classes`](https://github.com/ellisnet/CodeBrix.SSH/tree/main/tests/CodeBrix.SSH.Tests/Classes) |
| Key fixtures | Private keys, public keys and OpenSSH certificates covering every supported key type in every supported container format, embedded in the test assembly | [`tests/CodeBrix.SSH.Tests/Data`](https://github.com/ellisnet/CodeBrix.SSH/tree/main/tests/CodeBrix.SSH.Tests/Data) |

> [!IMPORTANT]
> The key fixtures are test data, not credentials. They are published in a public repository and must
> never be used to authenticate against a real host.

Useful starting points in the suite: `SshClientTest.cs` and `SshCommandTest.cs` for the client and
command lifecycle, `ClientAuthenticationTest.cs` for multi-factor negotiation, `PrivateKeyFileTest.cs`
for key formats, `KnownHostsStoreTest.cs` and `KnownHostsClientExtensionsTest.cs` for host key
policies, the `ShellStreamTest*.cs` family for buffering and `Expect`, the `SftpClientTest*.cs` family
for SFTP, and `ForwardedPortLocalTest.cs` with its siblings for tunnels.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.SSH/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.SSH/blob/main/AGENT-README.txt) |
| Non-package content in the repository | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.SSH/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.SSH/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.SSH.Tests](https://github.com/ellisnet/CodeBrix.SSH/tree/main/tests/CodeBrix.SSH.Tests) |

XML documentation ships alongside the assembly, so every type and member described here is available
to IntelliSense.

## License

CodeBrix.SSH is licensed under the MIT License, and the license is also named in the package ID
(`CodeBrix.SSH.MitLicenseForever`). For the provenance and licensing of open source code included in
this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.SSH/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Cryptography](CodeBrix.Cryptography.md) - the cryptography this library is built on, usable directly for keys, certificates and PEM
- [CodeBrix.Docker](CodeBrix.Docker.md) - manage containers on a remote host, over its own SSH transport
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.SSH on GitHub](https://github.com/ellisnet/CodeBrix.SSH) - source and tests
