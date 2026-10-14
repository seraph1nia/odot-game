using System.Text.Json;
using Xunit;

namespace Game.Core.Tests;

// Tier A: real accepted commands and detached/serialized projections catch
// home/identity reinterpretation and optimistic free reinforcement. Older DTO
// goldens do not express squads. Deterministic seed, no engine/transport process.
public sealed class SharedStateTests
{
    [Fact]
    public void DeploymentMovesTheHomeClaimSoNewSquadsCannotStackOnItsDestination()
    {
        using var match = new Match(new() { StartingGold = 100, StartingWood = 100 }, combatSeed: 3);
        City city = match.Join()!; city.Metal = 100;
        long sequence = 0;
        CommandResult Apply(string action, int slot = -1, Building building = Building.Empty, int squadId = 0, int cell = 0)
            => match.Apply(city.Id, Command.FromSnapshot(match.Snapshot(), ++sequence, action, city.Id, slot, building, squadId: squadId, cell: cell));
        Assert.True(Apply("start").Accepted); Assert.True(Apply("build", 0, Building.Barracks).Accepted); Assert.True(Apply("recruit", 0).Accepted);
        SquadState original = Assert.Single(match.Snapshot().Squads);
        var arena = new SharedArena([city.Id]);
        Assert.True(Apply("deploy-squad", squadId: original.Id, cell: arena.Deployment(city.Id, 1)).Accepted);
        Assert.Equal(1, Assert.Single(city.Soldiers).Assignment!.Tile);
        Assert.True(Apply("recruit", 0, squadId: -1).Accepted);
        MatchSnapshot state = match.Snapshot();
        Assert.Equal(2, state.Squads.Length);
        Assert.Equal(2, state.Squads.Select(s => s.Hex.Position.Cell).Distinct().Count());
        Assert.Equal(0, city.Soldiers.Last(u => u.SquadId != original.Id).Assignment!.Tile);
        Assert.False(Apply("deploy-squad", squadId: original.Id, cell: arena.Deployment(city.Id, 0)).Accepted);
        SnapshotCompatibility.RequireSharedArena(state);
    }

    [Fact]
    public void PaidReinforcementPreservesInjuredVeteranAndStableGroupFormationIdentity()
    {
        using var match = new Match(new() { StartingGold = 100, StartingWood = 100 }, combatSeed: 6);
        City city = match.Join()!; city.Metal = 100;
        long sequence = 0;
        void Apply(string action, int squadId = 0)
        {
            CommandResult result = match.Apply(city.Id, Command.FromSnapshot(match.Snapshot(), ++sequence, action, city.Id, 0, Building.Barracks, squadId: squadId));
            Assert.True(result.Accepted, result.Message);
        }
        Apply("start"); Apply("build"); Apply("recruit");
        UnitState veteran = Assert.Single(city.Soldiers);
        match.Combat.Seed(match.Combat.Inspect(veteran.Id) with { Health = HealthPoints.FromWhole(10), Experience = 3 });
        Apply("upgrade"); int metal = city.Metal;
        Apply("recruit", veteran.SquadId);
        UnitState[] members = city.Soldiers.ToArray();
        Assert.Equal(2, members.Length); Assert.True(city.Metal < metal);
        Assert.All(members, member => Assert.Equal(veteran.SquadId, member.SquadId));
        Assert.Equal(HealthPoints.FromWhole(10), members.Single(m => m.Id == veteran.Id).Health);
        Assert.Equal(1, members.Single(m => m.Id == veteran.Id).Level);
        Assert.Equal(2, members.Single(m => m.Id != veteran.Id).Level);
        Assert.Equal(3, members.Single(m => m.Id == veteran.Id).Experience);
        Assert.Equal(0, members.Single(m => m.Id != veteran.Id).Experience);
        Assert.Equal(2, members.Select(m => m.FormationSlot).Distinct().Count());
        Assert.Equal(veteran.FormationSlot, members.Single(m => m.Id == veteran.Id).FormationSlot);
        MatchSnapshot rebuilt = JsonSerializer.Deserialize<MatchSnapshot>(JsonSerializer.Serialize(match.Snapshot(), WireJson.Options), WireJson.Options)!;
        SnapshotCompatibility.RequireSharedArena(rebuilt);
        Assert.Equal(members.Select(m => (m.Id, m.SquadId, m.FormationSlot, m.Level, m.Health, m.Experience)), rebuilt.Players[0].Soldiers.Select(m => (m.Id, m.SquadId, m.FormationSlot, m.Level, m.Health, m.Experience)));
    }

