using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    // Source slices own animation breadth and application routes. This short slice
    // establishes that an exported/installed archive actually loads and plays.
    private async Task PackedUiScenario(string executable, CancellationToken token)
    {
        Child client = StartGameRole("ui-client", "menu", false, 0, executable, "--combat-seed", "1");
        await client.WaitFor(e => e.Type == "menu", "packed menu launch", options.StartupTimeout, token);
        UiObservation menu = await WaitUi(client, p => p.Screen == "menu" && p.MusicPlaying && p.Models > 0, "packed menu assets ready", token);
        Require(!menu.Connected && menu.Revision == -1, "packed menu starts without a match");
        await MenuCheckpoint(client, "packed-menu", token);
        await Click(client, "Singleplayer", token);
        await client.WaitFor(e => e.Type == "ack" && e.Result?.Accepted == true && e.State?.Phase == Phase.Building, "packed solo starts through actual input", options.StartupTimeout, token);
        await WaitUi(client, p => p.Screen == "session" && p.Connected, "packed solo presentation", token);
        await Pick(client, 0, token);
        GameEvent purchase = await ClickAck(client, "Farm", token);
        Require(State(purchase).Players.Single().Slots[0].Type == Building.Farm, "packed ordinary purchase reaches local authority");
        await Action(client, "build 1 barracks", token);
        await Action(client, "build 2 metalmine", token);
        for (int turn = 0; turn < 3; turn++) await Action(client, "ready", token);
        for (int recruit = 0; recruit < 6; recruit++) await Action(client, "recruit 1", token);
        await SimulationSpeed(client, 1, token);
        await Action(client, "ready", token);
        UiObservation live = await WaitUi(client, p => p.Units.Any(u => u.Visible && u.Type == UnitType.Swordsman && u.Clip == "Walking_A"), "packed live representative animation", token);
        Require(live.UnitBindings.Length == 18 && live.Units.Any(u => u.WeaponAttached), "packed faction rigs, required clips and weapon bindings load from archive");
        await Action(client, "pause", token);
        await WaitUi(client, p => p.Units.Length > 0, "packed paused layout checkpoint", token);
        await MenuCheckpoint(client, "packed-animation", token);
        await GeometryAtBothSizes(client, "packed-layout", token);
        await Click(client, "Settings", token);
        await Click(client, "ReturnToMenu", token);
        await Click(client, "ConfirmReturn", token);
        await WaitUi(client, p => p.Screen == "menu" && !p.Connected, "packed owned session disposal", token);
        await Click(client, "Exit", token);
        Require(await client.WaitExit(token) == 0, "packed displayed Exit terminates owned process");
    }
}
