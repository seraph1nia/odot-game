using System.Text;
using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed record UiTarget(float X, float Y, bool Visible, bool Enabled)
{
    public float Width { get; init; }
    public float Height { get; init; }
    public string Icon { get; init; } = "";
    public string Text { get; init; } = "";
    public string CostText { get; init; } = "";
}
internal sealed record UnitObservation
{
    public int Id { get; init; }
    public UnitType Type { get; init; }
    public Faction Faction { get; init; }
    public UnitClass Class { get; init; }
    public int Rank { get; init; }
    public int Destination { get; init; }
    public int Health { get; init; }
    public int MaximumHealth { get; init; }
    public bool Deployed { get; init; }
    public bool Dead { get; init; }
    public bool Visible { get; init; }
    public bool WeaponAttached { get; init; }
    public bool ShotVisible { get; init; }
    public bool AttackActive { get; init; }
    public bool HitActive { get; init; }
    public bool InteractionEnabled { get; init; }
    public long AttackSequence { get; init; }
    public int ShotCount { get; init; }
    public long ActionStartTick { get; init; }
    public long ImpactTick { get; init; }
    public long ReadyTick { get; init; }
    public long EffectSequence { get; init; }
    public string Clip { get; init; } = "";
    public string BoneRotation { get; init; } = "";
    public double PoseSeconds { get; init; }
    public float X { get; init; }
    public float Z { get; init; }
    public float BoneX { get; init; }
    public float BoneY { get; init; }
    public float BoneZ { get; init; }
}
internal sealed record EffectObservation
{
    public int Active { get; init; }
    public int Voices { get; init; }
    public int CueCount { get; init; }
    public int Dropped { get; init; }
    public string Bus { get; init; } = "";
    public string[] Positions { get; init; } = [];
}
internal sealed record HealthBarObservation
{
    public int Id { get; init; }
    public int Current { get; init; }
    public int Maximum { get; init; }
    public double Fraction { get; init; }
    public bool Visible { get; init; }
    public float X { get; init; }
    public float Y { get; init; }
    public float Width { get; init; }
    public float Height { get; init; }
    public float FillWidth { get; init; }
    public bool InputIgnored { get; init; }
    public string Track { get; init; } = "";
    public string Fill { get; init; } = "";
}
internal sealed record StockpileObservation(int Gold, int Food, int Wood);
internal sealed record CameraObservation
{
    public float Zoom { get; init; }
    public float PanX { get; init; }
    public float PanZ { get; init; }
    public float BaseSize { get; init; }
    public float Size { get; init; }
    public float Height { get; init; }
    public float[] Rotation { get; init; } = [];
    public float ReferenceX { get; init; }
    public float ReferenceY { get; init; }
    public double MovementSeconds { get; init; }
    public float DirectionX { get; init; }
    public float DirectionY { get; init; }
}
internal sealed record UiObservation
{
    public CameraObservation Camera { get; init; } = new();
    public LandscapeObservation Landscape { get; init; } = new();
    public AssetPlacementObservation[] Placements { get; init; } = [];
    public bool WindowFocused { get; init; }
    public bool UiProvenanceBundled { get; init; }
    public string PanelTexture { get; init; } = "";
    public string[] ButtonTextures { get; init; } = [];
    public int FocusBorder { get; init; }
    public string SliderTexture { get; init; } = "";
    public string Id { get; init; } = "";
    public string? Error { get; init; }
    public string Screen { get; init; } = "";
    public string FocusedControl { get; init; } = "";
    public ulong MusicInstance { get; init; }
    public ulong NativeWindow { get; init; }
    public float MusicPosition { get; init; }
    public bool MusicPlaying { get; init; }
    public string FeedbackText { get; init; } = "";
    public string SteamStatus { get; init; } = "";
    public bool JoinConfirmationOpen { get; init; }
    public bool FriendsOpen { get; init; }
    public int FriendCount { get; init; }
    public string InviteStatus { get; init; } = "";
    public string PhaseText { get; init; } = "";
    public string StatsText { get; init; } = "";
    public string StatusText { get; init; } = "";
    public string DetailText { get; init; } = "";
    public string RosterText { get; init; } = "";
    public long Revision { get; init; }
    public int SelectedSlot { get; init; }
    public bool Connected { get; init; }
    public bool SettingsOpen { get; init; }
    public int ResolutionFocused { get; init; }
    public int ResolutionSelected { get; init; }
    public bool DropdownOpen { get; init; }
    public string DropdownTexture { get; init; } = "";
    public string DialogTexture { get; init; } = "";
    public string[] TabTextures { get; init; } = [];
    public int MasterVolume { get; init; }
    public string BuildVersion { get; init; } = "";
    public string UpdateStatus { get; init; } = "";
    public float MasterGain { get; init; }
    public bool MasterMuted { get; init; }
    public string Display { get; init; } = "";
    public string AudioDriver { get; init; } = "";
    public string UserDataPath { get; init; } = "";
    public string Renderer { get; init; } = "";
    public int Models { get; init; }
    public string[] LoadedModels { get; init; } = [];
    public int Materials { get; init; }
    public bool MusicLoaded { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int Colors { get; init; }
    public string? Screenshot { get; init; }
    public Dictionary<string, UiTarget> Targets { get; init; } = [];
    public string[] UnitBindings { get; init; } = [];
    public UnitObservation[] Units { get; init; } = [];
    public HealthBarObservation[] HealthBars { get; init; } = [];
    public Dictionary<string, string> ResourceIcons { get; init; } = [];
    public bool ResourceRowsSingleLine { get; init; }
    public float HudHeight { get; init; }
    public float HudTop { get; init; }
    public double CombatTick { get; init; }
    public double VisualSeconds { get; init; }
    public long EventCursor { get; init; }
    public int PlaybackGeneration { get; init; }
    public EffectObservation Effects { get; init; } = new();
    public StockpileObservation? Stockpiles { get; init; }
    public float[] PlotHeights { get; init; } = [];
    public int[] BuildingVariants { get; init; } = [];
    public float[] AmbientAngles { get; init; } = [];
}

internal static class UiProtocol
{
    public static async Task<UiObservation> Probe(Child client, int timeout, CancellationToken token, string? screenshot = null)
    {
        string id = Guid.NewGuid().ToString("N");
        await client.Send("ui-probe " + id + (screenshot is null ? "" : " " + Convert.ToBase64String(Encoding.UTF8.GetBytes(screenshot))));
        GameEvent response = await client.WaitFor(e => e.Type == "ui" && JsonSerializer.Deserialize<UiObservation>(e.Message!, WireJson.Options)?.Id == id, "fresh UI response " + id, timeout, token);
        UiObservation value = JsonSerializer.Deserialize<UiObservation>(response.Message!, WireJson.Options)!;
        if (value.Error is not null) throw new InvalidOperationException("UI observation/capture failed: " + value.Error);
        return value;
    }
    public static UiTarget Target(UiObservation observation, string name)
    {
        if (!observation.Targets.TryGetValue(name, out UiTarget? target) || !target.Visible || !target.Enabled)
            throw new InvalidOperationException($"Required UI control '{name}' is missing, hidden or disabled.");
        return target;
    }
    public static void Frame(UiObservation frame, string expectedPath)
    {
        if (frame.Screenshot != expectedPath || !File.Exists(expectedPath) || new FileInfo(expectedPath).Length < 1000 || frame.Width <= 0 || frame.Height <= 0 || frame.Colors < 12)
            throw new InvalidOperationException($"Missing or empty rendered checkpoint: {expectedPath} (sample colors={frame.Colors}).");
    }
}

internal sealed partial class Runner
{
    private static string UiRisk(string name) => name switch
    {
        "combat" => "Paused camera/health-bar reprojection with frozen world anchors; a few probes in the existing first-wave slice. Authoritative overhead bars on both factions, damage/pause/casualty cleanup; eight imported role/faction rigs, sword/axe/cast poses, tower projectiles, contact and effect/audio/death pause cleanup; cheap tests cannot sample rendered bones, pools or voices. One first-wave slice, ordinary setup, 60s bound.",
        "economy" => "Hex/surface contacts and countryside coverage through existing captures/probes; no new setup. Cursor zoom, held WASD/arrows, pan limits/reset/resize and moved roof picking; seconds of fresh input/probes in existing setup, no extra battle. Trio resource/cost icons and full HUD/plot bounds; picking/control routing to authority and rendered assets; headless tests miss input and presentation.",
        "reconnect" => "Local camera retention/disconnected inspection and restored world picking; no extra setup. Overhead bar reconstruction/fractions without duplicates; visible recovery control and retained presentation/identity; headless resume cannot exercise the button.",
        "settings" => "Camera HUD/modal/consumed-key priority and interrupted holds/window focus; owned input/probe waits in existing setup. Kit tabs/dialog/dropdown/slider styling and focus; modal input leakage and preference isolation/persistence; numerical rules tests cannot observe the UI.",
        "launcher" => "Shared menu/starting landscape and return cleanliness through existing probes; no extra setup. Trio panel/button resources, semantic icons and full control bounds; application navigation, direct-invitation fixture modal/focus/scrolling, local session transitions and actual process exit; cheap checks miss native controls. Steam remains disabled.",
        "exported-package" => "Packed shared countryside and contacts through existing captures; no additional match. Packed cursor zoom/pan/reset/picking parity using actual events in existing setup. Packed-resource loading and actual UI input; source tests cannot detect package-only omissions.",
        "installed-linux" => "Installed launcher, packed presentation and normal input; archive inventory alone cannot establish an installed graphical launch.",
        _ => throw new ArgumentException("Unknown UI scenario: " + name)
    };
    private async Task<UiObservation> WaitUi(Child client, Func<UiObservation, bool> predicate, string expectation, CancellationToken token, int? timeout = null)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token); deadline.CancelAfter(timeout ?? options.StartupTimeout);
        try
        {
            while (true)
            {
                UiObservation observation = await UiProtocol.Probe(client, options.StartupTimeout, deadline.Token);
                if (predicate(observation)) return observation;
            }
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        { throw new TimeoutException($"{client.Name}: UI deadline for {expectation}. Evidence: {_scope!.EvidenceDirectory}"); }
    }
    private async Task Click(Child client, string name, CancellationToken token)
    {
        UiObservation ui = await WaitUi(client, p => p.Targets.TryGetValue(name, out UiTarget? target) && target.Visible && target.Enabled, name, token);
        UiTarget target = UiProtocol.Target(ui, name);
        if (!ui.WindowFocused && !ui.SettingsOpen && !ui.FriendsOpen && !ui.JoinConfirmationOpen)
        {
            NativeWindowClose.Focus(client, options, ui.NativeWindow);
            await WaitUi(client, p => p.WindowFocused, "owned click window focused", token);
        }
        await ClickPoint(client, target);
    }
    private static Task ClickPoint(Child client, UiTarget target)
        => client.Send(FormattableString.Invariant($"click {target.X} {target.Y}"));
    private async Task<GameEvent> ClickAck(Child client, string name, CancellationToken token, UiObservation? observed = null)
    {
        long previous = client.History().Where(e => e.Type == "ack").Select(e => e.Result!.Sequence).DefaultIfEmpty().Max();
        if (observed is null) await Click(client, name, token);
        else
        {
            Require(observed.WindowFocused, "fresh observed click retains owned window focus");
            await ClickPoint(client, UiProtocol.Target(observed, name));
        }
        GameEvent result = await client.WaitFor(e => e.Type == "ack" && e.Result!.Sequence > previous, name + " authoritative result", options.StartupTimeout, token);
        Require(result.Result!.Accepted, $"{name} input accepted through normal protocol: {result.Message}");
        return result;
    }
    private async Task Pick(Child client, int slot, CancellationToken token)
    {
        await Click(client, "Plot" + slot, token);
        await WaitUi(client, p => p.SelectedSlot == slot, "selected plot " + slot, token);
    }
    private async Task<UiObservation> ResizeTo1280(Child client, CancellationToken token)
    {
        await Click(client, "GraphicsTab", token);
        await Click(client, "Resolution", token);
        await WaitUi(client, p => p.DropdownOpen, "resolution choices open", token);
        // Pointer-opened menus begin with no keyboard item focused.
        await client.Send("key Down");
        await WaitUi(client, p => p.ResolutionFocused == 0 && p.DropdownOpen, "first resolution focused with native Down", token);
        await client.Send("key Down");
        await WaitUi(client, p => p.ResolutionFocused == 1 && p.DropdownOpen, "1280x720 choice focused with native Down", token);
        await client.Send("key Enter");
        return await WaitUi(client, p => p.Width == 1280 && p.Height == 720 && p.SettingsOpen && !p.DropdownOpen, "native resolution changes to supported 1280x720", token);
    }
    private static void HealthBars(UiObservation frame)
    {
        if (frame.HealthBars.Select(b => b.Id).Distinct().Count() != frame.HealthBars.Length) throw new InvalidOperationException("Duplicate unit health bars.");
        foreach (HealthBarObservation bar in frame.HealthBars)
        {
            UnitObservation unit = frame.Units.Single(u => u.Id == bar.Id);
            if (unit.Dead || !unit.Visible || !unit.Deployed || bar.Current != unit.Health || bar.Maximum != unit.MaximumHealth
                || bar.Fraction != PresentationLimits.HealthFraction(unit.Health, unit.MaximumHealth)
                || Math.Abs(bar.FillWidth - 43 * bar.Fraction) > 0.001 || !bar.InputIgnored
                || bar.Track != "res://Assets/TrioUI/derived/health_track.svg" || bar.Fill != "res://Assets/TrioUI/derived/health_fill.svg")
                throw new InvalidOperationException("Health bar disagrees with sampled living unit: " + bar.Id);
            if (bar.Visible && (bar.Width <= 0 || bar.Height <= 0 || bar.X < 0 || bar.Y < 0 || bar.X + bar.Width > frame.Width + 1 || bar.Y + bar.Height > frame.HudTop + 1))
                throw new InvalidOperationException("Health bar outside visible world area: " + bar.Id);
        }
        foreach (UnitObservation unit in frame.Units.Where(u => u.Visible && u.Deployed && !u.Dead))
            if (!frame.HealthBars.Any(b => b.Id == unit.Id)) throw new InvalidOperationException("Missing living unit health bar: " + unit.Id);
    }
    private static void TrioPresentation(UiObservation frame)
    {
        Require(frame.UiProvenanceBundled, "kit provenance and permission documentation bundled with UI assets");
        Require(frame.PanelTexture == "res://Assets/TrioUI/cozy/panel_plain.svg" && frame.FocusBorder == 2, "actual kit panel and keyboard focus styles loaded");
        Require(frame.ButtonTextures.Length == 4 && frame.ButtonTextures.Distinct().Count() == 4 && frame.ButtonTextures.All(p => p.StartsWith("res://Assets/TrioUI/derived/button_", StringComparison.Ordinal)), "four distinct kit button-state textures");
        Require(frame.SliderTexture == "res://Assets/TrioUI/derived/slider_knob.svg", "kit slider knob loaded");
        if (frame.SettingsOpen || frame.FriendsOpen || frame.JoinConfirmationOpen)
            Require(frame.DialogTexture == "res://Assets/TrioUI/cozy/panel_plain.svg", "actual owned dialog uses shared kit panel");
        if (frame.SettingsOpen)
            Require(frame.TabTextures.Length == 2 && frame.TabTextures.Distinct().Count() == 2, "selected and unselected tabs use distinct kit textures");
        if (frame.DropdownOpen)
            Require(frame.DropdownTexture == "res://Assets/TrioUI/cozy/panel_plain.svg", "actual dropdown popup uses shared kit panel");
        if (frame.Screen == "session")
        {
            Require(frame.ResourceRowsSingleLine && frame.HudHeight <= 310, "readable single-line resource values within bounded HUD reservation");
            Require(frame.ResourceIcons.GetValueOrDefault("Gold") == "res://Assets/TrioUI/icons/icon_gold_pile.svg" && frame.ResourceIcons.GetValueOrDefault("Food") == "res://Assets/TrioUI/icons/icon_bread.svg" && frame.ResourceIcons.GetValueOrDefault("Wood") == "", "gold/food icons and explicit wood text from actual HUD");
            foreach (var (name, plot) in frame.Targets.Where(t => t.Key.StartsWith("Plot", StringComparison.Ordinal) && t.Value.Visible))
                Require(plot.X >= 0 && plot.X <= frame.Width && plot.Y >= 0 && plot.Y < frame.HudTop, "plot selectable above HUD: " + name);
            HealthBars(frame);
        }
        foreach (var (name, target) in frame.Targets.Where(t => t.Value.Visible && !t.Key.StartsWith("Plot", StringComparison.Ordinal) && !t.Key.StartsWith("InviteFriend", StringComparison.Ordinal)))
            Require(target.X - target.Width / 2 >= -1 && target.X + target.Width / 2 <= frame.Width + 1 && target.Y - target.Height / 2 >= -1 && target.Y + target.Height / 2 <= frame.Height + 1,
                "complete control bounds inside viewport: " + name);
    }
    private async Task Checkpoint(Child client, string name, CancellationToken token)
    {
        string path = Path.Combine(_scope!.EvidenceDirectory, name + ".png");
        UiObservation frame = await UiProtocol.Probe(client, options.StartupTimeout, token, path);
        UiProtocol.Frame(frame, path);
        TrioPresentation(frame);
        Countryside(frame);
        Require(frame.Display == "X11" && frame.Models > 0 && frame.Materials > 0 && frame.MusicLoaded, "rendered UI/models/materials/music loaded on owned X11 display");
        Require(frame.UnitBindings.Length == 18, "eight faction/role rigs, required clips/hand bindings and ten weapons imported");
        Require(frame.AudioDriver == "Dummy" && (frame.Renderer.Contains("llvmpipe", StringComparison.OrdinalIgnoreCase) || frame.Renderer.Contains("softpipe", StringComparison.OrdinalIgnoreCase)), "actual client uses silent Dummy audio and Mesa software rendering");
        Require(Path.GetFullPath(frame.UserDataPath).StartsWith(Path.Combine(_scope.Directory, "ui-client", "data") + Path.DirectorySeparatorChar, StringComparison.Ordinal), "effective user:// belongs to this client scope");
        await File.WriteAllTextAsync(Path.Combine(_scope.EvidenceDirectory, name + "-observation.json"), JsonSerializer.Serialize(frame, Evidence.JsonOptions), token);
        Console.WriteLine($"FRAME {name}: {frame.Width}x{frame.Height}, colors={frame.Colors}, renderer={frame.Renderer}; {path}");
    }
    private async Task EconomyDetails(Child client, Child observer, CancellationToken token)
    {
        await Pick(client, 3, token); await ClickAck(client, "Lumbermill", token);
        await Action(observer, "build 0 catapulttower", token); await Action(observer, "build 1 lumbermill", token);
        await ClickAck(client, "Ready", token); await Action(observer, "ready", token);
        await Observe(client, s => s.Phase == Phase.Preparation, "economy third production remains spendable", token);
        UiObservation preparation = await WaitUi(client, p => p.PhaseText.Contains("Preparation", StringComparison.Ordinal), "preparation label", token);
        Require(preparation.Targets["Ready"].Enabled && preparation.StatsText.Contains("Wood 5", StringComparison.Ordinal), "ready-for-battle control and exact wood HUD");
        await Action(observer, "build 2 arrowtower", token);
        await Pick(client, 1, token); await ClickAck(client, "Recruit", token);
        await ClickAck(client, "Ready", token); await Action(observer, "ready", token);
        MatchSnapshot next = await Observe(client, s => s.Wave == 2 && s.Phase == Phase.Building || s.Phase == Phase.Defeat, "economy short first wave", token);
        Require(next.Phase != Phase.Defeat, "ordinary economy setup reaches further controls");
        await Action(client, "ready", token); await Action(observer, "ready", token);
        await Observe(client, s => s.Wave == 2 && s.Turn == 2, "economy Blacksmith resources", token);
        await Pick(client, 4, token); await ClickAck(client, "Blacksmith", token);
        await Action(client, "ready", token); await Action(observer, "ready", token);
        await Observe(client, s => s.Wave == 2 && s.Turn == 3, "economy research resources", token);
        await Pick(client, 4, token); GameEvent researched = await ClickAck(client, "ResearchMelee", token);
        await Observe(observer, s => s.Revision >= State(researched).Revision && s.Players.Single(p => p.Id == client.PlayerId).Research.Melee == 1, "observer sees class research", token);
        await ClickAck(client, "Ready", token); await Action(observer, "ready", token);
        await Observe(client, s => s.Wave == 2 && s.Phase == Phase.Preparation, "economy second preparation", token);
        await Action(observer, "upgrade 0", token);
        await Click(client, "City" + observer.PlayerId, token);
        UiObservation upgraded = await WaitUi(client, p => p.BuildingVariants.Length == 9 && p.BuildingVariants[0] == 2, "actual Catapult structural upgrade", token);
        Require(upgraded.PlotHeights.Length == 9 && upgraded.PlotHeights[0] < upgraded.PlotHeights[3] && upgraded.PlotHeights[3] < upgraded.PlotHeights[6], "three actual terrace heights");
        Countryside(upgraded);
        Require(upgraded.Placements.Any(p => p.Asset.EndsWith("tower_base_blue.gltf", StringComparison.Ordinal))
            && upgraded.Placements.Any(p => p.Name == "Slot0" && p.Support > 1), "actual tower seated on raised support deck");
        CityState city = Latest(client).Players.Single(c => c.Id == observer.PlayerId);
        Require(upgraded.Stockpiles == new StockpileObservation(PresentationLimits.StockpileCount(city.Gold), PresentationLimits.StockpileCount(city.Food), PresentationLimits.StockpileCount(city.Wood)), "actual resource node counts match authoritative tier thresholds");
        for (int slot = 0; slot < 9; slot++) await Pick(client, slot, token);
        await Pick(client, 0, token);
        UiObservation foreign = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(!foreign.Targets["Upgrade"].Enabled, "foreign tower remains read only");
        await CameraZoom(client, token);
        await Pick(client, 0, token);
        Require(!(await UiProtocol.Probe(client, options.StartupTimeout, token)).Targets["Upgrade"].Enabled, "zoomed foreign roof remains read only");
        await Checkpoint(client, "economy-camera-roof", token);
        await Click(client, "ResetView", token);
        await Checkpoint(client, "economy-upgraded-roof", token);
        await Click(client, "City" + client.PlayerId, token); await Pick(client, 4, token);
    }
    private async Task UiScenario(string name, CancellationToken token)
    {
        Console.WriteLine($"UI risk: {name}: {UiRisk(name)}");
        using var combatDeadline = CancellationTokenSource.CreateLinkedTokenSource(token);
        if (name == "combat") combatDeadline.CancelAfter(60000);
        token = combatDeadline.Token;
        bool package = name == "exported-package";
        string? executable = package ? Path.Combine(_root, "dist", "client", "odot.x86_64") : null;
        var started = await StartServer("ui-server", options.Port ?? _scope!.Port(), token, retryAutomatic: options.Port is null, exported: package ? Path.Combine(_root, "dist", "server", "odot.x86_64") : null);
        Child client = StartGame("ui-client", false, false, started.Port, executable);
        await client.WaitFor(e => e.Type == "connected", "graphical client connected", options.StartupTimeout, token);
        Child observer = StartGame("ui-observer", false, true, started.Port, executable, "--automated");
        await observer.WaitFor(e => e.Type == "connected", "independent observer connected", options.StartupTimeout, token);
        await WaitUi(client, p => p.Connected && p.Models > 0, "presentation ready", token);
        GameEvent initial = await ClickAck(client, "Start", token);
        await Observe(observer, s => s.Revision >= State(initial).Revision && s.Phase == Phase.Building, "observer sees UI Start", token);
        switch (name)
        {
            case "economy":
            case "exported-package":
                await CameraControls(client, !package, token);
                await Pick(client, 0, token);
                int gold = Latest(client).Players.Single(p => p.Id == client.PlayerId).Gold;
                UiObservation costs = await UiProtocol.Probe(client, options.StartupTimeout, token);
                ResourceCost farmCost = Latest(client).BuildingCatalog.Single(b => b.Type == Building.Farm).Construction;
                Require(costs.Targets["Farm"].CostText.Contains(farmCost.Gold + " gold", StringComparison.Ordinal) && costs.Targets["Farm"].CostText.Contains(farmCost.Wood + " wood", StringComparison.Ordinal), "visible resource cost row retains exact authoritative gold/wood");
                GameEvent purchase = await ClickAck(client, "Farm", token);
                await Observe(observer, s => s.Revision >= State(purchase).Revision && s.Players.Single(p => p.Id == client.PlayerId).Slots[0].Type == Building.Farm, "observer sees UI purchase", token);
                Require(State(purchase).Players.Single(p => p.Id == client.PlayerId).Gold == gold - State(purchase).Rules.BuildCost, "UI purchase spends authoritative gold once");
                if (!package)
                {
                    await Pick(client, 1, token); await ClickAck(client, "Barracks", token);
                    await Action(client, "ready", token); await Action(observer, "ready", token);
                    await Observe(client, s => s.Turn == 2, "economy first production", token);
                    await Pick(client, 2, token); await ClickAck(client, "ArcheryRange", token);
                    await Pick(client, 1, token); GameEvent recruit = await ClickAck(client, "Recruit", token);
                    await Observe(observer, s => s.Revision >= State(recruit).Revision && s.Players.Single(p => p.Id == client.PlayerId).Soldiers.Length == 1, "observer sees UI recruitment", token);
                    await Action(client, "ready", token); await Action(observer, "ready", token);
                    await Observe(client, s => s.Turn == 3, "economy second production", token);
                    await Pick(client, 2, token); GameEvent ranged = await ClickAck(client, "RecruitRanged", token);
                    await Observe(observer, s => s.Revision >= State(ranged).Revision && s.Players.Single(p => p.Id == client.PlayerId).Soldiers.Any(u => u.Type == UnitType.Crossbowman), "observer sees UI ranged recruitment", token);
                    for (int slot = 0; slot < 9; slot++) await Pick(client, slot, token);
                    await EconomyDetails(client, observer, token);
                }
                await Checkpoint(client, package ? "packed-building" : "economy-building", token);
                if (package)
                {
                    await MixedArmy(client, observer, token, farmExists: true, towers: true);
                    await Click(client, "City" + observer.PlayerId, token);
                    UiObservation tower = await WaitUi(client, p => p.Effects.Active > 0 && p.Effects.Bus == "Master", "packed tower feedback", token);
                    Require(tower.LoadedModels.Contains("Medieval/building_tower_catapult_blue.gltf") && tower.Stockpiles is not null && tower.Effects.Voices <= 8, "packed tower model, resource piles and bounded Master audio load from exports");
                    await Checkpoint(client, "packed-catapult", token);
                    await Click(client, "City" + client.PlayerId, token);
                    await CombatCheckpoint(client, observer, token, shortCheck: true);
                }
                break;
            case "combat":
                await SpecialistArmy(client, observer, token);
                await CombatCheckpoint(client, observer, token);
                break;
            case "reconnect":
                await MixedArmy(client, observer, token);
                await Observe(client, s => CombatPlayback.All(s).Any(u => u.PendingImpact), "current attack before reconnect", token);
                await ClickAck(client, "Pause", token);
                int identity = client.PlayerId, connection = client.PeerId;
                string before = Gameplay(Latest(client));
                UiObservation cameraBefore = await CameraZoom(client, token);
                await client.Send("disconnect");
                await client.WaitFor(e => e.Type == "server-disconnected", "local transport disconnected", options.StartupTimeout, token);
                await Observe(observer, s => !s.Players.Single(p => p.Id == identity).Connected, "observer sees absent city", token);
                Require(SameCamera(cameraBefore.Camera, (await UiProtocol.Probe(client, options.StartupTimeout, token)).Camera), "transport loss retains local view");
                await HoldPan(client, ["D"], token);
                UiObservation cameraDisconnected = await UiProtocol.Probe(client, options.StartupTimeout, token);
                await Click(client, "Reconnect", token);
                GameEvent resumed = await client.WaitFor(e => e.Type == "connected" && e.PeerId != connection, "Reconnect control restores a new transport", options.StartupTimeout, token);
                Require(resumed.PlayerId == identity && Gameplay(State(resumed)) == before, "Reconnect input preserves city identity and gameplay");
                await Observe(observer, s => s.Players.Single(p => p.Id == identity).Connected && s.Revision >= State(resumed).Revision, "observer sees restored identity", token);
                UiObservation restored = await WaitUi(client, p => p.Units.Length == CombatPlayback.All(State(resumed)).Length && p.EventCursor == State(resumed).EventSequence,
                    "restored current unit baseline", token);
                Require(SameCamera(cameraDisconnected.Camera, restored.Camera), "same-match reconnect preserves adjusted camera");
                await Pick(client, 1, token);
                await Click(client, "ResetView", token);
                HealthBars(restored);
                foreach (HealthBarObservation bar in restored.HealthBars)
                {
                    UnitState expected = CombatPlayback.All(State(resumed)).Single(u => u.Id == bar.Id);
                    Require(bar.Current == expected.Health && bar.Maximum == expected.Profile.Health && bar.Fraction == PresentationLimits.HealthFraction(expected.Health, expected.Profile.Health), "reconnect reconstructs current authoritative health fraction");
                }
                Require(restored.Units.All(u => !u.Dead && u.EffectSequence == 0) && restored.Effects.Active == 0 && restored.Effects.Voices == 0, "reconnect baselines living poses without historical effects/corpses");
                Require(restored.Units.Any(u => u.Type == UnitType.Crossbowman && u.WeaponAttached), "reconnect preserves ranged rig and profile");
                await Checkpoint(client, "restored-connection", token);
                break;
            case "settings":
                await CameraInputPriority(client, token);
                await Pick(client, 0, token);
                UiObservation closed = await UiProtocol.Probe(client, options.StartupTimeout, token);
                UiTarget behind = UiProtocol.Target(closed, "Farm");
                await Click(client, "Settings", token);
                await WaitUi(client, p => p.SettingsOpen, "settings modal open", token);
                await ClickPoint(client, behind);
                await WaitUi(client, p => p.SettingsOpen, "modal retains input", token);
                GameEvent barrier = await Action(observer, "unknown", token, false);
                Require(State(barrier).Players.Single(p => p.Id == client.PlayerId).Slots[0].Type == Building.Empty, "settings modal blocks gameplay input behind it");
                await Click(client, "DisplayMode", token);
                await WaitUi(client, p => p.DropdownOpen, "native display dropdown open", token);
                await Checkpoint(client, "settings-dropdown", token);
                await client.Send("key Escape");
                await WaitUi(client, p => p.SettingsOpen && !p.DropdownOpen, "Escape closes dropdown before settings", token);
                UiObservation resized = await ResizeTo1280(client, token);
                TrioPresentation(resized);
                Require(Math.Abs(resized.Camera.Zoom - closed.Camera.Zoom) < 0.001 && Math.Abs(resized.Camera.PanX - closed.Camera.PanX) < 0.001 && Math.Abs(resized.Camera.PanZ - closed.Camera.PanZ) < 0.001, "resize preserves relative navigation");
                await Click(client, "AboutTab", token);
                UiObservation about = await WaitUi(client, p => p.Targets.TryGetValue("CheckUpdate", out UiTarget? update) && update.Visible, "About controls visible", token);
                Require(about.BuildVersion == "Development build", "source settings identify a development build accurately");
                await Click(client, "CheckUpdate", token);
                await WaitUi(client, p => p.UpdateStatus.Contains("Development builds", StringComparison.Ordinal), "development update feedback", token);
                await Click(client, "AudioTab", token);
                await Click(client, "Volume", token);
                await UiProtocol.Probe(client, options.StartupTimeout, token);
                await client.Send("key Right");
                UiObservation changed = await WaitUi(client, p => p.MasterVolume != closed.MasterVolume, "representative volume change", token);
                Require(SameCamera(resized.Camera, changed.Camera), "consumed slider arrows do not pan camera");
                Require(Math.Abs(changed.MasterGain - changed.MasterVolume / 100f) < 0.001f && changed.MasterMuted == (changed.MasterVolume == 0), "Master state follows actual slider input (silent Dummy audio)");
                await client.Send("key Home");
                UiObservation muted = await WaitUi(client, p => p.MasterVolume == 0 && p.MasterMuted, "Master mute applies to effect bus", token);
                Require(muted.Effects.Bus == "Master" && muted.Effects.Voices <= 8, "synthesized cues use the muted Master bus and bounded voices");
                for (int volume = 0; volume < changed.MasterVolume; volume++) await client.Send("key Right");
                await WaitUi(client, p => p.MasterVolume == changed.MasterVolume, "restore chosen owned test volume", token);
                await Checkpoint(client, "settings-audio", token);
                await Click(client, "CloseSettings", token);
                await WaitUi(client, p => !p.SettingsOpen, "settings closed and saved", token);
                await Click(client, "ResetView", token);
                RequireOverview(await UiProtocol.Probe(client, options.StartupTimeout, token));
                int originalId = client.PlayerId;
                await client.Send("quit"); Require(await client.WaitExit(token) == 0, "settings client exits cleanly");
                await Observe(observer, s => !s.Players.Single(p => p.Id == originalId).Connected, "restart absent city", token);
                Child restarted = StartGame("ui-client", false, false, started.Port);
                GameEvent recovered = await restarted.WaitFor(e => e.Type == "connected", "settings client restarted", options.StartupTimeout, token);
                UiObservation persisted = await WaitUi(restarted, p => p.Connected && p.Models > 0, "restarted settings ready", token);
                RequireOverview(persisted);
                Require(recovered.PlayerId == originalId && persisted.MasterVolume == changed.MasterVolume && Math.Abs(persisted.MasterGain - changed.MasterGain) < 0.001f, "owned preference survives client restart without regrant");
                await Checkpoint(restarted, "settings-restarted", token);
                break;
        }
    }
}
