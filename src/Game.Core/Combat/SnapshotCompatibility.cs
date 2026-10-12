namespace Game.Core;

// A deliberate refusal, not migration by inventing squads from old soldier
// fields. Reconnect credentials are separate; this validates combat projections.
public static class SnapshotCompatibility
{
    public const int MinimumSquadRulesVersion = 6;
    public static void RequireSharedArena(MatchSnapshot state)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (state.CombatRulesVersion < MinimumSquadRulesVersion || state.Rules?.Combat is null || state.Rules.Arena is null
            || state.Rules.Combat.RulesVersion != state.CombatRulesVersion || state.Rules.Combat.Board is not { Capacity: 1, Id: "shared-watch-arena" }
            || state.Arena is null || state.Arena.Board is not { Capacity: 1 } || state.Squads is null || state.Structures is null)
            throw new ArgumentException("Legacy combat snapshot refused: this client requires shared arena squad rules. Rejoin a compatible authority or start a fresh match.", nameof(state));
        if (state.Players is null || state.Enemies is null || state.DyingBodies is null || state.Players.Length > 4
            || state.Players.Length == 0 && state.Phase != Phase.Lobby || state.Rules.Arena.SquadCapacity is < 1 or > 12)
            throw new ArgumentException("Malformed shared arena roster.", nameof(state));
        UnitState[] members = state.Players.SelectMany(p => p.Soldiers).Concat(state.Enemies).ToArray();
        if (members.Select(m => m.Id).Distinct().Count() != members.Length || state.Squads.Select(s => s.Id).Distinct().Count() != state.Squads.Length)
            throw new ArgumentException("Duplicate member or squad identity.", nameof(state));
        var byId = members.ToDictionary(m => m.Id);
        var cells = state.Arena.Board.Cells.Select(c => c.Id).ToHashSet();
        var actors = state.Squads.Where(s => s.Hex.Lifecycle == UnitLifecycle.Alive).ToArray();
        if (actors.Select(s => s.Hex.Position.Cell).Distinct().Count() != actors.Length
            || actors.Any(s => !cells.Contains(s.Hex.Position.Cell) || state.Arena.Footprints.Any(f => f.Cell == s.Hex.Position.Cell)))
            throw new ArgumentException("Shared arena squads violate exclusive static-cell occupancy.", nameof(state));
        foreach (SquadState squad in state.Squads)
        {
            if (squad.Id <= 0 || squad.Hex.Id != squad.Id || squad.Members.Length > (squad.IsBoss ? 1 : state.Rules.Arena.SquadCapacity)
                || squad.Members.Distinct().Count() != squad.Members.Length || squad.ParticipatingMembers.Distinct().Count() != squad.ParticipatingMembers.Length
                || squad.ParticipatingMembers.Any(id => !squad.Members.Contains(id)))
                throw new ArgumentException("Invalid squad population or actor identity.", nameof(state));
            foreach (int id in squad.Members)
                if (!byId.TryGetValue(id, out UnitState? member) || member.SquadId != squad.Id || member.Type != squad.Type || member.Faction != squad.Faction
                    || member.Owner != squad.Owner || member.IsBoss != squad.IsBoss || member.Experience < 0 || member.Health <= 0 || member.Health > member.Profile.Health
                    || member.FormationSlot < 0 || member.FormationSlot >= (squad.IsBoss ? 1 : state.Rules.Arena.SquadCapacity))
                    throw new ArgumentException("Invalid persistent squad member.", nameof(state));
            if (squad.Members.Select(id => byId[id].FormationSlot).Distinct().Count() != squad.Members.Length)
                throw new ArgumentException("Squad members have overlapping formation slots.", nameof(state));
        }
        if (members.Any(m => !state.Squads.Any(s => s.Id == m.SquadId && s.Members.Contains(m.Id))))
            throw new ArgumentException("A persistent member has no squad identity.", nameof(state));
    }
}
