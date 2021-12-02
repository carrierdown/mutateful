namespace Mutateful.Commands;

public class LoopOptions
{
    [OptionInfo(OptionType.Default, 1, noImplicitCast: true)]
    public decimal/*ActualDecimal*/ Length { get; set; } = 1;
}

// # desc: Lengthens the incoming clips according to the factor specified (e.g. 2 would double the clip length)
public static class Loop
{
    public static ProcessResult<Clip[]> Apply(Command command, params Clip[] clips)
    {
        var (success, msg) = OptionParser.TryParseOptions(command, out LoopOptions options);
        return success ? Apply(options, clips) : new ProcessResult<Clip[]>(msg);
    }

    public static ProcessResult<Clip[]> Apply(LoopOptions options, params Clip[] clips)
    {
        foreach (var clip in clips)
        {
            ClipUtilities.EnlargeClipByLooping(clip, options.Length * clip.Length);
        }
        return new ProcessResult<Clip[]>(clips);
    }
}