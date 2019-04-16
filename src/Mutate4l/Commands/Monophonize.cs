using Mutate4l.Utility;
using System.Collections.Generic;
using Mutate4l.Core;

namespace Mutate4l.Commands
{
    public static class Monophonize
    {
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
