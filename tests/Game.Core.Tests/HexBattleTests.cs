using Xunit;

namespace Game.Core.Tests;

// Replacement for individual-anchor/city-entry combat. Tier A uses real squad
// actions and persistent members. Existing SharedArena/SquadReservation/CombatPolicy
// suites own conflict cycles, malformed claims, static routes and bounded progress.
public sealed class HexBattleTests
{
    [Fact]
    public void TargetGroupIsCommittedUntilRecoveryThenNearerCurrentGroupWins()
    {
        using var combat = new CombatSimulation(new(), seed: 123);
        int actor = CombatFixture.Group(combat, UnitType.Crossbowman, 1, 1, Faction.Adventurers, 17)[0];
        int original = CombatFixture.Group(combat, UnitType.Mage, 1, 0, Faction.Skeletons, 18)[0];
        int incoming = CombatFixture.Group(combat, UnitType.Swordsman, 1, 0, Faction.Skeletons, 3)[0];
        foreach (int id in new[] { original, incoming }) combat.Seed(combat.Inspect(id) with { Action = new CombatAction.Recovery(0, 0, 1000) });
        combat.Step(1, []);
        int committed = combat.Read(original).SquadId;
        Assert.Equal(committed, combat.SquadSnapshots().Single(s => s.Owner == 1).TargetId);
        combat.Seed(combat.Inspect(original) with { Location = new(UnitLifecycle.Alive, new(1, 1)) });
        combat.Seed(combat.Inspect(incoming) with { Location = new(UnitLifecycle.Alive, new(18, 1)) });
        for (int tick = 2; tick <= 60; tick++) combat.Step(tick, []);
        Assert.Equal(committed, combat.SquadSnapshots().Single(s => s.Owner == 1).TargetId);
        Assert.Equal(1, combat.Read(actor).AttackSequence);
        combat.Step(61, []);
        Assert.Equal(combat.Read(incoming).SquadId, combat.SquadSnapshots().Single(s => s.Owner == 1).TargetId);
        Assert.Equal(2, combat.Read(actor).AttackSequence);
    }

    [Theory]
    [InlineData(129, true)]
    [InlineData(130, false)]
    public void CommittedImpactUsesExclusiveSourceUntilExactSquadArrival(int impactTick, bool landed)
    {
        using var combat = new CombatSimulation(new() { RangedHealth = 10, SoldierDamage = 4 });
        int[] group = CombatFixture.Group(combat, UnitType.Crossbowman, 3, 1, Faction.Adventurers, 17);
        int attacker = CombatFixture.Group(combat, UnitType.Swordsman, 1, 0, Faction.Skeletons, 18)[0];
        CombatUnit mover = combat.Inspect(group[0]);
        combat.Seed(mover with { Action = CombatActions.Move(mover.Action, combat.Board.Transition(new(17, 1), new(16, 1)), 100, 30) });
        CombatUnit strike = combat.Inspect(attacker);
        combat.Seed(strike with { Action = CombatActions.Attack(strike.Action, new(CombatTargetKind.Unit, mover.SquadId), impactTick - 12, 12, 48) });
        combat.Advance(impactTick, []);
        CombatEvent impact = Assert.Single(combat.Events(), e => e.Type == CombatEventType.Impact);
        Assert.Equal(landed, impact.Landed);
        Assert.Equal(landed ? 2600 : 3000, group.Sum(id => combat.Read(id).Health));
        Assert.All(group, id => Assert.Equal(landed ? 17 : 16, combat.Read(id).Hex!.Position.Cell));
        Assert.Equal(2, combat.Reservations.Positions.Length); // two actors, never three member leases
        Assert.Equal(landed, combat.Reservations.Transit.Any(t => t.UnitId == mover.SquadId));
        CombatReservations rebuilt = CombatReservations.Reconstruct(combat.Arena, combat.SquadSnapshots());
        Assert.Equal(combat.Reservations.Positions, rebuilt.Positions); Assert.Equal(combat.Reservations.Transit, rebuilt.Transit);
    }

    [Fact]
    public void DeathRetainsAnImmutableTargetPoseButReleasesItsActorCellImmediately()
    {
        using var combat = new CombatSimulation(new());
        int actor = CombatFixture.Group(combat, UnitType.Swordsman, 1, 1, Faction.Adventurers, 17)[0];
        int target = CombatFixture.Group(combat, UnitType.Crossbowman, 1, 0, Faction.Skeletons, 18)[0];
        combat.Seed(combat.Inspect(actor) with { Health = 100 });
        for (int tick = 1; tick <= 19; tick++) combat.Step(tick, []);
        UnitState corpse = Assert.Single(combat.Dying()); Assert.Equal(actor, corpse.Id);
        HexPosePoint aim = Assert.IsType<HexPosePoint>(corpse.Hex!.FrozenAim);
        Assert.Equal(new HexPosition(18, 1), aim.Position); Assert.Equal(0, aim.Transition);
        Assert.DoesNotContain(combat.Reservations.Positions, p => p.UnitId == corpse.SquadId);
        combat.Seed(combat.Inspect(target) with { Location = new(UnitLifecycle.Alive, new(10, 1)) });
        combat.Advance(20, []);
        Assert.Equal(aim, Assert.Single(combat.Dying()).Hex!.FrozenAim);
        Assert.Equal(corpse.Hex.DeathStartTick + corpse.Profile.DeathTicks, corpse.Hex.DeathEndTick);
        combat.Cleanup(corpse.Hex.DeathEndTick); Assert.Empty(combat.Dying());
    }

