using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Game.Core;
using DevRunner;
using Xunit;

namespace DevRunner.Tests;

public sealed class HarnessTests
{
    [Fact]
    public void CasualtyCaptureCannotFreezeBeforeItsDamagedOpponentExists()
    {
        // Captured failure: W3 tick 769, a fresh corpse, but every living enemy
        // in the focused city was full-health; foreign damage is not sufficient.
        var corpse = new UnitState(5, 0) { Destination = 1, Hex = new(5, 1, Faction.Adventurers, UnitLifecycle.Dying, new(17, 1), DeathStartTick: 757, DeathEndTick: 805) };
        var full = new UnitState(41, 4000) { Destination = 1, Faction = Faction.Skeletons, Profile = new(4000, 1000, 1, 30, 12, 60) };
        var foreign = full with { Id = 46, Destination = 2, Health = 3800 };
        var state = new MatchSnapshot("capture", 1, 769, Phase.Combat, false, 3, 3, 12, new(), [], [full, foreign]) { DyingBodies = [corpse] };
        Assert.False(Runner.CasualtyInspectionReady(state, 1, 750));
        MatchSnapshot eligible = state with { Enemies = [full with { Health = 3000 }, foreign] };
        Assert.True(Runner.CasualtyInspectionReady(eligible, 1, 750));
        Assert.False(Runner.CasualtyInspectionReady(eligible, 1, 757));
        Assert.False(Runner.CasualtyInspectionReady(eligible with { Tick = 793 }, 1, 750));
        Assert.False(Runner.CasualtyInspectionReady(eligible with { DyingBodies = [] }, 1, 750));
        Assert.False(Runner.CasualtyInspectionReady(eligible with { Enemies = [full with { Health = 0 }, foreign] }, 1, 750));
    }

    [Fact]
    public void HistoricalCasualtyEligibilityDoesNotEstablishActualPauseReceiptEligibility()
    {
        // Retained latest-CI pause: tick827/revision922, focused city1 has a
        // fresh corpse but living enemies44/45 are full; damaged46/47 are city2.
        // Earlier eligibility is controlled data, not a reconstructed timestamp.
        var corpse = new UnitState(41, 0)
        {
            Destination = 1,
            Faction = Faction.Skeletons,
            Hex = new(41, 1, Faction.Skeletons, UnitLifecycle.Dying, new(11, 1), DeathStartTick: 818, DeathEndTick: 866)
        };
        var full = new UnitState(44, 3000) { Destination = 1, Faction = Faction.Skeletons, Profile = new(3000, 1000, 1, 30, 12, 60) };
        var foreign = new UnitState(46, 600) { Destination = 2, Faction = Faction.Skeletons, Profile = new(4000, 1000, 1, 30, 12, 60) };
        var actualPause = new MatchSnapshot("current-casualty", 922, 827, Phase.Combat, true, 3, 3, 12, new(), [], [full, foreign]) { DyingBodies = [corpse] };
        MatchSnapshot earlier = actualPause with { Revision = 918, Tick = 824, Paused = false, Enemies = [full with { Health = 2000 }, foreign] };
        Assert.True(Runner.CasualtyInspectionReady(earlier, 1, 800));
        Assert.False(Runner.CasualtyInspectionReady(actualPause, 1, 800));
        Assert.Contains(actualPause.DyingBodies, u => u.Destination == 1 && u.Hex!.DeathEndTick > actualPause.Tick + 12);
        Assert.Contains(actualPause.Enemies, u => u.Health > 0 && u.Health < u.Profile.Health);
        Assert.DoesNotContain(actualPause.Enemies, u => u.Destination == 1 && u.Health > 0 && u.Health < u.Profile.Health);
    }

