using System.Text.Json;
using DevRunner;
using Game.Core;
using Xunit;

namespace DevRunner.Tests;

public sealed class CurrentObservationTests
{
    [Theory]
    [InlineData("melee")]
    [InlineData("splash")]
    [InlineData("defender")]
    [InlineData("tower")]
    [InlineData("catapult-splash")]
    [InlineData("expiry")]
    [InlineData("pulse")]
    [InlineData("poison")]
    public async Task ImminentDangerDoesNotOverrideAValidActualFrozenReceipt(string cause)
    {
        StatusState burn = StatusPolicy.Apply(new(), new(139, StatusKind.Burn, 132, 3, 3284, 252), new());
        var enemy = new UnitState(139, 840, Origin: 1, Destination: 1) { Statuses = burn };
        var sword = new UnitState(122, 1000) { TargetId = 139, PendingImpact = true, ImpactTick = 3290, Profile = new(1000, 1800, 1, 30, 12, 60) };
        var city = new CityState(1, true, false, 0, 0, 100, [], [], 0);
        var state = new MatchSnapshot("burn-capture", 1, 3284, Phase.Combat, false, 8, 3, 40, new(), [city], [enemy]);
        switch (cause)
        {
            case "melee": city = city with { Soldiers = [sword] }; break;
            case "splash": city = city with { Soldiers = [sword with { TargetId = 999, Profile = sword.Profile with { SplashHexRadius = 1 } }] }; break;
            case "defender": city = city with { Defender = new(1, -1, Building.Empty, 1, TargetId: 999, PendingImpact: true) }; break;
            case "tower": city = city with { Towers = [new(1, 2, Building.ArrowTower, 1, TargetId: 139, PendingImpact: true)] }; break;
            case "catapult-splash": city = city with { Towers = [new(1, 2, Building.CatapultTower, 1, TargetId: 999, PendingImpact: true)] }; break;
            case "expiry": state = state with { Tick = burn.Burn!.ExpiresTick - 1 }; break;
            case "pulse": enemy = enemy with { Health = burn.Burn!.Strength }; break;
            case "poison": enemy = enemy with { Statuses = StatusPolicy.Apply(burn, new(139, StatusKind.Poison, 140, 1, 3284, 600), new()) }; break;
        }
        state = state with { Players = [city], Enemies = [enemy] };
        // These were predictive-filter fixtures. Even imminent danger does not
        // invalidate an actual frozen receipt whose observed target/burn survived.
        GameEvent receipt = new("ack", State: state with { Revision = 2, Paused = true }, Result: new(11, true, "paused"));
        MatchSnapshot current = state;
        var captured = await BurnAdmission.Pause(state, 1, 10, () => current,
            (_, _) => throw new InvalidOperationException("Valid receipt requires no re-arm."),
            (command, _) => { Assert.Equal("pause", command); current = receipt.State!; return Task.FromResult(receipt); }, default);
        Assert.Same(receipt, captured.Receipt);
        Assert.Equal(enemy.Id, captured.TargetId);
    }

    [Fact]
    public void LatestDeliveredStateUsesArrivalOrderAndIgnoresStatelessEvents()
    {
        var events = new ChildEvents();
        Assert.Null(events.LatestState);
        var first = new MatchSnapshot("first", 100, 100, Phase.Combat, false, 1, 1, 1, new(), [], []);
        events.Add(new("snapshot", State: first), 100);
        Assert.Same(first, events.LatestState);
        var fresh = first with { MatchId = "fresh", Revision = 1, Tick = 0 };
        events.Add(new("connected", State: fresh), 100);
        events.Add(new("ready"), 100);
        Assert.Same(fresh, events.LatestState);
        Assert.Same(events.History().Last(e => e.State is not null).State, events.LatestState);
        for (int n = 0; n < ChildEvents.MaximumStates; n++) events.Add(new("snapshot", State: fresh), 100);
        Assert.Same(fresh, events.LatestState);
    }

