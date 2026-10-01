using Game.Core;
using Godot;

namespace Game;

// Bounded graphical-only effects. Cosmetic overload drops cues, never authoritative state.
internal sealed partial class VillageFeedback : Node3D
{
    private sealed record Effect(MeshInstance3D Node, Vector3 From, Vector3 To, double Start, double Duration, bool Projectile);
    private readonly List<Effect> _active = [];
    private readonly Queue<MeshInstance3D> _pool = new();
    private readonly List<AudioStreamPlayer> _voices = [];
    private readonly Dictionary<int, AudioStreamWav> _sounds = [];
    private int _cueCount, _dropped;
    private double _nextAmbience = 8;
    public override void _Ready()
    {
        for (int n = 0; n < PresentationLimits.BattleEffects; n++)
        {
            var node = new MeshInstance3D
            {
                Mesh = new SphereMesh { Radius = .07f, Height = .14f },
                Visible = false,
                MaterialOverride = new StandardMaterial3D { ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded, AlbedoColor = new("f3d995") }
            };
            AddChild(node); _pool.Enqueue(node);
        }
        for (int n = 0; n < PresentationLimits.SoundVoices; n++)
        { var voice = new AudioStreamPlayer { Bus = "Master", VolumeDb = -12 }; AddChild(voice); _voices.Add(voice); }
        for (int n = 0; n < 6; n++) _sounds[n] = new AudioStreamWav { Format = AudioStreamWav.FormatEnum.Format16Bits, MixRate = CueSynthesis.SampleRate, Data = CueSynthesis.Generate(n) };
    }
    public void Clear()
    {
        foreach (Effect effect in _active) { effect.Node.Visible = false; _pool.Enqueue(effect.Node); }
        _active.Clear(); StopVoices(); _nextAmbience = 8;
    }
    public void StopVoices() { foreach (AudioStreamPlayer voice in _voices) voice.Stop(); }
    private void Sound(int cue, bool audible)
    {
        if (!audible) return;
        AudioStreamPlayer? voice = _voices.FirstOrDefault(v => !v.Playing);
        if (voice is null) { _dropped++; return; }
        voice.Stream = _sounds[cue]; voice.Play(); _cueCount++;
    }
    private void Spawn(Vector3 from, Vector3 to, double time, double duration, bool projectile)
    {
        if (!_pool.TryDequeue(out MeshInstance3D? node)) { _dropped++; return; }
        node.Position = from; node.Scale = Vector3.One; node.Visible = true;
        _active.Add(new(node, from, to, time, duration, projectile));
    }
    public void Action(Command request, int focus, double seconds, bool audible)
    {
        if (request.City != focus || request.Slot is < 0 or >= 9) return;
        Vector3 point = new((focus - 1) * 40, 0, 0); point += VillageLayout.Slot(request.Slot) + new Vector3(0, .6f, 0);
        for (int n = 0; n < 5; n++) Spawn(point, point + new Vector3((n - 2) * .18f, .7f, (n % 2) * .2f), seconds, .5, false);
        Sound(request.Action == "research" ? 1 : 0, audible);
    }
    public void Combat(CombatEvent entry, int focus, double seconds, bool audible, Vector3? towerSource = null)
    {
        int city = entry.Tower?.City ?? entry.Unit?.Destination ?? 0;
        if (city != focus) return;
        Vector3 center = new((city - 1) * 40, 0, 0);
        Vector3 impact = center + new Vector3((float)entry.ImpactLateral, .5f, -(float)entry.ImpactForward);
        if (entry.Type == CombatEventType.AttackStarted && (entry.Tower is not null || entry.Unit?.Class is UnitClass.Ranged or UnitClass.Magic))
        {
            Vector3 source = entry.Tower is { } tower ? towerSource ?? center + VillageLayout.Slot(tower.Slot) + new Vector3(0, 1.4f, 0)
                : center + new Vector3((float)entry.Unit!.Lateral, .6f, -(float)entry.Unit.Position);
            Spawn(source, impact, seconds, Math.Max(.01, ((entry.Tower?.ImpactTick ?? entry.Unit!.ImpactTick) - entry.Tick) / (double)Match.StepsPerSecond), true);
        }
        if (entry.Type == CombatEventType.Impact && entry.Landed)
        {
            for (int n = 0; n < Math.Max(1, entry.Victims.Length); n++) Spawn(impact, impact + new Vector3((n - 1) * .2f, .35f, .1f), seconds, .2, false);
            Sound(entry.Tower?.Type == Building.CatapultTower ? 5 : entry.Unit?.Class == UnitClass.Magic ? 4 : 2, audible);
        }
        if (entry.Type == CombatEventType.Death) Sound(entry.Unit?.Faction == Faction.Skeletons ? 3 : 2, audible);
    }
    public void Sample(double seconds, bool running)
    {
        if (!running) StopVoices();
        if (running && seconds >= _nextAmbience) { Sound(4, true); _nextAmbience = seconds + 12; }
        foreach (Effect effect in _active.ToArray())
        {
            float fraction = (float)Math.Clamp((seconds - effect.Start) / effect.Duration, 0, 1);
            if (fraction >= 1) { effect.Node.Visible = false; _pool.Enqueue(effect.Node); _active.Remove(effect); continue; }
            effect.Node.Position = effect.From.Lerp(effect.To, fraction) + (effect.Projectile ? new Vector3(0, Mathf.Sin(fraction * Mathf.Pi) * .4f, 0) : Vector3.Zero);
            effect.Node.Scale = Vector3.One * (effect.Projectile ? 1 : 1 + fraction * 2);
        }
    }
    public object Observe() => new { Active = _active.Count, Voices = _voices.Count(v => v.Playing), CueCount = _cueCount, Dropped = _dropped, Bus = "Master", Positions = _active.Select(e => e.Node.Position.ToString()).ToArray() };
}
