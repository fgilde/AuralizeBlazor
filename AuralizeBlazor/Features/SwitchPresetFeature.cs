using BlazorJS.Attributes;

namespace AuralizeBlazor.Features;

/// <summary>
/// This feature allows to switch between different presets based on bass energy.
/// </summary>
public class SwitchPresetFeature : VisualizerFeatureBase
{
    /// <inheritdoc />
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
    public double MinEnergy { get; set; } = .35;

    /// <summary>
    /// If true the preset will be picked randomly, otherwise the next preset will be selected.
    /// </summary>
    [ForJs]
    public bool PickRandom { get; set; } = true;
}
