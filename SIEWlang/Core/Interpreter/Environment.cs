using System.Diagnostics.CodeAnalysis;
using System.Xml.Schema;
using SIEWlang.Core.Lexer;

namespace SIEWlang.Core.Interpreter;

public class Environment
{
    
    private readonly Environment? enclosing;

    public Environment()
    {
        enclosing = null;
    }

    public Environment(Environment env)
    {
        enclosing = env;
    }

    // Even though a variable is represented by a Token, we use the variable name (string) as the key instead of the Token itself.
    // This is because the IDENTIFIER Token corresponds to a specific location in the source code.
    // However, when resolving variables, all IDENTIFIER tokens with the same name should refer to the same logical variable,
    // regardless of where they appear in the code. Therefore, we store and compare variable names (strings), not Token instances.
    private readonly Dictionary<string, object?> values = new();

    public void Define(string name, object? value)
    {
        values[name] = value;
        /*
         * This is a deliberate semantic choice:
         * We allow variables to be redefined by declaring them again with "var". For example:
         * 
         * var a = "some";
         * print a; // prints: some
         * var a = "some some";
         * print a; // prints: some some
         * 
         * This behavior is permitted in Scheme at the top level, and we adopt the same convention here.
         * 
         * "When in doubt, do what Scheme does." 
         */
    }

    public object? Get(Token name)
    {
        if  (values.TryGetValue(name.Lexeme, out var value)) return value;

        if (enclosing is not null) return enclosing.Get(name);

        throw new RuntimeError(name, $"RunTimeError: Undefined variable {name.Lexeme}.");
    }

    public void Assing(Token name, object value)
    {
        if(values.ContainsKey(name.Lexeme))
        {
            values[name.Lexeme] = value;
            return;
        }

        if (enclosing is not null)
        {
            enclosing.Assing(name, value);
            return;
        }

        throw new RuntimeError(name, $"RunTimeError: Undefined variable '{name.Lexeme}'");
    }
}
