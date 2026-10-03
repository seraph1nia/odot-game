using System.Text.Json;
using Xunit;

namespace Game.Core.Tests;

public sealed class ArmyProgressionTests
{
    private static Match Start(int count = 1)
    {
        var match = new Match(combatSeed: 1); for (int n = 0; n < count; n++) match.Join();
        Act(match, "start"); return match;
    }
    private static Command Request(Match match, string action, int slot = -1, UnitType type = UnitType.Swordsman)
        => new(1, match.Id, match.Phase, match.TurnSerial, action, 1, slot, SoldierType: type,
            ExpectedGeneration: slot is >= 0 and < 9 ? match.Players[1].Slots[slot].Generation : 0);
    private static void Act(Match match, string action, int slot = -1, UnitType type = UnitType.Swordsman)
        => Assert.True(match.Apply(1, Request(match, action, slot, type)).Accepted);
    private static void Build(Match match, int slot, Building type)
        => Assert.True(match.Apply(1, new(1, match.Id, match.Phase, match.TurnSerial, "build", 1, slot, type)).Accepted);
    private static void Stock(City city) { city.Gold = 1000; city.Wood = 1000; city.Stone = 1000; city.Metal = 1000; city.Cloth = 1000; }
    private static void Prepare(Match match)
    {
        for (int turn = 0; turn < 3; turn++)
            foreach (City city in match.Players.Values.Where(c => c.Connected))
                Assert.True(match.Apply(city.Id, new(1, match.Id, match.Phase, match.TurnSerial, "ready", city.Id)).Accepted);
        Assert.Equal(Phase.Preparation, match.Phase);
    }

