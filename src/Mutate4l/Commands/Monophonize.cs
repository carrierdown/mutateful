using Mutate4l.Utility;
using System.Collections.Generic;
using Mutate4l.Core;

namespace Mutate4l.Commands
{
    // # desc: Makes the clip monophonic by removing any overlapping notes. Lower notes have precedence over higher notes.
    public static class Monophonize
    {
        // TODO: Add option to cut overlapping events, so that more of the original clip is preserved
        
        public static ProcessResultArray<Clip> Apply(params Clip[] clips)
        {
            var processedClips = new List<Clip>();
            foreach (var clip in clips)
            {
                processedClips.Add(ClipUtilities.Monophonize(clip));
            }
            return new ProcessResultArray<Clip>(processedClips.ToArray());
        }
    }
}
