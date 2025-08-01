﻿using SIEWlang.Core.Callable;
using SIEWlang.Core.Lexer;
using SIEWlang.Core.Parser;
using static SIEWlang.Core.Lexer.TokenType;

namespace SIEWlang.Core.Interpreter;

public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
{
    public Environment _globals = new Environment(); // this is the outer most enviroment, we hold here the native funcitons
    private Environment _currentEnvironment; // this enviroment changes at runtime, it will ends always at the outermost, becasuse at that leve is globals
    private Dictionary<Expr, int> _locals = [];

    public Interpreter()
    {
        _currentEnvironment = _globals;
        _globals.Define("clock", new NativeClockFunction());
    }

    public void ExecuteBlock(List<Stmt> statements, Environment environment)
    {
        Environment previous = _currentEnvironment;
        try
        {
            _currentEnvironment = environment;
            foreach (var stmt in statements)
            {
                Execute(stmt);
            }
        }
        finally
        {
            _currentEnvironment = previous;
        }
    }

    // A list of statements is considered a program.
    // Grammar: program → statement* EOF;
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

    public void Resolve(Expr expr, int depth)
    {
        _locals[expr] = depth;
    }

    public object VisitAssignExpr(Expr.Assign expr)
    {
        object value = Evaluate(expr.Value);

        if (_locals.TryGetValue(expr, out int distance))
        {
            _currentEnvironment.AssingAt(distance, expr.Name, value);
        }
        else
        {
            _globals.Assing(expr.Name, value);
        }

        return value; // the reason why we return this is because assing is an expression that can be nested inside other like "print a = 2";
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
                        return (double)left + (double)right;
                    }

                    // this is how is shown in the tutorial
                    /*                  if (left is string && right is string)
                                        {
                                            return (string)left + (string)right;
                                        }
                    */

                    // this is my implementation to allow string concatenation with string and other types
                    if (left is string || right is string)
                    {
                        return Stringify(left) + Stringify(right);
                    }

                    throw new RuntimeError(expr.Operator,
                                "RunTimeError: Operands must be two numbers or two strings.");
                }
            case SLASH:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left / (double)right;

            case STAR:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left * (double)right;
        }

        // Unreachable
        return null;
    }

    public object VisitBlockStmt(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.Statements, new Environment(_currentEnvironment)); // we pass the global enviroment to the block enviroment.

        // This function should conceptually return void.
        // However, because the visitor interface is generic, we cannot use void as the return type.
        // Therefore, we use object and return null, treating this function as if it were void.
        return null;
    }

    public object VisitCallExpr(Expr.Call expr) // this expression is when we call something callable, like a function, class... etc
    {
        object callee = Evaluate(expr.Callee);

        List<object> arguments = [];

        foreach (var argument in expr.Arguments)
        {
            arguments.Add(Evaluate(argument)); // we are here evaluating the arguments in the order they are declared. A subtle semantic choice here then.
        }

        if (callee is not ISiewCallable)
        {
            throw new RuntimeError(expr.Paren, "Can only call functions and classes");
        }

        ISiewCallable function = (ISiewCallable)callee;

        if (arguments.Count != function.Arity())
        {
            throw new RuntimeError(expr.Paren, $"Expected {function.Arity()} arguments but got {arguments.Count} instead.");
        }

        return function.Call(this, arguments);
    }

    public object VisitClassStmt(Stmt.Class stmt)
    {
        _currentEnvironment.Define(stmt.Name.Lexeme, null); // we define it before

        SiewClass klass = new SiewClass(stmt.Name.Lexeme); // so we can refer to the same class later inside the class

        _currentEnvironment.Assing(stmt.Name, klass); // we assing the result after

        // This function should conceptually return void.
        // However, because the visitor interface is generic, we cannot use void as the return type.
        // Therefore, we use object and return null, treating this function as if it were void.
        return null;
    }

    public object VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.expression);

        // This function should conceptually return void.
        // However, because the visitor interface is generic, we cannot use void as the return type.
        // Therefore, we use object and return null, treating this function as if it were void.
        return null;
    }

    public object VisitFunctionStmt(Stmt.Function stmt) // this node is executed in the interpreter when te funcion is declared
    {
        SiewFunction function = new(stmt, _currentEnvironment);

        _currentEnvironment.Define(stmt.Name.Lexeme, function);

        // This function should conceptually return void.
        // However, because the visitor interface is generic, we cannot use void as the return type.
        // Therefore, we use object and return null, treating this function as if it were void.
        return null;
    }

    public object VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.Expression);
    }

    public object VisitIfStmt(Stmt.If stmt)
    {
        if (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.ThenBranch);
        }
        else if (IsTruthy(stmt.ElseBranch))
        {
            Execute(stmt.ElseBranch);
        }

        // This function should conceptually return void.
        // However, because the visitor interface is generic, we cannot use void as the return type.
        // Therefore, we use object and return null, treating this function as if it were void.
        return null;
    }

    public object VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value;
    }

    public object VisitLogicalExpr(Expr.Logical expr)
    {
        object left = Evaluate(expr.Left);

        if (expr.Operator.TokenType == OR)
        {
            if (IsTruthy(left)) return left; // if an or encounter a true value at first, dont do more.
        }
        else
        {
            if (!IsTruthy(left)) return left; // if an and encounter a false value at fist, dont do more.
        }

        return Evaluate(expr.Right);
    }

    public object VisitPrintStmt(Stmt.Print stmt)
    {
        object value = Evaluate(stmt.Expression);

        if (stmt.PrintType.TokenType == PRINTL)
        {
            Console.WriteLine(Stringify(value));
        }
        else
        {
            Console.Write(Stringify(value));
        }

        // This function should conceptually return void.
        // However, because the visitor interface is generic, we cannot use void as the return type.
        // Therefore, we use object and return null, treating this function as if it were void.
        return null;
    }

    public object VisitReturnStmt(Stmt.Return stmt)
    {
        object? value = null;

        if (stmt.Value is not null)
        {
            value = Evaluate(stmt.Value);
        }

        throw new Return(value);
    }

    // "Our interpreter performs a post-order traversal—
    // each node evaluates its children before performing its own computation."
    public object VisitUnaryExpr(Expr.Unary expr)
    {
        object rigth = Evaluate(expr.Right);

        switch (expr.Operator.TokenType)
        {
            // This is one of the key mechanisms of dynamically-typed languages.
            // The user does not specify types explicitly— the interpreter handles type logic at runtime.
            case MINUS:
                CheckNumberOperant(expr.Operator, rigth);
                return -(double)rigth;

            case BANG:
                return !IsTruthy(rigth);
        }

        // Unreachable
        return null;
    }

    public object VisitVariableExpr(Expr.Variable expr)
    {
        return LookUpVariable(expr.Name, expr);
    }

    public object VisitVarStmt(Stmt.Var stmt)
    {
        object value = null;

        if (stmt.Initializer != null)
        {
            value = Evaluate(stmt.Initializer);
        }

        _currentEnvironment.Define(stmt.Name.Lexeme, value);

        // This function should conceptually return void.
        // However, because the visitor interface is generic, we cannot use void as the return type.
        // Therefore, we use object and return null, treating this function as if it were void.
        return null;
    }

    public object VisitWhileStmt(Stmt.While stmt)
    {
        while (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.Body);
        }

        // This function should conceptually return void.
        // However, because the visitor interface is generic, we cannot use void as the return type.
        // Therefore, we use object and return null, treating this function as if it were void.
        return null;
    }

    // Checks that both operands are numbers for binary operators that require numerical operands.
    private void CheckNumberOperands(Token operatr,
                                  Object left, Object right)
    {
        if (left is double && right is double) return;

        throw new RuntimeError(operatr, "RunTimeError: Operands must be numbers.");
    }

    // Checks that the operand is a number for unary operators.
    private void CheckNumberOperant(Token operatr, object operand)
    {
        if (operand is double) return;
        throw new RuntimeError(operatr, "RunTimeError: Operand must be a number");
    }

    private object Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    private void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }

    private bool IsEqual(object left, object right)
    {
        if (left == null && right == null) return true;
        if (left == null) return false;

        return left.Equals(right);
    }

    // This implements the same truthy concept as in Ruby.
    // In this language, only nil and false are falsy. Everything else is truthy.
    private bool IsTruthy(object expr)
    {
        if (expr == null) return false;
        if (expr is bool) return (bool)expr;
        return true;
    }

    private object LookUpVariable(Token name, Expr expr)
    {
        if (_locals.TryGetValue(expr, out var distance))
        {
            return _currentEnvironment.GetAt(name.Lexeme, distance);
        }

        return _globals.Get(name);
    }

    private string Stringify(object value)
    {
        if (value == null) return "nil";

        if (value is double)
        {
            string? text = value.ToString();
            if (text.EndsWith(".0"))
            {
                text = text.Substring(0, text.Length - 2);
            }

            return text;
        }

        return value.ToString();
    }
}