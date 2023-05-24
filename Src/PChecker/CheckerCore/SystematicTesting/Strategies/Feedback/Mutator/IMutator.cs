using PChecker.Random;

namespace PChecker.SystematicTesting.Strategies.Feedback.Mutator;

public interface IMutator
{
    StreamBasedValueGenerator Mutate(StreamBasedValueGenerator prev);
}