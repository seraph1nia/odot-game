using System.Text.Json;
using Game.Core;
using Xunit;

namespace DevRunner.Tests;

public sealed class CombatPoseDiagnosticTests
{
    private static readonly CombatPoseFlags Flags = new(true, false, true, true, true, true, true);
    private static UiObservation Frame(string id, long revision = 10) => new()
    {
        Id = id,
        MatchId = "synthetic-match",
        Revision = revision,
        CombatTick = 120,
        ObservedCity = 1,
        Width = 1280,
        Height = 720,
        Camera = new() { Zoom = 2, PanX = 3, PanZ = 4 }
    };
    private static UnitObservation Actor(long readyTick = 150) => new()
    {
        Id = 7,
        Type = UnitType.Swordsman,
        Visible = true,
        Clip = "attack",
        AttackActive = true,
        ReadyTick = readyTick,
        X = 2,
        Z = 5,
        PoseSeconds = .12
    };

    [Fact]
    public void ReportsEachFlagAndMissingProvenanceWithoutInventingWitnesses()
    {
        var diagnostic = new CombatPoseDiagnostic();
        var frame = Frame("returned-id");
        diagnostic.First("sword", CombatPoseDiagnostic.Witness(frame, Actor(), "retained-entry-frame"));
        diagnostic.Begin(Flags);
        using JsonDocument report = JsonDocument.Parse(diagnostic.Finish(Flags, "failed-or-cancelled"));
        Assert.False(report.RootElement.GetProperty("Entry").GetProperty("Mage").GetBoolean());
        Assert.True(report.RootElement.GetProperty("Exit").GetProperty("InspectionChanged").GetBoolean());
        JsonElement[] records = report.RootElement.GetProperty("Provenance").EnumerateArray().ToArray();
        Assert.Equal(CombatPoseDiagnostic.FlagNames, records.Select(record => record.GetProperty("Flag").GetString()));
        JsonElement sword = records[0].GetProperty("FirstWitness");
        Assert.Equal("returned-id", sword.GetProperty("ResponseId").GetString());
        Assert.Equal(7, sword.GetProperty("Actor").GetProperty("Id").GetInt32());
        Assert.Equal("attack", sword.GetProperty("Actor").GetProperty("Clip").GetString());
        Assert.Equal(.12, sword.GetProperty("Actor").GetProperty("PoseSeconds").GetDouble());
        Assert.Equal(120, sword.GetProperty("Tick").GetDouble());
        Assert.Equal(10, sword.GetProperty("Revision").GetInt64());
        Assert.Equal(2, sword.GetProperty("Zoom").GetDouble());
        Assert.Equal(3, sword.GetProperty("PanX").GetDouble());
        Assert.Equal(4, sword.GetProperty("PanZ").GetDouble());
        Assert.Equal(JsonValueKind.Null, sword.GetProperty("ReceivedRevision").ValueKind);
        Assert.All(records.Skip(1), record =>
        {
            Assert.Equal(JsonValueKind.Null, record.GetProperty("FirstWitness").ValueKind);
            Assert.NotNull(record.GetProperty("MissingProvenance").GetString());
        });
    }

    [Fact]
    public void FirstWitnessUsesCallArrivalOrderAndRecoveryRetainsBothRealResponses()
    {
        using var authority = new AuthoritySession(AuthorityPolicy.Solo);
        MatchSnapshot received = authority.Snapshot() with { Tick = 123, Revision = 8 };
        var diagnostic = new CombatPoseDiagnostic();
        var earlier = CombatPoseDiagnostic.Witness(Frame("first-response", 20), Actor(), "current-poll", received);
        var later = CombatPoseDiagnostic.Witness(Frame("second-response", 3), Actor(), "current-poll", received);
        diagnostic.First("sword", earlier);
        diagnostic.First("sword", later);
        diagnostic.Recovery(earlier, later);
        diagnostic.Recovery(later, earlier);
        diagnostic.Poll(Frame("second-response", 3), received);
        using JsonDocument report = JsonDocument.Parse(diagnostic.Finish(Flags, "passed"));
        Assert.Equal("first-response", report.RootElement.GetProperty("Provenance")[0].GetProperty("FirstWitness").GetProperty("ResponseId").GetString());
        JsonElement previous = report.RootElement.GetProperty("RecoveryPrevious"), current = report.RootElement.GetProperty("RecoveryCurrent");
        Assert.Equal("first-response", previous.GetProperty("ResponseId").GetString());
        Assert.Equal("second-response", current.GetProperty("ResponseId").GetString());
        foreach (JsonElement witness in new[] { previous, current })
        {
            Assert.Equal(7, witness.GetProperty("Actor").GetProperty("Id").GetInt32());
            Assert.Equal(150, witness.GetProperty("Actor").GetProperty("ReadyTick").GetInt64());
            Assert.Equal(2, witness.GetProperty("Actor").GetProperty("X").GetDouble());
            Assert.Equal(5, witness.GetProperty("Actor").GetProperty("Z").GetDouble());
            Assert.Equal(123, witness.GetProperty("ReceivedTick").GetInt64());
            Assert.Equal(8, witness.GetProperty("ReceivedRevision").GetInt64());
        }
        Assert.Equal(3, report.RootElement.GetProperty("LastObservation").GetProperty("Revision").GetInt64());
    }

