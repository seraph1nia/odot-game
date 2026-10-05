using System.Text.Json;
using DevRunner;
using Game;
using Xunit;

namespace DevRunner.Tests;

public sealed class OwnedTimingTraceTests
{
    [Fact]
    public void TraceRequiresOwnershipAndBoundsOrdinalDigestClockEvidence()
    {
        string root = Path.Combine(Path.GetTempPath(), "odot-timing-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            string path = Path.Combine(root, "trace.jsonl");
            Assert.Throws<InvalidOperationException>(() => new OwnedTimingTrace(path, false));
            long before = OwnedTimingTrace.Now();
            using (var trace = new OwnedTimingTrace(path, true))
            {
                for (int index = 0; index < OwnedTimingTrace.MaximumRecords + 2; index++) trace.Record("control", digest: OwnedTimingTrace.Digest("fixed"));
                Assert.True(trace.Truncated);
            }
            using JsonDocument header = JsonDocument.Parse(File.ReadLines(path).First());
            Assert.Equal(1_000_000_000L, header.RootElement.GetProperty("Info").GetProperty("Frequency").GetInt64());
            string[] records = File.ReadAllLines(path); Assert.Equal(OwnedTimingTrace.MaximumRecords + 1, records.Length);
            long previous = before; int ordinal = 0;
            foreach (string record in records)
            {
                using JsonDocument value = JsonDocument.Parse(record);
                long current = value.RootElement.GetProperty("Nanoseconds").GetInt64(); Assert.True(current >= previous); previous = current;
                if (value.RootElement.TryGetProperty("Ordinal", out JsonElement order)) Assert.Equal(++ordinal, order.GetInt32());
            }
            Assert.Equal(OwnedTimingTrace.Digest("fixed"), OwnedTimingTrace.Digest("fixed"));
            Assert.NotEqual(OwnedTimingTrace.Digest("fixed"), OwnedTimingTrace.Digest("changed"));
        }
        finally { Directory.Delete(root, true); }
    }
    [Fact]
    public void ReportRejectsMismatchedClockFrequencyAndRequiresClosedOwners()
    {
        using JsonDocument left = JsonDocument.Parse("{\"Clock\":\"monotonic\",\"Frequency\":1000000000,\"BootId\":\"same\",\"TimeNamespace\":\"same\"}");
        using JsonDocument right = JsonDocument.Parse("{\"Clock\":\"monotonic\",\"Frequency\":1000,\"BootId\":\"same\",\"TimeNamespace\":\"same\"}");
        MeleeTimingReport.RequireAligned(left.RootElement, left.RootElement);
        Assert.Throws<InvalidOperationException>(() => MeleeTimingReport.RequireAligned(left.RootElement, right.RootElement));
    }
    [Fact]
    public void ClosedOwnedReportCorrelatesDigestsRatherThanInventingPacketIds()
    {
        string root = Path.Combine(Path.GetTempPath(), "odot-report-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
        try
        {
            using var match = new Game.Core.Match();
            Game.Core.MatchSnapshot state = match.Snapshot() with { Tick = 1 };
            string digest = OwnedTimingTrace.Digest("fixed snapshot");
            using (var server = new OwnedTimingTrace(Path.Combine(root, "ui-server-timing.jsonl"), true)) server.Record("authority-publish", state, digest);
            foreach (string name in new[] { "ui-client", "ui-observer" })
            {
                using var peer = new OwnedTimingTrace(Path.Combine(root, name + "-timing.jsonl"), true);
                peer.Record("snapshot-rpc-receive", state, digest); peer.Record("snapshot-decode-complete", state, digest);
                peer.Record("rendered-observation", state, info: new { PresentedTick = 1 });
            }
            using (var driver = new OwnedTimingTrace(Path.Combine(root, "driver-timing.jsonl"), true)) driver.Record("early-observer-register", state);
            MeleeTimingReport.Write(root);
            using JsonDocument result = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "timing-verdict-evidence.json")));
            Assert.False(result.RootElement.GetProperty("Acceptance").GetBoolean());
            Assert.True(result.RootElement.GetProperty("Graphical").GetProperty("RevisionOrderConserved").GetBoolean());
        }
        finally { Directory.Delete(root, true); }
    }
    [Fact]
    public void DiagnosticSelectionsDoNotReplaceOrdinaryMeleeOrEnterUnfilteredUi()
    {
        Assert.Equal("admission", Options.Parse(["test-ui", "--scenario", "combat", "--checkpoint", "admission"]).UiCheckpoint);
        Assert.Null(Options.Parse(["test-ui"]).UiCheckpoint);
        Assert.Equal("melee", Options.Parse(["test-ui", "--scenario", "combat", "--checkpoint", "melee"]).UiCheckpoint);
        Assert.Throws<ArgumentException>(() => Options.Parse(["test-ui", "--scenario", "reconnect", "--checkpoint", "admission"]));
    }
}
