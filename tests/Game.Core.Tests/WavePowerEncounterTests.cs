using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

// Explicit bounded Tier E balance qualification: direct power arithmetic cannot
// prove a paid opening survives or takes fewer real hits. Same fixed seeds and
// paid production driver before/after, no grants, altered prices, child processes
// or second simulator. Three-wave pairs + selected twenty-wave role horizons.
[Trait("Tier", "Deep")]
public sealed class WavePowerEncounterTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData("frontline", 1, 1UL, 3)]
    [InlineData("frontline", 1, 123UL, 3)]
    [InlineData("ranged", 1, 1UL, 1)]
    [InlineData("ranged", 1, 123UL, 1)]
    [InlineData("support", 2, 1UL, 3)]
    [InlineData("support", 3, 1UL, 3)]
    [InlineData("support", 4, 1UL, 3)]
    public void SamePaidOpeningFacesLessPhysicalAttrition(string family, int players, ulong seed, int horizon)
    {
        var before = PaidCampaignFixture.Run(output, family, players, seed, stopAfterWave: horizon, rules: WavePowerBaseline.Rules());
        var after = PaidCampaignFixture.Run(output, family, players, seed, stopAfterWave: horizon);
        PaidCampaignFixture.Wave prior = before.Waves[0], current = after.Waves[0];
        // Before the first impact, the exact same earned army and investments.
        Assert.Equal(before.Commands.TakeWhile(c => c.Command.TurnSerial <= 3).Select(c => c.Command),
            after.Commands.TakeWhile(c => c.Command.TurnSerial <= 3).Select(c => c.Command));
        Assert.Equal(prior.Entry.SelectMany(c => c.Soldiers), current.Entry.SelectMany(c => c.Soldiers));
        Assert.Equal(prior.PaidFood, current.PaidFood);
        Assert.Equal(0, prior.Unfed); Assert.Equal(0, current.Unfed);
        Assert.True(current.LostHealth < prior.LostHealth, $"W1 lost HP {prior.LostHealth}->{current.LostHealth}");
        Assert.True(current.Casualties <= prior.Casualties);
        Assert.True(current.KeepDamage <= prior.KeepDamage);
        Assert.True(after.Final.LastRewardedWave >= before.Final.LastRewardedWave);
        Assert.Null(before.Stall); Assert.Null(after.Stall);
        if (family != "ranged")
        {
            Assert.Equal(horizon, after.Final.LastRewardedWave);
            Assert.Equal(Phase.Building, after.Outcome);
        }
        output.WriteLine($"PAIR {family}/{players}/seed={seed}: W1 ticks {prior.Ticks}->{current.Ticks}, lostHP {prior.LostHealth}->{current.LostHealth}, casualties {prior.Casualties}->{current.Casualties}, Keep {prior.KeepDamage}->{current.KeepDamage}; cleared {before.Final.LastRewardedWave}->{after.Final.LastRewardedWave}; result {before.Outcome}->{after.Outcome}");
    }

    [Theory]
    [InlineData("frontline")]
    [InlineData("mixed")]
    [InlineData("ranged")]
    public void LaterPaidRoleHorizonRetainsRealCombatAndRecordsAnyGenuineLoss(string family)
    {
        var before = PaidCampaignFixture.Run(output, family, 1, 1, rules: WavePowerBaseline.Rules());
        var result = PaidCampaignFixture.Run(output, family, 1, 1);
        Assert.NotEmpty(result.Commands);
        Assert.True(result.Final.LastRewardedWave >= before.Final.LastRewardedWave);
        Assert.True(result.Final.LastRewardedWave >= 3);
        Assert.Contains(result.Waves, w => w.Number >= 10);
        Assert.Null(result.Stall);
        // Melee and mixed acceptance remains a genuine victory; a paid pure
        // ranged horizon is measured, not silently promoted to a victory claim.
        if (family != "ranged") Assert.Equal(Phase.Victory, result.Outcome);
        else Assert.True(result.Outcome is Phase.Victory or Phase.Defeat);
        output.WriteLine($"HORIZON {family}: cleared {before.Final.LastRewardedWave}->{result.Final.LastRewardedWave}, outcome={before.Outcome}->{result.Outcome}/{result.Reason}, ticks={before.CombatTicks}->{result.CombatTicks}, casualties={before.Waves.Sum(w => w.Casualties)}->{result.Waves.Sum(w => w.Casualties)}");
    }
}
