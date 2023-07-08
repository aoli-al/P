using System;
using System.Collections.Generic;

namespace PChecker.Matcher;

public class ExistPairMatcher : EventSeqMatcher
{
    private readonly PairMatcher _matcher;
    public ExistPairMatcher(PairMatcher matcher)
    {
        _matcher = matcher;
    }


    public override bool Matches(List<EventObj> events)
    {
        for (int i = 0; i < events.Count - 1; i++)
        {
            for (int j = i + 1; j < events.Count; j++)
            {
                var pair = Tuple.Create(events[i], events[j]);
                if (_matcher.Matches(pair))
                {
                    return true;
                }
            }
        }
        return false;
    }
}