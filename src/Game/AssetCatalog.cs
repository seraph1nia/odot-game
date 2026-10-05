using Game.Core;

namespace Game;

public sealed record BuildingVisual(string Name, string Path, string Upgrade, bool RaisedUpgrade = false);
public sealed record UnitVisual(string Name, string Path, string MainProp, string OffhandProp, string MainSocket, string OffhandSocket, string StrikeSocket);

// Presentation identities only. Canonical enums, requests, saves and numerical
// profiles are not renamed to fit an imported silhouette.
public static class AssetCatalog
{
    public const string Root = "res://Assets/Authored/";
    public const string Revision = "a640d721065233dfbf488747694cd95110d4a907";
    public const string Permission = "the repo is my own work, so yes, use it";
    public const string Meadow = "environment/components/forest_hex_meadow.glb";
    public const string Stream = "environment/components/forest_hex_stream.glb";
    public const string Bridge = "environment/hex_woodland_bridge.glb";
    public const string Home = "buildings/tree_house.glb";
    public const string Defender = "buildings/arrow_tower.glb";
    public const string Arrow = "props/arrow.glb";
    public const double AttackKeyStart = 1 / 24.0;
    public const double AttackKeyEnd = 25 / 24.0;
    public const double AttackMarker = 13 / 24.0;
    public const string Plinth = "environment/components/forest_shrine_plinth.glb";
    public const string Timber = "environment/components/forest_bridge_plank.glb";
    public const string GoldCargo = "props/kit_crate.glb";
    public const string Provisions = "props/kit_open_barrel.glb";

