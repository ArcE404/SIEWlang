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

    public Expr Parse()
    {
        try
        {
            return Expression();
        }
        catch (ParseError error)
        {
            return null;
        }
    }

    // Expression -> Equality
    private Expr Expression()
    {
        return Equality();
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

    //primary → NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")" ;
    private Expr Primary()
    {
        if (Match(FALSE)) return new Expr.Literal(false);
        if (Match(TRUE)) return new Expr.Literal(true);
        if (Match(NIL)) return new Expr.Literal(null);

        if(Match(NUMBER, STRING))
        {
            return new Expr.Literal(Previous().Literal); // it is the previus one because Match consume the token
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
