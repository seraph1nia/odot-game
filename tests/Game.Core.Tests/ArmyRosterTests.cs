using System.Text.Json;
using DevRunner;
using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

public sealed class ArmyRosterTests(ITestOutputHelper output)
{
    private static Match Start()
    {
        var match = new Match(combatSeed: 1); match.Join();
        Assert.True(Act(match, "start").Accepted);
        City city = match.Players[1]; city.Gold = 100; city.Wood = 100; city.Stone = 100; city.Metal = 100; city.Food = 100;
        Assert.True(Act(match, "build", 0, Building.Barracks).Accepted);
        return match;
    }
    private static CommandResult Act(Match match, string action, int slot = -1, Building building = Building.Empty, int unit = 0)
        => match.Apply(1, new(1, match.Id, match.Phase, match.TurnSerial, action, 1, slot, building,
            ExpectedGeneration: slot >= 0 ? match.Players[1].Slots[slot].Generation : 0,
            UnitId: unit, ExpectedHomeCount: match.Players[1].PurchasedHomes,
            ExpectedTrackLevel: slot >= 0 ? action == "upgrade-capacity" ? match.Players[1].Slots[slot].CapacityLevel : action == "upgrade-healing" ? match.Players[1].Slots[slot].HealingLevel : -1 : -1));
    private static string State(Match match) => JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);

    [Fact]
    public void PaidRecruitmentUsesFirstFitAndRejectsFullWithoutPayment()
    {
        using Match match = Start(); City city = match.Players[1];
        for (int i = 0; i < 6; i++) Assert.True(Act(match, "recruit", 0).Accepted);
        UnitState[] units = city.Soldiers.ToArray();
        Assert.Equal(2, units.Select(u => u.Assignment!.Tile).Distinct().Count());
        Assert.Equal(match.Army.HomeCells[0], units[0].Assignment!.Tile);
        Assert.All(units.Take(3), u => Assert.Equal(units[0].Assignment!.Tile, u.Assignment!.Tile));
        Assert.Equal(6, units.Select(u => (u.Assignment!.Tile, u.Assignment.Anchor)).Distinct().Count());
        string before = State(match);
        Assert.False(Act(match, "recruit", 0).Accepted); Assert.Equal(before, State(match));
        int gold = city.Gold; Assert.True(Act(match, "buy-home").Accepted);
        Assert.Equal(gold - 5, city.Gold); Assert.Equal(3, city.PurchasedHomes);
        Assert.True(Act(match, "recruit", 0).Accepted); Assert.Equal(match.Army.HomeCells[2], city.Soldiers[^1].Assignment!.Tile);
        Assert.Equal(units, city.Soldiers.Take(6));
    }

    [Fact]
    public void RetireAndHallTransfersPreserveIdentityWoundsAndRejectFullDestinations()
    {
        using Match match = Start(); City city = match.Players[1];
        Assert.True(Act(match, "build", 1, Building.TownHall).Accepted);
        for (int i = 0; i < 6; i++) Assert.True(Act(match, "recruit", 0).Accepted);
        UnitState veteran = city.Soldiers[0]; match.Combat.Seed(veteran with { Health = 3101 });
        Assert.True(Act(match, "store", 1, unit: veteran.Id).Accepted);
        UnitState stored = city.Soldiers.Single(u => u.Id == veteran.Id);
        Assert.True(stored.Assignment!.Stored); Assert.False(stored.Deployed); Assert.Equal(3101, stored.Health);
        Assert.True(Act(match, "recruit", 0).Accepted);
        string full = State(match); Assert.False(Act(match, "send", 1, unit: veteran.Id).Accepted); Assert.Equal(full, State(match));
        for (int i = 0; i < 2; i++) Assert.True(Act(match, "store", 1, unit: city.Soldiers.First(u => !u.Assignment!.Stored).Id).Accepted);
        full = State(match); Assert.False(Act(match, "store", 1, unit: city.Soldiers.First(u => !u.Assignment!.Stored).Id).Accepted); Assert.Equal(full, State(match));
        Assert.False(Act(match, "sell", 1).Accepted); Assert.Equal(full, State(match));
        ResourceCost resources = city.Resources;
        int retire = city.Soldiers.First(u => !u.Assignment!.Stored).Id;
        Assert.True(Act(match, "retire", unit: retire).Accepted); Assert.Equal(resources, city.Resources);
        Assert.DoesNotContain(city.Soldiers, u => u.Id == retire);
        Assert.True(Act(match, "send", 1, unit: veteran.Id).Accepted);
        UnitState returned = city.Soldiers.Single(u => u.Id == veteran.Id);
        Assert.False(returned.Assignment!.Stored); Assert.Equal(3101, returned.Health); Assert.Equal(veteran.Level, returned.Level);
    }

    [Fact]
    public void IndependentHallTracksHaveNoImmediateHealingAndKeepInvestment()
    {
        using Match match = Start(); City city = match.Players[1];
        Assert.True(Act(match, "build", 1, Building.TownHall).Accepted);
        Assert.True(Act(match, "upgrade-healing", 1).Accepted);
        Assert.Equal(1, city.Slots[1].CapacityLevel); Assert.Equal(2, city.Slots[1].HealingLevel);
        string upgraded = State(match);
        Assert.False(match.Apply(1, new(1, match.Id, match.Phase, match.TurnSerial, "upgrade-healing", 1, 1,
            ExpectedGeneration: city.Slots[1].Generation, ExpectedTrackLevel: 1)).Accepted); Assert.Equal(upgraded, State(match));
        Assert.True(Act(match, "upgrade-capacity", 1).Accepted);
        Assert.Equal(2, city.Slots[1].CapacityLevel); Assert.Equal(2, city.Slots[1].HealingLevel);
        Assert.Equal(new ResourceCost(19, 6, Stone: 6), city.Slots[1].Investment);
        Assert.False(Act(match, "upgrade", 1).Accepted);
        Assert.Equal(2, city.PurchasedHomes);
        Assert.True(Act(match, "upgrade-capacity", 1).Accepted); Assert.True(Act(match, "upgrade-healing", 1).Accepted);
        Assert.Equal(3, city.Slots[1].CapacityLevel); Assert.Equal(3, city.Slots[1].HealingLevel);
        Assert.Null(city.Snapshot().Army!.Halls.Single().CapacityQuote); Assert.Null(city.Snapshot().Army!.Halls.Single().HealingQuote);
        string complete = State(match); Assert.False(Act(match, "upgrade-capacity", 1).Accepted); Assert.Equal(complete, State(match));
        ResourceCost refund = city.Snapshot().Slots[1].Refund; ResourceCost before = city.Resources;
        Assert.True(Act(match, "sell", 1).Accepted); Assert.True(before.TryAdd(refund, out ResourceCost returned)); Assert.Equal(returned, city.Resources);
        Assert.Equal(2, city.PurchasedHomes);
    }

    [Fact]
    public void OrdinaryPaidSixUnitOpeningClearsAndCanAffordItsFirstHome()
    {
        using var match = new Match(combatSeed: 1); City city = match.Join()!; Assert.True(Act(match, "start").Accepted);
        while (match.Phase is Phase.Building or Phase.Preparation)
        {
            for (int actions = 0; actions < 100 && CampaignStrategy.Next(match.Snapshot(), 1) is EconomyAction plan; actions++)
                Assert.True(match.Apply(1, plan.Command(match.Snapshot(), 1)).Accepted);
            Assert.True(Act(match, "ready").Accepted);
        }
        Assert.Equal(6, city.Soldiers.Count); Assert.Equal(2, city.PurchasedHomes); Assert.Equal(6, city.LastUpkeep!.Paid);
        while (match.Phase == Phase.Combat || match.Combat.HasDeaths) match.Step();
        Assert.Equal(Phase.Building, match.Phase); Assert.Equal(2, match.Wave); Assert.False(city.Eliminated);
        output.WriteLine($"Paid opening: survivors={city.Soldiers.Count}, gold={city.Gold}, wood={city.Wood}, stone={city.Stone}, food={city.Food}");
        Assert.True(Act(match, "buy-home").Accepted); Assert.Equal(3, city.PurchasedHomes);
    }

    [Fact]
    public void FrozenHomeQuotesAndSnapshotArraysCannotMutateTheAuthority()
    {
        var settings = new ArmySettings(); using var match = new Match(new Rules { Army = settings }); match.Join();
        CombatFingerprint fingerprint = match.ConfigurationFingerprint;
        settings.HomePrices[0] = 999;
        MatchSnapshot snapshot = match.Snapshot(); snapshot.Rules.Army.HomePrices[0] = 888; snapshot.Players[0].Army!.HomePrices[0] = 777;
        snapshot.Players[0].Army!.Homes[0] = new(999, true, 6);
        Assert.Equal(5, match.Army.HomeQuote(2)!.Value.Gold); Assert.Equal(5, match.Snapshot().Rules.Army.HomePrices[0]);
        Assert.Equal(5, match.Snapshot().Players[0].Army!.HomePrices[0]); Assert.Equal(fingerprint, match.ConfigurationFingerprint);
        Assert.Equal(0, match.Snapshot().Players[0].Army!.Homes[0].Used);
    }

    [Fact]
    public void FirstFitDoesNotPoolSizeAcrossHomesOrRepackSurvivors()
    {
        var config = new ArmyConfiguration(new(), new HexBoard(HexBoardDefinition.Default()));
        UnitState Unit(int id, int size, int tile) => new(id, 100)
        { Faction = Faction.Adventurers, Profile = new() { Size = size }, Assignment = new(-1, 0, tile, id) };
        UnitState[] existing = [Unit(1, 4, config.HomeCells[0]), Unit(2, 4, config.HomeCells[1])];
        Assert.Null(config.Find(existing, 2, 3));
        ArmyAssignment? fitting = config.Find(existing, 2, 2);
        Assert.NotNull(fitting); Assert.Equal(config.HomeCells[0], fitting.Tile);
        Assert.Equal(config.HomeCells[1], existing[1].Assignment!.Tile);
    }
}
