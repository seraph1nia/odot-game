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
                var baseline = new CombatPlayback(); baseline.Accept(state);
                view.Synchronize(state, layout, true, 1); view.Sample(baseline, CombatPlayback.All(state));
                Check(view.FindChildren("ArenaCell*", "MeshInstance3D", true, false).Count == 63 * players, "native arena component contains every connected cell for " + players + " players");
                Check(view.FindChildren("StructureHealth*", "Label3D", true, false).Count == 0, "Keep and Watchpost never create floating health/name text");
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
                    Check(view.GetNode<BattlefieldPerimeter>("ArenaCloudPerimeter").GetChildren().OfType<MeshInstance3D>().Count(n => n.Mesh is BoxMesh) == 4,
                        "arena keeps continuous cloud banks on every edge without changing global atmosphere");
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
            await HorizontalBoard(view, camera);
            await ArenaReceiving(view);
            await ArenaBufferedSummaries(view, camera);
            await ArenaBufferedRally(view, camera);
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
    private async Task ArenaBufferedSummaries(SharedArenaView arenaView, Camera3D camera)
    {
        using var match = new Match(new Rules
        {
            StartingGold = 100,
            StartingWood = 100,
            RangedHealth = 1,
            RangedDamage = 0,
            DefenderDamage = 0,
            Campaign = new([new(1, false, [new(UnitType.Swordsman, 1)], default), new(2, false, [new(UnitType.Swordsman, 1)], default)])
        }, combatSeed: 1);
        match.Join(); match.Players[1].Metal = 100; match.Players[1].Food = 100;
        long sequence = 0;
        void Act(string action, int slot = -1, Building building = Building.Empty) => Check(match.Apply(1,
            Command.FromSnapshot(match.Snapshot(), ++sequence, action, 1, slot, building, UnitType.Crossbowman)).Accepted, "buffered native fixture accepts " + action);
        Act("start"); Act("build", 0, Building.ArcheryRange); Act("recruit", 0);
        for (int gate = 0; gate < 3; gate++) Act("ready");
        var arena = new SharedArena([1]);
        int memberId = match.Players[1].Soldiers.Single().Id;
        HexTransition move = arena.Board.Transition(new(17, 1), new(18, 1));
        match.Combat.Seed(match.Combat.Inspect(memberId) with
        {
            Location = new(UnitLifecycle.Alive, move.Source),
            Action = new CombatAction.Moving(1, move.Destination, move.Id, 0, 3)
        });
        int squadId = match.Combat.Read(memberId).SquadId;
        int enemyId = match.Combat.Enemies().Single().Id;
        int neighbor = arena.Board.Cell(18).Neighbors.First(c => c != 17 && arena.CanOccupy(c));
        match.Combat.Seed(match.Combat.Inspect(enemyId) with
        {
            Location = new(UnitLifecycle.Alive, new(neighbor, 1)),
            Action = new CombatAction.Windup(1, 1, 30, new(new(CombatTargetKind.Unit, squadId), 0, 6))
        });
        MatchSnapshot Receive() => SnapshotPayload.DecodeForReceiving(SnapshotPayload.Encode(match.Snapshot()));
        int entryId = match.Combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons);
        int entrySquad = match.Combat.Read(entryId).SquadId;
        MatchSnapshot accepted = Receive();
        var playback = new CombatPlayback(); Check(playback.Accept(accepted), "initial compressed buffered arena frame is accepted");
        double secondsPerTick = 1.0 / (Match.StepsPerSecond * CombatSpeedRates.Factor(accepted.CombatSpeed));
        var layout = new CombatLayout(new(accepted.Arena!.Board)); layout.Synchronize(accepted);
        var rig = new UnitView(accepted.Players[0].Soldiers.Single()); AddChild(rig);
        var entryRig = new UnitView(accepted.Enemies.Single(u => u.Id == entryId)); AddChild(entryRig);
        var bar = new UnitHealthBar(); AddChild(bar);
        try
        {
            async Task Sample(bool alive)
            {
                layout.Synchronize(accepted); arenaView.Synchronize(accepted, layout, false, 1);
                var units = playback.Units().ToDictionary(u => u.Id);
                arenaView.Sample(playback, units.Values);
                rig.Sample(units[memberId], playback.Tick, playback.VisualSeconds, 0, layout, units);
                entryRig.Sample(units[entryId], playback.Tick, playback.VisualSeconds, 0, layout, units);
                bar.Sample(rig, camera, new Rect2(-100000, -100000, 200000, 200000));
                Check(bar.Current == rig.State.Health && bar.Maximum == rig.State.Profile.Health && (!alive ? !bar.Visible : bar.Fraction > 0),
                    "actual member bar shares buffered health and disappears at death, not on snapshot receipt");
                await Frames();
                Check(entryRig.Visible == (playback.Tick >= 1)
                    && (arenaView.GetNodeOrNull<Node3D>("SquadSummary" + entrySquad) is not null) == (playback.Tick >= 1),
                    "native entry model and summary wait for their shared admission tick");
                Node3D? label = arenaView.GetNodeOrNull<Node3D>("SquadSummary" + squadId);
                Check(rig.Visible && rig.Dead != alive && (label is not null) == alive,
                    "native summary membership agrees with the buffered live/dead rig");
                if (!alive) return;
                UnitState member = units[memberId];
                System.Numerics.Vector3 formation = SharedCombatProjection.Formation(member.FormationSlot, accepted.Rules.Arena.SquadCapacity, member.IsBoss);
                Check(label!.GlobalPosition.IsEqualApprox(rig.Position - new Vector3(formation.X, formation.Y, formation.Z) + Vector3.Up * 1.6f),
                    "native summary and model share the same buffered actor anchor");
                var observed = System.Text.Json.JsonSerializer.SerializeToElement(arenaView.Observe(camera,
                    GetViewport().GetFinalTransform(), GetViewport().GetVisibleRect()), WireJson.Options);
                var summary = observed.EnumerateArray().Single(s => s.GetProperty("Id").GetInt32() == squadId);
                Check(summary.GetProperty("Members").GetArrayLength() == 1
                    && summary.GetProperty("Members")[0].GetProperty("Health").GetInt32() == member.Health
                    && summary.GetProperty("Text").GetString() == "",
                    "nonvisual squad diagnostics expose buffered member health without floating text");
            }
            await Sample(true);
            match.Step(); accepted = Receive(); playback.Accept(accepted); await Sample(true);
            match.Step(); accepted = Receive(); playback.Accept(accepted); await Sample(true);
            Check(playback.Tick == 0 && !entryRig.Visible, "another admitted revision cannot discard the queued native pose");
            playback.Advance(secondsPerTick, true); await Sample(true);
            Check(playback.Tick == 1 && entryRig.Visible, "native admission activates exactly at its declared tick");
            while (match.Tick < 3) match.Step();
            accepted = Receive(); Check(playback.Accept(accepted) && playback.Tick < 3, "movement completion arrives ahead of the unpaused presentation clock");
            Check(accepted.Players[0].Soldiers.Single().Hex!.Position == move.Destination, "authority completes the committed move");
            await Sample(true);
            match.Step(); accepted = Receive(); Check(playback.Accept(accepted) && playback.Tick < 3, "another accepted arrival revision cannot discard the buffered move");
            await Sample(true);
            Check(rig.State.Hex!.Action == UnitActionKind.Moving && rig.State.Hex.Position == move.Source, "native rig retains the committed route across overlapping arrival frames");
            while (playback.Tick < 3) { playback.Advance(secondsPerTick, true); await Sample(true); }
            Check(playback.Tick == 3 && rig.State.Hex!.Position == move.Destination, "buffered model and summary reach movement completion together");
            while (match.Tick < 6) match.Step();
            accepted = Receive(); Check(playback.Accept(accepted) && playback.Tick < 6, "final-member death arrives ahead of the unpaused presentation clock");
            Check(accepted.DyingBodies.Single().Id == memberId && accepted.Players[0].Soldiers.Length == 0, "authority publishes a genuine last-member death");
            await Sample(true);
            match.Step(); accepted = Receive(); Check(playback.Accept(accepted) && playback.Tick < 6, "tick-seven death revision arrives before the shared death boundary");
            await Sample(true);
            while (playback.Tick < 5) { playback.Advance(secondsPerTick, true); await Sample(true); }
            playback.Advance(secondsPerTick, true); await Sample(false);
            Check(playback.Tick == 6 && rig.Clip == "death", "native summary disappears only when the buffered rig starts its death clip");
            Act("pause"); accepted = Receive(); playback.Accept(accepted);
            double pausedTick = playback.Tick; playback.Advance(1, true); await Sample(false);
            Check(playback.Tick == pausedTick && rig.Clip == "death", "pause retains the sampled corpse and native summary absence");
            playback.Accept(accepted, baseline: true); await Sample(false);
            Check(playback.Tick == accepted.Tick && playback.Drain().Length == 0, "baseline selects the received death without replay");
            int generation = playback.Generation;
            MatchSnapshot gap = accepted with
            {
                Revision = accepted.Revision + 1,
                EventSequence = accepted.EventSequence + 2,
                OldestEventSequence = accepted.EventSequence + 3,
                CombatEvents = []
            };
            Check(playback.Accept(gap) && playback.Generation == generation + 1, "event gap discards buffered member state");
            await Sample(false);
        }
        finally { RemoveChild(bar); bar.Free(); RemoveChild(rig); rig.Free(); RemoveChild(entryRig); entryRig.Free(); }
        await ArenaBufferedBoundaries(arenaView, camera);
    }
    private async Task ArenaBufferedBoundaries(SharedArenaView arenaView, Camera3D camera)
    {
        foreach (string transition in new[] { "admission", "arrival", "action" })
        {
            using var match = new Match(new Rules { StartingGold = 100, StartingWood = 100, RangedHealth = 1, RangedDamage = 0, DefenderDamage = 0 }, combatSeed: 1);
            match.Join(); match.Players[1].Metal = 100; match.Players[1].Food = 100;
            long sequence = 0;
            void Act(string action, int slot = -1, Building building = Building.Empty) => Check(match.Apply(1,
                Command.FromSnapshot(match.Snapshot(), ++sequence, action, 1, slot, building, UnitType.Crossbowman)).Accepted, "native two-boundary fixture accepts " + action);
            Act("start"); Act("build", 0, Building.ArcheryRange); Act("recruit", 0);
            for (int gate = 0; gate < 3; gate++) Act("ready");
            var arena = new SharedArena([1]);
            int memberId = match.Players[1].Soldiers.Single().Id;
            int squadId = match.Combat.Read(memberId).SquadId;
            int enemyId = match.Combat.Enemies()[0].Id;
            foreach (UnitState extra in match.Combat.Enemies().Where(u => u.Id != enemyId)) match.Combat.Remove(extra.Id);
            HexTransition move = arena.Board.Transition(new(17, 1), new(18, 1));
            int endpoint = transition == "admission" ? 1 : 18;
            int enemyCell = transition == "admission" ? 2 : arena.Board.Cell(endpoint).Neighbors.First(c => c != 17 && arena.CanOccupy(c));
            match.Combat.Seed(match.Combat.Inspect(enemyId) with
            {
                Location = new(UnitLifecycle.Alive, new(enemyCell, 1)),
                Action = new CombatAction.Windup(1, 1, 30, new(new(CombatTargetKind.Unit, squadId), 0, 7))
            });
            match.Combat.Seed(match.Combat.Inspect(memberId) with
            {
                Location = new(transition == "admission" ? UnitLifecycle.Queued : UnitLifecycle.Alive,
                    transition == "admission" ? default : transition == "arrival" ? move.Source : move.Destination),
                Action = transition == "admission" ? new CombatAction.Waiting() : transition == "arrival"
                    ? new CombatAction.Moving(1, move.Destination, move.Id, 0, 6) : new CombatAction.Recovery(1, 0, 6)
            });
            int entryBlocker = 0;
            if (transition == "admission")
                foreach (int cell in arena.EnemyCells(1).Where(c => c != enemyCell))
                {
                    int id = match.Combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons, isBoss: true);
                    match.Combat.Seed(match.Combat.Inspect(id) with { Location = new(UnitLifecycle.Alive, new(cell, 1)), Action = new CombatAction.Recovery(1, 0, 100) });
                    if (cell == endpoint) entryBlocker = id;
                }
            while (match.Tick < 3) match.Step();
            MatchSnapshot Receive() => SnapshotPayload.DecodeForReceiving(SnapshotPayload.Encode(match.Snapshot()));
            MatchSnapshot accepted = Receive();
            UnitState initial = accepted.Players[0].Soldiers.Single();
            var playback = new CombatPlayback(); playback.Accept(accepted);
            var layout = new CombatLayout(new(accepted.Arena!.Board));
            var rig = new UnitView(initial); AddChild(rig);
            double secondsPerTick = 1.0 / (Match.StepsPerSecond * CombatSpeedRates.Factor(accepted.CombatSpeed));
            try
            {
                async Task Sample(UnitState expected)
                {
                    layout.Synchronize(accepted); arenaView.Synchronize(accepted, layout, false, 1);
                    var units = playback.Units().ToDictionary(u => u.Id);
                    arenaView.Sample(playback, units.Values);
                    rig.Sample(units[memberId], playback.Tick, playback.VisualSeconds, 0, layout, units);
                    await Frames();
                    Check(rig.State.Hex == expected.Hex && rig.State.Health == expected.Health && rig.State.AttackSequence == expected.AttackSequence,
                        "native " + transition + " model retains each accepted boundary at " + playback.Tick);
                    bool alive = expected.Hex!.Lifecycle == UnitLifecycle.Alive;
                    Node3D? label = arenaView.GetNodeOrNull<Node3D>("SquadSummary" + squadId);
                    Check(rig.Visible == expected.Deployed && rig.Dead == (expected.Hex.Lifecycle == UnitLifecycle.Dying) && (label is not null) == alive,
                        "native " + transition + " summary and model share membership at " + playback.Tick);
                    if (!alive) return;
                    System.Numerics.Vector3 formation = SharedCombatProjection.Formation(expected.FormationSlot, accepted.Rules.Arena.SquadCapacity, expected.IsBoss);
                    Check(label!.GlobalPosition.IsEqualApprox(rig.Position - new Vector3(formation.X, formation.Y, formation.Z) + Vector3.Up * 1.6f),
                        "native intermediate " + transition + " pose agrees with its summary anchor");
                    var observed = System.Text.Json.JsonSerializer.SerializeToElement(arenaView.Observe(camera,
                        GetViewport().GetFinalTransform(), GetViewport().GetVisibleRect()), WireJson.Options);
                    Check(observed.EnumerateArray().Single(s => s.GetProperty("Id").GetInt32() == squadId).GetProperty("Members")[0].GetProperty("Health").GetInt32() == expected.Health,
                        "native intermediate summary exposes the admitted living member's health");
                }
                await Sample(initial);
                while (match.Tick < 5) match.Step();
                if (entryBlocker > 0) match.Combat.Remove(entryBlocker);
                match.Step(); accepted = Receive(); playback.Accept(accepted);
                UnitState intermediate = accepted.Players[0].Soldiers.Single();
                Check(intermediate.Hex!.Lifecycle == UnitLifecycle.Alive && intermediate.Hex.Position.Cell == endpoint
                    && intermediate.Hex.Action == UnitActionKind.Windup && intermediate.Hex.StartTick == 6,
                    "authority publishes genuine admission/arrival/action at six before lethal impact");
                if (transition == "admission") Check(intermediate.Hex.AdmittedTick == 6, "authority admits the blocked queued member exactly at six");
                match.Step(); accepted = Receive(); playback.Accept(accepted);
                UnitState dead = accepted.DyingBodies.Single();
                Check(dead.Id == memberId && dead.Hex!.DeathStartTick == 7 && playback.Tick == 4,
                    "genuine compressed death at seven arrives before the intermediate boundary is sampled");
                await Sample(initial);
                playback.Advance(secondsPerTick, true); await Sample(initial);
                playback.Advance(secondsPerTick, true); await Sample(intermediate);
                Check(playback.Tick == 6 && !rig.Dead && rig.Visible, "native intermediate state is alive exactly at six");
                playback.Advance(secondsPerTick, true); await Sample(dead);
                Check(playback.Tick == 7 && rig.Clip == "death", "native intermediate member dies exactly at seven");
            }
            finally { RemoveChild(rig); rig.Free(); }
        }
    }
    private async Task ArenaBufferedRally(SharedArenaView arenaView, Camera3D camera)
    {
        using var match = new Match(new Rules { StartingGold = 100, StartingWood = 100, RangedDamage = 0, DefenderDamage = 0, Arena = new() { WatchpostHealth = 1, RallyDurationTicks = 6 } }, combatSeed: 1);
        match.Join();
        Check(match.Apply(1, Command.FromSnapshot(match.Snapshot(), 1, "start", 1)).Accepted, "Rally component authority starts");
        match.Players[1].Stone = 100;
        Check(match.Apply(1, Command.FromSnapshot(match.Snapshot(), 2, "build", 1, 0, Building.ArrowTower)).Accepted,
            "objective overlay fixture pays for a real invulnerable purchased tower");
        camera.Position = CombatLayout.ArenaOrigin + new Vector3(15, 25, 25); camera.LookAt(CombatLayout.ArenaOrigin);
        int memberId = match.Combat.Create(UnitType.Crossbowman, 1, 1, 1, assignment: new(-1, 0, 0, 1));
        match.Combat.RestoreHomes();
        for (int gate = 0; gate < 3; gate++)
            Check(match.Apply(1, Command.FromSnapshot(match.Snapshot(), 2, "ready", 1)).Accepted, "Rally component settles an ordinary END");
        var arena = new SharedArena([1]);
        match.Combat.Seed(match.Combat.Inspect(memberId) with { Location = new(UnitLifecycle.Alive, new(17, 1)) });
        int squadId = match.Combat.Read(memberId).SquadId;
        int enemyId = match.Combat.Enemies()[0].Id;
        foreach (UnitState extra in match.Combat.Enemies().Where(u => u.Id != enemyId)) match.Combat.Remove(extra.Id);
        int approach = arena.Board.Cell(arena.Watchpost(1)).Neighbors.First(arena.CanOccupy);
        match.Combat.Seed(match.Combat.Inspect(enemyId) with
        {
            Location = new(UnitLifecycle.Alive, new(approach, 1)),
            Action = new CombatAction.Windup(1, 1, 30, new(new(CombatTargetKind.Watchpost, 1), 0, 3))
        });
        MatchSnapshot Receive() => SnapshotPayload.DecodeForReceiving(SnapshotPayload.Encode(match.Snapshot()));
        MatchSnapshot accepted = Receive(); var playback = new CombatPlayback(); playback.Accept(accepted);
        var layout = new CombatLayout(new(accepted.Arena!.Board));
        var rig = new UnitView(accepted.Players[0].Soldiers.Single()); AddChild(rig);
        var bars = new Control(); AddChild(bars);
        double secondsPerTick = 1.0 / (Match.StepsPerSecond * CombatSpeedRates.Factor(accepted.CombatSpeed));
        try
        {
            async Task Sample(bool disrupted)
            {
                layout.Synchronize(accepted); arenaView.Synchronize(accepted, layout, false, 1);
                var units = playback.Units().ToDictionary(u => u.Id);
                arenaView.Sample(playback, units.Values);
                rig.Sample(units[memberId], playback.Tick, playback.VisualSeconds, 0, layout, units);
                arenaView.UpdateHealthBars(bars, camera, new Rect2(-100000, -100000, 200000, 200000));
                UnitHealthBar post = bars.GetNode<UnitHealthBar>("StructureHealth_1_Watchpost_-1");
                Check(post.Current == (playback.Tick < 3 ? HealthPoints.FromWhole(1) : 0) && post.Maximum == HealthPoints.FromWhole(1)
                    && post.Visible == (playback.Tick < 3), "actual objective bar waits for buffered damage/death and never lingers over the ruin");
                Check(bars.GetChildCount() == 2 && accepted.Structures.Length == 3,
                    "only meaningful Keep/Watchpost HP creates bars; the purchased 1/1 sentinel never fabricates health");
                arenaView.Visible = false; arenaView.UpdateHealthBars(bars, camera, new Rect2(-100000, -100000, 200000, 200000));
                Check(bars.GetChildren().OfType<UnitHealthBar>().All(b => !b.Visible), "hidden arena cannot leave objective bars visible");
                arenaView.Visible = true; arenaView.UpdateHealthBars(bars, camera, new Rect2(-100000, -100000, 200000, 200000));
                Check(arenaView.GetNode<Node3D>("Watchpost_1_-1").GetChild<Node3D>(0).Visible == (playback.Tick < 3),
                    "objective model and health bar switch to the ruin on the same clock");
                await Frames();
                Check(rig.Visible && !rig.Dead && (playback.Tick < playback.RallyDisruptions().GetValueOrDefault(squadId)) == disrupted
                    && arenaView.FindChildren("SquadSummary*", "Label3D", true, false).Count == 0,
                    "Rally retains its buffered state without restoring floating squad text at " + playback.Tick);
            }
            for (int tick = 0; tick < 3; tick++) match.Step();
            accepted = Receive(); Check(playback.Accept(accepted), "compressed Watchpost loss enters playback");
            Check(accepted.CombatEvents.Any(e => e.Type == CombatEventType.RallyDisrupted && e.Tick == 3), "real Watchpost loss emits the Rally transition");
            await Sample(false);
            match.Step(); accepted = Receive(); playback.Accept(accepted); await Sample(false);
            while (playback.Tick < 3) { playback.Advance(secondsPerTick, true); await Sample(playback.Tick >= 3); }
            Check(match.Apply(1, Command.FromSnapshot(match.Snapshot(), 3, "pause", 1)).Accepted, "Rally component pauses");
            accepted = Receive(); playback.Accept(accepted); await Sample(true);
            double pausedTick = playback.Tick; playback.Advance(1, true); await Sample(true);
            Check(playback.Tick == pausedTick, "pause freezes Rally and model sampling together");
            Check(match.Apply(1, Command.FromSnapshot(match.Snapshot(), 4, "resume", 1)).Accepted, "Rally component resumes");
            accepted = Receive(); playback.Accept(accepted);
            while (match.Tick < 9) match.Step();
            accepted = Receive(); playback.Accept(accepted); await Sample(true);
            match.Step(); accepted = Receive(); playback.Accept(accepted); await Sample(true);
            while (playback.Tick < 9) { playback.Advance(secondsPerTick, true); await Sample(playback.Tick < 9); }
            playback.Drain(); await Sample(false);
            ulong[] oldBars = bars.GetChildren().Select(n => n.GetInstanceId()).ToArray();
            using var replacement = new Match(); replacement.Join();
            MatchSnapshot fresh = replacement.Snapshot(); var freshLayout = new CombatLayout(new(fresh.Arena!.Board)); freshLayout.Synchronize(fresh);
            arenaView.Synchronize(fresh, freshLayout, false, 1); await Frames();
            Check(oldBars.All(id => !GodotObject.IsInstanceIdValid(id)) && bars.GetChildCount() == 0,
                "arena replacement detaches and actually frees externally parented objective bars");
        }
        finally { RemoveChild(rig); rig.Free(); RemoveChild(bars); bars.Free(); }
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
            Check(arenaView.FindChildren("StructureHealth*", "Label3D", true, false).Count == 0,
                "purchased tower upgrade never restores floating owner/name/level text");
            TowerState tower = state.Players[0].Towers.Single(t => t.Slot == 4);
            Vector3 origin = arenaView.WeaponOrigin(tower)!.Value;
            Check(origin.DistanceTo(layout.CellPosition(state.Structures.Single(s => s.Kind == ArenaStructureKind.ArrowTower).Cell) + CombatLayout.ArenaOrigin) < 8, "tower cue origin comes from native imported model at its real static cell");
            Check(arenaView.WeaponOrigin(state.Players[0].Defender!) is not null, "Watchpost cue uses existing imported defender resource at physical objective");
            if (level == 1) { lower = origin.Y; Apply("upgrade"); }
            else Check(origin.Y > lower, "accepted tower upgrade rebuilds physical native plinth and raises weapon origin");
        }
        MatchSnapshot roster = match.Snapshot(); var formation = new CombatLayout(new(roster.Arena!.Board)) { Arena = roster.Arena }; formation.Synchronize(roster);
        UnitState[] members = roster.Players[0].Soldiers; var byId = members.ToDictionary(u => u.Id);
        var baseline = new CombatPlayback(); baseline.Accept(roster);
        arenaView.Synchronize(roster, formation, false, 1); arenaView.Sample(baseline, members);
        var summaries = System.Text.Json.JsonSerializer.SerializeToElement(arenaView.Observe(GetViewport().GetCamera3D(),
            GetViewport().GetFinalTransform(), GetViewport().GetVisibleRect()), WireJson.Options);
        Check(summaries.GetArrayLength() == 1 && summaries[0].GetProperty("Members").GetArrayLength() == members.Length,
            "actual native summary exposes the complete current participating group");
        foreach (var summary in summaries[0].GetProperty("Members").EnumerateArray())
        {
            UnitState member = byId[summary.GetProperty("Id").GetInt32()];
            Check(summary.GetProperty("Health").GetInt32() == member.Health && summary.GetProperty("Maximum").GetInt32() == member.Profile.Health
                && summaries[0].GetProperty("Text").GetString() == "",
                "nonvisual squad diagnostics retain each member's authoritative individual HP/max");
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
