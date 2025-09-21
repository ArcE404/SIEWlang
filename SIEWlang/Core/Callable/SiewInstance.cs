using SIEWlang.Core.Interpreter;
using SIEWlang.Core.Lexer;

namespace SIEWlang.Core.Callable;

public class SiewInstance
{
    private SiewClass SiewClass { get; set; }
    private Dictionary<string, object> Fields = []; // the instance store the state, the fields

    public SiewInstance(SiewClass siewClass)
    {
        SiewClass = siewClass;
    }

    public object Get(Token name)
    {
        if (Fields.TryGetValue(name.Lexeme, out object value)) 
        { 
            return value; 
        }

        SiewFunction? method = SiewClass.FindMethod(name.Lexeme);

        /*
         * The `this` keyword provides access to the instance state from within
         * method bodies. The resolver arranges for every method to have a parent
         * scope where `this` is defined. Binding a method pairs that parent scope
         * with the concrete instance so `this` inside the method refers to the
         * correct receiver.
         *
         * Continue in Bind() for the runtime details of how the instance is
         * captured in the method’s closure.
         */
        if (method is not null) return method.Bind(this);

        throw new RuntimeError(name, $"Undefined property {name.Lexeme}.");
    }

    public void Set(Token name, object value)
    {
        Fields[name.Lexeme] = value;
    }

    public override string ToString()
    {
        return $"{SiewClass.Name} instance";
    }
}
