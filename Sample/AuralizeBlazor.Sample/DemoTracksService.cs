using System.Collections.Concurrent;
using AuralizeBlazor.Types;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Nextended.Core;

namespace AuralizeBlazor.Sample;

public class DemoTracksService
{
    private readonly NavigationManager _navigationManager;
    private List<IAuralizerTrack> _eyirishTracks = null;
    private ConcurrentDictionary<string, LyricData> _lyricsCache = new ConcurrentDictionary<string, LyricData>();

    public DemoTracksService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }


    private async Task<LyricData?> LoadLyricsAsync(string file)
    {
        if (_lyricsCache.TryGetValue(file, out var cached))
            return cached;
        try
        {            
            var bytes = await new HttpClient().GetByteArrayAsync(_navigationManager.ToAbsoluteUri(file));
            if(MimeType.AudioTypes.Any(m => MimeType.GetExtension(m) == Path.GetExtension(file)))
                return LyricData.FromAudioBytes(bytes); 
            var result = bytes is { Length: > 0 } ? (file.EndsWith("ttml") || file.EndsWith("xml") ? LyricData.FromTtml(bytes) : LyricData.FromLrc(bytes)) : null;
            if (result != null)
                _lyricsCache[file] = result;
            return result;
        }
        catch (Exception) { }
        return null;
    }

    public IAuralizerTrack MainDemoTrack => DemoTracks[2];
    public IAuralizerTrack[] DemoTracks => GetDemoTracks().ToArray();
    public IAuralizerTrack[] EyirishTracks => GetEyirish().ToArray();

    public IAuralizerTrack LebenWieImFilm() => new AuralizerTrack("/Leben wie im Film.mp3");

    public IEnumerable<IAuralizerTrack> GetDemoTracks()
    {
        yield return new AuralizerTrack("/Auralizer Celtic.mp3");
        yield return new AuralizerTrack("/Auralizer Dreamy rock.mp3");
        yield return new AuralizerTrack("/Auralizer's Beat.mp3");
        yield return new AuralizerTrack("/Digital Serenade.mp3");
        yield return new AuralizerTrack("/Leaving_of_Liverpool.mp3");
        yield return LebenWieImFilm();
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
        tracks[0].Lyrics = await LoadLyricsAsync("Anthem with flo.xml");
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
        yield return new AuralizerTrack("/Leaving_of_Liverpool.mp3");
    }
}