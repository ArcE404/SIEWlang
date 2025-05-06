namespace SIEWlang.Core.Lexer;

public class Lexer
{
    private string SourceCode { set; get; }
     
    public Lexer(string sourceCode)
    {
        SourceCode = sourceCode;
    }

    public IEnumerable<Token> ScanTokens()
    {
        return new List<Token>();
    }
}
