using Xunit;

namespace Game.Core.Tests;

// Tier A production authority regressions; old individual actor/city shield tests
// cannot prove shared group impacts. Explicit seeded member fixtures isolate the
// combat seam; paid commands below exercise the real economy/command boundary.
public sealed class SharedCombatTests
{
    private static int[] Group(CombatSimulation combat, UnitType type, int count, int owner, Faction faction, int cell, int destination = 1)
    {
        int[] ids = Enumerable.Range(0, count).Select(_ => combat.Create(type, owner, destination, destination, faction,
            assignment: faction == Faction.Adventurers ? new(-1, 0, 0, 1) : null)).ToArray();
        foreach (int id in ids) combat.Seed(combat.Inspect(id) with { Location = new(UnitLifecycle.Alive, new(cell, 1)) });
        return ids;
    }

    [Fact]
    public void SharedCycleResolvesMutualSquadDeathSimultaneouslyAndReleasesCellsImmediately()
    {
        var rules = new Rules { MageDamage = 50, Arena = new() { MageSecondaryDamage = 50 } };
        using var combat = new CombatSimulation(rules);
        int[] allies = Group(combat, UnitType.Mage, 3, 1, Faction.Adventurers, 17);
        int[] enemies = Group(combat, UnitType.Mage, 3, 0, Faction.Skeletons, 18);
        Assert.Equal(2, combat.SquadSnapshots().Length);
        combat.StartActions([]);
        Assert.All(combat.SquadSnapshots(), s => Assert.Equal(UnitActionKind.Windup, s.Hex.Action));
        combat.Advance(24, []);
        Assert.Empty(combat.Snapshot());
        Assert.Equal(6, combat.Dying().Length);
        Assert.Equal(2, combat.Events().Count(e => e.Type == CombatEventType.SquadDeath));
        Assert.Equal(6, combat.Events().Count(e => e.Type == CombatEventType.Impact && e.Landed));
        Assert.Empty(combat.Reservations.Positions);
        Assert.All(allies.Concat(enemies), id => Assert.Equal(0, combat.Read(id).Health));
    }

    [Fact]
    public void OverkillStaysOnPrimaryWhileOtherMembersKeepIndividualHealth()
    {
        using var combat = new CombatSimulation(new() { SoldierDamage = 500 });
        Group(combat, UnitType.Swordsman, 1, 1, Faction.Adventurers, 17);
        int[] defenders = Group(combat, UnitType.Swordsman, 3, 0, Faction.Skeletons, 18);
        combat.StartActions([]); combat.Advance(12, []);
        Assert.Single(defenders, id => combat.Read(id).Health == 0);
        Assert.Equal(2, defenders.Count(id => combat.Read(id).Health == HealthPoints.FromWhole(40)));
        SquadState target = combat.SquadSnapshots().Single(s => s.Faction == Faction.Skeletons);
        Assert.Equal(2, target.ParticipatingMembers.Length);
        Assert.Equal(UnitLifecycle.Alive, target.Hex.Lifecycle);
        Assert.DoesNotContain(combat.Events(), e => e.Type == CombatEventType.SquadDeath && e.SquadId == target.Id);
    }

