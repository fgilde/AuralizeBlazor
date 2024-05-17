namespace AuralizeBlazor.Options;

public enum InitialRender
{
    None = 0,
    WithRandomData = 1,
    WithRealAudioData = 2,
    WithFullSpectrumAudioData = 3,
    WithFullSpectrumAudioDataPrefilledWithRandomData = 6,
    SimulateWithRandomData = 4,
    SimulateWithFullSpectrumAudioData = 5,
}