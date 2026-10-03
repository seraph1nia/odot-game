using System.Text.Json;
using Xunit;

namespace Game.Core.Tests;

public sealed class StatusEffectTests
{
    private static readonly StatusRules Rules = new();
    private static StatusApplication Application(StatusKind kind, long tick, int strength = 100, int source = 1, long attack = 1) => new(2, kind, source, attack, tick, strength);
    [Fact]
    public void BurnRefreshPreservesPendingTickAndTerminalDamage()
    {
        StatusState state = StatusPolicy.Apply(new(), Application(StatusKind.Burn, 0), Rules);
        state = StatusPolicy.Apply(state, Application(StatusKind.Burn, 10, 200, 3, 2), Rules);
        Assert.Equal(60, state.Burn!.NextDamageTick); Assert.Equal(190, state.Burn.ExpiresTick); Assert.Equal(200, state.Burn.Strength);
        Assert.Empty(StatusPolicy.Advance(state, 59, Rules).Damage);
        var due = StatusPolicy.Advance(state, 60, Rules); Assert.Equal(200, Assert.Single(due.Damage).Damage); Assert.Equal(120, due.State.Burn!.NextDamageTick);
        state = StatusPolicy.Apply(new(), Application(StatusKind.Burn, 0), Rules);
        for (int tick = 60; tick <= 180; tick += 60) { due = StatusPolicy.Advance(state, tick, Rules); Assert.Single(due.Damage); state = due.State; }
        Assert.Null(state.Burn); Assert.Empty(StatusPolicy.Advance(state, 181, Rules).Damage);
        Assert.Equal(new StatusState(), StatusPolicy.Apply(new(), Application(StatusKind.Burn, 0, 0), Rules));
    }
    [Fact]
    public void PoisonFourthRefreshKeepsIndependentDeadlinesAndCanonicalIdentity()
    {
        StatusState Build(IEnumerable<StatusApplication> input) => input.OrderBy(a => a.VictimId).ThenBy(a => a.Kind).ThenBy(a => a.SourceId).ThenBy(a => a.AttackSequence)
            .Aggregate(new StatusState(), (s, a) => StatusPolicy.Apply(s, a, Rules));
        StatusApplication[] hits = Enumerable.Range(1, 4).Select(n => Application(StatusKind.Poison, 0, n * 100, n)).ToArray();
        StatusState forward = Build(hits), reverse = Build(hits.Reverse()); Assert.Equal(forward, reverse); Assert.Equal(3, forward.Poison.Length);
        Assert.Equal(1, forward.Poison[0].Identity.SourceId); Assert.Equal(400, forward.Poison[0].Strength); Assert.All(forward.Poison, p => Assert.Equal(60, p.NextDamageTick));
        StatusState state = new();
        for (int n = 0; n < 3; n++) state = StatusPolicy.Apply(state, Application(StatusKind.Poison, n * 10, 100, source: n + 1), Rules);
        for (int tick = 1; tick <= 360; tick++) state = StatusPolicy.Advance(state, tick, Rules).State;
        Assert.Equal(2, state.Poison.Length); Assert.Equal(370, state.Poison[0].ExpiresTick); Assert.Equal(380, state.Poison[1].ExpiresTick);
        StatusEffect[] detached = state.Detach().Poison; detached[0] = detached[0] with { Strength = 999 }; Assert.Equal(100, state.Poison[0].Strength);
    }
    [Fact]
    public void ChillIsStrongestOnlyExpiresAtBoundaryAndRoundsPositiveDurations()
    {
        StatusState state = StatusPolicy.Apply(new(), Application(StatusKind.Chill, 0, 20), Rules);
        state = StatusPolicy.Apply(state, Application(StatusKind.Chill, 5, 40), Rules); state = StatusPolicy.Apply(state, Application(StatusKind.Chill, 10, 20), Rules);
        Assert.Equal(40, state.ChillAt(189)); Assert.Equal(0, state.ChillAt(190)); Assert.Equal(190, state.Chill!.ExpiresTick);
        Assert.Equal(2, StatusPolicy.Duration(1, 20)); Assert.Equal(42, StatusPolicy.Duration(30, 40));
        Assert.Equal(1, StatusPolicy.Potency(1, 10)); Assert.Equal(0, StatusPolicy.Potency(0, 30));
        Assert.Throws<OverflowException>(() => StatusPolicy.Duration(int.MaxValue, 50));
        Assert.Throws<ArgumentException>(() => new StatusRules { BurnPeriod = 0 }.Validate());
        Assert.Throws<ArgumentException>(() => new StatusRules { PoisonCap = 4 }.Validate());
        Assert.Throws<ArgumentException>(() => StatusPolicy.Validate(new() { Poison = Enumerable.Repeat(new StatusEffect(new(0, 1, 1), 100, 0, 360, 60), 4).ToArray() }, Rules));
    }
    [Fact]
    public void PeriodicLethalDamageStillAllowsDueImpactThenClearsCasualtyScheduling()
    {
        using var combat = new CombatSimulation(new Rules { DefenderDamage = 0 });
        int ally = combat.Create(UnitType.Swordsman, 1, 1, 1), enemy = combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons);
        Place(combat, ally, 11, 1); Place(combat, enemy, 8, 1);
        CombatUnit attacker = combat.Inspect(ally), target = combat.Inspect(enemy);
        combat.Seed(attacker with { Health = 100, Statuses = StatusPolicy.Apply(new(), Application(StatusKind.Poison, 0), Rules), Action = CombatActions.Attack(attacker.Action, new(CombatTargetKind.Unit, enemy), 0, 60, 30) });
        combat.Advance(60, []); Assert.Equal(target.Health - attacker.Profile.Damage, combat.Read(enemy).Health);
        Assert.Equal(UnitLifecycle.Dying, combat.Read(ally).Hex!.Lifecycle); Assert.False(combat.Read(ally).Statuses.Active);
        Assert.Contains(combat.Events(), e => e.Type == CombatEventType.Death && e.Unit!.Id == ally && e.Unit.Statuses.Poison.Length == 1);
    }
    [Fact]
    public void LandedSplashAppliesOnlyToSurvivorsAndPoisonTransferTicksWhileQueued()
    {
        using var combat = new CombatSimulation(new Rules { DefenderDamage = 0 });
        int mage = combat.Create(UnitType.Mage, 1, 1, 1, capabilities: new(BurnPercent: 20)); Place(combat, mage, 11, 1);
        int[] enemies = Enumerable.Range(0, 3).Select(n => combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons)).ToArray();
        for (int n = 0; n < 3; n++) Place(combat, enemies[n], 8, n + 1);
        CombatUnit actor = combat.Inspect(mage); combat.Seed(actor with { Action = CombatActions.Attack(actor.Action, new(CombatTargetKind.Unit, enemies[0]), 0, 1, 30) });
        combat.Advance(1, []); Assert.Equal(2, combat.Enemies().Count(u => u.Statuses.Burn is not null));
        int afflicted = combat.Enemies().First(u => u.Statuses.Burn is not null).Id; UnitState before = combat.Read(afflicted);
        combat.Transfer(afflicted, 2); Assert.False(combat.Read(afflicted).Deployed); Assert.Equal(before.Statuses, combat.Read(afflicted).Statuses);
        combat.Remove(mage); combat.Advance(61, []); Assert.Equal(before.Health - before.Statuses.Burn!.Strength, combat.Read(afflicted).Health);
        Assert.Equal(UnitLifecycle.Queued, combat.Read(afflicted).Hex!.Lifecycle); Assert.DoesNotContain(combat.Reservations.Positions, p => p.UnitId == afflicted);
        UnitState transported = JsonSerializer.Deserialize<UnitState>(JsonSerializer.Serialize(combat.Read(afflicted), WireJson.Options), WireJson.Options)!;
        using var restored = new CombatSimulation(new()); restored.Seed(transported); Assert.Equal(transported.Statuses, restored.Read(afflicted).Statuses);
    }
    [Fact]
    public void ChillCommitsIntervalsAndExpiryDoesNotChangeMovementReservations()
    {
        using var combat = new CombatSimulation(new()); int id = combat.Create(UnitType.Swordsman, 1, 1, 1); Place(combat, id, 11, 1);
        CombatUnit actor = combat.Inspect(id); StatusState chill = StatusPolicy.Apply(new(), Application(StatusKind.Chill, 0, 40), Rules);
        CombatAction windup = CombatActions.Attack(actor.Action, new(CombatTargetKind.City, 1), 170,
            StatusPolicy.Duration(actor.Profile.WindupTicks, chill.ChillAt(170)), StatusPolicy.Duration(actor.Profile.CadenceTicks - actor.Profile.WindupTicks, chill.ChillAt(170)));
        combat.Seed(actor with { Action = windup, Statuses = chill }); long impact = combat.Read(id).ImpactTick, ready = combat.Read(id).ReadyTick;
        combat.Advance(180, []); Assert.Null(combat.Read(id).Statuses.Chill); Assert.Equal(impact, combat.Read(id).ImpactTick); Assert.Equal(ready, combat.Read(id).ReadyTick);
        Assert.Equal(0, combat.Read(id).Statuses.ChillAt(180)); Assert.Equal(actor.Profile.MoveTicks, StatusPolicy.Duration(actor.Profile.MoveTicks, 0));
    }
    private static void Place(CombatSimulation combat, int id, int cell, int anchor)
    { UnitState state = combat.Read(id); combat.Seed(state with { Hex = CombatFixture.At(state, cell, anchor) }); }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 10)]
    public void ZeroDamageAndInvalidPrimaryMissCannotApplyAnyStatusOrSplash(bool removePrimary, int damage)
    {
        using var combat = new CombatSimulation(new Rules { MageDamage = damage, DefenderDamage = 0 });
        int mage = combat.Create(UnitType.Mage, 1, 1, 1, capabilities: new(BurnPercent: 20, PoisonPercent: 10, ChillPercent: 20));
        int primary = combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons), secondary = combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons);
        Place(combat, mage, 11, 1); Place(combat, primary, 8, 1); Place(combat, secondary, 8, 2);
        CombatUnit actor = combat.Inspect(mage);
        combat.Seed(actor with { Action = CombatActions.Attack(actor.Action, new(CombatTargetKind.Unit, primary), 0, 1, 30) });
        int original = combat.Read(secondary).Health;
        if (removePrimary) combat.Remove(primary);
        combat.Advance(1, []);
        Assert.All(combat.Enemies(), u => Assert.False(u.Statuses.Active)); Assert.Equal(original, combat.Read(secondary).Health);
    }

    [Fact]
    public void GuardianRoundsEachSmallOrdinaryAndPeriodicContributionOnce()
    {
        using var combat = new CombatSimulation(new Rules { DefenderDamage = 0 });
        int ally = combat.Create(UnitType.Swordsman, 1, 1, 1, capabilities: new(ReductionPercent: 20)); Place(combat, ally, 11, 1);
        int[] enemies = Enumerable.Range(0, 2).Select(_ => combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons)).ToArray();
        for (int n = 0; n < enemies.Length; n++)
        {
            Place(combat, enemies[n], 8, n + 1); CombatUnit actor = combat.Inspect(enemies[n]);
            combat.Seed(actor with { Profile = actor.Profile with { Damage = 3 }, Action = CombatActions.Attack(actor.Action, new(CombatTargetKind.Unit, ally), 0, 60, 30) });
        }
        CombatUnit target = combat.Inspect(ally); combat.Seed(target with { Statuses = StatusPolicy.Apply(new(), new(ally, StatusKind.Burn, enemies[0], 1, 0, 3), Rules) });
        combat.Advance(60, []);
        Assert.Equal(target.Health - 9, combat.Read(ally).Health);
        Assert.Equal(3, Assert.Single(combat.Events().Single(e => e.Type == CombatEventType.Hit && e.Unit!.Id == ally).Periodic).Damage);
    }
}
