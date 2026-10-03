namespace DevRunner;

internal sealed record RoadmapItem(string Change, string State, string Priority, string[] SourceIdeas,
    string[] DependsOn, string[] BlockedBy, string[] ConflictsWith, string Notes, string? Approval, string? Verification);
internal sealed record PlanningIdea(string Id, string Status, string[] RelatedChanges, string[] Supersedes, string Path);
internal sealed record PlanningChange(string Id, string Directory, bool Archived);
internal sealed record PlanningState(string Root, RoadmapItem[] Queue, PlanningIdea[] Ideas, Dictionary<string, PlanningChange> Changes);
internal sealed record PlanningResult(string? Change, string[] Diagnostics, string[] Exclusions)
{
    public bool Valid => Diagnostics.Length == 0;
}
