using Game.Core;
using Xunit;

namespace Game.Core.Tests;

public sealed class WorldTests
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(1000, 1000)]
    [InlineData(float.MaxValue, float.MaxValue)]
    public void InputCannotExceedMovementSpeed(float x, float y)
    {
        var world = new World(initialCoin: new(700, 400));
        world.AddPlayer(2, new(200, 200));
        world.SetInput(2, new(x, y));
        world.Step();
        double distance = (world.Players[2].Position - new Point(200, 200)).Length;
        Assert.InRange(distance, 2.99, 3.001);
    }

    [Theory]
    [InlineData(float.NaN, 0)]
    [InlineData(0, float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity, 1)]
    public void InvalidInputStopsMovement(float x, float y)
    {
        var world = new World();
        world.AddPlayer(2);
        Point start = world.Players[2].Position;
        world.SetInput(2, new(1, 0));
        Assert.False(world.SetInput(2, new(x, y)));
        world.Step();
        Assert.Equal(start, world.Players[2].Position);
    }

    [Fact]
    public void MovementStaysInRoomAndRemovedPlayersCannotMove()
    {
        var world = new World();
        world.AddPlayer(2, new(799, 479));
        world.SetInput(2, new(1, 1));
        for (int i = 0; i < 120; i++) world.Step();
        Assert.Equal(new Point(788, 468), world.Players[2].Position);
        world.SetInput(2, new(-1, -1));
        for (int i = 0; i < 500; i++) world.Step();
        Assert.Equal(new Point(12, 12), world.Players[2].Position);
        world.RemovePlayer(2);
        Assert.Empty(world.Players);
        Assert.False(world.SetInput(2, new(1, 0)));
    }

    [Fact]
    public void SimultaneousPickupAwardsExactlyOnePointAndRespawnsOnce()
    {
        var world = new World(initialCoin: new(300, 200));
        world.AddPlayer(7, new(300, 200));
        world.AddPlayer(2, new(300, 200));
        world.Step();
        Assert.Equal(1, world.Players[2].Score);
        Assert.Equal(0, world.Players[7].Score);
        Assert.Equal(2, world.CoinGeneration);
        Assert.InRange(world.Coin.X, 9, 791);
        Assert.InRange(world.Coin.Y, 9, 471);
        Assert.All(world.Players.Values, p => Assert.True((p.Position - world.Coin).Length > World.PlayerRadius + World.CoinRadius));
        world.Step();
        Assert.Equal(1, world.Players.Values.Sum(p => p.Score));
        Assert.Equal(2, world.CoinGeneration);
    }

    [Fact]
    public void SinglePickupAndSessionRemovalUpdateScore()
    {
        var world = new World(initialCoin: new(300, 200));
        world.AddPlayer(2, new(300, 200));
        world.Step();
        Assert.Equal(1, world.Snapshot().Players.Single().Score);
        world.RemovePlayer(2);
        world.AddPlayer(2);
        Assert.Equal(0, world.Players[2].Score);
    }

    [Fact]
    public void FixedSeedAndInputSequenceProduceIdenticalState()
    {
        var a = new World(128);
        var b = new World(128);
        a.AddPlayer(2); b.AddPlayer(2);
        for (int i = 0; i < 600; i++)
        {
            Point direction = (a.Coin - a.Players[2].Position).Limited();
            a.SetInput(2, direction); b.SetInput(2, direction);
            a.Step(); b.Step();
            Assert.Equal(a.Coin, b.Coin);
            Assert.Equal(a.CoinGeneration, b.CoinGeneration);
            Assert.Equal(a.Snapshot().Players, b.Snapshot().Players);
        }
        Assert.True(a.Players[2].Score > 0);
    }
}