    [Fact]
    public void RecordsStayBoundedAndOversizedProvenanceIsExplicitlyMissing()
    {
        using var authority = new AuthoritySession(AuthorityPolicy.Solo);
        var diagnostic = new CombatPoseDiagnostic();
        for (int n = 0; n < 1000; n++)
        {
            diagnostic.Poll(Frame("response-" + n), authority.Snapshot());
            foreach (string flag in CombatPoseDiagnostic.FlagNames)
                diagnostic.First(flag, CombatPoseDiagnostic.Witness(Frame("first-response"), Actor(), "current-poll"));
        }
        string content = diagnostic.Finish(Flags, "passed");
        Assert.True(System.Text.Encoding.UTF8.GetByteCount(content) < 128 * 1024);
        using JsonDocument report = JsonDocument.Parse(content);
        Assert.Equal(7, report.RootElement.GetProperty("Provenance").GetArrayLength());
        Assert.Equal(1000, report.RootElement.GetProperty("Polls").GetInt32());
        Assert.Equal("response-999", report.RootElement.GetProperty("LastObservation").GetProperty("ResponseId").GetString());
        var oversized = new CombatPoseDiagnostic();
        oversized.First("Mage", CombatPoseDiagnostic.Witness(Frame(new string('x', 8193)), Actor(), "current-poll"));
        oversized.First("Mage", CombatPoseDiagnostic.Witness(Frame("later-response"), Actor(), "current-poll"));
        using JsonDocument missing = JsonDocument.Parse(oversized.Finish(Flags, "failed-or-cancelled"));
        Assert.Equal(JsonValueKind.Null, missing.RootElement.GetProperty("Provenance")[1].GetProperty("FirstWitness").ValueKind);
        Assert.NotNull(missing.RootElement.GetProperty("Provenance")[1].GetProperty("MissingProvenance").GetString());
    }

