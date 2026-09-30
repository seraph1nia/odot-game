using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace DevRunner;

internal static class PrivateDisplay
{
    public static readonly string[] RequiredTools = ["Xvfb", "xvfb-run", "xauth", "xdpyinfo", "glxinfo", "openbox", "setsid"];
    public static void CheckPrerequisites(string? searchPath = null)
    {
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
            throw new InvalidOperationException("Private-display verification currently requires Linux x86_64.");
        string[] paths = (searchPath ?? Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator);
        string[] missing = RequiredTools.Where(tool => !paths.Any(p =>
        {
            string path = Path.Combine(p, tool);
            return File.Exists(path) && OperatingSystem.IsLinux() && (File.GetUnixFileMode(path) & (UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute)) != 0;
        })).ToArray();
        if (missing.Length > 0)
            throw new InvalidOperationException("Missing private-display prerequisites: " + string.Join(", ", missing) +
                ". Install distro packages listed in README (Xvfb, Xauthority/X11 utilities, Mesa and Openbox), then retry. No desktop fallback or automatic installation.");
        Console.WriteLine("Private-display executable prerequisites available; software OpenGL will be checked on the owned display.");
    }
    public static void ValidateWorker(Options options)
    {
        string? runtime = Environment.GetEnvironmentVariable("ODOT_UI_RUNTIME");
        string? authority = Environment.GetEnvironmentVariable("XAUTHORITY");
        string? display = Environment.GetEnvironmentVariable("DISPLAY");
        if (options.WorkerToken is null || Environment.GetEnvironmentVariable("ODOT_UI_WORKER") != options.WorkerToken ||
            runtime is null || !File.Exists(Path.Combine(runtime, "worker-token")) ||
            File.ReadAllText(Path.Combine(runtime, "worker-token")) != options.WorkerToken ||
            authority is null || Path.GetFullPath(authority) != Path.Combine(Path.GetFullPath(runtime), "xauthority") || !File.Exists(authority) ||
            display is null || !Regex.IsMatch(display, "^:[0-9]+$") || Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") is not null)
            throw new InvalidOperationException("UI worker requires its supervisor-owned X11 display, Xauthority and runtime marker; desktop fallback is forbidden.");
    }
    public static string RequireSoftwareGraphics(string info)
    {
        string renderer = info.Split('\n').FirstOrDefault(s => s.StartsWith("OpenGL renderer string:", StringComparison.Ordinal)) ?? "";
        Match version = Regex.Match(info, @"OpenGL version string: (\d+)\.(\d+)");
        if (!version.Success || new Version(int.Parse(version.Groups[1].Value), int.Parse(version.Groups[2].Value)) < new Version(3, 3) ||
            !(renderer.Contains("llvmpipe", StringComparison.OrdinalIgnoreCase) || renderer.Contains("softpipe", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Owned display needs Mesa software OpenGL >= 3.3.\n" + info);
        return renderer;
    }
}

internal sealed partial class Runner
{
    private static Child ExpectedFailure(Child child, bool bindFailure = false)
    {
        child.ExpectedFailure = true;
        if (bindFailure) child.AllowedEngineError = "Couldn't create an ENet host.";
        return child;
    }
    private async Task<(Child Server, int Port)> StartServer(string name, int port, CancellationToken token, bool retryAutomatic = true, string? exported = null)
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        for (int attempt = 0; ; attempt++)
        {
            Child server = StartGame(name + (attempt == 0 ? "" : "-retry" + attempt), true, true, port, exported);
            try
            {
                int remaining = options.StartupTimeout - (int)timer.ElapsedMilliseconds;
                if (remaining <= 0) throw new TimeoutException($"{name}: automatic bind retry exceeded startup deadline.");
                await server.WaitFor(e => e.Type == "ready", "server readiness", remaining, token);
                return (server, port);
            }
            catch (InvalidOperationException error) when (retryAutomatic && attempt < 3 && error.Message.Contains("Cannot bind server", StringComparison.Ordinal))
            {
                server.ExpectedFailure = true;
                server.AllowedEngineError = "Couldn't create an ENet host.";
                await server.DisposeAsync();
                port = _scope!.Port();
                Console.WriteLine($"{name}: automatic bind collision; retrying an owned candidate within startup deadline.");
            }
            catch { await server.DisposeAsync(); throw; }
        }
    }
    private void CheckPackages()
    {
        foreach (string role in new[] { "client", "server" })
            if (!File.Exists(Path.Combine(_root, "dist", role, "odot.x86_64")))
                throw new InvalidOperationException($"Missing dist/{role}/odot.x86_64; run mise run export-{role} first. The selected package check never implicitly exports.");
    }
    private async Task UiTests(string? selection)
    {
        PrivateDisplay.CheckPrerequisites();
        if (selection == "exported-package") CheckPackages();
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        deadline.CancelAfter(options.Timeout);
        await using var display = new ScenarioScope("display-" + (selection == "exported-package" ? "package" : "source"), _evidence);
        string token = Guid.NewGuid().ToString("N");
        await File.WriteAllTextAsync(Path.Combine(display.Directory, "worker-token"), token, cancellation);
        var env = display.EnvironmentFor("display");
        env["DISPLAY"] = null; env["WAYLAND_DISPLAY"] = null; env["XAUTHORITY"] = null;
        env["DBUS_SESSION_BUS_ADDRESS"] = null;
        env["LIBGL_ALWAYS_SOFTWARE"] = "true"; env["LP_NUM_THREADS"] = "2";
        env["ODOT_UI_WORKER"] = token; env["ODOT_UI_RUNTIME"] = display.Directory;
        var args = new List<string> { "xvfb-run", "--auto-servernum", "--auth-file", Path.Combine(display.Directory, "xauthority"),
            "--error-file", Path.Combine(display.EvidenceDirectory, "xvfb.log"), "--server-args=-screen 0 1920x1080x24 -nolisten tcp",
            "dotnet", typeof(Runner).Assembly.Location, "_ui-worker", "--evidence-directory", _evidence.Directory, "--worker-token", token,
            "--startup-timeout-ms", options.StartupTimeout.ToString(), "--timeout-ms", options.Timeout.ToString() };
        if (selection is not null) args.AddRange(["--scenario", selection]);
        if (options.Port is not null) args.AddRange(["--port", options.Port.Value.ToString()]);
        try
        {
            await _evidence.Measure(selection == "exported-package" ? "exported-ui" : "source-ui", "suite", async () =>
            {
                await using var worker = new Child("private-display-worker", "setsid", args, _root, environment: env, evidenceDirectory: display.EvidenceDirectory, ownsGroup: true);
                Console.WriteLine($"OWNED display worker pid={worker.ProcessId}; coverage={(selection ?? "all source slices")}");
                int code = await worker.WaitExit(deadline.Token);
                if (code != 0 || worker.HasEngineErrors) throw new InvalidOperationException($"Private-display worker failed ({code}); evidence: {display.EvidenceDirectory}\n{worker.Tail()}");
            });
        }
        catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
        { throw new TimeoutException($"UI suite exceeded {options.Timeout} ms; owned worker/display/peers were cleaned up. Evidence: {display.EvidenceDirectory}"); }
    }
    private async Task UiWorker()
    {
        PrivateDisplay.ValidateWorker(options);
        await Capture("xdpyinfo");
        string info = await Capture("glxinfo", "-B");
        Console.WriteLine(PrivateDisplay.RequireSoftwareGraphics(info));
        await File.WriteAllTextAsync(Path.Combine(_evidence.Directory, options.Scenario == "exported-package" ? "package-renderer.txt" : "source-renderer.txt"), info, cancellation);
        await using var wm = new Child("private-openbox", "openbox", ["--sm-disable"], _root, evidenceDirectory: _evidence.Directory, quiet: true);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation); deadline.CancelAfter(options.Timeout);
        string[] selected = options.Scenario is null ? ScenarioNames.Ui.Where(n => n != "exported-package").ToArray() : [options.Scenario];
        var scenarios = selected.Select(name => new Scenario(name, UiRisk(name), token => _evidence.Measure(name, "ui", async () =>
        {
            await using var owned = new ScenarioScope(name, _evidence, graphical: true);
            var worker = new Runner(options, token, _evidence, owned);
            await worker.UiScenario(name, token);
            await owned.DisposeAsync(); owned.CheckErrors();
        }))).ToArray();
        Console.WriteLine($"UI coverage: {(options.Scenario is null ? "all source slices" : "selected: " + options.Scenario)}; jobs=1; {string.Join(", ", selected)}");
        await ScenarioScheduler.Run(scenarios, 1, deadline.Token);
        if (wm.HasExited) throw new InvalidOperationException("Owned window manager exited unexpectedly.\n" + wm.Tail());
    }
}
