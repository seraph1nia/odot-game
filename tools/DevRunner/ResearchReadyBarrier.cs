using Game.Core;

namespace DevRunner;

// A readiness acknowledgement can remain in Preparation during death cleanup.
// Both peers must observe resolution of the submitted stage before iterating.
internal static class ResearchReadyBarrier
{
    public static bool Resolved(MatchSnapshot started, MatchSnapshot accepted, MatchSnapshot observed)
        => accepted.MatchId == started.MatchId && observed.MatchId == started.MatchId
            && observed.Revision >= accepted.Revision && observed.TurnSerial > started.TurnSerial;
}
