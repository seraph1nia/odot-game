using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.Numerics;

namespace Game.Core;

public enum DeploymentAffinity { Neutral, Adventurers, Skeletons }
public readonly record struct HexCoordinate(int Q, int R)
{
    public int Column => Q + (R - (R & 1)) / 2;
    public static HexCoordinate FromOffset(int column, int row) => new(column - (row - (row & 1)) / 2, row);
    public int Distance(HexCoordinate other)
    {
        long q = (long)Q - other.Q, r = (long)R - other.R;
        return checked((int)((Math.Abs(q) + Math.Abs(r) + Math.Abs(q + r)) / 2));
    }
}

// Authoring DTOs are copied/validated by HexBoard; arrays never become live rules.
public sealed record HexCellDefinition(int Id, HexCoordinate Coordinate, DeploymentAffinity Affinity, int[] Neighbors);
public sealed record HexFootprint(int Id, int CapacityCost, int Mask, int AnchorX, int AnchorForward);
public sealed record HexEntryDefinition(Faction Faction, int[] RearCells, int[] FrontCells);
public sealed record HexBoardDefinition(string Id, int Version, int Capacity, HexCellDefinition[] Cells,
    HexFootprint[] Footprints, HexEntryDefinition[] Entries, int[] SiegeCells)
{
    public static HexBoardDefinition Default()
    {
        var cells = new List<HexCellDefinition>();
        for (int row = -5; row <= 1; row++)
            for (int column = -1; column <= 1; column++)
                cells.Add(new(cells.Count + 1, HexCoordinate.FromOffset(column, row),
                    row == -5 ? DeploymentAffinity.Skeletons : row == 1 ? DeploymentAffinity.Adventurers : DeploymentAffinity.Neutral, []));
        HexCellDefinition[] graph = cells.Select(c => c with
        { Neighbors = cells.Where(n => n.Id != c.Id && n.Coordinate.Distance(c.Coordinate) == 1).Select(n => n.Id).ToArray() }).ToArray();
        // Six authored positions in two rows. Anchors are thousandths of a world
        // unit, solely for presentation. Masks, never anchors, decide legality.
        int[] x = [-900, 0, 900, -900, 0, 900], forward = [-550, -550, -550, 550, 550, 550];
        int[] masks = [1, 2, 4, 8, 16, 32, 3, 6, 24, 48, 9, 18, 36, 7, 56, 63];
        HexFootprint[] footprints = masks.Select((mask, index) =>
        {
            int[] positions = Enumerable.Range(0, 6).Where(p => (mask & (1 << p)) != 0).ToArray();
            return new HexFootprint(index + 1, positions.Length, mask, positions.Sum(p => x[p]) / positions.Length,
                positions.Sum(p => forward[p]) / positions.Length);
        }).ToArray();
        int[] Row(int row, bool reverse = false) => graph.Where(c => c.Coordinate.R == row).OrderBy(c => Math.Abs(c.Coordinate.Column))
            .ThenBy(c => reverse ? -c.Coordinate.Column : c.Coordinate.Column).Select(c => c.Id).ToArray();
        return new("village-hex", 1, 6, graph, footprints,
            [new(Faction.Adventurers, Row(1), Row(0)), new(Faction.Skeletons, Row(-5, true), Row(-4, true))], Row(0));
    }
}

public sealed class HexCell
{
    public int Id { get; }
    public HexCoordinate Coordinate { get; }
    public DeploymentAffinity Affinity { get; }
    public ReadOnlyCollection<int> Neighbors { get; }
    internal HexCell(HexCellDefinition definition)
    { Id = definition.Id; Coordinate = definition.Coordinate; Affinity = definition.Affinity; Neighbors = Array.AsReadOnly(definition.Neighbors.Order().ToArray()); }
}
public readonly record struct HexPosition(int Cell, int Footprint);
public readonly record struct HexTransition(int Id, HexPosition Source, HexPosition Destination, int EdgeToken);

// Immutable graph/footprint catalog and precomputed queries. The city anchor is
// a distance endpoint, deliberately absent from adjacency and transit routes.
public sealed class HexBoard
{
    private readonly FrozenDictionary<int, HexCell> _cells;
    private readonly FrozenDictionary<int, HexFootprint> _footprints;
    private readonly FrozenDictionary<(int Source, int Destination), int> _distances;
    private readonly FrozenDictionary<(Faction Faction, int Source, int Destination), int> _movementDistances;
    private readonly FrozenDictionary<(HexPosition Source, HexPosition Destination), HexTransition> _transitions;
    private readonly FrozenDictionary<Faction, (ReadOnlyCollection<int> Rear, ReadOnlyCollection<int> Front)> _entries;
    public string Id { get; }
    public int Version { get; }
    public int Capacity { get; }
    public int FullMask => (1 << Capacity) - 1;
    public int Diameter { get; }
    public ReadOnlyCollection<HexCell> Cells { get; }
    public ReadOnlyCollection<HexFootprint> Footprints { get; }
    public ReadOnlyCollection<int> SiegeCells { get; }
    public ReadOnlyCollection<HexTransition> Transitions { get; }

