using System.Text.Json;
using Xunit;

namespace Game.Core.Tests;

public sealed class AuthorityCombatTests
{
    private static string Json(MatchSnapshot state) => JsonSerializer.Serialize(state, WireJson.Options);
    private static void Opening(Action<string, int, Building> act, Func<MatchSnapshot> snapshot)
    {
        act("start", -1, Building.Empty);
        act("build", 0, Building.Farm); act("build", 2, Building.Farm); act("build", 1, Building.Barracks);
        for (int production = 1; production <= 3; production++)
        {
            act("ready", -1, Building.Empty);
            while (snapshot().Players.Single().Food >= 5) act("recruit", 1, Building.Empty);
        }
        act("ready", -1, Building.Empty);
    }

    [Fact]
    public void CompleteWireStateRebuildsMovementDeathAndDecisionsWithoutHistory()
    {
        using var match = new Match(combatSeed: 1); match.Join();
        Opening((action, slot, building) => Assert.True(VillageStrategyTests.Act(match, 1, action, slot, building).Accepted), match.Snapshot);
        bool movement = false, death = false, recovery = false;
        for (int step = 0; step < 2000 && (match.Phase == Phase.Combat || match.Snapshot().DyingBodies.Length > 0); step++)
        {
            match.Step(); MatchSnapshot published = match.Snapshot();
            MatchSnapshot received = JsonSerializer.Deserialize<MatchSnapshot>(Json(published), WireJson.Options)!;
            Assert.Equal(Json(published), Json(received));
            Assert.Equal(1UL, received.CombatSeed);
            var configuration = new CombatConfiguration(received.Rules);
            Assert.Equal(configuration.Fingerprint, received.ConfigurationFingerprint);
            Assert.Equal(configuration.RulesVersion, received.CombatRulesVersion);
            UnitState[] units = CombatPlayback.All(received).Concat(received.DyingBodies).ToArray();
            Assert.All(units, u => { Assert.NotNull(u.Hex); Assert.NotNull(u.Decision); });
            var occupancy = new HexOccupancy(configuration.Board); occupancy.Rebuild(units.Select(u => ReservationOwner.FromSnapshot(u.Hex!)));
            Assert.Equal(JsonSerializer.Serialize(received.Reservations), JsonSerializer.Serialize(occupancy.Snapshot()));
            movement |= units.Any(u => u.Hex!.HoldsTransit);
            death |= received.DyingBodies.Length > 0;
            recovery |= units.Any(u => u.Hex!.Action == UnitActionKind.Recovery);
            Assert.InRange(received.CombatEvents.Length, 0, CombatSimulation.HistoryLimit);
            Assert.All(received.CombatEvents, e => Assert.InRange(e.Tick, Math.Max(0, received.Tick - CombatSimulation.HistoryTicks), received.Tick));
            Assert.Equal(received.CombatEvents.Length, received.CombatEvents.Select(e => e.Sequence).Distinct().Count());
            Assert.All(received.CombatEvents.Where(e => !e.TargetCity && (e.Type == CombatEventType.AttackStarted || e.Landed || e.Type == CombatEventType.Death && e.Unit?.Deployed == true)), e => Assert.NotNull(e.ImpactPose));
            if (received.DyingBodies.Length > 0)
            {
                var playback = new CombatPlayback();
                playback.Accept(received with { CombatEvents = [], OldestEventSequence = received.EventSequence + 1 }, baseline: true);
                Assert.Empty(playback.Drain());
                Assert.Equal(JsonSerializer.Serialize(units.OrderBy(u => u.Id)), JsonSerializer.Serialize(playback.Units()));
            }
        }
        Assert.True(movement && death && recovery); Assert.Equal(Phase.Building, match.Phase);
        Assert.Empty(match.Snapshot().DyingBodies); Assert.Equal(3, match.ProductionCount);
    }

    [Fact]
    public void AuthorityModesShareSeededCombatPauseCleanupAndEndedState()
    {
        using var solo = new AuthoritySession(AuthorityPolicy.Solo, combatSeed: 1);
        using var host = new AuthoritySession(AuthorityPolicy.PlayingHost, combatSeed: 1);
        using var dedicated = new AuthoritySession(AuthorityPolicy.Dedicated, combatSeed: 1);
        Assert.True(dedicated.Admit(2, WireJson.ProtocolVersion, "", 0).Accepted);
        AuthoritySession[] sessions = [solo, host, dedicated];
        long sequence = 0;
        void EqualStates()
        {
            string[] states = sessions.Select(s => Json(s.Snapshot() with { MatchId = "", Revision = 0 })).ToArray();
            Assert.All(states, s => Assert.Equal(states[0], s));
        }
        void Act(string action, int slot = -1, Building building = Building.Empty)
        {
            sequence++;
            foreach (AuthoritySession session in sessions)
            {
                MatchSnapshot state = session.Snapshot();
                var command = new Command(sequence, state.MatchId, state.Phase, state.TurnSerial, action, 1, slot, building);
                CommandResult result = session.Policy == AuthorityPolicy.Dedicated
                    ? session.Request(2, JsonSerializer.Serialize(command, WireJson.Options), 1000)!
                    : session.ExecuteLocal(command);
                Assert.True(result.Accepted, result.Message);
            }
            EqualStates();
        }
        Opening(Act, solo.Snapshot); bool pausedDeath = false;
        for (int step = 0; step < 2000 && (solo.Phase == Phase.Combat || solo.Snapshot().DyingBodies.Length > 0); step++)
        {
            foreach (AuthoritySession session in sessions) session.Step();
            EqualStates();
            if (!pausedDeath && solo.Snapshot().DyingBodies.Length > 0)
            {
                Act("pause"); string frozen = Json(solo.Snapshot());
                Assert.All(sessions, s => Assert.True(s.Paused));
                for (int tick = 0; tick < 12; tick++) foreach (AuthoritySession session in sessions) session.Step();
                EqualStates(); Assert.Equal(frozen, Json(solo.Snapshot()));
                Act("resume"); Assert.All(sessions, s => Assert.False(s.Paused)); pausedDeath = true;
            }
        }
        Assert.True(pausedDeath); Assert.Equal(Phase.Building, solo.Phase); Assert.Empty(solo.Snapshot().DyingBodies);
        foreach (AuthoritySession session in sessions)
        {
            string retained = Json(session.Snapshot()); session.End(); session.End(); session.Step();
            Assert.True(session.IsEnded); Assert.True(session.CombatReleased); Assert.Equal(retained, Json(session.Snapshot()));
            Assert.False(session.Admit(3, WireJson.ProtocolVersion, "", 2000).Accepted);
        }
        EqualStates();
    }
}
