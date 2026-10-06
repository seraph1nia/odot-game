using Xunit;

namespace Game.Core.Tests;

public sealed class RoutingScoreEfficiencyTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExactMinimaMatchFullEnumerationIncludingUnreachableAndZeroDistance(bool detour)
    {
        HexBoardDefinition definition = HexBoardDefinition.Default();
        if (detour) definition = definition with
        {
            Cells = definition.Cells.Select(c => c with { Neighbors = c.Neighbors.Where(n => !((c.Id == 8 && n == 9) || (c.Id == 9 && n == 8))).ToArray() }).ToArray()
        };
        var board = new HexBoard(definition);
        using var combat = new CombatSimulation(new());
        CombatUnit prototype = combat.Inspect(combat.Create(UnitType.Swordsman, 1, 1, 1));
        var routing = new HexRouting(board, new HexOccupancy(board));
        foreach (Faction faction in Enum.GetValues<Faction>())
            foreach (HexCell source in board.Cells.Where(c => board.Allows(faction, c.Id)))
                foreach (int range in new[] { 1, 2, board.Diameter })
                    foreach (bool blocked in new[] { false, true })
                    {
                        CombatUnit actor = prototype with
                        {
                            Identity = prototype.Identity with { Faction = faction },
                            Location = new(UnitLifecycle.Alive, new(source.Id, 1)),
                            Profile = prototype.Profile with { HexRange = range }
                        };
                        var search = new HexReachability(board, source.Id, cell => !blocked && board.Allows(faction, cell));
                        foreach (HexCell target in board.Cells)
                            foreach (CombatTargetKind kind in Enum.GetValues<CombatTargetKind>())
                            {
                                int Distance(int cell) => kind == CombatTargetKind.City ? board.CityDistance(cell) : board.Distance(cell, target.Id);
                                int[] goals = board.Cells.Where(c => Distance(c.Id) >= 1 && Distance(c.Id) <= range).Select(c => c.Id).ToArray();
                                int steps = goals.Select(cell => search.Distances.GetValueOrDefault(cell, int.MaxValue)).Where(d => d > 0).DefaultIfEmpty(int.MaxValue).Min();
                                int staticSteps = goals.Where(cell => board.Allows(faction, cell)).Select(cell => board.MovementDistance(faction, source.Id, cell)).DefaultIfEmpty(int.MaxValue).Min();
                                ApproachScore score = routing.Score(actor, new(kind, 123), target.Id, 7, search);
                                Assert.Equal(steps, score.Steps); Assert.Equal(staticSteps, score.StaticSteps);
                                // Reusing geometry must not reuse a different target identity/initiative.
                                ApproachScore reused = routing.Score(actor, new(kind, 456), target.Id, 9, search);
                                Assert.Equal(score with { Target = new(kind, 456), Initiative = 9 }, reused);
                            }
                    }
    }
}
