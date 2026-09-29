using System.Text.Json;
using System.Text.Json.Serialization;

namespace Game.Core;

public sealed record GameEvent(string Type, int PeerId = 0, WorldSnapshot? State = null, string? Message = null);

public static class WireJson
{
    public const string EventPrefix = "ODOT_EVENT ";
    public static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
