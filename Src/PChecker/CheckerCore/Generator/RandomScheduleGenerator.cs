using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using PChecker.SystematicTesting.Operations;
using PChecker.SystematicTesting.Strategies.Feedback.Mutator;

namespace PChecker.Random;

internal class RandomScheduleGenerator: IScheduleGenerator<RandomScheduleGenerator>
{
    private readonly System.Random _random = new();

    public List<int> Choices = new();
    private int _index = 0;


    public RandomScheduleGenerator(List<int> choices)
    {
        Choices = choices;
    }

    public RandomScheduleGenerator()
    {
    }


    public AsyncOperation? NextRandomOperation(List<AsyncOperation> enabledOperations)
    {
        if (enabledOperations.Count == 0)
        {
            return null;
        }

        if (enabledOperations.Count == 1)
        {
            return enabledOperations[0];
        }

        if (_index >= Choices.Count)
        {
            Choices.Add(_random.Next(enabledOperations.Count));
        }
        return enabledOperations[Choices[_index++] % enabledOperations.Count];
    }

    public RandomScheduleGenerator Mutate()
    {
        return new RandomScheduleMutator().Mutate(this);
    }

    public RandomScheduleGenerator Copy()
    {
        return new RandomScheduleGenerator(Choices);
    }
}