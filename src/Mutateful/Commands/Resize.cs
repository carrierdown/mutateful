﻿namespace Mutateful.Commands;

public class ResizeOptions
{
    [OptionInfo(type: OptionType.Default, 1/512f, noImplicitCast: true)]
    public decimal/*ActualDecimal*/ Factor { get; set; } = 1.0m;
}

// # desc: Resizes the current clip based on the specified factor (i.e. 0.5 halves the size of the clip, effectively doubling its tempo)
public static class Resize
{
    public static ProcessResult<Clip[]> Apply(Command command, params Clip[] clips)
    {
        var (success, msg) = OptionParser.TryParseOptions(command, out ResizeOptions options);
        return !success ? new ProcessResult<Clip[]>(msg) : Apply(options, clips);
    }

    public static ProcessResult<Clip[]> Apply(ResizeOptions options, params Clip[] clips)
    {
        foreach (var clip in clips)
        {
            foreach (var note in clip.Notes)
            {
                note.Duration *= options.Factor;
                note.Start *= options.Factor;
            }
            clip.Length *= options.Factor;
        }

        return new ProcessResult<Clip[]>(clips);
    }
}