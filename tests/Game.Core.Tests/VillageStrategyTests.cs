using System.Text.Json;
using Game.Core;
using DevRunner;
using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

public sealed class VillageStrategyTests(ITestOutputHelper output)
{
    // The old city-teleport/admission journey is consolidated into SessionFlowTests:
    // paid three-city rescue, serialized veteran continuity and future allocation.
    // SharedCombatTests owns deterministic cross-sector rescue and source continuity.

    internal static CommandResult Act(Match match, int city, string action, int slot = -1, Building building = Building.Empty, UnitType unit = UnitType.Swordsman, UnitClass @class = UnitClass.Melee)
        => match.Apply(city, new(1, match.Id, match.Phase, match.TurnSerial, action, city, slot, building, unit, ExpectedGeneration: slot is >= 0 and < 9 ? match.Players[city].Slots[slot].Generation : 0));
    internal static Match Start(int count = 1, ulong seed = 123)
    {
        var match = new Match(combatSeed: seed); for (int n = 0; n < count; n++) match.Join();
        Assert.True(Act(match, 1, "start").Accepted); return match;
    }
    [Fact]
    public void FixedPointRanksFormattingAndOverflowAreExplicit()
    {
        Assert.Equal(105, HealthPoints.Ranked(100, 1)); Assert.Equal(110, HealthPoints.Ranked(100, 2));
        Assert.Equal("1.05", HealthPoints.Format(105)); Assert.Equal("10", HealthPoints.Format(1000));
        Assert.Throws<OverflowException>(() => HealthPoints.FromWhole(int.MaxValue));
        Assert.Throws<OverflowException>(() => HealthPoints.Ranked(int.MaxValue, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => HealthPoints.Ranked(100, 3));
        Assert.Throws<ArgumentException>(() => new Match(new Rules { StartingWood = -1 }));
    }
    [Fact]
    public void CatalogConstructionIsAtomicAndLumbermillRecoveryIsExplicit()
    {
        using Match match = Start(); City city = match.Players[1]; city.Wood = 0;
        string before = JsonSerializer.Serialize(match.Snapshot());
        Assert.False(Act(match, 1, "build", 0, Building.Farm).Accepted); Assert.Equal(before, JsonSerializer.Serialize(match.Snapshot()));
        Assert.False(Act(match, 1, "build", 0, Building.Lumbermill).Accepted);
        Assert.True(match.Apply(1, new(1, match.Id, match.Phase, match.TurnSerial, "build", 1, 0, Building.Lumbermill, Payment: ConstructionPayment.GoldRecovery)).Accepted); Assert.True(Act(match, 1, "ready").Accepted);
        Assert.Equal(1, city.Wood); Assert.Equal(10, city.Gold);
        Assert.Equal(14, match.Snapshot().BuildingCatalog.Length); Assert.Equal(4, match.Snapshot().UnitCatalog.Length);
    }
    [Fact]
    public void ThirdEndSettlesIncomeAndFoodIntoCombatWithoutPrespendingOrFourthInvestment()
    {
        using Match match = Start(); Assert.True(Act(match, 1, "build", 0, Building.Farm).Accepted);
        Assert.True(Act(match, 1, "build", 1, Building.Barracks).Accepted);
        Assert.True(Act(match, 1, "build", 2, Building.MetalMine).Accepted);
        for (int n = 0; n < 2; n++) Assert.True(Act(match, 1, "ready").Accepted);
        Assert.Equal(Phase.Preparation, match.Phase); Assert.Equal(2, match.ProductionCount); Assert.Equal(3, match.Turn);
        for (int n = 0; n < 4; n++) Assert.True(Act(match, 1, "recruit", 1).Accepted);
        Assert.Equal(0, match.Players[1].Metal);
        string unchanged = JsonSerializer.Serialize(match.Snapshot());
        Assert.False(Act(match, 1, "recruit", 1).Accepted); // No advance on the third END's future metal.
        Assert.Equal(unchanged, JsonSerializer.Serialize(match.Snapshot()));
        MatchSnapshot preparation = match.Snapshot(); CityState before = preparation.Players.Single();
        Assert.True(Act(match, 1, "ready").Accepted);
        Assert.Equal(Phase.Combat, match.Phase); Assert.Equal(3, match.ProductionCount);
        Assert.Equal(preparation.TurnSerial + 1, match.TurnSerial);
        CityState after = match.Players[1].Snapshot();
        ResourceCost income = Assert.IsType<ResourceCost>(before.ProductionIncome);
        Assert.True(before.Resources.TryAdd(income, out ResourceCost funded));
        Assert.True(funded.TryPay(new(Food: after.LastUpkeep!.Paid), out ResourceCost settled));
        Assert.Equal(settled, after.Resources); Assert.Equal(4, after.Metal); Assert.Equal(4, after.LastUpkeep.Paid);
        Assert.Equal(before.Soldiers.Select(u => u.Id), after.LastUpkeep.Funded);
        Assert.Equal(before.Soldiers.Select(u => (u.Id, u.Health, u.Profile)), after.Soldiers.Select(u => (u.Id, u.Health, u.Profile)));
        unchanged = JsonSerializer.Serialize(match.Snapshot());
        Assert.False(match.Apply(1, Command.FromSnapshot(preparation, 1, "ready", 1)).Accepted);
        Assert.False(Act(match, 1, "recruit", 1).Accepted); Assert.False(Act(match, 1, "ready").Accepted);
        Assert.Equal(unchanged, JsonSerializer.Serialize(match.Snapshot())); // No fourth spending/settlement gate.
    }
    [Theory]
    [InlineData(UnitType.Swordsman, Building.Barracks)]
    [InlineData(UnitType.Berserker, Building.Barracks)]
    [InlineData(UnitType.Crossbowman, Building.ArcheryRange)]
    [InlineData(UnitType.Mage, Building.Arcanum)]
    public void RecruitmentUsesTheCorrectBuildingAndTechnologyWithoutHealing(UnitType type, Building building)
    {
        using Match match = Start(); City city = match.Players[1]; city.Gold = 200; city.Food = 100; city.Wood = 100; city.Stone = 100; city.Metal = 100; city.Cloth = 100;
        Assert.True(Act(match, 1, "build", 0, building).Accepted); Assert.True(Act(match, 1, "build", 1, Building.ResearchTower).Accepted);
        Assert.True(Act(match, 1, "build", 2, Building.Farm).Accepted);
        Assert.False(Act(match, 1, "recruit", 2, unit: type).Accepted);
        Assert.True(Act(match, 1, "recruit", 0, unit: type).Accepted); UnitState old = city.Soldiers.Single();
        match.Combat.Seed(old with { Health = old.Health - 100 });
        TechnologyId foundation = Catalogs.Class(type) switch { UnitClass.Melee => TechnologyId.MeleeFoundation, UnitClass.Ranged => TechnologyId.RangedFoundation, _ => TechnologyId.MagicFoundation };
        city.Research = new(Points: 3);
        var purchase = new Command(1, match.Id, match.Phase, match.TurnSerial, "research-tech", 1, Technology: foundation);
        Assert.True(match.Apply(1, purchase).Accepted);
        UnitState researched = city.Soldiers.Single(); Assert.Equal(old.Health - 100, researched.Health); Assert.Equal(old.Id, researched.Id);
        Assert.Equal(old.Hex, researched.Hex); Assert.Equal(HealthPoints.Ranked(old.Profile.Damage, 1), researched.Profile.Damage);
        Assert.False(match.Apply(1, purchase).Accepted);
        Assert.True(Act(match, 1, "upgrade", 1).Accepted); Assert.False(match.Apply(1, purchase).Accepted);
        Assert.True(Act(match, 1, "recruit", 0, unit: type).Accepted);
        Assert.Equal(HealthPoints.Ranked(old.Profile.Health, 1), city.Soldiers[^1].Health);
        using var combat = new CombatSimulation(new()); int skeleton = combat.Create(type, 0, 1, 1, Faction.Skeletons, capabilities: researched.Capabilities);
        Assert.Equal(WavePowerBaseline.Reduced(city.Soldiers[^1].Profile), combat.Read(skeleton).Profile);
    }
    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    [InlineData(123UL)]
    public void NoInvestmentLosesThroughCityDamageInsteadOfTheStallFallback(ulong seed)
    {
        using Match match = Start(seed: seed);
        while (match.Phase is not Phase.Victory and not Phase.Defeat && match.Tick < match.Configuration.MaximumWaveTicks)
        {
            if (match.Phase is Phase.Building or Phase.Preparation) Assert.True(Act(match, 1, "ready").Accepted);
            else match.Step();
        }
        Assert.Equal(Phase.Defeat, match.Phase); Assert.Equal(DefeatReason.AllCitiesFallen, match.DefeatReason);
        Assert.Null(match.Stall); Assert.Equal(0, match.Players[1].Health); Assert.True(match.Players[1].Eliminated);
        output.WriteLine($"No investment: seed={seed}, config={match.Configuration.Fingerprint}, wave={match.Wave}, tick={match.Tick}, cityHP={match.Players[1].Health}.");
    }
}
