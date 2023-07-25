using System.Collections.Generic;
using PChecker.Feedback;
using PChecker.Feedback.EventMatcher;

namespace PChecker.SystematicTesting.Strategies.Feedback;

internal interface IFeedbackGuidedStrategy: ISchedulingStrategy
{
    public void ObserveRunningResults(CfgEventPatternObserver patternObserver, ControlledRuntime runtime);
    public int TotalSavedInputs();
    public int CurrentInputIndex();
    public int GetAllCoveredStates();
    public List<string> GetLastSavedScheduling();
}