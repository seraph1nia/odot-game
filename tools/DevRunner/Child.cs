using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Channels;
using System.Runtime.InteropServices;
using Game.Core;

namespace DevRunner;

internal sealed class Child : IAsyncDisposable
{
    private readonly Process _process;
    private readonly StreamWriter _log;
    private readonly StreamWriter? _engineLog;
    private long _engineBytes;
    private string? _failureCondition;
    private int _simulationSpeed = 1;
    private bool _engineErrorOverflow;
    private readonly Task _stdout;
    private readonly Task _stderr;
    private readonly Channel<byte> _changed = Channel.CreateBounded<byte>(new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropWrite });
    private readonly ChildEvents _events = new();
    private readonly Queue<string> _tail = new();
    private readonly List<string> _engineErrors = [];
    private readonly bool _game;
    private readonly bool _quiet;
    private readonly object _gate = new();
    private bool _disposed;
    private readonly bool _ownsGroup;
    private int? _exitCode;
    private readonly bool _trace;
    private long _logBytes;
    private bool _logTruncated;
    private long _lastFlush;
    private bool _logClosed;
    public long LastTick { get { lock (_gate) return _events.LastTick; } }

    public string Name { get; }
    public int PlayerId { get; private set; }
    public int PeerId { get; private set; }
    public Task Exited { get; }
    public int ExitCode => _exitCode ?? _process.ExitCode;
    public bool HasExited => Exited.IsCompleted;
    public bool HasEngineErrors { get; private set; }
    public bool ExpectedFailure { get; set; }
    public string? AllowedEngineError { get; set; }
    public bool HasUnexpectedEngineErrors { get { lock (_gate) return _engineErrorOverflow || _engineErrors.Any(e => AllowedEngineError is null || !e.Contains(AllowedEngineError, StringComparison.Ordinal)); } }
    public string LogPath { get; }
    public int ProcessId => _process.Id;

    public Child(string name, string executable, IEnumerable<string> args, string directory, bool game = false, bool quiet = false, string? workingDirectory = null,
        IReadOnlyDictionary<string, string?>? environment = null, string? evidenceDirectory = null, bool ownsGroup = false, bool sanitizeSteam = false, bool trace = false, string? engineLogPath = null, int simulationSpeed = 1)
    {
        Name = name;
        _game = game;
        _quiet = quiet;
        _ownsGroup = ownsGroup;
        _trace = trace; _simulationSpeed = simulationSpeed;
        evidenceDirectory ??= Path.Combine(directory, "logs");
        Directory.CreateDirectory(evidenceDirectory);
        LogPath = Path.Combine(evidenceDirectory, $"{name}-{Guid.NewGuid():N}.log");
        _log = new StreamWriter(LogPath);
        _engineLog = engineLogPath is null ? null : new StreamWriter(engineLogPath);
        var start = new ProcessStartInfo(executable)
        {
            WorkingDirectory = workingDirectory ?? directory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true
        };
        foreach (string arg in args) start.ArgumentList.Add(arg);
        if (environment is not null) foreach (var (key, value) in environment)
            if (value is null) start.Environment.Remove(key); else start.Environment[key] = value;
        _process = new Process { StartInfo = start };
        try
        {
            if (!_process.Start()) throw new InvalidOperationException($"Could not start {executable}.");
        }
        catch
        {
            _log.Dispose(); _engineLog?.Dispose(); _process.Dispose(); throw;
        }
        _stdout = ReadOutput(_process.StandardOutput, false, sanitizeSteam);
        _stderr = ReadOutput(_process.StandardError, true, sanitizeSteam);
        Exited = FinishReading();
    }

    private async Task FinishReading()
    {
        await _process.WaitForExitAsync();
        _exitCode = _process.ExitCode;
        await Task.WhenAll(_stdout, _stderr);
        lock (_gate) { _log.Flush(); _engineLog?.Flush(); }
        _changed.Writer.TryComplete();
    }

    private async Task ReadOutput(StreamReader reader, bool error, bool sanitizeSteam)
    {
        while (await reader.ReadLineAsync() is { } line)
        {
            // The lobby identifier is public discovery data needed to run the guest side.
            // Other native diagnostics can contain account identifiers and are redacted.
            bool publicLobby = line.StartsWith(WireJson.EventPrefix, StringComparison.Ordinal)
                && line.Contains("\"Type\":\"steam-lobby\"", StringComparison.Ordinal);
            if (sanitizeSteam && !publicLobby) line = System.Text.RegularExpressions.Regex.Replace(line, @"\b\d{17}\b", "[SteamID redacted]");
            GameEvent? parsed = null;
            if (line.StartsWith("ODOT_UI ", StringComparison.Ordinal))
                parsed = new GameEvent("ui", Message: line[8..]);
            else if (line.StartsWith(WireJson.EventPrefix, StringComparison.Ordinal))
            {
                try { parsed = JsonSerializer.Deserialize<GameEvent>(line[WireJson.EventPrefix.Length..], WireJson.Options); }
                catch (JsonException e) { parsed = new("error", Message: $"Malformed event: {e.Message}"); }
            }
            string safe = DiagnosticText.Redact(!_trace && parsed is not null ? DiagnosticText.Compact(parsed) : line);
            lock (_gate)
            {
                if (line.StartsWith("ERROR:", StringComparison.Ordinal)) { HasEngineErrors = true; if (_engineErrors.Count < 512) _engineErrors.Add(safe[..Math.Min(safe.Length, 8192)]); else _engineErrorOverflow = true; }
                if (parsed is not null)
                {
                    if (parsed.Type == "connected") { PlayerId = parsed.PlayerId; PeerId = parsed.PeerId; }
                    if (parsed.Type == "pacing" && int.TryParse(parsed.Message?.Split(':').Last(), out int speed)) _simulationSpeed = speed;
                    _events.Add(parsed, System.Text.Encoding.UTF8.GetByteCount(line));
                }
                if (parsed is null && _engineLog is not null && _engineBytes < 8 * 1024 * 1024)
                {
                    _engineLog.WriteLine(safe); _engineBytes += System.Text.Encoding.UTF8.GetByteCount(safe) + 1;
                    if (_engineBytes >= 8 * 1024 * 1024) _engineLog.WriteLine("Engine diagnostic retention truncated at 8 MiB; parsed error results remain in supervisor evidence.");
                    if (error) _engineLog.Flush();
                }
                WriteLog((error ? "stderr: " : "") + safe, error || parsed?.Type is "error" or "ack");
                _tail.Enqueue(safe);
                while (_tail.Count > 25) _tail.Dequeue();
            }
            _changed.Writer.TryWrite(0);
            if (parsed is not null && parsed.Type != "snapshot" && parsed.Type != "ui") Console.WriteLine($"[{Name}] {parsed.Type} {DiagnosticText.Redact(parsed.Message ?? "")}");
            else if (parsed is null && (!_quiet || error)) Console.WriteLine($"[{Name}] {DiagnosticText.Redact(line)}");
        }
    }

    private void WriteLog(string line, bool important)
    {
        // Bound routine verbosity; important results/errors remain attributable.
        if (_logBytes < 8 * 1024 * 1024 || important || _trace)
        { _log.WriteLine(line); _logBytes += System.Text.Encoding.UTF8.GetByteCount(line) + 1; }
        else if (!_logTruncated) { _log.WriteLine("Routine diagnostic retention truncated at 8 MiB; errors/results and state ring retained."); _logTruncated = true; }
        if (important || Environment.TickCount64 - _lastFlush >= 1000)
        { _log.Flush(); _lastFlush = Environment.TickCount64; }
    }

    public void DumpEvidence(string condition)
    {
        lock (_gate)
        {
            if (condition != "Owned cleanup checkpoint") _failureCondition ??= condition;
            string content = JsonSerializer.Serialize(new
            {
                Condition = _failureCondition ?? condition,
                Trace = _trace,
                SimulationSpeed = _simulationSpeed,
                LastTick,
                TranscriptBytes = _logBytes,
                EngineBytes = _engineBytes,
                TruncatedStates = _events.DroppedStates,
                RecentUi = _events.RecentUi,
                States = _events.States()
            }, Evidence.JsonOptions);
            File.WriteAllText(LogPath + ".states.json", DiagnosticText.Redact(content));
            if (!_logClosed) { _log.Flush(); _engineLog?.Flush(); }
        }
    }

    public async Task<GameEvent> WaitFor(Func<GameEvent, bool> predicate, string expectation, int timeoutMs, CancellationToken cancellation)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        deadline.CancelAfter(timeoutMs);
        try
        {
            while (true)
            {
                lock (_gate)
                {
                    GameEvent? existing = _events.Find(predicate);
                    if (existing is not null) return existing;
                }
                if (!await _changed.Reader.WaitToReadAsync(deadline.Token)) break;
                while (_changed.Reader.TryRead(out _)) { }
            }
            throw new InvalidOperationException($"{Name} exited before {expectation} (exit {_process.ExitCode}).\n{Tail()}");
        }
        catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
        {
            DumpEvidence("Timeout: " + expectation);
            throw new TimeoutException($"{Name}: timed out waiting for {expectation} after {timeoutMs} ms.\n{Tail()}");
        }
        catch { DumpEvidence("Failed wait: " + expectation); throw; }
    }

    public GameEvent[] History() { lock (_gate) return _events.History(); }
    public string Tail() { lock (_gate) return string.Join(Environment.NewLine, _tail); }

    public async Task Send(string command)
    {
        await _process.StandardInput.WriteLineAsync(command);
        await _process.StandardInput.FlushAsync();
    }

    public async Task<int> WaitExit(CancellationToken cancellation)
    {
        try { await Exited.WaitAsync(cancellation); }
        catch { DumpEvidence("Exit wait cancelled or failed"); throw; }
        if (ExitCode != 0) DumpEvidence("Child exited with " + ExitCode);
        return ExitCode;
    }

    public void TerminateUnexpectedly()
    {
        if (_process.HasExited) throw new InvalidOperationException(Name + " exited before the owned host-loss check.");
        ExpectedFailure = true;
        _process.Kill(entireProcessTree: true);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        DumpEvidence("Owned cleanup checkpoint");
        if (!_process.HasExited && _game)
        {
            try { await Send("quit"); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            catch (InvalidOperationException) { }
            try { await Exited.WaitAsync(TimeSpan.FromSeconds(2)); } catch (TimeoutException) { }
        }
        if (!Exited.IsCompleted)
        {
            if (_ownsGroup)
            {
                SignalOwnedGroup(15);
                try { await Exited.WaitAsync(TimeSpan.FromSeconds(5)); } catch (TimeoutException) { SignalOwnedGroup(9); }
            }
            else if (!_process.HasExited)
            {
                try { _process.Kill(entireProcessTree: true); }
                catch (InvalidOperationException) when (_process.HasExited) { }
            }
            await Exited.WaitAsync(TimeSpan.FromSeconds(5));
        }
        else await Exited;
        if (_ownsGroup)
        {
            // xvfb-run's EXIT trap signals Xvfb without awaiting its final cache writes.
            // Drain the owned group before deleting the display's runtime directory.
            SignalOwnedGroup(15);
            var deadline = Stopwatch.StartNew();
            while (GroupRunning(_process.Id) && deadline.Elapsed < TimeSpan.FromSeconds(1)) await Task.Delay(25);
            if (GroupRunning(_process.Id)) SignalOwnedGroup(9);
            while (GroupRunning(_process.Id) && deadline.Elapsed < TimeSpan.FromSeconds(3)) await Task.Delay(25);
            if (GroupRunning(_process.Id)) throw new TimeoutException($"Owned process group {_process.Id} did not stop.");
        }
        lock (_gate) { _log.Dispose(); _engineLog?.Dispose(); _logClosed = true; }
        _process.Dispose();
    }
    private void SignalOwnedGroup(int signal)
    {
        if (Kill(-_process.Id, signal) == 0) return;
        int error = Marshal.GetLastPInvokeError();
        // Linux ESRCH: the owned process group has already exited.
        if (error == 3) return;
        throw new Win32Exception(error, $"Could not signal owned process group {_process.Id} with signal {signal}.");
    }
    private static bool GroupRunning(int group)
    {
        foreach (string directory in Directory.EnumerateDirectories("/proc"))
        {
            if (!int.TryParse(Path.GetFileName(directory), out _)) continue;
            try
            {
                string stat = File.ReadAllText(Path.Combine(directory, "stat"));
                string[] fields = stat[(stat.LastIndexOf(')') + 2)..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (fields[0] != "Z" && int.Parse(fields[2], CultureInfo.InvariantCulture) == group) return true;
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        return false;
    }
    [DllImport("libc", EntryPoint = "kill", SetLastError = true)]
    private static extern int Kill(int pid, int signal);
}
