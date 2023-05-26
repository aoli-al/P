using System;
using System.IO;
using PChecker.Random;

namespace PChecker.SystematicTesting.Strategies.Feedback.Mutator;

public class RandomInputMutator : IMutator<RandomInputGenerator>
{
    private readonly int _meanMutationCount = 32;
    private readonly int _meanMutationSize = 128;
    private System.Random _random = new();
    public RandomInputGenerator Mutate(RandomInputGenerator prev)
    {
        byte[] bytes = prev.GetBytesCopy();
        int mutations = Utils.SampleGeometric(_meanMutationCount, _random);

        while (mutations-- > 0)
        {
            int offset = _random.Next(bytes.Length);
            int mutationSize = Utils.SampleGeometric(_meanMutationSize, _random);
            _random.NextBytes(new Span<Byte>(bytes, offset, Math.Min(mutationSize, bytes.Length - offset)));
        }
        return new RandomInputGenerator(new System.Random(), new MemoryStream(bytes));
    }
}