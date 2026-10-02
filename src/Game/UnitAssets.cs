using Godot;
using Game.Core;

namespace Game;

// Loaded only by graphical presentation. Dedicated and automated roles never call this.
internal static class UnitAssets
{
    public const string Root = "res://Assets/KayKit/Characters/";
    public static readonly string[] RequiredClips = ["Idle", "Walking_A", "Running_A", "Hit_A", "Death_A"];
    public static string AttackClip(UnitType type) => type switch
    { UnitType.Berserker => "2H_Melee_Attack_Chop", UnitType.Crossbowman => "2H_Ranged_Shoot", UnitType.Mage => "Spellcast_Shoot", _ => "1H_Melee_Attack_Slice_Horizontal" };
    public static string Character(UnitType type, Faction faction) => faction == Faction.Skeletons ? type switch
    { UnitType.Berserker => "Skeleton_Minion.glb", UnitType.Crossbowman => "Skeleton_Rogue.glb", UnitType.Mage => "Skeleton_Mage.glb", _ => "Skeleton_Warrior.glb" }
        : type switch { UnitType.Berserker => "Barbarian.glb", UnitType.Crossbowman => "Rogue.glb", UnitType.Mage => "Mage.glb", _ => "Knight.glb" };
    public static string Weapon(UnitType type, Faction faction) => faction == Faction.Skeletons ? type switch
    { UnitType.Berserker => "Skeleton_Axe.gltf", UnitType.Crossbowman => "Skeleton_Crossbow.gltf", UnitType.Mage => "Skeleton_Staff.gltf", _ => "Skeleton_Blade.gltf" }
        : type switch { UnitType.Berserker => "axe_2handed.gltf", UnitType.Crossbowman => "crossbow_2handed.gltf", UnitType.Mage => "staff.gltf", _ => "sword_1handed.gltf" };
    private static readonly Dictionary<string, PackedScene> Scenes = [];

    public static Node3D Instantiate(string name)
    {
        if (!Scenes.TryGetValue(name, out PackedScene? scene))
            Scenes[name] = scene = GD.Load<PackedScene>(Root + name) ?? throw new InvalidOperationException("Missing unit asset: " + name);
        return scene.Instantiate<Node3D>();
    }

    public static (AnimationPlayer Player, Skeleton3D Skeleton) Bind(Node3D model, UnitType type = UnitType.Swordsman)
    {
        AnimationPlayer player = model.FindChildren("*", "AnimationPlayer", true, false).OfType<AnimationPlayer>().Single();
        Skeleton3D skeleton = model.FindChildren("*", "Skeleton3D", true, false).OfType<Skeleton3D>().Single();
        if (skeleton.GetBoneCount() == 0 || skeleton.FindBone("handslot.r") < 0 || skeleton.FindBone("root") < 0)
            throw new InvalidOperationException("Character rig lacks its hand attachment.");
        foreach (string clip in RequiredClips.Append(AttackClip(type)))
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
        if (player.GetAnimation(AttackClip(type)).Length <= CombatPlayback.ImpactMarker(type))
            throw new InvalidOperationException("Attack marker is outside its imported clip.");
        player.Stop();
        return (player, skeleton);
    }

    internal static void Equip(Node3D model, Skeleton3D skeleton, UnitType type, Faction faction)
    {
        // The character files include optional props; equip only our selected weapon.
        foreach (MeshInstance3D mesh in model.FindChildren("*", "MeshInstance3D", true, false).OfType<MeshInstance3D>())
            if (mesh.Name.ToString().Contains("Sword", StringComparison.Ordinal) || mesh.Name.ToString().Contains("Shield", StringComparison.Ordinal)
                || mesh.Name.ToString().Contains("Crossbow", StringComparison.Ordinal) || mesh.Name.ToString().Contains("Dagger", StringComparison.Ordinal)
                || mesh.Name.ToString().Contains("Bow", StringComparison.Ordinal) || mesh.Name.ToString().Contains("Arrow", StringComparison.Ordinal) || mesh.Name.ToString().Contains("Knife", StringComparison.Ordinal)
                || mesh.Name.ToString().Contains("Throwable", StringComparison.Ordinal) || mesh.Name.ToString().Contains("Staff", StringComparison.Ordinal) || mesh.Name.ToString().Contains("Axe", StringComparison.Ordinal)) mesh.Visible = false;
        var hand = new BoneAttachment3D { BoneName = "handslot.r" }; skeleton.AddChild(hand);
        hand.AddChild(UnitAssets.Instantiate(UnitAssets.Weapon(type, faction)));
    }

    public static string[] Validate(Node parent)
    {
        var reports = new List<string>();
        foreach (Faction faction in Enum.GetValues<Faction>())
            foreach (UnitType type in Enum.GetValues<UnitType>())
            {
                string name = Character(type, faction);
                Node3D model = Instantiate(name);
                parent.AddChild(model);
                try
                {
                    var rig = Bind(model, type);
                    reports.Add($"{name}: bones={rig.Skeleton.GetBoneCount()}, clips={RequiredClips.Length + 1}, attack={AttackClip(type)}, marker={CombatPlayback.ImpactMarker(type):0.###}, length={rig.Player.GetAnimation(AttackClip(type)).Length:0.###}, hand=handslot.r, skinned/materials");
                }
                finally { parent.RemoveChild(model); model.Free(); }
            }
        foreach (string weapon in Enum.GetValues<Faction>().SelectMany(f => Enum.GetValues<UnitType>().Select(t => Weapon(t, f))).Append("arrow.gltf").Append("Skeleton_Arrow.gltf").Distinct())
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
