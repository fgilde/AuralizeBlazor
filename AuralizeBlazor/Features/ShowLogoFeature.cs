using BlazorJS.Attributes;
using System.ComponentModel;

namespace AuralizeBlazor.Features;

/// <summary>
/// Show logo feature to display a custom Text.
/// </summary>
public class ShowLogoFeature: VisualizerFeatureBase
{
    /// <inheritdoc />
    public override string FullJsNamespace => "window.AuralizeBlazor.features.showLogo";

    /// <summary>
    /// Label to display.
    /// </summary>
    [ForJs]
    public string Label { get; set; }

    /// <summary>
    /// Label to display.
    /// </summary>
    [ForJs]
    public string Image { get; set; }

    /// <summary>
    /// Scale size for image
    /// </summary>
    [ForJs]
    public float ImageScale { get; set; } = 1f;

    /// <summary>
    /// Scale size for text
    /// </summary>
    [ForJs]
    public float TextScale { get; set; } = 1f;

    /// <summary>
    /// Position of the label.
    /// </summary>
    [ForJs]
    public VisualPosition LabelPosition { get; set; } = VisualPosition.TopRight;

    /// <summary>
    /// Position of the label.
    /// </summary>
    [ForJs]
    public VisualPosition ImagePosition { get; set; } = VisualPosition.TopLeft;


    /// <summary>
    /// Color of the label, if not set the default color from gradient will be used.
    /// </summary>
    [ForJs]
    public string LabelColor { get; set; } = "";

}