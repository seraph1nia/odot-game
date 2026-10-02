namespace DevRunner;

internal static class RulePartitions
{
    public static readonly string[] SoloClasses = ["FrontlineCampaignTests", "MixedCampaignTests", "TowersCampaignTests", "ResearchCampaignTests", "SessionCampaignTests"];
    public const string CooperativeClass = "CooperativeCampaignTests";
    public static string SoloFilter => string.Join('|', SoloClasses.Select(c => "FullyQualifiedName~" + c));
    public static string CooperativeFilter => "FullyQualifiedName~" + CooperativeClass;
    public static string GeneralFilter => string.Join('&', SoloClasses.Append(CooperativeClass).Select(c => "FullyQualifiedName!~" + c));
}

internal sealed partial class Runner
{
    private Task CheapTests()
    {
        var checks = new[]
        {
            (Name: "rules-cooperative", Project: "tests/Game.Core.Tests/Game.Core.Tests.csproj", Filter: RulePartitions.CooperativeFilter),
            (Name: "rules-solo", Project: "tests/Game.Core.Tests/Game.Core.Tests.csproj", Filter: RulePartitions.SoloFilter),
            (Name: "rules-general", Project: "tests/Game.Core.Tests/Game.Core.Tests.csproj", Filter: RulePartitions.GeneralFilter),
            (Name: "tooling", Project: "tests/DevRunner.Tests/DevRunner.Tests.csproj", Filter: "")
        };
        var scenarios = checks.Select(check => new Scenario(check.Name, "Isolated C# process; disjoint campaign coverage", async token =>
        {
            var worker = new Runner(options, token, _evidence, root: _root);
            var args = new List<string> { "test", check.Project, "--no-restore", "--no-build", "--nologo" };
            if (check.Filter.Length != 0) args.AddRange(["--filter", check.Filter]);
            await worker.Execute(check.Name, "dotnet", args.ToArray());
        })).ToArray();
        Console.WriteLine("C# coverage: all rules and tooling; process budget=2; cooperative=9, solo strategies=12, serialized session campaign=1; disjoint filters.");
        return _evidence.Measure("rules", "suite", () => ScenarioScheduler.Run(scenarios, 2, cancellation));
    }
}
