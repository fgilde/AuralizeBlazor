using BlazorJS.Attributes;

namespace AuralizeBlazor.Features;

public class RadialRadiusFeature : VisualizerFeatureBase
{
    public override string[] RequiredJsFiles => new[] { "./_content/AuralizeBlazor/js/features/radialRadiusFeature.js" };
    public override string FullJsNamespace => "window.AuralizeBlazor.features.radialRadius";
}
