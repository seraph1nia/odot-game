namespace Game;

// Engine scheduling only: the authority still executes every ordinary fixed step.
internal sealed class VerificationPacing
{
    public bool Owned { get; }
    public int Speed { get; private set; } = 1;
    public VerificationPacing(string? ownedData, string? marker, string? token)
    {
        if (ownedData is null || marker is null || string.IsNullOrEmpty(token)) return;
        try
        {
            Owned = Path.GetDirectoryName(Path.GetFullPath(marker)) == Path.GetFullPath(ownedData)
                && File.Exists(marker) && File.ReadAllText(marker) == token;
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
    public bool Configure(int speed, bool authority)
    {
        if (speed is < 1 or > 8) throw new ArgumentOutOfRangeException(nameof(speed), "Simulation speed must be 1..8.");
        if (!Owned || !authority) return false;
        Speed = speed; return true;
    }
    public void Run(Action step, Func<bool> paused)
    {
        for (int n = 0; n < Speed && !paused(); n++) step();
    }
}
