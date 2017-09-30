using Mutate4l.Dto;
using Mutate4l.Options;
using Mutate4l.Utility;
using System.Linq;

namespace Mutate4l.Commands
{

    // constrain: first clip timing and/or pitch is replicated on all following clips. Position is optionally scaled with the Strength parameter.
    public class Constrain
    {
        public static ProcessResult Apply(ConstrainOptions options, params Clip[] clips)
        {
            Clip masterClip = clips[0];
            Clip[] slaveClips = clips.Skip(1).ToArray();
            ClipUtilities.NormalizeClipLengths(clips);
            Clip[] processedClips = slaveClips.Select(c => new Clip(c.Length, c.IsLooping)).ToArray();

            for (var i = 0; i < slaveClips.Length; i++)
            {
                var slaveClip = slaveClips[i];
                foreach (var note in slaveClip.Notes)
                {
                    var constrainedNote = new Note(note);
                    if (options.Pitch)
                    {
                        constrainedNote.Pitch = ClipUtilities.FindNearestNotePitchInSet(note, masterClip.Notes);
                    }
                    if (options.Start)
                    {
                        var newStart = ClipUtilities.FindNearestNoteStartInSet(note, masterClip.Notes);
                        constrainedNote.Start += (newStart - constrainedNote.Start) * (options.Strength / 100);
                    }
                    slaveClip.Notes.Add(constrainedNote);
                }
            }
            return new ProcessResult(processedClips);
        }
    }
}
