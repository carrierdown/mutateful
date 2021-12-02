namespace Mutateful.Commands;

public class FilterOptions
{
    [OptionInfo(OptionType.Default, 1/512f)]
    public decimal Duration { get; set; } = 1/64m;

    public bool Invert { get; set; }
}

// # desc: Filters out notes shorter than the length specified (default 1/64). If -invert is specified, notes longer than the specified length are removed.
public static class Filter
{
    public static ProcessResult<Clip[]> Apply(Command command, params Clip[] clips)
    {
        var (success, msg) = OptionParser.TryParseOptions(command, out FilterOptions options);
        if (!success)
        {
            return new ProcessResult<Clip[]>(msg);
        }
        return Apply(options, clips);
    }

    public static ProcessResult<Clip[]> Apply(FilterOptions options, params Clip[] clips)
    {
        var processedClips = new Clip[clips.Length];

        for (var c = 0; c < clips.Length; c++)
        {
            var clip = clips[c];
            var processedClip = new Clip(clip.Length, clip.IsLooping);

            processedClip.Notes = (options.Invert) ?

                clip.Notes.Where(x => x.Duration < options.Duration).ToSortedList() :
                clip.Notes.Where(x => x.Duration > options.Duration).ToSortedList();

            processedClips[c] = processedClip;
        }

        return new ProcessResult<Clip[]>(processedClips);
    }
}