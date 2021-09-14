using System.Linq;
using Mutateful.Compiler;
using Mutateful.Core;

namespace Mutateful.Commands
{
    public class TakeOptions
    {
        [OptionInfo(type: OptionType.Default, 1)] public int[] TakeCounts { get; set; } = { 2 };
        
        public bool Thin { get; set; } // includes silence between skipped notes as well, effectively "thinning out" the clip 
        
        [OptionInfo(0, 127)] public int LowPitch { get; set; } = 0;

        [OptionInfo(0, 127)] public int HighPitch { get; set; } = 127;

        [OptionInfo(0, 127)] public int LowVelocity { get; set; } = 0;

        [OptionInfo(0, 127)] public int HighVelocity { get; set; } = 127;
    }
    
    // # desc: Creates a new clip by taking every # note from another clip. If more than one skip value is specified, they are cycled through.
    public static class Take
    {
        public static ProcessResultArray<Clip> Apply(Command command, Clip[] clips, bool doExtract = false)
        {
            var (success, msg) = OptionParser.TryParseOptions(command, out TakeOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            if (doExtract)
            {
                options.Thin = true;
                options.TakeCounts = new[] {1};
            }
            return Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(TakeOptions options, params Clip[] clips)
        {
            var resultClips = new Clip[clips.Length];

            // Normalize take values (typical input range: 1 - N, while 0 - N is used internally)
            for (var ix = 0; ix < options.TakeCounts.Length; ix++)
            {
                options.TakeCounts[ix]--;
            }

            var (lowVelocity, highVelocity) = (options.LowVelocity, options.HighVelocity);
            if (lowVelocity > highVelocity) (lowVelocity, highVelocity) = (highVelocity, lowVelocity);
            var (lowPitch, highPitch) = (options.LowPitch, options.HighPitch);
            if (lowPitch > highPitch) (lowPitch, highPitch) = (highPitch, lowPitch);
            
            var i = 0;
            foreach (var clip in clips)
            {
                var filteredNotes = clip.Notes.Where(x =>
                        x.Velocity >= lowVelocity && x.Velocity <= highVelocity && x.Pitch >= lowPitch && x.Pitch <= highPitch).ToList();
                
                var resultClip = new Clip(clips[i].Length, clips[i].IsLooping);
                decimal currentPos = 0;
                var noteIx = 0;
                var currentTake = options.TakeCounts[0];
                var takeIx = 0;
                // We want to keep the length of the newly created clip approximately equal to the original, therefore we keep
                // going until we have filled at least the same length as the original clip
                while (currentPos < resultClip.Length)
                {
                    if (currentTake == 0)
                    {
                        if (noteIx >= clip.Count) noteIx %= clip.Count;
                        var note = new NoteEvent(filteredNotes[noteIx]) {Start = currentPos};
                        currentPos += clip.DurationUntilNextNote(noteIx);
                        resultClip.Add(note);
                        currentTake = options.TakeCounts[++takeIx % options.TakeCounts.Length];
                    }
                    else
                    {
                        if (options.Thin) currentPos += clip.DurationUntilNextNote(noteIx);
                        currentTake--;
                    }
                    noteIx++;
                }
                resultClips[i] = resultClip;
                i++;
            }
            return new ProcessResultArray<Clip>(resultClips);
        }
    }
}