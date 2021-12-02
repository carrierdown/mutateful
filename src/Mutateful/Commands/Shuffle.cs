namespace Mutateful.Commands;

public class ShuffleOptions
{
    public Clip By { get; set; } = new Clip(4, true);

    [OptionInfo(type: OptionType.Default, 0)]
    public int[] ShuffleValues { get; set; } = new int[0];
}

// # desc: Shuffles the order of notes by a list of numbers of arbitrary length, or by another clip. When another clip is specified, the relative pitch of each note is used to determine the shuffle order.
public static class Shuffle
{
    public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
    {
        var (success, msg) = OptionParser.TryParseOptions(command, out ShuffleOptions options);
        return !success ? new ProcessResultArray<Clip>(msg) : Apply(options, clips);
    }

    public static ProcessResultArray<Clip> Apply(ShuffleOptions options, params Clip[] clips)
    {
        if (options.By.Notes.Count == 0) options.By = clips[0];
        if (options.By.Count == 0 && options.ShuffleValues.Length == 0)
        {
            return new ProcessResultArray<Clip>("No -by clip or shuffle values specified.");
        }

        ClipUtilities.Monophonize(options.By);
        var targetClips = new Clip[clips.Length];

        int[] shuffleValues;
        if (options.ShuffleValues.Length == 0)
        {
            int minPitch = options.By.Notes.Min(x => x.Pitch);
            shuffleValues = options.By.Notes.Select(x => x.Pitch - minPitch).ToArray();
        }
        else
        {
            shuffleValues = options.ShuffleValues.Select(x => Math.Clamp(x, 1, 100) - 1).ToArray();
        }

        var c = 0;
        foreach (var clip in clips) // we only support one generated clip since these are tied to a specific clip slot. Maybe support multiple clips under the hood, but discard any additional clips when sending the output is the most flexible approach.
        {
            clip.GroupSimultaneousNotes();
            targetClips[c] = new Clip(clip.Length, clip.IsLooping);
            
            var numShuffleIndexes = shuffleValues.Length;
            if (numShuffleIndexes < clip.Notes.Count) numShuffleIndexes = clip.Notes.Count;
            var indexes = new int[numShuffleIndexes];

            for (var i = 0; i < numShuffleIndexes; i++)
            {
                // Calc shuffle indexes as long as there are notes in the source clip. If the clip to be shuffled contains more events than the source, add zero-indexes so that the rest of the sequence is produced sequentially.
                if (i < shuffleValues.Length)
                {
                    indexes[i] = (int)Math.Floor(((float)shuffleValues[i] / clip.Notes.Count) * clip.Notes.Count);
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
            targetClips[c].Flatten();
            c++;
        }

        return new ProcessResultArray<Clip>(targetClips);
    }
}