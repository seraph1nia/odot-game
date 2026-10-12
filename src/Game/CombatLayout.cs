using Game.Core;
using Godot;

namespace Game;

// One shared-world projection; individual offsets are cosmetic formation slots.
// Neither offsets, lunges nor projectile flight affect authority positions.
internal sealed class CombatLayout(HexBoard board)
{
    private int _capacity = 3;
    internal static Vector3 ArenaOrigin => new(0, 0, 100);
    private Vector3 Offset => CellCenter(board.Cells[0].Id).Lerp(CellCenter(board.Cells[^1].Id), .5f);
    public static Vector3 Center(int city) => new((city - 1) * 40, 0, 0);
    private Vector3 CellCenter(int id)
    {
        HexCoordinate coordinate = board.Cell(id).Coordinate;
        return new(coordinate.Column * VillageLayout.HalfWidth * 2 + (Math.Abs(coordinate.R) % 2) * VillageLayout.HalfWidth, 0, coordinate.R * VillageLayout.RowStep);
    }
    internal Vector3 CellPosition(int id) => CellCenter(id) - Offset;
    internal void Synchronize(MatchSnapshot state) => _capacity = state.Rules.Arena.SquadCapacity;
    private Vector3 Formation(UnitState unit)
    {
        if (unit.IsBoss) return Vector3.Zero;
        int slot = unit.FormationSlot;
        float angle = -Mathf.Pi / 2 + slot * Mathf.Tau / _capacity;
        float radius = _capacity <= 3 ? .65f : 1.05f;
        return new(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
    }
    public Vector3 Position(UnitState unit, double tick)
    {
        HexUnitState hex = unit.Hex ?? throw new ArgumentException("Combat models require hex state.", nameof(unit));
        return Pose(hex, tick) + Formation(unit);
    }
    private Vector3 Pose(HexUnitState hex, double tick)
    {
        if (hex.Lifecycle is UnitLifecycle.Queued or UnitLifecycle.Reserve or UnitLifecycle.Stored) return ArenaOrigin;
        Vector3 position = CellPosition(hex.Position.Cell);
        if (hex.HoldsTransit)
        {
            Vector3 destination = CellPosition(hex.Destination.Cell);
            System.Numerics.Vector3 sampled = CombatVisualTiming.Position(new(position.X, position.Y, position.Z),
                new(destination.X, destination.Y, destination.Z), hex, tick);
            position = new(sampled.X, sampled.Y, sampled.Z);
        }
        return ArenaOrigin + position;
    }
    internal Vector3 SquadPosition(SquadState squad, double tick) => Pose(squad.Hex, tick);
    internal Vector3 StructurePosition(int owner, SquadTargetKind kind, ArenaState arena)
        => ArenaOrigin + CellPosition(arena.Footprints.Single(f => f.Owner == owner && f.Kind == (kind == SquadTargetKind.Watchpost ? ArenaStructureKind.Watchpost : ArenaStructureKind.Keep)).Cell);
    internal ArenaState? Arena { get; set; }
    public float Facing(UnitState unit, double tick, IReadOnlyDictionary<int, UnitState> units)
    {
        Vector3 from = Position(unit, tick), direction = Vector3.Zero;
        if (unit.Hex is { HoldsTransit: true } body) direction = CellPosition(body.Destination.Cell) - CellPosition(body.Position.Cell);
        else if (unit.Hex is { FrozenAim: { } aim } frozen && (frozen.Lifecycle == UnitLifecycle.Dying || frozen.FrozenTick is not null)) direction = AimPosition(unit, aim) - from;
        else if (unit.TargetCity && Arena is not null) direction = StructurePosition(unit.TargetId, unit.TargetKind, Arena) - from;
        else if (units.TryGetValue(unit.TargetId, out UnitState? target) && target.Deployed) direction = Position(target, tick) - from;
        if (direction.LengthSquared() < .00001f) direction = new(0, 0, unit.Faction == Faction.Adventurers ? -1 : 1);
        return Mathf.Atan2(direction.X, direction.Z);
    }
    public Vector3 AimPosition(UnitState unit, HexPosePoint aim) => PosePosition(unit.Destination, aim);
    public Vector3 PosePosition(int city, HexPosePoint pose)
    {
        HexUnitState target = new(0, city, Faction.Adventurers, UnitLifecycle.Alive, pose.Position,
            pose.Transition == 0 ? UnitActionKind.Waiting : UnitActionKind.Moving, pose.Destination, pose.Transition, StartTick: 0, EndTick: pose.DurationTicks);
        return Pose(target, pose.ElapsedTicks);
    }
    public Vector3? EventImpact(CombatEvent entry)
    {
        if (entry.Structure is { } structure) return ArenaOrigin + CellPosition(structure.Cell) + new Vector3(0, 1, 0);
        if (entry.TargetCity && entry.Unit is { } unit && Arena is not null) return StructurePosition(entry.TargetId, unit.TargetKind, Arena) + new Vector3(0, 1, 0);
        return entry.ImpactPose is { } pose ? PosePosition(1, pose) + new Vector3(0, .5f, 0) : null;
    }
}
