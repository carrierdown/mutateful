using Mutate4l.Compiler;
using Mutate4l.Core;
using Mutate4l.Utility;

namespace Mutate4l.Commands
{
    // Scales velocity by value 
    // TODO: An optional second argument enforces an abritrary minumum which we add back in;
    public class VelocityScaleOptions
    {
        [OptionInfo(type: OptionType.Default, 0)]
        public decimal Strength { get; set; } 
    }

    // # desc: Scale a clips notes' velocities.
    public static class VelocityScale
    {
        const int FullMidiVelocityRange = 127;

        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            (var success, var msg) = OptionParser.TryParseOptions(command, out VelocityScaleOptions options);

            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(VelocityScaleOptions options, params Clip[] clips)
        {
            var processedClips = new Clip[clips.Length];         

            var i = 0;

            foreach (var clip in clips)
            {
                var processedClip = new Clip(clip.Length, clip.IsLooping);
  
                processedClip.Notes.AddRange(ClipUtilities.GetSplitNotesInRangeAtPosition(0, clip.Length, clip.Notes, 0));

                foreach (var item in processedClip.Notes)
                {
                    item.Velocity = System.Math.Min(FullMidiVelocityRange, System.Math.Abs((int)System.Math.Floor(item.Velocity * (float)options.Strength)));
                }
                processedClips[i++] = processedClip;
            }
            return new ProcessResultArray<Clip>(processedClips);
        }
    }
}
