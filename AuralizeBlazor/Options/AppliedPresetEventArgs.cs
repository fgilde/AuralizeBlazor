namespace AuralizeBlazor.Options;

/// <summary>
/// Event arguments for when a preset is applied.
/// </summary>
public class AppliedPresetEventArgs
{
    /// <summary>
    /// Create a new instance of the event arguments.
    /// </summary>
    /// <param name="preset"></param>
    /// <param name="applySettings"></param>
    public AppliedPresetEventArgs(AuralizerPreset preset, PresetApplySettings applySettings)
    {
        Preset = preset;
        ApplySettings = applySettings;
    }

    /// <summary>
    /// The preset that was applied.
    /// </summary>
    public AuralizerPreset Preset { get; set; }

    /// <summary>
    /// Settings that were applied.
    /// </summary>
    public PresetApplySettings ApplySettings { get; set; }
}