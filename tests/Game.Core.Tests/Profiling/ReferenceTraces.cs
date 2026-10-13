using System.Security.Cryptography;
using System.Text.Json;
using DevRunner;
using Xunit;

namespace Game.Core.Tests.Profiling;

internal static class ReferenceTraces
{
    internal static IReadOnlyDictionary<string, string> Capture(bool observe = false)
    {
        var values = new SortedDictionary<string, string>(StringComparer.Ordinal);
        void Combat(string name, bool wounded)
        {
            using var combat = new CombatSimulation(new(), seed: 8);
            combat.ConfigureArena([1, 2]);
            int actor = CombatFixture.Group(combat, UnitType.Swordsman, 1, 1, Faction.Adventurers, 23)[0];
            int enemy = CombatFixture.Group(combat, UnitType.Swordsman, 1, 0, Faction.Skeletons, 24)[0];
            if (wounded)
            {
                combat.Seed(combat.Read(actor) with { Health = 100 }); combat.Seed(combat.Read(enemy) with { Health = 100 });
                for (int origin = 1; origin <= 10; origin++)
                    for (int member = 0; member < 3; member++) combat.Create(UnitType.Swordsman, 0, origin, 1, Faction.Skeletons);
            }
            else CombatFixture.Group(combat, UnitType.Swordsman, 1, 0, Faction.Skeletons, 25);
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            long cursor = 0;
            for (int tick = 1; tick <= 90; tick++)
            {
                if (observe) { _ = combat.Snapshot(); _ = combat.SquadSnapshots(); _ = combat.Events().Reverse().ToArray(); }
                combat.Step(tick, []);
                if (!wounded && tick == 30)
                {
                    UnitState before = combat.Read(enemy); combat.Transfer(enemy, 2); UnitState after = combat.Read(enemy);
                    Assert.Equal(before.Hex!.Position, after.Hex!.Position); Assert.Equal(before.SquadId, after.SquadId);
                    Assert.Equal(before.Health, after.Health); Assert.Equal(before.Profile, after.Profile);
                    Assert.Equal(before.ImpactTick, after.ImpactTick); Assert.Equal(before.ReadyTick, after.ReadyTick);
                }
                CombatReservations rebuilt = CombatReservations.Reconstruct(combat.Arena, combat.SquadSnapshots());
                Assert.Equal(combat.Reservations.Positions, rebuilt.Positions); Assert.Equal(combat.Reservations.Transit, rebuilt.Transit);
                CombatEvent[] events = combat.Events().Where(e => e.Sequence > cursor).ToArray();
                if (events.Length > 0) cursor = events[^1].Sequence;
                hash.AppendData(JsonSerializer.SerializeToUtf8Bytes(new { Tick = tick, Units = combat.Snapshot(), Dying = combat.Dying(), combat.Reservations, Events = events }, WireJson.Options));
            }
            if (wounded) Assert.Contains(combat.Events(), e => e.Type == CombatEventType.Death);
            values[name] = Convert.ToHexString(hash.GetHashAndReset());
        }
        Combat("squad-route-target-ties-sector-redirection", false); Combat("simultaneous-squad-death-real-queued-entry", true);
        CombatReplayInput replay = CombatReplayFixture.Generate();
        values["ordinary-playback-pause-resume"] = replay.Digest;
        using (var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
        {
            var playback = new CombatPlayback();
            foreach (CombatReplayFrame frame in replay.Frames)
            {
                if (frame.Snapshot is { } state) playback.Accept(state);
                playback.Advance(frame.Delta, true);
                hash.AppendData(JsonSerializer.SerializeToUtf8Bytes(new { frame.Index, playback.Tick, playback.VisualSeconds, Units = playback.Units(), Events = playback.Drain(), playback.EventCursor }, WireJson.Options));
            }
            values["ordinary-playback-output"] = Convert.ToHexString(hash.GetHashAndReset());
        }
        using Match terminal = new(new Rules { Campaign = new([new(1, false, [new(UnitType.Swordsman, 1)], default)]) }, matchId: "session", combatSeed: 8);
        City city = terminal.Join()!; city.Food = 100;
        Assert.True(VillageStrategyTests.Act(terminal, 1, "start").Accepted);
        for (int i = 0; i < 6; i++) terminal.Combat.Create(UnitType.Swordsman, 1, 1, 1, assignment: new(-1, 0, i / 3, 1));
        for (int i = 0; i < 4; i++) Assert.True(VillageStrategyTests.Act(terminal, 1, "ready").Accepted);
        using var stateHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        for (int i = 0; i < 1000 && (terminal.Phase == Phase.Combat || terminal.Combat.HasDeaths); i++)
        {
            if (i == 20)
            {
                Assert.True(VillageStrategyTests.Act(terminal, 1, "pause").Accepted);
                terminal.Step(); Assert.True(terminal.Paused);
                Assert.True(VillageStrategyTests.Act(terminal, 1, "resume").Accepted);
            }
            terminal.Step(); stateHash.AppendData(JsonSerializer.SerializeToUtf8Bytes(terminal.Snapshot(), WireJson.Options));
        }
        Assert.Equal(Phase.Victory, terminal.Phase);
        values["pause-resume-terminal-cleanup"] = Convert.ToHexString(stateHash.GetHashAndReset());
        return values;
    }
}
