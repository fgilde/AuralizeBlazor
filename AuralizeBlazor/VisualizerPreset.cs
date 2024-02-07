using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AuralizeBlazor.Features;
using AuralizeBlazor.Options;
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

    internal void Apply(BlazorAudioVisualizer blazorAudioVisualizer, bool? resetFirst = null)
    {
        if (resetFirst ?? ResetToDefaultFirst)
            Default._action(blazorAudioVisualizer);
        var currentFeatures = blazorAudioVisualizer.Features?.ToList() ?? new List<IVisualizerFeature>();
        _action(blazorAudioVisualizer);
        var newFeatures = blazorAudioVisualizer.Features;
        foreach (var feature in newFeatures ?? Array.Empty<IVisualizerFeature>())
        {
            if (currentFeatures.Any(f => f.GetType() == feature.GetType())) continue;
            feature.AppliedFromPreset = true;
            currentFeatures.Add(feature);
        }
        blazorAudioVisualizer.Features = currentFeatures.ToArray();
    }

    public static VisualizerPreset Default =>
        new(0, nameof(Default), visualizer =>
        {
            var ignore = new[] // This properties are not reset to default
            {
                nameof(BlazorAudioVisualizer.Features), 
                nameof(BlazorAudioVisualizer.ChildContent), 
                nameof(BlazorAudioVisualizer.AudioElements),
                nameof(BlazorAudioVisualizer.ClickAction),
                nameof(BlazorAudioVisualizer.ContextMenuAction),
                nameof(BlazorAudioVisualizer.DoubleClickAction),
                nameof(BlazorAudioVisualizer.ConnectAllAudioSources),
                nameof(BlazorAudioVisualizer.BackgroundImage),
                nameof(BlazorAudioVisualizer.ConnectMicrophone)
            };
            
            visualizer.Features = (visualizer.Features ?? Array.Empty<IVisualizerFeature>()).Where(f => !f.AppliedFromPreset).ToArray();
            
            var defaultVisualizer = new BlazorAudioVisualizer(); // Instance with default values
            var properties = typeof(BlazorAudioVisualizer)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (!prop.CanRead || !prop.CanWrite || ignore.Contains(prop.Name) || prop.GetCustomAttribute(typeof(ForJs)) == null || visualizer?.IgnoredPropertiesForReset?.Contains(prop.Name) == true) 
                    continue;
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

    public static VisualizerPreset RadialSpectrumStereo => new(3,nameof(RadialSpectrumStereo), visualizer =>
    {
        visualizer.Mode = VisualizationMode.OneFourthOctaveBands;
        visualizer.Gradient = AudioMotionGradient.Prism;
        visualizer.LedBars = false;
        visualizer.MaxFrequency = 20000;
        visualizer.MinFrequency = 20;
        visualizer.Radial = true;
        visualizer.SpinSpeed = 1;
        visualizer.Stereo = true;
    });

    public static VisualizerPreset RadialSpectrumScale => new(4, nameof(RadialSpectrumScale), visualizer =>
    {
        RadialSpectrumStereo.Apply(visualizer);
        visualizer.Stereo = false;
        visualizer.AlphaBars = true;
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

    public static VisualizerPreset BarkScaleLinearAmplitudeWithWaveNode => new(7, nameof(BarkScaleLinearAmplitudeWithWaveNode), visualizer =>
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
        visualizer.GradientLeft = AudioMotionGradient.Steelblue;
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
        visualizer.GradientLeft = AudioMotionGradient.Steelblue;
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

    public static VisualizerPreset NeonBars => new(12, nameof(NeonBars), visualizer =>
    {
        visualizer.Mode = VisualizationMode.OneEighthOctaveBands;
        visualizer.BarSpacing = .25;
        visualizer.FillAlpha = .3;
        visualizer.Gradient = AudioMotionGradient.Prism;
        visualizer.LumiBars = false;
        visualizer.AlphaBars = false;
        visualizer.LineWidth = 1;
        visualizer.OutlineBars = true;
    });

    public static VisualizerPreset LumiBars => new(13, nameof(LumiBars), visualizer =>
    {
        visualizer.Mode = VisualizationMode.OneEighthOctaveBands;
        visualizer.BarSpacing = .25;
        visualizer.FillAlpha = .3;
        visualizer.Gradient = AudioMotionGradient.Prism;
        visualizer.LumiBars = true;
        visualizer.AlphaBars = false;
        visualizer.LineWidth = 1;
        visualizer.OutlineBars = true;
    });

    public static VisualizerPreset GalacticJourney => new(14, nameof(GalacticJourney), visualizer =>
    {
        visualizer.Mode = VisualizationMode.LineAreaGraph;
        //visualizer.Gradient = AudioMotionGradient.Aurora;
        visualizer.Gradient = AudioMotionGradient.Aurora;
        visualizer.MaxFrequency = 18000;
        visualizer.MinFrequency = 40;
        visualizer.LinearAmplitude = false;
        visualizer.ReflexRatio = 0.2;
        visualizer.ReflexAlpha = 0.3;
        visualizer.ShowPeaks = true;
        visualizer.Mirror = MirrorMode.Right;
        visualizer.LineWidth = 0.5;
        visualizer.FillAlpha = 0.4;
        visualizer.Radial = false;
        visualizer.SpinSpeed = 0.5; // Only effective if radial is true, but set for visual consistency
    });

    public static VisualizerPreset DeepSeaDive => new(15, nameof(DeepSeaDive), visualizer =>
    {
        visualizer.Mode = VisualizationMode.DiscreteFrequencies;
        visualizer.Gradient = AudioMotionGradient.DeepSea;
        visualizer.MaxFrequency = 16000;
        visualizer.MinFrequency = 30;
        visualizer.LumiBars = true;
        visualizer.BarSpacing = 0.2;
        visualizer.FillAlpha = 0.5;
        visualizer.AlphaBars = true;
        visualizer.RoundBars = true;
        visualizer.Smoothing = 0.8;
    });

    public static VisualizerPreset RetroVinyl => new(16, nameof(RetroVinyl), visualizer =>
    {
        visualizer.Mode = VisualizationMode.OneThirdOctaveBands;
        visualizer.Gradient = AudioMotionGradient.Sunset;
        visualizer.Radial = true;
        visualizer.SpinSpeed = 0.2;
        visualizer.MaxFrequency = 14000;
        visualizer.MinFrequency = 50;
        visualizer.RadialInvert = true;
        visualizer.BarSpacing = 0.3;
        visualizer.FillAlpha = 0.7;
        visualizer.OutlineBars = false;
    });

    public static VisualizerPreset ElectricPulse => new(17, nameof(ElectricPulse), visualizer =>
    {
        visualizer.Mode = VisualizationMode.LineAreaGraph;
        visualizer.Gradient = AudioMotionGradient.Electric;
        visualizer.LineWidth = 2;
        visualizer.ShowPeaks = false;
        visualizer.LinearAmplitude = true;
        visualizer.MaxFrequency = 12000;
        visualizer.MinFrequency = 60;
        visualizer.FillAlpha = 0.5;
        visualizer.Mirror = MirrorMode.Left;
        visualizer.ReflexRatio = 0.5;
        visualizer.ReflexAlpha = 0.75;
    });

    public static VisualizerPreset AuroraDreams => new(18, nameof(AuroraDreams), visualizer =>
    {
        visualizer.Mode = VisualizationMode.OneFourthOctaveBands;
        visualizer.Gradient = AudioMotionGradient.Aurora;
        visualizer.MaxFrequency = 15000;
        visualizer.MinFrequency = 20;
        visualizer.Smoothing = 0.6;
        visualizer.FillAlpha = 0.3;
        visualizer.AlphaBars = true;
        visualizer.LineWidth = 0.8;
        visualizer.LumiBars = true; 
        visualizer.LedBars = true; 
        visualizer.ShowScaleX = false; 
    });

    public static VisualizerPreset NeonPulse => new(19, nameof(NeonPulse), visualizer =>
    {
        visualizer.Mode = VisualizationMode.LineAreaGraph;
        visualizer.Gradient = AudioMotionGradient.NeonGlow;
        visualizer.LineWidth = 2;
        visualizer.FillAlpha = 0.5;
        visualizer.LinearAmplitude = true;
        visualizer.MaxFrequency = 12000;
        visualizer.MinFrequency = 60;
        visualizer.Mirror = MirrorMode.None;
        visualizer.ReflexRatio = 0.5;
        visualizer.ReflexAlpha = 0.75;
    });

    public static VisualizerPreset CosmicJourney => new(20, nameof(CosmicJourney), visualizer =>
    {
        visualizer.Mode = VisualizationMode.OneFourthOctaveBands;
        visualizer.Gradient = AudioMotionGradient.CosmicPulse;
        visualizer.MaxFrequency = 18000;
        visualizer.MinFrequency = 20;
        visualizer.Smoothing = 0.5;
        visualizer.FillAlpha = 0.3;
        visualizer.AlphaBars = true;
        visualizer.Radial = true;
        visualizer.SpinSpeed = 0.2;
    });

    public static VisualizerPreset MorningRise => new(21, nameof(MorningRise), visualizer =>
    {
        visualizer.Mode = VisualizationMode.OneThirdOctaveBands;
        visualizer.Gradient = AudioMotionGradient.Sunrise;
        visualizer.MaxFrequency = 16000;
        visualizer.MinFrequency = 40;
        visualizer.LumiBars = true;
        visualizer.BarSpacing = 0.2;
        visualizer.FillAlpha = 0.7;
        visualizer.Smoothing = 0.8;
        visualizer.Radial = false;
    });

    public static VisualizerPreset MiamiSpectrumOutline => new(22, nameof(MiamiSpectrumOutline), visualizer =>
    {
        visualizer.Mode = VisualizationMode.OneEighthOctaveBands;
        visualizer.Gradient = AudioMotionGradient.Miami;
        visualizer.MaxFrequency = 20000;
        visualizer.MinFrequency = 20;
        visualizer.BarSpacing = 0.1;
        visualizer.OutlineBars = true;
        visualizer.LineWidth = 2; 
        visualizer.FillAlpha = 0; 
        visualizer.LinearAmplitude = false;
        visualizer.Smoothing = 0.6;
        visualizer.ShowScaleX = true;
    });


}