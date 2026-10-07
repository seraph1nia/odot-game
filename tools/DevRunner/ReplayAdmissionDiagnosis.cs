using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Game.Core;

namespace DevRunner;

// A finite, non-rendering diagnosis using the retained actual Game.dll replay
// types and the SAME loaded Core serializer/options. It grants no admission.
internal static class ReplayAdmissionDiagnosis
{
    // Metadata behavior fixture ONLY, never a replay model or consumer substitute.
    private sealed class EmptyLocationAssembly : Assembly
    {
        public override string FullName => typeof(WireJson).Assembly.FullName!;
        public override string Location => "";
        public override AssemblyName GetName(bool copiedName) => typeof(WireJson).Assembly.GetName(copiedName);
        public override Module ManifestModule => typeof(WireJson).Assembly.ManifestModule;
    }
    internal static void Run(string inputPath, string consumerPath, string output, bool predicateGuards = false)
    {
        string corePath = Path.Combine(Path.GetDirectoryName(consumerPath)!, "Game.Core.dll");
        static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        if (Hash(corePath) != Hash(typeof(WireJson).Assembly.Location)) throw new InvalidDataException("Loaded protocol assembly differs from exact replay consumer dependency.");
        Assembly godot = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(Path.GetDirectoryName(consumerPath)!, "GodotSharp.dll"));
        Assembly consumer = AssemblyLoadContext.Default.LoadFromAssemblyPath(consumerPath);
        Type type = consumer.GetType("Game.PresentationReplayInput", throwOnError: true)!;
        object Read(object instance, string property) => instance.GetType().GetProperty(property)!.GetValue(instance)!;
        // The generic methods are the same overloads used by the game predicate,
        // with its actual internal record types, not alternate DTOs/options.
        MethodInfo deserialize = typeof(JsonSerializer).GetMethods().Single(m => m.Name == "Deserialize" && m.IsGenericMethodDefinition
            && m.GetParameters() is var p && p.Length == 2 && p[0].ParameterType == typeof(string) && p[1].ParameterType == typeof(JsonSerializerOptions));
        MethodInfo serialize = typeof(JsonSerializer).GetMethods().Single(m => m.Name == "SerializeToUtf8Bytes" && m.IsGenericMethodDefinition
            && m.GetParameters() is var p && p.Length == 2 && p[1].ParameterType == typeof(JsonSerializerOptions));
        if (predicateGuards)
        {
            MethodInfo guard = consumer.GetType("Game.PresentationReplay", throwOnError: true)!.GetMethod("InvalidAdmission", BindingFlags.NonPublic | BindingFlags.Static)!;
            var cases = new List<object>();
            foreach (string mutation in new[] { "none", "version", "count", "index", "delta", "focus", "zoom", "digest" })
            {
                // Synthetic guard-only records; no authority, pose or acceptance input.
                var framesNode = new System.Text.Json.Nodes.JsonArray();
                for (int i = 0; i < (mutation == "count" ? 599 : 600); i++)
                    framesNode.Add(JsonSerializer.SerializeToNode(new
                    {
                        Index = mutation == "index" && i == 0 ? 7 : i,
                        Delta = mutation == "delta" && i == 0 ? 0.0 : 1.0 / 60,
                        Focus = mutation == "focus" && i == 0 ? 0 : 1,
                        Zoom = mutation == "zoom" && i == 0 ? 0 : 1
                    }, WireJson.Options));
                string versionName = mutation == "version" ? "bad" : "authored-scale-replay-v1";
                var sourceNode = new System.Text.Json.Nodes.JsonObject { ["Version"] = versionName, ["Digest"] = "bad", ["Frames"] = framesNode, ["Commands"] = 0, ["FinalTick"] = 0 };
                object fixture = deserialize.MakeGenericMethod(type).Invoke(null, [sourceNode.ToJsonString(), WireJson.Options])!;
                Array fixtureFrames = (Array)Read(fixture, "Frames");
                byte[] expectedBytes = (byte[])serialize.MakeGenericMethod(fixtureFrames.GetType()).Invoke(null, [fixtureFrames, WireJson.Options])!;
                if (mutation != "digest") sourceNode["Digest"] = Convert.ToHexString(SHA256.HashData(expectedBytes));
                fixture = deserialize.MakeGenericMethod(type).Invoke(null, [sourceNode.ToJsonString(), WireJson.Options])!;
                var trace = new System.Text.Json.Nodes.JsonObject(); object?[] arguments = [fixture, trace, null];
                bool refused = (bool)guard.Invoke(null, arguments)!;
                string? first = trace["FirstFailedClause"]?.GetValue<string>();
                string? expectedFirst = mutation switch { "none" => null, "version" => "Version", "count" => "FrameCount", "index" => "Index", "delta" => "Delta", "focus" => "Focus", "zoom" => "Zoom", _ => "ComputedDigest" };
                bool digestReached = mutation is "none" or "digest";
                if (refused != (mutation != "none") || first != expectedFirst || (arguments[2] is not null) != digestReached
                    || (trace["ComputedDigest"] is not null) != digestReached) throw new InvalidDataException("Actual unchanged admission/short-circuit guard failed: " + mutation);
                if (mutation == "version" && trace["FrameCount"] is not null || mutation is "version" or "count" && trace["Frames"] is not null)
                    throw new InvalidDataException("Admission recomputed a skipped branch.");
                if (digestReached && !expectedBytes.AsSpan().SequenceEqual((byte[])arguments[2]!)) throw new InvalidDataException("Admission receipt replaced actually evaluated bytes.");
                if (mutation is "index" or "delta" or "focus" or "zoom")
                {
                    var frameTrace = trace["EvaluatedFrames"]!.AsArray();
                    int expectedChecks = mutation switch { "index" => 1, "delta" => 2, "focus" => 3, _ => 4 };
                    if (frameTrace.Count != 1 || frameTrace[0]!.AsObject().Count != expectedChecks + 1) throw new InvalidDataException("Frame short-circuit/stop-first-failure drifted.");
                }
                cases.Add(new { Mutation = mutation, Refused = refused, FirstFailedClause = first, DigestReached = digestReached, Result = "pass actual consumer predicate/short-circuit/used-byte guard" });
            }
            Type replay = consumer.GetType("Game.PresentationReplay", throwOnError: true)!;
            MethodInfo moduleReader = replay.GetMethod("AdmissionModule", BindingFlags.NonPublic | BindingFlags.Static)!;
            var emptyModule = (System.Text.Json.Nodes.JsonObject)moduleReader.Invoke(null, [new EmptyLocationAssembly()])!;
            if (emptyModule["Location"]!.GetValue<string>() != "" || emptyModule["LocationFileHashUnavailable"] is null || emptyModule["LocationFileSha256"] is not null || emptyModule["Mvid"] is null)
                throw new InvalidDataException("Empty loaded location invented provenance or lost accessible metadata.");
            MethodInfo persister = replay.GetMethod("PersistAdmissionReceipt", BindingFlags.NonPublic | BindingFlags.Static)!;
            foreach (bool refused in new[] { true, false })
            {
                string receiptPath = output + ".partial-" + refused;
                var trace = new System.Text.Json.Nodes.JsonObject { ["Refused"] = refused, ["FirstFailedClause"] = refused ? "ComputedDigest" : null };
                byte[] used = [1, 2, 3];
                Func<object> unavailable = () => throw new InvalidOperationException("owned optional metadata unavailable fixture");
                persister.Invoke(null, [receiptPath, trace, used, refused, unavailable]);
                var retained = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(receiptPath + ".admission.json"))!;
                if (retained["Refused"]!.GetValue<bool>() != refused || retained["FirstFailedClause"]?.GetValue<string>() != (refused ? "ComputedDigest" : null)
                    || retained["OptionalProvenanceUnavailable"] is null || !used.AsSpan().SequenceEqual(File.ReadAllBytes(receiptPath + ".admission-used-frames.json")))
                    throw new InvalidDataException("Optional provenance lost required payload/guard or replaced first observed failure.");
                cases.Add(new { Mutation = "optional-metadata-" + refused, Refused = refused, Result = "pass required partial receipt/used bytes/firstfailure preservation" });
            }
            cases.Add(new { Mutation = "empty-actual-location", Result = "pass explicit unavailable filehash/empty path/accessible metadata without candidate substitution" });
            string blockedParent = output + ".blocked-parent"; File.WriteAllText(blockedParent, "owned required receipt failure fixture");
            bool optionalCalled = false, requiredFailed = false;
            try { persister.Invoke(null, [Path.Combine(blockedParent, "receipt"), new System.Text.Json.Nodes.JsonObject(), new byte[] { 1 }, false, (Func<object>)(() => { optionalCalled = true; return "not reachable"; })]); }
            catch (TargetInvocationException error) when (error.InnerException is IOException) { requiredFailed = true; }
            if (!requiredFailed || optionalCalled) throw new InvalidDataException("Missing required guard/usedpayload did not stop before optional provenance.");
            cases.Add(new { Mutation = "required-receipt-failure", Result = "pass cannot continue without required usedpayload/guard" });
            File.WriteAllText(output, JsonSerializer.Serialize(new { Cases = cases, ConsumerSha256 = Hash(consumerPath), Source = "actual Game.dll static admission guard; synthetic validation-only fixtures, not pose/input/replay admission" }, WireJson.Options));
            return;
        }
        string text = File.ReadAllText(inputPath);
        object input = deserialize.MakeGenericMethod(type).Invoke(null, [text, WireJson.Options]) ?? throw new InvalidDataException("Actual consumer deserialized null replay input.");
        Array frames = (Array)Read(input, "Frames"); string version = (string)Read(input, "Version"), declared = (string)Read(input, "Digest");
        byte[] canonical = (byte[])serialize.MakeGenericMethod(frames.GetType()).Invoke(null, [frames, WireJson.Options])!;
        string computed = Convert.ToHexString(SHA256.HashData(canonical));
        using JsonDocument source = JsonDocument.Parse(text);
        byte[] producer = Encoding.UTF8.GetBytes(source.RootElement.GetProperty("Frames").GetRawText());
        var violations = new List<object>();
        int frameIndex = 0;
        foreach (object frame in frames)
        {
            int index = (int)Read(frame, "Index"), focus = (int)Read(frame, "Focus"); double delta = (double)Read(frame, "Delta"); float zoom = (float)Read(frame, "Zoom");
            bool badIndex = index != frameIndex, badDelta = delta != 1.0 / 60, badFocus = focus is not (1 or 2), badZoom = version == "authored-scale-replay-v1" ? zoom is not (1 or 3) : zoom != 0;
            if (badIndex || badDelta || badFocus || badZoom) violations.Add(new
            {
                At = frameIndex,
                Index = index,
                Focus = focus,
                Delta = delta,
                DeltaBits = BitConverter.DoubleToInt64Bits(delta),
                ExpectedDelta = 1.0 / 60,
                ExpectedDeltaBits = BitConverter.DoubleToInt64Bits(1.0 / 60),
                Zoom = zoom,
                View = frame.GetType().GetProperty("View")!.GetValue(frame),
                BadIndex = badIndex,
                BadDelta = badDelta,
                BadFocus = badFocus,
                BadZoom = badZoom
            });
            frameIndex++;
        }
        bool badVersion = version is not ("ordinary-combat-replay-v1" or "authored-scale-replay-v1"), badCount = frames.Length != 600;
        int firstByte = 0; while (firstByte < Math.Min(producer.Length, canonical.Length) && producer[firstByte] == canonical[firstByte]) firstByte++;
        string? FieldDifference(JsonElement a, JsonElement b, string path)
        {
            if (a.ValueKind != b.ValueKind) return path;
            if (a.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty p in a.EnumerateObject())
                { if (!b.TryGetProperty(p.Name, out JsonElement value)) return path + "." + p.Name; string? found = FieldDifference(p.Value, value, path + "." + p.Name); if (found is not null) return found; }
                if (a.EnumerateObject().Count() != b.EnumerateObject().Count()) return path + ".<extra-property>";
                return null;
            }
            if (a.ValueKind == JsonValueKind.Array)
            {
                if (a.GetArrayLength() != b.GetArrayLength()) return path + ".Length";
                for (int i = 0; i < a.GetArrayLength(); i++) { string? found = FieldDifference(a[i], b[i], path + "[" + i + "]"); if (found is not null) return found; }
                return null;
            }
            return a.GetRawText() == b.GetRawText() ? null : path;
        }
        using JsonDocument typed = JsonDocument.Parse(canonical);
        string? difference = FieldDifference(source.RootElement.GetProperty("Frames"), typed.RootElement, "Frames");
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        File.WriteAllBytes(output + ".consumer-frames.json", canonical); File.WriteAllBytes(output + ".producer-frames.json", producer);
        object AssemblyFacts(Assembly a) => new { a.FullName, a.Location, Sha256 = Hash(a.Location), Mvid = a.ManifestModule.ModuleVersionId };
        File.WriteAllText(output, JsonSerializer.Serialize(new
        {
            Schema = "actual-replay-consumer-admission-diagnosis-v1",
            InputPath = inputPath,
            InputSha256 = Hash(inputPath),
            Consumer = AssemblyFacts(consumer),
            Protocol = AssemblyFacts(typeof(WireJson).Assembly),
            GodotBinding = AssemblyFacts(godot),
            Serializer = AssemblyFacts(typeof(JsonSerializer).Assembly),
            WireJson.ProtocolVersion,
            Framework = RuntimeInformation.FrameworkDescription,
            Runtime = Environment.Version.ToString(),
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            Arguments = Environment.GetCommandLineArgs(),
            Options = new
            {
                WireJson.Options.DefaultIgnoreCondition,
                WireJson.Options.PropertyNameCaseInsensitive,
                WireJson.Options.WriteIndented,
                NumberHandling = WireJson.Options.NumberHandling.ToString(),
                NamingPolicy = WireJson.Options.PropertyNamingPolicy?.GetType().FullName,
                Converters = WireJson.Options.Converters.Select(c => c.GetType().AssemblyQualifiedName).ToArray()
            },
            Version = version,
            FrameCount = frames.Length,
            BadVersion = badVersion,
            BadFrameCount = badCount,
            FrameViolations = violations,
            DeclaredDigest = declared,
            ComputedTypedRoundtripDigest = computed,
            ProducerFrameBytesDigest = Convert.ToHexString(SHA256.HashData(producer)),
            DigestMatches = computed == declared,
            ExistingPredicateWouldRefuse = badVersion || badCount || violations.Count != 0 || computed != declared,
            FirstByteDifference = producer.AsSpan().SequenceEqual(canonical) ? (int?)null : firstByte,
            FirstFieldDifference = difference,
            ProducerBytes = producer.Length,
            ConsumerBytes = canonical.Length,
            Scope = "single non-rendering actual consumer-type/serializer check; same CLR but DevRunner host, not original exited Godot module receipt; no acceptance, input/protocol/options/guard/runtime changes or rendered retry"
        }, WireJson.Options));
    }
}
