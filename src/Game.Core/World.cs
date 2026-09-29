namespace Game.Core;

public readonly record struct Point(float X, float Y)
{
    public static Point Zero => new(0, 0);
    public bool IsFinite => float.IsFinite(X) && float.IsFinite(Y);
    public double Length => Math.Sqrt((double)X * X + (double)Y * Y);
    public Point Limited()
    {
        if (!IsFinite) return Zero;
        double length = Length;
        return length > 1 ? new((float)(X / length), (float)(Y / length)) : this;
    }
    public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);
    public static Point operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
    public static Point operator *(Point a, float scale) => new(a.X * scale, a.Y * scale);
}

public sealed record PlayerState(int Id, Point Position, int Score);
public sealed record WorldSnapshot(long Tick, int CoinGeneration, Point Coin, PlayerState[] Players);

/// <summary>Authoritative rules. Rendering, transport and wall-clock time live outside this class.</summary>
public sealed class World
{
    public const int Width = 800;
    public const int Height = 480;
    public const float PlayerRadius = 12;
    public const float CoinRadius = 9;
    public const float Speed = 180;
    public const float StepSeconds = 1f / 60;
    private readonly Random _random;
    private readonly SortedDictionary<int, PlayerState> _players = new();
    private readonly Dictionary<int, Point> _inputs = new();

    public long Tick { get; private set; }
    public int CoinGeneration { get; private set; } = 1;
    public Point Coin { get; private set; }
    public IReadOnlyDictionary<int, PlayerState> Players => _players;

    public World(int seed = 42, Point? initialCoin = null)
    {
        _random = new Random(seed);
        Coin = initialCoin is { IsFinite: true } p ? Clamp(p, CoinRadius) : SpawnCoin();
    }

    public void AddPlayer(int id, Point? position = null)
    {
        if (id <= 1 || _players.ContainsKey(id)) throw new ArgumentException("Player ID must be unique and greater than 1.", nameof(id));
        Point spawn = position ?? new Point(60 + (_players.Count % 8) * 40, 60 + (_players.Count / 8) * 40);
        if (!spawn.IsFinite) throw new ArgumentException("Spawn must be finite.", nameof(position));
        _players.Add(id, new(id, Clamp(spawn, PlayerRadius), 0));
        _inputs.Add(id, Point.Zero);
    }

    public void RemovePlayer(int id)
    {
        _players.Remove(id);
        _inputs.Remove(id);
    }

    public bool SetInput(int id, Point direction)
    {
        if (!_players.ContainsKey(id)) return false;
        _inputs[id] = direction.Limited();
        return direction.IsFinite;
    }

    public void Step()
    {
        foreach (int id in _players.Keys.ToArray())
        {
            PlayerState player = _players[id];
            _players[id] = player with { Position = Clamp(player.Position + _inputs[id] * (Speed * StepSeconds), PlayerRadius) };
        }
        // Select once against the original coin. Sorting makes simultaneous pickups stable.
        PlayerState? winner = _players.Values.FirstOrDefault(p => (p.Position - Coin).Length <= PlayerRadius + CoinRadius);
        if (winner is not null)
        {
            _players[winner.Id] = winner with { Score = winner.Score + 1 };
            CoinGeneration++;
            Coin = SpawnCoin();
        }
        Tick++;
    }

    public WorldSnapshot Snapshot() => new(Tick, CoinGeneration, Coin, _players.Values.ToArray());

    private static Point Clamp(Point p, float margin) => new(
        Math.Clamp(p.X, margin, Width - margin), Math.Clamp(p.Y, margin, Height - margin));

    private Point SpawnCoin()
    {
        for (int attempt = 0; attempt < 128; attempt++)
        {
            Point p = new(_random.Next(40, Width - 40), _random.Next(40, Height - 40));
            if (Available(p)) return p;
        }
        // A bounded fallback keeps dense or unusual test worlds reproducible.
        for (int y = 40; y < Height - 40; y += 40)
            for (int x = 40; x < Width - 40; x += 40)
                if (Available(new(x, y))) return new(x, y);
        throw new InvalidOperationException("No unoccupied coin spawn exists.");
    }

    private bool Available(Point p) => _players.Values.All(player => (player.Position - p).Length > PlayerRadius + CoinRadius + 6);
}
