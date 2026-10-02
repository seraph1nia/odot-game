using System.Text.Json;
using Xunit;

namespace Game.Core.Tests;

public sealed class CombatTests(Xunit.Abstractions.ITestOutputHelper output)
{
    private static readonly HexBoard TestBoard = new(HexBoardDefinition.Default());
    private static Match Battle(Rules? rules = null, int soldiers = 0, bool mixed = false, bool reverse = false)
    {
        var match = new Match(rules, combatSeed: 123); match.Join();
        int[] ids = Enumerable.Range(1, soldiers).ToArray();
        foreach (int id in reverse ? ids.Reverse() : ids)
        {
            UnitType type = mixed ? (UnitType)((id - 1) % 4) : UnitType.Swordsman;
            match.Combat.Seed(new(id, match.Combat.Profile(type).Health, 0, 1, 1)
            { Owner = 1, Type = type, Deployed = false });
        }
        Apply(match, "start"); Apply(match, "ready"); Apply(match, "ready"); Apply(match, "ready"); Apply(match, "ready");
        return match;
    }
    private static CommandResult Apply(Match match, string action, int slot = -1, UnitType type = UnitType.Swordsman)
        => match.Apply(1, new(1, match.Id, match.Phase, match.TurnSerial, action, 1, slot, SoldierType: type));
    private static CombatSimulation Duel(Rules? rules = null, UnitType type = UnitType.Swordsman)
    {
        var combat = new CombatSimulation(rules ?? new Rules { DefenderDamage = 0 });
        int soldier = combat.Create(type, 1, 1, 1), enemy = combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons);
        combat.Seed(combat.Read(soldier) with { Hex = CombatFixture.At(combat.Read(soldier), 11, type == UnitType.Crossbowman ? 1 : 7), Deployed = true });
        combat.Seed(combat.Read(enemy) with { Hex = CombatFixture.At(combat.Read(enemy), type == UnitType.Crossbowman ? 2 : 8, 7), Deployed = true });
        return combat;
    }
    private static void Separated(UnitState[] units)
    {
        foreach (UnitState unit in units.Where(u => u.Deployed))
        {
            Assert.NotNull(unit.Hex); Assert.InRange(unit.Hex.Position.Cell, 1, 21);
            foreach (UnitState other in units.Where(u => u.Deployed && u.Destination == unit.Destination && u.Id > unit.Id))
            {
                if (unit.Hex.Position.Cell != other.Hex!.Position.Cell) continue;
                Assert.Equal(unit.Faction, other.Faction);
                Assert.Equal(0, TestBoard.Footprint(unit.Hex.Position.Footprint).Mask & TestBoard.Footprint(other.Hex.Position.Footprint).Mask);
            }
        }
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CrowdedBattleIsSeparatedDeterministicAndMakesProgress(bool mixed)
    {
        var rules = new Rules { WaveOne = 32, DefenderDamage = 0 };
        using Match a = Battle(rules, 32, mixed); using Match b = Battle(rules, 32, mixed, reverse: true);
        Assert.NotEqual(a.Id, b.Id);
        bool hadDeaths = false, hadMageSecondary = false; var deaths = new HashSet<int>();
        long outcomeTick = 0;
        int steps = 0;
        while ((a.Phase == Phase.Combat || a.Combat.HasDeaths) && steps++ < 6000)
        {
            // Reading/traversing snapshots and events cannot advance any combat stream.
            if (steps % 7 == 0)
            {
                _ = JsonSerializer.Serialize(b.Snapshot(), WireJson.Options);
                _ = b.Combat.Events().Reverse().Select(e => e.Victims.Reverse().ToArray()).ToArray();
                _ = b.Combat.Dying().Reverse().ToArray();
            }
            a.Step(); b.Step();
            if (outcomeTick == 0 && a.Phase != Phase.Combat) outcomeTick = a.Tick;
            UnitState[] units = a.Combat.Snapshot(); Separated(units);
            foreach (CombatEvent death in a.Combat.Events().Where(e => e.Tick == a.Tick && e.Type == CombatEventType.Death)) deaths.Add(death.Unit!.Id);
            hadMageSecondary |= a.Combat.Events().Any(e => e.Tick == a.Tick && e.Type == CombatEventType.Impact && e.Unit?.Type == UnitType.Mage && e.Landed && e.Victims.Length > 1);
            UnitState[] current = units.Concat(a.Combat.Dying()).ToArray();
            var currentIds = current.Select(u => u.Id).ToHashSet();
            Assert.Equal(64, current.Length + deaths.Count(id => !currentIds.Contains(id)));
            var index = new HexOccupancy(a.Combat.Board); index.Rebuild(current.Select(u => u.Hex!));
            Assert.Equal(JsonSerializer.Serialize(index.Snapshot()), JsonSerializer.Serialize(a.Combat.Reservations));
            Assert.All(current.Where(u => u.Deployed), u => Assert.InRange(u.Hex!.Position.Cell, 1, 21));
            Assert.All(current.Where(u => !u.Deployed), u => Assert.False(u.PendingImpact));
            MatchSnapshot first = a.Snapshot() with { MatchId = "normalized-session", Revision = 0 };
            MatchSnapshot second = b.Snapshot() with { MatchId = "normalized-session", Revision = 0 };
            Assert.True(JsonSerializer.Serialize(first, WireJson.Options) == JsonSerializer.Serialize(second, WireJson.Options),
                $"First divergent complete combat state: tick={a.Tick}, seed={a.CombatSeed}, config={a.Configuration.Fingerprint}, mixed={mixed}.");
            hadDeaths |= a.Enemies.Count < 32 || a.Players[1].Soldiers.Count < 32;
        }
        output.WriteLine($"Crowded mixed={mixed}, seed={a.CombatSeed}, config={a.Configuration.Fingerprint}: outcome={outcomeTick}, cleanup={a.Tick}, {a.Phase}, survivors={a.Combat.Snapshot().Length}, Mage secondary={hadMageSecondary}");
        Assert.True(hadDeaths); Assert.NotEqual(Phase.Combat, a.Phase); Assert.Equal(a.Phase, b.Phase);
        Assert.Empty(a.Combat.Dying()); Assert.Empty(b.Combat.Dying());
        if (mixed) Assert.True(hadMageSecondary, "The mixed complete-trace fixture must exercise actual splash victim selection.");
        Assert.NotEqual(DefeatReason.BattleStalled, a.DefeatReason);
    }
    [Fact]
    public void LargeEntryQueuesConserveUnitsAndNeverOverlap()
    {
        using Match match = Battle(new Rules { WaveOne = 80 }, 80);
        UnitState[] units = match.Combat.Snapshot();
        Assert.Equal(160, units.Length); Assert.Contains(units, u => !u.Deployed); Separated(units);
        match.Step(); Assert.Equal(160, match.Combat.Snapshot().Length); Separated(match.Combat.Snapshot());
        Assert.All(match.Combat.Snapshot().Where(u => !u.Deployed), u => Assert.False(u.PendingImpact));
        var deaths = new HashSet<int>(); int steps = 1;
        while (match.Phase == Phase.Combat && steps++ < 18000)
        {
            match.Step();
            foreach (CombatEvent death in match.Combat.Events().Where(e => e.Tick == match.Tick && e.Type == CombatEventType.Death)) deaths.Add(death.Unit!.Id);
            UnitState[] current = match.Combat.Snapshot().Concat(match.Combat.Dying()).ToArray();
            var currentIds = current.Select(u => u.Id).ToHashSet();
            Assert.Equal(160, current.Length + deaths.Count(id => !currentIds.Contains(id)));
            var index = new HexOccupancy(match.Combat.Board); index.Rebuild(current.Select(u => u.Hex!));
            Assert.Equal(JsonSerializer.Serialize(index.Snapshot()), JsonSerializer.Serialize(match.Combat.Reservations));
            Assert.All(current.Where(u => u.Deployed), u => Assert.InRange(u.Hex!.Position.Cell, 1, 21));
            Assert.All(current.Where(u => !u.Deployed), u => Assert.False(u.PendingImpact));
        }
        output.WriteLine($"80-per-side queue: {steps} ticks, {match.Phase}, reason={match.DefeatReason}, deaths={deaths.Count}.");
        Assert.NotEqual(Phase.Combat, match.Phase); Assert.NotEqual(DefeatReason.BattleStalled, match.DefeatReason);
    }
    [Fact]
    public void MeleeStopsAtReachAndImpactsAreSimultaneous()
    {
        using var combat = Duel(new Rules { SoldierDamage = 10, DefenderDamage = 0 });
        for (int tick = 1; tick <= 12; tick++)
        {
            combat.Step(tick, []); UnitState[] units = combat.Snapshot();
            Assert.Equal(2, units.Length); Assert.All(units, u => Assert.Equal(1000, u.Health)); Separated(units);
            Assert.All(units, u => { Assert.False(u.Hex!.HoldsTransit); Assert.Equal(1, u.AttackSequence); Assert.Equal(13, u.ImpactTick); });
        }
        combat.Step(13, []); Assert.Empty(combat.Snapshot());
        Assert.Equal(2, combat.Events().Count(e => e.Type == CombatEventType.Death));
        Assert.Equal(2, combat.Events().Count(e => e.Type == CombatEventType.Impact && e.Landed && e.Tick == 13));
    }
    [Fact]
    public void RecoveryPreventsDuplicateHitsAndRetainsCadence()
    {
        using var combat = Duel(new Rules { SoldierHealth = 100, SoldierDamage = 1, BerserkerDamage = 1 });
        for (int tick = 1; tick <= 90; tick++) combat.Step(tick, []);
        Assert.Equal(new long[] { 13, 73 }, combat.Events().Where(e => e.Type == CombatEventType.Impact && e.Unit?.Id == 1).Select(e => e.Tick));
        Assert.Equal(9800, combat.Read(2).Health); Assert.Equal(2, combat.Read(1).AttackSequence);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DeadOrTransferredWindupTargetDoesNotReceiveAnOrphanHit(bool transfer)
    {
        using var combat = Duel(type: UnitType.Crossbowman);
        combat.Step(1, []);
        if (transfer) combat.Transfer(2, 2); else combat.Remove(2);
        for (int tick = 2; tick <= 19; tick++) combat.Step(tick, []);
        CombatEvent result = Assert.Single(combat.Events(), e => e.Type == CombatEventType.Impact && e.Unit?.Id == 1);
        Assert.False(result.Landed); Assert.Equal(2, result.TargetId);
        if (transfer) Assert.Equal(1000, combat.Read(2).Health);
    }
    [Fact]
    public void CrossbowmanHoldsShootingRangeAndShootsAtItsImpactTick()
    {
        using var combat = Duel(type: UnitType.Crossbowman);
        combat.Step(1, []); Assert.False(combat.Read(1).Hex!.HoldsTransit); Assert.True(combat.Read(1).PendingImpact);
        for (int tick = 2; tick <= 18; tick++) combat.Step(tick, []);
        Assert.Equal(1000, combat.Read(2).Health);
        combat.Step(19, []); Assert.Equal(700, combat.Read(2).Health);
        Assert.All(combat.Events().Where(e => e.Type == CombatEventType.Impact && e.Landed && e.Unit?.Id == 1), e => Assert.Equal(19, e.Tick));
    }
    [Fact]
    public void RangedUnitApproachesBeyondRangeAndStillShootsWhenReachedInMelee()
    {
        using var combat = Duel(type: UnitType.Crossbowman);
        combat.Seed(combat.Read(1) with { Hex = CombatFixture.At(combat.Read(1), 17, 1) }); combat.Step(1, []);
        Assert.False(combat.Read(1).PendingImpact); Assert.Equal(UnitActionKind.Moving, combat.Read(1).Hex!.Action); Assert.Empty(combat.Events());
        using var close = Duel(type: UnitType.Crossbowman);
        close.Seed(close.Read(2) with { Hex = CombatFixture.At(close.Read(2), 8, 7) });
        for (int tick = 1; tick <= 19; tick++) { close.Step(tick, []); Separated(close.Snapshot()); }
        Assert.Equal(400, close.Read(1).Health); Assert.Equal(700, close.Read(2).Health);
        Assert.Equal(3, close.Read(1).Profile.HexRange);
    }
    [Fact]
    public void PauseFreezesPendingImpactAndEventIdentityAcrossResynchronization()
    {
        using Match match = Battle(new Rules { DefenderDamage = 0 }, 1);
        CombatFixture.Change(match, match.Enemies[0].Id, e => e with { Hex = CombatFixture.At(e, 14, 7) });
        match.Step(); Assert.True(match.Players[1].Soldiers[0].PendingImpact); Apply(match, "pause");
        string frozen = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
        CombatFixture.Steps(match, 1000); Assert.Equal(frozen, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
        Apply(match, "resume"); CombatFixture.Steps(match, 12);
        Assert.Contains(match.Snapshot().CombatEvents, e => e.Type == CombatEventType.Impact && e.Unit?.Type == UnitType.Swordsman && e.Landed);
    }
    [Fact]
    public void TypedRecruitmentIsAtomicAndFoodOnlyWithUpgradeReduction()
    {
        using var match = new Match(); match.Join(); Apply(match, "start");
        Assert.True(match.Apply(1, new(1, match.Id, match.Phase, match.TurnSerial, "build", 1, 0, Building.Farm)).Accepted);
        Assert.True(match.Apply(1, new(1, match.Id, match.Phase, match.TurnSerial, "build", 1, 1, Building.Barracks)).Accepted);
        Apply(match, "ready"); Apply(match, "ready");
        Assert.True(match.Apply(1, new(1, match.Id, match.Phase, match.TurnSerial, "build", 1, 2, Building.ArcheryRange)).Accepted);
        string before = JsonSerializer.Serialize(match.Snapshot());
        Assert.False(Apply(match, "recruit", 1, (UnitType)999).Accepted); Assert.Equal(before, JsonSerializer.Serialize(match.Snapshot()));
        Assert.True(Apply(match, "recruit", 1).Accepted);
        var command = new Command(10, match.Id, match.Phase, match.TurnSerial, "recruit", 1, 2, SoldierType: UnitType.Crossbowman);
        var ledger = new CommandLedger(); Assert.True(ledger.Execute(command, () => match.Apply(1, command)).Accepted);
        Assert.True(ledger.Execute(command, () => throw new InvalidOperationException("Applied twice")).Accepted);
        Assert.Equal(new[] { UnitType.Swordsman, UnitType.Crossbowman }, match.Players[1].Soldiers.Select(u => u.Type));
        Assert.Equal(0, match.Players[1].Food); Assert.Equal(20, match.Players[1].Gold);
        Assert.Equal(800, match.Players[1].Soldiers.Single(u => u.Type == UnitType.Crossbowman).Health);
        Apply(match, "ready"); Apply(match, "ready"); Assert.Equal(Phase.Combat, match.Phase);
        Assert.False(Apply(match, "recruit", 1, UnitType.Crossbowman).Accepted);
    }
    [Fact]
    public void CombatDisposalReleasesRegistryAndNeverReusesAStableUnitId()
    {
        var combat = new CombatSimulation(new());
        int first = combat.Create(UnitType.Swordsman, 1, 1, 1); combat.Remove(first);
        int second = combat.Create(UnitType.Swordsman, 1, 1, 1); Assert.True(second > first);
        combat.Dispose(); combat.Dispose(); Assert.True(combat.IsDisposed); Assert.True(combat.RegistryReleased);
        Assert.Throws<ObjectDisposedException>(() => combat.Snapshot());
    }
    [Theory]
    [InlineData(UnitType.Swordsman)]
    [InlineData(UnitType.Crossbowman)]
    public void BothRecruitmentTypesRespectOwnershipPauseReadinessAndReducedCost(UnitType type)
    {
        using var match = new Match(); match.Join(); match.Join(); Apply(match, "start");
        match.Players[1].Slots[1] = new(type == UnitType.Crossbowman ? Building.ArcheryRange : Building.Barracks, 2); match.Players[1].Food = 8;
        Command recruit = new(10, match.Id, match.Phase, match.TurnSerial, "recruit", 1, 1, SoldierType: type);
        Assert.False(match.Apply(2, recruit).Accepted); Assert.Equal(8, match.Players[1].Food);
        Apply(match, "pause"); Assert.False(match.Apply(1, recruit).Accepted); Apply(match, "resume");
        Apply(match, "ready"); Assert.False(match.Apply(1, recruit).Accepted); Apply(match, "unready");
        Assert.True(match.Apply(1, recruit).Accepted); Assert.Equal(4, match.Players[1].Food);
        Assert.True(match.Apply(1, recruit).Accepted); Assert.Equal(0, match.Players[1].Food);
        string before = JsonSerializer.Serialize(match.Snapshot());
        Assert.False(match.Apply(1, recruit).Accepted); Assert.Equal(before, JsonSerializer.Serialize(match.Snapshot()));
    }
    [Fact]
    public void OccupiedTransferApproachConservesBodiesAndRecovery()
    {
        using Match match = Battle(new Rules { WaveOne = 32, DefenderDamage = 0, SoldierDamage = 0, RangedDamage = 0, BerserkerDamage = 0, MageDamage = 0 }, 32, mixed: true);
        CombatFixture.Steps(match, 480);
        UnitState before = match.Enemies.OrderByDescending(e => e.Cooldown).First();
        Assert.True(before.Cooldown > 0); match.Combat.Transfer(before.Id, 2); match.Combat.Transfer(before.Id, 1);
        UnitState queued = match.Combat.Read(before.Id);
        Assert.Equal(before.Health, queued.Health); Assert.Equal(before.ReadyTick, queued.ReadyTick); Assert.False(queued.PendingImpact);
        int count = match.Combat.Snapshot().Length;
        for (int step = 0; step < 60; step++) { match.Step(); Separated(match.Combat.Snapshot()); }
        Assert.Equal(count, match.Combat.Snapshot().Length);
    }
    [Fact]
    public void EventHistoryIsBoundedAndSnapshotRoundTripsAllCombatData()
    {
        using Match match = Battle(new Rules
        {
            WaveOne = 64,
            SoldierDamage = 0,
            RangedDamage = 0,
            BerserkerDamage = 0,
            MageDamage = 0,
            DefenderDamage = 0,
            RangedReach = 12,
            MeleeWindupTicks = 1,
            RangedWindupTicks = 1,
            AttackTicks = 3
        }, 64, mixed: true);
        for (int tick = 0; tick < 360; tick++) match.Step();
        MatchSnapshot state = match.Snapshot();
        Assert.InRange(state.CombatEvents.Length, 1, CombatSimulation.HistoryLimit);
        Assert.All(state.CombatEvents, e => Assert.InRange(e.Tick, state.Tick - CombatSimulation.HistoryTicks, state.Tick));
        Assert.True(state.OldestEventSequence > 1); Assert.Equal(state.CombatEvents[0].Sequence, state.OldestEventSequence);
        string json = JsonSerializer.Serialize(state, WireJson.Options);
        Assert.Equal(json, JsonSerializer.Serialize(JsonSerializer.Deserialize<MatchSnapshot>(json, WireJson.Options), WireJson.Options));
        output.WriteLine($"Crowded history: {json.Length} JSON characters, {state.CombatEvents.Length} events, oldest={state.OldestEventSequence}, newest={state.EventSequence}");
        Assert.InRange(json.Length, 1, 4_000_000);
    }
    [Fact]
    public void BurstHistoryEvictsAtRecordCapAndExpiredHistoryExposesHighWaterGap()
    {
        using var combat = new CombatSimulation(new Rules());
        for (int count = 0; count < 4097; count++) combat.Create(UnitType.Swordsman, 1, 1, 1);
        combat.EliminateArmy(1);
        Assert.Equal(4096, combat.Events().Length); Assert.Equal(2, combat.OldestEventSequence); Assert.Equal(4097, combat.EventSequence);
        Assert.Empty(combat.Snapshot()); combat.Step(121, []);
        Assert.Empty(combat.Events()); Assert.Equal(4098, combat.OldestEventSequence);
    }
}
