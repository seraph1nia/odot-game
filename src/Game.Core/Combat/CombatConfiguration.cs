using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;

namespace Game.Core;

public sealed record UnitSpace(int Size, int Initiative, int MoveTicks, int DeathTicks);
public sealed record DefenseTiming(int WindupTicks, int RecoveryTicks, int VictimCap = 1, int SplashHexRadius = 0)
{
    public int CadenceTicks => checked(WindupTicks + RecoveryTicks);
}
public sealed record CombatSettings
{
    public int RulesVersion { get; init; } = 4;
    public HexBoardDefinition Board { get; init; } = HexBoardDefinition.Default();
    public UnitSpace Swordsman { get; init; } = new(2, 10, 30, 48);
    public UnitSpace Berserker { get; init; } = new(2, 20, 27, 48);
    public UnitSpace Crossbowman { get; init; } = new(2, 30, 30, 48);
    public UnitSpace Mage { get; init; } = new(2, 40, 30, 48);
    public int BerserkerWindupTicks { get; init; } = 18;
    public int BerserkerRecoveryTicks { get; init; } = 54;
    public int MageWindupTicks { get; init; } = 24;
    public int MageRecoveryTicks { get; init; } = 66;
    public int MageHexRange { get; init; } = 3;
    public int MageVictimCap { get; init; } = 2;
    public int MageSplashHexRadius { get; init; } = 1;
    public DefenseTiming Defender { get; init; } = new(12, 48);
    public DefenseTiming ArrowTower { get; init; } = new(12, 48);
    public DefenseTiming CatapultTower { get; init; } = new(30, 90, 3);
    public int ArrowLevelOneDamage { get; init; } = 5;
    public int ArrowLevelTwoDamage { get; init; } = 7;
    public int CatapultLevelOneDamage { get; init; } = 6;
    public int CatapultLevelTwoDamage { get; init; } = 8;
    public int RetryTicks { get; init; } = 6;
    public int NoHealthProgressTicks { get; init; } = 3600;
    public int MaximumWaveTicks { get; init; } = 18000;
}
public sealed record HexCombatProfile(UnitType Type, int Health, int Damage, int Size, int Initiative,
    int HexRange, int MoveTicks, int WindupTicks, int RecoveryTicks, int DeathTicks, int VictimCap = 1, int SplashHexRadius = 0)
{
    public int CadenceTicks => checked(WindupTicks + RecoveryTicks);
    public HexCombatProfile Researched(int rank) => this with { Health = HealthPoints.Ranked(Health, rank), Damage = HealthPoints.Ranked(Damage, rank) };
    internal WeaponProfile Runtime() => new(Health, Damage, HexRange, MoveTicks, WindupTicks, CadenceTicks)
    {
        Size = Size,
        Initiative = Initiative,
        DeathTicks = DeathTicks,
        VictimCap = VictimCap,
        SplashHexRadius = SplashHexRadius
    };
}
public readonly record struct CombatFingerprint(ulong A, ulong B, ulong C, ulong D)
{
    public override string ToString() => $"{A:x16}{B:x16}{C:x16}{D:x16}";
}

// One authority freezes its rules once. Neither authoring arrays nor returned
// definitions can mutate the board/profiles used by an ongoing fight.
public sealed class CombatConfiguration
{
    private WorkCounters? _work;
    internal WorkCounters? Work
    {
        get => _work;
        set { _work = value; value?.Support(WorkMetric.ProfileResolutions, WorkMetric.ProfileCacheMisses, WorkMetric.ProfileCacheHits); }
    }
    private readonly Dictionary<(UnitType Type, int Rank, bool Boss, int Level), HexCombatProfile> _resolved = [];
    internal int CachedProfiles => _resolved.Count;
    internal void ClearCache() { _resolved.Clear(); Work = null; }
    private readonly FrozenDictionary<UnitType, HexCombatProfile> _profiles;
    private readonly FrozenDictionary<(Building Type, int Level), DefenseProfile> _towerProfiles;
    internal DefenseProfile BuiltInDefense { get; }
    internal DefenseProfile Tower(Building type, int level) => _towerProfiles[(type, level)];
    public int RulesVersion { get; }
    public HexBoard Board { get; }
    public ReadOnlyCollection<HexCombatProfile> Units { get; }
    public DefenseTiming Defender { get; }
    public ReadOnlyCollection<TowerDefinition> Towers { get; }
    public int DefenderDamage { get; }
    public int RetryTicks { get; }
    public int NoHealthProgressTicks { get; }
    public int MaximumWaveTicks { get; }
    public CombatFingerprint Fingerprint { get; }

