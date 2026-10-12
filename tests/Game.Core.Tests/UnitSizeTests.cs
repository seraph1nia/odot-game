using System.Text.Json;
using Xunit;

namespace Game.Core.Tests;

// Tier A migration: numerical size budgets are intentionally gone. Ordinary
// population and one-member bosses share the same exclusive actor rules. Other
// progression/research tests preserve HP/equipment multipliers independently.
public sealed class UnitSizeTests
{
    [Theory]
    [InlineData(UnitType.Swordsman)]
    [InlineData(UnitType.Berserker)]
    [InlineData(UnitType.Crossbowman)]
    [InlineData(UnitType.Mage)]
    public void BossAndOrdinaryGroupUseOneCellEachRegardlessOfPopulation(UnitType type)
    {
        var arena = new SharedArena([1]);
        using var combat = new CombatSimulation(new(), seed: 1);
        int[] allies = Enumerable.Range(0, 3).Select(_ => combat.Create(type, 1, 1, 1, assignment: new(-1, 0, 0, 1))).ToArray();
        int boss = combat.Create(type, 0, 1, 1, Faction.Skeletons, isBoss: true, level: 3);
        combat.BeginWave();
        SquadState[] squads = combat.SquadSnapshots(); Assert.Equal(2, squads.Length);
        Assert.Equal(allies, squads.Single(s => !s.IsBoss).Members);
        Assert.Equal(boss, Assert.Single(squads.Single(s => s.IsBoss).Members));
        Assert.All(combat.Reservations.Positions, claim => Assert.Equal(1, claim.Size));
        Assert.Equal(2, combat.Reservations.Positions.Length);
        Assert.Equal(2, combat.Reservations.Positions.Select(p => p.Cell).Distinct().Count());
        Assert.Equal(1, combat.Read(boss).Hex!.Size);
        Assert.True(combat.Read(boss).Profile.Health > combat.Read(allies[0]).Profile.Health);
        Assert.Equal(JsonSerializer.Serialize(combat.Reservations), JsonSerializer.Serialize(CombatReservations.Reconstruct(arena, squads)));
        combat.Transfer(boss, 1);
        Assert.Equal(squads.Single(s => s.IsBoss).Hex.Position, combat.Read(boss).Hex!.Position);
    }
}
