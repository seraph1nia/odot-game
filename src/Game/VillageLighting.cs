using Godot;

namespace Game;

// A shared presentation rig, not a repaint of immutable authored materials.
internal static class VillageLighting
{
    internal static Godot.Environment Environment(bool studio = false) => new()
    {
        BackgroundMode = studio ? Godot.Environment.BGMode.ClearColor : Godot.Environment.BGMode.Color,
        BackgroundColor = new("a7c4c2"),
        AmbientLightSource = Godot.Environment.AmbientSource.Color,
        AmbientLightColor = new("d6e0e4"),
        AmbientLightEnergy = studio ? .45f : .24f,
        TonemapMode = Godot.Environment.ToneMapper.Linear
    };
    internal static DirectionalLight3D Sun(bool studio = false) => new()
    {
        RotationDegrees = new(-55, -25, 0),
        LightColor = new("fff2da"),
        LightEnergy = studio ? 1f : .4f,
        ShadowEnabled = !studio,
        DirectionalShadowMaxDistance = 65
    };
}
