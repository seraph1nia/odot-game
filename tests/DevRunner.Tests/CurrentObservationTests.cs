using System.Text.Json;
using DevRunner;
using Game.Core;
using Xunit;

namespace DevRunner.Tests;

public sealed class CurrentObservationTests
{
    [Theory]
    [InlineData("death")]
    [InlineData("expiry")]
    [InlineData("clear")]
    public async Task RetainedBurnCannotTriggerCaptureAfterTheCurrentStateLosesIt(string transition)
    {
        string root = Path.Combine(Environment.CurrentDirectory, "logs", "current-observation-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            StatusState burn = StatusPolicy.Apply(new(), new(2, StatusKind.Burn, 1, 1, 100, 100), new());
            var enemy = new UnitState(2, 1000, Origin: 1, Destination: 1) { Faction = Faction.Skeletons, Statuses = burn };
            var earlier = new MatchSnapshot("current-burn", 1, 100, Phase.Combat, false, 8, 8, 12, new(), [], [enemy]);
            MatchSnapshot current = transition switch
            {
                "death" => earlier with { Revision = 2, Tick = 101, Enemies = [] },
                "expiry" => earlier with { Revision = 2, Tick = 280, Enemies = [enemy with { Statuses = StatusPolicy.Advance(burn, 280, new()).State }] },
                _ => earlier with { Revision = 2, Tick = 101, Phase = Phase.Building, Wave = 9, Enemies = [] }
            };
            MatchSnapshot fresh = earlier with { Revision = 3, Tick = 281, Enemies = [enemy with { Statuses = StatusPolicy.Apply(new(), new(2, StatusKind.Burn, 1, 2, 281, 100), new()) }] };
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
            Assert.Equal(281, observed.Enemies.Single().Statuses.Burn!.AppliedTick);
            await child.Send("quit");
            Assert.Equal(0, await child.WaitExit(deadline.Token));
        }
        finally { Directory.Delete(root, true); }
    }
}
