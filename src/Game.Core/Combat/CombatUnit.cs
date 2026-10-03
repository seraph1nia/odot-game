namespace Game.Core;

internal sealed record CombatLocation(UnitLifecycle Lifecycle, HexPosition Position, long AdmittedTick = 0,
    long DeathStartTick = 0, long DeathEndTick = 0, int FrozenMoveTicks = 0, long? FrozenTick = null, HexPosePoint? FrozenAim = null);

// This value is stored once, as an Arch component. Numerical policies receive
// immutable values; the serialized UnitState never serves as simulation input.
internal sealed record CombatUnit(UnitIdentity Identity, int Health, WeaponProfile Profile, CombatLocation Location,
    CombatAction Action, CombatDecisionState Decision)
{
    public int Id => Identity.Id;
    public UnitType Type => Identity.Type;
    public int Owner => Identity.Owner;
    public int Origin => Identity.Origin;
    public int Destination => Identity.Destination;
    public Faction Faction => Identity.Faction;
    public UnitClass Class => Catalogs.Class(Type);
    public bool Deployed => Location.Lifecycle is UnitLifecycle.Alive or UnitLifecycle.Dying;
    public bool IsTargetable => Location.Lifecycle == UnitLifecycle.Alive;
    public bool CanAct => IsTargetable && Location.FrozenTick is null;
    public bool PendingImpact => CanAct && Action is CombatAction.Windup;
    public long ReadyTick => Action.ReadyTick;
    public CombatTarget? Target => Action is CombatAction.Windup or CombatAction.Recovery ? Action.Attack?.Target
        : Decision.ObjectiveId > 0 ? new(Decision.ObjectiveCity ? CombatTargetKind.City : CombatTargetKind.Unit, Decision.ObjectiveId)
        : Action.Attack?.Target;
    public ReservationOwner Reservation => new(Id, Destination, Faction, Location.Lifecycle, Location.Position, Action.Sequence,
        Action is CombatAction.Waiting, Action as CombatAction.Moving, Location.DeathStartTick, Location.DeathEndTick, Location.FrozenMoveTicks, Profile.Size);
    public HexPosePoint? Pose(long tick)
    {
        if (!Deployed) return null;
        if (Action is not CombatAction.Moving move) return new(Location.Position);
        int duration = checked((int)(move.EndTick - move.StartTick));
        int elapsed = Location.Lifecycle == UnitLifecycle.Dying ? Location.FrozenMoveTicks
            : checked((int)Math.Clamp((Location.FrozenTick ?? tick) - move.StartTick, 0, duration));
        return new(Location.Position, move.Destination, move.Transition, elapsed, duration);
    }
}

// Occupancy receives only the evidence needed for size/anchor/transit ownership.
// It does not retain a competing action store or schedule attacks.
internal sealed record ReservationOwner(int Id, int City, Faction Faction, UnitLifecycle Lifecycle, HexPosition Position,
    long ActionSequence, bool Ready, CombatAction.Moving? Move = null, long DeathStartTick = 0, long DeathEndTick = 0, int FrozenMoveTicks = 0, int Size = 2)
{
    public bool HoldsTransit => Move is not null;
    public HexPosition Destination => Move?.Destination ?? default;
    public int Transition => Move?.Transition ?? 0;
    public long StartTick => Move?.StartTick ?? 0;
    public long EndTick => Move?.EndTick ?? 0;

    public static ReservationOwner FromSnapshot(HexUnitState state)
    {
        if (!Enum.IsDefined(state.Action)) throw new ArgumentException("Invalid projected action.", nameof(state));
        return new(state.Id, state.City, state.Faction, state.Lifecycle, state.Position,
            state.ActionSequence, state.Action == UnitActionKind.Waiting,
            state.HoldsTransit ? new(state.ActionSequence, state.Destination, state.Transition, state.StartTick, state.EndTick) : null,
            state.DeathStartTick, state.DeathEndTick, state.FrozenMoveTicks, state.Size);
    }
}

