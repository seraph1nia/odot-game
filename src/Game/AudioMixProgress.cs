namespace Game;

// Infer the mix timestamp from a bounded clock sample. Comparing elapsed times
// alone misses mixer cycles when several occur between rendered frames.
internal sealed class AudioMixProgress
{
    private double? _lastMixUpper;
    public int Cycles { get; private set; }

    public void Observe(ulong beforeUsec, double secondsSinceMix, ulong afterUsec)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(afterUsec, beforeUsec);
        if (!double.IsFinite(secondsSinceMix) || secondsSinceMix < 0)
            throw new ArgumentOutOfRangeException(nameof(secondsSinceMix));
        double elapsedUsec = secondsSinceMix * 1_000_000;
        // One microsecond of rounding tolerance; sampling delays widen the interval.
        double lower = beforeUsec - elapsedUsec - 1;
        double upper = afterUsec - elapsedUsec + 1;
        if (_lastMixUpper is null) _lastMixUpper = upper;
        else if (lower > _lastMixUpper.Value)
        {
            Cycles++;
            _lastMixUpper = upper;
        }
        else _lastMixUpper = Math.Min(_lastMixUpper.Value, upper);
    }
}
