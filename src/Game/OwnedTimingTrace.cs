using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Game.Core;

namespace Game;

// Finite, opt-in owned Linux diagnostic. No wire fields or packet contents.
internal sealed class OwnedTimingTrace : IDisposable
{
    internal const int MaximumRecords = 8192;
    [StructLayout(LayoutKind.Sequential)]
    private struct Timespec { public long Seconds, Nanoseconds; }
    [DllImport("libc", EntryPoint = "clock_gettime", SetLastError = true)]
    private static extern int ClockGetTime(int clock, out Timespec time);
    internal static long Now()
    {
        if (!OperatingSystem.IsLinux() || ClockGetTime(1, out Timespec time) != 0) throw new InvalidOperationException("Owned timing requires Linux CLOCK_MONOTONIC.");
        return checked(time.Seconds * 1_000_000_000 + time.Nanoseconds);
    }
    internal static string Digest(string value) => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value)));
    private readonly StreamWriter _writer;
    private readonly object _gate = new();
    private int _ordinal;
    private bool _closed;
    internal bool Truncated { get; private set; }
    internal OwnedTimingTrace(string path, bool owned)
    {
        if (!owned || !Path.IsPathFullyQualified(path)) throw new InvalidOperationException("Timing trace requires validated test ownership and an absolute evidence path.");
        _writer = new StreamWriter(new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read));
        Record("clock", info: new
        {
            Clock = "clock_gettime(CLOCK_MONOTONIC=1)",
            Frequency = 1_000_000_000L,
            Alignment = "same Linux kernel/boot/time namespace; absolute monotonic nanoseconds, no per-process epoch subtraction",
            BootId = File.ReadAllText("/proc/sys/kernel/random/boot_id").Trim(),
            TimeNamespace = new FileInfo("/proc/self/ns/time").LinkTarget,
            Process = Environment.ProcessId,
            UtcAnchor = DateTimeOffset.UtcNow,
            MaximumRecords
        });
    }
    internal void Record(string kind, MatchSnapshot? state = null, string? digest = null, object? info = null)
    {
        lock (_gate)
        {
            if (_closed) return;
            long stamp = Now();
            if (_ordinal >= MaximumRecords)
            {
                if (!Truncated) { Truncated = true; _writer.WriteLine(JsonSerializer.Serialize(new { Kind = "overflow", Nanoseconds = stamp }, WireJson.Options)); _writer.Flush(); }
                return;
            }
            _writer.WriteLine(JsonSerializer.Serialize(new
            {
                Ordinal = ++_ordinal,
                Nanoseconds = stamp,
                Kind = kind,
                state?.MatchId,
                state?.Revision,
                state?.Tick,
                state?.Phase,
                state?.Paused,
                state?.Wave,
                state?.TurnSerial,
                state?.EventSequence,
                Digest = digest,
                Info = info
            }, WireJson.Options));
            if (_ordinal % 32 == 0 || kind.Contains("ack", StringComparison.Ordinal) || kind.Contains("register", StringComparison.Ordinal)) _writer.Flush();
        }
    }
    public void Dispose()
    {
        lock (_gate)
        {
            if (_closed) return;
            Record("end", info: new { Truncated }); _closed = true; _writer.Dispose();
        }
    }
}
