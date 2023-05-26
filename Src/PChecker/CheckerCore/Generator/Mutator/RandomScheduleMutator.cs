using System;
using System.Collections.Generic;
using System.IO;
using PChecker.Random;

namespace PChecker.SystematicTesting.Strategies.Feedback.Mutator;

internal class RandomScheduleMutator : IMutator<RandomScheduleGenerator>
{
    private readonly int _meanMutationCount = 5;
    private readonly int _meanMutationSize = 5;
    private System.Random _random = new();
    public RandomScheduleGenerator Mutate(RandomScheduleGenerator prev)
    {
        int mutations = Utils.SampleGeometric(_meanMutationCount, _random);

        List<int> newChoices = new List<int>(prev.Choices);

        while (mutations-- > 0)
        {
            int offset = _random.Next(newChoices.Count);
            int mutationSize = Utils.SampleGeometric(_meanMutationSize, _random);
            for (int i = 0; i < offset + mutationSize; i++)
            {
                if (i + offset >= newChoices.Count)
                {
                    break;
                }

                newChoices[i + offset] = _random.Next();
            }
        }

        return new RandomScheduleGenerator(newChoices);
    }
}