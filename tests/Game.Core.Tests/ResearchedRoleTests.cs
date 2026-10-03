using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

public sealed class ResearchedRoleTests(ITestOutputHelper output)
{
    [Fact]
    public void GuardianAndFrostHaveObservableBenefitsAtTheSameEquipmentAndFoodCost()
    {
        int Guardian(int reduction)
        {
            using var combat = new CombatSimulation(new());
            int ally = combat.Create(UnitType.Swordsman, 1, 1, 1, capabilities: new(FoundationPercent: 5, ReductionPercent: reduction));
            int enemy = combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons);
            Place(combat, ally, 11, 1); Place(combat, enemy, 8, 1);
            for (int tick = 1; tick <= 240; tick++) combat.Step(tick, []);
            return combat.Soldiers(1).FirstOrDefault(u => u.Id == ally)?.Health ?? 0;
        }
        int Frost(int chill)
        {
            using var combat = new CombatSimulation(new());
            int mage = combat.Create(UnitType.Mage, 1, 1, 1, capabilities: new(FoundationPercent: 5, ChillPercent: chill));
            int sword = combat.Create(UnitType.Swordsman, 1, 1, 1), enemy = combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons, level: 3);
            Place(combat, mage, 11, 1); Place(combat, sword, 11, 3); Place(combat, enemy, 8, 1);
            for (int tick = 1; tick <= 200; tick++) combat.Step(tick, []);
            return combat.Soldiers(1).FirstOrDefault(u => u.Id == mage)?.Health ?? 0;
        }
        int ordinary = Guardian(0), protectedHealth = Guardian(10), magic = Frost(0), chilled = Frost(20);
        output.WriteLine($"Guardian: same Swordsman 10 metal/1 food; 9 research vs 3 foundation; HP {HealthPoints.Format(ordinary)} / {HealthPoints.Format(protectedHealth)} after 240 ticks.");
        output.WriteLine($"Frost: same Mage + Sword, 5 gold/15 cloth/10 metal and 3 food; 9 research vs 3 foundation; Mage HP {HealthPoints.Format(magic)} / {HealthPoints.Format(chilled)} after 200 ticks, before the slowed enemy's next committed impact.");
        Assert.True(protectedHealth > ordinary); Assert.True(chilled > magic);
    }
    [Fact]
    public void ChillExpiryDuringMovementPreservesArrivalAndAllReservationClaims()
    {
        using var combat = new CombatSimulation(new()); int ally = combat.Create(UnitType.Swordsman, 1, 1, 1), enemy = combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons);
        Place(combat, ally, 17, 1); Place(combat, enemy, 5, 1); combat.Cleanup(170);
        CombatUnit actor = combat.Inspect(ally); combat.Seed(actor with { Statuses = StatusPolicy.Apply(new(), new(ally, StatusKind.Chill, enemy, 1, 0, 40), new()) });
        combat.StartActions([]); CombatUnit committed = combat.Inspect(ally); var move = Assert.IsType<CombatAction.Moving>(committed.Action);
        Assert.Equal(42, move.EndTick - move.StartTick); string claims = System.Text.Json.JsonSerializer.Serialize(combat.Reservations, WireJson.Options);
        combat.Advance(180, []); Assert.False(combat.Read(ally).Statuses.Active); Assert.Equal(move, combat.Inspect(ally).Action);
        Assert.Equal(claims, System.Text.Json.JsonSerializer.Serialize(combat.Reservations, WireJson.Options));
    }
    private static void Place(CombatSimulation combat, int id, int cell, int anchor)
    { UnitState unit = combat.Read(id); combat.Seed(unit with { Hex = CombatFixture.At(unit, cell, anchor) }); }

    [Fact]
    public void StatusExpiryActionsOccupancyAndEventsAreIdenticalWithReversedActorInsertion()
    {
        string[] Run(bool reverse)
        {
            using var combat = new CombatSimulation(new(), seed: 17);
            int sword = combat.Create(UnitType.Swordsman, 1, 1, 1), mage = combat.Create(UnitType.Mage, 1, 1, 1);
            int enemy = combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons), ranged = combat.Create(UnitType.Crossbowman, 0, 1, 1, Faction.Skeletons);
            Place(combat, sword, 11, 1); Place(combat, mage, 11, 3); Place(combat, enemy, 8, 1); Place(combat, ranged, 8, 3);
            UnitState[] units = combat.Snapshot().Select(u => u with { Statuses = StatusPolicy.Apply(new(), new(u.Id, u.Faction == Faction.Adventurers ? StatusKind.Chill : StatusKind.Poison, u.Faction == Faction.Adventurers ? enemy : mage, 1, 0, u.Faction == Faction.Adventurers ? 40 : 100), new()) }).ToArray();
            foreach (UnitState unit in units) combat.Remove(unit.Id);
            foreach (UnitState unit in reverse ? units.Reverse() : units) combat.Seed(unit);
            var trace = new List<string>();
            for (int tick = 1; tick <= 240; tick++)
            {
                combat.Step(tick, []);
                trace.Add(System.Text.Json.JsonSerializer.Serialize(new { Units = combat.Snapshot(), Dying = combat.Dying(), combat.Reservations, Events = combat.Events() }, WireJson.Options));
            }
            return trace.ToArray();
        }
        Assert.Equal(Run(false), Run(true));
    }
}
