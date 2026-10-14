using System.IO.Compression;
using System.Text.Json;
using Game;
using Xunit;

namespace Game.Core.Tests;

// Tier A admission: the engine cannot cheaply prove fixed-tick equivalence,
// malformed historical snapshots or preview/spawn identity. Seeded numerical
// sessions, explicit wall slices and no children; estimated <1s per case.
public sealed class BattleFlowTests
{
    private static CommandResult Act(Match match, string action, int city = 1)
        => match.Apply(city, Command.FromSnapshot(match.Snapshot(), 1, action, city));
    private static void Start(AuthoritySession session)
    {
        long sequence = 0;
        void Act(string action) => Assert.True(session.ExecuteLocal(Command.FromSnapshot(session.Snapshot(), ++sequence, action, 1)).Accepted);
        Act("start"); Act("ready"); Act("ready"); Act("ready");
        Assert.Equal(Phase.Combat, session.Phase);
    }
    private static string Combat(MatchSnapshot state) => JsonSerializer.Serialize(state with { Revision = 0, CombatSpeed = CombatSpeed.One }, WireJson.Options);

    [Fact]
    public void ExactlyThreePreparationsPreserveOperationsAndPerProductionValues()
    {
        using var match = new Match(combatSeed: 123); match.Join();
        Assert.True(Act(match, "start").Accepted);
        Assert.Equal(Phase.Building, match.Phase); Assert.Equal(1, match.Turn);
        Assert.False(Act(match, "build").Accepted); // No selected plot.
        Assert.True(match.Apply(1, Command.FromSnapshot(match.Snapshot(), 1, "build", 1, 0, Building.Farm)).Accepted);
        for (int turn = 1; turn <= 3; turn++)
        {
            Assert.Equal(turn, match.Turn);
            Assert.Equal(turn == 3 ? Phase.Preparation : Phase.Building, match.Phase);
            Assert.Equal(turn - 1, match.ProductionCount);
            Assert.Contains("> Preparation " + turn, ProgressionPresentation.PhaseRows(match.Snapshot()));
            Assert.True(Act(match, "unready").Accepted);
            Command old = Command.FromSnapshot(match.Snapshot(), 1, "ready", 1);
            Assert.True(match.Apply(1, old).Accepted);
            Assert.False(match.Apply(1, old).Accepted);
        }
        Assert.Equal(Phase.Combat, match.Phase);
        Assert.Equal(3, match.ProductionCount); Assert.Equal(18, match.Players[1].Gold); Assert.Equal(15, match.Players[1].Food);
        Assert.Equal(["  Preparation 1", "  Preparation 2", "  Preparation 3", "> Combat"], ProgressionPresentation.PhaseRows(match.Snapshot()));
    }

