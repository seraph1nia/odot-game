using System.Diagnostics;

namespace Game.Core.Tests.Profiling;

internal static class OwnedProfileProcess
{
    public static async Task Run(ProcessStartInfo start, TimeSpan bound, CancellationToken cancellation)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        TimeSpan grace = TimeSpan.FromMilliseconds(500);
        deadline.CancelAfter(bound > grace ? bound - grace : bound);
        start.RedirectStandardInput = true;
        using var child = new Process { StartInfo = start };
        if (!child.Start()) throw new InvalidOperationException("Profile worker failed to start.");
        try
        {
            await child.WaitForExitAsync(deadline.Token);
            if (child.ExitCode != 0) throw new InvalidOperationException($"Profile worker exited with code {child.ExitCode}.");
        }
        catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
        { throw new TimeoutException($"Profile worker exceeded {bound.TotalSeconds}s; this run is incomplete."); }
        finally
        {
            if (!child.HasExited)
            {
                try { await child.StandardInput.WriteLineAsync("cancel"); await child.StandardInput.FlushAsync(CancellationToken.None); }
                catch (IOException) { }
                if (bound > grace) await Task.WhenAny(child.WaitForExitAsync(CancellationToken.None), Task.Delay(grace, CancellationToken.None));
            }
            if (!child.HasExited)
                try { child.Kill(entireProcessTree: true); }
                catch (InvalidOperationException) when (child.HasExited) { }
            await child.WaitForExitAsync(CancellationToken.None);
        }
    }
}
