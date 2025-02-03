using System.Numerics;

namespace AuralizeBlazor.Types;

public class ThicknessOf<T> where T: INumber<T>
{
    public ThicknessOf(T all)
    {
        Left = all;
        Right = all;
        Top = all;
        Bottom = all;
    }


    public ThicknessOf(T left, T right, T top, T bottom)
    {
        Left = left;
        Right = right;
        Top = top;
        Bottom = bottom;
    }

    public T Left { get; set; }
    public T Right { get; set; }
    public T Top { get; set; }
    public T Bottom { get; set; }
}