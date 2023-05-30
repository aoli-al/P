using System;
using PChecker.Generator.Object;

namespace PChecker.Generator.Mutator;

internal class PCTScheduleMutator : IMutator<PCTScheduleGenerator>
{
    private readonly int _meanMutationCount = 5;
    private readonly int _meanMutationSize = 5;
    private System.Random _random = new();
    public PCTScheduleGenerator Mutate(PCTScheduleGenerator prev)
    {
        return new PCTScheduleGenerator(prev.Random, MutateRandomChoices(prev.IntChoices), prev.MaxPrioritySwitchPoints, prev.MaxSchedulingSteps);
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