    [Fact]
    public void CurrentStateReadsDoNotAllocateCopiesOfTheRetainedHistory()
    {
        var events = new ChildEvents();
        var state = new MatchSnapshot("current", 1, 1, Phase.Combat, false, 1, 1, 1, new(), [], []);
        events.Add(new("snapshot", State: state), 100);
        for (int n = 0; n < ChildEvents.MaximumEvents - 1; n++) events.Add(new("ready"), 100);
        Assert.Same(state, events.LatestState);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int n = 0; n < 1000; n++) Assert.Same(state, events.LatestState);
        long currentReads = GC.GetAllocatedBytesForCurrentThread() - before;
        before = GC.GetAllocatedBytesForCurrentThread();
        for (int n = 0; n < 1000; n++) Assert.Same(state, events.History().Last(e => e.State is not null).State);
        long historicalReads = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.True(currentReads < historicalReads / 10, $"Current reads allocated {currentReads} bytes; history reads allocated {historicalReads} bytes.");
    }

    [Theory]
    [InlineData("death")]
    [InlineData("expiry")]
    [InlineData("clear")]
    [InlineData("repaused")]
    public async Task RetainedBurnCannotTriggerCaptureAfterTheCurrentStateLosesIt(string transition)
    {
        string root = Path.Combine(Environment.CurrentDirectory, "logs", "current-observation-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            StatusState burn = StatusPolicy.Apply(new(), new(2, StatusKind.Burn, 1, 1, 100, 100), new());
            var enemy = new UnitState(2, 1000, Origin: 1, Destination: 1) { Faction = Faction.Skeletons, Statuses = burn };
            var earlier = new MatchSnapshot("current-burn", 1, 100, Phase.Combat, transition == "repaused", 8, 8, 12, new(), [], [enemy]);
            MatchSnapshot current = transition switch
            {
                "death" => earlier with { Revision = 2, Tick = 101, Enemies = [] },
                "expiry" => earlier with { Revision = 2, Tick = 280, Enemies = [enemy with { Statuses = StatusPolicy.Advance(burn, 280, new()).State }] },
                "repaused" => earlier with { Revision = 2, Enemies = [] },
                _ => earlier with { Revision = 2, Tick = 101, Phase = Phase.Building, Wave = 9, Enemies = [] }
            };
            long freshTick = transition == "repaused" ? 100 : 281;
            MatchSnapshot fresh = earlier with { Revision = 3, Tick = freshTick, Enemies = [enemy with { Statuses = StatusPolicy.Apply(new(), new(2, StatusKind.Burn, 1, 2, freshTick, 100), new()) }] };
            string Emit(GameEvent value) => "printf 'ODOT_EVENT %s\\n' '" + JsonSerializer.Serialize(value, WireJson.Options) + "'; ";
            string script = Emit(new("snapshot", State: earlier)) + Emit(new("snapshot", State: current)) + Emit(new("ready"))
                + "read -r command; " + Emit(new("snapshot", State: fresh)) + "read -r command; exit 0";
            await using var child = new Child("burn-observer", "/bin/sh", ["-c", script], root);
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await child.WaitFor(e => e.Type == "ready", "both initial states delivered", 2000, deadline.Token);
            var runner = new Runner(Options.Parse(["test-network"]), deadline.Token, new Evidence(root), root: root);
            bool HasBurn(MatchSnapshot state) => state.Revision >= earlier.Revision && state.Enemies.Any(u => u.Statuses.Burn is not null);

            // Historical barriers remain available; only transient capture needs current state.
            GameEvent historical = await child.WaitFor(e => e.State is { } state && HasBurn(state), "historical burn", 2000, deadline.Token);
            Assert.Equal(earlier.Revision, historical.State!.Revision);
            if (transition == "clear")
            {
                MatchSnapshot settled = await runner.ObserveCurrent(child, state => state.Phase != Phase.Combat || HasBurn(state), "current wave clear or burn", deadline.Token);
                Assert.Equal(current.Revision, settled.Revision);
                Assert.Equal(Phase.Building, settled.Phase);
            }
            Task<MatchSnapshot> capture = runner.ObserveCurrent(child, HasBurn, "current real burn", deadline.Token);
            Assert.False(capture.IsCompleted);
            await child.Send("next");
            MatchSnapshot observed = await capture;
            Assert.Equal(fresh.Revision, observed.Revision);
            Assert.Equal(freshTick, observed.Enemies.Single().Statuses.Burn!.AppliedTick);
            await child.Send("quit");
            Assert.Equal(0, await child.WaitExit(deadline.Token));
        }
        finally { Directory.Delete(root, true); }
    }
}
