namespace Game.Core;

public sealed record ArmySettings
{
    public int[] HomePrices { get; init; } = [5, 8, 12, 18];
}

// Persistent roster location, never a transient combat reservation or a second health owner.
public sealed record ArmyAssignment(int HallSlot, long HallGeneration, int Tile, int Anchor)
{
    public bool Stored => HallSlot >= 0;
    public HexPosition Position => new(Tile, Anchor);
}
public sealed record ArmyHomeState(int Cell, bool Purchased, int Used);
public sealed record TownHallState(int Slot, long Generation, int CapacityLevel, int HealingLevel,
    int Capacity, int HealingPercent, ResourceCost? CapacityQuote, ResourceCost? HealingQuote);
public sealed record ArmyState(int PurchasedHomes, ArmyHomeState[] Homes, int[] HomePrices, TownHallState[] Halls)
{
    public int SquadCapacity { get; init; } = 3;
}

// One frozen quote/placement module serves authority, projections and disabled-action previews.
// Callers supply the owned roster; only living assigned soldiers claim placement.
// Health and lifetime remain in combat.
public sealed class ArmyConfiguration
{
    private readonly int[] _prices;
    private readonly int[] _anchors;
    private readonly int[] _cells;
    private readonly int _squadCapacity;
    public IReadOnlyList<int> HomeCells => Array.AsReadOnly(_cells);
    public ArmyConfiguration(ArmySettings settings, HexBoard board, int squadCapacity = 3)
    {
        if (squadCapacity is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(squadCapacity));
        _squadCapacity = squadCapacity;
        ArgumentNullException.ThrowIfNull(settings);
        if (settings.HomePrices is null || settings.HomePrices.Length != 4
            || settings.HomePrices.Any(p => p <= 0) || settings.HomePrices.Zip(settings.HomePrices.Skip(1)).Any(p => p.First >= p.Second))
            throw new ArgumentException("Four increasing positive home prices required.", nameof(settings));
        _prices = settings.HomePrices.ToArray();
        ArgumentNullException.ThrowIfNull(board);
        // Persistent home indexes are not battlefield hex identities. The
        // shared arena maps the owner's six homes to physical deployment cells.
        _cells = Enumerable.Range(0, 6).ToArray();
        _anchors = [1];
    }
    public ArmySettings Definition() => new() { HomePrices = _prices.ToArray() };
    public int[] Prices() => _prices.ToArray();
    public ResourceCost? HomeQuote(int purchased) => purchased is >= 2 and < 6 ? new ResourceCost(_prices[purchased - 2]) : null;
    public static int Capacity(int level) => level is >= 1 and <= 3 ? level * 6 : throw new ArgumentOutOfRangeException(nameof(level));
    public static int HealingPercent(int level) => level is >= 1 and <= 3 ? level * 5 : throw new ArgumentOutOfRangeException(nameof(level));
    public static ResourceCost? Upgrade(int level, bool healing) => level switch
    { 1 => new(healing ? 6 : 8, 2, Stone: 2), 2 => new(12, 2, Stone: 3), 3 => null, _ => throw new ArgumentOutOfRangeException(nameof(level)) };
    public static int HealedHealth(int health, int maximum, int percent)
    {
        if (health < 1 || maximum < health || percent is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(health));
        return checked((int)Math.Min(maximum, (long)health + ((long)maximum * percent + 99) / 100));
    }
    public ArmyAssignment? Find(IEnumerable<UnitState> soldiers, int purchased, int size, int hallSlot = -1, long generation = 0, int capacity = 0,
        UnitType? type = null, int squadId = 0)
    {
        if (size is < 1 or > 6 || purchased is < 2 or > 6 || squadId < -1) throw new ArgumentOutOfRangeException(nameof(size));
        UnitState[] claims = soldiers.Where(u => u.Health > 0 && u.Assignment is not null)
            .Where(u => hallSlot < 0 ? !u.Assignment!.Stored : u.Assignment!.HallSlot == hallSlot && u.Assignment.HallGeneration == generation).ToArray();
        if (hallSlot >= 0)
        {
            // Town Hall capacity is individual soldiers, independent of squad size.
            for (int tile = 0; tile < capacity; tile++)
                if (!claims.Any(u => u.Assignment!.Tile == tile)) return new(hallSlot, generation, tile, _anchors[0]);
            return null;
        }
        if (squadId >= 0)
            foreach (var group in claims.GroupBy(u => u.Assignment!.Tile).OrderBy(g => Array.IndexOf(_cells, g.Key)))
                if (group.Count() < _squadCapacity && group.All(u => (!type.HasValue || u.Type == type) && !u.IsBoss)
                    && (squadId == 0 || group.All(u => u.SquadId == squadId))) return group.First().Assignment;
        if (squadId > 0) return null;
        foreach (int tile in _cells.Take(purchased))
            if (!claims.Any(u => u.Assignment!.Tile == tile)) return new(-1, 0, tile, _anchors[0]);
        return null;
    }
    public ArmyState Snapshot(IEnumerable<UnitState> soldiers, int purchased, IReadOnlyList<SlotState> slots)
    {
        UnitState[] living = soldiers.Where(u => u.Health > 0).ToArray();
        return new(purchased, _cells.Select((cell, index) => new ArmyHomeState(cell, index < purchased,
            living.Count(u => u.Assignment is { Stored: false } a && a.Tile == cell))).ToArray(), Prices(),
            slots.Select((slot, index) => (slot, index)).Where(s => s.slot.Type == Building.TownHall)
                .Select(s => new TownHallState(s.index, s.slot.Generation, s.slot.CapacityLevel, s.slot.HealingLevel,
                    Capacity(s.slot.CapacityLevel), HealingPercent(s.slot.HealingLevel), Upgrade(s.slot.CapacityLevel, false), Upgrade(s.slot.HealingLevel, true))).ToArray())
        { SquadCapacity = _squadCapacity };
    }
}
