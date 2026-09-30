namespace Game.Core.Tests;

// Deliberate fixture access replaces writable public army lists. No engine or wire hook.
internal static class CombatFixture
{
    public static void Soldier(Match match, int id, int city, int health, double forward, double lateral = 0, UnitType type = UnitType.Swordsman)
        => match.Combat.Seed(new(id, health, forward, 0, city, city) { Type = type, Owner = city, Lateral = lateral });
    public static void Change(Match match, int id, Func<UnitState, UnitState> update) => match.Combat.Seed(update(match.Combat.Read(id)));
    public static void ClearEnemies(Match match)
    {
        foreach (UnitState enemy in match.Enemies) match.Combat.Remove(enemy.Id);
    }
    public static void AtCityEdge(Match match, bool resetRecovery = false)
    {
        foreach (var group in match.Enemies.GroupBy(e => e.Destination))
        {
            int index = 0;
            foreach (UnitState enemy in group)
            {
                int n = index++;
                match.Combat.Seed(enemy with
                {
                    Position = CombatSimulation.Radius + n / 7 * 0.48,
                    Lateral = (n % 7 - 3) * 0.48,
                    Deployed = true,
                    Cooldown = resetRecovery ? 0 : enemy.Cooldown,
                    ReadyTick = resetRecovery ? match.Tick : enemy.ReadyTick,
                    PendingImpact = false,
                    TargetId = 0,
                    TargetCity = false
                });
            }
        }
    }
    public static void Steps(Match match, int steps) { for (int i = 0; i < steps; i++) match.Step(); }
}
