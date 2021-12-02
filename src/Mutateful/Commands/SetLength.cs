namespace Mutateful.Commands;

public class SetLengthOptions
{
    [OptionInfo(type: OptionType.Default, 1/512f)]
    public decimal[] Lengths { get; set; } = { 4/16m };
}

// # desc: Sets the length of all notes to the specified value(s). When more values are specified, they are cycled through.
public static class SetLength
{
    public static ProcessResult<Clip[]> Apply(Command command, params Clip[] clips)
    {
        var (success, msg) = OptionParser.TryParseOptions(command, out SetLengthOptions options);
        if (!success)
        {
            return new ProcessResult<Clip[]>(msg);
        }
        return Apply(options, clips);
    }

    public static ProcessResult<Clip[]> Apply(SetLengthOptions options, params Clip[] clips)
    {
        var resultClips = ClipUtilities.CreateEmptyPlaceholderClips(clips);
        for (var index = 0; index < clips.Length; index++)
        {
            var clip = clips[index];
            var resultClip = resultClips[index];
            var lengthCounter = 0;
            foreach (var note in clip.Notes)
            {
                ClipUtilities.AddNoteCutting(resultClip, new NoteEvent(note)
                {
                    Duration = options.Lengths[lengthCounter++ % options.Lengths.Length]
                });
            }
        }
        return new ProcessResult<Clip[]>(resultClips);
    }
}