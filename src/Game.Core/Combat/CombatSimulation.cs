using Arch.Core;
using EcsWorld = Arch.Core.World;

namespace Game.Core;

// ECS is the only unit store. Reservations are a derived index.
internal sealed class CombatSimulation : IDisposable
{
    public const double Width = 3.4, Radius = .20, Tolerance = 1e-6;
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
    private readonly QueryDescription _units = new QueryDescription().WithAll<UnitIdentity>();
    private int _nextId = 1, _wave = 1;
    private long _tick;
    public CombatSimulation(Rules rules, CombatConfiguration? configuration = null, ulong seed = 0)
    {
        _configuration = configuration ?? new(rules); _board = _configuration.Board;
        _occupancy = new(_board); _routing = new(_board, _occupancy); _seed = seed;
        _world = EcsWorld.Create();
    }
    public long EventSequence { get; private set; }
    public bool IsDisposed { get; private set; }
    public bool HasDeaths => All().Any(u => u.Hex!.Lifecycle == UnitLifecycle.Dying);
    internal bool RegistryReleased => !ReferenceEquals(EcsWorld.Worlds[_world.Id], _world);
    internal HexBoard Board => _board;
    internal CombatReservations Reservations => _occupancy.Snapshot();
    internal AdmissionBound[] Admissions => _admissions.Values.ToArray();
    public WeaponProfile Profile(UnitType type, int rank = 0) => _configuration.Unit(type, rank).Legacy();
    private ref HexUnitState Spatial(int id) => ref _world.Get<HexUnitState>(_entities[id]);
    private ref CombatDecisionState Decision(int id) => ref _world.Get<CombatDecisionState>(_entities[id]);
    private CombatDecisionKey Key(int id, int city, long sequence, int generation, CombatPurpose purpose, CombatActorKind kind = CombatActorKind.Unit)
        => new(_seed, _configuration.Fingerprint, _wave, city, kind, id, sequence, generation, purpose);
    private CombatDecisionState NewDecision(int id, int city, long sequence)
        => new(sequence, 0, SeededDecision.Rank(Key(id, city, sequence, 0, CombatPurpose.MovementRank)));

