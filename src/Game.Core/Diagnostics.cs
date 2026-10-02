using System.Text.Json;
using System.Text.Json.Serialization;

namespace Game.Core;

public sealed record GameEvent(string Type, int PeerId = 0, MatchSnapshot? State = null, string? Message = null, int PlayerId = 0, CommandResult? Result = null);

public static class WireJson
{
    public const int ProtocolVersion = 6;
    public const string EventPrefix = "ODOT_EVENT ";
    public static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };
}
