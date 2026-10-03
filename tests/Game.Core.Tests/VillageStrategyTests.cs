using System.Text.Json;
using Game.Core;
using DevRunner;
using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

public sealed class VillageStrategyTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(109UL)]
    // A reproducible frontage witness, not a balance sample. First-fit paid homes
    // use a Stonecutter-backed L3 receiving opening; seed 109 retains every
    // cleared-frontage, bounded admission, retained identity and survival assertion.
    public void OrdinaryThreeCityProgressionReinforcesAClearedOccupiedForwardBand(ulong seed)
    {
        using Match match = Start(3, seed);
        foreach (int id in new[] { 1, 2 })
        {
            Assert.True(Act(match, id, "build", 0, Building.Farm).Accepted);
            Assert.True(Act(match, id, "build", 1, Building.Barracks).Accepted);
            Assert.True(Act(match, id, "build", 2, Building.MetalMine).Accepted);
        }
        void Invest(int id)
        {
            City city = match.Players[id];
            while (CampaignStrategy.ReinforcementInvestment(match.Snapshot(), id, id == 1) is EconomyAction decision)
                Assert.True(match.Apply(id, decision.Command(match.Snapshot(), id)).Accepted);
            output.WriteLine($"P{id} W{match.Wave} T{match.Turn} army={city.Soldiers.Count} gold={city.Gold} wood={city.Wood}");
        }
        void Prepare(bool firstWave)
        {
            while (match.Phase is Phase.Building or Phase.Preparation)
            {
                foreach (City city in match.Players.Values.Where(c => !c.Eliminated && !c.Ready))
                {
                    if (city.Id == 1 || firstWave && city.Id == 2) Invest(city.Id);
                    else if (city.Id == 2 && match.Phase == Phase.Preparation) Assert.True(Act(match, 2, "recruit", 1).Accepted);

                    Assert.True(Act(match, city.Id, "ready").Accepted);
                }
                if (match.Players.Values.Where(c => !c.Eliminated).All(c => c.Ready)) match.Step();
            }
        }
        Prepare(true);
        while (match.Phase == Phase.Combat) match.Step();
        Assert.Equal(Phase.Building, match.Phase); Assert.Equal(2, match.Wave); Assert.True(match.Players[3].Eliminated);
        Prepare(false); Assert.Equal(12, match.Enemies.Count);
        MatchSnapshot? before = null, transfer = null;
        for (int step = 0; step < 3000 && match.Phase == Phase.Combat && !match.Players[2].Eliminated; step++)
        {
            before = match.Snapshot(); match.Step();
            if (match.Players[2].Eliminated) transfer = match.Snapshot();
        }
        Assert.NotNull(before); Assert.NotNull(transfer);
        output.WriteLine($"transfer {before.Tick}: A soldiers={before.Players[0].Soldiers.Length}, enemies={before.Enemies.Count(u => u.Destination == 1)}");
        Assert.DoesNotContain(before.Enemies, u => u.Destination == 1);
        int[] forward = match.Configuration.Board.Front(Faction.Skeletons).ToArray();
        int[] held = before.Players[0].Soldiers.Where(u => u.Deployed && forward.Contains(u.Hex!.Position.Cell)).Select(u => u.Hex!.Position.Cell).Distinct().Order().ToArray();
        output.WriteLine($"cleared-forward transfer seed={seed} tick={transfer.Tick}; held={string.Join(',', held)}; required={string.Join(',', forward)}");
        Assert.NotEmpty(held);
        AdmissionBound bound = Assert.Single(transfer.Admissions, a => a.City == 1);
        Assert.NotNull(bound.AdmissionTick); Assert.True(bound.AdmissionTick <= bound.FirstAdmissionBound);
        Assert.Contains(transfer.Enemies, u => u.Id == bound.FirstUnitId && u.Deployed && u.Origin != 1);
        while (match.Phase == Phase.Combat) match.Step();
        Assert.Equal(Phase.Building, match.Phase); Assert.Equal(3, match.Wave); Assert.Equal(DefeatReason.None, match.DefeatReason);
    }

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
    public void ThirdProductionCanBeSpentBeforeBattleAndPreparationHasNoIncome()
    {
        using Match match = Start(); Assert.True(Act(match, 1, "build", 0, Building.Farm).Accepted);
        Assert.True(Act(match, 1, "build", 1, Building.Barracks).Accepted);
        Assert.True(Act(match, 1, "build", 2, Building.MetalMine).Accepted);
        for (int n = 0; n < 3; n++) Assert.True(Act(match, 1, "ready").Accepted);
        Assert.Equal(Phase.Preparation, match.Phase); Assert.Equal(3, match.ProductionCount); Assert.Equal(3, match.Turn);
        Command stale = new(1, match.Id, Phase.Building, match.TurnSerial - 1, "ready", 1);
        Assert.False(match.Apply(1, stale).Accepted);
        Assert.True(Act(match, 1, "recruit", 1).Accepted); CityState before = match.Players[1].Snapshot();
        Assert.True(Act(match, 1, "ready").Accepted); Assert.Equal(Phase.Combat, match.Phase);
        CityState after = match.Players[1].Snapshot(); Assert.Equal(before.Gold, after.Gold); Assert.Equal(before.Food - match.Economy.Upkeep(UnitType.Swordsman), after.Food); Assert.Equal(before.Wood, after.Wood);
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
        Assert.Equal(city.Soldiers[^1].Profile, combat.Read(skeleton).Profile);
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
