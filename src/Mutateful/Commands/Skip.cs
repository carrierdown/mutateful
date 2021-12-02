namespace Mutateful.Commands;

public class SkipOptions
{
    [OptionInfo(type: OptionType.Default, 1)]
    public int[] SkipCounts { get; set; } = { 2 };
}
    
// # desc: Creates a new clip by skipping every # note from another clip. If more than one skip value is specified, they are cycled through.
public static class Skip
{
    public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
    {
        var (success, msg) = OptionParser.TryParseOptions(command, out SkipOptions options);
        if (!success)
        {
            return new ProcessResultArray<Clip>(msg);
        }
        return Apply(options, clips);
    }

    public static ProcessResultArray<Clip> Apply(SkipOptions options, params Clip[] clips)
    {
        var resultClips = new Clip[clips.Length];

        // Normalize skip values (typical input range: 1 - N, while 0 - N is used internally)
        for (var ix = 0; ix < options.SkipCounts.Length; ix++)
        {
            options.SkipCounts[ix]--;
        }

        if (options.SkipCounts.All(x => x == 0))
        {
            return new ProcessResultArray<Clip>("The given input to skip would produce an empty clip. Aborting...");
        }
        
        var i = 0;
        foreach (var clip in clips)
        {
            var resultClip = new Clip(clips[i].Length, clips[i].IsLooping);
            decimal currentPos = 0;
            var noteIx = 0;
            var currentSkip = options.SkipCounts[0];
            var skipIx = 0;
            // We don't want skipping notes to result in shorter clips, therefore we keep going until we have filled at least
            // the same length as the original clip
            while (currentPos < resultClip.Length)
            {
                if (currentSkip > 0)
                {
                    if (noteIx >= clip.Count) noteIx %= clip.Count;
                    var note = new NoteEvent(clip.Notes[noteIx]) {Start = currentPos};
                    currentPos += clip.DurationUntilNextNote(noteIx);
                    resultClip.Add(note);
                    currentSkip--;
                }
                else
                {
                    currentSkip = options.SkipCounts[++skipIx % options.SkipCounts.Length];
                }
                noteIx++;
            }
            resultClips[i] = resultClip;
            i++;
        }
        return new ProcessResultArray<Clip>(resultClips);
    }
}