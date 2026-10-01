using System.Globalization;

namespace Game.Core;

// Health and damage on the wire are integer hundredths; resources remain whole units.
public static class HealthPoints
{
    public const int Scale = 100;
    public static int FromWhole(int value) => value < 0 ? throw new ArgumentOutOfRangeException(nameof(value)) : checked(value * Scale);
    public static int Ranked(int baseValue, int rank)
    {
        if (baseValue < 0 || rank is < 0 or > 2) throw new ArgumentOutOfRangeException(nameof(rank));
        return checked((int)(checked((long)baseValue * (100 + 5 * rank)) / 100));
    }
    public static string Format(int value) => (value / (decimal)Scale).ToString("0.##", CultureInfo.InvariantCulture);
}
public enum Faction { Adventurers, Skeletons }
public enum UnitClass { Melee, Ranged, Magic }
public readonly record struct ResourceCost(int Gold, int Wood = 0, int Food = 0)
{
    public bool CanPay(int gold, int wood, int food) => gold >= Gold && wood >= Wood && food >= Food;
}
public sealed record BuildingDefinition(Building Type, ResourceCost Construction, ResourceCost Upgrade, int LevelOneOutput = 0, int LevelTwoOutput = 0, UnitType[]? Recruits = null)
{
    public int Output(int level) => level switch { 1 => LevelOneOutput, 2 => LevelTwoOutput, _ => 0 };
}
public sealed record TowerDefinition(Building Type, int Level, int Damage, int WindupTicks, int CadenceTicks, int VictimCap, double SplashRadius);
public sealed record UnitDefinition(UnitType Type, UnitClass Class, ResourceCost Recruitment, WeaponProfile Profile);
public readonly record struct ResearchRanks(int Melee = 0, int Ranged = 0, int Magic = 0)
{
    public int For(UnitClass @class) => @class switch { UnitClass.Melee => Melee, UnitClass.Ranged => Ranged, UnitClass.Magic => Magic, _ => throw new ArgumentOutOfRangeException(nameof(@class)) };
    public ResearchRanks Increase(UnitClass @class) => @class switch { UnitClass.Melee => this with { Melee = Melee + 1 }, UnitClass.Ranged => this with { Ranged = Ranged + 1 }, UnitClass.Magic => this with { Magic = Magic + 1 }, _ => throw new ArgumentOutOfRangeException(nameof(@class)) };
}
public static class Catalogs
{
    public static BuildingDefinition[] Buildings(Rules rules) =>
    [
        new(Building.Farm, new(rules.BuildCost, 10), new(rules.UpgradeCost, 10), rules.FarmOutput, rules.FarmOutputLevelTwo),
        new(Building.Mine, new(rules.BuildCost), new(rules.UpgradeCost, 10), rules.MineOutput, rules.MineOutputLevelTwo),
        new(Building.Lumbermill, new(rules.BuildCost), new(rules.UpgradeCost, 10), rules.WoodOutput, rules.WoodOutputLevelTwo),
        new(Building.Barracks, new(rules.BuildCost, 10), new(rules.UpgradeCost, 10), Recruits: [UnitType.Swordsman, UnitType.Berserker]),
        new(Building.ArcheryRange, new(rules.BuildCost, 10), new(rules.UpgradeCost, 10), Recruits: [UnitType.Crossbowman]),
        new(Building.Arcanum, new(25, 10), new(rules.UpgradeCost, 10), Recruits: [UnitType.Mage]),
        new(Building.Blacksmith, new(rules.BuildCost, 10), new(rules.UpgradeCost, 10)),
        new(Building.ArrowTower, new(rules.BuildCost, 15), new(rules.UpgradeCost, 10)),
        new(Building.CatapultTower, new(30, 20), new(rules.UpgradeCost, 10))
    ];
    public static TowerDefinition[] Towers() =>
    [
        new(Building.ArrowTower, 1, 200, 12, 60, 1, 0), new(Building.ArrowTower, 2, 300, 12, 60, 1, 0),
        new(Building.CatapultTower, 1, 300, 30, 120, 3, .75), new(Building.CatapultTower, 2, 400, 30, 120, 3, .75)
    ];
    public static UnitDefinition[] Units(Rules rules) =>
    [
        new(UnitType.Swordsman, UnitClass.Melee, new(0, Food: rules.RecruitCost), new(HealthPoints.FromWhole(rules.SoldierHealth), HealthPoints.FromWhole(rules.SoldierDamage), Match.Reach, 1, rules.MeleeWindupTicks, rules.AttackTicks)),
        new(UnitType.Berserker, UnitClass.Melee, new(0, Food: 6), new(HealthPoints.FromWhole(rules.BerserkerHealth), HealthPoints.FromWhole(rules.BerserkerDamage), Match.Reach, 1.1, 18, 72)),
        new(UnitType.Crossbowman, UnitClass.Ranged, new(0, Food: rules.RangedRecruitCost), new(HealthPoints.FromWhole(rules.RangedHealth), HealthPoints.FromWhole(rules.RangedDamage), rules.RangedReach, 1, rules.RangedWindupTicks, rules.AttackTicks)),
        new(UnitType.Mage, UnitClass.Magic, new(2, Food: 7), new(HealthPoints.FromWhole(rules.MageHealth), HealthPoints.FromWhole(rules.MageDamage), 3, 1, 24, 90) { VictimCap = 3, SplashRadius = .75 })
    ];
    public static UnitClass Class(UnitType type) => type switch { UnitType.Swordsman or UnitType.Berserker => UnitClass.Melee, UnitType.Crossbowman => UnitClass.Ranged, UnitType.Mage => UnitClass.Magic, _ => throw new ArgumentOutOfRangeException(nameof(type)) };
    public static UnitType EnemyRole(int wave, int allocationIndex) => (wave switch
    { 1 => new[] { UnitType.Swordsman, UnitType.Berserker }, 2 => [UnitType.Swordsman, UnitType.Berserker, UnitType.Crossbowman], _ => [UnitType.Swordsman, UnitType.Berserker, UnitType.Crossbowman, UnitType.Mage] })[allocationIndex % (wave + 1)];
}
public sealed record TowerState(int City, int Slot, Building Type, int Level, long AttackSequence = 0, int TargetId = 0, long ActionStartTick = 0, long ImpactTick = 0, long ReadyTick = 0, bool PendingImpact = false);
