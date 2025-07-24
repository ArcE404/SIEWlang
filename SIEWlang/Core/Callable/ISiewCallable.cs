namespace SIEWlang.Core.Callable;

public interface ISiewCallable
{
    object Call(Interpreter.Interpreter interpreter, List<object> arguments);
    int Arity();
    string ToString();
}