    [Fact]
    public void EconomyObservationRoundTripPreservesIncomeUpkeepAndFullBounds()
    {
        var original = new UiObservation
        {
            ResourceIncome = new() { ["Gold"] = "+2", ["Food"] = "0", ["Wood"] = "Unavailable" },
            IncomeLabel = "Next building turn",
            IncomeContext = "Stale · waiting for connection",
            CompactUpkeep = ["Paid this battle · W3", "4 food", "Sat out", "2"],
            Targets = new() { ["UpkeepTable"] = new(1000, 220, true, true) { Width = 240, Height = 44 }, ["LumbermillRecovery"] = new(500, 670, true, false) { Tooltip = "Need 1 more gold" } }
        };
        UiObservation parsed = JsonSerializer.Deserialize<UiObservation>(JsonSerializer.Serialize(original, WireJson.Options), WireJson.Options)!;
        Assert.Equal(original.ResourceIncome, parsed.ResourceIncome); Assert.Equal(original.CompactUpkeep, parsed.CompactUpkeep);
        Assert.Equal(original.IncomeLabel, parsed.IncomeLabel); Assert.Equal(original.IncomeContext, parsed.IncomeContext);
        Assert.Equal(original.Targets["UpkeepTable"], parsed.Targets["UpkeepTable"]);
        Assert.Equal("Need 1 more gold", parsed.Targets["LumbermillRecovery"].Tooltip);
    }
    [Fact]
    public void DeathCleanupUsesActualRemovalTimeEvenWhenTheProbeArrivesLate()
    {
        var frame = new UiObservation
        {
            CombatTick = 1104,
            VisualSeconds = 23.4,
            DeathCleanups = [new(44, 976, 977, 21.4)]
        };
        UiObservation parsed = JsonSerializer.Deserialize<UiObservation>(JsonSerializer.Serialize(frame, WireJson.Options), WireJson.Options)!;
        Assert.Equal(frame.DeathCleanups, parsed.DeathCleanups);
        DeathCleanupObservation cleanup = UiProtocol.DeathCleanup(parsed, 44, 976, 20.6);
        Assert.Equal(21.4, cleanup.VisualSeconds);
        Assert.True(parsed.VisualSeconds - 20.6 > 2);
        Assert.Equal(cleanup, UiProtocol.DeathCleanup(parsed with { VisualSeconds = 100, CombatTick = 10000 }, 44, 976, 20.6));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(2.001)]
    public void DeathCleanupRetainsTheExactTwoUnpausedSecondBound(double elapsed)
    {
        var frame = new UiObservation { CombatTick = 100, VisualSeconds = 20, DeathCleanups = [new(7, 48, 48, elapsed)] };
        if (elapsed == 2) Assert.Equal(elapsed, UiProtocol.DeathCleanup(frame, 7, 48, 0).VisualSeconds);
        else Assert.Throws<InvalidOperationException>(() => UiProtocol.DeathCleanup(frame, 7, 48, 0));
    }

    [Fact]
    public void DeathCleanupRejectsAbsentWrongPrematureAndStillRenderedWitnesses()
    {
        var frame = new UiObservation { CombatTick = 100, VisualSeconds = 20, DeathCleanups = [new(7, 48, 48, 1)] };
        Assert.Throws<InvalidOperationException>(() => UiProtocol.DeathCleanup(frame with { DeathCleanups = [] }, 7, 48, 0));
        Assert.Throws<InvalidOperationException>(() => UiProtocol.DeathCleanup(frame, 8, 48, 0));
        Assert.Throws<InvalidOperationException>(() => UiProtocol.DeathCleanup(frame, 7, 49, 0));
        Assert.Throws<InvalidOperationException>(() => UiProtocol.DeathCleanup(frame with { DeathCleanups = [new(7, 48, 47, 1)] }, 7, 48, 0));
        Assert.Throws<InvalidOperationException>(() => UiProtocol.DeathCleanup(frame with { Units = [new() { Id = 7, Dead = true }] }, 7, 48, 0));
        Assert.Throws<InvalidOperationException>(() => UiProtocol.DeathCleanup(frame, 7, 48, 2));
    }

