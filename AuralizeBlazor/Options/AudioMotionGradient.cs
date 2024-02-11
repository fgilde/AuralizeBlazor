using System;
using System.Linq;

namespace AuralizeBlazor.Options;

/// <summary>
/// Gradient for the audio motion visualizer.
/// </summary>
public class AudioMotionGradient : IEquatable<AudioMotionGradient>
{
    /// <summary>
    /// Name of the gradient.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Background color of the gradient.
    /// </summary>
    public string BgColor { get; set; }
    
    /// <summary>
    /// Is this is true, the gradient is predefined and will not force the register call for audioMotion.
    /// </summary>
    public bool IsPredefined { get; set; }
    
    /// <summary>
    /// Color stops for the gradient.
    /// </summary>
    public ColorStop[] ColorStops { get; set; }
    
    // PREDEFINED GRADIENTS
    public static AudioMotionGradient Classic => new()
    {
        Name = nameof(Classic),
        IsPredefined = true,
        ColorStops = new[]
        {
            new ColorStop { Color = "red" },
            new ColorStop { Color = "yellow"}, 
            new ColorStop { Color = "lime" } 
        }
    };

    public static AudioMotionGradient Prism => new()
    {
        Name = nameof(Prism),
        IsPredefined = true,
        ColorStops = new[]
        {
            new ColorStop { Color = "#a35" },
            new ColorStop { Color = "#c66" },
            new ColorStop { Color = "#e94" },
            new ColorStop { Color = "#ed0" },
            new ColorStop { Color = "#9d5" },
            new ColorStop { Color = "#4d8" },
            new ColorStop { Color = "#2cb" },
            new ColorStop { Color = "#0bc" },
            new ColorStop { Color = "#09c" },
            new ColorStop { Color = "#36b" }
        }
    };

    public static AudioMotionGradient OrangeRed => new()
    {
        Name = "Orangered",
        IsPredefined = true,
        BgColor = "#3e2f29",
        ColorStops = new[]
        {
            new ColorStop { Color = "OrangeRed" }
        }
    };

    public static AudioMotionGradient Rainbow => new()
    {
        Name = nameof(Rainbow),
        IsPredefined = true,
        ColorStops = new[]
        {
            new ColorStop { Color = "#817" },
            new ColorStop { Color = "#a35" },
            new ColorStop { Color = "#c66" },
            new ColorStop { Color = "#e94" },
            new ColorStop { Color = "#ed0" },
            new ColorStop { Color = "#9d5" },
            new ColorStop { Color = "#4d8" },
            new ColorStop { Color = "#2cb" },
            new ColorStop { Color = "#0bc" },
            new ColorStop { Color = "#09c" },
            new ColorStop { Color = "#36b" },
            new ColorStop { Color = "#639" }
        }
    };
    
    public static AudioMotionGradient Steelblue => new()
    {
        Name = nameof(Steelblue),
        IsPredefined = true,
        BgColor = "#222c35",
        ColorStops = new[]
        {
            new ColorStop { Color = "SteelBlue" }
        }
    };
    

    // CUSTOM GRADIENTS
    public static AudioMotionGradient Greyscale => new()
    {
        Name = nameof(Greyscale),
        ColorStops = new ColorStop[]
        {
            new() { Color = "#000000", Position = 0 },
            new() { Color = "#404040", Position = 0.166 },
            new() { Color = "#808080", Position = 0.333 },
            new() { Color = "#C0C0C0", Position = 0.5 },
            new() { Color = "#FFFFFF", Position = 0.666 }
        }
    };

    public static AudioMotionGradient Fire => new()
    {
        Name = nameof(Fire),
        ColorStops = new ColorStop[]
        {
            new() { Color = "#000000", Position = 0 },
            new() { Color = "#FF0000", Position = 0.166 },
            new() { Color = "#FF7F00", Position = 0.333 },
            new() { Color = "#FFFF00", Position = 0.5 },
            new() { Color = "#FFFFFF", Position = 0.666 }
        }
    };

    public static AudioMotionGradient Ice => new()
    {
        Name = nameof(Ice),
        ColorStops = new ColorStop[]
        {
            new() { Color = "#000000", Position = 0 },
            new() { Color = "#0000FF", Position = 0.166 },
            new() { Color = "#4B0082", Position = 0.333 }
        }
    };

    public static AudioMotionGradient Aurora => new()
    {
        Name = nameof(Aurora),
        ColorStops = new ColorStop[]
        {
            new() { Color = "#88cc88", Position = 0 }, // Soft green
            new() { Color = "#00CCFF", Position = 0.5 }, // Sky blue
            new() { Color = "#8822cc", Position = 1 } // Deep purple
        }
    };

    public static AudioMotionGradient Electric => new()
    {
        Name = nameof(Electric),
        ColorStops = new ColorStop[]
        {
            new() { Color = "#f5f311", Position = 0 }, // Bright yellow
            new() { Color = "#ff0084", Position = 0.5 }, // Hot pink
            new() { Color = "#11f5f3", Position = 1 } // Cyan
        }
    };

    public static AudioMotionGradient Sunset => new()
    {
        Name = nameof(Sunset),
        ColorStops = new ColorStop[]
        {
            new() { Color = "#ff5e62", Position = 0 }, // Soft red
            new() { Color = "#ff9966", Position = 0.5 }, // Orange
            new() { Color = "#ffcccc", Position = 1 } // Pale pink
        }
    };

    public static AudioMotionGradient DeepSea => new()
    {
        Name = nameof(DeepSea),
        ColorStops = new ColorStop[]
        {
            new() { Color = "#002233", Position = 0 }, // Dark blue
            new() { Color = "#004466", Position = 0.5 }, // Medium blue
            new() { Color = "#0088cc", Position = 1 } // Light blue
        }
    };

