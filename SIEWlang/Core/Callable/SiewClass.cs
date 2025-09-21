
namespace SIEWlang.Core.Callable;

public class SiewClass : SiewInstance, ISiewCallable
{
    public readonly string Name;
    public readonly Dictionary<string, SiewFunction> Methods; // the class store the behaviour, the methods

    public SiewClass(string name, Dictionary<string, SiewFunction> methods) : base(null)
    {
        Name = name;
        Methods = methods;
    }

    public SiewClass(string name, Dictionary<string, SiewFunction> methods, SiewClass metaClass) : base(metaClass)
    {
        Name = name;
        Methods = methods;
    }

    public int Arity()
    {
        SiewFunction? initializer = FindMethod("init");

        if (initializer is not null)
        {
            return initializer.Arity();
        }

        return 0;
    }

    public object Call(Interpreter.Interpreter interpreter, List<object> arguments)
    {
        // when we are creating the state of the class (the instance) we also make the init (constructor)
        SiewInstance instance = new(this);

        SiewFunction? initializer = FindMethod("init");

        if (initializer is not null)
        {
            initializer.Bind(instance).Call(interpreter, arguments); // This is the prove that constructors ARE JUST a METHOD.
        }

        return instance;
    }

    public SiewFunction? FindMethod(string name)
    {
        return Methods.TryGetValue(name, out var value) ? value : null;
    }

    public override string ToString()
    {
        return Name;
    }
}
