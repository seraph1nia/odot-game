namespace Game.Core;

public sealed record BattleFoodForecast(long Demand, int Available, int Paid, int[] Participating, int[] Unfed);
public sealed record BattleUpkeepReceipt(int Wave, int Paid, int[] Participating, int[] Unfed);

public static class BattleFood
{
    public static BattleFoodForecast Forecast(IEnumerable<UnitState> soldiers, int available, EconomyConfiguration economy)
        => Forecast(soldiers.Select(u => (u.Id, u.Health, u.Faction, u.Level, u.Type)), available, economy.Upkeep);
    public static BattleFoodForecast Forecast(IEnumerable<UnitState> soldiers, int available, IReadOnlyList<RecruitmentQuote> quotes)
        => Forecast(soldiers.Select(u => (u.Id, u.Health, u.Faction, u.Level, u.Type)), available, type => quotes.First(q => q.Type == type).Upkeep);
    internal static BattleFoodForecast Forecast(IEnumerable<CombatUnit> soldiers, int available, EconomyConfiguration economy)
        => Forecast(soldiers.Select(u => (u.Id, u.Health, u.Faction, Level: u.Identity.Level, u.Type)), available, economy.Upkeep);
    private static BattleFoodForecast Forecast(IEnumerable<(int Id, int Health, Faction Faction, int Level, UnitType Type)> soldiers, int available, Func<UnitType, int> upkeep)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(available);
        var living = soldiers.Where(u => u.Health > 0 && u.Faction == Faction.Adventurers)
            .OrderByDescending(u => u.Level).ThenBy(u => u.Id).ToArray();
        var participants = new List<int>(); var unfed = new List<int>();
        long demand = 0; int remaining = available;
        foreach (var unit in living)
        {
            int cost = upkeep(unit.Type); demand = checked(demand + cost);
            if (remaining >= cost) { remaining -= cost; participants.Add(unit.Id); }
            else unfed.Add(unit.Id);
        }
        return new(demand, available, available - remaining, participants.ToArray(), unfed.ToArray());
    }
}
