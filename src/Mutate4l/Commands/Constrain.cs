using Mutate4l.Core;
using Mutate4l.Dto;
using Mutate4l.Utility;
using System.Linq;
using static Mutate4l.Commands.ConstrainMode;

namespace Mutate4l.Commands
{
    public enum ConstrainMode
    {
        Pitch,
        Rhythm,
        Both
    }

    public class ConstrainOptions
    {
        public ConstrainMode Mode { get; set; } = Pitch;

        [OptionInfo(min: 1, max: 100)]
        public int Strength { get; set; } = 100;

        public Clip By { get; set; }

        public bool Strict { get; set; }
    }

    // constrain: first clip timing and/or pitch is replicated on all following clips. Position is optionally scaled with the Strength parameter.
    public class Constrain
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            (var success, var msg) = OptionParser.TryParseOptions(command, out ConstrainOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(ConstrainOptions options, params Clip[] clips)
        {
            if (options.By != null)
            {
                clips = clips.Prepend(options.By).ToArray();
            }
            ClipUtilities.NormalizeClipLengths(clips);
            if (clips.Length < 2) return new ProcessResultArray<Clip>(clips);
            Clip masterClip = clips[0];
            Clip[] slaveClips = clips.Skip(1).ToArray();
            Clip[] processedClips = slaveClips.Select(c => new Clip(c.Length, c.IsLooping)).ToArray();

            for (var i = 0; i < slaveClips.Length; i++)
            {
                var slaveClip = slaveClips[i];
                foreach (var note in slaveClip.Notes)
                {
                    var constrainedNote = new NoteEvent(note);
                    if (options.Mode == Pitch || options.Mode == Both)
                    {
                        if (options.Strict)
                            constrainedNote.Pitch = ClipUtilities.FindNearestNotePitchInSet(note, masterClip.Notes);
                        else
                            constrainedNote.Pitch = ClipUtilities.FindNearestNotePitchInSetMusical(note, masterClip.Notes);
                    }
                    if (options.Mode == Rhythm || options.Mode == Both)
                    {
                        var newStart = ClipUtilities.FindNearestNoteStartInSet(note, masterClip.Notes);
                        constrainedNote.Start += (newStart - constrainedNote.Start) * (options.Strength / 100);
                    }
                    processedClips[i].Notes.Add(constrainedNote);
                }
            }
            return new ProcessResultArray<Clip>(processedClips);
        }
    }
}
