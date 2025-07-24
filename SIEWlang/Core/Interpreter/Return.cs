namespace SIEWlang.Core.Interpreter;

internal class Return : Exception
{
    public object? Value;
    public Return(object? value)
    {
        Value = value;
    }
}
