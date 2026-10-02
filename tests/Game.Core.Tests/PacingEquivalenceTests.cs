using System.Text.Json;
using Game;
using Xunit;

namespace Game.Core.Tests;

public sealed class PacingEquivalenceTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(8)]
    public void BatchedOrdinaryStepsPreserveEveryIntermediateStateAndEvent(int speed)
    {
        string root = Path.Combine(Path.GetTempPath(), "odot-steps-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string marker = Path.Combine(root, "owner"); File.WriteAllText(marker, "fixture");
        try
        {
            using var ordinary = new SessionFlow(); using var batched = new SessionFlow();
            ordinary.Act(1, new("start")); batched.Act(1, new("start"));
            ordinary.Prepare(); batched.Prepare();
            var pacing = new VerificationPacing(root, marker, "fixture");
            Assert.True(pacing.Configure(speed, true));
            for (int callback = 0; callback < 240 / speed; callback++)
                pacing.Run(() =>
                {
                    ordinary.Session.Step(); batched.Session.Step();
                    var expected = ordinary.State with { MatchId = batched.State.MatchId };
                    Assert.Equal(JsonSerializer.Serialize(expected, WireJson.Options), JsonSerializer.Serialize(batched.State, WireJson.Options));
                }, () => batched.State.Paused);
            ordinary.Act(1, new("pause")); batched.Act(1, new("pause"));
            long frozen = batched.State.Tick;
            for (int n = 0; n < 30; n++) pacing.Run(batched.Session.Step, () => batched.State.Paused);
            Assert.Equal(frozen, batched.State.Tick);
            ordinary.Act(1, new("resume")); batched.Act(1, new("resume"));
            pacing.Run(() => { ordinary.Session.Step(); batched.Session.Step(); }, () => batched.State.Paused);
            Assert.Equal(JsonSerializer.Serialize(ordinary.State with { MatchId = batched.State.MatchId }, WireJson.Options), JsonSerializer.Serialize(batched.State, WireJson.Options));
        }
        finally { Directory.Delete(root, true); }
    }
}
