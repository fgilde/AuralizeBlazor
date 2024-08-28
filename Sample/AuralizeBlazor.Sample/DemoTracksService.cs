namespace AuralizeBlazor.Sample;

public class DemoTracksService
{
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

    public IEnumerable<IAuralizerTrack> GetEyirish()
    {
        yield return new AuralizerTrack("/1.Anthems-with-Flo.mp3");
        yield return new AuralizerTrack("/2.Brother-from-another-mother.mp3");
        yield return new AuralizerTrack("/3.Brothers-of-the-heart.mp3");
        yield return new AuralizerTrack("/4.Unified-Voice.mp3");
        yield return new AuralizerTrack("/5.Heading-Back-to-Dublin.mp3");
        yield return new AuralizerTrack("/7.Rosalie.mp3");
        yield return new AuralizerTrack("/8.Fiona's Melody.mp3");
    }
}