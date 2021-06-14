using System.Collections.Generic;
using Mutateful.Compiler;
using Mutateful.Core;
using Mutateful.Utility;

namespace Mutateful.Commands
{
    public class MaskOptions
    {
        public Clip By { get; set; } // If specified, clips passed in are masked (i.e. portions of notes removed) by this clip.
                                     // Otherwise, clips passed in are masked against a dummy clip containing a note as long as
                                     // the clip, effectively inversing the passed in clip.
    }
    
    // # desc: Creates a masking clip which is used to remove or shorten notes not overlapping with the mask clip. If no -by clip is specified, a sustained note is used instead, effectively inversing the clip rhythmically.  
    public static class Mask
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            var (success, msg) = OptionParser.TryParseOptions(command, out MaskOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }

            return Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(MaskOptions options, params Clip[] clips)
        {
            var processedClips = new List<Clip>(clips.Length);
            var byClip = options.By != null && options.By.Count > 0;

            if (byClip)
            {
                foreach (var clip in clips)
                {
                    MaskNotesByClip(clip, options.By);
                    processedClips.Add(clip);
                }
            }
            else
            {
                foreach (var clip in clips)
                {
                    var clipToMask = new Clip(clip.Length, clip.IsLooping);
                    clipToMask.Add(new NoteEvent(60, 0, clipToMask.Length, 100));
                    MaskNotesByClip(clipToMask, clip);
                    processedClips.Add(clipToMask);
                }
            }

            return new ProcessResultArray<Clip>(processedClips.ToArray());
        }

        private static void MaskNotesByClip(Clip clipToMask, Clip maskClip)
        {
//            var smallestGap = 1 / 256m;

            var maskClipIx = 0;
            while (maskClipIx < maskClip.Notes.Count)
            {
                var maskNote = maskClip.Notes[maskClipIx];
                int i = 0;
                while (i < clipToMask.Notes.Count)
                {
                    var note = clipToMask.Notes[i];
                    var clonedNote = new NoteEvent(note);
                    if (maskNote.CrossesStartOfIntervalInclusive(clonedNote.Start, clonedNote.End))
                    {
                        note.Duration = maskNote.End - note.Start;
                        note.Start = maskNote.End;
                    }
                    else if (maskNote.CrossesEndOfIntervalInclusive(clonedNote.Start, clonedNote.End))
                    {
                        note.Duration -= note.End - maskNote.Start;
                    }
                    else if (maskNote.InsideIntervalInclusive(clonedNote.Start, clonedNote.End))
                    {
                        note.Duration = maskNote.Start - note.Start;
                        note.Pitch = maskNote.Pitch;
                        if (clonedNote.End > maskNote.End)
                        {
                            clipToMask.Notes.Add(new NoteEvent(note.Pitch, maskNote.End, clonedNote.End - maskNote.End, note.Velocity));
                        }
                    }

                    i++;
                }
                maskClipIx++;
            }
        }
    }
}