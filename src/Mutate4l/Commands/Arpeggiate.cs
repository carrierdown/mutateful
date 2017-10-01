using Mutate4l.Dto;
using Mutate4l.Options;
using Mutate4l.Utility;
using System.Collections.Generic;
using System.Linq;

namespace Mutate4l.Commands
{
    public class Arpeggiate
    {
        public static ProcessResult Apply(ArpeggiateOptions options, params Clip[] clips)
        {
            if (clips.Length < 2)
            {
                clips = new Clip[] { clips[0], clips[0] };
            }
            Clip arpSequence = clips[0];
            Clip[] triggerClips = clips.Skip(1).ToArray();
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
                    var notes = ClipUtilities.GetSplitNotesInRangeAtPosition(0, note.Duration, arpSequence.Notes, note.Start);
                    notes.ForEach(n => n.Pitch += triggerClip.RelativePitch(i));
                    clip.Notes.AddRange(notes);
                }
                processedClips.Add(clip);
            }
            return new ProcessResult(processedClips.ToArray());
        }
    }
}
