namespace DevRunner;

internal sealed record Options(string Command, string Host, string Bind, int? Port, int StartupTimeout, int Timeout, string[] EngineArgs)
{
    public static Options Parse(string[] args)
    {
        string command = args.FirstOrDefault() ?? "help";
        string host = "127.0.0.1";
        string bind = "127.0.0.1";
        int? port = null;
        int startup = 15000;
        int timeout = 60000;
        var engineArgs = new List<string>();
        for (int i = 1; i < args.Length; i++)
        {
            string Value() => ++i < args.Length ? args[i] : throw new ArgumentException($"Missing value for {args[i - 1]}.");
            switch (args[i])
            {
                case "--host": host = Value(); break;
                case "--bind": bind = Value(); break;
                case "--port": port = int.Parse(Value()); break;
                case "--startup-timeout-ms": startup = int.Parse(Value()); break;
                case "--timeout-ms": timeout = int.Parse(Value()); break;
                case "--engine-arg": engineArgs.Add(Value()); break;
                case "--help": return new("help", host, bind, port, startup, timeout, []);
                default: throw new ArgumentException($"Unknown runner argument: {args[i]}");
            }
        }
        if (port is < 1 or > 65535 || startup <= 0 || timeout <= 0)
            throw new ArgumentException("Port must be 1..65535 and deadlines must be positive.");
        return new(command, host, bind, port, startup, timeout, engineArgs.ToArray());
    }
}
