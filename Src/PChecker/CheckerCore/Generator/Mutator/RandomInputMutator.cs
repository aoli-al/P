using System;
using System.IO;
using PChecker.Generator.Object;

namespace PChecker.Generator.Mutator;

public class RandomInputMutator : IMutator<RandomInputGenerator>
{
    private readonly int _meanMutationCount = 32;
    private readonly int _meanMutationSize = 128;
    private System.Random _random = new();
    public RandomInputGenerator Mutate(RandomInputGenerator prev)
    {
        return new RandomInputGenerator(prev.Random, MutateRandomChoices(prev.IntChoices),
            MutateRandomChoices(prev.DoubleChoices));
    }

    private RandomChoices<T> MutateRandomChoices<T> (RandomChoices<T> randomChoices)
        where T: IConvertible
    {
        RandomChoices<T> newChoices = new RandomChoices<T>(randomChoices);
        int mutations = Utils.SampleGeometric(_meanMutationCount, _random);

        while (mutations-- > 0)
        {
            int offset = _random.Next(newChoices.Data.Count);
            int mutationSize = Utils.SampleGeometric(_meanMutationSize, _random);
            for (int i = 0; i < offset + mutations; i++)
            {
                if (i >= newChoices.Data.Count)
                {
                    break;
                }

                newChoices.Data[i] = newChoices.GenerateNew();
            }
        }

        return newChoices;
    }
}