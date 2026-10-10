using Godot;

namespace Game;

// Extends the existing owned asset-fidelity route: unused orientation classes
// are real imported diagnostic placements, not claims about the live map.
internal static class GroundPlacementProof
{
    internal static object Run(Node owner)
    {
        var assets = new LandscapeAssets();
        var meshes = DetailedGround.Paths.ToDictionary(p => p, p => assets.Terrain(p));
        static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("Detailed ground: " + message);
        }
        foreach (Mesh mesh in meshes.Values)
            for (int i = 0; i < mesh.GetSurfaceCount(); i++)
            {
                Require(mesh.SurfaceGetMaterial(i) is BaseMaterial3D { NoDepthTest: false, Transparency: BaseMaterial3D.TransparencyEnum.Disabled }, "ordinary opaque depth testing");
                Require(mesh.SurfaceGetMaterial(i).RenderPriority == 0, "no priority bypass");
            }
        int stacks = 0, joins = 0;
        var orientations = new List<object>();
        foreach (string floor in DetailedGround.Bases)
            foreach (string overlay in DetailedGround.Paths.Except(DetailedGround.Bases))
            {
                Aabb bottom = meshes[floor].GetAabb(), top = meshes[overlay].GetAabb();
                Require(Math.Abs(bottom.End.Y) < .0001 && Math.Abs(bottom.Position.Y + .36) < .0001, "unchanged occupation plane/foundation");
                Require(top.Position.Y - bottom.End.Y >= .0059, "base/overlay clearance"); stacks++;
            }
        foreach (GroundOverlay family in Enum.GetValues<GroundOverlay>())
            for (int first = 0; first < 6; first++)
                for (int other = first; other < 6; other++)
                {
                    GroundConnector connector = DetailedGround.Connector(family, first, other == first ? null : other);
                    Transform3D transform = VillageLayout.GroundTransform(0, 0, connector.Turns);
                    // Instantiate the same native root wrapper as the game uses,
                    // guarding the imported quaternion/Euler placement pitfall.
                    var parent = new Node3D(); owner.AddChild(parent);
                    try
                    {
                        Node3D native = assets.Native(parent, connector.Asset, transform.Origin, VillageLayout.TerrainScale, 90 + connector.Turns * 60);
                        Vector3[] original = StaticGeometry.Positions(native), batched = Positions(meshes[connector.Asset]).Select(p => transform * p).ToArray();
                        Require(original.Length == batched.Length && original.All(p => batched.Any(q => p.DistanceTo(q) < .0001)), "native root and installed batch rotations agree");
                    }
                    finally { owner.RemoveChild(parent); parent.Free(); }
                    foreach (int edge in connector.Edges)
                    {
                        var next = DetailedGround.Neighbor(0, 0, edge);
                        GroundConnector neighbor = DetailedGround.Connector(family, (edge + 3) % 6);
                        Transform3D adjacent = VillageLayout.GroundTransform(next.Column, next.Row, neighbor.Turns);
                        int canonical = (edge - connector.Turns + 6) % 6;
                        Vector3[] a = GroundGeometry.Profile(meshes[connector.Asset], canonical).Select(p => transform * p).ToArray();
                        Vector3[] b = GroundGeometry.Profile(meshes[neighbor.Asset], 0).Select(p => adjacent * p).ToArray();
                        Require(a.Length == b.Length && a.All(p => b.Any(q => p.DistanceTo(q) < .0001)), "neighbor full port width/profile/height: " + connector.Asset + " edge " + edge);
                        Vector3 midpoint = (transform.Origin + adjacent.Origin) / 2;
                        Vector3 port = GroundGeometry.Port(meshes[connector.Asset], transform, canonical);
                        Require(new Vector2(port.X - midpoint.X, port.Z - midpoint.Z).Length() < .0001, "edge midpoint, not vertex");
                        joins++;
                    }
                    orientations.Add(new { Family = family.ToString(), connector.Asset, connector.Turns, connector.Edges });
                }
        // Real decorative bridge adaptation must leave neither competing floor
        // nor any recessed-water/foam meshes, while preserving the deck exactly.
        var holder = new Node3D(); owner.AddChild(holder);
        try
        {
            Node3D bridge = assets.Native(holder, AssetCatalog.Bridge, VillageLayout.Hex(1, 5), VillageLayout.TerrainScale, 90);
            MeshInstance3D[] deck = bridge.FindChildren("Bridge*deck*plank*", "MeshInstance3D", true, false).OfType<MeshInstance3D>().ToArray();
            Transform3D[] before = deck.Select(n => n.GlobalTransform).ToArray();
            Require(deck.Length > 0 && GroundGeometry.AdaptBridge(bridge) == 5, "consumer bridge adaptation");
            Require(before.SequenceEqual(deck.Select(n => n.GlobalTransform)), "bridge deck unchanged");
            Require(!bridge.FindChildren("*", "Node3D", true, false).Any(GroundGeometry.Recessed), "no buried recessed components");
            Aabb water = VillageLayout.GroundTransform(1, 5) * meshes[DetailedGround.Connector(GroundOverlay.River, 0, 3).Asset].GetAabb();
            float deckBottom = deck.Min(n => (n.GlobalTransform * n.Mesh.GetAabb()).Position.Y);
            Require(deckBottom > water.End.Y + .01, "surface water remains below timber deck with normal depth");
        }
        finally { owner.RemoveChild(holder); holder.Free(); }
        return new { Stacks = stacks, NeighborJoins = joins, Orientations = orientations, Scope = "owned imported ground diagnostics: connector orientations, neighbor profiles, base/overlay stacks and bridge adaptation" };
    }
    private static IEnumerable<Vector3> Positions(Mesh mesh) => Enumerable.Range(0, mesh.GetSurfaceCount())
        .SelectMany(i => mesh.SurfaceGetArrays(i)[(int)Mesh.ArrayType.Vertex].AsVector3Array());
}
