using System.Collections.Generic;
using System.Runtime.InteropServices;
using Plang.Compiler.TypeChecker.AST;
using Plang.Compiler.TypeChecker.AST.Declarations;
using Plang.Compiler.TypeChecker.AST.Expressions;
using Plang.Compiler.TypeChecker.AST.Statements;

namespace Plang.Compiler.TypeChecker;

public class AffectedStatesAnalyzer
{
    private ITranslationErrorHandler _handler;
    private Function _function;

    private AffectedStatesAnalyzer(ITranslationErrorHandler handler, Function function)
    {
        _handler = handler;
        _function = function;
    }

    public static void AnalyzeMachineMethods(ITranslationErrorHandler handler, IEnumerable<Function> allFunctions)
    {
        foreach (var function in allFunctions)
        {
            var analyzer = new AffectedStatesAnalyzer(handler, function);
            analyzer.Analyze();
        }
    }

    private void Analyze()
    {
        if (_function.IsForeign)
        {
            return;
        }
        AnalyzeStmt(_function.Body);
    }

    private void AnalyzeStmt(IPStmt stmt)
    {
        switch (stmt)
        {
            case CompoundStmt compoundStmt:
                foreach (var subStmt in compoundStmt.Statements)
                {
                    AnalyzeStmt(subStmt);
                }

                break;
            case AssignStmt assignStmt:
                AnalyzeExpr(assignStmt.Location, true);
                AnalyzeExpr(assignStmt.Value, false);
                break;
            case AddStmt addStmt:
                AnalyzeExpr(addStmt.Variable, true);
                AnalyzeExpr(addStmt.Value, false);
                break;
            case AnnounceStmt announceStmt:
                AnalyzeExpr(announceStmt.Payload, false);
                AnalyzeExpr(announceStmt.PEvent, false);
                break;
            case AssertStmt assertStmt:
                AnalyzeExpr(assertStmt.Assertion, false);
                AnalyzeExpr(assertStmt.Message, false);
                break;
            case CtorStmt ctorStmt:
                foreach (var arg in ctorStmt.Arguments)
                {
                    AnalyzeExpr(arg, false);
                }

                break;
            case ForeachStmt foreachStmt:
                AnalyzeExpr(foreachStmt.IterCollection, false);
                AnalyzeStmt(foreachStmt.Body);
                break;
            case FunCallStmt funCallStmt:
                foreach (var arg in funCallStmt.ArgsList)
                {
                    AnalyzeExpr(arg, false);
                }

                break;
            case IfStmt ifStmt:
                AnalyzeExpr(ifStmt.Condition, false);
                AnalyzeStmt(ifStmt.ThenBranch);
                AnalyzeStmt(ifStmt.ElseBranch);
                break;
            case InsertStmt insertStmt:
                AnalyzeExpr(insertStmt.Variable, true);
                AnalyzeExpr(insertStmt.Value, false);
                AnalyzeExpr(insertStmt.Index, false);
                break;
            case MoveAssignStmt moveAssignStmt:
                AnalyzeExpr(moveAssignStmt.ToLocation, true);
                AnalyzeVariable(moveAssignStmt.FromVariable, false);
                break;
            case RemoveStmt removeStmt:
                AnalyzeExpr(removeStmt.Variable, true);
                AnalyzeExpr(removeStmt.Value, false);
                break;
            case ReturnStmt returnStmt:
                AnalyzeExpr(returnStmt.ReturnValue, false);
                break;
            case SwapAssignStmt swapAssignStmt:
                AnalyzeExpr(swapAssignStmt.NewLocation, true);
                AnalyzeVariable(swapAssignStmt.OldLocation, true);
                break;
            case WhileStmt whileStmt:
                AnalyzeExpr(whileStmt.Condition, false);
                AnalyzeStmt(whileStmt.Body);
                break;
        }
    }

    private void AnalyzeExpr(IPExpr expr, bool isLhsExpr)
    {
        switch (expr)
        {
            case MapAccessExpr mapAccessExpr:
                AnalyzeExpr(mapAccessExpr.MapExpr, isLhsExpr);
                AnalyzeExpr(mapAccessExpr.IndexExpr, false);
                break;
            case NamedTupleAccessExpr namedTupleAccessExpr:
                AnalyzeExpr(namedTupleAccessExpr.SubExpr, isLhsExpr);
                break;
            case VariableAccessExpr variableAccessExpr:
                AnalyzeVariable(variableAccessExpr.Variable, isLhsExpr);
                break;
            case BinOpExpr binOpExpr:
                AnalyzeExpr(binOpExpr.Lhs, false);
                AnalyzeExpr(binOpExpr.Lhs, false);
                break;
            case CastExpr castExpr:
                AnalyzeExpr(castExpr.SubExpr, false);
                break;
            case ChooseExpr chooseExpr:
                AnalyzeExpr(chooseExpr.SubExpr, false);
                break;
            case CoerceExpr coerceExpr:
                AnalyzeExpr(coerceExpr.SubExpr, false);
                break;
            case ContainsExpr containsExpr:
                AnalyzeExpr(containsExpr.Collection, false);
                AnalyzeExpr(containsExpr.Item, false);
                break;
            case CtorExpr ctorExpr:
                foreach (var arg in ctorExpr.Arguments)
                {
                    AnalyzeExpr(arg, false);
                }
                break;
            case FunCallExpr funCallExpr:
                foreach (var arg in funCallExpr.Arguments)
                {
                    AnalyzeExpr(arg, false);
                }
                break;
            case IVariableRef variableRef:
                AnalyzeVariable(variableRef.Variable, isLhsExpr);
                break;
            case KeysExpr keysExpr:
                AnalyzeExpr(keysExpr.Expr, false);
                break;
            case NamedTupleExpr namedTupleExpr:
                foreach (var fields in namedTupleExpr.TupleFields)
                {
                    AnalyzeExpr(fields, false);
                }
                break;
            case SeqAccessExpr seqAccessExpr:
                AnalyzeExpr(seqAccessExpr.SeqExpr, isLhsExpr);
                AnalyzeExpr(seqAccessExpr.IndexExpr, false);
                break;
            case SetAccessExpr setAccessExpr:
                AnalyzeExpr(setAccessExpr.SetExpr, isLhsExpr);
                AnalyzeExpr(setAccessExpr.IndexExpr, false);
                break;
            case SizeofExpr sizeofExpr:
                AnalyzeExpr(sizeofExpr.Expr, false);
                break;
            case TupleAccessExpr tupleAccessExpr:
                AnalyzeExpr(tupleAccessExpr.SubExpr, isLhsExpr);
                break;
            case UnaryOpExpr unaryOpExpr:
                AnalyzeExpr(unaryOpExpr.SubExpr, isLhsExpr);
                break;
            case UnnamedTupleExpr unnamedTupleExpr:
                foreach (var field in unnamedTupleExpr.TupleFields)
                {
                    AnalyzeExpr(field, false);
                }
                break;
            case ValuesExpr valuesExpr:
                AnalyzeExpr(valuesExpr.Expr, false);
                break;
        }
    }


    private void AnalyzeVariable(Variable variable, bool isLhsExpr)
    {
        if (variable.Role == VariableRole.Field)
        {
            if (isLhsExpr)
            {
                _function.WriteFields.Add(variable);
            }
            else
            {
                _function.ReadFields.Add(variable);
            }
        }
    }
}