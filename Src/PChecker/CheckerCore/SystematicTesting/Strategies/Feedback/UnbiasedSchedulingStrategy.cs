using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using PChecker.Feedback.EventMatcher;
using PChecker.Generator;
using PChecker.SystematicTesting.Operations;
using PChecker.SystematicTesting.Traces;

namespace PChecker.SystematicTesting.Strategies.Feedback;

internal class UnbiasedSchedulingStrategy<TInput, TSchedule> : FeedbackGuidedStrategy<TInput, TSchedule>
    where TInput: IInputGenerator<TInput>
    where TSchedule: IScheduleGenerator<TSchedule>
{

    private Nfa _nfa;


    public override bool GetNextOperation(AsyncOperation current, IEnumerable<AsyncOperation> ops, out AsyncOperation next)
    {
        var highPriority = _nfa.FindHighPriorityOperations(ops);
        var result = base.GetNextOperation(current, highPriority, out next);
        return result;
    }

    public UnbiasedSchedulingStrategy(CheckerConfiguration checkerConfiguration, TInput input, TSchedule schedule) : base(checkerConfiguration, input, schedule)
    {
    }

    public override void SetNFA(Nfa nfa)
    {
        _nfa = nfa;
    }
}