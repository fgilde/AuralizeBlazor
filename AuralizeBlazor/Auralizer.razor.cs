using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AuralizeBlazor.Features;
using AuralizeBlazor.Options;
using BlazorJS;
using BlazorJS.Attributes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Nextended.Core.Extensions;

namespace AuralizeBlazor;

/// <summary>
/// Blazor component for visualizing audio data.
/// </summary>
public partial class Auralizer
{
    private const bool Minify = true;
    
    private string _id = Guid.NewGuid().ToFormattedId();
    protected override string ComponentJsFile() => Minify ? "./_content/AuralizeBlazor/js/auralize.min.js" : "./_content/AuralizeBlazor/js/components/auralizer.js";
    protected string AudioMotionLib() => "./_content/AuralizeBlazor/js/lib/audioMotion4.4.0.min.js"; // => "https://cdn.skypack.dev/audiomotion-analyzer?min";
    protected override string ComponentJsInitializeMethodName() => "initializeAuralizer";
    
    private bool _isMessageVisible;
    private bool _created;
    private bool _minOneInputConnected;
    private bool _isPlaying;
    private int _playingElements = 0;
    private int _presetIdx = 0;
    private string _message;
    private string visualizerMouseOverCls;
    private string containerMouseOverCls;
    private ElementReference _visualizer;
    private IJSObjectReference _audioMotion;
    private RenderFragment _childContent;
    private AudioMotionGradient _gradient = AudioMotionGradient.Classic;
    private IVisualizerFeature[] _features = Array.Empty<IVisualizerFeature>();

    
    public bool IsPlaying => _isPlaying;
    
    /// <summary>
    /// Invoked when the mouse pointer enters the container area.
    /// </summary>
    [Parameter] public EventCallback<MouseEventArgs> OnContainerMouseOver { get; set; }

    /// <summary>
    /// Invoked when the mouse pointer leaves the container area.
    /// </summary>
    [Parameter] public EventCallback<MouseEventArgs> OnContainerMouseOut { get; set; }

    /// <summary>
    /// Invoked when the mouse pointer enters the visualizer area.
    /// </summary>
    [Parameter] public EventCallback<MouseEventArgs> OnVisualizerMouseOver { get; set; }

    /// <summary>
    /// Invoked when the mouse pointer leaves the visualizer area.
    /// </summary>
    [Parameter] public EventCallback<MouseEventArgs> OnVisualizerMouseOut { get; set; }

    /// <summary>
    /// Invoked when a preset is applied to the visualizer.
    /// </summary>
    [Parameter] public EventCallback<AuralizerPreset> OnPresetApplied { get; set; }

    /// <summary>
    /// Invoked when the gradient used by the visualizer is changed.
    /// </summary>
    [Parameter] public EventCallback<AudioMotionGradient> GradientChanged { get; set; }

    /// <summary>
    /// Invoked when the features of the visualizer change.
    /// </summary>
    [Parameter] public EventCallback<IVisualizerFeature[]> FeaturesChanged { get; set; }

    /// <summary>
    /// Invoked when playing status changed
    /// </summary>
    [Parameter] public EventCallback<bool> IsPlayingChanged { get; set; }

    /// <summary>
    /// Invoked when an input source is connected to the visualizer.
    /// </summary>
    [Parameter] public EventCallback OnInputConnected { get; set; }

    /// <summary>
    /// Invoked when the visualizer component is created.
    /// </summary>
    [Parameter] public EventCallback OnCreated { get; set; }

    /// <summary>
    /// Invoked when the visualizer component is ready for interaction.
    /// </summary>
    [Parameter] public EventCallback OnReady { get; set; }

    /// <summary>
    /// Indicates whether child content should be overlaid on the visualizer.
    /// </summary>
    [Parameter, ForJs] public bool OverlayChildContent { get; set; }

    /// <summary>
    /// Defines the action taken when the visualizer is clicked.
    /// </summary>
    [Parameter, ForJs("visualizerClickAction")] public VisualizerAction ClickAction { get; set; } = VisualizerAction.None;

