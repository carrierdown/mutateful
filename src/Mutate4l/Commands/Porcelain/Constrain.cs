using Mutate4l.Dto;
using Mutate4l.Options;
using Mutate4l.Utility;
using System;
using System.Linq;

namespace Mutate4l.Commands.Porcelain
{

    // constrain: first clip timing and/or pitch is replicated on all following clips. Position is optionally scaled with the Strength parameter.
    public class Constrain
    {
        public static ProcessResult Apply(ConstrainOptions options, params Clip[] clips)
        {
            if (clips.Length < 2)
            {
                clips = new Clip[] { clips[0], clips[0] };
            }
            Clip masterClip = clips[0];
            Clip[] slaveClips = clips.Skip(1).ToArray();
            ClipUtilities.NormalizeClipLengths(clips);
            Clip[] processedClips = slaveClips.Select(c => new Clip(c.Length, c.IsLooping)).ToArray();

            for (var i = 0; i < slaveClips.Length; i++)
            {
                var slaveClip = slaveClips[i];
                foreach (var note in slaveClip.Notes)
                {
                    var constrainedNote = new NoteEvent(note);
                    if (options.Pitch)
                    {
                        var absPitch = ClipUtilities.FindNearestNotePitchInSet(note, masterClip.Notes) % 12;
                        constrainedNote.Pitch = (((constrainedNote.Pitch / 12)) * 12) + (absPitch == 0 ? 12 : 0) + absPitch;
                    }
                    if (options.Start)
                    {
                        var newStart = ClipUtilities.FindNearestNoteStartInSet(note, masterClip.Notes);
                        constrainedNote.Start += (newStart - constrainedNote.Start) * (options.Strength / 100);
                    }
                    processedClips[i].Notes.Add(constrainedNote);
                }
            }
            return new ProcessResult(processedClips);
        }
    }
}
