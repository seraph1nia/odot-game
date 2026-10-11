using System.Text.Json;

namespace DevRunner;

internal sealed partial class Runner
{
    private async Task VanillaPopupDiagnostic(Child client, CancellationToken token)
    {
        UiObservation initial = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(HardwareVulkan(initial.Graphics) && initial.ProcessId == client.ProcessId && initial.NativeWindow != 0 && initial.WindowMode == "Windowed" && initial.Graphics.Msaa == "Msaa4X",
            "stripped native controls use the same owned hardware/method/driver/window/High MSAA binding");
        await Click(client, "Settings", token);
        await Click(client, "Graphics_renderer", token);
        await WaitUi(client, p => p.GraphicsDropdownOpen && p.GraphicsPopupFocus["renderer"] == -1, "vanilla pointer-opened popup starts unfocused", token);
        await client.Send("key Down"); await client.Send("key Down");
        await WaitUi(client, p => p.GraphicsPopupFocus["renderer"] == 1 && p.GraphicsDropdownOpen, "vanilla native keyboard item focused", token);
        await client.Send("key Enter");
        UiObservation closed = await WaitUi(client, p => !p.GraphicsDropdownOpen && p.GraphicsDraft["renderer"] == 1 && p.SettingsOpen, "vanilla Enter dismisses without game refresh/state handlers", token);
        await File.WriteAllTextAsync(Path.Combine(_scope!.EvidenceDirectory, "vanilla-popup-closed.json"), JsonSerializer.Serialize(closed, Evidence.JsonOptions), token);
        await Click(client, "CloseVanilla", token);
        await WaitUi(client, p => !p.SettingsOpen, "vanilla dialog closes before displayed Exit", token);
        await Click(client, "Exit", token);
        Require(await client.WaitExit(token) == 0, "stripped native fixture exits through actual displayed control");
        Require(!client.HasEngineErrors, "vanilla popup has no native disconnect errors");
    }
}
