using DevRunner;
using Game.Core;
using Xunit;

namespace DevRunner.Tests;

public sealed class CasualtyAdmissionTests
{
    private static readonly string[] IdentityCallsites = ["loop-entry", "accepted-receipt", "after-owned-resume", "frozen-current"];
    private static readonly string[] IdentityFaults = ["match", "configuration", "phase", "turn", "wave"];
    private static readonly string[] OwnPauseAndResume = ["pause", "resume"];
    private static readonly string[] OnePause = ["pause"];
    private static readonly string[] RearmedPause = ["pause", "resume", "pause"];
    private static readonly string[] CappedPauses = ["pause", "resume", "pause", "resume", "pause", "resume"];
    private static MatchSnapshot Eligible(long revision = 918, long tick = 824, bool paused = false)
    {
        var body = new UnitState(41, 0) { Destination = 1, Hex = new(41, 1, Faction.Skeletons, UnitLifecycle.Dying, new(11, 1), DeathStartTick: 818, DeathEndTick: 866) };
        var enemy = new UnitState(44, 2000) { Destination = 1, Faction = Faction.Skeletons, Profile = new(3000, 1000, 1, 30, 12, 60) };
        return new("casualty", revision, tick, Phase.Combat, paused, 3, 3, 12, new(), [], [enemy]) { DyingBodies = [body] };
    }

    private static MatchSnapshot Ineligible(MatchSnapshot state) => state with
    {
        Enemies = [state.Enemies[0] with { Health = 3000 }, state.Enemies[0] with { Id = 46, Destination = 2, Health = 600 }]
    };

    private sealed class Driver
    {
        internal MatchSnapshot Current = Eligible(917, 823, true);
        internal readonly Queue<MatchSnapshot> Arrivals = new();
        internal readonly Queue<GameEvent> Receipts = new();
        internal readonly List<string> Commands = [];
        internal readonly List<long> Floors = [];
        internal Func<CancellationToken, Task>? WaitOverride;
        internal Func<string, GameEvent, GameEvent>? AfterCommand;
        internal Task<MatchSnapshot> Run(CancellationToken token = default) => CasualtyAdmission.Pause(Current, 1, 800, 10,
            () => Current, async (floor, cancellation) =>
            {
                Floors.Add(floor);
                cancellation.ThrowIfCancellationRequested();
                if (WaitOverride is not null) await WaitOverride(cancellation);
                else Current = Arrivals.Dequeue();
            }, (command, cancellation) =>
            {
                cancellation.ThrowIfCancellationRequested();
                Commands.Add(command);
                GameEvent receipt = Receipts.Dequeue();
                if (receipt.State is { } state) Current = state;
                return Task.FromResult(AfterCommand?.Invoke(command, receipt) ?? receipt);
            }, token);
    }

    private static GameEvent Ack(MatchSnapshot state, long sequence = 11, bool accepted = true)
        => new("ack", State: state, Result: new(sequence, accepted, "control"));

    [Fact]
    public async Task HistoricalWitnessWakesButLatestIneligibleStateCannotRequestPause()
    {
        var driver = new Driver();
        // This is the old caller's red control: retained 918 is eligible, but
        // current 922 (the actual failed pause's facts) is not.
        Assert.True(Runner.CasualtyInspectionReady(Eligible(), 1, 800));
        driver.Arrivals.Enqueue(Ineligible(Eligible(922, 827)));
        driver.Arrivals.Enqueue(Eligible(923, 828));
        driver.Receipts.Enqueue(Ack(Eligible(924, 828, true)));
        MatchSnapshot frozen = await driver.Run();
        Assert.Equal(828, frozen.Tick);
        Assert.Equal(new long[] { 917, 922 }, driver.Floors);
        Assert.Equal(OnePause, driver.Commands);
    }

    [Fact]
    public async Task AgedHistoricalBodyCannotAuthorizeLatestPause()
    {
        var driver = new Driver();
        Assert.True(Runner.CasualtyInspectionReady(Eligible(), 1, 800));
        driver.Arrivals.Enqueue(Eligible(922, 836));
        MatchSnapshot fresh = FreshAfterAging();
        driver.Arrivals.Enqueue(fresh);
        driver.Receipts.Enqueue(Ack(fresh with { Revision = 925, Paused = true }));
        MatchSnapshot frozen = await driver.Run();
        Assert.Equal(841, frozen.Tick);
        Assert.Equal(new long[] { 917, 922 }, driver.Floors);
        Assert.Equal(OnePause, driver.Commands);
    }

