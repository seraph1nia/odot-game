namespace Game;

internal static class WoodlandEligibility
{
    internal static bool Foliage(string asset, string material) => asset switch
    {
        "props/kit_pine.glb" => material is "pine" or "pine_light",
        "environment/components/forest_spiral_tree.glb" => material is "forest_leaf_teal" or "forest_leaf_lilac",
        "environment/components/forest_fern.glb" or "environment/components/forest_leaf_clump.glb" => material is "forest_leaf" or "forest_leaf_light",
        _ => false
    };
}
