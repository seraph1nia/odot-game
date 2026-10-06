using System.Text.Json;
using Game.Core;
using Xunit;

namespace DevRunner.Tests;

public sealed class CombatRecoveryTraceTests
{
    private static UnitObservation Actor => new()
    {
        Id = 5,
        Type = UnitType.Swordsman,
        Visible = true,
        Health = 1000,
        MaximumHealth = 1000,
        Clip = "hit",
        AttackActive = true,
        HitActive = true,
        AttackSequence = 6,
        ImpactTick = 732,
        ReadyTick = 780,
        X = 2.1f,
        Z = -4.646152f,
        Hex = new(5, 1, Faction.Adventurers, UnitLifecycle.Alive, new(1, 1), UnitActionKind.Recovery, ActionSequence: 11)
    };
    private static UiObservation Frame(string id, double tick = 737, long revision = 829) => new()
    {
        Id = id,
        MatchId = "hosted-sequence-control",
        Connected = true,
        MatchPhase = Phase.Combat,
        ObservedCity = 1,
        Wave = 3,
        PlaybackGeneration = 4,
        CombatTick = tick,
        Revision = revision,
        Units = [Actor]
    };
    private static MatchSnapshot Received
    {
        get { using var authority = new AuthoritySession(AuthorityPolicy.Solo); return authority.Snapshot() with { MatchId = "latest-arrival", Tick = 800, Revision = 900 }; }
    }
    private static CombatPoseFlags Flags(bool recovery) => new(false, false, false, false, false, recovery, false);

