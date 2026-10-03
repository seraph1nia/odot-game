using Xunit;

namespace Game.Core.Tests.Profiling;

public sealed class LargeBattleFixtureTests
{
    [Fact]
    public void AuthoredStressBoardHasReciprocalNeighborsAndProtectedRows()
    {
        var board = new HexBoard(LargeBattle.Board());
        Assert.Equal(256, board.Cells.Count); Assert.Equal(6, board.Capacity); Assert.Equal(6, board.Anchors.Count);
        Assert.All(board.Cells, c => Assert.All(c.Neighbors, n => Assert.Contains(c.Id, board.Cell(n).Neighbors)));
        foreach (Faction faction in Enum.GetValues<Faction>())
        {
            Assert.Equal(32, board.Rear(faction).Count); Assert.Equal(32, board.Front(faction).Count);
            Assert.All(board.Rear(faction), cell => Assert.False(board.Allows(faction == Faction.Adventurers ? Faction.Skeletons : Faction.Adventurers, cell)));
        }
    }
    [Theory]
    [InlineData(128)]
    [InlineData(512)]
    [InlineData(2048)]
    public void SyntheticArmiesUseOrdinaryProfilesAndBalancedStableIdentities(int size)
    {
        using Match match = LargeBattle.Create(size, 1);
        CombatUnit[] units = match.Combat.Units();
        Assert.Equal(size, units.Length);
        Assert.Equal(Enumerable.Range(1, size), units.Select(u => u.Id));
        foreach (Faction faction in Enum.GetValues<Faction>())
            foreach (UnitType role in Enum.GetValues<UnitType>())
                Assert.Equal(size / 8, units.Count(u => u.Faction == faction && u.Type == role));
        Assert.All(units, u => { Assert.Equal(1, u.Identity.Level); Assert.Equal(2, u.Profile.Size); Assert.Equal(match.Combat.Profile(u.Type), u.Profile); Assert.NotEqual(UnitLifecycle.Reserve, u.Location.Lifecycle); });
        if (size == 2048)
        {
            Assert.Equal(192, units.Count(u => u.IsTargetable && u.Faction == Faction.Adventurers));
            Assert.Equal(192, units.Count(u => u.IsTargetable && u.Faction == Faction.Skeletons));
            Assert.Contains(units, u => u.Location.Lifecycle == UnitLifecycle.Queued);
        }
    }
}
