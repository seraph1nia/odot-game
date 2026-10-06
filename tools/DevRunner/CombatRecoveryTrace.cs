using Game.Core;

namespace DevRunner;

// Diagnostic custody only: no serialization, clock sampling or I/O in live callbacks.
internal sealed class CombatRecoveryTrace
{
    internal const int MaximumCallbacks = 64;
    internal const long MaximumBudgetBytes = 1024 * 1024;
    private readonly List<Callback> _callbacks = [];
    public long CallbackCount { get; private set; }
    public long? FirstOrdinal { get; private set; }
    public long? LastOrdinal { get; private set; }
    public long DroppedCallbacks { get; private set; }
    public long DroppedActors { get; private set; }
    public long DroppedPriorCandidates { get; private set; }
    public long DroppedBudgetBytes { get; private set; }
    public long RetainedBudgetBytes { get; private set; }
    public IReadOnlyList<Callback> Callbacks => _callbacks;
    public int CallbackLimit { get; } = MaximumCallbacks;
    public long ByteBudgetLimit { get; } = MaximumBudgetBytes;
    public bool Truncated => DroppedCallbacks != 0;
    public string Availability { get; } = "Response and presented fields are actual callback input; received fields are latest arrival, not presented state. Callback/request monotonic timestamps unavailable at this interface; not synthesized. No sampling outside the two existing early-live callbacks.";
    public string ByteAccounting { get; } = "Conservative compact-JSON storage budget: 2048 bytes per header/actor/prior candidate plus six bytes per UTF-16 string code unit (maximum JSON escaping). Whole callbacks are retained or explicitly counted as dropped; no 8 KiB witness filter. Serialization occurs only at existing Finish boundaries.";

    internal sealed class ActorRecord(UnitObservation unit)
    {
        public CombatPoseActor Actor { get; } = CombatPoseDiagnostic.Witness(new(), unit, "").Actor!;
        public int? HexId { get; } = unit.Hex?.Id;
        public int? HexCity { get; } = unit.Hex?.City;
        public UnitLifecycle? Lifecycle { get; } = unit.Hex?.Lifecycle;
        public long ActionStartTick { get; } = unit.ActionStartTick;
        public double AttackBlend { get; } = unit.AttackBlend;
        public double HitBlend { get; } = unit.HitBlend;
        public double WalkingBlend { get; } = unit.WalkingBlend;
        public string Decision { get; set; } = unit.Visible ? "not-evaluated" : "hidden:not-evaluated";
    }
    internal sealed class PriorRecord(UnitObservation unit, CombatPoseWitness witness, UnitObservation? current, double tick)
    {
        public CombatPoseWitness Witness { get; } = witness;
        public ActorRecord RenderedActor { get; } = new(unit) { Decision = "retained-eligible-candidate" };
        public bool WindowExpiredAtCallback { get; } = tick >= witness.Actor!.ReadyTick;
        public string Decision { get; set; } = current is null ? "actor-absent" : current.Visible ? "not-compared" : "hidden:not-evaluated";
    }
    internal sealed class Callback(long ordinal, bool connected, CombatPoseWitness frame, CombatPoseWitness? last,
        ActorRecord[] actors, PriorRecord[] previous)
    {
        public long Ordinal { get; } = ordinal;
        public bool Connected { get; } = connected;
        public CombatPoseWitness Frame { get; } = frame;
        public CombatPoseWitness? PreviousFrame { get; } = last;
        public bool Fresh { get; set; }
        public bool SameWindow { get; set; }
        public string[] WindowChanges { get; set; } = [];
        public string Decision { get; set; } = "entered";
        public bool Proven { get; set; }
        public bool? WaitPredicateAccepted { get; set; }
        public long? CallbackMonotonicTimestamp { get; }
        public long? RequestMonotonicTimestamp { get; }
        public ActorRecord[] Actors { get; } = actors;
        public PriorRecord[] PreviousCandidates { get; } = previous;
        internal void Actor(UnitObservation unit, string decision)
        {
            Actors.First(a => a.Actor.Id == unit.Id).Decision = decision;
            foreach (PriorRecord prior in PreviousCandidates.Where(p => p.Witness.Actor!.Id == unit.Id))
                if (prior.Decision == "not-compared") prior.Decision = decision;
        }
        internal void End(string decision, bool proven) { Decision = decision; Proven = proven; }
    }

