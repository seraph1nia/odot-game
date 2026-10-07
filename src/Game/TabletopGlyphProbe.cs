using Game.Core;
using Godot;

namespace Game;

// Read-only access to the existing live city/owner references. This diagnostic
// never names, reparents or reauthors production labels or their owners.
public partial class Tabletop
{
    internal object GlyphIdentity(Label3D label)
    {
        static float[] V(Vector3 v) => [v.X, v.Y, v.Z];
        foreach ((int id, Node3D board) in _boards)
        {
            CityState? city = game.State?.Players.SingleOrDefault(c => c.Id == id);
            if (city is null) continue;
            if (label.GetParent() == board.GetNodeOrNull("Buildings"))
            {
                for (int slot = 0; slot < city.Slots.Length; slot++)
                {
                    SlotState state = city.Slots[slot];
                    Node3D? owner = board.GetNodeOrNull<Node3D>("Buildings/Slot" + slot);
                    if (state.Type == Building.Empty || owner is null || _buildingBounds[id][slot] is not { } bounds) continue;
                    Vector3 position = new(SlotPosition(slot).X, bounds.End.Y + .28f, SlotPosition(slot).Z);
                    string text = state.Type == Building.TownHall ? "Town hall" : $"{AssetCatalog.BuildingName(state.Type)} L{state.Level}";
                    string asset = AssetCatalog.Building(state.Type).Path;
                    if (label.Position != position || label.Text != text || label.FontSize != 20 || !owner.HasMeta("asset") || owner.GetMeta("asset").AsString() != asset) continue;
                    return new
                    {
                        Matched = true,
                        Key = $"city:{id}/slot:{slot}/title",
                        City = id,
                        Slot = slot,
                        Building = state.Type.ToString(),
                        state.Level,
                        Asset = asset,
                        Protected = state.Type is Building.ArrowTower or Building.CatapultTower or Building.TownHall,
                        ExpectedText = text,
                        ExpectedLocal = V(position),
                        OwnerGlobal = V(owner.GlobalPosition),
                        BeforeBasis = "identical retained input/state/physical controls and immutable Tabletop/AssetCatalog authoring; not original GPU glyph readback"
                    };
                }
            }
            if (label.GetParent() == board.GetNodeOrNull("ArmyHomes") && city.Army is { } army && game.State is { } snapshot)
            {
                for (int index = 0; index < army.Homes.Length; index++)
                {
                    ArmyHomeState home = army.Homes[index];
                    HexCoordinate coordinate = snapshot.Rules.Combat.Board.Cells.Single(c => c.Id == home.Cell).Coordinate;
                    Vector3 position = VillageLayout.Hex(coordinate.Column, coordinate.R) + new Vector3(0, .12f, .95f);
                    string text = $"Space {index + 1} · {(home.Purchased ? "6 size" : "locked")}";
                    if (label.Name != "Home" + index || label.Text != text || label.FontSize != 18 || label.Position != position) continue;
                    return new
                    {
                        Matched = true,
                        Key = $"city:{id}/home:{index}/title",
                        City = id,
                        Home = index,
                        home.Cell,
                        home.Purchased,
                        Protected = true,
                        ExpectedText = text,
                        ExpectedLocal = V(position),
                        BeforeBasis = "identical retained input/state/physical controls and immutable ArmyPanel/HexBoard authoring; not original GPU glyph readback"
                    };
                }
            }
        }
        return new { Matched = false, Key = "unresolved", Reason = "No unique live city/slot/home authoring reference matched actual title/fontsize/local transform/owner asset." };
    }
}
