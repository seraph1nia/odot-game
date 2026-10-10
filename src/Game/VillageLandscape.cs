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
    private sealed record ScatterGroup(LandscapeAssets.SceneryPart[] Parts, Aabb Bounds, MultiMeshInstance3D[] Batches)
    {
        internal readonly List<Transform3D> Placements = [];
    }
    private readonly Dictionary<(string Asset, int X, int Z), ScatterGroup> _scatter = [];
    private Vector3[] _renderedCells = [];
    private string[] _coreTerrain = [];
    private readonly Node3D _bridge;
    private readonly int _bridgeSubstratesRemoved;
    private readonly List<(string Name, MultiMeshInstance3D Batch)> _paths = [];
    private float[][] _bridgeLandings = [];
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
        })
        {
            Vector3 point = VillageLayout.Hex(column, row);
            point.Y = GroundSurface(point);
            _static.Add(assets.Place(this, path, point, size));
        }
        // Only this consumer's bridge loses its recessed stream/foam/fall and
        // substrate. The new same-origin floor + surface stream continues below
        // the unchanged timber deck; no old/new water interface is joined.
        _bridge = assets.Native(this, AssetCatalog.Bridge, VillageLayout.Hex(VillageLayout.BridgeColumn, VillageLayout.RiverRow), VillageLayout.TerrainScale, 90);
        _bridgeSubstratesRemoved = GroundGeometry.AdaptBridge(_bridge);
        for (int slot = 0; slot < 9; slot++) PlotOutline(slot);
        ComposeVillage();
    }
    private void ComposeVillage()
    {
        // The authority approach, old props, structures and plot centers remain
        // untouched. These pockets frame the village from its wooded sides/bank;
        // no new tall canopy or shadow can enter the battle's projected band.
        foreach (var (pocket, column, row, tree) in new[]
        {
            ("West grove", -4, 4, true), ("West grove", -5, 8, true),
            ("Mushroom hollow", -3, 3, false), ("Mushroom hollow", -3, 4, false),
            ("East grove", 4, 4, true), ("East grove", 5, 8, true),
            ("Bank garden", -1, 6, false), ("Bank garden", 0, 7, false)
        })
        {
            Vector3 anchor = VillageLayout.Hex(column, row);
            if (tree)
            {
                Decoration(pocket, "environment/components/forest_spiral_tree.glb", anchor + new Vector3(-.25f, 0, .15f), 2.8f);
                Decoration(pocket, "props/kit_pine.glb", anchor + new Vector3(1.1f, 0, .55f), 1.65f);
                Decoration(pocket, "environment/components/forest_fern.glb", anchor + new Vector3(.65f, 0, -.55f), .65f);
            }
            else
            {
                Decoration(pocket, "environment/components/forest_mushroom_small.glb", anchor + new Vector3(-.5f, 0, .2f), pocket == "Mushroom hollow" ? 1.3f : .8f);
                Decoration(pocket, "environment/components/forest_mushroom_small.glb", anchor + new Vector3(.45f, 0, -.3f), pocket == "Mushroom hollow" ? .8f : .5f);
                Decoration(pocket, "environment/components/forest_fern.glb", anchor + new Vector3(.7f, 0, .55f), .55f);
            }
            Decoration(pocket, "environment/components/forest_moss.glb", anchor + new Vector3(-.65f, 0, -.65f), .7f);
            Decoration(pocket, "environment/components/forest_leaf_clump.glb", anchor + new Vector3(.1f, 0, .7f), .5f);
        }
        Decoration("Bridge clearing", "environment/components/forest_lantern_post.glb", VillageLayout.Hex(2, 6), 1.1f);
        Decoration("Bridge clearing", "environment/components/forest_boulder.glb", VillageLayout.Hex(2, 7) + new Vector3(.5f, 0, .25f), .65f);
        Decoration("Wayside shrine", "environment/components/forest_shrine_plinth.glb", VillageLayout.Hex(1, 10) + new Vector3(.9f, 0, .3f), 1.15f);
        Decoration("Wayside shrine", "environment/components/forest_boulder.glb", VillageLayout.Hex(2, 10) + new Vector3(-.3f, 0, .1f), 1.2f);
        Decoration("Woodland trail clearing", "environment/components/forest_mushroom_small.glb", VillageLayout.Hex(-5, 7) + new Vector3(-.8f, 0, .3f), 1.15f);
        foreach (var (column, row) in new[] { (-12, 8), (13, 6) })
        {
            Vector3 spring = VillageLayout.Hex(column, row);
            Decoration("Rocky watercourse end", "environment/components/forest_boulder.glb", spring + new Vector3(.3f, 0, .7f), .9f);
            Decoration("Rocky watercourse end", "environment/components/forest_boulder.glb", spring + new Vector3(-.8f, 0, .35f), .65f);
            Decoration("Rocky watercourse end", "environment/components/forest_fern.glb", spring + new Vector3(.85f, 0, -.65f), .7f);
        }

        // A timber village-edge walk, not a painted combat road. Ordinary imported
        // plank buffers/materials are reused in six short batches. Consolidation
        // is fidelity-checked, unlike the rejected global static flattening.
        Node3D template = _assets.Place(this, AssetCatalog.Timber, Vector3.Zero, 1.1f);
        ArrayMesh mesh;
        try { mesh = StaticGeometry.Consolidate(StaticGeometry.Capture(template)); }
        finally { RemoveChild(template); template.Free(); }
        _bridgeLandings = DeckLandings();
        Vector3[] north = [new(-6, 0, VillageLayout.RowStep * 2), new(-6, 0, VillageLayout.RowStep * 3),
            new(-6.7f, 0, VillageLayout.RowStep * 4), new(-6, 0, 11.95f), new(4.5f, 0, 11.95f), new(4.5f, 0, 12.15f)];
        for (int i = 1; i < north.Length; i++) Path("Village walk " + i, mesh, north[i - 1], north[i]);
        Path("South bank walk", mesh, new(4.5f, 0, 13.83f), VillageLayout.Hex(1, 6));
    }
    private void Decoration(string pocket, string path, Vector3 point, float size)
    {
        // Offsets are local grouping, never an elevation guess or map mutation.
        point.Y = GroundSurface(point);
        Node3D node = _assets.Place(this, path, point, size);
        node.SetMeta("pocket", pocket);
        foreach (GeometryInstance3D geometry in node.FindChildren("*", "GeometryInstance3D", true, false).OfType<GeometryInstance3D>())
            geometry.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
        foreach (Light3D light in node.FindChildren("*", "Light3D", true, false).OfType<Light3D>()) light.Visible = false;
        _static.Add(node);
    }
    private float GroundSurface(Vector3 point)
    {
        int row = (int)Math.Round(point.Z / VillageLayout.RowStep);
        int column = (int)Math.Round((point.X - Math.Abs(row % 2) * VillageLayout.HalfWidth) / (VillageLayout.HalfWidth * 2));
        float height = DetailedGround.Elevation(column, row);
        foreach (var (c, r, connector) in DetailedGround.BankTrail().Concat(DetailedGround.WoodlandTrail()).Concat(DetailedGround.Watercourse()))
        {
            // Every imported ribbon fits the common hex. This deliberately
            // generous broad phase skips only distant cells; nearby contacts
            // still resolve actual triangles, not a guessed ribbon height.
            float x = c * VillageLayout.HalfWidth * 2 + Math.Abs(r % 2) * VillageLayout.HalfWidth;
            if (Math.Abs(point.X - x) > VillageLayout.Radius * 2 || Math.Abs(point.Z - r * VillageLayout.RowStep) > VillageLayout.Radius * 2) continue;
            height = Math.Max(height, GroundGeometry.Surface(_assets.Terrain(connector.Asset), VillageLayout.GroundTransform(c, r, connector.Turns), point) ?? height);
        }
        return height;
    }
    private void Path(string name, Mesh mesh, Vector3 start, Vector3 end)
    {
        start.Y = 0;
        end.Y = 0;
        Vector3 tangent = (end - start).Normalized();
        float span = mesh.GetAabb().Size.X;
        int count = (int)Math.Ceiling(start.DistanceTo(end) / (span * .985f));
        var transforms = new List<Transform3D>();
        for (int i = 0; i < count; i++)
        {
            Vector3 point = start.Lerp(end, (i + .5f) / count);
            point.Y = WalkSurface(point);
            var transform = new Transform3D(new Basis(tangent, Vector3.Up, tangent.Cross(Vector3.Up)), point);
            transforms.Add(transform);
        }
        // The authored bridge has a raised deck. A second grounded course on
        // its lower bank makes an honest timber step, not a floating connector.
        foreach (Transform3D foundation in transforms.ToArray())
            if (foundation.Origin.Y < 0)
                transforms.Add(foundation.Translated(new(0, mesh.GetAabb().Size.Y, 0)));
        var batch = new MultiMeshInstance3D
        {
            Name = name.Replace(' ', '_'),
            Multimesh = new MultiMesh { TransformFormat = MultiMesh.TransformFormatEnum.Transform3D, Mesh = mesh, InstanceCount = transforms.Count },
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        AddChild(batch); _paths.Add((name, batch)); batch.SetMeta("foundation_count", count);
        for (int i = 0; i < transforms.Count; i++) batch.Multimesh.SetInstanceTransform(i, transforms[i]);
    }
    private static float WalkSurface(Vector3 point)
    {
        // Unlike the ordinary center-anchored props, a walk crosses hex edges.
        // Resolve the actual supporting hex footprint at the stepped banks;
        // do not change the shared combat/structure surface mapping.
        int nearestRow = (int)Math.Round(point.Z / VillageLayout.RowStep);
        float height = float.NegativeInfinity;
        for (int row = nearestRow - 1; row <= nearestRow + 1; row++)
        {
            int nearestColumn = (int)Math.Round((point.X - Math.Abs(row) % 2 * VillageLayout.HalfWidth) / (VillageLayout.HalfWidth * 2));
            for (int column = nearestColumn - 1; column <= nearestColumn + 1; column++)
            {
                Vector3 local = point - VillageLayout.Hex(column, row);
                if (Math.Abs(local.X) <= VillageLayout.HalfWidth + .001f && Math.Abs(local.Z) <= VillageLayout.Radius - Math.Abs(local.X) / Mathf.Sqrt(3) + .001f)
                    height = Math.Max(height, VillageLayout.Height(column, row));
            }
        }
        if (!float.IsFinite(height)) throw new InvalidOperationException("Walk lost its supporting hex.");
        return height;
    }
    private float[][] DeckLandings()
    {
        var points = new List<Vector3>();
        foreach (MeshInstance3D plank in _bridge.FindChildren("*", "MeshInstance3D", true, false).OfType<MeshInstance3D>()
            .Where(n => n.Name.ToString().Contains("bridge", StringComparison.OrdinalIgnoreCase) && n.Name.ToString().Contains("plank", StringComparison.OrdinalIgnoreCase)))
            for (int surface = 0; surface < plank.Mesh.GetSurfaceCount(); surface++)
                points.AddRange(plank.Mesh.SurfaceGetArrays(surface)[(int)Mesh.ArrayType.Vertex].AsVector3Array().Select(p => ToLocal(plank.GlobalTransform * p)));
        if (points.Count == 0) throw new InvalidOperationException("Missing authored bridge deck geometry.");
        return new[] { points.Min(p => p.Z), points.Max(p => p.Z) }.Select(z =>
        {
            Vector3[] edge = points.Where(p => Math.Abs(p.Z - z) < .001f).ToArray();
            return new[] { (edge.Min(p => p.X) + edge.Max(p => p.X)) / 2, edge.Max(p => p.Y), z };
        }).ToArray();
    }
    private float[] ProjectedBounds(Node3D node, Camera3D camera)
    {
        Aabb box = LandscapeAssets.Bounds(node);
        Transform2D screen = GetViewport().GetFinalTransform();
        Vector2[] corners = Enumerable.Range(0, 8).Select(i => screen * camera.UnprojectPosition(ToGlobal(box.GetEndpoint(i)))).ToArray();
        return [corners.Min(p => p.X), corners.Min(p => p.Y), corners.Max(p => p.X), corners.Max(p => p.Y)];
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
            Front: Math.Max(7, (int)Math.Ceiling((max.Z + travel.Y * 2) / VillageLayout.RowStep) + 3));
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
            bool originalScatter = (row < 3 || row > 14 || column is < -12 or > 12)
                && (row < 3 || DetailedGround.ClearOfRoutes(cell.Position.X, cell.Position.Z, .7f));
            Vector3 originalFoot = new(0, DetailedGround.Elevation(column, row) - VillageLayout.Height(column, row), 0);
            if (originalScatter && (column <= -5 || row <= -7 && column >= 5) && Math.Abs(column * 17 + row * 31) % 7 == 0)
                _assets.Place(cell, "props/kit_pine.glb", originalFoot, 1.5f);
            else if (originalScatter && (column <= -5 || column >= 5 || row <= -7 || row >= 7) && Math.Abs(column * 13 + row * 23) % 19 == 0)
                _assets.Place(cell, "environment/components/forest_boulder.glb", originalFoot, .5f);
            foreach (GroundDecoration detail in DetailedGround.Scatter(column, row)) Scatter(detail, column, row);
        }
        RebuildTerrain();
        RebuildScatter();
    }
    private void RebuildTerrain()
    {
        var placements = _tiles.Keys.Select(k => (Kind: DetailedGround.Base(k.Column, k.Row), k.Column, k.Row, Turns: DetailedGround.FloorTurns(k.Column, k.Row)))
            .Concat(DetailedGround.Watercourse().Concat(DetailedGround.BankTrail()).Concat(DetailedGround.WoodlandTrail())
                .Where(p => _tiles.ContainsKey((p.Column, p.Row))).Select(p => (Kind: p.Connector.Asset, p.Column, p.Row, p.Connector.Turns)));
        foreach (var group in placements.GroupBy(k => (k.Kind, X: (int)Math.Floor(k.Column / 8.0), Z: (int)Math.Floor(k.Row / 8.0))))
        {
            if (!_terrain.TryGetValue(group.Key, out MultiMeshInstance3D? batch))
            {
                if (!_meshes.TryGetValue(group.Key.Kind, out Mesh? imported)) _meshes[group.Key.Kind] = imported = _assets.Terrain(group.Key.Kind);
                batch = new MultiMeshInstance3D { Multimesh = new MultiMesh { TransformFormat = MultiMesh.TransformFormatEnum.Transform3D, Mesh = imported }, CastShadow = GeometryInstance3D.ShadowCastingSetting.Off };
                AddChild(batch); _terrain[group.Key] = batch;
            }
            var keys = group.ToArray();
            batch.Multimesh.InstanceCount = keys.Length;
            for (int i = 0; i < keys.Length; i++)
                batch.Multimesh.SetInstanceTransform(i, VillageLayout.GroundTransform(keys[i].Column, keys[i].Row, keys[i].Turns));
        }
        // Count floor cells once, not overlay layers or the timber-only bridge.
        _renderedCells = _tiles.Values.Select(n => n.Position).ToArray();
        _coreTerrain = _terrain.SelectMany(p => Enumerable.Range(0, p.Value.Multimesh.InstanceCount).Select(i => (p.Key.Kind, Transform: p.Value.Multimesh.GetInstanceTransform(i))))
            .Where(p => Math.Abs(p.Transform.Origin.X) <= 10.5f && Math.Abs(p.Transform.Origin.Z) <= 13.1f)
            .Select(p => FormattableString.Invariant($"{p.Kind}:{p.Transform.Origin.X:F3}:{p.Transform.Origin.Y:F3}:{p.Transform.Origin.Z:F3}:{p.Transform.Basis.X.X:F3}:{p.Transform.Basis.X.Z:F3}"))
            .Order(StringComparer.Ordinal).ToArray();
    }
    private void Scatter(GroundDecoration detail, int column, int row)
    {
        var key = (detail.Asset, X: (int)Math.Floor(column / 16.0), Z: (int)Math.Floor(row / 16.0));
        if (!_scatter.TryGetValue(key, out ScatterGroup? group))
        {
            var geometry = _assets.Scenery(detail.Asset);
            MultiMeshInstance3D[] batches = geometry.Parts.Select(p => new MultiMeshInstance3D
            {
                Multimesh = new MultiMesh { TransformFormat = MultiMesh.TransformFormatEnum.Transform3D, Mesh = p.Mesh },
                MaterialOverride = p.Material,
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
            }).ToArray();
            foreach (MultiMeshInstance3D batch in batches) AddChild(batch);
            _scatter[key] = group = new(geometry.Parts, geometry.Bounds, batches);
        }
        group.Placements.Add(new(new Basis(Vector3.Up, Mathf.DegToRad(detail.Yaw)).Scaled(Vector3.One * detail.Size),
            new Vector3(detail.X, DetailedGround.Elevation(column, row), detail.Z)));
    }
    private void RebuildScatter()
    {
        // Cover grows only when its envelope changes. No per-frame population,
        // RNG, new lights, shadows, or copies of imported meshes/textures.
        foreach (ScatterGroup group in _scatter.Values)
            for (int part = 0; part < group.Parts.Length; part++)
            {
                MultiMesh instances = group.Batches[part].Multimesh;
                instances.InstanceCount = group.Placements.Count;
                for (int i = 0; i < group.Placements.Count; i++) instances.SetInstanceTransform(i, group.Placements[i] * group.Parts[part].Transform);
            }
    }
    // Read-only native ownership seam for the owned fixed-view counterfactual.
    internal IEnumerable<MultiMeshInstance3D> NativeGrassOwners() => _terrain.Where(p => p.Key.Kind == DetailedGround.Grass).Select(p => p.Value);
    private IEnumerable<(string Asset, MultiMesh Instances, Transform3D Transform)> Installed(string asset) => _terrain.Where(p => p.Key.Kind == asset)
        .SelectMany(p => Enumerable.Range(0, p.Value.Multimesh.InstanceCount).Select(i => (asset, p.Value.Multimesh, p.Value.Multimesh.GetInstanceTransform(i))));
    private object[] ObserveRoute(IEnumerable<(int Column, int Row, GroundConnector Connector)> route) => route.SelectMany(p => Installed(p.Connector.Asset)
        .Where(n => n.Transform.Origin.IsEqualApprox(VillageLayout.GroundTransform(p.Column, p.Row).Origin)).Select(n => new
        {
            n.Asset,
            p.Column,
            p.Row,
            Ports = p.Connector.Edges.Select(e => GroundGeometry.Port(n.Instances.Mesh, n.Transform, (e - p.Connector.Turns + 6) % 6))
                .Select(v => new[] { v.X, v.Y, v.Z }).ToArray(),
            Profiles = p.Connector.Edges.Select(e => GroundGeometry.Profile(n.Instances.Mesh, (e - p.Connector.Turns + 6) % 6)
                .Select(v => n.Transform * v).Select(v => new[] { v.X, v.Y, v.Z }).ToArray()).ToArray(),
            Floor = DetailedGround.Base(p.Column, p.Row),
            Clearance = (n.Transform * new Vector3(0, n.Instances.Mesh.GetAabb().Position.Y, 0)).Y - n.Transform.Origin.Y
        }).Cast<object>()).ToArray();
    internal object Observe(Camera3D camera, Rect2 area)
    {
        Vector3[] cells = _renderedCells;
        float[] Viewpoint(int column, int row)
        {
            Vector2 screen = GetViewport().GetFinalTransform() * camera.UnprojectPosition(ToGlobal(VillageLayout.Hex(column, row)));
            return [screen.X, screen.Y];
        }
        return new
        {
            Tiles = cells.Length,
            TerrainBatches = _terrain.Count,
            TerrainSurfaces = _meshes.Values.Sum(m => m.GetSurfaceCount()),
            TerrainBounds = _meshes.Select(p => new { Asset = p.Key, Min = new[] { p.Value.GetAabb().Position.X, p.Value.GetAabb().Position.Y, p.Value.GetAabb().Position.Z }, Max = new[] { p.Value.GetAabb().End.X, p.Value.GetAabb().End.Y, p.Value.GetAabb().End.Z } }).ToArray(),
            River = ObserveRoute(DetailedGround.Watercourse()),
            DirtTrail = ObserveRoute(DetailedGround.BankTrail()),
            WoodlandTrail = ObserveRoute(DetailedGround.WoodlandTrail()),
            SceneryBatches = _scatter.Values.Sum(g => g.Batches.Length),
            SceneryInstances = _scatter.Values.Sum(g => g.Batches.Sum(b => b.Multimesh.InstanceCount)),
            SceneryMeshes = _scatter.Values.SelectMany(g => g.Parts.Select(p => p.Mesh)).Distinct().Count(),
            SceneryTransformError = _scatter.Values.SelectMany(g => g.Batches.SelectMany((b, part) => Enumerable.Range(0, b.Multimesh.InstanceCount).Select(i =>
            {
                Transform3D actual = b.Multimesh.GetInstanceTransform(i), expected = g.Placements[i] * g.Parts[part].Transform;
                return Math.Max(actual.Origin.DistanceTo(expected.Origin), Math.Max(actual.Basis.X.DistanceTo(expected.Basis.X),
                    Math.Max(actual.Basis.Y.DistanceTo(expected.Basis.Y), actual.Basis.Z.DistanceTo(expected.Basis.Z))));
            }))).DefaultIfEmpty().Max(),
            Scenery = _scatter.SelectMany(p => Enumerable.Range(0, p.Value.Batches[0].Multimesh.InstanceCount).Select(i =>
            {
                // Recover placement from actual emitted native instance bases,
                // not from the requested coordinates kept by the composition.
                Transform3D placement = p.Value.Batches[0].Multimesh.GetInstanceTransform(i) * p.Value.Parts[0].Transform.AffineInverse();
                Vector3 v = placement.Origin;
                Aabb box = placement * p.Value.Bounds;
                Transform2D screen = GetViewport().GetFinalTransform();
                Vector2[] corners = Enumerable.Range(0, 8).Select(j => screen * camera.UnprojectPosition(ToGlobal(box.GetEndpoint(j)))).ToArray();
                return new
                {
                    p.Key.Asset,
                    X = v.X,
                    Y = v.Y,
                    Z = v.Z,
                    Scale = placement.Basis.X.Length(),
                    Support = GroundSurface(v),
                    Contact = new[] { v.X, v.Y, v.Z },
                    ProjectedBounds = new[] { corners.Min(c => c.X), corners.Min(c => c.Y), corners.Max(c => c.X), corners.Max(c => c.Y) }
                };
            })).ToArray(),
            TrailView = Viewpoint(2, 7),
            ForestView = Viewpoint(-3, 2),
            FloorCells = DetailedGround.Bases.Sum(b => Installed(b).Count()),
            BridgeSubstratesRemoved = _bridgeSubstratesRemoved,
            BridgeRecessedMeshes = _bridge.FindChildren("*", "MeshInstance3D", true, false).Count(n => n.Name.ToString().Contains("river", StringComparison.OrdinalIgnoreCase) || n.Name.ToString().Contains("waterfall", StringComparison.OrdinalIgnoreCase) || n.Name.ToString().Contains("foam", StringComparison.OrdinalIgnoreCase)),
            CoreTerrain = _coreTerrain,
            BridgeLandings = _bridgeLandings,
            Paths = _paths.Select(p =>
            {
                MultiMesh instances = p.Batch.Multimesh; Aabb box = instances.Mesh.GetAabb();
                Vector3[] vertices = Enumerable.Range(0, instances.Mesh.GetSurfaceCount())
                    .SelectMany(i => instances.Mesh.SurfaceGetArrays(i)[(int)Mesh.ArrayType.Vertex].AsVector3Array()).ToArray();
                float bottom = vertices.Min(v => v.Y);
                Vector3[] underside = vertices.Where(v => v.Y <= bottom + .001f).ToArray();
                Vector3 a = instances.GetInstanceTransform(0) * new Vector3(box.Position.X, 0, 0);
                int foundations = p.Batch.GetMeta("foundation_count").AsInt32();
                Vector3 b = instances.GetInstanceTransform(foundations - 1) * new Vector3(box.End.X, 0, 0);
                return new
                {
                    p.Name,
                    Planks = instances.InstanceCount,
                    Foundations = foundations,
                    Thickness = box.Size.Y,
                    Start = new[] { a.X, a.Y, a.Z },
                    End = new[] { b.X, b.Y, b.Z },
                    Centers = Enumerable.Range(0, instances.InstanceCount).Select(i => { Vector3 v = instances.GetInstanceTransform(i).Origin; return new[] { v.X, v.Y, v.Z }; }).ToArray(),
                    Bases = Enumerable.Range(0, instances.InstanceCount).Select(i =>
                    {
                        Basis basis = instances.GetInstanceTransform(i).Basis;
                        return new[] { basis.X, basis.Y, basis.Z }.Select(v => new[] { v.X, v.Y, v.Z }).ToArray();
                    }).ToArray(),
                    BottomHeights = Enumerable.Range(0, instances.InstanceCount).Select(i =>
                    {
                        Transform3D transform = instances.GetInstanceTransform(i);
                        float[] heights = underside.Select(v => (transform * v).Y - (v.Y - bottom)).ToArray();
                        return new[] { heights.Min(), heights.Max() };
                    }).ToArray(),
                    Supports = Enumerable.Range(0, instances.InstanceCount).Select(i => WalkSurface(instances.GetInstanceTransform(i).Origin)).ToArray(),
                    Footprints = Enumerable.Range(0, instances.InstanceCount).Select(i => { Aabb b = instances.GetInstanceTransform(i) * box; return new[] { b.Position.X, b.Position.Z, b.End.X, b.End.Z }; }).ToArray()
                };
            }).ToArray(),
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
                Pocket = n.HasMeta("pocket") ? n.GetMeta("pocket").AsString() : "",
                ProjectedBounds = n.HasMeta("pocket") ? ProjectedBounds(n, camera) : [],
                X = n.Position.X,
                Y = n.Position.Y,
                Scale = n.GetChild<Node3D>(0).Scale.X,
                Z = n.Position.Z,
                Support = GroundSurface(n.Position),
                Contact = new[] { LandscapeAssets.Contact(n).X, LandscapeAssets.Contact(n).Y, LandscapeAssets.Contact(n).Z }
            }).ToArray()
        };
    }
}