    [Fact]
    public void FullSourceUiHasABoundedAggregateBudgetAndPreservesOverrides()
    {
        Assert.Equal(900000, Options.Parse(["ci"]).Timeout);
        Assert.Equal(900000, Options.Parse(["ci-source"]).Timeout);
        Assert.Equal(600000, Options.Parse(["test-ui"]).Timeout);
        Assert.Equal(300000, Options.Parse(["test-ui", "--scenario", "combat"]).Timeout);
        Assert.Equal(180000, Options.Parse(["test-ui", "--scenario", "combat", "--ui-checkpoint", "research"]).Timeout);
        Assert.Equal("research", Options.Parse(["test-ui", "--scenario", "combat", "--ui-checkpoint", "research"]).UiCheckpoint);
        Assert.Equal(300000, Options.Parse(["test-ui", "--scenario", "economy"]).Timeout);
        Assert.Equal(300000, Options.Parse(["test-network"]).Timeout);
        Assert.Equal(42, Options.Parse(["ci", "--timeout-ms", "42"]).Timeout);
        Assert.Equal(300000, Options.Parse(["_ui-worker", "--timeout-ms", "300000"]).Timeout);
    }
    [Fact]
    public void CombatIsSelectableSourceCoverageAndPackagesRemainExplicit()
    {
        Assert.Equal("combat", Options.Parse(["test-ui", "--scenario", "combat"]).Scenario);
        string[] expected = ["economy", "reconnect", "settings", "launcher", "combat"];
        Assert.Equal(expected, ScenarioNames.Ui.Where(n => n is not ("exported-package" or "installed-linux")));
        Assert.Equal("exported-package", Options.Parse(["test-ui", "--scenario", "exported-package"]).Scenario);
        Assert.Throws<ArgumentException>(() => Options.Parse(["test-network", "--scenario", "combat"]));
    }
    [Fact]
    public void EarlyMeleeCheckpointIsScopedToTheExistingCombatSlice()
    {
        Assert.Equal("army", Options.Parse(["test-ui", "--scenario", "economy", "--checkpoint", "army"]).UiCheckpoint);
        Assert.Equal("army", Options.Parse(["_ui-worker", "--scenario", "economy", "--checkpoint", "army"]).UiCheckpoint);
        Assert.Throws<ArgumentException>(() => Options.Parse(["test-ui", "--scenario", "combat", "--checkpoint", "army"]));
        Assert.Equal("melee", Options.Parse(["test-ui", "--scenario", "combat", "--checkpoint", "melee"]).UiCheckpoint);
        Assert.Equal("melee", Options.Parse(["_ui-worker", "--scenario", "combat", "--checkpoint", "melee"]).UiCheckpoint);
        Assert.Throws<ArgumentException>(() => Options.Parse(["test-ui", "--checkpoint", "melee"]));
        Assert.Throws<ArgumentException>(() => Options.Parse(["test-ui", "--scenario", "reconnect", "--checkpoint", "melee"]));
        Assert.Throws<ArgumentException>(() => Options.Parse(["test-ui", "--scenario", "combat", "--checkpoint", "unknown"]));
        Assert.Throws<ArgumentException>(() => Options.Parse(["test-network", "--checkpoint", "melee"]));
    }
    [Fact]
    public void RiggedObservationPreservesStringEnumsAndActualPoseFields()
    {
        const string json = """
            {"Units":[{"Id":7,"Type":"Crossbowman","Clip":"2H_Ranged_Shoot","PoseSeconds":0.43,"BoneRotation":"(0, 1, 0, 0)","WeaponAttached":true,"Dead":false,"ShotVisible":true}],"EventCursor":12,"CombatTick":44,"PlaybackGeneration":2}
            """;
        UiObservation observation = JsonSerializer.Deserialize<UiObservation>(json, WireJson.Options)!;
        UnitObservation unit = Assert.Single(observation.Units);
        Assert.Equal(UnitType.Crossbowman, unit.Type); Assert.True(unit.WeaponAttached); Assert.True(unit.ShotVisible);
        Assert.Equal(0.43, unit.PoseSeconds); Assert.Equal(12, observation.EventCursor); Assert.Equal(2, observation.PlaybackGeneration);
    }
    [Fact]
    public void DevelopmentCapacityIncludesThePlayingHost()
    {
        Assert.Equal(1, Options.Parse(["dev"]).Guests);
        Assert.Equal(3, Options.Parse(["dev", "--guests", "3"]).Guests);
        Assert.Throws<ArgumentException>(() => Options.Parse(["dev", "--guests", "4"]));
        Assert.Throws<ArgumentException>(() => Options.Parse(["server", "--guests", "1"]));
    }

    [Fact]
    public void PairedSteamVerificationRequiresAnExplicitSide()
    {
        Assert.Throws<ArgumentException>(() => Options.Parse(["test-steam"]));
        Assert.Throws<ArgumentException>(() => Options.Parse(["test-steam", "--role", "host", "--lobby", "100"]));
        var guest = Options.Parse(["test-steam", "--role", "guest", "--lobby", "100", "--exported"]);
        Assert.Equal("guest", guest.SteamRole); Assert.Equal(100UL, guest.Lobby); Assert.True(guest.Exported);
        var direct = Options.Parse(["test-steam", "--role", "guest", "--scenario", "direct-invite"]);
        Assert.Equal("direct-invite", direct.Scenario); Assert.Null(direct.Lobby);
        Assert.Throws<ArgumentException>(() => Options.Parse(["test-steam", "--role", "guest", "--scenario", "direct-invite", "--lobby", "100"]));
    }

