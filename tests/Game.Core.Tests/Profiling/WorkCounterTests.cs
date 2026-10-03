using Xunit;

namespace Game.Core.Tests.Profiling;

public sealed class WorkCounterTests
{
    [Fact]
    public void UnavailableZeroResetAndOwnershipAreDistinct()
    {
        var first = new WorkCounters(); var second = new WorkCounters();
        first.Support(WorkMetric.WorldViews); second.Support(WorkMetric.WorldViews);
        Assert.Null(first.Snapshot()[WorkMetric.FullPoseSamples]); Assert.Equal(0, first.Snapshot()[WorkMetric.WorldViews]);
        first.Add(WorkMetric.WorldViews, 2); Assert.Equal(2, first.Snapshot()[WorkMetric.WorldViews]); Assert.Equal(0, second.Snapshot()[WorkMetric.WorldViews]);
        first.Reset(); Assert.Equal(0, first.Snapshot()[WorkMetric.WorldViews]);
        Assert.Throws<InvalidOperationException>(() => first.Add(WorkMetric.FullPoseSamples));
        Assert.Throws<ArgumentOutOfRangeException>(() => first.Add(WorkMetric.WorldViews, -1));
    }
    [Fact]
    public void AWholeViewVisitsEachStoredUnitAndProjectsEachLivingUnit()
    {
        using var match = new Match(combatSeed: 1);
        var work = new WorkCounters(); match.SetWorkCounters(work);
        match.Combat.Create(UnitType.Swordsman, 1, 1, 1);
        match.Combat.Create(UnitType.Mage, 0, 1, 1, Faction.Skeletons);
        work.Reset(); Assert.Equal(2, match.Combat.Snapshot().Length);
        Assert.Equal(1, work.Snapshot()[WorkMetric.WorldViews]); Assert.Equal(2, work.Snapshot()[WorkMetric.UnitVisits]);
        Assert.Equal(2, work.Snapshot()[WorkMetric.UnitProjections]); Assert.Equal(1, work.Snapshot()[WorkMetric.Sorts]);
        Assert.Equal(0, work.Snapshot()[WorkMetric.RangeCandidates]);
    }
    [Fact]
    public void KnownTwoCellSearchAndCodecSitesHaveCheckableCounts()
    {
        var work = new WorkCounters();
        work.Support(WorkMetric.BfsSearches, WorkMetric.BfsDequeues, WorkMetric.BfsEdges);
        var board = new HexBoard(HexBoardDefinition.Default());
        var search = new HexReachability(board, 1, cell => cell == 2, work);
        Assert.Equal(2, search.Distances.Count); Assert.Equal(1, search.Distances[2]);
        Assert.Equal(1, work.Snapshot()[WorkMetric.BfsSearches]); Assert.Equal(2, work.Snapshot()[WorkMetric.BfsDequeues]); Assert.Equal(7, work.Snapshot()[WorkMetric.BfsEdges]);
        using var match = new Match(combatSeed: 1);
        MatchSnapshot snapshot = match.Snapshot();
        string payload = Game.SnapshotPayload.Encode(snapshot, work);
        Assert.Equal(LargeBattle.Digest(snapshot), LargeBattle.Digest(Game.SnapshotPayload.Decode(payload, work)));
        Assert.Equal(1, work.Snapshot()[WorkMetric.CodecEncodes]); Assert.Equal(1, work.Snapshot()[WorkMetric.CodecDecodes]);
        Assert.Equal(Convert.FromBase64String(payload).Length, work.Snapshot()[WorkMetric.CompressedBytes]);
        Assert.Equal(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(snapshot, WireJson.Options).Length, work.Snapshot()[WorkMetric.JsonBytes]);
    }
    [Fact]
    public void CountersDoNotChangeOrderedStateOrEvents()
    {
        static string Replay(bool enabled)
        {
            using Match match = LargeBattle.Create(128, 8, enabled ? new WorkCounters() : null);
            for (int tick = 0; tick < 90; tick++) match.Step();
            return LargeBattle.Digest(match.Snapshot() with { MatchId = "session" });
        }
        Assert.Equal(Replay(false), Replay(true));
    }
}
