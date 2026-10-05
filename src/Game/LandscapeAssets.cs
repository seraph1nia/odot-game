using Godot;

namespace Game;

// Imported geometry stays immutable. Wrappers express ground-footprint placement.
internal sealed class LandscapeAssets
{
    private readonly Dictionary<string, PackedScene> _scenes = [];
    private readonly Dictionary<string, (Aabb Bounds, Vector3 Foot)> _geometry = [];
    private readonly Dictionary<string, ArrayMesh> _meshes = [];
    private readonly Dictionary<string, string> _versions = [];
    internal IEnumerable<string> Paths => _scenes.Keys;
    private static string Version(string path)
    {
        string source = AssetCatalog.Root + path;
        string import = source + ".import";
        string imported = "";
        using var config = new ConfigFile();
        if (Godot.FileAccess.FileExists(import) && config.Load(import) == Error.Ok) imported = config.GetValue("remap", "path", "").AsString();
        // Packed graphical resources may expose only their remap/imported bytes.
        // Include manifest and imported content, not a path-only persisted cache.
        return StaticRenderKey.Create(
            (Godot.FileAccess.FileExists(source) ? Godot.FileAccess.GetSha256(source) : Godot.FileAccess.GetSha256(AssetCatalog.Root + "manifest.json")) + ":" + (Godot.FileAccess.FileExists(imported) ? Godot.FileAccess.GetSha256(imported) : ""),
            Godot.FileAccess.FileExists(import) ? Godot.FileAccess.GetSha256(import) : "packed-import",
            Engine.GetVersionInfo()["string"].AsString(), ProjectSettings.GetSetting("rendering/renderer/rendering_method").AsString());
    }

    internal Node3D Native(Node3D parent, string path, Vector3 position, float scale, float rotation = 0)
    {
        Node3D node = Scene(path).Instantiate<Node3D>();
        node.Position = position; node.Scale = Vector3.One * scale; node.RotationDegrees = new(0, rotation, 0);
        node.SetMeta("asset", path);
        parent.AddChild(node); return node;
    }
    private PackedScene Scene(string path)
    {
        if (!AssetCatalog.RequiredPaths.Contains(path, StringComparer.Ordinal)) throw new InvalidOperationException("Unregistered landscape export: " + path);
        string version = Version(path);
        if (!_scenes.TryGetValue(path, out PackedScene? scene) || _versions.GetValueOrDefault(path) != version)
        {
            _geometry.Remove(path); _meshes.Remove(path);
            _scenes[path] = scene = ResourceLoader.Load<PackedScene>(AssetCatalog.Root + path, cacheMode: _versions.ContainsKey(path) ? ResourceLoader.CacheMode.ReplaceDeep : ResourceLoader.CacheMode.Reuse)
                ?? throw new InvalidOperationException("Missing authored landscape: " + path);
            _versions[path] = version;
        }
        return scene;
    }
    internal Node3D Place(Node3D parent, string path, Vector3 position, float size)
    {
        var wrapper = new Node3D { Position = position }; parent.AddChild(wrapper);
        // Optional static-scene consolidation is deferred: AppendFrom does
        // not preserve normals under all imported nonuniform transforms.
        Node3D model = Scene(path).Instantiate<Node3D>(); wrapper.AddChild(model);
        if (!_geometry.TryGetValue(path, out var geometry))
        {
            Vector3[] points = StaticGeometry.Positions(model);
            Vector3 min = points.Aggregate((a, b) => a.Min(b)), max = points.Aggregate((a, b) => a.Max(b));
            Vector3[] floor = points.Where(p => p.Y <= min.Y + (max.Y - min.Y) * .025f + .001f).ToArray();
            Vector3 lower = floor.Aggregate((a, b) => a.Min(b)), upper = floor.Aggregate((a, b) => a.Max(b));
            geometry = (new Aabb(min, max - min), new Vector3((lower.X + upper.X) / 2, min.Y, (lower.Z + upper.Z) / 2));
            _geometry[path] = geometry;
        }
        float extent = path.StartsWith("buildings/", StringComparison.Ordinal) ? Math.Max(geometry.Bounds.Size.X, geometry.Bounds.Size.Z)
            : Math.Max(geometry.Bounds.Size.X, Math.Max(geometry.Bounds.Size.Y, geometry.Bounds.Size.Z));
        float factor = size / extent;
        model.Scale *= factor; model.Position -= geometry.Foot * factor;
        wrapper.SetMeta("asset", path); wrapper.SetMeta("foot", geometry.Foot); wrapper.SetMeta("bounds", geometry.Bounds);
        return wrapper;
    }
    internal static Aabb Bounds(Node3D wrapper) => wrapper.Transform * wrapper.GetChild<Node3D>(0).Transform * wrapper.GetMeta("bounds").AsAabb();
    internal static Vector3 Contact(Node3D wrapper) => wrapper.Transform * wrapper.GetChild<Node3D>(0).Transform * wrapper.GetMeta("foot").AsVector3();
    internal static float TowerDeck(Node3D wrapper) => Bounds(wrapper).End.Y;
    internal ArrayMesh Terrain(string path)
    {
        if (_meshes.TryGetValue(path, out ArrayMesh? cached)) return cached;
        Node3D template = Scene(path).Instantiate<Node3D>();
        try { return _meshes[path] = StaticGeometry.Consolidate(StaticGeometry.Capture(template)); }
        finally { template.Free(); }
    }
}
