namespace Game.Core;

public enum Phase { Lobby, Building, Combat, Victory, Defeat }
public enum Building { Empty, Mine, Farm, Barracks }
public sealed record Rules
{
    public int CityHealth { get; init; } = 100;
    public int StartingGold { get; init; } = 60;
    public int BaseGold { get; init; } = 10;
    public int BuildCost { get; init; } = 20;
    public int UpgradeCost { get; init; } = 20;
    public int MineOutput { get; init; } = 3;
    public int FarmOutput { get; init; } = 5;
    public int RecruitCost { get; init; } = 5;
    public int SoldierHealth { get; init; } = 10;
    public int SoldierDamage { get; init; } = 4;
    public int EnemyHealth { get; init; } = 10;
    public int EnemyDamage { get; init; } = 3;
    public int DefenderDamage { get; init; } = 1;
    public int AttackTicks { get; init; } = 60;
    public int RangedHealth { get; init; } = 8;
    public int RangedDamage { get; init; } = 3;
    public int RangedRecruitCost { get; init; } = 5;
    public double RangedReach { get; init; } = 3;
    public int MeleeWindupTicks { get; init; } = 12;
    public int RangedWindupTicks { get; init; } = 18;
    public int WaveOne { get; init; } = 4;
    public int WaveTwo { get; init; } = 6;
    public int WaveThree { get; init; } = 8;
    public int Allocation(int wave) => wave switch { 1 => WaveOne, 2 => WaveTwo, 3 => WaveThree, _ => 0 };
}
public sealed record SlotState(Building Type, int Level);
public sealed record UnitState(int Id, int Health, double Position, int Cooldown, int Origin = 0, int Destination = 0)
{
    public UnitType Type { get; init; }
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
    public bool Eliminated => Health <= 0;
}
public sealed record MatchSnapshot(string MatchId, long Revision, long Tick, Phase Phase, bool Paused, int Wave, int Turn, int TurnSerial, Rules Rules, CityState[] Players, UnitState[] Enemies)
{
    public long EventSequence { get; init; }
    public long OldestEventSequence { get; init; } = 1;
    public CombatEvent[] CombatEvents { get; init; } = [];
}
public sealed record Command(long Sequence, string MatchId, Phase ExpectedPhase, int TurnSerial, string Action, int City = 0, int Slot = -1, Building Building = Building.Empty, UnitType SoldierType = UnitType.Swordsman);
public sealed record CommandResult(long Sequence, bool Accepted, string Message);
public sealed class City
{
    private readonly CombatSimulation _combat;
    internal City(int id, Rules rules, CombatSimulation combat)
    {
        Id = id; Gold = rules.StartingGold; Health = rules.CityHealth; _combat = combat;
    }
    public int Id { get; }
    public bool Connected { get; set; } = true;
    public bool Ready { get; set; }
    public int Gold { get; set; }
    public int Food { get; set; }
    public int Health { get; set; }
    public bool Eliminated => Health <= 0;
    public SlotState[] Slots { get; } = Enumerable.Range(0, 9).Select(_ => new SlotState(Building.Empty, 0)).ToArray();
    public IReadOnlyList<UnitState> Soldiers => _combat.Soldiers(Id);
    public int DefenderCooldown { get; set; }
    public CityState Snapshot() => new(Id, Connected, Ready, Gold, Food, Health, Slots.ToArray(), _combat.Soldiers(Id), DefenderCooldown);
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
            || Rules.EnemyHealth < 1 || Rules.CityHealth < 1 || Rules.SoldierDamage < 0 || Rules.RangedDamage < 0 || Rules.EnemyDamage < 0
            || Rules.DefenderDamage < 0 || Rules.RecruitCost < 2 || Rules.RangedRecruitCost < 2 || !double.IsFinite(Rules.RangedReach) || Rules.RangedReach < Reach)
            throw new ArgumentException("Invalid combat profile or timing.", nameof(rules));
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
    { EventSequence = _combat.EventSequence, OldestEventSequence = _combat.OldestEventSequence, CombatEvents = _combat.Events() };

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
        if (Phase != Phase.Building || city.Eliminated) return Reject("Only living cities can act during building.");
        if (command.Action is "ready" or "unready")
        {
            city.Ready = command.Action == "ready"; Revision++; ResolveReady();
            return new(command.Sequence, true, city.Ready ? "Ready." : "Readiness updated.");
        }
        if (city.Ready) return Reject("Unready before editing.");
        if (command.Slot is < 0 or >= 9) return Reject("Choose a slot from 0 to 8.");
        SlotState slot = city.Slots[command.Slot];
        switch (command.Action)
        {
            case "build":
                if (command.Building is not (Building.Mine or Building.Farm or Building.Barracks)) return Reject("Unknown building.");
                if (slot.Type != Building.Empty) return Reject("Slot occupied.");
                if (city.Gold < Rules.BuildCost) return Reject("Not enough gold.");
                city.Gold -= Rules.BuildCost; city.Slots[command.Slot] = new(command.Building, 1);
                break;
            case "upgrade":
                if (slot.Type == Building.Empty || slot.Level != 1) return Reject("Select a level-one building.");
                if (city.Gold < Rules.UpgradeCost) return Reject("Not enough gold.");
                city.Gold -= Rules.UpgradeCost; city.Slots[command.Slot] = slot with { Level = 2 };
                break;
            case "recruit":
                if (slot.Type != Building.Barracks) return Reject("Select your barracks.");
                if (command.SoldierType is not (UnitType.Swordsman or UnitType.Crossbowman)) return Reject("Unknown soldier type.");
                int cost = _combat.Profile(command.SoldierType).FoodCost - (slot.Level - 1);
                if (city.Food < cost) return Reject("Not enough food.");
                city.Food -= cost; _combat.Create(command.SoldierType, city.Id, city.Id, city.Id); _combat.AdmitEntries();
                break;
            default: return Reject("Unknown action.");
        }
        Revision++;
        return new(command.Sequence, true, $"{command.Action} accepted.");
    }
    private void ResolveReady()
    {
        if (Paused || Phase != Phase.Building) return;
        City[] required = Players.Values.Where(p => p.Connected && !p.Eliminated).ToArray();
        if (required.Length == 0 || required.Any(p => !p.Ready)) return;
        foreach (City city in Players.Values.Where(p => !p.Eliminated))
        {
            city.Gold += Rules.BaseGold + city.Slots.Where(s => s.Type == Building.Mine).Sum(s => Rules.MineOutput * s.Level);
            city.Food += city.Slots.Where(s => s.Type == Building.Farm).Sum(s => Rules.FarmOutput * s.Level);
            city.Ready = false;
        }
        if (Turn == 3) BeginWave();
        else { Turn++; TurnSerial++; }
        Revision++;
    }
    private void BeginWave()
    {
        Phase = Phase.Combat;
        City[] living = Players.Values.Where(p => !p.Eliminated).ToArray();
        foreach (City city in living)
        {
            city.DefenderCooldown = 0;
            for (int n = 0; n < Rules.Allocation(Wave); n++) Spawn(city.Id, city.Id);
        }
        int assigned = 0;
        foreach (int dead in _roster.Where(id => Players[id].Eliminated))
            for (int n = 0; n < Rules.Allocation(Wave); n++) Spawn(dead, living[assigned++ % living.Length].Id);
        _combat.BeginWave();
    }
    private void Spawn(int origin, int destination) => _combat.Create(UnitType.Enemy, 0, origin, destination);

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
