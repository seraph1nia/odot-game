using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    // Tier C: actual production host/ENet/codec; A/B cannot prove delivery or
    // credential-bound restoration of active field claims/actions. Two fixed
    // Keep-health controls, one battle each, owned children/ordered readers only.
    // Explicit synthetic tower materials/wound/poses: NOT earned balance,
    // full-game/reconnect acceptance or a general state injection interface.
    // Estimated ~20s plus existing serialized source preparation.
    private async Task ActiveFallen(int port, CancellationToken token)
    {
        await using (var denied = ExpectedFailure(StartGame("encounter-guest-refused", false, true, port, null,
            "--combat-seed", "1", "--controlled-receiving-encounter", "fallen-field")))
        {
            await denied.WaitFor(e => e.Type == "error" && e.Message!.StartsWith("Controlled receiving encounter requires", StringComparison.Ordinal),
                "owned guest cannot select authority fixture", options.StartupTimeout, token);
            Require(await denied.WaitExit(token) == 1, "fixture gate refuses guest before creating a peer/session");
        }
        await ActiveFieldCase(port, false, token);
        await ActiveFieldCase(port, true, token);
    }

    private async Task ActiveFieldCase(int port, bool protectedKeep, CancellationToken token)
    {
        string name = protectedKeep ? "protected-field" : "fallen-field";
        await using var host = StartGameRole(name + "-host", "playing-host", true, port, null,
            "--combat-seed", "1", "--controlled-receiving-encounter", name);
        await host.WaitFor(e => e.Type == "ready", "fixed controlled host ready", options.StartupTimeout, token);
        string path = Path.Combine(_scope!.Directory, name + "-owner.json");
        await using var owner = StartGame(name + "-owner", false, true, port, null, "--session-file", path);
        GameEvent joined = await owner.WaitFor(e => e.Type == "connected", "identified field owner joined", options.StartupTimeout, token);
        Require(joined.PlayerId == 2, "fixed input has exactly two owners");
        await using (var legacy = ExpectedFailure(StartGame(name + "-protocol14", false, true, port, null, "--protocol-version", "14")))
            await legacy.WaitFor(e => e.Type == "connection-failed" && e.Message!.Contains("Protocol", StringComparison.Ordinal),
                "both predecessor protocol14 schemas refuse before credentials", options.StartupTimeout, token);
        await Observe(host, s => s.Players.Length == 2, "two-owner fixed roster", token);
        await Action(host, "start", token);
        await Observe(owner, s => s.Phase == Phase.Building, "controlled opening received", token);
        for (int gate = 0; gate < 3; gate++)
        {
            foreach (var (player, action) in CampaignStrategy.RedistributionPreparation(Latest(host), host.PlayerId, owner.PlayerId))
            {
                Child client = player == host.PlayerId ? host : owner;
                string request = action.Action == "build"
                    ? $"{(action.Payment == ConstructionPayment.GoldRecovery ? "build-recovery" : "build")} {action.Slot} {action.Building}"
                    : $"recruit {action.Slot} {action.Unit}";
                await Action(client, request, token);
            }
            foreach (Child client in new[] { host, owner })
            {
                // Owned acceleration1 retains the accepted game's own rate;
                // it is not an extra phase or a second gameplay clock.
                if (gate == 2 && client == owner) await SimulationSpeed(host, 1, token);
                await Action(client, "ready", token);
            }
        }
        GameEvent configured = await host.WaitFor(e => e.Type == "controlled-encounter" && e.State?.Phase == Phase.Combat,
            "explicit numerical input receipt", options.StartupTimeout, token);
        MatchSnapshot input = State(configured);
        int[] fields = input.Players[1].Soldiers.Select(u => u.Id).ToArray();
        Require(fields.Length == 6 && input.Players[1].Soldiers.All(u => u.Participating && u.Health > 0) && input.Enemies.Length == 1,
            "nonempty identified active field roster, not stored-veteran substitution");
        int injured = input.Players[1].Soldiers.Single(u => u.Health == HealthPoints.FromWhole(33)).Id;
        Child receiving = owner;
        long loss = 0;
        Child? restarted = null;
        try
        {
            if (!protectedKeep)
            {
                await ObserveCurrent(owner, s => s.Phase == Phase.Combat && s.Players[1].Eliminated
                    && s.Players[1].Soldiers.Any(u => fields.Contains(u.Id) && u.Participating && u.Health > 0),
                    "current active fallen field survivor; missed windows fail", token);
                MatchSnapshot frozen = State(await Action(owner, "pause", token));
                Require(frozen.Phase == Phase.Combat && frozen.Players[1].Eliminated
                    && frozen.Players[1].Soldiers.All(u => fields.Contains(u.Id) && u.Participating && u.Health > 0), "active fallen roster at accepted pause barrier");
                loss = frozen.CombatEvents.Single(e => e.Type == CombatEventType.StructureDestroyed
                    && e.Structure is { Owner: 2, Kind: ArenaStructureKind.Keep }).Tick;
                Require(frozen.Players[1].Soldiers.Single(u => u.Id == injured).Health == HealthPoints.FromWhole(33), "individual injury survived actual receiving");
                Require(frozen.Structures.Where(s => s.Owner == 2).All(s => !s.Active)
                    && frozen.Players[1].Towers.All(t => !t.PendingImpact), "fallen owner's physical weapons cease without deleting surviving field actors");
                MatchSnapshot seen = await Observe(host, s => s.Revision == frozen.Revision, "exact authority/peer active-field barrier", token);
                Require(JsonSerializer.Serialize(seen, WireJson.Options) == JsonSerializer.Serialize(frozen, WireJson.Options), "actual peer receives exact authoritative members/HP/squads/source/action/events/config");
                CombatContact(frozen); CombatContact(seen);
                await owner.Send("disconnect");
                await owner.WaitFor(e => e.Type == "server-disconnected", "active field peer disconnected", options.StartupTimeout, token);
                await Observe(host, s => !s.Players[1].Connected, "active fallen owner absent", token);
                GameEvent[] old = owner.History(); await owner.Send("reconnect");
                GameEvent rebound = await owner.WaitFor(e => e.Type == "connected" && !old.Any(e0 => ReferenceEquals(e0, e)), "new peer resumes active fields on original host", options.StartupTimeout, token);
                Require(rebound.PlayerId == 2 && rebound.PeerId != joined.PeerId && Gameplay(State(rebound)) == Gameplay(frozen), "same-host peer resume preserves exact active sources/actions/member wounds");
                await owner.Send("quit"); Require(await owner.WaitExit(token) == 0, "original active field process exits normally");
                await Observe(host, s => !s.Players[1].Connected, "active field absent before owned restart", token);
                restarted = StartGame(name + "-resumed", false, true, port, null, "--session-file", path);
                GameEvent restored = await restarted.WaitFor(e => e.Type == "connected", "active field process restart", options.StartupTimeout, token);
                Require(restored.PlayerId == 2 && Gameplay(State(restored)) == Gameplay(frozen), "credential-file restart preserves the same active roster/claims/commitments without regrant");
                CombatContact(State(restored)); receiving = restarted;
                await Action(receiving, "resume", token);
            }
            MatchSnapshot impact = await Observe(receiving, s => s.CombatEvents.Any(e => e.Tick > loss && e.Type == CombatEventType.Impact
                && e.Landed && e.Damage > 0 && e.Victims.Length > 0 && e.Unit?.Owner == 2 && fields.Contains(e.Unit.Id)),
                "nonempty actually received current-battle field contribution", token);
            Require(impact.Players[1].Eliminated == !protectedKeep, "Keep-health disconfirming control, all other numerical inputs unchanged");
            CombatContact(impact);
            if (!protectedKeep)
            {
                Require(receiving.History().Where(e => e.State is not null).SelectMany(e => e.State!.CombatEvents)
                    .All(e => e.Tick <= loss || e.Type != CombatEventType.Impact || e.Tower?.City != 2), "no received post-fall owner weapon impact (same-tick simultaneous effects are allowed)");
                MatchSnapshot clear = await Observe(receiving, s => s.Phase == Phase.Building && s.Wave == 2 && s.DyingBodies.Length == 0,
                    "living owner actually clears continuing battle and physical cleanup", token);
                await Observe(host, s => s.Revision >= clear.Revision && s.DyingBodies.Length == 0, "authority clear received", token);
                foreach (string text in new[] { "ready", "build 0 farm", "recruit 1", $"retire {injured}", "research-tech melee-foundation" })
                {
                    MatchSnapshot before = Latest(receiving);
                    GameEvent denied = await Action(receiving, text, token, false);
                    Require(denied.Message == "Only living cities can act during building." && Gameplay(State(denied)) == Gameplay(before), "fallen field owner's city action cannot mutate live state: " + text);
                    GameEvent[] prior = receiving.History(); await receiving.Send($"retry {denied.Result!.Sequence}");
                    GameEvent retry = await receiving.WaitFor(e => e.Type == "ack" && e.Result == denied.Result && !prior.Any(e0 => ReferenceEquals(e0, e)), "fresh refused retry", options.StartupTimeout, token);
                    Require(Gameplay(State(retry)) == Gameplay(before), "refused retry is nonmutating");
                }
                CityState fallen = Latest(receiving).Players[1]; MatchSnapshot? next = null;
                for (int gate = 0; gate < 3; gate++)
                {
                    next = State(await Action(host, "ready", token));
                    CityState current = next.Players[1];
                    Require(current.Resources == fallen.Resources && current.Research == fallen.Research
                        && current.Soldiers.Select(RetainedMember).SequenceEqual(fallen.Soldiers.Select(RetainedMember)), "fallen field roster receives no future production/research/healing");
                }
                Require(next is { Phase: Phase.Combat, Wave: 2 } && next.Enemies.All(u => u.Destination == 1)
                    && next.Players[1].Soldiers.All(u => !u.Participating && u.Hex!.Lifecycle == UnitLifecycle.Reserve)
                    && next.Reservations.Positions.All(p => next.Squads.Single(s => s.Id == p.UnitId).Owner != 2), "surviving owner alone advances; fallen fighters become future nonparticipating Reserve/no claims");
                CombatContact(next!);
                await Observe(receiving, s => s.Wave == 2 && s.Phase == Phase.Combat, "actual peer receives survivor-only next allocation", token);
            }
            await host.Send("menu"); await host.WaitFor(e => e.Type == "menu", "original controlled host ends session", options.StartupTimeout, token);
            await receiving.WaitFor(e => e.Type == "session-ended", "host end actually delivered", options.StartupTimeout, token);
            int acks = receiving.History().Count(e => e.Type == "ack"); await receiving.Send("ready"); await receiving.Send("menu");
            await receiving.WaitFor(e => e.Type == "menu", "controlled peer returns after host loss", options.StartupTimeout, token);
            Require(receiving.History().Count(e => e.Type == "ack") == acks, "requests disabled after original host end");
            await host.Send("quit"); Require(await host.WaitExit(token) == 0, "controlled host exits before next fixed case reuses endpoint");
        }
        finally { if (restarted is not null) await restarted.DisposeAsync(); }
    }
}
