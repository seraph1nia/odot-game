using DevRunner;
using Game;
using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

public sealed class ResearchWitnessTests(ITestOutputHelper output)
{
    [Fact]
    public void OrdinaryOpeningEarnsFireAndFieldsAMageWithinTenWaves()
    {
        using var match = new Match(combatSeed: 1); match.Join(); match.Join(); Assert.True(VillageStrategyTests.Act(match, 1, "start").Accepted);
        bool burn = false;
        for (int steps = 0; steps < 30000 && match.Phase is not (Phase.Victory or Phase.Defeat) && match.Wave <= 10 && !burn; steps++)
        {
            if (match.Phase == Phase.Combat) { match.Step(); burn = match.Enemies.Any(u => u.Statuses.Burn is not null); continue; }
            foreach (City city in match.Players.Values.Where(c => !c.Ready && !c.Eliminated))
            {
                for (int actions = 0; actions < 100; actions++)
                {
                    MatchSnapshot state = match.Snapshot(); EconomyAction? action = city.Id == 1 ? CampaignStrategy.ResearchWitness(state, city.Id) : CampaignStrategy.Next(state, city.Id);
                    if (action is null) break; Assert.True(match.Apply(city.Id, action.Command(state, city.Id)).Accepted);
                }
                if (city.Id == 1)
                    foreach (TechnologyId id in new[] { TechnologyId.MagicFoundation, TechnologyId.Fire })
                        if (match.Economy.Research.Eligibility(city.Research, id).Available)
                        { Assert.True(match.Apply(1, new(1, match.Id, match.Phase, match.TurnSerial, "research-tech", 1, Technology: id)).Accepted); output.WriteLine($"W{match.Wave}: {id}, points={city.Research.Points}, productions={match.ProductionCount}"); }
                if (city.Id == 1 && match.Phase == Phase.Preparation) output.WriteLine($"W{match.Wave} army={string.Join(',', city.Soldiers.Select(u => u.Type))} stocks={city.Resources} buildings={string.Join(',', city.Slots.Select(s => s.Type))}");
                Assert.True(VillageStrategyTests.Act(match, city.Id, "ready").Accepted);
            }
            if (match.Phase == Phase.Preparation && match.Players.Values.All(c => c.Ready)) match.Step();
        }
        Assert.True(burn, $"No burn by W{match.Wave}, {match.Phase}"); Assert.True(match.Players[1].Research.Has(TechnologyId.Fire));
        Assert.Contains(match.Players[1].Soldiers, u => u.Type == UnitType.Mage && u.Capabilities.BurnPercent == 20);
        Assert.False(match.Economy.Research.Eligibility(match.Players[1].Research, TechnologyId.Frost).Available);
    }
    [Fact]
    public void CurrentStatusesBaselinePauseAndDetachWithoutReplay()
    {
        StatusState status = StatusPolicy.Apply(new(), new(2, StatusKind.Poison, 1, 1, 0, 100), new());
        UnitState enemy = new(2, 1000, Origin: 1, Destination: 1) { Type = UnitType.Swordsman, Faction = Faction.Skeletons, Statuses = status };
        MatchSnapshot State(long revision, long tick, bool paused, long seq) => new("a", revision, tick, Phase.Combat, paused, 1, 3, 3, new(), [], [enemy])
        { EventSequence = seq, OldestEventSequence = seq, CombatEvents = [new(seq, tick, CombatEventType.Hit, enemy)] };
        var playback = new CombatPlayback(); playback.Accept(State(1, 60, true, 1)); Assert.Empty(playback.Drain());
        playback.Advance(10, true); Assert.Equal(60, playback.Tick); Assert.Contains("Poison", ProgressionPresentation.StatusText(playback.Units()[0].Statuses, playback.Tick));
        UnitState returned = playback.Units()[0]; returned.Statuses.Poison[0] = returned.Statuses.Poison[0] with { Strength = 999 }; Assert.Equal(100, playback.Units()[0].Statuses.Poison[0].Strength);
        playback.Accept(State(2, 300, true, 100)); Assert.Empty(playback.Drain()); Assert.Contains("Poison", ProgressionPresentation.StatusBadge(playback.Units()[0].Statuses, playback.Tick));
        playback.Advance(10, false); Assert.Equal(300, playback.Tick);
        playback.Accept(State(3, 361, true, 200), baseline: true); Assert.Empty(playback.Drain()); Assert.Empty(ProgressionPresentation.StatusBadge(playback.Units()[0].Statuses, playback.Tick));
        playback.Accept(State(4, 362, false, 201) with { Phase = Phase.Building, Enemies = [], CombatEvents = [] }, baseline: true);
        Assert.Empty(playback.Units()); Assert.Empty(playback.Drain());
        playback.Accept(State(1, 0, false, 0) with { MatchId = "fresh", Phase = Phase.Building, Enemies = [], CombatEvents = [] }, baseline: true); Assert.Empty(playback.Units());
    }
    [Fact]
    public void ResearchControlsUseOwnedStageAndAuthoritativeEligibilityWithoutSpeculativeCues()
    {
        using var match = new Match(); match.Join(); match.Join(); Assert.True(VillageStrategyTests.Act(match, 1, "start").Accepted);
        match.Players[1].Research = new(9);
        MatchSnapshot state = match.Snapshot(); CityState city = state.Players[0];
        TechnologyEligibility Access(MatchSnapshot value, CityState target, bool connected = true) => ProgressionPresentation.ResearchAccess(value, target, 1, connected, TechnologyId.MagicFoundation);
        Assert.True(Access(state, city).Available); Assert.True(Access(state with { Phase = Phase.Preparation }, city).Available);
        Assert.False(Access(state, state.Players[1]).Available); Assert.False(Access(state, city, false).Available);
        Assert.False(Access(state with { Paused = true }, city).Available); Assert.False(Access(state, city with { Ready = true }).Available);
        Assert.False(Access(state, city with { Health = 0 }).Available); Assert.False(Access(state with { Phase = Phase.Combat }, city).Available);
        Assert.Contains("Requires", ProgressionPresentation.ResearchAccess(state, city, 1, true, TechnologyId.Fire).Reason);
        var cues = new ActionCues(); cues.Clear(state.MatchId);
        var request = new Command(1, state.MatchId, state.Phase, state.TurnSerial, "research-tech", 1, Technology: TechnologyId.MagicFoundation);
        cues.Observe(request, new(1, false, "locked")); Assert.Empty(cues.Drain());
        cues.Observe(request with { Sequence = 2 }, new(2, true, "accepted")); Assert.Single(cues.Drain());
        cues.Observe(request with { Sequence = 2 }, new(2, true, "retry")); Assert.Empty(cues.Drain());
    }
    [Theory]
    [InlineData(UnitType.Mage, 20, 0)]
    [InlineData(UnitType.Crossbowman, 0, 10)]
    public void ResearchedSupportProducesCapturedPeriodicDamageWithOrdinaryEquipment(UnitType role, int burn, int poison)
    {
        int Fight(UnitCapabilities capabilities)
        {
            using var combat = new CombatSimulation(new Rules { DefenderDamage = 0 });
            int attacker = combat.Create(role, 1, 1, 1, capabilities: capabilities); UnitState ally = combat.Read(attacker); combat.Seed(ally with { Hex = CombatFixture.At(ally, 11, 1) });
            int defender = combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons, level: 3); UnitState enemy = combat.Read(defender); combat.Seed(enemy with { Hex = CombatFixture.At(enemy, 8, 1) });
            int total = 0; long cursor = 0;
            for (int tick = 1; tick <= 400; tick++) { combat.Step(tick, []); total += combat.Events().Where(e => e.Sequence > cursor && e.Type == CombatEventType.Hit).Sum(e => e.Periodic.Sum(p => p.Damage)); cursor = combat.EventSequence; }
            return total;
        }
        var economy = new EconomyConfiguration(new()); output.WriteLine($"{role}: equipment {economy.Recruitment(role)}, food {economy.Upkeep(role)}/battle; research path 9 points; comparison 400 ticks");
        Assert.Equal(0, Fight(default)); int damage = Fight(new(FoundationPercent: 5, BurnPercent: burn, PoisonPercent: poison)); Assert.True(damage > 0); output.WriteLine($"Captured periodic damage {HealthPoints.Format(damage)}");
    }
}
