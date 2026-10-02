using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

public sealed class FrontlineCampaignTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    [InlineData(123UL)]
    public void OrdinarySoloStrategyWins(ulong seed) => CampaignAcceptance.Run(output, "frontline", 1, seed);
}

public sealed class MixedCampaignTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    [InlineData(123UL)]
    public void OrdinarySoloStrategyWins(ulong seed) => CampaignAcceptance.Run(output, "mixed", 1, seed);
}

public sealed class TowersCampaignTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    [InlineData(123UL)]
    public void OrdinarySoloStrategyWins(ulong seed) => CampaignAcceptance.Run(output, "towers", 1, seed);
}

public sealed class ResearchCampaignTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    [InlineData(123UL)]
    public void OrdinarySoloStrategyWins(ulong seed) => CampaignAcceptance.Run(output, "research", 1, seed);
}

public sealed class CooperativeCampaignTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(2, 0UL)]
    [InlineData(2, 1UL)]
    [InlineData(2, 123UL)]
    [InlineData(3, 0UL)]
    [InlineData(3, 1UL)]
    [InlineData(3, 123UL)]
    [InlineData(4, 0UL)]
    [InlineData(4, 1UL)]
    [InlineData(4, 123UL)]
    public void OrdinaryCooperativeStrategyWins(int players, ulong seed) => CampaignAcceptance.Run(output, "frontline", players, seed);
}
