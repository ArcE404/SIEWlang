using SIEWlang.Core.Lexer;
using static SIEWlang.Core.Lexer.TokenType;
using static SIEWlang.Core.Parser.Expr;

namespace SIEWlang.Core.Parser;

public class Parser
{
    private int Current = 0;

    private List<Token> Tokens;

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

    private Stmt ForStatement()
    {
        Consume(LEFT_PAREN, "Expect '(' after 'for'.");
        
        // the for loop need an initializer 
        Stmt? initializer;
        if (Match(SEMICOLON))
        {
            initializer = null;
        }else if (Match(VAR))
        {
            // it can be the variable
            initializer = VarDeclaration();
        }
        else
        {
            // or it can be an expression
            initializer = ExpressionStatement();
        }

        Expr? condition = null;
        if (!Check(SEMICOLON))
        {
            condition = Expression();
        }
        Consume(SEMICOLON, "Expect ';' after loop condition.");

        Expr? increment = null;
        if (!Check(RIGHT_PAREN))
        {
            increment = Expression();
        }

        Consume(RIGHT_PAREN, "Expect ')' after for clauses.");

        Stmt body = Statement();

        // Time to desugar the `for` loop into a `while` loop.
        // We do this transformation from the inside out, starting from the increment.

        if (increment is not null)
        {
            // We wrap the original body inside a block,
            // and append the increment expression to the end of it.
            // This ensures the increment runs after each iteration.
            body = new Stmt.Block(
            [
                body,                        // loop body
                new Stmt.Expression(increment) // increment expression
            ]);
        }

        // If no condition is provided, treat it as `true` (infinite loop).
        condition ??= new Expr.Literal(true);

        // Now, wrap the updated body into a `while` loop with the condition.
        body = new Stmt.While(condition, body);

        // If there's an initializer, it should run before the loop starts.
        // So we create a block that runs the initializer once, and then enters the loop.
        if (initializer is not null)
        {
            // This block also ensures that any variables declared in the initializer
            // stay scoped to the loop only.
            body = new Stmt.Block(
            [
                initializer,
                body
        ]);
        }

        // In the end, we’ve transformed:
        //
        //   for (var i = 0; i < 5; i = i + 1) {
        //       doSomething();
        //   }
        //
        // Into something like:
        //
        //   {
        //       var i = 0;
        //       while (i < 5) {
        //           doSomething();
        //           i = i + 1;
        //       }
        //   }

        return body;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) Current++;
        return Previous();
    }

    private Expr And()
    {
        Expr expr = Equality();

        while (Match(AND))
        {
            Token operatr = Previous();
            Expr right = Equality();
            expr = new Expr.Logical(expr, operatr, right);
        }

        return expr;
    }

    private Expr Assignment()
    {
        // Attempt to parse the left-hand side of an assignment. This may consume an identifier (an IDENTIFIER is an Expr.Variable in the way we handle it)
        // or any expression that could potentially be a valid assignment target (l-value).
        Expr expr = Or();

        // Check if this is an assignment (e.g., using "="). At this point, we need confirm
        // that the left-hand side was indeed an l-value candidate.
        if (Match(EQUAL))
        {
            Token equals = Previous();

            // Recursively parse the right-hand side. Assignment is right-associative,
            // so something like "a = b = c" should be interpreted as "a = (b = c)".
            // We re-enter the same Assignment rule to check if there's another assignment
            // on the right-hand side and hansdle it correctly.
            Expr value = Assignment();

            // Ensure the left-hand side is a valid assignment target (i.e., a variable).
            if (expr is Expr.Variable)
            {
                Token name = ((Expr.Variable)expr).Name;
                return new Expr.Assign(name, value);
            } 
            else if (expr is Expr.Get) 
            {
                Expr.Get get = (Expr.Get)expr;
                return new Expr.Set(get.Object, get.Name, value);
            }

            // If not a variable, then it's an invalid assignment target.
            Error(equals, "ParseError: Invalid assignment target.");
        }

        // If no assignment was matched, return the original expression.
        return expr;
    }

    private List<Stmt> Block()
    {
        var statements = new List<Stmt>();

        while (!Check(RIGHT_BRACE) && !IsAtEnd()) // the Check method do not consume the token, the IsAtEnd is to avoid infinite loops.
        {
            statements.Add(Declaration());
        }

        Consume(RIGHT_BRACE, "ParseError: Expect '}' after block.");
        return statements;
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().TokenType == type;
    }

    // comparison → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
    private Expr Comparison()
    {
        Expr expr = Term();

        while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
        {
            Token operatr = Previous(); // we use previus becase we made the desition of consume the token in the Match method.
            Expr right = Term();
            expr = new Expr.Binary(expr, operatr, right);
        }

        return expr;
    }

    private Token Consume(TokenType type, string errorMessage)
    {
        if (Check(type)) return Advance(); // advance will advance first if possible, and always return the previous

        throw Error(Peek(), errorMessage);
    }

    // declaration -> varDecl | FnDecl | ClassDecl | statement
    private Stmt Declaration()
    {
        try
        {
            if (Match(CLASS)) return ClassDeclaration();
            if (Match(FN)) return Function("function");
            if (Match(VAR)) return VarDeclaration();

            return Statement();
        }
        catch (ParseError e)
        {
            Synchronize();
            return null;
        }
    }

    private Stmt ClassDeclaration()
    {
        Token name = Consume(IDENTIFIER, "Expected class name.");
        Consume(LEFT_BRACE, "Expect '{' before class body");

        List<Stmt.Function> methods = [];
        while (!Check(RIGHT_BRACE) && !IsAtEnd())
        {
            methods.Add(Function("method"));
        }

        Consume(RIGHT_BRACE, "Expect '}' after class body.");

        return new Stmt.Class(name, methods);
    }

    private Stmt.Function Function(string kind)
    {
        Token name = Consume(IDENTIFIER, $"Expect {kind} name.");
        Consume(LEFT_PAREN, "Expect '(' after " + kind + " name.");

        List<Token> parameters = [];
        if (!Check(RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 parameters.");
                }

                // since consume returns the token, in this case the name, we can use it to return the name and if it is not a name we can send the error
                parameters.Add(Consume(IDENTIFIER, "Expect parameter name.")); 
            } while (Match(COMMA));
        }

        Consume(RIGHT_PAREN, "Expect ')' after parameters.");

        Consume(LEFT_BRACE, "Expect '{' before " + kind + " body.");
        List<Stmt> body = Block(); // the block functions assumes that the open { is already being matched.
        return new Stmt.Function(name, parameters, body);
    }

    // Equality -> Comparison (("!=" | "==") Comparison )*
    private Expr Equality()
    {
        Expr expr = Comparison();

        while (Match(BANG_EQUAL, EQUAL_EQUAL))
        {
            Token operatr = Previous(); // we use previus becase we made the desition of consume the token in the Match method.
            Expr right = Comparison();
            expr = new Expr.Binary(expr, operatr, right);
        }

        return expr;
    }

    private ParseError Error(Token token, String message)
    {
        Siew.Error(token, message);
        return new ParseError();
    }

    // Expression -> Assignment
    private Expr Expression()
    {
        return Assignment();
    }

    private Stmt ExpressionStatement()
    {
        Expr expr = Expression();

        Consume(SEMICOLON, "ParseError: Expect ';' after value.");

        return new Stmt.Expression(expr);
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

    //ifStmt         → "if" "(" expression ")" statement
    //                  ( "else" statement )? ;
    private Stmt IfStatement()
    {
        Consume(LEFT_PAREN, "ParseError: Expect '(' after the if.");
        Expr condition = Expression();
        Consume(RIGHT_PAREN, "ParseError: Expect ')' after the if condition"); 

        Stmt thenBranch = Statement();
        Stmt? elseBranch = null;

        if (Match(ELSE))
        {
            elseBranch = Statement();
        }

        return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private bool IsAtEnd()
    {
        return Peek().TokenType == EOF;
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

    // assigment -> IDENTIFIER "=" Assignment | Equality;
    private Expr Or()
    {
        Expr expr = And();

        while (Match(OR))
        {
            Token operatr = Previous();
            Expr right = And();
            expr = new Expr.Logical(expr, operatr, right);
        }

        return expr;
    }

    private Token Peek()
    {
        return Tokens[Current];
    }

    private Token Previous()
    {
        return Tokens[Current - 1];
    }

    //primary → NUMBER | STRING | "true" | "false" | "nil" | IDENTIFIER | "(" expression ")" ;
    private Expr Primary()
    {
        if (Match(FALSE)) return new Expr.Literal(false);
        if (Match(TRUE)) return new Expr.Literal(true);
        if (Match(NIL)) return new Expr.Literal(null);

        if (Match(NUMBER, STRING))
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
            Consume(RIGHT_PAREN, "ParseError: Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }

        if (Match(THIS)) return new Expr.This(Previous());

        throw Error(Peek(), "ParseError: Expect expression.");
    }

    private Stmt PrintStatement()
    {
        Token printType = Previous();
        // we consume the print token when we do the match in the statement method.
        Expr value = Expression();

        Consume(SEMICOLON, "ParseError: Expect ';' after value."); // we separates the expressions using semiclone, this is the reason of the semiclone.
        return new Stmt.Print(printType, value);
    }

    //statement → exprStmt | printStmt ;
    private Stmt Statement()
    {
        if (Match(FOR)) return ForStatement();
        if (Match(IF)) return IfStatement(); // we consume the if here
        if (Match(RETURN)) return ReturnStatement(); 
        if (Match(PRINT, PRINTL)) return PrintStatement();
        if (Match(WHILE)) return WhileStatement();

        if (Match(LEFT_BRACE)) return new Stmt.Block(Block()); //  this is a block statament

        return ExpressionStatement();
    }

    private Stmt ReturnStatement()
    {
        Token keyword = Previous();
        Expr? value = null;

        if (!Check(SEMICOLON))
        {
            value = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after return statement.");
        return new Stmt.Return(keyword, value);
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

    // unary → ( "!" | "-" ) unary | primary ;
    private Expr Unary()
    {
        if (Match(BANG, MINUS))
        {
            Token operatr = Previous();
            Expr right = Unary();
            return new Expr.Unary(operatr, right);
        }

        return Call();
    }

    // call → primary ( "(" arguments? ")" | "." IDENTIFIER )* ;
    private Expr Call()
    {
        Expr expr = Primary();

        while (true) 
        {
            if(Match(LEFT_PAREN))
            {
                expr = FinishCall(expr);
            }else if (Match(DOT))
            {
                Token name = Consume(IDENTIFIER, "Expect property name after '.'");

                expr = new Expr.Get(expr, name);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private Expr FinishCall(Expr callee)
    {
        List<Expr> arguments = [];

        if (!Check(RIGHT_PAREN)) // we check because we need to consume the parentesis to check later
        {
            do
            {
                if(arguments.Count >= 255) 
                {
                    Error(Peek(), "Can't have more than 255 arguments.");
                }
                arguments.Add(Expression());
            } while (Match(COMMA));
        }

        Token paren = Consume(RIGHT_PAREN, "Expect ')' after arguments.");

        return new Expr.Call(callee, paren, arguments);
    }

    private Stmt VarDeclaration()
    {
        Token name = Consume(IDENTIFIER, "ParseError: Expect variable name.");

        Expr? init = null;
        if (Match(EQUAL))
        {
            init = Expression();
        }

        Consume(SEMICOLON, "ParseError: Expect ';' after value."); // we separates the expressions using semiclone, this is the reason of the semiclone.
        return new Stmt.Var(name, init);
    }

    // whileStmt → "while" "(" expression ")" statement ;
    private Stmt WhileStatement()
    {
        Consume(LEFT_PAREN, "ParseError: Expect '(' after the if.");
        Expr condition = Expression();
        Consume(RIGHT_PAREN, "ParseError: Expect ')' after the if condition");
        Stmt body = Statement();

        return new Stmt.While(condition, body);
    }

    private class ParseError : Exception
    { }
}