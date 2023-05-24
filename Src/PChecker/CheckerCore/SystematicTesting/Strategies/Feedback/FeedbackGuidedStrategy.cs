using System;
using System.Collections.Generic;
using System.Linq;
using PChecker.Actors;
using PChecker.Random;
using PChecker.SystematicTesting.Operations;
using PChecker.SystematicTesting.Strategies.Feedback.Mutator;

namespace PChecker.SystematicTesting.Strategies.Feedback;

record StrategyGenerator(StreamBasedValueGenerator InputGenerator, StreamBasedValueGenerator ScheduleGenerator);

record StrategyMutator(IMutator InputMutator, IMutator ScheduleMutator);

internal class FeedbackGuidedStrategy : ISchedulingStrategy
{
    private StrategyGenerator _generator;
    private StrategyMutator _mutator = new StrategyMutator(new RandomMutator(), new RandomMutator());

    private readonly int _maxScheduledSteps;

    private int _scheduledSteps;

    private readonly CheckerConfiguration _checkerConfiguration;

    private readonly HashSet<int> _visitedStates = new();

    private readonly LinkedList<StrategyGenerator> _savedGenerators = new();

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
        // Noop
        _scheduledSteps = 0;
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
    public void ObserveRunningResults(ControlledRuntime runtime)
    {
        // TODO: implement real feedback.
        int stateHash = runtime.GetCoverageInfo().EventInfo.GetHashCode();
        if (!_visitedStates.Contains(stateHash))
        {
            _savedGenerators.AddLast(_generator);
        }
        PrepareNextInput();
    }

    private void PrepareNextInput()
    {
        if (_savedGenerators.Count == 0)
        {
            // Create a new input if no input is saved.
            _generator = new StrategyGenerator(new StreamBasedValueGenerator(_checkerConfiguration),
                new StreamBasedValueGenerator(_checkerConfiguration));
            return;
        }
        if (_numMutations == _maxMutations)
        {
            _currentNode = _currentNode?.Next;
        }
        _currentNode ??= _savedGenerators.First;
        _generator = MutateGenerator(_currentNode!.Value);
    }

    private StrategyGenerator MutateGenerator(StrategyGenerator prev)
    {
        return new StrategyGenerator(
            _mutator.InputMutator.Mutate(prev.InputGenerator),
            _mutator.ScheduleMutator.Mutate(prev.ScheduleGenerator)
        );
    }
}