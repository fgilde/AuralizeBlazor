using System.ComponentModel;

namespace AuralizeBlazor.Options;

/// <summary>
/// Simple actions that can be performed by the visualizer
/// </summary>
public enum VisualizerAction
{
    /// <summary>
    /// No action
    /// </summary>
    None,

    /// <summary>
    /// Toggle play/pause
    /// </summary>
    [Label("Play/Pause")]
    TogglePlayPause,
    //ToggleMute = 2,

    /// <summary>
    /// Toggle all features
    /// </summary>
    [Label("Enable/Disable all Features")]
    ToggleAllFeatures,

    /// <summary>
    /// Toggle picture in picture
    /// </summary>
    [Label("Picture in Picture")]
    TogglePictureInPicture,

    /// <summary>
    /// Toggle fullscreen
    /// </summary>
    [Label("Fullscreen")]
    ToggleFullscreen,

    /// <summary>
    /// Toggle full page
    /// </summary>
    [Label("Fullpage")]
    ToggleFullPage,

    /// <summary>
    /// Connect/disconnect microphone
    /// </summary>
    [Label("Connect/Disconnect Microphone")]
    ToggleMicrophone,

    /// <summary>
    /// Select next preset
    /// </summary>
    [Label("Next Preset")]
    NextPreset,

    /// <summary>
    /// Select previous preset
    /// </summary>
    [Label("Previous Preset")]
    PreviousPreset,

    /// <summary>
    /// Select next track
    /// </summary>
    [Label("Next Track")]
    NextTrack,

    /// <summary>
    /// Select previous track
    /// </summary>
    [Label("Previous Track")]
    PreviousTrack,

    /// <summary>
    /// Open/close track list
    /// </summary>
    [Label("Show Track List")]
    DisplayTrackList,

    /// <summary>
    /// Open/close preset list
    /// </summary>
    [Label("Show Preset List")]
    DisplayPresetList,

    /// <summary>
    /// Open/close action menu
    /// </summary>
    [Label("Action Menu")]
    DisplayActionMenu,
}