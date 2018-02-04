using Mutate4l.Dto;
using Mutate4l.Options;
using Mutate4l.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mutate4l.Commands.Porcelain
{
    public class Ratchet
    {
        // Needs control sequence and target sequence(s)
        // If only one clip is specified, clip will be both control and target sequence(s)
        // Enhancement: When shaping the ratchet, velocity might be another variable used by the control sequence to control extent of shaping
        public static ProcessResult Apply(RatchetOptions options, params Clip[] clips)
        {
            if (clips.Length < 2)
            {
                clips = new Clip[] { clips[0], clips[0] };
            }

            Clip controlSequence = new Clip(clips[0]);
            Clip[] targetSequences = clips.Skip(1).Select(x => new Clip(x)).ToArray();
            ClipUtilities.NormalizeClipLengths(targetSequences.Prepend(controlSequence).ToArray());
            Clip[] resultSequences = new Clip[targetSequences.Count()];

            // find highest and lowest pitch value in control seq
            int lowestPitch = controlSequence.Notes.Select(x => x.Pitch).Min();
            int highestPitch = controlSequence.Notes.Select(x => x.Pitch).Max();
            int controlRange = Math.Max(highestPitch - lowestPitch, 1);
            int targetRange = Math.Max(Math.Abs(options.Max - options.Min), 1);
            int i = 0;
            foreach (var targetSequence in targetSequences)
            {
                // set pitch for each targetNote = (pitch - lowestPitch) / controlRange * targetRange, corresponding to actual ratchet-value for this event
                foreach (var note in targetSequence.Notes)
                {
                    note.Pitch = (note.Pitch - lowestPitch) / controlRange * targetRange;
                }
                resultSequences[i++] = DoRatchet(controlSequence, targetSequence);
            }

            return new ProcessResult(clips);
        }

        private static Clip DoRatchet(Clip controlSequence, Clip targetSequence)
        {
            Dictionary<int, bool> processedIndexes = new Dictionary<int, bool>(targetSequence.Notes.Count);
            Clip result = new Clip(targetSequence.Length, targetSequence.IsLooping);

            foreach (var controlNote in controlSequence.Notes)
            {
                targetSequence.Notes.Where(x => x.StartsInsideInterval(controlNote.Start, controlNote.End)).ToList().ForEach(note =>
                    {
                        int ratchetCount = controlNote.Pitch;
                        decimal newDuration = note.Duration / ratchetCount;
                        for (int i = 0; i < ratchetCount; i++)
                        {
                            result.Notes.Add(new NoteEvent(note.Pitch, note.Start + (i * newDuration), newDuration, note.Velocity));
                        }
                        note.Start = -1; // ensure that note is only processed once
                    }
                );
            }

            return result;
        }
    }
}
