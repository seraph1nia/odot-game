using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    // Tier E, opt-in extension of the existing RX6800 UiSmoke/Roguelite owner.
    // Numerical/native tests cannot prove pixels, full-app modal routing or the
    // same-tick rendered transition. One seed-1 solo, ordinary paid first wave,
    // six viewport sizes; no state loader, extra supervisor or CI GPU dependency.
    // Uses existing ordered Child events and owned viewport capture/cleanup.
    private async Task HorizontalBoardWitness(Child client, CancellationToken token)
    {
        Require(options.DesktopSurface, "horizontal witness requires explicitly authorized hardware surface");
        async Task Resize(int width, int height)
        {
            UiObservation current = await UiProtocol.Probe(client, options.StartupTimeout, token);
            NativeWindowClose.Resize(client, options, current.NativeWindow, (uint)width, (uint)height);
            await WaitUi(client, p => p.Width == width && p.Height == height, "owned viewport resize", token);
        }
        async Task Invest()
        {
            for (int n = 0; n < 100; n++)
            {
                EconomyAction? decision = CampaignStrategy.Next(Latest(client), client.PlayerId);
                if (decision is null) return;
                string command = decision.Action switch
                {
                    "build" => $"{(decision.Payment == ConstructionPayment.GoldRecovery ? "build-recovery" : "build")} {decision.Slot} {decision.Building}",
                    "recruit" => $"recruit {decision.Slot} {decision.Unit}",
                    "retire" => $"retire {decision.UnitId}",
                    "research-tech" => $"research-tech {TechnologyIds.Name(decision.Technology)}",
                    "trade" => $"trade {decision.Slot} {decision.Resource} {decision.Bundles}",
                    _ => $"{decision.Action} {decision.Slot}"
                };
                await Action(client, command, token);
            }
            throw new InvalidOperationException("Bounded first-wave investment exhausted.");
        }
        await Invest(); await ClickAck(client, "Ready", token);
        await Invest(); await ClickAck(client, "Ready", token);
        await Invest();
        await WaitUi(client, p => p.MatchPhase == Phase.Preparation && p.SharedArenaVisible && p.Units.Any(u => u.Id > 0 && u.Deployed), "paid preparation figures", token);
        foreach ((int width, int height) in new[] { (1100, 820), (1280, 720), (1600, 900), (1920, 1080), (2560, 1080), (820, 1100) })
        {
            await Resize(width, height); await Click(client, "ResetView", token);
            UiObservation frame = await WitnessFrame(client, $"horizontal-{width}x{height}", token);
            Require(frame.ArenaFrame is { OpaqueCoverage: true } && frame.Camera.Rotation[1] == 0, "horizontal cloud-bounded shared arena");
            UiTarget ledger = frame.Targets["ResourceTable"];
            foreach (int slot in Enumerable.Range(0, 15))
            {
                UiTarget plot = UiProtocol.Target(frame, "Plot" + slot);
                Require(plot.X >= 0 && plot.X < ledger.X - ledger.Width / 2 && plot.Y >= 64 && plot.Y < frame.HudTop, "all building/home rays inside measured UI safe area");
                if (slot < 9) await Pick(client, slot, token);
                else
                {
                    // A standing member can occlude the home center. Click its
                    // exposed near edge rather than selecting through a rig.
                    await client.Send(FormattableString.Invariant($"click {plot.X} {plot.Y + 12}"));
                    await WaitUi(client, p => p.SelectedSlot == slot, "exposed home edge selects " + slot, token);
                }
                Console.WriteLine($"PICK {width}x{height}: plot {slot}");
            }
        }
        await Resize(1920, 1080); await Click(client, "ResetView", token);
        UiObservation preparation = await WitnessFrame(client, "preparation-entry-poses", token);
        MatchSnapshot prepared = Latest(client);
        foreach (int id in new[] { preparation.Units.First(u => u.Id > 0 && u.Deployed).Id, preparation.Units.First(u => u.Id < 0 && u.Deployed).Id })
        {
            await Click(client, "Unit" + id, token);
            await WaitUi(client, p => p.InspectedUnit?.Id == id, "visible preparation member remains inspected", token);
            await WitnessFrame(client, "inspected-" + (id > 0 ? "soldier" : "forecast"), token);
            await Pick(client, 8, token);
        }
        await HorizontalModalWitness(client, "doctrine-modal", open: true, token);
        await Click(client, "CloseRun", token);
        await CameraZoom(client, token);
        UiObservation navigation = await WitnessFrame(client, "camera-zoom", token);
        await client.Send(FormattableString.Invariant($"mouse-down {navigation.Camera.ReferenceX} {navigation.Camera.ReferenceY}"));
        await client.Send(FormattableString.Invariant($"mouse-move {navigation.Camera.ReferenceX + 55} {navigation.Camera.ReferenceY + 30}"));
        await client.Send(FormattableString.Invariant($"mouse-up {navigation.Camera.ReferenceX + 55} {navigation.Camera.ReferenceY + 30}"));
        await WaitUi(client, p => !SameCamera(navigation.Camera, p.Camera), "drag changes camera after modal closes", token);
        await WitnessFrame(client, "camera-drag", token);
        await Click(client, "ResetView", token);
        Require(SameCamera(preparation.Camera, (await UiProtocol.Probe(client, options.StartupTimeout, token)).Camera), "normal input restored and reset returns compact fit");
        Require(Gameplay(Latest(client)) == Gameplay(prepared), "inspection/navigation/modal probing changes no gameplay state");
        preparation = await WitnessFrame(client, "preparation-before-ready", token);
        // These ordinary authority commands share the existing physics command
        // drain. A pointer click is deferred by Godot; batching it with Pause
        // disables Ready before its deferred release. Use the same ordinary
        // Ready action as the two preceding button clicks, then Pause, rather
        // than adding a simulation hook or comparing later moving poses.
        _ = UiProtocol.Target(preparation, "Ready");
        long floor = AckSequence(client);
        await client.Send("ready\npause");
        await client.WaitFor(e => e.Type == "ack" && e.Result!.Sequence > floor && e.State is { Phase: Phase.Combat, Paused: true }, "Ready plus ordinary tick-zero pause", options.StartupTimeout, token);
        MatchSnapshot entry = Latest(client);
        Require(entry.Tick == prepared.Tick && entry.Paused && entry.Phase == Phase.Combat, "live visible combat entry frozen before first combat tick");
        await WaitUi(client, p => p.MatchPhase == Phase.Combat && p.Paused && p.CombatTick == entry.Tick, "rendered tick-zero entry", token);
        UiObservation combat = await WitnessFrame(client, "combat-tick-zero", token);
        UnitState[] projected = BattleDeployment.Preview(prepared);
        UnitState[] enemies = projected.Where(u => u.Id < 0).ToArray();
        Require(combat.Units.Count(u => u.Visible && u.Deployed) == preparation.Units.Count(u => u.Visible && u.Deployed), "same number of preview and live field bodies");
        var poses = new List<object>();
        foreach (UnitState actual in entry.Players.SelectMany(c => c.Soldiers).Concat(entry.Enemies).Where(u => u.Deployed))
        {
            int oldId = actual.Id;
            if (actual.Faction == Faction.Skeletons) oldId = enemies[Array.FindIndex(entry.Enemies, u => u.Id == actual.Id)].Id;
            UnitObservation before = preparation.Units.Single(u => u.Id == oldId);
            UnitObservation after = combat.Units.Single(u => u.Id == actual.Id);
            Require(before.Visible && after.Visible && Math.Abs(before.X - after.X) < .001 && Math.Abs(before.Z - after.Z) < .001
                && Math.Abs(before.Heading - after.Heading) < .001 && before.FormationSlot == after.FormationSlot
                && before.Type == after.Type && before.Owner == after.Owner && before.Destination == after.Destination,
                "every rendered member keeps its exact entry root, formation and facing");
            poses.Add(new { Preview = before, Combat = after });
        }
        await File.WriteAllTextAsync(Path.Combine(_scope!.EvidenceDirectory, "entry-parity.json"), JsonSerializer.Serialize(new { Prepared = prepared, Entry = entry, Poses = poses }, Evidence.JsonOptions), token);
        await Action(client, "resume", token); await SimulationSpeed(client, 8, token); await ClickAck(client, "SpeedTwo", token);
        await ObserveCurrent(client, s => s.Phase is Phase.RewardDraft or Phase.Defeat, "ordinary earned first-wave outcome", token);
        Require(Latest(client).Phase == Phase.RewardDraft, "paid seeded first wave earns reward dialog");
        await WaitUi(client, p => p.RunOpen && p.MatchPhase == Phase.RewardDraft, "earned reward modal", token);
        await HorizontalModalWitness(client, "earned-reward-modal", open: false, token);
        string selected = CampaignStrategy.RewardChoice(Latest(client).Players.Single().Run);
        UiObservation choice = await UiProtocol.Probe(client, options.StartupTimeout, token);
        for (int n = 0; choice.Targets["Run_" + selected].Y > choice.Targets["CloseRun"].Y - 60 && n < 12; n++)
        {
            await client.Send("wheel 960 540 down"); choice = await UiProtocol.Probe(client, options.StartupTimeout, token);
        }
        await ClickAck(client, "Run_" + selected, token);
        await WaitUi(client, p => !p.RunOpen && p.MatchPhase == Phase.Building, "accepted reward restores normal world", token);
        await Pick(client, 0, token); await CameraZoom(client, token);
        await WitnessFrame(client, "reward-accepted-input-restored", token);
        await Click(client, "ReturnToMenu", token); await Click(client, "ConfirmReturn", token);
        await WaitUi(client, p => p.Screen == "menu" && !p.Connected, "owned return disposes first session", token);
        await Click(client, "Singleplayer", token);
        await WaitUi(client, p => p.MatchPhase == Phase.Lobby && p.Camera.Zoom == 1 && p.Camera.PanX == 0 && p.Camera.PanZ == 0, "fresh doctrine lobby resets camera", token);
        await WitnessFrame(client, "fresh-lobby-camera-reset", token);
        await Click(client, "ReturnToMenu", token); await Click(client, "ConfirmReturn", token);
        await WaitUi(client, p => p.Screen == "menu" && !p.Connected, "final return", token);
        await Click(client, "Exit", token); Require(await client.WaitExit(token) == 0, "owned horizontal witness Exit");
    }
    private async Task HorizontalModalWitness(Child client, string label, bool open, CancellationToken token)
    {
        if (open) { await Pick(client, 8, token); await Click(client, "RunProgression", token); }
        UiObservation before = await WaitUi(client, p => p.RunOpen, "real run modal", token);
        before = await WitnessFrame(client, label + "-before", token);
        // At 1920px the centered 960px dialog leaves actual left district plots
        // exposed. Attack that world ray, not a card/button that may legitimately
        // close/change the modal. Never use Escape (it opens Settings).
        var plot = before.Targets.Where(p => p.Key.StartsWith("Plot", StringComparison.Ordinal) && p.Value.X < (before.Width - 960) / 2f && p.Value.Y > 64 && p.Value.Y < before.HudTop)
            .OrderBy(p => p.Value.X).First();
        await Wheel(client, plot.Value.X, plot.Value.Y, true);
        await ClickPoint(client, plot.Value);
        await client.Send("key-down D");
        await UiProtocol.Probe(client, options.StartupTimeout, token);
        await client.Send("key-up D");
        UiObservation after = await WitnessFrame(client, label + "-after", token);
        await File.WriteAllTextAsync(Path.Combine(_scope!.EvidenceDirectory, label + "-input.json"), JsonSerializer.Serialize(new { Target = plot, Input = "wheel up; click; key-down D; probe; key-up D", Before = before, After = after }, Evidence.JsonOptions), token);
        Require(after.RunOpen && after.ModalOpen && before.SelectedSlot == after.SelectedSlot && SameCamera(before.Camera, after.Camera), "modal blocks world picking and camera input");
    }
}
