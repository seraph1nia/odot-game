using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

// Selected numerical qualification, not final earned-balance acceptance. Finite
// seeds 1 and 123, production twenty-wave horizon, 100000 fixed steps, four current
// readiness gates/wave and <=100 paid decisions/gate. No stock/HP/wave setters.
// A diagnostic row records genuine loss/stall, never labels it a campaign win.
// Legacy full-victory acceptance remains separate until timing lands/calibration.
[Trait("Tier", "Deep")]
public sealed class SquadCampaignBaselineTests(ITestOutputHelper output)
{
    public static IEnumerable<object[]> Cases()
    {
        foreach (ulong seed in new[] { 1UL, 123UL })
        {
            foreach (string family in new[] { "frontline", "mixed", "towers", "research" }) yield return [family, 1, seed];
        }
    }
    [Theory]
    [MemberData(nameof(Cases))]
    public void ProvisionalPaidTwentyWaveHorizonRecordsPhysicalThreatsAndIndividualAttrition(string family, int players, ulong seed)
        => Check(PaidCampaignFixture.Run(output, family, players, seed));

    [Theory]
    [InlineData(1UL)]
    [InlineData(123UL)]
    public void ProvisionalCooperativeRescueUsesActualForeignOriginImpacts(ulong seed)
        => Check(PaidCampaignFixture.Run(output, "rescue", 3, seed));

    [Theory]
    [InlineData("ranged", 1UL)]
    [InlineData("ranged", 123UL)]
    [InlineData("reserve", 1UL)]
    [InlineData("reserve", 123UL)]
    public void ProvisionalRangedAndReserveHorizonIncludesPaidEquipmentAndRealHallSlots(string family, ulong seed)
    {
        var result = PaidCampaignFixture.Run(output, family, 1, seed);
        Check(result);
        Assert.Contains(result.Waves[0].Entry[0].Soldiers, u => u.Type == (family == "ranged" ? UnitType.Crossbowman : UnitType.Swordsman));
        if (family == "reserve") Assert.True(result.HealedHealth > 0);
    }
    private static void Check(PaidCampaignFixture.Result result)
    {
        Assert.Equal(20, result.Final.TotalWaves); Assert.NotEmpty(result.Waves); Assert.NotEmpty(result.Commands);
        Assert.True(result.Outcome is Phase.Victory or Phase.Defeat);
        Assert.Equal(result.Commands.Count(c => c.Command.Action == "recruit") - result.Commands.Count(c => c.Command.Action == "retire")
            - result.Final.Players.Sum(c => c.Soldiers.Length), result.Waves.Sum(w => w.Casualties));
        if (result.Outcome == Phase.Victory) Assert.Equal(20, result.Final.LastRewardedWave);
        else Assert.NotEqual(DefeatReason.None, result.Reason);
        // Paid control guards apply to all outcomes, not only winning transcripts.
        Assert.All(result.Final.Players, c => Assert.InRange(c.Army!.PurchasedHomes, 2, 6));
        if (result.Family == "rescue")
        {
            Assert.All(result.Waves, w => Assert.Empty(w.Entry[^1].Soldiers));
            Assert.Contains(result.Waves, w => w.ForeignOriginImpacts > 0);
        }
    }
}