    [Fact]
    public void StatsRoundOriginalBaseOnceBeforeBossAndFractionalResearch()
    {
        var configuration = new CombatConfiguration(new());
        int[] health = [40, 54, 73, 98, 133], damage = [10, 14, 18, 25, 33];
        for (int level = 1; level <= 5; level++)
        {
            HexCombatProfile ordinary = configuration.Unit(UnitType.Swordsman, level: level);
            Assert.Equal(health[level - 1] * 100, ordinary.Health); Assert.Equal(damage[level - 1] * 100, ordinary.Damage);
            Assert.Equal(2, ordinary.Size);
            Assert.Equal(configuration.Unit(UnitType.Swordsman).MoveTicks, ordinary.MoveTicks);
            Assert.Equal(configuration.Unit(UnitType.Swordsman).CadenceTicks, ordinary.CadenceTicks);
        }
        HexCombatProfile boss = configuration.Unit(UnitType.Swordsman, isBoss: true, level: 3);
        Assert.Equal(58400, boss.Health); Assert.Equal(3600, boss.Damage); Assert.Equal(6, boss.Size);
        Assert.Equal(106400, configuration.Unit(UnitType.Swordsman, isBoss: true, level: 5).Health);
        HexCombatProfile researched = configuration.Unit(UnitType.Swordsman, rank: 1, isBoss: true, level: 3);
        Assert.Equal(61320, researched.Health); Assert.Equal(3780, researched.Damage);
        Assert.Equal(17900, configuration.Unit(UnitType.Swordsman, level: 6).Health);
        Assert.Equal(3, Progression.ScaleWhole(2, 2)); // 2.7 rounds up.
        Assert.Equal(14, Progression.ScaleWhole(10, 2)); // 13.5 rounds upward.
        Assert.Throws<ArgumentOutOfRangeException>(() => configuration.Unit(UnitType.Swordsman, level: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => configuration.Unit(UnitType.Swordsman, level: 257));
        Assert.Throws<OverflowException>(() => configuration.Unit(UnitType.Swordsman, level: 200));
        Assert.Throws<OverflowException>(() => Progression.ScaleWhole(int.MaxValue, 2));
    }

    [Fact]
    public void EveryOrdinaryArchetypeHasFactionSymmetryBeyondTheRecruitCap()
    {
        using var combat = new CombatSimulation(new());
        foreach (UnitType type in Enum.GetValues<UnitType>())
            for (int level = 1; level <= 6; level++)
            {
                int ally = combat.Create(type, 1, 1, 1, rank: 2, level: level);
                int enemy = combat.Create(type, 0, 1, 1, Faction.Skeletons, rank: 2, level: level);
                Assert.Equal(combat.Read(ally).Profile, combat.Read(enemy).Profile);
                Assert.Equal(2, combat.Read(ally).Size); Assert.Equal(level, combat.Read(enemy).Level);
            }
    }

    [Fact]
    public void EquipmentPricesAreBaseDerivedNearestWholeAndUpkeepIsSeparate()
    {
        var economy = new EconomyConfiguration(new());
        int[] swords = [2, 3, 4, 5, 7], cloth = [3, 4, 5, 7, 10], gold = [1, 1, 2, 2, 3];
        for (int level = 1; level <= 5; level++)
        {
            Assert.Equal(new ResourceCost(Metal: swords[level - 1]), economy.Recruitment(UnitType.Swordsman, level));
            Assert.Equal(new ResourceCost(gold[level - 1], Cloth: cloth[level - 1]), economy.Recruitment(UnitType.Mage, level));
            foreach (UnitType type in Enum.GetValues<UnitType>())
            {
                ResourceCost quote = economy.Recruitment(type, level); Assert.Equal(0, quote.Food);
                Assert.Equal(type is UnitType.Berserker or UnitType.Mage ? 2 : 1, economy.Upkeep(type));
                if (level > 1)
                    Assert.True(Enum.GetValues<Resource>().Sum(r => (long)quote.Amount(r)) > Enum.GetValues<Resource>().Sum(r => (long)economy.Recruitment(type, level - 1).Amount(r)));
            }
        }
        Assert.Throws<ArgumentException>(() => new EconomyConfiguration(new Rules { MageClothCost = 0 }));
        var small = new EconomyConfiguration(new Rules { SwordMetalCost = 1 });
        Assert.Equal(small.Recruitment(UnitType.Swordsman), small.Recruitment(UnitType.Swordsman, 2));
        Assert.Throws<ArgumentException>(() => new EconomyConfiguration(new Rules { MageGoldCost = 0 }));
        Assert.Throws<ArgumentException>(() => new EconomyConfiguration(new Rules { SwordUpkeep = 0 }));
        Assert.NotEqual(economy.Fingerprint, new EconomyConfiguration(new Rules { SwordUpkeep = 2 }).Fingerprint);
    }

    [Fact]
    public void IndependentRecruitmentLevelsAndResearchPreserveWoundedVeteransAcrossSales()
    {
        using Match match = Start(); City city = match.Players[1]; Stock(city); city.Food = 17;
        Build(match, 0, Building.Barracks); Build(match, 1, Building.Barracks); Build(match, 2, Building.ResearchTower);
        Act(match, "recruit", 0); UnitState veteran = Assert.Single(city.Soldiers);
        match.Combat.Seed(veteran with { Health = veteran.Health - 100 });
        for (int upgrade = 0; upgrade < 4; upgrade++) Act(match, "upgrade", 1);
        Assert.Equal(5, city.Slots[1].Level); Assert.Equal(1, city.Slots[0].Level); Assert.Single(city.Soldiers);
        Act(match, "recruit", 1); Assert.Equal(17, city.Food);
        Assert.Equal(1, city.Soldiers[0].Level); Assert.Equal(5, city.Soldiers[1].Level);
        city.Research = city.Research.Income(points: 3); Assert.True(match.Apply(1, Request(match, "research-tech") with { Technology = TechnologyId.MeleeFoundation }).Accepted); Assert.Equal(3900, city.Soldiers[0].Health); Assert.Equal(13300, city.Soldiers[1].Health);
        Assert.Equal(13965, city.Soldiers[1].Profile.Health); Assert.Equal(3465, city.Soldiers[1].Profile.Damage);
        UnitState[] old = city.Soldiers.ToArray(); Act(match, "sell", 1); Act(match, "sell", 2);
        Assert.Equal(old, city.Soldiers); Assert.True(city.Research.Has(TechnologyId.MeleeFoundation));
        Assert.False(match.Apply(1, Request(match, "research", 2)).Accepted);
        Build(match, 1, Building.Barracks); Assert.Equal(1, city.Slots[1].Level); Assert.Equal(2, city.Soldiers.Count);
        Act(match, "recruit", 1); Assert.Equal(1, city.Soldiers[^1].Level); Assert.Equal(4200, city.Soldiers[^1].Health);
        Assert.False(match.Apply(1, Request(match, "upgrade", 2)).Accepted);
    }

    [Fact]
    public void BossResearchTransferAndWireNeverReapplyMultipliers()
    {
        using var combat = new CombatSimulation(new());
        int ally = combat.Create(UnitType.Swordsman, 1, 1, 1, rank: 0, isBoss: true, level: 5);
        UnitState initial = combat.Read(ally); combat.Seed(initial with { Health = initial.Health - 100 });
        combat.Research(1, new(Owned: 1UL << (int)TechnologyId.MeleeFoundation), new()); combat.Transfer(ally, 2);
        UnitState transferred = combat.Read(ally); Assert.Equal(5, transferred.Level); Assert.True(transferred.IsBoss); Assert.Equal(6, transferred.Size);
        Assert.Equal(initial.Health - 100, transferred.Health); Assert.Equal(111720, transferred.Profile.Health); Assert.Equal(6930, transferred.Profile.Damage);
        string json = JsonSerializer.Serialize(transferred, WireJson.Options);
        Assert.Equal(json, JsonSerializer.Serialize(JsonSerializer.Deserialize<UnitState>(json, WireJson.Options), WireJson.Options));
        int enemy = combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons, rank: 1, isBoss: true, level: 5);
        Assert.Equal(transferred.Profile, combat.Read(enemy).Profile);
    }

