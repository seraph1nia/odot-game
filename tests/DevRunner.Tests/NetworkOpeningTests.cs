using DevRunner;
using Game.Core;
using Xunit;

namespace DevRunner.Tests;

public sealed class NetworkOpeningTests
{
    private static Match Opening(ulong seed)
    {
        var match = new Match(combatSeed: seed); match.Join(); match.Join();
        void Act(int player, string action, int slot = -1, Building building = Building.Empty,
            ConstructionPayment payment = ConstructionPayment.Standard)
            => Assert.True(match.Apply(player, Command.FromSnapshot(match.Snapshot(), match.Revision + 1,
                action, player, slot, building, payment: payment)).Accepted);
        Act(1, "start");
        foreach (int player in new[] { 1, 2 })
        {
            Act(player, "build", 0, Building.Farm);
            Act(player, "build", 1, Building.Barracks);
            Act(player, "build", 2, Building.MetalMine);
        }
        foreach (int player in new[] { 1, 2 })
        {
            Act(player, "build", 3, Building.Lumbermill, ConstructionPayment.GoldRecovery);
            Act(player, "ready");
        }
        foreach (int player in new[] { 1, 2 })
        {
            Act(player, "recruit", 1); Act(player, "recruit", 1); Act(player, "ready");
        }
        foreach (int player in new[] { 1, 2 })
        {
            Act(player, "build", 4, Building.Stonecutter); Act(player, "buy-plot", 5);
            Act(player, "recruit", 1); Act(player, "recruit", 1); Act(player, "ready");
        }
        foreach (int player in new[] { 1, 2 })
        {
            Act(player, "recruit", 1); Act(player, "recruit", 1); Act(player, "ready");
        }
        Assert.Equal(Phase.Combat, match.Phase);
        for (int ticks = 0; ticks < 3000 && match.Phase == Phase.Combat; ticks++) match.Step();
        return match;
    }

    [Fact]
    public void OwnedNetworkSeedKeepsBothCitiesAliveForThePaidRangedWitness()
    {
        using Match match = Opening(Runner.AuthorityResumeVictorySeed);
        Assert.Equal(Phase.Building, match.Phase); Assert.Equal(2, match.Wave);
        Assert.All(match.Players.Values, city => Assert.False(city.Eliminated));
        MatchSnapshot before = match.Snapshot();
        Command purchase = Command.FromSnapshot(before, 100, "buy-plot", 2, 7);
        Assert.True(match.Apply(2, purchase).Accepted);
        Assert.True(match.Apply(2, Command.FromSnapshot(match.Snapshot(), 101, "build", 2, 7, Building.ArcheryRange)).Accepted);
        CityState city = match.Snapshot().Players.Single(p => p.Id == 2);
        Assert.Equal(Building.ArcheryRange, city.Slots[7].Type);
        ResourceCost plot = new(before.PlotPrices[before.Players[1].Slots.Count(s => s.Purchased) - 5]);
        ResourceCost building = before.BuildingCatalog.Single(b => b.Type == Building.ArcheryRange).Construction;
        Assert.True(before.Players[1].Resources.TryPay(plot, out ResourceCost afterPlot));
        Assert.True(afterPlot.TryPay(building, out ResourceCost afterBuilding));
        Assert.Equal(afterBuilding, city.Resources);
    }

    [Fact]
    public void ExactPurchaseMeasurementWaitsForOrdinaryPostClearHomeRestoration()
    {
        using Match match = Opening(Runner.AuthorityResumeVictorySeed);
        MatchSnapshot clear = match.Snapshot();
        Assert.NotEmpty(clear.DyingBodies);
        for (int tick = 0; tick < 120 && match.Snapshot().DyingBodies.Length > 0; tick++) match.Step();
        MatchSnapshot settled = match.Snapshot();
        Assert.Empty(settled.DyingBodies);
        Assert.Equal(clear.Phase, settled.Phase); Assert.Equal(clear.TurnSerial, settled.TurnSerial);
        Assert.All(settled.Players, city =>
        {
            CityState original = clear.Players.Single(p => p.Id == city.Id);
            Assert.Equal(original.Resources, city.Resources);
            Assert.Equal(original.Soldiers.Select(u => (u.Id, u.Health, u.Assignment)), city.Soldiers.Select(u => (u.Id, u.Health, u.Assignment)));
            Assert.All(city.Soldiers, unit => Assert.Equal(unit.Assignment!.Position, unit.Hex!.Position));
        });
        Assert.NotEqual(clear.Players[1].Soldiers[0].Hex!.Position, settled.Players[1].Soldiers[0].Hex!.Position);
        Assert.True(match.Apply(2, Command.FromSnapshot(settled, 100, "buy-home", 2)).Accepted);
        MatchSnapshot accepted = match.Snapshot();
        for (int tick = 0; tick < 120; tick++) match.Step();
        Assert.True(match.Apply(2, Command.FromSnapshot(match.Snapshot(), 101, "pause", 2)).Accepted);
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(accepted.Players[1], WireJson.Options),
            System.Text.Json.JsonSerializer.Serialize(match.Snapshot().Players[1], WireJson.Options));
    }

    [Fact]
    public void TeamClearDoesNotMakeAnEliminatedCityEligibleToPurchase()
    {
        // Counterexample to the former random-seed fixture's team-only clear check.
        using Match match = Opening(96);
        Assert.Equal(Phase.Building, match.Phase); Assert.Equal(2, match.Wave);
        Assert.False(match.Players[1].Eliminated); Assert.True(match.Players[2].Eliminated);
        MatchSnapshot before = match.Snapshot();
        Command purchase = Command.FromSnapshot(before, 100, "buy-plot", 2, 7);
        CommandResult refused = match.Apply(2, purchase);
        Assert.False(refused.Accepted);
        Assert.Equal("Only living cities can act during building.", refused.Message);
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(before, WireJson.Options),
            System.Text.Json.JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
    }
}
