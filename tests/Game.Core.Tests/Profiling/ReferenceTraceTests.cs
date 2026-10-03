using Xunit;

namespace Game.Core.Tests.Profiling;

public sealed class ReferenceTraceTests
{
    [Fact]
    public void CurrentEconomyProtocolOrderedReferencesRemainExact()
    {
        // Refreshed snapshot hashes for the economy protocol; the three combat/output hashes are unchanged. Only session identity
        // is canonicalized; seed 8, rules, unit ids, route/event order and deadlines
        // remain inputs. The ordinary replay uses seed 1 and its fixed match id.
        IReadOnlyDictionary<string, string> actual = ReferenceTraces.Capture();
        Assert.Equal("F69D7EFCA70A193F6FD6DD8A370FD36C83AF9A0309817D7848A382D14C37BBB3", actual["ordinary-playback-output"]);
        Assert.Equal("BB8D4BF4832A38199364B788CBEAF7AEB8428FC84B9971F60056523F1D260B85", actual["ordinary-playback-pause-resume"]);
        Assert.Equal("51C1CFA9E680534E82E299E53989AD0362C3575A1558B9B7C7B98B53B33942B5", actual["pause-resume-terminal-cleanup"]);
        Assert.Equal("306CAEFC9C5E153826E9F82BA5127860CAC722EA750EB71213A199FE211D075D", actual["route-target-ties-transfer"]);
        Assert.Equal("EC232F79EFB0DDFD5DDB1E2597B49970EB43C77EBCC58E396741D047A875AB66", actual["simultaneous-impact-death-queued-admission"]);
    }
}
