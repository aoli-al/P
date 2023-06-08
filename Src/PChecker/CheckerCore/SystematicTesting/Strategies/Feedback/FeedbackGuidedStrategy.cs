using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using PChecker.Actors;
using PChecker.Coverage;
using PChecker.Generator;
using PChecker.Random;
using PChecker.SystematicTesting.Operations;
using AsyncOperation = PChecker.SystematicTesting.Operations.AsyncOperation;

namespace PChecker.SystematicTesting.Strategies.Feedback;


internal class FeedbackGuidedStrategy<TInput, TSchedule> : IFeedbackGuidedStrategy
    where TInput: IInputGenerator<TInput>
    where TSchedule: IScheduleGenerator<TSchedule>
{
    public record StrategyGenerator(TInput InputGenerator, TSchedule ScheduleGenerator);

    protected StrategyGenerator Generator;

    private readonly int _maxScheduledSteps;

    private int _scheduledSteps;

    private readonly CheckerConfiguration _checkerConfiguration;

    private readonly EventCoverage _visitedEvents = new();
    private readonly HashSet<int> _visitedTimelines = new();

    protected readonly List<StrategyGenerator> SavedGenerators = new();

    private readonly int _maxMutationsWithoutNewSaved = 20;

    private int _numMutationsWithoutNewSaved = 0;

    private int _currentInputIndex = 0;

    public int CurrentInputIndex()
    {
        return _currentInputIndex;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="FeedbackGuidedStrategy"/> class.
    /// </summary>
    public FeedbackGuidedStrategy(CheckerConfiguration checkerConfiguration, TInput input, TSchedule schedule)
    {
        _maxScheduledSteps = checkerConfiguration.MaxFairSchedulingSteps;
        _checkerConfiguration = checkerConfiguration;
        Generator = new StrategyGenerator(input, schedule);
    }

    /// <inheritdoc/>
    public bool GetNextOperation(AsyncOperation current, IEnumerable<AsyncOperation> ops, out AsyncOperation next)
    {
        var enabledOperations = ops.Where(op => op.Status is AsyncOperationStatus.Enabled).ToList();
        next = Generator.ScheduleGenerator.NextRandomOperation(enabledOperations, current);
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
        bool updated = _visitedEvents.Merge(runtime.GetCoverageInfo().EventInfo) ||
                       _visitedTimelines.Add(runtime.TimeLineObserver.GetCurrentTimeline());
        if (updated)
        {
            SavedGenerators.Add(Generator);
            _numMutationsWithoutNewSaved = 0;
        }
    }

    public int TotalSavedInputs()
    {
        return SavedGenerators.Count;
    }

    private void PrepareNextInput()
    {
        Generator.ScheduleGenerator.PrepareForNextInput();
        if (SavedGenerators.Count == 0)
        {
            // Mutate current input if no input is saved.
            Generator = MutateGenerator(Generator);
            return;
        }
        if (_numMutationsWithoutNewSaved >= _maxMutationsWithoutNewSaved)
        {
            MoveToNextInput();
        }
        else
        {
            _numMutationsWithoutNewSaved ++;
        }

        if (_currentInputIndex >= SavedGenerators.Count)
        {
            _currentInputIndex = 0;
        }
        Generator = MutateGenerator(SavedGenerators[_currentInputIndex]);
    }

    protected virtual void MoveToNextInput()
    {
        _currentInputIndex += 1;
        _numMutationsWithoutNewSaved = 0;
    }


    protected virtual StrategyGenerator MutateGenerator(StrategyGenerator prev)
    {
        return new StrategyGenerator(Generator.InputGenerator.Mutate(), Generator.ScheduleGenerator.Mutate());
    }
}