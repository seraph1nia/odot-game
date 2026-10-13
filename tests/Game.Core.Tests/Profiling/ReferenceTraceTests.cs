using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests.Profiling;

[Trait("Tier", "Deep")]
public sealed class ReferenceTraceTests(ITestOutputHelper output)
{
    [Fact]
    public void CurrentSquadProfileInputsRepeatWithoutObservationConsumption()
    {
        // Explicit profile-input qualification, not a refreshed historical
        // individual-actor golden or a twenty-wave balance witness. Capture uses
        // real groups, exclusive reconstruction, physical sector redirection,
        // paid playback input and homogeneous terminal cleanup. Cheap numerical
        // and identity negative controls remain in the shared/authority suites.
        IReadOnlyDictionary<string, string> ordinary = ReferenceTraces.Capture();
        IReadOnlyDictionary<string, string> observed = ReferenceTraces.Capture(observe: true);
        Assert.Equal(ordinary.Keys, observed.Keys);
        foreach ((string name, string digest) in ordinary)
        {
            output.WriteLine($"{name}: {digest}");
            Assert.Equal(digest, observed[name]);
        }
    }
}
