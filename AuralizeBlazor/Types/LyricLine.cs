using System;

namespace AuralizeBlazor.Types;

/// <summary>
/// Represents a line of lyrics.
/// </summary>
public class LyricLine
{
    public LyricLine(TimeSpan timeStamp, string text)
    {
        TimeStamp = timeStamp;
        Text = text;
    }

    public TimeSpan TimeStamp { get; set; }
    public string Text { get; set; }
}