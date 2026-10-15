namespace Game.Core;

public enum CardRarity { Common, Uncommon, Relic }
public sealed record DoctrineDefinition(string Id, string Name, string Crest, string Category, string Theme,
    string Benefits, string Drawback, ResourceCost StartingBonus, ResourceCost StartingPenalty,
    int WatchpostPercent = 0, int MeleePercent = 0, int MageSecondaryPercent = 0, int CrossbowRange = 0,
    int CrossbowHealthPercent = 0, int Research = 0, bool Merchant = false, bool Grovekeeper = false);
public sealed record RewardCard(string Id, string Name, CardRarity Rarity, int MaxStacks, string Effect,
    int Amount = 0, ResourceCost Resources = default, bool Multiplied = false);
public sealed record AcquiredCard(string Id, int Count);
public sealed record RunState
{
    public string Doctrine { get; init; } = "warden";
    public bool Started { get; init; }
    public AcquiredCard[] Cards { get; init; } = [];
    public int DraftWave { get; init; }
    public ulong OfferSeed { get; init; }
    public string[] Offers { get; init; } = [];
    public string? Choice { get; init; }
    public int MerchantWave { get; init; }
    public int CodexClears { get; init; }
    public int EnemyKills { get; init; }
    public int[] FirstAttackSquads { get; init; } = [];
    public int[] WarhornSquads { get; init; } = [];
    public int Stacks(string id) => Cards.FirstOrDefault(c => c.Id == id)?.Count ?? 0;
    public RunState Detach() => this with { Cards = Cards.ToArray(), Offers = Offers.ToArray(), FirstAttackSquads = FirstAttackSquads.ToArray(), WarhornSquads = WarhornSquads.ToArray() };
}
public static class RunCatalog
{
    public static IReadOnlyList<DoctrineDefinition> Doctrines { get; } = Array.AsReadOnly<DoctrineDefinition>(
    [
        new("warden", "The Warden", "♜", "Fortress / survival", "Stand as the old forest stands.", "+2 Stone; Watchpost +25% maximum HP", "−2 starting Gold", new(Stone: 2), new(Gold: 2), WatchpostPercent: 25),
        new("merchant", "The Merchant", "⚖", "Trade / flexibility", "Every road brings opportunity.", "+4 Gold; first Market sale each wave +1 Gold (once per transaction)", "−2 starting Wood", new(Gold: 4), new(Wood: 2), Merchant: true),
        new("warmaster", "The Warmaster", "⚔", "Aggressive melee", "Steel answers the darkness.", "+4 Metal, +6 Food; Swordsman/Berserker +10% primary damage", "+1 Metal per Swordsman/Berserker recruit", new(Metal: 4, Food: 6), default, MeleePercent: 10),
        new("arcanist", "The Arcanist", "✦", "Magic / research", "Read the secrets beneath the roots.", "+3 Cloth, +3 Stone, +1 Research; Mage +10% secondary damage", "−3 starting Gold", new(Cloth: 3, Stone: 3), new(Gold: 3), MageSecondaryPercent: 10, Research: 1),
        new("grovekeeper", "The Grovekeeper", "♧", "Sustainable economy", "Tend today what shelters tomorrow.", "+2 Wood; +1 Wood with any Lumbermill, +1 Food with any Farm per production (once per city)", "+1 Metal per Swordsman/Berserker/Crossbowman; +1 Cloth per Mage recruit", new(Wood: 2), default, Grovekeeper: true),
        new("huntsman", "The Huntsman", "➶", "Ranged / positioning", "Keep the enemy beyond the treeline.", "+2 Metal; Crossbowmen +1 range", "Crossbowmen −10% maximum HP", new(Metal: 2), default, CrossbowRange: 1, CrossbowHealthPercent: -10)
    ]);
    public static IReadOnlyList<RewardCard> Cards { get; } = Array.AsReadOnly<RewardCard>(
    [
        new("E01", "The Lumber Guild", CardRarity.Common, 1, "Every Lumbermill +1 Wood per production; additive with Worldtree.", 1),
        new("E02", "Bountiful Harvest", CardRarity.Common, 1, "Every Farm +1 Food per production; additive with research.", 1),
        new("E03", "Golden Veins", CardRarity.Common, 1, "Every Gold Mine +1 Gold per production.", 1),
        new("E04", "Master Stonecutters", CardRarity.Common, 1, "Every Stonecutter +1 Stone per production.", 1),
        new("E05", "Deep Mining", CardRarity.Uncommon, 1, "Every Metal Mine +1 Metal per production.", 1),
        new("E06", "The Weaver's Pact", CardRarity.Uncommon, 1, "Every Weaver +1 Cloth per production.", 1),
        new("A01", "Hardened Shields", CardRarity.Common, 2, "Swordsmen +10% maximum HP per stack; living and future recruits.", 10),
        new("A02", "Berserker's Fury", CardRarity.Common, 2, "Berserkers +12% primary damage per stack.", 12),
        new("A03", "Precision Fletching", CardRarity.Common, 2, "Crossbowmen +10% primary damage per stack.", 10),
        new("A04", "Arcane Barrage", CardRarity.Common, 2, "Mages +12% secondary damage per stack, within the engaged squad only.", 12),
        new("A05", "Veteran Drills", CardRarity.Uncommon, 1, "All friendly soldiers +5% maximum HP, present and future.", 5),
        new("A06", "Efficient Rations", CardRarity.Uncommon, 1, "Berserker/Mage upkeep −1 Food per participating member (minimum 1).", 1),
        new("T01", "Reinforced Watchposts", CardRarity.Common, 2, "Watchpost +20% maximum HP per stack; resets next battle.", 20),
        new("T02", "Sharpened Shafts", CardRarity.Common, 2, "Arrow/Rapid Arrow towers +10% damage per stack.", 10),
        new("T03", "Heavy Ammunition", CardRarity.Common, 2, "Catapult/Bombard towers +10% primary and secondary damage per stack.", 10),
        new("T04", "Eagle's Eye", CardRarity.Uncommon, 1, "Arrow/Rapid Arrow towers +1 attack range.", 1),
        new("T05", "Rapid Mechanisms", CardRarity.Uncommon, 1, "Arrow/Rapid Arrow towers full attack cadence −10%; positive windup/recovery.", 10),
        new("T06", "Quick Winches", CardRarity.Uncommon, 1, "Catapult/Bombard towers full attack cadence −10%; positive windup/recovery.", 10),
        new("M01", "Spell Focus", CardRarity.Common, 2, "Mages +10% primary damage per stack.", 10),
        new("M02", "Arcane Endurance", CardRarity.Common, 2, "Mages +12% maximum HP per stack.", 12),
        new("M03", "Frost Runes", CardRarity.Common, 1, "Mage attacks +10 percentage points Chill, even without research; cap 50%, existing duration.", 10),
        new("M04", "Ember Sigil", CardRarity.Uncommon, 1, "Mage attacks +10 percentage points Burn, even without research; cap 40%, existing duration.", 10),
        new("M05", "Venom-Tipped Bolts", CardRarity.Uncommon, 1, "Crossbow attacks +5 percentage points Poison, even without research; cap 20%, existing stack cap/duration.", 5),
        new("M06", "Elemental Conduit", CardRarity.Uncommon, 1, "Friendly soldiers +8% primary damage against a squad already afflicted by Burn, Poison or Chill before impact; once only.", 8),
        new("F01", "Timber Shipment", CardRarity.Common, 2, "Immediately +4 Wood × reward multiplier.", Resources: new(Wood: 4), Multiplied: true),
        new("F02", "Emergency Provisions", CardRarity.Common, 2, "Immediately +8 Food × reward multiplier.", Resources: new(Food: 8), Multiplied: true),
        new("F03", "Equipment Shipment", CardRarity.Common, 2, "Immediately +3 Metal and +2 Cloth × reward multiplier.", Resources: new(Metal: 3, Cloth: 2), Multiplied: true),
        new("F04", "Field Surgeons", CardRarity.Uncommon, 1, "Immediately restore up to 10 Keep HP and heal each living friendly soldier by 20% of missing HP, including storage/reserves. No resurrection.", 20),
        new("F05", "Forgotten Knowledge", CardRarity.Uncommon, 1, "Immediately +2 personal Worldtree Research points.", 2),
        new("F06", "Royal Treasury", CardRarity.Common, 2, "Immediately +5 Gold × reward multiplier.", Resources: new(Gold: 5), Multiplied: true),
        new("R01", "Echoing Warhorn", CardRarity.Relic, 1, "First committed attack action per friendly squad each battle +25% primary and secondary damage.", 25),
        new("R02", "Ashen Quiver", CardRarity.Relic, 1, "Crossbow hits also apply Burn equal to 8% of actual damaging hit; existing Burn timing.", 8),
        new("R03", "Iron Harvest", CardRarity.Relic, 1, "Each successful battle +1 Metal per 3 enemy members killed by this city's soldiers, maximum 5 Metal.", 3),
        new("R04", "Crystal Prism", CardRarity.Relic, 1, "Mages +20% secondary damage, within the engaged squad only.", 20),
        new("R05", "Ancient Codex", CardRarity.Relic, 1, "+1 Research every second successful clear after acquisition; acquisition clear excluded.", 2),
        new("R06", "Guardian Roots", CardRarity.Relic, 1, "Keep receives 15% less structural damage while its actual Watchpost is alive.", 15)
    ]);
    public static DoctrineDefinition Doctrine(string id) => Doctrines.Single(d => d.Id == id);
    public static RewardCard Card(string id) => Cards.Single(c => c.Id == id);
    public static int Amount(RunState? run, string id) => (run?.Stacks(id) ?? 0) * Card(id).Amount;
}

