using System.Security.Cryptography;
using System.Text;
using DevRunner;
namespace DevRunner.Tests;

internal sealed class PlanningFixture : IDisposable
{
    internal string Root { get; } = Path.Combine(Path.GetTempPath(), "odot-planning-" + Guid.NewGuid().ToString("N"));
    internal PlanningFixture()
    {
        Directory.CreateDirectory(Root);
        PlanningEvidence.Git(Root, "init", "-q", "-b", "main");
        Write("source.cs", "// fixture production input\n");
        Write("openspec/specs/existing/spec.md", "# Existing guarantee\n");
        Write("planning/roadmap.yaml", "version: 1\nqueue: []\n");
    }
    internal void Write(string path, string text)
    {
        string full = Path.Combine(Root, path);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, text);
    }
    internal void Change(string id, bool complete = false, bool archived = false)
    {
        string dir = "openspec/changes/" + (archived ? "archive/2026-10-03-" : "") + id;
        Write(dir + "/.openspec.yaml", "schema: spec-driven\ncreated: 2026-10-03\n");
        Write(dir + "/proposal.md", "# Proposal\n");
        Write(dir + "/design.md", "# Design\n");
        Write(dir + "/tasks.md", complete ? "- [x] task\n" : "- [ ] task\n");
        Write(dir + "/specs/new/spec.md", "## ADDED Requirements\n### Requirement: New\n");
    }
    internal void Roadmap(params string[] entries) => Write("planning/roadmap.yaml", "version: 1\nqueue:\n" + string.Join('\n', entries) + "\n");
    internal static string Entry(string id, string state = "proposed", string depends = "[]", string conflicts = "[]", string blockers = "[]", string sources = "[]", string priority = "normal") => $"""
          - change: {id}
            state: {state}
            priority: {priority}
            source_ideas: {sources}
            depends_on: {depends}
            blocked_by: {blockers}
            conflicts_with: {conflicts}
            notes: ""
        """ + (state is "ready" or "in_progress" or "verified" or "completed" or "archived" ? $"\n    approval: planning/evidence/{id}/approval.md" : "")
        + (state is "verified" or "completed" or "archived" ? $"\n    verification: planning/evidence/{id}/review.md" : "");
    internal void Idea(string id, string status = "exploring", string changes = "[]", string? path = null) => Write(path ?? $"planning/ideas/{id}.md", $"""
        ---
        id: {id}
        title: Fixture idea
        status: {status}
        created: 2026-10-03
        related_changes: {changes}
        supersedes: []
        tags: []
        ---
        # Problem
        Fixture.
        """);
    internal void Approval(string id)
    {
        var state = PlanningStore.Read(Root);
        Write($"planning/evidence/{id}/approval.md", $"""
            ---
            change: {id}
            approved_by: user
            approved_at: 2026-10-03
            authorization: Implement exactly {id}.
            spec_digest: {PlanningEvidence.SpecDigest(Root, state.Changes[id])}
            ---
            Explicit test fixture, not live authorization.
            """);
    }
    internal string Commit()
    {
        PlanningEvidence.Git(Root, "add", ".");
        PlanningEvidence.Git(Root, "-c", "user.name=Fixture", "-c", "user.email=fixture@example.invalid", "commit", "-q", "--allow-empty", "-m", "fixture");
        return PlanningEvidence.Git(Root, "rev-parse", "HEAD").Trim();
    }
    internal void Review(string id, bool landed = true)
    {
        string head = Commit();
        var state = PlanningStore.Read(Root);
        string canonicalHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(File.ReadAllText(Path.Combine(Root, "openspec/specs/existing/spec.md")))));
        Write($"planning/evidence/{id}/review.md", $"""
            ---
            change: {id}
            result: PASS
            reviewer_task: independent-reviewer
            shipper_task: shipper
            base: {head}
            head: {head}
            spec_digest: {PlanningEvidence.SpecDigest(Root, state.Changes[id])}
            code_digest: {PlanningEvidence.CodeDigest(Root)}
            canonical_inputs:
              openspec/specs/existing/spec.md: {canonicalHash}
            attempt: 1
            validation_passed: true
            decisions_resolved: true
            """ + (landed ? $"\nlanded_revision: {head}" : "") + "\n---\nFixture PASS, not live acceptance.\n");
    }
    internal PlanningResult Check() => PlanningValidation.Check(Root);
    public void Dispose() => Directory.Delete(Root, true);
}