    private static MatchSnapshot FreshAfterAging()
    {
        MatchSnapshot state = Eligible(924, 841);
        UnitState body = state.DyingBodies[0];
        return state with { DyingBodies = [body with { Hex = body.Hex! with { DeathStartTick = 838, DeathEndTick = 886 } }] };
    }

    [Fact]
    public async Task RequestToReceiptAgingResumesAndWaitsForANewFreshBody()
    {
        var driver = new Driver();
        driver.Arrivals.Enqueue(Eligible());
        MatchSnapshot aged = Eligible(922, 836, true);
        Assert.False(Runner.CasualtyInspectionReady(aged, 1, 800));
        driver.Receipts.Enqueue(Ack(aged));
        driver.Receipts.Enqueue(Ack(aged with { Revision = 923, Paused = false }, 12));
        MatchSnapshot fresh = FreshAfterAging();
        driver.Arrivals.Enqueue(fresh);
        driver.Receipts.Enqueue(Ack(fresh with { Revision = 925, Paused = true }, 13));
        MatchSnapshot frozen = await driver.Run();
        Assert.Equal(841, frozen.Tick);
        Assert.Equal(RearmedPause, driver.Commands);
        Assert.Equal(new long[] { 917, 923 }, driver.Floors);
    }

    [Fact]
    public void EveryAuthoredRigRejectsAgedBodyDespiteRemainingLifetime()
    {
        foreach (Faction faction in Enum.GetValues<Faction>())
            foreach (UnitType type in Enum.GetValues<UnitType>())
            {
                MatchSnapshot state = Eligible(tick: 836);
                UnitState body = state.DyingBodies[0] with { Type = type, Faction = faction };
                state = state with { DyingBodies = [body] };
                Assert.True(body.Hex!.DeathStartTick > 800 && body.Hex.DeathEndTick > state.Tick + 12);
                Assert.False(Runner.CasualtyInspectionReady(state, 1, 800));
                Assert.False(Runner.CasualtyInspectionReady(state with { Tick = 832 }, 1, 800));
                Assert.True(Runner.CasualtyInspectionReady(state with { Tick = 831 }, 1, 800));
            }
    }

    [Fact]
    public void FreshPoseBoundaryRemainsStrict()
    {
        Assert.True(Runner.FreshDeathPose(Math.BitDecrement(.35)));
        Assert.False(Runner.FreshDeathPose(.35));
        Assert.False(Runner.FreshDeathPose(Math.BitIncrement(.35)));
    }

    [Fact]
    public void BodySelectionAndFrameRejectNewerButAgedBodyWhenAnotherBodyIsEligible()
    {
        MatchSnapshot state = Eligible(922, 836, true);
        UnitState body = state.DyingBodies[0];
        UnitState fresh = body with { Hex = body.Hex! with { DeathEndTick = 918 } };
        UnitState aged = body with { Id = 42, Hex = body.Hex! with { Id = 42, DeathStartTick = 822, DeathEndTick = 870 } };
        state = state with { DyingBodies = [fresh, aged] };
        UnitState selected = Runner.EligibleCasualties(state, 1, 800).MaxBy(u => u.Hex!.DeathStartTick)!;
        Assert.Equal(41, selected.Id);
        UiObservation frame = Frame() with { CombatTick = 836 };
        CasualtyAdmission.Frame(state, state, frame, 1, 800, selected.Id);
        frame = frame with { Units = [frame.Units[0] with { Id = 42 }, frame.Units[1]] };
        Assert.Throws<InvalidOperationException>(() => CasualtyAdmission.Frame(state, state, frame, 1, 800, 42));
    }

