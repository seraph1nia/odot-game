using System.Numerics;
using Game.Core;

namespace Game;

// Pure presentation calculations: the renderer and cheap tests share this seam.
// No rules, reservations, random choices or independent wall clock live here.
internal static class CombatVisualTiming
{
    public static double MoveFraction(HexUnitState hex, double tick)
    {
        if (!hex.HoldsTransit) return 0;
        double elapsed = hex.Lifecycle == UnitLifecycle.Dying ? hex.FrozenMoveTicks : (hex.FrozenTick ?? tick) - hex.StartTick;
        return Math.Clamp(elapsed / Math.Max(1, hex.EndTick - hex.StartTick), 0, 1);
    }
    public static Vector3 Position(Vector3 source, Vector3 destination, HexUnitState hex, double tick)
        => Vector3.Lerp(source, destination, (float)MoveFraction(hex, tick));
    public static double Walking(HexUnitState? hex, double tick)
        => hex is { Lifecycle: UnitLifecycle.Alive, HoldsTransit: true }
            ? .8 * Envelope(hex.FrozenTick ?? tick, hex.StartTick, hex.EndTick, 4, 4) : 0;
    public static double Attack(UnitState unit, double tick)
        => unit.Hex?.Lifecycle == UnitLifecycle.Dying || unit.AttackSequence == 0 || unit.TargetId == 0 ? 0
            : Envelope(unit.Hex?.FrozenTick ?? tick, unit.ActionStartTick, unit.ReadyTick, Math.Min(3, unit.ImpactTick - unit.ActionStartTick), 4);
    public static double Hit(double seconds, double started, bool dead, bool attacking)
        => dead || started < 0 ? 0 : Envelope(seconds, started, started + .15, .025, .05) * (attacking ? .3 : 1);
    private static double Envelope(double time, double start, double end, double fadeIn, double fadeOut)
    {
        if (time < start || time >= end) return 0;
        double duration = Math.Max(0, end - start);
        double entrance = Math.Clamp((time - start) / Math.Max(.00001, Math.Min(fadeIn, duration / 2)), 0, 1);
        double exit = Math.Clamp((end - time) / Math.Max(.00001, Math.Min(fadeOut, duration / 2)), 0, 1);
        double weight = Math.Min(entrance, exit);
        return weight * weight * (3 - 2 * weight);
    }
    public const int IntentDashes = 12;
    public const int StrikeTrailTicks = 12;
    public const float LocalStrikeRadius = .34f;
    public static string MeleePhase(UnitState actor, double tick)
    {
        tick = actor.Hex?.FrozenTick ?? tick;
        if (actor.Hex?.Lifecycle != UnitLifecycle.Alive || actor.Hex.Action is not (UnitActionKind.Windup or UnitActionKind.Recovery)
            || actor.TargetId == 0 || actor.TargetCity || actor.AttackSequence == 0 || tick < actor.ActionStartTick || tick >= actor.ImpactTick + StrikeTrailTicks) return "none";
        return tick < actor.ImpactTick ? "intent" : actor.AttackLanded == true ? "landed" : "miss";
    }
    public static float Facing(float current, float desired, double elapsedTicks)
    {
        if (elapsedTicks <= 0) return current;
        double turn = Math.IEEERemainder(desired - current, Math.Tau);
        return (float)(current + turn * (1 - Math.Exp(-elapsedTicks / 6)));
    }
}