// All run percentage arithmetic uses positive ceiling, checked 64-bit products,
// checked int conversion; zeros stay zero, HP >= 1 and cadence parts >= 1.
// Stat percentages add before this single scale; existing research foundation is
// the previous stage. Existing living current HP is retained (clamped to a
// reduced maximum); increasing a maximum never heals or resurrects a veteran.
public static class RunEffects
{
    public static int Scale(int value, int percent) => checked((int)((checked((long)value * (100 + percent)) + 99) / 100));
    public static int Portion(int value, int percent) => checked((int)((checked((long)value * percent) + 99) / 100));
    public static int Multiplier(int wave) => wave <= 6 ? 1 : wave <= 13 ? 2 : 3;
    public static UnitCapabilities Capabilities(UnitCapabilities c, RunState? run, UnitType type)
    {
        if (run is null || !run.Started) return c;
        DoctrineDefinition d = RunCatalog.Doctrine(run.Doctrine);
        int hp = RunCatalog.Amount(run, "A05"), x = 0, y = 0, range = 0;
        switch (type)
        {
            case UnitType.Swordsman: hp += RunCatalog.Amount(run, "A01"); x += d.MeleePercent; break;
            case UnitType.Berserker: x += d.MeleePercent + RunCatalog.Amount(run, "A02"); break;
            case UnitType.Crossbowman: hp += d.CrossbowHealthPercent; x += RunCatalog.Amount(run, "A03"); range = d.CrossbowRange; c = c with { PoisonPercent = Math.Min(20, c.PoisonPercent + RunCatalog.Amount(run, "M05")), BurnPercent = Math.Max(c.BurnPercent, RunCatalog.Amount(run, "R02")) }; break;
            case UnitType.Mage: hp += RunCatalog.Amount(run, "M02"); x += RunCatalog.Amount(run, "M01"); y += d.MageSecondaryPercent + RunCatalog.Amount(run, "A04") + RunCatalog.Amount(run, "R04"); c = c with { ChillPercent = Math.Min(50, c.ChillPercent + RunCatalog.Amount(run, "M03")), BurnPercent = Math.Min(40, c.BurnPercent + RunCatalog.Amount(run, "M04")) }; break;
        }
        return c with { HealthPercent = hp, PrimaryPercent = x, SecondaryPercent = y, RangeBonus = range };
    }
    public static ResourceCost Recruitment(ResourceCost cost, RunState? run, UnitType type)
    {
        if (run is null || !run.Started) return cost;
        bool grove = RunCatalog.Doctrine(run.Doctrine).Grovekeeper;
        ResourceCost extra = type == UnitType.Mage ? new(Cloth: grove ? 1 : 0)
            : new(Metal: grove || run.Doctrine == "warmaster" && type is UnitType.Swordsman or UnitType.Berserker ? 1 : 0);
        if (!cost.TryAdd(extra, out ResourceCost total)) throw new OverflowException();
        return total;
    }
    public static int Upkeep(int upkeep, RunState? run, UnitType type) => Math.Max(1, upkeep - (type is UnitType.Berserker or UnitType.Mage ? RunCatalog.Amount(run, "A06") : 0));
    public static TowerDefinition Tower(TowerDefinition tower, RunState? run)
    {
        bool catapult = tower.Type is Building.CatapultTower or Building.BombardTower;
        int percent = RunCatalog.Amount(run, catapult ? "T03" : "T02");
        int speed = RunCatalog.Amount(run, catapult ? "T06" : "T05");
        int cadence = Math.Max(2, Scale(tower.CadenceTicks, -speed));
        int windup = Math.Clamp(Scale(tower.WindupTicks, -speed), 1, cadence - 1);
        return tower with { Damage = Scale(tower.Damage, percent), SecondaryDamage = Scale(tower.SecondaryDamage, percent), HexRange = tower.HexRange + (catapult ? 0 : RunCatalog.Amount(run, "T04")), CadenceTicks = cadence, WindupTicks = windup };
    }
    public static int MerchantBonus(RunState run, int wave) => run.Started && RunCatalog.Doctrine(run.Doctrine).Merchant && run.MerchantWave != wave ? 1 : 0;
    public static string? ProducerCard(Building building) => building switch { Building.Lumbermill => "E01", Building.Farm => "E02", Building.Mine => "E03", Building.Stonecutter => "E04", Building.MetalMine => "E05", Building.Weaver => "E06", _ => null };
    public static int Watchpost(int health, RunState run) => Scale(health, run.Started ? RunCatalog.Doctrine(run.Doctrine).WatchpostPercent + RunCatalog.Amount(run, "T01") : 0);
    public static (int Common, int Uncommon, int Relic) Weights(int wave) => wave <= 4 ? (75, 25, 0) : wave <= 6 ? (60, 40, 0) : wave <= 9 ? (60, 35, 5) : (45, 40, 15);
    // Independent pure namespaces: no mutable combat RNG or UI enumeration.
    public static ulong Seed(ulong runSeed, ulong stream, int wave, int city = 0) => SeededDecision.Mix(runSeed ^ stream ^ ((ulong)(uint)wave << 32) ^ (uint)city);
    public const ulong RewardStream = 0x524557415244UL, EncounterStream = 0x454E434F554E5445UL;
    public static bool Eligible(City city, RewardCard card, int wave, int keepMaximum)
    {
        RunState run = city.Run;
        if (city.Eliminated || run.Stacks(card.Id) >= card.MaxStacks || card.Rarity == CardRarity.Relic && (wave < 7 || run.Cards.Count(s => RunCatalog.Card(s.Id).Rarity == CardRarity.Relic) >= 2)) return false;
        if (card.Id == "F04" && city.Health >= keepMaximum && city.Soldiers.All(s => s.Health >= s.Profile.Health)) return false;
        if (card.Id == "F05") { try { _ = city.Research.Income(points: card.Amount); } catch (OverflowException) { return false; } }
        if (!card.Resources.TryMultiply(card.Multiplied ? Multiplier(wave) : 1, out ResourceCost resources) || !city.Resources.TryAdd(resources, out _)) return false;
        return card.Multiplied || card.Id is "F04" or "F05" || city.CanApplyRun(Acquire(run, card.Id));
        // Unlock-dependent passives remain meaningful for reachable future purchases.
    }
    public static RunState Acquire(RunState run, string id) => run with
    {
        Cards = run.Cards.Where(c => c.Id != id).Append(new AcquiredCard(id, run.Stacks(id) + 1)).OrderBy(c => c.Id, StringComparer.Ordinal).ToArray(),
        Choice = id,
        CodexClears = id == "R05" ? 0 : run.CodexClears
    };
    public static string[] Offers(City city, int wave, ulong seed, int keepMaximum)
    {
        var eligible = RunCatalog.Cards.Where(c => Eligible(city, c, wave, keepMaximum)).OrderBy(c => c.Id, StringComparer.Ordinal).ToList();
        var offers = new List<string>();
        var weights = Weights(wave);
        for (int slot = 0; slot < 3; slot++)
        {
            seed = SeededDecision.Mix(seed); int roll = (int)(seed % 100);
            CardRarity rarity = roll < weights.Common ? CardRarity.Common : roll < weights.Common + weights.Uncommon ? CardRarity.Uncommon : CardRarity.Relic;
            // Empty rarity falls back in Common, Uncommon, Relic order, never
            // violating early-wave rarity locks or the player's relic cap.
            var pool = eligible.Where(c => c.Rarity == rarity && (c.Rarity != CardRarity.Relic || weights.Relic > 0)).ToArray();
            if (pool.Length == 0)
            {
                RewardCard[] fallback = eligible.Where(c => c.Rarity != CardRarity.Relic || weights.Relic > 0).OrderBy(c => c.Rarity).ThenBy(c => c.Id, StringComparer.Ordinal).ToArray();
                pool = fallback.Length == 0 ? [] : fallback.Where(c => c.Rarity == fallback[0].Rarity).ToArray();
            }
            if (pool.Length == 0) throw new InvalidOperationException("Reward catalog exhausted before three distinct eligible offers.");
            seed = SeededDecision.Mix(seed); RewardCard chosen = pool[(int)(seed % (ulong)pool.Length)];
            offers.Add(chosen.Id); eligible.Remove(chosen);
        }
        return offers.ToArray();
    }
}
