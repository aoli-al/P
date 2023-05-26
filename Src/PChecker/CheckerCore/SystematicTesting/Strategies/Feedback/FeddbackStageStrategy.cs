using PChecker.Random;

namespace PChecker.SystematicTesting.Strategies.Feedback;


internal class TwoStageFeedbackStrategy<TInput, TSchedule> : FeedbackGuidedStrategy<TInput, TSchedule>
    where TInput: IInputGenerator<TInput>, new()
    where TSchedule: IScheduleGenerator<TSchedule>, new()
{

    private int _scheduleMutationWithoutUpdates = 0;
    private readonly int _maxSchedulMutationsWithoutUpdates = 100;
    public TwoStageFeedbackStrategy(CheckerConfiguration checkerConfiguration) : base(checkerConfiguration)
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
                new TSchedule()
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