using DevRunner;
using Xunit;

namespace DevRunner.Tests;

public sealed class LinuxPortLeaseFactAttribute : FactAttribute
{
    public LinuxPortLeaseFactAttribute()
    {
        if (!OperatingSystem.IsLinux()) Skip = "Kernel flock verification requires native Linux; Windows enforces FileShare.None natively.";
    }
}

public sealed class PortLeaseTests
{
    [LinuxPortLeaseFact]
    public async Task ASeparateProcessCannotBorrowAnOwnedPortLeaseAndReleaseMakesItUsable()
    {
        // Linux FileShare.None uses a kernel flock. No Godot/network process is
        // needed to establish that the lease survives a different runner process.
        int port = PortAllocator.Acquire();
        string lease = Path.Combine(Path.GetTempPath(), "odot-port-leases", port.ToString(System.Globalization.CultureInfo.InvariantCulture));
        try { Assert.NotEqual(0, await Probe()); }
        finally { PortAllocator.Release(port); }
        Assert.Equal(0, await Probe());
        async Task<int> Probe()
        {
            await using var process = new Child("port-lock-probe", "flock", ["--nonblock", lease, "true"], Environment.CurrentDirectory);
            using var bound = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            return await process.WaitExit(bound.Token);
        }
    }
}
