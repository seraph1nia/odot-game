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
    public static string UnitLabel(UnitState unit)
    {
        if (unit.IsBoss) return $"BOSS L{unit.Level}";
        string role = unit.Type switch { UnitType.Berserker => "B", UnitType.Crossbowman => "R", UnitType.Mage => "M", _ => "S" };
        return $"{(unit.Faction == Faction.Skeletons ? "E" : "")}{role} L{unit.Level}";
    }
    private static string ArmyDetail(UnitState unit, CityState city, bool preview)
    {
        string status = preview && city.FoodForecast is BattleFoodForecast forecast ? forecast.Participating.Contains(unit.Id) ? "next: fed" : "next: reserve"
            : !unit.Participating ? "reserve" : unit.Deployed ? "participating" : "fed · capacity queue";
        return $"#{unit.Id} {unit.Type}{(unit.IsBoss ? " BOSS" : "")} L{unit.Level} · HP {HealthPoints.Format(unit.Health)}/{HealthPoints.Format(unit.Profile.Health)} · damage {HealthPoints.Format(unit.Profile.Damage)} · size {unit.Size} · {status}";
    }
}