    [Fact]
    public void Failed737To790SampleAndAllPriorCandidatesSurviveExistingFailureWhitelist()
    {
        var diagnostic = new CombatPoseDiagnostic();
        var collector = new CombatRecoveryObservation(diagnostic);
        var first = Frame("actual-first") with { Units = [Actor, Actor with { Id = 6, Hex = Actor.Hex! with { Id = 6 } }] };
        Assert.False(collector.Observe(first, Received, "early-live:locomotion"));
        diagnostic.EarlyRecoveryTrace.PredicateResult(true);
        UnitObservation windup = Actor with
        {
            Id = 34,
            Clip = "attack",
            ImpactTick = 791,
            ReadyTick = 845,
            Hex = Actor.Hex! with { Id = 34, Action = UnitActionKind.Windup }
        };
        Assert.False(collector.Observe(Frame("actual-second", 790, 882) with { Units = [windup] }, Received, "early-live:attack"));
        diagnostic.EarlyRecoveryTrace.PredicateResult(true);
        string root = Fixture();
        try
        {
            File.WriteAllText(Path.Combine(root, "combat-pose-proof.json"), diagnostic.Finish(Flags(collector.Proven), "failed"));
            foreach (string name in new[] { "client", "observer", "authority" })
                File.WriteAllText(Path.Combine(root, name + ".states.json"), "{\"States\":[],\"RecentUi\":[],\"Token\":\"private-secret\"}");
            File.WriteAllText(Path.Combine(root, "credentials.json"), "private-secret");
            File.WriteAllText(Path.Combine(root, "ui-client.log"), "private-secret");
            CombatFailureEvidence.Collect(root, Path.Combine(root, "client.states.json"), Path.Combine(root, "observer.states.json"), Path.Combine(root, "authority.states.json"));
            string bundle = Path.Combine(root, "pose-failure");
            using JsonDocument proof = JsonDocument.Parse(File.ReadAllText(Path.Combine(bundle, "combat-pose-proof.json")));
            Assert.False(proof.RootElement.GetProperty("Exit").GetProperty("Recovery").GetBoolean());
            Assert.Equal(JsonValueKind.Null, proof.RootElement.GetProperty("RecoveryPrevious").ValueKind);
            Assert.Equal(JsonValueKind.Null, proof.RootElement.GetProperty("RecoveryCurrent").ValueKind);
            JsonElement trace = proof.RootElement.GetProperty("EarlyRecoveryTrace");
            Assert.Equal(2, trace.GetProperty("CallbackCount").GetInt64());
            Assert.False(trace.GetProperty("Truncated").GetBoolean());
            JsonElement[] callbacks = trace.GetProperty("Callbacks").EnumerateArray().ToArray();
            Assert.Equal(new long[] { 1, 2 }, callbacks.Select(c => c.GetProperty("Ordinal").GetInt64()));
            Assert.Equal<string?>(["actual-first", "actual-second"], callbacks.Select(c => c.GetProperty("Frame").GetProperty("ResponseId").GetString()));
            Assert.Equal(new double[] { 737, 790 }, callbacks.Select(c => c.GetProperty("Frame").GetProperty("Tick").GetDouble()));
            Assert.Equal(new long[] { 829, 882 }, callbacks.Select(c => c.GetProperty("Frame").GetProperty("Revision").GetInt64()));
            Assert.Equal<string?>(["early-live:locomotion", "early-live:attack"], callbacks.Select(c => c.GetProperty("Frame").GetProperty("Source").GetString()));
            Assert.All(callbacks, c =>
            {
                Assert.Equal(JsonValueKind.Null, c.GetProperty("RequestMonotonicTimestamp").ValueKind);
                Assert.Equal(JsonValueKind.Null, c.GetProperty("CallbackMonotonicTimestamp").ValueKind);
                Assert.True(c.GetProperty("WaitPredicateAccepted").GetBoolean());
                Assert.Equal("latest-arrival", c.GetProperty("Frame").GetProperty("ReceivedMatchId").GetString());
            });
            JsonElement[] prior = callbacks[1].GetProperty("PreviousCandidates").EnumerateArray().ToArray();
            Assert.Equal<int>([5, 6], prior.Select(p => p.GetProperty("Witness").GetProperty(nameof(CombatPoseWitness.Actor)).GetProperty("Id").GetInt32()));
            Assert.All(prior, p =>
            {
                Assert.True(p.GetProperty("WindowExpiredAtCallback").GetBoolean());
                Assert.Equal("actor-absent", p.GetProperty("Decision").GetString());
                Assert.Equal(780, p.GetProperty("Witness").GetProperty(nameof(CombatPoseWitness.Actor)).GetProperty("ReadyTick").GetInt64());
            });
            Assert.Equal("ineligible:action:Windup,before-impact", callbacks[1].GetProperty("Actors")[0].GetProperty("Decision").GetString());
            using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(bundle, "manifest.json")));
            JsonElement[] files = manifest.RootElement.GetProperty("Files").EnumerateArray().ToArray();
            Assert.Equal(23, files.Length);
            Assert.Equal("retained", files.Single(f => f.GetProperty("Name").GetString() == "combat-pose-proof.json").GetProperty("Status").GetString());
            Assert.False(File.Exists(Path.Combine(bundle, "credentials.json")));
            Assert.False(File.Exists(Path.Combine(bundle, "ui-client.log")));
            using JsonDocument state = JsonDocument.Parse(File.ReadAllText(Path.Combine(bundle, "ui-client.states.json")));
            Assert.Equal("[redacted]", state.RootElement.GetProperty("Token").GetString());
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void ProvenPairTransfersUnchangedAndLateCallbacksDoNotGrowEarlyTrace()
    {
        var diagnostic = new CombatPoseDiagnostic();
        var collector = new CombatRecoveryObservation(diagnostic);
        Assert.False(collector.Observe(Frame("first"), Received, "early-live:locomotion"));
        diagnostic.EarlyRecoveryTrace.PredicateResult(false);
        Assert.True(collector.Observe(Frame("second", 740, 832), Received, "early-live:attack"));
        diagnostic.EarlyRecoveryTrace.PredicateResult(true);
        Assert.True(collector.Observe(Frame("late", 790, 882) with { Paused = true }, Received, "current-poll"));
        using JsonDocument proof = JsonDocument.Parse(diagnostic.Finish(Flags(collector.Proven), "control"));
        Assert.Equal("first", proof.RootElement.GetProperty("RecoveryPrevious").GetProperty("ResponseId").GetString());
        Assert.Equal("second", proof.RootElement.GetProperty("RecoveryCurrent").GetProperty("ResponseId").GetString());
        Assert.Equal(2, diagnostic.EarlyRecoveryTrace.CallbackCount);
        Assert.Equal("accepted-pair", diagnostic.EarlyRecoveryTrace.Callbacks[1].Actors[0].Decision);
        Assert.Equal("accepted-pair", diagnostic.EarlyRecoveryTrace.Callbacks[1].PreviousCandidates[0].Decision);
        Assert.False(diagnostic.EarlyRecoveryTrace.Callbacks[0].WaitPredicateAccepted);
        Assert.True(diagnostic.EarlyRecoveryTrace.Callbacks[1].WaitPredicateAccepted);
    }

