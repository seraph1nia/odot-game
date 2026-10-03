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
public sealed record ArmyState(int PurchasedHomes, ArmyHomeState[] Homes, int[] HomePrices, TownHallState[] Halls);

// One frozen quote/placement module serves authority, projections and disabled-action previews.
// All placement inputs are living owned soldiers; health and lifetime remain in combat.
public sealed class ArmyConfiguration
{
    private readonly int[] _prices;
    private readonly int[] _anchors;
    private readonly int[] _cells;
    public IReadOnlyList<int> HomeCells => Array.AsReadOnly(_cells);
    public ArmyConfiguration(ArmySettings settings, HexBoard board)
    {
        ArgumentNullException.ThrowIfNull(settings);
        if (settings.HomePrices is null || settings.HomePrices.Length != 4
            || settings.HomePrices.Any(p => p <= 0) || settings.HomePrices.Zip(settings.HomePrices.Skip(1)).Any(p => p.First >= p.Second))
            throw new ArgumentException("Four increasing positive home prices required.", nameof(settings));
        _prices = settings.HomePrices.ToArray();
        _cells = board.Front(Faction.Adventurers).Take(3).Concat(board.Rear(Faction.Adventurers).Take(3)).ToArray();
        if (_cells.Length != 6 || _cells.Distinct().Count() != 6) throw new ArgumentException("Six allied home cells required.", nameof(board));
        _anchors = board.Anchors.OrderBy(a => a.AnchorForward).ThenBy(a => a.Id).Select(a => a.Id).ToArray();
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
    public ArmyAssignment? Find(IEnumerable<UnitState> soldiers, int purchased, int size, int hallSlot = -1, long generation = 0, int capacity = 0)
    {
        if (size is < 1 or > 6 || purchased is < 2 or > 6) throw new ArgumentOutOfRangeException(nameof(size));
        ArmyAssignment[] claims = soldiers.Where(u => u.Health > 0 && u.Assignment is not null)
            .Where(u => hallSlot < 0 ? !u.Assignment!.Stored : u.Assignment!.HallSlot == hallSlot && u.Assignment.HallGeneration == generation)
            .Select(u => u.Assignment!).ToArray();
        UnitState[] owned = soldiers.Where(u => u.Health > 0 && u.Assignment is not null).ToArray();
        IEnumerable<int> tiles = hallSlot < 0 ? _cells.Take(purchased) : Enumerable.Range(0, capacity / 6);
        foreach (int tile in tiles)
        {
            int used = owned.Where(u => u.Assignment!.Tile == tile && claims.Contains(u.Assignment)).Sum(u => u.Size);
            if (used + size > 6) continue;
            foreach (int anchor in _anchors)
                if (!claims.Any(c => c.Tile == tile && c.Anchor == anchor)) return new(hallSlot, generation, tile, anchor);
        }
        return null;
    }
    public ArmyState Snapshot(IEnumerable<UnitState> soldiers, int purchased, IReadOnlyList<SlotState> slots)
    {
        UnitState[] living = soldiers.Where(u => u.Health > 0).ToArray();
        return new(purchased, _cells.Select((cell, index) => new ArmyHomeState(cell, index < purchased,
            living.Where(u => u.Assignment is { Stored: false } a && a.Tile == cell).Sum(u => u.Size))).ToArray(), Prices(),
            slots.Select((slot, index) => (slot, index)).Where(s => s.slot.Type == Building.TownHall)
                .Select(s => new TownHallState(s.index, s.slot.Generation, s.slot.CapacityLevel, s.slot.HealingLevel,
                    Capacity(s.slot.CapacityLevel), HealingPercent(s.slot.HealingLevel), Upgrade(s.slot.CapacityLevel, false), Upgrade(s.slot.HealingLevel, true))).ToArray());
    }
}
