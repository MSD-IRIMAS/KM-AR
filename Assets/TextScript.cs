using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Video;

/// <summary>
/// Script class that manages the Kara's text part
/// </summary>
public class TextScript : MonoBehaviour
{
    private static readonly string DELIMITER = "\\k";
    private static readonly float F_MODE_DELAY = 0.25f;

    private TMP_Text text;
    private List<KaraLine> fields_understood = new();
    private VideoPlayer video;
    new private AudioSource audio;
    private DateTime now = DateTime.Today;
    private int current_line = 0;
    private int current_field = 0;
    private bool line_shown = false;
    private float line_time = 0;
    public bool Kara_started { get; set; } = false;
    public bool IsVideo { get; set; } = true;

    /// <summary>
    /// Create and returns a stream from a string
    /// </summary>
    /// <param name="s">The string to stream</param>
    public static Stream GenerateStreamFromString(string s)
    {
        MemoryStream stream = new();
        StreamWriter writer = new(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// Concatenate all strings in strings into a single string separated with sep
    /// </summary>
    /// <param name="strings">The strings to concatenate</param>
    /// <param name="sep">The separator</param>
    public static string ConcatenateWith(IEnumerable<string> strings, string sep = ", ")
    {
        string ret = "";
        foreach (string item in strings)
        {
            ret += item + sep;
        }
        if (sep.Length == 0) return ret;
        else return ret.Remove(ret.Length - sep.Length);
    }

    /// <summary>
    /// Interpret a .ass karaoke line into Pairing object
    /// </summary>
    /// <param name="line">The line to interpret</param>
    /// <returns>A Pairing object containing the interpreted line</returns>
    protected static Pairing InterpretLyrics(string line)
    {
        List<int> delays = new();
        List<string> lyrics = new();

        string[] fields = line.Split(DELIMITER, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string field in fields)
        {
            string delay = "";
            string lyric = "";
            uint count = 0;
            bool fMode = false;

            using CharEnumerator enumerator = field.GetEnumerator();
            enumerator.MoveNext(); count++; // Initialize the enumerator on the first character
            if ((enumerator.Current < 0x30 || enumerator.Current > 0x39) && enumerator.Current != 'f') continue; // If the first character is not a number and isn't 'f', ignore the field
            if (enumerator.Current == 'f') // If the fist character is 'f', activate f mode and go to the next character
            {
                fMode = true;
                enumerator.MoveNext(); count++;
            }

            while (enumerator.Current >= 0x30 && enumerator.Current <= 0x39) // While the current character is a number...
            {
                delay += enumerator.Current;
                enumerator.MoveNext(); count++;
            }

            if (enumerator.Current == '\\') continue; // If the first character after the number is '\' (the beacon marker in .ass files) ignore the field
            enumerator.MoveNext(); count++;

            while (count <= field.Length && enumerator.Current != '{') // While the line is not finished and the current character is not '{'...
            {
                lyric += enumerator.Current;
                enumerator.MoveNext(); count++;
            }

            if (fMode)
            {
                int delayToEnd = int.Parse(delay);
                int delayToBegin = Convert.ToInt32(delayToEnd * F_MODE_DELAY);
                int padding = delayToEnd - delayToBegin;
                delays.Add(delayToBegin);
                lyrics.Add(lyric);
                delays.Add(padding);
                lyrics.Add(string.Empty);
            }
            else
            {
                delays.Add(int.Parse(delay));
                lyrics.Add(lyric);
            }
        }
        return new Pairing(delays, lyrics);
    }

    /// <summary>
    /// Sums all integers of a list
    /// </summary>
    /// <param name="ts">The integers to sum</param>
    /// <returns>The sum of all integers</returns>
    public static int Sum(IEnumerable<int> ts)
    {
        int ret = 0;
        foreach (int t in ts)
        {
            ret += t;
        }
        return ret;
    }

    /// <summary>
    /// Load kara into memory
    /// </summary>
    /// <param name="subs">The kara .ass file to load (the content of the file *not* the path to the file)</param>
    public void LoadKara(string subs)
    {
        if (fields_understood.Count != 0) fields_understood = new();
        int first = subs.IndexOf("[Events]") + "[Events]".Length + 1; // +1 for the newline character
        string lines_cut = subs[first..];
        string[] lines_cut_cut = lines_cut.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        List<List<string>> fields_processed = new();
        foreach (string line in lines_cut_cut)
        {
            string[] fields = line.Split(',');
            if (!(fields[0].StartsWith("Comment") || fields[0].StartsWith("Format")))
            {
                List<string> item = new();
                item.Add(fields[1]);
                item.Add(fields[2]);
                item.Add(ConcatenateWith(fields[9..])); //if the line contains comas, it is split into fields, so we concatenate them back. Also the line  
                fields_processed.Add(item);
            }
        }

        for (int i = 0; i<fields_processed.Count; i++)
        {
            //if this is not the last line and the end of the current line happens after the begining of the next line, replace the end of the current line by the begining of the next line
            if (i < fields_processed.Count-1 && DateTime.Parse(fields_processed[i][1])>DateTime.Parse(fields_processed[i+1][0])) fields_understood.Add(new KaraLine(DateTime.Parse(fields_processed[i][0]), DateTime.Parse(fields_processed[i+1][0]), InterpretLyrics(fields_processed[i][2])));
            //else process the line normaly
            else fields_understood.Add(new KaraLine(DateTime.Parse(fields_processed[i][0]), DateTime.Parse(fields_processed[i][1]), InterpretLyrics(fields_processed[i][2])));
        }

        print("Kara loading done");
    }

    // Start is called before the first frame update
    private void Start()
    {
        text = GetComponentInChildren<TextMeshPro>();
        if (text == null)
        {
            throw new MissingComponentException("Text was not found");
        }
        video = GetComponentInChildren<VideoPlayer>();
        if (video == null)
        {
            throw new MissingComponentException("Video was not found");
        }
        audio = GetComponentInChildren<AudioSource>();
        if (audio == null)
        {
            throw new MissingComponentException("Audio was not found");
        }
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (Kara_started) TextUpdateAlt();
    }

    /// <summary>
    /// Updates the text using the script's own clock
    /// </summary>
    [Obsolete("Use TextUpdateAlt instead")]
    protected void TextUpdate()
    {
        now = now.AddSeconds(Time.fixedDeltaTime); // Increments the internal clock
        if (current_line < fields_understood.Count && now >= fields_understood[current_line].Begin) // If we have not yet reached the last line and if the clock time is equal to or after the line's begin time...
        {
            if (!line_shown) // If the line wasn't shown yet...
            {
                text.text = ConcatenateWith(fields_understood[current_line].Pairing.Lyrics, ""); // ...Show it...
                line_shown = true; // ...And make sure it won't be shawn again
            }
            line_time += Time.fixedDeltaTime; // Increments the time since the begining of the field
            if (current_field < fields_understood[current_line].Pairing.Delays.Count && line_time >= fields_understood[current_line].Pairing.Delays[current_field] / 100f) // If we have not yet reached the end of the line and if the elapsed time since the begining of the field is biger than the delay of the current field (divided by 100 since it's centiseconds)...
            {
                string blue_part = ConcatenateWith(fields_understood[current_line].Pairing.Lyrics.ToArray()[..(current_field + 1)], ""); // ...Create the highlighted part...
                string white_part = ConcatenateWith(fields_understood[current_line].Pairing.Lyrics.ToArray()[(current_field + 1)..], ""); // ...Create the not highlighted part...
                text.text = "<color=blue>" + blue_part + "</color>" + white_part; // ...Show the line...
                line_time = 0; // ...Reset the time since the begining of the field...
                current_field++; // ...And increment the field counter
            }
            if (now >= fields_understood[current_line].End) // Same as above, but with end time
            {
                // Reset some variables...
                text.text = "";
                line_shown = false;
                current_field = 0;
                line_time = 0;
                // ...And increment the line counter
                current_line++;
            }
        }
    }

    /// <summary>
    /// Updates the text using the video or audio clock time
    /// </summary>
    protected void TextUpdateAlt()
    {
        double time;
        if (IsVideo) time = video.clockTime;
        else time = audio.time;
        if (current_line < fields_understood.Count && time >= fields_understood[current_line].Begin.ToSeconds()) // If we have not yet reached the last line and if the video clock time is equal to or after the line's begin time...
        {
            if (!line_shown) // If the line wasn't shown yet...
            {
                text.text = ConcatenateWith(fields_understood[current_line].Pairing.Lyrics, ""); // ...Show it...
                line_shown = true; // ...And make sure it won't be shawn again
            }
            double line_time = time - fields_understood[current_line].Begin.ToSeconds(); // Computes the time elapsed since the begining of the line
            if (current_field < fields_understood[current_line].Pairing.Delays.Count && line_time >= Sum(fields_understood[current_line].Pairing.Delays.ToArray()[..(current_field + 1)]) / 100d) // If we have not yet reached the end of the line and if the elapsed time since the begining of the line is biger than the sum of all delays up to the current field (divided by 100 since it's centiseconds)...
            {
                string blue_part = ConcatenateWith(fields_understood[current_line].Pairing.Lyrics.ToArray()[..(current_field + 1)], ""); // ...Create the highlighted part...
                string white_part = ConcatenateWith(fields_understood[current_line].Pairing.Lyrics.ToArray()[(current_field + 1)..], ""); // ...Create the not highlighted part...
                text.text = "<color=blue>" + blue_part + "</color>" + white_part; // ...Show the line...
                current_field++; // ...And increment the field counter
            }
            if (time >= fields_understood[current_line].End.ToSeconds()) // Same as above, but with end time
            {
                // Reset some variables...
                text.text = "";
                line_shown = false;
                current_field = 0;
                // ...And increment the line counter
                current_line++;
            }
        }
    }

    /// <summary>
    /// Reset the script's variables
    /// </summary>
    public void ResetKara()
    {
        Kara_started = false;
        text.text = "";
        line_shown = false;
        current_line = 0;
        current_field = 0;
        line_time = 0;
        now = DateTime.Today;
    }
}

/// <summary>
/// Represents a Pairing between the lyrics segments and the time they shall appear after the previous one
/// </summary>
public readonly struct Pairing
{
    /// <summary>
    /// Delays after linked lyrics segments shall appear (in centiseconds i.e tens of miliseconds)
    /// </summary>
    public List<int> Delays { get; }
    /// <summary>
    /// The lyrics segments
    /// </summary>
    public List<string> Lyrics { get; }

    public Pairing(List<int> delays, List<string> lyrics)
    {
        Delays = delays;
        Lyrics = lyrics;
    }
}

/// <summary>
/// A line from a Karaoke
/// </summary>
public readonly struct KaraLine
{
    /// <summary>
    /// When the line shall begin
    /// </summary>
    public DateTime Begin { get; }
    /// <summary>
    /// When the line shall end
    /// </summary>
    public DateTime End { get; }
    /// <summary>
    ///  Pairing between the lyrics segments and the time they shall appear after the previous one
    /// </summary>
    public Pairing Pairing { get; }
    
    public KaraLine(DateTime begin, DateTime end, Pairing pairing)
    {
        Begin = begin;
        End = end;
        Pairing = pairing;
    }
}

/// <summary>
/// Static class containing extention methods
/// </summary>
public static class MyExtentions
{
    /// <summary>
    /// Converts the time part of a DateTime Object into seconds
    /// </summary>
    /// <param name="time"></param>
    /// <returns>The number of seconds elapsed since midnight</returns>
    public static double ToSeconds(this DateTime time)
    {
        return time.Hour * 3600 + time.Minute * 60 + time.Second + time.Millisecond * 0.001;
    }

    /// <summary>
    /// Remove unauthorized characters from a string
    /// </summary>
    /// <param name="dirtyString">The string to clean</param>
    /// <returns>The input string without bad characters</returns>
    public static string Sanitize(this string dirtyString)
    {
        string removeChars = "?&^$#@!+-,:;<>'\"\\*";
        string result = dirtyString;

        foreach (char c in removeChars) result = result.Replace(c.ToString(), string.Empty);

        return result;
    }
}