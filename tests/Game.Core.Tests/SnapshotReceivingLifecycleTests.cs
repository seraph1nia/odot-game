using System.Text.Json;
using Game;
using Xunit;

namespace Game.Core.Tests;

public sealed class SnapshotReceivingLifecycleTests
{
    private static void Act(Match match, string action, int slot = -1, Building building = Building.Empty, UnitType type = UnitType.Swordsman, int city = 1)
    {
        CommandResult result = match.Apply(city, Command.FromSnapshot(match.Snapshot(), 1, action, city, slot, building, type));
        Assert.True(result.Accepted, result.Message);
    }
    private static void Ready(Match match)
    {
        foreach (int city in match.Players.Keys) Act(match, "ready", city: city);
    }
    private static MatchSnapshot Receive(Match match)
    {
        MatchSnapshot sent = match.Snapshot();
        MatchSnapshot received = SnapshotPayload.DecodeForReceiving(SnapshotPayload.Encode(sent));
        Assert.Equal(JsonSerializer.Serialize(sent, WireJson.Options), JsonSerializer.Serialize(received, WireJson.Options));
        return received;
    }

    private static MatchSnapshot ClearWithRetainedMove(Match match, bool atHomes = false)
    {
        var arena = new SharedArena([1], match.Players[1].Towers.Keys.Where(ArmyConfiguration.IsArmyPlot).Select(slot => (1, slot)));
        SquadState[] squads = match.Snapshot().Squads.Where(s => s.Owner == 1 && s.Hex.Lifecycle == UnitLifecycle.Alive).ToArray();
        Assert.Equal(2, squads.Length);
        int requested = arena.Deployment(1, 2);
        long tick = match.Tick;
        HexTransition transition = atHomes
            ? arena.Board.Transition(new(arena.Deployment(1, 0), 1), new(arena.Deployment(1, 1), 1))
            : arena.Board.Transition(new(17, 1), new(18, 1));
        foreach (int id in squads[0].Members)
            match.Combat.Seed(match.Combat.Inspect(id) with
            {
                Location = new(UnitLifecycle.Alive, transition.Source),
                Action = new CombatAction.Moving(1, transition.Destination, transition.Id, tick, tick + 30)
            });
        int enemy = match.Combat.Enemies()[0].Id;
        foreach (UnitState extra in match.Combat.Enemies().Where(u => u.Id != enemy)) match.Combat.Remove(extra.Id);
        int neighbor = arena.Board.Cell(requested).Neighbors.First(c => arena.CanOccupy(c) && c != transition.Source.Cell && c != transition.Destination.Cell);
        match.Combat.Seed(match.Combat.Inspect(enemy) with { Health = 1, Location = new(UnitLifecycle.Alive, new(neighbor, 1)) });
        int enemySquad = match.Combat.Read(enemy).SquadId;
        foreach (int id in squads[1].Members)
            match.Combat.Seed(match.Combat.Inspect(id) with
            {
                Location = new(UnitLifecycle.Alive, new(requested, 1)),
                Action = new CombatAction.Windup(1, 1, tick + 25, new(new(CombatTargetKind.Unit, enemySquad), tick, tick + 1))
            });
        Receive(match);
        match.Step();
        MatchSnapshot cleared = Receive(match);
        Assert.Equal(Phase.Building, cleared.Phase);
        return cleared;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PostClearDeploymentChangesOnlyHomeClaimUntilBodiesExpireAndThirdEndStartsCombat(bool preparationEdits)
    {
        using Match match = PaidCampaignFixture.Opening(players: 1);
        if (preparationEdits)
        {
            ClearWithRetainedMove(match);
            while (match.Combat.HasDeaths) { match.Step(); Receive(match); }
            Act(match, "build", 4, Building.Stonecutter);
            Act(match, "ready"); Act(match, "ready");
            Act(match, "sell", 4); Act(match, "build", 4, Building.TownHall);
            Act(match, "ready");
        }
        var arena = new SharedArena([1]);
        MatchSnapshot cleared = ClearWithRetainedMove(match, atHomes: preparationEdits);
        SquadState[] squads = cleared.Squads.Where(s => s.Owner == 1).ToArray();
        int mover = squads[0].Id, attacker = squads[1].Id;
        int requested = arena.Deployment(1, 2);
        Assert.Equal(preparationEdits ? 3 : 2, cleared.Wave);
        Assert.True(match.Combat.HasDeaths);
        SquadState retained = cleared.Squads.Single(s => s.Id == mover);
        Assert.Equal(UnitActionKind.Moving, retained.Hex.Action); Assert.NotNull(retained.Hex.FrozenTick);
        Assert.Equal(requested, cleared.Squads.Single(s => s.Id == attacker).Hex.Position.Cell);
        Act(match, "buy-home", ArmyConfiguration.PlotOffset + 2);
        CommandResult deployment = match.Apply(1, Command.FromSnapshot(match.Snapshot(), 2, "deploy-squad", 1) with { SquadId = mover, Cell = requested });
        Assert.True(deployment.Accepted, deployment.Message);
        MatchSnapshot deployed = Receive(match);
        Assert.Equal(requested, deployed.Squads.Single(s => s.Id == mover).DeploymentCell);
        Assert.Equal(retained.Hex, deployed.Squads.Single(s => s.Id == mover).Hex);
        Assert.Equal(cleared.Reservations.Positions, deployed.Reservations.Positions);
        Assert.Equal(cleared.Reservations.Transit, deployed.Reservations.Transit);
        Assert.All(deployed.Players[0].Soldiers.Where(u => u.SquadId == mover), u => Assert.Equal(2, u.Assignment!.Tile));
        Assert.False(match.Apply(1, Command.FromSnapshot(deployed, 3, "deploy-squad", 1) with { SquadId = attacker, Cell = requested }).Accepted);
        long productions = deployed.ProductionCount;
        void Retained(MatchSnapshot frame)
        {
            foreach (SquadState original in squads)
            {
                SquadState current = frame.Squads.Single(s => s.Id == original.Id);
                Assert.Equal(original.Hex, current.Hex);
                Assert.Equal((original.AttackSequence, original.ImpactTick, original.ReadyTick), (current.AttackSequence, current.ImpactTick, current.ReadyTick));
            }
            foreach (UnitState original in cleared.Players[0].Soldiers)
                if (frame.Players[0].Soldiers.FirstOrDefault(u => u.Id == original.Id) is { } current && current.SquadId == original.SquadId)
                    Assert.Equal(original.Hex, current.Hex);
            Assert.Equal(cleared.Reservations.Positions, frame.Reservations.Positions);
            Assert.Equal(cleared.Reservations.Transit, frame.Reservations.Transit);
        }
        void Edit(string action, int slot = -1, int unit = 0, int squad = 0, int cell = 0, Building building = Building.Empty)
        {
            CommandResult result = match.Apply(1, Command.FromSnapshot(match.Snapshot(), 10, action, 1, slot, building) with { UnitId = unit, SquadId = squad, Cell = cell });
            Assert.True(result.Accepted, result.Message);
            Retained(Receive(match));
        }
        int stored = 0;
        if (preparationEdits)
        {
            Edit("regroup-member", unit: squads[0].Members[0], squad: mover);
            Edit("retire", unit: squads[0].Members[^1]); Edit("retire", unit: squads[1].Members[^1]);
            Edit("recruit", 1, squad: mover);
            int reinforcement = match.Players[1].Soldiers[^1].Id;
            Assert.Equal(UnitLifecycle.Reserve, match.Combat.Read(reinforcement).Hex!.Lifecycle);
            Edit("recruit", 1, squad: -1);
            int recruit = match.Players[1].Soldiers[^1].Id;
            Assert.Equal(UnitLifecycle.Queued, match.Combat.Read(recruit).Hex!.Lifecycle);
            Edit("regroup-member", unit: recruit, squad: attacker);
            Assert.Equal(UnitLifecycle.Reserve, match.Combat.Read(recruit).Hex!.Lifecycle);
            Edit("store", 4, unit: recruit);
            Assert.Equal(UnitLifecycle.Stored, match.Combat.Read(recruit).Hex!.Lifecycle);
            Edit("send", 4, unit: recruit, squad: attacker);
            Assert.Equal(attacker, match.Combat.Read(recruit).SquadId);
            Assert.Equal(UnitLifecycle.Reserve, match.Combat.Read(recruit).Hex!.Lifecycle);
            Edit("regroup-member", unit: reinforcement, squad: -1);
            Assert.Equal(UnitLifecycle.Queued, match.Combat.Read(reinforcement).Hex!.Lifecycle);
            Edit("store", 4, unit: reinforcement); stored = reinforcement;
            Edit("build", ArmyConfiguration.PlotOffset, building: Building.ArrowTower);
            MatchSnapshot purchased = Receive(match);
            Assert.Equal(Building.ArrowTower, purchased.Players[0].Army!.Homes[0].Plot.Type);
            Assert.Contains(purchased.Players[0].Towers, t => t.Slot == ArmyConfiguration.PlotOffset);
            Assert.DoesNotContain(purchased.Arena!.Footprints, f => f.Slot == ArmyConfiguration.PlotOffset);
            Assert.DoesNotContain(purchased.Structures, s => s.Slot == ArmyConfiguration.PlotOffset);
            Edit("sell", ArmyConfiguration.PlotOffset);
            Act(match, "ready"); Retained(Receive(match));
            Edit("deploy-squad", squad: attacker, cell: arena.Deployment(1, 0));
            Edit("build", ArmyConfiguration.PlotOffset + 1, building: Building.ArrowTower);
            Assert.DoesNotContain(Receive(match).Arena!.Footprints, f => f.Slot == ArmyConfiguration.PlotOffset + 1);
        }
        for (int gate = preparationEdits ? 1 : 0; gate < 3; gate++) { Act(match, "ready"); Retained(Receive(match)); }
        Assert.Equal(Phase.Preparation, match.Phase);
        Assert.Equal(productions + 2, match.Snapshot().ProductionCount);
        for (int tick = 0; tick < 120 && match.Phase != Phase.Combat; tick++)
        {
            match.Step(); MatchSnapshot frame = Receive(match);
            if (match.Combat.HasDeaths) Retained(frame);
        }
        MatchSnapshot entered = Receive(match);
        Assert.False(match.Combat.HasDeaths); Assert.Equal(Phase.Combat, entered.Phase);
        Assert.Equal(productions + 3, entered.ProductionCount);
        Assert.Equal(requested, entered.Squads.Single(s => s.Id == mover).Hex.Position.Cell);
        Assert.Equal(arena.Deployment(1, preparationEdits ? 0 : 1), entered.Squads.Single(s => s.Id == attacker).Hex.Position.Cell);
        if (preparationEdits)
        {
            Assert.Equal(UnitLifecycle.Stored, entered.Players[0].Soldiers.Single(u => u.Id == stored).Hex!.Lifecycle);
            Assert.Contains(entered.Arena!.Footprints, f => f.Slot == ArmyConfiguration.PlotOffset + 1);
            Assert.Contains(entered.Structures, s => s.Slot == ArmyConfiguration.PlotOffset + 1 && s.Active);
            MatchSnapshot nextClear = ClearWithRetainedMove(match);
            SquadState[] retainedActors = nextClear.Squads.Where(s => s.Owner == 1 && s.Hex.Lifecycle == UnitLifecycle.Alive).ToArray();
            long nextProductions = nextClear.ProductionCount;
            Act(match, "sell", ArmyConfiguration.PlotOffset + 1);
            MatchSnapshot sold = Receive(match);
            Assert.Equal(Building.Empty, sold.Players[0].Army!.Homes[1].Plot.Type);
            Assert.Contains(sold.Arena!.Footprints, f => f.Slot == ArmyConfiguration.PlotOffset + 1);
            for (int gate = 0; gate < 3; gate++) { Act(match, "ready"); Receive(match); }
            Assert.Equal(Phase.Preparation, match.Phase);
            for (int tick = 0; tick < 120 && match.Phase != Phase.Combat; tick++)
            {
                MatchSnapshot frame = Receive(match);
                foreach (SquadState actor in retainedActors) Assert.Equal(actor.Hex, frame.Squads.Single(s => s.Id == actor.Id).Hex);
                match.Step();
            }
            MatchSnapshot resumed = Receive(match);
            Assert.Equal(Phase.Combat, resumed.Phase); Assert.Equal(4, resumed.Wave);
            Assert.Equal(nextProductions + 3, resumed.ProductionCount);
            Assert.DoesNotContain(resumed.Arena!.Footprints, f => f.Slot == ArmyConfiguration.PlotOffset + 1);
            Assert.DoesNotContain(resumed.Structures, s => s.Slot == ArmyConfiguration.PlotOffset + 1);
        }
    }

    [Theory]
    [InlineData("regroup")]
    [InlineData("retire-reinforced")]
    [InlineData("retire-returned")]
    [InlineData("retire-regrouped")]
    public void RemovingLastFrozenParticipantReleasesClaimsWithoutDeployingItsNewGroup(string origin)
    {
        using Match match = PaidCampaignFixture.Opening(players: 1);
        if (origin == "retire-returned")
        {
            ClearWithRetainedMove(match);
            while (match.Combat.HasDeaths) match.Step();
            Act(match, "build", 4, Building.Stonecutter); Act(match, "ready"); Act(match, "ready");
            Act(match, "sell", 4); Act(match, "build", 4, Building.TownHall); Act(match, "ready");
        }
        MatchSnapshot cleared = ClearWithRetainedMove(match, atHomes: true);
        SquadState mover = cleared.Squads.Single(s => s.Owner == 1 && s.Hex.Action == UnitActionKind.Moving);
        SquadState other = cleared.Squads.Single(s => s.Owner == 1 && s.Id != mover.Id);
        var arena = new SharedArena([1]);
        Act(match, "buy-home", ArmyConfiguration.PlotOffset + 2);
        void Edit(string action, int unit = 0, int squad = 0, int cell = 0, int slot = -1)
        {
            CommandResult result = match.Apply(1, Command.FromSnapshot(match.Snapshot(), 20, action, 1, slot) with { UnitId = unit, SquadId = squad, Cell = cell });
            Assert.True(result.Accepted, result.Message);
            Assert.Equal(other.Hex, Receive(match).Squads.Single(s => s.Id == other.Id).Hex);
        }
        Edit("deploy-squad", squad: mover.Id, cell: arena.Deployment(1, 2));
        if (origin == "retire-returned") Edit("store", unit: mover.Members[1], slot: 4);
        foreach (int id in mover.Members.Skip(origin == "retire-returned" ? 2 : 1)) Edit("retire", unit: id);
        int recruit;
        if (origin == "retire-returned")
        {
            Edit("send", unit: mover.Members[1], squad: mover.Id, slot: 4); recruit = mover.Members[1];
        }
        else if (origin == "retire-regrouped")
        {
            Edit("regroup-member", unit: other.Members[0], squad: mover.Id); recruit = other.Members[0];
        }
        else
        {
            Edit("recruit", squad: mover.Id, slot: 1); recruit = match.Players[1].Soldiers[^1].Id;
        }
        Edit(origin == "regroup" ? "regroup-member" : "retire", unit: mover.Members[0], squad: -1);
        MatchSnapshot edited = Receive(match);
        SquadState reserve = edited.Squads.Single(s => s.Id == mover.Id);
        Assert.Equal(UnitLifecycle.Reserve, reserve.Hex.Lifecycle);
        Assert.False(reserve.Hex.HoldsTransit);
        Assert.Equal(UnitLifecycle.Reserve, edited.Players[0].Soldiers.Single(u => u.Id == recruit).Hex!.Lifecycle);
        if (origin == "regroup") Assert.Equal(UnitLifecycle.Queued, edited.Players[0].Soldiers.Single(u => u.Id == mover.Members[0]).Hex!.Lifecycle);
        else Assert.DoesNotContain(edited.Players[0].Soldiers, u => u.Id == mover.Members[0]);
        Assert.DoesNotContain(edited.Reservations.Positions, p => p.UnitId == mover.Id);
        Assert.DoesNotContain(edited.Reservations.Transit, p => p.UnitId == mover.Id);
        long productions = edited.ProductionCount;
        for (int gate = 0; gate < 3; gate++) Act(match, "ready");
        for (int tick = 0; tick < 120 && match.Phase != Phase.Combat; tick++) { match.Step(); Receive(match); }
        MatchSnapshot entered = Receive(match);
        Assert.Equal(Phase.Combat, entered.Phase);
        Assert.Equal(productions + 3, entered.ProductionCount);
        Assert.Equal(origin == "regroup" ? 3 : 2, entered.Squads.Count(s => s.Owner == 1 && s.Hex.Lifecycle == UnitLifecycle.Alive));
        Assert.Equal(arena.Deployment(1, 2), entered.Squads.Single(s => s.Id == mover.Id).Hex.Position.Cell);
    }

    [Fact]
    public void RetiringLastParticipantOfPaidUnfedSquadImmediatelyPublishesValidReleasedClaims()
    {
        using var match = new Match(new Rules
        {
            StartingGold = 100,
            StartingWood = 100,
            FarmOutput = 1,
            DefenderDamage = 0,
            Campaign = new([new(1, false, [new(UnitType.Swordsman, 1)], default), new(2, false, [new(UnitType.Swordsman, 1)], default)])
        }, combatSeed: 1);
        match.Join(); Act(match, "start"); Act(match, "build", 0, Building.Barracks); Act(match, "build", 2, Building.MetalMine);
        Act(match, "ready"); Act(match, "ready"); Act(match, "build", 4, Building.Farm);
        for (int i = 0; i < 3; i++) Act(match, "recruit", 0);
        Act(match, "ready");
        MatchSnapshot entered = Receive(match);
        int participant = Assert.Single(entered.Players[0].LastUpkeep!.Participating);
        Assert.Equal(2, entered.Players[0].LastUpkeep!.Unfed.Length);
        int enemy = Assert.Single(entered.Enemies).Id;
        int squadId = match.Combat.Read(participant).SquadId, enemySquad = match.Combat.Read(enemy).SquadId;
        match.Combat.Seed(match.Combat.Inspect(enemy) with { Health = 1, Location = new(UnitLifecycle.Alive, new(18, 1)) });
        foreach (UnitState member in entered.Players[0].Soldiers.OrderBy(u => u.Id == participant))
            match.Combat.Seed(match.Combat.Inspect(member.Id) with
            {
                Location = new(member.Hex!.Lifecycle, new(17, 1)),
                Action = new CombatAction.Windup(1, 1, 25, new(new(CombatTargetKind.Unit, enemySquad), 0, 1))
            });
        match.Step(); Assert.Equal(Phase.Building, match.Phase); Assert.True(match.Combat.HasDeaths);
        Receive(match);
        CommandResult retired = match.Apply(1, Command.FromSnapshot(match.Snapshot(), 2, "retire", 1) with { UnitId = participant });
        Assert.True(retired.Accepted, retired.Message);
        MatchSnapshot received = Receive(match);
        Assert.Equal(UnitLifecycle.Reserve, received.Squads.Single(s => s.Id == squadId).Hex.Lifecycle);
        Assert.Empty(received.Squads.Single(s => s.Id == squadId).ParticipatingMembers);
        Assert.DoesNotContain(received.Reservations.Positions, p => p.UnitId == squadId);
        Assert.DoesNotContain(received.Reservations.Transit, p => p.UnitId == squadId);
        while (match.Combat.HasDeaths) { match.Step(); Receive(match); }
        Assert.Equal(UnitLifecycle.Alive, Receive(match).Squads.Single(s => s.Id == squadId).Hex.Lifecycle);
    }

    [Fact]
    public void PaidPartialSquadRetainsReservesWhenItsLastParticipantDiesAndBodyExpires()
    {
        using var match = new Match(new Rules
        {
            StartingGold = 100,
            StartingWood = 100,
            SoldierHealth = 1,
            FarmOutput = 1,
            DefenderDamage = 0,
            Campaign = new([new(1, false, [new(UnitType.Crossbowman, 1)], default)])
        }, combatSeed: 1);
        match.Join(); Act(match, "start"); Act(match, "build", 0, Building.Barracks); Act(match, "build", 2, Building.MetalMine);
        Act(match, "ready"); Act(match, "ready"); Act(match, "build", 4, Building.Farm);
        for (int i = 0; i < 3; i++) Act(match, "recruit", 0);
        UnitState[] purchased = Receive(match).Players[0].Soldiers;
        Act(match, "ready");
        MatchSnapshot entered = Receive(match);
        int participant = Assert.Single(entered.Players[0].LastUpkeep!.Participating);
        int[] reserves = entered.Players[0].LastUpkeep!.Unfed;
        Assert.Equal(2, reserves.Length);
        SquadState original = entered.Squads.Single(s => s.Owner == 1);
        MatchSnapshot frame = entered;
        for (int i = 0; i < 1000 && !frame.DyingBodies.Any(b => b.Id == participant); i++)
        {
            Assert.Equal(Phase.Combat, match.Phase); match.Step(); frame = Receive(match);
        }
        UnitState body = Assert.Single(frame.DyingBodies, b => b.Id == participant);
        SquadState fallen = frame.Squads.Single(s => s.Id == original.Id);
        Assert.Equal(UnitLifecycle.Dying, fallen.Hex.Lifecycle); Assert.Empty(fallen.ParticipatingMembers);
        Assert.Equal(reserves, fallen.Members);
        Assert.DoesNotContain(frame.Reservations.Positions, p => p.UnitId == fallen.Id);
        Assert.DoesNotContain(frame.Reservations.Transit, p => p.UnitId == fallen.Id);
        foreach (int id in reserves)
        {
            UnitState reserve = frame.Players[0].Soldiers.Single(u => u.Id == id);
            UnitState bought = purchased.Single(u => u.Id == id);
            Assert.Equal(UnitLifecycle.Reserve, reserve.Hex!.Lifecycle); Assert.False(reserve.PendingImpact);
            Assert.Equal((bought.Health, bought.Level, bought.Experience, bought.Assignment, bought.SquadId, bought.FormationSlot),
                (reserve.Health, reserve.Level, reserve.Experience, reserve.Assignment, reserve.SquadId, reserve.FormationSlot));
        }
        while (match.Tick < body.Hex!.DeathEndTick)
        {
            Assert.Equal(body.Hex, frame.DyingBodies.Single(b => b.Id == body.Id).Hex);
            match.Step(); frame = Receive(match);
        }
        Assert.DoesNotContain(frame.DyingBodies, b => b.Id == participant);
        Assert.Equal(reserves, frame.Players[0].Soldiers.Select(u => u.Id));
        Assert.DoesNotContain(frame.Reservations.Positions, p => p.UnitId == original.Id);
    }

    [Fact]
    public void PaidPartialSquadKeepsCommittedWindupAndLatchedAimAfterAnotherActorKillsItsTarget()
    {
        using var match = new Match(new Rules
        {
            StartingGold = 100,
            StartingWood = 100,
            SoldierHealth = 1,
            SoldierDamage = 0,
            BerserkerHealth = 400,
            BerserkerDamage = 0,
            RangedDamage = 50,
            FarmOutput = 1,
            StoneOutput = 2,
            ClothOutput = 5,
            MageUpkeep = 1,
            DefenderDamage = 0,
            Combat = new() { MageHexRange = 4, MageWindupTicks = 120, MageRecoveryTicks = 30 },
            Campaign = new([new(1, false, [new(UnitType.Swordsman, 1), new(UnitType.Berserker, 1)], default)])
        }, combatSeed: 1);
        match.Join(); match.Join(); Act(match, "start"); Act(match, "build", 0, Building.ArcheryRange);
        Act(match, "build", 2, Building.MetalMine); Act(match, "build", 3, Building.Stonecutter); Act(match, "build", 4, Building.Weaver);
        Ready(match); Act(match, "buy-plot", 5); Act(match, "build", 5, Building.Farm); Ready(match);
        Act(match, "build", 1, Building.Arcanum); Act(match, "recruit", 0, type: UnitType.Crossbowman);
        for (int i = 0; i < 3; i++) Act(match, "recruit", 1, type: UnitType.Mage);
        Ready(match);
        MatchSnapshot frame = Receive(match);
        SquadState mages = frame.Squads.Single(s => s.Owner == 1 && s.Type == UnitType.Mage);
        Assert.Single(mages.ParticipatingMembers); Assert.Equal(3, mages.Members.Length);
        UnitState? latched = null;
        for (int i = 0; i < 1200 && latched is null; i++)
        {
            Assert.Equal(Phase.Combat, match.Phase); match.Step(); frame = Receive(match);
            SquadState actor = frame.Squads.Single(s => s.Id == mages.Id);
            if (actor.Hex.Action != UnitActionKind.Windup || actor.TargetKind != SquadTargetKind.Squad
                || frame.Squads.Any(s => s.Id == actor.TargetId && s.Hex.Lifecycle == UnitLifecycle.Alive)) continue;
            latched = frame.Players[0].Soldiers.Single(u => actor.ParticipatingMembers.Contains(u.Id));
        }
        Assert.NotNull(latched); Assert.True(latched.PendingImpact); Assert.NotNull(latched.Hex!.FrozenAim);
        SquadState committed = frame.Squads.Single(s => s.Id == mages.Id);
        Assert.All(frame.Players[0].Soldiers.Where(u => u.SquadId == mages.Id && u.Hex!.Lifecycle == UnitLifecycle.Reserve), reserve =>
        {
            Assert.Equal(UnitActionKind.Windup, reserve.Hex!.Action); Assert.Equal(0, reserve.TargetId);
            Assert.Equal(committed.AttackSequence, reserve.AttackSequence); Assert.False(reserve.PendingImpact);
        });
        Assert.Contains(frame.CombatEvents, e => e.Type == CombatEventType.Death && e.Unit?.Id == latched.TargetId);
        while (match.Tick < committed.ImpactTick)
        {
            UnitState pending = frame.Players[0].Soldiers.Single(u => u.Id == latched.Id);
            Assert.Equal(latched.TargetId, pending.TargetId); Assert.Equal(latched.Hex.FrozenAim, pending.Hex!.FrozenAim);
            Assert.Equal(committed.AttackSequence, pending.AttackSequence);
            match.Step(); frame = Receive(match);
        }
        UnitState recovered = frame.Players[0].Soldiers.Single(u => u.Id == latched.Id);
        Assert.Equal(UnitActionKind.Recovery, recovered.Hex!.Action); Assert.False(recovered.PendingImpact);
        Assert.Equal(committed.ReadyTick, recovered.ReadyTick); Assert.Equal(latched.TargetId, recovered.TargetId);
        Assert.Equal(latched.Hex.FrozenAim, recovered.Hex.FrozenAim);
        Assert.Contains(frame.CombatEvents, e => e.Unit?.Id == latched.Id && e.Type == CombatEventType.Impact
            && e.Unit.AttackSequence == committed.AttackSequence && !e.Landed);
    }
}
