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
    public int WaveOne { get; init; } = 4;
    public int WaveTwo { get; init; } = 6;
    public int WaveThree { get; init; } = 8;
    public int Allocation(int wave) => wave switch { 1 => WaveOne, 2 => WaveTwo, 3 => WaveThree, _ => 0 };
}
public sealed record SlotState(Building Type, int Level);
public sealed record UnitState(int Id, int Health, double Position, int Cooldown, int Origin = 0, int Destination = 0);
public sealed record CityState(int Id, bool Connected, bool Ready, int Gold, int Food, int Health, SlotState[] Slots, UnitState[] Soldiers, int DefenderCooldown)
{
    public bool Eliminated => Health <= 0;
}
public sealed record MatchSnapshot(string MatchId, long Revision, long Tick, Phase Phase, bool Paused, int Wave, int Turn, int TurnSerial, Rules Rules, CityState[] Players, UnitState[] Enemies);
public sealed record Command(long Sequence, string MatchId, Phase ExpectedPhase, int TurnSerial, string Action, int City = 0, int Slot = -1, Building Building = Building.Empty);
public sealed record CommandResult(long Sequence, bool Accepted, string Message);
public sealed class Unit(int id, int health, double position, int origin = 0, int destination = 0)
{
    public int Id { get; } = id;
    public int Health { get; set; } = health;
    public double Position { get; set; } = position;
    public int Cooldown { get; set; }
    public int Origin { get; } = origin;
    public int Destination { get; set; } = destination;
    public UnitState Snapshot() => new(Id, Health, Position, Cooldown, Origin, Destination);
}
public sealed class City(int id, Rules rules)
{
    public int Id { get; } = id;
    public bool Connected { get; set; } = true;
    public bool Ready { get; set; }
    public int Gold { get; set; } = rules.StartingGold;
    public int Food { get; set; }
    public int Health { get; set; } = rules.CityHealth;
    public bool Eliminated => Health <= 0;
    public SlotState[] Slots { get; } = Enumerable.Range(0, 9).Select(_ => new SlotState(Building.Empty, 0)).ToArray();
    public List<Unit> Soldiers { get; } = [];
    public int DefenderCooldown { get; set; }
    public CityState Snapshot() => new(Id, Connected, Ready, Gold, Food, Health, Slots.ToArray(), Soldiers.Select(s => s.Snapshot()).ToArray(), DefenderCooldown);
}

public sealed class Match(Rules? rules = null, string? matchId = null)
{
    public const int StepsPerSecond = 60;
    public const double LaneLength = 12;
    public const double Reach = 0.55;
    public Rules Rules { get; } = rules ?? new();
    public string Id { get; } = matchId ?? Guid.NewGuid().ToString("N");
    public SortedDictionary<int, City> Players { get; } = [];
    public List<Unit> Enemies { get; } = [];
    public Phase Phase { get; private set; } = Phase.Lobby;
    public bool Paused { get; private set; }
    public long Revision { get; private set; }
    public long Tick { get; private set; }
    public int Wave { get; private set; } = 1;
    public int Turn { get; private set; } = 1;
    public int TurnSerial { get; private set; } = 1;
    private int _nextPlayer = 1;
    private int _nextEntity = 1;
    private int[] _roster = [];