    internal Callback? Begin(UiObservation frame, MatchSnapshot received, string source, UiObservation? last,
        Dictionary<int, (UnitObservation Unit, CombatPoseWitness Witness)> previous)
    {
        if (source is not ("early-live:locomotion" or "early-live:attack")) return null;
        long ordinal = ++CallbackCount;
        FirstOrdinal ??= ordinal;
        LastOrdinal = ordinal;
        long bytes = 2048 + Strings(frame.Id, source, frame.MatchId, received.MatchId)
            + (last is null ? 0 : 2048 + Strings(last.Id, last.MatchId));
        foreach (UnitObservation unit in frame.Units) bytes += 2048 + Strings(unit.Clip);
        foreach (var prior in previous.Values)
            bytes += 2048 + Strings(prior.Witness.ResponseId, prior.Witness.Source, prior.Witness.MatchId,
                prior.Witness.ReceivedMatchId, prior.Witness.Actor?.Clip, prior.Unit.Clip);
        if (_callbacks.Count >= MaximumCallbacks || bytes > MaximumBudgetBytes - RetainedBudgetBytes)
        {
            DroppedCallbacks++;
            DroppedActors += frame.Units.Length;
            DroppedPriorCandidates += previous.Count;
            DroppedBudgetBytes += bytes;
            return null;
        }
        var callback = new Callback(ordinal, frame.Connected, CombatPoseDiagnostic.Witness(frame, null, source, received),
            last is null ? null : CombatPoseDiagnostic.Witness(last, null, "previous-collector-frame"),
            frame.Units.Select(u => new ActorRecord(u)).ToArray(),
            previous.Values.Select(p => new PriorRecord(p.Unit, p.Witness, frame.Units.FirstOrDefault(u => u.Id == p.Unit.Id), frame.CombatTick)).ToArray());
        _callbacks.Add(callback);
        RetainedBudgetBytes += bytes;
        return callback;
    }
    internal void PredicateResult(bool accepted)
    {
        if (_callbacks.LastOrDefault() is { } callback && callback.Ordinal == LastOrdinal)
            callback.WaitPredicateAccepted = accepted;
    }
    private static long Strings(params string?[] strings) => strings.Sum(s => 6L * (s?.Length ?? 0));

    internal static string Ineligible(UiObservation frame, UnitObservation unit)
    {
        var reasons = new List<string>();
        if (unit.Dead) reasons.Add("dead");
        if (unit.Health <= 0) reasons.Add("nonpositive-health");
        if (unit.Hex is not { } hex) reasons.Add("missing-hex");
        else
        {
            if (hex.Lifecycle != UnitLifecycle.Alive) reasons.Add("non-alive-lifecycle");
            if (hex.Action != UnitActionKind.Recovery) reasons.Add("action:" + hex.Action);
            if (hex.FrozenTick is not null) reasons.Add("frozen-action");
            if (hex.Id != unit.Id) reasons.Add("actor-hex-mismatch");
            if (hex.City != frame.ObservedCity) reasons.Add("city-mismatch");
        }
        if (unit.AttackSequence <= 0) reasons.Add("noncurrent-attack");
        if (frame.CombatTick < unit.ImpactTick) reasons.Add("before-impact");
        if (frame.CombatTick >= unit.ReadyTick) reasons.Add("expired");
        if (!unit.AttackActive) reasons.Add("inactive-attack-layer");
        if (unit.Clip is not ("attack" or "hit")) reasons.Add("noncombat-clip");
        return "ineligible:" + string.Join(",", reasons);
    }
    internal static string Mismatch(UnitObservation previous, UnitObservation current)
    {
        var reasons = new List<string>();
        if (previous.ReadyTick != current.ReadyTick) reasons.Add("ready-tick");
        if (previous.AttackSequence != current.AttackSequence) reasons.Add("attack-sequence");
        if (previous.Hex!.ActionSequence != current.Hex!.ActionSequence) reasons.Add("action-sequence");
        if (previous.ImpactTick != current.ImpactTick) reasons.Add("impact-tick");
        return "candidate-identity-mismatch:" + string.Join(",", reasons);
    }
}
