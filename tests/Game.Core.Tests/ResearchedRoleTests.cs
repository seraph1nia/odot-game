using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

public sealed class ResearchedRoleTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(UnitType.Swordsman, TechnologyId.MeleeFoundation, TechnologyId.Guardian)]
    [InlineData(UnitType.Mage, TechnologyId.MagicFoundation, TechnologyId.Frost)]
    public void ResearchedGroupBenefitChangesReceivedInjuryWithoutChangingEquipmentOrFood(UnitType role, TechnologyId foundation, TechnologyId specialization)
    {
        // Tier A numerical encounter: three members per exclusive source, actual
        // profiles/status/mean-chill/actions. Not a paid research or campaign claim;
        // ResearchWitness/CityResearch/PaidCampaign own earned prices/prerequisites.
        // Old stacked Mage+Sword/member-anchor fixtures are retired together.
        (int Health, int Attacks, string Claims) Fight(bool specialized)
        {
            using var combat = new CombatSimulation(new(), seed: 1);
            CombatFixture.Group(combat, role, 3, 1, Faction.Adventurers, 23);
            int[] enemies = CombatFixture.Group(combat, UnitType.Swordsman, 3, 0, Faction.Skeletons, 24);
            var catalog = new ResearchCatalog();
            ResearchState research = new(Owned: (1UL << (int)foundation) | (specialized ? 1UL << (int)specialization : 0));
            combat.Research(1, research, catalog);
            int attacks = 0; long cursor = 0;
            for (int tick = 1; tick <= 73; tick++)
            {
                combat.Step(tick, []);
                attacks += combat.Events().Count(e => e.Sequence > cursor && e.Type == CombatEventType.Impact && e.Landed && e.Unit?.Faction == Faction.Skeletons);
                cursor = combat.EventSequence;
                CombatReservations claims = CombatReservations.Reconstruct(combat.Arena, combat.SquadSnapshots());
                Assert.Equal(combat.Reservations.Positions, claims.Positions); Assert.Equal(combat.Reservations.Transit, claims.Transit);
            }
            if (specialized && role == UnitType.Mage)
                Assert.All(enemies.Select(combat.Read), u => Assert.NotNull(u.Statuses.Chill));
            return (combat.Soldiers(1).Sum(u => u.Health), attacks, JsonSerializer.Serialize(combat.Reservations, WireJson.Options));
        }
        var ordinary = Fight(false); var researched = Fight(true);
        Assert.True(researched.Health > ordinary.Health);
        if (role == UnitType.Mage) Assert.True(researched.Attacks < ordinary.Attacks);
        else Assert.Equal(ordinary.Attacks, researched.Attacks);
        var economy = new EconomyConfiguration(new());
        output.WriteLine($"Explicit numerical {role} group: same equipment={economy.Recruitment(role)}, food={economy.Upkeep(role)} per member; 73 ticks; HP={ordinary.Health}/{researched.Health}, enemy impacts={ordinary.Attacks}/{researched.Attacks}. Earned prices are separate proof.");
    }
}
