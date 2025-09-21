using SIEWlang.Core.Interpreter;
using SIEWlang.Core.Parser;

namespace SIEWlang.Core.Callable;

public class SiewFunction(Stmt.Function declaration, Interpreter.Environment closure, bool isInitializer = false) : ISiewCallable
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
        Interpreter.Environment environment = new(closure);
        for (int i = 0; i < arguments.Count; i++)  // yeah, well, this is slow as fuck
        {
            environment.Define(declaration.Params[i].Lexeme, arguments[i]); // semantic choice here again, the order of the arguments matters a lot
        }

        try
        {
            interpreter.ExecuteBlock(declaration.Body, environment); // in a function only exist the function scope (and the inner ones) and the global ones;
        }
        catch (Return ret)
        {
            if (isInitializer) return closure.GetAt("this", 0);
            return ret.Value;
        }

        if (isInitializer) return closure.GetAt("this", 0);

        return null; // we add return values later
    }

    public SiewFunction Bind(SiewInstance instance)
    {
        /*
         * Binding “connects” the `this` keyword used inside a method to the actual
         * instance on which the method is accessed. We do this by extending the
         * function’s closure with an environment that defines `this = instance`.
         *
         * After binding, using `this` inside the method is equivalent to using
         * that instance directly.
         *
         * Example:
         *   var obj = SomeClass();
         *   obj.Foo();      // Inside Foo(), `this` == obj
         */

        // in the upper most cases this clousure is the global enviroment
        Interpreter.Environment environment = new(closure);
        environment.Define("this", instance);

        // Return a new SiewFunction with the same declaration,
        // but now closed over the environment that defines `this`.
        // This keeps runtime resolution in sync with the resolver: when the method is invoked,
        // the call environment will have this environment as its parent.
        return new SiewFunction(declaration, environment, isInitializer);
    }

    public override string ToString()
    {
        return $"<fn {declaration.Name.Lexeme}>";
    }
}
