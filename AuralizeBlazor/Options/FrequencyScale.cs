using System.ComponentModel;

namespace AuralizeBlazor.Options;

public enum FrequencyScale
{
    [Description("log")]
    Log,

    [Description("bark")]
    Bark,

    [Description("mel")]
    Mel,

    [Description("linear")]
    Linear
}