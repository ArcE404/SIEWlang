namespace SIEWlang.Core.Lexer;

public class Token
{
    private TokenType TokenType { set; get; }
    private string Lexeme { set; get; }
    private Object Literal { set; get; }
    private int Line { set; get; }

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
