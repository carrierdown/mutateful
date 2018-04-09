using Mutate4l.Dto;
using Mutate4l.Options;
using Mutate4l.Utility;
using System.Linq;

namespace Mutate4l.Commands
{
    public class Filter
    {
        public static ProcessResult Apply(FilterOptions options, params Clip[] clips)
        {
            var processedClips = new Clip[clips.Length];

            for (var c = 0; c < clips.Length; c++)
            {
                var clip = clips[c];
                var processedClip = new Clip(clip.Length, clip.IsLooping);

                processedClip.Notes = clip.Notes.Where(x => x.Duration > options.Duration).ToSortedList();

                processedClips[c] = processedClip;
            }

            return new ProcessResult(processedClips);
        }
    }
}
