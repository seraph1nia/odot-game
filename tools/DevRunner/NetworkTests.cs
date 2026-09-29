using System.Net;
using System.Net.Sockets;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    private async Task NetworkTests()
    {
        using var suite = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        suite.CancelAfter(options.Timeout);
        CancellationToken token = suite.Token;
        try
        {
            int port = options.Port ?? FreePort();
            await using (var server = StartGame("test-server", true, true, port, null, "--seed", "42"))
            {
                await server.WaitFor(e => e.Type == "ready", "server readiness", options.StartupTimeout, token);
                await using var a = StartGame("test-a", false, true, port, null, "--automated");
                GameEvent connected = await a.WaitFor(e => e.Type == "connected", "client A connection", options.StartupTimeout, token);
                int aId = connected.PeerId;
                Point initial = State(connected).Players.Single(p => p.Id == aId).Position;
                await a.Send("collect");
                WorldSnapshot first = State(await a.WaitFor(e => e.Type == "bot-stopped", "first pickup", options.Timeout, token));
                Require(first.CoinGeneration == 2 && first.Players.Single(p => p.Id == aId).Score == 1, "first pickup awards exactly one point");
                Require(first.Players.Single(p => p.Id == aId).Position != initial, "movement reaches the server-owned coin");

                await using var b = StartGame("test-b", false, true, port, null, "--automated");
                GameEvent joined = await b.WaitFor(e => e.Type == "connected", "late client B connection", options.StartupTimeout, token);
                int bId = joined.PeerId;
                WorldSnapshot late = State(joined);
                Require(aId != bId && late.Players.Length == 2 && late.Players.Single(p => p.Id == aId).Score == 1
                    && late.CoinGeneration == first.CoinGeneration && late.Coin == first.Coin,
                    "late join receives the existing world, coin and score");
                Point bStart = late.Players.Single(p => p.Id == bId).Position;
                Point aStart = late.Players.Single(p => p.Id == aId).Position;
                await a.Send("move 1000 1000");
                WorldSnapshot moved = State(await a.WaitFor(e => HasState(e) && e.State!.Tick > late.Tick + 15
                    && (e.State.Players.Single(p => p.Id == aId).Position - aStart).Length > 10,
                    "bounded client A movement", options.Timeout, token));
                await a.Send("stop");
                WorldSnapshot remote = State(await b.WaitFor(e => HasState(e) && e.State!.Tick == moved.Tick,
                    "same movement snapshot at client B", options.StartupTimeout, token));
                Require(moved.Players.SequenceEqual(remote.Players), "both clients observe the same authoritative positions");
                Require(remote.Players.Single(p => p.Id == bId).Position == bStart, "client A movement cannot move client B");
                Require((moved.Players.Single(p => p.Id == aId).Position - aStart).Length
                    <= (moved.Tick - late.Tick) * World.Speed * World.StepSeconds + 0.1,
                    "oversized input cannot exceed the server movement speed");
                await a.Send("invalid");
                await a.Send("collect");
                WorldSnapshot second = State(await a.WaitFor(e => e.Type == "bot-stopped" && e.State!.CoinGeneration >= 3,
                    "second pickup", options.Timeout, token));
                WorldSnapshot agreed = State(await b.WaitFor(e => HasState(e) && e.State!.CoinGeneration == second.CoinGeneration,
                    "shared coin and score after collection", options.StartupTimeout, token));
                Require(second.CoinGeneration == 3 && second.Players.Sum(p => p.Score) == 2
                    && agreed.Players.Sum(p => p.Score) == 2 && agreed.Coin == second.Coin,
                    "one additional coin generation awards exactly one point on both clients");

                await b.Send("quit");
                Require(await b.WaitExit(token) == 0, "client B exits cleanly");
                await a.WaitFor(e => HasState(e) && e.State!.Tick > second.Tick && e.State.Players.Length == 1
                    && e.State.Players[0].Id == aId, "disconnected player removal", options.StartupTimeout, token);
                Require(true, "disconnect removes the avatar and session score");
                await server.Send("quit");
                await a.WaitFor(e => e.Type == "server-disconnected", "server disconnect feedback", options.StartupTimeout, token);
                Require(await a.WaitExit(token) == 0, "server shutdown produces bounded client disconnect feedback");
            }
            await FailureCases(token);
            Console.WriteLine("Network verification passed.");
        }
        catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
        {
            throw new TimeoutException($"Network suite exceeded {options.Timeout} ms; children were cleaned up.");
        }
    }

    private async Task FailureCases(CancellationToken token)
    {
        // Keep a UDP socket bound so no actual Godot server can accidentally be reached.
        using var occupied = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        occupied.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        int port = ((IPEndPoint)occupied.LocalEndPoint!).Port;
        await using (var client = StartGame("unavailable-client", false, true, port, null,
            "--automated", "--connect-timeout-ms", "1000"))
        {
            await client.WaitFor(e => e.Type == "connection-failed", "unavailable server feedback", 5000, token);
            Require(await client.WaitExit(token) != 0, "unavailable server produces bounded failure");
        }
        await using (var server = StartGame("occupied-server", true, true, port))
        {
            await server.WaitFor(e => e.Type == "error", "occupied-port diagnostic", 5000, token);
            Require(await server.WaitExit(token) != 0 && occupied.IsBound, "port collision fails without killing its owner");
        }
        await using (var server = StartGame("readiness-timeout-server", true, true, FreePort()))
        {
            await server.WaitFor(e => e.Type == "ready", "failure-test server readiness", options.StartupTimeout, token);
            try
            {
                await server.WaitFor(e => e.Type == "nonexistent-event", "deliberately missing readiness", 200, token);
                throw new InvalidOperationException("A missing readiness event incorrectly passed.");
            }
            catch (TimeoutException)
            {
                Require(true, "missing readiness/state fails with a deadline and attributable logs");
            }
            await server.Send("quit");
            await server.WaitExit(token);
            try
            {
                await server.WaitFor(e => e.Type == "another-missing-event", "event after child exit", 1000, token);
                throw new InvalidOperationException("An exited child incorrectly satisfied an expectation.");
            }
            catch (InvalidOperationException e) when (e.Message.Contains("exited before", StringComparison.Ordinal))
            {
                Require(true, "child exit fails pending expectations rather than reporting success");
            }
        }
    }
}
