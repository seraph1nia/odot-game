using System.Numerics;
using System.Text.Json;
using Game;
using Xunit;

namespace Game.Core.Tests;

// Tier A admission: the former native preview test checked allocation count and
// distinct figures, not grouping/entry/funding parity. Exercise actual Match entry
// without stepping simulation, compare ordered identities and pure projection.
// Deterministic numerical supplies are explicit, not an earned campaign witness.
// Cost: 1..4-owner setup/three ENDs, no engine/transport/rendering resources.
public sealed class BattleDeploymentTests
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    [InlineData(3, 1)]
    [InlineData(4, 1)]
    [InlineData(2, 2)]
    public void ForecastEqualsActualCombatEntryWithoutChangingAuthority(int owners, int catalog)
    {
        CampaignDefinition campaign = catalog switch
        {
            1 => new([new(1, false, [new(UnitType.Swordsman, 1, 25), new(UnitType.Mage, 2, 4), new(UnitType.Swordsman, 3, 2)], default)]),
            2 => new([new(1, true, [new(UnitType.Swordsman, 3, IsBoss: true)], default)]),
            _ => CampaignDefinition.Default()
        };
        using var match = new Match(new() { StartingGold = 100, StartingWood = 100, Campaign = campaign }, combatSeed: 123);
        for (int i = 0; i < owners; i++) match.Join();
        long sequence = 0;
        void Act(int owner, string action, int slot = -1, Building building = Building.Empty, int squad = 0, int cell = 0)
        {
            CommandResult result = match.Apply(owner, Command.FromSnapshot(match.Snapshot(), ++sequence, action, owner, slot, building, squadId: squad, cell: cell));
            Assert.True(result.Accepted, $"{action} P{owner}: {result.Message}");
        }
        Act(1, "start");
        foreach (City city in match.Players.Values)
        {
            city.Metal = 100; city.Stone = 100; city.Food = 2;
            Act(city.Id, "build", 0, Building.Barracks);
            for (int n = 0; n < 4; n++) Act(city.Id, "recruit", 0);
            Act(city.Id, "build", 2, Building.TownHall);
            Assert.True(match.Apply(city.Id, Command.FromSnapshot(match.Snapshot(), ++sequence, "store", city.Id, 2, unitId: city.Soldiers[^1].Id)).Accepted);
            UnitState first = city.Soldiers[0];
            Act(city.Id, "deploy-squad", squad: first.SquadId, cell: match.Snapshot().Arena!.Sectors.Single(s => s.Owner == city.Id).DeploymentCells[1]);
        }
        for (int turn = 0; turn < 2; turn++) foreach (int owner in match.Players.Keys) Act(owner, "ready");
        MatchSnapshot preparation = SessionFlow.RoundTrip(match.Snapshot());
        UnitState[] preview = BattleDeployment.Preview(preparation);
        Assert.Equal(owners * 2, preview.Count(u => u.Id > 0 && u.Deployed));
        // Small disconfirming counterfactual: just add final-production Farm.
        // The projection must update funded IDs immediately, without payment.
        Act(1, "build", 1, Building.Farm);
        match.Players[1].Research = new(Points: 30); // Explicit research supply; purchase is ordinary authority command.
        Assert.True(match.Apply(1, Command.FromSnapshot(match.Snapshot(), ++sequence, "research-tech", 1, technology: TechnologyId.MeleeFoundation)).Accepted);
        Assert.NotEqual(preparation.Players[0].Soldiers[0].Profile, BattleDeployment.Preview(match.Snapshot()).First().Profile);
        MatchSnapshot final = SessionFlow.RoundTrip(match.Snapshot());
        preview = BattleDeployment.Preview(final);
        Assert.Equal(3, preview.Count(u => u.Owner == 1 && u.Deployed));
        string unchanged = JsonSerializer.Serialize(final, WireJson.Options);
        Assert.Equal(JsonSerializer.Serialize(preview, WireJson.Options), JsonSerializer.Serialize(BattleDeployment.Preview(final), WireJson.Options));
        Assert.Equal(unchanged, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
        Assert.Empty(match.Enemies); // No forecast-only live ID, claim or damage.
        foreach (int owner in match.Players.Keys) Act(owner, "ready");
        MatchSnapshot entered = SessionFlow.RoundTrip(match.Snapshot());
        Assert.Equal(final.Tick, entered.Tick);
        Assert.Equal(Phase.Combat, entered.Phase);
        Assert.Equal(JsonSerializer.Serialize(final.Arena), JsonSerializer.Serialize(entered.Arena));
        var board = new HexBoard(entered.Arena!.Board);
        UnitState[] actual = entered.Players.SelectMany(c => c.Soldiers).Concat(entered.Enemies).ToArray();
        Assert.Equal(preview.Length, actual.Length);
        UnitState[] forecastEnemies = preview.Where(u => u.Id < 0).ToArray();
        Assert.All(forecastEnemies, u => Assert.Contains(u.Id, final.NextWave.Select(e => e.Id)));
        for (int index = 0; index < actual.Length; index++)
        {
            UnitState live = actual[index];
            UnitState forecast = live.Faction == Faction.Skeletons ? forecastEnemies[Array.IndexOf(entered.Enemies, live)] : preview.Single(u => u.Id == live.Id);
            Assert.Equal((forecast.Type, forecast.Origin, forecast.Destination, forecast.Owner, forecast.Level, forecast.IsBoss, forecast.Health, forecast.Profile),
                (live.Type, live.Origin, live.Destination, live.Owner, live.Level, live.IsBoss, live.Health, live.Profile));
            Assert.Equal(forecast.Deployed, live.Deployed);
            Assert.Equal(forecast.FormationSlot, live.FormationSlot);
            Assert.Equal(forecast.Hex!.Lifecycle, live.Hex!.Lifecycle);
            if (!live.Deployed) continue;
            Assert.Equal(forecast.Hex!.Position, live.Hex!.Position);
            Assert.Equal(SharedCombatProjection.Position(board, forecast.Hex, forecast.FormationSlot, final.Rules.Arena.SquadCapacity, forecast.IsBoss, final.Tick),
                SharedCombatProjection.Position(board, live.Hex, live.FormationSlot, entered.Rules.Arena.SquadCapacity, live.IsBoss, entered.Tick));
        }
        // No Keep exception. A controlled prior-wave Watchpost ruin projects
        // its real entry reset, while Keep wounds and invulnerable weapons stay.
        MatchSnapshot ruined = final with { Structures = final.Structures.Select(s => s.Kind == ArenaStructureKind.Watchpost ? s with { Health = 0, DestroyedTick = 1 } : s).ToArray() };
        Assert.Equal(JsonSerializer.Serialize(BattleDeployment.Structures(ruined)), JsonSerializer.Serialize(entered.Structures));
        Assert.Empty(BattleDeployment.Preview(entered));
    }

    [Fact]
    public void FallenOriginRedistributionRetainsForecastOrderAndActualEntryCells()
    {
        using var match = new Match(combatSeed: 9); match.Join(); match.Join(); match.Join();
        long sequence = 0;
        void Ready() => Assert.True(match.Apply(1, Command.FromSnapshot(match.Snapshot(), ++sequence, "ready", 1)).Accepted);
        Assert.True(match.Apply(1, Command.FromSnapshot(match.Snapshot(), ++sequence, "start", 1)).Accepted);
        // Controlled wave boundary, not earned defeat/campaign evidence.
        foreach (City city in match.Players.Values.Skip(1)) city.Connected = false;
        Ready(); Ready(); Ready();
        foreach (UnitState enemy in match.Enemies.ToArray()) match.Combat.Remove(enemy.Id);
        match.Players[3].Health = 0; match.Step();
        Assert.True(match.Players[3].Eliminated);
        Ready(); Ready();
        MatchSnapshot prepared = match.Snapshot();
        UnitState[] expected = BattleDeployment.Preview(prepared).Where(u => u.Id < 0).ToArray();
        Assert.Contains(expected, u => u.Origin == 3 && u.Destination != 3);
        Assert.Equal(prepared.NextWave.Select(u => u.Id), expected.Select(u => u.Id));
        Ready();
        Assert.Equal(prepared.Tick, match.Tick);
        Assert.Equal(expected.Select(u => (u.Type, u.Origin, u.Destination, u.FormationSlot, u.Deployed, Cell: u.Deployed ? u.Hex!.Position.Cell : 0)),
            match.Enemies.Select(u => (u.Type, u.Origin, u.Destination, u.FormationSlot, u.Deployed, Cell: u.Deployed ? u.Hex!.Position.Cell : 0)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void PointyVerticesHorizontalRowsAndPickingPreserveEveryLogicalNeighbor(int owners)
    {
        var arena = new SharedArena(Enumerable.Range(1, owners));
        HexBoard board = arena.Board;
        float diameter = SharedCombatProjection.HalfWidth * 2;
        Assert.True(Vector3.Distance(SharedCombatProjection.Corner(1), new(0, 0, SharedCombatProjection.Radius)) < .00001f);
        foreach (HexCell cell in board.Cells)
        {
            Vector3 center = SharedCombatProjection.CellPosition(board, cell.Id);
            Assert.True(SharedCombatProjection.Contains(Vector3.Zero));
            foreach (int neighbor in cell.Neighbors)
            {
                Assert.Equal(1, board.Distance(cell.Id, neighbor));
                Vector3 delta = SharedCombatProjection.CellPosition(board, neighbor) - center;
                Assert.InRange(delta.Length(), diameter - .0001f, diameter + .0001f);
                Assert.False(SharedCombatProjection.Contains(delta));
            }
            foreach (int corner in Enumerable.Range(0, 6))
            {
                Assert.True(SharedCombatProjection.Contains(SharedCombatProjection.Corner(corner, .999f)));
                Assert.False(SharedCombatProjection.Contains(SharedCombatProjection.Corner(corner, 1.001f)));
            }
            HexCell? right = board.Cells.FirstOrDefault(c => c.Coordinate.R == cell.Coordinate.R && c.Coordinate.Column == cell.Coordinate.Column + 1);
            if (right is not null)
            {
                Vector3 delta = SharedCombatProjection.CellPosition(board, right.Id) - center;
                Assert.Equal(0, delta.Z); Assert.InRange(delta.X, diameter - .0001f, diameter + .0001f);
            }
        }
    }
}
