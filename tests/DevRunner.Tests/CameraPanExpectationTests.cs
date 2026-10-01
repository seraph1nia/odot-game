using DevRunner;
using Xunit;

namespace DevRunner.Tests;

public sealed class CameraPanExpectationTests
{
    [Fact]
    public void ClipsOnlyTheAxisThatReachesItsBoundary()
    {
        var view = new CameraObservation { Size = 20, PanZ = -7, Rotation = [0, MathF.PI / 4, 0] };
        var expected = CameraPanExpectation.Calculate(view, .4, 1, ["D"]);
        Assert.Equal(4 / Math.Sqrt(2), expected.X, 5);
        Assert.Equal(-8, expected.Z);
        Assert.NotEqual(-7 - 4 / Math.Sqrt(2), expected.Z);
    }

    [Fact]
    public void NormalizesDiagonalsAndEquivalentBindings()
    {
        var view = new CameraObservation { Size = 4, Rotation = [0, 0, 0] };
        var diagonal = CameraPanExpectation.Calculate(view, 1, 1, ["W", "D"]);
        Assert.Equal(Math.Sqrt(2), diagonal.X, 5);
        Assert.Equal(-Math.Sqrt(2), diagonal.Z, 5);
        Assert.Equal(CameraPanExpectation.Calculate(view, 1, 1, ["D"]), CameraPanExpectation.Calculate(view, 1, 1, ["D", "Right"]));
    }
}
