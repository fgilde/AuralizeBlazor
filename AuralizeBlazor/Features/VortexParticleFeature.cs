using BlazorJS.Attributes;
using System.Xml.Linq;

namespace AuralizeBlazor.Features;

public sealed class VortexParticleFeature : VisualizerFeatureBase
{
    public override string FullJsNamespace => $"window.AuralizeBlazor.features.vortexParticle";

    [ForJs("epicText")]
    public string Text { get; set; }

    [ForJs("particleCount")]
    public int ParticleCount { get; set; } = 200;
}