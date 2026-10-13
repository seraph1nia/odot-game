using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    // Tier C: genuine ENet delivery, retained fallen-owner credential/ledger and
    // reconstruction are not proved by an in-memory session/component adapter.
    // Seed1, two early paid waves plus one next-wave allocation, <=100 decisions
    // per gate; existing children/ordered readers/port leases/owned cleanup only.
    // This retained individual is stored, NOT proof of active fallen fighters.
    private async Task EliminatedResume(int port, CancellationToken token)
    {
        await using var host = StartGameRole("fallen-host", "playing-host", true, port, null, "--combat-seed", "1");
        await host.WaitFor(e => e.Type == "ready", "fallen owner host ready", options.StartupTimeout, token);
        await using var ally = StartGame("fallen-ally", false, true, port, null, "--automated");
        await ally.WaitFor(e => e.Type == "connected", "second living owner", options.StartupTimeout, token);
        string path = Path.Combine(_scope!.Directory, "fallen-owner.json");
        await using var owner = StartGame("fallen-owner", false, true, port, null, "--session-file", path);
        GameEvent joined = await owner.WaitFor(e => e.Type == "connected", "third paid owner", options.StartupTimeout, token);
        int fallenId = joined.PlayerId;
        await Observe(host, s => s.Players.Length == 3, "three-owner shared roster", token);
        await Action(host, "start", token);
        foreach (Child client in new[] { ally, owner }) await Observe(client, s => s.Phase == Phase.Building, "paid opening delivered", token);
        Command? purchase = null;
        for (int wave = 1; wave <= 2; wave++)
        {
            await Observe(host, s => s.Wave == wave && s.DyingBodies.Length == 0, "home cleanup before paid investment", token);
            for (int gate = 0; gate < 4; gate++)
                foreach (Child client in new[] { host, ally, owner })
                {
                    Command? spent = await EliminatedInvestment(client, client == owner, token);
                    if (spent is not null && client == owner) purchase = spent;
                    await Action(client, "ready", token);
                }
            if (wave == 1)
            {
                MatchSnapshot clear = await Observe(host, s => s.Phase == Phase.Building && s.Wave == 2, "real cross-sector first clear", token);
                Require(clear.Players.All(c => !c.Eliminated) && clear.Players.Single(c => c.Id == fallenId).Health < HealthPoints.FromWhole(100),
                    "exposed Keep survives damaged through actual cooperative combat");
                foreach (Child client in new[] { ally, owner }) await Observe(client, s => s.Phase == Phase.Building && s.Wave == 2, "cross-sector clear delivered", token);
            }
        }
        await ObserveCurrent(owner, s => s.Phase == Phase.Combat && s.Players.Single(c => c.Id == fallenId).Eliminated,
            "current fallen-owner snapshot during continuing battle", token);
        // Spectators retain the intentional pause/resume privilege, never city
        // investment/readiness. Pause is an accepted authority barrier, not a sleep.
        MatchSnapshot frozen = State(await Action(owner, "pause", token));
        CityState fallen = frozen.Players.Single(c => c.Id == fallenId);
        Require(frozen.Phase == Phase.Combat && fallen.Eliminated && purchase is not null, "paid eliminated spectator remains in original ongoing match");
        UnitState member = fallen.Soldiers.Single();
        Require(member.Assignment is { Stored: true } && member.Health > 0 && !member.Participating && member.Hex!.Position == default,
            "paid stored survivor is retained, off-grid and unfunded rather than deleted");
        Require(fallen.ProductionIncome == default(ResourceCost), "fallen city publishes no future production");
        foreach (Child client in new[] { host, ally })
        {
            MatchSnapshot seen = await Observe(client, s => s.Revision == frozen.Revision, "identical fallen spectator barrier", token);
            Require(JsonSerializer.Serialize(seen, WireJson.Options) == JsonSerializer.Serialize(frozen, WireJson.Options), "actual peers agree on fallen roster, injuries, actions, claims, events and rules");
            CombatContact(seen);
        }
        CombatContact(frozen);
        Require(frozen.Squads.Any(s => s.Faction == Faction.Adventurers && s.Owner != fallenId && s.Hex.Lifecycle == UnitLifecycle.Alive),
            "surviving owner squads continue the current shared battle");
        await using (var protocol = ExpectedFailure(StartGame("fallen-old-protocol", false, true, port, null, "--automated", "--protocol-version", "1")))
            await protocol.WaitFor(e => e.Type == "connection-failed" && e.Message!.Contains("Protocol", StringComparison.Ordinal), "legacy protocol refusal", options.StartupTimeout, token);
        int malformed = owner.History().Count(e => e.Type == "ack" && e.Result?.Sequence == 0);
        await owner.Send("raw {bad");
        GameEvent malformedReceipt = await owner.WaitFor(e => e.Type == "ack" && e.Result?.Sequence == 0 && !e.Result.Accepted
            && owner.History().Count(e => e.Type == "ack" && e.Result?.Sequence == 0) > malformed, "malformed spectator request refused", options.StartupTimeout, token);
        Require(Gameplay(State(malformedReceipt)) == Gameplay(frozen), "malformed request changes no live state");
        await owner.Send("disconnect");
        await owner.WaitFor(e => e.Type == "server-disconnected", "owned spectator peer disconnect", options.StartupTimeout, token);
        await Observe(host, s => s.Paused && !s.Players.Single(c => c.Id == fallenId).Connected, "fallen city retained while absent", token);
        GameEvent[] beforeReconnect = owner.History();
        await owner.Send("reconnect");
        GameEvent rebound = await owner.WaitFor(e => e.Type == "connected" && !beforeReconnect.Any(old => ReferenceEquals(old, e)), "same-host same-process spectator reconnect", options.StartupTimeout, token);
        Require(rebound.PlayerId == fallenId && rebound.PeerId != joined.PeerId && Gameplay(State(rebound)) == Gameplay(frozen), "new ENet peer resumes identical frozen fallen city without regrant");
        await owner.Send("quit"); Require(await owner.WaitExit(token) == 0, "owned spectator process exits normally");
        await Observe(host, s => !s.Players.Single(c => c.Id == fallenId).Connected, "spectator absent before process restart", token);
        await using var resumed = StartGame("fallen-resumed", false, true, port, null, "--session-file", path);
        GameEvent restored = await resumed.WaitFor(e => e.Type == "connected", "same-host spectator process restart", options.StartupTimeout, token);
        Require(restored.PlayerId == fallenId && Gameplay(State(restored)) == Gameplay(frozen), "persisted credential retains member HP/identity and commitments without a fresh city");
        CombatContact(State(restored));
        await resumed.Send("raw " + JsonSerializer.Serialize(purchase, WireJson.Options));
        GameEvent retried = await resumed.WaitFor(e => e.Type == "ack" && e.Result?.Sequence == purchase!.Sequence, "original paid recruitment retry after fall and restart", options.StartupTimeout, token);
        Require(retried.Result!.Accepted && Gameplay(State(retried)) == Gameplay(frozen), "old accepted spending remains exactly once after elimination and restart");
        await Action(resumed, "resume", token);
        MatchSnapshot clearSecond = await Observe(host, s => s.Phase == Phase.Building && s.Wave == 3 && s.DyingBodies.Length == 0, "surviving owners clear continuing battle", token);
        await Observe(resumed, s => s.Revision >= clearSecond.Revision && s.Phase == Phase.Building && s.DyingBodies.Length == 0, "spectator receives clear and home cleanup", token);
        MatchSnapshot beforeDenial = Latest(resumed);
        foreach (string command in new[] { "ready", "build 0 farm", "recruit 2", $"retire {member.Id}", "research-tech melee-foundation" })
        {
            GameEvent denied = await Action(resumed, command, token, false);
            Require(denied.Message == "Only living cities can act during building." && Gameplay(State(denied)) == Gameplay(beforeDenial), "fallen owner city command refused without mutation: " + command);
            GameEvent[] old = resumed.History();
            await resumed.Send($"retry {denied.Result!.Sequence}");
            GameEvent duplicate = await resumed.WaitFor(e => e.Type == "ack" && e.Result == denied.Result && !old.Any(prior => ReferenceEquals(prior, e)), "refused spectator retry delivered", options.StartupTimeout, token);
            Require(Gameplay(State(duplicate)) == Gameplay(beforeDenial), "refused retry preserves full state");
        }
        CityState beforeIncome = beforeDenial.Players.Single(c => c.Id == fallenId);
        MatchSnapshot? next = null;
        for (int gate = 0; gate < 4; gate++)
        {
            foreach (Child client in new[] { host, ally })
            {
                await EliminatedInvestment(client, false, token);
                next = State(await Action(client, "ready", token));
            }
            CityState current = next!.Players.Single(c => c.Id == fallenId);
            Require(current.Resources == beforeIncome.Resources && current.Research == beforeIncome.Research
                && current.Soldiers.Select(RetainedMember).SequenceEqual(beforeIncome.Soldiers.Select(RetainedMember)) && !current.Ready,
                "living owners alone advance while fallen owner receives no production/healing/research/recruits");
        }
        Require(next is { Phase: Phase.Combat, Wave: 3 } && next.Enemies.Length == 15 && next.Enemies.All(u => u.Destination != fallenId)
            && next.Enemies.Any(u => u.Origin == fallenId), "next roster counted once and allocated to surviving owners only");
        CityState future = next!.Players.Single(c => c.Id == fallenId);
        Require(future.Soldiers.All(u => !u.Participating && u.Hex!.Lifecycle == UnitLifecycle.Stored)
            && next.Reservations.Positions.All(p => p.UnitId != member.SquadId)
            && JsonSerializer.Serialize(future.LastUpkeep, WireJson.Options) == JsonSerializer.Serialize(beforeIncome.LastUpkeep, WireJson.Options),
            "fallen survivor holds no future combat claim or new upkeep receipt");
        CombatContact(next);
        await Observe(resumed, s => s.Wave == 3 && s.Phase == Phase.Combat && s.Tick >= next.Tick, "actual spectator receives surviving-owner next battle", token);
        await host.Send("menu");
        await host.WaitFor(e => e.Type == "menu", "original host ends owned match", options.StartupTimeout, token);
        await resumed.WaitFor(e => e.Type == "session-ended", "host end reaches spectator", options.StartupTimeout, token);
        int acknowledgments = resumed.History().Count(e => e.Type == "ack");
        await resumed.Send("ready"); await resumed.Send("menu");
        await resumed.WaitFor(e => e.Type == "menu", "headless spectator return path after host end", options.StartupTimeout, token);
        Require(resumed.History().Count(e => e.Type == "ack") == acknowledgments, "post-host-loss gameplay is disabled");
    }

    private static string RetainedMember(UnitState member) => JsonSerializer.Serialize(new
    { member.Id, member.Health, member.Level, member.Profile, member.Experience, member.SquadId, member.FormationSlot, member.Assignment, member.Statuses }, WireJson.Options);

    private async Task<Command?> EliminatedInvestment(Child client, bool reserve, CancellationToken token)
    {
        Command? recruit = null;
        for (int actions = 0; actions < 100; actions++)
        {
            MatchSnapshot before = Latest(client);
            EconomyAction? plan = reserve ? CampaignStrategy.SpectatorReserveOpening(before, client.PlayerId) : CampaignStrategy.Next(before, client.PlayerId);
            if (plan is null) return recruit;
            string text = plan.Action switch
            {
                "build" => $"{(plan.Payment == ConstructionPayment.GoldRecovery ? "build-recovery" : "build")} {plan.Slot} {plan.Building}",
                "recruit" => $"recruit {plan.Slot} {plan.Unit}",
                "store" or "send" => $"{plan.Action} {plan.Slot} {plan.UnitId}",
                "retire" => $"retire {plan.UnitId}",
                "research-tech" => $"research-tech {TechnologyIds.Name(plan.Technology)}",
                "trade" => $"trade {plan.Slot} {plan.Resource} {plan.Bundles}",
                _ => $"{plan.Action} {plan.Slot}"
            };
            GameEvent accepted = await Action(client, text, token);
            if (plan.Action == "recruit")
            {
                recruit = plan.Command(before, client.PlayerId, accepted.Result!.Sequence);
                CityState old = before.Players.Single(c => c.Id == client.PlayerId), current = State(accepted).Players.Single(c => c.Id == client.PlayerId);
                RecruitmentQuote quote = old.RecruitmentQuotes.Single(q => q.Type == plan.Unit && q.Level == old.Slots[plan.Slot].Level);
                Require(old.Resources.TryPay(quote.Cost, out ResourceCost expected) && expected == current.Resources, "recruitment pays actual quote exactly once");
            }
        }
        throw new InvalidOperationException("Eliminated-owner paid opening exceeded 100 decisions.");
    }
}