    public HexBoard(HexBoardDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (definition.Cells is null || definition.Footprints is null || definition.Entries is null || definition.SiegeCells is null)
            throw new ArgumentException("Missing board data.", nameof(definition));
        if (string.IsNullOrWhiteSpace(definition.Id) || definition.Version < 1 || definition.Capacity is < 1 or > 12
            || definition.Cells.Length is < 2 or > 256 || definition.Footprints.Length is < 1 or > 128)
            throw new ArgumentException("Invalid board identity or bounded capacity/topology.", nameof(definition));
        Id = definition.Id; Version = definition.Version; Capacity = definition.Capacity;
        if (definition.Cells.Any(c => c is null || c.Neighbors is null || c.Id <= 0 || !Enum.IsDefined(c.Affinity)
            || Math.Abs((long)c.Coordinate.Q) > 10000 || Math.Abs((long)c.Coordinate.R) > 10000
            || c.Neighbors.Length > 6 || c.Neighbors.Distinct().Count() != c.Neighbors.Length)
            || definition.Cells.Select(c => c.Id).Distinct().Count() != definition.Cells.Length
            || definition.Cells.Select(c => c.Coordinate).Distinct().Count() != definition.Cells.Length)
            throw new ArgumentException("Invalid or duplicate cells/neighbors.", nameof(definition));
        Cells = Array.AsReadOnly(definition.Cells.OrderBy(c => c.Id).Select(c => new HexCell(c)).ToArray());
        _cells = Cells.ToFrozenDictionary(c => c.Id);
        foreach (HexCell cell in Cells)
            foreach (int neighbor in cell.Neighbors)
                if (!_cells.TryGetValue(neighbor, out HexCell? other) || cell.Coordinate.Distance(other.Coordinate) != 1 || !other.Neighbors.Contains(cell.Id))
                    throw new ArgumentException("Neighbors must be known adjacent axial cells with reciprocal links.", nameof(definition));
        var distances = new Dictionary<(int, int), int>();
        foreach (HexCell source in Cells)
        {
            var queue = new Queue<int>(); queue.Enqueue(source.Id); distances[(source.Id, source.Id)] = 0;
            while (queue.TryDequeue(out int current))
                foreach (int next in Cell(current).Neighbors)
                    if (distances.TryAdd((source.Id, next), distances[(source.Id, current)] + 1)) queue.Enqueue(next);
            if (Cells.Any(c => !distances.ContainsKey((source.Id, c.Id))))
                throw new ArgumentException("Combat board must be connected.", nameof(definition));
        }
        _distances = distances.ToFrozenDictionary(); Diameter = distances.Values.Max();
        if (definition.Footprints.Any(f => f is null || f.Id <= 0 || f.CapacityCost <= 0 || f.Mask <= 0 || (f.Mask & ~FullMask) != 0
            || BitOperations.PopCount((uint)f.Mask) != f.CapacityCost || Math.Abs((long)f.AnchorX) > 10000 || Math.Abs((long)f.AnchorForward) > 10000)
            || definition.Footprints.Select(f => f.Id).Distinct().Count() != definition.Footprints.Length
            || definition.Footprints.Select(f => f.Mask).Distinct().Count() != definition.Footprints.Length)
            throw new ArgumentException("Invalid or duplicate footprint masks/anchors.", nameof(definition));
        Footprints = Array.AsReadOnly(definition.Footprints.OrderBy(f => f.Id).ToArray());
        _footprints = Footprints.ToFrozenDictionary(f => f.Id);
        if (Footprints.Any(a => Footprints.Any(b => b.Id > a.Id && (a.Mask & b.Mask) == 0 && a.AnchorX == b.AnchorX && a.AnchorForward == b.AnchorForward)))
            throw new ArgumentException("Disjoint footprints require distinct anchors.", nameof(definition));
        if (definition.Entries.Length != 2 || definition.Entries.Any(e => e is null || e.RearCells is null || e.FrontCells is null)
            || definition.Entries.Select(e => e.Faction).Distinct().Count() != 2)
            throw new ArgumentException("Both factions need one entry definition.", nameof(definition));
        var entries = new Dictionary<Faction, (ReadOnlyCollection<int>, ReadOnlyCollection<int>)>();
        foreach (HexEntryDefinition entry in definition.Entries)
        {
            if (!Enum.IsDefined(entry.Faction) || entry.RearCells.Length == 0 || entry.FrontCells.Length == 0
                || entry.RearCells.Concat(entry.FrontCells).Distinct().Count() != entry.RearCells.Length + entry.FrontCells.Length
                || entry.RearCells.Any(id => !_cells.ContainsKey(id) || Cell(id).Affinity != Affinity(entry.Faction))
                || entry.FrontCells.Any(id => !_cells.ContainsKey(id) || Cell(id).Affinity != DeploymentAffinity.Neutral)
                || entry.RearCells.Any(id => !Cell(id).Neighbors.Any(entry.FrontCells.Contains)))
                throw new ArgumentException("Protected rear entries must connect to their neutral front band.", nameof(definition));
            entries.Add(entry.Faction, (Array.AsReadOnly(entry.RearCells.ToArray()), Array.AsReadOnly(entry.FrontCells.ToArray())));
        }
        _entries = entries.ToFrozenDictionary();
        var movementDistances = new Dictionary<(Faction, int, int), int>();
        foreach (Faction faction in Enum.GetValues<Faction>())
            foreach (HexCell source in Cells.Where(c => Allows(faction, c.Id)))
            {
                var search = new HexReachability(this, source.Id, cell => Allows(faction, cell));
                foreach ((int cell, int distance) in search.Distances) movementDistances.Add((faction, source.Id, cell), distance);
            }
        _movementDistances = movementDistances.ToFrozenDictionary();
        if (definition.SiegeCells.Length == 0 || definition.SiegeCells.Distinct().Count() != definition.SiegeCells.Length
            || definition.SiegeCells.Any(id => !_cells.ContainsKey(id) || Cell(id).Affinity != DeploymentAffinity.Neutral))
            throw new ArgumentException("City anchor requires neutral siege links.", nameof(definition));
        SiegeCells = Array.AsReadOnly(definition.SiegeCells.Order().ToArray());
        foreach (Faction faction in Enum.GetValues<Faction>())
            foreach (int rear in Rear(faction))
            {
                var reachable = new HashSet<int> { rear }; var queue = new Queue<int>(); queue.Enqueue(rear);
                while (queue.TryDequeue(out int current))
                    foreach (int next in Cell(current).Neighbors)
                        if (Allows(faction, next) && reachable.Add(next)) queue.Enqueue(next);
                if (!Front(faction).Any(reachable.Contains) || !SiegeCells.Any(reachable.Contains))
                    throw new ArgumentException("Each protected entry needs a legal frontier and city approach.", nameof(definition));
            }
        long transitionCount = Cells.Sum(c => c.Neighbors.Count) * Footprints.GroupBy(f => f.CapacityCost).Sum(g => (long)g.Count() * g.Count());
        if (transitionCount > 500000) throw new ArgumentException("Board has too many footprint transitions.", nameof(definition));
        var transitions = new List<HexTransition>(); var edges = new Dictionary<(int, int), int>();
        foreach (HexCell source in Cells)
            foreach (int next in source.Neighbors)
            {
                var edge = (Math.Min(source.Id, next), Math.Max(source.Id, next));
                if (!edges.TryGetValue(edge, out int token)) edges.Add(edge, token = edges.Count + 1);
                foreach (HexFootprint from in Footprints)
                    foreach (HexFootprint to in Footprints.Where(f => f.CapacityCost == from.CapacityCost))
                        transitions.Add(new(transitions.Count + 1, new(source.Id, from.Id), new(next, to.Id), token));
            }
        Transitions = Array.AsReadOnly(transitions.ToArray());
        _transitions = transitions.ToFrozenDictionary(t => (t.Source, t.Destination));
    }

