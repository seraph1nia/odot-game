using System.Security.Cryptography;
using System.Text.Json;
using Xunit;

namespace Game.Core.Tests.Profiling;

// Synthetic shared-arena stress input, not earned balance. Four owners field
// at most six homogeneous squads each; the remaining requested individuals are
// real queued enemy groups. Topology, occupancy, profiles and steps are production.
internal static class LargeBattle
{
    public const int Horizon = 600;
    public static Match Create(int size, ulong seed, WorkCounters? work = null)
    {
        if (size is not (128 or 512 or 2048)) throw new ArgumentOutOfRangeException(nameof(size));
        UnitType[] roles = Enum.GetValues<UnitType>();
        int friendsPerOwner = Math.Min(size / 8, 18);
        int enemiesPerOwner = size / 4 - friendsPerOwner;
        var entries = roles.Select((type, index) => new SpawnEntry(type, 1, enemiesPerOwner / roles.Length + (index < enemiesPerOwner % roles.Length ? 1 : 0))).ToArray();
        var rules = new Rules { Campaign = new([new(1, false, entries, default)]) };
        var match = new Match(rules, combatSeed: seed);
        try
        {
            match.SetWorkCounters(work);
            for (int owner = 0; owner < 4; owner++) match.Join();
            Assert.True(VillageStrategyTests.Act(match, 1, "start").Accepted);
            foreach (City city in match.Players.Values)
            {
                city.Food = size * 10; city.Gold = 1000; // Declared synthetic stocks, not earned economy.
                for (int purchase = 0; purchase < 4; purchase++)
                    Assert.True(match.Apply(city.Id, Command.FromSnapshot(match.Snapshot(), 1, "buy-home", city.Id)).Accepted);
                for (int home = 0, remaining = friendsPerOwner; remaining > 0; home++)
                    for (int member = 0, count = Math.Min(3, remaining); member < count; member++, remaining--)
                        match.Combat.Create(roles[home % roles.Length], city.Id, city.Id, city.Id, assignment: new(-1, 0, home, 1));
            }
            for (int turn = 0; turn < 3; turn++)
                foreach (City city in match.Players.Values) Assert.True(VillageStrategyTests.Act(match, city.Id, "ready").Accepted);
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
            Assert.All(units.Where(u => u.Deployed), u => Assert.True(match.Combat.Board.Allows(u.Faction, u.Location.Position.Cell)));
            foreach (var cell in state.Reservations.Positions.GroupBy(p => p.Cell))
            {
                PositionReservation claim = Assert.Single(cell);
                Assert.Equal(1, claim.Size); Assert.Equal(1, claim.Anchor); Assert.Equal(1, claim.City);
                Assert.Contains(state.Squads, s => s.Id == claim.UnitId);
            }
            CombatReservations rebuilt = CombatReservations.Reconstruct(state.Arena!, state.Squads);
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
            Assert.InRange(match.Tick, 1, Horizon);
            Assert.Contains(checkpoints, p => p.Adventurers > 0 && p.Skeletons > 0 && p.Queued > 0);
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
