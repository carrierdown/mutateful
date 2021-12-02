namespace Mutateful.Commands.Experimental;

public class ScanOptions
{
    [OptionInfo(min: 1, max: 500)]
    public int Count { get; set; } = 8;

    public decimal Window { get; set; } = 1;
}

// rename to stretch -grainsize 1/16 -factor 2.0
public class Scan
{
    public static ProcessResult<Clip[]> Apply(Command command, params Clip[] clips)
    {
        (var success, var msg) = OptionParser.TryParseOptions(command, out ScanOptions options);
        if (!success)
        {
            return new ProcessResult<Clip[]>(msg);
        }
        var processedClips = new Clip[clips.Length];

        for (var c = 0; c < clips.Length; c++)
        {
            var clip = clips[c];
            var processedClip = new Clip(options.Window * options.Count, clip.IsLooping);
            decimal delta = clip.Length / options.Count,
                curPos = 0;

            for (int i = 0; i < options.Count; i++)
            {
                processedClip.Notes.AddRange(ClipUtilities.GetSplitNotesInRangeAtPosition(curPos, curPos + options.Window, clip.Notes, options.Window * i));
                curPos += delta;
            }
            processedClips[c] = processedClip;
        }

        return new ProcessResult<Clip[]>(processedClips);
    }
}