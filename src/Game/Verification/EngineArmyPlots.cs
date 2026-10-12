using Game.Core;
using Godot;

namespace Game.Verification;

public partial class EngineTests
{
    // Tier B admission: authority/codec tests cannot catch actual font/resource,
    // roof-ray picking, native signal binding or premature HUD acknowledgment.
    // Minimal real HUD + army receiver + camera, never Tabletop._Ready/Main/world.
    // Estimated cost: a few layout frames and existing tower resources, <60s.
    private async Task ArmyPlots()
    {
        GetWindow().Size = new(1920, 1080);
        using Theme theme = ApplicationTheme.Create();
        var host = new Control { Theme = theme, MouseFilter = Control.MouseFilterEnum.Ignore };
        AddChild(host); host.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        using var match = new Match(new Rules { StartingGold = 100, StartingWood = 100 });
        match.Join(); match.Join();
        Check(match.Apply(1, Command.FromSnapshot(match.Snapshot(), 1, "start", 1)).Accepted, "army component starts only numerical authority");
        match.Players[1].Stone = 100; match.Players[1].Metal = 100;
        var session = new ReceivingSession(match);
        var hud = new Tabletop(session);
        var camera = new Camera3D { Projection = Camera3D.ProjectionType.Orthogonal, Size = 8, Current = true };
        AddChild(camera);
        var assets = new LandscapeAssets();
        ArmyPlotView? receiver = null;
        var dialogs = new List<UiDialog>();
        try
        {
            hud.InitializeHud(host, dialog => { host.AddChild(dialog); dialogs.Add(dialog); }, () => dialogs.Any(d => d.Visible), () => { });
            hud.RefreshHud(1, -1);
            Button Button(string name) => host.FindChildren(name, "Button", true, false).OfType<Button>().Single();
            Label Detail() => host.FindChildren("SelectedPlot", "Label", true, false).OfType<Label>().Single();
            void CheckInspection(string expected) => Check(Detail().Text.Contains(expected, StringComparison.Ordinal)
                && dialogs.Any(d => d.Content.FindChildren("*", "Label", true, false).OfType<Label>().Any(l => l.Text.Contains(expected, StringComparison.Ordinal))),
                "selected tower and Details receive authoritative damage description: " + expected);
            void Receive()
            {
                if (receiver is not null) { RemoveChild(receiver); receiver.Free(); }
                receiver = new ArmyPlotView(session.State.Players[0], session.State, assets); AddChild(receiver);
                hud.ReceiveArmyPlotBounds(1, receiver);
            }
            void Pick(int id, bool roof = false)
            {
                Vector3 point = ArmyPlotView.PlotPosition(session.State, id);
                if (roof && receiver!.Bounds[id - ArmyConfiguration.PlotOffset] is { } bounds)
                    point = new(bounds.GetCenter().X, bounds.End.Y, bounds.GetCenter().Z);
                camera.Position = point + Vector3.Up * 20; camera.LookAt(point, Vector3.Forward);
                Vector2 screen = camera.UnprojectPosition(point);
                int picked = hud.PickPlotRay(camera.ProjectRayOrigin(screen), camera.ProjectRayNormal(screen));
                Check(picked == id, $"native camera ray picks production army {(roof ? "tower roof" : "plot")} {id}");
                hud.SelectPlot(picked);
            }
            async Task Click(string name)
            {
                await Frames(); Button button = Button(name);
                Check(button.IsVisibleInTree() && !button.Disabled && button.GetGlobalRect().HasArea(), "native army action laid out: " + name);
                this.Click(button.GetGlobalRect().GetCenter());
            }
            async Task CheckUpgradeSpacing(int plot)
            {
                foreach (Vector2I size in new[] { new Vector2I(1100, 820), new Vector2I(1280, 720), new Vector2I(1920, 1080), new Vector2I(2200, 1640) })
                {
                    GetWindow().Size = size;
                    foreach (bool connected in new[] { true, false })
                    {
                        session.Connected = connected; hud.RefreshHud(1, plot); await Frames(); await Frames();
                        foreach (var (name, caption) in new[]
                        {
                            ("RapidArrowUpgrade", "4 gold · 2 wood · 2 stone"),
                            ("BombardUpgrade", "4 gold · 2 wood · 2 stone · 1 metal")
                        })
                        {
                            Button button = Button(name);
                            Check(button.IsVisibleInTree() && button.Disabled == !connected,
                                $"native tower upgrade state on plot {plot}: {name}, connected={connected}");
                            Check(button.GetNode<RichTextLabel>("Cost").GetParsedText() == caption,
                                "complete authoritative tower upgrade cost caption: " + name);
                            CheckStackedCost(button);
                        }
                    }
                }
                GetWindow().Size = new(1920, 1080);
                session.Connected = true; hud.RefreshHud(1, plot); await Frames(); await Frames();
            }
            Receive(); Pick(14); await Frames();
            Check(host.FindChildren("BuyHome", "Button", true, false).Count == 0 && Detail().Text.Contains("Army plot 6 · Locked", StringComparison.Ordinal),
                "actual HUD has numbered locked army plot and no parallel Buy unit space button");
            Check(Button("BuyPlot").Text.Contains("5 gold", StringComparison.Ordinal), "army selection receives first real plot quote");
            await Click("BuyPlot");
            Check(session.Commands[^1] is { Action: "buy-plot", Slot: 14, ExpectedHomeCount: 2 } && session.Results[^1].Accepted,
                "native direct plot purchase uses authoritative quote/command owner");
            Check(session.State.Players[0].Gold == 100 && session.State.Players[0].Army!.Homes[5].State == ArmyPlotStatus.Locked,
                "intent cannot optimistically acquire army land or spend");
            session.Acknowledge(); hud.RefreshHud(1, 14); Receive(); Pick(14);
            Check(Detail().Text.Contains("OwnedEmpty", StringComparison.Ordinal) && !Button("ArrowTower").Disabled,
                "acknowledged empty plot exposes actual defense construction");
            await Click("ArrowTower");
            Check(session.Commands[^1] is { Action: "build", Slot: 14, Building: Building.ArrowTower } && session.Results[^1].Accepted,
                "native empty army plot construction builds a real base tower");
            session.Acknowledge(); hud.RefreshHud(1, 14); Receive(); Pick(14, roof: true);
            Check(receiver!.Bounds[5] is not null && Detail().Text.Contains("TowerOccupied · 6/6", StringComparison.Ordinal)
                && receiver.GetNode<Label3D>("Home5").Text.Contains("all 6 spots", StringComparison.Ordinal),
                "actual imported tower bounds, label and selected inspection expose whole-plot occupation");
            CheckInspection("single target · no splash");
            await CheckUpgradeSpacing(14);
            Check(Button("BombardUpgrade").TooltipText.Contains("radius 1 hex · up to 3 enemies", StringComparison.Ordinal),
                "default bombard preview retains authoritative radius and victim cap");
            match.Players[1].Metal = 0;
            await Click("BombardUpgrade"); hud.RefreshHud(1, 14);
            Check(!session.Results[^1].Accepted && match.Players[1].Towers[14].Type == Building.ArrowTower
                && host.FindChildren("SessionFeedback", "Label", true, false).OfType<Label>().Single().Text == session.Results[^1].Message,
                "stale native upgrade quote rejects without mutation and returns real authority feedback");
            match.Players[1].Metal = 100; session.Acknowledge(); hud.RefreshHud(1, 14);
            await Click("BombardUpgrade");
            Check(session.Commands[^1] is { Action: "upgrade-tower", Slot: 14, Building: Building.BombardTower, ExpectedTrackLevel: 1 }
                && session.Results[^1].Accepted && session.State.Players[0].Towers.Single().Type == Building.ArrowTower,
                "native bombard path submits actual upgrade intent without optimistic tower mutation");
            session.Acknowledge(); hud.RefreshHud(1, 14); Receive(); Pick(14, roof: true);
            Check(session.State.Players[0].Towers.Single() is { Type: Building.BombardTower, Level: 2 }
                && Detail().Text.Contains("Bombard tower", StringComparison.Ordinal) && !Button("BombardUpgrade").Visible,
                "accepted upgraded artwork/inspection consumes authoritative path and removes duplicate upgrades");
            CheckInspection("radius 1 hex · up to 3 enemies");
            Pick(9); await Click("ArrowTower"); session.Acknowledge(); hud.RefreshHud(1, 9); Receive(); Pick(9, roof: true);
            Check(Button("RapidArrowUpgrade").TooltipText.Contains("single target · no splash", StringComparison.Ordinal), "faster arrow preview explicitly excludes splash");
            await Click("RapidArrowUpgrade"); session.Acknowledge(); hud.RefreshHud(1, 9); Receive(); Pick(9, roof: true);
            Check(session.State.Players[0].Towers.Single(t => t.Slot == 9) is { Type: Building.RapidArrowTower, Level: 2 },
                "second real native path acknowledges faster single-target arrow upgrade");
            CheckInspection("single target · no splash");
            Pick(10); await Click("CatapultTower"); session.Acknowledge(); hud.RefreshHud(1, 10); Receive(); Pick(10, roof: true);
            CheckInspection("same-cell splash · radius 0 hex · up to 3 enemies");
            await Click("Upgrade"); session.Acknowledge(); hud.RefreshHud(1, 10);
            Check(session.Results[^1].Accepted && session.State.Players[0].Towers.Single(t => t.Slot == 10).Level == 2,
                "native legacy army catapult upgrade retains its independent level path");
            CheckInspection("same-cell splash · radius 0 hex · up to 3 enemies");
            session.SendAction("build", 1, Building.CatapultTower); session.Acknowledge(); hud.RefreshHud(1, 1);
            CheckInspection("same-cell splash · radius 0 hex · up to 3 enemies");
            await Click("Upgrade"); session.Acknowledge(); hud.RefreshHud(1, 1);
            Check(session.Results[^1].Accepted && session.State.Players[0].Towers.Single(t => t.Slot == 1).Level == 2,
                "native legacy city catapult upgrade retains its independent level path");
            CheckInspection("same-cell splash · radius 0 hex · up to 3 enemies");
            session.SendAction("build", 2, Building.ArrowTower); session.Acknowledge(); hud.RefreshHud(1, 2);
            Check(session.Results[^1].Accepted, "city base arrow tower enables both native upgrade siblings");
            await CheckUpgradeSpacing(2);
            // Retained snapshots with all capacity tower-blocked disable recruitment.
            session.SendAction("build", 0, Building.Barracks); session.Acknowledge(); hud.RefreshHud(1, 0); await Frames();
            Check(Button("Recruit").Disabled, "actual recruitment UI excludes all tower occupied slots, including initially owned army plots");
            Check(!hud.IsInsideTree() && hud.GetChildCount() == 0 && host.Theme == theme,
                "army fixture uses production current native resources and no application/world startup");
            foreach (var (timing, expected) in new[]
            {
                (new DefenseTiming(30, 90, 4, 0), "same-cell splash · radius 0 hex · up to 4 enemies"),
                (new DefenseTiming(30, 90, 1, 2), "single target · no splash")
            })
            {
                using var configured = new Match(new Rules { StartingGold = 100, StartingWood = 100, Combat = new() { BombardTower = timing } });
                configured.Join(); configured.Join();
                Check(configured.Apply(1, Command.FromSnapshot(configured.Snapshot(), 1, "start", 1)).Accepted, "configured preview authority starts");
                configured.Players[1].Stone = 100; configured.Players[1].Metal = 100;
                var previewSession = new ReceivingSession(configured);
                var previewHost = new Control { Theme = theme };
                host.AddChild(previewHost);
                var previewHud = new Tabletop(previewSession);
                try
                {
                    previewHud.InitializeHud(previewHost, dialog => previewHost.AddChild(dialog), () => false, () => { });
                    previewSession.SendAction("build", 9, Building.ArrowTower); previewSession.Acknowledge(); previewHud.RefreshHud(1, 9);
                    var button = previewHost.FindChildren("BombardUpgrade", "Button", true, false).OfType<Button>().Single();
                    Check(button.TooltipText.Contains(expected, StringComparison.Ordinal)
                        && previewHud.DetailModals.OfType<UiDialog>().Any(d => d.Content.FindChildren("*", "Label", true, false).OfType<Label>().Any(l => l.Text.Contains(expected, StringComparison.Ordinal))),
                        "configured bombard preview and Details describe victim cap before radius");
                    button.EmitSignal(BaseButton.SignalName.Pressed);
                    Check(previewSession.Results[^1].Accepted, "configured native bombard upgrade accepted");
                    previewSession.Acknowledge(); previewHud.RefreshHud(1, 9);
                    Check(previewHost.FindChildren("SelectedPlot", "Label", true, false).OfType<Label>().Single().Text.Contains(expected, StringComparison.Ordinal),
                        "configured selected bombard receives the same authoritative description as its preview");
                }
                finally
                {
                    host.RemoveChild(previewHost); previewHost.Free(); previewHud.Free();
                }
            }
        }
        finally
        {
            if (receiver is not null) { RemoveChild(receiver); receiver.Free(); }
            RemoveChild(camera); camera.Free(); RemoveChild(host); host.Free(); hud.Free();
        }
    }
}
