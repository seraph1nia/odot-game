using System.Text.Json;
using DevRunner;
using Game.Core;
using Xunit;

namespace DevRunner.Tests;

// Tier A process-policy inputs: ordinary paid opening, stable post-clear
// bookkeeping and a cheap fallen-owner negative. No socket/render/balance claim.
public sealed class NetworkOpeningTests
{
    private static Match Opening()
    {
        var match = new Match(combatSeed: Runner.AuthorityResumeVictorySeed);
        try
        {
            match.Join(); match.Join();
            void Act(int player, string action, int slot = -1, Building building = Building.Empty,
                ConstructionPayment payment = ConstructionPayment.Standard)
                => Assert.True(match.Apply(player, Command.FromSnapshot(match.Snapshot(), match.Revision + 1,
                    action, player, slot, building, payment: payment)).Accepted);
            Act(1, "start");
            for (int preparation = 0; preparation < 3; preparation++)
            {
                foreach (var (player, action) in CampaignStrategy.RedistributionPreparation(match.Snapshot(), 1, 2))
                    Act(player, action.Action, action.Slot, action.Building, action.Payment);
                foreach (int player in new[] { 1, 2 }) Act(player, "ready");
            }
            Assert.Equal(Phase.Combat, match.Phase); Assert.Equal(3, match.ProductionCount);
            Assert.All(match.Players.Values, city =>
            {
                Assert.Equal(6, city.Soldiers.Count); Assert.Equal(6, city.LastUpkeep!.Paid);
                Assert.Equal(9, city.Food); Assert.Equal(12, city.Metal); Assert.Equal(0, city.Stone);
            });
            for (int ticks = 0; ticks < 3000 && match.Phase == Phase.Combat; ticks++) match.Step();
            Assert.Equal(Phase.Building, match.Phase); Assert.Equal(2, match.Wave);
            Assert.All(match.Players.Values, city => Assert.False(city.Eliminated));
            return match;
        }
        catch { match.Dispose(); throw; }
    }
    [Fact]
    public void EarnedOpeningPaysExactNextPlotAndRangedTrainerWithoutHiddenStock()
    {
        using Match match = Opening(); MatchSnapshot before = match.Snapshot();
        Assert.True(match.Apply(2, Command.FromSnapshot(before, 100, "buy-plot", 2, 7)).Accepted);
        Assert.True(match.Apply(2, Command.FromSnapshot(match.Snapshot(), 101, "build", 2, 7, Building.ArcheryRange)).Accepted);
        CityState city = match.Snapshot().Players[1]; Assert.Equal(Building.ArcheryRange, city.Slots[7].Type);
        ResourceCost plot = new(before.PlotPrices[before.Players[1].Slots.Count(s => s.Purchased) - 5]);
        ResourceCost building = before.BuildingCatalog.Single(b => b.Type == Building.ArcheryRange).Construction;
        Assert.True(before.Players[1].Resources.TryPay(plot, out ResourceCost afterPlot));
        Assert.True(afterPlot.TryPay(building, out ResourceCost afterBuilding)); Assert.Equal(afterBuilding, city.Resources);
    }
    [Fact]
    public void CosmeticCleanupRestoresPhysicalSquadHomesBeforeExactRetryMeasurement()
    {
        using Match match = Opening(); MatchSnapshot clear = match.Snapshot(); Assert.NotEmpty(clear.DyingBodies);
        for (int tick = 0; tick < 120 && match.Snapshot().DyingBodies.Length > 0; tick++) match.Step();
        MatchSnapshot settled = match.Snapshot(); Assert.Empty(settled.DyingBodies);
        Assert.Equal(clear.Phase, settled.Phase); Assert.Equal(clear.TurnSerial, settled.TurnSerial);
        var arena = new SharedArena(settled.Players.Select(c => c.Id));
        foreach (CityState city in settled.Players)
        {
            CityState original = clear.Players.Single(p => p.Id == city.Id);
            Assert.Equal(original.Resources, city.Resources);
            Assert.Equal(original.Soldiers.Select(u => (u.Id, u.Health, u.Assignment, u.SquadId, u.FormationSlot, u.Experience)),
                city.Soldiers.Select(u => (u.Id, u.Health, u.Assignment, u.SquadId, u.FormationSlot, u.Experience)));
            Assert.All(city.Soldiers, unit => Assert.Equal(new HexPosition(arena.Deployment(city.Id, unit.Assignment!.Tile), 1), unit.Hex!.Position));
        }
        Assert.NotEqual(clear.Players[1].Soldiers[0].Hex!.Position, settled.Players[1].Soldiers[0].Hex!.Position);
        Assert.True(match.Apply(2, Command.FromSnapshot(settled, 100, "buy-home", 2)).Accepted);
        MatchSnapshot accepted = match.Snapshot();
        for (int tick = 0; tick < 120; tick++) match.Step();
        Assert.True(match.Apply(2, Command.FromSnapshot(match.Snapshot(), 101, "pause", 2)).Accepted);
        Assert.Equal(JsonSerializer.Serialize(accepted.Players[1], WireJson.Options), JsonSerializer.Serialize(match.Snapshot().Players[1], WireJson.Options));
    }
    [Fact]
    public void PolicyAndAuthorityRefuseFallenOwnerWithoutMutatingItsPaidState()
    {
        // No second fragile random-seed journey to engineer an isolated-city
        // death. Keep loss is explicit; core combat tests own how a Keep falls.
        using var match = new Match(combatSeed: 1); match.Join(); match.Join();
        Assert.True(match.Apply(1, Command.FromSnapshot(match.Snapshot(), 1, "start", 1)).Accepted);
        Assert.True(match.Apply(2, Command.FromSnapshot(match.Snapshot(), 2, "build", 2, 0, Building.Farm)).Accepted);
        match.Players[2].Health = 0;
        MatchSnapshot before = match.Snapshot();
        Assert.Null(CampaignStrategy.Next(before, 2));
        CommandResult refused = match.Apply(2, Command.FromSnapshot(before, 100, "buy-plot", 2, 7));
        Assert.False(refused.Accepted); Assert.Equal("Only living cities can act during building.", refused.Message);
        Assert.Equal(JsonSerializer.Serialize(before, WireJson.Options), JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
    }
}