    [Fact]
    public void StartupExtensionsTrackActualProjectDescriptorsWithoutTouchingOtherCacheFiles()
    {
        string project = Path.Combine(Path.GetTempPath(), "odot-import-fixture-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(project, "addons", "steam"));
            Directory.CreateDirectory(Path.Combine(project, "addons", "another"));
            Directory.CreateDirectory(Path.Combine(project, ".godot"));
            File.WriteAllText(Path.Combine(project, "addons", "steam", "steam.gdextension"), "descriptor");
            File.WriteAllText(Path.Combine(project, "addons", "another", "another.gdextension"), "descriptor");
            string list = Path.Combine(project, ".godot", "extension_list.cfg");
            string cache = Path.Combine(project, ".godot", "filesystem_cache10");
            File.WriteAllText(list, "res://removed.gdextension\n");
            File.WriteAllText(cache, "existing imports");
            EditorImports.SeedStartupExtensions(project);
            Assert.Equal(["res://addons/another/another.gdextension", "res://addons/steam/steam.gdextension"], File.ReadAllLines(list));
            Assert.Equal("existing imports", File.ReadAllText(cache));
            string content = File.ReadAllText(list);
            EditorImports.SeedStartupExtensions(project);
            Assert.Equal(content, File.ReadAllText(list));
        }
        finally { if (Directory.Exists(project)) Directory.Delete(project, true); }
    }

    [Fact]
    public void StartupExtensionsRespectIgnoredAndGeneratedDirectories()
    {
        string project = Path.Combine(Path.GetTempPath(), "odot-import-fixture-" + Guid.NewGuid().ToString("N"));
        try
        {
            foreach (string directory in new[] { ".godot", "excluded", "nested-project" })
            {
                Directory.CreateDirectory(Path.Combine(project, directory));
                File.WriteAllText(Path.Combine(project, directory, "hidden.gdextension"), "descriptor");
            }
            File.WriteAllText(Path.Combine(project, "excluded", ".gdignore"), "");
            File.WriteAllText(Path.Combine(project, "nested-project", "project.godot"), "");
            File.WriteAllText(Path.Combine(project, ".hidden.gdextension"), "descriptor");
            EditorImports.SeedStartupExtensions(project);
            Assert.Empty(File.ReadAllText(Path.Combine(project, ".godot", "extension_list.cfg")));
        }
        finally { if (Directory.Exists(project)) Directory.Delete(project, true); }
    }

    [Theory]
    [InlineData("export-client", "--production")]
    [InlineData("export-client", "--production", "--steam-app-id", "480")]
    [InlineData("export-client", "--steam-app-id", "0")]
    public void InvalidSteamPackageIdentityFailsBeforeExport(params string[] args)
    {
        Assert.Throws<ArgumentException>(() => Options.Parse(args));
    }

    [Fact]
    public void SteamChecksAndProductionAreExplicitIndependentOptions()
    {
        Assert.Null(Options.Parse(["export-client"]).SteamAppId);
        var check = Options.Parse(["check-steam-extension", "--offline", "--exported"]);
        Assert.True(check.Offline); Assert.True(check.Exported);
        var production = Options.Parse(["export-client", "--production", "--steam-app-id", "123456"]);
        Assert.True(production.Production); Assert.Equal(123456u, production.SteamAppId);
        Assert.Throws<ArgumentException>(() => Options.Parse(["ci", "--production"]));
        Assert.Throws<ArgumentException>(() => Options.Parse(["client", "--offline"]));
    }

