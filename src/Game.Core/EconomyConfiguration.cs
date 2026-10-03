using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Security.Cryptography;

namespace Game.Core;

public sealed record ResearchQuote(int Rank, ResourceCost Cost, int RequiredBlacksmithLevel);

public sealed record RecruitmentQuote(UnitType Type, int Level, ResourceCost Cost, int Upkeep, WeaponProfile Profile);

public sealed record MarketRate(Resource Resource, int Units, int Gold);

// Frozen numerical quotes. Mutable authoring/catalog arrays never escape into
// an authority's live economy; projections receive independent copies.
public sealed class EconomyConfiguration
{
    private readonly FrozenDictionary<Building, BuildingDefinition> _buildings;
    private readonly FrozenDictionary<Resource, MarketRate> _rates;
    private readonly FrozenDictionary<(UnitType Type, int Level), ResourceCost> _recruitment;
    private readonly FrozenDictionary<UnitType, int> _upkeep;
    private readonly ResearchQuote[] _research = [new(1, new(10), 1), new(2, new(15), 2)];
    private readonly int[] _expansionPrices = [25, 40, 60, 90];
    private readonly Dictionary<ResearchRanks, RecruitmentQuote[]> _quotes = [];
    private CombatConfiguration? _quoteConfiguration;
    private readonly BuildingDefinition[] _buildingCatalog;
    private readonly MarketRate[] _marketCatalog;
    private readonly KeyValuePair<(UnitType Type, int Level), ResourceCost>[] _recruitmentOrder;
    public ResourceCost StartingResources { get; }
    public ResourceCost BaseProduction { get; }
    public CombatFingerprint Fingerprint { get; }

