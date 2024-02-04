using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorJS;
using BlazorJS.Attributes;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Nextended.Core.Helper;

namespace AuralizeBlazor;

public partial class BlazorAudioVisualizer
{
    protected override string ComponentJsFile() => "./_content/AuralizeBlazor/js/components/audioVisualizer.js";
    protected string AudioMotionLib() => "./_content/AuralizeBlazor/js/lib/audioMotion4.4.0.min.js";
    // protected string AudioMotionLib() => "https://cdn.skypack.dev/audiomotion-analyzer?min";
    protected override string ComponentJsInitializeMethodName() => "initializeBlazorAudioVisualizer";

    private ElementReference _visualizer;
    private IJSObjectReference _audioMotion;
    private RenderFragment _childContent;
    private AudioMotionGradient _gradient = AudioMotionGradient.Classic;

    [Parameter]
    public RenderFragment ChildContent
    {
        get => _childContent;
        set
        {
            if (_childContent != value)
            {
                _childContent = value;
                _ = UpdateJsOptions();
            }
        }
    }

    [Parameter, ForJs] 
    public ElementReference[] AudioElements { get; set; }

    [Parameter]
    public bool ConnectAllAudioSources { get; set; }
 
    [Parameter]
    public string Class { get; set; }

    [Parameter]
    public double Opacity { get; set; } = 1;

    [Parameter]
    public double HoverOpacity { get; set; } = 1;

    [Parameter, ForJs]
    public bool ConnectMicrophone { get; set; }

    [Parameter]
    public string Height { get; set; } = "400px";

    [Parameter]
    public string Width { get; set; } = "100%";
    
    public void ApplyPreset(VisualizerPreset preset)
    {
        preset.Apply(this);
    }


    #region Audio Motion Parameters


    [Parameter, ForJs]
    public int Sensitivity { get; set; } = 100;


    [Parameter, ForJs("channelLayout")]
    public ChannelLayout ChannelLayout { get; set; } = ChannelLayout.Single;

    [Parameter, ForJs(IgnoreOnParams = true)]
    public bool Stereo
    {
        get => ChannelLayout != ChannelLayout.Single;
        set => ChannelLayout = value ? ChannelLayout.DualVertical : ChannelLayout.Single;
    }


    [Parameter, ForJs("frequencyScale")]
    public FrequencyScale FrequencyScale { get; set; } = FrequencyScale.Log;

    [Parameter, ForJs("fftSize")]
    public int FFTSize { get; set; } = 8192;

    [Parameter, ForJs("maxDecibels")]
    public int MaxDecibels { get; set; } = -25;

    [Parameter, ForJs("minDecibels")]
    public int MinDecibels { get; set; } = -85;

    [Parameter, ForJs("maxFreq")]
    public int MaxFrequency { get; set; } = 22000;

    [Parameter, ForJs("minFreq")]
    public int MinFrequency { get; set; } = 20;

    [Parameter, ForJs("smoothing")]
    public double Smoothing { get; set; } = 0.5;

    [Parameter, ForJs("volume")]
    public double Volume { get; set; } = 1;

    [Parameter, ForJs("weightingFilter")]
    public WeightingFilter WeightingFilter { get; set; }

    [Parameter, ForJs]
    public VisualizationMode Mode { get; set; } = VisualizationMode.DiscreteFrequencies;

    [Parameter, ForJs("maxFPS")]
    public int MaxFPS { get; set; } = 0;

    [Parameter, ForJs("reflexRatio")]
    public double ReflexRatio { get; set; } = 0;

    [Parameter, ForJs("roundBars")]
    public bool RoundBars { get; set; } = false;

    [Parameter, ForJs("showBgColor")]
    public bool ShowBgColor { get; set; } = true;

    [Parameter, ForJs("showFPS")]
    public bool ShowFPS { get; set; } = false;

    [Parameter, ForJs("showPeaks")]
    public bool ShowPeaks { get; set; } = true;

    [Parameter, ForJs("showScaleX")]
    public bool ShowScaleX { get; set; } = true;

    [Parameter, ForJs("showScaleY")]
    public bool ShowScaleY { get; set; } = false;

    [Parameter, ForJs("spinSpeed")]
    public double SpinSpeed { get; set; } = 0;

    [Parameter, ForJs("splitGradient")]
    public bool SplitGradient { get; set; } = false;

    [Parameter, ForJs("start")]
    public bool IsActive { get; set; } = true;

    [Parameter, ForJs("trueLeds")]
    public bool TrueLeds { get; set; } = false;

    [Parameter, ForJs("useCanvas")]
    public bool UseCanvas { get; set; } = true;


    [Parameter, ForJs("colorMode")]
    public ColorMode ColorMode { get; set; } = ColorMode.Gradient;

    [Parameter, ForJs("gradient")]
    public AudioMotionGradient Gradient
    {
        get => _gradient;
        set => GradientLeft = GradientRight = (_gradient = value);
    }

    [Parameter, ForJs(IgnoreOnParams = true)] public AudioMotionGradient? GradientLeft { get; set; } = null;

