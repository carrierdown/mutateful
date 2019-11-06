using Mutate4l.Cli;
using Mutate4l.Core;
using Mutate4l.Utility;

namespace Mutate4l.Commands
{
    // Extracts notes of the current clips and inserts nothing in front, effectively padding them. 
    public class PrePadOptions
    {
        [OptionInfo(type: OptionType.Default, 1/2f)]
        public decimal[] Lengths { get; set; } = { 1 };
    }

    // # desc: Prepads a clip with the desired length.
    public static class PrePad
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            (var success, var msg) = OptionParser.TryParseOptions(command, out PrePadOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(PrePadOptions options, params Clip[] clips)
        {
            var processedClips = new Clip[clips.Length];
           
            var i = 0;

            foreach (var clip in clips)

            {
                var duration = options.Lengths[0] + clip.Length;

                var processedClip = new Clip(duration, clip.IsLooping);

                System.Collections.Generic.List<NoteEvent> Tempnotes = ClipUtilities.GetSplitNotesInRangeAtPosition(0, duration, clip.Notes, 0);

                foreach (var item in Tempnotes)
                {
                    item.Start += options.Lengths[0];
                }

                processedClip.Notes.AddRange(Tempnotes);
                processedClips[i++] = processedClip;
            }
            return new ProcessResultArray<Clip>(processedClips);
        }
    }
}
