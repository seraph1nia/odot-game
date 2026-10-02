using Game.Core;

namespace DevRunner;

internal sealed record EconomyAction(string Action, int Slot = -1, Building Building = Building.Empty, UnitType Unit = UnitType.Swordsman,
    UnitClass Research = UnitClass.Melee, Resource Resource = Resource.Wood, int Bundles = 0)
{
    public Command Command(MatchSnapshot state, int player, long sequence = 1)
    {
        CityState city = state.Players.Single(p => p.Id == player);
        return new(sequence, state.MatchId, state.Phase, state.TurnSerial, Action, player, Slot, Building, Unit, Research,
            Slot is >= 0 and < 9 ? city.Slots[Slot].Generation : 0, city.Slots.Count(s => s.Purchased) - 5, Resource, Bundles);
    }
}

// Ordinary-command verification policy shared by the cheap balance cases and
// process drivers. All prices, profiles, outputs and maxima come from snapshots.
internal static class CampaignStrategy
{
    private static readonly int[] ProducerSlots = [0, 1, 3, 4];
    private static readonly Resource[] SaleOrder = [Resource.Cloth, Resource.Metal, Resource.Food, Resource.Stone, Resource.Wood];
    private static readonly (int Slot, Building Type)[] Core = [(0, Building.Farm), (1, Building.MetalMine), (2, Building.Barracks), (3, Building.Lumbermill), (4, Building.Stonecutter)];
    private static readonly (int Slot, Building Type)[] Frontline = [(5, Building.Market), (6, Building.Mine)];
    private static readonly (int Slot, Building Type)[] Mixed = [(8, Building.Market), (5, Building.Weaver), (6, Building.Arcanum), (7, Building.ArcheryRange)];
    private static readonly (int Slot, Building Type)[] Towers = [(8, Building.Market), (5, Building.ArrowTower), (6, Building.CatapultTower), (7, Building.ArrowTower)];
    private static readonly (int Slot, Building Type)[] Research = [(5, Building.Blacksmith), (6, Building.Market), (7, Building.Mine)];

