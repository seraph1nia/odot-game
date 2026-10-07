using System.Text.Json;
using System.Text.Json.Nodes;

namespace Game;

// The captured-control receipt/completion contract is executable without an engine.
internal static partial class NativeBoundaryControls
{
    internal static bool WriteRestorationReceipt(JsonNode current, object fields, IReadOnlyList<object> results, string output)
    {
        JsonNode restored = JsonNode.Parse(JsonSerializer.Serialize(new { InputDigest = current["InputDigest"]!.GetValue<string>(), Frame = current["Frame"]!.GetValue<int>(), Fields = fields }, Core.WireJson.Options))!;
        bool exact = JsonNode.DeepEquals(current, restored);
        File.WriteAllText(output + ".controls.json", JsonSerializer.Serialize(new
        {
            Schema = "owned-native-boundary-controls-v1",
            Results = results,
            ExactObservedRestoration = exact,
            Method = $"same frozen frame{current["Frame"]!.GetValue<int>()}/camera/light; actual material overrides, new visible occluder and shadow-only native caster; every mutation restored before next capture",
            Complete = results.Count == 5 && exact
        }, Core.WireJson.Options));
        return exact;
    }
    internal static bool FinishRestoration(JsonNode current, Func<object> observe, IReadOnlyList<object> results, string output, Exception? firstException, Action<Exception> reportReceiptFailure)
    {
        try { return WriteRestorationReceipt(current, observe(), results, output); }
        catch (Exception e) when (firstException is not null)
        {
            // A failed final observation/receipt must not replace the original
            // control failure already propagating through the caller's finally.
            reportReceiptFailure(e);
            return false;
        }
    }
    internal static void RequireComplete(int count, bool exact)
    {
        if (count != 5 || !exact) throw new InvalidDataException("Native boundary controls incomplete or not exactly restored.");
    }
}
