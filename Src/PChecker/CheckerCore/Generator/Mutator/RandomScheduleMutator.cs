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
        return new RandomScheduleGenerator(prev.Random, Utils.MutateRandomChoices(prev.IntChoices, _meanMutationCount, _meanMutationSize, _random));
    }

}