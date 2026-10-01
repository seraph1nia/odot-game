using Godot;

namespace Game;

// Passive scenery shared by the menu and each city. No session or authority access.
internal sealed partial class VillageLandscape : Node3D
{
    internal static Vector3 Home => VillageLayout.Hex(0, 1);
    internal static Vector3 Defender => VillageLayout.Hex(-1, 0);
    private readonly LandscapeAssets _assets;
    private readonly Dictionary<(int Column, int Row), Node3D> _tiles = [];
    private readonly List<Node3D> _static = [];
    private readonly Dictionary<(string Kind, int X, int Z), MultiMeshInstance3D> _terrain = [];
    private readonly Dictionary<string, Mesh> _meshes = [];
    private Vector3[] _renderedCells = [];
    private string[] _coreTerrain = [];
    private readonly Node3D _bridge;
    private readonly Dictionary<string, Transform3D> _meshTransforms = [];
    private (int Left, int Right, int Back, int Front)? _range;

    internal VillageLandscape(Node3D parent, LandscapeAssets assets)
    {
        _assets = assets; Name = "Scenery"; parent.AddChild(this);
        _static.Add(assets.Place(this, "Medieval/building_home_A_blue.gltf", Home, 1.8f));
        _static.Add(assets.Place(this, "Medieval/building_tower_A_blue.gltf", Defender, 1.4f));
        foreach (var (name, column, row, size) in new[]
        {
            ("hills_A_trees", -3, -4, 2.6f), ("hills_A_trees", -3, -1, 2.6f),
            ("hills_A_trees", -3, 2, 2.6f), ("hills_A_trees", -3, 4, 2.6f),
            ("trees_A_small", 2, -4, 2.0f), ("trees_A_small", 2, -1, 2.0f), ("trees_A_small", -2, 5, 2.0f),
            ("rock_single_C", -2, -3, .6f), ("rock_single_C", 2, -3, .6f),
            ("rock_single_C", 2, 4, .6f), ("rock_single_C", -2, 1, .6f),
            ("hill_single_A", 2, 5, 1.6f), ("mountain_A", -3, 5, 2.7f), ("mountain_B", -3, -5, 2.7f),
            ("barrel", 1, 1, .45f), ("sack", 2, 0, .35f)
        }) _static.Add(assets.Place(this, $"Medieval/{name}.gltf", VillageLayout.Hex(column, row), size));
        // Bridge includes submerged legs; preserve its authored terrain-relative origin.
        _bridge = assets.Native(this, "Medieval/building_bridge_A.gltf", VillageLayout.Hex(3, 1) + new Vector3(0, .4f, 0), VillageLayout.TerrainScale, 90);
        for (int slot = 0; slot < 9; slot++) PlotOutline(slot);
    }
    private void PlotOutline(int slot)
    {
        var surface = new SurfaceTool(); surface.Begin(Mesh.PrimitiveType.Triangles);
        for (int edge = 0; edge < 6; edge++)
        {
            float a = Mathf.DegToRad(30 + edge * 60), b = a + Mathf.Pi / 3;
            Vector3 outerA = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * VillageLayout.Radius * .92f;
            Vector3 outerB = new Vector3(Mathf.Cos(b), 0, Mathf.Sin(b)) * VillageLayout.Radius * .92f;
            foreach (Vector3 v in new[] { outerA, outerB * .965f, outerB, outerA, outerA * .965f, outerB * .965f }) surface.AddVertex(v);
        }
        this.AddChild(new MeshInstance3D
        {
            Name = "Plot" + slot,
            Position = VillageLayout.Slot(slot) + new Vector3(0, .018f, 0),
            Mesh = surface.Commit(),
            MaterialOverride = new StandardMaterial3D { AlbedoColor = new("e0dbaf"), ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded, CullMode = BaseMaterial3D.CullModeEnum.Disabled },
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        });
    }
    internal static Vector3[] Footprint(Camera3D camera, Rect2 area, Vector3 center)
    {
        var points = new List<Vector3>();
        foreach (float y in new[] { -.4f, 0, 1.75f })
            foreach (Vector2 corner in new[] { area.Position, new Vector2(area.End.X, area.Position.Y), area.End, new Vector2(area.Position.X, area.End.Y) })
            {
                Vector3 origin = camera.ProjectRayOrigin(corner), ray = camera.ProjectRayNormal(corner);
                points.Add(origin + ray * ((y - origin.Y) / ray.Y) - center);
            }
        return points.ToArray();
    }
    internal void Cover(Camera3D camera, Rect2 area, Vector2 travel)
    {
        Vector3[] points = Footprint(camera, area, this.GlobalPosition);
        Vector3 min = points.Aggregate((a, b) => a.Min(b)), max = points.Aggregate((a, b) => a.Max(b));
        // Extra rings hide exterior walls and prop overhang. Double travel covers both extremes from any current pan.
        var range = (Left: Math.Min(-4, (int)Math.Floor((min.X - travel.X * 2) / 3) - 3),
            Right: Math.Max(4, (int)Math.Ceiling((max.X + travel.X * 2) / 3) + 3),
            Back: Math.Min(-6, (int)Math.Floor((min.Z - travel.Y * 2) / VillageLayout.RowStep) - 3),
            Front: Math.Max(6, (int)Math.Ceiling((max.Z + travel.Y * 2) / VillageLayout.RowStep) + 3));
        if (_range is { } previous) range = (Math.Min(range.Left, previous.Left), Math.Max(range.Right, previous.Right), Math.Min(range.Back, previous.Back), Math.Max(range.Front, previous.Front));
        if (_range == range) return;
        _range = range;
        foreach (var key in _tiles.Keys.Where(k => k.Column < range.Left || k.Column > range.Right || k.Row < range.Back || k.Row > range.Front).ToArray())
        { Node3D tile = _tiles[key]; this.RemoveChild(tile); tile.QueueFree(); _tiles.Remove(key); }
        for (int row = range.Back; row <= range.Front; row++) for (int column = range.Left; column <= range.Right; column++)
        {
            if (_tiles.ContainsKey((column, row))) continue;
            var cell = new Node3D { Position = VillageLayout.Hex(column, row) }; this.AddChild(cell); _tiles[(column, row)] = cell;
            // Fixed coordinate pattern; no tall props in the playable region or its foreground sightlines.
            if ((column <= -5 || row <= -7 && column >= 5) && Math.Abs(column * 17 + row * 31) % 7 == 0)
                _assets.Place(cell, "Medieval/trees_A_small.gltf", Vector3.Zero, 1.5f);
            else if ((column <= -5 || column >= 5 || row <= -7 || row >= 7) && Math.Abs(column * 13 + row * 23) % 19 == 0)
                _assets.Place(cell, "Medieval/rock_single_C.gltf", Vector3.Zero, .5f);
        }
        RebuildTerrain();
    }
    private void RebuildTerrain()
    {
        foreach (var group in _tiles.Keys.GroupBy(k => (Kind: k.Column == 3 ? "hex_river_B" : k.Column == -4 && k.Row is >= 2 and <= 4 ? "hex_grass_sloped_low" : "hex_grass", X: (int)Math.Floor(k.Column / 8.0), Z: (int)Math.Floor(k.Row / 8.0))))
        {
            if (!_terrain.TryGetValue(group.Key, out MultiMeshInstance3D? batch))
            {
                if (!_meshes.TryGetValue(group.Key.Kind, out Mesh? imported))
                {
                    Node3D template = _assets.Native(this, $"Medieval/{group.Key.Kind}.gltf", Vector3.Zero, 1);
                    template.Visible = false;
                    MeshInstance3D mesh = template is MeshInstance3D direct ? direct : template.FindChildren("*", "MeshInstance3D", true, false).OfType<MeshInstance3D>().Single();
                    _meshTransforms[group.Key.Kind] = template.GlobalTransform.AffineInverse() * mesh.GlobalTransform;
                    _meshes[group.Key.Kind] = imported = mesh.Mesh;
                }
                batch = new MultiMeshInstance3D { Multimesh = new MultiMesh { TransformFormat = MultiMesh.TransformFormatEnum.Transform3D, Mesh = imported }, CastShadow = GeometryInstance3D.ShadowCastingSetting.Off };
                AddChild(batch); _terrain[group.Key] = batch;
            }
            var keys = group.ToArray();
            batch.Multimesh.InstanceCount = keys.Length;
            for (int i = 0; i < keys.Length; i++)
            {
                var (column, row) = keys[i];
                float rotation = column == 3 ? (Math.Abs(row) % 2 == 1 ? 60 : 240) : group.Key.Kind == "hex_grass_sloped_low" ? 180 : 0;
                var transform = new Transform3D(new Basis(Vector3.Up, Mathf.DegToRad(rotation)).Scaled(Vector3.One * VillageLayout.TerrainScale), VillageLayout.Hex(column, row));
                batch.Multimesh.SetInstanceTransform(i, transform * _meshTransforms[group.Key.Kind]);
            }
        }
        // Capture installed batch transforms only when geometry changes, keeping fresh probes cheap.
        _renderedCells = _terrain.Values.SelectMany(n => Enumerable.Range(0, n.Multimesh.InstanceCount).Select(i => n.Multimesh.GetInstanceTransform(i).Origin)).ToArray();
        _coreTerrain = _terrain.SelectMany(p => Enumerable.Range(0, p.Value.Multimesh.InstanceCount).Select(i => (p.Key.Kind, Transform: p.Value.Multimesh.GetInstanceTransform(i))))
            .Where(p => Math.Abs(p.Transform.Origin.X) <= 10.5f && Math.Abs(p.Transform.Origin.Z) <= 13.1f)
            .Select(p => FormattableString.Invariant($"{p.Kind}:{p.Transform.Origin.X:F3}:{p.Transform.Origin.Y:F3}:{p.Transform.Origin.Z:F3}:{p.Transform.Basis.X.X:F3}:{p.Transform.Basis.X.Z:F3}"))
            .Order(StringComparer.Ordinal).ToArray();
    }
    internal object Observe(Camera3D camera, Rect2 area)
    {
        Vector3[] cells = _renderedCells;
        return new
        {
            Tiles = cells.Length,
            TerrainBatches = _terrain.Count,
            CoreTerrain = _coreTerrain,
            Bridge = FormattableString.Invariant($"{_bridge.Position.X:F3}:{_bridge.Position.Y:F3}:{_bridge.Position.Z:F3}:{_bridge.Scale.X:F3}:{_bridge.Rotation.Y:F3}"),
            Plots = GetChildren().OfType<MeshInstance3D>().Count(n => n.Name.ToString().StartsWith("Plot", StringComparison.Ordinal)),
            MinX = cells.Min(p => p.X),
            MaxX = cells.Max(p => p.X),
            MinZ = cells.Min(p => p.Z),
            MaxZ = cells.Max(p => p.Z),
            View = Footprint(camera, area, this.GlobalPosition).Select(p => new[] { p.X, p.Z }).ToArray(),
            Static = _static.Select(n => new
            {
                Asset = n.GetMeta("asset").AsString(),
                X = n.Position.X,
                Y = n.Position.Y,
                Scale = n.GetChild<Node3D>(0).Scale.X,
                Z = n.Position.Z,
                Contact = new[] { LandscapeAssets.Contact(n).X, LandscapeAssets.Contact(n).Y, LandscapeAssets.Contact(n).Z }
            }).ToArray()
        };
    }
}
