using System;
using System.Collections.Generic;
using System.Linq;
using PChecker.Actors.Events;

namespace PChecker.Matcher;

public interface IMatcher<T>
{
    bool Matches(T e);
}


public abstract class IJunction<T> : IMatcher<T>
{
    public abstract bool Matches(T e);

    public IJunction<T> And(IMatcher<T> other)
    {
        return new Conjunction<T>(this, other);
    }

    public IJunction<T> Or(IMatcher<T> other)
    {
        return new Disjunction<T>(this, other);
    }
}


public class Conjunction<T> : IJunction<T>
{
    private IMatcher<T>[] _matchers;

    public Conjunction(params IMatcher<T>[] matchers)
    {
        _matchers = matchers;
    }

    public override bool Matches(T e)
    {
        return _matchers.All(it => it.Matches(e));
    }
}

public class Disjunction<T> : IJunction<T>
{
    private IMatcher<T>[] _matchers;

    public Disjunction(params IMatcher<T>[] matchers)
    {
        _matchers = matchers;
    }

    public override bool Matches(T e)
    {
        return _matchers.Any(it => it.Matches(e));
    }
}


public abstract class ElementMatcher : IJunction<EventObj>
{
}

public abstract class PairMatcher : IJunction<Tuple<EventObj, EventObj>>
{
}

public abstract class EventSeqMatcher : IJunction<List<EventObj>>
{
}