using System;
using System.Collections.Generic;
using System.Linq;
using PChecker.Actors.EventQueues;
using PChecker.Actors.Events;
using PChecker.Feedback.EventMatcher;
using PChecker.SystematicTesting.Operations;

namespace PChecker.Generator;

internal class PatternGuidedScheduleGenerator : IScheduleGenerator<PatternGuidedScheduleGenerator>
{
    public NfaMatcher NfaMatcher;
    public PatternGuidedScheduleGenerator Mutate()
    {
        return this;
    }

    public PatternGuidedScheduleGenerator Copy()
    {
        return this;
    }

    public AsyncOperation? NextRandomOperation(List<AsyncOperation> enabledOperations, AsyncOperation current)
    {

        foreach (var enabledOperation in enabledOperations)
        {
            switch (enabledOperation)
            {
                case ActorOperation actor:
                {
                    if (actor.Type == AsyncOperationType.Stop || actor.Type == AsyncOperationType.Receive)
                    {
                        (DequeueStatus status, Event e, Guid opGroupId, EventInfo info)  = actor.Actor.Inbox.PeepNextEvent();
                        if (status == DequeueStatus.Success)
                        {

                        }
                    }
                }
                    break;
            }
        }

        if (enabledOperations.Count == 0)
        {
            return null;
        }

        if (enabledOperations.Count == 1)
        {
            return enabledOperations[0];
        }

        return enabledOperations[1];
    }

    public void PrepareForNextInput()
    {
    }
}