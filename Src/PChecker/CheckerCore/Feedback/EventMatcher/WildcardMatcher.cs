using System.Collections.Generic;
using PChecker.Actors.Events;

namespace PChecker.Feedback.EventMatcher;

internal class WildcardMatcher :  IMatcher
{
    public bool MatchOne(Event e)
    {
        return true;
    }

    public void Reset()
    {
    }

    public HashSet<int> GetVisitedStates()
    {
        return new HashSet<int>();
    }
}