    public CombatConfiguration(Rules rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        CombatSettings settings = rules.Combat;
        ArgumentNullException.ThrowIfNull(settings);
        Board = new(settings.Board); RulesVersion = settings.RulesVersion;
        RetryTicks = settings.RetryTicks; NoHealthProgressTicks = settings.NoHealthProgressTicks; MaximumWaveTicks = settings.MaximumWaveTicks;
        Defender = settings.Defender; DefenderDamage = HealthPoints.FromWhole(rules.DefenderDamage);
        if (RulesVersion < 1 || RetryTicks <= 0 || NoHealthProgressTicks <= 0 || MaximumWaveTicks <= 0)
            throw new ArgumentException("Combat versions, retry intervals and limits must be positive.", nameof(rules));
        Units = Array.AsReadOnly(Enum.GetValues<UnitType>().Select(type => Profile(rules, type)).ToArray());
        foreach (HexCombatProfile profile in Units)
        {
            if (profile.Health <= 0 || profile.Damage < 0 || profile.Size is < 1 or > 6 || profile.Initiative < 0 || profile.HexRange < 1
                || profile.MoveTicks <= 0 || profile.WindupTicks <= 0 || profile.RecoveryTicks <= 0 || profile.DeathTicks <= 0
                || profile.VictimCap <= 0 || profile.SplashHexRadius < 0)
                throw new ArgumentException("Invalid combat profile or invalid size.", nameof(rules));
            _ = profile.Researched(2); _ = profile.CadenceTicks;
        }
        _profiles = Units.ToFrozenDictionary(p => p.Type);
        foreach (DefenseTiming timing in new[] { Defender, settings.ArrowTower, settings.CatapultTower })
        {
            if (timing.WindupTicks <= 0 || timing.RecoveryTicks <= 0 || timing.VictimCap <= 0 || timing.SplashHexRadius < 0)
                throw new ArgumentException("Defense windup/recovery and victim cap must be positive.", nameof(rules));
            _ = timing.CadenceTicks;
        }
        Towers = Array.AsReadOnly(TowerProfiles(rules));
        BuiltInDefense = new(DefenderDamage, Defender.WindupTicks, Defender.RecoveryTicks, Defender.VictimCap, Defender.SplashHexRadius);
        _towerProfiles = Towers.ToFrozenDictionary(p => (p.Type, p.Level),
            p => new DefenseProfile(p.Damage, p.WindupTicks, checked(p.CadenceTicks - p.WindupTicks), p.VictimCap, p.SplashHexRadius));
        // Reject values that could overflow the shared same-tick accumulator
        // even if every deployed body plus all nine towers hit one victim.
        int maxActors = checked(Board.Cells.Count * Board.Capacity + 10);
        if (Units.Any(p => p.Researched(2).Damage > int.MaxValue / maxActors)
            || Towers.Any(p => p.Damage > int.MaxValue / maxActors) || DefenderDamage > int.MaxValue / maxActors)
            throw new ArgumentException("Damage could overflow a simultaneous accumulator.", nameof(rules));
        long approachAllowance = (long)Board.Diameter * Units.Max(p => p.MoveTicks) + Units.Max(p => p.CadenceTicks)
            + Units.Max(p => p.DeathTicks) + RetryTicks;
        if (NoHealthProgressTicks < approachAllowance)
            throw new ArgumentException("No-health-progress allowance is shorter than an ordinary approach/action/cleanup interval.", nameof(rules));
        foreach (UnitType type in Enum.GetValues<UnitType>())
            for (int level = 1; level <= 6; level++) _ = Unit(type, rank: 2, level: level);
        _ = Unit(UnitType.Swordsman, rank: 2, isBoss: true, level: 3);
        _ = Unit(UnitType.Swordsman, rank: 2, isBoss: true, level: 5);
        Fingerprint = ComputeFingerprint();
    }

