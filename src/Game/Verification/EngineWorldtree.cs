using Game.Core;
using Godot;

namespace Game.Verification;

public partial class EngineTests
{
    // Tier B: imported resources, GDScript signals/tweens/native focus and real
    // game adapter binding cannot execute in xUnit. One minimal SceneTree; no
    // Main, renderer, menu, game peers or graphical process. Arithmetic is Tier A.
    private async Task Worldtree()
    {
        var host = new Control { Size = new(1280, 720) }; AddChild(host);
        var outside = new Button { Text = "Outside", Name = "OutsideResearch" }; host.AddChild(outside);
        var dialog = new WorldtreeDialog { Invoker = outside }; host.AddChild(dialog);
        var second = new WorldtreeResearch(); host.AddChild(second); second.Hide();
        try
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Control tree = dialog.Research.Tree;
            var topology = tree.Get("topology").AsGodotDictionary();
            var buttons = tree.Get("buttons").AsGodotDictionary();
            var edges = tree.Get("edges").AsGodotDictionary();
            Check(topology["nodes"].AsGodotArray().Count == 24 && topology["edges"].AsGodotArray().Count == 23 && buttons.Count == 24 && edges.Count == 23,
                "actual imported JSON and native canonical 24/23 modules load");
            foreach (Variant icon in topology["icon_catalog"].AsGodotArray())
                Check(GD.Load<Texture2D>("res://UI/art/worldtree/icons/" + icon.AsString() + ".svg") is not null, "imported icon: " + icon.AsString());
            using var match = new Match(); match.Join();
            Check(match.Apply(1, new(1, match.Id, match.Phase, match.TurnSerial, "start", 1)).Accepted, "adapter fixture starts through numerical authority");
            match.Players[1].Research = new(30);
            void Refresh() { var state = match.Snapshot(); dialog.Research.Update(state, state.Players[0], 1, true); }
            dialog.Research.Requested = id =>
            {
                Check(match.Apply(1, new(1, match.Id, match.Phase, match.TurnSerial, "research-tech", 1, Technology: id)).Accepted, "actual intent invokes existing authority command");
            };
            Refresh(); dialog.Open();
            var prior = tree.Get("snapshot").AsGodotDictionary().Duplicate(true);
            tree.Call("select_node", "primary_left");
            Check(dialog.Research.Description.Text.Contains("Melee Foundation", StringComparison.Ordinal), "native selection projects real selected-node description");
            tree.Call("request_action");
            Check(match.Players[1].Research.Has(TechnologyId.MeleeFoundation), "intent callback acknowledges real research purchase");
            Check(tree.Get("snapshot").AsGodotDictionary()["acquired"].AsGodotArray().Count == prior["acquired"].AsGodotArray().Count,
                "presentation intent never changes acquired state before authority snapshot");
            Refresh();
            await ToSignal(tree, "awakening_finished");
            tree.Call("select_node", "secondary_left_a"); tree.Call("request_action"); Refresh();
            ulong tween = tree.Get("animation").As<Tween>().GetInstanceId();
            Refresh(); Refresh();
            dialog.Research.Update(match.Snapshot(), match.Players[1].Snapshot(), 1, true, "Authoritative command feedback");
            Check(tree.Get("animation").As<Tween>().GetInstanceId() == tween && dialog.Research.Feedback.Text == "Authoritative command feedback", "unchanged tick and feedback snapshots preserve active child tween while showing real feedback");
            var awakening = edges["secondary_left_a"].AsGodotDictionary();
            var sibling = edges["secondary_left_b"].AsGodotDictionary();
            Check(awakening["sap"].As<Control>().Get("progress").AsDouble() == 0 && awakening["fresh"].As<TextureRect>().Modulate.A == 0,
                "new child begins its own sap before foliage");
            Check(sibling["sap"].As<Control>().Get("progress").AsDouble() == 0 && sibling["fresh"].As<TextureRect>().Modulate.A == 0,
                "unrelated sibling stays dormant during child awakening");
            Check(second.Tree.Get("snapshot").AsGodotDictionary()["acquired"].AsGodotArray().Count == 0, "separate instance stays unchanged");
            await ToSignal(tree, "awakening_finished");
            Check(awakening["sap"].As<Control>().Get("progress").AsDouble() == 1 && awakening["fresh"].As<TextureRect>().Modulate.A == 1
                && sibling["sap"].As<Control>().Get("progress").AsDouble() == 0, "actual child tween finishes independently");
            tree.Call("select_node", "terminal_left_a_2"); tree.Call("request_action"); Refresh();
            Check(dialog.Research.Description.Text.Contains("stone per building turn", StringComparison.Ordinal)
                && tree.Get("points_label").As<Label>().Text.StartsWith("Research points", StringComparison.Ordinal), "adapter displays actual production benefit and existing research currency");
            await ToSignal(tree, "awakening_finished");
            var snapshot = tree.Get("snapshot").AsGodotDictionary().Duplicate(true);
            var malformed = snapshot.Duplicate(true); malformed["points"] = -1;
            Check(!tree.Call("set_data", malformed).AsBool() && tree.Get("snapshot").AsGodotDictionary()["points"].AsInt32() == snapshot["points"].AsInt32(), "malformed native snapshot rejected atomically");
            malformed = snapshot.Duplicate(true); malformed["icons"].AsGodotDictionary().Remove("root");
            Check(!tree.Call("set_data", malformed).AsBool(), "incomplete native icon map rejected");
            tree.Call("select_node", "terminal_left_b_2");
            Check(tree.Get("action").As<Button>().Disabled && tree.Get("action").As<Button>().FocusMode == Control.FocusModeEnum.None
                && dialog.Research.Description.Text.Contains("Inactive slot", StringComparison.Ordinal), "inactive slot stays inspectable but cannot request research or trap focus");
            int requests = 0; dialog.Research.Requested = _ => requests++; tree.Call("request_action"); Check(requests == 0, "inactive intent cannot escape into game commands");
            dialog.Close();
            // Unconfined imported component retains real native host-boundary links.
            second.Show(); second.Update(match.Snapshot(), match.Players[1].Snapshot(), 1, true);
            var standaloneButtons = second.Tree.Get("buttons").AsGodotDictionary();
            var first = standaloneButtons["root"].As<Button>(); var last = standaloneButtons["terminal_right_b_2"].As<Button>();
            Check(first.FocusPrevious.IsEmpty && last.FocusNext.IsEmpty,
                "component boundary links stay open for native host traversal when footer is disabled");
            outside.FocusNext = first.GetPath(); outside.FocusPrevious = last.GetPath();
            outside.GrabFocus(); Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = true });
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(GetViewport().GuiGetFocusOwner() == first, "native host Tab enters actual first tree node");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = false });
            standaloneButtons["primary_center"].As<Button>().GrabFocus(); Input.ParseInputEvent(new InputEventKey { Keycode = Key.Enter, Pressed = true });
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Enter, Pressed = false });
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(second.Description.Text.Contains("Magic Foundation", StringComparison.Ordinal), "native Enter press/release inspects focused tree node through actual signal");
            last.GrabFocus(); Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = true });
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(GetViewport().GuiGetFocusOwner() == second.Tree.Get("action").As<Button>(), "native Tab reaches available request footer");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = false });
            TechnologyId intent = TechnologyId.None; second.Requested = id => intent = id;
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Enter, Pressed = true });
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Enter, Pressed = false });
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(intent == TechnologyId.MagicFoundation && !match.Players[1].Research.Has(intent), "native footer Enter press/release emits intent without allocating locally");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = true });
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(GetViewport().GuiGetFocusOwner() == outside, "native forward Tab escapes component into host");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = false });
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, ShiftPressed = true, Pressed = true });
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(GetViewport().GuiGetFocusOwner() == last, "native reverse Tab re-enters actual last tree node");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, ShiftPressed = true, Pressed = false });
            first.GrabFocus(); Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, ShiftPressed = true, Pressed = true });
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(GetViewport().GuiGetFocusOwner() == outside, "native reverse Tab escapes first node into host");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, ShiftPressed = true, Pressed = false });
            dialog.Open();
            buttons["terminal_right_b_2"].As<Button>().GrabFocus();
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = true });
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Control focus = GetViewport().GuiGetFocusOwner();
            Check(focus is not null && dialog.IsAncestorOf(focus), "visible game modal confines actual native Tab");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = false });
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, ShiftPressed = true, Pressed = true });
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(dialog.IsAncestorOf(GetViewport().GuiGetFocusOwner()), "visible game modal confines reverse Tab");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, ShiftPressed = true, Pressed = false });
            bool settings = false; dialog.OpenSettings = () => settings = true; dialog.ChildModalOpen = () => settings;
            dialog.SettingsButton.EmitSignal(BaseButton.SignalName.Pressed); Check(settings, "real Settings button calls existing host callback");
            tree.Call("select_node", "primary_right"); tree.Call("request_action");
            dialog.CloseButton.EmitSignal(BaseButton.SignalName.Pressed);
            Check(requests == 0 && dialog.Visible, "child Settings guard blocks underlying purchase and Close callbacks");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Escape, Pressed = true });
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(dialog.Visible, "research host does not steal child Settings Escape");
            settings = false; Input.ParseInputEvent(new InputEventKey { Keycode = Key.Escape, Pressed = false });
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Escape, Pressed = true });
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(!dialog.Visible, "native Escape closes only visible research modal");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Escape, Pressed = false });
            host.Size = new(1600, 900); dialog.Open();
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(tree.Get("buttons").AsGodotDictionary().Count == 24 && tree.Get("edges").AsGodotDictionary().Count == 23, "resize preserves physical tree/control identities");
            var state = match.Snapshot(); dialog.Research.Update(state, state.Players[0], 1, false);
            Check(tree.Get("snapshot").AsGodotDictionary()["available"].AsGodotArray().Count == 0 && match.Players[1].Research.Has(TechnologyId.Stonecraft), "disconnection disables requests without wiping progress");
            // Free during a real bound tween; SceneTree must emit no freed-object
            // errors or leaked tweens. Runner checks engine stderr on process exit.
            dialog.Research.Update(state, state.Players[0], 1, true);
            tree.Call("select_node", "primary_right");
            Check(match.Apply(1, new(1, match.Id, match.Phase, match.TurnSerial, "research-tech", 1, Technology: TechnologyId.RangedFoundation)).Accepted, "lifetime fixture purchases through authority");
            Refresh(); dialog.QueueFree();
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(!GodotObject.IsInstanceValid(dialog), "actual awakening component freed with modal lifetime");
        }
        finally { RemoveChild(host); host.Free(); }
    }
}