    [Theory]
    [InlineData("full")]
    [InlineData("foreign")]
    [InlineData("dead")]
    [InlineData("expired")]
    public async Task EligibilityLostAtActualReceiptResumesOwnPauseAndRearmsFreshFloor(string loss)
    {
        var driver = new Driver();
        driver.Arrivals.Enqueue(Eligible());
        MatchSnapshot bad = Eligible(922, 827, true);
        bad = loss switch
        {
            "full" => bad with { Enemies = [bad.Enemies[0] with { Health = 3000 }] },
            "foreign" => Ineligible(bad),
            "dead" => bad with { Enemies = [bad.Enemies[0] with { Health = 0 }] },
            _ => bad with { DyingBodies = [] }
        };
        Assert.False(Runner.CasualtyInspectionReady(bad, 1, 800));
        driver.Receipts.Enqueue(Ack(bad));
        driver.Receipts.Enqueue(Ack(bad with { Revision = 923, Paused = false }, 12));
        driver.Arrivals.Enqueue(Eligible(924, 828));
        driver.Receipts.Enqueue(Ack(Eligible(925, 828, true), 13));
        MatchSnapshot frozen = await driver.Run();
        Assert.Equal(925, frozen.Revision);
        Assert.Equal(RearmedPause, driver.Commands);
        Assert.Equal(new long[] { 917, 923 }, driver.Floors);
    }

