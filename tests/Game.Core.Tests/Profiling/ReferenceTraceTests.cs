using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests.Profiling;

public sealed class ReferenceTraceTests(ITestOutputHelper output)
{
    [Fact]
    public void CurrentRosterProtocolOrderedReferencesRemainExact()
    {
        // The paid replay deliberately starts six soldiers first-fit across two physical homes,
        // replacing seeded balanced class formation. Its output and snapshot hashes therefore change;
        // terminal snapshots also include roster quotes/food receipts. The two isolated combat hashes
        // MUST remain unchanged: no numerical, enemy formation or low-level admission rebalance.
        // Only session identity is canonicalized; all seeds, ids, order and deadlines remain inputs.
        IReadOnlyDictionary<string, string> actual = ReferenceTraces.Capture();
        foreach ((string name, string digest) in actual) output.WriteLine($"{name}: {digest}");
        Assert.Equal("451CAC9884D5131EDD2CBB8D321DBB2F7BC6C1511054D6C616053D9142FF8C8C", actual["ordinary-playback-output"]);
        Assert.Equal("0DC06EE7F69B387B9ABB00947C659DE8BEB8C5FFF840761FD1F847F349DED6E7", actual["ordinary-playback-pause-resume"]);
        Assert.Equal("351DCB70DB7AE0FEFEF658DD35970E5208E2D9EEADFB4ADEF9A84DB8090E7538", actual["pause-resume-terminal-cleanup"]);
        Assert.Equal("306CAEFC9C5E153826E9F82BA5127860CAC722EA750EB71213A199FE211D075D", actual["route-target-ties-transfer"]);
        Assert.Equal("EC232F79EFB0DDFD5DDB1E2597B49970EB43C77EBCC58E396741D047A875AB66", actual["simultaneous-impact-death-queued-admission"]);
    }
}