    public int Create(UnitType type, int owner, int origin, int destination, Faction faction = Faction.Adventurers, int rank = 0)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (destination <= 0 || origin <= 0 || !Enum.IsDefined(faction)) throw new ArgumentException("Invalid unit identity.");
        int id = _nextId; _nextId = checked(_nextId + 1); WeaponProfile profile = Profile(type, rank);
        Entity entity = _world.Create(new UnitIdentity(id, type, owner, origin, destination, faction, rank), new UnitHealth(profile.Health),
            new HexUnitState(id, destination, faction, UnitLifecycle.Queued, default), NewDecision(id, destination, 0), new UnitTarget(), new UnitAttack(), profile);
        _entities.Add(id, entity); return id;
    }
    public void Research(int city, UnitClass @class, int rank)
    {
        foreach (UnitState unit in Soldiers(city).Where(u => u.Class == @class))
        {
            ref UnitIdentity identity = ref _world.Get<UnitIdentity>(_entities[unit.Id]); identity = identity with { Rank = rank };
            _world.Get<WeaponProfile>(_entities[unit.Id]) = Profile(unit.Type, rank);
        }
    }
    private UnitState[] All()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        var ids = new List<int>(_entities.Count); _world.Query(in _units, (ref UnitIdentity identity) => ids.Add(identity.Id)); ids.Sort();
        return ids.Select(Read).ToArray();
    }
    public UnitState[] Snapshot() => All().Where(u => u.Hex!.Lifecycle != UnitLifecycle.Dying).ToArray();
    public UnitState[] Dying() => All().Where(u => u.Hex!.Lifecycle == UnitLifecycle.Dying).ToArray();
    public UnitState[] Soldiers(int city) => Snapshot().Where(u => u.Owner == city && u.Faction == Faction.Adventurers).ToArray();
    public UnitState[] Enemies() => Snapshot().Where(u => u.Faction == Faction.Skeletons).ToArray();
    public CombatEvent[] Events() => _events.ToArray();
    public long OldestEventSequence => _events.Count == 0 ? EventSequence + 1 : _events[0].Sequence;
    // Temporary derived presentation adapter; never read by numerical decisions.
    private (double Forward, double Lateral) Anchor(HexPosition position)
    {
        if (position.Cell == 0) return default;
        HexCell cell = _board.Cell(position.Cell); HexFootprint f = _board.Footprint(position.Footprint);
        return (-cell.Coordinate.R * 2.598076211 - f.AnchorForward / 1000.0,
            cell.Coordinate.Column * 3 + (cell.Coordinate.R & 1) * 1.5 + f.AnchorX / 1000.0);
    }
    internal UnitState Read(int id)
    {
        Entity entity = _entities[id]; UnitIdentity identity = _world.Get<UnitIdentity>(entity); HexUnitState hex = Spatial(id);
        UnitTarget target = _world.Get<UnitTarget>(entity); UnitAttack attack = _world.Get<UnitAttack>(entity);
        var from = Anchor(hex.Position); var to = hex.HoldsTransit ? Anchor(hex.Destination) : from;
        double elapsed = hex.Lifecycle == UnitLifecycle.Dying ? hex.FrozenMoveTicks : Math.Clamp(_tick - hex.StartTick, 0, Math.Max(0, hex.EndTick - hex.StartTick));
        double fraction = hex.HoldsTransit ? elapsed / Math.Max(1, hex.EndTick - hex.StartTick) : 0;
        var aim = target.City ? (Forward: -2.598076211, Lateral: from.Lateral)
            : _entities.ContainsKey(target.Id) ? Anchor(Spatial(target.Id).Position) : to;
        double forward = aim.Forward - from.Forward, lateral = aim.Lateral - from.Lateral;
        double length = Math.Sqrt(forward * forward + lateral * lateral);
        bool moving = hex.Lifecycle == UnitLifecycle.Alive && hex.HoldsTransit;
        return new(id, _world.Get<UnitHealth>(entity).Value, from.Forward + (to.Forward - from.Forward) * fraction,
            checked((int)Math.Max(0, attack.ReadyTick - _tick)), identity.Origin, identity.Destination)
        {
            Hex = hex,
            Decision = Decision(id) with { Route = Decision(id).Route.ToArray(), Visited = Decision(id).Visited.ToArray() },
            Type = identity.Type,
            Faction = identity.Faction,
            Rank = identity.Rank,
            Owner = identity.Owner,
            Lateral = from.Lateral + (to.Lateral - from.Lateral) * fraction,
            Deployed = hex.Lifecycle != UnitLifecycle.Queued,
            MoveForward = moving ? (to.Forward - from.Forward) * Match.StepsPerSecond / (hex.EndTick - hex.StartTick) : 0,
            MoveLateral = moving ? (to.Lateral - from.Lateral) * Match.StepsPerSecond / (hex.EndTick - hex.StartTick) : 0,
            FacingForward = length > 0 ? forward / length : identity.Faction == Faction.Adventurers ? 1 : -1,
            FacingLateral = length > 0 ? lateral / length : 0,
            TargetId = target.Id,
            TargetCity = target.City,
            AttackSequence = attack.Sequence,
            ActionStartTick = attack.StartTick,
            ImpactTick = attack.ImpactTick,
            ReadyTick = attack.ReadyTick,
            PendingImpact = attack.Pending,
            Profile = _world.Get<WeaponProfile>(entity)
        };
    }
    internal void Seed(UnitState state)
    {
        HexUnitState hex = state.Hex ?? (state.Deployed ? throw new ArgumentException("Deployed fixtures require explicit hex state.", nameof(state))
            : new(state.Id, state.Destination, state.Faction, UnitLifecycle.Queued, default));
        if (hex.Id != state.Id || hex.City != state.Destination || hex.Faction != state.Faction) throw new ArgumentException("Fixture identity mismatch.", nameof(state));
        // Validate a complete prospective index before replacing the ECS entity.
        _occupancy.Rebuild(All().Where(u => u.Id != state.Id).Select(u => u.Hex!).Append(hex));
        if (_entities.Remove(state.Id, out Entity old)) _world.Destroy(old);
        WeaponProfile profile = state.Profile.Health > 0 ? state.Profile : Profile(state.Type, state.Rank);
        Entity entity = _world.Create(new UnitIdentity(state.Id, state.Type, state.Owner, state.Origin, state.Destination, state.Faction, state.Rank), new UnitHealth(state.Health),
            hex, state.Decision ?? NewDecision(state.Id, state.Destination, 0), new UnitTarget(state.TargetId, state.TargetCity),
            new UnitAttack(state.AttackSequence, state.ActionStartTick, state.ImpactTick, Math.Max(state.ReadyTick, checked(_tick + state.Cooldown)), state.PendingImpact, state.TargetId, state.TargetCity), profile);
        _entities.Add(state.Id, entity); _nextId = Math.Max(_nextId, checked(state.Id + 1));
    }
    internal void Remove(int id)
    {
        if (!_entities.TryGetValue(id, out Entity entity)) return;
        _occupancy.Release(Spatial(id).City, id); _entities.Remove(id); _world.Destroy(entity);
    }
    public void BeginWave(int wave = 1)
    {
        if (HasDeaths) throw new InvalidOperationException("Cannot reform while prior deaths hold space.");
        _wave = wave; _events.Clear(); _occupancy.Clear(); _admissions.Clear();
        foreach (UnitState unit in Snapshot())
        {
            Spatial(unit.Id) = new(unit.Id, unit.Destination, unit.Faction, UnitLifecycle.Queued, default, ActionSequence: unit.Hex!.ActionSequence);
            Decision(unit.Id) = NewDecision(unit.Id, unit.Destination, checked(unit.Decision!.Sequence + 1));
            _world.Get<UnitTarget>(_entities[unit.Id]) = default;
            _world.Get<UnitAttack>(_entities[unit.Id]) = new(unit.AttackSequence, 0, 0, Math.Max(_tick, unit.ReadyTick), false, 0, false);
        }
        AdmitEntries(initial: true);
    }
    public void AdmitEntries(bool initial = false)
    {
        UnitState[] queued = Snapshot().Where(u => u.Hex!.Lifecycle == UnitLifecycle.Queued)
            .OrderBy(u => initial ? u.Class == UnitClass.Melee : u.Class != UnitClass.Melee).ThenBy(u => u.Profile.Initiative)
            .ThenBy(u => initial ? SeededDecision.Rank(Key(u.Id, u.Destination, u.Decision!.Sequence, 0, CombatPurpose.Formation)) : u.Decision!.SchedulingRank).ThenBy(u => u.Id).ToArray();
        foreach (UnitState unit in queued)
        {
            IEnumerable<int[]> bands = unit.Class != UnitClass.Melee ? [_board.Rear(unit.Faction).ToArray()]
                : initial ? [_board.Front(unit.Faction).ToArray()] : [_board.Front(unit.Faction).ToArray(), _board.Rear(unit.Faction).ToArray()];
            foreach (int[] band in bands)
            {
                HexPosition? position = band.Select((cell, preference) => new { cell, preference })
                    .OrderBy(c => _occupancy.UsedCapacity(unit.Destination, c.cell)).ThenBy(c => c.preference)
                    .SelectMany(c => _occupancy.Free(unit.Destination, unit.Faction, c.cell, unit.Profile.CapacityCost)
                        .OrderBy(f => unit.Faction == Faction.Adventurers ? f.AnchorForward : -f.AnchorForward).ThenBy(f => f.Id)
                        .Select(f => (HexPosition?)new HexPosition(c.cell, f.Id))).FirstOrDefault();
                if (position is null) continue;
                HexUnitState admitted = unit.Hex! with
                {
                    Lifecycle = UnitLifecycle.Alive,
                    Position = position.Value,
                    Action = unit.ReadyTick > _tick ? UnitActionKind.Recovery : UnitActionKind.Waiting,
                    EndTick = unit.ReadyTick
                };
                if (!_occupancy.TryPlace(admitted)) throw new InvalidOperationException("Placement lost its atomic reservation.");
                Spatial(unit.Id) = admitted; break;
            }
        }
        foreach (AdmissionBound bound in _admissions.Values.Where(b => b.AdmissionTick is null).ToArray())
        {
            UnitState? first = Snapshot().FirstOrDefault(u => u.Destination == bound.City && u.Faction == Faction.Skeletons && u.Deployed);
            if (first is not null) _admissions[bound.City] = bound with { AdmissionTick = _tick, FirstUnitId = first.Id };
        }
    }
    internal void TrackClearedAdmission(int city)
    {
        if (_admissions.TryGetValue(city, out AdmissionBound? previous) && previous.AdmissionTick is null) return;
        UnitState[] current = All();
        UnitState[] incoming = current.Where(u => u.Destination == city && u.Faction == Faction.Skeletons && u.Hex!.Lifecycle == UnitLifecycle.Queued).ToArray();
        if (incoming.Length == 0 || current.Any(u => u.Destination == city && u.Faction == Faction.Skeletons && u.Hex!.IsTargetable)) return;
        bool Fits(HexOccupancy index) => incoming.Any(u => _board.Rear(Faction.Skeletons).Any(cell => index.Free(city, Faction.Skeletons, cell, u.Profile.CapacityCost).Any()));
        long bound = _tick;
        if (!Fits(_occupancy))
        {
            long[] releases = current.Where(u => u.Destination == city && u.Faction == Faction.Skeletons && u.Hex!.Lifecycle == UnitLifecycle.Dying
                && (_board.Rear(Faction.Skeletons).Contains(u.Hex.Position.Cell) || _board.Rear(Faction.Skeletons).Contains(u.Hex.Destination.Cell)))
                .Select(u => u.Hex!.DeathEndTick).Distinct().Order().ToArray();
            bool found = false;
            foreach (long release in releases)
            {
                var predicted = new HexOccupancy(_board);
                predicted.Rebuild(current.Where(u => u.Hex!.Lifecycle != UnitLifecycle.Dying || u.Hex.DeathEndTick > release).Select(u => u.Hex!));
                if (!Fits(predicted)) continue;
                bound = release; found = true; break;
            }
            if (!found) throw new InvalidOperationException("Cleared protected entry has no finite first-admission bound.");
        }
        _admissions[city] = new(city, _tick, bound);
    }
    public void Cleanup(long tick)
    {
        _tick = tick; _events.RemoveAll(e => e.Tick < tick - HistoryTicks);
        foreach (UnitState unit in Dying()) if (_occupancy.ExpireDeath(unit.Hex!, tick)) Remove(unit.Id);
        TrimHistory();
    }
    public void Advance(long tick, IEnumerable<City> cities)
    {
        Cleanup(tick);
        foreach (UnitState unit in Snapshot().Where(u => u.Hex!.Lifecycle == UnitLifecycle.Alive))
        {
            HexUnitState hex = unit.Hex!;
            if (_occupancy.Arrive(hex, tick))
            {
                Spatial(unit.Id) = hex with { Position = hex.Destination, Destination = default, Transition = 0, Action = UnitActionKind.Waiting };
                CombatDecisionState d = Decision(unit.Id);
                Decision(unit.Id) = NewDecision(unit.Id, unit.Destination, checked(d.Sequence + 1)) with
                {
                    Generation = d.Generation,
                    ObjectiveId = d.ObjectiveId,
                    ObjectiveCity = d.ObjectiveCity,
                    ObjectiveCell = d.ObjectiveCell,
                    Route = d.Route.Skip(1).ToArray(),
                    Visited = d.Visited.Append(hex.Position.Cell).Distinct().ToArray()
                };
            }
            else if (hex.Action == UnitActionKind.Recovery && tick >= unit.ReadyTick)
            { Spatial(unit.Id) = hex with { Action = UnitActionKind.Waiting }; Decision(unit.Id) = NewDecision(unit.Id, unit.Destination, checked(unit.Decision!.Sequence + 1)); }
        }
        UnitState[] alive = Snapshot().Where(u => u.Hex!.IsTargetable).ToArray(); var current = alive.ToDictionary(u => u.Id);
        var damage = new SortedDictionary<int, int>(); var cityDamage = new SortedDictionary<int, int>();
        void Add(SortedDictionary<int, int> accumulator, int id, int amount) => accumulator[id] = checked(accumulator.GetValueOrDefault(id) + amount);
        foreach (UnitState unit in alive.Where(u => u.PendingImpact && tick >= u.ImpactTick))
        {
            UnitAttack attack = _world.Get<UnitAttack>(_entities[unit.Id]);
            bool exposed = !alive.Any(u => u.Destination == unit.Destination && u.Faction == Faction.Adventurers);
            UnitState? primary = current.GetValueOrDefault(attack.TargetId);
            bool valid = attack.TargetCity ? unit.Faction == Faction.Skeletons && attack.TargetId == unit.Destination && exposed
                    && _board.CityDistance(unit.Hex!.Position.Cell) <= unit.Profile.HexRange
                : primary is not null && primary.Faction != unit.Faction && primary.Destination == unit.Destination && InRange(unit, primary);
            int[] victims = valid && !attack.TargetCity ? Victims(primary!, alive, unit.Profile.VictimCap, unit.Profile.SplashHexRadius,
                Key(unit.Id, unit.Destination, unit.Decision!.Sequence, 0, CombatPurpose.Splash)) : [];
            if (valid && attack.TargetCity) Add(cityDamage, attack.TargetId, unit.Profile.Damage);
            foreach (int id in victims) Add(damage, id, unit.Profile.Damage);
            Emit(CombatEventType.Impact, unit, attack.TargetId, attack.TargetCity, unit.Profile.Damage, valid, primary, victims);
            _world.Get<UnitAttack>(_entities[unit.Id]) = attack with { Pending = false };
            Spatial(unit.Id) = unit.Hex! with { Action = UnitActionKind.Recovery };
        }
        foreach (City city in cities.Where(c => !c.Eliminated).OrderBy(c => c.Id))
        {
            ResolveDefense(city, city.Defender, alive, damage);
            foreach (TowerState tower in city.Towers.Values.OrderBy(t => t.Slot).ToArray()) ResolveDefense(city, tower, alive, damage);
        }
        foreach ((int id, int amount) in damage)
        {
            ref UnitHealth health = ref _world.Get<UnitHealth>(_entities[id]); int effective = Math.Min(health.Value, amount); health = new(health.Value - effective);
            if (effective > 0) Emit(CombatEventType.Hit, Read(id), damage: effective, landed: true);
        }
        foreach (City city in cities) city.Health = Math.Max(0, city.Health - cityDamage.GetValueOrDefault(city.Id));
        foreach (UnitState unit in Snapshot().Where(u => u.Health <= 0)) Kill(unit);
        TrimHistory();
    }
    private void ResolveDefense(City city, TowerState tower, UnitState[] alive, SortedDictionary<int, int> damage)
    {
        if (!tower.PendingImpact || _tick < tower.ImpactTick) return;
        bool defender = tower.Slot < 0;
        TowerDefinition? profile = defender ? null : _configuration.Towers.Single(p => p.Type == tower.Type && p.Level == tower.Level);
        int amount = profile?.Damage ?? _configuration.DefenderDamage;
        UnitState? primary = alive.FirstOrDefault(u => u.Id == tower.TargetId && u.Faction == Faction.Skeletons && u.Destination == city.Id);
        int[] victims = primary is null ? [] : Victims(primary, alive, profile?.VictimCap ?? 1, profile?.SplashHexRadius ?? 0,
            Key(defender ? city.Id : tower.Slot + 1, city.Id, tower.AttackSequence, 0, CombatPurpose.Splash, defender ? CombatActorKind.Defender : CombatActorKind.Tower));
        foreach (int id in victims) damage[id] = checked(damage.GetValueOrDefault(id) + amount);
        EmitTower(CombatEventType.Impact, tower, primary, amount, primary is not null, victims);
        if (defender) { city.Defender = tower with { PendingImpact = false }; if (primary is not null) Emit(CombatEventType.DefenderShot, primary, primary.Id, damage: amount, landed: true); }
        else city.Towers[tower.Slot] = tower with { PendingImpact = false };
    }
    public void StartActions(IEnumerable<City> cities)
    {
        UnitState[] alive = Snapshot().Where(u => u.Hex!.IsTargetable).ToArray(); var moves = new List<(UnitState Actor, HexPosition To)>();
        foreach (UnitState actor in alive.Where(u => u.Hex!.Action == UnitActionKind.Waiting && _tick >= u.ReadyTick))
        {
            UnitState[] opponents = alive.Where(u => u.Destination == actor.Destination && u.Faction != actor.Faction).ToArray();
            UnitState? target = Select(opponents.Where(u => InRange(actor, u)), u => _board.Distance(actor.Hex!.Position.Cell, u.Hex!.Position.Cell),
                Key(actor.Id, actor.Destination, actor.Decision!.Sequence, actor.Decision.Generation, CombatPurpose.Target));
            bool city = opponents.Length == 0 && actor.Faction == Faction.Skeletons;
            if (target is not null || city && _board.CityDistance(actor.Hex!.Position.Cell) <= actor.Profile.HexRange)
            { StartAttack(actor, target, city); continue; }
            if (opponents.Length == 0 && !city) continue;
            CombatDecisionState d = Decision(actor.Id);
            UnitState? objective = opponents.FirstOrDefault(u => u.Id == d.ObjectiveId);
            bool unchanged = d.ObjectiveId != 0 && (d.ObjectiveCity ? city : objective is not null && d.ObjectiveCell == objective.Hex!.Position.Cell);
            if (!unchanged) { d = d with { Generation = checked(d.Generation + 1), Route = [], Visited = [] }; Decision(actor.Id) = d; }
            if (d.Route.Length > 0 && _occupancy.CanPlace(actor.Destination, actor.Faction, d.Route[0]))
            { moves.Add((Read(actor.Id), d.Route[0])); continue; }
            if (unchanged && d.ObservedRevision == _occupancy.Revision && _tick < d.RetryTick) continue;
            CombatDecisionKey key = Key(actor.Id, actor.Destination, d.Sequence, d.Generation, CombatPurpose.Target);
            IEnumerable<UnitState?> targets = city ? [null] : unchanged && objective is not null ? [objective] : opponents.Cast<UnitState?>();
            Approach[] approaches = targets.Select(t => _routing.Find(actor, t, alive, key, unchanged ? d.Visited : [])).ToArray();
            Approach[] feasible = approaches.Where(a => a.Route.Length > 0).ToArray();
            Approach[] candidates = feasible.Length > 0 ? feasible : approaches;
            int best = candidates.Min(a => feasible.Length > 0 ? a.Route.Length : a.StaticSteps);
            candidates = candidates.Where(a => (feasible.Length > 0 ? a.Route.Length : a.StaticSteps) == best).ToArray();
            int initiative = candidates.Min(a => a.City ? 0 : opponents.Single(u => u.Id == a.TargetId).Profile.Initiative);
            candidates = candidates.Where(a => (a.City ? 0 : opponents.Single(u => u.Id == a.TargetId).Profile.Initiative) == initiative).OrderBy(a => a.TargetId).ToArray();
            Approach chosen = candidates[SeededDecision.Choose(key, candidates.Length)];
            Decision(actor.Id) = d with
            {
                ObjectiveId = chosen.TargetId,
                ObjectiveCity = chosen.City,
                ObjectiveCell = chosen.TargetCell,
                Route = chosen.Route,
                ObservedRevision = _occupancy.Revision,
                RetryTick = checked(_tick + _configuration.RetryTicks)
            };
            _world.Get<UnitTarget>(_entities[actor.Id]) = new(chosen.TargetId, chosen.City);
            if (chosen.Route.Length > 0) moves.Add((Read(actor.Id), chosen.Route[0]));
        }
        foreach (var move in moves.OrderBy(m => m.Actor.Profile.Initiative).ThenBy(m => m.Actor.Decision!.SchedulingRank).ThenBy(m => m.Actor.Id))
        {
            HexUnitState hex = move.Actor.Hex!; long sequence = checked(hex.ActionSequence + 1);
            if (!_occupancy.TryMove(hex, move.To, sequence)) continue;
            Spatial(hex.Id) = hex with
            {
                Action = UnitActionKind.Moving,
                Destination = move.To,
                Transition = _board.Transition(hex.Position, move.To).Id,
                ActionSequence = sequence,
                StartTick = _tick,
                EndTick = checked(_tick + move.Actor.Profile.MoveTicks)
            };
        }
        foreach (City city in cities.Where(c => !c.Eliminated).OrderBy(c => c.Id))
        {
            StartDefense(city, city.Defender, alive);
            foreach (TowerState tower in city.Towers.Values.OrderBy(t => t.Slot).ToArray()) StartDefense(city, tower, alive);
        }
        TrimHistory();
    }
    private void StartAttack(UnitState unit, UnitState? target, bool city)
    {
        Entity entity = _entities[unit.Id]; UnitAttack previous = _world.Get<UnitAttack>(entity); int targetId = target?.Id ?? unit.Destination;
        _world.Get<UnitTarget>(entity) = new(targetId, city);
        _world.Get<UnitAttack>(entity) = new(checked(previous.Sequence + 1), _tick, checked(_tick + unit.Profile.WindupTicks), checked(_tick + unit.Profile.CadenceTicks), true, targetId, city);
        Spatial(unit.Id) = unit.Hex! with { Action = UnitActionKind.Windup, ActionSequence = checked(unit.Hex!.ActionSequence + 1), StartTick = _tick, EndTick = checked(_tick + unit.Profile.CadenceTicks) };
        Emit(CombatEventType.AttackStarted, Read(unit.Id), targetId, city, primary: target);
    }
    private void StartDefense(City city, TowerState tower, UnitState[] alive)
    {
        if (tower.PendingImpact || _tick < tower.ReadyTick) return;
        bool defender = tower.Slot < 0;
        TowerDefinition? profile = defender ? null : _configuration.Towers.Single(p => p.Type == tower.Type && p.Level == tower.Level);
        int windup = profile?.WindupTicks ?? _configuration.Defender.WindupTicks, cadence = profile?.CadenceTicks ?? _configuration.Defender.CadenceTicks;
        UnitState? target = Select(alive.Where(u => u.Faction == Faction.Skeletons && u.Destination == city.Id), u => _board.CityDistance(u.Hex!.Position.Cell),
            Key(defender ? city.Id : tower.Slot + 1, city.Id, tower.AttackSequence, 0, CombatPurpose.Target, defender ? CombatActorKind.Defender : CombatActorKind.Tower));
        if (target is null) return;
        tower = tower with
        {
            AttackSequence = checked(tower.AttackSequence + 1),
            TargetId = target.Id,
            ActionStartTick = _tick,
            ImpactTick = checked(_tick + windup),
            ReadyTick = checked(_tick + cadence),
            PendingImpact = true
        };
        if (defender) city.Defender = tower; else city.Towers[tower.Slot] = tower;
        EmitTower(CombatEventType.AttackStarted, tower, target, profile?.Damage ?? _configuration.DefenderDamage, false, []);
    }
    private static UnitState? Select(IEnumerable<UnitState> candidates, Func<UnitState, int> distance, CombatDecisionKey key)
    {
        UnitState[] values = candidates.ToArray(); if (values.Length == 0) return null;
        int closest = values.Min(distance); values = values.Where(u => distance(u) == closest).ToArray();
        int initiative = values.Min(u => u.Profile.Initiative); values = values.Where(u => u.Profile.Initiative == initiative).OrderBy(u => u.Id).ToArray();
        return values[SeededDecision.Choose(key, values.Length)];
    }
    private bool InRange(UnitState actor, UnitState target)
    { int distance = _board.Distance(actor.Hex!.Position.Cell, target.Hex!.Position.Cell); return distance >= 1 && distance <= actor.Profile.HexRange; }
    private int[] Victims(UnitState primary, UnitState[] all, int cap, int radius, CombatDecisionKey key)
    {
        var result = new List<int> { primary.Id };
        var remaining = all.Where(u => u.Id != primary.Id && u.Destination == primary.Destination && u.Faction == primary.Faction
            && _board.Distance(primary.Hex!.Position.Cell, u.Hex!.Position.Cell) <= radius).ToList();
        while (result.Count < cap && remaining.Count > 0)
        {
            UnitState victim = Select(remaining, u => _board.Distance(primary.Hex!.Position.Cell, u.Hex!.Position.Cell), key with { Generation = result.Count })!;
            result.Add(victim.Id); remaining.Remove(victim);
        }
        return result.ToArray();
    }
    private void Kill(UnitState unit)
    {
        HexUnitState hex = unit.Hex!;
        _world.Get<UnitHealth>(_entities[unit.Id]) = new(0);
        _world.Get<UnitAttack>(_entities[unit.Id]) = _world.Get<UnitAttack>(_entities[unit.Id]) with { Pending = false };
        if (hex.Lifecycle == UnitLifecycle.Queued) { Emit(CombatEventType.Death, unit with { Health = 0 }); Remove(unit.Id); return; }
        Spatial(unit.Id) = hex with
        {
            Lifecycle = UnitLifecycle.Dying,
            DeathStartTick = _tick,
            DeathEndTick = checked(_tick + unit.Profile.DeathTicks),
            FrozenMoveTicks = hex.HoldsTransit ? checked((int)(_tick - hex.StartTick)) : 0
        };
        Emit(CombatEventType.Death, Read(unit.Id));
    }
    public void EliminateArmy(int city)
    { foreach (UnitState soldier in Soldiers(city)) Kill(soldier); TrimHistory(); }
    public void Transfer(int id, int destination)
    {
        HexUnitState hex = Spatial(id); if (hex.Lifecycle == UnitLifecycle.Dying) throw new InvalidOperationException("Dying units cannot transfer.");
        _occupancy.Release(hex.City, id);
        ref UnitIdentity identity = ref _world.Get<UnitIdentity>(_entities[id]); identity = identity with { Destination = destination };
        Spatial(id) = new(id, destination, hex.Faction, UnitLifecycle.Queued, default, ActionSequence: hex.ActionSequence);
        Decision(id) = NewDecision(id, destination, checked(Decision(id).Sequence + 1));
        _world.Get<UnitTarget>(_entities[id]) = default;
        _world.Get<UnitAttack>(_entities[id]) = _world.Get<UnitAttack>(_entities[id]) with { Pending = false, TargetId = 0, TargetCity = false };
    }
    public void Step(long tick, IEnumerable<City> cities)
    { City[] values = cities.ToArray(); Advance(tick, values); AdmitEntries(); StartActions(values); }
    private void Emit(CombatEventType type, UnitState unit, int target = 0, bool city = false, int damage = 0, bool landed = false, UnitState? primary = null, int[]? victims = null)
    {
        EventSequence = checked(EventSequence + 1);
        _events.Add(new(EventSequence, _tick, type, unit, target, city, damage, landed)
        { ImpactForward = primary?.Position ?? 0, ImpactLateral = primary?.Lateral ?? unit.Lateral, Victims = victims ?? [] });
    }
    private void EmitTower(CombatEventType type, TowerState tower, UnitState? target, int damage, bool landed, int[] victims)
    {
        EventSequence = checked(EventSequence + 1);
        _events.Add(new(EventSequence, _tick, type, null, tower.TargetId, false, damage, landed)
        { Tower = tower, ImpactForward = target?.Position ?? 0, ImpactLateral = target?.Lateral ?? 0, Victims = victims });
    }
    private void TrimHistory() { if (_events.Count > HistoryLimit) _events.RemoveRange(0, _events.Count - HistoryLimit); }
    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true; _entities.Clear(); _events.Clear(); _admissions.Clear(); _occupancy.Clear(); EcsWorld.Destroy(_world); GC.SuppressFinalize(this);
    }
}
