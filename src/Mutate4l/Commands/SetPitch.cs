using System.Linq;
using Mutate4l.Cli;
using Mutate4l.Core;
using Mutate4l.Utility;

namespace Mutate4l.Commands
{
    public class SetPitchOptions
    {
        [OptionInfo(OptionType.Default, 0, 127)]
        public int[] PitchValues { get; set; } = new int[0];
        
        public Clip By { get; set; } = Clip.Empty;
    }
    
    // Simple command to set pitch of all notes to specified value(s). If more values are specified, they are cycled through.
    public class SetPitch
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            var (success, msg) = OptionParser.TryParseOptions(command, out SetPitchOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }
        
        public static ProcessResultArray<Clip> Apply(SetPitchOptions options, params Clip[] clips)
        {
            var resultClips = ClipUtilities.CreateEmptyPlaceholderClips(clips);

            int[] pitches;
            if (options.PitchValues.Length > 0)
            {
                pitches = options.PitchValues;
            } else
            {
                pitches = options.By.Notes.Select(x => x.Pitch).ToArray();
            }
            if (pitches.Length == 0) return new ProcessResultArray<Clip>(clips, "SetPitch did nothing, since neither pitches or -by clip was specified.");

            for (var i = 0; i < clips.Length; i++)
            {
                var clip = clips[i];
                var resultClip = resultClips[i];
                var pitchIx = 0;
                foreach (var note in clip.Notes)
                {
                    var repitchedNote = new NoteEvent(note)
                    {
                        Pitch = pitches[pitchIx++ % pitches.Length]
                    };
                    ClipUtilities.AddNoteCutting(resultClip, repitchedNote);
                }
            }

            return new ProcessResultArray<Clip>(resultClips);
        }
    }
}