    public static EconomyAction? Next(MatchSnapshot state, int player, string family = "frontline")
    {
        CityState city = state.Players.Single(p => p.Id == player);
        if (city.Eliminated || !city.Connected || city.Ready || state.Phase is not (Phase.Building or Phase.Preparation) || state.Paused) return null;
        bool Affordable(ResourceCost cost) => city.Resources.TryPay(cost, out _);
        EconomyAction? Construction(int slot, Building type)
        {
            if (city.Slots[slot].Type != Building.Empty) return null;
            if (!city.Slots[slot].Purchased)
            {
                int expansions = city.Slots.Count(s => s.Purchased) - 5;
                return expansions < state.PlotPrices.Length && Affordable(new(state.PlotPrices[expansions])) ? new("buy-plot", slot) : null;
            }
            return Affordable(state.BuildingCatalog.Single(b => b.Type == type).Construction) ? new("build", slot, type) : null;
        }
        EconomyAction? Upgrade(int slot, int target)
            => city.Slots[slot].Type != Building.Empty && city.Slots[slot].Level < target && city.Slots[slot].UpgradeQuote is ResourceCost quote && Affordable(quote)
                ? new("upgrade", slot) : null;
        if (state.Wave >= 13 && city.Stone >= 120 && city.Slots[2].Level == 5)
        {
            if (city.Slots[4].Type == Building.Stonecutter) return new("sell", 4);
            if (city.Slots[4].Type == Building.Empty && Affordable(state.BuildingCatalog.Single(b => b.Type == Building.Mine).Construction)) return new("build", 4, Building.Mine);
        }
        foreach ((int slot, Building type) in Core)
            if (Construction(slot, type) is EconomyAction founding) return founding;
        int soldierTier = Math.Min(5, 1 + (state.Wave + 1) / 3);
        if (Upgrade(2, soldierTier) is EconomyAction soldierUpgrade) return soldierUpgrade;
        foreach (int slot in ProducerSlots)
            if (Upgrade(slot, 2) is EconomyAction producerUpgrade) return producerUpgrade;
        (int Slot, Building Type)[] branch = family switch { "mixed" => Mixed, "towers" => Towers, "research" => Research, _ => Frontline };
        foreach ((int slot, Building type) in branch)
            if (Construction(slot, type) is EconomyAction expansion) return expansion;
        foreach ((int slot, Building type) in branch)
        {
            BuildingDefinition definition = state.BuildingCatalog.Single(b => b.Type == type);
            int target = definition.Recruits is null ? definition.MaximumLevel : Math.Min(5, 1 + (state.Wave + 1) / 4);
            if (Upgrade(slot, target) is EconomyAction branchUpgrade) return branchUpgrade;
        }
        if (family == "research" && city.Slots[5].Type == Building.Blacksmith && city.Research.Melee < city.Slots[5].Level)
        {
            ResourceCost cost = state.ResearchQuotes.Single(q => q.Rank == city.Research.Melee + 1).Cost;
            if (Affordable(cost)) return new("research", 5);
        }
        foreach ((UnitType type, int target) in Targets(family, state.Wave))
        {
            if (city.Soldiers.Count(u => u.Type == type) >= target) continue;
            int slot = Array.FindIndex(city.Slots, s => state.BuildingCatalog.SingleOrDefault(b => b.Type == s.Type)?.Recruits?.Contains(type) == true);
            if (slot < 0) continue;
            RecruitmentQuote quote = city.RecruitmentQuotes.Single(q => q.Type == type && q.Level == city.Slots[slot].Level);
            if (Affordable(quote.Cost) && city.Food >= (city.FoodForecast?.Demand ?? 0) + quote.Upkeep) return new("recruit", slot, Unit: type);
        }
        int market = Array.FindIndex(city.Slots, s => s.Type == Building.Market);
        if (market >= 0 && city.Gold < 100)
        {
            foreach (Resource resource in SaleOrder)
            {
                int retain = resource switch { Resource.Food => checked((int)(city.FoodForecast?.Demand ?? 0) + 20), Resource.Metal or Resource.Cloth => 100, Resource.Stone => 40, _ => 50 };
                MarketRate rate = state.MarketRates.Single(r => r.Resource == resource);
                int bundles = (city.Resources.Amount(resource) - retain) / rate.Units;
                if (bundles > 0) return new("trade", market, Resource: resource, Bundles: bundles);
            }
        }
        return null;
    }
    public static EconomyAction? ReinforcementInvestment(MatchSnapshot state, int player, bool receivingCity)
    {
        CityState city = state.Players.Single(p => p.Id == player);
        if (city.Ready || city.Eliminated || state.Phase is not (Phase.Building or Phase.Preparation)) return null;
        bool Affordable(ResourceCost cost) => city.Resources.TryPay(cost, out _);
        if (receivingCity)
        {
            if (city.Slots[3].Type == Building.Empty && Affordable(state.BuildingCatalog.Single(b => b.Type == Building.Lumbermill).Construction)) return new("build", 3, Building.Lumbermill);
            if (state.Wave > 1 && city.Slots[1].Level == 1 && city.Slots[1].UpgradeQuote is ResourceCost barracks && Affordable(barracks)) return new("upgrade", 1);
            if (city.Slots[4].Type == Building.Empty && Affordable(state.BuildingCatalog.Single(b => b.Type == Building.Farm).Construction)) return new("build", 4, Building.Farm);
            if (city.Slots[0].Level == 1 && city.Slots[0].UpgradeQuote is ResourceCost farm && Affordable(farm)) return new("upgrade", 0);
        }
        int target = receivingCity && state.Wave > 1 ? 12 : 6;
        RecruitmentQuote quote = city.RecruitmentQuotes.Single(q => q.Type == UnitType.Swordsman && q.Level == city.Slots[1].Level);
        return city.Soldiers.Length < target && Affordable(quote.Cost) && city.Food >= (city.FoodForecast?.Demand ?? 0) + quote.Upkeep ? new("recruit", 1) : null;
    }
    private static (UnitType Type, int Target)[] Targets(string family, int wave)
    {
        if (wave == 1) return [(UnitType.Swordsman, 6)];
        return family switch
        {
            "mixed" => [(UnitType.Swordsman, 12), (UnitType.Mage, wave < 10 ? 2 : 3), (UnitType.Crossbowman, wave < 10 ? 2 : 3), (UnitType.Berserker, 2)],
            "towers" => [(UnitType.Swordsman, wave < 8 ? 10 : 14)],
            "research" => [(UnitType.Swordsman, 16)],
            _ => [(UnitType.Swordsman, wave < 5 ? 12 : 20)]
        };
    }
}
