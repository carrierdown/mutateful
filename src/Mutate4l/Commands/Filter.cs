using Mutate4l.Core;
using Mutate4l.Utility;
using System.Linq;
using Mutate4l.Cli;

namespace Mutate4l.Commands
{
    public class FilterOptions
    {
        [OptionInfo(OptionType.Default)]
        public decimal Duration { get; set; } = 1 / 64m;

        public bool Invert { get; set; }
    }

    public static class Filter
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            (var success, var msg) = OptionParser.TryParseOptions(command, out FilterOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }


        public static ProcessResultArray<Clip> Apply(FilterOptions options, params Clip[] clips)
        {
            var processedClips = new Clip[clips.Length];

            for (var c = 0; c < clips.Length; c++)
            {
                var clip = clips[c];
                var processedClip = new Clip(clip.Length, clip.IsLooping);

                processedClip.Notes = (options.Invert) ?

                    clip.Notes.Where(x => x.Duration < options.Duration).ToSortedList() :

                    clip.Notes.Where(x => x.Duration > options.Duration).ToSortedList();

                processedClips[c] = processedClip;
            }

            return new ProcessResultArray<Clip>(processedClips);
        }
    }
}
