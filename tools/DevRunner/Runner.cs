using System.Net;
using System.Net.Sockets;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner(Options options, CancellationToken cancellation, Evidence? evidence = null, ScenarioScope? scope = null)
{
    private readonly string _root = FindRoot();
    private string GameDirectory => Path.Combine(_root, "src", "Game");
    private const string EngineVersion = "4.7.2";
    private readonly Evidence _evidence = evidence ?? new Evidence(FindRoot(), options.EvidenceDirectory);
    private readonly ScenarioScope? _scope = scope;
    private string? _devSessions;

    public async Task Run()
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        string result = "passed";
        try
        {
            switch (options.Command)
            {
                case "help":
                    Console.WriteLine("Commands: dev, server, client, prepare, test-network, test-ui, check-ui-prerequisites, prepare-templates, export-client, export-server, ci\nOptions: --host ADDRESS --bind ADDRESS --port PORT --startup-timeout-ms MS --timeout-ms MS --session-file PATH\nNetwork: --jobs N (default 2; serial 1), --scenario NAME\nUI: --scenario NAME (source slices serial; exported-package uses existing exports)\nNetwork scenarios: " + string.Join(", ", ScenarioNames.Network) + "\nUI scenarios: " + string.Join(", ", ScenarioNames.Ui) + "\nDesktop dev/client also accept repeated --engine-arg VALUE.");
                    break;
                case "prepare": await Prepare(); break;
                case "dev": await Interactive(); break;
                case "server":
                case "client": await SingleRole(); break;
                case "test-network": await Prepare(); await NetworkTests(); break;
                case "check-ui-prerequisites": PrivateDisplay.CheckPrerequisites(); break;
                case "test-ui":
                    PrivateDisplay.CheckPrerequisites();
                    if (options.Scenario != "exported-package") await Prepare(); else CheckPackages();
                    await UiTests(options.Scenario); break;
                case "_ui-worker": await UiWorker(); break;
                case "prepare-templates": await PrepareTemplates(); break;
                case "export-client": await Prepare(); await PrepareTemplates(); await Export(false); break;
                case "export-server": await Prepare(); await PrepareTemplates(); await Export(true); break;
                case "ci": await Ci(); break;
                default: throw new ArgumentException($"Unknown command: {options.Command}");
            }
        }
        catch { result = "failed"; throw; }
        finally
        {
            string coverage = options.Scenario is not null ? "selected: " + options.Scenario : options.Command switch
            {
                "test-ui" or "_ui-worker" => "all source UI slices",
                "ci" or "test-network" => "full required set",
                _ => "command only; no test coverage claimed"
            };
            await _evidence.Summary(options.Command, coverage, options.Command is "ci" or "test-network" ? options.Jobs : 1, result, timer.Elapsed.TotalSeconds);
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
        await Execute("restore", "dotnet", "restore", "Odot.slnx", "--locked-mode");
        await BuildAndImport();
    }
    private async Task BuildAndImport()
    {
        await Execute("build", "dotnet", "build", "Odot.slnx", "--no-restore", "--nologo");
        await Execute("import", "godot", "--headless", "--path", GameDirectory, "--editor", "--import");
    }

    private Child StartGame(string name, bool server, bool headless, int port, string? exported = null, params string[] extra)
    {
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
        args.AddRange(["--", server ? "--server" : "--client", "--supervised", "--port", port.ToString(), server ? "--bind" : "--host", server ? options.Bind : options.Host]);
        if (!server && !extra.Contains("--session-file"))
        {
            string session = _scope is not null ? Path.Combine(_scope.Directory, name + ".json") : _devSessions is not null ? Path.Combine(_devSessions, name + ".json")
                : options.SessionFile ?? Path.Combine(_root, ".sessions", $"{name}-{port}.json");
            args.AddRange(["--session-file", session]);
        }
        args.AddRange(extra);
        var child = new Child(name, exported ?? "godot", args, _root, game: true, quiet: headless || _scope is not null,
            workingDirectory: exported is null ? _root : Path.GetDirectoryName(exported), environment: _scope?.EnvironmentFor(name), evidenceDirectory: _scope?.EvidenceDirectory ?? _evidence.Directory);
        return _scope?.Own(child) ?? child;
    }

    private async Task Interactive()
    {
        await Prepare();
        _devSessions = Path.Combine(Path.GetTempPath(), "odot-dev-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_devSessions);
        try
        {
            int port = options.Port ?? 7000;
            await using var server = StartGame("server", true, true, port);
            await server.WaitFor(e => e.Type == "ready", "server readiness", options.StartupTimeout, cancellation);
            await using var a = StartGame("client-a", false, false, port);
            await using var b = StartGame("client-b", false, false, port);
            await Task.WhenAny(server.Exited, a.Exited, b.Exited).WaitAsync(cancellation);
            int code = new[] { server, a, b }.First(p => p.HasExited).ExitCode;
            if (code != 0) throw new InvalidOperationException($"A supervised game process exited with {code}.");
        }
        finally { Directory.Delete(_devSessions, true); _devSessions = null; }
    }

    private async Task SingleRole()
    {
        await Prepare();
        await using var child = StartGame(options.Command, options.Command == "server", options.Command == "server", options.Port ?? 7000);
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
