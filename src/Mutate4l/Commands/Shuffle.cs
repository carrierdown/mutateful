using Mutate4l.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mutate4l.Commands
{
    public class ShuffleOptions
    {
        public Clip By { get; set; }
    }

    public class Shuffle
    {
        public static ProcessResultArray<Clip> Apply(ShuffleOptions options, params Clip[] clips)
        {
            var c = 0;
            if (options.By == null || options.By.Notes.Count == 0) options.By = clips[0];
            var targetClips = new Clip[clips.Length];
            foreach (var clip in clips) // we only support one generated clip since these are tied to a specific clip slot. Maybe support multiple clips under the hood, but discard any additional clips when sending the output is the most flexible approach.
            {
                targetClips[c] = new Clip(clip.Length, clip.IsLooping);
                // scale notes to indexes
                int maxPitch = options.By.Notes.Max(x => x.Pitch);
                int minPitch = options.By.Notes.Min(x => x.Pitch);
                int range = maxPitch - minPitch;
                if (range == 0) range = 1;

                var numShuffleIndexes = options.By.Notes.Count;
                if (numShuffleIndexes < clip.Notes.Count) numShuffleIndexes = clip.Notes.Count;
                var indexes = new int[numShuffleIndexes];
                for (var i = 0; i < numShuffleIndexes; i++)
                {
                    // Calc shuffle indexes as long as there are notes in the source clip. If the clip to be shuffled contains more events than the source, add zero-indexes so that the rest of the sequence is produced sequentially.
                    if (i < options.By.Notes.Count)
                    {
                        indexes[i] = (int)Math.Floor(((options.By.Notes[i].Pitch - minPitch - 0f) / clip.Notes.Count) * clip.Notes.Count);
                    } else
                    {
                        indexes[i] = 0;
                    }
                }

                // preserve original durations until next note
                var durationUntilNextNote = new List<decimal>(clip.Notes.Count);
                for (var i = 0; i < clip.Notes.Count; i++)
                {
                    durationUntilNextNote.Add(clip.DurationUntilNextNote(i));
                }

                // do shuffle
                var j = 0;
                decimal pos = 0m;
                while (clip.Notes.Count > 0)
                {
                    int currentIx = indexes[j++] % clip.Notes.Count;
                    targetClips[c].Notes.Add(
                        new NoteEvent(clip.Notes[currentIx]) {
                            Start = pos
                        }
                    );
                    pos += durationUntilNextNote[currentIx];
                    durationUntilNextNote.RemoveAt(currentIx);
                    clip.Notes.RemoveAt(currentIx);
                }
                c++;
            }

            return new ProcessResultArray<Clip>(targetClips);
        }
    }
}
