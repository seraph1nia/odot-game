using Game.Core;
using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

public sealed class SessionCampaignTests(ITestOutputHelper output)
{
    private static readonly int[] BossWaves = [10, 20];
    [Fact]
    public void OrdinarySerializedAuthorityCampaignWinsAndRoundTripsBothBossesAndTerminalReward()
    {
        using var flow = new SessionFlow();
        flow.Act(1, new("start"));
        var bossWaves = new HashSet<int>();
        int ticks = 0;
        while (flow.Session.Phase is Phase.Building or Phase.Preparation or Phase.Combat)
        {
            flow.Prepare();
            MatchSnapshot battle = flow.State;
            if (battle.Enemies.Any(u => u.IsBoss))
            {
                bossWaves.Add(battle.Wave);
                SessionFlow.RoundTrip(battle);
            }
            while (flow.Session.Phase == Phase.Combat)
            {
                Assert.True(ticks++ < 100000, $"Campaign exceeded its bound at W{battle.Wave}.");
                flow.Session.Step();
            }
            // Allow ordinary death cleanup to release the next preparation gate.
            for (int cleanup = 0; cleanup < 120; cleanup++) flow.Session.Step();
            output.WriteLine($"seed=1 wave={battle.Wave} tick={flow.State.Tick} phase={flow.Session.Phase}");
        }
        MatchSnapshot terminal = SessionFlow.RoundTrip(flow.State);
        Assert.Equal(Phase.Victory, terminal.Phase); Assert.Equal(20, terminal.Wave);
        Assert.Equal(BossWaves, bossWaves.Order().ToArray());
        Assert.Equal(20, terminal.LastRewardedWave);
        Assert.All(terminal.Players, c => Assert.False(c.Eliminated));
        string before = SessionFlow.Gameplay(terminal);
        Assert.False(flow.Send(1, flow.Command(1, new("ready"))).Accepted);
        for (int step = 0; step < 120; step++) flow.Session.Step();
        Assert.Equal(before, SessionFlow.Gameplay(flow.State));
    }
}
