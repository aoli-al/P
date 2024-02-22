using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using PChecker.Actors;


enum State {
    PosReachR,
    PosExctdW,
    PosNoInfo,
    PosSat,
    NegExctdW,
    NegReachW,
    NegOtherW,
    NegNoInfo,
    NegUnsat,
}

public record Constraint(Operation op1, Operation op2, bool positive)
{
    public override string ToString()
    {
        return $"({op1}, {op2}, {positive})";
    }
}
public record AbstractSchedule(HashSet<Constraint> constraints) {

    public void Mutate(HashSet<Constraint> allConstraints)
    {
        HashSet<Constraint> constraints = new();
        
    }
}