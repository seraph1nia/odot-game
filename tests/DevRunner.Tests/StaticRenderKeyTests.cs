using Game;
using Xunit;

namespace DevRunner.Tests;

public sealed class StaticRenderKeyTests
{
    [Fact]
    public void IdenticalInputsReuseOneDeterministicBufferKey()
        => Assert.Equal(StaticRenderKey.Create("source", "import", "4.7.2", "gl_compatibility"), StaticRenderKey.Create("source", "import", "4.7.2", "gl_compatibility"));

    [Theory]
    [InlineData("changed source", "import", "4.7.2", "gl_compatibility", StaticRenderKey.OptimizerVersion)]
    [InlineData("source", "changed import", "4.7.2", "gl_compatibility", StaticRenderKey.OptimizerVersion)]
    [InlineData("source", "import", "different engine", "gl_compatibility", StaticRenderKey.OptimizerVersion)]
    [InlineData("source", "import", "4.7.2", "different backend", StaticRenderKey.OptimizerVersion)]
    [InlineData("source", "import", "4.7.2", "gl_compatibility", "different optimizer")]
    public void EveryRelevantUpdateInvalidatesTheDerivedBuffer(string source, string import, string engine, string backend, string optimizer)
        => Assert.NotEqual(StaticRenderKey.Create("source", "import", "4.7.2", "gl_compatibility"), StaticRenderKey.Create(source, import, engine, backend, optimizer));
}
