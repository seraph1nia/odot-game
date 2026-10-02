using DevRunner;
using Xunit;

namespace DevRunner.Tests;

public sealed class RulePartitionTests
{
    [Fact]
    public void OrdinaryCampaignRowsBelongToExactlyOneProcessPartition()
    {
        var rows = new List<(string Class, int Players, ulong Seed)>();
        foreach (ulong seed in new ulong[] { 0, 1, 123 })
        {
            foreach (string family in RulePartitions.SoloClasses.Where(c => c != "SessionCampaignTests")) rows.Add((family, 1, seed));
            foreach (int players in new[] { 2, 3, 4 }) rows.Add((RulePartitions.CooperativeClass, players, seed));
        }
        Assert.Equal(21, rows.Count); Assert.Equal(21, rows.Distinct().Count());
        Assert.Equal(12, rows.Count(r => RulePartitions.SoloFilter.Contains(r.Class, StringComparison.Ordinal)));
        Assert.Equal(9, rows.Count(r => RulePartitions.CooperativeFilter.Contains(r.Class, StringComparison.Ordinal)));
        Assert.All(rows, r => Assert.Contains("FullyQualifiedName!~" + r.Class, RulePartitions.GeneralFilter, StringComparison.Ordinal));
        Assert.Contains("FullyQualifiedName~SessionCampaignTests", RulePartitions.SoloFilter, StringComparison.Ordinal);
        Assert.DoesNotContain("FullyQualifiedName!~CampaignTests&", RulePartitions.GeneralFilter, StringComparison.Ordinal);
    }
}
