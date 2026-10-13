using DevRunner;
using Game.Core;
using Xunit;
using Xunit.Abstractions;

namespace DevRunner.Tests;

// Numerical detector/admission controls, NOT rendered/PNG/ENet acceptance.
// Shared squads use one anchor + cosmetic formation, never city-local stacking.
// Paid fixed W1 input, actual commitments/impacts; finite4000 ticks. Old exact
// 301/638/973 windows are history, not current gameplay guarantees. Preserve
// linked cue, group, near/far/simultaneous, freshness, handoff and capture guards.
public sealed class MeleeVisualProofTests(ITestOutputHelper output)
{
    private static readonly HexBoard Board = new SharedArena([1, 2]).Board;
    private static void Act(Match match, int player, string action)
        => Assert.True(match.Apply(player, Command.FromSnapshot(match.Snapshot(), 1, action, player)).Accepted);
    private static Match Prepared(ulong seed = 1)
    {
        // One ordinary configured wave bounds terminal controls; this is not
        // a twenty-wave campaign/balance proof or a fabricated terminal flag.
        var match = new Match(new Rules { Campaign = new(CampaignDefinition.Default().Waves.Take(1).ToArray()) }, combatSeed: seed);
        match.Join(); match.Join(); Act(match, 1, "start");
        for (int gate = 0; gate < 3; gate++)
        {
            foreach (var (player, plan) in CampaignStrategy.RedistributionPreparation(match.Snapshot(), 1, 2))
                Assert.True(match.Apply(player, plan.Command(match.Snapshot(), player)).Accepted);
            if (gate < 2) { Act(match, 1, "ready"); Act(match, 2, "ready"); }
        }
        Assert.Equal(Phase.Preparation, match.Phase); Assert.Equal(0, match.Tick);
        Assert.All(match.Players.Values, c => Assert.Equal(6, c.Soldiers.Count));
        return match;
    }
    private static void Admit(Match match) { Act(match, 1, "ready"); Act(match, 2, "ready"); }
    private static MatchSnapshot Windup(Match match)
    {
        for (int n = 0; n < 4000 && match.Phase == Phase.Combat; n++)
        {
            if (MeleeVisualProof.HasMilestone(match.Snapshot(), MeleeCoverage.Windup, Board)) return match.Snapshot();
            match.Step();
        }
        throw new InvalidOperationException("Fixed paid shared W1 did not produce a genuine group windup.");
    }
    private static MatchSnapshot Finish(Match match)
    {
        for (int n = 0; n < 4000 && match.Phase == Phase.Combat; n++) match.Step();
        Assert.True(match.Phase is Phase.Victory or Phase.Defeat); return match.Snapshot();
    }
    private static UiObservation Frame(MatchSnapshot state)
    {
        UnitState[] actors = CombatPlayback.All(state).Concat(state.DyingBodies).Where(u => u.Hex is not null).ToArray();
        UnitObservation[] units = actors.Select(u =>
        {
            var p = Game.SharedCombatProjection.Position(Board, u.Hex!, u.FormationSlot, state.Rules.Arena.SquadCapacity, u.IsBoss, state.Tick);
            return new UnitObservation
            {
                Id = u.Id,
                Type = u.Type,
                Faction = u.Faction,
                Class = u.Class,
                Destination = u.Destination,
                SquadId = u.SquadId,
                FormationSlot = u.FormationSlot,
                IsBoss = u.IsBoss,
                Deployed = u.Deployed,
                Visible = u.Deployed,
                Dead = u.Hex!.Lifecycle == UnitLifecycle.Dying,
                Hex = u.Hex,
                X = p.X,
                Z = p.Z,
                AttackSequence = u.AttackSequence,
                ActionStartTick = u.ActionStartTick,
                ImpactTick = u.ImpactTick,
                AttackActive = u.AttackSequence > 0 && state.Tick >= u.ActionStartTick && state.Tick < u.ReadyTick,
                WeaponAttached = true,
                BoneRotation = "numerical-projection-not-imported-bone"
            };
        }).ToArray();
        StrikeObservation[] strikes = actors.Where(u => u.Class == UnitClass.Melee && u.Hex is { Lifecycle: UnitLifecycle.Alive }
            && !u.TargetCity && u.AttackSequence > 0 && state.Tick >= u.ActionStartTick && state.Tick < u.ImpactTick + 12
            && units.Any(t => t.Id == u.TargetId)).Select(u =>
        {
            UnitObservation t = units.Single(t => t.Id == u.TargetId);
            return new StrikeObservation
            {
                Id = u.Id,
                TargetId = u.TargetId,
                AttackSequence = u.AttackSequence,
                ImpactTick = u.ImpactTick,
                AttackLanded = u.AttackLanded,
                Visible = true,
                CueStyle = "dashed-intent-local-strike",
                Phase = Game.CombatVisualTiming.MeleePhase(u, state.Tick),
                IntentVisible = state.Tick < u.ImpactTick,
                IntentDashes = 12,
                StrikeVisible = state.Tick >= u.ImpactTick,
                StrikeRadius = .34f,
                ImpactVisible = u.AttackLanded == true && state.Tick >= u.ImpactTick,
                TargetX = t.X,
                TargetZ = t.Z
            };
        }).ToArray();
        return new()
        {
            Units = units,
            Strikes = strikes,
            CombatTick = state.Tick,
            Revision = state.Revision,
            PhaseText = state.Paused ? "Combat PAUSED" : "Combat"
        };
    }
    [Fact]
    public void ReleasedCosmeticDeathsMayOverlapWhileSettledLivingActorsRemainDistinct()
    {
        var living = new UnitObservation { Id = 1, Destination = 1, Visible = true, Deployed = true };
        var dead = new UnitObservation { Id = 2, Destination = 2, Visible = true, Deployed = true, Dead = true, X = .2f };
        Runner.RenderedContact(new() { Units = [living, dead] });
        Assert.Throws<InvalidOperationException>(() => Runner.RenderedContact(new() { Units = [living, dead with { Dead = false }] }));
        Runner.RenderedContact(new() { Units = [living, dead with { Visible = false }] });
        Runner.RenderedContact(new() { Units = [living, dead with { X = .5f }] });
        HexUnitState transit = new(1, 1, Faction.Adventurers, UnitLifecycle.Alive, new(14, 1), UnitActionKind.Moving,
            new(13, 1), StartTick: 100, EndTick: 130);
        Runner.RenderedContact(new() { Units = [living with { Hex = transit }, dead] });
        Assert.Throws<InvalidOperationException>(() => Runner.RenderedContact(new() { Units = [living with { Hex = transit with { Action = UnitActionKind.Waiting } }, dead with { Dead = false }] }));
    }
    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    [InlineData(123UL)]
    public void PaidSharedOpeningProvidesRealLinkedGroupActionOpportunities(ulong seed)
    {
        using Match match = Prepared(seed); Admit(match);
        MeleeCoverage found = MeleeCoverage.None;
        for (int n = 0; n < 4000 && match.Phase == Phase.Combat; n++)
        {
            match.Step();
            foreach (MeleeWitness witness in MeleeVisualProof.Inspect(Frame(match.Snapshot()), Board)) found |= witness.Coverage;
        }
        output.WriteLine($"Actual numerical shared actions seed={seed}: {found}; no rendered proof.");
        const MeleeCoverage required = MeleeCoverage.Shared | MeleeCoverage.Near | MeleeCoverage.Far | MeleeCoverage.Simultaneous | MeleeCoverage.Windup | MeleeCoverage.Impact;
        Assert.Equal(required, found & required);
    }
    [Fact]
    public void CueLinkGroupAndCommitmentNegativesCannotAuthorizeAttackOrSimultaneity()
    {
        using Match match = Prepared(); Admit(match); MatchSnapshot live = Windup(match); UiObservation frame = Frame(live);
        Assert.Contains(MeleeVisualProof.Inspect(frame, Board), w => (w.Coverage & MeleeCoverage.Windup) != 0);
        foreach (UiObservation broken in new[]
        {
            frame with { Strikes = frame.Strikes.Select(s => s with { TargetX = s.TargetX + .1f }).ToArray() },
            frame with { Strikes = frame.Strikes.Select(s => s with { CueStyle = "opaque-beam" }).ToArray() },
            frame with { Strikes = frame.Strikes.Select(s => s with { AttackSequence = s.AttackSequence + 1 }).ToArray() },
            frame with { Strikes = frame.Strikes.Select(s => s with { StrikeRadius = 3 }).ToArray() },
            frame with { Units = frame.Units.Select(u => u with { WeaponAttached = false }).ToArray() }
        }) Assert.All(MeleeVisualProof.Inspect(broken, Board), w => Assert.Equal(MeleeCoverage.None, w.Coverage & (MeleeCoverage.Windup | MeleeCoverage.Impact | MeleeCoverage.Simultaneous)));
        Assert.Throws<InvalidOperationException>(() => Runner.RenderedContact(frame with { Strikes = frame.Strikes.Select(s => s with { StrikeRadius = 3 }).ToArray() }));
    }
    [Fact]
    public void FreshHandoffPauseAndImmutablePersistenceKeepTheirOriginalGuards()
    {
        using Match match = Prepared(); MatchSnapshot preparation = match.Snapshot();
        Assert.Throws<InvalidOperationException>(() => new MeleeAdmission(preparation with { Tick = 1 }));
        var admission = new MeleeAdmission(preparation);
        Assert.Throws<InvalidOperationException>(() => admission.Arm(preparation));
        Admit(match); admission.Arm(match.Snapshot()); MatchSnapshot live = Windup(match); admission.Witness(live, Board);
        Assert.True(admission.LiveWitnesses > 0); Assert.False(admission.MayPause(live, live, MeleeCoverage.Windup, Board));
        Assert.Throws<InvalidOperationException>(() => admission.Handoff(TimeSpan.FromSeconds(12.001), 12, 4));
        Assert.Throws<InvalidOperationException>(() => admission.Handoff(TimeSpan.FromSeconds(11), 11, 4));
        Assert.Throws<InvalidOperationException>(() => admission.Handoff(TimeSpan.FromSeconds(11), 12, 3));
        admission.Handoff(TimeSpan.FromSeconds(11), 12, 4);
        Assert.Throws<InvalidOperationException>(() => admission.Handoff(TimeSpan.FromSeconds(11), 12, 4));
        Assert.True(admission.MayPause(live, live, MeleeCoverage.Windup, Board));
        Assert.False(admission.MayPause(live with { MatchId = "other" }, live, MeleeCoverage.Windup, Board));
        match.Step(); Assert.False(admission.MayPause(live, match.Snapshot(), MeleeCoverage.Windup, Board));
        live = Windup(match); Act(match, 2, "pause"); MatchSnapshot paused = match.Snapshot(); UiObservation frame = Frame(paused);
        MeleeAdmission.FrozenFrame(frame, paused);
        Assert.Throws<InvalidOperationException>(() => MeleeAdmission.FrozenFrame(frame with { CombatTick = frame.CombatTick - 1 }, paused));
        Assert.Throws<InvalidOperationException>(() => MeleeAdmission.FrozenFrame(frame with { Revision = frame.Revision - 1 }, paused));
        Assert.Throws<InvalidOperationException>(() => MeleeAdmission.FrozenFrame(frame with { PhaseText = "Combat" }, paused));
        Assert.Throws<InvalidOperationException>(() => MeleeAdmission.FrozenFrame(frame, paused with { Phase = Phase.Defeat }));
        var captures = new Game.OwnedFrameCapture(); byte[] pixels = new byte[16];
        var first = captures.Acquire("overview", "overview.png", 2, 2, pixels, "numerical owned capture ordering");
        var second = captures.Acquire("close", "close.png", 2, 2, pixels, "not a rendered image");
        Assert.Throws<InvalidOperationException>(() => captures.Persist(second.Id, (_, p) => p));
        Assert.Equal(first, captures.Persist(first.Id, (_, p) => p)); Assert.Equal(second, captures.Persist(second.Id, (_, p) => p));
        var playback = new CombatPlayback(); playback.Accept(paused);
        for (int n = 0; n < 1200; n++) { match.Step(); playback.Advance(.1, true); }
        Assert.Equal(paused.Tick, match.Tick); Assert.Equal(paused.Tick, playback.Tick); Assert.Equal(0, captures.Pending);
        Act(match, 2, "resume"); MatchSnapshot terminal = Finish(match);
        Assert.False(admission.MayPause(terminal, terminal, MeleeCoverage.Windup, Board));
        Assert.False(match.Apply(2, Command.FromSnapshot(live, 100, "pause", 2)).Accepted);
        Assert.False(match.Apply(2, Command.FromSnapshot(terminal, 101, "pause", 2)).Accepted);
    }
    [Fact]
    public void NewestPresentationAndHistoricalEventRetentionAreSeparateFromPauseAuthority()
    {
        using Match match = Prepared(); Admit(match); MatchSnapshot first = Windup(match);
        var playback = new CombatPlayback(); Assert.True(playback.Accept(first));
        var history = new ChildEvents(); history.Add(new("snapshot", State: first), 100);
        match.Step(); MatchSnapshot next = match.Snapshot(); history.Add(new("snapshot", State: next), 100);
        Assert.True(playback.Accept(next)); Assert.False(playback.Accept(first));
        Assert.Equal(first, history.Find(e => e.State?.Revision == first.Revision)!.State);
        Assert.True(match.Apply(2, Command.FromSnapshot(first, 100, "pause", 2)).Accepted);
        Assert.Equal(next.Tick, match.Tick); Assert.NotEqual(first.Tick, match.Tick);
        playback.Accept(match.Snapshot()); playback.Advance(.1, true); Assert.Equal(next.Tick, playback.Tick);
    }
}
