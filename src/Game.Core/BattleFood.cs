namespace Game.Core;

public sealed record BattleFoodForecast(long Demand, int Available, int Paid, int[] Participating, int[] Unfed);
public sealed record BattleUpkeepReceipt(int Wave, int Paid, int[] Participating, int[] Unfed);

public static class BattleFood
{
    public static BattleFoodForecast Forecast(IEnumerable<UnitState> soldiers, int available, EconomyConfiguration economy)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(available);
        UnitState[] living = soldiers.Where(u => u.Health > 0 && u.Faction == Faction.Adventurers)
            .OrderByDescending(u => u.Level).ThenBy(u => u.Id).ToArray();
        var participants = new List<int>(); var unfed = new List<int>();
        long demand = 0; int remaining = available;
        foreach (UnitState unit in living)
        {
            int cost = economy.Upkeep(unit.Type); demand = checked(demand + cost);
            if (remaining >= cost) { remaining -= cost; participants.Add(unit.Id); }
            else unfed.Add(unit.Id);
        }
        return new(demand, available, available - remaining, participants.ToArray(), unfed.ToArray());
    }
}
