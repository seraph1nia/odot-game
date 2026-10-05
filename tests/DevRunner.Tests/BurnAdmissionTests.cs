using DevRunner;
using Game.Core;
using Xunit;

namespace DevRunner.Tests;

public sealed class BurnAdmissionTests
{
    private static readonly string[] OnePause = ["pause"];
    private static readonly string[] RearmedPause = ["pause", "resume", "pause"];
    private static readonly string[] CappedPauses = ["pause", "resume", "pause", "resume", "pause", "resume"];
    private static readonly string[] ResumedPause = ["pause", "resume"];
    private static MatchSnapshot Burning(long revision = 20, long tick = 100, bool paused = false)
    {
        StatusState burn = StatusPolicy.Apply(new(), new(139, StatusKind.Burn, 132, 3, 100, 252), new());
        var enemy = new UnitState(139, 840, Origin: 1, Destination: 1) { Statuses = burn };
        return new("research", revision, tick, Phase.Combat, paused, 8, 3, 40, new(), [], [enemy]);
    }

    private static GameEvent Ack(MatchSnapshot state, long sequence = 11)
        => new("ack", State: state, Result: new(sequence, true, "ordinary control"));

    private sealed class Driver
    {
        internal MatchSnapshot Current = Burning();
        internal readonly Queue<MatchSnapshot> Arrivals = new();
        internal readonly Queue<GameEvent> Receipts = new();
        internal readonly List<string> Commands = [];
        internal readonly List<long> Floors = [];
        internal Func<CancellationToken, Task>? WaitOverride;
        internal Action? AfterCommand;
        internal MatchSnapshot? AfterReceipt;
        internal Task<(GameEvent Receipt, int TargetId)> Run(CancellationToken token = default) => BurnAdmission.Pause(Current, 1, 10,
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
                if (receipt.State is { } state) Current = AfterReceipt ?? state;
                AfterCommand?.Invoke();
                return Task.FromResult(receipt);
            }, token);
    }

    [Theory]
    [InlineData("death")]
    [InlineData("expiry")]
    [InlineData("replacement")]
    [InlineData("effect")]
    public async Task ObservedBurnLostDuringRequestMustResumeOwnIneligibleReceiptAndRearm(string loss)
    {
        var driver = new Driver();
        MatchSnapshot bad = Burning(21, 101, true);
        bad = loss switch
        {
            "death" => bad with { Enemies = [bad.Enemies[0] with { Health = 0 }] },
            "expiry" => bad with { Tick = 280, Enemies = [bad.Enemies[0] with { Statuses = StatusPolicy.Advance(bad.Enemies[0].Statuses, 280, new()).State }] },
            "replacement" => bad with { Enemies = [bad.Enemies[0] with { Id = 140 }] },
            _ => bad with { Enemies = [bad.Enemies[0] with { Statuses = StatusPolicy.Apply(new(), new(139, StatusKind.Burn, 132, 4, 101, 252), new()) }] }
        };
        // A different live burn must not stand in for the observed target/effect.
        bad = bad with { Enemies = [.. bad.Enemies, Burning().Enemies[0] with { Id = 999 }] };
        driver.Receipts.Enqueue(Ack(bad));
        MatchSnapshot running = bad with { Revision = 22, Paused = false };
        driver.Receipts.Enqueue(Ack(running, 12));
        MatchSnapshot fresh = Burning(23, 281) with
        {
            Enemies = [Burning().Enemies[0] with { Statuses = StatusPolicy.Apply(new(), new(139, StatusKind.Burn, 132, 4, 281, 252), new()) }]
        };
        driver.Arrivals.Enqueue(fresh);
        GameEvent accepted = Ack(fresh with { Revision = 24, Paused = true }, 13);
        driver.Receipts.Enqueue(accepted);
        var result = await driver.Run();
        Assert.Same(accepted, result.Receipt);
        Assert.Equal(139, result.TargetId);
        Assert.Equal(RearmedPause, driver.Commands);
        Assert.Equal(new long[] { 22 }, driver.Floors);
    }

    [Fact]
    public async Task ValidCurrentFrozenReceiptRetainsExactRequestIdentityAndDeadlines()
    {
        var driver = new Driver();
        GameEvent accepted = Ack(Burning(21, 101, true));
        driver.Receipts.Enqueue(accepted);
        var result = await driver.Run();
        Assert.Same(accepted, result.Receipt);
        Assert.Equal(11, result.Receipt.Result!.Sequence);
        Assert.Equal(Burning().Enemies[0].Statuses.Burn, result.Receipt.State!.Enemies[0].Statuses.Burn);
        Assert.Equal(OnePause, driver.Commands);
        Assert.Empty(driver.Floors);
    }

    [Theory]
    [InlineData("rejected")]
    [InlineData("unreadable")]
    [InlineData("sequence")]
    [InlineData("revision")]
    [InlineData("tick")]
    [InlineData("match")]
    [InlineData("configuration")]
    [InlineData("turn")]
    [InlineData("wave")]
    [InlineData("terminal")]
    [InlineData("clear")]
    [InlineData("unpaused")]
    [InlineData("type")]
    public async Task InvalidReceiptCannotAuthorizeCaptureOrResume(string fault)
    {
        var driver = new Driver();
        GameEvent receipt = Ack(Burning(21, 101, true));
        receipt = fault switch
        {
            "rejected" => receipt with { Result = new(11, false, "rejected") },
            "unreadable" => receipt with { State = null },
            "sequence" => receipt with { Result = new(10, true, "duplicate") },
            "revision" => receipt with { State = receipt.State! with { Revision = 19 } },
            "tick" => receipt with { State = receipt.State! with { Tick = 99 } },
            "match" => receipt with { State = receipt.State! with { MatchId = "reset" } },
            "configuration" => receipt with { State = receipt.State! with { ConfigurationFingerprint = new(1, 2, 3, 4) } },
            "turn" => receipt with { State = receipt.State! with { TurnSerial = 41 } },
            "wave" => receipt with { State = receipt.State! with { Wave = 9 } },
            "terminal" => receipt with { State = receipt.State! with { Phase = Phase.Defeat } },
            "clear" => receipt with { State = receipt.State! with { Phase = Phase.Building } },
            "type" => receipt with { Type = "snapshot" },
            _ => receipt with { State = receipt.State! with { Paused = false } }
        };
        driver.Receipts.Enqueue(receipt);
        await Assert.ThrowsAsync<InvalidOperationException>(() => driver.Run());
        Assert.Equal(OnePause, driver.Commands);
    }

    [Theory]
    [InlineData("resume")]
    [InlineData("tick")]
    [InlineData("session")]
    [InlineData("effect")]
    public async Task LostCurrentFrozenOwnershipCannotCaptureOrUndoPause(string loss)
    {
        var driver = new Driver();
        MatchSnapshot frozen = Burning(21, 101, true);
        driver.Receipts.Enqueue(Ack(frozen));
        driver.AfterReceipt = loss switch
        {
            "resume" => frozen with { Revision = 22, Paused = false },
            "tick" => frozen with { Revision = 22, Tick = 102 },
            "session" => frozen with { MatchId = "reset" },
            _ => frozen with { Enemies = [frozen.Enemies[0] with { Statuses = StatusState.Empty }] }
        };
        await Assert.ThrowsAsync<InvalidOperationException>(() => driver.Run());
        Assert.Equal(OnePause, driver.Commands);
    }

    [Fact]
    public async Task ExistingPauseWaitsForFreshCurrentRevisionAndDoesNotUndoOtherOwner()
    {
        var driver = new Driver { Current = Burning(paused: true) };
        driver.Arrivals.Enqueue(Burning(21, 101));
        driver.Receipts.Enqueue(Ack(Burning(22, 101, true)));
        await driver.Run();
        Assert.Equal(new long[] { 20 }, driver.Floors);
        Assert.Equal(OnePause, driver.Commands);
    }

    [Fact]
    public async Task FiniteAttemptCapResumesLastOwnedIneligiblePauseBeforeFailure()
    {
        var driver = new Driver();
        for (int i = 0; i < BurnAdmission.MaximumAttempts; i++)
        {
            MatchSnapshot bad = Burning(21 + i * 3, 101 + i, true) with { Enemies = [] };
            driver.Receipts.Enqueue(Ack(bad, 11 + i * 2));
            driver.Receipts.Enqueue(Ack(bad with { Revision = 22 + i * 3, Paused = false }, 12 + i * 2));
            driver.Arrivals.Enqueue(Burning(23 + i * 3, 102 + i));
        }
        await Assert.ThrowsAsync<InvalidOperationException>(() => driver.Run());
        Assert.Equal(CappedPauses, driver.Commands);
        Assert.Equal(2, driver.Floors.Count);
        Assert.False(driver.Current.Paused);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CancellationDoesNotLeaveAnAdmissionWaiterOrSendUnownedResume(bool afterRequest)
    {
        using var cancellation = new CancellationTokenSource();
        var driver = new Driver { Current = Burning(paused: !afterRequest) };
        if (afterRequest)
        {
            driver.Receipts.Enqueue(Ack(Burning(21, 101, true) with { Enemies = [] }));
            driver.AfterCommand = cancellation.Cancel;
        }
        else driver.WaitOverride = token => { cancellation.Cancel(); token.ThrowIfCancellationRequested(); return Task.CompletedTask; };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => driver.Run(cancellation.Token));
        Assert.Equal(afterRequest ? OnePause : Array.Empty<string>(), driver.Commands);
    }

    [Theory]
    [InlineData("sequence")]
    [InlineData("rejected")]
    [InlineData("paused")]
    [InlineData("stale")]
    public async Task InvalidResumeReceiptCannotRearmOrSendAnotherPause(string fault)
    {
        var driver = new Driver();
        MatchSnapshot bad = Burning(21, 101, true) with { Enemies = [] };
        driver.Receipts.Enqueue(Ack(bad));
        GameEvent resume = Ack(bad with { Revision = 22, Paused = false }, 12);
        driver.Receipts.Enqueue(fault switch
        {
            "sequence" => resume with { Result = new(11, true, "old ack") },
            "rejected" => resume with { Result = new(12, false, "rejected") },
            "paused" => resume with { State = bad },
            _ => resume with { State = bad with { Revision = 20, Paused = false } }
        });
        await Assert.ThrowsAsync<InvalidOperationException>(() => driver.Run());
        Assert.Equal(ResumedPause, driver.Commands);
        Assert.Empty(driver.Floors);
    }

    [Fact]
    public async Task ExpiredCurrentObservationWaitsForFreshStateBeforeRequestingPause()
    {
        var driver = new Driver { Current = Burning(tick: 280) };
        MatchSnapshot fresh = Burning(21, 281) with
        {
            Enemies = [Burning().Enemies[0] with { Statuses = StatusPolicy.Apply(new(), new(139, StatusKind.Burn, 132, 4, 281, 252), new()) }]
        };
        driver.Arrivals.Enqueue(fresh);
        GameEvent accepted = Ack(fresh with { Revision = 22, Paused = true });
        driver.Receipts.Enqueue(accepted);
        var result = await driver.Run();
        Assert.Same(accepted, result.Receipt);
        Assert.Equal(new long[] { 20 }, driver.Floors);
        Assert.Equal(OnePause, driver.Commands);
    }

    [Theory]
    [InlineData("terminal")]
    [InlineData("clear")]
    [InlineData("reset")]
    public async Task FreshCurrentLossAfterResumeFailsRatherThanInventingANewOpening(string loss)
    {
        var driver = new Driver();
        MatchSnapshot bad = Burning(21, 101, true) with { Enemies = [] };
        driver.Receipts.Enqueue(Ack(bad));
        driver.Receipts.Enqueue(Ack(bad with { Revision = 22, Paused = false }, 12));
        MatchSnapshot next = Burning(23, 102);
        driver.Arrivals.Enqueue(loss switch
        {
            "terminal" => next with { Phase = Phase.Victory },
            "clear" => next with { Phase = Phase.Building, Wave = 9 },
            _ => next with { MatchId = "reset" }
        });
        await Assert.ThrowsAsync<InvalidOperationException>(() => driver.Run());
        Assert.Equal(ResumedPause, driver.Commands);
    }
}
