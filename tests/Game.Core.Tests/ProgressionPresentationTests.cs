using Game;
using Xunit;

namespace Game.Core.Tests;

public sealed class ProgressionPresentationTests
{
    [Fact]
    public void CombatShowsItsPaidReceiptEvenWhenTheCurrentForecastIsUnfunded()
    {
        using var match = new Match(combatSeed: 1); City city = match.Join()!;
        CityState snapshot = city.Snapshot() with
        {
            FoodForecast = new(6, 0, 0, [], [1, 2, 3]),
            LastUpkeep = new(3, 6, [1, 2, 3], [])
        };
        ProgressionView view = ProgressionPresentation.Describe(match.Snapshot() with { Phase = Phase.Combat, Wave = 3 }, snapshot);
        Assert.Contains("paid 6 food", view.Food); Assert.Contains("Wave 3", view.Food); Assert.DoesNotContain("Next", view.Food);
        ProgressionView preview = ProgressionPresentation.Describe(match.Snapshot() with { Phase = Phase.Preparation }, snapshot);
        Assert.Contains("pay 0", preview.Food); Assert.Contains("3 reserve", preview.Food);
    }
    [Fact]
    public void VictoryKeepsTheActualFinalRewardAndUpkeepWithoutInventingAnotherBattle()
    {
        using var match = new Match(combatSeed: 1); City city = match.Join()!;
        CityState snapshot = city.Snapshot() with
        {
            FoodForecast = new(12, 30, 12, [], []),
            LastUpkeep = new(20, 9, [1], []),
            LastReward = new(20, true, new(20, 10, 10))
        };
        ProgressionView view = ProgressionPresentation.Describe(match.Snapshot() with { Phase = Phase.Victory, Wave = 20 }, snapshot);
        Assert.Contains("20 waves held", view.Phase); Assert.Contains("Wave 20: paid 9", view.Food);
        Assert.DoesNotContain("Next", view.Food); Assert.Contains("W20: +20 gold, +10 wood, +10 food", view.Reward);
    }
    [Fact]
    public void CitySwitchesRepeatedSnapshotsAndFreshSessionsDoNotReplayPreviousReceipts()
    {
        using var match = new Match(combatSeed: 1); City first = match.Join()!, second = match.Join()!;
        MatchSnapshot state = match.Snapshot() with { Phase = Phase.Building };
        CityState rewarded = first.Snapshot() with { LastReward = new(2, false, new(10, 5, 5)) };
        ProgressionView a = ProgressionPresentation.Describe(state, rewarded);
        Assert.Equal(a, ProgressionPresentation.Describe(state, rewarded));
        Assert.Equal("No clear reward yet", ProgressionPresentation.Describe(state, second.Snapshot()).Reward);
        using var fresh = new Match(combatSeed: 1); City newCity = fresh.Join()!;
        ProgressionView reset = ProgressionPresentation.Describe(fresh.Snapshot(), newCity.Snapshot());
        Assert.Equal("No clear reward yet", reset.Reward); Assert.Equal("No battle upkeep yet", reset.Food);
        Assert.Contains("Land 5/9", reset.Land); Assert.Contains("25 gold", reset.Land);
    }
    [Fact]
    public void PausedBossAndStalledDefeatKeepTheirAuthoritativeMeaning()
    {
        using var match = new Match(combatSeed: 1); City city = match.Join()!;
        MatchSnapshot state = match.Snapshot() with { Phase = Phase.Combat, Wave = 10, Paused = true };
        string boss = ProgressionPresentation.Describe(state, city.Snapshot()).Phase;
        Assert.Contains("PAUSED", boss); Assert.Contains("Wave 10/20 · BOSS", boss);
        string stalled = ProgressionPresentation.Describe(state with { Phase = Phase.Defeat, DefeatReason = DefeatReason.BattleStalled }, city.Snapshot()).Phase;
        Assert.Contains("battle stalled", stalled); Assert.DoesNotContain("all cities fell", stalled);
    }
    [Fact]
    public void ArmyDetailsDistinguishPreviewReservesActualReservesAndFedCapacityQueues()
    {
        using var match = new Match(combatSeed: 1); City city = match.Join()!;
        int high = match.Combat.Create(UnitType.Swordsman, 1, 1, 1, level: 5), low = match.Combat.Create(UnitType.Swordsman, 1, 1, 1);
        UnitState veteran = match.Combat.Read(high);
        Assert.Equal("S L5", ProgressionPresentation.UnitLabel(veteran));
        Assert.Equal("BOSS L5", ProgressionPresentation.UnitLabel(veteran with { IsBoss = true, Faction = Faction.Skeletons })); match.Combat.Seed(veteran with { Health = 2100 });
        CityState snapshot = city.Snapshot() with { FoodForecast = new(2, 1, 1, [high], [low]) };
        ProgressionView preview = ProgressionPresentation.Describe(match.Snapshot() with { Phase = Phase.Preparation }, snapshot);
        Assert.Contains($"#{high} Swordsman L5 · HP 21/133 · damage 33 · size 2 · next: fed", preview.Army);
        Assert.Contains($"#{low} Swordsman L1", preview.Army); Assert.Contains("next: reserve", preview.Army);
        UnitState[] units = snapshot.Soldiers.Select(u => u with { Deployed = false, Hex = u.Hex! with { Lifecycle = u.Id == high ? UnitLifecycle.Queued : UnitLifecycle.Reserve } }).ToArray();
        ProgressionView battle = ProgressionPresentation.Describe(match.Snapshot() with { Phase = Phase.Combat }, snapshot with { Soldiers = units });
        Assert.Contains("fed · capacity queue", battle.Army); Assert.Contains("· reserve", battle.Army); Assert.DoesNotContain("next:", battle.Army);
    }
}
