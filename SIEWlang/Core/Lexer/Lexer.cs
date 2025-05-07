using static SIEWlang.Core.Lexer.TokenType;

namespace SIEWlang.Core.Lexer;

public class Lexer
{
    private string SourceCode { set; get; }
    private List<Token> Tokens = [];

    private int Line = 1;
    private int Current = 0;
    private int Start = 0;
    
    public Lexer(string sourceCode)
    {
        SourceCode = sourceCode;
    }

    public IEnumerable<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            Start = Current;
            ScanToken();
        }

        Tokens.Add(new(TokenType.EOF, "", null, Line));
        return new List<Token>();
    }

    private bool IsAtEnd()
    {
        return Current >= SourceCode.Length;
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
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;

            case '\n':
                Line++;
                break;
            default: Siew.Error(Line, "Unexpected character."); break;
        }
    }

    private char Advance()
    {
        return SourceCode.ElementAt(Current++);
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

    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return SourceCode.ElementAt(Current);
    }
}
