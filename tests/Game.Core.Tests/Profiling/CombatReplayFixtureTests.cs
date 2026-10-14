using DevRunner;
using Xunit;

namespace Game.Core.Tests.Profiling;

[Trait("Tier", "Deep")]
public sealed class CombatReplayFixtureTests
{
    [Fact]
    public void FixedReplayIncludesPauseMovementImpactsDeathsAndHiddenCityReturn()
    {
        CombatReplayInput input = CombatReplayFixture.Generate();
        Assert.Equal(600, input.Frames.Length); Assert.Equal(input.Digest, CombatReplayFixture.Generate().Digest);
        Assert.Equal(2, input.Commands);
        var arena = new SharedArena(input.Frames[0].Snapshot!.Players.Select(c => c.Id));
        foreach (CityState city in input.Frames[0].Snapshot!.Players)
        {
            Assert.Equal(2, city.Army!.PurchasedHomes); Assert.Equal(6, city.Soldiers.Length);
            Assert.Equal(2, city.Soldiers.Select(u => u.Assignment!.Tile).Distinct().Count());
            Assert.All(city.Soldiers, u => Assert.Equal(new HexPosition(arena.Deployment(city.Id, u.Assignment!.Tile), 1), u.Hex!.Position));
            Assert.All(city.Soldiers.GroupBy(u => u.Assignment!.Tile), tile => Assert.Equal(city.Army!.SquadCapacity, tile.Count()));
        }
        Assert.All(input.Frames, frame => Assert.Equal(1.0 / 60, frame.Delta));
        Assert.Equal(2, input.Frames[240].Focus); Assert.Equal(1, input.Frames[360].Focus);
        Assert.True(input.Frames[150].Snapshot!.Paused); Assert.False(input.Frames[170].Snapshot!.Paused);
        MatchSnapshot[] states = input.Frames.Where(f => f.Snapshot is not null).Select(f => f.Snapshot!).ToArray();
        foreach (MatchSnapshot state in states)
        {
            SnapshotCompatibility.RequireSharedArena(state);
            CombatReservations claims = CombatReservations.Reconstruct(state.Arena!, state.Squads);
            Assert.Equal(state.Reservations.Positions, claims.Positions); Assert.Equal(state.Reservations.Transit, claims.Transit);
        }
        Assert.Contains(states, s => s.Squads.Any(u => u.Hex.HoldsTransit));
        Assert.Contains(states.SelectMany(s => s.CombatEvents), e => e.Type == CombatEventType.Impact && e.Landed);
        Assert.Contains(states.SelectMany(s => s.CombatEvents), e => e.Type == CombatEventType.Death);
        Assert.Contains(states.SelectMany(s => s.DyingBodies), u => u.Hex!.DeathEndTick < input.FinalTick - 3);
        var playback = new CombatPlayback();
        var expired = new HashSet<int>();
        foreach (CombatReplayFrame frame in input.Frames)
        {
            if (frame.Snapshot is { } state) playback.Accept(state);
            playback.Advance(frame.Delta, true);
            UnitState[] units = playback.Units();
            Assert.DoesNotContain(units, u => u.Hex is { Lifecycle: UnitLifecycle.Dying } death && death.DeathEndTick <= playback.Tick);
            foreach (CombatEvent entry in playback.Drain())
                if (entry.Type == CombatEventType.Death) expired.Add(entry.Unit!.Id);
        }
        Assert.NotEmpty(expired);
    }
}
