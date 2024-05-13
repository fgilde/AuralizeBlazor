namespace AuralizeBlazor.Options;

/// <summary>
/// Settings for applying a preset.
/// </summary>
public class PresetApplySettings
{
    public PresetApplySettings(PresetApplyReason reason)
    {
        Reason = reason;
    }

    /// <summary>
    /// If true all settings will be reset before applying the preset.
    /// </summary>
    public bool? ResetFirst { get; set; } = null;
    
    /// <summary>
    /// If true and the setting in the auralizer is also true a message with the name will be displayed if the preset is applied.
    /// </summary>
    public bool DisplayMessageIf { get; set; } = true;

    /// <summary>
    /// The reason for applying the preset.
    /// </summary>
    public PresetApplyReason Reason { get; set; }
}

public enum PresetApplyReason
{
    Unknown = 0,
    Initial = 1,
    UserSelectedFromList = 2,
    UserSelectedAsAction = 3,
    ExternalCall = 4,
    FromFeature = 5
}
