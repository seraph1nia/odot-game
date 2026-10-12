using Game.Core;
using Godot;

namespace Game.Verification;

public partial class EngineTests
{
    // Tier B: imported resources, GDScript signals/tweens/native focus and real
    // game adapter binding cannot execute in xUnit. One minimal SceneTree; no
    // Main, renderer, menu, game peers or graphical process. Arithmetic is Tier A.
    private async Task HoverResearch(WorldtreeDialog dialog, Button socket, bool panelOnRight)
    {
        dialog.Research.FocusRoot(false);
        MoveResearchPointer(new(4, 4)); await Frames();
        string selected = dialog.Research.Tree.Get("snapshot").AsGodotDictionary()["selected"].AsString();
        Rect2 node = socket.GetGlobalTransformWithCanvas() * new Rect2(Vector2.Zero, socket.Size);
        MoveResearchPointer(node.GetCenter()); await Frames();
        Check(dialog.Research.HoverDetailsVisible && !socket.HasFocus()
            && dialog.Research.Tree.Get("snapshot").AsGodotDictionary()["selected"].AsString() == selected,
            "native socket hover opens details without clicking, selecting or focusing it");
        Rect2 panel = dialog.Research.HoverDetailsRect;
        float near = panelOnRight ? node.End.X : panel.End.X;
        float far = panelOnRight ? panel.Position.X : node.Position.X;
        float top = Math.Max(node.Position.Y, panel.Position.Y), bottom = Math.Min(node.End.Y, panel.End.Y);
        Check(far > near && bottom > top, "actual native placement has a traversable gap on the expected side");
        Vector2 gap = new((near + far) / 2, (top + bottom) / 2);
        MoveResearchPointer(gap); await Frames();
        Check(GetViewport().GetMousePosition().IsEqualApprox(gap), "native viewport records the actual transit input position");
        GD.Print($"HOVER node={node} panel={panel} pointer={GetViewport().GetMousePosition()} side={(panelOnRight ? "right" : "left")} visible={dialog.Research.HoverDetailsVisible}");
        Check(dialog.Research.HoverDetailsVisible && !socket.HasFocus(), "hover-only details survive frames inside the native transit gap");
        MoveResearchPointer(new(panelOnRight ? panel.Position.X + 2 : panel.End.X - 2, gap.Y)); await Frames();
        Check(dialog.Research.HoverDetailsVisible, "native pointer enters the actual panel without losing details");
        var action = dialog.Research.Tree.Get("action").As<Button>();
        MoveResearchPointer(action.GetGlobalTransformWithCanvas() * (action.Size / 2)); await Frames();
        Check(dialog.Research.HoverDetailsVisible, "native Research action remains reachable from hover alone");
    }
    private void MoveResearchPointer(Vector2 position)
    {
        Vector2 native = GetViewport().GetFinalTransform() * position;
        Input.ParseInputEvent(new InputEventMouseMotion { Position = native, GlobalPosition = native });
    }