    [Fact]
    public void CountCapPreservesPrefixAndNeverAttachesDroppedPredicateToPriorCallback()
    {
        var diagnostic = new CombatPoseDiagnostic();
        var collector = new CombatRecoveryObservation(diagnostic);
        for (int i = 0; i < CombatRecoveryTrace.MaximumCallbacks + 3; i++)
        {
            collector.Observe(Frame("response-" + i, 800 + i, 900 + i) with
            {
                Units = i > CombatRecoveryTrace.MaximumCallbacks ? [Actor with { ImpactTick = 800, ReadyTick = 980 }] : []
            }, Received, "early-live:attack");
            diagnostic.EarlyRecoveryTrace.PredicateResult(i >= CombatRecoveryTrace.MaximumCallbacks);
        }
        CombatRecoveryTrace trace = diagnostic.EarlyRecoveryTrace;
        Assert.Equal(CombatRecoveryTrace.MaximumCallbacks, trace.Callbacks.Count);
        Assert.Equal(3, trace.DroppedCallbacks);
        Assert.Equal(2, trace.DroppedActors);
        Assert.Equal(1, trace.DroppedPriorCandidates);
        Assert.True(collector.Proven); // Diagnostic truncation cannot change acceptance.
        Assert.True(trace.Truncated);
        Assert.Equal(1, trace.FirstOrdinal);
        Assert.Equal(CombatRecoveryTrace.MaximumCallbacks + 3, trace.LastOrdinal);
        Assert.Equal(Enumerable.Range(1, CombatRecoveryTrace.MaximumCallbacks).Select(i => (long)i), trace.Callbacks.Select(c => c.Ordinal));
        Assert.All(trace.Callbacks, c => Assert.False(c.WaitPredicateAccepted));
        Assert.True(trace.DroppedBudgetBytes > 0);
        Assert.InRange(JsonSerializer.SerializeToUtf8Bytes(trace).LongLength, 0, trace.RetainedBudgetBytes);
    }

    [Fact]
    public void ByteCapIsExplicitAndDoesNotSilentlyApplyEightKiBWitnessFilter()
    {
        var diagnostic = new CombatPoseDiagnostic();
        var collector = new CombatRecoveryObservation(diagnostic);
        string largeResponseId = new('x', 9000);
        Assert.False(collector.Observe(Frame(largeResponseId), Received, "early-live:locomotion"));
        Assert.False(collector.Observe(Frame("expired", 790, 882), Received, "early-live:attack"));
        Assert.Equal(largeResponseId, diagnostic.EarlyRecoveryTrace.Callbacks[1].PreviousCandidates[0].Witness.ResponseId);
        Assert.Equal("ineligible:expired", diagnostic.EarlyRecoveryTrace.Callbacks[1].Actors[0].Decision);
        Assert.True(diagnostic.EarlyRecoveryTrace.Callbacks[1].PreviousCandidates[0].WindowExpiredAtCallback);
        for (int i = 0; i < 4; i++)
            collector.Observe(Frame("large-" + i, 800 + i, 900 + i) with { Units = [Actor with { Clip = new string('x', 90000) }] }, Received, "early-live:attack");
        CombatRecoveryTrace trace = diagnostic.EarlyRecoveryTrace;
        Assert.True(trace.Truncated);
        Assert.True(trace.DroppedCallbacks > 0);
        Assert.Equal(trace.DroppedCallbacks, trace.DroppedActors);
        Assert.InRange(trace.RetainedBudgetBytes, 1, CombatRecoveryTrace.MaximumBudgetBytes);
        Assert.True(trace.DroppedBudgetBytes > 0);
        using JsonDocument proof = JsonDocument.Parse(diagnostic.Finish(Flags(false), "failed"));
        Assert.Equal(largeResponseId, proof.RootElement.GetProperty("EarlyRecoveryTrace").GetProperty("Callbacks")[1].GetProperty("PreviousCandidates")[0].GetProperty("Witness").GetProperty("ResponseId").GetString());
        Assert.InRange(JsonSerializer.SerializeToUtf8Bytes(trace).LongLength, 0, trace.RetainedBudgetBytes);
    }

    [Theory]
    [InlineData("paused", "frame-ineligible:paused")]
    [InlineData("disconnected", "frame-ineligible:disconnected")]
    [InlineData("phase", "frame-ineligible:non-combat")]
    [InlineData("duplicate", "duplicate-response")]
    [InlineData("missing-id", "missing-response")]
    [InlineData("tick", "response-rejected:nonadvancing-tick")]
    [InlineData("revision", "response-rejected:regressing-revision")]
    public void FrameReturnPointsAreRecordedWithoutAdmittingRejectedSamples(string kind, string decision)
    {
        var diagnostic = new CombatPoseDiagnostic();
        var collector = new CombatRecoveryObservation(diagnostic);
        collector.Observe(Frame("first"), Received, "early-live:locomotion");
        UiObservation next = Frame("second", 740, 832);
        next = kind switch
        {
            "paused" => next with { Paused = true },
            "disconnected" => next with { Connected = false },
            "phase" => next with { MatchPhase = Phase.Building },
            "duplicate" => next with { Id = "first" },
            "missing-id" => next with { Id = "" },
            "tick" => next with { CombatTick = 737 },
            "revision" => next with { Revision = 828 },
            _ => throw new ArgumentException(kind)
        };
        Assert.False(collector.Observe(next, Received, "early-live:attack"));
        Assert.Equal(decision, diagnostic.EarlyRecoveryTrace.Callbacks[1].Decision);
        Assert.Equal("not-evaluated", diagnostic.EarlyRecoveryTrace.Callbacks[1].Actors[0].Decision);
        Assert.EndsWith("prior-cleared", diagnostic.EarlyRecoveryTrace.Callbacks[1].PreviousCandidates[0].Decision);
    }

