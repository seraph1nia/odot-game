using System.IO.Compression;
using System.Text.Json;
using Game;
using Xunit;
using DevRunner;

namespace Game.Core.Tests.Profiling;

internal static class SnapshotProfile
{
    private const int Repetitions = 25;
    public static object Run(Measurements measurement, WorkCounters? work, ulong seed)
    {
        measurement.Enter("setup");
        var samples = new List<object>();
        foreach (string name in new[] { "ordinary", "large" })
        {
            measurement.Enter("setup");
            using Match match = name == "ordinary" ? CombatReplayFixture.StartFight(1, seed) : LargeBattle.Create(2048, seed);
            for (int tick = 0; tick < 36; tick++) match.Step();
            WorkCounters? localWork = work is null ? null : new();
            match.SetWorkCounters(localWork);
            measurement.Enter("assertion-hash");
            MatchSnapshot fixedInput = match.Snapshot() with { MatchId = "session" };
            byte[] json = JsonSerializer.SerializeToUtf8Bytes(fixedInput, WireJson.Options);
            string compressed = SnapshotPayload.Encode(fixedInput, localWork);
            Assert.Equal(LargeBattle.Digest(fixedInput), LargeBattle.Digest(SnapshotPayload.Decode(compressed, localWork)));
            Assert.Equal(compressed, Compress(json));
            using (MemoryStream decoded = Decompress(compressed)) Assert.Equal(json, decoded.GetBuffer().AsSpan(0, checked((int)decoded.Length)).ToArray());
            var phases = new Dictionary<string, MetricSample>(StringComparer.Ordinal);
            void Measure(string phase, Action action)
            {
                measurement.Enter(name + "-" + phase); MetricSample start = measurement.Sample();
                for (int repeat = 0; repeat < Repetitions; repeat++) action();
                phases[phase] = MetricSample.Difference(start, measurement.Sample());
            }
            Measure("projection", () => _ = match.Snapshot());
            Measure("json-serialize", () => _ = JsonSerializer.SerializeToUtf8Bytes(fixedInput, WireJson.Options));
            Measure("json-deserialize", () => _ = JsonSerializer.Deserialize<MatchSnapshot>(json, WireJson.Options));
            Measure("brotli-compress-base64", () => _ = Compress(json));
            Measure("brotli-decompress-base64", () => { using MemoryStream decoded = Decompress(compressed); });
            Measure("codec-encode", () => _ = SnapshotPayload.Encode(fixedInput, localWork));
            Measure("codec-decode", () => _ = SnapshotPayload.Decode(compressed, localWork));
            measurement.Enter("assertion-hash");
            samples.Add(new
            {
                Name = name,
                InputDigest = LargeBattle.Digest(fixedInput),
                match.ConfigurationFingerprint,
                Repetitions,
                JsonBytes = json.Length,
                CompressedBytes = Convert.FromBase64String(compressed).Length,
                TransportCharacters = compressed.Length,
                Phases = phases,
                Work = localWork?.Snapshot()
            });
        }
        return samples;
    }
    // Measurement copies of the existing settings/stages, checked against the
    // production codec. They don't change its allocation or decompression path.
    internal static string Compress(byte[] json)
    {
        using var output = new MemoryStream();
        using (var compressor = new BrotliStream(output, CompressionLevel.Fastest, leaveOpen: true)) compressor.Write(json);
        return Convert.ToBase64String(output.ToArray());
    }
    internal static MemoryStream Decompress(string payload)
    {
        using var input = new MemoryStream(Convert.FromBase64String(payload));
        using var decompressor = new BrotliStream(input, CompressionMode.Decompress);
        var output = new MemoryStream();
        try
        {
            byte[] buffer = new byte[8192]; int count;
            while ((count = decompressor.Read(buffer)) != 0)
            {
                if (output.Length + count > 16 * 1024 * 1024) throw new InvalidDataException("Snapshot exceeds the transport bound.");
                output.Write(buffer, 0, count);
            }
            return output;
        }
        catch { output.Dispose(); throw; }
    }
}
