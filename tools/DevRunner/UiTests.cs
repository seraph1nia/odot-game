using System.Text;
using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed record UiTarget(float X, float Y, bool Visible, bool Enabled);
internal sealed record UiObservation
{
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
    public string PhaseText { get; init; } = "";
    public string StatusText { get; init; } = "";
    public string DetailText { get; init; } = "";
    public string RosterText { get; init; } = "";
    public long Revision { get; init; }
    public int SelectedSlot { get; init; }
    public bool Connected { get; init; }
    public bool SettingsOpen { get; init; }
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
    public int Materials { get; init; }
    public bool MusicLoaded { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int Colors { get; init; }
    public string? Screenshot { get; init; }
    public Dictionary<string, UiTarget> Targets { get; init; } = [];
}

internal static class UiProtocol
{
    public static async Task<UiObservation> Probe(Child client, int timeout, CancellationToken token, string? screenshot = null)
    {
        string id = Guid.NewGuid().ToString("N");
        await client.Send("ui-probe " + id + (screenshot is null ? "" : " " + Convert.ToBase64String(Encoding.UTF8.GetBytes(screenshot))));
        GameEvent response = await client.WaitFor(e => e.Type == "ui" && JsonSerializer.Deserialize<UiObservation>(e.Message!)?.Id == id, "fresh UI response " + id, timeout, token);
        UiObservation value = JsonSerializer.Deserialize<UiObservation>(response.Message!)!;
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
        "economy" => "Picking/control routing to authority and rendered assets; headless tests miss input and presentation.",
        "reconnect" => "Visible recovery control and retained presentation/identity; headless resume cannot exercise the button.",
        "settings" => "Modal input leakage and preference isolation/persistence; numerical rules tests cannot observe the UI.",
        "launcher" => "Application navigation, local session transitions and actual process exit; core/network checks miss visible controls.",
        "exported-package" => "Packed-resource loading and actual UI input; source tests cannot detect package-only omissions.",
        "installed-linux" => "Installed launcher, packed presentation and normal input; archive inventory alone cannot establish an installed graphical launch.",
        _ => throw new ArgumentException("Unknown UI scenario: " + name)
    };
    private async Task<UiObservation> WaitUi(Child client, Func<UiObservation, bool> predicate, string expectation, CancellationToken token)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token); deadline.CancelAfter(options.StartupTimeout);
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
        await ClickPoint(client, target);
    }
    private static Task ClickPoint(Child client, UiTarget target)
        => client.Send(FormattableString.Invariant($"click {target.X} {target.Y}"));
    private async Task<GameEvent> ClickAck(Child client, string name, CancellationToken token)
    {
        long previous = client.History().Where(e => e.Type == "ack").Select(e => e.Result!.Sequence).DefaultIfEmpty().Max();
        await Click(client, name, token);
        GameEvent result = await client.WaitFor(e => e.Type == "ack" && e.Result!.Sequence > previous, name + " authoritative result", options.StartupTimeout, token);
        Require(result.Result!.Accepted, $"{name} input accepted through normal protocol: {result.Message}");
        return result;
    }
    private async Task Pick(Child client, int slot, CancellationToken token)
    {
        await Click(client, "Plot" + slot, token);
        await WaitUi(client, p => p.SelectedSlot == slot, "selected plot " + slot, token);
    }
    private async Task Checkpoint(Child client, string name, CancellationToken token)
    {
        string path = Path.Combine(_scope!.EvidenceDirectory, name + ".png");
        UiObservation frame = await UiProtocol.Probe(client, options.StartupTimeout, token, path);
        UiProtocol.Frame(frame, path);
        Require(frame.Display == "X11" && frame.Models > 0 && frame.Materials > 0 && frame.MusicLoaded, "rendered UI/models/materials/music loaded on owned X11 display");
        Require(frame.AudioDriver == "Dummy" && (frame.Renderer.Contains("llvmpipe", StringComparison.OrdinalIgnoreCase) || frame.Renderer.Contains("softpipe", StringComparison.OrdinalIgnoreCase)), "actual client uses silent Dummy audio and Mesa software rendering");
        Require(Path.GetFullPath(frame.UserDataPath).StartsWith(Path.Combine(_scope.Directory, "ui-client", "data") + Path.DirectorySeparatorChar, StringComparison.Ordinal), "effective user:// belongs to this client scope");
        await File.WriteAllTextAsync(Path.Combine(_scope.EvidenceDirectory, name + "-observation.json"), JsonSerializer.Serialize(frame, Evidence.JsonOptions), token);
        Console.WriteLine($"FRAME {name}: {frame.Width}x{frame.Height}, colors={frame.Colors}, renderer={frame.Renderer}; {path}");
    }
    private async Task UiScenario(string name, CancellationToken token)
    {
        Console.WriteLine($"UI risk: {name}: {UiRisk(name)}");
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
                await Pick(client, 0, token);
                int gold = Latest(client).Players.Single(p => p.Id == client.PlayerId).Gold;
                GameEvent purchase = await ClickAck(client, "Farm", token);
                await Observe(observer, s => s.Revision >= State(purchase).Revision && s.Players.Single(p => p.Id == client.PlayerId).Slots[0].Type == Building.Farm, "observer sees UI purchase", token);
                Require(State(purchase).Players.Single(p => p.Id == client.PlayerId).Gold == gold - State(purchase).Rules.BuildCost, "UI purchase spends authoritative gold once");
                if (!package)
                {
                    await Pick(client, 0, token);
                    GameEvent upgrade = await ClickAck(client, "Upgrade", token);
                    await Observe(observer, s => s.Revision >= State(upgrade).Revision && s.Players.Single(p => p.Id == client.PlayerId).Slots[0].Level == 2, "observer sees UI upgrade", token);
                    await Pick(client, 1, token);
                    await ClickAck(client, "Barracks", token);
                    // Production is setup, not another UI flow under test.
                    await Advance([client, observer], token);
                    await Pick(client, 1, token);
                    GameEvent recruit = await ClickAck(client, "Recruit", token);
                    await Observe(observer, s => s.Revision >= State(recruit).Revision && s.Players.Single(p => p.Id == client.PlayerId).Soldiers.Length == 1, "observer sees UI recruitment", token);
                }
                await Checkpoint(client, package ? "packed-building" : "economy-building", token);
                break;
            case "reconnect":
                int identity = client.PlayerId, connection = client.PeerId;
                string before = Gameplay(Latest(client));
                await client.Send("disconnect");
                await client.WaitFor(e => e.Type == "server-disconnected", "local transport disconnected", options.StartupTimeout, token);
                await Observe(observer, s => !s.Players.Single(p => p.Id == identity).Connected, "observer sees absent city", token);
                await Click(client, "Reconnect", token);
                GameEvent resumed = await client.WaitFor(e => e.Type == "connected" && e.PeerId != connection, "Reconnect control restores a new transport", options.StartupTimeout, token);
                Require(resumed.PlayerId == identity && Gameplay(State(resumed)) == before, "Reconnect input preserves city identity and gameplay");
                await Observe(observer, s => s.Players.Single(p => p.Id == identity).Connected && s.Revision >= State(resumed).Revision, "observer sees restored identity", token);
                await Checkpoint(client, "restored-connection", token);
                break;
            case "settings":
                await Pick(client, 0, token);
                UiObservation closed = await UiProtocol.Probe(client, options.StartupTimeout, token);
                UiTarget behind = UiProtocol.Target(closed, "Farm");
                await Click(client, "Settings", token);
                await WaitUi(client, p => p.SettingsOpen, "settings modal open", token);
                await ClickPoint(client, behind);
                await WaitUi(client, p => p.SettingsOpen, "modal retains input", token);
                GameEvent barrier = await Action(observer, "unknown", token, false);
                Require(State(barrier).Players.Single(p => p.Id == client.PlayerId).Slots[0].Type == Building.Empty, "settings modal blocks gameplay input behind it");
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
                Require(Math.Abs(changed.MasterGain - changed.MasterVolume / 100f) < 0.001f && changed.MasterMuted == (changed.MasterVolume == 0), "Master state follows actual slider input (silent Dummy audio)");
                await Checkpoint(client, "settings-audio", token);
                await Click(client, "CloseSettings", token);
                await WaitUi(client, p => !p.SettingsOpen, "settings closed and saved", token);
                int originalId = client.PlayerId;
                await client.Send("quit"); Require(await client.WaitExit(token) == 0, "settings client exits cleanly");
                await Observe(observer, s => !s.Players.Single(p => p.Id == originalId).Connected, "restart absent city", token);
                Child restarted = StartGame("ui-client", false, false, started.Port);
                GameEvent recovered = await restarted.WaitFor(e => e.Type == "connected", "settings client restarted", options.StartupTimeout, token);
                UiObservation persisted = await WaitUi(restarted, p => p.Connected && p.Models > 0, "restarted settings ready", token);
                Require(recovered.PlayerId == originalId && persisted.MasterVolume == changed.MasterVolume && Math.Abs(persisted.MasterGain - changed.MasterGain) < 0.001f, "owned preference survives client restart without regrant");
                await Checkpoint(restarted, "settings-restarted", token);
                break;
        }
    }
}
