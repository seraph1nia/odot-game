using System.Text.Json;
using Game.Core;

// Private client data. Never include credentials in diagnostics or public snapshots.
public sealed record LocalSession(string Endpoint, string MatchId, string Token, long NextSequence);
public sealed class SessionFile(string path, string endpoint)
{
    public LocalSession? Value { get; private set; }
    public void Load()
    {
        if (!File.Exists(path)) return;
        LocalSession? stored = JsonSerializer.Deserialize<LocalSession>(File.ReadAllText(path));
        if (stored?.Endpoint == endpoint && stored.NextSequence > 0) Value = stored;
    }
    public void Welcome(string matchId, string token)
    {
        Value = new(endpoint, matchId, token, Value?.MatchId == matchId ? Value.NextSequence : 1); Save();
    }
    public long Reserve()
    {
        LocalSession stored = Value ?? throw new InvalidOperationException("Session not initialized.");
        long sequence = stored.NextSequence;
        Value = stored with { NextSequence = checked(sequence + 1) }; Save(); return sequence;
    }
    public void Clear() { Value = null; if (File.Exists(path)) File.Delete(path); }
    private void Save()
    {
        string full = Path.GetFullPath(path); Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        string temp = full + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(Value));
        if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(temp, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        File.Move(temp, full, true);
    }
}
