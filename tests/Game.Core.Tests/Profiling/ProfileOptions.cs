using System.Globalization;

namespace Game.Core.Tests.Profiling;

internal sealed record ProfileOptions(string Command, string Strategy = "frontline", int Players = 4, ulong Seed = 1,
    int Iterations = 3, string Configuration = "Release", string? Scenario = null, int[]? Sizes = null, bool WorkCounters = false)
{
    public static ProfileOptions Parse(string[] args)
    {
        if (args.Length == 0) throw new ArgumentException("Select a profiling command.");
        var options = new ProfileOptions(args[0]);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 1; i < args.Length; i++)
        {
            string name = args[i];
            if (!seen.Add(name)) throw new ArgumentException($"Repeated argument: {name}.");
            string Value() => ++i < args.Length ? args[i] : throw new ArgumentException($"Missing value for {name}.");
            options = name switch
            {
                "--strategy" when options.Command == "profile-campaign" => options with { Strategy = Value() },
                "--players" when options.Command == "profile-campaign" => options with { Players = int.Parse(Value(), CultureInfo.InvariantCulture) },
                "--seed" => options with { Seed = ulong.Parse(Value(), CultureInfo.InvariantCulture) },
                "--iterations" when options.Command != "test-scale" => options with { Iterations = int.Parse(Value(), CultureInfo.InvariantCulture) },
                "--configuration" => options with { Configuration = Value() },
                "--scenario" when options.Command != "profile-campaign" => options with { Scenario = Value() },
                "--sizes" when options.Command == "profile-scale" => options with { Sizes = Value().Split(',').Select(s => int.Parse(s, CultureInfo.InvariantCulture)).ToArray() },
                "--work-counters" => options with { WorkCounters = true },
                _ => throw new ArgumentException($"Unknown argument: {name}.")
            };
        }
        if (options.Command is not ("profile-campaign" or "profile-scale" or "test-scale" or "profile-snapshots")) throw new ArgumentException("Unknown profiling command.");
        if (options.Configuration is not ("Debug" or "Release") || options.Iterations is < 1 or > 10) throw new ArgumentException("Use Debug/Release and 1..10 measured iterations.");
        if (options.Strategy is not ("frontline" or "mixed" or "towers" or "research") || options.Players is < 1 or > 4 || options.Players != 1 && options.Strategy != "frontline")
            throw new ArgumentException("Select a solo strategy or frontline with 1..4 players.");
        if (options.Command is "profile-scale" or "test-scale" && options.Scenario != "large-battle" || options.Command == "profile-snapshots" && options.Scenario != "ordinary-and-large")
            throw new ArgumentException("Select the documented scenario explicitly.");
        if (options.Sizes is { } sizes && (sizes.Length == 0 || sizes.Distinct().Count() != sizes.Length || sizes.Any(s => s is not (128 or 512 or 2048))))
            throw new ArgumentException("Sizes must be distinct members of 128,512,2048.");
        return options;
    }
}
