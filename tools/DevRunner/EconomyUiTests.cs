using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    private readonly HashSet<string> _inputWitnesses = [];
    private async Task<MatchSnapshot> UiReadyPair(Child client, Child observer, CancellationToken token, bool actualInput = true)
    {
        await TowerInvestment(observer, token);
        if (actualInput && _inputWitnesses.Add("Ready")) await ClickAck(client, "Ready", token);
        else await Action(client, "ready", token);
        MatchSnapshot resolved = State(await Action(observer, "ready", token));
        return await Observe(client, s => s.Revision >= resolved.Revision && s.TurnSerial == resolved.TurnSerial && s.Phase == resolved.Phase, "ordinary UI production/battle synchronization", token);
    }
    private async Task TowerOpening(Child observer, CancellationToken token)
    {
        await Action(observer, "build 0 farm", token); await Action(observer, "build 1 metalmine", token); await Action(observer, "build 2 barracks", token);
    }
    private async Task TowerInvestment(Child observer, CancellationToken token)
    {
        MatchSnapshot state = Latest(observer); CityState city = state.Players.Single(p => p.Id == observer.PlayerId);
        if (city.Eliminated || city.Ready || state.Phase is not (Phase.Building or Phase.Preparation)) return;
        if (city.Slots[3].Type == Building.Empty && city.Resources.TryPay(state.BuildingCatalog.Single(b => b.Type == Building.Lumbermill).Construction, out _))
            city = State(await Action(observer, "build 3 lumbermill", token)).Players.Single(p => p.Id == observer.PlayerId);
        if (city.Slots[4].Type == Building.Empty && city.Resources.TryPay(state.BuildingCatalog.Single(b => b.Type == Building.Stonecutter).Construction, out _))
            city = State(await Action(observer, "build 4 stonecutter", token)).Players.Single(p => p.Id == observer.PlayerId);
        while (city.Slots[2].Type == Building.Barracks && city.Soldiers.Length < 6)
        {
            RecruitmentQuote soldier = city.RecruitmentQuotes.Single(q => q.Type == UnitType.Swordsman && q.Level == city.Slots[2].Level);
            if (!city.Resources.TryPay(soldier.Cost, out _) || city.Food < (city.FoodForecast?.Demand ?? 0) + soldier.Upkeep) break;
            city = State(await Action(observer, "recruit 2", token)).Players.Single(p => p.Id == observer.PlayerId);
        }
        if (state.Wave >= 2 && city.Slots[2].Type == Building.Barracks && city.Resources.TryAdd(city.Slots[2].Refund, out ResourceCost refunded)
            && refunded.TryPay(state.BuildingCatalog.Single(b => b.Type == Building.CatapultTower).Construction, out _))
        {
            await Action(observer, "sell 2", token); city = State(await Action(observer, "build 2 catapulttower", token)).Players.Single(p => p.Id == observer.PlayerId);
        }
        if (city.Slots[2].Type == Building.CatapultTower && city.Slots[2].UpgradeQuote is ResourceCost upgrade && city.Resources.TryPay(upgrade, out _)) await Action(observer, "upgrade 2", token);
    }
    private async Task UiSwords(Child client, int slot, int target, CancellationToken token, bool actualInput = true)
    {
        if (actualInput && !_inputWitnesses.Contains("Recruit")) await Pick(client, slot, token);
        while (Latest(client).Players.Single(p => p.Id == client.PlayerId) is CityState city && city.Soldiers.Length < target
            && city.Resources.TryPay(city.RecruitmentQuotes.Single(q => q.Type == UnitType.Swordsman && q.Level == city.Slots[slot].Level).Cost, out _))
        {
            int food = city.Food; GameEvent recruit = actualInput && _inputWitnesses.Add("Recruit") ? await ClickAck(client, "Recruit", token) : await Action(client, $"recruit {slot}", token);
            Require(State(recruit).Players.Single(p => p.Id == client.PlayerId).Food == food, "actual material recruitment does not deduct food");
        }
    }
    private async Task<MatchSnapshot> UiClear(Child client, Child observer, int wave, CancellationToken token)
    {
        MatchSnapshot clear = await Observe(client, s => s.Phase == Phase.Defeat || s.Phase == Phase.Building && s.Wave == wave + 1, "ordinary UI wave clear", token);
        Require(clear.Phase == Phase.Building && clear.LastRewardedWave == wave, "funded defense clears without a stalled result and publishes its reward");
        await Observe(observer, s => s.Revision >= clear.Revision && s.Phase == Phase.Building, "observer sees reward/next wave", token);
        return clear;
    }
    private async Task EconomyDetails(Child client, Child observer, CancellationToken token)
    {
        await Click(client, "Details", token);
        UiObservation details = await WaitUi(client, p => p.DetailsOpen, "Details inspection opens explicitly", token);
        Require(details.UpkeepText.Length > 0 && details.RewardText.Length > 0 && details.RosterText.Contains('P') && details.DetailsText.Contains("Gold:", StringComparison.Ordinal), "Details retains upkeep, reward and cooperative roster");
        await Click(client, "CloseDetails", token);
        await Pick(client, 1, token); await ClickAck(client, "MetalMine", token);
        await Pick(client, 2, token); await ClickAck(client, "Barracks", token);
        await TowerOpening(observer, token);
        await UiReadyPair(client, observer, token); await UiReadyPair(client, observer, token);
        await Action(client, "build 3 lumbermill", token);
        await UiReadyPair(client, observer, token); await TowerInvestment(observer, token);
        await UiSwords(client, 2, 6, token);
        CityState equipped = Latest(client).Players.Single(p => p.Id == client.PlayerId);
        Require(equipped.Soldiers.Length == 6 && equipped.Food == 15 && equipped.Metal == 0, "six first-wave recruits are equipped by actual production with food retained");
        await UiReadyPair(client, observer, token);
        Require(Latest(client).Players.Single(p => p.Id == client.PlayerId).LastUpkeep is { Wave: 1, Paid: 6 }, "battle-start food receipt pays once through actual Ready input");
        await UiClear(client, observer, 1, token);
        await Action(client, "build 4 stonecutter", token);
        for (int production = 0; production < 3; production++) { await UiReadyPair(client, observer, token); await TowerInvestment(observer, token); }
        await Pick(client, 1, token); await ClickAck(client, "Sell", token);
        await ClickAck(client, "ResearchTower", token);

        await UiSwords(client, 2, 6, token); await UiReadyPair(client, observer, token); await UiClear(client, observer, 2, token);
        await UiReadyPair(client, observer, token);
        await Pick(client, 1, token); await ClickAck(client, "Upgrade", token);
        await UiReadyPair(client, observer, token);
        UnitState veteran = Latest(client).Players.Single(p => p.Id == client.PlayerId).Soldiers.First(u => u.Health < u.Profile.Health);
        await Click(client, "Research", token); await Click(client, "ResearchMelee", token); await ClickAck(client, "TechMeleeFoundation", token); await Click(client, "CloseResearch", token);
        await Pick(client, 1, token); await ClickAck(client, "Sell", token); await ClickAck(client, "MetalMine", token);
        CityState rebuilt = Latest(client).Players.Single(p => p.Id == client.PlayerId);
        Require(rebuilt.Research.Has(TechnologyId.MeleeFoundation) && rebuilt.Soldiers.Single(u => u.Id == veteran.Id).Health == veteran.Health, "selling research tower preserves technology and veteran wounds");
        CityState full = Latest(client).Players.Single(p => p.Id == client.PlayerId);
        Require(full.Slots.Where(s => s.Purchased).All(s => s.Type != Building.Empty), "five purchased plots are full before expansion");
        await Pick(client, 5, token);
        UiObservation locked = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(locked.LockedMarkers.Contains("Locked5", StringComparer.Ordinal), "locked plot has gold marker before purchase");
        GameEvent expansion = await ClickAck(client, "BuyPlot", token);
        UiObservation bought = await WaitUi(client, p => !p.LockedMarkers.Contains("Locked5", StringComparer.Ordinal), "accepted purchase removes gold marker", token);
        Require(bought.SelectedSlot == 5, "buy marker does not change plot selection");
        Require(State(expansion).Players.Single(p => p.Id == client.PlayerId).Slots[5].Purchased, "actual plot purchase permanently opens selected land");
        await UiReadyPair(client, observer, token);
        await Pick(client, 4, token); await ClickAck(client, "Sell", token);
        await Pick(client, 5, token); await ClickAck(client, "Market", token);
        async Task FundGold(int target, int metalReserve, int foodReserve)
        {
            for (int trade = 0; trade < 20; trade++)
            {
                MatchSnapshot state = Latest(client); CityState current = state.Players.Single(p => p.Id == client.PlayerId);
                if (current.Gold >= target) return;
                MarketRate? rate = new[] { Resource.Metal, Resource.Stone, Resource.Cloth, Resource.Food }
                    .Select(resource => state.MarketRates.Single(r => r.Resource == resource))
                    .FirstOrDefault(r => current.Resources.Amount(r.Resource) >= r.Units
                        + (r.Resource == Resource.Metal ? metalReserve : r.Resource == Resource.Food ? foodReserve : 0));
                Require(rate is not null, "ordinary surplus stocks fund the quoted economy purchase");
                await Action(client, $"trade 5 {rate!.Resource} 1", token);
            }
            throw new InvalidOperationException("Economy funding exceeds its bounded ordinary trades.");
        }
        CityState marketCity = Latest(client).Players.Single(p => p.Id == client.PlayerId);
        ResourceCost upgradeQuote = marketCity.Slots[2].UpgradeQuote!.Value;
        RecruitmentQuote futureRecruit = marketCity.RecruitmentQuotes.Single(q => q.Type == UnitType.Swordsman && q.Level == 2);
        ResourceCost equipmentQuote = futureRecruit.Cost;
        int retainedFood = checked((int)(marketCity.FoodForecast!.Demand + futureRecruit.Upkeep + Latest(client).MarketRates.Single(r => r.Resource == Resource.Food).Units));
        await ClickAck(client, "TradeMetal", token);
        await FundGold(upgradeQuote.Gold + equipmentQuote.Gold, equipmentQuote.Metal, retainedFood);
        await Pick(client, 2, token); await ClickAck(client, "Upgrade", token);
        int food = Latest(client).Players.Single(p => p.Id == client.PlayerId).Food;
        GameEvent leveled = await ClickAck(client, "Recruit", token);
        CityState army = State(leveled).Players.Single(p => p.Id == client.PlayerId);
        Require(army.Food == food && army.Soldiers[^1].Level == 2 && army.Soldiers.Any(u => u.Level == 1), "current level-two recruitment preserves mixed-level veterans and food");
        await Pick(client, 5, token); GameEvent foodSale = await ClickAck(client, "TradeFood", token);
        CityState soldFood = State(foodSale).Players.Single(p => p.Id == client.PlayerId);
        Require(soldFood.Food == food - Latest(client).MarketRates.Single(r => r.Resource == Resource.Food).Units && soldFood.FoodForecast!.Available == soldFood.Food, "food sale refreshes the authoritative next-battle preview");
        ResourceCost lumberQuote = Latest(client).BuildingCatalog.Single(b => b.Type == Building.Lumbermill).Construction;
        await FundGold(Math.Max(0, lumberQuote.Gold - soldFood.Slots[3].Refund.Gold), 0, (int)soldFood.FoodForecast!.Demand);
        await Pick(client, 3, token); long generation = Latest(client).Players.Single(p => p.Id == client.PlayerId).Slots[3].Generation;
        await ClickAck(client, "Sell", token); await ClickAck(client, "Lumbermill", token);
        Require(Latest(client).Players.Single(p => p.Id == client.PlayerId).Slots[3] is { Level: 1 } replacement && replacement.Generation > generation, "sale and rebuild establish a fresh producer on retained land");
        await Pick(client, 5, token); await Checkpoint(client, "economy-materials-market-land", token);
        await Click(client, "Settings", token); await Click(client, "GraphicsTab", token); await Click(client, "Resolution", token);
        await WaitUi(client, p => p.DropdownOpen, "economy second supported viewport choices", token);
        await client.Send("key Down"); await WaitUi(client, p => p.DropdownOpen && p.ResolutionFocused == 0, "1100x820 focused through native key input", token);
        await client.Send("key Enter"); await WaitUi(client, p => p.Width == 1100 && p.Height == 820 && !p.DropdownOpen, "expanded economy at 1100x820", token);
        await Click(client, "CloseSettings", token); await Checkpoint(client, "economy-market-1100", token);
        await Click(client, "City" + observer.PlayerId, token); await Pick(client, 2, token);
        UiObservation upgraded = await WaitUi(client, p => p.BuildingVariants.Length == 9 && p.BuildingVariants[2] == 2, "actual Catapult structural upgrade", token);
        Require(upgraded.PlotHeights.Length == 9 && upgraded.PlotHeights[0] < upgraded.PlotHeights[3] && upgraded.PlotHeights[3] < upgraded.PlotHeights[6], "three actual terrace heights");
        Countryside(upgraded);
        Require(upgraded.Placements.Any(p => p.Asset.EndsWith("tower_base_blue.gltf", StringComparison.Ordinal)) && upgraded.Placements.Any(p => p.Name == "Slot2" && p.Support > 1), "actual tower seated on raised support deck");
        CityState city = Latest(client).Players.Single(c => c.Id == observer.PlayerId);
        Require(upgraded.Stockpiles == new StockpileObservation(PresentationLimits.StockpileCount(city.Gold), PresentationLimits.StockpileCount(city.Food), PresentationLimits.StockpileCount(city.Wood)), "actual resource piles match current stocks");
        for (int slot = 0; slot < 9; slot++) await Pick(client, slot, token);
        await Pick(client, 2, token); UiObservation foreign = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(!foreign.Targets["Upgrade"].Enabled && !foreign.Targets["Sell"].Enabled, "foreign tower remains read only");
        await SimulationSpeed(_scope!.Children.First(c => c.Name.StartsWith("ui-server", StringComparison.Ordinal)), 1, token);
        await CameraZoom(client, token); await Pick(client, 2, token);
        Require(!(await UiProtocol.Probe(client, options.StartupTimeout, token)).Targets["Upgrade"].Enabled, "zoomed foreign roof remains read only");
        await Checkpoint(client, "economy-camera-roof", token); await Click(client, "ResetView", token); await Checkpoint(client, "economy-upgraded-roof", token);
        await Click(client, "City" + client.PlayerId, token); await Pick(client, 2, token);
    }
}
