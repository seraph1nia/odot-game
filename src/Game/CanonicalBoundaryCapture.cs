using System.Text.Json.Nodes;

namespace Game;

// Only a validated ordinary comparison may dispatch the bound frame399 producers.
// The runner supplies a producer-free request to the separate warmup execution.
internal static class CanonicalBoundaryCapture
{
    internal static async Task Run(int frame, JsonNode? request, Action compare, Func<Task> glyph, Func<Task> controls)
    {
        compare();
        if (frame != 399) return;
        bool captureGlyph = request?["GlyphCompletion"]?.GetValue<bool>() == true;
        bool runControls = request?["BoundaryControls"]?.GetValue<bool>() == true;
        if (!captureGlyph && !runControls) return;
        if (request?["TargetFrame"]?.GetValue<int>() != frame)
            throw new InvalidDataException("Canonical producers require the independently bound frame399 request.");
        if (captureGlyph) await glyph();
        if (runControls) await controls();
    }
}
