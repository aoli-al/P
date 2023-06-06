using System.Collections.Generic;

namespace PChecker.Feedback;

public class ComparableList : IEqualityComparer<ComparableList>
{
    public List<long> Data = new();
    private readonly double _threshold;

    public ComparableList(double threshold)
    {
        _threshold = threshold;
    }

    public bool Equals(ComparableList x, ComparableList y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Data.Equals(y.Data);
    }

    public int GetHashCode(ComparableList obj)
    {
        return obj.Data.GetHashCode();
    }
}