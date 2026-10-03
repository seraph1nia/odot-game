using Xunit;

namespace Game.Core.Tests;

public sealed class DefensePolicyTests
{
    [Fact]
    public void CompleteProfilesRetainDefaultTimingAndConfiguredSplash()
    {
        var defaults = new CombatConfiguration(new());
        Assert.Equal(new DefenseProfile(200, 12, 48, 1, 0), defaults.BuiltInDefense);
        Assert.Equal(5, defaults.RulesVersion);
        var custom = new CombatConfiguration(new() { Combat = new() { Defender = new(7, 23, 3, 2) } });
        Assert.Equal(new DefenseProfile(200, 7, 23, 3, 2), custom.BuiltInDefense);
        Assert.NotEqual(defaults.Fingerprint, custom.Fingerprint);
        var defense = new DefenseActor(DefenseIdentity.Defender(1), new(1, -1, Building.Empty, 0), custom.BuiltInDefense);
        TowerState action = defense.Start(8, 100);
        Assert.Equal((100L, 107L, 130L, 1L), (action.ActionStartTick, action.ImpactTick, action.ReadyTick, action.AttackSequence));
        Assert.Throws<ArgumentException>(() => DefenseIdentity.Tower(1, -1));
    }

    [Theory]
    [InlineData(1, 1, false, 1)]
    [InlineData(2, 0, false, 1)]
    [InlineData(2, 1, false, 2)]
    [InlineData(2, 1, true, 0)]
    public void DefenderUsesCapRadiusAndLockedPrimaryWithoutFriendlyFire(int cap, int radius, bool removePrimary, int count)
    {
        using var match = new Match(new Rules { SoldierHealth = 10, Combat = new() { Defender = new(1, 1, cap, radius) } }, combatSeed: 123);
        City city = match.Join()!;
        int primary = CombatDecisionRegressionTests.At(match.Combat, Faction.Skeletons, 8);
        int secondary = CombatDecisionRegressionTests.At(match.Combat, Faction.Skeletons, 9);
        int friendly = CombatDecisionRegressionTests.At(match.Combat, Faction.Adventurers, 11);
        var defense = new DefenseActor(DefenseIdentity.Defender(city.Id), city.Defender, match.Configuration.BuiltInDefense);
        city.Defender = defense.Start(primary, 0);
        if (removePrimary) match.Combat.Remove(primary);
        match.Combat.Advance(1, [city]);
        CombatEvent impact = Assert.Single(match.Combat.Events(), e => e.Type == CombatEventType.Impact);
        Assert.Equal(count, impact.Victims.Length); Assert.Equal(count, impact.Victims.Distinct().Count());
        Assert.DoesNotContain(friendly, impact.Victims);
        Assert.Equal(1000, match.Combat.Read(friendly).Health);
        Assert.Equal(count == 2 ? 800 : 1000, match.Combat.Read(secondary).Health);
        match.Combat.Advance(2, [city]);
        Assert.Single(match.Combat.Events(), e => e.Type == CombatEventType.Impact);
    }

    [Fact]
    public void UnitDefenderAndTowerResolveOnceAgainstTheSamePreDamageView()
    {
        Rules rules = new() { Combat = new() { ArrowTower = new(12, 48) } };
        using var match = new Match(rules, combatSeed: 123);
        City city = match.Join()!;
        int soldier = CombatDecisionRegressionTests.At(match.Combat, Faction.Adventurers, 17);
        int enemy = CombatDecisionRegressionTests.At(match.Combat, Faction.Skeletons, 14);
        match.Combat.Seed(match.Combat.Inspect(soldier) with { Health = 100 });
        match.Combat.Seed(match.Combat.Inspect(enemy) with { Health = 500 });
        city.Towers[0] = new(city.Id, 0, Building.ArrowTower, 1);
        match.Combat.StartActions([city]);
        match.Combat.Advance(12, [city]);
        CombatEvent[] impacts = match.Combat.Events().Where(e => e.Type == CombatEventType.Impact).ToArray();
        Assert.Equal(4, impacts.Length); Assert.All(impacts, impact => Assert.True(impact.Landed));
        Assert.Equal(3, impacts.Count(impact => impact.Victims.SequenceEqual([enemy])));
        Assert.Equal(new[] { soldier, enemy }, match.Combat.Dying().Select(u => u.Id));
        Assert.Equal(500, Assert.Single(match.Combat.Events(), e => e.Type == CombatEventType.Hit && e.Unit!.Id == enemy).Damage);
        match.Combat.Advance(13, [city]);
        Assert.Equal(4, match.Combat.Events().Count(e => e.Type == CombatEventType.Impact));
        city.Health = 0;
        match.Combat.StartActions([city]);
        Assert.False(city.Defender.PendingImpact); Assert.False(city.Towers[0].PendingImpact);
    }
}
