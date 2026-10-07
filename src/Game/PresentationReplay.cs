using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
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
    private readonly string? _baseline;
    private readonly string? _boundaryEvidence;
    private readonly List<object> _boundaryValidation = [];
    private readonly string? _ownership;
    private readonly bool _normalHistoryPrefix;
    private readonly int _ownershipFrame = 299;
    private readonly bool _remainingViews;
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
    public PresentationReplay(GameApplication application, string inputPath, string outputPath, bool counters, Action exit, string? baseline = null, string? ownership = null, string? boundaryEvidence = null)
    {
        long loading = Stopwatch.GetTimestamp();
        if (new FileInfo(inputPath).Length > 64 * 1024 * 1024) throw new InvalidDataException("Replay input exceeds its bound.");
        _input = JsonSerializer.Deserialize<PresentationReplayInput>(System.IO.File.ReadAllText(inputPath), WireJson.Options) ?? throw new InvalidDataException("Missing replay input.");
        bool refused;
        if (ownership is not null && JsonNode.Parse(System.IO.File.ReadAllText(ownership))!["AdmissionReceipt"]?.GetValue<bool>() == true)
        {
            var details = new JsonObject();
            refused = InvalidAdmission(_input, details, out byte[]? evaluatedPayload);
            try { WriteAdmissionReceipt(inputPath, outputPath, details, evaluatedPayload, refused); }
            catch (Exception receiptError) { GD.PrintErr("Admission receipt unavailable: " + receiptError.Message); if (!refused) throw; }
        }
        else
            refused = _input.Version is not ("ordinary-combat-replay-v1" or "authored-scale-replay-v1") || _input.Frames.Length != 600
                || _input.Frames.Where((frame, index) => frame.Index != index || frame.Delta != 1.0 / 60 || frame.Focus is not (1 or 2) || (_input.Version == "authored-scale-replay-v1" ? frame.Zoom is not (1 or 3) : frame.Zoom != 0)).Any()
                || Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(_input.Frames, WireJson.Options))) != _input.Digest;
        if (refused) throw new InvalidDataException("Invalid replay script or digest.");
        if (baseline is not null && (_input.Version != "authored-scale-replay-v1" || !Directory.Exists(baseline))) throw new InvalidDataException("Boundary proof needs the matching authored diagnostic baseline.");
        if (ownership is not null && (baseline is not null || _input.Version != "authored-scale-replay-v1")) throw new InvalidDataException("Ownership is an isolated diagnostic, not a comparison retry.");
        if (boundaryEvidence is not null && (baseline is null || ownership is not null || !System.IO.File.Exists(boundaryEvidence)))
            throw new InvalidDataException("Native family evidence requires an ordinary matching baseline comparison.");
        _boundaryEvidence = boundaryEvidence;
        _ownership = ownership;
        _normalHistoryPrefix = ownership is not null && System.Text.Json.Nodes.JsonNode.Parse(System.IO.File.ReadAllText(ownership))!["NormalPrefix"]?.GetValue<bool>() == true;
        if (ownership is not null) _ownershipFrame = System.Text.Json.Nodes.JsonNode.Parse(System.IO.File.ReadAllText(ownership))!["TargetFrame"]?.GetValue<int>() ?? 299;
        _remainingViews = ownership is not null && JsonNode.Parse(System.IO.File.ReadAllText(ownership))!["RemainingViews"]?.GetValue<bool>() == true;
        if (_ownershipFrame is not (299 or 399 or 499 or 599) || _ownershipFrame != 299 && !_normalHistoryPrefix || _remainingViews && (_ownershipFrame is not (499 or 599) || !_normalHistoryPrefix)
            || _ownershipFrame == 599 && !_remainingViews)
            throw new InvalidDataException("Ownership target requires its authorized ordinary rendered prefix; remaining views are only499+599.");
        _baseline = baseline;
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
    // Bounded diagnostic invocation only. Same conditions, order, and nested
    // short-circuit as the untouched ordinary guard above; each evaluated
    // value/result is captured once. Never revisit an earlier failure.
    internal static bool InvalidAdmission(PresentationReplayInput input, JsonObject details, out byte[]? evaluatedPayload)
    {
        byte[]? bytes = null;
        bool Check<T>(JsonObject target, string name, T value, Func<T, bool> condition)
        {
            bool invalid = condition(value);
            target[name] = JsonSerializer.SerializeToNode(new { Value = value, Refused = invalid }, WireJson.Options);
            if (invalid && details["FirstFailedClause"] is null) details["FirstFailedClause"] = name;
            return invalid;
        }
        bool FrameInvalid(PresentationReplayFrame frame, int index)
        {
            var trace = new JsonObject { ["Ordinal"] = index };
            details["EvaluatedFrames"]!.AsArray().Add(trace);
            return Check(trace, "Index", frame.Index, value => value != index)
                || Check(trace, "Delta", frame.Delta, value => value != 1.0 / 60)
                || Check(trace, "Focus", frame.Focus, value => value is not (1 or 2))
                || Check(trace, "Zoom", frame.Zoom, value => input.Version == "authored-scale-replay-v1" ? value is not (1 or 3) : value != 0);
        }
        bool FramesInvalid(int ignored)
        {
            details["EvaluatedFrames"] = new JsonArray();
            return input.Frames.Where(FrameInvalid).Any();
        }
        string Compute()
        {
            bytes = JsonSerializer.SerializeToUtf8Bytes(input.Frames, WireJson.Options);
            return Convert.ToHexString(SHA256.HashData(bytes));
        }
        details["DeclaredDigest"] = input.Digest;
        bool refused = Check(details, "Version", input.Version, value => value is not ("ordinary-combat-replay-v1" or "authored-scale-replay-v1"))
            || Check(details, "FrameCount", input.Frames.Length, value => value != 600)
            || Check(details, "Frames", 0, FramesInvalid)
            || Check(details, "ComputedDigest", Compute(), value => value != input.Digest);
        evaluatedPayload = bytes;
        details["Refused"] = refused;
        return refused;
    }
    private static void WriteAdmissionReceipt(string inputPath, string outputPath, JsonObject details, byte[]? evaluatedPayload, bool refused)
    {
        PersistAdmissionReceipt(outputPath, details, evaluatedPayload, refused, () => new
        {
            Schema = "owned-original-godot-replay-admission-v1",
            InputPath = inputPath,
            InputFileSha256 = Convert.ToHexString(SHA256.HashData(System.IO.File.ReadAllBytes(inputPath))),
            ActualUserArguments = OS.GetCmdlineUserArgs(),
            ActualProcessArguments = System.Environment.GetCommandLineArgs(),
            Consumer = AdmissionModule(typeof(PresentationReplay).Assembly),
            Protocol = AdmissionModule(typeof(WireJson).Assembly),
            Serializer = AdmissionModule(typeof(JsonSerializer).Assembly),
            Runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            FrameworkVersion = System.Environment.Version.ToString(),
            Options = new
            {
                WireJson.Options.DefaultIgnoreCondition,
                WireJson.Options.PropertyNameCaseInsensitive,
                WireJson.Options.WriteIndented,
                WireJson.Options.NumberHandling,
                NamingPolicy = WireJson.Options.PropertyNamingPolicy?.GetType().AssemblyQualifiedName,
                Converters = WireJson.Options.Converters.Select(c => c.GetType().AssemblyQualifiedName).ToArray()
            },
            Scope = "actual ordered short-circuit admission only; original failure preserved; no skipped branch/digest recomputation or replay acceptance"
        });
    }
    internal static JsonObject AdmissionModule(System.Reflection.Assembly assembly)
    {
        var result = new JsonObject { ["Basis"] = "actual loaded module metadata/location; MVID is not cryptographic loaded-image proof" };
        void Read<T>(string name, Func<T> read)
        {
            try { result[name] = JsonSerializer.SerializeToNode(read(), WireJson.Options); }
            catch (Exception error) { result[name + "Unavailable"] = error.Message; }
        }
        Read("FullName", () => assembly.FullName);
        Read("Version", () => assembly.GetName().Version?.ToString());
        Read("Location", () => assembly.Location);
        Read("Mvid", () => assembly.ManifestModule.ModuleVersionId);
        string? location = result["Location"]?.GetValue<string>();
        if (string.IsNullOrEmpty(location)) result["LocationFileHashUnavailable"] = "Actual module has no accessible disk location; no path/filehash invented or built-file substituted.";
        else Read("LocationFileSha256", () => Convert.ToHexString(SHA256.HashData(System.IO.File.ReadAllBytes(location))));
        return result;
    }
    internal static void PersistAdmissionReceipt(string outputPath, JsonObject details, byte[]? evaluatedPayload, bool refused, Func<object> optionalProvenance)
    {
        // REQUIRED actual guard and used bytes survive optional metadata errors.
        if (evaluatedPayload is not null) System.IO.File.WriteAllBytes(outputPath + ".admission-used-frames.json", evaluatedPayload);
        details["Result"] = refused ? "FAILED admission; original exception follows" : "PASSED admission only; not replay/native/raster acceptance";
        System.IO.File.WriteAllText(outputPath + ".admission.json", details.ToJsonString(WireJson.Options));
        try { details["Provenance"] = JsonSerializer.SerializeToNode(optionalProvenance(), WireJson.Options); }
        catch (Exception error) { details["OptionalProvenanceUnavailable"] = error.Message; }
        try { System.IO.File.WriteAllText(outputPath + ".admission.json", details.ToJsonString(WireJson.Options)); }
        catch (Exception error) { GD.PrintErr("Optional admission provenance persistence unavailable; required guard/payload retained: " + error.Message); }
    }
    public override void _Ready()
    {
        if (_ownership is not null && !_normalHistoryPrefix) { SetProcess(false); Callable.From(InspectOwnership).CallDeferred(); return; }
        if (_tabletop.ReplayCosts is not null)
            RenderingServer.ViewportSetMeasureRenderTime(GetViewport().GetViewportRid(), true);
    }
    private async void InspectOwnership()
    {
        try
        {
            // Settle the ordinary owned window/UI, then restore the retained
            // managed presentation timeline without rendering a sweep.
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            for (int i = 0; i <= 299; i++)
            {
                PresentationReplayFrame frame = _input.Frames[i];
                if (frame.Snapshot is { } state) _session.State = state;
                _tabletop.ReplayFocus(frame.Focus); _tabletop._Process(frame.Delta); _tabletop.ReplayZoom(frame.Zoom);
            }
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            var fields = new Dictionary<string, object?>(); var targets = new Dictionary<string, object>();
            _tabletop.AppendUiObservation(fields, targets);
            string current = JsonSerializer.Serialize(new { InputDigest = _input.Digest, Frame = 299, Fields = fields }, WireJson.Options);
            System.IO.File.WriteAllText(_output + ".ownership-observation.json", current);
            using Image image = GetViewport().GetTexture().GetImage();
            if (image.SavePng(_output + ".ownership.png") != Error.Ok) throw new IOException("Ownership view capture failed.");
            PixelOwnershipDiagnostic.Inspect(_tabletop, _ownership!, current, image, _output + ".ownership.json");
            if (System.Text.Json.Nodes.JsonNode.Parse(System.IO.File.ReadAllText(_ownership!))!["GlyphCompletion"]?.GetValue<bool>() == true)
                await PixelGlyphCompletion.Run(_tabletop, _output + ".ownership.json", _output + ".glyph-completion", current);
            if (System.Text.Json.Nodes.JsonNode.Parse(System.IO.File.ReadAllText(_ownership!))!["BoundaryControls"]?.GetValue<bool>() == true)
                throw new InvalidDataException("Fast reconstruction is not native pixel-control admission; use the ordinary rendered prefix.");
            _exit();
        }
        catch (Exception e) { GD.PrintErr(e); GetTree().Quit(1); }
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
                BoundaryValidation = _boundaryValidation.ToArray(),
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
            string path = _output + "." + _input.Frames[_index - 1].View + ".zoom" + _input.Frames[_index - 1].Zoom;
            await Capture(path);
            if (_remainingViews && _ownershipFrame == 499 && _index == 500)
            {
                await InspectRemainingView(path, 499);
                SetProcess(true); return;
            }
            if (_normalHistoryPrefix && _index == _ownershipFrame + 1)
            {
                // Ordinary _Process/render/capture barriers through the target,
                // never a managed-only restoration or a full600-frame run.
                string current = System.IO.File.ReadAllText(path + ".observation.json");
                System.IO.File.Copy(path + ".observation.json", _output + ".ownership-observation.json");
                System.IO.File.Copy(path + ".png", _output + ".ownership.png");
                using Image image = Image.LoadFromFile(path + ".png");
                PixelOwnershipDiagnostic.Inspect(_tabletop, _ownership!, current, image, _output + ".ownership.json");
                System.IO.File.WriteAllText(_output + ".prefix.json", JsonSerializer.Serialize(new
                {
                    Schema = "owned-normal-prefix-v1",
                    Frames = _index,
                    InputDigest = _input.Digest,
                    Mode = "ordinary scripted frame processing/native rendering/capture barriers to the independently bound target only",
                    Samples = _samples,
                    FrameUpdateMilliseconds = _frames
                }, WireJson.Options));
                if (System.Text.Json.Nodes.JsonNode.Parse(System.IO.File.ReadAllText(_ownership!))!["GlyphCompletion"]?.GetValue<bool>() == true)
                    await PixelGlyphCompletion.Run(_tabletop, _output + ".ownership.json", _output + ".glyph-completion", current);
                if (System.Text.Json.Nodes.JsonNode.Parse(System.IO.File.ReadAllText(_ownership!))!["BoundaryControls"]?.GetValue<bool>() == true)
                    await NativeBoundaryControls.Run(_tabletop, _ownership!, current, _output + ".boundary-controls");
                _exit(); return;
            }
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
            string observation = JsonSerializer.Serialize(new { InputDigest = _input.Digest, Frame = _index - 1, Fields = fields }, WireJson.Options);
            System.IO.File.WriteAllText(path + ".observation.json", observation);
            if (_baseline is not null)
            {
                string beforePath = Path.Combine(_baseline, Path.GetFileName(path));
                using Image before = Image.LoadFromFile(beforePath + ".png");
                if (before.IsEmpty() || before.GetWidth() != image.GetWidth() || before.GetHeight() != image.GetHeight()) throw new InvalidDataException("Missing or mismatched actual before frame.");
                before.Convert(Image.Format.Rgba8); image.Convert(Image.Format.Rgba8);
                long validationStart = Stopwatch.GetTimestamp(), validationBytes = GC.GetTotalAllocatedBytes(precise: true);
                NonDefenseRoofEvidence? proof = null;
                if (_boundaryEvidence is not null)
                {
                    var manifest = System.Text.Json.Nodes.JsonNode.Parse(System.IO.File.ReadAllText(_boundaryEvidence))!;
                    var frame = _input.Frames[_index - 1];
                    string? request = manifest["Bindings"] is not null ? NonDefenseRoofEvidence.BoundRequest(manifest, frame.Index, frame.View, (int)frame.Zoom)
                        : frame.Index == 299 ? _boundaryEvidence : null;
                    if (request is not null) proof = NativeRoofAttestation.Build(_tabletop, request, observation, image, path + ".roof-evidence");
                }
                int pixels = LandscapeBoundaryProof.Verify(System.Text.Json.Nodes.JsonNode.Parse(System.IO.File.ReadAllText(beforePath + ".observation.json"))!,
                    System.Text.Json.Nodes.JsonNode.Parse(observation)!, image.GetWidth(), image.GetHeight(), before.GetData(), image.GetData(), proof);
                var validation = new
                {
                    Frame = _index - 1,
                    Milliseconds = Stopwatch.GetElapsedTime(validationStart).TotalMilliseconds,
                    AllocatedBytes = GC.GetTotalAllocatedBytes(precise: true) - validationBytes,
                    Scope = "extra exact boundary/native attestation work included in aggregate elapsed/cumulative allocations, not presentation update samples or peak memory"
                };
                _boundaryValidation.Add(validation);
                System.IO.File.WriteAllText(path + ".boundary.json", JsonSerializer.Serialize(new
                {
                    InputDigest = _input.Digest,
                    Frame = _index - 1,
                    Baseline = beforePath,
                    ProtectedPixels = pixels,
                    Verification = validation,
                    AggregateIncludesVerification = _index != 600,
                    Result = "unchanged",
                    Scope = "fixed-input diagnostic battle pixels/poses/structures/terrain; separate live acceptance required; final599capture/proof outside raw aggregate just as original finalcapture was"
                }, WireJson.Options));
            }
        }
    }
    private async Task InspectRemainingView(string path, int frame)
    {
        long start = Stopwatch.GetTimestamp(), allocated = GC.GetTotalAllocatedBytes(precise: true);
        JsonNode request = JsonNode.Parse(System.IO.File.ReadAllText(_ownership!))!;
        if (frame == 599)
        {
            request["BeforeObservation"] = request["LastBeforeObservation"]!.GetValue<string>();
            request["BeforePng"] = request["LastBeforePng"]!.GetValue<string>();
            request["AfterObservation"] = path + ".observation.json"; request["AfterPng"] = path + ".png";
            request["TargetFrame"] = 599;
        }
        string current = System.IO.File.ReadAllText(path + ".observation.json");
        JsonNode beforeObservation = JsonNode.Parse(System.IO.File.ReadAllText(request["BeforeObservation"]!.GetValue<string>()))!;
        JsonNode afterObservation = JsonNode.Parse(current)!;
        using Image before = Image.LoadFromFile(request["BeforePng"]!.GetValue<string>());
        using Image after = Image.LoadFromFile(path + ".png");
        before.Convert(Image.Format.Rgba8); after.Convert(Image.Format.Rgba8);
        if (before.GetWidth() != after.GetWidth() || before.GetHeight() != after.GetHeight()) throw new InvalidDataException("Remaining native capture resolution changed.");
        byte[] a = before.GetData(), b = after.GetData();
        int[][] changes = LandscapeBoundaryProof.ChangedProtectedPixels(beforeObservation, afterObservation, after.GetWidth(), after.GetHeight(), a, b);
        System.IO.File.WriteAllText(path + ".remaining-differences.json", JsonSerializer.Serialize(new
        {
            Frame = frame,
            View = _input.Frames[frame].View,
            Pixels = changes,
            Differences = changes.Select(p => new { Pixel = p, Before = a.AsSpan((p[1] * after.GetWidth() + p[0]) * 4, 4).ToArray(), After = b.AsSpan((p[1] * after.GetWidth() + p[0]) * 4, 4).ToArray() })
        }, WireJson.Options));
        bool restorationUnion = frame == 599 && request["RestorationUnion"]?.GetValue<bool>() == true;
        if (restorationUnion)
        {
            foreach (var entry in request["UnionEvidenceHashes"]!.AsObject())
                if (Convert.ToHexString(SHA256.HashData(System.IO.File.ReadAllBytes(entry.Key))) != entry.Value!.GetValue<string>()) throw new InvalidDataException("Stale same599-frame measured union evidence.");
            using Image frozen = Image.LoadFromFile(request["RetainedFrozenPng"]!.GetValue<string>());
            using Image restored = Image.LoadFromFile(request["RetainedRestoredPng"]!.GetValue<string>());
            frozen.Convert(Image.Format.Rgba8); restored.Convert(Image.Format.Rgba8);
            JsonNode frozenObservation = JsonNode.Parse(System.IO.File.ReadAllText(request["RetainedFrozenObservation"]!.GetValue<string>()))!;
            JsonNode restoredObservation = JsonNode.Parse(System.IO.File.ReadAllText(request["RetainedRestoredObservation"]!.GetValue<string>()))!;
            int[][] originalChanges = LandscapeBoundaryProof.ChangedProtectedPixels(beforeObservation, frozenObservation, after.GetWidth(), after.GetHeight(), a, frozen.GetData());
            int[][] restoreChanges = LandscapeBoundaryProof.ChangedProtectedPixels(frozenObservation, restoredObservation, after.GetWidth(), after.GetHeight(), frozen.GetData(), restored.GetData());
            int[][] union = originalChanges.Concat(restoreChanges).Select(p => (X: p[0], Y: p[1])).Distinct().OrderBy(p => p.Y).ThenBy(p => p.X).Select(p => new[] { p.X, p.Y }).ToArray();
            if (union.Length != 6 || !JsonNode.DeepEquals(request["Pixels"], JsonSerializer.SerializeToNode(union, WireJson.Options))) throw new InvalidDataException("599 query must equal its own measured six-point union, never an inherited coordinate list.");
        }
        else if (frame == 599) request["Pixels"] = JsonSerializer.SerializeToNode(changes, WireJson.Options);
        if (changes.Length > 64) throw new InvalidDataException("Remaining view exceeded its bounded changed-point investigation; no points dropped.");
        NonDefenseRoofEvidence? proof = null;
        if (frame == 499 || restorationUnion || changes.Length > 0)
        {
            string ownedRequest = path + ".remaining-request.json";
            System.IO.File.WriteAllText(ownedRequest, request.ToJsonString());
            PixelOwnershipDiagnostic.Inspect(_tabletop, ownedRequest, current, after, path + ".ownership.json");
            await PixelGlyphCompletion.Run(_tabletop, path + ".ownership.json", path + ".glyph-completion", current);
            JsonNode native = JsonNode.Parse(System.IO.File.ReadAllText(path + ".ownership.json"))!;
            JsonNode glyph = JsonNode.Parse(System.IO.File.ReadAllText(path + ".glyph-completion.glyphs.json"))!;
            proof = NonDefenseRoofEvidence.Create(beforeObservation, afterObservation, native, native, glyph, glyph["Labels"]!, after.GetWidth(), after.GetHeight());
            if (glyph["Labels"]!.AsArray().Count > 0)
            {
                using Image restored = Image.LoadFromFile(path + ".glyph-completion.restored.png"); restored.Convert(Image.Format.Rgba8);
                JsonNode restoredObservation = JsonNode.Parse(System.IO.File.ReadAllText(path + ".glyph-completion.restored-observation.json"))!;
                JsonNode restoredNative = native;
                if (restorationUnion)
                {
                    PixelOwnershipDiagnostic.Inspect(_tabletop, ownedRequest, restoredObservation.ToJsonString(), restored, path + ".restored-native.json");
                    restoredNative = JsonNode.Parse(System.IO.File.ReadAllText(path + ".restored-native.json"))!;
                }
                var restorationProof = NonDefenseRoofEvidence.Create(afterObservation, restoredObservation, native, restoredNative, glyph, glyph["Labels"]!, after.GetWidth(), after.GetHeight());
                int restoredPixels = LandscapeBoundaryProof.Verify(afterObservation, restoredObservation, after.GetWidth(), after.GetHeight(), b, restored.GetData(), restorationProof);
                System.IO.File.WriteAllText(path + ".restoration-proof.json", JsonSerializer.Serialize(new
                {
                    Frame = frame,
                    ProtectedPixels = restoredPixels,
                    FrozenObservationSha256 = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(current))),
                    RestoredObservation = path + ".glyph-completion.restored-observation.json",
                    NativeStacksCompared = restorationUnion,
                    Result = "exact protected native restoration with this frame's independently proved preexisting non-defense family samples only"
                }, WireJson.Options));
            }
        }
        int compared = LandscapeBoundaryProof.Verify(beforeObservation, afterObservation, after.GetWidth(), after.GetHeight(), a, b, proof);
        System.IO.File.WriteAllText(path + ".remaining-proof.json", JsonSerializer.Serialize(new
        {
            Frame = frame,
            View = _input.Frames[frame].View,
            ProtectedPixels = compared,
            ChangedProtectedPixels = changes.Length,
            Result = proof is null ? "exact unchanged; no exclusion" : "independently proved original non-defense mine-roof family and no glyph contribution",
            VerificationMilliseconds = Stopwatch.GetElapsedTime(start).TotalMilliseconds,
            VerificationAllocatedBytes = GC.GetTotalAllocatedBytes(precise: true) - allocated,
            Scope = "ordinary frozen remaining-view native investigation only; includes exact discovery/geometry/glyph/restoration work, not matched render cost or live acceptance"
        }, WireJson.Options));
        if (frame == 499) System.IO.File.Copy(path + ".ownership.json", _output + ".ownership.json");
    }
    private async void Finish()
    {
        try
        {
            await Capture(_output);
            if (_remainingViews) await InspectRemainingView(_output, 599);
            _exit();
        }
        catch (Exception e) { GD.PrintErr(e); GetTree().Quit(1); }
    }
    public override void _ExitTree() => _process.Dispose();
}
