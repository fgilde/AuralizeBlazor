using System.ComponentModel;

namespace AuralizeBlazor;

public enum VisualizationMode
{
    DiscreteFrequencies = 0,
    OneTwentyFourthOctaveBands = 1,
    OneTwelfthOctaveBands = 2,
    OneEighthOctaveBands = 3,
    OneSixthOctaveBands = 4,
    OneFourthOctaveBands = 5,
    OneThirdOctaveBands = 6,
    HalfOctaveBands = 7,
    FullOctaveBands = 8,    
    LineAreaGraph = 10
}


public enum MirrorMode
{
    None = 0,
    Left = -1,
    Right = 1,
}


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

public enum ColorMode
{
    [Description("gradient")]
    Gradient,

    [Description("bar-index")]
    BarIndex,

    [Description("bar-level")]
    BarLevel
}

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

public enum AudioMotionGradient
{
    [Description("classic")]
    Classic,

    [Description("prism")]
    Prism,

    [Description("rainbow")]
    Rainbow,

    [Description("orangered")]
    OrangeRed,

    [Description("steelblue")]
    SteelBlue
}

public enum VisualizerAction
{
    None = 0,
    TogglePlayPause = 1,
    //ToggleMute = 2,
    ToggleAllFeatures = 3,
    TogglePictureInPicture = 4,
    ToggleFullscreen = 5,
    ToggleMicrophone = 6,
    NextPreset = 7,
    PreviousPreset = 8,
}