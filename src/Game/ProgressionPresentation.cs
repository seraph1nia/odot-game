using Game.Core;

namespace Game;

public sealed record ProgressionView(string Phase, string Land, string Food, string Reward, string Army);

// Pure current-state projection: receipts remain actual results, even when a
// snapshot also carries a forecast for a future battle.
public static class ProgressionPresentation
{
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
        string reward = city.LastReward is WaveClearReceipt clear ? $"Last clear W{clear.Wave}: +{clear.Amount.Gold} gold, +{clear.Amount.Wood} wood, +{clear.Amount.Food} food" : "No clear reward yet";
        string army = string.Join('\n', city.Soldiers.Select(unit => ArmyDetail(unit, city, preview)));
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
    public static string UnitStats(UnitState unit) => $"Health {HealthPoints.Format(unit.Health)}/{HealthPoints.Format(unit.Profile.Health)}\nLevel {unit.Level} · {unit.Faction}\nDamage per attack {HealthPoints.Format(unit.Profile.Damage)}\nSize {unit.Size}";
    public static string UnitName(UnitState unit) => (unit.IsBoss ? "Boss · " : "") + (unit.Faction == Faction.Skeletons ? "Skeleton " : "") + unit.Type;
    public static string UnitDescription(UnitState unit) => unit.Type switch
    {
        UnitType.Berserker => "A heavy melee fighter with powerful axe attacks.",
        UnitType.Crossbowman => "A ranged fighter who attacks with a crossbow.",
        UnitType.Mage => "A spellcaster who attacks with magic.",
        _ => "A close combat fighter armed with a sword."
    };
    private static string ArmyDetail(UnitState unit, CityState city, bool preview)
    {
        string status = preview && city.FoodForecast is BattleFoodForecast forecast ? forecast.Participating.Contains(unit.Id) ? "next: fed" : "next: reserve"
            : !unit.Participating ? "reserve" : unit.Deployed ? "participating" : "fed · capacity queue";
        return $"#{unit.Id} {unit.Type}{(unit.IsBoss ? " BOSS" : "")} L{unit.Level} · HP {HealthPoints.Format(unit.Health)}/{HealthPoints.Format(unit.Profile.Health)} · damage {HealthPoints.Format(unit.Profile.Damage)} · size {unit.Size} · {status}";
    }
}
