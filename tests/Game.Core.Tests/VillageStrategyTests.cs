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
    // Tier A earned progression, not incidental seeded spatial setup. Default
    // rules and real paid commands own identity, resources and transfer. The
    // genuine occupied-source invariant is independently established by
    // HexBattleTests.RedistributionIntoOccupiedClearedFrontageAdmitsEngagesAndCompletes.
    public void PaidThreeCityProgressionPreservesClearedTransferIdentityAndCompletes(ulong seed)
    {
        using Match match = Start(3, seed);
        var born = new Dictionary<int, (int Owner, UnitType Type, int Level)>();
        var payments = new List<object>();
        void Paid(int id, Command command)
        {
            MatchSnapshot state = match.Snapshot();
            CityState before = state.Players.Single(p => p.Id == id);
            ResourceCost? quoted = command.Action switch
            {
                "recruit" => before.RecruitmentQuotes.Single(q => q.Type == command.SoldierType && q.Level == before.Slots[command.Slot].Level).Cost,
                "buy-home" => match.Army.HomeQuote(before.Army!.PurchasedHomes),
                "build" => command.Payment == ConstructionPayment.GoldRecovery
                    ? state.BuildingCatalog.Single(b => b.Type == command.Building).RecoveryConstruction
                    : state.BuildingCatalog.Single(b => b.Type == command.Building).Construction,
                "upgrade" => before.Slots[command.Slot].UpgradeQuote,
                "sell" => null,
                _ => throw new InvalidOperationException("Unexpected paid witness action: " + command.Action)
            };
            CommandResult result = match.Apply(id, command); Assert.True(result.Accepted, result.Message);
            CityState after = match.Snapshot().Players.Single(p => p.Id == id);
            if (quoted is ResourceCost quote)
            {
                Assert.True(before.Resources.TryPay(quote, out ResourceCost balance)); Assert.Equal(balance, after.Resources);
            }
            else
            {
                Assert.Equal("sell", command.Action);
                Assert.True(before.Resources.TryAdd(before.Slots[command.Slot].Refund, out ResourceCost balance)); Assert.Equal(balance, after.Resources);
            }
            payments.Add(new { match.Wave, match.Turn, Owner = id, command.Action, command.Slot, command.Payment, Quote = quoted, Before = before.Resources, After = after.Resources });
            foreach (UnitState unit in after.Soldiers.Where(u => !before.Soldiers.Any(old => old.Id == u.Id)))
            { Assert.Equal(id, unit.Owner); born.Add(unit.Id, (unit.Owner, unit.Type, unit.Level)); }
        }
        void Build(int id, int slot, Building type, ConstructionPayment payment = ConstructionPayment.Standard)
            => Paid(id, Command.FromSnapshot(match.Snapshot(), 1, "build", id, slot, type, payment: payment));
        foreach (int id in new[] { 1, 2 })
        {
            Build(id, 0, Building.Farm); Build(id, 2, Building.MetalMine); Build(id, 4, Building.MetalMine);
            Build(id, 3, Building.Lumbermill, ConstructionPayment.GoldRecovery);
        }
        void Invest(int id)
        {
            City city = match.Players[id];
            while (CampaignStrategy.ReinforcementInvestment(match.Snapshot(), id, id == 1) is EconomyAction decision)
                Paid(id, decision.Command(match.Snapshot(), id));
            output.WriteLine($"P{id} W{match.Wave} T{match.Turn} army={city.Soldiers.Count} gold={city.Gold} wood={city.Wood}");
        }
        void Prepare(bool firstWave)
        {
            if (firstWave)
            {
                for (int preparation = 0; preparation < 2; preparation++)
                    foreach (int player in new[] { 1, 2, 3 }) Assert.True(Act(match, player, "ready").Accepted);
                foreach (int player in new[] { 1, 2 }) Build(player, 1, Building.Barracks);
                // Retain the witness's two-city insertion order with actual
                // affordable two-unit batches, not guessed friendly IDs. All
                // twelve recruits now pay in final preparation from sixteen
                // metal per equipped city; no final income is spent here.
                for (int batch = 0; batch < 3; batch++)
                    foreach (int player in new[] { 1, 2 })
                        for (int recruit = 0; recruit < 2; recruit++) Paid(player, Command.FromSnapshot(match.Snapshot(), 1, "recruit", player, 1));
                foreach (int player in new[] { 1, 2, 3 }) Assert.True(Act(match, player, "ready").Accepted);
                Assert.All(match.Players.Values.Where(c => c.Id != 3), c =>
                {
                    Assert.Equal(6, c.Soldiers.Count); Assert.Equal(6, c.LastUpkeep!.Paid);
                    Assert.Equal(c.Soldiers.Select(u => u.Id), c.LastUpkeep.Funded);
                    Assert.Equal(12, c.Metal); Assert.Equal(9, c.Food);
                });
                return;
            }
            Paid(1, Command.FromSnapshot(match.Snapshot(), 1, "sell", 1, 4));
            Build(1, 4, Building.Stonecutter);
            while (match.Phase is Phase.Building or Phase.Preparation)
            {
                foreach (City city in match.Players.Values.Where(c => !c.Eliminated && !c.Ready))
                {
                    if (city.Id == 1) Invest(city.Id);
                    else if (city.Id == 2 && match.Phase == Phase.Preparation) Paid(2, Command.FromSnapshot(match.Snapshot(), 1, "recruit", 2, 1));

                    Assert.True(Act(match, city.Id, "ready").Accepted);
                }
                if (match.Players.Values.Where(c => !c.Eliminated).All(c => c.Ready)) match.Step();
            }
        }
        Prepare(true);
        while (match.Phase == Phase.Combat) match.Step();
        Assert.Equal(Phase.Building, match.Phase); Assert.Equal(2, match.Wave); Assert.True(match.Players[3].Eliminated);
        Prepare(false); Assert.Equal(12, match.Enemies.Count);
        Assert.Equal(6, match.ProductionCount);
        MatchSnapshot entry = match.Snapshot();
        Assert.All(entry.Players.Where(c => !c.Eliminated), c =>
        {
            Assert.Empty(c.LastUpkeep!.Unfed); Assert.Equal(c.Soldiers.Select(u => u.Id).Order(), c.LastUpkeep.Funded.Order());
            Assert.Equal(6, entry.Enemies.Count(u => u.Destination == c.Id));
        });
        Assert.Equal(4, entry.Enemies.Count(u => u.Origin == 3));
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
        output.WriteLine($"paid-transfer seed={seed} tick={transfer.Tick}; incidental held={string.Join(',', held)}; front={string.Join(',', forward)}");
        output.WriteLine(JsonSerializer.Serialize(before.Players[0].Soldiers.Select(u => new { u.Id, u.Owner, Position = u.Hex!.Position, u.Hex.Action, u.ReadyTick }), WireJson.Options));
        Assert.NotEmpty(before.Players[0].Soldiers);
        foreach (UnitState incoming in transfer.Enemies.Where(u => u.Destination == 1))
        {
            UnitState source = before.Enemies.Single(u => u.Id == incoming.Id);
            Assert.NotEqual(1, source.Destination); Assert.NotEqual(1, incoming.Origin);
            Assert.Equal((source.Id, source.Origin, source.Level, source.Profile, source.Size, source.Owner, source.Faction),
                (incoming.Id, incoming.Origin, incoming.Level, incoming.Profile, incoming.Size, incoming.Owner, incoming.Faction));
            Assert.InRange(incoming.Health, 1, source.Health);
        }
        foreach (UnitState veteran in before.Players[0].Soldiers)
        {
            UnitState preserved = transfer.Players[0].Soldiers.Single(u => u.Id == veteran.Id);
            Assert.Equal((veteran.Owner, veteran.Health, veteran.Level, veteran.Profile, veteran.Assignment),
                (preserved.Owner, preserved.Health, preserved.Level, preserved.Profile, preserved.Assignment));
        }
        AdmissionBound bound = Assert.Single(transfer.Admissions, a => a.City == 1);
        Assert.NotNull(bound.AdmissionTick); Assert.True(bound.AdmissionTick <= bound.FirstAdmissionBound);
        Assert.Contains(transfer.Enemies, u => u.Id == bound.FirstUnitId && u.Deployed && u.Origin != 1);
        while (match.Phase == Phase.Combat) match.Step();
        Assert.Equal(Phase.Building, match.Phase); Assert.Equal(3, match.Wave); Assert.Equal(DefeatReason.None, match.DefeatReason);
        Assert.NotEmpty(match.Players[1].Soldiers);
        Assert.All(match.Players[1].Soldiers, u => Assert.Equal(born[u.Id], (u.Owner, u.Type, u.Level)));
        Assert.False(match.Players[1].Eliminated); Assert.True(match.Players[2].Eliminated); Assert.True(match.Players[3].Eliminated);
        output.WriteLine(JsonSerializer.Serialize(new { Payments = payments, Born = born, SurvivorIds = match.Players[1].Soldiers.Select(u => u.Id) }, WireJson.Options));
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
    public void FinalPreparationSettlesProductionBeforeUpkeepWithoutAnotherEditingWindow()
    {
        using Match match = Start(); Assert.True(Act(match, 1, "build", 0, Building.Farm).Accepted);
        Assert.True(Act(match, 1, "build", 1, Building.Barracks).Accepted);
        Assert.True(Act(match, 1, "build", 2, Building.MetalMine).Accepted);
        for (int n = 0; n < 2; n++) Assert.True(Act(match, 1, "ready").Accepted);
        Assert.Equal(Phase.Preparation, match.Phase); Assert.Equal(2, match.ProductionCount); Assert.Equal(3, match.Turn);
        Command stale = new(1, match.Id, Phase.Building, match.TurnSerial - 1, "ready", 1);
        Assert.False(match.Apply(1, stale).Accepted);
        Assert.True(Act(match, 1, "recruit", 1).Accepted); CityState before = match.Players[1].Snapshot();
        Assert.True(Act(match, 1, "ready").Accepted); Assert.Equal(Phase.Combat, match.Phase);
        CityState after = match.Players[1].Snapshot();
        Assert.True(before.Resources.TryAdd(before.ProductionIncome!.Value, out ResourceCost produced));
        Assert.True(produced.TryPay(new(Food: match.Economy.Upkeep(UnitType.Swordsman)), out ResourceCost paid));
        Assert.Equal(paid, after.Resources); Assert.Equal(3, match.ProductionCount);
        Assert.False(Act(match, 1, "recruit", 1).Accepted);
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
