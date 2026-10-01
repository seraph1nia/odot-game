using Godot;

namespace Game;

// Imported geometry stays immutable. Wrappers express ground-footprint placement.
internal sealed class LandscapeAssets
{
    private readonly Dictionary<string, PackedScene> _scenes = [];
    private readonly Dictionary<string, (Aabb Bounds, Vector3 Foot)> _geometry = [];
    internal IEnumerable<string> Paths => _scenes.Keys;

    internal Node3D Native(Node3D parent, string path, Vector3 position, float scale, float rotation = 0)
    {
        Node3D node = Scene(path).Instantiate<Node3D>();
        node.Position = position; node.Scale = Vector3.One * scale; node.RotationDegrees = new(0, rotation, 0);
        parent.AddChild(node);
        return node;
    }
    private PackedScene Scene(string path)
    {
        if (!_scenes.TryGetValue(path, out PackedScene? scene)) _scenes[path] = scene = GD.Load<PackedScene>("res://Assets/KayKit/" + path);
        return scene;
    }
    internal Node3D Place(Node3D parent, string path, Vector3 position, float size)
    {
        var wrapper = new Node3D { Position = position }; parent.AddChild(wrapper);
        Node3D model = Scene(path).Instantiate<Node3D>(); wrapper.AddChild(model);
        if (!_geometry.TryGetValue(path, out var geometry))
        {
            var points = new List<Vector3>();
            void Visit(Node3D node, Transform3D transform)
            {
                Transform3D current = transform * node.Transform;
                if (node is MeshInstance3D { Mesh: { } mesh }) points.AddRange(mesh.GetFaces().Select(p => current * p));
                foreach (Node3D child in node.GetChildren().OfType<Node3D>()) Visit(child, current);
            }
            Visit(model, Transform3D.Identity);
            if (points.Count == 0) throw new InvalidOperationException("Missing landscape geometry: " + path);
            Vector3 min = points.Aggregate((a, b) => a.Min(b)), max = points.Aggregate((a, b) => a.Max(b));
            Vector3[] floor = points.Where(p => p.Y <= min.Y + (max.Y - min.Y) * 0.025f + 0.001f).ToArray();
            Vector3 lower = floor.Aggregate((a, b) => a.Min(b)), upper = floor.Aggregate((a, b) => a.Max(b));
            geometry = (new Aabb(min, max - min), new Vector3((lower.X + upper.X) / 2, min.Y, (lower.Z + upper.Z) / 2));
            _geometry[path] = geometry;
        }
        float factor = size / Math.Max(geometry.Bounds.Size.X, Math.Max(geometry.Bounds.Size.Y, geometry.Bounds.Size.Z));
        model.Scale *= factor; model.Position -= geometry.Foot * factor;
        wrapper.SetMeta("asset", path); wrapper.SetMeta("foot", geometry.Foot); wrapper.SetMeta("bounds", geometry.Bounds);
        return wrapper;
    }
    internal static Aabb Bounds(Node3D wrapper) => wrapper.Transform * wrapper.GetChild<Node3D>(0).Transform * wrapper.GetMeta("bounds").AsAabb();
    internal static Vector3 Contact(Node3D wrapper) => wrapper.Transform * wrapper.GetChild<Node3D>(0).Transform * wrapper.GetMeta("foot").AsVector3();
    // Central deck is at authored Y=1.4; the decorative battlements reach 1.5.
    internal static float TowerDeck(Node3D wrapper) => (wrapper.Transform * wrapper.GetChild<Node3D>(0).Transform * new Vector3(0, 1.4f, 0)).Y;
}
