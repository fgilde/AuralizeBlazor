﻿using System;
using System.Collections.Generic;
using BlazorJS.Attributes;

namespace AuralizeBlazor.Features;

public abstract class VisualizerFeatureBase: IVisualizerFeature
{
    public virtual string OnCanvasDrawCallbackName => "onCanvasDraw";
    public abstract string FullJsNamespace { get;}
    public virtual string[] RequiredJsFiles => Array.Empty<string>();

    public virtual Dictionary<string, object> GetJsOptions() => ForJs.GetJsObjectFrom(this);
}

public interface IVisualizerFeature
{
    public string OnCanvasDrawCallbackName { get; }
    public string FullJsNamespace { get; }
    public string[] RequiredJsFiles { get; }
    public Dictionary<string, object> GetJsOptions();
}