using BlazorJS.Attributes;

namespace AuralizeBlazor.Features;

/// <summary>
/// Energy meter feature.
/// </summary>
public class EnergyMeterFeature : VisualizerFeatureBase
{
    /// <inheritdoc />
    public override string FullJsNamespace => "window.AuralizeBlazor.features.energyMeter";

    /// <summary>
    /// Show the peak energy bar.
    /// </summary>
    [ForJs] public bool ShowPeakEnergyBar { get; set; } = true;
    
    /// <summary>
    /// If true the peak energy bar will be shown, otherwise it will be hidden.
    /// </summary>
    [ForJs] public bool ShowBassLight { get; set; } = true;
    
    /// <summary>
    /// If true the midrange energy bar will be shown, otherwise it will be hidden.
    /// </summary>
    [ForJs] public bool ShowMidrangeLight { get; set; } = true;
    
    /// <summary>
    /// If true the treble energy bar will be shown, otherwise it will be hidden.
    /// </summary>
    [ForJs] public bool ShowTrebleLight { get; set; } = true;

    /// <summary>
    /// The text to show for the bass energy bar.
    /// </summary>
    [ForJs] public string BassText { get; set; } = "BASS";
    
    /// <summary>
    /// The text to show for the midrange energy bar.
    /// </summary>
    [ForJs] public string MidrangeText { get; set; } = "MIDRANGE";
    
    /// <summary>
    /// The text to show for the treble energy bar.
    /// </summary>
    [ForJs] public string TrebleText { get; set; } = "TREBLE";
}