    public City? Join()
    {
        if (Phase != Phase.Lobby || Players.Count >= 4) return null;
        var city = new City(_nextPlayer++, Rules);
        Players.Add(city.Id, city); Revision++;
        return city;
    }
    public void SetConnected(int id, bool connected)
    {
        if (!Players.TryGetValue(id, out City? city) || city.Connected == connected) return;
        city.Connected = connected; city.Ready = false; Revision++;
        ResolveReady();
    }
    public MatchSnapshot Snapshot() => new(Id, Revision, Tick, Phase, Paused, Wave, Turn, TurnSerial, Rules, Players.Values.Select(p => p.Snapshot()).ToArray(), Enemies.OrderBy(e => e.Id).Select(e => e.Snapshot()).ToArray());

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
                int cost = Rules.RecruitCost - (slot.Level - 1);
                if (city.Food < cost) return Reject("Not enough food.");
                city.Food -= cost; city.Soldiers.Add(new(_nextEntity++, Rules.SoldierHealth, 0));
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
            foreach (Unit soldier in city.Soldiers) { soldier.Position = 0; soldier.Cooldown = 0; }
            for (int n = 0; n < Rules.Allocation(Wave); n++) Spawn(city.Id, city.Id);
        }
        int assigned = 0;
        foreach (int dead in _roster.Where(id => Players[id].Eliminated))
            for (int n = 0; n < Rules.Allocation(Wave); n++) Spawn(dead, living[assigned++ % living.Length].Id);
    }
    private void Spawn(int origin, int destination) => Enemies.Add(new(_nextEntity++, Rules.EnemyHealth, LaneLength, origin, destination));

    public void Step()
    {
        if (Paused || Phase != Phase.Combat) return;
        Tick++; Revision++;
        var damage = new Dictionary<Unit, int>();
        var cityDamage = new Dictionary<City, int>();
        void Hit(Unit target, int amount) => damage[target] = damage.GetValueOrDefault(target) + amount;
        foreach (City city in Players.Values.Where(p => !p.Eliminated))
        {
            Unit[] enemies = Enemies.Where(e => e.Destination == city.Id).OrderBy(e => e.Id).ToArray();
            Unit[] soldiers = city.Soldiers.OrderBy(s => s.Id).ToArray();
            foreach (Unit soldier in soldiers)
            {
                soldier.Cooldown = Math.Max(0, soldier.Cooldown - 1);
                Unit? target = enemies.OrderBy(e => Math.Abs(e.Position - soldier.Position)).ThenBy(e => e.Id).FirstOrDefault();
                if (target is null) continue;
                soldier.Position = Approach(soldier.Position, target.Position, 1.0 / StepsPerSecond);
                if (Math.Abs(soldier.Position - target.Position) <= Reach && soldier.Cooldown == 0)
                { Hit(target, Rules.SoldierDamage); soldier.Cooldown = Rules.AttackTicks; }
            }
            foreach (Unit enemy in enemies)
            {
                enemy.Cooldown = Math.Max(0, enemy.Cooldown - 1);
                Unit? target = soldiers.OrderBy(s => Math.Abs(s.Position - enemy.Position)).ThenBy(s => s.Id).FirstOrDefault();
                double destination = target?.Position ?? 0;
                enemy.Position = Approach(enemy.Position, destination, 0.8 / StepsPerSecond);
                if (Math.Abs(enemy.Position - destination) <= Reach && enemy.Cooldown == 0)
                {
                    if (target is null) cityDamage[city] = cityDamage.GetValueOrDefault(city) + Rules.EnemyDamage;
                    else Hit(target, Rules.EnemyDamage);
                    enemy.Cooldown = Rules.AttackTicks;
                }
            }
            city.DefenderCooldown = Math.Max(0, city.DefenderCooldown - 1);
            Unit? defended = enemies.OrderBy(e => e.Position).ThenBy(e => e.Id).FirstOrDefault();
            if (defended is not null && city.DefenderCooldown == 0)
            { Hit(defended, Rules.DefenderDamage); city.DefenderCooldown = Rules.AttackTicks; }
        }
        foreach ((Unit unit, int amount) in damage) unit.Health = Math.Max(0, unit.Health - amount);
        foreach ((City city, int amount) in cityDamage) city.Health = Math.Max(0, city.Health - amount);
        Enemies.RemoveAll(e => e.Health <= 0);
        foreach (City city in Players.Values)
        {
            city.Soldiers.RemoveAll(s => s.Health <= 0);
            if (city.Eliminated) { city.Ready = false; city.Soldiers.Clear(); }
        }
        City[] survivors = Players.Values.Where(p => !p.Eliminated).ToArray();
        if (survivors.Length == 0) { Phase = Phase.Defeat; return; }
        int next = 0;
        foreach (Unit enemy in Enemies.Where(e => Players[e.Destination].Eliminated).OrderBy(e => e.Id))
        { enemy.Destination = survivors[next++ % survivors.Length].Id; enemy.Position = LaneLength; }
        if (Enemies.Count != 0) return;
        if (Wave == 3) Phase = Phase.Victory;
        else { Wave++; Turn = 1; TurnSerial++; Phase = Phase.Building; }
    }
    private static double Approach(double position, double target, double speed) => Math.Clamp(position + Math.Clamp(target - position, -speed, speed), 0, LaneLength);
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
