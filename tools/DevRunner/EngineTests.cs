namespace DevRunner;

internal sealed partial class Runner
{
    private static readonly string[] RendererStartupScenarios = ["renderer-forward", "renderer-compatibility"];
    private async Task EngineTests(bool exported = false)
    {
        int timeout = options.ExplicitTimeout ? options.Timeout : 60000;
        string[] selected = exported ? ["worldtree"] : options.Scenario is null ? ["settings", "animation", "assets", "worldtree", "receiving", "arena"] : [options.Scenario];
        foreach (string scenario in selected)
            await _evidence.Measure("engine-" + scenario, "engine", async () =>
            {
                using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
                deadline.CancelAfter(timeout);
                await using var owned = new ScenarioScope("engine-" + scenario, _evidence);
                var environment = owned.EnvironmentFor("component");
                string marker = Path.Combine(environment["ODOT_OWNED_DATA"]!, ".verification-owner");
                string token = Guid.NewGuid().ToString("N");
                File.WriteAllText(marker, token);
                environment["ODOT_VERIFICATION_MARKER"] = marker;
                environment["ODOT_VERIFICATION_TOKEN"] = token;
                environment["ODOT_STEAM_DISABLED"] = "1";
                string executable = exported ? AtmosphereClientPackage() : "godot";
                string[] arguments = exported
                    ? ["--headless", "--audio-driver", "Dummy", "--", "--supervised", "--engine-component-probe", scenario]
                    : ["--headless", "--audio-driver", "Dummy", "--path", GameDirectory, "res://Verification/EngineTests.tscn", "--", "--scenario", scenario];
                await using var child = owned.Own(new Child("engine-" + scenario, executable, arguments,
                    _root, game: true, environment: environment, evidenceDirectory: owned.EvidenceDirectory,
                    engineLogPath: Path.Combine(owned.EvidenceDirectory, "engine.log")));
                await child.WaitFor(e => e.Type == "engine-ready", scenario + " component readiness", options.StartupTimeout, deadline.Token);
                await child.WaitFor(e => e.Type == (scenario == "assets" ? "asset-fidelity" : "engine-test"),
                    scenario + " production component checks", timeout, deadline.Token);
                int code = await child.WaitExit(deadline.Token);
                deadline.Token.ThrowIfCancellationRequested();
                if (code != 0 || child.HasEngineErrors)
                    throw new InvalidOperationException("Engine component failed.\n" + child.Tail());
                if (!exported && scenario == "settings")
                {
                    // Intentional restart of this same minimal component client,
                    // reusing only its owned preferences. No application or GPU.
                    foreach (string startup in RendererStartupScenarios)
                    {
                        string[] restartArguments = [.. arguments[..^1], startup];
                        await using var restarted = owned.Own(new Child(startup, executable, restartArguments,
                            _root, game: true, environment: environment, evidenceDirectory: owned.EvidenceDirectory,
                            engineLogPath: Path.Combine(owned.EvidenceDirectory, startup + ".log")));
                        await restarted.WaitFor(e => e.Type == "engine-ready", startup + " readiness", options.StartupTimeout, deadline.Token);
                        await restarted.WaitFor(e => e.Type == "engine-test", startup + " native project override", timeout, deadline.Token);
                        int restartCode = await restarted.WaitExit(deadline.Token);
                        if (restartCode != 0 || restarted.HasEngineErrors) throw new InvalidOperationException("Renderer startup component failed.\n" + restarted.Tail());
                    }
                }
                await owned.DisposeAsync(); owned.CheckErrors();
            });
        Console.WriteLine("Engine coverage: " + string.Join(", ", selected) + "; headless components, no session or graphical proof.");
    }
}
