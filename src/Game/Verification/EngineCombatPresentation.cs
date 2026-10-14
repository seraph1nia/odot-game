using System.Text.Json;
using Game.Core;
using Godot;

namespace Game.Verification;

public partial class EngineTests
{
    // Tier B: A can prove damage/claims, not actual imported AnimationTree,
    // socket transforms, native projectile meshes, individual health controls
    // or corpse animation/removal.
    private async Task CombatPresentation()
    {
        await GroupPresentation();
        await MeleeWindupPresentation();
        await MeleeCasualtyPresentation();
        await TerminalMeleePresentation();
        await CommittedMissPresentation();
        await StockpilePresentation();
    }
    private static int[] PresentationGroup(CombatSimulation combat, UnitType type, int count, Faction faction, int cell, int home = 0)
    {
        int[] ids = Enumerable.Range(0, count).Select(_ => combat.Create(type, faction == Faction.Adventurers ? 1 : 0, 1, 1, faction,
            assignment: faction == Faction.Adventurers ? new(-1, 0, home, 1) : null)).ToArray();
        foreach (int id in ids) combat.Seed(combat.Inspect(id) with { Location = new(UnitLifecycle.Alive, new(cell, 1)) });
        return ids;
    }
    private static JsonElement Observation(object value) => JsonSerializer.SerializeToElement(value, WireJson.Options);
    // These component-only frames project the explicit Combat.Advance clock;
    // they are not an AuthoritySession/ENet or earned World.Step witness.
    private static MatchSnapshot Received(Match match, long tick = 0)
        => SnapshotPayload.DecodeForReceiving(SnapshotPayload.Encode(match.Snapshot() with { Tick = tick }));

