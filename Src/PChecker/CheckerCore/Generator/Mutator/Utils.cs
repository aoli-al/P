using System;

namespace PChecker.Generator.Mutator;

public class Utils
{
    public static int SampleGeometric(double mean, System.Random random) {
        double p = 1 / mean;
        double uniform = random.NextDouble();
        return (int) Math.Ceiling(Math.Log(1 - uniform) / Math.Log(1 - p));
    }
}