using Game.Core;

namespace Game;

internal enum HudSection { Status, Roster, Economy, Context, Controls }

// Exact displayed inputs, with owned scalar tokens instead of revision/hash
// invalidation. Mutable incoming arrays never back retained comparison state.
internal sealed class HudInvalidation
{
    private readonly Dictionary<HudSection, object?[]> _previous = [];
    internal bool Refresh(HudSection section, object?[] inputs)
    {
        if (_previous.TryGetValue(section, out object?[]? old) && old.SequenceEqual(inputs)) return false;
        _previous[section] = inputs; return true;
    }
    internal static Dictionary<HudSection, object?[]> Capture(MatchSnapshot? state, int focus, int slot, string group,
        bool connected, string status, string feedback, int player, int host, bool canStart, bool canInvite)
    {
        CityState? city = state?.Players.FirstOrDefault(p => p.Id == focus), me = state?.Players.FirstOrDefault(p => p.Id == player);
        var result = new Dictionary<HudSection, object?[]>();
        object?[] Common() => [state?.MatchId, state?.Phase, state?.Paused];
        result[HudSection.Status] = [.. Common(), connected, status, feedback, state?.DefeatReason, state?.TotalWaves];
        var roster = new List<object?> { state?.MatchId, state?.Phase, focus, player, host };
        Add(roster, state?.Players.Select(p => (object)(p.Id, p.Connected, p.Ready, p.Eliminated)));
        result[HudSection.Roster] = roster.ToArray();
        List<object?> economy = [.. Common(), focus, state?.Wave, state?.Turn, state?.TotalWaves, state?.DefeatReason,
            state?.WaveCatalog.FirstOrDefault(w => w.Number == state.Wave)?.IsBoss, city?.Resources, city?.LastReward];
        if (city?.FoodForecast is { } forecast)
        {
            economy.Add((forecast.Demand, forecast.Available, forecast.Paid)); Add(economy, forecast.Participating.Cast<object>()); Add(economy, forecast.Unfed.Cast<object>());
        }
        else economy.Add(null);
        if (city?.LastUpkeep is { } upkeep)
        { economy.Add((upkeep.Wave, upkeep.Paid)); Add(economy, upkeep.Participating.Cast<object>()); Add(economy, upkeep.Unfed.Cast<object>()); }
        else economy.Add(null);
        Add(economy, city?.Soldiers.Select(u => (object)(u.Id, u.Type, u.Level, u.IsBoss, u.Health, u.Profile, u.Participating, u.Deployed)));
        result[HudSection.Economy] = economy.ToArray();
        List<object?> context = [.. Common(), connected, focus, slot, group, player, city?.Resources, city?.Research, me?.Resources, me?.Ready, me?.Eliminated];
        Add(context, city?.Slots.Cast<object>()); Add(context, city?.RecruitmentQuotes.Cast<object>());
        Add(context, state?.TowerCatalog.Cast<object>()); Add(context, state?.MarketRates.Cast<object>());
        Add(context, state?.ResearchQuotes.Cast<object>()); Add(context, state?.PlotPrices.Cast<object>());
        if (state is null) context.Add(null);
        else foreach (BuildingDefinition building in state.BuildingCatalog)
        {
            context.Add((building.Type, building.Construction, building.Upgrade, building.LevelOneOutput, building.LevelTwoOutput, building.MaximumLevel, building.Produces));
            Add(context, building.Recruits?.Cast<object>());
        }
        result[HudSection.Context] = context.ToArray();
        result[HudSection.Controls] = [.. Common(), connected, status, feedback, player, host, canStart, canInvite, me?.Ready, me?.Eliminated];
        return result;
    }
    private static void Add(List<object?> tokens, IEnumerable<object>? values)
    {
        if (values is null) { tokens.Add(null); return; }
        object[] copied = values.ToArray(); tokens.Add(copied.Length); tokens.AddRange(copied);
    }
}
