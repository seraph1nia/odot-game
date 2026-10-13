using System.IO.Compression;
using System.Text.Json;
using Game.Core;

namespace Game;

// Compress the complete JSON message before ENet fragments it. Diagnostics and
// authority state remain ordinary snapshots; only the RPC payload is encoded.
public static class SnapshotPayload
{
    private const int MaximumBytes = 16 * 1024 * 1024;
    public static string Encode(MatchSnapshot snapshot, WorkCounters? work = null)
    {
        work?.Support(WorkMetric.CodecEncodes, WorkMetric.CodecDecodes, WorkMetric.JsonBytes, WorkMetric.CompressedBytes);
        work?.Add(WorkMetric.CodecEncodes);
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(snapshot, WireJson.Options);
        if (json.Length > MaximumBytes) throw new InvalidDataException("Snapshot exceeds the transport bound.");
        using var output = new MemoryStream();
        using (var compressor = new BrotliStream(output, CompressionLevel.Fastest, leaveOpen: true)) compressor.Write(json);
        work?.Add(WorkMetric.JsonBytes, json.Length); work?.Add(WorkMetric.CompressedBytes, output.Length);
        return Convert.ToBase64String(output.ToArray());
    }
    public static MatchSnapshot? Decode(string payload, WorkCounters? work = null)
    {
        work?.Support(WorkMetric.CodecEncodes, WorkMetric.CodecDecodes, WorkMetric.JsonBytes, WorkMetric.CompressedBytes);
        work?.Add(WorkMetric.CodecDecodes);
        if (payload.Length > (MaximumBytes + 2) / 3 * 4) throw new InvalidDataException("Snapshot exceeds the transport bound.");
        using var input = new MemoryStream(Convert.FromBase64String(payload));
        using var decompressor = new BrotliStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        byte[] buffer = new byte[8192];
        int count;
        while ((count = decompressor.Read(buffer)) != 0)
        {
            if (output.Length + count > MaximumBytes) throw new InvalidDataException("Snapshot exceeds the transport bound.");
            output.Write(buffer, 0, count);
        }
        MatchSnapshot? state = JsonSerializer.Deserialize<MatchSnapshot>(output.GetBuffer().AsSpan(0, checked((int)output.Length)), WireJson.Options);
        if (state is not null && (state.BattleFlowVersion != MatchSnapshot.CurrentBattleFlowVersion
            || state.Phase == Phase.Building && state.Turn is not (1 or 2)
            || state.Phase == Phase.Preparation && state.Turn != 3
            || !Enum.IsDefined(state.CombatSpeed)))
            throw new InvalidDataException("Incompatible battle-flow snapshot. Reconnect to a matching running host; historical snapshots are not game saves.");
        return state;
    }
}
