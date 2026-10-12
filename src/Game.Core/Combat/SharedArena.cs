using System.Collections.ObjectModel;

namespace Game.Core;

public sealed record ArenaSettings
{
    public int SquadCapacity { get; init; } = 3;
    public int WatchpostHealth { get; init; } = 50;
    public int RallyDamageReductionPercent { get; init; } = 20;
    public int RallyDurationTicks { get; init; } = 240;
    public int DefenderRange { get; init; } = 4;
    public int ArrowRange { get; init; } = 4;
    public int CatapultRange { get; init; } = 5;
    public int SwordsmanSecondaryDamage { get; init; }
    public int BerserkerSecondaryDamage { get; init; } = 3;
    public int CrossbowmanSecondaryDamage { get; init; }
    public int MageSecondaryDamage { get; init; } = 4;
    public int SwordsmanStructureDamage { get; init; } = 5;
    public int BerserkerStructureDamage { get; init; } = 7;
    public int CrossbowmanStructureDamage { get; init; } = 3;
    public int MageStructureDamage { get; init; } = 4;
    public int CatapultSecondaryDamage { get; init; } = 6;
}

public enum ArenaStructureKind { Keep, Watchpost, ArrowTower, CatapultTower }
public sealed record ArenaFootprint(int Owner, ArenaStructureKind Kind, int Slot, int Cell);
public sealed record ArenaSector(int Owner, int[] DeploymentCells, int[] EnemyCells);
public sealed record ArenaState(HexBoardDefinition Board, ArenaSector[] Sectors, ArenaFootprint[] Footprints);

// The battlefield's single topology/footprint owner. Settlement plots are not
// battlefield cells. Sectors define legal preparation, never combat barriers.
public sealed class SharedArena
{
    public const int SectorWidth = 7, Depth = 9;
    private readonly ArenaSector[] _sectors;
    private readonly ArenaFootprint[] _footprints;
    private readonly HashSet<int> _staticCells;
    public HexBoard Board { get; }
    public ReadOnlyCollection<ArenaFootprint> Footprints { get; }
    public SharedArena(IEnumerable<int> owners)
    {
        int[] roster = owners.Order().ToArray();
        if (roster.Length is < 1 or > 4 || roster.Any(id => id <= 0) || roster.Distinct().Count() != roster.Length)
            throw new ArgumentException("Arena needs one to four distinct owners.", nameof(owners));
        int width = roster.Length * SectorWidth;
        int Cell(int column, int row) => row * width + column + 1;
        var cells = new List<HexCellDefinition>();
        for (int row = 0; row < Depth; row++)
            for (int column = 0; column < width; column++)
                cells.Add(new(Cell(column, row), HexCoordinate.FromOffset(column, row),
                    row == 0 ? DeploymentAffinity.Skeletons : row == Depth - 1 ? DeploymentAffinity.Adventurers : DeploymentAffinity.Neutral, []));
        HexCellDefinition[] graph = cells.Select(c => c with
        { Neighbors = cells.Where(n => n.Id != c.Id && c.Coordinate.Distance(n.Coordinate) == 1).Select(n => n.Id).ToArray() }).ToArray();
        int[] Row(int row) => graph.Where(c => c.Coordinate.R == row).Select(c => c.Id).ToArray();
        Board = new(new("shared-watch-arena", 1, 1, graph, [new(1, 0, 0)],
            [new(Faction.Adventurers, Row(8), Row(7)), new(Faction.Skeletons, Row(0), Row(1))], Row(7)));
        var sectors = new List<ArenaSector>();
        var footprints = new List<ArenaFootprint>();
        (int Column, int Row)[] towerCells = [(0, 5), (6, 5), (0, 6), (6, 6), (0, 7), (6, 7), (1, 8), (5, 8), (3, 7)];
        for (int index = 0; index < roster.Length; index++)
        {
            int owner = roster[index], start = index * SectorWidth;
            sectors.Add(new(owner, Enumerable.Range(5, 2).SelectMany(row => Enumerable.Range(2, 3).Select(col => Cell(start + col, row))).ToArray(),
                Enumerable.Range(start, SectorWidth).Select(col => Cell(col, 0)).ToArray()));
            footprints.Add(new(owner, ArenaStructureKind.Keep, -1, Cell(start + 3, 8)));
            footprints.Add(new(owner, ArenaStructureKind.Watchpost, -1, Cell(start + 3, 4)));
            for (int slot = 0; slot < towerCells.Length; slot++)
                footprints.Add(new(owner, ArenaStructureKind.ArrowTower, slot, Cell(start + towerCells[slot].Column, towerCells[slot].Row)));
        }
        _sectors = sectors.ToArray(); _footprints = footprints.ToArray();
        _staticCells = footprints.Select(f => f.Cell).ToHashSet();
        Footprints = Array.AsReadOnly(_footprints);
    }
    public int Keep(int owner) => _footprints.Single(f => f.Owner == owner && f.Kind == ArenaStructureKind.Keep).Cell;
    public int Watchpost(int owner) => _footprints.Single(f => f.Owner == owner && f.Kind == ArenaStructureKind.Watchpost).Cell;
    public int Tower(int owner, int slot) => _footprints.Single(f => f.Owner == owner && f.Slot == slot && f.Slot >= 0).Cell;
    public int Deployment(int owner, int home) => _sectors.Single(s => s.Owner == owner).DeploymentCells[home];
    public int DeploymentHome(int owner, int cell) => Array.IndexOf(_sectors.Single(s => s.Owner == owner).DeploymentCells, cell);
    public int[] EnemyCells(int owner) => _sectors.Single(s => s.Owner == owner).EnemyCells.ToArray();
    public bool CanOccupy(int cell) => Board.Cells.Any(c => c.Id == cell) && !_staticCells.Contains(cell);
    public bool CanDeploy(int owner, int cell, int homes) => homes is >= 2 and <= 6 && _sectors.Any(s => s.Owner == owner && s.DeploymentCells.Take(homes).Contains(cell));
    public ArenaState Snapshot() => new(Board.Definition(), _sectors.Select(s => s with
    { DeploymentCells = s.DeploymentCells.ToArray(), EnemyCells = s.EnemyCells.ToArray() }).ToArray(), _footprints.ToArray());

    // Deterministic breadth-first path to any attack cell. Occupied endpoints
    // are excluded by the caller; structure cells are always impassable.
    internal int[] Route(int source, int target, int range, Func<int, bool> passable)
    {
        if (!CanOccupy(source) || range < 1) throw new ArgumentException("Invalid arena route origin/range.");
        var previous = new Dictionary<int, int> { [source] = 0 };
        var queue = new Queue<int>(); queue.Enqueue(source);
        while (queue.TryDequeue(out int current))
        {
            if (Board.Distance(current, target) <= range)
            {
                var path = new List<int>();
                for (int at = current; at != source; at = previous[at]) path.Add(at);
                path.Reverse(); return path.ToArray();
            }
            foreach (int next in Board.Cell(current).Neighbors)
                if (CanOccupy(next) && passable(next) && previous.TryAdd(next, current)) queue.Enqueue(next);
        }
        return [];
    }
}
