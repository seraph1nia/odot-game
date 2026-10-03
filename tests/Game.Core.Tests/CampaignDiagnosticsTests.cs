using Xunit;

namespace Game.Core.Tests;

public sealed class CampaignDiagnosticsTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InvestmentValidationDoesNotDependOnFormatting(bool detailed)
    {
        var output = new Profiling.ProfileOutput(); var diagnostics = new CampaignDiagnostics(output, detailed);
        ResourceCost sum = CampaignDiagnostics.Investment([new(1, 2, 3, 4, 5, 6), new(7, 8, 9, 10, 11, 12)]);
        Assert.Equal(new ResourceCost(8, 10, 12, 14, 16, 18), sum);
        Assert.ThrowsAny<Exception>(() => CampaignDiagnostics.Investment([new(int.MaxValue), new(1)]));
        Assert.ThrowsAny<Exception>(() => CampaignDiagnostics.Investment([new(-1)]));
        diagnostics.Record(() => sum.ToString());
        Assert.Equal(detailed ? 1 : 0, output.Lines);
    }
    [Fact]
    public void CompactSuccessIsCompleteAndFailureExpandsOnlyBoundedRecentDomainFacts()
    {
        var output = new Profiling.ProfileOutput(); var diagnostics = new CampaignDiagnostics(output, false);
        int formatted = 0;
        for (int action = 0; action < 100; action++)
        {
            int index = action;
            diagnostics.Record(() => { formatted++; return $"frontline P1 W2 T3 action={index}; stocks={new ResourceCost(1, 2, 3, 4, 5, 6)}; investment={default(ResourceCost)}"; });
        }
        Assert.Equal(0, formatted); Assert.Equal(64, diagnostics.RecentCount); Assert.Equal(0, output.Lines);
        diagnostics.Failure(); Assert.Equal(64, formatted); Assert.Equal(65, output.Lines);
        Assert.Contains("action=99", output.Recent.Last()); Assert.DoesNotContain(output.Recent, text => text.Contains("action=0;", StringComparison.Ordinal));
        diagnostics.Summary("waveTicks=1:20; casualties=2; recruits=3; expansions=1; trades=2; sales=1; firstLevelFiveBattle=3; final=Victory");
        Assert.Contains("firstLevelFiveBattle=3", output.Recent.Last());
    }
}
