using Godot;

namespace Game;

internal sealed partial class EnetGuestPumpDriver : Node
{
    internal EnetGuestPump Pump = null!;
    internal Func<long> Generation = null!;
    internal Action<Exception> Failure = null!;
    public override void _PhysicsProcess(double delta)
    {
        try { Pump.Service(Generation(), Engine.GetPhysicsFrames()); }
        catch (Exception error) { Pump.Release(); SetPhysicsProcess(false); Failure(error); }
    }
    public override void _ExitTree() => Pump.Release();
}
