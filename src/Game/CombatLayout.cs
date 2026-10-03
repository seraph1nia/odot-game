using Game.Core;
using Godot;

namespace Game;

// Presentation-only mapping. Direct visual crossings never change numerical
// adjacency, range, capacity, committed endpoints or transit reservations.
internal sealed class CombatLayout(HexBoard board)
{
    public static Vector3 Center(int city) => new((city - 1) * 40, 0, 0);
    private static Vector3 Anchor(HexBoard board, HexPosition position)
    {
        HexCoordinate cell = board.Cell(position.Cell).Coordinate;
        HexAnchor anchor = board.Anchor(position.Anchor);
        return VillageLayout.Hex(cell.Column, cell.R) + new Vector3(anchor.AnchorX / 1000f, 0, anchor.AnchorForward / 1000f);
    }
    public Vector3 Position(UnitState unit, double tick)
    {
        HexUnitState hex = unit.Hex ?? throw new ArgumentException("Combat models require hex state.", nameof(unit));
        return Position(unit.Destination, hex, tick);
    }
    private Vector3 Position(int city, HexUnitState hex, double tick)
    {
        if (hex.Lifecycle is UnitLifecycle.Queued or UnitLifecycle.Reserve) return Center(city);
        Vector3 position = Anchor(board, hex.Position);
        if (hex.HoldsTransit)
        {
            Vector3 destination = Anchor(board, hex.Destination);
            System.Numerics.Vector3 sampled = CombatVisualTiming.Position(new(position.X, position.Y, position.Z),
                new(destination.X, destination.Y, destination.Z), hex, tick);
            position = new(sampled.X, sampled.Y, sampled.Z);
        }
        return Center(city) + position;
    }
    public float Facing(UnitState unit, double tick, IReadOnlyDictionary<int, UnitState> units)
    {
        Vector3 from = Position(unit, tick), direction = Vector3.Zero;
        if (unit.Hex is { HoldsTransit: true } body) direction = Anchor(board, body.Destination) - Anchor(board, body.Position);
        else if (unit.Hex is { FrozenAim: { } aim } frozen && (frozen.Lifecycle == UnitLifecycle.Dying || frozen.FrozenTick is not null))
            direction = AimPosition(unit, aim) - from;
        else if (unit.TargetCity) direction = Center(unit.Destination) + VillageLandscape.Defender - from;
        else if (units.TryGetValue(unit.TargetId, out UnitState? target) && target.Deployed) direction = Position(target, tick) - from;
        if (direction.LengthSquared() < .00001f) direction = new(0, 0, unit.Faction == Faction.Adventurers ? -1 : 1);
        return Mathf.Atan2(direction.X, direction.Z);
    }
    public Vector3 AimPosition(UnitState unit, HexPosePoint aim) => PosePosition(unit.Destination, aim);
    public Vector3 PosePosition(int city, HexPosePoint pose)
    {
        HexUnitState target = new(0, city, Faction.Adventurers, UnitLifecycle.Alive, pose.Position,
            pose.Transition == 0 ? UnitActionKind.Waiting : UnitActionKind.Moving, pose.Destination, pose.Transition, StartTick: 0, EndTick: pose.DurationTicks);
        return Position(city, target, pose.ElapsedTicks);
    }
    public Vector3? EventImpact(CombatEvent entry)
    {
        int city = entry.Tower?.City ?? entry.Unit?.Destination ?? 0;
        if (city == 0) return null;
        if (entry.TargetCity) return Center(city) + VillageLandscape.Home + new Vector3(0, .8f, 0);
        return entry.ImpactPose is { } pose ? PosePosition(city, pose) + new Vector3(0, .5f, 0) : null;
    }
}
