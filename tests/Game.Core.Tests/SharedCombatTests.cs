using Xunit;

namespace Game.Core.Tests;

// Tier A production authority regressions; old individual actor/city shield tests
// cannot prove shared group impacts. Explicit seeded member fixtures isolate the
// combat seam; paid commands below exercise the real economy/command boundary.
public sealed class SharedCombatTests
{
    private static int[] Group(CombatSimulation combat, UnitType type, int count, int owner, Faction faction, int cell)
    {
        int[] ids = Enumerable.Range(0, count).Select(_ => combat.Create(type, owner, 1, 1, faction,
            assignment: faction == Faction.Adventurers ? new(-1, 0, 16, 1) : null)).ToArray();
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
}