    public static AudioMotionGradient NeonGlow => new()
    {
        Name = nameof(NeonGlow),
        ColorStops = new ColorStop[]
        {
            new() { Color = "#ff00ff", Position = 0 }, // Bright pink
            new() { Color = "#00ffff", Position = 0.5 }, // Cyan
            new() { Color = "#ff00ff", Position = 1 } // Bright pink again for the glow effect
        }
    };

    public static AudioMotionGradient CosmicPulse => new()
    {
        Name = nameof(CosmicPulse),
        ColorStops = new ColorStop[]
        {
            new() { Color = "#000033", Position = 0 }, // Deep space blue
            new() { Color = "#6600ff", Position = 0.3 }, // Purple
            new() { Color = "#ff00ff", Position = 0.6 }, // Pink
            new() { Color = "#ff6600", Position = 1 } // Orange
        }
    };

    public static AudioMotionGradient Sunrise => new()
    {
        Name = nameof(Sunrise),
        ColorStops = new ColorStop[]
        {
            new() { Color = "#ff5e62", Position = 0 }, // Red
            new() { Color = "#ff9966", Position = 0.5 }, // Orange
            new() { Color = "#ffff99", Position = 1 } // Yellow
        }
    };

    public static AudioMotionGradient SpectrumOutline => new()
    {
        Name = nameof(SpectrumOutline),
        ColorStops = new ColorStop[]
        {
            new() { Color = "#FF4500", Position = 0 }, // Orange Red
            new() { Color = "#FF8C00", Position = 0.2 }, // Dark Orange
            new() { Color = "#FFD700", Position = 0.4 }, // Gold
            new() { Color = "#9ACD32", Position = 0.6 }, // Yellow Green
            new() { Color = "#40E0D0", Position = 0.8 }, // Turquoise
            new() { Color = "#00BFFF", Position = 1 } // Deep Sky Blue
        }
    };

    public static AudioMotionGradient Apple => new()
    {
        Name = "Apple ][",
        ColorStops = new ColorStop[]
        {
        new() { Color = "#61bb46", Position = .1667 },
        new() { Color = "#fdb827", Position = .3333 },
        new() { Color = "#f5821f", Position = .5 },
        new() { Color = "#e03a3e", Position = .6667 },
        new() { Color = "#963d97", Position = .8333 },
        new() { Color = "#009ddc", Position = 1 }
        }
    };


    public static AudioMotionGradient Borealis => new()
    {
        Name = "Borealis",
        BgColor = "#0d1526",
        ColorStops = new ColorStop[]
        {
        new() { Color = "hsl(120, 100%, 50%)", Position = .1 },
        new() { Color = "hsl(189, 100%, 40%)", Position = .5 },
        new() { Color = "hsl(290, 60%, 40%)", Position = 1 }
        }
    };

    public static AudioMotionGradient PacificDream => new ()
    {
        Name = "PacificDream",
        BgColor = "#051319",
        ColorStops = new ColorStop[]
        {
            new() { Color = "#34e89e", Position = .1 },
            new() { Color = "#0f3443", Position = 1 }
        }
    };

    public static AudioMotionGradient Candy => new()
    {
        Name = "Candy",
        BgColor = "#0d0619",
        ColorStops = new ColorStop[]
        {
        new() { Color = "#ffaf7b", Position = .1 },
        new() { Color = "#d76d77", Position = .5 },
        new() { Color = "#3a1c71", Position = 1 }
        }
    };

    public static AudioMotionGradient Cool => new()
    {
        Name = "Cool",
        BgColor = "#0b202b",
        ColorStops = new ColorStop[]
        {
        new() { Color = "hsl(208, 0%, 100%)", Position = 0 },
        new() { Color = "hsl(208, 100%, 35%)", Position = 1 }
        }
    };

    public static AudioMotionGradient Dusk => new()
    {
        Name = "Dusk",
        BgColor = "#0e172a",
        ColorStops = new ColorStop[]
        {
        new() { Color = "hsl(55, 100%, 50%)", Position = .2 },
        new() { Color = "hsl(16, 100%, 50%)", Position = 1 }
        }
    };

    public static AudioMotionGradient Miami => new()
    {
        Name = "Miami",
        BgColor = "#110a11",
        ColorStops = new ColorStop[]
        {
        new() { Color = "rgb(251, 198, 6)", Position = .024 },
        new() { Color = "rgb(224, 82, 95)", Position = .283 },
        new() { Color = "rgb(194, 78, 154)", Position = .462 },
        new() { Color = "rgb(32, 173, 190)", Position = .794 },
        new() { Color = "rgb(22, 158, 95)", Position = 1 }
        }
    };

    public static AudioMotionGradient Outrun => new()
    {
        Name = "Outrun",
        BgColor = "#101",
        ColorStops = new ColorStop[]
        {
        new() { Color = "rgb(255, 223, 67)", Position = 0 },
        new() { Color = "rgb(250, 84, 118)", Position = .182 },
        new() { Color = "rgb(198, 59, 243)", Position = .364 },
        new() { Color = "rgb(133, 80, 255)", Position = .525 },
        new() { Color = "rgb(74, 104, 247)", Position = .688 },
        new() { Color = "rgb(35, 210, 255)", Position = 1 }
        }
    };

    public bool Equals(AudioMotionGradient other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name && BgColor == other.BgColor && (Equals(ColorStops, other.ColorStops) || ColorStops.SequenceEqual(other.ColorStops));
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((AudioMotionGradient)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, BgColor, ColorStops);
    }
}

public class ColorStop
{
    public string Color { get; set; }
    public double Position { get; set; }
}