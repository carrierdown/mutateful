using Mutate4l.Cli;
using Mutate4l.Core;

namespace Mutate4l.Commands
{
    public class SetLengthOptions
    {
        [OptionInfo(type: OptionType.Default)]
        public decimal[] Lengths { get; set; } = {0.25m};
    }
    
    // Simple command to set length of all notes to specified value(s). If more values are specified, they are cycled through.
    public static class SetLength
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            var (success, msg) = OptionParser.TryParseOptions(command, out SetLengthOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(SetLengthOptions options, params Clip[] clips)
        {
            var resultClips = new Clip[clips.Length];

            var i = 0;
            foreach (var clip in clips)
            {
                var resultClip = new Clip(clips[i].Count, clips[i].IsLooping);
                var lengthCounter = 0;
                foreach (var note in clip.Notes)
                {
                    resultClip.Add(new NoteEvent(note.Pitch, note.Start, options.Lengths[lengthCounter++ % options.Lengths.Length], note.Velocity));
                }

                resultClips[i] = resultClip;
                i++;
            }
            return new ProcessResultArray<Clip>(resultClips);
        }
    }
}