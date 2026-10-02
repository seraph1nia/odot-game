using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    private async Task MeleeArmy(Child client, Child observer, CancellationToken token)
    {
        await Pick(client, 0, token); await ClickAck(client, "Farm", token);
        await Pick(client, 2, token); await ClickAck(client, "MetalMine", token);
        await Pick(client, 1, token); await ClickAck(client, "Barracks", token);
        for (int production = 1; production <= 3; production++)
        {
            await Action(client, "ready", token); MatchSnapshot resolved = State(await Action(observer, "ready", token));
            await Observe(client, s => s.Revision >= resolved.Revision && s.ProductionCount == production, "melee production synchronization", token);
            await Pick(client, 1, token);
            ResourceCost cost = resolved.Players.Single(c => c.Id == client.PlayerId).RecruitmentQuotes.Single(q => q.Type == UnitType.Swordsman && q.Level == 1).Cost;
            while (Latest(client).Players.Single(c => c.Id == client.PlayerId).Soldiers.Length < 6 && Latest(client).Players.Single(c => c.Id == client.PlayerId).Resources.TryPay(cost, out _)) await ClickAck(client, "Recruit", token);
        }
        CityState army = Latest(client).Players.Single(c => c.Id == client.PlayerId);
        Require(army.Soldiers.Length == 6 && army.Soldiers.All(u => u.Type == UnitType.Swordsman)
            && army.Food == 15 && army.Wood == 0, "ordinary Farm/Metal Mine recruitment equips six frontline units without food spending or grants");
        await Action(client, "ready", token); MatchSnapshot battle = State(await Action(observer, "ready", token));
        await Observe(client, s => s.Phase == Phase.Combat && s.Revision >= battle.Revision, "melee first wave", token);
        Require(Latest(client).CombatSeed == battle.CombatSeed && Latest(client).ConfigurationFingerprint == battle.ConfigurationFingerprint,
            "cooperative clients agree on the generated combat seed and configuration");
    }
    private async Task MeleeCheckpoint(Child client, Child observer, CancellationToken token)
    {
        var board = new HexBoard(Latest(client).Rules.Combat.Board);
        MeleeCoverage covered = MeleeCoverage.None;
        var witnesses = new List<MeleeWitness>();
        var captures = new HashSet<MeleeCoverage>();
        MeleeCoverage firstSide = MeleeCoverage.None;
        const MeleeCoverage required = MeleeCoverage.Shared | MeleeCoverage.Near | MeleeCoverage.Far | MeleeCoverage.Simultaneous | MeleeCoverage.Windup | MeleeCoverage.Impact;
        // Size-two support and fixed anchors change seeded opportunities. The
        // two-wave proof includes four captures and ordinary intervening turns;
        // the measured former 40s allowance expired after its first capture.
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token); deadline.CancelAfter(65000);
        try
        {
            while ((covered & required) != required || captures.Count < 2)
            {
                MeleeCoverage phase = !captures.Contains(MeleeCoverage.Windup) ? MeleeCoverage.Windup : MeleeCoverage.Impact;
                UiObservation initial = await UiProtocol.Probe(client, options.StartupTimeout, deadline.Token);
                RenderedContact(initial); HealthBars(initial);
                MatchSnapshot state = Latest(client);
                if (state.Phase == Phase.Combat)
                {
                    long revision = Latest(observer).Revision;
                    using var milestoneCancellation = CancellationTokenSource.CreateLinkedTokenSource(deadline.Token);
                    async Task<MatchSnapshot> PauseMilestone()
                    {
                        MatchSnapshot milestone = await Observe(observer, s => s.Revision > revision && !s.Paused
                            && (s.Phase != Phase.Combat || MeleeVisualProof.HasMilestone(s, phase, board)), "ordinary melee action milestone", milestoneCancellation.Token);
                        return milestone.Phase == Phase.Combat ? State(await Action(observer, "pause", milestoneCancellation.Token)) : milestone;
                    }
                    Task<MatchSnapshot> pause = PauseMilestone();
                    try
                    {
                        while (!pause.IsCompleted)
                        {
                            UiObservation moving = await UiProtocol.Probe(client, options.StartupTimeout, deadline.Token);
                            RenderedContact(moving); HealthBars(moving);
                            foreach (MeleeWitness witness in MeleeVisualProof.Inspect(moving, board)) { covered |= witness.Coverage; witnesses.Add(witness); }
                        }
                        state = await pause;
                    }
                    catch
                    {
                        milestoneCancellation.Cancel();
                        try { await pause; } catch (Exception) { }
                        throw;
                    }
                }
                Require(state.Phase is Phase.Building or Phase.Preparation or Phase.Combat && state.Wave <= 2,
                    "ordinary melee proof completes before wave three or a terminal result");
                if (state.Phase is Phase.Building or Phase.Preparation)
                {
                    // The observer can see a clear before the graphical peer's
                    // current-state message arrives. Do not issue next-stage
                    // requests using that peer's former combat/turn identity.
                    await Observe(client, s => s.Revision >= state.Revision && s.Phase == state.Phase && s.TurnSerial == state.TurnSerial,
                        "melee transition synchronization", deadline.Token);
                    CityState city = state.Players.Single(c => c.Id == client.PlayerId);
                    if (!city.Ready)
                    {
                        if (state.Phase == Phase.Building)
                        {
                            await Pick(client, 1, deadline.Token);
                            ResourceCost cost = city.RecruitmentQuotes.Single(q => q.Type == UnitType.Swordsman && q.Level == city.Slots[1].Level).Cost;
                            while (Latest(client).Players.Single(c => c.Id == client.PlayerId).Soldiers.Length < 6 && Latest(client).Players.Single(c => c.Id == client.PlayerId).Resources.TryPay(cost, out _)) await ClickAck(client, "Recruit", deadline.Token);
                        }
                        await Action(client, "ready", deadline.Token);
                    }
                    CityState other = Latest(client).Players.Single(c => c.Id == observer.PlayerId);
                    if (!other.Eliminated && !other.Ready) await Action(observer, "ready", deadline.Token);
                    continue;
                }
                UiObservation frozen = await WaitUi(client, p => p.PhaseText.Contains("PAUSED", StringComparison.Ordinal) && p.CombatTick == state.Tick,
                    "melee capture pause", deadline.Token);
                RenderedContact(frozen); HealthBars(frozen);
                MeleeWitness[] current = MeleeVisualProof.Inspect(frozen, board);
                foreach (MeleeWitness witness in current) { covered |= witness.Coverage; witnesses.Add(witness); }
                MeleeCoverage side = firstSide == MeleeCoverage.Near ? MeleeCoverage.Far : MeleeCoverage.Near;
                MeleeCoverage needed = MeleeCoverage.Shared | phase | (phase == MeleeCoverage.Windup ? MeleeCoverage.Simultaneous : MeleeCoverage.None);
                MeleeWitness? candidate = current.FirstOrDefault(w => (w.Coverage & needed) == needed
                    && (phase == MeleeCoverage.Windup ? (w.Coverage & (MeleeCoverage.Near | MeleeCoverage.Far)) != 0 : (w.Coverage & side) != 0)
                    );
                MeleeWitness? paused = candidate;
                if (paused is not null)
                {
                    await Observe(observer, s => s.Paused && s.Tick == (long)frozen.CombatTick, "observer sees melee capture pause", deadline.Token);
                    MeleeCoverage capturedSide = (paused.Coverage & MeleeCoverage.Near) != 0 ? MeleeCoverage.Near : MeleeCoverage.Far;
                    string label = (phase == MeleeCoverage.Windup ? "windup-" : "impact-") + capturedSide.ToString().ToLowerInvariant();
                    await Click(client, "ResetView", deadline.Token); RequireOverview(await UiProtocol.Probe(client, options.StartupTimeout, deadline.Token));
                    await Checkpoint(client, "combat-melee-" + label + "-overview", deadline.Token);
                    UiObservation overview = await UiProtocol.Probe(client, options.StartupTimeout, deadline.Token);
                    StrikeObservation strike = overview.Strikes.Single(s => s.Id == paused.Actor && s.AttackSequence == paused.Sequence);
                    float x = (strike.SourceScreenX + strike.TargetScreenX) / 2, y = (strike.SourceScreenY + strike.TargetScreenY) / 2;
                    Require(x > 0 && x < overview.Width && y > 0 && y < overview.HudTop, "melee pair lies in the world viewport");
                    await FocusWorld(client, deadline.Token);
                    for (int n = 0; n < 20; n++) await Wheel(client, x, y, true);
                    UiObservation close = await WaitUi(client, p => Math.Abs(p.Camera.Zoom - 3) < .001, "melee close view", deadline.Token);
                    StrikeObservation closeStrike = close.Strikes.Single(s => s.Id == paused.Actor && s.AttackSequence == paused.Sequence);
                    Require(closeStrike.SourceScreenX > 0 && closeStrike.SourceScreenX < close.Width && closeStrike.TargetScreenX > 0 && closeStrike.TargetScreenX < close.Width
                        && closeStrike.SourceScreenY > 0 && closeStrike.SourceScreenY < close.HudTop && closeStrike.TargetScreenY > 0 && closeStrike.TargetScreenY < close.HudTop,
                        "both linked melee endpoints remain visible in the close view");
                    Require(frozen.CombatTick == close.CombatTick && JsonSerializer.Serialize(frozen.Units) == JsonSerializer.Serialize(close.Units),
                        "camera inspection does not change paused anchors, poses or actions");
                    await Checkpoint(client, "combat-melee-" + label + "-close", deadline.Token);
                    await Click(client, "ResetView", deadline.Token); captures.Add(phase);
                    if (phase == MeleeCoverage.Windup) firstSide = capturedSide;
                }
                await Action(observer, "resume", deadline.Token);
            }
            Console.WriteLine($"MELEE GATE: {covered}; {witnesses.Count} live-node witnesses; four overview/close windup/impact frames.");
        }
        finally
        {
            await File.WriteAllTextAsync(Path.Combine(_scope!.EvidenceDirectory, "combat-melee-witnesses.json"),
                JsonSerializer.Serialize(new
                {
                    Latest(client).CombatSeed,
                    Latest(client).ConfigurationFingerprint,
                    Coverage = covered.ToString(),
                    Complete = (covered & required) == required && captures.Count == 2,
                    Captures = captures.Select(c => c.ToString()).ToArray(),
                    Witnesses = witnesses
                }, Evidence.JsonOptions), CancellationToken.None);
        }
    }
}
