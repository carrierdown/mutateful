using Mutateful.Core;

namespace Mutateful.Commands
{
    public static class Legato
    {
        // # desc: Removes silence between notes. Basically the same as the built-in legato function in Live, but often useful in the context of a mutateful formula as well.
        public static ProcessResultArray<Clip> Apply(params Clip[] clips)
        {
            var resultClips = new Clip[clips.Length];
            const decimal smallestGap = 4m / 64m;

            for (var x = 0; x < clips.Length; x++)
            {
                var clip = clips[x];
                var resultClip = new Clip(clips[x].Length, clips[x].IsLooping);

                for (var y = 0; y < clip.Notes.Count; y++)
                {
                    var note = clip.Notes[y];
                    var duration = clip.DurationUntilNextNoteOrEndOfClip(y);
                    if (duration < smallestGap)
                    {
                        var yy = y;
                        while (++yy < clip.Count && duration < smallestGap)
                        {
                            duration = clip.DurationUntilNextNoteOrEndOfClip(yy);
                        }

                        duration = clip.DurationBetweenNotes(y, yy);
                    }
                    resultClip.Add(new NoteEvent(note.Pitch, note.Start, duration, note.Velocity));
                }
                resultClips[x] = resultClip;
            }
            return new ProcessResultArray<Clip>(resultClips);
        }
    }
}