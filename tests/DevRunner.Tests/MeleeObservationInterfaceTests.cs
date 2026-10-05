using DevRunner;
using Xunit;

namespace DevRunner.Tests;

// Controlled protocol only: no Godot, network, render, PNG or desktop.
public sealed class MeleeObservationInterfaceTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ProbeIdentityRejectsMisTaggedResponsesButDoesNotEstablishAuthorityFreshness(bool correctId)
    {
        string directory = Path.Combine(Path.GetTempPath(), "odot-observation-control-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            // A fresh request can return an old applied revision. This control
            // deliberately supplies no witness/PNG and makes no acceptance claim.
            string script = "IFS= read -r request; set -- $request; "
                + (correctId ? "id=$2; " : "id=mis-tagged; ")
                + "printf 'ODOT_UI {\"Id\":\"%s\",\"Revision\":32,\"CombatTick\":8,\"PhaseText\":\"Combat\"}\\n' \"$id\"";
            await using var child = new Child("observation-control", "setsid", ["/bin/sh", "-c", script], directory, quiet: true, ownsGroup: true);
            if (!correctId)
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() => UiProtocol.Probe(child, 2000, CancellationToken.None, live: true));
                return;
            }
            UiObservation observed = await UiProtocol.Probe(child, 2000, CancellationToken.None, live: true);
            Assert.NotEqual("mis-tagged", observed.Id);
            Assert.Equal(32, observed.Revision); Assert.Equal(8, observed.CombatTick);
            Assert.Null(observed.Screenshot); Assert.Null(observed.RawCapture);
            Assert.Empty(observed.Units); Assert.Empty(observed.Strikes);
            // Receipt identity is not a state-freshness proof. Milestone capture
            // must separately agree with a current, nonterminal pause ack and
            // actual rendered action, not just this fresh observation id.
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
