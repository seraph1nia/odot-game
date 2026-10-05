using Game;
using Xunit;

namespace DevRunner.Tests;

public sealed class OwnedFrameCaptureTests
{
    private static byte[] Pixels => Enumerable.Range(0, 16).Select(n => (byte)n).ToArray();
    [Fact]
    public void DelayedPersistenceCannotResamplePixelsTicksOrAuthority()
    {
        var owner = new OwnedFrameCapture();
        byte[] input = Pixels;
        var receipt = owner.Acquire("frame-1", "one.png", 2, 2, input, "tick=301;revision=44;camera=near;bone=attack");
        input[0] = 255;
        TimeSpan live = TimeSpan.FromSeconds(11.9);
        OwnedFrameCapture.RequireLiveCompletion(live, 12, 4);
        long authorityTick = 301;
        TimeSpan controlledPersistenceDelay = TimeSpan.FromSeconds(20);
        DateTimeOffset clock = DateTimeOffset.UnixEpoch + live;
        var result = owner.Persist(receipt.Id, (acquired, immutableCopy) =>
        {
            // Controlled slow evidence completion while authority advances;
            // this callback never has a scene/view/authority sampling interface.
            authorityTick += 1200; clock += controlledPersistenceDelay;
            Assert.Equal(receipt, acquired); Assert.Equal(Pixels, immutableCopy);
            return immutableCopy;
        });
        Assert.Equal(1501, authorityTick); Assert.Equal(receipt, result);
        Assert.True((clock - DateTimeOffset.UnixEpoch).TotalSeconds > 12); // old mixed contract would fail
        Assert.True(controlledPersistenceDelay <= TimeSpan.FromSeconds(30));
        Assert.Contains("tick=301", result.Metadata); Assert.Equal(0, owner.Pending);
        OwnedFrameCapture.RequireLiveCompletion(live, 12, 4);
    }
    [Theory]
    [InlineData(12.001, 12, 4)]
    [InlineData(11, 11, 4)]
    [InlineData(11, 12, 3)]
    public void SlowOrMissingLiveWitnessStillFailsOriginalBound(double seconds, int observations, int captures)
        => Assert.Throws<InvalidOperationException>(() => OwnedFrameCapture.RequireLiveCompletion(TimeSpan.FromSeconds(seconds), observations, captures));
    [Fact]
    public void DuplicateMissingAndOutOfOrderFramesFail()
    {
        var owner = new OwnedFrameCapture();
        owner.Acquire("first", "first.png", 2, 2, Pixels, "first tick");
        owner.Acquire("second", "second.png", 2, 2, Pixels, "second tick");
        Assert.Throws<InvalidOperationException>(() => owner.Acquire("first", "third.png", 2, 2, Pixels, "new tick"));
        Assert.Throws<InvalidOperationException>(() => owner.Persist("second", (_, pixels) => pixels));
        owner.Persist("first", (_, pixels) => pixels);
        Assert.Throws<InvalidOperationException>(() => owner.Persist("first", (_, pixels) => pixels));
    }
    [Fact]
    public void CorruptedMissingPngAndCancellationPreventCompletionAndRetainOwnership()
    {
        var owner = new OwnedFrameCapture();
        owner.Acquire("frame", "frame.png", 2, 2, Pixels, "tick=301");
        Assert.Throws<InvalidOperationException>(() => owner.Persist("frame", (_, pixels) => { pixels[0] ^= 1; return pixels; }));
        Assert.Equal(1, owner.Pending);
        Assert.Throws<FileNotFoundException>(() => owner.Persist("frame", (_, _) => throw new FileNotFoundException()));
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => owner.Persist("frame", (_, _) =>
        {
            Assert.Throws<InvalidOperationException>(owner.Clear);
            cancellation.Token.ThrowIfCancellationRequested(); return Pixels;
        }));
        Assert.Equal(1, owner.Pending); owner.Clear(); Assert.Equal(0, owner.Pending);
    }
}
