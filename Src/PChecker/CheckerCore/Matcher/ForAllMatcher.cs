using System.Collections.Generic;
using System.Linq;

namespace PChecker.Matcher;

public class ForAllMatcher : EventSeqMatcher
{
    private ElementMatcher _matcher;
    public ForAllMatcher(ElementMatcher matcher)
    {
        _matcher = matcher;
    }

    public override bool Matches(List<EventObj> events)
    {
        return events.All(it => _matcher.Matches(it));
    }

}