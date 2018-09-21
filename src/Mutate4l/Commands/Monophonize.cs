using Mutate4l.Dto;
using Mutate4l.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l.Commands
{
    public class Monophonize
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
