using Xunit;

namespace Game.Core.Tests;

public sealed class CombatConfigurationTests
{
    [Theory]
    [InlineData(UnitType.Swordsman, 2, 10, 30, 1, 12, 48)]
    [InlineData(UnitType.Berserker, 2, 20, 27, 1, 18, 54)]
    [InlineData(UnitType.Crossbowman, 2, 30, 30, 3, 18, 42)]
    [InlineData(UnitType.Mage, 2, 40, 30, 3, 24, 66)]
    public void CandidateProfilesAreCentralizedAndResearchChangesHealthAndBothDirectChannels(UnitType type, int cost, int initiative, int move, int range, int windup, int recovery)
    {
        var config = new CombatConfiguration(new()); HexCombatProfile p = config.Unit(type);
        Assert.Equal((cost, initiative, move, range, windup, recovery, 48), (p.Size, p.Initiative, p.MoveTicks, p.HexRange, p.WindupTicks, p.RecoveryTicks, p.DeathTicks));
        Assert.Equal(p with { Health = HealthPoints.Ranked(p.Health, 2), Damage = HealthPoints.Ranked(p.Damage, 2), SecondaryDamage = HealthPoints.Ranked(p.SecondaryDamage, 2) }, config.Unit(type, 2));
        using var combat = new CombatSimulation(new());
        int a = combat.Create(type, 1, 1, 1, rank: 2), b = combat.Create(type, 0, 1, 1, Faction.Skeletons, 2);
        Assert.Equal(combat.Read(a).Profile, combat.Read(b).Profile);
        Assert.Throws<ArgumentOutOfRangeException>(() => config.Unit(type, 3));
    }
    [Fact]
    public void MageCandidateHasWithinGroupSecondaryDamageAndSingleTargetCadenceTradeoff()
    {
        var c = new CombatConfiguration(new()); HexCombatProfile mage = c.Unit(UnitType.Mage), crossbow = c.Unit(UnitType.Crossbowman);
        Assert.Equal(1200, mage.Damage); Assert.Equal(2500, mage.Health);
        Assert.True((long)mage.Damage * crossbow.CadenceTicks < (long)crossbow.Damage * mage.CadenceTicks);
        Assert.True((long)mage.Damage * 2 * crossbow.CadenceTicks > (long)crossbow.Damage * mage.CadenceTicks);
        Assert.Equal(400, mage.SecondaryDamage); Assert.Equal(0, crossbow.SecondaryDamage);
        Assert.Equal(mage.StructureDamage, c.Unit(UnitType.Mage, rank: 2).StructureDamage);
        Assert.Equal(new ResourceCost(1, Cloth: 3), Catalogs.Units(new()).Single(u => u.Type == UnitType.Mage).Recruitment);
        // This is a profile check, not the still-required ordinary role fixture.
    }
    [Theory]
    [InlineData("move")]
    [InlineData("death")]
    [InlineData("capacity")]
    [InlineData("initiative")]
    [InlineData("windup")]
    [InlineData("recovery")]
    [InlineData("splash")]
    [InlineData("defender")]
    [InlineData("limit")]
    [InlineData("retry")]
    [InlineData("short-allowance")]
    public void InvalidCombatSettingsAreRejectedBeforeAnEcsWorldIsCreated(string defect)
    {
        CombatSettings c = new(); c = defect switch
        {
            "move" => c with { Swordsman = c.Swordsman with { MoveTicks = 0 } },
            "death" => c with { Mage = c.Mage with { DeathTicks = 0 } },
            "capacity" => c with { Swordsman = c.Swordsman with { Size = 7 } },
            "initiative" => c with { Crossbowman = c.Crossbowman with { Initiative = -1 } },
            "windup" => c with { MageWindupTicks = 0 },
            "recovery" => c with { MageRecoveryTicks = 0 },
            "splash" => c with { MageSplashHexRadius = -1 },
            "defender" => c with { Defender = new(0, 48) },
            "limit" => c with { MaximumWaveTicks = 0 },
            "retry" => c with { RetryTicks = 0 },
            _ => c with { NoHealthProgressTicks = 1 }
        };
        Assert.Throws<ArgumentException>(() => new Match(new Rules { Combat = c }));
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
    public void FingerprintsAreCanonicalAndFreezeAllCombatInputs()
    {
        var rules = new Rules(); var a = new CombatConfiguration(rules);
        HexBoardDefinition reversed = rules.Combat.Board with
        { Cells = rules.Combat.Board.Cells.Reverse().Select(c => c with { Neighbors = c.Neighbors.Reverse().ToArray() }).ToArray(), Anchors = rules.Combat.Board.Anchors.Reverse().ToArray() };
        Assert.Equal(a.Fingerprint, new CombatConfiguration(rules with { Combat = rules.Combat with { Board = reversed } }).Fingerprint);
        Assert.Equal(a.Fingerprint, new CombatConfiguration(rules with { StartingGold = 999 }).Fingerprint);
        Assert.NotEqual(a.Fingerprint, new CombatConfiguration(rules with { MageDamage = 3 }).Fingerprint);
        Assert.NotEqual(a.Fingerprint, new CombatConfiguration(rules with { Combat = rules.Combat with { RetryTicks = 7 } }).Fingerprint);
        var equivalent = new CombatConfiguration(rules with { Combat = rules.Combat with { RapidArrowTower = rules.Combat.RapidArrowTower with { VictimCap = 3, SplashHexRadius = 2 } } });
        Assert.Equal(a.Towers, equivalent.Towers);
        Assert.Equal(a.ActorDecisionFingerprint, equivalent.ActorDecisionFingerprint);
        Assert.Equal(a.Fingerprint, equivalent.Fingerprint);
        rules.Combat.Board.Cells[0].Neighbors[0] = 0;
        Assert.Equal(HexBoardDefinition.Default().Cells[0].Neighbors, a.Board.Cell(1).Neighbors);
    }
    [Theory]
    [InlineData(3, 0)]
    [InlineData(1, 2)]
    [InlineData(3, 2)]
    public void FullIdentityIncludesRetainedArrowDecisionInputsDespiteNormalizedDamage(int cap, int radius)
    {
        Rules rules = new();
        Rules changed = rules with { Combat = rules.Combat with { ArrowTower = new(12, 48, cap, radius) } };
        using var baseline = new Match(rules, combatSeed: 1);
        using var candidate = new Match(changed, combatSeed: 1);
        Assert.Equal(baseline.Configuration.Towers, candidate.Configuration.Towers);
        Assert.All(candidate.Configuration.Towers.Where(t => t.Type == Building.ArrowTower), t =>
        { Assert.Equal(1, t.VictimCap); Assert.Equal(0, t.SplashHexRadius); });
        Assert.NotEqual(baseline.Configuration.ActorDecisionFingerprint, candidate.Configuration.ActorDecisionFingerprint);
        Assert.NotEqual(baseline.Configuration.Fingerprint, candidate.Configuration.Fingerprint);
        Assert.NotEqual(baseline.ConfigurationFingerprint, candidate.ConfigurationFingerprint);
        MatchSnapshot received = Game.SnapshotPayload.Decode(Game.SnapshotPayload.Encode(candidate.Snapshot()))!;
        Assert.Equal(candidate.ConfigurationFingerprint, received.ConfigurationFingerprint);
        Assert.Equal(candidate.ConfigurationFingerprint, RulesIdentity.Resolve(received.Rules));
        Assert.Equal(cap, received.Rules.Combat.ArrowTower.VictimCap);
        Assert.Equal(radius, received.Rules.Combat.ArrowTower.SplashHexRadius);
        using var reconstructed = new Match(received.Rules, combatSeed: received.CombatSeed);
        Assert.Equal(candidate.Configuration.Fingerprint, reconstructed.Configuration.Fingerprint);
        Assert.Equal(candidate.Configuration.ActorDecisionFingerprint, reconstructed.Configuration.ActorDecisionFingerprint);
        Assert.Equal(candidate.ConfigurationFingerprint, reconstructed.ConfigurationFingerprint);
        Assert.Equal(candidate.Configuration.Fingerprint, new CombatConfiguration(changed).Fingerprint);
    }
    [Theory]
    [InlineData(1, 0)]
    [InlineData(3, 2)]
    public void SharedSquadDecisionsAndEventsRepeatAfterCompleteRulesReconstruction(int cap, int radius)
    {
        // Tier A: replace old six-anchor/per-member transcript, not refresh its
        // golden. Complete rules/seed reconstruct actual squad actions and events;
        // extra observations cannot consume decisions. Explicit combat fixture,
        // paid tower command, bounded 180 ticks; no campaign/transport claim.
        Rules rules = new() { Combat = new() { ArrowTower = new(12, 48, cap, radius) } };
        Rules received = System.Text.Json.JsonSerializer.Deserialize<Rules>(System.Text.Json.JsonSerializer.Serialize(rules, WireJson.Options), WireJson.Options)!;
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
            Assert.Equal(System.Text.Json.JsonSerializer.Serialize(a, WireJson.Options), System.Text.Json.JsonSerializer.Serialize(b, WireJson.Options));
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
        for (int n = 0; n < 4; n++) VillageStrategyTests.Act(match, 1, "ready");
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
