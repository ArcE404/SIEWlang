namespace SIEWlang.Core.Lexer;

public class Token
{
    public TokenType TokenType { private set; get; }
    public string Lexeme { private set; get; }
    public Object Literal { private set; get; }
    public int Line { private set; get; }

    public Token(TokenType tokenType, string lexeme, Object literal, int line)
    {
        TokenType = tokenType;
        Lexeme = lexeme;
        Literal = literal;
        Line = line;
    }

    public string ToString()
    {
        return $"{TokenType} {Lexeme} {Literal}";
    }
}
