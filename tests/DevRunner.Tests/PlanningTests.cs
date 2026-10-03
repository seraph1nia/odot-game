using DevRunner;
using Xunit;
namespace DevRunner.Tests;

public sealed class PlanningTests
{
    [Theory]
    [InlineData("version: 2\nqueue: []", "version")]
    [InlineData("version: 1\nversion: 1\nqueue: []", "duplicate")]
    [InlineData("version: 1\nqueue: []\nunknown: true", "unknown")]
    [InlineData("version: 1\nqueue: null", "array")]
    [InlineData("version: 1\nqueue: &items []", "anchors")]
    public void RejectsMalformedYaml(string text, string diagnostic)
    {
        using var fixture = new PlanningFixture();
        fixture.Write("planning/roadmap.yaml", text);
        var result = fixture.Check();
        Assert.False(result.Valid);
        Assert.Contains(diagnostic, string.Join('\n', result.Diagnostics), StringComparison.OrdinalIgnoreCase);
        Assert.Null(result.Change);
    }

    [Theory]
    [InlineData("duplicate", "duplicate change")]
    [InlineData("missing", "does not exist")]
    [InlineData("dependency", "referenced change")]
    [InlineData("order", "must precede")]
    [InlineData("cycle", "cycle")]
    [InlineData("state", "invalid state")]
    [InlineData("source", "source idea")]
    [InlineData("conflict", "referenced change")]
    [InlineData("parallel", "at most one")]
    [InlineData("archive", "archived change")]
    public void RejectsGraphAndStateViolations(string violation, string diagnostic)
    {
        using var fixture = new PlanningFixture();
        fixture.Change("first"); fixture.Change("second");
        switch (violation)
        {
            case "duplicate": fixture.Roadmap(PlanningFixture.Entry("first"), PlanningFixture.Entry("first")); break;
            case "missing": fixture.Roadmap(PlanningFixture.Entry("absent")); break;
            case "dependency": fixture.Roadmap(PlanningFixture.Entry("first", depends: "[absent]")); break;
            case "order": fixture.Roadmap(PlanningFixture.Entry("first", depends: "[second]"), PlanningFixture.Entry("second")); break;
            case "cycle": fixture.Roadmap(PlanningFixture.Entry("first", depends: "[second]"), PlanningFixture.Entry("second", depends: "[first]")); break;
            case "state": fixture.Roadmap(PlanningFixture.Entry("first", "imaginary")); break;
            case "source": fixture.Roadmap(PlanningFixture.Entry("first", sources: "[IDEA-009]")); break;
            case "conflict": fixture.Roadmap(PlanningFixture.Entry("first", conflicts: "[absent]")); break;
            case "parallel": fixture.Roadmap(PlanningFixture.Entry("first", "in_progress"), PlanningFixture.Entry("second", "in_progress")); fixture.Approval("first"); fixture.Approval("second"); break;
            case "archive": Directory.Delete(Path.Combine(fixture.Root, "openspec/changes/first"), true); fixture.Change("first", archived: true); fixture.Roadmap(PlanningFixture.Entry("first", "ready")); fixture.Approval("first"); break;
        }
        var result = fixture.Check();
        Assert.False(result.Valid);
        Assert.Contains(diagnostic, string.Join('\n', result.Diagnostics), StringComparison.Ordinal);
        Assert.Null(result.Change);
    }

    [Fact]
    public void PlaceholdersCanStayProposedButCannotBeReady()
    {
        using var fixture = new PlanningFixture();
        fixture.Write("openspec/changes/placeholder/.openspec.yaml", "schema: spec-driven\n");
        fixture.Roadmap(PlanningFixture.Entry("placeholder"));
        Assert.True(fixture.Check().Valid);
        fixture.Roadmap(PlanningFixture.Entry("placeholder", "ready")); fixture.Approval("placeholder");
        Assert.Contains(fixture.Check().Diagnostics, message => message.Contains("complete proposal", StringComparison.Ordinal));
    }

