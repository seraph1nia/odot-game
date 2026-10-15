using Xunit;

namespace Game.Core.Tests;

// Explicit ordinary-command policy for pre-existing progression fixtures. It
// does not change authority behavior, synthesize offers or suppress a phase.
internal static class RewardFixture
{
    internal static string Choose(RunState run) => DevRunner.CampaignStrategy.RewardChoice(run);
    internal static void Resolve(Match match)
    {
        if (match.Phase != Phase.RewardDraft) return;
        foreach (City city in match.Players.Values.Where(c => c.Connected && !c.Eliminated && c.Run.Choice is null))
            Assert.True(match.Apply(city.Id, Command.FromSnapshot(match.Snapshot(), 1, "reward:" + Choose(city.Run), city.Id)).Accepted);
        Assert.Equal(Phase.Building, match.Phase);
    }
}
