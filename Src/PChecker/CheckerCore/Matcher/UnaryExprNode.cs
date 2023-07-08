namespace PChecker.Matcher;


public class UnaryExprNode : BaseNode
{
    public enum Op
    {
        STAR,
        MAYBE
    }

    public Op Operator;
    public BaseNode Expression;

    public UnaryExprNode(Op op, BaseNode expression)
    {
        Operator = op;
        Expression = expression;
    }
}