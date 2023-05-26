namespace PChecker.Random;

public interface IGenerator<T>
{
    T Mutate();

    T Copy();
}