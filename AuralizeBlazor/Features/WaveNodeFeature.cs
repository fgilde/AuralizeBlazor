using BlazorJS.Attributes;
using System.ComponentModel;

namespace AuralizeBlazor.Features;

public class WaveNodeFeature : VisualizerFeatureBase
{
    public override string[] RequiredJsFiles => new[] { "./_content/AuralizeBlazor/js/features/waveNodeFeature.js" };
    public override string FullJsNamespace => "window.AuralizeBlazor.features.waveNode";
}
