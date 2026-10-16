using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    // Disposable product driver: existing Child/ScenarioScope/private-display ownership.
    // Ordinary paid commands only; no state loader, kills, resources or simulated clears.
    private async Task RogueliteWitness(CancellationToken token)
    {
        if (Environment.GetEnvironmentVariable("ODOT_ROGUELITE_EXTRA") == "1") { await DoctrineWitness(token); return; }
        Child client = StartGameRole("roguelite", "menu", false, 0, null, "--combat-seed", "1");
        await client.WaitFor(e => e.Type == "menu", "menu startup", options.StartupTimeout, token);
        await WaitUi(client, p => p.Screen == "menu", "menu", token);
        if (options.DesktopSurface) await WitnessFrame(client, "campaign-menu", token);
        await Click(client, "Singleplayer", token);
        await client.WaitFor(e => e.Type == "connected" && e.State?.Phase == Phase.Lobby, "solo doctrine lobby", options.StartupTimeout, token);
        await Click(client, "RunProgression", token);
        UiObservation doctrines = await WaitUi(client, p => p.RunOpen && p.Targets.Keys.Count(k => k.StartsWith("Run_", StringComparison.Ordinal)) == 6, "six doctrine controls", token);
        await WitnessFrame(client, "01-doctrines-top", token);
        await ClickAck(client, "Run_merchant", token);
        Require(Latest(client).Players.Single().Run.Doctrine == "merchant", "real Merchant click changes owned lobby doctrine");
        Require(Latest(client).Players.Single().Gold == 12, "doctrine preview does not allocate starting resources");
        await WitnessFrame(client, "02-merchant-selected", token);
        for (int n = 0; n < 7; n++) await client.Send("wheel 880 480 down");
        await WitnessFrame(client, "03-doctrines-lower", token);
        for (int n = 0; n < 15; n++) await client.Send("wheel 880 400 up");
        await WaitUi(client, p => p.Targets["Run_warden"].Y < 500, "first doctrine restored by scrolling", token);
        await ClickAck(client, "Run_warden", token);
        await Click(client, "CloseRun", token);
        await ClickAck(client, "Start", token);
        Require(Latest(client).Phase == Phase.Building && Latest(client).Players.Single().Gold == 10 && Latest(client).Players.Single().Stone == 2, "explicit Warden start allocates exact stocks once");
        await Action(client, "doctrine:merchant", token, false);
        await Click(client, "RunProgression", token);
        await WaitUi(client, p => p.RunOpen && p.Targets.Where(t => t.Key.StartsWith("Run_", StringComparison.Ordinal)).All(t => !t.Value.Enabled), "all doctrine choices locked after start", token);
        await WitnessFrame(client, "04-doctrine-locked", token);
        await Click(client, "CloseRun", token);
        if (Environment.GetEnvironmentVariable("ODOT_HORIZONTAL_VERIFY") == "1") { await HorizontalBoardWitness(client, token); return; }
        await SimulationSpeed(client, 8, token);
        await ClickAck(client, "SpeedTwo", token);
        var records = new List<object>();
        var acquired = new List<string>();
        int drafts = 0;
        for (int wave = 1; wave <= 20; wave++)
        {
            for (int stage = 1; stage <= 3; stage++)
            {
                await ObserveCurrent(client, s => s.Wave == wave && s.Phase is Phase.Building or Phase.Preparation && s.DyingBodies.Length == 0, "ordinary editable gate", token);
                for (int count = 0; count < 100; count++)
                {
                    MatchSnapshot before = Latest(client);
                    EconomyAction? decision = CampaignStrategy.Next(before, client.PlayerId);
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
                    Require(count < 99, "bounded ordinary campaign investment");
                }
                MatchSnapshot forecastState = Latest(client);
                string Forecast(UnitState u) => $"{u.Type}:{u.Level}:{u.Rank}:{u.IsBoss}:{u.Owner}:{u.Destination}:{u.Profile}";
                string[] forecast = forecastState.NextWave.Select(Forecast).Order().ToArray();
                await Action(client, "ready", token);
                if (stage == 3)
                {
                    MatchSnapshot combat = await ObserveCurrent(client, s => s.Phase == Phase.Combat && s.Wave == wave, "paid combat entry", token);
                    // Latest combat may already have lost members; the ordered reader retains the entry snapshot.
                    MatchSnapshot entry = client.History().Where(e => e.State?.Phase == Phase.Combat && e.State.Wave == wave).Select(e => e.State!).First();
                    Require(forecast.SequenceEqual(entry.Enemies.Select(Forecast).Order()), "actual combat allocation equals preceding canonical forecast W" + wave);
                    Require(combat.ProductionCount == wave * 3, "exactly three settlements per wave W" + wave);
                }
            }
            MatchSnapshot result = await ObserveCurrent(client, s => s.Wave >= wave && s.Phase is Phase.RewardDraft or Phase.Victory or Phase.Defeat, "earned wave outcome", token);
            Require(result.Phase != Phase.Defeat, "paid campaign remains alive W" + wave);
            if (wave == 20)
            {
                Require(result.Phase == Phase.Victory && result.ProductionCount == 60 && drafts == 19, "earned victory has 19 drafts and 60 settlements with no final draft");
                await WitnessFrame(client, "09-victory", token);
                records.Add(new { Wave = wave, result.Phase, result.ProductionCount, City = result.Players.Single() });
                break;
            }
            Require(result.Phase == Phase.RewardDraft && result.Wave == wave + 1, "successful nonfinal clear enters authoritative reward draft");
            string[] offers = result.Players.Single().Run.Offers;
            Require(offers.Length == 3 && offers.Distinct().Count() == 3, "three distinct offers W" + wave);
            if (wave == 19) Require(!offers.Contains("A04") && !offers.Contains("R04") && !offers.Contains("R05") && !offers.Contains("R03"), "final solo draft excludes effects with no remaining trigger");
            UiObservation draft = await WaitUi(client, p => p.RunOpen && p.RunText.Any(t => t.StartsWith("Next wave " + (wave + 1) + ":", StringComparison.Ordinal)), "earned native reward dialog and next-wave forecast", token);
            Require(offers.All(id => draft.Targets.ContainsKey("Run_" + id)), "native choices equal authority offers");
            if (wave is 1 or 7 or 14 or 19) await WitnessFrame(client, "draft-after-" + wave, token);
            if (wave == 1)
            {
                await client.Send("key Escape");
                await WaitUi(client, p => !p.RunOpen, "Escape closes draft without choice", token);
                Require(Latest(client).Phase == Phase.RewardDraft && Latest(client).Players.Single().Run.Choice is null, "closing does not advance draft");
                await WitnessFrame(client, "05-closed-draft", token);
                await client.Send("key Enter");
                await WaitUi(client, p => p.RunOpen, "keyboard reopens reward modal from restored invoker", token);
                await Action(client, "reward:invalid", token, false);
            }
            string selected = CampaignStrategy.RewardChoice(result.Players.Single().Run);
            // Third card can be below the scroll viewport. Scroll through the real modal.
            for (int n = 0; n < 10; n++) await client.Send("wheel 880 480 up");
            UiObservation choiceFrame = await UiProtocol.Probe(client, options.StartupTimeout, token);
            for (int n = 0; choiceFrame.Targets["Run_" + selected].Y > choiceFrame.Targets["CloseRun"].Y - 60 && n < 12; n++)
            {
                await client.Send("wheel 880 480 down");
                choiceFrame = await UiProtocol.Probe(client, options.StartupTimeout, token);
            }
            await ClickAck(client, "Run_" + selected, token);
            MatchSnapshot chosen = Latest(client);
            Require(chosen.Phase == Phase.Building && chosen.ProductionCount == result.ProductionCount, "one native card click advances without fourth settlement");
            Require(chosen.Players.Single().Run.Choice == selected && chosen.Players.Single().Run.Stacks(selected) > 0, "accepted snapshot contains acquired card");
            await Action(client, "reward:" + selected, token, false);
            drafts++; acquired.Add(selected);
            records.Add(new { ClearedWave = wave, Offers = offers, Choice = selected, result.ProductionCount, Run = chosen.Players.Single().Run, Reward = chosen.Players.Single().LastReward, Forecast = result.NextWave });
            await File.WriteAllTextAsync(Path.Combine(_scope!.EvidenceDirectory, "earned-campaign.json"), JsonSerializer.Serialize(new { Drafts = drafts, Acquired = acquired, Waves = records }, Evidence.JsonOptions), token);
        }
        await File.WriteAllTextAsync(Path.Combine(_scope!.EvidenceDirectory, "earned-campaign.json"), JsonSerializer.Serialize(new { Drafts = drafts, Acquired = acquired, Waves = records }, Evidence.JsonOptions), token);
        await client.Send("quit"); Require(await client.WaitExit(token) == 0, "owned game exits normally");
    }
    private async Task DoctrineWitness(CancellationToken token)
    {
        Child client = StartGameRole("doctrines", "menu", false, 0, null, "--combat-seed", "1");
        await client.WaitFor(e => e.Type == "menu", "initial menu", options.StartupTimeout, token);
        if (options.DesktopSurface) await WitnessFrame(client, "doctrines-menu", token);
        (string Id, int Gold, int Wood, int Stone, int Metal, int Cloth, int Food, int Research)[] starts =
        [
            ("warden", 10, 6, 2, 0, 0, 0, 0),
            ("merchant", 16, 4, 0, 0, 0, 0, 0), ("warmaster", 12, 6, 0, 4, 0, 6, 0),
            ("arcanist", 9, 6, 3, 0, 3, 0, 1), ("grovekeeper", 12, 8, 0, 0, 0, 0, 0), ("huntsman", 12, 6, 0, 2, 0, 0, 0)
        ];
        var receipts = new List<object>();
        foreach (var expected in starts)
        {
            await WaitUi(client, p => p.Screen == "menu", "fresh menu", token);
            await Click(client, "Singleplayer", token);
            MatchSnapshot lobby = await ObserveCurrent(client, s => s.Phase == Phase.Lobby && !receipts.Any(r => ReferenceEquals(r, s)), "fresh solo lobby", token);
            string match = lobby.MatchId;
            async Task NativeChoice(string name)
            {
                long floor = client.History().Where(e => e.Type == "ack" && e.State?.MatchId == match).Select(e => e.Result!.Sequence).DefaultIfEmpty().Max();
                await Click(client, name, token);
                GameEvent ack = await client.WaitFor(e => e.Type == "ack" && e.State?.MatchId == match && e.Result!.Sequence > floor, "native " + name, options.StartupTimeout, token);
                Require(ack.Result!.Accepted, "native " + name + " accepted");
            }
            await Click(client, "RunProgression", token);
            await WaitUi(client, p => p.RunOpen, "doctrine dialog open", token);
            for (int n = 0; n < 15; n++) await client.Send("wheel 880 480 up");
            UiObservation frame = await UiProtocol.Probe(client, options.StartupTimeout, token);
            string target = "Run_" + expected.Id;
            for (int n = 0; frame.Targets[target].Y > frame.Targets["CloseRun"].Y - 60 && n < 20; n++)
            {
                await client.Send("wheel 880 480 down");
                frame = await UiProtocol.Probe(client, options.StartupTimeout, token);
            }
            await NativeChoice(target);
            Require(Latest(client).Players.Single().Run.Doctrine == expected.Id, "lobby selection is " + expected.Id);
            await WitnessFrame(client, "selected-" + expected.Id, token);
            await Click(client, "CloseRun", token);
            await NativeChoice("Start");
            CityState city = Latest(client).Players.Single();
            Require(city.Gold == expected.Gold && city.Wood == expected.Wood && city.Stone == expected.Stone && city.Metal == expected.Metal && city.Cloth == expected.Cloth && city.Food == expected.Food && city.Research.Points == expected.Research, "exact live starting allocation " + expected.Id);
            receipts.Add(new { expected.Id, city.Resources, city.Research, city.Run, city.RecruitmentQuotes });
            if (expected.Id == "huntsman")
            {
                await Click(client, "Settings", token);
                await ResizeTo1280(client, token);
                await Click(client, "CloseSettings", token);
                await Click(client, "RunProgression", token);
                await WaitUi(client, p => p.RunOpen && p.Width == 1280 && p.Height == 720, "resized locked doctrine dialog", token);
                await WitnessFrame(client, "doctrines-1280x720", token);
                await Click(client, "CloseRun", token);
            }
            await Click(client, "ReturnToMenu", token); await Click(client, "ConfirmReturn", token);
            await WaitUi(client, p => p.Screen == "menu" && !p.Connected, "session disposed before next doctrine", token);
        }
        await File.WriteAllTextAsync(Path.Combine(_scope!.EvidenceDirectory, "six-starting-allocations.json"), JsonSerializer.Serialize(receipts, Evidence.JsonOptions), token);
        await Click(client, "Exit", token); Require(await client.WaitExit(token) == 0, "all six doctrine sessions exit normally");
    }
    private async Task<UiObservation> WitnessFrame(Child client, string name, CancellationToken token)
    {
        string path = Path.Combine(_scope!.EvidenceDirectory, name + ".png");
        UiObservation frame = await UiProtocol.Probe(client, options.StartupTimeout, token, path);
        UiProtocol.Frame(frame, path);
        if (options.DesktopSurface) RequireDesktopGpu(client, frame);
        await File.WriteAllTextAsync(Path.Combine(_scope.EvidenceDirectory, name + ".json"), JsonSerializer.Serialize(frame, Evidence.JsonOptions), token);
        Console.WriteLine($"FRAME {name}: {frame.Width}x{frame.Height}; {path}");
        return frame;
    }
}
