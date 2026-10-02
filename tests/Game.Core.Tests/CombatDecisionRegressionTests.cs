using Xunit;

namespace Game.Core.Tests;

public sealed class CombatDecisionRegressionTests
{
    internal static int At(CombatSimulation combat, Faction faction, int cell, UnitType type = UnitType.Swordsman)
    {
        int id = combat.Create(type, faction == Faction.Adventurers ? 1 : 0, 1, 1, faction);
        UnitState unit = combat.Read(id);
        combat.Seed(unit with { Hex = CombatFixture.At(unit, cell, 1) });
        return id;
    }

    [Fact]
    public void ActionBoundaryRanksNewCloserOpponent()
    {
        using var combat = new CombatSimulation(new(), seed: 123);
        int actor = At(combat, Faction.Adventurers, 17);
        int far = At(combat, Faction.Skeletons, 2);
        int near = At(combat, Faction.Skeletons, 8);
        UnitState unit = combat.Read(actor);
        combat.Seed(unit with { Decision = unit.Decision! with { ObjectiveId = far, ObjectiveCell = 2 } });
        combat.StartActions([]);
        Assert.Equal(near, combat.Read(actor).Decision!.ObjectiveId);
    }

    [Fact]
    public void ConfiguredDefenderSplashIsApplied()
    {
        Rules rules = new() { Combat = new() { Defender = new(1, 1, VictimCap: 2, SplashHexRadius: 1) } };
        using var match = new Match(rules, combatSeed: 123);
        City city = match.Join()!;
        int first = At(match.Combat, Faction.Skeletons, 8);
        int second = At(match.Combat, Faction.Skeletons, 9);
        city.Defender = city.Defender with { TargetId = first, PendingImpact = true, ImpactTick = 1 };
        int health = match.Combat.Read(second).Health;
        match.Combat.Advance(1, [city]);
        Assert.Equal(health - HealthPoints.FromWhole(rules.DefenderDamage), match.Combat.Read(second).Health);
        Assert.Equal(new[] { first, second }, Assert.Single(match.Combat.Events(), e => e.Type == CombatEventType.Impact).Victims);
    }
}
