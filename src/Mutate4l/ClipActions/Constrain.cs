using Mutate4l.Dto;
using System.Linq;
using Mutate4l.Core;

namespace Mutate4l.ClipActions
{
    public class ConstrainOptions
    {
        [OptionInfo(type: OptionType.AllOrSpecified)]
        public bool Pitch { get; set; }

        [OptionInfo(type: OptionType.AllOrSpecified)]
        public bool Start { get; set; }

        [OptionInfo(min: 1, max: 100)]
        public int Strength { get; set; } = 100;

        // todo: possibly have option for whether Constrain should output only processed clips or if source clip should also be included
        // todo: option for whether Strength should also affect Pitch?
    }

    // constrain: first clip timing and/or pitch is replicated on all following clips. Position is optionally scaled with the Strength parameter.
    public class Constrain
    {
        public static ProcessResult Apply(ConstrainOptions options, params Clip[] clips)
        {
            if (clips.Length < 2)
            {
                return new ProcessResult("Error: Less than two clips were specified.");
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
