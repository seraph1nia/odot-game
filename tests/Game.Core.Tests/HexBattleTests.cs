using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

public sealed class HexBattleTests(ITestOutputHelper output)
{
    private static int At(CombatSimulation combat, UnitType type, Faction faction, int cell, int anchor, bool wait = false)
    {
        int id = combat.Create(type, faction == Faction.Adventurers ? 1 : 0, 1, 1, faction);
        UnitState unit = combat.Read(id);
        HexUnitState hex = CombatFixture.At(unit, cell, anchor);
        combat.Seed(unit with { Hex = wait ? hex with { Action = UnitActionKind.Recovery, EndTick = 500 } : hex, Deployed = true, ReadyTick = wait ? 500 : 0 });
        return id;
    }
    [Fact]
    public void TargetDistanceThenInitiativeAndNewBoundarySelectionUseCurrentState()
    {
        using var combat = new CombatSimulation(new(), seed: 123);
        int actor = At(combat, UnitType.Crossbowman, Faction.Adventurers, 17, 1);
        int nearer = At(combat, UnitType.Mage, Faction.Skeletons, 14, 1, wait: true);
        int lower = At(combat, UnitType.Swordsman, Faction.Skeletons, 11, 1, wait: true);
        combat.Step(1, []); Assert.Equal(nearer, combat.Read(actor).TargetId);
        UnitState enemy = combat.Read(lower);
        combat.Seed(enemy with { Hex = enemy.Hex! with { Position = new(16, 1) } });
        for (int tick = 2; tick <= 60; tick++) combat.Step(tick, []);
        Assert.Equal(nearer, combat.Read(actor).TargetId);
        combat.Step(61, []); Assert.Equal(lower, combat.Read(actor).TargetId);
        Assert.Equal(2, combat.Read(actor).AttackSequence);
    }
    [Theory]
    [InlineData(129, true)]
    [InlineData(130, false)]
    public void ImpactSeesOnlySourceBeforeArrivalAndDestinationOnArrival(int impact, bool landed)
    {
        using var combat = new CombatSimulation(new());
        int mover = At(combat, UnitType.Crossbowman, Faction.Adventurers, 11, 1);
        UnitState unit = combat.Read(mover); HexPosition to = new(8, 1);
        combat.Seed(unit with
        {
            Hex = unit.Hex! with
            {
                Action = UnitActionKind.Moving,
                Destination = to,
                Transition = combat.Board.Transition(unit.Hex.Position, to).Id,
                StartTick = 100,
                EndTick = 130,
                ActionSequence = 1
            }
        });
        int attacker = At(combat, UnitType.Swordsman, Faction.Skeletons, 14, 1);
        UnitState strike = combat.Read(attacker);
        combat.Seed(strike with
        {
            Hex = strike.Hex! with { Action = UnitActionKind.Windup, StartTick = impact - 12, EndTick = impact + 48, ActionSequence = 1 },
            TargetId = mover,
            AttackSequence = 1,
            PendingImpact = true,
            ActionStartTick = impact - 12,
            ImpactTick = impact,
            ReadyTick = impact + 48
        });
        combat.Advance(impact, []);
        CombatEvent hit = Assert.Single(combat.Events(), e => e.Type == CombatEventType.Impact);
        Assert.Equal(landed, hit.Landed); Assert.Equal(landed ? 2000 : 3000, combat.Read(mover).Health);
        Assert.Equal(landed ? 11 : 8, combat.Read(mover).Hex!.Position.Cell);
        Assert.Equal(landed ? 3 : 2, combat.Reservations.Positions.Length);
    }
    [Fact]
    public void DeathFacingCapturesTheTargetPoseWithoutRetainingItsLiveState()
    {
        using var combat = new CombatSimulation(new());
        int actor = At(combat, UnitType.Swordsman, Faction.Adventurers, 17, 1);
        int target = At(combat, UnitType.Crossbowman, Faction.Skeletons, 14, 1);
        combat.Seed(combat.Read(actor) with { Health = 100 });
        for (int tick = 1; tick <= 19; tick++) combat.Step(tick, []);
        UnitState corpse = Assert.Single(combat.Dying()); Assert.Equal(actor, corpse.Id);
        HexPosePoint aim = Assert.IsType<HexPosePoint>(corpse.Hex!.FrozenAim);
        Assert.Equal(new HexPosition(14, 1), aim.Position); Assert.Equal(0, aim.Transition);
        UnitState opponent = combat.Read(target);
        combat.Seed(opponent with { Hex = opponent.Hex! with { Position = new(11, 1) } });
        combat.Step(20, []);
        Assert.Equal(aim, Assert.Single(combat.Dying()).Hex!.FrozenAim);
        Assert.Equal(67, corpse.Hex.DeathEndTick);
    }
    [Fact]
    public void LethalImpactAtRecoveryEndPreventsANewAttackOnThatTick()
    {
        using var combat = new CombatSimulation(new());
        int actor = At(combat, UnitType.Swordsman, Faction.Adventurers, 17, 1);
        int enemy = At(combat, UnitType.Swordsman, Faction.Skeletons, 14, 1);
        UnitState recovering = combat.Read(actor), striking = combat.Read(enemy);
        combat.Seed(recovering with
        { Health = 100, ReadyTick = 61, AttackSequence = 1, TargetId = enemy, Hex = recovering.Hex! with { Action = UnitActionKind.Recovery, ActionSequence = 1, StartTick = 1, EndTick = 61 } });
        combat.Seed(striking with
        {
            TargetId = actor,
            PendingImpact = true,
            AttackSequence = 1,
            ActionStartTick = 49,
            ImpactTick = 61,
            ReadyTick = 109,
            Hex = striking.Hex! with { Action = UnitActionKind.Windup, ActionSequence = 1, StartTick = 49, EndTick = 109 }
        });
        combat.Step(61, []);
        Assert.Equal(actor, Assert.Single(combat.Dying()).Id);
        Assert.DoesNotContain(combat.Events(), e => e.Type == CombatEventType.AttackStarted && e.Unit?.Id == actor);
        Assert.Equal(1, Assert.Single(combat.Dying()).AttackSequence);
    }
    [Fact]
    public void FeasibleApproachBeatsACloserButFullyBlockedOpponent()
    {
        using var combat = new CombatSimulation(new());
        int actor = At(combat, UnitType.Swordsman, Faction.Adventurers, 16, 1);
        int near = At(combat, UnitType.Swordsman, Faction.Skeletons, 11, 1, wait: true);
        int reachable = At(combat, UnitType.Mage, Faction.Skeletons, 15, 1, wait: true);
        foreach (int cell in new[] { 7, 8, 10, 12, 13, 14 })
            for (int anchor = 1; anchor <= 3; anchor++) At(combat, UnitType.Crossbowman, Faction.Adventurers, cell, anchor, wait: true);
        Assert.Equal(2, combat.Board.Distance(16, combat.Read(near).Hex!.Position.Cell));
        Assert.Equal(3, combat.Board.Distance(16, combat.Read(reachable).Hex!.Position.Cell));
        combat.Step(1, []);
        UnitState advancing = combat.Read(actor); Assert.Equal(reachable, advancing.TargetId);
        Assert.Equal(reachable, advancing.Decision!.ObjectiveId); Assert.Equal(UnitActionKind.Moving, advancing.Hex!.Action);
        Assert.Equal(17, advancing.Hex.Destination.Cell); Assert.Equal(2, advancing.Decision.Route.Length);
    }
    [Fact]
    public void FormationAllocatesRearSupportAndSpreadsMeleeBeforePacking()
    {
        using var a = new CombatSimulation(new(), seed: 123); using var b = new CombatSimulation(new(), seed: 123);
        a.Create(UnitType.Mage, 1, 1, 1); a.Create(UnitType.Crossbowman, 1, 1, 1);
        for (int n = 0; n < 6; n++) a.Create(UnitType.Swordsman, 1, 1, 1);
        foreach (UnitState unit in a.Snapshot().Reverse()) b.Seed(unit);
        a.BeginWave(); b.BeginWave();
        Assert.Equal(JsonSerializer.Serialize(a.Snapshot()), JsonSerializer.Serialize(b.Snapshot()));
        Assert.All(a.Snapshot().Where(u => u.Class != UnitClass.Melee), u => Assert.Contains(u.Hex!.Position.Cell, a.Board.Rear(u.Faction)));
        UnitState[] melee = a.Snapshot().Where(u => u.Class == UnitClass.Melee).ToArray();
        Assert.All(melee, u => Assert.Contains(u.Hex!.Position.Cell, a.Board.Front(u.Faction)));
        var columns = melee.GroupBy(u => u.Hex!.Position.Cell).ToArray();
        Assert.Equal(3, columns.Length); Assert.All(columns, g => Assert.Equal(2, g.Count()));
        Assert.All(columns, g => Assert.Collection(g.Select(u => u.Hex!.Position.Anchor).Order(), f => Assert.Equal(1, f), f => Assert.Equal(2, f)));
        Assert.Equal(new HexPosition(20, 1), a.Snapshot().Single(u => u.Type == UnitType.Crossbowman).Hex!.Position);
        Assert.Equal(new HexPosition(19, 1), a.Snapshot().Single(u => u.Type == UnitType.Mage).Hex!.Position);
        Assert.All(a.Snapshot(), u => { Assert.False(u.PendingImpact); Assert.Equal(0, u.AttackSequence); });
    }
    [Fact]
    public void ClearedProtectedEntryBoundUsesCumulativeCompatibleDeathReleases()
    {
        using var combat = new CombatSimulation(new(), seed: 123);
        foreach (int cell in combat.Board.Front(Faction.Skeletons))
            foreach (int anchor in new[] { 1, 2, 3 }) At(combat, UnitType.Swordsman, Faction.Adventurers, cell, anchor);
        foreach (int cell in combat.Board.Rear(Faction.Skeletons))
            for (int anchor = 1; anchor <= 3; anchor++)
            {
                int id = At(combat, UnitType.Crossbowman, Faction.Skeletons, cell, anchor);
                UnitState body = combat.Read(id);
                combat.Seed(body with
                {
                    Health = 0,
                    Hex = body.Hex! with
                    {
                        Lifecycle = UnitLifecycle.Dying,
                        DeathStartTick = anchor == 1 ? 0 : 24,
                        DeathEndTick = anchor == 1 ? 48 : 72
                    }
                });
            }
        int incoming = combat.Create(UnitType.Swordsman, 0, 2, 2, Faction.Skeletons, isBoss: true);
        combat.Advance(25, []); combat.Transfer(incoming, 1); combat.TrackClearedAdmission(1);
        AdmissionBound bound = Assert.Single(combat.Admissions); Assert.Equal(72, bound.FirstAdmissionBound);
        for (int tick = 26; tick < 72; tick++)
        {
            combat.Step(tick, []); combat.TrackClearedAdmission(1);
            Assert.False(combat.Read(incoming).Deployed); Assert.Equal(bound, Assert.Single(combat.Admissions));
        }
        combat.Step(72, []);
        Assert.Contains(combat.Read(incoming).Hex!.Position.Cell, combat.Board.Rear(Faction.Skeletons));
        Assert.Equal(72, Assert.Single(combat.Admissions).AdmissionTick);
        Assert.Equal(incoming, Assert.Single(combat.Admissions).FirstUnitId);
        for (int tick = 73; tick <= 84; tick++) combat.Step(tick, []);
        Assert.Contains(combat.Events(), e => e.Unit?.Id == incoming && e.Type == CombatEventType.Impact && e.Landed);
        Assert.All(combat.Snapshot().Where(u => u.Faction == Faction.Adventurers), u => Assert.DoesNotContain(u.Hex!.Position.Cell, combat.Board.Rear(Faction.Skeletons)));
    }
    [Theory]
    [InlineData(UnitType.Swordsman, false)]
    [InlineData(UnitType.Berserker, false)]
    [InlineData(UnitType.Crossbowman, false)]
    [InlineData(UnitType.Mage, false)]
    [InlineData(UnitType.Swordsman, true)]
    [InlineData(UnitType.Berserker, true)]
    [InlineData(UnitType.Crossbowman, true)]
    [InlineData(UnitType.Mage, true)]
    public void RedistributionIntoOccupiedClearedFrontageAdmitsEngagesAndCompletes(UnitType type, bool retainedDeaths)
    {
        using var match = new Match(new Rules { Campaign = CampaignFixture.Three(first: 1), DefenderDamage = 0 }, combatSeed: 123);
        match.Join(); match.Join(); VillageStrategyTests.Act(match, 1, "start");
        for (int n = 0; n < 4; n++)
            foreach (int city in new[] { 1, 2 }) VillageStrategyTests.Act(match, city, "ready");
        int oldTarget = match.Enemies.Single(e => e.Destination == 2).Id;
        foreach (UnitState unit in match.Enemies) match.Combat.Remove(unit.Id);
        int incoming = match.Combat.Create(type, 0, 1, 1, Faction.Skeletons);
        UnitState enemy = match.Combat.Read(incoming);
        long start = 27 - enemy.Profile.WindupTicks, ready = start + enemy.Profile.CadenceTicks;
        match.Combat.Seed(enemy with
        {
            Health = enemy.Health - 100,
            Hex = CombatFixture.At(enemy, 17, 1) with
            { Action = UnitActionKind.Windup, ActionSequence = 1, StartTick = start, EndTick = ready },
            TargetId = 1,
            TargetCity = true,
            PendingImpact = true,
            AttackSequence = 1,
            ActionStartTick = start,
            ImpactTick = 27,
            ReadyTick = ready
        });
        match.Players[1].Health = 1; CombatFixture.Steps(match, 25);
        // The cleared city retains ordinary committed attacks against its old
        // target. Their normal miss/recovery deadlines let arrivals engage;
        // do not fabricate an extra-long recovery to manufacture that proof.
        foreach (int cell in match.Combat.Board.Front(Faction.Skeletons))
        {
            UnitType guardType = cell == 6 ? UnitType.Mage : UnitType.Swordsman;
            int id = match.Combat.Create(guardType, 2, 2, 2);
            UnitState guard = match.Combat.Read(id); long guardReady = 24 + guard.Profile.CadenceTicks;
            match.Combat.Seed(guard with
            {
                Hex = CombatFixture.At(guard, cell, 1) with
                { Action = UnitActionKind.Windup, ActionSequence = 1, StartTick = 24, EndTick = guardReady },
                TargetId = oldTarget,
                AttackSequence = 1,
                PendingImpact = true,
                ActionStartTick = 24,
                ImpactTick = 24 + guard.Profile.WindupTicks,
                ReadyTick = guardReady
            });
        }
        if (retainedDeaths)
            foreach (int cell in match.Combat.Board.Rear(Faction.Skeletons))
                for (int anchor = 1; anchor <= 3; anchor++)
                {
                    int id = match.Combat.Create(UnitType.Crossbowman, 0, 2, 2, Faction.Skeletons);
                    UnitState corpse = match.Combat.Read(id);
                    match.Combat.Seed(corpse with
                    {
                        Health = 0,
                        Hex = CombatFixture.At(corpse, cell, anchor) with
                        { Lifecycle = UnitLifecycle.Dying, DeathStartTick = anchor == 1 ? 0 : 24, DeathEndTick = anchor == 1 ? 48 : 72 }
                    });
                }
        UnitState before = match.Combat.Read(incoming); CombatFixture.Steps(match, 2);
        Assert.True(match.Players[1].Eliminated);
        UnitState transferred = Assert.Single(match.Enemies);
        Assert.Equal((before.Id, before.Health, before.Profile, before.ReadyTick),
            (transferred.Id, transferred.Health, transferred.Profile, transferred.ReadyTick));
        Assert.Equal(2, transferred.Destination); Assert.False(transferred.PendingImpact);
        long bound = retainedDeaths ? 48 : 27;
        Assert.Equal(bound, Assert.Single(match.Snapshot().Admissions).FirstAdmissionBound);
        while (match.Tick < bound)
        {
            Assert.False(match.Combat.Read(incoming).Deployed); match.Step();
            Assert.Equal(bound, Assert.Single(match.Snapshot().Admissions).FirstAdmissionBound);
        }
        Assert.Contains(match.Combat.Read(incoming).Hex!.Position.Cell, match.Combat.Board.Rear(Faction.Skeletons));
        Assert.Equal(bound, Assert.Single(match.Snapshot().Admissions).AdmissionTick);
        bool attacked = false, healthProgress = false;
        while (match.Phase == Phase.Combat && match.Tick < 1200)
        {
            match.Step();
            attacked |= match.Combat.Events().Any(e => e.Type == CombatEventType.Impact && e.Unit?.Id == incoming && e.Landed && e.Tick >= bound);
            healthProgress |= match.Combat.Events().Any(e => e.Type == CombatEventType.Hit && e.Tick >= bound);
        }
        Assert.True(attacked); Assert.True(healthProgress); Assert.Equal(Phase.Building, match.Phase);
        Assert.Equal(DefeatReason.None, match.DefeatReason); Assert.Empty(match.Enemies);
        Assert.All(match.Players[2].Soldiers, u => Assert.DoesNotContain(u.Hex!.Position.Cell, match.Combat.Board.Rear(Faction.Skeletons)));
        output.WriteLine($"Transferred {type}: retainedDeaths={retainedDeaths}, transfer=27, admission={bound}, completion={match.Tick}, remaining soldier HP={match.Players[2].Soldiers.Sum(u => u.Health)}.");
    }
    private sealed record RoleRun(int ClearTick, int SupportEndTick, int Health, int SecondaryHits, Dictionary<int, decimal> Damage, long[] ImpactTicks, int? FirstSupportDamageTick, int SupportId, int[] EnemyTargets);
    private static RoleRun Role(UnitType support, int screen, int enemies, UnitType enemy = UnitType.Swordsman)
    {
        using var combat = new CombatSimulation(new(), seed: 123);
        // Keep enemy and support identities identical when adding/removing the
        // screen so the control changes only its formation, not random domains.
        for (int n = 0; n < enemies; n++) combat.Create(enemy, 0, 1, 1, Faction.Skeletons);
        int actor = combat.Create(support, 1, 1, 1);
        for (int n = 0; n < screen; n++) combat.Create(UnitType.Swordsman, 1, 1, 1);
        combat.BeginWave(); var credits = new Dictionary<int, decimal>(); var impacts = new List<long>(); var enemyTargets = new List<int>(); int secondary = 0, end = 6000;
        int tick = 0; int? firstDamage = null;
        while (tick++ < 6000 && combat.Enemies().Length > 0 && combat.Soldiers(1).Length > 0)
        {
            Dictionary<int, int> health = combat.Snapshot().ToDictionary(u => u.Id, u => u.Health);
            combat.Step(tick, []);
            enemyTargets.AddRange(combat.Events().Where(e => e.Tick == tick && e.Type == CombatEventType.AttackStarted && e.Unit?.Faction == Faction.Skeletons && !e.TargetCity).Select(e => e.TargetId));
            if (firstDamage is null && combat.Events().Any(e => e.Tick == tick && e.Type == CombatEventType.Hit && e.Unit?.Id == actor)) firstDamage = tick;
            CombatEvent[] all = combat.Events().Where(e => e.Tick == tick && e.Type == CombatEventType.Impact && e.Landed).ToArray();
            foreach (CombatEvent hit in all.Where(e => e.Unit?.Id == actor))
            {
                impacts.Add(tick); secondary += Math.Max(0, hit.Victims.Length - 1);
                decimal credited = 0;
                foreach (int id in hit.Victims)
                {
                    int total = all.Where(e => e.Victims.Contains(id)).Sum(e => e.Damage);
                    credited += Math.Min(health[id], total) * (decimal)hit.Damage / total;
                }
                credits[tick] = credits.GetValueOrDefault(tick) + credited;
            }
            if (end == 6000 && !combat.Soldiers(1).Any(u => u.Id == actor)) end = tick;
        }
        Assert.True(tick < 6000, "Role fixture did not complete.");
        return new(combat.Enemies().Length == 0 ? tick - 1 : int.MaxValue, Math.Min(end, tick - 1), combat.Soldiers(1).Sum(u => u.Health), secondary, credits, impacts.ToArray(), firstDamage, actor, enemyTargets.ToArray());
    }
    [Fact]
    public void MageMakesAnEffectiveContributionToOrdinaryTwoVictimClustering()
    {
        RoleRun mage = Role(UnitType.Mage, 6, 6), crossbow = Role(UnitType.Crossbowman, 6, 6);
        int interval = Math.Min(mage.SupportEndTick, crossbow.SupportEndTick);
        decimal Damage(RoleRun r) => r.Damage.Where(p => p.Key <= interval).Sum(p => p.Value);
        output.WriteLine($"Ordinary 6-vs-6 screen: Mage equipment=15 cloth/5 gold upkeep=2 damage={Damage(mage)} secondary={mage.SecondaryHits} clear={mage.ClearTick} health={mage.Health}; Crossbow equipment=5 metal/10 wood upkeep=1 damage={Damage(crossbow)} clear={crossbow.ClearTick} health={crossbow.Health}; shared live interval={interval}.");
        Assert.NotEqual(int.MaxValue, mage.ClearTick); Assert.NotEqual(int.MaxValue, crossbow.ClearTick);
        Assert.True(mage.SecondaryHits > 0); Assert.True(Damage(mage) > Damage(crossbow));
        Assert.True(mage.ClearTick < crossbow.ClearTick || mage.Health > crossbow.Health);
    }
    [Fact]
    public void MeleeScreenProtectsSupportThroughItsInitialAttacks()
    {
        RoleRun screen = Role(UnitType.Crossbowman, 1, 1), exposed = Role(UnitType.Crossbowman, 0, 1);
        output.WriteLine($"Screened support impacts={string.Join(',', screen.ImpactTicks)} first damage={screen.FirstSupportDamageTick}; unscreened impacts={string.Join(',', exposed.ImpactTicks)} first damage={exposed.FirstSupportDamageTick}.");
        Assert.Equal(screen.SupportId, exposed.SupportId); Assert.NotNull(exposed.FirstSupportDamageTick); Assert.NotEmpty(screen.ImpactTicks);
        Assert.True(screen.FirstSupportDamageTick is null || screen.FirstSupportDamageTick > exposed.FirstSupportDamageTick);
        Assert.True(screen.FirstSupportDamageTick is null || screen.FirstSupportDamageTick > screen.ImpactTicks[0]);
        Assert.True(screen.ImpactTicks[0] < exposed.FirstSupportDamageTick);
        Assert.NotEqual(screen.SupportId, screen.EnemyTargets[0]); Assert.Equal(exposed.SupportId, exposed.EnemyTargets[0]);
        Assert.True(screen.Health > exposed.Health);
    }
    [Fact]
    public void CrowdedMixedFormationAdmitsQueuedMeleeThatActuallyAttacks()
    {
        using var combat = new CombatSimulation(new(), seed: 123);
        for (int n = 0; n < 12; n++) combat.Create(UnitType.Swordsman, 1, 1, 1);
        for (int n = 0; n < 9; n++) combat.Create(UnitType.Crossbowman, 1, 1, 1);
        UnitType[] opponents = [UnitType.Swordsman, UnitType.Berserker, UnitType.Crossbowman, UnitType.Mage];
        for (int n = 0; n < 18; n++) combat.Create(opponents[n % opponents.Length], 0, 1, 1, Faction.Skeletons);
        combat.BeginWave(); int[] queued = combat.Soldiers(1).Where(u => !u.Deployed && u.Class == UnitClass.Melee).Select(u => u.Id).ToArray();
        Assert.Equal(3, queued.Length); bool attacked = false;
        for (int tick = 1; tick <= 1000 && !attacked; tick++)
        {
            combat.Step(tick, []);
            attacked = combat.Events().Any(e => e.Tick == tick && e.Type == CombatEventType.Impact && e.Landed && e.Unit is not null && queued.Contains(e.Unit.Id));
        }
        Assert.True(attacked); Assert.NotEmpty(combat.Enemies());
        Assert.Contains(combat.Soldiers(1).Concat(combat.Dying()), u => u.Type == UnitType.Crossbowman);
    }
    [Fact]
    public void LowerInitiativeWinsTransitContentionWhileIndependentMoveStartsTogether()
    {
        using var combat = new CombatSimulation(new Rules { RangedReach = 1, Combat = new() { Berserker = new(6, 20, 27, 48) } }, seed: 123);
        int support = At(combat, UnitType.Crossbowman, Faction.Adventurers, 14, 1);
        int melee = At(combat, UnitType.Swordsman, Faction.Adventurers, 14, 2);
        int independent = At(combat, UnitType.Swordsman, Faction.Adventurers, 7, 1);
        At(combat, UnitType.Berserker, Faction.Adventurers, 13, 1, wait: true);
        At(combat, UnitType.Berserker, Faction.Adventurers, 19, 1, wait: true);
        At(combat, UnitType.Swordsman, Faction.Adventurers, 17, 1, wait: true);
        At(combat, UnitType.Swordsman, Faction.Adventurers, 17, 3, wait: true);
        At(combat, UnitType.Mage, Faction.Skeletons, 16, 1, wait: true);
        At(combat, UnitType.Mage, Faction.Skeletons, 2, 1, wait: true);
        combat.Step(1, []);
        Assert.Equal(UnitActionKind.Waiting, combat.Read(support).Hex!.Action);
        Assert.Equal(UnitActionKind.Moving, combat.Read(melee).Hex!.Action);
        Assert.Equal(UnitActionKind.Moving, combat.Read(independent).Hex!.Action);
        Assert.Equal(1, combat.Read(melee).Hex!.StartTick); Assert.Equal(1, combat.Read(independent).Hex!.StartTick);
        Assert.Equal(2, combat.Reservations.Transit.Length);
    }
    [Fact]
    public void UnchangedBlockedEpisodeKeepsItsSeededRankAndObjective()
    {
        using var combat = new CombatSimulation(new Rules { Combat = new() { Swordsman = new(6, 10, 30, 48) } }, seed: 123);
        int actor = At(combat, UnitType.Swordsman, Faction.Adventurers, 11, 1);
        foreach (int cell in combat.Board.Cell(11).Neighbors) At(combat, UnitType.Swordsman, Faction.Adventurers, cell, 1, wait: true);
        At(combat, UnitType.Swordsman, Faction.Skeletons, 1, 1, wait: true);
        combat.Step(1, []); CombatDecisionState initial = combat.Read(actor).Decision!;
        for (int tick = 2; tick <= 50; tick++)
        {
            combat.Step(tick, []); UnitState unit = combat.Read(actor);
            Assert.Equal(initial.SchedulingRank, unit.Decision!.SchedulingRank); Assert.Equal(initial.ObjectiveId, unit.Decision.ObjectiveId);
            Assert.Equal(initial.Sequence, unit.Decision.Sequence); Assert.Equal(initial.Generation, unit.Decision.Generation);
            Assert.Equal(UnitActionKind.Waiting, unit.Hex!.Action); Assert.Equal(11, unit.Hex.Position.Cell); Assert.False(unit.Hex.HoldsTransit);
        }
        int other = combat.Create(UnitType.Swordsman, 2, 2, 2);
        CombatUnit elsewhere = combat.Inspect(other);
        HexTransition transition = combat.Board.Transition(new(17, 1), new(14, 1));
        combat.Seed(elsewhere with
        {
            Location = new(UnitLifecycle.Alive, transition.Source),
            Action = CombatActions.Move(elsewhere.Action, transition, 50, 30)
        });
        int searches = combat.RouteSearches;
        for (int tick = 51; tick <= 80; tick++) combat.Step(tick, []);
        Assert.Equal(initial.Generation, combat.Read(actor).Decision!.Generation);
        Assert.Equal(initial.SchedulingRank, combat.Read(actor).Decision!.SchedulingRank);
        Assert.Equal(initial.ObjectiveId, combat.Read(actor).Decision!.ObjectiveId);
        Assert.Equal(searches, combat.RouteSearches);
    }
    [Fact]
    public void IsolatedOpponentPreservesCrossbowmansCadenceAndSurvivalAdvantage()
    {
        RoleRun mage = Role(UnitType.Mage, 0, 1, UnitType.Mage), crossbow = Role(UnitType.Crossbowman, 0, 1, UnitType.Mage);
        output.WriteLine($"Isolated Mage opponent: Mage clear={mage.ClearTick} health={mage.Health} impacts={string.Join(',', mage.ImpactTicks)}; Crossbow clear={crossbow.ClearTick} health={crossbow.Health} impacts={string.Join(',', crossbow.ImpactTicks)}.");
        Assert.Equal(0, mage.SecondaryHits); Assert.Equal(0, crossbow.SecondaryHits);
        Assert.Equal(90, mage.ImpactTicks[1] - mage.ImpactTicks[0]); Assert.Equal(60, crossbow.ImpactTicks[1] - crossbow.ImpactTicks[0]);
        Assert.True(crossbow.ClearTick < mage.ClearTick); Assert.True(crossbow.Health > mage.Health);
    }
}
