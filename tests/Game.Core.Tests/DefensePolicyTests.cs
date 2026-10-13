using System.Text.Json;
using Xunit;

namespace Game.Core.Tests;

// Tier A production physical defenses. Numerical seeded groups isolate delivery;
// actual paid build/upgrade commands and pause use Match. Broader paid ownership,
// retry/reconnect/codec negatives remain in ArmyPlotTower/Authority suites.
public sealed class DefensePolicyTests
{
    private static void Act(Match match, string action, int slot = -1, Building type = Building.Empty)
    {
        CommandResult result = match.Apply(1, Command.FromSnapshot(match.Snapshot(), match.Revision + 1, action, 1, slot, type));
        Assert.True(result.Accepted, result.Message);
    }
    private static Match PaidTower(Building type, int players = 1)
    {
        var match = new Match(new() { StartingGold = 100, StartingWood = 100, RangedHealth = 100, Arena = new() { SquadCapacity = 5 } }, combatSeed: 123);
        try
        {
            for (int player = 0; player < players; player++) match.Join();
            City owner = match.Players[1]; owner.Stone = owner.Metal = 100;
            Act(match, "start");
            Act(match, "build", 0, type is Building.RapidArrowTower or Building.BombardTower ? Building.ArrowTower : type);
            if (type is Building.RapidArrowTower or Building.BombardTower) Act(match, "upgrade-tower", 0, type);
            for (int round = 0; round < 8 && match.Phase != Phase.Combat; round++)
                foreach (City city in match.Players.Values.Where(c => !c.Ready).ToArray())
                {
                    CommandResult ready = match.Apply(city.Id, Command.FromSnapshot(match.Snapshot(), match.Revision + 1, "ready", city.Id));
                    Assert.True(ready.Accepted, ready.Message);
                }
            Assert.Equal(Phase.Combat, match.Phase);
            CombatFixture.ClearEnemies(match);
            foreach (City city in match.Players.Values) city.Defender = city.Defender with { ReadyTick = 100000 };
            return match;
        }
        catch { match.Dispose(); throw; }
    }

    [Fact]
    public void DefenseCatalogPublishesActualXYRangeAndConfiguredCycleWithoutASeparateActor()
    {
        var configuration = new CombatConfiguration(new());
        Assert.Equal(new DefenseProfile(200, 12, 48), configuration.BuiltInDefense);
        foreach (var (type, level, x, y, windup, cadence, range) in new[]
        {
            (Building.ArrowTower, 1, 500, 0, 12, 60, 4), (Building.ArrowTower, 2, 700, 0, 12, 60, 4),
            (Building.CatapultTower, 1, 600, 600, 30, 120, 5), (Building.CatapultTower, 2, 800, 600, 30, 120, 5),
            (Building.RapidArrowTower, 2, 700, 0, 6, 30, 4), (Building.BombardTower, 2, 800, 600, 30, 120, 5)
        })
        {
            TowerDefinition profile = Assert.Single(configuration.Towers, t => t.Type == type && t.Level == level);
            Assert.Equal((x, y, windup, cadence, range), (profile.Damage, profile.SecondaryDamage, profile.WindupTicks, profile.CadenceTicks, profile.HexRange));
        }
        var custom = new CombatConfiguration(new() { Combat = new() { Defender = new(7, 23), ArrowTower = new(8, 32) }, Arena = new() { ArrowRange = 2 } });
        Assert.NotEqual(configuration.Fingerprint, custom.Fingerprint);
        var defense = new DefenseActor(DefenseIdentity.Defender(1), new(1, -1, Building.Empty, 0), custom.BuiltInDefense);
        TowerState committed = defense.Start(8, 100);
        Assert.Equal((100L, 107L, 130L, 1L), (committed.ActionStartTick, committed.ImpactTick, committed.ReadyTick, committed.AttackSequence));
        using var owners = new Match(); owners.Join(); owners.Join();
        Assert.Throws<ArgumentException>(() => defense.Commit(owners.Players[2], committed));
        Assert.Throws<ArgumentException>(() => DefenseIdentity.Tower(1, -1));
    }

