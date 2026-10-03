using Xunit;

namespace Game.Core.Tests.Profiling;

public sealed class ReferenceTraceTests
{
    [Fact]
    public void CurrentResearchProtocolOrderedReferencesRemainExact()
    {
        // Refreshed for the research/status protocol and combat fingerprint. Only session identity
        // is canonicalized; seed 8, rules, unit ids, route/event order and deadlines
        // remain inputs. The ordinary replay uses seed 1 and its fixed match id.
        IReadOnlyDictionary<string, string> actual = ReferenceTraces.Capture();
        Assert.Equal("F69D7EFCA70A193F6FD6DD8A370FD36C83AF9A0309817D7848A382D14C37BBB3", actual["ordinary-playback-output"]);
        Assert.Equal("3CB0A4B14936150FD193BCD4152A3716E748C2C1B71F4C7DE746EDBA05202348", actual["ordinary-playback-pause-resume"]);
        Assert.Equal("DA3E0CEDBA35CCBD3C118DCAFC949E19AF5FF210A785181C25A1D91765F2C0C5", actual["pause-resume-terminal-cleanup"]);
        Assert.Equal("306CAEFC9C5E153826E9F82BA5127860CAC722EA750EB71213A199FE211D075D", actual["route-target-ties-transfer"]);
        Assert.Equal("EC232F79EFB0DDFD5DDB1E2597B49970EB43C77EBCC58E396741D047A875AB66", actual["simultaneous-impact-death-queued-admission"]);
    }
}
