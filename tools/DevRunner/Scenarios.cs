using System.Globalization;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Text.Json;

namespace DevRunner;

internal static class ScenarioNames
{
    public static readonly string[] Network = ["authority-resume-victory", "redistribution", "defeat", "failure-cases"];
    public static readonly string[] Ui = ["economy", "reconnect", "settings", "exported-package"];
}

internal sealed record Scenario(string Name, string Risk, Func<CancellationToken, Task> Execute);
internal sealed record Measurement(string Name, string Kind, string Result, DateTimeOffset Started, double Seconds, string? Condition);

internal sealed class Evidence
{
    public static JsonSerializerOptions JsonOptions { get; } = new() { WriteIndented = true };
    private readonly ConcurrentQueue<Measurement> _results = new();
    public string Directory { get; }
    public Evidence(string root, string? directory = null)
    {
        Directory = directory ?? Path.Combine(root, "logs", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + "-" + Guid.NewGuid().ToString("N")[..8]);
        System.IO.Directory.CreateDirectory(Directory);
        Console.WriteLine($"Evidence: {Directory}");
    }
    public async Task Measure(string name, string kind, Func<Task> action)
    {
        var started = DateTimeOffset.UtcNow;
        var timer = Stopwatch.StartNew();
        string result = "passed";
        string? condition = null;
        Console.WriteLine($"START {kind}: {name}");
        try { await action(); }
        catch (Exception error) { result = error is OperationCanceledException ? "cancelled" : "failed"; condition = error.Message; throw; }
        finally
        {
            var measurement = new Measurement(name, kind, result, started, timer.Elapsed.TotalSeconds, condition);
            _results.Enqueue(measurement);
            await File.WriteAllTextAsync(Path.Combine(Directory, name + ".json"), JsonSerializer.Serialize(measurement, JsonOptions));
            Console.WriteLine($"RESULT {kind}: {name} {result} {measurement.Seconds:F2}s{(condition is null ? "" : ": " + condition)}");
        }
    }
    public async Task Summary(string command, string coverage, int jobs, string result, double seconds)
    {
        var report = new { Command = command, Coverage = coverage, Jobs = jobs, Result = result, Seconds = seconds, Measurements = _results.ToArray() };
        string name = command == "_ui-worker" ? "ui-" + (coverage.Contains("exported-package", StringComparison.Ordinal) ? "package" : "source") : command.TrimStart('_');
        await File.WriteAllTextAsync(Path.Combine(Directory, name + "-summary.json"), JsonSerializer.Serialize(report, JsonOptions));
        Console.WriteLine($"{command}: {coverage}; jobs={jobs}; {result}; {seconds:F2}s. Evidence: {Directory}");
    }
}

internal static class ScenarioScheduler
{
    public static async Task Run(IReadOnlyList<Scenario> scenarios, int jobs, CancellationToken cancellation)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(jobs);
        using var siblings = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        var gate = new object();
        int next = 0;
        ExceptionDispatchInfo? failure = null;
        async Task Worker()
        {
            while (true)
            {
                Scenario scenario;
                lock (gate)
                {
                    if (failure is not null || siblings.IsCancellationRequested || next == scenarios.Count) return;
                    scenario = scenarios[next++];
                }
                try { await scenario.Execute(siblings.Token); }
                catch (Exception error)
                {
                    lock (gate)
                    {
                        failure ??= ExceptionDispatchInfo.Capture(error is OperationCanceledException ? error : new InvalidOperationException($"Scenario {scenario.Name}: {error.Message}", error));
                    }
                    siblings.Cancel();
                    return;
                }
            }
        }
        await Task.WhenAll(Enumerable.Range(0, Math.Min(jobs, scenarios.Count)).Select(_ => Worker()));
        failure?.Throw();
        cancellation.ThrowIfCancellationRequested();
    }
}

internal sealed class ScenarioScope : IAsyncDisposable
{
    private readonly List<Child> _children = [];
    private bool _disposed;
    private readonly List<int> _ports = [];
    public string Name { get; }
    public string Directory { get; } = Path.Combine(Path.GetTempPath(), "odot-test-" + Guid.NewGuid().ToString("N"));
    public string EvidenceDirectory { get; }
    public bool Graphical { get; }
    public ScenarioScope(string name, Evidence evidence, bool graphical = false)
    {
        Name = name; Graphical = graphical;
        EvidenceDirectory = Path.Combine(evidence.Directory, name);
        System.IO.Directory.CreateDirectory(Directory);
        System.IO.Directory.CreateDirectory(EvidenceDirectory);
        if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(Directory, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        Console.WriteLine($"OWNED {name}: runtime={Directory}; evidence={EvidenceDirectory}");
    }
    public Dictionary<string, string?> EnvironmentFor(string client)
    {
        string storage = Path.Combine(Directory, client);
        var env = new Dictionary<string, string?>();
        foreach (string category in new[] { "DATA", "CONFIG", "CACHE" })
        {
            string path = Path.Combine(storage, category.ToLowerInvariant());
            System.IO.Directory.CreateDirectory(path);
            env["XDG_" + category + "_HOME"] = path;
        }
        env["ODOT_OWNED_DATA"] = env["XDG_DATA_HOME"];
        if (Graphical) env["WAYLAND_DISPLAY"] = null;
        return env;
    }
    public Child Own(Child child) { _children.Add(child); return child; }
    public int Port() { int port = PortAllocator.Acquire(); _ports.Add(port); return port; }
    public void CheckErrors()
    {
        foreach (Child child in _children)
            if (child.HasUnexpectedEngineErrors || (!child.ExpectedFailure && child.ExitCode != 0))
                throw new InvalidOperationException($"{Name}/{child.Name}: unexpected engine error or exit {child.ExitCode}.\n{child.Tail()}");
    }
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        var failures = new List<Exception>();
        try
        {
            // Clients release presentation and transport before their server exits.
            foreach (Child child in Enumerable.Reverse(_children))
                try { await child.DisposeAsync(); } catch (Exception error) { failures.Add(error); }
        }
        finally
        {
            foreach (int port in _ports) PortAllocator.Release(port);
            if (System.IO.Directory.Exists(Directory)) System.IO.Directory.Delete(Directory, true);
        }
        if (failures.Count != 0) throw new AggregateException("Owned cleanup failed.", failures);
    }
}

internal static class VerificationGate
{
    public static async Task Run(Func<Task> sourceChecks, Func<Task> exports, Func<Task> packageChecks)
    {
        await sourceChecks();
        await exports();
        await packageChecks();
    }
}

internal static class PortAllocator
{
    private static readonly HashSet<int> Reserved = [];
    public static int Acquire()
    {
        lock (Reserved)
        {
            for (int n = 0; n < 100; n++)
            {
                using var socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
                socket.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 0));
                int port = ((System.Net.IPEndPoint)socket.LocalEndPoint!).Port;
                if (Reserved.Add(port)) return port;
            }
            throw new InvalidOperationException("Cannot allocate a distinct loopback port.");
        }
    }
    public static void Release(int port) { lock (Reserved) Reserved.Remove(port); }
}
