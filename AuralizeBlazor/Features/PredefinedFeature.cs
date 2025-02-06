namespace AuralizeBlazor.Features;

/// <summary>
/// This feature allows to visualize the wave node as a separate line.
/// </summary>
public class PredefinedFeature(string name) : VisualizerFeatureBase
{
    /// <inheritdoc />
    public override string FullJsNamespace => $"window.AuralizeBlazor.features.{name}";
}
