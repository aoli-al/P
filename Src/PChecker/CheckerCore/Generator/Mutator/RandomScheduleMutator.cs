using System;
using System.Collections.Generic;
using PChecker.Generator.Object;

namespace PChecker.Generator.Mutator;

internal class RandomScheduleMutator : IMutator<RandomScheduleGenerator>
{
    private readonly int _meanMutationCount = 5;
    private readonly int _meanMutationSize = 5;
    private System.Random _random = new();
    public RandomScheduleGenerator Mutate(RandomScheduleGenerator prev)
    {
        return new RandomScheduleGenerator(prev.Random, MutateRandomChoices(prev.IntChoices));
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