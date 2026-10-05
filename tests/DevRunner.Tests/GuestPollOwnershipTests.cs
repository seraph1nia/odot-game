using Game;
using Xunit;

namespace DevRunner.Tests;

public sealed class GuestPollOwnershipTests
{
    [Fact]
    public void OwnerRejectsStaleForeignReentrantAndReleasedAdmission()
    {
        var lease = new GuestPollOwnership(3, 7);
        Assert.Throws<InvalidOperationException>(() => lease.Enter(2, 7));
        Assert.Throws<InvalidOperationException>(() => lease.Enter(3, 8));
        lease.Enter(3, 7);
        Assert.Throws<InvalidOperationException>(() => lease.Enter(3, 7));
        lease.Exit(); lease.Release();
        Assert.Throws<InvalidOperationException>(() => lease.Enter(3, 7));
    }
    [Fact]
    public void DisconnectDuringCallbackReleasesWithoutDestroyingEnteredStack()
    {
        var lease = new GuestPollOwnership(4, 8); lease.Enter(4, 8); lease.Release();
        Assert.False(lease.Active); Assert.True(lease.Entered);
        lease.Exit(); Assert.False(lease.Entered);
        var replacement = new GuestPollOwnership(5, 8);
        Assert.Throws<InvalidOperationException>(() => replacement.Enter(4, 8));
        replacement.Enter(5, 8); replacement.Exit();
    }
}
