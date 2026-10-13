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

    // Tier A: a representative squad member is not the realized numerical
    // primary. Existing damage totals miss wrong native aim/early recovery loss.
    // One fixed 1-v-3 encounter/12 steps, no transport/graphics (estimated <1s).
    [Fact]
    public void RealizedPrimaryRemainsThePublishedAimAfterItsDeathWithoutRetargetingTheCommittedGroup()
    {
        using var combat = new CombatSimulation(new() { SoldierDamage = 15, RangedDamage = 0 });
        int actor = Group(combat, UnitType.Swordsman, 1, 1, Faction.Adventurers, 17)[0];
        int[] defenders = Group(combat, UnitType.Crossbowman, 3, 0, Faction.Skeletons, 18);
        foreach (int id in defenders) combat.Seed(combat.Inspect(id) with { Health = HealthPoints.FromWhole(10) });
        combat.StartActions([]);
        CombatEvent started = Assert.Single(combat.Events(), e => e.Type == CombatEventType.AttackStarted && e.Unit?.Id == actor);
        combat.Advance(12, []);
        CombatEvent impact = Assert.Single(combat.Events(), e => e.Type == CombatEventType.Impact && e.Unit?.Id == actor);
        UnitState recovery = combat.Read(actor);
        Assert.True(impact.Landed); Assert.Equal(0, combat.Read(impact.TargetId).Health);
        Assert.Equal(2, combat.Enemies().Length);
        Assert.Equal(impact.TargetId, started.TargetId);
        Assert.Equal(impact.TargetId, impact.Unit!.TargetId);
        Assert.Equal(impact.TargetId, recovery.TargetId);
        Assert.Equal(impact.ImpactPose, recovery.Hex!.FrozenAim);
        Assert.Equal(UnitActionKind.Recovery, recovery.Hex.Action);
        Assert.Equal(started.Unit!.AttackSequence, recovery.AttackSequence);
        Assert.Equal(started.Unit.ReadyTick, recovery.ReadyTick);
        Assert.Contains(combat.Reservations.Positions, p => p.UnitId == combat.Read(impact.TargetId).SquadId && p.Cell == 18); // The surviving target squad still owns its source.
    }

    [Fact]
    public void KeepCanBeHitWhileDistantAlliesLiveAndFallenOwnersSurviveTheBattle()
    {
        // Tier A controlled physical encounter on an actual paid two-owner
        // roster. Explicit wounded Keep/member and poses isolate World.Step's
        // elimination ordering; this is not an earned balance/real ENet claim.
        using Match match = PaidCampaignFixture.Opening(seed: 1);
        var arena = new SharedArena([1, 2]);
        City city = match.Players[2];
        int enemy = match.Combat.Enemies().Last().Id;
        foreach (UnitState extra in match.Combat.Enemies().Where(u => u.Id != enemy)) match.Combat.Remove(extra.Id);
        foreach (SquadState squad in match.Snapshot().Squads.Where(s => s.Faction == Faction.Adventurers))
        {
            int home = match.Combat.Read(squad.Members[0]).Assignment!.Tile;
            int cell = squad.Owner == 1 ? arena.EnemyCells(1)[home] : arena.Deployment(1, home);
            foreach (int id in squad.Members)
                match.Combat.Seed(match.Combat.Inspect(id) with { Location = new(UnitLifecycle.Alive, new(cell, 1)) });
        }
        int approach = arena.Board.Cell(arena.Keep(2)).Neighbors.First(arena.CanOccupy);
        match.Combat.Seed(match.Combat.Inspect(enemy) with { Location = new(UnitLifecycle.Alive, new(approach, 1)) });
        int injured = city.Soldiers[0].Id;
        match.Combat.Seed(match.Combat.Inspect(injured) with { Health = HealthPoints.FromWhole(33) });
        city.Health = HealthPoints.FromWhole(5);
        MatchSnapshot before = match.Snapshot();
        for (int tick = 0; tick < 20 && !city.Eliminated; tick++) { before = match.Snapshot(); match.Step(); }
        Assert.True(city.Eliminated); Assert.Equal(Phase.Combat, match.Phase);
        MatchSnapshot fallen = Game.SnapshotPayload.DecodeForReceiving(Game.SnapshotPayload.Encode(match.Snapshot()));
        UnitState[] survivors = fallen.Players[1].Soldiers;
        Assert.Equal(before.Players[1].Soldiers.Select(u => (u.Id, u.Health, u.Level, u.Experience, u.SquadId, u.FormationSlot)),
            survivors.Select(u => (u.Id, u.Health, u.Level, u.Experience, u.SquadId, u.FormationSlot)));
        Assert.All(survivors, u => { Assert.True(u.Participating); Assert.Equal(UnitLifecycle.Alive, u.Hex!.Lifecycle); });
        Assert.Equal(HealthPoints.FromWhole(33), survivors.Single(u => u.Id == injured).Health);
        foreach (SquadState squad in fallen.Squads.Where(s => s.Owner == 2))
        {
            SquadState previous = before.Squads.Single(s => s.Id == squad.Id);
            Assert.Equal(previous.Hex, squad.Hex); Assert.Equal(previous.ImpactTick, squad.ImpactTick); Assert.Equal(previous.ReadyTick, squad.ReadyTick);
            Assert.Contains(fallen.Reservations.Positions, claim => claim.UnitId == squad.Id && claim.Cell == squad.Hex.Position.Cell && claim.Anchor == squad.Hex.Position.Anchor);
        }
        PaidCampaignFixture.Legal(fallen);
        Assert.Contains(fallen.CombatEvents, e => e.Type == CombatEventType.StructureDestroyed && e.Structure?.Owner == 2 && e.Structure.Kind == ArenaStructureKind.Keep);
        long cursor = fallen.EventSequence; bool continued = false;
        for (int tick = 0; tick < 3000 && match.Phase == Phase.Combat; tick++)
        {
            match.Step();
            continued |= match.Combat.Events().Any(e => e.Sequence > cursor && e.Type == CombatEventType.Impact && e.Landed && e.Unit?.Owner == 2);
            cursor = match.Combat.EventSequence;
        }
        Assert.True(continued, "Fallen-owner survivors must actually land a later current-battle strike.");
        Assert.Equal(Phase.Building, match.Phase);
        Assert.Contains(city.Soldiers, u => u.Experience == 1);
        for (int cleanup = 0; cleanup < 120 && match.Combat.HasDeaths; cleanup++) match.Step();
        Assert.False(match.Combat.HasDeaths);
        for (int gate = 0; gate < 3; gate++) PaidCampaignFixture.Apply(match, 1, new("ready"));
        MatchSnapshot next = match.Snapshot();
        Assert.Equal(Phase.Combat, next.Phase);
        Assert.All(next.Players[1].Soldiers, u => { Assert.False(u.Participating); Assert.Equal(UnitLifecycle.Reserve, u.Hex!.Lifecycle); });
        Assert.DoesNotContain(next.Reservations.Positions, p => next.Squads.Single(s => s.Id == p.UnitId).Owner == 2);
    }

    // Tier A full-force strength, explicitly arithmetic equipment/food setup.
    // Paid opening diagnostics cannot isolate a complete ranged army. Default
    // wave/profiles, retained seeds, <6000 steps; no children or melee screen.
    [Theory]
    [InlineData(UnitType.Swordsman, 8UL)]
    [InlineData(UnitType.Crossbowman, 1UL)]
    [InlineData(UnitType.Crossbowman, 123UL)]
    public void NormalSoloTwoSquadDefenseAdvancesWithoutOccupancyCollisionsOrStall(UnitType type, ulong seed)
    {
        using var match = new Match(new() { StartingGold = 100, StartingWood = 100 }, combatSeed: seed);
        City city = match.Join()!; city.Metal = 100; city.Food = 100;
        long sequence = 0;
        void Apply(string action, int slot = -1, Building building = Building.Empty)
        {
            CommandResult result = match.Apply(city.Id, Command.FromSnapshot(match.Snapshot(), ++sequence, action, city.Id, slot, building, type));
            Assert.True(result.Accepted, result.Message);
        }
        Apply("start"); Apply("build", 0, type == UnitType.Crossbowman ? Building.ArcheryRange : Building.Barracks);
        for (int n = 0; n < 6; n++) Apply("recruit", 0);
        MatchSnapshot roster = match.Snapshot();
        Assert.Equal(6, city.Soldiers.Count);
        Assert.All(roster.Squads, squad => Assert.Equal(3, squad.Members.Length));
        Assert.Equal(2, roster.Squads.Length);
        Apply("ready"); Apply("ready"); Apply("ready");
        Assert.Equal(6, match.Snapshot().Players[0].LastUpkeep!.Paid);
        for (int tick = 0; tick < 6000 && match.Phase == Phase.Combat; tick++)
        {
            match.Step();
            SquadState[] actors = match.Snapshot().Squads.Where(s => s.Hex.Lifecycle == UnitLifecycle.Alive).ToArray();
            Assert.Equal(actors.Length, actors.Select(s => s.Hex.Position.Cell).Distinct().Count());
        }
        Assert.Equal(Phase.Building, match.Phase);
        Assert.True(city.Health > 0);
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
        // Numerical partial-funding input precedes the real third END, not
        // a retired fourth investment gate. Preserve exact2funded/4unfed.
        Apply("ready"); Apply("ready");
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
        MatchSnapshot received = Game.SnapshotPayload.DecodeForReceiving(Game.SnapshotPayload.Encode(match.Snapshot()));
        Assert.Contains(received.DyingBodies, u => u.Faction == Faction.Skeletons);
        Assert.Equal(Phase.Defeat, received.Phase); Assert.Equal(DefeatReason.AllCitiesFallen, received.DefeatReason);
        Assert.Equal(0, received.LastRewardedWave); Assert.Null(received.Players[0].LastReward);
        CombatEvent destroyed = Assert.Single(received.CombatEvents, e => e.Type == CombatEventType.StructureDestroyed && e.Structure?.Kind == ArenaStructureKind.Keep);
        Assert.Contains(received.CombatEvents, e => e.Type == CombatEventType.Death && e.Unit?.Faction == Faction.Skeletons && e.Tick == destroyed.Tick);
        PaidCampaignFixture.Legal(received);
    }
}