    [Fact]
    public void ExactArchiveIdentityRejectsAmbiguousDates()
    {
        using var fixture = new PlanningFixture();
        fixture.Change("done", true, true);
        fixture.Write("openspec/changes/archive/2026-10-04-done/.openspec.yaml", "schema: spec-driven\n");
        Assert.Contains(fixture.Check().Diagnostics, message => message.Contains("ambiguous", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("empty", "promoted idea")]
    [InlineData("reciprocal", "reciprocal")]
    [InlineData("duplicate", "duplicate idea")]
    [InlineData("frontmatter", "frontmatter")]
    [InlineData("unknown", "invalid idea status")]
    public void ValidatesIdeasAcrossArchiveAndSourceLinks(string violation, string diagnostic)
    {
        using var fixture = new PlanningFixture();
        fixture.Change("first"); fixture.Roadmap(PlanningFixture.Entry("first"));
        fixture.Idea("IDEA-001", "promoted", violation == "empty" ? "[]" : "[first]");
        if (violation == "duplicate") fixture.Idea("IDEA-001", path: "planning/archive/ideas/IDEA-001.md");
        if (violation == "frontmatter") fixture.Write("planning/ideas/IDEA-001.md", "missing frontmatter");
        if (violation == "unknown") fixture.Idea("IDEA-001", "unknown");
        Assert.Contains(diagnostic, string.Join('\n', fixture.Check().Diagnostics), StringComparison.Ordinal);
    }

    [Fact]
    public void ReadySelectionRespectsOrderDependenciesAndBlockers()
    {
        using var fixture = new PlanningFixture();
        foreach (string id in new[] { "first", "second", "third", "fourth" }) fixture.Change(id);
        fixture.Roadmap(PlanningFixture.Entry("first"), PlanningFixture.Entry("second", "ready", "[first]", priority: "high"),
            PlanningFixture.Entry("third", "ready", blockers: "[decision pending]"), PlanningFixture.Entry("fourth", "ready", priority: "low"));
        foreach (string id in new[] { "second", "third", "fourth" }) fixture.Approval(id);
        var result = fixture.Check();
        Assert.True(result.Valid, string.Join('\n', result.Diagnostics));
        Assert.Equal("fourth", result.Change);
        Assert.Contains(result.Exclusions, message => message.Contains("unfinished dependency first", StringComparison.Ordinal));
        Assert.Equal(result.Change, fixture.Check().Change);
        Assert.Equal(result.Exclusions, fixture.Check().Exclusions);
    }

    [Fact]
    public void InProgressAndUnresolvedConflictPreventDispatch()
    {
        using var fixture = new PlanningFixture(); fixture.Change("first"); fixture.Change("second");
        fixture.Roadmap(PlanningFixture.Entry("first", "in_progress"), PlanningFixture.Entry("second", "ready")); fixture.Approval("first"); fixture.Approval("second");
        Assert.True(fixture.Check().Valid); Assert.Null(fixture.Check().Change);
        fixture.Roadmap(PlanningFixture.Entry("first"), PlanningFixture.Entry("second", "ready", conflicts: "[first]"));
        Assert.True(fixture.Check().Valid); Assert.Null(fixture.Check().Change);
    }

    [Fact]
    public void SupportsSkippedSpecsAndHistoricalArchivedDependencies()
    {
        using var fixture = new PlanningFixture(); fixture.Change("old", true, true); fixture.Change("first");
        Directory.Delete(Path.Combine(fixture.Root, "openspec/changes/first/specs"), true);
        fixture.Write("openspec/changes/first/.openspec.yaml", "schema: spec-driven\nskip_specs: true\n");
        fixture.Roadmap(PlanningFixture.Entry("first", "ready", "[old]")); fixture.Approval("first");
        Assert.Equal("first", fixture.Check().Change);
    }

    [Fact]
    public void LegacyCompletionDoesNotFabricatePassButRequiresCompletedTasks()
    {
        using var fixture = new PlanningFixture(); fixture.Change("old", true); fixture.Change("first");
        fixture.Roadmap(PlanningFixture.Entry("old", "legacy_completed"), PlanningFixture.Entry("first", "ready", "[old]")); fixture.Approval("first");
        Assert.Equal("first", fixture.Check().Change);
        fixture.Write("openspec/changes/old/tasks.md", "- [ ] still pending\n");
        Assert.False(fixture.Check().Valid);
    }

    [Fact]
    public void ApprovalDigestIgnoresTaskMarkersButRejectsChangedRequirements()
    {
        using var fixture = new PlanningFixture(); fixture.Change("first"); fixture.Roadmap(PlanningFixture.Entry("first", "ready")); fixture.Approval("first");
        fixture.Write("openspec/changes/first/tasks.md", "- [x] task\n"); Assert.Equal("first", fixture.Check().Change);
        fixture.Write("openspec/changes/first/proposal.md", "different requirements\n");
        Assert.Contains(fixture.Check().Diagnostics, message => message.Contains("stale", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("source", "code_digest")]
    [InlineData("canonical", "canonical input")]
    [InlineData("reviewer", "distinct")]
    [InlineData("decision", "resolved decisions")]
    [InlineData("attempt", "attempt")]
    [InlineData("unlanded", "landed_revision")]
    public void CompletionNeedsCurrentIndependentPass(string violation, string diagnostic)
    {
        using var fixture = new PlanningFixture(); fixture.Change("first", true); fixture.Roadmap(PlanningFixture.Entry("first", violation == "unlanded" ? "completed" : "verified")); fixture.Approval("first"); fixture.Review("first", violation != "unlanded");
        if (violation == "source") fixture.Write("source.cs", "changed\n");
        if (violation == "canonical") fixture.Write("openspec/specs/existing/spec.md", "changed guarantee\n");
        if (violation is "reviewer" or "decision" or "attempt")
        {
            string path = Path.Combine(fixture.Root, "planning/evidence/first/review.md");
            string text = File.ReadAllText(path);
            text = violation switch { "reviewer" => text.Replace("independent-reviewer", "shipper", StringComparison.Ordinal), "decision" => text.Replace("decisions_resolved: true", "decisions_resolved: false", StringComparison.Ordinal), _ => text.Replace("attempt: 1", "attempt: 3", StringComparison.Ordinal) };
            fixture.Write("planning/evidence/first/review.md", text);
        }
        Assert.Contains(diagnostic, string.Join('\n', fixture.Check().Diagnostics), StringComparison.Ordinal);
    }

    [Fact]
    public void ValidPassSurvivesReceiptAndArchiveMovement()
    {
        using var fixture = new PlanningFixture(); fixture.Change("first", true); fixture.Roadmap(PlanningFixture.Entry("first", "verified")); fixture.Approval("first"); fixture.Review("first", false);
        Assert.True(fixture.Check().Valid, string.Join('\n', fixture.Check().Diagnostics));
        Directory.CreateDirectory(Path.Combine(fixture.Root, "openspec/changes/archive"));
        Directory.Move(Path.Combine(fixture.Root, "openspec/changes/first"), Path.Combine(fixture.Root, "openspec/changes/archive/2026-10-03-first"));
        fixture.Roadmap(PlanningFixture.Entry("first", "archived")); fixture.Review("first");
        Assert.True(fixture.Check().Valid, string.Join('\n', fixture.Check().Diagnostics));
    }

    [Fact]
    public void CompletedHistoryAllowsLaterUnrelatedProductionAndCanonicalChanges()
    {
        using var fixture = new PlanningFixture(); fixture.Change("first", true); fixture.Roadmap(PlanningFixture.Entry("first", "completed")); fixture.Approval("first"); fixture.Review("first");
        fixture.Write("source.cs", "later separately reviewed work\n"); fixture.Write("openspec/specs/existing/spec.md", "later guarantee\n"); fixture.Commit();
        Assert.True(fixture.Check().Valid, string.Join('\n', fixture.Check().Diagnostics));
    }

    [Fact]
    public void CommandReportsNoEligibleAsSuccessAndInvalidAsFailure()
    {
        using var fixture = new PlanningFixture(); using var output = new StringWriter();
        Assert.Equal(0, PlanningCommand.Run(fixture.Root, "planning-next", ["--json"], output)); Assert.Contains("\"change\":null", output.ToString(), StringComparison.Ordinal);
        fixture.Roadmap(PlanningFixture.Entry("absent")); output.GetStringBuilder().Clear();
        Assert.Equal(1, PlanningCommand.Run(fixture.Root, "planning-next", ["--json"], output)); Assert.Contains("does not exist", output.ToString(), StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => PlanningCommand.Run(fixture.Root, "planning-next", ["--reorder"], output));
    }

    [Fact]
    public void RejectsSymlinkedRequirementsBeforeReadingExternalFiles()
    {
        if (!OperatingSystem.IsLinux()) return;
        using var fixture = new PlanningFixture(); fixture.Change("first"); fixture.Roadmap(PlanningFixture.Entry("first", "ready")); fixture.Approval("first");
        string specs = Path.Combine(fixture.Root, "openspec/changes/first/specs");
        Directory.Delete(specs, true); Directory.CreateSymbolicLink(specs, Path.Combine(fixture.Root, "openspec/specs"));
        Assert.Contains(fixture.Check().Diagnostics, message => message.Contains("symlink", StringComparison.Ordinal));
    }

    [Fact]
    public void RejectsPathsOutsideEvidenceOwnership()
    {
        using var fixture = new PlanningFixture(); fixture.Change("first");
        fixture.Roadmap(PlanningFixture.Entry("first", "ready").Replace("planning/evidence/first/approval.md", "../../credentials.md", StringComparison.Ordinal));
        Assert.Contains(fixture.Check().Diagnostics, message => message.Contains("receipt path", StringComparison.Ordinal));
        Assert.Throws<InvalidDataException>(() => PlanningStore.SafePath(fixture.Root, "../outside"));
        Assert.Throws<InvalidDataException>(() => PlanningStore.SafePath(fixture.Root, "planning/evidence/first/../second/approval.md"));
    }
}
