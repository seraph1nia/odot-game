using System.Security.Cryptography;
using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed record CombatReplayFrame(int Index, double Delta, int Focus, MatchSnapshot? Snapshot,
    [property: System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)] string? View = null,
    [property: System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)] float Zoom = 0);
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
    internal static CombatReplayInput GenerateAuthored()
    {
        var frames = new List<CombatReplayFrame>(600);
        UnitType[] roles = Enum.GetValues<UnitType>();
        foreach (int population in new[] { 16, 64, 256 })
        {
            var rules = new Rules { Campaign = new([new(1, false, roles.Select(t => new SpawnEntry(t, 1, population / 8)).ToArray(), default)]) };
            using var match = new Match(rules, matchId: "authored-profile-" + population, combatSeed: 1);
            City city = match.Join()!;
            Apply(match, 1, "start");
            city.Gold = city.Food = city.Wood = city.Stone = city.Metal = city.Cloth = 10000;
            Building[] buildings = [Building.Farm, Building.Barracks, Building.MetalMine, Building.Lumbermill, Building.ArcheryRange, Building.Stonecutter, Building.Weaver, Building.Arcanum, Building.ArrowTower];
            int plots = population == 16 ? 2 : population == 64 ? 5 : 9;
            for (int slot = 0; slot < plots; slot++)
            {
                if (slot >= 5) ApplySlot("buy-plot", slot);
                ApplySlot("build", slot, buildings[slot]);
            }
            void ApplySlot(string action, int slot, Building building = Building.Empty)
            {
                CommandResult result = match.Apply(1, Command.FromSnapshot(match.Snapshot(), 1, action, 1, slot, building));
                if (!result.Accepted) throw new InvalidOperationException(result.Message);
            }
            int friends = population == 16 ? 6 : population == 64 ? 12 : 18;
            while (city.Snapshot().Army!.PurchasedHomes < friends / 3) ApplySlot("buy-home", -1);
            for (int n = 0; n < friends; n++)
            {
                UnitType role = n % 2 == 0 ? UnitType.Swordsman : UnitType.Berserker;
                CommandResult result = match.Apply(1, Command.FromSnapshot(match.Snapshot(), 1, "recruit", 1, 1, soldierType: role));
                if (!result.Accepted) throw new InvalidOperationException(result.Message);
            }
            Append("settlement-" + population, false);
            for (int turn = 0; turn < 4; turn++) Apply(match, 1, "ready");
            for (int tick = 0; tick < 300 && match.Phase == Phase.Combat; tick++) match.Step();
            Append("combat-" + population, true);
            void Append(string view, bool advancing)
            {
                for (int frame = 0; frame < 100; frame++)
                {
                    if (advancing && frame > 0) match.Step();
                    frames.Add(new(frames.Count, 1.0 / 60, 1, frame % 4 == 0 ? match.Snapshot() : null, view, frame < 50 ? 1 : 3));
                }
            }
        }
        return new("authored-scale-replay-v1", Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(frames, WireJson.Options))), frames.ToArray(), 0, 0);
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
