using Godot;

namespace Game;

// An explicit leaf-material allowlist, not a tree-wide or all-mesh deformation.
// Imported geometry/resources stay intact; cached derived lit materials retain
// authored textures, PBR channels, UV transforms and an original-resource link.
internal static class WoodlandMotion
{
    private static readonly Dictionary<(ulong Material, bool Water), ShaderMaterial> Materials = [];
    private static Shader? _shader;
    // Release only after this lifetime's landscape consumers have been freed.
    // Static derived materials otherwise retain authored textures past native
    // RenderingServer teardown in a short-lived component process.
    internal static void ReleaseDerivedMaterials()
    {
        foreach (ShaderMaterial material in Materials.Values) material.Dispose();
        Materials.Clear(); _shader?.Dispose(); _shader = null;
    }
    internal static void Foliage(Node3D node, string asset)
    {
        foreach (MeshInstance3D mesh in node.FindChildren("*", "MeshInstance3D", true, false).OfType<MeshInstance3D>())
            for (int surface = 0; surface < mesh.Mesh.GetSurfaceCount(); surface++)
                if (mesh.GetActiveMaterial(surface) is StandardMaterial3D source && WoodlandEligibility.Foliage(asset, source.ResourceName) && Supported(source))
                {
                    mesh.SetSurfaceOverrideMaterial(surface, Adapt(source, false)); mesh.ExtraCullMargin = .08f;
                    mesh.SetMeta("cosmetic_foliage", true);
                }
    }
    internal static void Water(ArrayMesh mesh, string asset)
    {
        if (!asset.StartsWith("environment/hex_river_overlay_", StringComparison.Ordinal)) return;
        for (int surface = 0; surface < mesh.GetSurfaceCount(); surface++)
            if (mesh.SurfaceGetMaterial(surface) is StandardMaterial3D source && source.ResourceName == "floor_river" && Supported(source))
                mesh.SurfaceSetMaterial(surface, Adapt(source, true));
    }
    private static bool Supported(StandardMaterial3D material) =>
        material.Transparency == BaseMaterial3D.TransparencyEnum.Disabled && material.CullMode == BaseMaterial3D.CullModeEnum.Disabled &&
        !material.EmissionEnabled && !material.AOEnabled && !material.Uv1Triplanar &&
        !material.DetailEnabled && !material.HeightmapEnabled && !material.ClearcoatEnabled && !material.RimEnabled && !material.AnisotropyEnabled &&
        !material.BacklightEnabled && !material.SubsurfScatterEnabled &&
        !material.VertexColorUseAsAlbedo && material.ShadingMode == BaseMaterial3D.ShadingModeEnum.PerPixel;
    private static ShaderMaterial Adapt(StandardMaterial3D source, bool water)
    {
        var key = (source.GetInstanceId(), water);
        if (Materials.TryGetValue(key, out ShaderMaterial? cached)) return cached;
        var result = new ShaderMaterial { Shader = _shader ??= GD.Load<Shader>("res://Shaders/WoodlandMotion.gdshader"), ResourceName = source.ResourceName };
        result.SetMeta("authored_material", source);
        result.SetShaderParameter("albedo_color", source.AlbedoColor);
        if (source.AlbedoTexture is not null) result.SetShaderParameter("albedo_texture", source.AlbedoTexture);
        if (source.RoughnessTexture is not null) result.SetShaderParameter("roughness_texture", source.RoughnessTexture);
        if (source.MetallicTexture is not null) result.SetShaderParameter("metallic_texture", source.MetallicTexture);
        if (source.NormalTexture is not null) result.SetShaderParameter("normal_texture", source.NormalTexture);
        result.SetShaderParameter("normal_enabled", source.NormalEnabled); result.SetShaderParameter("normal_scale", source.NormalScale);
        result.SetShaderParameter("roughness", source.Roughness); result.SetShaderParameter("metallic", source.Metallic); result.SetShaderParameter("specular", source.MetallicSpecular);
        result.SetShaderParameter("uv_scale", source.Uv1Scale); result.SetShaderParameter("uv_offset", source.Uv1Offset);
        result.SetShaderParameter("roughness_channel", Channel((int)source.RoughnessTextureChannel));
        result.SetShaderParameter("metallic_channel", Channel((int)source.MetallicTextureChannel));
        result.SetShaderParameter("wind_surface", !water); result.SetShaderParameter("water_surface", water);
        Materials[key] = result; return result;
    }
    private static Vector4 Channel(int channel) => channel switch { 1 => new(0, 1, 0, 0), 2 => new(0, 0, 1, 0), 3 => new(0, 0, 0, 1), 4 => new(.333333f, .333333f, .333333f, 0), _ => new(1, 0, 0, 0) };
}
