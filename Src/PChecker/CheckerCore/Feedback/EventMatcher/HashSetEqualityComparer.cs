using System;
using System.Collections.Generic;

namespace PChecker.Feedback.EventMatcher;

public class HashSetEqualityComparer : IEqualityComparer<HashSet<int>>
{
    public bool Equals(HashSet<int> x, HashSet<int> y)
    {
        if (x == null || y == null)
            return x == y;
        else
            return x.SetEquals(y);
    }

    public int GetHashCode(HashSet<int> obj)
    {
        if (obj == null)
            return 0;
        int hashcode = 0;
        foreach (var item in obj)
            hashcode ^= item.GetHashCode();
        return hashcode;
    }
}