    [Fact]
    public void FinalProductionPreflightsIncomeProgressAndRewardWithoutPartialSettlement()
    {
        using var match = new Match(combatSeed: 123); match.Join();
        Assert.True(Act(match, "start").Accepted); Assert.True(Act(match, "ready").Accepted); Assert.True(Act(match, "ready").Accepted);
        match.Players[1].Gold = int.MaxValue - 1;
        string before = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
        Assert.False(Act(match, "ready").Accepted);
        Assert.Equal(before, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
        Assert.Equal(2, match.ProductionCount); Assert.False(match.Players[1].Ready);
        match.Players[1].Gold = 12; match.Players[1].Stone = 2;
        Assert.True(match.Apply(1, Command.FromSnapshot(match.Snapshot(), 1, "build", 1, 0, Building.ResearchTower)).Accepted);
        match.Players[1].Research = new(int.MaxValue, 2);
        before = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
        Assert.False(Act(match, "ready").Accepted);
        Assert.Equal(before, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
        match.Players[1].Research = default;
        match.Players[1].Gold = int.MaxValue - 2;
        before = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
        Assert.False(Act(match, "ready").Accepted);
        Assert.Equal(before, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
    }

    [Theory]
    [InlineData(AuthorityPolicy.PlayingHost)]
    [InlineData(AuthorityPolicy.Dedicated)]
    public void FinalStonecutterSettlesOnceAcrossRetryCodecAndReconnect(AuthorityPolicy policy)
    {
        using var flow = new SessionFlow(policy);
        flow.Act(1, new("start"));
        flow.Act(2, new("build", 0, Building.Lumbermill));
        for (int preparation = 1; preparation <= 2; preparation++)
        {
            flow.Act(1, new("ready")); flow.Act(2, new("ready"));
            Assert.Equal(preparation, flow.State.ProductionCount);
        }
        flow.Act(2, new("build", 1, Building.Stonecutter));
        Assert.Equal(0, flow.State.Players[1].Stone);
        flow.Disconnect(2); flow.Rebind(2, 12);
        Assert.Equal(2, SessionFlow.RoundTrip(flow.State).ProductionCount);
        flow.Act(1, new("ready"));
        Command final = flow.Act(2, new("ready"));
        MatchSnapshot entered = SessionFlow.RoundTrip(flow.State);
        Assert.Equal(Phase.Combat, entered.Phase); Assert.Equal(3, entered.ProductionCount);
        Assert.Equal(1, entered.Players[1].Stone);
        Assert.True(flow.Send(2, final).Accepted);
        flow.Disconnect(2); flow.Rebind(2, 13);
        Assert.True(flow.Send(2, final).Accepted);
        Assert.Equal(SessionFlow.Gameplay(entered), SessionFlow.Gameplay(flow.State));
    }

    [Fact]
    public void FinalFarmForecastMatchesSettlementAndRejectsOverflowWithoutInventingFunding()
    {
        using var match = new Match(); match.Join();
        Assert.True(Act(match, "start").Accepted);
        Assert.True(match.Apply(1, Command.FromSnapshot(match.Snapshot(), 1, "build", 1, 1, Building.Barracks)).Accepted);
        Assert.True(match.Apply(1, Command.FromSnapshot(match.Snapshot(), 2, "build", 1, 2, Building.MetalMine)).Accepted);
        Assert.True(Act(match, "ready").Accepted); Assert.True(Act(match, "ready").Accepted);
        for (int i = 0; i < 4; i++) Assert.True(match.Apply(1, Command.FromSnapshot(match.Snapshot(), i + 3, "recruit", 1, 1)).Accepted);
        Assert.True(match.Apply(1, Command.FromSnapshot(match.Snapshot(), 7, "build", 1, 0, Building.Farm)).Accepted);
        MatchSnapshot prepared = SessionFlow.RoundTrip(match.Snapshot());
        CityState city = prepared.Players[0]; BattleFoodForecast forecast = city.FoodForecast!;
        Assert.Equal(0, city.Food); Assert.Equal(5, forecast.Available); Assert.Equal(4, forecast.Paid);
        Assert.Equal(city.Soldiers.Select(u => u.Id), forecast.Funded); Assert.Equal(forecast.Funded, forecast.Participating); Assert.Empty(forecast.Unfed);
        Assert.Equal("1", ProgressionPresentation.Economy(prepared, city, true).BalanceValue);
        ProgressionView details = ProgressionPresentation.Describe(prepared, city);
        Assert.Contains("4 fed/0 reserve", details.Food); Assert.DoesNotContain("next: reserve", details.Army);
        match.Players[1].Gold = int.MaxValue;
        MatchSnapshot blocked = match.Snapshot(); Assert.Null(blocked.Players[0].FoodForecast);
        Assert.Equal("Unavailable", ProgressionPresentation.Economy(blocked, blocked.Players[0], true).UpkeepValue);
        Assert.Contains("unavailable", ProgressionPresentation.Describe(blocked, blocked.Players[0]).Food);
        Assert.Contains("next: unavailable", ProgressionPresentation.Describe(blocked, blocked.Players[0]).Army);
        string unchanged = JsonSerializer.Serialize(blocked, WireJson.Options);
        Assert.False(Act(match, "ready").Accepted); Assert.Equal(unchanged, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
        match.Players[1].Gold = city.Gold;
        Assert.True(Act(match, "ready").Accepted);
        CityState entered = match.Snapshot().Players[0]; BattleUpkeepReceipt paid = entered.LastUpkeep!;
        Assert.Equal(forecast.Paid, paid.Paid); Assert.Equal(forecast.Funded, paid.Funded);
        Assert.Equal(forecast.Participating, paid.Participating); Assert.Equal(forecast.Unfed, paid.Unfed);
        Assert.Equal(forecast.Available - forecast.Paid, entered.Food);
    }

    [Theory]
    [InlineData(CombatSpeed.One, 4)]
    [InlineData(CombatSpeed.OneAndHalf, 8.0 / 3)]
    [InlineData(CombatSpeed.Two, 2)]
    public void PlaybackMovementAndEventTimeFollowTheAcceptedGlobalRate(CombatSpeed speed, double wallTicks)
    {
        using var match = new Match();
        var playback = new CombatPlayback();
        MatchSnapshot state = match.Snapshot() with { Phase = Phase.Combat, CombatSpeed = speed };
        playback.Accept(state);
        playback.Accept(state with { Revision = 1, Tick = 3 });
        playback.Advance(wallTicks / 60, true);
        Assert.Equal(2, playback.Tick, 10);
        Assert.Equal(2.0 / 60, playback.VisualSeconds, 10);
        playback.Advance(.1, false); Assert.Equal(2, playback.Tick, 10);
    }

    [Fact]
    public void RatesAdmitExactWallTimeAndProduceIdenticalSeededCombatAtEquivalentTicks()
    {
        string? baseline = null;
        foreach ((CombatSpeed speed, int wallFrames) in new[] { (CombatSpeed.One, 240), (CombatSpeed.OneAndHalf, 160), (CombatSpeed.Two, 120) })
        {
            using var session = new AuthoritySession(AuthorityPolicy.Solo, matchId: "same", combatSeed: 123);
            Start(session);
            Assert.True(session.ExecuteLocal(Command.FromSnapshot(session.Snapshot(), 5, CombatSpeedRates.Action(speed), 1)).Accepted);
            for (int n = 0; n < wallFrames; n++) session.Advance(1.0 / 60);
            Assert.Equal(120, session.Snapshot().Tick);
            Assert.Equal(speed, SessionFlow.RoundTrip(session.Snapshot()).CombatSpeed);
            string combat = Combat(session.Snapshot());
            baseline ??= combat; Assert.Equal(baseline, combat);
        }
    }

    [Theory]
    [InlineData(CombatSpeed.One, 30)]
    [InlineData(CombatSpeed.OneAndHalf, 45)]
    [InlineData(CombatSpeed.Two, 60)]
    public void OneSecondHasTruthfulPaceAndBoundedCatchUp(CombatSpeed speed, int expectedTicks)
    {
        using var session = new AuthoritySession(AuthorityPolicy.Solo, combatSeed: 123); Start(session);
        Assert.True(session.ExecuteLocal(Command.FromSnapshot(session.Snapshot(), 5, CombatSpeedRates.Action(speed), 1)).Accepted);
        for (int n = 0; n < 60; n++) session.Advance(1.0 / 60);
        Assert.Equal(expectedTicks, session.Snapshot().Tick);
        session.Advance(1000);
        Assert.InRange(session.Snapshot().Tick - expectedTicks, 0, 6);
        Assert.Throws<ArgumentOutOfRangeException>(() => session.Advance(double.NaN));
    }

    [Fact]
    public void PauseDiscardsDebtAndPreparationNeverAccumulatesCombatTime()
    {
        using var session = new AuthoritySession(AuthorityPolicy.Solo, combatSeed: 123);
        Assert.True(session.ExecuteLocal(Command.FromSnapshot(session.Snapshot(), 1, "start", 1)).Accepted);
        session.Advance(1000); Assert.Equal(0, session.Snapshot().Tick);
        for (long sequence = 2; sequence <= 4; sequence++) Assert.True(session.ExecuteLocal(Command.FromSnapshot(session.Snapshot(), sequence, "ready", 1)).Accepted);
        session.Advance(1.0 / 60); Assert.Equal(0, session.Snapshot().Tick); // Half-tick credit.
        Assert.True(session.ExecuteLocal(Command.FromSnapshot(session.Snapshot(), 5, "pause", 1)).Accepted);
        session.Advance(1000); Assert.Equal(0, session.Snapshot().Tick);
        Assert.True(session.ExecuteLocal(Command.FromSnapshot(session.Snapshot(), 6, "resume", 1)).Accepted);
        session.Advance(1.0 / 60); Assert.Equal(0, session.Snapshot().Tick);
        session.Advance(1.0 / 60); Assert.Equal(1, session.Snapshot().Tick);
    }

    [Theory]
    [InlineData(AuthorityPolicy.PlayingHost)]
    [InlineData(AuthorityPolicy.Dedicated)]
    public void SpeedUsesSessionAuthorityAndSurvivesRetryDisconnectResume(AuthorityPolicy policy)
    {
        using var flow = new SessionFlow(policy, 2, 123);
        flow.Act(1, new("start"));
        Command request = flow.Command(2, new("speed-2x"));
        CommandResult result = flow.Send(2, request);
        Assert.Equal(policy == AuthorityPolicy.Dedicated, result.Accepted);
        Assert.Equal(result, flow.Send(2, request));
        if (policy == AuthorityPolicy.PlayingHost) flow.Act(1, new("speed-1.5x"));
        MatchSnapshot before = flow.State;
        flow.Disconnect(2); flow.Rebind(2, 12);
        MatchSnapshot restored = SessionFlow.RoundTrip(flow.State);
        Assert.Equal(before.CombatSpeed, restored.CombatSpeed);
        Assert.Equal(JsonSerializer.Serialize(before.NextWave), JsonSerializer.Serialize(restored.NextWave));
        Assert.False(flow.Send(1, flow.Command(1, new("speed-3x"))).Accepted);
        Assert.False(flow.Session.Admit(13, WireJson.ProtocolVersion - 1, "", flow.AdvanceClock()).Accepted);
    }

    [Fact]
    public void ForecastIsStableReadOnlyAndIsTheSpawnManifestIncludingRankedBosses()
    {
        Rules rules = new() { Campaign = new([new(1, true, [new(UnitType.Mage, 4, Rank: 2, IsBoss: true)], new(4, 2, 10))]) };
        using var match = new Match(rules, combatSeed: 123); match.Join(); match.Join(); match.Players[2].Health = 0;
        Assert.True(Act(match, "start").Accepted);
        UnitState[] forecast = SessionFlow.RoundTrip(match.Snapshot()).NextWave;
        Assert.Equal(2, forecast.Length); Assert.Empty(match.Enemies);
        Assert.All(forecast, u => { Assert.True(u.Id < 0); Assert.Null(u.Hex); Assert.Equal(1, u.Destination); Assert.Equal(0, u.TargetId); });
        string stable = JsonSerializer.Serialize(forecast);
        match.SetConnected(2, false); match.SetConnected(2, true);
        for (int turn = 1; turn <= 3; turn++)
        {
            Assert.Equal(stable, JsonSerializer.Serialize(match.Snapshot().NextWave));
            Assert.Empty(match.Combat.EnemyMembership());
            Assert.True(Act(match, "ready").Accepted);
        }
        Assert.Empty(match.Snapshot().NextWave);
        Assert.Equal(forecast.Select(u => (u.Type, u.Level, u.Rank, u.IsBoss, u.Origin, u.Destination, u.Profile)),
            match.Enemies.Select(u => (u.Type, u.Level, u.Rank, u.IsBoss, u.Origin, u.Destination, u.Profile)));
        Assert.All(match.Enemies, u => Assert.True(u.Id > 0));
    }

    [Fact]
    public void HistoricalFourthPhaseSnapshotsAreRefusedNotReinterpreted()
    {
        using var match = new Match(); match.Join(); Assert.True(Act(match, "start").Accepted);
        MatchSnapshot current = match.Snapshot();
        _ = SessionFlow.RoundTrip(current);
        Assert.Throws<InvalidDataException>(() => SnapshotPayload.Decode(SnapshotPayload.Encode(current with { Phase = Phase.Building, Turn = 3 })));
        // Actual legacy JSON has no version/speed/manifest, not merely a current
        // snapshot with a test flag. Compress it through the historical codec.
        var document = System.Text.Json.Nodes.JsonNode.Parse(JsonSerializer.Serialize(current, WireJson.Options))!.AsObject();
        document.Remove("BattleFlowVersion"); document.Remove("CombatSpeed"); document.Remove("NextWave");
        document["Phase"] = "Preparation";
        using var output = new MemoryStream();
        using (var brotli = new BrotliStream(output, CompressionLevel.Fastest, leaveOpen: true)) brotli.Write(System.Text.Encoding.UTF8.GetBytes(document.ToJsonString()));
        InvalidDataException refusal = Assert.Throws<InvalidDataException>(() => SnapshotPayload.DecodeForReceiving(Convert.ToBase64String(output.ToArray())));
        Assert.Contains("Rejoin a compatible authority or start a fresh match", refusal.Message);
    }
}