    /// <summary>
    /// Defines the action taken when the visualizer is double-clicked.
    /// </summary>
    [Parameter, ForJs("visualizerDblClickAction")] public VisualizerAction DoubleClickAction { get; set; } = VisualizerAction.None;

    /// <summary>
    /// Defines the action taken when the context menu is invoked on the visualizer.
    /// </summary>
    [Parameter, ForJs("visualizerCtxMenuAction")] public VisualizerAction ContextMenuAction { get; set; } = VisualizerAction.None;

    /// <summary>
    /// Defines the action taken when the mouse wheel is scrolled up over the visualizer.
    /// </summary>
    [Parameter] public VisualizerAction MouseWheelUpAction { get; set; } = VisualizerAction.PreviousPreset;

    /// <summary>
    /// Defines the action taken when the mouse wheel is scrolled down over the visualizer.
    /// </summary>
    [Parameter] public VisualizerAction MouseWheelDownAction { get; set; } = VisualizerAction.NextPreset;

    /// <summary>
    /// Determines whether the name of the preset is shown when it is changed.
    /// </summary>
    [Parameter] public bool ShowPresetNameOnChange { get; set; }

    /// <summary>
    /// Specifies the initial preset to be applied to the visualizer.
    /// </summary>
    [Parameter] public AuralizerPreset InitialPreset { get; set; }

    /// <summary>
    /// All here added presets will be available for the user to apply to the visualizer by using mousewheel.
    /// </summary>
    [Parameter]
    public AuralizerPreset[] Presets { get; set; }

    /// <summary>
    /// Sets the background image of the visualizer component.
    /// </summary>
    [Parameter, ForJs]
    public string BackgroundImage { get; set; }

    /// <summary>
    /// If a preset is applied this properties will be ignored when the preset is applied and a reset is triggered from the preset.
    /// </summary>
    [Parameter]
    public string[] IgnoredPropertiesForReset { get; set; }

    /// <summary>
    /// The content to be rendered within the visualizer component. All here containing audio and video elements will be connected to the visualizer.
    /// </summary>
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

    /// <summary>
    /// Array of visualizer features to be applied to the visualizer.
    /// </summary>
    [Parameter, ForJs(IgnoreOnParams = true)]
    public IVisualizerFeature[] Features
    {
        get => _features;
        set
        {
            if (_features == value || (_features != null && value != null && _features.SequenceEqual(value) ))
                return;
            _features = value;
            FeaturesChanged.InvokeAsync(Features);
            _ = ImportFeatureFilesAsync();
        }
    }

    /// <summary>
    /// An array of audio elements (e.g., audio or video tags) to be visualized.
    /// </summary>
    [Parameter, ForJs]
    public ElementReference[] AudioElements { get; set; }

    /// <summary>
    /// A flag indicating whether all audio sources should be connected to the visualizer.
    /// </summary>
    [Parameter]
    public bool ConnectAllAudioSources { get; set; }

    /// <summary>
    ///  CSS class for the visualizer component.
    /// </summary>
    [Parameter]
    public string Class { get; set; }

    /// <summary>
    ///  CSS class for the visualizer component.
    /// </summary>
    [Parameter]
    public string Style { get; set; }

    /// <summary>
    /// Opacity of the visualizer component.
    /// </summary>
    [Parameter]
    public double Opacity { get; set; } = 1;

    /// <summary>
    /// Opacity of the visualizer component when hovered.
    /// </summary>
    [Parameter]
    public double HoverOpacity { get; set; } = 1;

    /// <summary>
    /// Connects the microphone to the visualizer.
    /// </summary>
    [Parameter, ForJs]
    public bool ConnectMicrophone { get; set; }

    /// <summary>
    /// Height of the visualizer component.
    /// </summary>
    [Parameter]
    public string Height { get; set; } = "400px";

    /// <summary>
    /// Width of the visualizer component.
    /// </summary>
    [Parameter]
    public string Width { get; set; } = "100%";

    /// <summary>
    /// Applies the given preset to the visualizer.
    /// </summary>
    public Auralizer ApplyPreset(AuralizerPreset preset, bool? resetFirst = null) => ExecuteApplyPreset(preset, resetFirst);

