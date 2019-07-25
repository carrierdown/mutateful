using Mutate4l.Core;
using Mutate4l.Utility;
using System.Linq;
using Mutate4l.Cli;

namespace Mutate4l.Commands
{
    public class ConstrainOptions
    {
        public Clip By { get; set; }

        public bool Strict { get; set; }
        
        // add option to constrain to pitch based on position, so that events occuring for instance during a chord are constrained to this only. If no events are available in the given span, the entire clip is used instead.
    }

    // constrain: first clip timing and/or pitch is replicated on all following clips. Position is optionally scaled with the Strength parameter.
    public static class Constrain
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            var (success, msg) = OptionParser.TryParseOptions(command, out ConstrainOptions options);
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
            var masterClip = clips[0];
            var slaveClips = clips.Skip(1).ToArray();
            var processedClips = slaveClips.Select(c => new Clip(c.Length, c.IsLooping)).ToArray();

            for (var i = 0; i < slaveClips.Length; i++)
            {
                var slaveClip = slaveClips[i];
                foreach (var note in slaveClip.Notes)
                {
                    var constrainedNote = new NoteEvent(note);
                    constrainedNote.Pitch = options.Strict ? 
                        ClipUtilities.FindNearestNotePitchInSet(note, masterClip.Notes) : 
                        ClipUtilities.FindNearestNotePitchInSetMusical(note, masterClip.Notes);
                    processedClips[i].Notes.Add(constrainedNote);
                }
            }
            return new ProcessResultArray<Clip>(processedClips);
        }
    }
}
