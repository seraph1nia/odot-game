using System.Runtime.InteropServices;
using DevRunner;

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancellation.Cancel(); };
using var sigterm = !OperatingSystem.IsWindows()
    ? PosixSignalRegistration.Create(PosixSignal.SIGTERM, context => { context.Cancel = true; cancellation.Cancel(); })
    : null;
try
{
    if (args.FirstOrDefault() is "planning-validate" or "planning-next" or "planning-inputs")
        return PlanningCommand.Run(Environment.CurrentDirectory, args[0], args[1..], Console.Out);
    var options = Options.Parse(args);
    var runner = new Runner(options, cancellation.Token);
    await runner.Run();
    return 0;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Stopped; supervised children were cleaned up.");
    return 130;
}
catch (Exception error)
{
    Console.Error.WriteLine($"FAILED: {error.Message}");
    return 1;
}