    [Theory]
    [InlineData(Building.ArrowTower, 12, 60, 1)]
    [InlineData(Building.RapidArrowTower, 6, 30, 1)]
    [InlineData(Building.CatapultTower, 30, 120, 5)]
    [InlineData(Building.BombardTower, 30, 120, 5)]
    public void PaidPhysicalTowerCommitsOneGroupCycleAndPauseCannotAdvanceIt(Building type, int windup, int cadence, int damaged)
    {
        using Match match = PaidTower(type); City owner = match.Players[1]; var arena = new SharedArena([1]);
        int cell = arena.Board.Cell(arena.Tower(1, 0)).Neighbors.First(arena.CanOccupy);
        int[] group = CombatFixture.Group(match.Combat, UnitType.Crossbowman, 5, 0, Faction.Skeletons, cell);
        match.Combat.StartActions([owner]);
        TowerState committed = owner.Towers[0];
        Assert.Equal((1L, (long)windup, (long)cadence), (committed.AttackSequence, committed.ImpactTick, committed.ReadyTick));
        Assert.Equal(match.Combat.Read(group[0]).SquadId, committed.TargetId);
        StructureState physical = Assert.Single(match.Snapshot().Structures, s => s.Slot == 0);
        Assert.Equal(arena.Tower(1, 0), physical.Cell); Assert.Equal(type, physical.TowerType);
        Assert.Empty(owner.Soldiers); Assert.Single(match.Combat.SquadSnapshots());
        Act(match, "pause"); string frozen = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
        CombatFixture.Steps(match, 100); Assert.Equal(frozen, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
        Act(match, "resume"); CombatFixture.Steps(match, windup);
        CombatEvent impact = Assert.Single(match.Combat.Events(), e => e.Type == CombatEventType.Impact && e.Tower?.Slot == 0);
        Assert.True(impact.Landed); Assert.Null(impact.Unit); Assert.Equal(damaged, impact.Victims.Length);
        TowerDefinition profile = Assert.Single(match.Configuration.Towers, t => t.Type == type && t.Level == committed.Level);
        Assert.Equal(50000 - profile.Damage - 4 * profile.SecondaryDamage, group.Sum(id => match.Combat.Read(id).Health));
        Assert.False(owner.Towers[0].PendingImpact);
        match.Combat.Advance(windup + 1, [owner]);
        Assert.Single(match.Combat.Events(), e => e.Type == CombatEventType.Impact && e.Tower?.Slot == 0);
    }

    [Theory]
    [InlineData(Building.ArrowTower)]
    [InlineData(Building.CatapultTower)]
    public void MovingCommittedGroupBeyondPhysicalRangeMissesWithoutAcquiringANearbyGroup(Building type)
    {
        using Match match = PaidTower(type); City owner = match.Players[1]; var arena = new SharedArena([1]);
        TowerDefinition profile = Assert.Single(match.Configuration.Towers, t => t.Type == type && t.Level == 1);
        int source = arena.Tower(1, 0), near = arena.Board.Cell(source).Neighbors.First(arena.CanOccupy);
        int enemy = CombatFixture.Group(match.Combat, UnitType.Crossbowman, 1, 0, Faction.Skeletons, near)[0];
        match.Combat.StartActions([owner]); TowerState committed = owner.Towers[0];
        int far = arena.Board.Cells.First(c => arena.CanOccupy(c.Id) && arena.Board.Distance(source, c.Id) > profile.HexRange).Id;
        match.Combat.Seed(match.Combat.Inspect(enemy) with { Location = new(UnitLifecycle.Alive, new(far, 1)) });
        int replacement = CombatFixture.Group(match.Combat, UnitType.Crossbowman, 1, 0, Faction.Skeletons, near)[0];
        match.Combat.Advance(committed.ImpactTick, [owner]);
        CombatEvent miss = Assert.Single(match.Combat.Events(), e => e.Type == CombatEventType.Impact && e.Tower?.Slot == 0);
        Assert.False(miss.Landed); Assert.Empty(miss.Victims);
        Assert.Equal(committed.TargetId, miss.Tower!.TargetId);
        Assert.Equal(10000, match.Combat.Read(enemy).Health); Assert.Equal(10000, match.Combat.Read(replacement).Health);
    }

    [Fact]
    public void WatchpostAndPurchasedTowerSelectFromTheirOwnDistinctPhysicalOrigins()
    {
        using var match = new Match(combatSeed: 123); City city = match.Join()!;
        city.Towers[0] = new(1, 0, Building.ArrowTower, 1);
        var arena = new SharedArena([1]);
        int towerCell = arena.Board.Cell(arena.Tower(1, 0)).Neighbors.First(c => arena.CanOccupy(c) && arena.Board.Distance(c, arena.Watchpost(1)) > 1);
        int postCell = arena.Board.Cell(arena.Watchpost(1)).Neighbors.First(c => arena.CanOccupy(c) && arena.Board.Distance(c, arena.Tower(1, 0)) > 1);
        int towerEnemy = CombatFixture.Group(match.Combat, UnitType.Crossbowman, 1, 0, Faction.Skeletons, towerCell)[0];
        int postEnemy = CombatFixture.Group(match.Combat, UnitType.Crossbowman, 1, 0, Faction.Skeletons, postCell)[0];
        match.Combat.StartActions([city]);
        Assert.Equal(match.Combat.Read(towerEnemy).SquadId, city.Towers[0].TargetId);
        Assert.Equal(match.Combat.Read(postEnemy).SquadId, city.Defender.TargetId);
        Assert.NotEqual(city.Defender.TargetId, city.Towers[0].TargetId);
    }

    [Fact]
    public void SquadWatchpostAndTowerGatherOnceAgainstTheSamePreImpactMembers()
    {
        using var match = new Match(combatSeed: 123); City city = match.Join()!;
        city.Towers[0] = new(1, 0, Building.ArrowTower, 1);
        int soldier = CombatFixture.Group(match.Combat, UnitType.Swordsman, 1, 1, Faction.Adventurers, 30)[0];
        int enemy = CombatFixture.Group(match.Combat, UnitType.Swordsman, 1, 0, Faction.Skeletons, 29)[0];
        match.Combat.Seed(match.Combat.Inspect(soldier) with { Health = 100 });
        match.Combat.Seed(match.Combat.Inspect(enemy) with { Health = 500 });
        match.Combat.StartActions([city]); match.Combat.Advance(12, [city]);
        CombatEvent[] impacts = match.Combat.Events().Where(e => e.Type == CombatEventType.Impact).ToArray();
        Assert.Equal(4, impacts.Length); Assert.All(impacts, impact => Assert.True(impact.Landed));
        Assert.Equal(3, impacts.Count(impact => impact.Victims.SequenceEqual([enemy])));
        Assert.Equal(new[] { soldier, enemy }, match.Combat.Dying().Select(u => u.Id));
        Assert.Equal(500, Assert.Single(match.Combat.Events(), e => e.Type == CombatEventType.Hit && e.Unit!.Id == enemy).Damage);
        Assert.Empty(match.Combat.Reservations.Positions);
        match.Combat.Advance(13, [city]); Assert.Equal(4, match.Combat.Events().Count(e => e.Type == CombatEventType.Impact));
    }

    [Fact]
    public void FallenOwnerCeasesCommittedDefenseButKeepsItsPurchasedWeaponForSpectating()
    {
        using Match match = PaidTower(Building.ArrowTower, players: 2); City owner = match.Players[1];
        var arena = new SharedArena([1, 2]);
        int cell = arena.Board.Cell(arena.Tower(1, 0)).Neighbors.First(arena.CanOccupy);
        int enemy = CombatFixture.Group(match.Combat, UnitType.Crossbowman, 1, 0, Faction.Skeletons, cell)[0];
        match.Combat.StartActions(match.Players.Values); Assert.True(owner.Towers[0].PendingImpact);
        SlotState purchase = owner.Slots[0]; owner.Health = 0;
        match.Step(); Assert.True(owner.Eliminated); Assert.Equal(Phase.Combat, match.Phase);
        Assert.False(owner.Towers[0].PendingImpact); Assert.False(owner.Defender.PendingImpact);
        Assert.Equal(purchase, owner.Slots[0]); Assert.Equal(Building.ArrowTower, owner.Towers[0].Type);
        StructureState weapon = Assert.Single(match.Snapshot().Structures, s => s.Owner == 1 && s.Slot == 0);
        Assert.False(weapon.Active); Assert.False(weapon.Destroyed);
        match.Combat.Advance(12, match.Players.Values);
        Assert.DoesNotContain(match.Combat.Events(), e => e.Type == CombatEventType.Impact && e.Tower?.City == 1);
        Assert.Equal(10000, match.Combat.Read(enemy).Health);
        string before = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
        CommandResult denied = match.Apply(1, Command.FromSnapshot(match.Snapshot(), match.Revision + 1, "sell", 1, 0));
        Assert.False(denied.Accepted); Assert.Equal(before, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
    }
}
