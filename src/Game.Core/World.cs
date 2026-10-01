namespace Game.Core;

public enum Phase { Lobby, Building, Preparation, Combat, Victory, Defeat }
public enum Building { Empty, Mine, Farm, Barracks, Lumbermill, ArcheryRange, Arcanum, Blacksmith, ArrowTower, CatapultTower }
public sealed record Rules
{
    public int CityHealth { get; init; } = 100;
    public int StartingGold { get; init; } = 60;
    public int BaseGold { get; init; } = 10;
    public int BuildCost { get; init; } = 20;
    public int UpgradeCost { get; init; } = 20;
    public int MineOutput { get; init; } = 5;
    public int MineOutputLevelTwo { get; init; } = 10;
    public int FarmOutputLevelTwo { get; init; } = 8;
    public int StartingWood { get; init; } = 30;
    public int WoodOutput { get; init; } = 5;
    public int WoodOutputLevelTwo { get; init; } = 10;
    public int FarmOutput { get; init; } = 5;
    public int RecruitCost { get; init; } = 5;
    public int SoldierHealth { get; init; } = 10;
    public int SoldierDamage { get; init; } = 4;
    public int DefenderDamage { get; init; } = 1;
    public int AttackTicks { get; init; } = 60;
    public int RangedHealth { get; init; } = 8;
    public int RangedDamage { get; init; } = 3;
    public int RangedRecruitCost { get; init; } = 5;
    public double RangedReach { get; init; } = 3;
    public int MeleeWindupTicks { get; init; } = 12;
    public int RangedWindupTicks { get; init; } = 18;
    public int BerserkerHealth { get; init; } = 8;
    public int BerserkerDamage { get; init; } = 6;
    public int MageHealth { get; init; } = 6;
    public int MageDamage { get; init; } = 2;
    public int WaveOne { get; init; } = 4;
    public int WaveTwo { get; init; } = 6;
    public int WaveThree { get; init; } = 8;
    public int Allocation(int wave) => wave switch { 1 => WaveOne, 2 => WaveTwo, 3 => WaveThree, _ => 0 };
}
public sealed record SlotState(Building Type, int Level);
public sealed record UnitState(int Id, int Health, double Position, int Cooldown, int Origin = 0, int Destination = 0)
{
    public UnitType Type { get; init; }
    public Faction Faction { get; init; }
    public UnitClass Class => Catalogs.Class(Type);
    public int Rank { get; init; }
    public int Owner { get; init; }
    public double Lateral { get; init; }
    public bool Deployed { get; init; } = true;
    public double MoveForward { get; init; }
    public double MoveLateral { get; init; }
    public double FacingForward { get; init; } = 1;
    public double FacingLateral { get; init; }
    public int TargetId { get; init; }
    public bool TargetCity { get; init; }
    public long AttackSequence { get; init; }
    public long ActionStartTick { get; init; }
    public long ImpactTick { get; init; }
    public long ReadyTick { get; init; }
    public bool PendingImpact { get; init; }
    public WeaponProfile Profile { get; init; }
}
public sealed record CityState(int Id, bool Connected, bool Ready, int Gold, int Food, int Health, SlotState[] Slots, UnitState[] Soldiers, int DefenderCooldown)
{
    public int Wood { get; init; }
    public ResearchRanks Research { get; init; }
    public TowerState[] Towers { get; init; } = [];
    public bool Eliminated => Health <= 0;
}
public sealed record MatchSnapshot(string MatchId, long Revision, long Tick, Phase Phase, bool Paused, int Wave, int Turn, int TurnSerial, Rules Rules, CityState[] Players, UnitState[] Enemies)
{
    public int ProductionCount { get; init; }
    public BuildingDefinition[] BuildingCatalog { get; init; } = [];
    public TowerDefinition[] TowerCatalog { get; init; } = [];
    public UnitDefinition[] UnitCatalog { get; init; } = [];
    public long EventSequence { get; init; }
    public long OldestEventSequence { get; init; } = 1;
    public CombatEvent[] CombatEvents { get; init; } = [];
}
public sealed record Command(long Sequence, string MatchId, Phase ExpectedPhase, int TurnSerial, string Action, int City = 0, int Slot = -1, Building Building = Building.Empty, UnitType SoldierType = UnitType.Swordsman, UnitClass ResearchClass = UnitClass.Melee);
public sealed record CommandResult(long Sequence, bool Accepted, string Message);
public sealed class City
{
    private readonly CombatSimulation _combat;
    internal City(int id, Rules rules, CombatSimulation combat)
    {
        Id = id; Gold = rules.StartingGold; Wood = rules.StartingWood; Health = HealthPoints.FromWhole(rules.CityHealth); _combat = combat;
    }
    public int Id { get; }
    public bool Connected { get; set; } = true;
    public bool Ready { get; set; }
    public int Gold { get; set; }
    public int Food { get; set; }
    public int Wood { get; set; }
    public ResearchRanks Research { get; set; }
    public Dictionary<int, TowerState> Towers { get; } = [];
    public int Health { get; set; }
    public bool Eliminated => Health <= 0;
    public SlotState[] Slots { get; } = Enumerable.Range(0, 9).Select(_ => new SlotState(Building.Empty, 0)).ToArray();
    public IReadOnlyList<UnitState> Soldiers => _combat.Soldiers(Id);
    public int DefenderCooldown { get; set; }
    public CityState Snapshot() => new(Id, Connected, Ready, Gold, Food, Health, Slots.ToArray(), _combat.Soldiers(Id), DefenderCooldown) { Wood = Wood, Research = Research, Towers = Towers.Values.OrderBy(t => t.Slot).ToArray() };
}

