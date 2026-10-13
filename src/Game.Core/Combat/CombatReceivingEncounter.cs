namespace Game.Core;

// One fixed numerical receiving encounter, not a simulator, save loader or
// network control plane. Only Game's owned headless playing-host bootstrap calls
// this internal seam; ordinary authority commands/RPCs cannot select a fixture.
// Synthetic tower materials, one explicit wound/Keep health and physical poses
// are deliberately NOT earned economy or full-game/network acceptance.
public sealed partial class AuthoritySession
{
    private bool _receivingPrepared, _receivingPlaced;

    internal bool ConfigureReceivingEncounter(bool protectedKeep)
    {
        if (IsEnded || _match.Players.Count != 2) return false;
        if (!_receivingPrepared && _match.Phase == Phase.Preparation)
        {
            City city = _match.Players[2];
            city.Gold = 100; city.Wood = 100; city.Stone = 100;
            foreach (var (action, building) in new[] { ("buy-plot", Building.Empty), ("build", Building.ArrowTower) })
            {
                CommandResult result = _match.Apply(2, Command.FromSnapshot(_match.Snapshot(), 1, action, 2, 11, building));
                if (!result.Accepted) throw new InvalidOperationException("Controlled receiving tower input refused: " + result.Message);
            }
            _receivingPrepared = true;
            return true;
        }
        if (!_receivingPrepared || _receivingPlaced || _match.Phase != Phase.Combat || _match.Tick != 0) return false;
        MatchSnapshot initial = _match.Snapshot();
        if (initial.Players.Any(c => c.Soldiers.Length != 6 || c.Soldiers.Any(u => !u.Participating)))
            throw new InvalidOperationException("Controlled receiving encounter requires two identified six-member field rosters.");
        var arena = new SharedArena([1, 2], [(2, 11)]);
        int enemy = _match.Combat.Enemies().Last().Id;
        foreach (UnitState extra in _match.Combat.Enemies().Where(u => u.Id != enemy)) _match.Combat.Remove(extra.Id);
        foreach (SquadState squad in initial.Squads.Where(s => s.Faction == Faction.Adventurers))
        {
            int home = _match.Combat.Read(squad.Members[0]).Assignment!.Tile;
            int cell = squad.Owner == 1 ? arena.EnemyCells(1)[home] : arena.Deployment(1, home);
            foreach (int id in squad.Members)
                _match.Combat.Seed(_match.Combat.Inspect(id) with { Location = new(UnitLifecycle.Alive, new(cell, 1)) });
        }
        int approach = arena.Board.Cell(arena.Keep(2)).Neighbors.First(arena.CanOccupy);
        _match.Combat.Seed(_match.Combat.Inspect(enemy) with { Location = new(UnitLifecycle.Alive, new(approach, 1)) });
        int injured = _match.Players[2].Soldiers[0].Id;
        _match.Combat.Seed(_match.Combat.Inspect(injured) with { Health = HealthPoints.FromWhole(33) });
        _match.Players[2].Health = HealthPoints.FromWhole(protectedKeep ? 100 : 5);
        _receivingPlaced = true;
        return true;
    }
}
