using BlazorJS.Attributes;

namespace AuralizeBlazor.Features;

public sealed class LineParticleFeature : VisualizerFeatureBase
{
    public override string FullJsNamespace => $"window.AuralizeBlazor.features.lineParticle";

    [ForJs("epicText")]
    public string Text { get; set; }

    [ForJs("particleCount")]
    public int ParticleCount { get; set; } = 200;

    [ForJs("connectionThreshold")]
    public double ConnectionThreshold { get; set; } = 50;
}