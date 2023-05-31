using System;
using PChecker.Generator.Object;

namespace PChecker.Generator.Mutator;

public class Utils
{
    public static int SampleGeometric(double p, double random) {
        return (int) Math.Ceiling(Math.Log(1 - random) / Math.Log(1 - p));
    }
    public static RandomChoices<T> MutateRandomChoices<T> (RandomChoices<T> randomChoices, int meanMutationCount, int meanMutationSize, System.Random random)
        where T: IConvertible
    {
        RandomChoices<T> newChoices = new RandomChoices<T>(randomChoices);
        int mutations = Utils.SampleGeometric(1.0f / meanMutationCount, random.NextDouble());

        while (mutations-- > 0)
        {
            int offset = random.Next(newChoices.Data.Count);
            int mutationSize = Utils.SampleGeometric(meanMutationSize, random.NextDouble());
            for (int i = 0; i < offset + mutationSize; i++)
            {
                if (i >= newChoices.Data.Count)
                {
                    break;
                }

                newChoices.Data[i] = newChoices.GenerateNew();
            }
        }

        return newChoices;
    }
}