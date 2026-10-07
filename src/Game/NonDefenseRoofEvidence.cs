using System.Text.Json.Nodes;

namespace Game;

// A retained, view-specific positive ownership proof, not a pixel ignore list.
// Construction requires matched before/current native stacks AND original-glyph
// raster witnesses. All other pixels still use the exact protected comparison.
public sealed class NonDefenseRoofEvidence
{
    private static readonly string[] GlyphControlKeys = ["Restored", "CameraExact", "ControlsRestored"];
    private readonly JsonNode _before, _after;
    private readonly HashSet<(int X, int Y)> _pixels;
    private readonly int _width, _height;
    private NonDefenseRoofEvidence(JsonNode before, JsonNode after, HashSet<(int, int)> pixels, int width, int height)
    { _before = NormalizeObservation(before); _after = NormalizeObservation(after); _pixels = pixels; _width = width; _height = height; }

    public static string? BoundRequest(JsonNode manifest, int frame, string? view, int zoom)
    {
        bool remaining = manifest["Schema"]?.GetValue<string>() == "bounded-native-roof-bindings-v2";
        if ((!remaining && manifest["Schema"]?.GetValue<string>() != "bounded-native-roof-bindings-v1") || manifest["Bindings"] is not JsonArray bindings || bindings.Count != (remaining ? 4 : 2))
            throw new InvalidDataException("Exactly the independently proved finite roof bindings are required.");
        var seen = new HashSet<int>();
        string? result = null;
        foreach (JsonNode? binding in bindings)
        {
            int key = binding!["Frame"]!.GetValue<int>();
            string expected = key switch { 299 => "settlement-64", 399 => "combat-64", 499 when remaining => "settlement-256", 599 when remaining => "combat-256", _ => throw new InvalidDataException("Unproved future view binding.") };
            if (!seen.Add(key) || binding["View"]?.GetValue<string>() != expected || binding["Zoom"]?.GetValue<int>() != 3 || string.IsNullOrEmpty(binding["Request"]?.GetValue<string>()))
                throw new InvalidDataException("Duplicate or mismatched roof frame/view/zoom binding.");
            if (key == frame)
            {
                if (view != expected || zoom != 3) throw new InvalidDataException("Stale roof view/zoom.");
                result = binding["Request"]!.GetValue<string>();
            }
        }
        return result;
    }
    public static NonDefenseRoofEvidence Create(JsonNode before, JsonNode after, JsonNode retained, JsonNode current,
        JsonNode glyph, JsonNode currentLabels, int width, int height)
    {
        static void Require(bool condition, string reason)
        { if (!condition) throw new InvalidDataException("Non-defense roof evidence refused: " + reason); }
        foreach (string key in new[] { "Frame", "InputDigest" })
        {
            Require(before[key] is not null && new[] { after, retained, current, glyph }.All(n => JsonNode.DeepEquals(before[key], n[key])), key);
        }
        Require(retained["Schema"]?.GetValue<string>() == "six-pixel-native-ownership-v1" && current["Schema"]?.GetValue<string>() == "six-pixel-native-ownership-v1", "native witness schema");
        Require(glyph["Schema"]?.GetValue<string>() == "six-pixel-glyph-coverage-v1" && GlyphControlKeys.All(k => glyph[k]?.GetValue<bool>() == true), "glyph capture/restoration");
        Require(retained["ImmutableInputs"]!.AsObject().Count >= 641 && JsonNode.DeepEquals(retained["ImmutableInputs"], current["ImmutableInputs"]), "source/asset/import provenance");
        foreach (string key in new[] { "NativeCamera", "Near", "Far" }) Require(JsonNode.DeepEquals(retained[key], current[key]), "native projection: " + key);
        Require(JsonNode.DeepEquals(NormalizeNodes(glyph["Labels"]!), NormalizeNodes(currentLabels)), "glyph text/font/alpha/depth/outline/pose changed");
        JsonArray original = retained["Pixels"]!.AsArray(), fresh = current["Pixels"]!.AsArray(), captures = glyph["Captures"]!.AsArray();
        int frame = before["Frame"]!.GetValue<int>();
        Require(frame is 299 or 399 or 499 or 599, "unproved frame");
        bool remaining = frame is 499 or 599;
        int labelCount = glyph["Labels"]!.AsArray().Count;
        bool noGlyph = remaining && labelCount == 0;
        if (remaining)
        {
            Require(glyph["RoleIdentities"] is JsonArray roles && roles.Count == labelCount
                && roles.Select(r => r!.GetValue<string>()).Distinct(StringComparer.Ordinal).Count() == labelCount, "complete unique current logical glyph identities");
            for (int label = 0; label < labelCount; label++)
                Require(glyph["Labels"]![label]!["Identity"]?["Matched"]?.GetValue<bool>() == true
                    && JsonNode.DeepEquals(glyph["RoleIdentities"]![label], glyph["Labels"]![label]!["Identity"]!["Key"]), "unresolved current logical glyph/authoring identity");
            Require(retained["GlyphInventory"] is JsonArray && JsonNode.DeepEquals(NormalizeNodes(retained["GlyphInventory"]!), NormalizeNodes(current["GlyphInventory"]!)), "complete current glyph bounds inventory");
            if (noGlyph) Require(glyph["NoPossibleGlyphs"]?.GetValue<bool>() == true && captures.Count == 0
                && current["GlyphInventory"]!.AsArray().All(g => g!["PossiblePixels"]!.AsArray().Count == 0), "no possible glyphs; no fictional capture");
        }
        bool mineOnly = frame == 399;
        Require(!mineOnly || glyph["MineTitleIdentity"]?.GetValue<string>() == "Slot2/mine-title" && glyph["Labels"]!.AsArray().Count == 1, "unique current native mine title");
        Require(original.Count > 0 && original.Count == fresh.Count && captures.Count == (remaining ? noGlyph ? 0 : labelCount + 1 : mineOnly ? 2 : 4), "complete native/glyph sample sets");
        Require(!mineOnly || captures[1]!["Name"]?.GetValue<string>() == "label0", "genuine unique original label capture");
        Require(noGlyph || captures[0]!["Name"]!.GetValue<string>() == "empty" && captures[0]!["AlphaPixels"]!.GetValue<int>() == 0, "transparent empty control");
        if (remaining) for (int label = 0; label < labelCount; label++) Require(captures[label + 1]!["Name"]?.GetValue<string>() == "label" + label, "every actual original label captured");
        if (remaining)
        {
            string[] candidates = fresh.SelectMany(p => p!["Unknown"]!.AsArray()).Select(u => u!["Node"]!.GetValue<string>()).Distinct(StringComparer.Ordinal).Select(NormalizeNode).Order(StringComparer.Ordinal).ToArray();
            string[] recorded = glyph["Labels"]!.AsArray().Select(l => NormalizeNode(l!["Node"]!.GetValue<string>())).Order(StringComparer.Ordinal).ToArray();
            Require(candidates.SequenceEqual(recorded), "every possible glyph accounted across all changed points");
        }
        Require(captures.Skip(1).All(c => c!["AlphaPixels"]!.GetValue<int>() > 0), "genuine glyphs must demonstrably paint elsewhere");
        var pixels = new HashSet<(int, int)>();
        for (int i = 0; i < original.Count; i++)
        {
            JsonNode a = original[i]!, b = fresh[i]!;
            int x = a["Pixel"]![0]!.GetValue<int>(), y = a["Pixel"]![1]!.GetValue<int>();
            Require(x >= 0 && x < width && y >= 0 && y < height && pixels.Add((x, y)), "sample bounds/uniqueness");
            foreach (string key in new[] { "Pixel", "RayOrigin", "RayDirection", "BeforeCandidates", "AfterCandidates", "Unknown" })
                Require(JsonNode.DeepEquals(NormalizeNodes(a[key]!), NormalizeNodes(b[key]!)), "current coverage/depth/buffers/contributors: " + key);
            JsonArray fronts = b["AfterCandidates"]!.AsArray(), old = a["BeforeCandidates"]!.AsArray();
            Require(fronts.Count >= 2 && old.Count >= 2, "opaque roof contributors missing");
            // No chosen depth quantum or winner: every retained possible front
            // contributor must belong to the identical preexisting family.
            for (int j = 0; j < 2; j++)
            {
                JsonNode hit = fronts[j]!;
                Require(JsonNode.DeepEquals(NormalizeNodes(hit), NormalizeNodes(old[j]!)), "before protected -> after occluder or different family");
                Require(hit["Asset"]?.GetValue<string>() == "buildings/metal_mine.glb" && hit["Owner"]?.GetValue<string>() == "Slot2"
                    && hit["NewDecoration"]?.GetValue<bool>() == false && hit["OpaqueUnmodified"]?.GetValue<bool>() == true
                    && hit["Priority"]?.GetValue<int>() == 0 && hit["Node"]!.GetValue<string>().Contains("/Mine office/roof_tile_instance_", StringComparison.Ordinal)
                    && (hit["Material"]!.GetValue<string>() is "roof_blue" or "roof_blue_light" || remaining && hit["Material"]!.GetValue<string>() == "roof_blue_dark") && hit["PixelEdgeMargin"]!.GetValue<float>() > 0, "not the attested non-defense roof family");
            }
            // Even a possible cross-family contributor immediately behind the
            // leading tile pair refuses; no ambiguity is rounded away.
            Require(fronts.Take(3).All(h => h!["Asset"]!.GetValue<string>() == "buildings/metal_mine.glb" && h["Owner"]!.GetValue<string>() == "Slot2" && !h["NewDecoration"]!.GetValue<bool>()), "cross-family depth ambiguity");
            foreach (JsonNode? capture in captures)
            {
                Require(capture!["Width"]!.GetValue<int>() == width && capture["Height"]!.GetValue<int>() == height, "glyph resolution");
                JsonNode sample = capture["Samples"]![i]!;
                Require(JsonNode.DeepEquals(sample["Pixel"], a["Pixel"]) && sample["Rgba"]!.AsArray().All(v => v!.GetValue<int>() == 0)
                    && sample["NeighborhoodMaximumAlpha"]!.GetValue<int>() == 0 && sample["ChangedFromEmpty"]?.GetValue<bool>() == false, "glyph/outline paints sample");
            }
            string[] unknown = b["Unknown"]!.AsArray().Select(u => NormalizeNode(u!["Node"]!.GetValue<string>())).ToArray();
            string[] labels = glyph["Labels"]!.AsArray().Select(l => NormalizeNode(l!["Node"]!.GetValue<string>())).ToArray();
            Require(remaining ? unknown.All(u => labels.Contains(u, StringComparer.Ordinal))
                : unknown.Length == labels.Length && unknown.Order(StringComparer.Ordinal).SequenceEqual(labels.Order(StringComparer.Ordinal)), "unresolved non-glyph contributor");
        }
        return new(before, after, pixels, width, height);
    }
    internal bool Allows(JsonNode before, JsonNode after, int width, int height, int x, int y) => width == _width && height == _height
        && _pixels.Contains((x, y)) && JsonNode.DeepEquals(_before, NormalizeObservation(before)) && JsonNode.DeepEquals(_after, NormalizeObservation(after));

