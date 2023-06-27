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

    protected int ScheduledSteps;
    private HashSet<int> _visitedStates = new();

    private readonly EventCoverage _visitedEvents = new();
    private readonly HashSet<int> _visitedEventSeqs = new();

    protected readonly List<StrategyGenerator> SavedGenerators = new();

    private readonly int _maxMutationsWithoutNewSaved = 3;

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
        Generator = new StrategyGenerator(input, schedule);
    }

    /// <inheritdoc/>
    public virtual bool GetNextOperation(AsyncOperation current, IEnumerable<AsyncOperation> ops, out AsyncOperation next)
    {
        var enabledOperations = ops.Where(op => op.Status is AsyncOperationStatus.Enabled).ToList();
        next = Generator.ScheduleGenerator.NextRandomOperation(enabledOperations, current);
        ScheduledSteps++;
        return next != null;
    }

    /// <inheritdoc/>
    public bool GetNextBooleanChoice(AsyncOperation current, int maxValue, out bool next)
    {
        next = Generator.InputGenerator.Next(maxValue) == 0;
        return true;
    }

    /// <inheritdoc/>
    public bool GetNextIntegerChoice(AsyncOperation current, int maxValue, out int next)
    {
        next = Generator.InputGenerator.Next(maxValue);
        return true;
    }

    /// <inheritdoc/>
    public virtual bool PrepareForNextIteration()
    {
        ScheduledSteps = 0;
        PrepareNextInput();
        return true;
    }

    /// <inheritdoc/>
    public int GetScheduledSteps()
    {
        return ScheduledSteps;
    }

    /// <inheritdoc/>
    public bool HasReachedMaxSchedulingSteps()
    {
        if (_maxScheduledSteps == 0)
        {
            return false;
        }

        return ScheduledSteps >= _maxScheduledSteps;
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
        ScheduledSteps = 0;
    }

    public List<string> LastSavedSchedule = new();

    /// <summary>
    /// This method observes the results of previous run and prepare for the next run.
    /// </summary>
    /// <param name="runtime">The ControlledRuntime of previous run.</param>
    public virtual void ObserveRunningResults(ControlledRuntime runtime)
    {
        // TODO: implement real feedback.
        if (runtime.EventPatternObserver.IsMatched())
        {
            int prevSize = _visitedEventSeqs.Count;
            _visitedEventSeqs.UnionWith(runtime.EventSeqObserver.SavedEvents);
            bool updated = _visitedEvents.Merge(runtime.GetCoverageInfo().EventInfo) ||
                           prevSize != _visitedEventSeqs.Count;
            if (updated)
            {
                LastSavedSchedule = runtime.EventPatternObserver.SavedEventTypes;
                SavedGenerators.Add(Generator);
                _numMutationsWithoutNewSaved = 0;
            }
        }
        else
        {
            int prevStates = _visitedStates.Count;
            _visitedStates.UnionWith(runtime.EventPatternObserver.Matcher.VisistedStates);
            if (_visitedStates.Count != prevStates)
            {
                LastSavedSchedule = runtime.EventPatternObserver.SavedEventTypes;
                SavedGenerators.Clear();
                SavedGenerators.Add(Generator);
                _numMutationsWithoutNewSaved = 0;
            }
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

    public HashSet<int> GetAllCoveredStates()
    {
        return _visitedStates;
    }

    public List<string> GetLastSavedScheduling()
    {
        return LastSavedSchedule;
    }
}