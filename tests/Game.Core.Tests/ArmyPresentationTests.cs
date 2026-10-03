using Game;
using Xunit;

namespace Game.Core.Tests;

public sealed class ArmyPresentationTests
{
    private static MatchSnapshot State()
    {
        using var match = new Match(); match.Join(); MatchSnapshot state = match.Snapshot(); CityState city = state.Players[0];
        SlotState[] slots = city.Slots.ToArray(); slots[1] = new(Building.TownHall, 1) { Purchased = true, Generation = 3, CapacityLevel = 1, HealingLevel = 1 };
        UnitState Unit(int id, bool stored) => new(id, 2901)
        {
            Owner = 1,
            Profile = new(4000, 1000, 1, 30, 12, 60) { Size = 2 },
            Assignment = new(stored ? 1 : -1, stored ? 3 : 0, stored ? 0 : 17, id),
            Hex = new(id, 1, Faction.Adventurers, stored ? UnitLifecycle.Stored : UnitLifecycle.Alive, stored ? default : new(17, id)),
            Deployed = !stored
        };
        UnitState[] soldiers = [Unit(1, false), Unit(2, true)];
        city = city with { Slots = slots, Soldiers = soldiers, Food = 1, FoodForecast = BattleFood.Forecast(soldiers, 1, match.Economy), Army = match.Army.Snapshot(soldiers, 2, slots) };
        return state with { Phase = Phase.Building, Players = [city] };
    }

    [Fact]
    public void StoredFoodAndRecoveryAreNotPresentedAsFieldParticipation()
    {
        MatchSnapshot state = State(); CityState city = state.Players[0];
        ProgressionView view = ProgressionPresentation.Describe(state, city);
        Assert.Contains("Field demand 1", view.Food); Assert.Contains("stored demand 1", view.Food);
        Assert.Contains("Town hall plot 2", view.Army); Assert.Contains("next: unfunded", view.Army); Assert.Contains("no completed paid recovery", view.Army);
        Assert.Equal("Unfunded soldiers", ProgressionPresentation.Economy(state, city, true).BalanceLabel);
        Assert.False(city.Soldiers[1].Participating); Assert.False(city.Soldiers[1].Deployed);
        UnitState paid = city.Soldiers[1] with { RecoveryEligible = true };
        CityState next = city with { Soldiers = [city.Soldiers[0], paid] };
        Assert.Contains("paid recovery eligible", ProgressionPresentation.Describe(state, next).Army);
        Assert.False(paid.Participating);
    }

    [Fact]
    public void HomeAndAssignmentChangesInvalidateDisabledActionsWithoutResourceChanges()
    {
        MatchSnapshot state = State(); var hud = new HudInvalidation();
        object?[] Context(MatchSnapshot snapshot) => HudInvalidation.Capture(snapshot, 1, 1, "Army", true, "", "", 1, 1, false, false)[HudSection.Context];
        Assert.True(hud.Refresh(HudSection.Context, Context(state))); Assert.False(hud.Refresh(HudSection.Context, Context(state)));
        CityState city = state.Players[0];
        MatchSnapshot retired = state with { Players = [city with { Soldiers = [city.Soldiers[0]] }] };
        Assert.True(hud.Refresh(HudSection.Context, Context(retired)));
        MatchSnapshot bought = retired with { Players = [retired.Players[0] with { Army = city.Army! with { PurchasedHomes = 3 } }] };
        Assert.True(hud.Refresh(HudSection.Context, Context(bought))); Assert.False(hud.Refresh(HudSection.Context, Context(bought)));
    }
}
