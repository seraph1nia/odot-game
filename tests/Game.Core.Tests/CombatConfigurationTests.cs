using Xunit;

namespace Game.Core.Tests;

public sealed class CombatConfigurationTests
{
    [Theory]
    [InlineData(UnitType.Swordsman, 2, 10, 30, 1, 12, 48)]
    [InlineData(UnitType.Berserker, 2, 20, 27, 1, 18, 54)]
    [InlineData(UnitType.Crossbowman, 2, 30, 30, 3, 18, 42)]
    [InlineData(UnitType.Mage, 2, 40, 30, 3, 24, 66)]
    public void CandidateProfilesAreCentralizedAndResearchChangesOnlyHealthAndDamage(UnitType type, int cost, int initiative, int move, int range, int windup, int recovery)
    {
        var config = new CombatConfiguration(new()); HexCombatProfile p = config.Unit(type);
        Assert.Equal((cost, initiative, move, range, windup, recovery, 48), (p.Size, p.Initiative, p.MoveTicks, p.HexRange, p.WindupTicks, p.RecoveryTicks, p.DeathTicks));
        Assert.Equal(p with { Health = HealthPoints.Ranked(p.Health, 2), Damage = HealthPoints.Ranked(p.Damage, 2) }, config.Unit(type, 2));
        using var combat = new CombatSimulation(new());
        int a = combat.Create(type, 1, 1, 1, rank: 2), b = combat.Create(type, 0, 1, 1, Faction.Skeletons, 2);
        Assert.Equal(combat.Read(a).Profile, combat.Read(b).Profile);
        Assert.Throws<ArgumentOutOfRangeException>(() => config.Unit(type, 3));
    }
    [Fact]
    public void MageCandidateHasTwoVictimBenefitAndSingleTargetCadenceTradeoff()
    {
        var c = new CombatConfiguration(new()); HexCombatProfile mage = c.Unit(UnitType.Mage), crossbow = c.Unit(UnitType.Crossbowman);
        Assert.Equal(1200, mage.Damage); Assert.Equal(2500, mage.Health);
        Assert.True((long)mage.Damage * crossbow.CadenceTicks < (long)crossbow.Damage * mage.CadenceTicks);
        Assert.True((long)mage.Damage * 2 * crossbow.CadenceTicks > (long)crossbow.Damage * mage.CadenceTicks);
        Assert.Equal(1, mage.SplashHexRadius); Assert.Equal(2, mage.VictimCap);
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
    [InlineData(1, 0, "79d304d3da6d850ff756b29178073f1d48f34d35259e183e4028e84cc991d693", 7127624517826291792UL, 2, 3, 3, 1)]
    [InlineData(3, 2, "7da1b06f7e590c3ee9894a31d0420d5c4daac0928cdff5c54b99ae8801993ebe", 12184540376889119036UL, 1, 1, 4, 2)]
    public void LegacyArrowAuthoringRetainsRecordedDecisionTranscript(int cap, int radius, string salt, ulong rank, int objective, int defender, int transitAnchor, int goalAnchor)
    {
        Rules rules = new() { Combat = new() { ArrowTower = new(12, 48, cap, radius) } };
        using var match = new Match(rules, combatSeed: 1);
        match.Join();
        Assert.True(VillageStrategyTests.Act(match, 1, "start").Accepted);
        Assert.True(VillageStrategyTests.Act(match, 1, "build", 0, Building.ArrowTower).Accepted);
        for (int anchor = 1; anchor <= 3; anchor++)
        {
            int id = match.Combat.Create(UnitType.Crossbowman, 0, 1, 1, Faction.Skeletons);
            match.Combat.Seed(match.Combat.Read(id) with { Hex = CombatFixture.At(match.Combat.Read(id), 8, anchor) });
        }
        int actor = match.Combat.Create(UnitType.Swordsman, 1, 1, 1);
        match.Combat.Seed(match.Combat.Read(actor) with { Hex = CombatFixture.At(match.Combat.Read(actor), 17, 1) });
        match.Combat.StartActions(match.Players.Values);
        Assert.Equal(salt, match.Configuration.ActorDecisionFingerprint.ToString());
        Assert.Equal(rank, match.Combat.Read(actor).Decision!.SchedulingRank);
        Assert.Equal(2, match.Players[1].Towers[0].TargetId);
        Assert.Equal(defender, match.Players[1].Defender.TargetId);
        Assert.Equal(objective, match.Combat.Read(actor).Decision!.ObjectiveId);
        Assert.Equal(new[] { new HexPosition(13, transitAnchor), new HexPosition(11, goalAnchor) }, match.Combat.Read(actor).Decision!.Route);
        foreach (int tick in new[] { 1, 12, 30, 31, 60, 61, 90 })
        {
            match.Combat.Advance(tick, match.Players.Values);
            UnitState unit = match.Combat.Read(actor);
            Assert.Equal(tick < 30 ? new HexPosition(17, 1) : new HexPosition(13, transitAnchor), unit.Hex!.Position);
            Assert.Equal(objective, unit.Decision!.ObjectiveId);
            Assert.Equal(tick < 30 ? [new HexPosition(13, transitAnchor), new HexPosition(11, goalAnchor)] : new[] { new HexPosition(11, goalAnchor) }, unit.Decision.Route);
        }
        string transcript = string.Join('|', match.Combat.Events().Select(e => $"{e.Tick}:{e.Type}:{e.Unit?.Id ?? 0}:{e.TargetId}:{e.Damage}:{e.Landed}:{string.Join(',', e.Victims)}"));
        string expected = "0:AttackStarted:1:4:0:False:|0:AttackStarted:2:4:0:False:|0:AttackStarted:3:4:0:False:|"
            + $"0:AttackStarted:0:{defender}:200:False:|0:AttackStarted:0:2:500:False:|12:Impact:0:{defender}:200:True:{defender}|"
            + $"12:DefenderShot:{defender}:{defender}:200:True:|12:Impact:0:2:500:True:2|"
            + (defender == 3 ? "12:Hit:2:0:500:True:|12:Hit:3:0:200:True:|" : "12:Hit:1:0:200:True:|12:Hit:2:0:500:True:|")
            + "30:Impact:1:4:1000:True:4|30:Impact:2:4:1000:True:4|30:Impact:3:4:1000:True:4|30:Hit:4:0:3000:True:";
        Assert.Equal(expected, transcript);
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
