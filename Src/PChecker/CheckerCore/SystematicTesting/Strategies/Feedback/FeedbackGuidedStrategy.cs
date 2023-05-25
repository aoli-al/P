using System;
using System.Collections.Generic;
using System.Linq;
using PChecker.Actors;
using PChecker.Coverage;
using PChecker.Random;
using PChecker.SystematicTesting.Operations;
using PChecker.SystematicTesting.Strategies.Feedback.Mutator;

namespace PChecker.SystematicTesting.Strategies.Feedback;

record StrategyGenerator(StreamBasedValueGenerator InputGenerator, StreamBasedValueGenerator ScheduleGenerator);

record StrategyMutator(IMutator InputMutator, IMutator ScheduleMutator);

internal class FeedbackGuidedStrategy : ISchedulingStrategy
{
    private StrategyGenerator _generator;
    protected StrategyMutator Mutator = new StrategyMutator(new RandomMutator(), new RandomMutator());

    private readonly int _maxScheduledSteps;

    private int _scheduledSteps;

    private readonly CheckerConfiguration _checkerConfiguration;

    private readonly EventCoverage _visitedEvents = new();

    protected readonly LinkedList<StrategyGenerator> SavedGenerators = new();

    private readonly int _maxMutations = 50;

    private int _numMutations = 0;

    private LinkedListNode<StrategyGenerator>? _currentNode = null;


    /// <summary>
    /// Initializes a new instance of the <see cref="FeedbackGuidedStrategy"/> class.
    /// </summary>
    public FeedbackGuidedStrategy(CheckerConfiguration checkerConfiguration)
    {
        _maxScheduledSteps = checkerConfiguration.MaxFairSchedulingSteps;
        _checkerConfiguration = checkerConfiguration;
        _generator = new StrategyGenerator(new StreamBasedValueGenerator(_checkerConfiguration),
            new StreamBasedValueGenerator(_checkerConfiguration));
    }

    /// <inheritdoc/>
    public bool GetNextOperation(AsyncOperation current, IEnumerable<AsyncOperation> ops, out AsyncOperation next)
    {
        var enabledOperations = ops.Where(op => op.Status is AsyncOperationStatus.Enabled).ToList();
        if (enabledOperations.Count == 0)
        {
            next = null;
            return false;
        }

        var idx = _generator.ScheduleGenerator.Next(enabledOperations.Count);
        next = enabledOperations[idx];

        _scheduledSteps++;

        return true;
    }

    /// <inheritdoc/>
    public bool GetNextBooleanChoice(AsyncOperation current, int maxValue, out bool next)
    {
        next = _generator.InputGenerator.Next(maxValue) == 0;

        _scheduledSteps++;

        return true;
    }

    /// <inheritdoc/>
    public bool GetNextIntegerChoice(AsyncOperation current, int maxValue, out int next)
    {
        next = _generator.InputGenerator.Next(maxValue);
        _scheduledSteps++;
        return true;
    }

    /// <inheritdoc/>
    public bool PrepareForNextIteration()
    {
        _scheduledSteps = 0;
        PrepareNextInput();
        return true;
    }

    /// <inheritdoc/>
    public int GetScheduledSteps()
    {
        return _scheduledSteps;
    }

    /// <inheritdoc/>
    public bool HasReachedMaxSchedulingSteps()
    {
        if (_maxScheduledSteps == 0)
        {
            return false;
        }

        return _scheduledSteps >= _maxScheduledSteps;
    }

    /// <inheritdoc/>
    public bool IsFair()
    {
        return true;
    }

    /// <inheritdoc/>
    public string GetDescription()
    {
        return "feedback";
    }

    /// <inheritdoc/>
    public void Reset()
    {
        _scheduledSteps = 0;
    }

    /// <summary>
    /// This method observes the results of previous run and prepare for the next run.
    /// </summary>
    /// <param name="runtime">The ControlledRuntime of previous run.</param>
    public virtual void ObserveRunningResults(ControlledRuntime runtime)
    {
        // TODO: implement real feedback.
        if (_visitedEvents.Merge(runtime.GetCoverageInfo().EventInfo))
        {
            SavedGenerators.AddLast(_generator);
        }
    }

    private void PrepareNextInput()
    {
        if (SavedGenerators.Count == 0)
        {
            // Create a new input if no input is saved.
            _generator = new StrategyGenerator(new StreamBasedValueGenerator(_checkerConfiguration),
                new StreamBasedValueGenerator(_checkerConfiguration));
            return;
        }
        if (_numMutations == _maxMutations)
        {
            _currentNode = _currentNode?.Next;
            _numMutations = 0;
        }
        else
        {
            _numMutations ++;
        }
        _currentNode ??= SavedGenerators.First;
        _generator = MutateGenerator(_currentNode!.Value);
    }

    protected virtual StrategyGenerator MutateGenerator(StrategyGenerator prev)
    {
        return new StrategyGenerator(
            Mutator.InputMutator.Mutate(prev.InputGenerator),
            Mutator.ScheduleMutator.Mutate(prev.ScheduleGenerator)
        );
    }
}