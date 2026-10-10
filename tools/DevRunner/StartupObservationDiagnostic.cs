using System.Diagnostics;
using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    // One boundary trace. A late response remains diagnostic failure, never a
    // relaxation of the original startup limit or an eight-view/native pass.
    private async Task StartupObservationDiagnostic(Child client, CancellationToken token)
    {
        string id = Guid.NewGuid().ToString("N");
        Console.WriteLine($"STARTUP DIAGNOSTIC request={id} pid={client.ProcessId}; originalLimit={options.StartupTimeout}ms");
        var watch = Stopwatch.StartNew();
        await client.Send("ui-probe " + id);
        GameEvent response;
        bool late = false;
        try
        {
            response = await client.WaitFor(e => e.Type == "ui" && e.Message!.Contains(id, StringComparison.Ordinal), "original startup observation", options.StartupTimeout, token);
        }
        catch (TimeoutException error)
        {
            late = true;
            await File.WriteAllTextAsync(Path.Combine(_scope!.EvidenceDirectory, "original-startup-failure.txt"), error.ToString(), token);
            Console.WriteLine("STARTUP DIAGNOSTIC ORIGINAL LIMIT FAILED; separately labeled 20s late boundary wait, not acceptance");
            response = await client.WaitFor(e => e.Type == "ui" && e.Message!.Contains(id, StringComparison.Ordinal), "late diagnostic response to the SAME request", 20000, token);
        }
        await File.WriteAllTextAsync(Path.Combine(_scope!.EvidenceDirectory, "startup-response.json"), response.Message, token);
        UiObservation frame = JsonSerializer.Deserialize<UiObservation>(response.Message!, WireJson.Options)!;
        Console.WriteLine($"STARTUP DIAGNOSTIC response elapsed={watch.Elapsed.TotalMilliseconds:F2}ms; late={late}; id={frame.Id}; screen={frame.Screen}; models={frame.Models}; error={frame.Error}; method={frame.Graphics.Method}/{frame.Graphics.Driver}; device={frame.Graphics.DeviceType}; nativeWindow={frame.NativeWindow}; owner={frame.ProcessId}");
        if (frame.Error is not null) throw new InvalidOperationException("Diagnostic observation error: " + frame.Error);
        string path = Path.Combine(_scope.EvidenceDirectory, "startup-game-only.png");
        UiObservation pixels = await UiProtocol.Probe(client, options.StartupTimeout, token, path);
        UiProtocol.Frame(pixels, path);
        await File.WriteAllTextAsync(Path.ChangeExtension(path, ".json"), JsonSerializer.Serialize(pixels, Evidence.JsonOptions), token);
        await client.Send("quit");
        Require(await client.WaitExit(token) == 0, "diagnostic owned game exits");
        if (late || frame.Models == 0 || frame.Screen != "menu") throw new InvalidOperationException("Original startup acceptance failed; late/partial diagnostic is not a pass.");
        Require(!client.HasEngineErrors, "diagnostic reports no native engine errors");
    }
}
