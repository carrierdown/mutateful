using Mutate4l.Core;
using Mutate4l.Dto;
using Mutate4l.Options;
using Mutate4l.Utility;
using System.Linq;
using static Mutate4l.Options.InterleaveMode;

namespace Mutate4l.Commands
{
    public class Interleave
    {
        public static ProcessResult Apply(InterleaveOptions options, params Clip[] clips)
        {
            if (clips.Length < 2)
            {
                clips = new Clip[] { clips[0], clips[0] };
            }
            decimal position = 0;
            int repeatsIndex = 0;
            Clip resultClip = new Clip(4, true); // Actual length set below, according to operation
            var clipTraversedStatuses = new bool[clips.Length];

            switch (options.Mode)
            {
                case Event:
                    var noteCounters = clips.Select(c => new IntCounter(c.Notes.Count)).ToArray();
                    position = clips[0].Notes[0].Start;

                    while (noteCounters.Any(nc => !nc.Overflow))
                    {
                        for (var clipIndex = 0; clipIndex < clips.Length; clipIndex++)
                        {
                            var clip = clips[clipIndex];
                            var currentNoteIndex = noteCounters[clipIndex].Value;

                            for (var repeats = 0; repeats < options.Repeats[repeatsIndex % options.Repeats.Length]; repeats++)
                            {
                                var note = clip.Notes[currentNoteIndex];
                                resultClip.Notes.Add(new Note(note.Pitch, position, note.Duration, note.Velocity));
                                position += clip.DurationUntilNextNote(currentNoteIndex);
                            }
                            if (options.Mask)
                            {
                                foreach (var noteCounter in noteCounters)
                                {
                                    noteCounter.Inc();
                                }
                            }
                            else
                            {
                                noteCounters[clipIndex].Inc();
                            }
                        }
                    }
                    resultClip.Length = position;
                    break;
                case Time:
                    var srcPositions = clips.Select(c => new DecimalCounter(c.Length)).ToArray();
                    int timeRangeIndex = 0;
                    
                    while (srcPositions.Any(c => !c.Overflow))
                    {
                        for (var clipIndex = 0; clipIndex < clips.Length; clipIndex++)
                        {
                            var clip = clips[clipIndex];
                            var currentTimeRange = options.Ranges[timeRangeIndex];
                            for (var repeats = 0; repeats < options.Repeats[repeatsIndex % options.Repeats.Length]; repeats++) 
                            {
                                resultClip.Notes.AddRange(
                                    ClipUtilities.GetSplitNotesInRangeAtPosition(
                                        srcPositions[clipIndex].Value,
                                        srcPositions[clipIndex].Value + currentTimeRange,
                                        clips[clipIndex].Notes,
                                        position
                                    )
                                );
                                position += currentTimeRange;
                            }
                            if (options.Mask)
                            {
                                foreach (var srcPosition in srcPositions)
                                {
                                    srcPosition.Inc(currentTimeRange);
                                }
                            }
                            else
                            {
                                srcPositions[clipIndex].Inc(currentTimeRange);
                            }
                            repeatsIndex++;
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
