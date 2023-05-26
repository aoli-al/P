using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using PChecker.Actors;
using PChecker.Coverage;
using PChecker.Random;
using PChecker.SystematicTesting.Operations;
using PChecker.SystematicTesting.Strategies.Feedback.Mutator;
using AsyncOperation = PChecker.SystematicTesting.Operations.AsyncOperation;

namespace PChecker.SystematicTesting.Strategies.Feedback;


internal class FeedbackGuidedStrategy<TInput, TSchedule> : IFeedbackGuidedStrategy
    where TInput: IInputGenerator<TInput>, new()
    where TSchedule: IScheduleGenerator<TSchedule>, new()
{
    protected record StrategyGenerator(TInput InputGenerator, TSchedule ScheduleGenerator);

    protected StrategyGenerator Generator;

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
        Generator = new StrategyGenerator(new TInput(), new TSchedule());
    }

    /// <inheritdoc/>
    public bool GetNextOperation(AsyncOperation current, IEnumerable<AsyncOperation> ops, out AsyncOperation next)
    {
        var enabledOperations = ops.Where(op => op.Status is AsyncOperationStatus.Enabled).ToList();
        next = Generator.ScheduleGenerator.NextRandomOperation(enabledOperations);
        _scheduledSteps++;
        return next != null;
    }

    /// <inheritdoc/>
    public bool GetNextBooleanChoice(AsyncOperation current, int maxValue, out bool next)
    {
        next = Generator.InputGenerator.Next(maxValue) == 0;

        _scheduledSteps++;

        return true;
    }

    /// <inheritdoc/>
    public bool GetNextIntegerChoice(AsyncOperation current, int maxValue, out int next)
    {
        next = Generator.InputGenerator.Next(maxValue);
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
            SavedGenerators.AddLast(Generator);
        }
    }

    private void PrepareNextInput()
    {
        if (SavedGenerators.Count == 0)
        {
            // Create a new input if no input is saved.
            Generator = new StrategyGenerator(new TInput(), new TSchedule());
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
        Generator = MutateGenerator(_currentNode!.Value);
    }

    protected virtual StrategyGenerator MutateGenerator(StrategyGenerator prev)
    {
        return new StrategyGenerator(Generator.InputGenerator.Mutate(), Generator.ScheduleGenerator.Mutate());
    }
}