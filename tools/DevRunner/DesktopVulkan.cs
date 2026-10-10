using System.Runtime.InteropServices;

namespace DevRunner;

internal sealed partial class Runner
{
    private static void CheckDesktopVulkanPrerequisites()
    {
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64 || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DISPLAY")))
            throw new VerificationPrerequisiteException("Explicit windowed desktop Vulkan unexecuted: requires authorized Linux x86_64 X11 surface. No automatic desktop fallback, installation or compositor changes.");
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
            await worker.AtmosphereScenario(deadline.Token);
            await owned.QuitGames(deadline.Token);
            await owned.DisposeAsync(); owned.CheckErrors();
        });
    }
}
