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
            Combat = new() { NoHealthProgressTicks = 500, MaximumWaveTicks = waveLimit }
        };
        var match = new Match(rules, combatSeed: 123);
        for (int n = 0; n < players; n++) match.Join();
        VillageStrategyTests.Act(match, 1, "start");
        if (progress) match.Combat.Create(UnitType.Swordsman, players, players, players);
        for (int n = 0; n < 4; n++)
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
    [Fact]
    public void HealthProgressInAnotherCityDoesNotExtendTheStuckCityDeadline()
    {
        using Match match = Start(2, progress: true);
        CombatFixture.Steps(match, 499);
        Assert.Equal(Phase.Combat, match.Phase);
        EngagementProgress other = match.Snapshot().Engagements.Single(e => e.City == 2);
        Assert.True(other.LastHealthProgressTick > 0); Assert.True(other.Deadline > 500);
        match.Step(); Assert.Equal(DefeatReason.BattleStalled, match.DefeatReason); Assert.Equal(1, match.Stall!.City); Assert.Equal(500, match.Tick);
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
    [Fact]
    public void TerminalMovePoseRemainsFrozenWhileRetainedDeathsCleanUp()
    {
        using Match match = Start(waveLimit: 10);
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