    public EconomyConfiguration(Rules rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        StartingResources = new(rules.StartingGold, rules.StartingWood);
        BaseProduction = new(rules.BaseGold);
        BuildingDefinition[] buildings = Catalogs.Buildings(rules);
        if (!StartingResources.IsValid || !BaseProduction.IsValid
            || buildings.Any(b => !b.Construction.IsValid || !b.Upgrade.IsValid || b.LevelOneOutput < 0 || b.LevelTwoOutput < 0 || b.MaximumLevel < 1 || (b.Produces is Resource r && !Enum.IsDefined(r))))
            throw new ArgumentException("Invalid economy quotes or production.", nameof(rules));
        UnitDefinition[] units = Catalogs.Units(rules);
        if (rules.SwordMetalCost < 1 || rules.BerserkerMetalCost < 1 || rules.CrossbowMetalCost < 1 || rules.CrossbowWoodCost < 1 || rules.MageClothCost < 1 || rules.MageGoldCost < 1
            || units.Any(u => !u.Recruitment.IsValid || u.Upkeep < 1)) throw new ArgumentException("Invalid recruitment quotes.", nameof(rules));
        var recruitment = new Dictionary<(UnitType, int), ResourceCost>();
        foreach (UnitDefinition unit in units)
        {
            long previous = 0;
            for (int level = 1; level <= 5; level++)
            {
                ResourceCost price = default;
                foreach (Resource resource in Enum.GetValues<Resource>())
                {
                    int original = unit.Recruitment.Amount(resource);
                    int amount = Progression.ScaleWhole(original, level, multiple: 5);
                    if (original > 0 && amount == 0) throw new ArgumentException("Positive equipment components cannot round to zero.", nameof(rules));
                    price = price.With(resource, amount);
                }
                long total = Enum.GetValues<Resource>().Sum(r => (long)price.Amount(r));
                if (total <= previous) throw new ArgumentException("Recruitment prices must increase with level.", nameof(rules));
                previous = total; recruitment.Add((unit.Type, level), price);
            }
        }
        _recruitment = recruitment.ToFrozenDictionary();
        _upkeep = units.ToFrozenDictionary(u => u.Type, u => u.Upkeep);
        _buildings = buildings.Select(Copy).ToFrozenDictionary(b => b.Type);
        _rates = Enum.GetValues<Resource>().Where(r => r != Resource.Gold)
            .Select(r => new MarketRate(r, 5, r is Resource.Metal or Resource.Cloth ? 2 : 1)).ToFrozenDictionary(r => r.Resource);
        _buildingCatalog = _buildings.Values.OrderBy(b => b.Type).ToArray();
        _marketCatalog = _rates.Values.OrderBy(r => r.Resource).ToArray();
        _recruitmentOrder = _recruitment.OrderBy(p => p.Key.Type).ThenBy(p => p.Key.Level).ToArray();
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(1); Write(writer, StartingResources); Write(writer, BaseProduction);
            foreach (BuildingDefinition b in _buildings.Values.OrderBy(b => b.Type))
            {
                writer.Write((int)b.Type); Write(writer, b.Construction); Write(writer, b.Upgrade);
                writer.Write(b.LevelOneOutput); writer.Write(b.LevelTwoOutput); writer.Write(b.MaximumLevel); writer.Write(b.Produces is Resource r ? (int)r : -1);
                for (int level = 1; level < b.MaximumLevel; level++)
                { _ = TryUpgrade(b.Type, level, out ResourceCost upgrade); Write(writer, upgrade); }
                UnitType[] recruits = b.Recruits ?? []; writer.Write(recruits.Length);
                foreach (UnitType type in recruits) writer.Write((int)type);
            }
            foreach (ResearchQuote quote in _research) { writer.Write(quote.Rank); Write(writer, quote.Cost); writer.Write(quote.RequiredBlacksmithLevel); }
            foreach (int price in _expansionPrices) writer.Write(price);
            foreach (var entry in _recruitment.OrderBy(p => p.Key.Type).ThenBy(p => p.Key.Level))
            { writer.Write((int)entry.Key.Type); writer.Write(entry.Key.Level); Write(writer, entry.Value); }
            foreach (var entry in _upkeep.OrderBy(p => p.Key)) { writer.Write((int)entry.Key); writer.Write(entry.Value); }
            foreach (MarketRate rate in _rates.Values.OrderBy(r => r.Resource))
            { writer.Write((int)rate.Resource); writer.Write(rate.Units); writer.Write(rate.Gold); }
        }
        Fingerprint = RulesIdentity.Hash(stream.ToArray());
    }

    private static BuildingDefinition Copy(BuildingDefinition definition) => definition with { Recruits = definition.Recruits?.ToArray() };
    public ResourceCost Recruitment(UnitType type, int level = 1) => _recruitment[(type, level)];
    public ResearchQuote[] ResearchQuotes() => _research.ToArray();
    public int Upkeep(UnitType type) => _upkeep[type];
    public RecruitmentQuote[] RecruitmentQuotes(CombatConfiguration configuration, ResearchRanks ranks = default)
    {
        if (!ReferenceEquals(_quoteConfiguration, configuration)) { _quotes.Clear(); _quoteConfiguration = configuration; }
        if (!_quotes.TryGetValue(ranks, out RecruitmentQuote[]? quotes))
        {
            quotes = _recruitmentOrder.Select(p => new RecruitmentQuote(p.Key.Type, p.Key.Level, p.Value,
                Upkeep(p.Key.Type), configuration.Unit(p.Key.Type, ranks.For(Catalogs.Class(p.Key.Type)), level: p.Key.Level).Runtime())).ToArray();
            _quotes.Add(ranks, quotes);
        }
        return quotes.ToArray();
    }
    internal void ClearCache() { _quotes.Clear(); _quoteConfiguration = null; }
    internal BuildingDefinition? BuildingRule(Building type) => _buildings.GetValueOrDefault(type);
    internal ResearchQuote ResearchRule(int rank) => _research[rank - 1];
    public BuildingDefinition Building(Building type) => Copy(_buildings[type]);
    public BuildingDefinition[] Buildings() => _buildingCatalog.Select(Copy).ToArray();
    public int[] PlotPrices() => _expansionPrices.ToArray();
    public bool TryPlotPrice(int expansionCount, out ResourceCost quote)
    {
        quote = default;
        if (expansionCount < 0 || expansionCount >= _expansionPrices.Length) return false;
        quote = new(_expansionPrices[expansionCount]); return true;
    }
    public bool TryUpgrade(Building type, int level, out ResourceCost quote)
    {
        quote = default;
        if (!_buildings.TryGetValue(type, out BuildingDefinition? building) || level < 1 || level >= building.MaximumLevel) return false;
        quote = building.Recruits is null ? building.Upgrade : level switch
        { 1 => building.Upgrade, 2 => new(30, 10, Stone: 10), 3 => new(45, 10, Stone: 15), 4 => new(70, 10, Stone: 20), _ => throw new InvalidOperationException("Invalid recruitment level.") };
        return true;
    }
    public ResourceCost Production(IEnumerable<SlotState> slots)
    {
        ResourceCost total = BaseProduction;
        foreach (SlotState slot in slots)
        {
            if (!_buildings.TryGetValue(slot.Type, out BuildingDefinition? building) || building.Produces is not Resource resource) continue;
            ResourceCost output = new ResourceCost().With(resource, building.Output(slot.Level));
            if (!total.TryAdd(output, out ResourceCost next)) throw new OverflowException("Production exceeds whole-resource bounds.");
            total = next;
        }
        return total;
    }
    public MarketRate[] MarketRates() => _marketCatalog.ToArray();
    public bool TryMarketQuote(Resource resource, int bundles, out ResourceCost stock, out ResourceCost proceeds)
    {
        stock = default; proceeds = default;
        if (bundles <= 0 || !_rates.TryGetValue(resource, out MarketRate? rate)) return false;
        if (!new ResourceCost().With(resource, rate.Units).TryMultiply(bundles, out ResourceCost amount)
            || !new ResourceCost(rate.Gold).TryMultiply(bundles, out ResourceCost gold)) return false;
        stock = amount; proceeds = gold; return true;
    }
    private static void Write(BinaryWriter writer, ResourceCost value)
    { foreach (Resource resource in Enum.GetValues<Resource>()) writer.Write(value.Amount(resource)); }
}

// Component identities stay separate: economic prices do not reroll otherwise
// identical combat. The match's published identity includes all frozen parts.
public static class RulesIdentity
{
    public static CombatFingerprint Resolve(Rules rules)
    {
        var combat = new CombatConfiguration(rules);
        return Combine(combat.Fingerprint, new EconomyConfiguration(rules).Fingerprint, new CampaignConfiguration(rules.Campaign, combat).Fingerprint);
    }
    public static CombatFingerprint Combine(CombatFingerprint combat, CombatFingerprint economy, CombatFingerprint campaign)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(1);
            foreach (CombatFingerprint part in new[] { combat, economy, campaign })
            { writer.Write(part.A); writer.Write(part.B); writer.Write(part.C); writer.Write(part.D); }
        }
        return Hash(stream.ToArray());
    }
    internal static CombatFingerprint Hash(byte[] bytes)
    {
        byte[] hash = SHA256.HashData(bytes);
        return new(BinaryPrimitives.ReadUInt64LittleEndian(hash), BinaryPrimitives.ReadUInt64LittleEndian(hash.AsSpan(8)),
            BinaryPrimitives.ReadUInt64LittleEndian(hash.AsSpan(16)), BinaryPrimitives.ReadUInt64LittleEndian(hash.AsSpan(24)));
    }
}
