namespace Game.Core;

public enum UnitType { Swordsman, Crossbowman, Enemy }
public enum CombatEventType { AttackStarted, Impact, Hit, Death, DefenderShot }
public readonly record struct WeaponProfile(int Health, int Damage, int FoodCost, double Range, double Speed, int WindupTicks, int CadenceTicks);
public sealed record CombatEvent(long Sequence, long Tick, CombatEventType Type, UnitState Unit, int TargetId = 0, bool TargetCity = false, int Damage = 0, bool Landed = false);

internal readonly record struct UnitIdentity(int Id, UnitType Type, int Owner, int Origin, int Destination);
internal readonly record struct UnitHealth(int Value);
internal readonly record struct UnitBody(double Forward, double Lateral, bool Deployed, double MoveForward = 0, double MoveLateral = 0, double FacingForward = 1, double FacingLateral = 0);
internal readonly record struct UnitTarget(int Id, bool City);
internal readonly record struct UnitAttack(long Sequence, long StartTick, long ImpactTick, long ReadyTick, bool Pending, int TargetId, bool TargetCity);

internal readonly record struct BattlePoint(double Forward, double Lateral)
{
    public double LengthSquared => Forward * Forward + Lateral * Lateral;
    public double Length => Math.Sqrt(LengthSquared);
    public BattlePoint Add(BattlePoint other) => new(Forward + other.Forward, Lateral + other.Lateral);
    public BattlePoint Subtract(BattlePoint other) => new(Forward - other.Forward, Lateral - other.Lateral);
    public BattlePoint Scale(double factor) => new(Forward * factor, Lateral * factor);
    public double Dot(BattlePoint other) => Forward * other.Forward + Lateral * other.Lateral;
    public BattlePoint Normalized() => Length > 1e-12 ? Scale(1 / Length) : default;
}
