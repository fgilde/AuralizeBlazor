using AuralizeBlazor.Types;
using Microsoft.AspNetCore.Components;
using Nextended.Core;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AuralizeBlazor;

/// <summary>
/// Track interface for track selection in the Auralizer component.
/// </summary>
[JsonConverter(typeof(AuralizerTrackJsonConverter))]
public interface IAuralizerTrack
{
    /// <summary>
    /// Label to display for the track.
    /// </summary>
    public string Label { get; }
    
    /// <summary>
    /// The URL of the track to play.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// The URL of the image to display for the track.
    /// </summary>
    public string Image { get; }

    /// <summary>
    /// The lyrics for the track.
    /// </summary>
    public LyricData? Lyrics { get; }
}

/// <summary>
/// Track class for track selection in the Auralizer component.
/// </summary>
public class AuralizerTrack: IAuralizerTrack
{
    public AuralizerTrack()
    {}

    public AuralizerTrack(string url): this(url, Path.GetFileNameWithoutExtension(url))
    {}

    public AuralizerTrack(string url, string label, string image = null)
    {
        Label = label;
        Url = url;
        Image = image;
    }

    /// <summary>
    /// Label to display for the track.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// The URL of the track to play.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// The URL of the image to display for the track.
    /// </summary>
    public string Image { get; set; }

    /// <summary>
    /// The lyrics for the track.
    /// </summary>
    public LyricData Lyrics { get; set; }

    
    public override string ToString() => Label;

}

/// <summary>
/// Deserializes <see cref="IAuralizerTrack"/> as <see cref="AuralizerTrack"/>, e.g. when the track list is set from JavaScript on the web component.
/// </summary>
internal class AuralizerTrackJsonConverter : JsonConverter<IAuralizerTrack>
{
    public override IAuralizerTrack Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<AuralizerTrack>(ref reader, options);

    public override void Write(Utf8JsonWriter writer, IAuralizerTrack value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, value.GetType(), options);
}
