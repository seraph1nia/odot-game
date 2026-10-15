using Game.Core;
using Godot;

namespace Game.Verification;

public partial class EngineTests
{
    // Tier B catches native toggle/focus/ack bindings and imported figure picking
    // that pure authority tests cannot. Minimal Controls + rigs, no Tabletop world,
    // Main, network or renderer acceptance. Estimated cost: a few native frames.
    private async Task BattleFlow()
    {
        GetWindow().Size = new(1100, 820);
        using Theme theme = ApplicationTheme.Create();
        using var match = new Match(combatSeed: 123); match.Join(); match.Join();
        Check(match.Apply(1, Command.FromSnapshot(match.Snapshot(), 1, "start", 1)).Accepted, "battle-flow component starts numerical authority only");
        Check(match.Apply(1, Command.FromSnapshot(match.Snapshot(), 2, "build", 1, 0, Building.Lumbermill)).Accepted, "component pays for ordinary lumber production");
        var session = new ReceivingSession(match);
        var root = new Control { Theme = theme, MouseFilter = Control.MouseFilterEnum.Ignore };
        AddChild(root); root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var chromeLayer = new CanvasLayer { Layer = 10 }; AddChild(chromeLayer);
        var chrome = new Control { Theme = theme, MouseFilter = Control.MouseFilterEnum.Ignore }; chromeLayer.AddChild(chrome);
        chrome.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var settings = new ClientSettings(); AddChild(settings); settings.Initialize(chrome, theme); settings.InSession = true;
        var hud = new Tabletop(session);
        var dialogs = new List<UiDialog>();
        var figures = new Node3D(); AddChild(figures);
        var camera = new Camera3D { Current = true, Projection = Camera3D.ProjectionType.Orthogonal, Size = 12, Position = new(0, 8, 1) };
        figures.AddChild(camera); camera.LookAt(new(0, 0, -9));
        try
        {
            hud.InitializeHud(root, dialog => { dialogs.Add(dialog); root.AddChild(dialog); }, () => settings.IsOpen, () => { });
            hud.RefreshHud(1, -1); await Frames();
            Button Speed(CombatSpeed speed) => root.FindChildren("Speed" + speed, "Button", true, false).OfType<Button>().Single();
            foreach (Vector2I size in new[] { new Vector2I(1100, 820), new Vector2I(1280, 720) })
            {
                GetWindow().Size = size; await Frames();
                Rect2 panel = root.FindChildren("CombatSpeed", "PanelContainer", true, false).OfType<PanelContainer>().Single().GetGlobalRect();
                Rect2 settingsRect = settings.SettingsButton.GetGlobalRect();
                Check(!panel.Intersects(settingsRect) && panel.End.X < size.X && panel.End.Y < size.Y,
                    "production top chrome leaves Settings and all speed choices in separate visible regions");
                Check(panel.Size.Y <= 64 && panel.Size.X <= 360,
                    "unwrapped native speed label keeps the three controls in a compact horizontal strip");
                Click(settingsRect.GetCenter()); await Frames();
                Check(settings.IsOpen && settings.ReturnButton.Visible && session.Commands.Count == 0, "actual Settings remains clickable during a session without issuing a match action");
                settings.Close(); await Frames();
            }
            GetWindow().Size = new(1100, 820); await Frames();
            Check(Speed(CombatSpeed.One).ButtonPressed && !Speed(CombatSpeed.Two).ButtonPressed
                && Enum.GetValues<CombatSpeed>().Select(s => Speed(s).Text).SequenceEqual(["1x", "1.5x", "2x"]),
                "top native speed control has exactly three choices and default 1x");
            Click(Speed(CombatSpeed.Two).GetGlobalRect().GetCenter()); await Frames();
            Check(session.Commands.Single().Action == "speed-2x" && session.Results.Single().Accepted
                && match.Snapshot().CombatSpeed == CombatSpeed.Two, "native speed pointer submits ordinary acknowledged global action");
            Check(Speed(CombatSpeed.One).ButtonPressed && !Speed(CombatSpeed.Two).ButtonPressed,
                "pending speed selection remains the delivered authority value, not optimistic native toggle state");
            session.Acknowledge(); hud.RefreshHud(1, -1);
            Check(Speed(CombatSpeed.Two).ButtonPressed && !Speed(CombatSpeed.One).ButtonPressed, "accepted speed refresh selects only 2x");
            Speed(CombatSpeed.OneAndHalf).GrabFocus(); PressKey(Key.Space); await Frames();
            Check(session.Commands.Last().Action == "speed-1.5x" && session.Results.Last().Accepted,
                "focused native speed control supports keyboard activation");
            session.Acknowledge(); hud.RefreshHud(1, -1);
            session.Connected = false; hud.RefreshHud(1, -1);
            int sent = session.Commands.Count;
            Click(Speed(CombatSpeed.One).GetGlobalRect().GetCenter()); await Frames();
            Check(Speed(CombatSpeed.One).Disabled && session.Commands.Count == sent && Speed(CombatSpeed.OneAndHalf).ButtonPressed,
                "disconnected speed input is disabled and retains synchronized selection");
            session.Connected = true; hud.RefreshHud(1, -1);

            MatchSnapshot prepared = match.Snapshot();
            UnitState[] manifest = BattleDeployment.Preview(prepared).Where(u => u.Id < 0).ToArray();
            var layout = new CombatLayout(new(prepared.Arena!.Board)); layout.Synchronize(prepared);
            Aabb content = BattlefieldFrame.Content(prepared, []);
            Dictionary<int, Vector3> formation = manifest.ToDictionary(u => u.Id, u => layout.Position(u, prepared.Tick));
            Rect2 ledger = root.GetNode<Control>("ResourceTable").GetGlobalRect();
            Rect2 bottom = root.GetNode<Control>("BottomPanel").GetGlobalRect();
            float lane = Math.Max(240, GetViewport().GetVisibleRect().Size.X - ledger.Position.X + 12);
            BattlefieldFrame.FitCamera(camera, content, CombatLayout.DistrictOrigin(prepared), GetViewport().GetVisibleRect().Size, bottom.Size.Y, lane);
            Check(manifest.All(u => content.HasPoint(formation[u.Id] - CombatLayout.DistrictOrigin(prepared))),
                "landed content-derived framing includes every authoritative forecast position");
            var states = manifest.ToDictionary(u => u.Id);
            var views = new List<UnitView>();
            foreach (UnitState unit in manifest.Where(u => u.Destination == 1))
            {
                var view = new UnitView(unit); figures.AddChild(view); views.Add(view);
                view.Sample(unit, 0, 0, 1, layout, states);
            }
            await Frames();
            Check(views.All(v => v.Visible && v.Clip == "idle" && v.StatusBadge.Length == 0 && v.FindChildren("*", "Label3D", true, false).Count == 0)
                && views.Select(v => v.Position).Distinct().Count() == views.Count,
                "actual imported enemy rigs form distinct idle read-only next-wave figures");
            Check(views.All(v => Enumerable.Range(0, 8).Select(i => camera.UnprojectPosition(v.PickingBounds().GetEndpoint(i)))
                .All(p => !ledger.HasPoint(p) && !bottom.HasPoint(p) && p.Y >= 64)),
                "actual native forecast rig silhouettes avoid the measured resource ledger and bottom/top chrome, not a guessed narrow lane");
            UnitView first = views[0];
            Aabb pickBounds = first.PickingBounds();
            Vector2 centerPoint = camera.UnprojectPosition(pickBounds.GetCenter());
            // A back member's body center can be behind a front member at the
            // low tabletop pitch. Its exposed roof/head remains selectable;
            // nearest-ray picking must not prefer a hidden body through a rig.
            Vector2 point = camera.UnprojectPosition(new(pickBounds.GetCenter().X, pickBounds.End.Y - .02f, pickBounds.GetCenter().Z));
            UnitView? picked = UnitPicking.Pick(views, camera, point);
            Console.WriteLine($"MEASURE forecast pick center={UnitPicking.Pick(views, camera, centerPoint)?.State.Id} exposed={picked?.State.Id} expected={first.State.Id}");
            Check(picked?.State.Id == first.State.Id, "production unit ray picker selects actual forecast rig bounds");
            var inspector = new UnitInspector(session, () => true); root.AddChild(inspector);
            inspector.Select(picked!.State); inspector.ConfigureActions(match.Snapshot().Players[0], true, match.Army);
            Check(inspector.UnitId == first.State.Id && inspector.Visible
                && inspector.FindChildren("*", "Label", true, false).OfType<Label>().Any(l => l.Text.Contains("read-only forecast", StringComparison.Ordinal))
                && !inspector.FindChildren("RetireUnit", "Button", true, false).OfType<Button>().Single().Visible,
                "ordinary unit inspector shows real forecast profile without owned army actions");
            Check(match.Enemies.Count == 0 && match.Snapshot().Reservations.Positions.Length == 0,
                "rendering and inspecting forecast creates no live enemy targets or reservations");
            for (int turn = 1; turn <= 3; turn++)
            {
                if (turn == 3)
                {
                    session.SendAction("build", 1, Building.Stonecutter);
                    Check(session.Results.Last().Accepted && match.Players[1].Stone == 2, "final preparation construction retains Warden starting stone before first income");
                    session.Acknowledge(); hud.RefreshHud(1, -1);
                }
                Button ready = root.FindChildren("Ready", "Button", true, false).OfType<Button>().Single();
                Check(ready.TooltipText.Contains("one production", StringComparison.Ordinal)
                    && (turn != 3 || ready.TooltipText.Contains("enter battle", StringComparison.Ordinal)), "actual Ready tooltip describes the adopted settlement boundary");
                Click(ready.GetGlobalRect().GetCenter()); await Frames();
                Check(session.Commands.Last().Action == "ready" && session.Results.Last().Accepted && match.ProductionCount == turn - 1,
                    "native readiness waits for the second city without early settlement");
                Check(match.Apply(2, Command.FromSnapshot(match.Snapshot(), 1, "ready", 2)).Accepted, "peer completes actual readiness");
                session.Acknowledge(); hud.RefreshHud(1, -1); await Frames();
                Check(match.ProductionCount == turn && match.Players.Values.All(c => !c.Ready), "native three-stage consumer observes one settlement per preparation");
            }
            Check(match.Players[1].Stone == 3, "final Stonecutter contributes before component combat entry");
            Check(match.Phase == Phase.Combat && match.Snapshot().NextWave.Length == 0 && match.Enemies.Count == manifest.Length,
                "combat transition consumes exactly the inspected forecast allocation");
            var actualLayout = new CombatLayout(new(match.Snapshot().Arena!.Board)); actualLayout.Synchronize(match.Snapshot());
            var liveStates = match.Enemies.ToDictionary(u => u.Id);
            for (int i = 0; i < manifest.Length; i++)
            {
                UnitState live = match.Enemies[i];
                Check(manifest[i].FormationSlot == live.FormationSlot && manifest[i].Hex!.Position == live.Hex!.Position
                    && formation[manifest[i].Id].IsEqualApprox(actualLayout.Position(live, match.Tick))
                    && Mathf.IsEqualApprox(layout.Facing(manifest[i], prepared.Tick, states), actualLayout.Facing(live, match.Tick, liveStates)),
                    "same authoritative inputs give identical preview/live member formation, native transform and facing before any combat tick");
            }
            inspector.Close();
            foreach (UnitView view in views) { figures.RemoveChild(view); view.Free(); }
            views.Clear();
            Check(inspector.UnitId is null && UnitPicking.Pick(views, camera, point) is null,
                "preview transition clears inspection and removes stale clickable forecast figures");
        }
        finally
        {
            foreach (UiDialog dialog in dialogs) { dialog.Hide(); dialog.Free(); }
            RemoveChild(root); root.Free(); hud.Free();
            RemoveChild(settings); settings.Free();
            RemoveChild(chromeLayer); chromeLayer.Free();
            RemoveChild(figures); figures.Free();
        }
    }
}
