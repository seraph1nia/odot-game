using Game.Core;
using Godot;

namespace Game.Verification;

public partial class EngineTests
{
    // Tier B: resource loading and rule tests miss a stale/absent HUD receiver,
    // wrong focused-city sender, or optimistic Worldtree allocation. Drive the
    // SAME production construction/update/CanEdit/SendAction bindings as Tabletop.
    // One minimal owned SceneTree, a numerical Match and delayed transport adapter;
    // no Main/GameApplication/world, network, screenshots or new test framework.
    // Estimated cost: native layout frames + three existing child awakenings,
    // under the ordinary 60s component bound; actual timings belong to the runner.
    private async Task Receiving()
    {
        GetWindow().Size = new(1920, 1080);
        using Theme theme = ApplicationTheme.Create();
        var applicationCanvas = new CanvasLayer { Layer = 10 };
        AddChild(applicationCanvas);
        var applicationHost = new Control { Theme = theme, MouseFilter = Control.MouseFilterEnum.Ignore };
        applicationCanvas.AddChild(applicationHost); applicationHost.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var host = new Control { Theme = theme, MouseFilter = Control.MouseFilterEnum.Ignore };
        AddChild(host); host.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        using var match = new Match();
        _ = match.Join(); _ = match.Join();
        Check(match.Apply(1, Command.FromSnapshot(match.Snapshot(), 1, "start", 1)).Accepted, "receiving setup starts numerical authority, not an application");
        match.Players[1].Research = new(30);
        var session = new ReceivingSession(match);
        var hud = new Tabletop(session);
        var settings = new ClientSettings(); applicationCanvas.AddChild(settings);
        settings.Initialize(applicationHost, theme);
        settings.Dialog.ResearchHost = () => hud.ResearchDialog;
        settings.InSession = true;
        GetWindow().Size = new(1920, 1080);
        UiDialog join = GameApplication.CreateJoinConfirmation(applicationHost, theme, () => hud.ResearchDialog);
        UiDialog leave = GameApplication.CreateLeaveConfirmation(applicationHost, theme, () => hud.ResearchDialog);
        settings.ReturnRequested = () => GameApplication.ShowLeaveConfirmation(leave, "End the current hosted game?");
        bool suspended = false;
        try
        {
            hud.InitializeHud(host, dialog => { dialog.ResearchHost = () => hud.ResearchDialog; applicationHost.AddChild(dialog); },
                () => suspended || GameApplication.SelectModal([leave, join, settings.Dialog, .. hud.DetailModals]) is not null, () => { });
            void Refresh(int slot = 1, int city = 1) => hud.RefreshHud(city, slot);
            void PublishResearchPoints(int points)
            {
                match.Players[1].Research = match.Players[1].Research with { Points = points };
                // Building phase does not advance fixed ticks. Cycle only the
                // fixture's other owned peer to publish a real authority revision,
                // rather than forging snapshot/version fields for the receiver.
                match.SetConnected(2, false); match.SetConnected(2, true);
                session.Acknowledge(); Refresh(slot: 2);
            }
            Button Button(string name) => host.FindChildren(name, "Button", true, false).OfType<Button>().Single();
            Label Label(string name) => host.FindChildren(name, "Label", true, false).OfType<Label>().Single();
            var ledger = host.GetNode<Control>("ResourceTable");
            Label Stock(string resource) => ledger.Get("value_labels").AsGodotDictionary()[resource].AsGodotArray()[0].As<Label>();
            Label Income(string resource) => ledger.Get("value_labels").AsGodotDictionary()[resource].AsGodotArray()[1].As<Label>();
            void Click(Control control)
            {
                Check(control.IsVisibleInTree() && control.GetGlobalRect().HasArea(), "native receiving target is laid out: " + control.Name);
                this.Click(control.GetGlobalRect().GetCenter());
            }
            Refresh(); await Frames();
            Check(!hud.IsInsideTree() && hud.GetChildCount() == 0, "receiving controller has no world, camera, landscape or application startup");
            Check(Stock("Wood").Text == "6" && Stock("Gold").Text == "12", "real resource-ledger native labels receive starting authority stock");
            Button farm = Button("Farm");
            Check(!farm.Disabled && farm.Text.Contains("+5 food/turn", StringComparison.Ordinal) && farm.GetNode<RichTextLabel>("Cost").Text.Length > 0,
                "actual farm control receives live producer benefit and resource quote");
            Click(farm);
            Check(session.Commands.Count == 1 && session.Commands[0] is { Action: "build", City: 1, Slot: 1, Building: Building.Farm }
                && session.Results[0].Accepted && match.Players[1].Slots[1].Type == Building.Farm, "native farm click crosses production CanEdit/SendAction and actual authority");
            Refresh();
            Check(Stock("Wood").Text == "6", "HUD does not optimistically spend before an accepted snapshot");
            session.Acknowledge(); Refresh();
            Check(Stock("Wood").Text == "4" && Income("Food").Text == "+5", "accepted snapshot updates actual stock and producer income labels");
            Check(Label("SessionFeedback").Text == session.Results[0].Message && Label("SelectedPlot").Text.Contains("+5 food/turn", StringComparison.Ordinal),
                "actual selected-building and feedback labels consume the acknowledged result");

            // Deliver an unfunded authority snapshot; no local UI allocator.
            match.Players[1].Wood = 1; session.Acknowledge();
            Refresh(slot: 2); await Frames();
            int before = session.Commands.Count;
            Check(farm.Disabled && farm.TooltipText.Contains("wood", StringComparison.OrdinalIgnoreCase), "unfunded native build remains disabled with resource reason");
            Click(farm);
            Check(session.Commands.Count == before, "native disabled farm cannot send an unfunded request");
            // Model a stale delivered quote: the true authority changed after that
            // snapshot. A real rejected response must reach the receiving labels.
            match.Players[1].Wood = 6; session.Acknowledge(); Refresh(slot: 2); await Frames();
            Check(!farm.Disabled, "received quote enables an affordable native build");
            match.Players[1].Wood = 0;
            Click(farm); Refresh(slot: 2);
            Check(session.Commands.Count == before + 1 && !session.Results[^1].Accepted && match.Players[1].Slots[2].Type == Building.Empty,
                "stale native request is rejected by authority without building or payment");
            Check(Label("SessionFeedback").Text == "Not enough resources." && Label("SessionFeedback").Text == session.Results[^1].Message,
                "rejection feedback is rendered by the real HUD without a revision change");
            match.Players[1].Wood = 6; session.Acknowledge(); Refresh(slot: 2);

            // Actual guarded callbacks, not just disabled cosmetics, deny foreign,
            // disconnected, ready, paused and foreground-suspended action contexts.
            before = session.Commands.Count;
            Click(Button("NextCity"));
            Check(farm.Disabled && Label("SelectedPlot").Text.Contains("Observing", StringComparison.Ordinal), "native city navigation makes a foreign city read-only");
            farm.EmitSignal(BaseButton.SignalName.Pressed);
            Check(session.Commands.Count == before, "foreign focus cannot submit through production action callback");
            Refresh(slot: 2); session.Connected = false; Refresh(slot: 2);
            farm.EmitSignal(BaseButton.SignalName.Pressed);
            Check(session.Commands.Count == before && farm.Disabled && Label("SelectedPlot").Text.Contains("synchronized", StringComparison.Ordinal), "disconnected receiver denies edits with synchronization reason");
            session.Connected = true; session.Acknowledge(); Refresh(slot: 2); await Frames();
            Click(Button("Ready")); session.Acknowledge(); Refresh(slot: 2);
            before = session.Commands.Count; farm.EmitSignal(BaseButton.SignalName.Pressed);
            Check(match.Players[1].Ready && farm.Disabled && session.Commands.Count == before && Label("SelectedPlot").Text.Contains("Unready", StringComparison.Ordinal), "ready city cannot edit through retained callback");
            Click(Button("Ready")); session.Acknowledge(); Refresh(slot: 2);
            Click(Button("Pause")); session.Acknowledge(); Refresh(slot: 2);
            before = session.Commands.Count; farm.EmitSignal(BaseButton.SignalName.Pressed);
            Check(match.Paused && farm.Disabled && session.Commands.Count == before && Label("SelectedPlot").Text.Contains("frozen", StringComparison.Ordinal), "paused receiver refuses edits with its real reason");
            Click(Button("Pause")); session.Acknowledge(); Refresh(slot: 2);
            suspended = true; before = session.Commands.Count; farm.EmitSignal(BaseButton.SignalName.Pressed);
            Check(session.Commands.Count == before, "application host suspension prevents the actual native action binding");
            suspended = false;

            // Reach the landed Worldtree through the actual HUD opener and sender,
            // not a replacement callback wired directly from tree to Match.Apply.
            await Frames(); Click(Button("Research")); await Frames();
            var dialog = host.GetNode<WorldtreeDialog>("WorldtreeResearchDialog");
            Control tree = dialog.Research.Tree;
            var nodes = tree.Get("buttons").AsGodotDictionary();
            Button action = tree.Get("action").As<Button>();
            Check(dialog.Visible && dialog.Research.Feedback.Text == session.Feedback, "real Worldtree opener binds current session snapshot and feedback");
            Click(nodes["secondary_left_a"].As<Button>()); await Frames();
            before = session.Commands.Count;
            Check(action.Disabled && dialog.Research.Description.Text.Contains("Requires Melee Foundation", StringComparison.Ordinal), "actual Guardian control displays missing prerequisite");
            Click(action); tree.EmitSignal("node_action_requested", "secondary_left_a");
            Check(session.Commands.Count == before && !match.Players[1].Research.Has(TechnologyId.Guardian), "native and forged prerequisite intent cannot escape receiving eligibility guard");
            Click(nodes["primary_left"].As<Button>()); await Frames();
            Check(!action.Disabled && dialog.Research.Description.Text.Contains("3 research points", StringComparison.Ordinal), "native foundation selection shows the actual three-point catalog cost");
            var peer = new ReceivingSession(match, 2);
            await SettingsResearchScope(settings, leave, dialog, () =>
            {
                peer.Acknowledge(); peer.SendAction("pause"); session.Acknowledge(); Refresh(slot: 2);
                Check(peer.Results[^1].Accepted && session.State.Paused, "peer pause is acknowledged while Settings covers research");
            }, () => session.Commands.Count);
            peer.Acknowledge(); peer.SendAction("resume"); session.Acknowledge(); Refresh(slot: 2);
            Click(nodes["primary_left"].As<Button>()); await Frames();
            await InvitationScope(join, dialog, farm, () => session.Commands.Count);
            session.SendAction("pause"); session.Acknowledge(); Refresh(slot: 2);
            Check(session.Results[^1].Accepted && session.State.Paused && action.Disabled && dialog.Research.Description.Text.EndsWith("Paused", StringComparison.Ordinal),
                "paused Building research shows its current denial before an invitation");
            GameApplication.ShowJoinConfirmation(join); await Frames();
            peer.Acknowledge(); peer.SendAction("resume"); session.Acknowledge(); Refresh(slot: 2);
            Check(peer.Results[^1].Accepted && !session.State.Paused && !dialog.Visible && action.Disabled,
                "another player's acknowledged resume reaches the HUD while research remains suspended");
            Click(join.GetCancelButton());
            Check(dialog.Visible && !action.Disabled && dialog.Research.Description.Text.EndsWith("Available.", StringComparison.Ordinal)
                && dialog.Research.Description.Text.Contains("3 research points", StringComparison.Ordinal)
                && tree.Get("snapshot").AsGodotDictionary()["available"].AsGodotArray().Any(v => v.AsString() == "primary_left")
                && dialog.Research.Feedback.Text == session.Feedback && nodes["primary_left"].As<Button>().HasFocus(),
                "invitation dismissal refreshes current availability, price, description and feedback before restoring research focus");
            action.GrabFocus(); GameApplication.ShowJoinConfirmation(join); await Frames();
            peer.Acknowledge(); peer.SendAction("pause"); session.Acknowledge(); Refresh(slot: 2);
            Check(peer.Results[^1].Accepted && session.State.Paused && !dialog.Visible, "another player's acknowledged pause arrives while the enabled purchase is covered");
            Click(join.GetCancelButton());
            Check(action.Disabled && dialog.Research.Description.Text.EndsWith("Paused", StringComparison.Ordinal)
                && tree.Get("snapshot").AsGodotDictionary()["available"].AsGodotArray().Count == 0
                && tree.Get("snapshot").AsGodotDictionary()["selected"].AsString() == "primary_left" && nodes["root"].As<Button>().HasFocus(),
                "restoration applies current denial and preserves selection while falling back from a disabled purchase focus");
            before = session.Commands.Count;
            PressKey(Key.Enter); await Frames();
            Check(session.Commands.Count == before && match.Players[1].Research.Points == 30, "Enter after denied restoration cannot submit or spend research");
            peer.Acknowledge(); peer.SendAction("resume"); session.Acknowledge(); Refresh(slot: 2);
            Click(nodes["primary_left"].As<Button>()); await Frames();
            action.GrabFocus(); PressKey(Key.Enter); await Frames(); Refresh(slot: 2);
            Check(session.Commands[^1] is { Action: "research-tech", City: 1, Technology: TechnologyId.MeleeFoundation }
                && session.Results[^1].Accepted && match.Players[1].Research.Points == 27, "production Worldtree sender purchases through real Match authority");
            Check(!tree.Get("snapshot").AsGodotDictionary()["acquired"].AsGodotArray().Any(v => v.AsString() == "primary_left")
                && tree.Get("snapshot").AsGodotDictionary()["points"].AsInt32() == 30, "research intent cannot allocate ownership or points before acknowledgment");
            session.Acknowledge(); Refresh(slot: 2); await ToSignal(tree, "awakening_finished");
            Check(tree.Get("snapshot").AsGodotDictionary()["acquired"].AsGodotArray().Any(v => v.AsString() == "primary_left")
                && tree.Get("snapshot").AsGodotDictionary()["points"].AsInt32() == 27, "real acknowledged snapshot updates the receiving Worldtree");
            Click(nodes["secondary_left_a"].As<Button>()); await Frames();
            Check(!action.Disabled && dialog.Research.Description.Text.Contains("6 research points", StringComparison.Ordinal), "native specialization now receives the actual six-point prerequisite quote");
            Click(action); GameApplication.ShowJoinConfirmation(join); await Frames();
            session.Acknowledge(); Refresh(slot: 2);
            Check(!dialog.Visible && tree.Get("snapshot").AsGodotDictionary()["points"].AsInt32() == 27,
                "accepted specialization acknowledgment does not activate covered research");
            Click(join.GetCancelButton());
            Check(tree.Get("snapshot").AsGodotDictionary()["points"].AsInt32() == 21
                && tree.Get("snapshot").AsGodotDictionary()["acquired"].AsGodotArray().Any(v => v.AsString() == "secondary_left_a")
                && nodes["secondary_left_b"].As<Button>().TooltipText.Contains("Permanently locked", StringComparison.Ordinal)
                && tree.Get("points_label").As<Label>().Text == $"Research points  {session.State.Players[0].Research.Points} · progress {session.State.Players[0].Research.Progress}/3"
                && dialog.Research.Feedback.Text == session.Results[^1].Message,
                "covered acknowledgment restores authoritative acquired nodes, points/progress, sibling lock and command feedback");
            await ToSignal(tree, "awakening_finished");
            Check(match.Players[1].Research.Has(TechnologyId.Guardian) && match.Players[1].Research.Points == 21, "production specialization request pays its real authority price");
            Click(nodes["secondary_left_b"].As<Button>()); await Frames();
            before = session.Commands.Count;
            Check(action.Disabled && dialog.Research.Description.Text.Contains("Permanently locked", StringComparison.Ordinal), "actual exclusive sibling remains inspectable and permanently locked");
            Click(action); tree.EmitSignal("node_action_requested", "secondary_left_b");
            Check(session.Commands.Count == before && !match.Players[1].Research.Has(TechnologyId.Assault), "locked sibling cannot send or spend through native/receiving guards");
            PublishResearchPoints(2);
            Click(nodes["terminal_left_a_2"].As<Button>()); await Frames();
            Check(action.Disabled && dialog.Research.Description.Text.Contains("9 research points", StringComparison.Ordinal)
                && dialog.Research.Description.Text.Contains("Needs 9", StringComparison.Ordinal), "real production offshoot control shows exact cost and insufficient research: " + dialog.Research.Description.Text);
            Click(action); tree.EmitSignal("node_action_requested", "terminal_left_a_2");
            Check(session.Commands.Count == before && match.Players[1].Research.Points == 2, "unfunded offshoot cannot mutate authoritative research");
            // A live quote can also become unaffordable before delivery; feedback
            // and the unchanged acquired set must survive that rejected request.
            PublishResearchPoints(12);
            match.Players[1].Research = match.Players[1].Research with { Points = 2 };
            Click(action); Refresh(slot: 2);
            Check(!session.Results[^1].Accepted && dialog.Research.Feedback.Text == session.Results[^1].Message
                && !match.Players[1].Research.Has(TechnologyId.Stonecraft), "Worldtree shows real rejected-command feedback without cosmetic acquisition");
            PublishResearchPoints(12);
            Click(action); session.Acknowledge(); Refresh(slot: 2); await ToSignal(tree, "awakening_finished");
            Check(match.Players[1].Research.Has(TechnologyId.Stonecraft) && match.Players[1].Research.Points == 3, "real production offshoot bridge pays nine points once");
            Check(dialog.Research.Description.Text.Contains("stone per building turn", StringComparison.Ordinal), "production offshoot description remains the landed researched benefit");
            session.Connected = false; Refresh(slot: 2); before = session.Commands.Count;
            dialog.Research.Requested?.Invoke(TechnologyId.MagicFoundation);
            Check(session.Commands.Count == before && action.Disabled, "Tabletop sender independently refuses research while disconnected");
            Click(dialog.CloseButton); await Frames();
            Check(!dialog.Visible && Button("Research").HasFocus(), "actual Worldtree Close ends its modal lifetime and restores the real opener");
            session.Connected = true; session.Acknowledge(); Refresh(slot: 2); await Frames();
            before = session.Commands.Count; Click(farm);
            Check(session.Commands.Count == before + 1 && session.Results[^1].Accepted && match.Players[1].Slots[2].Type == Building.Farm,
                "closing the real modal releases the host guard and resumes the actual HUD sender");
        }
        finally
        {
            RemoveChild(applicationCanvas); applicationCanvas.Free();
            RemoveChild(host); host.Free(); hud.Free();
        }
        Check(!GodotObject.IsInstanceValid(host) && !GodotObject.IsInstanceValid(hud), "receiving owner frees its real Controls and world-free controller");
    }
}

