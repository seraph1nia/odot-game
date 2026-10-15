using Game.Core;
using Godot;

namespace Game;

// A minimal production receiver: six authoritative IDs, labels, footprint and
// existing tower artwork. It can be hosted without starting the game/world.
internal sealed partial class ArmyPlotView : Node3D
{
    internal Aabb?[] Bounds { get; } = new Aabb?[ArmyConfiguration.PlotCount];
    internal static Vector3 PlotPosition(MatchSnapshot state, int id, int owner = 0)
    {
        int index = id - ArmyConfiguration.PlotOffset;
        if (index is < 0 or >= ArmyConfiguration.PlotCount) throw new ArgumentOutOfRangeException(nameof(id));
        ArenaSector sector = owner == 0 ? state.Arena!.Sectors[0] : state.Arena!.Sectors.Single(s => s.Owner == owner);
        System.Numerics.Vector3 point = SharedCombatProjection.CellPosition(state.Arena!.Board, sector.DeploymentCells[index]);
        return CombatLayout.ArenaOrigin + new Vector3(point.X, point.Y, point.Z) - CombatLayout.DistrictOrigin(state);
    }
    internal static string Title(ArmyHomeState home) => $"Army {home.Number} · " + (home.State switch
    {
        ArmyPlotStatus.Locked => "locked · purchase",
        ArmyPlotStatus.OwnedEmpty => "owned · empty",
        ArmyPlotStatus.UnitOccupied => $"squad · {home.Used} members",
        _ => $"{AssetCatalog.BuildingName(home.Plot.Type)} · whole squad home"
    });
    internal ArmyPlotView(CityState city, MatchSnapshot state, LandscapeAssets assets)
    {
        Name = "ArmyHomes";
        foreach (ArmyHomeState home in city.Army?.Homes ?? [])
        {
            int index = home.PlotId - ArmyConfiguration.PlotOffset;
            Vector3 point = PlotPosition(state, home.PlotId, city.Id);
            if (!ArmyConfiguration.IsTower(home.Plot.Type) && home.State != ArmyPlotStatus.UnitOccupied) AddChild(new Label3D
            {
                Name = "Home" + index,
                Position = point + new Vector3(0, .12f, .95f),
                Text = Title(home),
                FontSize = 18,
                PixelSize = .018f,
                Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
                Modulate = home.Purchased ? new Color("c7e6c5") : new Color("b9b5aa"),
                OutlineSize = 6
            });
            // Footprints are real production geometry, not a capacity-only HUD.
            var footprint = new MeshInstance3D
            {
                Name = "Footprint" + index,
                Position = point + new Vector3(0, .015f, 0),
                Mesh = new CylinderMesh { TopRadius = VillageLayout.Radius * .92f, BottomRadius = VillageLayout.Radius * .92f, Height = .02f, RadialSegments = 6 },
                MaterialOverride = new StandardMaterial3D { AlbedoColor = home.Purchased ? new Color("527756") : new Color("777367"), Roughness = 1 }
            };
            AddChild(footprint);
            if (!home.Purchased) AddChild(new Sprite3D { Name = "LockedHome" + index, Position = point + new Vector3(0, .35f, 0), Texture = UiAssets.Icon("Gold"), PixelSize = .014f, Billboard = BaseMaterial3D.BillboardModeEnum.Enabled, Shaded = false });
            // Physical weapon art belongs to SharedArenaView in preparation too;
            // retain these bounds for ordinary building-panel roof selection.
            if (!ArmyConfiguration.IsTower(home.Plot.Type)) continue;
            BuildingVisual visual = AssetCatalog.Building(home.Plot.Type);
            Node3D model = assets.Place(this, visual.Path, point, 1.9f); model.Name = "Tower" + home.PlotId;
            Aabb bounds = LandscapeAssets.Bounds(model);
            if (home.Plot.Level >= 2)
            {
                Node3D addition = assets.Place(this, visual.Upgrade, point, 1.9f); addition.Name = "Upgrade" + home.PlotId;
                model.Position = new(model.Position.X, LandscapeAssets.TowerDeck(addition), model.Position.Z);
                bounds = LandscapeAssets.Bounds(model).Merge(LandscapeAssets.Bounds(addition));
            }
            Bounds[index] = bounds;
        }
    }
}
