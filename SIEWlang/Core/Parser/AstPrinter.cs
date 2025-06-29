using System.Text;
using SIEWlang.Core.Lexer;
using static SIEWlang.Core.Parser.Expr;

namespace SIEWlang.Core.Parser;

internal class AstPrinter : IVisitor<string>
{
    public string Print(Expr expr)
    {
        return expr.Accept(this);
    }

    public string VisitAssignExpr(Assign expr)
    {
        throw new NotImplementedException();
    }

    public string VisitBinaryExpr(Binary expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right); // this seems to be the reason why in js we can do somthing like 1 + "1"...
    }

    public string VisitGroupingExpr(Grouping expr) 
    {
        return Parenthesize("group", expr.Expression);
    }

    public string VisitLiteralExpr(Literal expr)
    {
        if (expr.Value == null) return "nil";

        return expr.Value.ToString();
    }

    public string VisitLogicalExpr(Logical expr)
    {
        throw new NotImplementedException();
    }

    public string VisitUnaryExpr(Unary expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Right);
    }

    public string VisitVariableExpr(Variable expr)
    {
        throw new NotImplementedException();
    }

    private string Parenthesize(string name, params Expr[] exprs)
    {
        StringBuilder stringBuilder = new();

        stringBuilder.Append("(").Append(name);
        foreach (Expr e in exprs)
        {
            stringBuilder.Append(" ");
            stringBuilder.Append(e.Accept(this));
        }
        stringBuilder.Append(")");

        return stringBuilder.ToString();
    }
}
