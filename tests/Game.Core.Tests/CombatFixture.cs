namespace Game.Core.Tests;

// Deliberate fixture access replaces writable public army lists. No engine or wire hook.
internal static class CombatFixture
{
    private static readonly int[] SiegeCells = [16, 17, 18];
    private static readonly int[] MeleeFootprints = [11, 12, 13];
    public static void Soldier(Match match, int id, int city, int health, UnitType type = UnitType.Swordsman)
        => match.Combat.Seed(new CombatUnit(new(id, type, city, city, city, Faction.Adventurers, 0), health, match.Combat.Profile(type),
            new(UnitLifecycle.Alive, new(17, type is UnitType.Swordsman or UnitType.Berserker ? 7 : 1)), new CombatAction.Waiting(), new(0, 0, 0)));
    public static HexUnitState At(UnitState unit, int cell, int footprint) => new(unit.Id, unit.Destination, unit.Faction, UnitLifecycle.Alive,
        new(cell, footprint), ActionSequence: unit.Hex?.ActionSequence ?? 0);
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
                CombatUnit unit = match.Combat.Inspect(enemy.Id);
                long ready = resetRecovery ? match.Tick : unit.ReadyTick;
                match.Combat.Seed(unit with
                {
                    Location = new(UnitLifecycle.Alive, new(SiegeCells[n % 3], MeleeFootprints[n / 3])),
                    Action = ready > match.Tick ? new CombatAction.Recovery(unit.Action.Sequence, unit.Action.AttackSequence, ready)
                        : new CombatAction.Waiting(unit.Action.Sequence, unit.Action.AttackSequence, ready),
                    Decision = unit.Decision with { ObjectiveId = 0, ObjectiveCity = false, Route = [], Visited = [] }
                });
            }
        }
    }
    public static void Steps(Match match, int steps) { for (int i = 0; i < steps; i++) match.Step(); }
}
