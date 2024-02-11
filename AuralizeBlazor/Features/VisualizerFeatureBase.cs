using System;
using System.Collections.Generic;
using BlazorJS.Attributes;

namespace AuralizeBlazor.Features;

/// <summary>
/// Base class for visualizer features.
/// </summary>
public abstract class VisualizerFeatureBase: IVisualizerFeature
{
    /// <summary>
    /// Will be set to true if the feature is applied from a preset.
    /// </summary>
    public bool AppliedFromPreset { get; set; }
    
    /// <summary>
    /// Method name to be called when the canvas is drawn.
    /// </summary>
    public virtual string OnCanvasDrawCallbackName => "onCanvasDraw";
    
    /// <summary>
    /// Namespace for the feature in the JavaScript world.
    /// </summary>
    public abstract string FullJsNamespace { get;}
    
    /// <summary>
    /// Files required for the feature to work.
    /// </summary>
    public virtual string[] RequiredJsFiles => Array.Empty<string>();

    /// <summary>
    /// Returns the JavaScript options for the feature.
    /// </summary>
    public virtual Dictionary<string, object> GetJsOptions() => ForJs.GetJsObjectFrom(this);
}

/// <summary>
/// Interface for visualizer features.
/// </summary>
public interface IVisualizerFeature
{
    /// <summary>
    /// Will be set to true if the feature is applied from a preset.
    /// </summary>
    internal bool AppliedFromPreset { get; set; }
    
    /// <summary>
    /// Method name to be called when the canvas is drawn.
    /// </summary>
    public string OnCanvasDrawCallbackName { get; }
    
    /// <summary>
    /// Namespace for the feature in the JavaScript world.
    /// </summary>
    public string FullJsNamespace { get; }
    
    /// <summary>
    /// Files required for the feature to work.
    /// </summary>
    public string[] RequiredJsFiles { get; }
    
    /// <summary>
    /// JavaScript options for the feature.
    /// </summary>
    public Dictionary<string, object> GetJsOptions();
}