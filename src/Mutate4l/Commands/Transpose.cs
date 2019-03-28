using Mutate4l.Dto;
using Mutate4l.Utility;

namespace Mutate4l.Commands
{
    public enum TransposeMode
    {
        Absolute, // pitch is transposed relative to an absolute base pitch of 60 (C4)
        Relative, // pitch is transposed relative to the first note in the transposing clip
        Overwrite // pitch is set to the notes in the transposing clip
    }

    public class TransposeOptions
    {
        public Clip By { get; set; } // Allows syntax like a1 transpose -by a2 -mode relative. This syntax makes it much clearer which clip is being affected, and which is used as the source.
        public TransposeMode Mode { get; set; } = TransposeMode.Relative;
    }

    // also needed: a transpose function (rangetranspose?) transposing all notes contained within the bounds of the respective note in the control clip
    public class Transpose
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            (var success, var msg) = OptionParser.TryParseOptions(command, out TransposeOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(TransposeOptions options, params Clip[] clips)
        {
            int basePitch = 60; // absolute basePitch should maybe be relative to c in whatever octave first note is in
            if (options.By == null)
            {
                return new ProcessResultArray<Clip>("No -by clip specified");
            }
            ClipUtilities.Monophonize(options.By);
            if (options.Mode == TransposeMode.Relative && options.By.Notes.Count > 0)
            {
                basePitch = options.By.Notes[0].Pitch;
            }

            foreach (var clip in clips)
            {
                clip.GroupSimultaneousNotes();
                for (var i = 0; i < clip.Count; i++)
                {
                    var noteEvent = clip.Notes[i];
                    var transposeNoteEvent = options.By.Notes[i % options.By.Notes.Count];
                    if (options.Mode == TransposeMode.Overwrite)
                    {
                        noteEvent.Pitch = transposeNoteEvent.Pitch;
                    } else
                    {
                        noteEvent.Pitch += transposeNoteEvent.Pitch - basePitch;
                    }
                }
                clip.Flatten();
            }
            return new ProcessResultArray<Clip>(clips); // currently destructive
        }
    }
}
