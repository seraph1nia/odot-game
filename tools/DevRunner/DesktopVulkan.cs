using System.Runtime.InteropServices;

namespace DevRunner;

internal sealed partial class Runner
{
    private static void CheckDesktopVulkanPrerequisites()
    {
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64 || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DISPLAY")))
            throw new VerificationPrerequisiteException("Explicit windowed desktop Vulkan unexecuted: requires authorized Linux x86_64 X11 surface. No automatic desktop fallback, installation or compositor changes.");
    }
    private static void RequireDesktopGpu(Child client, UiObservation frame)
    {
        Require(HardwareVulkan(frame.Graphics) && frame.Graphics.Gpu == "AMD Radeon RX 6800 (RADV NAVI21)",
            "authorized RX 6800/RADV Forward+/Vulkan RenderingDevice; no software or backend fallback");
        Require(frame.ProcessId == client.ProcessId && frame.NativeWindow != 0 && frame.WindowMode == "Windowed",
            "authenticated owned PID and native X11 window in Windowed mode");
    }
    private async Task DesktopVulkan()
    {
        Require(options.DesktopSurface && options.Command == "test-vulkan" && options.Renderer == "forward-plus", "explicit authorized windowed desktop native slice, never mandatory CI fallback");
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        deadline.CancelAfter(options.Timeout);
        await _evidence.Measure("desktop-vulkan", "ui", async () =>
        {
            await using var owned = new ScenarioScope("desktop-vulkan", _evidence, graphical: true);
            var worker = new Runner(options, deadline.Token, _evidence, owned);
            Console.WriteLine("Explicit windowed desktop Vulkan: isolated test XDG/state, owned game-only viewport captures. No desktop/window-manager/system mutations or unrelated captures/process control.");
            if (options.UiCheckpoint == "smoke") await worker.UiSmoke(null, deadline.Token);
            else await worker.AtmosphereScenario(deadline.Token);
            await owned.QuitGames(deadline.Token);
            await owned.DisposeAsync(); owned.CheckErrors();
        });
    }
}
