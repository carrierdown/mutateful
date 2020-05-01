using System.Linq;
using Mutate4l.Cli;
using Mutate4l.Core;

namespace Mutate4l.Commands
{
    public class InvertOptions
    {
        [OptionInfo(type: OptionType.Default, 1/512f, noImplicitCast: true)]
        public decimal/*ActualDecimal*/ Factor { get; set; } = 1.0m;
    }

    // # desc: Resizes the current clip based on the specified factor (i.e. 0.5 halves the size of the clip, effectively doubling its tempo)
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
                DoInvert(clip);
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