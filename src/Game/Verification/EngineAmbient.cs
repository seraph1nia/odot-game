using Game.Core;
using Godot;

namespace Game.Verification;

public partial class EngineTests
{
    // Tier B admission: GLB names/extras alone cannot catch a frozen native
    // consumer, lost pivot/material, or water flattened into a static batch.
    // Uses real production placement and minimal owned nodes, not Main/peers.
    // Automatic advancement is witnessed once; deterministic seeks cover every
    // imported target and seam. Expected cost: seconds in the existing child.
    private async Task Ambient()
    {
        var holder = new Node3D(); AddChild(holder);
        var assets = new LandscapeAssets();
        try
        {
            var paths = Enum.GetValues<Building>().Where(b => b != Building.Empty).Select(b => AssetCatalog.Building(b).Path)
                .Concat(new[] { AssetCatalog.Home, AssetCatalog.Defender, AssetCatalog.Stream })
                .Concat(DetailedGround.Paths.Where(LandscapeAssets.AmbientWater)).Distinct(StringComparer.Ordinal).ToArray();
            var consumers = paths.Select(path => (Path: path, Node: path.StartsWith("buildings/", StringComparison.Ordinal)
                ? assets.Place(holder, path, Vector3.Zero, 1.9f)
                : assets.Native(holder, path, Vector3.Zero, VillageLayout.TerrainScale))).ToArray();
            var players = consumers.Select(c => c.Node.FindChildren("*", "AnimationPlayer", true, false).OfType<AnimationPlayer>().Single()).ToArray();
            double start = players[0].CurrentAnimationPosition;
            // A real idle-frame advance, not just calling Advance in the test.
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(players[0].CurrentAnimationPosition > start && players[0].IsPlaying(), "production ambient player advances on ordinary process frames");
            foreach (var consumer in consumers)
            {
                AnimationPlayer player = consumer.Node.FindChildren("*", "AnimationPlayer", true, false).OfType<AnimationPlayer>().Single();
                string name = player.CurrentAnimation;
                Animation animation = player.GetAnimation(name);
                Check(name == "ambient" || name.EndsWith("/ambient", StringComparison.Ordinal), "actual imported ambient clip selected: " + consumer.Path);
                Check(animation.LoopMode == Godot.Animation.LoopModeEnum.Linear && player.SpeedScale == 1 && animation.Length > 4 && animation.Length < 4.1,
                    "source loop duration and cosmetic elapsed-second speed: " + consumer.Path);
                Node root = player.GetNode(player.RootNode);
                var targets = new HashSet<Node3D>();
                for (int track = 0; track < animation.GetTrackCount(); track++)
                {
                    Check(animation.TrackGetType(track) is Godot.Animation.TrackType.Position3D or Godot.Animation.TrackType.Rotation3D or Godot.Animation.TrackType.Scale3D,
                        "ambient has only bounded object transforms, no callbacks/gameplay: " + consumer.Path);
                    NodePath path = animation.TrackGetPath(track);
                    targets.Add(root.GetNode<Node3D>(new NodePath(path.GetConcatenatedNames())));
                }
                Check(targets.Count > 0 && targets.All(t => t != consumer.Node && (t is MeshInstance3D || t.GetChildCount() > 0)), "localized mesh/pivot targets resolve inside native hierarchy");
                // Godot resamples at the existing import FPS; loop closure is
                // its actual zero/end boundary, not a fractional source key that
                // may interpolate between two resampled keys.
                Console.WriteLine($"MEASURE ambient: {consumer.Path}; clip={name}; length={animation.Length:R}; targets={targets.Count}");
                player.Seek(0, true);
                var rest = consumer.Node.FindChildren("*", "Node3D", true, false).OfType<Node3D>().Prepend(consumer.Node).ToDictionary(n => n, n => n.Transform);
                player.Seek(25 / 24.0, true);
                Check(targets.Any(t => !t.Transform.IsEqualApprox(rest[t])), "actual localized transform changes at source extremum: " + consumer.Path);
                Check(rest.Where(p => !targets.Contains(p.Key)).All(p => p.Key.Transform.IsEqualApprox(p.Value)), "structural local/rest transforms remain fixed: " + consumer.Path);
                player.Seek(animation.Length, true);
                Check(rest.All(p => p.Key.Transform.IsEqualApprox(p.Value)), "actual imported loop endpoints close: " + consumer.Path);
                player.Seek(animation.Length - .01, true); player.Advance(.1);
                Check(player.IsPlaying() && player.CurrentAnimationPosition < .2, "actual playback wraps instead of stopping: " + consumer.Path);
                if (LandscapeAssets.AmbientWater(consumer.Path))
                {
                    var movingMeshes = targets.OfType<MeshInstance3D>().Concat(targets.SelectMany(t => t.FindChildren("*", "MeshInstance3D", true, false).OfType<MeshInstance3D>())).ToArray();
                    Check(movingMeshes.Length == 1 && movingMeshes[0].Mesh.GetFaces().Length / 3 == 8, "one tiny native eight-triangle shimmer, no moving whole sheet");
                    MeshInstance3D mesh = movingMeshes[0];
                    Check(mesh.GetActiveMaterial(0) is StandardMaterial3D { AlbedoTexture: not null, RoughnessTexture: not null } material
                        && material.RoughnessTextureChannel == BaseMaterial3D.TextureChannel.Green && mesh.MaterialOverride is null,
                        "animated water retains actual authored color/roughness binding, not an unbound shader substitute");
                }
            }
            // Actual shared menu/city landscape installs the watercourse natively,
            // not only the direct resource test above. Fixed bridge intentionally
            // removes incompatible recessed water; its surface river still loops.
            var landscape = new VillageLandscape(holder, assets);
            var camera = new Camera3D { Position = new(18, 22, 30), Projection = Camera3D.ProjectionType.Orthogonal, Size = 28 };
            holder.AddChild(camera); camera.LookAt(Vector3.Zero);
            landscape.Cover(camera, new Rect2(0, 0, 1100, 820), Vector2.Zero);
            Node3D[] water = landscape.FindChildren("*", "Node3D", true, false).OfType<Node3D>()
                .Where(n => n.HasMeta("asset") && LandscapeAssets.AmbientWater(n.GetMeta("asset").AsString())).ToArray();
            Check(water.Length > 0 && water.Length <= DetailedGround.Watercourse().Count(), "only bounded installed watercourse uses native animation owners");
            Check(water.All(n => n.FindChildren("*", "AnimationPlayer", true, false).OfType<AnimationPlayer>().Single().IsPlaying()), "every actual landscape water consumer plays its loop");
            Check(water.SelectMany(n => n.FindChildren("*", "GeometryInstance3D", true, false).OfType<GeometryInstance3D>())
                .All(n => n.CastShadow == GeometryInstance3D.ShadowCastingSetting.Off), "native water preserves the former batches' shadowless cost policy");
            var homePlayers = landscape.FindChildren("*", "AnimationPlayer", true, false).OfType<AnimationPlayer>().Where(p => p.IsPlaying()).ToArray();
            Check(homePlayers.Length == water.Length + 2, "fixed home and invulnerable defender also animate; stripped bridge does not play removed tracks");
            int count = water.Length;
            landscape.Cover(camera, new Rect2(0, 0, 1100, 820), Vector2.Zero);
            Check(landscape.FindChildren("*", "AnimationPlayer", true, false).OfType<AnimationPlayer>().Count(p => p.IsPlaying()) == count + 2,
                "repeat cover does not duplicate ambient consumers");
        }
        finally { RemoveChild(holder); holder.Free(); }
    }
}
