using Game.Core;

namespace Game;

public sealed record EconomyView(string IncomeLabel, string Context, string UpkeepLabel, string UpkeepValue, string BalanceLabel, string BalanceValue);

public sealed record ProgressionView(string Phase, string Land, string Food, string Reward, string Army);

// Pure current-state projection: receipts remain actual results, even when a
// snapshot also carries a forecast for a future battle.
public static class ProgressionPresentation
{
    public static string Income(ResourceCost? income, Resource resource)
        => income is ResourceCost value ? value.Amount(resource) > 0 ? $"+{value.Amount(resource)}" : "0" : "Unavailable";
    public static EconomyView Economy(MatchSnapshot? state, CityState? city, bool connected)
    {
        string label = state?.Phase == Phase.Building ? "Income/turn" : "Next building turn";
        bool inactive = city?.Eliminated == true || state?.Phase is Phase.Victory or Phase.Defeat;
        string context = !connected ? "Stale · waiting for connection" : state?.Paused == true ? "Paused · synchronized values" : inactive ? "Inactive · no future income"
            : state?.Phase is Phase.Preparation or Phase.Combat ? "Ready for battle grants no income" : "";
        if (state?.Phase is Phase.Building or Phase.Preparation && city?.FoodForecast is { } forecast)
            return new(label, context, "Next battle", $"{forecast.Demand} food",
                forecast.Unfed.Length > 0 ? $"{forecast.Unfed.Length} soldiers will sit out" : "Food after payment",
                forecast.Unfed.Length > 0 ? "" : (forecast.Available - forecast.Paid).ToString(System.Globalization.CultureInfo.InvariantCulture));
        if (city?.LastUpkeep is { } receipt)
            return new(label, context, state?.Phase == Phase.Combat ? $"Paid this battle · W{receipt.Wave}" : $"Last battle · W{receipt.Wave}",
                $"{receipt.Paid} food", "Sat out", receipt.Unfed.Length.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return new(label, context, "Next battle", "0 food", "Food after payment", (city?.Food ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
    public static string CostExplanation(ResourceCost cost, ResourceCost stocks)
        => string.Join('\n', Enum.GetValues<Resource>().Where(r => cost.Amount(r) > 0).Select(r => stocks.Amount(r) >= cost.Amount(r)
            ? $"{r}: {stocks.Amount(r)}/{cost.Amount(r)} · affordable"
            : $"Need {cost.Amount(r) - stocks.Amount(r)} more {r.ToString().ToLowerInvariant()} · " + (r switch
            { Resource.Gold => "production, Gold Mine or Market", Resource.Wood => "Lumbermill", Resource.Food => "Farm", Resource.Stone => "Stonecutter", Resource.Metal => "Metal Mine", _ => "Weaver" })));
    public static string ProducerBenefit(BuildingDefinition definition, int level = 1, bool upgrade = false)
        => definition.Produces is Resource resource ? upgrade ? $"{definition.Output(level)} → {definition.Output(level + 1)} {resource.ToString().ToLowerInvariant()}/turn"
            : $"+{definition.Output(level)} {resource.ToString().ToLowerInvariant()}/turn" : "";
    public static string FoodSalePreview(CityState city, MarketRate rate)
    {
        if (city.Food < rate.Units) return $"Need {rate.Units - city.Food} more food · Farm";
        BattleFoodForecast forecast = BattleFood.Forecast(city.Soldiers, city.Food - rate.Units, city.RecruitmentQuotes);
        return $"After sale: pay {forecast.Paid} food · {forecast.Unfed.Length} soldiers will sit out · food after payment {forecast.Available - forecast.Paid}";
    }
    public static TechnologyEligibility ResearchAccess(MatchSnapshot? state, CityState? city, int player, bool connected, TechnologyId technology)
    {
        string? blocked = !connected ? "Waiting for connection" : city is null ? "Waiting for city" : city.Id != player ? "Observing another city"
            : city.Eliminated ? "City fallen" : state?.Paused == true ? "Paused" : city.Ready ? "Unready to purchase"
            : state?.Phase is not (Phase.Building or Phase.Preparation) ? "Available during building/preparation" : null;
        return blocked is not null ? new(technology, false, blocked) : city!.Technologies.FirstOrDefault(t => t.Id == technology) ?? new(technology, false, "Unavailable");
    }
    public static ProgressionView Describe(MatchSnapshot? state, CityState? city)
    {
        string phase = state is null ? "Waiting for server" : state.Phase switch
        {
            Phase.Victory => $"VICTORY • {state.TotalWaves} waves held",
            Phase.Defeat => PresentationLimits.DefeatText(state.DefeatReason),
            _ => $"{(state.Paused ? "PAUSED • " : "")}{state.Phase}\nWave {state.Wave}/{state.TotalWaves}{(state.WaveCatalog.FirstOrDefault(w => w.Number == state.Wave)?.IsBoss == true ? " · BOSS" : "")} • Turn {state.Turn}/3"
        };
        if (city is null || state is null) return new(phase, "", "No battle upkeep yet", "No clear reward yet", "");
        int plots = city.Slots.Count(p => p.Purchased), count = plots - 5;
        string land = $"Land {plots}/9" + (count >= 0 && count < state.PlotPrices.Length ? $" · next {state.PlotPrices[count]} gold" : " · fully expanded");
        bool preview = state.Phase is Phase.Building or Phase.Preparation;
        string food = preview && city.FoodForecast is BattleFoodForecast forecast ? $"Next: {forecast.Demand} food · pay {forecast.Paid} · {forecast.Participating.Length} fed/{forecast.Unfed.Length} reserve"
            : city.LastUpkeep is BattleUpkeepReceipt paid ? $"Wave {paid.Wave}: paid {paid.Paid} food · {paid.Participating.Length} participated" : "No battle upkeep yet";
        string reward = city.LastReward is WaveClearReceipt clear ? $"Last clear W{clear.Wave}: +{clear.Amount.Gold} gold, +{clear.Amount.Wood} wood, +{clear.Amount.Food} food, +{clear.Research} research" : "No clear reward yet";
        string army = string.Join('\n', city.Soldiers.Select(unit => ArmyDetail(unit, city, preview, state.Tick)));
        return new(phase, land, food, reward, army);
    }
    public static string UnitLabel(UnitState unit) => RomanLevel(unit.Level);
    public static string RomanLevel(int level)
    {
        if (level < 1) return "";
        var result = new System.Text.StringBuilder();
        foreach (var (value, numeral) in new[] { (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"), (100, "C"), (90, "XC"), (50, "L"), (40, "XL"), (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I") })
            while (level >= value) { result.Append(numeral); level -= value; }
        return result.ToString();
    }
    public static string[] PhaseRows(MatchSnapshot? state)
    {
        string[] labels = ["Building 1", "Building 2", "Building 3", "Preparation", "Combat"];
        int active = state?.Phase switch { Phase.Building => state.Turn - 1, Phase.Preparation => 3, Phase.Combat => 4, _ => -1 };
        return labels.Select((label, index) => (index == active ? "> " : "  ") + label).ToArray();
    }
    public static int HealthPercent(int current, int maximum) => (int)Math.Round(PresentationLimits.HealthFraction(current, maximum) * 100, MidpointRounding.AwayFromZero);
    public static string UnitStats(UnitState unit, double tick = 0) => $"Health {HealthPoints.Format(unit.Health)}/{HealthPoints.Format(unit.Profile.Health)}\nLevel {unit.Level} · {unit.Faction}\nDamage per attack {HealthPoints.Format(unit.Profile.Damage)}\nSize {unit.Size}\n{CapabilityText(unit.Capabilities)}\n{StatusText(unit.Statuses, tick)}\nCommitted: impact {unit.ImpactTick}, ready {unit.ReadyTick}";
    public static string CapabilityText(UnitCapabilities c)
    {
        var parts = new List<string>();
        if (c.FoundationPercent > 0) parts.Add($"Health/direct damage +{c.FoundationPercent}%");
        if (c.DirectDamagePercent > 0) parts.Add($"Direct damage +{c.DirectDamagePercent}%");
        if (c.ReductionPercent > 0) parts.Add($"Incoming damage -{c.ReductionPercent}%");
        if (c.BurnPercent > 0) parts.Add($"Burn: {c.BurnPercent}% captured damage per tick");
        if (c.PoisonPercent > 0) parts.Add($"Poison: {c.PoisonPercent}% per stack tick (max 3)");
        if (c.ChillPercent > 0) parts.Add($"Chill: future actions +{c.ChillPercent}% duration");
        return string.Join(" · ", parts);
    }
    public static string StatusText(StatusState state, double tick)
    {
        var parts = new List<string>();
        if (state.Burn is { } b && tick <= b.ExpiresTick) parts.Add($"Burn {HealthPoints.Format(b.Strength)}/tick · {Math.Max(0, b.ExpiresTick - tick) / 60:0.0}s");
        StatusEffect[] poison = state.Poison.Where(p => tick <= p.ExpiresTick).ToArray();
        if (poison.Length > 0) parts.Add($"Poison ×{poison.Length}: " + string.Join(", ", poison.Select(p => $"{HealthPoints.Format(p.Strength)}/tick · {Math.Max(0, p.ExpiresTick - tick) / 60:0.0}s")));
        if (state.Chill is { } c && tick < c.ExpiresTick) parts.Add($"Chill +{c.Strength}% future duration · {Math.Max(0, c.ExpiresTick - tick) / 60:0.0}s");
        return string.Join(" · ", parts);
    }
    public static string StatusBadge(StatusState state, double tick)
        => string.Join("\n", new[] { state.Burn is { } b && tick < b.ExpiresTick ? "▲ Burn" : "", state.Poison.Any(p => tick < p.ExpiresTick) ? $"● Poison ×{state.Poison.Count(p => tick < p.ExpiresTick)}" : "", state.Chill is { } c && tick < c.ExpiresTick ? "◆ Chill" : "" }.Where(s => s.Length > 0));
    public static string UnitName(UnitState unit) => (unit.IsBoss ? "Boss · " : "") + (unit.Faction == Faction.Skeletons ? "Skeleton " : "") + unit.Type;
    public static string UnitDescription(UnitState unit) => unit.Type switch
    {
        UnitType.Berserker => "A heavy melee fighter with powerful axe attacks.",
        UnitType.Crossbowman => "A ranged fighter who attacks with a crossbow.",
        UnitType.Mage => "A spellcaster who attacks with magic.",
        _ => "A close combat fighter armed with a sword."
    };
    private static string ArmyDetail(UnitState unit, CityState city, bool preview, long tick)
    {
        string status = preview && city.FoodForecast is BattleFoodForecast forecast ? forecast.Participating.Contains(unit.Id) ? "next: fed" : "next: reserve"
            : !unit.Participating ? "reserve" : unit.Deployed ? "participating" : "fed · capacity queue";
        string committed = unit.Statuses.Chill is null ? "" : $" · committed impact {unit.ImpactTick}, ready {unit.ReadyTick}";
        return $"#{unit.Id} {unit.Type}{(unit.IsBoss ? " BOSS" : "")} L{unit.Level} · HP {HealthPoints.Format(unit.Health)}/{HealthPoints.Format(unit.Profile.Health)} · damage {HealthPoints.Format(unit.Profile.Damage)} · size {unit.Size} · {status} · {CapabilityText(unit.Capabilities)} · {StatusText(unit.Statuses, tick)}{committed}";
    }
}
