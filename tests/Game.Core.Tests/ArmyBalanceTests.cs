using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

// Explicit provisional calibration: default prices and actual paid commands.
// Retires alternative-economy curves/old size-six/home-index/role-win goldens;
// individual capacity/payment/recovery negatives live in the paid driver and A.
[Trait("Tier", "Deep")]
public sealed class ArmyBalanceTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(1UL)]
    [InlineData(123UL)]
    public void PaidInvestmentControlsChangeActualCapacityEquipmentAndRecoveryWithoutSubsidies(ulong seed)
    {
        var full = PaidCampaignFixture.Run(output, "frontline", 1, seed);
        var homes = PaidCampaignFixture.Run(output, "frontline", 1, seed, maximumHomes: 2);
        var equipment = PaidCampaignFixture.Run(output, "frontline", 1, seed, maximumTier: 1);
        Assert.Equal(Phase.Victory, full.Outcome); // Retained ordinary investment acceptance; economics still provisional.
        Assert.All(homes.Waves.SelectMany(w => w.Entry), c => Assert.Equal(2, c.Army!.PurchasedHomes));
        Assert.Contains(full.Waves, w => w.Entry[0].Army!.PurchasedHomes > 2);
        Assert.DoesNotContain(homes.Commands, c => c.Command.Action == "buy-home");
        Assert.Contains(full.Commands, c => c.Command.Action == "buy-home");
        Assert.All(equipment.Waves.SelectMany(w => w.Entry).SelectMany(c => c.Soldiers), u => Assert.Equal(1, u.Level));
        Assert.Contains(full.Waves.SelectMany(w => w.Entry).SelectMany(c => c.Soldiers), u => u.Level > 1);
        foreach (var run in new[] { full, homes, equipment })
        {
            Assert.True(run.Outcome is Phase.Victory or Phase.Defeat);
            Assert.NotEqual(DefeatReason.BattleStalled, run.Reason);
            Assert.Contains(run.Commands, c => c.Command.Action == "recruit");
        }
        output.WriteLine($"PROVISIONAL seed={seed}: ordinary={full.Outcome}/W{full.ReachedWave}, two-home={homes.Outcome}/W{homes.ReachedWave}, L1={equipment.Outcome}/W{equipment.ReachedWave}; actual Keep damage={full.Waves.Sum(w => w.KeepDamage)}/{homes.Waves.Sum(w => w.KeepDamage)}/{equipment.Waves.Sum(w => w.KeepDamage)}");
    }
    [Theory]
    [InlineData(1UL)]
    [InlineData(123UL)]
    public void PaidHallUpgradesChangeRealRecoveryInvestmentAndPreserveIndividualBounds(ulong seed)
    {
        var upgraded = PaidCampaignFixture.Run(output, "reserve", 1, seed);
        var basic = PaidCampaignFixture.Run(output, "reserve", 1, seed, hallUpgrades: false);
        Assert.Contains(upgraded.Commands, c => c.Command.Action == "store");
        Assert.Contains(basic.Commands, c => c.Command.Action == "store");
        Assert.DoesNotContain(basic.Commands, c => c.Command.Action is "upgrade-capacity" or "upgrade-healing");
        Assert.All(basic.Final.Players.SelectMany(c => c.Army!.Halls), h => { Assert.Equal(1, h.CapacityLevel); Assert.Equal(1, h.HealingLevel); });
        Assert.True(upgraded.RecoveryInvestment.Gold > basic.RecoveryInvestment.Gold);
        Assert.True(upgraded.HealedHealth > 0); Assert.True(basic.HealedHealth > 0);
        Assert.NotEqual(DefeatReason.BattleStalled, upgraded.Reason); Assert.NotEqual(DefeatReason.BattleStalled, basic.Reason);
        output.WriteLine($"PROVISIONAL seed={seed}: upgraded/basic Hall cost={upgraded.RecoveryInvestment}/{basic.RecoveryInvestment}, healed hundredths={upgraded.HealedHealth}/{basic.HealedHealth}, outcomes={upgraded.Outcome}/W{upgraded.ReachedWave}, {basic.Outcome}/W{basic.ReachedWave}");
    }
}
