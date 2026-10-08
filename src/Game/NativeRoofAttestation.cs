using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Godot;

namespace Game;

// Admission for this retained investigation only. Paths/hash receipts stay in
// owned evidence, never in production data or a general object-id service.
internal static class NativeRoofAttestation
{
    internal static NonDefenseRoofEvidence Build(Tabletop scene, string requestPath, string observation, Image image, string output)
    {
        JsonNode request = JsonNode.Parse(File.ReadAllText(requestPath))!;
        foreach (var entry in request["EvidenceHashes"]!.AsObject())
            if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(entry.Key))) != entry.Value!.GetValue<string>())
                throw new InvalidDataException("Stale retained roof/glyph evidence: " + entry.Key);
        PixelOwnershipDiagnostic.Inspect(scene, requestPath, observation, image, output + ".current-native.json");
        JsonNode current = JsonNode.Parse(File.ReadAllText(output + ".current-native.json"))!;
        JsonNode retained = JsonNode.Parse(File.ReadAllText(request["NativeWitness"]!.GetValue<string>()))!;
        JsonNode glyph = JsonNode.Parse(File.ReadAllText(request["GlyphWitness"]!.GetValue<string>()))!;
        Camera3D camera = scene.GetViewport().GetCamera3D() ?? throw new InvalidDataException("Missing attestation camera.");
        string[] names = current["Pixels"]!.AsArray().SelectMany(p => p!["Unknown"]!.AsArray()).Select(u => u!["Node"]!.GetValue<string>()).Distinct(StringComparer.Ordinal).ToArray();
        int frame = current["Frame"]!.GetValue<int>();
        object[] labels = names.Select(n =>
        {
            Label3D label = scene.GetNodeOrNull<Label3D>(n) ?? throw new InvalidDataException("Unresolved current glyph: " + n);
            return frame is 499 or 599 ? PixelGlyphCompletion.DescribeMapped(scene, camera, label) : PixelGlyphCompletion.Describe(scene, camera, label);
        }).ToArray();
        if (current["Frame"]!.GetValue<int>() == 399)
            PixelGlyphCompletion.RequireMineTitle(names.Select(n => scene.GetNode<Label3D>(n)).ToArray());
        JsonNode currentLabels = JsonNode.Parse(JsonSerializer.Serialize(labels, Game.Core.WireJson.Options))!;
        var proof = NonDefenseRoofEvidence.Create(JsonNode.Parse(File.ReadAllText(request["BeforeObservation"]!.GetValue<string>()))!, JsonNode.Parse(observation)!,
            retained, current, glyph, currentLabels, image.GetWidth(), image.GetHeight());
        File.WriteAllText(output + ".attestation.json", JsonSerializer.Serialize(new
        {
            Schema = "same-preexisting-nondefense-roof-family-v1",
            Frame = current["Frame"],
            InputDigest = current["InputDigest"],
            Width = image.GetWidth(),
            Height = image.GetHeight(),
            EvidenceHashes = request["EvidenceHashes"],
            CurrentLabels = currentLabels,
            CurrentNative = output + ".current-native.json",
            Result = "matched immutable source/native buffer/pose/projection/depth family with no original glyph coverage; no exact winning tile or D24 claim"
        }, Game.Core.WireJson.Options));
        return proof;
    }
}
