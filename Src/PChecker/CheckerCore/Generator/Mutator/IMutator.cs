using PChecker.Random;

namespace PChecker.SystematicTesting.Strategies.Feedback.Mutator;

public interface IMutator<T>
{
    T Mutate(T prev);
}