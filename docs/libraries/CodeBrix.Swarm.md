<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Swarm</sub>

# CodeBrix.Swarm

**CodeBrix.Swarm is three cross-platform .NET libraries for running a swarm of worker processes across
many hosts under one coordinator: the Queen coordinates, a Hive runs on each host and starts the Worker
processes there, and a Worker is one process doing the work.** Each library is one role, and an
application is built on the one it plays - a swarm is normally three separate programs, each adding the
one package for its own role. The Queen owns its own web host, so the coordinator can be a desktop or
console program rather than a web application; traffic is one-way, from the Queen out, over two
authenticated message hubs; and each Hive decides how much of its host to take, so the application
answers only "what do I start next?" and "what does one Worker do?".

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Swarm](https://github.com/ellisnet/CodeBrix.Swarm) |
| **Packages** | [`CodeBrix.Swarm.Queen.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Swarm.Queen.MitLicenseForever) - the coordinator, one per swarm<br>[`CodeBrix.Swarm.Hive.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Swarm.Hive.MitLicenseForever) - the per-host member that starts and ends Workers<br>[`CodeBrix.Swarm.Worker.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Swarm.Worker.MitLicenseForever) - one Worker process<br>The three are released together and are installed at the same version |
| **License** | MIT, all three; see [License](#license) |
| **Requires** | .NET 10 or later. Each package pulls in the Microsoft-published `Microsoft.AspNetCore.SignalR.Client` automatically. The Queen package also declares the ASP.NET Core shared framework, so a coordinator program needs the ASP.NET Core runtime installed; the Hive and Worker packages do not. A network the Hives and Workers can reach the coordinator over - the intended arrangement is plain HTTP on a trusted local network. No Redis, no Docker and no database is involved |
| **Use it from** | Any .NET 10 application: a console tool, a service or a desktop program, including a CodeBrix.Platform application acting as the coordinator |
| **Platforms** | Debian-based Linux, Windows and macOS. A Hive measures its host, and has an implementation for each of those three; on any other operating system it throws `PlatformNotSupportedException` |

## What it does

- **Queen** - starts and owns a web host with two hubs on it, one for Hives and one for Workers, and
  waits until it is listening; mints access tokens from one master secret the application supplies,
  with a different key derived for each role, so a token for one role cannot even be decrypted at the
  other's hub; sends application-defined messages to everything connected to either hub; sends the two
  messages the swarm defines for itself, "the work is over" to the Hives and "end yourself" to the
  Workers; and reports live counts of connected Hives and Workers, with events as they come and go.
- **Hive** - subscribes to the coordinator before it does anything else and starts nothing until it has
  reached it once; measures the host's memory and processor by direct calls into the operating system,
  with no native library; starts Workers one at a time, admitting each on *projected* memory use learnt
  from the Workers already running, never past limits whose defaults are also ceilings; hands each
  Worker its configuration down a private pipe that then stays open as a lifeline; waits longer after a
  Worker that fails immediately; and ends every Worker politely, then firmly, when the work is over.
- **Worker** - reads its configuration from the first line of standard input and watches the rest of
  that pipe as its lifeline; subscribes to the coordinator; runs the application's work until it
  finishes, the coordinator ends it, its Hive goes away, or the coordinator stays out of reach too long;
  always runs the application's shutdown step; and exits with a documented code that says which of
  those happened. One explicit switch runs a Worker by hand with no Hive anywhere.

## When to use it

Reach for CodeBrix.Swarm when a body of work is split into many independent processes that should run
on whatever machines are available, each machine taking as much of the work as it has room for and no
more, all of it under the control of one program that can tell everything to stop.

It is deliberately small, and these are not its job:

- It is not a queue and not a work distributor. Nothing is retained or replayed: a message reaches
  whatever is connected at the instant it is sent. What a Worker works on travels in its start-up
  configuration, and putting failed work back is the application's business.
- Traffic is one-way. A Hive or a Worker cannot call the coordinator, report progress or ask for work
  over the hubs; results travel over whatever the application already uses for data.
- The coordinator cannot address one Hive or one Worker; a message goes to every connection on a hub.
- There is no discovery and no registry of hosts. The coordinator's address and the tokens reach each
  host out of band.
- Nothing persists across a restart, and there is no encryption beyond the token: over plain HTTP the
  traffic, token included, is in the clear.
- A Hive decides whether to start a Worker; it does not limit a running Worker's memory or processor
  use, and never ends a running Worker because the host got busy.

## Getting started

Each program adds the package for its own role:

```bash
dotnet add package CodeBrix.Swarm.Queen.MitLicenseForever
dotnet add package CodeBrix.Swarm.Hive.MitLicenseForever
dotnet add package CodeBrix.Swarm.Worker.MitLicenseForever
```

The package IDs carry the license suffix; the namespaces do not. You install
`CodeBrix.Swarm.Queen.MitLicenseForever` and you write `using CodeBrix.Swarm.Queen;`. The shared
contract every role speaks - `CodeBrix.Swarm.Core`, with `CodeBrix.Swarm.Core.Messaging` and
`CodeBrix.Swarm.Core.Configuration` - travels inside each of the three packages rather than beside them.
There is no package of that name and nothing extra to install.

The coordinator, from the repository README. It mints both tokens a host needs, because only the
coordinator ever holds the secret:

```csharp
using CodeBrix.Swarm.Core.Messaging;
using CodeBrix.Swarm.Queen;

await using var queen = await SwarmQueen.StartAsync(
    new SwarmQueenOptions
    {
        Url = "http://0.0.0.0:5000",
        MasterSecret = secretFromTheApplicationsOwnStore
    },
    CancellationToken.None);

//Minted here, because only the coordinator ever holds the secret, and handed to each host out of
//band. A Hive needs both its own token and the one to pass on to its Workers.
var hiveToken = queen.CreateHiveToken();
var workerToken = queen.CreateWorkerToken();

queen.Connected += (_, connection) =>
    Console.WriteLine($"{connection.Role} arrived: {queen.HiveCount} Hive(s), {queen.WorkerCount} Worker(s)");

await queen.SendToWorkersAsync(
    SwarmMessage.Create("app.rate-limit", new { PerSecond = 20 }),
    CancellationToken.None);

//The work is over: every Hive ends its Workers and finishes, and nothing starts another.
await queen.SendTerminateAllWorkersToHivesAsync(CancellationToken.None);
```

One host. The application answers one question, and "not now" is never a failure:

```csharp
using CodeBrix.Swarm.Hive;

var outcome = await SwarmHiveHost.RunAsync(new SwarmHiveOptions
{
    QueenUrl = "http://192.168.1.10:5000",
    HiveToken = hiveToken,
    WorkerToken = workerToken,

    //The one question an application answers. "Not now" is never a failure.
    NextWorkerAsync = (context, cancellationToken) => Task.FromResult(
        queue.TryDequeue(out var item)
            ? WorkerLaunch.StartWithWork("/opt/example/ExampleWorker", [], item)
            : WorkerLaunch.NotNow),

    //Optional: leave more of the host alone than the defaults do. Asking for MORE is refused.
    Limits = new SwarmHostLimits { MaxRamPercent = 70, MaxWorkers = 8 }
});

return SwarmHiveHost.ExitCodeFor(outcome);
```

One Worker. Its whole program is "describe the work, hand it over, exit on what comes back":

```csharp
using CodeBrix.Swarm.Worker;

return await SwarmWorkerHost.RunAsync(
    new SwarmWorkerOptions
    {
        WorkAsync = async (context, cancellationToken) =>
        {
            var item = context.GetWork<WorkItem>();
            await ProcessAsync(item, cancellationToken);
        }
    },
    args);
```

Returning normally means finished; throwing means failed.

## Key concepts

### The Queen

`SwarmQueen.StartAsync(options, cancellationToken)` validates the options, starts the web host, maps
the two hubs, waits until it is listening and returns the coordinator; there is no constructor.
`SwarmQueenOptions.Url` must be an absolute http or https address - a port of zero takes any free port,
and `SwarmQueenOptions.AnyFreeLoopbackPortUrl` is the value a test should use. `BaseUrl` reports where
the coordinator is really listening, and `HiveHubUrl` and `WorkerHubUrl` are it plus `/swarm/hive` and
`/swarm/worker`. `MasterSecret` is required and must be at least 32 characters; a short secret is
refused, never stretched. `LoggerFactory` is null by default, and the host's own logging providers are
cleared, so supply one to see anything.

Tokens come from `CreateHiveToken()` and `CreateWorkerToken()`, which never expire, or from their
`TimeSpan lifetime` overloads. `SwarmTokenAuthority`, in `CodeBrix.Swarm.Queen.Authentication`, mints
and reads tokens away from a running coordinator - in a provisioning tool, for instance. An expiry is
checked when a connection is made, not while it is open.

`SendToHivesAsync` and `SendToWorkersAsync` send an application-defined `SwarmMessage`.
`SendTerminateAllWorkersToHivesAsync` says the work is over: every Hive ends its Workers, stops
starting new ones for good, and finishes - nothing undoes it. `SendTerminateToWorkersAsync` ends the
Workers but leaves the Hives free to start more. `StopAsync` stops listening and closes every
connection; it does not end the work.

### Messages and the shared contract

A `SwarmMessage` carries a `Kind`, matched exactly and case-sensitively, and an optional JSON payload:
`SwarmMessage.Create(kind, payload)` builds one and `GetPayload<T>()` reads it. Kinds beginning
`swarm.` are reserved for the two built-in messages, and sending or registering one is refused - give
the application's own kinds a prefix such as `app.`. A Hive or a Worker registers a handler with
`context.RegisterHandler(kind, handler)` in its `ConfigureAsync` step, which runs before it connects, so
nothing sent in the first moments is missed; a kind nothing is registered for is ignored on purpose.

### The Hive

`SwarmHiveHost.RunAsync(options, cancellationToken)` runs until something ends it and returns a
`SwarmHiveOutcome` - `ToldToTerminate`, `QueenUnreachable`, `Cancelled` or `Failed` - and
`SwarmHiveHost.ExitCodeFor(outcome)` turns that into the program's exit code. `QueenUrl`,
`HiveToken`, `WorkerToken` and `NextWorkerAsync` are required. A Hive needs two tokens because traffic is
one-way: it can never ask the coordinator for the token its Workers need.

`NextWorkerAsync` answers with `WorkerLaunch.NotNow`, which costs one `NotNowInterval` and nothing
else, or with `WorkerLaunch.Start` or `StartWithWork`, naming the Worker program, its arguments, and
the application's part of its configuration. That configuration is never put on the command line or
in an environment variable - both are readable by other processes - but written to the Worker's
standard input. Every Worker is named `{HiveId}-{n}`, starts at below-normal priority, and on Linux is
marked as the kernel's preferred out-of-memory victim where the host allows it.

### How much of a host a Hive takes

`SwarmHostLimits` holds `MaxRamPercent` and `MaxCpuPercent` (default and ceiling 90, the processor
figure averaged over `CpuAveragingWindow`, 30 seconds by default), `FreeRamFloorBytes` (1 GiB by
default) and an optional `MaxWorkers`. An application may lower a limit but never raise it: a
percentage above the ceiling is refused with a `SwarmConfigurationException`. Three environment
variables - `CODEBRIX_SWARM_MAX_WORKERS`, `CODEBRIX_SWARM_MAX_RAM_PERCENT` and
`CODEBRIX_SWARM_MAX_CPU_PERCENT` - let whoever operates a host hold its Hive back further still; a value
that would raise what the application asked for is ignored. The limits only ever stop new Workers from
starting. A Hive starts one Worker, waits `SettleInterval` (5 seconds by default), and measures again.

### The Worker, the lifeline and the exit codes

`SwarmWorkerHost.RunAsync(options, args)` runs the Worker from start to finish and returns its exit
code. `WorkAsync` is required; `ConfigureAsync`, `ShutdownAsync` and `Report` are optional. The work's
cancellation token is cancelled when the Worker is told to end, when its Hive goes away, or when the
coordinator has been out of reach for `QueenUnreachableWindow` (60 seconds by default). The shutdown
step always runs, with an uncancelled token, and `context.Outcome` says why.

After writing the configuration a Hive leaves the pipe open. While it is open the Hive is there; the end
of it means the Hive has gone or is asking the Worker to exit. The library watches it, so a Worker never
reads standard input itself.

`--swarm-config <path>` (or `--swarm-config=<path>`) runs a Worker by hand from a configuration file,
with no Hive and no lifeline; it is the only argument the library reads, and every other argument
belongs to the application. `WorkerConfiguration` and `WorkerSwarmSettings` in
`CodeBrix.Swarm.Core.Configuration` write such a file.

The exit codes in `SwarmExitCodes` are part of the contract, and a Hive reads them:

| Code | Constant | Meaning |
| --- | --- | --- |
| 0 | `Success` | Ended normally: the work finished, it was told to end, its Hive closed the lifeline, or it was cancelled |
| 69 | `QueenUnreachable` | The coordinator stayed out of reach for the whole retry window |
| 70 | `WorkFailed` | The application's own work threw |
| 78 | `ConfigurationInvalid` | No usable configuration |

## Examples

A Worker's shutdown step, told why it is stopping:

```csharp
ShutdownAsync = async (context, _) =>
{
    switch (context.Outcome)
    {
        case SwarmWorkerOutcome.WorkFinished:
            await MarkDoneAsync();
            break;

        case SwarmWorkerOutcome.ToldToTerminate:
        case SwarmWorkerOutcome.LifelineClosed:
            await PutTheWorkBackAsync();   // somebody else will finish it
            break;

        default:
            await RecordAsync(context.Outcome);
            break;
    }
},
```

A Hive that pauses on an application-defined message - pausing is exactly answering "not now" until it
is over:

```csharp
ConfigureAsync = (context, _) =>
{
    context.RegisterHandler("app.pause", (message, _) =>
    {
        paused = message.GetPayload<PauseRequest>().Paused;
        return Task.CompletedTask;
    });

    return Task.CompletedTask;
},

NextWorkerAsync = (context, _) => Task.FromResult(
    paused ? WorkerLaunch.NotNow : NextFromTheQueue()),
```

Ending a swarm deliberately, from the coordinator:

```csharp
// Stop the Workers but leave the Hives free to start more:
await queen.SendTerminateToWorkersAsync(CancellationToken.None);

// THE SHOW IS OVER - every Hive ends its Workers and finishes for good:
await queen.SendTerminateAllWorkersToHivesAsync(CancellationToken.None);

await queen.StopAsync(CancellationToken.None);
```

Give the Hives a moment between the Hive message and `StopAsync`: a Hive that loses its connection
first still winds down, but by the slower route of its retry window running out.

Writing a configuration file to run one Worker by hand against a running coordinator:

```csharp
using CodeBrix.Swarm.Core.Configuration;

var configuration = new WorkerConfiguration
{
    Swarm = new WorkerSwarmSettings
    {
        QueenUrl = "http://127.0.0.1:5000",
        Token    = workerTokenFromTheCoordinator,
        WorkerId = "by-hand"
    },
    Work = "{\"batch\":7}"
};

File.WriteAllText("/tmp/worker.json", configuration.ToJsonLine());

//  ExampleWorker --swarm-config /tmp/worker.json
```

## Pitfalls

- **A refused token costs the whole retry window.** A Hive or a Worker cannot tell "the coordinator said
  no" from "the coordinator did not answer", so a wrong, expired or wrong-role token shows up as a
  minute of nothing and then exit code 69. When a host goes quiet for exactly the window, suspect the
  token before the network.
- **Handing a Hive only one token, or the two the wrong way round.** `CreateHiveToken` for `HiveToken`,
  `CreateWorkerToken` for `WorkerToken`; if the Hive connects but its Workers never do, check the Worker
  token first.
- **Expecting a late joiner to be caught up.** Nothing is retained. Put what a new arrival must know in
  its start-up configuration.
- **Stopping the coordinator instead of ending the work.** `StopAsync` on its own tells nobody the work
  is over; Hives keep retrying for their window and may start Workers in the meantime. Send
  `SendTerminateAllWorkersToHivesAsync` first.
- **A new master secret on every restart.** It invalidates every token already handed out; keep the
  secret where the application keeps its other secrets.
- **Tokens on a command line or in an environment variable.** Anything else on the host can read those.
- **Plain HTTP on an untrusted network.** The token is the whole of the authentication and travels in the
  clear; keep the swarm on a network you control, or supply an https address and the certificate
  configuration the ASP.NET Core host needs.
- **Forgetting the ASP.NET Core runtime on the coordinator's machine.** The program fails to start
  without it.
- **Asking for more of a host than the ceiling.** `MaxRamPercent = 95` throws; the limits exist to be
  lowered.
- **A slow or throwing `NextWorkerAsync`.** It runs on the Hive's only thread of decision, and throwing
  ends the run as `Failed`. Take from a ready queue, and answer `NotNow` when unsure.
- **Two Hives on one host.** Each measures the whole host; give each its own `HiveId` and lower limits
  if it has to be done.
- **Reading standard input in a Worker.** It steals the lifeline.
- **Exiting a Worker with a code of its own.** Return what `RunAsync` returns, and let the work throw to
  report a failure.
- **Swallowing `OperationCanceledException` in the work.** The token is cancelled because something is
  ending the Worker; let it propagate, and do cleanup in `ShutdownAsync`, which runs within the Hive's
  grace period - 10 seconds by default.
- **A fixed port in a test.** Two runs collide; use `SwarmQueenOptions.AnyFreeLoopbackPortUrl` and read
  `BaseUrl`.

## Samples and tools in the repository

The repository ships three packages. Everything below is samples and tests: none of it is packed.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| SwarmSample.Queen | A console coordinator: the library owning the web host, minting both tokens a host needs and printing them as one line of JSON, an application-defined message, "the work is over", live counts and the connect and disconnect events | [`samples/SwarmSample.Queen`](https://github.com/ellisnet/CodeBrix.Swarm/tree/main/samples/SwarmSample.Queen) |
| SwarmSample.Hive | The one question a Hive's application answers, "not now" once enough Workers have been handed out, a handler registered before connecting, being told about every Worker that ends, a lowered `MaxWorkers`, and ending tidily on Ctrl-C | [`samples/SwarmSample.Hive`](https://github.com/ellisnet/CodeBrix.Swarm/tree/main/samples/SwarmSample.Hive) |
| SwarmSample.Worker | A Worker whose work is deliberately dull, so its shape shows: its own JSON configuration carried through unread, silence unless asked to report, and a command-line switch of its own that the library leaves alone | [`samples/SwarmSample.Worker`](https://github.com/ellisnet/CodeBrix.Swarm/tree/main/samples/SwarmSample.Worker) |
| End-to-end tests | A real coordinator on a free loopback port, a real Hive, and the real sample Worker started as child processes: tokens refused across roles, caps honored, messages reaching a whole swarm, Workers finishing, failing and being replaced, both kinds of termination, the coordinator going away and the lifeline closing. On by default; needs nothing but the SDK and localhost | [`tests/CodeBrix.Swarm.EndToEnd.Tests`](https://github.com/ellisnet/CodeBrix.Swarm/tree/main/tests/CodeBrix.Swarm.EndToEnd.Tests) |
| Per-library tests | One test project for each of the Queen, the Hive, the Worker and the shared contract | [`tests`](https://github.com/ellisnet/CodeBrix.Swarm/tree/main/tests) |

The three samples run together as a real swarm, on one machine or across several: start the
coordinator, pipe the line of JSON it prints into the Hive's standard input, and type commands at the
coordinator. [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Swarm/blob/main/EXTRAS-README.txt)
has the steps.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Swarm/blob/main/README.md) |
| The Queen's API guide (ships inside the Queen package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Swarm/blob/main/AGENT-README.txt) |
| The Hive's API guide | [src/CodeBrix.Swarm.Hive/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Swarm/blob/main/src/CodeBrix.Swarm.Hive/AGENT-README.txt) |
| The Worker's API guide | [src/CodeBrix.Swarm.Worker/AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Swarm/blob/main/src/CodeBrix.Swarm.Worker/AGENT-README.txt) |
| Samples, tests and other non-package content | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Swarm/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Swarm/blob/main/README-INDEX.txt) |
| Provenance and licensing | [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Swarm/blob/main/THIRD-PARTY-NOTICES.txt) |
| Tests (worked examples of every API) | [tests](https://github.com/ellisnet/CodeBrix.Swarm/tree/main/tests) |

Each of the three guides opens with the same short account of how the roles fit together. XML
documentation ships alongside every assembly.

## License

CodeBrix.Swarm is licensed under the MIT License, and the license is also named in the package IDs:
`CodeBrix.Swarm.Queen.MitLicenseForever`, `CodeBrix.Swarm.Hive.MitLicenseForever` and
`CodeBrix.Swarm.Worker.MitLicenseForever`. License acceptance is required. For the provenance and
licensing record of the libraries, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Swarm/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Redis](CodeBrix.Redis.md) and [CodeBrix.Docker](CodeBrix.Docker.md) - the family's other networking and services libraries
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.Swarm on GitHub](https://github.com/ellisnet/CodeBrix.Swarm) - source, samples and tests
