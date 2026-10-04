using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Game.Core;

public enum Phase { Lobby, Building, Preparation, Combat, Victory, Defeat }
public enum Building { Empty, Mine, Farm, Barracks, Lumbermill, ArcheryRange, Arcanum, ResearchTower = 7, ArrowTower, CatapultTower, Stonecutter, MetalMine, Weaver, Market, TownHall }
public sealed record Rules
{
    public CombatSettings Combat { get; init; } = new();
    public ResearchSettings Research { get; init; } = new();
    public ArmySettings Army { get; init; } = new();
    public int CityHealth { get; init; } = 100;
    public int StartingGold { get; init; } = 12;
    public int BaseGold { get; init; } = 2;
    public int BuildCost { get; init; } = 4;
    public int UpgradeCost { get; init; } = 4;
    public int MineOutput { get; init; } = 1;
    public int MineOutputLevelTwo { get; init; } = 2;
    public int FarmOutputLevelTwo { get; init; } = 8;
    public int StartingWood { get; init; } = 6;
    public int WoodOutput { get; init; } = 1;
    public int WoodOutputLevelTwo { get; init; } = 2;
    public int FarmOutput { get; init; } = 5;
    public int StoneOutput { get; init; } = 1;
    public int StoneOutputLevelTwo { get; init; } = 2;
    public int MetalOutput { get; init; } = 4;
    public int MetalOutputLevelTwo { get; init; } = 8;
    public int ClothOutput { get; init; } = 4;
    public int ClothOutputLevelTwo { get; init; } = 8;
    public int SwordMetalCost { get; init; } = 2;
    public int BerserkerMetalCost { get; init; } = 3;
    public int CrossbowMetalCost { get; init; } = 1;
    public int CrossbowWoodCost { get; init; } = 2;
    public int MageClothCost { get; init; } = 3;
    public int MageGoldCost { get; init; } = 1;
    public int SwordUpkeep { get; init; } = 1;
    public int BerserkerUpkeep { get; init; } = 2;
    public int CrossbowUpkeep { get; init; } = 1;
    public int MageUpkeep { get; init; } = 2;
    public int SoldierHealth { get; init; } = 40;
    public int SoldierDamage { get; init; } = 10;
    public int DefenderDamage { get; init; } = 2;
    public int AttackTicks { get; init; } = 60;
    public int RangedHealth { get; init; } = 30;
    public int RangedDamage { get; init; } = 10;
    public double RangedReach { get; init; } = 3;
    public int MeleeWindupTicks { get; init; } = 12;
    public int RangedWindupTicks { get; init; } = 18;
    public int BerserkerHealth { get; init; } = 30;
    public int BerserkerDamage { get; init; } = 15;
    public int MageHealth { get; init; } = 25;
    public int MageDamage { get; init; } = 12;
    public CampaignDefinition Campaign { get; init; } = CampaignDefinition.Default();
}
public sealed record SlotState(Building Type, int Level)
{
    public bool Purchased { get; init; } = true;
    public long Generation { get; init; }
    public ResourceCost Investment { get; init; }
    public ResourceCost Refund => Investment.HalfRefund();
    public ResourceCost? UpgradeQuote { get; init; }
    public int CapacityLevel { get; init; }
    public int HealingLevel { get; init; }
}
public sealed record UnitState(int Id, int Health, int Cooldown = 0, int Origin = 0, int Destination = 0)
{
    public HexUnitState? Hex { get; init; }
    public CombatDecisionState? Decision { get; init; }
    public UnitType Type { get; init; }
    public Faction Faction { get; init; }
    public UnitClass Class => Catalogs.Class(Type);
    public int Level { get; init; } = 1;
    public bool Participating => Hex?.Lifecycle is not (UnitLifecycle.Reserve or UnitLifecycle.Stored);
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public ArmyAssignment? Assignment { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public bool RecoveryEligible { get; init; }
    public bool IsBoss { get; init; }
    public int Size => Profile.Size;
    public int Rank { get; init; }
    public int Owner { get; init; }
    public bool Deployed { get; init; } = true;
    public int TargetId { get; init; }
    public bool TargetCity { get; init; }
    public long AttackSequence { get; init; }
    public long ActionStartTick { get; init; }
    public long ImpactTick { get; init; }
    public long ReadyTick { get; init; }
    public bool PendingImpact { get; init; }
    public bool? AttackLanded { get; init; }
    public WeaponProfile Profile { get; init; }
    public UnitCapabilities Capabilities { get; init; }
    public StatusState Statuses { get; init; } = new();
}
public sealed record CityState(int Id, bool Connected, bool Ready, int Gold, int Food, int Health, SlotState[] Slots, UnitState[] Soldiers, int DefenderCooldown)
{
    public TowerState? Defender { get; init; }
    public int Wood { get; init; }
    public int Stone { get; init; }
    public int Metal { get; init; }
    public int Cloth { get; init; }
    public ResourceCost Resources => new(Gold, Wood, Food, Stone, Metal, Cloth);
    public ResourceCost? ProductionIncome { get; init; }
    public ResearchState Research { get; init; }
    public TechnologyEligibility[] Technologies { get; init; } = [];
    public RecruitmentQuote[] RecruitmentQuotes { get; init; } = [];
    public BattleFoodForecast? FoodForecast { get; init; }
    public WaveClearReceipt? LastReward { get; init; }
    public BattleUpkeepReceipt? LastUpkeep { get; init; }
    public TowerState[] Towers { get; init; } = [];
    public ArmyState? Army { get; init; }
    public bool Eliminated => Health <= 0;
}
public sealed record MatchSnapshot(string MatchId, long Revision, long Tick, Phase Phase, bool Paused, int Wave, int Turn, int TurnSerial, Rules Rules, CityState[] Players, UnitState[] Enemies)
{
    public ulong CombatSeed { get; init; }
    public int CombatRulesVersion { get; init; }
    public int DecisionAlgorithmVersion { get; init; } = 1;
    public CombatFingerprint ConfigurationFingerprint { get; init; }
    public UnitState[] DyingBodies { get; init; } = [];
    public CombatReservations Reservations { get; init; } = new([], []);
    public AdmissionBound[] Admissions { get; init; } = [];
    public EngagementProgress[] Engagements { get; init; } = [];
    public DefeatReason DefeatReason { get; init; }
    public StallDiagnostic? Stall { get; init; }
    public long WaveStartedTick { get; init; }
    public int TotalWaves { get; init; }
    public int LastRewardedWave { get; init; }
    public WaveDefinition[] WaveCatalog { get; init; } = [];
    public int ProductionCount { get; init; }
    public BuildingDefinition[] BuildingCatalog { get; init; } = [];
    public TechnologyDefinition[] TechnologyCatalog { get; init; } = [];
    public MarketRate[] MarketRates { get; init; } = [];
    public int[] PlotPrices { get; init; } = [];
    public TowerDefinition[] TowerCatalog { get; init; } = [];
    public UnitDefinition[] UnitCatalog { get; init; } = [];
    public long EventSequence { get; init; }
    public long OldestEventSequence { get; init; } = 1;
    public CombatEvent[] CombatEvents { get; init; } = [];
}
public enum ConstructionPayment { Standard, GoldRecovery }
public sealed record Command(long Sequence, string MatchId, Phase ExpectedPhase, int TurnSerial, string Action, int City = 0, int Slot = -1, Building Building = Building.Empty, UnitType SoldierType = UnitType.Swordsman, long ExpectedGeneration = 0, int ExpectedExpansionCount = -1, Resource Resource = Resource.Wood, int Bundles = 0, TechnologyId Technology = TechnologyId.None, ConstructionPayment Payment = ConstructionPayment.Standard, int UnitId = 0, int ExpectedHomeCount = -1, int ExpectedTrackLevel = -1)
{
    // Capture contextual expectations once, after the caller reserves identity.
    // Retries resend this Command, never reconstruct it from newer state.
    public static Command FromSnapshot(MatchSnapshot snapshot, long sequence, string action, int city,
        int slot = -1, Building building = Building.Empty, UnitType soldierType = UnitType.Swordsman,
        TechnologyId technology = TechnologyId.None, Resource resource = Resource.Wood, int bundles = 0,
        ConstructionPayment payment = ConstructionPayment.Standard, int unitId = 0)
    {
        CityState? target = snapshot.Players.FirstOrDefault(p => p.Id == city);
        SlotState? instance = target is not null && slot is >= 0 and < 9 ? target.Slots[slot] : null;
        int track = action switch
        {
            "upgrade-capacity" => instance?.CapacityLevel ?? -1,
            "upgrade-healing" => instance?.HealingLevel ?? -1,
            _ => -1
        };
        return new(sequence, snapshot.MatchId, snapshot.Phase, snapshot.TurnSerial, action, city, slot, building,
            soldierType, instance?.Generation ?? 0, target is null ? -1 : target.Slots.Count(s => s.Purchased) - 5,
            resource, bundles, technology, payment, unitId, target?.Army?.PurchasedHomes ?? -1, track);
    }
}
public sealed record CommandResult(long Sequence, bool Accepted, string Message);
public sealed class City
{
    private readonly CombatSimulation _combat;
    private readonly EconomyConfiguration _economy;
    private readonly CombatConfiguration _configuration;
    private readonly ArmyConfiguration _army;
    internal City(int id, Rules rules, CombatSimulation combat, EconomyConfiguration economy, CombatConfiguration configuration, ArmyConfiguration? army = null)
    {
        Id = id; Gold = rules.StartingGold; Wood = rules.StartingWood; Health = HealthPoints.FromWhole(rules.CityHealth); _combat = combat; _economy = economy; _configuration = configuration; _army = army ?? new(rules.Army, configuration.Board);
        Defender = new(id, -1, Building.Empty, 1);
    }
    public int Id { get; }
    public bool Connected { get; set; } = true;
    public bool Ready { get; set; }
    public int Gold { get; set; }
    public int Food { get; set; }
    public int Wood { get; set; }
    public int Stone { get; set; }
    public int Metal { get; set; }
    public int Cloth { get; set; }
    public ResourceCost Resources
    {
        get => new(Gold, Wood, Food, Stone, Metal, Cloth);
        internal set
        {
            if (!value.IsValid) throw new ArgumentException("Invalid city resources.", nameof(value));
            Gold = value.Gold; Wood = value.Wood; Food = value.Food; Stone = value.Stone; Metal = value.Metal; Cloth = value.Cloth;
        }
    }
    public WaveClearReceipt? LastReward { get; internal set; }
    public BattleUpkeepReceipt? LastUpkeep { get; internal set; }
    public ResearchState Research { get; set; }
    public Dictionary<int, TowerState> Towers { get; } = [];
    public int Health { get; set; }
    public bool Eliminated => Health <= 0;
    public SlotState[] Slots { get; } = Enumerable.Range(0, 9).Select(i => new SlotState(Building.Empty, 0) { Purchased = i < 5 }).ToArray();
    public int ExpansionCount => Slots.Count(s => s.Purchased) - 5;
    public int PurchasedHomes { get; internal set; } = 2;
    internal long NextBuildingGeneration { get; set; } = 1;
    public IReadOnlyList<UnitState> Soldiers => _combat.Soldiers(Id);
    public int DefenderCooldown { get; set; }
    public TowerState Defender { get; set; }
    public CityState Snapshot() => Snapshot(_combat.Soldiers(Id));
    internal CityState Snapshot(UnitState[] soldiers, bool terminal = false) => new(Id, Connected, Ready, Gold, Food, Health, Slots.Select(s => s with { UpgradeQuote = _economy.TryUpgrade(s.Type, s.Level, out ResourceCost quote) ? quote : null }).ToArray(), soldiers, DefenderCooldown)
    { Army = _army.Snapshot(soldiers, PurchasedHomes, Slots), ProductionIncome = terminal || Eliminated ? default(ResourceCost) : _economy.ProjectedProduction(Slots), Wood = Wood, Stone = Stone, Metal = Metal, Cloth = Cloth, Research = Research, Technologies = _economy.Research.Definitions().Select(n => _economy.Research.Eligibility(Research, n.Id)).ToArray(), RecruitmentQuotes = _economy.RecruitmentQuotes(_configuration, Research), FoodForecast = terminal ? null : BattleFood.Forecast(soldiers, Food, _economy), LastReward = LastReward, LastUpkeep = LastUpkeep is null ? null : LastUpkeep with { Participating = LastUpkeep.Participating.ToArray(), Unfed = LastUpkeep.Unfed.ToArray(), Funded = LastUpkeep.Funded.ToArray() }, Defender = Defender, Towers = Towers.Values.OrderBy(t => t.Slot).ToArray() };
}

public sealed class Match : IDisposable
{
    private readonly CombatSimulation _combat;
    private readonly UnitDefinition[] _unitCatalog;
    private readonly SettlementCommands _settlementCommands;
    internal CombatSimulation Combat => _combat;
    public void SetWorkCounters(WorkCounters? work)
    {
        work?.Support(WorkMetric.MatchSnapshots, WorkMetric.ProfileResolutions, WorkMetric.ProfileCacheMisses,
            WorkMetric.OccupancyChecks, WorkMetric.BfsSearches, WorkMetric.BfsDequeues, WorkMetric.BfsEdges);
        _combat.Work = work;
    }
    public Match(Rules? rules = null, string? matchId = null, ulong? combatSeed = null)
    {
        Rules = rules ?? new(); Id = matchId ?? Guid.NewGuid().ToString("N");
        if (Rules.AttackTicks <= Rules.MeleeWindupTicks || Rules.AttackTicks <= Rules.RangedWindupTicks
            || Rules.MeleeWindupTicks < 1 || Rules.RangedWindupTicks < 1 || Rules.SoldierHealth < 1 || Rules.RangedHealth < 1
            || Rules.BerserkerHealth < 1 || Rules.MageHealth < 1 || Rules.CityHealth < 1 || Rules.SoldierDamage < 0 || Rules.RangedDamage < 0 || Rules.BerserkerDamage < 0 || Rules.MageDamage < 0
            || Rules.DefenderDamage < 0 || !double.IsFinite(Rules.RangedReach) || Rules.RangedReach < 1)
            throw new ArgumentException("Invalid combat profile or timing.", nameof(rules));
        if (Rules.StartingGold < 0 || Rules.StartingWood < 0 || Rules.BaseGold < 0 || Rules.BuildCost < 0 || Rules.UpgradeCost < 0
            || Rules.MineOutput < 0 || Rules.MineOutputLevelTwo < 0 || Rules.FarmOutput < 0 || Rules.FarmOutputLevelTwo < 0
            || Rules.WoodOutput < 0 || Rules.WoodOutputLevelTwo < 0)
            throw new ArgumentException("Invalid economy or allocation.", nameof(rules));
        _ = HealthPoints.FromWhole(Rules.CityHealth);
        _ = HealthPoints.FromWhole(Rules.DefenderDamage);
        _ = Catalogs.Units(Rules);
        Configuration = new(Rules);
        Army = new(Rules.Army, Configuration.Board);
        Economy = new(Rules);
        Campaign = new(Rules.Campaign, Configuration);
        ConfigurationFingerprint = RulesIdentity.Combine(Configuration.Fingerprint, Economy.Fingerprint, Campaign.Fingerprint);
        // Retain only copied authoring data as well as the frozen live catalog.
        Rules = Rules with { Army = Army.Definition(), Combat = Rules.Combat with { Board = Configuration.Board.Definition() }, Campaign = Campaign.Definition() };
        _unitCatalog = Catalogs.Units(Rules);
        CombatSeed = combatSeed ?? BinaryPrimitives.ReadUInt64LittleEndian(RandomNumberGenerator.GetBytes(sizeof(ulong)));
        _combat = new(Rules, Configuration, CombatSeed);
        _settlementCommands = new(Economy, Army, Configuration, _combat);
    }
    public const int StepsPerSecond = 60;
    public Rules Rules { get; }
    public CombatConfiguration Configuration { get; }
    public EconomyConfiguration Economy { get; }
    public ArmyConfiguration Army { get; }
    public CampaignConfiguration Campaign { get; }
    public int LastRewardedWave { get; private set; }
    public CombatFingerprint ConfigurationFingerprint { get; }
    public ulong CombatSeed { get; }
    public string Id { get; }
    public SortedDictionary<int, City> Players { get; } = [];
    public IReadOnlyList<UnitState> Enemies => _combat.Enemies();
    public Phase Phase { get; private set; } = Phase.Lobby;
    public bool Paused { get; private set; }
    public long Revision { get; private set; }
    public long Tick { get; private set; }
    public int Wave { get; private set; } = 1;
    public int Turn { get; private set; } = 1;
    public int TurnSerial { get; private set; } = 1;
    public int ProductionCount { get; private set; }
    private int _nextPlayer = 1;
    private int[] _roster = [];
    private readonly SortedDictionary<int, EngagementProgress> _engagements = [];
    public DefeatReason DefeatReason { get; private set; }
    public StallDiagnostic? Stall { get; private set; }
    public long WaveStartedTick { get; private set; }

