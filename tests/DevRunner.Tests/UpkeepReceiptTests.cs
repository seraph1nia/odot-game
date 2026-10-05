using DevRunner;
using Game;
using Game.Core;
using Xunit;

namespace DevRunner.Tests;

public sealed class UpkeepReceiptTests
{
    private static void Act(Match match, int player, string action, int slot = -1, Building building = Building.Empty)
        => Assert.True(match.Apply(player, Command.FromSnapshot(match.Snapshot(), match.Revision + 1, action, player, slot, building)).Accepted);

    private static Match PaidBattle()
    {
        var match = new Match(combatSeed: 1); match.Join(); match.Join(); Act(match, 1, "start");
        foreach (int player in new[] { 1, 2 })
        {
            Act(match, player, "build", 0, Building.Farm);
            Act(match, player, "build", 1, Building.Barracks);
            Act(match, player, "build", 2, Building.MetalMine);
        }
        for (int turn = 0; turn < 3; turn++) foreach (int player in new[] { 1, 2 }) Act(match, player, "ready");
        foreach (int player in new[] { 1, 2 }) { Act(match, player, "recruit", 1); Act(match, player, "ready"); }
        Assert.Equal(Phase.Combat, match.Phase);
        return match;
    }

    private static UiObservation Frame(MatchSnapshot state, int player = 1)
    {
        EconomyView view = ProgressionPresentation.Economy(state, state.Players.Single(p => p.Id == player), true);
        return new()
        {
            Connected = true,
            MatchId = state.MatchId,
            Revision = state.Revision,
            MatchPhase = state.Phase,
            Wave = state.Wave,
            TurnSerial = state.TurnSerial,
            Paused = state.Paused,
            ObservedCity = player,
            CompactUpkeep = [view.UpkeepLabel, view.UpkeepValue, view.BalanceLabel, view.BalanceValue]
        };
    }

    [Fact]
    public void SlowObservationCannotRecoverAnEndedWaveButOrdinaryPauseRetainsActualPayment()
    {
        using Match running = PaidBattle(); MatchSnapshot admitted = running.Snapshot();
        for (int tick = 0; tick < 10000 && running.Phase == Phase.Combat; tick++) running.Step();
        Assert.NotEqual(Phase.Combat, running.Phase);
        Assert.NotEqual("Paid this battle · W1", Frame(running.Snapshot()).CompactUpkeep[0]);
        Assert.False(UiProtocol.PaidUpkeep(Frame(running.Snapshot()), admitted, 1));

        using Match paused = PaidBattle(); MatchSnapshot ready = paused.Snapshot();
        Act(paused, 2, "pause"); MatchSnapshot frozen = paused.Snapshot();
        for (int tick = 0; tick < 10000; tick++) paused.Step();
        MatchSnapshot delayed = paused.Snapshot();
        Assert.Equal(frozen.Revision, delayed.Revision); Assert.Equal(frozen.Tick, delayed.Tick);
        Assert.Equal(ready.Players[0].Resources, delayed.Players[0].Resources);
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(ready.Players[0].LastUpkeep, WireJson.Options),
            System.Text.Json.JsonSerializer.Serialize(delayed.Players[0].LastUpkeep, WireJson.Options));
        Assert.True(UiProtocol.PaidUpkeep(Frame(delayed), frozen, 1));
        Act(paused, 2, "resume"); paused.Step();
        Assert.False(paused.Paused); Assert.True(paused.Tick > frozen.Tick);
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(ready.Players[0].LastUpkeep, WireJson.Options),
            System.Text.Json.JsonSerializer.Serialize(paused.Snapshot().Players[0].LastUpkeep, WireJson.Options));
    }

    [Fact]
    public void PaymentRequiresCurrentFrozenSessionWaveRevisionCityAndAllFourExactValues()
    {
        using Match match = PaidBattle(); Act(match, 2, "pause"); MatchSnapshot battle = match.Snapshot();
        UiObservation frame = Frame(battle);
        Assert.True(UiProtocol.PaidUpkeep(frame, battle, 1));
        UiObservation[] invalid =
        [
            frame with { Connected = false }, frame with { MatchId = "another-session" },
            frame with { Revision = frame.Revision - 1 }, frame with { Revision = frame.Revision + 1 },
            frame with { MatchPhase = Phase.Building }, frame with { Paused = false },
            frame with { Wave = frame.Wave + 1 }, frame with { TurnSerial = frame.TurnSerial + 1 },
            frame with { ObservedCity = 2 }, frame with { CompactUpkeep = [] }
        ];
        Assert.All(invalid, wrong => Assert.False(UiProtocol.PaidUpkeep(wrong, battle, 1)));
        for (int index = 0; index < 4; index++)
        {
            string[] wrong = frame.CompactUpkeep.ToArray(); wrong[index] = "wrong";
            Assert.False(UiProtocol.PaidUpkeep(frame with { CompactUpkeep = wrong }, battle, 1));
        }
        Assert.False(UiProtocol.PaidUpkeep(frame, battle with { Paused = false }, 1));
        Assert.False(UiProtocol.PaidUpkeep(frame, battle with { Phase = Phase.Building }, 1));
        Assert.False(UiProtocol.PaidUpkeep(frame, battle with { Wave = battle.Wave + 1 }, 1));
    }
}