    [Fact]
    public void FoodAllocationSkipsUnaffordableStrongerUnitsAndKeepsStableTies()
    {
        var economy = new EconomyConfiguration(new());
        UnitState[] soldiers = [new(4, 100) { Type = UnitType.Swordsman, Level = 2 }, new(3, 100) { Type = UnitType.Swordsman, Level = 2 },
            new(1, 100) { Type = UnitType.Mage, Level = 5 }, new(2, 0) { Type = UnitType.Swordsman, Level = 5 }];
        BattleFoodForecast forecast = BattleFood.Forecast(soldiers, 1, economy);
        Assert.Equal(4, forecast.Demand); Assert.Equal(1, forecast.Paid); Assert.Equal(3, Assert.Single(forecast.Participating)); Assert.Collection(forecast.Unfed, id => Assert.Equal(1, id), id => Assert.Equal(4, id));
        Assert.Equal(JsonSerializer.Serialize(forecast), JsonSerializer.Serialize(BattleFood.Forecast(soldiers.Reverse(), 1, economy)));
    }

    [Fact]
    public void BattleEntryChargesDisconnectedLivingCityOnceAndReservesHaveNoClaims()
    {
        using Match match = Start(2); City city = match.Players[1]; Stock(city);
        Build(match, 0, Building.Barracks); Build(match, 1, Building.Arcanum);
        for (int upgrade = 0; upgrade < 4; upgrade++) Act(match, "upgrade", 1);
        Act(match, "recruit", 1, UnitType.Mage); Act(match, "recruit", 0); city.Food = 1;
        Prepare(match); Assert.Equal(Phase.Preparation, match.Phase); Assert.Null(city.LastUpkeep); Assert.Equal(1, city.Food);
        UnitState mage = city.Soldiers.Single(u => u.Type == UnitType.Mage);
        UnitState sword = city.Soldiers.Single(u => u.Type == UnitType.Swordsman);
        Act(match, "ready"); Act(match, "unready"); Assert.Equal(1, city.Food);
        Act(match, "pause"); match.SetConnected(1, false);
        City other = match.Players[2]; other.Food = 5;
        Assert.True(match.Apply(2, new(1, match.Id, match.Phase, match.TurnSerial, "resume", 2)).Accepted);
        Assert.True(match.Apply(2, new(1, match.Id, match.Phase, match.TurnSerial, "ready", 2)).Accepted);
        Assert.Equal(Phase.Combat, match.Phase); Assert.Equal(0, city.Food); Assert.Equal(5, other.Food);
        Assert.Equal(new[] { sword.Id }, city.LastUpkeep!.Participating); Assert.Equal(new[] { mage.Id }, city.LastUpkeep.Unfed);
        UnitState reserve = city.Soldiers.Single(u => u.Id == mage.Id);
        Assert.Equal(UnitLifecycle.Reserve, reserve.Hex!.Lifecycle); Assert.False(reserve.Deployed); Assert.False(reserve.Participating);
        Assert.Equal(mage.Health, reserve.Health); Assert.Equal(5, reserve.Level); Assert.Equal(0, reserve.TargetId); Assert.False(reserve.PendingImpact);
        Assert.DoesNotContain(match.Snapshot().Reservations.Positions, p => p.UnitId == mage.Id);
        for (int step = 0; step < 120; step++) match.Step();
        Assert.Equal(mage.Health, city.Soldiers.Single(u => u.Id == mage.Id).Health); Assert.Equal(0, city.Food);
        match.SetConnected(1, true); Assert.Equal(0, city.Food); Assert.Equal(1, city.LastUpkeep.Wave);
    }

