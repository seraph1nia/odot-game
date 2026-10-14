using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

// Tier A: arithmetic + production forecasts/live allocations at every wave and
// roster width. Existing catalog/transition tests miss enemy-only strength and
// double application after receipt. Fixed seed, no children; 140 short transitions including fallen-owner pressure.
public sealed class WavePowerTests(ITestOutputHelper output)
{
    [Fact]
    public void ScoreUsesLeveledDurabilityAndPrimaryCadenceWithNoInventedRoleBonuses()
    {
        var combat = new CombatConfiguration(WavePowerBaseline.Rules());
        Assert.Equal(1m, WavePower.Member(combat.Unit(UnitType.Swordsman).Runtime()));
        Assert.Equal(1m, WavePower.Member(combat.Unit(UnitType.Berserker).Runtime()));
        Assert.Equal(1.1m, WavePower.Member(combat.Unit(UnitType.Crossbowman).Runtime()));
        Assert.Equal(.7125m, WavePower.Member(combat.Unit(UnitType.Mage).Runtime()));
        Assert.Equal(5m, WavePower.Member(combat.Unit(UnitType.Swordsman, isBoss: true).Runtime()));
        Assert.True(WavePower.Member(combat.Unit(UnitType.Swordsman, level: 5).Runtime())
            > WavePower.Member(combat.Unit(UnitType.Swordsman, level: 4).Runtime()));
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, false)]
    [InlineData(4, false)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(4, true)]
    public void EveryWaveHasExactlySeventyFivePercentBaselinePowerWithoutChangingMembersOrPlayerStats(int players, bool fallenOwner)
    {
        Rules before = WavePowerBaseline.Rules();
        var baseline = new CombatConfiguration(before);
        using var match = new Match(combatSeed: 123);
        for (int i = 0; i < players; i++) match.Join();
        PaidCampaignFixture.Apply(match, 1, new("start"));
        Assert.Equal(baseline.ActorDecisionFingerprint, match.Configuration.ActorDecisionFingerprint);
        Assert.NotEqual(baseline.Fingerprint, match.Configuration.Fingerprint);
        foreach (UnitType type in Enum.GetValues<UnitType>())
            for (int level = 1; level <= 6; level++)
                Assert.Equal(baseline.Unit(type, level: level), match.Configuration.Unit(type, level: level));
        for (int wave = 1; wave <= 20; wave++)
        {
            WaveDefinition original = before.Campaign.Waves[wave - 1], current = match.Campaign.Wave(wave);
            Assert.Equal(original.Entries, current.Entries);
            Assert.Equal(original.Reward, current.Reward);
            Assert.Equal(original.IsBoss, current.IsBoss);
            decimal priorPower = WavePower.Total(original, baseline, players);
            decimal power = WavePower.Total(current, match.Configuration, players);
            Assert.Equal(priorPower * .75m, power, precision: 12);
            MatchSnapshot forecast = match.Snapshot();
            Assert.Equal(current.Allocation * players, forecast.NextWave.Length);
            Assert.Equal(power, forecast.NextWave.Sum(u => WavePower.Member(u.Profile)), precision: 12);
            // Rules receipt/reload remains 75, not .75 squared or a new default.
            Rules received = JsonSerializer.Deserialize<Rules>(JsonSerializer.Serialize(forecast.Rules, WireJson.Options), WireJson.Options)!;
            Assert.Equal(75, received.EnemyPowerPercent);
            Assert.Equal(power, WavePower.Total(current, new(received), players));
            SnapshotCompatibility.RequireSharedArena(JsonSerializer.Deserialize<MatchSnapshot>(
                JsonSerializer.Serialize(forecast, WireJson.Options), WireJson.Options)!);
            while (match.Phase is Phase.Building or Phase.Preparation)
                foreach (City city in match.Players.Values.Where(c => !c.Ready && !c.Eliminated))
                    PaidCampaignFixture.Apply(match, city.Id, new("ready"));
            Assert.Equal(Phase.Combat, match.Phase);
            Assert.Equal(power, match.Enemies.Sum(u => WavePower.Member(u.Profile)), precision: 12);
            foreach (int owner in match.Players.Keys)
            {
                Assert.Equal(current.Allocation, match.Enemies.Count(u => u.Origin == owner));
                if (!fallenOwner || wave == 1)
                    Assert.Equal(current.Allocation, match.Enemies.Count(u => u.Origin == owner && u.Destination == owner));
            }
            if (fallenOwner && wave > 1)
            {
                Assert.True(match.Players[players].Eliminated);
                Assert.DoesNotContain(match.Enemies, u => u.Destination == players);
                Assert.Contains(match.Enemies, u => u.Origin == players);
            }
            foreach (UnitState enemy in match.Enemies)
            {
                WeaponProfile originalProfile = baseline.Unit(enemy.Type, enemy.Rank, enemy.IsBoss, enemy.Level).Runtime();
                Assert.Equal(originalProfile with
                {
                    Health = originalProfile.Health * 3 / 4,
                    Damage = originalProfile.Damage * 3 / 4,
                    SecondaryDamage = originalProfile.SecondaryDamage * 3 / 4,
                    StructureDamage = originalProfile.StructureDamage * 3 / 4
                }, enemy.Profile);
            }
            if (players == 1) output.WriteLine($"W{wave}: {current.Allocation} members, before={priorPower:F6}, after={power:F6}, change={(power / priorPower - 1) * 100:F2}%");
            foreach (UnitState enemy in match.Enemies.ToArray()) match.Combat.Remove(enemy.Id);
            if (fallenOwner && wave == 1) match.Players[players].Health = 0; // allocation boundary, not earned loss
            match.Step();
            Assert.Equal(wave, match.LastRewardedWave);
        }
        Assert.Equal(Phase.Victory, match.Phase);
    }

    [Fact]
    public void StrengthSettingIsValidatedIdentityBoundAndNotAppliedToRestoredHealth()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CombatConfiguration(new() { EnemyPowerPercent = 0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CombatConfiguration(new() { EnemyPowerPercent = 101 }));
        using var match = new Match(combatSeed: 123);
        match.Join(); PaidCampaignFixture.Apply(match, 1, new("start"));
        MatchSnapshot forecast = match.Snapshot();
        Rules mismatched = forecast.Rules with { EnemyPowerPercent = 100 };
        Assert.Throws<ArgumentException>(() => SnapshotCompatibility.RequireSharedArena(forecast with { Rules = mismatched }));
        Assert.Throws<ArgumentException>(() => SnapshotCompatibility.RequireSharedArena(forecast with
        {
            Rules = mismatched,
            ConfigurationFingerprint = RulesIdentity.Resolve(mismatched)
        }));
        using var combat = new CombatSimulation(new(), seed: 123);
        int enemy = combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons, level: 3, isBoss: true);
        UnitState original = combat.Read(enemy);
        combat.Seed(original with { Health = original.Health - 100 });
        UnitState injured = combat.Read(enemy);
        combat.Seed(JsonSerializer.Deserialize<UnitState>(JsonSerializer.Serialize(injured, WireJson.Options), WireJson.Options)!);
        Assert.Equal(injured, combat.Read(enemy));
        Assert.Equal(original.Profile, combat.Read(enemy).Profile);
        // Missing strength cannot silently reinterpret a historical rules file.
        string json = JsonSerializer.Serialize(new Rules(), WireJson.Options);
        using JsonDocument document = JsonDocument.Parse(json);
        string name = document.RootElement.EnumerateObject().Single(p => p.Name.Equals("enemyPowerPercent", StringComparison.OrdinalIgnoreCase)).Name;
        var fields = document.RootElement.EnumerateObject().Where(p => p.Name != name).ToDictionary(p => p.Name, p => p.Value);
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Rules>(JsonSerializer.Serialize(fields), WireJson.Options));
    }
}
