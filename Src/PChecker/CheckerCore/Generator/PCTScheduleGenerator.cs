using System;
using System.Collections.Generic;
using System.Linq;
using PChecker.Generator.Mutator;
using PChecker.Generator.Object;
using PChecker.IO.Debugging;
using PChecker.SystematicTesting.Operations;

namespace PChecker.Generator;

internal sealed class PctScheduleGenerator: IScheduleGenerator<PctScheduleGenerator>
{
    public System.Random Random;
    public RandomChoices<int> PriorityChoices;
    public RandomChoices<double> SwitchPointChoices;
    private readonly List<AsyncOperation> _prioritizedOperations = new();
    private int _nextPriorityChangePoint;
    private int _scheduledSteps;
    public int MaxSchedulingSteps;
    private readonly double _switchPointProbability = 0.01;

    public PctScheduleGenerator(System.Random random, RandomChoices<int>? intChoices, RandomChoices<double>? switchPointChoices)
    {
        Random = random;
        PriorityChoices = intChoices != null ? new RandomChoices<int>(intChoices) : new RandomChoices<int>(random);
        SwitchPointChoices = switchPointChoices != null ? new RandomChoices<double>(switchPointChoices) :
            new RandomChoices<double>(random);

        _nextPriorityChangePoint = Utils.SampleGeometric(_switchPointProbability, SwitchPointChoices.Next());
    }

    public PctScheduleGenerator(CheckerConfiguration checkerConfiguration):
        this(new System.Random((int?)checkerConfiguration.RandomGeneratorSeed ?? Guid.NewGuid().GetHashCode()), null, null)
    {
    }

    public PctScheduleGenerator Mutate()
    {
        return new PCTScheduleMutator().Mutate(this);
    }

    public PctScheduleGenerator Copy()
    {
        return new PctScheduleGenerator(Random, PriorityChoices, SwitchPointChoices);
    }

    public AsyncOperation? NextRandomOperation(List<AsyncOperation> enabledOperations, AsyncOperation current)
    {
        _scheduledSteps += 1;
        if (enabledOperations.Count == 0)
        {
            if (_nextPriorityChangePoint == _scheduledSteps)
            {
                MovePriorityChangePointForward();
            }
            return null;
        }

        return GetPrioritizedOperation(enabledOperations, current);
    }


    private AsyncOperation GetPrioritizedOperation(List<AsyncOperation> ops, AsyncOperation current)
    {
        if (_prioritizedOperations.Count == 0)
        {
            _prioritizedOperations.Add(current);
        }

        foreach (var op in ops.Where(op => !_prioritizedOperations.Contains(op)))
        {
            var mIndex = PriorityChoices.Next() % _prioritizedOperations.Count + 1;
            _prioritizedOperations.Insert(mIndex, op);
            Debug.WriteLine("<PCTLog> Detected new operation '{0}' at index '{1}'.", op.Id, mIndex);
        }

        if (_nextPriorityChangePoint == _scheduledSteps)
        {
            if (ops.Count == 1)
            {
                MovePriorityChangePointForward();
            }
            else
            {
                var priority = GetHighestPriorityEnabledOperation(ops);
                _prioritizedOperations.Remove(priority);
                _prioritizedOperations.Add(priority);
                Debug.WriteLine("<PCTLog> Operation '{0}' changes to lowest priority.", priority);
                _nextPriorityChangePoint += Utils.SampleGeometric(_switchPointProbability, SwitchPointChoices.Next());
            }
        }

        var prioritizedSchedulable = GetHighestPriorityEnabledOperation(ops);
        if (Debug.IsEnabled)
        {
            Debug.WriteLine("<PCTLog> Prioritized schedulable '{0}'.", prioritizedSchedulable);
            Debug.Write("<PCTLog> Priority list: ");
            for (var idx = 0; idx < _prioritizedOperations.Count; idx++)
            {
                if (idx < _prioritizedOperations.Count - 1)
                {
                    Debug.Write("'{0}', ", _prioritizedOperations[idx]);
                }
                else
                {
                    Debug.WriteLine("'{0}'.", _prioritizedOperations[idx]);
                }
            }
        }

        return ops.First(op => op.Equals(prioritizedSchedulable));
    }

    private AsyncOperation GetHighestPriorityEnabledOperation(IEnumerable<AsyncOperation> choices)
    {
        AsyncOperation prioritizedOp = null;
        foreach (var entity in _prioritizedOperations)
        {
            if (choices.Any(m => m == entity))
            {
                prioritizedOp = entity;
                break;
            }
        }

        return prioritizedOp;
    }

    private void MovePriorityChangePointForward()
    {
        _nextPriorityChangePoint += 1;
        Debug.WriteLine("<PCTLog> Moving priority change to '{0}'.", _nextPriorityChangePoint);
    }


}