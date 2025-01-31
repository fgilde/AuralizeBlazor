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

    /// <summary>
    /// If true random colors are applied
    /// </summary>
    [ForJs]
    public bool OverridePresetColorsWithRandoms { get; set; }

    /// <summary>
    /// If <see cref="OverridePresetColorsWithRandoms"/> is true , this value will be used as the minimum time in milliseconds between two consecutive color switch events.
    /// If the value is null the <see cref="MinDebounceTimeInMs"/> value will be used.
    /// </summary>
    [ForJs]
    public int? MinDebounceForColorTimeInMs { get; set; } = null;

    /// <summary>
    /// if <see cref="OverridePresetColorsWithRandoms"/> is true , this value will be used as the minimum energy level required to trigger a color switch event.
    /// If the value is null the <see cref="MinEnergy"/> value will be used.
    /// </summary>
    [ForJs]
    public double? MinEnergyForColor { get; set; } = null;

}
