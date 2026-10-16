using System.Text.Json;
using DevRunner;
using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

// Tier A paid schedule coverage: synthetic ample-balance recovery tests cannot
// catch missing joint material windows. Real default rules, fixed declared seeds,
// bounded early clears, no grants/children. Historical recipes are replaced as
// schedules, not granted balances or relaxed quotes.
public sealed class PaidPreparationScheduleTests(ITestOutputHelper output)
{
    private static void Act(Match match, string action, int slot = -1, Building building = Building.Empty,
        ConstructionPayment payment = ConstructionPayment.Standard, int unit = 0, UnitType type = UnitType.Swordsman)
    {
        PaidCampaignFixture.Apply(match, 1, new(action, slot, building, Unit: type, Payment: payment, UnitId: unit));
        if (action == "ready")
            foreach (City city in match.Players.Values.Where(c => c.Id != 1 && !c.Eliminated && !c.Ready))
            {
                for (int choices = 0; CampaignStrategy.Next(match.Snapshot(), city.Id, "support") is EconomyAction plan; choices++)
                {
                    Assert.True(choices < 100);
                    PaidCampaignFixture.Apply(match, city.Id, plan);
                }
                PaidCampaignFixture.Apply(match, city.Id, new("ready"));
            }
    }
    private static void Clear(Match match, int wave)
    {
        for (int i = 0; i < 10000 && (match.Phase == Phase.Combat || match.Combat.HasDeaths); i++) match.Step();
        RewardFixture.Resolve(match);
        Assert.Equal(Phase.Building, match.Phase); Assert.Equal(wave + 1, match.Wave); Assert.Equal(wave, match.LastRewardedWave);
        Assert.False(match.Combat.HasDeaths);
    }
    private static bool HasRoom(CityState city, UnitType type)
        => city.Army!.Homes.Any(h => h.Purchased && h.Plot.Type == Building.Empty && h.Used < city.Army.SquadCapacity
            && city.Soldiers.Where(u => u.Assignment is { Stored: false } a && a.Tile == h.Cell).All(u => u.Type == type));
    private static void Room(Match match, UnitType type)
    {
        if (!HasRoom(match.Snapshot().Players[0], type)) Act(match, "buy-home");
    }
    private static void Fill(Match match)
    {
        while (match.Players[1].Soldiers.Count(u => !u.Assignment!.Stored) < 6)
        {
            Room(match, UnitType.Swordsman);
            Act(match, "recruit", 2);
        }
    }

    [Fact]
    public void EarlyPaidLumberAndFinalStonecutterFundTheSpecialistSupplyChain()
    {
        using var match = new Match(combatSeed: 1); match.Join(); match.Join(); Act(match, "start");
        Act(match, "build", 0, Building.Farm); Act(match, "build", 1, Building.MetalMine); Act(match, "build", 2, Building.Barracks);
        Act(match, "build", 3, Building.Lumbermill, ConstructionPayment.GoldRecovery);
        Act(match, "ready"); Act(match, "ready"); Act(match, "build", 4, Building.Stonecutter);
        for (int i = 0; i < 4; i++) Act(match, "recruit", 2);
        Assert.Equal(new ResourceCost(10, Food: 10, Stone: 2), match.Players[1].Resources);
        Act(match, "ready");
        Assert.Equal(new ResourceCost(12, 1, Food: 11, Stone: 3, Metal: 4), match.Players[1].Resources);
        Clear(match, 1); Act(match, "ready"); Act(match, "ready");
        Assert.Equal(new ResourceCost(18, 4, Food: 26, Stone: 5, Metal: 12), match.Players[1].Resources);
        Act(match, "sell", 1); Act(match, "build", 1, Building.Weaver);
        Act(match, "sell", 4); Act(match, "build", 4, Building.Arcanum);
        Assert.Equal(new ResourceCost(13, 2, Food: 26, Stone: 2, Metal: 12), match.Players[1].Resources);
        // A spare individual place in a sword squad is not Berserker room.
        // Buy actual homogeneous field space with earned gold.
        Room(match, UnitType.Berserker);
        Act(match, "recruit", 2, type: UnitType.Berserker); Fill(match);
        Assert.Contains(match.Players[1].Soldiers, u => u.Type == UnitType.Berserker);
        Act(match, "ready"); Assert.Equal(4, match.Players[1].Cloth);
        Clear(match, 2); Act(match, "ready");
        Room(match, UnitType.Mage);
        Act(match, "recruit", 4, type: UnitType.Mage);
        Assert.Contains(match.Players[1].Soldiers, u => u.Type == UnitType.Mage);
    }

