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
        EventCount,
        TimeRange
    }

    public class InterleaveOptions
    {
        public InterleaveMode Mode { get; set; } = TimeRange;
        public int[] Counts { get; set; }
        public decimal EventRangeA { get; set; } = 1; // todo: support list of any number of ranges instead
        public decimal EventRangeB { get; set; } = 1; // todo: support list of any number of ranges instead
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
        public static ProcessResult Apply(InterleaveOptions options, params Clip[] clips) // todo: Expand to interleave any list of two clips or more
        {
            clips = clips.Where(c => c.Notes.Count > 0).ToArray();
            if (clips.Length < 2)
            {
                return new ProcessResult("Error: Less than two clips with content were specified.");
            }
            Clip resultClip = new Clip(clips.Sum(c => c.Length), true); // todo: will only work when counts are all set = 1
            decimal position = clips[0].Notes[0].Start;
            var a = clips[0];
            var b = clips[1];

            switch (options.Mode)
            {
                case EventCount:
                    while (clips.Any(c => c.Retriggered == false))
                    {
                        foreach (var clip in clips)
                        {
                            var noteInfo = clip.GetNextNoteInfo();
                            resultClip.Notes.Add(new Note(noteInfo.Pitch, position, noteInfo.Duration, noteInfo.Velocity));
                            position += noteInfo.DurationUntilNextNote;
                        }
                    }
                    resultClip.Length = position; // need to do something similar for TimeRange
                    break;
                case TimeRange:
                    var scrPositions = new decimal[clips.Length];
                    var clipTraversedStatuses = new bool[clips.Length];
                    
                    //srcPositionA = 0,
                    //    srcPositionB = 0;
                    //a.Notes = Utility.SplitNotesAtEvery(a.Notes, options.EventRangeA, b.Length);
                    //b.Notes = Utility.SplitNotesAtEvery(b.Notes, options.EventRangeB, a.Length);

                    while (position < resultClip.Length)
                    {
                        resultClip.Notes.AddRange(Utility.GetNotesInRangeAtPosition(srcPositionA, srcPositionA + options.EventRangeA, a.Notes, position));
                        position += options.EventRangeA;
                        srcPositionA += options.EventRangeA;
                        if (srcPositionA >= a.Length) srcPositionA = 0;

                        resultClip.Notes.AddRange(Utility.GetNotesInRangeAtPosition(srcPositionB, srcPositionB + options.EventRangeB, b.Notes, position));
                        position += options.EventRangeB;
                        srcPositionB += options.EventRangeB;
                        if (srcPositionB >= b.Length) srcPositionB = 0;
                    }
                    break;
            }
            return new ProcessResult(new Clip[] { resultClip });
        }
    }
}
