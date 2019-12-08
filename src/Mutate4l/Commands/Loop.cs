using Mutate4l.Cli;
using Mutate4l.Core;
using Mutate4l.Utility;

namespace Mutate4l.Commands
{
    public class LoopOptions
    {
        [OptionInfo(OptionType.Default, 1, noImplicitCast: true)]
        public decimal/*ActualDecimal*/ Length { get; set; } = 1;
    }

    // # desc: Lengthens the incoming clips according to the factor specified (e.g. 2 would double the clip length)
    public static class Loop
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            var (success, msg) = OptionParser.TryParseOptions(command, out LoopOptions options);
            return success ? Apply(options, clips) : new ProcessResultArray<Clip>(msg);
        }

        public static ProcessResultArray<Clip> Apply(LoopOptions options, params Clip[] clips)
        {
            foreach (var clip in clips)
            {
                ClipUtilities.EnlargeClipByLooping(clip, options.Length * clip.Length);
            }
            return new ProcessResultArray<Clip>(clips);
        }
    }
}