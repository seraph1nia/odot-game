using System.Text.Json;
using Xunit;

namespace Game.Core.Tests;

public sealed class CombatConfigurationTests
{
    [Theory]
    [InlineData(UnitType.Swordsman, 10, 30, 1, 12, 48)]
    [InlineData(UnitType.Berserker, 20, 27, 1, 18, 54)]
    [InlineData(UnitType.Crossbowman, 30, 30, 3, 18, 42)]
    [InlineData(UnitType.Mage, 40, 30, 3, 24, 66)]
    public void ProfilesOwnNumericalTimingAndResearchChangesBothDirectChannelsNotStructures(UnitType type, int initiative, int move, int range, int windup, int recovery)
    {
        var config = new CombatConfiguration(new()); HexCombatProfile p = config.Unit(type);
        Assert.Equal((initiative, move, range, windup, recovery, 48), (p.Initiative, p.MoveTicks, p.HexRange, p.WindupTicks, p.RecoveryTicks, p.DeathTicks));
        Assert.Equal(p with { Health = HealthPoints.Ranked(p.Health, 2), Damage = HealthPoints.Ranked(p.Damage, 2), SecondaryDamage = HealthPoints.Ranked(p.SecondaryDamage, 2) }, config.Unit(type, 2));
        using var combat = new CombatSimulation(new());
        int a = combat.Create(type, 1, 1, 1, rank: 2), b = combat.Create(type, 0, 1, 1, Faction.Skeletons, 2);
        Assert.Equal(combat.Read(a).Profile, combat.Read(b).Profile);
        Assert.Equal(1, combat.Read(a).Hex!.Size); Assert.Equal(1, combat.Read(b).Hex!.Size);
        Assert.Throws<ArgumentOutOfRangeException>(() => config.Unit(type, 3));
    }
    [Fact]
    public void MageHasWithinGroupSecondaryDamageAndSingleTargetCadenceTradeoff()
    {
        var c = new CombatConfiguration(new()); HexCombatProfile mage = c.Unit(UnitType.Mage), crossbow = c.Unit(UnitType.Crossbowman);
        Assert.Equal(1200, mage.Damage); Assert.Equal(2500, mage.Health);
        Assert.True((long)mage.Damage * crossbow.CadenceTicks < (long)crossbow.Damage * mage.CadenceTicks);
        Assert.True((long)mage.Damage * 2 * crossbow.CadenceTicks > (long)crossbow.Damage * mage.CadenceTicks);
        Assert.Equal(400, mage.SecondaryDamage); Assert.Equal(0, crossbow.SecondaryDamage);
        Assert.Equal(mage.StructureDamage, c.Unit(UnitType.Mage, rank: 2).StructureDamage);
        Assert.Equal(new ResourceCost(1, Cloth: 3), Catalogs.Units(new()).Single(u => u.Type == UnitType.Mage).Recruitment);
    }
    [Theory]
    [InlineData("move")]
    [InlineData("death")]
    [InlineData("initiative")]
    [InlineData("windup")]
    [InlineData("recovery")]
    [InlineData("range")]
    [InlineData("defender")]
    [InlineData("limit")]
    [InlineData("retry")]
    [InlineData("short-allowance")]
    [InlineData("old-version")]
    [InlineData("future-version")]
    public void InvalidCombatSettingsAreRejectedBeforeAnEcsWorldIsCreated(string defect)
    {
        CombatSettings c = new(); c = defect switch
        {
            "move" => c with { Swordsman = c.Swordsman with { MoveTicks = 0 } },
            "death" => c with { Mage = c.Mage with { DeathTicks = 0 } },
            "initiative" => c with { Crossbowman = c.Crossbowman with { Initiative = -1 } },
            "windup" => c with { MageWindupTicks = 0 },
            "recovery" => c with { MageRecoveryTicks = 0 },
            "range" => c with { MageHexRange = 0 },
            "defender" => c with { Defender = new(0, 48) },
            "limit" => c with { MaximumWaveTicks = 0 },
            "retry" => c with { RetryTicks = 0 },
            "old-version" => c with { RulesVersion = 6 },
            "future-version" => c with { RulesVersion = 8 },
            _ => c with { NoHealthProgressTicks = 1 }
        };
        Assert.Throws<ArgumentException>(() => new Match(new Rules { Combat = c }));
        Assert.Throws<ArgumentException>(() => new Match(new Rules { Arena = new() { SquadCapacity = 0 } }));
    }
    [Fact]
    public void OverflowAndFractionalHexRangesAreRejected()
    {
        Assert.Throws<OverflowException>(() => new Match(new Rules { MageHealth = int.MaxValue }));
        Assert.Throws<OverflowException>(() => new Match(new Rules { Combat = new() { MageWindupTicks = int.MaxValue } }));
        Assert.Throws<ArgumentException>(() => new Match(new Rules { SoldierDamage = 1000000 }));
        Assert.Throws<ArgumentException>(() => new Match(new Rules { RangedReach = 1.5 }));
    }
    [Fact]
    public void FingerprintsAreCanonicalAndFreezeActualCombatInputs()
    {
        var rules = new Rules(); var a = new CombatConfiguration(rules);
        HexBoardDefinition reversed = rules.Combat.Board with
        { Cells = rules.Combat.Board.Cells.Reverse().Select(c => c with { Neighbors = c.Neighbors.Reverse().ToArray() }).ToArray(), Anchors = rules.Combat.Board.Anchors.Reverse().ToArray() };
        Assert.Equal(a.Fingerprint, new CombatConfiguration(rules with { Combat = rules.Combat with { Board = reversed } }).Fingerprint);
        Assert.Equal(a.Fingerprint, new CombatConfiguration(rules with { StartingGold = 999 }).Fingerprint);
        Assert.NotEqual(a.Fingerprint, new CombatConfiguration(rules with { MageDamage = 3 }).Fingerprint);
        Assert.NotEqual(a.Fingerprint, new CombatConfiguration(rules with { Combat = rules.Combat with { RetryTicks = 7 } }).Fingerprint);
        rules.Combat.Board.Cells[0].Neighbors[0] = 0;
        Assert.Equal(HexBoardDefinition.Default().Cells[0].Neighbors, a.Board.Cell(1).Neighbors);
    }
    [Theory]
    [InlineData("arrow")]
    [InlineData("rapid")]
    [InlineData("secondary")]
    [InlineData("range")]
    [InlineData("capacity")]
    public void CompleteIdentityAndWireRulesIncludeActualSquadAndWeaponInputs(string change)
    {
        Rules rules = new(); Rules changed = change switch
        {
            "arrow" => rules with { Combat = rules.Combat with { ArrowTower = new(13, 48) } },
            "rapid" => rules with { Combat = rules.Combat with { RapidArrowTower = new(7, 24) } },
            "secondary" => rules with { Arena = rules.Arena with { CatapultSecondaryDamage = 7 } },
            "range" => rules with { Arena = rules.Arena with { ArrowRange = 5 } },
            _ => rules with { Arena = rules.Arena with { SquadCapacity = 4 } }
        };
        using var baseline = new Match(rules, combatSeed: 1);
        using var candidate = new Match(changed, combatSeed: 1);
        if (change == "rapid") Assert.Equal(baseline.Configuration.ActorDecisionFingerprint, candidate.Configuration.ActorDecisionFingerprint);
        else Assert.NotEqual(baseline.Configuration.ActorDecisionFingerprint, candidate.Configuration.ActorDecisionFingerprint);
        Assert.NotEqual(baseline.ConfigurationFingerprint, candidate.ConfigurationFingerprint);
        MatchSnapshot received = Game.SnapshotPayload.Decode(Game.SnapshotPayload.Encode(candidate.Snapshot()))!;
        using var reconstructed = new Match(received.Rules, combatSeed: received.CombatSeed);
        Assert.Equal(candidate.Configuration.Fingerprint, reconstructed.Configuration.Fingerprint);
        Assert.Equal(candidate.Configuration.ActorDecisionFingerprint, reconstructed.Configuration.ActorDecisionFingerprint);
        Assert.Equal(candidate.ConfigurationFingerprint, reconstructed.ConfigurationFingerprint);
        Assert.Equal(candidate.ConfigurationFingerprint, RulesIdentity.Resolve(received.Rules));
        Assert.Equal(6, received.TowerCatalog.Length);
    }
    [Theory]
    [InlineData(12, 48)]
    [InlineData(15, 45)]
    public void SharedSquadDecisionsAndEventsRepeatAfterCompleteRulesReconstruction(int windup, int recovery)
    {
        // Tier A: complete rules/seed reconstruct squad actions and events; extra
        // observations cannot consume decisions. Paid tower plus explicit combat
        // poses, bounded 180 ticks; not a campaign or transport witness.
        Rules rules = new() { Combat = new() { ArrowTower = new(windup, recovery) } };
        Rules received = JsonSerializer.Deserialize<Rules>(JsonSerializer.Serialize(rules, WireJson.Options), WireJson.Options)!;
        using var first = new Match(rules, combatSeed: 1);
        using var restored = new Match(received, combatSeed: 1);
        var arena = new SharedArena([1]);
        int targetCell = arena.Board.Cell(arena.Tower(1, 0)).Neighbors.First(arena.CanOccupy);
        foreach (Match match in new[] { first, restored })
        {
            match.Join();
            Assert.True(VillageStrategyTests.Act(match, 1, "start").Accepted);
            Assert.True(VillageStrategyTests.Act(match, 1, "build", 0, Building.ArrowTower).Accepted);
            int[] enemies = Enumerable.Range(0, 3).Select(_ => match.Combat.Create(UnitType.Crossbowman, 0, 1, 1, Faction.Skeletons)).ToArray();
            foreach (int id in enemies) match.Combat.Seed(match.Combat.Inspect(id) with { Location = new(UnitLifecycle.Alive, new(targetCell, 1)) });
            int actor = match.Combat.Create(UnitType.Swordsman, 1, 1, 1, assignment: new(-1, 0, 0, 1));
            match.Combat.Seed(match.Combat.Inspect(actor) with { Location = new(UnitLifecycle.Alive, new(arena.Deployment(1, 0), 1)) });
        }
        Assert.Equal(first.ConfigurationFingerprint, restored.ConfigurationFingerprint);
        bool impact = false;
        for (int tick = 0; tick < 180; tick++)
        {
            _ = first.Snapshot(); _ = first.Combat.Events().Reverse().ToArray();
            first.Combat.Step(tick, first.Players.Values); restored.Combat.Step(tick, restored.Players.Values);
            MatchSnapshot a = first.Snapshot() with { MatchId = "session" }, b = restored.Snapshot() with { MatchId = "session" };
            Assert.Equal(JsonSerializer.Serialize(a, WireJson.Options), JsonSerializer.Serialize(b, WireJson.Options));
            CombatReservations rebuilt = CombatReservations.Reconstruct(arena, a.Squads);
            Assert.Equal(a.Reservations.Positions, rebuilt.Positions); Assert.Equal(a.Reservations.Transit, rebuilt.Transit);
            impact |= a.CombatEvents.Any(e => e.Type == CombatEventType.Impact && e.Landed);
        }
        Assert.True(impact);
    }
    [Fact]
    public void ExplicitAndGeneratedSeedsAreRetainedWithoutSessionIdentityInput()
    {
        using var a = new Match(matchId: "a", combatSeed: 123); using var b = new Match(matchId: "b", combatSeed: 123);
        Assert.Equal(123UL, a.CombatSeed); Assert.Equal(a.CombatSeed, b.CombatSeed); Assert.Equal(a.Configuration.Fingerprint, b.Configuration.Fingerprint);
        using var generated = new Match(); ulong seed = generated.CombatSeed;
        generated.Join(); generated.Snapshot(); Assert.Equal(seed, generated.CombatSeed);
    }
    [Fact]
    public void PublishedBoardAndDecisionArraysCannotMutateTheAuthority()
    {
        using var match = new Match(combatSeed: 123); match.Join(); VillageStrategyTests.Act(match, 1, "start");
        for (int n = 0; n < 3; n++) VillageStrategyTests.Act(match, 1, "ready");
        match.Step(); MatchSnapshot published = match.Snapshot();
        UnitState actor = published.Enemies.First(u => u.Decision!.Route.Length > 0);
        HexPosition expected = actor.Decision!.Route[0]; actor.Decision.Route[0] = default;
        published.Rules.Combat.Board.Cells[0].Neighbors[0] = 999;
        MatchSnapshot fresh = match.Snapshot();
        Assert.Equal(expected, fresh.Enemies.Single(u => u.Id == actor.Id).Decision!.Route[0]);
        Assert.DoesNotContain(999, fresh.Rules.Combat.Board.Cells[0].Neighbors);
        Assert.Equal(123UL, fresh.CombatSeed); Assert.Equal(match.ConfigurationFingerprint, fresh.ConfigurationFingerprint);
    }
}
