using System.Text.Json;
using DevRunner;
using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

// Tier A earned history ONLY. The exact default paid prefix/horizon could not
// produce the continuing eliminated stored-owner conjunction. Original failed
// evidence remains019; complete mapped A/C spectator guards qualified021 before
// retiring that fragile combined consumer. This is not balance/full-session or
// eliminated-spectator acceptance. Thin actual authority/paid/codec observations,
// seed1, bounded paid prefix +nineReady, finite4000-tick W3; no grants/new simulator.
public sealed class PaidStoredVeteranHistoryTests(ITestOutputHelper output)
{
    [Fact]
    public void ServedVeteransKeepPaidStorageIdentityAndFundedHealingThroughoutFixedDefaultHorizon()
    {
        using var flow = new SessionFlow(players: 3, seed: 1);
        flow.Act(1, new("start"));
        var ledger = new List<object>();
        Command Paid(int player, EconomyAction action)
        {
            MatchSnapshot before = flow.State;
            Command command = flow.Act(player, action);
            MatchSnapshot after = Receive(flow.State);
            PaidCampaignFixture.AssertPaidTransition(before, after, player, action);
            ledger.Add(new
            {
                command.Action,
                Player = player,
                before.Wave,
                before.Phase,
                before.Turn,
                Before = before.Players,
                After = after.Players,
                before.ProductionCount,
                AfterProduction = after.ProductionCount
            });
            return command;
        }
        _ = PreparePaidPrefix(flow, Paid, ledger);
        int prefixCommands = ledger.Count;
        MatchSnapshot prefix = Receive(flow.State);
        Assert.Equal(6, prefix.ProductionCount);
        Assert.Equal(Phase.Building, prefix.Phase); Assert.Equal(3, prefix.Wave);
        UnitState[] earned = prefix.Players[2].Soldiers;
        Assert.NotEmpty(earned);
        Assert.All(earned, u => { Assert.True(u.Experience >= 1); Assert.True(u.Assignment!.Stored); });
        flow.StepUntil(s => s.DyingBodies.Length == 0, 4000);
        for (int gate = 0; gate < 3; gate++)
            foreach (int player in prefix.Players.Where(c => !c.Eliminated).Select(c => c.Id)) Paid(player, new("ready"));
        MatchSnapshot entry = Receive(flow.State);
        Assert.Equal(Phase.Combat, entry.Phase); Assert.Equal(9, entry.ProductionCount);
        Assert.Equal(prefixCommands + 9, ledger.Count);
        UnitState[] stored = entry.Players[2].Soldiers;
        Assert.Equal(earned.Select(u => u.Id), stored.Select(u => u.Id));
        foreach (UnitState before in earned)
        {
            UnitState after = stored.Single(u => u.Id == before.Id);
            Assert.Equal(Json(Member(before with { Health = after.Health })), Json(Member(after)));
            Assert.InRange(after.Health, before.Health, after.Profile.Health);
        }
        var observations = new List<object>();
        long lastEvent = 0;
        bool continuingStoredElimination = false;
        for (int step = 0; step <= 4000; step++)
        {
            MatchSnapshot current = flow.State;
            CityState owner = current.Players[2];
            Assert.Equal(Json(stored.Select(Member)), Json(owner.Soldiers.Select(Member)));
            Assert.All(owner.Soldiers, u =>
            {
                Assert.True(u.Assignment!.Stored); Assert.False(u.Participating);
                Assert.Equal(UnitLifecycle.Stored, u.Hex!.Lifecycle); Assert.Equal(default, u.Hex.Position);
                Assert.DoesNotContain(current.Reservations.Positions, p => p.UnitId == u.SquadId);
            });
            CombatEvent[] events = current.CombatEvents.Where(e => e.Sequence > lastEvent).ToArray();
            if (events.Length > 0) lastEvent = events[^1].Sequence;
            observations.Add(new
            {
                current.Tick,
                current.Wave,
                current.Phase,
                Cities = current.Players.Select(c => new
                { c.Id, c.Health, c.Eliminated, Stored = c.Soldiers.Where(u => u.Assignment is { Stored: true }).Select(Member).ToArray() }).ToArray(),
                Events = events
            });
            continuingStoredElimination |= current.Phase == Phase.Combat && owner.Eliminated
                && current.Players.Any(c => c.Id != owner.Id && !c.Eliminated);
            if (current.Phase != Phase.Combat || step == 4000) break;
            flow.Session.Step();
        }
        MatchSnapshot boundary = Receive(flow.State);
        Assert.NotEqual(Phase.Combat, boundary.Phase); // finite actual boundary, not missing-witness success
        if (boundary.DefeatReason == DefeatReason.AllCitiesFallen)
        {
            Assert.Equal(Phase.Defeat, boundary.Phase);
            Assert.All(boundary.Players, c => Assert.True(c.Eliminated));
        }
        string? directory = Environment.GetEnvironmentVariable("ODOT_CAMPAIGN_EVIDENCE_DIRECTORY");
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "paid-stored-history.json"), Json(new
            { Ledger = ledger, Prefix = prefix, Entry = entry, Observations = observations, ContinuingStoredElimination = continuingStoredElimination, Boundary = boundary }));
        }
        output.WriteLine($"Earned storage history ONLY: states={observations.Count}, phase={boundary.Phase}, tick={boundary.Tick}, continuingStoredElimination={continuingStoredElimination}; no default-paid spectator/balance qualification.");
    }

    // Thin accepted-command transcript shared with controlled receiving. Never
    // seeds profiles/HP/resources/flags or implements combat/economy itself.
    internal static Command PreparePaidPrefix(SessionFlow flow, Func<int, EconomyAction, Command> paid, List<object>? ledger = null, Action<MatchSnapshot>? observe = null)
    {
        Command? purchase = null;
        for (int wave = 1; wave <= 2; wave++)
        {
            flow.StepUntil(s => s.DyingBodies.Length == 0);
            for (int gate = 0; gate < 3; gate++)
            {
                foreach (var (player, action) in CampaignStrategy.EliminatedSupportPreparation(flow.State)) paid(player, action);
                for (int action = 0; CampaignStrategy.SpectatorReserveOpening(flow.State, 3) is EconomyAction plan; action++)
                {
                    Assert.True(action < 100);
                    Command command = paid(3, plan);
                    if (plan.Action == "recruit") purchase = command;
                }
                foreach (int player in new[] { 1, 2, 3 }) paid(player, new("ready"));
            }
            MatchSnapshot entry = flow.State;
            MatchSnapshot outcome = flow.StepUntil(s =>
            {
                observe?.Invoke(s);
                return s.Players[2].Eliminated || s.Phase != Phase.Combat;
            }, 4000);
            ledger?.Add(new { Action = "actual combat outcome", Before = entry, After = outcome });
            Assert.Equal(Phase.Building, outcome.Phase); Assert.False(outcome.Players[2].Eliminated);
            if (wave == 1)
            {
                Assert.NotEmpty(outcome.Players[2].Soldiers);
                Assert.All(outcome.Players[2].Soldiers, u => Assert.True(u.Experience >= 1));
            }
        }
        Assert.NotNull(purchase);
        return purchase;
    }
    // Explicit fields, not ValueTuple (production JSON serializes tuples to{}).
    private static object Member(UnitState member) => new
    { member.Id, member.Health, member.Level, member.Profile, member.Experience, member.SquadId, member.FormationSlot, member.Assignment, member.Statuses };
    private static string Json<T>(T state) => JsonSerializer.Serialize(state, WireJson.Options);
    private static MatchSnapshot Receive(MatchSnapshot state)
    {
        MatchSnapshot received = Game.SnapshotPayload.DecodeForReceiving(Game.SnapshotPayload.Encode(state));
        Assert.Equal(Json(state), Json(received));
        CombatReservations claims = CombatReservations.Reconstruct(received.Arena!, received.Squads);
        Assert.Equal(received.Reservations.Positions, claims.Positions); Assert.Equal(received.Reservations.Transit, claims.Transit);
        return received;
    }
}
