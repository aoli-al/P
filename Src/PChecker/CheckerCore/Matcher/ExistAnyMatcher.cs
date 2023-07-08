using System.Collections.Generic;
using System.Linq;

namespace PChecker.Matcher;

public class ExistAnyMatcher : EventSeqMatcher
{
    public ElementMatcher _matcher;
    public ExistAnyMatcher(ElementMatcher matcher)
    {
        _matcher = matcher;
    }
    public override bool Matches(List<EventObj> events)
    {
        return events.Any(it => _matcher.Matches(it));
    }
}