    [Fact]
    public void PausedAuthorityResumeRetainsUpkeepAndAcceptedBattleReadyReceipt()
    {
        using var session = new AuthoritySession(AuthorityPolicy.Dedicated);
        AdmissionResult admitted = session.Admit(2, WireJson.ProtocolVersion, "", 1000); Assert.True(admitted.Accepted);
        int peer = 2; long sequence = 0;
        Command RequestAt(string action, int slot = -1, Building building = Building.Empty)
        {
            MatchSnapshot state = session.Snapshot(); CityState city = Assert.Single(state.Players);
            return new(++sequence, state.MatchId, state.Phase, state.TurnSerial, action, city.Id, slot, building,
                ExpectedGeneration: slot is >= 0 and < 9 ? city.Slots[slot].Generation : 0);
        }
        CommandResult Send(Command command) => session.Request(peer, JsonSerializer.Serialize(command, WireJson.Options), 1000)!;
        Assert.True(Send(RequestAt("start")).Accepted);
        Assert.True(Send(RequestAt("build", 0, Building.Farm)).Accepted);
        Assert.True(Send(RequestAt("build", 1, Building.MetalMine)).Accepted);
        Assert.True(Send(RequestAt("build", 2, Building.Barracks)).Accepted);
        for (int production = 0; production < 3; production++) Assert.True(Send(RequestAt("ready")).Accepted);
        Assert.True(Send(RequestAt("recruit", 2)).Accepted); Assert.Equal(15, Assert.Single(session.Snapshot().Players).Food);
        Assert.True(Send(RequestAt("pause")).Accepted); Assert.False(Send(RequestAt("ready")).Accepted);
        Assert.True(Send(RequestAt("resume")).Accepted); Command ready = RequestAt("ready"); Assert.True(Send(ready).Accepted);
        Assert.True(Send(RequestAt("pause")).Accepted); CityState before = Assert.Single(session.Snapshot().Players);
        Assert.Equal(14, before.Food); Assert.Equal(1, before.LastUpkeep!.Paid);
        session.Disconnect(2);
        AdmissionResult resumed = session.Admit(3, WireJson.ProtocolVersion, admitted.Credential, 1000, expectedMatchId: session.MatchId);
        Assert.True(resumed.Accepted); peer = 3; CityState after = Assert.Single(resumed.State!.Players);
        Assert.Equal(before.Resources, after.Resources); Assert.Equal(JsonSerializer.Serialize(before.LastUpkeep), JsonSerializer.Serialize(after.LastUpkeep));
        Assert.Equal(JsonSerializer.Serialize(before.Soldiers), JsonSerializer.Serialize(after.Soldiers));
        string stateBeforeRetry = JsonSerializer.Serialize(session.Snapshot(), WireJson.Options);
        Assert.True(Send(ready).Accepted); Assert.Equal(stateBeforeRetry, JsonSerializer.Serialize(session.Snapshot(), WireJson.Options));
        Assert.Equal(1, after.Soldiers.Single().Level); Assert.True(after.Soldiers.Single().Participating);
    }

