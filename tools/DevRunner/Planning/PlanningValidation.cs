namespace DevRunner;

internal static class PlanningValidation
{
    internal static PlanningResult Check(string root)
    {
        try { return Check(PlanningStore.Read(root)); }
        catch (Exception error) when (error is InvalidDataException or IOException or UnauthorizedAccessException or YamlDotNet.Core.YamlException or ArgumentException)
        { return new(null, [$"planning/roadmap.yaml: {error.Message}"], []); }
    }

    internal static PlanningResult Check(PlanningState state)
    {
        var diagnostics = new List<string>();
        var queue = new Dictionary<string, RoadmapItem>(StringComparer.Ordinal);
        var positions = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int i = 0; i < state.Queue.Length; i++)
        {
            var item = state.Queue[i];
            if (!queue.TryAdd(item.Change, item)) diagnostics.Add($"planning/roadmap.yaml: duplicate change '{item.Change}'.");
            positions.TryAdd(item.Change, i);
        }
        var ideas = new Dictionary<string, PlanningIdea>(StringComparer.Ordinal);
        foreach (var idea in state.Ideas)
        {
            if (!ideas.TryAdd(idea.Id, idea)) diagnostics.Add($"{idea.Path}: duplicate idea '{idea.Id}'.");
            if (idea.Status is not ("inbox" or "exploring" or "shaped" or "promoted" or "parked" or "rejected"))
                diagnostics.Add($"{idea.Path}: invalid idea status '{idea.Status}'.");
            foreach (string change in idea.RelatedChanges)
                if (!state.Changes.ContainsKey(change)) diagnostics.Add($"{idea.Path}: missing related change '{change}'.");
            if (idea.Status == "promoted")
            {
                if (idea.RelatedChanges.Length == 0) diagnostics.Add($"{idea.Path}: promoted idea needs related_changes.");
                foreach (string change in idea.RelatedChanges)
                    if (!queue.TryGetValue(change, out var item) || !item.SourceIdeas.Contains(idea.Id, StringComparer.Ordinal))
                        diagnostics.Add($"{idea.Path}: promoted '{idea.Id}' lacks reciprocal roadmap source link in '{change}'.");
            }
        }
        foreach (var idea in state.Ideas)
            foreach (string superseded in idea.Supersedes)
                if (superseded == idea.Id || !ideas.ContainsKey(superseded)) diagnostics.Add($"{idea.Path}: missing/self superseded idea '{superseded}'.");
        if (state.Queue.Count(item => item.State == "in_progress") > 1) diagnostics.Add("planning/roadmap.yaml: at most one in_progress implementation is allowed.");
        foreach (var item in state.Queue)
        {
            string label = $"planning/roadmap.yaml [{item.Change}]";
            if (item.State is not ("proposed" or "ready" or "in_progress" or "blocked" or "verified" or "completed" or "archived" or "legacy_completed"))
                diagnostics.Add($"{label}: invalid state '{item.State}'.");
            if (item.Priority is not ("low" or "normal" or "high")) diagnostics.Add($"{label}: priority must be low, normal or high.");
            if (!state.Changes.TryGetValue(item.Change, out var change)) diagnostics.Add($"{label}: OpenSpec change does not exist.");
            else
            {
                if (change.Archived && item.State is not ("archived" or "legacy_completed")) diagnostics.Add($"{label}: archived change cannot use '{item.State}'.");
                if (!change.Archived && item.State == "archived") diagnostics.Add($"{label}: archived state requires an archive directory.");
                if (item.State is "ready" or "in_progress" or "verified" or "completed" or "archived")
                {
                    diagnostics.AddRange(PlanningEvidence.CheckApproval(state, item, change));
                    if (!PlanningEvidence.Ready(state.Root, change)) diagnostics.Add($"{label}: complete proposal/design/specs/tasks and supported OpenSpec metadata required.");
                }
                if (item.State is "verified" or "completed" or "archived") diagnostics.AddRange(PlanningEvidence.CheckReview(state, item, change));
                if (item.State == "legacy_completed" && !PlanningEvidence.TasksComplete(change)) diagnostics.Add($"{label}: legacy completion requires a nonempty fully completed task list.");
            }
            foreach (string idea in item.SourceIdeas)
            {
                if (!ideas.TryGetValue(idea, out var source)) diagnostics.Add($"{label}: source idea '{idea}' does not exist.");
                else if (!source.RelatedChanges.Contains(item.Change, StringComparer.Ordinal)) diagnostics.Add($"{label}: source idea '{idea}' lacks related_changes link.");
            }
            foreach (string reference in item.DependsOn.Concat(item.ConflictsWith))
            {
                if (!state.Changes.TryGetValue(reference, out var referenced)) diagnostics.Add($"{label}: referenced change '{reference}' does not exist.");
                else if (!referenced.Archived && !queue.ContainsKey(reference)) diagnostics.Add($"{label}: active reference '{reference}' must be in queue.");
            }
            foreach (string dependency in item.DependsOn)
                if (positions.TryGetValue(dependency, out int position) && position >= positions[item.Change])
                    diagnostics.Add($"{label}: dependency '{dependency}' must precede dependent in queue.");
            if (item.BlockedBy.Any(string.IsNullOrWhiteSpace)) diagnostics.Add($"{label}: blocker text cannot be empty.");
        }
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        foreach (string id in queue.Keys) Cycle(id);
        if (diagnostics.Count > 0) return new(null, [.. diagnostics], []);
        var exclusions = new List<string>();
        bool inProgress = state.Queue.Any(item => item.State == "in_progress");
        foreach (var item in state.Queue)
        {
            var reasons = new List<string>();
            if (item.State != "ready") reasons.Add($"state {item.State}");
            if (inProgress) reasons.Add("an implementation is in_progress");
            foreach (string dependency in item.DependsOn)
                if (!Finished(dependency)) reasons.Add($"unfinished dependency {dependency}");
            reasons.AddRange(item.BlockedBy.Select(blocker => $"blocker {blocker}"));
            foreach (string conflict in item.ConflictsWith)
                if (!Finished(conflict)) reasons.Add($"unresolved conflict {conflict}");
            if (reasons.Count == 0) return new(item.Change, [], [.. exclusions]);
            exclusions.Add($"{item.Change}: {string.Join("; ", reasons)}");
        }
        return new(null, [], [.. exclusions]);

        bool Finished(string id) => queue.TryGetValue(id, out var entry)
            ? entry.State is "completed" or "archived" or "legacy_completed"
            : state.Changes[id].Archived; // Historical archive outside queue predates the workflow.
        void Cycle(string id)
        {
            if (visited.Contains(id) || !queue.TryGetValue(id, out var item)) return;
            if (!visiting.Add(id)) { diagnostics.Add($"planning/roadmap.yaml: dependency cycle at '{id}'."); return; }
            foreach (string dependency in item.DependsOn) Cycle(dependency);
            visiting.Remove(id);
            visited.Add(id);
        }
    }
}