    private static JsonNode NormalizeObservation(JsonNode input)
    {
        JsonNode copy = input.DeepClone();
        if (copy["Fields"]?["Placements"] is JsonArray placements)
            foreach (JsonNode? p in placements)
                if (p?["Name"]?.GetValue<string>().StartsWith("@Node3D@", StringComparison.Ordinal) == true) p.AsObject().Remove("Name");
        return copy;
    }
    private static string NormalizeNode(string path)
    {
        int slash = path.IndexOf('/');
        string value = slash < 0 ? path : path[(slash + 1)..];
        return value.Split('/').Select(p => p.StartsWith("@Label3D@", StringComparison.Ordinal) ? "@Label3D@" : p).Aggregate((a, b) => a + "/" + b);
    }
    private static JsonNode NormalizeNodes(JsonNode input)
    {
        JsonNode copy = input.DeepClone();
        void Visit(JsonNode node)
        {
            if (node is JsonObject value)
            {
                if (value["Node"] is JsonValue path) value["Node"] = NormalizeNode(path.GetValue<string>());
                foreach (JsonNode? child in value.Select(p => p.Value).ToArray()) if (child is not null) Visit(child);
            }
            else if (node is JsonArray array) foreach (JsonNode? child in array) if (child is not null) Visit(child);
        }
        Visit(copy); return copy;
    }
}
