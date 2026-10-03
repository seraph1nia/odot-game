namespace Game.Core;

// Immutable values for one mutation stage, never a second unit authority.
// Grouping preserves the canonical input order. No buffer is publicly exposed.
internal sealed class CombatStage(CombatUnit[] units)
{
    private readonly Dictionary<int, CombatUnit[]> _cities = units.GroupBy(u => u.Destination).ToDictionary(g => g.Key, g => g.ToArray());
    private readonly Dictionary<(int City, Faction Faction), CombatUnit[]> _factions = units.GroupBy(u => (u.Destination, u.Faction)).ToDictionary(g => g.Key, g => g.ToArray());
    private Dictionary<int, CombatUnit>? _byId;
    internal Dictionary<int, CombatUnit> ById => _byId ??= units.ToDictionary(u => u.Id);
    internal CombatUnit[] City(int city) => _cities.GetValueOrDefault(city) ?? [];
    internal CombatUnit[] Faction(int city, Faction faction) => _factions.GetValueOrDefault((city, faction)) ?? [];
    internal CombatUnit[] Opponents(CombatUnit actor) => Faction(actor.Destination, actor.Faction == Game.Core.Faction.Adventurers ? Game.Core.Faction.Skeletons : Game.Core.Faction.Adventurers);
}
