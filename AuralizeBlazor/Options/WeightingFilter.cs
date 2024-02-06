using System.ComponentModel;

namespace AuralizeBlazor.Options;

public enum WeightingFilter
{
    [Description("")]
    None,

    [Description("A")]
    A,

    [Description("B")]
    B,

    [Description("C")]
    C,

    [Description("D")]
    D,

    [Description("468")]
    Filter468
}