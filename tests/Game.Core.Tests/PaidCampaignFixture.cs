using System.Text.Json;
using DevRunner;
using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

// Verification driver, not a simulator: production Match, default twenty waves,
// accepted paid commands and fixed steps only. Never grants resources or members.
// Integrated economics: exactly three Preparations; final END settles and
// enters Combat immediately. Tier A uses bounded openings; full runs are Tier
// Deep. Historical four-edit baselines are not qualification for these inputs.
internal static class PaidCampaignFixture
{
    internal sealed record Receipt(Command Command, ResourceCost Before, ResourceCost After, ResearchState ResearchBefore, ResearchState ResearchAfter);
    internal sealed record Travel(long Tick, int Squad, int Owner, int From, int To);
    internal sealed record Rescue(long Sequence, long Tick, int Attacker, int Squad, int Owner, int Victim, int ThreatenedOwner, int Cell, int KeepHealth);
    internal sealed record Wave(int Number, int Ticks, int KeepDamage, int WatchpostDamage, int Casualties, int Injured, int LostHealth,
        int Unfed, int PaidFood, int ForeignOriginImpacts, CityState[] Entry, CityState[] Exit)
    {
        // Opt-in diagnostic transcript of the existing numerical event reader;
        // never expands incoming snapshots, protocol history or simulation.
        public UnitState[] EnemyEntry { get; init; } = [];
        public CombatEvent[] Events { get; init; } = [];
        public Travel[] Travel { get; init; } = [];
        public Rescue[] Rescues { get; init; } = [];
    }
    internal sealed record Result(string Family, int Players, ulong Seed, int MaximumHomes, int MaximumTier, bool HallUpgrades,
        Phase Outcome, DefeatReason Reason, StallDiagnostic? Stall, int ReachedWave, int ProductionCount, int CombatTicks,
        long HealedHealth, ResourceCost RecoveryInvestment, Receipt[] Commands, Wave[] Waves, MatchSnapshot Final)
    {
        public string Economics { get; init; } = "Integrated PR24: exactly three Preparations, final END settles and enters Combat without another spending window";
        public int RequestedHorizon { get; init; } = 20;
    }
    internal static Match Opening(int players = 2, ulong seed = 1)
    {
        var match = new Match(combatSeed: seed);
        try
        {
            for (int i = 0; i < players; i++) match.Join();
            Apply(match, 1, new("start"));
            foreach (int player in match.Players.Keys)
            {
                Apply(match, player, new("build", 0, Building.Farm));
                Apply(match, player, new("build", 2, Building.MetalMine));
                Apply(match, player, new("build", 4, Building.MetalMine));
                Apply(match, player, new("build", 3, Building.Lumbermill, Payment: ConstructionPayment.GoldRecovery));
            }
            for (int preparation = 0; preparation < 3; preparation++)
                foreach (int player in match.Players.Keys)
                {
                    if (preparation == 2)
                    {
                        // Two actually paid mines earn twelve equipment metal
                        // before final END. Refund and earned timber fund the
                        // trainer; the stone producer waits for real next-wave
                        // stock, never final-income spending or a grant.
                        Apply(match, player, new("sell", 4));
                        Apply(match, player, new("build", 1, Building.Barracks));
                        for (int recruit = 0; recruit < 6; recruit++) Apply(match, player, new("recruit", 1));
                    }
                    Apply(match, player, new("ready"));
                }
            Assert.Equal(Phase.Combat, match.Phase);
            return match;
        }
        catch { match.Dispose(); throw; }
    }
    internal static void Clear(Match match)
    {
        for (int tick = 0; tick < 4000 && (match.Phase == Phase.Combat || match.Combat.HasDeaths); tick++) match.Step();
        RewardFixture.Resolve(match);
        Assert.Equal(Phase.Building, match.Phase); Assert.False(match.Combat.HasDeaths);
        if (match.Wave == 2)
            foreach (City city in match.Players.Values.Where(c => !c.Eliminated && c.Slots[4].Type == Building.Empty))
                Apply(match, city.Id, new("build", 4, Building.Stonecutter));
        Legal(match.Snapshot());
    }
    internal static Receipt Apply(Match match, int player, EconomyAction action)
    {
        MatchSnapshot before = match.Snapshot(); CityState city = before.Players.Single(c => c.Id == player);
        Command command = action.Command(before, player, before.Revision + 1);
        CommandResult accepted = match.Apply(player, command);
        Assert.True(accepted.Accepted, $"P{player} W{before.Wave} {before.Phase}/{before.Turn}: {action}: {accepted.Message}");
        MatchSnapshot after = match.Snapshot();
        AssertPaidTransition(before, after, player, action);
        CityState next = after.Players.Single(c => c.Id == player);
        return new(command with { MatchId = "session" }, city.Resources, next.Resources, city.Research, next.Research);
    }
    // Same quote/production/identity check for a real AuthoritySession request;
    // numerical ownership remains Match, no second economy or supervisor.
    internal static void AssertPaidTransition(MatchSnapshot before, MatchSnapshot after, int player, EconomyAction action)
    {
        CityState city = before.Players.Single(c => c.Id == player);
        if (action.Action == "start")
        {
            foreach (CityState owner in after.Players) Assert.Equal(RunPresentation.Starting(after.Rules, RunCatalog.Doctrine(owner.Run.Doctrine)), owner.Resources);
            return;
        }
        if (action.Action.StartsWith("reward:", StringComparison.Ordinal))
        {
            string id = action.Action[7..]; Assert.Contains(id, city.Run.Offers);
            Assert.Equal(city.Run.Stacks(id) + 1, after.Players.Single(c => c.Id == player).Run.Stacks(id));
            Assert.Equal(before.ProductionCount, after.ProductionCount); return;
        }
        ResourceCost cost = action.Action switch
        {
            "build" => action.Payment == ConstructionPayment.GoldRecovery
                ? before.BuildingCatalog.Single(b => b.Type == action.Building).RecoveryConstruction!.Value
                : before.BuildingCatalog.Single(b => b.Type == action.Building).Construction,
            "buy-home" => new(city.Army!.HomePrices[city.Army.PurchasedHomes - 2]),
            "buy-plot" => ArmyConfiguration.IsArmyPlot(action.Slot) ? new(city.Army!.HomePrices[city.Army.PurchasedHomes - 2])
                : new(before.PlotPrices[city.Slots.Count(s => s.Purchased) - 5]),
            "upgrade" => Plot(city, action.Slot).UpgradeQuote!.Value,
            "upgrade-tower" => before.BuildingCatalog.Single(b => b.Type == (action.Building == Building.RapidArrowTower
                ? Building.ArrowTower : Building.CatapultTower)).Upgrade,
            "upgrade-capacity" => city.Army!.Halls.Single(h => h.Slot == action.Slot).CapacityQuote!.Value,
            "upgrade-healing" => city.Army!.Halls.Single(h => h.Slot == action.Slot).HealingQuote!.Value,
            "recruit" => city.RecruitmentQuotes.Single(q => q.Type == action.Unit && q.Level == city.Slots[action.Slot].Level).Cost,
            "trade" => default(ResourceCost).With(action.Resource, checked(before.MarketRates.Single(r => r.Resource == action.Resource).Units * action.Bundles)),
            _ => default
        };
        ResourceCost income = action.Action switch
        {
            "sell" => Plot(city, action.Slot).Refund,
            "trade" => new(checked(before.MarketRates.Single(r => r.Resource == action.Resource).Gold * action.Bundles)),
            _ => default
        };
        CityState next = after.Players.Single(c => c.Id == player);
        foreach (CityState old in before.Players)
        {
            ResourceCost expected = old.Resources;
            if (old.Id == player)
            {
                Assert.True(expected.TryPay(cost, out expected)); Assert.True(expected.TryAdd(income, out expected));
            }
            if (after.ProductionCount != before.ProductionCount)
            { Assert.Equal(before.ProductionCount + 1, after.ProductionCount); Assert.True(expected.TryAdd(old.ProductionIncome!.Value, out expected)); }
            if (after.Phase == Phase.Combat && before.Phase == Phase.Preparation && !old.Eliminated)
            {
                CityState entered = after.Players.Single(c => c.Id == old.Id);
                Assert.Equal(old.FoodForecast!.Paid, entered.LastUpkeep!.Paid);
                Assert.Equal(old.FoodForecast.Funded, entered.LastUpkeep.Funded);
                Assert.Equal(old.FoodForecast.Participating, entered.LastUpkeep.Participating);
                Assert.True(expected.TryPay(new(Food: entered.LastUpkeep.Paid), out expected));
            }
            Assert.Equal(expected, after.Players.Single(c => c.Id == old.Id).Resources);
        }
        foreach (UnitState old in city.Soldiers.Where(u => next.Soldiers.Any(n => n.Id == u.Id)))
        {
            UnitState member = next.Soldiers.Single(u => u.Id == old.Id);
            Assert.Equal(old.Level, member.Level); Assert.Equal(old.Experience, member.Experience);
            if (after.ProductionCount == before.ProductionCount || old.Assignment is not { Stored: true } || !old.RecoveryEligible)
                Assert.Equal(old.Health, member.Health);
            else Assert.InRange(member.Health, old.Health, member.Profile.Health);
            if (action.Action != "research-tech") Assert.Equal(old.Profile, member.Profile);
        }
        if (action.Action == "research-tech")
        {
            Assert.True(city.Technologies.Single(t => t.Id == action.Technology).Available);
            Assert.Equal(city.Research.Points - before.TechnologyCatalog.Single(t => t.Id == action.Technology).Cost, next.Research.Points);
            Assert.True(next.Research.Has(action.Technology));
        }
        if (action.Action == "recruit")
        {
            UnitState recruit = Assert.Single(next.Soldiers, u => city.Soldiers.All(old => old.Id != u.Id));
            Assert.Equal(action.Unit, recruit.Type); Assert.Equal(recruit.Profile.Health, recruit.Health); Assert.Equal(0, recruit.Experience);
        }
        Legal(after);
    }
    private static SlotState Plot(CityState city, int slot) => ArmyConfiguration.IsArmyPlot(slot)
        ? city.Army!.Homes.Single(h => h.PlotId == slot).Plot : city.Slots[slot];
    internal static void Legal(MatchSnapshot state)
    {
        SnapshotCompatibility.RequireSharedArena(state);
        foreach (CityState city in state.Players)
        {
            ArmyState army = city.Army!;
            Assert.All(army.Homes, h => Assert.InRange(h.Used, 0, h.Purchased && h.Plot.Type == Building.Empty ? army.SquadCapacity : 0));
            foreach (var home in city.Soldiers.Where(u => u.Assignment is { Stored: false }).GroupBy(u => u.Assignment!.Tile))
            {
                Assert.Single(home.Select(u => u.Type).Distinct()); Assert.Single(home.Select(u => u.SquadId).Distinct());
                Assert.Equal(home.Count(), home.Select(u => u.FormationSlot).Distinct().Count());
            }
            Assert.All(army.Halls, h => Assert.InRange(city.Soldiers.Count(u => u.Assignment is { Stored: true } a && a.HallSlot == h.Slot && a.HallGeneration == h.Generation), 0, h.Capacity));
            Assert.All(city.Soldiers.Where(u => u.Assignment is { Stored: true }), u =>
            {
                Assert.False(u.Deployed); Assert.False(u.Participating); Assert.Equal(default, u.Hex!.Position);
                Assert.DoesNotContain(state.Reservations.Positions, p => p.UnitId == u.SquadId);
            });
        }
        CombatReservations rebuilt = CombatReservations.Reconstruct(state.Arena!, state.Squads);
        Assert.Equal(state.Reservations.Positions, rebuilt.Positions); Assert.Equal(state.Reservations.Transit, rebuilt.Transit);
    }
    internal static Result Run(ITestOutputHelper output, string family, int players, ulong seed, int maximumHomes = 6, int maximumTier = 5,
        bool hallUpgrades = true, int stopAfterWave = 20, Rules? rules = null)
    {
        using var match = new Match(rules, combatSeed: seed);
        for (int i = 0; i < players; i++) match.Join();
        var commands = new List<Receipt>(); var waves = new List<Wave>();
        int ticks = 0; long healed = 0; ResourceCost recovery = default;
        bool trace = Environment.GetEnvironmentVariable("ODOT_CAMPAIGN_TRACE") == "1";
        commands.Add(Apply(match, 1, new("start")));
        while (match.Phase is not (Phase.Victory or Phase.Defeat) && match.Wave <= stopAfterWave)
        {
            // Cleanup is finite and owns no production; investment must not race
            // restoration of persistent homes after cosmetic death expiry.
            if (match.Phase == Phase.RewardDraft)
                foreach (City city in match.Players.Values.Where(c => c.Connected && !c.Eliminated && c.Run.Choice is null))
                    commands.Add(Apply(match, city.Id, new("reward:" + CampaignStrategy.RewardChoice(city.Run))));
            for (int cleanup = 0; cleanup < 120 && match.Combat.HasDeaths; cleanup++) match.Step();
            Assert.False(match.Combat.HasDeaths);
            for (int preparation = 0; preparation < 3 && match.Phase is Phase.Building or Phase.Preparation; preparation++)
                foreach (City city in match.Players.Values.Where(c => !c.Eliminated))
                {
                    for (int actions = 0; ; actions++)
                    {
                        Assert.True(actions < 100, "Paid strategy did not converge.");
                        MatchSnapshot before = match.Snapshot();
                        string approach = family == "rescue" && city.Id == players ? "uninvested" : family == "rescue" ? "support" : family;
                        EconomyAction? plan = approach == "uninvested" ? null : CampaignStrategy.Next(before, city.Id, approach, maximumHomes, maximumTier, hallUpgrades);
                        if (plan is null) break;
                        Receipt receipt = Apply(match, city.Id, plan); commands.Add(receipt);
                        if (plan.Building == Building.TownHall || plan.Action is "upgrade-capacity" or "upgrade-healing")
                        {
                            ResourceCost paid = default;
                            foreach (Resource resource in Enum.GetValues<Resource>()) paid = paid.With(resource, Math.Max(0, receipt.Before.Amount(resource) - receipt.After.Amount(resource)));
                            Assert.True(recovery.TryAdd(paid, out recovery));
                        }
                    }
                    MatchSnapshot old = match.Snapshot(); commands.Add(Apply(match, city.Id, new("ready")));
                    foreach (CityState prior in old.Players)
                        foreach (UnitState member in prior.Soldiers)
                            healed += Math.Max(0, match.Players[prior.Id].Soldiers.Single(u => u.Id == member.Id).Health - member.Health);
                }
            Assert.Equal(Phase.Combat, match.Phase);
            MatchSnapshot entry = match.Snapshot(); long start = match.Tick, cursor = match.Combat.EventSequence;
            int casualties = 0, keep = 0, post = 0, foreign = 0;
            var events = new List<CombatEvent>();
            var travel = new List<Travel>(); var rescues = new List<Rescue>();
            HexBoard board = match.Combat.Board;
            var previous = entry.Squads.Where(s => s.Faction == Faction.Adventurers && s.Hex.Lifecycle == UnitLifecycle.Alive)
                .ToDictionary(s => s.Id, s => s.Hex.Position.Cell);
            while (match.Phase == Phase.Combat)
            {
                Assert.True(++ticks <= 100000, "Paid campaign exceeded its documented fixed-step bound.");
                match.Step();
                if (family == "rescue")
                    foreach (SquadState squad in match.Combat.SquadSnapshots().Where(s => s.Faction == Faction.Adventurers && s.Hex.Lifecycle == UnitLifecycle.Alive))
                    {
                        if (previous.TryGetValue(squad.Id, out int from) && from != squad.Hex.Position.Cell)
                        {
                            Assert.Contains(squad.Hex.Position.Cell, board.Cell(from).Neighbors);
                            travel.Add(new(match.Tick, squad.Id, squad.Owner, from, squad.Hex.Position.Cell));
                        }
                        previous[squad.Id] = squad.Hex.Position.Cell;
                    }
                foreach (CombatEvent e in match.Combat.Events().Where(e => e.Sequence > cursor))
                {
                    if (trace) events.Add(e);
                    if (e.Type == CombatEventType.Death && e.Unit?.Faction == Faction.Adventurers) casualties++;
                    if (e.Type is CombatEventType.StructureHit or CombatEventType.StructureDestroyed)
                    { if (e.Structure?.Kind == ArenaStructureKind.Keep) keep += e.Damage; if (e.Structure?.Kind == ArenaStructureKind.Watchpost) post += e.Damage; }
                    // Hit projects the damaged member; Impact projects the
                    // contributing attacker and its actual victim IDs.
                    if (e.Type == CombatEventType.Impact && e.Landed && e.Unit is { Faction: Faction.Adventurers } attacker
                        && entry.Enemies.Any(u => e.Victims.Contains(u.Id) && u.Origin != attacker.Owner))
                    {
                        foreign++;
                        if (family == "rescue")
                            foreach (int victim in e.Victims)
                            {
                                UnitState? original = entry.Enemies.FirstOrDefault(u => u.Id == victim && u.Origin != attacker.Owner);
                                if (original is null) continue;
                                UnitState enemy = match.Combat.Read(victim);
                                City threat = match.Players[enemy.Destination];
                                int sector = Array.FindIndex(entry.Arena!.Sectors, s => s.Owner == attacker.Owner);
                                bool crossed = board.Cell(attacker.Hex!.Position.Cell).Coordinate.Column / SharedArena.SectorWidth != sector;
                                if (crossed && enemy.Destination != attacker.Owner && !threat.Eliminated)
                                    rescues.Add(new(e.Sequence, e.Tick, attacker.Id, attacker.SquadId, attacker.Owner, victim,
                                        threat.Id, attacker.Hex.Position.Cell, threat.Health));
                            }
                    }
                }
                cursor = match.Combat.EventSequence;
                if (ticks % 60 == 0) Legal(match.Snapshot());
            }
            MatchSnapshot exit = match.Snapshot(); Legal(exit);
            foreach (UnitState member in exit.Players.SelectMany(c => c.Soldiers))
            {
                UnitState veteran = entry.Players.SelectMany(c => c.Soldiers).Single(u => u.Id == member.Id);
                Assert.Equal(veteran.Level, member.Level); Assert.Equal(veteran.Profile, member.Profile); Assert.True(member.Health <= veteran.Health);
                Assert.Equal(veteran.SquadId, member.SquadId); Assert.Equal(veteran.FormationSlot, member.FormationSlot); Assert.Equal(veteran.Assignment, member.Assignment);
                Assert.Equal(veteran.Experience + (match.Phase != Phase.Defeat && veteran.Participating ? 1 : 0), member.Experience);
            }
            int injured = exit.Players.SelectMany(c => c.Soldiers).Count(u => u.Health < u.Profile.Health);
            waves.Add(new(entry.Wave, checked((int)(match.Tick - start)), keep, post, casualties, injured,
                entry.Players.Sum(c => c.Soldiers.Sum(u => u.Health)) - exit.Players.Sum(c => c.Soldiers.Sum(u => u.Health)),
                entry.Players.Where(c => c.LastUpkeep?.Wave == entry.Wave).Sum(c => c.LastUpkeep!.Unfed.Length),
                entry.Players.Where(c => c.LastUpkeep?.Wave == entry.Wave).Sum(c => c.LastUpkeep!.Paid), foreign, entry.Players, exit.Players)
            { EnemyEntry = trace ? entry.Enemies : [], Events = events.ToArray(), Travel = travel.ToArray(), Rescues = rescues.ToArray() });
            if (trace)
            {
                Dictionary<int, int> origins = entry.Enemies.ToDictionary(u => u.Id, u => u.Origin);
                int independentlyCounted = events.Count(e => e.Type == CombatEventType.Impact && e.Landed
                    && e.Unit is { Faction: Faction.Adventurers } u && e.Victims.Any(id => origins.TryGetValue(id, out int origin) && origin != u.Owner));
                Assert.Equal(foreign, independentlyCounted);
            }
            output.WriteLine($"PROVISIONAL {family}/{players}/seed={seed} W{entry.Wave}: ticks={match.Tick - start}, Keep={keep}, Watchpost={post}, casualties={casualties}, injured={injured}, foreign-origin member impacts={foreign}, result={match.Phase}/{match.DefeatReason}");
        }
        Result result = new(family, players, seed, maximumHomes, maximumTier, hallUpgrades, match.Phase, match.DefeatReason, match.Stall,
            match.Wave, match.ProductionCount, ticks, healed, recovery, commands.ToArray(), waves.ToArray(), match.Snapshot() with { MatchId = "session" })
        { RequestedHorizon = stopAfterWave };
        string json = JsonSerializer.Serialize(result, WireJson.Options);
        string? directory = Environment.GetEnvironmentVariable("ODOT_CAMPAIGN_EVIDENCE_DIRECTORY");
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, $"{family}-{players}-{seed}-homes{maximumHomes}-tier{maximumTier}-hall{hallUpgrades}-horizon{stopAfterWave}-enemy{match.Rules.EnemyPowerPercent}.json"), json);
        }
        output.WriteLine(json);
        return result;
    }
}
