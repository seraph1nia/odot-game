using System.Security.Cryptography;
using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed record CombatReplayFrame(int Index, double Delta, int Focus, MatchSnapshot? Snapshot);
internal sealed record CombatReplayInput(string Version, string Digest, CombatReplayFrame[] Frames, int Commands, long FinalTick);

internal static class CombatReplayFixture
{
    internal static Match StartFight(int players, ulong seed, string? matchId = null)
    {
        var match = new Match(matchId: matchId, combatSeed: seed);
        try
        {
            for (int i = 0; i < players; i++) match.Join();
            Apply(match, 1, "start");
            while (match.Phase is Phase.Building or Phase.Preparation)
            {
                foreach (City city in match.Players.Values.Where(c => !c.Ready))
                {
                    int actions = 0;
                    while (CampaignStrategy.Next(match.Snapshot(), city.Id) is EconomyAction action)
                    {
                        if (++actions > 100) throw new InvalidOperationException("Replay economy policy exceeded its bound.");
                        CommandResult result = match.Apply(city.Id, action.Command(match.Snapshot(), city.Id));
                        if (!result.Accepted) throw new InvalidOperationException(result.Message);
                    }
                    Apply(match, city.Id, "ready");
                }
                if (match.Players.Values.All(c => c.Ready)) match.Step();
            }
            return match;
        }
        catch { match.Dispose(); throw; }
    }
    private static void Apply(Match match, int city, string action)
    {
        CommandResult result = match.Apply(city, new(1, match.Id, match.Phase, match.TurnSerial, action, city));
        if (!result.Accepted) throw new InvalidOperationException(result.Message);
    }
    internal static CombatReplayInput Generate()
    {
        using Match match = StartFight(2, 1, "combat-playback-profile");
        var frames = new List<CombatReplayFrame>(600);
        int commands = 0;
        for (int frame = 0; frame < 600; frame++)
        {
            if (frame == 150) { Apply(match, 1, "pause"); commands++; }
            if (frame == 170) { Apply(match, 1, "resume"); commands++; }
            if (frame > 0) match.Step();
            frames.Add(new(frame, 1.0 / 60, frame is >= 240 and < 360 ? 2 : 1,
                frame % 4 == 0 || frame is 150 or 170 ? match.Snapshot() : null));
        }
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(frames, WireJson.Options);
        return new("ordinary-combat-replay-v1", Convert.ToHexString(SHA256.HashData(bytes)), frames.ToArray(), commands, match.Tick);
    }
}