    [Fact]
    public void ArbitraryPurchasedPlotsAndPhysicalTowersShareExclusiveSquadDeploymentClaims()
    {
        // Tier A landed tower integration: total purchase count must not grant an
        // unpurchased cell or let a squad occupy a physical army-plot weapon.
        using var match = new Match(new() { StartingGold = 200, StartingWood = 200 }, combatSeed: 6);
        City city = match.Join()!; match.Join(); city.Metal = 100;
        long sequence = 0;
        CommandResult Apply(string action, int slot = -1, Building type = Building.Empty, int squadId = 0, int cell = 0)
            => match.Apply(1, Command.FromSnapshot(match.Snapshot(), ++sequence, action, 1, slot, type, squadId: squadId, cell: cell));
        Assert.True(Apply("start").Accepted); Assert.True(Apply("build", 0, Building.Barracks).Accepted);
        Assert.True(Apply("build", 10, Building.ArrowTower).Accepted);
        Assert.True(Apply("buy-plot", 14).Accepted);
        for (int member = 0; member < 4; member++) Assert.True(Apply("recruit", 0).Accepted);
        var arena = new SharedArena([1, 2]);
        UnitState high = city.Soldiers.Single(u => u.Assignment!.Tile == 5);
        Assert.Equal(arena.Deployment(1, 5), high.Hex!.Position.Cell);
        Assert.False(Apply("deploy-squad", squadId: high.SquadId, cell: arena.Deployment(1, 2)).Accepted);
        Assert.False(Apply("deploy-squad", squadId: high.SquadId, cell: arena.Deployment(1, 1)).Accepted);
        StructureState weapon = Assert.Single(match.Snapshot().Structures, s => s.Owner == 1 && s.Slot == 10);
        Assert.Equal(arena.Deployment(1, 1), weapon.Cell);
        SnapshotCompatibility.RequireSharedArena(match.Snapshot());
        Assert.True(Apply("sell", 10).Accepted);
        Assert.True(Apply("deploy-squad", squadId: high.SquadId, cell: arena.Deployment(1, 1)).Accepted);
        Assert.Equal(1, city.Soldiers.Single(u => u.Id == high.Id).Assignment!.Tile);
        Assert.True(Apply("build", 14, Building.ArrowTower).Accepted);
        SnapshotCompatibility.RequireSharedArena(match.Snapshot());
        Assert.Equal(arena.Deployment(1, 5), Assert.Single(match.Snapshot().Structures, s => s.Owner == 1 && s.Slot == 14).Cell);
    }

    [Fact]
    public void LegacyOrOverlappingSnapshotsAreRefusedRatherThanInventingSquads()
    {
        using var match = new Match(combatSeed: 1); match.Join();
        MatchSnapshot state = match.Snapshot(); SnapshotCompatibility.RequireSharedArena(state);
        Assert.Contains("Legacy combat snapshot refused", Assert.Throws<ArgumentException>(() => SnapshotCompatibility.RequireSharedArena(state with { Arena = null })).Message, StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => SnapshotCompatibility.RequireSharedArena(state with { CombatRulesVersion = 5 }));
        Assert.Throws<ArgumentException>(() => new CombatConfiguration(new() { Combat = new() { RulesVersion = 5 } }));
        using var altered = new Match(new() { Arena = new() { MageSecondaryDamage = 0 } }, combatSeed: 1);
        Assert.NotEqual(match.ConfigurationFingerprint, altered.ConfigurationFingerprint);
        Assert.Equal(63, state.Rules.Combat.Board.Cells.Length);
        Assert.Equal(1, state.Rules.Combat.Board.Capacity);
    }
}
