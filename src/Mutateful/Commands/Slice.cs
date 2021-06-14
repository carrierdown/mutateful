using System.Collections.Generic;
using Mutateful.Compiler;
using Mutateful.Core;
using Mutateful.Utility;

namespace Mutateful.Commands
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
