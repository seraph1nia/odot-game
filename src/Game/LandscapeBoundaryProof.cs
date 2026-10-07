using System.Text.Json.Nodes;

namespace Game;

// A generated observation/RGBA contract, not a source-text test or authority.
// Used only by the owned, fixed-input presentation diagnostic.
public static class LandscapeBoundaryProof
{
    public static int Verify(JsonNode before, JsonNode after, int width, int height, ReadOnlySpan<byte> beforePixels, ReadOnlySpan<byte> afterPixels, NonDefenseRoofEvidence? roofEvidence = null)
    {
        foreach (string key in new[] { "InputDigest", "Frame" })
            if (!JsonNode.DeepEquals(before[key], after[key])) throw new InvalidDataException("Boundary input/frame changed: " + key);
        JsonNode a = before["Fields"]!, b = after["Fields"]!;
        foreach (string key in new[] { "Camera", "Units", "Strikes", "PlotHeights", "BuildingVariants", "HomeHealth", "CombatTick", "VisualSeconds", "MatchId", "MatchPhase", "Wave", "TurnSerial" })
            if (a[key] is null || b[key] is null || !JsonNode.DeepEquals(a[key], b[key])) throw new InvalidDataException("Protected presentation changed or missing: " + key);
        static JsonArray Placements(JsonNode fields)
        {
            if (fields["Placements"] is not JsonArray placements) throw new InvalidDataException("Missing installed placements.");
            return new(placements.Select(p =>
            {
                JsonObject value = p!.DeepClone().AsObject();
                // Auto-generated native node counters are not gameplay/slot ids;
                // adding passive siblings changes them without moving a crate.
                if (value["Name"]?.GetValue<string>().StartsWith("@Node3D@", StringComparison.Ordinal) == true) value.Remove("Name");
                return (JsonNode)value;
            }).ToArray());
        }
        if (!JsonNode.DeepEquals(Placements(a), Placements(b))) throw new InvalidDataException("Protected installed placements changed.");
        foreach (string key in new[] { "CoreTerrain", "River", "Bridge", "Plots" })
            if (!JsonNode.DeepEquals(a["Landscape"]![key], b["Landscape"]![key])) throw new InvalidDataException("Protected terrain changed: " + key);
        static JsonArray BattleProps(JsonNode fields) => new(fields["Landscape"]!["Static"]!.AsArray()
            .Where(p => p!["Z"]!.GetValue<float>() <= 4.34f).Select(p =>
            {
                JsonObject value = p!.DeepClone().AsObject();
                if (value["Pocket"] is not null && value["Pocket"]!.GetValue<string>().Length > 0) throw new InvalidDataException("New scenery entered the battle boundary.");
                value.Remove("Pocket"); value.Remove("ProjectedBounds"); return (JsonNode)value;
            }).ToArray());
        if (!JsonNode.DeepEquals(BattleProps(a), BattleProps(b))) throw new InvalidDataException("Battle scenery changed.");
        if (width <= 0 || height <= 0 || beforePixels.Length != checked(width * height * 4) || afterPixels.Length != beforePixels.Length)
            throw new InvalidDataException("Boundary needs matching complete RGBA frames.");
        var camera = new ProtectedProjection(b["Camera"]!, height);
        int protectedPixels = 0;
        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
        {
            if (!camera.Contains(x, y)) continue;
            protectedPixels++;
            int offset = (y * width + x) * 4;
            if (!beforePixels.Slice(offset, 4).SequenceEqual(afterPixels.Slice(offset, 4)) && roofEvidence?.Allows(before, after, width, height, x, y) != true)
                throw new InvalidDataException($"Battle pixel changed at ({x},{y}); non-battle differences are not a global waiver.");
        }
        if (protectedPixels < width * height / 100) throw new InvalidDataException("Battle comparison has no meaningful protected region.");
        return protectedPixels;
    }
    public static int[][] ChangedProtectedPixels(JsonNode before, JsonNode after, int width, int height, ReadOnlySpan<byte> beforePixels, ReadOnlySpan<byte> afterPixels)
    {
        // Validate the same protected physical/state contract before discovering
        // actual changed pixels. This grants no pixel/family exemption.
        _ = Verify(before, after, width, height, beforePixels, beforePixels);
        if (afterPixels.Length != beforePixels.Length) throw new InvalidDataException("Mismatched native RGBA frame.");
        var changed = new List<int[]>();
        var camera = new ProtectedProjection(after["Fields"]!["Camera"]!, height);
        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
            if (camera.Contains(x, y) && !beforePixels.Slice((y * width + x) * 4, 4).SequenceEqual(afterPixels.Slice((y * width + x) * 4, 4)))
                changed.Add([x, y]);
        return changed.ToArray();
    }
    public static bool ProtectedPixel(JsonNode camera, int height, int x, int y) => new ProtectedProjection(camera, height).Contains(x, y);

    // Cache parsed floats/trigonometry, NOT a rounded float size/height scale.
    // The pixel arithmetic keeps the original double multiply/divide order.
    internal readonly struct ProtectedProjection
    {
        private readonly float size, referenceX, referenceY;
        private readonly int height;
        private readonly double cosYaw, sinYaw, sinPitch;
        internal ProtectedProjection(JsonNode camera, int height)
        {
            size = camera["Size"]!.GetValue<float>();
            float pitch = -camera["Rotation"]![0]!.GetValue<float>(), yaw = camera["Rotation"]![1]!.GetValue<float>();
            if (!float.IsFinite(size) || size <= 0 || pitch is <= 0 or >= 1.5f) throw new InvalidDataException("Invalid controlled camera.");
            referenceX = camera["ReferenceX"]!.GetValue<float>(); referenceY = camera["ReferenceY"]!.GetValue<float>();
            this.height = height;
            cosYaw = Math.Cos(yaw); sinYaw = Math.Sin(yaw); sinPitch = Math.Sin(pitch);
        }
        internal bool Contains(int x, int y)
        {
            double right = (x + .5 - referenceX) * size / height;
            double up = -(y + .5 - referenceY) * size / height;
            // Unchanged city-local ground reference (2,0,-3), with rear depth
            // for projected heads and all old battle scenery, not just feet.
            double groundX = 2 + cosYaw * right - sinYaw * up / sinPitch;
            double groundZ = -3 - sinYaw * right - cosYaw * up / sinPitch;
            return groundX is >= -14 and <= 14 && groundZ is >= -22 and <= 4.34;
        }
    }
}
