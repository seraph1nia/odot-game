using Game.Core;

namespace Game;

public sealed record WorldtreeSlot(string VisualId, TechnologyId Technology, string Icon);
public sealed record WorldtreeState(string[] Acquired, string[] Available, string Selected, int Points);

// Canonical artwork IDs are presentation identities, never protocol/save IDs.
// The three structural nodes describe existing village systems, not free purchases.
public static class WorldtreeProjection
{
    public static IReadOnlyList<WorldtreeSlot> Slots { get; } = Array.AsReadOnly(new WorldtreeSlot[]
    {
        new("root", TechnologyId.None, "world_seed"),
        new("trunk_1", TechnologyId.None, "book"),
        new("trunk_2", TechnologyId.None, "tower"),
        new("primary_left", TechnologyId.MeleeFoundation, "crossed_swords"),
        new("primary_center", TechnologyId.MagicFoundation, "spark"),
        new("primary_right", TechnologyId.RangedFoundation, "bow"),
        new("secondary_left_a", TechnologyId.Guardian, "shield"),
        new("secondary_left_b", TechnologyId.Assault, "crossed_swords"),
        new("secondary_center_a", TechnologyId.Fire, "flame"),
        new("secondary_center_b", TechnologyId.Frost, "snowflake"),
        new("secondary_right_a", TechnologyId.Venom, "poison_drop"),
        new("secondary_right_b", TechnologyId.Precision, "eye"),
        new("terminal_left_a_1", TechnologyId.GuardianMastery, "shield"),
        new("terminal_left_a_2", TechnologyId.Stonecraft, "stone"),
        new("terminal_left_b_1", TechnologyId.AssaultMastery, "antlers"),
        new("terminal_left_b_2", TechnologyId.None, "acorn"),
        new("terminal_center_a_1", TechnologyId.FireMastery, "flame"),
        new("terminal_center_a_2", TechnologyId.None, "lantern"),
        new("terminal_center_b_1", TechnologyId.FrostMastery, "snowflake"),
        new("terminal_center_b_2", TechnologyId.Irrigation, "water_drop"),
        new("terminal_right_a_1", TechnologyId.VenomMastery, "poison_drop"),
        new("terminal_right_a_2", TechnologyId.None, "thorn_vine"),
        new("terminal_right_b_1", TechnologyId.PrecisionMastery, "eye"),
        new("terminal_right_b_2", TechnologyId.ManagedForestry, "leaf")
    });
    public static WorldtreeSlot? Find(string visualId) => Slots.FirstOrDefault(s => s.VisualId == visualId);
    public static WorldtreeState Project(MatchSnapshot? state, CityState? city, int player, bool connected, string selected)
    {
        string[] acquired = Slots.Where(s => city is not null && (s.VisualId is "root" or "trunk_1" or "trunk_2" || city.Research.Has(s.Technology))).Select(s => s.VisualId).ToArray();
        string[] available = Slots.Where(s => s.Technology != TechnologyId.None && ProgressionPresentation.ResearchAccess(state, city, player, connected, s.Technology).Available).Select(s => s.VisualId).ToArray();
        return new(acquired, available, Find(selected) is null ? "root" : selected, city?.Research.Points ?? 0);
    }
    public static string Title(string id) => Find(id)?.Technology is { } technology && technology != TechnologyId.None
        ? string.Join(' ', TechnologyIds.Name(technology).Split('-').Select(word => char.ToUpperInvariant(word[0]) + word[1..]))
        : id switch { "root" => "The Common Watch", "trunk_1" => "Shared knowledge", "trunk_2" => "Research towers", _ => "Dormant possibility" };
    public static string Description(string id, MatchSnapshot? state, CityState? city, int player, bool connected)
    {
        if (Find(id) is not { } slot) return "Unknown node.";
        if (slot.Technology == TechnologyId.None)
            return id switch
            {
                "root" => $"The Common Watch\n• {state?.Rules.BaseGold ?? 2} gold per production\n• City-specific research",
                "trunk_1" => "Shared knowledge\n• +1 point per shared wave clear\n• One specialization per combat class",
                "trunk_2" => $"Research towers\n• Progress per production: L1 {state?.Rules.Research.TowerLevelOneProgress ?? 1} / L2 {state?.Rules.Research.TowerLevelTwoProgress ?? 2}\n• 3 progress → 1 point",
                _ => "Dormant possibility\n• Inactive slot"
            };
        TechnologyDefinition? node = state?.TechnologyCatalog.FirstOrDefault(n => n.Id == slot.Technology);
        if (node is null) return "Waiting for authoritative research catalog.";
        string benefit = node.Production is { } production
            ? $"+{production.ExtraPerProducer} {production.Resource.ToString().ToLowerInvariant()} per building turn / {AssetCatalog.BuildingName(production.Building)}"
            : ProgressionPresentation.CapabilityText(node.Benefit) + " · " + string.Join(", ", node.EligibleTypes.Select(type => AssetCatalog.UnitName(type)));
        string choice = node.Tier == 2 ? "\n• Locks the other specialization" : "";
        var access = ProgressionPresentation.ResearchAccess(state, city, player, connected, slot.Technology);
        string reason = access.Reason == "Available during building/preparation" ? "Building/preparation only" : access.Reason;
        foreach (WorldtreeSlot known in Slots.Where(s => s.Technology != TechnologyId.None))
            reason = reason.Replace(TechnologyIds.Name(known.Technology), Title(known.VisualId), StringComparison.Ordinal);
        string denial = access.Available ? "" : "\n• " + reason;
        return $"{Title(id)}\n• {node.Cost} points\n• {benefit}{choice}{denial}";
    }
}
