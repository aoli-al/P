using System.Collections.Generic;
using PChecker.SystematicTesting.Operations;

namespace PChecker.Random;

internal interface IScheduleGenerator<T>: IGenerator<T>
{
    public AsyncOperation? NextRandomOperation(List<AsyncOperation> enabledOperations);

}