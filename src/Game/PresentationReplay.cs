using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Game.Core;
using Godot;

namespace Game;

internal sealed record PresentationReplayFrame(int Index, double Delta, int Focus, MatchSnapshot? Snapshot);
internal sealed record PresentationReplayInput(string Version, string Digest, PresentationReplayFrame[] Frames, int Commands, long FinalTick);

// An owned diagnostic input adapter: no authority, RPC or command handler.
internal sealed class ReplaySession : IGameSession
{
    public MatchSnapshot? State { get; set; }
    public int PlayerId => 1;
    public bool Connected => true;
    public string Status => "Presentation replay";
    public string Feedback => "";
    public bool CanStart => false;
    public bool CanInvite => false;
    public int HostPlayerId => 1;
    public long SendAction(string action, int slot = -1, Building building = Building.Empty, int city = 0, UnitType soldierType = UnitType.Swordsman, UnitClass researchClass = UnitClass.Melee, Game.Core.Resource resource = Game.Core.Resource.Wood, int bundles = 0) => throw new InvalidOperationException("Replay input is read-only.");
    public Command[] DrainActionCues() => [];
    public void Connect(bool fresh = false) { }
}

internal sealed partial class PresentationReplay : Node
{
    private readonly PresentationReplayInput _input;
    private readonly string _output;
    private readonly ReplaySession _session = new();
    private readonly Tabletop _tabletop;
    private readonly WorkCounters? _work;
    private readonly Action _exit;
    private readonly List<double> _frames = new(600);
    private readonly Process _process = Process.GetCurrentProcess();
    private long _allocated, _start;
    private TimeSpan _cpu;
    private int[] _gc = [];
    private int _index;
    private readonly double _clockCalibrationMilliseconds;
    public PresentationReplay(GameApplication application, string inputPath, string outputPath, bool counters, Action exit)
    {
        if (new FileInfo(inputPath).Length > 64 * 1024 * 1024) throw new InvalidDataException("Replay input exceeds its bound.");
        _input = JsonSerializer.Deserialize<PresentationReplayInput>(System.IO.File.ReadAllText(inputPath), WireJson.Options) ?? throw new InvalidDataException("Missing replay input.");
        if (_input.Version != "ordinary-combat-replay-v1" || _input.Frames.Length != 600
            || _input.Frames.Where((frame, index) => frame.Index != index || frame.Delta != 1.0 / 60 || frame.Focus is not (1 or 2)).Any()
            || Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(_input.Frames, WireJson.Options))) != _input.Digest)
            throw new InvalidDataException("Invalid replay script or digest.");
        _output = outputPath; _work = counters ? new() : null; _exit = exit;
        _session.State = _input.Frames[0].Snapshot;
        _tabletop = application.ShowReplay(_session); _tabletop.SetProcess(false); _tabletop.SetWorkCounters(_work);
        _tabletop._Process(0); _work?.Reset();
        long calibration = Stopwatch.GetTimestamp();
        for (int sample = 0; sample < 100000; sample++) _ = Stopwatch.GetElapsedTime(Stopwatch.GetTimestamp());
        _clockCalibrationMilliseconds = Stopwatch.GetElapsedTime(calibration).TotalMilliseconds;
    }
    public override void _Process(double delta)
    {
        try
        {
            if (_index == 0)
            {
                _start = Stopwatch.GetTimestamp(); _cpu = _process.TotalProcessorTime;
                _allocated = GC.GetTotalAllocatedBytes(precise: true); _gc = [GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2)];
            }
            PresentationReplayFrame frame = _input.Frames[_index];
            if (frame.Snapshot is { } state) _session.State = state;
            _tabletop.ReplayFocus(frame.Focus);
            long? posesBefore = _work?.Snapshot()[WorkMetric.FullPoseSamples];
            long start = Stopwatch.GetTimestamp();
            _tabletop._Process(frame.Delta);
            _frames.Add(Stopwatch.GetElapsedTime(start).TotalMilliseconds);
            if (_work is not null && _work.Snapshot()[WorkMetric.FullPoseSamples] - posesBefore != _tabletop.ReplayVisibleViews)
                throw new InvalidOperationException("Replay must evaluate each visible rig exactly once per frame.");
            if (++_index != 600) return;
            SetProcess(false);
            double[] ordered = _frames.Order().ToArray();
            var result = new
            {
                Schema = "odot-presentation-profile-v1",
                _input.Version,
                InputDigest = _input.Digest,
                Frames = _index,
                _input.FinalTick,
                Configuration = "Debug",
                Renderer = RenderingServer.GetVideoAdapterName(),
                FramePacing = "native max-fps 30, scripted delta 1/60; managed update only, no native GPU claim",
                ClockCalibration = new { Samples = 100000, ElapsedMilliseconds = _clockCalibrationMilliseconds, NanosecondsPerFrameTimer = _clockCalibrationMilliseconds * 1000000 / 100000 },
                ElapsedMilliseconds = Stopwatch.GetElapsedTime(_start).TotalMilliseconds,
                CpuMilliseconds = (_process.TotalProcessorTime - _cpu).TotalMilliseconds,
                AllocatedBytes = GC.GetTotalAllocatedBytes(precise: true) - _allocated,
                Collections = new[] { GC.CollectionCount(0) - _gc[0], GC.CollectionCount(1) - _gc[1], GC.CollectionCount(2) - _gc[2] },
                FrameUpdateMilliseconds = _frames.ToArray(),
                P50 = ordered[299],
                P95 = ordered[569],
                P99 = ordered[593],
                Work = _work?.Snapshot()
            };
            System.IO.File.WriteAllText(_output, JsonSerializer.Serialize(result, WireJson.Options));
            Callable.From(Finish).CallDeferred();
        }
        catch (Exception e) { GD.PrintErr(e); GetTree().Quit(1); }
    }
    private async void Finish()
    {
        try
        {
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            using ViewportTexture texture = GetViewport().GetTexture();
            using Image image = texture.GetImage();
            if (image.SavePng(_output + ".png") != Error.Ok) throw new IOException("Replay capture failed.");
            _exit();
        }
        catch (Exception e) { GD.PrintErr(e); GetTree().Quit(1); }
    }
    public override void _ExitTree() => _process.Dispose();
}
