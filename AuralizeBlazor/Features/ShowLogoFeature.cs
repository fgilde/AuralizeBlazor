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
    /// Position of the label.
    /// </summary>
    [ForJs]
    public TextPosition Position { get; set; } = TextPosition.TopRight;

    
    /// <summary>
    /// Color of the label, if not set the default color from gradient will be used.
    /// </summary>
    [ForJs]
    public string LabelColor { get; set; } = "";

}

/// <summary>
/// Text position.
/// </summary>
public enum TextPosition
{
    /// <summary>
    /// Text will be displayed Top left.
    /// </summary>
    [Description("top-left")]
    TopLeft = 0,
    
    /// <summary>
    /// Text will be displayed Top center.
    /// </summary>
    [Description("top-center")]
    TopCenter = 1,
    
    /// <summary>
    /// Text will be displayed Top right.
    /// </summary>
    [Description("top-right")]
    TopRight = 2,
    
    /// <summary>
    /// Text will be displayed Center left.
    /// </summary>
    [Description("center-left")]
    CenterLeft = 3,
    
    /// <summary>
    /// Text will be displayed in center.
    /// </summary>
    [Description("center-center")]
    CenterCenter = 4,
    
    /// <summary>
    /// Text will be displayed Center right.
    /// </summary>
    [Description("center-right")]
    CenterRight = 5,
    
    /// <summary>
    /// Text will be displayed bottom left.
    /// </summary>
    [Description("bottom-left")]
    BottomLeft = 6,
    
    /// <summary>
    /// Text will be displayed bottom center.
    /// </summary>
    [Description("bottom-center")]
    BottomCenter = 7,
    
    /// <summary>
    /// Text will be displayed bottom right.
    /// </summary>
    [Description("bottom-right")]
    BottomRight = 8,
}