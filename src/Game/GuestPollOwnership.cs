namespace Game;

// Exact lease guards, independent of Godot; no scheduling or transport policy.
internal sealed class GuestPollOwnership(long generation, int thread)
{
    internal bool Active { get; private set; } = true;
    internal bool Entered { get; private set; }
    internal void Enter(long currentGeneration, int currentThread)
    {
        if (!Active || generation != currentGeneration || thread != currentThread || Entered)
            throw new InvalidOperationException("Stale, foreign-thread, released or reentrant guest polling owner.");
        Entered = true;
    }
    internal void Exit() { if (!Entered) throw new InvalidOperationException("Guest polling owner was not entered."); Entered = false; }
    internal void Release() => Active = false;
}
