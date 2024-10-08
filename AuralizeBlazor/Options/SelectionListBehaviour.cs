﻿using System.ComponentModel;

namespace AuralizeBlazor.Options;

public enum SelectionListBehaviour
{
    Hidden,
    AlwaysVisible,
    Toggleable
}

public enum SelectionListMode
{
    [Description("default")]
    Default,
    [Description("compact")]
    Compact
}

public enum Position
{
    [Description("top left")]
    TopLeft,
    [Description("top right")]
    TopRight,
    [Description("bottom left")]
    BottomLeft,
    [Description("bottom right")]
    BottomRight,
    [Description("center left")]
    CenterLeft,
    [Description("center right")]
    CenterRight,
    [Description("mouse")]
    Mouse
}