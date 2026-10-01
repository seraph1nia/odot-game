using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Odot.Verification;

// Executes the locked xUnit framework directly: no test-host communication
// socket, no reflection-based reimplementation of facts/theories/fixtures.
internal static class InProcessTests
{
    public static int Main()
    {
        using var sink = new Results();
        using var framework = new XunitTestFramework(sink);
        using ITestFrameworkExecutor executor = framework.GetExecutor(Assembly.GetExecutingAssembly().GetName());
        var options = new FrameworkOptions();
        executor.RunAll(sink, options, options);
        if (!sink.Finished.Wait(TimeSpan.FromMinutes(3)))
        { Console.Error.WriteLine("In-process xUnit exceeded its three-minute bound."); return 1; }
        return sink.Failed ? 1 : 0;
    }
    private sealed class FrameworkOptions : ITestFrameworkDiscoveryOptions, ITestFrameworkExecutionOptions
    {
        private readonly Dictionary<string, object?> _values = [];
        public T GetValue<T>(string name) => _values.TryGetValue(name, out object? value) ? (T)value! : default!;
        public void SetValue<T>(string name, T value) => _values[name] = value;
    }
    private sealed class Results : LongLivedMarshalByRefObject, IMessageSink, IDisposable
    {
        public ManualResetEventSlim Finished { get; } = new();
        public bool Failed { get; private set; }
        public bool OnMessage(IMessageSinkMessage message)
        {
            if (message is ITestFailed failed)
            {
                Failed = true;
                Console.Error.WriteLine("FAIL: " + failed.Test.DisplayName);
                Console.Error.WriteLine(string.Join(Environment.NewLine, failed.Messages));
                Console.Error.WriteLine(string.Join(Environment.NewLine, failed.StackTraces));
                Console.Error.WriteLine(failed.Output);
            }
            if (message is IErrorMessage error)
            { Failed = true; Console.Error.WriteLine(string.Join(Environment.NewLine, error.Messages)); }
            if (message is ITestPassed passed && !string.IsNullOrWhiteSpace(passed.Output)) Console.WriteLine(passed.Output);
            if (message is ITestAssemblyFinished finished)
            {
                Failed |= finished.TestsFailed > 0 || finished.TestsRun == 0;
                Console.WriteLine($"xUnit: {finished.TestsRun} total, {finished.TestsFailed} failed, {finished.TestsSkipped} skipped; {finished.ExecutionTime:F2}s.");
                Finished.Set();
            }
            return true;
        }
        public void Dispose() => Finished.Dispose();
    }
}
