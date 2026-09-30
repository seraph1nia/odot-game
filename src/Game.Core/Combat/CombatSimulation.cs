using Arch.Core;
using EcsWorld = Arch.Core.World;

namespace Game.Core;

// The only mutable soldier/enemy store. Buffers below live for one fixed step only.
internal sealed class CombatSimulation(Rules rules) : IDisposable
{
    public const double Width = 3.4;
    public const double Radius = 0.20;
    public const double Tolerance = 1e-6;
    public const int HistoryTicks = 120;
    public const int HistoryLimit = 4096;
    private readonly EcsWorld _world = EcsWorld.Create();
    private readonly Dictionary<int, Entity> _entities = [];
    private readonly List<CombatEvent> _events = [];
    private readonly QueryDescription _units = new QueryDescription().WithAll<UnitIdentity>();
    private int _nextId = 1;
    private long _tick;
    public long EventSequence { get; private set; }
    public bool IsDisposed { get; private set; }
    internal bool RegistryReleased => !ReferenceEquals(EcsWorld.Worlds[_world.Id], _world);

    public WeaponProfile Profile(UnitType type) => type switch
    {
        UnitType.Swordsman => new(rules.SoldierHealth, rules.SoldierDamage, rules.RecruitCost, Match.Reach, 1, rules.MeleeWindupTicks, rules.AttackTicks),
        UnitType.Crossbowman => new(rules.RangedHealth, rules.RangedDamage, rules.RangedRecruitCost, rules.RangedReach, 1, rules.RangedWindupTicks, rules.AttackTicks),
        UnitType.Enemy => new(rules.EnemyHealth, rules.EnemyDamage, 0, Match.Reach, 0.8, rules.MeleeWindupTicks, rules.AttackTicks),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    public int Create(UnitType type, int owner, int origin, int destination)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        int id = _nextId++;
        WeaponProfile profile = Profile(type);
        Entity entity = _world.Create(new UnitIdentity(id, type, owner, origin, destination), new UnitHealth(profile.Health),
            new UnitBody(type == UnitType.Enemy ? Match.LaneLength : 0, 0, false, FacingForward: type == UnitType.Enemy ? -1 : 1),
            new UnitTarget(), new UnitAttack(), profile);
        _entities.Add(id, entity);
        return id;
    }

