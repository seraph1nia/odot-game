using DevRunner;
using Game.Core;
using Xunit;
using Xunit.Abstractions;

namespace DevRunner.Tests;

public sealed class MeleeVisualProofTests(ITestOutputHelper output)
{
    private static readonly HexBoard Board = new(HexBoardDefinition.Default());
    private static UnitObservation Observe(UnitState unit, long tick)
    {
        HexUnitState hex = unit.Hex!;
        HexCoordinate cell = hex.Lifecycle == UnitLifecycle.Queued ? default : Board.Cell(hex.Position.Cell).Coordinate;
        HexAnchor? anchor = hex.Lifecycle == UnitLifecycle.Queued ? null : Board.Anchor(hex.Position.Anchor);
        return new()
        {
            Id = unit.Id,
            Type = unit.Type,
            Faction = unit.Faction,
            Class = unit.Class,
            Destination = unit.Destination,
            Deployed = unit.Deployed,
            Visible = unit.Deployed,
            Dead = hex.Lifecycle == UnitLifecycle.Dying,
            Hex = hex,
            X = cell.Column * 3 + (cell.R & 1) * 1.5f + (anchor?.AnchorX ?? 0) / 1000f,
            Z = cell.R * 2.598076211f + (anchor?.AnchorForward ?? 0) / 1000f,
            AttackSequence = unit.AttackSequence,
            ActionStartTick = unit.ActionStartTick,
            ImpactTick = unit.ImpactTick,
            AttackActive = unit.AttackSequence > 0 && tick >= unit.ActionStartTick && tick < unit.ReadyTick,
            WeaponAttached = true,
            BoneRotation = "fixture",
        };
    }
    // This checks the ordinary setup's action opportunities and the detector;
    // it deliberately cannot establish imported rigs, PNGs or rendered clearance.
    private static UiObservation Frame(MatchSnapshot state)
    {
        UnitState[] actors = state.Players.Single(c => c.Id == 1).Soldiers.Concat(state.Enemies.Where(u => u.Destination == 1))
            .Concat(state.DyingBodies.Where(u => u.Destination == 1)).ToArray();
        UnitObservation[] units = actors.Select(u => Observe(u, state.Tick)).ToArray();
        StrikeObservation[] strikes = actors.Where(u => u.Class == UnitClass.Melee && u.Hex is { Lifecycle: UnitLifecycle.Alive, Action: UnitActionKind.Windup or UnitActionKind.Recovery }
            && !u.TargetCity && u.AttackSequence > 0 && state.Tick >= u.ActionStartTick && state.Tick < u.ImpactTick + 12)
            .Where(u => units.Any(t => t.Id == u.TargetId)).Select(u =>
            {
                UnitObservation target = units.Single(t => t.Id == u.TargetId);
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
                    TargetX = target.X,
                    TargetZ = target.Z
                };
            }).ToArray();
        return new() { Units = units, Strikes = strikes, CombatTick = state.Tick };
    }
    private static void Act(Match match, int city, string action, int slot = -1, Building building = Building.Empty)
        => Assert.True(match.Apply(city, new(1, match.Id, match.Phase, match.TurnSerial, action, city, slot, building, ExpectedGeneration: slot is >= 0 and < 9 ? match.Players[city].Slots[slot].Generation : 0)).Accepted);
    [Fact]
    public void AVisibleRetainedDeathStillRequiresRenderedBodyClearance()
    {
        var living = new UnitObservation { Id = 1, Destination = 1, Visible = true, Deployed = true };
        var dying = new UnitObservation { Id = 2, Destination = 1, Visible = true, Deployed = true, Dead = true, X = .2f };
        Assert.Throws<InvalidOperationException>(() => Runner.RenderedContact(new() { Units = [living, dying] }));
        Runner.RenderedContact(new() { Units = [living, dying with { Visible = false }] });
        Runner.RenderedContact(new() { Units = [living, dying with { X = .5f }] });
    }
    [Fact]
    public void CommittedTransitCanCrossButSettledLivingAndDyingAnchorsCannotCollapse()
    {
        HexUnitState transit = new(1, 1, Faction.Adventurers, UnitLifecycle.Alive, new(14, 1), UnitActionKind.Moving,
            new(11, 1), StartTick: 100, EndTick: 130);
        var moving = new UnitObservation { Id = 1, Destination = 1, Visible = true, Deployed = true, Hex = transit };
        var standing = new UnitObservation { Id = 2, Destination = 1, Visible = true, Deployed = true, X = .1f };
        Runner.RenderedContact(new() { Units = [moving, standing] });
        Runner.RenderedContact(new() { Units = [moving with { Dead = true, Hex = transit with { Lifecycle = UnitLifecycle.Dying, FrozenMoveTicks = 15 } }, standing] });
        Assert.Throws<InvalidOperationException>(() => Runner.RenderedContact(new() { Units = [moving with { Hex = transit with { Action = UnitActionKind.Waiting } }, standing] }));
        Assert.Throws<InvalidOperationException>(() => Runner.RenderedContact(new() { Units = [moving with { Hex = transit with { Action = UnitActionKind.Waiting } }, standing with { Dead = true }] }));
    }
    [Fact]
    public void AnUnlinkedCueCannotProveAnAttackOrASimultaneousExchange()
    {
        UnitObservation Unit(int id, Faction faction, int cell, int anchor) => Observe(new UnitState(id, 1000, 0, 1, 1)
        {
            Faction = faction,
            Type = UnitType.Swordsman,
            Deployed = true,
            AttackSequence = 1,
            ActionStartTick = 0,
            ImpactTick = 12,
            ReadyTick = 60,
            Hex = new(id, 1, faction, UnitLifecycle.Alive, new(cell, anchor), UnitActionKind.Windup, StartTick: 0, EndTick: 60)
        }, 6);
        UnitObservation actor = Unit(1, Faction.Adventurers, 14, 1), ally = Unit(2, Faction.Adventurers, 14, 3), enemy = Unit(3, Faction.Skeletons, 11, 1), opponent = Unit(4, Faction.Skeletons, 11, 3);
        var frame = new UiObservation
        {
            Units = [actor, ally, enemy, opponent],
            CombatTick = 6,
            Strikes = [new() { Id = 1, TargetId = 3, AttackSequence = 1, ImpactTick = 12, Visible = true, TargetX = enemy.X, TargetZ = enemy.Z,
                CueStyle = "dashed-intent-local-strike", Phase = "intent", IntentVisible = true, IntentDashes = 12, StrikeRadius = .34f },
                new() { Id = 3, TargetId = 1, AttackSequence = 1, ImpactTick = 12, Visible = true, TargetX = actor.X + .1f, TargetZ = actor.Z,
                CueStyle = "dashed-intent-local-strike", Phase = "intent", IntentVisible = true, IntentDashes = 12, StrikeRadius = .34f }]
        };
        MeleeWitness[] found = MeleeVisualProof.Inspect(frame, Board);
        Assert.NotEqual(MeleeCoverage.None, found.Single(w => w.Actor == 1).Coverage & MeleeCoverage.Windup);
        Assert.Equal(MeleeCoverage.None, found.Single(w => w.Actor == 3).Coverage & (MeleeCoverage.Windup | MeleeCoverage.Impact));
        Assert.All(found, w => Assert.Equal(MeleeCoverage.None, w.Coverage & MeleeCoverage.Simultaneous));
        MeleeWitness[] opaque = MeleeVisualProof.Inspect(frame with { Strikes = frame.Strikes.Select(s => s with { CueStyle = "opaque-beam" }).ToArray() }, Board);
        Assert.All(opaque, w => Assert.Equal(MeleeCoverage.None, w.Coverage & (MeleeCoverage.Windup | MeleeCoverage.Impact)));
        Runner.RenderedContact(frame with { Strikes = [frame.Strikes[0]] });
        Assert.Throws<InvalidOperationException>(() => Runner.RenderedContact(frame with { Strikes = [frame.Strikes[0] with { StrikeRadius = 3 }] }));
    }
    // Deterministic admission controls, not renderer/transport or PNG acceptance.
    // The delayed schedules below are controlled inputs, not reconstructed wall
    // timestamps: the retained compact protocol logs contain no arrival clock.
    private static Match PreparedAdmissionControl()
    {
        var match = new Match(combatSeed: 1); match.Join(); match.Join(); Act(match, 1, "start");
        Act(match, 1, "build", 0, Building.Farm); Act(match, 1, "build", 2, Building.MetalMine); Act(match, 1, "build", 1, Building.Barracks);
        for (int production = 1; production <= 3; production++)
        {
            Act(match, 1, "ready"); Act(match, 2, "ready");
            while (match.Players[1].Soldiers.Count < 6 && match.Players[1].Resources.TryPay(match.Economy.Recruitment(UnitType.Swordsman), out _)) Act(match, 1, "recruit", 1);
        }
        Assert.Equal(Phase.Preparation, match.Phase); Assert.Equal(0, match.Tick);
        return match;
    }
    private static void AdmitCombat(Match match) { Act(match, 1, "ready"); Act(match, 2, "ready"); }

    [Fact]
    public void PreReadyRegistrationAndSerializedLateRegistrationHaveDifferentLiveWindows()
    {
        using var match = PreparedAdmissionControl();
        long registeredRevision = match.Revision;
        AdmitCombat(match);
        long early = -1, late = -1;
        // Model 50ms publication at fixed tick granularity, not real ENet timing.
        // 638 is a controlled 10.63s speed-1 delay, not the retained observer's
        // unknown exact revision when the current fixture finally registered.
        while (match.Phase == Phase.Combat)
        {
            match.Step();
            if (match.Tick % 4 != 0) continue;
            MatchSnapshot state = match.Snapshot();
            if (state.Revision <= registeredRevision || !MeleeVisualProof.HasMilestone(state, MeleeCoverage.Windup, Board)) continue;
            if (early < 0) early = state.Tick;
            if (match.Tick >= 638 && late < 0) late = state.Tick;
        }
        output.WriteLine($"Controlled registration: pre-Ready first published windup={early}; after tick638={late}; terminal={match.Phase}/{match.Tick}.");
        Assert.InRange(early, 301, 307); Assert.Equal(-1, late);
        Assert.Equal(Phase.Defeat, match.Phase); Assert.Equal(973, match.Tick);
    }
    [Fact]
    public void EarlyLivePausePreservesMilestoneAcrossDelayedRenderingAndOrderedPersistence()
    {
        using var match = PreparedAdmissionControl();
        AdmitCombat(match);
        while (!MeleeVisualProof.HasMilestone(match.Snapshot(), MeleeCoverage.Windup, Board)) match.Step();
        Act(match, 2, "pause"); MatchSnapshot paused = match.Snapshot();
        var playback = new CombatPlayback(); playback.Accept(paused);
        var owner = new Game.OwnedFrameCapture(); byte[] pixels = new byte[16];
        var first = owner.Acquire("overview", "overview.png", 2, 2, pixels, $"revision={paused.Revision};tick={paused.Tick};view=overview");
        var second = owner.Acquire("close", "close.png", 2, 2, pixels, $"revision={paused.Revision};tick={paused.Tick};view=close");
        for (int delayed = 0; delayed < 1200; delayed++) { match.Step(); playback.Advance(.1, true); }
        Assert.Equal(paused.Tick, match.Tick); Assert.Equal(paused.Tick, playback.Tick);
        Assert.True(MeleeVisualProof.HasMilestone(match.Snapshot(), MeleeCoverage.Windup, Board));
        Assert.Throws<InvalidOperationException>(() => owner.Persist(second.Id, (_, p) => p));
        Assert.Equal(first, owner.Persist(first.Id, (_, p) => p));
        Assert.Equal(second, owner.Persist(second.Id, (_, p) => p));
        Assert.Equal(0, owner.Pending);
        // This does not prove the live progression can still meet 12 seconds:
        // early pause can freeze its current action before progression completes.
    }
    [Fact]
    public void PresentationUsesNewestAcceptedStateRatherThanDrainingAStateQueue()
    {
        using var match = PreparedAdmissionControl(); AdmitCombat(match);
        var playback = new CombatPlayback(); playback.Accept(match.Snapshot());
        MatchSnapshot? older = null, newest = null;
        for (int step = 1; step <= 440; step++)
        {
            match.Step();
            if (step % 4 != 0) continue;
            older = newest; newest = match.Snapshot();
            // Delayed rendering: all arrived revisions can be accepted without
            // advancing a graphical frame. Combat events are NOT discarded.
            Assert.True(playback.Accept(newest));
        }
        Assert.InRange(playback.Tick, newest!.Tick - 3, newest.Tick);
        Assert.False(playback.Accept(older!));
        playback.Advance(.1, true); Assert.Equal(newest.Tick, playback.Tick);
        Assert.NotEmpty(playback.Drain());
        Assert.All(playback.Units(), u => Assert.Contains(u.Id, CombatPlayback.All(newest).Concat(newest.DyingBodies).Select(v => v.Id)));
        output.WriteLine($"Controlled delayed presentation: latest received/applied={newest.Tick}; presented={playback.Tick}; older revision rejected.");
    }
    [Fact]
    public void HistoricalMilestoneDoesNotMakeAStalePauseActAtItsHistoricalTick()
    {
        using var match = PreparedAdmissionControl(); AdmitCombat(match);
        while (!MeleeVisualProof.HasMilestone(match.Snapshot(), MeleeCoverage.Windup, Board)) match.Step();
        MatchSnapshot milestone = match.Snapshot();
        var history = new ChildEvents(); history.Add(new("snapshot", State: milestone), 100);
        while (match.Tick < 638) match.Step();
        MatchSnapshot current = match.Snapshot(); history.Add(new("snapshot", State: current), 100);
        GameEvent? historical = history.Find(e => e.State is { } s && MeleeVisualProof.HasMilestone(s, MeleeCoverage.Windup, Board));
        Assert.Equal(milestone, historical!.State);
        Assert.False(MeleeVisualProof.HasMilestone(current, MeleeCoverage.Windup, Board));
        // Pause quotes validate match/phase/turn, NOT tick/revision. Thus a
        // stale same-turn quote can succeed, but freezes current authority.
        Assert.True(match.Apply(2, Command.FromSnapshot(milestone, 100, "pause", 2)).Accepted);
        Assert.Equal(current.Tick, match.Tick); Assert.NotEqual(milestone.Tick, match.Tick);
        Assert.True(match.Paused); Assert.False(MeleeVisualProof.HasMilestone(match.Snapshot(), MeleeCoverage.Windup, Board));
        output.WriteLine($"Controlled stale pause: historical={milestone.Tick}; actual authority pause={match.Tick} (not a milestone capture).");
    }
    [Fact]
    public void CurrentAuthorityTerminalStateRejectsBothHistoricalAndFreshPauseQuotes()
    {
        using var match = PreparedAdmissionControl(); AdmitCombat(match);
        MatchSnapshot stale = match.Snapshot();
        while (match.Phase == Phase.Combat) match.Step();
        MatchSnapshot terminal = match.Snapshot(); Assert.Equal(Phase.Defeat, terminal.Phase);
        CommandResult old = match.Apply(2, Command.FromSnapshot(stale, 100, "pause", 2));
        CommandResult fresh = match.Apply(2, Command.FromSnapshot(terminal, 101, "pause", 2));
        Assert.False(old.Accepted); Assert.Contains("Stale", old.Message);
        Assert.False(fresh.Accepted); Assert.Contains("finished", fresh.Message);
        Assert.False(match.Paused); Assert.Equal(terminal.Tick, match.Tick);
    }

    [Fact]
    public void EarlyObserverCannotPauseUntilActualProgressionHandoff()
    {
        using var match = PreparedAdmissionControl();
        var admission = new MeleeAdmission(match.Snapshot()); AdmitCombat(match); admission.Arm(match.Snapshot());
        while (!MeleeVisualProof.HasMilestone(match.Snapshot(), MeleeCoverage.Windup, Board)) match.Step();
        MatchSnapshot live = match.Snapshot(); admission.Witness(live, Board);
        Assert.True(admission.LiveWitnesses > 0); Assert.False(match.Paused);
        Assert.False(admission.MayPause(live, live, MeleeCoverage.Windup, Board));
        admission.Handoff(TimeSpan.FromSeconds(11.9), 12, 4);
        Assert.True(admission.MayPause(live, live, MeleeCoverage.Windup, Board));
        Act(match, 2, "pause"); MatchSnapshot paused = match.Snapshot();
        MeleeAdmission.FrozenFrame(Frame(paused) with { Revision = paused.Revision, PhaseText = "Combat PAUSED" }, paused);
    }
    [Fact]
    public void DelayedPeerRenderAndPersistenceCannotAuthorizeHistoricalOrTerminalPause()
    {
        using var match = PreparedAdmissionControl();
        var admission = new MeleeAdmission(match.Snapshot()); AdmitCombat(match); admission.Arm(match.Snapshot());
        while (!MeleeVisualProof.HasMilestone(match.Snapshot(), MeleeCoverage.Windup, Board)) match.Step();
        MatchSnapshot historical = match.Snapshot(); admission.Witness(historical, Board);
        var pixels = new Game.OwnedFrameCapture();
        for (int index = 0; index < 4; index++) pixels.Acquire("progression-" + index, index + ".png", 2, 2, new byte[16], "acquired immutable progression metadata");
        admission.Handoff(TimeSpan.FromSeconds(11), 12, 4);
        // Ordinary authority continues while peer/render and ordered I/O lag.
        while (match.Tick < 638) match.Step();
        for (int index = 0; index < 4; index++) pixels.Persist("progression-" + index, (_, p) => p);
        Assert.False(admission.MayPause(historical, match.Snapshot(), MeleeCoverage.Windup, Board));
        Assert.False(admission.MayPause(historical with { MatchId = "other" }, historical, MeleeCoverage.Windup, Board));
        while (match.Phase == Phase.Combat) match.Step(); MatchSnapshot terminal = match.Snapshot();
        Assert.False(admission.MayPause(terminal, terminal, MeleeCoverage.Windup, Board));
        Assert.False(match.Paused); Assert.Equal(0, pixels.Pending);
    }
    [Fact]
    public void FrozenMilestoneRejectsStaleTickRevisionPhaseAndTerminalAuthority()
    {
        using var match = PreparedAdmissionControl(); AdmitCombat(match);
        while (!MeleeVisualProof.HasMilestone(match.Snapshot(), MeleeCoverage.Windup, Board)) match.Step();
        Act(match, 2, "pause"); MatchSnapshot paused = match.Snapshot();
        UiObservation frame = Frame(paused) with { Revision = paused.Revision, PhaseText = "Combat PAUSED" };
        MeleeAdmission.FrozenFrame(frame, paused);
        Assert.Throws<InvalidOperationException>(() => MeleeAdmission.FrozenFrame(frame with { CombatTick = frame.CombatTick - 1 }, paused));
        Assert.Throws<InvalidOperationException>(() => MeleeAdmission.FrozenFrame(frame with { Revision = paused.Revision - 1 }, paused));
        Assert.Throws<InvalidOperationException>(() => MeleeAdmission.FrozenFrame(frame with { PhaseText = "Combat" }, paused));
        Assert.Throws<InvalidOperationException>(() => MeleeAdmission.FrozenFrame(frame, paused with { Phase = Phase.Defeat }));
    }
    [Fact]
    public void ObserverRegistrationAndHandoffKeepTickZeroAndOriginalCountsBounds()
    {
        using var match = PreparedAdmissionControl(); MatchSnapshot preparation = match.Snapshot();
        Assert.Throws<InvalidOperationException>(() => new MeleeAdmission(preparation with { Tick = 1 }));
        var admission = new MeleeAdmission(preparation);
        Assert.Throws<InvalidOperationException>(() => admission.Arm(preparation));
        AdmitCombat(match); admission.Arm(match.Snapshot());
        Assert.Throws<InvalidOperationException>(() => admission.Handoff(TimeSpan.FromSeconds(12.001), 12, 4));
        Assert.Throws<InvalidOperationException>(() => admission.Handoff(TimeSpan.FromSeconds(11), 11, 4));
        Assert.Throws<InvalidOperationException>(() => admission.Handoff(TimeSpan.FromSeconds(11), 12, 3));
        admission.Handoff(TimeSpan.FromSeconds(11), 12, 4);
        Assert.Throws<InvalidOperationException>(() => admission.Handoff(TimeSpan.FromSeconds(11), 12, 4));
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    [InlineData(123UL)]
    public void OrdinarySixSwordOpeningGeneratesTheRequiredCheckpointOpportunities(ulong seed)
    {
        using var match = new Match(combatSeed: seed); match.Join(); match.Join(); Act(match, 1, "start");
        Act(match, 1, "build", 0, Building.Farm); Act(match, 1, "build", 2, Building.MetalMine); Act(match, 1, "build", 1, Building.Barracks);
        if (seed == 0)
        {
            Act(match, 2, "build", 0, Building.Farm); Act(match, 2, "build", 2, Building.MetalMine); Act(match, 2, "build", 1, Building.Barracks);
        }
        for (int production = 1; production <= 3; production++)
        {
            Act(match, 1, "ready"); Act(match, 2, "ready");
            while (match.Players[1].Soldiers.Count < 6 && match.Players[1].Resources.TryPay(match.Economy.Recruitment(UnitType.Swordsman), out _)) Act(match, 1, "recruit", 1);
            if (seed == 0) while (match.Players[2].Soldiers.Count < 6 && match.Players[2].Resources.TryPay(match.Economy.Recruitment(UnitType.Swordsman), out _)) Act(match, 2, "recruit", 1);
        }
        Assert.Equal(6, match.Players[1].Soldiers.Count); Assert.Equal(15, match.Players[1].Food); Assert.Equal(0, match.Players[1].Wood);
        Act(match, 1, "ready"); Act(match, 2, "ready");
        MeleeCoverage coverage = MeleeCoverage.None; var opportunities = new HashSet<(MeleeCoverage Phase, MeleeCoverage Side)>(); bool simultaneousCapture = false;
        MeleeCoverage capturedSide = MeleeCoverage.None; long windupTick = -1, impactTick = -1;
        for (int step = 1; step <= 3600 && match.Phase is Phase.Combat or Phase.Building or Phase.Preparation; step++)
        {
            if (match.Phase is Phase.Building or Phase.Preparation)
            {
                foreach (City city in match.Players.Values.Where(c => !c.Eliminated))
                {
                    if (city.Id == 1 || seed == 0) while (city.Soldiers.Count < 6 && city.Resources.TryPay(match.Economy.Recruitment(UnitType.Swordsman), out _)) Act(match, city.Id, "recruit", 1);
                    if (!city.Ready) Act(match, city.Id, "ready");
                }
                match.Step(); continue;
            }
            match.Step(); UiObservation frame = Frame(match.Snapshot());
            long tick = match.Tick;
            foreach (MeleeWitness witness in MeleeVisualProof.Inspect(frame, Board))
            {
                coverage |= witness.Coverage;
                if ((witness.Coverage & MeleeCoverage.Shared) == 0) continue;
                simultaneousCapture |= (witness.Coverage & (MeleeCoverage.Windup | MeleeCoverage.Simultaneous)) == (MeleeCoverage.Windup | MeleeCoverage.Simultaneous) && witness.ImpactTick - tick >= 6;
                if (capturedSide == MeleeCoverage.None && (witness.Coverage & (MeleeCoverage.Windup | MeleeCoverage.Simultaneous)) == (MeleeCoverage.Windup | MeleeCoverage.Simultaneous)
                    && witness.ImpactTick - tick >= 6 && (witness.Coverage & (MeleeCoverage.Near | MeleeCoverage.Far)) != 0)
                {
                    capturedSide = (witness.Coverage & MeleeCoverage.Near) != 0 ? MeleeCoverage.Near : MeleeCoverage.Far; windupTick = tick;
                }
                MeleeCoverage opposite = capturedSide == MeleeCoverage.Near ? MeleeCoverage.Far : MeleeCoverage.Near;
                if (windupTick >= 0 && tick > windupTick && impactTick < 0 && (witness.Coverage & (MeleeCoverage.Impact | opposite)) == (MeleeCoverage.Impact | opposite)) impactTick = tick;
                foreach (MeleeCoverage side in new[] { MeleeCoverage.Near, MeleeCoverage.Far })
                {
                    if ((witness.Coverage & side) == 0) continue;
                    if ((witness.Coverage & MeleeCoverage.Windup) != 0 && witness.ImpactTick - tick >= 6) opportunities.Add((MeleeCoverage.Windup, side));
                    if ((witness.Coverage & MeleeCoverage.Impact) != 0) opportunities.Add((MeleeCoverage.Impact, side));
                }
            }
            if (opportunities.Count == 4 && impactTick >= 0) break;
        }
        output.WriteLine($"Proof wave={match.Wave}, phase={match.Phase}, tick={match.Tick}; ordered capture windup={windupTick} ({capturedSide}), opposite impact={impactTick}.");
        output.WriteLine($"Melee setup seed={seed}: coverage={coverage}; capture opportunities={string.Join(';', opportunities)}.");
        const MeleeCoverage required = MeleeCoverage.Shared | MeleeCoverage.Near | MeleeCoverage.Far | MeleeCoverage.Simultaneous | MeleeCoverage.Windup | MeleeCoverage.Impact;
        Assert.Equal(required, coverage & required);
        Assert.True(simultaneousCapture, "The shared simultaneous windup has no capture allowance.");
        Assert.True(impactTick > windupTick && windupTick >= 0, "The first shared simultaneous windup needs a later opposite-side impact.");
        // The required graphical seed-1 proof stays within two waves. The
        // extra seed-0 detector sample reaches its opposite-side impact in wave three.
        Assert.InRange(match.Wave, 1, seed == 1 ? 2 : 3);
        Assert.Contains((MeleeCoverage.Windup, MeleeCoverage.Near), opportunities); Assert.Contains((MeleeCoverage.Windup, MeleeCoverage.Far), opportunities);
        Assert.Contains((MeleeCoverage.Impact, MeleeCoverage.Near), opportunities); Assert.Contains((MeleeCoverage.Impact, MeleeCoverage.Far), opportunities);
    }
}
