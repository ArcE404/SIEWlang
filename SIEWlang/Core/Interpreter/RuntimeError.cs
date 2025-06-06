using SIEWlang.Core.Lexer;

namespace SIEWlang.Core.Interpreter;

internal class RuntimeError : Exception
{
    public readonly Token Token;

    public RuntimeError(Token token, string message) : base(message)
    {
       this.Token = token;
    }
}
