using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace AuralizeBlazor.Extensions;

internal static class ImageHelper
{
    public static string GetHashSHA1(this byte[] data)
    {
        using var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
        return string.Concat(sha1.ComputeHash(data).Select(x => x.ToString("X2")));
    }

    public static Task<List<Rgba32>> ExtractMainColorsFromImageAsync(byte[] imageBytes, int maxColors = 16,
        int sampleInterval = 10, int? targetHeight = null)
    {
        return Task.Run(() => ExtractMainColorsFromImage(imageBytes, maxColors, sampleInterval, targetHeight));
    }

    public static List<Rgba32> ExtractMainColorsFromImage(byte[] imageBytes, int maxColors = 16, int sampleInterval = 10, int? targetHeight = null)
    {
        using var image = Image.Load<Rgba32>(imageBytes);

        // Calculate the new width to maintain aspect ratio

        // Downscale the image
        if (targetHeight.HasValue)
        {
            int targetWidth = (int)(image.Width * (targetHeight.Value / (float)image.Height));
            image.Mutate(x => x.Resize(targetWidth, targetHeight.Value));
        }

        // After downscaling, proceed with color extraction
        Dictionary<Rgba32, int> colorCounts = new Dictionary<Rgba32, int>();

        for (int y = 0; y < image.Height; y += sampleInterval)
        {
            for (int x = 0; x < image.Width; x += sampleInterval)
            {
                Rgba32 pixelColor = image[x, y];

                // Optional: Reduce color precision to group similar colors together
                pixelColor = ReduceColorPrecision(pixelColor);

                if (colorCounts.ContainsKey(pixelColor))
                {
                    colorCounts[pixelColor]++;
                }
                else
                {
                    colorCounts[pixelColor] = 1;
                }
            }
        }

        // Sort by frequency and return the top N colors
        return colorCounts
            .OrderByDescending(c => c.Value)
            .Take(maxColors)
            .Select(c => c.Key)
            .ToList();
    }

    private static Rgba32 ReduceColorPrecision(Rgba32 color, int bitReduction = 3)
    {
        byte reduce(byte component) => (byte)(component & ~(0xFF >> bitReduction));

        return new Rgba32(reduce(color.R), reduce(color.G), reduce(color.B), color.A);
    }

}