    public static IEnumerable<object[]> ActorRejectionIndices() => ActorRejections().Select((_, index) => new object[] { index });
    private static IEnumerable<object[]> ActorRejections()
    {
        yield return [Actor with { Visible = false }, "hidden:not-evaluated"];
        yield return [Actor with { Dead = true }, "ineligible:dead"];
        yield return [Actor with { Health = 0 }, "ineligible:nonpositive-health"];
        yield return [Actor with { Hex = null }, "ineligible:missing-hex"];
        yield return [Actor with { Hex = Actor.Hex! with { Lifecycle = UnitLifecycle.Dying } }, "ineligible:non-alive-lifecycle"];
        yield return [Actor with { Hex = Actor.Hex! with { Action = UnitActionKind.Windup } }, "ineligible:action:Windup"];
        yield return [Actor with { Hex = Actor.Hex! with { FrozenTick = 737 } }, "ineligible:frozen-action"];
        yield return [Actor with { Hex = Actor.Hex! with { Id = 6 } }, "ineligible:actor-hex-mismatch"];
        yield return [Actor with { Hex = Actor.Hex! with { City = 2 } }, "ineligible:city-mismatch"];
        yield return [Actor with { AttackSequence = 0 }, "ineligible:noncurrent-attack"];
        yield return [Actor with { ImpactTick = 741 }, "ineligible:before-impact"];
        yield return [Actor with { ReadyTick = 740 }, "ineligible:expired"];
        yield return [Actor with { AttackActive = false }, "ineligible:inactive-attack-layer"];
        yield return [Actor with { Clip = "walk" }, "ineligible:noncombat-clip"];
        yield return [Actor with { ReadyTick = 781 }, "candidate-identity-mismatch:ready-tick"];
        yield return [Actor with { AttackSequence = 7 }, "candidate-identity-mismatch:attack-sequence"];
        yield return [Actor with { ImpactTick = 733 }, "candidate-identity-mismatch:impact-tick"];
        yield return [Actor with { Hex = Actor.Hex! with { ActionSequence = 12 } }, "candidate-identity-mismatch:action-sequence"];
    }

    [Theory]
    [MemberData(nameof(ActorRejectionIndices))]
    public void ActorRejectionDecisionsAndPriorSnapshotsRemainObservable(int index)
    {
        object[] example = ActorRejections().ElementAt(index);
        var actor = (UnitObservation)example[0];
        var decision = (string)example[1];
        var diagnostic = new CombatPoseDiagnostic();
        var collector = new CombatRecoveryObservation(diagnostic);
        collector.Observe(Frame("first"), Received, "early-live:locomotion");
        Assert.False(collector.Observe(Frame("second", 740, 832) with { Units = [actor] }, Received, "early-live:attack"));
        CombatRecoveryTrace.Callback trace = diagnostic.EarlyRecoveryTrace.Callbacks[1];
        Assert.Equal(decision, trace.Actors[0].Decision);
        Assert.Equal(780, trace.PreviousCandidates[0].Witness.Actor!.ReadyTick);
        Assert.Equal(6, trace.PreviousCandidates[0].Witness.Actor!.AttackSequence);
        Assert.Equal(11, trace.PreviousCandidates[0].Witness.Actor!.ActionSequence);
        Assert.Equal("completed", trace.Decision);
    }

