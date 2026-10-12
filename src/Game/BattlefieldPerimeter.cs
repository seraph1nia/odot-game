using Godot;

namespace Game;

// Opaque geometry, not density fog: works in Compatibility and when atmosphere
// is disabled. Grey cloud banks conceal the cut terrain and suggest continuation.
internal sealed partial class BattlefieldPerimeter : Node3D
{
    private (Rect2 Inner, Rect2 Outer)? _bounds;
    internal StandardMaterial3D Material { get; } = new()
    {
        AlbedoColor = new Color("9ba3a6"),
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        Transparency = BaseMaterial3D.TransparencyEnum.Disabled,
        CullMode = BaseMaterial3D.CullModeEnum.Disabled
    };
    internal void Cover(Rect2 inner, Rect2 boundary)
    {
        Rect2 outer = boundary.Grow(24);
        if (_bounds == (inner, outer)) return;
        _bounds = (inner, outer);
        foreach (Node child in GetChildren()) { RemoveChild(child); child.QueueFree(); }
        // Continuous opaque backstop under overlapping cloud lobes; unlike
        // particles, no gap can reveal a hard map edge or distant city geometry.
        Slab(new(outer.Position, new(inner.Position.X - outer.Position.X, outer.Size.Y)));
        Slab(new(new(inner.End.X, outer.Position.Y), new(outer.End.X - inner.End.X, outer.Size.Y)));
        Slab(new(new(inner.Position.X, outer.Position.Y), new(inner.Size.X, inner.Position.Y - outer.Position.Y)));
        Slab(new(new(inner.Position.X, inner.End.Y), new(inner.Size.X, outer.End.Y - inner.End.Y)));
        Bank(inner.Position, new(inner.End.X, inner.Position.Y));
        Bank(new(inner.Position.X, inner.End.Y), inner.End);
        Bank(inner.Position, new(inner.Position.X, inner.End.Y));
        Bank(new(inner.End.X, inner.Position.Y), inner.End);
    }
    private void Slab(Rect2 rect)
    {
        AddChild(new MeshInstance3D
        {
            Position = new(rect.GetCenter().X, -.25f, rect.GetCenter().Y),
            Mesh = new BoxMesh { Size = new(rect.Size.X, 2.5f, rect.Size.Y) },
            MaterialOverride = Material,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        });
    }
    private void Bank(Vector2 start, Vector2 end)
    {
        int count = (int)Math.Ceiling(start.DistanceTo(end) / 1.8f);
        for (int i = 0; i <= count; i++)
        {
            Vector2 p = start.Lerp(end, (float)i / count);
            float radius = 1.6f + .2f * (i % 3);
            AddChild(new MeshInstance3D
            {
                Position = new(p.X, .25f, p.Y),
                Scale = new(1, .7f, 1),
                Mesh = new SphereMesh { Radius = radius, Height = radius * 2, RadialSegments = 16, Rings = 8 },
                MaterialOverride = Material,
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
            });
        }
    }
}
