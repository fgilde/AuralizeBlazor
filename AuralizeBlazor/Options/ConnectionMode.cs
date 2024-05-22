namespace AuralizeBlazor.Options;

/// <summary>
/// Mode how the auralizer should connect to audio
/// </summary>
public enum ConnectionMode
{
    /// <summary>
    /// Creates a gain node for the element(s) to connect to.
    /// </summary>
    Gain = 0,
    
    /// <summary>
    /// Creates a captured stream for the element(s) and connects to the stream. Should be used when more than one Visualizer is connected to the same source
    /// </summary>
    Stream = 1,

    /// <summary>
    /// Connects directly to the element, but then connection can never change at runtime
    /// </summary>
    Direct = 2
}