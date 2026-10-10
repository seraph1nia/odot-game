namespace Game;

// The private-X11 ownership gate admits only an absent socket in this owner's
// runtime namespace, never an inherited compositor endpoint or desktop fallback.
internal static class OwnedDisplayIsolation
{
    internal static bool AbsentWaylandSocket(string? socket, string? clientRuntime, string? displayRuntime)
    {
        if (socket is null) return true; // Legacy private-X11 launch with no Wayland variable.
        if (clientRuntime is null || displayRuntime is null || !Path.IsPathFullyQualified(socket)) return false;
        string runtime = Path.GetFullPath(clientRuntime);
        string owner = Path.GetFullPath(displayRuntime).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return runtime.StartsWith(owner, StringComparison.Ordinal) && Directory.Exists(runtime)
            && Path.GetFullPath(socket) == Path.Combine(runtime, "no-wayland-fallback")
            && !File.Exists(socket) && !Directory.Exists(socket);
    }
}
