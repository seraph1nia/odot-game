using System.Text.RegularExpressions;
using Game.Core;

namespace DevRunner;

internal static partial class DiagnosticText
{
    public static string Redact(string line) => EscapedSecretField().Replace(SecretField().Replace(line.Replace("\\\"", "\\u0022", StringComparison.Ordinal), "$1[redacted]$2"), "$1[redacted]$2");
    public static string Compact(GameEvent value)
    {
        if (value.Type == "ui")
        {
            using var document = System.Text.Json.JsonDocument.Parse(value.Message!);
            return "ui id=" + (document.RootElement.TryGetProperty("Id", out var id) ? id.GetString() : "unknown")
                + (document.RootElement.TryGetProperty("Error", out var error) ? " error=" + error.GetString() : "");
        }
        return value.State is { } state
            ? $"{value.Type} peer={value.PeerId} player={value.PlayerId} revision={state.Revision} tick={state.Tick} phase={state.Phase} wave={state.Wave} paused={state.Paused} sequence={value.Result?.Sequence}: {Redact(value.Message ?? "")}" : Redact(System.Text.Json.JsonSerializer.Serialize(value, WireJson.Options));
    }
    [GeneratedRegex("(\"(?:credential|token|sessionKey)\"\\s*:\\s*\")[^\"]*(\")", RegexOptions.IgnoreCase)]
    private static partial Regex SecretField();
    [GeneratedRegex(@"(\\u0022(?:credential|token|sessionKey)\\u0022\s*:\s*\\u0022)[^\\]*(\\u0022)", RegexOptions.IgnoreCase)]
    private static partial Regex EscapedSecretField();
}