    private Auralizer ExecuteApplyPreset(AuralizerPreset preset, bool? resetFirst = null, bool messageIf = true)
    {
        if (Presets?.Contains(preset) == true)
            _presetIdx = Array.IndexOf(Presets, preset);
        if (messageIf && ShowPresetNameOnChange)
            ShowMessage(preset.Name, TimeSpan.FromSeconds(2));
        preset.Apply(this, resetFirst);
        OnPresetApplied.InvokeAsync(preset);
        return this;
    }

    private CancellationTokenSource _messageHideCts;
    private bool _isActive = true;

    /// <summary>
    /// Displays a message on the visualizer for the specified duration.
    /// </summary>
    public void ShowMessage(string message, TimeSpan hideAfter)
    {
        _messageHideCts?.Cancel();
        _messageHideCts = new CancellationTokenSource();

        _message = message;
        _isMessageVisible = true;
        InvokeAsync(StateHasChanged); 

        var currentCts = _messageHideCts;
        Task.Delay(hideAfter, currentCts.Token).ContinueWith(task =>
        {
            if (!task.IsCanceled)
            {
                _isMessageVisible = false;
                InvokeAsync(() =>
                {
                    StateHasChanged();
                    Task.Delay(500, currentCts.Token).ContinueWith(_ => 
                    {
                        if (!currentCts.Token.IsCancellationRequested)
                        {
                            _message = null;
                            InvokeAsync(StateHasChanged);
                        }
                    }, currentCts.Token);
                });
            }
        }, currentCts.Token);
    }

    /// <summary>
    /// Removes a visualizer feature from the visualizer.
    /// </summary>
    public Auralizer RemoveFeature<T>() where T : IVisualizerFeature => RemoveFeature(typeof(T));

    /// <summary>
    /// Removes a visualizer feature from the visualizer.
    /// </summary>
    public Auralizer RemoveFeature(Type featureType) => RemoveFeature(_features.Where(f => f.GetType() == featureType).ToArray());

    /// <summary>
    /// Removes a visualizer feature from the visualizer.
    /// </summary>
    public Auralizer RemoveFeature(params IVisualizerFeature[] feature)
    {
        _features = _features.Except(feature).ToArray();
        FeaturesChanged.InvokeAsync(Features);
        return this;
    }

    /// <summary>
    /// Adds a visualizer feature to the visualizer.
    /// </summary>
    public Auralizer AddFeature<T>(T feature) where T : IVisualizerFeature
    {
        Features = _features.Concat(new IVisualizerFeature[] { feature }).ToArray();
        return this;
    }


    #region Audio Motion Parameters

    /// <summary>
    /// Defines the internal radius of the radial spectrum. Must be between 0 and 1.
    /// </summary>
    [Parameter, ForJs] public double Radius { get; set; } = .3;

    /// <summary>
    /// Sets the sensitivity of the visualizer. Higher values make it more sensitive to sound.
    /// </summary>
    [Parameter, ForJs]
    public int Sensitivity { get; set; } = 100;

    /// <summary>
    /// Specifies the channel layout for the visualizer (e.g., single, dual-vertical).
    /// </summary>
    [Parameter, ForJs("channelLayout")]
    public ChannelLayout ChannelLayout { get; set; } = ChannelLayout.Single;

    /// <summary>
    /// Determines whether to use stereo mode, changing the channel layout accordingly.
    /// </summary>
    [Parameter, ForJs(IgnoreOnParams = true)]
    public bool Stereo
    {
        get => ChannelLayout != ChannelLayout.Single;
        set => ChannelLayout = value ? ChannelLayout.DualVertical : ChannelLayout.Single;
    }

    /// <summary>
    /// Specifies the frequency scale used by the visualizer (e.g., logarithmic, linear).
    /// </summary>
    [Parameter, ForJs("frequencyScale")]
    public FrequencyScale FrequencyScale { get; set; } = FrequencyScale.Log;

