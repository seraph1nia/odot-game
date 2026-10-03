using System.Diagnostics;

namespace Game.Core.Tests.Profiling;

internal sealed record MetricSample(double ElapsedMilliseconds, double CpuMilliseconds, long AllocatedBytes, int Gen0, int Gen1, int Gen2)
{
    public static MetricSample Difference(MetricSample start, MetricSample end) => new(
        end.ElapsedMilliseconds - start.ElapsedMilliseconds, end.CpuMilliseconds - start.CpuMilliseconds,
        end.AllocatedBytes - start.AllocatedBytes, end.Gen0 - start.Gen0, end.Gen1 - start.Gen1, end.Gen2 - start.Gen2);
}

// Process-wide metrics: no concurrent workloads, no forced GC, no inferred pause time.
internal sealed class Measurements : IDisposable
{
    private readonly Process _process = Process.GetCurrentProcess();
    private readonly CancellationToken _cancellation;
    private readonly long _deadline;
    private string? _phase;
    private long _phaseStart;
    private readonly Dictionary<string, double> _phases = new(StringComparer.Ordinal);
    public long BoundaryCalls { get; private set; }
    public Measurements(TimeSpan bound, CancellationToken cancellation)
    { _cancellation = cancellation; _deadline = Stopwatch.GetTimestamp() + checked((long)(bound.TotalSeconds * Stopwatch.Frequency)); }
    public MetricSample Sample() => new(Stopwatch.GetTimestamp() * 1000.0 / Stopwatch.Frequency,
        _process.TotalProcessorTime.TotalMilliseconds, GC.GetTotalAllocatedBytes(precise: true), GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
    public void Enter(string? phase)
    {
        BoundaryCalls++;
        _cancellation.ThrowIfCancellationRequested();
        long now = Stopwatch.GetTimestamp();
        if (now > _deadline) throw new TimeoutException("Profile execution exceeded its per-execution bound.");
        if (_phase == phase) return;
        if (_phase is not null) _phases[_phase] = _phases.GetValueOrDefault(_phase) + (now - _phaseStart) * 1000.0 / Stopwatch.Frequency;
        _phase = phase; _phaseStart = now;
    }
    public IReadOnlyDictionary<string, double> Finish() { Enter(null); return new Dictionary<string, double>(_phases, StringComparer.Ordinal); }
    internal static object Calibrate()
    {
        using var measurement = new Measurements(TimeSpan.FromSeconds(60), CancellationToken.None);
        long start = Stopwatch.GetTimestamp();
        for (int i = 0; i < 100000; i++) measurement.Enter(i % 2 == 0 ? "a" : "b");
        double boundaries = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        start = Stopwatch.GetTimestamp();
        for (int i = 0; i < 250; i++) _ = measurement.Sample();
        return new
        {
            BoundaryCalls = 100000,
            BoundaryMilliseconds = boundaries,
            NanosecondsPerBoundary = boundaries * 10,
            MetricReads = 250,
            MetricReadMilliseconds = Stopwatch.GetElapsedTime(start).TotalMilliseconds,
            Scope = "excluded calibration; approximate API/boundary cost, not a subtraction from workload timing"
        };
    }
    public void Dispose() => _process.Dispose();
}
