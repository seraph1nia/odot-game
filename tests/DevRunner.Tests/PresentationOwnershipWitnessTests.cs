using System.Text.Json.Nodes;
using Xunit;

namespace DevRunner.Tests;

public sealed class PresentationOwnershipWitnessTests
{
    [Fact]
    public void ExactZeroRemaining599UsesItsActualCompletedWitness()
    {
        using var fixture = new WitnessFixture();
        Assert.Equal(fixture.Output + ".remaining-proof.json", Runner.PresentationOwnershipWitness(fixture.Output, fixture.Request, fixture.Input));
    }
    [Theory]
    [InlineData("TargetFrame", "499")]
    [InlineData("RemainingViews", "false")]
    [InlineData("RestorationUnion", "true")]
    [InlineData("BoundaryControls", "true")]
    public void OtherRequestsCannotBorrowZeroRemainingProof(string key, string value)
    {
        using var fixture = new WitnessFixture(); fixture.Request[key] = JsonNode.Parse(value);
        Assert.Throws<InvalidOperationException>(() => Runner.PresentationOwnershipWitness(fixture.Output, fixture.Request, fixture.Input));
    }
    [Theory]
    [InlineData(".remaining-proof.json", "Frame", "499")]
    [InlineData(".remaining-proof.json", "View", "\"combat-64\"")]
    [InlineData(".remaining-proof.json", "ChangedProtectedPixels", "1")]
    [InlineData(".remaining-proof.json", "ProtectedPixels", "0")]
    [InlineData(".remaining-proof.json", "Result", "\"independently proved original non-defense mine-roof family and no glyph contribution\"")]
    [InlineData(".remaining-differences.json", "Pixels", "[[139,573]]")]
    [InlineData(".remaining-differences.json", "Differences", "[{}]")]
    [InlineData(".observation.json", "Frame", "499")]
    [InlineData(".observation.json", "InputDigest", "\"other-input\"")]
    [InlineData("", "Frames", "599")]
    [InlineData("", "InputDigest", "\"other-input\"")]
    [InlineData("", "Schema", "\"incomplete\"")]
    public void FailedOrMismatchedReceiptsCannotBecomeOwnershipEvidence(string suffix, string key, string value)
    {
        using var fixture = new WitnessFixture(); JsonNode document = JsonNode.Parse(File.ReadAllText(fixture.Output + suffix))!;
        document[key] = JsonNode.Parse(value); File.WriteAllText(fixture.Output + suffix, document.ToJsonString());
        Assert.Throws<InvalidOperationException>(() => Runner.PresentationOwnershipWitness(fixture.Output, fixture.Request, fixture.Input));
    }
    [Theory]
    [InlineData(".remaining-proof.json")]
    [InlineData(".remaining-differences.json")]
    [InlineData(".observation.json")]
    [InlineData(".png")]
    [InlineData("")]
    public void MissingCompletionArtifactsRefuseSuccess(string suffix)
    {
        using var fixture = new WitnessFixture(); File.Delete(fixture.Output + suffix);
        Assert.Throws<InvalidOperationException>(() => Runner.PresentationOwnershipWitness(fixture.Output, fixture.Request, fixture.Input));
    }
    [Fact]
    public void ExistingOwnershipWitnessKeepsItsPreference()
    {
        using var fixture = new WitnessFixture(); File.WriteAllText(fixture.Output + ".ownership.json", "{}");
        Assert.Equal(fixture.Output + ".ownership.json", Runner.PresentationOwnershipWitness(fixture.Output, fixture.Request, fixture.Input));
    }
    [Fact]
    public void PreexistingOutputsCannotBeReusedAsFreshSourceEvidence()
    {
        using var fixture = new WitnessFixture();
        Assert.Throws<InvalidOperationException>(() => Runner.RequireFreshOwnershipOutput(fixture.Output));
        Runner.RequireFreshOwnershipOutput(fixture.Output + ".new");
    }
    [Fact]
    public async Task SourceAndRequestSnapshotsMustStillMatchAfterExecution()
    {
        string root = Path.Combine(Path.GetTempPath(), "ownership-source-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            foreach (string area in new[] { "src", "tests", "tools", ".github", ".mise" }) Directory.CreateDirectory(Path.Combine(root, area));
            foreach (string file in new[] { "mise.toml", "mise.lock", "global.json", "Directory.Build.props", ".editorconfig", "Odot.slnx" }) File.WriteAllText(Path.Combine(root, file), "{}");
            string request = Path.Combine(root, "request.json"); File.WriteAllText(request, "{}");
            string source = Path.Combine(root, "src", "owned-input.dat"); File.WriteAllText(source, "original");
            var evidence = new Evidence(root);
            var runner = new Runner(Options.Parse(["profile-presentation", "--scenario", "authored-scale", "--iterations", "1", "--pixel-ownership-request", request]), CancellationToken.None, evidence, root: root);
            await runner.WritePresentationIdentity(CancellationToken.None);
            JsonNode identity = JsonNode.Parse(File.ReadAllText(Path.Combine(evidence.Directory, "profile-identity.json")))!;
            var inputs = identity["Inputs"]!.AsObject().ToDictionary(p => p.Key, p => p.Value!.GetValue<string>());
            runner.RequireUnchangedOwnershipInputs(request, "{}", inputs);
            File.WriteAllText(source, "changed");
            Assert.Throws<InvalidOperationException>(() => runner.RequireUnchangedOwnershipInputs(request, "{}", inputs));
            File.WriteAllText(source, "original"); File.WriteAllText(request, "{\"TargetFrame\":499}");
            Assert.Throws<InvalidOperationException>(() => runner.RequireUnchangedOwnershipInputs(request, "{}", inputs));
        }
        finally { Directory.Delete(root, recursive: true); }
    }
    private sealed class WitnessFixture : IDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(), "owned-witness-" + Guid.NewGuid().ToString("N"));
        internal string Output { get; }
        internal CombatReplayInput Input { get; } = CombatReplayFixture.GenerateAuthored();
        internal JsonNode Request { get; } = JsonNode.Parse("""{"TargetFrame":599,"RemainingViews":true}""")!;
        internal WitnessFixture()
        {
            Directory.CreateDirectory(directory); Output = Path.Combine(directory, "profile.json");
            File.WriteAllText(Output, new JsonObject { ["Schema"] = "odot-presentation-profile-v1", ["Frames"] = 600, ["InputDigest"] = Input.Digest }.ToJsonString());
            File.WriteAllText(Output + ".observation.json", new JsonObject { ["Frame"] = 599, ["InputDigest"] = Input.Digest }.ToJsonString());
            File.WriteAllText(Output + ".remaining-proof.json", """{"Frame":599,"View":"combat-256","ProtectedPixels":128,"ChangedProtectedPixels":0,"Result":"exact unchanged; no exclusion"}""");
            File.WriteAllText(Output + ".remaining-differences.json", """{"Frame":599,"View":"combat-256","Pixels":[],"Differences":[]}""");
            File.WriteAllBytes(Output + ".png", [1]); // Artifact-existence contract only, never native rendering evidence.
        }
        public void Dispose() => Directory.Delete(directory, recursive: true);
    }
}
