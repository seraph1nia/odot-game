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
        rules.Combat.Board.Cells[0].Neighbors[0] = 0;
        Assert.Equal(HexBoardDefinition.Default().Cells[0].Neighbors, a.Board.Cell(1).Neighbors);
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
