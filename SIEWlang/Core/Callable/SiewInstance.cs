namespace SIEWlang.Core.Callable;

public  class SiewInstance
{
    private SiewClass SiewClass { get; set; }

    public SiewInstance(SiewClass siewClass)
    {
        SiewClass = siewClass;
    }

    public override string ToString()
    {
        return $"{SiewClass.Name} instance";
    }
}