public sealed class Match : IDisposable
{
    private readonly CombatSimulation _combat;
    internal CombatSimulation Combat => _combat;
    public Match(Rules? rules = null, string? matchId = null)
    {
        Rules = rules ?? new(); Id = matchId ?? Guid.NewGuid().ToString("N");
        if (Rules.AttackTicks <= Rules.MeleeWindupTicks || Rules.AttackTicks <= Rules.RangedWindupTicks
            || Rules.MeleeWindupTicks < 1 || Rules.RangedWindupTicks < 1 || Rules.SoldierHealth < 1 || Rules.RangedHealth < 1
            || Rules.BerserkerHealth < 1 || Rules.MageHealth < 1 || Rules.CityHealth < 1 || Rules.SoldierDamage < 0 || Rules.RangedDamage < 0 || Rules.BerserkerDamage < 0 || Rules.MageDamage < 0
            || Rules.DefenderDamage < 0 || Rules.RecruitCost < 2 || Rules.RangedRecruitCost < 2 || !double.IsFinite(Rules.RangedReach) || Rules.RangedReach < Reach)
            throw new ArgumentException("Invalid combat profile or timing.", nameof(rules));
        if (Rules.StartingGold < 0 || Rules.StartingWood < 0 || Rules.BaseGold < 0 || Rules.BuildCost < 0 || Rules.UpgradeCost < 0
            || Rules.MineOutput < 0 || Rules.MineOutputLevelTwo < 0 || Rules.FarmOutput < 0 || Rules.FarmOutputLevelTwo < 0
            || Rules.WoodOutput < 0 || Rules.WoodOutputLevelTwo < 0 || Rules.WaveOne < 1 || Rules.WaveTwo < 1 || Rules.WaveThree < 1)
            throw new ArgumentException("Invalid economy or allocation.", nameof(rules));
        _ = HealthPoints.FromWhole(Rules.CityHealth);
        _ = HealthPoints.FromWhole(Rules.DefenderDamage);
        _ = Catalogs.Units(Rules);
        _combat = new(Rules);
    }
    public const int StepsPerSecond = 60;
    public const double LaneLength = 12;
    public const double Reach = 0.55;
    public Rules Rules { get; }
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

