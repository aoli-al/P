using System;
using System.Collections.Generic;
using System.Linq;
using PChecker.Generator.Mutator;
using PChecker.Generator.Object;
using PChecker.IO.Debugging;
using PChecker.SystematicTesting.Operations;

namespace PChecker.Generator;

internal sealed class PCTScheduleGenerator: IScheduleGenerator<PCTScheduleGenerator>
{
    public System.Random Random;
    public RandomChoices<int> IntChoices;
    public readonly int MaxPrioritySwitchPoints;
    private readonly List<AsyncOperation> _prioritizedOperations = new();
    private readonly SortedSet<int> _priorityChangePoints = new();
    private int _scheduledSteps;
    public int MaxSchedulingSteps;

    public PCTScheduleGenerator(System.Random random, RandomChoices<int>? intChoices, int maxPrioritySwitchPoints, int maxSchedulingSteps)
    {
        MaxSchedulingSteps = maxSchedulingSteps;
        IntChoices = intChoices != null ? new RandomChoices<int>(intChoices) : new RandomChoices<int>(random);

        var scheduleSize = IntChoices.Next() % MaxSchedulingSteps;
        MaxPrioritySwitchPoints = maxPrioritySwitchPoints;
        MaxSchedulingSteps = maxSchedulingSteps;

        var range = new List<int>();
        for (var idx = 1; idx < scheduleSize; idx++)
        {
            range.Add(idx);
        }

        foreach (var point in Shuffle(range).Take(maxPrioritySwitchPoints))
        {
            _priorityChangePoints.Add(point);
        }
    }

    private IList<int> Shuffle(IList<int> list)
    {
        var result = new List<int>(list);
        for (var idx = result.Count - 1; idx >= 1; idx--)
        {
            var point = IntChoices.Next() % list.Count;
            var temp = result[idx];
            result[idx] = result[point];
            result[point] = temp;
        }

        return result;
    }

    public PCTScheduleGenerator(CheckerConfiguration checkerConfiguration):
        this(new System.Random((int?)checkerConfiguration.RandomGeneratorSeed ?? Guid.NewGuid().GetHashCode()), null, checkerConfiguration.StrategyBound, checkerConfiguration.MaxUnfairSchedulingSteps)
    {
    }

    public PCTScheduleGenerator Mutate()
    {
            return new PCTScheduleMutator().Mutate(this);
    }

    public PCTScheduleGenerator Copy()
    {
        return new PCTScheduleGenerator(Random, IntChoices, MaxPrioritySwitchPoints, MaxSchedulingSteps);
    }

    public AsyncOperation? NextRandomOperation(List<AsyncOperation> enabledOperations, AsyncOperation current)
    {
        _scheduledSteps += 1;
        if (enabledOperations.Count == 0)
        {
            return null;
        }

        if (enabledOperations.Count == 1)
        {
            return enabledOperations[0];
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
            var mIndex = IntChoices.Next() % _prioritizedOperations.Count + 1;
            _prioritizedOperations.Insert(mIndex, op);
            Debug.WriteLine("<PCTLog> Detected new operation '{0}' at index '{1}'.", op.Id, mIndex);
        }

        if (_priorityChangePoints.Contains(_scheduledSteps))
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
        _priorityChangePoints.Remove(_scheduledSteps);
        var newPriorityChangePoint = _scheduledSteps + 1;
        while (_priorityChangePoints.Contains(newPriorityChangePoint))
        {
            newPriorityChangePoint++;
        }

        _priorityChangePoints.Add(newPriorityChangePoint);
        Debug.WriteLine("<PCTLog> Moving priority change to '{0}'.", newPriorityChangePoint);
    }


}