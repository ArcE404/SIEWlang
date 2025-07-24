using SIEWlang.Core.Lexer;
using SIEWlang.Core.Parser;

namespace SIEWlang.Core.Semantics;

public class Resolver : Expr.IVisitor<object>, Stmt.IVisitor<object>
{
    private enum FunctionType
    {
        NONE,
        FUNCTION
    }

    private readonly Interpreter.Interpreter Interpreter;
    private Stack<Dictionary<string, bool>> Scopes = new();

    private FunctionType CurrentFunction = FunctionType.NONE;

    public Resolver(Interpreter.Interpreter interpreter)
    {
        Interpreter = interpreter;
    }

    public void Resolve(List<Stmt> statements)
    {
        foreach (var statement in statements)
        {
            Resolve(statement);
        }
    }

    private void ResolveFunction(Stmt.Function function, FunctionType type)
    {
        FunctionType enclosingFunction = CurrentFunction;
        CurrentFunction = type;
        BeginScope();

        foreach (Token param in function.Params)
        {
            Declare(param);
            Define(param);
        }

        Resolve(function.Body);

        EndScope();
        CurrentFunction = enclosingFunction;
    }

    public object VisitAssignExpr(Expr.Assign expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);

        // This function should conceptually return void.
        // However, because the visitor interface is generic, we cannot use void as the return type.
        // Therefore, we use object and return null, treating this function as if it were void.
        return null;
    }

    public object VisitBinaryExpr(Expr.Binary expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object VisitBlockStmt(Stmt.Block stmt)
    {
        BeginScope();
        Resolve(stmt.Statements);
        EndScope();

        // This function should conceptually return void.
        // However, because the visitor interface is generic, we cannot use void as the return type.
        // Therefore, we use object and return null, treating this function as if it were void.
        return null;
    }

    public object VisitCallExpr(Expr.Call expr)
    {
        Resolve(expr.Callee);

        foreach (Expr argument in expr.Arguments)
        {
            Resolve(argument);
        }

        return null;
    }

    public object VisitExpressionStmt(Stmt.Expression stmt)
    {
        Resolve(stmt.expression);
        return null;
    }

    public object VisitFunctionStmt(Stmt.Function stmt)
    {
        Declare(stmt.Name);

        // the reason why we define the funcion before reolve the funcion is becauase we need to allow the
        // function to refer itself insite the body (recution)
        Define(stmt.Name);

        ResolveFunction(stmt, FunctionType.FUNCTION);

        // This function should conceptually return void.
        // However, because the visitor interface is generic, we cannot use void as the return type.
        // Therefore, we use object and return null, treating this function as if it were void.
        return null;
    }

    public object VisitGroupingExpr(Expr.Grouping expr)
    {
        Resolve(expr.Expression);
        return null;
    }

    public object VisitIfStmt(Stmt.If stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.ThenBranch);
        if (stmt.ElseBranch != null) Resolve(stmt.ElseBranch);
        return null;
    }

    public object VisitLiteralExpr(Expr.Literal expr)
    {
        return null;
    }

    public object VisitLogicalExpr(Expr.Logical expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object VisitPrintStmt(Stmt.Print stmt)
    {
        Resolve(stmt.Expression);
        return null;
    }

    public object VisitReturnStmt(Stmt.Return stmt)
    {
        if (CurrentFunction == FunctionType.NONE)
        {
            Siew.Error(stmt.Keyword, "Can't return from top-level code.");
        }

        if (stmt.Value != null)
        {
            Resolve(stmt.Value);
        }

        return null;
    }

    public object VisitUnaryExpr(Expr.Unary expr)
    {
        Resolve(expr.Right);
        return null;
    }

    public object VisitVariableExpr(Expr.Variable expr)
    {
        if (Scopes.Count != 0 && Scopes.Peek().TryGetValue(expr.Name.Lexeme, out bool value) && !value)
        {
            Siew.Error(expr.Name, "Cannot read a local variable in its own initializer.");
        }

        ResolveLocal(expr, expr.Name);

        // This function should conceptually return void.
        // However, because the visitor interface is generic, we cannot use void as the return type.
        // Therefore, we use object and return null, treating this function as if it were void.
        return null;
    }

    public object VisitVarStmt(Stmt.Var stmt)
    {
        Declare(stmt.Name);
        if (stmt.Initializer is not null)
        {
            Resolve(stmt.Initializer);
        }

        Define(stmt.Name);

        // This function should conceptually return void.
        // However, because the visitor interface is generic, we cannot use void as the return type.
        // Therefore, we use object and return null, treating this function as if it were void.
        return null;
    }

    public object VisitWhileStmt(Stmt.While stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.Body);
        return null;
    }

    private void BeginScope()
    {
        Scopes.Push([]);
    }

    private void Declare(Token name)
    {
        if (Scopes.Count == 0) return;

        var scope = Scopes.Peek();

        if (scope.ContainsKey(name.Lexeme))
        {
            Siew.Error(name, "Already a variable with this name in this scope.");
        }

        Scopes.Peek()[name.Lexeme] = false;
    }

    private void Define(Token name)
    {
        if (Scopes.Count == 0) return;

        Scopes.Peek()[name.Lexeme] = true;
    }

    private void EndScope()
    {
        Scopes.Pop();
    }

    private void Resolve(Stmt statement)
    {
        statement.Accept(this);
    }

    private void Resolve(Expr expr)
    {
        expr.Accept(this);
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        for (int i = 0; i < Scopes.Count; i++)
        {
            if (Scopes.ElementAt(i).ContainsKey(name.Lexeme))
            {
                Interpreter.Resolve(expr, i);
                return;
            }
        }
    }
}