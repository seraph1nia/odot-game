using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    private async Task<Child> TransportedResearch(Child a, Child b, Child server, int port, string sessionPath, CancellationToken token)
    {
        Console.WriteLine("Research transport risk: typed tower-independent purchases and retries, complete active captured status deadlines and paused current-state resume. Reuses authority-resume-victory peers and ordinary early-match investment; bounded at wave ten/120s, no extra scenario or full campaign.");
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token); deadline.CancelAfter(120000);
        token = deadline.Token;
        Command? purchase = null;
        while (true)
        {
            token.ThrowIfCancellationRequested();
            MatchSnapshot state = Latest(a);
            if (state.Phase is Phase.Victory or Phase.Defeat || state.Wave > 10) throw new InvalidOperationException("Research transport opening exceeded wave ten or ended without a status witness.");
            if (state.Phase == Phase.Combat)
            {
                MatchSnapshot settled = await Observe(a, s => s.Revision >= state.Revision && (s.Phase != Phase.Combat || s.Enemies.Any(u => u.Statuses.Burn is not null)), "shared clear or real burn", token);
                if (settled.Enemies.Any(u => u.Statuses.Burn is not null)) break;
                await Observe(b, s => s.Revision >= settled.Revision && s.Phase == settled.Phase, "ordinary research stage agreement", token); continue;
            }
            foreach (Child child in new[] { a, b })
            {
                for (int n = 0; n < 100; n++)
                {
                    EconomyAction? action = CampaignStrategy.ResearchWitness(Latest(child), child.PlayerId); if (action is null) break;
                    string command = action.Action switch { "build" => $"{(action.Payment == ConstructionPayment.GoldRecovery ? "build-recovery" : "build")} {action.Slot} {action.Building}", "recruit" => $"recruit {action.Slot} {action.Unit}", "trade" => $"trade {action.Slot} {action.Resource} {action.Bundles}", _ => $"{action.Action} {action.Slot}" };
                    await Action(child, command, token);
                }
            }
            foreach (TechnologyId id in new[] { TechnologyId.MagicFoundation, TechnologyId.Fire })
            {
                MatchSnapshot before = Latest(a); CityState city = before.Players.Single(p => p.Id == a.PlayerId);
                if (!city.Technologies.Single(t => t.Id == id).Available) continue;
                if (id == TechnologyId.Fire && !city.Soldiers.Any(u => u.Type == UnitType.Mage)) continue;
                GameEvent result = await Action(a, "research-tech " + TechnologyIds.Name(id), token);
                purchase = new(result.Result!.Sequence, before.MatchId, before.Phase, before.TurnSerial, "research-tech", a.PlayerId, Technology: id);
            }
            if (Latest(a).Players.Single(c => c.Id == a.PlayerId).Research.Has(TechnologyId.Fire)) await SimulationSpeed(server, 1, token);
            foreach (Child child in new[] { a, b }) await Action(child, "ready", token);
            MatchSnapshot barrier = Latest(b); await Observe(a, s => s.Revision >= barrier.Revision && s.TurnSerial >= barrier.TurnSerial, "research readiness barrier", token);
        }
        MatchSnapshot frozen = State(await Action(a, "pause", token));
        MatchSnapshot seen = await Observe(b, s => s.Paused && s.Tick == frozen.Tick, "same paused active status tick", token);
        Require(frozen.Enemies.Any(u => u.Statuses.Burn is not null), "paused authority retains active captured burn deadlines");
        Require(Gameplay(frozen) == Gameplay(seen), "peers agree on complete research and captured status deadlines");
        Require(frozen.Players.Single(p => p.Id == a.PlayerId).Research.Has(TechnologyId.Fire)
            && !frozen.Players.Single(p => p.Id == a.PlayerId).Technologies.Single(t => t.Id == TechnologyId.Frost).Available, "transported personal Fire choice and Frost lock");
        Require(purchase is not null, "ordinary point-funded purchase captured");
        await a.Send("raw " + JsonSerializer.Serialize(purchase, WireJson.Options));
        GameEvent retry = await a.WaitFor(e => e.Type == "ack" && e.Result!.Sequence == purchase!.Sequence && e.State is { Paused: true } s && s.Tick == frozen.Tick, "research retry", options.StartupTimeout, token);
        Require(Gameplay(State(retry)) == Gameplay(frozen), "retry charges no research or status twice");
        int player = b.PlayerId; int oldPeer = b.PeerId;
        await b.Send("quit"); Require(await b.WaitExit(token) == 0, "owned research peer exits before restart");
        await Observe(a, s => s.Revision >= frozen.Revision && !s.Players.Single(c => c.Id == player).Connected, "owned research city retained while absent", token);
        Child recovered = StartGame("test-research-resumed", false, true, port, null, "--automated", "--session-file", sessionPath);
        GameEvent resumed = await recovered.WaitFor(e => e.Type == "connected" && e.PeerId != oldPeer, "paused active research/status resume", options.StartupTimeout, token);
        Require(resumed.PlayerId == player && Gameplay(State(resumed)) == Gameplay(frozen), "resume retains points, choices and active deadlines without regrant");
        Require(frozen.Players.Where(c => !c.Eliminated).All(c => c.LastReward is { Research: 1 }), "latest shared clear research receipt is exactly one");
        return recovered;
    }
}
