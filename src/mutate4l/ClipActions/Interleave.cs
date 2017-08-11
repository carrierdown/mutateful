using Mutate4l.Cli;
using Mutate4l.Core;
using Mutate4l.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using static Mutate4l.ClipActions.InterleaveMode;

// interleave f3 f4 mode timerange eventrangea 1/8 eventrangeb 1/8 => f6

namespace Mutate4l.ClipActions
{
    public enum InterleaveMode
    {
        Event,
        Time
    }

    public class InterleaveOptions
    {
        public InterleaveMode Mode { get; set; } = Time;
        public int[] Counts { get; set; } = new int[] { 1 };
        public decimal[] Ranges { get; set; } = new decimal[] { 1 };
    }

    /// <summary>
    /// Interleaves the contents of one clip with another (FR: Interleave contents of x clips).
    /// 
    /// <list type="table">
    ///     <listheader>
    ///         <term>Options</term>
    ///         <description>description</description>
    ///     </listheader>
    ///     <item>
    ///         <term>mode</term>
    ///         <description>
    ///         <para>eventcount: Interleaved clip is created by counting events in each clip and intertwining them, such that resulting clip will contain first note of first clip + distance to next note, followed by first note of second clip + distance to next note, and so on.</para>
    ///         <para>timerange: Interleaved clip is created by first splitting both clips at given lengths, like 1/8 or 1/16 (splitting any notes that cross these boundaries). The resulting chunks are then placed into the interleaved clip in an interleaved fashion.</para>
    ///         </description>
    ///     </item>
    /// </list>
    /// </summary>
    public class Interleave
    {
        public static ProcessResult Apply(InterleaveOptions options, params Clip[] clips)
        {
            clips = clips.Where(c => c.Notes.Count > 0).ToArray();
            if (clips.Length < 2)
            {
                return new ProcessResult("Error: Less than two clips with content were specified.");
            }
            decimal position = 0;
            Clip resultClip = new Clip(4, true); // Actual length set below, according to operation

            // todo: add counts
            switch (options.Mode)
            {
                case Event:
                    position = clips[0].Notes[0].Start;
                    while (clips.Any(c => c.Retriggered == false))
                    {
                        foreach (var clip in clips)
                        {
                            var noteInfo = clip.GetNextNoteInfo();
                            resultClip.Notes.Add(new Note(noteInfo.Pitch, position, noteInfo.Duration, noteInfo.Velocity));
                            position += noteInfo.DurationUntilNextNote;
                        }
                    }
                    resultClip.Length = position;
                    break;
                case Time:
                    var srcPositions = new decimal[clips.Length];
                    var clipTraversedStatuses = new bool[clips.Length];
                    var timeRanges = options.Ranges;
                    int timeRangeIndex = 0;
                    
                    while (clipTraversedStatuses.Any(c => c == false))
                    {
                        for (var clipIndex = 0; clipIndex < clips.Length; clipIndex++)
                        {
                            var clip = clips[clipIndex];
                            resultClip.Notes.AddRange(
                                Utility.GetSplitNotesInRangeAtPosition(
                                    srcPositions[clipIndex], 
                                    srcPositions[clipIndex] + timeRanges[timeRangeIndex], 
                                    clips[clipIndex].Notes, 
                                    position
                                )
                            );
                            position += timeRanges[timeRangeIndex];
                            srcPositions[clipIndex] += timeRanges[timeRangeIndex];
                            if (srcPositions[clipIndex] >= clips[clipIndex].Length)
                            {
                                srcPositions[clipIndex] = 0;
                                clipTraversedStatuses[clipIndex] = true;
                            }
                            timeRangeIndex = (timeRangeIndex + 1) % timeRanges.Length;
                        }
                    }
                    resultClip.Length = position;
                    break;
            }
            return new ProcessResult(new Clip[] { resultClip });
        }
    }
}
