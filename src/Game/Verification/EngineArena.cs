using Game.Core;
using Godot;

namespace Game.Verification;

public partial class EngineTests
{
    // Tier B admission: native shared Camera3D/picking/rig/component resources
    // and real receiving Button -> accepted Match snapshot are not observable
    // by pure rules tests. Minimal components only; no Main/world/full campaign,
    // multiplayer peer, display or artistic/choreography claim. Estimated <10s.
    private async Task Arena()
    {
        var view = new SharedArenaView(); AddChild(view);
        var camera = new Camera3D { Current = true, Projection = Camera3D.ProjectionType.Orthogonal, Far = 180 }; AddChild(camera);
        var navigation = new TabletopCamera(camera);
        try
        {
            for (int players = 1; players <= 4; players++)
            {
                using var match = new Match(combatSeed: 9);
                for (int index = 0; index < players; index++) match.Join();
                MatchSnapshot state = match.Snapshot();
                var layout = new CombatLayout(new(state.Arena!.Board)) { Arena = state.Arena }; layout.Synchronize(state);
                view.Synchronize(state, layout, true, 1); view.Sample(0);
                Check(view.FindChildren("ArenaCell*", "MeshInstance3D", true, false).Count == 63 * players, "native arena component contains every connected cell for " + players + " players");
                Check(view.FindChildren("StructureHealth*", "Label3D", true, false).Count == players * 2, "each owner has a native Keep and Watchpost label");
                MultiMeshInstance3D[] groundBatches = view.GetChildren().OfType<MultiMeshInstance3D>().ToArray();
                Check(groundBatches.Length == 3 && groundBatches.Sum(b => b.Multimesh.InstanceCount) == 63 * players,
                    "three imported woodland surface families cover every playable cell exactly once");
                foreach (MultiMeshInstance3D batch in groundBatches)
                {
                    Check(DetailedGround.Bases.Contains(batch.GetMeta("asset").AsString()) && batch.Multimesh.Mesh is ArrayMesh,
                        "arena loads actual registered authored ground buffers");
                    for (int i = 0; i < batch.Multimesh.InstanceCount; i++)
                    {
                        Transform3D transform = batch.Multimesh.GetInstanceTransform(i);
                        float? height = GroundGeometry.Surface(batch.Multimesh.Mesh, transform, transform.Origin);
                        Check(height is not null && Math.Abs(height.Value) < .001f, "imported arena contact plane stays at the exact logical ground");
                    }
                }
                Node3D dressing = view.GetNode<Node3D>("ArenaWoodland");
                Rect2 playable = BattlefieldFrame.GroundRect(view.Content);
                Check(dressing.GetChildCount() <= 44, "one-to-four-owner exterior dressing has a bounded node budget");
                foreach (Node3D prop in dressing.GetChildren().OfType<Node3D>())
                    Check(!playable.HasPoint(new(prop.Position.X, prop.Position.Z)), "perimeter props stay outside all playable cells/objectives");
                ulong[] stableGround = groundBatches.Select(b => b.GetInstanceId()).ToArray();
                ulong staticDressing = dressing.GetInstanceId();
                Check(match.Apply(1, Command.FromSnapshot(state, 1, "start", 1)).Accepted, "ordinary source revision starts the component match");
                state = match.Snapshot(); view.Synchronize(state, layout, false, 1); view.Synchronize(state, layout, true, 1);
                Check(stableGround.SequenceEqual(view.GetChildren().OfType<MultiMeshInstance3D>().Select(b => b.GetInstanceId()))
                    && view.GetNode<Node3D>("ArenaWoodland").GetInstanceId() == staticDressing,
                    "ordinary accepted revisions and deployment toggles reuse static woodland nodes/resources");
                Check(view.FindChildren("DeploymentHome*", "Label3D", true, false).OfType<Label3D>().Count(l => l.Visible) == 6,
                    "owned home markers expose shape/text states without showing foreign editable homes");
                Aabb content = view.Content;
                Check(state.Arena!.Board.Cells.All(c => content.HasPoint(layout.CellPosition(c.Id))), "content-derived arena bounds retain every cell");
                foreach (ArenaSector sector in state.Arena.Sectors)
                    foreach (int cell in sector.DeploymentCells)
                    {
                        Vector3 point = CombatLayout.ArenaOrigin + layout.CellPosition(cell);
                        Check(view.PickCell(point + Vector3.Up * 20, Vector3.Down) == cell, "native ground picking returns exact authoritative deployment cell");
                    }
                foreach (Vector2I dimensions in new[] { new Vector2I(1280, 720), new(1920, 1080), new(820, 1100) })
                {
                    GetWindow().Size = dimensions; await Frames(); Rect2 viewport = GetViewport().GetVisibleRect(); float hud = viewport.Size.Y * .3f;
                    BattlefieldFrame.FitCamera(camera, content, CombatLayout.ArenaOrigin, viewport.Size, hud);
                    Rect2 ground = BattlefieldFrame.GroundRect(content); ground.Position += new Vector2(CombatLayout.ArenaOrigin.X, CombatLayout.ArenaOrigin.Z);
                    navigation.Fit(ground, viewport, sharedArena: true); navigation.Reset(); view.CoverBoundary(navigation.Boundary);
                    Check(BattlefieldFrame.GroundRect(view.OpaqueBounds).Encloses(navigation.Boundary), "opaque native arena mesh covers the complete projected pan/zoom/portrait safe boundary");
                    foreach (int corner in Enumerable.Range(0, 8))
                        Check(new Rect2(Vector2.Zero, new(viewport.Size.X - 240, viewport.Size.Y - hud)).Grow(1).HasPoint(camera.UnprojectPosition(content.GetEndpoint(corner) + CombatLayout.ArenaOrigin)), "default native projection frames complete arena at " + dimensions);
                    navigation.ZoomAt(viewport.GetCenter(), true, 40);
                    navigation.Drag(new(10000, 0, 10000), viewport.GetCenter());
                    Check(navigation.Boundary.Grow(.02f).Encloses(BattlefieldFrame.Footprint(camera, viewport)), "arena extreme pan remains inside content-derived safe bounds");
                    if (players == 4 && dimensions.X == 1920) Check(Math.Abs(navigation.Pan.X) > TabletopCamera.Travel.X, "four-player arena pan is not clamped to old settlement travel");
                }
            }
            // The real lobby grows without a new match/playback identity. Pure
            // projection tests and a fresh native layout per width miss this
            // receiving seam; reuse the actual native layout across all joins.
            using (var lobby = new Match(combatSeed: 9))
            {
                lobby.Join(); MatchSnapshot initial = lobby.Snapshot();
                var growing = new CombatLayout(new(initial.Arena!.Board));
                for (int players = 1; players <= 4; players++)
                {
                    if (players > 1) lobby.Join();
                    MatchSnapshot state = lobby.Snapshot(); growing.Synchronize(state);
                    view.Synchronize(state, growing, false, 1);
                    Check(state.MatchId == initial.MatchId && ReferenceEquals(growing.Arena, state.Arena),
                        "same-lobby roster growth refreshes native geometry and physical footprints");
                    var expected = new HexBoard(state.Arena!.Board);
                    foreach (HexCellDefinition cell in state.Arena.Board.Cells)
                    {
                        System.Numerics.Vector3 point = SharedCombatProjection.CellPosition(expected, cell.Id);
                        Check(growing.CellPosition(cell.Id).IsEqualApprox(new(point.X, point.Y, point.Z)),
                            "expanded lobby cell projects using current accepted arena width");
                    }
                }
            }
            using (var solo = new Match(combatSeed: 9))
            {
                solo.Join(); MatchSnapshot state = solo.Snapshot();
                var layout = new CombatLayout(new(state.Arena!.Board)) { Arena = state.Arena }; layout.Synchronize(state);
                var oldGround = view.GetChildren().OfType<MultiMeshInstance3D>().ToArray();
                view.Synchronize(state, layout, true, 1); await Frames();
                Check(oldGround.All(n => !GodotObject.IsInstanceValid(n)) && view.FindChildren("ArenaCell*", "MeshInstance3D", true, false).Count == 63,
                    "fresh four-player-to-solo replacement releases previous static geometry and markers");
            }
            await ArenaReceiving(view);
        }
        finally { RemoveChild(camera); camera.Free(); RemoveChild(view); view.Free(); }
    }
    private async Task ArenaReceiving(SharedArenaView arenaView)
    {
        GetWindow().Size = new(1920, 1080);
        using Theme theme = ApplicationTheme.Create();
        var host = new Control { Theme = theme, MouseFilter = Control.MouseFilterEnum.Ignore }; AddChild(host); host.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var modalHost = new Control { Theme = theme, MouseFilter = Control.MouseFilterEnum.Ignore }; AddChild(modalHost); modalHost.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        using var match = new Match(new() { StartingGold = 100, StartingWood = 100 }, combatSeed: 11);
        match.Join(); match.Join(); match.Players[1].Metal = 100; // Explicit component equipment fixture; actual commands still pay each soldier.
        Check(match.Apply(1, Command.FromSnapshot(match.Snapshot(), 1, "start", 1)).Accepted, "numerical receiving authority starts");
        var session = new ReceivingSession(match); var hud = new Tabletop(session);
        try
        {
            hud.InitializeHud(host, dialog => modalHost.AddChild(dialog), () => false, () => { });
            Button Find(string name) => host.FindChildren(name, "Button", true, false).Concat(modalHost.FindChildren(name, "Button", true, false)).OfType<Button>().Single();
            void Refresh() => hud.RefreshHud(1, 0);
            Refresh(); await Frames();
            Find("ArmyChoices").EmitSignal(BaseButton.SignalName.Pressed); Refresh(); await Frames();
            Find("Barracks").EmitSignal(BaseButton.SignalName.Pressed);
            Check(session.Results[^1].Accepted && match.Players[1].Slots[0].Type == Building.Barracks, "production build binding crosses numerical authority");
            session.Acknowledge(); Refresh(); await Frames();
            Check(!Find("Recruit").Disabled, "ordinary native field recruitment is enabled at an empty purchased home, not tested as a Hall slot");
            Find("Recruit").EmitSignal(BaseButton.SignalName.Pressed);
            Check(session.Results[^1].Accepted && match.Players[1].Soldiers.Count == 1, "paid native recruit creates one member and squad");
            session.Acknowledge(); Refresh(); await Frames();
            Find("OpenSquads").EmitSignal(BaseButton.SignalName.Pressed); await Frames(); hud.RefreshSquads(); await Frames();
            int squad = match.Snapshot().Squads.Single(s => s.Owner == 1).Id;
            Check(Find("Squad" + squad).IsVisibleInTree(), "native roster receives stable squad identity");
            int metal = match.Players[1].Metal;
            Find("ReinforceSquad").EmitSignal(BaseButton.SignalName.Pressed);
            Check(session.Commands[^1].SquadId == squad && session.Results[^1].Accepted && match.Players[1].Metal < metal && match.Players[1].Soldiers.Count == 2, "production paid reinforcement binding preserves squad and spends per member");
            Check(session.State.Players[0].Soldiers.Length == 1, "roster does not optimistically invent a member before authoritative acknowledgment");
            session.Acknowledge(); hud.RefreshSquads(); await Frames();
            Check(Find("SquadMember" + match.Players[1].Soldiers[^1].Id).IsVisibleInTree(), "native member list receives accepted reinforcement identity/HP");
            match.Players[1].Metal = 0;
            Find("ReinforceSquad").EmitSignal(BaseButton.SignalName.Pressed);
            Check(!session.Results[^1].Accepted && match.Players[1].Soldiers.Count == 2, "stale displayed paid reinforcement reaches real denial without an optimistic member");
            session.Acknowledge(); hud.RefreshSquads(); await Frames();
            Button memberButton = Find("SquadMember" + match.Players[1].Soldiers[0].Id); memberButton.GrabFocus();
            var peer = new ReceivingSession(match, 2); peer.SendAction("ready"); Check(peer.Results[^1].Accepted, "other player's ready changes the real authority revision");
            session.Acknowledge(); hud.RefreshSquads(); await Frames();
            Check(GodotObject.IsInstanceValid(memberButton) && memberButton == Find(memberButton.Name.ToString()) && GetViewport().GuiGetFocusOwner() == memberButton, "native member focus survives authoritative revision without recreating the roster");
            hud.RefreshHud(2, 0); hud.RefreshSquads(); await Frames();
            Check(Find("ReinforceSquad").Disabled && Find("RegroupMember").Disabled, "native ally inspection never grants army editing ownership");
            await ArenaModels(match, arenaView);
            hud.RefreshHud(1, 0); hud.RefreshSquads(); await Frames();
            match.Players[1].Metal = 100; // Component supply, each request still pays.
            Check(match.Apply(1, Command.FromSnapshot(match.Snapshot(), 500, "build", 1, 1, Building.TownHall)).Accepted,
                "native storage fixture buys a real Hall with its complete quote");
            session.Acknowledge(); hud.RefreshSquads(); await Frames();
            Check(!Find("RecruitNewSquad").Disabled, "native new-squad control recognizes the second free field home");
            Find("RecruitNewSquad").EmitSignal(BaseButton.SignalName.Pressed);
            int recruit = match.Players[1].Soldiers[^1].Id;
            int newSquad = match.Combat.Read(recruit).SquadId;
            Check(session.Results[^1].Accepted && newSquad != squad && session.State.Players[0].Soldiers.Length == 2,
                "native new-squad recruitment pays separately and waits for acknowledgment");
            session.Acknowledge(); hud.RefreshSquads(); await Frames();
            Find("Squad" + newSquad).EmitSignal(BaseButton.SignalName.Pressed); await Frames();
            var destination = modalHost.FindChild("RegroupDestination", true, false) as OptionButton
                ?? throw new InvalidOperationException("Missing native regroup selector");
            destination.Select(destination.GetItemIndex(squad));
            Find("RegroupMember").EmitSignal(BaseButton.SignalName.Pressed);
            Check(session.Results[^1].Accepted && match.Combat.Read(recruit).SquadId == squad,
                "selected native regroup destination retains the paid member's identity");
            session.Acknowledge(); hud.RefreshSquads(); await Frames();
            Find("Squad" + squad).EmitSignal(BaseButton.SignalName.Pressed); await Frames();
            Find("SquadMember" + recruit).EmitSignal(BaseButton.SignalName.Pressed);
            Find("SquadStoreMember").EmitSignal(BaseButton.SignalName.Pressed);
            Check(session.Results[^1].Accepted && match.Combat.Read(recruit).Assignment is { Stored: true },
                "native Hall storage transfers exactly the selected individual");
            session.Acknowledge(); hud.RefreshSquads(); await Frames();
            Find("Squad" + match.Combat.Read(recruit).SquadId).EmitSignal(BaseButton.SignalName.Pressed); await Frames();
            destination.Select(destination.GetItemIndex(squad));
            Check(!Find("SquadReleaseMember").Disabled, "native return control recognizes compatible field capacity");
            Find("SquadReleaseMember").EmitSignal(BaseButton.SignalName.Pressed);
            Check(session.Results[^1].Accepted && match.Combat.Read(recruit).Assignment is { Stored: false }
                && match.Combat.Read(recruit).SquadId == squad, "native Hall return uses the selected compatible field squad");
            session.Acknowledge(); hud.RefreshSquads(); await Frames();
            Find("Squad" + squad).EmitSignal(BaseButton.SignalName.Pressed); await Frames();
            Find("SquadMember" + recruit).EmitSignal(BaseButton.SignalName.Pressed);
            ResourceCost beforeRetirement = match.Players[1].Resources;
            Find("SquadRetireMember").EmitSignal(BaseButton.SignalName.Pressed);
            Check(session.Results[^1].Accepted && match.Players[1].Soldiers.Count == 2 && match.Players[1].Resources == beforeRetirement,
                "native retirement removes only the selected recruit without refund");
            session.Acknowledge(); hud.RefreshSquads(); await Frames();
            // Explicit numerical fallen-city receiving input; actual ENet and
            // physical elimination ordering are separate C/A owners. Exercise
            // genuine native roster/HUD callbacks after compressed delivery.
            match.Players[1].Health = 0;
            session.Acknowledge(); hud.RefreshHud(1, 0); hud.RefreshSquads(); await Frames();
            int commands = session.Commands.Count;
            ResourceCost stocks = match.Players[1].Resources;
            Check(session.State.Players[0].Eliminated && session.State.Players[0].Soldiers.Length == 2,
                "native receiver retains each fallen owner's individual instead of deleting its roster");
            foreach (string name in new[] { "ReinforceSquad", "RegroupMember", "SquadRetireMember" })
            {
                Button button = Find(name);
                Check(button.Disabled, "fallen-owner native action is disabled: " + name);
                button.EmitSignal(BaseButton.SignalName.Pressed);
            }
            modalHost.GetChildren().OfType<UiDialog>().Single(d => d.Title.StartsWith("Shared army", StringComparison.Ordinal)).GetOkButton().EmitSignal(BaseButton.SignalName.Pressed);
            await Frames();
            Button ready = Find("Ready");
            Check(ready.Disabled, "fallen spectator cannot use native readiness");
            this.Click(ready.GetGlobalRect().GetCenter()); await Frames();
            Check(session.Commands.Count == commands && match.Players[1].Soldiers.Count == 2 && match.Players[1].Resources == stocks
                && host.FindChildren("SelectedPlot", "Label", true, false).OfType<Label>().Single().Text.Contains("fallen", StringComparison.Ordinal),
                "retained native callbacks deny fallen owner without optimistic member or stock mutation");
            Check(!Find("Pause").Disabled, "fallen spectator retains deliberate shared pause privilege");
            peer.Acknowledge(); peer.SendAction("unready");
            Check(peer.Results[^1].Accepted, "living peer explicitly leaves its earlier ready state before editing");
            peer.Acknowledge(); peer.SendAction("build", 0, Building.Farm);
            Check(peer.Results[^1].Accepted && match.Players[2].Slots[0].Type == Building.Farm,
                "another owner's accepted city investment remains independently live");
            Console.WriteLine("MEASURE arena receiving: native components and accepted/denied numerical authority, no real transport or complete UI journey.");
        }
        finally
        {
            hud.Free(); RemoveChild(host); host.Free(); RemoveChild(modalHost); modalHost.Free();
        }
    }
    private async Task ArenaModels(Match match, SharedArenaView arenaView)
    {
        match.Players[1].Stone = 100; // Explicit component tower materials, not a campaign earning proof.
        long sequence = 100;
        void Apply(string action) => Check(match.Apply(1, Command.FromSnapshot(match.Snapshot(), sequence++, action, 1, 4, Building.ArrowTower)).Accepted, "native model fixture pays real " + action + " command");
        Apply("build");
        float lower = 0;
        for (int level = 1; level <= 2; level++)
        {
            MatchSnapshot state = match.Snapshot(); var layout = new CombatLayout(new(state.Arena!.Board)) { Arena = state.Arena }; layout.Synchronize(state);
            arenaView.Synchronize(state, layout, true, 1); await Frames();
            TowerState tower = state.Players[0].Towers.Single(t => t.Slot == 4);
            Vector3 origin = arenaView.WeaponOrigin(tower)!.Value;
            Check(origin.DistanceTo(layout.CellPosition(state.Structures.Single(s => s.Kind == ArenaStructureKind.ArrowTower).Cell) + CombatLayout.ArenaOrigin) < 8, "tower cue origin comes from native imported model at its real static cell");
            Check(arenaView.WeaponOrigin(state.Players[0].Defender!) is not null, "Watchpost cue uses existing imported defender resource at physical objective");
            if (level == 1) { lower = origin.Y; Apply("upgrade"); }
            else Check(origin.Y > lower, "accepted tower upgrade rebuilds physical native plinth and raises weapon origin");
        }
        MatchSnapshot roster = match.Snapshot(); var formation = new CombatLayout(new(roster.Arena!.Board)) { Arena = roster.Arena }; formation.Synchronize(roster);
        UnitState[] members = roster.Players[0].Soldiers; var byId = members.ToDictionary(u => u.Id);
        arenaView.Synchronize(roster, formation, false, 1); arenaView.Sample(roster.Tick);
        var summaries = System.Text.Json.JsonSerializer.SerializeToElement(arenaView.Observe(GetViewport().GetCamera3D(),
            GetViewport().GetFinalTransform(), GetViewport().GetVisibleRect()), WireJson.Options);
        Check(summaries.GetArrayLength() == 1 && summaries[0].GetProperty("Members").GetArrayLength() == members.Length,
            "actual native summary exposes the complete current participating group");
        foreach (var summary in summaries[0].GetProperty("Members").EnumerateArray())
        {
            UnitState member = byId[summary.GetProperty("Id").GetInt32()];
            Check(summary.GetProperty("Health").GetInt32() == member.Health && summary.GetProperty("Maximum").GetInt32() == member.Profile.Health
                && summaries[0].GetProperty("Text").GetString()!.Contains($"{HealthPoints.Format(member.Health)}/{HealthPoints.Format(member.Profile.Health)}", StringComparison.Ordinal),
                "loaded native summary text retains each member's authoritative individual HP/max");
        }
        Check(members.Select(u => formation.Position(u, roster.Tick)).Distinct().Count() == members.Length, "accepted per-member formation slots remain distinct on a single squad hex");
        foreach (UnitState member in members)
        {
            var rig = new UnitView(member); AddChild(rig);
            try
            {
                rig.Sample(member, roster.Tick, .1, 0, formation, byId); await Frames();
                Check(rig.Visible && rig.Clip == "idle" && rig.Position.IsEqualApprox(formation.Position(member, roster.Tick)), "genuine native member rig samples accepted shared formation without independent motion");
                Check(rig.State.Health == member.Health && match.Snapshot().Players[0].Soldiers.Single(u => u.Id == member.Id).Health == member.Health, "native pose sampling never feeds cosmetics back into individual health");
                var observed = System.Text.Json.JsonSerializer.SerializeToElement(rig.Observe(), WireJson.Options);
                Check(observed.GetProperty("SquadId").GetInt32() == member.SquadId && observed.GetProperty("FormationSlot").GetInt32() == member.FormationSlot
                    && !observed.TryGetProperty("Size", out _), "production native rig observation carries squad/formation identity, not retired member-size metadata");
            }
            finally { RemoveChild(rig); rig.Free(); }
        }
    }
}
