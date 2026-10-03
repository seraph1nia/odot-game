using DevRunner;
using Game.Core;
using Xunit;
using Xunit.Abstractions;

namespace DevRunner.Tests;

public sealed class MeleeVisualProofTests(ITestOutputHelper output)
{
    private static readonly HexBoard Board = new(HexBoardDefinition.Default());
    private static UnitObservation Observe(UnitState unit, long tick)
    {
        HexUnitState hex = unit.Hex!;
        HexCoordinate cell = hex.Lifecycle == UnitLifecycle.Queued ? default : Board.Cell(hex.Position.Cell).Coordinate;
        HexAnchor? anchor = hex.Lifecycle == UnitLifecycle.Queued ? null : Board.Anchor(hex.Position.Anchor);
        return new()
        {
            Id = unit.Id,
            Type = unit.Type,
            Faction = unit.Faction,
            Class = unit.Class,
            Destination = unit.Destination,
            Deployed = unit.Deployed,
            Visible = unit.Deployed,
            Dead = hex.Lifecycle == UnitLifecycle.Dying,
            Hex = hex,
            X = cell.Column * 3 + (cell.R & 1) * 1.5f + (anchor?.AnchorX ?? 0) / 1000f,
            Z = cell.R * 2.598076211f + (anchor?.AnchorForward ?? 0) / 1000f,
            AttackSequence = unit.AttackSequence,
            ActionStartTick = unit.ActionStartTick,
            ImpactTick = unit.ImpactTick,
            AttackActive = unit.AttackSequence > 0 && tick >= unit.ActionStartTick && tick < unit.ReadyTick,
            WeaponAttached = true,
            BoneRotation = "fixture",
        };
    }
    // This checks the ordinary setup's action opportunities and the detector;
    // it deliberately cannot establish imported rigs, PNGs or rendered clearance.
    private static UiObservation Frame(MatchSnapshot state)
    {
        UnitState[] actors = state.Players.Single(c => c.Id == 1).Soldiers.Concat(state.Enemies.Where(u => u.Destination == 1))
            .Concat(state.DyingBodies.Where(u => u.Destination == 1)).ToArray();
        UnitObservation[] units = actors.Select(u => Observe(u, state.Tick)).ToArray();
        StrikeObservation[] strikes = actors.Where(u => u.Class == UnitClass.Melee && u.Hex is { Lifecycle: UnitLifecycle.Alive, Action: UnitActionKind.Windup or UnitActionKind.Recovery }
            && !u.TargetCity && u.AttackSequence > 0 && state.Tick >= u.ActionStartTick && state.Tick < u.ImpactTick + 12)
            .Where(u => units.Any(t => t.Id == u.TargetId)).Select(u =>
            {
                UnitObservation target = units.Single(t => t.Id == u.TargetId);
                return new StrikeObservation
                {
                    Id = u.Id,
                    TargetId = u.TargetId,
                    AttackSequence = u.AttackSequence,
                    ImpactTick = u.ImpactTick,
                    AttackLanded = u.AttackLanded,
                    Visible = true,
                    ImpactVisible = u.AttackLanded == true && state.Tick >= u.ImpactTick,
                    TargetX = target.X,
                    TargetZ = target.Z
                };
            }).ToArray();
        return new() { Units = units, Strikes = strikes, CombatTick = state.Tick };
    }
    private static void Act(Match match, int city, string action, int slot = -1, Building building = Building.Empty)
        => Assert.True(match.Apply(city, new(1, match.Id, match.Phase, match.TurnSerial, action, city, slot, building, ExpectedGeneration: slot is >= 0 and < 9 ? match.Players[city].Slots[slot].Generation : 0)).Accepted);
    [Fact]
    public void AVisibleRetainedDeathStillRequiresRenderedBodyClearance()
    {
        var living = new UnitObservation { Id = 1, Destination = 1, Visible = true, Deployed = true };
        var dying = new UnitObservation { Id = 2, Destination = 1, Visible = true, Deployed = true, Dead = true, X = .2f };
        Assert.Throws<InvalidOperationException>(() => Runner.RenderedContact(new() { Units = [living, dying] }));
        Runner.RenderedContact(new() { Units = [living, dying with { Visible = false }] });
        Runner.RenderedContact(new() { Units = [living, dying with { X = .5f }] });
    }
    [Fact]
    public void AnUnlinkedCueCannotProveAnAttackOrASimultaneousExchange()
    {
        UnitObservation Unit(int id, Faction faction, int cell, int anchor) => Observe(new UnitState(id, 1000, 0, 1, 1)
        {
            Faction = faction,
            Type = UnitType.Swordsman,
            Deployed = true,
            AttackSequence = 1,
            ActionStartTick = 0,
            ImpactTick = 12,
            ReadyTick = 60,
            Hex = new(id, 1, faction, UnitLifecycle.Alive, new(cell, anchor), UnitActionKind.Windup, StartTick: 0, EndTick: 60)
        }, 6);
        UnitObservation actor = Unit(1, Faction.Adventurers, 14, 1), ally = Unit(2, Faction.Adventurers, 14, 3), enemy = Unit(3, Faction.Skeletons, 11, 1), opponent = Unit(4, Faction.Skeletons, 11, 3);
        var frame = new UiObservation
        {
            Units = [actor, ally, enemy, opponent],
            CombatTick = 6,
            Strikes = [new() { Id = 1, TargetId = 3, AttackSequence = 1, ImpactTick = 12, Visible = true, TargetX = enemy.X, TargetZ = enemy.Z },
                new() { Id = 3, TargetId = 1, AttackSequence = 1, ImpactTick = 12, Visible = true, TargetX = actor.X + .1f, TargetZ = actor.Z }]
        };
        MeleeWitness[] found = MeleeVisualProof.Inspect(frame, Board);
        Assert.NotEqual(MeleeCoverage.None, found.Single(w => w.Actor == 1).Coverage & MeleeCoverage.Windup);
        Assert.Equal(MeleeCoverage.None, found.Single(w => w.Actor == 3).Coverage & (MeleeCoverage.Windup | MeleeCoverage.Impact));
        Assert.All(found, w => Assert.Equal(MeleeCoverage.None, w.Coverage & MeleeCoverage.Simultaneous));
    }
    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    [InlineData(123UL)]
    public void OrdinarySixSwordOpeningGeneratesTheRequiredCheckpointOpportunities(ulong seed)
    {
        using var match = new Match(combatSeed: seed); match.Join(); match.Join(); Act(match, 1, "start");
        Act(match, 1, "build", 0, Building.Farm); Act(match, 1, "build", 2, Building.MetalMine); Act(match, 1, "build", 1, Building.Barracks);
        if (seed == 0)
        {
            Act(match, 2, "build", 0, Building.Farm); Act(match, 2, "build", 2, Building.MetalMine); Act(match, 2, "build", 1, Building.Barracks);
        }
        for (int production = 1; production <= 3; production++)
        {
            Act(match, 1, "ready"); Act(match, 2, "ready");
            while (match.Players[1].Soldiers.Count < 6 && match.Players[1].Resources.TryPay(match.Economy.Recruitment(UnitType.Swordsman), out _)) Act(match, 1, "recruit", 1);
            if (seed == 0) while (match.Players[2].Soldiers.Count < 6 && match.Players[2].Resources.TryPay(match.Economy.Recruitment(UnitType.Swordsman), out _)) Act(match, 2, "recruit", 1);
        }
        Assert.Equal(6, match.Players[1].Soldiers.Count); Assert.Equal(15, match.Players[1].Food); Assert.Equal(0, match.Players[1].Wood);
        Act(match, 1, "ready"); Act(match, 2, "ready");
        MeleeCoverage coverage = MeleeCoverage.None; var opportunities = new HashSet<(MeleeCoverage Phase, MeleeCoverage Side)>(); bool simultaneousCapture = false;
        MeleeCoverage capturedSide = MeleeCoverage.None; long windupTick = -1, impactTick = -1;
        for (int step = 1; step <= 3600 && match.Phase is Phase.Combat or Phase.Building or Phase.Preparation; step++)
        {
            if (match.Phase is Phase.Building or Phase.Preparation)
            {
                foreach (City city in match.Players.Values.Where(c => !c.Eliminated))
                {
                    if (city.Id == 1 || seed == 0) while (city.Soldiers.Count < 6 && city.Resources.TryPay(match.Economy.Recruitment(UnitType.Swordsman), out _)) Act(match, city.Id, "recruit", 1);
                    if (!city.Ready) Act(match, city.Id, "ready");
                }
                match.Step(); continue;
            }
            match.Step(); UiObservation frame = Frame(match.Snapshot());
            long tick = match.Tick;
            foreach (MeleeWitness witness in MeleeVisualProof.Inspect(frame, Board))
            {
                coverage |= witness.Coverage;
                if ((witness.Coverage & MeleeCoverage.Shared) == 0) continue;
                simultaneousCapture |= (witness.Coverage & (MeleeCoverage.Windup | MeleeCoverage.Simultaneous)) == (MeleeCoverage.Windup | MeleeCoverage.Simultaneous) && witness.ImpactTick - tick >= 6;
                if (capturedSide == MeleeCoverage.None && (witness.Coverage & (MeleeCoverage.Windup | MeleeCoverage.Simultaneous)) == (MeleeCoverage.Windup | MeleeCoverage.Simultaneous)
                    && witness.ImpactTick - tick >= 6 && (witness.Coverage & (MeleeCoverage.Near | MeleeCoverage.Far)) != 0)
                {
                    capturedSide = (witness.Coverage & MeleeCoverage.Near) != 0 ? MeleeCoverage.Near : MeleeCoverage.Far; windupTick = tick;
                }
                MeleeCoverage opposite = capturedSide == MeleeCoverage.Near ? MeleeCoverage.Far : MeleeCoverage.Near;
                if (windupTick >= 0 && tick > windupTick && impactTick < 0 && (witness.Coverage & (MeleeCoverage.Impact | opposite)) == (MeleeCoverage.Impact | opposite)) impactTick = tick;
                foreach (MeleeCoverage side in new[] { MeleeCoverage.Near, MeleeCoverage.Far })
                {
                    if ((witness.Coverage & side) == 0) continue;
                    if ((witness.Coverage & MeleeCoverage.Windup) != 0 && witness.ImpactTick - tick >= 6) opportunities.Add((MeleeCoverage.Windup, side));
                    if ((witness.Coverage & MeleeCoverage.Impact) != 0) opportunities.Add((MeleeCoverage.Impact, side));
                }
            }
            if (opportunities.Count == 4 && impactTick >= 0) break;
        }
        output.WriteLine($"Proof wave={match.Wave}, phase={match.Phase}, tick={match.Tick}; ordered capture windup={windupTick} ({capturedSide}), opposite impact={impactTick}.");
        output.WriteLine($"Melee setup seed={seed}: coverage={coverage}; capture opportunities={string.Join(';', opportunities)}.");
        const MeleeCoverage required = MeleeCoverage.Shared | MeleeCoverage.Near | MeleeCoverage.Far | MeleeCoverage.Simultaneous | MeleeCoverage.Windup | MeleeCoverage.Impact;
        Assert.Equal(required, coverage & required);
        Assert.True(simultaneousCapture, "The shared simultaneous windup has no capture allowance.");
        Assert.True(impactTick > windupTick && windupTick >= 0, "The first shared simultaneous windup needs a later opposite-side impact.");
        // The required graphical seed-1 proof stays within two waves. The
        // extra seed-0 detector sample reaches its opposite-side impact in wave three.
        Assert.InRange(match.Wave, 1, seed == 1 ? 2 : 3);
        Assert.Contains((MeleeCoverage.Windup, MeleeCoverage.Near), opportunities); Assert.Contains((MeleeCoverage.Windup, MeleeCoverage.Far), opportunities);
        Assert.Contains((MeleeCoverage.Impact, MeleeCoverage.Near), opportunities); Assert.Contains((MeleeCoverage.Impact, MeleeCoverage.Far), opportunities);
    }
}