    public UnitState[] Snapshot()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        var ids = new List<int>(_entities.Count);
        _world.Query(in _units, (ref UnitIdentity identity) => ids.Add(identity.Id));
        ids.Sort();
        return ids.Select(Read).ToArray();
    }
    public UnitState[] Soldiers(int city) => Snapshot().Where(u => u.Owner == city && u.Type != UnitType.Enemy).ToArray();
    public UnitState[] Enemies() => Snapshot().Where(u => u.Type == UnitType.Enemy).ToArray();
    public CombatEvent[] Events() => _events.ToArray();
    public long OldestEventSequence => _events.Count == 0 ? EventSequence + 1 : _events[0].Sequence;

    internal UnitState Read(int id)
    {
        Entity entity = _entities[id];
        UnitIdentity identity = _world.Get<UnitIdentity>(entity);
        UnitBody body = _world.Get<UnitBody>(entity);
        UnitTarget target = _world.Get<UnitTarget>(entity);
        UnitAttack attack = _world.Get<UnitAttack>(entity);
        return new(id, _world.Get<UnitHealth>(entity).Value, body.Forward, (int)Math.Max(0, attack.ReadyTick - _tick), identity.Origin, identity.Destination)
        {
            Type = identity.Type,
            Owner = identity.Owner,
            Lateral = body.Lateral,
            Deployed = body.Deployed,
            MoveForward = body.MoveForward,
            MoveLateral = body.MoveLateral,
            FacingForward = body.FacingForward,
            FacingLateral = body.FacingLateral,
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

    // Internal seam used by the test assembly's fixture builder, never exposed on the protocol.
    internal void Seed(UnitState state)
    {
        if (_entities.ContainsKey(state.Id)) Remove(state.Id);
        WeaponProfile profile = state.Profile.Health > 0 ? state.Profile : Profile(state.Type);
        Entity entity = _world.Create(new UnitIdentity(state.Id, state.Type, state.Owner, state.Origin, state.Destination), new UnitHealth(state.Health),
            new UnitBody(state.Position, state.Lateral, state.Deployed, state.MoveForward, state.MoveLateral, state.FacingForward, state.FacingLateral),
            new UnitTarget(state.TargetId, state.TargetCity),
            new UnitAttack(state.AttackSequence, state.ActionStartTick, state.ImpactTick, Math.Max(state.ReadyTick, _tick + state.Cooldown), state.PendingImpact, state.TargetId, state.TargetCity), profile);
        _entities.Add(state.Id, entity); _nextId = Math.Max(_nextId, state.Id + 1);
    }
    internal void Remove(int id)
    {
        if (_entities.Remove(id, out Entity entity)) _world.Destroy(entity);
    }

    public void BeginWave()
    {
        _events.Clear();
        foreach (UnitState unit in Snapshot())
        {
            ref UnitBody body = ref _world.Get<UnitBody>(_entities[unit.Id]);
            body = new(unit.Type == UnitType.Enemy ? Match.LaneLength : 0, 0, false, FacingForward: unit.Type == UnitType.Enemy ? -1 : 1);
            _world.Get<UnitTarget>(_entities[unit.Id]) = default;
            _world.Get<UnitAttack>(_entities[unit.Id]) = new(unit.AttackSequence, 0, 0, _tick, false, 0, false);
        }
        AdmitEntries();
    }

    public void AdmitEntries()
    {
        UnitState[] all = Snapshot();
        var occupied = all.Where(u => u.Deployed).ToList();
        foreach (UnitState unit in all.Where(u => !u.Deployed).OrderBy(u => u.Type == UnitType.Crossbowman).ThenBy(u => u.Id))
        {
            bool placed = false;
            for (int row = 0; row < 6 && !placed; row++)
                foreach (int column in new[] { 0, -1, 1, -2, 2, -3, 3 })
                {
                    double forward = unit.Type == UnitType.Enemy ? Match.LaneLength - Radius - row * 0.48
                        : unit.Type == UnitType.Crossbowman ? Radius + row * 0.48 : Radius + 5 * 0.48 - row * 0.48;
                    var point = new BattlePoint(forward, column * 0.48);
                    if (occupied.Any(other => other.Destination == unit.Destination && Point(other).Subtract(point).Length < 2 * Radius - Tolerance)) continue;
                    _world.Get<UnitBody>(_entities[unit.Id]) = new(forward, point.Lateral, true, FacingForward: unit.Type == UnitType.Enemy ? -1 : 1);
                    occupied.Add(unit with { Position = forward, Lateral = point.Lateral, Deployed = true }); placed = true; break;
                }
        }
    }

    public void Step(long tick, IEnumerable<City> cities)
    {
        _tick = tick;
        _events.RemoveAll(e => e.Tick < tick - HistoryTicks);
        AdmitEntries();
        UnitState[] read = Snapshot();
        var active = read.Where(u => u.Deployed && u.Health > 0).ToArray();
        var positions = new Dictionary<int, BattlePoint>();
        foreach (UnitState unit in active)
        {
            if (unit.PendingImpact)
            {
                positions[unit.Id] = Point(unit);
                ref UnitBody waiting = ref _world.Get<UnitBody>(_entities[unit.Id]);
                waiting = waiting with { MoveForward = 0, MoveLateral = 0 };
                continue;
            }
            UnitState[] opponents = active.Where(u => u.Destination == unit.Destination && IsEnemy(u) != IsEnemy(unit)).ToArray();
            UnitState? nearest = opponents.OrderBy(u => Point(u).Subtract(Point(unit)).LengthSquared).ThenBy(u => u.Id).FirstOrDefault();
            UnitState? target = opponents.FirstOrDefault(u => u.Id == unit.TargetId) ?? nearest;
            if (target is not null && nearest is not null && unit.Type != UnitType.Crossbowman && nearest.Id != target.Id
                && Intercepts(Point(unit), Point(target), Point(nearest))) target = nearest;
            bool cityTarget = target is null && IsEnemy(unit);
            _world.Get<UnitTarget>(_entities[unit.Id]) = new(target?.Id ?? (cityTarget ? unit.Destination : 0), cityTarget);
            BattlePoint from = Point(unit), goal = target is not null ? Point(target) : cityTarget ? new(0, unit.Lateral) : from;
            BattlePoint direction = goal.Subtract(from).Normalized();
            double distance = goal.Subtract(from).Length;
            double step = Math.Min(unit.Profile.Speed / Match.StepsPerSecond, Math.Max(0, distance - unit.Profile.Range));
            BattlePoint move = unit.PendingImpact ? default : direction.Scale(step);
            BattlePoint chosen = Constrain(unit, move, active, positions);
            if (step > 0 && !unit.PendingImpact && chosen.Length < step * 0.25 && FriendlyBlocker(unit, move, active))
            {
                double sign = unit.Id % 2 == 0 ? 1 : -1;
                var tangent = new BattlePoint(-direction.Lateral, direction.Forward);
                foreach (double side in new[] { sign, -sign })
                {
                    BattlePoint alternative = Constrain(unit, tangent.Scale(step * side), active, positions);
                    if (alternative.Length > chosen.Length) chosen = alternative;
                }
            }
            BattlePoint end = from.Add(chosen); positions[unit.Id] = end;
            _world.Get<UnitBody>(_entities[unit.Id]) = new(end.Forward, end.Lateral, true,
                chosen.Forward * Match.StepsPerSecond, chosen.Lateral * Match.StepsPerSecond,
                direction.LengthSquared > 0 ? direction.Forward : unit.FacingForward, direction.LengthSquared > 0 ? direction.Lateral : unit.FacingLateral);
        }

        UnitState[] moved = Snapshot().Where(u => u.Deployed).ToArray();
        var current = moved.ToDictionary(u => u.Id);
        var damage = new Dictionary<int, int>();
        var cityDamage = new Dictionary<int, int>();
        foreach (UnitState unit in moved)
        {
            Entity entity = _entities[unit.Id];
            UnitTarget target = _world.Get<UnitTarget>(entity);
            ref UnitAttack attack = ref _world.Get<UnitAttack>(entity);
            bool inRange = target.City ? unit.Position <= unit.Profile.Range + Tolerance
                : current.TryGetValue(target.Id, out UnitState? opponent) && Point(opponent).Subtract(Point(unit)).Length <= unit.Profile.Range + Tolerance;
            if (!attack.Pending && tick >= attack.ReadyTick && target.Id != 0 && inRange)
            {
                attack = new(attack.Sequence + 1, tick, tick + unit.Profile.WindupTicks, tick + unit.Profile.CadenceTicks, true, target.Id, target.City);
                Emit(CombatEventType.AttackStarted, Read(unit.Id), target.Id, target.City);
            }
            if (!attack.Pending || tick < attack.ImpactTick) continue;
            bool valid = attack.TargetCity ? attack.TargetId == unit.Destination && unit.Position <= unit.Profile.Range + Tolerance
                : current.TryGetValue(attack.TargetId, out UnitState? victim) && victim.Destination == unit.Destination && IsEnemy(victim) != IsEnemy(unit)
                    && Point(victim).Subtract(Point(unit)).Length <= unit.Profile.Range + Tolerance;
            if (valid)
            {
                Dictionary<int, int> hits = attack.TargetCity ? cityDamage : damage;
                hits[attack.TargetId] = hits.GetValueOrDefault(attack.TargetId) + unit.Profile.Damage;
            }
            Emit(CombatEventType.Impact, Read(unit.Id), attack.TargetId, attack.TargetCity, unit.Profile.Damage, valid);
            attack = attack with { Pending = false };
        }
        foreach (City city in cities.Where(c => !c.Eliminated).OrderBy(c => c.Id))
        {
            city.DefenderCooldown = Math.Max(0, city.DefenderCooldown - 1);
            UnitState? target = moved.Where(u => IsEnemy(u) && u.Destination == city.Id).OrderBy(u => u.Position).ThenBy(u => u.Id).FirstOrDefault();
            if (target is null || city.DefenderCooldown != 0) continue;
            damage[target.Id] = damage.GetValueOrDefault(target.Id) + rules.DefenderDamage;
            city.DefenderCooldown = rules.AttackTicks;
            Emit(CombatEventType.DefenderShot, target, target.Id, damage: rules.DefenderDamage, landed: true);
        }
        foreach ((int id, int amount) in damage.OrderBy(p => p.Key))
        {
            ref UnitHealth health = ref _world.Get<UnitHealth>(_entities[id]);
            health = new(Math.Max(0, health.Value - amount));
            if (amount > 0) Emit(CombatEventType.Hit, Read(id), damage: amount, landed: true);
        }
        foreach (City city in cities)
            city.Health = Math.Max(0, city.Health - cityDamage.GetValueOrDefault(city.Id));
        foreach (UnitState dead in Snapshot().Where(u => u.Health <= 0))
        { Emit(CombatEventType.Death, dead); Remove(dead.Id); }
        TrimHistory();
    }

    public void EliminateArmy(int city)
    {
        foreach (UnitState soldier in Soldiers(city))
        { Emit(CombatEventType.Death, soldier with { Health = 0 }); Remove(soldier.Id); }
        TrimHistory();
    }
    public void Transfer(int id, int destination)
    {
        Entity entity = _entities[id];
        ref UnitIdentity identity = ref _world.Get<UnitIdentity>(entity); identity = identity with { Destination = destination };
        _world.Get<UnitBody>(entity) = new(Match.LaneLength, 0, false, FacingForward: -1);
        _world.Get<UnitTarget>(entity) = default;
        ref UnitAttack attack = ref _world.Get<UnitAttack>(entity); attack = attack with { Pending = false, TargetId = 0, TargetCity = false };
    }
    private void Emit(CombatEventType type, UnitState unit, int target = 0, bool city = false, int damage = 0, bool landed = false)
        => _events.Add(new(++EventSequence, _tick, type, unit, target, city, damage, landed));
    private void TrimHistory() { if (_events.Count > HistoryLimit) _events.RemoveRange(0, _events.Count - HistoryLimit); }
    private static BattlePoint Point(UnitState unit) => new(unit.Position, unit.Lateral);
    private static bool IsEnemy(UnitState unit) => unit.Type == UnitType.Enemy;
    private static bool Intercepts(BattlePoint from, BattlePoint to, BattlePoint blocker)
    {
        BattlePoint ray = to.Subtract(from), relative = blocker.Subtract(from);
        if (ray.LengthSquared < 1e-12) return false;
        double fraction = relative.Dot(ray) / ray.LengthSquared;
        return fraction is > 0 and < 1 && relative.Subtract(ray.Scale(fraction)).Length < 2 * Radius;
    }
    private static bool FriendlyBlocker(UnitState unit, BattlePoint move, UnitState[] all)
        => all.Any(other => other.Id != unit.Id && other.Destination == unit.Destination && IsEnemy(unit) == IsEnemy(other)
            && Point(other).Subtract(Point(unit).Add(move)).Length < 2 * Radius + 0.01);

    private static BattlePoint Constrain(UnitState unit, BattlePoint move, UnitState[] all, Dictionary<int, BattlePoint> positions)
    {
        BattlePoint from = Point(unit);
        var bounded = new BattlePoint(Math.Clamp(from.Forward + move.Forward, Radius, Match.LaneLength - Radius),
            Math.Clamp(from.Lateral + move.Lateral, -Width / 2 + Radius, Width / 2 - Radius)).Subtract(from);
        bool Safe(double fraction)
        {
            BattlePoint delta = bounded.Scale(fraction);
            foreach (UnitState other in all)
            {
                if (other.Id == unit.Id || other.Destination != unit.Destination) continue;
                BattlePoint relative = from.Subtract(Point(other));
                BattlePoint otherMove = positions.TryGetValue(other.Id, out BattlePoint end) ? end.Subtract(Point(other)) : default;
                double envelope = 2 * Radius + delta.Length + otherMove.Length;
                if (relative.LengthSquared > envelope * envelope) continue;
                BattlePoint velocity = delta.Subtract(otherMove);
                double closest = velocity.LengthSquared > 1e-18 ? Math.Clamp(-relative.Dot(velocity) / velocity.LengthSquared, 0, 1) : 0;
                if (relative.Add(velocity.Scale(closest)).Length < 2 * Radius - Tolerance / 10) return false;
            }
            return true;
        }
        if (Safe(1)) return bounded;
        double low = 0, high = 1;
        for (int iteration = 0; iteration < 18; iteration++)
        { double middle = (low + high) / 2; if (Safe(middle)) low = middle; else high = middle; }
        return bounded.Scale(low);
    }

    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true; _entities.Clear(); _events.Clear(); EcsWorld.Destroy(_world); GC.SuppressFinalize(this);
    }
}
