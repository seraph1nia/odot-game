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
    private Dictionary<int, UnitState[]> _summaryMembers = [];
    private readonly Dictionary<int, Label3D> _homes = [];
    private readonly Dictionary<int, StandardMaterial3D> _ownerMaterials = [];
    private readonly StandardMaterial3D _ground = new() { AlbedoColor = new("455741"), Roughness = 1 };
    private readonly StandardMaterial3D _available = new() { AlbedoColor = new("b7cc91"), ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded };
    private readonly StandardMaterial3D _occupied = new() { AlbedoColor = new("8d997b"), Roughness = 1 };
    private readonly StandardMaterial3D _selected = new() { AlbedoColor = new("f2dfac"), ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded };
    private readonly StandardMaterial3D _locked = new() { AlbedoColor = new("665f55"), Roughness = 1 };
    private MeshInstance3D? _diorama;
    private MatchSnapshot? _state;
    private CombatLayout? _layout;
    private string _identity = "";
    internal Aabb Content { get; private set; }
    internal Aabb OpaqueBounds => _diorama is { Mesh: { } mesh } plate ? plate.GlobalTransform * mesh.GetAabb() : default;
    internal static Color OwnerColor(int owner) => new(owner switch { 1 => "6da9e3", 2 => "ddb467", 3 => "9ccd83", 4 => "bb92d3", _ => "cd7067" });
    internal void Synchronize(MatchSnapshot state, CombatLayout layout, bool deployment, int localOwner, int selectedSquad = 0)
    {
        if (state.Arena is not { } arena) return;
        _state = state; _layout = layout;
        Position = CombatLayout.ArenaOrigin;
        string identity = state.MatchId + ":" + arena.Board.Cells.Length;
        if (_identity != identity)
        {
            foreach (Node child in GetChildren()) { RemoveChild(child); child.QueueFree(); }
            _cells.Clear(); _structures.Clear(); _squads.Clear(); _summaryMembers.Clear(); _homes.Clear(); _identity = identity;
            Mesh mesh = CellOutline();
            foreach (HexCellDefinition cell in arena.Board.Cells)
            {
                var tile = new MeshInstance3D { Name = "ArenaCell" + cell.Id, Position = layout.CellPosition(cell.Id) + new Vector3(0, .015f, 0), Mesh = mesh, MaterialOverride = _ground, CastShadow = GeometryInstance3D.ShadowCastingSetting.Off };
                AddChild(tile); _cells[cell.Id] = tile;
            }
            Vector3 min = _cells.Values.Select(c => c.Position).Aggregate((a, b) => a.Min(b)) - new Vector3(VillageLayout.Radius, 1, VillageLayout.Radius);
            Vector3 max = _cells.Values.Select(c => c.Position).Aggregate((a, b) => a.Max(b)) + new Vector3(VillageLayout.Radius, 5.2f, VillageLayout.Radius);
            Content = new(min, max - min);
            _diorama = new MeshInstance3D
            {
                Name = "ArenaDiorama",
                Position = Content.GetCenter() * new Vector3(1, 0, 1) - new Vector3(0, 1.02f, 0),
                Mesh = new BoxMesh { Size = new(Content.Size.X + 24, 1.8f, Content.Size.Z + 24) },
                MaterialOverride = new StandardMaterial3D { AlbedoColor = new("374735"), Roughness = 1 }
            }; AddChild(_diorama);
            BuildGround(arena, layout);
            DressBoundary();
            foreach (ArenaSector sector in arena.Sectors)
                foreach (int cell in sector.DeploymentCells)
                {
                    Label3D label = Label("DeploymentHome" + cell);
                    label.Position = layout.CellPosition(cell) + new Vector3(0, .18f, 0);
                    label.FontSize = 24; label.PixelSize = .024f; _homes[cell] = label;
                }
        }
        CityState? own = state.Players.FirstOrDefault(p => p.Id == localOwner);
        int[] homes = arena.Sectors.FirstOrDefault(s => s.Owner == localOwner)?.DeploymentCells ?? [];
        int[] purchased = own?.Army?.Homes.Where(h => h.Purchased && h.Plot.Type == Building.Empty).Select(h => homes[h.Cell]).ToArray() ?? [];
        int[] occupied = state.Squads.Where(s => s.Owner == localOwner && s.Members.Length > 0 && s.Hex.Lifecycle != UnitLifecycle.Stored).Select(s => s.DeploymentCell > 0 ? s.DeploymentCell : s.Hex.Position.Cell).ToArray();
        int selected = state.Squads.FirstOrDefault(s => s.Id == selectedSquad && s.Owner == localOwner)?.DeploymentCell ?? 0;
        foreach ((int id, MeshInstance3D tile) in _cells)
            tile.MaterialOverride = deployment && homes.Contains(id) ? id == selected ? _selected : purchased.Contains(id) ? occupied.Contains(id) ? _occupied : _available : _locked : _ground;
        foreach ((int cell, Label3D label) in _homes)
        {
            label.Visible = deployment && homes.Contains(cell);
            if (!label.Visible) continue;
            int home = Array.IndexOf(homes, cell);
            ArmyHomeState? assignment = own?.Army?.Homes.FirstOrDefault(h => h.Cell == home);
            string status = cell == selected ? "selected" : assignment?.Plot.Type != Building.Empty ? "tower" : !purchased.Contains(cell) ? "locked" : occupied.Contains(cell) ? "occupied" : "available";
            label.Text = $"P{localOwner} · {home + 1}\n{status}";
            label.Modulate = cell == selected ? _selected.AlbedoColor : OwnerColor(localOwner);
        }
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
            // Retain the owner seal and a low ruin marker after an objective
            // falls. Purchased inactive towers keep their actual building.
            view.Model.GetChild<Node3D>(0).Visible = !structure.Destroyed;
            MeshInstance3D seal = view.Model.GetNode<MeshInstance3D>("OwnerSeal");
            seal.Scale = new(1, structure.Destroyed ? 8 : 1, 1);
            bool objective = structure.Kind is ArenaStructureKind.Keep or ArenaStructureKind.Watchpost;
            view.Label.Text = $"P{structure.Owner} {(objective ? structure.Kind.ToString() : AssetCatalog.BuildingName(structure.TowerType))}" + (objective ? $" · {HealthPoints.Format(structure.Health)}/{HealthPoints.Format(structure.MaximumHealth)}" : $" L{structure.Level}")
                + (structure.Destroyed ? " · fallen" : !structure.Active ? " · inactive" : "");
        }
    }
    private static ImmediateMesh CellOutline()
    {
        var mesh = new ImmediateMesh(); mesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);
        Vector3 Point(int corner, float radius) => new(Mathf.Cos(Mathf.Pi / 6 + corner * Mathf.Pi / 3) * radius, 0, Mathf.Sin(Mathf.Pi / 6 + corner * Mathf.Pi / 3) * radius);
        for (int edge = 0; edge < 6; edge++)
        {
            Vector3 a = Point(edge, VillageLayout.Radius * .99f), b = Point(edge + 1, VillageLayout.Radius * .99f);
            Vector3 c = Point(edge, VillageLayout.Radius * .96f), d = Point(edge + 1, VillageLayout.Radius * .96f);
            foreach (Vector3 vertex in new[] { a, c, b, b, c, d }) { mesh.SurfaceSetNormal(Vector3.Up); mesh.SurfaceAddVertex(vertex); }
        }
        mesh.SurfaceEnd(); return mesh;
    }
    private void BuildGround(ArenaState arena, CombatLayout layout)
    {
        // Original imported floor buffers/materials, batched by surface family.
        // Only deterministic geometry IDs vary the cosmetics; authority RNG
        // and cell/contact coordinates are untouched.
        string Family(HexCellDefinition cell) => ((cell.Coordinate.Column + cell.Coordinate.R * 3) % 11) switch
        { 0 or 1 => DetailedGround.Moss, 5 => DetailedGround.Dirt, _ => DetailedGround.Grass };
        foreach (var group in arena.Board.Cells.GroupBy(Family))
        {
            HexCellDefinition[] cells = group.ToArray();
            var batch = new MultiMeshInstance3D
            {
                Name = "ArenaGround" + System.IO.Path.GetFileNameWithoutExtension(group.Key),
                Multimesh = new MultiMesh { TransformFormat = MultiMesh.TransformFormatEnum.Transform3D, Mesh = _assets.Terrain(group.Key), InstanceCount = cells.Length },
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
            };
            batch.SetMeta("asset", group.Key); AddChild(batch);
            for (int i = 0; i < cells.Length; i++)
                batch.Multimesh.SetInstanceTransform(i, new(new Basis(Vector3.Up, Mathf.Pi / 2 + cells[i].Id % 3 * Mathf.Pi / 3).Scaled(Vector3.One * VillageLayout.TerrainScale), layout.CellPosition(cells[i].Id)));
        }
    }
    private void DressBoundary()
    {
        var frame = new Node3D { Name = "ArenaWoodland" }; AddChild(frame);
        Rect2 ground = BattlefieldFrame.GroundRect(Content);
        // Canopy only along the far edge: it cannot hide active cells from the
        // fixed camera angle. Low side clusters finish the open fighting field.
        int count = (int)Math.Ceiling(ground.Size.X / 8);
        for (int i = 0; i <= count; i++)
        {
            float x = Mathf.Lerp(ground.Position.X, ground.End.X, (float)i / count);
            Node3D tree = _assets.Place(frame, "environment/components/forest_spiral_tree.glb", new(x, -.1f, ground.Position.Y - 3.5f), 2.4f + i % 3 * .25f);
            tree.RotationDegrees = new(0, i * 71 % 360, 0);
            _assets.Place(frame, "environment/components/forest_boulder.glb", new(x + 1.6f, -.1f, ground.Position.Y - 2.8f), .85f);
        }
        for (int side = 0; side < 2; side++)
            for (int i = 0; i < 5; i++)
            {
                float x = side == 0 ? ground.Position.X - 2.4f : ground.End.X + 2.4f;
                float z = Mathf.Lerp(ground.Position.Y, ground.End.Y, i / 4f);
                _assets.Place(frame, "environment/components/forest_boulder.glb", new(x, -.1f, z), .7f);
                _assets.Place(frame, "environment/components/forest_fern.glb", new(x, -.1f, z + .8f), .5f);
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
            PixelSize = .024f,
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
            OutlineModulate = new("233a3f"),
            OutlineSize = 4,
            NoDepthTest = false
        }; AddChild(label); return label;
    }
    internal void Sample(double tick, IEnumerable<UnitState> units)
    {
        if (_state is null || _layout is null) return;
        _summaryMembers = units.Where(u => u.Hex?.Lifecycle == UnitLifecycle.Alive).GroupBy(u => u.SquadId).ToDictionary(g => g.Key, g => g.ToArray());
        foreach (int id in _squads.Keys.Except(_summaryMembers.Keys).ToArray()) { _squads[id].QueueFree(); _squads.Remove(id); }
        foreach ((int id, UnitState[] participating) in _summaryMembers)
        {
            UnitState squad = participating[0];
            if (!_squads.TryGetValue(id, out Label3D? label)) _squads[id] = label = Label("SquadSummary" + id);
            label.Position = _layout.SquadPosition(squad.Hex!, tick) - Position + new Vector3(0, squad.IsBoss ? 2.5f : 1.6f, 0);
            label.Modulate = OwnerColor(squad.Owner);
            string status = string.Join(' ', participating.Select(u => ProgressionPresentation.StatusBadge(u.Statuses, tick)).Where(s => s.Length > 0).Distinct());
            string role = squad.Type switch { UnitType.Swordsman => "Sword", UnitType.Berserker => "Axe", UnitType.Crossbowman => "Bow", _ => "Mage" };
            label.Text = $"{(squad.Faction == Faction.Skeletons ? squad.IsBoss ? "BOSS" : "Enemy" : $"P{squad.Owner}")} {role} ×{participating.Length}\n"
                + string.Join(' ', participating.Select(u => $"{HealthPoints.Format(u.Health)}/{HealthPoints.Format(u.Profile.Health)}"))
                + (status.Length > 0 ? "\n" + status : "") + (tick < (_state.Squads.FirstOrDefault(s => s.Id == id)?.RallyDisruptedUntil ?? 0) ? "\nRally disrupted" : "");
        }
    }
    internal object[] Observe(Camera3D camera, Transform2D screen, Rect2 world)
    {
        if (_state is null) return [];
        return _summaryMembers.Select(group =>
        {
            UnitState s = group.Value[0];
            Label3D label = _squads[group.Key];
            Vector2 point = screen * camera.UnprojectPosition(label.GlobalPosition);
            return (object)new
            {
                Id = group.Key,
                s.Owner,
                s.Faction,
                s.Type,
                label.Text,
                Visible = label.IsVisibleInTree() && !camera.IsPositionBehind(label.GlobalPosition) && world.HasPoint(point),
                X = point.X,
                Y = point.Y,
                Members = group.Value.Select(member => new { member.Id, member.Health, Maximum = member.Profile.Health }).ToArray()
            };
        }).ToArray();
    }
    internal object ObserveFrame(Camera3D camera)
    {
        MultiMeshInstance3D[] ground = GetChildren().OfType<MultiMeshInstance3D>().ToArray();
        Node3D? woodland = GetNodeOrNull<Node3D>("ArenaWoodland");
        Rect2 playable = BattlefieldFrame.GroundRect(Content);
        return new
        {
            Cells = ground.Sum(g => g.Multimesh.InstanceCount),
            Ground = ground.Select(g => g.GetMeta("asset").AsString()).Order().ToArray(),
            Props = woodland?.GetChildCount() ?? 0,
            SceneryClear = woodland is not null && woodland.GetChildren().OfType<Node3D>().All(p => !playable.HasPoint(new(p.Position.X, p.Position.Z))),
            OpaqueCoverage = BattlefieldFrame.GroundRect(OpaqueBounds).Encloses(BattlefieldFrame.Footprint(camera, GetViewport().GetVisibleRect()))
        };
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
