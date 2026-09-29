using System.Diagnostics;
using System.Text.Json;
using System.Threading.Channels;
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
    private readonly bool _game;
    private readonly bool _quiet;
    private readonly object _gate = new();
    private bool _disposed;

    public string Name { get; }
    public Task Exited { get; }
    public int ExitCode => _process.ExitCode;
    public bool HasExited => _process.HasExited;
    public bool HasEngineErrors { get; private set; }

    public Child(string name, string executable, IEnumerable<string> args, string directory, bool game = false, bool quiet = false, string? workingDirectory = null)
    {
        Name = name;
        _game = game;
        _quiet = quiet;
        Directory.CreateDirectory(Path.Combine(directory, "logs"));
        _log = new StreamWriter(Path.Combine(directory, "logs", $"{name}-{Guid.NewGuid():N}.log")) { AutoFlush = true };
        var start = new ProcessStartInfo(executable)
        {
            WorkingDirectory = workingDirectory ?? directory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true
        };
        foreach (string arg in args) start.ArgumentList.Add(arg);
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
        await Task.WhenAll(_stdout, _stderr);
        _events.Writer.TryComplete();
    }

    private async Task ReadOutput(StreamReader reader, bool error)
    {
        while (await reader.ReadLineAsync() is { } line)
        {
            lock (_gate)
            {
                if (line.StartsWith("ERROR:", StringComparison.Ordinal)) HasEngineErrors = true;
                _log.WriteLine((error ? "stderr: " : "") + line);
                _tail.Enqueue(line);
                while (_tail.Count > 25) _tail.Dequeue();
            }
            if (line.StartsWith(WireJson.EventPrefix, StringComparison.Ordinal))
            {
                GameEvent? value;
                try { value = JsonSerializer.Deserialize<GameEvent>(line[WireJson.EventPrefix.Length..], WireJson.Options); }
                catch (JsonException e) { value = new("error", Message: $"Malformed event: {e.Message}"); }
                if (value is not null)
                {
                    lock (_gate)
                    {
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
            catch (InvalidOperationException) { }
            try { await Exited.WaitAsync(TimeSpan.FromSeconds(2)); } catch (TimeoutException) { }
        }
        if (!_process.HasExited)
        {
            try { _process.Kill(entireProcessTree: true); }
            catch (InvalidOperationException) when (_process.HasExited) { }
            await Exited;
        }
        else await Exited;
        _log.Dispose(); _process.Dispose();
    }
}
