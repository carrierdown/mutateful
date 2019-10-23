using System.Collections.Generic;
using Mutate4l.Cli;
using Mutate4l.Core;
using Mutate4l.Utility;

namespace Mutate4l.Commands
{
    public class SliceOptions
    {
        [OptionInfo(type: OptionType.Default, 1/128f)]
        public decimal[] Lengths { get; set; } = { 1/16m };
    }

    // # desc: Slices a clip (i.e. cutting any notes) at a regular or irregular set of fractions.
    public static class Slice
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            var (success, msg) = OptionParser.TryParseOptions(command, out SliceOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(SliceOptions options, params Clip[] clips)
        {
            var processedClips = new List<Clip>();
            foreach (var clip in clips)
            {
                processedClips.Add(ClipUtilities.SplitNotesAtEvery(clip, options.Lengths));
            }
            return new ProcessResultArray<Clip>(processedClips.ToArray());
        }
    }
}
