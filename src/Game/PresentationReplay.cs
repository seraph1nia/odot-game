using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Game.Core;
using Godot;

namespace Game;

internal sealed record PresentationReplayFrame(int Index, double Delta, int Focus, MatchSnapshot? Snapshot,
    [property: System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)] string? View = null,
    [property: System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)] float Zoom = 0);
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
    public long SendAction(string action, int slot = -1, Building building = Building.Empty, int city = 0, UnitType soldierType = UnitType.Swordsman, TechnologyId technology = TechnologyId.None, Game.Core.Resource resource = Game.Core.Resource.Wood, int bundles = 0, ConstructionPayment payment = ConstructionPayment.Standard, int unitId = 0) => throw new InvalidOperationException("Replay input is read-only.");
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
    private readonly List<object> _samples = new(600);
    private readonly double _loadingMilliseconds;
    private readonly Process _process = Process.GetCurrentProcess();
    private long _allocated, _start, _previousFrame;
    private TimeSpan _cpu;
    private int[] _gc = [];
    private int _index;
    private readonly double _clockCalibrationMilliseconds;
    public PresentationReplay(GameApplication application, string inputPath, string outputPath, bool counters, Action exit)
    {
        long loading = Stopwatch.GetTimestamp();
        if (new FileInfo(inputPath).Length > 64 * 1024 * 1024) throw new InvalidDataException("Replay input exceeds its bound.");
        _input = JsonSerializer.Deserialize<PresentationReplayInput>(System.IO.File.ReadAllText(inputPath), WireJson.Options) ?? throw new InvalidDataException("Missing replay input.");
        if (_input.Version is not ("ordinary-combat-replay-v1" or "authored-scale-replay-v1") || _input.Frames.Length != 600
            || _input.Frames.Where((frame, index) => frame.Index != index || frame.Delta != 1.0 / 60 || frame.Focus is not (1 or 2) || (_input.Version == "authored-scale-replay-v1" ? frame.Zoom is not (1 or 3) : frame.Zoom != 0)).Any()
            || Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(_input.Frames, WireJson.Options))) != _input.Digest)
            throw new InvalidDataException("Invalid replay script or digest.");
        _output = outputPath; _work = counters ? new() : null; _exit = exit;
        _session.State = _input.Frames[0].Snapshot;
        _tabletop = application.ShowReplay(_session); _tabletop.SetProcess(false); _tabletop.SetWorkCounters(_work);
        _tabletop._Process(0); _work?.Reset();
        _loadingMilliseconds = Stopwatch.GetElapsedTime(loading).TotalMilliseconds;
        if (_input.Version == "authored-scale-replay-v1")
        {
            _tabletop.ReplayCosts = new(StringComparer.Ordinal);
        }
        long calibration = Stopwatch.GetTimestamp();
        for (int sample = 0; sample < 100000; sample++) _ = Stopwatch.GetElapsedTime(Stopwatch.GetTimestamp());
        _clockCalibrationMilliseconds = Stopwatch.GetElapsedTime(calibration).TotalMilliseconds;
    }
    public override void _Ready()
    {
        if (_tabletop.ReplayCosts is not null)
            RenderingServer.ViewportSetMeasureRenderTime(GetViewport().GetViewportRid(), true);
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
            long frameStamp = Stopwatch.GetTimestamp();
            double wallFrame = _previousFrame == 0 ? 0 : (frameStamp - _previousFrame) * 1000.0 / Stopwatch.Frequency;
            _previousFrame = frameStamp;
            PresentationReplayFrame frame = _input.Frames[_index];
            if (frame.Snapshot is { } state) _session.State = state;
            _tabletop.ReplayFocus(frame.Focus);
            long? posesBefore = _work?.Snapshot()[WorkMetric.FullPoseSamples];
            long allocation = GC.GetAllocatedBytesForCurrentThread();
            long start = Stopwatch.GetTimestamp();
            _tabletop._Process(frame.Delta);
            double update = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            long updateBytes = GC.GetAllocatedBytesForCurrentThread() - allocation;
            _frames.Add(update);
            if (_tabletop.ReplayCosts is { } costs)
            {
                _samples.Add(new
                {
                    frame.Index,
                    frame.View,
                    frame.Zoom,
                    Settled = frame.Index % 50 >= 10,
                    FrameMilliseconds = wallFrame,
                    EngineDeltaMilliseconds = delta * 1000,
                    UpdateMilliseconds = update,
                    UpdateBytes = updateBytes,
                    Phases = new Dictionary<string, double>(costs),
                    VisibleViews = _tabletop.ReplayVisibleViews,
                    StoredUnits = _session.State!.Players.Sum(c => c.Soldiers.Length) + _session.State.Enemies.Length,
                    PhysicsMilliseconds = Performance.GetMonitor(Performance.Monitor.TimePhysicsProcess) * 1000,
                    PhysicsObjects = Performance.GetMonitor(Performance.Monitor.Physics3DActiveObjects),
                    CollisionPairs = Performance.GetMonitor(Performance.Monitor.Physics3DCollisionPairs),
                    RenderCpuMilliseconds = RenderingServer.ViewportGetMeasuredRenderTimeCpu(GetViewport().GetViewportRid()),
                    RenderGpuMilliseconds = RenderingServer.ViewportGetMeasuredRenderTimeGpu(GetViewport().GetViewportRid()),
                    DrawCalls = RenderingServer.GetRenderingInfo(RenderingServer.RenderingInfo.TotalDrawCallsInFrame),
                    Primitives = RenderingServer.GetRenderingInfo(RenderingServer.RenderingInfo.TotalPrimitivesInFrame),
                    TextureBytes = RenderingServer.GetRenderingInfo(RenderingServer.RenderingInfo.TextureMemUsed),
                    BufferBytes = RenderingServer.GetRenderingInfo(RenderingServer.RenderingInfo.BufferMemUsed),
                    Nodes = Performance.GetMonitor(Performance.Monitor.ObjectNodeCount),
                    Resources = Performance.GetMonitor(Performance.Monitor.ObjectResourceCount),
                    NativeStaticBytes = Performance.GetMonitor(Performance.Monitor.MemoryStatic)
                });
                _tabletop.ReplayZoom(frame.Zoom);
            }
            if (_work is not null && _work.Snapshot()[WorkMetric.FullPoseSamples] - posesBefore != _tabletop.ReplayVisibleViews)
                throw new InvalidOperationException("Replay must evaluate each visible rig exactly once per frame.");
            if (++_index != 600)
            {
                if (_tabletop.ReplayCosts is not null && _index % 50 == 0)
                {
                    SetProcess(false);
                    Callable.From(CaptureView).CallDeferred();
                }
                return;
            }
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
                LoadingMilliseconds = _loadingMilliseconds,
                Samples = _samples.ToArray(),
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
    private async void CaptureView()
    {
        try
        {
            await Capture(_output + "." + _input.Frames[_index - 1].View + ".zoom" + _input.Frames[_index - 1].Zoom);
            SetProcess(true);
        }
        catch (Exception e) { GD.PrintErr(e); GetTree().Quit(1); }
    }
    private async Task Capture(string path)
    {
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        using ViewportTexture texture = GetViewport().GetTexture();
        using Image image = texture.GetImage();
        if (image.SavePng(path + ".png") != Error.Ok) throw new IOException("Replay capture failed.");
        if (_tabletop.ReplayCosts is not null)
        {
            var fields = new Dictionary<string, object?>(); var targets = new Dictionary<string, object>();
            _tabletop.AppendUiObservation(fields, targets);
            System.IO.File.WriteAllText(path + ".observation.json", JsonSerializer.Serialize(new { InputDigest = _input.Digest, Frame = _index - 1, Fields = fields }, WireJson.Options));
        }
    }
    private async void Finish()
    {
        try
        {
            await Capture(_output);
            _exit();
        }
        catch (Exception e) { GD.PrintErr(e); GetTree().Quit(1); }
    }
    public override void _ExitTree() => _process.Dispose();
}
