using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;

namespace PChecker.Matcher.Tmp;

public interface IProcessor<I, out O>
{
    public O Process(I input);
}

public abstract class IJunction<T> : IProcessor<T, bool>
{

    public IJunction<T> And(IJunction<T> other)
    {
        return new Conjunction<T>(this, other);
    }

    public IJunction<T> Or(IJunction<T> other)
    {
        return new Disjunction<T>(this, other);
    }

    public abstract bool Process(T input);
}


public class Conjunction<T> : IJunction<T>
{
    private IProcessor<T, bool>[] _matchers;

    public Conjunction(params IProcessor<T, bool>[] matchers)
    {
        _matchers = matchers;
    }

    public override bool Process(T e)
    {
        return _matchers.All(it => it.Process(e));
    }
}

public class Disjunction<T> : IJunction<T>
{
    private IProcessor<T, bool>[] _matchers;

    public Disjunction(params IProcessor<T, bool>[] matchers)
    {
        _matchers = matchers;
    }

    public override bool Process(T e)
    {
        return _matchers.Any(it => it.Process(e));
    }
}


public abstract class SeqProcessor : IProcessor<IEnumerable<EventObj>, IEnumerable<EventObj>>
{
    public abstract IEnumerable<EventObj> Process(IEnumerable<EventObj> input);
}

public abstract class ElementMatcher : IJunction<EventObj>
{
    public abstract override bool Process(EventObj input);
}


public class ForAll : IJunction<IEnumerable<EventObj>>
{
    private ElementMatcher _matcher;

    public ForAll(ElementMatcher matcher)
    {
        _matcher = matcher;
    }

    public override bool Process(IEnumerable<EventObj> input)
    {
        return input.All(it => _matcher.Process(it));
    }
}

public class First : IProcessor<IEnumerable<EventObj>, EventObj>
{
    private ElementMatcher _matcher;

    public First(ElementMatcher matcher)
    {
        _matcher = matcher;
    }

    public EventObj Process(IEnumerable<EventObj> input)
    {
        return input.First(it => _matcher.Process(it));
    }
}

public class Equal<T> : IJunction<IEnumerable<EventObj>>
{
    private IProcessor<IEnumerable<EventObj>, T> _p1;
    private IProcessor<IEnumerable<EventObj>, T> _p2;

    public Equal(IProcessor<IEnumerable<EventObj>, T> p1, IProcessor<IEnumerable<EventObj>, T> p2)
    {
        _p1 = p1;
        _p2 = p2;
    }

    public override bool Process(IEnumerable<EventObj> input)
    {
        return _p1.Process(input).Equals(_p2.Process(input));
    }
}

public class LessThan : IJunction<IEnumerable<EventObj>>
{
    private IProcessor<IEnumerable<EventObj>, int> _p1;
    private IProcessor<IEnumerable<EventObj>, int> _p2;

    public LessThan(IProcessor<IEnumerable<EventObj>, int> p1, IProcessor<IEnumerable<EventObj>, int> p2)
    {
        _p1 = p1;
        _p2 = p2;
    }

    public override bool Process(IEnumerable<EventObj> input)
    {
        return _p1.Process(input) < _p2.Process(input);
    }
}

public class Count : IProcessor<IEnumerable<EventObj>, int>
{
    private ElementMatcher _matcher;

    public Count(ElementMatcher matcher)
    {
        _matcher = matcher;
    }

    public int Process(IEnumerable<EventObj> input)
    {
        return input.Count(it => _matcher.Process(it));
    }
}

public class ConstantMatcher<T> : IProcessor<IEnumerable<EventObj>, T>
{
    private T _value;
    public ConstantMatcher(T value)
    {
        _value = value;
    }

    public T Process(IEnumerable<EventObj> input)
    {
        return _value;
    }
}