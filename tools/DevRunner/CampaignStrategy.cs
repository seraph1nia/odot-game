using Game.Core;

namespace DevRunner;

internal sealed record EconomyAction(string Action, int Slot = -1, Building Building = Building.Empty, UnitType Unit = UnitType.Swordsman,
    TechnologyId Technology = TechnologyId.None, Resource Resource = Resource.Wood, int Bundles = 0, ConstructionPayment Payment = ConstructionPayment.Standard)
{
    public Command Command(MatchSnapshot state, int player, long sequence = 1)
    {
        CityState city = state.Players.Single(p => p.Id == player);
        return new(sequence, state.MatchId, state.Phase, state.TurnSerial, Action, player, Slot, Building, Unit,
            Slot is >= 0 and < 9 ? city.Slots[Slot].Generation : 0, city.Slots.Count(s => s.Purchased) - 5, Resource, Bundles, Technology, Payment);
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
    private static readonly (int Slot, Building Type)[] Research = [(5, Building.ResearchTower), (6, Building.Market), (7, Building.Mine)];

    private static EconomyAction? Build(CityState city, MatchSnapshot state, int slot, Building type)
    {
        BuildingDefinition definition = state.BuildingCatalog.Single(b => b.Type == type);
        if (city.Resources.TryPay(definition.Construction, out _)) return new("build", slot, type);
        if (city.Wood == 0 && definition.RecoveryConstruction is ResourceCost recovery && city.Resources.TryPay(recovery, out _))
            return new("build", slot, type, Payment: ConstructionPayment.GoldRecovery);
        return null;
    }
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
                if (city.Slots.Any(s => s.Purchased && s.Type == Building.Empty)) return null;
                int expansions = city.Slots.Count(s => s.Purchased) - 5;
                return expansions < state.PlotPrices.Length && Affordable(new(state.PlotPrices[expansions])) ? new("buy-plot", slot) : null;
            }
            return Build(city, state, slot, type);
        }
        EconomyAction? Upgrade(int slot, int target)
            => city.Slots[slot].Type != Building.Empty && city.Slots[slot].Level < target && city.Slots[slot].UpgradeQuote is ResourceCost quote && Affordable(quote)
                ? new("upgrade", slot) : null;
        if (state.Wave >= 13 && city.Stone >= 24 && city.Slots[2].Level == 5)
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
        if (family == "research")
            foreach (TechnologyId id in new[] { TechnologyId.MeleeFoundation, TechnologyId.Guardian, TechnologyId.GuardianMastery })
                if (city.Technologies.Single(t => t.Id == id).Available) return new("research-tech", Technology: id);
        foreach ((UnitType type, int target) in Targets(family, state.Wave))
        {
            if (city.Soldiers.Count(u => u.Type == type) >= target) continue;
            int slot = Array.FindIndex(city.Slots, s => state.BuildingCatalog.SingleOrDefault(b => b.Type == s.Type)?.Recruits?.Contains(type) == true);
            if (slot < 0) continue;
            RecruitmentQuote quote = city.RecruitmentQuotes.Single(q => q.Type == type && q.Level == city.Slots[slot].Level);
            if (Affordable(quote.Cost) && city.Food >= (city.FoodForecast?.Demand ?? 0) + quote.Upkeep) return new("recruit", slot, Unit: type);
        }
        int market = Array.FindIndex(city.Slots, s => s.Type == Building.Market);
        if (market >= 0)
        {
            foreach (Resource resource in SaleOrder)
            {
                // Gold is less restrictive now; retain equipment reserves and sell only surplus food when funded.
                if (city.Gold >= 20 && resource != Resource.Food) continue;
                int retain = resource switch { Resource.Food => checked((int)(city.FoodForecast?.Demand ?? 0) + 20), Resource.Metal or Resource.Cloth => 20, Resource.Stone => 8, _ => 10 };
                MarketRate rate = state.MarketRates.Single(r => r.Resource == resource);
                int bundles = (city.Resources.Amount(resource) - retain) / rate.Units;
                if (bundles > 0) return new("trade", market, Resource: resource, Bundles: bundles);
            }
        }
        return null;
    }
    public static EconomyAction? ResearchWitness(MatchSnapshot state, int player)
    {
        CityState city = state.Players.Single(c => c.Id == player);
        if (city.Ready || city.Eliminated || state.Paused || state.Phase is not (Phase.Building or Phase.Preparation)) return null;
        bool Affordable(ResourceCost cost) => city.Resources.TryPay(cost, out _);
        foreach (Building type in new[] { Building.Farm, Building.MetalMine, Building.Barracks, Building.Lumbermill, Building.Stonecutter, Building.ResearchTower, Building.Weaver, Building.Arcanum })
        {
            if (city.Slots.Any(s => s.Type == type) || type == Building.Stonecutter && city.Slots.Any(s => s.Type == Building.ResearchTower)
                && (city.Slots.Any(s => s.Type == Building.Arcanum) || city.Stone >= 9)) continue;
            if (type == Building.Arcanum && city.Stone >= 9) { int stone = Array.FindIndex(city.Slots, s => s.Type == Building.Stonecutter); if (stone >= 0) return new("sell", stone); }
            int plot = Array.FindIndex(city.Slots, s => s.Type == Building.Empty);
            if (plot < 0) continue;
            SlotState slot = city.Slots[plot];
            if (!slot.Purchased) { int count = city.Slots.Count(s => s.Purchased) - 5; if (count < state.PlotPrices.Length && Affordable(new(state.PlotPrices[count]))) return new("buy-plot", plot); }
            else if (Build(city, state, plot, type) is EconomyAction construction) return construction;
        }
        foreach (Building type in new[] { Building.Barracks, Building.Farm, Building.MetalMine, Building.Lumbermill, Building.ResearchTower, Building.Stonecutter })
        {
            int slot = Array.FindIndex(city.Slots, s => s.Type == type); if (slot < 0) continue;
            int target = type == Building.Barracks ? Math.Min(3, 1 + (state.Wave + 1) / 3) : 2;
            if (city.Slots[slot].Level < target && city.Slots[slot].UpgradeQuote is { } quote && Affordable(quote)) return new("upgrade", slot);
        }
        foreach (UnitType role in new[] { UnitType.Swordsman, UnitType.Mage })
        {
            int target = role == UnitType.Mage ? 2 : state.Wave == 1 ? 6 : 12;
            if (city.Soldiers.Count(u => u.Type == role) >= target) continue;
            int slot = Array.FindIndex(city.Slots, s => state.BuildingCatalog.FirstOrDefault(b => b.Type == s.Type)?.Recruits?.Contains(role) == true);
            if (slot < 0) continue;
            RecruitmentQuote quote = city.RecruitmentQuotes.Single(q => q.Type == role && q.Level == city.Slots[slot].Level);
            if (Affordable(quote.Cost) && city.Food >= (city.FoodForecast?.Demand ?? 0) + quote.Upkeep) return new("recruit", slot, Unit: role);
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
            if (city.Slots[3].Type == Building.Empty && Build(city, state, 3, Building.Lumbermill) is EconomyAction lumbermill) return lumbermill;
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
