using AuralizeBlazor.Types;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace AuralizeBlazor.Sample;

public class DemoTracksService
{
    private readonly NavigationManager _navigationManager;
    private List<IAuralizerTrack> _eyirishTracks = null;

    public DemoTracksService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }


    private async Task<LyricData?> LoadLyricsAsync(string file)
    {
        try
        {            
            var bytes = await new HttpClient().GetByteArrayAsync(_navigationManager.ToAbsoluteUri(file));
            return bytes is { Length: > 0 } ? (file.EndsWith("ttml") ? LyricData.FromTtml(bytes) : LyricData.FromLrc(bytes)) : null;
        }
        catch (Exception e) { }
        return null;
    }

    public IAuralizerTrack MainDemoTrack => DemoTracks[2];
    public IAuralizerTrack[] DemoTracks => GetDemoTracks().ToArray();
    public IAuralizerTrack[] EyirishTracks => GetEyirish().ToArray();
    
    public IEnumerable<IAuralizerTrack> GetDemoTracks()
    {
        yield return new AuralizerTrack("/Auralizer Celtic.mp3");
        yield return new AuralizerTrack("/Auralizer Dreamy rock.mp3");
        yield return new AuralizerTrack("/Auralizer's Beat.mp3");
        yield return new AuralizerTrack("/Digital Serenade.mp3");
        yield return new AuralizerTrack("/sample.mp3", "Simple Demo 1");
        yield return new AuralizerTrack("/sample2.mp3", "Simple Demo 2");
        yield return new AuralizerTrack("/sample3.mp3", "Simple Demo 3");
        yield return new AuralizerTrack("/sample4.mp3", "Simple Demo 4");
        yield return GetEyirish().First();
    }

    public async Task<List<IAuralizerTrack>> GetEyirishAsync()
    {
        if (_eyirishTracks != null)
            return _eyirishTracks;
        var tracks = GetEyirish().Cast<AuralizerTrack>().ToList();
        tracks[0].Lyrics = await LoadLyricsAsync("Anthem with flo.ttml");
        return _eyirishTracks = tracks.Cast<IAuralizerTrack>().ToList();
    }

    public IEnumerable<IAuralizerTrack> GetEyirish()
    {
        yield return new AuralizerTrack("/1.Anthems-with-Flo.mp3");
        yield return new AuralizerTrack("/2.Brother-from-another-mother.mp3");
        yield return new AuralizerTrack("/3.Brothers-of-the-heart.mp3");
        yield return new AuralizerTrack("/4.Unified-Voice.mp3");
        yield return new AuralizerTrack("/5.Heading-Back-to-Dublin.mp3");
        yield return new AuralizerTrack("/6.Paddys_Roar.mp3");
        yield return new AuralizerTrack("/7.Rosalie.mp3");
        yield return new AuralizerTrack("/8.Fiona's Melody.mp3");
    }
}