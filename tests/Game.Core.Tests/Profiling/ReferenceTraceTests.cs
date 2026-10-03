using Xunit;

namespace Game.Core.Tests.Profiling;

public sealed class ReferenceTraceTests
{
    [Fact]
    public void InstrumentedUnoptimizedOrderedReferencesRemainExact()
    {
        // Captured before any runtime/harness optimization. Only session identity
        // is canonicalized; seed 8, rules, unit ids, route/event order and deadlines
        // remain inputs. The ordinary replay uses seed 1 and its fixed match id.
        IReadOnlyDictionary<string, string> actual = ReferenceTraces.Capture();
        Assert.Equal("105D6640ECC166A7E8F6557591048DA10E821151F9F2A2F460CD5B47CC8AF431", actual["ordinary-playback-output"]);
        Assert.Equal("2C3BA0DE7F10142B533E9609371A4532FAD90F413E7A0DC3DAD8EE21B8D47AB3", actual["ordinary-playback-pause-resume"]);
        Assert.Equal("BA56F166301AE4D4A5ADD46A52E4502D85697A6396AC3984A644D112F4D0D019", actual["pause-resume-terminal-cleanup"]);
        Assert.Equal("C16EDCE859933260BE48536027CD5E1F53D1B7F7BB7C9DF1FC0F8E3285B1A8FB", actual["route-target-ties-transfer"]);
        Assert.Equal("AC985BF1B166512F695FB614BB0A340415E08A8E71BD965CF48D010FB44BE751", actual["simultaneous-impact-death-queued-admission"]);
    }
}
