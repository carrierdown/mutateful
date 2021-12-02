﻿namespace Mutateful.Commands;

public class SliceOptions
{
    [OptionInfo(type: OptionType.Default, 1/128f)]
    public decimal[] Lengths { get; set; } = { 4/16m };
}

// # desc: Slices a clip (i.e. cutting any notes) at a regular or irregular set of fractions.
public static class Slice
{
    public static ProcessResult<Clip[]> Apply(Command command, params Clip[] clips)
    {
        var (success, msg) = OptionParser.TryParseOptions(command, out SliceOptions options);
        if (!success)
        {
            return new ProcessResult<Clip[]>(msg);
        }
        return Apply(options, clips);
    }

    public static ProcessResult<Clip[]> Apply(SliceOptions options, params Clip[] clips)
    {
        var processedClips = new List<Clip>();
        foreach (var clip in clips)
        {
            processedClips.Add(ClipUtilities.SplitNotesAtEvery(clip, options.Lengths));
        }
        return new ProcessResult<Clip[]>(processedClips.ToArray());
    }
}