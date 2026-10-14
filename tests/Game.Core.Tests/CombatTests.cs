using System.Text.Json;
using Xunit;

namespace Game.Core.Tests;

public sealed class CombatTests(Xunit.Abstractions.ITestOutputHelper output)
{
    private static Match Battle(Rules? rules = null, int soldiers = 0, bool mixed = false, bool reverse = false)
    {
        var match = new Match(rules, combatSeed: 123); match.Join();
        // Deliberate domain fixture, not paid campaign evidence. Six physical
        // homes host up to eighteen members; reversed insertion retains actor IDs.
        Assert.InRange(soldiers, 0, 18);
        for (int home = 0; home < 6; home++) match.Players[1].ArmyPlots[home] = new(Building.Empty, 0);
        int[] ids = Enumerable.Range(1, soldiers).ToArray();
        foreach (int id in reverse ? ids.Reverse() : ids)
        {
            int home = (id - 1) / 3;
            UnitType type = mixed ? (UnitType)(home % 4) : UnitType.Swordsman;
            match.Combat.Seed(new UnitState(id, match.Combat.Profile(type).Health, 0, 1, 1)
            { Owner = 1, Type = type, Deployed = false, SquadId = 100 + home, FormationSlot = (id - 1) % 3, Assignment = new(-1, 0, home, 1) });
        }
        match.Players[1].Food = match.Players[1].Soldiers.Sum(u => match.Economy.Upkeep(u.Type)); // Normally fed, deliberate seeded combat fixture.
        Apply(match, "start"); Apply(match, "ready"); Apply(match, "ready"); Apply(match, "ready");
        return match;
    }
    private static CommandResult Apply(Match match, string action, int slot = -1, UnitType type = UnitType.Swordsman)
        => match.Apply(1, new(1, match.Id, match.Phase, match.TurnSerial, action, 1, slot, SoldierType: type, ExpectedGeneration: slot is >= 0 and < 9 ? match.Players[1].Slots[slot].Generation : 0));
    private static CombatSimulation Duel(Rules? rules = null, UnitType type = UnitType.Swordsman)
    {
        var combat = new CombatSimulation(rules ?? new Rules { SoldierHealth = 10, SoldierDamage = 4, RangedHealth = 8, RangedDamage = 3, DefenderDamage = 0 });
        int soldier = combat.Create(type, 1, 1, 1), enemy = combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons);
        combat.Seed(combat.Read(soldier) with { Hex = CombatFixture.At(combat.Read(soldier), 17, 1), Deployed = true });
        combat.Seed(combat.Read(enemy) with { Hex = CombatFixture.At(combat.Read(enemy), type == UnitType.Crossbowman ? 3 : 18, 1), Deployed = true });
        return combat;
    }
    private static void Separated(CombatSimulation combat)
    {
        // Tier A exclusive actors, not member-size packing. Members in a group
        // share its pose but retain distinct formation slots and health identities.
        SquadState[] actors = combat.SquadSnapshots().Where(s => s.Hex.Lifecycle == UnitLifecycle.Alive).ToArray();
        Assert.Equal(actors.Length, actors.Select(s => s.Hex.Position.Cell).Distinct().Count());
        foreach (SquadState actor in actors)
        {
            Assert.Contains(combat.Board.Cells, c => c.Id == actor.Hex.Position.Cell);
            Assert.DoesNotContain(combat.Arena.Footprints, f => f.Cell == actor.Hex.Position.Cell);
            UnitState[] members = combat.Snapshot().Where(u => actor.ParticipatingMembers.Contains(u.Id)).ToArray();
            Assert.Equal(actor.ParticipatingMembers.Length, members.Length);
            Assert.Equal(members.Length, members.Select(u => u.FormationSlot).Distinct().Count());
            Assert.All(members, u => Assert.Equal(actor.Hex.Position, u.Hex!.Position));
        }
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CrowdedBattleIsSeparatedDeterministicAndMakesProgress(bool mixed)
    {
        var rules = new Rules { Campaign = CampaignFixture.Three(first: 32), DefenderDamage = 0 };
        using Match a = Battle(rules, 18, mixed); using Match b = Battle(rules, 18, mixed, reverse: true);
        var arena = new SharedArena([1]);
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
            UnitState[] units = a.Combat.Snapshot(); Separated(a.Combat);
            foreach (CombatEvent death in a.Combat.Events().Where(e => e.Tick == a.Tick && e.Type == CombatEventType.Death)) deaths.Add(death.Unit!.Id);
            hadMageSecondary |= a.Combat.Events().Any(e => e.Tick == a.Tick && e.Type == CombatEventType.Impact && e.Unit?.Type == UnitType.Mage && e.Landed && e.Victims.Length > 1);
            UnitState[] current = units.Concat(a.Combat.Dying()).ToArray();
            var currentIds = current.Select(u => u.Id).ToHashSet();
            Assert.Equal(50, current.Length + deaths.Count(id => !currentIds.Contains(id)));
            CombatReservations rebuilt = CombatReservations.Reconstruct(arena, a.Combat.SquadSnapshots());
            Assert.Equal(JsonSerializer.Serialize(rebuilt), JsonSerializer.Serialize(a.Combat.Reservations));
            Assert.All(units.Where(u => !u.Deployed), u => Assert.False(u.PendingImpact));
            MatchSnapshot first = a.Snapshot() with { MatchId = "normalized-session", Revision = 0 };
            MatchSnapshot second = b.Snapshot() with { MatchId = "normalized-session", Revision = 0 };
            Assert.True(JsonSerializer.Serialize(first, WireJson.Options) == JsonSerializer.Serialize(second, WireJson.Options),
                $"First divergent complete combat state: tick={a.Tick}, seed={a.CombatSeed}, config={a.Configuration.Fingerprint}, mixed={mixed}.");
            hadDeaths |= a.Enemies.Count < 32 || a.Players[1].Soldiers.Count < 18;
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
        using Match match = Battle(new Rules { Campaign = CampaignFixture.Three(first: 80) }, 18);
        var arena = new SharedArena([1]);
        UnitState[] units = match.Combat.Snapshot();
        Assert.Equal(98, units.Length); Assert.Contains(units, u => !u.Deployed); Separated(match.Combat);
        match.Step(); Assert.Equal(98, match.Combat.Snapshot().Length); Separated(match.Combat);
        Assert.All(match.Combat.Snapshot().Where(u => !u.Deployed), u => Assert.False(u.PendingImpact));
        var deaths = new HashSet<int>(); int steps = 1;
        while (match.Phase == Phase.Combat && steps++ < 18000)
        {
            match.Step();
            foreach (CombatEvent death in match.Combat.Events().Where(e => e.Tick == match.Tick && e.Type == CombatEventType.Death)) deaths.Add(death.Unit!.Id);
            UnitState[] current = match.Combat.Snapshot().Concat(match.Combat.Dying()).ToArray();
            var currentIds = current.Select(u => u.Id).ToHashSet();
            Assert.Equal(98, current.Length + deaths.Count(id => !currentIds.Contains(id)));
            Separated(match.Combat);
            CombatReservations rebuilt = CombatReservations.Reconstruct(arena, match.Combat.SquadSnapshots());
            Assert.Equal(JsonSerializer.Serialize(rebuilt), JsonSerializer.Serialize(match.Combat.Reservations));
            Assert.All(match.Combat.Snapshot().Where(u => !u.Deployed), u => Assert.False(u.PendingImpact));
        }
        output.WriteLine($"80-per-side queue: {steps} ticks, {match.Phase}, reason={match.DefeatReason}, deaths={deaths.Count}.");
        Assert.NotEqual(Phase.Combat, match.Phase); Assert.NotEqual(DefeatReason.BattleStalled, match.DefeatReason);
    }
    [Fact]
    public void MeleeStopsAtReachAndImpactsAreSimultaneous()
    {
        using var combat = Duel(new Rules { SoldierHealth = 10, SoldierDamage = 10, DefenderDamage = 0 });
        for (int tick = 1; tick <= 12; tick++)
        {
            combat.Step(tick, []); UnitState[] units = combat.Snapshot();
            Assert.Equal(2, units.Length); Assert.All(units, u => Assert.Equal(1000, u.Health)); Separated(combat);
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
    public void FinalTargetLossMissesWhileSectorRedirectionRetainsCommittedGroup(bool transfer)
    {
        using var combat = Duel(type: UnitType.Crossbowman);
        combat.Step(1, []);
        if (transfer) combat.Transfer(2, 2); else combat.Remove(2);
        for (int tick = 2; tick <= 19; tick++) combat.Step(tick, []);
        CombatEvent result = Assert.Single(combat.Events(), e => e.Type == CombatEventType.Impact && e.Unit?.Id == 1);
        Assert.Equal(transfer, result.Landed); Assert.Equal(2, result.TargetId);
        if (transfer) Assert.Equal(700, combat.Read(2).Health);
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
        int rear = new SharedArena([1]).Deployment(1, 3);
        combat.Seed(combat.Read(1) with { Hex = CombatFixture.At(combat.Read(1), rear, 1) }); combat.Step(1, []);
        Assert.False(combat.Read(1).PendingImpact); Assert.Equal(UnitActionKind.Moving, combat.Read(1).Hex!.Action); Assert.Empty(combat.Events());
        using var close = Duel(type: UnitType.Crossbowman);
        close.Seed(close.Read(2) with { Hex = CombatFixture.At(close.Read(2), 18, 1) });
        for (int tick = 1; tick <= 19; tick++) { close.Step(tick, []); Separated(close); }
        Assert.Equal(400, close.Read(1).Health); Assert.Equal(700, close.Read(2).Health);
        Assert.Equal(3, close.Read(1).Profile.HexRange);
    }
    [Fact]
    public void PauseFreezesPendingImpactAndEventIdentityAcrossResynchronization()
    {
        using Match match = Battle(new Rules { DefenderDamage = 0 }, 1);
        int home = match.Players[1].Soldiers[0].Hex!.Position.Cell;
        int approach = match.Combat.Board.Cell(home).Neighbors.First(c => !match.Combat.Arena.Footprints.Any(f => f.Cell == c));
        CombatFixture.Change(match, match.Enemies[0].Id, e => e with { Hex = CombatFixture.At(e, approach, 1) });
        match.Step(); Assert.True(match.Players[1].Soldiers[0].PendingImpact); Apply(match, "pause");
        string frozen = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
        CombatFixture.Steps(match, 1000); Assert.Equal(frozen, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
        Apply(match, "resume"); CombatFixture.Steps(match, 12);
        Assert.Contains(match.Snapshot().CombatEvents, e => e.Type == CombatEventType.Impact && e.Unit?.Type == UnitType.Swordsman && e.Landed);
    }
    [Fact]
    public void TypedRecruitmentIsAtomicAndChargesEquipmentWithoutFood()
    {
        using var match = new Match(); match.Join(); Apply(match, "start");
        Assert.True(match.Apply(1, new(1, match.Id, match.Phase, match.TurnSerial, "build", 1, 0, Building.Farm)).Accepted);
        Assert.True(match.Apply(1, new(1, match.Id, match.Phase, match.TurnSerial, "build", 1, 1, Building.Barracks)).Accepted);
        match.Players[1].Metal = 20; // Explicit stock for this transaction fixture.
        match.Players[1].Wood = 30;
        Apply(match, "ready"); Apply(match, "ready");
        Assert.True(match.Apply(1, new(1, match.Id, match.Phase, match.TurnSerial, "build", 1, 2, Building.ArcheryRange)).Accepted);
        string before = JsonSerializer.Serialize(match.Snapshot());
        Assert.False(Apply(match, "recruit", 1, (UnitType)999).Accepted); Assert.Equal(before, JsonSerializer.Serialize(match.Snapshot()));
        Assert.True(Apply(match, "recruit", 1).Accepted);
        var command = new Command(10, match.Id, match.Phase, match.TurnSerial, "recruit", 1, 2, SoldierType: UnitType.Crossbowman, ExpectedGeneration: match.Players[1].Slots[2].Generation);
        var ledger = new CommandLedger(); Assert.True(ledger.Execute(command, () => match.Apply(1, command)).Accepted);
        Assert.True(ledger.Execute(command, () => throw new InvalidOperationException("Applied twice")).Accepted);
        Assert.Equal(new[] { UnitType.Swordsman, UnitType.Crossbowman }, match.Players[1].Soldiers.Select(u => u.Type));
        Assert.Equal(10, match.Players[1].Food); Assert.Equal(16, match.Players[1].Gold);
        Assert.Equal(17, match.Players[1].Metal);
        Assert.Equal(4000, match.Players[1].Soldiers.Single(u => u.Type == UnitType.Crossbowman).Health);
        Apply(match, "ready"); Assert.Equal(Phase.Combat, match.Phase);
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
    public void BothRecruitmentTypesRespectOwnershipPauseReadinessAndLevelPrices(UnitType type)
    {
        using var match = new Match(); match.Join(); match.Join(); Apply(match, "start");
        match.Players[1].Slots[1] = new(type == UnitType.Crossbowman ? Building.ArcheryRange : Building.Barracks, 2); match.Players[1].Food = 8;
        ResourceCost equipment = match.Economy.Recruitment(type, 2);
        match.Players[1].Metal = equipment.Metal * 2; match.Players[1].Wood = equipment.Wood * 2;
        Command recruit = new(10, match.Id, match.Phase, match.TurnSerial, "recruit", 1, 1, SoldierType: type, ExpectedGeneration: match.Players[1].Slots[1].Generation);
        Assert.False(match.Apply(2, recruit).Accepted); Assert.Equal(8, match.Players[1].Food);
        Apply(match, "pause"); Assert.False(match.Apply(1, recruit).Accepted); Apply(match, "resume");
        Apply(match, "ready"); Assert.False(match.Apply(1, recruit).Accepted); Apply(match, "unready");
        Assert.True(match.Apply(1, recruit).Accepted); Assert.Equal(8, match.Players[1].Food);
        Assert.True(match.Apply(1, recruit).Accepted); Assert.Equal(8, match.Players[1].Food);
        Assert.All(match.Players[1].Soldiers, u => Assert.Equal(2, u.Level));
        string before = JsonSerializer.Serialize(match.Snapshot());
        Assert.False(match.Apply(1, recruit).Accepted); Assert.Equal(before, JsonSerializer.Serialize(match.Snapshot()));
    }
    [Fact]
    public void OccupiedTransferApproachConservesBodiesAndRecovery()
    {
        using Match match = Battle(new Rules
        {
            Campaign = CampaignFixture.Three(first: 32),
            CityHealth = 100000,
            DefenderDamage = 0,
            SoldierDamage = 0,
            RangedDamage = 0,
            BerserkerDamage = 0,
            MageDamage = 0,
            Arena = new() { MageSecondaryDamage = 0, BerserkerSecondaryDamage = 0 }
        }, 18, mixed: true);
        CombatFixture.Steps(match, 480);
        UnitState before = match.Enemies.OrderByDescending(e => e.Cooldown).First();
        Assert.True(before.Cooldown > 0); match.Combat.Transfer(before.Id, 2); match.Combat.Transfer(before.Id, 1);
        UnitState redirected = match.Combat.Read(before.Id);
        Assert.Equal(before.Health, redirected.Health); Assert.Equal(before.ReadyTick, redirected.ReadyTick);
        Assert.Equal(before.PendingImpact, redirected.PendingImpact); Assert.Equal(before.Hex, redirected.Hex);
        int count = match.Combat.Snapshot().Length;
        for (int step = 0; step < 60; step++) { match.Step(); Separated(match.Combat); }
        Assert.Equal(count, match.Combat.Snapshot().Length);
    }
    [Fact]
    public void EventHistoryIsBoundedAndSnapshotRoundTripsAllCombatData()
    {
        using Match match = Battle(new Rules
        {
            Campaign = CampaignFixture.Three(first: 64),
            CityHealth = 100000,
            Arena = new() { MageSecondaryDamage = 0, BerserkerSecondaryDamage = 0 },
            SoldierDamage = 0,
            RangedDamage = 0,
            BerserkerDamage = 0,
            MageDamage = 0,
            DefenderDamage = 0,
            RangedReach = 12,
            MeleeWindupTicks = 1,
            RangedWindupTicks = 1,
            AttackTicks = 3
        }, 18, mixed: true);
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
        // Elimination no longer kills survivors. Generate a genuine burst of
        // individual casualties outside deployment; numerical cleanup emits and
        // trims them without creating thousands of illegal occupied actors.
        for (int id = 1; id <= 4097; id++)
            combat.Seed(new UnitState(id, 0, 0, 1, 1)
            { Owner = 1, SquadId = id, Profile = combat.Profile(UnitType.Swordsman), Hex = new(id, 1, Faction.Adventurers, UnitLifecycle.Reserve, default) });
        combat.Advance(0, []);
        Assert.Equal(4096, combat.Events().Length); Assert.Equal(2, combat.OldestEventSequence); Assert.Equal(4097, combat.EventSequence);
        Assert.Empty(combat.Snapshot()); combat.Step(121, []);
        Assert.Empty(combat.Events()); Assert.Equal(4098, combat.OldestEventSequence);
    }
}
