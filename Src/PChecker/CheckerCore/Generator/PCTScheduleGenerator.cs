using System.Collections.Generic;
using PChecker.SystematicTesting.Operations;

namespace PChecker.Generator;

internal sealed class PCTScheduleGenerator: IScheduleGenerator<PCTScheduleGenerator>
{
    public PCTScheduleGenerator Mutate()
    {
        throw new System.NotImplementedException();
    }

    public PCTScheduleGenerator Copy()
    {
        throw new System.NotImplementedException();
    }

    public AsyncOperation? NextRandomOperation(List<AsyncOperation> enabledOperations)
    {
        throw new System.NotImplementedException();
    }
}