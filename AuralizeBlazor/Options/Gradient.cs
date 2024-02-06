namespace AuralizeBlazor.Options;

public class Gradient
{
    public string Name { get; set; }
    public ColorStrop[] Colors { get; set; }

    public static Gradient Classic => new() {Name = "Classic"};
    public static Gradient Prism => new() {Name = "Prism" };
    public static Gradient Rainbow => new() {Name = "Rainbow" };
    public static Gradient Orangered => new() {Name = "Orangered" };
    public static Gradient Steelblue => new() {Name = "Steelblue" };
    
    public static Gradient[] Gradients = new Gradient[]
    {
        new Gradient
        {
            Name = "Rainbow",
            Colors = new ColorStrop[]
            {
                new ColorStrop { Color = "#FF0000", Position = 0 },
                new ColorStrop { Color = "#FF7F00", Position = 0.166 },
                new ColorStrop { Color = "#FFFF00", Position = 0.333 },
                new ColorStrop { Color = "#00FF00", Position = 0.5 },
                new ColorStrop { Color = "#0000FF", Position = 0.666 },
                new ColorStrop { Color = "#4B0082", Position = 0.833 },
                new ColorStrop { Color = "#9400D3", Position = 1 },
            }
        },
        new Gradient
        {
            Name = "Grayscale",
            Colors = new ColorStrop[]
            {
                new ColorStrop { Color = "#000000", Position = 0 },
                new ColorStrop { Color = "#404040", Position = 0.166 },
                new ColorStrop { Color = "#808080", Position = 0.333 },
                new ColorStrop { Color = "#C0C0C0", Position = 0.5 },
                new ColorStrop { Color = "#FFFFFF", Position = 0.666 },
            }
        },
        new Gradient
        {
            Name = "Fire",
            Colors = new ColorStrop[]
            {
                new ColorStrop { Color = "#000000", Position = 0 },
                new ColorStrop { Color = "#FF0000", Position = 0.166 },
                new ColorStrop { Color = "#FF7F00", Position = 0.333 },
                new ColorStrop { Color = "#FFFF00", Position = 0.5 },
                new ColorStrop { Color = "#FFFFFF", Position = 0.666 },
            }
        },
        new Gradient
        {
            Name = "Ice",
            Colors = new ColorStrop[]
            {
                new ColorStrop { Color = "#000000", Position = 0 },
                new ColorStrop { Color = "#0000FF", Position = 0.166 },
                new ColorStrop { Color = "#4B0082", Position = 0.333 },
            }
        }
    };
}

public class ColorStrop
{
    public string Color { get; set; }
    public double Position { get; set; }
}
