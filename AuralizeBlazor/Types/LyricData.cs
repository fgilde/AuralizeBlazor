﻿using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System;

namespace AuralizeBlazor.Types;

/// <summary>
/// Contains the entire lyric data and provides methods for adding, removing, and retrieving.
/// Also contains static factory methods for parsing different formats.
/// </summary>
public class LyricData
{
    private readonly List<LyricLine> _lines = new List<LyricLine>();

    /// <summary>
    ///All lines in the lyric data.
    /// </summary>
    public IReadOnlyList<LyricLine> Lines => _lines.OrderBy(l => l.TimeStamp).ToList();

    /// <summary>
    /// Adds a single line.
    /// </summary>
    public void Add(LyricLine line)
    {
        if (line == null) throw new ArgumentNullException(nameof(line));
        _lines.Add(line);
    }

    /// <summary>
    /// Removes a single line.
    /// </summary>
    public bool Remove(LyricLine line)
    {
        return _lines.Remove(line);
    }

    /// <summary>
    /// Removes all lines within the specified range.
    /// </summary>
    public void Remove(RangeOf<TimeSpan> range)
    {
        if (range == null) throw new ArgumentNullException(nameof(range));
        _lines.RemoveAll(line => line.TimeStamp >= range.Min && line.TimeStamp <= range.Max);
    }

    /// <summary>
    /// Retrieves all lines within the specified range.
    /// </summary>
    public IEnumerable<LyricLine> GetLines(RangeOf<TimeSpan> range)
    {
        if (range == null) throw new ArgumentNullException(nameof(range));
        return _lines.Where(line => line.TimeStamp >= range.Min && line.TimeStamp <= range.Max)
                     .OrderBy(line => line.TimeStamp);
    }

    #region Fabrikmethoden zum Parsen

    // --- LRC-Parser ---

    /// <summary>
    /// Factory method to create a LyricData object from an LRC file.
    /// </summary>
    public static LyricData FromLrcFile(string path)
    {
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        var content = File.ReadAllText(path);
        return FromLrc(content);
    }

    /// <summary>
    /// Factory method to create a LyricData object from an LRC stream.
    /// </summary>
    public static LyricData FromLrc(Stream stream)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        using (var reader = new StreamReader(stream))
        {
            var content = reader.ReadToEnd();
            return FromLrc(content);
        }
    }

    /// <summary>
    /// Factory method to create a LyricData object from an LRC byte array.
    /// </summary>
    public static LyricData FromLrc(byte[] bytes)
    {
        if (bytes == null) throw new ArgumentNullException(nameof(bytes));
        var content = Encoding.UTF8.GetString(bytes);
        return FromLrc(content);
    }

    /// <summary>
    /// Factory method to create a LyricData object from an LRC string.
    /// Format: [mm:ss.xx] Text of the line
    /// </summary>
    public static LyricData FromLrc(string lrcContent)
    {
        if (lrcContent == null) throw new ArgumentNullException(nameof(lrcContent));
        var lyricData = new LyricData();
        var lines = lrcContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var parts = line.Split(']');
            if (parts.Length < 2) continue; // keine gültigen Zeitstempel

            var text = parts.Last().Trim();
            foreach (var part in parts)
            {
                if (part.StartsWith("["))
                {
                    var timeStr = part.TrimStart('[');
                    if (TryParseLrcTime(timeStr, out TimeSpan timeStamp))
                    {
                        lyricData.Add(new LyricLine(timeStamp, text));
                    }
                }
            }
        }

        return lyricData;
    }

    /// <summary>
    /// Parses an LRC time stamp (e.g., "03:15.67" or "03:15").
    /// </summary>
    private static bool TryParseLrcTime(string s, out TimeSpan timeStamp)
    {
        timeStamp = TimeSpan.Zero;
        if (TimeSpan.TryParseExact(s, new[] { @"mm\:ss\.ff", @"mm\:ss" }, CultureInfo.InvariantCulture, out timeStamp))
            return true;
        return false;
    }

    // --- SRT-Parser ---
    /// <summary>
    /// Factory method to create a LyricData object from an SRT file.
    /// </summary>
    public static LyricData FromSrtFile(string path)
    {
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        var content = File.ReadAllText(path);
        return FromSrt(content);
    }

    /// <summary>
    /// Factory method to create a LyricData object from an SRT stream.
    /// </summary>
    public static LyricData FromSrt(Stream stream)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        using (var reader = new StreamReader(stream))
        {
            var content = reader.ReadToEnd();
            return FromSrt(content);
        }
    }

    /// <summary>
    /// Factory method to create a LyricData object from an SRT byte array.
    /// </summary>
    public static LyricData FromSrt(byte[] bytes)
    {
        if (bytes == null) throw new ArgumentNullException(nameof(bytes));
        var content = Encoding.UTF8.GetString(bytes);
        return FromSrt(content);
    }

    /// <summary>
    /// Factory method to create a LyricData object from an SRT string.
    /// Each block in the SRT file must contain at least three lines: index, time stamps, and text.
    /// </summary>
    public static LyricData FromSrt(string srtContent)
    {
        if (srtContent == null) throw new ArgumentNullException(nameof(srtContent));
        var lyricData = new LyricData();
        var blocks = srtContent.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var block in blocks)
        {
            var lines = block.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length >= 3)
            {
                var timeLine = lines[1];
                var times = timeLine.Split(new[] { " --> " }, StringSplitOptions.RemoveEmptyEntries);
                if (times.Length == 2 &&
                    TimeSpan.TryParseExact(times[0].Replace(',', '.'), @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture, out TimeSpan start))
                {
                    // Wir nehmen hier nur die erste Textzeile
                    var text = lines[2];
                    lyricData.Add(new LyricLine(start, text));
                }
            }
        }

        return lyricData;
    }

    // --- TTML-Parser ---
    /// <summary>
    /// Factory method to create a LyricData object from a TTML file.
    /// </summary>
    public static LyricData FromTtmlFile(string path)
    {
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        var doc = XDocument.Load(path);
        return FromTtml(doc);
    }

    /// <summary>
    /// Factory method to create a LyricData object from a TTML stream.
    /// </summary>
    public static LyricData FromTtml(Stream stream)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        var doc = XDocument.Load(stream);
        return FromTtml(doc);
    }

    /// <summary>
    /// Factory method to create a LyricData object from a TTML byte array.
    /// </summary>
    public static LyricData FromTtml(byte[] bytes)
    {
        if (bytes == null) throw new ArgumentNullException(nameof(bytes));
        using (var ms = new MemoryStream(bytes))
        {
            var doc = XDocument.Load(ms);
            return FromTtml(doc);
        }
    }

    private static LyricData FromTtml(XDocument doc)
    {
        var lyricData = new LyricData();
        XNamespace ns = doc.Root.GetDefaultNamespace();
        var paragraphs = doc.Descendants(ns + "p");
        foreach (var p in paragraphs)
        {
            var beginAttr = p.Attribute("begin")?.Value;
            if (beginAttr != null && TryParseTtmlTime(beginAttr, out TimeSpan begin))
            {
                var text = p.Value.Trim();
                lyricData.Add(new LyricLine(begin, text));
            }
        }
        return lyricData;
    }

    private static bool TryParseTtmlTime(string s, out TimeSpan timeStamp)
    {
        timeStamp = TimeSpan.Zero;
        if (TimeSpan.TryParseExact(s, @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture, out timeStamp))
            return true;
        return TimeSpan.TryParse(s, out timeStamp);
    }

    #endregion
}