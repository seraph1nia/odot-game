using Game.Core;
using Godot;

namespace Game.Verification;

public partial class EngineTests
{
    // Tier B: catches native Camera3D projection/resize regressions missed by
    // arithmetic-only tests. No world/application/GPU; five owned aspect ratios,
    // extreme pointer intents and real resource construction, estimated <1 s.
    private async Task CameraBoundaries()
    {
        var camera = new Camera3D { Projection = Camera3D.ProjectionType.Orthogonal, Current = true, Far = 180 }; AddChild(camera);
        var navigation = new TabletopCamera(camera);
        var perimeter = new BattlefieldPerimeter(); AddChild(perimeter);
        Aabb content = BattlefieldFrame.Content(null, []);
        var board = HexBoardDefinition.Default();
        Check(board.Cells.All(cell => board.Anchors.All(anchor => content.HasPoint(VillageLayout.Hex(cell.Coordinate.Column, cell.Coordinate.R) + new Vector3(anchor.AnchorX / 1000f, 0, anchor.AnchorForward / 1000f)))),
            "frame retains every army/enemy formation anchor");
        Check(Enumerable.Range(0, 9).All(slot => content.HasPoint(VillageLayout.Slot(slot))), "frame retains all nine buildable plots");
        try
        {
            foreach (Vector2I dimensions in new[] { new Vector2I(1100, 820), new(1280, 720), new(1920, 1080), new(2560, 1080), new(820, 1100) })
            {
                GetWindow().Size = dimensions;
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                Rect2 viewport = GetViewport().GetVisibleRect();
                float hud = viewport.Size.Y * .32f;
                // Smallest counterfactual: same native fitter/aspect/HUD, only the
                // former padded box differs. This is not a pixel/GPU witness.
                BattlefieldFrame.FitCamera(camera, new(new Vector3(-10.5f, -1.5f, -15), new Vector3(22.5f, 6.7f, 30)), Vector3.Zero, viewport.Size, hud);
                float formerSize = camera.Size;
                BattlefieldFrame.FitCamera(camera, content, Vector3.Zero, viewport.Size, hud);
                Check(camera.Size < formerSize, "content framing is genuinely tighter than former padded framing");
                navigation.Fit(BattlefieldFrame.GroundRect(content), viewport);
                Check(navigation.Boundary.Grow(.01f).Encloses(BattlefieldFrame.Footprint(camera, viewport)), "resize remains confined while preserving prior pan/zoom");
                navigation.Reset();
                foreach (int i in Enumerable.Range(0, 8))
                {
                    Vector2 pixel = camera.UnprojectPosition(content.GetEndpoint(i));
                    Check(new Rect2(Vector2.Zero, new(viewport.Size.X - 240, viewport.Size.Y - hud)).Grow(1).HasPoint(pixel), $"default framing retains full playable silhouette at {dimensions}: corner={i}, pixel={pixel}, viewport={viewport}, size={camera.Size}");
                }
                void Confined(string input)
                {
                    Check(navigation.Boundary.Grow(.01f).Encloses(BattlefieldFrame.Footprint(camera, viewport)), input + " cannot expose past the opaque boundary at " + dimensions);
                    Vector3 aim = navigation.ViewCenter!.Value;
                    Check(BattlefieldFrame.GroundRect(content).Grow(BattlefieldFrame.Margin + .01f).HasPoint(new(aim.X, aim.Z)), input + " cannot pan the view center beyond the cloud bank");
                }
                Confined("resize/fit");
                Console.WriteLine($"MEASURE camera {dimensions}: oldSize={formerSize:F3}; size={camera.Size:F3}; content={content}; boundary={navigation.Boundary}");
                foreach (Vector2 cursor in new[] { viewport.Position, viewport.End, viewport.GetCenter() })
                {
                    navigation.ZoomAt(cursor, true, 40); Confined("zoom in");
                    foreach (Vector3 anchor in new[] { new Vector3(-10000, 0, -10000), new(10000, 0, 10000) })
                    { navigation.Drag(anchor, cursor); Confined("extreme drag"); }
                    navigation.ZoomAt(cursor, false, 100); Confined("zoom out");
                    Check(navigation.Zoom == 1, "zoom-out cannot exceed fitted overview");
                }
                navigation.Reset(); Confined("reset");
                foreach (Key code in new[] { Key.A, Key.D, Key.W, Key.S })
                {
                    using var key = new InputEventKey { Keycode = code, PhysicalKeycode = code, Pressed = true };
                    Input.ParseInputEvent(key);
                    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                    Console.WriteLine($"MEASURE held-key input: key={code}; logical={Input.IsKeyPressed(code)}; physical={Input.IsPhysicalKeyPressed(code)}; display={DisplayServer.GetName()}");
                    navigation.ObserveKey(key);
                    Check(navigation.Press(key), "native camera action accepts physical key " + code);
                    double beforeMovement = navigation.MovementSeconds;
                    Check(Input.IsPhysicalKeyPressed(code), "complete native synthetic key supplies physical polling state");
                    navigation.Move(100000, true, .68f);
                    Confined("held-action pan");
                    Check(navigation.MovementSeconds > beforeMovement && navigation.MovementDirection != Vector2.Zero, "accepted native key and polling state actually exercise production pan");
                    key.Pressed = false; Input.ParseInputEvent(key); navigation.ObserveKey(key);
                }
                perimeter.Cover(BattlefieldFrame.GroundRect(content).Grow(BattlefieldFrame.Margin), navigation.Boundary);
                Check(perimeter.Material.Transparency == BaseMaterial3D.TransparencyEnum.Disabled && perimeter.Material.AlbedoColor.A == 1,
                    "cloud backstop is opaque independently of renderer/fog settings");
                Check(perimeter.GetChildren().OfType<MeshInstance3D>().Count(n => n.Mesh is BoxMesh) == 4,
                    "four continuous exterior backstops exist alongside cloud lobes");
                navigation.ZoomAt(viewport.GetCenter(), true, 40);
                navigation.Drag(new(10000, 0, 10000), viewport.GetCenter()); Confined("pre-resize pan/zoom");
            }
            // One passive landscape component, not Tabletop/world/session. Check
            // emitted imported native terrain and bounded exterior accents.
            var owner = new Node3D(); AddChild(owner);
            try
            {
                var landscape = new VillageLandscape(owner, new LandscapeAssets());
                var magic = new ForestMagic(); owner.AddChild(magic); magic.View(landscape);
                Vector3 formerAccent = magic.Lights[3].Position;
                landscape.CoverBattlefield(BattlefieldFrame.GroundRect(content), navigation.Boundary);
                using var report = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(landscape.Observe(camera, GetViewport().GetVisibleRect())));
                int tiles = report.RootElement.GetProperty("Tiles").GetInt32();
                Check(tiles is > 0 and <= 300 && report.RootElement.GetProperty("FloorCells").GetInt32() == tiles, "compact terrain emits one real imported floor per bounded cell");
                var nativeCenters = report.RootElement.GetProperty("CoreTerrain").EnumerateArray().Select(entry => entry.GetString()!.Split(':'))
                    .Select(parts => new Vector2(float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture), float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture))).ToArray();
                using var probeMesh = new BoxMesh();
                using var readback = new MultiMesh { TransformFormat = MultiMesh.TransformFormatEnum.Transform3D, Mesh = probeMesh, InstanceCount = 1 };
                Vector3 expected = new(7, 0, 11); readback.SetInstanceTransform(0, new Transform3D(Basis.Identity, expected));
                Vector3 actual = readback.GetInstanceTransform(0).Origin;
                Console.WriteLine($"MEASURE native terrain readback: standalone expected={expected}; actual={actual}; distinctReportedCenters={nativeCenters.Distinct().Count()}; devicePresent={RenderingServer.GetRenderingDevice() is not null}");
                if (actual == expected)
                    Check(board.Cells.Select(cell => VillageLayout.Hex(cell.Coordinate.Column, cell.Coordinate.R)).Concat(Enumerable.Range(0, 9).Select(VillageLayout.Slot))
                        .All(point => nativeCenters.Any(center => center.DistanceTo(new(point.X, point.Z)) < .01f)), "native emitted floors retain all combat cells and buildable plot centers");
                else
                    Check(actual == Vector3.Zero && RenderingServer.GetRenderingDevice() is null,
                        "Dummy has no MultiMesh transform readback: coordinate retention is bounded-helper proof only, not native buffer proof");
                Rect2 inner = BattlefieldFrame.GroundRect(content).Grow(BattlefieldFrame.Margin);
                Check(landscape.MagicSites.Count == 4 && landscape.MagicSites.All(site => inner.Grow(-2.99f).HasPoint(new(site.Ground.X, site.Ground.Z))),
                    "all four magical accents stay inside compact terrain instead of showing above exterior fog");
                magic.View(landscape);
                Check(magic.Lights[3].Position != formerAccent && Enumerable.Range(0, 4).All(i => magic.Lights[i].Position.DistanceTo(landscape.MagicSites[i].Ground + new Vector3(0, i == 1 ? 1.2f : .7f, 0)) < .001f),
                    "existing live accent rig refreshes moved sites on the same compact landscape");
                Check(landscape.GetNode<BattlefieldPerimeter>("OpaqueCloudPerimeter").Material.Transparency == BaseMaterial3D.TransparencyEnum.Disabled,
                    "real passive landscape owns the renderer-independent opaque perimeter");
                Console.WriteLine($"MEASURE compact landscape: tiles={tiles}; floors={report.RootElement.GetProperty("FloorCells")}; accents={landscape.MagicSites.Count}");
            }
            finally { RemoveChild(owner); owner.Free(); WoodlandMotion.ReleaseDerivedMaterials(); }
            // Release managed wrappers while the owned RenderingServer is still
            // alive, rather than deferring their finalizers until process teardown.
            GC.Collect(); GC.WaitForPendingFinalizers();
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
        finally { RemoveChild(camera); camera.Free(); RemoveChild(perimeter); perimeter.Free(); GetWindow().Size = new(1100, 820); }
    }
}
