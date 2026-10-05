using System.Text.Json;

namespace DevRunner;

// Fixed-schema post-cleanup analysis of the owned admission diagnostic only.
internal static class MeleeTimingReport
{
    private sealed record Entry(long Nanoseconds, string Kind, long? Revision, long? Tick, string? Digest, JsonElement Info);
    private static (JsonElement Clock, Entry[] Entries) Read(string path)
    {
        JsonElement[] lines = File.ReadLines(path).Select(s => JsonSerializer.Deserialize<JsonElement>(s)).ToArray();
        if (lines.Length > Game.OwnedTimingTrace.MaximumRecords || lines.Any(e => e.GetProperty("Kind").GetString() == "overflow")
            || lines[^1].GetProperty("Kind").GetString() != "end") throw new InvalidOperationException("Incomplete/overflowed timing owner evidence.");
        long previous = 0;
        for (int index = 0; index < lines.Length; index++)
        {
            if (lines[index].GetProperty("Ordinal").GetInt32() != index + 1 || lines[index].GetProperty("Nanoseconds").GetInt64() < previous)
                throw new InvalidOperationException("Timing ordinals/clocks are not ordered.");
            previous = lines[index].GetProperty("Nanoseconds").GetInt64();
        }
        return (lines[0].GetProperty("Info"), lines.Select(e => new Entry(e.GetProperty("Nanoseconds").GetInt64(), e.GetProperty("Kind").GetString()!,
            e.TryGetProperty("Revision", out var revision) ? revision.GetInt64() : null,
            e.TryGetProperty("Tick", out var tick) ? tick.GetInt64() : null,
            e.TryGetProperty("Digest", out var digest) ? digest.GetString() : null,
            e.TryGetProperty("Info", out var info) ? info : JsonSerializer.Deserialize<JsonElement>("{}"))).ToArray());
    }
    internal static void RequireAligned(JsonElement left, JsonElement right)
    {
        foreach (string field in new[] { "Clock", "Frequency", "BootId", "TimeNamespace" })
            if (left.GetProperty(field).ToString() != right.GetProperty(field).ToString()) throw new InvalidOperationException("Cross-process clock alignment mismatch: " + field);
    }
    internal static void Write(string directory)
    {
        var server = Read(Path.Combine(directory, "ui-server-timing.jsonl"));
        var client = Read(Path.Combine(directory, "ui-client-timing.jsonl"));
        var observer = Read(Path.Combine(directory, "ui-observer-timing.jsonl"));
        var driver = Read(Path.Combine(directory, "driver-timing.jsonl"));
        RequireAligned(server.Clock, client.Clock); RequireAligned(server.Clock, observer.Clock); RequireAligned(server.Clock, driver.Clock);
        Entry[] publications = server.Entries.Where(e => e.Kind == "authority-publish").ToArray();
        object Stats(IEnumerable<double> values)
        {
            double[] sorted = values.Order().ToArray();
            return new { Count = sorted.Length, Minimum = sorted[0], Median = sorted[sorted.Length / 2], Maximum = sorted[^1] };
        }
        object Delivery(Entry[] entries)
        {
            var delays = new List<double>(); var decode = new List<double>(); var revisions = new List<long>();
            var records = new List<object>();
            foreach (Entry receive in entries.Where(e => e.Kind == "snapshot-rpc-receive"))
            {
                // Forced identical-state publications can repeat a digest. The
                // most recent prior send supplies a conservative latency lower
                // bound; no new wire/packet identifier is invented.
                Entry? send = publications.LastOrDefault(p => p.Digest == receive.Digest && p.Nanoseconds <= receive.Nanoseconds);
                if (send is null) throw new InvalidOperationException("Received snapshot lacks an owned publication digest.");
                Entry decoded = entries.First(e => e.Kind == "snapshot-decode-complete" && e.Digest == receive.Digest && e.Nanoseconds >= receive.Nanoseconds);
                if (decoded.Revision != send.Revision) throw new InvalidOperationException("Snapshot digest/revision mismatch.");
                revisions.Add(send.Revision!.Value);
                if (send.Tick > 0)
                {
                    double ms = (receive.Nanoseconds - send.Nanoseconds) / 1e6;
                    delays.Add(ms); decode.Add((decoded.Nanoseconds - receive.Nanoseconds) / 1e6);
                    records.Add(new { send.Revision, send.Tick, Send = send.Nanoseconds, Receive = receive.Nanoseconds, DecodeComplete = decoded.Nanoseconds, LatencyLowerBoundMs = ms });
                }
            }
            if (!revisions.SequenceEqual(revisions.Order())) throw new InvalidOperationException("Snapshot revision order changed.");
            return new
            {
                Received = revisions.Count,
                Published = publications.Length,
                RevisionOrderConserved = true,
                PublicationToRpcLowerBoundMilliseconds = Stats(delays),
                DecodeMilliseconds = Stats(decode),
                Records = records,
                Limitation = "End-of-diagnostic undelivered tail is not a proven drop; no post-terminal catch-up/acceptance claim."
            };
        }
        Entry[] presented = client.Entries.Where(e => e.Kind == "rendered-observation").ToArray();
        var age = presented.Select(e =>
        {
            Entry authority = publications.Last(p => p.Nanoseconds <= e.Nanoseconds);
            double tick = e.Info.TryGetProperty("PresentedTick", out var value) ? value.GetDouble() : 0;
            return new { e.Nanoseconds, AppliedTick = e.Tick, PresentedTick = tick, PublishedAuthorityTick = authority.Tick, AuthorityGapTicks = authority.Tick - tick };
        }).ToArray();
        string json = JsonSerializer.Serialize(new
        {
            Acceptance = false,
            Clock = server.Clock,
            Graphical = Delivery(client.Entries),
            Observer = Delivery(observer.Entries),
            ObservationAges = age,
            Coordination = driver.Entries.Where(e => e.Kind.Contains("observer", StringComparison.Ordinal) || e.Kind.Contains("registration", StringComparison.Ordinal)).ToArray()
        }, Evidence.JsonOptions);
        File.WriteAllText(Path.Combine(directory, "timing-verdict-evidence.json"), json);
    }
}
