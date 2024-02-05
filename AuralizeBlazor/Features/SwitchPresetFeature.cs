using BlazorJS.Attributes;

namespace AuralizeBlazor.Features;

public class SwitchPresetFeature : VisualizerFeatureBase
{
    public override string FullJsNamespace => "window.AuralizeBlazor.features.switchPresetFeature";

    /// <summary>
    /// The minimum time in milliseconds between two consecutive switch events.
    /// </summary>
    [ForJs]
    public int MinDebounceTimeInMs { get; set; } = 500;

    /// <summary>
    /// The minimum energy level required to trigger a switch event.
    /// </summary>
    [ForJs]
    public double MinEnergy { get; set; } = 0.25;
}