    /// <summary>
    /// Sets the FFT size for the audio processing. Higher values provide more detail.
    /// </summary>
    [Parameter, ForJs("fftSize")]
    public int FFTSize { get; set; } = 8192;

    /// <summary>
    /// Sets the maximum decibel level for the visualizer.
    /// </summary>
    [Parameter, ForJs("maxDecibels")]
    public int MaxDecibels { get; set; } = -25;

    /// <summary>
    /// Sets the minimum decibel level for the visualizer.
    /// </summary>
    [Parameter, ForJs("minDecibels")]
    public int MinDecibels { get; set; } = -85;

    /// <summary>
    /// Specifies the maximum frequency displayed by the visualizer.
    /// </summary>
    [Parameter, ForJs("maxFreq")]
    public int MaxFrequency { get; set; } = 22000;

    /// <summary>
    /// Specifies the minimum frequency displayed by the visualizer.
    /// </summary>
    [Parameter, ForJs("minFreq")]
    public int MinFrequency { get; set; } = 20;

    /// <summary>
    /// Controls the smoothing time constant for the visualizer.
    /// </summary>
    [Parameter, ForJs("smoothing")]
    public double Smoothing { get; set; } = 0.5;

    /// <summary>
    /// Adjusts the volume for the audio processing.
    /// </summary>
    [Parameter, ForJs("volume")]
    public double Volume { get; set; } = 1;

    /// <summary>
    /// Applies a weighting filter for the frequency data visualization.
    /// </summary>
    [Parameter, ForJs("weightingFilter")]
    public WeightingFilter WeightingFilter { get; set; }

    /// <summary>
    /// Sets the visualization mode for the audio spectrum (e.g., discrete frequencies).
    /// </summary>
    [Parameter, ForJs]
    public VisualizationMode Mode { get; set; } = VisualizationMode.DiscreteFrequencies;

    /// <summary>
    /// Limits the maximum frames per second (FPS) for the visualizer.
    /// </summary>
    [Parameter, ForJs("maxFPS")]
    public int MaxFPS { get; set; } = 0;

    /// <summary>
    /// Adjusts the ratio of the reflex effect in the visualizer.
    /// </summary>
    [Parameter, ForJs("reflexRatio")]
    public double ReflexRatio { get; set; } = 0;

    /// <summary>
    /// Enables or disables round bars in the visualizer.
    /// </summary>
    [Parameter, ForJs("roundBars")]
    public bool RoundBars { get; set; } = false;

    /// <summary>
    /// Controls the background color visibility of the visualizer.
    /// </summary>
    [Parameter, ForJs("showBgColor")]
    public bool ShowBgColor { get; set; } = true;

    /// <summary>
    /// Toggles the display of frames per second (FPS) on the visualizer.
    /// </summary>
    [Parameter, ForJs("showFPS")]
    public bool ShowFPS { get; set; } = false;

    /// <summary>
    /// Determines whether amplitude peaks are shown in the visualizer.
    /// </summary>
    [Parameter, ForJs("showPeaks")]
    public bool ShowPeaks { get; set; } = true;

    /// <summary>
    /// Controls the visibility of the scale on the X-axis.
    /// </summary>
    [Parameter, ForJs("showScaleX")]
    public bool ShowScaleX { get; set; } = true;

    /// <summary>
    /// Controls the visibility of the scale on the Y-axis.
    /// </summary>
    [Parameter, ForJs("showScaleY")]
    public bool ShowScaleY { get; set; } = false;

    /// <summary>
    /// Sets the rotation speed for the radial visualization mode.
    /// </summary>
    [Parameter, ForJs("spinSpeed")]
    public double SpinSpeed { get; set; } = 0;

    /// <summary>
    /// Splits the color gradient between left and right channels in dual layouts.
    /// </summary>
    [Parameter, ForJs("splitGradient")]
    public bool SplitGradient { get; set; } = false;

