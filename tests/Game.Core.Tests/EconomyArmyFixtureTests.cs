using System.Text.Json;
using DevRunner;
using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

// Historical paid transcripts deliberately migrated to three end-of-preparation
// settlements. Current commands/receipts own identities; historical ticks and
// enemy IDs are not new friendly goldens. No gameplay setters or resource grants.
public sealed class EconomyArmyFixtureTests(ITestOutputHelper output)
{
    internal const ulong Seed = 14056307608042553509UL;
    [Fact]
    public void RecoveredReconnectSeedNeedsAnAffordableHomeBeforeItsSeventhMixedRole()
    {
        using var match = new Match(combatSeed: 2723540745024433499UL); match.Join(); match.Join();
        void Act(int p, EconomyAction plan) => Assert.True(match.Apply(p, plan.Command(match.Snapshot(), p)).Accepted);
        void Ready() { Act(1, new("ready")); Act(2, new("ready")); }
        Act(1, new("start"));
        foreach (int p in new[] { 1, 2 })
        {
            Act(p, new("build", 0, Building.Farm)); Act(p, new("build", 1, Building.MetalMine)); Act(p, new("build", 4, Building.MetalMine));
            Act(p, new("build", 3, Building.Lumbermill, Payment: ConstructionPayment.GoldRecovery));
        }
        Ready(); Ready();
        foreach (int p in new[] { 1, 2 }) Act(p, new("build", 2, Building.Barracks));
        for (int i = 0; i < 4; i++) Act(2, new("recruit", 2));
        for (int i = 0; i < 6; i++) Act(1, new("recruit", 2));
        for (int i = 0; i < 2; i++) Act(2, new("recruit", 2));
        Assert.All(match.Players.Values, c => { Assert.Equal(4, c.Metal); Assert.Equal(10, c.Food); }); Ready();
        for (int i = 0; i < 3000 && (match.Phase == Phase.Combat || match.Combat.HasDeaths); i++) match.Step();
        Assert.Equal(Phase.Building, match.Phase); Assert.Equal(6, match.Players[1].Soldiers.Count);
        Assert.Equal([1000, 2000, 2000, 4000, 4000, 2000], match.Players[1].Soldiers.Select(u => u.Health));
        Act(1, new("sell", 4)); Act(1, new("build", 4, Building.ArcheryRange)); Ready(); Ready();
        CityState before = match.Snapshot().Players[0]; Assert.Equal(new ResourceCost(20, 3, Food: 24, Metal: 20), before.Resources);
        var ranged = new EconomyAction("recruit", 4, Unit: UnitType.Crossbowman);
        string unchanged = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
        Assert.False(match.Apply(1, ranged.Command(match.Snapshot(), 1)).Accepted);
        Assert.Equal(unchanged, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
        Act(1, new("buy-home")); Act(1, ranged);
        CityState after = match.Snapshot().Players[0];
        Assert.Equal(new ResourceCost(15, 1, Food: 24, Metal: 19), after.Resources);
        Assert.Equal(3, after.Army!.PurchasedHomes); Assert.Equal(7, after.Soldiers.Length);
        Assert.Equal(UnitType.Crossbowman, after.Soldiers[^1].Type);
        Assert.Equal(after.Army.Homes[2].Cell, after.Soldiers[^1].Assignment!.Tile);
        Assert.Equal(before.Soldiers.Select(u => (u.Id, u.Health, u.Assignment)), after.Soldiers.Take(6).Select(u => (u.Id, u.Health, u.Assignment)));
    }

    // Recovered network transcript: logs/20261004-050537-85a32d77.
    // Same default paid opening and seed; tick 391 retains the captured cleanup.
    [Fact]
    public void RecoveredNetworkSeedPaysForRoomBeforeExplicitRangedRecruitAndKeepsRetries()
    {
        const ulong seed = 11669866211869037831UL;
        using var flow = new SessionFlow(AuthorityPolicy.Dedicated, seed: seed);
        void Act(int player, EconomyAction action) => flow.Act(player, action);
        void Ready() { Act(1, new("ready")); Act(2, new("ready")); }
        void Recruit(int player, int count) { for (int i = 0; i < count; i++) Act(player, new("recruit", 1)); }
        CityState City() => flow.State.Players.Single(p => p.Id == 2);
        string Frozen() => JsonSerializer.Serialize(flow.State, WireJson.Options);
        Act(1, new("start"));
        foreach (int player in new[] { 1, 2 })
        {
            Act(player, new("build", 0, Building.Farm));
            Act(player, new("build", 4, Building.MetalMine));
            Act(player, new("build", 2, Building.MetalMine));
        }
        foreach (int player in new[] { 1, 2 })
        { Act(player, new("build", 3, Building.Lumbermill, Payment: ConstructionPayment.GoldRecovery)); Act(player, new("ready")); }
        Ready();
        foreach (int player in new[] { 1, 2 }) { Act(player, new("build", 1, Building.Barracks)); Act(player, new("buy-plot", 5)); }
        for (int batch = 0; batch < 3; batch++) { Recruit(1, 2); Recruit(2, 2); }
        Ready();
        flow.StepUntil(s => s.Phase == Phase.Building);
        Assert.Equal(2, flow.State.Wave);
        Act(2, new("sell", 4)); Act(2, new("buy-plot", 7)); Act(2, new("build", 7, Building.ArcheryRange));
        Act(1, new("upgrade", 1)); Ready();
        flow.StepUntil(s => s.Tick == 391);
        Recruit(1, 1); Act(1, new("ready")); Recruit(2, 2); Act(2, new("ready"));
        CityState before = City();
        Assert.Equal(seed, flow.State.CombatSeed);
        Assert.Equal(3, flow.State.Turn);
        Assert.Equal(new ResourceCost(7, 3, Food: 24, Metal: 16), before.Resources);
        Assert.Equal(2, before.Army!.PurchasedHomes);
        Assert.Equal([6, 6], before.Army.Homes.Where(h => h.Purchased).Select(h => h.Used));
        Assert.Equal([3, 7, 11, 12, 22, 23], before.Soldiers.Select(u => u.Id));
        Assert.Equal([4000, 1000, 2000, 3000, 4000, 4000], before.Soldiers.Select(u => u.Health));
        var ranged = new EconomyAction("recruit", 7, Unit: UnitType.Crossbowman);
        Command rejected = flow.Command(2, ranged); string unchanged = Frozen();
        CommandResult refusal = flow.Send(2, rejected);
        Assert.False(refusal.Accepted);
        Assert.Equal("Battlefield homes are full. Buy a home, store or retire a unit.", refusal.Message);
        Assert.Equal(unchanged, Frozen());
        Assert.Equal(refusal, flow.Send(2, rejected)); Assert.Equal(unchanged, Frozen());
        Assert.Equal(5, before.Army.HomePrices[before.Army.PurchasedHomes - 2]);
        long revision = flow.State.Revision;
        Command purchase = flow.Act(2, new("buy-home"));
        CityState expanded = City();
        Assert.Equal(revision + 1, flow.State.Revision);
        Assert.Equal(new ResourceCost(2, 3, Food: 24, Metal: 16), expanded.Resources);
        Assert.Equal(3, expanded.Army!.PurchasedHomes);
        unchanged = Frozen();
        Assert.True(flow.Send(2, purchase).Accepted); Assert.Equal(unchanged, Frozen());
        // Refused identities remain refused after capacity changes; a fresh request succeeds.
        Assert.Equal(refusal, flow.Send(2, rejected)); Assert.Equal(unchanged, Frozen());
        Command accepted = flow.Command(2, ranged);
        CommandResult result = flow.Send(2, accepted); Assert.True(result.Accepted);
        CityState after = City();
        Assert.Equal(revision + 2, flow.State.Revision);
        Assert.Equal(new ResourceCost(2, 1, Food: 24, Metal: 15), after.Resources);
        Assert.Equal(7, after.Soldiers.Length);
        Assert.Equal(before.Soldiers.Select(u => (u.Id, u.Health, u.Assignment)), after.Soldiers.Take(6).Select(u => (u.Id, u.Health, u.Assignment)));
        UnitState recruited = after.Soldiers[^1];
        Assert.Equal(UnitType.Crossbowman, recruited.Type);
        Assert.Equal(after.Army!.Homes[2].Cell, recruited.Assignment!.Tile);
        unchanged = Frozen(); Assert.Equal(result, flow.Send(2, accepted)); Assert.Equal(unchanged, Frozen());
        string gameplay = SessionFlow.Gameplay(flow.State);
        flow.Disconnect(2); flow.Rebind(2, 9);
        Assert.Equal(result, flow.Send(2, accepted)); Assert.Equal(gameplay, SessionFlow.Gameplay(flow.State));
        output.WriteLine(JsonSerializer.Serialize(new { seed, before.Resources, After = after.Resources, recruited.Id, Revision = revision + 2 }, WireJson.Options));
    }

    [Fact]
    public void SameSeedOrdinaryCampaignPreparesAndClearsTheThirdWaveWithoutFixtureGrants()
    {
        using var match = new Match(combatSeed: Seed); match.Join(); match.Join();
        Assert.True(match.Apply(1, new(1, match.Id, match.Phase, match.TurnSerial, "start", 1)).Accepted);
        MatchSnapshot? entry = null;
        int ticks = 0;
        while (match.Wave <= 3 && match.Phase is not (Phase.Defeat or Phase.Victory))
        {
            if (match.Phase == Phase.Combat || match.Phase == Phase.Preparation && match.Combat.HasDeaths)
            { Assert.True(ticks++ < 10000); match.Step(); continue; }
            foreach (City city in match.Players.Values)
            {
                int actions = 0;
                while (CampaignStrategy.Next(match.Snapshot(), city.Id) is EconomyAction plan)
                { Assert.True(actions++ < 100); Assert.True(match.Apply(city.Id, plan.Command(match.Snapshot(), city.Id)).Accepted); }
                MatchSnapshot current = match.Snapshot();
                Assert.True(match.Apply(city.Id, new EconomyAction("ready").Command(current, city.Id)).Accepted);
            }
            if (match.Phase == Phase.Combat && match.Wave == 3) entry = match.Snapshot();
        }
        Assert.NotNull(entry); Assert.Equal(Phase.Building, match.Phase); Assert.Equal(4, match.Wave);
        Assert.All(match.Players.Values, c => Assert.False(c.Eliminated));
        output.WriteLine(JsonSerializer.Serialize(new { Seed, Entry = entry.Players.Select(p => new { p.Id, p.Resources, p.Army, Units = p.Soldiers.Select(u => new { u.Id, u.Level, u.Health, u.Assignment }), p.LastUpkeep }), Result = match.Phase.ToString(), match.Tick }, WireJson.Options));
    }

    [Theory]
    [InlineData("old-arrow")]
    [InlineData("old-no-arrow")]
    [InlineData("home-only")]
    [InlineData("producer-only")]
    [InlineData("refill-only")]
    [InlineData("growth")]
    public void RecoveredPaidPreparationSeparatesEmptyCapacityFromEquippedCooperativeGrowth(string mode)
    {
        using var match = new Match(combatSeed: Seed); match.Join(); match.Join();
        bool early = mode is "producer-only" or "refill-only" or "growth";
        void Act(int player, string action, int slot = -1, Building building = Building.Empty, ConstructionPayment payment = ConstructionPayment.Standard, Resource resource = Resource.Metal, TechnologyId technology = TechnologyId.None)
        {
            var decision = new EconomyAction(action, slot, building, Technology: technology, Payment: payment, Resource: resource, Bundles: action == "trade" ? 1 : 0);
            CommandResult result = match.Apply(player, decision.Command(match.Snapshot(), player));
            Assert.True(result.Accepted, $"{mode} W{match.Wave} T{match.Turn} P{player}: {decision}: {result.Message}");
        }
        void Build(int p, int s, Building b) => Act(p, "build", s, b);
        void Ready() { Act(1, "ready"); Act(2, "ready"); }
        void Recruit(int player, int count) { for (int i = 0; i < count; i++) Act(player, "recruit", 2); }
        void Clear()
        {
            for (int i = 0; i < 10000 && (match.Phase == Phase.Combat || match.Combat.HasDeaths); i++) match.Step();
            Assert.False(match.Combat.HasDeaths);
        }
        Act(1, "start"); Build(1, 0, Building.Farm); Build(1, 1, Building.MetalMine); Build(1, 2, Building.Barracks);
        Build(2, 0, Building.Farm); Build(2, 1, Building.MetalMine); Build(2, 2, Building.Barracks); Act(2, "build", 3, Building.Lumbermill, ConstructionPayment.GoldRecovery);
        if (early) Act(1, "build", 3, Building.Lumbermill, ConstructionPayment.GoldRecovery);
        Ready(); Recruit(2, 2); Ready();
        if (early) Build(1, 4, Building.Stonecutter); else Act(1, "build", 3, Building.Lumbermill, ConstructionPayment.GoldRecovery);
        Build(2, 4, Building.Stonecutter); Recruit(2, 2); Recruit(1, 4);
        Assert.Equal(10, match.Players[1].Food); Assert.Equal(0, match.Players[1].Metal);
        int[] openingIds = match.Players[1].Soldiers.Select(u => u.Id).ToArray();
        Assert.Equal(4, openingIds.Length);
        Ready();
        Assert.Equal(openingIds, match.Players[1].LastUpkeep!.Funded);
        Clear(); Assert.Equal(Phase.Building, match.Phase);
        Assert.All(match.Players[1].Soldiers, u => { Assert.Contains(u.Id, openingIds); Assert.Equal(1, u.Owner); Assert.True(u.RecoveryEligible); });
        Assert.Contains(match.Players[1].Soldiers, u => u.Health < u.Profile.Health);
        Assert.Equal(early ? 1 : 0, match.Players[1].Stone);
        if (!early) Build(1, 4, Building.Stonecutter);
        Ready();
        // A common paid wave-2 refill (not a second opening mine) keeps the
        // receiving city alive so wave-3 capacity/growth remains a comparison.
        if (early) Act(1, "upgrade", 1);
        Ready();
        Recruit(2, 6 - match.Players[2].Soldiers.Count);
        Act(2, "sell", 2); Build(2, 2, Building.CatapultTower);
        // Preserve the producer rather than replacing it with a research tower:
        // final income cannot be spent before combat under the new contract.
        Recruit(1, 6 - match.Players[1].Soldiers.Count); Ready(); Clear();
        Assert.Equal(Phase.Building, match.Phase);
        Assert.All(match.Players[1].Soldiers, u => { Assert.Equal(1, u.Owner); Assert.True(u.RecoveryEligible); });
        if (mode == "old-arrow") { Act(2, "buy-plot", 5); Build(2, 5, Building.ArrowTower); }
        Ready();
        Act(2, "upgrade", 2);
        Ready(); Act(1, "upgrade", 2); Recruit(1, 1);
        CityState original = match.Snapshot().Players[0];
        Assert.Contains(original.Soldiers, u => u.Level == 2);
        Assert.Contains(original.Soldiers, u => u.Level == 1 && u.Health < u.Profile.Health);
        Assert.Equal(2, original.Army!.PurchasedHomes);
        if (mode == "home-only") { Act(1, "buy-home"); Act(2, "buy-home"); }
        if (mode is "refill-only" or "growth")
        {
            int target = mode == "growth" ? 9 : 6;
            while (match.Players[1].Soldiers.Count < target && match.Players[1].Metal >= original.RecruitmentQuotes.Single(q => q.Type == UnitType.Swordsman && q.Level == 2).Cost.Metal)
            {
                if (match.Players[1].Soldiers.Count >= match.Players[1].PurchasedHomes * 3) Act(1, "buy-home");
                Recruit(1, 1);
            }
        }
        if (mode == "growth")
        {
            int count = 0;
            while (CampaignStrategy.EconomyDefense(match.Snapshot(), 2) is EconomyAction plan)
            { Assert.True(count++ < 16); Assert.True(match.Apply(2, plan.Command(match.Snapshot(), 2)).Accepted); }
        }
        MatchSnapshot preparation = match.Snapshot();
        Assert.All(preparation.Players, p => Assert.All(p.Soldiers, u => Assert.Equal(u.Assignment!.Position, u.Hex!.Position)));
        Ready(); MatchSnapshot entry = match.Snapshot();
        Assert.All(entry.Players, p => { Assert.Empty(p.LastUpkeep!.Unfed); Assert.Equal(p.Soldiers.Length, p.LastUpkeep.Paid); Assert.Equal(p.FoodForecast!.Demand, p.LastUpkeep.Paid); });
        Assert.All(entry.Enemies, u => Assert.Equal(1, u.Level));
        Assert.All(entry.Players, p => { Assert.Equal(3, entry.Enemies.Count(u => u.Destination == p.Id && u.Type == UnitType.Swordsman)); Assert.Equal(2, entry.Enemies.Count(u => u.Destination == p.Id && u.Type == UnitType.Crossbowman)); });
        Clear();
        if (mode == "growth")
        {
            Assert.Equal(Phase.Building, match.Phase); Assert.Equal(4, match.Wave); Assert.Equal(3, match.LastRewardedWave);
            Assert.All(match.Players.Values, p => { Assert.False(p.Eliminated); Assert.Equal(3, p.LastReward!.Wave); });
            Assert.All(entry.Players, p => Assert.True(p.Soldiers.Length > 6 && p.Army!.PurchasedHomes > 2));
        }
        else if (mode == "refill-only") { Assert.Equal(Phase.Building, match.Phase); Assert.True(match.Players[2].Eliminated); }
        else { Assert.Equal(Phase.Defeat, match.Phase); Assert.Equal(DefeatReason.AllCitiesFallen, match.DefeatReason); }
        output.WriteLine(JsonSerializer.Serialize(new { Mode = mode, Seed, Preparation = preparation.Players.Select(p => new { p.Id, p.Resources, p.Army, Units = p.Soldiers.Select(u => new { u.Id, u.Level, u.Health, u.Assignment }) }), Outcome = match.Phase.ToString(), match.Tick, CityHealth = match.Players.Values.Select(p => p.Health), Receipts = entry.Players.Select(p => p.LastUpkeep) }, WireJson.Options));
    }
}
