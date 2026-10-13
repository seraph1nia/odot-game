using Game.Core;
using Godot;

namespace Game;

// Native battlefield component: one arena, physical objectives, compact squad
// summaries and owned deployment picking. Cosmetics consume accepted snapshots.
internal sealed partial class SharedArenaView : Node3D
{
    private readonly LandscapeAssets _assets = new();
    private readonly Dictionary<int, MeshInstance3D> _cells = [];
    private readonly Dictionary<(int Owner, ArenaStructureKind Kind, int Slot), (Node3D Model, Label3D Label, int Level)> _structures = [];
    private readonly Dictionary<int, Label3D> _squads = [];
    private readonly Dictionary<int, StandardMaterial3D> _ownerMaterials = [];
    private readonly StandardMaterial3D _ground = new() { AlbedoColor = new("536543"), Roughness = 1 };
    private readonly StandardMaterial3D _available = new() { AlbedoColor = new("a7bc75"), Roughness = 1 };
    private readonly StandardMaterial3D _occupied = new() { AlbedoColor = new("8d997b"), Roughness = 1 };
    private MeshInstance3D? _diorama;
    private MatchSnapshot? _state;
    private CombatLayout? _layout;
    private string _identity = "";
    internal Aabb Content { get; private set; }
    internal Aabb OpaqueBounds => _diorama is { Mesh: { } mesh } plate ? plate.GlobalTransform * mesh.GetAabb() : default;
    internal static Color OwnerColor(int owner) => new(owner switch { 1 => "6da9e3", 2 => "ddb467", 3 => "9ccd83", 4 => "bb92d3", _ => "cd7067" });
    internal void Synchronize(MatchSnapshot state, CombatLayout layout, bool deployment, int localOwner)
    {
        if (state.Arena is not { } arena) return;
        _state = state; _layout = layout;
        Position = CombatLayout.ArenaOrigin;
        string identity = state.MatchId + ":" + arena.Board.Cells.Length;
        if (_identity != identity)
        {
            foreach (Node child in GetChildren()) { RemoveChild(child); child.QueueFree(); }
            _cells.Clear(); _structures.Clear(); _squads.Clear(); _identity = identity;
            var mesh = new CylinderMesh { TopRadius = VillageLayout.Radius * .98f, BottomRadius = VillageLayout.Radius * .98f, Height = .14f, RadialSegments = 6 };
            foreach (HexCellDefinition cell in arena.Board.Cells)
            {
                var tile = new MeshInstance3D { Name = "ArenaCell" + cell.Id, Position = layout.CellPosition(cell.Id) - new Vector3(0, .08f, 0), Mesh = mesh, MaterialOverride = _ground };
                tile.RotationDegrees = new(0, 30, 0); AddChild(tile); _cells[cell.Id] = tile;
            }
            Vector3 min = _cells.Values.Select(c => c.Position).Aggregate((a, b) => a.Min(b)) - new Vector3(VillageLayout.Radius, 1, VillageLayout.Radius);
            Vector3 max = _cells.Values.Select(c => c.Position).Aggregate((a, b) => a.Max(b)) + new Vector3(VillageLayout.Radius, 5.2f, VillageLayout.Radius);
            Content = new(min, max - min);
            _diorama = new MeshInstance3D
            {
                Name = "ArenaDiorama",
                Position = Content.GetCenter() * new Vector3(1, 0, 1) - new Vector3(0, 1.02f, 0),
                Mesh = new BoxMesh { Size = new(Content.Size.X + 24, 1.8f, Content.Size.Z + 24) },
                MaterialOverride = new StandardMaterial3D { AlbedoColor = new("253326"), Roughness = 1 }
            }; AddChild(_diorama);
        }
        CityState? own = state.Players.FirstOrDefault(p => p.Id == localOwner);
        int[] homes = arena.Sectors.FirstOrDefault(s => s.Owner == localOwner)?.DeploymentCells ?? [];
        int[] purchased = own?.Army?.Homes.Where(h => h.Purchased && h.Plot.Type == Building.Empty).Select(h => homes[h.Cell]).ToArray() ?? [];
        int[] occupied = state.Squads.Where(s => s.Owner == localOwner && s.Members.Length > 0 && s.Hex.Lifecycle != UnitLifecycle.Stored).Select(s => s.DeploymentCell > 0 ? s.DeploymentCell : s.Hex.Position.Cell).ToArray();
        foreach ((int id, MeshInstance3D tile) in _cells)
            tile.MaterialOverride = deployment && purchased.Contains(id) ? occupied.Contains(id) ? _occupied : _available : _ground;
        var liveStructures = state.Structures.ToDictionary(s => (s.Owner, s.Kind, s.Slot));
        foreach (var key in _structures.Keys.Except(liveStructures.Keys).ToArray())
        { RemoveStructure(_structures[key].Model, _structures[key].Label); _structures.Remove(key); }
        foreach (StructureState structure in state.Structures)
        {
            var key = (structure.Owner, structure.Kind, structure.Slot);
            if (_structures.TryGetValue(key, out var view) && view.Level != structure.Level)
            { RemoveStructure(view.Model, view.Label); _structures.Remove(key); }
            if (!_structures.ContainsKey(key))
            {
                string path = structure.Kind switch
                {
                    ArenaStructureKind.Keep => AssetCatalog.Home,
                    ArenaStructureKind.Watchpost => AssetCatalog.Defender,
                    ArenaStructureKind.CatapultTower => AssetCatalog.Building(Building.CatapultTower).Path,
                    _ => AssetCatalog.Building(Building.ArrowTower).Path
                };
                var footprint = new Node3D { Name = $"{structure.Kind}_{structure.Owner}_{structure.Slot}", Position = layout.CellPosition(structure.Cell) }; AddChild(footprint);
                Node3D model = _assets.Place(footprint, path, Vector3.Zero, structure.Kind == ArenaStructureKind.Keep ? 2.4f : 1.8f);
                if (structure.Level >= 2 && structure.Slot >= 0)
                {
                    Node3D plinth = _assets.Place(footprint, AssetCatalog.Plinth, Vector3.Zero, 1.9f);
                    model.Position += new Vector3(0, LandscapeAssets.TowerDeck(plinth), 0);
                }
                var label = Label($"StructureHealth_{structure.Owner}_{structure.Kind}_{structure.Slot}");
                label.Position = layout.CellPosition(structure.Cell) + new Vector3(0, 3, 0); label.Modulate = OwnerColor(structure.Owner);
                view = (footprint, label, structure.Level); _structures.Add(key, view);
                footprint.AddChild(new MeshInstance3D
                {
                    Name = "OwnerSeal",
                    Position = new(0, .02f, 0),
                    Mesh = new CylinderMesh { TopRadius = 1.2f, BottomRadius = 1.2f, Height = .02f, RadialSegments = 6 },
                    MaterialOverride = OwnerMaterial(structure.Owner)
                });
            }
            view.Model.Visible = !structure.Destroyed;
            bool objective = structure.Kind is ArenaStructureKind.Keep or ArenaStructureKind.Watchpost;
            view.Label.Text = $"P{structure.Owner} {(objective ? structure.Kind.ToString() : AssetCatalog.BuildingName(structure.TowerType))}" + (objective ? $" · {HealthPoints.Format(structure.Health)}/{HealthPoints.Format(structure.MaximumHealth)}" : $" L{structure.Level}")
                + (structure.Destroyed ? " · fallen" : !structure.Active ? " · inactive" : "");
        }
    }
    private void RemoveStructure(Node3D model, Label3D label)
    { RemoveChild(model); RemoveChild(label); model.QueueFree(); label.QueueFree(); }
    internal void CoverBoundary(Rect2 worldBoundary)
    {
        if (_diorama?.Mesh is not BoxMesh mesh) return;
        // Match the native projected safe footprint, including HUD/portrait
        // compensation; a fixed village-sized base would expose empty world.
        Vector2 center = worldBoundary.GetCenter() - new Vector2(Position.X, Position.Z);
        _diorama.Position = new(center.X, -1.02f, center.Y);
        mesh.Size = new(worldBoundary.Size.X + 2, 1.8f, worldBoundary.Size.Y + 2);
    }
    private StandardMaterial3D OwnerMaterial(int owner)
    {
        if (!_ownerMaterials.TryGetValue(owner, out StandardMaterial3D? material))
            _ownerMaterials[owner] = material = new() { AlbedoColor = OwnerColor(owner), ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded };
        return material;
    }
    private Label3D Label(string name)
    {
        var label = new Label3D
        {
            Name = name,
            FontSize = 30,
            PixelSize = .012f,
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
            OutlineModulate = new("233a3f"),
            OutlineSize = 4,
            NoDepthTest = false
        }; AddChild(label); return label;
    }
    internal void Sample(double tick)
    {
        if (_state is null || _layout is null) return;
        var members = _state.Players.SelectMany(p => p.Soldiers).Concat(_state.Enemies).ToDictionary(u => u.Id);
        SquadState[] alive = _state.Squads.Where(s => s.Hex.Lifecycle == UnitLifecycle.Alive && s.ParticipatingMembers.Length > 0).ToArray();
        foreach (int id in _squads.Keys.Except(alive.Select(s => s.Id)).ToArray()) { _squads[id].QueueFree(); _squads.Remove(id); }
        foreach (SquadState squad in alive)
        {
            if (!_squads.TryGetValue(squad.Id, out Label3D? label)) _squads[squad.Id] = label = Label("SquadSummary" + squad.Id);
            UnitState[] participating = squad.ParticipatingMembers.Where(members.ContainsKey).Select(id => members[id]).ToArray();
            label.Position = _layout.SquadPosition(squad, tick) - Position + new Vector3(0, squad.IsBoss ? 2.5f : 1.6f, 0);
            label.Modulate = OwnerColor(squad.Owner);
            string status = string.Join(' ', participating.Select(u => ProgressionPresentation.StatusBadge(u.Statuses, tick)).Where(s => s.Length > 0).Distinct());
            label.Text = $"{(squad.Faction == Faction.Skeletons ? squad.IsBoss ? "BOSS" : "Enemy" : $"P{squad.Owner}")} {AssetCatalog.UnitName(squad.Type, squad.Faction)} ×{participating.Length}\n"
                + string.Join(' ', participating.Select(u => $"{HealthPoints.Format(u.Health)}/{HealthPoints.Format(u.Profile.Health)}"))
                + (status.Length > 0 ? "\n" + status : "") + (tick < squad.RallyDisruptedUntil ? "\nRally disrupted" : "");
        }
    }
    internal int PickCell(Vector3 origin, Vector3 direction)
    {
        if (_layout is null || _state?.Arena is null || Math.Abs(direction.Y) < .001f) return 0;
        float distance = (Position.Y - origin.Y) / direction.Y;
        if (distance < 0) return 0;
        Vector3 point = origin + direction * distance - Position;
        return _state.Arena.Board.Cells.Select(c => (c.Id, Delta: point - _layout.CellPosition(c.Id)))
            .Where(c => Math.Abs(c.Delta.X) <= VillageLayout.HalfWidth && Math.Abs(c.Delta.Z) <= VillageLayout.Radius - Math.Abs(c.Delta.X) / Mathf.Sqrt(3))
            .OrderBy(c => c.Delta.LengthSquared()).Select(c => c.Id).FirstOrDefault();
    }
    internal Vector3? WeaponOrigin(TowerState tower)
    {
        ArenaStructureKind kind = tower.Slot < 0 ? ArenaStructureKind.Watchpost : tower.Type is Building.CatapultTower or Building.BombardTower ? ArenaStructureKind.CatapultTower : ArenaStructureKind.ArrowTower;
        if (!_structures.TryGetValue((tower.City, kind, tower.Slot), out var view)) return null;
        Aabb bounds = view.Model.Transform * LandscapeAssets.Bounds(view.Model.GetChild<Node3D>(0));
        return Position + new Vector3(bounds.GetCenter().X, bounds.End.Y - .1f, bounds.GetCenter().Z);
    }
}
