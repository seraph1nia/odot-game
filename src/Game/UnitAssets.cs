using Godot;

namespace Game;

// Loaded only by graphical presentation. Dedicated and automated roles never call this.
internal static class UnitAssets
{
    public const string Root = "res://Assets/KayKit/Characters/";
    public static readonly string[] RequiredClips = ["Idle", "Walking_A", "Running_A", "1H_Melee_Attack_Slice_Horizontal", "2H_Ranged_Shoot", "Hit_A", "Death_A"];
    private static readonly Dictionary<string, PackedScene> Scenes = [];

    public static Node3D Instantiate(string name)
    {
        if (!Scenes.TryGetValue(name, out PackedScene? scene))
            Scenes[name] = scene = GD.Load<PackedScene>(Root + name) ?? throw new InvalidOperationException("Missing unit asset: " + name);
        return scene.Instantiate<Node3D>();
    }

    public static (AnimationPlayer Player, Skeleton3D Skeleton) Bind(Node3D model)
    {
        AnimationPlayer player = model.FindChildren("*", "AnimationPlayer", true, false).OfType<AnimationPlayer>().Single();
        Skeleton3D skeleton = model.FindChildren("*", "Skeleton3D", true, false).OfType<Skeleton3D>().Single();
        if (skeleton.GetBoneCount() == 0 || skeleton.FindBone("handslot.r") < 0)
            throw new InvalidOperationException("Character rig lacks its hand attachment.");
        foreach (string clip in RequiredClips)
        {
            if (!player.HasAnimation(clip) || player.GetAnimation(clip).GetTrackCount() == 0)
                throw new InvalidOperationException("Missing character animation: " + clip);
            Animation animation = player.GetAnimation(clip);
            for (int track = 0; track < animation.GetTrackCount(); track++)
                if (animation.TrackGetType(track) == Animation.TrackType.Method)
                    throw new InvalidOperationException("Character clips cannot invoke gameplay methods.");
        }
        MeshInstance3D[] meshes = model.FindChildren("*", "MeshInstance3D", true, false).OfType<MeshInstance3D>().ToArray();
        if (!meshes.Any(mesh => mesh.Skin is not null) || meshes.Any(mesh => mesh.Mesh is null || mesh.Mesh.GetSurfaceCount() == 0 || mesh.Mesh.SurfaceGetMaterial(0) is null))
            throw new InvalidOperationException("Character lacks a skinned mesh or original material.");
        player.Stop();
        return (player, skeleton);
    }

    public static string[] Validate(Node parent)
    {
        var reports = new List<string>();
        foreach (string name in new[] { "Knight.glb", "Rogue.glb" })
        {
            Node3D model = Instantiate(name);
            parent.AddChild(model);
            try
            {
                var rig = Bind(model);
                reports.Add($"{name}: bones={rig.Skeleton.GetBoneCount()}, clips={RequiredClips.Length}, hand=handslot.r, skinned/materials");
            }
            finally { parent.RemoveChild(model); model.Free(); }
        }
        foreach (string weapon in new[] { "sword_1handed.gltf", "crossbow_2handed.gltf", "arrow.gltf" })
        {
            Node3D model = Instantiate(weapon);
            try
            {
                if (model.FindChildren("*", "MeshInstance3D", true, false).Count == 0)
                    throw new InvalidOperationException("Weapon has no mesh: " + weapon);
                reports.Add(weapon + ": mesh/material imported");
            }
            finally { model.Free(); }
        }
        return reports.ToArray();
    }
}
