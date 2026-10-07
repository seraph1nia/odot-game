using System.Text.Json.Nodes;
using Game;
using Xunit;

namespace DevRunner.Tests;

public sealed class NativeBoundaryControlReceiptTests
{
    [Theory]
    [InlineData(299)]
    [InlineData(399)]
    [InlineData(499)]
    public void ExactRestorationBindsActualCapturedFrame(int frame)
    {
        string directory = Path.Combine(Path.GetTempPath(), "native-control-receipt-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            JsonNode current = new JsonObject
            {
                ["InputDigest"] = "bound-input",
                ["Frame"] = frame,
                ["Fields"] = JsonNode.Parse("""{"Camera":{"Size":10},"Units":[]}""")
            };
            string output = Path.Combine(directory, "control");
            bool exact = NativeBoundaryControls.WriteRestorationReceipt(current, current["Fields"]!, Enumerable.Range(0, 5).Select(i => (object)new { Name = i }).ToArray(), output);
            Assert.True(exact);
            NativeBoundaryControls.RequireComplete(5, exact);
            JsonNode receipt = JsonNode.Parse(File.ReadAllText(output + ".controls.json"))!;
            Assert.True(receipt["Complete"]!.GetValue<bool>());
            Assert.True(receipt["ExactObservedRestoration"]!.GetValue<bool>());
            Assert.Contains($"frame{frame}/camera/light", receipt["Method"]!.GetValue<string>());
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
    [Theory]
    [InlineData(5, false)]
    [InlineData(0, true)]
    [InlineData(4, true)]
    [InlineData(6, true)]
    public void SuccessfulTerminalResultRequiresAllControlsAndExactRestoration(int count, bool exact)
        => Assert.Throws<InvalidDataException>(() => NativeBoundaryControls.RequireComplete(count, exact));
    [Fact]
    public void AFailedFinalObservationCannotReplaceTheFirstControlFailure()
    {
        var first = new InvalidDataException("original control failure");
        var final = new IOException("final observation failure");
        Exception? reported = null;
        Func<object> observer = () => throw final;
        Action execute = () =>
        {
            try { throw first; }
            finally { NativeBoundaryControls.FinishRestoration(new JsonObject(), observer, [], "unused", first, e => reported = e); }
        };
        Exception? actual = Record.Exception(execute);
        Assert.Same(first, actual);
        Assert.Same(final, reported);
        Assert.Throws<IOException>(() => NativeBoundaryControls.FinishRestoration(new JsonObject(), observer, [], "unused", null, _ => { }));
    }
    [Fact]
    public void FailedRestorationKeepsNegativeReceiptAndRefusesSuccess()
    {
        string directory = Path.Combine(Path.GetTempPath(), "native-control-negative-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            JsonNode current = JsonNode.Parse("""{"InputDigest":"bound-input","Frame":299,"Fields":{"UnitMaterial":"original"}}""")!;
            bool exact = NativeBoundaryControls.WriteRestorationReceipt(current, JsonNode.Parse("""{"UnitMaterial":"changed"}""")!, new object[5], Path.Combine(directory, "control"));
            Assert.False(exact);
            Assert.Throws<InvalidDataException>(() => NativeBoundaryControls.RequireComplete(5, exact));
            Assert.False(JsonNode.Parse(File.ReadAllText(Path.Combine(directory, "control.controls.json")))!["Complete"]!.GetValue<bool>());
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
