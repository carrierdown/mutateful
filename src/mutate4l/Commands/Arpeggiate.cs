using Mutate4l.Dto;
using Mutate4l.Options;
using Mutate4l.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mutate4l.Commands
{
    public class Arpeggiate
    {
        // Add option to dynamically set # of events that should be rescaled to another note, probably via velocity.
        public static ProcessResultArray<Clip> Apply(ArpeggiateOptions options, params Clip[] clips)
        {
            if (clips.Length < 2)
            {
                clips = new Clip[] { clips[0], clips[0] };
            }
            Clip arpSequence = clips[0];
            Clip[] triggerClips = clips.Skip(1).ToArray();
            foreach (var triggerClip in triggerClips)
            {
                ClipUtilities.Monophonize(triggerClip);
            }
            ClipUtilities.Monophonize(arpSequence);
            
            var processedClips = new List<Clip>(triggerClips.Length);

            if (arpSequence.Notes[0].Start != 0 && options.RemoveOffset)
            {
                foreach (var arpNote in arpSequence.Notes)
                {
                    arpNote.Start -= arpSequence.Notes[0].Start;
                }
            }
            
            foreach (var triggerClip in triggerClips)
            {
                Clip clip = new Clip(triggerClip.Length, triggerClip.IsLooping);
                for (var i = 0; i < triggerClip.Notes.Count; i++)
                {
                    var note = triggerClip.Notes[i];
                    //var notes = ClipUtilities.GetSplitNotesInRangeAtPosition(0, note.Duration, arpSequence.Notes, note.Start);
                    var count = Math.Min(arpSequence.Notes.Count, options.Rescale);
                    var duration = arpSequence.Notes[count - 1].Start + arpSequence.Notes[count - 1].Duration;
                    var arpedNotes = new List<NoteEvent>(count);
                    for (var srcI = 0; srcI < count; srcI++)
                    {
                        var arpNote = arpSequence.Notes[srcI];
                        var arpedNote = new NoteEvent(arpNote);
                        arpedNote.Start = note.Start + ((arpedNote.Start / duration) * note.Duration);
                        arpedNote.Duration = (arpedNote.Duration / duration) * note.Duration;
                        arpedNote.Pitch = note.Pitch + arpSequence.RelativePitch(srcI);
                        arpedNotes.Add(arpedNote);
                    }
/*                    var arpNotes = arpSequence.Notes.Take(options.Rescale);
                    var arpi = 0;
                    foreach (var currentArpNote in arpNotes) // todo: test with chords as arpseq
                    {
                        currentArpNote.Pitch = note.Pitch + arpSequence.RelativePitch(arpi);
                        arpi++;
                    }*/
                    clip.Notes.AddRange(arpedNotes);
                }
                processedClips.Add(clip);
            }
            return new ProcessResultArray<Clip>(processedClips.ToArray());
        }
    }
}
