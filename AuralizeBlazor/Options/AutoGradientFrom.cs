using System;

namespace AuralizeBlazor.Options;

public enum AutoGradientFrom
{
    None,
    BackgroundImage,
    MetaCoverImage,
    TrackListCoverImage = 4,
    Random,
}