    public HexCombatProfile Unit(UnitType type, int rank = 0, bool isBoss = false, int level = 1)
    {
        Work?.Add(WorkMetric.ProfileResolutions);
        var key = (type, rank, isBoss, level);
        if (_resolved.TryGetValue(key, out HexCombatProfile? cached)) { Work?.Add(WorkMetric.ProfileCacheHits); return cached; }
        Work?.Add(WorkMetric.ProfileCacheMisses);
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
        HexCombatProfile original = _profiles[type];
        int health = Progression.ScaleWhole(original.Health / HealthPoints.Scale, level);
        int damage = Progression.ScaleWhole(original.Damage / HealthPoints.Scale, level);
        if (isBoss) { health = checked(health * Progression.BossHealthMultiplier); damage = checked(damage * Progression.BossDamageMultiplier); }
        HexCombatProfile resolved = (original with
        { Health = HealthPoints.FromWhole(health), Damage = HealthPoints.FromWhole(damage), Size = isBoss ? 6 : original.Size }).Researched(rank);
        int maximumActors = checked(Board.Cells.Count * Board.Capacity + 10);
        if (resolved.Damage > int.MaxValue / maximumActors) throw new ArgumentException("Leveled damage could overflow the simultaneous accumulator.", nameof(level));
        _resolved.Add(key, resolved); return resolved;
    }
    internal static HexCombatProfile Profile(Rules rules, UnitType type)
    {
        CombatSettings c = rules.Combat;
        UnitSpace space = type switch
        { UnitType.Swordsman => c.Swordsman, UnitType.Berserker => c.Berserker, UnitType.Crossbowman => c.Crossbowman, UnitType.Mage => c.Mage, _ => throw new ArgumentOutOfRangeException(nameof(type)) };
        int health = type switch { UnitType.Swordsman => rules.SoldierHealth, UnitType.Berserker => rules.BerserkerHealth, UnitType.Crossbowman => rules.RangedHealth, _ => rules.MageHealth };
        int damage = type switch { UnitType.Swordsman => rules.SoldierDamage, UnitType.Berserker => rules.BerserkerDamage, UnitType.Crossbowman => rules.RangedDamage, _ => rules.MageDamage };
        if (!double.IsFinite(rules.RangedReach) || rules.RangedReach < 1 || rules.RangedReach > int.MaxValue || rules.RangedReach != Math.Truncate(rules.RangedReach))
            throw new ArgumentException("Ranged reach must be an integer hex distance.", nameof(rules));
        int range = type switch { UnitType.Swordsman or UnitType.Berserker => 1, UnitType.Crossbowman => (int)rules.RangedReach, _ => c.MageHexRange };
        int windup = type switch { UnitType.Swordsman => rules.MeleeWindupTicks, UnitType.Berserker => c.BerserkerWindupTicks, UnitType.Crossbowman => rules.RangedWindupTicks, _ => c.MageWindupTicks };
        int recovery = type switch { UnitType.Swordsman or UnitType.Crossbowman => checked(rules.AttackTicks - windup), UnitType.Berserker => c.BerserkerRecoveryTicks, _ => c.MageRecoveryTicks };
        return new(type, HealthPoints.FromWhole(health), HealthPoints.FromWhole(damage), space.Size, space.Initiative, range,
            space.MoveTicks, windup, recovery, space.DeathTicks, type == UnitType.Mage ? c.MageVictimCap : 1, type == UnitType.Mage ? c.MageSplashHexRadius : 0);
    }
    internal static TowerDefinition[] TowerProfiles(Rules rules)
    {
        CombatSettings c = rules.Combat;
        TowerDefinition Tower(Building type, int level, int damage, DefenseTiming timing) => new(type, level, HealthPoints.FromWhole(damage),
            timing.WindupTicks, timing.CadenceTicks, timing.VictimCap, timing.SplashHexRadius);
        return [Tower(Building.ArrowTower, 1, c.ArrowLevelOneDamage, c.ArrowTower), Tower(Building.ArrowTower, 2, c.ArrowLevelTwoDamage, c.ArrowTower),
            Tower(Building.CatapultTower, 1, c.CatapultLevelOneDamage, c.CatapultTower), Tower(Building.CatapultTower, 2, c.CatapultLevelTwoDamage, c.CatapultTower)];
    }
    private CombatFingerprint ComputeFingerprint()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            // BinaryWriter integers are little endian; fields/collections have
            // fixed order, explicit lengths, and no runtime object hashes.
            writer.Write(RulesVersion); writer.Write(Progression.Growth); writer.Write(Progression.MaximumExponentLevel);
            writer.Write(Progression.BossHealthMultiplier); writer.Write(Progression.BossDamageMultiplier); writer.Write(6); // Boss size and size-budget semantics.
            writer.Write(Board.Id); writer.Write(Board.Version); writer.Write(Board.Capacity);
            writer.Write(Board.Cells.Count);
            foreach (HexCell cell in Board.Cells)
            {
                writer.Write(cell.Id); writer.Write(cell.Coordinate.Q); writer.Write(cell.Coordinate.R); writer.Write((int)cell.Affinity);
                writer.Write(cell.Neighbors.Count); foreach (int n in cell.Neighbors) writer.Write(n);
            }
            writer.Write(Board.Anchors.Count);
            foreach (HexAnchor f in Board.Anchors)
            { writer.Write(f.Id); writer.Write(f.AnchorX); writer.Write(f.AnchorForward); }
            foreach (Faction faction in Enum.GetValues<Faction>())
            {
                writer.Write((int)faction); writer.Write(Board.Rear(faction).Count); foreach (int c in Board.Rear(faction)) writer.Write(c);
                writer.Write(Board.Front(faction).Count); foreach (int c in Board.Front(faction)) writer.Write(c);
            }
            writer.Write(Board.SiegeCells.Count); foreach (int c in Board.SiegeCells) writer.Write(c);
            writer.Write(Board.Transitions.Count);
            foreach (HexTransition t in Board.Transitions)
            { writer.Write(t.Id); writer.Write(t.Source.Cell); writer.Write(t.Source.Anchor); writer.Write(t.Destination.Cell); writer.Write(t.Destination.Anchor); writer.Write(t.EdgeToken); }
            writer.Write(Units.Count);
            foreach (HexCombatProfile p in Units)
            {
                writer.Write((int)p.Type); writer.Write(p.Health); writer.Write(p.Damage); writer.Write(p.Size); writer.Write(p.Initiative);
                writer.Write(p.HexRange); writer.Write(p.MoveTicks); writer.Write(p.WindupTicks); writer.Write(p.RecoveryTicks); writer.Write(p.DeathTicks);
                writer.Write(p.VictimCap); writer.Write(p.SplashHexRadius);
            }
            writer.Write(DefenderDamage); writer.Write(Defender.WindupTicks); writer.Write(Defender.RecoveryTicks); writer.Write(Defender.VictimCap); writer.Write(Defender.SplashHexRadius);
            writer.Write(Towers.Count);
            foreach (TowerDefinition p in Towers)
            { writer.Write((int)p.Type); writer.Write(p.Level); writer.Write(p.Damage); writer.Write(p.WindupTicks); writer.Write(p.CadenceTicks); writer.Write(p.VictimCap); writer.Write(p.SplashHexRadius); }
            writer.Write(RetryTicks); writer.Write(NoHealthProgressTicks); writer.Write(MaximumWaveTicks);
        }
        byte[] hash = SHA256.HashData(stream.ToArray());
        return new(BinaryPrimitives.ReadUInt64LittleEndian(hash), BinaryPrimitives.ReadUInt64LittleEndian(hash.AsSpan(8)),
            BinaryPrimitives.ReadUInt64LittleEndian(hash.AsSpan(16)), BinaryPrimitives.ReadUInt64LittleEndian(hash.AsSpan(24)));
    }
}