    [Fact]
    public void FedPurchasedHomeSoldiersPayAtEntryAndReceiptsAreProjectionCopies()
    {
        using Match match = Start(); City city = match.Players[1]; Stock(city);
        Build(match, 0, Building.Barracks);
        for (int purchase = 0; purchase < 2; purchase++)
            Assert.True(match.Apply(1, Request(match, "buy-home") with { ExpectedHomeCount = city.PurchasedHomes }).Accepted);
        for (int recruit = 0; recruit < 12; recruit++) Act(match, "recruit", 0);
        city.Food = 12; Prepare(match); Assert.Equal(12, city.Snapshot().FoodForecast!.Demand);
        Act(match, "ready"); Assert.Equal(0, city.Food); Assert.Equal(12, city.LastUpkeep!.Paid);
        Assert.Equal(12, city.LastUpkeep.Participating.Length); Assert.Empty(city.LastUpkeep.Unfed);
        Assert.Equal(12, city.Soldiers.Count(u => u.Deployed && u.Participating));
        Assert.All(city.Soldiers, u => Assert.Equal(u.Assignment!.Position, u.Hex!.Position));
        Assert.All(city.Soldiers.GroupBy(u => u.Assignment!.Tile), home => Assert.InRange(home.Sum(u => u.Size), 1, 6));
        CityState projection = city.Snapshot(); projection.LastUpkeep!.Participating[0] = 999; projection.LastUpkeep.Funded[0] = 999;
        Assert.DoesNotContain(999, city.Snapshot().LastUpkeep!.Participating); Assert.DoesNotContain(999, city.Snapshot().LastUpkeep!.Funded);
        long revision = match.Revision; match.Snapshot(); Assert.Equal(revision, match.Revision); Assert.Equal(0, city.Food);
    }

    [Fact]
    public void CleanupBarrierAndAcceptedReadinessRetryDoNotChargeFoodEarly()
    {
        using Match match = Start(); City city = match.Players[1]; Stock(city);
        Build(match, 0, Building.Barracks); Act(match, "recruit", 0); city.Food = 1;
        int corpseId = match.Combat.Create(UnitType.Swordsman, 1, 1, 1);
        UnitState corpse = match.Combat.Read(corpseId);
        match.Combat.Seed(corpse with
        {
            Health = 0,
            Deployed = true,
            Hex = new(corpseId, 1, Faction.Adventurers, UnitLifecycle.Dying, new(19, 1), DeathEndTick: 5, Size: 2)
        });
        Prepare(match); var ledger = new CommandLedger(); Command ready = Request(match, "ready");
        Assert.True(ledger.Execute(ready, () => match.Apply(1, ready)).Accepted);
        Assert.Equal(Phase.Preparation, match.Phase); Assert.Equal(1, city.Food); Assert.Null(city.LastUpkeep);
        for (int tick = 0; tick < 4; tick++) match.Step();
        Assert.Equal(Phase.Preparation, match.Phase); Assert.Equal(1, city.Food);
        Assert.True(ledger.Execute(ready, () => match.Apply(1, ready)).Accepted); Assert.Equal(1, city.Food);
        match.Step(); Assert.Equal(Phase.Combat, match.Phase); Assert.Equal(0, city.Food); Assert.Equal(1, city.LastUpkeep!.Paid);
        Assert.Single(city.LastUpkeep.Participating); Assert.Empty(city.LastUpkeep.Unfed);
        Assert.True(ledger.Execute(ready, () => match.Apply(1, ready)).Accepted); Assert.Equal(0, city.Food);
    }

