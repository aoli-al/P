using PChecker.Generator;
using PChecker.Random;

namespace PChecker.SystematicTesting.Strategies.Feedback;


internal class TwoStageFeedbackStrategy<TInput, TSchedule> : FeedbackGuidedStrategy<TInput, TSchedule>
    where TInput: IInputGenerator<TInput>
    where TSchedule: IScheduleGenerator<TSchedule>
{

    private int _scheduleMutationWithoutUpdates = 0;
    private readonly int _maxSchedulMutationsWithoutUpdates = 100;
    public TwoStageFeedbackStrategy(CheckerConfiguration checkerConfiguration, TInput input, TSchedule schedule) : base(checkerConfiguration, input, schedule)
    {
    }

    public override void ObserveRunningResults(ControlledRuntime runtime)
    {
        int numSavedInput = SavedGenerators.Count;
        base.ObserveRunningResults(runtime);
        if (SavedGenerators.Count != numSavedInput)
        {
            _scheduleMutationWithoutUpdates = 0;
        }
        else
        {
            _scheduleMutationWithoutUpdates += 1;
        }
    }

    protected override StrategyGenerator MutateGenerator(StrategyGenerator prev)
    {
        if (_scheduleMutationWithoutUpdates > _maxSchedulMutationsWithoutUpdates)
        {
            _scheduleMutationWithoutUpdates = 0;
            return new StrategyGenerator(
                Generator.InputGenerator.Mutate(),
                Generator.ScheduleGenerator.Mutate()
            );
        }
        else
        {
            return new StrategyGenerator(
                Generator.InputGenerator.Copy(),
                Generator.ScheduleGenerator.Mutate()
            );
        }
    }
}