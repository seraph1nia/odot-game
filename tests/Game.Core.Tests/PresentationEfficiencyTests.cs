using DevRunner;
using Game;
using Xunit;

namespace Game.Core.Tests;

public sealed class PresentationEfficiencyTests
{
    [Fact]
    public void HudUsesDisplayedValuesIncludingSameTickChangesAndOwnsComparisonTokens()
    {
        using Match match = CombatReplayFixture.StartFight(2, 1);
        MatchSnapshot state = match.Snapshot();
        var hud = new HudInvalidation();
        Dictionary<HudSection, object?[]> Capture(MatchSnapshot value, int slot = -1, bool connected = true, string feedback = "")
            => HudInvalidation.Capture(value, 1, slot, "Army", connected, "", feedback, 1, 1, false, false);
        var initial = Capture(state);
        Assert.All(initial, pair => Assert.True(hud.Refresh(pair.Key, pair.Value)));
        Assert.All(Capture(state with { Revision = state.Revision + 1, Tick = state.Tick + 1 }), pair => Assert.False(hud.Refresh(pair.Key, pair.Value)));
        CityState city = state.Players[0];
        MatchSnapshot City(CityState changed) => state with { Players = [changed, state.Players[1]] };
        void Changed(HudSection section, MatchSnapshot changed) => Assert.False(initial[section].SequenceEqual(Capture(changed)[section]));
        Changed(HudSection.Economy, City(city with { Gold = city.Gold + 1 }));
        Changed(HudSection.Context, City(city with { Research = new(Owned: 1UL << (int)TechnologyId.MeleeFoundation) }));
        Changed(HudSection.Economy, City(city with { Soldiers = city.Soldiers.Select((u, i) => i == 0 ? u with { Health = u.Health - 1 } : u).ToArray() }));
        StatusState poison = StatusPolicy.Apply(new(), new(1, StatusKind.Poison, 2, 1, 0, 100), new());
        MatchSnapshot afflicted = City(city with { Soldiers = city.Soldiers.Select((u, i) => i == 0 ? u with { Statuses = poison } : u).ToArray() });
        Changed(HudSection.Economy, afflicted);
        Assert.False(Capture(afflicted)[HudSection.Economy].SequenceEqual(Capture(afflicted with { Tick = afflicted.Tick + 1 })[HudSection.Economy]));
        object?[] remembered = Capture(afflicted)[HudSection.Economy];
        poison.Poison[0] = poison.Poison[0] with { Strength = 999 };
        Assert.False(remembered.SequenceEqual(Capture(afflicted)[HudSection.Economy]));
        Changed(HudSection.Economy, City(city with { Soldiers = city.Soldiers.Select(u => u with { Hex = u.Hex! with { Lifecycle = UnitLifecycle.Reserve } }).ToArray() }));
        Changed(HudSection.Economy, City(city with { LastUpkeep = new(1, 2, [99], [100]) }));
        Changed(HudSection.Roster, City(city with { Connected = !city.Connected }));
        Changed(HudSection.Status, state with { Paused = !state.Paused });
        Assert.False(initial[HudSection.Context].SequenceEqual(Capture(state, slot: 0)[HudSection.Context]));
        Assert.False(initial[HudSection.Context].SequenceEqual(Capture(state, connected: false)[HudSection.Context]));
        Assert.False(initial[HudSection.Status].SequenceEqual(Capture(state, feedback: "rejected")[HudSection.Status]));
        // Incoming nested arrays cannot silently update the remembered inputs.
        state.Players[0].Slots[0] = state.Players[0].Slots[0] with { Level = 99 };
        Assert.True(hud.Refresh(HudSection.Context, Capture(state)[HudSection.Context]));
    }

    [Fact]
    public void PlaybackIndexesOnlyAcceptedInputsAndDetachesRoutesEventsAndResults()
    {
        using Match match = CombatReplayFixture.StartFight(2, 1);
        MatchSnapshot state = match.Snapshot();
        UnitState first = state.Enemies[0];
        UnitState routed = first with { Decision = first.Decision! with { Route = [new(11, 1)], Visited = [17] } };
        state = state with { Enemies = [routed] };
        var work = new WorkCounters(); var playback = new CombatPlayback { Work = work };
        playback.Accept(state); work.Reset();
        routed.Decision!.Route[0] = new(999, 999);
        for (int frame = 0; frame < 10; frame++)
        {
            UnitState owned = playback.Units().Single(u => u.Id == first.Id);
            Assert.Equal((11, 1), (owned.Decision!.Route[0].Cell, owned.Decision.Route[0].Anchor));
            owned.Decision.Route[0] = new(777, 777);
        }
        Assert.Equal(0, work.Snapshot()[WorkMetric.PlaybackIndexBuilds]);
        Assert.Equal(0, work.Snapshot()[WorkMetric.Sorts]);
        long sequence = state.EventSequence + 1;
        var death = new CombatEvent(sequence, state.Tick + 2, CombatEventType.Death, routed) { Victims = [123] };
        MatchSnapshot next = state with
        {
            Revision = state.Revision + 1,
            Tick = state.Tick + 3,
            EventSequence = sequence + 1,
            OldestEventSequence = sequence,
            CombatEvents = [death, death with { Sequence = sequence + 1, Tick = state.Tick + 3 }]
        };
        Assert.True(playback.Accept(next)); Assert.True(playback.AwaitingDeath(first.Id));
        death.Victims[0] = 999;
        Assert.False(playback.Accept(next)); Assert.Equal(1, work.Snapshot()[WorkMetric.PlaybackIndexBuilds]);
        playback.Advance(2.0 / Match.StepsPerSecond, true);
        Assert.Equal(123, Assert.Single(playback.Drain()).Victims[0]); Assert.True(playback.AwaitingDeath(first.Id));
        playback.Advance(1.0 / Match.StepsPerSecond, true); Assert.Single(playback.Drain()); Assert.False(playback.AwaitingDeath(first.Id));
        playback.Accept(next with { Revision = next.Revision + 1 }, baseline: true);
        Assert.False(playback.AwaitingDeath(first.Id)); Assert.Empty(playback.Drain());
    }

    [Theory]
    [InlineData(true, 1, 1, true)]
    [InlineData(true, 2, 1, false)]
    [InlineData(false, 1, 1, false)]
    public void OnlyDeployedFocusedBodiesEvaluateSkeletons(bool deployed, int city, int focus, bool sample)
        => Assert.Equal(sample, PresentationLimits.SamplesPose(new(1, 8, 0, 1, city) { Deployed = deployed }, focus));
}
