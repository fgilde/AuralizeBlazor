using System;
using System.Linq;
using System.Reflection;
using AuralizeBlazor.Features;
using BlazorJS.Attributes;
using Nextended.Core.Types;

namespace AuralizeBlazor;

public class VisualizerPreset : SuperType<VisualizerPreset>
{
    private readonly Action<BlazorAudioVisualizer> _action;

    /// <summary>
    /// This is true by default.
    /// If this value is false, the default values are not applied before applying the preset.
    /// </summary>
    public bool ResetToDefaultFirst { get; set; } = true;

    public VisualizerPreset(int id, string name, Action<BlazorAudioVisualizer> action) : base(id, name, name)
    {
        _action = action;
    }

    public void Apply(BlazorAudioVisualizer blazorAudioVisualizer, bool? resetFirst = null)
    {
        if(resetFirst ?? ResetToDefaultFirst)
            Default._action(blazorAudioVisualizer);
        _action(blazorAudioVisualizer);
    }

    public static VisualizerPreset Default =>
        new(0, nameof(Default), visualizer =>
        {
            var ignore = new[] // This properties are not reset to default
            {
                nameof(BlazorAudioVisualizer.ChildContent), 
                nameof(BlazorAudioVisualizer.AudioElements),
                nameof(BlazorAudioVisualizer.ClickAction),
                nameof(BlazorAudioVisualizer.ContextMenuAction),
                nameof(BlazorAudioVisualizer.DoubleClickAction),
                nameof(BlazorAudioVisualizer.ConnectAllAudioSources),
                nameof(BlazorAudioVisualizer.BackgroundImage),
                nameof(BlazorAudioVisualizer.ConnectMicrophone)
            };
            var defaultVisualizer = new BlazorAudioVisualizer(); // Instance with default values
            var properties = typeof(BlazorAudioVisualizer).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (!prop.CanRead || !prop.CanWrite || ignore.Contains(prop.Name) || prop.GetCustomAttribute(typeof(ForJs)) == null) continue;
                var defaultValue = prop.GetValue(defaultVisualizer);
                prop.SetValue(visualizer, defaultValue);
            }
        });

    public static VisualizerPreset ClassicLedBars => new(1, nameof(ClassicLedBars), visualizer =>
    {
        visualizer.Mode = VisualizationMode.OneThirdOctaveBands;
        visualizer.AnsiBands = true;
        visualizer.BarSpacing = 0.5;
        visualizer.LedBars = true;
        visualizer.MaxFrequency = 20000;
        visualizer.MinFrequency = 25;
        visualizer.TrueLeds = true;
    });

    public static VisualizerPreset MirrorWave => new(2, nameof(MirrorWave), visualizer =>
    {
        visualizer.Mode = VisualizationMode.LineAreaGraph;
        visualizer.FillAlpha = 0.6;
        visualizer.Gradient = AudioMotionGradient.Rainbow;
        visualizer.LineWidth = 1.5;
        visualizer.MaxFrequency = 20000;
        visualizer.MinFrequency = 30;
        visualizer.Mirror = MirrorMode.Left;
        visualizer.ReflexAlpha = 1;
        visualizer.ReflexRatio = 0.5;
        visualizer.ShowPeaks = false;
        visualizer.ShowScaleX = false;
    });

    public static VisualizerPreset RadialSpectrum => new(3,nameof(RadialSpectrum), visualizer =>
    {
        visualizer.Mode = VisualizationMode.OneFourthOctaveBands;
        visualizer.Gradient = AudioMotionGradient.Prism;
        visualizer.LedBars = false;
        visualizer.MaxFrequency = 20000;
        visualizer.MinFrequency = 20;
        visualizer.Radial = true;
        visualizer.SpinSpeed = 1;
    });

    public static VisualizerPreset RadialSpectrumScale => new(4, nameof(RadialSpectrumScale), visualizer =>
    {
        RadialSpectrum.Apply(visualizer);
        visualizer.Features = new[] { new RadialRadiusFeature() };
    });

    public static VisualizerPreset RadialInverse => new(5, nameof(RadialInverse), visualizer =>
    {
        visualizer.Mode = VisualizationMode.OneEighthOctaveBands;
        visualizer.BarSpacing = .25;
        visualizer.FillAlpha = .5;
        visualizer.Gradient = AudioMotionGradient.Prism;
        visualizer.LinearAmplitude = true;
        visualizer.LinearBoost = 1.8;
        visualizer.LineWidth = 1.5;
        visualizer.MaxDecibels = -30;
        visualizer.MaxFrequency = 16000;
        visualizer.Radial = true;
        visualizer.RadialInvert = true;
        visualizer.ShowBgColor = true;
        visualizer.ShowPeaks = true;
        visualizer.SpinSpeed = 2;
        visualizer.OutlineBars = true;
        visualizer.WeightingFilter = WeightingFilter.D;
    });

    public static VisualizerPreset BarkScaleLinearAmplitude => new(6, nameof(BarkScaleLinearAmplitude), visualizer =>
    {
        visualizer.FrequencyScale = FrequencyScale.Bark;
        visualizer.Gradient = AudioMotionGradient.Rainbow;
        visualizer.LinearAmplitude = true;
        visualizer.LinearBoost = 1.8;
        visualizer.MaxFrequency = 20000;
        visualizer.MinFrequency = 20;
        visualizer.ReflexAlpha = 0.25;
        visualizer.ReflexFit = true;
        visualizer.ReflexRatio = 0.3;
        visualizer.ShowScaleX = true;
        visualizer.WeightingFilter = WeightingFilter.D;
    });

    public static VisualizerPreset BarkScaleLinearAmplitudeWithWaveNode => new(7,nameof(BarkScaleLinearAmplitudeWithWaveNode), visualizer =>
    {
        BarkScaleLinearAmplitude.Apply(visualizer);
        visualizer.Features = new[] { new WaveNodeFeature() };
    });

    public static VisualizerPreset DualChannelCombined => new(8, nameof(DualChannelCombined), visualizer =>
    {
        visualizer.Mode = VisualizationMode.LineAreaGraph;
        visualizer.ChannelLayout = ChannelLayout.DualCombined;
        visualizer.FillAlpha = 0.25;
        visualizer.FrequencyScale = FrequencyScale.Bark;
        visualizer.GradientLeft = AudioMotionGradient.SteelBlue;
        visualizer.GradientRight = AudioMotionGradient.OrangeRed;
        visualizer.LinearAmplitude = true;
        visualizer.LinearBoost = 1.8;
        visualizer.LineWidth = 1.5;
        visualizer.MaxFrequency = 20000;
        visualizer.MinFrequency = 20;
        visualizer.Mirror = 0;
        visualizer.Radial = false;
        visualizer.ReflexRatio = 0;
        visualizer.ShowPeaks = false;
        visualizer.WeightingFilter = WeightingFilter.D;
    });

    public static VisualizerPreset RoundBarsBarLevelColorMode => new(9, nameof(RoundBarsBarLevelColorMode), visualizer =>
    {
        visualizer.Mode = VisualizationMode.OneTwelfthOctaveBands;
        visualizer.AlphaBars = false;
        visualizer.AnsiBands = false;
        visualizer.BarSpacing = 0.25;
        visualizer.ChannelLayout = ChannelLayout.Single;
        visualizer.ColorMode = ColorMode.BarLevel;
        visualizer.FrequencyScale = FrequencyScale.Log;
        visualizer.Gradient = AudioMotionGradient.Prism;
        visualizer.LedBars = false;
        visualizer.LinearAmplitude = true;
        visualizer.LinearBoost = 1.6;
        visualizer.LumiBars = false;
        visualizer.MaxFrequency = 16000;
        visualizer.MinFrequency = 30;
        visualizer.Mirror = 0;
        visualizer.Radial = false;
        visualizer.ReflexRatio = 0.5;
        visualizer.ReflexAlpha = 1;
        visualizer.RoundBars = true;
        visualizer.ShowPeaks = false;
        visualizer.ShowScaleX = false;
        visualizer.Smoothing = 0.7;
        visualizer.WeightingFilter = WeightingFilter.D;
    });

    public static VisualizerPreset ReflexMirror => new(10, nameof(ReflexMirror), visualizer =>
    {
        visualizer.Mode = VisualizationMode.LineAreaGraph;
        visualizer.ChannelLayout = ChannelLayout.Single;
        visualizer.Gradient = AudioMotionGradient.Rainbow;
        visualizer.LinearAmplitude = false;
        visualizer.ReflexRatio = 0.4;
        visualizer.ShowBgColor = true;
        visualizer.ShowPeaks = true;
        visualizer.ShowScaleX = false;
        visualizer.Mirror = MirrorMode.Left;
        visualizer.MaxFrequency = 8000;
        visualizer.MinFrequency = 20;
        visualizer.LineWidth = 2;
        visualizer.FillAlpha = 0.2;
    });

    public static VisualizerPreset DualLedBars => new(11, nameof(DualLedBars), visualizer =>
    {
        visualizer.Mode = VisualizationMode.OneTwelfthOctaveBands;
        visualizer.AlphaBars = false;
        visualizer.AnsiBands = false;
        visualizer.BarSpacing = 0.1;
        visualizer.ChannelLayout = ChannelLayout.DualVertical;
        visualizer.GradientLeft = AudioMotionGradient.SteelBlue;
        visualizer.GradientRight = AudioMotionGradient.OrangeRed;
        visualizer.LedBars = true;
        visualizer.LumiBars = false;
        visualizer.Radial = false;
        visualizer.ReflexRatio = 0;
        visualizer.ShowPeaks = true;
        visualizer.ShowScaleX = false;
        visualizer.Mirror = 0;
        visualizer.MaxFrequency = 16000;
        visualizer.MinFrequency = 20;
    });
}