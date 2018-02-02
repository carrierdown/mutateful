using Mutate4l.Dto;
using Mutate4l.Options;
using System.Collections.Generic;

namespace Mutate4l.Commands.Porcelain
{
    public class Ratchet
    {
        // Needs control sequence and target sequence
        // If only one clip is specified, clip will be both control and target sequence
        // When shaping the ratchet, velocity might be another variable used by the control sequence to control extent of shaping
        public static ProcessResult Apply(RatchetOptions options, params Clip[] clips)
        {
            if (clips.Length < 2)
            {
                clips = new Clip[] { clips[0], clips[0] };
            }

            Clip controlSequence = new Clip(clips[0]);
            Clip targetSequence = new Clip(clips[1]);
            Clip resultSequence = new Clip(targetSequence.Length, targetSequence.IsLooping);
            Dictionary<int, bool> processedIndexes = new Dictionary<int, bool>(targetSequence.Notes.Count);

            // find highest and lowest pitch value in control seq

            // set pitch for each targetNote = (val - minVal) / (maxVal - minVal) * (max - min), corresponding to actual ratchet-value for this event

            return new ProcessResult(clips);
        }
    }
}
