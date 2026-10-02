using Xunit;

namespace Game.Core.Tests;

public sealed class CombatActionTests
{
    [Fact]
    public void AttackHasOneLockedTargetAndExactWindupRecoveryDeadlines()
    {
        var target = new CombatTarget(CombatTargetKind.Unit, 7);
        CombatAction.Windup attack = CombatActions.Attack(new CombatAction.Waiting(), target, 100, 12, 48);
        Assert.Equal(1, attack.Sequence); Assert.Equal(1, attack.AttackSequence);
        Assert.Same(target, attack.Attack!.Target); Assert.Equal(112, attack.Attack.ImpactTick); Assert.Equal(160, attack.ReadyTick);
        Assert.Throws<InvalidOperationException>(() => CombatActions.Impact(attack, 111, true));
        CombatAction.Recovery miss = CombatActions.Impact(attack, 112, false);
        Assert.False(miss.Attack!.Landed); Assert.Equal(attack.Sequence, miss.Sequence); Assert.Equal(160, miss.ReadyTick);
        Assert.Throws<InvalidOperationException>(() => CombatActions.Complete(miss, 159));
        CombatAction.Waiting ready = CombatActions.Complete(miss, 160);
        Assert.Equal(UnitActionKind.Waiting, ready.Kind); Assert.Equal(2, CombatActions.Attack(ready, target, 160, 12, 48).AttackSequence);
    }

    [Fact]
    public void MoveCompletionPreservesHistoryAndCannotCompleteBeforeArrival()
    {
        var board = new HexBoard(HexBoardDefinition.Default());
        HexTransition transition = board.Transition(new(11, 1), new(8, 1));
        CombatAction.Moving move = CombatActions.Move(new CombatAction.Waiting(), transition, 100, 30);
        Assert.Equal(130, move.EndTick); Assert.Equal(transition.Destination, move.Destination);
        Assert.Throws<InvalidOperationException>(() => CombatActions.Complete(move, 129));
        Assert.Equal(move.Sequence, CombatActions.Complete(move, 130).Sequence);
    }

    [Fact]
    public void CancellationPreservesRecoveryAndOrdinalButCancelsThePrimary()
    {
        CombatAction.Windup attack = CombatActions.Attack(new CombatAction.Waiting(), new(CombatTargetKind.City, 2), 100, 12, 48);
        CombatAction cancelled = CombatActions.Cancel(attack, 105);
        Assert.IsType<CombatAction.Recovery>(cancelled); Assert.Equal(160, cancelled.ReadyTick);
        Assert.Null(cancelled.Attack); Assert.Equal(attack.AttackSequence, cancelled.AttackSequence);
        Assert.Throws<InvalidOperationException>(() => CombatActions.Complete(cancelled, 159));
        Assert.IsType<CombatAction.Waiting>(CombatActions.Complete(cancelled, 160));
    }

    [Fact]
    public void InvalidActionsAndOverlappingSchedulingAreRejected()
    {
        Assert.Throws<ArgumentException>(() => new CombatTarget(CombatTargetKind.Unit, 0));
        Assert.Throws<ArgumentException>(() => new CombatAction.Windup(1, 1, 20, null!));
        Assert.Throws<ArgumentException>(() => new CombatAction.Windup(1, 1, 20, new(null!, 10, 12)));
        Assert.Throws<ArgumentException>(() => new CombatAction.Moving(1, new(8, 1), 1, 10, 10));
        Assert.Throws<ArgumentException>(() => new CombatAction.Windup(1, 1, 20, new(new(CombatTargetKind.Unit, 1), 10, 10)));
        Assert.Throws<ArgumentException>(() => new CombatAction.Windup(1, 1, 20, new(new(CombatTargetKind.Unit, 1), 10, 20)));
        Assert.Throws<ArgumentOutOfRangeException>(() => CombatActions.Attack(new CombatAction.Waiting(), new(CombatTargetKind.Unit, 1), 10, 0, 10));
        CombatAction.Windup attack = CombatActions.Attack(new CombatAction.Waiting(), new(CombatTargetKind.Unit, 1), 10, 12, 48);
        Assert.Throws<InvalidOperationException>(() => CombatActions.Attack(attack, new(CombatTargetKind.Unit, 2), 11, 12, 48));
    }

    [Fact]
    public void FixtureBoundaryRejectsCompetingActionRepresentationsAtomically()
    {
        using var combat = new CombatSimulation(new());
        int id = CombatDecisionRegressionTests.At(combat, Faction.Adventurers, 17);
        UnitState before = combat.Read(id);
        CombatReservations reservations = combat.Reservations;
        Assert.Throws<ArgumentException>(() => combat.Seed(before with { PendingImpact = true, TargetId = 2, AttackSequence = 1, ImpactTick = 12, ReadyTick = 60 }));
        Assert.Equal(before.Hex, combat.Read(id).Hex);
        Assert.Equal(reservations.Positions, combat.Reservations.Positions);
        Assert.Equal(reservations.Transit, combat.Reservations.Transit);
    }
}
