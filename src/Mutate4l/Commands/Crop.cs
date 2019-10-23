using Mutate4l.Cli;
using Mutate4l.Core;
using Mutate4l.Utility;

namespace Mutate4l.Commands
{
    // Extracts a region of the current clips, effectively cropping them. If two params, start - duration is used, otherwise 0 - duration.
    public class CropOptions
    {
        [OptionInfo(type: OptionType.Default, 1/32f)]
        public decimal[] Lengths { get; set; } = { 2 };
    }

    // # desc: Crops a clip to the desired length, or within the desired region.
    public static class Crop
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            (var success, var msg) = OptionParser.TryParseOptions(command, out CropOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(CropOptions options, params Clip[] clips)
        {
            var processedClips = new Clip[clips.Length];
            var start = options.Lengths.Length > 1 ? options.Lengths[0] : 0;
            var duration = options.Lengths.Length > 1 ? options.Lengths[1] : options.Lengths[0];
            var i = 0;

            foreach (var clip in clips)
            {
                var processedClip = new Clip(duration, clip.IsLooping);
                processedClip.Notes.AddRange(ClipUtilities.GetSplitNotesInRangeAtPosition(start, start + duration, clip.Notes, 0));
                processedClips[i++] = processedClip;
            }
            return new ProcessResultArray<Clip>(processedClips);
        }
    }
}
