using System.Text.Json;
using Game;
using Xunit;

namespace Game.Core.Tests;

// Tier A: real command owners catch shared-purchase/assignment races and damage
// bugs, while cheaper catalog tests cannot. Fixed IDs/seed and owned ECS lifetime;
// snapshot/codec/resume are persistence, not an invented authoritative save API.
// Expected cost: milliseconds per numerical fixture, no launched game/process.
public sealed class ArmyPlotTowerTests(Xunit.Abstractions.ITestOutputHelper output)
{
    private static Match Create(int squadCapacity = 3)
    {
        var match = new Match(new Rules { StartingGold = 100, StartingWood = 100, DefenderDamage = 0, Arena = new() { SquadCapacity = squadCapacity } }, combatSeed: 17);
        match.Join(); match.Join(); Act(match, "start");
        match.Players[1].Stone = 100; match.Players[1].Metal = 100;
        return match;
    }
    private static Command Request(Match match, string action, int slot = -1, Building type = Building.Empty, int unit = 0)
        => Command.FromSnapshot(match.Snapshot(), 1, action, 1, slot, type, unitId: unit);
    private static void Act(Match match, string action, int slot = -1, Building type = Building.Empty, int unit = 0)
        => Assert.True(match.Apply(1, Request(match, action, slot, type, unit)).Accepted);
    private static void Reject(Match match, Command command)
    {
        string before = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
        Assert.False(match.Apply(1, command).Accepted);
        Assert.Equal(before, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
    }
    [Fact]
    public void DirectArbitraryPlotPurchaseRetainsPricesAndDeterministicLegalAssignmentAfterMovesRemovalAndWireLoad()
    {
        using Match match = Create(); City city = match.Players[1];
        Command stale = Request(match, "buy-plot", 11);
        ResourceCost before = city.Resources;
        Act(match, "buy-plot", 14);
        Assert.Equal(before.Gold - 5, city.Gold); Assert.Equal(3, city.PurchasedHomes);
        Assert.True(city.ArmyPlots[5].Purchased); Assert.False(city.ArmyPlots[2].Purchased);
        Reject(match, stale); Reject(match, Request(match, "buy-plot", 14));
        Reject(match, Request(match, "buy-plot", 11) with { City = 2 });
        city.Gold = 0; Reject(match, Request(match, "buy-plot", 11)); city.Gold = 90;
        Act(match, "build", 0, Building.Barracks); Act(match, "build", 1, Building.TownHall);
        Act(match, "build", 10, Building.ArrowTower);
        for (int n = 0; n < 3; n++) Act(match, "recruit", 0);
        Assert.All(city.Soldiers, u => Assert.Equal(match.Army.HomeCells[0], u.Assignment!.Tile));
        Reject(match, Request(match, "build", 9, Building.ArrowTower));
        for (int n = 0; n < 3; n++) Act(match, "recruit", 0);
        Assert.Equal(3, city.Soldiers.Count(u => u.Assignment!.Tile == match.Army.HomeCells[5]));
        Reject(match, Request(match, "recruit", 0));
        int id = city.Soldiers[0].Id;
        Act(match, "store", 1, unit: id); Act(match, "send", 1, unit: id);
        Assert.Equal(match.Army.HomeCells[0], city.Soldiers.Single(u => u.Id == id).Assignment!.Tile);
        Act(match, "retire", unit: id); Act(match, "recruit", 0);
        Assert.Equal(match.Army.HomeCells[0], city.Soldiers[^1].Assignment!.Tile);
        int gold = city.Gold; Act(match, "buy-plot", 11); Assert.Equal(gold - 8, city.Gold); // Second army acquisition costs 8, never city land too.
        Act(match, "recruit", 0);
        Assert.Equal(match.Army.HomeCells[2], city.Soldiers[^1].Assignment!.Tile);
        MatchSnapshot loaded = SnapshotPayload.Decode(SnapshotPayload.Encode(match.Snapshot()))!;
        CityState received = loaded.Players[0]; ArmyState army = received.Army!;
        Assert.Equal(Enumerable.Range(9, 6), army.Homes.Select(h => h.PlotId));
        Assert.Equal(ArmyPlotStatus.TowerOccupied, army.Homes[1].State); Assert.Equal(0, army.Homes[1].Used); // no fictitious squad members
        Assert.Equal(ArmyPlotStatus.UnitOccupied, army.Homes[2].State); Assert.Equal(ArmyPlotStatus.Locked, army.Homes[3].State);
        Assert.Equal(city.Soldiers.Select(u => u.Assignment), received.Soldiers.Select(u => u.Assignment));
        Assert.Equal(match.Army.Find(city.Soldiers, city.PurchasedHomes, 2, plots: city.ArmyPlots),
            match.Army.Find(received.Soldiers, army.PurchasedHomes, 2, plots: army.Homes.Select(h => h.Plot).ToArray()));
        Assert.DoesNotContain(city.Soldiers, u => u.Assignment!.Tile == match.Army.HomeCells[1]);
    }
    [Theory]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    public void EveryLegalArmyPlotHasAtomicWholeFootprintBuildAndExclusiveUpgradeLifecycle(int plot)
    {
        using Match match = Create(); City city = match.Players[1];
        if (plot >= 11) { Reject(match, Request(match, "build", plot, Building.ArrowTower)); Act(match, "buy-plot", plot); }
        Reject(match, Request(match, "build", plot, Building.Farm));
        Reject(match, Request(match, "build", plot, Building.ArrowTower) with { City = 2 });
        Act(match, "pause"); Reject(match, Request(match, "build", plot, Building.ArrowTower)); Act(match, "resume");
        Act(match, "ready"); Reject(match, Request(match, "build", plot, Building.ArrowTower)); Act(match, "unready");
        Act(match, "build", plot, Building.ArrowTower);
        SlotState basis = city.ArmyPlots[plot - 9]; Assert.True(basis.Generation > 0);
        StructureState physical = Assert.Single(match.Snapshot().Structures, s => s.Owner == 1 && s.Slot == plot);
        Assert.Equal(new SharedArena([1, 2]).Deployment(1, plot - 9), physical.Cell);
        Assert.Contains(match.Snapshot().Arena!.Footprints, f => f.Owner == 1 && f.Slot == plot && f.Cell == physical.Cell);
        Command rapid = Request(match, "upgrade-tower", plot, Building.RapidArrowTower);
        Command bombard = Request(match, "upgrade-tower", plot, Building.BombardTower);
        Reject(match, bombard with { ExpectedGeneration = basis.Generation + 1 });
        ResourceCost stock = city.Resources; city.Metal = 0; Reject(match, bombard); city.Resources = stock;
        Act(match, "upgrade-tower", plot, Building.BombardTower);
        Assert.Equal(stock.Gold - 4, city.Gold); Assert.Equal(stock.Metal - 1, city.Metal);
        Reject(match, rapid); Reject(match, bombard); Reject(match, Request(match, "upgrade", plot));
        TowerState tower = city.Towers[plot]; Assert.Equal(Building.BombardTower, tower.Type); Assert.Equal(2, tower.Level);
        Assert.Equal(basis.Generation, city.ArmyPlots[plot - 9].Generation);
        ResourceCost refund = city.ArmyPlots[plot - 9].Refund; stock = city.Resources;
        Act(match, "sell", plot); Assert.True(stock.TryAdd(refund, out ResourceCost refunded)); Assert.Equal(refunded, city.Resources);
        Assert.True(city.ArmyPlots[plot - 9].Purchased); Assert.Empty(city.Towers);
        Assert.DoesNotContain(match.Snapshot().Arena!.Footprints, f => f.Owner == 1 && f.Slot == plot);
        Reject(match, Request(match, "sell", plot));
        Act(match, "build", plot, Building.ArrowTower); Reject(match, rapid);
        Assert.True(city.ArmyPlots[plot - 9].Generation > basis.Generation);
    }
    [Fact]
    public void ReturningStoredUnitCannotUseAnyTowerFootprintUntilSaleReleasesOwnedCapacity()
    {
        using Match match = Create(); City city = match.Players[1];
        Act(match, "build", 0, Building.Barracks); Act(match, "build", 1, Building.TownHall);
        Act(match, "recruit", 0); int unit = city.Soldiers[0].Id; Act(match, "store", 1, unit: unit);
        Act(match, "build", 9, Building.ArrowTower); Act(match, "build", 10, Building.ArrowTower);
        Reject(match, Request(match, "send", 1, unit: unit)); Reject(match, Request(match, "recruit", 0));
        Act(match, "sell", 10); Act(match, "send", 1, unit: unit);
        Assert.Equal(match.Army.HomeCells[1], city.Soldiers[0].Assignment!.Tile);
        Assert.Equal(ArmyPlotStatus.TowerOccupied, city.Snapshot().Army!.Homes[0].State);
    }
    [Theory]
    [InlineData(Building.RapidArrowTower, 1)]
    [InlineData(Building.BombardTower, 5)]
    public void UpgradedArmyDefenseHitsOneAssignedForeignSquadWithoutCapsOrCrossHexDamage(Building path, int count)
    {
        using Match match = Create(squadCapacity: 5); City city = match.Players[1];
        Act(match, "build", 10, Building.ArrowTower); Act(match, "upgrade-tower", 10, path);
        CombatSimulation combat = match.Combat;
        var arena = new SharedArena([1, 2], [(1, 10)]);
        int cell = arena.Board.Cell(arena.Tower(1, 10)).Neighbors.First(arena.CanOccupy);
        int[] group = Enumerable.Range(0, 5).Select(_ => combat.Create(UnitType.Crossbowman, 0, 2, 2, Faction.Skeletons)).ToArray();
        foreach (int id in group) combat.Seed(combat.Inspect(id) with { Location = new(UnitLifecycle.Alive, new(cell, 1)) });
        int adjacentCell = arena.Board.Cell(cell).Neighbors.First(c => arena.CanOccupy(c) && c != arena.Tower(1, 10));
        int adjacent = combat.Create(UnitType.Crossbowman, 0, 1, 1, Faction.Skeletons);
        combat.Seed(combat.Inspect(adjacent) with { Location = new(UnitLifecycle.Alive, new(adjacentCell, 1)) });
        int friendly = combat.Create(UnitType.Swordsman, 1, 1, 1);
        int friendlyCell = arena.Board.Cells.First(c => arena.CanOccupy(c.Id) && c.Id != cell && c.Id != adjacentCell).Id;
        combat.Seed(combat.Inspect(friendly) with { Location = new(UnitLifecycle.Alive, new(friendlyCell, 1)) });
        city.Towers[10] = city.Towers[10] with { TargetId = combat.Read(group[0]).SquadId, PendingImpact = true, AttackSequence = 1, ImpactTick = 30, ReadyTick = 120 };
        int initial = combat.Read(group[0]).Health;
        TowerDefinition profile = match.Configuration.Towers.Single(t => t.Type == path && t.Level == 2);
        combat.Advance(30, match.Players.Values);
        CombatEvent impact = Assert.Single(combat.Events(), e => e.Type == CombatEventType.Impact && e.Tower?.Slot == 10);
        Assert.Equal(count, impact.Victims.Length); Assert.Equal(count, impact.Victims.Distinct().Count());
        Assert.All(impact.Victims, id => Assert.Contains(id, group));
        Assert.Equal(city.Id, impact.Tower!.City); Assert.Null(impact.Unit);
        Assert.All(group, id => Assert.Equal(initial - (id == impact.TargetId ? profile.Damage : profile.SecondaryDamage), combat.Read(id).Health));
        Assert.Equal(initial, combat.Read(adjacent).Health);
        Assert.Equal(HealthPoints.FromWhole(match.Rules.SoldierHealth), combat.Read(friendly).Health);
        if (path == Building.RapidArrowTower) Assert.Equal(30, profile.CadenceTicks);
        combat.Advance(31, match.Players.Values); Assert.Equal(count, combat.Events().Count(e => e.Type == CombatEventType.Hit));
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BombardKeepsCommittedGroupAfterMemberDeathOrSectorRedirection(bool memberDies)
    {
        using Match match = Create(); City city = match.Players[1];
        Act(match, "build", 9, Building.ArrowTower); Act(match, "upgrade-tower", 9, Building.BombardTower);
        var arena = new SharedArena([1, 2], [(1, 9)]);
        int cell = arena.Board.Cell(arena.Tower(1, 9)).Neighbors.First(arena.CanOccupy);
        int[] group = Enumerable.Range(0, 2).Select(_ => match.Combat.Create(UnitType.Crossbowman, 0, 1, 1, Faction.Skeletons)).ToArray();
        foreach (int id in group) match.Combat.Seed(match.Combat.Inspect(id) with { Location = new(UnitLifecycle.Alive, new(cell, 1)) });
        int target = match.Combat.Read(group[0]).SquadId;
        city.Towers[9] = city.Towers[9] with { TargetId = target, PendingImpact = true, AttackSequence = 1, ImpactTick = 30 };
        if (memberDies) match.Combat.Remove(group[0]); else match.Combat.Transfer(group[0], 2);
        match.Combat.Advance(30, match.Players.Values);
        CombatEvent impact = Assert.Single(match.Combat.Events(), e => e.Type == CombatEventType.Impact);
        Assert.True(impact.Landed); Assert.Equal(target, impact.SquadId);
        Assert.True(match.Combat.Read(group[1]).Health < HealthPoints.FromWhole(match.Rules.RangedHealth));
        // Loss of the final member must miss, not retarget a neighboring group.
        foreach (int id in group) match.Combat.Remove(id);
        city.Towers[9] = city.Towers[9] with { PendingImpact = true, ImpactTick = 31 };
        match.Combat.Advance(31, match.Players.Values);
        CombatEvent miss = Assert.Single(match.Combat.Events(), e => e.Type == CombatEventType.Impact && e.Tick == 31);
        Assert.False(miss.Landed); Assert.Empty(miss.Victims);
    }
    [Fact]
    public void TowerOnlyAuthoringPublishesDistinctRulesWithoutRerollingUnchangedUnitAndDefenderDecisions()
    {
        Rules rules = new(); Rules changed = rules with { Combat = rules.Combat with { RapidArrowTower = new(3, 17), BombardTower = new(20, 80, 4, 2) } };
        using var baseline = new Match(rules, combatSeed: 1); using var variant = new Match(changed, combatSeed: 1);
        Assert.NotEqual(baseline.Configuration.Fingerprint, variant.Configuration.Fingerprint);
        Assert.Equal(baseline.Configuration.ActorDecisionFingerprint, variant.Configuration.ActorDecisionFingerprint);
        foreach (Match match in new[] { baseline, variant })
        {
            match.Join(); Act(match, "start"); Act(match, "build", 0, Building.ArrowTower);
            for (int n = 0; n < 4; n++) Act(match, "ready");
        }
        for (int n = 0; n < 180; n++)
        {
            baseline.Step(); variant.Step();
            Assert.Equal(JsonSerializer.Serialize(baseline.Enemies, WireJson.Options), JsonSerializer.Serialize(variant.Enemies, WireJson.Options));
            Assert.Equal(baseline.Players[1].Defender, variant.Players[1].Defender);
            Assert.Equal(baseline.Players[1].Towers[0], variant.Players[1].Towers[0]);
        }
        Rules meaningful = rules with { Combat = rules.Combat with { ArrowLevelOneDamage = 9 } };
        Assert.NotEqual(baseline.Configuration.ActorDecisionFingerprint, new CombatConfiguration(meaningful).ActorDecisionFingerprint);
    }
    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    [InlineData(123UL)]
    [InlineData(14056307608042553509UL)]
    public void LegacyBaseTowerSelectionIgnoresNewVariantMetadataWithOneSaltCounterfactual(ulong seed)
    {
        Rules rules = new(); Rules newFields = rules with { Combat = rules.Combat with { RapidArrowTower = new(3, 17), BombardTower = new(20, 80, 4, 2) } };
        using var baseline = new Match(rules, combatSeed: seed); using var candidate = new Match(newFields, combatSeed: seed);
        int Target(Match match)
        {
            match.Join(); Act(match, "start"); Act(match, "build", 0, Building.ArrowTower);
            var arena = new SharedArena([1]);
            int[] cells = arena.Board.Cell(arena.Tower(1, 0)).Neighbors.Where(arena.CanOccupy).Take(3).ToArray();
            Assert.Equal(3, cells.Length);
            for (int index = 0; index < cells.Length; index++)
            {
                int id = match.Combat.Create(UnitType.Crossbowman, 0, index + 1, 1, Faction.Skeletons);
                match.Combat.Seed(match.Combat.Inspect(id) with { Location = new(UnitLifecycle.Alive, new(cells[index], 1)) });
            }
            match.Combat.StartActions(match.Players.Values);
            return match.Players[1].Towers[0].TargetId;
        }
        int oldTarget = Target(baseline), actual = Target(candidate); Assert.Equal(oldTarget, actual);
        SquadState[] enemies = candidate.Combat.SquadSnapshots().Where(u => u.Faction == Faction.Skeletons).ToArray();
        Assert.Contains(enemies, s => s.Id == actual);
        // Change ONLY the salt in the existing selector, not actors, positions,
        // profiles, resources, sequence or target metric. Full identity is the
        // rejected pre-fix coupling; it is never used as a compatibility shortcut.
        var counterfactual = new CombatDecisionKey(seed, candidate.Configuration.Fingerprint, 1, 1, CombatActorKind.Tower, 1, 0, 0, CombatPurpose.Target);
        var topology = new SharedArena([1]);
        int fullIdentityTarget = enemies.OrderBy(s => topology.Board.Distance(topology.Tower(1, 0), s.Hex.Position.Cell))
            .ThenBy(s => SeededDecision.Mix(SeededDecision.State(counterfactual) ^ (ulong)s.Id)).First().Id;
        Assert.Equal(new DefenseProfile(500, 12, 48, 1, 0), candidate.Configuration.Tower(Building.ArrowTower, 1));
        output.WriteLine(JsonSerializer.Serialize(new
        {
            seed,
            EarliestActionTick = 0,
            Slot = 0,
            OldNamespaceTarget = oldTarget,
            ActualTarget = actual,
            FullIdentityCounterfactualTarget = fullIdentityTarget,
            LegacySalt = candidate.Configuration.ActorDecisionFingerprint.ToString(),
            FullIdentity = candidate.Configuration.Fingerprint.ToString(),
            Stock = candidate.Players[1].Resources,
            Profiles = candidate.Configuration.Towers
        }, WireJson.Options));
    }
    [Fact]
    public void SerializedRemotePurchaseUpgradeRetryAndCredentialResumePreserveRealPlotState()
    {
        using var authority = new AuthoritySession(AuthorityPolicy.PlayingHost, new Rules { StartingGold = 100, StartingWood = 100 });
        AdmissionResult guest = authority.Admit(2, WireJson.ProtocolVersion, "", 0); Assert.True(guest.Accepted);
        long hostSeq = 0, seq = 0; int peer = 2; ulong now = 0;
        void Host(string action) => Assert.True(authority.RequestLocal(JsonSerializer.Serialize(Command.FromSnapshot(authority.Snapshot(), ++hostSeq, action, 1), WireJson.Options)).Accepted);
        Command Request(string action, int slot = -1, Building type = Building.Empty)
            => Command.FromSnapshot(authority.Snapshot(), ++seq, action, 2, slot, type);
        CommandResult Send(Command command) => authority.Request(peer, JsonSerializer.Serialize(command, WireJson.Options), now += 1000)!;
        void Act(string action, int slot = -1, Building type = Building.Empty) => Assert.True(Send(Request(action, slot, type)).Accepted);
        Host("start"); Act("build", 0, Building.Stonecutter); Act("build", 1, Building.MetalMine);
        Host("ready"); Act("ready"); Host("ready"); Act("ready");
        Command stale = Request("buy-plot", 11), purchase = Request("buy-plot", 14);
        Assert.True(Send(purchase).Accepted); Assert.True(Send(purchase).Accepted); Assert.False(Send(stale with { Sequence = ++seq }).Accepted);
        Act("build", 14, Building.ArrowTower); Command upgrade = Request("upgrade-tower", 14, Building.BombardTower);
        Assert.True(Send(upgrade).Accepted);
        string accepted = JsonSerializer.Serialize(authority.Snapshot(), WireJson.Options);
        Assert.True(Send(upgrade).Accepted); Assert.Equal(accepted, JsonSerializer.Serialize(authority.Snapshot(), WireJson.Options));
        authority.Disconnect(peer); peer = 3;
        AdmissionResult resumed = authority.Admit(peer, WireJson.ProtocolVersion, guest.Credential, now + 1000);
        Assert.True(resumed.Accepted); Assert.Equal(2, resumed.PlayerId);
        MatchSnapshot loaded = SnapshotPayload.Decode(SnapshotPayload.Encode(resumed.State!))!;
        ArmyHomeState plot = loaded.Players[1].Army!.Homes[5];
        Assert.Equal(14, plot.PlotId); Assert.Equal(6, plot.Number); Assert.Equal(ArmyPlotStatus.TowerOccupied, plot.State);
        Assert.Equal(Building.BombardTower, plot.Plot.Type); Assert.Equal(2, plot.Plot.Level);
        Assert.Equal(plot.Plot.Type, loaded.Players[1].Towers.Single().Type);
        Assert.Equal(upgrade.ExpectedGeneration, plot.Plot.Generation);
    }
}
