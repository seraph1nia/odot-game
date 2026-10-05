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
    private readonly Dictionary<string, (Vector3 Left, Vector3 Right)> _waterEdges = [];
    private (int Left, int Right, int Back, int Front)? _range;

    internal VillageLandscape(Node3D parent, LandscapeAssets assets)
    {
        _assets = assets; Name = "Scenery"; parent.AddChild(this);
        _static.Add(assets.Place(this, AssetCatalog.Home, Home, 2.3f));
        _static.Add(assets.Place(this, AssetCatalog.Defender, Defender, 1.7f));
        foreach (var (path, column, row, size) in new[]
        {
            ("environment/components/forest_spiral_tree.glb", -3, -4, 2.4f),
            ("environment/components/forest_spiral_tree.glb", -3, -1, 2.2f),
            ("environment/components/forest_spiral_tree.glb", -3, 2, 2.1f),
            ("environment/components/forest_spiral_tree.glb", -3, 4, 2.4f),
            ("props/kit_pine.glb", 2, -4, 1.8f), ("props/kit_pine.glb", 2, -1, 1.8f),
            ("environment/components/forest_boulder.glb", -2, -3, .65f),
            ("environment/components/forest_boulder.glb", 2, -3, .65f),
            ("environment/components/forest_boulder.glb", 2, 4, .7f),
            ("environment/components/forest_boulder.glb", -2, 1, .6f),
            ("environment/components/forest_boulder.glb", -3, 6, 1.8f),
            ("environment/components/forest_boulder.glb", -3, -5, 1.8f),
            ("environment/components/forest_fern.glb", -3, 1, .75f),
            ("environment/components/forest_mushroom_small.glb", -2, 4, .55f),
            ("environment/components/forest_leaf_clump.glb", -2, -4, .7f),
            ("environment/components/forest_moss.glb", 2, 0, .65f),
            ("environment/components/forest_lantern_post.glb", -2, 6, 1.1f),
            ("props/kit_barrel.glb", 1, 1, .45f), (AssetCatalog.Provisions, 2, 0, .35f)
        }) _static.Add(assets.Place(this, path, VillageLayout.Hex(column, row), size));
        // This complete authored stream/bridge cell replaces, not overlays, one
        // stream instance. Preserve bank zero and intentionally submerged supports.
        _bridge = assets.Native(this, AssetCatalog.Bridge, VillageLayout.Hex(VillageLayout.BridgeColumn, VillageLayout.RiverRow), VillageLayout.TerrainScale, 90, original: true);
        _waterEdges[AssetCatalog.Bridge] = WaterEdges(_bridge);
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
            MaterialOverride = new StandardMaterial3D { AlbedoColor = new(slot < 5 ? "e0dbaf" : "757d78"), ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded, CullMode = BaseMaterial3D.CullModeEnum.Disabled },
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        });
    }
    internal void SetPlots(Game.Core.SlotState[] slots)
    {
        for (int slot = 0; slot < slots.Length; slot++)
        {
            var plot = GetNode<MeshInstance3D>("Plot" + slot);
            ((StandardMaterial3D)plot.MaterialOverride).AlbedoColor = new(slots[slot].Purchased ? "e0dbaf" : "757d78");
        }
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
                _assets.Place(cell, "props/kit_pine.glb", Vector3.Zero, 1.5f);
            else if ((column <= -5 || column >= 5 || row <= -7 || row >= 7) && Math.Abs(column * 13 + row * 23) % 19 == 0)
                _assets.Place(cell, "environment/components/forest_boulder.glb", Vector3.Zero, .5f);
        }
        RebuildTerrain();
        // Deferred candidate: ordinary gameplay keeps the original hierarchy
        // without allocating hidden native batches. Only the owned cost slice
        // exercises this measured but insufficient optimization.
    }
    private void RebuildTerrain()
    {
        foreach (var group in _tiles.Keys.Where(k => k != (VillageLayout.BridgeColumn, VillageLayout.RiverRow))
            .GroupBy(k => (Kind: k.Row == VillageLayout.RiverRow ? AssetCatalog.Stream : AssetCatalog.Meadow, X: (int)Math.Floor(k.Column / 8.0), Z: (int)Math.Floor(k.Row / 8.0))))
        {
            if (!_terrain.TryGetValue(group.Key, out MultiMeshInstance3D? batch))
            {
                if (!_meshes.TryGetValue(group.Key.Kind, out Mesh? imported))
                {
                    _meshes[group.Key.Kind] = imported = _assets.Terrain(group.Key.Kind);
                    if (group.Key.Kind == AssetCatalog.Stream)
                    {
                        Node3D template = _assets.Native(this, group.Key.Kind, Vector3.Zero, 1, original: true);
                        _waterEdges[group.Key.Kind] = WaterEdges(template);
                        this.RemoveChild(template); template.Free();
                    }
                }
                batch = new MultiMeshInstance3D { Multimesh = new MultiMesh { TransformFormat = MultiMesh.TransformFormatEnum.Transform3D, Mesh = imported }, CastShadow = GeometryInstance3D.ShadowCastingSetting.Off };
                AddChild(batch); _terrain[group.Key] = batch;
            }
            var keys = group.ToArray();
            batch.Multimesh.InstanceCount = keys.Length;
            for (int i = 0; i < keys.Length; i++)
            {
                var (column, row) = keys[i];
                // Hexagonal symmetry permits six orientations without changing
                // coverage or the anchor lattice; it breaks repeated meadow UVs.
                float yaw = Mathf.Pi / 2 + (group.Key.Kind == AssetCatalog.Meadow ? Mathf.PosMod(column * 17 + row * 31, 6) * Mathf.Pi / 3 : 0);
                var transform = new Transform3D(new Basis(Vector3.Up, yaw).Scaled(Vector3.One * VillageLayout.TerrainScale), VillageLayout.Hex(column, row));
                batch.Multimesh.SetInstanceTransform(i, transform);
            }
        }
        // Capture installed batch transforms only when geometry changes, keeping fresh probes cheap.
        _renderedCells = _terrain.Values.SelectMany(n => Enumerable.Range(0, n.Multimesh.InstanceCount).Select(i => n.Multimesh.GetInstanceTransform(i).Origin)).Append(_bridge.Position).ToArray();
        _coreTerrain = _terrain.SelectMany(p => Enumerable.Range(0, p.Value.Multimesh.InstanceCount).Select(i => (p.Key.Kind, Transform: p.Value.Multimesh.GetInstanceTransform(i))))
            .Where(p => Math.Abs(p.Transform.Origin.X) <= 10.5f && Math.Abs(p.Transform.Origin.Z) <= 13.1f)
            .Select(p => FormattableString.Invariant($"{p.Kind}:{p.Transform.Origin.X:F3}:{p.Transform.Origin.Y:F3}:{p.Transform.Origin.Z:F3}:{p.Transform.Basis.X.X:F3}:{p.Transform.Basis.X.Z:F3}"))
            .Order(StringComparer.Ordinal).ToArray();
    }
    private static (Vector3 Left, Vector3 Right) WaterEdges(Node3D model)
    {
        // The composed bridge copies the component as .001; Godot also
        // normalizes imported node names. Require its unique continuous-water
        // mesh, not the separate waterfall/foam decoration.
        MeshInstance3D water = model.FindChildren("Continuous*river*water*", "MeshInstance3D", true, false).OfType<MeshInstance3D>().Single();
        Transform3D relative = model.GlobalTransform.AffineInverse() * water.GlobalTransform;
        Aabb box = relative * water.Mesh.GetAabb();
        Vector3 center = box.GetCenter();
        return (new(center.X, box.End.Y, box.Position.Z), new(center.X, box.End.Y, box.End.Z));
    }
    internal object Observe(Camera3D camera, Rect2 area)
    {
        Vector3[] cells = _renderedCells;
        return new
        {
            Tiles = cells.Length,
            TerrainBatches = _terrain.Count,
            TerrainSurfaces = _meshes.Values.Sum(m => m.GetSurfaceCount()),
            TerrainBounds = _meshes.Select(p => new { Asset = p.Key, Min = new[] { p.Value.GetAabb().Position.X, p.Value.GetAabb().Position.Y, p.Value.GetAabb().Position.Z }, Max = new[] { p.Value.GetAabb().End.X, p.Value.GetAabb().End.Y, p.Value.GetAabb().End.Z } }).ToArray(),
            River = _terrain.Where(p => p.Key.Kind == AssetCatalog.Stream)
                .SelectMany(p => Enumerable.Range(0, p.Value.Multimesh.InstanceCount).Select(i => (Asset: p.Key.Kind, Transform: p.Value.Multimesh.GetInstanceTransform(i))))
                .Append((Asset: AssetCatalog.Bridge, Transform: _bridge.Transform)).Select(p =>
                {
                    var edge = _waterEdges[p.Asset]; Vector3 a = p.Transform * edge.Left, b = p.Transform * edge.Right;
                    return new { Asset = p.Asset, Start = new[] { a.X, a.Y, a.Z }, End = new[] { b.X, b.Y, b.Z } };
                }).ToArray(),
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