    public City? Join()
    {
        if (Phase != Phase.Lobby || Players.Count >= 4) return null;
        var city = new City(_nextPlayer++, Rules, _combat, Economy, Configuration, Army);
        Players.Add(city.Id, city); Revision++;
        return city;
    }
    public void SetConnected(int id, bool connected)
    {
        if (!Players.TryGetValue(id, out City? city) || city.Connected == connected) return;
        city.Connected = connected; city.Ready = false; Revision++;
        ResolveReady();
    }
    public MatchSnapshot Snapshot()
    {
        _combat.Work?.Add(WorkMetric.MatchSnapshots);
        var units = _combat.ProjectAll();
        var soldiers = units.Living.Where(u => u.Faction == Faction.Adventurers).GroupBy(u => u.Owner).ToDictionary(g => g.Key, g => g.ToArray());
        return new(Id, Revision, Tick, Phase, Paused, Wave, Turn, TurnSerial,
        Rules with { Army = Army.Definition(), Combat = Rules.Combat with { Board = Configuration.Board.Definition() }, Campaign = Campaign.Definition() }, Players.Values.Select(p => p.Snapshot(soldiers.GetValueOrDefault(p.Id) ?? [], Phase is Phase.Victory or Phase.Defeat)).ToArray(), units.Living.Where(u => u.Faction == Faction.Skeletons).ToArray())
        {
            TotalWaves = Campaign.TotalWaves,
            LastRewardedWave = LastRewardedWave,
            WaveCatalog = Campaign.Definition().Waves,
            ProductionCount = ProductionCount,
            BuildingCatalog = Economy.Buildings(),
            MarketRates = Economy.MarketRates(),
            TechnologyCatalog = Economy.Research.Definitions(),
            PlotPrices = Economy.PlotPrices(),
            TowerCatalog = Configuration.Towers.ToArray(),
            UnitCatalog = _unitCatalog.ToArray(),
            EventSequence = _combat.EventSequence,
            OldestEventSequence = _combat.OldestEventSequence,
            CombatEvents = _combat.Events(),
            CombatSeed = CombatSeed,
            CombatRulesVersion = Configuration.RulesVersion,
            ConfigurationFingerprint = ConfigurationFingerprint,
            DyingBodies = units.Dying,
            Reservations = _combat.Reservations,
            Admissions = _combat.Admissions,
            Engagements = _engagements.Values.ToArray(),
            DefeatReason = DefeatReason,
            Stall = Stall,
            WaveStartedTick = WaveStartedTick
        };
    }

