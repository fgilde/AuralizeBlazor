namespace AuralizeBlazor.Types;

public class RangeOf<T>
{
    public RangeOf(T min, T max)
    {
        Min = min;
        Max = max;
    }

    public T Min { get; set; }
    public T Max { get; set; }
}