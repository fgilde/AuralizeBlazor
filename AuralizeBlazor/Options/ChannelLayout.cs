using System.ComponentModel;

namespace AuralizeBlazor.Options;

public enum ChannelLayout
{
    [Description("single")]
    Single,

    [Description("dual-horizontal")]
    DualHorizontal,

    [Description("dual-vertical")]
    DualVertical,

    [Description("dual-combined")]
    DualCombined
}
