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
    public string Tooltip { get; init; } = "";
}
internal sealed record UnitObservation
{
    public int Id { get; init; }
    public UnitType Type { get; init; }
    public Faction Faction { get; init; }
    public UnitClass Class { get; init; }
    public int Rank { get; init; }
    public int Level { get; init; }
    public bool IsBoss { get; init; }
    public int Size { get; init; }
    public bool Participating { get; init; }
    public int Destination { get; init; }
    public int Health { get; init; }
    public int MaximumHealth { get; init; }
    public bool Deployed { get; init; }
    public bool Dead { get; init; }
    public bool Visible { get; init; }
    public bool WeaponAttached { get; init; }
    public bool EquipmentAligned { get; init; }
    public string EquipmentRotation { get; init; } = "";
    public bool ShotVisible { get; init; }
    public bool AttackActive { get; init; }
    public bool HitActive { get; init; }
    public bool InteractionEnabled { get; init; }
    public long AttackSequence { get; init; }
    public int ShotCount { get; init; }
    public long ActionStartTick { get; init; }
    public long ImpactTick { get; init; }
    public long ReadyTick { get; init; }
    public HexUnitState? Hex { get; init; }
    public bool? AttackLanded { get; init; }
    public long EffectSequence { get; init; }
    public string Clip { get; init; } = "";
    public string BoneRotation { get; init; } = "";
    public double PoseSeconds { get; init; }
    public float X { get; init; }
    public float Z { get; init; }
    public float Heading { get; init; }
    public double WalkingBlend { get; init; }
    public double AttackBlend { get; init; }
    public double HitBlend { get; init; }
    public float BoneX { get; init; }
    public float BoneY { get; init; }
    public float BoneZ { get; init; }
}
internal sealed record StrikeObservation
{
    public int Id { get; init; }
    public int TargetId { get; init; }
    public long AttackSequence { get; init; }
    public long ImpactTick { get; init; }
    public bool? AttackLanded { get; init; }
    public bool Visible { get; init; }
    public bool ImpactVisible { get; init; }
    public string CueStyle { get; init; } = "";
    public string Phase { get; init; } = "";
    public bool IntentVisible { get; init; }
    public int IntentDashes { get; init; }
    public bool StrikeVisible { get; init; }
    public float StrikeRadius { get; init; }
    public float SourceX { get; init; }
    public float SourceZ { get; init; }
    public float TargetX { get; init; }
    public float TargetZ { get; init; }
    public float SourceScreenX { get; init; }
    public float SourceScreenY { get; init; }
    public float TargetScreenX { get; init; }
    public float TargetScreenY { get; init; }
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
    public string Role { get; init; } = "";
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
internal sealed record DeathCleanupObservation(int Id, long DeathEndTick, double CombatTick, double VisualSeconds);
internal sealed record RenderCostObservation(long DrawCalls, long Objects, long Primitives, long TextureBytes, long BufferBytes, double Nodes, double Resources,
    double FramesPerSecond, double ProcessMilliseconds, double PhysicsMilliseconds);
internal sealed record ProbeCostObservation(double FrameWaitMilliseconds, double ReadbackMilliseconds, double SamplingMilliseconds, double PngMilliseconds, double ObservationMilliseconds)
{
    public bool Live { get; init; }
    public ulong ProcessFrame { get; init; }
    public double RenderSetupCpuMs { get; init; }
    public double ViewportCpuMs { get; init; }
    public double ViewportGpuMs { get; init; }
}
internal sealed record RenderGroupObservation(string Group, int Nodes, long Surfaces, long Triangles, long Instances, int Materials);
internal sealed record UiObservation
{
    public bool AuthoredProvenanceBundled { get; init; }
    public string[] InstalledModels { get; init; } = [];
    public RenderCostObservation? RenderCosts { get; init; }
    public ProbeCostObservation? ProbeCosts { get; init; }
    public RenderGroupObservation[] RenderGroups { get; init; } = [];
    public Game.OwnedFrameCapture.Receipt? RawCapture { get; init; }
    public Dictionary<int, string> StatusBadges { get; init; } = [];
    public string ResearchText { get; init; } = "";
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
    public string WindowTitle { get; init; } = "";
    public string BrandTitle { get; init; } = "";
    public string BrandPositioning { get; init; } = "";
    public bool BrandTextFits { get; init; }
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
    public string UpkeepText { get; init; } = "";
    public string RewardText { get; init; } = "";
    public string ConstructionGroup { get; init; } = "";
    public string PhaseText { get; init; } = "";
    public string StatsText { get; init; } = "";
    public string StatusText { get; init; } = "";
    public string DetailText { get; init; } = "";
    public string DetailsText { get; init; } = "";
    public bool DetailsOpen { get; init; }
    public ArmyState? ArmyHomes { get; init; }
    public string[] HomeMarkers { get; init; } = [];
    public string[] WorldLabels { get; init; } = [];
    public bool TownHallOpen { get; init; }
    public string TownHallText { get; init; } = "";
    public int SelectedStoredUnit { get; init; }
    public string[] ResourceOrder { get; init; } = [];
    public int ObservedCity { get; init; }
    public int[] CityIds { get; init; } = [];
    public string[] PhaseRows { get; init; } = [];
    public Dictionary<string, int> ResourceBalances { get; init; } = [];
    public Dictionary<string, string> ResourceIncome { get; init; } = [];
    public string IncomeLabel { get; init; } = "";
    public string IncomeContext { get; init; } = "";
    public string[] CompactUpkeep { get; init; } = [];
    public bool ReturnConfirmationOpen { get; init; }
    public string ConfirmationFocus { get; init; } = "";
    public string ReturnWarning { get; init; } = "";
    public InspectorObservation? InspectedUnit { get; init; }
    public HomeHealthObservation HomeHealth { get; init; } = new();
    public string[] LockedMarkers { get; init; } = [];
    public string RosterText { get; init; } = "";
    public long Revision { get; init; }
    public string MatchId { get; init; } = "";
    public Phase MatchPhase { get; init; }
    public int Wave { get; init; }
    public bool Paused { get; init; }
    public int TurnSerial { get; init; }
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
    public DeathCleanupObservation[] DeathCleanups { get; init; } = [];
    public StrikeObservation[] Strikes { get; init; } = [];
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
    public static bool PaidUpkeep(UiObservation frame, MatchSnapshot battle, int player)
    {
        BattleUpkeepReceipt? receipt = battle.Players.Single(p => p.Id == player).LastUpkeep;
        return battle.Phase == Phase.Combat && battle.Paused && receipt?.Wave == battle.Wave
            && frame.Connected && frame.MatchId == battle.MatchId && frame.Revision == battle.Revision
            && frame.MatchPhase == battle.Phase && frame.Paused && frame.Wave == battle.Wave && frame.TurnSerial == battle.TurnSerial
            && frame.ObservedCity == player && frame.CompactUpkeep.SequenceEqual(new[]
            { $"Paid this battle · W{receipt.Wave}", $"{receipt.Paid} food", "Sat out", receipt.Unfed.Length.ToString(System.Globalization.CultureInfo.InvariantCulture) });
    }
    public static DeathCleanupObservation DeathCleanup(UiObservation frame, int id, long deathEndTick, double sinceVisualSeconds)
    {
        DeathCleanupObservation? cleanup = frame.DeathCleanups.SingleOrDefault(sample => sample.Id == id);
        if (cleanup is null || cleanup.DeathEndTick != deathEndTick || frame.Units.Any(unit => unit.Id == id)
            || !double.IsFinite(cleanup.CombatTick) || cleanup.CombatTick < deathEndTick || cleanup.CombatTick > frame.CombatTick)
            throw new InvalidOperationException("Death cleanup requires the removed model's current-session expiry witness.");
        double elapsed = cleanup.VisualSeconds - sinceVisualSeconds;
        if (!double.IsFinite(elapsed) || elapsed < 0 || elapsed > 2 || cleanup.VisualSeconds > frame.VisualSeconds)
            throw new InvalidOperationException($"Death view must free within two unpaused seconds; actual removal elapsed={elapsed:R}.");
        return cleanup;
    }

