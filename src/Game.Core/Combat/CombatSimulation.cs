using Arch.Core;
using EcsWorld = Arch.Core.World;

namespace Game.Core;

// ECS is the only unit store. Reservations are a derived index.
internal sealed partial class CombatSimulation : IDisposable
{
    public const int HistoryTicks = 120, HistoryLimit = 4096;
    private readonly CombatConfiguration _configuration;
    private readonly HexBoard _board;
    private readonly HexOccupancy _occupancy;
    private readonly HexRouting _routing;
    private readonly ulong _seed;
    private readonly EcsWorld _world;
    private readonly Dictionary<int, Entity> _entities = [];
    private readonly List<CombatEvent> _events = [];
    private readonly SortedDictionary<int, AdmissionBound> _admissions = [];
    private readonly SortedSet<int> _healthProgress = [];
    private readonly Dictionary<int, BattlefieldObservation> _observations = [];
    private readonly Dictionary<int, BattlefieldObservation> _cityObservations = [];
    private readonly QueryDescription _units = new QueryDescription().WithAll<CombatUnit>();
    private int _nextId = 1, _wave = 1;
    private long _tick;
    public CombatSimulation(Rules rules, CombatConfiguration? configuration = null, ulong seed = 0)
    {
        _configuration = configuration ?? new(rules); _board = _configuration.Board;
        _occupancy = new(_board); _routing = new(_board, _occupancy); _seed = seed;
        _world = EcsWorld.Create();
    }
    private WorkCounters? _work;
    internal WorkCounters? Work
    {
        get => _work;
        set
        {
            _work = value; _occupancy.Work = value; _routing.Work = value; _configuration.Work = value;
            value?.Support(WorkMetric.WorldViews, WorkMetric.UnitVisits, WorkMetric.Sorts, WorkMetric.SortElements,
                WorkMetric.UnitProjections, WorkMetric.RouteElementsCopied, WorkMetric.VisitedElementsCopied,
                WorkMetric.ObservationBuilds, WorkMetric.ObservationActorVisits, WorkMetric.ObservationReservationVisits,
                WorkMetric.OpponentVisits, WorkMetric.RangeCandidates, WorkMetric.SplashCandidates, WorkMetric.QueuedCandidates);
        }
    }
    public long EventSequence { get; private set; }
    public bool IsDisposed { get; private set; }
    public bool HasDeaths => AnyUnit(u => u.Location.Lifecycle == UnitLifecycle.Dying);
    internal bool RegistryReleased => !ReferenceEquals(EcsWorld.Worlds[_world.Id], _world);
    internal HexBoard Board => _board;
    internal CombatReservations Reservations => _occupancy.Snapshot();
    internal AdmissionBound[] Admissions => _admissions.Values.ToArray();
    internal int[] HealthProgressCities => _healthProgress.ToArray();
    public WeaponProfile Profile(UnitType type, int rank = 0, bool isBoss = false, int level = 1) => _configuration.Unit(type, rank, isBoss, level).Runtime();
    private ref CombatUnit Unit(int id) => ref _world.Get<CombatUnit>(_entities[id]);
    internal CombatUnit Inspect(int id) => Unit(id);
    internal CombatUnit[] Units() => All();
    internal int RouteSearches => _routing.Searches;
    private CombatDecisionKey Key(int id, int city, long sequence, int generation, CombatPurpose purpose, CombatActorKind kind = CombatActorKind.Unit)
        => new(_seed, _configuration.Fingerprint, _wave, city, kind, id, sequence, generation, purpose);
    private CombatDecisionState NewDecision(int id, int city, long sequence)
        => new(sequence, 0, SeededDecision.Rank(Key(id, city, sequence, 0, CombatPurpose.MovementRank)));

