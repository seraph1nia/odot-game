using Godot;

namespace Game;

// Owns exactly one input-transparent control per sampled rig. Squad members
// keep independent HP in stable compact slots; no second squad-level overlay.
internal sealed class MemberHealthBars(Control root)
{
    private readonly Dictionary<int, UnitHealthBar> _bars = [];
    internal void Clear()
    {
        foreach (UnitHealthBar bar in _bars.Values) { root.RemoveChild(bar); bar.QueueFree(); }
        _bars.Clear();
    }
    internal void Sample(IReadOnlyDictionary<int, UnitView> views, CombatLayout layout, double tick, int capacity, Camera3D camera, Rect2 world)
    {
        foreach (int id in _bars.Keys.Except(views.Keys).ToArray())
        { root.RemoveChild(_bars[id]); _bars[id].QueueFree(); _bars.Remove(id); }
        foreach ((int id, UnitView view) in views)
        {
            bool grouped = id > 0 && view.State.Hex is not null;
            if (!_bars.TryGetValue(id, out UnitHealthBar? bar))
            { bar = new(grouped ? new Vector2(16, 7) : new Vector2(26, 8)) { Name = "MemberHealth" + id }; root.AddChild(bar); _bars.Add(id, bar); }
            Vector3? anchor = grouped
                ? layout.SquadPosition(view.State.Hex!, view.State.Hex!.FrozenTick ?? tick) + Vector3.Up * (view.State.IsBoss ? 2.5f : 1.6f) : null;
            Vector2 offset = grouped ? UnitHealthBar.FormationOffset(view.State.FormationSlot, view.State.IsBoss ? 1 : capacity) : default;
            bar.Sample(view, camera, world, anchor, offset);
        }
    }
    internal object[] Observe(Transform2D screen) => _bars.OrderBy(p => p.Key).Select(p => p.Value.Observe(p.Key, screen)).ToArray();
}
