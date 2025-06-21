using SIEWlang.Core.Lexer;
using static SIEWlang.Core.Lexer.TokenType;
namespace SIEWlang.Core.Parser;

public class Parser
{
    private class ParseError : Exception { }

    private List<Token> Tokens;
    private int Current = 0;

    public Parser(List<Token> tokens)
    {
        Tokens = tokens;
    }

    // program → statement* EOF ;
    public List<Stmt> Parse()
    {
        List<Stmt> statements = new();
        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }
        return statements;
    }

    // declaration -> varDecl | statement
    private Stmt Declaration()
    {
        try
        {
            if (Match(VAR)) return VarDeclaration();

            return Statement();
        }
        catch (ParseError e)
        {
            Synchronize();
            return null;
        }
    }

    private Stmt VarDeclaration()
    {
        Token name = Consume(IDENTIFIER, "Expect variable name.");

        Expr? init = null;
        if (Match(EQUAL)){
            init = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after value."); // we separates the expressions using semiclone, this is the reason of the semiclone.
        return new Stmt.Var(name, init);
    }

    //statement → exprStmt | printStmt ;
    private Stmt Statement()
    {
        if (Match(PRINT)) return PrintStatement();

        if (Match(LEFT_BRACE)) return new Stmt.Block(Block());

        return ExpressionStatement();
    }

    private List<Stmt> Block()
    {
        var statements = new List<Stmt>();

        while (!Check(RIGHT_BRACE) && !IsAtEnd()) // the Check method do not consume the token, the IsAtEnd is to avoid infinite loops.
        {
            statements.Add(Declaration());
        }

        Consume(RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    private Stmt PrintStatement() 
    {
        // we consume the print token when we do the match in the statement method.
        Expr value = Expression();

        Consume(SEMICOLON, "Expect ';' after value."); // we separates the expressions using semiclone, this is the reason of the semiclone.
        return new Stmt.Print(value);
    }

    private Stmt ExpressionStatement()
    {
        Expr expr = Expression();

        Consume(SEMICOLON, "Expect ';' after value.");

        return new Stmt.Expression(expr);
    }

    // Expression -> Assignment
    private Expr Expression()
    {
        return Assignment();
    }

    // assigment -> IDENTIFIER "=" Assignment | Equality;

    private Expr Assignment()
    {
        // Attempt to parse the left-hand side of an assignment. This may consume an identifier
        // or any expression that could potentially be a valid assignment target (l-value).
        Expr expr = Equality();

        // Check if this is an assignment (e.g., using "="). At this point, we confirm
        // that the left-hand side was indeed an l-value candidate.
        if (Match(EQUAL))
        {
            Token equals = Previous();

            // Recursively parse the right-hand side. Assignment is right-associative,
            // so something like "a = b = c" should be interpreted as "a = (b = c)".
            // We re-enter the same Assignment rule to check if there's another assignment
            // on the right-hand side and handle it correctly.
            Expr value = Assignment();

            // Ensure the left-hand side is a valid assignment target (i.e., a variable).
            if (expr is Expr.Variable)
            {
                Token name = ((Expr.Variable)expr).Name;
                return new Expr.Assign(name, value);
            }

            // If not a variable, then it's an invalid assignment target.
            Error(equals, "Invalid assignment target.");
        }

        // If no assignment was matched, return the original expression.
        return expr;
    }

    // Equality -> Comparison (("!=" | "==") Comparison )*
    private Expr Equality()
    {
        Expr expr = Comparison();

        while(Match(BANG_EQUAL, EQUAL_EQUAL)) 
        {
            Token operatr = Previous(); // we use previus becase we made the desition of consume the token in the Match method.
            Expr right = Comparison();
            expr = new Expr.Binary(expr, operatr, right);
        }

        return expr;
    }

    // comparison → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
    private Expr Comparison()
    {
        Expr expr = Term();

        while(Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
        {
            Token operatr = Previous(); // we use previus becase we made the desition of consume the token in the Match method.
            Expr right = Term();
            expr = new Expr.Binary(expr, operatr, right);
        }

        return expr;
    }

    // term → factor ( ( "-" | "+" ) factor )* ;
    private Expr Term()
    {
        Expr expr = Factor();

        while (Match(MINUS, PLUS))
        {
            Token operatr = Previous(); // we use previus becase we made the desition of consume the token in the Match method.
            Expr right = Factor();
            expr = new Expr.Binary(expr, operatr, right);
        }

        return expr;
    }

    //factor → unary ( ( "/" | "*" ) unary )* ;

    private Expr Factor()
    {
        Expr expr = Unary();

        while (Match(SLASH, STAR))
        {
            Token operatr = Previous(); // we use previus becase we made the desition of consume the token in the Match method.
            Expr right = Unary();
            expr = new Expr.Binary(expr, operatr, right);
        }

        return expr;
    }

    // unary → ( "!" | "-" ) unary | primary ;
    private Expr Unary()
    {
        if(Match(BANG, MINUS))
        {
            Token operatr = Previous();
            Expr right = Unary();
            return new Expr.Unary(operatr, right);
        }

        return Primary();
    }

    //primary → NUMBER | STRING | "true" | "false" | "nil" | IDENTIFIER | "(" expression ")" ;
    private Expr Primary()
    {
        if (Match(FALSE)) return new Expr.Literal(false);
        if (Match(TRUE)) return new Expr.Literal(true);
        if (Match(NIL)) return new Expr.Literal(null);

        if(Match(NUMBER, STRING))
        {
            return new Expr.Literal(Previous().Literal); // it is the previus one because Match consume the token
        }

        if (Match(IDENTIFIER))
        {
            return new Expr.Variable(Previous());
        }

        if (Match(LEFT_PAREN))
        {
            Expr expr = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }

        throw Error(Peek(), "Expect expression.");
    }

    private Token Consume(TokenType type, string errorMessage)
    {
        if (Check(type)) return Advance(); // advance will advance first if possible, and always return the previous

        throw Error(Peek(), errorMessage);
    }

    private ParseError Error(Token token, String message)
    {
        Siew.Error(token, message);
        return new ParseError();
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().TokenType == SEMICOLON) return; // We use previus becase we advance before this loop / at the end of this loop. 

            switch (Peek().TokenType) // Since the previus one is not a semiclon, we can safely say that the current one is part of the faulty statemnt
            {
                case CLASS:
                case FN:
                case VAR:
                case FOR:
                case IF:
                case WHILE:
                case PRINT:
                case RETURN:
                    return;
            }

            Advance();
        }
    }

    private bool Match(params TokenType[] tokenTypes)
    {
        foreach (var tokenType in tokenTypes)
        {
            if (Check(tokenType))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().TokenType == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) Current++;
        return Previous();
    }

    private Token Previous()
    {
        return Tokens[Current - 1];
    }

    private Token Peek()
    {
        return Tokens[Current];
    }

    private bool IsAtEnd()
    {
        return Peek().TokenType == EOF;
    }
}
