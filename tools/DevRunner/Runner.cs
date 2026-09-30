using System.Globalization;
using System.Net;
using System.Net.Sockets;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner(Options options, CancellationToken cancellation, Evidence? evidence = null, ScenarioScope? scope = null, string? root = null)
{
    private readonly string _root = root ?? FindRoot();
    private string GameDirectory => Path.Combine(_root, "src", "Game");
    private const string EngineVersion = "4.7.2";
    private readonly Evidence _evidence = evidence ?? new Evidence(FindRoot(), options.EvidenceDirectory);
    private readonly ScenarioScope? _scope = scope;

    public async Task Run()
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        string result = "passed";
        try
        {
            switch (options.Command)
            {
                case "help":
                    Console.WriteLine("Commands: dev, play, server, client, prepare, test-network, test-ui, check-ui-prerequisites, prepare-templates, export-client, export-server, ci\nOptions: --host ADDRESS --bind ADDRESS --port PORT --startup-timeout-ms MS --timeout-ms MS --session-file PATH\nNetwork: --jobs N (default 2; serial 1), --scenario NAME\nUI: --scenario NAME (source slices serial; exported-package uses existing exports)\nNetwork scenarios: " + string.Join(", ", ScenarioNames.Network) + "\nUI scenarios: " + string.Join(", ", ScenarioNames.Ui) + "\nDesktop dev/client/play accept repeated --engine-arg VALUE. Dev: --guests 1..3 (default 1).");
                    Console.WriteLine("Steam compatibility: check-steam-extension [--offline] [--exported] [--target linux-x64|windows-x64] (single account). Paired: test-steam --role host|guest [--lobby ID] [--exported] (two accounts/machines; normal desktop).\nClient packaging: export-client [--tag vVERSION] [--target linux-x64|windows-x64] [--steam-app-id ID] [--production] (stable tags/production require own non-480 ID; tagged exports require a clean tag checkout).\nNative Windows validation: ci-windows (source/export/runtime/offline solo; no publishing).\nDevelopment Steam initialization defaults to 480; ODOT_STEAM_APP_ID overrides development runs.");
                    break;
                case "prepare": await Prepare(); break;
                case "check-steam-extension": await CheckSteamExtension(); break;
                case "test-steam": await TestSteam(); break;
                case "dev": await Interactive(); break;
                case "server":
                case "play":
                case "client": await SingleRole(); break;
                case "test-network": await Prepare(); await NetworkTests(); break;
                case "check-ui-prerequisites": PrivateDisplay.CheckPrerequisites(); break;
                case "test-ui":
                    PrivateDisplay.CheckPrerequisites();
                    if (options.Scenario != "exported-package") await Prepare(); else CheckPackages();
                    await UiTests(options.Scenario); break;
                case "_ui-worker": await UiWorker(); break;
                case "prepare-templates": await PrepareTemplates(); break;
                case "export-client":
                    if (options.ReleaseTag is not null) await TaggedExport();
                    else { await Prepare(); await PrepareTemplates(); await Export(false); }
                    break;
                case "export-server": await Prepare(); await PrepareTemplates(); await Export(true); break;
                case "ci": await Ci(); break;
                case "ci-source": await CiSource(); break;
                case "ci-linux-package": await CiLinuxPackage(); break;
                case "ci-windows": await WindowsCi(); break;
                default: throw new ArgumentException($"Unknown command: {options.Command}");
            }
        }
        catch (VerificationPrerequisiteException) { result = "unexecuted"; throw; }
        catch { result = "failed"; throw; }
        finally
        {
            string coverage = options.Scenario is not null ? "selected: " + options.Scenario : options.Command switch
            {
                "test-ui" or "_ui-worker" => "all source UI slices",
                "ci" or "test-network" => "full required set",
                "ci-source" => "all source gates; no exports or package coverage",
                "ci-linux-package" => "Linux exports/native/headless/private UI; source gates separate",
                "ci-windows" => "Windows source/native/export/offline solo; Linux and real Steam gates separate",
                _ => "command only; no test coverage claimed"
            };
            await _evidence.Summary(options.Command, coverage, options.Command is "ci" or "ci-source" or "test-network" ? options.Jobs : 1, result, timer.Elapsed.TotalSeconds);
        }
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(Environment.CurrentDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Odot.slnx"))) return directory.FullName;
            directory = directory.Parent;
        }
        throw new InvalidOperationException("Run from the repository root (Odot.slnx was not found).");
    }

    private async Task<string> Capture(string executable, params string[] args)
    {
        var start = new System.Diagnostics.ProcessStartInfo(executable)
        {
            WorkingDirectory = _root,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (string arg in args) start.ArgumentList.Add(arg);
        using var process = System.Diagnostics.Process.Start(start) ?? throw new InvalidOperationException($"Cannot start {executable}.");
        Task<string> stdout = process.StandardOutput.ReadToEndAsync(cancellation);
        Task<string> stderr = process.StandardError.ReadToEndAsync(cancellation);
        try
        {
            await process.WaitForExitAsync(cancellation).WaitAsync(TimeSpan.FromSeconds(15), cancellation);
            string output = await stdout;
            if (process.ExitCode != 0) throw new InvalidOperationException($"{executable} failed: {await stderr}");
            return output.Trim();
        }
        finally
        {
            if (!process.HasExited) { process.Kill(true); await process.WaitForExitAsync(); }
        }
    }

    private async Task Execute(string name, string executable, params string[] args)
    {
        await _evidence.Measure(name, "phase", async () =>
        {
            await using var child = new Child(name, executable, args, _root, evidenceDirectory: _evidence.Directory);
            int code = await child.WaitExit(cancellation);
            if (code != 0 || (executable == "godot" && child.HasEngineErrors))
                throw new InvalidOperationException($"{name} failed with exit {code} or engine errors.\n{child.Tail()}");
        });
    }

    private async Task Preflight()
    {
        string sdk = await Capture("dotnet", "--version");
        if (sdk != "10.0.401") throw new InvalidOperationException($"Expected .NET SDK 10.0.401, got {sdk}; run mise install --locked dotnet.");
        string engine;
        try { engine = await Capture("godot", "--version"); }
        catch (Exception e) when (e is System.ComponentModel.Win32Exception or InvalidOperationException)
        { throw new InvalidOperationException("Godot .NET is missing or broken; run mise install --locked godot-dotnet and retain its GodotSharp directory.", e); }
        if (!engine.StartsWith(EngineVersion + ".stable.mono.", StringComparison.Ordinal))
            throw new InvalidOperationException($"Expected Godot {EngineVersion} .NET edition, got {engine}; use the locked godot-dotnet tool.");
        Console.WriteLine($"Tools: .NET {sdk}; Godot {engine}");
    }

    private async Task Prepare()
    {
        await Preflight();
        await VerifySteamFiles();
        await Execute("restore", "dotnet", "restore", "Odot.slnx", "--locked-mode");
        await BuildAndImport();
    }
    private async Task BuildAndImport()
    {
        await Execute("build", "dotnet", "build", "Odot.slnx", "--no-restore", "--nologo");
        await Import("import");
    }

    private Task Import(string name) => ExecuteEditor(name, options.StartupTimeout, false,
        "--headless", "--path", GameDirectory, "--editor", "--import");

    private async Task ExecuteEditor(string name, int timeout, bool exporting, params string[] args)
    {
        await _evidence.Measure(name, "phase", async () =>
        {
            EditorImports.SeedStartupExtensions(GameDirectory);
            // Both measures are needed: early extension loading and a cold help cache
            // avoid Godot's deferred extension-documentation callback after shutdown.
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            deadline.CancelAfter(timeout);
            await using var owned = new ScenarioScope(name, _evidence);
            var environment = owned.EnvironmentFor("editor");
            if (exporting)
            {
                // Stock export templates live in the normal data directory, not the help cache.
                environment.Remove("XDG_DATA_HOME"); environment.Remove("ODOT_OWNED_DATA");
                if (OperatingSystem.IsWindows()) environment.Remove("APPDATA");
            }
            await using var child = owned.Own(new Child(name, "godot", args, _root,
                environment: environment, evidenceDirectory: owned.EvidenceDirectory));
            int code = await child.WaitExit(deadline.Token);
            if (code != 0 || child.HasEngineErrors)
                throw new InvalidOperationException($"{name} failed with exit {code} or engine errors.\n{child.Tail()}");
            await owned.DisposeAsync(); owned.CheckErrors();
        });
    }

    private Child StartGame(string name, bool server, bool headless, int port, string? exported = null, params string[] extra)
        => StartGameRole(name, server ? "server" : "client", headless, port, exported, extra);

    private Child StartGameRole(string name, string role, bool headless, int port, string? exported = null, params string[] extra)
    {
        bool server = role is "server" or "playing-host";
        bool guest = role == "client";
        if (role is not ("server" or "playing-host" or "client" or "menu" or "solo")) throw new ArgumentException("Unknown game role: " + role);
        var args = new List<string>();
        if (headless) args.Add("--headless");
        if (_scope is not null) args.AddRange(["--log-file", Path.Combine(_scope.EvidenceDirectory, name + "-engine-" + Guid.NewGuid().ToString("N") + ".log")]);
        if (exported is null) args.AddRange(["--path", GameDirectory]);
        if (!headless)
        {
            if (_scope?.Graphical == true)
                args.AddRange(["--verbose", "--display-driver", "x11", "--rendering-method", "gl_compatibility", "--rendering-driver", "opengl3", "--audio-driver", "Dummy", "--max-fps", "30", "--windowed", "--resolution", "1100x820"]);
            if (name == "client-a") args.AddRange(["--position", "30,80"]);
            if (name == "client-b") args.AddRange(["--position", "900,80"]);
            args.AddRange(options.EngineArgs);
        }
        args.AddRange(["--", "--supervised"]);
        if (role != "menu") args.Add("--" + role);
        if (server || guest) args.AddRange(["--port", port.ToString(CultureInfo.InvariantCulture), server ? "--bind" : "--host", server ? options.Bind : options.Host]);
        if (guest && !extra.Contains("--session-file"))
        {
            string session = _scope is not null ? Path.Combine(_scope.Directory, name + ".json")
                : options.SessionFile ?? Path.Combine(_root, ".sessions", $"{name}-{port}.json");
            args.AddRange(["--session-file", session]);
        }
        args.AddRange(extra);
        var environment = _scope?.EnvironmentFor(name);
        if (OperatingSystem.IsWindows() && exported is not null)
        {
            environment ??= new Dictionary<string, string?>();
            WindowsStandaloneEnvironment(environment);
        }
        if (_scope?.Graphical == true && role is "menu" or "solo") environment!["ODOT_STEAM_DISABLED"] = "1";
        bool steamDisabled = (environment?.GetValueOrDefault("ODOT_STEAM_DISABLED") ?? Environment.GetEnvironmentVariable("ODOT_STEAM_DISABLED")) == "1";
        if (OperatingSystem.IsLinux() && System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.X64
            && SteamOverlayLaunch.IsEligible(options.Command, role, headless || args.Contains("--headless"), _scope?.Graphical == true, steamDisabled))
        {
            string? preload = SteamOverlayLaunch.Preload(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Environment.GetEnvironmentVariable("LD_PRELOAD"));
            environment ??= new Dictionary<string, string?>();
            environment["ODOT_STEAM_OVERLAY_PRELOADED"] = preload is null ? "0" : "1";
            if (preload is not null)
            {
                environment["LD_PRELOAD"] = preload;
                Console.WriteLine("Steam overlay renderer prepared for the game process.");
            }
            else Console.WriteLine("Steam overlay renderer was not found in the native Steam installation. Launch through Steam to use invitations; Single player remains available.");
        }
        var child = new Child(name, exported ?? "godot", args, _root, game: true, quiet: headless || _scope is not null,
            workingDirectory: exported is null ? _root : Path.GetDirectoryName(exported), environment: environment, evidenceDirectory: _scope?.EvidenceDirectory ?? _evidence.Directory,
            sanitizeSteam: options.Command == "test-steam");
        return _scope?.Own(child) ?? child;
    }

    private async Task Interactive()
    {
        await Prepare();
        // Own per-process preferences and credentials, while using the normal desktop.
        await using var owned = new ScenarioScope("dev", _evidence);
        var worker = new Runner(options, cancellation, _evidence, owned);
        int port = options.Port ?? 7000;
        Child host = worker.StartGameRole("client-a", "playing-host", false, port);
        await host.WaitFor(e => e.Type == "ready", "playing host readiness", options.StartupTimeout, cancellation);
        var peers = new List<Child> { host };
        for (int i = 0; i < options.Guests; i++)
        {
            Child guest = worker.StartGameRole(i == 0 ? "client-b" : "client-" + (i + 2), "client", false, port);
            peers.Add(guest);
            await guest.WaitFor(e => e.Type == "connected", "guest admission", options.StartupTimeout, cancellation);
        }
        Console.WriteLine($"Development: playing host + {options.Guests} guest(s); close a window or Ctrl-C to stop owned peers.");
        await Task.WhenAny(peers.Select(p => p.Exited)).WaitAsync(cancellation);
        Child exited = peers.First(p => p.HasExited);
        if (exited.ExitCode != 0) throw new InvalidOperationException($"{exited.Name} exited with {exited.ExitCode}.\n{exited.Tail()}");
        await owned.DisposeAsync(); owned.CheckErrors();
    }

    private async Task SingleRole()
    {
        await Prepare();
        await using var child = StartGameRole(options.Command, options.Command == "play" ? "menu" : options.Command, options.Command == "server", options.Port ?? 7000);
        int code = await child.WaitExit(cancellation);
        if (code != 0) throw new InvalidOperationException($"{options.Command} exited with {code}.");
    }

    private static int FreePort()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)socket.LocalEndPoint!).Port;
    }

    private static MatchSnapshot State(GameEvent e) => e.State ?? throw new InvalidOperationException($"{e.Type} has no state.");
    private static bool HasState(GameEvent e) => e.Type is "snapshot" or "connected" or "ack" && e.State is not null;
    private static void Require(bool condition, string expectation)
    {
        if (!condition) throw new InvalidOperationException(expectation);
        Console.WriteLine($"PASS: {expectation}");
    }
}
