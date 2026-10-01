namespace DevRunner;

internal static class CameraPanExpectation
{
    internal static (double X, double Z) Calculate(CameraObservation before, double elapsed, double worldFraction, string[] keys)
    {
        var directions = keys.Select(key => key switch
        {
            "D" or "Right" => (X: 1, Y: 0),
            "A" or "Left" => (X: -1, Y: 0),
            "W" or "Up" => (X: 0, Y: -1),
            "S" or "Down" => (X: 0, Y: 1),
            _ => throw new ArgumentException("Unknown pan key: " + key, nameof(keys))
        }).Distinct().ToArray();
        double x = directions.Sum(d => d.X), y = directions.Sum(d => d.Y), length = Math.Sqrt(x * x + y * y);
        if (length == 0) return (before.PanX, before.PanZ);
        double distance = before.Size * worldFraction * .5 * elapsed / length;
        double yaw = before.Rotation[1], cosine = Math.Cos(yaw), sine = Math.Sin(yaw);
        return (Math.Clamp(before.PanX + (cosine * x + sine * y) * distance, -6, 6),
            Math.Clamp(before.PanZ + (-sine * x + cosine * y) * distance, -8, 8));
    }
}