    [Fact]
    public void KeepCanBeHitWhileDistantAlliesLiveAndFallenOwnersSurviveTheBattle()
    {
        using var match = new Match(new() { SoldierDamage = 0, DefenderDamage = 0 }, combatSeed: 1);
        City city = match.Join()!;
        match.Combat.ConfigureArena([city.Id]);
        int allied = Group(match.Combat, UnitType.Swordsman, 1, city.Id, Faction.Adventurers, 17)[0];
        int enemy = match.Combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons);
        var arena = new SharedArena([1]);
        int approach = match.Combat.Board.Cell(arena.Keep(1)).Neighbors.First(arena.CanOccupy);
        match.Combat.Seed(match.Combat.Inspect(enemy) with { Location = new(UnitLifecycle.Alive, new(approach, 1)) });
        city.Health = HealthPoints.FromWhole(5);
        match.Combat.StartActions([city]); match.Combat.Advance(12, [city]);
        Assert.True(city.Eliminated);
        Assert.True(match.Combat.Read(allied).Health > 0);
        Assert.Equal(UnitLifecycle.Alive, match.Combat.Read(allied).Hex!.Lifecycle);
        Assert.Contains(match.Combat.Events(), e => e.Type == CombatEventType.StructureDestroyed && e.Structure?.Kind == ArenaStructureKind.Keep);
        match.Combat.EliminateArmy(city.Id);
        Assert.Equal(UnitLifecycle.Alive, match.Combat.Read(allied).Hex!.Lifecycle);
        match.Combat.BeginWave(2, [], [city]);
        Assert.Equal(UnitLifecycle.Reserve, match.Combat.Read(allied).Hex!.Lifecycle);
    }

    [Fact]
    public void NormalSoloTwoSquadDefenseAdvancesWithoutOccupancyCollisionsOrStall()
    {
        using var match = new Match(new() { StartingGold = 100, StartingWood = 100 }, combatSeed: 8);
        City city = match.Join()!; city.Metal = 100; city.Food = 100;
        long sequence = 0;
        void Apply(string action, int slot = -1, Building building = Building.Empty)
        {
            CommandResult result = match.Apply(city.Id, Command.FromSnapshot(match.Snapshot(), ++sequence, action, city.Id, slot, building));
            Assert.True(result.Accepted, result.Message);
        }
        Apply("start"); Apply("build", 0, Building.Barracks);
        for (int n = 0; n < 6; n++) Apply("recruit", 0);
        Apply("ready"); Apply("ready"); Apply("ready"); Apply("ready");
        for (int tick = 0; tick < 6000 && match.Phase == Phase.Combat; tick++)
        {
            match.Step();
            SquadState[] actors = match.Snapshot().Squads.Where(s => s.Hex.Lifecycle == UnitLifecycle.Alive).ToArray();
            Assert.Equal(actors.Length, actors.Select(s => s.Hex.Position.Cell).Distinct().Count());
        }
        Assert.NotEqual(Phase.Combat, match.Phase);
        Assert.Null(match.Stall);
        Assert.Equal(2, match.Wave);
        Assert.Contains(city.Soldiers, u => u.Health > 0);
        Assert.All(city.Soldiers, u => { Assert.Equal(1, u.Experience); Assert.Equal(1, u.Level); });
    }

    [Fact]
    public void RecruitmentCreatesHomogeneousThreeMemberSquadsAndPartialFundingKeepsRoster()
    {
        using var match = new Match(new() { StartingGold = 100, StartingWood = 100 }, combatSeed: 4);
        City city = match.Join()!; city.Metal = 100;
        long sequence = 0;
        void Apply(string action, int slot = -1, Building building = Building.Empty, int squadId = 0)
        {
            Command command = Command.FromSnapshot(match.Snapshot(), ++sequence, action, city.Id, slot, building, squadId: squadId);
            CommandResult result = match.Apply(city.Id, command); Assert.True(result.Accepted, result.Message);
        }
        Apply("start"); Apply("build", 0, Building.Barracks);
        for (int n = 0; n < 6; n++) Apply("recruit", 0);
        SquadState[] roster = match.Snapshot().Squads;
        Assert.Equal(2, roster.Length); Assert.All(roster, s => Assert.Equal(3, s.Members.Length));
        Apply("ready"); Apply("ready"); Apply("ready");
        city.Food = 2; Apply("ready");
        Assert.Equal(Phase.Combat, match.Phase);
        MatchSnapshot snapshot = match.Snapshot();
        Assert.Equal(6, snapshot.Players[0].Soldiers.Length);
        Assert.Equal(2, snapshot.Players[0].LastUpkeep!.Participating.Length);
        Assert.Equal(4, snapshot.Players[0].LastUpkeep!.Unfed.Length);
        SquadState[] active = snapshot.Squads.Where(s => s.Faction == Faction.Adventurers && s.Hex.Lifecycle == UnitLifecycle.Alive).ToArray();
        Assert.Single(active); Assert.Equal(2, active[0].ParticipatingMembers.Length);
        Assert.Equal(3, snapshot.Reservations.Positions.Length); // one allied actor + four authored enemies grouped 3+1
        int[] members = roster.SelectMany(s => s.Members).Order().ToArray();
        Assert.Equal(members, snapshot.Players[0].Soldiers.Select(u => u.Id).Order());
    }

    [Fact]
    public void WatchpostBreakDisruptsOnlyItsParticipatingOwnerOnceAndAppliesToCommittedDirectDamage()
    {
        using var match = new Match(new() { MageDamage = 20, DefenderDamage = 0, Arena = new() { WatchpostHealth = 5 } }, combatSeed: 7);
        match.Join(); match.Join(); City[] owners = match.Players.Values.ToArray(); var arena = new SharedArena([1, 2]);
        int[] affected = Group(match.Combat, UnitType.Mage, 3, 1, Faction.Adventurers, arena.Deployment(1, 0));
        int[] helper = Group(match.Combat, UnitType.Mage, 3, 2, Faction.Adventurers, arena.Deployment(2, 0), 2);
        int enemy = Group(match.Combat, UnitType.Swordsman, 1, 0, Faction.Skeletons, arena.Board.Cell(arena.Watchpost(1)).Neighbors.First(arena.CanOccupy))[0];
        match.Combat.BeginWave(1, [], owners);
        int approach = arena.Board.Cell(arena.Watchpost(1)).Neighbors.First(cell => arena.CanOccupy(cell) && arena.Board.Cell(cell).Coordinate.Column == 4);
        match.Combat.Seed(match.Combat.Inspect(enemy) with { Location = new(UnitLifecycle.Alive, new(approach, 1)) });
        match.Combat.StartActions(owners); match.Combat.Advance(12, owners);
        Assert.True(match.Combat.StructureSnapshots(owners).Single(s => s.Owner == 1 && s.Kind == ArenaStructureKind.Watchpost).Destroyed);
        Assert.Equal(252, match.Combat.SquadSnapshots().Single(s => s.Members.Contains(affected[0])).RallyDisruptedUntil);
        Assert.Equal(0, match.Combat.SquadSnapshots().Single(s => s.Members.Contains(helper[0])).RallyDisruptedUntil);
        Assert.Single(match.Combat.Events(), e => e.Type == CombatEventType.RallyDisrupted);
        match.Combat.Advance(24, owners);
        Assert.Equal(3, match.Combat.Events().Count(e => e.Type == CombatEventType.Impact && e.Unit?.Owner == 1 && e.Landed && e.Damage == HealthPoints.FromWhole(16)));
        CombatEvent[] beforeExpiry = match.Combat.Events(); // Receiving consumers retain sequence evidence, not an unbounded authority event window.
        match.Combat.Advance(252, owners); Assert.Single(match.Combat.Events(), e => e.Type == CombatEventType.RallyRestored);
        Assert.Single(beforeExpiry.Concat(match.Combat.Events()).DistinctBy(e => e.Sequence), e => e.Type == CombatEventType.RallyDisrupted);
        match.Combat.BeginWave(2, [], owners);
        Assert.Equal(HealthPoints.FromWhole(5), match.Combat.StructureSnapshots(owners).Single(s => s.Owner == 1 && s.Kind == ArenaStructureKind.Watchpost).Health);
        Assert.All(match.Combat.SquadSnapshots(), s => Assert.Equal(0, s.RallyDisruptedUntil));
    }

    [Fact]
    public void NeighborCanPhysicallyCrossItsSectorAndRescueAnAlreadyDamagedKeep()
    {
        using var match = new Match(new() { MageDamage = 60, SoldierDamage = 0, DefenderDamage = 0, Combat = new() { MageHexRange = 1 }, Arena = new() { MageSecondaryDamage = 60 } }, combatSeed: 12);
        for (int player = 0; player < 4; player++) match.Join();
        var arena = new SharedArena(Enumerable.Range(1, 4)); City[] owners = match.Players.Values.ToArray();
        int[] rescuers = Group(match.Combat, UnitType.Mage, 3, 3, Faction.Adventurers, arena.Deployment(3, 5), 3);
        int approach = arena.Board.Cell(arena.Keep(4)).Neighbors.First(cell => arena.CanOccupy(cell) && arena.Board.Cell(cell).Coordinate.R == 8);
        Group(match.Combat, UnitType.Swordsman, 3, 0, Faction.Skeletons, approach, 4);
        for (int tick = 0; tick < 600 && match.Combat.Enemies().Length > 0; tick++) match.Combat.Step(tick, owners);
        Assert.Empty(match.Combat.Enemies());
        Assert.InRange(match.Players[4].Health, 1, HealthPoints.FromWhole(100) - 1);
        Assert.Contains(rescuers, id => arena.Board.Cell(match.Combat.Read(id).Hex!.Position.Cell).Coordinate.Column >= 21);
        Assert.Contains(match.Combat.Events(), e => e.Type == CombatEventType.Impact && e.Unit?.Owner == 3 && e.Landed && e.Damage > 0);
        SnapshotCompatibility.RequireSharedArena(match.Snapshot());
    }

    [Fact]
    public void LastKeepAndLastEnemyDyingTogetherEndsInDefeatNotClearReward()
    {
        using var match = new Match(new() { MageDamage = 60, DefenderDamage = 0, Combat = new() { MageWindupTicks = 12, MageRecoveryTicks = 78 } }, combatSeed: 9);
        City city = match.Join()!; long sequence = 0;
        void Ready(string action) => Assert.True(match.Apply(1, Command.FromSnapshot(match.Snapshot(), ++sequence, action, 1)).Accepted);
        Ready("start"); while (match.Phase != Phase.Combat) Ready("ready");
        foreach (UnitState enemy in match.Combat.Enemies()) match.Combat.Remove(enemy.Id);
        var arena = new SharedArena([1]);
        int approach = arena.Board.Cell(arena.Keep(1)).Neighbors.First(arena.CanOccupy);
        int alliedCell = arena.Board.Cells.Where(c => arena.CanOccupy(c.Id) && arena.Board.Distance(c.Id, approach) == 2).OrderBy(c => c.Id).Last().Id;
        int allied = Group(match.Combat, UnitType.Mage, 1, 1, Faction.Adventurers, alliedCell)[0];
        Group(match.Combat, UnitType.Swordsman, 1, 0, Faction.Skeletons, approach);
        city.Health = HealthPoints.FromWhole(5); int gold = city.Gold;
        for (int tick = 0; tick < 20 && match.Phase == Phase.Combat; tick++) match.Step();
        Assert.Equal(Phase.Defeat, match.Phase); Assert.Equal(1, match.Wave); Assert.Equal(gold, city.Gold);
        Assert.Empty(match.Combat.Enemies()); Assert.True(match.Combat.Read(allied).Health > 0);
        Assert.Contains(match.Snapshot().DyingBodies, u => u.Faction == Faction.Skeletons);
    }
}