    [Fact]
    public void MovingCasualtyFacesItsActualWatchpostObjectiveWithoutHoldingTransit()
    {
        using var combat = new CombatSimulation(new());
        int actor = CombatFixture.Group(combat, UnitType.Swordsman, 1, 1, Faction.Adventurers, 24)[0];
        CombatUnit member = combat.Inspect(actor);
        int post = combat.Arena.Footprints.Single(f => f.Kind == ArenaStructureKind.Watchpost).Cell;
        combat.Seed(member with
        {
            Health = 100,
            Statuses = StatusPolicy.Apply(new(), new(actor, StatusKind.Poison, 99, 1, 0, 100), new()),
            Action = CombatActions.Move(member.Action, combat.Board.Transition(new(24, 1), new(25, 1)), 50, 30),
            Decision = member.Decision with { ObjectiveId = 1, ObjectiveCity = true, ObjectiveCell = post }
        });
        combat.Advance(60, []);
        UnitState corpse = Assert.Single(combat.Dying());
        Assert.Equal(new HexPosition(post, 1), Assert.IsType<HexPosePoint>(corpse.Hex!.FrozenAim).Position);
        Assert.Equal(10, corpse.Hex.FrozenMoveTicks);
        Assert.Empty(combat.Reservations.Positions); Assert.Empty(combat.Reservations.Transit);
    }

    [Fact]
    public void LethalImpactAtRecoveryBoundaryCannotStartAnotherSquadCycle()
    {
        using var combat = new CombatSimulation(new());
        int actor = CombatFixture.Group(combat, UnitType.Swordsman, 1, 1, Faction.Adventurers, 17)[0];
        int enemy = CombatFixture.Group(combat, UnitType.Swordsman, 1, 0, Faction.Skeletons, 18)[0];
        CombatUnit recovering = combat.Inspect(actor), striking = combat.Inspect(enemy);
        combat.Seed(recovering with { Health = 100, Action = new CombatAction.Recovery(1, 1, 61) });
        combat.Seed(striking with { Action = CombatActions.Attack(striking.Action, new(CombatTargetKind.Unit, recovering.SquadId), 49, 12, 48) });
        combat.Step(61, []);
        UnitState corpse = Assert.Single(combat.Dying()); Assert.Equal(actor, corpse.Id); Assert.Equal(1, corpse.AttackSequence);
        Assert.DoesNotContain(combat.Events(), e => e.Type == CombatEventType.AttackStarted && e.SquadId == recovering.SquadId);
        Assert.DoesNotContain(combat.Reservations.Positions, p => p.UnitId == recovering.SquadId);
    }

    [Fact]
    public void PhysicalMeleeScreenInterceptsPressureAndProtectsRangedSupport()
    {
        (int Health, int FirstTarget, int Screen, int Support, long FirstShot) Run(bool screened)
        {
            using var combat = new CombatSimulation(new(), seed: 123);
            int enemy = CombatFixture.Group(combat, UnitType.Swordsman, 1, 0, Faction.Skeletons, 10)[0];
            int[] screen = CombatFixture.Group(combat, UnitType.Swordsman, 3, 1, Faction.Adventurers, 17);
            int support = CombatFixture.Group(combat, UnitType.Crossbowman, 1, 1, Faction.Adventurers, 24)[0];
            int screenId = combat.Read(screen[0]).SquadId;
            if (!screened) foreach (int id in screen) combat.Remove(id);
            combat.Step(1, []);
            int first = combat.Read(enemy).Decision!.ObjectiveId;
            for (int tick = 2; tick <= 90; tick++) combat.Step(tick, []);
            long shot = combat.Events().First(e => e.Type == CombatEventType.Impact && e.Unit?.Id == support && e.Landed).Tick;
            return (combat.Read(support).Health, first, screenId, combat.Read(support).SquadId, shot);
        }
        var protectedRun = Run(true); var exposed = Run(false);
        Assert.Equal(protectedRun.Screen, protectedRun.FirstTarget);
        Assert.Equal(exposed.Support, exposed.FirstTarget);
        Assert.Equal(protectedRun.FirstShot, exposed.FirstShot); // no invented cadence bonus
        Assert.True(protectedRun.Health > exposed.Health);
    }
}
