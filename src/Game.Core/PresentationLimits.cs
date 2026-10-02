namespace Game.Core;

// Pure, bounded presentation mappings shared by tests; exact resource amounts stay in snapshots/HUD.
public static class PresentationLimits
{
    public static string DefeatText(DefeatReason reason) => reason == DefeatReason.BattleStalled ? "DEFEAT • battle stalled" : "DEFEAT • all cities fell";
    public static double HealthFraction(int current, int maximum)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximum);
        return Math.Clamp(current / (double)maximum, 0, 1);
    }
    public const int SoundVoices = 8;
    public const int BattleEffects = 64;
    public static int StockpileCount(int amount) => amount switch { <= 0 => 0, < 20 => 1, < 50 => 3, _ => 6 };
}