    [Fact]
    public void AllUnfedArmyDoesNotScreenSiegeAndDefenseStillFires()
    {
        using Match match = Start(); City city = match.Players[1]; Stock(city);
        Build(match, 0, Building.Barracks); Act(match, "recruit", 0);
        UnitState soldier = Assert.Single(city.Soldiers); Prepare(match); Act(match, "ready");
        Assert.Equal(0, city.LastUpkeep!.Paid); Assert.Equal(soldier.Id, Assert.Single(city.LastUpkeep.Unfed));
        for (int step = 0; step < 1000 && city.Health == 10000; step++) match.Step();
        Assert.True(city.Health < 10000); Assert.Equal(DefeatReason.None, match.DefeatReason);
        Assert.Equal(soldier.Health, Assert.Single(city.Soldiers).Health);
        Assert.Contains(match.Snapshot().CombatEvents, e => e.Tower?.Type == Building.Empty && e.Type == CombatEventType.Impact && e.Landed);
        Assert.DoesNotContain(match.Snapshot().CombatEvents, e => e.Unit?.Id == soldier.Id || !e.TargetCity && e.TargetId == soldier.Id);
        Assert.DoesNotContain(match.Snapshot().Reservations.Positions, p => p.UnitId == soldier.Id);
    }

    [Fact]
    public void DeathSnapshotRetainsLeveledBossIdentityAndProfile()
    {
        using var combat = new CombatSimulation(new());
        int boss = combat.Create(UnitType.Swordsman, 1, 1, 1, isBoss: true, level: 3);
        int opponent = combat.Create(UnitType.Crossbowman, 0, 1, 1, Faction.Skeletons, level: 6);
        UnitState initial = combat.Read(boss);
        combat.Seed(initial with { Health = 1, Hex = CombatFixture.At(initial, 8, 1) });
        UnitState enemy = combat.Read(opponent); combat.Seed(enemy with { Hex = CombatFixture.At(enemy, 11, 1) });
        for (int tick = 1; tick <= 300 && combat.Dying().Length == 0; tick++) combat.Step(tick, []);
        UnitState dying = Assert.Single(combat.Dying()); Assert.Equal(boss, dying.Id); Assert.Equal(3, dying.Level);
        Assert.True(dying.IsBoss); Assert.Equal(6, dying.Size); Assert.Equal(initial.Profile, dying.Profile);
        Assert.Equal(0, dying.Health); Assert.Equal(UnitLifecycle.Dying, dying.Hex!.Lifecycle);
        Assert.Equal(dying.Id, Assert.Single(combat.Events(), e => e.Type == CombatEventType.Death).Unit!.Id);
    }

    [Fact]
    public void UnfedReservesReturnNextWaveWithSameHealthWhileFedCapacityQueuesPay()
    {
        using var combat = new CombatSimulation(new());
        int reserve = combat.Create(UnitType.Mage, 1, 1, 1, level: 3); UnitState original = combat.Read(reserve);
        combat.Seed(original with { Health = original.Health - 100 });
        _ = Enumerable.Range(0, 12).Select(_ => combat.Create(UnitType.Swordsman, 1, 1, 1, level: 4)).ToArray();
        var economy = new EconomyConfiguration(new());
        BattleFoodForecast forecast = BattleFood.Forecast(combat.Soldiers(1), 12, economy);
        Assert.Equal(12, forecast.Paid); Assert.Equal(12, forecast.Participating.Length); Assert.Equal(new[] { reserve }, forecast.Unfed);
        combat.BeginWave(1, forecast.Unfed); Assert.Equal(UnitLifecycle.Reserve, combat.Read(reserve).Hex!.Lifecycle);
        Assert.Contains(combat.Soldiers(1), u => u.Participating && !u.Deployed);
        combat.BeginWave(2); UnitState returned = combat.Read(reserve);
        Assert.True(returned.Participating); Assert.Equal(original.Health - 100, returned.Health); Assert.Equal(3, returned.Level);
    }
}