    [Theory]
    [InlineData("match")]
    [InlineData("city")]
    [InlineData("wave")]
    [InlineData("playback-generation")]
    public void EveryWindowIdentityAxisReportsPriorClearWithoutProvingPair(string axis)
    {
        var diagnostic = new CombatPoseDiagnostic();
        var collector = new CombatRecoveryObservation(diagnostic);
        collector.Observe(Frame("first"), Received, "early-live:locomotion");
        UiObservation next = Frame("second", 740, 832);
        next = axis switch
        {
            "match" => next with { MatchId = "new-match" },
            "city" => next with { ObservedCity = 2 },
            "wave" => next with { Wave = 4 },
            "playback-generation" => next with { PlaybackGeneration = 5 },
            _ => throw new ArgumentException(axis)
        };
        Assert.False(collector.Observe(next, Received, "early-live:attack"));
        CombatRecoveryTrace.Callback trace = diagnostic.EarlyRecoveryTrace.Callbacks[1];
        Assert.False(trace.SameWindow);
        Assert.Equal(axis, Assert.Single(trace.WindowChanges));
        Assert.Equal("window-changed:prior-cleared", trace.PreviousCandidates[0].Decision);
    }

    [Theory]
    [InlineData(-.1f, 0)]
    [InlineData(.1f, 0)]
    [InlineData(0, -.1f)]
    [InlineData(0, .1f)]
    public void DriftExceptionRetainsBothActualActorsAndAxisWithoutRelaxingGuard(float dx, float dz)
    {
        var diagnostic = new CombatPoseDiagnostic();
        var collector = new CombatRecoveryObservation(diagnostic);
        collector.Observe(Frame("first"), Received, "early-live:locomotion");
        Assert.Throws<InvalidOperationException>(() => collector.Observe(Frame("second", 740, 832) with
        { Units = [Actor with { X = Actor.X + dx, Z = Actor.Z + dz }] }, Received, "early-live:attack"));
        CombatRecoveryTrace.Callback trace = diagnostic.EarlyRecoveryTrace.Callbacks[1];
        Assert.Equal("throw:position-drift", trace.Decision);
        Assert.Equal("position-drift:" + (dx != 0 ? "X" : "Z"), trace.Actors[0].Decision);
        Assert.Equal(Actor.X, trace.PreviousCandidates[0].Witness.Actor!.X);
        Assert.Equal(Actor.Z, trace.PreviousCandidates[0].Witness.Actor!.Z);
        Assert.False(collector.Proven);
    }

    [Theory]
    [InlineData("attack")]
    [InlineData("hit")]
    public void LayerExceptionIsRecordedBeforeOriginalThrow(string clip)
    {
        var diagnostic = new CombatPoseDiagnostic();
        var collector = new CombatRecoveryObservation(diagnostic);
        Assert.Throws<InvalidOperationException>(() => collector.Observe(Frame("bad-layer") with
        { Units = [Actor with { Clip = clip, AttackActive = false, HitActive = false }] }, Received, "early-live:attack"));
        Assert.Equal("throw:invalid-" + clip + "-layer", diagnostic.EarlyRecoveryTrace.Callbacks[0].Decision);
        Assert.False(collector.Proven);
    }

    [Fact]
    public void NoEarlyCallbackIsDistinguishableFromRejectedSamplesAndUnownedActorFieldsAreExcluded()
    {
        var diagnostic = new CombatPoseDiagnostic();
        var collector = new CombatRecoveryObservation(diagnostic);
        collector.Observe(Frame("late-only"), Received, "current-poll");
        using JsonDocument empty = JsonDocument.Parse(diagnostic.Finish(Flags(false), "failed"));
        Assert.Equal(0, empty.RootElement.GetProperty("EarlyRecoveryTrace").GetProperty("CallbackCount").GetInt64());
        Assert.Equal(JsonValueKind.Null, empty.RootElement.GetProperty("EarlyRecoveryTrace").GetProperty("FirstOrdinal").ValueKind);
        collector.Observe(Frame("actual", 740, 832) with
        {
            Units = [Actor with
            { BoneRotation = "private-secret", EquipmentRotation = "private-secret" }]
        }, Received, "early-live:attack");
        using JsonDocument populated = JsonDocument.Parse(diagnostic.Finish(Flags(collector.Proven), "control"));
        JsonElement actor = populated.RootElement.GetProperty("EarlyRecoveryTrace").GetProperty("Callbacks")[0].GetProperty("Actors")[0];
        Assert.False(actor.TryGetProperty("BoneRotation", out _));
        Assert.False(actor.TryGetProperty("EquipmentRotation", out _));
        Assert.False(actor.GetProperty(nameof(CombatPoseWitness.Actor)).TryGetProperty("BoneRotation", out _));
        Assert.False(actor.GetProperty(nameof(CombatPoseWitness.Actor)).TryGetProperty("EquipmentRotation", out _));
    }

    private static string Fixture()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "Odot.slnx"))) root = root.Parent;
        string path = Path.Combine(root!.FullName, "logs", "early-recovery-trace-control-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
