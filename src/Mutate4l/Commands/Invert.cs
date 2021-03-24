using System.Linq;
using Mutate4l.Compiler;
using Mutate4l.Core;

namespace Mutate4l.Commands
{
    public class InvertOptions
    {
        [OptionInfo(type: OptionType.Default, 0)]
        public int Position { get; set; } = 1;
    }

    // # desc: Inverts the contents of the current clip the specified number of times. For each inversion, all notes with the currently lowest pitch value are moved one octave up.
    public static class Invert
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            var (success, msg) = OptionParser.TryParseOptions(command, out InvertOptions options);
            return !success ? new ProcessResultArray<Clip>(msg) : Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(InvertOptions options, params Clip[] clips)
        {
            foreach (var clip in clips)
            {
                for (var i = 0; i < options.Position; i++)
                {
                    DoInvert(clip);
                }
            }

            return new ProcessResultArray<Clip>(clips);
        }

        public static void DoInvert(Clip clip)
        {
            var lowestPitch = clip.Notes.Min(x => x.Pitch);
            var notesToInvert = clip.Notes.Where(x => x.Pitch == lowestPitch);
            foreach (var note in notesToInvert)
            {
                note.Pitch += 12;
            }
        }
    }
}