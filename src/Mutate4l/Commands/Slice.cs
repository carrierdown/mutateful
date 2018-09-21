using Mutate4l.Core;
using Mutate4l.Dto;
using Mutate4l.Utility;
using System.Collections.Generic;

namespace Mutate4l.Commands
{
    public class SliceOptions
    {
        [OptionInfo(type: OptionType.Default)]
        public decimal[] Lengths { get; set; } = new decimal[] { .25m };
    }

    public class Slice
    {
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
