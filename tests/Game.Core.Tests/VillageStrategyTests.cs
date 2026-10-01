using System.Text.Json;
using Game.Core;
using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

public sealed class VillageStrategyTests(ITestOutputHelper output)
{
    internal static CommandResult Act(Match match, int city, string action, int slot = -1, Building building = Building.Empty, UnitType unit = UnitType.Swordsman, UnitClass @class = UnitClass.Melee)
        => match.Apply(city, new(1, match.Id, match.Phase, match.TurnSerial, action, city, slot, building, unit, @class));
    private static Match Start(int count = 1)
    {
        var match = new Match(); for (int n = 0; n < count; n++) match.Join();
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
    public void CatalogConstructionIsAtomicAndLumbermillNeedsNoWood()
    {
        using Match match = Start(); City city = match.Players[1]; city.Wood = 0;
        string before = JsonSerializer.Serialize(match.Snapshot());
        Assert.False(Act(match, 1, "build", 0, Building.Farm).Accepted); Assert.Equal(before, JsonSerializer.Serialize(match.Snapshot()));
        Assert.True(Act(match, 1, "build", 0, Building.Lumbermill).Accepted); Assert.True(Act(match, 1, "ready").Accepted);
        Assert.Equal(5, city.Wood); Assert.Equal(50, city.Gold);
        Assert.Equal(9, match.Snapshot().BuildingCatalog.Length); Assert.Equal(4, match.Snapshot().UnitCatalog.Length);
    }
    [Fact]
    public void ThirdProductionCanBeSpentBeforeBattleAndPreparationHasNoIncome()
    {
        using Match match = Start(); Assert.True(Act(match, 1, "build", 0, Building.Farm).Accepted);
        Assert.True(Act(match, 1, "build", 1, Building.Barracks).Accepted);
        for (int n = 0; n < 3; n++) Assert.True(Act(match, 1, "ready").Accepted);
        Assert.Equal(Phase.Preparation, match.Phase); Assert.Equal(3, match.ProductionCount); Assert.Equal(3, match.Turn);
        Command stale = new(1, match.Id, Phase.Building, match.TurnSerial - 1, "ready", 1);
        Assert.False(match.Apply(1, stale).Accepted);
        Assert.True(Act(match, 1, "recruit", 1).Accepted); CityState before = match.Players[1].Snapshot();
        Assert.True(Act(match, 1, "ready").Accepted); Assert.Equal(Phase.Combat, match.Phase);
        CityState after = match.Players[1].Snapshot(); Assert.Equal(before.Gold, after.Gold); Assert.Equal(before.Food, after.Food); Assert.Equal(before.Wood, after.Wood);
    }
    [Theory]
    [InlineData(UnitType.Swordsman, Building.Barracks)]
    [InlineData(UnitType.Berserker, Building.Barracks)]
    [InlineData(UnitType.Crossbowman, Building.ArcheryRange)]
    [InlineData(UnitType.Mage, Building.Arcanum)]
    public void RecruitmentUsesTheCorrectBuildingAndRanksWithoutHealing(UnitType type, Building building)
    {
        using Match match = Start(); City city = match.Players[1]; city.Gold = 200; city.Food = 100; city.Wood = 100;
        Assert.True(Act(match, 1, "build", 0, building).Accepted); Assert.True(Act(match, 1, "build", 1, Building.Blacksmith).Accepted);
        Assert.True(Act(match, 1, "build", 2, Building.Farm).Accepted);
        Assert.False(Act(match, 1, "recruit", 2, unit: type).Accepted);
        Assert.True(Act(match, 1, "recruit", 0, unit: type).Accepted); UnitState old = city.Soldiers.Single();
        match.Combat.Seed(old with { Health = old.Health - 100 });
        UnitClass @class = Catalogs.Class(type);
        Assert.True(Act(match, 1, "research", 1, @class: @class).Accepted);
        UnitState ranked = city.Soldiers.Single(); Assert.Equal(old.Health - 100, ranked.Health); Assert.Equal(old.Id, ranked.Id);
        Assert.Equal(old.Position, ranked.Position); Assert.Equal(HealthPoints.Ranked(old.Profile.Damage, 1), ranked.Profile.Damage);
        Assert.False(Act(match, 1, "research", 1, @class: @class).Accepted);
        Assert.True(Act(match, 1, "upgrade", 1).Accepted); Assert.True(Act(match, 1, "research", 1, @class: @class).Accepted);
        Assert.False(Act(match, 1, "research", 1, @class: @class).Accepted);
        Assert.True(Act(match, 1, "recruit", 0, unit: type).Accepted);
        Assert.Equal(HealthPoints.Ranked(old.Profile.Health, 2), city.Soldiers[^1].Health);
        using var combat = new CombatSimulation(new()); int skeleton = combat.Create(type, 0, 1, 1, Faction.Skeletons, 2);
        Assert.Equal(city.Soldiers[^1].Profile, combat.Read(skeleton).Profile);
    }
    [Theory]
    [InlineData("frontline", 1)]
    [InlineData("mixed", 1)]
    [InlineData("towers", 1)]
    [InlineData("research", 1)]
    [InlineData("frontline", 2)]
    [InlineData("frontline", 3)]
    [InlineData("frontline", 4)]
    public void OrdinaryStrategiesWinWithinBoundedSteps(string strategy, int players)
    {
        using Match match = Start(players);
        int ticks = 0, recruits = 0, preparations = 0;
        var recruitedRoles = new HashSet<UnitType>();
        var waveTicks = new Dictionary<int, int>();
        while (match.Phase is Phase.Building or Phase.Preparation or Phase.Combat)
        {
            if (match.Phase == Phase.Combat)
            {
                Assert.True(ticks++ < 18000, "Strategy exceeded 300 seconds of fixed steps."); waveTicks[match.Wave] = waveTicks.GetValueOrDefault(match.Wave) + 1; match.Step(); continue;
            }
            foreach (City city in match.Players.Values.Where(c => !c.Eliminated))
            {
                void TryBuild(int slot, Building building) { if (city.Slots[slot].Type == Building.Empty) Act(match, city.Id, "build", slot, building); }
                TryBuild(0, Building.Farm);
                if (strategy == "towers") TryBuild(1, Building.Lumbermill);
                if (strategy == "towers") { TryBuild(2, Building.ArrowTower); TryBuild(3, Building.CatapultTower); TryBuild(4, Building.ArrowTower); }
                else { TryBuild(2, Building.Barracks); }
                Act(match, city.Id, "upgrade", 0);
                if (strategy != "towers") TryBuild(1, Building.Lumbermill);
                TryBuild(5, Building.Farm);
                if (strategy == "mixed") { TryBuild(3, Building.ArcheryRange); TryBuild(4, Building.Arcanum); }
                if (strategy == "research")
                {
                    TryBuild(3, Building.Blacksmith);
                    Act(match, city.Id, "research", 3);
                    if (city.Research.Melee == 1) { Act(match, city.Id, "upgrade", 3); Act(match, city.Id, "research", 3); }
                }
                Act(match, city.Id, "upgrade", 0); TryBuild(5, Building.Farm);
                UnitType[] priority = strategy == "mixed" ? [UnitType.Mage, UnitType.Crossbowman, UnitType.Berserker, UnitType.Swordsman] : [UnitType.Swordsman];
                bool recruited;
                do
                {
                    recruited = false;
                    foreach (UnitType type in priority)
                    {
                        int slot = Array.FindIndex(city.Slots, s => Catalogs.Buildings(match.Rules).SingleOrDefault(b => b.Type == s.Type)?.Recruits?.Contains(type) == true);
                        if (slot >= 0 && Act(match, city.Id, "recruit", slot, unit: type).Accepted) { recruits++; recruitedRoles.Add(type); recruited = true; }
                    }
                } while (recruited);
                if (strategy == "towers") foreach (int slot in new[] { 2, 3, 4 }) Act(match, city.Id, "upgrade", slot);
                output.WriteLine($"{strategy} P{city.Id} wave={match.Wave} turn={match.Turn} phase={match.Phase}: gold={city.Gold} wood={city.Wood} food={city.Food} army={city.Soldiers.Count} HP={HealthPoints.Format(city.Health)}");
            }
            if (match.Phase == Phase.Preparation) preparations++;
            foreach (City city in match.Players.Values.Where(c => c.Connected && !c.Eliminated)) Assert.True(Act(match, city.Id, "ready").Accepted);
        }
        output.WriteLine($"{strategy}/{players}: waveTicks={string.Join(',', waveTicks.OrderBy(p => p.Key).Select(p => p.Value))}; casualties={recruits - match.Players.Values.Sum(c => c.Soldiers.Count)}; {ticks} ticks; recruited={recruits}; surviving={match.Players.Values.Sum(c => c.Soldiers.Count)}; cityHP={string.Join(',', match.Players.Values.Select(c => HealthPoints.Format(c.Health)))}");
        Assert.Equal(Phase.Victory, match.Phase); Assert.Equal(9, match.ProductionCount); Assert.Equal(3, preparations);
        if (strategy == "mixed") Assert.Equal(4, recruitedRoles.Count);
        if (strategy == "research") Assert.True(match.Players[1].Research.Melee >= 1);
        if (strategy == "towers") Assert.Contains(match.Players[1].Towers.Values, t => t.AttackSequence > 0);
    }
}
