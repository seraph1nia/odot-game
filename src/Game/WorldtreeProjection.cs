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
                "root" => $"Established village · not a purchase. Base income: {state?.Rules.BaseGold ?? 2} gold per production. Research belongs to each city.",
                "trunk_1" => "Established shared knowledge · not a purchase. Each shared wave clear grants every surviving city 1 research point. Choose one specialization per combat class; the other is permanently locked.",
                "trunk_2" => $"Established research system · not a purchase. Research towers supply progress each production (level 1: {state?.Rules.Research.TowerLevelOneProgress ?? 1}; level 2: {state?.Rules.Research.TowerLevelTwoProgress ?? 2}). Every 3 progress becomes 1 research point; carry is retained after sales.",
                _ => "Inactive slot · no implemented research, cost or benefit. The canonical artwork is retained; this node cannot be purchased."
            };
        TechnologyDefinition? node = state?.TechnologyCatalog.FirstOrDefault(n => n.Id == slot.Technology);
        if (node is null) return "Waiting for authoritative research catalog.";
        string benefit = node.Production is { } production
            ? $"Each {AssetCatalog.BuildingName(production.Building)} produces +{production.ExtraPerProducer} {production.Resource.ToString().ToLowerInvariant()} per building turn, at either level. No output without the producer; selling removes its output."
            : ProgressionPresentation.CapabilityText(node.Benefit) + ". Applies to existing and newly recruited " + string.Join(", ", node.EligibleTypes.Select(type => AssetCatalog.UnitName(type))) + "; no healing or level change.";
        string requirement = node.Prerequisite == TechnologyId.None ? "No prerequisite." : "Requires " + Title(Slots.Single(s => s.Technology == node.Prerequisite).VisualId) + ".";
        string choice = node.Tier == 2 ? " Choosing this specialization permanently locks the other specialization and its descendants for this class." : "";
        return $"{Title(id)} · {node.Cost} research points\n{benefit}\n{requirement}{choice}\n{ProgressionPresentation.ResearchAccess(state, city, player, connected, slot.Technology).Reason}";
    }
}
