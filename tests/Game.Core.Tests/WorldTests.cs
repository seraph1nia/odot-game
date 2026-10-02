using System.Text.Json;
using Game.Core;
using Xunit;

namespace Game.Core.Tests;

public sealed class WorldTests
{
    private static Command Cmd(Match m, int id, string action, int slot = -1, Building building = Building.Empty, long seq = 1) => new(seq, m.Id, m.Phase, m.TurnSerial, action, id, slot, building, ExpectedGeneration: slot is >= 0 and < 9 ? m.Players[id].Slots[slot].Generation : 0);
    private static void Act(Match m, int id, string action, int slot = -1, Building building = Building.Empty) => Assert.True(m.Apply(id, Cmd(m, id, action, slot, building)).Accepted);
    private static Match Started(int count = 1, Rules? rules = null, ulong seed = 123)
    {
        var m = new Match(rules, combatSeed: seed);
        for (int i = 0; i < count; i++) Assert.NotNull(m.Join());
        Act(m, 1, "start"); return m;
    }
    private static void Ready(Match m)
    {
        foreach (City c in m.Players.Values.Where(c => !c.Eliminated && c.Connected)) Act(m, c.Id, "ready");
        for (int tick = 0; tick < 48 && m.Phase == Phase.Preparation && m.Players.Values.Where(c => !c.Eliminated && c.Connected).All(c => c.Ready); tick++) m.Step();
    }
    private static void Battle(Match m) { Ready(m); Ready(m); Ready(m); Ready(m); Assert.Equal(Phase.Combat, m.Phase); }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void RosterLocksAndSnapshotRoundTrips(int count)
    {
        using Match m = Started(count);
        Assert.Null(m.Join());
        Assert.All(m.Players.Values, c => { Assert.Equal(60, c.Gold); Assert.Equal(0, c.Food); Assert.Equal(10000, c.Health); Assert.Equal(9, c.Slots.Length); });
        string json = JsonSerializer.Serialize(m.Snapshot(), WireJson.Options);
        Assert.Equal(json, JsonSerializer.Serialize(JsonSerializer.Deserialize<MatchSnapshot>(json, WireJson.Options), WireJson.Options));
        using var lobby = new Match(); for (int i = 0; i < 4; i++) lobby.Join(); Assert.Null(lobby.Join());
    }
    [Fact]
    public void StartLocksOnlyConnectedRoster()
    {
        using var m = new Match(); m.Join(); m.Join(); m.SetConnected(2, false); Act(m, 1, "start");
        Assert.Single(m.Players); Assert.False(m.Apply(2, Cmd(m, 2, "ready")).Accepted);
    }
    [Fact]
    public void PurchasesAreAtomicOwnedAndBounded()
    {
        using Match m = Started(2); Act(m, 1, "build", 0, Building.Mine);
        Assert.Equal(40, m.Players[1].Gold);
        Command[] invalid = [Cmd(m, 1, "build", 0, Building.Farm), Cmd(m, 1, "build", -1, Building.Farm), Cmd(m, 1, "build", 9, Building.Farm), Cmd(m, 1, "build", 1, (Building)100), Cmd(m, 2, "build", 1, Building.Farm), Cmd(m, 1, "nonsense", 1)];
        foreach (Command c in invalid)
        {
            string before = JsonSerializer.Serialize(m.Snapshot());
            Assert.False(m.Apply(1, c).Accepted); Assert.Equal(before, JsonSerializer.Serialize(m.Snapshot()));
        }
        Act(m, 1, "build", 1, Building.Farm); Act(m, 1, "build", 2, Building.Barracks);
        Assert.False(m.Apply(1, Cmd(m, 1, "build", 3, Building.Mine)).Accepted); Assert.Equal(0, m.Players[1].Gold);
    }
    [Fact]
    public void ProductionRecruitmentAndUpgradesAreExplicit()
    {
        using Match m = Started();
        Act(m, 1, "build", 0, Building.Farm); Act(m, 1, "build", 1, Building.Barracks); Act(m, 1, "build", 2, Building.MetalMine);
        Assert.False(m.Apply(1, Cmd(m, 1, "recruit", 1)).Accepted);
        Ready(m); Assert.Equal(5, m.Players[1].Food); Assert.Empty(m.Players[1].Soldiers);
        Assert.False(m.Apply(1, Cmd(m, 1, "recruit", 0)).Accepted);
        Act(m, 1, "recruit", 1); Assert.Single(m.Players[1].Soldiers); Assert.Equal(5, m.Players[1].Food);
        Ready(m); Ready(m); m.Players[1].Wood = 10; // Isolate the upgrade transaction from the separate supply-chain gate.
        Act(m, 1, "upgrade", 1); int food = m.Players[1].Food;
        Act(m, 1, "recruit", 1); Assert.Equal(food, m.Players[1].Food);
        Assert.Equal(4000, m.Players[1].Soldiers[0].Health); Assert.Equal(2, m.Players[1].Soldiers[1].Level);
        using var n = Started(); n.Players[1].Stone = 10; Act(n, 1, "build", 0, Building.Mine); Act(n, 1, "upgrade", 0); Ready(n); Assert.Equal(40, n.Players[1].Gold);
    }
    [Fact]
    public void ReadyEditingDisconnectAndUniqueTurnGuards()
    {
        using Match m = Started(2); Command stale = Cmd(m, 2, "ready"); Act(m, 1, "ready");
        Assert.False(m.Apply(1, Cmd(m, 1, "build", 0, Building.Mine)).Accepted);
        Act(m, 1, "unready"); Act(m, 1, "build", 0, Building.Mine); Act(m, 1, "ready");
        m.SetConnected(2, false); Assert.Equal(2, m.Turn); Assert.Equal(70, m.Players[2].Gold); Assert.False(m.Players[2].Ready);
        m.SetConnected(2, true); Assert.False(m.Apply(2, stale).Accepted);
        m.SetConnected(1, false); m.SetConnected(2, false); long revision = m.Revision;
        for (int i = 0; i < 100; i++) m.Step(); Assert.Equal(revision, m.Revision); Assert.Equal(2, m.Turn);
        m.SetConnected(1, true); Ready(m); Ready(m); Ready(m); Assert.Equal(Phase.Combat, m.Phase);
        Assert.False(m.Apply(1, Cmd(m, 1, "recruit", 0)).Accepted);
    }
    [Fact]
    public void RetryLedgerPreservesAcceptedAndRejectedIdentity()
    {
        using Match m = Started(); var ledger = new CommandLedger(); Command command = Cmd(m, 1, "build", 0, Building.Farm, 5);
        CommandResult once = ledger.Execute(command, () => m.Apply(1, command));
        Assert.Equal(once, ledger.Execute(command, () => throw new InvalidOperationException("Executed twice"))); Assert.Equal(40, m.Players[1].Gold);
        Command bad = command with { Sequence = 6, Slot = 0 }; Assert.False(ledger.Execute(bad, () => m.Apply(1, bad)).Accepted);
        Assert.False(ledger.Execute(bad with { Slot = 1 }, () => throw new InvalidOperationException("Reused rejection")).Accepted);
        Assert.True(ledger.Execute(command with { Sequence = 7, Slot = 1 }, () => m.Apply(1, command with { Sequence = 7, Slot = 1 })).Accepted);
    }
    [Fact]
    public void CombatIsDeterministicBoundedAndBuiltInDefenseWorks()
    {
        using Match a = Started(); using Match b = Started(); Battle(a); Battle(b);
        int health = a.Enemies.Sum(e => e.Health);
        a.Step(); b.Step(); Assert.Equal(health, a.Enemies.Sum(e => e.Health)); Assert.True(a.Players[1].Defender.PendingImpact); Assert.False(a.Players[1].Eliminated);
        for (int i = 0; i < 600; i++)
        {
            a.Step(); b.Step(); Assert.Equal(JsonSerializer.Serialize(a.Snapshot().Enemies), JsonSerializer.Serialize(b.Snapshot().Enemies));
            Assert.All(a.Enemies.Where(e => e.Deployed), e => Assert.InRange(e.Hex!.Position.Cell, 1, 21));
        }
        Assert.True(a.Enemies.Sum(e => e.Health) < health);
        long shots = (a.Tick - 1 - a.Configuration.Defender.WindupTicks) / a.Configuration.Defender.CadenceTicks + 1;
        Assert.True(a.Enemies.Sum(e => e.Health) >= health - shots * a.Configuration.DefenderDamage); // At most one configured shot per cadence.
    }
    [Fact]
    public void ArmyDeathDoesNotEliminateCityAndDamageIsSimultaneous()
    {
        using Match m = Started(2, new Rules { DefenderDamage = 0, SoldierHealth = 10, SoldierDamage = 10, MeleeWindupTicks = 1 }); Battle(m);
        CombatFixture.Soldier(m, 1000, 1, 2);
        CombatFixture.Change(m, m.Enemies[0].Id, e => e with { Hex = CombatFixture.At(e, 14, 1) });
        CombatFixture.Steps(m, 2);
        Assert.Empty(m.Players[1].Soldiers); Assert.DoesNotContain(m.Enemies, e => e.Id == 1); Assert.False(m.Players[1].Eliminated);
    }
    [Fact]
    public void EliminatingOneCityCancelsItsLockedTowerWhileOtherCitiesKeepFighting()
    {
        using Match match = Started(2, new Rules { Campaign = CampaignFixture.Three(first: 1), DefenderDamage = 0, MeleeWindupTicks = 1 });
        match.Players[1].Stone = 15; match.Players[1].Metal = 10;
        Act(match, 1, "build", 0, Building.CatapultTower); Battle(match);
        match.Players[1].Health = 1; CombatFixture.AtCityEdge(match);
        match.Step(); Assert.True(match.Players[1].Towers[0].PendingImpact);
        match.Step(); Assert.True(match.Players[1].Eliminated); Assert.Equal(Phase.Combat, match.Phase);
        Assert.False(match.Players[1].Towers[0].PendingImpact); Assert.False(match.Players[1].Defender.PendingImpact);
        CombatFixture.Steps(match, 30);
        Assert.DoesNotContain(match.Combat.Events(), e => e.Type == CombatEventType.Impact && e.Tower?.City == 1);
    }
    [Fact]
    public void NewlyDeployedDefenderMakesACommittedCityAttackMissWithoutRetargeting()
    {
        var rules = new Rules { DefenderDamage = 0 };
        using var combat = new CombatSimulation(rules); var city = new City(1, rules, combat, new(rules), new(rules));
        int attacker = combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons);
        UnitState enemy = combat.Read(attacker);
        combat.Seed(enemy with { Hex = CombatFixture.At(enemy, 17, 1) });
        combat.Step(1, [city]); Assert.True(combat.Read(attacker).TargetCity);
        int defender = combat.Create(UnitType.Crossbowman, 1, 1, 1);
        UnitState ally = combat.Read(defender);
        combat.Seed(ally with { Hex = CombatFixture.At(ally, 20, 1) });
        combat.Advance(13, [city]);
        CombatEvent impact = Assert.Single(combat.Events(), e => e.Type == CombatEventType.Impact && e.Unit?.Id == attacker);
        Assert.True(impact.TargetCity); Assert.False(impact.Landed); Assert.Equal(10000, city.Health);
        Assert.Equal(UnitActionKind.Recovery, combat.Read(attacker).Hex!.Action); Assert.Equal(61, combat.Read(attacker).ReadyTick);
        Assert.Equal(ally.Health, combat.Read(defender).Health);
    }
    [Fact]
    public void RapidNextWaveReadyWaitsForDeathReleaseWithoutGrantingExtraIncome()
    {
        using Match match = Started(rules: new Rules { Campaign = CampaignFixture.Three(first: 1) }); Battle(match);
        UnitState enemy = Assert.Single(match.Enemies);
        match.Combat.Seed(enemy with
        { Health = 100, ReadyTick = 500, Hex = enemy.Hex! with { Action = UnitActionKind.Recovery, EndTick = 500 } });
        CombatFixture.Steps(match, 13);
        Assert.Equal(Phase.Building, match.Phase); Assert.Empty(match.Enemies);
        UnitState corpse = Assert.Single(match.Snapshot().DyingBodies); Assert.Equal(61, corpse.Hex!.DeathEndTick);
        Ready(match); Ready(match); Ready(match); Assert.Equal(Phase.Preparation, match.Phase);
        CityState prepared = match.Players[1].Snapshot(); Act(match, 1, "ready");
        Assert.Equal(Phase.Preparation, match.Phase); Assert.True(match.Players[1].Ready);
        Act(match, 1, "pause"); CombatFixture.Steps(match, 100); Assert.Equal(13, match.Tick);
        Act(match, 1, "resume"); CombatFixture.Steps(match, 47);
        Assert.Equal(60, match.Tick); Assert.Equal(Phase.Preparation, match.Phase); Assert.Single(match.Snapshot().DyingBodies);
        match.Step(); Assert.Equal(Phase.Combat, match.Phase); Assert.Equal(2, match.Wave); Assert.Empty(match.Snapshot().DyingBodies);
        Assert.Equal(6, match.ProductionCount); Assert.False(match.Players[1].Ready);
        CityState started = match.Players[1].Snapshot();
        Assert.Equal((prepared.Gold, prepared.Food, prepared.Wood), (started.Gold, started.Food, started.Wood));
        match.Step(); Assert.Equal(2, match.Wave); Assert.Equal(6, match.ProductionCount);
    }
    [Fact]
    public void TransfersConserveHealthCooldownAndSplitFiveThreeTwo()
    {
        using Match m = Started(3, new Rules { DefenderDamage = 0, Campaign = CampaignFixture.Three(first: 5), MeleeWindupTicks = 1 }); Battle(m);
        foreach (UnitState enemy in m.Enemies.Where(e => e.Destination != 1)) m.Combat.Remove(enemy.Id); m.Players[1].Health = 2; m.SetConnected(3, false);
        int[] ids = m.Enemies.Select(e => e.Id).ToArray(); foreach (UnitState e in m.Enemies) CombatFixture.Change(m, e.Id, u => u with { Health = 7 }); CombatFixture.AtCityEdge(m);
        CombatFixture.Steps(m, 2); Assert.True(m.Players[1].Eliminated); Assert.Equal(3, m.Enemies.Count(e => e.Destination == 2)); Assert.Equal(2, m.Enemies.Count(e => e.Destination == 3));
        Assert.Equal(ids, m.Enemies.Select(e => e.Id)); Assert.All(m.Enemies, e =>
        {
            Assert.Equal(7, e.Health); Assert.Equal(e.Profile.CadenceTicks - 1, e.Cooldown); Assert.True(e.Deployed); Assert.False(e.PendingImpact);
            Assert.Contains(e.Hex!.Position.Cell, m.Combat.Board.Front(Faction.Skeletons).Concat(m.Combat.Board.Rear(Faction.Skeletons)));
        });
        m.Players[2].Health = 2; CombatFixture.AtCityEdge(m, resetRecovery: true);
        CombatFixture.Steps(m, 2); Assert.All(m.Enemies, e => Assert.Equal(3, e.Destination)); Assert.Equal(5, m.Enemies.Count);
    }
    [Fact]
    public void SimultaneousEliminationsExcludeBothAndFutureBudgetsCountOnce()
    {
        using Match m = Started(3, new Rules { DefenderDamage = 0, MeleeWindupTicks = 1 }); Battle(m);
        m.Players[1].Health = m.Players[2].Health = 2; CombatFixture.AtCityEdge(m);
        CombatFixture.Steps(m, 2); Assert.All(m.Enemies, e => Assert.Equal(3, e.Destination)); Assert.Equal(12, m.Enemies.Count);
        CombatFixture.ClearEnemies(m); m.Step(); Battle(m); Assert.Equal(12, m.Enemies.Count); Assert.All(m.Enemies, e => Assert.Equal(3, e.Destination));
        Assert.Equal(4, m.Enemies.Count(e => e.Origin == 1));
        using var two = Started(3); Battle(two); two.Players[1].Health = 0; CombatFixture.ClearEnemies(two); two.Step(); Battle(two);
        Assert.Equal(6, two.Enemies.Count(e => e.Destination == 2)); Assert.Equal(6, two.Enemies.Count(e => e.Destination == 3));
    }
    [Fact]
    public void CompactFixturePreservesHealthNineTurnsAndStopsAtItsCatalogEnd()
    {
        using Match m = Started(rules: new Rules { Campaign = CampaignFixture.Three() }); m.Players[1].Food = 3; CombatFixture.Soldier(m, 900, 1, 7); m.Players[1].Health = 80;
        for (int wave = 1; wave <= 3; wave++)
        {
            Assert.Equal((wave - 1) * 5 + 1, m.TurnSerial); Battle(m); Assert.Equal(wave, m.Wave);
            Assert.Equal(7, m.Players[1].Soldiers[0].Health); CombatFixture.ClearEnemies(m); m.Step();
        }
        Assert.Equal(Phase.Victory, m.Phase); Assert.Equal(15, m.TurnSerial); Assert.Equal(9, m.ProductionCount); Assert.Equal(80, m.Players[1].Health);
        MatchSnapshot outcome = m.Snapshot(); m.Step(); Assert.Equal(outcome.Tick, m.Tick); Assert.False(m.Apply(1, Cmd(m, 1, "ready")).Accepted);
        using var defeat = Started(new Rules { SoldierHealth = 10, DefenderDamage = 10, Campaign = CampaignFixture.Three(first: 1), MeleeWindupTicks = 1, Combat = new() { Defender = new(1, 59) } });
        Battle(defeat); defeat.Players[1].Health = 2;
        CombatFixture.AtCityEdge(defeat); defeat.Combat.Seed(defeat.Enemies[0] with
        {
            TargetId = 1,
            TargetCity = true,
            PendingImpact = true,
            ImpactTick = 2,
            ReadyTick = 60,
            AttackSequence = 1,
            Hex = defeat.Enemies[0].Hex! with { Action = UnitActionKind.Windup, ActionSequence = 1, StartTick = 0, EndTick = 60 }
        }); defeat.Step(); defeat.Step();
        Assert.Empty(defeat.Enemies); Assert.Equal(Phase.Defeat, defeat.Phase);
    }
    private static Match Started(Rules rules) => Started(1, rules);
    [Fact]
    public void PauseFreezesAllTimersAndConnectionsStillChangeRevision()
    {
        using Match m = Started(2); Battle(m); m.Step(); Act(m, 1, "pause");
        string frozen = JsonSerializer.Serialize(m.Snapshot()); for (int i = 0; i < 3600; i++) m.Step(); Assert.Equal(frozen, JsonSerializer.Serialize(m.Snapshot()));
        Assert.False(m.Apply(1, Cmd(m, 1, "ready")).Accepted); Assert.False(m.Apply(1, Cmd(m, 1, "recruit", 1)).Accepted);
        long tick = m.Tick; long revision = m.Revision; m.SetConnected(2, false); Assert.Equal(tick, m.Tick); Assert.True(m.Revision > revision);
        Act(m, 1, "resume"); m.Step(); Assert.Equal(tick + 1, m.Tick);
        using var building = Started(2); Act(building, 1, "pause"); Assert.False(building.Apply(2, Cmd(building, 2, "ready")).Accepted);
    }
    [Fact]
    public void DisconnectDuringPauseResolvesExistingReadyCheckOnlyOnResume()
    {
        using Match m = Started(2);
        Act(m, 1, "ready"); Act(m, 2, "pause"); m.SetConnected(2, false);
        Assert.Equal(1, m.Turn); Assert.Equal(60, m.Players[1].Gold);
        Act(m, 1, "resume");
        Assert.Equal(2, m.Turn); Assert.Equal(70, m.Players[1].Gold); Assert.False(m.Players[1].Ready);
        Assert.Equal(70, m.Players[2].Gold);
    }
    [Fact]
    public void OrdinaryPaidOpeningFieldsSixFedSoldiersAndClearsFirstWave()
    {
        using Match m = Started(seed: 1);
        Act(m, 1, "build", 0, Building.Farm); Act(m, 1, "build", 2, Building.MetalMine); Act(m, 1, "build", 1, Building.Barracks);
        Ready(m); Ready(m); Ready(m);
        while (m.Players[1].Resources.TryPay(m.Economy.Recruitment(UnitType.Swordsman), out _)) Act(m, 1, "recruit", 1);
        Assert.Equal(6, m.Players[1].Soldiers.Count); Assert.Equal(15, m.Players[1].Food);
        Ready(m); Assert.Equal(9, m.Players[1].Food); Assert.All(m.Players[1].Soldiers, u => Assert.True(u.Participating));
        int steps = 0; while (m.Phase == Phase.Combat && steps++ < 6000) m.Step();
        Assert.Equal(Phase.Building, m.Phase); Assert.Equal(2, m.Wave); Assert.False(m.Players[1].Eliminated);
        // The four strategy families separately exercise the complete twenty-wave catalog.
    }
}
