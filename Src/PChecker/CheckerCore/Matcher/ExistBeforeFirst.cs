using System.Collections.Generic;
using System.Linq;

namespace PChecker.Matcher;

public class ExistBeforeFirst : EventSeqMatcher
{
    private ElementMatcher _m1;
    private ElementMatcher _m2;
    public ExistBeforeFirst(ElementMatcher m1, ElementMatcher m2)
    {
        _m1 = m1;
        _m2 = m2;
    }

    public override bool Matches(List<EventObj> events)
    {
        bool m1Matched = false;
        bool m2Matched = false;
        foreach (var eventObj in events)
        {
            if (!m1Matched && _m1.Matches(eventObj))
            {
                m1Matched = true;
            }

            if (_m2.Matches(eventObj))
            {
                m2Matched = true;
                break;
            }
        }
        return m1Matched && m2Matched;
    }

}