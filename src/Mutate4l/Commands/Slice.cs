using Mutate4l.Dto;
using Mutate4l.Options;
using Mutate4l.Utility;
using System.Collections.Generic;

namespace Mutate4l.Commands
{
    public class Slice
    {
        public static ProcessResult Apply(SliceOptions options, params Clip[] clips)
        {
            var processedClips = new List<Clip>();
            foreach (var clip in clips)
            {
                processedClips.Add(ClipUtilities.SplitNotesAtEvery(clip, options.Lengths));
            }
            return new ProcessResult(processedClips.ToArray());
        }
    }
}
