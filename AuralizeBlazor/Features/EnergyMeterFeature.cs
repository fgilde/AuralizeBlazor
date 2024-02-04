using BlazorJS.Attributes;
using System.ComponentModel;

namespace AuralizeBlazor.Features;

public class EnergyMeterFeature : VisualizerFeatureBase
{
    public override string[] RequiredJsFiles => new[] { "./_content/AuralizeBlazor/js/features/energyMeterFeature.js" };
    public override string FullJsNamespace => "window.AuralizeBlazor.features.energyMeter";

    [ForJs] public bool ShowPeakEnergyBar { get; set; } = true;
    [ForJs] public bool ShowBassLight { get; set; } = true;
    [ForJs] public bool ShowMidrangeLight { get; set; } = true;
    [ForJs] public bool ShowTrebleLight { get; set; } = true;

    [ForJs] public string BassText { get; set; } = "BASS";
    [ForJs] public string MidrangeText { get; set; } = "MIDRANGE";
    [ForJs] public string TrebleText { get; set; } = "TREBLE";
}
