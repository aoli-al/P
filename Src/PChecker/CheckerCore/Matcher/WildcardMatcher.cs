using System.Collections.Generic;
using PChecker.Actors.Events;

namespace PChecker.Matcher;

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

    public bool IsInterestingEvent(Event e)
    {
        return true;
    }
}