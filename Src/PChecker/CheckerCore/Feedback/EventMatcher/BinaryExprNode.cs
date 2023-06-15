namespace PChecker.Feedback.EventMatcher;

public class BinaryExprNode : BaseNode
{
    public enum Op
    {
        ALTERNATION,
        SEQUENCE,
    }

    public Op Operator;
    public BaseNode Left;
    public BaseNode Right;

    public BinaryExprNode(Op op, BaseNode left, BaseNode right)
    {
        Operator = op;
        Left = left;
        Right = right;
    }
}
