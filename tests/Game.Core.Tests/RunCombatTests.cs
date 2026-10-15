using Xunit;

namespace Game.Core.Tests;

// Tier A production combat boundaries. Controlled poses/health/committed actions
// isolate timing and attribution, not earned balance. No alternate simulator,
// source-string checks, processes or global state. A few exact impact ticks.
public sealed class RunCombatTests
{
    private static RunState Run(params string[] cards) => new() { Started = true, Cards = cards.GroupBy(id => id).Select(g => new AcquiredCard(g.Key, g.Count())).ToArray() };
    private static int[] Group(Match m, UnitType type, int count, int owner, int cell, UnitCapabilities capabilities = default)
    {
        Faction faction = owner == 0 ? Faction.Skeletons : Faction.Adventurers;
        int[] ids = Enumerable.Range(0, count).Select(_ => m.Combat.Create(type, owner, 1, 1, faction, capabilities: capabilities,
            assignment: owner == 0 ? null : new(-1, 0, owner - 1, 1))).ToArray();
        foreach (int id in ids) m.Combat.Seed(m.Combat.Inspect(id) with { Location = new(UnitLifecycle.Alive, new(cell, 1)) });
        return ids;
    }
    [Fact]
    public void WarhornIsOneCommittedSquadActionNotOneBonusForEachMembersLaterCycle()
    {
        using var m = new Match(new() { EnemyPowerPercent = 100, SoldierHealth = 500, DefenderDamage = 0 }, combatSeed: 1);
        City city = m.Join()!; city.Run = Run("R01");
        int[] attackers = Group(m, UnitType.Swordsman, 3, 1, 17); Group(m, UnitType.Swordsman, 3, 0, 18);
        m.Combat.StartActions([city]); Assert.Single(city.Run.FirstAttackSquads);
        m.Combat.Advance(12, [city]);
        Assert.Equal(3, m.Combat.Events().Count(e => e.Type == CombatEventType.Impact && e.Unit?.Owner == 1 && e.Damage == 1250));
        Assert.Empty(city.Run.WarhornSquads);
        m.Combat.Advance(60, [city]); m.Combat.StartActions([city]); m.Combat.Advance(72, [city]);
        Assert.Equal(3, m.Combat.Events().Count(e => e.Tick == 72 && e.Type == CombatEventType.Impact && e.Unit?.Owner == 1 && e.Damage == 1000));
        Assert.All(attackers, id => Assert.Equal(2, m.Combat.Read(id).AttackSequence));
        m.Combat.BeginWave(2, [], [city]); Assert.Empty(city.Run.FirstAttackSquads); Assert.Empty(city.Run.WarhornSquads);
    }
    [Fact]
    public void ConduitSamplesWholeEnemySquadBeforeAllSameTickApplicationsAndNeverExpandsSplash()
    {
        using var m = new Match(new() { EnemyPowerPercent = 100, SoldierHealth = 500, SoldierDamage = 0, DefenderDamage = 0 }, combatSeed: 1);
        City city = m.Join()!; city.Run = Run("M03", "M04", "M06");
        UnitCapabilities c = RunEffects.Capabilities(default, city.Run, UnitType.Mage);
        Group(m, UnitType.Mage, 3, 1, 17, c); int[] target = Group(m, UnitType.Swordsman, 3, 0, 18);
        int untouched = Group(m, UnitType.Swordsman, 1, 0, 19)[0];
        m.Combat.StartActions([city]); m.Combat.Advance(24, [city]);
        CombatEvent[] first = m.Combat.Events().Where(e => e.Type == CombatEventType.Impact && e.Unit?.Owner == 1).ToArray();
        Assert.Equal(3, first.Length); Assert.All(first, e => { Assert.Equal(2000, e.Damage); Assert.Equal(target.Order(), e.Victims.Order()); });
        Assert.All(target, id => { Assert.NotNull(m.Combat.Read(id).Statuses.Burn); Assert.Equal(10, m.Combat.Read(id).Statuses.Chill!.Strength); });
        Assert.Equal(50000, m.Combat.Read(untouched).Health);
        m.Combat.Advance(90, [city]); m.Combat.StartActions([city]); m.Combat.Advance(114, [city]);
        Assert.Equal(3, m.Combat.Events().Count(e => e.Tick == 114 && e.Type == CombatEventType.Impact && e.Unit?.Owner == 1 && e.Damage == 2096));
        Assert.Equal(50000, m.Combat.Read(untouched).Health);
    }
    [Fact]
    public void AshenQuiverUsesActualHitWhilePoisonPreservesExistingResearchCaptureAndBothCoexist()
    {
        using var m = new Match(new() { EnemyPowerPercent = 100, SoldierHealth = 500, SoldierDamage = 0, DefenderDamage = 0 }, combatSeed: 2);
        City city = m.Join()!; city.Run = Run("R02", "M05");
        UnitCapabilities c = RunEffects.Capabilities(new(DirectDamagePercent: 20, PoisonPercent: 15), city.Run, UnitType.Crossbowman);
        Group(m, UnitType.Crossbowman, 1, 1, 17, c); int victim = Group(m, UnitType.Swordsman, 1, 0, 18, new(ReductionPercent: 20))[0];
        m.Combat.StartActions([city]); m.Combat.Advance(18, [city]);
        UnitState struck = m.Combat.Read(victim);
        Assert.Equal(50000 - 1152, struck.Health); Assert.Equal(93, struck.Statuses.Burn!.Strength);
        Assert.Equal(288, Assert.Single(struck.Statuses.Poison).Strength);
        Assert.Equal(78, struck.Statuses.Burn.NextDamageTick); Assert.Equal(198, struck.Statuses.Burn.ExpiresTick);
        m.Combat.Advance(78, [city]); Assert.Equal(struck.Health - RunEffects.Scale(93, -20) - RunEffects.Scale(288, -20), m.Combat.Read(victim).Health);
    }
    [Fact]
    public void SimultaneousLethalTickCreditsOneCityAndLowestActualAttackerMemberId()
    {
        using var m = new Match(new() { EnemyPowerPercent = 100, SoldierDamage = 10, DefenderDamage = 0 }, combatSeed: 3);
        City a = m.Join()!, b = m.Join()!; a.Run = Run("R03"); b.Run = Run("R03");
        int low = Group(m, UnitType.Swordsman, 1, 1, 17)[0]; int high = Group(m, UnitType.Swordsman, 1, 2, 19)[0];
        int victim = Group(m, UnitType.Swordsman, 1, 0, 18)[0];
        m.Combat.Seed(m.Combat.Inspect(victim) with { Health = 1500 });
        Assert.True(low < high); m.Combat.StartActions([a, b]); m.Combat.Advance(12, [a, b]);
        Assert.Equal(0, m.Combat.Read(victim).Health); Assert.Equal(1, a.Run.EnemyKills); Assert.Equal(0, b.Run.EnemyKills);
        m.Combat.Advance(13, [a, b]); Assert.Equal(1, a.Run.EnemyKills);
        m.Combat.BeginWave(2, [], [a, b]); Assert.Equal(0, a.Run.EnemyKills);
    }
    [Fact]
    public void LethalPeriodicTickKeepsStrongerPotencyOwnerAfterWeakerRefreshAndSourceDeath()
    {
        using var m = new Match(new() { DefenderDamage = 0 }, combatSeed: 3);
        City a = m.Join()!, b = m.Join()!; a.Run = Run("R03"); b.Run = Run("R03"); m.Combat.BeginWave(1, [], [a, b]);
        int strong = Group(m, UnitType.Mage, 1, 1, 17)[0], weak = Group(m, UnitType.Mage, 1, 2, 19)[0];
        int victim = Group(m, UnitType.Swordsman, 1, 0, 18)[0];
        StatusState status = StatusPolicy.Apply(new(), new(victim, StatusKind.Burn, weak, 1, 0, 1), m.Configuration.Statuses);
        status = StatusPolicy.Apply(status, new(victim, StatusKind.Burn, strong, 1, 1, 10), m.Configuration.Statuses);
        status = StatusPolicy.Apply(status, new(victim, StatusKind.Burn, weak, 2, 2, 1), m.Configuration.Statuses);
        Assert.Equal(weak, status.Burn!.Identity.SourceId); Assert.Equal(strong, status.Burn.PotencySourceId);
        m.Combat.Seed(m.Combat.Inspect(victim) with { Health = 5, Statuses = status }); m.Combat.Remove(strong);
        m.Combat.Advance(60, [a, b]); Assert.Equal(0, m.Combat.Read(victim).Health);
        Assert.Equal(1, a.Run.EnemyKills); Assert.Equal(0, b.Run.EnemyKills);
    }
    [Fact]
    public void GuardianRootsReadsActualWatchpostIncludingSameTickDestructionAndNextReset()
    {
        using var m = new Match(new() { EnemyPowerPercent = 100, DefenderDamage = 0, Arena = new() { WatchpostHealth = 4 } }, combatSeed: 1);
        City city = m.Join()!; city.Run = Run("R06"); var arena = new SharedArena([1]);
        m.Combat.BeginWave(1, [], [city]);
        int keep = Group(m, UnitType.Swordsman, 1, 0, arena.Board.Cell(arena.Keep(1)).Neighbors.First(arena.CanOccupy))[0];
        int post = Group(m, UnitType.Swordsman, 1, 0, arena.Board.Cell(arena.Watchpost(1)).Neighbors.First(arena.CanOccupy))[0];
        void Commit(int id, CombatTargetKind kind, long impact, long sequence)
            => m.Combat.Seed(m.Combat.Inspect(id) with { Action = new CombatAction.Windup(sequence, sequence, impact + 48, new(new(kind, 1), impact - 1, impact)) });
        Commit(keep, CombatTargetKind.City, 1, 1); Commit(post, CombatTargetKind.Watchpost, 2, 1);
        m.Combat.Advance(1, [city]); Assert.Equal(9575, city.Health);
        Commit(keep, CombatTargetKind.City, 2, 2); m.Combat.Advance(2, [city]);
        Assert.Equal(9075, city.Health); Assert.True(m.Combat.StructureSnapshots([city]).Single(s => s.Kind == ArenaStructureKind.Watchpost).Destroyed);
        m.Combat.BeginWave(2, [], [city]);
        Assert.Equal(500, m.Combat.StructureSnapshots([city]).Single(s => s.Kind == ArenaStructureKind.Watchpost).Health);
    }
    [Fact]
    public void ExistingHpDoesNotCompoundOrHealAndFieldSurgeonsIncludesStoredAndReserveLivingIds()
    {
        using var m = new Match(combatSeed: 1); City city = m.Join()!;
        int[] ids = Group(m, UnitType.Swordsman, 3, 1, 17);
        foreach (int id in ids) m.Combat.Seed(m.Combat.Inspect(id) with { Health = 2901 });
        m.Combat.Seed(m.Combat.Inspect(ids[1]) with { Location = new(UnitLifecycle.Stored, default) });
        m.Combat.Seed(m.Combat.Inspect(ids[2]) with { Location = new(UnitLifecycle.Reserve, default) });
        city.Run = Run("A01", "A01", "A05"); m.Combat.Research(city.Id, default, m.Economy.Research, city.Run);
        Assert.All(ids, id => { Assert.Equal(5000, m.Combat.Read(id).Profile.Health); Assert.Equal(2901, m.Combat.Read(id).Health); });
        m.Combat.Research(city.Id, default, m.Economy.Research, city.Run); Assert.All(ids, id => Assert.Equal(2901, m.Combat.Read(id).Health));
        m.Combat.FieldSurgeons(city.Id, 20); Assert.All(ids, id => Assert.Equal(3321, m.Combat.Read(id).Health));
        m.Combat.Seed(m.Combat.Inspect(ids[0]) with { Health = 0, Location = new(UnitLifecycle.Dying, new(17, 1)) });
        m.Combat.FieldSurgeons(city.Id, 20); Assert.Equal(0, m.Combat.Read(ids[0]).Health);
        Assert.Equal(3657, m.Combat.Read(ids[1]).Health); Assert.Equal(3657, m.Combat.Read(ids[2]).Health);
    }
    [Theory]
    [InlineData(Building.ArrowTower, 600, 54, 11, 5)]
    [InlineData(Building.RapidArrowTower, 840, 27, 6, 5)]
    [InlineData(Building.CatapultTower, 720, 108, 27, 5)]
    [InlineData(Building.BombardTower, 960, 108, 27, 5)]
    public void TowerStacksAndFullCadenceUseTheSameCanonicalProjection(Building type, int damage, int cadence, int windup, int range)
    {
        using var m = new Match();
        TowerDefinition original = m.Configuration.Towers.Single(t => t.Type == type && t.Level == (type is Building.RapidArrowTower or Building.BombardTower ? 2 : 1));
        TowerDefinition tower = RunEffects.Tower(original, Run("T02", "T02", "T03", "T03", "T04", "T05", "T06"));
        Assert.Equal(damage, tower.Damage); Assert.Equal(cadence, tower.CadenceTicks); Assert.Equal(windup, tower.WindupTicks); Assert.Equal(range, tower.HexRange);
        Assert.Equal(RunEffects.Scale(original.SecondaryDamage, 20), tower.SecondaryDamage);
        Assert.True(tower.CadenceTicks > tower.WindupTicks && tower.WindupTicks > 0);
        Assert.Equal(8250, RunEffects.Watchpost(5000, Run("T01", "T01")));
    }
}
