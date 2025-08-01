
namespace SIEWlang.Core.Callable;

public class SiewClass : ISiewCallable
{
    public readonly string Name;

    public SiewClass(string name)
    {
        Name = name;
    }

    public int Arity()
    {
        return 0;
    }

    public object Call(Interpreter.Interpreter interpreter, List<object> arguments)
    {
        SiewInstance instance = new(this);
        return instance;
    }

    public override string ToString()
    {
        return Name;
    }
}
