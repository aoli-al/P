using System.Collections.Generic;
using Antlr4.Runtime.Atn;

namespace PChecker.Feedback.EventMatcher;

public class EventLangVisitor : EventLangBaseVisitor<BaseNode>
{
    public override BaseNode VisitEventObj(EventLangParser.EventObjContext context)
    {
        string eventName = context.@event().Iden().GetText();
        Dictionary<string, string> constraints = new();
        if (context.@event().eventDescList() != null)
        {
            foreach (var desc in context.@event().eventDescList().eventDesc())
            {
                var text = desc.StringLiteral().GetText();

                constraints[desc.Iden().GetText()] = text.Substring(1, text.Length - 2);
            }
        }

        return new EventNode(eventName, constraints);
    }

    public override BaseNode VisitAnyExpr(EventLangParser.AnyExprContext context)
    {
        return new EventNode(".", new());
    }

    public override BaseNode VisitUnaryExpr(EventLangParser.UnaryExprContext context)
    {
        var node = Visit(context.exp());
        if (context.PLUS() != null)
        {
            return new BinaryExprNode(BinaryExprNode.Op.SEQUENCE, node,
                new UnaryExprNode(UnaryExprNode.Op.STAR, node)
            );
        }

        if (context.MAYBE() != null)
        {
            return new BinaryExprNode(BinaryExprNode.Op.ALTERNATION, EventNode.Epsilon, node);
        }

        return new UnaryExprNode(UnaryExprNode.Op.STAR, node);
    }

    public override BaseNode VisitAlterExpr(EventLangParser.AlterExprContext context)
    {
        return new BinaryExprNode(BinaryExprNode.Op.ALTERNATION, Visit(context.exp(0)), Visit(context.exp(1)));
    }

    public override BaseNode VisitSeqExpr(EventLangParser.SeqExprContext context)
    {
        return new BinaryExprNode(BinaryExprNode.Op.SEQUENCE, Visit(context.exp(0)), Visit(context.exp(1)));
    }

    public override BaseNode VisitGroupExpr(EventLangParser.GroupExprContext context)
    {
        return Visit(context.exp());
    }
}