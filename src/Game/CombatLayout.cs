using Game.Core;
using Godot;

namespace Game;

// Presentation-only mapping. Hex adjacency, range and reservations never read
// these vectors. The anchor/transition identity selects one fixed route.
internal sealed class CombatLayout(HexBoard board)
{
    private const float TransitRing = 1.5f;
    private sealed record CachedRoute(Vector3[] Points, float[] Segments, float[] Cumulative);
    private readonly Dictionary<int, CachedRoute> _routes = [];
    public static Vector3 Center(int city) => new((city - 1) * 40, 0, 0);
    private static Vector3 Cell(HexBoard board, int cell)
    {
        HexCoordinate coordinate = board.Cell(cell).Coordinate;
        return VillageLayout.Hex(coordinate.Column, coordinate.R);
    }
    private static Vector3 Anchor(HexBoard board, HexPosition position)
    {
        HexAnchor anchor = board.Anchor(position.Anchor);
        return Cell(board, position.Cell) + new Vector3(anchor.AnchorX / 1000f, 0, anchor.AnchorForward / 1000f);
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
            int transition = board.Transition(hex.Position, hex.Destination).Id;
            double elapsed = hex.Lifecycle == UnitLifecycle.Dying ? hex.FrozenMoveTicks : (hex.FrozenTick ?? tick) - hex.StartTick;
            float fraction = (float)Math.Clamp(elapsed / Math.Max(1, hex.EndTick - hex.StartTick), 0, 1);
            if (!_routes.TryGetValue(transition, out CachedRoute? cached))
            {
                Vector3[] points = Route(board, hex.Position, hex.Destination);
                var segments = new float[points.Length]; var cumulative = new float[points.Length];
                for (int i = 1; i < points.Length; i++) { segments[i] = points[i - 1].DistanceTo(points[i]); cumulative[i] = cumulative[i - 1] + segments[i]; }
                cached = new(points, segments, cumulative); _routes.Add(transition, cached);
            }
            Vector3[] route = cached.Points;
            float remaining = cached.Cumulative[^1] * fraction;
            for (int i = 1; i < route.Length; i++)
            {
                float length = cached.Segments[i];
                if (length > .00001f && remaining <= length) { position = route[i - 1].Lerp(route[i], remaining / length); break; }
                remaining -= length; position = route[i];
            }
        }
        return Center(city) + position;
    }
    public float Facing(UnitState unit, double tick, IReadOnlyDictionary<int, UnitState> units)
    {
        Vector3 from = Position(unit, tick), direction = Vector3.Zero;
        if (unit.Hex is { HoldsTransit: true } body && (body.Lifecycle == UnitLifecycle.Dying || body.FrozenTick is not null))
        {
            UnitState moving = unit with { Hex = body with { Lifecycle = UnitLifecycle.Alive, FrozenTick = null } };
            double elapsed = body.Lifecycle == UnitLifecycle.Dying ? body.FrozenMoveTicks : body.FrozenTick!.Value - body.StartTick;
            direction = Position(moving, body.StartTick + elapsed + .25) - from;
        }
        else if (unit.Hex is { FrozenAim: { } aim } frozen && (frozen.Lifecycle == UnitLifecycle.Dying || frozen.FrozenTick is not null))
        {
            direction = AimPosition(unit, aim) - from;
        }
        else if (unit.Hex is { HoldsTransit: true, Lifecycle: UnitLifecycle.Alive, FrozenTick: null }) direction = Position(unit, tick + .25) - from;
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
    private static Vector3 Exit(HexBoard board, HexPosition position)
    {
        HexAnchor f = board.Anchor(position.Anchor);
        float x = f.AnchorX / 1000f;
        float sign = f.AnchorForward > 0 ? 1 : -1;
        return Cell(board, position.Cell) + new Vector3(x, 0, sign * Mathf.Sqrt(TransitRing * TransitRing - x * x));
    }
    private static void Arc(List<Vector3> route, Vector3 center, Vector3 from, Vector3 to)
    {
        float first = Mathf.Atan2(from.Z - center.Z, from.X - center.X), last = Mathf.Atan2(to.Z - center.Z, to.X - center.X);
        float turn = Mathf.Wrap(last - first, -Mathf.Pi, Mathf.Pi);
        for (int n = 1; n <= 8; n++)
        {
            float angle = first + turn * n / 8;
            route.Add(center + new Vector3(Mathf.Cos(angle) * TransitRing, 0, Mathf.Sin(angle) * TransitRing));
        }
    }
    private static Vector3[] Route(HexBoard board, HexPosition from, HexPosition to)
    {
        Vector3 source = Cell(board, from.Cell), destination = Cell(board, to.Cell), portal = (source + destination) / 2;
        Vector3 sourceExit = Exit(board, from), destinationExit = Exit(board, to);
        var route = new List<Vector3> { Anchor(board, from), sourceExit };
        Arc(route, source, sourceExit, portal); route.Add(portal);
        Arc(route, destination, portal, destinationExit); route.Add(destinationExit); route.Add(Anchor(board, to));
        return route.ToArray();
    }
}