    public City? Join()
    {
        if (Phase != Phase.Lobby || Players.Count >= 4) return null;
        var city = new City(_nextPlayer++, Rules, _combat);
        Players.Add(city.Id, city); Revision++;
        return city;
    }
    public void SetConnected(int id, bool connected)
    {
        if (!Players.TryGetValue(id, out City? city) || city.Connected == connected) return;
        city.Connected = connected; city.Ready = false; Revision++;
        ResolveReady();
    }
    public MatchSnapshot Snapshot() => new(Id, Revision, Tick, Phase, Paused, Wave, Turn, TurnSerial, Rules, Players.Values.Select(p => p.Snapshot()).ToArray(), _combat.Enemies())
    { ProductionCount = ProductionCount, BuildingCatalog = Catalogs.Buildings(Rules), TowerCatalog = Catalogs.Towers(), UnitCatalog = Catalogs.Units(Rules), EventSequence = _combat.EventSequence, OldestEventSequence = _combat.OldestEventSequence, CombatEvents = _combat.Events() };

    public CommandResult Apply(int sender, Command command)
    {
        CommandResult Reject(string message) => new(command.Sequence, false, message);
        if (!Players.TryGetValue(sender, out City? city) || !city.Connected) return Reject("Unauthenticated player.");
        if (command.MatchId != Id || command.ExpectedPhase != Phase || command.TurnSerial != TurnSerial) return Reject("Stale match, phase or turn.");
        if (command.City != 0 && command.City != sender) return Reject("You do not own this city.");
        if (Phase is Phase.Victory or Phase.Defeat) return Reject("Match finished. Restart the server for a new match.");
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
            city.Ready = command.Action == "ready"; Revision++; ResolveReady();
            return new(command.Sequence, true, city.Ready ? "Ready." : "Readiness updated.");
        }
        if (city.Ready) return Reject("Unready before editing.");
        if (command.Slot is < 0 or >= 9) return Reject("Choose a slot from 0 to 8.");
        SlotState slot = city.Slots[command.Slot];
        BuildingDefinition? definition = Catalogs.Buildings(Rules).FirstOrDefault(b => b.Type == slot.Type);
        void Pay(ResourceCost cost) { city.Gold -= cost.Gold; city.Wood -= cost.Wood; city.Food -= cost.Food; }
        bool CanPay(ResourceCost cost) => cost.CanPay(city.Gold, city.Wood, city.Food);
        switch (command.Action)
        {
            case "build":
                BuildingDefinition? construction = Catalogs.Buildings(Rules).FirstOrDefault(b => b.Type == command.Building);
                if (construction is null) return Reject("Unknown building.");
                if (slot.Type != Building.Empty) return Reject("Slot occupied.");
                if (!CanPay(construction.Construction)) return Reject("Not enough resources.");
                Pay(construction.Construction); city.Slots[command.Slot] = new(command.Building, 1);
                if (command.Building is Building.ArrowTower or Building.CatapultTower)
                    city.Towers[command.Slot] = new(city.Id, command.Slot, command.Building, 1);
                break;
            case "upgrade":
                if (definition is null || slot.Level != 1) return Reject("Select a level-one building.");
                if (!CanPay(definition.Upgrade)) return Reject("Not enough resources.");
                Pay(definition.Upgrade); city.Slots[command.Slot] = slot with { Level = 2 };
                if (city.Towers.TryGetValue(command.Slot, out TowerState? tower)) city.Towers[command.Slot] = tower with { Level = 2 };
                break;
            case "recruit":
                if (definition?.Recruits?.Contains(command.SoldierType) != true) return Reject("This building cannot recruit that role.");
                UnitDefinition recruit = Catalogs.Units(Rules).Single(u => u.Type == command.SoldierType);
                ResourceCost cost = recruit.Recruitment with { Food = Math.Max(1, recruit.Recruitment.Food - (slot.Level - 1)) };
                if (!CanPay(cost)) return Reject("Not enough resources.");
                Pay(cost); _combat.Create(command.SoldierType, city.Id, city.Id, city.Id, rank: city.Research.For(recruit.Class)); _combat.AdmitEntries();
                break;
            case "research":
                if (slot.Type != Building.Blacksmith || !Enum.IsDefined(command.ResearchClass)) return Reject("Select your Blacksmith and a valid class.");
                int rank = city.Research.For(command.ResearchClass);
                if (rank >= 2 || rank >= slot.Level) return Reject("Upgrade the Blacksmith or choose an uncompleted class.");
                ResourceCost researchCost = new(rank == 0 ? 10 : 15);
                if (!CanPay(researchCost)) return Reject("Not enough resources.");
                Pay(researchCost); city.Research = city.Research.Increase(command.ResearchClass);
                _combat.Research(city.Id, command.ResearchClass, rank + 1);
                break;
            default: return Reject("Unknown action.");
        }
        Revision++;
        return new(command.Sequence, true, $"{command.Action} accepted.");
    }
    private void ResolveReady()
    {
        if (Paused || Phase is not (Phase.Building or Phase.Preparation)) return;
        City[] required = Players.Values.Where(p => p.Connected && !p.Eliminated).ToArray();
        if (required.Length == 0 || required.Any(p => !p.Ready)) return;
        foreach (City city in Players.Values) city.Ready = false;
        TurnSerial++;
        if (Phase == Phase.Preparation) { BeginWave(); Revision++; return; }
        foreach (City city in Players.Values.Where(p => !p.Eliminated))
        {
            int Output(Building type) => city.Slots.Where(s => s.Type == type).Sum(s => Catalogs.Buildings(Rules).Single(b => b.Type == type).Output(s.Level));
            city.Gold += Rules.BaseGold + Output(Building.Mine);
            city.Food += Output(Building.Farm);
            city.Wood += Output(Building.Lumbermill);
        }
        ProductionCount++;
        if (Turn == 3) Phase = Phase.Preparation;
        else Turn++;
        Revision++;
    }
    private void BeginWave()
    {
        Phase = Phase.Combat;
        City[] living = Players.Values.Where(p => !p.Eliminated).ToArray();
        foreach (City city in living)
        {
            city.DefenderCooldown = 0;
            foreach (TowerState tower in city.Towers.Values.ToArray()) city.Towers[tower.Slot] = tower with { TargetId = 0, PendingImpact = false, ReadyTick = Tick };
            for (int n = 0; n < Rules.Allocation(Wave); n++) Spawn(city.Id, city.Id, n);
        }
        int assigned = 0;
        foreach (int dead in _roster.Where(id => Players[id].Eliminated))
            for (int n = 0; n < Rules.Allocation(Wave); n++) Spawn(dead, living[assigned++ % living.Length].Id, n);
        _combat.BeginWave();
    }
    private void Spawn(int origin, int destination, int index) => _combat.Create(Catalogs.EnemyRole(Wave, index), 0, origin, destination, Faction.Skeletons);

    public void Step()
    {
        if (Paused || Phase != Phase.Combat) return;
        Tick++; Revision++;
        _combat.Step(Tick, Players.Values);
        foreach (City city in Players.Values.Where(c => c.Eliminated))
        { city.Ready = false; _combat.EliminateArmy(city.Id); }
        City[] survivors = Players.Values.Where(p => !p.Eliminated).ToArray();
        if (survivors.Length == 0) { Phase = Phase.Defeat; return; }
        int next = 0;
        foreach (UnitState enemy in Enemies.Where(e => Players[e.Destination].Eliminated).OrderBy(e => e.Id))
            _combat.Transfer(enemy.Id, survivors[next++ % survivors.Length].Id);
        if (Enemies.Count != 0) return;
        if (Wave == 3) Phase = Phase.Victory;
        else { Wave++; Turn = 1; TurnSerial++; Phase = Phase.Building; }
    }
    public void Dispose() { _combat.Dispose(); GC.SuppressFinalize(this); }
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
