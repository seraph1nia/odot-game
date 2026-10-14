using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    private async Task ResearchCheckpoint(Child client, Child observer, CancellationToken token)
    {
        Console.WriteLine("Research checkpoint risk: actual global purchase routing, permanent sibling lock, current burn badge/inspection and owned Reconnect control restoring active statuses without historical cues; ordinary earned points, at most wave ten; setup bound 120s, feature bound 60s; existing owned combat display/peers and cleanup.");
        Child server = _scope!.Children.First(c => c.Name.StartsWith("ui-server", StringComparison.Ordinal));
        await _evidence.Measure("research-setup", "checkpoint", async () =>
        {
            using var setup = CancellationTokenSource.CreateLinkedTokenSource(token); setup.CancelAfter(120000);
            await SimulationSpeed(server, options.SimulationSpeed, setup.Token);
            async Task Invest(Child child)
            {
                for (int n = 0; n < 100; n++)
                {
                    MatchSnapshot state = Latest(child);
                    EconomyAction? action = CampaignStrategy.ResearchWitness(state, child.PlayerId);
                    if (action is null) return;
                    string command = action.Action switch { "build" => $"{(action.Payment == ConstructionPayment.GoldRecovery ? "build-recovery" : "build")} {action.Slot} {action.Building}", "recruit" => $"recruit {action.Slot} {action.Unit}", "trade" => $"trade {action.Slot} {action.Resource} {action.Bundles}", "retire" => $"retire {action.UnitId}", _ => $"{action.Action} {action.Slot}" };
                    await Action(child, command, setup.Token);
                }
                throw new InvalidOperationException("Research setup action bound exceeded.");
            }
            while (true)
            {
                MatchSnapshot state = Latest(client); Require(state.Phase is not (Phase.Victory or Phase.Defeat) && state.Wave <= 10, "research opening remains an ordinary early match");
                if (state.Paused) await Action(observer, "resume", setup.Token);
                if (state.Phase == Phase.Combat) { await UiClear(client, observer, state.Wave, setup.Token); continue; }
                await Invest(client); await Invest(observer);
                CityState city = Latest(client).Players.Single(c => c.Id == client.PlayerId);
                if (!city.Research.Has(TechnologyId.MagicFoundation) && city.Research.Points >= 3)
                {
                    await Click(client, "Research", setup.Token); await Click(client, "Node_primary_center", setup.Token);
                    await ClickAck(client, "PurchaseResearch", setup.Token); await Click(client, "CloseResearch", setup.Token);
                }
                city = Latest(client).Players.Single(c => c.Id == client.PlayerId);
                if (city.Research.Points >= 6 && city.Research.Has(TechnologyId.MagicFoundation) && city.Soldiers.Any(u => u.Type == UnitType.Mage)) break;
                await UiReadyPair(client, observer, setup.Token, actualInput: false);
            }
        });
        await _evidence.Measure("research-controls", "checkpoint", async () =>
        {
            using var feature = CancellationTokenSource.CreateLinkedTokenSource(token); feature.CancelAfter(60000);
            await SimulationSpeed(server, 1, feature.Token);
            await Click(client, "Research", feature.Token); await Click(client, "Node_secondary_center_b", feature.Token);
            UiObservation before = await UiProtocol.Probe(client, options.StartupTimeout, feature.Token);
            if (options.UiCheckpoint == "research") Require(before.SelectedSlot == -1, "research purchases need no selected plot");
            Require(before.Targets["PurchaseResearch"].Enabled, "Frost is initially available");
            await Click(client, "Node_secondary_center_a", feature.Token);
            Require((await UiProtocol.Probe(client, options.StartupTimeout, feature.Token)).Targets["PurchaseResearch"].Enabled, "Fire is initially available");
            int points = Latest(client).Players.Single(c => c.Id == client.PlayerId).Research.Points;
            await ClickAck(client, "PurchaseResearch", feature.Token);
            await Click(client, "Node_secondary_center_b", feature.Token);
            UiObservation locked = await WaitUi(client, p => p.ResearchText.Contains("Permanently locked", StringComparison.Ordinal), "acknowledged Frost lock", feature.Token);
            Require(!locked.Targets["PurchaseResearch"].Enabled && Latest(client).Players.Single(c => c.Id == client.PlayerId).Research.Points == points - 6, "Fire spends six actual points and locks Frost");
            await Click(client, "CloseResearch", feature.Token);
            while (Latest(client).Phase is Phase.Building or Phase.Preparation) await UiReadyPair(client, observer, feature.Token, actualInput: false);
            var capture = await PauseBurn(observer, client.PlayerId, feature.Token);
            MatchSnapshot paused = State(capture.Receipt);
            await ObserveCurrent(client, s => s.MatchId == paused.MatchId && s.Revision >= paused.Revision && s.Paused && s.Tick == paused.Tick,
                "current burn pause receipt barrier", feature.Token);
            UiObservation burning = await WaitUi(client, p => p.CombatTick == paused.Tick && p.Units.Any(u => u.Id == capture.TargetId && !u.Dead), "current captured burn actor", feature.Token);
            Require(burning.StatusBadges.Count == 0, "active statuses never restore floating actor text");
            UnitObservation target = burning.Units.Single(u => u.Id == capture.TargetId);
            await ClickPoint(client, burning.Targets["Unit" + target.Id]);
            await WaitUi(client, p => p.InspectedUnit?.StatsText.Contains("Burn", StringComparison.Ordinal) == true, "burn inspection", feature.Token);
            await Checkpoint(client, "research-current-burn", feature.Token);
            UiObservation still = await UiProtocol.Probe(client, options.StartupTimeout, feature.Token);
            Require(still.CombatTick == burning.CombatTick, "pause freezes status countdown");
            int identity = client.PlayerId, connection = client.PeerId;
            string retained = Gameplay(Latest(client));
            await client.Send("disconnect");
            await client.WaitFor(e => e.Type == "server-disconnected", "research view loses owned transport", options.StartupTimeout, feature.Token);
            UiObservation disconnected = await UiProtocol.Probe(client, options.StartupTimeout, feature.Token);
            Require(disconnected.CombatTick == still.CombatTick && disconnected.InspectedUnit?.StatsText == still.InspectedUnit?.StatsText,
                "transport loss freezes useful inspection status without floating labels");
            await Click(client, "Reconnect", feature.Token);
            GameEvent restored = await client.WaitFor(e => e.Type == "connected" && e.PeerId != connection, "research view reconnects through its control", options.StartupTimeout, feature.Token);
            Require(restored.PlayerId == identity && Gameplay(State(restored)) == retained, "graphical recovery retains personal research and active status deadlines");
            UiObservation baseline = await WaitUi(client, p => p.EventCursor == State(restored).EventSequence && p.StatusBadges.Count == 0,
                "current status baseline after reconnect", feature.Token);
            await Click(client, "Unit" + target.Id, feature.Token);
            await WaitUi(client, p => p.InspectedUnit?.Id == target.Id && p.InspectedUnit.StatsText == still.InspectedUnit?.StatsText,
                "current inspected status after reconnect", feature.Token);
            Require(baseline.Units.All(u => u.EffectSequence == 0) && baseline.Effects.Active == 0 && baseline.Effects.Voices == 0, "active status restoration replays no historical flashes or sounds");
            await Action(observer, "resume", feature.Token);
        });
    }
}
