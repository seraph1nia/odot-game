using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    // Mapped020 Tier C: actual ENet conjunction/resume, not default-paid
    // combined journey or active fighters. Real paid service/Hall prefix;
    // explicit numerical original-enemy roster/poses/Keep threshold afterwards.
    // Existing children/readers/ports/preferences/credential/cleanup owners only.
    private async Task EliminatedResume(int port, CancellationToken token)
    {
        await StoredVeteranCase(port, false, token);
        await StoredVeteranCase(port, true, token);
    }
    private async Task StoredVeteranCase(int port, bool protectedKeep, CancellationToken token)
    {
        string name = protectedKeep ? "protected-stored" : "fallen-stored";
        await using var host = StartGameRole(name + "-host", "playing-host", true, port, null,
            "--combat-seed", "1", "--controlled-receiving-encounter", name);
        await host.WaitFor(e => e.Type == "ready", "controlled stored host ready", options.StartupTimeout, token);
        await using var ally = StartGame(name + "-ally", false, true, port, null);
        await ally.WaitFor(e => e.Type == "connected", "second living Keep owner", options.StartupTimeout, token);
        string path = Path.Combine(_scope!.Directory, name + "-owner.json");
        await using var owner = StartGame(name + "-owner", false, true, port, null, "--session-file", path);
        GameEvent joined = await owner.WaitFor(e => e.Type == "connected", "paid Hall owner", options.StartupTimeout, token);
        int fallenId = joined.PlayerId;
        Require(host.PlayerId == 1 && ally.PlayerId == 2 && fallenId == 3, "fixed three-owner accepted roster");
        await Observe(host, s => s.Players.Length == 3, "three owner roster", token);
        await Action(host, "start", token);
        Command? purchase = null;
        for (int wave = 1; wave <= 2; wave++)
        {
            await Observe(host, s => s.Wave == wave && s.Phase == Phase.Building && s.DyingBodies.Length == 0,
                "real paid wave/cleanup boundary", token);
            for (int gate = 0; gate < 3; gate++)
            {
                MatchSnapshot currentPhase = Latest(host);
                foreach (Child client in new[] { ally, owner })
                    await ObserveCurrent(client, s => s.MatchId == currentPhase.MatchId && s.Phase == currentPhase.Phase
                        && s.Wave == currentPhase.Wave && s.TurnSerial == currentPhase.TurnSerial,
                        "current paid phase/wave/serial delivered before decisions (Lobby serial alone is ambiguous)", token);
                foreach (var (player, action) in CampaignStrategy.EliminatedSupportPreparation(Latest(host)))
                    await StoredPaidAction(player == 1 ? host : ally, action, token);
                for (int action = 0; CampaignStrategy.SpectatorReserveOpening(Latest(owner), fallenId) is EconomyAction plan; action++)
                {
                    Require(action < 100, "finite real paid stored prefix");
                    Command command = await StoredPaidAction(owner, plan, token);
                    if (plan.Action == "recruit") purchase = command;
                }
                foreach (Child client in new[] { host, ally, owner }) await Action(client, "ready", token);
                if (gate < 2)
                {
                    int serial = Latest(owner).TurnSerial;
                    foreach (Child client in new[] { host, ally })
                        await Observe(client, s => s.TurnSerial == serial, "actual END settlement delivered", token);
                }
            }
            MatchSnapshot clear = await Observe(host, s => s.Phase == Phase.RewardDraft && s.Wave == wave + 1,
                "actual paid clear before numerical input", token);
            Require(clear.Players.All(c => !c.Eliminated), "paid prefix did not eliminate a Keep");
            await ResolveRewards([host, ally, owner], token);
        }
        Require(purchase is not null, "authentic recruitment receipt retained");
        await Observe(host, s => s.DyingBodies.Length == 0, "paid prefix physical cleanup", token);
        for (int gate = 0; gate < 3; gate++)
        {
            if (gate == 2) await SimulationSpeed(host, 1, token);
            foreach (Child client in new[] { host, ally, owner }) await Action(client, "ready", token);
            if (gate < 2)
            {
                int serial = Latest(owner).TurnSerial;
                foreach (Child client in new[] { host, ally }) await Observe(client, s => s.TurnSerial == serial, "third-wave END delivered", token);
            }
        }
        GameEvent configured = await host.WaitFor(e => e.Type == "controlled-encounter" && e.State is { Phase: Phase.Combat, Wave: 3 },
            "declared numerical stored input", options.StartupTimeout, token);
        MatchSnapshot input = State(configured);
        CityState authentic = input.Players.Single(c => c.Id == fallenId);
        Require(authentic.Soldiers.Length > 0 && authentic.Soldiers.All(u => u.Experience >= 1 && u.Assignment is { Stored: true } && !u.Participating)
            && input.Enemies.Length == 1, "real served stored IDs + ONE controlled original enemy, no field substitution");
        Child receiving = owner;
        Child? restarted = null;
        try
        {
            if (!protectedKeep)
            {
                await ObserveCurrent(owner, s => s.Phase == Phase.Combat && s.Players.Single(c => c.Id == fallenId).Eliminated
                    && s.Players.Any(c => c.Id != fallenId && !c.Eliminated), "actual full continuing eliminated stored conjunction", token);
                MatchSnapshot frozen = State(await Action(owner, "pause", token));
                CityState fallen = frozen.Players.Single(c => c.Id == fallenId);
                Require(frozen.Phase == Phase.Combat && fallen.Eliminated && frozen.Enemies.Length > 0
                    && frozen.Players.Any(c => c.Id != fallenId && !c.Eliminated)
                    && frozen.Squads.Any(s => s.Owner != fallenId && s.Faction == Faction.Adventurers && s.ParticipatingMembers.Length > 0),
                    "full conjunction at accepted authority pause, actual living field support");
                Require(fallen.Soldiers.Select(RetainedMember).SequenceEqual(authentic.Soldiers.Select(RetainedMember))
                    && fallen.ProductionIncome == default(ResourceCost), "exact paid stored HP/XP/equipment/assignment and no fallen production");
                Require(frozen.CombatEvents.Any(e => e.Type == CombatEventType.StructureDestroyed
                    && e.Structure is { Kind: ArenaStructureKind.Keep, Owner: 3 }) && frozen.Structures.Where(s => s.Owner == fallenId).All(s => !s.Active),
                    "genuine Keep destruction, not fake eliminated flag; owner weapons inactive");
                foreach (Child client in new[] { host, ally })
                {
                    MatchSnapshot seen = await Observe(client, s => s.Revision == frozen.Revision, "exact controlled spectator barrier", token);
                    Require(JsonSerializer.Serialize(seen, WireJson.Options) == JsonSerializer.Serialize(frozen, WireJson.Options),
                        "actual peers receive exact actors/member HP/XP/source/claims/events/config");
                    CombatContact(seen);
                }
                CombatContact(frozen);
                await using (var protocol = ExpectedFailure(StartGame("stored-protocol14", false, true, port, null, "--protocol-version", "14")))
                    await protocol.WaitFor(e => e.Type == "connection-failed" && e.Message!.Contains("Protocol", StringComparison.Ordinal),
                        "both predecessor14 histories refuse before credentials", options.StartupTimeout, token);
                GameEvent[] oldRaw = owner.History(); await owner.Send("raw {bad");
                GameEvent malformed = await owner.WaitFor(e => e.Type == "ack" && e.Result is { Sequence: 0, Accepted: false }
                    && !oldRaw.Any(old => ReferenceEquals(old, e)), "fresh malformed spectator refusal", options.StartupTimeout, token);
                Require(Gameplay(State(malformed)) == Gameplay(frozen), "malformed request nonmutating");
                // Existing build CLI binds foreign City while reserving this
                // client's real monotonic sequence; no fake future raw sequence.
                GameEvent foreignAck = await Action(owner, $"build 0 Farm {host.PlayerId}", token, false);
                Require(Gameplay(State(foreignAck)) == Gameplay(frozen), "foreign ownership refusal nonmutating");
                await owner.Send("disconnect");
                await owner.WaitFor(e => e.Type == "server-disconnected", "owned spectator disconnect", options.StartupTimeout, token);
                await Observe(host, s => !s.Players.Single(c => c.Id == fallenId).Connected, "stored owner retained while absent", token);
                GameEvent[] beforeReconnect = owner.History(); await owner.Send("reconnect");
                GameEvent rebound = await owner.WaitFor(e => e.Type == "connected" && !beforeReconnect.Any(old => ReferenceEquals(old, e)),
                    "same-host new-peer stored restore", options.StartupTimeout, token);
                Require(rebound.PlayerId == fallenId && rebound.PeerId != joined.PeerId && Gameplay(State(rebound)) == Gameplay(frozen),
                    "same-host stored roster/HP/XP/claims/commitments no regrant");
                await owner.Send("quit"); Require(await owner.WaitExit(token) == 0, "owned spectator exits before restart");
                await Observe(host, s => !s.Players.Single(c => c.Id == fallenId).Connected, "absent before restart", token);
                restarted = StartGame(name + "-resumed", false, true, port, null, "--session-file", path);
                GameEvent restored = await restarted.WaitFor(e => e.Type == "connected", "credential-file stored process restore", options.StartupTimeout, token);
                Require(restored.PlayerId == fallenId && Gameplay(State(restored)) == Gameplay(frozen), "exact credential-bound stored resume");
                CombatContact(State(restored)); receiving = restarted;
                await receiving.Send("raw " + JsonSerializer.Serialize(purchase, WireJson.Options));
                GameEvent retry = await receiving.WaitFor(e => e.Type == "ack" && e.Result?.Sequence == purchase!.Sequence,
                    "original paid receipt retry after loss/restart", options.StartupTimeout, token);
                Require(retry.Result!.Accepted && Gameplay(State(retry)) == Gameplay(frozen), "accepted original purchase exactly once");
                await Action(receiving, "resume", token);
            }
            MatchSnapshot clearThird = await Observe(receiving, s => s.Phase == Phase.RewardDraft && s.Wave == 4 && s.DyingBodies.Length == 0,
                "actual controlled one-enemy team clear", token);
            await ResolveRewards([host, ally, receiving], token);
            Require(clearThird.Players.Single(c => c.Id == fallenId).Eliminated == !protectedKeep
                && clearThird.Players.Single(c => c.Id == fallenId).Soldiers.Select(RetainedMember).SequenceEqual(authentic.Soldiers.Select(RetainedMember)),
                "threshold disconfirming control, exact stored members preserved");
            await Observe(host, s => s.Revision >= clearThird.Revision && s.DyingBodies.Length == 0, "authority cleanup delivered", token);
            if (!protectedKeep)
            {
                foreach (string text in new[] { "ready", "build 0 farm", "recruit 2", $"retire {authentic.Soldiers[0].Id}", "research-tech melee-foundation" })
                {
                    MatchSnapshot before = Latest(receiving);
                    GameEvent denied = await Action(receiving, text, token, false);
                    Require(denied.Message == "Only living cities can act during building." && Gameplay(State(denied)) == Gameplay(before),
                        "fallen stored city denial nonmutating: " + text);
                    GameEvent[] old = receiving.History(); await receiving.Send($"retry {denied.Result!.Sequence}");
                    GameEvent again = await receiving.WaitFor(e => e.Type == "ack" && e.Result == denied.Result && !old.Any(e0 => ReferenceEquals(e0, e)),
                        "fresh refused spectator retry", options.StartupTimeout, token);
                    Require(Gameplay(State(again)) == Gameplay(before), "refused retry does not mutate");
                }
                CityState fallen = Latest(receiving).Players.Single(c => c.Id == fallenId);
                UnitState[] manifest = clearThird.NextWave;
                MatchSnapshot? next = null;
                for (int gate = 0; gate < 3; gate++)
                {
                    foreach (Child client in new[] { host, ally }) next = State(await Action(client, "ready", token));
                    CityState current = next!.Players.Single(c => c.Id == fallenId);
                    Require(current.Resources == fallen.Resources && current.Research == fallen.Research && !current.Ready
                        && current.Soldiers.Select(RetainedMember).SequenceEqual(fallen.Soldiers.Select(RetainedMember))
                        && JsonSerializer.Serialize(current.LastUpkeep, WireJson.Options) == JsonSerializer.Serialize(fallen.LastUpkeep, WireJson.Options),
                        "all three real future ENDs exclude fallen resources/research/healing/food/member admission");
                    if (gate < 2) await Observe(host, s => s.TurnSerial == next!.TurnSerial, "living-only END delivered", token);
                }
                Require(next is { Phase: Phase.Combat, Wave: 4 } && next.Enemies.Length == manifest.Length
                    && next.Enemies.All(u => u.Destination != fallenId) && next.Enemies.Any(u => u.Origin == fallenId)
                    && manifest.Select(u => (u.Type, u.Level, u.Rank, u.Origin, u.Destination, u.Profile))
                        .SequenceEqual(next.Enemies.Select(u => (u.Type, u.Level, u.Rank, u.Origin, u.Destination, u.Profile))),
                    "authoritative future manifest consumed once, survivor-only allocation");
                Require(next!.Players.Single(c => c.Id == fallenId).Soldiers.All(u => !u.Participating && u.Hex!.Lifecycle == UnitLifecycle.Stored
                    && u.Hex.Position == default && next.Reservations.Positions.All(p => p.UnitId != u.SquadId)), "stored veteran future no combat claims");
                CombatContact(next);
                await Observe(receiving, s => s.Wave == 4 && s.Phase == Phase.Combat, "actual future allocation received", token);
            }
            await host.Send("menu"); await host.WaitFor(e => e.Type == "menu", "original host ends controlled session", options.StartupTimeout, token);
            await receiving.WaitFor(e => e.Type == "session-ended", "host end reaches actual spectator", options.StartupTimeout, token);
            int acks = receiving.History().Count(e => e.Type == "ack"); await receiving.Send("ready"); await receiving.Send("menu");
            await receiving.WaitFor(e => e.Type == "menu", "owned post-host-loss return", options.StartupTimeout, token);
            Require(receiving.History().Count(e => e.Type == "ack") == acks, "gameplay disabled after host loss");
            await host.Send("quit"); Require(await host.WaitExit(token) == 0, "host reaped before endpoint control reuse");
        }
        finally { if (restarted is not null) await restarted.DisposeAsync(); }
    }

    private static string RetainedMember(UnitState member) => JsonSerializer.Serialize(new
    { member.Id, member.Health, member.Level, member.Profile, member.Experience, member.SquadId, member.FormationSlot, member.Assignment, member.Statuses }, WireJson.Options);

    private async Task<Command> StoredPaidAction(Child client, EconomyAction plan, CancellationToken token)
    {
        MatchSnapshot before = Latest(client);
        string text = plan.Action switch
        {
            "build" => $"{(plan.Payment == ConstructionPayment.GoldRecovery ? "build-recovery" : "build")} {plan.Slot} {plan.Building}",
            "recruit" => $"recruit {plan.Slot} {plan.Unit}",
            "store" or "send" => $"{plan.Action} {plan.Slot} {plan.UnitId}",
            _ => $"{plan.Action} {plan.Slot}"
        };
        GameEvent accepted = await Action(client, text, token);
        if (plan.Action == "recruit")
        {
            CityState old = before.Players.Single(c => c.Id == client.PlayerId), current = State(accepted).Players.Single(c => c.Id == client.PlayerId);
            RecruitmentQuote quote = old.RecruitmentQuotes.Single(q => q.Type == plan.Unit && q.Level == old.Slots[plan.Slot].Level);
            Require(old.Resources.TryPay(quote.Cost, out ResourceCost expected) && expected == current.Resources, "actual quoted equipment paid once");
        }
        return plan.Command(before, client.PlayerId, accepted.Result!.Sequence);
    }
}