    public CommandResult Apply(int sender, Command command)
    {
        CommandResult Reject(string message) => new(command.Sequence, false, message);
        if (!Players.TryGetValue(sender, out City? city) || !city.Connected) return Reject("Unauthenticated player.");
        if (command.MatchId != Id || command.ExpectedPhase != Phase || command.TurnSerial != TurnSerial) return Reject("Stale match, phase or turn.");
        if (command.City != 0 && command.City != sender) return Reject("You do not own this city.");
        if (Phase is Phase.Victory or Phase.Defeat) return Reject("Match finished. Restart the server for a new match.");
        if (!Enum.IsDefined(command.Payment) || command.Payment != ConstructionPayment.Standard && command.Action != "build") return Reject("Invalid construction payment choice.");
        if (command.Action is "pause" or "resume")
        {
            if (Phase == Phase.Lobby) return Reject("Start the match first.");
            bool paused = command.Action == "pause";
            if (Paused != paused) { Paused = paused; Revision++; if (!paused) ResolveReady(); }
            return new(command.Sequence, true, paused ? $"Player {sender} paused the match." : $"Player {sender} resumed the match.");
        }
        if (Paused) return Reject("Match paused.");
        if (command.Action == "start")
        {
            if (Phase != Phase.Lobby) return Reject("Match already started.");
            foreach (int absent in Players.Values.Where(p => !p.Connected).Select(p => p.Id).ToArray()) Players.Remove(absent);
            _roster = Players.Keys.ToArray(); Phase = Phase.Building; Revision++;
            return new(command.Sequence, true, "Match started.");
        }
        if (Phase is not (Phase.Building or Phase.Preparation) || city.Eliminated) return Reject("Only living cities can act during building.");
        if (command.Action is "ready" or "unready")
        {
            bool previous = city.Ready;
            city.Ready = command.Action == "ready";
            if (!ResolveReady()) { city.Ready = previous; return Reject("Resource balances would overflow during production or the next battle reward."); }
            Revision++;
            return new(command.Sequence, true, city.Ready ? "Ready." : "Readiness updated.");
        }
        if (city.Ready) return Reject("Unready before editing.");
        CommandResult result = _settlementCommands.Apply(city, command);
        if (result.Accepted) Revision++;
        return result;
    }
    private void RestoreHomes() => _combat.RestoreHomes();
    private bool ResolveReady()
    {
        if (Paused || Phase is not (Phase.Building or Phase.Preparation)) return true;
        City[] required = Players.Values.Where(p => p.Connected && !p.Eliminated).ToArray();
        if (required.Length == 0 || required.Any(p => !p.Ready)) return true;
        if (Phase == Phase.Preparation && _combat.HasDeaths) return true;
        var balances = new Dictionary<int, ResourceCost>();
        var research = new Dictionary<int, ResearchState>();
        var recovery = new Dictionary<int, int>();
        if (Phase == Phase.Building)
        {
            try
            {
                foreach (City city in Players.Values.Where(p => !p.Eliminated))
                {
                    ResourceCost income = Economy.Production(city.Slots);
                    if (!city.Resources.TryAdd(income, out ResourceCost balance)) return false;
                    balances.Add(city.Id, balance);
                    research.Add(city.Id, city.Research.Income(progress: Economy.ResearchProduction(city.Slots)));
                }
                recovery = _combat.RecoveryPlan(Players.Values.Where(p => !p.Eliminated));
            }
            catch (OverflowException) { return false; }
        }
        if (Phase == Phase.Preparation)
        {
            ResourceCost reward = Campaign.WaveRule(Wave).Reward;
            foreach (City city in Players.Values.Where(c => !c.Eliminated))
            {
                BattleFoodForecast forecast = _combat.FoodForecast(city.Id, city.Food, Economy);
                try { _ = city.Research.Income(points: 1); } catch (OverflowException) { return false; }
                if (!city.Resources.TryPay(new(Food: forecast.Paid), out ResourceCost afterFood) || !afterFood.TryAdd(reward, out _)) return false;
            }
        }
        foreach (City city in Players.Values) city.Ready = false;
        TurnSerial++;
        if (Phase == Phase.Preparation) { BeginWave(); Revision++; return true; }
        foreach ((int id, ResourceCost balance) in balances) { Players[id].Resources = balance; Players[id].Research = research[id]; }
        _combat.Heal(recovery);
        ProductionCount++;
        if (Turn == 3) Phase = Phase.Preparation;
        else Turn++;
        Revision++; return true;
    }
    private void BeginWave()
    {
        Phase = Phase.Combat; WaveStartedTick = Tick; _engagements.Clear();
        City[] living = Players.Values.Where(p => !p.Eliminated).ToArray();
        var food = living.ToDictionary(c => c.Id, c => _combat.FoodForecast(c.Id, c.Food, Economy));
        foreach (City city in living)
        {
            BattleFoodForecast forecast = food[city.Id];
            city.Food -= forecast.Paid;
            city.LastUpkeep = new(Wave, forecast.Paid, forecast.Participating.ToArray(), forecast.Unfed.ToArray()) { Funded = forecast.Funded.ToArray() };
            city.DefenderCooldown = 0;
            city.Defender = city.Defender with { TargetId = 0, PendingImpact = false, ReadyTick = Math.Max(Tick, city.Defender.ReadyTick) };
            foreach (TowerState tower in city.Towers.Values.ToArray()) city.Towers[tower.Slot] = tower with { TargetId = 0, PendingImpact = false, ReadyTick = Tick };
            foreach (SpawnEntry entry in Campaign.WaveRule(Wave).Entries)
                for (int n = 0; n < entry.Count; n++) Spawn(city.Id, city.Id, entry);
        }
        int assigned = 0;
        foreach (int dead in _roster.Where(id => Players[id].Eliminated))
            foreach (SpawnEntry entry in Campaign.WaveRule(Wave).Entries)
                for (int n = 0; n < entry.Count; n++) Spawn(dead, living[assigned++ % living.Length].Id, entry);
        _combat.BeginWave(Wave, food.Values.SelectMany(f => f.Unfed));
        UpdateEngagements([]);
    }
    private void Spawn(int origin, int destination, SpawnEntry entry) => _combat.Create(entry.Type, 0, origin, destination, Faction.Skeletons, entry.Rank, entry.IsBoss, entry.Level);