    // Tier A conjunction: real paid research/veterans, Market sales, replacement
    // trainer/equipment, exclusive tower and homogeneous capacity share one
    // default-rule authority. Ample-stock arithmetic cannot prove the joint
    // material windows. Retained seed, two bounded clears/nine ENDs; seconds,
    // no children/grants/alternate simulator. The separate single-mine control
    // above stays single-mine. Old order/W3 deadline is not a gameplay contract.
    [Fact]
    public void PaidResearchTradeTrainerAndExclusiveTowerShareEarnedVeteransAndFinalSettlement()
    {
        using var match = new Match(combatSeed: 14056307608042553509UL); match.Join(); match.Join();
        var ledger = new List<object>();
        CityState City(int p) => match.Snapshot().Players.Single(c => c.Id == p);
        void Record(string action, MatchSnapshot before, MatchSnapshot after)
        {
            ledger.Add(new
            {
                Action = action,
                before.Wave,
                before.Phase,
                before.Turn,
                BeforeProduction = before.ProductionCount,
                AfterProduction = after.ProductionCount,
                Before = before.Players,
                After = after.Players
            });
            output.WriteLine($"{action}: W{before.Wave}/{before.Phase}/{before.Turn}, production {before.ProductionCount}->{after.ProductionCount}; "
                + string.Join("; ", after.Players.Select(c => $"P{c.Id} {c.Resources}, research={c.Research}, Keep={c.Health}, members={c.Soldiers.Length}")));
        }
        void Send(int p, EconomyAction action)
        {
            MatchSnapshot before = match.Snapshot();
            try { PaidCampaignFixture.Apply(match, p, action); }
            finally { Record($"P{p} {action}", before, match.Snapshot()); }
        }
        void Ready()
        {
            MatchSnapshot before = match.Snapshot();
            Send(1, new("ready"));
            Assert.Equal(before.ProductionCount, match.ProductionCount);
            Send(2, new("ready"));
            Assert.Equal(before.ProductionCount + 1, match.ProductionCount);
            foreach (CityState city in before.Players)
            {
                int progress = before.Rules.Research.TowerLevelOneProgress * city.Slots.Count(s => s.Type == Building.ResearchTower && s.Level == 1)
                    + before.Rules.Research.TowerLevelTwoProgress * city.Slots.Count(s => s.Type == Building.ResearchTower && s.Level == 2);
                Assert.Equal(city.Research.Income(progress: progress), City(city.Id).Research);
            }
            _ = SessionFlow.RoundTrip(match.Snapshot());
        }
        void FillCity(int p, int trainer, int target)
        {
            for (int i = 0; City(p).Soldiers.Count(u => u.Assignment is { Stored: false }) < target; i++)
            {
                Assert.True(i < 12, "Bounded paid recruitment did not converge.");
                Assert.True(HasRoom(City(p), UnitType.Swordsman));
                Send(p, new("recruit", trainer));
            }
        }
        void EarnedClear(int wave)
        {
            MatchSnapshot entry = match.Snapshot();
            Assert.Equal(Phase.Combat, entry.Phase); Assert.Equal(wave, entry.Wave);
            try
            {
                for (int i = 0; i < 4000 && (match.Phase == Phase.Combat || match.Combat.HasDeaths); i++) match.Step();
                Assert.Equal(Phase.RewardDraft, match.Phase); Assert.Equal(wave + 1, match.Wave);
                Assert.Equal(wave, match.LastRewardedWave); Assert.False(match.Combat.HasDeaths);
                Assert.Equal(entry.ProductionCount, match.ProductionCount);
                foreach (CityState city in match.Snapshot().Players)
                {
                    Assert.False(city.Eliminated);
                    CityState old = entry.Players.Single(c => c.Id == city.Id);
                    Assert.True(old.Resources.TryAdd(city.LastReward!.Amount, out ResourceCost reward));
                    Assert.Equal(reward, city.Resources); Assert.Equal(old.Research.Income(points: 1), city.Research);
                    foreach (UnitState member in city.Soldiers)
                    {
                        UnitState veteran = old.Soldiers.Single(u => u.Id == member.Id);
                        Assert.Equal(veteran.Level, member.Level); Assert.Equal(veteran.Profile, member.Profile);
                        Assert.Equal(veteran.Assignment, member.Assignment); Assert.Equal(veteran.SquadId, member.SquadId);
                        Assert.Equal(veteran.FormationSlot, member.FormationSlot);
                        Assert.InRange(member.Health, 1, veteran.Health);
                        Assert.Equal(veteran.Experience + (veteran.Participating ? 1 : 0), member.Experience);
                    }
                }
                PaidCampaignFixture.Legal(SessionFlow.RoundTrip(match.Snapshot()));
                RewardFixture.Resolve(match);
            }
            finally { Record($"earned clear W{wave}", entry, match.Snapshot()); }
        }
        void Refuse(int p, EconomyAction action, string reason)
        {
            MatchSnapshot before = match.Snapshot();
            CommandResult refusal = match.Apply(p, action.Command(before, p, before.Revision + 1));
            Record($"refused P{p} {action}: {refusal.Message}", before, match.Snapshot());
            Assert.False(refusal.Accepted); Assert.Contains(reason, refusal.Message);
            Assert.Equal(JsonSerializer.Serialize(before, WireJson.Options), JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
        }
        try
        {
            Send(1, new("start"));
            foreach (int p in new[] { 1, 2 })
            {
                Send(p, new("build", 0, Building.Farm)); Send(p, new("build", 1, Building.MetalMine));
                Send(p, new("build", 4, Building.MetalMine));
                Send(p, new("build", 3, Building.Lumbermill, Payment: ConstructionPayment.GoldRecovery));
            }
            Ready(); Ready();
            foreach (int p in new[] { 1, 2 })
            {
                Send(p, new("sell", 4)); Send(p, new("build", 2, Building.Barracks)); FillCity(p, 2, 6);
            }
            Ready(); EarnedClear(1);

            foreach (int p in new[] { 1, 2 }) Send(p, new("build", 4, Building.Stonecutter));
            Ready(); Ready();
            foreach (int p in new[] { 1, 2 }) FillCity(p, 2, 6);
            Send(1, new("buy-plot", 5)); Send(1, new("build", 5, Building.ResearchTower));
            Send(2, new("upgrade", 2)); Send(2, new("buy-home")); FillCity(2, 2, 7);
            Ready(); EarnedClear(2);

            // Two actually bought Lumbermills fund the complete later schedule.
            Send(1, new("buy-plot", 6)); Send(1, new("build", 6, Building.Lumbermill));
            Send(2, new("buy-plot", 5)); Send(2, new("build", 5, Building.Barracks));
            Send(2, new("sell", 2)); Send(2, new("build", 2, Building.Lumbermill));
            Ready();
            Send(1, new("upgrade", 5)); Send(2, new("upgrade", 5));
            Ready();
            Assert.Equal(Phase.Preparation, match.Phase); Assert.Equal(3, match.Turn);
            CityState beforeResearch = City(1), foreign = City(2);
            UnitState[] wounded = beforeResearch.Soldiers.Where(u => u.Experience > 0 && u.Health < u.Profile.Health).ToArray();
            Assert.NotEmpty(wounded);
            Send(1, new("research-tech", Technology: TechnologyId.MeleeFoundation));
            foreach (UnitState old in beforeResearch.Soldiers)
            {
                UnitState veteran = City(1).Soldiers.Single(u => u.Id == old.Id);
                Assert.Equal(old.Health, veteran.Health); Assert.Equal(old.Level, veteran.Level);
                Assert.Equal(old.Experience, veteran.Experience); Assert.Equal(old.SquadId, veteran.SquadId);
                Assert.Equal(old.FormationSlot, veteran.FormationSlot); Assert.Equal(old.Assignment, veteran.Assignment);
                Assert.True(veteran.Profile.Health > old.Profile.Health); Assert.True(veteran.Profile.Damage > old.Profile.Damage);
            }
            Assert.Equal(JsonSerializer.Serialize(foreign, WireJson.Options), JsonSerializer.Serialize(City(2), WireJson.Options));
            Refuse(1, new("research-tech", Technology: TechnologyId.MeleeFoundation), "Acquired");
            Send(1, new("sell", 5)); Assert.True(City(1).Research.Has(TechnologyId.MeleeFoundation));
            // Complete the actual conjunction before another battle: the first
            // whole plan's W3 six/seven-member field lost a Keep and every fighter.
            // This proves no W3 victory. It retains real W1/W2 wounds and XP,
            // not the old arbitrary purchase order or a new synthetic injury.
            Send(1, new("build", 5, Building.Market));
            CityState beforeSale = City(1);
            MarketRate food = match.Snapshot().MarketRates.Single(r => r.Resource == Resource.Food);
            string unchanged = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
            BattleFoodForecast projection = BattleFood.Project(beforeSale, true, food)!;
            Assert.Equal(unchanged, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
            Send(1, new("trade", 5, Resource: Resource.Food, Bundles: 1));
            BattleFoodForecast received = City(1).FoodForecast!;
            Assert.Equal(JsonSerializer.Serialize(projection, WireJson.Options), JsonSerializer.Serialize(received, WireJson.Options));
            Send(1, new("buy-home")); FillCity(1, 2, 6);
            Send(1, new("upgrade", 2)); FillCity(1, 2, 9);
            Send(1, new("trade", 5, Resource: Resource.Metal, Bundles: 1));
            int missingBundles = City(1).Metal / match.Snapshot().MarketRates.Single(r => r.Resource == Resource.Metal).Units + 1;
            Refuse(1, new("trade", 5, Resource: Resource.Metal, Bundles: missingBundles), "stock");

            Send(2, new("buy-plot", 12)); FillCity(2, 5, 9);
            Send(2, new("sell", 4)); // Actual material refund completes tower wood.
            Send(2, new("build", 12, Building.ArrowTower));
            SlotState baseTower = City(2).Army!.Homes.Single(h => h.PlotId == 12).Plot;
            Send(2, new("upgrade-tower", 12, Building.RapidArrowTower));
            SlotState rapid = City(2).Army!.Homes.Single(h => h.PlotId == 12).Plot;
            Assert.Equal(baseTower.Generation, rapid.Generation); Assert.Equal(Building.RapidArrowTower, rapid.Type);
            Assert.Equal(2, rapid.Level);
            Assert.True(baseTower.Investment.TryAdd(match.Snapshot().BuildingCatalog.Single(b => b.Type == Building.ArrowTower).Upgrade, out ResourceCost investment));
            Assert.Equal(investment, rapid.Investment);
            Assert.Contains(City(2).Towers, t => t.Slot == 12 && t.Type == rapid.Type && t.Level == rapid.Level);
            Refuse(2, new("upgrade-tower", 12, Building.BombardTower), "base arrow tower");
            Assert.True(City(2).Resources.TryPay(City(2).RecruitmentQuotes.Single(q => q.Type == UnitType.Swordsman && q.Level == 2).Cost, out _));
            Refuse(2, new("recruit", 5), "No compatible squad space");

            // Real final income would fund this quote, but not before END.
            ResourceCost farmUpgrade = City(2).Slots[0].UpgradeQuote!.Value;
            Assert.False(City(2).Resources.TryPay(farmUpgrade, out _));
            Assert.True(City(2).Resources.TryAdd(City(2).ProductionIncome!.Value, out ResourceCost future));
            Assert.True(future.TryPay(farmUpgrade, out _));
            Refuse(2, new("upgrade", 0), "Not enough resources");
            MatchSnapshot finalPreparation = match.Snapshot();
            Assert.Equal(8, finalPreparation.ProductionCount); Assert.Equal(Phase.Preparation, finalPreparation.Phase);
            foreach (UnitState old in wounded)
            {
                UnitState veteran = City(1).Soldiers.Single(u => u.Id == old.Id);
                Assert.Equal(old.Health, veteran.Health); Assert.Equal(old.Experience, veteran.Experience);
                Assert.Equal(old.Level, veteran.Level); Assert.Equal(old.SquadId, veteran.SquadId);
                Assert.Equal(old.FormationSlot, veteran.FormationSlot); Assert.Equal(old.Assignment, veteran.Assignment);
            }
            foreach (CityState city in finalPreparation.Players)
            {
                Assert.Equal(9, city.Soldiers.Count(u => u.Assignment is { Stored: false }));
                Assert.Equal(3, city.Army!.Homes.Count(h => h.Purchased && h.Plot.Type == Building.Empty));
                Assert.Contains(city.Soldiers, u => u.Level == 1); Assert.Contains(city.Soldiers, u => u.Level == 2);
                Assert.Single(city.Slots, s => s.Type == Building.MetalMine);
                Assert.Equal(1, city.Slots.Single(s => s.Type == Building.MetalMine).Level);
            }
            Ready();
            MatchSnapshot combat = SessionFlow.RoundTrip(match.Snapshot());
            Assert.Equal(Phase.Combat, combat.Phase); Assert.Equal(3, combat.Wave); Assert.Equal(9, combat.ProductionCount);
            foreach (CityState city in combat.Players)
            {
                Assert.Equal(9, city.LastUpkeep!.Paid); Assert.Empty(city.LastUpkeep.Unfed);
                Assert.Equal(city.Soldiers.Select(u => u.Id).Order(), city.LastUpkeep.Funded.Order());
                Assert.Equal(city.Soldiers.Where(u => u.Assignment is { Stored: false }).Select(u => u.Id).Order(), city.LastUpkeep.Participating.Order());
            }
            foreach (CityState city in combat.Players)
                foreach (UnitState member in city.Soldiers)
                {
                    UnitState before = finalPreparation.Players.Single(c => c.Id == city.Id).Soldiers.Single(u => u.Id == member.Id);
                    Assert.Equal(before.Health, member.Health); Assert.Equal(before.Profile, member.Profile);
                    Assert.Equal(before.Level, member.Level); Assert.Equal(before.Experience, member.Experience);
                    Assert.Equal(before.SquadId, member.SquadId); Assert.Equal(before.FormationSlot, member.FormationSlot);
                    Assert.Equal(before.Assignment, member.Assignment);
                }
            Assert.True(combat.Players[0].Research.Has(TechnologyId.MeleeFoundation));
            Assert.Equal(Building.Market, combat.Players[0].Slots[5].Type);
            Assert.Equal(2, combat.Players[1].Slots[5].Level);
            Assert.Contains(combat.Structures, s => s.Owner == 2 && s.Slot == 12 && s.TowerType == Building.RapidArrowTower && s.Active && !s.Destroyed);
            Assert.True(City(2).Resources.TryPay(farmUpgrade, out _));
            Refuse(2, new("upgrade", 0), "building"); // Now affordable, but no fourth edit.
            PaidCampaignFixture.Legal(combat);
        }
        finally
        {
            string json = JsonSerializer.Serialize(ledger, WireJson.Options);
            string? directory = Environment.GetEnvironmentVariable("ODOT_CAMPAIGN_EVIDENCE_DIRECTORY");
            if (directory is not null)
            {
                Directory.CreateDirectory(directory); File.WriteAllText(Path.Combine(directory, "joint-paid-schedule.json"), json);
            }
            output.WriteLine($"Joint paid ledger: {ledger.Count} received before/after actions; JSON characters={json.Length}; full evidence directory={directory ?? "not selected"}.");
        }
    }

    [Fact]
    public void PaidHallAndIndependentUpgradeTracksRetainThreeLaterRecoverySettlements()
    {
        using var match = new Match(combatSeed: 1); match.Join(); Act(match, "start");
        Act(match, "build", 0, Building.Farm); Act(match, "build", 1, Building.MetalMine); Act(match, "build", 3, Building.Lumbermill);
        Act(match, "ready"); Act(match, "build", 4, Building.Stonecutter); Act(match, "ready");
        Act(match, "sell", 0); Act(match, "build", 2, Building.Barracks);
        for (int i = 0; i < 4; i++) Act(match, "recruit", 2);
        City city = match.Players[1]; Assert.Equal(new ResourceCost(14, Food: 10, Stone: 3), city.Resources);
        Act(match, "ready"); Assert.Equal(new ResourceCost(16, 1, Food: 6, Stone: 4, Metal: 4), city.Resources);
        Clear(match, 1); Assert.Equal(new ResourceCost(18, 2, Food: 11, Stone: 4, Metal: 4), city.Resources);
        Act(match, "build", 0, Building.Farm); Act(match, "ready");
        Act(match, "buy-plot", 5);
        string early = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
        CommandResult premature = match.Apply(1, Command.FromSnapshot(match.Snapshot(), 1000, "build", 1, 5, Building.TownHall));
        Assert.False(premature.Accepted); Assert.Contains("Not enough resources", premature.Message);
        Assert.Equal(early, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
        Act(match, "ready");
        Assert.Equal(new ResourceCost(17, 2, Food: 21, Stone: 6, Metal: 12), city.Resources);
        Act(match, "build", 5, Building.TownHall);
        while (city.Soldiers.Count < 4) Act(match, "recruit", 2);
        int[] veterans = city.Soldiers.Where(u => u.RecoveryEligible).OrderBy(u => u.Health).ThenBy(u => u.Id).Take(3).Select(u => u.Id).ToArray();
        Assert.Equal(3, veterans.Length);
        foreach (int id in veterans) Act(match, "store", 5, unit: id);
        Assert.Contains(city.Soldiers, u => !u.Assignment!.Stored);
        int reserve = veterans[0], wounded = city.Soldiers.Single(u => u.Id == reserve).Health;
        Fill(match); Assert.Equal(4, city.Stone);
        Assert.Equal(wounded, city.Soldiers.Single(u => u.Id == reserve).Health);
        Act(match, "buy-home"); Act(match, "send", 5, unit: reserve);
        int retired = city.Soldiers.Where(u => !u.Assignment!.Stored && u.Id != reserve).OrderBy(u => u.Health).ThenBy(u => u.Id).First().Id;
        Act(match, "retire", unit: retired);
        Assert.Equal(6, city.Soldiers.Count(u => !u.Assignment!.Stored));
        int[] stored = city.Soldiers.Where(u => u.Assignment!.Stored).Select(u => u.Id).ToArray();
        Assert.Equal(2, stored.Length);
        var beforeSettlement = stored.ToDictionary(id => id, id => city.Soldiers.Single(u => u.Id == id).Health);
        Act(match, "ready"); Assert.Equal(8, city.LastUpkeep!.Paid);
        Assert.Equal(stored.Order(), city.LastUpkeep.Funded.Except(city.LastUpkeep.Participating).Order());
        Assert.All(stored, id => Assert.Equal(Math.Min(city.Soldiers.Single(u => u.Id == id).Profile.Health, beforeSettlement[id] + 200), city.Soldiers.Single(u => u.Id == id).Health));
        Clear(match, 2); Assert.Equal(5, city.Stone);
        var health = stored.ToDictionary(id => id, id => city.Soldiers.Single(u => u.Id == id).Health);
        ResourceCost before = city.Resources;
        Act(match, "upgrade-healing", 5);
        Assert.Equal(new ResourceCost(before.Gold - 6, before.Wood - 2, before.Food, before.Stone - 2, before.Metal, before.Cloth), city.Resources);
        Assert.Equal(2, city.Slots[5].HealingLevel); Assert.Equal(1, city.Slots[5].CapacityLevel); Assert.Equal(3, city.PurchasedHomes);
        Assert.All(stored, id => Assert.Equal(health[id], city.Soldiers.Single(u => u.Id == id).Health));
        for (int production = 1; production <= 2; production++)
        {
            Act(match, "ready");
            Assert.All(stored, id => Assert.Equal(Math.Min(city.Soldiers.Single(u => u.Id == id).Profile.Health, health[id] + 400 * production), city.Soldiers.Single(u => u.Id == id).Health));
        }
        // Same earned Hall also replaces the obsolete reserve-W5 deadline and
        // prematurely purchased Hall fixtures: actual individual slot6, overflow,
        // returned healed identity and final-settlement/no-fourth-edit guards.
        // Quotes/production for every action use the shared paid receipt driver.
        UnitState returnable = city.Soldiers.Single(u => u.Id == stored[0]);
        Act(match, "send", 5, unit: returnable.Id);
        UnitState returned = city.Soldiers.Single(u => u.Id == returnable.Id);
        Assert.Equal(returnable.Health, returned.Health); Assert.Equal(returnable.Profile, returned.Profile);
        Assert.Equal(returnable.Level, returned.Level); Assert.Equal(returnable.Experience, returned.Experience);
        Assert.False(returned.Assignment!.Stored);
        Act(match, "store", 5, unit: returned.Id);
        // Actual clears have casualties, not a promised six field survivors.
        // Fill the genuinely paid base Hall6 using observed survivors/new paid
        // recruits, then prove its capacity refusal and the SAME seventh ID's
        // acceptance after independent capacity upgrade. Never guess troop IDs.
        for (int i = 0; city.Soldiers.Count(u => u.Assignment!.Stored) < 6; i++)
        {
            Assert.True(i < 6);
            UnitState? field = city.Soldiers.FirstOrDefault(u => !u.Assignment!.Stored);
            if (field is null)
            {
                int[] existing = city.Soldiers.Select(u => u.Id).ToArray();
                Act(match, "recruit", 2);
                field = Assert.Single(city.Soldiers, u => !existing.Contains(u.Id));
            }
            Act(match, "store", 5, unit: field.Id);
        }
        Assert.Equal(1, city.Slots[5].CapacityLevel); Assert.Equal(2, city.Slots[5].HealingLevel);
        int[] oldIds = city.Soldiers.Select(u => u.Id).ToArray();
        Act(match, "recruit", 2);
        int overflow = Assert.Single(city.Soldiers, u => !oldIds.Contains(u.Id)).Id;
        string unchanged = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
        CommandResult denied = match.Apply(1, Command.FromSnapshot(match.Snapshot(), 1000, "store", 1, 5, unitId: overflow));
        Assert.False(denied.Accepted); Assert.Contains("Destination has no available individual slot", denied.Message);
        Assert.Equal(unchanged, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
        Act(match, "upgrade-capacity", 5); Assert.Equal(2, city.Slots[5].CapacityLevel); Assert.Equal(2, city.Slots[5].HealingLevel);
        Act(match, "store", 5, unit: overflow);
        Assert.Equal(7, city.Soldiers.Count(u => u.Assignment!.Stored));
        Assert.Equal(6, city.Soldiers.Single(u => u.Id == overflow).Assignment!.Tile);
        Assert.Equal(12, match.Snapshot().Players[0].Army!.Halls.Single(h => h.Slot == 5).Capacity);
        MatchSnapshot beforeFinal = match.Snapshot();
        Act(match, "ready");
        MatchSnapshot received = SessionFlow.RoundTrip(match.Snapshot());
        Assert.Equal(Phase.Combat, received.Phase); Assert.Equal(3, received.Wave); Assert.Equal(9, received.ProductionCount);
        Assert.Equal(beforeFinal.Players[0].FoodForecast!.Paid, received.Players[0].LastUpkeep!.Paid);
        Assert.Equal(received.Players[0].Soldiers.Length, received.Players[0].LastUpkeep!.Paid); Assert.Empty(received.Players[0].LastUpkeep!.Unfed);
        foreach (UnitState member in received.Players[0].Soldiers)
        {
            UnitState old = beforeFinal.Players[0].Soldiers.Single(u => u.Id == member.Id);
            Assert.Equal(old.Level, member.Level); Assert.Equal(old.Experience, member.Experience);
            Assert.Equal(old.Profile, member.Profile); Assert.Equal(old.SquadId, member.SquadId);
            Assert.Equal(old.FormationSlot, member.FormationSlot); Assert.Equal(old.Assignment, member.Assignment);
            if (member.Assignment!.Stored)
            {
                Assert.Equal(ArmyConfiguration.HealedHealth(old.Health, old.Profile.Health, 10), member.Health);
                Assert.False(member.Participating); Assert.Equal(UnitLifecycle.Stored, member.Hex!.Lifecycle);
                Assert.Equal(default, member.Hex.Position);
                Assert.DoesNotContain(received.Reservations.Positions, p => p.UnitId == member.SquadId);
            }
            else Assert.Equal(old.Health, member.Health);
        }
        Assert.All(stored, id => Assert.Equal(Math.Min(city.Soldiers.Single(u => u.Id == id).Profile.Health, health[id] + 1200), city.Soldiers.Single(u => u.Id == id).Health));
        PaidCampaignFixture.Legal(received);
        output.WriteLine($"Earned Hall: paid base6-overflow then capacity12/healing10%, stored7/upkeep7; slot6 off-grid. Before={JsonSerializer.Serialize(beforeFinal.Players[0], WireJson.Options)} After={JsonSerializer.Serialize(received.Players[0], WireJson.Options)}");
    }
}
