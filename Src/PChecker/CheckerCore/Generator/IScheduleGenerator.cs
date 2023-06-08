using System.Collections.Generic;
using PChecker.SystematicTesting.Operations;

namespace PChecker.Generator;

internal interface IScheduleGenerator<T>: IGenerator<T>
{
    public AsyncOperation? NextRandomOperation(List<AsyncOperation> enabledOperations, AsyncOperation current);


    public void PrepareForNextInput();
}