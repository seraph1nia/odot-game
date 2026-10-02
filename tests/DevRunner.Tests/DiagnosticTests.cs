using System.Text.Json;
using Xunit;
using DevRunner;
using Game.Core;

namespace DevRunner.Tests;

public sealed class DiagnosticTests
{
    [Fact]
    public void SnapshotRingBoundsBytesAndKeepsResultsIndependentOfStates()
    {
        var events = new ChildEvents();
        using var authority = new AuthoritySession(AuthorityPolicy.Solo);
        for (int n = 0; n < 40; n++)
        {
            authority.Step();
            events.Add(new("snapshot", State: authority.Snapshot()), 1024 * 1024);
        }
        Assert.Equal(16, events.States().Length);
        Assert.Equal(24, events.DroppedStates);
        Assert.Equal(authority.Snapshot().Tick, events.LastTick);
        events.Add(new("ack", Result: new(7, true, "accepted")), 100);
        Assert.Equal(7, events.Find(e => e.Type == "ack")!.Result!.Sequence);
        Assert.NotNull(events.Find(e => e.State?.Revision == authority.Snapshot().Revision));
        events.Add(new("snapshot", State: authority.Snapshot()), ChildEvents.MaximumStateBytes + 1);
        Assert.Empty(events.States());
    }

    [Fact]
    public void MatchRestartKeepsArrivalOrderAndCountsEachResultOnce()
    {
        using var authority = new AuthoritySession(AuthorityPolicy.Solo);
        var events = new ChildEvents();
        var original = authority.Snapshot() with { MatchId = "original", Revision = 100 };
        events.Add(new("connected", State: original), 100);
        for (int n = 0; n < 20; n++) events.Add(new("snapshot", State: original with { Revision = 101 + n }), 100);
        Assert.Null(events.Find(e => e.Type == "connected" && e.State is { } state && state.MatchId != original.MatchId));
        var fresh = original with { MatchId = "fresh", Revision = 1 };
        events.Add(new("connected", State: fresh), 100);
        Assert.Equal("fresh", events.History().Last(e => e.State is not null).State!.MatchId);
        Assert.Equal(fresh, events.Find(e => e.Type == "connected" && e.State is { } state && state.MatchId != original.MatchId)!.State);
        events.Add(new("ack", State: fresh, Result: new(1, true, "accepted")), 100);
        Assert.Single(events.History(), e => e.Type == "ack");
    }

    [Fact]
    public void ReliableOverflowIsVisibleAndObservedResultsRemainOrdered()
    {
        var pending = new ChildEvents();
        for (int n = 0; n <= ChildEvents.MaximumEvents; n++) pending.Add(new("ready", PeerId: n), 10);
        Assert.Throws<InvalidOperationException>(() => pending.Find(_ => true));
        var driven = new ChildEvents();
        for (int n = 0; n <= ChildEvents.MaximumEvents; n++)
        {
            driven.Add(new("ready", PeerId: n), 10);
            Assert.Equal(n, driven.Find(e => e.PeerId == n)!.PeerId);
        }
        Assert.Null(driven.Failure);
        Assert.Equal(ChildEvents.MaximumEvents, driven.History().Length);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AbruptChildFlushesRedactedDiagnosticsAndReportsEarlyExit(bool trace)
    {
        string root = Path.Combine(Path.GetTempPath(), "odot-diagnostic-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await using var child = new Child("fixture", "/bin/sh", ["-c", "printf 'ODOT_EVENT {\"Type\":\"ready\",\"Message\":\"ready\",\"Credential\":\"secret-fixture\"}\\n'; printf 'ERROR: abrupt fixture\\n' >&2; exit 3"], root, trace: trace);
            Assert.Equal(3, await child.WaitExit(CancellationToken.None));
            await Assert.ThrowsAsync<InvalidOperationException>(() => child.WaitFor(e => e.Type == "missing", "missing checkpoint", 2000, CancellationToken.None));
            Assert.True(child.HasUnexpectedEngineErrors);
            Assert.DoesNotContain("secret-fixture", await File.ReadAllTextAsync(child.LogPath));
            Assert.Contains("abrupt fixture", await File.ReadAllTextAsync(child.LogPath));
            using var dump = JsonDocument.Parse(await File.ReadAllTextAsync(child.LogPath + ".states.json"));
            Assert.Contains("Child exited with 3", dump.RootElement.GetProperty("Condition").GetString());
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task OwnedChildFailureRetainsBoundedFullStatesAndSummaryReportsLimits()
    {
        string root = Path.Combine(Path.GetTempPath(), "odot-ring-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var authority = new AuthoritySession(AuthorityPolicy.Solo);
            string input = Path.Combine(root, "events.txt");
            var lines = new List<string>();
            for (int n = 0; n < 40; n++)
            {
                authority.Step();
                lines.Add(WireJson.EventPrefix + JsonSerializer.Serialize(new GameEvent("snapshot", State: authority.Snapshot()), WireJson.Options));
            }
            await File.WriteAllLinesAsync(input, lines);
            var evidence = new Evidence(root);
            await using var child = new Child("fixture", "/bin/sh", ["-c", "cat \"$1\"; exit 4", "fixture", input], root, evidenceDirectory: evidence.Directory, simulationSpeed: 4);
            Assert.Equal(4, await child.WaitExit(CancellationToken.None));
            using var dump = JsonDocument.Parse(await File.ReadAllTextAsync(child.LogPath + ".states.json"));
            Assert.Equal(16, dump.RootElement.GetProperty("States").GetArrayLength());
            Assert.Equal(24, dump.RootElement.GetProperty("TruncatedStates").GetInt32());
            Assert.Equal(4, dump.RootElement.GetProperty("SimulationSpeed").GetInt32());
            Assert.Equal(authority.Snapshot().Tick, dump.RootElement.GetProperty("LastTick").GetInt64());
            await evidence.Summary("test-network", "fixture", 2, "failed", 1, 2, 4);
            using var report = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(evidence.Directory, "test-network-summary.json")));
            Assert.True(report.RootElement.GetProperty("EvidenceBytes").GetInt64() > 0);
            Assert.Equal(2, report.RootElement.GetProperty("Jobs").GetInt32());
            Assert.Equal(2, report.RootElement.GetProperty("UiJobs").GetInt32());
            Assert.Equal(4, report.RootElement.GetProperty("SetupSimulationSpeed").GetInt32());
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void NestedSecretFieldsAreRedacted()
    {
        string source = JsonSerializer.Serialize(new { Message = "{\"Credential\":\"nested-secret\"}", Token = "outer-secret", SessionKey = "key-secret" });
        string safe = DiagnosticText.Redact(source);
        Assert.DoesNotContain("nested-secret", safe);
        Assert.DoesNotContain("outer-secret", safe);
        Assert.DoesNotContain("key-secret", safe);
    }
}
