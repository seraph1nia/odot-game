using System.Text.Json;

namespace Game;

// Private client data. Never include credentials in diagnostics or public snapshots.
// Optional namespace fields preserve existing ENet files for the same running match.
public sealed record LocalSession(string Endpoint, string MatchId, string Token, long NextSequence,
    string Transport = "enet", string Application = "odot", string OriginalHostIdentity = "")
{
    public override string ToString() => $"LocalSession {{ Transport = {Transport}, Application = {Application}, MatchId = {MatchId} }}";
}
public sealed class SessionFile(string path, string endpoint, string transport = "enet", string application = "odot", string originalHostIdentity = "")
{
    public LocalSession? Value { get; private set; }
    public string Transport => transport;
    public void Load()
    {
        Value = null;
        if (!File.Exists(path)) return;
        LocalSession? stored = JsonSerializer.Deserialize<LocalSession>(File.ReadAllText(path));
        if (stored?.Endpoint == endpoint && stored.Transport == transport && stored.Application == application
            && stored.OriginalHostIdentity == originalHostIdentity && stored.NextSequence > 0) Value = stored;
    }
    public void Welcome(string matchId, string token)
    {
        Value = new(endpoint, matchId, token, Value?.MatchId == matchId ? Value.NextSequence : 1, transport, application, originalHostIdentity); Save();
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
