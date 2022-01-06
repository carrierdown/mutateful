namespace Mutateful.Commands;

public class SetPitchOptions
{
    [OptionInfo(OptionType.Default, 0, 127)]
    public int[] PitchValues { get; set; } = new int[0];
    
    public Clip By { get; set; } = Clip.Empty;
}

// # desc: Sets the pitch of all notes to the specified value(s). When more values are specified, they are cycled through.
public static class SetPitch
{
    public static ProcessResult<Clip[]> Apply(Command command, params Clip[] clips)
    {
        var (success, msg) = OptionParser.TryParseOptions(command, out SetPitchOptions options);
        if (!success)
        {
            return new ProcessResult<Clip[]>(msg);
        }
        return Apply(options, clips);
    }
    
    public static ProcessResult<Clip[]> Apply(SetPitchOptions options, params Clip[] clips)
    {
        var resultClips = ClipUtilities.CreateEmptyPlaceholderClips(clips);

        int[] pitches;
        if (options.PitchValues.Length > 0)
        {
            pitches = options.PitchValues;
        } else
        {
            pitches = options.By.Notes.Select(x => x.Pitch).ToArray();
        }
        if (pitches.Length == 0) return new ProcessResult<Clip[]>(clips, "SetPitch did nothing, since neither pitches or -by clip was specified.");

        for (var i = 0; i < clips.Length; i++)
        {
            var clip = clips[i];
            var resultClip = resultClips[i];
            var pitchIx = 0;
            foreach (var note in clip.Notes)
            {
                var repitchedNote = note with
                {
                    Pitch = pitches[pitchIx++ % pitches.Length]
                };
                ClipUtilities.AddNoteCutting(resultClip, repitchedNote);
            }
        }

        return new ProcessResult<Clip[]>(resultClips);
    }
}