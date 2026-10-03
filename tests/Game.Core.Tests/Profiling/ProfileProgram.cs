using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Xunit.Abstractions;

namespace Game.Core.Tests.Profiling;

internal static class ProfileProgram
{
    private static readonly JsonSerializerOptions EvidenceJson = new() { WriteIndented = true };
    public static int Main(string[] args)
    {
        if (args.Length == 2 && args[1] == "--help" && args[0] is "profile-campaign" or "profile-scale" or "profile-snapshots" or "test-scale")
        {
            Console.WriteLine("Engine-free profiles: --configuration Debug|Release (default Release), --seed UINT64 (default 1), --work-counters; serial owned workers, 600s each.\nprofile-campaign: --strategy frontline|mixed|towers|research --players 1..4 (co-op frontline only).\nprofile-scale: --scenario large-battle --sizes 128,512,2048.\ntest-scale: --scenario large-battle (2,048 actors, exactly 600 normal ticks).\nprofile-snapshots: --scenario ordinary-and-large.\nProfiles: --iterations 1..10 (default 3), one separate warm-up excluded from summaries. Test-scale runs once without warm-up.");
            return 0;
        }
        if (args.FirstOrDefault() == "_profile-reference-capture") { Console.WriteLine(JsonSerializer.Serialize(ReferenceTraces.Capture(), EvidenceJson)); return 0; }
        if (args.FirstOrDefault() == "_profile-lifecycle-probe") { Thread.Sleep(Timeout.Infinite); return 0; }
        if (args.FirstOrDefault() == "_profile-worker") return Worker(args);
        if (args.Length == 0) return Odot.Verification.InProcessTests.Main();
        using var cancellation = new CancellationTokenSource();
        ConsoleCancelEventHandler cancel = (_, e) => { e.Cancel = true; cancellation.Cancel(); };
        Console.CancelKeyPress += cancel;
        string? directory = null;
        try
        {
            ProfileOptions options = ProfileOptions.Parse(args);
            string configuration = new DirectoryInfo(AppContext.BaseDirectory).Parent!.Name;
            if (options.Configuration != configuration) throw new ArgumentException($"Executable configuration is {configuration}; requested {options.Configuration}.");
            string root = FindRoot();
            directory = Path.Combine(root, "logs", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture) + "-" + options.Command + "-" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(directory);
            Write(directory, "identity", Identity(root, options));
            var samples = new List<ProfileSample>();
            int[] sizes = options.Command == "profile-scale" ? options.Sizes ?? [128, 512, 2048] : options.Command == "test-scale" ? [2048] : [0];
            foreach (int size in sizes)
                for (int iteration = 0; iteration <= (options.Command == "test-scale" ? 0 : options.Iterations); iteration++)
                {
                    string sampleName = $"size-{size}-iteration-{iteration}";
                    var start = new ProcessStartInfo("dotnet") { UseShellExecute = false };
                    start.ArgumentList.Add(typeof(ProfileProgram).Assembly.Location);
                    foreach (string argument in new[] { "_profile-worker", directory, size.ToString(System.Globalization.CultureInfo.InvariantCulture), iteration.ToString(System.Globalization.CultureInfo.InvariantCulture) }.Concat(args)) start.ArgumentList.Add(argument);
                    OwnedProfileProcess.Run(start, TimeSpan.FromSeconds(600), cancellation.Token).GetAwaiter().GetResult();
                    ProfileSample sample = JsonSerializer.Deserialize<ProfileSample>(File.ReadAllText(Path.Combine(directory, sampleName + ".json")))!;
                    if (!sample.Warmup) samples.Add(sample);
                    Console.WriteLine($"{options.Command}/{size}: {(sample.Warmup ? "warmup" : "iteration " + iteration)} completed; {directory}");
                }
            Write(directory, "samples", samples);
            Write(directory, "summary", samples.GroupBy(s => s.Size).ToDictionary(g => g.Key, g => Summarize(g.ToArray())));
            return 0;
        }
        catch (Exception e)
        {
            if (directory is not null) Write(directory, "supervision-failure", new { Error = e.ToString(), Complete = false });
            Console.Error.WriteLine(e.Message + (directory is null ? "" : " Evidence: " + directory)); return 1;
        }
        finally { Console.CancelKeyPress -= cancel; }
    }
    private static int Worker(string[] args)
    {
        string directory = args[1]; int size = int.Parse(args[2], System.Globalization.CultureInfo.InvariantCulture);
        int iteration = int.Parse(args[3], System.Globalization.CultureInfo.InvariantCulture);
        ProfileOptions options = ProfileOptions.Parse(args[4..]);
        using var cancellation = new CancellationTokenSource();
        _ = Task.Run(async () => { _ = await Console.In.ReadLineAsync(); cancellation.Cancel(); });
        var diagnostics = new ProfileOutput();
        object calibration = Measurements.Calibrate();
        using var measurement = new Measurements(TimeSpan.FromSeconds(600), cancellation.Token);
        MetricSample start = measurement.Sample();
        try
        {
            object? workload = null;
            WorkCounters? work = options.WorkCounters ? new() : null;
            if (options.Command == "profile-campaign")
            {
                using var trace = new CampaignTrace();
                workload = CampaignAcceptance.Run(diagnostics, options.Strategy, options.Players, options.Seed, measurement, work, trace);
            }
            else if (options.Command is "profile-scale" or "test-scale") workload = LargeBattle.Run(size, options.Seed, measurement, work);
            else if (options.Command == "profile-snapshots") workload = SnapshotProfile.Run(measurement, work, options.Seed);
            else throw new ArgumentException("This workload is not implemented yet.");
            bool warmup = iteration == 0 && options.Command != "test-scale";
            var sample = new ProfileSample(iteration, warmup, MetricSample.Difference(start, measurement.Sample()), measurement.Finish(), diagnostics.Lines, diagnostics.Characters, diagnostics.Recent.LastOrDefault(), size, workload, options.Command == "profile-snapshots" ? null : work?.Snapshot(), measurement.BoundaryCalls, calibration, Environment.ProcessId);
            Write(directory, $"size-{size}-iteration-{iteration}", sample);
            return 0;
        }
        catch (Exception e)
        {
            Write(directory, $"failure-size-{size}-iteration-{iteration}", new { Iteration = iteration, Error = e.ToString(), Recent = diagnostics.Recent.ToArray(), Metrics = MetricSample.Difference(start, measurement.Sample()) });
            Console.Error.WriteLine(e.Message); return 1;
        }
    }
    internal static object Summarize(IReadOnlyList<ProfileSample> samples)
    {
        static object Distribution(IEnumerable<double> values)
        {
            double[] ordered = values.Order().ToArray();
            int middle = ordered.Length / 2;
            return new { Count = ordered.Length, Median = ordered.Length % 2 == 0 ? (ordered[middle - 1] + ordered[middle]) / 2 : ordered[middle], Minimum = ordered[0], Maximum = ordered[^1] };
        }
        return new { ElapsedMilliseconds = Distribution(samples.Select(s => s.Metrics.ElapsedMilliseconds)), CpuMilliseconds = Distribution(samples.Select(s => s.Metrics.CpuMilliseconds)), AllocatedBytes = Distribution(samples.Select(s => (double)s.Metrics.AllocatedBytes)), PhasesMilliseconds = samples.SelectMany(s => s.PhasesMilliseconds.Keys).Distinct().ToDictionary(p => p, p => Distribution(samples.Select(s => s.PhasesMilliseconds.GetValueOrDefault(p))), StringComparer.Ordinal) };
    }
    internal static string FindRoot()
    {
        for (var current = new DirectoryInfo(AppContext.BaseDirectory); current is not null; current = current.Parent)
            if (File.Exists(Path.Combine(current.FullName, "Odot.slnx"))) return current.FullName;
        throw new DirectoryNotFoundException("Odot.slnx was not found above the executable.");
    }
    private static object Identity(string root, ProfileOptions options)
    {
        var inputs = new SortedDictionary<string, string>(StringComparer.Ordinal);
        void Visit(string directory)
        {
            foreach (string path in Directory.EnumerateFiles(directory).Order(StringComparer.Ordinal))
                inputs[Path.GetRelativePath(root, path)] = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
            foreach (string child in Directory.EnumerateDirectories(directory).Order(StringComparer.Ordinal))
                if (Path.GetFileName(child) is not ("bin" or "obj" or ".godot")) Visit(child);
        }
        foreach (string area in new[] { "src", "tests", "tools", ".github", ".mise" }) Visit(Path.Combine(root, area));
        foreach (string file in new[] { "mise.toml", "mise.lock", "global.json", "Directory.Build.props", ".editorconfig", "Odot.slnx" })
            inputs[file] = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(root, file))));
        return new
        {
            Schema = "odot-profile-v1",
            Options = options,
            SourceDigest = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(inputs))),
            Inputs = inputs,
            Runtime = RuntimeInformation.FrameworkDescription,
            Sdk = File.ReadAllText(Path.Combine(root, "global.json")),
            Os = RuntimeInformation.OSDescription,
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            LogicalProcessors = Environment.ProcessorCount,
            Machine = Environment.MachineName,
            Cpu = File.Exists("/proc/cpuinfo") ? File.ReadLines("/proc/cpuinfo").FirstOrDefault(l => l.StartsWith("model name", StringComparison.Ordinal)) : null,
            Concurrency = 1,
            WorkCounterSchemaVersion = WorkCounters.SchemaVersion,
            WarmupExecutions = 1,
            PerExecutionBoundSeconds = 600,
            OverallBoundSeconds = 600 * (options.Command == "test-scale" ? 1 : (options.Iterations + 1) * (options.Command == "profile-scale" ? (options.Sizes?.Length ?? 3) : 1)),
            Isolation = "one serial owned worker per execution; separate warm-up warms filesystem/runtime caches, each worker includes its own JIT",
            DetailedCampaignTrace = Environment.GetEnvironmentVariable("ODOT_CAMPAIGN_TRACE") == "1",
            MetricsScope = "process; managed allocation includes all threads; GC counts are collections, not pauses",
            Phases = "monotonic wall time; boundary checks/timestamps included; no per-operation timers"
        };
    }
    internal static void Write(string directory, string name, object value) => File.WriteAllText(Path.Combine(directory, name + ".json"), JsonSerializer.Serialize(value, EvidenceJson));
}

internal sealed record ProfileSample(int Iteration, bool Warmup, MetricSample Metrics, IReadOnlyDictionary<string, double> PhasesMilliseconds, long Lines, long Characters, string? Summary, int Size = 0, object? Workload = null, IReadOnlyDictionary<WorkMetric, long?>? Work = null, long BoundaryCalls = 0, object? Calibration = null, int ProcessId = 0);

internal sealed class ProfileOutput : ITestOutputHelper
{
    public Queue<string> Recent { get; } = new();
    public long Lines { get; private set; }
    public long Characters { get; private set; }
    public void WriteLine(string message) { Lines++; Characters += message.Length; Recent.Enqueue(message); if (Recent.Count > 64) Recent.Dequeue(); }
    public void WriteLine(string format, params object[] args) => WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, format, args));
}
