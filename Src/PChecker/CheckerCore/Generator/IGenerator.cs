namespace PChecker.Generator;

public interface IGenerator<T>
{
    T Mutate();

    T Copy();
}