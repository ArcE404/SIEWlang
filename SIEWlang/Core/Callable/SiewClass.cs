
namespace SIEWlang.Core.Callable;

public class SiewClass : ISiewCallable
{
    public readonly string Name;
    public readonly Dictionary<string, SiewFunction> Methods; // the class store the behaviour, the methods
    public readonly SiewClass? SuperClass;
    public SiewClass(string name, Dictionary<string, SiewFunction> methods, SiewClass? supeclass)
    {
        Name = name;
        Methods = methods;
        SuperClass = supeclass;
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
        
        if (Methods.TryGetValue(name, out var value))
        {
            return value;
        } else if (SuperClass is not null)
        {
            return SuperClass.FindMethod(name);
        }

        return null;
    }

    public override string ToString()
    {
        return Name;
    }
}
