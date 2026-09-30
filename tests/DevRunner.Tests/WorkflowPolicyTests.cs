using System.Text.RegularExpressions;
using Xunit;

namespace DevRunner.Tests;

public sealed partial class WorkflowPolicyTests
{
    [Fact]
    public void ActionsAreImmutableAndReleaseWorkflowDoesNotRerunTests()
    {
        string root = Root();
        string ci = File.ReadAllText(Path.Combine(root, ".github", "workflows", "ci.yml"));
        string release = File.ReadAllText(Path.Combine(root, ".github", "workflows", "release.yml"));
        foreach (Match match in Uses().Matches(ci + "\n" + release)) Assert.Matches("^[0-9a-f]{40}$", match.Groups[1].Value);
        Assert.DoesNotContain("actions/checkout@v", ci + release, StringComparison.Ordinal);
        Assert.DoesNotContain("jdx/mise-action@v", ci + release, StringComparison.Ordinal);
        Assert.DoesNotContain("mise run ci", release, StringComparison.Ordinal);
        Assert.DoesNotContain("verify-installed", release, StringComparison.Ordinal);
        Assert.Contains("needs: preflight", release, StringComparison.Ordinal);
        Assert.Contains("permissions:\n      contents: write", release, StringComparison.Ordinal);
        Assert.DoesNotContain("actions/upload-artifact", ci, StringComparison.Ordinal);
        Assert.Contains("on:\n  push:\n    branches: [main]\n  pull_request:\n    branches: [main]", ci, StringComparison.Ordinal);
        Assert.DoesNotContain("workflow_dispatch:", ci, StringComparison.Ordinal);
        Assert.Contains("  linux-package:", ci, StringComparison.Ordinal);
        Assert.DoesNotContain("\n  windows-package:", ci, StringComparison.Ordinal);
        Assert.Contains("on:\n  release:\n    types: [published]", release, StringComparison.Ordinal);
        Assert.DoesNotContain("\n  push:", release, StringComparison.Ordinal);
        Assert.DoesNotContain("\n  pull_request:", release, StringComparison.Ordinal);
        Assert.DoesNotContain("workflow_dispatch:", release, StringComparison.Ordinal);
    }

    private static string Root()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Odot.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }

    [GeneratedRegex(@"uses:\s+[^@\s]+@([0-9a-f]+)", RegexOptions.CultureInvariant)]
    private static partial Regex Uses();
}
