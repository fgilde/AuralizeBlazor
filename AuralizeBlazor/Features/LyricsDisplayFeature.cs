using System.ComponentModel;
using BlazorJS.Attributes;

namespace AuralizeBlazor.Features;

public sealed class LyricsDisplayFeature : VisualizerFeatureBase
{
    public override string FullJsNamespace => $"window.AuralizeBlazor.features.lyricsDisplay";

    /// <summary>
    /// The position of the lyrics on the screen.
    /// </summary>
    [ForJs]
    public LyricsPosition TextPosition { get; set; }

    /// <summary>
    /// The colors you want to use for the gradient. Leave null to use the applied colors to auralizer.
    /// </summary>
    [ForJs("gradientColors")]
    public string[]? Colors { get; set; }

    /// <summary>
    /// Font size for the lyrics.
    /// </summary>
    [ForJs("baseFontSize")]
    public float FontSize { get; set; } = 40;

    /// <summary>
    /// Line spacing for the lyrics.
    /// </summary>
    [ForJs]
    public float LineSpacing { get; set; } = 50;

    /// <summary>
    /// Set to true to animate each word instead of line
    /// </summary>
    [ForJs]
    public bool EnableWordAnimation { get; set; } = true;

    /// <summary>
    /// Maximum dispersion offsets for word animation X (in pixels; reference values)
    /// </summary>
    [ForJs]
    public float WordAnimationMaxX { get; set; } = 50;

    /// <summary>
    /// Maximum dispersion offsets for word animation Y (in pixels; reference values)
    /// </summary>
    [ForJs]
    public float WordAnimationMaxY { get; set; } = 30;
}

public enum LyricsPosition
{
    [Description("top")]
    Top = 0,
    [Description("middle")]
    Middle = 1,
    [Description("bottom")]
    Bottom = 2
}