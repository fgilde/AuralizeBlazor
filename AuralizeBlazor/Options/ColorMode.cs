using System.ComponentModel;

namespace AuralizeBlazor.Options;

public enum ColorMode
{
    [Description("gradient")]
    Gradient,

    [Description("bar-index")]
    BarIndex,

    [Description("bar-level")]
    BarLevel
}