// Transport stand-in only: capture with the same Command.FromSnapshot helper as
// Main, execute the real numerical authority, then explicitly deliver snapshots.
// It owns no duplicate purchasing, pricing, research or presentation logic.
internal sealed class ReceivingSession(Match authority, int player = 1) : IGameSession
{
    public MatchSnapshot State { get; private set; } = authority.Snapshot();
    public int PlayerId => player;
    public bool Connected { get; set; } = true;
    public string Status => Connected ? "Connected" : "Disconnected";
    public string Feedback { get; private set; } = "";
    public bool CanStart => State.Phase == Phase.Lobby;
    public bool CanInvite => false;
    public int HostPlayerId => 1;
    public List<Command> Commands { get; } = [];
    public List<CommandResult> Results { get; } = [];
    public void Acknowledge() => State = authority.Snapshot();
    public long SendAction(string action, int slot = -1, Building building = Building.Empty, int city = 0, UnitType soldierType = UnitType.Swordsman,
        TechnologyId technology = TechnologyId.None, Game.Core.Resource resource = Game.Core.Resource.Wood, int bundles = 0,
        ConstructionPayment payment = ConstructionPayment.Standard, int unitId = 0)
    {
        if (!Connected) return 0;
        Command command = Command.FromSnapshot(State, Commands.Count + 2L, action, city == 0 ? PlayerId : city, slot, building, soldierType, technology, resource, bundles, payment, unitId);
        Commands.Add(command);
        CommandResult result = authority.Apply(PlayerId, command);
        Results.Add(result); Feedback = result.Message;
        return command.Sequence;
    }
    public Command[] DrainActionCues() => [];
    public void Connect(bool fresh = false) { Connected = true; Acknowledge(); }
}
