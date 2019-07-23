using System;
using System.Linq;
using Mutate4l.Cli;
using Mutate4l.Core;
using Mutate4l.Utility;

namespace Mutate4l.Commands
{
    public class SetRhythmOptions
    {
        public Clip By { get; set; }
    }
    
    public static class SetRhythm
    {
        // Combines two clips by taking the timing and durations from one clip and the pitch/velocity from another
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            var (success, msg) = OptionParser.TryParseOptions(command, out SetRhythmOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(SetRhythmOptions options, params Clip[] clips)
        {
            if (options.By != null)
            {
                clips = clips.Prepend(options.By).ToArray();
            }

            if (clips.Length < 2)
            {
                return new ProcessResultArray<Clip>(clips, $"SetRhythm: Skipped command because it needs 2 clips, and {clips.Length} were passed in.");
            }
            
            var resultClips = new Clip[clips.Length - 1];
            var byClip = clips[0];
            var byIndex = 0;
            var resultClipIx = 0;

            for (var i = 1; i < clips.Length; i++)
            {
                var clip = clips[i];
                var resultClip = new Clip(0, clip.IsLooping);

                foreach (var note in clip.Notes)
                {
                    var byNote = byClip.Notes[byIndex % byClip.Count];

                    // special case: add silence between start of clip and first note, but only the first time, since subsequent silences are handled by DurationUntilNextNote
                    if (resultClip.Length == 0 && byIndex == 0)
                    {
                        resultClip.Length = byNote.Start;
                    }
                    
                    resultClip.Add(new NoteEvent(note.Pitch, resultClip.Length, byNote.Duration, note.Velocity));
                    resultClip.Length += byClip.DurationUntilNextNote(byIndex % byClip.Count);
                    byIndex++;
                }
                
                // stacked/overlapping notes will lead to incorrect final length of clip, so check if this is the case
                var latestNoteEnd = resultClip.Notes.Max(x => x.End);
                if (latestNoteEnd > resultClip.Length)
                {
                    resultClip.Length = latestNoteEnd;
                }
                
                resultClip.Length = Utilities.RoundUpToNearestSixteenth(resultClip.Length); // quantize clip length to nearest 1/16, or Live won't accept it
                resultClips[resultClipIx++] = resultClip;
            }

            return new ProcessResultArray<Clip>(resultClips);
        }
    }
}