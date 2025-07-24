namespace SIEWlang.Core.Callable;

public class NativeClockFunction : ISiewCallable
{
    public int Arity()
    {
        return 0;
    }

    public object Call(Interpreter.Interpreter interpreter, List<object> arguments)
    {
        return (double)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000;
    }

    public override string ToString()
    {
        return $"<Native Clock Function>";
    }
}
