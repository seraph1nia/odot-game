using Game.Core;
using Godot;

namespace Game;

// A bounded display formation, not hex occupancy or a second spawn generator.
// Every figure is the authority's manifest entry. The default catalog (including
// all four allocations in one surviving lane) fits the existing approach bounds.
internal static class NextWaveFormation
{
    internal static Dictionary<int, Vector3> Positions(IEnumerable<UnitState> manifest)
    {
        var result = new Dictionary<int, Vector3>();
        foreach (var city in manifest.GroupBy(u => u.Destination))
        {
            UnitState[] units = city.ToArray();
            int columns = Math.Min(6, units.Length);
            int rows = (units.Length + columns - 1) / columns;
            for (int index = 0; index < units.Length; index++)
            {
                // Compress only exceptionally large custom catalogs; never move
                // the camera's bounds or hide/guess part of a forecast.
                float spacing = Math.Min(1.25f, 5f / Math.Max(1, rows - 1));
                result.Add(units[index].Id, CombatLayout.Center(city.Key)
                    + new Vector3((index % columns - (columns - 1) / 2f) * 1.35f, 0, -8 - index / columns * spacing));
            }
        }
        return result;
    }
}
