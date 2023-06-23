using System.Collections.Generic;
using System.Linq;
using PChecker.Generator;
using PChecker.SystematicTesting.Operations;

namespace PChecker.SystematicTesting.Strategies.Feedback;

internal class UnbiasedSchedulingStrategy<TInput, TSchedule> : FeedbackGuidedStrategy<TInput, TSchedule>
    where TInput: IInputGenerator<TInput>
    where TSchedule: IScheduleGenerator<TSchedule>
{
    private string _prefix = "";
    private Dictionary<string, int> _coverage = new();
    private bool _shouldSaveThisScheduling = false;


    public override bool GetNextOperation(AsyncOperation current, IEnumerable<AsyncOperation> ops, out AsyncOperation next)
    {
        if (_prefix.Length != 0)
        {
            var enabledOperations = ops.Where(op => op.Status is AsyncOperationStatus.Enabled).ToList();
            if (enabledOperations.Count > _coverage.GetValueOrDefault(_prefix, 1))
            {
                _shouldSaveThisScheduling = true;
                _coverage[_prefix] = enabledOperations.Count;
            }
        }
        var result = base.GetNextOperation(current, ops, out next);
        if (result)
        {
            _prefix += "," + next.Name;
        }

        return result;
    }

    public UnbiasedSchedulingStrategy(CheckerConfiguration checkerConfiguration, TInput input, TSchedule schedule) : base(checkerConfiguration, input, schedule)
    {
    }

    public override void ObserveRunningResults(ControlledRuntime runtime)
    {
        if (_shouldSaveThisScheduling)
        {
            SavedGenerators.Add(Generator);
        }
        // base.ObserveRunningResults(runtime);
    }

    public override bool PrepareForNextIteration()
    {
        _prefix = "";
        return base.PrepareForNextIteration();
    }
}