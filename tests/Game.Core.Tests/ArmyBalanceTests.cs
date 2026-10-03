using System.Text.Json;
using DevRunner;
using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

public sealed class ArmyBalanceTests(ITestOutputHelper output)
{
    public static IEnumerable<object[]> Cases()
    {
        foreach (string curve in new[] { "candidate", "steeper" })
            foreach (string family in new[] { "frontline", "reserve" })
                foreach (ulong seed in new[] { 0UL, 1UL, 123UL }) yield return [curve, family, seed, 6, 5, true];
        foreach (ulong seed in new[] { 0UL, 1UL, 123UL })
        {
            yield return ["candidate", "frontline", seed, 2, 5, true];
            yield return ["candidate", "frontline", seed, 6, 1, true];
        }
        yield return ["candidate", "reserve", 1UL, 6, 5, false];
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void PaidChoicesRecordBoundedCapacityRecoveryAndChallengingControls(string curve, string family, ulong seed, int maximumHomes, int maximumTier, bool hallUpgrades)
    {
        int[] prices = curve == "candidate" ? [5, 8, 12, 18] : [5, 8, 16, 24];
        using var match = new Match(new Rules { Army = new() { HomePrices = prices } }, combatSeed: seed);
        City city = match.Join()!;
        int recruits = 0, retired = 0, stores = 0, sends = 0, housingGold = 0, hallGold = 0, equipmentMetal = 0, upkeep = 0, shortageWaves = 0, productionFood = 0, rewardFood = 0, ticks = 0;
        long healed = 0, cursor = 0;
        var homes = new List<object>(); var waves = new List<object>();
        var deaths = new HashSet<int>(); var attackers = new HashSet<int>(); var deployed = new HashSet<int>();
        int waveAttacks = 0;
        void Events()
        {
            CombatEvent[] events = match.Combat.Events();
            if (events.Length > 0) Assert.True(events[0].Sequence <= cursor + 1, "Calibration must not lose retained combat events.");
            foreach (CombatEvent e in events.Where(e => e.Sequence > cursor))
            {
                if (e.Unit is not { Faction: Faction.Adventurers, Owner: 1 } unit) continue;
                if (e.Type == CombatEventType.Death) deaths.Add(unit.Id);
                if (e.Type == CombatEventType.AttackStarted) { attackers.Add(unit.Id); waveAttacks++; }
            }
            if (events.Length > 0) cursor = events[^1].Sequence;
            foreach (CombatUnit unit in match.Combat.Units().Where(u => u.Owner == 1 && u.IsTargetable)) deployed.Add(unit.Id);
        }
        void Apply(EconomyAction action)
        {
            MatchSnapshot before = match.Snapshot(); CityState old = before.Players[0];
            Assert.True(match.Apply(1, action.Command(before, 1)).Accepted, $"{curve}/{family}/{seed}: {action}");
            CityState next = city.Snapshot();
            if (action.Action == "recruit")
            {
                recruits++; equipmentMetal += old.Metal - next.Metal;
                Assert.Equal(old.Food, next.Food);
            }
            if (action.Action == "retire") { retired++; Assert.Equal(old.Resources, next.Resources); Assert.DoesNotContain(next.Soldiers, u => u.Id == action.UnitId); }
            if (action.Action is "store" or "send")
            {
                UnitState original = old.Soldiers.Single(u => u.Id == action.UnitId), moved = next.Soldiers.Single(u => u.Id == action.UnitId);
                Assert.Equal(original.Health, moved.Health); Assert.Equal(original.Profile, moved.Profile); Assert.Equal(original.Level, moved.Level);
                Assert.Equal(original.RecoveryEligible, moved.RecoveryEligible); Assert.Equal(old.Resources, next.Resources);
                if (action.Action == "store") stores++; else sends++;
            }
            if (action.Action == "buy-home")
            {
                housingGold += old.Gold - next.Gold;
                homes.Add(new { Wave = before.Wave, before.Turn, Phase = before.Phase.ToString(), Count = next.Army!.PurchasedHomes, Paid = old.Gold - next.Gold });
            }
            if (action.Action is "upgrade-capacity" or "upgrade-healing" || action.Action == "build" && action.Building == Building.TownHall) hallGold += old.Gold - next.Gold;
            if (match.ProductionCount > before.ProductionCount)
            {
                productionFood += old.ProductionIncome!.Value.Food;
                foreach (UnitState unit in old.Soldiers.Where(u => u.Assignment is { Stored: true }))
                {
                    int increase = next.Soldiers.Single(u => u.Id == unit.Id).Health - unit.Health;
                    Assert.True(increase >= 0); if (!unit.RecoveryEligible) Assert.Equal(0, increase); healed += increase;
                }
            }
            if (match.Phase == Phase.Combat && before.Phase == Phase.Preparation)
            {
                attackers.Clear(); deployed.Clear(); waveAttacks = 0;
                BattleUpkeepReceipt receipt = next.LastUpkeep!; upkeep += receipt.Paid; if (receipt.Unfed.Length > 0) shortageWaves++;
                Assert.Equal(old.Food - receipt.Paid, next.Food); Assert.Equal(old.FoodForecast!.Funded, receipt.Funded);
                Assert.All(next.Soldiers.Where(u => u.Assignment is { Stored: true }), u => { Assert.False(u.Deployed); Assert.False(u.Participating); Assert.DoesNotContain(u.Id, receipt.Participating); });
            }
            Assert.InRange(city.PurchasedHomes, 2, maximumHomes);
            Assert.All(next.Army!.Homes, h => Assert.InRange(h.Used, 0, h.Purchased ? 6 : 0));
            Assert.All(next.Army.Halls, hall => Assert.InRange(next.Soldiers.Where(u => u.Assignment!.HallSlot == hall.Slot).Sum(u => u.Size), 0, hall.Capacity));
        }
        Apply(new("start"));
        while (match.Phase is not (Phase.Victory or Phase.Defeat))
        {
            if (match.Phase == Phase.Combat || match.Phase == Phase.Preparation && match.Combat.HasDeaths)
            {
                Assert.True(++ticks < 100000, "Paid calibration exceeded its fixed-tick bound.");
                int wave = match.Wave; Phase previous = match.Phase;
                match.Step(); if (ticks % 4 == 0 || match.Phase != previous) Events();
                if (previous == Phase.Combat && match.Phase != Phase.Combat)
                {
                    CityState current = city.Snapshot();
                    rewardFood += current.LastReward?.Wave == wave ? current.LastReward.Amount.Food : 0;
                    waves.Add(new
                    {
                        Wave = wave,
                        Field = current.Soldiers.Count(u => !u.Assignment!.Stored),
                        Stored = current.Soldiers.Count(u => u.Assignment!.Stored),
                        Deployed = deployed.Count,
                        Attackers = attackers.Count,
                        Attacks = waveAttacks,
                        city.Gold,
                        city.Food,
                        CityHealth = city.Health,
                        Homes = city.PurchasedHomes,
                        Tiers = current.Soldiers.GroupBy(u => u.Level).ToDictionary(g => g.Key, g => g.Count())
                    });
                }
                continue;
            }
            int actions = 0;
            while (CampaignStrategy.Next(match.Snapshot(), 1, family, maximumHomes, maximumTier, hallUpgrades) is EconomyAction plan)
            { Assert.True(actions++ < 100, "Paid roster policy must converge."); Apply(plan); }
            Apply(new("ready"));
        }
        while (match.Combat.HasDeaths) { match.Step(); Events(); }
        Events();
        Assert.NotEqual(DefeatReason.BattleStalled, match.DefeatReason);
        Assert.Equal(recruits - retired - city.Soldiers.Count, deaths.Count);
        Assert.All(city.Soldiers.Where(u => !u.Assignment!.Stored), u => Assert.Equal(u.Assignment!.Position, u.Hex!.Position));
        if (maximumHomes == 6 && maximumTier == 5 && family == "frontline") Assert.Equal(Phase.Victory, match.Phase);
        if (family == "reserve") { Assert.True(stores > 0); Assert.True(healed > 0); }
        output.WriteLine(JsonSerializer.Serialize(new
        {
            Curve = curve,
            Family = family,
            Seed = seed,
            MaximumHomes = maximumHomes,
            MaximumTier = maximumTier,
            HallUpgrades = hallUpgrades,
            Outcome = match.Phase.ToString(),
            match.Wave,
            Reason = match.DefeatReason.ToString(),
            Ticks = ticks,
            Recruits = recruits,
            Retired = retired,
            Casualties = deaths.Count,
            Stores = stores,
            Sends = sends,
            HealedHundredths = healed,
            HousingGold = housingGold,
            HallGold = hallGold,
            EquipmentMetal = equipmentMetal,
            Upkeep = upkeep,
            ShortageWaves = shortageWaves,
            ProducedFood = productionFood,
            RewardFood = rewardFood,
            FinalGold = city.Gold,
            FinalFood = city.Food,
            FinalField = city.Soldiers.Count(u => !u.Assignment!.Stored),
            FinalStored = city.Soldiers.Count(u => u.Assignment!.Stored),
            Homes = homes,
            Waves = waves
        }, WireJson.Options));
    }
}
