using Game;
using Xunit;

namespace DevRunner.Tests;

public sealed class WindowsGraphicsFactAttribute : FactAttribute
{
    public WindowsGraphicsFactAttribute()
    {
        if (!OperatingSystem.IsWindows()) Skip = "Requires native Windows command-line APIs; Linux execution is not Windows coverage.";
    }
}

public sealed class GraphicsTests
{
    [Fact]
    public void MissingOrInvalidSavedValuesUseSafeDefaultsIndependently()
    {
        var defaults = new GraphicsPreferences();
        foreach (object? invalid in new object?[] { null, "high", true, 1.5, -1L, long.MaxValue })
        {
            GraphicsPreferences loaded = GraphicsPreferences.Load(_ => invalid);
            Assert.Equal(defaults.Values, loaded.Values); Assert.Equal(100, loaded.Exposure);
        }
        foreach (GraphicsOption option in GraphicsPreferences.Options)
            for (int value = 0; value < option.Choices.Length; value++)
            {
                var loaded = GraphicsPreferences.Load(key => key == option.Key ? (long)value : null);
                Assert.Equal(value, loaded[option.Key]);
            }
        Assert.Equal(80, GraphicsPreferences.Load(key => key == "exposure" ? 80L : null).Exposure);
        Assert.Equal(120, GraphicsPreferences.Load(key => key == "exposure" ? 120L : null).Exposure);
        Assert.Equal(100, GraphicsPreferences.Load(key => key == "exposure" ? 121L : null).Exposure);
    }
    [Theory]
    [InlineData(0, 1, 0, 0, 0)]
    [InlineData(1, 2, 0, 1, 0)]
    [InlineData(2, 3, 1, 1, 0)]
    [InlineData(3, 3, 2, 1, 1)]
    public void TiersCoordinateCostWithoutChangingRendererWeatherOrComfort(int tier, int shadows, int ao, int volumetric, int indirect)
    {
        var preferences = new GraphicsPreferences();
        preferences.Edit("renderer", 1); preferences.Edit("weather", 4); preferences.Edit("reduce_motion", 1); preferences.Edit("fps", 2);
        preferences.Edit("quality", tier);
        Assert.Equal(tier, preferences["quality"]); Assert.Equal(shadows, preferences["shadows"]);
        Assert.Equal(ao, preferences["ao"]); Assert.Equal(volumetric, preferences["volumetric"]); Assert.Equal(indirect, preferences["indirect"]);
        Assert.Equal(1, preferences["renderer"]); Assert.Equal(4, preferences["weather"]); Assert.Equal(1, preferences["reduce_motion"]); Assert.Equal(2, preferences["fps"]);
    }
    [Fact]
    public void IndividualEditsSelectCustomWithoutResettingOtherCurrentValues()
    {
        var preferences = new GraphicsPreferences(); preferences.Edit("quality", 3);
        var before = preferences.Clone(); preferences.Edit("fog", 1);
        Assert.Equal(4, preferences["quality"]); Assert.Equal(1, preferences["fog"]);
        foreach (var entry in before.Values.Where(p => p.Key is not ("quality" or "fog"))) Assert.Equal(entry.Value, preferences[entry.Key]);
        preferences.SetExposure(999); Assert.Equal(120, preferences.Exposure);
        preferences.SetExposure(-4); Assert.Equal(80, preferences.Exposure);
        Assert.Equal(100, before.Exposure);
    }
    [Fact]
    public void CompatibilityFallbackNeverMutatesDesiredPreferences()
    {
        var desired = new GraphicsPreferences(); desired.Edit("quality", 3);
        GraphicsPreferences effective = desired.Effective(false);
        Assert.False(GraphicsPreferences.Supported("aa", 1, false));
        Assert.False(GraphicsPreferences.Supported("aa", 2, false));
        Assert.False(GraphicsPreferences.Supported("volumetric", 1, false));
        Assert.False(GraphicsPreferences.Supported("indirect", 1, false));
        Assert.True(GraphicsPreferences.Supported("ao", 2, false)); // Locked 4.7 supports Compatibility SSAO.
        Assert.Equal(0, effective["volumetric"]); Assert.Equal(0, effective["indirect"]); Assert.Equal(3, effective["aa"]);
        Assert.Equal(1, desired["volumetric"]); Assert.Equal(1, desired["indirect"]); Assert.Equal(2, desired["aa"]);
        Assert.Equal("forward_plus", desired.DesiredMethod);
        Assert.Equal(desired.Values, desired.Effective(true).Values);
    }
    [Fact]
    public void DesktopVulkanIsExplicitAndNeverAnOrdinaryCiFallback()
    {
        Assert.True(Runner.OwnsVerificationCommand("test-vulkan"));
        Assert.True(Runner.OwnsVerificationCommand("test-atmosphere"));
        Assert.False(Runner.OwnsVerificationCommand("play"));
        Assert.False(Runner.OwnsVerificationCommand("dev"));
        Assert.True(Options.Parse(["test-vulkan", "--desktop"]).DesktopSurface);
        Assert.True(Options.Parse(["test-vulkan", "--desktop", "--exported"]).Exported);
        Assert.False(Options.Parse(["test-vulkan"]).DesktopSurface);
        Assert.Throws<ArgumentException>(() => Options.Parse(["ci", "--desktop"]));
        Assert.Throws<ArgumentException>(() => Options.Parse(["test-atmosphere", "--desktop"]));
    }
    [Theory]
    [InlineData("popup-diagnostic")]
    [InlineData("vanilla-popup")]
    [InlineData("vanilla-native")]
    public void PopupDiagnosticsRequireExplicitWindowedDesktopAuthority(string checkpoint)
    {
        Assert.Equal(checkpoint, Options.Parse(["test-vulkan", "--desktop", "--exported", "--ui-checkpoint", checkpoint]).UiCheckpoint);
        Assert.Throws<ArgumentException>(() => Options.Parse(["test-vulkan", "--ui-checkpoint", checkpoint]));
        Assert.Throws<ArgumentException>(() => Options.Parse(["ci", "--ui-checkpoint", checkpoint]));
    }
    [Fact]
    public void CompatibilityTwoSampleChoiceUsesOffWithoutLosingDesiredOrOtherEffects()
    {
        var desired = new GraphicsPreferences();
        GraphicsPreferences effective = desired.Effective(false);
        Assert.Equal(3, desired["aa"]); Assert.Equal(0, effective["aa"]);
        foreach (var entry in desired.Values.Where(p => p.Key is not ("aa" or "volumetric" or "indirect"))) Assert.Equal(entry.Value, effective[entry.Key]);
        Assert.Equal(desired.Values, desired.Effective(true).Values);
        desired.Edit("fps", 0);
        Assert.Equal(4, desired["quality"]); Assert.Equal(0, desired.Effective(false)["aa"]);
        desired.Edit("aa", 4);
        Assert.Equal(4, desired.Effective(false)["aa"]);
        Assert.Equal("travel-diagnostic", Options.Parse(["test-ui", "--scenario", "economy", "--checkpoint", "travel-diagnostic"]).UiCheckpoint);
        Assert.Equal("travel", Options.Parse(["test-ui", "--scenario", "economy", "--checkpoint", "travel"]).UiCheckpoint);
    }
    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(1, 0, false)]
    [InlineData(2, 3, false)]
    [InlineData(3, 0, true)]
    [InlineData(4, 4, true)]
    public void ActualCompatibilityAaPolicyHasNoUnsupportedScreenSpaceModes(int choice, int effective, bool supported)
    {
        var desired = new GraphicsPreferences(); desired.Edit("aa", choice);
        Assert.Equal(choice, desired["aa"]); Assert.Equal(choice, desired.Effective(true)["aa"]);
        Assert.Equal(effective, desired.Effective(false)["aa"]);
        Assert.Equal(supported, GraphicsPreferences.Supported("aa", choice, false));
    }
    [Fact]
    public void RequestedHighKeepsItsSupportedCompatibilityValues()
    {
        var desired = new GraphicsPreferences(); desired.Edit("quality", 2);
        GraphicsPreferences effective = desired.Effective(false);
        Assert.Equal(2, effective["quality"]); Assert.Equal(3, effective["shadows"]);
        Assert.Equal(4, effective["aa"]); Assert.Equal(1, effective["ao"]); Assert.Equal(2, effective["fog"]);
        Assert.Equal(1, effective["rain"]); Assert.Equal(1, effective["particles"]);
        Assert.Equal(0, effective["volumetric"]); Assert.Equal(0, effective["indirect"]);
    }
    [Fact]
    public async Task GraphicalChildCannotReachTheInheritedDefaultWaylandSocket()
    {
        string directory = Path.Combine(Path.GetTempPath(), "odot-graphics-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            var evidence = new Evidence(directory, Path.Combine(directory, "evidence"));
            await using var owned = new ScenarioScope("graphics", evidence, graphical: true, runtimeRoot: directory);
            Dictionary<string, string?> environment = owned.EnvironmentFor("client");
            string runtime = environment["XDG_RUNTIME_DIR"]!;
            Assert.True(Directory.Exists(runtime)); Assert.False(File.Exists(environment["WAYLAND_DISPLAY"]!));
            Assert.Equal(Path.Combine(runtime, "no-wayland-fallback"), environment["WAYLAND_DISPLAY"]);
            Assert.StartsWith(owned.Directory, runtime, StringComparison.Ordinal);
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }
    [Fact]
    public void PrivateX11ConsentGateAdmitsOnlyTheAbsentOwnedWaylandPath()
    {
        string owner = Path.Combine(Path.GetTempPath(), "odot-display-gate-" + Guid.NewGuid().ToString("N"));
        string runtime = Path.Combine(owner, "scenario", "client", "runtime");
        string socket = Path.Combine(runtime, "no-wayland-fallback");
        Directory.CreateDirectory(runtime);
        try
        {
            Assert.True(OwnedDisplayIsolation.AbsentWaylandSocket(null, null, null));
            Assert.True(OwnedDisplayIsolation.AbsentWaylandSocket(socket, runtime, owner));
            Assert.False(OwnedDisplayIsolation.AbsentWaylandSocket("wayland-0", runtime, owner));
            Assert.False(OwnedDisplayIsolation.AbsentWaylandSocket(Path.Combine(runtime, "wayland-0"), runtime, owner));
            Assert.False(OwnedDisplayIsolation.AbsentWaylandSocket(socket, runtime, owner + "-different"));
            Assert.False(OwnedDisplayIsolation.AbsentWaylandSocket(socket, null, owner));
            Assert.False(OwnedDisplayIsolation.AbsentWaylandSocket(socket, runtime, null));
            File.WriteAllText(socket, "not an absent endpoint");
            Assert.False(OwnedDisplayIsolation.AbsentWaylandSocket(socket, runtime, owner));
            File.Delete(socket); Directory.CreateDirectory(socket);
            Assert.False(OwnedDisplayIsolation.AbsentWaylandSocket(socket, runtime, owner));
        }
        finally { Directory.Delete(owner, true); }
    }
    [Theory]
    [InlineData("test-vulkan")]
    [InlineData("test-atmosphere")]
    public void OptionalExportedAtmosphereChecksRequireOnlyTheClient(string command)
    {
        string root = Path.Combine(Directory.GetCurrentDirectory(), "logs", "atmosphere-prerequisite-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var evidence = new Evidence(root, Path.Combine(root, "evidence"));
            var runner = new Runner(Options.Parse([command, "--exported"]), CancellationToken.None, evidence, root: root);
            string executable = Path.Combine(root, "dist", "client", "odot.x86_64");
            var missing = Assert.Throws<VerificationPrerequisiteException>(() => runner.AtmosphereClientPackage());
            Assert.Contains(executable, missing.Message, StringComparison.Ordinal);
            Directory.CreateDirectory(Path.GetDirectoryName(executable)!);
            File.WriteAllBytes(executable, []);
            Assert.Equal(executable, runner.AtmosphereClientPackage());
            Assert.Throws<InvalidOperationException>(() => runner.CheckPackages());
            string server = Path.Combine(root, "dist", "server", "odot.x86_64");
            Directory.CreateDirectory(Path.GetDirectoryName(server)!);
            File.WriteAllBytes(server, []);
            runner.CheckPackages();
        }
        finally { Directory.Delete(root, true); }
    }
    [WindowsGraphicsFact]
    public void WindowsNativeArgumentsRetainConsumedEngineFlagsAndQuotedPaths()
    {
        string[] args = OriginalProcessArguments.WindowsArguments("\"C:\\Program Files\\Godot\\Godot.exe\" --path \"C:\\Game With Spaces\" --max-fps 30 --disable-vsync --windowed --resolution 1280x720 -- --solo");
        string[] expected = [@"C:\Program Files\Godot\Godot.exe", "--path", @"C:\Game With Spaces", "--max-fps", "30", "--disable-vsync", "--windowed", "--resolution", "1280x720", "--", "--solo"];
        Assert.Equal(expected, args);
    }
    [WindowsGraphicsFact]
    public void WindowsOriginalArgumentReaderDoesNotDependOnGodotsConsumedArgumentArray()
    {
        string[] expected = OriginalProcessArguments.WindowsArguments(Environment.CommandLine).TakeWhile(a => a != "--").ToArray();
        Assert.NotEmpty(expected);
        Assert.Equal(expected, OriginalProcessArguments.Read([]));
    }
    [Fact]
    public async Task MissingNativeSurfaceRemainsUnexecutedThroughScheduling()
    {
        var missing = new VerificationPrerequisiteException("missing owned GPU presentation");
        var actual = await Assert.ThrowsAsync<VerificationPrerequisiteException>(() => ScenarioScheduler.Run(
            [new Scenario("native", "actual prerequisite classification", _ => Task.FromException(missing))], 1, CancellationToken.None));
        Assert.Same(missing, actual);
    }
    [Fact]
    public void AccessibilityAndAtmosphereOffAreLocalEffectivePolicyNotLostPreferences()
    {
        var preferences = new GraphicsPreferences(); preferences.Edit("quality", 3);
        preferences.Edit("reduce_motion", 1); preferences.Edit("reduce_effects", 1);
        GraphicsPreferences effective = preferences.Effective(true);
        Assert.Equal(0, effective["wind"]); Assert.Equal(0, effective["foliage"]); Assert.Equal(0, effective["glow"]); Assert.Equal(0, effective["indirect"]);
        Assert.Equal(1, preferences["wind"]); Assert.Equal(1, preferences["glow"]);
        preferences.Edit("atmosphere", 0); effective = preferences.Effective(true);
        foreach (string key in new[] { "fog", "volumetric", "particles", "wind", "foliage", "glow" }) Assert.Equal(0, effective[key]);
        Assert.Equal(3, effective["shadows"]);
        Assert.Throws<ArgumentOutOfRangeException>(() => preferences.Edit("weather", 99));
    }
    [Theory]
    [InlineData("environment/components/forest_spiral_tree.glb", "forest_leaf_teal", true)]
    [InlineData("environment/components/forest_spiral_tree.glb", "bark", false)]
    [InlineData("environment/components/forest_spiral_tree.glb", "gill_glow", false)]
    [InlineData("props/kit_pine.glb", "pine", true)]
    [InlineData("props/kit_pine.glb", "bark", false)]
    [InlineData("buildings/bakery.glb", "forest_leaf", false)]
    [InlineData("characters/adventurer.glb", "pine", false)]
    [InlineData("environment/components/forest_fern.glb", "forest_leaf", true)]
    public void OnlyExplicitActualLeafMaterialsSway(string asset, string material, bool expected) => Assert.Equal(expected, WoodlandEligibility.Foliage(asset, material));
    [Theory]
    [InlineData("forward_plus", "vulkan", "DiscreteGpu", true, true)]
    [InlineData("forward_plus", "vulkan", "Cpu", true, false)]
    [InlineData("forward_plus", "opengl3", "DiscreteGpu", true, false)]
    [InlineData("gl_compatibility", "opengl3", "DiscreteGpu", false, false)]
    [InlineData("forward_plus", "vulkan", "DiscreteGpu", false, false)]
    public void VulkanQualificationCannotBeAnAssumedBackendLabel(string method, string driver, string device, bool renderingDevice, bool expected) =>
        Assert.Equal(expected, Runner.HardwareVulkan(new GraphicsObservation { Method = method, Driver = driver, DeviceType = device, RenderingDevice = renderingDevice, Display = "X11" }));
    [Theory]
    [InlineData("play", "forward-plus", "forward_plus", "vulkan")]
    [InlineData("client", "compatibility", "gl_compatibility", "opengl3")]
    public void ExplicitRendererOverridesAreSourceExportEngineFlagsNotPreferences(string command, string choice, string method, string driver)
    {
        Options options = Options.Parse([command, "--renderer", choice]);
        var runner = new Runner(options, CancellationToken.None, root: Directory.GetCurrentDirectory());
        foreach (string? exported in new string?[] { null, "packed-game" })
        {
            List<string> args = runner.GameArguments("peer", command == "play" ? "menu" : "client", false, 7000, exported);
            int boundary = args.IndexOf("--");
            Assert.Contains(method, args.Take(boundary)); Assert.Contains(driver, args.Take(boundary));
        }
        Assert.DoesNotContain("--rendering-method", runner.GameArguments("observer", "client", true, 7000));
        Assert.Throws<ArgumentException>(() => Options.Parse(["test-ui", "--renderer", choice]));
        Assert.Throws<ArgumentException>(() => Options.Parse(["server", "--renderer", choice]));
    }
}
