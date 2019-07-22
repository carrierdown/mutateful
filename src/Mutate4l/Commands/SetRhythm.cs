using System.Linq;
using Mutate4l.Cli;
using Mutate4l.Core;

namespace Mutate4l.Commands
{
    public class SetRhythmOptions
    {
        [OptionInfo(0, 127, type:OptionType.Default)]
        public int[] Pitches { get; set; }
        
        public Clip By { get; set; }
    }
    
    public static class SetRhythm
    {
        // Combines two clips by taking the timing and durations from one clip and the pitch/velocity from another
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            var (success, msg) = OptionParser.TryParseOptions(command, out SetRhythmOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(SetRhythmOptions options, params Clip[] clips)
        {
            if (options.By != null)
            {
                clips = clips.Prepend(options.By).ToArray();
            }

            if (clips.Length < 2)
            {
                return new ProcessResultArray<Clip>(clips, $"SetRhythm: Skipped command because it needs 2 clips, and {clips.Length} were passed in.");
            }
            
            var resultClips = new Clip[clips.Length - 1];
            var byClip = clips[0];
            var byIndex = 0;
            var resultClipIx = 0;

            for (var i = 1; i < clips.Length; i++)
            {
                var clip = clips[i];
                var resultClip = new Clip(clip.Length, clip.IsLooping);
                decimal offset = 0;

                foreach (var note in clip.Notes)
                {
                    var byNote = byClip.Notes[byIndex % byClip.Count];
                    offset = (byIndex / byClip.Count) * byClip.Length;
                    resultClip.Add(new NoteEvent(note.Pitch, offset + byNote.Start, byNote.Duration, note.Velocity));
                    byIndex++;
                }
                resultClip.Length += offset; 
                resultClips[resultClipIx++] = resultClip;
            }

            return new ProcessResultArray<Clip>(resultClips);
        }
    }
}