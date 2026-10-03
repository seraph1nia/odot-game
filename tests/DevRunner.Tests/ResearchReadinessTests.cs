using DevRunner;
using Game.Core;
using Xunit;

namespace DevRunner.Tests;

public sealed class ResearchReadinessTests
{
    private static CommandResult Act(Match match, int player, string action, int slot = -1, Building building = Building.Empty)
        => match.Apply(player, new(match.Revision + 1, match.Id, match.Phase, match.TurnSerial, action, player, slot, building,
            ExpectedGeneration: slot is >= 0 and < 9 ? match.Players[player].Slots[slot].Generation : 0));
    // Ordinary paid two-city opening, no combat state setters or grants.
    private static Match AfterClear(bool finishCleanup)
    {
        var match = new Match(combatSeed: 17837719233072821886); match.Join(); match.Join();
        Assert.True(Act(match, 1, "start").Accepted);
        foreach (int player in new[] { 1, 2 })
        {
            Assert.True(Act(match, player, "build", 0, Building.Farm).Accepted);
            Assert.True(Act(match, player, "build", 1, Building.Barracks).Accepted);
            Assert.True(Act(match, player, "build", 2, Building.MetalMine).Accepted);
        }
        for (int turn = 0; turn < 3; turn++) foreach (int player in new[] { 1, 2 }) Assert.True(Act(match, player, "ready").Accepted);
        foreach (int player in new[] { 1, 2 }) for (int recruit = 0; recruit < 6; recruit++) Assert.True(Act(match, player, "recruit", 1).Accepted);
        foreach (int player in new[] { 1, 2 }) Assert.True(Act(match, player, "ready").Accepted);
        for (int step = 0; step < 3600 && match.Phase == Phase.Combat; step++) match.Step();
        Assert.Equal(Phase.Building, match.Phase); Assert.Equal(2, match.Wave);
        Assert.NotEmpty(match.Snapshot().DyingBodies);
        if (finishCleanup) for (int step = 0; step < 120 && match.Snapshot().DyingBodies.Length > 0; step++) match.Step();
        for (int turn = 0; turn < 3; turn++) foreach (int player in new[] { 1, 2 }) Assert.True(Act(match, player, "ready").Accepted);
        Assert.Equal(Phase.Preparation, match.Phase);
        return match;
    }
    [Fact]
    public void AcceptedPreparationIsNotAResolvedStageWhileBodiesRemain()
    {
        using Match match = AfterClear(false); MatchSnapshot started = match.Snapshot();
        foreach (int player in new[] { 1, 2 }) Assert.True(Act(match, player, "ready").Accepted);
        MatchSnapshot accepted = match.Snapshot();
        Assert.Equal(Phase.Preparation, accepted.Phase); Assert.Equal(started.TurnSerial, accepted.TurnSerial);
        Assert.All(accepted.Players, p => Assert.True(p.Ready)); Assert.NotEmpty(accepted.DyingBodies);
        Assert.False(ResearchReadyBarrier.Resolved(started, accepted, accepted));
        // Revision changes and repeated accepted Ready requests cannot resolve
        // the stage. No upkeep is paid until the retained bodies actually expire.
        Assert.True(Act(match, 1, "ready").Accepted);
        MatchSnapshot repeated = match.Snapshot();
        Assert.False(ResearchReadyBarrier.Resolved(started, accepted, repeated));
        Assert.Equal(started.Players.Select(p => p.Food), repeated.Players.Select(p => p.Food));
        Assert.All(repeated.Players, p => Assert.Equal(1, p.LastUpkeep!.Wave));
        long deadline = repeated.DyingBodies.Max(u => u.Hex!.DeathEndTick);
        while (match.Tick < deadline)
        {
            match.Step();
            if (match.Tick < deadline) Assert.False(ResearchReadyBarrier.Resolved(started, accepted, match.Snapshot()));
        }
        MatchSnapshot resolved = match.Snapshot(); Assert.Equal(Phase.Combat, resolved.Phase);
        Assert.Empty(resolved.DyingBodies); Assert.True(ResearchReadyBarrier.Resolved(started, accepted, resolved));
        Assert.Equal(started.TurnSerial + 1, resolved.TurnSerial);
        Assert.All(resolved.Players, p =>
        {
            Assert.False(p.Ready); Assert.Equal(2, p.LastUpkeep!.Wave);
            CityState before = started.Players.Single(c => c.Id == p.Id);
            Assert.NotNull(before.FoodForecast);
            Assert.Equal(before.FoodForecast.Paid, p.LastUpkeep.Paid);
            Assert.Equal(before.Food - p.LastUpkeep.Paid, p.Food);
        });
        // The obsolete driver sends a fresh Ready after automatic entry and
        // receives the same error as the network trace, despite living cities.
        CommandResult raced = Act(match, 1, "ready");
        Assert.False(raced.Accepted); Assert.Equal("Only living cities can act during building.", raced.Message);
        Assert.All(resolved.Players, p => Assert.False(p.Eliminated));
    }
    [Fact]
    public void ImmediateResolutionAndPeerRevisionGuardsRemainValid()
    {
        using Match match = AfterClear(true); MatchSnapshot started = match.Snapshot();
        Assert.Empty(started.DyingBodies);
        Assert.True(Act(match, 1, "ready").Accepted); Assert.True(Act(match, 2, "ready").Accepted);
        MatchSnapshot accepted = match.Snapshot(); Assert.Equal(Phase.Combat, accepted.Phase);
        Assert.True(ResearchReadyBarrier.Resolved(started, accepted, accepted));
        Assert.False(ResearchReadyBarrier.Resolved(started, accepted, accepted with { Revision = accepted.Revision - 1 }));
        Assert.False(ResearchReadyBarrier.Resolved(started, accepted, accepted with { MatchId = "different" }));
        Assert.False(ResearchReadyBarrier.Resolved(started, accepted with { MatchId = "different" }, accepted));
        Assert.False(ResearchReadyBarrier.Resolved(started, accepted, accepted with { TurnSerial = started.TurnSerial }));
    }
}