    private async Task Worldtree()
    {
        var canvas = new CanvasLayer(); AddChild(canvas);
        var host = new WorldtreeInputHost { Size = new(GetViewport().GetVisibleRect().Size.X, 620) }; canvas.AddChild(host);
        var outside = new Button { Text = "Outside", Name = "OutsideResearch" }; host.AddChild(outside);
        int outsideClicks = 0; outside.Pressed += () => outsideClicks++;
        var dialog = new WorldtreeDialog { Invoker = outside }; host.AddChild(dialog);
        GetWindow().GuiEmbedSubwindows = true;
        var embedded = new AcceptDialog { Name = "EmbeddedModal", Position = new(700, 700), Size = new(240, 100), Transient = true, Exclusive = false, Visible = false };
        host.AddChild(embedded);
        host.ModalCandidates = [embedded, dialog];
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
            void Click(Control control)
            {
                Vector2 position = control.GetGlobalTransformWithCanvas() * (control.Size / 2);
                GetViewport().PushInput(new InputEventMouseMotion { Position = position, GlobalPosition = position }, true);
                GetViewport().PushInput(new InputEventMouseButton { Position = position, GlobalPosition = position, ButtonIndex = MouseButton.Left, Pressed = true }, true);
                GetViewport().PushInput(new InputEventMouseButton { Position = position, GlobalPosition = position, ButtonIndex = MouseButton.Left, Pressed = false }, true);
            }
            void WorldKey()
            {
                GetViewport().PushInput(new InputEventKey { Keycode = Key.F9, Pressed = true }, true);
                GetViewport().PushInput(new InputEventKey { Keycode = Key.F9, Pressed = false }, true);
            }
            Refresh(); dialog.Open();
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(GameApplication.SelectModal(host.ModalCandidates) == dialog, "visible research is selected over hidden embedded window");
            WorldKey();
            Check(!host.ConsumedBeforeGui && host.WorldActions == 0, "research selection allows GUI dispatch while blocking unhandled world actions");
            embedded.Show();
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(GameApplication.SelectModal(host.ModalCandidates) == embedded, "visible embedded window takes priority over research");
            Click(buttons["primary_left"].As<Button>());
            Check(host.ConsumedBeforeGui && tree.Get("snapshot").AsGodotDictionary()["selected"].AsString() == "root", "embedded-window outside mouse events are consumed before native research selection");
            WorldKey();
            Check(host.WorldActions == 0, "embedded-window selection blocks underlying world actions");
            embedded.Hide();
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Click(outside);
            Check(outsideClicks == 0, "research scrim blocks native clicks on underlying controls");
            await HoverResearch(dialog, buttons["primary_left"].As<Button>(), panelOnRight: true);
            MoveResearchPointer(new(4, 4)); await Frames();
            Check(!dialog.Research.HoverDetailsVisible, "genuine leave dismisses hover details despite root retaining keyboard focus");
            await HoverResearch(dialog, buttons["primary_right"].As<Button>(), panelOnRight: false);
            MoveResearchPointer(buttons["terminal_right_a_2"].As<Button>().GetGlobalTransformWithCanvas()
                * (buttons["terminal_right_a_2"].As<Button>().Size / 2)); await Frames();
            Check(dialog.Research.HoverDetailsVisible && tree.Get("action").As<Button>().Disabled
                && dialog.Research.Description.Text.Contains("Inactive slot", StringComparison.Ordinal), "moving to another native socket coherently replaces hover details and eligibility");
            MoveResearchPointer(new(4, 4)); await Frames();
            Check(!dialog.Research.HoverDetailsVisible, "inactive hover also dismisses on genuine leave");
            canvas.Transform = new Transform2D(0, new Vector2(12, 8)); await Frames();
            await HoverResearch(dialog, buttons["primary_left"].As<Button>(), panelOnRight: true);
            var prior = tree.Get("snapshot").AsGodotDictionary().Duplicate(true);
            Click(buttons["primary_left"].As<Button>());
            Check(dialog.Research.Description.Text.Contains("Melee Foundation", StringComparison.Ordinal), "native socket click reaches GUI through application input boundary");
            // With duplicate bottom information removed, a 620px host can now fit
            // the tree. Force a genuinely constrained native scroll fixture.
            host.Size = new(host.Size.X, 520);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            var scroll = dialog.FindChildren("*", "ScrollContainer", true, false).OfType<ScrollContainer>().Single();
            scroll.ScrollVertical = 0;
            Vector2 wheelPosition = scroll.GetGlobalTransformWithCanvas() * new Vector2(20, 20);
            GetViewport().PushInput(new InputEventMouseButton { Position = wheelPosition, GlobalPosition = wheelPosition, ButtonIndex = MouseButton.WheelDown, Pressed = true }, true);
            GetViewport().PushInput(new InputEventMouseButton { Position = wheelPosition, GlobalPosition = wheelPosition, ButtonIndex = MouseButton.WheelDown, Pressed = false }, true);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(scroll.ScrollVertical > 0, "native wheel reaches modal scroll through application input boundary");
            await HoverResearch(dialog, buttons["primary_right"].As<Button>(), panelOnRight: false);
            Rect2 beforeScroll = dialog.Research.HoverDetailsRect;
            buttons["primary_right"].As<Button>().GrabFocus();
            scroll.ScrollVertical = 0; await Frames();
            Check(dialog.Research.HoverDetailsVisible && dialog.Research.HoverDetailsRect != beforeScroll,
                "current native scroll geometry repositions the same keyboard-owned details");
            host.Size = new(host.Size.X, 620);
            await Frames();
            await HoverResearch(dialog, buttons["primary_left"].As<Button>(), panelOnRight: true);
            tree.Get("action").As<Button>().GrabFocus();
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Enter, Pressed = true });
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Enter, Pressed = false });
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
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
                && !tree.Get("points_label").As<Label>().Visible && !tree.GetNode<Control>("SelectedNodeFooter").Visible,
                "single hover surface displays benefit without duplicate currency/footer information");
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
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Click(dialog.CloseButton);
            Check(!dialog.Visible, "native Close click reaches GUI through application input boundary");
            Check(GameApplication.SelectModal(host.ModalCandidates) is null, "closed research and embedded window leave no active modal");
            WorldKey();
            Check(host.WorldActions == 1, "world actions resume only after the modal closes");
            dialog.Open(); tree.Call("select_node", "primary_center");
            tree.Get("action").As<Button>().GrabFocus();
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Click(tree.Get("action").As<Button>());
            Check(requests == 1 && !match.Players[1].Research.Has(TechnologyId.MagicFoundation), "native Research click emits intent through application boundary without local purchase");
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
            Check(second.HoverDetailsVisible && standaloneButtons["primary_center"].As<Button>().TooltipText.Length == 0,
                "keyboard focus reaches the sole details surface without native duplicate tooltip");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = true });
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(GetViewport().GuiGetFocusOwner() == second.Tree.Get("action").As<Button>(), "native Tab reaches focused node purchase within hover surface");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = false });
            TechnologyId intent = TechnologyId.None; second.Requested = id => intent = id;
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Enter, Pressed = true });
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Enter, Pressed = false });
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(intent == TechnologyId.MagicFoundation && !match.Players[1].Research.Has(intent), "native footer Enter press/release emits intent without allocating locally");
            last.GrabFocus(); Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = true });
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
        finally { RemoveChild(canvas); canvas.Free(); }
    }
}

internal sealed partial class WorldtreeInputHost : Control
{
    public Node?[] ModalCandidates { get; set; } = [];
    public bool ConsumedBeforeGui { get; private set; }
    public int WorldActions { get; private set; }
    public override void _Input(InputEvent @event)
    {
        GameApplication.RouteModalInput(@event, GetViewport(), ModalCandidates);
        ConsumedBeforeGui = GetViewport().IsInputHandled();
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if (GameApplication.SelectModal(ModalCandidates) is null && @event is InputEventKey { Keycode: Key.F9, Pressed: true }) WorldActions++;
    }
}
