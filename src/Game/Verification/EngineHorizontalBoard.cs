using Game.Core;
using Godot;

namespace Game.Verification;

public partial class EngineTests
{
    // Tier B admission: direct rules tests cannot observe imported UnitView roots,
    // native facing/picking, camera safe regions or river resource ports. Reuse the
    // existing minimal arena owner; no application, session, display or pixel proof.
    // Numerical equipment supplies are explicit; estimated cost a few seconds.
    private async Task HorizontalBoard(SharedArenaView arena, Camera3D camera)
    {
        var figures = new Node3D(); AddChild(figures);
        var districtRoot = new Node3D(); AddChild(districtRoot);
        try
        {
            using var match = new Match(new() { StartingGold = 100, StartingWood = 100 }, combatSeed: 123);
            match.Join(); match.Join();
            long sequence = 0;
            void Act(int owner, string action, int slot = -1, Building building = Building.Empty)
                => Check(match.Apply(owner, Command.FromSnapshot(match.Snapshot(), ++sequence, action, owner, slot, building)).Accepted, "parity fixture ordinary " + action);
            Act(1, "start");
            foreach (City city in match.Players.Values)
            {
                city.Metal = 100; city.Food = 100;
                Act(city.Id, "build", 0, Building.Barracks);
                for (int member = 0; member < 4; member++) Act(city.Id, "recruit", 0);
            }
            for (int turn = 0; turn < 2; turn++) { Act(1, "ready"); Act(2, "ready"); }
            MatchSnapshot prepared = match.Snapshot();
            var layout = new CombatLayout(new(prepared.Arena!.Board)); layout.Synchronize(prepared);
            UnitState[] projected = BattleDeployment.Preview(prepared);
            var units = projected.ToDictionary(u => u.Id);
            var views = new Dictionary<int, UnitView>();
            foreach (UnitState member in projected.Where(u => u.Deployed))
            {
                var view = new UnitView(member); figures.AddChild(view); views.Add(member.Id, view);
                view.Sample(member, prepared.Tick, 0, 0, layout, units, deployment: true);
            }
            var before = views.ToDictionary(p => p.Key, p => p.Value.GlobalTransform);
            Aabb content = BattlefieldFrame.Content(prepared, []);
            Vector3 origin = CombatLayout.DistrictOrigin(prepared);
            districtRoot.Position = origin;
            var landscape = new VillageLandscape(districtRoot, new LandscapeAssets(), district: true);
            var navigation = new TabletopCamera(camera);
            foreach (Vector2I size in new[] { new Vector2I(1100, 820), new(1280, 720), new(1600, 900), new(1920, 1080), new(2560, 1080), new(820, 1100) })
            {
                GetWindow().Size = size; await Frames();
                Rect2 viewport = GetViewport().GetVisibleRect(); float hud = viewport.Size.Y * .32f;
                BattlefieldFrame.FitCamera(camera, content, origin, viewport.Size, hud, rightLane: 408);
                Rect2 world = BattlefieldFrame.GroundRect(content); world.Position += new Vector2(origin.X, origin.Z);
                navigation.Fit(world, viewport, true); navigation.Reset();
                // A 2D opaque footprint can still pass while the near plane cuts
                // through the raised cloud backstop (the portrait green-strip defect).
                // Native ray/depth checks are cheaper than a graphical journey;
                // keep both surround heights inside the actual clipping range.
                foreach (Vector2 pixel in new[] { viewport.Position, viewport.End })
                    foreach (float height in new[] { BattlefieldFrame.Ground, BattlefieldFrame.Roof })
                    {
                        Vector3 start = camera.ProjectRayOrigin(pixel), ray = camera.ProjectRayNormal(pixel);
                        Vector3 hit = start + ray * ((height - start.Y) / ray.Y);
                        float depth = (camera.GlobalPosition - hit).Dot(camera.GlobalBasis.Z);
                        Check(depth >= camera.Near && depth <= camera.Far, "entire cloud/backstop height range survives native near/far clipping at " + size);
                    }
                Rect2 safe = new(new Vector2(0, 64), new(viewport.Size.X - 408, viewport.Size.Y - hud - 64));
                foreach (int corner in Enumerable.Range(0, 8))
                    Check(safe.Grow(1).HasPoint(camera.UnprojectPosition(content.GetEndpoint(corner) + origin)), "landscape arrangement fits all actionable content outside top/right/bottom UI at " + size);
                foreach (int slot in Enumerable.Range(0, 9))
                {
                    Vector3 center = origin + VillageLayout.Slot(slot);
                    Vector2 pixel = camera.UnprojectPosition(center);
                    Vector3 ray = camera.ProjectRayNormal(pixel), start = camera.ProjectRayOrigin(pixel);
                    Vector3 hit = start + ray * ((center.Y - start.Y) / ray.Y);
                    Check(VillageLayout.Contains(slot, hit - origin), "native projected horizontal building center picks its unchanged logical slot");
                }
                arena.Synchronize(prepared, layout, true, 1);
                arena.CoverBoundary(navigation.Boundary, world);
                landscape.CoverBattlefield(BattlefieldFrame.GroundRect(content), new(navigation.Boundary.Position - new Vector2(origin.X, origin.Z), navigation.Boundary.Size));
                BattlefieldPerimeter[] surrounds = arena.FindChildren("*", "Node3D", true, false).Concat(districtRoot.FindChildren("*", "Node3D", true, false)).OfType<BattlefieldPerimeter>().ToArray();
                Check(surrounds.Length == 1 && surrounds[0].GetParent() == arena, "combined board has exactly one arena-owned surround at " + size);
                MeshInstance3D[] slabs = surrounds[0].GetChildren().OfType<MeshInstance3D>().Where(n => n.Mesh is BoxMesh).ToArray();
                Check(slabs.Length == 4 && surrounds[0].GetChildren().OfType<MeshInstance3D>().Any(n => n.Mesh is SphereMesh)
                    && surrounds[0].Material.Transparency == BaseMaterial3D.TransparencyEnum.Disabled, "single surround retains all four opaque backstops and cloud banks");
                Rect2 inner = world.Grow(BattlefieldFrame.Margin), outer = navigation.Boundary.Grow(20);
                foreach (float x in new[] { outer.Position.X, inner.GetCenter().X, outer.End.X })
                    foreach (float z in new[] { outer.Position.Y, inner.GetCenter().Y, outer.End.Y })
                        if (!inner.HasPoint(new(x, z)))
                            Check(slabs.Any(n => BattlefieldFrame.GroundRect(n.GlobalTransform * n.Mesh.GetAabb()).Grow(.01f).HasPoint(new(x, z))), "one physical surround covers every combined-board edge/corner at " + size);
                var firstRow = prepared.Arena.Board.Cells.Where(c => c.Coordinate.R == 0).Select(c => camera.UnprojectPosition(CombatLayout.ArenaOrigin + layout.CellPosition(c.Id))).ToArray();
                Check(firstRow.All(p => Math.Abs(p.Y - firstRow[0].Y) < .01f), "actual native projection reads arena enemy cells in a horizontal row");
                navigation.ZoomAt(viewport.GetCenter(), true, 40); navigation.Drag(new(10000, 0, 10000), viewport.GetCenter());
                Check(navigation.Boundary.Grow(.02f).Encloses(BattlefieldFrame.Footprint(camera, viewport)), "compact landscape retains bounded user zoom/pan and opaque edge coverage");
                Check(BattlefieldFrame.GroundRect(arena.OpaqueBounds).Encloses(navigation.Boundary), "physical arena backstop covers complete combined board/cloud bounds");
                Console.WriteLine($"MEASURE horizontal-board size={size} overview={camera.Size * navigation.Zoom:F3} content={content}");
            }
            using (var solo = new Match())
            {
                solo.Join();
                GetWindow().Size = new(1100, 820); await Frames();
                Vector2 size = GetViewport().GetVisibleRect().Size; float hud = size.Y * .32f;
                BattlefieldFrame.FitCamera(camera, new(new Vector3(-10.5f, -1.5f, -15), new Vector3(22.5f, 6.7f, 30)), Vector3.Zero, size, hud, rightLane: 353);
                float former = camera.Size;
                MatchSnapshot state = solo.Snapshot();
                BattlefieldFrame.FitCamera(camera, BattlefieldFrame.Content(state, []), CombatLayout.DistrictOrigin(state), size, hud, rightLane: 353);
                Check(camera.Size < former, "compact solo landscape fits closer than former padded framing with identical native fitter/UI/aspect inputs");
                Console.WriteLine($"MEASURE horizontal-solo overview={camera.Size:F3} former-padded-same-fitter={former:F3}");
            }
            // Read actual imported ribbon geometry at the new divider transforms.
            var assets = new LandscapeAssets();
            var river = VillageLandscape.Divider.ToArray();
            for (int index = 1; index < river.Length; index++)
            {
                var a = river[index - 1]; var b = river[index];
                int edge = Enumerable.Range(0, 6).Single(e => DetailedGround.Neighbor(a.Column, a.Row, e) == (b.Column, b.Row));
                Vector3 from = GroundGeometry.Port(assets.Terrain(a.Connector.Asset), landscape.GroundTransform(a.Column, a.Row, a.Connector.Turns), (edge - a.Connector.Turns + 6) % 6);
                Vector3 to = GroundGeometry.Port(assets.Terrain(b.Connector.Asset), landscape.GroundTransform(b.Column, b.Row, b.Connector.Turns), ((edge + 3) % 6 - b.Connector.Turns + 6) % 6);
                Check(from.DistanceTo(to) < .002f, "reused native river connectors meet in all three dimensions across the vertical divider");
            }
            Node3D bridge = landscape.GetChildren().OfType<Node3D>().First(n => n.HasMeta("asset") && n.GetMeta("asset").AsString() == AssetCatalog.Bridge);
            Check(bridge.Position.IsEqualApprox(VillageLayout.Hex(2, 4)) && !bridge.FindChildren("*", "Node3D", true, false).Any(GroundGeometry.Recessed),
                "actual reused timber bridge crosses divider without recessed substrate or second water surface");
            Node3D[] waters = landscape.FindChildren("*", "Node3D", true, false).OfType<Node3D>()
                .Where(n => n.HasMeta("asset") && n.GetMeta("asset").AsString().Contains("hex_river_overlay", StringComparison.Ordinal)).ToArray();
            Check(waters.Length == 9 && waters.All(n => Math.Abs(landscape.ToLocal(n.GlobalPosition).Y + .03f) < .001f),
                "actual native animated river owners retain the lowered continuous channel, not the old horizontal watercourse");
            Act(1, "ready"); Act(2, "ready");
            MatchSnapshot entered = match.Snapshot();
            Check(entered.Tick == prepared.Tick && entered.Phase == Phase.Combat, "parity comparison observes actual combat entry, never an advanced simulation");
            layout.Synchronize(entered);
            arena.Synchronize(entered, layout, false, 1);
            Check(arena.FindChildren("*", "Node3D", true, false).Concat(districtRoot.FindChildren("*", "Node3D", true, false)).OfType<BattlefieldPerimeter>().Count() == 1,
                "actual combat transition retains the single combined-board surround");
            var live = entered.Players.SelectMany(c => c.Soldiers).Concat(entered.Enemies).ToDictionary(u => u.Id);
            UnitState[] enemies = projected.Where(u => u.Id < 0).ToArray();
            foreach (UnitState member in live.Values.Where(u => u.Deployed))
            {
                int id = member.Faction == Faction.Skeletons ? enemies[Array.FindIndex(entered.Enemies, u => u.Id == member.Id)].Id : member.Id;
                UnitView view = views[id];
                view.Sample(member, entered.Tick, 0, 0, layout, live);
                Check(view.GlobalTransform.IsEqualApprox(before[id]) && view.Clip == "idle", "actual native member root/facing/formation is identical across preview -> combat start");
                BattlefieldFrame.FitCamera(camera, content, origin, GetViewport().GetVisibleRect().Size, 200);
                Aabb bounds = view.PickingBounds();
                Vector2 pixel = camera.UnprojectPosition(new(bounds.GetCenter().X, bounds.End.Y - .02f, bounds.GetCenter().Z));
                Check(UnitPicking.Pick(views.Values, camera, pixel) == view, "exposed imported member remains ray-pickable among all co-op/enemy rigs after actual entry");
            }
            await ArenaInspection(camera);
        }
        finally
        {
            RemoveChild(figures); figures.Free(); RemoveChild(districtRoot); districtRoot.Free();
            WoodlandMotion.ReleaseDerivedMaterials();
        }
    }
    private async Task ArenaInspection(Camera3D camera)
    {
        using Theme theme = ApplicationTheme.Create();
        var host = new Control { Theme = theme, MouseFilter = Control.MouseFilterEnum.Ignore }; AddChild(host); host.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var figures = new Node3D(); AddChild(figures);
        using var match = new Match(new() { StartingGold = 100, StartingWood = 100 }, combatSeed: 123);
        match.Join(); match.Join();
        long sequence = 0;
        void Act(int owner, string action, int slot = -1, Building building = Building.Empty)
            => Check(match.Apply(owner, Command.FromSnapshot(match.Snapshot(), ++sequence, action, owner, slot, building)).Accepted, "inspection fixture ordinary " + action);
        Act(1, "start");
        foreach (City city in match.Players.Values)
        {
            city.Metal = 100; city.Food = 100;
            Act(city.Id, "build", 0, Building.Barracks); Act(city.Id, "recruit", 0);
        }
        int corpse = match.Combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons);
        match.Combat.Seed(match.Combat.Inspect(corpse) with
        {
            Health = 0,
            Location = new(UnitLifecycle.Dying, new(match.Snapshot().Arena!.Sectors[0].EnemyCells[0], 1), DeathEndTick: 100, FrozenTick: 0)
        });
        Act(1, "recruit", 0);
        int recruited = match.Players[1].Soldiers[^1].Id;
        var session = new ReceivingSession(match); session.Acknowledge();
        var hud = new Tabletop(session);
        bool modal = false;
        try
        {
            hud.InitializeHud(host, dialog => host.AddChild(dialog), () => modal, () => { });
            hud.RefreshHud(1, -1); await Frames();
            UnitInspector inspector = host.GetNode<UnitInspector>("UnitInspector");
            Button retire = inspector.FindChildren("RetireUnit", "Button", true, false).OfType<Button>().Single();
            Button store = inspector.FindChildren("StoreUnit", "Button", true, false).OfType<Button>().Single();
            MatchSnapshot state = session.State;
            Check(match.Combat.HasDeaths && !state.Players[0].Soldiers.Single(u => u.Id == recruited).Deployed, "real paid recruit is not snapshot-deployed while a corpse is being cleaned up");
            var layout = new CombatLayout(new(state.Arena!.Board)); layout.Synchronize(state);
            var projected = BattleDeployment.Preview(state).ToDictionary(u => u.Id);
            var views = new Dictionary<int, UnitView>();
            foreach (UnitState unit in projected.Values.Where(u => u.Deployed))
            {
                var view = new UnitView(unit); figures.AddChild(view); views.Add(unit.Id, view);
                view.Sample(unit, state.Tick, 0, 0, layout, projected, deployment: true);
            }
            int commands = session.Commands.Count;
            foreach (UnitView view in views.Values)
            {
                Aabb bounds = view.PickingBounds(); Vector3 point = bounds.GetCenter();
                camera.Position = point + Vector3.Up * 20; camera.Size = 8; camera.LookAt(point, Vector3.Forward);
                UnitView? picked = UnitPicking.Pick(views.Values, camera, camera.UnprojectPosition(point));
                Check(picked == view, "native ray selects each projected own/foreign/forecast member");
                inspector.Select(picked!.State); hud.UpdateInspector(views, state.Tick);
                Check(inspector.Visible && inspector.UnitId == view.State.Id, "selected displayed member remains inspectable regardless of focus or snapshot deployment");
                if (view.State.Owner != session.PlayerId)
                {
                    Check(!retire.Visible && retire.Disabled && !store.Visible && store.Disabled, "foreign soldiers and every forecast destination remain read-only");
                    retire.EmitSignal(BaseButton.SignalName.Pressed); store.EmitSignal(BaseButton.SignalName.Pressed);
                    Check(session.Commands.Count == commands, "retained foreign/forecast callbacks cannot send editing commands");
                }
                else Check(retire.Visible && !retire.Disabled, "owned projected member retains ordinary editing controls");
            }
            Check(views.Values.Any(v => v.State.Owner == 2) && views.Values.Any(v => v.State.Id < 0 && v.State.Destination == 2), "inspection exercised foreign soldiers and foreign-destined seeded forecast");
            UnitView own = views[recruited];
            UnitState sampled = own.State with { Health = own.State.Health - 1 };
            own.Synchronize(sampled); inspector.Select(own.State); hud.UpdateInspector(views, state.Tick);
            Check(Math.Abs(inspector.FindChildren("*", "ProgressBar", true, false).OfType<ProgressBar>().Single().Value / 100 - (double)sampled.Health / sampled.Profile.Health) < .00001, "inspector health follows displayed playback state rather than newest snapshot");
            hud.RefreshHud(2, -1); hud.UpdateInspector(views, state.Tick);
            Check(inspector.Visible && retire.Disabled && store.Disabled, "changing focused city retains inspection without granting editing");
            retire.EmitSignal(BaseButton.SignalName.Pressed); store.EmitSignal(BaseButton.SignalName.Pressed);
            Check(session.Commands.Count == commands, "off-focus owned callbacks remain guarded");
            hud.RefreshHud(1, -1);
            foreach (string exclusion in new[] { "hidden", "undeployed", "dead", "missing", "modal" })
            {
                own.Synchronize(sampled); own.Visible = true; views[recruited] = own; modal = false;
                inspector.Select(sampled);
                switch (exclusion)
                {
                    case "hidden": own.Visible = false; break;
                    case "undeployed": own.Synchronize(sampled with { Deployed = false }); break;
                    case "dead": own.Synchronize(sampled with { Health = 0, Hex = sampled.Hex! with { Lifecycle = UnitLifecycle.Dying } }); break;
                    case "missing": views.Remove(recruited); break;
                    case "modal": modal = true; break;
                }
                hud.UpdateInspector(views, state.Tick);
                Check(!inspector.Visible && inspector.UnitId is null, "inspection closes for " + exclusion + " member");
            }
            modal = false; own.Synchronize(sampled); own.Visible = true; views[recruited] = own;
            inspector.Select(sampled); hud.UpdateInspector(views, state.Tick);
            retire.EmitSignal(BaseButton.SignalName.Pressed);
            Check(session.Commands.Count == commands + 1 && session.Commands[^1].UnitId == recruited && session.Results[^1].Accepted
                && match.Players[1].Soldiers.All(u => u.Id != recruited), "owned projected member still retires through ordinary accepted authority");
        }
        finally { hud.Free(); RemoveChild(host); host.Free(); RemoveChild(figures); figures.Free(); }
    }
}
