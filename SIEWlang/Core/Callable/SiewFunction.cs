using SIEWlang.Core.Interpreter;
using SIEWlang.Core.Parser;

namespace SIEWlang.Core.Callable;

public class SiewFunction(Stmt.Function declaration, Interpreter.Environment closure) : ISiewCallable
{
    public int Arity()
    {
        return declaration.Params.Count;
    }

    public object Call(Interpreter.Interpreter interpreter, List<object> arguments)
    {
        // The reason why we use an enviroment for each function call is to make recurtion to work
        // in a recurtion function the same variable names are atached to diferent values, just at diferent times. 
        // The call function is the one that make that "time" diference, making posible that recurtion without braking the 
        // Enviroment tree.
        Interpreter.Environment environment = new(interpreter._globals);
        for (int i = 0; i < arguments.Count; i++)  // this is slow as f***
        {
            environment.Define(declaration.Params[i].Lexeme, arguments[i]); // semantic choice here again, the order of the arguments matters a lot
        }

        try
        {
            interpreter.ExecuteBlock(declaration.Body, environment); // in a function only exist the function scope (and the inner ones) and the global ones;
        }
        catch (Return ret)
        {
            return ret.Value;
        }

        return null; // we add return values later
    }

    public override string ToString()
    {
        return $"<fn {declaration.Name.Lexeme}>";
    }
}
