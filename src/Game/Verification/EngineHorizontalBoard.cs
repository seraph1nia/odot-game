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
        }
        finally
        {
            RemoveChild(figures); figures.Free(); RemoveChild(districtRoot); districtRoot.Free();
            WoodlandMotion.ReleaseDerivedMaterials();
        }
    }
}
