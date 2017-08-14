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
        public bool AdvanceAll { get; set; } = false; // todo: b
    }

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
            int countIndex = 0;
            Clip resultClip = new Clip(4, true); // Actual length set below, according to operation
            var clipTraversedStatuses = new bool[clips.Length];

            switch (options.Mode)
            {
                case Event:
                    var currentNoteIndexes = new int[clips.Length];
                    position = clips[0].Notes[0].Start;

                    while (clipTraversedStatuses.Any(c => c == false))
                    {
                        for (var clipIndex = 0; clipIndex < clips.Length; clipIndex++)
                        {
                            var clip = clips[clipIndex];
                            for (var repeats = 0; repeats < options.Counts[countIndex % options.Counts.Length]; repeats++)
                            {
                                var note = clip.Notes[currentNoteIndexes[clipIndex]];
                                resultClip.Notes.Add(new Note(note.Pitch, position, note.Duration, note.Velocity));
                                decimal durationUntilNextNote;
                                if (currentNoteIndexes[clipIndex] == clip.Notes.Count - 1)
                                {
                                    durationUntilNextNote = clip.EndDelta;
                                    currentNoteIndexes[clipIndex] = 0;
                                    clipTraversedStatuses[clipIndex] = true;
                                } else
                                {
                                    durationUntilNextNote = clip.Notes[currentNoteIndexes[clipIndex] + 1].Start - clip.Notes[currentNoteIndexes[clipIndex]].Start;
                                }
                                position += durationUntilNextNote;
                            }
                            currentNoteIndexes[clipIndex]++;
                        }
                    }
                    resultClip.Length = position;
                    break;
                case Time:
                    var srcPositions = new decimal[clips.Length];
                    int timeRangeIndex = 0;
                    
                    while (clipTraversedStatuses.Any(c => c == false))
                    {
                        for (var clipIndex = 0; clipIndex < clips.Length; clipIndex++)
                        {
                            var clip = clips[clipIndex];
                            for (var repeats = 0; repeats < options.Counts[countIndex % options.Counts.Length]; repeats++) 
                            {
                                resultClip.Notes.AddRange(
                                    Utility.GetSplitNotesInRangeAtPosition(
                                        srcPositions[clipIndex],
                                        srcPositions[clipIndex] + options.Ranges[timeRangeIndex],
                                        clips[clipIndex].Notes,
                                        position
                                    )
                                );
                                position += options.Ranges[timeRangeIndex];
                            }
                            srcPositions[clipIndex] += options.Ranges[timeRangeIndex];
                            if (srcPositions[clipIndex] >= clips[clipIndex].Length)
                            {
                                srcPositions[clipIndex] = 0;
                                clipTraversedStatuses[clipIndex] = true;
                            }
                            countIndex++;
                            timeRangeIndex = (timeRangeIndex + 1) % options.Ranges.Length; // this means that you cannot use the Counts parameter to have varying time ranges for each repeat
                        }
                    }
                    resultClip.Length = position;
                    break;
            }
            return new ProcessResult(new Clip[] { resultClip });
        }
    }
}