    [Parameter, ForJs(IgnoreOnParams = true)] public AudioMotionGradient? GradientRight { get; set; } = null;

    [Parameter, ForJs("alphaBars")]
    public bool AlphaBars { get; set; } = false;

    [Parameter, ForJs("ansiBands")]
    public bool AnsiBands { get; set; } = false;

    [Parameter, ForJs("barSpace")]
    public double BarSpacing { get; set; } = 0.1;

    [Parameter, ForJs("bgAlpha")]
    public double BgAlpha { get; set; } = 0.7;

    [Parameter, ForJs("fillAlpha")]
    public double FillAlpha { get; set; } = 1;

    [Parameter, ForJs("ledBars")]
    public bool LedBars { get; set; } = false;

    [Parameter, ForJs("linearAmplitude")]
    public bool LinearAmplitude { get; set; } = false;

    [Parameter, ForJs("linearBoost")]
    public double LinearBoost { get; set; } = 1;

    [Parameter, ForJs("lineWidth")]
    public double LineWidth { get; set; } = 0;

    [Parameter, ForJs("loRes")]
    public bool LoRes { get; set; } = false;

    [Parameter, ForJs("lumiBars")]
    public bool LumiBars { get; set; } = false;

    [Parameter, ForJs("mirror")]
    public MirrorMode Mirror { get; set; } = MirrorMode.None;

    [Parameter, ForJs("noteLabels")]
    public bool NoteLabels { get; set; } = false;

    [Parameter, ForJs("outlineBars")]
    public bool OutlineBars { get; set; } = false;

    [Parameter, ForJs]
    public bool Overlay { get; set; }

    [Parameter, ForJs("peakLine")]
    public bool PeakLine { get; set; } = false;

    [Parameter, ForJs("radial")]
    public bool Radial { get; set; } = false;

    [Parameter, ForJs("radialInvert")]
    public bool RadialInvert { get; set; }

    [Parameter, ForJs("reflexAlpha")]
    public double ReflexAlpha { get; set; } = 0.15;

    [Parameter, ForJs("reflexBright")]
    public double ReflexBright { get; set; } = 1;

    [Parameter, ForJs("reflexFit")]
    public bool ReflexFit { get; set; }


    #endregion

    public void AddAudioElement(ElementReference audioElement) => AudioElements = AudioElements == null ? new[] { audioElement } : AudioElements.Concat(new[] { audioElement }).ToArray();
    public void RemoveAudioElement(ElementReference audioElement) => AudioElements = AudioElements?.Where(e => e.Id != audioElement.Id).ToArray();
    public void SetAudioElements(params ElementReference[] audioElements) => AudioElements = audioElements;


    public Task ConnectToMicrophone()
    {
        ConnectMicrophone = true;
        return UpdateJsOptions();
    }

    public Task DisconnectFromMicrophone()
    {
        ConnectMicrophone = false;
        return UpdateJsOptions();
    }

    /// <summary>
    /// Toggles the full screen mode and returns true if the current state is full screen otherwise false.
    /// </summary>
    public ValueTask<bool> ToggleFullScreen() => JsReference.InvokeAsync<bool>("toggleFullscreen");
    
    /// <summary>
    /// Toggles the picture in picture mode and returns true if the current state is picture in picture otherwise false.
    /// </summary>
    public ValueTask<bool> TogglePictureInPicture() => JsReference.InvokeAsync<bool>("togglePip");

    private async Task UpdateJsOptions()
    {
        if (JsReference != null)
            await JsReference.InvokeVoidAsync("setOptions", JsOptions());
    }

    protected override async Task OnJsOptionsChanged()
    {
        await UpdateJsOptions();
    }

    private object JsOptions()
    {
        return this.AsJsObject(new
        {
            gradientLeft = GradientLeft.HasValue ? GradientLeft.Value.ToDescriptionString() : Gradient.ToDescriptionString(),
            gradientRight = GradientRight.HasValue ? GradientRight.Value.ToDescriptionString() : Gradient.ToDescriptionString(),
            connectAll = ConnectAllAudioSources || ChildContent != null,
            queryOwner = ChildContent != null && !ConnectAllAudioSources ? _visualizer : default,
            audioMotion = _audioMotion,
            visualizer = _visualizer
        });
    }
    
    /// <summary>
    /// Gets the JavaScript arguments to pass to the component.
    /// </summary>
    public override object[] GetJsArguments() => new[] { ElementReference, CreateDotNetObjectReference(), JsOptions() };

    /// <inheritdoc />
    public override async Task ImportModuleAndCreateJsAsync()
    {
        if (!await JsRuntime.IsNamespaceAvailableAsync("AudioMotionAnalyzer"))
            _audioMotion = await JsRuntime.ImportModuleAsync(AudioMotionLib());
        await base.ImportModuleAndCreateJsAsync();
    }

    private string StyleStr()
    {
        var style = new StringBuilder();
        if(!string.IsNullOrWhiteSpace(Height))
            style.Append($"height: {Height};");
        if (!string.IsNullOrWhiteSpace(Width))
            style.Append($"width: {Width};");
        return style.ToString();
    }
}