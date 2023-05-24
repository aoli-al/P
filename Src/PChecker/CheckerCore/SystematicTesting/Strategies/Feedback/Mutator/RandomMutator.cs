using System;
using System.IO;
using PChecker.Random;

namespace PChecker.SystematicTesting.Strategies.Feedback.Mutator;

public class RandomMutator : IMutator
{
    private readonly int _meanMutationCount = 32;
    private readonly int _meanMutationSize = 128;
    private System.Random _random = new();
    public StreamBasedValueGenerator Mutate(StreamBasedValueGenerator prev)
    {
        byte[] bytes = prev.GetBytesCopy();
        int mutations = SampleGeometric(_meanMutationCount);

        while (mutations-- > 0)
        {
            int offset = _random.Next(bytes.Length);
            int mutationSize = SampleGeometric(_meanMutationSize);
            _random.NextBytes(new Span<Byte>(bytes, offset, Math.Min(mutationSize, bytes.Length - offset)));
        }
        return new StreamBasedValueGenerator(new System.Random(), new MemoryStream(bytes));
    }

    public int SampleGeometric(double mean) {
        double p = 1 / mean;
        double uniform = _random.NextDouble();
        return (int) Math.Ceiling(Math.Log(1 - uniform) / Math.Log(1 - p));
    }
}