    public void Step()
    {
        if (Paused || Phase != Phase.Combat && !_combat.HasDeaths) return;
        Tick = checked(Tick + 1); Revision++;
        if (Phase != Phase.Combat) { _combat.Cleanup(Tick); if (!_combat.HasDeaths) RestoreHomes(); ResolveReady(); return; }
        _combat.Advance(Tick, Players.Values);
        foreach (City city in Players.Values.Where(c => c.Eliminated))
        {
            city.Ready = false; city.Defender = city.Defender with { PendingImpact = false };
            foreach (TowerState tower in city.Towers.Values.ToArray()) city.Towers[tower.Slot] = tower with { PendingImpact = false };
            _combat.EliminateArmy(city.Id);
        }
        City[] survivors = Players.Values.Where(p => !p.Eliminated).ToArray();
        if (survivors.Length == 0) { DefeatReason = DefeatReason.AllCitiesFallen; Phase = Phase.Defeat; _combat.StopActions(Players.Values); return; }
        CombatUnit[] enemies = _combat.EnemyMembership();
        int[] cleared = survivors.Where(c => !enemies.Any(e => e.Destination == c.Id)).Select(c => c.Id).ToArray();
        int next = 0;
        foreach (CombatUnit enemy in enemies.Where(e => Players[e.Destination].Eliminated))
            _combat.Transfer(enemy.Id, survivors[next++ % survivors.Length].Id);
        if (next != 0) enemies = _combat.EnemyMembership();
        foreach (int city in cleared) _combat.TrackClearedAdmission(city);
        _combat.AdmitEntries();
        if (enemies.Length != 0)
        {
            UpdateEngagements(_combat.HealthProgressCities, enemies);
            EngagementProgress? expired = _engagements.Values.FirstOrDefault(e => Tick >= e.Deadline);
            if (expired is not null) Stall = new(BattleLimit.NoHealthProgress, Tick, expired.City, expired.LastHealthProgressTick);
            else if (Tick - WaveStartedTick >= Configuration.MaximumWaveTicks) Stall = new(BattleLimit.WaveDuration, Tick);
            if (Stall is not null) { DefeatReason = DefeatReason.BattleStalled; Phase = Phase.Defeat; _combat.StopActions(Players.Values); return; }
            _combat.StartActions(Players.Values); return;
        }
        _engagements.Clear(); _combat.StopActions(Players.Values);
        if (LastRewardedWave < Wave)
        {
            WaveDefinition completed = Campaign.WaveRule(Wave);
            var awards = new Dictionary<int, ResourceCost>();
            foreach (City city in survivors)
            {
                if (!city.Resources.TryAdd(completed.Reward, out ResourceCost balance)) throw new InvalidOperationException("Previously validated clear reward exceeds resource bounds.");
                _ = city.Research.Income(points: 1); awards.Add(city.Id, balance);
            }
            foreach (City city in survivors)
            { city.Resources = awards[city.Id]; city.Research = city.Research.Income(points: 1); city.LastReward = new(Wave, completed.IsBoss, completed.Reward) { Research = 1 }; }
            _combat.AuthorizeRecovery(survivors.SelectMany(c => c.LastUpkeep!.Funded));
            LastRewardedWave = Wave;
        }
        if (!_combat.HasDeaths) RestoreHomes();
        if (Wave == Campaign.TotalWaves) Phase = Phase.Victory;
        else { Wave++; Turn = 1; TurnSerial++; Phase = Phase.Building; }
    }
    private void UpdateEngagements(int[] healthProgress, CombatUnit[]? enemies = null)
    {
        int[] active = (enemies ?? _combat.EnemyMembership()).Select(e => e.Destination).Distinct().Order().ToArray();
        foreach (int city in _engagements.Keys.Except(active).ToArray()) _engagements.Remove(city);
        foreach (int city in active)
        {
            if (!_engagements.TryGetValue(city, out EngagementProgress? progress)) progress = new(city, Tick, Tick, checked(Tick + Configuration.NoHealthProgressTicks));
            if (healthProgress.Contains(city)) progress = progress with { LastHealthProgressTick = Tick, Deadline = checked(Tick + Configuration.NoHealthProgressTicks) };
            _engagements[city] = progress;
        }
    }
    public void Dispose() { _combat.Dispose(); Configuration.ClearCache(); Economy.ClearCache(); GC.SuppressFinalize(this); }
}

// Lifetime is the stable player, never the transient ENet peer.
public sealed class CommandLedger
{
    private long _highWater;
    private readonly SortedDictionary<long, CommandResult> _recent = [];
    public CommandResult Execute(Command command, Func<CommandResult> apply)
    {
        if (command.Sequence <= 0) return new(command.Sequence, false, "Sequence must be positive.");
        if (command.Sequence <= _highWater) return _recent.GetValueOrDefault(command.Sequence) ?? new(command.Sequence, false, "Already processed; use current state.");
        CommandResult result = apply(); _highWater = command.Sequence; _recent.Add(command.Sequence, result);
        if (_recent.Count > 128) _recent.Remove(_recent.Keys.First());
        return result;
    }
}
