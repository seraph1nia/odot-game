using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    // Disposable end-to-end regression witness: actual paid ENet campaigns,
    // no fixed-result fixture. Existing Child/Scenarios own every process/state.
    private async Task FinalDraftWitness(CancellationToken token)
    {
        var receipts = new List<object>();
        for (int players = Environment.GetEnvironmentVariable("ODOT_FINAL_DRAFT_FROM") == "3" ? 3 : 1; players <= 4; players++)
        {
            int port = _scope!.Port();
            Child host = StartGameRole($"late-{players}-host", "playing-host", true, port, null, "--combat-seed", "1");
            await host.WaitFor(e => e.Type == "ready", "original host ready", options.StartupTimeout, token);
            var clients = new List<Child> { host };
            for (int index = 1; index < players; index++)
            {
                Child guest = StartGameRole($"late-{players}-guest-{index}", "client", true, port);
                await guest.WaitFor(e => e.Type == "connected", "guest joined", options.StartupTimeout, token);
                clients.Add(guest);
            }
            await ObserveCurrent(host, s => s.Players.Length == players, "complete original roster", token);
            await SessionAction(host, "start", token);
            await SimulationSpeed(host, 8, token);
            await SessionAction(host, CombatSpeedRates.Action(CombatSpeed.Two), token);
            for (int wave = 1; wave <= 19; wave++)
            {
                for (int stage = 1; stage <= 3; stage++)
                {
                    foreach (Child client in clients)
                    {
                        await ObserveCurrent(client, s => s.Wave == wave && s.Turn == stage && s.Phase is Phase.Building or Phase.Preparation && s.DyingBodies.Length == 0, "current paid preparation", token);
                        for (int action = 0; action < 100; action++)
                        {
                            EconomyAction? decision = CampaignStrategy.Next(Latest(client), client.PlayerId);
                            if (decision is null) break;
                            string command = decision.Action switch
                            {
                                "build" => $"{(decision.Payment == ConstructionPayment.GoldRecovery ? "build-recovery" : "build")} {decision.Slot} {decision.Building}",
                                "recruit" => $"recruit {decision.Slot} {decision.Unit}",
                                "retire" => $"retire {decision.UnitId}",
                                "research-tech" => $"research-tech {TechnologyIds.Name(decision.Technology)}",
                                "trade" => $"trade {decision.Slot} {decision.Resource} {decision.Bundles}",
                                _ => $"{decision.Action} {decision.Slot}"
                            };
                            await Action(client, command, token);
                            Require(action < 99, "bounded policy");
                        }
                        await Action(client, "ready", token);
                    }
                }
                MatchSnapshot draft = await ObserveCurrent(host, s => s.Phase is Phase.RewardDraft or Phase.Defeat, "earned multiplayer draft", token);
                Require(draft.Phase == Phase.RewardDraft && draft.Players.All(c => !c.Eliminated) && draft.ProductionCount == wave * 3, $"all {players} cities earned clear {wave} with three settlements");
                foreach (Child client in clients)
                    await ObserveCurrent(client, s => s.Phase == Phase.RewardDraft && s.Wave == wave + 1, "all clients receive same draft barrier", token);
                foreach (CityState city in draft.Players)
                {
                    Require(city.Run.Offers.Length == 3 && city.Run.Offers.Distinct().Count() == 3, "three distinct personal offers");
                    if (wave == 19) Require(!city.Run.Offers.Contains("A04") && !city.Run.Offers.Contains("R04"), $"P{city.Id} final draft excludes secondary-only cards for {players} original players");
                }
                if (wave != 19)
                {
                    foreach (Child client in clients)
                    {
                        RunState run = draft.Players.Single(c => c.Id == client.PlayerId).Run;
                        string[] safe = run.Offers.Where(id => id is not ("A04" or "R04") && (!id.StartsWith('R') || run.Cards.All(c => !c.Id.StartsWith('R')))).ToArray();
                        Require(safe.Length > 0, "adversarial route can avoid secondary cards and keep room for a relic until final draft");
                        // Prefer a non-relic whenever available so a forced relic
                        // later does not exhaust the adversarial fixture's capacity.
                        string[] ordinary = safe.Where(id => !id.StartsWith('R')).ToArray();
                        await Action(client, "reward:" + CampaignStrategy.RewardChoice(run with { Offers = ordinary.Length > 0 ? ordinary : safe }), token);
                    }
                    await ObserveCurrent(host, s => s.Phase == Phase.Building && s.Wave == wave + 1, "all choices resolve barrier", token);
                    continue;
                }
                Require(draft.Players.All(c => c.Run.Stacks("A04") == 0 && c.Run.Stacks("R04") == 0 && c.Run.Cards.Count(card => card.Id.StartsWith('R')) < 2), "final secondary exclusion is not masked by stacking or relic caps");
                foreach (Child client in clients)
                {
                    await Action(client, "reward:A04", token, false);
                    await Action(client, "reward:R04", token, false);
                }
                await Action(host, "reward:" + CampaignStrategy.RewardChoice(draft.Players.Single(c => c.Id == host.PlayerId).Run), token);
                if (players > 1) Require(Latest(host).Phase == Phase.RewardDraft, "connected unselected guests still hold final-draft barrier");
                foreach (Child guest in clients.Skip(1)) { await guest.Send("quit"); Require(await guest.WaitExit(token) == 0, "guest quits pending draft"); }
                MatchSnapshot fallback = await ObserveCurrent(host, s => s.Phase == Phase.Building && s.Wave == 20, "disconnect resolves final fallback", token);
                foreach (CityState absent in fallback.Players.Where(c => c.Id != host.PlayerId))
                    Require(absent.Run.Choice == absent.Run.Offers[0] && absent.Run.Choice is not ("A04" or "R04"), "final fallback chooses first meaningful offer");
                Require(fallback.ProductionCount == 57, "fallback adds no settlement");
                receipts.Add(new { Players = players, FinalDraft = draft, Resolved = fallback });
                await File.WriteAllTextAsync(Path.Combine(_scope.EvidenceDirectory, "multiplayer-final-drafts.json"), JsonSerializer.Serialize(receipts, Evidence.JsonOptions), token);
            }
            await host.Send("quit"); Require(await host.WaitExit(token) == 0, "original host closes cleanly");
        }
    }
}
