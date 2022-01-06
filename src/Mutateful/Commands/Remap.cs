namespace Mutateful.Commands;

public class RemapOptions
{
    public Clip To { get; set; } = Clip.Empty;
}

// # desc: Remaps a set of pitches to another set of pitches
public static class Remap
{
    public static ProcessResult<Clip[]> Apply(Command command, params Clip[] clips)
    {
        var (success, msg) = OptionParser.TryParseOptions(command, out RemapOptions options);
        if (!success)
        {
            return new ProcessResult<Clip[]>(msg);
        }
        return Apply(options, clips);
    }

    public static ProcessResult<Clip[]> Apply(RemapOptions options, params Clip[] clips)
    {
        var resultClips = ClipUtilities.CreateEmptyPlaceholderClips(clips);

        for (var i = 0; i < clips.Length; i++)
        {
            var clip = clips[i];
            var resultClip = resultClips[i];
            var sourcePitches = clip.Notes.Select(x => x.Pitch).Distinct().OrderBy(x => x).ToList();
            var destPitches = options.To.Count > 0
                ? options.To.Notes.Select(x => x.Pitch).Distinct().OrderBy(x => x).ToList()
                : Enumerable.Range(36, Math.Min(sourcePitches.Count, 128 - 36)).ToList();
            var inc = 1f;

            if (destPitches.Count < sourcePitches.Count)
            {
                inc = (float) destPitches.Count / sourcePitches.Count;
            }

            var map = new Dictionary<int, int>();
            var destIx = 0f;
            foreach (var sourcePitch in sourcePitches)
            {
                map[sourcePitch] = destPitches[(int) Math.Floor(destIx)];
                destIx += inc;
            }

            foreach (var note in clip.Notes)
            {
                var remappedNote = note with { Pitch = map[note.Pitch] };
                ClipUtilities.AddNoteCutting(resultClip, remappedNote);
            }
        }
        return new ProcessResult<Clip[]>(resultClips);
    }
}