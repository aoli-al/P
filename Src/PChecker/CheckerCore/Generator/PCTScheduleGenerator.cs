using System;
using System.Collections.Generic;
using System.Linq;
using PChecker.Feedback;
using PChecker.Generator.Mutator;
using PChecker.Generator.Object;
using PChecker.IO.Debugging;
using PChecker.SystematicTesting.Operations;
using PChecker.SystematicTesting.Strategies.Probabilistic;

namespace PChecker.Generator;

internal sealed class PctScheduleGenerator: PriorizationSchedulingBase, IScheduleGenerator<PctScheduleGenerator>
{

    class PCTProvider : PriorizationProvider
    {
        public RandomChoices<int> PriorityChoices;
        public RandomChoices<double> SwitchPointChoices;
        public PCTProvider(RandomChoices<int> priority, RandomChoices<double> switchPoint)
        {
            PriorityChoices = priority;
            SwitchPointChoices = switchPoint;
        }


        public int AssignPriority(int numOps)
        {

            return PriorityChoices.Next() % numOps + 1;
        }

        public double SwitchPointChoice()
        {
            return SwitchPointChoices.Next();
        }
    }
    public System.Random Random;
    public RandomChoices<int> PriorityChoices;
    public RandomChoices<double> SwitchPointChoices;

    public PctScheduleGenerator(System.Random random, RandomChoices<int>? priorityChoices, RandomChoices<double>? switchPointChoices, int numSwitchPoints, int maxScheduleLength, ConflictOpMonitor? monitor): base(numSwitchPoints, maxScheduleLength, new PCTProvider(priorityChoices != null ? new RandomChoices<int>(priorityChoices) : new RandomChoices<int>(random), switchPointChoices != null ? new RandomChoices<double>(switchPointChoices) :
            new RandomChoices<double>(random)), monitor)
    {
        Random = random;
        var provider = (PCTProvider) Provider;
        PriorityChoices = provider.PriorityChoices;
        SwitchPointChoices = provider.SwitchPointChoices;
    }

    public PctScheduleGenerator(CheckerConfiguration checkerConfiguration, ConflictOpMonitor? monitor):
        this(new System.Random((int?)checkerConfiguration.RandomGeneratorSeed ?? Guid.NewGuid().GetHashCode()), null, null, checkerConfiguration.StrategyBound,  0, monitor)
    {
    }

    public PctScheduleGenerator Mutate()
    {
        return new PCTScheduleMutator().Mutate(this);
    }

    public PctScheduleGenerator New()
    {
        return new PctScheduleGenerator(Random, null, null, MaxPrioritySwitchPoints, ScheduleLength, ConflictOpMonitor);
    }

    public PctScheduleGenerator Copy()
    {
        return new PctScheduleGenerator(Random, PriorityChoices, SwitchPointChoices, MaxPrioritySwitchPoints, ScheduleLength, ConflictOpMonitor);
    }

    public AsyncOperation? NextRandomOperation(List<AsyncOperation> enabledOperations, AsyncOperation current)
    {
        if (GetNextOperation(current, enabledOperations, out var next)) {
            return next;
        } else {
            return null;
        }
    }


    public void PrepareForNextInput()
    {
        PrepareForNextIteration();
    }
}