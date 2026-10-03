using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

// Only domain facts enter this buffer; protocol credentials and arbitrary wire
// payloads are never accepted. Formatting recent transactions is failure/trace work.
internal sealed class CampaignDiagnostics(ITestOutputHelper output, bool detailed)
{
    private readonly Queue<Func<string>> _recent = new();
    internal int RecentCount => _recent.Count;
    internal void Record(Func<string> facts)
    {
        _recent.Enqueue(facts); if (_recent.Count > 64) _recent.Dequeue();
        if (detailed) output.WriteLine(facts());
    }
    internal void Summary(string facts)
    {
        output.WriteLine(facts); _recent.Enqueue(() => facts); if (_recent.Count > 64) _recent.Dequeue();
    }
    internal void Failure()
    {
        output.WriteLine("Recent campaign facts (maximum 64):");
        foreach (Func<string> facts in _recent) output.WriteLine(facts());
    }
    internal static ResourceCost Investment(IEnumerable<ResourceCost> investments)
    {
        ResourceCost sum = default;
        foreach (ResourceCost investment in investments)
        {
            Assert.True(sum.TryAdd(investment, out ResourceCost next), "Invalid/overflowing campaign building investment.");
            sum = next;
        }
        return sum;
    }
}
