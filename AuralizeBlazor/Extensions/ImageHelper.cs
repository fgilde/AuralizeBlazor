using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using StbImageSharp;

namespace AuralizeBlazor.Extensions;

internal static class ImageHelper
{
    public static string GetHashSHA1(this byte[] data)
    {
        using var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
        return string.Concat(sha1.ComputeHash(data).Select(x => x.ToString("X2")));
    }

    public static string ToHex(this Color color) => $"{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";

    public static Task<List<Color>> ExtractMainColorsFromImageAsync(byte[] imageBytes, int maxColors = 16, int sampleInterval = 10)
    {
        return Task.Run(() => ExtractMainColorsFromImage(imageBytes, maxColors, sampleInterval));
    }

    public static List<Color> ExtractMainColorsFromImage(byte[] imageBytes, int maxColors = 16, int sampleInterval = 10)
    {
        var image = ImageResult.FromMemory(imageBytes, ColorComponents.RedGreenBlueAlpha);
        var data = image.Data;

        Dictionary<Color, int> colorCounts = new Dictionary<Color, int>();

        for (int y = 0; y < image.Height; y += sampleInterval)
        {
            for (int x = 0; x < image.Width; x += sampleInterval)
            {
                int i = (y * image.Width + x) * 4;

                // Reduce color precision to group similar colors together
                var pixelColor = Color.FromArgb(data[i + 3], Reduce(data[i]), Reduce(data[i + 1]), Reduce(data[i + 2]));

                colorCounts[pixelColor] = colorCounts.GetValueOrDefault(pixelColor) + 1;
            }
        }

        // Sort by frequency and return the top N colors
        return colorCounts
            .OrderByDescending(c => c.Value)
            .Take(maxColors)
            .Select(c => c.Key)
            .ToList();
    }

    private static byte Reduce(byte component, int bitReduction = 3) => (byte)(component & ~(0xFF >> bitReduction));
}
