using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    // Explicit graphical smoke: one owned offline application, no server/observer,
    // battles, resizing, campaign preparation or repeated screenshots.
    private async Task UiSmoke(string? executable, CancellationToken token)
    {
        if (Environment.GetEnvironmentVariable("ODOT_ROGUELITE_VERIFY") == "1") { await RogueliteWitness(token); return; }
        Child client = StartGameRole("ui-smoke", "menu", false, 0, executable);
        await client.WaitFor(e => e.Type == "menu", "menu startup", options.StartupTimeout, token);
        UiObservation menu = await WaitUi(client, p => p.Screen == "menu" && p.MusicPlaying, "menu ready", token);
        if (options.DesktopSurface) await WitnessFrame(client, "smoke-menu", token);
        await Click(client, "Singleplayer", token);
        await client.WaitFor(e => e.Type == "connected" && e.State?.Phase == Phase.Lobby,
            "solo lobby opens through real menu input", options.StartupTimeout, token);
        await ClickAck(client, "Start", token);
        await Pick(client, 0, token); await ClickAck(client, "Farm", token);
        await Pick(client, 1, token); await ClickAck(client, "Barracks", token);
        await Pick(client, 2, token); await ClickAck(client, "MetalMine", token);
        await ClickAck(client, "Ready", token); // One production, not a battle.
        await Pick(client, 1, token);
        GameEvent recruit = await ClickAck(client, "Recruit", token);
        Require(State(recruit).Players.Single().Soldiers.Length == 1, "real build/recruit controls reach solo authority");
        UiObservation session = await WaitUi(client, p => p.Screen == "session" && p.Connected, "session presentation", token);
        Require(session.MusicInstance == menu.MusicInstance && session.MusicPlaying, "music lifetime survives menu/session transition");
        // The session is a compact cloud-bounded battlefield, not the menu's
        // full-background countryside. MenuCheckpoint's exterior-terrain check
        // rejects that intentional framing. Keep this smoke scoped to real
        // build/recruit/lifetime plus pixels; EngineCamera owns travel bounds.
        UiObservation frame = await WitnessFrame(client, "smoke-session", token);
        WoodlandPresentation(frame);
        Require(frame.Display == "X11" && frame.Models > 0 && frame.MusicLoaded && frame.MusicPlaying, "solo frame uses bundled scenery and persistent music");
        if (options.DesktopSurface) Require(frame.AudioDriver == "Dummy", "owned GPU smoke uses silent audio");
        else Require(frame.AudioDriver == "Dummy" && (frame.Renderer.Contains("llvmpipe", StringComparison.OrdinalIgnoreCase) || frame.Renderer.Contains("softpipe", StringComparison.OrdinalIgnoreCase)), "solo route uses owned software graphics and silent audio");
        Require(Path.GetFullPath(frame.UserDataPath).StartsWith(Path.Combine(_scope!.Directory, client.Name, "data") + Path.DirectorySeparatorChar, StringComparison.Ordinal), "solo preferences belong to owned client storage");
        await Click(client, "ReturnToMenu", token); await Click(client, "ConfirmReturn", token);
        await WaitUi(client, p => p.Screen == "menu" && !p.Connected, "return disposes session", token);
        await Click(client, "Exit", token);
        Require(await client.WaitExit(token) == 0, "real Exit terminates owned application");
    }
}
