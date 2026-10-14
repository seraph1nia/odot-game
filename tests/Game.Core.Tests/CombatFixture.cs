namespace Game.Core.Tests;

// Deliberate fixture access, never a production/engine/wire mutation hook.
// Members share one squad pose; static structures and other squads remain solid.
internal static class CombatFixture
{
    public static void Soldier(Match match, int id, int city, int health, UnitType type = UnitType.Swordsman)
    {
        ArmyAssignment assignment = match.Army.Find(match.Players[city].Soldiers, match.Players[city].PurchasedHomes,
            plots: match.Players[city].ArmyPlots, type: type) ?? throw new InvalidOperationException("Fixture requires an available owned squad home.");
        int cell = new SharedArena(match.Players.Keys).Deployment(city, assignment.Tile);
        match.Combat.Seed(new CombatUnit(new(id, type, city, city, city, Faction.Adventurers, 0), health, match.Combat.Profile(type),
            new(UnitLifecycle.Alive, new(cell, 1)), new CombatAction.Waiting(), new(0, 0, 0))
        { Assignment = assignment });
    }
    // Explicit numerical group setup for focused combat tests. Creating all
    // members before deployment uses the real homogeneous grouping owner;
    // member stats/actions are never reimplemented in a fixture simulator.
    public static int[] Group(CombatSimulation combat, UnitType type, int count, int owner, Faction faction, int cell, int destination = 1, int rank = 0)
    {
        int home = 0;
        if (faction == Faction.Adventurers)
        {
            int[] used = combat.Soldiers(owner).Where(u => u.Assignment is { Stored: false }).Select(u => u.Assignment!.Tile).Distinct().ToArray();
            home = Enumerable.Range(0, 6).First(h => !used.Contains(h));
        }
        int[] members = Enumerable.Range(0, count).Select(_ => combat.Create(type, owner, destination, destination, faction, rank,
            assignment: faction == Faction.Adventurers ? new(-1, 0, home, 1) : null)).ToArray();
        foreach (int id in members) combat.Seed(combat.Inspect(id) with { Location = new(UnitLifecycle.Alive, new(cell, 1)) });
        return members;
    }
    public static HexUnitState At(UnitState unit, int cell, int anchor) => new(unit.Id, unit.Destination, unit.Faction, UnitLifecycle.Alive,
        new(cell, anchor), ActionSequence: unit.Hex?.ActionSequence ?? 0, Size: 1);
    public static void Change(Match match, int id, Func<UnitState, UnitState> update) => match.Combat.Seed(update(match.Combat.Read(id)));
    public static void ClearEnemies(Match match)
    {
        foreach (UnitState enemy in match.Enemies) match.Combat.Remove(enemy.Id);
    }
    public static void AtCityEdge(Match match, bool resetRecovery = false)
    {
        ArenaState state = match.Combat.Arena;
        var occupied = match.Combat.SquadSnapshots().Where(s => s.Hex.Lifecycle == UnitLifecycle.Alive).Select(s => s.Hex.Position.Cell).ToHashSet();
        foreach (var group in match.Enemies.GroupBy(e => e.SquadId))
        {
            UnitState first = group.First();
            occupied.Remove(first.Hex!.Position.Cell);
            int keep = state.Footprints.Single(f => f.Owner == first.Destination && f.Kind == ArenaStructureKind.Keep).Cell;
            int cell = match.Combat.Board.Cells.Where(c => !occupied.Contains(c.Id) && !state.Footprints.Any(f => f.Cell == c.Id))
                .OrderBy(c => match.Combat.Board.Distance(c.Id, keep)).ThenByDescending(c => c.Coordinate.R).ThenBy(c => c.Id).First().Id;
            occupied.Add(cell);
            foreach (UnitState enemy in group)
            {
                CombatUnit unit = match.Combat.Inspect(enemy.Id);
                long ready = resetRecovery ? match.Tick : unit.ReadyTick;
                match.Combat.Seed(unit with
                {
                    Location = new(UnitLifecycle.Alive, new(cell, 1)),
                    Action = ready > match.Tick ? new CombatAction.Recovery(unit.Action.Sequence, unit.Action.AttackSequence, ready)
                        : new CombatAction.Waiting(unit.Action.Sequence, unit.Action.AttackSequence, ready),
                    Decision = unit.Decision with { ObjectiveId = 0, ObjectiveCity = false, Route = [], Visited = [] }
                });
            }
        }
    }
    public static void Steps(Match match, int steps) { for (int i = 0; i < steps; i++) match.Step(); }
}
