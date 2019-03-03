using Mutate4l.Core;
using Mutate4l.Dto;
using Mutate4l.Utility;
using System.Collections.Generic;
using System.Linq;
using static Mutate4l.Commands.InterleaveMode;

namespace Mutate4l.Commands
{
    public class InterleaveOptions
    {
        public bool ChunkChords { get; set; } = true; // Process notes having the exact same start times as a single event. Only applies to event mode.
        public int[] EnableMask { get; set; } = new int[] { 1 }; // Deprecated. Allows specifying a sequence of numbers to use as a mask for whether the note should be included or omitted. E.g. 1 0 will alternately play and omit every even/odd note. Useful when combining two or more clips but you want to retain only the notes for the current track. In this scenario you would have several formulas that are the same except having different masks.
        public bool Solo { get; set; } = false; // A quicker and more convenient way to control what enablemask is usually made to do - soloing the events from the current channel only.
        public InterleaveMode Mode { get; set; } = Time;
        public decimal[] Ranges { get; set; } = new decimal[] { 1 };
        public int[] Repeats { get; set; } = new int[] { 1 };
        public bool Skip { get; set; } = false; // Whether to skip events at the corresponding location in other clips
        // todo: public decimal[] ScaleFactors { get; set; } = new decimal[] { 1 }; // Scaling is done after slicing, but prior to interleaving
    }

    public enum InterleaveMode
    {
        Event,
        Time
    }

    public class Interleave
    {
        public static ProcessResultArray<Clip> Apply(InterleaveOptions options, ClipMetaData metadata, params Clip[] clips)
        {
            if (clips.Length < 2)
            {
                clips = new Clip[] { clips[0], clips[0] };
            }
            decimal position = 0;
            int repeatsIndex = 0;
            Clip resultClip = new Clip(4, true);

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
                            var currentNoteCounter = noteCounters[clipIndex];

                            for (var repeats = 0; repeats < options.Repeats[repeatsIndex % options.Repeats.Length]; repeats++)
                            {
                                var note = clip.Notes[currentNoteCounter.Value];

                                if (!options.Solo || clip.ClipReference.Track == metadata.TrackNumber) resultClip.Notes.Add(new NoteEvent(note.Pitch, position, note.Duration, note.Velocity));
                                //if (options.EnableMask[clipIndex % options.EnableMask.Length] == 1) resultClip.Notes.Add(new NoteEvent(note.Pitch, position, note.Duration, note.Velocity));
                                if (options.ChunkChords && clip.Notes.Any(x => x.Start == note.Start && x.Pitch != note.Pitch))
                                {
                                    // Note: This works ok-ish, but chunking will not be 100% unless proper chunking is implemented at the clip level, since Skip for instance will not behave correctly in all situations unless chords have been chunked prior to processing
                                    var chordNotes = clip.Notes.Where(x => x.Start == note.Start && x.Pitch != note.Pitch).Select(x => new NoteEvent(x.Pitch, position, x.Duration, x.Velocity));
                                    if (!options.Solo || clip.ClipReference.Track == metadata.TrackNumber) resultClip.Notes.AddRange(chordNotes);
                                    //if (options.EnableMask[clipIndex % options.EnableMask.Length] == 1) resultClip.Notes.AddRange(chordNotes);
                                    currentNoteCounter.Inc(chordNotes.Count());
                                }
                                position += clip.DurationUntilNextNote(currentNoteCounter.Value);
//                                position += clip.Notesclip.SilentDurationUntilNextNote(currentNoteCounter.Value);
                            }
                            if (options.Skip)
                                foreach (var noteCounter in noteCounters) noteCounter.Inc();
                            else
                                currentNoteCounter.Inc();
                            repeatsIndex++;
                        }
                    }
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
                                if (!options.Solo || clip.ClipReference.Track == metadata.TrackNumber)
                                {
                                    resultClip.Notes.AddRange(
                                        ClipUtilities.GetSplitNotesInRangeAtPosition(
                                            srcPositions[clipIndex].Value,
                                            srcPositions[clipIndex].Value + currentTimeRange,
                                            clips[clipIndex].Notes,
                                            position
                                        )
                                    );
                                }
                                position += currentTimeRange;
                            }
                            if (options.Skip)
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
                    break;
            }
            resultClip.Length = position;
            return new ProcessResultArray<Clip>(new Clip[] { resultClip });
        }
    }
}