    [Fact]
    public void NumericRunnerArgumentsUseInvariantCulture()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        var culture = (CultureInfo)original.Clone();
        culture.NumberFormat.PositiveSign = "plus";
        try
        {
            CultureInfo.CurrentCulture = culture;
            Options options = Options.Parse(["test-network", "--jobs", "+2", "--startup-timeout-ms", "+15000"]);
            Assert.Equal(2, options.Jobs);
            Assert.Equal(15000, options.StartupTimeout);
        }
        finally { CultureInfo.CurrentCulture = original; }
    }
    [Theory]
    [InlineData("rules")]
    [InlineData("network")]
    [InlineData("source-ui")]
    public async Task FailedSourceGatePreventsBothExports(string condition)
    {
        bool exported = false, packaged = false;
        await Assert.ThrowsAsync<InvalidOperationException>(() => VerificationGate.Run(() => Task.FromException(new InvalidOperationException(condition)),
            () => { exported = true; return Task.CompletedTask; }, () => { packaged = true; return Task.CompletedTask; }));
        Assert.False(exported); Assert.False(packaged);
    }
    [Fact]
    public async Task ExportedGraphicalFailurePreventsOverallSuccess()
    {
        bool exported = false;
        var failure = await Assert.ThrowsAsync<InvalidOperationException>(() => VerificationGate.Run(() => Task.CompletedTask,
            () => { exported = true; return Task.CompletedTask; }, () => Task.FromException(new InvalidOperationException("packed UI failure"))));
        Assert.True(exported); Assert.Equal("packed UI failure", failure.Message);
    }
    [Fact]
    public async Task OwnedGroupCleanupStopsDescendantsWhenWrapperAlreadyExited()
    {
        string root = Path.Combine(Path.GetTempPath(), "odot-group-fixture-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await using var child = new Child("wrapper", "setsid", ["/bin/sh", "-c", "printf 'ODOT_EVENT {\"Type\":\"ready\"}\\n'; sleep 30 &"], root, ownsGroup: true);
            await child.WaitFor(e => e.Type == "ready", "fixture readiness", 2000, CancellationToken.None);
            await child.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(child.HasExited);
        }
        finally { Directory.Delete(root, true); }
    }
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task SchedulerBoundsWorkersAndRunsEveryCase(int jobs)
    {
        int active = 0, max = 0, completed = 0;
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var scenarios = Enumerable.Range(0, 4).Select(n => new Scenario(n.ToString(CultureInfo.InvariantCulture), "fixture", async token =>
        {
            int count = Interlocked.Increment(ref active);
            Interlocked.Exchange(ref max, Math.Max(max, count));
            if (count == jobs) started.TrySetResult();
            await release.Task.WaitAsync(token);
            Interlocked.Decrement(ref active); Interlocked.Increment(ref completed);
        })).ToArray();
        Task run = ScenarioScheduler.Run(scenarios, jobs, CancellationToken.None);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(3));
        Assert.Equal(jobs, active);
        release.SetResult(); await run.WaitAsync(TimeSpan.FromSeconds(3));
        Assert.Equal(jobs, max); Assert.Equal(4, completed);
    }
    [Fact]
    public async Task FailureCancelsSiblingAwaitsCleanupAndStopsQueuedWork()
    {
        bool cleaned = false, queued = false;
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Scenario[] scenarios = [
            new("active", "fixture", async token => { started.SetResult(); try { await Task.Delay(Timeout.Infinite, token); } finally { cleaned = true; } }),
            new("origin", "fixture", async _ => { await started.Task; throw new InvalidOperationException("original assertion"); }),
            new("queued", "fixture", _ => { queued = true; return Task.CompletedTask; })];
        var failure = await Assert.ThrowsAsync<InvalidOperationException>(() => ScenarioScheduler.Run(scenarios, 2, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(3)));
        Assert.Contains("origin: original assertion", failure.Message);
        Assert.True(cleaned); Assert.False(queued);
    }
    [Theory]
    [InlineData("test-network", "--jobs", "0")]
    [InlineData("test-network", "--jobs", "-1")]
    [InlineData("test-network", "--scenario", "unknown")]
    [InlineData("test-ui", "--scenario", "unknown")]
    [InlineData("ci", "--scenario", "economy")]
    [InlineData("test-ui", "--engine-arg", "--headless")]
    public void InvalidSelectionFailsBeforeAnyProcess(string command, string flag, string value)
        => Assert.Throws<ArgumentException>(() => Options.Parse([command, flag, value]));
    [Fact]
    public void SelectionAndExplicitEndpointContract()
    {
        Options selected = Options.Parse(["test-network", "--scenario", "defeat", "--jobs", "1"]);
        Assert.Equal("defeat", selected.Scenario); Assert.Equal(1, selected.Jobs);
        Assert.Equal(2, Options.Parse(["test-network"]).Jobs);
        Assert.Throws<ArgumentException>(() => Options.Parse(["test-network", "--scenario", "defeat", "--port", "7001"]));
        Assert.Equal(7001, Options.Parse(["test-network", "--scenario", "authority-resume-victory", "--port", "7001"]).Port);
    }
    [Fact]
    public void MissingDisplayPrerequisitesFailWithoutFallback()
    {
        var error = Assert.Throws<VerificationPrerequisiteException>(() => PrivateDisplay.CheckPrerequisites(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))));
        Assert.Contains("Xvfb", error.Message); Assert.Contains("No desktop fallback", error.Message);
        Assert.Throws<InvalidOperationException>(() => PrivateDisplay.ValidateWorker(Options.Parse(["_ui-worker"])));
        Assert.Throws<InvalidOperationException>(() => PrivateDisplay.RequireSoftwareGraphics("OpenGL version string: 4.6\nOpenGL renderer string: hardware GPU"));
    }
    [Fact]
    public async Task MissingPrerequisitesAreRecordedAsUnexecuted()
    {
        string root = Path.Combine(Path.GetTempPath(), "odot-prerequisite-" + Guid.NewGuid().ToString("N"));
        try
        {
            var evidence = new Evidence(root);
            await Assert.ThrowsAsync<VerificationPrerequisiteException>(() => evidence.Measure("display", "ui",
                () => { PrivateDisplay.CheckPrerequisites(Path.Combine(root, "missing-tools")); return Task.CompletedTask; }));
            using var report = System.Text.Json.JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(evidence.Directory, "display.json")));
            Assert.Equal("unexecuted", report.RootElement.GetProperty("Result").GetString());
            Assert.Contains("prerequisites", report.RootElement.GetProperty("Condition").GetString());
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
    [Fact]
    public async Task OwnedStateIsIsolatedAndCleanupPreservesUnrelatedProcess()
    {
        string root = Path.Combine(Path.GetTempPath(), "odot-fixture-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        using var unrelated = Process.Start("sleep", "30")!;
        try
        {
            var evidence = new Evidence(root);
            var scope = new ScenarioScope("fixture", evidence);
            string runtime = scope.Directory;
            Assert.Equal(scope.EnvironmentFor("a")["XDG_DATA_HOME"], scope.EnvironmentFor("a")["XDG_DATA_HOME"]);
            Assert.NotEqual(scope.EnvironmentFor("a")["XDG_DATA_HOME"], scope.EnvironmentFor("b")["XDG_DATA_HOME"]);
            Child owned = scope.Own(new Child("owned", "/bin/sh", ["-c", "printf 'ODOT_EVENT {\"Type\":\"ready\"}\\n'; sleep 30"], root, environment: scope.EnvironmentFor("a"), evidenceDirectory: scope.EvidenceDirectory));
            await owned.WaitFor(e => e.Type == "ready", "fixture readiness", 2000, CancellationToken.None);
            File.WriteAllText(Path.Combine(scope.EnvironmentFor("a")["XDG_DATA_HOME"]!, "settings.cfg"), "owned setting");
            await scope.DisposeAsync(); await scope.DisposeAsync();
            Assert.True(owned.HasExited); Assert.False(Directory.Exists(runtime)); Assert.False(unrelated.HasExited);
            Assert.True(File.Exists(owned.LogPath));
        }
        finally { if (!unrelated.HasExited) unrelated.Kill(); await unrelated.WaitForExitAsync(); Directory.Delete(root, true); }
    }
    [Fact]
    public async Task UiRequestIdsRejectStaleResponsesAndPropagateCaptureFailure()
    {
        string root = Path.Combine(Path.GetTempPath(), "odot-ui-fixture-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await using var child = new Child("probe", "/bin/sh", ["-c", "printf 'ODOT_UI {\"Id\":\"stale\"}\\n'; read -r command id; printf 'ODOT_UI {\"Id\":\"%s\",\"Error\":\"capture failed\"}\\n' \"$id\""], root);
            var error = await Assert.ThrowsAsync<InvalidOperationException>(() => UiProtocol.Probe(child, 2000, CancellationToken.None));
            Assert.Contains("capture failed", error.Message);
        }
        finally { Directory.Delete(root, true); }
    }
    [Fact]
    public async Task UnexpectedEngineErrorsAndMissingFramesFail()
    {
        string root = Path.Combine(Path.GetTempPath(), "odot-error-fixture-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await using var scope = new ScenarioScope("fixture", new Evidence(root));
            var child = scope.Own(new Child("error", "/bin/sh", ["-c", "echo 'ERROR: fixture engine error' >&2"], root));
            await child.WaitExit(CancellationToken.None); await scope.DisposeAsync();
            Assert.Throws<InvalidOperationException>(scope.CheckErrors);
            Assert.Throws<InvalidOperationException>(() => UiProtocol.Target(new UiObservation(), "Start"));
            Assert.Throws<InvalidOperationException>(() => UiProtocol.Frame(new UiObservation(), Path.Combine(root, "missing.png")));
        }
        finally { Directory.Delete(root, true); }
    }
}
