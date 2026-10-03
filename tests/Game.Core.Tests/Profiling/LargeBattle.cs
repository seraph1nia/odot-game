using System.Security.Cryptography;
using System.Text.Json;
using Xunit;

namespace Game.Core.Tests.Profiling;

// Synthetic capacity/stress workload. Economy funding and armies are test-owned;
// combat uses ordinary profiles, size, decisions, admission and fixed ticks.
internal static class LargeBattle
{
    public const int Horizon = 600;
    public static HexBoardDefinition Board()
    {
        HexBoardDefinition ordinary = HexBoardDefinition.Default();
        var cells = new List<HexCellDefinition>(256);
        for (int row = 0; row < 8; row++)
            for (int column = 0; column < 32; column++)
                cells.Add(new(cells.Count + 1, HexCoordinate.FromOffset(column, row),
                    row == 0 ? DeploymentAffinity.Skeletons : row == 7 ? DeploymentAffinity.Adventurers : DeploymentAffinity.Neutral, []));
        HexCellDefinition[] graph = cells.Select(c => c with { Neighbors = cells.Where(n => n.Id != c.Id && n.Coordinate.Distance(c.Coordinate) == 1).Select(n => n.Id).ToArray() }).ToArray();
        int[] Row(int row) => graph.Where(c => c.Coordinate.R == row).Select(c => c.Id).ToArray();
        return new("synthetic-large-battle-32x8", 1, 6, graph, ordinary.Anchors,
            [new(Faction.Skeletons, Row(0), Row(1)), new(Faction.Adventurers, Row(7), Row(6))], Row(6));
    }
    public static Match Create(int size, ulong seed, WorkCounters? work = null)
    {
        if (size is not (128 or 512 or 2048)) throw new ArgumentOutOfRangeException(nameof(size));
        UnitType[] roles = Enum.GetValues<UnitType>();
        var rules = new Rules { Combat = new() { Board = Board() }, Campaign = new([new(1, false, roles.Select(t => new SpawnEntry(t, 1, size / 8)).ToArray(), default)]) };
        var match = new Match(rules, combatSeed: seed);
        try
        {
            match.SetWorkCounters(work);
            City city = match.Join()!; city.Food = size * 10;
            Assert.True(VillageStrategyTests.Act(match, city.Id, "start").Accepted);
            foreach (UnitType type in roles)
                for (int i = 0; i < size / 8; i++) match.Combat.Create(type, city.Id, city.Id, city.Id);
            for (int turn = 0; turn < 4; turn++) Assert.True(VillageStrategyTests.Act(match, city.Id, "ready").Accepted);
            Assert.Equal(Phase.Combat, match.Phase);
            return match;
        }
        catch { match.Dispose(); throw; }
    }
    public static LargeBattleResult Run(int size, ulong seed, Measurements measurement, WorkCounters? work = null)
    {
        measurement.Enter("setup");
        using Match match = Create(size, seed, work);
        measurement.Enter("assertion-hash");
        MatchSnapshot initial = match.Snapshot();
        string input = Digest(initial with { MatchId = "session" });
        int[] identities = match.Combat.Units().Select(u => u.Id).ToArray();
        var initialQueued = match.Combat.Units().Where(u => u.Location.Lifecycle == UnitLifecycle.Queued).Select(u => u.Id).ToHashSet();
        var admitted = new HashSet<int>(); var deaths = new HashSet<int>();
        using var eventHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using var stateHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        long cursor = 0, activeUnitTicks = 0, attacks = 0;
        var checkpoints = new List<BattlePopulation>();
        var populations = new List<BattlePopulation>();
        BattlePopulation Observe()
        {
            CombatUnit[] units = match.Combat.Units();
            foreach (CombatUnit unit in units.Where(u => u.Deployed && initialQueued.Contains(u.Id))) admitted.Add(unit.Id);
            return new(match.Tick, units.Count(u => u.Location.Lifecycle == UnitLifecycle.Alive),
                units.Count(u => u.Location.Lifecycle == UnitLifecycle.Alive && u.Faction == Faction.Adventurers),
                units.Count(u => u.Location.Lifecycle == UnitLifecycle.Alive && u.Faction == Faction.Skeletons),
                units.Count(u => u.Location.Lifecycle == UnitLifecycle.Queued), units.Count(u => u.Location.Lifecycle == UnitLifecycle.Reserve), units.Count(u => u.Location.Lifecycle == UnitLifecycle.Dying));
        }
        void Checkpoint()
        {
            MatchSnapshot state = match.Snapshot();
            CombatUnit[] units = match.Combat.Units();
            Assert.Equal(units.Length, units.Select(u => u.Id).Distinct().Count());
            Assert.All(units, u => Assert.Contains(u.Id, identities));
            Assert.All(identities.Except(units.Select(u => u.Id)), id => Assert.Contains(id, deaths));
            Assert.All(units.Where(u => u.Deployed), u => Assert.True(match.Configuration.Board.Allows(u.Faction, u.Location.Position.Cell)));
            foreach (var cell in state.Reservations.Positions.GroupBy(p => (p.City, p.Cell)))
            {
                Assert.InRange(cell.Sum(p => p.Size), 1, 6);
                Assert.Single(cell.Select(p => p.Faction).Distinct());
                Assert.Equal(cell.Count(), cell.Select(p => p.Anchor).Distinct().Count());
            }
            CombatReservations rebuilt = CombatReservations.Reconstruct(match.Configuration.Board, units.Select(u => CombatProjection.Snapshot(u, match.Tick).Hex!));
            Assert.Equal(JsonSerializer.Serialize(state.Reservations, WireJson.Options), JsonSerializer.Serialize(rebuilt, WireJson.Options));
            Assert.InRange(state.CombatEvents.Length, 0, CombatSimulation.HistoryLimit);
            Assert.All(state.CombatEvents, e => Assert.True(e.Tick >= match.Tick - CombatSimulation.HistoryTicks));
            BattlePopulation population = Observe(); checkpoints.Add(population);
            Console.WriteLine($"large-battle/{size}: tick={match.Tick}, deployed={population.Deployed}, queued={population.Queued}, landed={attacks}, deaths={deaths.Count}");
            stateHash.AppendData(JsonSerializer.SerializeToUtf8Bytes(state with { MatchId = "session" }, WireJson.Options));
        }
        Checkpoint();
        while (match.Tick < Horizon && match.Phase == Phase.Combat)
        {
            measurement.Enter("stepping"); match.Step();
            measurement.Enter("assertion-hash");
            BattlePopulation population = Observe(); populations.Add(population); activeUnitTicks += population.Deployed;
            CombatEvent[] events = match.Combat.Events().Where(e => e.Sequence > cursor).ToArray();
            foreach (CombatEvent entry in events)
            {
                Assert.Equal(cursor + 1, entry.Sequence); cursor = entry.Sequence;
                if (entry.Type == CombatEventType.Death && entry.Unit is { } unit) deaths.Add(unit.Id);
                if (entry.Type == CombatEventType.Impact && entry.Landed) attacks++;
                eventHash.AppendData(JsonSerializer.SerializeToUtf8Bytes(entry, WireJson.Options));
            }
            if (match.Tick % 60 == 0 || match.Phase != Phase.Combat) Checkpoint();
        }
        if (size == 2048)
        {
            Assert.Equal(Horizon, match.Tick);
            Assert.Contains(checkpoints, p => p.Deployed >= 256 && p.Adventurers >= 128 && p.Skeletons >= 128 && p.Queued > 0);
            Assert.True(attacks > 0); Assert.NotEmpty(deaths); Assert.NotEmpty(admitted);
        }
        return new(size, seed, match.Tick, match.Phase.ToString(), input, Convert.ToHexString(stateHash.GetHashAndReset()), Convert.ToHexString(eventHash.GetHashAndReset()),
            activeUnitTicks, attacks, deaths.Count, admitted.Count, checkpoints.ToArray(), populations.Max(p => p.Deployed), populations.Average(p => p.Deployed),
            new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["living"] = Population(populations.Select(p => p.Deployed + p.Queued + p.Reserve)),
                ["deployed"] = Population(populations.Select(p => p.Deployed)),
                ["queued"] = Population(populations.Select(p => p.Queued)),
                ["reserve"] = Population(populations.Select(p => p.Reserve)),
                ["dying"] = Population(populations.Select(p => p.Dying))
            });
    }
    private static object Population(IEnumerable<int> values)
    { int[] samples = values.ToArray(); return new { Peak = samples.Max(), Mean = samples.Average(), Samples = samples.Length }; }
    internal static string Digest<T>(T value) => Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(value, WireJson.Options)));
}
internal sealed record BattlePopulation(long Tick, int Deployed, int Adventurers, int Skeletons, int Queued, int Reserve, int Dying);
internal sealed record LargeBattleResult(int Size, ulong Seed, long Ticks, string Phase, string InputDigest, string StateDigest, string EventDigest,
    long ActiveUnitTicks, long LandedAttacks, int Deaths, int InitiallyQueuedAdmissions, BattlePopulation[] Checkpoints, int PeakDeployed, double MeanDeployed,
    IReadOnlyDictionary<string, object> Populations);