    public int Create(UnitType type, int owner, int origin, int destination, Faction faction = Faction.Adventurers, int rank = 0, bool isBoss = false, int level = 1, UnitCapabilities capabilities = default)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (destination <= 0 || origin <= 0 || !Enum.IsDefined(faction)) throw new ArgumentException("Invalid unit identity.");
        WeaponProfile profile = _configuration.Unit(type, rank, isBoss, level, capabilities).Runtime();
        int id = _nextId; _nextId = checked(_nextId + 1);
        Entity entity = _world.Create(new CombatUnit(new(id, type, owner, origin, destination, faction, rank, isBoss, level), profile.Health, profile,
            new(UnitLifecycle.Queued, default), new CombatAction.Waiting(), NewDecision(id, destination, 0))
        { Capabilities = capabilities });
        _entities.Add(id, entity); return id;
    }
    public void Research(int city, ResearchState research, ResearchCatalog catalog)
    {
        var updates = Living().Where(u => u.Owner == city && u.Faction == Faction.Adventurers).Select(unit =>
        {
            UnitCapabilities capabilities = catalog.Capabilities(research, unit.Type);
            return unit with { Capabilities = capabilities, Profile = _configuration.Unit(unit.Type, unit.Identity.Rank, unit.Identity.IsBoss, unit.Identity.Level, capabilities).Runtime() };
        }).ToArray();
        foreach (CombatUnit unit in updates) Unit(unit.Id) = unit;
    }
    private CombatUnit[] All()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        _work?.Add(WorkMetric.WorldViews); _work?.Add(WorkMetric.UnitVisits, _entities.Count);
        _work?.Add(WorkMetric.Sorts); _work?.Add(WorkMetric.SortElements, _entities.Count);
        var values = new List<CombatUnit>(_entities.Count); _world.Query(in _units, (ref CombatUnit unit) => values.Add(unit));
        return values.OrderBy(u => u.Id).ToArray();
    }
    private CombatUnit[] Living() => All().Where(u => u.Location.Lifecycle != UnitLifecycle.Dying).ToArray();
    private bool AnyUnit(Func<CombatUnit, bool> predicate)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        foreach (Entity entity in _entities.Values)
        {
            _work?.Add(WorkMetric.UnitVisits);
            if (predicate(_world.Get<CombatUnit>(entity))) return true;
        }
        return false;
    }
    internal CombatUnit[] EnemyMembership() => Living().Where(u => u.Faction == Faction.Skeletons).ToArray();
    internal CombatUnit[] SoldierMembership(int city) => Living().Where(u => u.Faction == Faction.Adventurers && u.Owner == city).ToArray();
    internal bool HasParticipatingSoldierLevel(int level) => AnyUnit(u => u.Faction == Faction.Adventurers && u.IsTargetable && u.Identity.Level == level);
    public UnitState[] Snapshot() => Living().Select(u => CombatProjection.Snapshot(u, _tick, _work)).ToArray();
    public UnitState[] Dying() => All().Where(u => u.Location.Lifecycle == UnitLifecycle.Dying).Select(u => CombatProjection.Snapshot(u, _tick, _work)).ToArray();
    public UnitState[] Soldiers(int city) => SoldierMembership(city).Select(u => CombatProjection.Snapshot(u, _tick, _work)).ToArray();
    public UnitState[] Enemies() => EnemyMembership().Select(u => CombatProjection.Snapshot(u, _tick, _work)).ToArray();
    internal (UnitState[] Living, UnitState[] Dying) ProjectAll()
    {
        CombatUnit[] ordered = All();
        return (ordered.Where(u => u.Location.Lifecycle != UnitLifecycle.Dying).Select(u => CombatProjection.Snapshot(u, _tick, _work)).ToArray(),
            ordered.Where(u => u.Location.Lifecycle == UnitLifecycle.Dying).Select(u => CombatProjection.Snapshot(u, _tick, _work)).ToArray());
    }
    public CombatEvent[] Events() => _events.Select(e => e with { Unit = e.Unit is null ? null : CombatProjection.Detach(e.Unit, _work), Victims = e.Victims.ToArray(), Periodic = e.Periodic.ToArray() }).ToArray();
    internal BattleFoodForecast FoodForecast(int city, int food, EconomyConfiguration economy)
        => BattleFood.Forecast(SoldierMembership(city), food, economy);
    public long OldestEventSequence => _events.Count == 0 ? EventSequence + 1 : _events[0].Sequence;
    internal UnitState Read(int id)
        => CombatProjection.Snapshot(Unit(id), _tick, _work);
    internal void Seed(UnitState state)
    {
        WeaponProfile profile = state.Profile.Health > 0 ? state.Profile : Profile(state.Type, state.Rank, state.IsBoss, state.Level);
        CombatUnit unit = CombatProjection.Restore(state, profile, state.Decision ?? NewDecision(state.Id, state.Destination, 0), _tick);
        Seed(unit);
    }
    internal void Seed(CombatUnit unit)
    {
        if (unit.Id <= 0 || unit.Identity.Level is < 1 or > Progression.MaximumExponentLevel || unit.Origin <= 0 || unit.Destination <= 0 || !Enum.IsDefined(unit.Type) || !Enum.IsDefined(unit.Faction)
            || !Enum.IsDefined(unit.Location.Lifecycle) || unit.Health < 0 || unit.Health > unit.Profile.Health
            || unit.Location.Lifecycle != UnitLifecycle.Dying && unit.Health == 0
            || unit.Location.Lifecycle is UnitLifecycle.Queued or UnitLifecycle.Reserve && unit.Action is CombatAction.Moving or CombatAction.Windup
            || unit.Profile.Size is < 1 or > 6 || unit.Identity.IsBoss && unit.Profile.Size != 6)
            throw new ArgumentException("Invalid unit health, identity or profile anchor.", nameof(unit));
        unit.Capabilities.Validate(); StatusPolicy.Validate(unit.Statuses, _configuration.Statuses);
        if (unit.Statuses.Poison.Append(unit.Statuses.Burn).OfType<StatusEffect>().Any(e => e.Strength > int.MaxValue / checked(_board.Cells.Count * _board.Capacity + 14))) throw new ArgumentException("Unsafe restored potency.");
        int nextId = Math.Max(_nextId, checked(unit.Id + 1));
        // Validate a complete prospective index before replacing the ECS entity.
        _occupancy.Rebuild(All().Where(u => u.Id != unit.Id).Select(u => u.Reservation).Append(unit.Reservation));
        if (_entities.Remove(unit.Id, out Entity old)) _world.Destroy(old);
        Entity entity = _world.Create(unit);
        _entities.Add(unit.Id, entity); _nextId = nextId; _observations.Remove(unit.Id);
    }
    internal void Remove(int id)
    {
        if (!_entities.TryGetValue(id, out Entity entity)) return;
        _occupancy.Release(Unit(id).Destination, id); _observations.Remove(id); _entities.Remove(id); _world.Destroy(entity);
    }
    public void BeginWave(int wave = 1, IEnumerable<int>? unfed = null)
    {
        if (HasDeaths) throw new InvalidOperationException("Cannot reform while prior deaths hold space.");
        var reserves = (unfed ?? []).ToHashSet();
        _wave = wave; _events.Clear(); _occupancy.Clear(); _routing.Clear(); _admissions.Clear(); _observations.Clear(); _cityObservations.Clear();
        foreach (CombatUnit unit in Living())
        {
            Unit(unit.Id) = unit with
            {
                Location = new(reserves.Contains(unit.Id) ? UnitLifecycle.Reserve : UnitLifecycle.Queued, default),
                Decision = NewDecision(unit.Id, unit.Destination, checked(unit.Decision.Sequence + 1)),
                Action = CombatActions.Cancel(unit.Action, _tick),
                Statuses = StatusState.Empty
            };
        }
        AdmitEntries(initial: true);
    }
    public void AdmitEntries(bool initial = false)
    {
        CombatUnit[] membership = Living();
        // A stage-local derived entry view. Every placement changes occupancy;
        // failed identical band queries can share their exact result meanwhile.
        long entryRevision = _occupancy.Revision;
        var entries = new Dictionary<(int City, Faction Faction, int Size, bool Front), HexPosition?>();
        CombatUnit[] queued = WorkOrdering.Input(membership.Where(u => u.Location.Lifecycle == UnitLifecycle.Queued), _work)
            .OrderBy(u => initial ? u.Class == UnitClass.Melee : u.Class != UnitClass.Melee).ThenBy(u => u.Profile.Initiative)
            .ThenBy(u => initial ? SeededDecision.Rank(Key(u.Id, u.Destination, u.Decision!.Sequence, 0, CombatPurpose.Formation)) : u.Decision!.SchedulingRank).ThenBy(u => u.Id).ToArray();
        foreach (CombatUnit unit in queued)
        {
            _work?.Add(WorkMetric.QueuedCandidates);
            int bands = unit.Class == UnitClass.Melee && !initial ? 2 : 1;
            for (int index = 0; index < bands; index++)
            {
                if (entryRevision != _occupancy.Revision) { entries.Clear(); entryRevision = _occupancy.Revision; }
                bool front = unit.Class == UnitClass.Melee && index == 0;
                var key = (unit.Destination, unit.Faction, unit.Profile.Size, front);
                if (!entries.TryGetValue(key, out HexPosition? position))
                {
                    IReadOnlyList<int> band = front ? _board.Front(unit.Faction) : _board.Rear(unit.Faction);
                    position = WorkOrdering.Input(band.Select((cell, preference) => new { cell, preference }), _work)
                        .OrderBy(c => _occupancy.UsedCapacity(unit.Destination, c.cell)).ThenBy(c => c.preference)
                        .SelectMany(c => WorkOrdering.Input(_occupancy.Free(unit.Destination, unit.Faction, c.cell, unit.Profile.Size), _work)
                            .OrderBy(f => unit.Faction == Faction.Adventurers ? f.AnchorForward : -f.AnchorForward).ThenBy(f => f.Id)
                            .Select(f => (HexPosition?)new HexPosition(c.cell, f.Id))).FirstOrDefault();
                    entries.Add(key, position);
                }
                if (position is null) continue;
                CombatUnit admitted = unit with
                {
                    Location = unit.Location with { Lifecycle = UnitLifecycle.Alive, Position = position.Value, AdmittedTick = _tick },
                    Action = unit.ReadyTick > _tick ? unit.Action : new CombatAction.Waiting(unit.Action.Sequence, unit.Action.AttackSequence, unit.ReadyTick, unit.Action.Attack)
                };
                if (!_occupancy.TryPlace(admitted.Reservation)) throw new InvalidOperationException("Placement lost its atomic reservation.");
                Unit(unit.Id) = admitted; break;
            }
        }
        foreach (AdmissionBound bound in _admissions.Values.Where(b => b.AdmissionTick is null).ToArray())
        {
            CombatUnit? first = membership.Select(u => Unit(u.Id)).FirstOrDefault(u => u.Destination == bound.City && u.Faction == Faction.Skeletons && u.Deployed);
            if (first is not null) _admissions[bound.City] = bound with { AdmissionTick = _tick, FirstUnitId = first.Id };
        }
    }
    internal void TrackClearedAdmission(int city)
    {
        if (_admissions.TryGetValue(city, out AdmissionBound? previous) && previous.AdmissionTick is null) return;
        CombatUnit[] current = All();
        CombatUnit[] incoming = current.Where(u => u.Destination == city && u.Faction == Faction.Skeletons && u.Location.Lifecycle == UnitLifecycle.Queued).ToArray();
        if (incoming.Length == 0 || current.Any(u => u.Destination == city && u.Faction == Faction.Skeletons && u.IsTargetable)) return;
        bool Fits(HexOccupancy index) => incoming.Any(u => _board.Rear(Faction.Skeletons).Any(cell => index.Free(city, Faction.Skeletons, cell, u.Profile.Size).Any()));
        long bound = _tick;
        if (!Fits(_occupancy))
        {
            long[] releases = current.Where(u => u.Destination == city && u.Faction == Faction.Skeletons && u.Location.Lifecycle == UnitLifecycle.Dying
                && (_board.Rear(Faction.Skeletons).Contains(u.Location.Position.Cell) || u.Action is CombatAction.Moving move && _board.Rear(Faction.Skeletons).Contains(move.Destination.Cell)))
                .Select(u => u.Location.DeathEndTick).Distinct().Order().ToArray();
            bool found = false;
            foreach (long release in releases)
            {
                var predicted = new HexOccupancy(_board);
                predicted.Rebuild(current.Where(u => u.Location.Lifecycle != UnitLifecycle.Dying || u.Location.DeathEndTick > release).Select(u => u.Reservation));
                if (!Fits(predicted)) continue;
                bound = release; found = true; break;
            }
            if (!found) throw new InvalidOperationException("Cleared protected entry has no finite first-admission bound.");
        }
        _admissions[city] = new(city, _tick, bound);
    }
    public void Cleanup(long tick) => Cleanup(tick, All());
    private void Cleanup(long tick, CombatUnit[] before)
    {
        _tick = tick; _events.RemoveAll(e => e.Tick < tick - HistoryTicks);
        foreach (CombatUnit unit in before.Where(u => u.Location.Lifecycle == UnitLifecycle.Dying)) if (_occupancy.ExpireDeath(unit.Reservation, tick)) Remove(unit.Id);
        TrimHistory();
    }
    public void Advance(long tick, IEnumerable<City> cities)
    {
        CombatUnit[] before = All();
        Cleanup(tick, before); _healthProgress.Clear();
        foreach (CombatUnit unit in before.Where(u => u.CanAct))
        {
            if (_occupancy.Arrive(unit.Reservation, tick))
            {
                var move = (CombatAction.Moving)unit.Action; CombatDecisionState d = unit.Decision;
                Unit(unit.Id) = unit with
                {
                    Location = unit.Location with { Position = move.Destination },
                    Action = CombatActions.Complete(move, tick),
                    Decision = NewDecision(unit.Id, unit.Destination, checked(d.Sequence + 1)) with
                    {
                        Generation = d.Generation,
                        ObjectiveId = d.ObjectiveId,
                        ObjectiveCity = d.ObjectiveCity,
                        ObjectiveCell = d.ObjectiveCell,
                        Route = d.Route.Skip(1).ToArray(),
                        Visited = d.Visited.Append(unit.Location.Position.Cell).Distinct().ToArray()
                    }
                };
            }
            else if (unit.Action is CombatAction.Recovery && tick >= unit.ReadyTick)
                Unit(unit.Id) = unit with { Action = CombatActions.Complete(unit.Action, tick), Decision = NewDecision(unit.Id, unit.Destination, checked(unit.Decision.Sequence + 1)) };
        }
        // Arrivals/recoveries replace immutable ECS values without changing this
        // stage's surviving targetable membership. Read their committed values.
        CombatUnit[] alive = before.Where(u => u.IsTargetable).Select(u => Unit(u.Id)).ToArray();
        _work?.Add(WorkMetric.UnitVisits, alive.Length);
        var stage = new CombatStage(alive); var current = stage.ById;
        var damage = new SortedDictionary<int, int>(); var cityDamage = new SortedDictionary<int, int>();
        var pendingStatuses = new List<StatusApplication>();
        Dictionary<int, StatusState> statusUpdates = GatherPeriodic(before, tick, damage);
        foreach (CombatUnit unit in alive.Where(u => u.PendingImpact && tick >= u.Action.Attack!.ImpactTick))
        {
            var action = (CombatAction.Windup)unit.Action; AttackRecord attack = action.Attack!;
            bool targetCity = attack.Target.Kind == CombatTargetKind.City; int targetId = attack.Target.Id;
            CombatUnit? primary = targetCity ? null : current.GetValueOrDefault(targetId);
            bool valid = targetCity ? unit.Faction == Faction.Skeletons && targetId == unit.Destination && stage.Faction(unit.Destination, Faction.Adventurers).Length == 0
                    && _board.CityDistance(unit.Location.Position.Cell) <= unit.Profile.HexRange
                : primary is not null && primary.Faction != unit.Faction && primary.Destination == unit.Destination && CombatDecisions.InRange(_board, unit, primary, _work);
            int[] victims = valid && !targetCity ? CombatDecisions.Victims(_board, primary!, stage.Faction(primary!.Destination, primary.Faction), unit.Profile.VictimCap, unit.Profile.SplashHexRadius,
                Key(unit.Id, unit.Destination, unit.Decision.Sequence, 0, CombatPurpose.Splash), _work) : [];
            if (valid && targetCity) Accumulate(cityDamage, targetId, unit.Profile.Damage);
            foreach (int id in victims)
            { Accumulate(damage, id, current[id].Capabilities.Reduce(unit.Profile.Damage)); GatherApplications(unit, id, tick, pendingStatuses); }
            Emit(CombatEventType.Impact, unit, targetId, targetCity, unit.Profile.Damage, valid, primary, victims);
            Unit(unit.Id) = unit with { Action = CombatActions.Impact(action, tick, valid) };
        }
        foreach (City city in cities.Where(c => !c.Eliminated).OrderBy(c => c.Id))
        {
            foreach (DefenseActor defense in Defenses(city)) ResolveDefense(city, defense, stage, damage);
        }
        foreach ((int id, int amount) in damage)
        {
            CombatUnit unit = Unit(id); int effective = Math.Min(unit.Health, amount); Unit(id) = unit with { Health = unit.Health - effective };
            if (effective > 0) { _healthProgress.Add(unit.Destination); Emit(CombatEventType.Hit, Unit(id), damage: effective, landed: true); }
        }
        foreach (City city in cities)
        {
            int next = Math.Max(0, city.Health - cityDamage.GetValueOrDefault(city.Id));
            if (next < city.Health) _healthProgress.Add(city.Id);
            city.Health = next;
        }
        CommitStatuses(statusUpdates, pendingStatuses);
        foreach (CombatUnit member in before.Where(u => u.Location.Lifecycle != UnitLifecycle.Dying))
        {
            CombatUnit unit = Unit(member.Id);
            if (unit.Health <= 0) Kill(unit);
        }
        TrimHistory();
    }
    private IEnumerable<DefenseActor> Defenses(City city)
    {
        yield return new(DefenseIdentity.Defender(city.Id), city.Defender, _configuration.BuiltInDefense);
        foreach ((int slot, TowerState tower) in WorkOrdering.Input(city.Towers, _work).OrderBy(p => p.Key).ToArray())
            yield return new(DefenseIdentity.Tower(city.Id, slot), tower, _configuration.Tower(tower.Type, tower.Level));
    }
    private static void Accumulate(SortedDictionary<int, int> damage, int id, int amount)
        => damage[id] = checked(damage.GetValueOrDefault(id) + amount);
    private void ResolveDefense(City city, DefenseActor defense, CombatStage stage, SortedDictionary<int, int> damage)
    {
        TowerState tower = defense.State; DefenseProfile profile = defense.Profile; DefenseIdentity identity = defense.Identity;
        if (!tower.PendingImpact || _tick < tower.ImpactTick) return;
        CombatUnit? primary = stage.ById.GetValueOrDefault(tower.TargetId);
        if (primary is not null && (primary.Faction != Faction.Skeletons || primary.Destination != city.Id)) primary = null;
        int[] victims = primary is null ? [] : CombatDecisions.Victims(_board, primary, stage.Faction(city.Id, Faction.Skeletons), profile.VictimCap, profile.SplashHexRadius,
            Key(identity.KeyId, city.Id, tower.AttackSequence, 0, CombatPurpose.Splash, identity.Kind), _work);
        foreach (int id in victims) Accumulate(damage, id, stage.ById[id].Capabilities.Reduce(profile.Damage));
        EmitTower(CombatEventType.Impact, tower, primary, profile.Damage, primary is not null, victims);
        defense.Commit(city, tower with { PendingImpact = false });
        if (identity.Kind == CombatActorKind.Defender && primary is not null) Emit(CombatEventType.DefenderShot, primary, primary.Id, damage: profile.Damage, landed: true);
    }
    public void StartActions(IEnumerable<City> cities)
    {
        CombatUnit[] alive = Living().Where(u => u.IsTargetable).ToArray(); var moves = new List<(CombatUnit Actor, HexPosition To)>();
        var stage = new CombatStage(alive);
        CombatReservations reservations = _occupancy.Snapshot();
        var positions = reservations.Positions.GroupBy(p => p.City).ToDictionary(g => g.Key, g => g.ToArray());
        var observations = new Dictionary<int, BattlefieldObservation>();
        foreach (CombatUnit actor in alive.Where(u => u.CanAct && u.Action is CombatAction.Waiting && _tick >= u.ReadyTick))
        {
            CombatUnit[] opponents = stage.Opponents(actor);
            _work?.Add(WorkMetric.OpponentVisits, opponents.Length);
            CombatUnit? target = CombatDecisions.Select(opponents.Where(u => CombatDecisions.InRange(_board, actor, u, _work)), u => _board.Distance(actor.Location.Position.Cell, u.Location.Position.Cell),
                Key(actor.Id, actor.Destination, actor.Decision.Sequence, actor.Decision.Generation, CombatPurpose.Target), _work);
            bool city = opponents.Length == 0 && actor.Faction == Faction.Skeletons;
            if (target is not null || city && _board.CityDistance(actor.Location.Position.Cell) <= actor.Profile.HexRange)
            { StartAttack(actor, target, city); continue; }
            if (opponents.Length == 0 && !city) continue;
            CombatDecisionState d = actor.Decision;
            if (!observations.TryGetValue(actor.Destination, out BattlefieldObservation? payload))
            {
                payload = BattlefieldObservation.Capture(actor.Destination, 0, new(positions.GetValueOrDefault(actor.Destination) ?? [], []), stage.City(actor.Destination), _work);
                if (_cityObservations.TryGetValue(actor.Destination, out BattlefieldObservation? previous) && previous.Matches(payload)) payload = previous;
                _cityObservations[actor.Destination] = observations[actor.Destination] = payload;
            }
            BattlefieldObservation observation = payload with { DecisionSequence = d.Sequence };
            if (_observations.TryGetValue(actor.Id, out BattlefieldObservation? prior) && prior.Matches(observation))
            {
                if (d.Route.Length > 0) moves.Add((actor, d.Route[0]));
                else if (_tick >= d.RetryTick) Unit(actor.Id) = actor with { Decision = d with { RetryTick = checked(_tick + _configuration.RetryTicks) } };
                continue;
            }
            if (d.ObjectiveId == 0) d = d with { Generation = checked(d.Generation + 1) };
            CombatDecisionKey key = Key(actor.Id, actor.Destination, d.Sequence, d.Generation, CombatPurpose.Target);
            HexReachability search = _routing.Search(actor, []);
            ApproachScore[] scores = city ? [_routing.Score(actor, new(CombatTargetKind.City, actor.Destination), 0, 0, search)]
                : opponents.Select(t => _routing.Score(actor, new(CombatTargetKind.Unit, t.Id), t.Location.Position.Cell, t.Profile.Initiative, search)).ToArray();
            ApproachScore winner = HexRouting.Select(scores, key, _work);
            bool unchanged = d.ObjectiveId == winner.Target.Id && d.ObjectiveCity == (winner.Target.Kind == CombatTargetKind.City) && d.ObjectiveCell == winner.Cell;
            if (!unchanged)
            {
                if (d.ObjectiveId != 0) d = d with { Generation = checked(d.Generation + 1) };
                d = d with { Visited = [], Route = [] }; key = key with { Generation = d.Generation };
            }
            bool retain = unchanged && winner.Reachable && d.Route.Length == winner.Steps
                && d.Route.All(p => _occupancy.CanPlace(actor.Destination, actor.Faction, p, actor.Profile.Size));
            HexPosition[] route;
            if (retain) route = d.Route;
            else
            {
                if (unchanged && d.Visited.Length > 0) search = _routing.Search(actor, d.Visited);
                route = _routing.Route(actor, winner, stage.City(actor.Destination), key, search);
            }
            Unit(actor.Id) = actor with
            {
                Decision = d with
                {
                    ObjectiveId = winner.Target.Id,
                    ObjectiveCity = winner.Target.Kind == CombatTargetKind.City,
                    ObjectiveCell = winner.Cell,
                    Route = route,
                    ObservedRevision = _occupancy.Revision,
                    RetryTick = checked(_tick + _configuration.RetryTicks)
                }
            };
            _observations[actor.Id] = observation;
            if (route.Length > 0) moves.Add((Unit(actor.Id), route[0]));
        }
        foreach (var move in WorkOrdering.Input(moves, _work).OrderBy(m => m.Actor.Profile.Initiative).ThenBy(m => m.Actor.Decision!.SchedulingRank).ThenBy(m => m.Actor.Id))
        {
            CombatAction.Moving action = CombatActions.Move(move.Actor.Action, _board.Transition(move.Actor.Location.Position, move.To), _tick, StatusPolicy.Duration(move.Actor.Profile.MoveTicks, move.Actor.Statuses.ChillAt(_tick)));
            if (_occupancy.TryAction(move.Actor.Reservation, action)) Unit(move.Actor.Id) = move.Actor with { Action = action };
        }
        foreach (City city in cities.Where(c => !c.Eliminated).OrderBy(c => c.Id))
        {
            foreach (DefenseActor defense in Defenses(city)) StartDefense(city, defense, stage.Faction(city.Id, Faction.Skeletons));
        }
        TrimHistory();
    }
    private void StartAttack(CombatUnit unit, CombatUnit? target, bool city)
    {
        int targetId = target?.Id ?? unit.Destination;
        CombatAction.Windup action = CombatActions.Attack(unit.Action, new(city ? CombatTargetKind.City : CombatTargetKind.Unit, targetId), _tick,
            StatusPolicy.Duration(unit.Profile.WindupTicks, unit.Statuses.ChillAt(_tick)), StatusPolicy.Duration(checked(unit.Profile.CadenceTicks - unit.Profile.WindupTicks), unit.Statuses.ChillAt(_tick)));
        if (!_occupancy.TryAction(unit.Reservation, action)) throw new InvalidOperationException("Attack lost its standing reservation.");
        Unit(unit.Id) = unit with { Action = action };
        Emit(CombatEventType.AttackStarted, Unit(unit.Id), targetId, city, primary: target);
    }
    private void StartDefense(City city, DefenseActor defense, CombatUnit[] alive)
    {
        TowerState tower = defense.State; DefenseIdentity identity = defense.Identity;
        if (tower.PendingImpact || _tick < tower.ReadyTick) return;
        CombatUnit? target = CombatDecisions.Select(alive.Where(u => u.Faction == Faction.Skeletons && u.Destination == city.Id), u => _board.CityDistance(u.Location.Position.Cell),
            Key(identity.KeyId, city.Id, tower.AttackSequence, 0, CombatPurpose.Target, identity.Kind), _work);
        if (target is null) return;
        tower = defense.Start(target.Id, _tick); defense.Commit(city, tower);
        EmitTower(CombatEventType.AttackStarted, tower, target, defense.Profile.Damage, false, []);
    }
    private HexPosePoint? FrozenAim(CombatUnit unit)
    {
        if (unit.Target is not { Kind: CombatTargetKind.Unit } identity || !_entities.ContainsKey(identity.Id)) return null;
        CombatUnit target = Unit(identity.Id);
        return target.Deployed && target.Destination == unit.Destination ? target.Pose(_tick) : null;
    }
    private void Kill(CombatUnit unit)
    {
        HexPosePoint? aim = FrozenAim(unit);
        if (!unit.Deployed) { Emit(CombatEventType.Death, unit with { Health = 0 }); Remove(unit.Id); return; }
        Unit(unit.Id) = unit with
        {
            Health = 0,
            Location = unit.Location with
            {
                Lifecycle = UnitLifecycle.Dying,
                DeathStartTick = _tick,
                DeathEndTick = checked(_tick + unit.Profile.DeathTicks),
                FrozenMoveTicks = unit.Action is CombatAction.Moving move ? checked((int)(_tick - move.StartTick)) : 0,
                FrozenAim = aim
            }
        };
        Emit(CombatEventType.Death, Unit(unit.Id));
        Unit(unit.Id) = Unit(unit.Id) with { Statuses = StatusState.Empty };
    }
    public void EliminateArmy(int city)
    { foreach (CombatUnit soldier in Living().Where(u => u.Owner == city && u.Faction == Faction.Adventurers)) Kill(soldier); TrimHistory(); }
    public void Transfer(int id, int destination)
    {
        CombatUnit unit = Unit(id);
        if (unit.Location.Lifecycle == UnitLifecycle.Dying) throw new InvalidOperationException("Dying units cannot transfer.");
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(destination);
        CombatAction action = CombatActions.Cancel(unit.Action, _tick);
        CombatDecisionState decision = NewDecision(id, destination, checked(unit.Decision.Sequence + 1));
        _occupancy.Release(unit.Destination, id); _observations.Remove(id);
        Unit(id) = unit with { Identity = unit.Identity with { Destination = destination }, Location = new(UnitLifecycle.Queued, default), Action = action, Decision = decision };
    }
    public void Step(long tick, IEnumerable<City> cities)
    { City[] values = cities.ToArray(); Advance(tick, values); AdmitEntries(); StartActions(values); }
    public void StopActions(IEnumerable<City> cities)
    {
        foreach (CombatUnit unit in Living())
        {
            Unit(unit.Id) = unit with { Statuses = StatusState.Empty, Location = unit.IsTargetable ? unit.Location with { FrozenTick = _tick, FrozenAim = FrozenAim(unit) } : unit.Location };
        }
        foreach (City city in cities)
        {
            city.Defender = city.Defender with { PendingImpact = false };
            foreach (TowerState tower in city.Towers.Values.ToArray()) city.Towers[tower.Slot] = tower with { PendingImpact = false };
        }
    }
    private void Emit(CombatEventType type, CombatUnit unit, int target = 0, bool city = false, int damage = 0, bool landed = false, CombatUnit? primary = null, int[]? victims = null)
    {
        EventSequence = checked(EventSequence + 1);
        _events.Add(new(EventSequence, _tick, type, CombatProjection.Snapshot(unit, _tick, _work), target, city, damage, landed)
        {
            ImpactPose = city ? null : primary is not null ? primary.Pose(_tick)
            : type is CombatEventType.Hit or CombatEventType.Death or CombatEventType.DefenderShot ? unit.Pose(_tick) : null,
            Periodic = type is CombatEventType.Hit or CombatEventType.Death ? _periodic.GetValueOrDefault(unit.Id)?.ToArray() ?? [] : [],
            Victims = victims ?? []
        });
    }
    private void EmitTower(CombatEventType type, TowerState tower, CombatUnit? target, int damage, bool landed, int[] victims)
    {
        EventSequence = checked(EventSequence + 1);
        _events.Add(new(EventSequence, _tick, type, null, tower.TargetId, false, damage, landed)
        { Tower = tower, ImpactPose = target?.Pose(_tick), Victims = victims });
    }
    private void TrimHistory() { if (_events.Count > HistoryLimit) _events.RemoveRange(0, _events.Count - HistoryLimit); }
    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true; _entities.Clear(); _events.Clear(); _admissions.Clear(); _healthProgress.Clear(); _observations.Clear(); _cityObservations.Clear(); _occupancy.Clear(); _routing.Clear(); EcsWorld.Destroy(_world); GC.SuppressFinalize(this);
    }
}
