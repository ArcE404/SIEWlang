using static SIEWlang.Core.Lexer.TokenType;

namespace SIEWlang.Core.Lexer;

public class Lexer
{
    private int Current = 0;

    private Dictionary<string, TokenType> Keywords = new()
    {
        { "and", AND },
        { "class", CLASS },
        { "else", ELSE },
        { "false", FALSE },
        { "for", FOR },
        { "fn", FN },
        { "if", IF },
        { "nil", NIL },
        { "or", OR },
        { "print", PRINT },
        { "printl", PRINTL },
        { "return", RETURN },
        { "super", SUPER },
        { "this", THIS },
        { "true", TRUE },
        { "var", VAR },
        { "while", WHILE }
    };

    private int Line = 1;
    private int Start = 0;
    private List<Token> Tokens = [];

    public Lexer(string sourceCode)
    {
        SourceCode = sourceCode;
    }

    private string SourceCode { set; get; }

    public IEnumerable<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            Start = Current;
            ScanToken();
        }

        Tokens.Add(new(TokenType.EOF, "", null, Line));
        return Tokens;
    }

    private void AddToken(TokenType tokenType)
    {
        AddToken(tokenType, null);
    }
      
    private void AddToken(TokenType tokenType, Object literal)
    {
        string text = SourceCode.Substring(Start, Current - Start); // clever, we use the start of the word and the current character evaluated
        Tokens.Add(new(tokenType, text, literal, Line));
    }

    private char Advance()
    {
        return SourceCode.ElementAt(Current++);
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();

        string text = SourceCode[Start..Current];

        // we try extract the reserved keyword
        // if we succeed we save the keyword and it is token type.
        if (Keywords.TryGetValue(text, out TokenType type))
        {
            AddToken(type);
            return;
        }

        // if we do not succeed we consider the alpha or alphanumeric value as
        // an identifier.
        AddToken(IDENTIFIER);
    }

    // we only care about letters here
    private bool IsAlpha(char c)
    {
        return (c >= 'a' && c <= 'z') ||
           (c >= 'A' && c <= 'Z') ||
            c == '_';
    }

    // we care about letters and numbers
    private bool IsAlphaNumeric(char c)
    {
        return IsAlpha(c) || IsDigit(c);
    }

    private bool IsAtEnd()
    {
        return Current >= SourceCode.Length;
    }

    private bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    // we verify if the next element is something expected to verify scenarios like  '==' '<=' '!=' and so on.
    // this works because at the moment we call Advance() we are also incrementing the counter so we are already
    // in this function evaluating the next character.
    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (!SourceCode.ElementAt(Current).Equals(expected)) return false;

        Current++;
        return true;
    }

    private void Number()
    {
        while (IsDigit(Peek())) Advance();

        // Look for a fractional part.
        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            // Consume the "."
            Advance();

            while (IsDigit(Peek())) Advance();
        }

        AddToken(NUMBER,
            double.Parse(SourceCode.Substring(Start, Current - Start)));
    }

    /*
     *     I could have made peek() take a parameter for the number of characters ahead to look instead of defining two functions,
     *     but that would allow arbitrarily far lookahead. Providing these two functions makes it clearer to a reader of the code
     *     that our scanner/lexer looks ahead at most two characters.
     */
    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return SourceCode.ElementAt(Current);
    }

    private char PeekNext()
    {
        // we verify that we are not at the end of the file
        if (Current + 1 >= SourceCode.Length) return '\0';

        return SourceCode.ElementAt(Current + 1);
    }

    private void ScanToken()
    {
        char c = Advance();
        switch (c)
        {
            case '(': AddToken(LEFT_PAREN); break;
            case ')': AddToken(RIGHT_PAREN); break;
            case '{': AddToken(LEFT_BRACE); break;
            case '}': AddToken(RIGHT_BRACE); break;
            case ',': AddToken(COMMA); break;
            case '.': AddToken(DOT); break;
            case '-': AddToken(MINUS); break;
            case '+': AddToken(PLUS); break;
            case ';': AddToken(SEMICOLON); break;
            case '*': AddToken(STAR); break;
            case '!':
                AddToken(Match('=') ? BANG_EQUAL : BANG);
                break;

            case '=':
                AddToken(Match('=') ? EQUAL_EQUAL : EQUAL);
                break;

            case '<':
                AddToken(Match('=') ? LESS_EQUAL : LESS);
                break;

            case '>':
                AddToken(Match('=') ? GREATER_EQUAL : GREATER);
                break;

            case '/':
                if (Match('/'))
                {
                    // A comment goes until the end of the line.
                    while (Peek() != '\n' && !IsAtEnd()) Advance();
                }
                else
                {
                    AddToken(SLASH);
                }
                break;

            case '"': StringLiteral(); break;
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;

            case '\n':
                Line++;
                break;

            default:
                {
                    if (IsDigit(c))
                    {
                        Number();
                    }
                    else if (IsAlpha(c))
                    {
                        Identifier();
                    }
                    else
                    {
                        Siew.Error(Line, "LexerError: Unexpected character.");
                    }
                    break;
                }
        }
    }

    private void StringLiteral()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            // We need to make sure if this is taking more than one line"
            if (Peek() == '\n') Line++;
            Advance();
        }

        // If there are no " at the end, the while above will consume the entire file. Then must print an error. Clever.
        if (IsAtEnd())
        {
            Siew.Error(Line, "LexerError: Unterminated string");
            return;
        }

        Advance();

        // we use + 1 and -2 to get rid of the "" and only have the string value
        string value = SourceCode.Substring(Start + 1, (Current - Start) - 2);

        AddToken(STRING, value);
    }
}