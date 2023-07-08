using System.Collections.Generic;

namespace PChecker.Matcher;

public class ForAllBeforeFirst : EventSeqMatcher
{
    private ElementMatcher _m1;
    private ElementMatcher _m2;
    public ForAllBeforeFirst(ElementMatcher m1, ElementMatcher m2)
    {
        _m1 = m1;
        _m2 = m2;
    }

    public override bool Matches(List<EventObj> events)
    {
        foreach (var eventObj in events)
        {
            if (_m2.Matches(eventObj))
            {
                return true;
            }
            if (!_m1.Matches(eventObj))
            {
                return false;
            }
        }
        return false;
    }

}