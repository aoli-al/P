using System.Collections.Generic;
using PChecker.Actors.Events;

namespace PChecker.Feedback.EventMatcher;

public interface IMatcher
{
    public bool MatchOne(Event e);
    public void Reset();

    public HashSet<int> GetVisitedStates();

    public bool IsInterestingEvent(Event e);
}