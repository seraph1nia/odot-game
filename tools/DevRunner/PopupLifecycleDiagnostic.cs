using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    private async Task PopupLifecycleDiagnostic(Child client, CancellationToken token)
    {
        Console.WriteLine("Bounded popup-only diagnostic: same Windowed hardware/High/render controls and original 15s/180s limits; no eight-view/export acceptance claim.");
        await Click(client, "Singleplayer", token);
        await Observe(client, state => state.Phase == Phase.Building, "ordinary owned solo popup context", token);
        await AtmosphereFrame(client, "popup-high-village", 0, "overview", token);
        await Click(client, "Settings", token);
        UiObservation before = await UiProtocol.Probe(client, options.StartupTimeout, token);
        await File.WriteAllTextAsync(Path.Combine(_scope!.EvidenceDirectory, "popup-before.json"), JsonSerializer.Serialize(before, Evidence.JsonOptions), token);
        await GraphicsOptionInput(client, "renderer", 1, token);
        // Reopen and cancel with actual Escape: native dropdown dismissal must
        // not close settings, alter the selected draft or leak to live play.
        await Click(client, "Graphics_renderer", token);
        await WaitUi(client, p => p.GraphicsDropdownOpen, "native graphics dropdown really reopens", token);
        await client.Send("key Escape");
        await WaitUi(client, p => p.SettingsOpen && !p.GraphicsDropdownOpen && p.GraphicsDraft["renderer"] == 1,
            "Escape closes only the reopened popup and retains its selected draft", token);
        await GraphicsOptionInput(client, "renderer", 1, token);
        UiObservation closed = await UiProtocol.Probe(client, options.StartupTimeout, token);
        await File.WriteAllTextAsync(Path.Combine(_scope.EvidenceDirectory, "popup-closed.json"), JsonSerializer.Serialize(closed, Evidence.JsonOptions), token);
        Require(closed.Graphics.Method == "forward_plus" && closed.GraphicsDraft["renderer"] == 1 && closed.SettingsOpen && !closed.GraphicsDropdownOpen,
            "real native keyboard selection closes its popup, changes only the draft and preserves the live window/session");
        await Click(client, "ApplyGraphics", token);
        await Click(client, "ReturnToMenu", token); await Click(client, "ConfirmReturn", token);
        await WaitUi(client, p => p.Screen == "menu" && !p.Connected, "actual popup diagnostic safe return", token);
        await Click(client, "Exit", token);
        Require(await client.WaitExit(token) == 0, "actual popup diagnostic displayed Exit");
        Require(!client.HasEngineErrors, "popup open/select/dismiss/return/exit has no engine errors");
    }
}
