using Game.Distribution;
using Xunit;

namespace DevRunner.Tests;

public sealed class ReleaseVersionTests
{
    [Theory]
    [InlineData("0.9.0", "0.10.0")]
    [InlineData("0.1.0-beta.2", "0.1.0-beta.10")]
    [InlineData("0.1.0-beta.99999999999999999999999", "0.1.0-beta.100000000000000000000000")]
    [InlineData("0.1.0-alpha", "0.1.0-alpha.1")]
    [InlineData("0.1.0-alpha.1", "0.1.0-alpha.beta")]
    [InlineData("0.1.0-beta.10", "0.1.0-rc.1")]
    [InlineData("0.1.0-rc.1", "0.1.0")]
    [InlineData("1.0.0", "2.0.0")]
    public void PrecedenceUsesNumericComponentsAndSemverPrereleaseRules(string older, string newer)
    {
        ReleaseVersion a = ReleaseVersion.FromTag("v" + older), b = ReleaseVersion.FromTag("v" + newer);
        Assert.True(a.ComparePrecedence(b) < 0);
        Assert.True(b.ComparePrecedence(a) > 0);
        Assert.Equal(0, a.ComparePrecedence(a));
    }

    [Theory]
    [InlineData("0.1.0", "0.1.0+different", false)]
    [InlineData("0.1.0+one", "0.1.0+two", false)]
    [InlineData("0.1.0", "0.2.0-beta.1", false)]
    [InlineData("0.1.0-beta.1", "0.1.0", true)]
    [InlineData("0.1.0-beta.1", "0.2.0-beta.1", true)]
    [InlineData("0.2.0-beta.1", "0.1.0", false)]
    [InlineData("0.1.0", "0.2.0", true)]
    public void ChannelAndStrictPrecedenceControlUpdates(string installed, string candidate, bool expected)
        => Assert.Equal(expected, ReleaseVersion.FromTag("v" + installed).CanUpdateTo(ReleaseVersion.FromTag("v" + candidate)));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1.2.3")]
    [InlineData("V1.2.3")]
    [InlineData("v1.2")]
    [InlineData("v01.2.3")]
    [InlineData("v1.2.03")]
    [InlineData("v1.2.3-beta.01")]
    [InlineData("v1.2.3-beta..1")]
    [InlineData("v1.2.3+meta..1")]
    [InlineData("v1.2.3+")]
    [InlineData("v1.2.3-β")]
    [InlineData("v1.2.3\n")]
    [InlineData("v65535.0.0")]
    [InlineData("v0.65535.0")]
    [InlineData("v0.0.65535")]
    [InlineData("v999999999999999999999.0.0")]
    public void UnsupportedTagsFail(string? tag)
        => Assert.Throws<ArgumentException>(() => ReleaseVersion.FromTag(tag));

    [Fact]
    public void FullIdentityAndSupportedNumericBoundaryAreRetained()
    {
        ReleaseVersion version = ReleaseVersion.FromTag("v65534.65534.65534-beta.0+build.01");
        Assert.Equal("65534.65534.65534-beta.0+build.01", version.Value);
        Assert.Equal("65534.65534.65534.0", version.NumericVersion);
        Assert.Equal("preview", version.Channel);
        Assert.Equal("stable", ReleaseVersion.FromTag("v0.0.0").Channel);
        Assert.Throws<ArgumentException>(() => ReleaseVersion.FromTag("v1.0.0-" + new string('a', 201)));
    }
}
