using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    // Small selectable vertical slice: actual inspector/hall input, stable ids,
    // physical homes and paid recovery cannot be proved by core transactions alone.
    // One owned solo window, three opening productions and two ordinary early waves;
    // no injected resources, outcomes, probes or desktop automation.
    private async Task ArmyUiScenario(CancellationToken token)
    {
        Console.WriteLine("Army checkpoint risk: misbound retire/store ids, hidden stored actors, wrong full-destination controls, coupled hall tracks or UI healing. One owned solo window, two paid early clears, bounded input and five PNGs; no complete graphical campaign.");
        // Hosted serial army completed in 229s; overlapping displays reached battle
        // two at 296s before parent cancellation. Allow measured contention headroom,
        // retaining the linked parent deadline and every assertion/capture.
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token); deadline.CancelAfter(Options.ArmyUiTimeout); token = deadline.Token;
        const string owner = "ui-army";
        await using Child client = StartGameRole(owner, "menu", false, 0, null, "--combat-seed", "1");
        await client.WaitFor(e => e.Type == "menu", "owned army menu", options.StartupTimeout, token);
        await SimulationSpeed(client, 1, token);
        await Click(client, "Singleplayer", token);
        await WaitUi(client, p => p.Screen == "session" && p.Connected && p.ArmyHomes?.PurchasedHomes == 2, "two ordinary starting army homes", token);
        CityState City() => Latest(client).Players.Single(p => p.Id == client.PlayerId);
        UnitState Unit(int id) => City().Soldiers.Single(u => u.Id == id);
        async Task SelectUnit(int id)
        {
            await WaitUi(client, p => p.Targets.TryGetValue("Unit" + id, out UiTarget? target) && target.Visible, "current selectable field unit " + id, token);
            await Click(client, "Unit" + id, token);
            UiObservation selected = await WaitUi(client, p => p.InspectedUnit?.Id == id && p.Targets["RetireUnit"].Visible, "owned inspector actions " + id, token);
            UiTarget inspector = selected.Targets["UnitInspector"];
            foreach (string name in new[] { "RetireUnit", "StoreUnit", "StoreDestination" })
            {
                UiTarget control = selected.Targets[name];
                Require(control.Y - control.Height / 2 >= inspector.Y - inspector.Height / 2 && control.Y + control.Height / 2 <= inspector.Y + inspector.Height / 2
                    && control.Y + control.Height / 2 <= selected.HudTop, "army action remains inside visible inspector, not clipped behind HUD: " + name);
            }
        }
        async Task Store(int id)
        {
            UnitState before = Unit(id); await SelectUnit(id); await ClickAck(client, "StoreUnit", token);
            UnitState stored = Unit(id);
            Require(stored.Assignment is { Stored: true } && stored.Health == before.Health && stored.Level == before.Level && stored.Id == before.Id, "actual Store retains identity, level and wounds without immediate healing");
            await WaitUi(client, p => p.InspectedUnit is null && (!p.Targets.TryGetValue("Unit" + id, out UiTarget? target) || !target.Visible), "stored actor is hidden and inspector closes", token);
        }
        async Task Clear(int wave)
        {
            MatchSnapshot result = await Observe(client, s => s.Phase == Phase.Defeat || s.Phase == Phase.Building && s.Wave == wave + 1, "paid army early clear " + wave, token);
            Require(result.Phase == Phase.Building && result.LastRewardedWave == wave, "ordinary six-unit army clears early wave " + wave);
            await WaitUi(client, p => p.Units.All(u => u.Hex?.Lifecycle != UnitLifecycle.Dying), "army retained death cleanup", token);
            CityState city = City();
            Require(city.Soldiers.Where(u => u.Assignment is { Stored: false }).All(u => u.Hex!.Position == u.Assignment!.Position), "all surviving field ids return to their exact physical homes");
        }
        await Pick(client, 0, token); await ClickAck(client, "Farm", token);
        await Pick(client, 1, token); await ClickAck(client, "MetalMine", token);
        await Pick(client, 2, token); await ClickAck(client, "Barracks", token);
        await Pick(client, 3, token); await Click(client, "ProductionChoices", token); await ClickAck(client, "LumbermillRecovery", token);
        // Reuse the paid opening to catch accidental world-label restoration or
        // broad title suppression. Live glyph text plus an existing PNG/pick route;
        // no extra peer, battle or display, and only one small capture/probe.
        await Pick(client, 3, token);
        await Checkpoint(client, "army-ground-labels", token, owner);
        UiObservation labels = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(labels.SelectedSlot == 3 && labels.Placements.Any(p => p.Name == "Slot3" && p.Asset == Game.AssetCatalog.Building(Building.Lumbermill).Path), "unlabelled hut retains its rendered model and actual roof selection");
        Require(!labels.WorldLabels.Contains("Woodcutter hut L1", StringComparer.Ordinal)
            && !Enumerable.Range(1, 2).Any(i => labels.WorldLabels.Contains($"Space {i} · 6 size", StringComparer.Ordinal)), "level-one hut and purchased-space world text are absent");
        Require(labels.WorldLabels.Contains("Bakery L1", StringComparer.Ordinal)
            && Enumerable.Range(3, 4).All(i => labels.WorldLabels.Contains($"Space {i} · locked", StringComparer.Ordinal)), "other building titles and locked-space text are retained");
        Require(labels.ArmyHomes?.PurchasedHomes == 2 && labels.Targets["Upgrade"].Visible && labels.Targets["Sell"].Enabled, "hut actions and army-space capacity remain available");
        await ClickAck(client, "Ready", token); await ClickAck(client, "Ready", token);
        await Pick(client, 4, token); await ClickAck(client, "Stonecutter", token);
        await ClickAck(client, "Ready", token); await UiSwords(client, 2, 6, token, actualInput: false);
        UiObservation opening = await WaitUi(client, p => p.ArmyHomes?.PurchasedHomes == 2 && p.Targets["BuyHome"].Text.Contains("5 gold", StringComparison.Ordinal), "initial physical home quote", token);
        Require(City().Soldiers.Length == 6 && opening.HomeMarkers.Count(n => n.StartsWith("LockedHome", StringComparison.Ordinal)) == 4, "six normally equipped units occupy two homes with four physical locks");
        await Pick(client, 2, token);
        Require(!(await UiProtocol.Probe(client, options.StartupTimeout, token)).Targets["Recruit"].Enabled, "full-home recruitment disables with no equipment payment");
        string full = EconomicCity(City()); await Action(client, "recruit 2", token, accepted: false); Require(EconomicCity(City()) == full, "full recruitment rejection is atomic through the actual session");
        await Checkpoint(client, "army-opening", token, owner);
        await ClickAck(client, "Ready", token); await Clear(1);
        await ClickAck(client, "Ready", token); // Actual production supplies Town hall materials.
        await Pick(client, 5, token); await ClickAck(client, "BuyPlot", token); await Click(client, "ArmyChoices", token); await ClickAck(client, "TownHall", token);
        int[] veterans = City().Soldiers.OrderBy(u => u.Health).ThenBy(u => u.Id).Take(3).Select(u => u.Id).ToArray(); Require(veterans.Length == 3, "paid opening supplies three reserve veterans");
        foreach (int id in veterans) { Require(Unit(id).RecoveryEligible, "completed paid receipt authorizes veteran recovery"); await Store(id); }
        int reserve = veterans[0], wounded = Unit(reserve).Health;
        await SelectUnit(City().Soldiers.First(u => !u.Assignment!.Stored).Id);
        Require(!(await UiProtocol.Probe(client, options.StartupTimeout, token)).Targets["StoreUnit"].Enabled, "full hall disables Store with its capacity reason");
        full = EconomicCity(City()); await Action(client, $"store 5 {City().Soldiers.First(u => !u.Assignment!.Stored).Id}", token, accepted: false); Require(EconomicCity(City()) == full, "full hall rejection loses no id, health or resources");
        await Pick(client, 2, token); await UiSwords(client, 2, 6, token, actualInput: false);
        for (int production = 0; production < 2; production++)
        {
            await ClickAck(client, "Ready", token); await UiSwords(client, 2, 6, token, actualInput: false);
            Require(Unit(reserve).Health == Math.Min(Unit(reserve).Profile.Health, wounded + 200 * (production + 1)), "stored paid veteran heals exactly once per actual 5% production");
        }
        Require(City().Soldiers.Count(u => !u.Assignment!.Stored) == 6, "normally paid replacements fill the two homes");
        await Pick(client, 5, token); await Click(client, "OpenTownHall", token);
        UiObservation hall = await WaitUi(client, p => p.TownHallOpen && p.Targets.ContainsKey("HallUnit" + reserve), "visible six-size Town hall roster", token);
        Require(hall.TownHallText.Contains("6/6 size", StringComparison.Ordinal), "hall shows occupied size slots and stored veteran ids");
        await Click(client, "HallUnit" + reserve, token);
        Require(!(await UiProtocol.Probe(client, options.StartupTimeout, token)).Targets["HallSend"].Enabled, "full battlefield disables hall Send instead of speculative overflow");
        await Checkpoint(client, "army-hall-full", token, owner);
        int health = Unit(reserve).Health; await ClickAck(client, "UpgradeHallHealing", token);
        Require(City().Slots[5] is { HealingLevel: 2, CapacityLevel: 1 } && Unit(reserve).Health == health && City().Army!.PurchasedHomes == 2, "actual healing upgrade is independent and neither heals nor buys field space");
        await Click(client, "CloseTownHall", token); await ClickAck(client, "BuyHome", token);
        UiObservation expanded = await WaitUi(client, p => p.ArmyHomes?.PurchasedHomes == 3 && p.HomeMarkers.Count(n => n.StartsWith("LockedHome", StringComparison.Ordinal)) == 3, "paid physical home removes exactly one lock", token);
        Require(!expanded.WorldLabels.Contains("Space 3 · 6 size", StringComparer.Ordinal)
            && !expanded.WorldLabels.Contains("Space 3 · locked", StringComparer.Ordinal)
            && expanded.WorldLabels.Contains("Space 4 · locked", StringComparer.Ordinal), "purchasing a space clears only its text and lock while retaining other locked labels");
        await Pick(client, 5, token); await Click(client, "OpenTownHall", token); await Click(client, "HallUnit" + reserve, token); await ClickAck(client, "HallSend", token);
        UnitState sent = Unit(reserve);
        Require(sent.Assignment is { Stored: false } && sent.Assignment.Tile == City().Army!.Homes[2].Cell && sent.Health == health, "hall Send first-fits the new third home without changing wounds or identity");
        await Click(client, "CloseTownHall", token);
        int retire = City().Soldiers.Where(u => !u.Assignment!.Stored && u.Id != reserve).OrderBy(u => u.Health).ThenBy(u => u.Id).First().Id;
        ResourceCost stocks = City().Resources; await SelectUnit(retire); await ClickAck(client, "RetireUnit", token);
        Require(City().Resources == stocks && City().Soldiers.All(u => u.Id != retire) && City().Soldiers.Count(u => !u.Assignment!.Stored) == 6, "actual inspector Retire frees room permanently with no refund");
        await Checkpoint(client, "army-expansion-retirement", token, owner);
        await ClickAck(client, "Ready", token); int[] storedIds = City().Soldiers.Where(u => u.Assignment!.Stored).Select(u => u.Id).ToArray();
        BattleUpkeepReceipt receipt = City().LastUpkeep!;
        Require(receipt.Funded.Length == 8 && storedIds.All(id => receipt.Funded.Contains(id) && !receipt.Participating.Contains(id)), "battle pays all eight living ids but stored ids never participate");
        await Clear(2);
        Dictionary<int, int> oldHealth = storedIds.ToDictionary(id => id, id => Unit(id).Health);
        for (int production = 1; production <= 3; production++)
        {
            await ClickAck(client, "Ready", token);
            Require(storedIds.All(id => Unit(id).Health == Math.Min(Unit(id).Profile.Health, oldHealth[id] + 400 * production)), "three actual 10% productions recover only completed paid stored ids");
        }
        await Pick(client, 5, token); await Click(client, "OpenTownHall", token);
        await ClickAck(client, "UpgradeHallCapacity", token);
        Require(City().Slots[5] is { HealingLevel: 2, CapacityLevel: 2 } && City().Army!.PurchasedHomes == 3, "actual storage upgrade leaves recovery level and battlefield capacity unchanged");
        await Checkpoint(client, "army-paid-recovery", token, owner);
        await Click(client, "CloseTownHall", token);
        await client.Send("quit"); Require(await client.WaitExit(token) == 0, "owned army window exits cleanly");
    }
}
