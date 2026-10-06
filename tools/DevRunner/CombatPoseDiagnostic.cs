using System.Text.Json;
using Game.Core;

namespace DevRunner;

// Test-owned provenance only. Flags and acceptance remain in CombatCheckpoint.
internal sealed record CombatPoseFlags(bool Sword, bool Mage, bool Axe, bool Hit, bool DamagedBar, bool Recovery, bool InspectionChanged);
internal sealed record CombatPoseActor(int Id, UnitType Type, bool Dead, bool Visible, string Clip, double PoseSeconds,
    bool AttackActive, bool HitActive, UnitActionKind? Action, long ReadyTick, float X, float Z, int Health, int MaximumHealth)
{
    public long ImpactTick { get; init; }
    public long AttackSequence { get; init; }
    public long? ActionSequence { get; init; }
    public long? FrozenTick { get; init; }
}
internal sealed record CombatPoseWitness(string ResponseId, string Source, string MatchId, double Tick, long Revision,
    int City, int Width, int Height, float Zoom, float PanX, float PanZ, CombatPoseActor? Actor,
    long? ReceivedTick, long? ReceivedRevision, string? ReceivedMatchId, int? InspectedId, int? InspectedHealth)
{
    public Phase Phase { get; init; }
    public bool Paused { get; init; }
    public int Wave { get; init; }
    public int PlaybackGeneration { get; init; }
}

internal sealed class CombatPoseDiagnostic
{
    internal static readonly string[] FlagNames = ["sword", "Mage", "axe", "hit", "damagedBar", "recovery", "inspectionChanged"];
    private readonly Dictionary<string, CombatPoseWitness?> _first = [];
    private CombatPoseWitness? _recoveryPrevious;
    public CombatPoseFlags? Entry { get; private set; }
    public CombatPoseWitness? LastObservation { get; private set; }
    public int Polls { get; private set; }
    public CombatPoseWitness? FirstRecoveryCandidate { get; private set; }

    public void RecoveryCandidate(CombatPoseWitness witness)
    {
        if (FirstRecoveryCandidate is null && Fits(witness)) FirstRecoveryCandidate = witness;
    }

    public static CombatPoseWitness Witness(UiObservation frame, UnitObservation? actor, string source, MatchSnapshot? received = null)
        => new(frame.Id, source, frame.MatchId, frame.CombatTick, frame.Revision, frame.ObservedCity,
            frame.Width, frame.Height, frame.Camera.Zoom, frame.Camera.PanX, frame.Camera.PanZ,
            actor is null ? null : new(actor.Id, actor.Type, actor.Dead, actor.Visible, actor.Clip, actor.PoseSeconds,
                actor.AttackActive, actor.HitActive, actor.Hex?.Action, actor.ReadyTick, actor.X, actor.Z, actor.Health, actor.MaximumHealth)
            {
                ImpactTick = actor.ImpactTick,
                AttackSequence = actor.AttackSequence,
                ActionSequence = actor.Hex?.ActionSequence,
                FrozenTick = actor.Hex?.FrozenTick
            },
            received?.Tick, received?.Revision, received?.MatchId, frame.InspectedUnit?.Id, frame.InspectedUnit?.Health)
        { Phase = frame.MatchPhase, Paused = frame.Paused, Wave = frame.Wave, PlaybackGeneration = frame.PlaybackGeneration };

    private static bool Fits(CombatPoseWitness witness) => JsonSerializer.SerializeToUtf8Bytes(witness).Length <= 8192;

    public void First(string flag, CombatPoseWitness? witness)
    {
        if (!FlagNames.Contains(flag)) throw new ArgumentException("Unknown pose flag.", nameof(flag));
        if (!_first.ContainsKey(flag) && witness is not null) _first.Add(flag, Fits(witness) ? witness : null);
    }
    public void Recovery(CombatPoseWitness previous, CombatPoseWitness current)
    {
        if (_first.ContainsKey("recovery")) return;
        if (!Fits(previous) || !Fits(current)) { _first.Add("recovery", null); return; }
        _recoveryPrevious = previous;
        First("recovery", current);
    }
    public void Begin(CombatPoseFlags flags) { Entry = flags; Console.WriteLine("POSE entry: " + JsonSerializer.Serialize(flags)); }
    public void Poll(UiObservation frame, MatchSnapshot received)
    {
        Polls++;
        CombatPoseWitness witness = Witness(frame, null, "current-poll", received);
        LastObservation = Fits(witness) ? witness : null;
    }
    public string Finish(CombatPoseFlags flags, string result)
    {
        Console.WriteLine("POSE exit (" + result + "): " + JsonSerializer.Serialize(flags));
        return JsonSerializer.Serialize(new
        {
            Result = result,
            Entry,
            Exit = flags,
            Polls,
            LastObservation,
            Provenance = FlagNames.Select(name => new
            {
                Flag = name,
                FirstWitness = _first.GetValueOrDefault(name),
                MissingProvenance = _first.GetValueOrDefault(name) is null ? "No retained witness (absent or over 8 KiB); do not infer an actor or frame." : null
            }),
            FirstRecoveryCandidate,
            RecoveryPrevious = _recoveryPrevious,
            RecoveryCurrent = _first.GetValueOrDefault("recovery"),
            Limitations = "First witnesses among retained entry frames and actual ordered early-live/late-loop responses only; entry-frame received metadata unavailable. Received state is latest arrival at callback, not applied/presented telemetry. No historical reconstruction or new PNG."
        }, Evidence.JsonOptions);
    }
}
