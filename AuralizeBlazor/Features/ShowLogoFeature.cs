using BlazorJS.Attributes;
using System.ComponentModel;

namespace AuralizeBlazor.Features;

public class ShowLogoFeature: VisualizerFeatureBase
{
    public override string FullJsNamespace => "window.AuralizeBlazor.features.showLogo";

    [ForJs]
    public string Label { get; set; }

    [ForJs]
    public TextPosition Position { get; set; } = TextPosition.TopRight;

    [ForJs]
    public string LabelColor { get; set; } = "";

}

public enum TextPosition
{
    [Description("top-left")]
    TopLeft = 0,
    [Description("top-center")]
    TopCenter = 1,
    [Description("top-right")]
    TopRight = 2,
    [Description("center-left")]
    CenterLeft = 3,
    [Description("center-center")]
    CenterCenter = 4,
    [Description("center-right")]
    CenterRight = 5,
    [Description("bottom-left")]
    BottomLeft = 6,
    [Description("bottom-center")]
    BottomCenter = 7,
    [Description("bottom-right")]
    BottomRight = 8,
}