    public static async Task<UiObservation> Probe(Child client, int timeout, CancellationToken token, string? screenshot = null, bool deferred = false, bool live = false)
    {
        string id = Guid.NewGuid().ToString("N");
        await client.Send("ui-probe " + id + (screenshot is null ? (live ? " none" : "") : " " + Convert.ToBase64String(Encoding.UTF8.GetBytes(screenshot))) + (live ? (deferred ? " deferred-live" : " live") : deferred ? " deferred" : ""));
        GameEvent response = await client.WaitFor(e => e.Type == "ui" && e.Message!.Contains(id, StringComparison.Ordinal) && JsonSerializer.Deserialize<UiObservation>(e.Message!, WireJson.Options)?.Id == id, "fresh UI response " + id, timeout, token);
        UiObservation value = JsonSerializer.Deserialize<UiObservation>(response.Message!, WireJson.Options)!;
        if (value.Error is not null) throw new InvalidOperationException("UI observation/capture failed: " + value.Error);
        return value;
    }
    public static async Task<(UiObservation Frame, double Milliseconds)> Persist(Child client, UiObservation captured, int timeout, CancellationToken token)
    {
        var expected = captured.RawCapture ?? throw new InvalidOperationException("Actual acquired raw frame is missing.");
        await client.Send("ui-persist-frame " + expected.Id);
        GameEvent response = await client.WaitFor(e => e.Type == "ui-frame-persisted" && e.Message!.Contains(expected.Id, StringComparison.Ordinal), "owned persisted raw frame " + expected.Id, timeout, token);
        using JsonDocument document = JsonDocument.Parse(response.Message!);
        var actual = document.RootElement.GetProperty("Receipt").Deserialize<Game.OwnedFrameCapture.Receipt>(WireJson.Options);
        if (actual != expected || actual!.Id != captured.Id) throw new InvalidOperationException("PNG persistence changed capture identity/pixels/metadata.");
        UiObservation persisted = captured with { Screenshot = actual.Path };
        Frame(persisted, actual.Path);
        return (persisted, document.RootElement.GetProperty("Milliseconds").GetDouble());
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
    private static readonly string[] EconomyResourceLabels = ["Gold", "Food", "Wood", "Stone", "Metal", "Cloth"];
    private static string UiRisk(string name) => name switch
    {
        "authored-scale" => "Selected authored settlement/combat diagnostic at 16/64/256 pressure recipes and zoom1/3; 600 frames, owned display, no default-suite expansion or graphical campaign",
        "combat-playback" => "Selected fixed-input runtime measurement of normal playback/views; owned 600-frame replay, no default-suite or large graphical fight",
        "combat" => "Direct committed walking with intentional transit overlap, bounded pose/facing blends and non-beam linked melee intent/strike/impact. The selectable melee gate adds at most twelve fresh progression observations/four PNGs in the same paid setup, peers and display (measured 4-6s); cheap timing tests and paused endpoints cannot prove actual advancing imported bones or rendered geometry. No additional scenario or graphical campaign. Paused focus away/return, current bones and inspection, camera/health-bar projection, rig/action/tower/effect/audio/casualty cleanup using the existing owned peers/display. Adds ordinary earned research through at most wave ten: actual global Fire purchase, permanent Frost lock and current paused burn badge/inspection. Research is separately selectable with 120s setup/30s feature bounds; default reuses the existing match under the supervisor-resolved worker deadline. Cheap tests cannot sample rendered controls, bones, pools or voices; no branch matrix or full graphical campaign.",
        "economy" => "Income/upkeep, explicit zero-wood recovery, build/upgrade/sale capacity, both-size inspector bounds and paused transport refresh. One bounded additional ordinary clear and two productions fund a real shortage sale; no extra peers or display. Hex/surface contacts and countryside coverage through existing captures/probes; no new setup. Cursor zoom, held WASD/arrows, pan limits, Space reset, left drag/interruption, resource-table input protection, resize and moved roof picking; seconds of fresh input/probes in existing setup. One actual witness per control family, repeated recruit/trade/production via ordinary requests, six-resource costs, purchased/locked land, contextual sales, Market bundles and full HUD/plot bounds. Research replaces the rank control in the same opening: actual level-two Research Tower upgrade, thirds-earned foundation and retained technology/wounds on sale, with quoted trades of current surplus stocks while reserving recruitment equipment and upkeep. Picking/control routing to authority and rendered assets; headless tests miss input and presentation.",
        "reconnect" => "Local camera retention/disconnected inspection and restored world picking; no extra setup. Overhead bar reconstruction/fractions without duplicates, current baseline without historical cues, visible recovery control and retained presentation/identity. Active research/status restoration through the same control is also asserted by the selectable combat/research checkpoint; headless resume cannot exercise the button or badges.",
        "settings" => "Camera HUD/modal/consumed-key priority and interrupted holds/window focus; owned input/probe waits in existing setup. Kit tabs/dialog/dropdown/slider styling and focus; modal input leakage and preference isolation/persistence; numerical rules tests cannot observe the UI.",
        "launcher" => "Live Common Watch title/window identity, cooperative positioning and measured label fit at existing sizes/return checkpoints; a few assertions and one multiplayer capture, no extra fixture. Shared menu/starting landscape and return cleanliness through existing probes; no extra setup. Trio panel/button resources, text controls and full control bounds; Cancel-default/Escape confirmation, guest leave/private resume and host return reuse the existing fixture; one owned guest restart and a few modal probes, no extra battle. Application navigation, direct-invitation fixture modal/focus/scrolling, local session transitions and actual process exit once per boundary, with prepared-window resizing for both sizes; cheap checks miss native controls. Steam remains disabled.",
        "exported-package" => "Packed shared countryside and contacts through existing captures; no additional match. Packed cursor zoom/pan/reset/picking parity using actual events in existing setup. Packed-resource loading and actual UI input; source tests cannot detect package-only omissions.",
        "installed-linux" => "Installed archive reuses focused packed launch/solo/purchase/bindings/live animation/two-size/Exit route; inventory alone cannot establish an installed graphical launch.",
        _ => throw new ArgumentException("Unknown UI scenario: " + name)
    };
    private static string IncomeText(ResourceCost? income, Resource resource) => income is ResourceCost value ? value.Amount(resource) > 0 ? $"+{value.Amount(resource)}" : "0" : "Unavailable";
    private async Task SimulationSpeed(Child authority, int speed, CancellationToken token)
    {
        string id = Guid.NewGuid().ToString("N");
        await authority.Send($"pacing {id} {speed}");
        await authority.WaitFor(e => e.Type == "pacing" && e.Message == id + ":" + speed,
            "owned simulation speed " + speed, options.StartupTimeout, token);
    }
    private async Task<UiObservation> WaitUi(Child client, Func<UiObservation, bool> predicate, string expectation, CancellationToken token, int? timeout = null, bool live = false)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token); deadline.CancelAfter(timeout ?? options.StartupTimeout);
        try
        {
            while (true)
            {
                UiObservation observation = await UiProtocol.Probe(client, options.StartupTimeout, deadline.Token, live: live);
                if (predicate(observation)) return observation;
            }
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        { throw new TimeoutException($"{client.Name}: UI deadline for {expectation}. Evidence: {_scope!.EvidenceDirectory}"); }
    }
    private async Task Click(Child client, string name, CancellationToken token)
    {
        if (name == "ResetView")
        {
            UiObservation initial = await UiProtocol.Probe(client, options.StartupTimeout, token);
            Require(!initial.Targets.ContainsKey("ResetView"), "reset button removed");
            await FocusWorld(client, token);
            await client.Send("key Space");
            UiObservation reset = await WaitUi(client, p => Math.Abs(p.Camera.Zoom - 1) < .001 && Math.Abs(p.Camera.PanX) < .001 && Math.Abs(p.Camera.PanZ) < .001, "Space resets overview", token);
            Require(reset.SelectedSlot == initial.SelectedSlot, "Space reset retains plot selection");
            return;
        }
        if (name.StartsWith("City", StringComparison.Ordinal) && int.TryParse(name.AsSpan(4), out int cityId))
        {
            UiObservation initial = await UiProtocol.Probe(client, options.StartupTimeout, token);
            for (int i = 0; initial.ObservedCity != cityId && i < initial.CityIds.Length; i++)
            {
                await Click(client, "NextCity", token);
                initial = await WaitUi(client, p => p.ObservedCity != initial.ObservedCity, "city arrow switches observation", token);
            }
            Require(initial.ObservedCity == cityId, "city selector reaches requested cooperative city"); return;
        }
        if (name == "ReturnToMenu")
        {
            UiObservation initial = await UiProtocol.Probe(client, options.StartupTimeout, token);
            if (!initial.SettingsOpen) await Click(client, "Settings", token);
        }
        if (Enum.TryParse(name, out Building building) && building != Building.Empty)
        {
            UiObservation initial = await UiProtocol.Probe(client, options.StartupTimeout, token);
            if (initial.Targets.TryGetValue(name, out UiTarget? construction) && !construction.Visible)
            {
                string group = building switch { Building.Barracks or Building.ArcheryRange or Building.Arcanum or Building.ResearchTower => "Army", Building.ArrowTower or Building.CatapultTower => "Defense", Building.Market => "Trade", _ => "Production" };
                await Click(client, group + "Choices", token);
            }
        }
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
        MatchSnapshot accepted = State(result);
        if (accepted.Phase is Phase.Building or Phase.Preparation)
            await WaitUi(client, frame => frame.Revision >= accepted.Revision && accepted.Players.FirstOrDefault(city => city.Id == frame.ObservedCity) is { } city
                && Enum.GetValues<Resource>().All(resource => frame.ResourceBalances.GetValueOrDefault(resource.ToString(), -1) == city.Resources.Amount(resource)
                    && frame.ResourceIncome.GetValueOrDefault(resource.ToString()) == IncomeText(city.ProductionIncome, resource)), "exact displayed resource balances after accepted action", token);
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
            string expectedRole = RomanLevel(unit.Level);
            if (unit.Dead || !unit.Visible || !unit.Deployed || bar.Current != unit.Health || bar.Maximum != unit.MaximumHealth
                || bar.Role != expectedRole
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
        Branding(frame);
        Require(frame.UiProvenanceBundled, "kit provenance and permission documentation bundled with UI assets");
        Require(frame.PanelTexture == "res://Assets/TrioUI/cozy/panel_plain.svg" && frame.FocusBorder == 2, "actual kit panel and keyboard focus styles loaded");
        Require(frame.ButtonTextures.Length == 4 && frame.ButtonTextures.Distinct().Count() == 4 && frame.ButtonTextures.All(p => p.StartsWith("res://Assets/TrioUI/derived/button_", StringComparison.Ordinal)), "four distinct kit button-state textures");
        Require(frame.SliderTexture == "res://Assets/TrioUI/derived/slider_knob.svg", "kit slider knob loaded");
        if (frame.SettingsOpen || frame.FriendsOpen || frame.JoinConfirmationOpen || frame.ReturnConfirmationOpen)
            Require(frame.DialogTexture == "res://Assets/TrioUI/cozy/panel_plain.svg", "actual owned dialog uses shared kit panel");
        if (frame.SettingsOpen)
            Require(frame.TabTextures.Length == 2 && frame.TabTextures.Distinct().Count() == 2, "selected and unselected tabs use distinct kit textures");
        if (frame.DropdownOpen)
            Require(frame.DropdownTexture == "res://Assets/TrioUI/cozy/panel_plain.svg", "actual dropdown popup uses shared kit panel");
        if (frame.Screen == "session")
        {
            Require(frame.ResourceRowsSingleLine && frame.HudHeight <= 190, "six exact single-line balances and approximately 180px HUD");
            Require(frame.ResourceOrder.SequenceEqual(new[] { "Gold", "Food", "Wood", "Stone", "Metal", "Cloth" }), "resource table uses requested row order");
            Require(EconomyResourceLabels.All(resource => frame.ResourceBalances.ContainsKey(resource)) && frame.UpkeepText.Length != 0, "six resources including zero and separate Details upkeep");
            Require(frame.ResourceIcons.Values.All(icon => icon.Length == 0) && frame.Targets.Values.All(target => target.Icon.Length == 0), "text controls and resource table omit semantic icons");
            Require(!frame.Targets.ContainsKey("ResetView") && (!frame.Targets.TryGetValue("ReturnToMenu", out UiTarget? leave) || !leave.Visible || frame.SettingsOpen), "reset and leave removed from bottom HUD");
            if (frame.Connected && frame.HomeHealth.Maximum > 0) Require(frame.HomeHealth.InputIgnored && frame.HomeHealth.PercentageInside && frame.HomeHealth.Percent == (int)Math.Round(PresentationLimits.HealthFraction(frame.HomeHealth.Current, frame.HomeHealth.Maximum) * 100, MidpointRounding.AwayFromZero), "home percentage uses matching authoritative health scales");
            foreach (var (name, plot) in frame.Targets.Where(t => t.Key.StartsWith("Plot", StringComparison.Ordinal) && t.Value.Visible))
                Require(plot.X >= 0 && plot.X <= frame.Width && plot.Y >= 0 && plot.Y < frame.HudTop, "plot selectable above HUD: " + name);
            HealthBars(frame);
        }
        foreach (var (name, target) in frame.Targets.Where(t => t.Value.Visible && !t.Key.StartsWith("Plot", StringComparison.Ordinal) && !t.Key.StartsWith("InviteFriend", StringComparison.Ordinal)))
            Require(target.X - target.Width / 2 >= -1 && target.X + target.Width / 2 <= frame.Width + 1 && target.Y - target.Height / 2 >= -1 && target.Y + target.Height / 2 <= frame.Height + 1,
                "complete control bounds inside viewport: " + name);
    }
    private async Task Checkpoint(Child client, string name, CancellationToken token, string dataOwner = "ui-client", Action<UiObservation>? validate = null)
    {
        string path = Path.Combine(_scope!.EvidenceDirectory, name + ".png");
        UiObservation frame = await UiProtocol.Probe(client, options.StartupTimeout, token, path);
        UiProtocol.Frame(frame, path);
        validate?.Invoke(frame);
        MatchSnapshot current = Latest(client);
        CityState? observedCity = current.Players.FirstOrDefault(city => city.Id == frame.ObservedCity);
        if (observedCity is not null && frame.Revision == current.Revision)
        {
            Require(frame.HomeHealth.Current == observedCity.Health && frame.HomeHealth.Maximum == HealthPoints.FromWhole(current.Rules.CityHealth), "home bar follows authoritative city health and maximum");
            Require(Enum.GetValues<Resource>().All(resource => frame.ResourceBalances[resource.ToString()] == observedCity.Resources.Amount(resource)), "observed resource amounts match exact authoritative balances including zero");
        }
        TrioPresentation(frame);
        Countryside(frame);
        Require(frame.Display == "X11" && frame.Models > 0 && frame.Materials > 0 && frame.MusicLoaded, "rendered UI/models/materials/music loaded on owned X11 display");
        Require(frame.UnitBindings.Length == Game.AssetCatalog.RequiredPaths.Count() + 8, "all required authored models and eight faction/role rigs, clips and equipment sockets imported");
        Require(frame.AudioDriver == "Dummy" && (frame.Renderer.Contains("llvmpipe", StringComparison.OrdinalIgnoreCase) || frame.Renderer.Contains("softpipe", StringComparison.OrdinalIgnoreCase)), "actual client uses silent Dummy audio and Mesa software rendering");
        Require(Path.GetFullPath(frame.UserDataPath).StartsWith(Path.Combine(_scope.Directory, dataOwner, "data") + Path.DirectorySeparatorChar, StringComparison.Ordinal), "effective user:// belongs to this client scope");
        await File.WriteAllTextAsync(Path.Combine(_scope.EvidenceDirectory, name + "-observation.json"), JsonSerializer.Serialize(frame, Evidence.JsonOptions), token);
        Console.WriteLine($"FRAME {name}: {frame.Width}x{frame.Height}, colors={frame.Colors}, renderer={frame.Renderer}; {path}");
    }
    private async Task UiScenario(string name, CancellationToken token)
    {
        if (name == "economy" && options.UiCheckpoint == "army") { await ArmyUiScenario(token); return; }
        if (options.UiCheckpoint is null) Console.WriteLine($"UI risk: {name}: {UiRisk(name)}");
        if (options.UiCheckpoint == "melee") Console.WriteLine("Melee checkpoint risk: fixed-anchor swings without a readable target, missed shared near/far occupants or conflicting routes. Ordinary six-Swordsman opening and recruitment up to wave two, four paused overview/close PNGs and live-node witnesses; 65s checkpoint/70s scenario bounds. Owned peers/display/data cleanup uses the existing combat slice.");
        using var combatDeadline = CancellationTokenSource.CreateLinkedTokenSource(token);
        if (name == "combat" && options.UiCheckpoint is "melee" or "research")
            combatDeadline.CancelAfter(options.UiCheckpoint == "melee" ? 70000 : 150000);
        token = combatDeadline.Token;
        bool package = name == "exported-package";
        string? executable = package ? Path.Combine(_root, "dist", "client", "odot.x86_64") : null;
        var started = await StartServer("ui-server", options.Port ?? _scope!.Port(), token, retryAutomatic: options.Port is null, exported: package ? Path.Combine(_root, "dist", "server", "odot.x86_64") : null,
            extra: name == "economy" ? ["--combat-seed", "14056307608042553509"] : name is "combat" or "exported-package" ? ["--combat-seed", "1"] : []);
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
                await SimulationSpeed(started.Server, 1, token);
                await CameraControls(client, !package, token);
                await SimulationSpeed(started.Server, options.SimulationSpeed, token);
                await Pick(client, 0, token);
                int gold = Latest(client).Players.Single(p => p.Id == client.PlayerId).Gold;
                UiObservation costs = await UiProtocol.Probe(client, options.StartupTimeout, token);
                ResourceCost farmCost = Latest(client).BuildingCatalog.Single(b => b.Type == Building.Farm).Construction;
                Require(costs.Targets["Farm"].CostText.Contains(farmCost.Wood + " wood", StringComparison.Ordinal) && !costs.Targets["Farm"].CostText.Contains("gold", StringComparison.Ordinal), "visible basic-building cost retains exact wood quote without gold");
                GameEvent purchase = await ClickAck(client, "Farm", token);
                await Observe(observer, s => s.Revision >= State(purchase).Revision && s.Players.Single(p => p.Id == client.PlayerId).Slots[0].Type == Building.Farm, "observer sees UI purchase", token);
                Require(State(purchase).Players.Single(p => p.Id == client.PlayerId).Gold == gold - farmCost.Gold, "UI purchase uses the authoritative construction quote once");
                if (!package)
                {
                    await EconomyDetails(client, observer, token);
                }
                await Checkpoint(client, package ? "packed-building" : "economy-building", token);
                if (!package)
                {
                    await _scope!.CompleteGames(token);
                    await ArmyUiScenario(token);
                }
                break;
            case "combat":
                if (options.UiCheckpoint == "admission") { await MeleeAdmissionDiagnostic(client, observer, token); break; }
                if (options.UiCheckpoint == "melee")
                {
                    await CoordinatedMelee(client, observer, token);
                    break;
                }
                if (options.UiCheckpoint == "research") { await ResearchCheckpoint(client, observer, token); break; }
                await SpecialistArmy(client, observer, token);
                await CombatCheckpoint(client, observer, token);
                break;
            case "reconnect":
                await MixedArmy(client, observer, token);
                await SimulationSpeed(_scope!.Children.First(c => c.Name.StartsWith("ui-server", StringComparison.Ordinal)), 1, token);
                long reconnectCasualtyAfter = Latest(observer).Tick;
                await Observe(observer, s => s.DyingBodies.Any(u => u.Destination == client.PlayerId
                    && u.Hex!.DeathStartTick > reconnectCasualtyAfter && u.Hex.DeathEndTick > s.Tick + 12), "fresh natural casualty before reconnect", token);
                MatchSnapshot deathPause = State(await Action(observer, "pause", token));
                await Observe(client, s => s.Paused && s.Tick == deathPause.Tick, "graphical casualty pause barrier", token);
                Require(deathPause.DyingBodies.Any(u => u.Destination == client.PlayerId), "pause retains a current casualty");
                await UnitInspection(client, "reconnect-unit-inspection", token);
                int identity = client.PlayerId, connection = client.PeerId;
                string before = Gameplay(Latest(client));
                UiObservation cameraBefore = await CameraZoom(client, token);
                UiObservation inspectedBeforeLoss = await OpenUnitInspector(client, token);
                await client.Send("disconnect");
                await client.WaitFor(e => e.Type == "server-disconnected", "local transport disconnected", options.StartupTimeout, token);
                await Observe(observer, s => !s.Players.Single(p => p.Id == identity).Connected, "observer sees absent city", token);
                UiObservation lost = await UiProtocol.Probe(client, options.StartupTimeout, token);
                Require(lost.InspectedUnit == inspectedBeforeLoss.InspectedUnit, "transport loss freezes inspector health and profile");
                Require(SameCamera(cameraBefore.Camera, (await UiProtocol.Probe(client, options.StartupTimeout, token)).Camera), "transport loss retains local view");
                await HoldPan(client, ["D"], token);
                UiObservation cameraDisconnected = await UiProtocol.Probe(client, options.StartupTimeout, token);
                await Click(client, "Reconnect", token);
                GameEvent resumed = await client.WaitFor(e => e.Type == "connected" && e.PeerId != connection, "Reconnect control restores a new transport", options.StartupTimeout, token);
                Require(resumed.PlayerId == identity && Gameplay(State(resumed)) == before, "Reconnect input preserves city identity and gameplay");
                await Observe(observer, s => s.Players.Single(p => p.Id == identity).Connected && s.Revision >= State(resumed).Revision, "observer sees restored identity", token);
                UiObservation restored = await WaitUi(client, p => p.Units.Length == CombatPlayback.All(State(resumed)).Length + State(resumed).DyingBodies.Length && p.EventCursor == State(resumed).EventSequence,
                    "restored current unit baseline", token);
                Require(SameCamera(cameraDisconnected.Camera, restored.Camera), "same-match reconnect preserves adjusted camera");
                await Pick(client, 2, token);
                await Click(client, "ResetView", token);
                CityState resumedCity = State(resumed).Players.Single(city => city.Id == restored.ObservedCity);
                Require(Enum.GetValues<Resource>().All(resource => restored.ResourceBalances[resource.ToString()] == resumedCity.Resources.Amount(resource)), "reconnect reconstructs exact displayed balances including zero");
                HealthBars(restored);
                foreach (HealthBarObservation bar in restored.HealthBars)
                {
                    UnitState expected = CombatPlayback.All(State(resumed)).Single(u => u.Id == bar.Id);
                    Require(bar.Current == expected.Health && bar.Maximum == expected.Profile.Health && bar.Fraction == PresentationLimits.HealthFraction(expected.Health, expected.Profile.Health), "reconnect reconstructs current authoritative health fraction");
                }
                Require(restored.Units.All(u => u.EffectSequence == 0) && restored.Effects.Active == 0 && restored.Effects.Voices == 0, "reconnect baselines current poses without historical effects");
                Require(restored.Units.Where(u => u.Dead).Select(u => u.Id).Order().SequenceEqual(deathPause.DyingBodies.Select(u => u.Id).Order()), "reconnect restores current dying bodies");
                Require(restored.Units.Any(u => u.Type == UnitType.Crossbowman && u.WeaponAttached), "reconnect preserves ranged rig and profile");
                await Checkpoint(client, "restored-connection", token);
                await client.Send("quit"); Require(await client.WaitExit(token) == 0, "reconnect client exits cleanly for process restart");
                await Observe(observer, s => !s.Players.Single(p => p.Id == identity).Connected, "owned client absent before restart", token);
                Child deathRestart = StartGame("ui-client", false, false, started.Port);
                GameEvent deathWelcome = await deathRestart.WaitFor(e => e.Type == "connected", "owned client restarts during paused death", options.StartupTimeout, token);
                Require(deathWelcome.PlayerId == identity && Gameplay(State(deathWelcome)) == before, "process restart restores paused health and actions without regrant");
                UiObservation deathBaseline = await WaitUi(deathRestart, p => p.Connected && p.Units.Length == restored.Units.Length && p.EventCursor == deathPause.EventSequence, "restarted current death baseline", token);
                foreach (UnitState body in deathPause.DyingBodies)
                {
                    UnitObservation pose = deathBaseline.Units.Single(u => u.Id == body.Id);
                    Require(pose.Dead && pose.Hex == body.Hex && pose.EffectSequence == 0, "restart reconstructs the original death interval and reservations");
                }
                Require(deathBaseline.Effects.Active == 0 && deathBaseline.Effects.Voices == 0, "process restart does not replay historical sound or effects");
                await Checkpoint(deathRestart, "restarted-death", token);
                long deathDeadline = deathPause.DyingBodies.Max(u => u.Hex!.DeathEndTick);
                int[] dyingIds = deathPause.DyingBodies.Select(u => u.Id).ToArray();
                await ClickAck(deathRestart, "Pause", token);
                MatchSnapshot releasedDeath = await Observe(observer, s => s.Tick >= deathDeadline && s.DyingBodies.All(u => !dyingIds.Contains(u.Id)), "resumed deaths expire at their declared deadline", token);
                CombatContact(releasedDeath);
                await WaitUi(deathRestart, p => p.Units.All(u => !dyingIds.Contains(u.Id)), "expired death models removed after resume", token);
                break;
            case "settings":
                await SimulationSpeed(started.Server, 1, token);
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
