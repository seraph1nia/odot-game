using Xunit;

namespace Game.Core.Tests;

public sealed class SeededDecisionTests
{
    private static CombatDecisionKey Key(ulong seed = 123, CombatPurpose purpose = CombatPurpose.Target)
        => new(seed, new(1, 2, 3, 4), 2, 3, CombatActorKind.Unit, 17, 9, 0, purpose);
    [Theory]
    [InlineData(0UL, 0UL)]
    [InlineData(1UL, 0x5692161d100b05e5UL)]
    [InlineData(ulong.MaxValue, 0xb4d055fcf2cbbd7bUL)]
    [InlineData(0x9e3779b97f4a7c15UL, 0xe220a8397b1dcdafUL)]
    public void MixerGoldenVectorsPinUnsignedWraparound(ulong value, ulong expected) => Assert.Equal(expected, SeededDecision.Mix(value));
    [Fact]
    public void DecisionTupleAndLocalStreamHaveIndependentGoldenVectors()
    {
        Assert.Equal(0x61e00b65c3c582e7UL, SeededDecision.State(Key()));
        Assert.Equal(0xa0ba958fc5a130caUL, SeededDecision.Rank(Key())); Assert.Equal(1, SeededDecision.Choose(Key(), 3));
        Assert.Equal(0xc677cd5257529401UL, SeededDecision.Rank(Key(purpose: CombatPurpose.MovementRank)));
        Assert.Equal(0x61e590428c50a260UL, SeededDecision.Rank(Key(purpose: CombatPurpose.GoalFootprint)));
        Assert.Equal(0xd40514418001d439UL, SeededDecision.Rank(Key(purpose: CombatPurpose.Route)));
        Assert.Equal(0x14f60986309fe9e3UL, SeededDecision.Rank(Key(purpose: CombatPurpose.Formation)));
        Assert.Equal(0x1f483c0e7d2579d1UL, SeededDecision.Rank(Key(purpose: CombatPurpose.Splash)));
    }
    [Fact]
    public void UnchangedRetriesAndOtherPurposesNeverConsumeATargetChoice()
    {
        int first = SeededDecision.Choose(Key(), 7); ulong rank = SeededDecision.Rank(Key(purpose: CombatPurpose.MovementRank));
        for (int i = 0; i < 100; i++)
        {
            SeededDecision.Choose(Key(purpose: CombatPurpose.Splash), 3);
            Assert.Equal(first, SeededDecision.Choose(Key(), 7)); Assert.Equal(rank, SeededDecision.Rank(Key(purpose: CombatPurpose.MovementRank)));
        }
        Assert.True(Enumerable.Range(0, 16).Select(seed => SeededDecision.Choose(Key((ulong)seed), 3)).Distinct().Count() > 1);
        Assert.NotEqual(rank, SeededDecision.Rank(Key(purpose: CombatPurpose.MovementRank) with { Sequence = 10 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => SeededDecision.Choose(Key(), 0));
        Assert.Throws<ArgumentException>(() => SeededDecision.State(Key() with { City = 0 }));
    }
}