    private static readonly IReadOnlyDictionary<Building, BuildingVisual> Buildings = new Dictionary<Building, BuildingVisual>
    {
        [Game.Core.Building.Farm] = new("Bakery", "buildings/bakery.glb", "props/kit_parcel.glb"),
        [Game.Core.Building.Lumbermill] = new("Woodcutter hut", "buildings/woodcutter_hut.glb", Timber),
        [Game.Core.Building.Mine] = new("Gold mine", "buildings/gold_mine.glb", GoldCargo),
        [Game.Core.Building.MetalMine] = new("Metal mine", "buildings/metal_mine.glb", GoldCargo),
        [Game.Core.Building.Stonecutter] = new("Stonecutter", "buildings/stone_cutter.glb", "props/kit_crate.glb"),
        [Game.Core.Building.Weaver] = new("Weaver", "buildings/weaver.glb", "props/kit_parcel.glb"),
        [Game.Core.Building.Barracks] = new("Barracks", "buildings/barracks.glb", "props/kit_spear.glb"),
        [Game.Core.Building.ArcheryRange] = new("Archery range", "buildings/archery_range.glb", "props/kit_target.glb"),
        [Game.Core.Building.Arcanum] = new("Magic academy", "buildings/magic_academy.glb", "props/kit_book.glb"),
        [Game.Core.Building.ResearchTower] = new("Research tower", "buildings/research_tower.glb", Plinth, true),
        [Game.Core.Building.ArrowTower] = new("Arrow tower", "buildings/arrow_tower.glb", Plinth, true),
        [Game.Core.Building.CatapultTower] = new("Bombard tower", "buildings/bombarding_tower.glb", Plinth, true),
        [Game.Core.Building.Market] = new("Market", "buildings/trade_market.glb", GoldCargo),
        [Game.Core.Building.TownHall] = new("Town hall", "buildings/town_hall.glb", "props/kit_book.glb")
    };
    private static readonly IReadOnlyDictionary<(Faction, UnitType), UnitVisual> Units = new Dictionary<(Faction, UnitType), UnitVisual>
    {
        [(Faction.Adventurers, UnitType.Swordsman)] = new("Knight", "characters/knight.glb", "sword", "shield", "weapon_socket.R", "weapon_socket.L", "weapon_socket.R"),
        [(Faction.Adventurers, UnitType.Berserker)] = new("Berserker", "characters/berserker.glb", "axe", "axe_offhand", "weapon_socket.R", "weapon_socket.L", "weapon_socket.R"),
        [(Faction.Adventurers, UnitType.Crossbowman)] = new("Archer", "characters/archer.glb", "bow", "arrow", "weapon_socket.L", "weapon_socket.R", "weapon_socket.L"),
        [(Faction.Adventurers, UnitType.Mage)] = new("Mage", "characters/mage.glb", "staff", "", "weapon_socket.L", "", "weapon_socket.R"),
        [(Faction.Skeletons, UnitType.Swordsman)] = new("Boneguard", "characters/evil_melee_unit.glb", "evil_sword", "evil_shield", "weapon_socket.R", "weapon_socket.L", "weapon_socket.R"),
        [(Faction.Skeletons, UnitType.Berserker)] = new("Horned berserker", "characters/evil_berserker_unit.glb", "evil_axe", "evil_axe_offhand", "weapon_socket.R", "weapon_socket.L", "weapon_socket.R"),
        [(Faction.Skeletons, UnitType.Crossbowman)] = new("Hooded crossbowman", "characters/evil_ranged_unit.glb", "crossbow", "", "weapon_socket.L", "", "weapon_socket.L"),
        [(Faction.Skeletons, UnitType.Mage)] = new("Horned mage", "characters/evil_mage_unit.glb", "evil_staff", "evil_hand_flame", "weapon_socket.L", "weapon_socket.R", "weapon_socket.R")
    };
    public static BuildingVisual Building(Building type) => Buildings.TryGetValue(type, out BuildingVisual? visual)
        ? visual : throw new ArgumentOutOfRangeException(nameof(type), type, "No authored building mapping.");
    public static UnitVisual Unit(UnitType type, Faction faction) => Units.TryGetValue((faction, type), out UnitVisual? visual)
        ? visual : throw new ArgumentOutOfRangeException(nameof(type), type, "No authored faction/role mapping.");
    public static string BuildingName(Building type) => type == Game.Core.Building.Empty ? "Empty plot" : Building(type).Name;
    public static string UnitName(UnitType type, Faction faction = Faction.Adventurers) => Unit(type, faction).Name;
    public static string UnitDescription(UnitType type, Faction faction) => type switch
    {
        UnitType.Berserker => "A heavy melee fighter armed with axes.",
        UnitType.Crossbowman => faction == Faction.Adventurers ? "A ranged fighter armed with a bow." : "A ranged fighter armed with a crossbow.",
        UnitType.Mage => "A spellcaster who attacks with magic.",
        UnitType.Swordsman => "A close combat fighter armed with a sword.",
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
    // Map an authored pose onto the authority's unchanged committed interval.
    public static double AttackPose(UnitState unit, double tick, double clipLength)
    {
        double pose = tick <= unit.ImpactTick ? AttackMarker * (tick - unit.ActionStartTick) / Math.Max(1, unit.ImpactTick - unit.ActionStartTick)
            : AttackMarker + (clipLength - AttackMarker) * (tick - unit.ImpactTick) / Math.Max(1, unit.ReadyTick - unit.ImpactTick);
        return Math.Clamp(pose, 0, clipLength);
    }
    // Exported PCKs retain .import mappings instead of the original GLB bytes.
    // Project only one supported mapping suffix; do not admit unknown bundles.
    public static string[] InstalledModels(IEnumerable<string> files)
    {
        string[] installed = files.Select(path => path.EndsWith(".remap", StringComparison.Ordinal) ? path[..^6]
                : path.EndsWith(".import", StringComparison.Ordinal) ? path[..^7] : path)
            .Where(path => Path.GetExtension(path) is ".glb" or ".gltf" or ".fbx" or ".obj" or ".blend" or ".dae" or ".bin")
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        string[] required = RequiredPaths.Select(path => Root + path).ToArray();
        if (!installed.SequenceEqual(required, StringComparer.Ordinal))
            throw new InvalidDataException("Graphical distribution has missing, legacy or unregistered 3D bundles. Missing: "
                + string.Join(", ", required.Except(installed, StringComparer.Ordinal)) + "; unexpected: "
                + string.Join(", ", installed.Except(required, StringComparer.Ordinal)));
        return installed;
    }

    public static IEnumerable<string> RequiredPaths => Buildings.Values.SelectMany(b => new[] { b.Path, b.Upgrade })
        .Concat(Units.Values.Select(u => u.Path))
        .Concat(new[] { Meadow, Stream, Bridge, Home, Defender, Arrow, Provisions,
            "props/kit_barrel.glb", "props/kit_pine.glb",
            "environment/components/forest_boulder.glb", "environment/components/forest_spiral_tree.glb",
            "environment/components/forest_fern.glb", "environment/components/forest_leaf_clump.glb",
            "environment/components/forest_moss.glb", "environment/components/forest_mushroom_small.glb",
            "environment/components/forest_lantern_post.glb" })
        .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal);
}
