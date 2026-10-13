namespace Game.Core;

// A deliberate refusal, not migration by inventing squads from old soldier
// fields. Reconnect credentials are separate; this validates combat projections.
public static class SnapshotCompatibility
{
    public const int CurrentSquadRulesVersion = 7;
    public static void RequireSharedArena(MatchSnapshot state)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (state.BattleFlowVersion != MatchSnapshot.CurrentBattleFlowVersion || !Enum.IsDefined(state.CombatSpeed)
            || state.Phase == Phase.Building && state.Turn is not (1 or 2)
            || state.Phase == Phase.Preparation && state.Turn != 3 || state.NextWave is null
            || state.NextWave.Any(u => u is null || u.Id >= 0 || u.Faction != Faction.Skeletons || u.Health != u.Profile.Health)
            || state.NextWave.Select(u => u.Id).Distinct().Count() != state.NextWave.Length)
            throw new ArgumentException("Legacy battle-flow snapshot refused: three preparations and authoritative wave forecasts are required. Rejoin a compatible authority or start a fresh match.", nameof(state));
        if (state.CombatRulesVersion != CurrentSquadRulesVersion || state.Rules?.Combat is null || state.Rules.Arena is null
            || state.Rules.Combat.RulesVersion != state.CombatRulesVersion || state.Rules.Combat.Board is not { Capacity: 1, Id: "shared-watch-arena" }
            || state.Arena is null || state.Arena.Board is not { Capacity: 1 } || state.Squads is null || state.Structures is null)
            throw new ArgumentException("Legacy combat snapshot refused: this client requires shared arena squad rules. Rejoin a compatible authority or start a fresh match.", nameof(state));
        if (state.Players is null || state.Enemies is null || state.DyingBodies is null || state.Players.Length > 4
            || state.Players.Any(p => p is null || p.Soldiers is null) || state.Enemies.Any(m => m is null)
            || state.Players.SelectMany(p => p.Soldiers).Any(m => m is null)
            || state.DyingBodies.Any(m => m is null) || state.Structures.Any(s => s is null)
            || state.CombatEvents is null || state.CombatEvents.Any(e => e is null || e.Victims is null || e.Periodic is null || e.Periodic.Any(p => p is null))
            || state.Squads.Any(s => s is null || s.Hex is null || s.Members is null || s.ParticipatingMembers is null || s.Decision is null)
            || state.Arena.Board.Cells is null || state.Arena.Footprints is null || state.Arena.Sectors is null
            || state.Arena.Board.Cells.Any(c => c is null) || state.Arena.Footprints.Any(f => f is null)
            || state.Arena.Sectors.Any(s => s is null || s.DeploymentCells is null || s.EnemyCells is null)
            || state.Players.Any(p => p.Id <= 0) || state.Players.Select(p => p.Id).Distinct().Count() != state.Players.Length
            || state.Players.Length == 0 && state.Phase != Phase.Lobby || state.Rules.Arena.SquadCapacity is < 1 or > 12)
            throw new ArgumentException("Malformed shared arena roster.", nameof(state));
        var configuration = new CombatConfiguration(state.Rules);
        if (RulesIdentity.Combine(configuration.Fingerprint, new EconomyConfiguration(state.Rules).Fingerprint,
            new CampaignConfiguration(state.Rules.Campaign, configuration).Fingerprint) != state.ConfigurationFingerprint)
            throw new ArgumentException("Combat configuration identity refused: received rules do not match the full authoritative fingerprint.", nameof(state));
        if (state.NextWave.Any(u => u.Profile.Health <= 0 || u.Statuses is not { Poison: not null } || u.Statuses.Poison.Any(p => p is null)
            || u.Profile != configuration.Unit(u.Type, u.Rank, u.IsBoss, u.Level).Runtime()))
            throw new ArgumentException("Authoritative wave forecast profile refused: received actors do not match configured identities.", nameof(state));
        var arena = new SharedArena(state.Players.Length == 0 ? [1] : state.Players.Select(p => p.Id),
            state.Arena.Footprints.Where(f => ArmyConfiguration.IsArmyPlot(f.Slot)).Select(f => (f.Owner, f.Slot)));
        ArenaState topology = arena.Snapshot();
        HexBoardDefinition expected = topology.Board, received = state.Arena.Board;
        if (received.Id != expected.Id || received.Version != expected.Version || received.Capacity != expected.Capacity
            || received.Cells.Length != expected.Cells.Length || received.Anchors is null || received.Entries is null || received.SiegeCells is null
            || !received.Anchors.SequenceEqual(expected.Anchors) || !received.SiegeCells.SequenceEqual(expected.SiegeCells)
            || received.Entries.Length != expected.Entries.Length
            || received.Entries.Where((e, i) => e is null || e.Faction != expected.Entries[i].Faction || e.RearCells is null || e.FrontCells is null
                || !e.RearCells.SequenceEqual(expected.Entries[i].RearCells) || !e.FrontCells.SequenceEqual(expected.Entries[i].FrontCells)).Any()
            || received.Cells.Where((c, i) => c.Id != expected.Cells[i].Id || c.Coordinate != expected.Cells[i].Coordinate
                || c.Affinity != expected.Cells[i].Affinity || c.Neighbors is null || !c.Neighbors.SequenceEqual(expected.Cells[i].Neighbors)).Any()
            || !state.Arena.Footprints.SequenceEqual(arena.Footprints)
            || state.Arena.Sectors.Length != topology.Sectors.Length
            || state.Arena.Sectors.Where((s, i) => s.Owner != topology.Sectors[i].Owner
                || !s.DeploymentCells.SequenceEqual(topology.Sectors[i].DeploymentCells)
                || !s.EnemyCells.SequenceEqual(topology.Sectors[i].EnemyCells)).Any())
            throw new ArgumentException("Malformed shared arena topology or footprints.", nameof(state));
        CombatReservations claims = CombatReservations.Reconstruct(arena, state.Squads);
        if (state.Reservations is null || state.Reservations.Positions is null || state.Reservations.Transit is null
            || !state.Reservations.Positions.SequenceEqual(claims.Positions) || !state.Reservations.Transit.SequenceEqual(claims.Transit))
            throw new ArgumentException("Shared arena reservation projection mismatch.", nameof(state));
        UnitState[] members = state.Players.SelectMany(p => p.Soldiers).Concat(state.Enemies).ToArray();
        foreach (UnitState member in members.Concat(state.DyingBodies).Concat(state.CombatEvents.Where(e => e.Unit is not null).Select(e => e.Unit!)))
            RequireProjection(member, arena.Board, configuration);
        foreach (UnitState body in state.DyingBodies)
            if (body.Health != 0 || body.Hex!.Lifecycle != UnitLifecycle.Dying)
                throw new ArgumentException("Invalid retained casualty.", nameof(state));
        foreach (CombatEvent entry in state.CombatEvents)
        {
            if (entry.ImpactPose is { } pose) RequirePose(pose, arena.Board);
            if (entry.Structure is { } structure && !arena.Board.Cells.Any(c => c.Id == structure.Cell))
                throw new ArgumentException("Invalid historical structure pose.", nameof(state));
        }
        if (state.Structures.Any(s => !arena.Footprints.Any(f => f.Owner == s.Owner && f.Slot == s.Slot && f.Cell == s.Cell
            && (f.Kind == s.Kind || f.Kind == ArenaStructureKind.ArrowTower && s.Kind == ArenaStructureKind.CatapultTower))))
            throw new ArgumentException("Invalid structure footprint.", nameof(state));
        if (members.Concat(state.DyingBodies).Select(m => m.Id).Distinct().Count() != members.Length + state.DyingBodies.Length
            || state.Squads.Select(s => s.Id).Distinct().Count() != state.Squads.Length)
            throw new ArgumentException("Duplicate member or squad identity.", nameof(state));
        var byId = members.ToDictionary(m => m.Id);
        foreach (SquadState squad in state.Squads)
        {
            if (squad.Id <= 0 || squad.Hex.Id != squad.Id || squad.Members.Length > (squad.IsBoss ? 1 : state.Rules.Arena.SquadCapacity)
                || squad.Members.Distinct().Count() != squad.Members.Length || squad.ParticipatingMembers.Distinct().Count() != squad.ParticipatingMembers.Length
                || squad.ParticipatingMembers.Any(id => !squad.Members.Contains(id))
                || (squad.ParticipatingMembers.Length > 0) != (squad.Hex.Lifecycle == UnitLifecycle.Alive)
                || squad.Decision.Route is null || squad.Decision.Visited is null || !Enum.IsDefined(squad.TargetKind)
                || squad.AttackSequence < 0 || squad.ReadyTick < 0 || squad.ImpactTick < 0
                || squad.Hex.Action == UnitActionKind.Windup && (squad.TargetId <= 0 || squad.TargetKind == SquadTargetKind.None
                    || squad.AttackSequence <= 0 || squad.Hex.StartTick < 0 || squad.ImpactTick <= squad.Hex.StartTick
                    || squad.ReadyTick <= squad.ImpactTick || squad.Hex.EndTick != squad.ReadyTick))
                throw new ArgumentException("Invalid squad population or actor identity.", nameof(state));
            foreach (int id in squad.Members)
                if (!byId.TryGetValue(id, out UnitState? member) || member.SquadId != squad.Id || member.Type != squad.Type || member.Faction != squad.Faction
                    || member.Owner != squad.Owner || member.IsBoss != squad.IsBoss || member.Experience < 0 || member.Health <= 0 || member.Health > member.Profile.Health
                    || member.FormationSlot < 0 || member.FormationSlot >= (squad.IsBoss ? 1 : state.Rules.Arena.SquadCapacity))
                    throw new ArgumentException("Invalid persistent squad member.", nameof(state));
                else if ((member.Hex! with { Id = squad.Id, City = 1, Lifecycle = squad.Hex.Lifecycle, FrozenAim = squad.Hex.FrozenAim }) != squad.Hex
                    || member.AttackSequence != squad.AttackSequence || member.ImpactTick != squad.ImpactTick || member.ReadyTick != squad.ReadyTick
                    || member.Hex.Lifecycle == UnitLifecycle.Dying
                    || member.Hex.Lifecycle == UnitLifecycle.Reserve && squad.Hex.Lifecycle is not (UnitLifecycle.Reserve or UnitLifecycle.Alive)
                    || member.Hex.Lifecycle != UnitLifecycle.Reserve && member.Hex.Lifecycle != squad.Hex.Lifecycle
                    || squad.ParticipatingMembers.Contains(id) != (member.Hex.Lifecycle == UnitLifecycle.Alive))
                    throw new ArgumentException("Member projection disagrees with its squad actor.", nameof(state));
            if (squad.Members.Select(id => byId[id].FormationSlot).Distinct().Count() != squad.Members.Length)
                throw new ArgumentException("Squad members have overlapping formation slots.", nameof(state));
        }
        if (members.Any(m => !state.Squads.Any(s => s.Id == m.SquadId && s.Members.Contains(m.Id))))
            throw new ArgumentException("A persistent member has no squad identity.", nameof(state));
    }
    private static void RequireProjection(UnitState member, HexBoard board, CombatConfiguration configuration)
    {
        if (member.Id <= 0 || !Enum.IsDefined(member.Type) || !Enum.IsDefined(member.Faction) || !Enum.IsDefined(member.TargetKind)
            || member.Hex is not { } hex || !Enum.IsDefined(hex.Lifecycle) || !Enum.IsDefined(hex.Action)
            || member.Health < 0 || member.Health > member.Profile.Health || member.Profile.Health <= 0
            || member.Experience < 0 || member.FormationSlot < 0 || member.FormationSlot >= (member.IsBoss ? 1 : configuration.Arena.SquadCapacity)
            || member.Deployed != (hex.Lifecycle is UnitLifecycle.Alive or UnitLifecycle.Dying)
            || member.Decision is not { Route: not null, Visited: not null } || member.Statuses is not { Poison: not null }
            || member.Statuses.Poison.Any(p => p is null))
            throw new ArgumentException("Malformed combat member projection.", nameof(member));
        member.Capabilities.Validate();
        StatusPolicy.Validate(member.Statuses, configuration.Statuses);
        _ = CombatProjection.Restore(member, member.Profile, member.Decision, 0);
        if (member.Deployed) RequirePose(new(hex.Position, hex.HoldsTransit ? hex.Destination : default,
            hex.HoldsTransit ? hex.Transition : 0, hex.Lifecycle == UnitLifecycle.Dying ? hex.FrozenMoveTicks : 0,
            hex.HoldsTransit ? checked((int)(hex.EndTick - hex.StartTick)) : 0), board);
        if (hex.FrozenAim is { } aim) RequirePose(aim, board);
    }
    private static void RequirePose(HexPosePoint pose, HexBoard board)
    {
        if (pose.Position.Anchor != 1 || !board.Cells.Any(c => c.Id == pose.Position.Cell)
            || pose.ElapsedTicks < 0 || pose.DurationTicks < 0 || pose.ElapsedTicks > pose.DurationTicks
            || pose.Transition != 0 && (pose.DurationTicks <= 0 || pose.Destination.Anchor != 1
                || !board.Cell(pose.Position.Cell).Neighbors.Contains(pose.Destination.Cell)
                || board.Transition(pose.Position, pose.Destination).Id != pose.Transition))
            throw new ArgumentException("Invalid combat pose.", nameof(pose));
    }
}
