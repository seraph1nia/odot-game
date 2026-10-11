using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    // Explicit graphical smoke: one owned offline application, no server/observer,
    // battles, resizing, campaign preparation or repeated screenshots.
    private async Task UiSmoke(string? executable, CancellationToken token)
    {
        Child client = StartGameRole("ui-smoke", "menu", false, 0, executable);
        await client.WaitFor(e => e.Type == "menu", "menu startup", options.StartupTimeout, token);
        UiObservation menu = await WaitUi(client, p => p.Screen == "menu" && p.MusicPlaying, "menu ready", token);
        await Click(client, "Singleplayer", token);
        await client.WaitFor(e => e.Type == "ack" && e.Result?.Accepted == true && e.State?.Phase == Phase.Building,
            "solo starts through real menu input", options.StartupTimeout, token);
        await Pick(client, 0, token); await ClickAck(client, "Farm", token);
        await Pick(client, 1, token); await ClickAck(client, "Barracks", token);
        await Pick(client, 2, token); await ClickAck(client, "MetalMine", token);
        await ClickAck(client, "Ready", token); // One production, not a battle.
        await Pick(client, 1, token);
        GameEvent recruit = await ClickAck(client, "Recruit", token);
        Require(State(recruit).Players.Single().Soldiers.Length == 1, "real build/recruit controls reach solo authority");
        UiObservation session = await WaitUi(client, p => p.Screen == "session" && p.Connected, "session presentation", token);
        Require(session.MusicInstance == menu.MusicInstance && session.MusicPlaying, "music lifetime survives menu/session transition");
        await MenuCheckpoint(client, "smoke-session", token); // One representative rendered witness.
        await Click(client, "ReturnToMenu", token); await Click(client, "ConfirmReturn", token);
        await WaitUi(client, p => p.Screen == "menu" && !p.Connected, "return disposes session", token);
        await Click(client, "Exit", token);
        Require(await client.WaitExit(token) == 0, "real Exit terminates owned application");
    }
}