    public HexCell Cell(int id) => _cells[id];
    public HexFootprint Footprint(int id) => _footprints[id];
    public int Distance(int source, int destination) => _distances[(source, destination)];
    public int MovementDistance(Faction faction, int source, int destination) => _movementDistances.GetValueOrDefault((faction, source, destination), int.MaxValue);
    public int CityDistance(int source) => SiegeCells.Min(cell => Distance(source, cell)) + 1;
    public bool Allows(Faction faction, int cell) => Cell(cell).Affinity is DeploymentAffinity.Neutral || Cell(cell).Affinity == Affinity(faction);
    public ReadOnlyCollection<int> Rear(Faction faction) => _entries[faction].Rear;
    public ReadOnlyCollection<int> Front(Faction faction) => _entries[faction].Front;
    public IEnumerable<HexFootprint> Fits(int capacityCost, int occupiedMask) => Footprints.Where(f => f.CapacityCost == capacityCost && (f.Mask & occupiedMask) == 0);
    public HexTransition Transition(HexPosition source, HexPosition destination) => _transitions[(source, destination)];
    public HexBoardDefinition Definition() => new(Id, Version, Capacity,
        Cells.Select(c => new HexCellDefinition(c.Id, c.Coordinate, c.Affinity, c.Neighbors.ToArray())).ToArray(), Footprints.ToArray(),
        Enum.GetValues<Faction>().Select(f => new HexEntryDefinition(f, Rear(f).ToArray(), Front(f).ToArray())).ToArray(), SiegeCells.ToArray());
    private static DeploymentAffinity Affinity(Faction faction) => faction switch
    { Faction.Adventurers => DeploymentAffinity.Adventurers, Faction.Skeletons => DeploymentAffinity.Skeletons, _ => throw new ArgumentOutOfRangeException(nameof(faction)) };
}
