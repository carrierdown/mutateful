using Mutate4l.Cli;
using Mutate4l.Core;

namespace Mutate4l.Commands
{
    public class RelengthOptions
    {
        [OptionInfo(type: OptionType.Default)]
        public decimal Factor { get; set; } = 1.0m;
    }

    public class Relength
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            (var success, var msg) = OptionParser.TryParseOptions(command, out RelengthOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(RelengthOptions options, params Clip[] clips)
        {
            foreach (var clip in clips)
            {
                foreach (var note in clip.Notes)
                {
                    note.Duration *= options.Factor;
                }
            }

            return new ProcessResultArray<Clip>(clips);
        }
    }
}
