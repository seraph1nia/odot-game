namespace DevRunner;

internal sealed partial class Runner
{
    private async Task AssetFidelity()
    {
        await _evidence.Measure("authored-static-fidelity", "asset", async () =>
        {
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            deadline.CancelAfter(TimeSpan.FromSeconds(60));
            await using var owned = new ScenarioScope("authored-static-fidelity", _evidence);
            var worker = new Runner(options, deadline.Token, _evidence, owned, _root);
            Child child = worker.StartGameRole("asset-probe", "menu", true, 0, extra: ["--asset-fidelity-probe"]);
            Game.Core.GameEvent result = await child.WaitFor(e => e.Type == "asset-fidelity", "actual static rendering-buffer fidelity and corruption regressions", 60000, deadline.Token);
            if (await child.WaitExit(deadline.Token) != 0 || child.HasEngineErrors || string.IsNullOrEmpty(result.Message))
                throw new InvalidOperationException("Authored fidelity process failed.\n" + child.Tail());
            await File.WriteAllTextAsync(Path.Combine(owned.EvidenceDirectory, "rendering-buffer-fidelity.json"), result.Message, deadline.Token);
            await owned.DisposeAsync(); owned.CheckErrors();
            Console.WriteLine("PASS: actual authored rendering vertices, transforms, triangle/material assignments, normals/UVs and corruption regressions");
        });
    }
}