    private async Task GroupPresentation()
    {
        using var match = new Match(new() { MageDamage = 0, SoldierDamage = 0, Arena = new() { SquadCapacity = 8, MageSecondaryDamage = 20 } }, combatSeed: 1);
        match.Join(); CombatSimulation combat = match.Combat;
        int[] actors = PresentationGroup(combat, UnitType.Mage, 3, Faction.Adventurers, 17);
        int[] victims = PresentationGroup(combat, UnitType.Swordsman, 5, Faction.Skeletons, 18);
        CombatAction.Moving move = CombatActions.Move(combat.Inspect(victims[0]).Action, combat.Board.Transition(new(18, 1), new(19, 1)), 0, 10);
        foreach (int id in victims) combat.Seed(combat.Inspect(id) with { Action = move });
        combat.Seed(combat.Inspect(actors[0]) with { Health = HealthPoints.FromWhole(13), Statuses = StatusPolicy.Apply(StatusState.Empty, new(actors[0], StatusKind.Chill, victims[0], 1, 0, 40), new()) });
        combat.StartActions([]);
        MatchSnapshot state = Received(match);
        var layout = new CombatLayout(new(state.Arena!.Board)) { Arena = state.Arena }; layout.Synchronize(state);
        var units = state.Players[0].Soldiers.Concat(state.Enemies).ToDictionary(u => u.Id);
        var rigs = new Dictionary<int, UnitView>();
        var feedback = new VillageFeedback(); AddChild(feedback);
        var camera = new Camera3D { Current = true, Projection = Camera3D.ProjectionType.Orthogonal, Size = 65, Far = 180, Position = CombatLayout.ArenaOrigin + new Vector3(15, 25, 25) }; AddChild(camera); camera.LookAt(CombatLayout.ArenaOrigin);
        var bar = new UnitHealthBar(); AddChild(bar);
        var healthRoot = new Control(); AddChild(healthRoot);
        var memberBars = new MemberHealthBars(healthRoot);
        try
        {
            foreach (UnitState member in units.Values) { var rig = new UnitView(member); AddChild(rig); rigs.Add(member.Id, rig); rig.Sample(member, 0, 0, 0, layout, units); }
            await Frames();
            UnitView actor = rigs[actors[0]];
            string before = Observation(actor.Observe()).GetProperty("BoneRotation").GetString()!;
            actor.Sample(units[actor.State.Id], actor.State.ImpactTick, .2, 0, layout, units);
            Check(actor.Clip == UnitAssets.AttackClip(UnitType.Mage) && Observation(actor.Observe()).GetProperty("AttackBlend").GetDouble() > 0
                && Observation(actor.Observe()).GetProperty("BoneRotation").GetString() != before, "production member AnimationTree actually samples imported windup/impact skeleton, not just raw clip metadata");
            Check(actor.StatusBadge.Length == 0 && actor.State.Statuses.Chill?.Strength == 40
                && rigs.Values.All(r => r.FindChildren("*", "Label3D", true, false).Count == 0), "members retain received statuses for inspection but never create floating text");
            bar.Sample(actor, camera, GetViewport().GetVisibleRect());
            Check(bar.Current == 1300 && bar.Maximum == actor.State.Profile.Health && Math.Abs(bar.Fraction - 1300.0 / bar.Maximum) < .00001
                && Observation(bar.Observe(actor.State.Id, Transform2D.Identity)).GetProperty("InputIgnored").GetBoolean(), "native health control projects individual hundredths/max/fraction and never intercepts input");
            Check(bar.FindChildren("*", "Label", true, false).Count == 0, "member health overlay is a compact text-free bar");
            // Tier B: real native controls must pack distinct member HP without
            // overlap; pure arithmetic cannot catch Control sizes/clipping.
            memberBars.Sample(rigs, layout, 0, state.Rules.Arena.SquadCapacity, camera, GetViewport().GetVisibleRect());
            var groupBars = actors.Select(id => healthRoot.GetNode<UnitHealthBar>("MemberHealth" + id)).ToArray();
            ulong[] stableBars = healthRoot.GetChildren().Select(n => n.GetInstanceId()).ToArray();
            memberBars.Sample(rigs, layout, 0, state.Rules.Arena.SquadCapacity, camera, GetViewport().GetVisibleRect()); await Frames();
            Check(stableBars.SequenceEqual(healthRoot.GetChildren().Select(n => n.GetInstanceId())) && healthRoot.GetChildCount() == rigs.Count,
                "production member overlay owner reuses exactly one native bar per sampled rig");
            foreach (int id in actors)
            {
                UnitView member = rigs[id]; UnitHealthBar compact = healthRoot.GetNode<UnitHealthBar>("MemberHealth" + id);
                Check(compact.Current == member.State.Health && compact.Maximum == member.State.Profile.Health && compact.Size == new Vector2(16, 7),
                    "compact member slot retains independent current/max health, never a squad mean");
                Control clip = compact.GetChildren().OfType<Control>().Single(c => c.ClipContents);
                Check(compact.GetChildren().OfType<Panel>().Single().Size == compact.Size && clip.GetChild<Panel>(0).Size.Y <= compact.Size.Y - 3,
                    "actual woodland style minimum sizes fit inside the compact track without oversized fill clipping");
            }
            Check(groupBars.SelectMany((a, i) => groupBars.Skip(i + 1).Select(b => !a.GetRect().Intersects(b.GetRect()))).All(clear => clear),
                "native compact member health slots do not overlap within a grouped actor");
            actor.Visible = false; bar.Sample(actor, camera, GetViewport().GetVisibleRect()); Check(!bar.Visible, "hidden rigs cannot leave a visible bar");
            actor.Visible = true; bar.Sample(actor, camera, new Rect2(0, 0, 1, 1)); Check(!bar.Visible, "bars outside the world viewport remain hidden");
            Check(actors.Select(id => rigs[id].Position).Distinct().Count() == actors.Length, "native members occupy distinct stable formation poses on one squad source");
            CombatEvent launch = state.CombatEvents.Single(e => e.Type == CombatEventType.AttackStarted && e.Unit?.Id == actors[0]);
            Vector3 predicted = layout.Position(units[launch.TargetId], launch.Unit!.ImpactTick) + new Vector3(0, .5f, 0);
            Check(layout.EventImpact(launch)!.Value.IsEqualApprox(predicted), "projectile launch aims at committed range-arrival pose and the selected member formation, not stale source/group center");
            Vector3 source = actor.StrikeOrigin;
            feedback.Combat(launch, 0, 0, false, source, layout.EventImpact(launch));
            MeshInstance3D projectile = feedback.GetChildren().OfType<MeshInstance3D>().Single(n => n.Visible);
            Check(projectile.Position.IsEqualApprox(source), "actual native projectile starts at sampled imported strike socket");
            double duration = (launch.Unit.ImpactTick - launch.Tick) / (double)Match.StepsPerSecond;
            feedback.Sample(duration / 2, false);
            Check(projectile.Position.IsEqualApprox(source.Lerp(predicted, .5f) + Vector3.Up * .4f), "actual native projectile mesh samples committed source/aim and common-clock flight");
            combat.Advance(launch.Unit.ImpactTick, []); state = Received(match, launch.Unit.ImpactTick); layout.Synchronize(state);
            var after = state.Players[0].Soldiers.Concat(state.Enemies).Concat(state.DyingBodies).ToDictionary(u => u.Id);
            CombatEvent[] impacts = state.CombatEvents.Where(e => e.Type == CombatEventType.Impact && e.Unit?.Faction == Faction.Adventurers).ToArray();
            Check(impacts.Length == actors.Length && impacts.All(e => e.Landed && e.Victims.Length == victims.Length - 1 && !e.Victims.Contains(e.TargetId)), "received zero-X/uncapped Y-only impacts retain actual primary and every secondary victim without cross-group substitution");
            foreach (CombatEvent impact in impacts)
            {
                Vector3 point = layout.Position(after[impact.TargetId], impact.Tick) + new Vector3(0, .5f, 0);
                Check(layout.EventImpact(impact)!.Value.IsEqualApprox(point), "native landed cue uses actual numerical primary identity/formation/arrival pose");
            }
            UnitState casualty = state.DyingBodies.First(); UnitView dead = rigs[casualty.Id];
            dead.Sample(casualty, casualty.Hex!.DeathStartTick, duration, 0, layout, after);
            Vector3 frozen = dead.Position; double firstPose = dead.PoseSeconds;
            string firstBone = Observation(dead.Observe()).GetProperty("BoneRotation").GetString()!;
            double midpoint = (casualty.Hex.DeathStartTick + casualty.Hex.DeathEndTick) / 2.0;
            dead.Sample(casualty, midpoint, duration + .2, 0, layout, after); await Frames();
            Console.WriteLine($"MEASURE native casualty: id={casualty.Id}; frozen={frozen}; sampled={dead.Position}; firstPose={firstPose}; midpointPose={dead.PoseSeconds}; start={casualty.Hex.DeathStartTick}; end={casualty.Hex.DeathEndTick}; playback={midpoint}");
            Check(dead.Dead && dead.Clip == "death" && dead.Position.IsEqualApprox(frozen) && dead.PoseSeconds > firstPose
                && Observation(dead.Observe()).GetProperty("BoneRotation").GetString() != firstBone, "casualty keeps exact frozen physical pose while actual imported death animation advances on common playback clock");
            bar.Sample(dead, camera, GetViewport().GetVisibleRect());
            Check(!bar.Visible && dead.StatusBadge.Length == 0 && !dead.Expired(casualty.Hex.DeathEndTick - .01) && dead.Expired(casualty.Hex.DeathEndTick), "casualty hides individual health/status and expires exactly without a cosmetic occupancy claim");
            combat.Advance(casualty.Hex.DeathEndTick, []);
            Check(!combat.Dying().Any(u => u.Id == casualty.Id), "production logical corpse cleanup completes independently of native cosmetics");
            ulong instance = dead.GetInstanceId(); dead.QueueFree(); rigs.Remove(casualty.Id); await Frames();
            Check(!GodotObject.IsInstanceIdValid(instance), "native casualty removal actually releases its imported rig node");
            ulong deadBar = healthRoot.GetNode<UnitHealthBar>("MemberHealth" + casualty.Id).GetInstanceId();
            memberBars.Sample(rigs, layout, casualty.Hex.DeathEndTick, state.Rules.Arena.SquadCapacity, camera, GetViewport().GetVisibleRect()); await Frames();
            Check(!GodotObject.IsInstanceIdValid(deadBar) && healthRoot.GetChildCount() == rigs.Count,
                "production overlay owner detaches and frees a removed member's bar without duplicate or lingering controls");
            memberBars.Clear(); await Frames(); Check(healthRoot.GetChildCount() == 0, "playback generation/session reset clears all member bars");
            Check(match.Players[1].Soldiers.Single(u => u.Id == actors[0]).Health == 1300, "native resource/pose/health sampling never mutates authoritative individual HP");
        }
        finally
        {
            foreach (UnitView rig in rigs.Values) { RemoveChild(rig); rig.Free(); }
            memberBars.Clear(); RemoveChild(healthRoot); healthRoot.Free();
            RemoveChild(bar); bar.Free(); RemoveChild(camera); camera.Free(); RemoveChild(feedback); feedback.Free();
        }
    }
    private async Task StockpilePresentation()
    {
        // Same production resource projection, but never parent/initialize the
        // Tabletop controller: no landscape/world/full renderer or application.
        using var match = new Match(); match.Join();
        Check(match.Apply(1, Command.FromSnapshot(match.Snapshot(), 1, "start", 1)).Accepted, "numerical stockpile receiver starts");
        var session = new ReceivingSession(match); var tabletop = new Tabletop(session);
        var board = new Node3D(); AddChild(board);
        try
        {
            tabletop.UpdateStockpiles(board, session.State.Players[0]);
            Node3D initial = board.GetNode<Node3D>("Stockpiles"); ulong id = initial.GetInstanceId();
            Check(initial.GetNode("Wood").GetChildCount() == PresentationLimits.StockpileCount(6)
                && initial.GetNode("Gold").GetChildCount() == PresentationLimits.StockpileCount(12)
                && initial.FindChildren("*", "MeshInstance3D", true, false).Count > 0, "real immutable cargo/wood resources instantiate from received city stock");
            session.SendAction("build", 0, Building.Farm);
            Check(session.Results[^1].Accepted, "stockpile change pays genuine production construction quote");
            tabletop.UpdateStockpiles(board, session.State.Players[0]);
            Check(board.GetNode("Stockpiles").GetInstanceId() == id, "native resources never optimistically spend before accepted receiving snapshot");
            session.Acknowledge(); tabletop.UpdateStockpiles(board, session.State.Players[0]);
            Check(session.State.Players[0].Wood == 4 && board.GetNode("Stockpiles").GetInstanceId() == id,
                "exact received spend within one coarse stockpile band retains native props; exact amounts belong to live HUD");
            foreach (int plot in new[] { 1, 2 })
            {
                session.SendAction("build", plot, Building.Farm);
                Check(session.Results[^1].Accepted, "ordinary paid construction reaches the real zero-stock threshold");
                tabletop.UpdateStockpiles(board, session.State.Players[0]);
                Check(board.GetNode("Stockpiles").GetInstanceId() == id, "unacknowledged threshold crossing cannot optimistically rebuild props");
                session.Acknowledge(); tabletop.UpdateStockpiles(board, session.State.Players[0]);
            }
            await Frames(); Node3D current = board.GetNode<Node3D>("Stockpiles");
            Check(session.State.Players[0].Wood == 0 && current.GetNode("Wood").GetChildCount() == 0
                && current.GetNode("Gold").GetChildCount() == PresentationLimits.StockpileCount(session.State.Players[0].Gold)
                && current.GetInstanceId() != id && !GodotObject.IsInstanceIdValid(id), "accepted compressed receiving updates native stock props at real band crossing and actually frees obsolete projection");
            ulong stable = current.GetInstanceId(); tabletop.UpdateStockpiles(board, session.State.Players[0]);
            Check(board.GetNode("Stockpiles").GetInstanceId() == stable && !tabletop.IsInsideTree() && tabletop.GetChildCount() == 0, "unchanged received resources retain nodes without starting a full world");
        }
        finally { tabletop.Free(); RemoveChild(board); board.Free(); }
    }
    private async Task MeleeWindupPresentation()
    {
        using var match = new Match(new() { SoldierDamage = 0 }, combatSeed: 1);
        match.Join(); CombatSimulation combat = match.Combat;
        PresentationGroup(combat, UnitType.Swordsman, 2, Faction.Adventurers, 17);
        PresentationGroup(combat, UnitType.Swordsman, 2, Faction.Skeletons, 18);
        combat.StartActions([]); MatchSnapshot initial = Received(match);
        var layout = new CombatLayout(new(initial.Arena!.Board)) { Arena = initial.Arena }; layout.Synchronize(initial);
        var rigs = new Dictionary<int, UnitView>();
        var strikes = new Dictionary<int, MeleeStrike>();
        var camera = new Camera3D { Position = CombatLayout.ArenaOrigin + new Vector3(15, 25, 25) }; AddChild(camera); camera.LookAt(CombatLayout.ArenaOrigin);
        try
        {
            var units = initial.Players[0].Soldiers.Concat(initial.Enemies).ToDictionary(u => u.Id);
            foreach (UnitState unit in units.Values)
            {
                var rig = new UnitView(unit); AddChild(rig); rigs.Add(unit.Id, rig);
                var strike = new MeleeStrike(); AddChild(strike); strikes.Add(unit.Id, strike);
                rig.Sample(unit, 0, 0, 0, layout, units);
            }
            var bones = rigs.ToDictionary(r => r.Key, r => Observation(r.Value.Observe()).GetProperty("BoneRotation").GetString());
            foreach (long tick in new long[] { 6, 12 })
            {
                combat.Advance(tick, []); MatchSnapshot state = Received(match, tick); layout.Synchronize(state);
                units = state.Players[0].Soldiers.Concat(state.Enemies).ToDictionary(u => u.Id);
                foreach (UnitState unit in units.Values) rigs[unit.Id].Sample(unit, tick, tick / 60.0, 0, layout, units);
                await Frames();
                foreach (UnitState unit in units.Values)
                {
                    strikes[unit.Id].Sample(rigs[unit.Id], rigs[unit.TargetId], tick, 0, layout);
                    Check(Observation(rigs[unit.Id].Observe()).GetProperty("BoneRotation").GetString() != bones[unit.Id], "native melee proof samples changing imported weapon-socket bones");
                }
                Main.Emit(new GameEvent(tick == 6 ? "melee-windup" : "melee-impact", State: state,
                    Message: JsonSerializer.Serialize(new
                    {
                        Units = rigs.Values.Select(r => r.Observe()).ToArray(),
                        Strikes = strikes.Values.Select(s => s.Observe(camera, Transform2D.Identity)).ToArray(),
                        CombatTick = tick
                    }, WireJson.Options)));
            }
        }
        finally
        {
            foreach (MeleeStrike strike in strikes.Values) { RemoveChild(strike); strike.Free(); }
            foreach (UnitView rig in rigs.Values) { RemoveChild(rig); rig.Free(); }
            RemoveChild(camera); camera.Free();
        }
    }
    // Tier B: the earlier casualty check sampled only already-dead rigs. This
    // reproduces a real simultaneous impact killing a lunging participant.
    private async Task MeleeCasualtyPresentation()
    {
        using var match = new Match(combatSeed: 1); match.Join();
        CombatSimulation combat = match.Combat;
        int victimId = PresentationGroup(combat, UnitType.Swordsman, 1, Faction.Adventurers, 17)[0];
        PresentationGroup(combat, UnitType.Swordsman, 1, Faction.Skeletons, 18);
        combat.Seed(combat.Inspect(victimId) with { Health = HealthPoints.FromWhole(1) });
        combat.StartActions([]); MatchSnapshot initial = Received(match);
        var layout = new CombatLayout(new(initial.Arena!.Board)) { Arena = initial.Arena }; layout.Synchronize(initial);
        var units = initial.Players[0].Soldiers.Concat(initial.Enemies).ToDictionary(u => u.Id);
        UnitState live = units[victimId];
        long impact = live.ImpactTick;
        var rig = new UnitView(live); AddChild(rig);
        try
        {
            rig.Sample(live, impact, impact / (double)Match.StepsPerSecond, 0, layout, units);
            await Frames(); Vector3 before = rig.Position;
            Check(Math.Abs(before.DistanceTo(layout.Position(live, impact)) - .65f) < .00001f,
                "actual live melee rig reaches its attack lunge before lethal impact");
            combat.Advance(impact, []); MatchSnapshot killed = Received(match, impact); layout.Synchronize(killed);
            units = killed.Players[0].Soldiers.Concat(killed.Enemies).Concat(killed.DyingBodies).ToDictionary(u => u.Id);
            UnitState casualty = killed.DyingBodies.Single(u => u.Id == victimId);
            rig.Sample(casualty, impact, impact / (double)Match.StepsPerSecond, 0, layout, units);
            Check(rig.Position.IsEqualApprox(before) && rig.Dead && rig.Clip == "death",
                "attacking casualty freezes its actual lunged root without a live-to-dead backward jump");
            double firstPose = rig.PoseSeconds;
            string firstBone = Observation(rig.Observe()).GetProperty("BoneRotation").GetString()!;
            double midpoint = (casualty.Hex!.DeathStartTick + casualty.Hex.DeathEndTick) / 2.0;
            rig.Sample(casualty, midpoint, midpoint / Match.StepsPerSecond, 0, layout, units); await Frames();
            Check(rig.Position.IsEqualApprox(before) && rig.PoseSeconds > firstPose
                && Observation(rig.Observe()).GetProperty("BoneRotation").GetString() != firstBone,
                "lunged casualty remains physically frozen while imported death clip advances");
            Check(!killed.Reservations.Positions.Any(p => p.UnitId == casualty.SquadId)
                && !killed.Reservations.Transit.Any(p => p.UnitId == casualty.SquadId),
                "frozen cosmetic lunge retains no logical squad claim");
        }
        finally { RemoveChild(rig); rig.Free(); }
    }
    private async Task TerminalMeleePresentation()
    {
        using var match = new Match(new()
        {
            StartingGold = 100,
            StartingWood = 100,
            SoldierDamage = 0,
            DefenderDamage = 0,
            Combat = new() { MaximumWaveTicks = 10 }
        }, combatSeed: 1);
        match.Join(); match.Players[1].Metal = 100; match.Players[1].Food = 100;
        long sequence = 0;
        void Act(string action, int slot = -1, Building building = Building.Empty) => Check(match.Apply(1,
            Command.FromSnapshot(match.Snapshot(), ++sequence, action, 1, slot, building)).Accepted, "native terminal fixture accepts " + action);
        Act("start"); Act("build", 0, Building.Barracks); Act("recruit", 0);
        for (int gate = 0; gate < 3; gate++) Act("ready");
        CombatSimulation combat = match.Combat;
        int actor = match.Players[1].Soldiers.Single().Id;
        int primary = combat.Enemies()[0].Id;
        foreach (UnitState extra in combat.Enemies().Where(u => u.Id != primary)) combat.Remove(extra.Id);
        HexTransition move = combat.Board.Transition(new(18, 1), new(11, 1));
        combat.Seed(combat.Inspect(actor) with { Location = new(UnitLifecycle.Alive, new(17, 1)), Action = new CombatAction.Waiting() });
        combat.Seed(combat.Inspect(primary) with
        {
            Location = new(UnitLifecycle.Alive, move.Source),
            Action = new CombatAction.Moving(1, move.Destination, move.Id, 0, 30)
        });
        combat.StartActions(match.Players.Values);
        while (match.Tick < 9) match.Step();
        MatchSnapshot before = SnapshotPayload.DecodeForReceiving(SnapshotPayload.Encode(match.Snapshot()));
        var layout = new CombatLayout(new(before.Arena!.Board)); layout.Synchronize(before);
        var units = before.Players[0].Soldiers.Concat(before.Enemies).ToDictionary(u => u.Id);
        UnitState live = units[actor];
        var rig = new UnitView(live); AddChild(rig);
        try
        {
            Check(live.PendingImpact && live.TargetId == primary && live.ImpactTick > 10 && units[primary].Hex!.HoldsTransit,
                "native terminal fixture has a genuine pending melee against an in-flight selected primary");
            rig.Sample(live, 10, 10.0 / Match.StepsPerSecond, 0, layout, units); await Frames();
            Vector3 root = rig.Position; float facing = layout.Facing(live, 10, units); float rotation = rig.Rotation.Y; double pose = rig.PoseSeconds;
            Check(Math.Abs(root.DistanceTo(layout.Position(live, 10)) - .65f) < .00001f,
                "pre-stop imported melee root retains its real lunge toward the primary's stop-time transit pose");
            match.Step();
            MatchSnapshot stopped = SnapshotPayload.DecodeForReceiving(SnapshotPayload.Encode(match.Snapshot())); layout.Synchronize(stopped);
            units = stopped.Players[0].Soldiers.Concat(stopped.Enemies).ToDictionary(u => u.Id);
            UnitState frozen = units[actor];
            Check(stopped.Tick == 10 && stopped.Phase == Phase.Defeat && stopped.Stall?.Limit == BattleLimit.WaveDuration,
                "ordinary World.Step reaches the configured wave-duration terminal boundary before impact");
            rig.Sample(frozen, 10, 10.0 / Match.StepsPerSecond, 0, layout, units); await Frames();
            Console.WriteLine($"MEASURE native terminal melee: before={root}; after={rig.Position}; beforeFacing={facing}; afterFacing={layout.Facing(frozen, 10, units)}; aim={frozen.Hex!.FrozenAim}");
            Check(rig.Position.IsEqualApprox(root) && Mathf.IsEqualApprox(layout.Facing(frozen, 10, units), facing)
                && Mathf.IsEqualApprox(rig.Rotation.Y, rotation) && rig.PoseSeconds == pose && rig.Clip == UnitAssets.AttackClip(UnitType.Swordsman),
                "terminal freeze preserves the imported melee root, lunge, aim, facing and mid-windup clip at the same tick");
            Check(frozen.Hex!.FrozenAim == new HexPosePoint(move.Source, move.Destination, move.Id, 10, 30) && !frozen.PendingImpact,
                "received terminal aim is the exact selected-primary pose at ten, without another committed impact");
            combat.Seed(combat.Inspect(primary) with { Location = new(UnitLifecycle.Alive, new(25, 1)), Action = new CombatAction.Waiting() });
            MatchSnapshot later = SnapshotPayload.DecodeForReceiving(SnapshotPayload.Encode(match.Snapshot())); layout.Synchronize(later);
            units = later.Players[0].Soldiers.Concat(later.Enemies).ToDictionary(u => u.Id);
            rig.Sample(units[actor], 20, 20.0 / Match.StepsPerSecond, 0, layout, units); await Frames();
            Check(rig.Position.IsEqualApprox(root) && Mathf.IsEqualApprox(layout.Facing(units[actor], 20, units), facing)
                && rig.PoseSeconds == pose && units[actor].Hex == frozen.Hex && units[actor].TargetId == primary,
                "stopped imported root and immutable primary observation do not follow subsequent live-target movement");
        }
        finally { RemoveChild(rig); rig.Free(); }
    }
    private async Task CommittedMissPresentation()
    {
        using var match = new Match(new() { MeleeWindupTicks = 24, SoldierDamage = 15, RangedDamage = 20 }, combatSeed: 1);
        match.Join(); CombatSimulation combat = match.Combat;
        int actorId = PresentationGroup(combat, UnitType.Swordsman, 1, Faction.Adventurers, 17)[0];
        PresentationGroup(combat, UnitType.Crossbowman, 1, Faction.Adventurers, 10, 1);
        int target = PresentationGroup(combat, UnitType.Swordsman, 1, Faction.Skeletons, 18)[0];
        combat.Seed(combat.Inspect(target) with { Health = HealthPoints.FromWhole(10) });
        combat.StartActions([]); MatchSnapshot initial = Received(match);
        var layout = new CombatLayout(new(initial.Arena!.Board)) { Arena = initial.Arena }; layout.Synchronize(initial);
        var rig = new UnitView(initial.Players[0].Soldiers.Single(u => u.Id == actorId)); AddChild(rig);
        var strike = new MeleeStrike(); AddChild(strike);
        var camera = new Camera3D(); AddChild(camera);
        try
        {
            combat.Advance(18, []); MatchSnapshot lost = Received(match, 18); layout.Synchronize(lost);
            var units = lost.Players[0].Soldiers.Concat(lost.DyingBodies).ToDictionary(u => u.Id);
            UnitState member = units[actorId];
            Check(lost.DyingBodies.Any(u => u.Id == target) && member.PendingImpact && member.ImpactTick == 24, "another real squad kills the target without resetting committed member cycle");
            rig.Sample(member, 20, .3, 0, layout, units);
            Check(rig.Clip == UnitAssets.AttackClip(UnitType.Swordsman) && Observation(rig.Observe()).GetProperty("AttackActive").GetBoolean(), "native committed windup remains an attack after losing its last target");
            combat.Advance(24, []); MatchSnapshot miss = Received(match, 24); layout.Synchronize(miss);
            units = miss.Players[0].Soldiers.Concat(miss.DyingBodies).ToDictionary(u => u.Id); member = units[actorId];
            rig.Sample(member, 24, .4, 0, layout, units); strike.Sample(rig, null, 24, 0, layout);
            JsonElement cue = Observation(strike.Observe(camera, Transform2D.Identity));
            Check(member.AttackLanded == false && cue.GetProperty("Phase").GetString() == "miss" && !cue.GetProperty("ImpactVisible").GetBoolean(), "actual native committed miss finishes with no invented landed hit or retarget");
            Check(miss.CombatEvents.Any(e => e.Unit?.Id == actorId && e.Type == CombatEventType.Impact && !e.Landed), "native miss witness came from real numerical impact, not a forged cue");
            rig.Sample(member, member.ReadyTick, 1, 0, layout, units);
            Check(rig.Clip == "idle", "native recovery ends at unchanged authoritative ready deadline");
            await Frames();
        }
        finally { RemoveChild(camera); camera.Free(); RemoveChild(strike); strike.Free(); RemoveChild(rig); rig.Free(); }
    }
}
