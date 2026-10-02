using DevRunner;
using Game;
using Xunit;

namespace DevRunner.Tests;

public sealed class VerificationPacingTests
{
    [Fact]
    public void UnownedAndGuestRequestsCannotChangeInteractiveSpeed()
    {
        var interactive = new VerificationPacing(null, null, null);
        Assert.False(interactive.Configure(4, true));
        Assert.Equal(1, interactive.Speed);
        WithOwned(pacing => { Assert.False(pacing.Configure(4, false)); Assert.Equal(1, pacing.Speed); });
    }
    [Fact]
    public void ExactStepsAndPauseNeverAccumulateCatchup()
    {
        WithOwned(pacing =>
        {
            Assert.True(pacing.Configure(4, true));
            int ticks = 0; bool paused = false;
            pacing.Run(() => { ticks++; if (ticks == 2) paused = true; }, () => paused);
            Assert.Equal(2, ticks);
            for (int n = 0; n < 100; n++) pacing.Run(() => ticks++, () => paused);
            Assert.Equal(2, ticks);
            paused = false;
            pacing.Run(() => ticks++, () => paused);
            Assert.Equal(6, ticks);
            Assert.True(pacing.Configure(1, true));
            pacing.Run(() => ticks++, () => paused);
            Assert.Equal(7, ticks);
        });
    }
    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    public void InvalidBoundsAreRejected(int speed)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new VerificationPacing(null, null, null).Configure(speed, true));
        Assert.Throws<ArgumentException>(() => Options.Parse(["test-network", "--simulation-speed", speed.ToString(System.Globalization.CultureInfo.InvariantCulture)]));
    }
    private static void WithOwned(Action<VerificationPacing> test)
    {
        string root = Path.Combine(Path.GetTempPath(), "odot-pacing-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string marker = Path.Combine(root, "owner"); File.WriteAllText(marker, "fixture-token");
        try
        {
            Assert.False(new VerificationPacing(root, marker, "wrong").Owned);
            Assert.False(new VerificationPacing(Path.Combine(root, "other"), marker, "fixture-token").Owned);
            var pacing = new VerificationPacing(root, marker, "fixture-token");
            Assert.True(pacing.Owned); test(pacing);
        }
        finally { Directory.Delete(root, true); }
    }
}
