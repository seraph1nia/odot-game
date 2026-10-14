using Game.Core;
using Godot;

namespace Game;

// One shared-world projection; individual offsets are cosmetic formation slots.
// Neither offsets, lunges nor projectile flight affect authority positions.
internal sealed class CombatLayout(HexBoard board)
{
    private int _capacity = 3;
    private string? _match;
    private readonly Dictionary<int, Vector3> _formations = []; // Cosmetic offsets only; reset per match/playback layout.
    internal static Vector3 ArenaOrigin => Convert(SharedCombatProjection.Origin);
    private static Vector3 Convert(System.Numerics.Vector3 point) => new(point.X, point.Y, point.Z);
    private Dictionary<int, Vector3> _preview = [];
    internal void SetPreview(UnitState[] manifest) => _preview = NextWaveFormation.Positions(manifest);
    public static Vector3 Center(int city) => new((city - 1) * 40, 0, 0);
    internal Vector3 CellPosition(int id) => Convert(SharedCombatProjection.CellPosition(board, id));
    internal void Synchronize(MatchSnapshot state)
    {
        if (_match != state.MatchId) { _formations.Clear(); _match = state.MatchId; }
        // A lobby can grow before its match identity/playback generation
        // changes. Geometry and physical footprints belong to the accepted
        // roster, not the first one-player welcome snapshot.
        if (state.Arena is { } arena)
        {
            if (board.Cells.Count != arena.Board.Cells.Length) board = new HexBoard(arena.Board);
            Arena = arena;
        }
        _capacity = state.Rules.Arena.SquadCapacity;
        foreach (UnitState member in state.CombatEvents.Where(e => e.Unit is not null).Select(e => e.Unit!)
            .Concat(state.Enemies).Concat(state.Players.SelectMany(c => c.Soldiers)).Concat(state.DyingBodies))
            _formations[member.Id] = Formation(member);
    }
    private Vector3 Formation(UnitState unit) => Convert(SharedCombatProjection.Formation(unit.FormationSlot, _capacity, unit.IsBoss));
    public Vector3 Position(UnitState unit, double tick)
    {
        if (unit.Id < 0 && _preview.TryGetValue(unit.Id, out Vector3 preview)) return preview;
        HexUnitState hex = unit.Hex ?? throw new ArgumentException("Combat models require hex state.", nameof(unit));
        return Pose(hex, tick) + Formation(unit);
    }
    private Vector3 Pose(HexUnitState hex, double tick) => Convert(SharedCombatProjection.Pose(board, hex, tick));
    internal Vector3 SquadPosition(HexUnitState hex, double tick) => Pose(hex, tick);
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
        else if (unit.Hex?.FrozenAim is { } lost) direction = AimPosition(unit, lost) - from;
        if (direction.LengthSquared() < .00001f) direction = new(0, 0, unit.Faction == Faction.Adventurers ? -1 : 1);
        return Mathf.Atan2(direction.X, direction.Z);
    }
    public Vector3 AimPosition(UnitState unit, HexPosePoint aim)
        => PosePosition(unit.Destination, aim) + (!unit.TargetCity ? _formations.GetValueOrDefault(unit.TargetId) : Vector3.Zero);
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
        if (entry.ImpactPose is not { } pose) return null;
        // Launch predicts only the target's already committed transition, never
        // a later move or guaranteed hit. Impact uses the actual pre-damage pose.
        if (entry.Type == CombatEventType.AttackStarted)
        {
            long untilImpact = (entry.Tower?.ImpactTick ?? entry.Unit!.ImpactTick) - entry.Tick;
            pose = pose with { ElapsedTicks = checked(pose.ElapsedTicks + (int)Math.Clamp(untilImpact, 0, Math.Max(0, pose.DurationTicks - pose.ElapsedTicks))) };
        }
        Vector3 formation = entry.Type is CombatEventType.Hit or CombatEventType.Death && entry.Unit is { } member
            ? Formation(member) : _formations.GetValueOrDefault(entry.TargetId);
        return PosePosition(1, pose) + formation + new Vector3(0, .5f, 0);
    }
}
