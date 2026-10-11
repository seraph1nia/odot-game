namespace DevRunner;

internal sealed partial class Runner
{
    private async Task UiResources()
    {
        await _evidence.Measure("ui-resources", "component", async () =>
        {
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            deadline.CancelAfter(TimeSpan.FromSeconds(60));
            await using var owned = new ScenarioScope("ui-resources", _evidence);
            var environment = owned.EnvironmentFor("component");
            environment["ODOT_STEAM_DISABLED"] = "1";
            environment["DISPLAY"] = null; environment["WAYLAND_DISPLAY"] = null;
            await using var child = owned.Own(new Child("ui-resources", "godot",
                ["--headless", "--audio-driver", "Dummy", "--path", GameDirectory, "--resolution", "1100x820", "--script", "res://Verification/ui_resources.gd"],
                _root, environment: environment, evidenceDirectory: owned.EvidenceDirectory));
            int code = await child.WaitExit(deadline.Token);
            if (code != 0 || child.HasEngineErrors || !child.Tail().Contains("UI_RESOURCES_OK", StringComparison.Ordinal))
                throw new InvalidOperationException("Headless UI components/settings fixture failed.\n" + child.Tail());
            await owned.DisposeAsync(); owned.CheckErrors();
            Console.WriteLine("PASS: focused headless production resource/modal/input/intent/settings fixture; no full game or graphical acceptance");
        });
    }
}
