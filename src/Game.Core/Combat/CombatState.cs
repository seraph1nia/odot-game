namespace Game.Core;

public sealed record AdmissionBound(int City, long TransferTick, long FirstAdmissionBound, long? AdmissionTick = null, int? FirstUnitId = null);

public enum UnitType { Swordsman, Crossbowman, Berserker, Mage }
public enum CombatEventType { AttackStarted, Impact, Hit, Death, DefenderShot }
public readonly record struct WeaponProfile(int Health, int Damage, double Range, double Speed, int WindupTicks, int CadenceTicks)
{
    public int CapacityCost { get; init; }
    public int Initiative { get; init; }
    public int HexRange { get; init; }
    public int MoveTicks { get; init; }
    public int DeathTicks { get; init; }
    public int SplashHexRadius { get; init; }
    public int VictimCap { get; init; } = 1;
    public double SplashRadius { get; init; }
}
public sealed record CombatEvent(long Sequence, long Tick, CombatEventType Type, UnitState? Unit, int TargetId = 0, bool TargetCity = false, int Damage = 0, bool Landed = false)
{
    public TowerState? Tower { get; init; }
    public double ImpactForward { get; init; }
    public double ImpactLateral { get; init; }
    public int[] Victims { get; init; } = [];
}

internal readonly record struct UnitIdentity(int Id, UnitType Type, int Owner, int Origin, int Destination, Faction Faction, int Rank);
internal readonly record struct UnitHealth(int Value);
internal readonly record struct UnitTarget(int Id, bool City);
internal readonly record struct UnitAttack(long Sequence, long StartTick, long ImpactTick, long ReadyTick, bool Pending, int TargetId, bool TargetCity);
public sealed record CombatDecisionState(long Sequence, int Generation, ulong SchedulingRank, int ObjectiveId = 0, bool ObjectiveCity = false,
    int ObjectiveCell = 0, long ObservedRevision = -1, long RetryTick = 0)
{
    public HexPosition[] Route { get; init; } = [];
    public int[] Visited { get; init; } = [];
}
