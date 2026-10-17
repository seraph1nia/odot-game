using System.Security.Cryptography;
using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    // Explicit tier E pixels: numerical/engine checks cannot judge texture density,
    // seams or game-camera readability. One owned RX6800 window, fixed seed/light/
    // camera/preset for paired revisions; ordinary menu + paid solo construction.
    // No authority fixtures, extra peers, renderer fallback or CI GPU dependency.
    private async Task TextureWitness(CancellationToken token)
    {
        string manifest = Path.Combine(GameDirectory, "Assets", "Authored", "manifest.json");
        await File.WriteAllTextAsync(Path.Combine(_scope!.EvidenceDirectory, "texture-inputs.json"), JsonSerializer.Serialize(new
        {
            AssetRevision = AuthoredAssets.ReadManifest(Path.GetDirectoryName(manifest)!).Revision,
            ManifestSha256 = Convert.ToHexStringLower(SHA256.HashData(await File.ReadAllBytesAsync(manifest, token))),
            GameAssemblySha256 = Convert.ToHexStringLower(SHA256.HashData(await File.ReadAllBytesAsync(Path.Combine(GameDirectory, ".godot", "mono", "temp", "bin", "Debug", "Game.dll"), token)))
        }, Evidence.JsonOptions), token);
        Child client = StartGameRole("texture-review", "menu", false, 0, extra: ["--combat-seed", "1"]);
        await client.WaitFor(e => e.Type == "menu", "texture review menu", options.StartupTimeout, token);
        await WaitUi(client, p => p.Screen == "menu" && p.Models > 0, "installed landscape ready", token);
        foreach (string view in new[] { "menu", "texture-mixed", "texture-forest", "texture-path", "texture-river" })
            await TextureFrame(client, "menu-" + view, view, token);
        // Same camera, genuine elapsed-time animation; the headless component
        // separately proves actual target transforms, width, banks and loop closure.
        await TextureFrame(client, "menu-river-motion", "texture-river", token);
        await TextureFrame(client, "menu-restored", "menu", token);
        await Click(client, "Singleplayer", token);
        await client.WaitFor(e => e.Type == "connected" && e.State?.Phase == Phase.Lobby, "normal solo lobby", options.StartupTimeout, token);
        await ClickAck(client, "Start", token);
        await Pick(client, 2, token); await ClickAck(client, "ArcheryRange", token);
        await WaitUi(client, p => p.Screen == "session" && p.Connected, "live settlement", token);
        await TextureFrame(client, "settlement", "overview", token);
        await TextureFrame(client, "archery", "texture-archery", token);
        await TextureFrame(client, "settlement-forest", "texture-forest", token);
        await TextureFrame(client, "settlement-river", "texture-river", token);
        await TextureFrame(client, "settlement-river-motion", "texture-river", token);
        // Photo framing changes the camera's diagnostic base. Restore the
        // fitted base explicitly before testing ordinary input, not the user
        // Space helper (which correctly resets to its current base).
        await TextureFrame(client, "settlement-restored", "overview", token);
        await Pick(client, 2, token);
        Require((await UiProtocol.Probe(client, options.StartupTimeout, token)).SelectedSlot == 2, "ordinary building picking survives material import");
        await Click(client, "ReturnToMenu", token); await Click(client, "ConfirmReturn", token);
        await WaitUi(client, p => p.Screen == "menu" && !p.Connected, "normal return", token);
        await Click(client, "Exit", token);
        Require(await client.WaitExit(token) == 0, "texture review owned window exits cleanly");
    }

    private async Task TextureFrame(Child client, string name, string view, CancellationToken token)
    {
        string command = "ui-atmosphere 0 " + view + " 12";
        await client.Send(command);
        await client.WaitFor(e => e.Type == "atmosphere-proof" && e.Message == command, "fixed light/camera texture view", options.StartupTimeout, token);
        UiObservation frame = await WitnessFrame(client, name, token);
        Require(frame.Graphics.CosmeticTime == 12 && frame.Graphics.Settings["weather"] == 0, "paired captures use identical cosmetic time/preset");
        Require(!client.HasEngineErrors, "texture review has no asset/shader/engine errors");
    }
}
