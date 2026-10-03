using System.Security.Cryptography;
using System.Text.Json;

namespace Game.Core.Tests.Profiling;

internal sealed class CampaignTrace : IDisposable
{
    private readonly IncrementalHash _commands = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
    private readonly List<object> _waves = [];
    private int _lastWave;
    public long Commands { get; private set; }
    public void Command(Command command)
    {
        Commands++;
        _commands.AppendData(JsonSerializer.SerializeToUtf8Bytes(command with { MatchId = "session" }, WireJson.Options));
    }
    public void Wave(Match match)
    {
        if (_lastWave == match.Wave) return;
        _lastWave = match.Wave;
        MatchSnapshot state = match.Snapshot() with { MatchId = "session" };
        _waves.Add(new { match.Wave, match.Tick, Digest = LargeBattle.Digest(state), Players = state.Players.Select(p => new { p.Id, p.Resources, Soldiers = p.Soldiers.Length, p.Health, p.LastUpkeep }) });
    }
    public object Finish(Match match, int ticks, int preparations)
    {
        MatchSnapshot final = match.Snapshot() with { MatchId = "session" };
        return new { Commands, CommandDigest = Convert.ToHexString(_commands.GetHashAndReset()), Waves = _waves.ToArray(), FinalDigest = LargeBattle.Digest(final), CombatTicks = ticks, Preparations = preparations, match.Phase, match.DefeatReason, match.ConfigurationFingerprint, match.CombatSeed };
    }
    public void Dispose() => _commands.Dispose();
}