    /// <summary>
    /// Controls whether the visualizer is active and starts processing audio immediately.
    /// </summary>
    [Parameter, ForJs("start")]
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive != value)
            {
                _isActive = value;
                UpdateIsPlaying(0);
            }
        }
    }

    /// <summary>
    /// When true, renders LED bars with individual colors from the current gradient.
    /// </summary>
    [Parameter, ForJs("trueLeds")]
    public bool TrueLeds { get; set; } = false;

    /// <summary>
    /// Determines whether the visualizer uses a canvas for rendering.
    /// </summary>
    [Parameter, ForJs("useCanvas")]
    public bool UseCanvas { get; set; } = true;

    /// <summary>
    /// Selects the color mode for the visualizer bars.
    /// </summary>
    [Parameter, ForJs("colorMode")]
    public ColorMode ColorMode { get; set; } = ColorMode.Gradient;

    /// <summary>
    /// Specifies the color gradient used for the visualizer.
    /// </summary>
    [Parameter, ForJs("gradient")]
    public AudioMotionGradient Gradient
    {
        get => _gradient;
        set => GradientLeft = GradientRight = (_gradient = CheckGradientChange(value));
    }

    private AudioMotionGradient CheckGradientChange(AudioMotionGradient value)
    {
        if (!value.Equals(_gradient))
            HandleOnGradientChanged(value);
        return value;
    }

    /// <summary>
    /// Specifies the color gradient for the left channel (dual layouts only).
    /// </summary>
    [Parameter, ForJs(IgnoreOnParams = true)] public AudioMotionGradient? GradientLeft { get; set; } = null;

    /// <summary>
    /// Specifies the color gradient for the right channel (dual layouts only).
    /// </summary>
    [Parameter, ForJs(IgnoreOnParams = true)] public AudioMotionGradient? GradientRight { get; set; } = null;

    /// <summary>
    /// Enables or disables alpha transparency for the bars based on their amplitude.
    /// </summary>
    [Parameter, ForJs("alphaBars")]
    public bool AlphaBars { get; set; } = false;

    /// <summary>
    /// Uses ANSI/IEC preferred frequencies for octave bands (affects visualization modes).
    /// </summary>
    [Parameter, ForJs("ansiBands")]
    public bool AnsiBands { get; set; } = false;

    /// <summary>
    /// Adjusts the spacing between bars in the visualizer.
    /// </summary>
    [Parameter, ForJs("barSpace")]
    public double BarSpacing { get; set; } = 0.1;

    /// <summary>
    /// Controls the background opacity of the visualizer.
    /// </summary>
    [Parameter, ForJs("bgAlpha")]
    public double BgAlpha { get; set; } = 0.7;

    /// <summary>
    /// Sets the fill opacity for bars or the graph area in the visualizer.
    /// </summary>
    [Parameter, ForJs("fillAlpha")]
    public double FillAlpha { get; set; } = 1;

    /// <summary>
    /// Toggles the LED bars effect for the visualizer.
    /// </summary>
    [Parameter, ForJs("ledBars")]
    public bool LedBars { get; set; } = false;

    /// <summary>
    /// Switches the amplitude scale from decibels to a linear representation.
    /// </summary>
    [Parameter, ForJs("linearAmplitude")]
    public bool LinearAmplitude { get; set; } = false;

    /// <summary>
    /// Boosts low-energy frequencies in linear amplitude mode.
    /// </summary>
    [Parameter, ForJs("linearBoost")]
    public double LinearBoost { get; set; } = 1;

    /// <summary>
    /// Sets the line width for graph mode or the outline of bars.
    /// </summary>
    [Parameter, ForJs("lineWidth")]
    public double LineWidth { get; set; } = 0;

    /// <summary>
    /// Enables low-resolution mode to improve performance on high-DPI displays.
    /// </summary>
    [Parameter, ForJs("loRes")]
    public bool LoRes { get; set; } = false;

    /// <summary>
    /// Enables luminance bars effect, displaying bars at full height with varying opacity.
    /// </summary>
    [Parameter, ForJs("lumiBars")]
    public bool LumiBars { get; set; } = false;

    /// <summary>
    /// Configures the mirroring effect for the visualizer.
    /// </summary>
    [Parameter, ForJs("mirror")]
    public MirrorMode Mirror { get; set; } = MirrorMode.None;

    /// <summary>
    /// Displays musical note labels instead of frequency values on the X-axis.
    /// </summary>
    [Parameter, ForJs("noteLabels")]
    public bool NoteLabels { get; set; } = false;

    /// <summary>
    /// Renders analyzer bars with an outline.
    /// </summary>
    [Parameter, ForJs("outlineBars")]
    public bool OutlineBars { get; set; } = false;

    /// <summary>
    /// Enables the overlay mode, allowing the visualizer to be displayed over other content.
    /// </summary>
    [Parameter, ForJs]
    public bool Overlay { get; set; }

    /// <summary>
    /// Connects peak values with a line in graph mode.
    /// </summary>
    [Parameter, ForJs("peakLine")]
    public bool PeakLine { get; set; } = false;

    /// <summary>
    /// Enables the radial visualization mode.
    /// </summary>
    [Parameter, ForJs("radial")]
    public bool Radial { get; set; } = false;

    /// <summary>
    /// Inverts the direction of radial bars towards the center.
    /// </summary>
    [Parameter, ForJs("radialInvert")]
    public bool RadialInvert { get; set; }

    /// <summary>
    /// Sets the opacity for the reflex effect in the visualizer.
    /// </summary>
    [Parameter, ForJs("reflexAlpha")]
    public double ReflexAlpha { get; set; } = 0.15;

    /// <summary>
    /// Adjusts the brightness for the reflex effect.
    /// </summary>
    [Parameter, ForJs("reflexBright")]
    public double ReflexBright { get; set; } = 1;

    /// <summary>
    /// Determines whether the reflex effect should fit the canvas height.
    /// </summary>
    [Parameter, ForJs("reflexFit")]
    public bool ReflexFit { get; set; }


    #endregion

    /// <summary>
    /// Adds an audio element to the visualizer.
    /// </summary>
    public void AddAudioElement(ElementReference audioElement) => AudioElements = AudioElements == null ? new[] { audioElement } : AudioElements.Concat(new[] { audioElement }).ToArray();
    
    /// <summary>
    /// Removes an audio element from the visualizer.
    /// </summary>
    public void RemoveAudioElement(ElementReference audioElement) => AudioElements = AudioElements?.Where(e => e.Id != audioElement.Id).ToArray();
    
    /// <summary>
    /// Sets the audio elements to be visualized.
    /// </summary>
    public void SetAudioElements(params ElementReference[] audioElements) => AudioElements = audioElements;

    /// <summary>
    /// Connects the microphone to the visualizer.
    /// </summary>
    public Task ConnectToMicrophone()
    {
        ConnectMicrophone = true;
        return UpdateJsOptions();
    }

    /// <summary>
    /// Disconnects the microphone from the visualizer.
    /// </summary>
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
        await ImportFeatureFilesAsync();
        if (InitialPreset != null)
        {
            ExecuteApplyPreset(InitialPreset, null, false);
            await UpdateJsOptions();
        }
    }

    /// <summary>
    /// Applies a random preset from the Presets to the visualizer.
    /// </summary>
    [JSInvokable]
    public Task RandomPreset()
    {
        if (Presets?.Any() == true)
        {
            ApplyPreset(Presets.MinBy(p => Guid.NewGuid()));
            return UpdateJsOptions();
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Applies the next preset from the Presets to the visualizer.
    /// </summary>
    [JSInvokable]
    public Task NextPresetAsync() => SelectPreset(1);

    /// <summary>
    /// Applies the previous preset from the Presets to the visualizer.
    /// </summary>
    /// <returns></returns>
    [JSInvokable]
    public Task PreviousPresetAsync() => SelectPreset(-1);

    [JSInvokable]
    public void HandleOnPlay()
    {
        UpdateIsPlaying(1);
    }

    [JSInvokable]
    public void HandleOnPause()
    {
        UpdateIsPlaying(-1);
    }    
    
    [JSInvokable]
    public void HandleOnEnded()
    {
        UpdateIsPlaying(-1);
    }

    private void UpdateIsPlaying(int c)
    {
        _playingElements += c;
        var isPlaying = _playingElements > 0 && IsActive;
        if (_isPlaying != isPlaying)
        {
            _isPlaying = isPlaying;
            HandleIsPlayingChanged(_isPlaying);
        }
    }

    [JSInvokable]
    public void HandleOnInputDisconnected()
    {
        
    }

    [JSInvokable]
    public void HandleOnInputConnected()
    {
        if(_minOneInputConnected)
            return;
        _minOneInputConnected = true;
        OnInputConnected.InvokeAsync();
        if (_created)
            OnReady.InvokeAsync();
    }    
    
    [JSInvokable]
    public void HandleOnCreated()
    {
        if(_created)
            return;
        _created = true;
        OnCreated.InvokeAsync();
        if(_minOneInputConnected)
            OnReady.InvokeAsync();
    }

    protected override async Task OnJsOptionsChanged()
    {
        await UpdateJsOptions();
    }

    /// <summary>
    /// Called playing status changed
    /// </summary>
    protected virtual void HandleIsPlayingChanged(bool value)
    {
        IsPlayingChanged.InvokeAsync(value);
    }

    /// <summary>
    /// Called when the gradient is changed.
    /// </summary>
    protected virtual void HandleOnGradientChanged(AudioMotionGradient value)
    {
        GradientChanged.InvokeAsync(value);
    }

    protected virtual Task HandleContainerMouseOver(MouseEventArgs arg)
    {
        containerMouseOverCls = "mouse-over";
        return OnContainerMouseOver.InvokeAsync(arg);
    }

    protected virtual Task HandleContainerMouseOut(MouseEventArgs arg)
    {
        containerMouseOverCls = null;
        return OnContainerMouseOut.InvokeAsync(arg);
    }

    protected virtual Task HandleVisualizerMouseOver(MouseEventArgs arg)
    {
        visualizerMouseOverCls = "mouse-over";
        return OnVisualizerMouseOver.InvokeAsync(arg);
    }

    protected virtual Task HandleVisualizerMouseOut(MouseEventArgs arg)
    {
        visualizerMouseOverCls = null;
        return OnVisualizerMouseOut.InvokeAsync(arg);
    }

    private async Task UpdateJsOptions()
    {
        if (JsReference != null)
            await JsReference.InvokeVoidAsync("setOptions", JsOptions());
    }

    private Task ImportFeatureFilesAsync()
    {
        var fileNames = Features.SelectMany(f => f.RequiredJsFiles).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToArray();
        return JsRuntime.LoadFilesAsync(fileNames);
    }

    private object JsOptions()
    {
        return this.AsJsObject(new
        {
            gradientLeft = GradientLeft ?? Gradient,
            gradientRight = GradientRight ?? Gradient,
            connectAll = ConnectAllAudioSources || ChildContent != null,
            queryOwner = ChildContent != null && !ConnectAllAudioSources ? ElementReference : default,
            audioMotion = _audioMotion,
            visualizer = _visualizer,
            features = Features.Select(f => new
            {
                onCanvasDrawCallbackName = f.OnCanvasDrawCallbackName,
                jsNamespace = f.FullJsNamespace,
                options = f.GetJsOptions()
            }).ToArray()
        });
    }

    private string StyleStr()
    {
        var style = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(Height))
            style.Append($"height: {Height};");
        if (!string.IsNullOrWhiteSpace(Width))
            style.Append($"width: {Width};");
        return style.ToString();
    }

    private Task HandleMouseWheel(WheelEventArgs arg)
    {
        //MaxFrequency = Math.Max(20, MaxFrequency + (int)arg.DeltaY);
        //return UpdateJsOptions();
        return JsReference
            .InvokeVoidAsync("handleAction", arg.DeltaY > 0 ? MouseWheelDownAction : MouseWheelUpAction, arg).AsTask();
    }

    private Task SelectPreset(int delta)
    {
        if (Presets == null || Presets.Length == 0)
            return Task.CompletedTask;
        int nextIndex = (_presetIdx + delta + Presets.Length) % Presets.Length;

        ApplyPreset(Presets[_presetIdx = nextIndex]);
        return UpdateJsOptions();
    }
}