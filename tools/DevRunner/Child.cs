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
    private readonly Task _stdout;
    private readonly Task _stderr;
    private readonly Channel<GameEvent> _events = Channel.CreateUnbounded<GameEvent>();
    private readonly List<GameEvent> _history = new();
    private readonly Queue<string> _tail = new();
    private readonly List<string> _engineErrors = [];
    private readonly bool _game;
    private readonly bool _quiet;
    private readonly object _gate = new();
    private bool _disposed;
    private readonly bool _ownsGroup;
    private int? _exitCode;

    public string Name { get; }
    public int PlayerId { get; private set; }
    public int PeerId { get; private set; }
    public Task Exited { get; }
    public int ExitCode => _exitCode ?? _process.ExitCode;
    public bool HasExited => Exited.IsCompleted;
    public bool HasEngineErrors { get; private set; }
    public bool ExpectedFailure { get; set; }
    public string? AllowedEngineError { get; set; }
    public bool HasUnexpectedEngineErrors { get { lock (_gate) return _engineErrors.Any(e => AllowedEngineError is null || !e.Contains(AllowedEngineError, StringComparison.Ordinal)); } }
    public string LogPath { get; }
    public int ProcessId => _process.Id;

    public Child(string name, string executable, IEnumerable<string> args, string directory, bool game = false, bool quiet = false, string? workingDirectory = null,
        IReadOnlyDictionary<string, string?>? environment = null, string? evidenceDirectory = null, bool ownsGroup = false)
    {
        Name = name;
        _game = game;
        _quiet = quiet;
        _ownsGroup = ownsGroup;
        evidenceDirectory ??= Path.Combine(directory, "logs");
        Directory.CreateDirectory(evidenceDirectory);
        LogPath = Path.Combine(evidenceDirectory, $"{name}-{Guid.NewGuid():N}.log");
        _log = new StreamWriter(LogPath) { AutoFlush = true };
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
            _log.Dispose(); _process.Dispose(); throw;
        }
        _stdout = ReadOutput(_process.StandardOutput, false);
        _stderr = ReadOutput(_process.StandardError, true);
        Exited = FinishReading();
    }

    private async Task FinishReading()
    {
        await _process.WaitForExitAsync();
        _exitCode = _process.ExitCode;
        await Task.WhenAll(_stdout, _stderr);
        _events.Writer.TryComplete();
    }

    private async Task ReadOutput(StreamReader reader, bool error)
    {
        while (await reader.ReadLineAsync() is { } line)
        {
            lock (_gate)
            {
                if (line.StartsWith("ERROR:", StringComparison.Ordinal)) { HasEngineErrors = true; _engineErrors.Add(line); }
                _log.WriteLine((error ? "stderr: " : "") + line);
                _tail.Enqueue(line);
                while (_tail.Count > 25) _tail.Dequeue();
            }
            if (line.StartsWith("ODOT_UI ", StringComparison.Ordinal))
            {
                var value = new GameEvent("ui", Message: line[8..]);
                lock (_gate) _history.Add(value);
                _events.Writer.TryWrite(value);
            }
            else if (line.StartsWith(WireJson.EventPrefix, StringComparison.Ordinal))
            {
                GameEvent? value;
                try { value = JsonSerializer.Deserialize<GameEvent>(line[WireJson.EventPrefix.Length..], WireJson.Options); }
                catch (JsonException e) { value = new("error", Message: $"Malformed event: {e.Message}"); }
                if (value is not null)
                {
                    lock (_gate)
                    {
                        if (value.Type == "connected") { PlayerId = value.PlayerId; PeerId = value.PeerId; }
                        _history.Add(value);
                        if (_history.Count > 512) _history.RemoveAt(0);
                    }
                    _events.Writer.TryWrite(value);
                    if (value.Type != "snapshot") Console.WriteLine($"[{Name}] {value.Type} {value.Message}");
                }
            }
            else if (!_quiet || error) Console.WriteLine($"[{Name}] {line}");
        }
    }

    public async Task<GameEvent> WaitFor(Func<GameEvent, bool> predicate, string expectation, int timeoutMs, CancellationToken cancellation)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        deadline.CancelAfter(timeoutMs);
        try
        {
            lock (_gate)
            {
                GameEvent? existing = _history.LastOrDefault(predicate);
                if (existing is not null) return existing;
            }
            while (await _events.Reader.WaitToReadAsync(deadline.Token))
            {
                while (_events.Reader.TryRead(out GameEvent? value))
                {
                    if (predicate(value)) return value;
                    if (value.Type == "error") throw new InvalidOperationException(value.Message);
                }
            }
            throw new InvalidOperationException($"{Name} exited before {expectation} (exit {_process.ExitCode}).\n{Tail()}");
        }
        catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
        {
            throw new TimeoutException($"{Name}: timed out waiting for {expectation} after {timeoutMs} ms.\n{Tail()}");
        }
    }

    public GameEvent[] History() { lock (_gate) return _history.ToArray(); }
    public string Tail() { lock (_gate) return string.Join(Environment.NewLine, _tail); }

    public async Task Send(string command)
    {
        await _process.StandardInput.WriteLineAsync(command);
        await _process.StandardInput.FlushAsync();
    }

    public async Task<int> WaitExit(CancellationToken cancellation)
    {
        await Exited.WaitAsync(cancellation);
        return ExitCode;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
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
                Kill(-_process.Id, 15);
                try { await Exited.WaitAsync(TimeSpan.FromSeconds(5)); } catch (TimeoutException) { Kill(-_process.Id, 9); }
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
            Kill(-_process.Id, 15);
            var deadline = Stopwatch.StartNew();
            while (GroupRunning(_process.Id) && deadline.Elapsed < TimeSpan.FromSeconds(1)) await Task.Delay(25);
            if (GroupRunning(_process.Id)) Kill(-_process.Id, 9);
            while (GroupRunning(_process.Id) && deadline.Elapsed < TimeSpan.FromSeconds(3)) await Task.Delay(25);
            if (GroupRunning(_process.Id)) throw new TimeoutException($"Owned process group {_process.Id} did not stop.");
        }
        _log.Dispose(); _process.Dispose();
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
                if (fields[0] != "Z" && int.Parse(fields[2]) == group) return true;
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        return false;
    }
    [DllImport("libc", EntryPoint = "kill", SetLastError = true)]
    private static extern int Kill(int pid, int signal);
}
