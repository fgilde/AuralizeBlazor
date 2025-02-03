using System;
using System.ComponentModel;
using AuralizeBlazor.Types;
using Nextended.Core.Helper;

namespace AuralizeBlazor.Features;

public class VisualPosition
{
    private VisualPosition(PositionInCanvas position)
    {
        Position = position;
        Name = position.ToDescriptionString();
    }

    public ThicknessOf<double> Margin { get; set; } = new(0, 0, 0, 0);

    /// <summary>
    /// If position is random, this will be the minimum and maximum time to change the position.
    /// </summary>
    public RangeOf<TimeSpan> RandomChangeDebounce { get; set; } = new (TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(1000));

    public RangeOf<double> RandomChangeDebounceInMs
    {
        get => new (RandomChangeDebounce.Min.TotalMilliseconds, RandomChangeDebounce.Max.TotalMilliseconds);
        set => RandomChangeDebounce = new(TimeSpan.FromMilliseconds(value.Min), TimeSpan.FromMilliseconds(value.Max));
    }

    public static implicit operator VisualPosition(PositionInCanvas position) => new(position);
    public static implicit operator PositionInCanvas(VisualPosition position) => position.Position;


    /// <summary>
    /// If position is random, this will be the minimum energy to change the position.
    /// </summary>
    public double RandomMinEnergy { get; set; } = 0.34;

    public string Name { get; private set; }
    
    public PositionInCanvas Position { get; private set; }


    public VisualPosition WithRandomChangeDebounce(TimeSpan min, TimeSpan max)
    {
        RandomChangeDebounce = new(min, max);
        return this;
    }

    public VisualPosition WithRandomMinEnergy(double value)
    {
        RandomMinEnergy = value;
        return this;
    }

    public VisualPosition WithMarginBottomAndTop(float value) => WithMarginBottom(value).WithMarginTop(value);
    public VisualPosition WithMarginLeftAndRight(float value) => WithMarginLeft(value).WithMarginRight(value);

    public VisualPosition WithMarginLeft(float left)
    {
        Margin = new(left, Margin.Top, Margin.Right, Margin.Bottom);
        return this;
    }

    public VisualPosition WithMarginTop(float top)
    {
        Margin = new(Margin.Left, top, Margin.Right, Margin.Bottom);
        return this;
    }

    public VisualPosition WithMarginRight(float right)
    {
        Margin = new(Margin.Left, Margin.Top, right, Margin.Bottom);
        return this;
    }

    public VisualPosition WithMarginBottom(float bottom)
    {
        Margin = new(Margin.Left, Margin.Top, Margin.Right, bottom);
        return this;
    }

    public VisualPosition WithMargin(float left, float top, float right, float bottom)
    {
        Margin = new(left, top, right, bottom);
        return this;
    }
    
    public static VisualPosition From(PositionInCanvas position) => new(position);
    public static VisualPosition TopLeft => new(PositionInCanvas.TopLeft);
    public static VisualPosition TopCenter => new(PositionInCanvas.TopCenter);
    public static VisualPosition TopRight => new(PositionInCanvas.TopRight);
    public static VisualPosition CenterLeft => new(PositionInCanvas.CenterLeft);
    public static VisualPosition CenterCenter => new(PositionInCanvas.CenterCenter);
    public static VisualPosition CenterRight => new(PositionInCanvas.CenterRight);
    public static VisualPosition BottomLeft => new(PositionInCanvas.BottomLeft);
    public static VisualPosition BottomCenter => new(PositionInCanvas.BottomCenter);
    public static VisualPosition BottomRight => new(PositionInCanvas.BottomRight);
    public static VisualPosition Random => new(PositionInCanvas.Random);
}


/// <summary>
/// position.
/// </summary>
public enum PositionInCanvas
{
    /// <summary>
    /// Text will be displayed Top left.
    /// </summary>
    [Description("top-left")]
    TopLeft = 0,

    /// <summary>
    /// Text will be displayed Top center.
    /// </summary>
    [Description("top-center")]
    TopCenter = 1,

    /// <summary>
    /// Text will be displayed Top right.
    /// </summary>
    [Description("top-right")]
    TopRight = 2,

    /// <summary>
    /// Text will be displayed Center left.
    /// </summary>
    [Description("center-left")]
    CenterLeft = 3,

    /// <summary>
    /// Text will be displayed in center.
    /// </summary>
    [Description("center-center")]
    CenterCenter = 4,

    /// <summary>
    /// Text will be displayed Center right.
    /// </summary>
    [Description("center-right")]
    CenterRight = 5,

    /// <summary>
    /// Text will be displayed bottom left.
    /// </summary>
    [Description("bottom-left")]
    BottomLeft = 6,

    /// <summary>
    /// Text will be displayed bottom center.
    /// </summary>
    [Description("bottom-center")]
    BottomCenter = 7,

    /// <summary>
    /// Text will be displayed bottom right.
    /// </summary>
    [Description("bottom-right")]
    BottomRight = 8,

    /// <summary>
    /// Random position will change based on energy
    /// </summary>
    [Description("random")]
    Random = 9,
}