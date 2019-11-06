using Mutate4l.Cli;
using Mutate4l.Core;
using Mutate4l.Utility;

namespace Mutate4l.Commands
{
    // Extracts notes of the current clips and inserts nothing in front, effectively padding them. 
    // An optional second argument enforces an abritrary total duration;
    public class PaddingOptions
    {
        [OptionInfo(type: OptionType.Default, 1/2f)]
        public decimal[] Lengths { get; set; } = { 2 };
    }

    // # desc: Prepads a clip with the desired length. Optionally with an abritrary total duration.
    public static class Padding
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            (var success, var msg) = OptionParser.TryParseOptions(command, out PaddingOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(PaddingOptions options, params Clip[] clips)
        {
            var processedClips = new Clip[clips.Length];         
            var i = 0;
            foreach (var clip in clips)
            {
                var duration = options.Lengths.Length > 1 ? options.Lengths[1] : options.Lengths[0] + clip.Length;
                var processedClip = new Clip(duration, clip.IsLooping);
  
                processedClip.Notes.AddRange(ClipUtilities.GetSplitNotesInRangeAtPosition(0, duration, clip.Notes, 0));

                foreach (var item in processedClip.Notes)
                {
                    item.Start += options.Lengths[0];
                }
                processedClips[i++] = processedClip;
            }
            return new ProcessResultArray<Clip>(processedClips);
        }
    }
}
