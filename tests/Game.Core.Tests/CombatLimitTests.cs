using System.Text.Json;
using Xunit;

namespace Game.Core.Tests;

public sealed class CombatLimitTests
{
    private static Match Start(int players = 1, int waveLimit = 1000, bool progress = false)
    {
        var rules = new Rules
        {
            Campaign = CampaignFixture.Three(first: 1),
            DefenderDamage = 0,
            SoldierHealth = 100,
            SoldierDamage = progress ? 1 : 0,
            Arena = new() { SwordsmanStructureDamage = 0 },
            // Fast numerical limit fixture, not relaxed production validation:
            // configured approach/action/cleanup is genuinely below 500 ticks.
            Combat = new()
            {
                NoHealthProgressTicks = 500,
                MaximumWaveTicks = waveLimit,
                Swordsman = new(10, 1, 48),
                Berserker = new(20, 1, 48),
                Crossbowman = new(30, 1, 48),
                Mage = new(40, 1, 48)
            }
        };
        var match = new Match(rules, combatSeed: 123);
        for (int n = 0; n < players; n++) match.Join();
        VillageStrategyTests.Act(match, 1, "start");
        if (progress)
        {
            match.Combat.Create(UnitType.Swordsman, players, players, players, assignment: new(-1, 0, 0, 1));
            match.Players[players].Food = 1;
        }
        for (int n = 0; n < 3; n++)
            foreach (City city in match.Players.Values) Assert.True(VillageStrategyTests.Act(match, city.Id, "ready").Accepted);
        if (progress)
        {
            UnitState first = match.Enemies.Single(e => e.Destination == 1);
            match.Combat.Seed(first with { Profile = first.Profile with { Damage = 0 } });
        }
        return match;
    }
    [Theory]
    [InlineData(1000, 500, BattleLimit.NoHealthProgress)]
    [InlineData(200, 200, BattleLimit.WaveDuration)]
    public void MovementAndZeroDamageDoNotResetExactLimit(int waveLimit, int deadline, BattleLimit expected)
    {
        using Match match = Start(waveLimit: waveLimit);
        int cityHealth = match.Players[1].Health, unitHealth = match.Enemies.Sum(e => e.Health);
        CombatFixture.Steps(match, deadline - 1); Assert.Equal(Phase.Combat, match.Phase);
        match.Step(); Assert.Equal(Phase.Defeat, match.Phase); Assert.Equal(deadline, match.Tick);
        Assert.Equal(DefeatReason.BattleStalled, match.DefeatReason); Assert.NotNull(match.Stall); Assert.Equal(expected, match.Stall.Limit);
        Assert.Equal(cityHealth, match.Players[1].Health); Assert.Equal(unitHealth, match.Enemies.Sum(e => e.Health));
        Assert.False(match.Players[1].Eliminated); Assert.All(match.Enemies, u => Assert.False(u.PendingImpact));
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SharedHealthProgressExtendsAllCurrentSectorPressureDeadlines(bool fallenSector)
    {
        using Match match = Start(fallenSector ? 3 : 2, progress: true);
        if (fallenSector) match.Players[2].Health = 0;
        CombatFixture.Steps(match, 499);
        Assert.Equal(Phase.Combat, match.Phase);
        EngagementProgress other = match.Snapshot().Engagements.Single(e => e.City == (fallenSector ? 1 : 2));
        if (fallenSector) Assert.DoesNotContain(match.Snapshot().Engagements, e => e.City == 2);
        Assert.True(other.LastHealthProgressTick > 0); Assert.True(other.Deadline > 500);
        match.Step(); Assert.Equal(Phase.Combat, match.Phase); Assert.Null(match.Stall); Assert.Equal(500, match.Tick);
        Assert.All(match.Snapshot().Engagements, e => Assert.True(e.Deadline > 500));
    }
    [Fact]
    public void PauseFreezesTheDeadlineWithoutCatchup()
    {
        using Match match = Start(); CombatFixture.Steps(match, 20); VillageStrategyTests.Act(match, 1, "pause");
        string before = JsonSerializer.Serialize(match.Snapshot()); CombatFixture.Steps(match, 1000);
        Assert.Equal(before, JsonSerializer.Serialize(match.Snapshot()));
        VillageStrategyTests.Act(match, 1, "resume"); CombatFixture.Steps(match, 479); Assert.Equal(Phase.Combat, match.Phase);
        match.Step(); Assert.Equal(500, match.Tick); Assert.Equal(DefeatReason.BattleStalled, match.DefeatReason);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NormalResultsPrecedeLimitsOnTheSameTick(bool fallen)
    {
        using Match match = Start(); CombatFixture.Steps(match, 499); CombatFixture.ClearEnemies(match);
        if (fallen) match.Players[1].Health = 0;
        match.Step(); Assert.Equal(fallen ? Phase.Defeat : Phase.Building, match.Phase);
        Assert.Equal(fallen ? DefeatReason.AllCitiesFallen : DefeatReason.None, match.DefeatReason); Assert.Null(match.Stall);
    }
    [Theory]
    [InlineData("wave")]
    [InlineData("progress")]
    [InlineData("keeps")]
    public void TerminalPendingMeleeCapturesItsMovingPrimaryAtTheExactStopTick(string reason)
    {
        using var match = new Match(new Rules
        {
            Campaign = CampaignFixture.Three(first: 1),
            DefenderDamage = 0,
            SoldierDamage = 0,
            Arena = new() { SwordsmanStructureDamage = 0 },
            Combat = new() { MaximumWaveTicks = reason == "wave" ? 10 : 18000 }
        }, combatSeed: 1);
        match.Join(); VillageStrategyTests.Act(match, 1, "start");
        int actor = match.Combat.Create(UnitType.Swordsman, 1, 1, 1, assignment: new(-1, 0, 0, 1));
        match.Players[1].Food = 1;
        for (int gate = 0; gate < 3; gate++) Assert.True(VillageStrategyTests.Act(match, 1, "ready").Accepted);
        int stop = reason == "progress" ? match.Configuration.NoHealthProgressTicks : 10;
        CombatFixture.Steps(match, stop - 10);
        int target = Assert.Single(match.Enemies).Id;
        HexTransition move = match.Combat.Board.Transition(new(18, 1), new(11, 1));
        match.Combat.Seed(match.Combat.Inspect(actor) with { Location = new(UnitLifecycle.Alive, new(17, 1)), Action = new CombatAction.Waiting() });
        match.Combat.Seed(match.Combat.Inspect(target) with
        {
            Location = new(UnitLifecycle.Alive, move.Source),
            Action = new CombatAction.Moving(1, move.Destination, move.Id, stop - 10, stop + 20)
        });
        match.Combat.StartActions(match.Players.Values);
        CombatFixture.Steps(match, 9);
        UnitState before = match.Combat.Read(actor);
        Assert.True(before.PendingImpact); Assert.Equal(target, before.TargetId); Assert.Null(before.Hex!.FrozenAim);
        Assert.True(before.ImpactTick > stop);
        if (reason == "keeps") match.Players[1].Health = 0;
        match.Step();
        MatchSnapshot received = Game.SnapshotPayload.DecodeForReceiving(Game.SnapshotPayload.Encode(match.Snapshot()));
        Assert.Equal(Phase.Defeat, received.Phase); Assert.Equal(stop, received.Tick);
        if (reason == "keeps") Assert.Equal(DefeatReason.AllCitiesFallen, received.DefeatReason);
        else Assert.Equal(reason == "wave" ? BattleLimit.WaveDuration : BattleLimit.NoHealthProgress, received.Stall!.Limit);
        UnitState frozen = Assert.Single(received.Players[0].Soldiers);
        HexPosePoint expected = new(move.Source, move.Destination, move.Id, 10, 30);
        Assert.Equal(expected, frozen.Hex!.FrozenAim); Assert.Equal(stop, frozen.Hex.FrozenTick);
        Assert.Equal((before.TargetId, before.AttackSequence, before.ActionStartTick, before.ImpactTick, before.ReadyTick, before.Health),
            (frozen.TargetId, frozen.AttackSequence, frozen.ActionStartTick, frozen.ImpactTick, frozen.ReadyTick, frozen.Health));
        Assert.Equal(before.Hex with { FrozenTick = stop, FrozenAim = expected }, frozen.Hex);
        Assert.False(frozen.PendingImpact);
        match.Combat.Seed(match.Combat.Inspect(target) with { Location = new(UnitLifecycle.Alive, new(25, 1)), Action = new CombatAction.Waiting() });
        match.Combat.StopActions(match.Players.Values);
        Assert.Equal(expected, match.Combat.Read(actor).Hex!.FrozenAim);
        Assert.Equal(target, match.Combat.Read(actor).TargetId);
        MatchSnapshot later = Game.SnapshotPayload.DecodeForReceiving(Game.SnapshotPayload.Encode(match.Snapshot()));
        Assert.Equal(frozen.Hex, Assert.Single(later.Players[0].Soldiers).Hex);
    }
    [Fact]
    public void TerminalMovePoseRemainsFrozenWhileRetainedDeathsCleanUp()
    {
        using Match match = Start(waveLimit: 10);
        CombatUnit moving = match.Combat.Inspect(match.Enemies.Single().Id);
        match.Combat.Seed(moving with
        {
            Action = CombatActions.Move(moving.Action, match.Combat.Board.Transition(moving.Location.Position, new(2, 1)), 0, 30)
        });
        int body = match.Combat.Create(UnitType.Crossbowman, 0, 1, 1, Faction.Skeletons);
        UnitState corpse = match.Combat.Read(body);
        match.Combat.Seed(corpse with
        {
            Health = 0,
            Deployed = true,
            Hex = CombatFixture.At(corpse, 1, 1) with
            { Lifecycle = UnitLifecycle.Dying, DeathStartTick = 0, DeathEndTick = 48 }
        });
        CombatFixture.Steps(match, 10); Assert.Equal(DefeatReason.BattleStalled, match.DefeatReason);
        UnitState frozen = Assert.Single(match.Enemies);
        Assert.Equal(UnitActionKind.Moving, frozen.Hex!.Action); Assert.Equal(10, frozen.Hex.FrozenTick);
        CombatFixture.Steps(match, 38); Assert.Empty(match.Snapshot().DyingBodies);
        UnitState current = Assert.Single(match.Enemies);
        Assert.Equal(frozen.Hex.FrozenTick, current.Hex!.FrozenTick); Assert.Equal(frozen.Hex, current.Hex);
        Assert.Equal(frozen.Hex.Position, current.Hex!.Position); Assert.Equal(frozen.Hex.Destination, current.Hex.Destination);
    }
}