    [Fact]
    public void FailureBundleRetainsOnlyOwnedWhitelistAndRedactsSerializedSecrets()
    {
        string root = Fixture();
        try
        {
            Seed(root);
            File.WriteAllText(Path.Combine(root, ".Xauthority"), "excluded-secret");
            File.WriteAllText(Path.Combine(root, "ui-client.log"), "excluded-secret");
            File.WriteAllText(Path.Combine(root, "credentials.json"), "excluded-secret");
            Collect(root);
            string bundle = Path.Combine(root, "pose-failure");
            string[] expected = CombatFailureEvidence.JsonFiles
                .Concat(CombatFailureEvidence.Milestones.SelectMany(name => new[] { name + ".png", name + "-observation.json" }))
                .Concat(["ui-client.states.json", "ui-observer.states.json", "ui-server.states.json", "manifest.json"]).Order().ToArray();
            Assert.Equal(expected, Directory.GetFiles(bundle).Select(Path.GetFileName).Order());
            using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(bundle, "manifest.json")));
            Assert.Equal(JsonValueKind.Null, manifest.RootElement.GetProperty("FirstMissing").ValueKind);
            Assert.All(manifest.RootElement.GetProperty("Files").EnumerateArray(), file => Assert.Equal("retained", file.GetProperty("Status").GetString()));
            using JsonDocument state = JsonDocument.Parse(File.ReadAllText(Path.Combine(bundle, "ui-client.states.json")));
            Assert.Equal("[redacted]", state.RootElement.GetProperty("Token").GetString());
            using JsonDocument recent = JsonDocument.Parse(state.RootElement.GetProperty("RecentUi").GetProperty("Message").GetString()!);
            Assert.Equal("[redacted]", recent.RootElement.GetProperty("Credential").GetString());
            Assert.Equal(File.ReadAllBytes(Path.Combine(root, "combat-paused.png")), File.ReadAllBytes(Path.Combine(bundle, "combat-paused.png")));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void MissingFieldAndByteLimitAreReportedWithoutUploadingOtherFiles()
    {
        string root = Fixture();
        try
        {
            Seed(root);
            File.WriteAllText(Path.Combine(root, "client.states.json"), "{\"States\":[]}");
            using (var large = File.Create(Path.Combine(root, "combat-casualty.png"))) large.SetLength(CombatFailureEvidence.MaximumFileBytes + 1);
            Collect(root);
            string bundle = Path.Combine(root, "pose-failure");
            using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(bundle, "manifest.json")));
            JsonElement[] files = manifest.RootElement.GetProperty("Files").EnumerateArray().ToArray();
            Assert.Equal("RecentUi", files.Single(file => file.GetProperty("Name").GetString() == "ui-client.states.json").GetProperty("FirstMissingField").GetString());
            Assert.Equal("file-byte-limit", files.Single(file => file.GetProperty("Name").GetString() == "combat-casualty.png").GetProperty("Status").GetString());
            Assert.False(File.Exists(Path.Combine(bundle, "combat-casualty.png")));
            Assert.NotEqual(JsonValueKind.Null, manifest.RootElement.GetProperty("FirstMissing").ValueKind);
            Assert.InRange(manifest.RootElement.GetProperty("TotalBytes").GetInt64(), 0, CombatFailureEvidence.MaximumTotalBytes);
        }
        finally { Directory.Delete(root, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ActualCheckpointEarlyCancellationStagesDefaultCombatOnlyAndPreservesCancellation(bool shortCheck)
    {
        string root = Fixture();
        try
        {
            var evidence = new Evidence(root);
            await using var scope = new ScenarioScope("combat", evidence, runtimeRoot: root);
            string shell = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh";
            string[] shellArgs = OperatingSystem.IsWindows() ? ["/c", "more > nul"] : ["-c", "while IFS= read -r command; do :; done"];
            Child Own(string name) => scope.Own(new Child(name, shell, shellArgs,
                scope.Directory, evidenceDirectory: scope.EvidenceDirectory));
            _ = Own("ui-server");
            Child client = Own("ui-client"), observer = Own("ui-observer");
            var runner = new Runner(Options.Parse(["_ui-worker", "--scenario", "combat"]), CancellationToken.None, evidence, scope, root);
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            // Execute the real orchestration boundary with owned shell peers, no engine/display and no timeout wait.
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => runner.CombatCheckpoint(client, observer, cancelled.Token, shortCheck));
            string bundle = Path.Combine(scope.EvidenceDirectory, "pose-failure");
            if (shortCheck)
            {
                Assert.False(Directory.Exists(bundle));
                return;
            }
            using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(bundle, "manifest.json")));
            JsonElement[] files = manifest.RootElement.GetProperty("Files").EnumerateArray().ToArray();
            Assert.Equal(23, files.Length);
            Assert.Equal(5, files.Count(file => file.GetProperty("Status").GetString() == "retained"));
            using JsonDocument proof = JsonDocument.Parse(File.ReadAllText(Path.Combine(bundle, "combat-pose-proof.json")));
            Assert.Equal("default-combat-failed;pose-loop-unexecuted", proof.RootElement.GetProperty("Result").GetString());
            Assert.Equal(JsonValueKind.Null, proof.RootElement.GetProperty("Entry").ValueKind);
            Assert.Equal(JsonValueKind.Null, proof.RootElement.GetProperty("RecoveryCurrent").ValueKind);
            Assert.False(proof.RootElement.GetProperty("Exit").GetProperty("Recovery").GetBoolean());
            using JsonDocument admission = JsonDocument.Parse(File.ReadAllText(Path.Combine(bundle, "combat-casualty-admission.json")));
            Assert.Equal(JsonValueKind.Null, admission.RootElement.GetProperty("AfterTick").ValueKind);
            Assert.Empty(admission.RootElement.GetProperty("Events").EnumerateArray());
            foreach (string name in new[] { "ui-client", "ui-observer", "ui-server" })
            {
                using JsonDocument state = JsonDocument.Parse(File.ReadAllText(Path.Combine(bundle, name + ".states.json")));
                Assert.True(state.RootElement.TryGetProperty("States", out _));
                Assert.True(state.RootElement.TryGetProperty("RecentUi", out _));
            }
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void EarlyAdmissionFailureBundleRetainsLivePairRejectionAndStateWithoutInventingLaterMilestones()
    {
        string root = Fixture();
        try
        {
            using var authority = new AuthoritySession(AuthorityPolicy.Solo);
            MatchSnapshot origin = authority.Snapshot() with { MatchId = "synthetic-match", Phase = Phase.Combat, Tick = 120, Revision = 10, Paused = true };
            MatchSnapshot current = origin with { Phase = Phase.Building, Tick = 151, Revision = 41 };
            var diagnostic = new CombatPoseDiagnostic();
            var recovery = new CombatRecoveryObservation(diagnostic);
            UnitObservation actor = Actor() with
            {
                Health = 1000,
                MaximumHealth = 1000,
                ImpactTick = 110,
                AttackSequence = 1,
                Hex = new(7, 1, Faction.Adventurers, UnitLifecycle.Alive, new(1, 1), UnitActionKind.Recovery, ActionSequence: 1)
            };
            UiObservation first = Frame("early-first") with { MatchPhase = Phase.Combat, Connected = true, Units = [actor] };
            UiObservation second = first with { Id = "early-second", CombatTick = 138, Revision = 28 };
            Assert.False(recovery.Observe(first, origin, "early-live:locomotion"));
            Assert.True(recovery.Observe(second, origin with { Tick = 138, Revision = 28 }, "early-live:attack"));
            // Subsequent paused observations must not replace the current rendered live pair.
            Assert.True(recovery.Observe(second with { Id = "paused", Paused = true }, origin, "control"));
            InvalidOperationException failure = Assert.Throws<InvalidOperationException>(() => CasualtyAdmission.Frozen(origin, current));
            File.WriteAllText(Path.Combine(root, "combat-pose-proof.json"),
                diagnostic.Finish(new(false, false, false, false, false, recovery.Proven, false), "default-combat-failed;pose-loop-unexecuted"));
            File.WriteAllText(Path.Combine(root, "combat-casualty-admission.json"), JsonSerializer.Serialize(new
            {
                Events = new object[] { new { Stage = "origin", State = origin },
                    new { Stage = "identity-rejection", Failure = failure.Data[CasualtyAdmission.IdentityEvidenceKey], State = failure.Data[CasualtyAdmission.IdentityStateKey] } }
            }));
            foreach (string name in new[] { "client", "observer", "authority" })
                File.WriteAllText(Path.Combine(root, name + ".states.json"), JsonSerializer.Serialize(new { States = new[] { origin, current }, RecentUi = new[] { first, second } }));
            Collect(root);
            string bundle = Path.Combine(root, "pose-failure");
            using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(bundle, "manifest.json")));
            JsonElement[] files = manifest.RootElement.GetProperty("Files").EnumerateArray().ToArray();
            Assert.Equal(23, files.Length);
            Assert.Equal(5, files.Count(file => file.GetProperty("Status").GetString() == "retained"));
            Assert.All(files.Where(file => file.GetProperty("Status").GetString() != "retained"), file => Assert.Equal("missing-file", file.GetProperty("Status").GetString()));
            using JsonDocument proof = JsonDocument.Parse(File.ReadAllText(Path.Combine(bundle, "combat-pose-proof.json")));
            Assert.Equal(JsonValueKind.Null, proof.RootElement.GetProperty("Entry").ValueKind);
            Assert.True(proof.RootElement.GetProperty("Exit").GetProperty("Recovery").GetBoolean());
            Assert.Equal("early-first", proof.RootElement.GetProperty("RecoveryPrevious").GetProperty("ResponseId").GetString());
            JsonElement witness = proof.RootElement.GetProperty("RecoveryCurrent");
            Assert.Equal("early-second", witness.GetProperty("ResponseId").GetString());
            Assert.Equal(138, witness.GetProperty("Tick").GetDouble());
            Assert.False(witness.GetProperty("Paused").GetBoolean());
            Assert.Equal(150, witness.GetProperty("Actor").GetProperty("ReadyTick").GetInt64());
            Assert.Equal(2, witness.GetProperty("Actor").GetProperty("X").GetDouble());
            Assert.Equal(5, witness.GetProperty("Actor").GetProperty("Z").GetDouble());
            using JsonDocument admission = JsonDocument.Parse(File.ReadAllText(Path.Combine(bundle, "combat-casualty-admission.json")));
            JsonElement rejection = admission.RootElement.GetProperty("Events")[1].GetProperty("Failure");
            Assert.Equal("frozen-current", rejection.GetProperty("Callsite").GetString());
            Assert.True(rejection.GetProperty("Failed").GetProperty("CombatPhase").GetBoolean());
            Assert.False(rejection.GetProperty("Failed").GetProperty("MatchId").GetBoolean());
            Assert.Equal(151, rejection.GetProperty("Current").GetProperty("Tick").GetInt64());
            using JsonDocument state = JsonDocument.Parse(File.ReadAllText(Path.Combine(bundle, "ui-observer.states.json")));
            Assert.Equal(41, state.RootElement.GetProperty("States")[1].GetProperty("Revision").GetInt64());
            Assert.Equal("early-second", state.RootElement.GetProperty("RecentUi")[1].GetProperty("Id").GetString());
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void FailureBundleEnforcesAggregateBudget()
    {
        string root = Fixture();
        try
        {
            Seed(root);
            foreach (string name in CombatFailureEvidence.Milestones.Take(5))
            {
                using var large = File.Create(Path.Combine(root, name + ".png"));
                large.SetLength(CombatFailureEvidence.MaximumFileBytes);
            }
            Collect(root);
            string bundle = Path.Combine(root, "pose-failure");
            using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(bundle, "manifest.json")));
            Assert.Contains(manifest.RootElement.GetProperty("Files").EnumerateArray(),
                file => file.GetProperty("Status").GetString() == "bundle-byte-limit");
            long actualBytes = Directory.GetFiles(bundle).Where(path => Path.GetFileName(path) != "manifest.json").Sum(path => new FileInfo(path).Length);
            Assert.Equal(actualBytes, manifest.RootElement.GetProperty("TotalBytes").GetInt64());
            Assert.InRange(actualBytes, 0, CombatFailureEvidence.MaximumTotalBytes);
        }
        finally { Directory.Delete(root, true); }
    }

    private static string Fixture()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "Odot.slnx"))) root = root.Parent;
        string path = Path.Combine(root!.FullName, "logs", "pose-diagnostic-control-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
    private static void Collect(string root) => CombatFailureEvidence.Collect(root, Path.Combine(root, "client.states.json"),
        Path.Combine(root, "observer.states.json"), Path.Combine(root, "authority.states.json"));
    private static void Seed(string root)
    {
        foreach (string name in CombatFailureEvidence.Milestones)
        {
            File.WriteAllBytes(Path.Combine(root, name + ".png"), [137, 80, 78, 71]);
            File.WriteAllText(Path.Combine(root, name + "-observation.json"), JsonSerializer.Serialize(Frame("actual-returned-id")));
        }
        File.WriteAllText(Path.Combine(root, "combat-casualty-admission.json"), "{\"Events\":[]}");
        File.WriteAllText(Path.Combine(root, "combat-casualty-pause.json"), "{\"MatchId\":\"synthetic\",\"Tick\":1,\"Revision\":2}");
        File.WriteAllText(Path.Combine(root, "combat-death-cleanup.json"), "{\"Removal\":{}}");
        var diagnostic = new CombatPoseDiagnostic();
        diagnostic.Begin(Flags);
        File.WriteAllText(Path.Combine(root, "combat-pose-proof.json"), diagnostic.Finish(Flags, "failed-or-cancelled"));
        foreach (string name in new[] { "client", "observer", "authority" })
            File.WriteAllText(Path.Combine(root, name + ".states.json"), JsonSerializer.Serialize(new
            {
                States = Array.Empty<object>(),
                RecentUi = new { Message = "{\"Credential\":\"nested-secret\"}" },
                Token = "secret"
            }));
    }
}
