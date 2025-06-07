using SIEWlang.Core.Lexer;
using SIEWlang.Core.Parser;
using static SIEWlang.Core.Lexer.TokenType;

namespace SIEWlang.Core.Interpreter;

public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
{

    // a list of statements is considered a Program.
    // program → statement* EOF;
    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (var stmt in statements)
            {
                Execute(stmt);
            }
        }
        catch (RuntimeError error)
        {
            Siew.RuntimeError(error);
        }
    }
    public object VisitBinaryExpr(Expr.Binary expr)
    {
        object left = Evaluate(expr.Left);
        object right = Evaluate(expr.Right);

        switch (expr.Operator.TokenType)
        {
            case BANG_EQUAL: return !IsEqual(left, right);
            case EQUAL_EQUAL: return IsEqual(left, right);
            case GREATER:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left > (double)right;
            case GREATER_EQUAL:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left >= (double)right;
            case LESS:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left < (double)right;
            case LESS_EQUAL:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left <= (double)right;
            case MINUS:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left - (double)right;
            case PLUS:
                {
                    if (left is double && right is double)
                    {
                        return (double)left + (double) right;
                    }

                    if(left is string && right is string)
                    {
                        return (string)left + (string)right;
                    }

                    throw new RuntimeError(expr.Operator,
                                "Operands must be two numbers or two strings.");
                }
            case SLASH:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left / (double)right;
            case STAR:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left * (double)right;
        }


        // unreachable
        return null;
    }

    public object VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.Expression);
    }

    public object VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value;
    }
    // "...our interpreter is doing a post-order traversal—each node evaluates its children before doing its own work."
    public object VisitUnaryExpr(Expr.Unary expr)
    {
        object rigth = Evaluate(expr.Right);

        switch (expr.Operator.TokenType)
        {
            // this is the core of the statically languajes.
            // The user dosent care the type, the interpreter will take care of it here at run time
            case MINUS:
                CheckNumberOperant(expr.Operator, rigth); 
                return -(double)rigth;
            case BANG:
                return !IsTruthy(rigth);
        }

        // Unreacheble
        return null;
    }

    private string Stringify(object value)
    {
        if (value == null) return "nil";

        if (value is double) {
            string? text = value.ToString();
            if (text.EndsWith(".0"))
            {
                text = text.Substring(0, text.Length - 2);
            }

            return text;
        }

        return value.ToString();
    }

    private object Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    // this is the same truthy as ruby's truthy
    private bool IsTruthy(object expr)
    {
        if (expr == null) return false;
        if (expr is bool) return (bool)expr;
        return true;
    }

    private bool IsEqual(object left, object right)
    {
        if (left == null && right == null) return true;
        if (left == null) return false;

        return left.Equals(right);
    }

    // we check that the types for the kind of operator is 
    private void CheckNumberOperant(Token operatr, object operand)
    {
        if (operand is double) return;
        throw new RuntimeError(operatr, "Operand must be a number");
    }

    private void CheckNumberOperands(Token operatr,
                                  Object left, Object right)
    {
        if (left is double && right is double) return;

        throw new RuntimeError(operatr, "Operands must be numbers.");
    }

    private void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }

    public object VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.expression);

        // this function should be void, but since the interface is generic, we cannot use void as a parameter 
        // to implement the interface. Therefore, we use object and return null and we treat this function as a void function.
        return null;
    }

    public object VisitPrintStmt(Stmt.Print stmt)
    {
        object value = Evaluate(stmt.Expression);

        Console.WriteLine(Stringify(value));

        // this function should be void, but since the interface is generic, we cannot use void as a parameter 
        // to implement the interface. Therefore, we use object and return null and we treat this function as a void function.
        return null;
    }
}
