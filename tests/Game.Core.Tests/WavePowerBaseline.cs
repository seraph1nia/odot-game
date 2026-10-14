namespace Game.Core.Tests;

// Frozen prechange inputs from 4430c3ed1d2de402ae40f404d82404036e076959 See
// docs/wave-power.md for the full source revision and measurement limitations.
// Intentionally NOT CampaignDefinition.Default(): a future catalog edit must
// not turn the before/after comparison into a comparison of reduced data.
internal static class WavePowerBaseline
{
    internal static Rules Rules() => new()
    {
        EnemyPowerPercent = 100,
        SoldierHealth = 40,
        SoldierDamage = 10,
        BerserkerHealth = 30,
        BerserkerDamage = 15,
        RangedHealth = 40,
        RangedDamage = 12,
        MageHealth = 25,
        MageDamage = 12,
        AttackTicks = 60,
        MeleeWindupTicks = 12,
        RangedWindupTicks = 18,
        Combat = new()
        {
            BerserkerWindupTicks = 18,
            BerserkerRecoveryTicks = 54,
            MageWindupTicks = 24,
            MageRecoveryTicks = 66
        },
        Arena = new()
        {
            SwordsmanSecondaryDamage = 0,
            BerserkerSecondaryDamage = 3,
            CrossbowmanSecondaryDamage = 0,
            MageSecondaryDamage = 4,
            SwordsmanStructureDamage = 5,
            BerserkerStructureDamage = 7,
            CrossbowmanStructureDamage = 3,
            MageStructureDamage = 4
        },
        Campaign = Catalog()
    };

    // Independent fixed-point expected values for faction-asymmetry assertions.
    internal static WeaponProfile Reduced(WeaponProfile profile) => profile with
    {
        Health = profile.Health * 3 / 4,
        Damage = profile.Damage * 3 / 4,
        SecondaryDamage = profile.SecondaryDamage * 3 / 4,
        StructureDamage = profile.StructureDamage * 3 / 4
    };

    private static CampaignDefinition Catalog()
    {
        SpawnEntry S(int count, int level) => new(UnitType.Swordsman, level, count);
        SpawnEntry B(int count, int level) => new(UnitType.Berserker, level, count);
        SpawnEntry C(int count, int level) => new(UnitType.Crossbowman, level, count);
        SpawnEntry M(int count, int level) => new(UnitType.Mage, level, count);
        WaveDefinition Wave(int number, params SpawnEntry[] entries) => new(number, false, entries, new(2, 1, 5));
        WaveDefinition Boss(int number, int level) => new(number, true, [new(UnitType.Swordsman, level, IsBoss: true)], new(4, 2, 10));
        return new([
            Wave(1, S(4, 1)), Wave(2, S(3, 1), B(1, 1)), Wave(3, S(3, 1), C(2, 1)),
            Wave(4, S(3, 1), B(1, 1), C(1, 1), M(1, 1)), Wave(5, S(3, 2), C(1, 1)),
            Wave(6, S(3, 2), B(1, 2), C(1, 1)), Wave(7, S(3, 2), C(2, 2), M(1, 1)),
            Wave(8, S(3, 2), B(2, 2), C(2, 2), M(1, 2)), Wave(9, S(3, 3), B(1, 2), C(1, 2), M(1, 2)),
            Boss(10, 3), Wave(11, S(3, 3), C(2, 2)), Wave(12, S(3, 3), B(1, 3), C(2, 3)),
            Wave(13, S(3, 3), C(2, 3), M(1, 3)), Wave(14, S(3, 3), B(2, 3), C(2, 3), M(1, 3)),
            Wave(15, S(3, 4), C(1, 3), M(1, 3)), Wave(16, S(3, 4), B(1, 4), C(2, 4)),
            Wave(17, S(3, 4), C(2, 4), M(1, 4)), Wave(18, S(3, 4), B(2, 4), C(2, 4), M(1, 4)),
            Wave(19, S(3, 5), B(1, 4), C(2, 4), M(1, 4)), Boss(20, 5)
        ]);
    }
}