    [Theory]
    [InlineData("rejected")]
    [InlineData("unreadable")]
    [InlineData("sequence")]
    [InlineData("revision")]
    [InlineData("match")]
    [InlineData("turn")]
    [InlineData("terminal")]
    [InlineData("unpaused")]
    public async Task InvalidReceiptCannotAuthorizeCaptureOrUndoAnyPause(string fault)
    {
        var driver = new Driver();
        driver.Arrivals.Enqueue(Eligible());
        GameEvent receipt = Ack(Eligible(922, 827, true));
        receipt = fault switch
        {
            "rejected" => receipt with { Result = new(11, false, "stale quote") },
            "unreadable" => receipt with { State = null },
            "sequence" => receipt with { Result = new(10, true, "duplicate") },
            "revision" => receipt with { State = Eligible(916, 822, true) },
            "match" => receipt with { State = receipt.State! with { MatchId = "replacement" } },
            "turn" => receipt with { State = receipt.State! with { TurnSerial = 13 } },
            "terminal" => receipt with { State = receipt.State! with { Phase = Phase.Defeat } },
            _ => receipt with { State = receipt.State! with { Paused = false } }
        };
        driver.Receipts.Enqueue(receipt);
        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => driver.Run());
        Assert.Equal(fault is "match" or "turn" or "terminal", error.Data[CasualtyAdmission.IdentityEvidenceKey] is CasualtyIdentityRejection);
        Assert.Equal(OnePause, driver.Commands);
    }

    [Fact]
    public async Task NeverUndoesExistingPauseOrAddsCompetingWaiterOnCancellation()
    {
        var driver = new Driver();
        using var cancellation = new CancellationTokenSource();
        driver.WaitOverride = async token => { await cancellation.CancelAsync(); token.ThrowIfCancellationRequested(); };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => driver.Run(cancellation.Token));
        Assert.Empty(driver.Commands);
        Assert.Single(driver.Floors);
    }

    [Fact]
    public async Task CancellationBetweenRequestAndReceiptCannotProceedOrSendAnUnownedResume()
    {
        var driver = new Driver();
        using var cancellation = new CancellationTokenSource();
        driver.Arrivals.Enqueue(Eligible());
        driver.Receipts.Enqueue(Ack(Ineligible(Eligible(922, 827, true))));
        driver.AfterCommand = (_, receipt) => { cancellation.Cancel(); return receipt; };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => driver.Run(cancellation.Token));
        Assert.Equal(OnePause, driver.Commands);
    }

    [Fact]
    public async Task AttemptCapResumesLastOwnedPauseBeforeFailingWithoutAnotherWait()
    {
        var driver = new Driver();
        for (int i = 0; i < CasualtyAdmission.MaximumAttempts; i++)
        {
            driver.Arrivals.Enqueue(Eligible(918 + i * 3));
            driver.Receipts.Enqueue(Ack(Ineligible(Eligible(919 + i * 3, 824, true)), 11 + i * 2));
            driver.Receipts.Enqueue(Ack(Eligible(920 + i * 3), 12 + i * 2));
        }
        await Assert.ThrowsAsync<InvalidOperationException>(() => driver.Run());
        Assert.Equal(CappedPauses, driver.Commands);
        Assert.Equal(CasualtyAdmission.MaximumAttempts, driver.Floors.Count);
        Assert.False(driver.Current.Paused);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TerminalOrReplacementCurrentStateCannotUseEarlierEligibleQuote(bool replacement)
    {
        var driver = new Driver();
        MatchSnapshot current = Eligible();
        driver.Arrivals.Enqueue(replacement ? current with { MatchId = "replacement" } : current with { Phase = Phase.Defeat });
        await Assert.ThrowsAsync<InvalidOperationException>(() => driver.Run());
        Assert.Empty(driver.Commands);
    }

    [Fact]
    public async Task AcceptedReceiptCannotUndoPauseWhenCurrentOwnershipWasLost()
    {
        var driver = new Driver();
        driver.Arrivals.Enqueue(Eligible());
        driver.Receipts.Enqueue(Ack(Ineligible(Eligible(922, 827, true))));
        driver.AfterCommand = (_, receipt) =>
        {
            driver.Current = receipt.State! with { MatchId = "replacement" };
            return receipt;
        };
        await Assert.ThrowsAsync<InvalidOperationException>(() => driver.Run());
        Assert.Equal(OnePause, driver.Commands);
    }

    public static IEnumerable<object[]> IdentityFailures() =>
        from callsite in IdentityCallsites
        from fault in IdentityFaults
        select new object[] { callsite, fault };

    private static MatchSnapshot ChangeIdentity(MatchSnapshot state, string fault) => fault switch
    {
        "match" => state with { MatchId = "replacement" },
        "configuration" => state with { ConfigurationFingerprint = new(1, 2, 3, 4) },
        "phase" => state with { Phase = Phase.Building },
        "turn" => state with { TurnSerial = state.TurnSerial + 1 },
        "wave" => state with { Wave = state.Wave + 1 },
        _ => throw new ArgumentException("Unknown fault.", nameof(fault))
    };

    [Theory]
    [MemberData(nameof(IdentityFailures))]
    public async Task IdentityRejectionReportsExactDisjunctAndCallsiteWithoutChangingCommandOwnership(string callsite, string fault)
    {
        var driver = new Driver();
        MatchSnapshot origin = driver.Current;
        driver.Arrivals.Enqueue(callsite == "loop-entry" ? ChangeIdentity(Eligible(), fault) : Eligible());
        MatchSnapshot pause = callsite == "after-owned-resume" ? Ineligible(Eligible(922, 827, true)) : Eligible(922, 827, true);
        driver.Receipts.Enqueue(Ack(callsite == "accepted-receipt" ? ChangeIdentity(pause, fault) : pause));
        driver.Receipts.Enqueue(Ack(pause with { Revision = 923, Paused = false }, 12));
        driver.AfterCommand = (command, receipt) =>
        {
            if (callsite == "frozen-current" && command == "pause" || callsite == "after-owned-resume" && command == "resume")
                driver.Current = ChangeIdentity(receipt.State!, fault);
            return receipt;
        };
        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => driver.Run());
        Assert.Equal("Casualty admission lost its current combat/session identity.", error.Message);
        var rejection = Assert.IsType<CasualtyIdentityRejection>(error.Data[CasualtyAdmission.IdentityEvidenceKey]);
        Assert.Equal(callsite, rejection.Callsite);
        Assert.Equal(new CasualtyIdentityDisjuncts(fault == "match", fault == "configuration", fault == "phase", fault == "turn", fault == "wave"), rejection.Failed);
        Assert.Equal(CasualtyIdentityState.From(callsite == "frozen-current" ? pause : origin), rejection.Origin);
        Assert.Equal(CasualtyIdentityState.From(driver.Current), rejection.Current);
        Assert.Same(driver.Current, error.Data[CasualtyAdmission.IdentityStateKey]);
        Assert.True(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(rejection).Length < 8192);
        if (callsite == "loop-entry")
        {
            Assert.Empty(driver.Commands);
            Assert.Null(rejection.ReceiptType);
            Assert.Null(rejection.ReceiptSequence);
            Assert.Null(rejection.ReceiptAccepted);
        }
        else
        {
            Assert.Equal(callsite == "after-owned-resume" ? OwnPauseAndResume : OnePause, driver.Commands);
            Assert.Equal("ack", rejection.ReceiptType);
            Assert.Equal(callsite == "after-owned-resume" ? 12 : 11, rejection.ReceiptSequence);
            Assert.True(rejection.ReceiptAccepted);
            Assert.Equal(0, rejection.ReceiptPeerId);
            Assert.Equal(0, rejection.ReceiptPlayerId);
        }
    }

    [Fact]
    public async Task RejectionReportsAllChangedFieldsWithoutCallingPhaseChangeASessionMismatch()
    {
        var driver = new Driver();
        MatchSnapshot changed = Eligible() with { MatchId = "replacement", ConfigurationFingerprint = new(1, 2, 3, 4), Phase = Phase.Defeat, TurnSerial = 13, Wave = 4 };
        driver.Arrivals.Enqueue(changed);
        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => driver.Run());
        var rejection = Assert.IsType<CasualtyIdentityRejection>(error.Data[CasualtyAdmission.IdentityEvidenceKey]);
        Assert.Equal(new CasualtyIdentityDisjuncts(true, true, true, true, true), rejection.Failed);
        Assert.Equal(Phase.Defeat, rejection.Current.Phase);
        Assert.Empty(driver.Commands);
    }

    [Fact]
    public void CityAndActorChangesAreNotIdentityDisjunctsButFrozenOwnershipStillApplies()
    {
        MatchSnapshot receipt = Eligible(paused: true);
        MatchSnapshot current = receipt with { Enemies = [receipt.Enemies[0] with { Destination = 2, Health = 600 }], DyingBodies = [] };
        CasualtyAdmission.Frozen(receipt, current);
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => CasualtyAdmission.Frozen(receipt, current with { Tick = current.Tick + 1 }));
        Assert.Equal("Casualty pause no longer belongs to the accepted frozen receipt.", error.Message);
        Assert.Null(error.Data[CasualtyAdmission.IdentityEvidenceKey]);
    }

    private static UiObservation Frame() => new()
    {
        Revision = 922,
        CombatTick = 827,
        ObservedCity = 1,
        PhaseText = "Combat PAUSED",
        Units = [new() { Id = 41, Destination = 1, Dead = true, Visible = true, PoseSeconds = .15 },
            new() { Id = 44, Destination = 1, Faction = Faction.Skeletons, Health = 2000, MaximumHealth = 3000, Visible = true }]
    };

    [Theory]
    [InlineData("valid")]
    [InlineData("hidden")]
    [InlineData("dead")]
    [InlineData("full")]
    [InlineData("foreign")]
    [InlineData("tick")]
    [InlineData("revision")]
    [InlineData("body")]
    [InlineData("aged")]
    [InlineData("boundary")]
    [InlineData("hidden-body")]
    [InlineData("foreign-body")]
    public void ActualFrozenRenderedFrameMustContainSameFreshBodyAndFocusedDamagedTarget(string fault)
    {
        MatchSnapshot receipt = Eligible(922, 827, true);
        UiObservation frame = Frame();
        UnitObservation target = frame.Units[1];
        frame = fault switch
        {
            "hidden" => frame with { Units = [frame.Units[0], target with { Visible = false }] },
            "dead" => frame with { Units = [frame.Units[0], target with { Dead = true }] },
            "full" => frame with { Units = [frame.Units[0], target with { Health = 3000 }] },
            "foreign" => frame with { Units = [frame.Units[0], target with { Destination = 2 }] },
            "tick" => frame with { CombatTick = 828 },
            "revision" => frame with { Revision = 918 },
            "body" => frame with { Units = [target] },
            "aged" => frame with { Units = [frame.Units[0] with { PoseSeconds = .4844 }, target] },
            "boundary" => frame with { Units = [frame.Units[0] with { PoseSeconds = .35 }, target] },
            "hidden-body" => frame with { Units = [frame.Units[0] with { Visible = false }, target] },
            "foreign-body" => frame with { Units = [frame.Units[0] with { Destination = 2 }, target] },
            _ => frame
        };
        if (fault == "valid") CasualtyAdmission.Frame(receipt, receipt, frame, 1, 800, 41);
        else Assert.Throws<InvalidOperationException>(() => CasualtyAdmission.Frame(receipt, receipt, frame, 1, 800, 41));
    }
}
