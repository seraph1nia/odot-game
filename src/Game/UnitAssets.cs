using Godot;
using Game.Core;

namespace Game;

// Only graphical consumers instantiate these immutable authored exports.
internal static class UnitAssets
{
    public const string Root = AssetCatalog.Root;
    public static readonly string[] RequiredClips = ["idle", "walk", "run", "hit", "death"];
    public static string AttackClip(UnitType type) => "attack";
    public static string Character(UnitType type, Faction faction) => AssetCatalog.Unit(type, faction).Path;
    private static readonly Dictionary<string, PackedScene> Scenes = [];

    public static Node3D Instantiate(string path)
    {
        if (!AssetCatalog.RequiredPaths.Contains(path, StringComparer.Ordinal)) throw new InvalidOperationException("Unregistered authored asset: " + path);
        if (!Scenes.TryGetValue(path, out PackedScene? scene))
            Scenes[path] = scene = GD.Load<PackedScene>(Root + path) ?? throw new InvalidOperationException("Missing authored asset: " + path);
        return scene.Instantiate<Node3D>();
    }
    public static (AnimationPlayer Player, Skeleton3D Skeleton) Bind(Node3D model, UnitType type = UnitType.Swordsman)
    {
        AnimationPlayer player = model.FindChildren("*", "AnimationPlayer", true, false).OfType<AnimationPlayer>().Single();
        Skeleton3D skeleton = model.FindChildren("*", "Skeleton3D", true, false).OfType<Skeleton3D>().Single();
        foreach (string bone in new[] { "root", "hand.L", "hand.R", "weapon_socket.L", "weapon_socket.R" })
            if (skeleton.FindBone(bone) < 0) throw new InvalidOperationException("Authored rig lacks required bone: " + bone);
        foreach (string clip in RequiredClips.Append(AttackClip(type)))
        {
            if (!player.HasAnimation(clip) || player.GetAnimation(clip).GetTrackCount() == 0)
                throw new InvalidOperationException("Missing authored character animation: " + clip);
            Animation animation = player.GetAnimation(clip);
            animation.LoopMode = clip is "idle" or "walk" or "run" ? Animation.LoopModeEnum.Linear : Animation.LoopModeEnum.None;
            for (int track = 0; track < animation.GetTrackCount(); track++)
                if (animation.TrackGetType(track) == Animation.TrackType.Method)
                    throw new InvalidOperationException("Character clips cannot invoke gameplay methods.");
        }
        MeshInstance3D[] meshes = Meshes(model);
        if (!meshes.Any(mesh => mesh.Skin is not null)) throw new InvalidOperationException("Authored character lacks skinned geometry.");
        ValidateGeometry(model);
        if (player.GetAnimation(AttackClip(type)).Length <= AssetCatalog.AttackMarker)
            throw new InvalidOperationException("Authored attack marker lies outside imported clip.");
        player.Stop();
        return (player, skeleton);
    }
    private static MeshInstance3D[] Meshes(Node3D model) => (model is MeshInstance3D direct ? new[] { direct } : [])
        .Concat(model.FindChildren("*", "MeshInstance3D", true, false).OfType<MeshInstance3D>()).ToArray();
    private static void ValidateGeometry(Node3D model)
    {
        MeshInstance3D[] meshes = Meshes(model);
        if (meshes.Length == 0 || meshes.Any(mesh => mesh.Mesh is null || mesh.Mesh.GetSurfaceCount() == 0
            || Enumerable.Range(0, mesh.Mesh.GetSurfaceCount()).Any(s => mesh.GetActiveMaterial(s) is null)))
            throw new InvalidOperationException("Authored asset lacks original mesh/material data: " + model.Name);
    }
    internal static void Equip(Node3D model, Skeleton3D skeleton, UnitType type, Faction faction)
    {
        // Exported props are already bone-parented with authored transforms. Do
        // not hide them, duplicate them, or rebuild a KayKit-style hand offset.
        UnitVisual visual = AssetCatalog.Unit(type, faction);
        RequireEquipment(model, skeleton, visual.MainProp, visual.MainSocket);
        if (visual.OffhandProp.Length > 0) RequireEquipment(model, skeleton, visual.OffhandProp, visual.OffhandSocket);
    }
    private static BoneAttachment3D RequireEquipment(Node3D model, Skeleton3D skeleton, string equipment, string socket)
    {
        string name = equipment;
        Node3D prop = model.FindChildren(name, "Node3D", true, false).OfType<Node3D>().SingleOrDefault()
            ?? throw new InvalidOperationException("Authored equipped prop missing: " + name);
        BoneAttachment3D? attachment = null;
        for (Node? parent = prop.GetParent(); parent is not null && parent != model; parent = parent.GetParent())
            if (parent is BoneAttachment3D bone) { attachment = bone; break; }
        if (attachment is null || attachment.BoneName != socket || attachment.GetParent() != skeleton || Meshes(prop).Length == 0)
            throw new InvalidOperationException($"Authored equipment {name} lacks imported {socket} attachment; parent={prop.GetParent()?.Name}, bone={attachment?.BoneName}.");
        return attachment;
    }
    internal static bool Equipped(Node3D model, Skeleton3D skeleton, UnitType type, Faction faction)
    { Equip(model, skeleton, type, faction); return true; }
    internal static (bool Aligned, string Rotation) EquipmentPose(Node3D model, Skeleton3D skeleton, UnitType type, Faction faction)
    {
        UnitVisual visual = AssetCatalog.Unit(type, faction);
        BoneAttachment3D main = RequireEquipment(model, skeleton, visual.MainProp, visual.MainSocket);
        bool Aligned(BoneAttachment3D attachment, string socket)
        {
            Transform3D pose = skeleton.GetBoneGlobalPose(skeleton.FindBone(socket));
            return attachment.Transform.Origin.DistanceTo(pose.Origin) < .002f
                && attachment.Transform.Basis.IsEqualApprox(pose.Basis);
        }
        bool aligned = Aligned(main, visual.MainSocket);
        if (visual.OffhandProp.Length > 0)
            aligned &= Aligned(RequireEquipment(model, skeleton, visual.OffhandProp, visual.OffhandSocket), visual.OffhandSocket);
        return (aligned, main.Transform.Basis.GetRotationQuaternion().ToString());
    }
    private static string[]? _installed;
    internal static string[] InstalledModels()
    {
        if (_installed is not null) return _installed;
        var paths = new HashSet<string>(StringComparer.Ordinal);
        void Visit(string path)
        {
            using DirAccess directory = DirAccess.Open(path) ?? throw new InvalidOperationException("Cannot inspect graphical asset distribution: " + path);
            foreach (string folder in directory.GetDirectories()) Visit(path + "/" + folder);
            foreach (string file in directory.GetFiles())
            {
                paths.Add(path + "/" + file);
            }
        }
        Visit("res://Assets");
        _installed = AssetCatalog.InstalledModels(paths); return _installed;
    }
    public static string[] Validate(Node parent)
    {
        var reports = new List<string>();
        foreach (Faction faction in Enum.GetValues<Faction>())
            foreach (UnitType type in Enum.GetValues<UnitType>())
            {
                string path = Character(type, faction); Node3D model = Instantiate(path); parent.AddChild(model);
                try
                {
                    var rig = Bind(model, type); Equip(model, rig.Skeleton, type, faction);
                    reports.Add($"{path}: bones={rig.Skeleton.GetBoneCount()}, clips=6, attack=attack, marker={AssetCatalog.AttackMarker:0.######}, length={rig.Player.GetAnimation("attack").Length:0.######}, sockets/equipment/skinned/materials");
                }
                finally { parent.RemoveChild(model); model.Free(); }
            }
        foreach (string path in AssetCatalog.RequiredPaths)
        {
            Node3D model = Instantiate(path);
            try
            {
                ValidateGeometry(model);
                foreach (string dependency in ResourceLoader.GetDependencies(Root + path))
                {
                    string resolved = dependency.Split("::", StringSplitOptions.None)[^1];
                    if (!resolved.StartsWith(Root, StringComparison.Ordinal))
                        throw new InvalidOperationException("Non-authored 3D dependency: " + path + " -> " + dependency);
                }
                reports.Add(path + ": required authored mesh/material/dependencies imported");
            }
            finally { model.Free(); }
        }
        return reports.ToArray();
    }
}
