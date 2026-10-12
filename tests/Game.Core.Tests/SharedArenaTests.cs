using Xunit;

namespace Game.Core.Tests;

// Tier A: new production topology, group contributions and admission transaction.
// Legacy city/size-budget tests cannot catch cross-sector occupancy or X/Y.
// Inputs are immutable, deterministic, engine-free; expected sub-second cost.
public sealed class SharedArenaTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void AdjacentSectorsAreConnectedAndStaticFootprintsNeverOverlapDeployment(int players)
    {
        var arena = new SharedArena(Enumerable.Range(1, players));
        ArenaState snapshot = arena.Snapshot();
        Assert.Equal(63 * players, arena.Board.Cells.Count);
        Assert.Equal(1, arena.Board.Capacity);
        Assert.Equal(players * 11, snapshot.Footprints.Select(f => f.Cell).Distinct().Count());
        foreach (ArenaSector sector in snapshot.Sectors)
        {
            Assert.Equal(6, sector.DeploymentCells.Length);
            Assert.All(sector.DeploymentCells, cell => Assert.True(arena.CanOccupy(cell)));
            Assert.All(snapshot.Footprints, footprint => Assert.False(arena.CanOccupy(footprint.Cell)));
            Assert.NotEmpty(arena.Route(sector.EnemyCells[0], arena.Keep(sector.Owner), 1, _ => true));
            if (players > 1)
                Assert.NotEmpty(arena.Route(sector.DeploymentCells[0], arena.Keep(sector.Owner % players + 1), 1, _ => true));
        }
        snapshot.Sectors[0].DeploymentCells[0] = -1;
        Assert.True(arena.CanDeploy(1, arena.Deployment(1, 0), 2));
        Assert.False(arena.CanDeploy(1, arena.Deployment(1, 2), 2));
    }

    [Theory]
    [InlineData(10, 0)]
    [InlineData(10, 3)]
    [InlineData(10, 10)]
    [InlineData(0, 3)]
    [InlineData(0, 0)]
    public void EveryContributorHitsOnePrimaryAndAllOtherMembersWithoutCaps(int x, int y)
    {
        CombatDecisionKey key = new(7, default, 1, 1, CombatActorKind.Unit, 1, 4, 0, CombatPurpose.Splash);
        int[] defenders = [11, 12, 13, 14, 15, 16];
        MemberStrike[] attackers = [new(1, new(x, y)), new(2, new(x, y)), new(3, new(x, y))];
        StrikeContribution[] result = SquadDamage.Contributions(attackers, defenders, key);
        Assert.Equal(3, result.Length);
        Assert.All(result, strike =>
        {
            Assert.Equal(6, strike.Damage.Length);
            Assert.Equal(x, strike.Damage.Single(d => d.MemberId == strike.PrimaryMemberId).Damage);
            Assert.All(strike.Damage.Where(d => d.MemberId != strike.PrimaryMemberId), d => Assert.Equal(y, d.Damage));
        });
        Assert.Equal(3 * (x + 5 * y), SquadDamage.Accumulate(result).Sum(d => d.Damage));
        Assert.Equal(result.Select(s => s.PrimaryMemberId), SquadDamage.Contributions(attackers.Reverse(), defenders.Reverse(), key).Select(s => s.PrimaryMemberId));
        Assert.Equal(x + 5 * y, SquadDamage.Accumulate(SquadDamage.Contributions(attackers.Take(1), defenders, key)).Sum(d => d.Damage));
    }

    [Fact]
    public void BossUsesOneContributorAndChillIsPopulationWeighted()
    {
        CombatDecisionKey key = new(1, default, 10, 1, CombatActorKind.Unit, 1, 1, 0, CombatPurpose.Splash);
        Assert.Equal(20, Assert.Single(SquadDamage.Accumulate(SquadDamage.Contributions([new(1, new(20, 5))], [2], key))).Damage);
        Assert.Equal(17, SquadDamage.AggregateChill([50, 0, 0]));
        Assert.Equal(25, SquadDamage.AggregateChill([50, 0]));
        Assert.Equal(0, SquadDamage.AggregateChill([]));
        Assert.Throws<ArgumentOutOfRangeException>(() => SquadDamage.Contributions([new(1, new(-1, 0))], [2], key));
    }

    [Fact]
    public void FriendlyExchangeCommitsTogetherAndUnrelatedMoveDoesNotWait()
    {
        var arena = new SharedArena([1]);
        int a = arena.Deployment(1, 0), b = arena.Deployment(1, 1), c = arena.Deployment(1, 3), d = arena.Deployment(1, 4);
        SquadMoveActor[] actors = [new(1, Faction.Adventurers, a, 10, 1), new(2, Faction.Adventurers, b, 10, 2), new(3, Faction.Adventurers, c, 10, 3)];
        SquadMoveCommit[] result = SquadMovement.Admit(arena, actors, [new(1, b, 27), new(2, a, 30), new(3, d, 12)], [], 5);
        Assert.Equal(3, result.Length);
        Assert.Equal(35, result.Single(m => m.Id == 1).EndTick);
        Assert.Equal(35, result.Single(m => m.Id == 2).EndTick);
        Assert.Equal(17, result.Single(m => m.Id == 3).EndTick);
        Assert.Equal(result.Single(m => m.Id == 1).Batch, result.Single(m => m.Id == 2).Batch);
        Assert.NotEqual(result.Single(m => m.Id == 1).Batch, result.Single(m => m.Id == 3).Batch);
        Assert.Equal(result, SquadMovement.Admit(arena, actors.Reverse(), [new(3, d, 12), new(2, a, 30), new(1, b, 27)], [], 5));
    }

    [Fact]
    public void ConflictsUseStableWinnerAndOpposingSwapOrUnmovingOccupantBlock()
    {
        var arena = new SharedArena([1]);
        int a = arena.Deployment(1, 0), b = arena.Deployment(1, 1), c = arena.Deployment(1, 2);
        SquadMoveActor[] actors = [new(1, Faction.Adventurers, a, 10, 2), new(2, Faction.Adventurers, c, 10, 1)];
        Assert.Equal(2, Assert.Single(SquadMovement.Admit(arena, actors, [new(1, b, 30), new(2, b, 30)], [], 0)).Id);
        Assert.Empty(SquadMovement.Admit(arena, actors, [new(1, b, 30)], [b], 0));
        SquadMoveActor[] opposed = [new(1, Faction.Adventurers, a, 10, 1), new(2, Faction.Skeletons, b, 10, 2)];
        Assert.Empty(SquadMovement.Admit(arena, opposed, [new(1, b, 30), new(2, a, 30)], [], 0));
        Assert.Empty(SquadMovement.Admit(arena, opposed, [new(1, b, 30)], [], 0));
        // Logical death removes the actor from admission; no body-duration lock.
        Assert.Single(SquadMovement.Admit(arena, opposed.Take(1), [new(1, b, 30)], [], 0));
    }
}