internal static class CombatProjection
{
    internal static UnitState Detach(UnitState unit, WorkCounters? work = null)
    {
        if (unit.Decision is not { } decision) return unit;
        work?.Add(WorkMetric.RouteElementsCopied, decision.Route.Length); work?.Add(WorkMetric.VisitedElementsCopied, decision.Visited.Length);
        return unit with { Decision = decision with { Route = decision.Route.ToArray(), Visited = decision.Visited.ToArray() } };
    }
    public static UnitState Snapshot(CombatUnit unit, long tick, WorkCounters? work = null)
    {
        work?.Add(WorkMetric.UnitProjections); work?.Add(WorkMetric.RouteElementsCopied, unit.Decision.Route.Length); work?.Add(WorkMetric.VisitedElementsCopied, unit.Decision.Visited.Length);
        CombatLocation location = unit.Location; CombatAction action = unit.Action;
        CombatAction.Moving? move = action as CombatAction.Moving;
        AttackRecord? attack = action.Attack; CombatTarget? target = unit.Target;
        return new(unit.Id, unit.Health, checked((int)Math.Max(0, action.ReadyTick - tick)), unit.Origin, unit.Destination)
        {
            Hex = new(unit.Id, unit.Destination, unit.Faction, location.Lifecycle, location.Position, action.Kind,
                move?.Destination ?? default, move?.Transition ?? 0, action.Sequence,
                move?.StartTick ?? attack?.StartTick ?? 0, move?.EndTick ?? action.ReadyTick,
                location.DeathStartTick, location.DeathEndTick, location.FrozenMoveTicks, location.AdmittedTick, location.FrozenTick, location.FrozenAim, unit.Profile.Size),
            Decision = unit.Decision with { Route = unit.Decision.Route.ToArray(), Visited = unit.Decision.Visited.ToArray() },
            Type = unit.Type,
            IsBoss = unit.Identity.IsBoss,
            Level = unit.Identity.Level,
            Faction = unit.Faction,
            Rank = unit.Identity.Rank,
            Owner = unit.Owner,
            Deployed = unit.Deployed,
            TargetId = target?.Id ?? 0,
            TargetCity = target?.Kind == CombatTargetKind.City,
            AttackSequence = action.AttackSequence,
            ActionStartTick = attack?.StartTick ?? 0,
            ImpactTick = attack?.ImpactTick ?? 0,
            ReadyTick = action.ReadyTick,
            PendingImpact = unit.PendingImpact,
            AttackLanded = attack?.Landed,
            Profile = unit.Profile
        };
    }

    // Deliberate fixture/restoration boundary; live rules never read this DTO.
    public static CombatUnit Restore(UnitState state, WeaponProfile profile, CombatDecisionState decision, long tick)
    {
        HexUnitState hex = state.Hex ?? (state.Deployed ? throw new ArgumentException("Deployed fixtures require explicit hex state.", nameof(state))
            : new(state.Id, state.Destination, state.Faction, UnitLifecycle.Queued, default, Size: profile.Size));
        if (hex.Id != state.Id || hex.City != state.Destination || hex.Faction != state.Faction || hex.Size != profile.Size || !Enum.IsDefined(hex.Action))
            throw new ArgumentException("Fixture identity/action mismatch.", nameof(state));
        long ready = Math.Max(state.ReadyTick, checked(tick + state.Cooldown));
        AttackRecord? attack = null;
        if (state.AttackSequence > 0 && state.TargetId > 0)
        {
            long start = state.ActionStartTick != 0 ? state.ActionStartTick : hex.StartTick;
            long impact = state.ImpactTick != 0 ? state.ImpactTick : checked(start + profile.WindupTicks);
            attack = new(new(state.TargetCity ? CombatTargetKind.City : CombatTargetKind.Unit, state.TargetId), start, impact, state.AttackLanded);
        }
        bool pending = hex.Action == UnitActionKind.Windup && hex.Lifecycle == UnitLifecycle.Alive && hex.FrozenTick is null;
        if (state.PendingImpact != pending || hex.Action == UnitActionKind.Windup && attack is null
            || hex.Action == UnitActionKind.Windup && (hex.EndTick != 0 && hex.EndTick != ready
                || state.ActionStartTick != 0 && hex.StartTick != state.ActionStartTick))
            throw new ArgumentException("A pending impact requires a matching attack action.", nameof(state));
        CombatAction action = hex.Action switch
        {
            UnitActionKind.Moving => new CombatAction.Moving(hex.ActionSequence, hex.Destination, hex.Transition, hex.StartTick, hex.EndTick,
                state.AttackSequence, ready, attack),
            UnitActionKind.Windup when attack is not null => new CombatAction.Windup(hex.ActionSequence, state.AttackSequence, ready, attack),
            UnitActionKind.Recovery => new CombatAction.Recovery(hex.ActionSequence, state.AttackSequence, ready, attack),
            _ => ready > tick ? new CombatAction.Recovery(hex.ActionSequence, state.AttackSequence, ready, attack)
                : new CombatAction.Waiting(hex.ActionSequence, state.AttackSequence, ready, attack)
        };
        return new(new(state.Id, state.Type, state.Owner, state.Origin, state.Destination, state.Faction, state.Rank, state.IsBoss, state.Level), state.Health, profile,
            new(hex.Lifecycle, hex.Position, hex.AdmittedTick, hex.DeathStartTick, hex.DeathEndTick, hex.FrozenMoveTicks, hex.FrozenTick, hex.FrozenAim), action, decision);
    }
}
