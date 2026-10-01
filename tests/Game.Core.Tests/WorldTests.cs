using System.Text.Json;
using Game.Core;
using Xunit;

namespace Game.Core.Tests;

public sealed class WorldTests
{
    private static Command Cmd(Match m, int id, string action, int slot = -1, Building building = Building.Empty, long seq = 1) => new(seq, m.Id, m.Phase, m.TurnSerial, action, id, slot, building);
    private static void Act(Match m, int id, string action, int slot = -1, Building building = Building.Empty) => Assert.True(m.Apply(id, Cmd(m, id, action, slot, building)).Accepted);
    private static Match Started(int count = 1, Rules? rules = null)
    {
        var m = new Match(rules);
        for (int i = 0; i < count; i++) Assert.NotNull(m.Join());
        Act(m, 1, "start"); return m;
    }
    private static void Ready(Match m)
    {
        foreach (City c in m.Players.Values.Where(c => !c.Eliminated && c.Connected)) Act(m, c.Id, "ready");
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
        using Match m = Started(rules: new Rules { StartingWood = 40 }); Act(m, 1, "build", 0, Building.Farm); Act(m, 1, "upgrade", 0); Act(m, 1, "build", 1, Building.Barracks);
        Assert.False(m.Apply(1, Cmd(m, 1, "upgrade", 0)).Accepted);
        Assert.False(m.Apply(1, Cmd(m, 1, "upgrade", 1)).Accepted);
        Assert.False(m.Apply(1, Cmd(m, 1, "recruit", 1)).Accepted);
        Ready(m); Assert.Equal(8, m.Players[1].Food); Assert.Empty(m.Players[1].Soldiers);
        Assert.False(m.Apply(1, Cmd(m, 1, "recruit", 0)).Accepted);
        Act(m, 1, "recruit", 1); Assert.Single(m.Players[1].Soldiers); Assert.Equal(3, m.Players[1].Food);
        Ready(m); Act(m, 1, "upgrade", 1); int food = m.Players[1].Food;
        Act(m, 1, "recruit", 1); Assert.Equal(food - 4, m.Players[1].Food); Assert.Equal(1000, m.Players[1].Soldiers[0].Health);
        using var n = Started(); Act(n, 1, "build", 0, Building.Mine); Act(n, 1, "upgrade", 0); Ready(n); Assert.Equal(40, n.Players[1].Gold);
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
        a.Step(); b.Step(); Assert.True(a.Enemies.Sum(e => e.Health) < health); Assert.False(a.Players[1].Eliminated);
        for (int i = 0; i < 600; i++)
        {
            a.Step(); b.Step(); Assert.Equal(a.Snapshot().Enemies, b.Snapshot().Enemies);
            Assert.All(a.Enemies, e => Assert.InRange(e.Position, 0, Match.LaneLength));
        }
        Assert.True(a.Enemies.Sum(e => e.Health) < health);
        Assert.True(a.Enemies.Sum(e => e.Health) >= health - 1200); // One low damage shot per second.
    }
    [Fact]
    public void ArmyDeathDoesNotEliminateCityAndDamageIsSimultaneous()
    {
        using Match m = Started(2, new Rules { DefenderDamage = 0, SoldierDamage = 10, MeleeWindupTicks = 1 }); Battle(m);
        CombatFixture.Soldier(m, 1000, 1, 2, 0.25);
        CombatFixture.Change(m, m.Enemies[0].Id, e => e with { Position = 0.80, Lateral = 0 });
        CombatFixture.Steps(m, 2);
        Assert.Empty(m.Players[1].Soldiers); Assert.DoesNotContain(m.Enemies, e => e.Id == 1); Assert.False(m.Players[1].Eliminated);
    }
    [Fact]
    public void TransfersConserveHealthCooldownAndSplitFiveThreeTwo()
    {
        using Match m = Started(3, new Rules { DefenderDamage = 0, WaveOne = 5, MeleeWindupTicks = 1 }); Battle(m);
        foreach (UnitState enemy in m.Enemies.Where(e => e.Destination != 1)) m.Combat.Remove(enemy.Id); m.Players[1].Health = 2; m.SetConnected(3, false);
        int[] ids = m.Enemies.Select(e => e.Id).ToArray(); foreach (UnitState e in m.Enemies) CombatFixture.Change(m, e.Id, u => u with { Health = 7 }); CombatFixture.AtCityEdge(m);
        CombatFixture.Steps(m, 2); Assert.True(m.Players[1].Eliminated); Assert.Equal(3, m.Enemies.Count(e => e.Destination == 2)); Assert.Equal(2, m.Enemies.Count(e => e.Destination == 3));
        Assert.Equal(ids, m.Enemies.Select(e => e.Id)); Assert.All(m.Enemies, e => { Assert.Equal(7, e.Health); Assert.Equal(e.Profile.CadenceTicks - 1, e.Cooldown); Assert.Equal(Match.LaneLength, e.Position); Assert.False(e.Deployed); });
        m.Players[2].Health = 2; CombatFixture.AtCityEdge(m, resetRecovery: true);
        CombatFixture.Steps(m, 2); Assert.All(m.Enemies, e => Assert.Equal(3, e.Destination)); Assert.Equal(5, m.Enemies.Count);
    }
    [Fact]
    public void SimultaneousEliminationsExcludeBothAndFutureBudgetsCountOnce()
    {
        using Match m = Started(3, new Rules { DefenderDamage = 0, MeleeWindupTicks = 1 }); Battle(m);
        m.Players[1].Health = m.Players[2].Health = 2; CombatFixture.AtCityEdge(m);
        CombatFixture.Steps(m, 2); Assert.All(m.Enemies, e => Assert.Equal(3, e.Destination)); Assert.Equal(12, m.Enemies.Count);
        CombatFixture.ClearEnemies(m); m.Step(); Battle(m); Assert.Equal(18, m.Enemies.Count); Assert.All(m.Enemies, e => Assert.Equal(3, e.Destination));
        Assert.Equal(6, m.Enemies.Count(e => e.Origin == 1));
        using var two = Started(3); Battle(two); two.Players[1].Health = 0; CombatFixture.ClearEnemies(two); two.Step(); Battle(two);
        Assert.Equal(9, two.Enemies.Count(e => e.Destination == 2)); Assert.Equal(9, two.Enemies.Count(e => e.Destination == 3));
    }
    [Fact]
    public void PersistentHealthNineTurnsAndNoFourthWave()
    {
        using Match m = Started(); CombatFixture.Soldier(m, 900, 1, 7, 0.25); m.Players[1].Health = 80;
        for (int wave = 1; wave <= 3; wave++)
        {
            Assert.Equal((wave - 1) * 5 + 1, m.TurnSerial); Battle(m); Assert.Equal(wave, m.Wave);
            Assert.Equal(7, m.Players[1].Soldiers[0].Health); CombatFixture.ClearEnemies(m); m.Step();
        }
        Assert.Equal(Phase.Victory, m.Phase); Assert.Equal(15, m.TurnSerial); Assert.Equal(9, m.ProductionCount); Assert.Equal(80, m.Players[1].Health);
        MatchSnapshot outcome = m.Snapshot(); m.Step(); Assert.Equal(outcome.Tick, m.Tick); Assert.False(m.Apply(1, Cmd(m, 1, "ready")).Accepted);
        using var defeat = Started(new Rules { DefenderDamage = 10, WaveOne = 1, MeleeWindupTicks = 1 });
        Battle(defeat); defeat.Players[1].Health = 2;
        CombatFixture.AtCityEdge(defeat); defeat.Combat.Seed(defeat.Enemies[0] with
        { TargetId = 1, TargetCity = true, PendingImpact = true, ImpactTick = 1, ReadyTick = 60, AttackSequence = 1 }); defeat.Step();
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
    public void StandardWinningEconomyCompletesThreeWaves()
    {
        using Match m = Started(); Act(m, 1, "build", 0, Building.Farm); Act(m, 1, "upgrade", 0); Act(m, 1, "build", 1, Building.Barracks);
        for (int wave = 1; wave <= 3; wave++)
        {
            for (int turn = 1; turn <= 3; turn++)
            {
                while (m.Players[1].Food >= m.Rules.RecruitCost) Act(m, 1, "recruit", 1);
                Ready(m);
            }
            while (m.Players[1].Food >= m.Rules.RecruitCost) Act(m, 1, "recruit", 1);
            Ready(m);
            int steps = 0; while (m.Phase == Phase.Combat && steps++ < 6000) m.Step();
            Assert.NotEqual(Phase.Combat, m.Phase); Assert.False(m.Players[1].Eliminated);
        }
        Assert.Equal(Phase.Victory, m.Phase);
    }
}
